using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEDailyReport.Model
{
    class Issue
    {
    }
    class PTEWEB_Issues_ByDaily
    {
        public Int32 ItemNameType { get; set; }

        public String TestStation { get; set; }

        public String Description { get; set; }

        public String IssueTrigger { get; set; }

        public int Status { get; set; }

        public string Org { get; set; }

        public int Title_id { get; set; }
    }
}
