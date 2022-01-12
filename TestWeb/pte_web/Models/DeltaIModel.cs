using PTE_Web.Connections;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;

namespace PTE_Web.Models
{
    public class DeltaIModel
    {
        public static List<TopSpareInfo> TopSpareTableTransferToSpareList(List<PTEWEB_ItemNameType_ByDaily_TOP10_LongCycleTime> Table)
        {
            var SpareList = new List<TopSpareInfo>();
            Table.ForEach(row =>
           {
               if (row.NO1_Item != null)
               {
                   SpareList.Add(new TopSpareInfo { FailItemName = row.NO1_Item, Spare = row.NO1_CycleTime });
               }
               if (row.NO2_Item != null)
               {
                   SpareList.Add(new TopSpareInfo { FailItemName = row.NO2_Item, Spare = row.NO2_CycleTime });
               }
               if (row.NO3_Item != null)
               {
                   SpareList.Add(new TopSpareInfo { FailItemName = row.NO3_Item, Spare = row.NO3_CycleTime });
               }
               if (row.NO4_Item != null)
               {
                   SpareList.Add(new TopSpareInfo { FailItemName = row.NO4_Item, Spare = row.NO4_CycleTime });
               }
               if (row.NO5_Item != null)
               {
                   SpareList.Add(new TopSpareInfo { FailItemName = row.NO5_Item, Spare = row.NO5_CycleTime });
               }
               if (row.NO6_Item != null)
               {
                   SpareList.Add(new TopSpareInfo { FailItemName = row.NO6_Item, Spare = row.NO6_CycleTime });
               }
               if (row.NO7_Item != null)
               {
                   SpareList.Add(new TopSpareInfo { FailItemName = row.NO7_Item, Spare = row.NO7_CycleTime });
               }
               if (row.NO8_Item != null)
               {
                   SpareList.Add(new TopSpareInfo { FailItemName = row.NO8_Item, Spare = row.NO8_CycleTime });
               }
               if (row.NO9_Item != null)
               {
                   SpareList.Add(new TopSpareInfo { FailItemName = row.NO9_Item, Spare = row.NO9_CycleTime });
               }
               if (row.NO10_Item != null)
               {
                   SpareList.Add(new TopSpareInfo { FailItemName = row.NO10_Item, Spare = row.NO10_CycleTime });
               }
           });
            return SpareList;
        }

        public static List<TopFailInfo> TopFailTableTransferToFailInfoList(List<PTEWEB_ItemNameType_ByDaily_TOP10_FailItem> Table)
        {
            var FailList = new List<TopFailInfo>();

            Table.ForEach(row =>
            {
                if (row.No1_Fail_Item != null)
                {
                    FailList.Add(new TopFailInfo { FailItemName = row.No1_Fail_Item, FaiCount = row.No1_Fail_Count });
                }
                if (row.No2_Fail_Item != null)
                {
                    FailList.Add(new TopFailInfo { FailItemName = row.No2_Fail_Item, FaiCount = row.No2_Fail_Count });
                }
                if (row.No3_Fail_Item != null)
                {
                    FailList.Add(new TopFailInfo { FailItemName = row.No3_Fail_Item, FaiCount = row.No3_Fail_Count });
                }
                if (row.No4_Fail_Item != null)
                {
                    FailList.Add(new TopFailInfo { FailItemName = row.No4_Fail_Item, FaiCount = row.No4_Fail_Count });
                }
                if (row.No5_Fail_Item != null)
                {
                    FailList.Add(new TopFailInfo { FailItemName = row.No5_Fail_Item, FaiCount = row.No5_Fail_Count });
                }
                if (row.No6_Fail_Item != null)
                {
                    FailList.Add(new TopFailInfo { FailItemName = row.No6_Fail_Item, FaiCount = row.No6_Fail_Count });
                }
                if (row.No7_Fail_Item != null)
                {
                    FailList.Add(new TopFailInfo { FailItemName = row.No7_Fail_Item, FaiCount = row.No7_Fail_Count });
                }
                if (row.No8_Fail_Item != null)
                {
                    FailList.Add(new TopFailInfo { FailItemName = row.No8_Fail_Item, FaiCount = row.No8_Fail_Count });
                }
                if (row.No9_Fail_Item != null)
                {
                    FailList.Add(new TopFailInfo { FailItemName = row.No9_Fail_Item, FaiCount = row.No9_Fail_Count });
                }
                if (row.No10_Fail_Item != null)
                {
                    FailList.Add(new TopFailInfo { FailItemName = row.No10_Fail_Item, FaiCount = row.No10_Fail_Count });
                }
            });

            return FailList;
        }

        public static Dictionary<string, string> GetCountDeltaByDataRangeAndItemNametype(List<PTEWEB_ItemNameType_ByDaily> Table)
        {
            var OrgList = new List<string>() { "T1", "T2", "T3" };
            var Output = new Dictionary<string, string>();

            var DeltaTable = new List<object>();
            var DailyDelta = new List<ItemNameTypeDeltaInfo>();

            OrgList.ForEach(_org =>
            {
                var OverDeltaCount = 0;

                var OrgRaws = (from raw in Table
                               where raw.Org == _org
                               select raw).ToList();
                var OrgItemList = OrgRaws.Select(x => x.ItemNameType).Distinct().ToList();

                OrgItemList.ForEach(item =>
                {
                    var ItemDelta = new ItemNameTypeDeltaInfo();
                    var OrgItemRaws = (from temp in OrgRaws
                                       where temp.ItemNameType == item
                                       select temp).OrderBy(x => x.Date).ToList();
                    if (OrgItemRaws.Count > 1)
                    {
                        ItemDelta = CountDeltaByTwoDays(OrgItemRaws[0], OrgItemRaws[OrgItemRaws.Count - 1]);
                        if (ItemDelta.SpareDelta > 5)
                            OverDeltaCount++;
                    }
                });

                Output[_org] = OverDeltaCount.ToString();
            });

            return Output;
        }

        public static List<ItemNameTypeDeltaInfo> GetRangeDeltaByItemNameType(List<PTEWEB_ItemNameType_ByDaily> Table)
        {
            var Output = new List<ItemNameTypeDeltaInfo>();
            var DailyDelta = new List<ItemNameTypeDeltaInfo>();
            if (Table.Count == 0) return DailyDelta;

            for (int i = 0; i < Table.Count - 1; i++)
            {
                DailyDelta.Add(CountDeltaByTwoDays(Table[i], Table[i + 1]));
            }

            return DailyDelta;
        }

        public static List<ItemNameTypeDeltaInfo> GetItemNameDeltaTable(List<PTEWEB_ItemNameType_ByDaily> Table)
        {
            var AllRangeDeltaInfo = Table.ToList().OrderBy(x => x.Date).ToList();
            var RangeDeltaTable = GetRangeDeltaByItemNameType(AllRangeDeltaInfo);
            return ProcessDeltaTableColor(RangeDeltaTable);
        }

        public static List<ItemNameTypeFYRTilteInfo> GetItemNameTitleDeltaTable(List<PTEWEB_ItemNameType_ByDaily> Table,string sdate,string edate)
        {
            if (Table.Count == 0) return new List<ItemNameTypeFYRTilteInfo>();

            var ProcessFYRData = DataHandlerFunctions.DataHandlerToProcessAllFYR(Table);
            var RangeDelta_Init = CountDeltaByTwoDays(Table[0], Table[Table.Count - 1]);
            var DeltaColors = ProcessDeltaTableColor(RangeDelta_Init);

            var Output = new List<ItemNameTypeFYRTilteInfo>()
            {
                new ItemNameTypeFYRTilteInfo(ProcessFYRData[0])
                {
                    Title=$@"{sdate} to {edate}",
                    Total = Table.Select(x =>x.Total).Sum(),
                    FailRateDelta = RangeDelta_Init.FailRateDelta,
                    RetryRateDelta = RangeDelta_Init.RetryRateDelta,
                    FYRDelta = RangeDelta_Init.FYRDelta,
                    SpareDelta = RangeDelta_Init.SpareDelta,
                    FYRColor = DeltaColors.FYRColor,
                    FRColor = DeltaColors.FRColor,
                    RTRColor = DeltaColors.RTRColor,
                    SpareColor = DeltaColors.SpareColor
                }
            };
            return Output;
        }

        private static ItemNameTypeDeltaInfo CountDeltaByTwoDays(PTEWEB_ItemNameType_ByDaily BaseDay, PTEWEB_ItemNameType_ByDaily ThisDay)
        {
            var Result = new ItemNameTypeDeltaInfo()
            {
                Title = BaseDay.Date.ToString("yyyy-MM-dd") + " to " + ThisDay.Date.ToString("yyyy-MM-dd"),
                FYRDelta = ThisDay.FYR == 0 || BaseDay.FYR == 0 ? 0 : Math.Round((ThisDay.FYR - BaseDay.FYR) / 100, 2) * 100,
                FailRateDelta = ThisDay.Fail_Rate == 0 || BaseDay.Fail_Rate == 0 ? 0 : Math.Round((ThisDay.Fail_Rate - BaseDay.Fail_Rate) / 100, 2) * 100,
                RetryRateDelta = ThisDay.Retry_Rate == 0 || BaseDay.Retry_Rate == 0 ? 0 : Math.Round((ThisDay.Retry_Rate - BaseDay.Retry_Rate) / 100, 2) * 100,
                SpareDelta = ThisDay.Avg_Pass_Time == 0 || BaseDay.Avg_Pass_Time == 0 ? 0 : Math.Round((ThisDay.Avg_Pass_Time - BaseDay.Avg_Pass_Time) / BaseDay.Avg_Pass_Time, 2) * 100
            };
            return Result;
        }

        private static List<ItemNameTypeDeltaInfo> ProcessDeltaTableColor(List<ItemNameTypeDeltaInfo> Table)
        {
            Table.ForEach(item =>
            {
                if (item.FYRDelta < -10)
                {
                    item.FYRColor = "red" + ";font-weight: bold;";
                }
                if (item.FYRDelta > 10)
                {
                    item.FYRColor = "blue" + ";font-weight: bold;";
                }
                if (item.RetryRateDelta > 10)
                {
                    item.RTRColor = "red" + ";font-weight: bold;";
                }
                if (item.RetryRateDelta < -10)
                {
                    item.RTRColor = "blue" + ";font-weight: bold;";
                }
                if (item.SpareDelta > 10)
                {
                    item.SpareColor = "red" + ";font-weight: bold;";
                }
                if (item.SpareDelta < -10)
                {
                    item.SpareColor = "blue" + ";font-weight: bold;";
                }
                if (item.FailRateDelta > 10)
                {
                    item.FRColor = "red" + ";font-weight: bold;";
                }
                if (item.FailRateDelta < -10)
                {
                    item.FRColor = "blue" + ";font-weight: bold;";
                }
            });
            return Table;
        }

        private static ItemNameTypeDeltaInfo ProcessDeltaTableColor(ItemNameTypeDeltaInfo Table)
        {
            if (Table.FYRDelta < -10)
            {
                Table.FYRColor = "red" + ";font-weight: bold;";
            }
            if (Table.FYRDelta > 10)
            {
                Table.FYRColor = "blue" + ";font-weight: bold;";
            }
            if (Table.RetryRateDelta > 10)
            {
                Table.RTRColor = "red" + ";font-weight: bold;";
            }
            if (Table.RetryRateDelta < -10)
            {
                Table.RTRColor = "blue" + ";font-weight: bold;";
            }
            if (Table.SpareDelta > 10)
            {
                Table.SpareColor = "red" + ";font-weight: bold;";
            }
            if (Table.SpareDelta < -10)
            {
                Table.SpareColor = "blue" + ";font-weight: bold;";
            }
            if (Table.FailRateDelta > 10)
            {
                Table.FRColor = "red" + ";font-weight: bold;";
            }
            if (Table.FailRateDelta < -10)
            {
                Table.FRColor = "blue" + ";font-weight: bold;";
            }
            return Table;
        }
    }

    public class TopFailInfo
    {
        public string FailItemName { get; set; }
        public int FaiCount { get; set; }
    }

    public class TopSpareInfo
    {
        public string FailItemName { get; set; }
        public double Spare { get; set; }
    }

    public class SimplyFYRInfo
    {
        public string Date { get; set; }

        public int ItemNameType { get; set; }

        public double Retry_Rate { get; set; }

        public double FYR { get; set; }

        public double Spare { get; set; }

        public double EstimateUPH { get; set; }

        public int Total { get; set; }

        public string href { get; set; }
    }

    public class ItemNameTypeDeltaInfo
    {
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "FYR Delta (%)")]
        public double FYRDelta { get; set; }

        [Display(Name = "Retry Rate Delta (%)")]
        public double RetryRateDelta { get; set; }

        [Display(Name = "Fail Rate Delta (%)")]
        public double FailRateDelta { get; set; }

        [Display(Name = "Spare Delta (%)")]
        public double SpareDelta { get; set; }

        [Display(Name = "FYR Color")]
        public string FYRColor { get; set; }

        [Display(Name = "Spare Color")]
        public string SpareColor { get; set; }

        [Display(Name = "RTR Color")]
        public string RTRColor { get; set; }

        [Display(Name = "FR Color")]
        public string FRColor { get; set; }

        public ItemNameTypeDeltaInfo()
        {
            Title = string.Empty;
            FYRDelta = 0;
            RetryRateDelta = 0;
            FailRateDelta = 0;
            SpareDelta = 0;
            FYRColor = "black";
            RTRColor = "black";
            FRColor = "black";
            SpareColor = "black";
        }
    }

    public class ItemNameTypeFYRTilteInfo
    {
        [Display(Name = "Org")]
        public String Org { get; set; }

        [Display(Name = "Description")]
        public String Description { get; set; }

        [Display(Name = "Title")]
        public String Title { get; set; }

        [Display(Name = "Pass_Rate (%)")]
        public Double Pass_Rate { get; set; }

        [Display(Name = "Fail_Rate (%)")]
        public Double Fail_Rate { get; set; }

        [Display(Name = "Retry_Rate (%)")]
        public Double Retry_Rate { get; set; }

        [Display(Name = "FYR (%)")]
        public Double FYR { get; set; }

        [Display(Name = "Total")]
        public Double Total { get; set; }

        [Display(Name = "Avg_Pass_Time")]
        public Double Avg_Pass_Time { get; set; }

        [Display(Name = "FYR Delta (%)")]
        public double FYRDelta { get; set; }

        [Display(Name = "Retry Rate Delta (%)")]
        public double RetryRateDelta { get; set; }

        [Display(Name = "Fail Rate Delta (%)")]
        public double FailRateDelta { get; set; }

        [Display(Name = "Spare Delta (%)")]
        public double SpareDelta { get; set; }

        [Display(Name = "FYR Color")]
        public string FYRColor { get; set; }

        [Display(Name = "Spare Color")]
        public string SpareColor { get; set; }

        [Display(Name = "RTR Color")]
        public string RTRColor { get; set; }

        [Display(Name = "FR Color")]
        public string FRColor { get; set; }

        public ItemNameTypeFYRTilteInfo(PTEWEB_ItemNameType_ByDaily Table)
        {
            Org = Table.Org;
            Description = Table.Description;
            FYR = Table.FYR;
            Total = 0;
            FYRDelta = 0;
            Fail_Rate = Table.Fail_Rate;
            FailRateDelta = 0;
            Retry_Rate = Table.Retry_Rate;
            RetryRateDelta = 0;
            Avg_Pass_Time = Table.Avg_Pass_Time;
            SpareDelta = 0;
            FYRColor = "black";
            RTRColor = "black";
            FRColor = "black";
            SpareColor = "black";
        }
    }
}