using System.Collections;
using System.Web.Mvc;
using Gobot.Models;
using System.Collections.Generic;
using System.Data.Odbc;

namespace Gobot.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
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

            return View();
        }

        [HttpPost]
        ActionResult Index(string UserName, string PassWord)
        {
            MySQLWrapper Bd = new MySQLWrapper("Max", "yolo");
            List<List<object>> ConnectResult = Bd.Function("Connect", new OdbcParameter(":username", UserName), new OdbcParameter(":password", PassWord));

            if(ConnectResult[0][0].ToString() == "1")
            {
                Session["username"] = UserName;
            }

            return View();
        }

        /// <summary>
        /// Returns full informations for the current match in JSON form
        /// </summary>
        /// <returns>JSON data of the current match</returns>
        string GetCurrentStats()
        {
            MySQLWrapper Bd = new MySQLWrapper("Max", "yolo");

            List<List<object>> InfoLiveMatch = Bd.Function("GetLiveStats");

            string Team1 = InfoLiveMatch[0][1].ToString();
            string Team2 = InfoLiveMatch[0][3].ToString();

            return Newtonsoft.Json.JsonConvert.SerializeObject(new[] { /*2 Teams*/
                new { /*Team 1*/
                    TeamId = InfoLiveMatch[0][0].ToString(),
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
                    TeamId = InfoLiveMatch[0][2].ToString(),
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