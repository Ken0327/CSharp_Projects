using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PTE_Web.Models
{
    public class PTEWEB_ItemNameType_RealOutput_ByDaily
    {
        [Display(Name = "Org")]
        public String Org { get; set; }

        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Display(Name = "ItemNameType")]
        public Int32 ItemNameType { get; set; }

        [Display(Name = "ProductName")]
        public String ProductName { get; set; }

        [Display(Name = "RealOutput")]
        public Int32 RealOutput { get; set; }

        [Display(Name = "EstimateUPH")]
        public float EstimateUPH { get; set; }

        [Display(Name = "UPH")]
        public Int32 UPH { get; set; }

        [Display(Name = "AvgSpare")]
        public float AvgSpare { get; set; }

        [Display(Name = "table")]
        public String table { get; set; }

        [Display(Name = "Gap")]
        public float Gap { get; set; }

    }
}