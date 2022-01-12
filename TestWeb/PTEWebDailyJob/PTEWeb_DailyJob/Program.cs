using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Data.Linq.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

namespace PTEWeb_DailyJob
{
    internal class Program
    {
        private static string[] Orgs = new string[] { "T1", "T2", "T3", "T5" };
        public static DbConn DC = new DbConn();
        public static DateTime Yesterday = DateTime.Now.AddDays(-1).Date;
        public static DateTime Today = Yesterday.AddDays(1).Date;
        private static List<Models.PTEWEB_ItemNameType_ByDaily> List_Datas = new List<Models.PTEWEB_ItemNameType_ByDaily>();
        private static List<Models.PTEWEB_ItemNameType_ByDaily_TOP10_FailItem> List_FailDatas = new List<Models.PTEWEB_ItemNameType_ByDaily_TOP10_FailItem>();
        private static List<Models.PTEWEB_ItemNameType_ByDaily_TOP10_LongCycleTime> List_LongCycleTime = new List<Models.PTEWEB_ItemNameType_ByDaily_TOP10_LongCycleTime>();
        private static List<Models.PTEWEB_nonITMXP_ByDaily> List_NonITM_Records = new List<Models.PTEWEB_nonITMXP_ByDaily>();
        private static List<Models.PTEWEB_Athena_ByDaily_TOP10_FailItem> List_AthenaFailDatas = new List<Models.PTEWEB_Athena_ByDaily_TOP10_FailItem>();
        private static List<Models.PTEWEB_uTube_ByDaily> List_uTube_Records = new List<Models.PTEWEB_uTube_ByDaily>();
        private DbConn Conn = new DbConn();
        private static DataTable ItemNameTable = new DataTable();

        private static void Main(string[] args)
        {
            JobStart();
        }

        public class PTEDailyJobResult
        {
            public string Org { get; set; }
            public DateTime Date { get; set; }
            public int FYRCount { get; set; } = 0;

            public string FYRErrorString { get; set; }

            public int CycleTimeCount { get; set; } = 0;

            public string CycleTimeErrorString { get; set; }

            public int FailItmeCount { get; set; } = 0;

            public string FailItemErrorString { get; set; }
        }

        private static bool JobStart()
        {
            ItemNameTable = DC.GetTableFromSql("Select * from [ate_result].[dbo].[ItemName]", DC.T1_ConnString);
            List<PTEDailyJobResult> ListResultDatas = new List<PTEDailyJobResult>();

            Parallel.ForEach(Orgs, Org =>
            {
                PTEDailyJobResult result = new PTEDailyJobResult();
                result.Org = Org;
                string FYRErrorString = "";
                string CycleTimeErrorString = "";
                string FailItemErrorString = "";
                int FYRCount = 0;
                int CycleTimeCount = 0;
                int FailItemCount = 0;
                GetOrgDailyFYRData(Org, out FYRErrorString, out CycleTimeErrorString, out FYRCount, out CycleTimeCount);
                GetOrgDailyFailItemData(Org, out FailItemErrorString, out FailItemCount);
                result.FYRErrorString = FYRErrorString;
                result.CycleTimeErrorString = CycleTimeErrorString;
                result.FailItemErrorString = FailItemErrorString;
                result.FYRCount = FYRCount;
                result.CycleTimeCount = CycleTimeCount;
                result.FailItmeCount = FailItemCount;
                ListResultDatas.Add(result);
            });
            string FYRResult = "";
            string CycleTimeResult = "";
            string FailItemResult = "";
            processNonITMDataFYR();
            processDataFYR(out FYRResult);
            processDataCycleTime(out CycleTimeResult);
            processDataFailItem(out FailItemResult);

            SendEmail send = new SendEmail();
            send.IsHtml = true;
            send.strSubject = Yesterday.ToString("yyyy/MM/dd") + " PTE Web Daily Job 數據 ";
            string html = "<!DOCTYPE html><html><body><h1>下表為" + Yesterday.ToString("yyyy/MM/dd") + "Job數據</ h1 ><table><thead>" +
                 "<tr>" +
                  "<th>Org</th>" +
                 "<th>FYR資料錯誤</th>" +
                 "<th>FYR查詢筆數</th>" +
                 "<th>FailItem資料錯誤</th>" +
                 "<th>FailItem查詢筆數</th>" +
                  "<th>CycleTime資料錯誤</th>" +
                 "<th>CycleTime查詢筆數</th>" +
                 "</tr></thead><tbody>";
            string subhtml = "";
            foreach (PTEDailyJobResult r in ListResultDatas)
            {
                subhtml += "<tr>" +
                    "<td>" + r.Org + "</td>" +
                    "<td>" + r.FYRErrorString + "</td>" +
                    "<td>" + r.FYRCount + "</td>" +
                    "<td>" + r.FailItemErrorString + "</td>" +
                    "<td>" + r.FailItmeCount + "</td>" +
                    "<td>" + r.CycleTimeErrorString + "</td>" +
                    "<td>" + r.CycleTimeCount + "</td>" +
                    "</tr>";
            }
            html += subhtml + "</tbody></table><br/>三廠FYR資料處理結果：" + FYRResult + "<br/>三廠FailItem資料處理結果：" + FailItemResult + "<br/>三廠CycleTime資料處理結果：" + CycleTimeResult + "</body></html>";
            send.strBody = html;
            send.strTo = "jerry.hsieh@garmin.com;justin.wu@garmin.com";
            send.Send();
            return true;
        }

        private static void GetOrgDailyuTubeData(string Org)
        {
            string SQL = "SELECT G.ProductName as ItemDescription, G.ItemNameType, G.DTotal, G.DFail,G.Pass,(G.Total - G.Pass)AS Fail, isnull(G.DPass, 0) as DPass ,isnull(round(G.PassRate, 3), 0) as PassRate,G.Total,round(G.Avg_Total_Time, 3) as Avg_Total_Time,isnull(round(G.AvgPassTime, 3), 0) as AvgPassTime,isnull(round((CAST(G.DFail * 100 as float) / G.DTotal), 3), 0) as DFailRate," +
                                "isnull(round((CAST((G.Total - G.Pass) * 100 as float) / G.Total), 3), 0) as FailRate,round((1 - (isnull(round((CAST(g.DFail as float) / G.DTotal), 3), 0))) * 100, 3) as FYR, isnull(round(CAST(G.Total as float) / G.DTotal - 1, 3) * 100, 0) as RTR  ,H.TestType,H.TestType2,'FT' as Source ,'uTube' as TestStation from " +
                                "(select E.*, F.DPass, F.AvgPassTime from((select B.*, isnull(D.DFail, 0) as DFail FROM(select  distinct ProductName, ItemNameType, COUNT(distinct(SerialNumber))As DTotal, COUNT(*) as Total, Sum(CAST(Spare AS FLOAT)) / Count(*) as Avg_Total_Time, SUM(CAST(Result AS INT)) as Pass, AVG(CAST(Result AS FLOAT)) * 100 as PassRate  FROM(SELECT Result, SerialNumber, Spare, ItemNameType, ProductName  FROM[ate_db_tblfinal_new].[dbo].[TblFinal] with(nolock) " +
                                " where  tdatetime between '" + Yesterday.ToString("yyyy/MM/dd") + "' and '" + Today.ToString("yyyy/MM/dd") + "' and ItemNameType = '7778')A group by a.ProductName, a.ItemNameType)B LEFT JOIN " +
                                " (select distinct ProductName, ItemNameType, COUNT(distinct(SerialNumber))As DFail from(SELECT ItemNameType, Result, SerialNumber, ProductName  FROM[ate_db_tblfinal_new].[dbo].[TblFinal] with(nolock) " +
                                " where  tdatetime between  '" + Yesterday.ToString("yyyy/MM/dd") + "' and '" + Today.ToString("yyyy/MM/dd") + "' and ItemNameType = '7778')C  where Result = 0 group by C.ProductName, ItemNameType) D on B.ProductName = D.ProductName and B.ItemNameType = D.ItemNameType)E  LEFT JOIN(select distinct Itemnametype, ProductName, AVG(CAST(Spare AS FLOAT)) as AvgPassTime, COUNT(distinct(SerialNumber))As DPass from " +
                                " (SELECT ItemNameType, Result, Spare, SerialNumber, ProductName  FROM[ate_db_tblfinal_new].[dbo].[TblFinal] with(nolock) " +
                                " where  tdatetime between  '" + Yesterday.ToString("yyyy/MM/dd") + "' and '" + Today.ToString("yyyy/MM/dd") + "' and ItemNameType = '7778')C where Result = 1 group by C.ProductName, ItemNameType)F on E.ProductName = F.ProductName and E.ItemNameType = F.ItemNameType))G LEFT JOIN " +
                                " (select  distinct itemnametype, TestType, TestType2, ProductName from(SELECT ItemNameType, Result, TestType, TestType2, SerialNumber, ProductName  FROM[ate_db_tblfinal_new].[dbo].[TblFinal] with(nolock) " +
                                " where  tdatetime between '" + Yesterday.ToString("yyyy/MM/dd") + "' and '" + Today.ToString("yyyy/MM/dd") + "' and ItemNameType = '7778')C group by  ItemNameType, C.TestType, C.TestType2, C.ProductName)H on G.ProductName = H.ProductName and G.ItemNameType = H.ItemNameType";
            DataTable dt = new DataTable();
            switch (Org)
            {
                case "T1":
                    dt = DC.GetTableFromSql(SQL, DC.T1_ConnString);
                    List_uTube_Records.AddRange(GetuTubeListData(dt, Org));
                    break;

                case "T2":
                    dt = DC.GetTableFromSql(SQL, DC.T2_ConnString);
                    List_uTube_Records.AddRange(GetuTubeListData(dt, Org));
                    break;

                case "T3":
                    dt = DC.GetTableFromSql(SQL, DC.T3_ConnString);
                    List_uTube_Records.AddRange(GetuTubeListData(dt, Org));
                    break;

                case "T5":
                    dt = DC.GetTableFromSql(SQL, DC.T5_ConnString);
                    List_uTube_Records.AddRange(GetuTubeListData(dt, Org));
                    break;
            }
        }

        private static void GetOrgDailyNonITMData(string Org)
        {
            string SQL_Athena = " select A.ESN,A.Result,A.TestType, CASE " +
                                  " WHEN A.TestType = 'W' THEN REPLACE(B.ItemDescription, 'RTESN', 'Gsensor') " +
                                  " WHEN A.TestType = 'R' THEN REPLACE(B.ItemDescription, 'RTESN', 'Compass') " +
                                  " END AS ItemDescription,A.Spare FROM " +
                                  " (Select distinct C.SerialNumber AS ESN, C.tDateTime, C.Result, C.TestType, D.ItemNameType, C.Spare FROM " +
                                  " (select SerialNumber, tDateTime, Result, Spare, TestType FROM[ate_db_tblfinal_new].[dbo].[TblFinal] with(nolock) " +
                                  " where  tdatetime between '" + Yesterday.ToString("yyyy/MM/dd") + "' and '" + Today.ToString("yyyy/MM/dd") + "' and ExeInfo like '%Athena%'  and itemnametype in ('451', '453'))C " +
                                  " inner join " +
                                  " (select ItemNameType, SerialNumber  FROM[ate_db_tblfinal_new].[dbo].[TblFinal]where TestType2 = 183)D " +
                                  " on C.SerialNumber = D.SerialNumber)A " +
                                  " inner join(select itemnametype, ItemDescription from[ate_result].[dbo].[ItemName])B " +
                                  " on A.ItemNameType = B.ItemNameType";

            string SQL_uTube = " select A.ESN,A.Result,A.TestType, " +//7778 .501
                                 " REPLACE(B.ItemDescription, 'RTESN', 'uTube') " +
                                 " AS ItemDescription,A.Spare FROM " +
                                 " (Select distinct C.SerialNumber AS ESN, C.tDateTime, C.Result, C.TestType, D.ItemNameType, C.Spare FROM " +
                                 " (select SerialNumber, tDateTime, Result, Spare, TestType FROM[ate_db_tblfinal_new].[dbo].[TblFinal] with(nolock) " +
                                 " where  tdatetime between '" + Yesterday.ToString("yyyy/MM/dd") + "' and '" + Today.ToString("yyyy/MM/dd") + "' and  itemnametype = '7778')C " +
                                 " inner join " +
                                 " (select ItemNameType, SerialNumber  FROM[ate_db_tblfinal_new].[dbo].[TblFinal]where TestType2 = 183)D " +
                                 " on C.SerialNumber = D.SerialNumber)A " +
                                 " inner join(select itemnametype, ItemDescription from[ate_result].[dbo].[ItemName])B " +
                                 " on A.ItemNameType = B.ItemNameType";
            string SQL_baro = " select A.ESN,A.Result,A.TestType, " +  //14 B
                                 " REPLACE(B.ItemDescription, 'RTESN', 'Baro') " +
                                 " AS ItemDescription,A.Spare FROM " +
                                 " (Select distinct C.SerialNumber AS ESN, C.tDateTime, C.Result, C.TestType, D.ItemNameType, C.Spare FROM " +
                                 " (select SerialNumber, tDateTime, Result, Spare, TestType FROM[ate_db_tblfinal_new].[dbo].[TblFinal] with(nolock) " +
                                 " where  tdatetime between '" + Yesterday.ToString("yyyy/MM/dd") + "' and '" + Today.ToString("yyyy/MM/dd") + "' and itemnametype='14')C " +
                                 " inner join " +
                                 " (select ItemNameType, SerialNumber  FROM[ate_db_tblfinal_new].[dbo].[TblFinal]where TestType2 = 183)D " +
                                 " on C.SerialNumber = D.SerialNumber)A " +
                                 " inner join(select itemnametype, ItemDescription from[ate_result].[dbo].[ItemName])B " +
                                 " on A.ItemNameType = B.ItemNameType";
            string SQL_AirTight = " select A.ESN,A.Result,A.TestType, " +  //8837 .599
                                 " REPLACE(B.ItemDescription, 'RTESN', 'Airtight') " +
                                "  AS ItemDescription,A.Spare FROM " +
                                " (Select distinct C.SerialNumber AS ESN, C.tDateTime, C.Result, C.TestType, D.ItemNameType, C.Spare FROM " +
                                " (select SerialNumber, tDateTime, Result, Spare, TestType FROM[ate_db_tblfinal_new].[dbo].[TblFinal] with(nolock) " +
                                " where  tdatetime between '" + Yesterday.ToString("yyyy/MM/dd") + "' and '" + Today.ToString("yyyy/MM/dd") + "' and itemnametype ='8837')C " +
                                " inner join " +
                                " (select ItemNameType, SerialNumber  FROM[ate_db_tblfinal_new].[dbo].[TblFinal]where TestType2 = 183)D " +
                                " on C.SerialNumber = D.SerialNumber)A " +
                                " inner join(select itemnametype, ItemDescription from[ate_result].[dbo].[ItemName])B " +
                                " on A.ItemNameType = B.ItemNameType";
            DataTable dt, utube, baro, airtight = new DataTable();
            switch (Org)
            {
                case "T1":
                    dt = DC.GetTableFromSql(SQL_Athena, DC.T1_ConnString);
                    utube = DC.GetTableFromSql(SQL_uTube, DC.T1_ConnString);
                    baro = DC.GetTableFromSql(SQL_baro, DC.T1_ConnString);
                    airtight = DC.GetTableFromSql(SQL_AirTight, DC.T1_ConnString);
                    List_NonITM_Records.AddRange(GetNonITMListData(dt, Org));
                    List_NonITM_Records.AddRange(GetNonAthenaListData(utube, Org, 7778, ".", "501", "uTube"));
                    List_NonITM_Records.AddRange(GetNonAthenaListData(baro, Org, 14, "B", "", "Baro"));
                    List_NonITM_Records.AddRange(GetNonAthenaListData(airtight, Org, 8837, ".", "599", "Airtight"));
                    break;

                case "T2":
                    dt = DC.GetTableFromSql(SQL_Athena, DC.T2_ConnString);
                    utube = DC.GetTableFromSql(SQL_uTube, DC.T2_ConnString);
                    baro = DC.GetTableFromSql(SQL_baro, DC.T2_ConnString);
                    airtight = DC.GetTableFromSql(SQL_AirTight, DC.T2_ConnString);
                    List_NonITM_Records.AddRange(GetNonITMListData(dt, Org));
                    List_NonITM_Records.AddRange(GetNonAthenaListData(utube, Org, 7778, ".", "501", "uTube"));
                    List_NonITM_Records.AddRange(GetNonAthenaListData(baro, Org, 14, "B", "", "Baro"));
                    List_NonITM_Records.AddRange(GetNonAthenaListData(airtight, Org, 8837, ".", "599", "Airtight"));
                    break;

                case "T3":
                    dt = DC.GetTableFromSql(SQL_Athena, DC.T3_ConnString);
                    utube = DC.GetTableFromSql(SQL_uTube, DC.T3_ConnString);
                    baro = DC.GetTableFromSql(SQL_baro, DC.T3_ConnString);
                    airtight = DC.GetTableFromSql(SQL_AirTight, DC.T3_ConnString);
                    List_NonITM_Records.AddRange(GetNonITMListData(dt, Org));
                    List_NonITM_Records.AddRange(GetNonAthenaListData(utube, Org, 7778, ".", "501", "uTube"));
                    List_NonITM_Records.AddRange(GetNonAthenaListData(baro, Org, 14, "B", "", "Baro"));
                    List_NonITM_Records.AddRange(GetNonAthenaListData(airtight, Org, 8837, ".", "599", "Airtight"));
                    break;

                case "T5":
                    dt = DC.GetTableFromSql(SQL_Athena, DC.T5_ConnString);
                    utube = DC.GetTableFromSql(SQL_uTube, DC.T5_ConnString);
                    baro = DC.GetTableFromSql(SQL_baro, DC.T5_ConnString);
                    airtight = DC.GetTableFromSql(SQL_AirTight, DC.T5_ConnString);
                    List_NonITM_Records.AddRange(GetNonITMListData(dt, Org));
                    List_NonITM_Records.AddRange(GetNonAthenaListData(utube, Org, 7778, ".", "501", "uTube"));
                    List_NonITM_Records.AddRange(GetNonAthenaListData(baro, Org, 14, "B", "", "Baro"));
                    List_NonITM_Records.AddRange(GetNonAthenaListData(airtight, Org, 8837, ".", "599", "Airtight"));
                    break;
            }
        }

        private static List<Models.PTEWEB_uTube_ByDaily> GetuTubeListData(DataTable dataTable, string Org)
        {
            try
            {
                return dataTable.AsEnumerable().Select(x => new Models.PTEWEB_uTube_ByDaily
                {
                    Date = Yesterday,
                    Org = Org,
                    ItemNameType = x.Field<int>("ItemNameType"),
                    Description = x.Field<string>("ItemDescription"),
                    TestType = x.Field<string>("TestType"),
                    TestType2 = x.Field<string>("TestType2"),
                    Total = x.Field<int>("Total"),
                    Pass = x.Field<int>("Pass"),
                    Fail = x.Field<int>("Fail"),
                    D_Total = x.Field<int>("DTotal"),
                    D_Pass = x.Field<int>("DPass"),
                    D_Fail = x.Field<int>("DFail"),
                    Pass_Rate = x.Field<double>("PassRate"),
                    Fail_Rate = x.Field<double>("FailRate"),
                    Retry_Rate = x.Field<double>("RTR"),
                    FYR = x.Field<double>("FYR"),
                    Avg_Pass_Time = x.Field<double>("AvgPassTime"),
                    Avg_Total_Time = x.Field<double>("Avg_Total_Time"),
                    Source = x.Field<string>("Source"),
                    TestStation = x.Field<string>("TestStation")
                }).ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static List<Models.PTEWEB_nonITMXP_ByDaily> GetNonITMListData(DataTable dataTable, string Org)
        {
            try
            {
                var record = from d in dataTable.AsEnumerable()
                             group d by new { ItemDescription = d.Field<string>("ItemDescription"), TestType = d.Field<string>("TestType") }
                            into grp
                             select new Models.PTEWEB_nonITMXP_ByDaily
                             {
                                 Date = Yesterday,
                                 Org = Org,
                                 ItemNameType = GetNonITM_ItemNameType(grp.Key.ItemDescription),
                                 Description = grp.Key.ItemDescription,
                                 TestType = grp.Key.TestType,
                                 TestType2 = "",
                                 Total = grp.Count(),
                                 Fail = grp.Count() - grp.Select(x => x.Field<int>("Result")).Sum(),
                                 Pass = grp.Select(x => x.Field<int>("Result")).Sum(),
                                 D_Total = grp.Select(g => g.Field<string>("ESN")).Distinct().Count(),
                                 D_Pass = grp.Where(x => x.Field<int>("Result") == 1).Select(g => g.Field<string>("ESN")).Distinct().Count(),
                                 D_Fail = grp.Where(x => x.Field<int>("Result") == 0).Select(g => g.Field<string>("ESN")).Distinct().Count(),
                                 Pass_Rate = Math.Round(Convert.ToDouble(grp.Where(x => x.Field<int>("Result") == 1).Count() * 100) / grp.Count(), 2),
                                 Fail_Rate = Math.Round(Convert.ToDouble(grp.Count() - grp.Select(x => x.Field<int>("Result")).Sum()) * 100 / grp.Count(), 2),
                                 Retry_Rate = Math.Round((Convert.ToDouble(grp.Count()) * 100 / grp.Select(g => g.Field<string>("ESN")).Distinct().Count()) - 1, 2),
                                 FYR = Math.Round(1 - (Convert.ToDouble(grp.AsEnumerable().Where(x => x.Field<int>("Result") == 0).Select(g => g.Field<string>("ESN")).Distinct().Count()) / grp.Select(g => g.Field<string>("ESN")).Distinct().Count()), 2) * 100,
                                 Avg_Pass_Time = Math.Round(grp.Where(x => x.Field<int>("Result") == 1).Select(g => Convert.ToDouble(g.Field<string>("Spare"))).Sum() / grp.Count(), 2),
                                 Avg_Total_Time = grp.Select(x => Convert.ToDouble(x.Field<string>("Spare"))).Sum() / grp.Count(),
                                 Source = "FT",
                                 TestStation = grp.Key.TestType == "W" ? "G-Sensor" : "E-Compass"
                             };
                return record.ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static List<Models.PTEWEB_nonITMXP_ByDaily> GetNonAthenaListData(DataTable dataTable, string Org, int itemNameType, string testtype, string testtype2, string TestStation)
        {
            try
            {
                var record = from d in dataTable.AsEnumerable()
                             group d by new { ItemDescription = d.Field<string>("ItemDescription") }
                            into grp
                             select new Models.PTEWEB_nonITMXP_ByDaily
                             {
                                 Date = Yesterday,
                                 Org = Org,
                                 ItemNameType = GetNonITM_ItemNameType(grp.Key.ItemDescription),
                                 Description = grp.Key.ItemDescription,
                                 TestType = testtype,
                                 TestType2 = testtype2,
                                 Total = grp.Count(),
                                 Fail = grp.Count() - grp.Select(x => x.Field<int>("Result")).Sum(),
                                 Pass = grp.Select(x => x.Field<int>("Result")).Sum(),
                                 D_Total = grp.Select(g => g.Field<string>("ESN")).Distinct().Count(),
                                 D_Pass = grp.Where(x => x.Field<int>("Result") == 1).Select(g => g.Field<string>("ESN")).Distinct().Count(),
                                 D_Fail = grp.Where(x => x.Field<int>("Result") == 0).Select(g => g.Field<string>("ESN")).Distinct().Count(),
                                 Pass_Rate = Math.Round(Convert.ToDouble(grp.Where(x => x.Field<int>("Result") == 1).Count() * 100) / grp.Count(), 2),
                                 Fail_Rate = Math.Round(Convert.ToDouble(grp.Count() - grp.Select(x => x.Field<int>("Result")).Sum()) * 100 / grp.Count(), 2),
                                 Retry_Rate = Math.Round((Convert.ToDouble(grp.Count()) * 100 / grp.Select(g => g.Field<string>("ESN")).Distinct().Count()) - 1, 2),
                                 FYR = Math.Round(1 - (Convert.ToDouble(grp.AsEnumerable().Where(x => x.Field<int>("Result") == 0).Select(g => g.Field<string>("ESN")).Distinct().Count()) / grp.Select(g => g.Field<string>("ESN")).Distinct().Count()), 2) * 100,
                                 Avg_Pass_Time = Math.Round(grp.Where(x => x.Field<int>("Result") == 1).Select(g => Convert.ToDouble(g.Field<string>("Spare"))).Sum() / grp.Count(), 2),
                                 Avg_Total_Time = grp.Select(x => Convert.ToDouble(x.Field<string>("Spare"))).Sum() / grp.Count(),
                                 Source = "FT",
                                 TestStation = TestStation
                             };
                return record.ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static int GetNonITM_ItemNameType(string des)
        {
            DbConn connection = new DbConn();
            DataTable dt = new DataTable();
            string SQL = "SELECT * FROM [PTEDB].[dbo].[PTEWEB_nonITMXP_ItemNameType]";
            dt = connection.GetTableFromSql(SQL, connection.TxSupport_ConnString);
            int ItemNameTypeMin = dt.AsEnumerable().Select(x => x.Field<int>("ItemNameType")).Min() - 1;
            var obj = from r in dt.AsEnumerable()
                      where r.Field<string>("Product_Des") == des
                      select r;

            return obj.Count() == 0 ? InsertNewItemNameType(des, ItemNameTypeMin) : obj.FirstOrDefault().Field<int>("ItemNameType");
        }

        public static int InsertNewItemNameType(string des, int itemnametype)
        {
            string sql = "INSERT INTO [PTEDB].[dbo].[PTEWEB_nonITMXP_ItemNameType]([ItemNameType],[Product_Des])VALUES('" + itemnametype + "','" + des + "')";
            DbConn connection = new DbConn();
            connection.ExecuteSql(sql, connection.TxSupport_ConnString);
            return itemnametype;
        }

        private static void GetOrgDailyAthenaFailItemData(string Org)
        {
            DataTable dt = new DataTable();
            string SQL = "SELECT FailItem, Count(FailItem)as FailCount,TestStation,ProductName as ItemDescription,ItemNameType " +
                                    "FROM( " +
                                    "SELECT ItemNameType,failitem, case when ItemNameType = 451 then 'E-Compass' else 'G-Sensor' END AS TestStation, SerialNumber, Substring(ExeInfo, PATINDEX('%Action%', ExeInfo) + 6, PATINDEX('%.xml%', ExeInfo) - (PATINDEX('%Action%', ExeInfo) + 6))AS ProductName  FROM[ate_db_tblfinal_new].[dbo].[TblFinal] with(nolock) where tdatetime between '" + Yesterday.ToString("yyyy/MM/dd") + "' and '" + Today.ToString("yyyy/MM/dd") + "' and ExeInfo like '%Athena%'  and itemnametype in ('451','453') and result = 0 )A " +
                                    "group by FailItem, TestStation, ProductName,ItemNameType order by ProductName asc";
            switch (Org)
            {
                case "T1":
                    List_AthenaFailDatas.AddRange(GetListAthenaFailItemData(DC.GetTableFromSql(SQL, DC.T1_ConnString), Org));
                    break;

                case "T2":
                    List_AthenaFailDatas.AddRange(GetListAthenaFailItemData(DC.GetTableFromSql(SQL, DC.T2_ConnString), Org));
                    break;

                case "T3":
                    List_AthenaFailDatas.AddRange(GetListAthenaFailItemData(DC.GetTableFromSql(SQL, DC.T3_ConnString), Org));
                    break;

                case "T5":
                    List_AthenaFailDatas.AddRange(GetListAthenaFailItemData(DC.GetTableFromSql(SQL, DC.T5_ConnString), Org));
                    break;
            }
        }

        private static void GetOrgDailyFailItemData(string Org, out string FailItemErrorString, out int FailItemCount)
        {
            DataTable dt = new DataTable();
            string Table = "TestTable";
            string SQL = "Select * FROM(Select   CAST(D.FailItem as int ) as FailItem,CAST(D.FailCount as int)as FailCount,CAST(D.ItemNameType as int)as ItemNameType,E.ItemDescription FROM (Select B.ItemNameType,C.FailItem,Count(C.FailItem)AS FailCount from(Select distinct( A.ItemNametype)as ItemNameType from" +
                                     " (select ItemNametype, Count(distinct(SerialNumber)) as Total from  " + Table + "  WITH(NOLOCK) where tdatetime between '" + Yesterday.ToString("yyyy/MM/dd") + "' and '" + Today.ToString("yyyy/MM/dd") + "' and  station not like '%_%N%' and station not like '%_%Z%'   and exeinfo like  '%ITMXP.exe%'  group by ItemNameType)A where A.Total > 1)B" +
                                     "	left join " + Table + " C on B.itemNameType = C.ItemNameType and C.tdatetime between '" + Yesterday.ToString("yyyy/MM/dd") + "' and '" + Today.ToString("yyyy/MM/dd") + "' and C.station not like '%_%N%' and C.station not like '%_%Z%'   and C.exeinfo like  '%ITMXP.exe%'  and C.Result = 0 group by B.ItemNameType,C.FailItem)D" +
                                     "	  left join[ate_result].[dbo].[ItemName] E on D.itemnametype = E.itemnametype)F WHERE F.FailItem is not NULL " +
                                     " order by ItemNameType,FailCount desc";
            string SQL_CPU = SQL.Replace(Table, "[ate_db_tblcpu].[dbo].[TblCpu]");
            string SQL_Final = SQL.Replace(Table, "[ate_db_tblfinal_new].[dbo].[TblFinal]");
            FailItemErrorString = "";
            FailItemCount = 0;
            string oFailItemErrorString = "";
            int oFailItemCount = 0;

            switch (Org)
            {
                case "T1":
                    List_FailDatas.AddRange(GetListFailItemData(DC.GetTableFromSql(SQL_CPU, DC.T1_ConnString), Org, out oFailItemErrorString, out oFailItemCount));
                    FailItemCount += oFailItemCount;
                    if (oFailItemErrorString != "")
                    {
                        FailItemErrorString += oFailItemErrorString + "<br/>";
                    }

                    List_FailDatas.AddRange(GetListFailItemData(DC.GetTableFromSql(SQL_Final, DC.T1_ConnString), Org, out oFailItemErrorString, out oFailItemCount));
                    FailItemCount += oFailItemCount;
                    if (oFailItemErrorString != "")
                    {
                        FailItemErrorString += oFailItemErrorString + "<br/>";
                    }
                    break;

                case "T2":
                    List_FailDatas.AddRange(GetListFailItemData(DC.GetTableFromSql(SQL_CPU, DC.T2_ConnString), Org, out oFailItemErrorString, out oFailItemCount));
                    FailItemCount += oFailItemCount;
                    if (oFailItemErrorString != "")
                    {
                        FailItemErrorString += oFailItemErrorString + "<br/>";
                    }
                    List_FailDatas.AddRange(GetListFailItemData(DC.GetTableFromSql(SQL_Final, DC.T2_ConnString), Org, out oFailItemErrorString, out oFailItemCount));
                    FailItemCount += oFailItemCount;
                    if (oFailItemErrorString != "")
                    {
                        FailItemErrorString += oFailItemErrorString + "<br/>";
                    }
                    break;

                case "T3":
                    List_FailDatas.AddRange(GetListFailItemData(DC.GetTableFromSql(SQL_CPU, DC.T3_ConnString), Org, out oFailItemErrorString, out oFailItemCount));
                    FailItemCount += oFailItemCount;
                    if (oFailItemErrorString != "")
                    {
                        FailItemErrorString += oFailItemErrorString + "<br/>";
                    }
                    List_FailDatas.AddRange(GetListFailItemData(DC.GetTableFromSql(SQL_Final, DC.T3_ConnString), Org, out oFailItemErrorString, out oFailItemCount));
                    FailItemCount += oFailItemCount;
                    if (oFailItemErrorString != "")
                    {
                        FailItemErrorString += oFailItemErrorString + "<br/>";
                    }
                    break;

                case "T5":
                    List_FailDatas.AddRange(GetListFailItemData(DC.GetTableFromSql(SQL_CPU, DC.T5_ConnString), Org, out oFailItemErrorString, out oFailItemCount));
                    FailItemCount += oFailItemCount;
                    if (oFailItemErrorString != "")
                    {
                        FailItemErrorString += oFailItemErrorString + "<br/>";
                    }
                    List_FailDatas.AddRange(GetListFailItemData(DC.GetTableFromSql(SQL_Final, DC.T5_ConnString), Org, out oFailItemErrorString, out oFailItemCount));
                    FailItemCount += oFailItemCount;
                    if (oFailItemErrorString != "")
                    {
                        FailItemErrorString += oFailItemErrorString + "<br/>";
                    }
                    break;
            }
        }

        private static void GetOrgDailyFYRData(string Org, out string FYRErrorString, out string CycleTimeErrorString, out int FYRCount, out int CycleTimeCount)
        {
            DataTable dt = new DataTable();
            string Table = "TestTable";
            string SQL =
            $@"DECLARE @tempTable TABLE
            (
                SerialNumber char(10),
                tDateTime datetime,
                Result INT,
                Spare INT,
                ItemNameType INT,
                TestType char(1),
                TestType2 varchar(25),
                ItemDescription varchar(36)
            )
            INSERT INTO @tempTable(SerialNumber, tDateTime, Result, Spare, ItemNameType, TestType, TestType2, ItemDescription)
            SELECT A.*,B.ItemDescription FROM(SELECT SerialNumber, tDateTime, Result, Spare, ItemNameType, TestType, TestType2 FROM {Table}
            where  exeinfo like  '%ITMXP.exe%'  and tdatetime between '{Yesterday.ToString("yyyy/MM/dd")}' and '{Today.ToString("yyyy/MM/dd")}' and result <= 1)A
            INNER JOIN(SELECT ItemDescription, ItemNameType FROM[ate_result].[dbo].[ItemName])B ON A.ItemNameType = B.ItemNameType

            DECLARE @ItemNameTypeTbl Table
            (
            ItemNameType INT,
            ItemDescription varchar(36)
            )
            INSERT INTO @ItemNameTypeTbl(ItemNameType, ItemDescription)
            select Distinct(ItemNameType)as ItemNameType,ItemDescription from @tempTable
            DECLARE @PassTbltmp Table
            (
            ItemNameType INT,
             D_Pass INT,
             Pass INT,
             AvgPassTime float
            )
            INSERT INTO @PassTbltmp(ItemNameType, D_Pass, Pass, AvgPassTime)

            SELECT ItemNameType, count(Distinct(serialnumber))as D_Pass,count(serialnumber) as Pass,Round(AVG(CAST(Spare AS FLOAT)), 2) as AvgPassTime From @tempTable where result = 1 group by ItemNametype

                 DECLARE @PassTbl Table
                 (
                 ItemNameType INT,
                  D_Pass INT,
                  Pass INT,
                  AvgPassTime float
                 )
            INSERT INTO @PassTbl(ItemNameType, D_Pass, Pass, AvgPassTime)
            select a.ItemNameType,isnull(b.D_Pass, 0),isnull(b.Pass, 0),isnull(b.AvgPassTime, 0) from @ItemNameTypeTbl as a left join @PassTbltmp b on a.ItemNameType = b.ItemNameType

            DECLARE @FailTblTmp Table
            (
            ItemNameType INT,
             D_Fail INT,
             Fail INT
            )
            INSERT INTO @FailTblTmp(ItemNameType, D_Fail, Fail)
            SELECT ItemNameType, count(Distinct(serialnumber)), count(serialnumber)  From @tempTable where result = 0 group by ItemNametype

                DECLARE @FailTbl Table
                (
                ItemNameType INT,
                 D_Fail INT,
                 Fail INT

                )
            INSERT INTO @FailTbl(ItemNameType, D_Fail, Fail)
            select a.ItemNameType,isnull(b.D_Fail, 0),isnull(b.Fail, 0) from @ItemNameTypeTbl as a left join @FailTblTmp b on a.ItemNameType = b.ItemNameType

            DECLARE @TotalTbl Table
            (
            ItemNameType INT,
             D_Total INT,
             Total INT,
             PassRate Float,
             Avg_Total_Time Float,
             RTR Float
            )
            INSERT INTO @TotalTbl(ItemNameType, D_Total, Total, PassRate, Avg_Total_Time, RTR)
            SELECT ItemNameType, count(Distinct(serialnumber)), count(SerialNumber), Round(AVG(CAST(Result AS FLOAT)) * 100, 2) as PassRate,
            Round(Sum(CAST(Spare AS FLOAT)) / Count(*), 2) as Avg_Total_Time,Round(100 * ((convert(float, count(SerialNumber)) / count(Distinct(serialnumber))) - 1), 2)
             From @tempTable group by ItemNametype
            DECLARE @DistinctTestType Table
            (
            ItemNameType INT,
            TestType Char(1),
            TestType2 varchar(25)
            )
            INSERT INTO @DistinctTestType(ItemNameType, TestType, TestType2)
            SELECT ItemNameType, TestType, TestType2 From @tempTable group by ItemNametype,TestType,TestType2

              DECLARE @FYRTbl Table
              (

              ItemNameType INT,
              FYR float
              )
            INSERT INTO @FYRTbl(ItemNameType, FYR)
            Select a.ItemNameType,Round(100 * (CONVERT(float, a.Count) / CONVERT(float, b.Total)), 2) as FYR from
                   (SELECT ItemNameType, sum(result) as Count From(
                   SELECT *
                   FROM(
                   SELECT ROW_NUMBER() OVER(PARTITION BY itemnametype, serialnumber ORDER BY tDateTime) as RowNum,
                   itemnametype, serialnumber, tDateTime, Result
                   FROM @tempTable) AS T1
                   WHERE RowNum = 1)AS T2 group by ItemNameType)a inner join @TotalTbl b on a.ItemNameType = b.ItemNameType

            DECLARE @FailRateTbl Table
            (
            ItemNameType INT,
            FailRate float
            )
            INSERT INTO @FailRateTbl(ItemNameType, FailRate)
            select a.ItemNameType,isnull(round((CAST((a.Total - b.Pass) * 100 as float) / a.Total), 3), 0) from
              (SELECT * from @TotalTbl)a left join @PassTbl as b on a.ItemNameType = b.ItemNameType

            select a.*,b.D_Pass as DPass,b.Pass,b.AvgPassTime,c.D_Fail as DFail,c.Fail,d.D_Total as DTotal,d.Total ,d.PassRate,d.Avg_Total_Time,d.RTR,e.TestType,e.TestType2,f.FYR,g.FailRate from(select* from @ItemNameTypeTbl)as a
            inner join @PassTbl b on a.ItemNameType = b.ItemNameType
            inner join @FailTbl c on a.ItemNameType = c.ItemNameType
            inner join @TotalTbl d on a.ItemNameType = d.ItemNameType
            inner join @DistinctTestType e on a.ItemNameType = e.ItemNameType
            inner join @FYRTbl f on a.ItemNameType = f.ItemNameType
            inner join @FailRateTbl g on a.ItemNameType = g.ItemNameType";
            string SQL_CPU = SQL.Replace(Table, "[ate_db_tblcpu].[dbo].[TblCpu]");
            string SQL_Final = SQL.Replace(Table, "[ate_db_tblfinal_new].[dbo].[TblFinal]");
            List<int> ItemNameTypes = new List<int>();
            FYRErrorString = "";
            CycleTimeErrorString = "";
            FYRCount = 0;
            CycleTimeCount = 0;
            string oFYRErrorString = "";
            string oCycleTimeErrorString = "";
            int oFYRCount = 0;
            int oCycleTimeCount = 0;

            switch (Org)
            {
                case "T1":

                    List_Datas.AddRange(GetListData(DC.GetTableFromSql(SQL_CPU, DC.T1_ConnString), Org, "ATE", out ItemNameTypes, out oFYRErrorString, out oFYRCount));
                    if (oFYRErrorString != "")
                    {
                        FYRErrorString += oFYRErrorString + "<br/>";
                    }
                    FYRCount += oFYRCount;
                    GetSpareTimes(Org, "[ate_db_tblcpu].[dbo].[TblCpu]", ItemNameTypes, List_Datas, out oCycleTimeErrorString, out oCycleTimeCount);
                    CycleTimeCount += oCycleTimeCount;
                    if (oCycleTimeErrorString != "")
                    {
                        CycleTimeErrorString += oCycleTimeErrorString + "<br/>";
                    }

                    if (oFYRErrorString != "")
                    {
                        FYRErrorString += oFYRErrorString + "<br/>";
                    }

                    List_Datas.AddRange(GetListData(DC.GetTableFromSql(SQL_Final, DC.T1_ConnString), Org, "FT", out ItemNameTypes, out oFYRErrorString, out oFYRCount));
                    if (oFYRErrorString != "")
                    {
                        FYRErrorString += oFYRErrorString + "<br/>";
                    }
                    FYRCount += oFYRCount;
                    GetSpareTimes(Org, "[ate_db_tblfinal_new].[dbo].[TblFinal]", ItemNameTypes, List_Datas, out oCycleTimeErrorString, out oCycleTimeCount);
                    CycleTimeCount += oCycleTimeCount;
                    if (oCycleTimeErrorString != "")
                    {
                        CycleTimeErrorString += oCycleTimeErrorString + "<br/>";
                    }

                    break;

                case "T2":

                    List_Datas.AddRange(GetListData(DC.GetTableFromSql(SQL_CPU, DC.T2_ConnString), Org, "ATE", out ItemNameTypes, out oFYRErrorString, out oFYRCount));
                    if (oFYRErrorString != "")
                    {
                        FYRErrorString += oFYRErrorString + "<br/>";
                    }
                    FYRCount += oFYRCount;
                    GetSpareTimes(Org, "[ate_db_tblcpu].[dbo].[TblCpu]", ItemNameTypes, List_Datas, out oCycleTimeErrorString, out oCycleTimeCount);
                    CycleTimeCount += oCycleTimeCount;
                    if (oCycleTimeErrorString != "")
                    {
                        CycleTimeErrorString += oCycleTimeErrorString + "<br/>";
                    }

                    List_Datas.AddRange(GetListData(DC.GetTableFromSql(SQL_Final, DC.T2_ConnString), Org, "FT", out ItemNameTypes, out oFYRErrorString, out oFYRCount));
                    if (oFYRErrorString != "")
                    {
                        FYRErrorString += oFYRErrorString + "<br/>";
                    }
                    FYRCount += oFYRCount;
                    GetSpareTimes(Org, "[ate_db_tblfinal_new].[dbo].[TblFinal]", ItemNameTypes, List_Datas, out oCycleTimeErrorString, out oCycleTimeCount);
                    CycleTimeCount += oCycleTimeCount;
                    if (oCycleTimeErrorString != "")
                    {
                        CycleTimeErrorString += oCycleTimeErrorString + "<br/>";
                    }
                    break;

                case "T3":

                    List_Datas.AddRange(GetListData(DC.GetTableFromSql(SQL_CPU, DC.T3_ConnString), Org, "ATE", out ItemNameTypes, out oFYRErrorString, out oFYRCount));
                    if (oFYRErrorString != "")
                    {
                        FYRErrorString += oFYRErrorString + "<br/>";
                    }
                    FYRCount += oFYRCount;
                    GetSpareTimes(Org, "[ate_db_tblcpu].[dbo].[TblCpu]", ItemNameTypes, List_Datas, out oCycleTimeErrorString, out oCycleTimeCount);
                    CycleTimeCount += oCycleTimeCount;
                    if (oCycleTimeErrorString != "")
                    {
                        CycleTimeErrorString += oCycleTimeErrorString + "<br/>";
                    }

                    List_Datas.AddRange(GetListData(DC.GetTableFromSql(SQL_Final, DC.T3_ConnString), Org, "FT", out ItemNameTypes, out oFYRErrorString, out oFYRCount));
                    if (oFYRErrorString != "")
                    {
                        FYRErrorString += oFYRErrorString + "<br/>";
                    }
                    FYRCount += oFYRCount;
                    GetSpareTimes(Org, "[ate_db_tblfinal_new].[dbo].[TblFinal]", ItemNameTypes, List_Datas, out oCycleTimeErrorString, out oCycleTimeCount);
                    CycleTimeCount += oCycleTimeCount;
                    if (oCycleTimeErrorString != "")
                    {
                        CycleTimeErrorString += oCycleTimeErrorString + "<br/>";
                    }
                    break;

                case "T5":

                    List_Datas.AddRange(GetListData(DC.GetTableFromSql(SQL_CPU, DC.T5_ConnString), Org, "ATE", out ItemNameTypes, out oFYRErrorString, out oFYRCount));
                    if (oFYRErrorString != "")
                    {
                        FYRErrorString += oFYRErrorString + "<br/>";
                    }
                    FYRCount += oFYRCount;
                    GetSpareTimes(Org, "[ate_db_tblcpu].[dbo].[TblCpu]", ItemNameTypes, List_Datas, out oCycleTimeErrorString, out oCycleTimeCount);
                    CycleTimeCount += oCycleTimeCount;
                    if (oCycleTimeErrorString != "")
                    {
                        CycleTimeErrorString += oCycleTimeErrorString + "<br/>";
                    }

                    List_Datas.AddRange(GetListData(DC.GetTableFromSql(SQL_Final, DC.T5_ConnString), Org, "FT", out ItemNameTypes, out oFYRErrorString, out oFYRCount));
                    if (oFYRErrorString != "")
                    {
                        FYRErrorString += oFYRErrorString + "<br/>";
                    }
                    FYRCount += oFYRCount;
                    GetSpareTimes(Org, "[ate_db_tblfinal_new].[dbo].[TblFinal]", ItemNameTypes, List_Datas, out oCycleTimeErrorString, out oCycleTimeCount);
                    CycleTimeCount += oCycleTimeCount;
                    if (oCycleTimeErrorString != "")
                    {
                        CycleTimeErrorString += oCycleTimeErrorString + "<br/>";
                    }
                    break;
            }
        }

        private static List<Models.PTEWEB_Athena_ByDaily_TOP10_FailItem> GetListAthenaFailItemData(DataTable dataTable, string Org)
        {
            int rcount = 0;
            int FailItemCount = 0;
            try
            {
                List<Models.PTEWEB_Athena_ByDaily_TOP10_FailItem> Fail_Para_Datas = new List<Models.PTEWEB_Athena_ByDaily_TOP10_FailItem>();
                Models.PTEWEB_Athena_ByDaily_TOP10_FailItem Fail_Para_Data = new Models.PTEWEB_Athena_ByDaily_TOP10_FailItem();
                var resultTbl = from r in dataTable.AsEnumerable()
                                group r by new { ItemDescription = r.Field<string>("ItemDescription"), TestStation = r.Field<string>("TestStation"), ItemNmeType = r.Field<int>("ItemNameType") }
                                into g
                                select new
                                {
                                    ItemDescription = g.Key.ItemDescription,
                                    Org = Org,
                                    Date = Yesterday,
                                    Total_Fail_Count = g.ToList().Sum(x => x.Field<int>("FailCount")),
                                    Item = g.ToList()
                                    .OrderByDescending(x => x.Field<int>("FailCount")).Where(x => Convert.ToInt32(x.Field<string>("FailItem")) <= 250).Select(x => new
                                    {
                                        failitems = x.Field<string>("FailItem"),
                                        Fail_Item = ItemNameTable.AsEnumerable().Where(i => i.Field<Int16>("ItemNameType") == x.Field<int>("ItemNameType")).Select(i => x.Field<string>("FailItem") == "0" ? "Item0" : (i.Field<string>("Name" + x.Field<string>("FailItem")) == null || i.Field<string>("Name" + x.Field<string>("FailItem")) == "") ? "Item" + x.Field<string>("FailItem") : i.Field<string>("Name" + x.Field<string>("FailItem"))),
                                        Fail_Count = x.Field<int>("FailCount"),
                                        Fail_Rate = Math.Round(Convert.ToDouble(x.Field<int>("FailCount")) * 100 / g.ToList().Sum(s => s.Field<int>("FailCount")), 2),
                                    }).ToList(),
                                    Description = g.Key.ItemDescription,
                                    TestStation = g.Key.TestStation,
                                    ItemNameType = g.Key.ItemNmeType
                                };
                foreach (var r in resultTbl)
                {
                    Fail_Para_Data = new Models.PTEWEB_Athena_ByDaily_TOP10_FailItem();
                    Fail_Para_Data.Date = r.Date;
                    Fail_Para_Data.ProductName = r.Description;
                    Fail_Para_Data.TestStation = r.TestStation;
                    Fail_Para_Data.Org = Org;
                    Fail_Para_Data.Total_Fail_Count = r.Total_Fail_Count;

                    switch (r.Item.Count())
                    {
                        case 1:
                            Fail_Para_Data.No1_Fail_Item = r.Item[0].Fail_Item.ToList()[0];
                            Fail_Para_Data.No1_Fail_Count = r.Item[0].Fail_Count;
                            Fail_Para_Data.No1_Fail_Rate = r.Item[0].Fail_Rate;
                            break;

                        case 2:
                            Fail_Para_Data.No1_Fail_Item = r.Item[0].Fail_Item.ToList()[0];
                            Fail_Para_Data.No1_Fail_Count = r.Item[0].Fail_Count;
                            Fail_Para_Data.No1_Fail_Rate = r.Item[0].Fail_Rate;

                            Fail_Para_Data.No2_Fail_Item = r.Item[1].Fail_Item.ToList()[0];
                            Fail_Para_Data.No2_Fail_Count = r.Item[1].Fail_Count;
                            Fail_Para_Data.No2_Fail_Rate = r.Item[1].Fail_Rate;
                            break;

                        case 3:
                            Fail_Para_Data.No1_Fail_Item = r.Item[0].Fail_Item.ToList()[0];
                            Fail_Para_Data.No1_Fail_Count = r.Item[0].Fail_Count;
                            Fail_Para_Data.No1_Fail_Rate = r.Item[0].Fail_Rate;

                            Fail_Para_Data.No2_Fail_Item = r.Item[1].Fail_Item.ToList()[0];
                            Fail_Para_Data.No2_Fail_Count = r.Item[1].Fail_Count;
                            Fail_Para_Data.No2_Fail_Rate = r.Item[1].Fail_Rate;

                            Fail_Para_Data.No3_Fail_Item = r.Item[2].Fail_Item.ToList()[0];
                            Fail_Para_Data.No3_Fail_Count = r.Item[2].Fail_Count;
                            Fail_Para_Data.No3_Fail_Rate = r.Item[2].Fail_Rate;
                            break;

                        case 4:
                            Fail_Para_Data.No1_Fail_Item = r.Item[0].Fail_Item.ToList()[0];
                            Fail_Para_Data.No1_Fail_Count = r.Item[0].Fail_Count;
                            Fail_Para_Data.No1_Fail_Rate = r.Item[0].Fail_Rate;

                            Fail_Para_Data.No2_Fail_Item = r.Item[1].Fail_Item.ToList()[0];
                            Fail_Para_Data.No2_Fail_Count = r.Item[1].Fail_Count;
                            Fail_Para_Data.No2_Fail_Rate = r.Item[1].Fail_Rate;

                            Fail_Para_Data.No3_Fail_Item = r.Item[2].Fail_Item.ToList()[0];
                            Fail_Para_Data.No3_Fail_Count = r.Item[2].Fail_Count;
                            Fail_Para_Data.No3_Fail_Rate = r.Item[2].Fail_Rate;

                            Fail_Para_Data.No4_Fail_Item = r.Item[3].Fail_Item.ToList()[0];
                            Fail_Para_Data.No4_Fail_Count = r.Item[3].Fail_Count;
                            Fail_Para_Data.No4_Fail_Rate = r.Item[3].Fail_Rate;
                            break;

                        case 5:
                            Fail_Para_Data.No1_Fail_Item = r.Item[0].Fail_Item.ToList()[0];
                            Fail_Para_Data.No1_Fail_Count = r.Item[0].Fail_Count;
                            Fail_Para_Data.No1_Fail_Rate = r.Item[0].Fail_Rate;

                            Fail_Para_Data.No2_Fail_Item = r.Item[1].Fail_Item.ToList()[0];
                            Fail_Para_Data.No2_Fail_Count = r.Item[1].Fail_Count;
                            Fail_Para_Data.No2_Fail_Rate = r.Item[1].Fail_Rate;

                            Fail_Para_Data.No3_Fail_Item = r.Item[2].Fail_Item.ToList()[0];
                            Fail_Para_Data.No3_Fail_Count = r.Item[2].Fail_Count;
                            Fail_Para_Data.No3_Fail_Rate = r.Item[2].Fail_Rate;

                            Fail_Para_Data.No4_Fail_Item = r.Item[3].Fail_Item.ToList()[0];
                            Fail_Para_Data.No4_Fail_Count = r.Item[3].Fail_Count;
                            Fail_Para_Data.No4_Fail_Rate = r.Item[3].Fail_Rate;

                            Fail_Para_Data.No5_Fail_Item = r.Item[4].Fail_Item.ToList()[0];
                            Fail_Para_Data.No5_Fail_Count = r.Item[4].Fail_Count;
                            Fail_Para_Data.No5_Fail_Rate = r.Item[4].Fail_Rate;
                            break;

                        case 6:
                            Fail_Para_Data.No1_Fail_Item = r.Item[0].Fail_Item.ToList()[0];
                            Fail_Para_Data.No1_Fail_Count = r.Item[0].Fail_Count;
                            Fail_Para_Data.No1_Fail_Rate = r.Item[0].Fail_Rate;

                            Fail_Para_Data.No2_Fail_Item = r.Item[1].Fail_Item.ToList()[0];
                            Fail_Para_Data.No2_Fail_Count = r.Item[1].Fail_Count;
                            Fail_Para_Data.No2_Fail_Rate = r.Item[1].Fail_Rate;

                            Fail_Para_Data.No3_Fail_Item = r.Item[2].Fail_Item.ToList()[0];
                            Fail_Para_Data.No3_Fail_Count = r.Item[2].Fail_Count;
                            Fail_Para_Data.No3_Fail_Rate = r.Item[2].Fail_Rate;

                            Fail_Para_Data.No4_Fail_Item = r.Item[3].Fail_Item.ToList()[0];
                            Fail_Para_Data.No4_Fail_Count = r.Item[3].Fail_Count;
                            Fail_Para_Data.No4_Fail_Rate = r.Item[3].Fail_Rate;

                            Fail_Para_Data.No5_Fail_Item = r.Item[4].Fail_Item.ToList()[0];
                            Fail_Para_Data.No5_Fail_Count = r.Item[4].Fail_Count;
                            Fail_Para_Data.No5_Fail_Rate = r.Item[4].Fail_Rate;

                            Fail_Para_Data.No6_Fail_Item = r.Item[5].Fail_Item.ToList()[0];
                            Fail_Para_Data.No6_Fail_Count = r.Item[5].Fail_Count;
                            Fail_Para_Data.No6_Fail_Rate = r.Item[5].Fail_Rate;

                            break;

                        case 7:
                            Fail_Para_Data.No1_Fail_Item = r.Item[0].Fail_Item.ToList()[0];
                            Fail_Para_Data.No1_Fail_Count = r.Item[0].Fail_Count;
                            Fail_Para_Data.No1_Fail_Rate = r.Item[0].Fail_Rate;

                            Fail_Para_Data.No2_Fail_Item = r.Item[1].Fail_Item.ToList()[0];
                            Fail_Para_Data.No2_Fail_Count = r.Item[1].Fail_Count;
                            Fail_Para_Data.No2_Fail_Rate = r.Item[1].Fail_Rate;

                            Fail_Para_Data.No3_Fail_Item = r.Item[2].Fail_Item.ToList()[0];
                            Fail_Para_Data.No3_Fail_Count = r.Item[2].Fail_Count;
                            Fail_Para_Data.No3_Fail_Rate = r.Item[2].Fail_Rate;

                            Fail_Para_Data.No4_Fail_Item = r.Item[3].Fail_Item.ToList()[0];
                            Fail_Para_Data.No4_Fail_Count = r.Item[3].Fail_Count;
                            Fail_Para_Data.No4_Fail_Rate = r.Item[3].Fail_Rate;

                            Fail_Para_Data.No5_Fail_Item = r.Item[4].Fail_Item.ToList()[0];
                            Fail_Para_Data.No5_Fail_Count = r.Item[4].Fail_Count;
                            Fail_Para_Data.No5_Fail_Rate = r.Item[4].Fail_Rate;

                            Fail_Para_Data.No6_Fail_Item = r.Item[5].Fail_Item.ToList()[0];
                            Fail_Para_Data.No6_Fail_Count = r.Item[5].Fail_Count;
                            Fail_Para_Data.No6_Fail_Rate = r.Item[5].Fail_Rate;

                            Fail_Para_Data.No7_Fail_Item = r.Item[6].Fail_Item.ToList()[0];
                            Fail_Para_Data.No7_Fail_Count = r.Item[6].Fail_Count;
                            Fail_Para_Data.No7_Fail_Rate = r.Item[6].Fail_Rate;
                            break;

                        case 8:
                            Fail_Para_Data.No1_Fail_Item = r.Item[0].Fail_Item.ToList()[0];
                            Fail_Para_Data.No1_Fail_Count = r.Item[0].Fail_Count;
                            Fail_Para_Data.No1_Fail_Rate = r.Item[0].Fail_Rate;

                            Fail_Para_Data.No2_Fail_Item = r.Item[1].Fail_Item.ToList()[0];
                            Fail_Para_Data.No2_Fail_Count = r.Item[1].Fail_Count;
                            Fail_Para_Data.No2_Fail_Rate = r.Item[1].Fail_Rate;

                            Fail_Para_Data.No3_Fail_Item = r.Item[2].Fail_Item.ToList()[0];
                            Fail_Para_Data.No3_Fail_Count = r.Item[2].Fail_Count;
                            Fail_Para_Data.No3_Fail_Rate = r.Item[2].Fail_Rate;

                            Fail_Para_Data.No4_Fail_Item = r.Item[3].Fail_Item.ToList()[0];
                            Fail_Para_Data.No4_Fail_Count = r.Item[3].Fail_Count;
                            Fail_Para_Data.No4_Fail_Rate = r.Item[3].Fail_Rate;

                            Fail_Para_Data.No5_Fail_Item = r.Item[4].Fail_Item.ToList()[0];
                            Fail_Para_Data.No5_Fail_Count = r.Item[4].Fail_Count;
                            Fail_Para_Data.No5_Fail_Rate = r.Item[4].Fail_Rate;

                            Fail_Para_Data.No6_Fail_Item = r.Item[5].Fail_Item.ToList()[0];
                            Fail_Para_Data.No6_Fail_Count = r.Item[5].Fail_Count;
                            Fail_Para_Data.No6_Fail_Rate = r.Item[5].Fail_Rate;

                            Fail_Para_Data.No7_Fail_Item = r.Item[6].Fail_Item.ToList()[0];
                            Fail_Para_Data.No7_Fail_Count = r.Item[6].Fail_Count;
                            Fail_Para_Data.No7_Fail_Rate = r.Item[6].Fail_Rate;

                            Fail_Para_Data.No8_Fail_Item = r.Item[7].Fail_Item.ToList()[0];
                            Fail_Para_Data.No8_Fail_Count = r.Item[7].Fail_Count;
                            Fail_Para_Data.No8_Fail_Rate = r.Item[7].Fail_Rate;
                            break;

                        case 9:
                            Fail_Para_Data.No1_Fail_Item = r.Item[0].Fail_Item.ToList()[0];
                            Fail_Para_Data.No1_Fail_Count = r.Item[0].Fail_Count;
                            Fail_Para_Data.No1_Fail_Rate = r.Item[0].Fail_Rate;

                            Fail_Para_Data.No2_Fail_Item = r.Item[1].Fail_Item.ToList()[0];
                            Fail_Para_Data.No2_Fail_Count = r.Item[1].Fail_Count;
                            Fail_Para_Data.No2_Fail_Rate = r.Item[1].Fail_Rate;

                            Fail_Para_Data.No3_Fail_Item = r.Item[2].Fail_Item.ToList()[0];
                            Fail_Para_Data.No3_Fail_Count = r.Item[2].Fail_Count;
                            Fail_Para_Data.No3_Fail_Rate = r.Item[2].Fail_Rate;

                            Fail_Para_Data.No4_Fail_Item = r.Item[3].Fail_Item.ToList()[0];
                            Fail_Para_Data.No4_Fail_Count = r.Item[3].Fail_Count;
                            Fail_Para_Data.No4_Fail_Rate = r.Item[3].Fail_Rate;

                            Fail_Para_Data.No5_Fail_Item = r.Item[4].Fail_Item.ToList()[0];
                            Fail_Para_Data.No5_Fail_Count = r.Item[4].Fail_Count;
                            Fail_Para_Data.No5_Fail_Rate = r.Item[4].Fail_Rate;

                            Fail_Para_Data.No6_Fail_Item = r.Item[5].Fail_Item.ToList()[0];
                            Fail_Para_Data.No6_Fail_Count = r.Item[5].Fail_Count;
                            Fail_Para_Data.No6_Fail_Rate = r.Item[5].Fail_Rate;

                            Fail_Para_Data.No7_Fail_Item = r.Item[6].Fail_Item.ToList()[0];
                            Fail_Para_Data.No7_Fail_Count = r.Item[6].Fail_Count;
                            Fail_Para_Data.No7_Fail_Rate = r.Item[6].Fail_Rate;

                            Fail_Para_Data.No8_Fail_Item = r.Item[7].Fail_Item.ToList()[0];
                            Fail_Para_Data.No8_Fail_Count = r.Item[7].Fail_Count;
                            Fail_Para_Data.No8_Fail_Rate = r.Item[7].Fail_Rate;

                            Fail_Para_Data.No9_Fail_Item = r.Item[8].Fail_Item.ToList()[0];
                            Fail_Para_Data.No9_Fail_Count = r.Item[8].Fail_Count;
                            Fail_Para_Data.No9_Fail_Rate = r.Item[8].Fail_Rate;
                            break;

                        case 10:
                            Fail_Para_Data.No1_Fail_Item = r.Item[0].Fail_Item.ToList()[0];
                            Fail_Para_Data.No1_Fail_Count = r.Item[0].Fail_Count;
                            Fail_Para_Data.No1_Fail_Rate = r.Item[0].Fail_Rate;

                            Fail_Para_Data.No2_Fail_Item = r.Item[1].Fail_Item.ToList()[0];
                            Fail_Para_Data.No2_Fail_Count = r.Item[1].Fail_Count;
                            Fail_Para_Data.No2_Fail_Rate = r.Item[1].Fail_Rate;

                            Fail_Para_Data.No3_Fail_Item = r.Item[2].Fail_Item.ToList()[0];
                            Fail_Para_Data.No3_Fail_Count = r.Item[2].Fail_Count;
                            Fail_Para_Data.No3_Fail_Rate = r.Item[2].Fail_Rate;

                            Fail_Para_Data.No4_Fail_Item = r.Item[3].Fail_Item.ToList()[0];
                            Fail_Para_Data.No4_Fail_Count = r.Item[3].Fail_Count;
                            Fail_Para_Data.No4_Fail_Rate = r.Item[3].Fail_Rate;

                            Fail_Para_Data.No5_Fail_Item = r.Item[4].Fail_Item.ToList()[0];
                            Fail_Para_Data.No5_Fail_Count = r.Item[4].Fail_Count;
                            Fail_Para_Data.No5_Fail_Rate = r.Item[4].Fail_Rate;

                            Fail_Para_Data.No6_Fail_Item = r.Item[5].Fail_Item.ToList()[0];
                            Fail_Para_Data.No6_Fail_Count = r.Item[5].Fail_Count;
                            Fail_Para_Data.No6_Fail_Rate = r.Item[5].Fail_Rate;

                            Fail_Para_Data.No7_Fail_Item = r.Item[6].Fail_Item.ToList()[0];
                            Fail_Para_Data.No7_Fail_Count = r.Item[6].Fail_Count;
                            Fail_Para_Data.No7_Fail_Rate = r.Item[6].Fail_Rate;

                            Fail_Para_Data.No8_Fail_Item = r.Item[7].Fail_Item.ToList()[0];
                            Fail_Para_Data.No8_Fail_Count = r.Item[7].Fail_Count;
                            Fail_Para_Data.No8_Fail_Rate = r.Item[7].Fail_Rate;

                            Fail_Para_Data.No9_Fail_Item = r.Item[8].Fail_Item.ToList()[0];
                            Fail_Para_Data.No9_Fail_Count = r.Item[8].Fail_Count;
                            Fail_Para_Data.No9_Fail_Rate = r.Item[8].Fail_Rate;

                            Fail_Para_Data.No10_Fail_Item = r.Item[9].Fail_Item.ToList()[0];
                            Fail_Para_Data.No10_Fail_Count = r.Item[9].Fail_Count;
                            Fail_Para_Data.No10_Fail_Rate = r.Item[9].Fail_Rate;
                            break;

                        default:
                            if (r.Item.Count() > 10)
                            {
                                Fail_Para_Data.No1_Fail_Item = r.Item[0].Fail_Item.ToList()[0];
                                Fail_Para_Data.No1_Fail_Count = r.Item[0].Fail_Count;
                                Fail_Para_Data.No1_Fail_Rate = r.Item[0].Fail_Rate;

                                Fail_Para_Data.No2_Fail_Item = r.Item[1].Fail_Item.ToList()[0];
                                Fail_Para_Data.No2_Fail_Count = r.Item[1].Fail_Count;
                                Fail_Para_Data.No2_Fail_Rate = r.Item[1].Fail_Rate;

                                Fail_Para_Data.No3_Fail_Item = r.Item[2].Fail_Item.ToList()[0];
                                Fail_Para_Data.No3_Fail_Count = r.Item[2].Fail_Count;
                                Fail_Para_Data.No3_Fail_Rate = r.Item[2].Fail_Rate;

                                Fail_Para_Data.No4_Fail_Item = r.Item[3].Fail_Item.ToList()[0];
                                Fail_Para_Data.No4_Fail_Count = r.Item[3].Fail_Count;
                                Fail_Para_Data.No4_Fail_Rate = r.Item[3].Fail_Rate;

                                Fail_Para_Data.No5_Fail_Item = r.Item[4].Fail_Item.ToList()[0];
                                Fail_Para_Data.No5_Fail_Count = r.Item[4].Fail_Count;
                                Fail_Para_Data.No5_Fail_Rate = r.Item[4].Fail_Rate;

                                Fail_Para_Data.No6_Fail_Item = r.Item[5].Fail_Item.ToList()[0];
                                Fail_Para_Data.No6_Fail_Count = r.Item[5].Fail_Count;
                                Fail_Para_Data.No6_Fail_Rate = r.Item[5].Fail_Rate;

                                Fail_Para_Data.No7_Fail_Item = r.Item[6].Fail_Item.ToList()[0];
                                Fail_Para_Data.No7_Fail_Count = r.Item[6].Fail_Count;
                                Fail_Para_Data.No7_Fail_Rate = r.Item[6].Fail_Rate;

                                Fail_Para_Data.No8_Fail_Item = r.Item[7].Fail_Item.ToList()[0];
                                Fail_Para_Data.No8_Fail_Count = r.Item[7].Fail_Count;
                                Fail_Para_Data.No8_Fail_Rate = r.Item[7].Fail_Rate;

                                Fail_Para_Data.No9_Fail_Item = r.Item[8].Fail_Item.ToList()[0];
                                Fail_Para_Data.No9_Fail_Count = r.Item[8].Fail_Count;
                                Fail_Para_Data.No9_Fail_Rate = r.Item[8].Fail_Rate;

                                Fail_Para_Data.No10_Fail_Item = r.Item[9].Fail_Item.ToList()[0];
                                Fail_Para_Data.No10_Fail_Count = r.Item[9].Fail_Count;
                                Fail_Para_Data.No10_Fail_Rate = r.Item[9].Fail_Rate;
                            }
                            break;
                    }
                    Fail_Para_Datas.Add(Fail_Para_Data);
                    rcount++;
                }
                FailItemCount = Fail_Para_Datas.Count();
                return Fail_Para_Datas;
            }
            catch (Exception ex)
            {
                rcount = 0;

                return null;
            }
        }

        private static List<Models.PTEWEB_ItemNameType_ByDaily_TOP10_FailItem> GetListFailItemData(DataTable dataTable, string Org, out string FailItemErrorString, out int FailItemCount)
        {
            FailItemErrorString = "";
            FailItemCount = 0;
            int rcount = 0;
            try
            {
                List<Models.PTEWEB_ItemNameType_ByDaily_TOP10_FailItem> Fail_Para_Datas = new List<Models.PTEWEB_ItemNameType_ByDaily_TOP10_FailItem>();
                Models.PTEWEB_ItemNameType_ByDaily_TOP10_FailItem Fail_Para_Data = new Models.PTEWEB_ItemNameType_ByDaily_TOP10_FailItem();
                var resultTbl = from r in dataTable.AsEnumerable()
                                group r by r.Field<int>("ItemNameType")
                                into g
                                select new
                                {
                                    ItemNameType = g.Key,
                                    Org = Org,
                                    Date = Yesterday,
                                    Total_Fail_Count = g.ToList().Sum(x => x.Field<int>("FailCount")),
                                    Item = g.ToList().OrderByDescending(x => x.Field<int>("FailCount")).Where(x => x.Field<int>("FailItem") <= 250).Select(x => new
                                    {
                                        Fail_Item = ItemNameTable.AsEnumerable().Where(i => i.Field<Int16>("ItemNameType") == g.Key).Select(i => i.Field<string>("Name" + x.Field<int>("FailItem"))),
                                        Fail_Count = x.Field<int>("FailCount"),
                                        Fail_Rate = Math.Round(Convert.ToDouble(x.Field<int>("FailCount")) * 100 / g.ToList().Sum(s => s.Field<int>("FailCount")), 2),
                                    }).ToList(),
                                    Description = g.ToList()[0].Field<string>("ItemDescription")
                                };
                foreach (var r in resultTbl)
                {
                    Fail_Para_Data = new Models.PTEWEB_ItemNameType_ByDaily_TOP10_FailItem();
                    Fail_Para_Data.Date = r.Date;
                    Fail_Para_Data.Description = r.Description;
                    Fail_Para_Data.ItemNameType = r.ItemNameType;
                    Fail_Para_Data.Org = Org;
                    Fail_Para_Data.Total_Fail_Count = r.Total_Fail_Count;

                    switch (r.Item.Count())
                    {
                        case 1:
                            Fail_Para_Data.No1_Fail_Item = r.Item[0].Fail_Item.ToList()[0];
                            Fail_Para_Data.No1_Fail_Count = r.Item[0].Fail_Count;
                            Fail_Para_Data.No1_Fail_Rate = r.Item[0].Fail_Rate;
                            break;

                        case 2:
                            Fail_Para_Data.No1_Fail_Item = r.Item[0].Fail_Item.ToList()[0];
                            Fail_Para_Data.No1_Fail_Count = r.Item[0].Fail_Count;
                            Fail_Para_Data.No1_Fail_Rate = r.Item[0].Fail_Rate;

                            Fail_Para_Data.No2_Fail_Item = r.Item[1].Fail_Item.ToList()[0];
                            Fail_Para_Data.No2_Fail_Count = r.Item[1].Fail_Count;
                            Fail_Para_Data.No2_Fail_Rate = r.Item[1].Fail_Rate;
                            break;

                        case 3:
                            Fail_Para_Data.No1_Fail_Item = r.Item[0].Fail_Item.ToList()[0];
                            Fail_Para_Data.No1_Fail_Count = r.Item[0].Fail_Count;
                            Fail_Para_Data.No1_Fail_Rate = r.Item[0].Fail_Rate;

                            Fail_Para_Data.No2_Fail_Item = r.Item[1].Fail_Item.ToList()[0];
                            Fail_Para_Data.No2_Fail_Count = r.Item[1].Fail_Count;
                            Fail_Para_Data.No2_Fail_Rate = r.Item[1].Fail_Rate;

                            Fail_Para_Data.No3_Fail_Item = r.Item[2].Fail_Item.ToList()[0];
                            Fail_Para_Data.No3_Fail_Count = r.Item[2].Fail_Count;
                            Fail_Para_Data.No3_Fail_Rate = r.Item[2].Fail_Rate;
                            break;

                        case 4:
                            Fail_Para_Data.No1_Fail_Item = r.Item[0].Fail_Item.ToList()[0];
                            Fail_Para_Data.No1_Fail_Count = r.Item[0].Fail_Count;
                            Fail_Para_Data.No1_Fail_Rate = r.Item[0].Fail_Rate;

                            Fail_Para_Data.No2_Fail_Item = r.Item[1].Fail_Item.ToList()[0];
                            Fail_Para_Data.No2_Fail_Count = r.Item[1].Fail_Count;
                            Fail_Para_Data.No2_Fail_Rate = r.Item[1].Fail_Rate;

                            Fail_Para_Data.No3_Fail_Item = r.Item[2].Fail_Item.ToList()[0];
                            Fail_Para_Data.No3_Fail_Count = r.Item[2].Fail_Count;
                            Fail_Para_Data.No3_Fail_Rate = r.Item[2].Fail_Rate;

                            Fail_Para_Data.No4_Fail_Item = r.Item[3].Fail_Item.ToList()[0];
                            Fail_Para_Data.No4_Fail_Count = r.Item[3].Fail_Count;
                            Fail_Para_Data.No4_Fail_Rate = r.Item[3].Fail_Rate;
                            break;

                        case 5:
                            Fail_Para_Data.No1_Fail_Item = r.Item[0].Fail_Item.ToList()[0];
                            Fail_Para_Data.No1_Fail_Count = r.Item[0].Fail_Count;
                            Fail_Para_Data.No1_Fail_Rate = r.Item[0].Fail_Rate;

                            Fail_Para_Data.No2_Fail_Item = r.Item[1].Fail_Item.ToList()[0];
                            Fail_Para_Data.No2_Fail_Count = r.Item[1].Fail_Count;
                            Fail_Para_Data.No2_Fail_Rate = r.Item[1].Fail_Rate;

                            Fail_Para_Data.No3_Fail_Item = r.Item[2].Fail_Item.ToList()[0];
                            Fail_Para_Data.No3_Fail_Count = r.Item[2].Fail_Count;
                            Fail_Para_Data.No3_Fail_Rate = r.Item[2].Fail_Rate;

                            Fail_Para_Data.No4_Fail_Item = r.Item[3].Fail_Item.ToList()[0];
                            Fail_Para_Data.No4_Fail_Count = r.Item[3].Fail_Count;
                            Fail_Para_Data.No4_Fail_Rate = r.Item[3].Fail_Rate;

                            Fail_Para_Data.No5_Fail_Item = r.Item[4].Fail_Item.ToList()[0];
                            Fail_Para_Data.No5_Fail_Count = r.Item[4].Fail_Count;
                            Fail_Para_Data.No5_Fail_Rate = r.Item[4].Fail_Rate;
                            break;

                        case 6:
                            Fail_Para_Data.No1_Fail_Item = r.Item[0].Fail_Item.ToList()[0];
                            Fail_Para_Data.No1_Fail_Count = r.Item[0].Fail_Count;
                            Fail_Para_Data.No1_Fail_Rate = r.Item[0].Fail_Rate;

                            Fail_Para_Data.No2_Fail_Item = r.Item[1].Fail_Item.ToList()[0];
                            Fail_Para_Data.No2_Fail_Count = r.Item[1].Fail_Count;
                            Fail_Para_Data.No2_Fail_Rate = r.Item[1].Fail_Rate;

                            Fail_Para_Data.No3_Fail_Item = r.Item[2].Fail_Item.ToList()[0];
                            Fail_Para_Data.No3_Fail_Count = r.Item[2].Fail_Count;
                            Fail_Para_Data.No3_Fail_Rate = r.Item[2].Fail_Rate;

                            Fail_Para_Data.No4_Fail_Item = r.Item[3].Fail_Item.ToList()[0];
                            Fail_Para_Data.No4_Fail_Count = r.Item[3].Fail_Count;
                            Fail_Para_Data.No4_Fail_Rate = r.Item[3].Fail_Rate;

                            Fail_Para_Data.No5_Fail_Item = r.Item[4].Fail_Item.ToList()[0];
                            Fail_Para_Data.No5_Fail_Count = r.Item[4].Fail_Count;
                            Fail_Para_Data.No5_Fail_Rate = r.Item[4].Fail_Rate;

                            Fail_Para_Data.No6_Fail_Item = r.Item[5].Fail_Item.ToList()[0];
                            Fail_Para_Data.No6_Fail_Count = r.Item[5].Fail_Count;
                            Fail_Para_Data.No6_Fail_Rate = r.Item[5].Fail_Rate;

                            break;

                        case 7:
                            Fail_Para_Data.No1_Fail_Item = r.Item[0].Fail_Item.ToList()[0];
                            Fail_Para_Data.No1_Fail_Count = r.Item[0].Fail_Count;
                            Fail_Para_Data.No1_Fail_Rate = r.Item[0].Fail_Rate;

                            Fail_Para_Data.No2_Fail_Item = r.Item[1].Fail_Item.ToList()[0];
                            Fail_Para_Data.No2_Fail_Count = r.Item[1].Fail_Count;
                            Fail_Para_Data.No2_Fail_Rate = r.Item[1].Fail_Rate;

                            Fail_Para_Data.No3_Fail_Item = r.Item[2].Fail_Item.ToList()[0];
                            Fail_Para_Data.No3_Fail_Count = r.Item[2].Fail_Count;
                            Fail_Para_Data.No3_Fail_Rate = r.Item[2].Fail_Rate;

                            Fail_Para_Data.No4_Fail_Item = r.Item[3].Fail_Item.ToList()[0];
                            Fail_Para_Data.No4_Fail_Count = r.Item[3].Fail_Count;
                            Fail_Para_Data.No4_Fail_Rate = r.Item[3].Fail_Rate;

                            Fail_Para_Data.No5_Fail_Item = r.Item[4].Fail_Item.ToList()[0];
                            Fail_Para_Data.No5_Fail_Count = r.Item[4].Fail_Count;
                            Fail_Para_Data.No5_Fail_Rate = r.Item[4].Fail_Rate;

                            Fail_Para_Data.No6_Fail_Item = r.Item[5].Fail_Item.ToList()[0];
                            Fail_Para_Data.No6_Fail_Count = r.Item[5].Fail_Count;
                            Fail_Para_Data.No6_Fail_Rate = r.Item[5].Fail_Rate;

                            Fail_Para_Data.No7_Fail_Item = r.Item[6].Fail_Item.ToList()[0];
                            Fail_Para_Data.No7_Fail_Count = r.Item[6].Fail_Count;
                            Fail_Para_Data.No7_Fail_Rate = r.Item[6].Fail_Rate;
                            break;

                        case 8:
                            Fail_Para_Data.No1_Fail_Item = r.Item[0].Fail_Item.ToList()[0];
                            Fail_Para_Data.No1_Fail_Count = r.Item[0].Fail_Count;
                            Fail_Para_Data.No1_Fail_Rate = r.Item[0].Fail_Rate;

                            Fail_Para_Data.No2_Fail_Item = r.Item[1].Fail_Item.ToList()[0];
                            Fail_Para_Data.No2_Fail_Count = r.Item[1].Fail_Count;
                            Fail_Para_Data.No2_Fail_Rate = r.Item[1].Fail_Rate;

                            Fail_Para_Data.No3_Fail_Item = r.Item[2].Fail_Item.ToList()[0];
                            Fail_Para_Data.No3_Fail_Count = r.Item[2].Fail_Count;
                            Fail_Para_Data.No3_Fail_Rate = r.Item[2].Fail_Rate;

                            Fail_Para_Data.No4_Fail_Item = r.Item[3].Fail_Item.ToList()[0];
                            Fail_Para_Data.No4_Fail_Count = r.Item[3].Fail_Count;
                            Fail_Para_Data.No4_Fail_Rate = r.Item[3].Fail_Rate;

                            Fail_Para_Data.No5_Fail_Item = r.Item[4].Fail_Item.ToList()[0];
                            Fail_Para_Data.No5_Fail_Count = r.Item[4].Fail_Count;
                            Fail_Para_Data.No5_Fail_Rate = r.Item[4].Fail_Rate;

                            Fail_Para_Data.No6_Fail_Item = r.Item[5].Fail_Item.ToList()[0];
                            Fail_Para_Data.No6_Fail_Count = r.Item[5].Fail_Count;
                            Fail_Para_Data.No6_Fail_Rate = r.Item[5].Fail_Rate;

                            Fail_Para_Data.No7_Fail_Item = r.Item[6].Fail_Item.ToList()[0];
                            Fail_Para_Data.No7_Fail_Count = r.Item[6].Fail_Count;
                            Fail_Para_Data.No7_Fail_Rate = r.Item[6].Fail_Rate;

                            Fail_Para_Data.No8_Fail_Item = r.Item[7].Fail_Item.ToList()[0];
                            Fail_Para_Data.No8_Fail_Count = r.Item[7].Fail_Count;
                            Fail_Para_Data.No8_Fail_Rate = r.Item[7].Fail_Rate;
                            break;

                        case 9:
                            Fail_Para_Data.No1_Fail_Item = r.Item[0].Fail_Item.ToList()[0];
                            Fail_Para_Data.No1_Fail_Count = r.Item[0].Fail_Count;
                            Fail_Para_Data.No1_Fail_Rate = r.Item[0].Fail_Rate;

                            Fail_Para_Data.No2_Fail_Item = r.Item[1].Fail_Item.ToList()[0];
                            Fail_Para_Data.No2_Fail_Count = r.Item[1].Fail_Count;
                            Fail_Para_Data.No2_Fail_Rate = r.Item[1].Fail_Rate;

                            Fail_Para_Data.No3_Fail_Item = r.Item[2].Fail_Item.ToList()[0];
                            Fail_Para_Data.No3_Fail_Count = r.Item[2].Fail_Count;
                            Fail_Para_Data.No3_Fail_Rate = r.Item[2].Fail_Rate;

                            Fail_Para_Data.No4_Fail_Item = r.Item[3].Fail_Item.ToList()[0];
                            Fail_Para_Data.No4_Fail_Count = r.Item[3].Fail_Count;
                            Fail_Para_Data.No4_Fail_Rate = r.Item[3].Fail_Rate;

                            Fail_Para_Data.No5_Fail_Item = r.Item[4].Fail_Item.ToList()[0];
                            Fail_Para_Data.No5_Fail_Count = r.Item[4].Fail_Count;
                            Fail_Para_Data.No5_Fail_Rate = r.Item[4].Fail_Rate;

                            Fail_Para_Data.No6_Fail_Item = r.Item[5].Fail_Item.ToList()[0];
                            Fail_Para_Data.No6_Fail_Count = r.Item[5].Fail_Count;
                            Fail_Para_Data.No6_Fail_Rate = r.Item[5].Fail_Rate;

                            Fail_Para_Data.No7_Fail_Item = r.Item[6].Fail_Item.ToList()[0];
                            Fail_Para_Data.No7_Fail_Count = r.Item[6].Fail_Count;
                            Fail_Para_Data.No7_Fail_Rate = r.Item[6].Fail_Rate;

                            Fail_Para_Data.No8_Fail_Item = r.Item[7].Fail_Item.ToList()[0];
                            Fail_Para_Data.No8_Fail_Count = r.Item[7].Fail_Count;
                            Fail_Para_Data.No8_Fail_Rate = r.Item[7].Fail_Rate;

                            Fail_Para_Data.No9_Fail_Item = r.Item[8].Fail_Item.ToList()[0];
                            Fail_Para_Data.No9_Fail_Count = r.Item[8].Fail_Count;
                            Fail_Para_Data.No9_Fail_Rate = r.Item[8].Fail_Rate;
                            break;

                        case 10:
                            Fail_Para_Data.No1_Fail_Item = r.Item[0].Fail_Item.ToList()[0];
                            Fail_Para_Data.No1_Fail_Count = r.Item[0].Fail_Count;
                            Fail_Para_Data.No1_Fail_Rate = r.Item[0].Fail_Rate;

                            Fail_Para_Data.No2_Fail_Item = r.Item[1].Fail_Item.ToList()[0];
                            Fail_Para_Data.No2_Fail_Count = r.Item[1].Fail_Count;
                            Fail_Para_Data.No2_Fail_Rate = r.Item[1].Fail_Rate;

                            Fail_Para_Data.No3_Fail_Item = r.Item[2].Fail_Item.ToList()[0];
                            Fail_Para_Data.No3_Fail_Count = r.Item[2].Fail_Count;
                            Fail_Para_Data.No3_Fail_Rate = r.Item[2].Fail_Rate;

                            Fail_Para_Data.No4_Fail_Item = r.Item[3].Fail_Item.ToList()[0];
                            Fail_Para_Data.No4_Fail_Count = r.Item[3].Fail_Count;
                            Fail_Para_Data.No4_Fail_Rate = r.Item[3].Fail_Rate;

                            Fail_Para_Data.No5_Fail_Item = r.Item[4].Fail_Item.ToList()[0];
                            Fail_Para_Data.No5_Fail_Count = r.Item[4].Fail_Count;
                            Fail_Para_Data.No5_Fail_Rate = r.Item[4].Fail_Rate;

                            Fail_Para_Data.No6_Fail_Item = r.Item[5].Fail_Item.ToList()[0];
                            Fail_Para_Data.No6_Fail_Count = r.Item[5].Fail_Count;
                            Fail_Para_Data.No6_Fail_Rate = r.Item[5].Fail_Rate;

                            Fail_Para_Data.No7_Fail_Item = r.Item[6].Fail_Item.ToList()[0];
                            Fail_Para_Data.No7_Fail_Count = r.Item[6].Fail_Count;
                            Fail_Para_Data.No7_Fail_Rate = r.Item[6].Fail_Rate;

                            Fail_Para_Data.No8_Fail_Item = r.Item[7].Fail_Item.ToList()[0];
                            Fail_Para_Data.No8_Fail_Count = r.Item[7].Fail_Count;
                            Fail_Para_Data.No8_Fail_Rate = r.Item[7].Fail_Rate;

                            Fail_Para_Data.No9_Fail_Item = r.Item[8].Fail_Item.ToList()[0];
                            Fail_Para_Data.No9_Fail_Count = r.Item[8].Fail_Count;
                            Fail_Para_Data.No9_Fail_Rate = r.Item[8].Fail_Rate;

                            Fail_Para_Data.No10_Fail_Item = r.Item[9].Fail_Item.ToList()[0];
                            Fail_Para_Data.No10_Fail_Count = r.Item[9].Fail_Count;
                            Fail_Para_Data.No10_Fail_Rate = r.Item[9].Fail_Rate;
                            break;

                        default:
                            if (r.Item.Count() > 10)
                            {
                                Fail_Para_Data.No1_Fail_Item = r.Item[0].Fail_Item.ToList()[0];
                                Fail_Para_Data.No1_Fail_Count = r.Item[0].Fail_Count;
                                Fail_Para_Data.No1_Fail_Rate = r.Item[0].Fail_Rate;

                                Fail_Para_Data.No2_Fail_Item = r.Item[1].Fail_Item.ToList()[0];
                                Fail_Para_Data.No2_Fail_Count = r.Item[1].Fail_Count;
                                Fail_Para_Data.No2_Fail_Rate = r.Item[1].Fail_Rate;

                                Fail_Para_Data.No3_Fail_Item = r.Item[2].Fail_Item.ToList()[0];
                                Fail_Para_Data.No3_Fail_Count = r.Item[2].Fail_Count;
                                Fail_Para_Data.No3_Fail_Rate = r.Item[2].Fail_Rate;

                                Fail_Para_Data.No4_Fail_Item = r.Item[3].Fail_Item.ToList()[0];
                                Fail_Para_Data.No4_Fail_Count = r.Item[3].Fail_Count;
                                Fail_Para_Data.No4_Fail_Rate = r.Item[3].Fail_Rate;

                                Fail_Para_Data.No5_Fail_Item = r.Item[4].Fail_Item.ToList()[0];
                                Fail_Para_Data.No5_Fail_Count = r.Item[4].Fail_Count;
                                Fail_Para_Data.No5_Fail_Rate = r.Item[4].Fail_Rate;

                                Fail_Para_Data.No6_Fail_Item = r.Item[5].Fail_Item.ToList()[0];
                                Fail_Para_Data.No6_Fail_Count = r.Item[5].Fail_Count;
                                Fail_Para_Data.No6_Fail_Rate = r.Item[5].Fail_Rate;

                                Fail_Para_Data.No7_Fail_Item = r.Item[6].Fail_Item.ToList()[0];
                                Fail_Para_Data.No7_Fail_Count = r.Item[6].Fail_Count;
                                Fail_Para_Data.No7_Fail_Rate = r.Item[6].Fail_Rate;

                                Fail_Para_Data.No8_Fail_Item = r.Item[7].Fail_Item.ToList()[0];
                                Fail_Para_Data.No8_Fail_Count = r.Item[7].Fail_Count;
                                Fail_Para_Data.No8_Fail_Rate = r.Item[7].Fail_Rate;

                                Fail_Para_Data.No9_Fail_Item = r.Item[8].Fail_Item.ToList()[0];
                                Fail_Para_Data.No9_Fail_Count = r.Item[8].Fail_Count;
                                Fail_Para_Data.No9_Fail_Rate = r.Item[8].Fail_Rate;

                                Fail_Para_Data.No10_Fail_Item = r.Item[9].Fail_Item.ToList()[0];
                                Fail_Para_Data.No10_Fail_Count = r.Item[9].Fail_Count;
                                Fail_Para_Data.No10_Fail_Rate = r.Item[9].Fail_Rate;
                            }
                            break;
                    }
                    Fail_Para_Datas.Add(Fail_Para_Data);
                    rcount++;
                }
                FailItemCount = Fail_Para_Datas.Count();
                return Fail_Para_Datas;
            }
            catch (Exception ex)
            {
                rcount = 0;
                FailItemCount = 0;
                FailItemErrorString = Org + " FailItem Get Error " + ex.ToString();
                return null;
            }
        }

        private static List<Models.PTEWEB_ItemNameType_ByDaily> GetListData(DataTable dataTable, string Org, string TestType, out List<int> ItemNameTypes, out string ErrorString, out int FYRCount)
        {
            try
            {
                ErrorString = "";
                ItemNameTypes = new List<int>();
                ItemNameTypes = dataTable.AsEnumerable().Select(x => x.Field<int>("ItemNameType")).ToList();
                FYRCount = dataTable.AsEnumerable().Count();
                return dataTable.AsEnumerable().Select(x => new Models.PTEWEB_ItemNameType_ByDaily
                {
                    Date = Yesterday,
                    Org = Org,
                    ItemNameType = x.Field<int>("ItemNameType"),
                    Description = x.Field<string>("ItemDescription"),
                    TestType = x.Field<string>("TestType"),
                    TestType2 = x.Field<string>("TestType2"),
                    Total = x.Field<int>("Total"),
                    Pass = x.Field<int>("Pass"),
                    Fail = x.Field<int>("Total") - x.Field<int>("Pass"),
                    D_Total = x.Field<int>("DTotal"),
                    D_Pass = x.Field<int>("DPass"),
                    D_Fail = x.Field<int>("DFail"),
                    Pass_Rate = x.Field<double>("PassRate"),
                    Fail_Rate = x.Field<double>("FailRate"),
                    Retry_Rate = x.Field<double>("RTR"),
                    FYR = x.Field<double>("FYR"),
                    Avg_Pass_Time = x.Field<double>("AvgPassTime"),
                    Avg_Total_Time = x.Field<double>("Avg_Total_Time"),
                    Source = TestType,
                }).ToList();
            }
            catch (Exception ex)
            {
                FYRCount = 0;
                ItemNameTypes = new List<int>();
                ErrorString = Org + " FYR Get List Data Error " + ex.ToString();
                return null;
            }
        }

        private static void processuTubeDataFYR()
        {
            {
                var InsertDatas = from r in List_uTube_Records
                                  group r by new { r.Org, r.Description }
                                                  into g
                                  select new
                                  {
                                      Records = g.ToList().FirstOrDefault()
                                  };
                DC.SetTransactionsuTubeFYR(InsertDatas.AsEnumerable().Select(x => x.Records).ToList());
            }
        }

        private static void processNonITMDataFYR()
        {
            var InsertDatas = from r in List_NonITM_Records
                              group r by new { r.Org, r.Description, }
                                              into g
                              select new
                              {
                                  Records = g.ToList().FirstOrDefault()
                              };
            DC.SetTransactionsNonITMFYR(InsertDatas.AsEnumerable().Select(x => x.Records).ToList());
        }

        private static void processDataFYR(out string FYRResult)
        {
            var InsertDatas = from r in List_Datas
                              group r by new { r.Org, r.ItemNameType, }
                                              into g
                              select new
                              {
                                  Records = g.ToList().FirstOrDefault()
                              };
            DC.SetTransactionsFYR(InsertDatas.AsEnumerable().Select(x => x.Records).ToList(), out FYRResult);
        }

        private static void processDataCycleTime(out string CycleTimeResult)
        {
            DC.SetTransactionsCycleTime(List_LongCycleTime, out CycleTimeResult);
        }

        private static void GetSpareTimes(string Org, string TargetTestTable, List<int> ItemNameTypeList, List<Models.PTEWEB_ItemNameType_ByDaily> DailyFYR, out string CycleTimeErrorString, out int CycleTimeCount)
        {
            CycleTimeErrorString = "";
            System.Configuration.ConnectionStringSettings conn = new System.Configuration.ConnectionStringSettings();
            switch (Org)
            {
                case "T1":
                    conn = DC.T1_ConnString;
                    break;

                case "T2":
                    conn = DC.T2_ConnString;
                    break;

                case "T3":
                    conn = DC.T3_ConnString;
                    break;

                case "T5":
                    conn = DC.T5_ConnString;
                    break;
            }
            try
            {
                foreach (int ItemNameType in ItemNameTypeList)
                {
                    Dictionary<string, string> ItemName_DBIndex = new Dictionary<string, string>();
                    ITMXPServerQuery queryitmxp = new ITMXPServerQuery();
                    string ITMXPQueryString = "select * from itmxp.testfile,itmxp.tbl_testinfo where itmxp.testfile.unikey = itmxp.tbl_testinfo.Idx_file and itmxp.tbl_testinfo.ItemNameType = '" + ItemNameType + "' order by unikey desc";
                    DataTable tempINITable = queryitmxp.QueryResult(ITMXPQueryString);
                    string CurrentIdx_file = "0";
                    for (int i = 0; i < tempINITable.Rows.Count; i++)
                    {
                        CurrentIdx_file = tempINITable.Rows[i]["Idx_file"].ToString();
                    }
                    //再查詢最新的Release或pilot run版本
                    ITMXPQueryString = "Select * from itmxp.tbl_fileversion where itmxp.tbl_fileversion.Idx_file = '" + CurrentIdx_file + "' and itmxp.tbl_fileversion.IsReleased = '1' order by AutoVersion desc limit 1";
                    string NeweastVersion = "0";
                    tempINITable = queryitmxp.QueryResult(ITMXPQueryString);
                    if (tempINITable.Rows.Count == 0)
                    {
                        ITMXPQueryString = "Select * from itmxp.tbl_fileversion where itmxp.tbl_fileversion.Idx_file = '" + CurrentIdx_file + "' order by AutoVersion desc limit 1";
                        tempINITable = queryitmxp.QueryResult(ITMXPQueryString);
                    }
                    for (int i = 0; i < tempINITable.Rows.Count; i++)
                    {
                        NeweastVersion = tempINITable.Rows[i]["AutoVersion"].ToString();
                    }
                    //查詢使用了哪些測試項目
                    ITMXPQueryString = @"select itmxp.testfile.XMLName ,itmxp.tbl_fileversion.AutoVersion ,itmxp.tbl_fileversion.IsReleased,itmxp.tbl_testitem.DBIndex, itmxp.tbl_testitem.Double1,itmxp.tbl_testitem.Double2,itmxp.tbl_testitem.Unit
							   from itmxp.tbl_testitem inner join itmxp.tbl_fileversion on ( itmxp.tbl_testitem.Idx_Version = itmxp.tbl_fileversion.Idx_Version )
							   inner join itmxp.testfile on ( itmxp.tbl_fileversion.Idx_file = itmxp.testfile.unikey )
							   where itmxp.testfile.unikey = '" + CurrentIdx_file + @"' and itmxp.tbl_fileversion.AutoVersion='" + NeweastVersion + "' and(itmxp.tbl_testitem.DBIndex<>0 or itmxp.tbl_testitem.DBIndex<>null )   group by itmxp.tbl_testitem.DBIndex";
                    tempINITable = queryitmxp.QueryResult(ITMXPQueryString);

                    DataTable ItemNamesTable = ItemNameTable.AsEnumerable().Where(x => x.Field<Int16>("ItemNameType") == ItemNameType).CopyToDataTable();

                    for (int i = 1; i <= tempINITable.Rows.Count - 1; i++)
                    {
                        if (Convert.ToInt32(tempINITable.Rows[i]["DBIndex"]) <= 250)
                        {
                            ItemName_DBIndex.Add(i.ToString(), ItemNamesTable.Rows[0]["Name" + tempINITable.Rows[i]["DBIndex"].ToString()].ToString());
                        }
                    }
                    DataTable SpareRetryAVGTable = new DataTable();
                    SpareRetryAVGTable.Columns.Add("DBIndex", typeof(float));
                    SpareRetryAVGTable.Columns.Add("ItemDescription", typeof(string));
                    SpareRetryAVGTable.Columns.Add("MaxSpare(PASS)", typeof(float));
                    SpareRetryAVGTable.Columns.Add("MinSpare(PASS)", typeof(float));
                    SpareRetryAVGTable.Columns.Add("AVGSpare(PASS)", typeof(float));
                    SpareRetryAVGTable.Columns.Add("MaxRetry(PASS)", typeof(float));
                    SpareRetryAVGTable.Columns.Add("MinRetry(PASS)", typeof(float));
                    SpareRetryAVGTable.Columns.Add("AVGRetry(PASS)", typeof(float));
                    string AVG_Spare_Retry_SQL = "";
                    int Item151 = 0;
                    for (int i = 1; i < tempINITable.Rows.Count; i++)
                    {
                        if (Convert.ToInt16(tempINITable.Rows[i]["DBIndex"]) > 150)
                        {
                            Item151 = 1;
                            if (Convert.ToInt16(tempINITable.Rows[i]["DBIndex"]) <= 250)
                            {
                                AVG_Spare_Retry_SQL = AVG_Spare_Retry_SQL + "AVG(CAST(" + "C.Spare" + tempINITable.Rows[i]["DBIndex"].ToString() + " AS FLOAT)) AS AvgSpareValue" + i.ToString() +
                                              ",MAX(" + "C.Spare" + tempINITable.Rows[i]["DBIndex"].ToString() + ")AS MaxSpareValue" + i.ToString() + ", Min(" + "C.Spare" + tempINITable.Rows[i]["DBIndex"].ToString() + ") AS MinSpareValue" + i.ToString() +
                                              " ,AVG(CAST(" + "C.Retry" + tempINITable.Rows[i]["DBIndex"].ToString() + " AS FLOAT)) AS AvgRetryValue" + i.ToString() +
                                              ",MAX(" + "C.Retry" + tempINITable.Rows[i]["DBIndex"].ToString() + ")AS MaxRetryValue" + i.ToString() + ", Min(" + "C.Retry" + tempINITable.Rows[i]["DBIndex"].ToString() + ") AS MinRetryValue" + i.ToString(); ;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            AVG_Spare_Retry_SQL = AVG_Spare_Retry_SQL + "AVG(CAST(" + "B.Spare" + tempINITable.Rows[i]["DBIndex"].ToString() + " AS FLOAT)) AS AvgSpareValue" + i.ToString() +
                                              ",MAX(" + "B.Spare" + tempINITable.Rows[i]["DBIndex"].ToString() + ")AS MaxSpareValue" + i.ToString() + ", Min(" + "B.Spare" + tempINITable.Rows[i]["DBIndex"].ToString() + ") AS MinSpareValue" + i.ToString() +
                                              " ,AVG(CAST(" + "B.Retry" + tempINITable.Rows[i]["DBIndex"].ToString() + " AS FLOAT)) AS AvgRetryValue" + i.ToString() +
                                              ",MAX(" + "B.Retry" + tempINITable.Rows[i]["DBIndex"].ToString() + ")AS MaxRetryValue" + i.ToString() + ", Min(" + "B.Retry" + tempINITable.Rows[i]["DBIndex"].ToString() + ") AS MinRetryValue" + i.ToString(); ;
                        }
                        if (i != tempINITable.Rows.Count - 1)
                            AVG_Spare_Retry_SQL = AVG_Spare_Retry_SQL + ",";
                    }
                    string SelectItemValueSQL = "";
                    AVG_Spare_Retry_SQL = AVG_Spare_Retry_SQL.TrimEnd(',');
                    if (Item151 == 0)
                    {
                        SelectItemValueSQL = "Select " + AVG_Spare_Retry_SQL + " from((Select SerialNumber,tdatetime,station  FROM " + TargetTestTable + " WHERE " + TargetTestTable + ".ItemNameType = '" + ItemNameType + "'" + " and " + TargetTestTable + ".Result = '1'" + "and station not like '%_%N%' and station not like '%_%Z%' and exeinfo like  '%ITMXP.exe%'  and tdatetime between'" + Yesterday.ToString("yyyy/MM/dd") + "' and '" + Today.ToString("yyyy/MM/dd") + "') AS A LEFT OUTER JOIN TblTestTime AS B on A.serialnumber = B.ESN and A.Station = B.Station and A.tDateTime = B.tDateTime)";
                    }
                    else
                    {
                        SelectItemValueSQL = "Select " + AVG_Spare_Retry_SQL + " from((Select SerialNumber,tdatetime,station  FROM " + TargetTestTable + " WHERE " + TargetTestTable + ".ItemNameType = '" + ItemNameType + "'" + " and " + TargetTestTable + ".Result = '1'" + "and station not like '%_%N%' and station not like '%_%Z%' and exeinfo like  '%ITMXP.exe%'  and tdatetime between'" + Yesterday.ToString("yyyy/MM/dd") + "' and '" + Today.ToString("yyyy/MM/dd") + "') AS A LEFT OUTER JOIN TblTestTime AS B on A.serialnumber = B.ESN and A.Station = B.Station and A.tDateTime = B.tDateTime  LEFT OUTER JOIN TblTestTime2 C on A.Station = C.Station and A.tDateTime = C.tDateTime and A.serialnumber = C.ESN)";
                    }
                    DataTable SpareRetryTable = DC.GetTableFromSql(SelectItemValueSQL, conn);

                    for (int i = 0; i < tempINITable.Rows.Count - 1; i++)
                    {
                        if (Convert.ToInt16(tempINITable.Rows[i]["DBIndex"]) <= 250)
                        {
                            DataRow tempRow = SpareRetryAVGTable.NewRow();
                            tempRow["DBIndex"] = (i + 1);
                            if ((i + 1) > ItemName_DBIndex.Count)
                                continue;
                            if (ItemName_DBIndex[(i + 1).ToString()].ToString() == string.Empty)
                                continue;
                            tempRow["ItemDescription"] = ItemName_DBIndex[(i + 1).ToString()].ToString();
                            tempRow["MaxSpare(PASS)"] = SpareRetryTable.Rows[0]["MaxSpareValue" + (i + 1).ToString()];
                            tempRow["MinSpare(PASS)"] = SpareRetryTable.Rows[0]["MinSpareValue" + (i + 1).ToString()];
                            tempRow["AVGSpare(PASS)"] = SpareRetryTable.Rows[0]["AvgSpareValue" + (i + 1).ToString()];
                            tempRow["MaxRetry(PASS)"] = SpareRetryTable.Rows[0]["MaxRetryValue" + (i + 1).ToString()];
                            tempRow["MinRetry(PASS)"] = SpareRetryTable.Rows[0]["MinRetryValue" + (i + 1).ToString()];
                            tempRow["AVGRetry(PASS)"] = SpareRetryTable.Rows[0]["AvgRetryValue" + (i + 1).ToString()];
                            if (tempRow["MaxSpare(PASS)"] != DBNull.Value)
                            {
                                SpareRetryAVGTable.Rows.Add(tempRow);
                            }
                        }
                    }
                    if (SpareRetryAVGTable.Rows.Count == 0)
                    {
                        continue;
                    }
                    SpareRetryAVGTable = SpareRetryAVGTable.AsEnumerable().OrderByDescending(x => x.Field<Single>("AVGSpare(PASS)")).CopyToDataTable();
                    Models.PTEWEB_ItemNameType_ByDaily_TOP10_LongCycleTime LCT = new Models.PTEWEB_ItemNameType_ByDaily_TOP10_LongCycleTime();
                    LCT.Date = Yesterday;
                    LCT.ItemNameType = ItemNameType;
                    LCT.Org = Org;
                    LCT.Discription = ItemNameTable.AsEnumerable().Where(x => x.Field<Int16>("ItemNameType") == ItemNameType).Select(x => x.Field<string>("ItemDescription")).First();
                    LCT.Avg_Pass_Time = DailyFYR.AsEnumerable().Where(x => x.ItemNameType == ItemNameType && x.Org == Org).Select(x => x.Avg_Pass_Time).First();
                    LCT.Avg_Total_Time = DailyFYR.AsEnumerable().Where(x => x.ItemNameType == ItemNameType && x.Org == Org).Select(x => x.Avg_Total_Time).First();
                    LCT.NO1_Item = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<string>("ItemDescription")).Count() > 1 ? SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<string>("ItemDescription")).ToList()[0] : null;
                    LCT.NO1_CycleTime = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).Count() > 1 ? Math.Round(SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).ToList()[0] / 1000, 2) : 0.0;
                    LCT.NO1_Percent = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).Count() > 1 ? Math.Round(SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).ToList()[0] / 1000, 2) / LCT.Avg_Total_Time : 0.0;
                    LCT.NO1_Percent = double.IsInfinity(Convert.ToDouble(LCT.NO1_Percent)) ? 0 : Math.Round(Convert.ToDouble(LCT.NO1_Percent * 100), 2);
                    LCT.NO2_Item = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<string>("ItemDescription")).Count() > 2 ? SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<string>("ItemDescription")).ToList()[1] : null;
                    LCT.NO2_CycleTime = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).Count() > 2 ? Math.Round(SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).ToList()[1] / 1000, 2) : 0.0;
                    LCT.NO2_Percent = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).Count() > 2 ? Math.Round(SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).ToList()[1] / 1000, 2) / LCT.Avg_Total_Time : 0.0;
                    LCT.NO2_Percent = double.IsInfinity(Convert.ToDouble(LCT.NO2_Percent)) ? 0 : Math.Round(Convert.ToDouble(LCT.NO2_Percent * 100), 2);
                    LCT.NO3_Item = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<string>("ItemDescription")).Count() > 3 ? SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<string>("ItemDescription")).ToList()[2] : null;
                    LCT.NO3_CycleTime = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).Count() > 3 ? Math.Round(SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).ToList()[2] / 1000, 2) : 0.0;
                    LCT.NO3_Percent = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).Count() > 3 ? Math.Round(SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).ToList()[2] / 1000, 2) / LCT.Avg_Total_Time : 0.0;
                    LCT.NO3_Percent = double.IsInfinity(Convert.ToDouble(LCT.NO3_Percent)) ? 0 : Math.Round(Convert.ToDouble(LCT.NO3_Percent * 100), 2);
                    LCT.NO4_Item = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<string>("ItemDescription")).Count() > 4 ? SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<string>("ItemDescription")).ToList()[3] : null;
                    LCT.NO4_CycleTime = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).Count() > 4 ? Math.Round(SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).ToList()[3] / 1000, 2) : 0.0;
                    LCT.NO4_Percent = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).Count() > 4 ? Math.Round(SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).ToList()[3] / 1000, 2) / LCT.Avg_Total_Time : 0.0;
                    LCT.NO4_Percent = double.IsInfinity(Convert.ToDouble(LCT.NO4_Percent)) ? 0 : Math.Round(Convert.ToDouble(LCT.NO4_Percent * 100), 2);
                    LCT.NO5_Item = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<string>("ItemDescription")).Count() > 5 ? SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<string>("ItemDescription")).ToList()[4] : null;
                    LCT.NO5_CycleTime = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).Count() > 5 ? Math.Round(SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).ToList()[4] / 1000, 2) : 0.0;
                    LCT.NO5_Percent = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).Count() > 5 ? Math.Round(SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).ToList()[4] / 1000, 2) / LCT.Avg_Total_Time : 0.0;
                    LCT.NO5_Percent = double.IsInfinity(Convert.ToDouble(LCT.NO5_Percent)) ? 0 : Math.Round(Convert.ToDouble(LCT.NO5_Percent * 100), 2);
                    LCT.NO6_Item = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<string>("ItemDescription")).Count() > 6 ? SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<string>("ItemDescription")).ToList()[5] : null;
                    LCT.NO6_CycleTime = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).Count() > 6 ? Math.Round(SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).ToList()[5] / 1000, 2) : 0.0;
                    LCT.NO6_Percent = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).Count() > 6 ? Math.Round(SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).ToList()[5] / 1000, 2) / LCT.Avg_Total_Time : 0.0;
                    LCT.NO6_Percent = double.IsInfinity(Convert.ToDouble(LCT.NO6_Percent)) ? 0 : Math.Round(Convert.ToDouble(LCT.NO6_Percent * 100), 2);
                    LCT.NO7_Item = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<string>("ItemDescription")).Count() > 7 ? SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<string>("ItemDescription")).ToList()[6] : null;
                    LCT.NO7_CycleTime = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).Count() > 7 ? Math.Round(SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).ToList()[6] / 1000, 2) : 0.0;
                    LCT.NO7_Percent = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).Count() > 7 ? Math.Round(SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).ToList()[6] / 1000, 2) / LCT.Avg_Total_Time : 0.0;
                    LCT.NO7_Percent = double.IsInfinity(Convert.ToDouble(LCT.NO7_Percent)) ? 0 : Math.Round(Convert.ToDouble(LCT.NO7_Percent * 100), 2);
                    LCT.NO8_Item = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<string>("ItemDescription")).Count() > 8 ? SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<string>("ItemDescription")).ToList()[7] : null;
                    LCT.NO8_CycleTime = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).Count() > 8 ? Math.Round(SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).ToList()[7] / 1000, 2) : 0.0;
                    LCT.NO8_Percent = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).Count() > 8 ? Math.Round(SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).ToList()[7] / 1000, 2) / LCT.Avg_Total_Time : 0.0;
                    LCT.NO8_Percent = double.IsInfinity(Convert.ToDouble(LCT.NO8_Percent)) ? 0 : Math.Round(Convert.ToDouble(LCT.NO8_Percent * 100), 2);
                    LCT.NO9_Item = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<string>("ItemDescription")).Count() > 9 ? SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<string>("ItemDescription")).ToList()[8] : null;
                    LCT.NO9_CycleTime = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).Count() > 9 ? Math.Round(SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).ToList()[8] / 1000, 2) : 0.0;
                    LCT.NO9_Percent = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).Count() > 9 ? Math.Round(SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).ToList()[8] / 1000, 2) / LCT.Avg_Total_Time : 0.0;
                    LCT.NO9_Percent = double.IsInfinity(Convert.ToDouble(LCT.NO9_Percent)) ? 0 : Math.Round(Convert.ToDouble(LCT.NO9_Percent * 100), 2);
                    LCT.NO10_Item = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<string>("ItemDescription")).Count() > 10 ? SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<string>("ItemDescription")).ToList()[9] : null;
                    LCT.NO10_CycleTime = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).Count() > 10 ? Math.Round(SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).ToList()[9] / 1000, 2) : 0.0;
                    LCT.NO10_Percent = SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).Count() > 10 ? Math.Round(SpareRetryAVGTable.AsEnumerable().Select(x => x.Field<Single>("AVGSpare(PASS)")).ToList()[9] / 1000, 2) / LCT.Avg_Total_Time : 0.0;
                    LCT.NO10_Percent = double.IsInfinity(Convert.ToDouble(LCT.NO10_Percent)) ? 0 : Math.Round(Convert.ToDouble(LCT.NO10_Percent * 100), 2);
                    List_LongCycleTime.Add(LCT);
                }
                CycleTimeCount = ItemNameTypeList.Count();
            }
            catch (Exception ex)
            {
                CycleTimeCount = 0;
                CycleTimeErrorString = Org + " CycleTime Get Data Error " + ex.ToString();
            }
        }

        private static void processDataFailItem(out string FailItemResult)
        {
            DC.SetTransactionsFailItem(List_FailDatas, out FailItemResult);
        }

        private static void processNonITMDataFailItem()
        {
            DC.SetTransactionsNonITMFailItem(List_AthenaFailDatas);
        }
    }
}