using Gobot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gobot.Controllers
{
    //Dank AF
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
            MySQLWrapper Bd = new MySQLWrapper();
            List<Match> Matchs = Bd.GetMatches(false);
            return View(Matchs);
        }

        public ActionResult Teams()
        {
            //Get team info
            MySQLWrapper Bd = new MySQLWrapper();
            List<Team> Teams = Bd.GetAllTeam();
            return View(Teams);
        }
        
        public JsonResult Team(string id)
        {
            //return Json("");
            //Get team info            
            return Json(new {name="Cloud6", wins=10, games=15, players=new[] { new { name = "Adam", gun = "ak-47", acc = 1.25, rTime = 1, kd = 2 }, new { name = "Adam", gun = "ak-47", acc = 1.25, rTime = 1, kd = 2 }, new { name = "Adam", gun = "ak-47", acc = 1.25, rTime = 1, kd = 2 }, new { name = "Adam", gun = "ak-47", acc = 1.25, rTime = 1, kd = 2 }, new { name = "Adam", gun = "ak-47", acc = 1.25, rTime = 1, kd = 2 } } }, JsonRequestBehavior.AllowGet);
        }
    }
}