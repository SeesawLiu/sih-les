using com.Sconit.Entity.ORD;
using System.Collections.Generic;
using System;
using System.IO;

namespace com.Sconit.Service
{
    public interface IMiscOrderMgr
    {
        void CreateOrUpdateMiscOrderAndDetails(MiscOrderMaster miscOrderMaster,
            IList<MiscOrderDetail> addMiscOrderDetailList, IList<MiscOrderDetail> updateMiscOrderDetailList, IList<MiscOrderDetail> deleteMiscOrderDetailList);

        void CreateMiscOrder(MiscOrderMaster miscOrderMaster);

        void QuickCreateMiscOrder(MiscOrderMaster miscOrderMaster, DateTime effectiveDate);

        void UpdateMiscOrder(MiscOrderMaster miscOrderMaster);

        void BatchUpdateMiscOrderDetails(string miscOrderNo, IList<MiscOrderDetail> addMiscOrderDetailList, IList<MiscOrderDetail> updateMiscOrderDetailList, IList<MiscOrderDetail> deleteMiscOrderDetailList);
        void BatchUpdateMiscOrderDetails(MiscOrderMaster miscOrderMaster, IList<MiscOrderDetail> addMiscOrderDetailList, IList<MiscOrderDetail> updateMiscOrderDetailList, IList<MiscOrderDetail> deleteMiscOrderDetailList);

        void BatchUpdateMiscOrderDetails(string miscOrderNo, IList<string> addHuIdList, IList<string> deleteHuIdList);
        void BatchUpdateMiscOrderDetails(MiscOrderMaster miscOrderMaster, IList<string> addHuIdList, IList<string> deleteHuIdList);

        void DeleteMiscOrder(string miscOrderNo);
        void DeleteMiscOrder(MiscOrderMaster miscOrderMaster);

        void CloseMiscOrder(string miscOrderNo);
        void CloseMiscOrder(string miscOrderNo, DateTime effectiveDate);
        void CloseMiscOrder(MiscOrderMaster miscOrderMaster);
        void CloseMiscOrder(MiscOrderMaster miscOrderMaster, DateTime effectiveDate);

        void CancelMiscOrder(string miscOrderNo);
        void CancelMiscOrder(string miscOrderNo, DateTime effectiveDate);
        void CancelMiscOrder(MiscOrderMaster miscOrderMaster);
        void CancelMiscOrder(MiscOrderMaster miscOrderMaster, DateTime effectiveDate);

        void CreateMiscOrderDetailFromXls(Stream inputStream, string miscOrderNo);
        void Import261262MiscOrder(Stream inputStream);

        void CreateMiscInvInitDetailFromXls(Stream inputStream, string miscOrderNo);

        void BatchUpdateMiscOrderDetailsAndClose(string miscOrderNo,
            IList<MiscOrderDetail> addMiscOrderDetailList, IList<MiscOrderDetail> updateMiscOrderDetailList, IList<MiscOrderDetail> deleteMiscOrderDetailList);
    }
}
