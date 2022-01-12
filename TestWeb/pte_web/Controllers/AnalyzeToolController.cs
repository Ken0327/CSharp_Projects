using ClosedXML.Excel;
using Newtonsoft.Json;
using NLog;
using PTE_Web.Connections;
using PTE_Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace PTE_Web.Controllers
{
    public class AnalyzeToolController : Controller
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger _PTElogger = LogManager.GetLogger("PTE_WEBLogger");

        // GET: AnalyzeTool
        public ActionResult Index()
        {
            return View();
        }

        [ValidateInput(false)]
        public ActionResult AnalyzeItemNameType(string org, string itemnametype, string startdate, string enddate, string source)
        {
            _logger.Trace($"TEST_DashBoard_{DateTime.Now.ToString()}");
            _PTElogger.Debug($"TEST_DashBoard_{DateTime.Now.ToString()}");
            var AllModels = new AnalyzePageModels();
            if (startdate == null || enddate == null || startdate == "" || enddate == "" || startdate == null || enddate == null)
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
            ViewBag.TitleDescription = DataHandlerFunctions.GetTestConfigByItemNameType(PageConfig.ItemNameType).Description;
            TempData.Keep("Source");
            TempData.Keep("Org");
            TempData.Keep("ItemNameType");
            TempData.Keep("StartDate");
            TempData.Keep("EndDate");
            var FYRAll = DataHandlerFunctions.DataCatcherByDateRange(PageConfig.SdateUTC, PageConfig.EdateUTC);
            var FYRAllByItemNameType = (from raw in FYRAll
                                        where raw.ItemNameType == PageConfig.ItemNameType && raw.Org == PageConfig.Org
                                        select raw).ToList();
            var FYRDaltaTitleTable = DeltaIModel.GetItemNameTitleDeltaTable(FYRAllByItemNameType, startdate, enddate);
            PageConfig.Source = DataHandlerFunctions.GetDBByItemNameType(PageConfig.ItemNameType) == "TblCpu" ? "ATE" : "FT";

            var AllFailItemTable = FailItemModel.GetFailItemTableByItemName_Org(PageConfig.Org, PageConfig.ItemNameType, PageConfig.SdateUTC, PageConfig.EdateUTC, PageConfig.Source);

            DataTable SpareTbl = new DataTable();

            AllModels.ItemSapreDatas = FailItemModel.GetFailItemSpareAndDelay(PageConfig.Org, PageConfig.ItemNameType, PageConfig.SdateUTC, PageConfig.EdateUTC, PageConfig.Source, out SpareTbl);

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

            var DailyFYRTableByOrg = DataHandlerFunctions.GetDailyFYR_FR_RTRByItemNameTypeAndOrg(PageConfig.SdateUTC, PageConfig.EdateUTC, PageConfig.ItemNameType, PageConfig.Org);

            ViewBag.TopFailItemJsonList = new JavaScriptSerializer().Serialize(AllFailItemTable.GetRange(0, AllFailItemTable.Count >= 5 ? 5 : AllFailItemTable.Count)).ToString();

            ViewBag.TopSpareItemJsonList = new JavaScriptSerializer().Serialize(SpareData.GetRange(0, SpareData.Count >= 5 ? 5 : SpareData.Count)).ToString();

            ViewBag.DailyFYRList = new JavaScriptSerializer().Serialize(DailyFYRTableByOrg.GetRange(0, DailyFYRTableByOrg.Count)).ToString();
            ViewBag.ItemUPH = DataHandlerFunctions.GetUPHByItemNameType(PageConfig.Org, PageConfig.ItemNameType);

            AllFailItemTable.ForEach(item =>
            {
                if (item.Order <= 10)
                {
                    item.FixtureRelation = DataHandlerFunctions.GetFixtureReleation(PageConfig, item.FailItem);
                }
            });
            AllModels.FYRTable = FYRDaltaTitleTable;
            AllModels.DeltaTable = DeltaIModel.GetItemNameDeltaTable(FYRAllByItemNameType);
            AllModels.DeltaTable.Reverse();
            AllModels.FailItemModel = AllFailItemTable;
            return View(AllModels);
        }

        [HttpPost]
        public ActionResult GetIssueByItemNameType(int ItemNameType, string date)
        {
            var AllModels = new AnalyzePageModels();
            AllModels.PTEWEB_Issues = DataHandlerFunctions.GetIssues(ItemNameType, date);
            return Json(AllModels.PTEWEB_Issues, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Get_CPKData(string org, string itemnametype, string startdate, string enddate, string source, int DbIndex, double specMin, double specMax)
        {
            var AllModels = new AnalyzePageModels();
            var PageConfig = new InitialParameter(org, itemnametype, startdate, enddate);
            TempData["StartDate"] = startdate;
            TempData["EndDate"] = enddate;
            TempData["ItemNameType"] = PageConfig.ItemNameType;
            TempData["Org"] = PageConfig.Org;
            TempData["Source"] = source;

            AllModels.CpkModel = DataHandlerFunctions.GetItemNameTypeCpkModel(PageConfig.ItemNameType, source, PageConfig.Org, PageConfig.SdateUTC, PageConfig.EdateUTC, DbIndex, specMin, specMax);

            return new JsonResult()
            {
                Data = AllModels.CpkModel,
                MaxJsonLength = int.MaxValue,/*重點在這行*/
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult RealTimeFixtureMonitor()
        {
            var org = Request.Form["DropDownList_org"];

            if (org == "" || org == null)
                org = "T2";

            var SelectListDict = PageFunction.InitialSelectItem(org);

            ViewBag.OrgList = SelectListDict["org"];

            var QueryDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).AddHours(-8);

            var RealtimeFixtureTable = new List<RealTimeFixtureCorrelation>();

            var ItemInfoList = GetAllItemInfoList(org, QueryDate);

            foreach (var item in ItemInfoList)
            {
                var failitem = DataHandlerFunctions.GetFailDetailInfo(item.Itemnametype, org, QueryDate.AddHours(-3).ToString("yyyy-MM-dd HH:mm:ss"), QueryDate.ToString("yyyy-MM-dd HH:mm:ss"));
                var FixtureCorrTable = new List<FailItemCorrelation_Fixture>();

                var CorrFlag = "NO";
                var ItemNameMappingDist = DataHandlerFunctions.GetItemByItemNameType(item.Itemnametype);

                failitem = failitem.OrderByDescending(x => x.FailCount).Take(3).ToList();
                failitem.ForEach(fitem =>
                {
                    var IniParameter = new InitialParameter(org, item.Itemnametype.ToString(), QueryDate.AddHours(-3).ToString("yyyy-MM-dd HH:mm:ss"), QueryDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    var ItemFixtureList = FailItemModel.GetFailItemCorrelationByCategory("Fixture", org, item.Itemnametype, QueryDate.AddHours(-3).ToString("yyyy-MM-dd HH:mm:ss"), QueryDate.ToString("yyyy-MM-dd HH:mm:ss"), fitem.FailItem);
                    if (DataHandlerFunctions.GetFixtureReleation(IniParameter, fitem.FailItem) == "YES")
                    {
                        CorrFlag = "YES";
                    }
                    if (ItemNameMappingDist.Keys.Contains($@"Name{fitem.FailItem}"))
                    {
                        fitem.ItemName = ItemNameMappingDist[$@"Name{fitem.FailItem}"].ToString();
                    }
                    FixtureCorrTable.Add(new FailItemCorrelation_Fixture() { FailItem = fitem.FailItem, ItemName = fitem.ItemName, Correlation = CorrFlag, FixtureList = ItemFixtureList });
                });
                RealtimeFixtureTable.Add(new RealTimeFixtureCorrelation() { ItemNameType = item.Itemnametype, ProductName = item.Description, FailItemTop3 = FixtureCorrTable, ItemCorrelation = CorrFlag });
            }

            var CorrelationCount = RealtimeFixtureTable.FindAll(item => item.ItemCorrelation == "YES").ToList();
            //GetTestConfigByItemNameType
            return View();
        }

        public List<TestItemConfig> GetAllItemInfoList(string org, DateTime QueryDate)
        {
            var ItemNameTypeList_Cpu = DataHandlerFunctions.GetItemNameTypeByHour(org, QueryDate, "tblcpu", 3);
            var ItemNameTypeList_final = DataHandlerFunctions.GetItemNameTypeByHour(org, QueryDate, "tblfinal", 3);
            var TotalList = new List<ItemNameType_Table>();
            TotalList.AddRange(ItemNameTypeList_Cpu);
            TotalList.AddRange(ItemNameTypeList_final);
            var ItemInfoList = new List<TestItemConfig>();

            TotalList.ForEach(item =>
            {
                var temprow = DataHandlerFunctions.GetTestConfigByItemNameType(item.ItemNametype);
                if (temprow != null)
                    ItemInfoList.Add(temprow);
            });

            return ItemInfoList;
        }

        public ActionResult CorrelationWindow(int failitem, string itemdescript, string type, double spec_max, double spec_min)
        {
            var startdate = string.Empty;
            var enddate = string.Empty;
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
                TempData["EndDate"] = enddate;
                TempData["StartDate"] = startdate;
            }

            var ItemCorrelationDict = new List<FailCorrelation>();
            var CorrRawData_ID = new Dictionary<int, string>();
            var CorrRawData_Data = new Dictionary<int, string>();
            var CorrDataMax = new Dictionary<int, double>();
            var CorrDataMin = new Dictionary<int, double>();
            TempData["Failitem"] = failitem.ToString();
            TempData["Type"] = type;
            TempData["Spec_Max"] = spec_max.ToString();
            TempData["Spec_Min"] = spec_min.ToString();
            TempData.Keep("Org");
            TempData.Keep("ItemNameType");
            TempData.Keep("StartDate");
            TempData.Keep("EndDate");

            var PageConfig = new InitialParameter(TempData["Org"].ToString(), TempData["ItemNameType"].ToString(), TempData["StartDate"].ToString(), TempData["EndDate"].ToString());

            ViewBag.ItemName = itemdescript;
            ViewBag.Type = type;
            ViewBag.FailItem = failitem;
            ViewBag.SpecMax = spec_max;
            ViewBag.SpecMin = spec_min;
            ViewBag.Org = PageConfig.Org;
            ViewBag.itemNameType = PageConfig.ItemNameType;
            ViewBag.St = PageConfig.SdateUTC;
            ViewBag.Et = PageConfig.EdateUTC;
            ViewBag.Source = PageConfig.Source;

            ItemCorrelationDict = FailItemModel.GetFailItemCorrelationByCategory(type, PageConfig.Org, PageConfig.ItemNameType, PageConfig.SdateUTC, PageConfig.EdateUTC, failitem);
            var PieDataList = FailItemModel.GetCorrelationPieData(ItemCorrelationDict, type);
            ViewBag.PieDataList = new JavaScriptSerializer().Serialize(PieDataList.GetRange(0, PieDataList.Count));
            ViewBag.PageConfig = PageConfig;
            var ThisModel = new FailItemCorrelationPageModels();
            ThisModel.ThisCorrelationsTable = ItemCorrelationDict;
            var CorrRawData = FailItemModel.GetCorrRawDataByGroupID(type, failitem, ItemCorrelationDict, PageConfig.SdateUTC, PageConfig.EdateUTC, PageConfig.ItemNameType, PageConfig.Org);
            ViewBag.SubGraphCounts = CorrRawData.Count();
            CorrRawData.ForEach(item =>
            {
                if (item.Value.Count != 0)
                {
                    CorrRawData_ID[CorrRawData_ID.Count] = item.Category;
                    CorrRawData_Data[CorrRawData_Data.Count] = string.Join(",", item.Value.ToArray());
                    if (spec_max != -999)
                    {
                        CorrDataMax[CorrDataMax.Count] = Math.Round(item.Value.Max(), 0) > spec_max ? Math.Round(item.Value.Max(), 0) : spec_max;
                        CorrDataMin[CorrDataMin.Count] = Math.Round(item.Value.Min(), 0) < spec_min ? Math.Round(item.Value.Min(), 0) : spec_min;
                    }
                    else
                    {
                        CorrDataMax[CorrDataMax.Count] = Math.Round(item.Value.Max(), 0);
                        CorrDataMin[CorrDataMin.Count] = Math.Round(item.Value.Min(), 0);
                    }
                }
            });

            ViewBag.CorrRawData_ID = CorrRawData_ID;

            ViewBag.CorrRawData_Data = CorrRawData_Data;

            ViewBag.MaxValue = CorrDataMax;

            ViewBag.MinValue = CorrDataMin;

            return View(ThisModel);
        }

        [HttpPost]
        public ActionResult CorrelationRawData(string org, string itemnametype, string startdate, string enddate, string source, string itemDescription, int DbIndex, double specMin, double specMax, string GroupString)
        {
            var TableModel = new List<TypeRawData_Correlation>();
            var AllModels = new AnalyzePageModels();
            TempData.Keep("Type");
            var type = TempData["Type"].ToString();
            AllModels.CpkModel = DataHandlerFunctions.GetItemNameTypeCpkModel_CorrelationType(int.Parse(itemnametype), type, GroupString, org, startdate, enddate, DbIndex, specMin, specMax);
            AllModels.CpkModel.CpkTable.First().ItemDescription = itemDescription;
            return new JsonResult()
            {
                Data = AllModels.CpkModel,
                MaxJsonLength = int.MaxValue,/*重點在這行*/
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpPost]
        public ActionResult GetNewSpecData(string PassStr, string FailStr, string fmin, string fmax, double Low, double Upper)
        {
            List<double> PassDatas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<double>>(PassStr);
            List<double> FailDatas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<double>>(FailStr);
            List<double> TestDatas = new List<double>();
            TestDatas.AddRange(PassDatas);
            TestDatas.AddRange(FailDatas);
            Class.NewSpecHistogramData FilterData = new Class.NewSpecHistogramData();
            FilterData.PassDatas = new List<double>();
            FilterData.FailDatas = new List<double>();            
            
            if (fmin != string.Empty || fmax != string.Empty)
            {
                if(fmin==string.Empty)
                    TestDatas = TestDatas.Where(x =>x <= double.Parse(fmax)).Select(x => x).ToList();//filter data
                else if (fmax==string.Empty)
                    TestDatas = TestDatas.Where(x => x >= double.Parse(fmin)).Select(x => x).ToList();//filter data
                else
                    TestDatas = TestDatas.Where(x => x >= double.Parse(fmin) && x <= double.Parse(fmax)).Select(x => x).ToList();//filter data
            }

            FilterData.AVG = TestDatas.Average();
            FilterData.STD = Class.cPk.StandardDeviation(TestDatas);

            FilterData.PassDatas = TestDatas.Where(x => x >= Low && x <= Upper).Select(x => x).ToList();

            FilterData.FailDatas = TestDatas.Where(x => x > Upper || x < Low).Select(x => x).ToList();

            return Json(FilterData, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetNewRangeData(string PassStr, string FailStr, string Min, string Max)
        {
            List<double> PassDatas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<double>>(PassStr);
            List<double> FailDatas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<double>>(FailStr);
            List<double> TestDatas = new List<double>();
            TestDatas.AddRange(PassDatas);
            TestDatas.AddRange(FailDatas);
            Class.NewSpecHistogramData FilterData = new Class.NewSpecHistogramData();
            FilterData.PassDatas = PassDatas;
            FilterData.FailDatas = FailDatas;
            FilterData.lsl = TestDatas.Min();
            FilterData.usl = TestDatas.Max();

            if (Min != string.Empty || Max != string.Empty)
            {
                if (Min==string.Empty)
                {
                    FilterData.usl = double.Parse(Max);
                    FilterData.PassDatas = TestDatas.Where(x => x <= FilterData.usl).Select(x => x).ToList();
                    FilterData.FailDatas = TestDatas.Where(x => x > FilterData.usl).Select(x => x).ToList();
                }
                else if(Max==string.Empty)
                {
                    FilterData.lsl = double.Parse(Min);
                    FilterData.PassDatas = TestDatas.Where(x => x >= FilterData.lsl).Select(x => x).ToList();
                    FilterData.FailDatas = TestDatas.Where(x => x < FilterData.lsl).Select(x => x).ToList();                    
                }
                else
                {
                    FilterData.usl = double.Parse(Max);
                    FilterData.lsl = double.Parse(Min);
                    FilterData.PassDatas = TestDatas.Where(x => x >= FilterData.lsl && x <= FilterData.usl).Select(x => x).ToList();
                    FilterData.FailDatas = TestDatas.Where(x => x > FilterData.usl || x < FilterData.lsl).Select(x => x).ToList();
                }
            }

            return Json(FilterData, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetMonthCpkTrendResults(string Org, string Source, int ItemNameType, int DBIndex, double specMin, double specMax, string st, string et)
        {
            string TargetTestTable = "";
            switch (Source)
            {
                case "ATE":
                    TargetTestTable = "TblCPU";
                    break;

                case "FT":
                    TargetTestTable = "TblFinal";
                    break;
            }
            string SQL = "SELECT Item" + DBIndex + " ,Result,FailItem,convert(varchar(10), tdatetime,120)as TestDate FROM " + TargetTestTable + " WHERE tdatetime>='" + st + "' and tdatetime<'" + et + "' and ItemNameType='" + ItemNameType + "' and (Item" + DBIndex + "<>0 and Item" + DBIndex + "st<>2)    order by tdatetime asc";
            DataTable MonthTbl = DataHandlerFunctions.GetOrgTable(Org, SQL);
            Class.MonthCpkData monthCpkData = DataHandlerFunctions.GetMonthCpkData(MonthTbl, specMin, specMax, "Item" + DBIndex, st, et, TargetTestTable);
            return Json(monthCpkData, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetDailyInfo(int ItemNameType, string Date, string Org, string Source)
        {
            List<PTE_Web.Class.DailyFYRInfo> List_FYR = new List<Class.DailyFYRInfo>();
            List_FYR = DataHandlerFunctions.Get_DailyFYRInfo(ItemNameType, Date, Org, Source);

            return Json(List_FYR, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Get_CPKRawData(string org, string itemnametype, string startdate, string enddate, string source, string DBIndex, string specMin, string specMax)
        {
            string TargetTestTable = "";
            switch (source)
            {
                case "ATE":
                    TargetTestTable = "TblCPU";
                    break;

                case "FT":
                    TargetTestTable = "TblFinal";
                    break;
            }
            string SQLCommand = "";

            SQLCommand = "Select '" + (DBIndex) + "' as Item,serialnumber, Item" + (DBIndex) + " as Value,Item" + (DBIndex) + "st as Status,'" + specMin + "' as SpecMin,'" + specMax + "' as SpecMax,tdatetime,result FROM " + TargetTestTable +
                         " WHERE Item" + (DBIndex) + "!= '0' and Item" + (DBIndex) + "st !=2 AND " + TargetTestTable + ".ItemNameType = '" + itemnametype + "' and tdatetime>='" + startdate + "' and tdatetime<'" + enddate + "'";

            DataTable _data = DataHandlerFunctions.GetOrgTable(org, SQLCommand);
            FileContentResult robj;
            _data.TableName = "Emptbl";
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(_data);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    var bytesdata = File(stream.ToArray(), "application/vnd.ms-excel", "RawData.xlsx");
                    robj = bytesdata;
                }
            }

            return new JsonResult()
            {
                Data = robj,
                MaxJsonLength = int.MaxValue,/*重點在這行*/
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult cusDataExport(string Title, string org, string itemnametype, string startdate, string enddate, string source)
        {
            var PageConfig = new InitialParameter(org, itemnametype, startdate, enddate);
            PageConfig.Source = DataHandlerFunctions.GetDBByItemNameType(PageConfig.ItemNameType) == "TblCpu" ? "ATE" : "FT";

            var Content = FailItemModel.GetFailItemTableByItemName_Org(PageConfig.Org, PageConfig.ItemNameType, PageConfig.SdateUTC, PageConfig.EdateUTC, PageConfig.Source);

            using (XLWorkbook wb = new XLWorkbook())
            {
                var data = Content.Select(c => new { c.Order, c.FailItem, c.ItemName, c.SpecMin, c.SpecMax, c.FailCount, c.FailRate, c.AccumulatePercent });

                var ws = wb.Worksheets.Add(Title, 1);

                int ColIndex = 1;
                foreach (var item in typeof(FailItemTable).GetProperties())
                {
                    if (item.Name != "ItemNameType" && item.Name != "TotalCount" && item.Name != "StdSpare" && item.Name != "AvgSpare" && item.Name != "FixtureRelation" && item.Name != "FailPercent")
                        ws.Cell(1, ColIndex++).Value = item.Name;
                }

                ws.Cell(2, 1).InsertData(data);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    wb.SaveAs(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return this.File(memoryStream.ToArray(), "application/vnd.ms-excel", $@"{itemnametype}_FailItem.xlsx");
                }
            }
        }

        public ActionResult CorrelationRawDataExport()
        {
            TempData.Keep("Org");
            TempData.Keep("ItemNameType");
            TempData.Keep("StartDate");
            TempData.Keep("EndDate");
            TempData.Keep("Failitem");
            TempData.Keep("Type");
            TempData.Keep("Spec_Max");
            TempData.Keep("Spec_Min");

            var org = TempData["Org"].ToString();
            var itemnametype = int.Parse(TempData["ItemNameType"].ToString());
            var startdate = TempData["StartDate"].ToString();
            var enddate = TempData["EndDate"].ToString();
            var failitem = int.Parse(TempData["Failitem"].ToString());

            var Title = "Fail";
            var PageConfig = new InitialParameter(org, itemnametype.ToString(), startdate, enddate);
            PageConfig.Source = DataHandlerFunctions.GetDBByItemNameType(PageConfig.ItemNameType) == "TblCpu" ? "ATE" : "FT";

            var Content = DataHandlerFunctions.GetAllRowDataAtCorrelation(failitem, startdate, enddate, itemnametype, org);

            using (XLWorkbook wb = new XLWorkbook())
            {
                var data = Content.Select(c => new { c.Serialnumber, c.tDatetime, c.station, c.stationid, c.productname, c.exeinfo, c.NOHGPN, c.username, c.ItemResult, c.ItemStatus });

                var ws = wb.Worksheets.Add(Title, 1);

                int ColIndex = 1;
                foreach (var item in typeof(ItemRawData).GetProperties())
                {
                    ws.Cell(1, ColIndex++).Value = item.Name;
                }

                ws.Cell(2, 1).InsertData(data);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    wb.SaveAs(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return this.File(memoryStream.ToArray(), "application/vnd.ms-excel", $@"{itemnametype}_Item{failitem}_RawData.xlsx");
                }
            }
        }
    }
}