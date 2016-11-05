using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gobot.Models;
using System.Data;
using System.Data.Odbc;
using System.ComponentModel;

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
            List<Match> Matches = Bd.GetFutureMatches(true);
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
            //return View(new List<Match>());
        }

        [HttpPost]
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
                    return editBetDb(MatchId, TeamId, oldAmount, Amount);
                else
                    return addBetDb(MatchId, TeamId, Amount);
            }
        }
        
        private int betExists(int MatchId, int TeamId)
        {
            MySQLWrapper Bd = new MySQLWrapper();
            List<OdbcParameter> conditions = new List<OdbcParameter>() {
                new OdbcParameter(":Username", ((User)Session["User"]).Username),
                new OdbcParameter(":TeamId", TeamId),
                new OdbcParameter(":MatchId", MatchId)
            };
            DataTable result = Bd.Select("bet", "User_Username = ? and Team_IdTeam = ? and Match_IdMatch = ?", conditions, "Mise");

            if (result.Rows.Count == 0)
                return 0;
            else
            {
                return (int)result.Rows[0]["Mise"];
            }
        }

        private ActionResult editBetDb(int MatchId, int TeamId, int oldAmount, int newAmount)
        {
            try
            {
                MySQLWrapper Bd = new MySQLWrapper();
                List<string> col = new List<string>() { "Mise" };
                List<OdbcParameter> parameters = new List<OdbcParameter>() { new OdbcParameter(":Mise", newAmount)  };
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
                        return Json(1);
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
                return Json(0);
            }
        }

        private ActionResult addBetDb(int MatchId, int TeamId, int Amount)
        {
            try
            {
                MySQLWrapper Bd = new MySQLWrapper();
                List<string> col = new List<string>() { "Mise", "Profit", "User_Username", "Team_IdTeam", "Match_IdMatch" };
                
                List<OdbcParameter> parameters = new List<OdbcParameter>() {
                    new OdbcParameter(":Mise", Amount),
                    new OdbcParameter(":Profit", 0),
                    new OdbcParameter(":Username", ((User)Session["User"]).Username),
                    new OdbcParameter(":Team", TeamId),
                    new OdbcParameter(":Match", MatchId)
                };
                int result = Bd.Insert("bet", col, parameters);

                if (result > 0)
                {
                    List<string> columns = new List<string>() { "Credit" };
                    List<OdbcParameter> values = new List<OdbcParameter>() { new OdbcParameter(":Credit", ((User)Session["User"]).Credits - Amount) };
                    List<OdbcParameter> user = new List<OdbcParameter>() { new OdbcParameter(":Username", ((User)Session["User"]).Username) };
                    int updateresult = Bd.Update("user", columns, values, "Username = ?", user);
                    if (updateresult == 1)
                    {
                        Session["User"] = Bd.GetUserFromDB(((User)Session["User"]).Username);
                        return Json(1);
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
                return Json(0);
            }
        }

        [HttpPost]
        public JsonResult Remove(int tId, int mId)
        {
            try
            {
                if ((User)Session["User"] == null)
                    return Json(0);
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

                        int result = Bd.Delete("bet", "Team_IdTeam = ? and Match_IdMatch = ? and User_Username = ?", Bet);
                        if (result > 0)
                        {

                            List<string> columns = new List<string>() { "Credit" };
                            List<OdbcParameter> values = new List<OdbcParameter>() { new OdbcParameter(":Credit", ((User)Session["User"]).Credits + Amount) };
                            List<OdbcParameter> user = new List<OdbcParameter>() { new OdbcParameter(":Username", ((User)Session["User"]).Username) };
                            int updateresult = Bd.Update("user", columns, values, "Username = ?", user);
                            if (updateresult == 1)
                            {
                                Session["User"] = Bd.GetUserFromDB(((User)Session["User"]).Username);
                                return Json(1);
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
                return Json(0);
            }
            
        }
    }
}