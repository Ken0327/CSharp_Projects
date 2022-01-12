using ClosedXML.Excel;
using Newtonsoft.Json;
using NLog;
using PTE_Web.Connections;
using PTE_Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace PTE_Web.Controllers
{
    public class DashBoardController : Controller
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger _PTElogger = LogManager.GetLogger("PTE_WEBLogger");

        // GET: DashBoard
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DashBoard()
        {
            //Test Config
            _logger.Trace($"TEST_DashBoard_{DateTime.Now.ToString()}");
            _PTElogger.Debug($"TEST_DashBoard_{DateTime.Now.ToString()}");

            var PageConfig = new DashBoardConfig(DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd"), "", DateTime.Today.ToString("yyyy-MM-dd"), "", "", "");

            TempData["Org"] = "All";

            var OrgList = new List<string>() { "T1", "T2", "T3", "T5" };

            var FYRTables = DataHandlerFunctions.GetFYRByDateRange(PageConfig.Sdate, PageConfig.Edate);

            var nonITMFYRTables = DataHandlerFunctions.GetFYRByDateRangeNonITM(PageConfig.Sdate, PageConfig.Edate, false);

            var TopFailRank = FYRTables.Count <= 20 ? FYRTables.Count : 20;

            var PerformanceByOrg = DataHandlerFunctions.GetGroupATEandFTPerformace(FYRTables, OrgList);

            var PerfotmanceHLByOrg = DataHandlerFunctions.GetGroupATEandFT_FYR_HL(FYRTables, OrgList);

            var IssueReportData = DataHandlerFunctions.GetMonthlyIssueReport();

            var IssueReportData_nonITM = DataHandlerFunctions.GetMonthlyIssueReport_NonITM();

            ViewBag.OrgList = OrgList;

            ViewBag.StationList = DataHandlerFunctions.GetTestStatieonList(PageConfig.Sdate, PageConfig.Edate);

            ViewBag.GroupList = DataHandlerFunctions.GetTestGroupList(PageConfig.Sdate, PageConfig.Edate);

            ViewBag.ItemNameTypeList = DataHandlerFunctions.GetItemNameTypeList();

            ViewBag.nonITMData = nonITMFYRTables;

            ViewBag.OrgPerformanceList = PerformanceByOrg;

            ViewBag.OrgPerformanceHL = PerfotmanceHLByOrg;

            ViewBag.IssueMonthlyReport = IssueReportData;

            ViewBag.IssueMonthlyReport_NonITM = IssueReportData_nonITM;
            ViewBag.FliterTable = (from item in FYRTables
                                   select new
                                   {
                                       description = item.Description,
                                       total = item.Total,
                                       avg_pass_time = item.Avg_Pass_Time,
                                       fyr = item.FYR,
                                       org = item.Org
                                   }).ToList().GetRange(0, TopFailRank);
            ViewBag.Title = "DashBoard";

            ViewBag.TrendTitle = $@"Test Performance between {PageConfig.Sdate} to {PageConfig.Edate}";
            ViewBag.TrendSubTitle = "Org:All-Station:All-Group:All";

            var DateDict = General.GetWeekPeriodDate(PageConfig.Sdate, PageConfig.Edate);

            var Alldata = DataHandlerFunctions.ProcessGroupTrendData(DateDict, "ALL", "ALL", "ALL", "ALL");

            var TrendData = JsonConvert.SerializeObject(Alldata).ToString();

            ViewBag.Data = TrendData;

            ViewBag.TrendTitle = $@"Test Performance between {PageConfig.Sdate} to {PageConfig.Edate}";

            ViewBag.TrendSubTitle = $@"Org:ALL-Station:ALL-Group:ALL";

            return View(FYRTables);
        }

        [HttpPost]
        public ActionResult _DashBoard_Trend_Category(string org, string station, string group, string daterange)
        {
            var dateList = daterange.Split(' ');

            var sdate = dateList[0];

            var edate = dateList[2];

            var DateDict = General.GetWeekPeriodDate(sdate, edate);

            var Alldata = DataHandlerFunctions.ProcessGroupTrendData(DateDict, org, station, group, "ALL");

            var TrendData = JsonConvert.SerializeObject(Alldata).ToString();

            ViewBag.Data = TrendData;

            ViewBag.TrendTitle = $@"Test Performance between {sdate} to {edate}";

            ViewBag.TrendSubTitle = $@"Org:{org}-Station:{station}-Group:{group}";

            return PartialView("_DashBoard_Trend");
        }

        [HttpPost]
        public ActionResult _DashBoard_Trend_ItemNameType(string daterange,string org, string itemnametype)
        {
            var ItemNameType = itemnametype.Split('_')[0];

            var dateList = daterange.Split(' ');

            var sdate = dateList[0];

            var edate = dateList[2];

            var DateDict = General.GetWeekPeriodDate(sdate, edate);

            var Alldata = DataHandlerFunctions.ProcessGroupTrendData(DateDict, org, "ALL", "ALL", ItemNameType);

            var TrendData = JsonConvert.SerializeObject(Alldata).ToString();

            ViewBag.Data = TrendData;

            ViewBag.TrendTitle = $@"Test Performance between {sdate} to {edate}";

            ViewBag.TrendSubTitle = $@"{itemnametype}";

            return PartialView("_DashBoard_Trend");
        }

        public ActionResult DashBoardByOrg(string org, string startdate, string enddate)
        {
            //Test Config
            _logger.Trace($"TEST_DashBoard_{DateTime.Now.ToString()}");
            _PTElogger.Debug($"TEST_DashBoard_{DateTime.Now.ToString()}");

            var PageConfig = new DashBoardConfig(startdate, TempData["StartDate"], enddate, TempData["EndDate"], org, "");

            TempData["StartDate"] = PageConfig.Sdate;
            TempData["EndDate"] = PageConfig.Edate;
            TempData["Org"] = PageConfig.Org;
            TempData.Keep("Org");
            TempData.Keep("StartDate");
            TempData.Keep("EndDate");
            var FYRTables = DataHandlerFunctions.GetFYRByDateAndOrg(PageConfig.Sdate, PageConfig.Edate, PageConfig.Org);

            var nonITMFYRTables = DataHandlerFunctions.GetFYRByDateRangeNonITM(PageConfig.Sdate, PageConfig.Edate, false);

            var TopFailRank = FYRTables.Count <= 20 ? FYRTables.Count : 20;

            ViewBag.Model = FYRTables;

            ViewBag.FliterTable = (from item in FYRTables
                                   select new
                                   {
                                       description = item.Description,
                                       total = item.Total,
                                       avg_pass_time = item.Avg_Pass_Time,
                                       fyr = item.FYR,
                                       org = item.Org
                                   }).ToList().GetRange(0, TopFailRank);

            ViewBag.JsonScatter = new JavaScriptSerializer().Serialize(
                               (from item in FYRTables
                                where item.Total > 500
                                select new
                                {
                                    description = item.Description,
                                    total = item.Total.ToString(),
                                    avg_pass_time = item.Avg_Pass_Time.ToString(),
                                    fyr = item.FYR.ToString(),
                                    org = item.Org,
                                    color = Models.OrgInfoConfig.OrgColor[item.Org]
                                }).ToList()).ToString();

            ViewBag.Title = $@"DashBoard{org}";

            var FTList = (from item in FYRTables
                          where item.Source == "FT" && item.Total > 500
                          select new
                          {
                              description = item.Description,
                              fyr = item.FYR
                          }).ToList();

            ViewBag.FTJsonList = new JavaScriptSerializer().Serialize(FTList.GetRange(0, FTList.Count >= 20 ? 20 : FTList.Count)).ToString();

            var ATEList = (from item in FYRTables
                           where item.Source == "ATE" && item.Total > 500
                           select new
                           {
                               description = item.Description,
                               fyr = item.FYR
                           }).ToList();

            ViewBag.ATEJsonList = new JavaScriptSerializer().Serialize(ATEList.GetRange(0, ATEList.Count >= 20 ? 20 : ATEList.Count)).ToString();

            ViewBag.nonITMData = nonITMFYRTables.Where(x => x.Org == PageConfig.Org).ToList();

            return View(FYRTables);
        }

        [HttpPost]
        public JsonResult SearchNonITMFYR(string st, string et, string org, string des)
        {
            var nonITMFYRTables = DataHandlerFunctions.GetFYRByDateRangeNonITM(st, et, true);
            var Records = (from r in nonITMFYRTables.AsEnumerable()
                           where r.Org == org && r.Description == des
                           orderby r.Date ascending
                           select new
                           {
                               Total = r.Total,
                               FYR = r.FYR,
                               UPH_Achievement = r.UPH_Achievement,
                               Date = r.Date.ToString("yyyy-MM-dd")
                           })
                           .ToList();
            return Json(Records, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DashBoardByPerson(string owner, string startdate, string enddate)
        {
            //Test Config
            _logger.Trace($"TEST_DashBoard_{DateTime.Now.ToString()}");
            _PTElogger.Debug($"TEST_DashBoard_{DateTime.Now.ToString()}");

            var PageConfig = new DashBoardConfig(startdate, TempData["StartDate"], enddate, TempData["EndDate"], "", owner);

            var OrgsIssue = new List<IssueOutlie>();

            TempData["StartDate"] = PageConfig.Sdate;
            TempData["EndDate"] = PageConfig.Edate;
            TempData["Owner"] = PageConfig.Owner;
            TempData.Keep("Owner");
            TempData.Keep("StartDate");
            TempData.Keep("EndDate");

            var PersonalIssueItemNameType = DataHandlerFunctions.GetOwnerIssueList(PageConfig.Owner).OrderBy(row => row.CreateDate).Select(x => x.ItemNameType).ToList();

            var PersonalIssueDetail = DataHandlerFunctions.DataIssuesByDateRange().FindAll(x => PersonalIssueItemNameType.Contains(x.ItemNameType)).ToList();

            OrgsIssue.Add(new IssueOutlie { OPEN = PersonalIssueDetail.FindAll(x => x.Status).ToList().Count(), CLOSE = PersonalIssueDetail.FindAll(x => !x.Status).ToList().Count() });

            var FYRTables = DataHandlerFunctions.GetFYRByDateAndOwner(PageConfig.Sdate, PageConfig.Edate, int.Parse(PageConfig.Owner));

            var TopFailRank = FYRTables.Count <= 20 ? FYRTables.Count : 20;

            var PerformanceByOwner = DataHandlerFunctions.GetGroupATEandFTPerformace(FYRTables, new List<string>() { PageConfig.Owner });

            var PerfotmanceHLByOwner = DataHandlerFunctions.GetGroupATEandFT_FYR_HL(FYRTables, new List<string>() { PageConfig.Owner });

            var FYRList = FYRTables.Select(x => x.FYR).ToList();

            ViewBag.OrgsIssue = OrgsIssue;

            ViewBag.OwnerPerformanceList = PerformanceByOwner;

            ViewBag.OwnerPerformanceHL = PerfotmanceHLByOwner;

            ViewBag.Model = FYRTables;

            ViewBag.IssueList = PersonalIssueDetail;

            ViewBag.FliterTable = (from item in FYRTables
                                   select new
                                   {
                                       description = item.Description,
                                       total = item.Total,
                                       avg_pass_time = item.Avg_Pass_Time,
                                       fyr = item.FYR,
                                       org = item.Org
                                   }).ToList().GetRange(0, TopFailRank);

            ViewBag.JsonScatter = new JavaScriptSerializer().Serialize(
                               (from item in FYRTables
                                where item.Total > 500
                                select new
                                {
                                    description = item.Description,
                                    total = item.Total.ToString(),
                                    avg_pass_time = item.Avg_Pass_Time.ToString(),
                                    fyr = item.FYR.ToString(),
                                    org = item.Org,
                                    color = Models.OrgInfoConfig.OrgColor[item.Org]
                                }).ToList()).ToString();

            ViewBag.Title = $@"DashBoard_{owner}";

            var StationList = (from item in FYRTables
                               where item.Total > 500
                               select new
                               {
                                   description = item.Description,
                                   fyr = item.FYR
                               }).ToList();

            ViewBag.StationJsonList = new JavaScriptSerializer().Serialize(StationList.GetRange(0, StationList.Count >= 20 ? 20 : StationList.Count)).ToString();

            return View(FYRTables);
        }

        public ActionResult ProductOwnerEditor(string owner, List<string> input)
        {
            bool UpdateResult;
            if (owner != null && input != null)
            {
                var ItemList = DataHandlerFunctions.ProcessSelectedItemNameType(input, owner);
                UpdateResult = DataHandlerFunctions.UpdateDataIntoProductOwnerTable(ItemList);
            }
            var enddate = DateTime.Now.ToString("yyyy-MM-dd");
            var startdate = DateTime.Now.AddDays(-180).ToString("yyyy-MM-dd");
            var OwnerTable = DataHandlerFunctions.OutProductOwnerDefault(startdate, enddate);

            foreach (var item in OwnerTable)
            {
                if (item.ProductionDays >= 100)
                {
                    item.Priority = "danger";
                    item.PriorityString = "High ";
                    item.TipString = $@"During last 180 days. Station  production days: {item.ProductionDays.ToString()}. Higher than 100 days.";
                }
                else if (item.ProductionDays >= 50)
                {
                    item.Priority = "warning";
                    item.PriorityString = "Medium";
                    item.TipString = $@"During last 180 days. Station  production days: {item.ProductionDays.ToString()}. Higher than 50 days.";
                }
                else
                {
                    item.Priority = "info";
                    item.PriorityString = "Low ";
                    item.TipString = $@"During last 180 days. Station  production days: {item.ProductionDays.ToString()}. Lower than 50 days.";
                }
            }

            return View(OwnerTable);
        }

        public ActionResult ExportIssueReport()
        {
            var Content = DataHandlerFunctions.GetIssueExportData();
            using (XLWorkbook wb = new XLWorkbook())
            {
                var data = Content.Select(c => new { c.Org, c.ItemNameType, c.Description, c.First_Yield_Rate_Old, c.First_Yield_Rate_New, c.UPH_Achievement_Rate_Old, c.UPH_Achievement_Rate_New, c.Fail_Rate_Old, c.Fail_Rate_New, c.Top_Fail_Item, c.Action_FailItem, c.Editor, c.Owner, c.Cause_Type, c.Cause_Comment, c.Action_Type, c.Action_Comment, c.AttachmentLink });

                var ws1 = wb.Worksheets.Add("ITMXP_Report", 1);

                int ColIndex = 1;
                foreach (var item in typeof(ExportReportModel).GetProperties())
                {
                    if (item.Name == "LastDateString" || item.Name == "CurrentDateString" || item.Name == "Top1_Fail_Item" || item.Name == "Top2_Fail_Item" || item.Name == "Top3_Fail_Item")
                        continue;
                    else if (item.Name == "First_Yield_Rate_Old")
                        ws1.Cell(1, ColIndex++).Value = item.Name + " %" + "(" + Content.First().LastDateString + ")";
                    else if (item.Name == "First_Yield_Rate_New")
                        ws1.Cell(1, ColIndex++).Value = item.Name + " %" + "(" + Content.First().CurrentDateString + ")";
                    else if (item.Name == "Fail_Rate_Old")
                        ws1.Cell(1, ColIndex++).Value = item.Name + " %";
                    else if (item.Name == "Fail_Rate_New")
                        ws1.Cell(1, ColIndex++).Value = item.Name + " %";
                    else if (item.Name == "UPH_Achievement_Rate_Old")
                        ws1.Cell(1, ColIndex++).Value = item.Name + " %";
                    else if (item.Name == "UPH_Achievement_Rate_New")
                        ws1.Cell(1, ColIndex++).Value = item.Name + " %";
                    else if (item.Name == "Action_FailItem")
                        ws1.Cell(1, ColIndex++).Value = item.Name + " (Comparison with previous)";
                    else
                        ws1.Cell(1, ColIndex++).Value = item.Name;
                }
                ws1.Range("A1:R1").Style.Fill.SetBackgroundColor(XLColor.LightSkyBlue);

                ws1.Cell(2, 1).InsertData(data);

                //MERGE
                //https://github.com/ClosedXML/ClosedXML/wiki/Merging-Cells

                var fddd = "testc";

                for (var col = 65; col <= 84; col++)
                {
                    if (col == 67)
                        fddd = "ertre";

                    var ColChar = (char)col;

                    var row = 2;
                    var StartCell = $@"{ColChar}{row.ToString()}";
                    var StartCellString = ws1.Cell(StartCell).Value;
                    var StartRow = 2;
                    if (StartCell == "D2" && StartCellString.ToString() != "" && StartCellString.ToString() != "NaN" && ws1.Cell("E2").Value.ToString() != "" && ws1.Cell("E2").Value.ToString() != "NaN")
                    {
                        if (double.Parse(ws1.Cell("D2").Value.ToString()) > double.Parse(ws1.Cell("E2").Value.ToString()))
                            ws1.Cell("E2").Style.Font.FontColor = XLColor.Red;
                        else
                            ws1.Cell("E2").Style.Font.FontColor = XLColor.Blue;
                    }

                    row++;
                    while (row <= 2 + Content.Count)
                    {
                        var NowCell = $@"{ColChar}{row.ToString()}";
                        var NowCellString = ws1.Cell(NowCell).Value.ToString();

                        if (NowCellString == StartCellString.ToString() && col <= 73 && NowCellString != "")
                        {
                            if (col == 65)
                                ws1.Cell(NowCell).Value = null;
                            if (col == 66)
                                ws1.Cell(NowCell).Value = null;
                            fddd = ws1.Cell($@"B{row}").Value.ToString();
                            if (col != 66 && ws1.Cell($@"B{row}").Value.ToString() == "")
                                ws1.Cell(NowCell).Value = null;
                        }
                        else
                        {
                            if (ws1.Cell($@"{ColChar}{row - 1}").Value.ToString() == "" && col <= 73)
                            {
                                if (col == 66)
                                    ws1.Range($@"{StartCell}:{ColChar}{row - 1}").Column(1).Merge();

                                if (col == 65)
                                    ws1.Range($@"{StartCell}:{ColChar}{row - 1}").Column(1).Merge();

                                if (col != 66 && ws1.Cell($@"B{row - 1}").Value.ToString() == "")
                                    ws1.Range($@"{StartCell}:{ColChar}{row - 1}").Column(1).Merge();
                            }
                            StartCell = $@"{ColChar}{row.ToString()}";
                            StartCellString = ws1.Cell(NowCell).Value;
                            StartRow = row;
                        }

                        if (ColChar == 'D' && NowCellString != "NaN" && NowCellString != "" && ws1.Cell($@"{(char)(col + 1)}{row}").Value.ToString() != "NaN" && ws1.Cell($@"{(char)(col + 1)}{row}").Value.ToString() != "")
                        {
                            var OldValue = double.Parse(NowCellString.Replace("%", String.Empty));
                            var NowValue = double.Parse((ws1.Cell($@"{(char)(col + 1)}{row}").Value.ToString().Replace("%", String.Empty)));

                            if (NowValue >= OldValue)
                            {
                                ws1.Cell($@"{(char)(col + 1)}{row}").Style.Font.FontColor = XLColor.Blue;
                            }
                            else
                            {
                                ws1.Cell($@"{(char)(col + 1)}{row}").Style.Font.FontColor = XLColor.Red;
                            }
                        }

                        if (ColChar == 'F' && NowCellString != "NaN" && NowCellString != "" && ws1.Cell($@"{(char)(col + 1)}{row}").Value.ToString() != "NaN" && ws1.Cell($@"{(char)(col + 1)}{row}").Value.ToString() != "")
                        {
                            var OldValue = double.Parse(NowCellString.Replace("%", String.Empty));
                            var NowValue = double.Parse((ws1.Cell($@"{(char)(col + 1)}{row}").Value.ToString().Replace("%", String.Empty)));

                            if (NowValue >= OldValue)
                            {
                                ws1.Cell($@"{(char)(col + 1)}{row}").Style.Font.FontColor = XLColor.Blue;
                            }
                            else
                            {
                                ws1.Cell($@"{(char)(col + 1)}{row}").Style.Font.FontColor = XLColor.Red;
                            }
                        }

                        row++;
                    }
                }

                ws1.Range($@"A1:{(char)84}{(Content.Count + 1).ToString()}").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                ws1.Columns().AdjustToContents(); // Adjust column width
                ws1.Rows().AdjustToContents();  // Adjust row heights

                //var ws2 = wb.Worksheets.Add("NonITMXP_Report", 2);
                //var data2 = Content.Select(c => new { c.Org, c.ItemNameType, c.Description, c.First_Yield_Rate_Old, c.First_Yield_Rate_New, c.UPH_Achievement_Rate_Old, c.UPH_Achievement_Rate_New, c.Fail_Rate_Old, c.Fail_Rate_New, c.Top_Fail_Item, c.Action_FailItem, c.Editor, c.Owner, c.Cause_Type, c.Cause_Comment, c.Action_Type, c.Action_Comment, c.AttachmentLink });
                //ws2.Cell(2, 1).InsertData(data2);

                var FinalOutput = GetNonITMXP_Report(wb);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    FinalOutput.SaveAs(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return this.File(memoryStream.ToArray(), "application/vnd.ms-excel", $@"IssueReport_{DateTime.Today.ToString("yyyy-MM-dd")}.xlsx");
                }
            }
        }

        public XLWorkbook GetNonITMXP_Report(XLWorkbook book)
        {
            var Content = DataHandlerFunctions.GetIssueExportData_NonITMXP();

            using (XLWorkbook wb = new XLWorkbook())
            {
                try
                {
                    var data = Content.Select(c => new { c.Org, c.ItemNameType, c.Description, c.First_Yield_Rate_Old, c.First_Yield_Rate_New, c.UPH_Achievement_Rate_Old, c.UPH_Achievement_Rate_New, c.Editor, c.Owner, c.Cause_Type, c.Cause_Comment, c.Action_Type, c.Action_Comment, c.AttachmentLink });

                    var ws1 = book.Worksheets.Add("NonITMXP_Report", 2);
                    int ColIndex = 1;
                    foreach (var item in typeof(ExportReportModel).GetProperties())
                    {
                        if (item.Name == "LastDateString" || item.Name == "CurrentDateString" || item.Name == "Top1_Fail_Item" || item.Name == "Top2_Fail_Item" || item.Name == "Top3_Fail_Item" || item.Name == "Fail_Rate_Old" || item.Name == "Fail_Rate_New" || item.Name == "Top_Fail_Item" || item.Name == "Action_FailItem")
                            continue;
                        else if (item.Name == "First_Yield_Rate_Old")
                            ws1.Cell(1, ColIndex++).Value = item.Name + " %" + "(" + Content.First().LastDateString + ")";
                        else if (item.Name == "First_Yield_Rate_New")
                            ws1.Cell(1, ColIndex++).Value = item.Name + " %" + "(" + Content.First().CurrentDateString + ")";
                        else if (item.Name == "Fail_Rate_Old")
                            ws1.Cell(1, ColIndex++).Value = item.Name + " %";
                        else if (item.Name == "Fail_Rate_New")
                            ws1.Cell(1, ColIndex++).Value = item.Name + " %";
                        else if (item.Name == "UPH_Achievement_Rate_Old")
                            ws1.Cell(1, ColIndex++).Value = item.Name + " %";
                        else if (item.Name == "UPH_Achievement_Rate_New")
                            ws1.Cell(1, ColIndex++).Value = item.Name + " %";
                        else if (item.Name == "Action_FailItem")
                            ws1.Cell(1, ColIndex++).Value = item.Name + " (Comparison with previous)";
                        else
                            ws1.Cell(1, ColIndex++).Value = item.Name;
                    }
                    ws1.Range("A1:O1").Style.Fill.SetBackgroundColor(XLColor.LightSkyBlue);

                    ws1.Cell(2, 1).InsertData(data);

                    //MERGE
                    //https://github.com/ClosedXML/ClosedXML/wiki/Merging-Cells

                    var fddd = "testc";

                    for (var col = 65; col <= 84; col++)
                    {
                        if (col == 67)
                            fddd = "ertre";

                        var ColChar = (char)col;

                        var row = 2;
                        var StartCell = $@"{ColChar}{row.ToString()}";
                        var StartCellString = ws1.Cell(StartCell).Value;
                        var StartRow = 2;
                        if (StartCell == "D2" && StartCellString.ToString() != "" && StartCellString.ToString() != "NaN" && ws1.Cell("E2").Value.ToString() != "" && ws1.Cell("E2").Value.ToString() != "NaN")
                        {
                            if (double.Parse(ws1.Cell("D2").Value.ToString()) > double.Parse(ws1.Cell("E2").Value.ToString()))
                                ws1.Cell("E2").Style.Font.FontColor = XLColor.Red;
                            else
                                ws1.Cell("E2").Style.Font.FontColor = XLColor.Blue;
                        }

                        row++;
                        while (row <= 2 + Content.Count)
                        {
                            var NowCell = $@"{ColChar}{row.ToString()}";
                            var NowCellString = ws1.Cell(NowCell).Value.ToString();

                            if (NowCellString == StartCellString.ToString() && col <= 73 && NowCellString != "")
                            {
                                if (col == 65)
                                    ws1.Cell(NowCell).Value = null;
                                if (col == 66)
                                    ws1.Cell(NowCell).Value = null;
                                fddd = ws1.Cell($@"B{row}").Value.ToString();
                                if (col != 66 && ws1.Cell($@"B{row}").Value.ToString() == "")
                                    ws1.Cell(NowCell).Value = null;
                            }
                            else
                            {
                                if (ws1.Cell($@"{ColChar}{row - 1}").Value.ToString() == "" && col <= 73)
                                {
                                    if (col == 66)
                                        ws1.Range($@"{StartCell}:{ColChar}{row - 1}").Column(1).Merge();

                                    if (col == 65)
                                        ws1.Range($@"{StartCell}:{ColChar}{row - 1}").Column(1).Merge();

                                    if (col != 66 && ws1.Cell($@"B{row - 1}").Value.ToString() == "")
                                        ws1.Range($@"{StartCell}:{ColChar}{row - 1}").Column(1).Merge();
                                }
                                StartCell = $@"{ColChar}{row.ToString()}";
                                StartCellString = ws1.Cell(NowCell).Value;
                                StartRow = row;
                            }

                            if (ColChar == 'D' && NowCellString != "NaN" && NowCellString != "" && ws1.Cell($@"{(char)(col + 1)}{row}").Value.ToString() != "NaN" && ws1.Cell($@"{(char)(col + 1)}{row}").Value.ToString() != "")
                            {
                                var OldValue = double.Parse(NowCellString.Replace("%", String.Empty));
                                var NowValue = double.Parse((ws1.Cell($@"{(char)(col + 1)}{row}").Value.ToString().Replace("%", String.Empty)));

                                if (NowValue >= OldValue)
                                {
                                    ws1.Cell($@"{(char)(col + 1)}{row}").Style.Font.FontColor = XLColor.Blue;
                                }
                                else
                                {
                                    ws1.Cell($@"{(char)(col + 1)}{row}").Style.Font.FontColor = XLColor.Red;
                                }
                            }

                            if (ColChar == 'F' && NowCellString != "NaN" && NowCellString != "" && ws1.Cell($@"{(char)(col + 1)}{row}").Value.ToString() != "NaN" && ws1.Cell($@"{(char)(col + 1)}{row}").Value.ToString() != "")
                            {
                                var OldValue = double.Parse(NowCellString.Replace("%", String.Empty));
                                var NowValue = double.Parse((ws1.Cell($@"{(char)(col + 1)}{row}").Value.ToString().Replace("%", String.Empty)));

                                if (NowValue >= OldValue)
                                {
                                    ws1.Cell($@"{(char)(col + 1)}{row}").Style.Font.FontColor = XLColor.Blue;
                                }
                                else
                                {
                                    ws1.Cell($@"{(char)(col + 1)}{row}").Style.Font.FontColor = XLColor.Red;
                                }
                            }

                            row++;
                        }
                    }

                    ws1.Range($@"A1:{(char)84}{(Content.Count + 1).ToString()}").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    ws1.Columns().AdjustToContents(); // Adjust column width
                    ws1.Rows().AdjustToContents();  // Adjust row heights

                    return book;
                }
                catch (Exception e)
                {
                    return book;
                }
            }
        }

        public ActionResult cusDataExport(string Title, string org, string startdate, string enddate)
        {
            var ITMXP_Content = DataHandlerFunctions.GetFYRByDateRange(startdate, enddate);
            var NonITMXP_Content = DataHandlerFunctions.GetFYRByDateRangeNonITM(startdate, enddate, false).Where(x => x.Org == org).ToList();

            if (org != "All")
            {
                ITMXP_Content = DataHandlerFunctions.GetFYRByDateAndOrg(startdate, enddate, org);
            }

            //ClosedXML的用法 先new一個Excel Workbook
            using (XLWorkbook wb = new XLWorkbook())
            {
                //取得我要塞入Excel內的資料
                var data_ITMXP = ITMXP_Content.Select(c => new { c.id, c.Org, c.ItemNameType, c.Description, c.TestType, c.TestType2, c.Total, c.Pass, c.Fail, c.D_Total, c.D_Pass, c.D_Fail, c.Pass_Rate, c.Fail_Rate, c.Retry_Rate, c.FYR, c.Avg_Pass_Time, c.Avg_Total_Time, c.TestStation });
                var data_Others = NonITMXP_Content.Select(c => new { c.id, c.Org, c.ItemNameType, c.Description, c.TestType, c.TestType2, c.Total, c.Pass, c.Fail, c.D_Total, c.D_Pass, c.D_Fail, c.Pass_Rate, c.Fail_Rate, c.Retry_Rate, c.FYR, c.Avg_Pass_Time, c.Avg_Total_Time, c.TestStation });

                //一個wrokbook內至少會有一個worksheet,並將資料Insert至這個位於A1這個位置上
                var ws_ITMXP = wb.Worksheets.Add("ITMXP", 1);
                //注意官方文件上說明,如果是要塞入Query後的資料該資料一定要變成是data.AsEnumerable()
                //但是我查詢出來的資料剛好是IQueryable ,其中IQueryable有繼承IEnumerable 所以不需要特別寫

                int ColIndex = 1;
                foreach (var item in typeof(PTEWEB_ItemNameType_ByDaily).GetProperties())
                {
                    if (item.Name != "Source" && item.Name != "Date")
                        ws_ITMXP.Cell(1, ColIndex++).Value = item.Name;
                }

                ws_ITMXP.Cell(2, 1).InsertData(data_ITMXP);

                var ws_Others = wb.Worksheets.Add("NonITMXP", 2);

                ColIndex = 1;
                foreach (var item in typeof(PTEWEB_ItemNameType_ByDaily).GetProperties())
                {
                    if (item.Name != "Source" && item.Name != "Date")
                        ws_Others.Cell(1, ColIndex++).Value = item.Name;
                }

                ws_Others.Cell(2, 1).InsertData(data_Others);

                //因為是用Query的方式,這個地方要用串流的方式來存檔
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    wb.SaveAs(memoryStream);
                    //請注意 一定要加入這行,不然Excel會是空檔
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    //注意Excel的ContentType,是要用這個"application/vnd.ms-excel" 不曉得為什麼網路上有的Excel ContentType超長,xlsx會錯 xls反而不會
                    return this.File(memoryStream.ToArray(), "application/vnd.ms-excel", $@"{org}_{Title}.xlsx");
                }
            }
        }

        public ActionResult DashBoardDynamic()
        {
            return View();
        }
    }

    public class DashBoardConfig
    {
        public string Sdate { get; set; }
        public string Edate { get; set; }
        public string Org { get; set; }
        public string Owner { get; set; }

        public DashBoardConfig(string _sdate, object tempSdate, string _edate, object tempEdate, string _org, string _owner)
        {
            var owner = 0;
            if (_owner == null || _owner == string.Empty)
                _owner = "0";
            var OwnerTryParseResult = int.TryParse(_owner, out owner);
            Owner = owner.ToString();

            if (_org == null || _org == "")
                _org = "T1";
            Org = _org;

            if (_sdate == null || _edate == null || _sdate == "" || _edate == "")
            {
                if (tempSdate != null || tempEdate != null)
                {
                    if (_sdate == "" || _edate == "")
                    {
                        _sdate = tempSdate.ToString();
                        _edate = tempEdate.ToString();
                    }
                }
                else
                {
                    _sdate = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd");
                    _edate = DateTime.Now.ToString("yyyy-MM-dd");
                }
            }

            Sdate = _sdate;
            Edate = _edate;
        }
    }
}