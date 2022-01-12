using Dapper;
using DocumentFormat.OpenXml.InkML;
using Newtonsoft.Json;
using NLog;
using PTE_Web.Class;
using PTE_Web.Controllers;
using PTE_Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace PTE_Web.Connections
{
    public static class DataHandlerFunctions
    {
        private static readonly Logger _PTElogger = LogManager.GetLogger("PTE_WEBLogger");

        public static List<SimplyFYRInfo> GetDailyFYR_FR_RTRByItemNameTypeAndOrg(string Sdate, string Edate, int ItemNameType, string Org)
        {
            var Output = new List<SimplyFYRInfo>();
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var Table = db.Query<PTEWEB_ItemNameType_ByDaily>($@"SELECT *FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily] where Org ='{Org}' and itemnametype ='{ItemNameType}' and Date between '{Sdate}' and '{Edate}' order by date asc ").ToList();

                    Table.ForEach(item =>
                    {
                        var IssueFlag = DataHandlerFunctions.GetBulletFromData(item.Date.ToString("yyyy-MM-dd"), ItemNameType);
                        var DailyUPHList = GetDailyUPH(new List<int>() { ItemNameType }, item.Date.ToString("yyyy-MM-dd"), Org);
                        var tempUPH = DailyUPHList.Count != 0 ? DailyUPHList.First().UPH : 999;
                        Output.Add(new SimplyFYRInfo
                        {
                            ItemNameType = ItemNameType,
                            Date = item.Date.ToString("yyyy-MM-dd"),
                            Total = item.Total,
                            FYR = item.FYR,
                            Spare = item.Avg_Pass_Time,
                            EstimateUPH = tempUPH,
                            Retry_Rate = item.Retry_Rate,
                            href = IssueFlag == "No" ? "" : $@"..\\Content\\exclamation-{IssueFlag}.png"
                        }); ;
                    });

                    return Output;
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return null;
            }
        }

        public static List<ItemUPHList> GetDailyUPH(List<int> ItemNameTypeList, string date, string org)
        {
            try
            {
                var StringList = new List<string>();
                ItemNameTypeList.ForEach(item =>
                {
                    StringList.Add(item.ToString());
                });
                var itemnametypeString = String.Join(",", ItemNameTypeList);
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var Output = db.Query<ItemUPHList>($@"SELECT  ItemNameType,[EstimateUPH] as UPH  FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_RealOutput_ByDaily] where date ='{date}' and ItemNameType in ({itemnametypeString}) and Org ='{org}' ").ToList();

                    return Output;
                }
            }
            catch (Exception e)
            {
                return new List<ItemUPHList>();
            }
        }

        public static Dictionary<string, string> ProcessDeltaInfo(string org, string workdate, List<Estimate_UPH> AllData, List<ItemMappingList> ItemList)
        {
            try
            {
                var Output = new Dictionary<string, string>();

                Output["TodayCount"] = ItemList.Count.ToString();

                var yesterday = (DateTime.Parse(workdate).AddDays(-1)).ToString("yyyy-MM-dd");
                var previous_FYR = GetFYRByDateAndOrg(yesterday, yesterday, org);
                var previous_UPH = GetDailyUPH(ItemList.Select(item => item.ItemNametype).ToList(), yesterday, org);

                var total = 0;
                var betterFYR = 0;
                var betterSpare = 0;
                var worstFYR = 0;
                var worstSpare = 0;
                var betterUPH = 0;
                var worstUPH = 0;
                ItemList.ForEach(item =>
                {
                    var tempToday = AllData.Where(x => x.itemnametype == item.ItemNametype).OrderByDescending(x => x.RealOutput).First();
                    var tempYesterdayFYR_Spare = previous_FYR.Where(x => x.ItemNameType == item.ItemNametype).ToList();
                    var tempYesterdayUPH = previous_UPH.Where(x => x.ItemNameType == item.ItemNametype).ToList();
                    if (tempYesterdayFYR_Spare.Count != 0 && tempYesterdayUPH.Count != 0)
                    {
                        total++;
                        betterFYR = tempYesterdayFYR_Spare.ToList().First().FYR <= tempToday.FYR ? betterFYR + 1 : betterFYR;
                        betterSpare = tempYesterdayFYR_Spare.ToList().First().Avg_Pass_Time <= tempToday.AvgSpare ? betterSpare + 1 : betterSpare;
                        betterUPH = tempYesterdayUPH.ToList().First().UPH <= tempToday.EstimateUPH ? betterUPH + 1 : betterUPH;
                    }
                });

                worstFYR = total - betterFYR;
                worstSpare = total - betterSpare;
                worstUPH = total - betterUPH;
                Output["CompareCount"] = total.ToString();
                Output["FYR_Better"] = betterFYR.ToString();
                Output["FYR_Better_Rate"] = (Math.Round((float)betterFYR / (float)total, 2) * 100).ToString();
                Output["FYR_Worst"] = worstFYR.ToString();
                Output["FYR_Worst_Rate"] = (Math.Round((float)worstFYR / (float)total, 2) * 100).ToString();
                Output["Spare_Better"] = betterSpare.ToString();
                Output["Spare_Better_Rate"] = (Math.Round((float)betterSpare / (float)total, 2) * 100).ToString();
                Output["Spare_Worst"] = worstSpare.ToString();
                Output["Spare_Worst_Rate"] = (Math.Round((float)worstSpare / (float)total, 2) * 100).ToString();
                Output["UPH_Better"] = betterUPH.ToString();
                Output["UPH_Better_Rate"] = (Math.Round((float)betterUPH / (float)total, 2) * 100).ToString();
                Output["UPH_Worst"] = worstUPH.ToString();
                Output["UPH_Worst_Rate"] = (Math.Round((float)worstUPH / (float)total, 2) * 100).ToString();
                return Output;
            }
            catch (Exception e)
            {
                return new Dictionary<string, string>();
            }
        }

        public static Dictionary<string, string> ProcessOutlineAtRealUPHByShift(List<Estimate_UPH> AllData)
        {
            try
            {
                var Output = new Dictionary<string, string>();
                var NightData = AllData.Where(item => item.shiftid != 0).ToList();
                var DayData = AllData.Where(item => item.shiftid == 0).ToList();

                //FYR Part
                var TotalCount = AllData.Select(item => item.RealOutput).Sum();
                var NightCount = NightData.Select(item => item.RealOutput).Sum();
                var DayCount = DayData.Select(item => item.RealOutput).Sum();

                var CalNightFYR = 0.0;
                NightData.ForEach(item =>
                {
                    CalNightFYR = CalNightFYR + (float)item.RealOutput * item.FYR;
                });
                var NightFYR = NightCount != 0 ? Math.Round((float)CalNightFYR / (float)NightCount, 2) : 0;

                var CalDayFYR = 0.0;
                DayData.ForEach(item =>
                {
                    CalDayFYR = CalDayFYR + (float)item.RealOutput * item.FYR;
                });
                var DayFYR = DayCount != 0 ? Math.Round((float)CalDayFYR / (float)DayCount, 2) : 0;

                var TotalFYR = Math.Round((DayFYR * ((float)DayCount / (float)TotalCount)) + (NightFYR * ((float)NightCount / (float)TotalCount)), 2);
                var NightTop3FYR = NightData.OrderBy(item => item.FYR).Take(3).ToList();
                var DayTop3FYR = DayData.OrderBy(item => item.FYR).Take(3).ToList();
                //FYR Assign
                Output["FYR_Total"] = TotalFYR.ToString();
                Output["FYR_D"] = DayFYR != 0 ? DayFYR.ToString() : "NaN";
                Output["FYR_N"] = NightFYR != 0 ? NightFYR.ToString() : "NaN";

                for (int i = 0; i < DayTop3FYR.Count; i++)
                {
                    Output[$@"FYR_D_T{i + 1}_Item"] = DayTop3FYR[i].itemnametype.ToString();
                    Output[$@"FYR_D_T{i + 1}_value"] = DayTop3FYR[i].FYR.ToString();
                }
                for (int i = 0; i < NightTop3FYR.Count; i++)
                {
                    Output[$@"FYR_N_T{i + 1}_Item"] = NightTop3FYR[i].itemnametype.ToString();
                    Output[$@"FYR_N_T{i + 1}_value"] = NightTop3FYR[i].FYR.ToString();
                }

                //Achievement
                var UPH_AvaliableRaw = AllData.Where(item => item.UPH < 999).ToList();
                var UPH_Achieve = UPH_AvaliableRaw.Where(item => item.EstimateUPH >= item.UPH).ToList();
                var UPH_AvaliableItemCount = UPH_AvaliableRaw.ToList().Count;
                Output["UPH_Total"] = (Math.Round(((float)UPH_Achieve.Count / (float)UPH_AvaliableItemCount), 2) * 100).ToString();
                Output["UPH_D"] = DayCount != 0 ? (Math.Round(((float)UPH_Achieve.Where(item => item.shiftid == 0).ToList().Count / (float)UPH_AvaliableRaw.Where(item => item.shiftid == 0).ToList().Count), 2) * 100).ToString() : "NaN";
                Output["UPH_N"] = NightCount != 0 ? (Math.Round(((float)UPH_Achieve.Where(item => item.shiftid == 1).ToList().Count / (float)UPH_AvaliableRaw.Where(item => item.shiftid == 1).ToList().Count), 2) * 100).ToString() : "NaN";

                return Output;
            }
            catch (Exception e)
            {
                return new Dictionary<string, string>();
            }
        }

        public static int GetUPHByItemNameType(string org, int ItemNameType)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var Output = db.Query<int>($@"SELECT  UPH  FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_RealOutput_ByDaily] where ItemNameType ='{ItemNameType}' and Org ='{org}' ").FirstOrDefault();
                    return Output;
                }
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public static List<PTEProductOwner> GetPTEProductOwnerTable()
        {
            try
            {
                var Output = new List<PTEProductOwner>();
                using (var db = ConnectionFactory.CreatConnection())
                {
                    Output = db.Query<PTEProductOwner>($@"SELECT  *  FROM [PTEDB].[dbo].[PTEWEB_ProductOwner]").ToList();
                }
                return Output;
            }
            catch (Exception e)
            {
                return new List<PTEProductOwner>();
            }
        }

        public static void UpdateIssues(string itemnametype, string org, string content)
        {
            using (SqlConnection conn = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["PTEWebConnectionString"].ToString()))
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();
                SqlCommand cmd0 = new SqlCommand();
                cmd0.Connection = conn;
                cmd0.CommandTimeout = 60;
                string SQL = "SELECT top 1 Issue_Status,Title_id FROM [PTEDB].[dbo].[PTEWEB_Issues_Title]where ItemNameType='" + itemnametype + "' and org='" + org + "' order by CreateDate desc";
                cmd0.CommandText = SQL;
                DataTable dt = new DataTable();
                dt.Load(cmd0.ExecuteReader());
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    if (Convert.ToBoolean(dt.Rows[0][0]))
                    {
                        cmd.CommandText = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_Title]SET Title='" + content + "' where itemnametype='" + itemnametype + "' and org='" + org + "';" +
                                                            "INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_History]([ItemNameType],[Status] ,[Contents] ,[DateTime] ,[Org])VALUES('" + itemnametype + "','1','" + content + "','" + DateTime.Now.ToString("yyyy-MM-dd") + "','" + org + "');" +
                                                            "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] SET IssueAlive_Dates='1',Status='1',LastUpdateDate='" + DateTime.Now.ToString("yyyy-MM-dd") + "',FYR_Time='" + DateTime.Now.ToString("yyyy-MM-dd") + "',Title_id_Custom='" + dt.Rows[0][1].ToString() + "'  where itemnametype='" + itemnametype + "' and Support_Org='" + org + "';";
                    }
                    else
                    {
                        cmd.CommandText = "INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_Title]( [ItemNameType],[Title],[Issue_Status]  ,[CreateDate],[Org]) output INSERTED.Title_id VALUES('" + itemnametype + "','" + content + "','" + 1 + "','" + DateTime.Now.ToString("yyyy-MM-dd") + "','" + org + "')";
                        cmd.Transaction = tran;
                        int titleid = (int)cmd.ExecuteScalar();
                        cmd.CommandText =
                                                           "INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_History]([ItemNameType],[Status] ,[Contents] ,[DateTime] ,[Org])VALUES('" + itemnametype + "','1','" + content + "','" + DateTime.Now.ToString("yyyy-MM-dd") + "','" + org + "');" +
                                                           "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] SET IssueAlive_Dates='1',Status='1',LastUpdateDate='" + DateTime.Now.ToString("yyyy-MM-dd") + "',FYR_Time='" + DateTime.Now.ToString("yyyy-MM-dd") + "',Title_id_Custom='" + titleid + "'  where itemnametype='" + itemnametype + "' and Support_Org='" + org + "';";
                    }
                    cmd.Transaction = tran;
                    cmd.ExecuteNonQuery();
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                }
            }
        }

        public static void InsertNewIssue(string itemnametype, string org, string content)
        {
            using (SqlConnection conn = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["PTEWebConnectionString"].ToString()))
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandTimeout = 60;
                    cmd.CommandText = "INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_Title]( [ItemNameType],[Title],[Issue_Status]  ,[CreateDate],[Org]) output INSERTED.Title_id VALUES('" + itemnametype + "','" + content + "','" + 1 + "','" + DateTime.Now.ToString("yyyy-MM-dd") + "','" + org + "')";
                    cmd.Transaction = tran;
                    int titleid = (int)cmd.ExecuteScalar();
                    string InsertDailySQL = "INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_ByDaily]([ItemNameType] ,[IssueAlive_Dates] ,[Status],[Support_Org] ,[LastUpdateDate],[FYR_Time],[Title_id_Custom])" +
                                                           "VALUES('" + itemnametype + "','1','1','" + org + "','" + DateTime.Now.ToString("yyyy-MM-dd") + "','" + DateTime.Now.ToString("yyyy-MM-dd") + "','" + titleid + "');" +
                                                           "INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_History]([ItemNameType],[Status] ,[Contents] ,[DateTime] ,[Org])VALUES('" + itemnametype + "','1','" + content + "','" + DateTime.Now.ToString("yyyy-MM-dd") + "','" + org + "')";
                    cmd.CommandText = InsertDailySQL;
                    cmd.ExecuteNonQuery();
                    tran.Commit();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                }
            }
        }

        public static bool CheckIssueStatus0(string org, string itemnametype)
        {
            DataTable Table = new DataTable();
            using (var db = ConnectionFactory.CreatConnection())
            {
                string SQL = "SELECT top 1 Issue_Status FROM [PTEDB].[dbo].[PTEWEB_Issues_Title]where ItemNameType='" + itemnametype + "' and org='" + org + "' order by CreateDate desc";
                Table.Load(db.ExecuteReader(SQL));

                if (Table.Rows.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public static List<string> GetIssueTrackingList(string itemnametype, string org)
        {
            try
            {
                var Output = new List<string>();
                using (var db = ConnectionFactory.CreatConnection())
                {
                    Output = db.Query<string>($@"SELECT  Empid  FROM [PTEDB].[dbo].[PTEWEB_RealTimeTrack] where itemnametype='{itemnametype}' and issue_org='{org}'").ToList();
                }
                return Output;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static List<ItemNameDesModel> GetqueryItemNameTypeorDes(string queryStr, string Org)
        {
            string sql = "SELECT distinct ItemNameType,Description FROM[PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily_TOP10_FailItem]where ItemNameType like '%" + queryStr + "%' or Description like'%" + queryStr + "%' and org = '" + Org + "'";
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var Output = new List<ItemNameDesModel>();
                    Output = db.Query<ItemNameDesModel>(sql).ToList();
                    return Output;
                }
            }
            catch (Exception ex)
            {
                return new List<ItemNameDesModel>();
            }
        }

        public static IEnumerable<SelectListItem> GetFailItemList(int ItemNameType)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var RangeTopFailItemList = db.Query<PTEWEB_ItemNameType_ByDaily_TOP10_FailItem>($@"SELECT top(14) * FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily_TOP10_FailItem] where ItemNameType = {ItemNameType} order by date desc").ToList();
                    var AllList = DeltaIModel.TopFailTableTransferToFailInfoList(RangeTopFailItemList);

                    var DistinctItemList = DistincItemList(AllList.Select(item => item.FailItemName).ToList());
                    var FailItemList = new List<TopFailInfo>();

                    var Output = new List<SelectListItem>();

                    DistinctItemList.ForEach(ItemName =>
                    {
                        var ItemCount = 0;
                        foreach (var ItemInfo in AllList)
                        {
                            if (ItemInfo.FailItemName == ItemName)
                            {
                                ItemCount += ItemInfo.FaiCount;
                            }
                        }
                        FailItemList.Add(new TopFailInfo { FailItemName = ItemName, FaiCount = ItemCount });
                    });

                    FailItemList.OrderBy(x => x.FaiCount).ToList();

                    FailItemList.ForEach(item =>
                    {
                        Output.Add(new SelectListItem() { Text = item.FailItemName, Value = item.FailItemName });
                    });

                    return Output;
                }
            }
            catch (Exception e)
            {
                return new List<SelectListItem>();
            }
        }

        public static IEnumerable<SelectListItem> GetActionList()
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var action_list = db.Query<PTEWEB_Issues_Actions>($@"SELECT [Actionid],[Action] from [PTEDB].[dbo].[PTEWEB_Issues_Actions]").ToList();
                    var ActionList = new List<SelectListItem>();

                    foreach (var item in action_list)
                    {
                        if (item.Actionid != 10 && item.Actionid != 11)
                            ActionList.Add(new SelectListItem() { Text = $@"{item.Action}", Value = item.Actionid.ToString() });
                    }
                    ActionList.Add(new SelectListItem() { Text = $@"其他", Value = "11" });
                    return ActionList;
                }
            }
            catch (Exception e)
            {
                return new List<SelectListItem>();
            }
        }

        public static IEnumerable<SelectListItem> GetCauseList()
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var ActionList = db.Query<PTEWEB_Issues_ReplyCause>($@"SELECT [Causeid],[Cause]  FROM [PTEDB].[dbo].[PTEWEB_Issues_ReplyCause]").ToList();
                    var OwnerList = new List<SelectListItem>();

                    foreach (var item in ActionList)
                    {
                        OwnerList.Add(new SelectListItem() { Text = $@"{item.Cause}", Value = item.Causeid.ToString() });
                    }
                    return OwnerList;
                }
            }
            catch (Exception e)
            {
                return new List<SelectListItem>();
            }
        }

        public static List<PTEProductOwnerIssue> GetOwnerIssueList(string owner)
        {
            var TempList = new List<PTEProductOwnerIssue>();
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    TempList = db.Query<PTEProductOwnerIssue>($@"select IssueTitle.Org,IssueTitle.ItemNameType,ProductOwner.Description,IssueTitle.Title,IssueTitle.Issue_Status,IssueTitle.CreateDate,IssueTitle.Title_id
                                                            from
                                                            (SELECT *  FROM [PTEDB].[dbo].[PTEWEB_Issues_Title]) AS IssueTitle
                                                            inner join
                                                            (SELECT [Org],[ItemNameType],Description FROM [PTEDB].[dbo].[PTEWEB_ProductOwner] WHERE OWNER ='{owner}') as ProductOwner
                                                            on IssueTitle.Org = ProductOwner.Org and IssueTitle.ItemNameType = ProductOwner.ItemNameType").ToList();
                }
                TempList.ForEach(item =>
                {
                    if (item.Title == "系統監控雙週計算良率過低")
                    {
                        item.Title = "FYR Issue";
                    }
                    else if (item.Title == "UPH達成率低於80%")
                    {
                        item.Title = "UPH Issue";
                    }
                });
                return TempList;
            }
            catch (Exception e)
            {
                return new List<PTEProductOwnerIssue>();
            }
        }

        public static string GetBulletFromData(string Date, int ItemNameType)
        {
            string IssueOutput = string.Empty;
            string ReplyOutput = string.Empty;
            string CloseOutput = string.Empty;
            using (var db = ConnectionFactory.CreatConnection())
            {
                var IssueCount = db.Query<PTEWEB_Issues_Title>($@"SELECT *FROM [PTEDB].[dbo].[PTEWEB_Issues_Title] where  itemnametype ='{ItemNameType}' and CreateDate >= '{Date + " 00:00:00"}' and CreateDate < '{Date + " 23:59:59"}' order by CreateDate asc ").ToList();
                IssueOutput = IssueCount.Count() > 0 ? "Open" : "No";

                var ReplyCount = db.Query<PTEWEB_Issues_Reply>($@"SELECT *FROM [PTEDB].[dbo].[PTEWEB_Issues_Content] where  itemnametype ='{ItemNameType}' and CreateTime >= '{Date + " 00:00:00"}' and CreateTime < '{Date + " 23:59:59"}'  order by CreateTime asc ").ToList();
                ReplyOutput = ReplyCount.Count() > 0 ? "Action" : "No";

                var CloseCount = db.Query<PTEWEB_Issues_Reply>($@"SELECT *FROM [PTEDB].[dbo].[PTEWEB_Issues_Content] where  itemnametype ='{ItemNameType}' and CreateTime >= '{Date + " 00:00:00"}' and CreateTime < '{Date + " 23:59:59"}'  and Actionid = 3 order by CreateTime asc ").ToList();
                CloseOutput = CloseCount.Count() > 0 ? "Close" : "No";
            }

            if (IssueOutput != "No")
                return IssueOutput;

            if (CloseOutput != "No")
                return CloseOutput;

            return ReplyOutput;
        }

        public static List<double> GetRawDataByCorrID(int itemnumber, string type, string id, string Sdate, string Edate, int ItemNameType, string Org)
        {
            var Output = new List<double>();
            var table = GetDBByItemNameType(ItemNameType).ToLower();
            try
            {
                var script = string.Empty;
                var script2 = string.Empty;

                //Fixture
                if (type == "Fixture")
                {
                    if (table == "tblfinal")
                    {
                        var id2 = id.Substring(id.Length - 1, 1);
                        var id1 = id.Remove(id.Length - 2);
                        script = $@"select  Item{itemnumber} from {table} where Item{itemnumber}St!=2 and ItemNameType ='{ItemNameType}' and Station ='{id1}' and stationid = {id2} and tDateTime between '{Sdate}' and '{Edate}' ";
                    }
                    else
                    {
                        var id2 = id.Substring(id.Length - 1, 1);
                        var id1 = id.Remove(id.Length - 2);

                        script = $@"select  Item{itemnumber} from {table} where Item{itemnumber}St!=2 and ItemNameType ='{ItemNameType}' and fixtureid2 ='{id}' and tDateTime between '{Sdate}' and '{Edate}' ";
                    }
                }

                //Others
                else
                {
                    script = $@"select Item{itemnumber} from {table} where Item{itemnumber}St!=2 and ItemNameType ='{ItemNameType}' and {type} ='{id}'  and tDateTime between '{Sdate}' and '{Edate}' ";
                }

                using (var db = ConnectionFactory.CreatConnection(Org))
                {
                    var result1 = db.Query<double>(script).ToList();
                    Output.AddRange(result1);
                    return Output;
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return null;
            }
        }

        public static List<PTEProductOwner> OutProductOwnerDefault(string SDate, string EDate)
        {
            try
            {
                using (var conn = ConnectionFactory.CreatConnection())
                {
                    return conn.Query<PTEProductOwner>($@"Select Daily.Org,Daily.ItemNameType,Daily.Description,owner.Owner,owner.Status,ProductionDays from
                                                        (Select T.Org,T.ItemNameType,T.Description, Max(t.number) as ProductionDays
                                                        FROM
                                                        (SELECT Org,ItemNameType,Description,row_number() over(partition by Org,ItemNameType,Description order by date desc) as number
                                                         FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily] where date between '{SDate}'and '{EDate}' ) AS T
	                                                        GROUP BY T.Org,T.ItemNameType,T.Description ) Daily
                                                        Left join
                                                        (select * FROM [PTEDB].[dbo].[PTEWEB_ProductOwner]) owner
                                                        on owner.ItemNameType = Daily.ItemNameType and owner.Org = Daily.Org
                                                        union
                                                        Select Daily.Org,Daily.ItemNameType,Daily.Description,owner.Owner,owner.Status,ProductionDays from
                                                        (Select T.Org,T.ItemNameType,T.Description, Max(t.number) as ProductionDays
                                                        FROM
                                                        (SELECT Org,ItemNameType,Description,row_number() over(partition by Org,ItemNameType,Description order by date desc) as number
                                                         FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily] where date between '{SDate}'and '{EDate}' ) AS T
	                                                        GROUP BY T.Org,T.ItemNameType,T.Description ) Daily
                                                        Left join
                                                        (select * FROM [PTEDB].[dbo].[PTEWEB_ProductOwner]) owner
                                                        on owner.ItemNameType = Daily.ItemNameType and owner.Org = Daily.Org ").ToList();
                }
            }
            catch (Exception e)
            {
                return new List<PTEProductOwner>();
            }
        }

        public static List<PTEProductOwner> ProcessSelectedItemNameType(List<string> ItemList, string owner)
        {
            var Output = new List<PTEProductOwner>();

            foreach (var item in ItemList)
            {
                var StationSplit = item.Split('#');
                var result = CheckItemExistedonOwnerTable(int.Parse(StationSplit[1]), StationSplit[0]);
                Output.Add(new PTEProductOwner() { Org = StationSplit[0], ItemNameType = int.Parse(StationSplit[1]), Description = StationSplit[2], Status = result, Owner = owner });
            }
            return Output;
        }

        public static bool UpdateDataIntoProductOwnerTable(List<PTEProductOwner> Input)
        {
            try
            {
                using (var conn = ConnectionFactory.CreatConnection())
                {
                    foreach (var item in Input)
                    {
                        //ItemNameType already existed in Owner Table.
                        if (item.Status == "Yes")
                        {
                            conn.Execute($@"update [PTEDB].[dbo].[PTEWEB_ProductOwner] set Owner ='{item.Owner}' , Status = 'Assigned'  , UpdateTime = '{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}' where ItemNameType = {item.ItemNameType} and Org = '{item.Org.Trim()}' ");
                        }
                        else
                        {
                            conn.Execute($@"INSERT INTO [PTEDB].[dbo].[PTEWEB_ProductOwner] (Org, ItemNameType, Description,Owner,Status,UpdateTime) VALUES ('{item.Org.Trim()}', {item.ItemNameType},'{item.Description}',{item.Owner},'Assigned','{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}');");
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static string CheckItemExistedonOwnerTable(int itemnametype, string org)
        {
            var result = "No";
            try
            {
                using (var conn = ConnectionFactory.CreatConnection())
                {
                    result = conn.Query<int>($@"SELECT count(*) Total  FROM [PTEDB].[dbo].[PTEWEB_ProductOwner]  where Org ='{org}        ' and ItemNameType ='{itemnametype}'").First() > 0 ? "Yes" : "No";
                }
                return result;
            }
            catch (Exception e)
            {
                return result;
            }
        }

        public static List<PTEWEB_ItemNameType_ByDaily> GetFYRByDateRange(string SDate, string EDate)
        {
            var AllData = DataCatcherByDateRange(SDate, EDate);
            var FinalData = DataHandlerToProcessAllFYR(AllData);

            return (from item in FinalData
                    orderby item.FYR ascending
                    select item).ToList().OrderBy(x => x.FYR).ToList();
        }

        public static List<PTEWEB_nonITMXP_ByDaily> GetFYRByDateRangeNonITM(string SDate, string EDate, bool flag)
        {
            var AllData = DataCatcherByDateRangeNonITM(SDate, EDate, flag);

            return (from item in AllData
                    orderby item.FYR ascending
                    select item).ToList().OrderBy(x => x.FYR).ToList();
        }

        public static List<PTEWEB_Issues_ByDaily> GetIssuesByDateRange()
        {
            var AllData = DataIssuesByDateRange();
            return (from item in AllData
                    orderby item.LastUpdateDate ascending
                    select item).ToList();
        }

        public static List<PTEWEB_ItemNameType_ByDaily> GetFYRByDateAndOrg(string SDate, string EDate, string Org)
        {
            var AllData = DataCatcherByDateRange(SDate, EDate, Org);
            var FinalData = DataHandlerToProcessAllFYR(AllData);
            return FinalData.OrderBy(x => x.FYR).ToList();
        }

        public static List<PTEWEB_ItemNameType_ByDaily> GetFYRByDateAndOwner(string SDate, string EDate, int Owner)
        {
            var AllData = DataCatcherByDateRangeAndOwner(SDate, EDate, Owner);
            var FinalData = DataHandlerToProcessAllFYR(AllData);
            return FinalData.OrderBy(x => x.FYR).ToList();
        }

        public static string GetIssueOrg(string issueid, string itemnametype)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    return db.Query<string>($@"SELECT org   FROM [PTEDB].[dbo].[PTEWEB_Issues_Title]  where Title_id ={issueid} and [ItemNameType] = {itemnametype}").FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static List<PTEWEB_ItemNameType_ByDaily> GetFYRByItemNameTypeAndTopNumber(string StartDate, string EndDate, int itemnametype, int Number)
        {
            var table = string.Empty;
            if (itemnametype < 990000)
                table = " [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily] ";
            else
            {
                table = " [PTEDB].[dbo].[PTEWEB_nonITMXP_ByDaily] ";
            }

            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var Table = db.Query<PTEWEB_ItemNameType_ByDaily>($@"SELECT *FROM {table} where Date between '{StartDate}' and '{EndDate}' and ItemNameType  = {itemnametype} order by date desc").ToList();

                    return Table;
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return new List<PTEWEB_ItemNameType_ByDaily>();
            }
        }

        public static List<DailyUPH> GetEstimateUPHByItemNameTypeAndTopNumber(string StartDate, string EndDate, int itemnametype, int Number)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var Table = db.Query<DailyUPH>($@"  SELECT *FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_RealOutput_ByDaily]where Date between '{StartDate}' and '{EndDate}' and ItemNameType  = {itemnametype} order by date desc").ToList();

                    return Table;
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return new List<DailyUPH>();
            }
        }

        public static List<PTEWEB_ItemNameType_ByDaily> DataHandlerToProcessAllFYR(List<PTEWEB_ItemNameType_ByDaily> AllData)
        {
            var ResultTable = new List<PTEWEB_ItemNameType_ByDaily>();
            var ItemNameList = (from temp in AllData
                                select temp.ItemNameType).Distinct().ToList();

            foreach (var item in ItemNameList)
            {                
                var GroupRaws = (from temp in AllData
                                 where temp.ItemNameType == item
                                 select temp).ToList();

                var AfterRow = new PTEWEB_ItemNameType_ByDaily()
                {
                    ItemNameType = item,
                    TestStation = GroupRaws.FirstOrDefault().TestStation,
                    StationType = GroupRaws.FirstOrDefault().StationType,
                    Description = GroupRaws.FirstOrDefault().Description,
                    Org = GroupRaws.FirstOrDefault().Org.Trim(),
                    Source = GroupRaws.FirstOrDefault().Source,

                    D_Total = (from temp in GroupRaws
                               select temp.D_Total).Sum(),
                    D_Fail = (from temp in GroupRaws
                              select temp.D_Fail).Sum(),
                    D_Pass = (from temp in GroupRaws
                              select temp.D_Pass).Sum(),

                    TestType = GroupRaws.FirstOrDefault().TestType,
                    TestType2 = GroupRaws.FirstOrDefault().TestType2,

                    Fail = (from temp in GroupRaws
                            select temp.Fail).Sum(),
                    Pass = (from temp in GroupRaws
                            select temp.Pass).Sum(),
                    Total = (from temp in GroupRaws
                             select temp.Total).Sum(),

                    Avg_Pass_Time = Math.Round((from temp in GroupRaws
                                                select temp.Avg_Pass_Time).Average(), 2),
                    Avg_Total_Time = Math.Round((from temp in GroupRaws
                                                 select temp.Avg_Total_Time).Average(), 2)
                };
                AfterRow.Pass_Rate = Math.Round(((float)AfterRow.Pass / (float)AfterRow.Total), 2) * 100;
                AfterRow.FYR = Math.Round(GroupRaws.FirstOrDefault().FYR, 2);
                AfterRow.Fail_Rate = Math.Round(((float)AfterRow.Fail / (float)AfterRow.Total), 2) * 100;
                AfterRow.Retry_Rate = Math.Round(((float)AfterRow.Total / (float)AfterRow.D_Total) - 1, 2) * 100;
                ResultTable.Add(AfterRow);
            }
            return ResultTable;
        }

        public static List<PTEWEB_ItemNameType_ByDaily> DataCatcherByDateRange(string Sdate, string Edate)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var Table = db.Query<PTEWEB_ItemNameType_ByDaily>($@"SELECT *FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily] where Date between '{Sdate}' and '{Edate}' ").ToList();

                    return Table;
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return null;
            }
        }

        public static List<PTEWEB_ItemNameType_ByDaily> DataCatcherByDateRangeAndOwner(string Sdate, string Edate, int Owner)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var Table = db.Query<PTEWEB_ItemNameType_ByDaily>($@"select *
                                                                        from
                                                                        ((SELECT [id],[Org],[Date],[ItemNameType],[Description],[TestType],[TestType2],[Total],
                                                                        [Pass],[Fail],[D_Total],[D_Pass],[D_Fail],[Pass_Rate],[Fail_Rate],[Retry_Rate],[FYR],
                                                                        [Avg_Pass_Time],[Avg_Total_Time],[Source],[TestStation],[StationType]   FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily]
                                                                        where Date between '{Sdate}' and '{Edate}')
                                                                        UNION
                                                                        (SELECT [id],[Org],[Date],[ItemNameType],[Description],[TestType],[TestType2],[Total],
                                                                        [Pass],[Fail],[D_Total],[D_Pass],[D_Fail],[Pass_Rate],[Fail_Rate],[Retry_Rate],[FYR],
                                                                        [Avg_Pass_Time],[Avg_Total_Time],[Source],[TestStation],'UnKnown' as [StationType] FROM [PTEDB].[dbo].[PTEWEB_nonITMXP_ByDaily]
                                                                        where Date between '{Sdate}' and '{Edate}')) TestData
                                                                        inner join
                                                                        (select ItemNameType,Org   FROM [PTEDB].[dbo].[PTEWEB_ProductOwner] where Owner = '{Owner}')  owner
                                                                        on TestData.Org = owner.Org and TestData.ItemNameType = owner.ItemNameType").ToList();

                    return Table;
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return new List<PTEWEB_ItemNameType_ByDaily>();
            }
        }

        public static List<PTEWEB_nonITMXP_ByDaily> DataCatcherByDateRangeNonITM(string Sdate, string Edate, bool flag)
        {
            try
            {
                List<PTEWEB_nonITMXP_ByDaily> Table = new List<PTEWEB_nonITMXP_ByDaily>();
                using (var db = ConnectionFactory.CreatConnection())
                {
                    if (flag)
                    {
                        var TableDaily = db.Query<PTEWEB_nonITMXP_ByDaily>($@"SELECT Org,ItemNameType,Description,TestStation,Source,TestType,TestType2,Date,
                                                                                                            sum(D_Total)as D_Total,
                                                                                                            sum(D_Fail)as D_Fail,
                                                                                                            sum(D_Pass)as D_Pass,
                                                                                                            sum(Fail)as Fail,
                                                                                                            sum(Pass)as Pass,
                                                                                                            sum(Total)as Total,
                                                                                                            round(AVG(Avg_Pass_Time),2)as Avg_Pass_Time,
                                                                                                            round(AVG(Avg_Total_Time),2)as Avg_Total_Time,
                                                                                                            round(cast(sum(Pass)*100 as float)/sum(Total),2) as Pass_Rate,
                                                                                                            round((1-round(cast(sum(D_Fail) as float)/sum(D_Total),3))*100,3) as FYR,
                                                                                                            round(cast(sum(Fail)*100 as float) /sum(Total) ,2)as Fail_Rate,
                                                                                                            round(((cast(sum(Total) as float) /sum(D_Total))-1)*100,2) as Retry_Rate
                                                                                                            FROM [PTEDB].[dbo].[PTEWEB_nonITMXP_ByDaily] where Date between '{Sdate}' and '{Edate}' group by ItemNameType,Description,Org,TestStation,Source,TestType,TestType2,Date
                                                                                                            order by Org asc").ToList();
                        var UPHTable = db.Query<PTEWEB_nonITMXP_UPH>($@"SELECT Org,ItemNameType,EstimateUPH,UPH,Date FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_RealOutput_ByDaily] where Date between '{Sdate}' and '{Edate}' and itemnametype>900000").ToList();

                        foreach (var r in TableDaily)
                        {
                            var u = (from h in UPHTable.AsEnumerable()
                                     where h.ItemNameType == r.ItemNameType
                                     && h.Org.Trim() == r.Org
                                     && h.Date == r.Date
                                     select h).ToList();
                            if (u.Count() > 0)
                            {
                                r.UPH_Achievement = Math.Round((u.First().EstimateUPH / u.First().UPH) * 100, 3);
                            }
                            else
                            {
                                r.UPH_Achievement = 0.0;
                            }
                            Table.Add(r);
                        }
                    }
                    else
                    {
                        var TableDaily = db.Query<PTEWEB_nonITMXP_ByDaily>($@"SELECT Org,ItemNameType,Description,TestStation,Source,TestType,TestType2,
                                                                                                            sum(D_Total)as D_Total,
                                                                                                            sum(D_Fail)as D_Fail,
                                                                                                            sum(D_Pass)as D_Pass,
                                                                                                            sum(Fail)as Fail,
                                                                                                            sum(Pass)as Pass,
                                                                                                            sum(Total)as Total,
                                                                                                            round(AVG(Avg_Pass_Time),2)as Avg_Pass_Time,
                                                                                                            round(AVG(Avg_Total_Time),2)as Avg_Total_Time,
                                                                                                            round(cast(sum(Pass)*100 as float)/sum(Total),2) as Pass_Rate,
                                                                                                            round((1-round(cast(sum(D_Fail) as float)/sum(D_Total),3))*100,3) as FYR,
                                                                                                            round(cast(sum(Fail)*100 as float) /sum(Total) ,2)as Fail_Rate,
                                                                                                            round(((cast(sum(Total) as float) /sum(D_Total))-1)*100,2) as Retry_Rate
                                                                                                            FROM [PTEDB].[dbo].[PTEWEB_nonITMXP_ByDaily] where Date between '{Sdate}' and '{Edate}' group by ItemNameType,Description,Org,TestStation,Source,TestType,TestType2
                                                                                                            order by Org asc").ToList();
                        return TableDaily;
                    }
                    return Table;
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return new List<PTEWEB_nonITMXP_ByDaily>();
            }
        }

        public static List<Hist_Chart_KeyValue> ProcessCorrelationRawData(List<double> input)
        {
            try
            {
                var Output = new List<Hist_Chart_KeyValue>();

                var Max = input.Max();
                var Min = input.Min();
                var Bin = (Max - Min) / 10;
                Max += Bin;
                var FinalRange = Min;
                while (Output.Count != 10)
                {
                    var tempCount = input.FindAll(x => x >= FinalRange && x < FinalRange + Bin).ToList();
                    Output.Add(new Hist_Chart_KeyValue() { xid = $@"{FinalRange.ToString("f2")} to {(FinalRange + Bin).ToString("f2")}", yvalue = tempCount.Count });
                    FinalRange += Bin;
                }
                return Output;
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return new List<Hist_Chart_KeyValue>();
            }
        }

        public static DataTable TestItemsHandler(int ItemNameType)
        {
            ITMXPServerQuery queryitmxp = new ITMXPServerQuery();
            //先得到相關於ItemNameType的file index
            string ITMXPQueryString = "SELECT * FROM itmxp.tbl_testinfo where ItemNameType='" + ItemNameType + "' order by Idx_TestInfo desc";
            DataTable IdxFileTable = queryitmxp.QueryResult(ITMXPQueryString);
            queryitmxp = new ITMXPServerQuery();
            ITMXPQueryString = "SELECT Idx_Version,IsReleased FROM itmxp.tbl_fileversion where Idx_file='" + IdxFileTable.Rows[0]["Idx_file"].ToString() + "'";
            IdxFileTable = queryitmxp.QueryResult(ITMXPQueryString);
            int idx_Version = 0;
            if (IdxFileTable.AsEnumerable().Where(x => x.Field<int>("IsReleased") == 1).Count() > 0)
            {
                //Release
                idx_Version = IdxFileTable.AsEnumerable().Where(x => x.Field<int>("IsReleased") == 1).OrderByDescending(x => x.Field<int>("Idx_Version")).Select(x => x.Field<int>("Idx_Version")).ToList()[0];
            }
            else
            {
                //PilotRun
                idx_Version = IdxFileTable.AsEnumerable().OrderByDescending(x => x.Field<int>("Idx_Version")).Select(x => x.Field<int>("Idx_Version")).ToList()[0];
            }

            queryitmxp = new ITMXPServerQuery();
            ITMXPQueryString = "SELECT * FROM itmxp.tbl_testitem where Idx_Version='" + idx_Version + "'";
            IdxFileTable = queryitmxp.QueryResult(ITMXPQueryString);
            return IdxFileTable;
        }

        public static DataTable GetTestCommands(int idx_TestItem)
        {
            ITMXPServerQuery queryitmxp = new ITMXPServerQuery();

            string ITMXPQueryString = "SELECT * FROM itmxp.tbl_commands where Idx_testitem='" + idx_TestItem + "'";
            DataTable CommandTable = queryitmxp.QueryResult(ITMXPQueryString);

            return CommandTable;
        }

        public static DataTable DataSpareHandler(int ItemNameType, string Org, string Sdate, string Edate, string source)
        {
            try
            {
                DataTable Table = new DataTable();
                using (var db = ConnectionFactory.CreatConnection())
                {
                    string SQL = "Select * FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_SpareTime_ByDaily]where Org='" + Org + "' and ItemNameType='" + ItemNameType + "' and DateTime>='" + Sdate + "' and DateTime<='" + Edate + "'";
                    Table.Load(db.ExecuteReader(SQL));
                }
                if (Table.Rows.Count == 0)
                { //舊資料
                    using (var db = ConnectionFactory.CreatConnection(Org))
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

                        string SpareAndRetryStr = "";
                        string SpareAndRetryStr2 = "";
                        for (int i = 1; i <= 150; i++)
                        {
                            SpareAndRetryStr += "b.Retry" + i + ",b.spare" + i + "" + ",";
                        }
                        for (int i = 151; i <= 250; i++)
                        {
                            SpareAndRetryStr2 += "c.Retry" + i + ",c.spare" + i + "" + ",";
                        }

                        SpareAndRetryStr = SpareAndRetryStr.TrimEnd(',');
                        SpareAndRetryStr2 = SpareAndRetryStr2.TrimEnd(',');
                        string SQL = "select " + SpareAndRetryStr + " FROM " +
                            "(SELECT ItemNameType,SerialNumber,tDateTime,station FROM " + TargetTestTable + "  with (nolock)  where tDateTime>='" + Sdate + "' and tDateTime<'" + Edate + "' and ItemNameType='" + ItemNameType + "' and result=1) A" +
                            " inner  join [ate_db].[dbo].[TblTestTime]B  with (nolock) on A.Serialnumber=B.ESN and A.tdatetime=B.TdateTime and A.station=b.station";
                        if (TargetTestTable == "TblCPU")
                        {
                            SQL = "select " + SpareAndRetryStr + "," + SpareAndRetryStr2 + "  FROM(select B.ESN,B.tdatetime,b.station," + SpareAndRetryStr + " FROM " +
                            "(SELECT ItemNameType,SerialNumber,tDateTime,station FROM " + TargetTestTable + "  with (nolock)  where tDateTime>='" + Sdate + "' and tDateTime<'" + Edate + "' and ItemNameType='" + ItemNameType + "' and result=1) A" +
                            " inner  join [ate_db].[dbo].[TblTestTime]B  with (nolock) on A.Serialnumber=B.ESN and A.tdatetime=B.TdateTime and A.station=b.station)B inner join  [ate_db].[dbo].[TblTestTime2]C  with (nolock) on B.ESN=C.ESN and  B.tdatetime=C.TdateTime and b.station=c.station";
                        }
                        Table.Load(db.ExecuteReader(SQL));
                    }
                }
                return Table;
            }
            catch (Exception e)
            {
                return new DataTable();
            }
        }

        public static Dictionary<string, object> GetItemByItemNameType(int itemnametype)
        {
            var ItemNameDict = new Dictionary<string, object>();
            try
            {
                using (var db = ConnectionFactory.CreatConnection("T1"))
                {
                    var Table = db.Query($@"select * from [ate_result].[dbo].[ItemName] where ItemNameType = '{itemnametype}'").FirstOrDefault();

                    foreach (var raw in Table)
                    {
                        ItemNameDict[raw.Key] = raw.Value ?? string.Empty;
                    }
                    return ItemNameDict;
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return new Dictionary<string, object>();
            }
        }

        public static List<FailItemTable> GetFailDetailInfo_SimplyPart_1(int ItemNameType, string Org, string Sdate, string Edate)
        {
            try
            {
                var Table = GetDBByItemNameType(ItemNameType);
                var Output = new List<FailItemTable>();
                using (var db = ConnectionFactory.CreatConnection(Org))
                {
                    var temp = db.Query<FailItemTable>($@"SELECT ItemNameType,FailItem,count(*) as FailCount
                    FROM {Table}
                    where ItemNameType ='{ItemNameType}'
                    and tDateTime between '{Sdate}' and '{Edate}'
                    and result =0
                    group by ItemNameType,FailItem", commandTimeout: 300).ToList();
                    Output.AddRange(temp);
                }
                var TotalCount = 0;
                using (var db = ConnectionFactory.CreatConnection(Org))
                {
                    TotalCount = db.Query<int>($@"SELECT count(*) as TotalCount
                    FROM {Table}
                    where ItemNameType ='{ItemNameType}'
                    and tDateTime between '{Sdate}' and '{Edate}'
                    and result =1", commandTimeout: 300).FirstOrDefault();
                }

                Output.ForEach(item =>
                {
                    item.TotalCount = TotalCount;
                    item.FailRate = Math.Round(((double)item.FailCount / (double)TotalCount), 3);
                });

                return Output;
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return new List<FailItemTable>();
            }
        }

        public static List<FailItemTable> GetFailDetailInfo(int ItemNameType, string Org, string Sdate, string Edate)
        {
            try
            {
                var Table = GetDBByItemNameType(ItemNameType);
                var Output = new List<FailItemTable>();
                using (var db = ConnectionFactory.CreatConnection(Org))
                {
                    var Output1 = db.Query<FailItemTable>($@"select TotalTable.ItemNameType,FailTable.FailItem,FailTable.FailCount,TotalTable.TotalCount
                    from
                    (SELECT ItemNameType,count(*) as TotalCount
                    FROM {Table}
                    where ItemNameType ='{ItemNameType}'
                    and tDateTime between '{Sdate}' and '{Edate}'
                    group by ItemNameType) AS TotalTable
                    LEFT JOIN
                    (SELECT ItemNameType,FailItem,count(*) as FailCount
                    FROM {Table}
                    where ItemNameType ='{ItemNameType}'
                    and tDateTime between '{Sdate}' and '{Edate}'
                    and result =0
                    group by ItemNameType,FailItem) as FailTable
                    on TotalTable.itemnametype = FailTable.itemnametype", commandTimeout: 60).ToList();

                    var Output2 = db.Query<FailItemTable>($@"select TotalTable.ItemNameType,FailTable.FailItem,FailTable.FailCount,TotalTable.TotalCount
                    from
                    (SELECT ItemNameType,count(SerialNumber) as TotalCount
                    FROM {Table}bt
                    where ItemNameType ='{ItemNameType}'
                    and tDateTime between '{Sdate}' and '{Edate}'
                    group by ItemNameType) AS TotalTable
                    LEFT JOIN
                    (SELECT ItemNameType,FailItem,count(SerialNumber) as FailCount
                    FROM {Table}bt
                    where ItemNameType ='{ItemNameType}'
                    and tDateTime between '{Sdate}' and '{Edate}'
                    and result =0
                    group by ItemNameType,FailItem) as FailTable
                    on TotalTable.itemnametype = FailTable.itemnametype", commandTimeout: 60).ToList();
                    Output.AddRange(Output1);
                    Output.AddRange(Output2);
                    return Output;
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return null;
            }
        }

        public static int GetSamplingTestCountByItemNameTypeAndFailItem(int ItemNameType, int FailItem, string Org, string Sdate, string Edate)
        {
            try
            {
                var Table = GetDBByItemNameType(ItemNameType);

                var _script = $@"SELECT count(*) as TotalCount   FROM {Table}
                                where ItemNameType ='{ItemNameType}' and tDateTime between '{Sdate}' and '{Edate}' and item{FailItem}st in (0,1)";
                using (var db = ConnectionFactory.CreatConnection(Org))
                {
                    return db.Query<int>(_script).FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public static List<FailCorrelation> GetGroupDataByScripts(string type, string Org, int ItemNameType, string Sdate, string Edate, int FailItem)
        {
            var script = string.Empty;
            var script2 = string.Empty;
            var Table = GetDBByItemNameType(ItemNameType);

            var failitemstring = FailItem == 0 ? " !=0" : $@" ={FailItem.ToString()}";

            //Fixture
            if (type == "Fixture")
            {
                if (Table.ToLower() == "tblfinal")
                {
                    script = $@"select AllData.Station,AllData.StationID ,TotalFailCount.FailCount as FailCount,AllData.total as TestCount
                            ,round((cast(TotalFailCount.FailCount as float)/cast(AllData.total as float)),3) as FailRate
                            ,ROUND((CAST(TotalFailCount.FailCount as float)/cast(TotalFail.Toal as float)),3) as FailPercent
                            from
                            (select Station,StationID,count(*) as total
                            from {Table}
                            where ItemNameType ='{ItemNameType}'
                            and tDateTime between '{Sdate}' and '{Edate}'
                            group by Station,StationID) as AllData ,
                            (select Station,StationID,count(*) as FailCount
                            from {Table}
                            where ItemNameType ='{ItemNameType}'
                            and FailItem {failitemstring}
                            and tDateTime between '{Sdate}' and '{Edate}'
                            group by Station,StationID) as TotalFailCount,
                            (select count(*) as Toal
                            from {Table}
                            where ItemNameType ='{ItemNameType}'
                            and FailItem {failitemstring}
                            and tDateTime between '{Sdate}' and '{Edate}') as TotalFail
                            where AllData.Station=TotalFailCount.Station and AllData.StationID=TotalFailCount.StationID and AllData.total>=10
                            order by FailRate desc";
                }
                else
                {
                    script = $@"select AllData.StationID ,TotalFailCount.FailCount as FailCount,AllData.total as TestCount
                            ,round((cast(TotalFailCount.FailCount as float)/cast(AllData.total as float)),3) as FailRate
                            ,ROUND((CAST(TotalFailCount.FailCount as float)/cast(TotalFail.Toal as float)),3) as FailPercent
                            from
                            (select FixtureID2 as StationID,count(*) as total
                            from {Table}
                            where ItemNameType ='{ItemNameType}'
                            and tDateTime between '{Sdate}' and '{Edate}'
                            group by FixtureID2) as AllData ,
                            (select FixtureID2 as StationID,count(*) as FailCount
                            from {Table}
                            where ItemNameType ='{ItemNameType}'
                            and FailItem {failitemstring}
                            and tDateTime between '{Sdate}' and '{Edate}'
                            group by FixtureID2) as TotalFailCount,
                            (select count(*) as Toal
                            from {Table}
                            where ItemNameType ='{ItemNameType}'
                            and FailItem {failitemstring}
                            and tDateTime between '{Sdate}' and '{Edate}') as TotalFail
                            where   AllData.StationID=TotalFailCount.StationID and AllData.total>=10
                            order by FailRate desc";
                }
            }

            //Others
            else
            {
                script = $@"select AllData.{type},TotalFailCount.FailCount as FailCount,AllData.total as TestCount
                        ,round((cast(TotalFailCount.FailCount as float)/cast(AllData.total as float)),3) as FailRate
                        ,ROUND((CAST(TotalFailCount.FailCount as float)/cast(TotalFail.Toal as float)),3) as FailPercent
                        from
                        (select {type},count(*) as total
                        from {Table}
                        where ItemNameType ='{ItemNameType}'
                        and tDateTime between '{Sdate}' and '{Edate}'
                        group by {type}) as AllData ,
                        (select {type},count(*) as FailCount
                        from {Table}
                        where ItemNameType ='{ItemNameType}'
                        and FailItem {failitemstring}
                        and tDateTime between '{Sdate}' and '{Edate}'
                        group by {type}) as TotalFailCount,
                        (select count(*) as Toal
                        from {Table}
                        where ItemNameType ='{ItemNameType}'
                        and FailItem {failitemstring}
                        and tDateTime between '{Sdate}' and '{Edate}') as TotalFail
                        where AllData.{type}=TotalFailCount.{type} and AllData.total>=10
                        order by FailRate desc";
            }

            try
            {
                using (var db = ConnectionFactory.CreatConnection(Org))
                {
                    var result = db.Query<FailCorrelation>(script, commandTimeout: 60).ToList();
                    var Output = new List<FailCorrelation>();
                    Output.AddRange(result);
                    return Output;
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return null;
            }
        }

        public static string GetDBByItemNameType(int ItemNameType)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var Table = db.Query<string>($@"SELECT top(1) Source FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily] where ItemNameType ='{ItemNameType}'").ToList();
                    return Table.FirstOrDefault().ToString() == "ATE" ? "TblCpu" : "TblFinal";
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return null;
            }
        }

        private static List<PTEWEB_ItemNameType_ByDaily> DataCatcherByDateRange(string Sdate, string Edate, string Org)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var Table = db.Query<PTEWEB_ItemNameType_ByDaily>($@"SELECT *FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily] where Date between '{Sdate}' and '{Edate}' and Org ='{Org}'").ToList();

                    return Table;
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return null;
            }
        }

        public static DataTable GetOrgTable(string Org, string SQL)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection(Org))
                {
                    DataTable Table = new DataTable();

                    Table.Load(db.ExecuteReader(SQL, commandTimeout: 30000));
                    return Table;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static List<PTEWEB_Issues_ByDaily> DataIssuesByDateRange()
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var Table = db.Query<PTEWEB_Issues_ByDaily>($@"Select E.*,F.IssueCount,F.Description,F.replyCount From ( select * from[PTEDB].[dbo].[PTEWEB_Issues_ByDaily])E
                                                                                                    inner join
                                                                                                    (select C.ItemNameType,count(D.ItemNameType)as IssueCount,c.Description,C.replyCount from
                                                                                                    (Select A.*,B.Description,b.replyCount FROM(
                                                                                                    (SELECT ItemNameType,Status,Support_Org,LastUpdateDate,Title_id_3Day,Title_id_14Day  FROM [PTEDB].[dbo].[PTEWEB_Issues_ByDaily]  )A
                                                                                                     left join
                                                                                                    (SELECT distinct x.ItemNameType,Description,count(y.ItemNameType)as replyCount FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily]x
                                                                                                    left join PTEWEB_Issues_Content y on x.ItemNameType=y.ItemNameType group by x.ItemNameType,x.Description
                                                                                                    )B on a.ItemNameType=B.ItemNameType)
                                                                                                    )C left join [PTEDB].[dbo].[PTEWEB_Issues_Title]D on C.ItemNameType=D.ItemNameType group by C.ItemNameType,C.Description,C.replyCount)F on E.itemNameType=F.ItemNameType
                                                                                                    order by E.LastUpdateDate desc").ToList();
                    Table.ForEach(item =>
                    {
                        var causeissueid = GetCauseIssueID(item.Title_id_3Day, item.Title_id_14Day, item.Title_id_Custom);
                        item.LinkImg = item.replyCount > 0 ? "<img src=\"../Content/hand.png\" title=\"查看回應\"/>" : "";

                        item.CauseIssueID = causeissueid;
                    });

                    return Table;
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return null;
            }
        }

        public static int GetCauseIssueID(string id_3d, string id_14d, string id_custom)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var id = db.Query<int>($@"SELECT Title_id  FROM [PTEDB].[dbo].[PTEWEB_Issues_Title] where Title_id in ({id_14d ?? "0"},{id_3d ?? "0"},{id_custom ?? "0"}) order by CreateDate asc").ToList();
                    return id.Count != 0 ? id.First() : 0;
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return 0;
            }
        }

        public static Class.MonthCpkData GetMonthCpkData(DataTable dataTable, double MinSpec, double MaxSpec, string Item, string st, string et, string table)
        {
            Class.MonthCpkData cpkData = new Class.MonthCpkData();
            var TestGroups = from result in dataTable.AsEnumerable()
                             group result by result["TestDate"] into g
                             select new
                             {
                                 Date = g.Key.ToString(),
                                 Data = g.ToList()
                             };
            cpkData.CPKs = new List<double>();
            cpkData.FailRates = new List<double>();
            cpkData.L_Date = new List<string>();

            foreach (var t in TestGroups)
            {
                cpkData.L_Date.Add(t.Date);

                cpkData.FailRates.Add(Math.Round((((double)t.Data.AsEnumerable().Where(x => x.Field<string>("FailItem") == Item.Substring(4)).Count() * 100) / t.Data.Count()), 2));
                if (table == "TblCPU")
                {
                    cpkData.CPKs.Add(new Class.cPk(t.Data.AsEnumerable().Select(x => Convert.ToDouble(x.Field<Single>(Item))).ToList(), Convert.ToDouble(MaxSpec), Convert.ToDouble(MinSpec)).Cpk);
                }
                else
                {
                    cpkData.CPKs.Add(new Class.cPk(t.Data.AsEnumerable().Select(x => x.Field<double>(Item)).ToList(), Convert.ToDouble(MaxSpec), Convert.ToDouble(MinSpec)).Cpk);
                }
            }
            return cpkData;
        }

        public static List<TopSpareInfo> GetTopSpareItemByItemNameType(string Sdate, string Edate, int ItemNameType)
        {
            var PreData = GetTop10SpareByDate_Itemnametype(Sdate, Edate, ItemNameType);
            var LongCycleItemList = DeltaIModel.TopSpareTableTransferToSpareList(PreData);
            var DistinctItemList = DistincItemList(LongCycleItemList.Select(item => item.FailItemName).ToList());
            var Output = new List<TopSpareInfo>();

            DistinctItemList.ForEach(item =>
            {
                var MaxSpare = 0.0;
                foreach (var iteminfo in LongCycleItemList)
                {
                    if (iteminfo.FailItemName == item && iteminfo.Spare > MaxSpare)
                    {
                        MaxSpare = iteminfo.Spare;
                    }
                }
                Output.Add(new TopSpareInfo { FailItemName = item, Spare = MaxSpare });
            });

            return Output.OrderBy(x => x.Spare).ToList();
        }

        private static List<PTEWEB_ItemNameType_ByDaily_TOP10_LongCycleTime> GetTop10SpareByDate_Itemnametype(string Sdate, string Edate, int ItemNameType)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    return db.Query<PTEWEB_ItemNameType_ByDaily_TOP10_LongCycleTime>($@"SELECT *
                      FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily_TOP10_LongCycleTime]
                      where ItemNameType ='{ItemNameType}'  and Date between '{Sdate}' and '{Edate}'  order by date desc").ToList();
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return null;
            }
        }

        public static List<TopFailInfo> GetTopFailItemByItemNameType(string Sdate, string Edate, int ItemNameType)
        {
            var PreData = GetTop10FailItemByDate_Itemnametype(Sdate, Edate, ItemNameType);
            var FailItemList = DeltaIModel.TopFailTableTransferToFailInfoList(PreData);
            var DistinctItemList = DistincItemList(FailItemList.Select(item => item.FailItemName).ToList());
            var Output = new List<TopFailInfo>();

            DistinctItemList.ForEach(ItemName =>
            {
                var ItemCount = 0;
                foreach (var ItemInfo in FailItemList)
                {
                    if (ItemInfo.FailItemName == ItemName)
                    {
                        ItemCount += ItemInfo.FaiCount;
                    }
                }
                Output.Add(new TopFailInfo { FailItemName = ItemName, FaiCount = ItemCount });
            });

            return Output.OrderBy(x => x.FaiCount).ToList();
        }

        private static List<string> DistincItemList(List<string> FailList)
        {
            var DistinctItem = new List<string>();
            foreach (var item in FailList)
            {
                if (!DistinctItem.Contains(item))
                {
                    DistinctItem.Add(item);
                }
            }
            return DistinctItem;
        }

        private static List<PTEWEB_ItemNameType_ByDaily_TOP10_FailItem> GetTop10FailItemByDate_Itemnametype(string Sdate, string Edate, int ItemNameType)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    return db.Query<PTEWEB_ItemNameType_ByDaily_TOP10_FailItem>($@"SELECT *
                      FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily_TOP10_FailItem]
                      where ItemNameType ='{ItemNameType}'  and Date between '{Sdate}' and '{Edate}'  order by date desc").ToList();
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return null;
            }
        }

        public static List<ItemNameTypeDeltaInfo> GetDeltaListByItemNameType(string Sdate, string Edate, int ItemNameType)
        {
            var AllRangeDeltaInfo = (from item in DataCatcherByDateRange(Sdate, Edate)
                                     where item.ItemNameType == ItemNameType
                                     select item).ToList().OrderBy(x => x.Date).ToList();
            var Output = DeltaIModel.GetRangeDeltaByItemNameType(AllRangeDeltaInfo);
            return Output;
        }

        public static Dictionary<string, string> GetFocusFYRCountByOrg(List<PTEWEB_ItemNameType_ByDaily> Alldata)
        {
            var Output = new Dictionary<string, string>();
            var OrgList = new List<string>() { "T1", "T2", "T3", "T5" };
            OrgList.ForEach(_org =>
            {
                var OrgTableATE = (from item in Alldata
                                   where item.Org == _org && item.Source == "ATE"
                                   select new
                                   {
                                       source = item.Source,
                                       total = item.D_Total,
                                       fail = item.D_Fail
                                   }).ToList();
                var OrgTableFT = (from item in Alldata
                                  where item.Org == _org && item.Source == "FT"
                                  select new
                                  {
                                      source = item.Source,
                                      total = item.D_Total,
                                      fail = item.D_Fail
                                  }).ToList();
                var AteValue = Math.Round((1 - ((float)OrgTableATE.Sum(x => x.fail) / (float)OrgTableATE.Sum(y => y.total))) * 100, 2).ToString() + "%";
                var FtValue = Math.Round((1 - ((float)OrgTableFT.Sum(x => x.fail) / (float)OrgTableFT.Sum(y => y.total))) * 100, 2).ToString() + "%";
                Output.Add(_org + "ATE", AteValue);
                Output.Add(_org + "FT", FtValue);
            });
            return Output;
        }

        public static Dictionary<string, string> GetFocusSpareCountByOrg()
        {
            return null;
        }

        public static Dictionary<string, string> GetGroupATEandFTPerformace(List<PTEWEB_ItemNameType_ByDaily> Alldata, List<string> _groupList)
        {
            var Output = new Dictionary<string, string>();
            var GroupList = _groupList;
            GroupList.ForEach(groupName =>
            {
                var Key = GroupList.Count > 1 ? groupName : string.Empty;

                var OrgTableATE = (from item in Alldata
                                   where item.Org == groupName || item.Source == "ATE" && item.Total > 500
                                   select new
                                   {
                                       source = item.Source,
                                       total = item.D_Total,
                                       fail = item.D_Fail
                                   }).ToList();
                var OrgTableFT = (from item in Alldata
                                  where item.Org == groupName || item.Source == "FT" && item.Total > 500
                                  select new
                                  {
                                      source = item.Source,
                                      total = item.D_Total,
                                      fail = item.D_Fail
                                  }).ToList();
                var AteValue = Math.Round((1 - ((float)OrgTableATE.Sum(x => x.fail) / (float)OrgTableATE.Sum(y => y.total))) * 100, 2).ToString() + "%";
                var FtValue = Math.Round((1 - ((float)OrgTableFT.Sum(x => x.fail) / (float)OrgTableFT.Sum(y => y.total))) * 100, 2).ToString() + "%";
                Output.Add(Key + "ATE", AteValue);
                Output.Add(Key + "FT", FtValue);
            });
            return Output;
        }

        public static Dictionary<string, List<string>> GetGroupATEandFT_FYR_HL(List<PTEWEB_ItemNameType_ByDaily> Alldata, List<string> _groupList)
        {
            var Output = new Dictionary<string, List<string>>();
            var HLList = new List<string>();
            var GroupList = _groupList;
            GroupList.ForEach(groupName =>
            {
                var Key = GroupList.Count > 1 ? groupName : string.Empty;

                var OrgTableATE = (from item in Alldata
                                   where item.Org == groupName || item.Source == "ATE" && item.Total > 500
                                   select new
                                   {
                                       FYR = item.FYR
                                   }).ToList();
                var OrgTableFT = (from item in Alldata
                                  where item.Org == groupName || item.Source == "FT" && item.Total > 500
                                  select new
                                  {
                                      FYR = item.FYR
                                  }).ToList();
                var AteValueH = OrgTableATE.Count != 0 ? OrgTableATE.Max(x => x.FYR).ToString() + " %" : "0 %";
                var AteValueL = OrgTableATE.Count != 0 ? OrgTableATE.Min(x => x.FYR).ToString() + " %" : "0 %";
                var FtValueH = OrgTableFT.Count != 0 ? OrgTableFT.Max(x => x.FYR).ToString() + " %" : "0 %";
                var FtValueL = OrgTableFT.Count != 0 ? OrgTableFT.Min(x => x.FYR).ToString() + " %" : "0 %";
                Output.Add(Key + "ATE", new List<string>() { AteValueH, AteValueL, OrgTableATE.Count.ToString() });
                Output.Add(Key + "FT", new List<string>() { FtValueH, FtValueL, OrgTableFT.Count.ToString() });
            });
            return Output;
        }

        public static Dictionary<string, string> GetTotalCountByOrg(List<PTEWEB_ItemNameType_ByDaily> Alldata)
        {
            var Output = new Dictionary<string, string>();

            var OrgList = new List<string>() { "T1", "T2", "T3", "T5" };
            foreach (var _org in OrgList)
            {
                var _count = (from item in Alldata
                              where item.Org == _org
                              select item.Total).ToList().Sum();

                Output.Add(_org, _count.ToString("N"));
            }
            return Output;
        }

        public static Dictionary<string, string> GetFocusCountByOrg(List<PTEWEB_ItemNameType_ByDaily> Alldata)
        {
            var Output = new Dictionary<string, string>();

            var OrgList = new List<string>() { "T1", "T2", "T3", "T5" };
            foreach (var _org in OrgList)
            {
                var _countA = (from item in Alldata
                               where item.Org == _org && item.Total > 500 && item.FYR < 90 && item.Source == "ATE"
                               select item.Total).ToList().Count();

                var _countF = (from item in Alldata
                               where item.Org == _org && item.Total > 500 && item.FYR < 90 && item.Source == "FT"
                               select item.Total).ToList().Count();

                Output.Add(_org + "A", _countA.ToString("N"));
                Output.Add(_org + "F", _countF.ToString("N"));
            }
            return Output;
        }

        public static Models.PTEWEB_Issue_Replys GetIssues(int ItemNameType, string date)
        {
            PTEWEB_Issue_Replys IR = new PTEWEB_Issue_Replys();
            IR.Issues = new List<PTEWEB_Issues_Title>();
            IR.Contents = new List<PTEWEB_Issues_Reply>();
            using (var db = ConnectionFactory.CreatConnection())
            {
                var table = db.Query<PTEWEB_Issues_Reply>($@"SELECT *
                      FROM [PTEDB].[dbo].[PTEWEB_Issues_Content]
                      where ItemNameType ='{ItemNameType}'  and CreateTime>= '{date + " 00:00:00"}' and CreateTime < '{date + " 23:59:59"}'  order by CreateTime desc").ToList();
                table.ForEach(item =>
                {
                    IR.Contents.Add(new PTEWEB_Issues_Reply
                    {
                        UserName = item.UserName,
                        ActionCommon = item.ActionCommon,
                        CauseCommon = item.CauseCommon,
                        fileName = item.fileName,
                        CreateTime = Convert.ToDateTime(item.CreateTime).ToString("yyyy-MM-dd hh:mm:ss")
                    });
                });
            }
            using (var db = ConnectionFactory.CreatConnection())
            {
                var table = db.Query<PTEWEB_Issues_Title>($@"SELECT *
                      FROM [PTEDB].[dbo].[PTEWEB_Issues_Title]
                      where ItemNameType ='{ItemNameType}'  and CreateDate = '{date}'  order by CreateDate desc").ToList();
                table.ForEach(item =>
                {
                    IR.Issues.Add(new PTEWEB_Issues_Title
                    {
                        Title = item.Title,
                        Issue_Status = item.Issue_Status,
                        ItemNameType = item.ItemNameType,
                        Title_id = item.Title_id,
                        CreateDate = Convert.ToDateTime(item.CreateDate).ToString("yyyy-MM-dd 00:00:00.000")
                    });
                });
            }
            return IR;
        }

        public static CpkModel GetItemNameTypeCpkModel_CorrelationType(int ItemNameType, string Type, string groupstring, string Org, string st, string et, int FailItem, double specMin, double specMax)
        {
            string TargetTestTable = GetDBByItemNameType(ItemNameType);
            string fliter = string.Empty;

            DataTable ItemValueAVGTable = new DataTable();
            Models.CpkModel cpkModel = new Models.CpkModel();
            //cpkModel.Items = new Dictionary<int, Tuple<string, string, double>>();
            cpkModel.CpkTable = new List<Models.TableRow>();
            try
            {
                Models.TableRow tr = new Models.TableRow();
                DataRow tempRow = ItemValueAVGTable.NewRow();
                tr.DBIndex = (FailItem).ToString();
                tr.MaxSpec = specMax.ToString();
                tr.MinSpec = specMin.ToString();

                string SQLCommand = "";

                SQLCommand = "Select '" + (FailItem) + "' as Item,serialnumber, Item" + (FailItem) + " as Item" + (FailItem) + "_Value,Item" + (FailItem) + "st as Item" + (FailItem) + "_Status,'" + tr.MinSpec + "' as SpecMin,'" + tr.MaxSpec + "' as SpecMax,tdatetime,result FROM " + TargetTestTable +
                             " WHERE Item" + (FailItem) + "!= '0' and Item" + (FailItem) + "st !=2 AND " + TargetTestTable + ".ItemNameType = '" + ItemNameType + "' and tdatetime>='" + st + "' and tdatetime<'" + et + "'";

                if (Type == "Fixture")
                {
                    if (TargetTestTable == "TblFinal")
                    {
                        var id2 = groupstring.Substring(groupstring.Length - 1, 1);
                        var id1 = groupstring.Remove(groupstring.Length - 2);
                        fliter = $@" and station = '{id1}' and stationid = {id2}";
                    }
                    else
                    {
                        fliter = $@" and fixtureid2 ='{groupstring}'";
                    }
                }
                else
                {
                    fliter = $@" and {Type} ='{groupstring}'";
                }

                SQLCommand += fliter;

                DataTable _data = GetOrgTable(Org, SQLCommand);

                // cpkModel.rawData = JsonConvert.SerializeObject(_data, Formatting.Indented);

                List<double> cpkArray = new List<double>();
                for (int index = 0; index < _data.Rows.Count - 1; index++)
                {
                    var tempdouble = Math.Round(Convert.ToDouble(_data.Rows[index]["Item" + (FailItem) + "_Value"]), 4);
                    cpkArray.Add(tempdouble);
                }
                tr.TestCount = _data.Rows.Count.ToString();
                Class.cPk cpkdata = new Class.cPk(cpkArray, specMax, specMin);
                tr.Cpk = cpkdata.Cpk;
                tr.SD = cpkdata.SD;
                tr.AVG = cpkdata.AVG;
                tr.Spec = specMin + " - " + specMax;
                tr.DataRange = st + " - " + et;
                if (TargetTestTable.ToLower() == "tblcpu")
                {
                    tr.FailCount = _data.AsEnumerable().Where(x => x.Field<bool>("result") == false).Count();
                }
                else
                {
                    tr.FailCount = _data.AsEnumerable().Where(x => x.Field<Int32>("result") == 0).Count();
                }
                tr.FailRate = ((float)tr.FailCount / (float.Parse(tr.TestCount))).ToString();
                tr.dataMin = Convert.ToDouble(tr.MinSpec) < cpkArray.Min() ? Convert.ToDouble(tr.MinSpec) : cpkArray.Min();
                tr.dataMax = Convert.ToDouble(tr.MaxSpec) > cpkArray.Max() ? Convert.ToDouble(tr.MaxSpec) : cpkArray.Max();
                ItemValueAVGTable.Rows.Add(tempRow);
                //Dictionary<int, Tuple<List<double>, string>> CpkData = new Dictionary<int, Tuple<List<double>, string>>();
                var jsonSerialiser = new JavaScriptSerializer();
                tr.PassStr = jsonSerialiser.Serialize(cpkArray.Where(x => x >= specMin && x <= specMax).Select(x => x).ToList());
                tr.FailStr = jsonSerialiser.Serialize(cpkArray.Where(x => x < specMin || x > specMax).Select(x => x).ToList());
                //cpkModel.Items.Add(Convert.ToInt16(tr.DBIndex), Tuple.Create(jsonPass, jsonFail, tr.Cpk));
                cpkModel.CpkTable.Add(tr);
            }
            catch (Exception ex)
            {
                string s = "";
            }
            return (cpkModel);
        }

        public static CpkModel GetItemNameTypeCpkModel(int ItemNameType, string Source, string Org, string st, string et, int FailItem, double specMin, double specMax)
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
            DataTable ItemValueAVGTable = new DataTable();
            Models.CpkModel cpkModel = new Models.CpkModel();
            //cpkModel.Items = new Dictionary<int, Tuple<string, string, double>>();
            cpkModel.CpkTable = new List<Models.TableRow>();
            try
            {
                Models.TableRow tr = new Models.TableRow();
                DataRow tempRow = ItemValueAVGTable.NewRow();
                tr.DBIndex = (FailItem).ToString();
                tr.MaxSpec = specMax.ToString();
                tr.MinSpec = specMin.ToString();

                string SQLCommand = "";

                SQLCommand = "Select '" + (FailItem) + "' as Item,serialnumber," +
                    " Item" + (FailItem) + " as Item" + (FailItem) + "_Value,Item" + (FailItem) + "st as Item" + (FailItem) + "_Status,'" + tr.MinSpec + "' as SpecMin,'" + tr.MaxSpec + "' as SpecMax,tdatetime,result FROM " + TargetTestTable +
                             " WHERE  Item" + (FailItem) + "!= '0' and Item" + FailItem + "<>-999 and Item" + (FailItem) + "st !=2 AND " + TargetTestTable + ".ItemNameType = '" + ItemNameType + "' and tdatetime>='" + st + "' and tdatetime<'" + et + "'";

                DataTable _data = GetOrgTable(Org, SQLCommand);

                // cpkModel.rawData = JsonConvert.SerializeObject(_data, Formatting.Indented);

                List<double> cpkArray = new List<double>();
                for (int index = 0; index < _data.Rows.Count - 1; index++)
                {
                    var tempdouble = Math.Round(Convert.ToDouble(_data.Rows[index]["Item" + (FailItem) + "_Value"]), 4);
                    cpkArray.Add(tempdouble);
                }
                tr.TestCount = _data.Rows.Count.ToString();
                Class.cPk cpkdata = new Class.cPk(cpkArray, specMax, specMin);
                tr.Cpk = cpkdata.Cpk;
                tr.SD = cpkdata.SD;
                tr.AVG = cpkdata.AVG;
                tr.Spec = specMin + " - " + specMax;
                tr.DataRange = st + " - " + et;
                if (TargetTestTable == "TblCPU")
                {
                    tr.FailCount = _data.AsEnumerable().Where(x => x.Field<bool>("result") == false).Count();
                }
                else
                {
                    tr.FailCount = _data.AsEnumerable().Where(x => x.Field<Int32>("result") == 0).Count();
                }
                tr.dataMin = Convert.ToDouble(tr.MinSpec) < cpkArray.Min() ? Convert.ToDouble(tr.MinSpec) : cpkArray.Min();
                tr.dataMax = Convert.ToDouble(tr.MaxSpec) > cpkArray.Max() ? Convert.ToDouble(tr.MaxSpec) : cpkArray.Max();
                ItemValueAVGTable.Rows.Add(tempRow);
                //Dictionary<int, Tuple<List<double>, string>> CpkData = new Dictionary<int, Tuple<List<double>, string>>();
                var jsonSerialiser = new JavaScriptSerializer();
                tr.PassStr = jsonSerialiser.Serialize(cpkArray.Where(x => x >= specMin && x <= specMax).Select(x => x).ToList());
                tr.FailStr = jsonSerialiser.Serialize(cpkArray.Where(x => x < specMin || x > specMax).Select(x => x).ToList());
                //cpkModel.Items.Add(Convert.ToInt16(tr.DBIndex), Tuple.Create(jsonPass, jsonFail, tr.Cpk));
                cpkModel.CpkTable.Add(tr);
            }
            catch (Exception ex)
            {
                string s = "";
            }
            return (cpkModel);
        }

        public static void AutoImportINI(string currentItemNameType, out Dictionary<string, string> UpperSpec, out Dictionary<string, string> LowerSpec, out Dictionary<string, string> ItemName_DBIndex, string Org)
        {
            UpperSpec = new Dictionary<string, string>();
            LowerSpec = new Dictionary<string, string>();
            ItemName_DBIndex = new Dictionary<string, string>();

            ITMXPServerQuery queryitmxp = new ITMXPServerQuery();
            //先得到相關於ItemNameType的file index
            string ITMXPQueryString = "select * from itmxp.testfile,itmxp.tbl_testinfo where itmxp.testfile.unikey = itmxp.tbl_testinfo.Idx_file and itmxp.tbl_testinfo.ItemNameType = '" + currentItemNameType + "' order by unikey desc";
            DataTable tempINITable = queryitmxp.QueryResult(ITMXPQueryString);
            string CurrentIdx_file = "0";
            for (int i = 0; i < tempINITable.Rows.Count; i++)
            {
                if ((tempINITable.Rows[i]["StationType"].ToString().Trim() != "5") && (tempINITable.Rows[i]["StationType"].ToString().Trim() != "9"))
                {
                    CurrentIdx_file = tempINITable.Rows[i]["Idx_file"].ToString();
                    break;
                }
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
            ITMXPQueryString = @"select itmxp.testfile.XMLName ,itmxp.tbl_fileversion.AutoVersion ,itmxp.tbl_fileversion.IsReleased,itmxp.tbl_testitem.DBIndex, itmxp.tbl_testitem.Double1,itmxp.tbl_testitem.Double2,itmxp.tbl_testitem.Unit
							   from itmxp.tbl_testitem inner join itmxp.tbl_fileversion on ( itmxp.tbl_testitem.Idx_Version = itmxp.tbl_fileversion.Idx_Version )
							   inner join itmxp.testfile on ( itmxp.tbl_fileversion.Idx_file = itmxp.testfile.unikey )
							   where itmxp.testfile.unikey = '" + CurrentIdx_file + @"' and itmxp.tbl_fileversion.AutoVersion='" + NeweastVersion + "'group by itmxp.tbl_testitem.DBIndex";
            tempINITable = queryitmxp.QueryResult(ITMXPQueryString);

            //將ini加到list裡面
            for (int i = 0; i < tempINITable.Rows.Count; i++)
            {
                UpperSpec.Add(tempINITable.Rows[i]["DBIndex"].ToString(), tempINITable.Rows[i]["Double1"].ToString());
                LowerSpec.Add(tempINITable.Rows[i]["DBIndex"].ToString(), tempINITable.Rows[i]["Double2"].ToString());
            }

            string ItemNameQueryString = "select * from  [ate_result].[dbo].[ItemName] where ItemNameType = '" + currentItemNameType + "'";

            DataTable ItemNameTable = DataHandlerFunctions.GetOrgTable(Org, ItemNameQueryString);
            for (int i = 1; i <= 250; i++)
            {
                ItemName_DBIndex.Add(i.ToString(), ItemNameTable.Rows[0]["Name" + i.ToString()].ToString());
            }
        }

        public static int GetProductionMapInfo(string tempGPN, string Org, int ItemNametype)
        {
            try
            {
                var RefGPN = GetRefGPNMap(tempGPN, Org) ?? tempGPN;

                RefGPN = RefGPN.Replace("XX", "%");
                var ItemCinfig = new TestItemConfig();
                ItemCinfig = GetTestConfigByItemNameType(ItemNametype);

                var scripts = "";

                if (ItemCinfig.Source != "TblCpu")
                    scripts = $@"SELECT top(1)POH  FROM [CAPA_DB].[dbo].[CAPA_Process_Map]  WHERE Top_Item like '{RefGPN}' and TestType='{ItemCinfig.TestType}' and TestType2 ='{ItemCinfig.TestType2}'  order by Refresh_time desc";
                else
                    scripts = $@"SELECT top(1)POH  FROM [CAPA_DB].[dbo].[CAPA_Process_Map_SMT]  WHERE Top_Item ='{RefGPN}' and TestType='{ItemCinfig.TestType}'  order by Refresh_time desc";

                using (var db = ConnectionFactory.CreatConnection())
                {
                    return db.Query<int>(scripts).FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return 999;
            }
        }

        public static TestItemConfig GetTestConfigByItemNameType(int Itemnametype)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var temp = db.Query<TestItemConfig>($@"SELECT top(1) Description, TestType,TestType2,Source  FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily] where ItemNameType ={Itemnametype} order by date desc ").FirstOrDefault();
                    temp.Itemnametype = Itemnametype;
                    temp.Source = temp.Source == "ATE" ? "TblCpu" : "TblFinal";
                    temp.TestType2 = temp.TestType2 == "" ? "NA" : temp.TestType2;

                    return temp;
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return null;
            }
        }

        //Undo
        public static RealTimeFixtureInfo GetRealFixtureCorrelationInfo(DateTime start, string org, int itemnametype)
        {
            return null;
        }

        public static List<ItemNameType_Table> GetItemNameTypeByHour(string org, DateTime Timeflag, string table, int hour)
        {
            var Output = new List<ItemNameType_Table>();
            var queryDate = Timeflag.AddHours(-8);
            try
            {
                using (var db = ConnectionFactory.CreatConnection(org))
                {
                    var script = $@"select final.ItemNameType  from (select ItemNameType, SerialNumber, Result, ROW_NUMBER() over(partition by SerialNumber order by Result) as raw_index  from ate_db.dbo.{table}    where   Station like '%{org}%'  and  tdatetime between '{queryDate.ToString("yyyy-MM-dd")} {queryDate.Hour}:00:00' and '{queryDate.ToString("yyyy-MM-dd")} {queryDate.AddHours(hour).Hour}:00:00')
                                    final join ate_result.dbo.ItemName as itemName on final.ItemNameType = itemName.ItemNameType   where final.raw_index = 1  group by  final.ItemNameType  order by  final.ItemNameType";
                    //var script = $"select itemnametype from {table} where tdatetime between '{queryDate.ToString("yyyy-MM-dd")} {queryDate.Hour}:00:00' and '{queryDate.ToString("yyyy-MM-dd")} {queryDate.AddHours(+1).Hour}:00:00'  group by ItemNameType order by ItemNameType asc";
                    var itemList = db.Query<int>(script).ToList();
                    foreach (var item in itemList)
                    {
                        Output.Add(new ItemNameType_Table { ItemNametype = item, Table = table });
                    }
                    return Output.ToList();
                }
            }
            catch (Exception e)
            {
                return new List<ItemNameType_Table>();
            }
        }

        public static List<Estimate_UPH> GetEstimateUPH(string org, int itemnametype, string table, DateTime thishour)
        {
            var output = new List<Estimate_UPH>();
            var queryDate = thishour.AddHours(-8);

            try
            {
                using (var db = ConnectionFactory.CreatConnection(org))
                {
                    var script = $@"select COUNT(*)as RealOutput,(cast(Datediff(MINUTE,MIN(tDateTime),MAX(tDateTime))as float)/60)as EstimateHours,COUNT(*)/(cast(Datediff(MINUTE,MIN(tDateTime),MAX(tDateTime))as float)/60) AS EstimateUPH ,AVG(cast(Spare as int)) as AvgSpare from {table}  where itemnametype ='{itemnametype.ToString()}'  and tDateTime BETWEEN '{queryDate.ToString("yyyy-MM-dd")} {queryDate.Hour.ToString()}:00:00' AND '{queryDate.ToString("yyyy-MM-dd")} {queryDate.AddHours(+1).Hour.ToString()}:00:00' AND FailItem =0";
                    output = db.Query<Estimate_UPH>(script).ToList();
                    output[0].itemnametype = itemnametype;
                    output[0].productname = GetTestConfigByItemNameType(itemnametype).Description;
                    output[0].TimeIndex = thishour.Hour;
                    output[0].table = table;
                }
                return output;
            }
            catch (Exception e)
            {
                return new List<Estimate_UPH>();
            }
        }

        public static List<PTE_Web.Class.SpareDaily> GetSpareDaily(int itemnametype, string org, string st, string et)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var script = $@"select CAST(DateTime AS DATE)as DateTime ,AvgSpare as Spare  FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_SpareTime_ByDaily] where ItemNameType ='{itemnametype}' and datetime>='{st}' and datetime<='{et}' and org='{org}' order by DateTime asc ";
                    var result = db.Query<PTE_Web.Class.SpareDaily>(script);
                    List<PTE_Web.Class.SpareDaily> dataList = new List<Class.SpareDaily>();
                    if (result.Count() > 0)
                    {
                        foreach (var r in result)
                        {
                            Class.SpareDaily sc = new Class.SpareDaily();
                            sc.DateTime = Convert.ToDateTime(r.DateTime).ToString("yyyy-MM-dd");
                            sc.Spare = r.Spare;
                            dataList.Add(sc);
                        }
                    }

                    return dataList;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static List<PTE_Web.Class.GPNSpare> GetSpareGPN(int itemnametype, string st, string et)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var script = $@"SELECT  [DateTime],[GPN],[Spare] FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_SpareDetail_ByDaily] where ItemNameType ='{itemnametype}' and datetime>='{st}' and datetime<='{et}' and PassCount>50 order by DateTime asc ";
                    var result = db.Query<PTE_Web.Class.SpareDaily>(script);
                    List<PTE_Web.Class.SpareDaily> dataList = new List<Class.SpareDaily>();
                    if (result.Count() > 0)
                    {
                        dataList = result.ToList();
                    }
                    List<PTE_Web.Class.GPNSpare> ListGPN = new List<Class.GPNSpare>();
                    var List = from r in dataList
                               group r by r.GPN into g
                               select new
                               {
                                   GPN = g.Key,
                                   Spare = g.Select(x => x.Spare).ToList()
                               };
                    foreach (var l in List)
                    {
                        Class.GPNSpare _gpnSpare = new Class.GPNSpare();
                        _gpnSpare.GPN = l.GPN;
                        _gpnSpare.Spare = new List<double>();
                        _gpnSpare.Spare = l.Spare;
                        ListGPN.Add(_gpnSpare);
                    }
                    return ListGPN;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private static string GetRefGPNMap(string GPN, string Org)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    return db.Query<string>($@"SELECT TOP (1) [Refer_GPN]  FROM [CAPA_DB].[dbo].[CAPA_ProductionMap_Refer] where gpn ='{GPN}' and ORG='{Org}' order by Refresh_time desc").FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return GPN;
            }
        }

        public static bool UpdateToIssueActionTable(int issueid, int itemnametype, PTEWEB_Issues_Reply input, string org, int trace_flag)
        {
            try
            {
                var thisday = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                using (var db = ConnectionFactory.CreatConnection())
                {
                    if (Convert.ToBoolean(trace_flag))
                    {
                        db.Execute($@"DELETE FROM [PTEDB].[dbo].[PTEWEB_RealTimeTrack] WHERE ItemNameType='{itemnametype}' and Empid='{input.UserName}' ");
                        db.Execute($@"INSERT INTO [PTEDB].[dbo].[PTEWEB_RealTimeTrack] (ItemNameType, Follower_Mail, Timestamp,Issue_Org,Status,Empid) VALUES ({itemnametype},'{MappingEmpMail(input.UserName)}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}','{org}','{trace_flag}','{input.UserName}');");
                    }
                    db.Execute($@"INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_Content] (Title_id, ItemNameType, Actionid,UserName,ActionCommon,CauseCommon,CreateTime,Owner,Causeid,FailItem,fileName,Valid) VALUES ({issueid}, {itemnametype},{input.Actionid},'{input.UserName}','{input.ActionCommon}','{input.CauseCommon}','{thisday}','{input.Owner}',{input.Causeid},'{input.FailItem}','{input.fileName}',0);");
                    return true;
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace($@"Insert Issue Action Fail{e.ToString()} ");
                return false;
            }
        }

        public static bool UpdateTrackingActionTable(int itemnametype, string Empid, string Org)
        {
            try
            {
                var thisday = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                using (var db = ConnectionFactory.CreatConnection())
                {
                    db.Execute($@"DELETE FROM [PTEDB].[dbo].[PTEWEB_RealTimeTrack] WHERE ItemNameType='{itemnametype}' and Empid='{Empid}' ");
                    db.Execute($@"INSERT INTO [PTEDB].[dbo].[PTEWEB_RealTimeTrack] (ItemNameType, Follower_Mail, Timestamp,Issue_Org,Status,Empid) VALUES ({itemnametype},'{MappingEmpMail(Empid)}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}','{Org}','{1}','{Empid}');");

                    // db.Execute($@"INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_Content] (Title_id, ItemNameType, Actionid,UserName,ActionCommon,CauseCommon,CreateTime,Owner,Causeid,FailItem,fileName,Valid) VALUES ({issueid}, {itemnametype},{input.Actionid},'{input.UserName}','{input.ActionCommon}','{input.CauseCommon}','{thisday}','{input.Owner}',{input.Causeid},'{input.FailItem}','{input.fileName}',0);");
                    return true;
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace($@"Insert Tracing Action Fail{e.ToString()} ");
                return false;
            }
        }

        public static bool RemoveTrackingActionTable(int itemnametype, string Empid, string Org)
        {
            try
            {
                var thisday = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                using (var db = ConnectionFactory.CreatConnection())
                {
                    db.Execute($@"DELETE FROM [PTEDB].[dbo].[PTEWEB_RealTimeTrack] WHERE ItemNameType='{itemnametype}' and Empid='{Empid}' and Issue_Org='{Org}' ");
                    return true;
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace($@"Insert Tracing Action Fail{e.ToString()} ");
                return false;
            }
        }

        public static bool CloseIssue(int itemnametype, string org)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    db.Execute($@"update [PTEDB].[dbo].[PTEWEB_Issues_Title]  set Issue_Status =0 where ItemNameType = {itemnametype.ToString()} and Org = '{org}' and Issue_Status =1");

                    db.Execute($@"update [PTEDB].[dbo].[PTEWEB_Issues_ByDaily]  set status =0 where Support_Org ='{org}' and ItemNameType = {itemnametype.ToString()}");

                    db.Execute($@"insert into [PTEDB].[dbo].[PTEWEB_Issues_History] (ItemNameType,Status,Contents,DateTime,Org) values ({itemnametype.ToString()},0,'結案','{DateTime.Today.ToString("yyyy-MM-dd")}','{org}')");
                }
                return true;
            }
            catch (Exception e)
            {
                _PTElogger.Trace($@"Close Issue Fail{e.ToString()} ");
                return false;
            }
        }

        private static string MappingMailandName(string mail)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    return db.Query<string>($@"SELECT EmpID  FROM [EmployeeBasic].[dbo].[Employee_Basic] where Email ='{mail}'").ToList().First();
                }
            }
            catch (Exception e)
            {
                return "UnKnown";
            }
        }

        private static string MappingEmpMail(string Empid)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    return db.Query<string>($@"SELECT Email  FROM [EmployeeBasic].[dbo].[Employee_Basic] where Empid ='{Empid}'").ToList().First();
                }
            }
            catch (Exception e)
            {
                return "UnKnown";
            }
        }

        public static List<PTEWEB_Issues_Owner> GetEditorList()
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    return db.Query<PTEWEB_Issues_Owner>($@"SELECT * FROM [PTEDB].[dbo].[PTEWEB_Issues_Owner]").ToList();
                }
            }
            catch (Exception e)
            {
                return new List<PTEWEB_Issues_Owner>();
            }
        }
       
        public static string GetGPNByItemNameType(int ItemNameType, string Org, string Table, DateTime date)
        {
            var script = string.Empty;
            var TestDate = date.AddHours(-8);
            script = Table.ToLower() == "tblcpu" ?
                $@"select top(1) (SUBSTRING(FixtureID1,1,3)+'-'+SUBSTRING(FixtureID1,4,5)+'-'+SUBSTRING(FixtureID1,9,2)) as GPN from tblcpu where ItemNameType ='{ItemNameType}' and tDateTime between '{TestDate.ToString("yyyy-MM-dd")} {TestDate.Hour.ToString()}:00:00' and '{TestDate.ToString("yyyy-MM-dd")} {TestDate.AddHours(1).Hour.ToString()}:00:00'  order by tDateTime desc" :
                $@"select top(1) NOHGPN from tblfinal where ItemNameType ='{ItemNameType}' and tDateTime between '{TestDate.ToString("yyyy-MM-dd")} {TestDate.Hour.ToString()}:00:00' and '{TestDate.ToString("yyyy-MM-dd")} {TestDate.AddHours(1).Hour.ToString()}:00:00'  order by tDateTime desc";

            try
            {
                using (var db = ConnectionFactory.CreatConnection(Org))
                {
                    return db.Query<string>(script).FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return "xxx-xxxxx-xx";
            }
        }

        public static string GetFixtureReleation(InitialParameter InitInfo, int failitem)
        {
            var FixtureInfo = GetGroupDataByScripts("Fixture", InitInfo.Org, InitInfo.ItemNameType, InitInfo.SdateUTC, InitInfo.EdateUTC, failitem) ?? new List<FailCorrelation>();
            if (FixtureInfo.Count == 0)
                return "NO";

            var FixtureTestCountLimit = (FixtureInfo.Sum(item => item.TestCount) / FixtureInfo.Count()) / 2;

            var AvaliableFixture = FixtureInfo.FindAll(item => item.TestCount >= FixtureTestCountLimit).ToList();
            double CorrelationLimitRate = (1.0 / ((double)(AvaliableFixture.Count == 0 ? 1 : AvaliableFixture.Count))) * 2;
            return AvaliableFixture.FindAll(item => (double)item.FailPercent >= CorrelationLimitRate).Count > 0 ? "YES" : "NO";
        }

        public static PTEWEB_Issues_Title GetIssueTitleInfo(string itemnametype, string org)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var Output = db.Query<PTEWEB_Issues_Title>($@"SELECT top(1)*  FROM [PTEDB].[dbo].[PTEWEB_Issues_Title] where ItemNameType ={itemnametype} and Org = '{org}' order by CreateDate desc").First() ?? new PTEWEB_Issues_Title();
                    return Output;
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return null;
            }
        }

        public static List<PTEWEB_Issues_Reply> GetIssueReplayByID(int Issueid, string itemnametype)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var output = db.Query<PTEWEB_Issues_Reply>($@"select * from PTEDB.dbo.PTEWEB_Issues_Content where ItemNameType = {itemnametype} order by CreateTime desc").ToList();

                    foreach (var item in output)
                    {
                        item.IsThisIssue = item.Title_id == Issueid ? true : false;
                    }

                    return TransIDtoRealName(output);
                }
            }
            catch (Exception e)
            {
                return new List<PTEWEB_Issues_Reply>();
            }
        }

        private static List<PTEWEB_Issues_Reply> TransIDtoRealName(List<PTEWEB_Issues_Reply> input)
        {
            try
            {
                var ActionMapData = new List<PTEWEB_Issues_Actions>();
                var CauseMapData = new List<PTEWEB_Issues_ReplyCause>();
                using (var db = ConnectionFactory.CreatConnection())
                {
                    ActionMapData = db.Query<PTEWEB_Issues_Actions>($@"SELECT [Actionid],[Action] from [PTEDB].[dbo].[PTEWEB_Issues_Actions]").ToList();
                    CauseMapData = db.Query<PTEWEB_Issues_ReplyCause>($@"SELECT [Causeid],[Cause]  FROM [PTEDB].[dbo].[PTEWEB_Issues_ReplyCause]").ToList();
                }

                input.ForEach(item =>
                {
                    item.Action = ActionMapData.Find(x => x.Actionid == item.Actionid).Action;
                    item.Cause = CauseMapData.Find(x => x.Causeid == item.Causeid).Cause;
                });
                return input;
            }
            catch (Exception e)
            {
                return input;
            }
        }

        public static PTEWEB_Issues_ByDaily GetIssueDailyInfo(string itemnametype, string org)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var Output = db.Query<PTEWEB_Issues_ByDaily>($@"select top(1)* from [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] where ItemNameType= {itemnametype} and Support_Org = '{org}' order by LastUpdateDate desc").ToList();
                    return Output.First();
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return null;
            }
        }

        public static List<DailyFYRInfo> Get_DailyFYRInfo(int ItemNameType, string Date, string Org, string Source)
        {
            string TargetTestTable = "";
            string JobString = "";
            string JobColumns = "";
            switch (Source)
            {
                case "ATE":
                    TargetTestTable = "TblCPU";
                    JobString = "Attribute2 AS JobNumber ";
                    JobColumns = "Attribute2";
                    break;

                case "FT":
                    TargetTestTable = "TblFinal";
                    JobString = "SO AS JobNumber ";
                    JobColumns = "SO";
                    break;
            }
            DataTable Table = new DataTable();
            DataTable ItemNameTbl = new DataTable();
            DataTable SO_FYR = new DataTable();
            using (var db = ConnectionFactory.CreatConnection(Org))
            {
                string SQL = "SELECT SerialNumber as ESN, FailItem,Result,Spare,tdatetime," + JobString + "FROM " + TargetTestTable + " WHERE ItemNameType='" + ItemNameType + "' and tdatetime>='" + Date + " 00:00:00' and tdatetime<'" + Date + " 23:59:59';";
                Table.Load(db.ExecuteReader(SQL));
                SQL = "Select * FROM[ate_result].[dbo].[ItemName]where ItemNameType = '" + ItemNameType + "'";
                ItemNameTbl.Load(db.ExecuteReader(SQL));
                SQL = $@"SELECT {JobString},(CAST(sum(CAST(result as int)) AS float) /CAST(count(*) AS float)) AS FYR From(
                   SELECT *
                   FROM(
                   SELECT ROW_NUMBER() OVER(PARTITION BY {JobColumns}, serialnumber ORDER BY tDateTime) as RowNum,
                   {JobColumns}, serialnumber, tDateTime, Result
                   FROM {TargetTestTable} WHERE ItemNameType='{ItemNameType}' and tdatetime>='{Date} 00:00:00' and tdatetime<'{Date} 23:59:59') AS T1
                   WHERE RowNum = 1 and Result in (0,1))AS T2 group by {JobColumns}";
                SO_FYR.Load(db.ExecuteReader(SQL));
            }

            var GroupSO = from r in Table.AsEnumerable()
                          group r by r.Field<string>("JobNumber")
                          into g
                          select new
                          {
                              JobNumber = g.Key,
                              FYR = Math.Round(SO_FYR.AsEnumerable().Where(x=>x.Field<string>("JobNumber") == g.Key).First().Field<double>("FYR")*100,1),                                    
                              Spare = Math.Round(g.Select(x => Convert.ToDouble(x.Field<string>("Spare").Trim())).ToList().Average(), 2),
                              Times = g.Select(x => x.Field<DateTime>("tdatetime")).ToList(),
                              Total = g.Count(),
                              Fail_Item = getItemName(ItemNameTbl.Rows[0], g.Where(x => Convert.ToInt32(x.Field<dynamic>("result")) == 0).Select(x => x.Field<string>("FailItem")).ToList())
                          };
            List<DailyFYRInfo> list = new List<DailyFYRInfo>();
            foreach (var r in GroupSO)
            {
                DailyFYRInfo dailyFYRInfo = new DailyFYRInfo();
                dailyFYRInfo.DateTimes = r.Times;
                dailyFYRInfo.FYR = r.FYR;
                dailyFYRInfo.JobNumber = r.JobNumber;
                dailyFYRInfo.Spare = r.Spare;
                dailyFYRInfo.Total = r.Total;
                foreach (var f in r.Fail_Item.GroupBy(x => x).ToList())
                {
                    FailItem failItem = new FailItem();
                    failItem.ItemName = f.Key;
                    failItem.FailCount = f.Count();
                    failItem.Rate = Math.Round(Convert.ToDouble(f.Count() * 100) / r.Total, 2);
                    dailyFYRInfo._FailItemList.Add(failItem);
                }
                dailyFYRInfo._FailItemList = dailyFYRInfo._FailItemList.OrderByDescending(x => x.FailCount).ToList();
                list.Add(dailyFYRInfo);
            }
            return list;
        }

        private static List<string> getItemName(DataRow dr, List<string> Failitem_List)
        {
            List<string> ItemList = new List<string>();
            foreach (string s in Failitem_List)
            {
                ItemList.Add(dr["Name" + s].ToString());
            }
            return ItemList;
        }

        public static List<Models.ReportModel> GetMonthlyIssueReport()
        {
            int nowDay = DateTime.Now.Day;
            DateTime LastStart;
            DateTime LastEnd;
            DateTime NewStart;
            DateTime NewEnd;
            if (nowDay >= 16)
            {
                if (DateTime.Now.Month == 1)
                {
                    LastStart = new DateTime(DateTime.Now.AddYears(-1).Year, DateTime.Now.AddMonths(-1).Month, 16);
                }
                else
                {
                    LastStart = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month, 16);
                }
                LastEnd = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);

                NewStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                NewEnd = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 15);
            }
            else
            {
                if (DateTime.Now.Month == 1)
                {
                    LastStart = new DateTime(DateTime.Now.AddYears(-1).Year, DateTime.Now.AddMonths(-1).Month, 1);
                    LastEnd = new DateTime(DateTime.Now.AddYears(-1).Year, DateTime.Now.AddMonths(-1).Month, 15);
                    NewStart = new DateTime(DateTime.Now.AddYears(-1).Year, DateTime.Now.AddMonths(-1).Month, 16);
                }
                else
                {
                    LastStart = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month, 1);
                    LastEnd = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month, 15);
                    NewStart = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month, 16);
                }
                NewEnd = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);
                // var firstDayCurrentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }
            //SQL one month data
            DataTable ItemTable = new DataTable();
            DataTable MonthlyFYRData = new DataTable();
            DataTable FailItemData = new DataTable();
            DataTable MonthReplyData = new DataTable();
            DataTable IssueTable = new DataTable();
            using (var db = ConnectionFactory.CreatConnection())
            {
                string SQL_ItemNameTYpe = "Select * from [PTEDB].[dbo].[PTEWEB_Issues_History] where DateTime>='" + LastStart.ToString("yyyy-MM-dd 00:00:00") + "' and DateTime<'" + NewEnd.ToString("yyyy-MM-dd 23:59:59") + "' order by DateTime desc";

                string SQL_AllIssue = "Select [ItemNameType],[Support_Org],[Status] FROM [PTEDB].[dbo].[PTEWEB_Issues_ByDaily]";

                string SQL_MonthlyFYRTbl = "Select Date,Org,ItemNameType,Description,Total,Pass,Fail,D_Total,D_Pass,D_Fail FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily] where Date>='" + LastStart.ToString("yyyy-MM-dd 00:00:00") + "' and Date<'" + NewEnd.ToString("yyyy-MM-dd 23:59:59") + "'";

                string SQL_MonthlyFailItem = "Select * FROM[PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily_TOP10_FailItem] where Date>='" + LastStart.ToString("yyyy-MM-dd 00:00:00") + "' and Date<'" + NewEnd.ToString("yyyy-MM-dd 23:59:59") + "'";

                string SQL_MonthlyReply = "Select * FROM [PTEDB].[dbo].[PTEWEB_Issues_Content]  where CreateTime>='" + LastStart.ToString("yyyy-MM-dd 00:00:00") + "' and CreateTime<'" + NewEnd.ToString("yyyy-MM-dd 23:59:59") + "'";

                MonthlyFYRData.Load(db.ExecuteReader(SQL_MonthlyFYRTbl));

                FailItemData.Load(db.ExecuteReader(SQL_MonthlyFailItem));

                ItemTable.Load(db.ExecuteReader(SQL_ItemNameTYpe));

                MonthReplyData.Load(db.ExecuteReader(SQL_MonthlyReply));

                IssueTable.Load(db.ExecuteReader(SQL_AllIssue));
            }
            var ReportGroup = from r in ItemTable.AsEnumerable()
                              group r by new { itemnametype = r.Field<int>("ItemNameType"), org = r.Field<string>("Org").Trim() }
                                          into g
                              select new
                              {
                                  ItemNameType = g.Key.itemnametype,
                                  Status = IssueTable.AsEnumerable().Where(x => x.Field<string>("Support_Org").Trim() == g.Key.org.Trim() &&
                                                                                                         x.Field<int>("ItemNameType") == g.Key.itemnametype).Select(x => x.Field<bool>("Status")).FirstOrDefault(),
                                  org = g.Key.org.Trim(),
                                  Content = g
                              };
            List<Models.ReportModel> LMR = new List<ReportModel>();
            Models.ReportModel MR = new ReportModel();
            foreach (var item in ReportGroup)
            {
                MR = GetSingleReport(item.ItemNameType, MonthlyFYRData, FailItemData, MonthReplyData, LastStart, LastEnd, NewStart, NewEnd, item.Status, item.org);

                if (MR != null)
                {
                    if (Convert.ToInt32(MR.ItemNameType) < 900000)
                    {
                        List<string> UPHList = new List<string>();
                        UPHList = Get_twoweekUPH(LastStart, LastEnd, NewStart, NewEnd, item.ItemNameType, MR.Org);
                        MR.LastUPH = UPHList[0];
                        MR.NextUPH = UPHList[1];
                        MR.LastDateString = "(" + LastStart.ToString("MMdd") + "-" + LastEnd.ToString("MMdd") + ")";
                        MR.CurrentDateString = "(" + NewStart.ToString("MMdd") + "-" + NewEnd.ToString("MMdd") + ")";
                        LMR.Add(MR);
                    }
                }
            }
            return LMR;
        }

        public static List<string> Get_twoweekUPH(DateTime lst, DateTime let, DateTime nst, DateTime net, int itemnametype, string org)
        {
            List<string> UPHList = new List<string>();
            UPHList.Add("0.0%");
            UPHList.Add("0.0%");
            string Lsql = $@"SELECT* FROM[PTEDB].[dbo].[PTEWEB_ItemNameType_RealOutput_ByDaily] where UPH!=999 and date between '" + lst.ToString("yyyy-MM-dd") + "'and '" + let.ToString("yyyy-MM-dd") + "' and org='" + org + "' and itemnametype='" + itemnametype + "' ";
            string Nsql = $@"SELECT* FROM[PTEDB].[dbo].[PTEWEB_ItemNameType_RealOutput_ByDaily] where UPH!=999 and date between '" + nst.ToString("yyyy-MM-dd") + "'and '" + net.ToString("yyyy-MM-dd") + "' and org='" + org + "' and itemnametype='" + itemnametype + "'";
            DataTable Ldt = new DataTable();
            DataTable Ndt = new DataTable();
            using (var db = ConnectionFactory.CreatConnection())
            {
                Ldt.Load(db.ExecuteReader(Lsql));

                Ndt.Load(db.ExecuteReader(Nsql));
            }
            if (Ldt.Rows.Count > 0)
            {
                UPHList[0] = Math.Round(Ldt.AsEnumerable().Average(x => x.Field<double>("EstimateUPH") / x.Field<int>("UPH")) * 100, 3).ToString() + "%";
            }
            if (Ndt.Rows.Count > 0)
            {
                UPHList[1] = Math.Round(Ndt.AsEnumerable().Average(x => x.Field<double>("EstimateUPH") / x.Field<int>("UPH")) * 100, 3).ToString() + "%";
            }
            return UPHList;
        }

        public static List<Models.ReportModel> GetMonthlyIssueReport_NonITM()
        {
            int nowDay = DateTime.Now.Day;
            DateTime LastStart;
            DateTime LastEnd;
            DateTime NewStart;
            DateTime NewEnd;
            if (nowDay >= 16)
            {
                if (DateTime.Now.Month == 1)
                {
                    LastStart = new DateTime(DateTime.Now.AddYears(-1).Year, DateTime.Now.AddMonths(-1).Month, 16);
                }
                else
                {
                    LastStart = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month, 16);
                }
                LastEnd = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);
                NewStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                NewEnd = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 15);
            }
            else
            {
                if (DateTime.Now.Month == 1)
                {
                    LastStart = new DateTime(DateTime.Now.AddYears(-1).Year, DateTime.Now.AddMonths(-1).Month, 1);
                    LastEnd = new DateTime(DateTime.Now.AddYears(-1).Year, DateTime.Now.AddMonths(-1).Month, 15);
                    NewStart = new DateTime(DateTime.Now.AddYears(-1).Year, DateTime.Now.AddMonths(-1).Month, 16);
                }
                else
                {
                    LastStart = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month, 1);
                    LastEnd = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month, 15);
                    NewStart = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month, 16);
                }
                NewEnd = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);
            }
            //SQL one month data
            DataTable ItemTable = new DataTable();
            DataTable MonthlyFYRData = new DataTable();
            DataTable FailItemData = new DataTable();
            DataTable MonthReplyData = new DataTable();
            DataTable IssueTable = new DataTable();
            using (var db = ConnectionFactory.CreatConnection())
            {
                string SQL_ItemNameTYpe = "Select * from [PTEDB].[dbo].[PTEWEB_Issues_History] where DateTime>='" + LastStart.ToString("yyyy-MM-dd 00:00:00") + "' and DateTime<'" + NewEnd.ToString("yyyy-MM-dd 23:59:59") + "' and ItemNameType>'900000' order by DateTime desc";

                string SQL_AllIssue = "Select [ItemNameType],[Support_Org],[Status] FROM [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] where ItemNameType>'900000'";

                string SQL_MonthlyFYRTbl = "Select Date,Org,ItemNameType,Description,Total,Pass,Fail,D_Total,D_Pass,D_Fail FROM [PTEDB].[dbo].[PTEWEB_nonITMXP_ByDaily] where Date>='" + LastStart.ToString("yyyy-MM-dd 00:00:00") + "' and Date<'" + NewEnd.ToString("yyyy-MM-dd 23:59:59") + "' and ItemNameType>'900000'";
                string SQL_MonthlyReply = "Select * FROM [PTEDB].[dbo].[PTEWEB_Issues_Content]  where CreateTime>='" + LastStart.ToString("yyyy-MM-dd 00:00:00") + "' and CreateTime<'" + NewEnd.ToString("yyyy-MM-dd 23:59:59") + "'";
                MonthReplyData.Load(db.ExecuteReader(SQL_MonthlyReply));
                MonthlyFYRData.Load(db.ExecuteReader(SQL_MonthlyFYRTbl));

                ItemTable.Load(db.ExecuteReader(SQL_ItemNameTYpe));

                IssueTable.Load(db.ExecuteReader(SQL_AllIssue));
            }
            var ReportGroup = from r in ItemTable.AsEnumerable()
                              group r by new { itemnametype = r.Field<int>("ItemNameType"), org = r.Field<string>("Org").Trim() }
                              into g
                              select new
                              {
                                  ItemNameType = g.Key.itemnametype,
                                  Status = IssueTable.AsEnumerable().Where(x => x.Field<string>("Support_Org").Trim() == g.Key.org.Trim() &&
                                                                                                         x.Field<int>("ItemNameType") == g.Key.itemnametype).Select(x => x.Field<bool>("Status")).FirstOrDefault(),
                                  org = g.Key.org.Trim(),
                                  Content = g
                              };
            List<Models.ReportModel> LMR = new List<ReportModel>();
            Models.ReportModel MR = new ReportModel();
            foreach (var item in ReportGroup)
            {
                MR = GetSingleReport(item.ItemNameType, MonthlyFYRData, FailItemData, MonthReplyData, LastStart, LastEnd, NewStart, NewEnd, item.Status, item.org);

                if (MR != null)
                {
                    if (Convert.ToInt32(MR.ItemNameType) > 900000)
                    {
                        List<string> UPHList = new List<string>();
                        UPHList = Get_twoweekUPH(LastStart, LastEnd, NewStart, NewEnd, item.ItemNameType, MR.Org);
                        MR.LastUPH = UPHList[0];
                        MR.NextUPH = UPHList[1];
                        MR.LastDateString = "(" + LastStart.ToString("MMdd") + "-" + LastEnd.ToString("MMdd") + ")";
                        MR.CurrentDateString = "(" + NewStart.ToString("MMdd") + "-" + NewEnd.ToString("MMdd") + ")";
                        LMR.Add(MR);
                    }
                }
            }
            return LMR;
        }

        public static Models.ReportModel GetSingleReport(int ItemNameType, DataTable report, DataTable failitem, DataTable replyData, DateTime ost, DateTime oet, DateTime nst, DateTime net, bool status, string org)
        {
            Models.ReportModel MR = new ReportModel();
            MR.LinkImg = status ? "open" : "close";
            MR.ItemNameType = ItemNameType.ToString();

            var LastCount = (from r in report.AsEnumerable()
                             where r.Field<DateTime>("Date") >= ost &&
                             r.Field<DateTime>("Date") < oet.AddDays(1)
                             && r.Field<int>("ItemNameType") == ItemNameType
                             select r).Count();

            DataTable LastReport;
            DataTable LastFailItemTbl = new DataTable();
            int LastTotal = 0;
            int CurrentTotal = 0;
            if (LastCount > 0)
            {
                LastReport = (from r in report.AsEnumerable()
                              where r.Field<DateTime>("Date") >= ost &&
                              r.Field<DateTime>("Date") < oet.AddDays(1)
                              && r.Field<int>("ItemNameType") == ItemNameType
                              select r).CopyToDataTable();
                LastTotal = LastReport.AsEnumerable().Select(x => x.Field<int>("Total")).Sum();
                LastFailItemTbl = failitem.AsEnumerable().Where(x => x.Field<int>("ItemNameType") == ItemNameType && x.Field<DateTime>("Date") >= ost && x.Field<DateTime>("Date") < oet.AddDays(1)).Count() > 0 ? failitem.AsEnumerable().Where(x => x.Field<int>("ItemNameType") == ItemNameType && x.Field<DateTime>("Date") >= ost && x.Field<DateTime>("Date") < oet.AddDays(1)).CopyToDataTable() : null;
                MR.FYR_Old = Math.Round(100 - (double)LastReport.AsEnumerable().Select(x => x.Field<int>("D_Fail")).Sum() * 100 / LastReport.AsEnumerable().Select(x => x.Field<int>("D_Total")).Sum(), 2) + "%";
            }
            var CurrentCount = (from r in report.AsEnumerable()
                                where r.Field<DateTime>("Date") >= nst &&
                                r.Field<DateTime>("Date") < net.AddDays(1)
                                && r.Field<int>("ItemNameType") == ItemNameType
                                select r).Count();
            DataTable CurrentReport;
            DataTable CurrentFailItemTbl = new DataTable();
            if (CurrentCount > 0)
            {
                CurrentReport = (from r in report.AsEnumerable()
                                 where r.Field<DateTime>("Date") >= nst &&
                                 r.Field<DateTime>("Date") < net.AddDays(1)
                                 && r.Field<int>("ItemNameType") == ItemNameType
                                 select r).CopyToDataTable();
                CurrentTotal = CurrentReport.AsEnumerable().Select(x => x.Field<int>("Total")).Sum();
                CurrentFailItemTbl = failitem.AsEnumerable().Where(x => x.Field<int>("ItemNameType") == ItemNameType && x.Field<DateTime>("Date") >= nst && x.Field<DateTime>("Date") < net.AddDays(1)).Count() > 0 ? failitem.AsEnumerable().Where(x => x.Field<int>("ItemNameType") == ItemNameType && x.Field<DateTime>("Date") >= nst && x.Field<DateTime>("Date") < net.AddDays(1)).CopyToDataTable() : new DataTable();
                MR.FYR_New = Math.Round(100 - (double)CurrentReport.AsEnumerable().Select(x => x.Field<int>("D_Fail")).Sum() * 100 / CurrentReport.AsEnumerable().Select(x => x.Field<int>("D_Total")).Sum(), 2) + "%";
            }
            if (LastCount > 0 || CurrentCount > 0) //過濾掉舊有的結案issue
            {
                MR.Description = (from r in report.AsEnumerable()
                                  where r.Field<int>("ItemNameType") == ItemNameType
                                  select r.Field<string>("Description")).ToList()[0];
                MR.Org = (from r in report.AsEnumerable()
                          orderby r.Field<DateTime>("Date") descending
                          where r.Field<int>("ItemNameType") == ItemNameType
                          select r.Field<string>("Org")).ToList()[0];
                var failitemTbl = failitem.AsEnumerable().Where(x => x.Field<int>("ItemNameType") == ItemNameType).Count() == 0 ? null : failitem.AsEnumerable().Where(x => x.Field<int>("ItemNameType") == ItemNameType).CopyToDataTable();
                if (failitemTbl != null)
                {
                    int TotalFailCount = 0;
                    Dictionary<string, Int32> FailItems = new Dictionary<string, Int32>();
                    foreach (DataRow dr in failitemTbl.AsEnumerable())
                    {
                        TotalFailCount += Convert.ToInt32(dr["Total_Fail_Count"]);

                        for (int i = 1; i < 11; i++)
                        {
                            if (dr["No" + i + "_Fail_Item"] == DBNull.Value)
                            {
                                continue;
                            }
                            else
                            {
                                if (!FailItems.ContainsKey(dr["No" + i + "_Fail_Item"].ToString()))
                                {
                                    FailItems.Add(dr["No" + i + "_Fail_Item"].ToString(), Convert.ToInt32(dr["No" + i + "_Fail_Count"]));
                                }
                                else
                                {
                                    FailItems[dr["No" + i + "_Fail_Item"].ToString()] += Convert.ToInt32(dr["No" + i + "_Fail_Count"]);
                                }
                            }
                        }
                    }
                    var Top3FailItems = (from r in FailItems
                                         orderby r.Value descending
                                         select r).Take(3);
                    int colorcount = 0;
                    foreach (var s in Top3FailItems)
                    {
                        string Failitem = s.Key + "(" + Math.Round((double)s.Value * 100 / TotalFailCount, 2) + "%)";
                        switch (colorcount)
                        {
                            case 0:
                                MR.F1 = Failitem;
                                break;

                            case 1:
                                MR.F2 = Failitem;
                                break;

                            case 2:
                                MR.F3 = Failitem;
                                break;
                        }
                        //if (colorcount % 2 == 0)
                        //{
                        //    MR.Top3_Fail_Item += "<label style=\"background-color: #FF69B4;font-size:16px;color:black;\">" + s.Key + "(" + Math.Round((double)s.Value * 100 / TotalFailCount, 2) + "%)</label><br/>";
                        //}
                        //else
                        //{
                        //    MR.Top3_Fail_Item += "<label style=\"background-color:#F0E68C;font-size:16px;;color:black;\">" + s.Key + "(" + Math.Round((double)s.Value * 100 / TotalFailCount, 2) + "%)</label><br/>";
                        //}
                        colorcount++;
                    }
                    int ItemReplyCount = (from r in replyData.AsEnumerable()
                                          where r.Field<int>("ItemNameType") == ItemNameType && r.Field<int>("Causeid") != 0
                                          select r).Count();
                    if (ItemReplyCount > 0)
                    {
                        var ReplyItems = (from r in replyData.AsEnumerable()
                                          where r.Field<int>("ItemNameType") == ItemNameType && r.Field<int>("Causeid") != 0
                                          group r by r.Field<string>("FailItem") into g
                                          select new
                                          {
                                              failitem = g.Key,
                                              data = g
                                          }
                                         );
                        int replyColor = 0;
                        foreach (var replyitem in ReplyItems)
                        {
                            int LastItemCount = 0;

                            int CurrentItemCount = 0;
                            for (int j = 0; j < LastFailItemTbl.Rows.Count; j++)
                            {
                                for (int k = 1; k < 11; k++)
                                {
                                    if (LastFailItemTbl.Rows[j]["No" + k + "_Fail_Item"] == DBNull.Value)
                                    {
                                        continue;
                                    }
                                    if (LastFailItemTbl.Rows[j]["No" + k + "_Fail_Item"].ToString() == replyitem.failitem)
                                    {
                                        LastItemCount += Convert.ToInt32(LastFailItemTbl.Rows[j]["No" + k + "_Fail_Count"]);
                                    }
                                }
                            }
                            for (int j = 0; j < CurrentFailItemTbl.Rows.Count; j++)
                            {
                                for (int k = 1; k < 11; k++)
                                {
                                    if (CurrentFailItemTbl.Rows[j]["No" + k + "_Fail_Item"] == DBNull.Value)
                                    {
                                        continue;
                                    }
                                    if (CurrentFailItemTbl.Rows[j]["No" + k + "_Fail_Item"].ToString() == replyitem.failitem)
                                    {
                                        CurrentItemCount += Convert.ToInt32(CurrentFailItemTbl.Rows[j]["No" + k + "_Fail_Count"]);
                                    }
                                }
                            }
                            string rf = replyitem.failitem;
                            string fro = Math.Round((double)LastItemCount * 100 / LastTotal, 2) + "%";
                            string frn = Math.Round((double)CurrentItemCount * 100 / CurrentTotal, 2) + "%";
                            switch (replyColor)
                            {
                                case 0:
                                    MR.Fail_Item_Action1 = rf;
                                    MR.Fail_Rate_Old1 = fro;
                                    MR.Fail_Rate_New1 = frn;
                                    break;

                                case 1:
                                    MR.Fail_Item_Action2 = rf;
                                    MR.Fail_Rate_Old2 = fro;
                                    MR.Fail_Rate_New2 = frn;
                                    break;

                                case 2:
                                    MR.Fail_Item_Action3 = rf;
                                    MR.Fail_Rate_Old3 = fro;
                                    MR.Fail_Rate_New3 = frn;
                                    break;
                            }

                            replyColor++;
                        }
                    }
                }
                return MR;
            }
            else
            {
                return null;
            }
        }

        private static Dictionary<int, string> GetActionDict()
        {
            try
            {
                var Output = new Dictionary<int, string>();
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var TempList = db.Query<PTEWEB_Issues_Actions>($@"SELECT *  FROM [PTEDB].[dbo].[PTEWEB_Issues_Actions]").ToList();
                    TempList.ForEach(item =>
                    {
                        Output[item.Actionid] = item.Action;
                    });
                    return Output;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private static Dictionary<int, string> GetCauseDict()
        {
            try
            {
                var Output = new Dictionary<int, string>();

                using (var db = ConnectionFactory.CreatConnection())

                {
                    var TempList = db.Query<PTEWEB_Issues_ReplyCause>($@"SELECT *  FROM [PTEDB].[dbo].[PTEWEB_Issues_ReplyCause]").ToList();
                    TempList.ForEach(item =>
                    {
                        Output[item.Causeid] = item.Cause;
                    });
                    return Output;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static Dictionary<string, DateTime> GetIssueReportDateDict()
        {
            var DateDict = new Dictionary<string, DateTime>();
            int nowDay = DateTime.Now.Day;
            int nowYear = DateTime.Now.Year;
            int nowMonth = DateTime.Now.Month;

            var FinalReport = new List<ExportReportModel>();
            DateTime LastStart;
            DateTime LastEnd;
            DateTime NewStart;
            DateTime NewEnd;
            if (nowDay >= 16)
            {
                NewStart = new DateTime(nowYear, nowMonth, 1);
                NewEnd = new DateTime(nowYear, nowMonth, 15);
                var firstDayCurrentMonth = new DateTime(nowYear, nowMonth, 1);
                LastStart = new DateTime(firstDayCurrentMonth.AddDays(-1).Year, firstDayCurrentMonth.AddDays(-1).Month, 16).AddDays(0);
                LastEnd = firstDayCurrentMonth.AddDays(-1);
            }
            else
            {
                var firstDayCurrentMonth = new DateTime(nowYear, nowMonth, 1);
                LastStart = new DateTime(firstDayCurrentMonth.AddDays(-1).Year, firstDayCurrentMonth.AddDays(-1).Month, 1).AddDays(0);
                LastEnd = new DateTime(firstDayCurrentMonth.AddDays(-1).Year, firstDayCurrentMonth.AddDays(-1).Month, 15);
                NewStart = new DateTime(firstDayCurrentMonth.AddDays(-1).Year, firstDayCurrentMonth.AddDays(-1).Month, 16);
                NewEnd = firstDayCurrentMonth.AddDays(-1);
            }

            DateDict["LastStart"] = LastStart;
            DateDict["LastEnd"] = LastEnd;
            DateDict["NewStart"] = NewStart;
            DateDict["NewEnd"] = NewEnd;

            return DateDict;
        }

        public static List<ExportReportModel> GetIssueExportData_NonITMXP()
        {
            try
            {
                var DateDict = GetIssueReportDateDict();
                var LastStart = DateDict["LastStart"];
                var NewEnd = DateDict["NewEnd"];
                var NewStart = DateDict["NewStart"];
                var LastEnd = DateDict["LastEnd"];

                var FinalReport = new List<ExportReportModel>();
                var ActionDict = GetActionDict();
                var CauseDict = GetCauseDict();

                var IssueList = new List<PTEWEB_Issues_Title>();
                var MonthlyFYRData = new List<PTEWEB_ItemNameType_ByDaily>();
                var MonthlyUPHTable = new List<PTEWEB_ItemNameType_RealOutput_ByDaily>();
                var MonthReplyData = new List<PTEWEB_Issues_Reply>();

                using (var db = ConnectionFactory.CreatConnection())
                {
                    var SQL_IssueList = $@"SELECT *  FROM [PTEDB].[dbo].[PTEWEB_Issues_Title] where CreateDate between '{LastStart.ToString("yyyy-MM-dd 00:00:00")}' and '{NewEnd.ToString("yyyy-MM-dd 23:59:59")}' and ItemNameType > 99000";
                    var SQL_FYRList = $@"SELECT *  FROM [PTEDB].[dbo].[PTEWEB_nonITMXP_ByDaily] where date between '{LastStart.ToString("yyyy-MM-dd 00:00:00")}' and '{NewEnd.ToString("yyyy-MM-dd 23:59:59")}'";
                    var SQL_UPHList = $@"SELECT *  FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_RealOutput_ByDaily] where Date between '{LastStart.ToString("yyyy-MM-dd 00:00:00")}' and '{NewEnd.ToString("yyyy-MM-dd 23:59:59")}'";
                    var SQL_MonthlyReply = "Select * FROM [PTEDB].[dbo].[PTEWEB_Issues_Content]";

                    IssueList = db.Query<PTEWEB_Issues_Title>(SQL_IssueList).ToList();
                    MonthlyFYRData = db.Query<PTEWEB_ItemNameType_ByDaily>(SQL_FYRList).ToList();
                    MonthlyUPHTable = db.Query<PTEWEB_ItemNameType_RealOutput_ByDaily>(SQL_UPHList).ToList();
                    MonthReplyData = db.Query<PTEWEB_Issues_Reply>(SQL_MonthlyReply).ToList();
                }

                foreach (var item in IssueList)
                {
                    var reportRow = new ExportReportModel();

                    var SupportOrg = item.Org.Trim();
                    var NewDateRangeData_FYR = MonthlyFYRData.FindAll(x => x.ItemNameType == item.ItemNameType && x.Org == SupportOrg && x.Date >= NewStart && x.Date <= NewEnd);
                    var OldDateRangeData_FYR = MonthlyFYRData.FindAll(x => x.ItemNameType == item.ItemNameType && x.Org == SupportOrg && x.Date >= LastStart && x.Date <= LastEnd);
                    var TotalDateRangeData_FYR = MonthlyFYRData.FindAll(x => x.ItemNameType == item.ItemNameType && x.Org == SupportOrg);

                    var CAPA_UPH = MonthlyUPHTable.FindAll(x => x.UPH != 999 && x.ItemNameType == item.ItemNameType).Count != 0 ? MonthlyUPHTable.FindAll(x => x.UPH != 999 && x.ItemNameType == item.ItemNameType).Select(x => x.UPH).Max() : 999;
                    var NewDateRangeData_UPH = MonthlyUPHTable.FindAll(x => x.ItemNameType == item.ItemNameType && x.Org.Trim() == SupportOrg && x.Date >= NewStart && x.Date <= NewEnd);
                    var OldDateRangeData_UPH = MonthlyUPHTable.FindAll(x => x.ItemNameType == item.ItemNameType && x.Org.Trim() == SupportOrg && x.Date >= LastStart && x.Date <= LastEnd);
                    var TotalDateRangeData_UPH = MonthlyUPHTable.FindAll(x => x.ItemNameType == item.ItemNameType && x.Org.Trim() == SupportOrg);
                    reportRow.ItemNameType = item.ItemNameType.ToString();
                    reportRow.Org = SupportOrg;
                    reportRow.Description = TotalDateRangeData_FYR.Count != 0 ? TotalDateRangeData_FYR.First().Description : "Unknown";
                    reportRow.First_Yield_Rate_New = NewDateRangeData_FYR.Count != 0 ? NewDateRangeData_FYR.Select(y => y.FYR).ToList().Average().ToString("F2") : "NaN";
                    reportRow.First_Yield_Rate_Old = OldDateRangeData_FYR.Count != 0 ? OldDateRangeData_FYR.Select(y => y.FYR).ToList().Average().ToString("F2") : "NaN";
                    var TempUPH_New = NewDateRangeData_UPH.Count != 0 ? NewDateRangeData_UPH.Select(y => y.EstimateUPH).ToList().Average().ToString("F2") : "NaN";
                    var TempUPH_Old = OldDateRangeData_UPH.Count != 0 ? OldDateRangeData_UPH.Select(y => y.EstimateUPH).ToList().Average().ToString("F2") : "NaN";

                    reportRow.UPH_Achievement_Rate_New = TempUPH_New != "NaN" ? ((float.Parse(TempUPH_New) / CAPA_UPH) * 100).ToString("F2") : TempUPH_New;
                    reportRow.UPH_Achievement_Rate_Old = TempUPH_Old != "NaN" ? ((float.Parse(TempUPH_Old) / CAPA_UPH) * 100).ToString("F2") : TempUPH_Old;
                    reportRow.CurrentDateString = NewStart.ToString("yyyy-MM-dd") + "~" + NewEnd.ToString("yyyy-MM-dd");
                    reportRow.LastDateString = LastStart.ToString("yyyy-MM-dd") + "~" + LastEnd.ToString("yyyy-MM-dd");
                    var ItemActionList = MonthReplyData.FindAll(x => x.Title_id == item.Title_id && x.ItemNameType == item.ItemNameType) ?? new List<PTEWEB_Issues_Reply>();
                    if (ItemActionList.Count() != 0)
                    {
                        var AttachmentRootPath = ItemActionList.First().fileName;
                        AttachmentRootPath = AttachmentRootPath != "" ? AttachmentRootPath.Replace(AttachmentRootPath.Split('\\').Last(), string.Empty) : AttachmentRootPath;
                        reportRow.AttachmentLink = AttachmentRootPath;

                        foreach (var reply in ItemActionList)
                        {
                            FinalReport.Add(new ExportReportModel()
                            {
                                Editor = reply.UserName,
                                Owner = reply.Owner,
                                ItemNameType = item.ItemNameType.ToString(),
                                Description = reportRow.Description,
                                First_Yield_Rate_New = reportRow.First_Yield_Rate_New,
                                First_Yield_Rate_Old = reportRow.First_Yield_Rate_Old,
                                UPH_Achievement_Rate_New = reportRow.UPH_Achievement_Rate_New,
                                UPH_Achievement_Rate_Old = reportRow.UPH_Achievement_Rate_Old,
                                Org = reportRow.Org,
                                Cause_Type = reply.Causeid != 0 ? CauseDict[reply.Causeid] : "結案",
                                Cause_Comment = reply.CauseCommon,
                                Action_Type = ActionDict[reply.Actionid],
                                Action_Comment = reply.ActionCommon,
                                AttachmentLink = reportRow.AttachmentLink,
                                CurrentDateString = reportRow.CurrentDateString,
                                LastDateString = reportRow.LastDateString
                            });
                        }
                    }
                    else
                    {
                        FinalReport.Add(reportRow);
                    }
                }
                FinalReport = FinalReport.OrderBy(x => x.Org).ToList();
                return ProcessFinalExportIssueTable(FinalReport);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static List<ExportReportModel> GetIssueExportData()
        {
            try
            {
                var DateDict = GetIssueReportDateDict();
                var LastStart = DateDict["LastStart"];
                var NewEnd = DateDict["NewEnd"];
                var NewStart = DateDict["NewStart"];
                var LastEnd = DateDict["LastEnd"];

                var FinalReport = new List<ExportReportModel>();

                var ActionDict = GetActionDict();
                var CauseDict = GetCauseDict();
                var ItemTable = new List<int>();
                var MonthlyFYRData = new List<PTEWEB_ItemNameType_ByDaily>();
                var MonthlyUPHTable = new List<PTEWEB_ItemNameType_RealOutput_ByDaily>();
                var FailItemData = new List<PTEWEB_ItemNameType_ByDaily_TOP10_FailItem>();
                var MonthReplyData = new List<PTEWEB_Issues_Reply>();
                var CreatIssueList = new List<PTEWEB_Issues_Title>();
                using (var db = ConnectionFactory.CreatConnection())
                {
                    string SQL_ItemNameTYpe = "Select ItemNameType from [PTEDB].[dbo].[PTEWEB_Issues_History] where DateTime>='" + LastStart.ToString("yyyy-MM-dd 00:00:00") + "' and DateTime<'" + NewEnd.ToString("yyyy-MM-dd 23:59:59") + "'";

                    string SQL_MonthlyFYRTbl = "Select Date,Org,ItemNameType,Description,Total,Pass,Fail,D_Total,D_Pass,D_Fail,FYR, Fail_Rate FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily] where Date>='" + LastStart.AddDays(-180).ToString("yyyy-MM-dd 00:00:00") + "' and Date<'" + NewEnd.ToString("yyyy-MM-dd 23:59:59") + "'";

                    string SQL_MonthyUPHTbl = $@"SELECT *  FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_RealOutput_ByDaily] where Date between '{LastStart.ToString("yyyy-MM-dd 00:00:00")}' and '{NewEnd.ToString("yyyy-MM-dd 23:59:59")}'";

                    string SQL_MonthlyFailItem = "Select * FROM[PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily_TOP10_FailItem] where Date>='" + LastStart.ToString("yyyy-MM-dd 00:00:00") + "' and Date<'" + NewEnd.ToString("yyyy-MM-dd 23:59:59") + "'";

                    string SQL_MonthlyReply = "Select * FROM [PTEDB].[dbo].[PTEWEB_Issues_Content]";

                    //string SQL_MonthlyReply = "Select * FROM [PTEDB].[dbo].[PTEWEB_Issues_Content]  where CreateTime>='" + LastStart.ToString("yyyy-MM-dd 00:00:00") + "' and CreateTime<'" + NewEnd.ToString("yyyy-MM-dd 23:59:59") + "'";

                    string SQL_IssueTitle = "Select * FROM [PTEDB].[dbo].[PTEWEB_Issues_Title] where Issue_Status = 1 ";

                    //string SQL_IssueTitle = "Select * FROM [PTEDB].[dbo].[PTEWEB_Issues_Title]  where CreateDate>='" + LastStart.ToString("yyyy-MM-dd 00:00:00") + "' and CreateDate<'" + NewEnd.ToString("yyyy-MM-dd 23:59:59") + "'";

                    MonthlyFYRData = db.Query<PTEWEB_ItemNameType_ByDaily>(SQL_MonthlyFYRTbl).ToList();

                    MonthlyUPHTable = db.Query<PTEWEB_ItemNameType_RealOutput_ByDaily>(SQL_MonthyUPHTbl).ToList();

                    FailItemData = db.Query<PTEWEB_ItemNameType_ByDaily_TOP10_FailItem>(SQL_MonthlyFailItem).ToList();

                    ItemTable = db.Query<int>(SQL_ItemNameTYpe).ToList().Distinct().ToList();

                    MonthReplyData = db.Query<PTEWEB_Issues_Reply>(SQL_MonthlyReply).ToList();

                    CreatIssueList = db.Query<PTEWEB_Issues_Title>(SQL_IssueTitle).ToList();
                }

                foreach (var item in CreatIssueList)
                {
                    var reportRow = new ExportReportModel();

                    try
                    {
                        var SupportOrg = item.Org;
                        var NewDateRangeData_FYR = MonthlyFYRData.FindAll(x => x.ItemNameType == item.ItemNameType && x.Org == SupportOrg && x.Date >= NewStart && x.Date <= NewEnd);
                        var OldDateRangeData_FYR = MonthlyFYRData.FindAll(x => x.ItemNameType == item.ItemNameType && x.Org == SupportOrg && x.Date >= LastStart && x.Date <= LastEnd);
                        var TotalDateRangeData_FYR = MonthlyFYRData.FindAll(x => x.ItemNameType == item.ItemNameType && x.Org == SupportOrg);

                        var CAPA_UPH = MonthlyUPHTable.FindAll(x => x.UPH != 999 && x.ItemNameType == item.ItemNameType).Count != 0 ? MonthlyUPHTable.FindAll(x => x.UPH != 999 && x.ItemNameType == item.ItemNameType).Select(x => x.UPH).Max() : 999;
                        var NewDateRangeData_UPH = MonthlyUPHTable.FindAll(x => x.ItemNameType == item.ItemNameType && x.Org.Trim() == SupportOrg && x.Date >= NewStart && x.Date <= NewEnd);
                        var OldDateRangeData_UPH = MonthlyUPHTable.FindAll(x => x.ItemNameType == item.ItemNameType && x.Org.Trim() == SupportOrg && x.Date >= LastStart && x.Date <= LastEnd);
                        var TotalDateRangeData_UPH = MonthlyUPHTable.FindAll(x => x.ItemNameType == item.ItemNameType && x.Org.Trim() == SupportOrg);

                        reportRow.ItemNameType = item.ItemNameType.ToString();
                        reportRow.Description = TotalDateRangeData_FYR.Count != 0 ? TotalDateRangeData_FYR.First().Description : "Unknown";

                        if (item.ItemNameType == 7641)
                            reportRow.ItemNameType = item.ItemNameType.ToString();

                        reportRow.First_Yield_Rate_New = NewDateRangeData_FYR.Count != 0 ? NewDateRangeData_FYR.Select(y => y.FYR).ToList().Average().ToString("F2") : "NaN";
                        reportRow.First_Yield_Rate_Old = OldDateRangeData_FYR.Count != 0 ? OldDateRangeData_FYR.Select(y => y.FYR).ToList().Average().ToString("F2") : "NaN";
                        reportRow.Fail_Rate_New = NewDateRangeData_FYR.Count != 0 ? NewDateRangeData_FYR.Select(y => y.Fail_Rate).ToList().Average().ToString("F2") : "NaN";
                        reportRow.Fail_Rate_Old = OldDateRangeData_FYR.Count != 0 ? OldDateRangeData_FYR.Select(y => y.Fail_Rate).ToList().Average().ToString("F2") : "NaN";
                        var TempUPH_New = NewDateRangeData_UPH.Count != 0 ? NewDateRangeData_UPH.Select(y => y.EstimateUPH).ToList().Average().ToString("F2") : "NaN";
                        var TempUPH_Old = OldDateRangeData_UPH.Count != 0 ? OldDateRangeData_UPH.Select(y => y.EstimateUPH).ToList().Average().ToString("F2") : "NaN";

                        reportRow.UPH_Achievement_Rate_New = TempUPH_New != "NaN" ? ((float.Parse(TempUPH_New) / CAPA_UPH) * 100).ToString("F2") : TempUPH_New;
                        reportRow.UPH_Achievement_Rate_Old = TempUPH_Old != "NaN" ? ((float.Parse(TempUPH_Old) / CAPA_UPH) * 100).ToString("F2") : TempUPH_Old;

                        reportRow.Org = SupportOrg;
                        var NewRangeFailItmeList = FailItemData.FindAll(x => x.ItemNameType == item.ItemNameType && x.Date <= NewEnd && x.Date >= NewStart && x.Org == SupportOrg).ToList();
                        var PreviousRangeFailItmeList = FailItemData.FindAll(x => x.ItemNameType == item.ItemNameType && x.Date <= LastEnd && x.Date >= LastStart && x.Org == SupportOrg).ToList();
                        var NewTotal = (MonthlyFYRData.FindAll(x => x.ItemNameType == item.ItemNameType && x.Org == SupportOrg && x.Date >= NewStart && x.Date <= NewEnd)).Select(y => y.Total).ToList().Sum();
                        var OldTotal = (MonthlyFYRData.FindAll(x => x.ItemNameType == item.ItemNameType && x.Org == SupportOrg && x.Date >= LastStart && x.Date <= LastEnd)).Select(y => y.Total).ToList().Sum();

                        var TotalFailCount = NewRangeFailItmeList.Select(x => x.Total_Fail_Count).Sum();

                        var ItemList = DeltaIModel.TopFailTableTransferToFailInfoList(NewRangeFailItmeList);
                        var DistinctItemList = ItemList.Select(x => x.FailItemName).ToList().Distinct().ToList();
                        var FailItemList = new List<TopFailInfo>();

                        DistinctItemList.ForEach(ItemName =>
                        {
                            var ItemCount = 0;
                            foreach (var ItemInfo in ItemList)
                            {
                                if (ItemInfo.FailItemName == ItemName)
                                {
                                    ItemCount += ItemInfo.FaiCount;
                                }
                            }
                            var FailRate = ((double)ItemCount / (double)NewTotal) * 100;
                            FailItemList.Add(new TopFailInfo { FailItemName = ItemName + $@"({FailRate.ToString("F2")} %)", FaiCount = ItemCount });
                        });
                        FailItemList = FailItemList.OrderBy(x => x.FaiCount).Reverse().ToList();
                        reportRow.Top_Fail_Item = FailItemList.Count >= 3 ? FailItemList.Take(3).ToList()[0].FailItemName + Environment.NewLine + FailItemList.Take(3).ToList()[1].FailItemName + Environment.NewLine + FailItemList.Take(3).ToList()[2].FailItemName : "NaN";
                        reportRow.CurrentDateString = NewStart.ToString("yyyy-MM-dd") + "~" + NewEnd.ToString("yyyy-MM-dd");
                        reportRow.LastDateString = LastStart.ToString("yyyy-MM-dd") + "~" + LastEnd.ToString("yyyy-MM-dd");

                        var ItemActionList = MonthReplyData.FindAll(x => x.Title_id == item.Title_id && x.ItemNameType == item.ItemNameType) ?? new List<PTEWEB_Issues_Reply>();

                        if (ItemActionList.Count() != 0)
                        {
                            var AttachmentRootPath = ItemActionList.First().fileName;
                            AttachmentRootPath = AttachmentRootPath != "" ? AttachmentRootPath.Replace(AttachmentRootPath.Split('\\').Last(), string.Empty) : AttachmentRootPath;
                            reportRow.AttachmentLink = AttachmentRootPath;

                            foreach (var reply in ItemActionList)
                            {
                                FinalReport.Add(new ExportReportModel()
                                {
                                    Editor = reply.UserName,
                                    Owner = reply.Owner,
                                    Action_FailItem = reply.FailItem == "" ? reply.FailItem : reply.FailItem + GetFailRateDeltaByFailItemAction(reply.FailItem, NewRangeFailItmeList, PreviousRangeFailItmeList, NewTotal, OldTotal),
                                    ItemNameType = item.ItemNameType.ToString(),
                                    Description = reportRow.Description,
                                    First_Yield_Rate_New = reportRow.First_Yield_Rate_New,
                                    First_Yield_Rate_Old = reportRow.First_Yield_Rate_Old,
                                    UPH_Achievement_Rate_New = reportRow.UPH_Achievement_Rate_New,
                                    UPH_Achievement_Rate_Old = reportRow.UPH_Achievement_Rate_Old,
                                    Fail_Rate_New = reportRow.Fail_Rate_New,
                                    Fail_Rate_Old = reportRow.Fail_Rate_Old,
                                    Top_Fail_Item = reportRow.Top_Fail_Item,
                                    Org = reportRow.Org,
                                    Cause_Type = reply.Causeid != 0 ? CauseDict[reply.Causeid] : "結案",
                                    Cause_Comment = reply.CauseCommon,
                                    Action_Type = ActionDict[reply.Actionid],
                                    Action_Comment = reply.ActionCommon,
                                    AttachmentLink = reportRow.AttachmentLink,
                                    CurrentDateString = reportRow.CurrentDateString,
                                    LastDateString = reportRow.LastDateString
                                });
                            }
                        }
                        else
                        {
                            FinalReport.Add(reportRow);
                        }
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                }
                FinalReport = FinalReport.OrderBy(x => x.Org).ToList();
                return ProcessFinalExportIssueTable(FinalReport);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private static List<ExportReportModel> ProcessFinalExportIssueTable(List<ExportReportModel> Input)
        {
            try
            {
                var Output = Input.FindAll(x => x.First_Yield_Rate_New != "NaN" || x.First_Yield_Rate_Old != "NaN").ToList();
                return Output;
            }
            catch (Exception e)
            {
                return Input;
            }
        }

        public static List<IssueContentModel> GetIssueReplys(int? ItemNameType, string Org)
        {
            var Output = new List<IssueContentModel>();
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var Table = db.Query<IssueContentModel>($@"Select A.*,B.Action,C.Cause,D.ENAME From " +
                                                                                                                    "(SELECT Title_id,ItemNameType,FailItem,ActionCommon,CauseCommon,UserName,Owner,FileName,CreateTime,Actionid,Causeid  FROM [PTEDB].[dbo].[PTEWEB_Issues_Content] " +
                                                                                                                    "Where ItemNameType='" + ItemNameType + "')A " +
                                                                                                                    "left join [PTEDB].[dbo].[PTEWEB_Issues_Actions]B on A.Actionid=B.Actionid " +
                                                                                                                    "left join [PTEDB].[dbo].[PTEWEB_Issues_ReplyCause]C on A.Causeid=C.Causeid " +
                                                                                                                    "left join [EmployeeBasic].[dbo].[Employee_Basic]D on A.UserName=D.EmpID " +
                                                                                                                    "left join[PTEDB].[dbo].[PTEWEB_Issues_Title]E on A.Title_id = E.Title_id " +
                                                                                                                    "WHERE E.Org = '" + Org + "'").ToList();
                    Table.ForEach(item =>
                    {
                        Output.Add(new IssueContentModel
                        {
                            LinkImg = "<img src=\"../Content/hand.png\"/>",
                            Title_id = item.Title_id,
                            ItemNameType = item.ItemNameType,
                            FailItem = item.FailItem,
                            ActionCommon = item.ActionCommon,
                            CauseCommon = item.CauseCommon,
                            UserName = item.UserName,
                            Owner = item.Owner,
                            FileName = (item.FileName == null || item.FileName == "" || item.FileName == "NaN") ? "" : "<a href=\"./Download?Title=Attachment&path=" + item.FileName + "\" target=\"_blank\"><img src=\"../Content/File.png\" alt=\"File\" width=\"42\" height=\"42\"></img><a>",
                            CreateTime = Convert.ToDateTime(item.CreateTime).ToString("yyyy-MM-dd HH:mm:ss"),
                            Action = item.Action,
                            Cause = item.Cause,
                            ENAME = item.ENAME
                        });
                    });

                    return Output;
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return null;
            }
        }

        private static string GetFailRateDeltaByFailItemAction(string FailItem, List<PTEWEB_ItemNameType_ByDaily_TOP10_FailItem> NewFailItem, List<PTEWEB_ItemNameType_ByDaily_TOP10_FailItem> OldFailItem, int NewTotal, int OldTotal)
        {
            try
            {
                var NewItemList = DeltaIModel.TopFailTableTransferToFailInfoList(NewFailItem);
                var PreviousItemList = DeltaIModel.TopFailTableTransferToFailInfoList(OldFailItem);

                var NewItemFailRate = ((double)NewItemList.FindAll(x => x.FailItemName == FailItem).Select(y => y.FaiCount).ToList().Sum() / (double)NewTotal) * 100;
                var PreviousItemFailRate = ((double)PreviousItemList.FindAll(x => x.FailItemName == FailItem).Select(y => y.FaiCount).ToList().Sum() / (double)OldTotal) * 100;

                return $@" ({PreviousItemFailRate.ToString("F2")} % -> {NewItemFailRate.ToString("F2")} %)";
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        public static List<DailyUPH> GetDailyUPHTable(string org)
        {
            try
            {
                var Output = new List<DailyUPH>();

                using (var db = ConnectionFactory.CreatConnection())
                {
                    Output = db.Query<DailyUPH>($@"SELECT *  FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_RealOutput_ByDaily]   where  UPH!=999  and RealOutput>=10 and org = '{org}' and date between '{DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd")}'and '{DateTime.Now.ToString("yyyy-MM-dd")}' ").ToList();
                }

                return Output;
            }
            catch (Exception e)
            {
                return new List<DailyUPH>();
            }
        }

        public static void DeleteIssueAction(string createtime, string itemnametype, string issueid)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    db.Execute($@"DELETE FROM [PTEDB].[dbo].[PTEWEB_Issues_Content]   where Title_id = {issueid} and ItemNameType = {itemnametype} and CreateTime ='{createtime}' ");
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
            }
        }

        public static List<ItemRawData> GetAllRowDataAtCorrelation(int itemnumber, string Sdate, string Edate, int ItemNameType, string Org)
        {
            var Output = new List<ItemRawData>();
            var table = GetDBByItemNameType(ItemNameType).ToLower();
            try
            {
                var script = table != "tblcpu" ? $@"select SerialNumber,tdatetime,station,stationid,exeinfo,nohgpn,productname,username,item{itemnumber} as ItemResult,item{itemnumber}st as ItemStatus  from {table} where Item{itemnumber}St!=2 and ItemNameType ='{ItemNameType}'  and tDateTime between '{Sdate}' and '{Edate}' " : $@"select SerialNumber,tdatetime,station,stationid,exeinfo,fixtureid1 as nohgpn,productname,username,item{itemnumber} as ItemResult,item{itemnumber}st as ItemStatus  from {table} where Item{itemnumber}St!=2 and ItemNameType ='{ItemNameType}'  and tDateTime between '{Sdate}' and '{Edate}' ";

                using (var db = ConnectionFactory.CreatConnection(Org))
                {
                    var result1 = db.Query<ItemRawData>(script).ToList();
                    Output.AddRange(result1);
                    return Output;
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return new List<ItemRawData>();
            }
        }

        public static Dictionary<string, string> GetDeltaDictByTwoDate(List<Estimate_UPH> thisday, SimplyFYRInfo compareDay)
        {
            try
            {
                var Output = new Dictionary<string, string>();

                var thisFYR = 0.0;
                var thisDayCount = thisday.Sum(x => x.RealOutput);
                thisday.ForEach(item =>
                {
                    thisFYR = thisFYR + ((double)item.FYR * ((double)item.RealOutput / (double)thisDayCount));
                });

                var thisdaySimplyInfo = new SimplyFYRInfo()
                {
                    Spare = thisday.Average(x => x.AvgSpare),

                    EstimateUPH = thisday.Max(x => x.EstimateUPH),

                    FYR = thisFYR
                };

                var TFYR = Math.Round(thisFYR * 100, 2);
                var PFYR = Math.Round(compareDay.FYR, 2);
                var TSpare = Math.Round(thisdaySimplyInfo.Spare, 2);
                var PSpare = Math.Round(compareDay.Spare, 2);
                var TEstimate = Math.Round(thisdaySimplyInfo.EstimateUPH, 2);
                var PEstimate = Math.Round(compareDay.EstimateUPH, 2);

                Output["T_FYR"] = TFYR.ToString();
                Output["P_FYR"] = PFYR.ToString();
                Output["T_Spare"] = TSpare.ToString();
                Output["P_Spare"] = PSpare.ToString();
                Output["T_EstimateUPH"] = TEstimate.ToString();
                Output["P_EstimateUPH"] = PEstimate.ToString();
                Output["D_FYR"] = (Math.Round((TFYR - PFYR) / PFYR, 2) * 100).ToString();
                Output["D_Spare"] = (Math.Round((TSpare - PSpare) / PSpare, 2) * 100).ToString();
                Output["D_EstimateUPH"] = (Math.Round((TEstimate - PEstimate) / PEstimate, 2) * 100).ToString();
                return Output;
            }
            catch (Exception e)
            {
                return new Dictionary<string, string>();
            }
        }

        public static List<Estimate_UPH> GetRealUPHInfoByItemNameTyoe_Org_Shift(string org, int itemnametype, int shift, string date)
        {
            try
            {
                var script = $@"SELECT *  FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_RealOutput] where org = '{org}' and itemnametype ={itemnametype} and shiftid = {shift} and date ='{date}'  order by TimeIndex asc ";

                using (var db = ConnectionFactory.CreatConnection())
                {
                    return db.Query<Estimate_UPH>(script).ToList();
                }
            }
            catch (Exception e)
            {
                return new List<Estimate_UPH>();
            }
        }

        public static List<PieChartData> GetRealUPHPieData(List<Estimate_UPH> Input)
        {
            try
            {
                var MoreThanUPH = new PieChartData() { category = "MoreThanUPH", value = 0 };
                var NeedFollowUp = new PieChartData() { category = "NeedFollowUp", value = 0 };
                var TooBad = new PieChartData() { category = "BadUPH&NoDefine", value = 0 };
                var Output = new List<PieChartData>();
                Input.ForEach(item =>
                {
                    if (item.UPH != 999)
                    {
                        if (item.EstimateUPH > (item.UPH * 0.95))
                            MoreThanUPH.value++;
                        else if (item.EstimateUPH <= (item.UPH * 0.95))
                        {
                            var PercentGap = item.Gap / item.AvgSpare;

                            if (PercentGap <= 0.3)
                                NeedFollowUp.value++;
                            else if (PercentGap > 0.3)
                                TooBad.value++;
                        }
                    }
                });
                Output.Add(MoreThanUPH);
                Output.Add(NeedFollowUp);
                Output.Add(TooBad);

                return Output;
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return null;
            }
        }

        public static List<string> GetTestStatieonList(string _sdate, string _edate)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var Output = db.Query<string>($@"SELECT TestStation  FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily]  where date between '{_sdate}' and '{_edate}' and TestStation is not null  group by TestStation").ToList();
                    Output.Remove("");
                    return Output;
                }
            }
            catch (Exception e)
            {
                return new List<string>();
            }
        }

        public static List<string> GetTestGroupList(string _sdate, string _edate)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var Output = db.Query<string>($@"SELECT StationType  FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily]  where date between '{_sdate}' and '{_edate}' and StationType is not null  group by StationType").ToList();
                    Output.Remove("");
                    return Output;
                }
            }
            catch (Exception e)
            {
                return new List<string>();
            }
        }

        public static List<string> GetItemNameTypeList()
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var Output = db.Query<string>($@"SELECT CONVERT(varchar(5),max(ItemNameType)) + '_' + Description  FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily]  group by Description order by max(ItemNameType) asc").ToList();
                    Output.Remove("");
                    return Output;
                }
            }
            catch (Exception e)
            {
                return new List<string>();
            }
        }

        public static Dictionary<string, Dictionary<string, double>> ProcessGroupTrendData(Dictionary<string, string> DateDict, string org, string station, string group, string itemnametype)
        {
            try
            {
                var scripthead = $@"SELECT *  FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily]  ";
                var OrgCondition = org == "ALL" ? "where org in ('T1','T2','T3','T5')" : $@" Where org = '{org}'";
                var StationCondition = station == "ALL" ? string.Empty : $@" and TestStation = '{station}'";
                var GroupCondition = group == "ALL" ? string.Empty : $@"and StationType = '{group}'";
                var ItemNameTypeCondition = itemnametype == "ALL" ? string.Empty : $@" and ItemNameType = '{itemnametype}'";
                var Output = new Dictionary<string, Dictionary<string, double>>();
                var orglist = new List<string>();

                orglist = org == "ALL" ? new List<string>() { "T1", "T2", "T3", "T5" } : new List<string>() { { org } };

                var KeyList = DateDict.Keys.ToList();
                foreach (var key in KeyList)//for each Week
                {
                    var orgdict = new Dictionary<string, double>();
                    var datestringList = DateDict[key].Split(' ');
                    var sdate = datestringList[0];
                    var edate = datestringList[1];
                    var script = $@"{scripthead} {OrgCondition} {StationCondition} {GroupCondition} {ItemNameTypeCondition} and date between '{sdate}' and '{edate}'";
                    var DataSourceProcesser = new DataBaseGroundHandler(sdate, edate, script);

                    foreach (var item in orglist)//for each for ORG
                    {
                        var source = item != itemnametype ? DataSourceProcesser.DataTableSource.Where(x => x["Org"].ToString() == item) : DataSourceProcesser.DataTableSource;

                        if (source.Count() == 0)
                        {
                            orgdict[item] = 0.0;
                            orgdict[item + "_retry"] = 0.0;
                            continue;
                        }

                        var countlist = source.Select(x => x["Total"]).ToList();
                        var totalcount = 0.0;
                        var fyr = 0.0;
                        var retry_rate = 0.0;
                        countlist.ForEach(x =>
                        {
                            totalcount += (int)x;
                        });

                        var retry_TotalCount = totalcount;
                        foreach (var raw in source)//Each of ItemNameType
                        {
                            if (raw["TestStation"] == null)
                            {
                                if ((float.Parse(raw["Retry_Rate"].ToString()) > 100 && (float)((int)raw["D_Total"]) < 15) || raw["Description"].ToString().Contains("Cali"))
                                    retry_TotalCount = retry_TotalCount - (int)((int)raw["Total"]);
                            }
                            else
                            {
                                if ((float.Parse(raw["Retry_Rate"].ToString()) > 100 && (float)((int)raw["D_Total"]) < 15) || raw["Description"].ToString().Contains("Cali") || raw["TestStation"].ToString().Contains("SNRCalibration"))
                                    retry_TotalCount = retry_TotalCount - (int)((int)raw["Total"]);
                            }
                        }
                        foreach (var raw in source)//Each of ItemNameType
                        {
                            var rate = (float)((int)raw["Total"]) / totalcount;
                            var raw_fyr = float.Parse(raw["FYR"].ToString());
                            fyr = fyr + rate * raw_fyr;

                            var each_itemnametype_retry_rate = (float)((int)raw["Total"]) / retry_TotalCount;
                            var raw_retryRate = float.Parse(raw["Retry_Rate"].ToString());
                            if (raw["TestStation"] == null)
                            {
                                if ((float.Parse(raw["Retry_Rate"].ToString()) > 100 && (float)((int)raw["D_Total"]) < 15) || raw["Description"].ToString().Contains("Cali"))
                                    continue;
                            }
                            else
                            {
                                if ((float.Parse(raw["Retry_Rate"].ToString()) > 100 && (float)((int)raw["D_Total"]) < 15) || raw["Description"].ToString().Contains("Cali") || raw["TestStation"].ToString().Contains("SNRCalibration"))
                                    continue;
                            }

                            retry_rate = retry_rate + each_itemnametype_retry_rate * raw_retryRate;
                        }
                        orgdict[item] = Math.Round(fyr, 1);
                        orgdict[item + "_retry"] = Math.Round(retry_rate, 1);
                    }

                    Output[key] = orgdict;
                }
                return Output;
            }
            catch (Exception e)
            {
                return new Dictionary<string, Dictionary<string, double>>();
            }
        }
    }
}