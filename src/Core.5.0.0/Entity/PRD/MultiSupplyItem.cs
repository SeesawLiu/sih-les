using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

//TODO: Add other using statements here

namespace com.Sconit.Entity.PRD
{
    public partial class MultiSupplyItem
    {
        #region Non O/R Mapping Properties

        //TODO: Add Non O/R Mapping Properties here. 

        #endregion
    }
    public class MuLtSupplyItemGroup
    {
        [Export(ExportName = "MuLtSupplyItemGroup")]
        [Display(Name = "MultiSupplyItem_GroupNo", ResourceType = typeof(Resources.PRD.MultiSupplyItem))]
        public string GroupNo { get; set; }

        [Export(ExportName = "MuLtSupplyItemGroup")]
        [Display(Name = "MultiSupplyItem_Supplier", ResourceType = typeof(Resources.PRD.MultiSupplyItem))]
        public string Supplier { get; set; }

        [Export(ExportName = "MuLtSupplyItemGroup")]
        [Display(Name = "MultiSupplyItem_Item", ResourceType = typeof(Resources.PRD.MultiSupplyItem))]
        public string Item { get; set; }

        [Export(ExportName = "MuLtSupplyItemGroup")]
        [Display(Name = "MultiSupplySupplier_CycleQty", ResourceType = typeof(Resources.PRD.MultiSupplySupplier))]
        public Decimal CycleQty { get; set; }

        [Export(ExportName = "MuLtSupplyItemGroup")]
        [Display(Name = "MultiSupplySupplier_IsActive", ResourceType = typeof(Resources.PRD.MultiSupplySupplier))]
        public string IsActiveDescription
        {
            get { return IsActive ? Resources.Global.True : Resources.Global.False; }
        }
        public Boolean IsActive { get; set; }
    }
}