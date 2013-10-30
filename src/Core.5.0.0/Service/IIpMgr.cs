using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.SCM;

namespace com.Sconit.Service
{
    public interface IIpMgr
    {
        IpMaster TransferFlow2Ip(FlowMaster flowMaster, IList<OrderDetail> orderDetailList);
        IpMaster TransferOrder2Ip(IList<OrderMaster> orderMasterList);
        IpMaster MergeOrderMaster2IpMaster(IList<OrderMaster> orderMasterList);
        IpMaster TransferPickList2Ip(PickListMaster pickListMaster);
        void CreateIp(IpMaster ipMaster);
        void CreateIp(IpMaster ipMaster, DateTime effectiveDate);
        void ManualCloseIp(string IpNo);
        void ManualCloseIp(IpMaster ipMaster);
        void TryCloseIp(IpMaster ipMaster);
        void TryCloseExpiredScheduleLineIp(DateTime dateTime);
        void CancelIp(string IpNo);
        void CancelIp(string IpNo, DateTime effectiveDate);
        void CancelIp(IpMaster ipMaster);
        void CancelIp(IpMaster ipMaster, DateTime effectiveDate);
        bool TryCloseExpiredScheduleLineIpDetail(IpDetail ipDetail);
        bool TryResumeClosedScheduleLineIpDetail(IpDetail ipDetail);
        void BatchUpIpDetLocationTo(IList<IpDetail> updatedIpDetails);
    }
}
