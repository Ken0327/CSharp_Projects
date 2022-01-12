using PTEDailyReport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEDailyReport.Model
{
    class General
    {
        
    }

    public class OutlineInfo
    {
        public string Org { get; set; }
        public int TotalStation { get; set; }
        public int GoodStationCount { get; set; }
        public int BadStationCount { get; set; }
        public int LargeQStationRate { get; set; }
        public double GoodStationRate { get; set; }
        public double BadStationRate { get; set; }

    }

    class OutPutInfo
    {
        public string Org { get; set; }
        public List<PTEWEB_ItemNameType_ByDaily> AllStation { get; set; }
        public OutlineInfo OutLine { get; set; }
        public List<PTEWEB_ItemNameType_ByDaily> FocusStationRaw { get; set; }
        public List<PTEWEB_Issues_ByDaily> LatestIssueList { get; set; }
        public List<PTEWEB_ItemNameType_RealOutput_ByDaily> FocusUPHStationRow { get; set; }

    }

    class HtmlContent
    {
        public string Org { get; set; }
        public string Title { get; set; }
        public string ContentHtmlString { get; set; }

    }
}
