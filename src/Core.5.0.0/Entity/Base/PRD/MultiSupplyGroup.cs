using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.PRD
{
    [Serializable]
    public partial class MultiSupplyGroup : EntityBase, IAuditable
    {
        #region O/R Mapping Properties

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "MultiSupplyGroup_GroupNo", ResourceType = typeof(Resources.PRD.MultiSupplyGroup))]
        public string GroupNo { get; set; }

        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "MultiSupplyGroup_EffSupplier", ResourceType = typeof(Resources.PRD.MultiSupplyGroup))]
        public string EffSupplier { get; set; }

        [Display(Name = "MultiSupplyGroup_TargetCycleQty", ResourceType = typeof(Resources.PRD.MultiSupplyGroup))]
        public Int32 TargetCycleQty { get; set; }
        [Display(Name = "MultiSupplyGroup_AccumulateQty", ResourceType = typeof(Resources.PRD.MultiSupplyGroup))]
        public Decimal AccumulateQty { get; set; }

        public Int32 CreateUserId { get; set; }
        public string CreateUserName { get; set; }
        public DateTime CreateDate { get; set; }
        public Int32 LastModifyUserId { get; set; }
        public string LastModifyUserName { get; set; }
        public DateTime LastModifyDate { get; set; }
        public Int32 Version { get; set; }

        [StringLength(100, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "MultiSupplyGroup_Description", ResourceType = typeof(Resources.PRD.MultiSupplyGroup))]
        public string Description { get; set; }

        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "MultiSupplyGroup_KBEffSupplier", ResourceType = typeof(Resources.PRD.MultiSupplyGroup))]
        public string KBEffSupplier { get; set; }

        [Display(Name = "MultiSupplyGroup_KBTargetCycleQty", ResourceType = typeof(Resources.PRD.MultiSupplyGroup))]
        public Decimal KBTargetCycleQty { get; set; }
        [Display(Name = "MultiSupplyGroup_KBAccumulateQty", ResourceType = typeof(Resources.PRD.MultiSupplyGroup))]
        public Decimal KBAccumulateQty { get; set; }

        #endregion

        public override int GetHashCode()
        {
            if (GroupNo != null)
            {
                return GroupNo.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            MultiSupplyGroup another = obj as MultiSupplyGroup;

            if (another == null)
            {
                return false;
            }
            else
            {
                return (this.GroupNo == another.GroupNo);
            }
        }
    }

}
