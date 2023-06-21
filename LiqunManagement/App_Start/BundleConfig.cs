using System.Web;
using System.Web.Optimization;

namespace LiqunManagement
{
    public class BundleConfig
    {
        //smartadmin
        public static void RegisterBundles(BundleCollection bundles)
        {

            #region smartadmin
            bundles.Add(new StyleBundle("~/smartadmin/css").Include(
                        "~/Content/smartadmin-package-4.5.1/smartadmin-html-full/dist/css/vendors.bundle.css"
                        , "~/Content/smartadmin-package-4.5.1/smartadmin-html-full/dist/css/app.bundle.css"
                       //"~/Content/smartadmin-package-4.5.1/smartadmin-ajax-seed-alpha/dist/css/vendors.bundle.css"
                       //, "~/Content/smartadmin-package-4.5.1/smartadmin-ajax-seed-alpha/dist/css/app.bundle.css"
                       ));

            bundles.Add(new ScriptBundle("~/smartadmin/js").Include(
                        "~/Content/smartadmin-package-4.5.1/smartadmin-html-full/dist/js/vendors.bundle.js"
                        , "~/Content/smartadmin-package-4.5.1/smartadmin-html-full/dist/js/app.bundle.js"
                        //"~/Content/smartadmin-package-4.5.1/smartadmin-ajax-seed-alpha/dist/js/vendors.bundle.js"
                        //, "~/Content/smartadmin-package-4.5.1/smartadmin-ajax-seed-alpha/dist/js/app.bundle.js"

                        ));
            #endregion

            #region 原生
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // 使用開發版本的 Modernizr 進行開發並學習。然後，當您
            // 準備好可進行生產時，請使用 https://modernizr.com 的建置工具，只挑選您需要的測試。
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            #endregion
        }

    }
}
