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


// TODO: Add try catch everywhere to prevent site from showing error 500.
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
