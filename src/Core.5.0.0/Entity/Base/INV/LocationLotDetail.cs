using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.INV
{
    [Serializable]
    public partial class LocationLotDetail : EntityBase, IAuditable
    {
        #region O/R Mapping Properties
		
		//[Display(Name = "Id", ResourceType = typeof(Resources.INV.LocationLotDetail))]
		public Int32 Id { get; set; }
        [Display(Name = "LocationLotDetail_Location", ResourceType = typeof(Resources.INV.LocationLotDetail))]
		public string Location { get; set; }
        //[Display(Name = "Bin", ResourceType = typeof(Resources.INV.LocationLotDetail))]
		public string Bin { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "LocationLotDetail_Item", ResourceType = typeof(Resources.INV.LocationLotDetail))]
		public string Item { get; set; }
        [Display(Name = "LocationLotDetail_LotNo", ResourceType = typeof(Resources.INV.LocationLotDetail))]
		public string LotNo { get; set; }
        [Display(Name = "LocationLotDetail_HuId", ResourceType = typeof(Resources.INV.LocationLotDetail))]
		public string HuId { get; set; }
        [Display(Name = "LocationLotDetail_Qty", ResourceType = typeof(Resources.INV.LocationLotDetail))]
		public Decimal Qty { get; set; }
        //[Display(Name = "IsConsignment", ResourceType = typeof(Resources.INV.LocationLotDetail))]
		public Boolean IsConsignment { get; set; }
        //[Display(Name = "PlanBill", ResourceType = typeof(Resources.INV.LocationLotDetail))]
		public Int32? PlanBill { get; set; }
        public string ConsignmentSupplier { get; set; }
        //[Display(Name = "QualityType", ResourceType = typeof(Resources.INV.LocationLotDetail))]
        public com.Sconit.CodeMaster.QualityType QualityType { get; set; }
        [Display(Name = "LocationLotDetail_IsFreeze", ResourceType = typeof(Resources.INV.LocationLotDetail))]
		public Boolean IsFreeze { get; set; }
        //[Display(Name = "IsATP", ResourceType = typeof(Resources.INV.LocationLotDetail))]
		public Boolean IsATP { get; set; }
        //[Display(Name = "OccupyType", ResourceType = typeof(Resources.INV.LocationLotDetail))]
        public com.Sconit.CodeMaster.OccupyType OccupyType { get; set; }
        //[Display(Name = "OccupyReferenceNo", ResourceType = typeof(Resources.INV.LocationLotDetail))]
        public string OccupyReferenceNo { get; set; }
        //[Display(Name = "CreateUserId", ResourceType = typeof(Resources.INV.LocationLotDetail))]
		public Int32 CreateUserId { get; set; }
        //[Display(Name = "CreateUserName", ResourceType = typeof(Resources.INV.LocationLotDetail))]
		public string CreateUserName { get; set; }
        //[Display(Name = "CreateDate", ResourceType = typeof(Resources.INV.LocationLotDetail))]
		public DateTime CreateDate { get; set; }
        //[Display(Name = "LastModifyUserId", ResourceType = typeof(Resources.INV.LocationLotDetail))]
		public Int32 LastModifyUserId { get; set; }
        //[Display(Name = "LastModifyUserName", ResourceType = typeof(Resources.INV.LocationLotDetail))]
		public string LastModifyUserName { get; set; }
        [Display(Name = "LocationLotDetail_LastModifyDate", ResourceType = typeof(Resources.INV.LocationLotDetail))]
		public DateTime LastModifyDate { get; set; }
        //	[Display(Name = "Version", ResourceType = typeof(Resources.INV.LocationLotDetail))]
		public Int32 Version { get; set; }

        public string Area { get; set; }
        public Int32 BinSequence { get; set; }
        public Decimal HuQty { get; set; }
        [Display(Name = "LocationLotDetail_UnitCount", ResourceType = typeof(Resources.INV.LocationLotDetail))]
        public Decimal UnitCount { get; set; }
        [Display(Name = "LocationLotDetail_HuUom", ResourceType = typeof(Resources.INV.LocationLotDetail))]
        public string HuUom { get; set; }
        public string BaseUom { get; set; }
        public Decimal UnitQty { get; set; }
        [Display(Name = "LocationLotDetail_ManufactureParty", ResourceType = typeof(Resources.INV.LocationLotDetail))]
        public string ManufactureParty { get; set; }
        public DateTime ManufactureDate { get; set; }
        public DateTime FirstInventoryDate { get; set; }
        //public string ConsigementParty { get; set; }
        public Boolean IsOdd { get; set; }

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
            LocationLotDetail another = obj as LocationLotDetail;

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
