using System;
using System.Collections.Generic;
using System.Text;

namespace DailyUPHJob
{
    public class PTEWEB_ItemNameType_RealOutput
    {
        public int ItemNameType { get; set; }
        public string Org { get; set; }
        public int TimeIndex { get; set; }
        public string table { get; set; }
        public string productname { get; set; }
        public string TestType { get; set; }
        public string TestType2 { get; set; }
        public string gpn { get; set; }
        public int UPH { get; set; }
        public int RealOutput { get; set; }
        public float EstimateHours { get; set; }
        public float EstimateUPH { get; set; }
        public float AvgSpare { get; set; }
        public int shiftid { get; set; }
        public DateTime Date { get; set; }
    }

    public class PTEWEB_ItemNameType_RealOutput_ByDaily
    {
        public DateTime Date { get; set; }
        public int ItemNameType { get; set; }
        public string Org { get; set; }
        public string table { get; set; }
        public string ProductName { get; set; }
        public int UPH { get; set; }
        public int RealOutput { get; set; }
        public float EstimateUPH { get; set; }
        public float AvgSpare { get; set; }
        public float Gap { get; set; }
    }
}