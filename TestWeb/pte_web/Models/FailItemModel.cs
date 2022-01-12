using PTE_Web.Connections;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;

namespace PTE_Web.Models
{
    public class FailItemModel
    {
        public static List<FailItemTable> GetFailItemTableByItemName_Org(string Org, int ItemNameType, string Sdate, string Edate, string Source)
        {
            Dictionary<string, string> Sampling = new Dictionary<string, string>();
            Dictionary<string, string> UpperSpec = new Dictionary<string, string>();
            Dictionary<string, string> LowerSpec = new Dictionary<string, string>();
            Dictionary<string, string> ItemName_DBIndex = new Dictionary<string, string>();
            AutoImportINI(ItemNameType.ToString(),out Sampling, out UpperSpec, out LowerSpec, out ItemName_DBIndex, Org);
            var Output = new List<FailItemTable>();
            var AllFailItemDetail = new List<FailItemTable>();
            var ItemNameMappingDist = DataHandlerFunctions.GetItemByItemNameType(ItemNameType);
            //   DataTable SpareTbl = DataHandlerFunctions.DataSpareHandler(ItemNameType, Org, Sdate, Edate, Source);
            Output = DataHandlerFunctions.GetFailDetailInfo(ItemNameType, Org, Sdate, Edate) ?? new List<FailItemTable>();
            var ordercounter = 1;
            var CummlateFailCount = 0;
            var TotalFailCount = Output.Sum(x => x.FailCount);
            Output.ForEach(item =>
            {
                if (ItemNameMappingDist.Keys.Contains($@"Name{item.FailItem}"))
                {
                    CummlateFailCount += item.FailCount;
                    if (UpperSpec.ContainsKey((item.FailItem).ToString()))
                    {
                        item.SpecMax = Convert.ToString(UpperSpec[(item.FailItem).ToString()]);
                        item.SpecMin = Convert.ToString(LowerSpec[(item.FailItem).ToString()]);
                        item.SamplingRate = Convert.ToString(Sampling[(item.FailItem).ToString()]);
                    }
                    else
                    {
                        item.SpecMax = "";
                        item.SpecMin = "";
                    }
                    if(item.SamplingRate!="100")
                    {
                        item.TotalCount = DataHandlerFunctions.GetSamplingTestCountByItemNameTypeAndFailItem(ItemNameType, item.FailItem, Org, Sdate, Edate);
                    }
                    item.ItemName = ItemNameMappingDist[$@"Name{item.FailItem}"].ToString();

                    item.AccumulatePercent = Math.Round((double)CummlateFailCount * 100 / TotalFailCount, 2);

                    item.FailPercent = Math.Round((double)item.FailCount * 100 / TotalFailCount, 2);
                    item.FailRate = Math.Round(((float)item.FailCount/(float)item.TotalCount)*100, 2);
                }
            });

            Output = Output.OrderByDescending(x => x.FailRate).ToList();

            Output.ForEach(x => { x.Order = ordercounter; ordercounter++; });

            return Output;
        }

        private static DataTable SpareTbl;
        private static DataTable TestItemsTbl;

        public static List<ItemSpareData> GetFailItemSpareAndDelay(string Org, int ItemNameType, string Sdate, string Edate, string Source, out DataTable SpareTbl1)
        {
            TestItemsTbl = DataHandlerFunctions.TestItemsHandler(ItemNameType);
            SpareTbl = DataHandlerFunctions.DataSpareHandler(ItemNameType, Org, Sdate, Edate, Source);
            SpareTbl1 = SpareTbl;
            List<ItemSpareData> SpareListItems = new List<ItemSpareData>();

            foreach (var dr in TestItemsTbl.AsEnumerable())
            {
                if (dr["TestType"].ToString() == "9999999" || dr["TestType"].ToString() == "999999")
                    continue;
                ItemSpareData item = new ItemSpareData(Convert.ToInt32(dr["Idx_TestItem"]));
                if (item._AvgSpare == 0.0)
                {
                    continue;
                }

                item._TestItem = dr["Name"].ToString();
                SpareListItems.Add(item);
            }

            return SpareListItems;
        }

        public class FailrowData
        {
            public string Item { get; set; }
            public string SerialNumber { get; set; }
            public double Value { get; set; }
            public int Status { get; set; }

            public string SpecMax { get; set; }

            public string SpecMin { get; set; }

            public DateTime tdatetime { get; set; }

            public int result { get; set; }
        }

        public class ItemSpareData
        {
            [Display(Name = "DbIndex")]
            public int _DbIndex { get; set; }

            [Display(Name = "ItemName")]
            public string _TestItem { get; set; }

            [Display(Name = "AvgSpare")]
            private double AvgSpare = 0.0;

            public double _AvgSpare
            {
                get
                {
                    return AvgSpare / 1000;
                }
                set
                {
                    AvgSpare = value;
                }
            }

            public string _Skewness_First { get; set; }

            public string _Skewness_Second { get; set; }

            [Display(Name = "AvgRepeat")]
            public double _AvgRetry { get; set; }

            [Display(Name = "xpRepeat")]
            public int _xpRepeat { get; set; }

            private double timeout = 0.0;

            [Display(Name = "TimeOut")]
            public double Timeout
            {
                get
                {
                    return timeout / 1000;
                }
                set
                {
                    timeout = value;
                }
            }

            private double passdelay = 0.0;

            [Display(Name = "FirstPassDelay")]
            public double _PassDelay
            {
                get
                {
                    return passdelay / 1000;
                }
                set
                {
                    passdelay = value;
                }
            }

            private double faildelay = 0.0;

            [Display(Name = "FailDelay")]
            public double _FailDelay
            {
                get
                {
                    return faildelay / 1000;
                }
                set
                {
                    faildelay = value;
                }
            }

            private double faildelay_repeat = 0.0;

            [Display(Name = "FailDelay_Repeat")]
            public double _FailDelay_Repeat
            {
                get
                {
                    return faildelay_repeat / 1000;
                }
                set
                {
                    faildelay_repeat = value;
                }
            }

            private double maxdelay = 0.0;

            [Display(Name = "MaxDelay")]
            public double _MaxDelay
            {
                get
                {
                    return maxdelay / 1000;
                }
                set
                {
                    maxdelay = value;
                }
            }

            private double PassDelay = 0.0;
            private double FailDelay = 0.0;
            private double MaxDelay = 0.0;
            private double FailDelay_Repeat = 0.0;

            public ItemSpareData(int idx_TestItem)
            {
                DataRow row = (from i in TestItemsTbl.AsEnumerable()
                               where i.Field<int>("Idx_TestItem") == idx_TestItem
                               select i).ToList()[0];

                Timeout = 0;

                if (Convert.ToInt32(row[2]) == 99999 || Convert.ToInt32(row[2]) == 999)
                {
                    _xpRepeat = 0;
                    _DbIndex = 0;
                    GetDelays(idx_TestItem, _xpRepeat, Convert.ToInt32(Timeout), out PassDelay, out FailDelay, out FailDelay_Repeat, out MaxDelay);
                    _PassDelay = PassDelay;
                    _FailDelay = FailDelay;
                    _FailDelay_Repeat = FailDelay_Repeat;
                    _MaxDelay = MaxDelay;
                    this._AvgRetry = 0;
                    this._AvgSpare = this._PassDelay;
                    this._Skewness_First = "0";
                    this._Skewness_Second = "0";
                }
                else
                {
                    _xpRepeat = row.Field<int>("xpRepeat");
                    _TestItem = row.Field<string>("Name");
                    _DbIndex = row.Field<int>("DBIndex");
                    this.Timeout = row.Field<int>("Timeout");
                    GetDelays(idx_TestItem, _xpRepeat, Convert.ToInt32(Timeout), out PassDelay, out FailDelay, out FailDelay_Repeat, out MaxDelay);
                    _PassDelay = PassDelay;
                    _FailDelay = FailDelay;
                    _FailDelay_Repeat = FailDelay_Repeat;
                    _MaxDelay = MaxDelay;
                    int spare = 0;
                    int retry = 0;

                    var v = SpareTbl.AsEnumerable().Where(x => x["Retry" + _DbIndex] != DBNull.Value);

                    this._AvgRetry = Math.Round(!SpareTbl.AsEnumerable().Where(x => x["Retry" + _DbIndex] != DBNull.Value).Any() ? 0.0 : SpareTbl.AsEnumerable().Where(x => x["Retry" + _DbIndex] != DBNull.Value).Average(x => Convert.ToDouble(x.Field<double>("Retry" + _DbIndex))), 2);
                    this._AvgSpare = Math.Round(!SpareTbl.AsEnumerable().Where(x => x["Spare" + _DbIndex] != DBNull.Value).Any() ? 0.0 : SpareTbl.AsEnumerable().Where(x => x["Retry" + _DbIndex] != DBNull.Value).Average(x => Convert.ToDouble(x.Field<double>("Spare" + _DbIndex))), 2);
                    string d1 = "0";
                    string d2 = "0";
                    if (SpareTbl.AsEnumerable().Where(x => x["Spare" + _DbIndex] != DBNull.Value).Any())
                    {
                        GetDistributeFirst_Second(this._AvgSpare, _DbIndex, SpareTbl.AsEnumerable().Where(x => x["Spare" + _DbIndex] != DBNull.Value).Count(), out d1, out d2);
                    }
                    this._Skewness_First = d1;
                    this._Skewness_Second = d2;
                }
            }
        }

        public static void GetDistributeFirst_Second(double avg, int _DbIndex, int DataCounts, out string d1, out string d2)
        {
            d1 = Math.Round(100 * (double)SpareTbl.AsEnumerable().Where(x => x["Spare" + _DbIndex] != DBNull.Value).Where(x => Convert.ToDouble(x["Spare" + _DbIndex]) < avg).Count() / DataCounts, 1).ToString();
            d2 = Math.Round(100.0 - Convert.ToDouble(d1), 1).ToString();
        }

        public static float standardDeviation(double[] arr, double mean, int n)
        {
            double sum = 0;

            // find standard deviation
            // deviation of data.
            for (int i = 0; i < n; i++)
                sum = (arr[i] - mean) * (arr[i] - mean);

            return (float)Math.Sqrt(sum / n);
        }

        private static float skewness(double[] arr, double mean, int n)
        {
            // Find skewness using
            // above formula
            double sum = 0;

            for (int i = 0; i < n; i++)
                sum = (arr[i] - mean) *
                      (arr[i] - mean) *
                      (arr[i] - mean);

            return (float)sum / (n * standardDeviation(arr, mean, n) *
                            standardDeviation(arr, mean, n) *
                            standardDeviation(arr, mean, n) *
                            standardDeviation(arr, mean, n));
        }

        private static void GetDelays(int idx_TestItem, int xpRepeat, int timeout, out double PassDelay, out double FailDelay, out double FailDelay_Repeat, out double MaxDelay)
        {
            ITMXPServerQuery queryitmxp = new ITMXPServerQuery();

            string ITMXPQueryString = "SELECT * FROM itmxp.tbl_commands where Idx_testitem='" + idx_TestItem + "'";
            DataTable CommandTable = queryitmxp.QueryResult(ITMXPQueryString);
            PassDelay = (from c in CommandTable.AsEnumerable()
                         where c.Field<int>("CmdList") == 0 || c.Field<int>("CmdList") == 1
                         select c.Field<int>("CmdDelay")).Sum();

            FailDelay = (from c in CommandTable.AsEnumerable()
                         where c.Field<int>("CmdList") == 2 && c.Field<int>("CmdRepeat") == 1
                         select c.Field<int>("CmdDelay")).Sum();
            var FailRow = (from c in CommandTable.AsEnumerable()
                           where c.Field<int>("CmdList") == 2
                           select c).ToList();
            FailDelay_Repeat = 0.0;
            MaxDelay = 0.0;
            if (xpRepeat != 0)
            {
                //ex
                //timeout 1000
                //repeat 20  期間內會執行1次
                //repeat 10    期間內會執行2次
                //repeat 1   期間內會執行20次
                int FailRepeatCount = FailRow.Count == 0 ? 0 : FailRow.Select(x => x.Field<int>("CmdRepeat")).Max();  //最大執行repeat的次數-1次 19

                int FailRepeatTime = FailRepeatCount == 0 ? 0 : (FailRepeatCount - 1) * timeout;//Fail 19  次後執行第20次
                foreach (var r in FailRow)
                {
                    FailDelay_Repeat += (FailRepeatCount / r.Field<int>("CmdRepeat")) * r.Field<int>("CmdDelay"); //執行次數*執行delay
                }
                FailDelay_Repeat += FailRepeatTime;//執行次數timeout時間+執行command時間

                int MaxRepeatTime = xpRepeat * timeout;

                foreach (var r in FailRow)
                {
                    MaxDelay += (xpRepeat / r.Field<int>("CmdRepeat")) * r.Field<int>("CmdDelay"); //執行次數*執行delay
                }

                MaxDelay += PassDelay + MaxRepeatTime;
            }
            else
            {
                PassDelay = (from c in CommandTable.AsEnumerable()
                             where c.Field<int>("CmdList") == 4
                             select c.Field<int>("CmdDelay")).Sum();
                FailDelay = (from c in CommandTable.AsEnumerable()
                             where c.Field<int>("CmdList") == 3
                             select c.Field<int>("CmdDelay")).Sum();
                MaxDelay = (PassDelay - FailDelay) > 0 ? PassDelay : FailDelay;
            }
        }

        private static void AutoImportINI(string currentItemNameType, out Dictionary<string,string> Sampling,out Dictionary<string, string> UpperSpec, out Dictionary<string, string> LowerSpec, out Dictionary<string, string> ItemName_DBIndex, string Org)
        {
            UpperSpec = new Dictionary<string, string>();
            LowerSpec = new Dictionary<string, string>();
            ItemName_DBIndex = new Dictionary<string, string>();
            Sampling = new Dictionary<string, string>();

            ITMXPServerQuery queryitmxp = new ITMXPServerQuery();
            //先得到相關於ItemNameType的file index
            string ITMXPQueryString = "select * from itmxp.testfile,itmxp.tbl_testinfo where itmxp.testfile.unikey = itmxp.tbl_testinfo.Idx_file and itmxp.tbl_testinfo.ItemNameType = '" + currentItemNameType + "' order by unikey desc";
            DataTable tempINITable = queryitmxp.QueryResult(ITMXPQueryString);
            string CurrentIdx_file = "0";
            for (int i = 0; i < tempINITable.Rows.Count; i++)
            {
                if ((tempINITable.Rows[i]["StationType"].ToString().Trim() != "5") && (tempINITable.Rows[i]["StationType"].ToString().Trim() != "9"))
                {
                    CurrentIdx_file = tempINITable.Rows[i]["Idx_file"].ToString();
                    break;
                }
            }

            //再查詢最新的Release或pilot run版本
            ITMXPQueryString = "Select * from itmxp.tbl_fileversion where itmxp.tbl_fileversion.Idx_file = '" + CurrentIdx_file + "' and itmxp.tbl_fileversion.IsReleased = '1' order by AutoVersion desc limit 1";
            string NeweastVersion = "0";
            tempINITable = queryitmxp.QueryResult(ITMXPQueryString);
            if (tempINITable.Rows.Count == 0)
            {
                ITMXPQueryString = "Select * from itmxp.tbl_fileversion where itmxp.tbl_fileversion.Idx_file = '" + CurrentIdx_file + "' order by AutoVersion desc limit 1";
                tempINITable = queryitmxp.QueryResult(ITMXPQueryString);
            }
            for (int i = 0; i < tempINITable.Rows.Count; i++)
            {
                NeweastVersion = tempINITable.Rows[i]["AutoVersion"].ToString();
            }
            ITMXPQueryString = @"select itmxp.testfile.XMLName ,itmxp.tbl_fileversion.AutoVersion ,itmxp.tbl_fileversion.IsReleased,itmxp.tbl_testitem.DBIndex, itmxp.tbl_testitem.Double1,itmxp.tbl_testitem.Double2,itmxp.tbl_testitem.Unit,itmxp.tbl_testitem.SamplingRate 
							   from itmxp.tbl_testitem inner join itmxp.tbl_fileversion on ( itmxp.tbl_testitem.Idx_Version = itmxp.tbl_fileversion.Idx_Version )
							   inner join itmxp.testfile on ( itmxp.tbl_fileversion.Idx_file = itmxp.testfile.unikey )
							   where itmxp.testfile.unikey = '" + CurrentIdx_file + @"' and itmxp.tbl_fileversion.AutoVersion='" + NeweastVersion + "'group by itmxp.tbl_testitem.DBIndex";
            tempINITable = queryitmxp.QueryResult(ITMXPQueryString);

            //將ini加到list裡面
            for (int i = 0; i < tempINITable.Rows.Count; i++)
            {
                UpperSpec.Add(tempINITable.Rows[i]["DBIndex"].ToString(), tempINITable.Rows[i]["Double1"].ToString());
                LowerSpec.Add(tempINITable.Rows[i]["DBIndex"].ToString(), tempINITable.Rows[i]["Double2"].ToString());
                Sampling.Add(tempINITable.Rows[i]["DBIndex"].ToString(), tempINITable.Rows[i]["SamplingRate"].ToString());
            }

            string ItemNameQueryString = "select * from  [ate_result].[dbo].[ItemName] where ItemNameType = '" + currentItemNameType + "'";

            DataTable ItemNameTable = DataHandlerFunctions.GetOrgTable(Org, ItemNameQueryString);
            for (int i = 1; i <= 150; i++)
            {
                ItemName_DBIndex.Add(i.ToString(), ItemNameTable.Rows[0]["Name" + i.ToString()].ToString());
            }
        }

        public static List<FailCorrelation> GetFailItemCorrelationByCategory(string type, string Org, int ItemNameType, string SDate, string Edate, int FailItem)
        {
            var typeName = new Dictionary<int, string>();

            var Output = new List<FailCorrelation>();

            Output = DataHandlerFunctions.GetGroupDataByScripts(type, Org, ItemNameType, SDate, Edate, FailItem);
            Output.ForEach(item =>
            {
                if (type == "Fixture")
                {
                    item.Stationid = item.Station == null ? item.Stationid : item.Station + "_" + item.Stationid;
                    item.Category1 = item.Stationid;
                }
                else
                {
                    if (type == "Station")
                        item.Category1 = item.Station;
                    if (type == "SerialNumber")
                        item.Category1 = item.SerialNumber;
                    if (type == "ExeInfo")
                        item.Category1 = item.ExeInfo;
                    if (type == "ProductName")
                        item.Category1 = item.ProductName;
                    if (type == "UserName")
                        item.Category1 = item.UserName;
                }
            });

            return Output;
        }

        public static List<PieChartData> GetCorrelationPieData(List<FailCorrelation> input, string type)
        {
            var Output = new List<PieChartData>();

            input.ForEach(item =>
            {
                if (type == "Station" && Output.Count <= 10)
                {
                    Output.Add(new PieChartData { category = item.Station, value = item.FailPercent });
                }
                if (type == "Fixture" && Output.Count <= 10)
                {
                    Output.Add(new PieChartData { category = item.Stationid, value = item.FailPercent });
                }
                if (type == "SerialNumber" && Output.Count <= 10)
                {
                    Output.Add(new PieChartData { category = item.SerialNumber, value = item.FailPercent });
                }
                if (type == "ExeInfo" && Output.Count <= 10)
                {
                    Output.Add(new PieChartData { category = item.ExeInfo, value = item.FailPercent });
                }
                if (type == "ProductName" && Output.Count <= 10)
                {
                    Output.Add(new PieChartData { category = item.ProductName, value = item.FailPercent });
                }
                if (type == "UserName" && Output.Count <= 10)
                {
                    Output.Add(new PieChartData { category = item.UserName, value = item.FailPercent });
                }
            });
            return Output;
        }

        public static List<CorrelationRawData> GetCorrRawDataByGroupID(string type, int Failitemnumber, List<FailCorrelation> CorrData, string Sdate, string Edate, int ItemNameType, string Org)
        {
            var GroupIDList = (from item in CorrData
                               select item.Category1).Distinct().ToList();
            var Output = new List<CorrelationRawData>();

            GroupIDList.ForEach(item =>
            {
                var _raw = DataHandlerFunctions.GetRawDataByCorrID(Failitemnumber, type, item, Sdate, Edate, ItemNameType, Org) ?? new List<double>();

                if (_raw.Count != 0)
                {
                    var raw = (from r in _raw
                               where r != -999
                               select r).ToList();
                    if (raw.Count != 0)
                        Output.Add(new CorrelationRawData { Category = item, Value = raw });
                }
            });

            return Output;
        }
    }

    public class FixtureRelation
    {
        public bool Relation { get; set; }
        public int TotalTest { get; set; }
        public int TotalFail { get; set; }
        public int dbindex { get; set; }
        public List<FixtureRelationByPort> PortInfo { get; set; }
    }

    public class FailItemCorrelation_Fixture
    {
        public int FailItem { get; set; }
        public string ItemName { get; set; }
        public string Correlation { get; set; }
        public List<FailCorrelation> FixtureList { get; set; }
    }

    public class FixtureRelationByPort
    {
        public int FixtureID { get; set; }
        public int FailCount { get; set; }
    }

    public class CorrelationRawData
    {
        [Display(Name = "Category")]
        public string Category { get; set; }

        [Display(Name = "Value")]
        public List<double> Value { set; get; }
    }

    public class PieChartData
    {
        public string category { get; set; }
        public double value { get; set; }
    }

    public class FailCorrelation
    {
        [Display(Name = "Category1")]
        public string Category1 { get; set; }

        [Display(Name = "Item")]
        public int Item { set; get; }

        [Display(Name = "Station ID")]
        public string Stationid { set; get; }

        [Display(Name = "Fixture")]
        public string Station { set; get; }

        [Display(Name = "Fail Count")]
        public int FailCount { set; get; }

        [Display(Name = "Fail Percent")]
        public double FailPercent { set; get; }

        [Display(Name = "TestCount")]
        public int TestCount { set; get; }

        [Display(Name = "Fail Rate")]
        public double FailRate { set; get; }

        [Display(Name = "SerialNumber")]
        public string SerialNumber { set; get; }

        [Display(Name = "ExeInfo")]
        public string ExeInfo { set; get; }

        [Display(Name = "Soft ware")]
        public string ProductName { set; get; }

        [Display(Name = "UserName")]
        public string UserName { set; get; }
    }

    public class FailItemTable
    {
        [Display(Name = "Order")]
        public int Order { set; get; }

        [Display(Name = "Item")]
        public int FailItem { set; get; }

        [Display(Name = "ItemNameType")]
        public int ItemNameType { set; get; }

        [Display(Name = "Description")]
        public string ItemName { set; get; }

        [Display(Name = "SpecMax")]
        public string SpecMax { get; set; }

        [Display(Name = "SpecMin")]
        public string SpecMin { get; set; }

        [Display(Name = "Fixture Relation")]
        public string FixtureRelation { get; set; }

        [Display(Name = "FailCount")]
        public int FailCount { set; get; }

        [Display(Name = "Fail Rate (%)")]
        public double FailRate { set; get; }

        [Display(Name = "ReTest Rate (%)")]
        public double ReTestRate { set; get; }

        [Display(Name = "Pass Rate (%)")]
        public double PassRate { set; get; }

        [Display(Name = "Fail Percent (%)")]
        public double FailPercent { get; set; }

        [Display(Name = "AccumulatePercent")]
        public double AccumulatePercent { set; get; }

        [Display(Name = "Avg Spare")]
        public double AvgSpare { get; set; }

        [Display(Name = "Std Spare")]
        public double StdSpare { get; set; }

        [Display(Name = "TotalCount")]
        public int TotalCount { set; get; }

        [Display(Name = "Source")]
        public string Source { get; set; }

        [Display(Name ="SamplingRate(%)")]
        public string SamplingRate { get; set; }

        //public List<FailCorrelation> C_Fixture { set; get; }

        //public List<FailCorrelation> C_Station { set; get; }

        //public List<FailCorrelation> C_ESN { set; get; }

        //public List<FailCorrelation> C_ExeInfo { set; get; }

        //public List<FailCorrelation> C_SoftWare { set; get; }

        //public List<FailCorrelation> C_UserName { set; get; }

        public FailItemTable()
        {
            Order = 0;
            FailItem = 0;
            ItemNameType = 0;
            ItemName = string.Empty;
            TotalCount = 0;
            FailCount = 0;
            FailRate = 0;
            ItemNameType = 0;
            AccumulatePercent = 0;
            StdSpare = 0;
            AvgSpare = 0;
            FixtureRelation = "UnKnown";
            Source = "UnKnown";
        }
    }

    public class ItemName
    {
        public Int16 ItemNameType { get; set; }

        public String Name1 { get; set; }

        public String Name2 { get; set; }

        public String Name3 { get; set; }

        public String Name4 { get; set; }

        public String Name5 { get; set; }

        public String Name6 { get; set; }

        public String Name7 { get; set; }

        public String Name8 { get; set; }

        public String Name9 { get; set; }

        public String Name10 { get; set; }

        public String Name11 { get; set; }

        public String Name12 { get; set; }

        public String Name13 { get; set; }

        public String Name14 { get; set; }

        public String Name15 { get; set; }

        public String Name16 { get; set; }

        public String Name17 { get; set; }

        public String Name18 { get; set; }

        public String Name19 { get; set; }

        public String Name20 { get; set; }

        public String Name21 { get; set; }

        public String Name22 { get; set; }

        public String Name23 { get; set; }

        public String Name24 { get; set; }

        public String Name25 { get; set; }

        public String Name26 { get; set; }

        public String Name27 { get; set; }

        public String Name28 { get; set; }

        public String Name29 { get; set; }

        public String Name30 { get; set; }

        public String Name31 { get; set; }

        public String Name32 { get; set; }

        public String Name33 { get; set; }

        public String Name34 { get; set; }

        public String Name35 { get; set; }

        public String Name36 { get; set; }

        public String Name37 { get; set; }

        public String Name38 { get; set; }

        public String Name39 { get; set; }

        public String Name40 { get; set; }

        public String Name41 { get; set; }

        public String Name42 { get; set; }

        public String Name43 { get; set; }

        public String Name44 { get; set; }

        public String Name45 { get; set; }

        public String Name46 { get; set; }

        public String Name47 { get; set; }

        public String Name48 { get; set; }

        public String Name49 { get; set; }

        public String Name50 { get; set; }

        public String Name51 { get; set; }

        public String Name52 { get; set; }

        public String Name53 { get; set; }

        public String Name54 { get; set; }

        public String Name55 { get; set; }

        public String Name56 { get; set; }

        public String Name57 { get; set; }

        public String Name58 { get; set; }

        public String Name59 { get; set; }

        public String Name60 { get; set; }

        public String Name61 { get; set; }

        public String Name62 { get; set; }

        public String Name63 { get; set; }

        public String Name64 { get; set; }

        public String Name65 { get; set; }

        public String Name66 { get; set; }

        public String Name67 { get; set; }

        public String Name68 { get; set; }

        public String Name69 { get; set; }

        public String Name70 { get; set; }

        public String Name71 { get; set; }

        public String Name72 { get; set; }

        public String Name73 { get; set; }

        public String Name74 { get; set; }

        public String Name75 { get; set; }

        public String Name76 { get; set; }

        public String Name77 { get; set; }

        public String Name78 { get; set; }

        public String Name79 { get; set; }

        public String Name80 { get; set; }

        public String Name81 { get; set; }

        public String Name82 { get; set; }

        public String Name83 { get; set; }

        public String Name84 { get; set; }

        public String Name85 { get; set; }

        public String Name86 { get; set; }

        public String Name87 { get; set; }

        public String Name88 { get; set; }

        public String Name89 { get; set; }

        public String Name90 { get; set; }

        public String Name91 { get; set; }

        public String Name92 { get; set; }

        public String Name93 { get; set; }

        public String Name94 { get; set; }

        public String Name95 { get; set; }

        public String Name96 { get; set; }

        public String Name97 { get; set; }

        public String Name98 { get; set; }

        public String Name99 { get; set; }

        public String Name100 { get; set; }

        public String Name101 { get; set; }

        public String Name102 { get; set; }

        public String Name103 { get; set; }

        public String Name104 { get; set; }

        public String Name105 { get; set; }

        public String Name106 { get; set; }

        public String Name107 { get; set; }

        public String Name108 { get; set; }

        public String Name109 { get; set; }

        public String Name110 { get; set; }

        public String Name111 { get; set; }

        public String Name112 { get; set; }

        public String Name113 { get; set; }

        public String Name114 { get; set; }

        public String Name115 { get; set; }

        public String Name116 { get; set; }

        public String Name117 { get; set; }

        public String Name118 { get; set; }

        public String Name119 { get; set; }

        public String Name120 { get; set; }

        public String Name121 { get; set; }

        public String Name122 { get; set; }

        public String Name123 { get; set; }

        public String Name124 { get; set; }

        public String Name125 { get; set; }

        public String Name126 { get; set; }

        public String Name127 { get; set; }

        public String Name128 { get; set; }

        public String Name129 { get; set; }

        public String Name130 { get; set; }

        public String Name131 { get; set; }

        public String Name132 { get; set; }

        public String Name133 { get; set; }

        public String Name134 { get; set; }

        public String Name135 { get; set; }

        public String Name136 { get; set; }

        public String Name137 { get; set; }

        public String Name138 { get; set; }

        public String Name139 { get; set; }

        public String Name140 { get; set; }

        public String Name141 { get; set; }

        public String Name142 { get; set; }

        public String Name143 { get; set; }

        public String Name144 { get; set; }

        public String Name145 { get; set; }

        public String Name146 { get; set; }

        public String Name147 { get; set; }

        public String Name148 { get; set; }

        public String Name149 { get; set; }

        public String Name150 { get; set; }

        public Int32 ORG_ID { get; set; }

        public String Station { get; set; }

        public String ItemDescription { get; set; }

        public String Name151 { get; set; }

        public String Name152 { get; set; }

        public String Name153 { get; set; }

        public String Name154 { get; set; }

        public String Name155 { get; set; }

        public String Name156 { get; set; }

        public String Name157 { get; set; }

        public String Name158 { get; set; }

        public String Name159 { get; set; }

        public String Name160 { get; set; }

        public String Name161 { get; set; }

        public String Name162 { get; set; }

        public String Name163 { get; set; }

        public String Name164 { get; set; }

        public String Name165 { get; set; }

        public String Name166 { get; set; }

        public String Name167 { get; set; }

        public String Name168 { get; set; }

        public String Name169 { get; set; }

        public String Name170 { get; set; }

        public String Name171 { get; set; }

        public String Name172 { get; set; }

        public String Name173 { get; set; }

        public String Name174 { get; set; }

        public String Name175 { get; set; }

        public String Name176 { get; set; }

        public String Name177 { get; set; }

        public String Name178 { get; set; }

        public String Name179 { get; set; }

        public String Name180 { get; set; }

        public String Name181 { get; set; }

        public String Name182 { get; set; }

        public String Name183 { get; set; }

        public String Name184 { get; set; }

        public String Name185 { get; set; }

        public String Name186 { get; set; }

        public String Name187 { get; set; }

        public String Name188 { get; set; }

        public String Name189 { get; set; }

        public String Name190 { get; set; }

        public String Name191 { get; set; }

        public String Name192 { get; set; }

        public String Name193 { get; set; }

        public String Name194 { get; set; }

        public String Name195 { get; set; }

        public String Name196 { get; set; }

        public String Name197 { get; set; }

        public String Name198 { get; set; }

        public String Name199 { get; set; }

        public String Name200 { get; set; }

        public String Name201 { get; set; }

        public String Name202 { get; set; }

        public String Name203 { get; set; }

        public String Name204 { get; set; }

        public String Name205 { get; set; }

        public String Name206 { get; set; }

        public String Name207 { get; set; }

        public String Name208 { get; set; }

        public String Name209 { get; set; }

        public String Name210 { get; set; }

        public String Name211 { get; set; }

        public String Name212 { get; set; }

        public String Name213 { get; set; }

        public String Name214 { get; set; }

        public String Name215 { get; set; }

        public String Name216 { get; set; }

        public String Name217 { get; set; }

        public String Name218 { get; set; }

        public String Name219 { get; set; }

        public String Name220 { get; set; }

        public String Name221 { get; set; }

        public String Name222 { get; set; }

        public String Name223 { get; set; }

        public String Name224 { get; set; }

        public String Name225 { get; set; }

        public String Name226 { get; set; }

        public String Name227 { get; set; }

        public String Name228 { get; set; }

        public String Name229 { get; set; }

        public String Name230 { get; set; }

        public String Name231 { get; set; }

        public String Name232 { get; set; }

        public String Name233 { get; set; }

        public String Name234 { get; set; }

        public String Name235 { get; set; }

        public String Name236 { get; set; }

        public String Name237 { get; set; }

        public String Name238 { get; set; }

        public String Name239 { get; set; }

        public String Name240 { get; set; }

        public String Name241 { get; set; }

        public String Name242 { get; set; }

        public String Name243 { get; set; }

        public String Name244 { get; set; }

        public String Name245 { get; set; }

        public String Name246 { get; set; }

        public String Name247 { get; set; }

        public String Name248 { get; set; }

        public String Name249 { get; set; }

        public String Name250 { get; set; }
    }
}