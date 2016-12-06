using Gobot.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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
            if ((User)Session["User"] == null)
                return RedirectToAction("Index", "Home");

            try
            {
                MySQLWrapper Bd = new MySQLWrapper();
                List<Match> FutureMatches = Bd.GetMatches(false, (double)Session["timeOffset"]);
                List<Match> Matches = new List<Match>();

                if (FutureMatches.Count() > 0)
                {
                    int firstDate = FutureMatches[0].Date.DayOfYear;
                    int secondDate = FutureMatches[0].Date.AddDays(-1).DayOfYear;
                    foreach (Match m in FutureMatches)
                    {
                        if (m.Date.DayOfYear == firstDate || m.Date.DayOfYear == secondDate)
                            Matches.Add(m);
                        else
                        {
                            Matches.Add(m);
                            break;
                        }
                    }
                }

                List<Bet> Bets = new List<Bet>();

                DataTable BetResult = Bd.Procedure("GetBetUser", new MySqlParameter(":Username", ((User)Session["User"]).Username));

                foreach (DataRow row in BetResult.Rows)
                {
                    Bets.Add(new Bet((int)row["IdBet"], (int)row["Mise"], (int)row["Profit"], ((User)Session["User"]).Username, (int)row["Team_IdTeam"], (int)row["Match_IdMatch"]));
                }

                foreach (Bet bet in Bets)
                {
                    foreach (Match match in Matches)
                    {
                        if (bet.MatchId == match.Id)
                        {
                            match.CurrentUserBet = true;
                            match.CurrentUserAmount = bet.Amount;
                            if (bet.TeamId == match.Teams[0].Id)
                            {
                                match.TeamNumberBet = 1;
                            }
                            else
                            {
                                match.TeamNumberBet = 2;
                            }
                        }
                    }
                }

                Bets.Clear();

                BetResult = Bd.Select("bet", "", new List<MySqlParameter>(), "*");

                foreach (DataRow row in BetResult.Rows)
                {
                    foreach (Match match in Matches)
                    {
                        if ((int)row["Match_IdMatch"] == match.Id)
                        {
                            if ((int)row["Team_IdTeam"] == match.Teams[0].Id)
                            {
                                match.Team1TotalBet += (int)row["Mise"];
                            }
                            else
                            {
                                match.Team2TotalBet += (int)row["Mise"];
                            }
                        }
                    }
                }

                return View(Matches);
            }
            catch (Exception)
            {
                return View(new List<Match>());
            }
        }

        public ActionResult Schedule()
        {
            if ((User)Session["User"] == null)
                return RedirectToAction("Index", "Home");

            try
            {
                MySQLWrapper Bd = new MySQLWrapper();
                List<Match> FutureMatches = Bd.GetMatches(true, (double)Session["timeOffset"]);
                List<Match> Matches = new List<Match>();

                if (FutureMatches.Count() > 0)
                {
                    int firstDate = FutureMatches[0].Date.DayOfYear;
                    int secondDate = FutureMatches[0].Date.AddDays(1).DayOfYear;
                    foreach (Match m in FutureMatches)
                    {
                        if (m.Date.DayOfYear == firstDate || m.Date.DayOfYear == secondDate)
                            Matches.Add(m);
                        else
                        {
                            Matches.Add(m);
                            break;
                        }
                    }
                }

                List<Bet> Bets = new List<Bet>();

                DataTable BetResult = Bd.Procedure("GetBetUser", new MySqlParameter(":Username", ((User)Session["User"]).Username));

                foreach (DataRow row in BetResult.Rows)
                {
                    Bets.Add(new Bet((int)row["IdBet"], (int)row["Mise"], (int)row["Profit"], ((User)Session["User"]).Username, (int)row["Team_IdTeam"], (int)row["Match_IdMatch"]));
                }

                foreach (Bet bet in Bets)
                {
                    foreach (Match match in Matches)
                    {
                        if (bet.MatchId == match.Id)
                        {
                            match.CurrentUserBet = true;
                            match.CurrentUserAmount = bet.Amount;
                            if (bet.TeamId == match.Teams[0].Id)
                            {
                                match.TeamNumberBet = 1;
                            }
                            else
                            {
                                match.TeamNumberBet = 2;
                            }
                        }
                    }
                }

                Bets.Clear();

                BetResult = Bd.Select("bet", "", new List<MySqlParameter>(), "*");

                foreach (DataRow row in BetResult.Rows)
                {
                    foreach (Match match in Matches)
                    {
                        if ((int)row["Match_IdMatch"] == match.Id)
                        {
                            if ((int)row["Team_IdTeam"] == match.Teams[0].Id)
                            {
                                match.Team1TotalBet += (int)row["Mise"];
                            }
                            else
                            {
                                match.Team2TotalBet += (int)row["Mise"];
                            }
                        }
                    }
                }

                return View(Matches);
            }
            catch (Exception)
            {
                return View(new List<Match>());
            }
        }

        public ActionResult Teams()
        {
            if ((User)Session["User"] == null)
                return RedirectToAction("Index", "Home");

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
                        bots.Add(new { name = b.Name, gun = b.Gun, gunComplet = b.GunComplet, k = b.Kills, d = b.Deaths, a = b.Assists, kd = (b.Deaths == 0 ? b.Kills :Math.Round((double)b.Kills / b.Deaths, 2)) });
                    
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