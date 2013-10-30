using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using com.Sconit.Entity.SAP;
using com.Sconit.Entity.SYS;
using com.Sconit.Util;
using com.Sconit.Utility;
namespace com.Sconit.Service.SAP
{
    public class BaseMgr
    {
        #region
        private static String _sAPServiceUserName { get; set; }
        private static String _sAPServicePassword { get; set; }
        private static Int32? _sAPServiceTimeOut { get; set; }
        private static string _sAPServiceAddress { get; set; }
        private static string _sAPServicePort { get; set; }

        public IGenericMgr genericMgr { get; set; }
        public ISystemMgr systemMgr { get; set; }
        public NVelocityTemplateRepository vmReporsitory { get; set; }
        public IEmailMgr emailMgr { get; set; }
        public INumberControlMgr numberControlMgr { get; set; }

        protected ICredentials Credentials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_sAPServiceUserName))
                {
                    _sAPServiceUserName = this.systemMgr.GetEntityPreferenceValue(
                        com.Sconit.Entity.SYS.EntityPreference.CodeEnum.SAPServiceUserName);
                }

                if (string.IsNullOrWhiteSpace(_sAPServicePassword))
                {
                    _sAPServicePassword = this.systemMgr.GetEntityPreferenceValue(
                        com.Sconit.Entity.SYS.EntityPreference.CodeEnum.SAPServicePassword);
                }

                return new NetworkCredential(_sAPServiceUserName, _sAPServicePassword);
            }
        }

        protected Int32 TimeOut
        {
            get
            {
                if (!_sAPServiceTimeOut.HasValue)
                {
                    _sAPServiceTimeOut = int.Parse(this.systemMgr.GetEntityPreferenceValue(
                        com.Sconit.Entity.SYS.EntityPreference.CodeEnum.SAPServiceTimeOut)
                    );
                }

                return _sAPServiceTimeOut.Value;
            }
        }

        private string SAPServiceAddress
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_sAPServiceAddress))
                {
                    _sAPServiceAddress = this.systemMgr.GetEntityPreferenceValue(
                        com.Sconit.Entity.SYS.EntityPreference.CodeEnum.SAPServiceAddress);
                }
                return _sAPServiceAddress;
            }
        }

        private string SAPServicePort
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_sAPServicePort))
                {
                    _sAPServicePort = this.systemMgr.GetEntityPreferenceValue(
                        com.Sconit.Entity.SYS.EntityPreference.CodeEnum.SAPServicePort);
                }
                return _sAPServicePort;
            }
        }

        protected string ReplaceSAPServiceUrl(string originalUrl)
        {
            return ServiceURLHelper.ReplaceServiceUrl(originalUrl, this.SAPServiceAddress, this.SAPServicePort);
        }
        #endregion

        protected void CreateSiSap<T>(T t)
        {
            try
            {
                ITraceable traceable = t as ITraceable;
                if (traceable != null)
                {
                    DateTime dateTimeNow = DateTime.Now;
                    traceable.CreateDate = dateTimeNow;
                    traceable.LastModifyDate = dateTimeNow;
                    traceable.Status = StatusEnum.Pending;
                }
                genericMgr.Create(t);
                genericMgr.FlushSession();
            }
            catch (Exception ex)
            {
                genericMgr.CleanSession();
                throw ex;
            }
        }

        protected void UpdateSiSap<T>(T t)
        {
            try
            {
                ITraceable traceable = t as ITraceable;
                if (traceable != null)
                {
                    traceable.LastModifyDate = DateTime.Now;
                }
                genericMgr.Update(t);
                genericMgr.FlushSession();
            }
            catch (Exception ex)
            {
                genericMgr.CleanSession();
                throw ex;
            }
        }

        protected void CreateSiSap<T>(IList<T> tList)
        {
            try
            {
                foreach (var t in tList)
                {
                    ITraceable traceable = t as ITraceable;
                    if (traceable != null)
                    {
                        DateTime dateTimeNow = DateTime.Now;
                        traceable.CreateDate = dateTimeNow;
                        traceable.LastModifyDate = dateTimeNow;
                        traceable.Status = StatusEnum.Pending;
                    }
                    genericMgr.Create(t);
                }
                genericMgr.FlushSession();
            }
            catch (Exception ex)
            {
                genericMgr.CleanSession();
                throw ex;
            }
        }

        protected void UpdateSiSap<T>(IList<T> tList)
        {
            try
            {
                foreach (var t in tList)
                {
                    ITraceable traceable = t as ITraceable;
                    if (traceable != null)
                    {
                        traceable.LastModifyDate = DateTime.Now;
                    }
                    genericMgr.Update(t);
                }
                genericMgr.FlushSession();
            }
            catch (Exception ex)
            {
                genericMgr.CleanSession();
                throw ex;
            }
        }

        protected void SendErrorMessage(IList<ErrorMessage> errorMessageList)
        {
            var distinctTemplates = errorMessageList.Select(t => t.Template).Distinct();
            foreach (var nVelocityTemplate in distinctTemplates)
            {
                MessageSubscirber messageSubscriber = genericMgr.FindById<MessageSubscirber>((int)nVelocityTemplate);
                var q_ItemErrors = errorMessageList.Where(t => (int)t.Template == (int)nVelocityTemplate).Take(messageSubscriber.MaxMessageSize);

                if (!string.IsNullOrWhiteSpace(messageSubscriber.Emails))
                {
                    IDictionary<string, object> data = new Dictionary<string, object>();
                    data.Add("Title", messageSubscriber.Description);
                    data.Add("ItemErrors", q_ItemErrors);
                    string content = vmReporsitory.RenderTemplate(nVelocityTemplate, data);
                    emailMgr.AsyncSendEmail(messageSubscriber.Description, content, messageSubscriber.Emails, MailPriority.High);
                }
            }
        }

        protected string GetTLog<T>(T item, string message)
        {
            string logMessage = string.Empty;
            if (!string.IsNullOrWhiteSpace(message))
            {
                logMessage = message + @"
";
            }
            if (item != null)
            {
                PropertyInfo[] scheduleBodyPropertyInfo = typeof(T).GetProperties();
                foreach (PropertyInfo pi in scheduleBodyPropertyInfo)
                {
                    logMessage += pi.Name + ":";
                    logMessage += pi.GetValue(item, null) + @"
";
                }
                int length = logMessage.Length > 4000 ? 4000 : logMessage.Length;
                logMessage.Substring(0, length);
                return logMessage.Substring(0, length);
            }
            return logMessage;
        }
    }
}
