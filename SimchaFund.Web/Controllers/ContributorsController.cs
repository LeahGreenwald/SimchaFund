using Microsoft.AspNetCore.Mvc;
using SimchaFund.Data;
using SimchaFund.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimchaFund.Web.Controllers
{
    public class ContributorsController : Controller
    {
        private string _connectionString = @"Data Source=.\sqlexpress;Initial Catalog=simchosFund;Integrated Security=true;";
        public IActionResult Index()
        {
            SimchaFundDb db = new SimchaFundDb(_connectionString);
            ContributorsViewModel vm = new ContributorsViewModel
            {
                Contributors = db.GetContributors()
            };
            if (!string.IsNullOrEmpty((string)TempData["message"]))
            {
                ViewBag.Message = (string)TempData["message"];
            }
            return View(vm);
        }
        public IActionResult New(Contributor contributor, decimal initialDeposit)
        {
            SimchaFundDb db = new SimchaFundDb(_connectionString);
            db.NewContributor(contributor, initialDeposit);
            TempData["message"] = "Contributor added successfully";
            return Redirect("/contributors/index");
        }
        public IActionResult Deposit(decimal amount, DateTime date, int contributorId)
        {
            SimchaFundDb db = new SimchaFundDb(_connectionString);
            db.NewDeposit(amount, date, contributorId);
            return Redirect("/contributors/index");
        }
        public IActionResult ShowHistory (int id)
        {
            SimchaFundDb db = new SimchaFundDb(_connectionString);
            List<History> histories = db.GetHistory(id);
            decimal balance = 0;
            histories.ForEach(h => balance += h.Amount);
            List<History> histories1 = histories.OrderByDescending(h => h.Date).ToList();
            HistoryViewModel vm = new HistoryViewModel { Histories = histories1, Name = db.Name(id), Balance = balance};
            return View(vm);
        }
    }
}
