using ClosedXML.Excel;
using NLog;
using PTE_Web.Connections;
using PTE_Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace PTE_Web.Controllers
{
    public class SpareToolController : Controller
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger _PTElogger = LogManager.GetLogger("PTE_WEBLogger");

        // GET: SpareTool
        [HttpPost]
        public ActionResult GetDisTributionData(int ItemNameType, int DBIndex)
        {
            var dt = TempData[ItemNameType.ToString()] as DataTable;
            TempData.Keep(ItemNameType.ToString());

            var list = dt.AsEnumerable().Where(x => x["Spare" + DBIndex] != DBNull.Value).Select(x => Convert.ToInt32(x["Spare" + DBIndex])).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDBindexDetailData(int ItemNameType, int DBIndex, string TestItem)
        {
            DataTable dt = DataHandlerFunctions.TestItemsHandler(ItemNameType);
            if (DBIndex != 0)
            {
                int Idx_TestItem = dt.AsEnumerable().Where(x => x["DBIndex"] != DBNull.Value && x.Field<int>("DBIndex") == DBIndex).Select(x => x.Field<int>("Idx_TestItem")).ToList()[0];
                ViewBag.CmdxpRepeat = dt.AsEnumerable().Where(x => x["DBIndex"] != DBNull.Value && x.Field<int>("DBIndex") == DBIndex).Select(x => x.Field<int>("xpRepeat")).ToList()[0];
                var CommandTbl = (from c in DataHandlerFunctions.GetTestCommands(Idx_TestItem).AsEnumerable()
                                  select new
                                  {
                                      Delay = c.Field<int>("CmdDelay"),
                                      CmdList = c.Field<int>("CmdList"),
                                      CmdRepeat = Convert.ToInt32(c["CmdRepeat"]),
                                      Des = Convert.ToInt32(c["CmdType"]) == 0 ? "IOP(" + Convert.ToInt32(c["Integer1"]) + "," + Convert.ToInt32(c["Integer2"]) + "," + Convert.ToInt32(c["Integer3"]) + ")" :
                                                Convert.ToInt32(c["CmdType"]) == 1 ? (c["String1"].ToString() == "Set" ? "Set power" + c["String2"].ToString() + " " + c["Double1"] + "V " + c["Double2"] + "A" : "Get current" + Convert.ToInt32(c["Integer1"]) + " average") :
                                                Convert.ToInt32(c["CmdType"]) == 3 ? c["String1"].ToString() + "" + c["String2"].ToString() + " " + Convert.ToInt32(c["Integer1"]) :
                                                Convert.ToInt32(c["CmdType"]) == 4 ? c["String1"].ToString() :
                                                Convert.ToInt32(c["CmdType"]) == 8 ? c["String2"].ToString() :
                                                Convert.ToInt32(c["CmdType"]) == 30 ? "UsbConnection Item" :
                                                Convert.ToInt32(c["CmdType"]) == 33 ? "RS232 Connect" :
                                                Convert.ToInt32(c["CmdType"]) == 113 ? c["String1"].ToString() + Convert.ToInt32(c["Integer1"]) :
                                                "Other Command(" + c["String1"].ToString() + ")",
                                  }
                                                     ).ToList();
                var result = new JavaScriptSerializer().Serialize(CommandTbl);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                int Idx_TestItem = dt.AsEnumerable().Where(x => (x["Name"]).ToString() == TestItem).Select(x => x.Field<int>("Idx_TestItem")).ToList()[0];
                ViewBag.CmdxpRepeat = 0;
                var CommandTbl = (from c in DataHandlerFunctions.GetTestCommands(Idx_TestItem).AsEnumerable()
                                  select new
                                  {
                                      Delay = c.Field<int>("CmdDelay"),
                                      CmdList = c.Field<int>("CmdList"),
                                      CmdRepeat = Convert.ToInt32(c["CmdRepeat"]),
                                      Des = Convert.ToInt32(c["CmdType"]) == 0 ? "IOP(" + Convert.ToInt32(c["Integer1"]) + "," + Convert.ToInt32(c["Integer2"]) + "," + Convert.ToInt32(c["Integer3"]) + ")" :
                                                Convert.ToInt32(c["CmdType"]) == 1 ? (c["String1"].ToString() == "Set" ? "Set power" + c["String2"].ToString() + " " + c["Double1"] + "V " + c["Double2"] + "A" : "Get current" + Convert.ToInt32(c["Integer1"]) + " average") :
                                                Convert.ToInt32(c["CmdType"]) == 3 ? c["String1"].ToString() + "" + c["String2"].ToString() + " " + Convert.ToInt32(c["Integer1"]) :
                                                Convert.ToInt32(c["CmdType"]) == 4 ? c["String1"].ToString() :
                                                Convert.ToInt32(c["CmdType"]) == 8 ? c["String2"].ToString() :
                                                Convert.ToInt32(c["CmdType"]) == 30 ? "UsbConnection Item" :
                                                Convert.ToInt32(c["CmdType"]) == 33 ? "RS232 Connect" :
                                                Convert.ToInt32(c["CmdType"]) == 113 ? c["String1"].ToString() + Convert.ToInt32(c["Integer1"]) :
                                                "Other Command(" + c["String1"].ToString() + ")",
                                  }
                                                     ).ToList();
                var result = new JavaScriptSerializer().Serialize(CommandTbl);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Index(string org, string itemnametype, string startdate, string enddate)
        {
            _logger.Trace($"TEST_DashBoard_{DateTime.Now.ToString()}");
            _PTElogger.Debug($"TEST_DashBoard_{DateTime.Now.ToString()}");
            var AllModels = new SparePageModels();

            if (startdate == null || enddate == null || startdate == "" || enddate == "")
            {
                if (TempData["StartDate"] != null || TempData["EndDate"] != null)
                {
                    if (startdate == "" || enddate == "")
                    {
                        startdate = TempData["StartDate"].ToString();
                        enddate = TempData["EndDate"].ToString();
                    }
                }
                else
                {
                    startdate = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd");
                    enddate = DateTime.Now.ToString("yyyy-MM-dd");
                }
            }

            var PageConfig = new InitialParameter(org, itemnametype, startdate, enddate);
            var SpecDictByFailItem = new Dictionary<int, TableRow>();
            PageConfig.Source = DataHandlerFunctions.GetDBByItemNameType(PageConfig.ItemNameType) == "TblCpu" ? "ATE" : "FT";
            TempData["StartDate"] = startdate;
            TempData["EndDate"] = enddate;
            TempData["ItemNameType"] = PageConfig.ItemNameType;
            TempData["Org"] = PageConfig.Org;
            TempData["Source"] = PageConfig.Source;
            TempData.Keep("Source");
            TempData.Keep("Org");
            TempData.Keep("ItemNameType");
            TempData.Keep("StartDate");
            TempData.Keep("EndDate");
            DataTable SpareTbl = new DataTable();
            AllModels.ItemSapreDatas = FailItemModel.GetFailItemSpareAndDelay(PageConfig.Org, PageConfig.ItemNameType, PageConfig.SdateUTC, PageConfig.EdateUTC, PageConfig.Source, out SpareTbl);
            TempData[PageConfig.ItemNameType.ToString()] = SpareTbl;
            var SpareData = (from r in AllModels.ItemSapreDatas.AsEnumerable()
                             orderby r._AvgSpare descending
                             select new
                             {
                                 xpRepeat = r._xpRepeat,
                                 ItemNameType = PageConfig.ItemNameType,
                                 DBIndex = r._DbIndex,
                                 Spare = r._AvgSpare,
                                 Item = r._TestItem
                             }).Take(10).ToList();
            var RetryData = (from r in AllModels.ItemSapreDatas.AsEnumerable()
                             orderby r._AvgRetry descending
                             select new
                             {
                                 xpRepeat = r._xpRepeat,
                                 ItemNameType = PageConfig.ItemNameType,
                                 DBIndex = r._DbIndex,
                                 Retry = r._AvgRetry,
                                 Item = r._TestItem
                             }).Take(10).ToList();
            ViewBag.Top10_Spare = new JavaScriptSerializer().Serialize(SpareData);
            ViewBag.Top10_Retry = new JavaScriptSerializer().Serialize(RetryData);
            ViewBag.TitleDescription = DataHandlerFunctions.GetTestConfigByItemNameType(PageConfig.ItemNameType).Description;
            ViewBag.SpareDaily = new JavaScriptSerializer().Serialize(DataHandlerFunctions.GetSpareDaily(PageConfig.ItemNameType, PageConfig.Org, PageConfig.SdateUTC, PageConfig.EdateUTC));
            ViewBag.SpareDailyGpn = new JavaScriptSerializer().Serialize(DataHandlerFunctions.GetSpareGPN(PageConfig.ItemNameType, PageConfig.SdateUTC, PageConfig.EdateUTC));
            return View(AllModels);
        }

        public ActionResult cusDataExport(string Title, string org, string itemnametype, string startdate, string enddate)
        {
            var PageConfig = new InitialParameter(org, itemnametype, startdate, enddate);
            PageConfig.Source = DataHandlerFunctions.GetDBByItemNameType(PageConfig.ItemNameType) == "TblCpu" ? "ATE" : "FT";
            DataTable SpareTbl = new DataTable();
            var Content = FailItemModel.GetFailItemSpareAndDelay(PageConfig.Org, PageConfig.ItemNameType, PageConfig.SdateUTC, PageConfig.EdateUTC, PageConfig.Source, out SpareTbl);

            using (XLWorkbook wb = new XLWorkbook())
            {
                var data = Content.Select(c => new { c._DbIndex, c._TestItem, c._AvgSpare, c._Skewness_First, c._Skewness_Second, c._AvgRetry, c._xpRepeat, c.Timeout, c._PassDelay, c._FailDelay, c._FailDelay_Repeat, c._MaxDelay });

                var ws = wb.Worksheets.Add(Title, 1);

                int ColIndex = 1;
                foreach (var item in typeof(FailItemModel.ItemSpareData).GetProperties())
                {
                    if (item.Name != "Source" && item.Name != "Date")
                        ws.Cell(1, ColIndex++).Value = item.Name;
                }

                ws.Cell(2, 1).InsertData(data);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    wb.SaveAs(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return this.File(memoryStream.ToArray(), "application/vnd.ms-excel", $@"{PageConfig.ItemNameType}_Spare.xlsx");
                }
            }
        }
    }
}