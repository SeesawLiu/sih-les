using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.PRD
{
    [Serializable]
    public partial class MultiSupplySupplier : EntityBase, IAuditable
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "MultiSupplySupplier_GroupNo", ResourceType = typeof(Resources.PRD.MultiSupplySupplier))]
        public string GroupNo { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "MultiSupplySupplier_Supplier", ResourceType = typeof(Resources.PRD.MultiSupplySupplier))]
        public string Supplier { get; set; }

        [Display(Name = "MultiSupplySupplier_Seq", ResourceType = typeof(Resources.PRD.MultiSupplySupplier))]
        public Int32 Seq { get; set; }

        [Display(Name = "MultiSupplySupplier_CycleQty", ResourceType = typeof(Resources.PRD.MultiSupplySupplier))]
        public Int32 CycleQty { get; set; }

        [Display(Name = "MultiSupplySupplier_SpillQty", ResourceType = typeof(Resources.PRD.MultiSupplySupplier))]
        public Int32 SpillQty { get; set; }

        public Int32 CreateUserId { get; set; }
        public string CreateUserName { get; set; }
        public DateTime CreateDate { get; set; }
        public Int32 LastModifyUserId { get; set; }
        public string LastModifyUserName { get; set; }
        public DateTime LastModifyDate { get; set; }
        public Decimal AccumulateQty { get; set; }

        public Boolean _isActive;
        [Display(Name = "MultiSupplySupplier_IsActive", ResourceType = typeof(Resources.PRD.MultiSupplySupplier))]
        public Boolean IsActive
        {
            get
            {
                if (Id == 0) _isActive = true;
                return _isActive;
            }
            set { _isActive = value; }
        }

        [Display(Name = "MultiSupplySupplier_Proportion", ResourceType = typeof(Resources.PRD.MultiSupplySupplier))]
        public string Proportion { get; set; }

        public Int32 Version { get; set; }
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
            MultiSupplySupplier another = obj as MultiSupplySupplier;

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
