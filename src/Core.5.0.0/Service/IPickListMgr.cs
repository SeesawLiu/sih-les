using System.Collections.Generic;
using com.Sconit.Entity.ORD;
using System;

namespace com.Sconit.Service
{
    public interface IPickListMgr
    {
        string CreatePickList4Qty(string deliveryGroup, string[] orderDetailIdArray, string[] pickQtyArray);

        void CancelPickList4Qty(string pickListNo);

        PickListMaster CreatePickList(IList<OrderDetail> orderDetailList);

        //void ReleasePickList(string pickListNo);

        //void ReleasePickList(PickListMaster pickListMaster);

        void CancelPickList(string pickListNo);

        void CancelPickList(PickListMaster pickListMaster);

        void StartPickList(string pickListNo);

        void StartPickList(PickListMaster pickListMaster);

        void DoPick(IList<PickListDetail> pickListDetailList);

        void DeletePickListResult(IList<PickListResult> pickListResultList);
    }
}
