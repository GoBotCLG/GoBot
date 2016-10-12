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
            MySQLWrapper Bd = new MySQLWrapper();
            List<Match> Matches = new List<Match>();
            List<Bet> Bets = new List<Bet>();

            DataTable MatchResult = Bd.Select("matchs", "Date>=now() order by Date", new List<OdbcParameter>(), "*");
            
            foreach(DataRow row in MatchResult.Rows)
            {
                Match m = new Match();
                Team t = new Team();
                Bot b = new Bot();

                for(int i = 0; i < 2; i++)
                {
                    DataTable teams = Bd.Procedure("TeamFromMatch", new OdbcParameter(":IdMatch", (int)row["Team_IdTeam" + (i + 1).ToString()]));
                    t.Id = (int)teams.Rows[i]["IdTeam"];
                    t.Name = teams.Rows[i]["Name"].ToString();
                    t.Wins = (int)teams.Rows[i]["Win"];
                    t.Games = (int)teams.Rows[i]["Game"];

                    //for(int j = 0; j < 5; j++)
                    //{
                    //    DataTable bots = Bd.Procedure("BotFromTeam", new OdbcParameter(":IdTeam", (int)teams.Rows[i]["IdTeam"]));
                    //    b.Id = bots.Rows;
                    //}
                }
            }

            List<OdbcParameter> param = new List<OdbcParameter>();

            OdbcParameter Username = new OdbcParameter(":Username", ((User)Session["User"]).Username);
            param.Add(Username);

            DataTable BetResult = Bd.Select("bet inner join matchs on bet.Match_IdMatch = matchs.IdMatch", "User_Username = ? order by Date", param, "*");

            foreach(DataRow row in BetResult.Rows)
            {
                Bet b = new Bet();
                b.Id = (int)row["IdBet"];
                b.Amount = (int)row["Mise"];
                b.Profit = (int)row["Profit"];
                b.Username = ((User)Session["User"]).Username;
                b.TeamId = (int)row["Team_IdTeam"];
                b.MatchId = (int)row["Match_IdMatch"];
                Bets.Add(b);
            }

            ViewBag.Matches = Matches;
            ViewBag.Bets = Bets;

            return View();
        }

        [HttpPost]
        public JsonResult Add(int MatchId, int TeamId, int Amount)
        {
            if(Session["User"] != null)
            {
                return Json(0);
            }
            else
            {
                MySQLWrapper Bd = new MySQLWrapper();

                DataTable UserResult = Bd.Procedure("GetUser", new OdbcParameter(":username", ((User)Session["User"]).Username));

                Session["User"] = Bd.GetUserFromDB(((User)Session["User"]).Username);

                if(((User)Session["User"]).Credits >= Amount)
                {
                    List<string> col = new List<string>();
                    col.Add("Mise");
                    col.Add("Profit");
                    col.Add("User_Username");
                    col.Add("Team_IdTeam");
                    col.Add("Match_IdMatch");
                    List<OdbcParameter> parameters = new List<OdbcParameter>();
                    parameters.Add(new OdbcParameter(":Mise", Amount));
                    parameters.Add(new OdbcParameter(":Profit", 0));
                    parameters.Add(new OdbcParameter(":Username", ((User)Session["User"]).Username));
                    parameters.Add(new OdbcParameter(":Team", TeamId));
                    parameters.Add(new OdbcParameter(":Match", MatchId));
                    int result = Bd.Insert("bet", col, parameters);

                    if(result > 0)
                    {
                        List<string> columns = new List<string>();
                        columns.Add("Credit");
                        List<OdbcParameter> values = new List<OdbcParameter>();
                        values.Add(new OdbcParameter(":Credit", ((User)Session["User"]).Credits - Amount));
                        List<OdbcParameter> conditions = new List<OdbcParameter>();
                        conditions.Add(new OdbcParameter(":Username", ((User)Session["User"]).Username));
                        int updateresult = Bd.Update("user", columns, values, "Username = ?", conditions);
                        if(updateresult == 1)
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
                        return Json(0);
                    }
                }
                else
                {
                    return Json(0);
                }
            }
        }

        [HttpPost]
        public JsonResult Remove(int BetId)
        {
            if((User)Session["User"] != null)
            {
                return Json(0);
            }
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