using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.MD
{
    [Serializable]
    public partial class LocationBinItem : EntityBase, IAuditable
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }
        [Export(ExportName = "LocationBinItem",ExportSeq=30)]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "LocationBinItem_Location", ResourceType = typeof(Resources.MD.LocationBinItem))]
		public string Location { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "LocationBinItem_Bin", ResourceType = typeof(Resources.MD.LocationBinItem))]
        [Export(ExportName = "LocationBinItem",ExportSeq=40)]
        public string Bin { get; set; }

        [Export(ExportName = "LocationBinItem",ExportSeq = 10)]
        [Display(Name = "LocationBinItem_Item", ResourceType = typeof(Resources.MD.LocationBinItem))]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        public string Item { get; set; }

        [Export(ExportName = "LocationBinItem", ExportSeq = 15)]
        [Display(Name = "LocationBinItem_ItemDesc", ResourceType = typeof(Resources.MD.LocationBinItem))]
        public string ItemDesc { get; set; }

        [Display(Name = "LocationBinItem_IsActive", ResourceType = typeof(Resources.MD.LocationBinItem))]
		public Boolean IsActive { get; set; }

		public Int32 CreateUserId { get; set; }

        [Display(Name = "LocationBinItem_CreateUserName", ResourceType = typeof(Resources.MD.LocationBinItem))]
		public string CreateUserName { get; set; }
        
		public DateTime CreateDate { get; set; }
		public Int32 LastModifyUserId { get; set; }
        [Display(Name = "Common_LastModifyDate", ResourceType = typeof(Resources.Global))]
		public string LastModifyUserName { get; set; }
        [Display(Name = "Common_LastModifyDate", ResourceType = typeof(Resources.Global))]
		public DateTime LastModifyDate { get; set; }
        
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
            LocationBinItem another = obj as LocationBinItem;

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
