using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using MediCore.Models;

namespace MediCore
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Los inputs HTML5 type="date" siempre envían formato ISO (yyyy-MM-dd),
            // lo cual entra en conflicto con la cultura del servidor (es-CR, dd/MM/yyyy)
            // configurada en Web.config. Este binder evita que las fechas se guarden
            // silenciosamente como 01/01/0001.
            ModelBinders.Binders.Add(typeof(DateTime), new DateTimeModelBinder());
            ModelBinders.Binders.Add(typeof(DateTime?), new DateTimeModelBinder());
        }
    }
}
