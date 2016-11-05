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

        public ActionResult History()
        {
            List<Match> Matchs = new MySQLWrapper().GetMatches(false);
            return View(Matchs);
        }

        public ActionResult Schedule()
        {
            List<Match> Matchs = new MySQLWrapper().GetMatches(true);
            return View(Matchs);
        }

        public ActionResult Teams()
        {
            List<Team> Teams = new MySQLWrapper().GetTeam(true);
            return View(Teams);
        }
        
        public JsonResult Team(int id)
        {
            try
            {
                List<Team> Teams = new MySQLWrapper().GetTeam(false, id);

                if (Teams.Count > 0)
                {
                    List<object> bots = new List<object>();
                    foreach (Bot b in Teams[0].TeamComp)
                        bots.Add(new { name = b.Name, gun = b.Id, acc = b.Id, rTime = b.Id, kd = (b.Deaths == 0 ? b.Kills :Math.Round((double)b.Kills / b.Deaths, 2)) });
                    
                    return Json(new
                    {
                        name = Teams[0].Name,
                        wins = Teams[0].Wins,
                        games = Teams[0].Games,
                        players = new[] { bots[0], bots[1], bots[2], bots[3], bots[4] }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json("");
            }
            catch (Exception)
            {
                return Json("");
            }
        }
    }
}