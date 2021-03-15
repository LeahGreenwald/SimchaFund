using SimchaFund.Data;
using System;
using System.Collections.Generic;

namespace SimchaFund.Web.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
    public class SimchosViewModel
    {
        public List<Simcha> Simchos { get; set; }
        public int TotalContributors { get; set; }
    }
    public class ContributorsViewModel
    {
        public List<Contributor> Contributors { get; set; }
    }
    public class HistoryViewModel
    {
        public List<History> Histories { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }
    }
    public class ContributionsViewModel
    {
        public List<Contributor> Contributors { get; set; }
        public List<ContribForSimcha> contribForSimcha { get; set; }
        public Simcha Simcha { get; set; }
        public int Counter { get; set; }
    }
}
