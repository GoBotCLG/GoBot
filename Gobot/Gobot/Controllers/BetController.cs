using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gobot.Models;
using System.Data;
using System.Data.Odbc;
using System.ComponentModel;
using System.Net;

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
            List<Match> FutureMatches = Bd.GetMatches(true);
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

            DataTable BetResult = Bd.Procedure("GetBetUser", new OdbcParameter(":Username", ((User)Session["User"]).Username));

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

            BetResult = Bd.Select("bet", "", new List<OdbcParameter>(), "*");

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
                DataTable UserResult = Bd.Procedure("GetUser", new OdbcParameter(":username", ((User)Session["User"]).Username));
                Session["User"] = Bd.GetUserFromDB(((User)Session["User"]).Username);
                int oldAmount = betExists(MatchId, TeamId);

                if (((User)Session["User"]).Credits < Amount)
                {
                    ViewBag.Error = "Vous ne possédez pas assez de crédits pour éffectuer ce pari.";
                    return Json(0);
                }
                else if (oldAmount > 0)
                    editBetDb(MatchId, TeamId, oldAmount, Amount);
                else
                    addBetDb(MatchId, TeamId, Amount);

                return RedirectToAction("Index", "Bet");
            }
        }
        
        private int betExists(int MatchId, int TeamId)
        {
            List<OdbcParameter> conditions = new List<OdbcParameter>() {
                new OdbcParameter(":Username", ((User)Session["User"]).Username),
                new OdbcParameter(":TeamId", TeamId),
                new OdbcParameter(":MatchId", MatchId)
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
                    List<OdbcParameter> parameters = new List<OdbcParameter>() { new OdbcParameter(":Mise", newAmount) };
                    List<OdbcParameter> conditions = new List<OdbcParameter>() {
                        new OdbcParameter(":Username", ((User)Session["User"]).Username),
                        new OdbcParameter(":TeamId", TeamId),
                        new OdbcParameter(":MatchId", MatchId)
                    };
                    int result = Bd.Update("bet", col, parameters, "User_Username = ? and Team_IdTeam = ? and Match_IdMatch = ?", conditions);

                    if (result > 0)
                    {
                        List<string> columns = new List<string>() { "Credit" };
                        List<OdbcParameter> values = new List<OdbcParameter>() { new OdbcParameter(":Credit", ((User)Session["User"]).Credits - newAmount + oldAmount) };
                        List<OdbcParameter> user = new List<OdbcParameter>() { new OdbcParameter(":Username", ((User)Session["User"]).Username) };
                        int updateresult = Bd.Update("user", columns, values, "Username = ?", user);
                        if (updateresult == 1)
                        {
                            Session["User"] = Bd.GetUserFromDB(((User)Session["User"]).Username);
                        }
                        else
                        {
                            parameters = new List<OdbcParameter>() { new OdbcParameter(":Mise", oldAmount) };
                            Bd.Update("bet", col, parameters, "User_Username = ? and Team_IdTeam = ? and Match_IdMatch = ?", conditions);
                            throw new Exception("Un joueur a fait un pari mais la somme n'a pas été déduie de son compte. La nouvelle mise n'a pas été enregistrée.");
                        }
                    }
                    else
                    {
                        throw new Exception("Une erreur est survenue lors du placement du pari. Veuillez réessayer.");
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = ex.Message;
                }
            }
        }

        private void addBetDb(int MatchId, int TeamId, int Amount)
        {
            try
            {
                MySQLWrapper Bd = new MySQLWrapper();
                List<string> col = new List<string>() { "Mise", "Profit", "User_Username", "Team_IdTeam", "Match_IdMatch" };
                
                List<OdbcParameter> parameters = new List<OdbcParameter>() {
                    new OdbcParameter(":Mise", Amount),
                    new OdbcParameter(":Profit", 0.ToString()),
                    new OdbcParameter(":Username", ((User)Session["User"]).Username),
                    new OdbcParameter(":Team", TeamId),
                    new OdbcParameter(":Match", MatchId)
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
                    List<OdbcParameter> values = new List<OdbcParameter>() { new OdbcParameter(":Credit", ((User)Session["User"]).Credits - Amount), new OdbcParameter(":EXP", xp), new OdbcParameter(":LVL", lvl) };
                    List<OdbcParameter> user = new List<OdbcParameter>() { new OdbcParameter(":Username", ((User)Session["User"]).Username) };
                    int updateresult = Bd.Update("user", columns, values, "Username = ?", user);
                    if (updateresult == 1)
                    {
                        Session["User"] = Bd.GetUserFromDB(((User)Session["User"]).Username);
                    }
                    else
                    {
                        List<OdbcParameter> conditions = new List<OdbcParameter>() {
                            new OdbcParameter(":Username", ((User)Session["User"]).Username),
                            new OdbcParameter(":TeamId", TeamId),
                            new OdbcParameter(":MatchId", MatchId)
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
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
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

                    List<OdbcParameter> Bet = new List<OdbcParameter>() {
                        new OdbcParameter(":TeamId", tId),
                        new OdbcParameter(":MatchId", mId),
                        new OdbcParameter(":Username", ((User)Session["User"]).Username)
                    };

                    DataTable resultBet = Bd.Select("bet", "Team_IdTeam = ? and Match_IdMatch = ? and User_Username = ?", Bet, "Mise", "User_Username", "Team_IdTeam", "Match_IdMatch", "Profit");
                    if (resultBet.Rows.Count > 0)
                    {
                        int Amount = (int)resultBet.Rows[0]["Mise"];
                        if ((int)resultBet.Rows[0]["Profit"] != 0)
                            throw new Exception("Une erreur est survenue lors de la supression du pari.");

                        Bet = new List<OdbcParameter>() {
                            new OdbcParameter(":TeamId", tId),
                            new OdbcParameter(":MatchId", mId),
                            new OdbcParameter(":Username", ((User)Session["User"]).Username)
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
                            List<OdbcParameter> values = new List<OdbcParameter>() { new OdbcParameter(":Credit", ((User)Session["User"]).Credits + Amount), new OdbcParameter(":EXP", xp), new OdbcParameter(":LVL", lvl) };
                            List<OdbcParameter> user = new List<OdbcParameter>() { new OdbcParameter(":Username", ((User)Session["User"]).Username) };
                            int updateresult = Bd.Update("user", columns, values, "Username = ?", user);
                            if (updateresult == 1)
                            {
                                Session["User"] = Bd.GetUserFromDB(((User)Session["User"]).Username);
                            }
                            else
                            {
                                List<string> col = new List<string>() { "Mise", "Profit", "User_Username", "Team_IdTeam", "Match_IdMatch" };

                                List<OdbcParameter> parameters = new List<OdbcParameter>() {
                                    new OdbcParameter(":Mise", (int)resultBet.Rows[0]["Mise"]),
                                    new OdbcParameter(":Profit", 0),
                                    new OdbcParameter(":Username", ((User)Session["User"]).Username),
                                    new OdbcParameter(":Team", (int)resultBet.Rows[0]["Team_IdTeam"]),
                                    new OdbcParameter(":Match", (int)resultBet.Rows[0]["Match_IdMatch"])
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

            DataTable users = new MySQLWrapper().Procedure("GetUserFromTeamBet", new OdbcParameter(":IdTeam", TeamId), new OdbcParameter(":IdMatch", MatchId));
            
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
                List<Match> Matches = Bd.GetMatches(true, lastMatchId);

                if (Matches.Count() > 0)
                {
                    List<Bet> Bets = new List<Bet>();
                    DataTable BetResult = Bd.Procedure("GetBetUser", new OdbcParameter(":Username", ((User)Session["User"]).Username));

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

                    BetResult = Bd.Select("bet", "", new List<OdbcParameter>(), "*");

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

                        matches_obj.Add(new { date = m.Date.Millisecond, id = m.Id, teams = teams });
                    }

                    object json = new { date = Matches[0].Date.Millisecond, matches = matches_obj };
                    return Json(json, JsonRequestBehavior.AllowGet);
                }
                else
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "No more matches");
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "An error as occured");
            }
        }
    }
}