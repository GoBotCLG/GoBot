using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gobot.Controllers
{
    public class BetController : Controller
    {
        // GET: Bet
        public ActionResult Index()
        {
            //Get schedule info for future matches
            return View();
        }

        [HttpPost]
        public ActionResult Add(int MatchId, int TeamId, int Amount)
        {
            //Add bet to DB
            return RedirectToAction("Index", "Bet");
        }

        [HttpPost]
        public ActionResult Remove(int BetId)
        {
            //Remove bet from DB
            return RedirectToAction("Index", "Bet");
        }
    }
}