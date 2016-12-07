using Gobot.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Gobot.Controllers
{
    public class WatchController : Controller
    {
        public ActionResult Index()
        {
            if (Session["User"] == null || ((User)Session["User"]).Username == "")
                return RedirectToAction("Index", "Home");

            Match m = new MySQLWrapper().GetLiveMatch((double)Session["timeOffset"]);

            if (m != null)
                return View(m);
            else
                return View();
        }

        public JsonResult IsCurrentDone()
        {
            var dateSession = (DateTime)Session["limitBetRefreshDate"];
            if (Session["User"] == null || ((User)Session["User"]).Username == "")
                return Json("", JsonRequestBehavior.DenyGet);

            var date = DateTime.Now;
            if (Session["limitBetRefreshDate"] == null)
                Session["limitBetRefreshDate"] = date;

            if (date >= (DateTime)Session["limitBetRefreshDate"])
            {
                try
                {
                    MySQLWrapper Bd = new MySQLWrapper();
                    Match currentMatch = new MySQLWrapper().GetLiveMatch((double)Session["timeOffset"]); // Get current match

                    if (currentMatch != null && currentMatch.TeamVictoire != 0) // if there is a current match and it is finished
                    {
                        DataTable bets = Bd.Procedure("GetBetsFromMatch", new MySqlParameter(":Id", currentMatch.Id)); // get all bets from this match
                        DataRow[] userBet = bets.Select(string.Format("User_Username = '{0}'", ((User)Session["User"]).Username)); // select user's bets from all bets

                        if (userBet != null && userBet.Length > 0) // if the user has placed a bet
                        {
                            Dictionary<string, object> obj = getMatchObjects(ref bets, userBet[0], currentMatch); // get information about the current match

                            Match nextMatch = getNextMatch(); // get the next match
                            if (nextMatch != null) // if there is a future match
                            {
                                object next_obj = getNextObject(nextMatch); // convert nextMatch (Match) to object

                                limitBetRefresh(nextMatch.Date.AddMinutes(5)); // prevent access to this method for 5 minutes
                                return Json(new { winner = obj["winner"], loser = obj["loser"], next = next_obj, bets = obj["bets"] }, JsonRequestBehavior.AllowGet); // return complete json
                            }
                            else // if there is no future match left
                            {
                                limitBetRefresh(date.Add(TimeSpan.FromMinutes(10)));// prevent access to this method for 10 minutes
                                return Json(new { winner = obj["winner"], loser = obj["loser"], bets = obj["bets"] }, JsonRequestBehavior.AllowGet); // return complete json
                            }
                        }
                        else // if the user has NOT placed a bet
                        {
                            Match nextMatch = getNextMatch(); // get the next match
                            if (nextMatch != null) // if there is a future match
                            {
                                object next_obj = new
                                {
                                    teams = new object[] {
                                    new { name = nextMatch.Teams[0].Name, img = nextMatch.Teams[0].ImagePath },
                                    new { name = nextMatch.Teams[1].Name, img = nextMatch.Teams[1].ImagePath }
                                },
                                    map = nextMatch.Map
                                };

                                limitBetRefresh(nextMatch.Date.AddMinutes(5)); // prevent access to this method for 5 minutes
                                return Json(new { bet = "noBet" }, JsonRequestBehavior.AllowGet); // return complete json 
                            }
                            else
                            {
                                limitBetRefresh(date.Add(TimeSpan.FromMinutes(1))); // prevent access to this method for 1 minute
                                return Json(new { bet = "noBet" }, JsonRequestBehavior.AllowGet); // return complete json
                            }
                        }
                    }
                    else // if the match is not yet finished
                    {
                        limitBetRefresh(date.Add(TimeSpan.FromMinutes(1))); // prevent access to this method for 1 minute
                        return Json("", JsonRequestBehavior.AllowGet); // return blank json
                    }
                }
                catch (Exception ex)
                {
                    limitBetRefresh(date.Add(TimeSpan.FromMinutes(1))); // prevent access to this method for 1 minute
                    return Json("", JsonRequestBehavior.AllowGet); // return blank json
                }
            }
            else
                return Json("", JsonRequestBehavior.AllowGet); // warn user that he cannot access this method yet
        }

        private void limitBetRefresh(DateTime limit)
        {
            Session["limitBetRefreshDate"] = limit;
        }

        private object getNextObject(Match nextMatch)
        {
            object next_obj = null;

            if (nextMatch != null)
            {
                next_obj = new
                {
                    teams = new object[] {
                                new { name = nextMatch.Teams[0].Name, img = nextMatch.Teams[0].ImagePath },
                                new { name = nextMatch.Teams[1].Name, img = nextMatch.Teams[1].ImagePath }
                            },
                    map = nextMatch.Map,
                };
            }

            return next_obj;
        }

        private Match getNextMatch()
        {
            MySQLWrapper bd = new MySQLWrapper();
            DataTable nextMatchBD = bd.Procedure("Nextmatch");

            Match nextMatch = null;
            if (nextMatchBD != null && nextMatchBD.Rows.Count > 0)
            {
                DataRow row = nextMatchBD.Rows[0];
                nextMatch = new Match();
                nextMatch.Id = (int)row["IdMatch"];
                nextMatch.Date = ((DateTime)row["Date"]).AddHours((double)Session["timeOffset"]);
                nextMatch.Teams[0] = bd.GetTeam(false, int.Parse(row["Team_IdTeam1"].ToString()))[0];
                nextMatch.Teams[1] = bd.GetTeam(false, int.Parse(row["Team_IdTeam2"].ToString()))[0];
                nextMatch.Team1Rounds = (int)row["RoundTeam1"];
                nextMatch.Team2Rounds = (int)row["RoundTeam2"];
                nextMatch.Map = row["Map"].ToString();
            }

            return nextMatch;
        }

        private Dictionary<string, object> getMatchObjects(ref DataTable bets, DataRow userBet, Match currentMatch)
        {
            Dictionary<string, long> totalBets = getTotalBets(ref bets, currentMatch.TeamVictoire); // get total bets on both sides

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
            object bets_obj = new { user = new { won = (winner.Id == int.Parse(userBet["Team_IdTeam"].ToString())), amount = userBet["Mise"], gain = userBet["Profit"] }, winners = totalBets["winner"], losers = totalBets["loser"] };

            return new Dictionary<string, object>() { { "winner", winner_obj }, { "loser", loser_obj }, { "bets", bets_obj } };
        }

        private static Dictionary<string, long> getTotalBets(ref DataTable bets, int winner)
        {
            decimal reduction = 0.9m;
            long loserTotal = 0, winnerTotal = 0, total = 0;

            foreach (DataRow row in bets.Rows)
            {
                if ((int)row["Team_IdTeam"] == winner)
                    winnerTotal += (long)row["Mise"];
                else
                    loserTotal += (long)row["Mise"];

                total += (long)row["Mise"];
            }

            loserTotal = (int)Math.Floor(decimal.Multiply(reduction, loserTotal));
            long admin = total - loserTotal - winnerTotal;

            return new Dictionary<string, long>() { { "winner", winnerTotal }, { "loser", loserTotal }, { "admin", admin } };
        }

        private long msUntilDate(DateTime date)
        {
            long now = DateTime.Now.Ticks;
            long after = date.Ticks;

            return (after - now) / 10000;
        }

        public JsonResult SetWatched()
        {
            if (Session["User"] == null || ((User)Session["User"]).Username == "")
                return Json("", JsonRequestBehavior.DenyGet);

            try
            {
                MySQLWrapper bd = new MySQLWrapper();

                Match currentMatch = bd.GetLiveMatch((double)Session["timeOffset"]);

                if (currentMatch != null && currentMatch.TeamVictoire == 0)
                {
                    DataTable watched = bd.Select("expliaison", "idUser = ? and idMatch = ?",
                        new List<MySqlParameter>() { new MySqlParameter(":Username", ((User)Session["User"]).Username), new MySqlParameter(":idMatch", currentMatch.Id) }, "*");

                    if (watched != null && watched.Rows.Count == 0)
                    {
                        bd.Insert("expliaison", new List<string>() { "idUser", "idMatch" },
                            new List<MySqlParameter>() { new MySqlParameter(":idUser", ((User)Session["User"]).Username), new MySqlParameter(":idMatch", currentMatch.Id) });
                        bd.Procedure("addEXP", new MySqlParameter(":Username", ((User)Session["User"]).Username), new MySqlParameter(":", 25));
                        bd.Procedure("AddWatchedUser", new MySqlParameter(":Username", ((User)Session["User"]).Username));
                    }
                }

                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
    }
}