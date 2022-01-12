using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PTE_Web.Models
{
    public class ProductOwnerModel
    {

    }
    public class PTEProductOwner
    {
        public string Org { get; set; }
        public int ItemNameType { get; set; }
        public string Description { get; set; }       
        public string Owner { get; set; }
        public string Status { get; set; }
        public DateTime UpdateTime { get; set; }
        public int ProductionDays { get; set; }
        // Red: >100 : Yellow: 50 ->100 : Blue <50
        public string Priority { get; set; }

        public string PriorityString { get; set; }

        public string UserPicLink { get; set; }

        public string TipString { get; set; }
    }

    public class PTEProductOwnerIssue
    {
        public string Org { get; set; }
        public int ItemNameType { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Issue_Status { get; set; }
        public DateTime CreateDate { get; set; }
        public int Title_id { get; set; }
    }

    public class PTEWEB_Issues_Owner
    {
        public int uid { get; set; }
        public string Org { get; set; }
        public int Empid { get; set; }
        public string Ename { get; set; }
    }
}