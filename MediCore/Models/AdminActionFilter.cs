using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace MediCore.Models
{

    public class AdminActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var nombreRol = filterContext.HttpContext.Session["NombreRol"] as string;

            if (!string.Equals(nombreRol, "ADMINISTRADOR", StringComparison.OrdinalIgnoreCase))
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Home" },
                        { "action", "Principal" }
                    });

                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}