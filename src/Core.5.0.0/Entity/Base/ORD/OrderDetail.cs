using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.INV;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.ORD
{
    [Serializable]
    public partial class OrderDetail : EntityBase, IAuditable
    {
        #region O/R Mapping Properties

        [Export(ExportName = "ProcurementDetailXls", ExportSeq = 10)]//要货明细
        [Export(ExportName = "ExportShipDetail", ExportSeq = 10)]
        [Display(Name = "OrderDetail_Id", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Int32 Id { get; set; }


        [Export(ExportName = "ExportSupplierDetXLS", ExportSeq = 10)]
        [Export(ExportName = "ExportPreviewXLS", ExportSeq = 80)]
        [Export(ExportName = "ProcurementDetailXls", ExportSeq = 20)]//要货明细
        [Export(ExportName = "ExportShipDetail", ExportSeq = 20)]
        [Export(ExportName = "ExportSeqDet", ExportSeq = 10)]
        [Display(Name = "OrderDetail_OrderNo", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string OrderNo { get; set; }

        public com.Sconit.CodeMaster.OrderType OrderType { get; set; }

        public com.Sconit.CodeMaster.OrderSubType OrderSubType { get; set; }

        [Export(ExportName = "ExportSeqDet", ExportSeq = 1)]
        [Display(Name = "OrderDetail_Sequence", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Int32 Sequence { get; set; }

        [Export(ExportName = "ExportSupplierDetXLS", ExportSeq = 20)]
        [Export(ExportName = "ExportPreviewXLS", ExportSeq = 30)]
        [Export(ExportName = "ProcurementDetailXls", ExportSeq = 50)]//要货明细
        [Export(ExportName = "ExportShipDetail", ExportSeq = 50)]
        [Export(ExportName = "ExportSeqDet", ExportSeq = 90)]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "OrderDetail_Item", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string Item { get; set; }

        [Export(ExportName = "ExportSupplierDetXLS", ExportSeq = 40)]
        [Export(ExportName = "ExportPreviewXLS", ExportSeq = 50)]
        [Export(ExportName = "ProcurementDetailXls", ExportSeq = 70)]//要货明细
        [Export(ExportName = "ExportShipDetail", ExportSeq = 70)]
        [Export(ExportName = "ExportSeqDet", ExportSeq = 130)]
        [Display(Name = "OrderDetail_ItemDescription", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ItemDescription { get; set; }

        [Export(ExportName = "ExportSupplierDetXLS", ExportSeq = 30)]
        [Export(ExportName = "ExportPreviewXLS", ExportSeq = 40)]
        [Export(ExportName = "ProcurementDetailXls", ExportSeq = 60)]//要货明细
        [Export(ExportName = "ExportShipDetail", ExportSeq = 60)]
        [Export(ExportName = "ExportSeqDet", ExportSeq = 100)]
        [Display(Name = "OrderDetail_ReferenceItemCode", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ReferenceItemCode { get; set; }

        public string BaseUom { get; set; }

        [Export(ExportName = "ExportSupplierDetXLS", ExportSeq = 50)]
        [Export(ExportName = "ExportPreviewXLS", ExportSeq = 70)]
        [Export(ExportName = "ProcurementDetailXls", ExportSeq = 80)]//要货明细
        [Export(ExportName = "ExportShipDetail", ExportSeq = 80)]
        [Display(Name = "OrderDetail_Uom", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string Uom { get; set; }

        // [Display(Name = "OrderDetail_PartyFrom", ResourceType = typeof(Resources.ORD.OrderDetail))]
        //public string PartyFrom { get; set; }

         [Display(Name = "OrderDetail_PartyTo", ResourceType = typeof(Resources.ORD.OrderDetail))]
         public string PartyTo { get; set; }

         [Export(ExportName = "ExportSupplierDetXLS", ExportSeq = 60)]
         [Export(ExportName = "ExportPreviewXLS", ExportSeq = 60)]
         [Export(ExportName = "ExportShipDetail", ExportSeq = 90)]
         [Export(ExportName = "ProcurementDetailXls", ExportSeq = 85)]//要货明细
         [Display(Name = "OrderDetail_UnitCount", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Decimal UnitCount { get; set; }
         [Export(ExportName = "ExportSupplierDetXLS", ExportSeq = 70)]
         [Display(Name = "OrderDetail_UnitCountDescription", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string UnitCountDescription { get; set; }
         [Export(ExportName = "ProcurementDetailXls", ExportSeq = 90)]//要货明细
         [Display(Name = "OrderDetail_MinUnitCount", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Decimal MinUnitCount { get; set; }

        public com.Sconit.CodeMaster.QualityType QualityType { get; set; }

        [Export(ExportName = "ExportPreviewXLS", ExportSeq = 110)]
        [Export(ExportName = "ProcurementDetailXls", ExportSeq = 210)]//要货明细
        [Export(ExportName = "ExportShipDetail", ExportSeq = 200)]
        [Export(ExportName = "ExportSeqDet", ExportSeq = 140)]
        [Display(Name = "OrderDetail_ManufactureParty", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ManufactureParty { get; set; }

        [Display(Name = "OrderDetail_RequiredQty", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Decimal RequiredQty { get; set; }

        [Export(ExportName = "ExportSupplierDetXLS", ExportSeq = 130)]
        [Export(ExportName = "ExportPreviewXLS", ExportSeq = 140)]
        [Export(ExportName = "ProcurementDetailXls", ExportSeq = 130)]//要货明细
        [Export(ExportName = "ExportShipDetail", ExportSeq = 120)]
        [Export(ExportName = "ExportSeqDet", ExportSeq = 120)]
        [Display(Name = "OrderDetail_OrderedQty", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Decimal OrderedQty { get; set; }

        [Export(ExportName = "ExportSupplierDetXLS", ExportSeq = 140)]
        [Export(ExportName = "ProcurementDetailXls", ExportSeq = 140)]//要货明细
        [Export(ExportName = "ExportShipDetail", ExportSeq = 130)]
        [Display(Name = "OrderDetail_ShippedQty", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Decimal ShippedQty { get; set; }

        [Export(ExportName = "ExportSupplierDetXLS", ExportSeq = 150)]
        [Export(ExportName = "ProcurementDetailXls", ExportSeq = 150)]//要货明细
        [Export(ExportName = "ExportShipDetail", ExportSeq = 140)]
        [Display(Name = "OrderDetail_ReceivedQty", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Decimal ReceivedQty { get; set; }

        [Display(Name = "OrderDetail_RejectedQty", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Decimal RejectedQty { get; set; }

        [Display(Name = "OrderDetail_ScrapQty", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Decimal ScrapQty { get; set; }

        [Display(Name = "OrderDetail_PickedQty", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Decimal PickedQty { get; set; }

        [Display(Name = "OrderDetail_UnitQty", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Decimal UnitQty { get; set; }

        [Display(Name = "OrderDetail_ReceiveLotSize", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Decimal? ReceiveLotSize { get; set; }

        [Export(ExportName = "ExportPreviewXLS", ExportSeq = 90)]
        [Display(Name = "OrderDetail_LocationFrom", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string LocationFrom { get; set; }

        [Display(Name = "OrderDetail_LocationFromName", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string LocationFromName { get; set; }

        [Export(ExportName = "ExportSupplierDetXLS", ExportSeq = 110)]
        [Export(ExportName = "ExportPreviewXLS", ExportSeq = 120)]
        [Export(ExportName = "ProcurementDetailXls", ExportSeq = 110)]//要货明细
        [Export(ExportName = "ExportShipDetail", ExportSeq = 100)]
        [Display(Name = "OrderDetail_LocationTo", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string LocationTo { get; set; }

        [Display(Name = "OrderDetail_LocationToName", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string LocationToName { get; set; }

        [Display(Name = "OrderDetail_IsInspect", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Boolean IsInspect { get; set; }

        //[Display(Name = "OrderDetail_InspectLocation", ResourceType = typeof(Resources.ORD.OrderDetail))]
        //public string InspectLocation { get; set; }

        //[Display(Name = "OrderDetail_InspectLocationName", ResourceType = typeof(Resources.ORD.OrderDetail))]
        //public string InspectLocationName { get; set; }

        //[Display(Name = "OrderDetail_RejectLocation", ResourceType = typeof(Resources.ORD.OrderDetail))]
        //public string RejectLocation { get; set; }

        //[Display(Name = "OrderDetail_RejectLocationName", ResourceType = typeof(Resources.ORD.OrderDetail))]
        //public string RejectLocationName { get; set; }

        [Display(Name = "OrderDetail_BillAddress", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string BillAddress { get; set; }

        [Display(Name = "OrderDetail_BillAddressDescription", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string BillAddressDescription { get; set; }

        [Display(Name = "OrderDetail_PriceList", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string PriceList { get; set; }

        [Display(Name = "OrderDetail_UnitPrice", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Decimal? UnitPrice { get; set; }

        [Display(Name = "OrderDetail_IsProvisionalEstimate", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Boolean IsProvisionalEstimate { get; set; }

        [Display(Name = "OrderDetail_Tax", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string Tax { get; set; }

        [Display(Name = "OrderDetail_IsIncludeTax", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Boolean IsIncludeTax { get; set; }

        [Display(Name = "OrderDetail_Bom", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string Bom { get; set; }

        [Display(Name = "OrderDetail_Routing", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string Routing { get; set; }

        //[Display(Name = "OrderDetail_ProductionScan", ResourceType = typeof(Resources.ORD.OrderDetail))]
        //public string ProductionScan { get; set; }

        //[Display(Name = "OrderDetail_HuLotSize", ResourceType = typeof(Resources.ORD.OrderDetail))]
        //public Decimal? HuLotSize { get; set; }

        [Display(Name = "OrderDetail_BillTerm", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public com.Sconit.CodeMaster.OrderBillTerm BillTerm { get; set; }

        [Export(ExportName = "ProcurementDetailXls", ExportSeq = 250)]//要货明细
        [Display(Name = "OrderDetail_Remark", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ZOPWZ { get; set; }
        public string ZOPID { get; set; }
        public string ZOPDS { get; set; }

        public Int32 CreateUserId { get; set; }

        [Display(Name = "OrderDetail_CreateUserName", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string CreateUserName { get; set; }

        [Display(Name = "OrderDetail_CreateDate", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public DateTime CreateDate { get; set; }

        public Int32 LastModifyUserId { get; set; }
        [Display(Name = "OrderDetail_LastModifyUserName", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string LastModifyUserName { get; set; }

        [Display(Name = "OrderDetail_LastModifyDate", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public DateTime LastModifyDate { get; set; }

        [Display(Name = "OrderDetail_Version", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Int32 Version { get; set; }
         [Display(Name = "OrderDetail_Container", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string Container { get; set; }
         [Export(ExportName = "ProcurementDetailXls", ExportSeq = 100)]//要货明细
         [Display(Name = "OrderDetail_ContainerDescription", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ContainerDescription { get; set; }

        public string Currency { get; set; }

        public string PickStrategy { get; set; }

        public string ExtraDemandSource { get; set; }

        public Boolean IsScanHu { get; set; }
        [Export(ExportName = "ExportPreviewXLS", ExportSeq = 10)]
        [Export(ExportName = "ProcurementDetailXls", ExportSeq = 30, ExportTitle = "OrderDetail_ExternalOrderNo_ExportDet", ExportTitleResourceType = typeof(Resources.ORD.OrderDetail))]
        [Export(ExportName = "ExportShipDetail", ExportSeq = 30, ExportTitle = "OrderDetail_ExternalOrderNo_ExportDet", ExportTitleResourceType = typeof(Resources.ORD.OrderDetail))]
        [Display(Name = "OrderDetail_ExternalOrderNo_Export", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ExternalOrderNo { get; set; }

        [Export(ExportName = "ExportPreviewXLS", ExportSeq = 20)]
        [Export(ExportName = "ProcurementDetailXls", ExportSeq = 40, ExportTitle = "OrderDetail_ExternalSequence_ExportDet", ExportTitleResourceType = typeof(Resources.ORD.OrderDetail))]
        [Export(ExportName = "ExportShipDetail", ExportSeq = 40, ExportTitle = "OrderDetail_ExternalSequence_ExportDet", ExportTitleResourceType = typeof(Resources.ORD.OrderDetail))]
        [Display(Name = "OrderDetail_ExternalSequence", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ExternalSequence { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public CodeMaster.ScheduleType ScheduleType { get; set; }

        [Export(ExportName = "ProcurementDetailXls", ExportSeq = 45)]//要货明细
        [Export(ExportName = "ExportSeqDet", ExportSeq = 70)]
        [Display(Name = "OrderDetail_ExternalOrderNo_Export", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ReserveNo { get; set; }

        [Export(ExportName = "ExportSeqDet", ExportSeq = 80)]
        [Display(Name = "OrderDetail_ExternalSequence", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ReserveLine { get; set; }

        [Export(ExportName = "ExportShipDetail", ExportSeq = 90)]
        [Export(ExportName = "ExportSeqDet", ExportSeq = 110)]
        [Display(Name = "OrderDetail_BinTo", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string BinTo { get; set; }
        [Display(Name = "OrderDetail_WMSSeq", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string WMSSeq { get; set; }

        public Boolean IsChangeUnitCount { get; set; }

        public string AUFNR { get; set; }
        [Display(Name = "OrderDetail_ConsignmentSupplier", ResourceType = typeof(Resources.ORD.PickListDetail))]
        public string ICHARG { get; set; }
        public string BWART { get; set; }

        [Display(Name = "OrderMaster_IsCreatePickList", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public Boolean IsCreatePickList { get; set; }
        #endregion

        public override int GetHashCode()
        {
            if (Id != 0)
            {
                return Id.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            OrderDetail another = obj as OrderDetail;

            if (another == null)
            {
                return false;
            }
            else
            {
                return (this.Id == another.Id);
            }
        }
    }

}
