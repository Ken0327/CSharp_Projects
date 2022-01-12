using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace PTE_Web.Models
{
    public class CpkModel
    {
        public List<TableRow> CpkTable { get; set; }

        //public string rawData
        //{
        //    get; set;
        //}

        //	public Dictionary<int, Tuple<string, string, double>> Items { get; set; }
    }

    public class TableRow
    {
        public string DBIndex { get; set; }
        public string ItemDescription { get; set; }
        public string MinSpec { get; set; }
        public string MaxSpec { get; set; }
        public string TestCount { get; set; }
        public double Cpk { get; set; }
        public double SD { get; set; }
        public double AVG { get; set; }
        public double dataMax { get; set; }
        public double dataMin { get; set; }
        public string DataRange { get; set; }
        public string Spec { get; set; }
        public int FailCount { get; set; }
        public string FailRate { get; set; }

        public string PassRate { get; set; }

        public string PassStr;

        public string FailStr;
    }
}