using ClosedXML.Excel;
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
    public class RealTimeController : Controller
    {
        public ActionResult RealTimeUPHPerformance(string workdate) //0:Now;1:Last
        {
            var AllDay = new List<Estimate_UPH>();

            var org = Request.Form["DropDownList_org"];

            var SelectListDict = PageFunction.InitialSelectItem(org);

            var actiondate = PageFunction.InitDateHandler(workdate);

            AllDay = RealTimeModel.GetAllRealContent(org, actiondate);

            ViewBag.OrgList = SelectListDict["org"];
            if (org == "" || org == null || AllDay.Count == 0)
                return View(AllDay);

            var ProductIndex = (from item in AllDay
                                select item.productname).Distinct().ToList();

            var ItemMapList = new List<ItemMappingList>();

            ProductIndex.ForEach(item =>
            {
                ItemMapList.Add(new ItemMappingList()
                {
                    ProductName = item,
                    ItemNametype = (from name in AllDay
                                    where name.productname == item
                                    select name.itemnametype).First()
                });
            });

            AllDay = PageFunction.ProcessAndUninDayNightRawData(AllDay, ItemMapList).OrderBy(item => item.GapPercent).ToList();

            var Title_Delta = DataHandlerFunctions.ProcessDeltaInfo(org, actiondate, AllDay, ItemMapList);

            var OutlineData_Dict = DataHandlerFunctions.ProcessOutlineAtRealUPHByShift(AllDay);

            ViewBag.TableDict = PageFunction.InitDeltaDict();

            ViewBag.TitleDelta = Title_Delta;
            ViewBag.workdate = actiondate.Replace("-", "/");
            ViewBag.DictOutline = OutlineData_Dict;
            ViewBag.UPH = 999;
            ViewBag.org = org;
            TempData["Org"] = org;
            TempData.Keep("Org");
            return View(AllDay);
        }

        public ActionResult RealUPHDailyHomePage()
        {
            var Org = Request.Form["DropDownList_org"];
            var SelectListDict = PageFunction.InitialSelectItem(Org);
            ViewBag.OrgList = SelectListDict["org"];

            var FinalTable = new UPHTrend_Table();

            var DictDailyData = new Dictionary<string, List<DailyUPH>>();
            var BoxChartValueList = new List<PTE_Web.Class.GPNSpare>();
            var ListOfDailyTrend = new List<DailyOutputTrend>();
            var OrgWeeklyRawData = DataHandlerFunctions.GetDailyUPHTable(Org);

            var GroupByItemNameTypeData = GetItemNameDailyUPH(OrgWeeklyRawData);

            var DateList = (from item in OrgWeeklyRawData
                            select item.Date).Distinct().ToList();

            foreach (var item in DateList)
            {
                var GapList = (from raw in OrgWeeklyRawData
                               where raw.Date == item
                               select ((double)raw.EstimateUPH / (double)raw.UPH)).ToList();
                var DailyLIst = (from raw in OrgWeeklyRawData
                                 where raw.Date == item
                                 select raw).ToList();
                BoxChartValueList.Add(new Class.GPNSpare { GPN = item.ToString("yyyy-MM-dd"), Spare = GapList });
                ListOfDailyTrend.Add(new DailyOutputTrend()
                {
                    date = item.ToString("yyyy-MM-dd"),
                    Output = DailyLIst.Sum(x => x.RealOutput),
                    totalstation = GapList.Count(),
                    UPHAchieveRate = float.Parse((((float)GapList.Where(gap => gap < 1).Count() / (float)GapList.Count()) * 100).ToString("f2"))
                });
                ListOfDailyTrend = ListOfDailyTrend.OrderBy(x => DateTime.Parse(x.date)).ToList();
            }

            var PreviousdayCompletionRate = 0.0;
            foreach (var item in ListOfDailyTrend)
            {
                if (item.date == ListOfDailyTrend.First().date)
                {
                    item.delta = 0;
                    PreviousdayCompletionRate = item.UPHAchieveRate;
                }
                else
                {
                    item.delta = float.Parse(((item.UPHAchieveRate - PreviousdayCompletionRate) / PreviousdayCompletionRate).ToString("F2")) * 100;
                    PreviousdayCompletionRate = item.UPHAchieveRate;
                }
            }

            ViewBag.Org = SelectListDict["org"].Where(x => x.Selected == true).First().Value.ToString();
            ViewBag.Sdate = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            ViewBag.Edate = DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.SpareDailyGpn = new JavaScriptSerializer().Serialize(BoxChartValueList);
            FinalTable.DailyTrendTable = ListOfDailyTrend;
            FinalTable.GroupItemNameTable = GroupByItemNameTypeData ?? new List<Estimate_UPH>();

            ViewBag.TrendChart = new JavaScriptSerializer().Serialize(
                    (from item in FinalTable.DailyTrendTable
                     select new
                     {
                         Date = item.date,
                         RealOutput = item.Output,
                         UPHAchieveRate = item.UPHAchieveRate
                     }).ToList()).ToString();

            ViewBag.TotalUnits_ATE = FinalTable.GroupItemNameTable.Where(x => x.table == "tblcpu").Sum(x => x.RealOutput);
            ViewBag.TotalUnits_FT = FinalTable.GroupItemNameTable.Where(x => x.table == "tblfinal").Sum(x => x.RealOutput);

            ViewBag.JsonScatter = new JavaScriptSerializer().Serialize(
                    (from item in FinalTable.GroupItemNameTable
                     where item.RealOutput > 1000 && item.EstimateUPH < item.UPH
                     select new
                     {
                         description = item.productname,
                         itemnametype = item.itemnametype,
                         realoutput_ate = item.table == "tblcpu" ? item.RealOutput.ToString() : null,
                         estimate_uph_ate = item.table == "tblcpu" ? item.EstimateUPH.ToString() : null,
                         UPHAchieveRate_ate = item.table == "tblcpu" ? (double.Parse((item.EstimateUPH / (double)item.UPH).ToString("F2")) * 100).ToString() : null,
                         realoutput_ft = item.table == "tblfinal" ? item.RealOutput.ToString() : null,
                         estimate_uph_ft = item.table == "tblfinal" ? item.EstimateUPH.ToString() : null,
                         UPHAchieveRate_ft = item.table == "tblfinal" ? (double.Parse((item.EstimateUPH / (double)item.UPH).ToString("F2")) * 100).ToString() : null,
                         color = item.table == "tblfinal" ? "#00ffff" : "#ffd000",
                         uph = item.UPH
                     }).ToList()).ToString();

            return View(FinalTable);
        }

        public List<Estimate_UPH> GetItemNameDailyUPH(List<DailyUPH> AllData)
        {
            try
            {
                var ItemList = AllData.Select(item => item.ItemNameType).Distinct().ToList();
                var Output = new List<Estimate_UPH>();

                foreach (var item in ItemList)
                {
                    var tempItemName = AllData.Where(x => x.ItemNameType == item).ToList();
                    if (tempItemName.Max(x => x.Gap) > 0)
                    {
                        var gap = double.Parse(tempItemName.Max(x => x.EstimateUPH).ToString("F2")) / (double)tempItemName.Max(x => x.UPH);
                        Output.Add(new Estimate_UPH()
                        {
                            itemnametype = item,
                            EstimateUPH = double.Parse(tempItemName.Max(x => x.EstimateUPH).ToString("F2")),
                            RealOutput = tempItemName.Sum(x => x.RealOutput),
                            UPH = tempItemName.Max(x => x.UPH),
                            AvgSpare = double.Parse(tempItemName.Average(x => x.AvgSpare).ToString("F2")),
                            productname = tempItemName.FirstOrDefault().ProductName,
                            GapPercent = double.Parse(gap.ToString("F2")) * 100,
                            color = gap > 1 ? "blue" : "red",
                            table = tempItemName.FirstOrDefault().table.ToString()
                        });
                    }
                }
                return Output.OrderBy(x => x.GapPercent).ToList();
            }
            catch (Exception e)
            {
                return new List<Estimate_UPH>();
            }
        }

        [HttpPost]
        public ActionResult _RealTimeUPHPerformance(string org, string itemnametype, string shift, string date)
        {
            var Itemnmaetype = int.Parse(itemnametype);

            var ShiftId = shift == "Day" ? 0 : 1;

            var workdate = DateTime.Parse(date).ToString("yyyy-MM-dd");

            var sdate = DateTime.Parse(date).AddDays(-60).ToString("yyyy-MM-dd");

            var ItemConfig = DataHandlerFunctions.GetTestConfigByItemNameType(Itemnmaetype);

            var PreviousTestPerformance = DataHandlerFunctions.GetDailyFYR_FR_RTRByItemNameTypeAndOrg(sdate, workdate, Itemnmaetype, org).OrderByDescending(item => DateTime.Parse(item.Date)).First();

            var Result = DataHandlerFunctions.GetRealUPHInfoByItemNameTyoe_Org_Shift(org, Itemnmaetype, ShiftId, workdate);

            var DeltaDict = DataHandlerFunctions.GetDeltaDictByTwoDate(Result, PreviousTestPerformance);

            foreach (var item in Result)
            {
                item.DateTime = DateTime.Parse(DateTime.Today.ToString("yyyy-MM-dd") + $@" {item.TimeIndex}:00:00");
            }

            var chartlist = Result.OrderBy(item => item.TimeIndex).ToList();

            ViewBag.chartlist = new JavaScriptSerializer().Serialize(chartlist.GetRange(0, chartlist.Count)).ToString();

            var uph = Result.First().UPH;

            ViewBag.TableDict = DeltaDict;

            ViewBag.P_itemnametype = itemnametype;
            ViewBag.P_description = ItemConfig.Description;

            ViewBag.UPH = uph == 0 ? 999 : uph;
            ViewBag.MaxY = uph + 200;
            return PartialView("_RealTimeUPHPerformance");
        }

        public ActionResult cusDataExport(string Title, string org, string shift)
        {
            var AllDay = RealTimeModel.GetAllDayData(org, shift);
            var TotalContent = PageFunction.GetInitialTable(org, AllDay);

            var ProductIndex = (from item in TotalContent
                                select item.productname).Distinct().ToList();

            var ItemMapList = new List<ItemMappingList>();

            ProductIndex.ForEach(item =>
            {
                ItemMapList.Add(new ItemMappingList()
                {
                    ProductName = item,
                    ItemNametype = (from name in TotalContent
                                    where name.productname == item
                                    select name.itemnametype).First()
                });
            });

            TotalContent = PageFunction.UpdateUPHData(TotalContent, ItemMapList, org).OrderBy(item => item.GapPercent).ToList();

            using (XLWorkbook wb = new XLWorkbook())
            {
                var data = TotalContent.Select(c => new { c.itemnametype, c.productname, c.RealOutput, c.EstimateUPH, c.UPH, c.AvgSpare, c.Gap });

                var ws = wb.Worksheets.Add(Title, 1);

                int ColIndex = 1;
                foreach (var item in typeof(Estimate_UPH).GetProperties())
                {
                    if (item.Name != "DateTime" && item.Name != "TimeIndex" && item.Name != "table" && item.Name != "testTimeRange" && item.Name != "GapPercent" && item.Name != "EstimateHours")
                        ws.Cell(1, ColIndex++).Value = item.Name;
                }

                ws.Cell(2, 1).InsertData(data);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    wb.SaveAs(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return this.File(memoryStream.ToArray(), "application/vnd.ms-excel", $@"{org}_{shift}_{Title}.xlsx");
                }
            }
        }
    }

    public class PageFunction
    {
        public static Dictionary<string, List<SelectListItem>> InitialSelectItem(string org)
        {
            var SelectDict = new Dictionary<string, List<SelectListItem>>();
            if (org == "" || org == null)
                org = "T2";

            var OrgList = new List<SelectListItem>();
            OrgList.Add(new SelectListItem { Text = "T1", Value = "T1", Selected = org == "T1" ? true : false });
            OrgList.Add(new SelectListItem { Text = "T2", Value = "T2", Selected = org == "T2" ? true : false });
            OrgList.Add(new SelectListItem { Text = "T3", Value = "T3", Selected = org == "T3" ? true : false });
            OrgList.Add(new SelectListItem { Text = "T5", Value = "T5", Selected = org == "T5" ? true : false });

            SelectDict["org"] = OrgList;
            return SelectDict;
        }

        public static string InitDateHandler(string workdate)
        {
            var date = workdate == null || workdate == "" ? DateTime.Now.ToString("yyyy/MM/dd") : DateTime.Parse(workdate).ToString("yyyy-MM-dd");
            return date;
        }

        public static Dictionary<string, string> InitDeltaDict()
        {
            var Output = new Dictionary<string, string>();
            Output["T_FYR"] = string.Empty;
            Output["P_FYR"] = string.Empty;
            Output["T_Spare"] = string.Empty;
            Output["P_Spare"] = string.Empty;
            Output["T_EstimateUPH"] = string.Empty;
            Output["P_EstimateUPH"] = string.Empty;
            Output["D_FYR"] = string.Empty;
            Output["D_Spare"] = string.Empty;
            Output["D_EstimateUPH"] = string.Empty;
            return Output;
        }

        /// <summary>
        /// Create Initial Table
        /// </summary>
        /// <param name="Org"></param>
        /// <param name="AllDay"></param>
        /// <returns></returns>
        public static List<Estimate_UPH> GetInitialTable(string Org, List<RealTimeUPH_Total> AllDay)
        {
            var TotalContent = new List<Estimate_UPH>();
            foreach (var item in AllDay)
            {
                var Toal_result = new List<Estimate_UPH>();

                foreach (var time_raw in item.ItemNameTypeList)
                {
                    var Item_result = DataHandlerFunctions.GetEstimateUPH(Org, time_raw.ItemNametype, time_raw.Table, item.StartTime);
                    if (Item_result.Count != 0)
                    {
                        if (Item_result.First().productname == "NULL") continue;

                        Item_result.First().DateTime = DateTime.Parse(item.Date.ToShortDateString() + $@" {Item_result.First().TimeIndex}:00:00");
                        Toal_result.AddRange(Item_result);
                    }
                }
                TotalContent.AddRange(Toal_result);
            }

            return TotalContent;
        }

        /// <summary>
        /// Update UPH into Final Table
        /// </summary>
        /// <param name="RawData"></param>
        /// <param name="ItemNameMapping"></param>
        /// <param name="Org"></param>
        /// <returns></returns>
        public static List<Estimate_UPH> UpdateUPHData(List<Estimate_UPH> RawData, List<ItemMappingList> ItemNameMapping, string Org)
        {
            try
            {
                foreach (var item in ItemNameMapping)
                {
                    var Raw = (from raw in RawData
                               where raw.itemnametype == item.ItemNametype
                               select raw).First();

                    var gpn = DataHandlerFunctions.GetGPNByItemNameType(Raw.itemnametype, Org, Raw.table, Raw.DateTime);

                    var uph = DataHandlerFunctions.GetProductionMapInfo(gpn, Org, Raw.itemnametype);

                    RawData.ForEach(raw =>
                    {
                        if (raw.itemnametype == Raw.itemnametype)
                            raw.UPH = uph == 0 ? 999 : uph;
                    });
                }

                return ProcessAndUninDayNightRawData(RawData, ItemNameMapping);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static List<Estimate_UPH> ProcessAndUninDayNightRawData(List<Estimate_UPH> AllData, List<ItemMappingList> ItemNameMapping)
        {
            try
            {
                var PreDayRawData = AllData.Where(item => item.shiftid == 0).ToList();
                var PreNightRawData = AllData.Where(item => item.shiftid == 1).ToList();

                var DayRawData = ProcessAllDataGroupByItemNameTypeByShift(PreDayRawData, ItemNameMapping, 0);
                var NightRawData = ProcessAllDataGroupByItemNameTypeByShift(PreNightRawData, ItemNameMapping, 1);
                var Output = new List<Estimate_UPH>();
                Output.AddRange(DayRawData);
                Output.AddRange(NightRawData);
                return Output;
            }
            catch (Exception e)
            {
                return new List<Estimate_UPH>();
            }
        }

        /// <summary>
        /// Calculation the Sum and Average Data
        /// </summary>
        /// <param name="RawData"></param>
        /// <param name="ItemNameMapping"></param>
        /// <returns></returns>
        public static List<Estimate_UPH> ProcessAllDataGroupByItemNameTypeByShift(List<Estimate_UPH> RawData, List<ItemMappingList> ItemNameMapping, int shiftid)
        {
            try
            {
                var Output = new List<Estimate_UPH>();

                foreach (var item in ItemNameMapping)
                {
                    double AverageUPH = 0;
                    var uph = 0;
                    var Sum = 0;
                    var Spare = 0.0;
                    var Gap = 0.0;
                    var Percent = 0.0;
                    var FYR = 0.0;

                    var temp = (from raw in RawData
                                where raw.itemnametype == item.ItemNametype
                                select raw).ToList();

                    if (temp.Count == 0) continue;

                    var tempOne = temp.FirstOrDefault();
                    AverageUPH = temp.Max(raw => raw.EstimateUPH);

                    Sum = temp.Sum(raw => raw.RealOutput);
                    uph = temp.Min(raw => raw.UPH);

                    temp.ForEach(row =>
                    {
                        FYR += ((double)row.RealOutput / (double)Sum) * row.FYR;
                    });

                    Spare = temp.Average(raw => raw.AvgSpare);

                    Gap = double.Parse(Convert.ToString(Math.Round((3600 / (uph / (AverageUPH / (3600 / Spare))) - Spare), 2)));

                    Percent = uph == 999 || Gap / Spare > 0.3 || AverageUPH >= uph ? 100 : Gap / Spare;

                    Output.Add(new Estimate_UPH() { itemnametype = tempOne.itemnametype, EstimateUPH = double.Parse(Convert.ToString(Math.Round(AverageUPH, 0))), RealOutput = Sum, productname = tempOne.productname, UPH = uph, GapPercent = Percent, Gap = Gap, AvgSpare = double.Parse(Convert.ToString(Math.Round(Spare, 2))), FYR = Math.Round(FYR, 2) * 100, shiftid = shiftid, shift = shiftid == 0 ? "Day" : "Night" });
                }
                return Output;
            }
            catch (Exception e)
            {
                return RawData;
            }
        }
    }

    //test code
    public class RealTimeFixtureCorrelation
    {
        public string ProductName { get; set; }
        public int ItemNameType { get; set; }
        public List<FailItemCorrelation_Fixture> FailItemTop3 { get; set; }
        public string ItemCorrelation { get; set; }
    }

    public class RealTimeUPH_Total
    {
        public string Org { get; set; }
        public DateTime Date { get; set; }
        public DateTime StartTime { get; set; }
        public List<ItemNameType_Table> ItemNameTypeList { get; set; }
    }
}