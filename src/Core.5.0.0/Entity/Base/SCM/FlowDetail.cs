using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.SCM
{
    [Serializable]
    public partial class FlowDetail : EntityBase, IAuditable
    {
        #region O/R Mapping Properties

        [Display(Name = "FlowDetail_Id", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Int32 Id { get; set; }

        [Export(ExportSeq = 20, ExportName = "ExportKanbanScanXLS")]
        [Export(ExportSeq = 10, ExportName = "ExportCheckFlowXLS")]
        [Export(ExportSeq=10,ExportName="ExportList")]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "FlowDetail_Flow", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string Flow { get; set; }

        [Display(Name = "FlowDetail_Strategy", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public com.Sconit.CodeMaster.FlowStrategy Strategy { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "FlowDetail_Sequence", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Int32 Sequence { get; set; }

        [Export(ExportSeq = 10, ExportName = "ExportKanbanScanXLS")]
        [Export(ExportSeq = 80, ExportName = "ExportCheckFlowXLS")]
        [Export(ExportSeq = 20, ExportName = "ExportList")]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "FlowDetail_Item", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string Item { get; set; }

        [Export(ExportSeq = 30, ExportName = "ExportKanbanScanXLS")]
        [Export(ExportSeq = 90, ExportName = "ExportCheckFlowXLS")]
        [Export(ExportSeq = 30, ExportName = "ExportList")]
        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "FlowDetail_ReferenceItemCode", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string ReferenceItemCode { get; set; }

        public string BaseUom { get; set; }

        [Export(ExportSeq = 100, ExportName = "ExportKanbanScanXLS")]
        [Export(ExportSeq = 70, ExportName = "ExportList")]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "FlowDetail_Uom", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string Uom { get; set; }

        [Export(ExportSeq = 80, ExportName = "ExportList")]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Range(1, 1000000, ErrorMessageResourceName = "Errors_Common_FieldEquals", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "FlowDetail_UnitCount", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Decimal UnitCount { get; set; }

        [Export(ExportSeq = 90, ExportName = "ExportList")]
        [Display(Name = "FlowDetail_UnitCountDescription", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string UnitCountDescription { get; set; }

        [Export(ExportSeq = 50, ExportName = "ExportKanbanScanXLS")]
        [Export(ExportSeq = 110, ExportName = "ExportCheckFlowXLS")]
        [Export(ExportSeq = 100, ExportName = "ExportList")]
        [Display(Name = "FlowDetail_MinUnitCount", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Decimal MinUnitCount { get; set; }


        [Display(Name = "FlowDetail_StartDate", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public DateTime? StartDate { get; set; }

        [Display(Name = "FlowDetail_EndDate", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public DateTime? EndDate { get; set; }

        //[Display(Name = "FlowDetail_HuLotSize", ResourceType = typeof(Resources.SCM.FlowDetail))]
        //public Decimal? HuLotSize { get; set; }

        [Display(Name = "FlowDetail_Bom", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string Bom { get; set; }

        [Export(ExportSeq = 50, ExportName = "ExportCheckFlowXLS")]
        [Export(ExportSeq = 130, ExportName = "ExportList")]
        [Display(Name = "FlowDetail_LocationFrom", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string LocationFrom { get; set; }

        [Export(ExportSeq = 60, ExportName = "ExportCheckFlowXLS")]
        [Export(ExportSeq = 140, ExportName = "ExportList")]
        [Display(Name = "FlowDetail_LocationTo", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string LocationTo { get; set; }

        //[Display(Name = "FlowDetail_InspectLocationFrom", ResourceType = typeof(Resources.SCM.FlowDetail))]
        //public string InspectLocationFrom { get; set; }

        //[Display(Name = "FlowDetail_InspectLocationTo", ResourceType = typeof(Resources.SCM.FlowDetail))]
        //public string InspectLocationTo { get; set; }

        //[Display(Name = "FlowDetail_RejectLocationFrom", ResourceType = typeof(Resources.SCM.FlowDetail))]
        //public string RejectLocationFrom { get; set; }

        //[Display(Name = "FlowDetail_RejectLocationTo", ResourceType = typeof(Resources.SCM.FlowDetail))]
        //public string RejectLocationTo { get; set; }
        //[Display(Name = "FlowDetail_PartyFrom", ResourceType = typeof(Resources.SCM.FlowDetail))]
        //public string PartyFrom { get; set; }

        [Display(Name = "FlowDetail_BillAddress", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string BillAddress { get; set; }

        [Display(Name = "FlowDetail_PriceList", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string PriceList { get; set; }

        [Display(Name = "FlowDetail_Routing", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string Routing { get; set; }

        [Display(Name = "FlowDetail_ReturnRouting", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string ReturnRouting { get; set; }

        [Display(Name = "FlowDetail_IsAutoCreate", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Boolean IsAutoCreate { get; set; }

        [Display(Name = "FlowDetail_IsInspect", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Boolean IsInspect { get; set; }

        [Display(Name = "FlowDetail_IsRejectInspect", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Boolean IsRejectInspect { get; set; }

        [Display(Name = "FlowDetail_SafeStock", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Decimal? SafeStock { get; set; }

        [Display(Name = "FlowDetail_MaxStock", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Decimal? MaxStock { get; set; }

        [Display(Name = "FlowDetail_MinLotSize", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Decimal? MinLotSize { get; set; }

        [Display(Name = "FlowDetail_OrderLotSize", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Decimal? OrderLotSize { get; set; }

        [Display(Name = "FlowDetail_ReceiveLotSize", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Decimal? ReceiveLotSize { get; set; }

        [Display(Name = "FlowDetail_BatchSize", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Decimal? BatchSize { get; set; }

        [Display(Name = "FlowDetail_RoundUpOption", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public com.Sconit.CodeMaster.RoundUpOption RoundUpOption { get; set; }

        //[Display(Name = "FlowDetail_BillTerm", ResourceType = typeof(Resources.SCM.FlowDetail))]
        //public com.Sconit.CodeMaster.OrderBillTerm FlowDetailBillTerm { get; set; }

        [Display(Name = "FlowDetail_BillTerm", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public com.Sconit.CodeMaster.OrderBillTerm BillTerm { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "FlowDetail_MrpWeight", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Int32 MrpWeight { get; set; }

        [Display(Name = "FlowDetail_MrpTotal", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public decimal MrpTotal { get; set; }

        [Display(Name = "FlowDetail_MrpTotalAdjust", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public decimal MrpTotalAdjust { get; set; }

        [Display(Name = "FlowDetail_ExtraDemandSource", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string ExtraDemandSource { get; set; }

        [Export(ExportSeq = 60, ExportName = "ExportKanbanScanXLS")]
        [Export(ExportSeq = 160, ExportName = "ExportList")]
        [Display(Name = "FlowDetail_Container", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string Container { get; set; }

        [Export(ExportSeq = 70, ExportName = "ExportKanbanScanXLS")]
        [Export(ExportSeq = 170, ExportName = "ExportList")]
        [Display(Name = "FlowDetail_ContainerDescription", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string ContainerDescription { get; set; }

        public Int32 CreateUserId { get; set; }

        public string CreateUserName { get; set; }

        public DateTime CreateDate { get; set; }

        public Int32 LastModifyUserId { get; set; }

        [Display(Name = "OrderDetail_LastModifyUserName", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string LastModifyUserName { get; set; }

        [Display(Name = "OrderDetail_LastModifyDate", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public DateTime LastModifyDate { get; set; }

        [Export(ExportSeq = 90, ExportName = "ExportKanbanScanXLS")]
        [Display(Name = "FlowDetail_ProductionScan", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string ProductionScan { get; set; }
        [Display(Name = "FlowDetail_PickStrategy", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string PickStrategy { get; set; }
        [Display(Name = "FlowDetail_EBELN", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string EBELN { get; set; }
        [Display(Name = "FlowDetail_EBELP", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string EBELP { get; set; }
        [Export(ExportSeq = 50, ExportName = "ExportList")]
        [Display(Name = "FlowDetail_IsChangeUnitCount", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Boolean IsChangeUnitCount { get; set; }

        [Export(ExportSeq = 80, ExportName = "ExportKanbanScanXLS")]
        [Display(Name = "FlowDetail_BinTo", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string BinTo { get; set; }

        [Display(Name = "FlowDetail_IsCheckedPackage", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Boolean? IsCheckedPackage { get; set; }

        [Display(Name = "FlowDetail_FreezeDays", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Int32 FreezeDays { get; set; }

        [Export(ExportSeq = 60, ExportName = "ExportList")]
        [Display(Name = "FlowMaster_IsActive", ResourceType = typeof(Resources.SCM.FlowMaster))]
        public Boolean IsActive { get; set; }

        [Display(Name = "FlowDetail_ManufactureParty", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public String ManufactureParty { get; set; }

        public string MrpTag { get; set; }

        [Display(Name = "FlowMaster_IsCreatePickList", ResourceType = typeof(Resources.SCM.FlowMaster))]
        public Boolean? IsCreatePickList { get; set; }

        [Export(ExportSeq = 110, ExportName = "ExportKanbanScanXLS")]
        [Display(Name = "FlowDetail_CycloidAmount", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Int32 CycloidAmount { get; set; }

        [Display(Name = "FlowDetail_OprefSequence", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string OprefSequence { get; set; }

        [Display(Name = "FlowDetail_ItemType", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string ItemType { get; set; }

        [Display(Name = "FlowDetail_Dock", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string Dock { get; set; }
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
            FlowDetail another = obj as FlowDetail;

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
