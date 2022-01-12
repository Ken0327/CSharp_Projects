using System.Collections.Generic;

namespace PTE_Web.Class
{
    public class MonthCpkData
    {
        public List<string> L_Date { get; set; }

        public List<double> CPKs { get; set; }

        public List<double> FailRates { get; set; }

        public string DataRange { get; set; }

        public int TotalCount { get; set; }

        public int FailCount { get; set; }

        public string Spec { get; set; }

        public string FailRate { get; set; }

        public double cpk { get; set; }
    }
}