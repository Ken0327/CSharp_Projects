using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Outlook = Microsoft.Office.Interop.Outlook;
using System.Threading;
using System.Net.Mail;
using System.Net;

//類別：傳送HTML 格式Mail---------------------------------------------------------------------------------------------------------------------------------------------------------------------------
public class SendEmail
{
	public SendEmail()
	{
	}

	public SendEmail(string strMailsrv, string strFrom)
	{
		_strMailsrv = strMailsrv;
		_strFrom = strFrom;
	}

	private string _strMailsrv = "t1smtp.ad.garmin.com";

	public string strMailsrv
	{
		get { return _strMailsrv; }
	}

	//Send Mail TO
	private string _strTo = string.Empty;

	public string strTo
	{
		get { return _strTo; }
		set { _strTo = value; }
	}

	//Mail Subject
	private string _strSubject = string.Empty;

	public string strSubject
	{
		set { _strSubject = value; }
		get { return _strSubject; }
	}

	//Mail Body
	private string _strBody = string.Empty;

	public string strBody
	{
		get { return _strBody; }
		set { _strBody = value; }
	}

	//Attached Patch
	private string _strAttachmentPath = string.Empty;

	public string AttachmentPath
	{
		get { return _strAttachmentPath; }
		set { _strAttachmentPath = value; }
	}

	//CC
	private string _strCC = string.Empty;

	public string strCC
	{
		get { return _strCC; }
		set { _strCC = value; }
	}

	private string _strBCC = string.Empty;

	public string strBCC
	{
		get { return _strBCC; }
		set { _strBCC = value; }
	}

	//HTML Formate
	private bool _IsHtml = true;

	public bool IsHtml
	{
		get { return _IsHtml; }
		set { _IsHtml = value; }
	}

	//
	private string _strFrom = "pesupport@garmin.com";

	public string strFrom
	{
		get { return _strFrom; }
		set { _strFrom = value; }
	}

	public void Send()
	{
		new Thread(new ThreadStart(SendMessage)).Start();
	}

	/// <summary>
	/// Send Email Message method.
	/// </summary>
	private void SendMessage()
	{
		try
		{
			MailMessage oMessage = new MailMessage();
			SmtpClient smtpClient = new SmtpClient(_strMailsrv);
			oMessage.From = new MailAddress(_strFrom);
			oMessage.To.Clear();
			foreach (string strMailTo in _strTo.Split(';'))
			{
				if (strMailTo.Trim() != string.Empty)
					oMessage.To.Add(strMailTo);
			}

			oMessage.Subject = _strSubject;
			oMessage.IsBodyHtml = IsHtml;
			oMessage.Body = strBody;

			oMessage.CC.Clear();
			oMessage.Bcc.Clear();
			if (_strCC != string.Empty)
			{
				foreach (string strMailCC in _strCC.Split(';'))
					if (strMailCC != "")
					{
						oMessage.CC.Add(strMailCC);
					}
			}

			if (_strBCC != string.Empty)
			{
				foreach (string strMailBCC in _strBCC.Split(';'))
					if (strMailBCC != "")
					{
						oMessage.Bcc.Add(strMailBCC);
					}
			}

			// Create and add the attachment
			if (AttachmentPath != string.Empty)
				oMessage.Attachments.Add(new Attachment(AttachmentPath));

			smtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;
			try
			{
				// Deliver the message
				//smtpClient.Send(oMessage);

				var sMail = new SmtpClient()
				{
					DeliveryMethod = SmtpDeliveryMethod.Network,
					Host = "t1smtp.ad.garmin.com"
				};
				sMail.Send(oMessage);
			}
			catch (Exception ex)
			{
				ex.ToString();
			}
		}
		catch (Exception ex)
		{
			ex.ToString();
		}
	}
}