using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace UPHTest
{
    public class DbConn
    {
        private static ConnectionStringSettings _T1_ConnString;
        private static ConnectionStringSettings _T2_ConnString;
        private static ConnectionStringSettings _T3_ConnString;
        private static ConnectionStringSettings _PTEDB_ConnString;

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

        public ConnectionStringSettings PTEDB_ConnString
        {
            get
            {
                return _PTEDB_ConnString;
            }
        }

        static DbConn()
        {
            LoadConfiguration();
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
                    Sconn.Close();
                    return dt;
                }
                catch (Exception ex)
                {
                    Sconn.Close();
                    return null;
                }
            }
        }

        public int ExecuteFromSqlReturnid(string SQL, ConnectionStringSettings SO_DBConn)
        {
            using (SqlConnection Sconn = new SqlConnection(SO_DBConn.ConnectionString))
            {
                try
                {
                    Sconn.Open();
                    string Contraints = SQL;
                    SqlCommand Scmd = new SqlCommand(Contraints, Sconn);
                    Scmd.CommandTimeout = int.MaxValue;
                    int returnValue = (int)Scmd.ExecuteScalar();
                    Sconn.Close();
                    return returnValue;
                }
                catch (Exception ex)
                {
                    Sconn.Close();
                    return 0;
                }
            }
        }

        public void ExecuteFromSql(string SQL, ConnectionStringSettings SO_DBConn)
        {
            using (SqlConnection Sconn = new SqlConnection(SO_DBConn.ConnectionString))
            {
                try
                {
                    Sconn.Open();
                    string Contraints = SQL;
                    SqlCommand Scmd = new SqlCommand(Contraints, Sconn);
                    Scmd.CommandTimeout = int.MaxValue;
                    Scmd.ExecuteNonQuery();
                    Sconn.Close();
                }
                catch (Exception ex)
                {
                    Sconn.Close();
                }
            }
        }

        private static void LoadConfiguration()
        {
            _T1_ConnString = ConfigurationManager.ConnectionStrings["ATEDB_T1"];
            _T2_ConnString = ConfigurationManager.ConnectionStrings["ATEDB_T2"];
            _T3_ConnString = ConfigurationManager.ConnectionStrings["ATEDB_T3"];
            _PTEDB_ConnString = ConfigurationManager.ConnectionStrings["PTEWEBDB"];
        }
    }
}