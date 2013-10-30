using System.Collections.Generic;
using System.Linq;
using com.Sconit.Entity.INV;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;
using System;

//TODO: Add other using statements here

namespace com.Sconit.Entity.ORD
{
    public partial class IpDetail
    {
        #region Non O/R Mapping Properties       
        public decimal ShipQtyInput
        {
            get
            {
                if (IpDetailInputs != null
                    && IpDetailInputs.Count > 0)
                {
                    return IpDetailInputs.Sum(i => i.ShipQty);
                }
                return 0;
            }
        }

        public decimal ReceiveQtyInput
        {
            get
            {
                if (IpDetailInputs != null
                    && IpDetailInputs.Count > 0)
                {
                    return IpDetailInputs.Sum(i => i.ReceiveQty);
                }
                return 0;
            }
        }     

        public decimal RemainReceiveQty
        {
            get
            {
                return this.Qty - this.ReceivedQty;
            }
        }

        public string CurrentPartyFrom { get; set; }
        public string CurrentPartyFromName { get; set; }
        public string CurrentPartyTo { get; set; }
        public string CurrentPartyToName { get; set; }

        public bool IsVoid { get; set; }
        public IList<IpLocationDetail> IpLocationDetails { get; set; }
        public IList<IpDetailInput> IpDetailInputs { get; set; }

        [Display(Name = "IpDetail_CurrentReceiveQty", ResourceType = typeof(Resources.ORD.IpDetail))]
        public decimal CurrentReceiveQty { get; set; }

        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.IpDetailType, ValueField = "Type")]
        [Display(Name = "IpDetail_Type", ResourceType = typeof(Resources.ORD.IpDetail))]
        public string TypeDescription { get; set; }

        [Display(Name = "IpDetail_LotNo", ResourceType = typeof(Resources.ORD.IpDetail))]
        public string LotNo { get; set; }

        [Display(Name = "IpDetail_HuQty", ResourceType = typeof(Resources.ORD.IpDetail))]
        public decimal HuQty { get; set; }

        [Display(Name = "IpDetail_IsCancel", ResourceType = typeof(Resources.ORD.IpDetail))]
        public Boolean IsCancel { get; set; }

       
        #endregion

        [Display(Name = "IpDetail_SupplierLotNo", ResourceType = typeof(Resources.ORD.IpDetail))]
        public String SupplierLotNo { get; set; }

        public string SAPLocationTo { get; set; }

        #region 查询导出显示列 辅助字段

        [Display(Name = "OrderMaster_PartyFrom", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string MastPartyFrom { get; set; }

        [Display(Name = "OrderMaster_PartyTo", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string MastPartyTo { get; set; }

        [Display(Name = "OrderMaster_Flow", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string MastFlow { get; set; }

        [Display(Name = "OrderMaster_Type", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string MastType { get; set; }

        [Display(Name = "OrderMaster_Status", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string MastStatus { get; set; }

        [Display(Name = "OrderMaster_CreateDate", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public DateTime MastCreateDate { get; set; }

        [Display(Name = "Location_SAPLocation", ResourceType = typeof(Resources.MD.Location))]
        public string SAPLocation { get; set; }
        #endregion

        #region methods
        public void AddIpLocationDetail(IpLocationDetail ipLocationDetail)
        {
            if (IpLocationDetails == null)
            {
                IpLocationDetails = new List<IpLocationDetail>();
            }
            IpLocationDetails.Add(ipLocationDetail);
        }

        public void AddIpLocationDetail(IList<IpLocationDetail> ipLocationDetailList)
        {
            if (ipLocationDetailList != null)
            {
                if (IpLocationDetails == null)
                {
                    IpLocationDetails = new List<IpLocationDetail>();
                }
                ((List<IpLocationDetail>)IpLocationDetails).AddRange(ipLocationDetailList);
            }
        }

        public void AddIpDetailInput(IpDetailInput ipDetailInput)
        {
            if (IpDetailInputs == null)
            {
                IpDetailInputs = new List<IpDetailInput>();
            }
            IpDetailInputs.Add(ipDetailInput);
        }
        #endregion

        public decimal AllowRecQty { get; set; }
    }

    public class IpDetailInput
    {
        public decimal ShipQty { get; set; }
        public decimal ReceiveQty { get; set; }
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
        public string Bin { get; set; }
        public string SequenceNo { get; set; }
        public string ConsignmentParty { get; set; }
        /// <summary>
        /// WMS收货单号
        /// </summary>
        public string WMSRecNo { get; set; }
        /// <summary>
        /// WMS收货单行号
        /// </summary>
        public string WMSRecSeq { get; set; }
        /// <summary>
        /// WMS发货单行号
        /// </summary>
        public string WMSIpSeq { get; set; }

        public IList<IpLocationDetail> ReceivedIpLocationDetailList { get; set; }

        public void AddReceivedIpLocationDetail(IpLocationDetail receivedIpLocationDetail)
        {
            if (ReceivedIpLocationDetailList == null)
            {
                ReceivedIpLocationDetailList = new List<IpLocationDetail>();
            }

            ReceivedIpLocationDetailList.Add(receivedIpLocationDetail);
        }
    }
}