using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace PTEWeb_DailyJob
{
    public class DbConn
    {
        private static ConnectionStringSettings _T1_ConnString;
        private static ConnectionStringSettings _T2_ConnString;
        private static ConnectionStringSettings _T3_ConnString;
        private static ConnectionStringSettings _T5_ConnString;
        private static ConnectionStringSettings _TxSupport_ConnString;

        public ConnectionStringSettings T1_ConnString
        {
            get
            {
                return _T1_ConnString;
            }
        }

        public ConnectionStringSettings TxSupport_ConnString
        {
            get
            {
                return _TxSupport_ConnString;
            }
        }

        public ConnectionStringSettings T2_ConnString
        {
            get
            {
                return _T2_ConnString;
            }
        }

        public ConnectionStringSettings T3_ConnString
        {
            get
            {
                return _T3_ConnString;
            }
        }

        public ConnectionStringSettings T5_ConnString
        {
            get
            {
                return _T5_ConnString;
            }
        }

        static DbConn()
        {
            LoadConfiguration();
        }

        public void SetTransactionsuTubeFYR(List<Models.PTEWEB_uTube_ByDaily> transactionListData)

        {
            using (
                var context = new Models.PTEDBEntities())
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (Models.PTEWEB_uTube_ByDaily data in transactionListData)
                        {
                            context.PTEWEB_uTube_ByDaily.Add(data);
                        }
                        // do another changes
                        context.SaveChanges();

                        dbContextTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                    }
                }
            }
        }

        public void SetTransactionsNonITMFYR(List<Models.PTEWEB_nonITMXP_ByDaily> transactionListData)

        {
            using (var context = new Models.PTEDBEntities())
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (Models.PTEWEB_nonITMXP_ByDaily data in transactionListData)
                        {
                            context.PTEWEB_nonITMXP_ByDaily.Add(data);
                        }
                        // do another changes
                        context.SaveChanges();

                        dbContextTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                    }
                }
            }
        }

        public void SetTransactionsFYR(List<Models.PTEWEB_ItemNameType_ByDaily> transactionListData, out string Result)
        {
            using (var context = new Models.PTEDBEntities())
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (Models.PTEWEB_ItemNameType_ByDaily data in transactionListData)
                        {
                            context.PTEWEB_ItemNameType_ByDaily.Add(data);
                        }
                        // do another changes
                        context.SaveChanges();

                        dbContextTransaction.Commit();
                        Result = "FYR 資料寫入成功，四廠共" + transactionListData.Count() + ":筆";
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        Result = "FYR 資料寫入失敗";
                        //Log, handle or absorbe I don't care ^_^
                    }
                }
            }
        }

        public void SetTransactionsCycleTime(List<Models.PTEWEB_ItemNameType_ByDaily_TOP10_LongCycleTime> transactionListData, out string Result)
        {
            using (var context = new Models.PTEDBEntities())
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (Models.PTEWEB_ItemNameType_ByDaily_TOP10_LongCycleTime data in transactionListData)
                        {
                            context.PTEWEB_ItemNameType_ByDaily_TOP10_LongCycleTime.Add(data);
                        }
                        // do another changes
                        context.SaveChanges();

                        dbContextTransaction.Commit();
                        Result = "CycleTime 資料寫入成功，四廠共" + transactionListData.Count() + ":筆";
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        Result = "CycleTime 資料寫入失敗";
                        //Log, handle or absorbe I don't care ^_^
                    }
                }
            }
        }

        public void SetTransactionsNonITMFailItem(List<Models.PTEWEB_Athena_ByDaily_TOP10_FailItem> transactionListData)
        {
            using (var context = new Models.PTEDBEntities())
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (Models.PTEWEB_Athena_ByDaily_TOP10_FailItem data in transactionListData)
                        {
                            context.PTEWEB_Athena_ByDaily_TOP10_FailItem.Add(data);
                        }
                        // do another changes
                        context.SaveChanges();

                        dbContextTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                    }
                }
            }
        }

        public void SetTransactionsFailItem(List<Models.PTEWEB_ItemNameType_ByDaily_TOP10_FailItem> transactionListData, out string Result)
        {
            using (var context = new Models.PTEDBEntities())
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (Models.PTEWEB_ItemNameType_ByDaily_TOP10_FailItem data in transactionListData)
                        {
                            context.PTEWEB_ItemNameType_ByDaily_TOP10_FailItem.Add(data);
                        }
                        // do another changes
                        context.SaveChanges();

                        dbContextTransaction.Commit();
                        Result = "FailItem 資料寫入成功，四廠共" + transactionListData.Count() + ":筆";
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        Result = "FailItem 資料寫入失敗";
                        //Log, handle or absorbe I don't care ^_^
                    }
                }
            }
        }

        public DataTable GetTableFromSql(string SQL, ConnectionStringSettings SO_DBConn)
        {
            using (SqlConnection Sconn = new SqlConnection(SO_DBConn.ConnectionString))
            {
                try
                {
                    Sconn.Open();

                    DataTable dt = new DataTable();
                    string Contraints = SQL;
                    SqlCommand Scmd = new SqlCommand(Contraints, Sconn);
                    Scmd.CommandTimeout = int.MaxValue;
                    dt.Load(Scmd.ExecuteReader());
                    return dt;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public void ExecuteSql(string SQL, ConnectionStringSettings SO_DBConn)
        {
            using (SqlConnection Sconn = new SqlConnection(SO_DBConn.ConnectionString))
            {
                try
                {
                    Sconn.Open();

                    SqlCommand Scmd = new SqlCommand(SQL, Sconn);
                    string Contraints = SQL;

                    Scmd.ExecuteNonQuery();
                    Sconn.Close();
                }
                catch (Exception ex)
                {
                }
            }
        }

        private static void LoadConfiguration()
        {
            _T1_ConnString = ConfigurationManager.ConnectionStrings["ATEDB_T1"];
            _T2_ConnString = ConfigurationManager.ConnectionStrings["ATEDB_T2"];
            _T3_ConnString = ConfigurationManager.ConnectionStrings["ATEDB_T3"];
            _T5_ConnString = ConfigurationManager.ConnectionStrings["ATEDB_T5"];
            _TxSupport_ConnString = ConfigurationManager.ConnectionStrings["PTEWEBDB"];
        }
    }
}