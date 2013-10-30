using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.VIEW
{
    public partial class MultiSupplyGroupView
    {
        #region Non O/R Mapping Properties

        [Display(Name = "MultiSupplyGroup_GroupNo", ResourceType = typeof(Resources.PRD.MultiSupplyGroup))]
        public string DisplayGroupNo { get; set; }

        #endregion
    }
}