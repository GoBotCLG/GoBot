using System.Collections;
using System.Web.Mvc;
using Gobot.Models;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Data;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

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

            //MySQLWrapper Bd = new MySQLWrapper();

            //DataTable InfoLiveMatch = Bd.Procedure("IsMatchCurrent");

            //if (InfoLiveMatch.Rows.Count > 0)
            //{
            //    JObject[] Teams = new JObject[2];
            //    if (InfoLiveMatch.Rows[0]["Team1"].ToString() != "")
            //    {
            //        Teams[0] = JObject.Parse(InfoLiveMatch.Rows[0]["Team1"].ToString());
            //        Teams[1] = JObject.Parse(InfoLiveMatch.Rows[0]["Team2"].ToString());
            //    }
            //    else
            //    {
            //        Teams[0] = new JObject();
            //        Teams[1] = new JObject();
            //    }
            //    List<OdbcParameter> idTeam = new List<OdbcParameter>();
            //    idTeam.Add(new OdbcParameter(":IdTeam", (int)InfoLiveMatch.Rows[0]["Team_IdTeam1"]));
            //    DataTable Team1 = Bd.Select("team", "IdTeam = ?", idTeam, "Win", "Game");
            //    Teams[0].Add("Wins", (int)Team1.Rows[0]["Win"]);
            //    Teams[0].Add("Games", (int)Team1.Rows[0]["Game"]);
            //    idTeam.Clear();
            //    idTeam.Add(new OdbcParameter(":IdTeam", (int)InfoLiveMatch.Rows[0]["Team_IdTeam2"]));
            //    DataTable Team2 = Bd.Select("team", "IdTeam = ?", idTeam, "Win", "Game");
            //    Teams[1].Add("Wins", (int)Team2.Rows[0]["Win"]);
            //    Teams[1].Add("Games", (int)Team2.Rows[0]["Game"]);

            //    ViewBag.LiveStats = Teams;
            //}

            return View();
        }

        [HttpPost]
        public ActionResult Index(LoginViewModel user)
        {
            if(user.Username != null && user.Username != "" && user.Username.Length <= 45)
            {
                MySQLWrapper Bd = new MySQLWrapper();

                OdbcParameter username = new OdbcParameter(":Username", user.Username);
                List<OdbcParameter> parameters = new List<OdbcParameter>();
                parameters.Add(username);
                DataTable pass = Bd.Select("user", "Username = ?", parameters, "Password");
                
                if (pass != null && pass.Rows.Count > 0)
                {
                    DataTable UserResult = Bd.Procedure("Connect", new OdbcParameter(":username", user.Username), new OdbcParameter(":password", PasswordEncrypter.EncryptPassword(user.Password, pass.Rows[0]["Password"].ToString().Substring(0, 64))));

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
            List<OdbcParameter> idTeam = new List<OdbcParameter>();
            idTeam.Add(new OdbcParameter(":IdTeam", (int)InfoLiveMatch.Rows[0]["Team_IdTeam1"]));
            DataTable Team1 = Bd.Select("team", "IdTeam = ?", idTeam, "Win", "Game");
            Teams[0].Add("Wins", (int)Team1.Rows[0]["Win"]);
            Teams[0].Add("Games", (int)Team1.Rows[0]["Game"]);
            idTeam.Clear();
            idTeam.Add(new OdbcParameter(":IdTeam", (int)InfoLiveMatch.Rows[0]["Team_IdTeam2"]));
            DataTable Team2 = Bd.Select("team", "IdTeam = ?", idTeam, "Win", "Game");
            Teams[1].Add("Wins", (int)Team2.Rows[0]["Win"]);
            Teams[1].Add("Games", (int)Team2.Rows[0]["Game"]);
            
            return Json(Teams.ToString(), JsonRequestBehavior.AllowGet);
        }
    }
}