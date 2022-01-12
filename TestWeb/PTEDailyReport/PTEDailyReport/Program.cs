using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using NLog;
using PTEDailyReport.Model;
using PTEDailyReport.Models;

namespace PTEDailyReport
{
    internal class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            //Mode : 1(Production Mode)/ 0(Test Mode)
            Config Config = new Config(Mode.Production, MailGroup.ALL);

            var ThisDay = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
            var OrgList = Config.MailAddress.Select(item => item.Org).Distinct().ToList();
            var Result = DataProcess.GetDataResult(OrgList, ThisDay);

            var HtmlContent = HTMLContentProcess.GetHtmlStringFromData(Result, ThisDay);
            HtmlContent.ForEach(item =>
            {
                var rlt = MailConfig.SendMail(item.Title, item.ContentHtmlString, Config.MailAddress.FindAll(o => o.Org == item.Org && o.Developer == 0).ToList());
                Thread.Sleep(2000);

                Console.WriteLine($@"{item.Org} Process Completed.");
            });
        }
    }
}