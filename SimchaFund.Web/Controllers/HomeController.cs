using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimchaFund.Data;
using SimchaFund.Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SimchaFund.Web.Controllers
{
    public class SimchosController : Controller
    {
        private string _connectionString = @"Data Source=.\sqlexpress;Initial Catalog=simchosFund;Integrated Security=true;";
        public IActionResult Index()
        {
            SimchaFundDb db = new SimchaFundDb(_connectionString);
            SimchosViewModel vm = new SimchosViewModel
            {
                Simchos = db.GetSimchos(),
                TotalContributors = db.GetContributorTotal()
            };
            if (!string.IsNullOrEmpty((string)TempData["message"]))
            {
                ViewBag.Message = (string)TempData["message"];
            }
            return View(vm);
        }
        [HttpPost]
        public IActionResult New(Simcha simcha)
        {
            SimchaFundDb db = new SimchaFundDb(_connectionString);
            db.AddSimcha(simcha);
            TempData["message"] = "Simcha added successfully";
            return Redirect("/simchos/index");
        }

        public IActionResult Contributions(int simchaId)
        {
            SimchaFundDb db = new SimchaFundDb(_connectionString);
            ContributionsViewModel vm = new ContributionsViewModel
            {
                Contributors = db.GetContributors(),
                Simcha = db.GetSimcha(simchaId),
                Counter = 0,
                contribForSimcha = db.GetIdsForContributed(simchaId)
            };
            return View(vm);
        }
        public IActionResult UpdateContributions(List<History> contributors, int simchaId)
        {
            SimchaFundDb db = new SimchaFundDb(_connectionString);
            List<History> contributors1 = contributors.Where(c => c.Include).ToList();
            db.UpdateContributions(contributors1, simchaId);
            return Redirect("/simchos");
        }
    }
}
