using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SimchaFund.Data
{
    public class SimchaFundDb
    {
        private readonly string _connectionString;
        public SimchaFundDb(string connectionString)
        {
            _connectionString = connectionString;
        }
        public List<Simcha> GetSimchos()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                List<Simcha> simchas = new List<Simcha>();
                cmd.CommandText = @"select s.*, COUNT(c.ContributorId) as contributorCount, sum(c.Amount) as total from Simchos s
                                    left join Contributions c
                                    on c.SimchaId = s.Id
                                    group by s.Date, s.Name, s.Id
                                    order by s.Date desc";
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Simcha simcha = new Simcha
                    {
                        Id = (int)reader["Id"],
                        Name = (string)reader["name"],
                        Date = (DateTime)reader["date"],
                        ContributorCount = (int)reader["contributorCount"]
                    };
                    if (reader["total"] != null)
                    {
                        simcha.Total = reader.GetOrNull<decimal>("total");
                    }
                    else
                    {
                        simcha.Total = 0;
                    }
                    simchas.Add(simcha);
                }
                return simchas;
            }
        }
        public int GetContributorTotal()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = "select COUNT(*) from Contributors";
                connection.Open();
                return (int)cmd.ExecuteScalar();
            }
        }
        public void AddSimcha(Simcha simcha)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = "insert into Simchos values(@name, @date)";
                cmd.Parameters.AddWithValue("@name", simcha.Name);
                cmd.Parameters.AddWithValue("@date", simcha.Date);
                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public List<Contributor> GetContributors()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"select c.*, sum(d.Amount) as deposited from Contributors c
                                    join Deposits d
                                    on d.ContributorId = c.Id
                                    group by c.FirstName, c.LastName, c.AlwaysInclude, c.Cell, c.Id
                                    select cr.id, sum(cn.Amount) as contributed from Contributors cr
                                    left join Contributions cn 
                                    on cn.ContributorId = cr.Id
                                    group by cr.id";
                List<Contributor> contributors = new List<Contributor>();
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    contributors.Add(new Contributor
                    {
                        FirstName = (string)reader["firstName"],
                        LastName = (string)reader["lastName"],
                        Cell = (string)reader["cell"],
                        Id = (int)reader["id"],
                        AlwaysInclude = (bool)reader["alwaysInclude"],
                        Balance = ((decimal)reader["deposited"]) - GetContributedForContributor((int)reader["id"])
                    });
                }
                return contributors;
            }
        }
        public decimal GetContributedForContributor(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"select sum(cn.Amount) as amount from Contributors cr
                                    left join Contributions cn
                                    on cn.ContributorId = cr.Id
                                    where cr.id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                var x = reader["amount"];
                decimal contributed = 0;
                if (x != null)
                {
                    contributed = reader.GetOrNull<decimal>("amount");
                }
                return contributed;
            }
        }
        public void NewContributor(Contributor contributor, decimal amount)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = "insert into contributors values (@firstName, @lastName, @cell, @alwaysInclude) select scope_identity()";
                cmd.Parameters.AddWithValue("@firstName", contributor.FirstName);
                cmd.Parameters.AddWithValue("@lastName", contributor.LastName);
                cmd.Parameters.AddWithValue("@cell", contributor.Cell);
                cmd.Parameters.AddWithValue("@alwaysInclude", contributor.AlwaysInclude);
                connection.Open();
                int id = (int)(decimal)cmd.ExecuteScalar();
                cmd.CommandText = "insert into deposits values (@id, @amount, getdate())";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.ExecuteNonQuery();
            }
        }
        public void NewDeposit(decimal amount, DateTime date, int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = "insert into deposits (contributorId, amount, date) values (@id, @amount, @date)";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@date", date);
                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public List<History> GetHistory(int id)
        {
            List<History> histories = new List<History>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"select * from Contributions c
                                    join simchos s 
                                    on c.SimchaId = s.Id
                                    where c.ContributorId = @id";
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    histories.Add(new History
                    {
                        Date = (DateTime)reader["date"],
                        Amount = -(decimal)reader["amount"],
                        Action = $"Contribution for the {(string)reader["name"]} simcha"
                    });
                }
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = "select * from deposits where ContributorId = @id";
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    histories.Add(new History
                    {
                        Date = (DateTime)reader["date"],
                        Amount = (decimal)reader["amount"],
                        Action = "Deposit"
                    });
                }

                return histories;
            }

            //public List<Contributor> GetContributorsForSimchos(int simchaId)
            //{
            //    using (SqlConnection connection = new SqlConnection(_connectionString))
            //    using (SqlCommand cmd = connection.CreateCommand())
            //    {
            //        cmd.CommandText = "";
            //    }
            //}
        }
        public string Name(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = "select firstName, lastName from contributors where id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                return $"{(string)reader["firstName"]} {(string)reader["lastName"]}";
            }
        }
        public Simcha GetSimcha(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = "select * from simchos where id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                return new Simcha
                {
                    Name = (string)reader["name"],
                    Id = (int)reader["id"]
                };
            }
        }
        public void UpdateContributions(List<History> contributors, int simchaId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"delete from contributions where simchaId = @id";
                cmd.Parameters.AddWithValue("@id", simchaId);
                connection.Open();
                cmd.ExecuteNonQuery();
            }
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                connection.Open();
                foreach (History contribution in contributors)
                {
                    if (contribution.Amount != 0)
                    {
                        cmd.CommandText = @"insert into contributions values (@simchaId, @contributorId, @amount)";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@simchaId", simchaId);
                        cmd.Parameters.AddWithValue("@contributorId", contribution.ContributorId);
                        cmd.Parameters.AddWithValue("@amount", contribution.Amount);
                        cmd.ExecuteNonQuery();
                    }

                }
            }
        }

        public List<ContribForSimcha> GetIdsForContributed(int simchaId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = "select contributorid, Amount from Contributions where SimchaId = @simchaId";
                cmd.Parameters.AddWithValue("@simchaid", simchaId);
                List<ContribForSimcha> contribForSimchas = new List<ContribForSimcha>();
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    contribForSimchas.Add(new ContribForSimcha
                    {
                        Amount = (decimal)reader["amount"],
                        Id = (int)reader["contributorId"]
                    });
                }
                return contribForSimchas;
            }
        }
        //public List<Contributor> GetContributorsForSimcha(int simchaid)
        //{
        //    using (SqlConnection connection = new SqlConnection(_connectionString))
        //    using (SqlCommand cmd = connection.CreateCommand())
        //    {
        //        cmd.CommandText = @"select c.*, sum(d.Amount) as deposited from Contributors c
        //                            join Deposits d
        //                            on d.ContributorId = c.Id
        //                            group by c.FirstName, c.LastName, c.AlwaysInclude, c.Cell, c.Id";
        //        List<Contributor> contributors = new List<Contributor>();
        //        connection.Open();
        //        SqlDataReader reader = cmd.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            contributors.Add(new Contributor
        //            {
        //                FirstName = (string)reader["firstName"],
        //                LastName = (string)reader["lastName"],
        //                Cell = (string)reader["cell"],
        //                Id = (int)reader["id"],
        //                AlwaysInclude = (bool)reader["alwaysInclude"],
        //                Balance = ((decimal)reader["deposited"]) - GetContributedForContributor((int)reader["id"])
        //            });
        //        }
        //        return contributors;
        //    }
        //}
    }
    public class Simcha
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int ContributorCount { get; set; }
        public decimal Total { get; set; }
    }
    public class Contributor
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Cell { get; set; }
        public bool AlwaysInclude { get; set; }
        public decimal Balance { get; set; }
    }
    public class History
    {
        public string Action { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int ContributorId { get; set; }
        public bool Include { get; set; }
    }
    public class Contribution
    {
        public int ContributorId { get; set; }
        public bool Include { get; set; }
        public decimal Amount { get; set; }
        public int SimchaId { get; set; }

    }
    public class ContribForSimcha
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
    }
}

public static class Extensions
{
    public static T GetOrNull<T>(this SqlDataReader reader, string column)
    {
        object value = reader[column];
        if (value == DBNull.Value)
        {
            return default(T);
        }
        return (T)value;
    }
}

