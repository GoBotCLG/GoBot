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
        }
    }
}

// TODO: Find how to count the number of videos watched + favorite team of user. (/Account/Index + /Watch/Index)
// TODO: Add xp when user watches a game.
// TODO: Code /Support/Contact with a form to send an email to us.
// TODO: Create a method (json) to get portion of bets to reduce load on refresh in [/stats/schedule, /stats/history] (load first 2 days on refresh and click button to load the next 20).
// TODO: Add an header to show which day is currently shown in [/stats/schedule, /stats/history] before the matches of the said day.
// TODO: Create (modify???) a method (json) to get current match information in /Home/Index.