using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEDailyReport.Model
{
    class PTEWEB_Issues_Title
    {
        public int Title_id { get; set; }
        public int ItemNameType { get; set; }

        public string Title { get; set; }

        public bool Issue_Status { get; set; }

        public string CreateDate { get; set; }
    }
}
