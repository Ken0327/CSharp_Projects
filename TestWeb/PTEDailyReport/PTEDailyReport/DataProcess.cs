using Dapper;
using NLog;
using PTEDailyReport.Model;
using PTEDailyReport.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PTEDailyReport
{
    internal class DataProcess
    {
        private static readonly Logger _PTElogger = LogManager.GetLogger("PTE_ReportLogger");

        private static readonly MassProductionDefiniton MassProductionConfig = new MassProductionDefiniton();

        public static List<OutPutInfo> GetDataResult(List<string> OrgList, string ThisDay)
        {
            var Output = new List<OutPutInfo>();

            OrgList.ForEach(org =>
            {
                var LatestIssueList = new List<PTEWEB_Issues_ByDaily>();

                var AllData_FYR = GetDailyFYR_FR_RTRByItemNameTypeAndOrg(ThisDay, org);
                var AllFocusUPHData = GetUPHDataResult(ThisDay, org);
                var Outline = new OutlineInfo()
                {
                    Org = org,
                    TotalStation = AllData_FYR.Count,
                    LargeQStationRate = AllData_FYR.FindAll(item => item.Total > MassProductionConfig.ProductionCounts).ToList().Count,
                    GoodStationCount = AllData_FYR.FindAll(item => item.FYR >= MassProductionConfig.FYR && item.Total > MassProductionConfig.ProductionCounts).ToList().Count,
                    BadStationCount = AllData_FYR.FindAll(item => (item.FYR < MassProductionConfig.FYR && item.Total > MassProductionConfig.ProductionCounts) || (item.Retry_Rate > MassProductionConfig.RetryRate && item.Total > MassProductionConfig.ProductionCounts)).ToList().Count,
                    GoodStationRate = (double)AllData_FYR.FindAll(item => item.FYR >= MassProductionConfig.FYR && item.Total > MassProductionConfig.ProductionCounts).ToList().Count / (double)AllData_FYR.Count,
                    BadStationRate = (double)AllData_FYR.FindAll(item => (item.FYR < MassProductionConfig.FYR && item.Total > MassProductionConfig.ProductionCounts) || (item.Retry_Rate > MassProductionConfig.RetryRate && item.Total > MassProductionConfig.ProductionCounts)).ToList().Count / AllData_FYR.Count,
                };

                var FocusList_FYR = AllData_FYR.FindAll(item => (item.FYR < MassProductionConfig.FYR && item.Total > MassProductionConfig.ProductionCounts) || (item.Retry_Rate >MassProductionConfig.RetryRate && item.Total > MassProductionConfig.ProductionCounts)).OrderBy(x => x.FYR).ToList();
                GetLatestIssueItemNameTypeList(ThisDay).ForEach(item =>
                {
                    var DetailInfo = AllData_FYR.FindAll(raw => raw.ItemNameType == item.ItemNameType && raw.Org == org).FirstOrDefault();
                    var IssueTrigger = GetIssueTriggerString(ThisDay, item.ItemNameType);
                    if (IssueTrigger != null && DetailInfo != null)
                        LatestIssueList.Add(new PTEWEB_Issues_ByDaily() { Org = org, Description = DetailInfo.Description, ItemNameType = item.ItemNameType, Status = item.Status, TestStation = DetailInfo.TestStation, IssueTrigger = IssueTrigger.Title, Title_id = IssueTrigger.Title_id });
                });

                FocusList_FYR.ForEach(item =>
                {
                    item.IssueStatus = IssueTypeTransform(GetIssueStatus(ThisDay, item.ItemNameType, item.Org));
                });
                AllFocusUPHData.ForEach(item =>
                {
                    item.Issue_Status = IssueTypeTransform(GetIssueStatus(ThisDay, item.ItemNameType, item.Org));
                });

                Output.Add(new OutPutInfo { Org = org, AllStation = AllData_FYR, OutLine = Outline, FocusStationRaw = FocusList_FYR, LatestIssueList = LatestIssueList, FocusUPHStationRow = AllFocusUPHData });
            });

            return Output;
        }

        public static List<PTEWEB_ItemNameType_RealOutput_ByDaily> GetUPHDataResult(string thisday, string Org)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    return db.Query<PTEWEB_ItemNameType_RealOutput_ByDaily>($@" SELECT Org,[ItemNameType],[ProductName],[RealOutput],[EstimateUPH],[UPH],(EstimateUPH / UPH) as UPHAchievement ,[AvgSpare],[Gap]  FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_RealOutput_ByDaily]  where date ='{thisday}' and Org ='{Org}' and uph != 999 AND (EstimateUPH / UPH) < 0.8 and RealOutput > {MassProductionConfig.ProductionCounts_UPH}").ToList() ?? new List<PTEWEB_ItemNameType_RealOutput_ByDaily>();
                }
            }
            catch (Exception e)
            {
                return new List<PTEWEB_ItemNameType_RealOutput_ByDaily>();
            }
        }

        private static PTEWEB_Issues_Title GetIssueTriggerString(string thisday, int itemnametype)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    var output = db.Query<PTEWEB_Issues_Title>($@" SELECT top(1) Title_id,Title  FROM [PTEDB].[dbo].[PTEWEB_Issues_Title]  where  ItemNameType ={itemnametype} and CreateDate ='{thisday}' ").First() ?? new PTEWEB_Issues_Title();
                    return output;
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return new PTEWEB_Issues_Title();
            }
        }

        private static List<PTEWEB_ItemNameType_ByDaily> GetDailyFYR_FR_RTRByItemNameTypeAndOrg(string thisday, string Org)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    return db.Query<PTEWEB_ItemNameType_ByDaily>($@"SELECT *FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily] where Org ='{Org}'  and Date = '{thisday}' AND Description NOT LIKE'%IQC%'
                                                                AND Description NOT LIKE '%RearCase%' AND Description NOT LIKE '%SNR%' AND Description NOT LIKE '%CALI%' order by itemnametype asc ").ToList();
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return null;
            }
        }

        private static List<PTEWEB_ItemNameType_ByDaily_TOP10_FailItem> GetTop10FailItemByDate_Itemnametype(string thisday, string Org, int ItemNameType)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    return db.Query<PTEWEB_ItemNameType_ByDaily_TOP10_FailItem>($@"SELECT *
                      FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily_TOP10_FailItem]
                      where ItemNameType ='{ItemNameType}'  and Date = '{thisday}' and  org = '{Org}'  order by date desc").ToList();
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return null;
            }
        }

        private static PTEWEB_Issues_ByDaily GetIssueStatus(string ThisDay, int Itemnametype, string Org)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    return db.Query<PTEWEB_Issues_ByDaily>($@"SELECT IssueAlive_Dates, Status  FROM [PTEDB].[dbo].[PTEWEB_Issues_ByDaily]  WHERE ItemNameType = '{Itemnametype}' and Support_Org='{Org}' and LastUpdateDate ='{ThisDay}'  ORDER BY LastUpdateDate DESC").FirstOrDefault()
                                ?? new PTEWEB_Issues_ByDaily()
                                {
                                    ItemNameType = Itemnametype,
                                    Org = Org,
                                    Status = 0
                                };
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return null;
            }
        }

        private static string IssueTypeTransform(PTEWEB_Issues_ByDaily status)
        {
            try
            {
                if (status.Status == 1)
                    return "Open";

                return "No Issue";
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return "No Issue";
            }
        }

        private static List<PTEWEB_Issues_ByDaily> GetLatestIssueItemNameTypeList(string ThisDate)
        {
            try
            {
                using (var db = ConnectionFactory.CreatConnection())
                {
                    return db.Query<PTEWEB_Issues_ByDaily>($@"SELECT ItemNameType,Issue_Status AS Status  FROM [PTEDB].[dbo].[PTEWEB_Issues_Title]  WHERE Issue_Status=1 AND CreateDate ='{ThisDate}' ").ToList();
                }
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return new List<PTEWEB_Issues_ByDaily>();
            }
        }

        public static List<MaillReciver> GetMailListFromServer()
        {
            try
            {
                var MailList = new List<MaillReciver>();
                var Resource = new List<MailList>();
                using (var db = ConnectionFactory.CreatConnection())
                {
                    Resource = db.Query<MailList>($@"SELECT * FROM [PTEDB].[dbo].[PTEWEB_Issues_Members]").ToList();
                }
                Resource.ForEach(OrgList =>
                {
                    var list = OrgList.Member_MailList.Split(';');
                    foreach (var man in list)
                        MailList.Add(new MaillReciver() { Org = OrgList.Org, Email = man, Name = man.Split('@').First() });
                });
                return MailList;
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return new List<MaillReciver>();
            }
        }
    }

    internal class HTMLContentProcess
    {
        private static readonly string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        private static readonly Logger _PTElogger = LogManager.GetLogger("PTE_ReportLogger");

        private static readonly MassProductionDefiniton MassProductionConfig = new MassProductionDefiniton();

        public static List<HtmlContent> GetHtmlStringFromData(List<OutPutInfo> DataInput, string ThisDay)
        {
            //test
            var columns = new string[] { "id", "Org", "Date", "ItemNameType", "TestStation", "Description", "TestType", "TestType2", "Total", "Pass", "Fail", "D_Total", "D_Pass", "D_Fail", "Pass_Rate", "Fail_Rate", "Retry_Rate", "FYR", "Avg_Pass_Time", "FYR", "Avg_Total_Time", "Source" };
            var background_img = path + $@"\background.gif";
            var logo_img = path + $@"\hd-logo.jpg";
            //test
            try
            {
                var Output = new List<HtmlContent>();

                DataInput.ForEach(item =>
                {
                    var Content = $@"<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">";
                    Content += $@"<body background=""{background_img}"">";
                    Content += $@" <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" valign=""top"">
                                    <table width=""100%"" bgcolor=""black"" valign=""top"">
                                        <!-- vertical spacer -->
                                        <tbody>
                                            <tr>
                                                <td style=""line-height:0px;"" align=""left""><img style=""display:block"" src=""{logo_img}""></td>
                                                <td width=""175"" align=""left"">
                                                    <p style=""margin: 0pt 0pt 0pt 25px; padding: 0px 0pt 0pt; color: rgb(255, 255, 255); font-size: 14px; font-weight: normal; font-family:arial; line-height: 26px; position: relative; z-index: 10;"">powered by</p>
                                                    <p style=""margin: 0pt 0pt 0pt 25px; padding: 0px 0pt 0pt; color: rgb(255, 255, 255); font-size: 26px; font-weight: normal; font-family:arial; line-height: 26px; position: relative; z-index: 9;"">PTE Team</p>
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>";
                    Content += AddBiWeekReportLink(IssueTitleDateStringGenerator());
                    Content += ProcessContentPart1(item, ThisDay);
                    if (item.LatestIssueList.Count != 0)
                        Content += ProcessContentPart2(item, ThisDay);
                    Content += ProcessContentPart3_FYR(item);

                    Content += ProcessContentPart4_UPH(item);

                    Output.Add(new HtmlContent { Org = item.Org, Title = $@"[{item.Org}][{ThisDay}]", ContentHtmlString = Content });
                });
                return Output;
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return null;
            }
        }

        private static string IssueTitleDateStringGenerator()
        {
            try
            {
                if (DateTime.Today.Day <= 15)
                {
                    var BasicDate = $@"{DateTime.Today.Year.ToString()}{DateTime.Today.AddMonths(-1).Month.ToString()}";
                    var Output = $@"IssueReport_{BasicDate}01-{BasicDate}30";
                    return Output;
                }
                else
                {
                    var thisMonth = $@"{DateTime.Today.Year.ToString()}{DateTime.Today.Month.ToString()}";
                    var lastMonth = $@"{DateTime.Today.Year.ToString()}{DateTime.Today.AddMonths(-1).Month.ToString()}";

                    var Output = $@"IssueReport_{lastMonth}15-{thisMonth}15";
                    return Output;
                }
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        private static string ProcessContentPart1(OutPutInfo item, string thisday)
        {
            try
            {
                var partial = $@"<p><font face=""arial"" color=""black"" size=""3""><b>Totally Performance review</b></font></p>
                                <table  width=""100%"" style=""text-align: left;"" border=""0"" cellpadding=""2"" cellspacing=""2"">
                                    <tbody>
                                        <tr>
                                            <td nowrap=""width=""20%"" align=""right"" valign=""top""><font face=""arial"" color=""black"" size=""3"">Report Date:</font></td>
                                            <td valign=""top""><font face=""arial"" color=""black"" size=""3"">{thisday}</font></td>
                                        </tr>
                                        <tr>
                                            <td nowrap=""width=""20%"" align=""right"" valign=""top""><font face=""arial"" color=""black"" size=""3"">Org:</font></td>
                                            <td valign=""top""><font face=""arial"" color=""black"" size=""3"">{item.Org}</font></td>
                                        </tr>
                                        <tr>
                                            <td nowrap=""width=""20%"" align=""right"" valign=""top""><font face=""arial"" color=""black"" size=""3"">Total Station:</font></td>
                                            <td valign=""top""><font face=""arial"" color=""black"" size=""3"">{item.OutLine.TotalStation}</font></td>
                                        </tr>
                                        <tr>
                                            <td nowrap=""width=""20%"" align=""right"" valign=""top""><font face=""arial"" color=""black"" size=""3"">Rule:</font></td>
                                            <td valign=""top""><font face=""arial"" color=""black"" size=""3"">(Count>{MassProductionConfig.ProductionCounts.ToString()} & FYR < {MassProductionConfig.FYR.ToString()}%) or (Count>{MassProductionConfig.ProductionCounts.ToString()} & Retry Rate > {MassProductionConfig.RetryRate.ToString()}%)</font></td>
                                        </tr>
                                        <tr>
                                            <td nowrap=""width=""20%"" align=""right"" valign=""top""><font face=""arial"" color=""black"" size=""3"">Alert Stations:</font></td>
                                            <td valign=""top""><font face=""arial"" color=""RED"" size=""3"">{item.OutLine.BadStationCount} </font></td>
                                        </tr>
                                        <tr>
                                            <td nowrap=""width=""20%"" align=""right"" valign=""top""><font face=""arial"" color=""black"" size=""3"">Total count:</font></td>
                                            <td valign=""top""><font face=""arial"" color=""black"" size=""3"">{item.AllStation.Sum(o => o.Total).ToString()}</font></td>
                                        </tr>
                                    </tbody>
                                </table>
                                <hr>";
                return partial;
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return string.Empty;
            }
        }

        private static string AddBiWeekReportLink(string xls_title)
        {
            var IssueLinkstr = $@"<h4 style=""font-family:arial;color:black;""><b>Latest Bi Weekly Report : </b><a href=""https://linwpa-pte01.ad.garmin.com/PTE_Web/DashBoard/ExportIssueReport""> {xls_title}</a></h4><hr>";
            return IssueLinkstr;
        }

        private static string ProcessContentPart2(OutPutInfo item, string thisday)
        {
            var issue_path = path + $@"\issue.png";
            try
            {
                var Partial = $@"<h4 style=""font-family:arial;color:black;""><b>Latest Creat Issue</b></h4>
                            <table  width=""100%"" style=""text-align: left;"" border=""1"" cellpadding=""2"" cellspacing=""2"">
                                        <thead>
                                            <tr style=""text-align: right;"" bgcolor=""black"" >
                                                <th > Org </th>
                                                <th> ItemNameType </th>
                                                <th> TestStation </th>
                                                <th> Description </th>
                                                <th> CreationDate </th>
                                                <th> IssueTrigger </th>
                                                <th> Detail</th>
                                            </tr>
                                        </thead>
                                        <tbody>";
                foreach (var raw in item.LatestIssueList)
                {
                    Partial += $@"<tr>
                                    <td>{raw.Org}</td>
                                    <td>{raw.ItemNameType}</td>
                                    <td>{raw.TestStation}</td>
                                    <td>{raw.Description}</td>
                                    <td>{thisday}</td>
                                    <td> {raw.IssueTrigger}</td>
                                    <td align=""center""><a href=""https://linwpa-pte01.ad.garmin.com/PTE_Web/IssueIndex/IssueEditor?issueid={raw.Title_id}&ItemNameType={raw.ItemNameType}&Org={raw.Org}""><img  style=""display:block"" width=""50"" src=""{issue_path}""></a></td>
                                 </tr>";
                }
                Partial += $@"</tbody></table><hr>";
                return Partial;
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return string.Empty;
            }
        }

        private static string ProcessContentPart3_FYR(OutPutInfo item)
        {
            try
            {
                var Partial = $@"<h4 style=""font-family:arial;color:black;""><b>FYR Focus station Detail</b></h4>
                                <table  width=""100%"" style=""text-align: left;"" border=""1"" cellpadding=""2"" cellspacing=""2"">
                                    <thead>
                                        <tr style=""text-align: right;"" bgcolor=""black"" >
                                            <th > Org </th>
                                            <th> ItemNameType </th>
                                            <th> TestStation </th>
                                            <th> Description </th>
                                            <th>IssueStatus </th>
                                            <th> FYR(%) </th>
                                            <th> RetryRate(%) </th>
                                            <th> Avg_Pass_Time(s) </th>
                                            <th> Avg_Total_Time (s) </th>
                                            <th> Pass</th>
                                            <th> Fail </th>
                                            <th> Total </th>
                                            <th> D_Pass </th>
                                            <th> D_Fail </th>
                                            <th> D_Total </th>
                                            <th>Detail</th>
                                        </tr>
                                    </thead>
                                    <tbody>";
                foreach (var raw in item.FocusStationRaw)
                {
                    var IssueMark = string.Empty;
                    if (raw.IssueStatus == "Open")
                        IssueMark = @"bgcolor=""pink""";

                    Partial += $@"<tr>
                                 <td {IssueMark}>{raw.Org}</td>
                                 <td {IssueMark}>{raw.ItemNameType}</td>
                                 <td {IssueMark}>{raw.TestStation}</td>
                                 <td {IssueMark}>{raw.Description}</td>
                                 <td {IssueMark}> {raw.IssueStatus}</td>
                                 <td {IssueMark}>{raw.FYR}</td>
                                 <td {IssueMark}>{raw.Retry_Rate}</td>
                                 <td {IssueMark}>{raw.Avg_Pass_Time}</td>
                                 <td {IssueMark}>{raw.Avg_Total_Time}</td>
                                 <td {IssueMark}>{raw.Pass}</td>
                                 <td {IssueMark}>{raw.Fail}</td>
                                 <td {IssueMark}>{raw.Total}</td>
                                 <td {IssueMark}>{raw.D_Pass}</td>
                                 <td {IssueMark}>{raw.D_Fail}</td>
                                 <td {IssueMark}>{raw.D_Total}</td>
                                 <td {IssueMark}><a href=""https://linwpa-pte01.ad.garmin.com/PTE_Web/AnalyzeTool/AnalyzeItemNameType?itemnametype={raw.ItemNameType}&org={raw.Org}"">Go</a></td>
                               </tr>";
                }
                Partial += $@"</tbody></table><hr>";
                //Partial += $@"<table width=""100%"" style=""text-align: left;"" border=""0"" cellpadding=""2"" cellspacing=""2"">
                //                        <tbody>
                //                            <tr>
                //                                <td nowrap="" width=""20%"" align=""right"" valign=""top""><font face=""arial"" color=""black"" size=""3"">Link to : <td valign=""top""><font face=""arial"" color=""black"" size=""3""><a href=""http://peweb/home/PTE_Web/DashBoard/DashBoardByOrg?Org={item.Org}"">PTE Web</a></td></tr></tbody></table></body>";
                return Partial;
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return string.Empty;
            }
        }

        private static string ProcessContentPart4_UPH(OutPutInfo item)
        {
            try
            {
                var Partial = $@"<h4 style=""font-family:arial;color:black;""><b>UPH Focus station Detail</b></h4>
                                <table  width=""100%"" style=""text-align: left;"" border=""1"" cellpadding=""2"" cellspacing=""2"">
                                    <thead>
                                        <tr style=""text-align: right;"" bgcolor=""black"" >
                                            <th > Org </th>
                                            <th> ItemNameType </th>
                                            <th> Description </th>
                                            <th>IssueStatus </th>
                                            <th> RealOutput </th>
                                            <th> EstimateUPH </th>
                                            <th> UPH </th>
                                            <th> UPHAchievement</th>
                                            <th> AvgSpare </th>
                                            <th> Gap </th>
                                        </tr>
                                    </thead>
                                    <tbody>";
                foreach (var raw in item.FocusUPHStationRow)
                {
                    var IssueMark = string.Empty;
                    if (raw.Issue_Status == "Open")
                        IssueMark = @"bgcolor=""pink""";

                    Partial += $@"<tr>
                                 <td {IssueMark}>{raw.Org}</td>
                                 <td {IssueMark}>{raw.ItemNameType}</td>
                                 <td {IssueMark}>{raw.ProductName}</td>
                                 <td {IssueMark}> {raw.Issue_Status}</td>
                                 <td {IssueMark}>{raw.RealOutput}</td>
                                 <td {IssueMark}>{raw.EstimateUPH}</td>
                                 <td {IssueMark}>{raw.UPH}</td>
                                 <td {IssueMark}>{raw.UPHAchievement}</td>
                                 <td {IssueMark}>{raw.AvgSpare}</td>
                                 <td {IssueMark}>{raw.Gap}</td>
                               </tr>";
                }
                Partial += $@"</tbody></table>
                                    <table width=""100%"" style=""text-align: left;"" border=""0"" cellpadding=""2"" cellspacing=""2"">
                                        <tbody>
                                            <tr>
                                                <td nowrap="" width=""20%"" align=""right"" valign=""top""><font face=""arial"" color=""black"" size=""3"">Link to : <td valign=""top""><font face=""arial"" color=""black"" size=""3""><a href=""https://linwpa-pte01.ad.garmin.com/PTE_Web/DashBoard/DashBoardByOrg?Org={item.Org}"">PTE Web</a></td></tr></tbody></table></body>";
                return Partial;
            }
            catch (Exception e)
            {
                _PTElogger.Trace(e.ToString());
                return string.Empty;
            }
        }
    }
}