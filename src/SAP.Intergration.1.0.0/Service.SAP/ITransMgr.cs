using System;
using System.Collections.Generic;
using com.Sconit.Entity.SAP.TRANS;
using com.Sconit.Utility;

namespace com.Sconit.Service.SAP
{
    public interface ITransMgr
    {
        void ExchangeMoveType();

        //void ReExchangeMoveType(int batchNo);
        void ReExchangeMoveType();

        void ManuallyReExchangeMoveType(IList<Entity.SAP.TRANS.InvTrans> invTransList);

        void ManuallyCloseMoveType(IList<Entity.SAP.TRANS.InvTrans> invTransList);
    }

    public interface ITrans1p5Mgr
    {
        void ExchangeSAPTrans(List<ErrorMessage> errorMessageList, int batchNo, IList<object[]> tcodeMoveTypes, IList<Entity.MD.Region> regionList, IList<Entity.MD.Location> locationList);
    }

    public interface ITrans2Mgr
    {
        void ExchangeSAPMiscOrder(List<ErrorMessage> errorMessageList, int batchNo, IList<object[]> tcodeMoveTypes, IList<Entity.MD.Region> regionList, IList<Entity.MD.Location> locationList);

        void CreateInvTrans(IList<com.Sconit.Entity.INV.LocationTransaction> transList, List<ErrorMessage> errorMessageList, int batchNo, DateTime dateTimeNow, IList<object[]> tcodeMoveTypes, IList<Entity.MD.Region> regionList, IList<Entity.MD.Location> locationList, com.Sconit.Entity.SAP.ORD.TableIndex tableIndex);

        void CallSapTransService(IList<InvTrans> invTransList, IList<ErrorMessage> errorMessageList);
    }
}
