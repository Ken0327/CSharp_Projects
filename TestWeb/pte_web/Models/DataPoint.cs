using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace PTE_Web.Models
{  
    public class DataPoint
    {
        public string Fixture { get; set; }
        public int[] FailCount { get; set; }

        public string[] Fixture_ID { get; set; }
    }
}