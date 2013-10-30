using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.Service.SD;
using System.Web.Services.Protocols;
using com.Sconit.Entity.Exception;

namespace com.Sconit.WebService.SD
{
    public class BaseWebService : com.Sconit.WebService.BaseWebService
    {
        protected IOrderMgr orderMgr { get { return GetService<IOrderMgr>(); } }

        protected IInventoryMgr inventoryMgr { get { return GetService<IInventoryMgr>(); } }

        protected ISecurityMgr sdSecurityMgr { get { return GetService<ISecurityMgr>(); } }

        protected IFlowMgrImpl flowMgr { get { return GetService<IFlowMgrImpl>(); } }

        protected IMasterDataMgrImpl masterDataMgr { get { return GetService<IMasterDataMgrImpl>(); } }

        protected IPickTaskMgr pickTaskMgr { get { return GetService<IPickTaskMgr>(); } }

        protected void ProcessException(Exception ex)
        {
            if (ex is BusinessException)
            {
                throw new SoapException(ex.Message, SoapException.ServerFaultCode, string.Empty);
            }
            else
            {
                throw new SoapException(ex.Message, SoapException.ServerFaultCode, string.Empty);
            }
        }
    }
}
