using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;
using System.Collections;
using System.IO;
using MimeKit.Utils;

namespace PTEDailyReport
{
    internal class MailConfig
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public static async Task SendMail(string Title, string content, List<MaillReciver> MailGroup)
        {
            try
            {
                var mailList = new List<MailboxAddress>();
                MailGroup.ForEach(item =>
                {
                    mailList.Add(new MailboxAddress(item.Name, item.Email));
                });

                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("PTE Reporter", "Reporter@example.com"));
                mailList.ForEach(item =>
                {
                    emailMessage.To.Add(item);
                });

                emailMessage.Subject = Title;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = content
                };

                string AppPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                List<string> imgpaths = new List<string>()
                {
                    $@"{AppPath}\background.gif",
                    $@"{AppPath}\hd-logo.jpg",
                    $@"{AppPath}\issue.png"
                };

                foreach (string imgpath in imgpaths)
                {
                    var image = bodyBuilder.LinkedResources.Add(imgpath);
                    image.ContentId = MimeUtils.GenerateMessageId();
                    bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace(imgpath, string.Format("cid:{0}", image.ContentId));
                }

                emailMessage.Body = bodyBuilder.ToMessageBody();
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.LocalDomain = "t2-pte-jasper-n";
                    await client.ConnectAsync("t1smtp.garmin.com.tw", 25, SecureSocketOptions.None).ConfigureAwait(false);
                    await client.SendAsync(emailMessage).ConfigureAwait(false);
                    await client.DisconnectAsync(true).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                _logger.Trace(e);
            }
        }
    }

    public class MaillReciver
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Org { get; set; }
        public int Developer { get; set; }
        public List<int> ProductOwnerList { get; set; }
    }
}