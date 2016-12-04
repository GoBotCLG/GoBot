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

// TODO: Create method that adds (1 video watched and 25xp) to user. (Check if match is finished, if not, check if row in 'expliaison' with session username and 
//       match_id exists, if yes, do nothing, if not, add xp/video and row in db.
// TODO: Create method /bet/GetPreviousDay.
// TODO: See if EOM popUp displays correctly.
// TODO: Add try catch everywhere to prevent site from showing error 500.
// TODO: Verify that only session user can access every method on the website.
// TODO: Code /Support/Contact with a form to send an email to us.
// TODO: Correctly display if incorrect information is entered in inputs.

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
