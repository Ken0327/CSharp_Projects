using PTE_Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;

namespace PTE_Web.Connections
{
    public class MGUDataHandler
    {
        public static List<string> GetAllGPN()
        {
            try
            {
                using (var conn = ConnectionFactory.CreatConnection("C5"))
                {
                    return conn.Query<string>($@"select Distinct(GPN)  FROM [config_db].[dbo].[xx_GPNControl]  where GPN LIKE '012%' OR GPN LIKE '%010%'").ToList();
                }
            }
            catch (Exception e)
            {
                return new List<string>();
            }
        }

        public static List<SimplyMGUChartInfo> OutMGUSOPerformanceChartList(List<BasicTestPerformance> input)
        {
            try
            {
                var ChartInfo = new List<SimplyMGUChartInfo>();

                foreach (var station in input)
                {
                    ChartInfo.Add(new SimplyMGUChartInfo()
                    {
                        TestStation = station.TestStation,
                        Total = station.Total,
                        EstimateUPH = station.UPH,
                        FYR = station.FYR
                    });
                }

                return ChartInfo;
            }
            catch (Exception e)
            {
                return new List<SimplyMGUChartInfo>();
            }
        }

        private static Dictionary<string, List<StationDetailTestItemInfo>> AppendFixtureCorrelationInfo(Dictionary<string, List<StationDetailTestItemInfo>> input, string so)
        {
            try
            {
                var StationList = input.Keys.ToList();

                foreach (var station in StationList)
                {
                    foreach (var item in input[station])
                    {
                        item.FixtureCorrelation = CheckFixtureCorr(so, station, item.FailStepNumber) == true ? "Yes" : "No";
                    }
                }
                return input;
            }
            catch (Exception e)
            {
                return input;
            }
        }

        private static bool CheckFixtureCorr(string so, string station, string failitem)
        {
            try
            {
                var TestFixtureRawData = GetFixtureRawDataByFailItemAndStation(so, station, failitem);
                if (TestFixtureRawData.Count == 0) return false;

                var FixtureTestCountLimit = (TestFixtureRawData.Sum(item => item.TestCount) / TestFixtureRawData.Count()) / 2;
                var AvaliableFixture = TestFixtureRawData.FindAll(item => item.TestCount >= FixtureTestCountLimit).ToList();
                double CorrelationLimitRate = (1.0 / ((double)(AvaliableFixture.Count == 0 ? 1 : AvaliableFixture.Count))) * 2;
                return AvaliableFixture.FindAll(item => (double)item.FailPercent >= CorrelationLimitRate).Count > 0 ? true : false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static List<StationCorrelationTable> GetFixtureRawDataByFailItemAndStation(string so, string station, string failitem)
        {
            try
            {
                var FixtureTable = new List<StationCorrelationTable>();

                using (var conn = ConnectionFactory.CreatConnection("C5"))
                {
                    var script = $@"SELECT Total.MachineName,Total.Position,coalesce(FailTable.FailCount,0)as FailCount,Total.TestCount,
                                    ROUND((CAST(FailCount AS FLOAT)/CAST(TestCount AS FLOAT)),3) as FailPercent
                                    FROM
                                    (SELECT MachineName,Position,count(*) as TestCount  FROM [MGUDB].[dbo].[MGUResult]  WHERE JobNumber ='{so}' and TestStation ='{station}'  group by MachineName,Position) as Total
                                    left join
                                    (SELECT MachineName,Position,count(*) as FailCount  FROM [MGUDB].[dbo].[MGUResult]  WHERE JobNumber ='{so}' and TestStation ='{station}' and FailStepNumber ='{failitem}'  group by MachineName,Position) AS FailTable
                                    on FailTable.MachineName = Total.MachineName and FailTable.Position = Total.Position";

                    FixtureTable = conn.Query<StationCorrelationTable>(script).ToList();
                }
                return FixtureTable;
            }
            catch (Exception e)
            {
                return new List<StationCorrelationTable>();
            }
        }

        public static Dictionary<string, List<BasicTestPerformance>> OutPerformanceByAllShopOrderAndDateTime(List<string> SoList, string gpn, string sdate, string edate)
        {
            try
            {
                var AllDict = new Dictionary<string, List<BasicTestPerformance>>();
                foreach (var so in SoList)
                {
                    var temp = GetBasicTestPerformance_ByGPN_SO(so, gpn, sdate, edate);

                    AllDict[so] = temp;
                }
                return AllDict;
            }
            catch (Exception e)
            {
                return new Dictionary<string, List<BasicTestPerformance>>();
            }
        }

        public static List<BasicTestPerformance> OutPerformanceBySingleShopOrderAndDateTime(string so, string gpn, string sdate, string edate)
        {
            var AllTable = new List<BasicTestPerformance>();

            AllTable.AddRange(GetBasicTestPerformance_ByGPN_SO(so, gpn, sdate, edate));

            return AllTable;
        }

        public static List<BasicTestPerformance> OutPerformanceByDateTime(string workdate)
        {
            var AllTable = new List<BasicTestPerformance>();

            AllTable.AddRange(GetBasicTestPerformance_ByDate(workdate));

            return AllTable;
        }

        public static List<SOTestPerformance> OutSOListByGPNAndDateTime(string gpn, string sdate, string edate)
        {
            var AllTable = new List<SOTestPerformance>();

            AllTable = GetSOBasicInfoByGPN(gpn, sdate, edate);

            return AllTable;
        }

        public static List<StationDetailTestItemInfo> OutTestItemListBySOandStation(string so, string sdate, string edate)
        {
            try
            {
                using (var conn = ConnectionFactory.CreatConnection("C5"))
                {
                    return conn.Query<StationDetailTestItemInfo>($@"SELECT TestStation,FailStepNumber,StepDesctiption,count(*)as FailCount  FROM [MGUDB].[dbo].[MGUResult]  where JobNumber ='{so}'   and StepResult = 0   group by TestStation,FailStepNumber,StepDesctiption").ToList();
                }
            }
            catch (Exception e)
            {
                return new List<StationDetailTestItemInfo>();
            }
        }

        public static List<StationDetailTestItemInfo> OutTestItemListByDateTimeandStation(string workdate)
        {
            try
            {
                var DB_Sdate = DateTime.Parse(workdate).AddDays(-1).ToString("yyyy-MM-dd") + " 16:00:00";
                var DB_Edate = workdate + " 15:59:59";
                using (var conn = ConnectionFactory.CreatConnection("C5"))
                {
                    return conn.Query<StationDetailTestItemInfo>($@"SELECT TestStation,FailStepNumber,StepDesctiption,count(*)as FailCount  FROM [MGUDB].[dbo].[MGUResult]  where CreatTime  between '{DB_Sdate}' and '{DB_Edate}'  and StepResult = 0   group by TestStation,FailStepNumber,StepDesctiption").ToList();
                }
            }
            catch (Exception e)
            {
                return new List<StationDetailTestItemInfo>();
            }
        }

        public static Dictionary<string, string> GenerateStationList(List<string> StationLiat)
        {
            var OutDict = new Dictionary<string, string>();
            StationLiat.ForEach(item =>
            {
                OutDict[item] = "fade";
            });

            OutDict[OutDict.First().Key] = "fade in active";

            return OutDict;
        }

        public static Dictionary<string, List<StationDetailTestItemInfo>> OutStationTestItemInfoDict(List<StationDetailTestItemInfo> source, List<BasicTestPerformance> outline)
        {
            try
            {
                var Output = new Dictionary<string, List<StationDetailTestItemInfo>>();
                foreach (var station in outline)
                {
                    var Rank = 1;
                    var temp = source.Where(item => item.TestStation == station.TestStation).ToList();
                    var AllFail = temp.Sum(item => item.FailCount);
                    temp = temp.OrderByDescending(item => item.FailCount).ToList();
                    temp.ForEach(item =>
                    {
                        item.TestStation = station.TestStation;
                        item.Order = Rank++;
                        item.FailPercent = double.Parse((((double)item.FailCount / (double)AllFail) * 100).ToString("F2"));
                        item.FailRate = double.Parse((((double)item.FailCount / (double)station.Total) * 100).ToString("F2"));
                    });

                    Output[station.TestStation] = temp;
                }

                Output = AppendFixtureCorrelationInfo(Output, outline.First().JobNumber.ToString());
                return Output;
            }
            catch (Exception e)
            {
                return new Dictionary<string, List<StationDetailTestItemInfo>>();
            }
        }

        public static List<DailyStationInfo> OutStationTestItemInfoList(List<StationDetailTestItemInfo> source, List<BasicTestPerformance> outline, List<MGUJobClass> joblist)
        {
            try
            {
                var Output = new List<DailyStationInfo>();
                foreach (var station in outline)
                {
                    var Rank = 1;
                    var station_detail_list = source.Where(item => item.TestStation == station.TestStation).ToList();
                    var station_job_list = joblist.Where(item => item.TestStation == station.TestStation).ToList();

                    var AllFail = station_detail_list.Sum(item => item.FailCount);
                    station_detail_list = station_detail_list.OrderByDescending(item => item.FailCount).ToList();
                    station_detail_list.ForEach(item =>
                    {
                        item.TestStation = station.TestStation;
                        item.Order = Rank++;
                        item.FailPercent = double.Parse((((double)item.FailCount / (double)AllFail) * 100).ToString("F2"));
                        item.FailRate = double.Parse((((double)item.FailCount / (double)station.Total) * 100).ToString("F2"));
                    });

                    var tempRow = new DailyStationInfo()
                    {
                        Station = station.TestStation,
                        FYR = station.FPR,
                        Total = station.Total,
                        _FailItemList = station_detail_list,
                        Spare = station.Avg_Pass_Time,
                        DetailLinkList = station_job_list
                    };
                    Output.Add(tempRow);
                }

                return Output;
            }
            catch (Exception e)
            {
                return new List<DailyStationInfo>();
            }
        }

        public static List<MGUResult> OutAllTestResultByESN(string esn)
        {
            try
            {
                using (var conn = ConnectionFactory.CreatConnection("C5"))
                {
                    return conn.Query<MGUResult>($@"SELECT *  FROM [MGUDB].[dbo].[MGUResult]  where serialnumber ='{esn}' order by CreatTime desc").ToList();
                }
            }
            catch (Exception e)
            {
                return new List<MGUResult>();
            }
        }

        public static Dictionary<string, string> OutLogPathDict(List<MGUResult> AllList)
        {
            try
            {
                var TestStationLogPathDict = new Dictionary<string, string>();

                var StationList = AllList.
                                GroupBy(c => c.TestStation).
                                Select(g => new { TestStation = g.Key, Count = g.Count() });

                return TestStationLogPathDict;
            }
            catch (Exception e)
            {
                return new Dictionary<string, string>();
            }
        }

        public static string GetESN_SNTransform(string input)
        {
            if (input.First() == 'G')
            {
                try
                {
                    using (var conn = ConnectionFactory.CreatConnection("C5"))
                    {
                        return conn.Query<string>($@"").First();
                    }
                }
                catch (Exception e)
                {
                    return string.Empty;
                }
            }
            else
            {
                try
                {
                    return string.Empty;
                }
                catch (Exception e)
                {
                    return string.Empty;
                }
            }
        }

        private static List<SOTestPerformance> GetSOBasicInfoByGPN(string gpn, string sdate, string edate)
        {
            try
            {
                using (var conn = ConnectionFactory.CreatConnection("C5"))
                {
                    return conn.Query<SOTestPerformance>($@"select PartNumber,JobNumber,SWVersion,ProdVersion,D_Total,Order_Date from
                        (SELECT ROW_NUMBER() OVER(partition by JobNumber ORDER BY COUNT(Distinct(Serialnumber)) desc ) AS ROWID,PartNumber,JobNumber,SWVersion,ProdVersion,COUNT(Distinct(Serialnumber))as D_Total ,min(CreatTime) as Order_Date
                          FROM [MGUDB].[dbo].[MGUResult]  where PartNumber ='{gpn}' and TestStation ='FCT'  and JobNumber!= '999999' and OperatorID !='9999' and CreatTime between '{sdate}' and '{edate}'   group by PartNumber,JobNumber,SWVersion,ProdVersion ) as total  where total.ROWID =1").OrderByDescending(x => DateTime.Parse(x.Order_Date)).ToList();
                }
            }
            catch (Exception e)
            {
                return new List<SOTestPerformance>();
            }
        }

        public static List<string> OutPartNumber(string sdate, string edate)
        {
            try
            {
                using (var conn = ConnectionFactory.CreatConnection("C5"))
                {
                    return conn.Query<string>($@"SELECT PartNumber  FROM [MGUDB].[dbo].[MGUResult]  WHERE JobNumber !='999999' and TestStation ='FCT' AND OperatorID !='9999' AND JobNumber !='32342355' and CreatTime between '{sdate}' and '{edate}'   group by PartNumber   ").ToList();
                }
            }
            catch (Exception e)
            {
                return new List<string>();
            }
        }

        private static Dictionary<string, int> GetStation_UPHByShoporder(string so)
        {
            try
            {
                var MapDict = new Dictionary<string, int>();
                var script = $@"select TestStation,MAX(Total) as UPH
                                from  (SELECT TestStation,CONVERT(varchar(13),CreatTime,120)as TimeIndex,Count(*) as Total
                                  FROM [MGUDB].[dbo].[MGUResult]
                                  where  JobNumber ='{so}' and StepResult = 1 and OperatorID !='9999'
                                  group by TestStation  ,CONVERT(varchar(13),CreatTime,120)) Finaltable group by Finaltable.TestStation";
                using (var conn = ConnectionFactory.CreatConnection("C5"))
                {
                    var df = conn.Query<BasicTestPerformance>(script).ToList();
                    foreach (var station in df)
                    {
                        MapDict[station.TestStation] = station.UPH;
                    }
                }
                return MapDict;
            }
            catch (Exception e)
            {
                return new Dictionary<string, int>();
            }
        }

        private static Dictionary<string, int> GetStation_UPHByWorkdate(string workdate)
        {
            try
            {
                var MapDict = new Dictionary<string, int>();
                var script = $@"select TestStation,MAX(Total) as UPH
                                from  (SELECT TestStation,CONVERT(varchar(13),CreatTime,120)as TimeIndex,Count(*) as Total
                                  FROM [MGUDB].[dbo].[MGUResult]
                                  where  CreatTime  between '{workdate} 00:00:00' and '{workdate} 23:59:59'  and StepResult = 1 and OperatorID !='9999'
                                  group by TestStation  ,CONVERT(varchar(13),CreatTime,120)) Finaltable group by Finaltable.TestStation";
                using (var conn = ConnectionFactory.CreatConnection("C5"))
                {
                    var df = conn.Query<BasicTestPerformance>(script).ToList();
                    foreach (var station in df)
                    {
                        MapDict[station.TestStation] = station.UPH;
                    }
                }
                return MapDict;
            }
            catch (Exception e)
            {
                return new Dictionary<string, int>();
            }
        }

        private static List<BasicTestPerformance> GetBasicTestPerformance_ByGPN_SO(string so, string gpn, string sdate, string edate)
        {
            try
            {
                var df = new List<BasicTestPerformance>();

                var script = $@"select TotalTable.ItemNameType,TotalTable.JobNumber,TotalTable.TestStation,TotalTable.Total,TotalTable.D_Total,PassTable.Pass,PassTable.D_Pass,PassTable.Avg_Pass_Time,TotalTable.Avg_Total_Time,isnull(FailTable.Fail,0) as Fail ,isnull(FailTable.D_Fail ,0) as D_Fail
                                from
                                (SELECT ItemNameType,TestStation,JobNumber,count(*) as Total,count(Distinct(SerialNumber)) as D_Total,AVG(Duration) as Avg_Total_Time
                                  FROM [MGUDB].[dbo].[MGUResult]
                                  where PartNumber ='{gpn}' and JobNumber ='{so}'  and OperatorID !='9999'
                                  group by ItemNameType,TestStation,JobNumber) TotalTable
                                  left join
                                  (SELECT ItemNameType,TestStation,JobNumber, count(*) as Pass ,  count(distinct(serialnumber)) as D_Pass ,AVG(Duration) as Avg_Pass_Time
                                  FROM [MGUDB].[dbo].[MGUResult]
                                  where PartNumber ='{gpn}' and JobNumber ='{so}'  and StepResult = 1 and OperatorID !='9999'
                                  group by ItemNameType,TestStation,JobNumber)as PassTable
                                  on TotalTable.ItemNameType = PassTable.ItemNameType
                                  left join
                                  (SELECT ItemNameType,TestStation,JobNumber, count(*) as Fail ,  count(distinct(serialnumber)) as D_Fail
                                  FROM [MGUDB].[dbo].[MGUResult]
                                  where PartNumber ='{gpn}' and JobNumber ='{so}'  and StepResult = 0 and OperatorID !='9999'
                                  group by ItemNameType,TestStation,JobNumber) as FailTable
                                  on FailTable.ItemNameType = TotalTable.ItemNameType";

                using (var conn = ConnectionFactory.CreatConnection("C5"))
                {
                    df = conn.Query<BasicTestPerformance>(script).ToList();
                }

                var UPHDict = GetStation_UPHByShoporder(so);
                foreach (var station in df)
                {
                    station.UPH = UPHDict[station.TestStation];
                }

                df.ForEach(item =>
                {
                    item.FPR = double.Parse(((1 - ((double)item.D_Fail / (double)item.D_Total)) * 100).ToString("F2"));
                    item.Fail_Rate = double.Parse(((1 - item.FYR) * 100).ToString("F2"));
                    item.Retry_Rate = double.Parse(((((double)item.Total / (double)item.D_Total) - 1) * 100).ToString("F2"));
                    item.FYR = double.Parse((((double)item.Pass / (double)item.Total) * 100).ToString("F2"));
                    item.Avg_Pass_Time = double.Parse(item.Avg_Pass_Time.ToString("F2"));
                    item.Avg_Total_Time = double.Parse(item.Avg_Total_Time.ToString("F2"));
                });
                return df;
            }
            catch (Exception e)
            {
                return new List<BasicTestPerformance>();
            }
        }

        private static List<BasicTestPerformance> GetBasicTestPerformance_ByDate(string workdate)
        {
            try
            {
                var DB_Sdate = DateTime.Parse(workdate).AddDays(-1).ToString("yyyy-MM-dd") + " 16:00:00";
                var DB_Edate = workdate + " 15:59:59";
                var df_Total = new List<BasicTestPerformance>();

                var df_FPR = new DailyMGUTrend();

                var script_Total = $@"select TotalTable.ItemNameType,TotalTable.TestStation,TotalTable.Total,TotalTable.D_Total,PassTable.Pass,PassTable.D_Pass,PassTable.Avg_Pass_Time,TotalTable.Avg_Total_Time,isnull(FailTable.Fail,0) as Fail ,isnull(FailTable.D_Fail ,0) as D_Fail
                                from
                                (SELECT ItemNameType,TestStation,count(*) as Total,count(Distinct(SerialNumber)) as D_Total,AVG(Duration) as Avg_Total_Time
                                  FROM [MGUDB].[dbo].[MGUResult]
                                  where CreatTime  between '{DB_Sdate}' and '{DB_Edate}'  and OperatorID !='9999'
                                  group by ItemNameType,TestStation) TotalTable
                                  left join
                                  (SELECT ItemNameType,TestStation, count(*) as Pass ,  count(distinct(serialnumber)) as D_Pass ,AVG(Duration) as Avg_Pass_Time
                                  FROM [MGUDB].[dbo].[MGUResult]
                                  where CreatTime  between '{DB_Sdate}' and '{DB_Edate}'   and StepResult = 1 and OperatorID !='9999'
                                  group by ItemNameType,TestStation)as PassTable
                                  on TotalTable.ItemNameType = PassTable.ItemNameType
                                  left join
                                  (SELECT ItemNameType,TestStation, count(*) as Fail ,  count(distinct(serialnumber)) as D_Fail
                                  FROM [MGUDB].[dbo].[MGUResult]
                                  where CreatTime  between '{DB_Sdate}' and '{DB_Edate}'   and StepResult = 0 and OperatorID !='9999'
                                  group by ItemNameType,TestStation) as FailTable
                                  on FailTable.ItemNameType = TotalTable.ItemNameType";

                var script_FPR = $@"SELECT TOP(1)  P.Date,round(MAX(FCT),2)*100 AS FCT_FPR,round(MAX(RUNIN),2)*100 AS RUNIN_FPR,round(MAX(FLASHING),2)*100 AS FLASHING_FPR,round(MAX(AFT),2)*100 AS AFT_FPR,round(MAX(SFT),2)*100 AS SFT_FPR,MIN(P.Pass) as Total
                                FROM (
	                                select FinalTable.Date,FinalTable.TestStation,FinalTable.Pass,FinalTable.Total,FinalTable.FPR
	                                from
	                                (select substring(Convert(char(17),SubTable.CreatTime,120),0,4) as Date,SubTable.TestStation,SubTable.Operation, Sum(SubTable.StepResult) as Pass,Count(SubTable.StepResult) as Total ,(CAST(Sum(SubTable.StepResult) AS float)/CAST(Count(SubTable.StepResult) as float)) as FPR
	                                from
	                                (SELECT SerialNumber,TestStation,Operation,CreatTime,StepResult,row_number() over(partition by SerialNumber,TestStation,Operation order by CreatTime asc) as number
	                                  FROM [MGUDB].[dbo].[MGUResult]
	                                  where CreatTime  between '{DB_Sdate}' and '{DB_Edate}' and Operation in ('EOL','FCT','FLASHING','OP_RUN_IN','SFT','RUNIN') and OperatorID !='9999' and StepResult in (0,1)
	                                  group by SerialNumber,TestStation,Operation,CreatTime,StepResult ) SubTable
	                                  where SubTable.number=1
	                                  group by TestStation,Operation,substring(Convert(char(17),SubTable.CreatTime,120),0,4)
	                                ) FinalTable
	                                where FinalTable.Total >100
                                ) t
                                PIVOT (
	                                MAX(FPR)
	                                FOR TestStation IN ([AFT], [FLASHING], [RUNIN],[FCT],[SFT])
                                ) p
                                GROUP BY P.Date";

                using (var conn = ConnectionFactory.CreatConnection("C5"))
                {
                    df_Total = conn.Query<BasicTestPerformance>(script_Total).ToList();
                    df_FPR = conn.Query<DailyMGUTrend>(script_FPR).FirstOrDefault();
                }

                df_Total.ForEach(item =>
                {
                    if (item.TestStation == "FCT")
                        item.FPR = df_FPR.FCT_FPR;
                    if (item.TestStation == "FLASHING")
                        item.FPR = df_FPR.FLASHING_FPR;
                    if (item.TestStation == "SFT")
                        item.FPR = df_FPR.SFT_FPR;
                    if (item.TestStation == "RUNIN")
                        item.FPR = df_FPR.RUNIN_FPR;
                    if (item.TestStation == "AFT")
                        item.FPR = df_FPR.AFT_FPR;
                });
                return df_Total;
            }
            catch (Exception e)
            {
                return new List<BasicTestPerformance>();
            }
        }

        public static List<MGUJobClass> GetProductionJobByStation_Date(string workdate)
        {
            try
            {
                var DB_Sdate = DateTime.Parse(workdate).AddDays(-1).ToString("yyyy-MM-dd") + " 16:00:00";
                var DB_Edate = workdate + " 15:59:59";
                using (var conn = ConnectionFactory.CreatConnection("C5"))
                {
                    var data = conn.Query<MGUJobClass>($@"SELECT TestStation,JobNumber,count(*) as Total
                                                  FROM [MGUDB].[dbo].[MGUResult]
                                                  where  CreatTime between '{DB_Sdate}'and '{DB_Edate}'
                                                 group by TestStation, JobNumber ").ToList();

                    foreach (var item in data)
                    {
                        item.JobNumber = $@"<a href=""http://pi.garmin.com.tw:999/SmartFactory/#/analytics/job?jobNumber={item.JobNumber}""> {item.JobNumber}</a>";
                    }
                    return data;
                }
            }
            catch (Exception e)
            {
                return new List<MGUJobClass>();
            }
        }

        private static ProductionData GetProductionClass(string Org, string GPN)
        {
            using (var connMssql = ConnectionFactory.CreatConnection("C5"))
            {
                string ProductNameGPN = GPN;

                var SWIStep = connMssql.Query<string>($@"SELECT Attribute2 FROM [config_db].[dbo].[xx_GPNControl] with(nolock) where Type = 'MGULabel' and Attribute1 = 'SWIStep' and GPN = '{ProductNameGPN}' and Status = '1'").FirstOrDefault();
                var ProdVersion = connMssql.Query<string>($@"SELECT Attribute2 FROM [config_db].[dbo].[xx_GPNControl] with(nolock) where Type = 'MGULabel' and Attribute1 = 'ProdVersion' and GPN = '{ProductNameGPN}' and Status = '1'").FirstOrDefault();
                var SWVersion = connMssql.Query<string>($@"SELECT Attribute2 FROM [config_db].[dbo].[xx_GPNControl] with(nolock) where Type = 'MGULabel' and Attribute1 = 'SWVersion' and GPN = '{ProductNameGPN}' and Status = '1'").FirstOrDefault();
                var Prefix_UID = connMssql.Query<string>($@"SELECT Attribute2 FROM [config_db].[dbo].[xx_GPNControl] with(nolock) where Type = 'MGULabel' and Attribute1 = 'Prefix_UID' and GPN = '{ProductNameGPN}' and Status = '1'").FirstOrDefault();
                var NAV = connMssql.Query<string>($@"SELECT Attribute2 FROM [config_db].[dbo].[xx_GPNControl] with(nolock) where Type = 'MGULabel' and Attribute1 = 'NAV' and GPN = '{ProductNameGPN}' and Status = '1'").FirstOrDefault();
                var BMWHWindex = connMssql.Query<string>($@"SELECT Attribute2 FROM [config_db].[dbo].[xx_GPNControl] with(nolock) where Type = 'MGULabel' and Attribute1 = 'BMW HW index' and GPN = '{ProductNameGPN}' and Status = '1'").FirstOrDefault();
                var Variant = connMssql.Query<string>($@"SELECT Attribute2 FROM [config_db].[dbo].[xx_GPNControl] with(nolock) where Type = 'MGULabel' and Attribute1 = 'Variant' and GPN = '{ProductNameGPN}' and Status = '1'").FirstOrDefault();
                var Process_EC = connMssql.Query<string>($@"SELECT Attribute2 FROM [config_db].[dbo].[xx_GPNControl] with(nolock) where Type = 'MGULabel' and Attribute1 = 'Process_EC' and GPN = '{ProductNameGPN}' and Status = '1'").FirstOrDefault();
                var Navigation_Country = connMssql.Query<string>($@"SELECT Attribute2 FROM [config_db].[dbo].[xx_GPNControl] with(nolock) where Type = 'MGULabel' and Attribute1 = 'Navigation_Country' and GPN = '{ProductNameGPN}' and Status = '1'").FirstOrDefault();
                var INTEL_ABL = connMssql.Query<string>($@"SELECT Attribute2 FROM [config_db].[dbo].[xx_GPNControl] with(nolock) where Type = 'MGULabel' and Attribute1 = 'INTEL_ABL' and GPN = '{ProductNameGPN}' and Status = '1'").FirstOrDefault();
                var IOC_APPL = connMssql.Query<string>($@"SELECT Attribute2 FROM [config_db].[dbo].[xx_GPNControl] with(nolock) where Type = 'MGULabel' and Attribute1 = 'IOC_APPL' and GPN = '{ProductNameGPN}' and Status = '1'").FirstOrDefault();
                var IOC_BIOS_D3 = connMssql.Query<string>($@"SELECT Attribute2 FROM [config_db].[dbo].[xx_GPNControl] with(nolock) where Type = 'MGULabel' and Attribute1 = 'IOC_BIOS_D3' and GPN = '{ProductNameGPN}' and Status = '1'").FirstOrDefault();
                var IOC_BIOS_D1 = connMssql.Query<string>($@"SELECT Attribute2 FROM [config_db].[dbo].[xx_GPNControl] with(nolock) where Type = 'MGULabel' and Attribute1 = 'IOC_BIOS_D1' and GPN = '{ProductNameGPN}' and Status = '1'").FirstOrDefault();
                var FlashDataro = connMssql.Query<string>($@"SELECT Attribute2 FROM [config_db].[dbo].[xx_GPNControl] with(nolock) where Type = 'MGULabel' and Attribute1 = 'FlashDataro' and GPN = '{ProductNameGPN}' and Status = '1'").FirstOrDefault();
                var CustomerPartNumber = connMssql.Query<string>($@"SELECT Attribute2 FROM [config_db].[dbo].[xx_GPNControl] with(nolock) where Type = 'MGULabel' and Attribute1 = 'Customer assembly number' and GPN = '{ProductNameGPN}' and Status = '1'").FirstOrDefault();
                var KeyIdentifier = connMssql.Query<string>($@"SELECT Attribute2 FROM [config_db].[dbo].[xx_GPNControl] with(nolock) where Type = 'MGULabel' and Attribute1 = 'SshKeyIdentifier' and GPN = '{ProductNameGPN}' and Status = '1'").FirstOrDefault();

                return new ProductionData
                {
                    ModelNumber = Variant,
                    SWIStep = SWIStep,
                    Process_EC = Process_EC,
                    Hardware_EC = BMWHWindex,
                    CustomerPartNumber = CustomerPartNumber,
                    SWVersion = SWVersion,
                    ProdVersion = ProdVersion,
                    NAV = NAV,
                    Prefix_UID = Prefix_UID,
                    NavigationCountry = Navigation_Country,
                    INTEL_ABL = INTEL_ABL,
                    IOC_APPL = IOC_APPL,
                    IOC_BIOS_D3 = IOC_BIOS_D3,
                    IOC_BIOS_D1 = IOC_BIOS_D1,
                    SWBuildNumber = "",
                    FlashDataro = FlashDataro,
                    SshKeyIdentifier = KeyIdentifier
                };
            }
        }

        public static List<DailyMGUTrend> GetAllDailyData()
        {
            var DailyList = new List<DailyMGUTrend>();

            try
            {
                var script = $@"SELECT P.Date,round(MAX(FCT),4)*100 AS FCT_FPR,round(MAX(RUNIN),4)*100 AS RUNIN_FPR,round(MAX(FLASHING),4)*100 AS FLASHING_FPR,round(MAX(AFT),4)*100 AS AFT_FPR,round(MAX(SFT),4)*100 AS SFT_FPR,MIN(P.Pass) as Total
                                FROM (
	                                select FinalTable.Date,FinalTable.TestStation,FinalTable.Pass,FinalTable.Total,FinalTable.FPR
	                                from
	                                (select substring(Convert(char(17),SubTable.CreatTime,120),0,11) as Date,SubTable.TestStation,SubTable.Operation, Sum(SubTable.StepResult) as Pass,Count(SubTable.StepResult) as Total ,(CAST(Sum(SubTable.StepResult) AS float)/CAST(Count(SubTable.StepResult) as float)) as FPR
	                                from
	                                (SELECT SerialNumber,TestStation,Operation,DATEADD(hour, 8, CreatTime) as CreatTime,StepResult,row_number() over(partition by SerialNumber,TestStation,Operation order by CreatTime asc) as number
	                                  FROM [MGUDB].[dbo].[MGUResult]
	                                  where CreatTime >='2020-08-10' and Operation in ('EOL','FCT','FLASHING','OP_RUN_IN','SFT','RUNIN') and OperatorID !='9999' and StepResult in (0,1)
	                                  group by SerialNumber,TestStation,Operation,CreatTime,StepResult ) SubTable
	                                  where SubTable.number=1
	                                  group by TestStation,Operation,substring(Convert(char(17),SubTable.CreatTime,120),0,11)
	                                ) FinalTable
	                                where FinalTable.Total >100
                                ) t
                                PIVOT (
	                                MAX(FPR)
	                                FOR TestStation IN ([AFT], [FLASHING], [RUNIN],[FCT],[SFT])
                                ) p
                                GROUP BY P.Date";
                using (var conn = ConnectionFactory.CreatConnection("C5"))
                {
                    DailyList = conn.Query<DailyMGUTrend>(script).ToList();
                }
                return DailyList;
            }
            catch (Exception e)
            {
                return new List<DailyMGUTrend>();
            }
        }
    }
}