using Gobot.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Web.Mvc;

namespace Gobot.Controllers
{
    public class WatchController : Controller
    {
        // GET: Watch
        public ActionResult Index()
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            MySQLWrapper Bd = new MySQLWrapper();

            DataTable InfoLiveMatch = Bd.Procedure("IsMatchCurrent");

            if (InfoLiveMatch.Rows.Count > 0)
            {
                List<Team> teamList = new List<Team>() {
                    new MySQLWrapper().GetTeam(false, int.Parse(InfoLiveMatch.Rows[0]["Team_IdTeam1"].ToString()))[0],
                    new MySQLWrapper().GetTeam(false, int.Parse(InfoLiveMatch.Rows[0]["Team_IdTeam2"].ToString()))[0]
                    };
                JObject[] Teams = new JObject[2];
                if (InfoLiveMatch.Rows[0]["Team1"].ToString() != "")
                {
                    Teams[0] = JObject.Parse("");
                    Teams[1] = JObject.Parse("");
                }
                else
                {
                    Teams[0] = new JObject();
                    Teams[1] = new JObject();
                }

                ViewBag.LiveStats = Teams;
            }

            return View();
        }

        public ActionResult Schedule()
        {
            return View();
        }
    }
}