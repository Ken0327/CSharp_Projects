using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PTE_Web.Models
{
    public class PTEWEB_Issue_Replys
    {
        public List<PTEWEB_Issues_Title> Issues { get; set; }

        public List<PTEWEB_Issues_Reply> Contents { get; set; }
    }

    public class PTEWEB_Issues_Reply
    {
        [Display(Name = "Issue ID")]
        public int Title_id { get; set; }

        [Display(Name = "Editor")]
        public string UserName { get; set; }

        [Display(Name = "Owner")]
        public string Owner { get; set; }

        [Display(Name = "ActionComment")]
        public string ActionCommon { get; set; }

        [Display(Name = "CauseComment")]
        public string CauseCommon { get; set; }

        [Display(Name = "Attachment Link")]
        public string fileName { get; set; }

        [Display(Name = "CreationTime")]
        public string CreateTime { get; set; }

        [Display(Name = "ActionID")]
        public int Actionid { get; set; }

        [Display(Name = "Causeid")]
        public int Causeid { get; set; }

        [Display(Name = "FailItem")]
        public string FailItem { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Cause Type")]
        public string Cause { get; set; }

        [Display(Name = "Action  Type")]
        public string Action { get; set; }

        [Display(Name = "ItemNameType")]
        public int ItemNameType { get; set; }

        public bool IsThisIssue { get; set; }

        //public PTEWEB_Issues_Reply(string uid,string owner,string Common, string filename, string Createtime, int arctionid , int causeid, string failitem, string status)
        //{



        //}
    }
}