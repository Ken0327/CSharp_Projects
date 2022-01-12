using System;
using System.ComponentModel.DataAnnotations;

namespace PTE_Web.Models
{
    public class PTEWEB_ItemNameType_ByDaily
    {
        [Display(Name = "ID")]
        public Int32 id { get; set; }

        [Display(Name = "Org")]
        public String Org { get; set; }

        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Display(Name = "ItemNameType")]
        public Int32 ItemNameType { get; set; }

        [Display(Name = "Description")]
        public String Description { get; set; }

        [Display(Name = "TestType")]
        public String TestType { get; set; }

        [Display(Name = "TestType2")]
        public String TestType2 { get; set; }

        [Display(Name = "Total")]
        public Int32 Total { get; set; }
        [Display(Name = "Pass")]

        public Int32 Pass { get; set; }

        [Display(Name = "Fail")]
        public Int32 Fail { get; set; }

        [Display(Name = "D_Total")]
        public Int32 D_Total { get; set; }

        [Display(Name = "D_Pass")]
        public Int32 D_Pass { get; set; }

        [Display(Name = "D_Fail")]
        public Int32 D_Fail { get; set; }

        [Display(Name = "Pass_Rate (%)")]
        public Double Pass_Rate { get; set; }

        [Display(Name = "Fail_Rate (%)")]
        public Double Fail_Rate { get; set; }

        [Display(Name = "Retry_Rate (%)")]
        public Double Retry_Rate { get; set; }

        [Display(Name = "FYR (%)")]
        public Double FYR { get; set; }

        [Display(Name = "Avg_Pass_Time (s)")]
        public Double Avg_Pass_Time { get; set; }

        [Display(Name = "Avg_Total_Time (s)")]
        public Double Avg_Total_Time { get; set; }

        [Display(Name = "Source")]
        public String Source { get; set; }

        [Display(Name = "TestStation")]
        public String TestStation { get; set; }

        [Display(Name = "StationType")]
        public String StationType { get; set; }
    }
}