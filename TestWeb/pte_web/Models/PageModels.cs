using System.Collections.Generic;

namespace PTE_Web.Models
{
    //Please add every PageModels here.
    public class PageModels
    {
    }

    public class AnalyzePageModels
    {
        public IEnumerable<FailItemModel.ItemSpareData> ItemSapreDatas { get; set; }

        public IEnumerable<ItemNameTypeFYRTilteInfo> FYRTable { get; set; }
        public IEnumerable<ItemNameTypeDeltaInfo> DeltaTable { get; set; }
        public CpkModel CpkModel { get; set; }
        public IEnumerable<FailItemTable> FailItemModel { get; set; }

        public PTEWEB_Issue_Replys PTEWEB_Issues { get; set; }
    }

    public class SparePageModels
    {
        public IEnumerable<FailItemModel.ItemSpareData> ItemSapreDatas { get; set; }
    }

    public class FailItemCorrelationPageModels
    {
        public IEnumerable<FailCorrelation> ThisCorrelationsTable { set; get; }
    }
}