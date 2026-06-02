using System.Web;
using System.Web.Optimization;

namespace MediCore
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Deshabilitar optimización para evitar errores con JS moderno
            BundleTable.EnableOptimizations = false;
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Scripts del panel interno (Mazer)
            bundles.Add(new ScriptBundle("~/bundles/mazer").Include(
                      "~/Scripts/perfect-scrollbar.min.js",
                      "~/Scripts/bootstrap.bundle.min.js",
                      "~/Scripts/mazer-main.js"));

            // CSS páginas de autenticación (login, registro, recuperar acceso)
            bundles.Add(new StyleBundle("~/Content/css-auth").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-icons.css",
                      "~/Content/app.css",
                      "~/Content/auth.css"));

            // CSS panel interno
            bundles.Add(new StyleBundle("~/Content/css-panel").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-icons.css",
                      "~/Content/app.css",
                      "~/Content/panel.css"));
        }
    }
}
