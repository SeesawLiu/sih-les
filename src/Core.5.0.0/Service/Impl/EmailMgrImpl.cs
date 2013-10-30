using System.Net.Mail;
using com.Sconit.Service.SendMail;

namespace com.Sconit.Service.Impl
{
    public class EmailMgrImpl : BaseMgr, IEmailMgr
    {
        public ISystemMgr systemMgr { get; set; }
        public string userName { get; set; }
        public string userPassword { get; set; }
        public int senderNo { get; set; }
        public bool isOneByOne { get; set; }

        public void SendEmail(string subject, string body, string mailTo)
        {
            SendEmail(subject, body, mailTo, string.Empty, MailPriority.Normal);
        }

        public void SendEmail(string subject, string body, string mailTo, string replayTo)
        {
            SendEmail(subject, body, mailTo, replayTo, MailPriority.Normal);
        }

        public void SendEmail(string subject, string body, string mailTo, MailPriority mailPriority)
        {
            SendEmail(subject, body, mailTo, string.Empty, mailPriority);
        }

        public void SendEmail(string subject, string body, string mailTo, string replayTo, MailPriority mailPriority)
        {
            //string SMTPEmailHost = systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.SMTPEmailHost);
            //string SMTPEmailPasswd = systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.SMTPEmailPasswd);
            //string emailFrom = systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.SMTPEmailAddr);

            SendMail.SendMailV2 sendMailv2 = new SendMail.SendMailV2();
            SendMail.SecurityHeader securityHeader = new SendMail.SecurityHeader();
            securityHeader.UserName = userName;
            securityHeader.UserPassword = userPassword;
            sendMailv2.SecurityHeaderValue = securityHeader;
            sendMailv2.SendEmailAndUpAttachments(mailTo, string.Empty, subject, body, new UpAttachments[] { }, isOneByOne, senderNo);
            //SMTPHelper.SendSMTPEMail(subject, body, emailFrom, mailTo, SMTPEmailHost, SMTPEmailPasswd, (!string.IsNullOrWhiteSpace(replayTo) ? replayTo : emailFrom), mailPriority);
        }

        public void AsyncSendEmail(string subject, string body, string mailTo)
        {
            AsyncSendEmail(subject, body, mailTo, string.Empty, MailPriority.Normal);
        }

        public void AsyncSendEmail(string subject, string body, string mailTo, string replayTo)
        {
            AsyncSendEmail(subject, body, mailTo, replayTo, MailPriority.Normal);
        }

        public void AsyncSendEmail(string subject, string body, string mailTo, MailPriority mailPriority)
        {
            AsyncSendEmail(subject, body, mailTo, string.Empty, mailPriority);
        }

        public void AsyncSendEmail(string subject, string body, string mailTo, string replayTo, MailPriority mailPriority)
        {
            AsyncSend asyncSend = new AsyncSend(this.SendEmail);
            asyncSend.BeginInvoke(subject, body, mailTo, replayTo, mailPriority, null, null);
        }

        public delegate void AsyncSend(string subject, string body, string mailTo, string replayTo, MailPriority mailPriority);
    }
}
