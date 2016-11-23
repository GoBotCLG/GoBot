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

// TODO: Fix image links and storage to correctly display images.
// TODO: Find a way to store image files directly in folder (have the windows rights to do so).
// TODO: Create new popUp window in [/bet/index, /stats/schedule, /stats/history] to show users who have also bet on that team.
// TODO: In new popUp window, when user clicks another user, show details of this user in the same window.
// TODO: Remove comments in AccountController in DeleteAccount.
// TODO: Find how to count the number of videos watched + favorite team of user. (/Account/Index + /Watch/Index)
// TODO: Code the little css /Bet/Ad needs.
// TODO: Implement the number of GC gained and lost on betting for the user. (/Account/Index)
// TODO: Add xp when user places bet, when he wins a bet and when he watches a game.
// TODO: Code /Support/Contact with a form to send an email to us.
// TODO: Create a method (json) to get portion of bets to reduce load on refresh in [/bet/index, /stats/schedule, /stats/history] (load first day on refresh and click button to load the next 20).
// TODO: Add an header to show which day is currently shown in [/bet/index, /stats/schedule, /stats/history] before the matches of the said day.
// TODO: Reduce match length for the next week to be able to test EOM bet's handling...
// TODO: Create (modify???) a method (json) to get current match information in /Home/Index.