using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using com.Sconit.Entity.SP.ORD;
using com.Sconit.Entity.SP;

namespace com.Sconit.Services.SP
{
    [ServiceContract]
    [ServiceKnownType(typeof(OrderMaster))]
    public interface IPublishing
    {
        //[OperationContract(IsOneWay = true)]
        //void Publish(OrderMaster e, string topicName);

        [OperationContract(IsOneWay = true)]
        void Publish(BaseEntity o);

    }
}
