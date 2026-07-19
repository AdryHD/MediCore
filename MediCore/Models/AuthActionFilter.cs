using System.Web.Mvc;
using System.Web.Routing;

namespace MediCore.Models
{
    // Bloquea el acceso a un controlador/acción si no hay una sesión de usuario activa.
    // Debe aplicarse a los controladores que requieren que el usuario haya iniciado sesión.
    public class AuthActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session["Consecutivo"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Home" },
                        { "action", "Index" }
                    });

                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
