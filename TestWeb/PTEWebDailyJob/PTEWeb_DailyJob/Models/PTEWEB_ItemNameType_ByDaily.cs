//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace PTEWeb_DailyJob.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class PTEWEB_ItemNameType_ByDaily
    {
        public int id { get; set; }
        public string Org { get; set; }
        public System.DateTime Date { get; set; }
        public int ItemNameType { get; set; }
        public string Description { get; set; }
        public string TestType { get; set; }
        public string TestType2 { get; set; }
        public int Total { get; set; }
        public int Pass { get; set; }
        public int Fail { get; set; }
        public int D_Total { get; set; }
        public int D_Pass { get; set; }
        public int D_Fail { get; set; }
        public double Pass_Rate { get; set; }
        public double Fail_Rate { get; set; }
        public double Retry_Rate { get; set; }
        public double FYR { get; set; }
        public double Avg_Pass_Time { get; set; }
        public double Avg_Total_Time { get; set; }
        public string Source { get; set; }
        public string TestStation { get; set; }
        public string StationType { get; set; }
    }
}
