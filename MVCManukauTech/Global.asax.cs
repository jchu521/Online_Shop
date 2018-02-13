using MVCManukauTech.Models;
using System.Data.Entity;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
//140803 JPC add this to fix routing problem not finding Web API URLs. Ref source:
// http://stackoverflow.com/questions/21934223/web-api-2-routing-the-resource-cannot-be-found
using System.Web.Http;

namespace MVCManukauTech
{
    // Note: For instructions on enabling IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=301868
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            //140803 JPC fix routing problems see details in comment above
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //140825 JPC had problem with Session always changing
        //StackOverflow article recommends "assign some data to the Session Object" to get it to work.  Maybe it needs a wake-up call?
        //ref: http://stackoverflow.com/questions/281881/sessionid-keeps-changing-in-asp-net-mvc-why
        //
        protected void Session_Start()
        {
            Session["OrderId"] = 0;
        }
    }
}
