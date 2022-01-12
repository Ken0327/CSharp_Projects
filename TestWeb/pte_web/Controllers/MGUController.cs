using PTE_Web.Connections;
using PTE_Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace PTE_Web.Controllers
{
    public class MGUController : Controller
    {
        // GET: MGU
        public ActionResult MGUDashBoard()
        {
            var DateDict = InitialDateInfo();

            var PartNumberList = InitialGPNList(Request.Form["DropDownList_GPN"], DateDict["sdate"], DateDict["edate"])["gpn"];
            var gpn = Request.Form["DropDownList_GPN"] ?? PartNumberList.First().Text;
            gpn = PartNumberList.Exists(item => item.Value == gpn) ? gpn : PartNumberList.First().Value;
            PartNumberList.Find(item => item.Value == gpn).Selected = true;

            var SOList = MGUDataHandler.OutSOListByGPNAndDateTime(gpn, DateDict["sdate"], DateDict["edate"]);

            TempData["StartDate"] = DateDict["sdate"];
            TempData["EndDate"] = DateDict["edate"];
            TempData.Keep("Owner");
            TempData.Keep("StartDate");

            ViewBag.GPNList = PartNumberList;
            ViewBag.WorkGPN = gpn;
            ViewBag.SO = SOList.First().JobNumber;

            var PageTable = new MGUPageDataTable();
            var SingleSODetailList = MGUDataHandler.OutPerformanceBySingleShopOrderAndDateTime(SOList.First().JobNumber, gpn, DateDict["sdate"], DateDict["edate"]);

            PageTable.SOList = SOList;
            PageTable.SingleDetailSOList = SingleSODetailList;

            var ChartInfo = MGUDataHandler.OutMGUSOPerformanceChartList(SingleSODetailList);

            ViewBag.StationOutline = new JavaScriptSerializer().Serialize(ChartInfo.GetRange(0, ChartInfo.Count)).ToString();

            var StationInfo = MGUDataHandler.OutTestItemListBySOandStation(SOList.First().JobNumber, DateDict["sdate"], DateDict["edate"]);

            var ItemInfo = MGUDataHandler.OutStationTestItemInfoDict(StationInfo, SingleSODetailList);

            ViewBag.ItemInfoDict = ItemInfo;

            ViewBag.StationList = ItemInfo.Keys.ToList();

            ViewBag.StationStatusDict = MGUDataHandler.GenerateStationList(ItemInfo.Keys.ToList());

            return View(PageTable);
        }

        public ActionResult MGUHome()
        {
            var DataList = MGUDataHandler.GetAllDailyData();

            ViewBag.DailyJsonList = new JavaScriptSerializer().Serialize(DataList).ToString();

            return View();
        }

        [HttpPost]
        public ActionResult GetDailyInfo(string workdate)
        {
            var List_FYR = new List<DailyStationInfo>();

            var DetailList = MGUDataHandler.OutPerformanceByDateTime(workdate);

            var StationInfo = MGUDataHandler.OutTestItemListByDateTimeandStation(workdate);

            var JobInfo = MGUDataHandler.GetProductionJobByStation_Date(workdate);

            var ItemInfo = MGUDataHandler.OutStationTestItemInfoList(StationInfo, DetailList, JobInfo);

            //test

            ItemInfo.ForEach(item =>
            {
                List_FYR.Add(new DailyStationInfo()
                {
                    _FailItemList = item._FailItemList,
                    Station = item.Station,
                    Total = item.Total,
                    FYR = item.FYR,
                    Spare = double.Parse(item.Spare.ToString("F2")),
                    DetailLinkList = item.DetailLinkList
                });
            });

            return Json(List_FYR, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _GetSOStationData(string so, string gpn, string sdate, string edate)
        {
            var SOTable = MGUDataHandler.OutPerformanceBySingleShopOrderAndDateTime(so, gpn, sdate, edate);
            var ChartInfo = MGUDataHandler.OutMGUSOPerformanceChartList(SOTable);
            var StationInfo = MGUDataHandler.OutTestItemListBySOandStation(so, sdate, edate);
            var ItemInfo = MGUDataHandler.OutStationTestItemInfoDict(StationInfo, SOTable);
            ViewBag.StationOutline = new JavaScriptSerializer().Serialize(ChartInfo.GetRange(0, ChartInfo.Count)).ToString();

            ViewBag.SOTable = SOTable;

            ViewBag.StationList = ItemInfo.Keys.ToList();

            ViewBag.ItemInfoDict = ItemInfo;

            ViewBag.StationStatusDict = MGUDataHandler.GenerateStationList(ItemInfo.Keys.ToList());

            ViewBag.SO = so;

            return PartialView("_GetSOStationData", SOTable);
        }

        public ActionResult GetTestLogByESN_SN(string esn)
        {
            var TestStationLogPathDict = new Dictionary<string, string>();

            var ResultList = MGUDataHandler.OutAllTestResultByESN(esn);

            var StationList = ResultList.
                              GroupBy(c => c.TestStation).
                              Select(g => new { TestStation = g.Key, Count = g.Count() });

            return View();
        }

        private static Dictionary<string, string> InitialDateInfo(string org = "C5")
        {
            var Output = new Dictionary<string, string>();

            Output["sdate"] = DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd");
            Output["edate"] = DateTime.Now.ToString("yyyy-MM-dd");

            return Output;
        }

        private static Dictionary<string, List<SelectListItem>> InitialGPNList(string gpn, string sdate, string edate, string org = "C5")
        {
            var SelectDict = new Dictionary<string, List<SelectListItem>>();

            var GPNList = new List<SelectListItem>();
            var Source = MGUDataHandler.OutPartNumber(sdate, edate);
            foreach (var item in Source)
            {
                GPNList.Add(new SelectListItem { Text = item, Value = item, Selected = org == item ? true : false });
            }
            if (GPNList.Count != 0)
                GPNList.First().Selected = true;
            SelectDict["gpn"] = GPNList;
            return SelectDict;
        }

        [HttpPost]
        public ActionResult Get_CPKData(string SO, string Station, string Failitem)
        {
            var FixtureRawData = MGUDataHandler.GetFixtureRawDataByFailItemAndStation(SO, Station, Failitem);

            //ViewBag.Test = FixtureRawData.Count.ToString();

            FixtureRawData = FixtureRawData.Where(x => x.FailCount > 0).OrderByDescending(x => x.FailPercent).ToList();

            return Json(FixtureRawData, JsonRequestBehavior.AllowGet);
        }
    }
}