using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PTE_Web.Models
{
    public class DashBoardGroundModel
    {
        protected string Daily_FYR_Output_Group1 { get; set; }

        protected string Daily_Item_Fail_Spare_Group2 { get; set; }

        protected string Daily_Basic_Item_All_Group_1_2 { get; set; }

        protected string Daily_FYR_Output_Issue_Group_1_3 { get; set; }

        public Dictionary<string, string> DataBaseGroupScriptDict = new Dictionary<string, string>();


        public string ProcessScript(string _script,string databasegroup)
        {
             return _script.Replace(databasegroup, DataBaseGroupScriptDict[databasegroup]);
        }
        public DashBoardGroundModel(string sdate,string edate)
        {
            try
            {
                var _Daily_FYR_Output_Group1 = $@"(select fyr_table.Org,fyr_table.ItemNameType,fyr_table.Date,fyr_table.Description,TestType,TestType2,Total,Pass,Fail,D_Total,D_Fail,Pass_Rate,Fail_Rate,Retry_Rate,FYR,Avg_Pass_Time,Avg_Total_Time,EstimateUPH,UPH,Gap
                                from [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily]as fyr_table 
                                inner join
                                (SELECT * FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_RealOutput_ByDaily] where Date between '{sdate}' and '{edate}') as uph_table
                                on uph_table.Date = fyr_table.Date and uph_table.ItemNameType = fyr_table.ItemNameType  AND uph_table.Org = fyr_table.Org
                                where fyr_table.Date between '{sdate}' and '{edate}') ";

                Daily_FYR_Output_Group1 = _Daily_FYR_Output_Group1;

                var _Daily_Item_Fail_Spare_Group2 = $@"(select  FailTop10.Org,FailTop10.Date,FailTop10.ItemNameType,FailTop10.Description,FailTop10.Total_Fail_Count,FailTop10.No1_Fail_Item,FailTop10.No1_Fail_Count,No2_Fail_Rate,No3_Fail_Item,No3_Fail_Count,No3_Fail_Rate,No4_Fail_Item,No4_Fail_Count,No4_Fail_Rate,No5_Fail_Item,No5_Fail_Count,No5_Fail_Rate,SpareTop10.NO1_Item as No1SpareItem,SpareTop10.NO1_CycleTime,SpareTop10.NO1_Percent,SpareTop10.NO2_Item as No2SpareItem ,SpareTop10.NO2_CycleTime,SpareTop10.NO2_Percent,SpareTop10.NO3_Item as No3SpareItem ,SpareTop10.NO3_CycleTime,SpareTop10.NO3_Percent,SpareTop10.NO4_Item as No4SpareItem ,SpareTop10.NO4_CycleTime,SpareTop10.NO4_Percent,SpareTop10.NO5_Item as No5SpareItme,SpareTop10.NO5_CycleTime,SpareTop10.NO5_Percent
                                    FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily_TOP10_FailItem] as FailTop10
                                    inner join
                                    (SELECT * from [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily_TOP10_LongCycleTime] where Date between '{sdate}' and '{edate}' ) as SpareTop10 
                                    on SpareTop10.Date = FailTop10.Date and SpareTop10.ItemNameType = FailTop10.ItemNameType  AND SpareTop10.Org = FailTop10.Org
                                    where SpareTop10.Date between'{sdate}' and '{edate}')";

                Daily_Item_Fail_Spare_Group2 = _Daily_Item_Fail_Spare_Group2;

                var _Daily_Basic_Item_All_Group_1_2 = $@"(select Group1Table.Org,Group1Table.ItemNameType,Group1Table.Date,Group1Table.Description,Group1Table.TestType,Group1Table.TestType2,Group1Table.Total,Group1Table.Pass,Group1Table.Fail,Group1Table.D_Total,Group1Table.D_Fail,Group1Table.Pass_Rate,Group1Table.Fail_Rate,Group1Table.Retry_Rate,Group1Table.FYR,Group1Table.Avg_Pass_Time,Group1Table.Avg_Total_Time,Group1Table.EstimateUPH,Group1Table.UPH,Group1Table.Gap,Group2Table.No1_Fail_Item,Group2Table.No1_Fail_Count,Group2Table.No1_Fail_Rate,Group2Table.No2_Fail_Item,Group2Table.No2_Fail_Count,Group2Table.No2_Fail_Rate,Group2Table.No3_Fail_Item,Group2Table.No3_Fail_Count,Group2Table.No3_Fail_Rate,Group2Table.No4_Fail_Item,Group2Table.No4_Fail_Count,Group2Table.No4_Fail_Rate,Group2Table.No5_Fail_Item,Group2Table.No5_Fail_Count,Group2Table.No5_Fail_Rate,Group2Table.No1SpareItem,Group2Table.NO1_CycleTime,Group2Table.NO1_Percent,Group2Table.No2SpareItem,Group2Table.NO2_CycleTime,Group2Table.NO2_Percent,Group2Table.No3SpareItem,Group2Table.NO3_CycleTime,Group2Table.NO3_Percent,Group2Table.No4SpareItem,Group2Table.NO4_CycleTime,Group2Table.NO4_Percent,Group2Table.No5SpareItme,Group2Table.NO5_CycleTime,Group2Table.NO5_Percent
                                                    from
                                                    (select  fyr_table.Org,fyr_table.ItemNameType,fyr_table.Date,fyr_table.Description,TestType,TestType2,Total,Pass,Fail,D_Total,D_Fail,Pass_Rate,Fail_Rate,Retry_Rate,FYR,Avg_Pass_Time,Avg_Total_Time,EstimateUPH,UPH,Gap
                                                    from [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily]as fyr_table 
                                                    inner join
                                                    (SELECT * FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_RealOutput_ByDaily] where Date between '{sdate}' and '{edate}' ) as uph_table
                                                    on uph_table.Date = fyr_table.Date and uph_table.ItemNameType = fyr_table.ItemNameType  AND uph_table.Org = fyr_table.Org
                                                    where fyr_table.Date between '{sdate}' and '{edate}') as Group1Table
                                                    inner join
                                                    (select  FailTop10.Org,FailTop10.Date,FailTop10.ItemNameType,FailTop10.Description,FailTop10.Total_Fail_Count,FailTop10.No1_Fail_Item,FailTop10.No1_Fail_Count,No1_Fail_Rate,No2_Fail_Item,No2_Fail_Count,No2_Fail_Rate,No3_Fail_Item,No3_Fail_Count,No3_Fail_Rate,No4_Fail_Item,No4_Fail_Count,No4_Fail_Rate,No5_Fail_Item,No5_Fail_Count,No5_Fail_Rate,SpareTop10.NO1_Item as No1SpareItem,SpareTop10.NO1_CycleTime,SpareTop10.NO1_Percent,SpareTop10.NO2_Item as No2SpareItem ,SpareTop10.NO2_CycleTime,SpareTop10.NO2_Percent,SpareTop10.NO3_Item as No3SpareItem ,SpareTop10.NO3_CycleTime,SpareTop10.NO3_Percent,SpareTop10.NO4_Item as No4SpareItem ,SpareTop10.NO4_CycleTime,SpareTop10.NO4_Percent,SpareTop10.NO5_Item as No5SpareItme,SpareTop10.NO5_CycleTime,SpareTop10.NO5_Percent
                                                    FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily_TOP10_FailItem] as FailTop10
                                                    inner join
                                                    (SELECT * from [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily_TOP10_LongCycleTime] where Date between '{sdate}' and '{edate}' ) as SpareTop10 
                                                    on SpareTop10.Date = FailTop10.Date and SpareTop10.ItemNameType = FailTop10.ItemNameType  AND SpareTop10.Org = FailTop10.Org
                                                    where SpareTop10.Date between '{sdate}' and '{edate}' ) as Group2Table
                                                    on Group1Table.ItemNameType = Group2Table.ItemNameType and Group1Table.Org = Group1Table.Org and Group1Table.Date = Group2Table.Date)";
                Daily_Basic_Item_All_Group_1_2 = _Daily_Basic_Item_All_Group_1_2;

                DataBaseGroupScriptDict["#Daily_FYR_Output_Group1"] = _Daily_FYR_Output_Group1;

                DataBaseGroupScriptDict["#Daily_Item_Fail_Spare_Group2"] = _Daily_Item_Fail_Spare_Group2;

                DataBaseGroupScriptDict["#Daily_Basic_Item_All_Group_1_2"] = _Daily_Basic_Item_All_Group_1_2;

            }
            catch(Exception e)
            {
                
            }
            
        }

    }

    public class PTEWebTable
    {
        public PTEWEB_ItemNameType_ByDaily FYRDaily { get; set; }
        public PTEWEB_ItemNameType_RealOutput_ByDaily RealOutputTable { get; set; }
        public PTEWEB_ItemNameType_ByDaily_TOP10_FailItem T10_FailTable { get; set; }
        public PTEWEB_ItemNameType_ByDaily_TOP10_LongCycleTime T10_LongSpareTable { get; set; }
    }


}