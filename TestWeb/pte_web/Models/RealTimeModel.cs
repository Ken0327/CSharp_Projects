using PTE_Web.Connections;
using PTE_Web.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Dapper;

namespace PTE_Web.Models
{
    public class RealTimeModel
    {
        public static List<RealTimeUPH_Total> GetAllDayData(string org, string shift)
        {
            var AllDay = new List<RealTimeUPH_Total>();

            if (shift == "Now")
            {
                var count = 0;
                while (true)
                {
                    if (DateTime.Now.AddHours(-count).Hour == 7 || DateTime.Now.AddHours(-count).Hour == 19) break;
                    var start = DateTime.Now.AddHours(-count);
                    var ItemNameList_Final = DataHandlerFunctions.GetItemNameTypeByHour(org, start, "tblfinal",1);
                    var ItemNameList_Cpu = DataHandlerFunctions.GetItemNameTypeByHour(org, start, "tblcpu",1);
                    var ItemNameListShift = new List<ItemNameType_Table>().Concat(ItemNameList_Final).ToList().Concat(ItemNameList_Cpu).ToList();
                    AllDay.Add(new RealTimeUPH_Total { Org = org, Date = DateTime.Parse(start.ToString("yyyy-MM-dd")), StartTime = DateTime.Parse(start.ToString("yyyy-MM-dd") + $@" {start.Hour}:00:00"), ItemNameTypeList = ItemNameListShift });
                    count++;
                }
            }

            if (shift == "Previous")
            {
                if (DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 20)   //Day
                {
                    var FlagDateTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 07:00:00");
                    while (FlagDateTime.Hour != 19)
                    {
                        var ItemNameList_Final = DataHandlerFunctions.GetItemNameTypeByHour(org, FlagDateTime, "tblfinal",1);
                        var ItemNameList_Cpu = DataHandlerFunctions.GetItemNameTypeByHour(org, FlagDateTime, "tblcpu",1);
                        var ItemNameListShift = new List<ItemNameType_Table>().Concat(ItemNameList_Final).ToList().Concat(ItemNameList_Cpu).ToList();
                        AllDay.Add(new RealTimeUPH_Total { Org = org, Date = DateTime.Parse(FlagDateTime.ToString("yyyy-MM-dd")), StartTime = FlagDateTime, ItemNameTypeList = ItemNameListShift });
                        FlagDateTime = FlagDateTime.AddHours(-1);
                    }
                }
                else
                {
                    var FlagDateTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 19:00:00");
                    while (FlagDateTime.Hour != 7)
                    {
                        var ItemNameList_Final = DataHandlerFunctions.GetItemNameTypeByHour(org, FlagDateTime, "tblfinal",1);
                        var ItemNameList_Cpu = DataHandlerFunctions.GetItemNameTypeByHour(org, FlagDateTime, "tblcpu",1);
                        var ItemNameListShift = new List<ItemNameType_Table>().Concat(ItemNameList_Final).ToList().Concat(ItemNameList_Cpu).ToList();
                        AllDay.Add(new RealTimeUPH_Total { Org = org, Date = DateTime.Parse(FlagDateTime.ToString("yyyy-MM-dd")), StartTime = FlagDateTime, ItemNameTypeList = ItemNameListShift });
                        FlagDateTime = FlagDateTime.AddHours(-1);
                    }
                }
            }
            return AllDay;
        }

        public static List<Estimate_UPH> GetAllRealContent(string org ,string date)
        {
            try
            {
                //var today = DateTime.Today.ToString("yyyy-MM-dd");


                var script = $@"SELECT *  FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_RealOutput] where Org = '{org}' and date = '{date}'";

                using (var db = ConnectionFactory.CreatConnection())
                {
                    var result = db.Query<Estimate_UPH>(script).ToList();
                    return result;
                }
            }
            catch(Exception e)
            {
                return null;
            }
        }
    }

    public class DailyUPH
    {
        public DateTime Date { get; set; }
        public string Org { get; set; }
        public int ItemNameType { get; set; }
        public string ProductName { get; set; }
        public int RealOutput { get; set; }
        public float EstimateUPH { get; set; }
        public int UPH { get; set; }
        public float AvgSpare { get; set; }
        public string table { get; set; }
        public float Gap { get; set; }
    }

    public class Estimate_UPH
    {
        [Display(Name = "Date Time")]
        public DateTime DateTime { get; set; }

        [Display(Name = "Time Index")]
        public int TimeIndex { get; set; }

        [Display(Name = "ItemNameType")]
        public int itemnametype { get; set; }

        [Display(Name = "Table")]
        public string table { get; set; }

        [Display(Name = "ProductName")]
        public string productname { get; set; }

        [Display(Name = "Time Range")]
        public string testTimeRange { get; set; }

        [Display(Name = "Real Output")]
        public int RealOutput { get; set; }

        [Display(Name = "Hours")]
        public double EstimateHours { get; set; }

        [Display(Name = "Estimate UPH")]
        public double EstimateUPH { get; set; }

        [Display(Name = "UPH")]
        public int UPH { get; set; }

        [Display(Name = "Average Spare (s)")]
        public double AvgSpare { get; set; }

        [Display(Name = "FYR (%)")]
        public double FYR { get; set; }

        [Display(Name = "Gap (s)")]
        public double Gap { get; set; }

        [Display(Name = "Spare (%)")]
        public double GapPercent { get; set; }

        public string color { get; set; }

        public int shiftid { get; set; }

        public string shift { get; set; }

    }

    public class DailyOutputTrend
    {
        [Display(Name = "Date")]
        public string date { get; set; }

        [Display(Name = "Real Output")]
        public int Output { get; set; }

        [Display(Name = "Total Station")]
        public int totalstation { get; set; }

        [Display(Name = "Completion rate (%)")]
        public float UPHAchieveRate { get; set; }

        [Display(Name = "Delta (%)")]
        public float delta { get; set; }
    }

    public class ItemUPHList
    {
        public int ItemNameType { get; set; }
        public int UPH { get; set; }
    }

    public class ItemNameType_Table
    {
        public string Table { get; set; }
        public int ItemNametype { get; set; }
    }

    public class RealTimeFixtureInfo
    {
        public string ProductName { get; set; }
        public int ItemNameType { get; set; }
        public double FYR { get; set; }
        public int count { get; set; }
        public bool On_Production { get; set; }
         
    }

    public class UPHTrend_Table
    {
        public List<Estimate_UPH> GroupItemNameTable { get; set; }
        public List<Estimate_UPH> FocusItemTable { get; set; }
        public List<DailyOutputTrend> DailyTrendTable { get; set; }
    }

    public class ItemMappingList
    {
        public string ProductName { get; set; }
        public int ItemNametype { get; set; }
    }
}