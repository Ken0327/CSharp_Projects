using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PTE_Web.Models
{
    public class PTEWEB_Issues_ReplyCause
    {
        [Display(Name = "Causeid")]
        public int Causeid { get; set; }

        [Display(Name = "Cause")]
        public string Cause { get; set; }
    }
}