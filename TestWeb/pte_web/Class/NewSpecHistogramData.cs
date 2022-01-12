using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PTE_Web.Class
{
    public class NewSpecHistogramData
    {
        public List<double> PassDatas { get; set; }

        public List<double> FailDatas { get; set; }

        public double AVG { get; set; }

        public double STD { get; set; }

        public double lsl { get; set; }

        public double usl { get; set; }
    }
}