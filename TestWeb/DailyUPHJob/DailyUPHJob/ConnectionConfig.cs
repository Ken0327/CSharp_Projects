using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using System.Linq;
using NLog;

namespace DailyUPHJob
{
    public class SqlCommandFunction
    {
        private static readonly Logger _PTElogger = LogManager.GetLogger("PTE_ReportLogger");

        public string ConnectionString { get; set; }

        public SqlCommandFunction()
        {
            ConnectionString = $@"Data Source=t1-pe-support\pesupport;Initial Catalog=PTEDB;Connection Timeout=300000; Persist Security Info=True;User ID=PIENG;Password=Q2iT5cwHJW3FH";
        }

        public List<PTEWEB_ItemNameType_RealOutput> GetDailyData(string processDate)
        {
            try
            {
                var Output = new List<PTEWEB_ItemNameType_RealOutput>();
                using (var db = new SqlConnection(ConnectionString))
                {
                    Output = db.Query<PTEWEB_ItemNameType_RealOutput>($@"select *  FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_RealOutput]  where Date = '{processDate}'  order by TimeIndex asc").ToList();

                    return Output;
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return new List<PTEWEB_ItemNameType_RealOutput>();
            }
        }

        public bool InsertDailyReport(List<PTEWEB_ItemNameType_RealOutput_ByDaily> data)
        {
            try
            {
                using (var db = new SqlConnection(ConnectionString))
                {
                    db.Execute("INSERT INTO [PTEDB].[dbo].[PTEWEB_ItemNameType_RealOutput_ByDaily] (Date,Org,ItemNameType,ProductName,RealOutput,EstimateUPH,UPH,AvgSpare ,[table],Gap)" +
                    " VALUES(@Date,@Org,@ItemNameType,@ProductName,@RealOutput,@EstimateUPH,@UPH,@AvgSpare ,@table,@Gap)", data);

                    return true;
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return false;
            }
        }
    }
}