using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;
namespace com.Sconit.Entity.MD
{
    [Serializable]
    public partial class Item : EntityBase, IAuditable
    {
        #region O/R Mapping Properties
        [Export(ExportName = "ExportItemXLS", ExportSeq = 10)]
        [Export(ExportName = "ExportShorCode", ExportSeq = 10)]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "Item_Code", ResourceType = typeof(Resources.MD.Item))]
		public string Code { get; set; }

        [Export(ExportName = "ExportItemXLS", ExportSeq = 20)]
        [Export(ExportName = "ExportShorCode", ExportSeq = 30)]
        [Display(Name = "Item_ReferenceCode", ResourceType = typeof(Resources.MD.Item))]
        public string ReferenceCode { get; set; }

        [Export(ExportName = "ExportItemXLS", ExportSeq = 60)]
        [Export(ExportName = "ExportShorCode", ExportSeq = 20)]
        [Display(Name = "Item_ShortCode", ResourceType = typeof(Resources.MD.Item))]
        public string ShortCode { get; set; }

        [Export(ExportName = "ExportItemXLS", ExportSeq = 40)]
        [Export(ExportName = "ExportShorCode", ExportSeq = 50)]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "Item_Uom", ResourceType = typeof(Resources.MD.Item))]
		public string Uom { get; set; }

        [Export(ExportName = "ExportItemXLS", ExportSeq = 30)]
        [Export(ExportName = "ExportShorCode", ExportSeq = 40)]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [StringLength(100, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "Item_Description", ResourceType = typeof(Resources.MD.Item))]
		public string Description { get; set; }

        [Export(ExportName = "ExportShorCode", ExportSeq = 60)]
        [Display(Name = "Item_UC", ResourceType = typeof(Resources.MD.Item))]
		public Decimal UnitCount { get; set; }

        [Display(Name = "Item_ItemCategory", ResourceType = typeof(Resources.MD.Item))]
		public string ItemCategory { get; set; }

        [Export(ExportName = "ExportItemXLS", ExportSeq = 110)]
        [Display(Name = "Item_IsActive", ResourceType = typeof(Resources.MD.Item))]
		public Boolean IsActive { get; set; }

        [Display(Name = "Item_IsPurchase", ResourceType = typeof(Resources.MD.Item))]
		public Boolean IsPurchase { get; set; }

        [Display(Name = "Item_IsSales", ResourceType = typeof(Resources.MD.Item))]
        public Boolean IsSales { get; set; }

        [Display(Name = "Item_IsManufacture", ResourceType = typeof(Resources.MD.Item))]
		public Boolean IsManufacture { get; set; }

        [Display(Name = "Item_IsSubContract", ResourceType = typeof(Resources.MD.Item))]
		public Boolean IsSubContract { get; set; }

        [Display(Name = "Item_IsCustomerGoods", ResourceType = typeof(Resources.MD.Item))]
        public Boolean IsCustomerGoods { get; set; }

        [Display(Name = "Item_IsVirtual", ResourceType = typeof(Resources.MD.Item))]
		public Boolean IsVirtual { get; set; }

        [Display(Name = "Item_IsKit", ResourceType = typeof(Resources.MD.Item))]
		public Boolean IsKit { get; set; }

        [Display(Name = "Item_Bom", ResourceType = typeof(Resources.MD.Item))]
		public string Bom { get; set; }

        [Display(Name = "Item_Location", ResourceType = typeof(Resources.MD.Item))]
		public string Location { get; set; }

        [Display(Name = "Item_Routing", ResourceType = typeof(Resources.MD.Item))]
		public string Routing { get; set; }

        [Display(Name = "Item_IsInvFreeze", ResourceType = typeof(Resources.MD.Item))]
        public Boolean IsInventoryFreeze { get; set; }

        [Export(ExportName = "ExportItemXLS", ExportSeq = 90)]
        [Display(Name = "Item_NotBackFlush", ResourceType = typeof(Resources.MD.Item))]
        public Boolean NotBackFlush { get; set; }

        [Display(Name = "Item_DISPO", ResourceType = typeof(Resources.MD.Item))]
        public string DISPO { get; set; }

        [Display(Name = "Item_PLIFZ", ResourceType = typeof(Resources.MD.Item))]
        public string PLIFZ { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "Item_Warranty", ResourceType = typeof(Resources.MD.Item))]
        public Int32 Warranty { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "Item_WarnLeadTime", ResourceType = typeof(Resources.MD.Item))]
        public Int32 WarnLeadTime { get; set; }

        [Export(ExportName = "ExportItemXLS", ExportSeq = 70)]
        [Export(ExportName = "ExportShorCode", ExportSeq = 70)]
        [Display(Name = "Item_SpecifiedModel", ResourceType = typeof(Resources.MD.Item))]
        public string SpecifiedModel { get; set; }

        [Display(Name = "Item_Container", ResourceType = typeof(Resources.MD.Item))]
        public string Container { get; set; }

        [Export(ExportName = "ExportItemXLS", ExportSeq = 80)]
        [Display(Name = "Item_ContainerDesc", ResourceType = typeof(Resources.MD.Item))]
        public string ContainerDesc { get; set; }

        [Export(ExportName = "ExportItemXLS", ExportSeq = 50)]
        [Display(Name = "Item_MinUnitCount", ResourceType = typeof(Resources.MD.Item))]
        public Decimal MinUnitCount { get; set; }

		public Int32 CreateUserId { get; set; }
		public string CreateUserName { get; set; }
		public DateTime CreateDate { get; set; }
		public Int32 LastModifyUserId { get; set; }
		public string LastModifyUserName { get; set; }
		public DateTime LastModifyDate { get; set; }

        public string BESKZ { get; set; }
        public string SOBSL { get; set; }
        /// <summary>
        /// 外部物料组
        /// </summary>
        public string EXTWG { get; set; }
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
            Item another = obj as Item;

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
