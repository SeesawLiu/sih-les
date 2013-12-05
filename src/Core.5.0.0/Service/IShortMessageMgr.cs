using System.Net.Mail;

namespace com.Sconit.Service
{
    public interface IShortMessageMgr
    {
        void SendMessage(string[] phones, string content);

        void AsyncSendMessage(string[] phones, string content);
    }
}
