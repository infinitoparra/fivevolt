using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ZigBeeCoordinator.Lib.Infrastructure;
using ZigBeeCoordinator.Lib.Models;

namespace ZigBeeCoordinator.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "DefaultWithExtension",
                "{controller}/{action}.{extension}/{id}",
                new { controller = "Home", action = "Index", extension = "html", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }

        protected void Application_Start()
        {
            MyLogManager.ConfigureFromWebConfig();

            // verify connection with database
            MyLogManager.GetLogger(typeof(MvcApplication)).Debug("Verifing connection with database");
            using (ZigBeeCoordinatorContext ctx = new ZigBeeCoordinatorContext())
            {
                ctx.DbCtx.CoordinatorUsers.FirstOrDefault();
            }

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}