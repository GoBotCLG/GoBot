using Gobot.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Gobot
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Thread Thread_setBets = new Thread(new ThreadStart(DoWork_Bets));
            Thread_setBets.Start();
        }

        protected void DoWork_Bets()
        {
            try
            {
                //int minutes = 2; // Interval in minutes
                //System.Timers.Timer timer = new System.Timers.Timer(1000 * 60 * minutes);
                //timer.Elapsed += new ElapsedEventHandler(SetLastMatchBets);
                //timer.Start();
            }
            catch (Exception ex)
            {

            }
        }

        protected void SetLastMatchBets(object source, ElapsedEventArgs e)
        {
            try
            {
                MySQLWrapper Bd = new MySQLWrapper();
                DataTable lastMatch = Bd.Procedure("GetLastFinishedMatch");

                if (lastMatch != null && lastMatch.Rows.Count > 0)
                {
                    DataRow row = lastMatch.Rows[0];

                    if ((int)row["BetsHandled"] == 0)
                    {
                        DataTable bets = Bd.Procedure("GetBetsFromMatch", new OdbcParameter(":Id", row["Match_IdMatch"]));
                        
                        if (bets != null && bets.Rows.Count > 0)
                        {
                            foreach (DataRow bet in bets.Rows)
                            {
                                // toAdd = Si le joueur a bet sur la team gagnante, ajouter le montant du bet, sinon le soustraire.
                                int toAdd = (int)bet["Team"] == (int)row["Team_Victory"] ? (int)bet["Amount"] : -(int)bet["Amount"];
                                DataTable userCredits = Bd.Select("user", "Username = ?", new List<OdbcParameter>() { new OdbcParameter(":Username", bet["Username"]) }, "Credit");

                                if (userCredits != null && userCredits.Rows.Count > 0)
                                {
                                    List<OdbcParameter> values = new List<OdbcParameter>() { new OdbcParameter(":Credit", (int)userCredits.Rows[0]["Credit"] + toAdd) };
                                    List<OdbcParameter> user = new List<OdbcParameter>() { new OdbcParameter(":Username", bet["Username"]) };
                                    Bd.Update("user", new List<string>() { "Credit" }, values, "Username = ?", user);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
