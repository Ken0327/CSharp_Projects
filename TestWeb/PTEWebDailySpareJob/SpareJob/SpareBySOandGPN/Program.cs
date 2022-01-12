using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpareBySOandGPN
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
            for (int k = 1; k < 2; k++)
            {
                PEsupportSQL = "SELECT Org,Source,ItemNameType FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily] WHERE Date='" + Today.AddDays(-k).ToString("yyyy-MM-dd") + "' order by org asc";
                ItemNameTable = new DataTable();
                ItemNameTable = Conn.GetTableFromSql(PEsupportSQL, Conn.PEsupport_ConnString);
                List<string> SQLList = new List<string>();
                int c = 1;
                foreach (var items in ItemNameTable.AsEnumerable())
                {
                    string SpareSQL = "";
                    string TargetTestTable = "";
                    switch (items.Field<string>("Source"))
                    {
                        case "ATE":
                            TargetTestTable = "TblCPU";
                            SpareSQL = " SELECT Spare,fixtureID1 as GPN,attribute2 as jobnumber FROM " + TargetTestTable +
                                                          " where itemnametype = '" + items.Field<int>("itemnametype") + "' and tDateTime>= '" + Today.AddDays(-k).ToString("yyyy-MM-dd") + "' and tDateTime<'" + Today.AddDays(-k + 1).ToString("yyyy-MM-dd") + "' and Result = 1";
                            break;

                        case "FT":
                            TargetTestTable = "TblFinal";
                            SpareSQL = " SELECT Spare,NOHGPN as GPN,SO as jobnumber FROM " + TargetTestTable +
                                                          " where itemnametype = '" + items.Field<int>("itemnametype") + "' and tDateTime>= '" + Today.AddDays(-k).ToString("yyyy-MM-dd") + "' and tDateTime<'" + Today.AddDays(-k + 1).ToString("yyyy-MM-dd") + "' and Result = 1";
                            break;
                    }
                    DataTable Table = new DataTable();
                    string Org = items.Field<string>("Org");
                    DataTable ItemTable = new DataTable();
                    switch (Org)
                    {
                        case "T1":
                            ItemTable = DC.GetTableFromSql(SpareSQL, DC.T1_ConnString);
                            break;

                        case "T2":
                            ItemTable = DC.GetTableFromSql(SpareSQL, DC.T2_ConnString);
                            break;

                        case "T3":
                            ItemTable = DC.GetTableFromSql(SpareSQL, DC.T3_ConnString);
                            break;

                        case "T5":
                            ItemTable = DC.GetTableFromSql(SpareSQL, DC.T5_ConnString);
                            break;
                    }
                    if (ItemTable.Rows.Count > 0)
                    {
                        var Groups = from item in ItemTable.AsEnumerable()
                                     group item by item.Field<string>("GPN") into @group
                                     select new
                                     {
                                         GPN = @group.Key,
                                         SOGroup = @group.GroupBy(x => x.Field<string>("jobnumber"))
                                     };
                        foreach (var item in Groups)
                        {
                            string GPN = item.GPN;
                            if (!item.GPN.Contains('-'))
                            {
                                GPN = GPN.Substring(0, 3) + "-" + GPN.Substring(3, 5) + "-" + GPN.Substring(8, 2);
                            }
                            foreach (var g in item.SOGroup)
                            {
                                SQLList.Add("INSERT INTO [PTEDB].[dbo].[PTEWEB_ItemNameType_SpareDetail_ByDaily]([DateTime],[So],[GPN] ,[ItemNameType] ,[Passcount] ,[Spare])VALUES(" +
                                "'" + Today.AddDays(-k).ToString("yyyy-MM-dd") + "'," +
                                "'" + g.Key + "'," +
                                "'" + GPN + "'," +
                                "'" + items.Field<int>("itemnametype") + "'," +
                                "'" + g.Count() + "'," +
                                "'" + Math.Round(g.Select(x => Convert.ToDouble(x.Field<string>("spare").Trim())).Sum() / g.Count(), 2) + "');");
                            }
                        }
                    }
                    else
                    {
                        SQLList.Add("INSERT INTO [PTEDB].[dbo].[PTEWEB_ItemNameType_SpareDetail_ByDaily]([DateTime],[So],[GPN] ,[ItemNameType] ,[Passcount] ,[Spare])VALUES(" +
                              "'" + Today.AddDays(-k).ToString("yyyy-MM-dd") + "'," +
                              "'" + 9999999 + "'," +
                              "'XXX-ZZZZZ-YY'," +
                              "'" + items.Field<int>("itemnametype") + "'," +
                              "'" + 0 + "'," +
                              "'" + 0 + "');");
                    }
                }
                string Result = "";
                Conn.SetTransactionsSpareTime(SQLList, Conn.PEsupport_ConnString, out Result);
            }
        }
    }
}