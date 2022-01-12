using System.Web.Optimization;

namespace PTE_Web
{
    public class BundleConfig
    {
        // 如需統合的詳細資訊，請瀏覽 https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // 使用開發版本的 Modernizr 進行開發並學習。然後，當您
            // 準備好可進行生產時，請使用 https://modernizr.com 的建置工具，只挑選您需要的測試。
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"

                      ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",

                      "~/Content/site.css"));  //amcharts lib

            bundles.Add(new ScriptBundle("~/bundles/assets/jquery").Include(
                    "~/Content/assets/js/jquery.min.js",
                    "~/Content/assets/js/bootstrap.min.js",
                    "~/Content/assets/plugins/morris-chart/morris.js",
                    "~/Content/assets/plugins/morris-chart/raphael-min.js",
                    "~/Content/assets/js/jquery.slimscroll.js",
                    "~/Content/assets/js/jquery.nicescroll.js"

                    ));

            bundles.Add(new StyleBundle("~/bundles/assets/css").Include(
                        "~/Content/assets/css/bootstrap.min.css",
                        "~/Content/assets/css/bootstrap-glyphicons.css",
                        "~/Content/assets/css/jquery-ui.min.css",
                        "~/Content/assets/css/datatables/jquery.datatables.min",
                        "~/Content/assets/css/datatables/jquery.dataTables-custom.css",
                        "~/Content/assets/css/icons.css",
                        "~/Content/assets/css/style.css",
                        "~/Content/assets/css/responsive.css",
                        "~/Content/amcharts/amcharts3/export.css",
                        "~/Content/fancyGrid/fancy.min.css"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/amcharts").Include(
                        "~/Content/amcharts/amcharts3/amcharts.js",
                        "~/Content/amcharts/amcharts3/light.js",
                        "~/Content/amcharts/amcharts3/serial.js",
                        "~/Content/amcharts/amcharts3/export.min.js",

                        "~/Content/amcharts/amcharts4/core.js",
                        "~/Content/amcharts/amcharts4/charts.js",
                        "~/Content/amcharts/amcharts4/frozen.js"));

            bundles.Add(new ScriptBundle("~/bundles/assets/plugins").Include(
                    "~/Content/assets/pages/dashboard.js",
                      "~/Content/assets/js/functions.js",
                    "~/Content/assets/plugins/jquery-ui/jquery-ui.min.js",
                    "~/Content/assets/plugins/moment/moment.js",
                    "~/Content/assets/plugins/jquery-sparkline/jquery.charts-sparkline.js",
                    "~/Content/assets/plugins/inputmask/jasny-bootstrap.min.js"
                    ));

            bundles.Add(new ScriptBundle("~/bundles/assets/plugins/datatables").Include(
                    "~/Content/assets/plugins/datatables/jquery.datatables.min.js",
                     "~/Content/TableFilter/tablefilter.js",
                     "~/Scripts/fancyGrid/fancy.full.min.js"

                ));

            bundles.Add(new StyleBundle("~/bundles/datetimepicker/css").Include(
                    "~/Content/datetimepicker/css/flatpickr.css",
                    "~/Content/datetimepicker/css/dark.css",
                    "~/Content/TableFilter/style/tablefilter.css",
                    "~/Content/tablePlugin/css/themes/infragistics/infragistics.theme.css",
                    "~/Content/tablePlugin/css/structure/infragistics.css"
                ));

            bundles.Add(new ScriptBundle("~/bundles/datetimepicker/js").Include(
                    "~/Content/datetimepicker/js/flatpickr.js",
                    "~/Content/datetimepicker/js/confirmDate.js",
                    "~/Content/datetimepicker/js/weekSelect.js",
                    "~/Content/datetimepicker/js/rangePlugin.js",
                    "~/Content/datetimepicker/js/minMaxTimePlugin.js",
                    "~/Content/datetimepicker/js/flatpickr.2.js"
                ));
            bundles.Add(new StyleBundle("~/bundles/highCharts/css").Include(
                "~/Content/highCharts/highchartsloading.css"));

            bundles.Add(new ScriptBundle("~/bundles/highCharts").Include(
                    "~/Content/highCharts/highcharts.js",
                    "~/Content/highCharts/histogram-bellcurve.js"
                ));

            bundles.Add(new StyleBundle("~/bundles/bootstrap-switch/css").Include(
                 "~/Content/bootstrap-switch/css/jquery.btnswitch.min.css"
             ));
            bundles.Add(new ScriptBundle("~/bundles/bootstrap-switch").Include(
                  "~/Content/bootstrap-switch/jquery.btnswitch.min.js"
              ));

            bundles.Add(new StyleBundle("~/CustomCss").Include(
                "~/Content/ajaxLoading/css/preloader.css",
                "~/Content/segmented-controls.css"
             ));
            bundles.Add(new ScriptBundle("~/CustomJs").Include(
                  "~/Content/ajaxLoading/js/jquery.preloader.js",
                  "~/Content/ajaxLoading/js/jquery.blockUI.min.js",
                  "~/Scripts/circles.js",
                  "~/Scripts/jquery-ui.min.js",
                   "~/Scripts/infragistics.core.js",
                  "~/Scripts/infragistics.lob.js",
                  "~/Scripts/Chart.js",
                  "~/Scripts/utils.js"
              ));
        }
    }
}