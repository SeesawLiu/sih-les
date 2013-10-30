using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.Entity.SAP.ORD;

namespace com.Sconit.Service.SAP
{
    public interface IDistributionMgr
    {
        string AlterDistributionOrder(IList<AlterDO> alterDOs);

        void PostDistributionOrder();

        void RePostDistributionOrder();
    }
}
