using Gobot.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Web.Mvc;

namespace Gobot.Controllers
{
    public class WatchController : Controller
    {
        // GET: Watch
        public ActionResult Index()
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            Match m = new MySQLWrapper().GetLiveMatch();

            if (m != null)
                return View(m);
            else
                return View();
        }

        public JsonResult IsCurrentDone()
        {
            try
            {
                MySQLWrapper Bd = new MySQLWrapper();
                Match currentMatch = Bd.GetLiveMatch();

                if (currentMatch != null && currentMatch.TeamVictoire != 0)
                {
                    DataTable userBet = Bd.Select("bet", "User_Username = ? and Match_IdMatch = ?", 
                        new List<OdbcParameter>() {
                            new OdbcParameter(":Username", ((User)Session["User"]).Username),
                            new OdbcParameter(":MatchId", currentMatch.Id)
                        }, "Mise", "Team_IdTeam");

                    if (userBet != null && userBet.Rows.Count > 0)
                    {
                        DataTable nextMatchBD = Bd.Procedure("Nextmatch");
                        Match nextMatch = null;
                        if (nextMatchBD != null && nextMatchBD.Rows.Count > 0)
                        {
                            DataRow row = nextMatchBD.Rows[0];
                            nextMatch = new Match();
                            nextMatch.Id = (int)row["IdMatch"];
                            nextMatch.Date = (DateTime)row["Date"];
                            nextMatch.Teams[0] = Bd.GetTeam(false, int.Parse(row["Team_IdTeam1"].ToString()))[0];
                            nextMatch.Teams[1] = Bd.GetTeam(false, int.Parse(row["Team_IdTeam2"].ToString()))[0];
                            nextMatch.Team1Rounds = (int)row["RoundTeam1"];
                            nextMatch.Team2Rounds = (int)row["RoundTeam2"];
                            nextMatch.Map = row["Map"].ToString();
                        }

                        Team winner, loser;
                        int winnerRounds, loserRounds;
                        if (currentMatch.TeamVictoire == currentMatch.Teams[0].Id)
                        {
                            winner = currentMatch.Teams[0];
                            loser = currentMatch.Teams[1];
                            winnerRounds = currentMatch.Team1Rounds;
                            loserRounds = currentMatch.Team2Rounds;
                        }
                        else
                        {
                            winner = currentMatch.Teams[1];
                            loser = currentMatch.Teams[0];
                            winnerRounds = currentMatch.Team2Rounds;
                            loserRounds = currentMatch.Team1Rounds;
                        }

                        object winner_obj = new { name = winner.Name, img = winner.ImagePath, rounds = winnerRounds };
                        object loser_obj = new { name = loser.Name, img = loser.ImagePath, rounds = loserRounds };
                        int bet = winner.Id == (int)userBet.Rows[0]["Team_IdTeam"] ? (int)userBet.Rows[0]["Mise"] : -(int)userBet.Rows[0]["Mise"];
                       

                        if (nextMatch != null)
                        {
                            long time = msUntilDate(nextMatch.Date);
                            object next = new
                            {
                                teams = new object[] {
                                    new { name = nextMatch.Teams[0].Name, img = nextMatch.Teams[0].ImagePath },
                                    new { name = nextMatch.Teams[1].Name, img = nextMatch.Teams[1].ImagePath }
                                },
                                map = nextMatch.Map,
                                time = time
                            };

                            return Json(new { winner = winner_obj, loser = loser_obj, next = next, bet = bet }, JsonRequestBehavior.AllowGet);
                        }

                        return Json(new { winner = winner_obj, loser = loser_obj, next = new { time = 0 }, bet = bet }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        DataTable nextMatchBD = Bd.Procedure("Nextmatch");

                        if (nextMatchBD != null && nextMatchBD.Rows.Count > 0)
                            return Json(new { bet = "noBet", next = new { time = msUntilDate((DateTime)nextMatchBD.Rows[0]["Date"]) } }, JsonRequestBehavior.AllowGet);
                        else
                            return Json(new { bet = "noBet", next = new { time = 0 } }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                    return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        private long msUntilDate(DateTime date)
        {
            long now = DateTime.Now.Ticks;
            long after = date.Ticks;

            return (after - now) / 10000;
        }

        private object GetTeam(bool v1, int v2)
        {
            throw new NotImplementedException();
        }
    }
}