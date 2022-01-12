using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PTE_Web.Models
{
    public class ReportModel
    {
        public string LinkImg { get; set; }
        public string ItemNameType { get; set; }

        public string Org { get; set; }
        public string Description { get; set; }

        public string F1 { get; set; }
        public string F2 { get; set; }
        public string F3 { get; set; }

        public string FYR_Old { get; set; }

        public string FYR_New { get; set; }

        public string Fail_Item_Action1 { get; set; }
        public string Fail_Item_Action2 { get; set; }
        public string Fail_Item_Action3 { get; set; }

        public string Fail_Rate_Old1 { get; set; }
        public string Fail_Rate_Old2 { get; set; }
        public string Fail_Rate_Old3 { get; set; }

        public string Fail_Rate_New1 { get; set; }
        public string Fail_Rate_New2 { get; set; }
        public string Fail_Rate_New3 { get; set; }

        public string LastDateString { get; set; }

        public string CurrentDateString { get; set; }

        public string LastUPH { get; set; }

        public string NextUPH { get; set; }
    }

    public class ExportReportModel
    {
        public string Org { get; set; }

        public string ItemNameType { get; set; }

        public string Description { get; set; }

        public string First_Yield_Rate_Old { get; set; }

        public string First_Yield_Rate_New { get; set; }

        public string UPH_Achievement_Rate_Old { get; set; }

        public string UPH_Achievement_Rate_New { get; set; }

        public string Fail_Rate_Old { get; set; }

        public string Fail_Rate_New { get; set; }

        public string Top_Fail_Item { get; set; }

        public string Top1_Fail_Item { get; set; }

        public string Top2_Fail_Item { get; set; }

        public string Top3_Fail_Item { get; set; }

        public string Action_FailItem { get; set; }

        public string Editor { get; set; }

        public string Owner { get; set; }

        public string Cause_Type { get; set; }

        public string Cause_Comment { get; set; }

        public string Action_Type { set; get; }

        public string Action_Comment { get; set; }

        public string AttachmentLink { get; set; }

        public string LastDateString { get; set; }

        public string CurrentDateString { get; set; }
    }
}