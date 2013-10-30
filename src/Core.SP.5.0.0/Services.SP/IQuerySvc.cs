using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using com.Sconit.PrintModel.ORD;

namespace com.Sconit.Services.SP
{
    [ServiceContract(Name = "QueryService")]
    public interface IQuerySvc
    {
        [OperationContract]
        PrintOrderMaster GetOrderMstr(string OrderNo);

        [OperationContract]
        IList<PrintOrderDetail> GetOrderDets(string OrderNo);
    }
}
