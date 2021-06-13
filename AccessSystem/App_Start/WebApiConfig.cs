using System.Web.Http;

namespace AccessCheckSystem
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            // Register the Access system route
            // Регистрация маршрутизации по ТЗ
            config.Routes.MapHttpRoute(
                name: "AccessCheckRoute",
                routeTemplate: "check",
                defaults: new { controller = "AccessCheckSystem", action = "Check" }
            );

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);
        }
    }
}
