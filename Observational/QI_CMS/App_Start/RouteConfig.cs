﻿using System.Web.Mvc;
using System.Web.Routing;

namespace ES_CapDien
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
            defaults: new { controller = "LoginAccount", action = "Login", id = UrlParameter.Optional }
            );
            //defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //    );
        }
    }
}