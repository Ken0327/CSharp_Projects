using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEWeb_DailyJob
{
	public class DBConvert
	{
		private string _connection;

		private string _sql;

		public DBConvert(string conName)
		{
			_connection = ConfigurationManager.ConnectionStrings[conName].ToString();
		}

		public string SQL
		{
			set
			{
				_sql = value;
			}
		}

		public void SaveDailyData(Models.PTEWEB_ItemNameType_ByDaily data)
		{
			Models.PTEDBEntities entity = new Models.PTEDBEntities();

			entity.PTEWEB_ItemNameType_ByDaily.Add(data);

			entity.SaveChanges();
		}

		public DataTable GetDataTable()
		{
			using (SqlConnection conn = new SqlConnection(_connection))
			{
				try
				{
					conn.Open();
					SqlCommand Scmd = new SqlCommand(this._sql, conn);
					DataTable dt = new DataTable();
					dt.Load(Scmd.ExecuteReader());
					conn.Close();
					conn.Dispose();
					return dt;
				}
				catch (Exception ex)
				{
					conn.Close();
					conn.Dispose();
					return null;
				}
			}
		}
	}
}