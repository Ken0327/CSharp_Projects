using Dapper;
using PTE_Web.Connections;
using PTE_Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PTE_Web.Controllers
{
    public class DataBaseGroundHandler
    {
        public string SDate { get; set; }

        public string EDate { get; set; }

        public DashBoardGroundModel ScriptModel { get; set; }

        public List<IDictionary<String, object>> DataTableSource { get; set; }

        public List<string> DataTableColumns { get; set; }

        public List<object> DataTableValues { get; set; }

        public string ExecuteScript { get; set; }

        public string defaultScript { get; set; }

        public bool QueryResult { get; set; }

        private List<IDictionary<String, object>> DataOutputHandler(string DataBaseGroup)
        {
            try
            {
                var list = new List<dynamic>();

                using (var db = ConnectionFactory.CreatConnection())
                {
                    list = db.Query(QueryResult ? ExecuteScript:defaultScript).ToList();
                }
                QueryResult = true;

                return DynamicHandler(list);
            }
            catch (Exception e)
            {
               var list = new List<dynamic>();

                QueryResult = false;

                using (var db = ConnectionFactory.CreatConnection())
                {
                    list = db.Query(defaultScript).ToList();
                }

                return new List<IDictionary<string, object>>();
            }
        }

        private static List<IDictionary<String, object>> DynamicHandler(List<dynamic> Input)
        {
            try
            {
                var Output = new List<IDictionary<String, object>>();
                foreach (dynamic rec in Input)
                {
                    var d = rec as IDictionary<string, object>;
                    Output.Add(d);
                }
                return Output;
            }
            catch (Exception e)
            {
                return new List<IDictionary<String, object>>();
            }
        }

        private static List<object> DataTableValuesHandler(List<IDictionary<String, object>> InputTable)
        {
            var TableValues = new List<object>();

            foreach (var item in InputTable)
            {
                TableValues.AddRange(item.Values.ToList());
            }

            return TableValues;
        }

        public DataBaseGroundHandler(string _sdate, string _edate,string _script, string databasegroup="")
        {
            QueryResult = false;
 
            var _SDate = DateTime.Today.AddDays(-7).ToString("yyyy-MM-dd");
            var _EDate = DateTime.Today.ToString("yyyy-MM-dd");

            if (_sdate != null && _edate != null)
            {
                try
                {
                    var _TryParseSDate = DateTime.Parse(_sdate);
                    var _TryParseEdate = DateTime.Parse(_edate);
                    if (_TryParseSDate < _TryParseEdate)
                    {
                        _SDate = _sdate;
                        _EDate = _edate;
                        ExecuteScript = _script;

                        QueryResult = true;
                    }
                    else
                    {
                        defaultScript = $@"Select * from {ScriptModel.DataBaseGroupScriptDict[databasegroup]} as FinalTable where date between '{SDate}' and '{EDate}'";

                    }

                }
                catch (Exception e)
                {
                    //Parse datetime fail, use initial date. (sdate/edate)
                }
            }
            SDate = _SDate;
            EDate = _EDate;

            ScriptModel = new DashBoardGroundModel(SDate, EDate);
            //ExecuteScript = _script.Replace(databasegroup, ScriptModel.DataBaseGroupScriptDict[databasegroup]);



            //defaultScript = $@"Select * from {ScriptModel.DataBaseGroupScriptDict[databasegroup]} as FinalTable where date between '{SDate}' and '{EDate}'";

            DataTableSource = DataOutputHandler(databasegroup);
            DataTableColumns = DataTableSource.Count!=0 ?DataTableSource.First().Keys.ToList():new List<string>();
            DataTableValues = DataTableValuesHandler(DataTableSource);
        }
    }
}