using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PTE_Web.Models
{
    public class TypeRawData_Correlation
    {
        public string typestring { get; set; }
        public double value { get; set; }
        public double spec_min { get; set; }
        public double spec_max { get; set; }
    }

    public class Hist_Chart_KeyValue
    {
        public string xid { get; set; }
        public int yvalue { get; set; }
    }

    public class CorrelationAddPageModel
    {
        public string ChartString { get; set; }
        public List<TypeRawData_Correlation> TableList { get; set; }
    }

    public class ItemRawData
    {
        public string Serialnumber { get; set; }
        public string tDatetime { get; set; }
        public string station { get; set; }
        public string stationid { get; set; }
        public string productname { get; set; }
        public string exeinfo { get; set; }
        public string NOHGPN { get; set; }
        public string username { get; set; }
        public string ItemResult { get; set; }
        public string ItemStatus { get; set; }

    }
}