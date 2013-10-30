using System;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.SCM;
using System.Collections.Generic;
using com.Sconit.Utility;
using com.Sconit.Entity.CUST;

namespace com.Sconit.Service.SAP
{
    public interface IProcurementMgr
    {
        List<com.Sconit.Entity.SAP.ORD.OrderDetail> GetProcOrders(string flow, string supplier, string item, string plant);

        //void CreateProcOrder(FlowMaster flowMaster, LeanEngine.Entity.Orders order);

        List<object[]> GetScheduleLineItem(string item, string supplier, string plant);
        //void CreateProcOrder(OrderMaster orderMaster);

        void CreateSAPScheduleLineFromLes();

        void CRSLSummaryFromLes();
    }

    public interface IProcurementOperatorMgr
    {
        void CreateOneCRSL(com.Sconit.Entity.SAP.ORD.CreateScheduleLine createScheduleLine, IList<ErrorMessage> errorMessageList);
    }
}
