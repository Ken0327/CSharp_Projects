using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PTE_Web.Models;

namespace PTE_Web.Controllers
{
    public class FixtureBoardController : Controller
    {
        private static string conn = ConfigurationManager.ConnectionStrings["ATEServerT1ConnectionString"].ToString();

        public ActionResult Index(string org)
        {
            switch (org)
            {
                case "T1":
                    ViewBag.org = org;
                    conn = ConfigurationManager.ConnectionStrings["ATEServerT1ConnectionString"].ToString();
                    break;

                case "T2":
                    ViewBag.org = org;
                    conn = ConfigurationManager.ConnectionStrings["ATEServerT2ConnectionString"].ToString();
                    break;

                case "T3":
                    ViewBag.org = org;
                    conn = ConfigurationManager.ConnectionStrings["ATEServerT3ConnectionString"].ToString();
                    break;

                case "T5":
                    ViewBag.org = org;
                    conn = ConfigurationManager.ConnectionStrings["ATEServerT5ConnectionString"].ToString();
                    break;

                default:
                    ViewBag.org = "T1";
                    conn = ConfigurationManager.ConnectionStrings["ATEServerT1ConnectionString"].ToString();
                    break;
            }

            string sql_org = @"CREATE TABLE #Table_name
                                        (
                                           itemnametype int,
                                           fixtureid2 varchar(20),
                                           station varchar(30),
                                           failcount int
                                        )
                                        INSERT INTO #Table_name (itemnametype,fixtureid2,station,failcount)
                                        select Top 20 itemnametype,fixtureid2,station, count(result) as failcount from tblcpu where result ='0' and Fixtureid2 !='' and fixtureid2 !='0' and fixtureid2 !='65535' and testtype2 not between 1501 and 1600 and tdatetime >= DATEADD(hour,-4,GETDATE()) and tdatetime <= GETDATE() group by itemnametype,fixtureid2,Station order by failcount desc
                                        select A.*,B.ItemDescription from #Table_name A inner join[ate_result].[dbo].[ItemName]B on A.ItemNameType = B.ItemNameType
                                        DROP TABLE #Table_name";
            string sql_os = @"CREATE TABLE #Table_name
                                        (
                                           itemnametype int,
                                           fixtureid2 varchar(20),
                                           station varchar(30),
                                           failcount int
                                        )
                                        INSERT INTO #Table_name (itemnametype,fixtureid2,station,failcount)
                                        select TOP 20 itemnametype,fixtureid2,station ,count(result) as failcount from tblcpu where result ='0' and testtype2 between 1501 and 1600 and Fixtureid2 !='' and fixtureid2 !='0' and fixtureid2 !='65535' and tdatetime >= DATEADD(hour,-2,GETDATE()) and tdatetime <= GETDATE() group by itemnametype,fixtureid2,station order by failcount desc
                                        select A.*,B.ItemDescription from #Table_name A inner join[ate_result].[dbo].[ItemName]B on A.ItemNameType = B.ItemNameType
                                        DROP TABLE #Table_name";
            DataTable dt_org = GetDataTable(sql_org);
            DataTable dt_os = GetDataTable(sql_os);
            List<string[]> FixtureDes = new List<string[]>();
            FixtureDes.Add(
                (from r in dt_org.AsEnumerable()
                 orderby r.Field<int>("FailCount") descending
                 select r.Field<string>("ItemDescription") + "_" + r.Field<string>("fixtureid2") + "號治具" + "_" + r.Field<string>("station")).ToArray()
                );
            FixtureDes.Add(
               (from r in dt_os.AsEnumerable()
                orderby r.Field<int>("FailCount") descending
                select r.Field<string>("ItemDescription") + "_" + r.Field<string>("fixtureid2") + "號治具" + "_" + r.Field<string>("station")).ToArray()
               );
            ViewBag.Label = FixtureDes;
            ViewBag.DT_org = dt_org;
            ViewBag.DT_os = dt_os;
            List<DataPoint> FixtureInfo = new List<DataPoint>
            {
                new DataPoint
                {
                    Fixture="廠內治具",
                    FailCount=
                     (from r in dt_org.AsEnumerable()
                      orderby r.Field<int>("FailCount") descending
                      select r.Field<int>("failcount")

                      ).ToArray(),
                     Fixture_ID=
                     (from r in dt_org.AsEnumerable()
                      orderby r.Field<int>("FailCount") descending
                      select r.Field<string>("fixtureid2")
                      ).ToArray()
                 },
                new DataPoint
                {
                    Fixture="外包治具",
                   FailCount=
                     (from r in dt_os.AsEnumerable()
                       orderby r.Field<int>("FailCount") descending
                      select r.Field<int>("failcount")
                      ).ToArray(),
                     Fixture_ID=
                     (from r in dt_os.AsEnumerable()
                       orderby r.Field<int>("FailCount") descending
                      select r.Field<string>("fixtureid2")
                      ).ToArray()
}
            };
            return View(FixtureInfo);
        }

        public DataTable GetDataTable(string cmd)
        {
            try
            {
                DataTable dt = new DataTable();
                using (SqlConnection sqlConn = new SqlConnection(conn))
                {
                    sqlConn.Open();
                    SqlCommand sqlComm = new SqlCommand();
                    sqlComm.Connection = sqlConn;
                    sqlComm.CommandTimeout = 3000;
                    sqlComm.CommandText = cmd;
                    SqlDataAdapter adapter = new SqlDataAdapter(sqlComm);
                    adapter.Fill(dt);
                    return dt;
                }
            }
            catch (SqlException e)
            {
                return null;
            }
        }
    }
}