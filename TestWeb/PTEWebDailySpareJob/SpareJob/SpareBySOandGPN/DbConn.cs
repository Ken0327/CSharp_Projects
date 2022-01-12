using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace SpareBySOandGPN
{
    public class DbConn
    {
        private static ConnectionStringSettings _T1_ConnString;
        private static ConnectionStringSettings _T2_ConnString;
        private static ConnectionStringSettings _T3_ConnString;
        private static ConnectionStringSettings _T5_ConnString;
        private static ConnectionStringSettings _PEsupportConnString;

        public ConnectionStringSettings T1_ConnString
        {
            get
            {
                return _T1_ConnString;
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

        public ConnectionStringSettings PEsupport_ConnString
        {
            get
            {
                return _PEsupportConnString;
            }
        }

        static DbConn()
        {
            LoadConfiguration();
        }

        public void SetTransactionsSpareTime(List<string> transactionListData, ConnectionStringSettings SO_DBConn, out string Result)
        {
            using (SqlConnection Sconn = new SqlConnection(SO_DBConn.ConnectionString))
            {
                SqlTransaction trans = null;
                try
                {
                    Sconn.Open();
                    trans = Sconn.BeginTransaction();
                    foreach (string sql in transactionListData)
                    {
                        using (SqlCommand Com = new SqlCommand(sql, Sconn, trans))
                        {
                            Com.ExecuteNonQuery();
                        }
                    }
                    trans.Commit();

                    Result = "SPareTime 資料寫入成功，三廠共" + transactionListData.Count() + ":筆";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    Result = "SpareTime 資料寫入失敗";
                    //Log, handle or absorbe I don't care ^_^
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

        private static void LoadConfiguration()
        {
            _T1_ConnString = ConfigurationManager.ConnectionStrings["ATEDB_T1"];
            _T2_ConnString = ConfigurationManager.ConnectionStrings["ATEDB_T2"];
            _T3_ConnString = ConfigurationManager.ConnectionStrings["ATEDB_T3"];
            _T5_ConnString = ConfigurationManager.ConnectionStrings["ATEDB_T5"];
            _PEsupportConnString = ConfigurationManager.ConnectionStrings["PTEWEBDB"];
        }
    }
}