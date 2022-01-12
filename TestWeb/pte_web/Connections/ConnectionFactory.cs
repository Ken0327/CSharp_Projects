using System;
using System.Data;
using System.Data.SqlClient;

namespace PTE_Web.Connections
{
    public static class ConnectionFactory
    {
        public static IDbConnection CreatConnection(string name = "default")
        {
            switch (name)
            {
                case "default":
                    {
                        var ConnectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["PTEWebConnectionString"].ConnectionString;

                        return new SqlConnection(ConnectionString);
                    }
                case "T1":
                    {
                        var ConnectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["ATEServerT1ConnectionString"].ConnectionString;

                        return new SqlConnection(ConnectionString);
                    }
                case "T2":
                    {
                        var ConnectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["ATEServerT2ConnectionString"].ConnectionString;

                        return new SqlConnection(ConnectionString);
                    }
                case "T3":
                    {
                        var ConnectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["ATEServerT3ConnectionString"].ConnectionString;

                        return new SqlConnection(ConnectionString);
                    }
                case "T5":
                    {
                        var ConnectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["ATEServerT5ConnectionString"].ConnectionString;

                        return new SqlConnection(ConnectionString);
                    }
                case "C5":
                    {
                        var ConnectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["ATEServerC5ConnectionString"].ConnectionString;

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