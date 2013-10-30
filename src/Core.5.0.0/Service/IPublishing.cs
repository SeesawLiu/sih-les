using System.ServiceModel;
using com.Sconit.Entity;
using com.Sconit.Entity.ORD;
using com.Sconit.PrintModel;

namespace com.Sconit.Service
{
    [ServiceContract]
    [ServiceKnownType(typeof(OrderMaster))]
    public interface IPublishing
    {
        //[OperationContract(IsOneWay = true)]
        //void Publish(OrderMaster e, string topicName);

        [OperationContract(IsOneWay = true)]
        void Publish(PrintBase o);

    }
}
