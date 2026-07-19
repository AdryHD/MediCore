using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace MediCore.Models
{
    // Bloquea el acceso a un controlador/acción si el usuario autenticado no tiene
    // el rol ADMINISTRADOR. Debe combinarse con [AuthActionFilter] para garantizar
    // que ya exista una sesión activa antes de validar el rol.
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
