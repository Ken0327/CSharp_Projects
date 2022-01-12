using System;
using System.Collections; // for arraylist
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;  // for using mssql
using System.Linq;
using System.Reflection;
using MySql.Data.MySqlClient; // for using mysql


public class ITMXPServerQuery
    {
        /// <summary>
    /// pte connection string
    /// </summary> 
        private string connectionString = "server=10.126.0.97;user id=ptereaderini;password=pt10wnerpwd;database=ITMXP;port = 3306";        

    /// <summary>
    /// Initializes a new instance of the QueryPtedb class. (1)
    /// </summary>
    public ITMXPServerQuery()
    {
    }

    /// <summary>
    /// Initializes a new instance of the QueryPtedb class. (2)
    /// </summary>
    /// <param name="dataSourceString"> connectiong string </param>
    public ITMXPServerQuery(string dataSourceString)
    {
        this.connectionString = dataSourceString;
    }

    /// <summary>
    /// Gets error message
    /// </summary>
    public string ErrorMessage
    {
        get;
        private set;
    }

    /// <summary>
    /// Method:: get query result from ptedb 
    /// </summary>
    /// <param name="queryString">query string</param>
    /// <returns>resultTable is the query result in datatable format</returns> 
    public DataTable QueryResult(string queryString)
    {
        DataTable resultTable = new DataTable();
        try
        {
            using (MySqlConnection mysqlConnection = new MySqlConnection())
            {
                mysqlConnection.ConnectionString = this.connectionString;
                mysqlConnection.Open();

                MySqlCommand mysqlCommand = mysqlConnection.CreateCommand();
                mysqlCommand.CommandText = queryString;

                MySqlDataAdapter sqlData = new MySqlDataAdapter(mysqlCommand);
                sqlData.Fill(resultTable);

                mysqlCommand.Dispose();
                ////mysqlConnection.Close();
                ////mysqlConnection.Dispose(); // SA1515
            }
        }
        catch (Exception errorMessage)
        {            
            this.ErrorMessage = errorMessage.ToString();
        }

        return resultTable;
    }

    /// <summary>
    /// Method:: execute update operation
    /// </summary>
    /// <param name="updateString">updatestring string</param>
    /// <returns>success or fail</returns> 
    public bool UpdateOperation(string updateString)
    {
        try
        {
            using (SqlConnection mssqlConnection = new SqlConnection())
            {
                mssqlConnection.ConnectionString = this.connectionString;
                mssqlConnection.Open();
                SqlCommand mssqlCommand = mssqlConnection.CreateCommand();
                mssqlCommand.CommandText = updateString;
                mssqlCommand.ExecuteNonQuery();
                mssqlCommand.Dispose();
                ////mssqlConnection.Close();
                ////mssqlConnection.Dispose();
            }
        }
        catch (Exception errorMessage)
        {
            this.ErrorMessage = errorMessage.ToString();
            
            if (this.ErrorMessage.Contains("違反 PRIMARY KEY 條件約束") || this.ErrorMessage.Contains("Violation of PRIMARY KEY constraint")) 
            {
                return true;
            }
            
            return false;
        }

        return true;
    }

    /// <summary>
    /// Method:: execute update operation and return the update identity
    /// </summary>
    /// <param name="updateString">updatestring string</param>
    /// <returns>identity number string</returns> 
    public string GetupdateOperationIdentity(string updateString)
    {
        string identityNumber = string.Empty;
        try
        {
            using (SqlConnection mssqlConnection = new SqlConnection())
            {
                mssqlConnection.ConnectionString = this.connectionString;
                mssqlConnection.Open();
                SqlCommand mssqlCommand = mssqlConnection.CreateCommand();
                mssqlCommand.CommandText = updateString;
                mssqlCommand.ExecuteNonQuery();

                mssqlCommand.CommandText = "select @@IDENTITY as esntestid";
                SqlDataReader myReader = mssqlCommand.ExecuteReader(); 

                while (myReader.Read())
                {
                    identityNumber = myReader["esntestid"].ToString();
                }                
                
                mssqlCommand.Dispose();
                ////mssqlConnection.Close();
                ////mssqlConnection.Dispose();
            }
        }
        catch (Exception errorMessage)
        {
            this.ErrorMessage = errorMessage.ToString();
            identityNumber = "error";
        }

        return identityNumber;
    }

    /// <summary>
    /// Method:: get ATE queryString that we can use it to find the upload data
    /// </summary>
    /// <param name="sourceString">source station </param>
    /// <param name="destinationString">destination station </param>
    /// <param name="targetString">target table </param>
    /// <param name="typeString">operation type </param>
    /// <returns>string that we can use it to query data</returns> 
    public string GetUploadTargetQueryString(string sourceString, string destinationString, string targetString, string typeString)
    {
        string queryString = "select top 1 * from tt_esn_upload where " +
                             "source = '" + sourceString + "' " +
                             "and destination = '" + destinationString + "' " +
                             "and target = '" + targetString + "' " +
                             "and type = '" + typeString + "' ";
        string startString = string.Empty;
        string returnString = string.Empty;

        try
        {
            using (SqlConnection mssqlConnection = new SqlConnection())
            {
                mssqlConnection.ConnectionString = this.connectionString;
                mssqlConnection.Open();

                SqlCommand mssqlCommand = mssqlConnection.CreateCommand();
                mssqlCommand.CommandText = queryString;

                SqlDataReader myReader = mssqlCommand.ExecuteReader();

                while (myReader.Read())
                {
                    startString = myReader["start"].ToString();
                }

                returnString = "select top 1 * from " + targetString + " where " +
                               "reference > " + startString +
                               " order by reference";

                mssqlCommand.Dispose();
                ////mssqlConnection.Close();
                ////mssqlConnection.Dispose();
            }
        }
        catch (Exception errorMessage)
        {
            this.ErrorMessage = errorMessage.ToString();
            returnString = "noData";
        }

        return returnString;
    }

    /// <summary>
    /// Method:: get PTE queryString that we can use it to find the upload data
    /// </summary>
    /// <param name="sourceString">source station </param>
    /// <param name="destinationString">destination station </param>
    /// <param name="targetString">target table </param>
    /// <param name="typeString">operation type </param>
    /// <returns>string that we can use it to query data</returns> 
    public string GetPtedbTargetQueryString(string sourceString, string destinationString, string targetString, string typeString)
    {
        string queryString = "select top 1 * from tt_esn_upload where " +
                             "source = '" + sourceString + "' " +
                             "and destination = '" + destinationString + "' " +
                             "and target = '" + targetString + "' " +
                             "and type = '" + typeString + "' ";
        string startString = string.Empty;
        string returnString = string.Empty;

        try
        {
            using (SqlConnection mssqlConnection = new SqlConnection())
            {
                mssqlConnection.ConnectionString = this.connectionString;
                mssqlConnection.Open();

                SqlCommand mssqlCommand = mssqlConnection.CreateCommand();
                mssqlCommand.CommandText = queryString;

                SqlDataReader myReader = mssqlCommand.ExecuteReader();

                while (myReader.Read())
                {
                    startString = myReader["start"].ToString();
                }

                returnString = "select top 10000 * from " + targetString + " where " +
                               "esntestid > " + startString +
                               " order by esntestid";

                mssqlCommand.Dispose();
                ////mssqlConnection.Close();
                ////mssqlConnection.Dispose();
            }
        }
        catch (Exception errorMessage)
        {
            this.ErrorMessage = errorMessage.ToString();
            returnString = "noData";
        }

        return returnString;
    }

    /// <summary>
    /// Method:: get station HashTable from tt_esn_station
    /// </summary>
    /// <returns>station HashTable  </returns>    
    public Hashtable GetStationHashTable()
    {
        Hashtable stationHashTable = new Hashtable();
        try
        {
            using (SqlConnection mssqlConnection = new SqlConnection())
            {
                string queryString = "select reference,station from tt_esn_station";

                mssqlConnection.ConnectionString = this.connectionString;
                mssqlConnection.Open();

                SqlCommand mssqlCommand = mssqlConnection.CreateCommand();
                mssqlCommand.CommandText = queryString;

                SqlDataReader myReader = mssqlCommand.ExecuteReader();
                
                while (myReader.Read())
                {
                    stationHashTable.Add(myReader["station"].ToString().ToUpper() as object, myReader["reference"] as object);
                }
                
                myReader.Close();
                ////myReader.Dispose();

                mssqlCommand.Dispose();
                ////mssqlConnection.Close();
                ////mssqlConnection.Dispose();
            }
        }
        catch (Exception errorMessage)
        {
            this.ErrorMessage = errorMessage.ToString();
        }

        return stationHashTable;
    }

    /// <summary>
    /// Method:: Save tblTable data to  tt_esn_main and tt_esn_test(000~s64)
    /// </summary>
    /// <param name="jobnoString">job number </param>
    /// <param name="stationIndex">station index </param>
    /// <param name="tblTable">tblcpu datatable</param>
    /// <param name="itemnameDataTableOfTblcpu">which itemname you use for tblTable</param>
    /// <param name="ateTableIndex">station index of ate_db.tblxxx</param>
    /// <param name="referenceString">reference of ate_db.tblxxx</param>
    /// <returns>success or fail </returns>
    public bool SaveTblTable(string jobnoString, string stationIndex, DataTable tblTable, DataTable itemnameDataTableOfTblcpu, string ateTableIndex, string referenceString)
    {
        ////DataTable itemnameDataTableOfTblcpu = new DataTable();
        ////itemnameDataTableOfTblcpu = this.QueryResult("Select * from tt_itemname where itemnametype = " + itemnameTypeString);
        
        // save to tt_esn_main >>>
        // esntestid 
        // esn
        string esnString = this.GetValueOfMainField(itemnameDataTableOfTblcpu, tblTable, "esn");

        if (esnString.Contains("1111111111"))
        {
            return true; // success, waive esn = 111111111 
        }

        // testtype
        string testTypeString = this.GetValueOfMainField(itemnameDataTableOfTblcpu, tblTable, "testtype");
        
        // result
        string resultString = this.GetValueOfMainField(itemnameDataTableOfTblcpu, tblTable, "result");
        
        // failitem
        string failitemString = this.GetValueOfMainField(itemnameDataTableOfTblcpu, tblTable, "failitem");
        
        // iwhen
        string iwhenString = this.GetValueOfMainField(itemnameDataTableOfTblcpu, tblTable, "iwhen");
        
        // spare
        string spareString = this.GetValueOfMainField(itemnameDataTableOfTblcpu, tblTable, "spare");
        
        // itemnametype
        string itemnametypeString = this.GetValueOfMainField(itemnameDataTableOfTblcpu, tblTable, "itemnametype");
        
        // station ***
        string stationString = stationIndex; 
        
        // fixture
        string fixtureString = this.GetValueOfMainField(itemnameDataTableOfTblcpu, tblTable, "fixture");
        
        // iwho
        string iwhoString = this.GetValueOfMainField(itemnameDataTableOfTblcpu, tblTable, "iwho");
        
        // failmode
        string failmodeString = this.GetValueOfMainField(itemnameDataTableOfTblcpu, tblTable, "failmode");
        
        // jobno ***
        // jobnoString = jobnoString
        
        // local_esntestid 
        // testUnikey
        long testUnikey;
        long esntestid;
        long station;
        long retry = 0;

        if (!long.TryParse(referenceString, out esntestid))
        {
            esntestid = 0;
        }

        if (!long.TryParse(ateTableIndex, out station))
        {
            station = 0;
        }

        testUnikey = station << 48 | esntestid << 16 | retry;
                
        // combine updatestring 
        string updateString = "insert into tt_esn_main " +
                              "(esn,testtype,result,failitem,iwhen,spare,itemnametype,station,fixture,iwho,failmode,jobno,testUnikey) values('" +
                              esnString + "','" +
                              testTypeString + "','" +
                              resultString + "','" +
                              failitemString + "','" +
                              iwhenString + "','" +
                              spareString + "','" +
                              itemnametypeString + "','" +
                              stationString + "','" +
                              fixtureString + "','" +
                              iwhoString + "','" +
                              failmodeString + "','" +
                              jobnoString + "','" +
                              testUnikey.ToString() + "')";

        if (this.UpdateOperation(updateString)) 
        {
            if (!this.SaveToTestTable(itemnameDataTableOfTblcpu, tblTable, testUnikey.ToString(), iwhenString))
            {
                return false; // send to tt_esn_test fail
            }
        }
        else
        {
            return false; // send to tt_esn_main fail
        }
        
        //// 2012-05-02 Daniel Lu Add tt_esn_conteol function >>>
        ////if (!this.UpdateControlTable(esnString, jobnoString, testTypeString, iwhenString, resultString))
        ////{
        ////    return false; // update to tt_esn_control fail
        ////}
        //// 2012-05-02 Daniel Lu Add tt_esn_conteol function <<<

        return true; // success
    }

    /// <summary>
    /// Method:: Update Conteol Table (1)
    /// </summary>
    /// <param name="esnString">the esn that you want to update or insert </param>
    /// <param name="jobnoString">the jobno of esn </param>
    /// <param name="testTypeString">the test type of esn </param>
    /// <param name="iwhenString">the updatetime of esn </param>
    /// <param name="resultString">the result of esn </param>
    /// <returns>success of fail </returns>
    public bool UpdateControlTable(string esnString, string jobnoString, string testTypeString, string iwhenString, string resultString)
    {
        string queryString = "select top 1 * from tt_esn_control with(nolock) where esn = '" + esnString + "'";

        DataTable controlTable = this.QueryResult(queryString);

        if (controlTable.Rows.Count == 0)
        {
            // insert the new esn
            string insertString = "insert into tt_esn_control " +
                                  "(esn,jobno,updatetime,locked,testflow) " +
                                  "values('" + esnString + "','" +
                                               jobnoString + "','" +
                                               iwhenString + "','" +
                                               "1','" +
                                               testTypeString + "')";

            if (!this.UpdateOperation(insertString))
            {
                return false;
            }
        }
        else
        {
            string testflowString = controlTable.Rows[0]["testflow"].ToString();
            string referenceString = controlTable.Rows[0]["reference"].ToString();

            if (testflowString.Contains(testTypeString))
            {
                // 重復測過相同的 testtype 且 pass 當做 fail 處理
                resultString = "0";
            }

            if (resultString == "0")
            {
                testTypeString = "," + testTypeString;

                // update the new esn
                string updateString = "update tt_esn_control "
                                    + "set updatetime = '" + iwhenString
                                    + "', locked = locked + 1"
                                    + " , assyjobno = '" + jobnoString + "'"
                                    + " where reference =" + referenceString;

                if (!this.UpdateOperation(updateString))
                {
                    return false;
                }
            }
            else if (resultString == "1" && testflowString == string.Empty)
            {
                // update the existed esn but testflowString is empty
                string updateString = "update tt_esn_control "
                                    + "set updatetime = '" + iwhenString
                                    + "', locked = locked + 1"
                                    + " , testflow = '" + testTypeString + "'"
                                    + " , assyjobno = '" + jobnoString + "'"
                                    + " where reference = " + referenceString;

                if (!this.UpdateOperation(updateString))
                {
                    return false;
                }
            }
            else if (resultString == "1")
            {
                testTypeString = "," + testTypeString;

                // update the existed esn
                string updateString = "update tt_esn_control "
                                    + "set updatetime = '" + iwhenString
                                    + "', locked = locked + 1"
                                    + " , testflow = testflow + '" + testTypeString + "'"
                                    + " , assyjobno = '" + jobnoString + "'"
                                    + " where reference = " + referenceString;

                if (!this.UpdateOperation(updateString))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Method:: Update Conteol Table (2)
    /// </summary>
    /// <param name="esnString">the esn that you want to update or insert </param>
    /// <param name="jobnoString">the jobno of esn </param>
    /// <param name="testTypeString">the test type of esn </param>
    /// <param name="iwhenString">the updatetime of esn </param>
    /// <param name="resultString">the result of esn </param>
    /// <param name="appendString">append string </param>
    /// <returns>success of fail </returns>
    public bool UpdateControlTable(string esnString, string jobnoString, string testTypeString, string iwhenString, string resultString, string appendString)
    {
        string queryString = "select top 1 * from tt_esn_control with(nolock) where esn = '" + esnString + "'";

        DataTable controlTable = this.QueryResult(queryString);

        if (controlTable.Rows.Count == 0)
        {
            // insert the new esn
            string insertString = "insert into tt_esn_control " +
                                  "(esn,jobno,updatetime,locked,append,testflow) " +
                                  "values('" + esnString + "','" +
                                               jobnoString + "','" +
                                               iwhenString + "','" +
                                               "1','" +
                                               appendString + "','" +
                                               testTypeString + "')";

            if (!this.UpdateOperation(insertString))
            {
                return false;
            }
        }
        else
        {
            string testflowString = controlTable.Rows[0]["testflow"].ToString();
            string referenceString = controlTable.Rows[0]["reference"].ToString();

            if (testflowString.Contains(testTypeString))
            {
                // 重復測過相同的 testtype 且 pass 當做 fail 處理
                resultString = "0";
            }

            if (resultString == "0")
            {
                testTypeString = "," + testTypeString;

                // update the existed esn
                string updateString = "update tt_esn_control "
                                    + "set updatetime = '" + iwhenString
                                    + "', locked = locked + 1"
                                    + " , append = '" + appendString + "'"
                                    + " , assyjobno = '" + jobnoString + "'"
                                    + " where reference =" + referenceString;

                if (!this.UpdateOperation(updateString))
                {
                    return false;
                }
            }
            else if (resultString == "1" && testflowString == string.Empty)
            {
                // update the existed esn but testflowString is empty
                string updateString = "update tt_esn_control "
                                    + "set updatetime = '" + iwhenString
                                    + "', locked = locked + 1"
                                    + " , append = '" + appendString + "'"
                                    + " , testflow = '" + testTypeString + "'"
                                    + " , assyjobno = '" + jobnoString + "'"
                                    + " where reference = " + referenceString;

                if (!this.UpdateOperation(updateString))
                {
                    return false;
                }
            }
            else if (resultString == "1")
            {
                testTypeString = "," + testTypeString;

                // update the existed esn
                string updateString = "update tt_esn_control "
                                    + "set updatetime = '" + iwhenString
                                    + "', locked = locked + 1"
                                    + " , append = '" + appendString + "'"
                                    + " , testflow = testflow + '" + testTypeString + "'"
                                    + " , assyjobno = '" + jobnoString + "'"
                                    + " where reference = " + referenceString;

                if (!this.UpdateOperation(updateString))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Method:: Bulk Copy Table (main and tt_esn_test_xxx)
    /// </summary>
    /// <param name="insertData">the Datatable you want to  Bulk Copy  </param>
    /// <param name="targetTable">the target Table </param>
    /// <returns>success of fail </returns>
    public bool BulkCopyTable(DataTable insertData, string targetTable)
    {
        SqlBulkCopy bulkCopy = new SqlBulkCopy(this.connectionString, SqlBulkCopyOptions.CheckConstraints);
        try
        {
            bulkCopy.DestinationTableName = targetTable;
            if (targetTable == "tt_esn_main")
            {
                // 將資料源表字段和目標表的欄位做個映射 main
                bulkCopy.ColumnMappings.Add("esn", "esn");
                bulkCopy.ColumnMappings.Add("testtype", "testtype");
                bulkCopy.ColumnMappings.Add("result", "result");
                bulkCopy.ColumnMappings.Add("failitem", "failitem");
                bulkCopy.ColumnMappings.Add("iwhen", "iwhen");
                bulkCopy.ColumnMappings.Add("spare", "spare");
                bulkCopy.ColumnMappings.Add("itemnametype", "itemnametype");
                bulkCopy.ColumnMappings.Add("station", "station");
                bulkCopy.ColumnMappings.Add("fixture", "fixture");
                bulkCopy.ColumnMappings.Add("iwho", "iwho");
                bulkCopy.ColumnMappings.Add("failmode", "failmode");
                bulkCopy.ColumnMappings.Add("jobno", "jobno");
                bulkCopy.ColumnMappings.Add("testUnikey", "testUnikey");
            }
            else if (targetTable == "tt_esn_control")
            {
                // 將資料源表字段和目標表的欄位做個映射 control
                bulkCopy.ColumnMappings.Add("esn", "esn");
                bulkCopy.ColumnMappings.Add("jobno", "jobno");
                bulkCopy.ColumnMappings.Add("finalidx", "finalidx");
                bulkCopy.ColumnMappings.Add("updatetime", "updatetime");
                bulkCopy.ColumnMappings.Add("locked", "locked");
                bulkCopy.ColumnMappings.Add("failitem", "failitem");
                bulkCopy.ColumnMappings.Add("append", "append");
                bulkCopy.ColumnMappings.Add("testflow", "testflow");
                bulkCopy.ColumnMappings.Add("assyjobno", "assyjobno");
            }
            else
            {
                // 將資料源表字段和目標表的欄位做個映射 test
                bulkCopy.ColumnMappings.Add("esntestid", "esntestid");
                bulkCopy.ColumnMappings.Add("state", "state");
                bulkCopy.ColumnMappings.Add("value", "value");
                bulkCopy.ColumnMappings.Add("spare", "spare");
                bulkCopy.ColumnMappings.Add("iwhen", "iwhen");
            }

            bulkCopy.BulkCopyTimeout = 100; // 每處理1000筆觸發一個事件向頁面上輸出一個消息
            bulkCopy.SqlRowsCopied += (object Sender, SqlRowsCopiedEventArgs args) =>
            {
                Console.WriteLine("Bulk Copy to PTEDB " + targetTable + "：" + args.RowsCopied.ToString() + "records");
            };
            bulkCopy.NotifyAfter = 100;
            bulkCopy.WriteToServer(insertData);
        }
        catch (Exception ex)
        {
            string exception = ex.ToString();
            if (exception.Contains("The duplicate key value is ("))
            {
                int start = exception.LastIndexOf("The duplicate key value is (");
                exception = exception.Substring(start + 28);
                int end = exception.IndexOf(").");
                exception = exception.Substring(0, end) + "<EOF>";
                this.ErrorMessage = exception;
            }
            else
            {
                this.ErrorMessage = exception;
            }

            return false;
        }

        return true;
    }
    
    /// <summary>
    /// Method:: Accroding to the pMainField we can get sourceTable.gField
    ///          and return the sourceTable.gField.value
    /// </summary>
    /// <param name="mapOfTable">like a map form itemnametype table which itemnametype = xxx </param>
    /// <param name="soruceTable">source table</param>
    /// <param name="queryField">which tt_esn_main.filed.value you want to get </param>
    /// <returns>Value of gField of sourceTable </returns>
    private string GetValueOfMainField(DataTable mapOfTable, DataTable soruceTable, string queryField)
    { 
        string valueString = string.Empty;
        string garminFieldName = string.Empty;
        object queryFieldObjcet = new object();
        queryFieldObjcet = queryField as object;

        IEnumerable<DataRow> results = from myRow in mapOfTable.AsEnumerable()  // 先用DataTable.AsEnumerable() 把 IQueryable介面拿到手才能快樂的LINQ。    
                                       where myRow.Field<string>("pMainField") == queryFieldObjcet.ToString()
                                       select myRow;
        
        if (results.Count<DataRow>() > 0)
        {
            garminFieldName = results.CopyToDataTable<DataRow>().Rows[0]["gField"].ToString().Trim();
            valueString = soruceTable.Rows[0][garminFieldName].ToString();

            if (queryField.ToLower() == "result")
            {
                if (valueString.ToLower().Trim() == "false")
                {
                    valueString = "0";
                }
                else if (valueString.ToLower().Trim() == "true")
                {
                    valueString = "1";
                }
                else
                {
                    // do nothing
                }
            }
            else if (queryField.ToLower() == "iwhen")
            {
                DateTime dt = Convert.ToDateTime(valueString);
                valueString = dt.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                // do nothing
            }
        }
        
        return valueString;
    }

    /// <summary>
    /// Method:: Accroding to the mapOfTable insert data to tt_esn_test tables 
    /// </summary>
    /// <param name="mapOfTable">like a map form itemnametype table which itemnametype = xxx </param>
    /// <param name="soruceTable">source table</param>
    /// <param name="testUnikeyString">unikey of recorder </param>
    /// <param name="iwhenString">iwhen to tt_esn_test</param>
    /// <returns>success or fail</returns>
    private bool SaveToTestTable(DataTable mapOfTable, DataTable soruceTable, string testUnikeyString, string iwhenString)
    {
        foreach (DataRow row in mapOfTable.Rows)
        {
            if (row["pTable"].ToString() != null)  
            {
                string tableString = row["pTable"].ToString().Trim();
                string garminFieldName = row["gField"].ToString().Trim();
                string valueString = soruceTable.Rows[0][garminFieldName].ToString().Trim();

                if (valueString.ToLower().Trim() == "false")
                {
                    valueString = "0";
                }
                else if (valueString.ToLower().Trim() == "true")
                {
                    valueString = "1";
                }
                else if (valueString.Length > 200) 
                {
                    // string limit varchar(200)
                    valueString = valueString.Substring(0, 200);
                }
                else
                {
                    // do nothing
                }

                if (valueString != string.Empty)
                {
                    string updateString = string.Empty;
                    if (tableString.ToLower().Contains("_s"))
                    {
                        // tt_esn_test_s01 ~ tt_esn_test_s64
                        updateString = "insert into " + tableString +
                                       " (esntestid,value,iwhen) values('" +
                                       testUnikeyString + "','" +
                                       valueString + "','" +
                                       iwhenString + "')";
                    }
                    else
                    {
                        // tt_esn_test_000 ~ tt_esn_test_447
                        updateString = "insert into " + tableString +
                                       " (esntestid,value,iwhen) values('" +
                                       testUnikeyString + "','" +
                                       valueString + "','" +
                                       iwhenString + "')";
                    }

                    if (this.UpdateOperation(updateString))
                    {
                        // pass
                    }
                    else
                    {
                        // fail 
                        return false;
                    }
                }
                else
                { 
                    // value == null -> donothing 
                }
            }
        }

        return true;
    }
 }
