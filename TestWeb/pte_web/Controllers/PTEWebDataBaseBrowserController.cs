using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Dynamic;
using PTE_Web.Models;
using PTE_Web.Connections;
using System.Data;

namespace PTE_Web.Controllers
{
    public class PTEWebDataBaseBrowserController : Controller
    {
        // GET: PTEWebDataBaseBrowser
        public ActionResult HomePlayGround( string startdate, string enddate,string sqlscript,string sqlscript_Head,string sqlscript_Tail)
        {
            var ChooseTable = Request.Form["DropDownList_DataBaseGroup"] ?? "#Daily_FYR_Output_Group1";

            var SQLscript_origion = $@"{sqlscript_Head} from {ChooseTable} as FinalTable where date between '{startdate}' and '{enddate}'  {sqlscript_Tail}";

            var ScriptModel = new DashBoardGroundModel(startdate, enddate);

            var SQLscript_input = ScriptModel.ProcessScript(SQLscript_origion, ChooseTable);

            var DataHandler = new DataBaseGroundHandler(startdate,enddate,SQLscript_input,ChooseTable);
 
            ViewBag.Sdate = DataHandler.SDate;

            ViewBag.Edate = DataHandler.EDate;

            ViewBag.HeadScriptString = DataHandler.QueryResult && sqlscript_Head!=null? sqlscript_Head:"Select * ";

            ViewBag.TailScriptString = DataHandler.QueryResult && sqlscript_Tail!=null? sqlscript_Tail:"  ";

            ViewBag.TableColumns = DataHandler.DataTableColumns;

            ViewBag.TableValues = DataHandler.DataTableValues;

            ViewBag.ChooseTable = ChooseTable;

            return View();
        }

        public ActionResult CpkDataTable(string startdate, string enddate, string itemnametype, string org,string bckItem)
        {
            if (startdate == null || enddate == null || startdate == "" || enddate == "" || startdate == null || enddate == null)
            {
                if (TempData["StartDate"] != null || TempData["EndDate"] != null)
                {
                    if (startdate == "" || enddate == "")
                    {
                        startdate = TempData["StartDate"].ToString();
                        enddate = TempData["EndDate"].ToString();
                    }
                }
                else
                {
                    startdate = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd");
                    enddate = DateTime.Now.ToString("yyyy-MM-dd");
                }
            }
            var PageConfig = new InitialParameter(org, itemnametype, startdate, enddate);

            TempData["StartDate"] = startdate;
            TempData["EndDate"] = enddate;
            TempData["ItemNameType"] = PageConfig.ItemNameType;
            TempData["Org"] = PageConfig.Org;

            var ItemNameTypeConfig = DataHandlerFunctions.GetTestConfigByItemNameType(PageConfig.ItemNameType);

            var ListCpkModel = new List<TableRow>();
            var SpareTable = new DataTable();
            var Output = DataBrowser.GetAlltemTableByItemName_Org_DateRange(PageConfig.Org, PageConfig.ItemNameType, startdate, enddate);

            var ItemSapreDatas = FailItemModel.GetFailItemSpareAndDelay(PageConfig.Org, PageConfig.ItemNameType, PageConfig.SdateUTC, PageConfig.EdateUTC, PageConfig.Source, out SpareTable);

            Output.ForEach(item =>
            {
                var _specmax = 0.0;
                var _specmin = 0.0;
                double.TryParse(item.SpecMax, out _specmax);
                double.TryParse(item.SpecMin, out _specmin);
                var _tempSpareFilter = ItemSapreDatas.Where(raw => raw._DbIndex == item.FailItem);
                var tempCpkTable = new TableRow()
                {
                    TestCount = item.TotalCount.ToString(),
                    AVG = _tempSpareFilter.Count() > 0 ? Math.Round(_tempSpareFilter.First()._AvgSpare, 2) : 0,
                    Cpk = -999,
                    SD = 0,
                    Spec = $@"{_specmin}-{_specmax}",
                    MaxSpec = _specmax.ToString(),
                    MinSpec = _specmin.ToString(),
                    DBIndex = item.FailItem.ToString(),
                    ItemDescription = item.ItemName,
                    FailCount = item.FailCount,
                    FailRate = item.FailRate.ToString(),
                    PassRate = item.PassRate.ToString()
                };

                if (bckItem =="on")
                {
                    if (double.TryParse(item.SpecMax, out _specmax) && double.TryParse(item.SpecMin, out _specmin) && _specmax != _specmin)
                    {
                        var CpkInfo = DataHandlerFunctions.GetItemNameTypeCpkModel(PageConfig.ItemNameType, item.Source, PageConfig.Org, PageConfig.SdateUTC, PageConfig.EdateUTC, item.FailItem, _specmin, _specmax);
                        if (CpkInfo.CpkTable.Count > 0)
                        {
                            tempCpkTable.SD = CpkInfo.CpkTable.First().SD;
                            tempCpkTable.Cpk = CpkInfo.CpkTable.First().Cpk;
                        }
                        else
                        {
                            tempCpkTable.SD = 0;
                            tempCpkTable.Cpk = 0;
                        }
                    }
                }
                ListCpkModel.Add(tempCpkTable);
            });

            ViewBag.Description = ItemNameTypeConfig.Description;
            return View(ListCpkModel);
        }

    }

}