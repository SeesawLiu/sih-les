using System;
using System.ComponentModel.DataAnnotations;
namespace com.Sconit.Entity.MD
{
    [Serializable]
    public partial class Location : EntityBase, IAuditable
    {
        #region O/R Mapping Properties

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "Location_Code", ResourceType = typeof(Resources.MD.Location))]
		public string Code { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "Location_Name", ResourceType = typeof(Resources.MD.Location))]
		public string Name { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "Location_Region", ResourceType = typeof(Resources.MD.Location))]
		public string Region { get; set; }

        [Display(Name = "Location_IsActive", ResourceType = typeof(Resources.MD.Location))]
		public Boolean IsActive { get; set; }

        [Display(Name = "Location_AllowNegaInv", ResourceType = typeof(Resources.MD.Location))]
		public Boolean AllowNegative { get; set; }

        [Display(Name = "Location_EnableAdvWM", ResourceType = typeof(Resources.MD.Location))]
		public Boolean EnableAdvanceWarehouseManagment { get; set; }

        [Display(Name = "Location_IsCS", ResourceType = typeof(Resources.MD.Location))]
		public Boolean IsConsignment { get; set; }

        [Display(Name = "Location_IsMRP", ResourceType = typeof(Resources.MD.Location))]
		public Boolean IsMRP { get; set; }

        [Display(Name = "Location_IsInvFreeze", ResourceType = typeof(Resources.MD.Location))]
        public Boolean IsInventoryFreeze { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "Location_SAPLocation", ResourceType = typeof(Resources.MD.Location))]
        public string SAPLocation { get; set; }
        public Int32 CreateUserId { get; set; }
		public string CreateUserName { get; set; }
		public DateTime CreateDate { get; set; }
		public Int32 LastModifyUserId { get; set; }
		public string LastModifyUserName { get; set; }
		public DateTime LastModifyDate { get; set; }
        [Display(Name = "Location_AllowNegativeConsigment", ResourceType = typeof(Resources.MD.Location))]
        public Boolean AllowNegativeConsignment { get; set; }
        [Display(Name = "Location_IsSource", ResourceType = typeof(Resources.MD.Location))]
        public Boolean IsSource { get; set; }
        public Boolean MergeLocationLotDet { get; set; }
        
        #endregion

		public override int GetHashCode()
        {
			if (Code != null)
            {
                return Code.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            Location another = obj as Location;

            if (another == null)
            {
                return false;
            }
            else
            {
            	return (this.Code == another.Code);
            }
        } 
    }
	
}
