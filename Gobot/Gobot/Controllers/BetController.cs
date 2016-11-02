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
            //if ((User)Session["User"] == null)
            //    return RedirectToAction("Index", "Home");

            //MySQLWrapper Bd = new MySQLWrapper();
            //List<Match> Matches = new List<Match>();
            //List<Bet> Bets = new List<Bet>();

            //DataTable MatchResult = Bd.Procedure("GetMatchAfter", new OdbcParameter(":date", DateTime.Now));

            //foreach(DataRow row in MatchResult.Rows)
            //{
            //    Match m = new Match();
            //    Team t = new Team();
            //    Bot b = new Bot();

            //    m.Id = (int)row["IdMatch"];
            //    m.Date = Convert.ToDateTime(row["Date"].ToString());
            //    m.CurrentUserBet = false;
            //    m.TeamVictoire = 0;
            //    m.TeamNumberBet = -1;
            //    m.Team1TotalBet = 0;
            //    m.Team2TotalBet = 0;

            //    for(int i = 0; i < 2; i++)
            //    {
            //        DataTable teams = Bd.Procedure("TeamFromMatch", new OdbcParameter(":IdMatch", (int)row["Team_IdTeam" + (i + 1).ToString()]));
            //        t.Id = (int)teams.Rows[i]["IdTeam"];
            //        t.Name = teams.Rows[i]["Name"].ToString();
            //        t.Wins = (int)teams.Rows[i]["Win"];
            //        t.Games = (int)teams.Rows[i]["Game"];

            //        for (int j = 0; j < 5; j++)
            //        {
            //            DataTable bots = Bd.Procedure("BotFromTeam", new OdbcParameter(":IdTeam", (int)teams.Rows[i]["IdTeam"]));
            //            b.Id = (int)bots.Rows[j]["IdBot"];
            //            b.Name = bots.Rows[j]["NomBot"].ToString();
            //            b.Kills = Convert.ToInt32(bots.Rows[j]["KDA"].ToString().Split('/')[0]);
            //            b.Kills = Convert.ToInt32(bots.Rows[j]["KDA"].ToString().Split('/')[0]);
            //            b.Kills = Convert.ToInt32(bots.Rows[j]["KDA"].ToString().Split('/')[0]);

            //            t.TeamComp[j] = b;
            //        }

            //        m.Teams[i] = t;
            //        Matches.Add(m);
            //    }
            //}

            //DataTable BetResult = Bd.Procedure("GetBetUser", new OdbcParameter(":Username", ((User)Session["User"]).Username));

            //foreach(DataRow row in BetResult.Rows)
            //{
            //    Bet b = new Bet();
            //    b.Id = (int)row["IdBet"];
            //    b.Amount = (int)row["Mise"];
            //    b.Profit = (int)row["Profit"];
            //    b.Username = ((User)Session["User"]).Username;
            //    b.TeamId = (int)row["Team_IdTeam"];
            //    b.MatchId = (int)row["Match_IdMatch"];
            //    Bets.Add(b);
            //}

            //foreach(Bet bet in Bets)
            //{
            //    foreach(Match match in Matches)
            //    {
            //        if(bet.MatchId == match.Id)
            //        {
            //            match.CurrentUserBet = true;
            //            match.CurrentUserAmount = bet.Amount;
            //            if(bet.TeamId == match.Teams[0].Id)
            //            {
            //                match.TeamNumberBet = 1;
            //            }
            //            else
            //            {
            //                match.TeamNumberBet = 2;
            //            }
            //        }
            //    }
            //}

            //Bets.Clear();

            //BetResult = Bd.Select("bet", "", new List<OdbcParameter>(), "*");

            //foreach(DataRow row in BetResult.Rows)
            //{
            //    foreach(Match match in Matches)
            //    {
            //        if((int)row["Match_IdMatch"] == match.Id)
            //        {
            //            if((int)row["Team_IdTeam"] == match.Teams[0].Id)
            //            {
            //                match.Team1TotalBet += (int)row["Mise"];
            //            }
            //            else
            //            {
            //                match.Team2TotalBet += (int)row["Mise"];
            //            }
            //        }
            //    }
            //}

            //return View(Matches);
            return View(new List<Match>());
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
            return 0;
        }

        private ActionResult editBetDb(int MatchId, int TeamId, int oldAmount, int newAmount)
        {
            try
            {
                MySQLWrapper Bd = new MySQLWrapper();
                List<string> col = new List<string>() { "Mise" };
                List<OdbcParameter> parameters = new List<OdbcParameter>() { new OdbcParameter(":Mise", newAmount)  };
                string where = "";
                int result = Bd.Update("bet", col, parameters, where);

                if (result > 0)
                {
                    List<string> columns = new List<string>();
                    columns.Add("Credit");
                    List<OdbcParameter> values = new List<OdbcParameter>();
                    values.Add(new OdbcParameter(":Credit", ((User)Session["User"]).Credits - newAmount + oldAmount));
                    List<OdbcParameter> conditions = new List<OdbcParameter>();
                    conditions.Add(new OdbcParameter(":Username", ((User)Session["User"]).Username));
                    int updateresult = Bd.Update("user", columns, values, "Username = ?", conditions);
                    if (updateresult == 1)
                    {
                        Session["User"] = Bd.GetUserFromDB(((User)Session["User"]).Username);
                        return Json(1);
                    }
                    else
                    {
                        throw new Exception("Un joueur a fait un pari mais la somme n'a pas été déduie de son compte.");
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
                    List<string> columns = new List<string>();
                    columns.Add("Credit");
                    List<OdbcParameter> values = new List<OdbcParameter>();
                    values.Add(new OdbcParameter(":Credit", ((User)Session["User"]).Credits - Amount));
                    List<OdbcParameter> conditions = new List<OdbcParameter>();
                    conditions.Add(new OdbcParameter(":Username", ((User)Session["User"]).Username));
                    int updateresult = Bd.Update("user", columns, values, "Username = ?", conditions);
                    if (updateresult == 1)
                    {
                        Session["User"] = Bd.GetUserFromDB(((User)Session["User"]).Username);
                        return Json(1);
                    }
                    else
                    {
                        throw new Exception("Un joueur a fait un pari mais la somme n'a pas été déduie de son compte.");
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
            

        private void removeBetDb(int BetId)
        {

        }

        [HttpPost]
        public JsonResult Remove(int BetId)
        {
            if((User)Session["User"] == null)
                return Json(0);
            else
            {
                MySQLWrapper Bd = new MySQLWrapper();
                Session["User"] = Bd.GetUserFromDB(((User)Session["User"]).Username);

                List<OdbcParameter> list = new List<OdbcParameter>();
                list.Add(new OdbcParameter(":IdBet", BetId));

                DataTable resultBet = Bd.Select("bet", "IdBet = ?", list, "Mise", "User_Username");
                int amount = (int)resultBet.Rows[0]["Mise"];
                if(((User)Session["User"]).Username != resultBet.Rows[0]["User_Username"].ToString())
                {
                    return Json(0);
                }
                
                List<OdbcParameter> param = new List<OdbcParameter>();
                OdbcParameter Id = new OdbcParameter(":Id", OdbcType.Int, 11);
                Id.Value = BetId;

                param.Add(Id);

                int result = Bd.Delete("bet", "IdBet = ?", param);

                if(result > 0)
                {

                    List<string> columnNames = new List<string>();
                    columnNames.Add("Credit");
                    List<OdbcParameter> values = new List<OdbcParameter>();
                    values.Add(new OdbcParameter(":Credit", ((User)Session["User"]).Credits + amount));
                    List<OdbcParameter> conditions = new List<OdbcParameter>();
                    conditions.Add(new OdbcParameter(":Username", ((User)Session["User"]).Username));
                    int res = Bd.Update("user", columnNames, values, "Username = ?", conditions);

                    if(res > 0)
                    {
                        Session["User"] = Bd.GetUserFromDB(((User)Session["User"]).Username);
                        return Json(1);
                    }
                    else
                    {
                        return Json(0);
                    }
                }
                else
                {
                    return Json(0);
                }
            }
        }
    }
}