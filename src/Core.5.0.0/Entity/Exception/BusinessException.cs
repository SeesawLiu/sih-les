using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.Sconit.Entity.Exception
{
    public class BusinessException : ApplicationException
    {
        private IList<Message> Messages { get; set; }

        public Boolean HasMessage
        {
            get
            {
                return Messages != null && Messages.Count > 0;
            }
        }

        public IList<Message> GetMessages()
        {
            return Messages;
        }

        public void AddMessage(Message message)
        {
            if (Messages == null)
            {
                Messages = new List<Message>();
            }
            Messages.Add(message);
        }

        public void AddMessage(string message)
        {
            if (Messages == null)
            {
                Messages = new List<Message>();
            }

            AddMessage(new Message(CodeMaster.MessageType.Error, message, null));
        }

        public void AddMessage(string message, params string[] messageParams)
        {
            if (Messages == null)
            {
                Messages = new List<Message>();
            }

            AddMessage(new Message(CodeMaster.MessageType.Error, message, messageParams));
        }

        public BusinessException()
            : base()
        {
        }

        public BusinessException(string message)
            : base(message)
        {
            AddMessage(new Message(CodeMaster.MessageType.Error, message, null));
        }

        public BusinessException(System.Exception ex)
            : base(string.Empty)
        {
            if (ex.InnerException != null)
            {
                AddMessage(ex.InnerException.InnerException != null
                               ? new Message(CodeMaster.MessageType.Error, ex.InnerException.InnerException.Message, null)
                               : new Message(CodeMaster.MessageType.Error, ex.InnerException.Message, null));
            }
            else
            {
                AddMessage(new Message(CodeMaster.MessageType.Error, ex.Message, null));
            }
        }

        public BusinessException(string message, params string[] messageParams)
            : base(message)
        {
            AddMessage(new Message(CodeMaster.MessageType.Error, message, messageParams));
        }

        public void AddMessages(List<Message> messages)
        {
            if (messages == null || messages.Count == 0)
                return;

            if (Messages == null)
            {
                Messages = new List<Message>();
            }

            foreach (var message in messages)
            {
                Messages.Add(message);
            }
        }
    }
}
