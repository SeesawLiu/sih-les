using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.VIEW
{
    [Serializable]
    public partial class MultiSupplyGroupView : EntityBase
    {
        #region O/R Mapping Properties

        public int RowId { get; set; }

        [Export(ExportName = "MultiSupplyGroup", ExportSeq = 1)]
        [Display(Name = "MultiSupplyGroup_GroupNo", ResourceType = typeof(Resources.PRD.MultiSupplyGroup))]
        public string GroupNo { get; set; }

        [Export(ExportName = "MultiSupplyGroup", ExportSeq = 2)]
        [Display(Name = "MultiSupplyGroup_Description", ResourceType = typeof(Resources.PRD.MultiSupplyGroup))]
        public string Description { get; set; }

        [Export(ExportName = "MultiSupplyGroup", ExportSeq = 3)]
        [Display(Name = "MultiSupplyGroup_EffSupplier", ResourceType = typeof(Resources.PRD.MultiSupplyGroup))]
        public string EffecitveSupplier { get; set; }

        [Export(ExportName = "MultiSupplyGroup", ExportSeq = 4)]
        [Display(Name = "MultiSupplyGroup_TargetCycleQty", ResourceType = typeof(Resources.PRD.MultiSupplyGroup))]
        public Int32? TargetCycleQty { get; set; }

        [Export(ExportName = "MultiSupplyGroup", ExportSeq = 5)]
        [Display(Name = "MultiSupplyGroup_AccumulateQty", ResourceType = typeof(Resources.PRD.MultiSupplyGroup))]
        public Decimal? AccumulateQty { get; set; }

        [Export(ExportName = "MultiSupplyGroup", ExportSeq = 6)]
        [Display(Name = "MultiSupplySupplier_Supplier", ResourceType = typeof(Resources.PRD.MultiSupplySupplier))]
        public string Supplier { get; set; }

        [Export(ExportName = "MultiSupplyGroup", ExportSeq = 7)]
        [Display(Name = "MultiSupplySupplier_Seq", ResourceType = typeof(Resources.PRD.MultiSupplySupplier))]
        public Int32? Sequence { get; set; }

        [Export(ExportName = "MultiSupplyGroup", ExportSeq = 8)]
        [Display(Name = "MultiSupplySupplier_CycleQty", ResourceType = typeof(Resources.PRD.MultiSupplySupplier))]
        public Int32? CycleQty { get; set; }

        [Export(ExportName = "MultiSupplyGroup", ExportSeq = 10)]
        [Display(Name = "MultiSupplySupplier_SpillQty", ResourceType = typeof(Resources.PRD.MultiSupplySupplier))]
        public Decimal? SpillQty { get; set; }

        [Export(ExportName = "MultiSupplyGroup", ExportSeq = 9)]
        [Display(Name = "MultiSupplySupplier_Proportion", ResourceType = typeof(Resources.PRD.MultiSupplySupplier))]
        public string Proportion { get; set; }

        [Display(Name = "MultiSupplySupplier_IsActive", ResourceType = typeof(Resources.PRD.MultiSupplySupplier))]
        public Boolean? IsActive { get; set; }

        [Export(ExportName = "MultiSupplyGroup", ExportSeq = 12)]
        [Display(Name = "MultiSupplyItem_Item", ResourceType = typeof(Resources.PRD.MultiSupplyItem))]
        public string Item { get; set; }

        [Export(ExportName = "MultiSupplyGroup", ExportSeq = 13)]
        [Display(Name = "MultiSupplyItem_ItemDescription", ResourceType = typeof(Resources.PRD.MultiSupplyItem))]
        public string ItemDescription { get; set; }

        [Export(ExportName = "MultiSupplyGroup", ExportSeq = 11)]
        [Display(Name = "MultiSupplyItem_SubstituteGroup", ResourceType = typeof(Resources.PRD.MultiSupplyItem))]
        public string SubstituteGroup { get; set; }

        public Int32? Id { get; set; }

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
            MultiSupplyGroupView another = obj as MultiSupplyGroupView;

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
