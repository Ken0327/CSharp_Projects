using System;
using System.ComponentModel.DataAnnotations;

namespace PTE_Web.Models
{
    public class PTEWEB_ItemNameType_ByDaily_TOP10_FailItem
    {
        [Display(Name = "id")]
        public Int32 id { get; set; }

        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Display(Name = "ItemNameType")]
        public Int32 ItemNameType { get; set; }

        [Display(Name = "Description")]
        public String Description { get; set; }

        [Display(Name = "Total_Fail_Count")]
        public Int32 Total_Fail_Count { get; set; }

        [Display(Name = "No1_Fail_Item")]
        public String No1_Fail_Item { get; set; }

        [Display(Name = "No1_Fail_Count")]
        public Int32 No1_Fail_Count { get; set; }

        [Display(Name = "No1_Fail_Rate")]
        public Double No1_Fail_Rate { get; set; }

        [Display(Name = "No2_Fail_Item")]
        public String No2_Fail_Item { get; set; }

        [Display(Name = "No2_Fail_Count")]
        public Int32 No2_Fail_Count { get; set; }

        [Display(Name = "No2_Fail_Rate")]
        public Double No2_Fail_Rate { get; set; }

        [Display(Name = "No3_Fail_Item")]
        public String No3_Fail_Item { get; set; }

        [Display(Name = "No3_Fail_Count")]
        public Int32 No3_Fail_Count { get; set; }

        [Display(Name = "No3_Fail_Rate")]
        public Double No3_Fail_Rate { get; set; }

        [Display(Name = "No4_Fail_Item")]
        public String No4_Fail_Item { get; set; }

        [Display(Name = "No4_Fail_Count")]
        public Int32 No4_Fail_Count { get; set; }

        [Display(Name = "No4_Fail_Rate")]
        public Double No4_Fail_Rate { get; set; }

        [Display(Name = "No5_Fail_Item")]
        public String No5_Fail_Item { get; set; }

        [Display(Name = "No5_Fail_Count")]
        public Int32 No5_Fail_Count { get; set; }

        [Display(Name = "No5_Fail_Rate")]
        public Double No5_Fail_Rate { get; set; }

        [Display(Name = "No6_Fail_Item")]
        public String No6_Fail_Item { get; set; }

        [Display(Name = "No6_Fail_Count")]
        public Int32 No6_Fail_Count { get; set; }

        [Display(Name = "No6_Fail_Rate")]
        public Double No6_Fail_Rate { get; set; }

        [Display(Name = "No7_Fail_Item")]
        public String No7_Fail_Item { get; set; }

        [Display(Name = "No7_Fail_Count")]
        public Int32 No7_Fail_Count { get; set; }

        [Display(Name = "No7_Fail_Rate")]
        public Double No7_Fail_Rate { get; set; }

        [Display(Name = "No8_Fail_Item")]
        public String No8_Fail_Item { get; set; }

        [Display(Name = "No8_Fail_Count")]
        public Int32 No8_Fail_Count { get; set; }

        [Display(Name = "No8_Fail_Rate")]
        public Double No8_Fail_Rate { get; set; }

        [Display(Name = "No9_Fail_Item")]
        public String No9_Fail_Item { get; set; }

        [Display(Name = "No9_Fail_Count")]
        public Int32 No9_Fail_Count { get; set; }

        [Display(Name = "No9_Fail_Rate")]
        public Double No9_Fail_Rate { get; set; }

        [Display(Name = "No10_Fail_Item")]
        public String No10_Fail_Item { get; set; }

        [Display(Name = "No10_Fail_Count")]
        public Int32 No10_Fail_Count { get; set; }

        [Display(Name = "No10_Fail_Rate")]
        public Double No10_Fail_Rate { get; set; }

        [Display(Name = "Org")]
        public String Org { get; set; }
    }
}