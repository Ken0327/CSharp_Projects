using PTE_Web.Connections;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace PTE_Web.Models
{  
    public class DataBrowser
    {

        public static List<FailItemTable> GetAlltemTableByItemName_Org_DateRange(string Org, int ItemNameType, string Sdate, string Edate)
        {
            Dictionary<string, string> UpperSpec = new Dictionary<string, string>();
            Dictionary<string, string> LowerSpec = new Dictionary<string, string>();
            Dictionary<string, string> ItemName_DBIndex = new Dictionary<string, string>();
            DataHandlerFunctions.AutoImportINI(ItemNameType.ToString(), out UpperSpec, out LowerSpec, out ItemName_DBIndex, Org);
            var AllTable = new List<FailItemTable>();
            var FailTable = new List<FailItemTable>();

            //FailTable = DataHandlerFunctions.GetFailDetailInfo(ItemNameType, Org, Sdate, Edate) ?? new List<FailItemTable>();
            FailTable = DataHandlerFunctions.GetFailDetailInfo_SimplyPart_1(ItemNameType, Org, Sdate, Edate) ?? new List<FailItemTable>();
            AllTable = GetAllItem(ItemName_DBIndex);
            
            var ordercounter = 1;
            var CummlateFailCount = 0;
            var TotalFailCount = FailTable.Sum(x => x.FailCount);
            var Source = DataHandlerFunctions.GetDBByItemNameType(ItemNameType)=="TblCpu" ? "ATE" : "FT" ;

            var LatestDBItem = UpperSpec.Keys.ToList();

            AllTable = AllTable.Where(x => LatestDBItem.Contains(x.FailItem.ToString())).ToList();

            AllTable.ForEach(item =>
            {
                var faildetail = FailTable.Where(fail => fail.FailItem == item.FailItem).FirstOrDefault();

                if (faildetail!=null)
                {                    
                    item.Order = ordercounter;
                    CummlateFailCount += faildetail.FailCount;
                    item.FailCount = faildetail.FailCount;
                    item.AccumulatePercent = Math.Round((double)CummlateFailCount * 100 / TotalFailCount, 2);

                    item.FailPercent = Math.Round((double)faildetail.FailCount * 100 / TotalFailCount, 2);
                    item.FailRate = Math.Round(faildetail.FailRate * 100, 2);
                    item.PassRate = Math.Round((1- faildetail.FailRate) * 100, 2);
                    ordercounter++;
                }
                else
                {
                    item.Order = 0;
                    item.AccumulatePercent = 0;
                    item.FailPercent = 0;
                    item.FailRate = 0;
                    item.PassRate = 100;
                }

                item.SpecMax = UpperSpec.ContainsKey((item.FailItem).ToString()) ? Convert.ToString(UpperSpec[(item.FailItem).ToString()]) : "" ;
                item.SpecMin = UpperSpec.ContainsKey((item.FailItem).ToString()) ? Convert.ToString(LowerSpec[(item.FailItem).ToString()]) : "" ;
                item.TotalCount = FailTable.Count != 0 ? FailTable.First().TotalCount : 99999;
                item.Source = Source;
            });

            return AllTable;
        }

        private static List<FailItemTable> GetAllItem(Dictionary<string,string> ItemTable)
        {
            try
            {
                var Output = new List<FailItemTable>();

                for(int i =1; i<=250;i++)
                {
                    if(ItemTable[i.ToString()]!=null && ItemTable[i.ToString()]!="NULL" && ItemTable[i.ToString()]!=string.Empty)
                    {
                        Output.Add(new FailItemTable() { ItemName = ItemTable[i.ToString()] , FailItem = i });
                    }
                }

                return Output;
            }
            catch(Exception e)
            {
                return new List<FailItemTable>();
            }
        }

        public static List<TableRow> GetCpkInfoByItem_Org_DateRange()
        {
            try
            {
                var Output = new List<TableRow>();


                return Output;
            }
            catch(Exception e)
            {
                return new List<TableRow>();
            }
        }

    }
}