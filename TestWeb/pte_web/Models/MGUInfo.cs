using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace PTE_Web.Models
{
    public class MGUInfo
    {
    }

    public class ProductionData
    {
        public string ModelNumber { get; set; }//B297, BXXX
        public string SWIStep { get; set; }
        public string Process_EC { get; set; }
        public string Hardware_EC { get; set; }
        public string CustomerPartNumber { get; set; }
        public string SWVersion { get; set; }
        public string SWVersionFLASHING { get; set; }
        public string ProdVersion { get; set; }
        public string NAV { get; set; }
        public string Prefix_UID { get; set; }
        public string NavigationCountry { get; set; }//CHN, ece, jpn
        public string INTEL_ABL { get; set; }
        public string IOC_APPL { get; set; }
        public string IOC_BIOS_D3 { get; set; }
        public string IOC_BIOS_D1 { get; set; }
        public string NeddleBoardID { get; set; }
        public string TypeCode { get; set; }
        public string SWBuildNumber { get; set; }
        public string SshKeyIdentifier { get; set; }
        public string FlashDataro { get; set; }
        public string OperationCode { get; set; }
    }

    public class MGURawData
    {
        public string SerialNumber { get; set; }
        public string SNLabel { get; set; }
        public string TestStation { get; set; }
        public string Operation { get; set; }
        public string Position { get; set; }
        public string StepNumber { get; set; }
        public string StepDesctiption { get; set; }
        public string AbsoluteLowerLimit { get; set; }
        public string MeasurementValue { get; set; }
        public string AbsoluteUpperLimit { get; set; }
        public string MeasurementUnit { get; set; }
        public string Duration { get; set; }
        public string StepResult { get; set; }
        public string StepEndTime { get; set; }
        public string CreatDateTime { get; set; }
        public int ItemNameType { get; set; }
    }

    public class MGUResult
    {
        public string SerialNumber { get; set; }
        public string SNLabel { get; set; }
        public string TestStation { get; set; }
        public string Operation { get; set; }
        public string Position { get; set; }
        public string Duration { get; set; }
        public string StepResult { get; set; }
        public string CreatTime { get; set; }
        public int ItemNameType { get; set; }
        public string BTaddress { get; set; }
        public string WLANaddress { get; set; }
        public string LEOaddress { get; set; }
        public string Sprvladdress { get; set; }
        public string SWVersion { get; set; }
        public string ProdVersion { get; set; }
        public string Location { get; set; }
        public string ModelNumber { get; set; }
        public string MachineName { get; set; }
        public string PartNumber { get; set; }
        public int JobNumber { get; set; }
        public int OperatorID { get; set; }
        public string Prefix_UID { get; set; }
        public string SWIStep { get; set; }
        public string TestType { get; set; }
        public string TestType2 { get; set; }
        public string UID { get; set; }
        public string NAV { get; set; }
        public string Process_EC { get; set; }
        public string MainBoard { get; set; }
        public string Hardware_EC { get; set; }
        public int FinalIndex { get; set; }
        public string FailStepNumber { get; set; }
        public string StepDesctiption { get; set; }
        public string AbsoluteLowerLimit { get; set; }
        public string MeasurementValue { get; set; }
        public string AbsoluteUpperLimit { get; set; }
        public string INTEL_ABL { get; set; }
        public string IOC_APPL { get; set; }
        public string IOC_BIOS_D3 { get; set; }
        public string IOC_BIOS_D1 { get; set; }
        public string NavigationCountry { get; set; }
        public string SWBuildNumber { get; set; }
        public string NeddleBoardID { get; set; }
        public string TypeCode { get; set; }
        public string SshKeyIdentifier { get; set; }
        public string FlashDataro { get; set; }
        public string HDCPTransceiver { get; set; }
        public string HDCPReceiver { get; set; }
    }

    public class RxJobInfo
    {
        public string PartNumber { get; set; }
        public string PartDescription { get; set; }
        public string CLASS_CODE { get; set; }
        public int QTY { get; set; }
        public string RxOthers { get; set; }
    }

    public class SOTestPerformance
    {
        [Display(Name = "JobNumber")]
        public string JobNumber { get; set; }

        [Display(Name = "PartNumber")]
        public string PartNumber { get; set; }

        [Display(Name = "D_Total")]
        public Int32 D_Total { get; set; }

        [Display(Name = "Prod Version")]
        public string ProdVersion { get; set; }

        [Display(Name = "SoftWare Version")]
        public string SWVersion { get; set; }

        [Display(Name = "Order Date")]
        public string Order_Date { get; set; }
    }

    public class BasicTestPerformance
    {
        [Display(Name = "ItemNameType")]
        public Int32 ItemNameType { get; set; }

        [Display(Name = "JobNumber")]
        public Int32 JobNumber { get; set; }

        [Display(Name = "TestStation")]
        public string TestStation { get; set; }

        [Display(Name = "Total")]
        public Int32 Total { get; set; }

        [Display(Name = "D_Total")]
        public Int32 D_Total { get; set; }

        [Display(Name = "Pass")]
        public Int32 Pass { get; set; }

        [Display(Name = "D_Pass")]
        public Int32 D_Pass { get; set; }

        [Display(Name = "Fail")]
        public Int32 Fail { get; set; }

        [Display(Name = "D_Fail")]
        public Int32 D_Fail { get; set; }

        [Display(Name = "Avg_Pass_Time (s)")]
        public double Avg_Pass_Time { get; set; }

        [Display(Name = "Avg_Total_Time (s)")]
        public double Avg_Total_Time { get; set; }

        [Display(Name = "Pass_Rate (%)")]
        public Double Pass_Rate { get; set; }

        [Display(Name = "Fail_Rate (%)")]
        public Double Fail_Rate { get; set; }

        [Display(Name = "Retry_Rate (%)")]
        public Double Retry_Rate { get; set; }

        [Display(Name = "Final Yield Rate (%)")]
        public Double FYR { get; set; }

        [Display(Name = "First Pass Rate (%)")]
        public Double FPR { get; set; }

        [Display(Name = "UPH")]
        public int UPH { get; set; }
    }

    public class MGUPageDataTable
    {
        public List<SOTestPerformance> SOList { get; set; }
        public List<BasicTestPerformance> SingleDetailSOList { get; set; }
        public Dictionary<String, List<BasicTestPerformance>> AllDetailSODict { get; set; }
    }

    public class SimplyMGUChartInfo
    {
        public string TestStation { get; set; }

        public double Retry_Rate { get; set; }

        public double FYR { get; set; }

        public double Spare { get; set; }

        public double EstimateUPH { get; set; }

        public int Total { get; set; }

        public string href { get; set; }
    }

    public class DailyStationInfo
    {
        public List<StationDetailTestItemInfo> _FailItemList = new List<StationDetailTestItemInfo>();

        public string Station { get; set; }

        public double FYR { get; set; }

        public int Total { get; set; }

        public double Spare { get; set; }

        public List<MGUJobClass> DetailLinkList { get; set; }
    }

    public class MGUJobClass
    {
        public string TestStation { get; set; }

        public string JobNumber { get; set; }
        public int Total { get; set; }
    }

    public class StationDetailTestItemInfo
    {
        public int Order { get; set; }

        public string TestStation { get; set; }

        public string StepDesctiption { get; set; }

        public string FailStepNumber { get; set; }

        public string FixtureCorrelation { get; set; }

        public int FailCount { get; set; }

        public double FailRate { get; set; }

        public double FailPercent { get; set; }

        public double FailCummulative { get; set; }

        public StationDetailTestItemInfo()
        {
            FixtureCorrelation = "No";
        }
    }

    public class StationPanelStatus
    {
        public string Station { get; set; }
        public string Status { get; set; }
    }

    public class StationCorrelationTable
    {
        public string MachineName { get; set; }
        public string Position { get; set; }
        public int FailCount { get; set; }
        public int TestCount { get; set; }
        public double FailPercent { get; set; }
    }

    public class DailyMGUTrend
    {
        public string date { get; set; }
        public double FCT_FPR { get; set; }
        public double FLASHING_FPR { get; set; }
        public double RUNIN_FPR { get; set; }
        public double AFT_FPR { get; set; }
        public double SFT_FPR { get; set; }
        public int Total { get; set; }
    }
}