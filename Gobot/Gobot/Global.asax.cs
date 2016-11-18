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

            //Thread Thread_setBets = new Thread(new ThreadStart(DoWork_Bets));
            //Thread_setBets.Start();
        }

        protected void DoWork_Bets()
        {
            try
            {
                //var timer = new System.Threading.Timer( e => SetLastMatchBets(), null, TimeSpan.Zero, TimeSpan.FromMinutes(0.5));
            }
            catch (Exception) { }
        }
    }
}
