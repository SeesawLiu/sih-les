using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.INV
{
    [Serializable]
    public partial class StockTakeDetail : EntityBase,IAuditable
    {
        #region O/R Mapping Properties
		public Int32 Id { get; set; }
        [Display(Name = "StockTakeDetail_StNo", ResourceType = typeof(Resources.INV.StockTake))]
        public string StNo { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "StockTakeDetail_Item", ResourceType = typeof(Resources.INV.StockTake))]
        public string Item { get; set; }
        [Display(Name = "StockTakeDetail_ItemDescription", ResourceType = typeof(Resources.INV.StockTake))]
        public string ItemDescription { get; set; }

        [Display(Name = "LocationDetailView_RefItemCode", ResourceType = typeof(Resources.View.LocationDetailView))]
        public string RefItemCode { get; set; }
        
        public com.Sconit.CodeMaster.QualityType QualityType { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "StockTakeDetail_Uom", ResourceType = typeof(Resources.INV.StockTake))]
        public string Uom { get; set; }
        public string BaseUom { get; set; }
        public Decimal UnitQty { get; set; }
        [Display(Name = "StockTakeDetail_HuId", ResourceType = typeof(Resources.INV.StockTake))]
        public string HuId { get; set; }
        public string LotNo { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "StockTakeDetail_Qty", ResourceType = typeof(Resources.INV.StockTake))]
        public Decimal Qty { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "StockTakeDetail_Location", ResourceType = typeof(Resources.INV.StockTake))]
        public string Location { get; set; }
        [Display(Name = "StockTakeDetail_LocationBin", ResourceType = typeof(Resources.INV.StockTake))]
        public string Bin { get; set; }

		public Int32 CreateUserId { get; set; }
		public string CreateUserName { get; set; }
		public DateTime CreateDate { get; set; }
		public Int32 LastModifyUserId { get; set; }
		public string LastModifyUserName { get; set; }
		public DateTime LastModifyDate { get; set; }

        [Display(Name = "LocationDetailView_suppliers", ResourceType = typeof(Resources.View.LocationDetailView))]
        public string CSSupplier { get; set; }

        //[Export(ExportName = "ExportBackUpInvXLS", ExportSeq = 60)]
        [Display(Name = "LocationDetailView_IsCS", ResourceType = typeof(Resources.View.LocationDetailView))]
        public Boolean IsConsigement { get; set; }
        
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
            StockTakeDetail another = obj as StockTakeDetail;

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
