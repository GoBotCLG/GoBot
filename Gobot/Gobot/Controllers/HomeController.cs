using System.Collections;
using System.Web.Mvc;
using Gobot.Models;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Data;
using System.ComponentModel;

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
            //    MySQLWrapper Bd = new MySQLWrapper("Max", "yolo");
            //    List<List<object>> InfoLiveMatch = Bd.Function("GetLiveStats");
            //    string Team1 = InfoLiveMatch[0][1].ToString();
            //    string Team2 = InfoLiveMatch[0][3].ToString();

            //    ViewBag.Team1Id = InfoLiveMatch[0][0].ToString();
            //    ViewBag.Team2Id = InfoLiveMatch[0][2].ToString();

            //    ViewBag.Team1Name = Team1.Split('"')[1];
            //    ViewBag.Team2Name = Team2.Split('"')[1];

            //    string[] Team1Ids = { Team1.Split('"')[3], Team1.Split('"')[7], Team1.Split('"')[11], Team1.Split('"')[15], Team1.Split('"')[19] };
            //    ViewBag.Team1Ids = Team1Ids;

            //    string[] Team2Ids = { Team2.Split('"')[3], Team2.Split('"')[7], Team2.Split('"')[11], Team2.Split('"')[15], Team2.Split('"')[19] };
            //    ViewBag.Team2Ids = Team2Ids;

            //    string[] Team1Names = { Team1.Split('"')[5], Team1.Split('"')[9], Team1.Split('"')[13], Team1.Split('"')[17], Team1.Split('"')[21] };
            //    ViewBag.Team1Names = Team1Names;

            //    string[] Team2Names = { Team2.Split('"')[5], Team2.Split('"')[9], Team2.Split('"')[13], Team2.Split('"')[17], Team2.Split('"')[21] };
            //    ViewBag.Team2Names = Team2Names;

            //    ViewBag.Team1Score = Team1.Split('"')[25];
            //    ViewBag.Team2Score = Team2.Split('"')[25];

            LoginViewModel model = new LoginViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(LoginViewModel user)
        {
            if(user.Username != null & user.Username != "")
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
                    if(UserResult.Rows[0]["Image"].GetType() != typeof(System.DBNull))
                    {
                        byte[] imagebytes = (byte[])UserResult.Rows[0]["Image"];
                        TypeConverter tc = TypeDescriptor.GetConverter(typeof(System.Drawing.Bitmap));
                        sessionuser.ProfilPic = (System.Drawing.Bitmap)tc.ConvertFrom(imagebytes);
                    }                    
                    sessionuser.Credits = (int)UserResult.Rows[0]["Credit"];
                    sessionuser.SteamID = UserResult.Rows[0]["SteamProfile"].ToString();
                    sessionuser.Wins = (int)UserResult.Rows[0]["Win"];
                    sessionuser.Games = (int)UserResult.Rows[0]["Game"];
                    sessionuser.TotalCredits = (int)UserResult.Rows[0]["TotalCredit"];
                    sessionuser.EXP = (int)UserResult.Rows[0]["EXP"];
                    sessionuser.Level = (int)UserResult.Rows[0]["LVL"];
                    Session["User"] = sessionuser;

                    return RedirectToAction("Index", "Watch");
                }
                else
                {
                    ViewBag.Error = "Mauvais nom d'utilisateur/mot de passe";
                    LoginViewModel model1 = new LoginViewModel();
                    return View(model1);
                }
            }            
            LoginViewModel model = new LoginViewModel();
            return View(model);
        }

        /// <summary>
        /// Returns full informations for the current match in JSON form
        /// </summary>
        /// <returns>JSON data of the current match</returns>
        public JsonResult GetCurrentStats()
        {
            MySQLWrapper Bd = new MySQLWrapper();

            DataTable InfoLiveMatch = Bd.Function("GetLiveStats");

            string Team1 = InfoLiveMatch.Rows[0][1].ToString();
            string Team2 = InfoLiveMatch.Rows[0][3].ToString();
            
            return Json(new[] { /*2 Teams*/
                new { /*Team 1*/
                    TeamId = InfoLiveMatch.Rows[0][0].ToString(),
                    TeamName = Team1.Split('"')[1],
                    TeamComp = new[] {
                        new { /*Bot 1 of Team 1*/
                            BotId = Team1.Split('"')[3],
                            BotName = Team1.Split('"')[5]
                        },
                        new { /*Bot 2 of Team 1*/
                            BotId = Team1.Split('"')[7],
                            BotName = Team1.Split('"')[9]
                        },
                        new { /*Bot 3 of Team 1*/
                            BotId = Team1.Split('"')[11],
                            BotName = Team1.Split('"')[13]
                        },
                        new { /*Bot 4 of Team 1*/
                            BotId = Team1.Split('"')[15],
                            BotName = Team1.Split('"')[17]
                        },
                        new { /*Bot 5 of Team 1*/
                            BotId = Team1.Split('"')[19],
                            BotName = Team1.Split('"')[21]
                        }
                    },
                    Score = Team1.Split('"')[25]
                },
                new { /*Team 2*/
                    TeamId = InfoLiveMatch.Rows[0][2].ToString(),
                    TeamName = Team2.Split('"')[1],
                    TeamComp = new[] {
                        new { /*Bot 1 of Team 2*/
                            BotId = Team2.Split('"')[3],
                            BotName = Team2.Split('"')[5]
                        },
                        new { /*Bot 2 of Team 2*/
                            BotId = Team2.Split('"')[7],
                            BotName = Team2.Split('"')[9]
                        },
                        new { /*Bot 3 of Team 2*/
                            BotId = Team2.Split('"')[11],
                            BotName = Team2.Split('"')[13]
                        },
                        new { /*Bot 4 of Team 2*/
                            BotId = Team2.Split('"')[15],
                            BotName = Team2.Split('"')[17]
                        },
                        new { /*Bot 5 of Team 2*/
                            BotId = Team2.Split('"')[19],
                            BotName = Team2.Split('"')[21]
                        }
                    },
                    Score = Team2.Split('"')[25]
                }
            });
        }
    }
}