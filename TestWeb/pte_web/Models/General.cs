using PTE_Web.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;

namespace PTE_Web.Models
{
    public class InitialParameter
    {
        public string SdateUTC { set; get; }
        public string EdateUTC { set; get; }
        public string SdateTW { set; get; }
        public string EdateTW { set; get; }
        public int ItemNameType { set; get; }
        public string Org { set; get; }
        public string Source { set; get; }

        public InitialParameter(string org, string itemnametype, string startdate, string enddate)
        {
            SdateUTC = string.Empty;
            EdateUTC = string.Empty;
            SdateTW = string.Empty;
            EdateTW = string.Empty;
            Org = "";
            if (startdate == null || enddate == null || startdate == "" || enddate == "")
            {
                SdateTW = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd");
                EdateTW = DateTime.Now.ToString("yyyy-MM-dd");
                SdateUTC = DateTime.Parse(SdateTW + " 00:00:00").AddHours(-8).ToString("yyyy-MM-dd HH:mm:ss");
                EdateUTC = DateTime.Parse(EdateTW + " 23:59:59").AddHours(-8).ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                if(startdate.Length!=19)
                {
                    startdate += " 00:00:00";
                    enddate += " 23:59:59";
                }

                //SdateTW = DateTime.Parse(startdate).ToString("yyyy-MM-dd");
                //EdateTW = DateTime.Parse(enddate).ToString("yyyy-MM-dd");
                SdateUTC = DateTime.Parse(startdate).AddHours(-8).ToString("yyyy-MM-dd HH:mm:ss");
                EdateUTC = DateTime.Parse(enddate).AddHours(-8).ToString("yyyy-MM-dd HH:mm:ss");
            }
            if (itemnametype == null || itemnametype == "" || org == null || org == "")
            {
                ItemNameType = 8484;
                Org = "T2";
                Source = "ATE";
            }
            else
            {
                ItemNameType = int.Parse(itemnametype);
                Org = org;
            }
        }
    }


    public class TestItemConfig
    {
        public int Itemnametype { get; set; }
        public string TestType { get; set; }
        public string TestType2 { get; set; }
        public string Source { get; set; }
        public string Description { get; set; }
        public string FixCorrelation { get; set; }



        //public TestItemConfig(int _itemnametype)
        //{

        //    Itemnametype = _itemnametype;

        //    var temp = DataHandlerFunctions.GetTestConfigByItemNameType(_itemnametype);

        //    TestType = temp!=null? temp.TestType:"";
        //    TestType2 = temp!=null? temp.TestType2:"";
        //    Source = temp != null ? temp.Source == "ATE" ? "TblCpu" : "TblFinal" : "";


        //}
    }

    public static class General
    {
        public static int GetWeekOfYear(string _dt)
        {
            var dt = DateTime.Parse(_dt);
            GregorianCalendar GetWeek = new GregorianCalendar();
            return GetWeek.GetWeekOfYear(dt, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }
        public static int GetWeekDayByDate(DateTime dt)
        {
            GregorianCalendar GetWeek = new GregorianCalendar();
            var baseWeek = GetWeek.GetDayOfWeek(dt);
            var initWeekDay = 0;

            switch (baseWeek)
            {
                case DayOfWeek.Monday:
                    initWeekDay = 1;
                    break;
                case DayOfWeek.Tuesday:
                    initWeekDay = 2;
                    break;
                case DayOfWeek.Wednesday:
                    initWeekDay = 3;
                    break;
                case DayOfWeek.Thursday:
                    initWeekDay = 4;
                    break;
                case DayOfWeek.Friday:
                    initWeekDay = 5;
                    break;
                case DayOfWeek.Saturday:
                    initWeekDay = 6;
                    break;
                case DayOfWeek.Sunday:
                    initWeekDay = 0;
                    break;
            }

            return initWeekDay;
        }

        // 不可跨年度
        public static string GetPeriodByWeek_SameYear(int week,int year)
        {
            GregorianCalendar GetWeek = new GregorianCalendar();
            var baseDate = DateTime.Parse($@"{year.ToString()}-01-01");
            var baseWeek = GetWeekDayByDate(baseDate);
            
            //W2 D1
            var W2D1Start = baseDate.AddDays(7 - baseWeek);

            if(week==1)
            {
                //每週由星期日開始
                return $@"{baseDate.ToString("yyyy-MM-dd")} {baseDate.AddDays(6-baseWeek).ToString("yyyy-MM-dd")}";
            }
            else
            {
                var sdate = W2D1Start.AddDays((week - 2) * 7);
                var edate = sdate.AddDays(6);
                return $@"{sdate.ToString("yyyy-MM-dd")} {edate.ToString("yyyy-MM-dd")}";
            }

            //2021-01-01 (W1S1) 5
            //2021-01-07 (W2D1) 0
            
        }

        public static Dictionary<string,string>GetWeekPeriodDate(string _sdate , string _edate)
        {
            try
            {
                var Output = new Dictionary<string, string>();

                GregorianCalendar GetWeek = new GregorianCalendar();

                var sdate = DateTime.Parse(_sdate);
                var edate = DateTime.Parse(_edate);

                if(sdate.Year == edate.Year)
                {
                    var weekOfSdate = GetWeek.GetWeekOfYear(sdate, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
                    var weekOfEdate = GetWeek.GetWeekOfYear(edate, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);

                    for (int i = weekOfSdate; i <= weekOfEdate; i++)
                    {
                        Output[$@"W{i}"] = GetPeriodByWeek_SameYear(i, sdate.Year);
                    }
                }
                else
                {
                    var tempEndDate = DateTime.Parse($@"{sdate.Year}-12-31");
                    var tempStartDate = DateTime.Parse($@"{edate.Year}-01-01");

                    var weekOfSdate = GetWeek.GetWeekOfYear(sdate, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
                    var weekOftempEndDate = GetWeek.GetWeekOfYear(tempEndDate, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);

                    for (int i = weekOfSdate; i <= weekOftempEndDate; i++)
                    {
                        Output[$@"W{i}"] = GetPeriodByWeek_SameYear(i, sdate.Year);
                    }

                    var weekOfEdate = GetWeek.GetWeekOfYear(edate, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
                    var weekOftempStartDate = GetWeek.GetWeekOfYear(tempStartDate, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);

                    for (int i = weekOftempStartDate; i <= weekOfEdate; i++)
                    {
                        Output[$@"W{i}"] = GetPeriodByWeek_SameYear(i, tempStartDate.Year);
                    }
                }


                return Output;
            }
            catch (Exception e)
            {
                return new Dictionary<string, string>();
            }
        }
    }
}