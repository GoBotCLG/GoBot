using System.Collections;
using System.Web.Mvc;
using Gobot.Models;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Data;
using System.ComponentModel;
using Newtonsoft.Json.Linq;

namespace Gobot.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if(Session["User"] != null)
            {
                return RedirectToAction("Index", "Watch");
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
                
                if (pass.Rows.Count > 0)
                {
                    DataTable UserResult = Bd.Procedure("Connect", new OdbcParameter(":username", user.Username), new OdbcParameter(":password", PasswordEncrypter.EncryptPassword(user.Password, pass.Rows[0]["Password"].ToString().Substring(0, 64))));

                    if (UserResult.Rows.Count > 0)
                    {
                        User sessionuser = new User();
                        sessionuser.Username = UserResult.Rows[0]["Username"].ToString();
                        sessionuser.Email = UserResult.Rows[0]["Email"].ToString();
                        sessionuser.ProfilPic = UserResult.Rows[0]["Image"].ToString().Replace("=\"\" ", "/");
                        if (UserResult.Rows[0]["Credit"].GetType() != typeof(System.DBNull))
                        {
                            sessionuser.Credits = (int)UserResult.Rows[0]["Credit"];
                        }
                        else
                        {
                            sessionuser.Credits = 0;
                        }
                        sessionuser.SteamID = UserResult.Rows[0]["SteamProfile"].ToString();
                        if(UserResult.Rows[0]["Win"].GetType() != typeof(System.DBNull))
                        {
                            sessionuser.Wins = (int)UserResult.Rows[0]["Win"];
                        }
                        else
                        {
                            sessionuser.Wins = 0;
                        }
                        if (UserResult.Rows[0]["Game"].GetType() != typeof(System.DBNull))
                        {
                            sessionuser.Games = (int)UserResult.Rows[0]["Game"];
                        }
                        else
                        {
                            sessionuser.Games = 0;
                        }
                        if (UserResult.Rows[0]["TotalCredit"].GetType() != typeof(System.DBNull))
                        {
                            sessionuser.TotalCredits = (int)UserResult.Rows[0]["TotalCredit"];
                        }
                        else
                        {
                            sessionuser.TotalCredits = 0;
                        }
                        if (UserResult.Rows[0]["EXP"].GetType() != typeof(System.DBNull))
                        {
                            sessionuser.EXP = (int)UserResult.Rows[0]["EXP"];
                        }
                        else
                        {
                            sessionuser.EXP = 0;
                        }
                        if (UserResult.Rows[0]["LVL"].GetType() != typeof(System.DBNull))
                        {
                            sessionuser.Level = (int)UserResult.Rows[0]["LVL"];
                        }
                        else
                        {
                            sessionuser.Level = 1;
                        }
                        Session["User"] = sessionuser;

                        return RedirectToAction("Index", "Account");
                    }
                }

                ViewBag.Error = "Nom d'utilisateur ou mot de passe invalide.";
                return View(user);
            }

            return View(user);
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