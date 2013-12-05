using System.Net.Mail;
using System.Web.Services;
using com.Sconit.Service;

namespace com.Sconit.WebService
{
    [WebService(Namespace = "http://com.Sconit.WebService.SMSService/")]
    public class ShortMessageService : BaseWebService
    {
        private IShortMessageMgr shortMessageMgr
        {
            get
            {
                return GetService<IShortMessageMgr>();
            }
        }

        [WebMethod]
        public void AsyncSend(string[] phones, string content)
        {
            shortMessageMgr.AsyncSendMessage(phones, content);
        }

        [WebMethod]
        public void Send(string[] phones, string content)
        {
            shortMessageMgr.SendMessage(phones, content);
        }
    }
}
