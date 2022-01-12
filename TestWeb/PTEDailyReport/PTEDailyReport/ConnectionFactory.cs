using System;
using System.Data;
using System.Data.SqlClient;

namespace PTEDailyReport
{
    public static class ConnectionFactory
    {
        public static IDbConnection CreatConnection(string name = "default")
        {
            switch (name)
            {
                case "default":
                    {
                        var ConnectionString = $@"Data Source = t1-pe-support\pesupport; User ID = PIENG; Password = Q2iT5cwHJW3FH;";

                        return new SqlConnection(ConnectionString);
                    }
                case "T1":
                    {
                        var ConnectionString = $@"Data Source = SHIWPD-ATESQLR; User ID = ate_oper; Password = ate.oper;";

                        return new SqlConnection(ConnectionString);
                    }
                case "T2":
                    {
                        var ConnectionString = $@"Data Source = JHOWPD-ATESQLR; User ID = ate_oper; Password = ate.oper;";

                        return new SqlConnection(ConnectionString);
                    }
                case "T3":
                    {
                        var ConnectionString = $@"Data Source = LINWPD-ATESQLR; User ID = ate_oper; Password = ate.oper;";

                        return new SqlConnection(ConnectionString);
                    }

                case "T5":
                    {
                        var ConnectionString = $@"Data Source = XINWPD-ATESQLR; User ID = ate_oper; Password = ate.oper;";

                        return new SqlConnection(ConnectionString);
                    }

                case "C5":
                    {
                        var ConnectionString = $@"Data Source = YANWPD-ATESQLR; User ID = ate_oper; Password = ate.oper;";

                        return new SqlConnection(ConnectionString);
                    }

                default:
                    {
                        throw new Exception("Connection name is not exiest");
                    }
            }
        }
    }
}