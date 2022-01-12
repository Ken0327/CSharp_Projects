using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PTE_Web.Models
{
    public class IssueContentModel
    {
        public string LinkImg { get; set; }

        public int Title_id { get; set; }
        public int ItemNameType { get; set; }
        public string FailItem { get; set; }
        public string Action { get; set; }
        public string Cause { get; set; }
        public string ActionCommon { get; set; }
        public string CauseCommon { get; set; }
        public string UserName { get; set; }
        public string Owner { get; set; }
        public string FileName { get; set; }
        public string CreateTime { get; set; }

        public string ENAME { get; set; }
    }

    public class IssueOutlie
    {
        public int OPEN { get; set; }

        public int CLOSE { get; set; }

    }
}