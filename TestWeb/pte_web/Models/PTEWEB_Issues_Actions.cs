using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PTE_Web.Models
{
    public class PTEWEB_Issues_Actions
    {
        [Display(Name = "Actionid")]
        public int Actionid { get; set; }

        [Display(Name = "Action")]
        public string Action { get; set; }
    }
}