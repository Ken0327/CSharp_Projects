using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using PTE_Web.Connections;
using PTE_Web.Models;

namespace PTE_Web.Controllers
{
    public class IssueIndexController : Controller
    {
        // GET: IssueIndex
        public ActionResult Index(int? ItemNameType, string Org)
        {
            var IssueTables = DataHandlerFunctions.DataIssuesByDateRange().Where(x => x.Status_Pic != "Pedding").GroupBy(x => new { x.ItemNameType, x.Support_Org }).ToList().Select(x => x.ToList()[0]).OrderByDescending(x => x.LastUpdateDate).ToList();

            var OrgList = new List<string>() { "T1", "T2", "T3", "T5" };
            var OrgsIssue = new List<IssueOutlie>();

            foreach (var org in OrgList)
            {
                var OrgTable_T = (from raw in IssueTables
                                  where raw.Support_Org == org && raw.Status
                                  select raw).ToList();

                var OrgTable_F = (from raw in IssueTables
                                  where raw.Support_Org == org && !raw.Status
                                  select raw).ToList();

                OrgsIssue.Add(new IssueOutlie { OPEN = OrgTable_T.Count(), CLOSE = OrgTable_F.Count() });
            }

            if (ItemNameType != null)
            {
                ViewBag.IssueReplys = DataHandlerFunctions.GetIssueReplys(ItemNameType, Org);
            }
            ViewBag.OrgsIssue = OrgsIssue;
            ViewBag.IssueList = IssueTables;
            return View();
        }

        [HttpPost]
        public ActionResult GetReplyAction(int? ItemNameType, string Org)
        {
            var data = DataHandlerFunctions.GetIssueReplys(ItemNameType, Org);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult InsertTracingInfoAction(int ItemNameType, string Empid, string Org)
        {
            var data = DataHandlerFunctions.UpdateTrackingActionTable(ItemNameType, Empid, Org);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult RemoveTracingInfoAction(int ItemNameType, string Empid, string Org)
        {
            var data = DataHandlerFunctions.RemoveTrackingActionTable(ItemNameType, Empid, Org);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(false)]
        public ActionResult IssueEditor(string issueid, string ItemNameType, string org, string userid, string ownerid, string failitem, string causeid, string cause_common, string actionid, string action_common, string creatime, HttpPostedFileBase file, string startdate, string enddate, string trace_flag)
        {
            if (org != null)
            {
                TempData["Org"] = org.ToString();
                TempData.Keep("Org");
            }
            else
            {
                org = DataHandlerFunctions.GetIssueOrg(issueid, ItemNameType);
                TempData["Org"] = org.ToString();
            }

            var DateDict = InitialDateInfo(startdate, TempData["StartDate"], enddate, TempData["EndDate"]);

            var IssueConfig = new IssueEditorConfig(issueid, ItemNameType, org, DateDict, startdate);

            TempData["StartDate"] = IssueConfig.DateDict["sdate"].ToString();
            TempData.Keep("StartDate");
            TempData["EndDate"] = IssueConfig.DateDict["edate"].ToString();
            TempData.Keep("EndDate");

            ViewBag.IssueID = IssueConfig.IssueID.ToString() == "0" ? string.Empty : IssueConfig.IssueID.ToString();
            ViewBag.ItemNameType = IssueConfig.ItemNameType.ToString() == "0" ? string.Empty : IssueConfig.ItemNameType.ToString();
            if (IssueConfig.IssueID == 0) return View();

            if (ownerid != null)
            {
                var ContentConfig = new PTEWEB_Issues_Reply()
                {
                    UserName = Request.Form["userid"],
                    Owner = Request.Form["ownerid"],
                    Actionid = int.Parse(Request.Form["actionid"] ?? "0"),
                    Causeid = int.Parse(Request.Form["causeid"] ?? "0"),
                    FailItem = Request.Form["failitem"] ?? string.Empty,
                    CauseCommon = Request.Form["cause_common"] ?? string.Empty,
                    ActionCommon = Request.Form["action_common"] ?? string.Empty,
                    fileName = "NaN"
                };
                if (action_common != "")
                {
                    var NeedClose = Request.Form["closeflag"] == "1" ? "on" : "off";
                    if (NeedClose == "on")
                    {
                        DataHandlerFunctions.CloseIssue(IssueConfig.ItemNameType, IssueConfig.Org);
                        IssueConfig.IssueConfigDict["PanelType"] = "success";
                        IssueConfig.IssueConfigDict["IssueStatus"] = "Close";
                        ContentConfig.ActionCommon = Request.Form["action_common2"];
                    }
                    if (file != null)
                    {
                        ContentConfig.fileName = Upload(IssueConfig.ItemNameType.ToString(), IssueConfig.IssueID.ToString(), file);
                    }
                    DataHandlerFunctions.UpdateToIssueActionTable(IssueConfig.IssueID, IssueConfig.ItemNameType, ContentConfig, org, Convert.ToInt32(trace_flag));
                }
            }

            if (creatime != null)
            {
                DataHandlerFunctions.DeleteIssueAction((DateTime.Parse(creatime)).ToString("yyyy-MM-dd hh:mm:ss"), ItemNameType, issueid);
            }
            ViewBag.DailyFYRList = new JavaScriptSerializer().Serialize(IssueConfig.DailyFYRInfo.GetRange(0, IssueConfig.DailyFYRInfo.Count)).ToString();
            ViewBag.IssueConfig = IssueConfig.IssueConfigDict;

            var IssueReplay = DataHandlerFunctions.GetIssueReplayByID(IssueConfig.IssueID, IssueConfig.ItemNameType.ToString());

            //ViewBag.TracingList
            string str_tracing = "";
            List<string> TracingList = DataHandlerFunctions.GetIssueTrackingList(IssueConfig.ItemNameType.ToString(), org);
            foreach (string str in TracingList)
            {
                str_tracing += dic_Emp.FirstOrDefault(x => x.Value == str).Key.ToString() + "|";
            }
            ViewBag.TracingList = str_tracing.TrimEnd('|');
            ViewBag.EditorList = IssueConfig.EditorList;
            ViewBag.ActionList = IssueConfig.ActionList;
            ViewBag.CauseList = IssueConfig.CauseList;
            ViewBag.OwnerList = IssueConfig.OwnerList;
            ViewBag.FailItemList = IssueConfig.FailItemList;
            IssueReplay.ForEach(item => item.Status = (item.Actionid == 10 ? "Close" : "Open"));

            return View(IssueReplay);
        }

        [HttpPost]
        public ActionResult InsertNewIssue(string frm_itemnametype, string org, string frm_description, string issueContent)
        {
            //step1.先找看看有無已經新增過的issue 有的話update 狀態
            bool Issue_status = DataHandlerFunctions.CheckIssueStatus0(org, frm_itemnametype);
            //step2.沒有的話，新增issue 與title 與 history
            if (Issue_status)
            {//可新增Issue
                DataHandlerFunctions.InsertNewIssue(frm_itemnametype, org, issueContent);
            }
            else
            {//upate
                DataHandlerFunctions.UpdateIssues(frm_itemnametype, org, issueContent);
            }
            TempData["Msg"] = "Issue新增完成";
            return RedirectToAction("index");
        }

        public ActionResult GetItemNameTypeDes(string queryStr, string org)
        {
            var result = new List<ItemNameDesModel>();
            result = DataHandlerFunctions.GetqueryItemNameTypeorDes(queryStr, org);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetIssueByItemNameType(int ItemNameType, string date)
        {
            var AllModels = new AnalyzePageModels();
            AllModels.PTEWEB_Issues = DataHandlerFunctions.GetIssues(ItemNameType, date);
            return Json(AllModels.PTEWEB_Issues, JsonRequestBehavior.AllowGet);
        }

        public string Upload(string itemnametype, string issueid, HttpPostedFileBase file)
        {
            try
            {
                var fileName = Path.GetFileName(file.FileName);
                var path = $@"//LINWPA-PTE01/Users/ptetw/Desktop/WebFolder/PTE_Web_Issue_Upload/{itemnametype}/{issueid}";
                if (file.ContentLength > 0)
                {
                    //若該資料夾不存在，則新增一個
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    var Finalpath = Path.Combine(path, fileName);
                    file.SaveAs(Finalpath);
                    return (path + "/" + fileName).Replace('/', '\\');
                }
                return "NaN";
            }
            catch (Exception e)
            {
                return "NaN";
            }
        }

        public ActionResult Download(string path)
        {
            FileInfo fl = new FileInfo(path);
            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = fl.Name,
                Inline = false,
            };
            Response.AppendHeader("Content-Disposition", cd.ToString());

            var readStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            string contentType = MimeMapping.GetMimeMapping(path);
            return File(readStream, contentType);
        }

        private static Dictionary<string, string> InitialDateInfo(string _sdate, object tempSdate, string _edate, object tempEdate)
        {
            var Output = new Dictionary<string, string>();

            if (_sdate == null || _edate == null || _sdate == "" || _edate == "")
            {
                if (tempSdate != null || tempEdate != null)
                {
                    _sdate = tempSdate.ToString();
                    _edate = tempEdate.ToString();
                }
                else
                {
                    _sdate = DateTime.Now.AddMonths(-3).ToString("yyyy-MM-dd");
                    _edate = DateTime.Now.ToString("yyyy-MM-dd");
                }
            }

            Output["sdate"] = _sdate;
            Output["edate"] = _edate;

            return Output;
        }

        public Dictionary<string, string> dic_Emp = DataHandlerFunctions.GetEditorList().ToDictionary(p => p.Ename, p => p.Empid.ToString());

    }

    public class IssueEditorConfig
    {
        public int IssueID { get; set; }
        public int ItemNameType { get; set; }
        public string Org { get; set; }

        public Dictionary<string, string> DateDict { get; set; }
        public Dictionary<string, Double> FYRDict { get; set; }
        public Dictionary<string, int> OutputDict { get; set; }
        public Dictionary<string, string> IssueConfigDict { get; set; }
        public PTEWEB_Issues_Title IssueTitle { get; set; }
        public PTEWEB_Issues_ByDaily IssueDaily { get; set; }
        public List<SimplyFYRInfo> DailyFYRInfo { get; set; }
        public IEnumerable<SelectListItem> OwnerList { get; set; }
        public IEnumerable<SelectListItem> EditorList { get; set; }
        public IEnumerable<SelectListItem> ActionList { get; set; }
        public IEnumerable<SelectListItem> CauseList { get; set; }
        public IEnumerable<SelectListItem> FailItemList { get; set; }

        public IssueEditorConfig(string issueid, string itemnametype, string org, Dictionary<string, string> datedict, string chooseSDate)
        {
            if (itemnametype == string.Empty || itemnametype == null)
            {
                IssueID = 0;
                ItemNameType = 0;
            }
            else
            {
                var dictfyr = new Dictionary<string, double>();
                var dictoutput = new Dictionary<string, int>();
                var issueconfig = new Dictionary<string, string>();
                var dailyfyrList = new List<SimplyFYRInfo>();
                var ownerlist = new List<SelectListItem>();
                IssueTitle = DataHandlerFunctions.GetIssueTitleInfo(itemnametype, org);
                IssueDaily = DataHandlerFunctions.GetIssueDailyInfo(itemnametype, org);
                if (IssueTitle == null)
                {
                    IssueID = 0;
                    ItemNameType = 0;
                }
                else
                {
                    ItemNameType = IssueTitle.ItemNameType;
                    IssueID = IssueTitle.Title_id;
                    Org = IssueTitle.Org;
                    issueconfig["Org"] = IssueDaily.Support_Org;
                    issueconfig["IssueType"] = IssueTitle.Title;
                    issueconfig["IssueCreateDate"] = DateTime.Parse(IssueTitle.CreateDate).ToString("yyyy-MM-dd");
                    issueconfig["IssueStatus"] = IssueTitle.Issue_Status == true ? "Open" : "Close";
                    issueconfig["IssueAlive_Dates"] = (DateTime.Now - DateTime.Parse(IssueTitle.CreateDate)).Days.ToString();
                    //issueconfig["IssueAlive_Dates"] = IssueDaily.IssueAlive_Dates.ToString();

                    issueconfig["TypeCount"] = (IssueTitle.Title.Contains("雙") ? 6 : 14).ToString(); ;

                    datedict["sdate"] = chooseSDate == "" || chooseSDate == null ? DateTime.Parse(IssueTitle.CreateDate).ToString("yyyy-MM-dd") : datedict["sdate"];
                    datedict["edate"] = chooseSDate == "" || chooseSDate == null ? DateTime.Parse(IssueTitle.CreateDate).AddDays(30).ToString("yyyy-MM-dd") : datedict["edate"];

                    var DailyFYRList = DataHandlerFunctions.GetFYRByItemNameTypeAndTopNumber(datedict["sdate"], datedict["edate"], ItemNameType, int.Parse(issueconfig["TypeCount"]));
                    var DailyUPHList = DataHandlerFunctions.GetEstimateUPHByItemNameTypeAndTopNumber(datedict["sdate"], datedict["edate"], ItemNameType, int.Parse(issueconfig["TypeCount"]));

                    var DataDateRange = DailyFYRList.Select(item => item.Date).ToList();
                    DataDateRange.ForEach(item =>
                    {
                        var TempUPHRaw = DailyUPHList.Find(raw => raw.Date.ToString("yyyy-MM-dd") == item.ToString("yyyy-MM-dd"));
                        var TempRaw = DailyFYRList.Find(raw => raw.Date.ToString("yyyy-MM-dd") == item.ToString("yyyy-MM-dd"));
                        if (TempRaw != null || TempUPHRaw != null)
                        {
                            var IssueFlag = DataHandlerFunctions.GetBulletFromData(item.ToString("yyyy-MM-dd"), ItemNameType);
                            dailyfyrList.Add(new SimplyFYRInfo()
                            {
                                ItemNameType = ItemNameType,
                                Date = item.ToString("yyyy-MM-dd"),
                                FYR = TempRaw == null ? 0 : TempRaw.FYR,
                                Total = TempRaw == null ? 0 : TempRaw.Total,
                                Spare = TempRaw == null ? 0 : TempRaw.Avg_Pass_Time,
                                EstimateUPH = TempUPHRaw == null ? 0 : TempUPHRaw.EstimateUPH,
                                href = IssueFlag == "No" ? "" : $@"..\\Content\\exclamation-{IssueFlag}.png"
                                //IssueCreate event (Done) / Issue Action event (Undo) / Issue Close event (Undo)
                            });
                            dictfyr[item.ToString("yyyy-MM-dd")] = TempRaw.FYR;
                            dictoutput[item.ToString("yyyy-MM-dd")] = TempRaw.Total;
                        }
                    });
                    issueconfig["IssueDescription"] = DataHandlerFunctions.GetTestConfigByItemNameType(ItemNameType).Description;
                    issueconfig["todayFYR"] = dictfyr.Count != 0 ? dictfyr.First().Value.ToString() : "NaN";
                    issueconfig["PanelType"] = IssueTitle.Issue_Status == true ? "danger" : "success";
                    IssueConfigDict = issueconfig;
                    FYRDict = dictfyr;
                    OutputDict = dictoutput;
                    ownerlist.Add(new SelectListItem() { Text = "PTE", Value = "PTE" });
                    ownerlist.Add(new SelectListItem() { Text = "EE", Value = "EE" });
                    ownerlist.Add(new SelectListItem() { Text = "PE", Value = "PE" });
                    ownerlist.Add(new SelectListItem() { Text = "AE", Value = "AE" });
                    OwnerList = ownerlist;
                    EditorList = from member in DataHandlerFunctions.GetEditorList().OrderBy(x=>x.Org)
                                 select new SelectListItem()
                                 {
                                     Text = $@"{member.Org}: {member.Ename}",
                                     Value = member.Empid.ToString()
                                 };
                    ActionList = DataHandlerFunctions.GetActionList();
                    CauseList = DataHandlerFunctions.GetCauseList();
                    FailItemList = DataHandlerFunctions.GetFailItemList(IssueTitle.ItemNameType);
                    DailyFYRInfo = dailyfyrList;
                    DateDict = datedict;
                }
            }
        }
    }
}