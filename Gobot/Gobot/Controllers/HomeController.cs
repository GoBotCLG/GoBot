using System.Collections;
using System.Web.Mvc;
using Gobot.Models;
using System.Collections.Generic;
using System.Data;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using MySql.Data.MySqlClient;

namespace Gobot.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Session["User"] != null)
            {
                return RedirectToAction("Index", "Account");
            }

            Match match = new MySQLWrapper().GetLiveMatch(0);
            if (match != null)
            {
                JObject[] Teams = new JObject[2];
                for (int i = 0; i < Teams.Length; i++)
                {
                    Teams[i] = new JObject();
                    Teams[i].Add("Name", match.Teams[i].Name);
                    Teams[i].Add("Wins", match.Teams[i].Wins);
                    Teams[i].Add("Games", match.Teams[i].Games);

                    JObject[] Bots = new JObject[5];
                    for (int j = 0; j < Bots.Length; j++)
                    {
                        Bots[j] = new JObject();
                        Bots[j].Add("Name", match.Teams[i].TeamComp[j].Name);
                        Bots[j].Add("Gun", match.Teams[i].TeamComp[j].Gun);
                    }

                    Teams[i].Add("Bot1", Bots[0]);
                    Teams[i].Add("Bot2", Bots[1]);
                    Teams[i].Add("Bot3", Bots[2]);
                    Teams[i].Add("Bot4", Bots[3]);
                    Teams[i].Add("Bot5", Bots[4]);
                }

                ViewBag.Team1 = match.Teams[0];
                ViewBag.Team2 = match.Teams[1];
            }

            return View();
        }

        [HttpPost]
        public ActionResult Index(LoginViewModel user)
        {
            if(user.Username != null && user.Username != "" && user.Username.Length <= 45)
            {
                MySQLWrapper Bd = new MySQLWrapper();

                MySqlParameter username = new MySqlParameter(":Username", user.Username);
                List<MySqlParameter> parameters = new List<MySqlParameter>();
                parameters.Add(username);
                DataTable pass = Bd.Select("user", "Username = ?", parameters, "Password");
                
                if (pass != null && pass.Rows.Count > 0)
                {
                    DataTable UserResult = Bd.Procedure("Connect", new MySqlParameter(":username", user.Username), new MySqlParameter(":password", PasswordEncrypter.EncryptPassword(user.Password, pass.Rows[0]["Password"].ToString().Substring(0, 64))));

                    if (UserResult.Rows.Count > 0)
                    {
                        Session["User"] = Bd.GetUserFromDB("", UserResult);
                        double offset;
                        try
                        {
                            offset = GetTimeOffset(Request.Form["clientTime"]);

                            if (offset > 25 || offset < -25)
                                offset = 0;
                        }
                        catch (Exception)
                        {
                            offset = 0;
                        }
                        Session["timeOffset"] = offset;
                        return RedirectToAction("Index", "Account");
                    }
                }

                TempData["error"] = "Nom d'utilisateur ou mot de passe invalide.";
                return View(user);
            }

            return View(user);
        }

        public static double GetTimeOffset(string time)
        {
            DateTime bdTime = new MySQLWrapper().GetBDTime().ToUniversalTime();
            try
            {
                DateTime clientTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(long.Parse(time));
                double offset = (clientTime - bdTime).TotalMinutes / 60;
                return (double)(Math.Round(2 * (offset))) / 2;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns full informations for the current match in JSON form
        /// </summary>
        /// <returns>JSON data of the current match</returns>
        public JsonResult GetCurrentStats()
        {
            MySQLWrapper Bd = new MySQLWrapper();

            DataTable InfoLiveMatch = Bd.Procedure("IsMatchCurrent");
            JObject[] Teams = new JObject[2];
            if (InfoLiveMatch.Rows[0]["Team1"].ToString() != "")
            {
                Teams[0] = JObject.Parse(InfoLiveMatch.Rows[0]["Team1"].ToString());
                Teams[1] = JObject.Parse(InfoLiveMatch.Rows[0]["Team2"].ToString());
            }
            else
            {
                Teams[0] = new JObject();
                Teams[1] = new JObject();
            }
            List<MySqlParameter> idTeam = new List<MySqlParameter>();
            idTeam.Add(new MySqlParameter(":IdTeam", (int)InfoLiveMatch.Rows[0]["Team_IdTeam1"]));
            DataTable Team1 = Bd.Select("team", "IdTeam = ?", idTeam, "Win", "Game");
            Teams[0].Add("Wins", (int)Team1.Rows[0]["Win"]);
            Teams[0].Add("Games", (int)Team1.Rows[0]["Game"]);
            idTeam.Clear();
            idTeam.Add(new MySqlParameter(":IdTeam", (int)InfoLiveMatch.Rows[0]["Team_IdTeam2"]));
            DataTable Team2 = Bd.Select("team", "IdTeam = ?", idTeam, "Win", "Game");
            Teams[1].Add("Wins", (int)Team2.Rows[0]["Win"]);
            Teams[1].Add("Games", (int)Team2.Rows[0]["Game"]);
            
            return Json(Teams.ToString(), JsonRequestBehavior.AllowGet);
        }
    }
}