using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DailyUPHJob
{
    internal class Program
    {
        private static readonly Logger _PTElogger = LogManager.GetLogger("PTE_ReportLogger");

        private static void Main(string[] args)
        {
            var db = new SqlCommandFunction();
            var ProcessFunc = new Process();
            var ProcessDate = ProcessFunc.ProgessDateHandler(1);
            foreach (var ProgressDate in ProcessDate)
            {
                _PTElogger.Trace($@"Date: {ProgressDate}...Start Progress");
                Console.WriteLine($@"Date: {ProgressDate}...Start Progress");
                var RawData = db.GetDailyData(ProgressDate);
                var DailyReportTable = ProcessFunc.ProcessRawData(RawData, ProgressDate);
                var result = db.InsertDailyReport(DailyReportTable);
                if (result)
                    _PTElogger.Trace($@"Date: {ProgressDate}...OK");
                else
                    _PTElogger.Trace($@"Date: {ProgressDate}...Fail");
                Console.WriteLine($@"Date: {ProgressDate}...Completed");
            }
        }
    }

    internal class Process
    {
        private static readonly Logger _PTElogger = LogManager.GetLogger("PTE_ReportLogger");

        // ProcessDateCount: How many days that you need to process.
        // Start Date is today.
        // Decrease process.
        // Production line must set ProcessDateCount as 1
        public List<string> ProgessDateHandler(int ProcessDateCount)
        {
            try
            {
                var DateList = new List<string>();
                for (int date_index = 1; date_index <= ProcessDateCount; date_index++)
                {
                    var ThisDate = DateTime.Today.AddDays(-1 * (date_index)).ToString("yyyy-MM-dd");
                    DateList.Add(ThisDate);
                }
                return DateList;
            }
            catch (Exception e)
            {
                return new List<string>() { DateTime.Today.AddDays(-1 * (ProcessDateCount)).ToString("yyyy-MM-dd") };
            }
        }

        public List<PTEWEB_ItemNameType_RealOutput_ByDaily> ProcessRawData(List<PTEWEB_ItemNameType_RealOutput> Table, string thisDate)
        {
            var Output = new List<PTEWEB_ItemNameType_RealOutput_ByDaily>();
            var OrgList = new List<string>() { "T1", "T2", "T3", "T5" };
            try
            {
                foreach (var org in OrgList)
                {
                    var OrgTable = Table.Where(x => x.Org.Trim() == org).ToList();
                    var ItemNameTypeList = (from item in OrgTable
                                            select item.ItemNameType).Distinct().ToList();
                    foreach (var item in ItemNameTypeList)
                    {
                        var TempTable_Org_Item = OrgTable.Where(x => x.ItemNameType == item).ToList();
                        var NewRaw = new PTEWEB_ItemNameType_RealOutput_ByDaily() { Date = DateTime.Parse(thisDate + " 00:00:00"), ItemNameType = item, Org = org, AvgSpare = TempTable_Org_Item.Average(x => x.AvgSpare), UPH = TempTable_Org_Item.First().UPH, EstimateUPH = TempTable_Org_Item.Max(X => X.EstimateUPH), RealOutput = TempTable_Org_Item.Sum(X => X.RealOutput), ProductName = TempTable_Org_Item.First().productname, table = TempTable_Org_Item.First().table };
                        NewRaw.Gap = CalculateSpareGap(NewRaw);
                        Output.Add(NewRaw);
                    }
                }

                return Output;
            }
            catch (Exception e)
            {
                _PTElogger.Trace($@"Process Fail: {e.ToString()}");
                return new List<PTEWEB_ItemNameType_RealOutput_ByDaily>();
            }
        }

        public float CalculateSpareGap(PTEWEB_ItemNameType_RealOutput_ByDaily input)
        {
            try
            {
                float gap = 0;
                if (input.UPH == 999 || input.UPH <= input.EstimateUPH)
                    return 0;

                var EstimatePort = input.EstimateUPH / ((float)3600 / input.AvgSpare);
                var GoalSpare = (float)3600 / ((float)input.UPH / EstimatePort);
                gap = input.AvgSpare - GoalSpare;
                return gap;
            }
            catch (Exception e)
            {
                _PTElogger.Trace($@"Gap Calculate Fail: {input.ItemNameType},UPH:{input.UPH},Output:{input.RealOutput},EstimateUPH:{input.EstimateUPH}");
                _PTElogger.Trace(e.ToString());
                return 0;
            }
        }
    }
}