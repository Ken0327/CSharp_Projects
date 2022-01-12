using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpareJob
{
    internal class Program
    {
        private static string[] Orgs = new string[] { "T1", "T2", "T3", "T5" };
        public static DbConn DC = new DbConn();
        public static DateTime Yesterday = DateTime.Now.AddDays(-1).Date;
        public static DateTime Today = Yesterday.AddDays(1).Date;
        private static string PEsupportSQL = "";
        public static DbConn Conn = new DbConn();
        private static DataTable ItemNameTable = new DataTable();

        private static void Main(string[] args)
        {
            for (int k = 3; k > 1; k--)
            {
                PEsupportSQL = "SELECT Org,Source,ItemNameType FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily] WHERE Date='" + Today.AddDays(-k).ToString("yyyy-MM-dd") + "' order by org asc";
                ItemNameTable = new DataTable();
                ItemNameTable = Conn.GetTableFromSql(PEsupportSQL, Conn.PEsupport_ConnString);
                List<string> SQLList = new List<string>();
                int c = 1;
                foreach (var items in ItemNameTable.AsEnumerable())
                {
                    string TargetTestTable = "";
                    switch (items.Field<string>("Source"))
                    {
                        case "ATE":
                            TargetTestTable = "TblCPU";
                            break;

                        case "FT":
                            TargetTestTable = "TblFinal";
                            break;
                    }
                    DataTable Table = new DataTable();
                    string SpareAndRetryStr = "";
                    string SpareAndRetryStr2 = "";//tblcpu2
                    for (int j = 1; j <= 150; j++)
                    {
                        SpareAndRetryStr += "b.Retry" + j + ",b.spare" + j + "" + ",";
                    }
                    for (int j = 151; j <= 250; j++)
                    {
                        SpareAndRetryStr2 += "c.Retry" + j + ",c.spare" + j + "" + ",";
                    }

                    SpareAndRetryStr = SpareAndRetryStr.TrimEnd(',');
                    SpareAndRetryStr2 = SpareAndRetryStr2.TrimEnd(',');
                    string SQL = "select " + SpareAndRetryStr + "," + SpareAndRetryStr2 + "  FROM(select B.ESN,B.tdatetime,b.station," + SpareAndRetryStr + " FROM " +
                        "(SELECT ItemNameType,SerialNumber,tDateTime,station FROM " + TargetTestTable + "  with (nolock)  where tDateTime>='" + Today.AddDays(-k).ToString("yyyy-MM-dd") + "' and tDateTime<'" + Today.AddDays(-k + 1).ToString("yyyy-MM-dd") + "' and ItemNameType='" + items.Field<int>("ItemNameType") + "' and result=1) A" +
                        " inner  join [ate_db].[dbo].[TblTestTime]B  with (nolock) on A.Serialnumber=B.ESN and A.tdatetime=B.TdateTime and A.station=b.station)B inner join  [ate_db].[dbo].[TblTestTime2]C  with (nolock) on B.ESN=C.ESN and  B.tdatetime=C.TdateTime and b.station=c.station";
                    string Org = items.Field<string>("Org");

                    string TotalSpareSQL = " SELECT ROUND(avg(cast( Spare as int)),2)as AVGSpare FROM " + TargetTestTable +
                                                           " where itemnametype = '" + items.Field<int>("itemnametype") + "' and tDateTime>= '" + Today.AddDays(-k).ToString("yyyy-MM-dd") + "' and tDateTime<'" + Today.AddDays(-k + 1).ToString("yyyy-MM-dd") + "' and Result = 1";

                    DataTable spareTbl = new DataTable();
                    DataTable singleSpare = new DataTable();
                    switch (Org)
                    {
                        case "T1":
                            spareTbl = DC.GetTableFromSql(SQL, DC.T1_ConnString);
                            singleSpare = DC.GetTableFromSql(TotalSpareSQL, DC.T1_ConnString);
                            break;

                        case "T2":
                            spareTbl = DC.GetTableFromSql(SQL, DC.T2_ConnString);
                            singleSpare = DC.GetTableFromSql(TotalSpareSQL, DC.T2_ConnString);
                            break;

                        case "T3":
                            spareTbl = DC.GetTableFromSql(SQL, DC.T3_ConnString);
                            singleSpare = DC.GetTableFromSql(TotalSpareSQL, DC.T3_ConnString);
                            break;

                        case "T5":
                            spareTbl = DC.GetTableFromSql(SQL, DC.T5_ConnString);
                            singleSpare = DC.GetTableFromSql(TotalSpareSQL, DC.T5_ConnString);
                            break;
                    }
                    string cols = "";
                    string vals = "";
                    string totalSpare = singleSpare.Rows[0]["AVGSpare"].ToString();
                    for (int i = 1; i <= 250; i++)
                    {
                        double _AvgRetry = Math.Round(!spareTbl.AsEnumerable().Where(x => x["Retry" + i] != DBNull.Value).Any() ? 0.0 : spareTbl.AsEnumerable().Where(x => x["Retry" + i] != DBNull.Value).Average(x => Convert.ToDouble(x.Field<int>("Retry" + i))), 2);
                        double _AvgSpare = Math.Round(!spareTbl.AsEnumerable().Where(x => x["Spare" + i] != DBNull.Value).Any() ? 0.0 : spareTbl.AsEnumerable().Where(x => x["Spare" + i] != DBNull.Value).Average(x => Convert.ToDouble(x.Field<int>("Spare" + i))), 2);
                        cols += "Retry" + i + ",Spare" + i + ",";
                        vals += "'" + _AvgRetry + "','" + _AvgSpare + "',";
                    }
                    SQLList.Add("Insert into [PTEDB].[dbo].[PTEWEB_ItemNameType_SpareTime_ByDaily](ItemNameType,Org," + cols + "DateTime,AvgSpare)values('" + items.Field<int>("itemnametype") + "','" + items.Field<string>("Org") + "'," + vals + "'" + Today.AddDays(-k).ToString("yyyy-MM-dd") + "','" + totalSpare + "')");

                    Console.WriteLine("剩餘項目" + (ItemNameTable.Rows.Count - c).ToString());
                    c++;
                    Thread.Sleep(200);
                }
                string Result = "";
                Conn.SetTransactionsSpareTime(SQLList, Conn.PEsupport_ConnString, out Result);
            }
        }
    }
}