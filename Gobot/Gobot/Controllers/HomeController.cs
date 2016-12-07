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
                return RedirectToAction("Index", "Account");

            try
            {
                Match match = new MySQLWrapper().GetLiveMatch(0);
                if (match != null)
                {
                    ViewBag.Team1 = match.Teams[0];
                    ViewBag.Team2 = match.Teams[1];
                }
            }
            catch (Exception) { }

            return View();
        }

        [HttpPost]
        public ActionResult Index(LoginViewModel user)
        {
            if (user.Username != null && user.Username != "" && user.Username.Length <= 45)
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
    }
}