using System;
using System.Collections.Generic;
using System.Linq;
using com.Sconit.Entity.INV;
using System.ComponentModel.DataAnnotations;

//TODO: Add other using statements here

namespace com.Sconit.Entity.ORD
{
    public partial class ReceiptDetail
    {
        #region Non O/R Mapping Properties
        public string CurrentPartyFrom { get; set; }
        public string CurrentPartyFromName { get; set; }
        public string CurrentPartyTo { get; set; }
        public string CurrentPartyToName { get; set; }
        public string CurrentExternalReceiptNo { get; set; }
        public com.Sconit.CodeMaster.OccupyType CurrentOccupyType { get; set; }
        public string CurrentOccupyReferenceNo { get; set; }
        public bool CurrentIsReceiveScanHu { get; set; }

        public IList<ReceiptLocationDetail> ReceiptLocationDetails { get; set; }
        public IList<ReceiptDetailInput> ReceiptDetailInputs { get; set; }
        public bool IsVoid { get; set; }
        public string LotNo { get; set; }

        public decimal ReceiveQtyInput
        {
            get
            {
                if (ReceiptDetailInputs != null
                    && ReceiptDetailInputs.Count > 0)
                {
                    return ReceiptDetailInputs.Sum(i => i.ReceiveQty);
                }
                return 0;
            }
        }

        public decimal ScrapQtyInput
        {
            get
            {
                if (ReceiptDetailInputs != null
                    && ReceiptDetailInputs.Count > 0)
                {
                    return ReceiptDetailInputs.Sum(i => i.ScrapQty);
                }
                return 0;
            }
        }

        //public decimal RejectQtyInput
        //{
        //    get
        //    {
        //        if (ReceiptDetailInputs != null
        //            && ReceiptDetailInputs.Count > 0)
        //        {
        //            return ReceiptDetailInputs.Sum(i => i.RejectQty);
        //        }
        //        return 0;
        //    }
        //}

        //public decimal ReceiveAndRejectQtyInput
        //{
        //    get
        //    {
        //        if (ReceiptDetailInputs != null
        //            && ReceiptDetailInputs.Count > 0)
        //        {
        //            return ReceiptDetailInputs.Sum(i => i.ReceiveQty) + ReceiptDetailInputs.Sum(i => i.RejectQty);
        //        }
        //        return 0;
        //    }
        //}
        #endregion

        #region 查询导出显示列 辅助字段

        [Display(Name = "OrderMaster_PartyFrom", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string MastPartyFrom { get; set; }

        [Display(Name = "OrderMaster_PartyTo", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string MastPartyTo { get; set; }


        [Display(Name = "OrderMaster_Type", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string MastType { get; set; }

        [Display(Name = "OrderMaster_Status", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string MastStatus { get; set; }

        [Display(Name = "OrderMaster_CreateDate", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public DateTime MastCreateDate { get; set; }

        [Display(Name = "Location_SAPLocation", ResourceType = typeof(Resources.MD.Location))]
        public string SAPLocation { get; set; }
        #endregion

        public void AddReceiptLocationDetail(ReceiptLocationDetail receiptLocationDetail)
        {
            if (ReceiptLocationDetails == null)
            {
                ReceiptLocationDetails = new List<ReceiptLocationDetail>();
            }
            ReceiptLocationDetails.Add(receiptLocationDetail);
        }

        public void AddReceiptLocationDetail(IList<ReceiptLocationDetail> receiptLocationDetailList)
        {
            if (receiptLocationDetailList != null)
            {
                if (ReceiptLocationDetails == null)
                {
                    ReceiptLocationDetails = new List<ReceiptLocationDetail>();
                }
                ((List<ReceiptLocationDetail>)ReceiptLocationDetails).AddRange(receiptLocationDetailList);
            }
        }

        public void AddReceiptDetailInput(ReceiptDetailInput receiptDetailInput)
        {
            if (ReceiptDetailInputs == null)
            {
                ReceiptDetailInputs = new List<ReceiptDetailInput>();
            }
            ReceiptDetailInputs.Add(receiptDetailInput);
        }
    }

    public class ReceiptDetailInput
    {
        public decimal ReceiveQty { get; set; }
        //给生产收货用
        public com.Sconit.CodeMaster.QualityType QualityType { get; set; }
        public decimal ScrapQty { get; set; }
        //public decimal RejectQty { get; set; }
        public string HuId { get; set; }
        public string LotNo { get; set; }
        public bool IsCreatePlanBill { get; set; }
        public bool IsConsignment { get; set; }
        public int? PlanBill { get; set; }
        public int? ActingBill { get; set; }
        public bool IsFreeze { get; set; }
        public bool IsATP { get; set; }
        public com.Sconit.CodeMaster.OccupyType OccupyType { get; set; }
        public string OccupyReferenceNo { get; set; }
        public string SequenceNo { get; set; }
        //WMS收货单行号
        public string WMSRecSeq { get; set; }

        public IList<IpLocationDetail> ReceivedIpLocationDetailList { get; set; }
        public void AddIpLocationDetail(IpLocationDetail ipLocationDetail)
        {
            if (ReceivedIpLocationDetailList == null)
            {
                ReceivedIpLocationDetailList = new List<IpLocationDetail>();
            }

            ReceivedIpLocationDetailList.Add(ipLocationDetail);
        }
    }
}