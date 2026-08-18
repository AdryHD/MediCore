using System.Web;
using System.Web.Optimization;

namespace MediCore
{
    public class BundleConfig
    {

        public static void RegisterBundles(BundleCollection bundles)
        {

            BundleTable.EnableOptimizations = false;
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new StyleBundle("~/Content/css-auth").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-icons.css",
                      "~/Content/app.css",
                      "~/Content/auth.css"));

            bundles.Add(new StyleBundle("~/Content/css-panel").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-icons.css",
                      "~/Content/app.css",
                      "~/Content/panel.css"));
        }
    }
}