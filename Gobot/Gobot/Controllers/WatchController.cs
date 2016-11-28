using Gobot.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Threading.Tasks;
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
            Match m = new MySQLWrapper().GetLiveMatch((double)Session["timeOffset"]);

            if (m != null)
                return View(m);
            else
                return View();
        }

        public JsonResult IsCurrentDone()
        {
            if (Session["User"] == null)
                return Json("", JsonRequestBehavior.AllowGet);

            if (Session["limitBetRefreshDate"] == null)
                Session["limitBetRefreshDate"] = DateTime.Now;

            var date = DateTime.Now;
            var limit = (DateTime)Session["limitBetRefreshDate"];

            if (DateTime.Now > (DateTime)Session["limitBetRefreshDate"])
            {
                try
                {
                    MySQLWrapper Bd = new MySQLWrapper();
                    Match currentMatch = Bd.GetLiveMatch((double)Session["timeOffset"]);

                    if (currentMatch != null && currentMatch.TeamVictoire != 0)
                    {
                        DataTable bets = Bd.Procedure("GetBetsFromMatch", new OdbcParameter(":Id", currentMatch.Id));
                        DataRow[] userBet = bets.Select(string.Format("User_Username = '{0}'", ((User)Session["User"]).Username));

                        if (userBet != null && userBet.Length > 0)
                        {
                            Dictionary<string, int> totalBets = getTotalBets(ref bets, currentMatch.TeamVictoire);

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
                            object bets_obj = new { user = new { won = (winner.Id == (int)userBet[0]["Team_IdTeam"]), amount = (int)userBet[0]["Mise"], gain = (int)userBet[0]["Profit"] }, winners = totalBets["winner"], losers = totalBets["loser"] };

                            if (nextMatch != null)
                            {
                                long time = msUntilDate(nextMatch.Date);
                                object next_obj = new
                                {
                                    teams = new object[] {
                                    new { name = nextMatch.Teams[0].Name, img = nextMatch.Teams[0].ImagePath },
                                    new { name = nextMatch.Teams[1].Name, img = nextMatch.Teams[1].ImagePath }
                                },
                                    map = nextMatch.Map,
                                    time = time
                                };

                                Session["limitBetRefreshDate"] = DateTime.Now.Add(TimeSpan.FromMilliseconds(time));
                                return Json(new { winner = winner_obj, loser = loser_obj, next = next_obj, bet = bets_obj }, JsonRequestBehavior.AllowGet);
                            }

                            Session["limitBetRefreshDate"] = DateTime.Now.Add(TimeSpan.FromMinutes(10));
                            return Json(new { winner = winner_obj, loser = loser_obj, next = new { time = 0 }, bet = bets_obj }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            DataTable nextMatchBD = Bd.Procedure("Nextmatch");

                            if (nextMatchBD != null && nextMatchBD.Rows.Count > 0)
                            {
                                long time = msUntilDate((DateTime)nextMatchBD.Rows[0]["Date"]);
                                Session["limitBetRefreshDate"] = DateTime.Now.Add(TimeSpan.FromMilliseconds(time));
                                return Json(new { bet = "noBet", next = new { time = time } }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                Session["limitBetRefreshDate"] = DateTime.Now.Add(TimeSpan.FromMinutes(1));
                                return Json(new { bet = "noBet", next = new { time = 0 } }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    else
                    {
                        Session["limitBetRefreshDate"] = DateTime.Now.Add(TimeSpan.FromMinutes(1));
                        return Json("", JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception)
                {
                    Session["limitBetRefreshDate"] = DateTime.Now.Add(TimeSpan.FromMinutes(1));
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            else
                return Json("", JsonRequestBehavior.AllowGet);
        }

        private Dictionary<string, int> getTotalBets(ref DataTable bets, int winner)
        {
            decimal reduction = 0.9m;
            int loserTotal = 0, winnerTotal = 0;

            foreach (DataRow row in bets.Rows)
            {
                if ((int)row["Team_IdTeam"] == winner)
                    winnerTotal += (int)row["Mise"];
                else
                    loserTotal += (int)row["Mise"];
            }

            loserTotal = (int)Math.Floor(Decimal.Multiply(reduction, loserTotal));
            winnerTotal = (int)Math.Floor(Decimal.Multiply(reduction, winnerTotal));
            return new Dictionary<string, int>() { { "winner", winnerTotal }, { "loser", loserTotal } };
        }

        private long msUntilDate(DateTime date)
        {
            long now = DateTime.Now.Ticks;
            long after = date.Ticks;

            return (after - now) / 10000;
        }
    }
}