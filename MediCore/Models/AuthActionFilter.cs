using System.Web.Mvc;
using System.Web.Routing;

namespace MediCore.Models
{

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