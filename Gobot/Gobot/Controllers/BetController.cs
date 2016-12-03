﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gobot.Models;
using System.Data;
using System.ComponentModel;
using System.Net;
using MySql.Data.MySqlClient;

namespace Gobot.Controllers
{
    public class BetController : Controller
    {
        // GET: Bet
        public ActionResult Index()
        {
            if ((User)Session["User"] == null)
                return RedirectToAction("Index", "Home");

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
        
        public ActionResult Add(int MatchId, int TeamId, int Amount)
        {
            if(Session["User"] == null)
                return RedirectToAction("Index", "Home");
            else
            {
                MySQLWrapper Bd = new MySQLWrapper();
                DataTable UserResult = Bd.Procedure("GetUser", new MySqlParameter(":username", ((User)Session["User"]).Username));
                Session["User"] = Bd.GetUserFromDB(((User)Session["User"]).Username);
                int oldAmount = betExists(MatchId, TeamId);

                if (oldAmount > 0)
                {
                    if (((User)Session["User"]).Credits + oldAmount < Amount)
                    {
                        TempData["error"] = "Vous ne possédez pas assez de crédits pour éffectuer ce pari.";
                        return RedirectToAction("Index", "Bet");
                    }
                    else
                        editBetDb(MatchId, TeamId, oldAmount, Amount);
                }
                else
                {
                    if (((User)Session["User"]).Credits < Amount)
                    {
                        TempData["error"] = "Vous ne possédez pas assez de crédits pour éffectuer ce pari.";
                        return RedirectToAction("Index", "Bet");
                    }
                    else
                        addBetDb(MatchId, TeamId, Amount);
                }

                return RedirectToAction("Index", "Bet");
            }
        }
        
        private int betExists(int MatchId, int TeamId)
        {
            List<MySqlParameter> conditions = new List<MySqlParameter>() {
                new MySqlParameter(":Username", ((User)Session["User"]).Username),
                new MySqlParameter(":TeamId", TeamId),
                new MySqlParameter(":MatchId", MatchId)
            };
            DataTable result = new MySQLWrapper().Select("bet", "User_Username = ? and Team_IdTeam = ? and Match_IdMatch = ?", conditions, "Mise");

            if (result.Rows.Count == 0)
                return 0;
            else
            {
                return (int)result.Rows[0]["Mise"];
            }
        }

        private void editBetDb(int MatchId, int TeamId, int oldAmount, int newAmount)
        {
            if (oldAmount != newAmount)
            {
                try
                {
                    MySQLWrapper Bd = new MySQLWrapper();
                    List<string> col = new List<string>() { "Mise" };
                    List<MySqlParameter> parameters = new List<MySqlParameter>() { new MySqlParameter(":Mise", newAmount) };
                    List<MySqlParameter> conditions = new List<MySqlParameter>() {
                        new MySqlParameter(":Username", ((User)Session["User"]).Username),
                        new MySqlParameter(":TeamId", TeamId),
                        new MySqlParameter(":MatchId", MatchId)
                    };
                    int result = Bd.Update("bet", col, parameters, "User_Username = ? and Team_IdTeam = ? and Match_IdMatch = ?", conditions);

                    if (result > 0)
                    {
                        List<string> columns = new List<string>() { "Credit" };
                        List<MySqlParameter> values = new List<MySqlParameter>() { new MySqlParameter(":Credit", ((User)Session["User"]).Credits - newAmount + oldAmount) };
                        List<MySqlParameter> user = new List<MySqlParameter>() { new MySqlParameter(":Username", ((User)Session["User"]).Username) };
                        int updateresult = Bd.Update("user", columns, values, "Username = ?", user);
                        if (updateresult == 1)
                        {
                            Session["User"] = Bd.GetUserFromDB(((User)Session["User"]).Username);
                        }
                        else
                        {
                            parameters = new List<MySqlParameter>() { new MySqlParameter(":Mise", oldAmount) };
                            Bd.Update("bet", col, parameters, "User_Username = ? and Team_IdTeam = ? and Match_IdMatch = ?", conditions);
                            throw new Exception("Un joueur a fait un pari mais la somme n'a pas été déduie de son compte. La nouvelle mise n'a pas été enregistrée.");
                        }
                    }
                    else
                    {
                        throw new Exception("Une erreur est survenue lors de la modification du pari. Veuillez réessayer.");
                    }
                }
                catch (Exception)
                {
                    TempData["error"] = "Une erreur s'est produite lors de la modification du pari.";
                }
            }
        }

        private void addBetDb(int MatchId, int TeamId, int Amount)
        {
            try
            {
                MySQLWrapper Bd = new MySQLWrapper();
                List<string> col = new List<string>() { "Mise", "Profit", "User_Username", "Team_IdTeam", "Match_IdMatch" };
                
                List<MySqlParameter> parameters = new List<MySqlParameter>() {
                    new MySqlParameter(":Mise", Amount),
                    new MySqlParameter(":Profit", 0.ToString()),
                    new MySqlParameter(":Username", ((User)Session["User"]).Username),
                    new MySqlParameter(":Team", TeamId),
                    new MySqlParameter(":Match", MatchId)
                };
                int result = Bd.Insert("bet", col, parameters);

                if (result > 0)
                {
                    int xp = ((User)Session["User"]).EXP + Bet.xpBet;
                    int lvl = ((User)Session["User"]).Level;
                    if (xp >= Models.User.xpLvl)
                    {
                        xp -= Models.User.xpLvl;
                        lvl += 1;
                    }
                    List<string> columns = new List<string>() { "Credit", "EXP", "LVL" };
                    List<MySqlParameter> values = new List<MySqlParameter>() { new MySqlParameter(":Credit", ((User)Session["User"]).Credits - Amount), new MySqlParameter(":EXP", xp), new MySqlParameter(":LVL", lvl) };
                    List<MySqlParameter> user = new List<MySqlParameter>() { new MySqlParameter(":Username", ((User)Session["User"]).Username) };
                    int updateresult = Bd.Update("user", columns, values, "Username = ?", user);
                    if (updateresult == 1)
                    {
                        Session["User"] = Bd.GetUserFromDB(((User)Session["User"]).Username);
                    }
                    else
                    {
                        List<MySqlParameter> conditions = new List<MySqlParameter>() {
                            new MySqlParameter(":Username", ((User)Session["User"]).Username),
                            new MySqlParameter(":TeamId", TeamId),
                            new MySqlParameter(":MatchId", MatchId)
                        };
                        Bd.Delete("bet", "User_Username = ? and Team_IdTeam = ? and Match_IdMatch = ?", conditions);
                        throw new Exception("Un joueur a fait un pari mais la somme n'a pas été déduie de son compte. La mise n'a pas été enregistrée.");
                    }
                }
                else
                {
                    throw new Exception("Une erreur est survenue lors du placement du pari. Veuillez réessayer.");
                }
            }
            catch (Exception)
            {
                TempData["error"] = "Une erreur s'est produite lors du placement du pari.";
            }
        }

        public ActionResult Remove(int tId, int mId)
        {
            try
            {
                if ((User)Session["User"] == null)
                    return RedirectToAction("Index", "Home");
                else
                {
                    MySQLWrapper Bd = new MySQLWrapper();
                    Session["User"] = Bd.GetUserFromDB(((User)Session["User"]).Username);

                    List<MySqlParameter> Bet = new List<MySqlParameter>() {
                        new MySqlParameter(":TeamId", tId),
                        new MySqlParameter(":MatchId", mId),
                        new MySqlParameter(":Username", ((User)Session["User"]).Username)
                    };

                    DataTable resultBet = Bd.Select("bet", "Team_IdTeam = ? and Match_IdMatch = ? and User_Username = ?", Bet, "Mise", "User_Username", "Team_IdTeam", "Match_IdMatch", "Profit");
                    if (resultBet.Rows.Count > 0)
                    {
                        int Amount = (int)resultBet.Rows[0]["Mise"];
                        if ((int)resultBet.Rows[0]["Profit"] != 0)
                            throw new Exception("Une erreur est survenue lors de la supression du pari.");

                        Bet = new List<MySqlParameter>() {
                            new MySqlParameter(":TeamId", tId),
                            new MySqlParameter(":MatchId", mId),
                            new MySqlParameter(":Username", ((User)Session["User"]).Username)
                        };

                        int result = Bd.Delete("bet", "Team_IdTeam = ? and Match_IdMatch = ? and User_Username = ?", Bet);
                        if (result > 0)
                        {
                            int xp = ((User)Session["User"]).EXP - Models.Bet.xpBet;
                            int lvl = ((User)Session["User"]).Level;
                            if (xp < 0)
                            {
                                xp += Models.User.xpLvl;
                                lvl -= 1;
                            }
                            List<string> columns = new List<string>() { "Credit", "EXP", "LVL" };
                            List<MySqlParameter> values = new List<MySqlParameter>() { new MySqlParameter(":Credit", ((User)Session["User"]).Credits + Amount), new MySqlParameter(":EXP", xp), new MySqlParameter(":LVL", lvl) };
                            List<MySqlParameter> user = new List<MySqlParameter>() { new MySqlParameter(":Username", ((User)Session["User"]).Username) };
                            int updateresult = Bd.Update("user", columns, values, "Username = ?", user);
                            if (updateresult == 1)
                            {
                                Session["User"] = Bd.GetUserFromDB(((User)Session["User"]).Username);
                            }
                            else
                            {
                                List<string> col = new List<string>() { "Mise", "Profit", "User_Username", "Team_IdTeam", "Match_IdMatch" };

                                List<MySqlParameter> parameters = new List<MySqlParameter>() {
                                    new MySqlParameter(":Mise", (int)resultBet.Rows[0]["Mise"]),
                                    new MySqlParameter(":Profit", 0),
                                    new MySqlParameter(":Username", ((User)Session["User"]).Username),
                                    new MySqlParameter(":Team", (int)resultBet.Rows[0]["Team_IdTeam"]),
                                    new MySqlParameter(":Match", (int)resultBet.Rows[0]["Match_IdMatch"])
                                };
                                Bd.Insert("bet", col, parameters);
                                throw new Exception("Une erreur est survenue lors de la supression du pari.");
                            }
                        }
                        else
                        {
                            throw new Exception("Une erreur est survenue lors de la supression du pari.");
                        }
                    }
                    else
                    {
                        throw new Exception("Une erreur est survenue lors de la supression du pari.");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return RedirectToAction("Index", "Bet");
        }

        public JsonResult GetMatchResultUrl()
        {
            try
            {
                string url = Url.Action("MatchResult", "Home");
                return Json(new { url = url }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json("", JsonRequestBehavior.DenyGet);
            }
        }

        public ActionResult Ad()
        {
            if ((User)Session["User"] == null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        public JsonResult GetBetUsers(int TeamId, int MatchId)
        {
            if ((User)Session["User"] == null)
                return Json("", JsonRequestBehavior.DenyGet);

            DataTable users = new MySQLWrapper().Procedure("GetUserFromTeamBet", new MySqlParameter(":IdTeam", TeamId), new MySqlParameter(":IdMatch", MatchId));
            
            if (users != null && users.Rows.Count > 0)
            {
                List<object> usersString = new List<object>();
                foreach (DataRow row in users.Rows)
                    usersString.Add(new { username = row["User_Username"], img = row["Image"] });

                object users_obj = new { users = usersString.ToArray() };
                return Json(users_obj, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new { users = new string[] { } }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBetUser(string Username)
        {
            if ((User)Session["User"] == null)
                return RedirectToAction("Index", "Bet");

            string username = Request.Form["Username"];
            return RedirectToAction("Index", "Account", new { Username = username });
        }

        public ActionResult GetNextDay(int lastMatchId)
        {
            if ((User)Session["User"] == null)
                return Json("", JsonRequestBehavior.DenyGet);

            try
            {
                MySQLWrapper Bd = new MySQLWrapper();
                List<Match> Matches = Bd.GetMatches(true, (double)Session["timeOffset"], lastMatchId, 1);

                if (Matches.Count() > 0)
                {
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

                    List<object> matches_obj = new List<object>();
                    foreach (Match m in Matches)
                    {
                        object[] teamBets = new object[2];

                        if (m.CurrentUserBet)
                        {
                            if (m.TeamNumberBet == 1)
                            {
                                teamBets[0] = new { total = m.Team1TotalBet, user = m.CurrentUserAmount };
                                teamBets[1] = new { total = m.Team2TotalBet };
                            }
                            else
                            {
                                teamBets[0] = new { total = m.Team1TotalBet };
                                teamBets[1] = new { total = m.Team2TotalBet, user = m.CurrentUserAmount };
                            }
                        }
                        else
                        {
                            teamBets[0] = new { total = m.Team1TotalBet };
                            teamBets[1] = new { total = m.Team2TotalBet };
                        }

                        object[] teams = {
                            new { id = m.Teams[0].Id, num = 1, name = m.Teams[0].Name, img = m.Teams[0].ImagePath, bet = teamBets[0] },
                            new { id = m.Teams[1].Id, num = 2, name = m.Teams[1].Name, img = m.Teams[1].ImagePath, bet = teamBets[1] }
                        };

                        matches_obj.Add(new { date = (m.Date.Hour + ":" + m.Date.Minute.ToString("00")), id = m.Id, teams = teams });
                    }

                    string date_day = Matches[0].Date.DayOfWeek.ToString();
                    string date_complete = Matches[0].Date.ToString("dd-MM-yyyy");
                    object json = new { date_day = date_day, date_complete = date_complete, matches = matches_obj };
                    return Json(json, JsonRequestBehavior.AllowGet);
                }
                else
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Il n'y a plus de parties à venir");
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Une erreur s'est produite lors de la récupération des parties.");
            }
        }

        public JsonResult WatchedAd()
        {
            if ((User)Session["User"] == null)
                return Json("", JsonRequestBehavior.DenyGet);
            else
            {
                try
                {
                    MySQLWrapper bd = new MySQLWrapper();
                    User user = bd.GetUserFromDB(((User)Session["User"]).Username);
                    DataTable update = new MySQLWrapper().Procedure("AddFunds", new MySqlParameter(":Username", user.Username), new MySqlParameter(":Credits", user.Credits + 50));
                    TempData["success"] = "Votre compte a ete credite 50 credits.";
                }
                catch (Exception)
                {
                    TempData["error"] = "Une erreur est survenue lors de l'attribution de vos credits.";
                }
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
    }
}