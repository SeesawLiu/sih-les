using System;
using com.Sconit.Entity.BIL;
using com.Sconit.Entity.INV;
using com.Sconit.Entity.ORD;

namespace com.Sconit.Service
{
    public interface IBillMgr
    {
        PlanBill CreatePlanBill(ReceiptDetail receiptDetail, ReceiptDetailInput receiptDetailInput);
        PlanBill CreatePlanBill(ReceiptDetail receiptDetail, ReceiptDetailInput receiptDetailInput, DateTime effectiveDate);
        BillTransaction SettleBill(PlanBill planBill);
        BillTransaction SettleBill(PlanBill planBill, DateTime effectiveDate);
        BillTransaction VoidSettleBill(ActingBill actingBill, PlanBill planBill, bool IsVoidPlanBill);
        void VoidPlanBill(PlanBill planBill);
        void CancelVoidPlanBill(PlanBill planBill);
        PlanBill LoadPlanBill(string item, string location, string consignmentSupplier, DateTime effectiveDate);
    }
}
