using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.Util;

namespace com.Sconit.Service.Impl
{
    public class ShortMessageMgrImpl : BaseMgr, IShortMessageMgr
    {
        public ISystemMgr systemMgr { get; set; }
        public string userName { get; set; }
        public string userPassword { get; set; }
        public string app { get; set; }
        public string sourceIdentifier { get; set; }

        public void SendMessage(string[] phones, string content)
        {
            MSWebService.MSWebService msWebService = new MSWebService.MSWebService();
            string MSWebServiceAddress = systemMgr.GetEntityPreferenceValue(com.Sconit.Entity.SYS.EntityPreference.CodeEnum.MSWebServiceAddress);
            string MSWebServicePort = systemMgr.GetEntityPreferenceValue(com.Sconit.Entity.SYS.EntityPreference.CodeEnum.MSWebServicePort);
            msWebService.Url = ServiceURLHelper.ReplaceServiceUrl(msWebService.Url, MSWebServiceAddress, MSWebServicePort);
            MSWebService.SecurityHeader header = new MSWebService.SecurityHeader();
            header.UserName = userName;
            header.UserPassword = userPassword;
            msWebService.SecurityHeaderValue = header;

            MSWebService.MSRequest msRequest = new MSWebService.MSRequest();
            msRequest.APP = app;//调用服务的应用程序
            msRequest.SourceIdentifier = sourceIdentifier;//调用端的唯一ID,以方便以后核对
            msRequest.Phones = phones;
            msRequest.Content = content;
            //msRequest.Remark = "备注";//备注内容(暂未启用)
            msWebService.SendMessage(msRequest);

            msWebService.Dispose();
        }

        public void AsyncSendMessage(string[] phones, string content)
        {
            AsyncSend asyncSend = new AsyncSend(this.SendMessage);
            asyncSend.BeginInvoke(phones, content, null, null);
        }

        public delegate void AsyncSend(string[] phones, string content);

    }
}
