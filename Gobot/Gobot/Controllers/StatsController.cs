using Gobot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gobot.Controllers
{
    public class StatsController : Controller
    {
        // GET: Stats
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Bet");
        }

        public ActionResult Match(string MatchId)
        {
            //Get Match info
            return View();
        }

        public ActionResult Teams()
        {
            //Get team info
            return View(new List<Team>());
        }

        public JsonResult Team(string TeamId, string json)
        {
            //Get team info
            return Json("");
        }

        public ActionResult Bot(string BotId)
        {
            //Get bot info
            return View();
        }
    }
}