using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.VIEW
{
    public partial class FlowMasterView
    {
        public int RowId { get; set; }

        [Export(ExportName = "JITFlow", ExportSeq = 1)]
        [Export(ExportName = "KanBanFlow", ExportSeq = 1)]
        [Export(ExportName = "SEQProcurementFlow", ExportSeq = 0)]
        [Export(ExportName = "SEQTransferFlow", ExportSeq = 1)]
        [Export(ExportName = "KBProductionFlow", ExportSeq = 1)]
        [Export(ExportName = "HemProductionFlow", ExportSeq = 1)]
        [Display(Name = "FlowMaster_Code", ResourceType = typeof(Resources.SCM.FlowMaster))]
        public string Code { get; set; }

        [Export(ExportName = "JITFlow", ExportSeq = 2)]
        [Export(ExportName = "KanBanFlow", ExportSeq = 2)]
        [Export(ExportName = "SEQProcurementFlow", ExportSeq = 1)]
        [Export(ExportName = "SEQTransferFlow", ExportSeq = 2)]
        [Export(ExportName = "KBProductionFlow", ExportSeq = 2, ExportTitle = "FlowMaster_ProductionDescription", ExportTitleResourceType = typeof(Resources.SCM.FlowMaster))]
        [Export(ExportName = "HemProductionFlow", ExportSeq = 2, ExportTitle = "FlowMaster_ProductionDescription", ExportTitleResourceType = typeof(Resources.SCM.FlowMaster))]
        [Display(Name = "FlowMaster_Description", ResourceType = typeof(Resources.SCM.FlowMaster))]
        public string Description { get; set; }

        public Boolean IsActive { get; set; }

        [Export(ExportName = "KanBanFlow", ExportSeq = 3)]
        [Export(ExportName = "JITFlow", ExportSeq = 3)]
        [Export(ExportName = "SEQTransferFlow", ExportSeq = 3)]
        [Export(ExportName = "KBProductionFlow", ExportSeq = 3, ExportTitle = "FlowMaster_ProductionPartyFrom", ExportTitleResourceType = typeof(Resources.SCM.FlowMaster))]
        [Export(ExportName = "HemProductionFlow", ExportSeq = 3, ExportTitle = "FlowMaster_ProductionPartyFrom", ExportTitleResourceType = typeof(Resources.SCM.FlowMaster))]
        [Display(Name = "FlowMaster_TransferPartyFrom", ResourceType = typeof(Resources.SCM.FlowMaster))]
        public string PartyFrom { get; set; }

        [Export(ExportName = "KanBanFlow", ExportSeq = 4)]
        [Export(ExportName = "JITFlow", ExportSeq = 4)]
        [Display(Name = "FlowMaster_TransferPartyTo", ResourceType = typeof(Resources.SCM.FlowMaster))]
        public string PartyTo { get; set; }

        [Export(ExportName = "KanBanFlow", ExportSeq = 5)]
        [Export(ExportName = "JITFlow", ExportSeq = 5)]
        [Export(ExportName = "SEQTransferFlow", ExportSeq = 4)]
        [Export(ExportName = "KBProductionFlow", ExportSeq = 4, ExportTitle = "FlowMaster_ProductionLocationFrom", ExportTitleResourceType = typeof(Resources.SCM.FlowMaster))]
        [Export(ExportName = "HemProductionFlow", ExportSeq = 4, ExportTitle = "FlowMaster_ProductionLocationFrom", ExportTitleResourceType = typeof(Resources.SCM.FlowMaster))]
        [Display(Name = "FlowMaster_LocationFrom", ResourceType = typeof(Resources.SCM.FlowMaster))]
        public string LocationFrom { get; set; }

        [Export(ExportName = "JITFlow", ExportSeq = 6)]
        [Export(ExportName = "KanBanFlow", ExportSeq = 6)]
        [Export(ExportName = "JITFlow", ExportSeq = 5)]
        [Export(ExportName = "KBProductionFlow", ExportSeq = 5, ExportTitle = "FlowMaster_ProductionLocationTo", ExportTitleResourceType = typeof(Resources.SCM.FlowMaster))]
        [Export(ExportName = "HemProductionFlow", ExportSeq = 5, ExportTitle = "FlowMaster_ProductionLocationTo", ExportTitleResourceType = typeof(Resources.SCM.FlowMaster))]
        [Display(Name = "FlowMaster_LocationTo", ResourceType = typeof(Resources.SCM.FlowMaster))]
        public string LocationTo { get; set; }

        [Export(ExportName = "JITFlow", ExportSeq = 12)]
        [Export(ExportName = "SEQTransferFlow", ExportSeq = 9)]
        [Display(Name = "FlowMaster_HuTemplate_Import", ResourceType = typeof(Resources.SCM.FlowMaster))]
        public string HuTemplate { get; set; }

        [Export(ExportName = "SEQTransferFlow", ExportSeq = 5)]
        [Export(ExportName = "SEQProcurementFlow", ExportSeq = 4)]
        [Display(Name = "FlowStrategy_LeadTime", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        public Decimal LeadTime { get; set; }

        [Export(ExportName = "SEQTransferFlow", ExportSeq = 6)]
        [Export(ExportName = "SEQProcurementFlow", ExportSeq = 5)]
        [Display(Name = "FlowStrategy_SequenceGroup", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        public string SequenceGroup { get; set; }

        [Export(ExportName = "SEQProcurementFlow", ExportSeq = 6)]
        [Display(Name = "FlowStrategy_SupplierGroup", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        public string SupplierGroup { get; set; }

        [Export(ExportName = "KanBanFlow", ExportSeq = 7)]
        [Export(ExportName = "JITFlow", ExportSeq = 7)]
        [Display(Name = "FlowStrategy_QiTiaoBian", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        public string QiTiaoBian { get; set; }

        [Export(ExportName = "SEQProcurementFlow", ExportSeq = 7)]
        [Display(Name = "FlowStrategy_SupplierGroupSequence", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        public Int32 SupplierGroupSequence { get; set; }

        [Export(ExportName = "KanBanFlow", ExportSeq = 8)]
        [Export(ExportName = "JITFlow", ExportSeq = 9)]
        [Export(ExportName = "SEQTransferFlow", ExportSeq = 8)]
        [Export(ExportName = "SEQProcurementFlow", ExportSeq = 8)]
        [Export(ExportName = "KBProductionFlow", ExportSeq = 6)]
        [Export(ExportName = "HemProductionFlow", ExportSeq = 6)]
        [Display(Name = "FlowDetail_Item", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string Item { get; set; }

        [Export(ExportName = "JITFlow", ExportSeq = 10)]
        [Export(ExportName = "KanBanFlow", ExportSeq = 9)]
        [Export(ExportName = "KBProductionFlow", ExportSeq = 7)]
        [Export(ExportName = "HemProductionFlow", ExportSeq = 7)]
        [Display(Name = "FlowDetail_ItemDescription", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string ItemDesc { get; set; }

        [Export(ExportName = "JITFlow", ExportSeq = 8)]
        [Export(ExportName = "KanBanFlow", ExportSeq = 11)]
        [Export(ExportName = "KBProductionFlow", ExportSeq = 9)]
        [Display(Name = "FlowDetail_SafeStock", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Decimal? SafeStock { get; set; }

        [Export(ExportName = "JITFlow", ExportSeq = 11)]
        [Export(ExportName = "KanBanFlow", ExportSeq = 12)]
        [Export(ExportName = "HemProductionFlow", ExportSeq = 11, ExportTitle = "FlowDetail_Dock_HEM", ExportTitleResourceType = typeof(Resources.SCM.FlowDetail))]
        [Display(Name = "FlowDetail_Dock", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string Dock { get; set; }

        [Export(ExportName = "SEQProcurementFlow", ExportSeq = 3)]
        [Display(Name = "FlowDetail_SEQ_PartyFrom", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string DetPartyFrom { get; set; }

        public Int32? Id { get; set; }

        [Export(ExportName = "KanBanFlow", ExportSeq = 10)]
        [Display(Name = "FlowDetail_BinTo", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string BinTo { get; set; }

        public CodeMaster.FlowStrategy FlowStrategy { get; set; }

        public CodeMaster.OrderType Type { get; set; }

        [Display(Name = "FlowMaster_ProdLineType", ResourceType = typeof(Resources.SCM.FlowMaster))]
        public CodeMaster.ProdLineType ProdLineType { get; set; }

        [Export(ExportName = "KBProductionFlow", ExportSeq = 8)]
        [Export(ExportName = "HemProductionFlow", ExportSeq = 8)]
        [Display(Name = "FlowDetail_VanSeries", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string VanSeries { get; set; }

        [Export(ExportName = "KBProductionFlow", ExportSeq = 10)]
        [Display(Name = "FlowDetail_BatchSize", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Decimal BatchSize { get; set; }

        [Export(ExportName = "KBProductionFlow", ExportSeq = 11)]
        [Export(ExportName = "HemProductionFlow", ExportSeq = 9)]
        [Display(Name = "FlowDetail_GroupNo", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string GroupNo { get; set; }

        [Export(ExportName = "KBProductionFlow", ExportSeq = 12)]
        [Export(ExportName = "HemProductionFlow", ExportSeq = 10)]
        [Display(Name = "FlowDetail_GroupDesc", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string GroupDesc { get; set; }

        [Export(ExportName = "KBProductionFlow", ExportSeq = 13)]
        [Display(Name = "FlowDetail_KanbanStation", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string KBStation { get; set; }

        [Export(ExportName = "KBProductionFlow", ExportSeq = 14)]
        [Display(Name = "FlowDetail_Shift", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string Shift { get; set; }

        public bool IsCut { get; set; }

        [Export(ExportName = "KBProductionFlow", ExportSeq = 16)]
        [Display(Name = "FlowDetail_CutLeadTime", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Decimal CutLeadTime { get; set; }

        [Export(ExportName = "KBProductionFlow", ExportSeq = 17)]
        [Display(Name = "FlowDetail_SupplyPoints", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string SupplyPoints { get; set; }

        [Export(ExportName = "HemProductionFlow", ExportSeq = 20)]
        [Display(Name = "FlowDetail_ExtraDemandSource", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string ExtraDemandSource { get; set; }

        [Export(ExportName = "HemProductionFlow", ExportSeq = 12)]
        [Display(Name = "FlowDetail_NItemCode", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string NItemCode { get; set; }

        [Export(ExportName = "HemProductionFlow", ExportSeq = 13)]
        [Display(Name = "FlowDetail_NItemDesc", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string NItemDesc { get; set; }

        [Export(ExportName = "HemProductionFlow", ExportSeq = 14)]
        [Display(Name = "FlowDetail_NCode", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string NCode { get; set; }

        [Export(ExportName = "HemProductionFlow", ExportSeq = 15)]
        [Display(Name = "FlowDetail_NWipQty", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Int32 NWipQty { get; set; }

        [Export(ExportName = "HemProductionFlow", ExportSeq = 16)]
        [Display(Name = "FlowDetail_WItemCode", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string WItemCode { get; set; }

        [Export(ExportName = "HemProductionFlow", ExportSeq = 17)]
        [Display(Name = "FlowDetail_WItemDesc", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string WItemDesc { get; set; }

        [Export(ExportName = "HemProductionFlow", ExportSeq = 18)]
        [Display(Name = "FlowDetail_WCode", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string WCode { get; set; }

        [Export(ExportName = "HemProductionFlow", ExportSeq = 19)]
        [Display(Name = "FlowDetail_WWipQty", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Int32 WWipQty { get; set; }

        [Export(ExportName = "KanBanFlow", ExportSeq = 13)]
        [Display(Name = "FlowDetail_Shelf", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string Shelf { get; set; }

        [Export(ExportName = "KanBanFlow", ExportSeq = 15)]
        [Display(Name = "FlowDetail_IsNotCalc", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public Boolean? IsRejectInspect { get; set; }

        public override int GetHashCode()
        {
            if (RowId != 0)
            {
                return RowId.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            FlowMasterView another = obj as FlowMasterView;

            if (another == null)
            {
                return false;
            }
            else
            {
                return (this.RowId == another.RowId);
            }
        }
    }
}
