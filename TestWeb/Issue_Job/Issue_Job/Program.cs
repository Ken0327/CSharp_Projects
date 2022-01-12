using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Issue_Job
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            for (int i = -1; i < 0; i++)
            {
                for (int j = 1; j < 6; j++)
                {
                    if (j == 4)
                        continue;
                    DailyCheck(i, ("T" + j));
                    WeeklyCheck(i, ("T" + j));
                }
            }
        }

        private static void WeeklyCheck(int k, string TxOrg)
        {
            DbConn conn = new DbConn();
            DateTime today = DateTime.Now.AddDays(k);
            //step1. 取得大表內容
            string FYRSQL = "";
            string nonITMFYRSQL = "";
            if (TxOrg == "T1")
            {
                FYRSQL = "Select ItemNameType,FYR,Date,Org,Description,D_Fail,D_Total FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily] where Date>='" + today.AddDays(-14).ToString("yyyy-MM-dd") + "' and Date<'" + today.AddDays(1).ToString("yyyy-MM-dd") + "'  and  Org='" + TxOrg + "' and Total>500  and (TestStation='ATE' or TestStation='RTESN' or TestStation='CTHT' or TestStation='FT' or TestStation='VT' or TestStation='Engineer')  order by ItemNameType,Date asc";
                nonITMFYRSQL = "Select ItemNameType,FYR,Date,Org,Description,D_Fail,D_Total FROM [PTEDB].[dbo].[PTEWEB_nonITMXP_ByDaily] where Date>='" + today.AddDays(-14).ToString("yyyy-MM-dd") + "' and Date<'" + today.AddDays(1).ToString("yyyy-MM-dd") + "'  and  Org='" + TxOrg + "' and Total>500  order by ItemNameType,Date asc";
            }
            if (TxOrg == "T2")
            {
                FYRSQL = "Select ItemNameType,FYR,Date,Org,Description,D_Fail,D_Total FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily] where Date>='" + today.AddDays(-14).ToString("yyyy-MM-dd") + "' and Date<'" + today.AddDays(1).ToString("yyyy-MM-dd") + "'  and  Org='" + TxOrg + "' and Total>1000  and (TestStation='ATE' or TestStation='RTESN' or TestStation='CTHT' or TestStation='FT' or TestStation='VT' or TestStation='Engineer')  order by ItemNameType,Date asc";
                nonITMFYRSQL = "Select ItemNameType,FYR,Date,Org,Description,D_Fail,D_Total FROM [PTEDB].[dbo].[PTEWEB_nonITMXP_ByDaily] where Date>='" + today.AddDays(-14).ToString("yyyy-MM-dd") + "' and Date<'" + today.AddDays(1).ToString("yyyy-MM-dd") + "'  and  Org='" + TxOrg + "' and Total>1000    order by ItemNameType,Date asc";
            }
            if (TxOrg == "T3")
            {
                FYRSQL = "Select ItemNameType,FYR,Date,Org,Description,D_Fail,D_Total FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily] where Date>='" + today.AddDays(-14).ToString("yyyy-MM-dd") + "' and Date<'" + today.AddDays(1).ToString("yyyy-MM-dd") + "'  and  Org='" + TxOrg + "' and Total>250  and (TestStation='ATE' or TestStation='RTESN' or TestStation='CTHT' or TestStation='FT' or TestStation='VT' or TestStation='Engineer')  order by ItemNameType,Date asc";
                nonITMFYRSQL = "Select ItemNameType,FYR,Date,Org,Description,D_Fail,D_Total FROM [PTEDB].[dbo].[PTEWEB_nonITMXP_ByDaily] where Date>='" + today.AddDays(-14).ToString("yyyy-MM-dd") + "' and Date<'" + today.AddDays(1).ToString("yyyy-MM-dd") + "'  and  Org='" + TxOrg + "' and Total>250    order by ItemNameType,Date asc";
            }

            if (TxOrg == "T5")
            {
                FYRSQL = "Select ItemNameType,FYR,Date,Org,Description,D_Fail,D_Total FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily] where Date>='" + today.AddDays(-14).ToString("yyyy-MM-dd") + "' and Date<'" + today.AddDays(1).ToString("yyyy-MM-dd") + "'  and  Org='" + TxOrg + "' and Total>250  and (TestStation='ATE' or TestStation='RTESN' or TestStation='CTHT' or TestStation='FT' or TestStation='VT' or TestStation='Engineer')  order by ItemNameType,Date asc";
                nonITMFYRSQL = "Select ItemNameType,FYR,Date,Org,Description,D_Fail,D_Total FROM [PTEDB].[dbo].[PTEWEB_nonITMXP_ByDaily] where Date>='" + today.AddDays(-14).ToString("yyyy-MM-dd") + "' and Date<'" + today.AddDays(1).ToString("yyyy-MM-dd") + "'  and  Org='" + TxOrg + "' and Total>250    order by ItemNameType,Date asc";
            }

            DataTable FYRTbl = conn.GetTableFromSql(FYRSQL, conn.PTEDB_ConnString);

            DataTable nonITMFYRTbl = conn.GetTableFromSql(nonITMFYRSQL, conn.PTEDB_ConnString);

            var ThreeDaysGroup = from f in FYRTbl.AsEnumerable()
                                 group f by f.Field<int>("ItemNameTYpe") into g
                                 select g;

            var nonITMThreeDaysGroup = from f in nonITMFYRTbl.AsEnumerable()
                                       group f by f.Field<int>("ItemNameTYpe") into g
                                       select g;

            foreach (var c in ThreeDaysGroup)
            {
                if (c.Count() >= 5)
                {
                    double FYR = Math.Round((1 - ((double)c.Sum(x => x.Field<int>("D_Fail")) / (double)c.Sum(x => x.Field<int>("D_Total")))) * 100, 2);
                    if (FYR < 90)
                    {
                        string itemSQL = "Select * FROM [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] where itemNameType='" + c.Key + "' and Support_Org='" + TxOrg + "'   order by LastUpdateDate desc";
                        DataTable singleItemNameType = conn.GetTableFromSql(itemSQL, conn.PTEDB_ConnString);
                        string org = TxOrg;

                        //1.資料表無記錄update itemNameType 新增記錄                               //2.資料表有記錄且issue為close, 轉為open，新增issue記錄
                        if (singleItemNameType.Rows[0]["Title_id_14Day"] == DBNull.Value || !Convert.ToBoolean(singleItemNameType.Rows[0]["Status"]))
                        {
                            if (checkNewTitleData(c.Key, org))
                            {
                                int insert_id = conn.ExecuteFromSqlReturnid("INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_Title]( [ItemNameType],[Title],[Issue_Status]  ,[CreateDate],[Org]) output INSERTED.Title_id VALUES('" + c.Key + "','系統監控雙週計算良率過低','" + 1 + "','" + today.ToString("yyyy-MM-dd") + "','" + org + "');", conn.PTEDB_ConnString);
                                string UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] set  LastUpdateDate='" + today.ToString("yyyy-MM-dd") + "',Title_id_14Day='" + insert_id + "',status=1 " +
                                                                   " where ItemNameType='" + c.Key + "' and Support_Org='" + org + "'; " +
                                                                   "INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_History]([ItemNameType] ,[Status],[Contents],[DateTime],[Org]) VALUES('" + c.Key + "','1','系統監控雙週計算良率過低','" + today.ToString("yyyy-MM-dd") + "','" + org + "');";
                                conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                            }
                            //no title set title desc 0 to titleid
                            else
                            {
                                int insert_id = conn.ExecuteFromSqlReturnid("select top 1 Title_id from PTEWEB_Issues_Title where itemnametype='" + c.Key + "' and org='" + org + "' order by title_id desc", conn.PTEDB_ConnString);
                                string UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] set  LastUpdateDate='" + today.ToString("yyyy-MM-dd") + "',Title_id_14Day='" + insert_id + "',status=1 " +
                                                                   " where ItemNameType='" + c.Key + "' and Support_Org='" + org + "'; " +
                                                                   "INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_History]([ItemNameType] ,[Status],[Contents],[DateTime],[Org]) VALUES('" + c.Key + "','1','系統監控雙週計算良率過低','" + today.ToString("yyyy-MM-dd") + "','" + org + "');UPDATE [PTEDB].[dbo].[PTEWEB_Issues_Title] SET [Title]='系統監控雙週計算良率過低' , [Issue_Status]='1',[CreateDate]='" + today.ToString("yyyy-MM-dd") + "'  where itemnametype='" + c.Key + "' and Title_id='" + insert_id + "' and [Org]='" + org.Trim() + "'";
                                conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                            }
                        }
                        else
                        {//open issue
                            if (checkNewTitleData(c.Key, org))
                            {
                                int insert_id = conn.ExecuteFromSqlReturnid("INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_Title]( [ItemNameType],[Title],[Issue_Status]  ,[CreateDate],[Org]) output INSERTED.Title_id VALUES('" + c.Key + "','系統監控雙週計算良率過低','" + 1 + "','" + today.ToString("yyyy-MM-dd") + "','" + org + "');", conn.PTEDB_ConnString);
                                string UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] set  LastUpdateDate='" + today.ToString("yyyy-MM-dd") + "',Title_id_14Day='" + insert_id + "',status=1 " +
                                                              " where ItemNameType='" + c.Key + "' and Support_Org='" + org + "'; " +
                                                              " INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_History]([ItemNameType] ,[Status],[Contents],[DateTime],[Org]) VALUES('" + c.Key + "','1','系統監控雙週計算良率過低','" + today.ToString("yyyy-MM-dd") + "','" + org + "');";
                                conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                            }
                            else
                            {
                                string UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_Title] SET [Title]='系統監控雙週計算良率過低' , [Issue_Status]='1',[CreateDate]='" + today.ToString("yyyy-MM-dd") + "'  where itemnametype='" + c.Key + "' and Title_id='" + Convert.ToInt32(singleItemNameType.Rows[0]["Title_id_14Day"]) + "' and [Org]='" + org.Trim() + "'";
                                conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                                UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] set  LastUpdateDate='" + today.ToString("yyyy-MM-dd") + "',status=1 " +
                                                         " where ItemNameType='" + c.Key + "' and Support_Org='" + org + "'; " +
                                                         "INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_History]([ItemNameType] ,[Status],[Contents],[DateTime],[Org]) VALUES('" + c.Key + "','1','系統監控雙週計算良率過低','" + today.ToString("yyyy-MM-dd") + "','" + org + "');";
                                conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                            }
                        }
                    }
                }
            }

            foreach (var c in nonITMThreeDaysGroup)
            {
                string check_sql = "Select * FROM [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] where itemNameType='" + c.Key + "' and Support_Org='" + TxOrg + "'   order by LastUpdateDate desc";
                DataTable checkTbl = conn.GetTableFromSql(check_sql, conn.PTEDB_ConnString);
                if (checkTbl.Rows.Count == 0)
                {
                    string insertNonITMSQL = "INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_ByDaily]([ItemNameType] ,[IssueAlive_Dates] ,[Status],[Support_Org] ,[LastUpdateDate],[FYR_Time],[ITM])" +
                                                           "VALUES('" + c.Key + "','0','0','" + TxOrg + "','" + today.ToString("yyyy-MM-dd") + "','" + c.LastOrDefault().Field<DateTime>("Date").ToString("yyyy-MM-dd") + "','0');";
                    conn.ExecuteFromSql(insertNonITMSQL, conn.PTEDB_ConnString);
                }

                if (c.Count() >= 5)
                {
                    double FYR = Math.Round((1 - ((double)c.Sum(x => x.Field<int>("D_Fail")) / (double)c.Sum(x => x.Field<int>("D_Total")))) * 100, 2);
                    if (FYR < 90)
                    {
                        string itemSQL = "Select * FROM [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] where itemNameType='" + c.Key + "' and Support_Org='" + TxOrg + "'   order by LastUpdateDate desc";
                        DataTable singleItemNameType = conn.GetTableFromSql(itemSQL, conn.PTEDB_ConnString);
                        string org = TxOrg;

                        //1.資料表無記錄update itemNameType 新增記錄                               //2.資料表有記錄且issue為close, 轉為open，新增issue記錄
                        if (singleItemNameType.Rows[0]["Title_id_14Day"] == DBNull.Value || !Convert.ToBoolean(singleItemNameType.Rows[0]["Status"]))
                        {
                            if (checkNewTitleData(c.Key, org))
                            {
                                int insert_id = conn.ExecuteFromSqlReturnid("INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_Title]( [ItemNameType],[Title],[Issue_Status]  ,[CreateDate],[Org]) output INSERTED.Title_id VALUES('" + c.Key + "','系統監控雙週計算良率過低','" + 1 + "','" + today.ToString("yyyy-MM-dd") + "','" + org + "');", conn.PTEDB_ConnString);
                                string UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] set  LastUpdateDate='" + today.ToString("yyyy-MM-dd") + "',Title_id_14Day='" + insert_id + "',status=1 " +
                                                                   " where ItemNameType='" + c.Key + "' and Support_Org='" + org + "'; " +
                                                                   "INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_History]([ItemNameType] ,[Status],[Contents],[DateTime],[Org]) VALUES('" + c.Key + "','1','系統監控雙週計算良率過低','" + today.ToString("yyyy-MM-dd") + "','" + org + "');";
                                conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                            }
                            else
                            {
                                int insert_id = conn.ExecuteFromSqlReturnid("select top 1 Title_id from PTEWEB_Issues_Title where itemnametype='" + c.Key + "' and org='" + org + "' order by title_id desc", conn.PTEDB_ConnString);
                                string UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] set  LastUpdateDate='" + today.ToString("yyyy-MM-dd") + "',Title_id_14Day='" + insert_id + "',status=1 " +
                                                                   " where ItemNameType='" + c.Key + "' and Support_Org='" + org + "'; " +
                                                                   "INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_History]([ItemNameType] ,[Status],[Contents],[DateTime],[Org]) VALUES('" + c.Key + "','1','系統監控雙週計算良率過低','" + today.ToString("yyyy-MM-dd") + "','" + org + "');UPDATE [PTEDB].[dbo].[PTEWEB_Issues_Title] SET [Title]='系統監控雙週計算良率過低' , [Issue_Status]='1',[CreateDate]='" + today.ToString("yyyy-MM-dd") + "'  where itemnametype='" + c.Key + "' and Title_id='" + insert_id + "' and [Org]='" + org.Trim() + "'";
                                conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                            }
                        }
                        else
                        {//open issue
                            if (checkNewTitleData(c.Key, org))
                            {
                                int insert_id = conn.ExecuteFromSqlReturnid("INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_Title]( [ItemNameType],[Title],[Issue_Status]  ,[CreateDate],[Org]) output INSERTED.Title_id VALUES('" + c.Key + "','系統監控雙週計算良率過低','" + 1 + "','" + today.ToString("yyyy-MM-dd") + "','" + org + "');", conn.PTEDB_ConnString);
                                string UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] set  LastUpdateDate='" + today.ToString("yyyy-MM-dd") + "',Title_id_14Day='" + insert_id + "',status=1 " +
                                                              " where ItemNameType='" + c.Key + "' and Support_Org='" + org + "'; " +
                                                              " INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_History]([ItemNameType] ,[Status],[Contents],[DateTime],[Org]) VALUES('" + c.Key + "','1','系統監控雙週計算良率過低','" + today.ToString("yyyy-MM-dd") + "','" + org + "');";
                                conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                            }
                            else
                            {
                                string UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_Title] SET [Title]='系統監控雙週計算良率過低' , [Issue_Status]='1',[CreateDate]='" + today.ToString("yyyy-MM-dd") + "'  where itemnametype='" + c.Key + "' and Title_id='" + Convert.ToInt32(singleItemNameType.Rows[0]["Title_id_14Day"]) + "',[Org]='" + org + "'";
                                conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                                UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] set  LastUpdateDate='" + today.ToString("yyyy-MM-dd") + "',status=1 " +
                                                         " where ItemNameType='" + c.Key + "' and Support_Org='" + org + "'; " +
                                                         "INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_History]([ItemNameType] ,[Status],[Contents],[DateTime],[Org]) VALUES('" + c.Key + "','1','系統監控雙週計算良率過低','" + today.ToString("yyyy-MM-dd") + "','" + org + "');";
                                conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                            }
                        }
                    }
                }
            }
        }

        private static void DailyCheck(int k, string TxOrg)
        {
            DbConn conn = new DbConn();
            DateTime today = DateTime.Now.AddDays(k);
            //step1. 取得大表內容
            string FYRSQL = "";
            string NonITMFYRSQL = "";
            if (TxOrg == "T1")
            {
                FYRSQL = "Select ItemNameType,FYR,Date,Org,Description FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily] where Date>='" + today.AddDays(-2).ToString("yyyy-MM-dd") + "' and Date<'" + today.AddDays(1).ToString("yyyy-MM-dd") + "' and  Org='" + TxOrg + "' and Total>500 and FYR<90 and (TestStation='ATE' or TestStation='RTESN' or TestStation='CTHT' or TestStation='FT' or TestStation='VT' or TestStation='Engineer') order by ItemNameType,Date asc";
                NonITMFYRSQL = "Select ItemNameType,FYR,Date,Org,Description FROM [PTEDB].[dbo].[PTEWEB_nonITMXP_ByDaily]  where Date>='" + today.AddDays(-2).ToString("yyyy-MM-dd") + "' and Date<'" + today.AddDays(1).ToString("yyyy-MM-dd") + "' and  Org='" + TxOrg + "' and Total>500 and FYR<90  order by ItemNameType,Date asc";
            }
            if (TxOrg == "T2")
            {
                FYRSQL = "Select ItemNameType,FYR,Date,Org,Description FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily] where Date>='" + today.AddDays(-2).ToString("yyyy-MM-dd") + "' and Date<'" + today.AddDays(1).ToString("yyyy-MM-dd") + "' and  Org='" + TxOrg + "' and Total>1000 and FYR<90 and (TestStation='ATE' or TestStation='RTESN' or TestStation='CTHT' or TestStation='FT' or TestStation='VT' or TestStation='Engineer') order by ItemNameType,Date asc";
                NonITMFYRSQL = "Select ItemNameType,FYR,Date,Org,Description FROM [PTEDB].[dbo].[PTEWEB_nonITMXP_ByDaily]  where Date>='" + today.AddDays(-2).ToString("yyyy-MM-dd") + "' and Date<'" + today.AddDays(1).ToString("yyyy-MM-dd") + "' and  Org='" + TxOrg + "' and Total>1000 and FYR<90  order by ItemNameType,Date asc";
            }
            if (TxOrg == "T3")
            {
                FYRSQL = "Select ItemNameType,FYR,Date,Org,Description FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily] where Date>='" + today.AddDays(-2).ToString("yyyy-MM-dd") + "' and Date<'" + today.AddDays(1).ToString("yyyy-MM-dd") + "' and  Org='" + TxOrg + "' and Total>250 and FYR<90 and (TestStation='ATE' or TestStation='RTESN' or TestStation='CTHT' or TestStation='FT' or TestStation='VT' or TestStation='Engineer') order by ItemNameType,Date asc";
                NonITMFYRSQL = "Select ItemNameType,FYR,Date,Org,Description FROM [PTEDB].[dbo].[PTEWEB_nonITMXP_ByDaily]  where Date>='" + today.AddDays(-2).ToString("yyyy-MM-dd") + "' and Date<'" + today.AddDays(1).ToString("yyyy-MM-dd") + "' and  Org='" + TxOrg + "' and Total>250 and FYR<90  order by ItemNameType,Date asc";
            }

            if (TxOrg == "T5")
            {
                FYRSQL = "Select ItemNameType,FYR,Date,Org,Description FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily] where Date>='" + today.AddDays(-2).ToString("yyyy-MM-dd") + "' and Date<'" + today.AddDays(1).ToString("yyyy-MM-dd") + "' and  Org='" + TxOrg + "' and Total>250 and FYR<90 and (TestStation='ATE' or TestStation='RTESN' or TestStation='CTHT' or TestStation='FT' or TestStation='VT' or TestStation='Engineer') order by ItemNameType,Date asc";
                NonITMFYRSQL = "Select ItemNameType,FYR,Date,Org,Description FROM [PTEDB].[dbo].[PTEWEB_nonITMXP_ByDaily]  where Date>='" + today.AddDays(-2).ToString("yyyy-MM-dd") + "' and Date<'" + today.AddDays(1).ToString("yyyy-MM-dd") + "' and  Org='" + TxOrg + "' and Total>250 and FYR<90  order by ItemNameType,Date asc";
            }

            DataTable FYRTbl = conn.GetTableFromSql(FYRSQL, conn.PTEDB_ConnString);

            DataTable NonITMFYRTbl = conn.GetTableFromSql(NonITMFYRSQL, conn.PTEDB_ConnString);

            var ThreeDaysGroup = from f in FYRTbl.AsEnumerable()
                                 group f by f.Field<int>("ItemNameTYpe") into g
                                 select g;
            var NonITMTreeDaysGroup = from f in NonITMFYRTbl.AsEnumerable()
                                      group f by f.Field<int>("ItemNameTYpe") into g
                                      select g;
            //ITMXP
            foreach (var items in ThreeDaysGroup)
            {
                foreach (var item in items)
                {
                    string itemSQL = "Select * FROM [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] where itemNameType='" + items.Key + "' and Support_Org='" + TxOrg + "' order by LastUpdateDate desc";
                    DataTable singleItemNameType = conn.GetTableFromSql(itemSQL, conn.PTEDB_ConnString);
                    if (singleItemNameType.Rows.Count == 0)
                    {
                        //add new record
                        string InsertSQL = "INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_ByDaily]([ItemNameType] ,[IssueAlive_Dates] ,[Status],[Support_Org] ,[LastUpdateDate],[FYR_Time])" +
                                                       "VALUES('" + items.Key + "','1','0','" + item.Field<string>("Org") + "','" + today.ToString("yyyy-MM-dd") + "','" + item.Field<DateTime>("Date").ToString("yyyy-MM-dd") + "');";
                        conn.ExecuteFromSql(InsertSQL, conn.PTEDB_ConnString);
                    }
                    else
                    {
                        if (item.Field<DateTime>("Date") > Convert.ToDateTime(singleItemNameType.Rows[0]["FYR_Time"]))
                        {
                            if (Convert.ToDateTime(singleItemNameType.Rows[0]["FYR_Time"]).AddDays(1) == item.Field<DateTime>("Date"))
                            {//連續天
                                if (Convert.ToInt32(singleItemNameType.Rows[0]["IssueAlive_Dates"]) == 2)
                                {
                                    //連續三天從沒發過或是有發過但issue close 必須新增一個issue並將status 更新為1
                                    if (singleItemNameType.Rows[0]["IssueAlive_Dates"] == DBNull.Value || !Convert.ToBoolean(singleItemNameType.Rows[0]["Status"]))
                                    {
                                        //檢查title 有無itemNameType
                                        int insert_id = conn.ExecuteFromSqlReturnid("INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_Title]( [ItemNameType],[Title],[Issue_Status]  ,[CreateDate],[Org]) output INSERTED.Title_id VALUES('" + items.Key + "','系統監控連續三天FYR小於90%','" + 1 + "','" + today.ToString("yyyy-MM-dd") + "','" + item.Field<string>("Org") + "');", conn.PTEDB_ConnString);
                                        string UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] set IssueAlive_Dates='3',status='1', LastUpdateDate='" + today.ToString("yyyy-MM-dd") + "',FYR_Time='" + item.Field<DateTime>("Date").ToString("yyyy-MM-dd") + "',Title_id_3Day='" + insert_id + "' " +
                                                                           " where ItemNameType='" + items.Key + "' and Support_Org='" + item.Field<string>("Org") + "'; " +
                                                                           " INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_History]([ItemNameType] ,[Status],[Contents],[DateTime],[Org]) VALUES('" + items.Key + "','1','系統監控連續三天FYR小於90%','" + today.ToString("yyyy-MM-dd") + "','" + item.Field<string>("Org") + "');";
                                        conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                                    }
                                    else //open issue
                                    {
                                        if (checkNewTitleData(items.Key, TxOrg))
                                        {
                                            int insert_id = conn.ExecuteFromSqlReturnid("INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_Title]( [ItemNameType],[Title],[Issue_Status]  ,[CreateDate],[Org]) output INSERTED.Title_id VALUES('" + items.Key + "','系統監控連續三天FYR小於90%','" + 1 + "','" + today.ToString("yyyy-MM-dd") + "','" + TxOrg + "');", conn.PTEDB_ConnString);
                                            string UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] set IssueAlive_Dates='3',status='1', LastUpdateDate='" + today.ToString("yyyy-MM-dd") + "',FYR_Time='" + item.Field<DateTime>("Date").ToString("yyyy-MM-dd") + "',Title_id_3Day='" + insert_id + "' " +
                                                                               " where ItemNameType='" + items.Key + "' and Support_Org='" + item.Field<string>("Org") + "'; " +
                                                                               " INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_History]([ItemNameType] ,[Status],[Contents],[DateTime],[Org]) VALUES('" + items.Key + "','1','系統監控連續三天FYR小於90%','" + today.ToString("yyyy-MM-dd") + "','" + TxOrg + "');";
                                            conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                                        }
                                        else
                                        {
                                            //title
                                            string UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_Title] SET [Title]='系統監控連續三天FYR小於90%' , [Issue_Status]='1',[CreateDate]='" + today.ToString("yyyy-MM-dd") + "'  where itemnametype='" + items.Key + "',[Org]='" + TxOrg + "'";
                                            conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                                            UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] set IssueAlive_Dates='3',status='1', LastUpdateDate='" + today.ToString("yyyy-MM-dd") + "',FYR_Time='" + item.Field<DateTime>("Date").ToString("yyyy-MM-dd") + "'" +
                                                                     " where ItemNameType='" + items.Key + "' and Support_Org='" + item.Field<string>("Org") + "';  " +
                                                                     " INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_History]([ItemNameType] ,[Status],[Contents],[DateTime],[Org]) VALUES('" + items.Key + "','1','系統監控連續三天FYR小於90%','" + today.ToString("yyyy-MM-dd") + "','" + TxOrg + "');";
                                            conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                                        }
                                    }
                                }
                                else
                                {
                                    //update table +1
                                    string UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] set IssueAlive_Dates='" + (Convert.ToInt32(singleItemNameType.Rows[0]["IssueAlive_Dates"]) + 1) + "', LastUpdateDate='" + today.ToString("yyyy-MM-dd") + "',FYR_Time='" + item.Field<DateTime>("Date").ToString("yyyy-MM-dd") + "' " +
                                                                 " where ItemNameType='" + items.Key + "' and Support_Org='" + item.Field<string>("Org") + "' ";
                                    conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                                }
                            }
                            else
                            {
                                string UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] set IssueAlive_Dates='1', LastUpdateDate='" + today.ToString("yyyy-MM-dd") + "',FYR_Time='" + item.Field<DateTime>("Date").ToString("yyyy-MM-dd") + "' " +
                                                                 " where ItemNameType='" + items.Key + "' and Support_Org='" + item.Field<string>("Org") + "'; ";
                                conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                                //has gap
                                //update issue to 1
                            }
                        }
                    }
                }
            }

            //NonITMXP
            foreach (var items in NonITMTreeDaysGroup)
            {
                foreach (var item in items)
                {
                    string itemSQL = "Select * FROM [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] where itemNameType='" + items.Key + "' and Support_Org='" + TxOrg + "' order by LastUpdateDate desc";
                    DataTable singleItemNameType = conn.GetTableFromSql(itemSQL, conn.PTEDB_ConnString);
                    if (singleItemNameType.Rows.Count == 0)
                    {
                        //add new record
                        string InsertSQL = "INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_ByDaily]([ItemNameType] ,[IssueAlive_Dates] ,[Status],[Support_Org] ,[LastUpdateDate],[FYR_Time],[ITM])" +
                                                       "VALUES('" + items.Key + "','1','0','" + item.Field<string>("Org") + "','" + today.ToString("yyyy-MM-dd") + "','" + item.Field<DateTime>("Date").ToString("yyyy-MM-dd") + "','0');";
                        conn.ExecuteFromSql(InsertSQL, conn.PTEDB_ConnString);
                    }
                    else
                    {
                        if (item.Field<DateTime>("Date") > Convert.ToDateTime(singleItemNameType.Rows[0]["FYR_Time"]))
                        {
                            if (Convert.ToDateTime(singleItemNameType.Rows[0]["FYR_Time"]).AddDays(1) == item.Field<DateTime>("Date"))
                            {//連續天
                                if (Convert.ToInt32(singleItemNameType.Rows[0]["IssueAlive_Dates"]) == 2)
                                {
                                    //連續三天從沒發過或是有發過但issue close 必須新增一個issue並將status 更新為1
                                    if (singleItemNameType.Rows[0]["IssueAlive_Dates"] == DBNull.Value || !Convert.ToBoolean(singleItemNameType.Rows[0]["Status"]))
                                    {
                                        //檢查title 有無itemNameType

                                        int insert_id = conn.ExecuteFromSqlReturnid("INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_Title]( [ItemNameType],[Title],[Issue_Status]  ,[CreateDate],[Org]) output INSERTED.Title_id VALUES('" + items.Key + "','系統監控連續三天FYR小於90%','" + 1 + "','" + today.ToString("yyyy-MM-dd") + "','" + item.Field<string>("Org") + "');", conn.PTEDB_ConnString);

                                        string UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] set IssueAlive_Dates='3',status='1', LastUpdateDate='" + today.ToString("yyyy-MM-dd") + "',FYR_Time='" + item.Field<DateTime>("Date").ToString("yyyy-MM-dd") + "',Title_id_3Day='" + insert_id + "' " +
                                                                           " where ItemNameType='" + items.Key + "' and Support_Org='" + item.Field<string>("Org") + "'; " +
                                                                           " INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_History]([ItemNameType] ,[Status],[Contents],[DateTime],[Org]) VALUES('" + items.Key + "','1','系統監控連續三天FYR小於90%','" + today.ToString("yyyy-MM-dd") + "','" + item.Field<string>("Org") + "');";
                                        conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                                    }
                                    else //open issue
                                    {
                                        if (checkNewTitleData(items.Key, TxOrg))
                                        {
                                            int insert_id = conn.ExecuteFromSqlReturnid("INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_Title]( [ItemNameType],[Title],[Issue_Status]  ,[CreateDate],[Org]) output INSERTED.Title_id VALUES('" + items.Key + "','系統監控連續三天FYR小於90%','" + 1 + "','" + today.ToString("yyyy-MM-dd") + "','" + TxOrg + "');", conn.PTEDB_ConnString);

                                            string UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] set IssueAlive_Dates='3',status='1', LastUpdateDate='" + today.ToString("yyyy-MM-dd") + "',FYR_Time='" + item.Field<DateTime>("Date").ToString("yyyy-MM-dd") + "',Title_id_3Day='" + insert_id + "' " +
                                                                               " where ItemNameType='" + items.Key + "' and Support_Org='" + item.Field<string>("Org") + "'; " +
                                                                               " INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_History]([ItemNameType] ,[Status],[Contents],[DateTime],[Org]) VALUES('" + items.Key + "','1','系統監控連續三天FYR小於90%','" + today.ToString("yyyy-MM-dd") + "','" + TxOrg + "');";
                                            conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                                        }
                                        else
                                        {
                                            //title
                                            string UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_Title] SET [Title]='系統監控連續三天FYR小於90%' , [Issue_Status]='1',[CreateDate]='" + today.ToString("yyyy-MM-dd") + "'  where itemnametype='" + items.Key + "',[Org]='" + TxOrg + "'";
                                            conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                                            UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] set IssueAlive_Dates='3',status='1', LastUpdateDate='" + today.ToString("yyyy-MM-dd") + "',FYR_Time='" + item.Field<DateTime>("Date").ToString("yyyy-MM-dd") + "'" +
                                                                     " where ItemNameType='" + items.Key + "' and Support_Org='" + item.Field<string>("Org") + "';  " +
                                                                     " INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_History]([ItemNameType] ,[Status],[Contents],[DateTime],[Org]) VALUES('" + items.Key + "','1','系統監控連續三天FYR小於90%','" + today.ToString("yyyy-MM-dd") + "','" + TxOrg + "');";
                                            conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                                        }
                                    }
                                }
                                else
                                {
                                    //update table +1
                                    string UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] set IssueAlive_Dates='" + (Convert.ToInt32(singleItemNameType.Rows[0]["IssueAlive_Dates"]) + 1) + "', LastUpdateDate='" + today.ToString("yyyy-MM-dd") + "',FYR_Time='" + item.Field<DateTime>("Date").ToString("yyyy-MM-dd") + "' " +
                                                                 " where ItemNameType='" + items.Key + "' and Support_Org='" + item.Field<string>("Org") + "' ";
                                    conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                                }
                            }
                            else
                            {
                                string UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] set IssueAlive_Dates='1', LastUpdateDate='" + today.ToString("yyyy-MM-dd") + "',FYR_Time='" + item.Field<DateTime>("Date").ToString("yyyy-MM-dd") + "' " +
                                                                 " where ItemNameType='" + items.Key + "' and Support_Org='" + item.Field<string>("Org") + "'; ";
                                conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                                //has gap
                                //update issue to 1
                            }
                        }
                    }
                }
            }
        }

        private static bool checkNewTitleData(int ItemNameType, string TxOrg)
        {
            DbConn conn = new DbConn();
            string SQL = "SELECT Issue_Status FROM [PTEDB].[dbo].[PTEWEB_Issues_Title]where itemNameType='" + ItemNameType + "' and Org='" + TxOrg + "' order by Title_id desc";
            DataTable dt = conn.GetTableFromSql(SQL, conn.PTEDB_ConnString);
            if (dt.Rows.Count > 0)
            {
                return !Convert.ToBoolean(dt.Rows[0]["Issue_Status"]);  //1 returen false->update 0->return true insert
            }
            else
            {
                return !false;//insert new record
            }
        }
    }
}