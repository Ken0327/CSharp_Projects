using System.Web.Mvc;
using System.Web.Routing;

namespace PTE_Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            //routes.MapRoute(
            //    name: "DashBoard_DashBoard_Home",
            //    url: "DashBoard/DashBoard/{id}",
            //    defaults: new { controller = "DashBoard", action = "DashBoard", id = UrlParameter.Optional }
            //);
            //
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}",
                defaults: new { controller = "DashBoard", action = "DashBoard" }
            );
            routes.MapRoute(
               name: "Fixture",
               url: "{controller}/{action}",
               defaults: new { controller = "FixtureBoard", action = "Index" }
           );
        }
    }
}