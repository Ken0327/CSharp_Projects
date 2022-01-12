namespace PTE_Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class PTEWEB_Issues_ByDaily
    {
        public int ItemNameType { get; set; }

        public int replyCount { get; set; }

        public int IssueAlive_Dates { get; set; }

        public bool Status { get; set; }

        private string _Status_Pic;

        public string Status_Pic
        {
            get
            {
                if (IssueCount > 0)
                {
                    return Status ? "../Content/fancyGrid/images/open.png" : "../Content/fancyGrid/images/close.png";
                }
                else
                {
                    return "Pedding";
                }
            }
        }

        public string LinkImg { get; set; }
        public string Support_Org { get; set; }

        public string LastUpdateDate { get; set; }

        public DateTime FYR_Time { get; set; }

        public DateTime TwoWeekTime { get; set; }

        public string Title_id_3Day { get; set; }

        public string Title_id_14Day { get; set; }

        public string Title_id_Custom { get; set; }

        public string Description { get; set; }

        public int CauseIssueID { get; set; }

        public int IssueCount { get; set; }
    }
}