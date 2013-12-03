using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.CUST
{
    [Serializable]
    public partial class OpRefMap : EntityBase, IAuditable
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }
        [Export(ExportName = "OpRefMapXls", ExportSeq = 10)]
        [Display(Name = "OpRefMap_SAPProdLine", ResourceType = typeof(Resources.CUST.OpRefMap))]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        public string SAPProdLine { get; set; }
        [Export(ExportName = "OpRefMapXls", ExportSeq = 20)]
        [Display(Name = "OpRefMap_ProdLine", ResourceType = typeof(Resources.CUST.OpRefMap))]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        public string ProdLine { get; set; }
        [Export(ExportName = "OpRefMapXls", ExportSeq = 30)]
        [Display(Name = "OpRefMap_Item", ResourceType = typeof(Resources.CUST.OpRefMap))]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        public string Item { get; set; }
        [Export(ExportName = "OpRefMapXls", ExportSeq = 50)]
        [Display(Name = "OpRefMap_ItemDesc", ResourceType = typeof(Resources.CUST.OpRefMap))]
        public string ItemDesc { get; set; }
        [Export(ExportName = "OpRefMapXls", ExportSeq = 40)]
        [Display(Name = "OpRefMap_ItemRefCode", ResourceType = typeof(Resources.CUST.OpRefMap))]
        public string ItemRefCode { get; set; }
        [Export(ExportName = "OpRefMapXls", ExportSeq = 60)]
        [Display(Name = "OpRefMap_OpReference", ResourceType = typeof(Resources.CUST.OpRefMap))]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        public string OpReference { get; set; }
        [Export(ExportName = "OpRefMapXls", ExportSeq = 70)]
        [Display(Name = "OpRefMap_RefOpReference", ResourceType = typeof(Resources.CUST.OpRefMap))]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        public string RefOpReference { get; set; }
        [Export(ExportName = "OpRefMapXls", ExportSeq = 80)]
        [Display(Name = "OpRefMap_IsPrimary", ResourceType = typeof(Resources.CUST.OpRefMap))]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        public Boolean? IsPrimary { get; set; }
		public Int32 CreateUserId { get; set; }
        [Export(ExportName = "OpRefMapXls", ExportSeq = 90)]
        [Display(Name = "OpRefMap_CreateUserName", ResourceType = typeof(Resources.CUST.OpRefMap))]
        public string CreateUserName { get; set; }
        [Export(ExportName = "OpRefMapXls", ExportSeq = 100)]
        [Display(Name = "OpRefMap_CreateDate", ResourceType = typeof(Resources.CUST.OpRefMap))]
        public DateTime CreateDate { get; set; }
		public Int32 LastModifyUserId { get; set; }
		public string LastModifyUserName { get; set; }
		public DateTime LastModifyDate { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "OpRefMap_Location", ResourceType = typeof(Resources.CUST.OpRefMap))]
        public string Location { get; set; }
        
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
            OpRefMap another = obj as OpRefMap;

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
