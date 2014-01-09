using System;
using System.Collections.Generic;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.SAP.ORD;
using com.Sconit.Entity.SCM;
using com.Sconit.Utility;

namespace com.Sconit.Service.SAP
{
    public interface IProductionMgr
    {
        void AutoCreateVanOrder(string prodLine);

        IList<string> UpdateVanOrder(string plant, string sapOrderNo, string prodlLine);

        void GetCurrentVanOrder(string plant, string sapOrderNo, string prodlLine);

        void ReportProdOrderOperation();

        void ReportProdOrderOperation(IList<ProdOpReport> prodOpReportList);

        IList<string> GetProductOrder(string plant, IList<string> sapOrderNoList);

        IList<string> GetProductOrder(string plant, IList<string> sapOrderTypeList, DateOption dateOption, DateTime? dateFrom, DateTime? dateTo, IList<string> mrpCtrlList);

        IList<string> GetCKDProductOrder(string plant, IList<string> sapOrderNoList, string sapProdLine, string sapOrderType);

        void BackflushProductionOrder();
    }

    public interface IReportProdOrderOperationMgr
    {
        void ReportProdOrderOperation(ProdOpReport prodOpReport, IList<ErrorMessage> errorMessageList, int maxFailCount);

        void CancelReportProdOrderOperation(string AUFNR, string TEXT);
    }

    public enum DateOption
    {
        EQ = 1,
        GT = 2,
        GE = 3,
        LT = 4,
        LE = 5,
        BT = 6,
    }
}
