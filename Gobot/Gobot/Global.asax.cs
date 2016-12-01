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

// TODO: see if EOM popUp displays correctly.
// TODO: correct EOM bet attribution
// TODO: correct getnextday method
// TODO: display correctly the guns used
// TODO: correct display of names that are too long for their container + add tag title everywhere that is a possibility.

// TODO: LATER||Create a method (json) to get portion of bets to reduce load on refresh in [/stats/schedule, /stats/history] (load first 2 days on refresh and click button to load the next 20).
// TODO: LATER||Add an header to show which day is currently shown in [/stats/schedule, /stats/history] before the matches of the said day.
// TODO: LATER||Code /Support/Contact with a form to send an email to us.
// TODO: LATER||Correctly display if incorrect information is entered in inputs.

/*
 
    <customErrors mode="On" defaultRedirect="~/Errors/500.html">
      <error statusCode="401" redirect="~/Errors/401.html" />
      <error statusCode="403" redirect="~/Errors/403.html" />
      <error statusCode="404" redirect="~/Errors/404.html" />
      <error statusCode="405" redirect="~/Errors/405.html" />
      <error statusCode="406" redirect="~/Errors/406.html" />
      <error statusCode="412" redirect="~/Errors/412.html" />
      <error statusCode="500" redirect="~/Errors/500.html" />
      <error statusCode="501" redirect="~/Errors/501.html" />
    </customErrors>
*/
