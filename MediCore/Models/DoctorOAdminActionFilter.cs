using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace MediCore.Models
{

    public class DoctorOAdminActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var rol = (filterContext.HttpContext.Session["NombreRol"] as string ?? "").ToUpper();

            if (rol != "ADMINISTRADOR" && rol != "DOCTOR")
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