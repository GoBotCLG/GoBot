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

            ViewBag.LiveStats = Teams;
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
                
                DataTable UserResult = Bd.Procedure("Connect", new OdbcParameter(":username", user.Username), new OdbcParameter(":password", PasswordEncrypter.EncryptPassword(user.Password, pass.Rows[0]["Password"].ToString().Substring(0, 64))));

                if (UserResult.Rows.Count > 0)
                {
                    User sessionuser = new User();
                    sessionuser.Username = UserResult.Rows[0]["Username"].ToString();
                    sessionuser.Email = UserResult.Rows[0]["Email"].ToString();
                    sessionuser.ProfilPic = UserResult.Rows[0]["Image"].ToString();
                    sessionuser.Credits = (int)UserResult.Rows[0]["Credit"];
                    sessionuser.SteamID = UserResult.Rows[0]["SteamProfile"].ToString();
                    sessionuser.Wins = (int)UserResult.Rows[0]["Win"];
                    sessionuser.Games = (int)UserResult.Rows[0]["Game"];
                    sessionuser.TotalCredits = (int)UserResult.Rows[0]["TotalCredit"];
                    sessionuser.EXP = (int)UserResult.Rows[0]["EXP"];
                    sessionuser.Level = (int)UserResult.Rows[0]["LVL"];
                    Session["User"] = sessionuser;

                    return RedirectToAction("Index", "Account");
                }
                else
                {
                    ViewBag.Error = "Nom d'utilisateur ou mot de passe invalide.";
                    return View(user);
                }
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

            ViewBag.LiveStats = Teams;
            return Json(Teams);
        }
    }
}