using System;

namespace PTEDailyReport.Models
{
    public class PTEWEB_ItemNameType_ByDaily
    {
        public Int32 Id { get; set; }

        public string IssueStatus { get; set; }

        public String Org { get; set; }

        public DateTime Date { get; set; }

        public Int32 ItemNameType { get; set; }

        public String TestStation { get; set; }

        public String Description { get; set; }

        public String TestType { get; set; }

        public String TestType2 { get; set; }

        public Int32 Total { get; set; }

        public Int32 Pass { get; set; }

        public Int32 Fail { get; set; }

        public Int32 D_Total { get; set; }

        public Int32 D_Pass { get; set; }

        public Int32 D_Fail { get; set; }

        public Double Pass_Rate { get; set; }

        public Double Fail_Rate { get; set; }

        public Double Retry_Rate { get; set; }

        public Double FYR { get; set; }

        public Double Avg_Pass_Time { get; set; }

        public Double Avg_Total_Time { get; set; }

        public String Source { get; set; }
    }
}