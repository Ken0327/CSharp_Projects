using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PTE_Web.Class
{
    public class DailyFYRInfo
    {
        public List<FailItem> _FailItemList = new List<FailItem>();

        public string JobNumber { get; set; }

        public double FYR { get; set; }

        public int Total { get; set; }

        public double Spare { get; set; }

        public List<DateTime> DateTimes { get; set; }

        public List<FailItem> FailItemList
        {
            get
            {
                return _FailItemList;
            }
        }
    }

    public class FailItem
    {
        public string ItemName { get; set; }
        public int FailCount { get; set; }
        public double Rate { get; set; }
    }
}