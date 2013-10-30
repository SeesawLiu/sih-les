using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using com.Sconit.Service;

namespace com.Sconit.Services.SP
{
    [ServiceContract(CallbackContract = typeof(IPublishing))]
    public interface ISubscription
    {
        [OperationContract]
        void Subscribe(string GUID, string subType, string flow, string region, string userNm);
        //void Subscribe(string topicName);

        [OperationContract]
        void UnSubscribe(string GUID, string subType, string flow, string region, string userNm);
        //void UnSubscribe(string topicName);
    }
}
