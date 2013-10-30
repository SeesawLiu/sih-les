using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.ORD
{
    [Serializable]
    public partial class MiscOrderDetail : EntityBase,IAuditable
    {
        #region O/R Mapping Properties
		
		//[Display(Name = "Id", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
		public Int32 Id { get; set; }
		//[Display(Name = "MiscOrderNo", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
		public string MiscOrderNo { get; set; }
        [Display(Name = "MiscOrderDetail_Sequence", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
		public Int32 Sequence { get; set; }
        [Display(Name = "MiscOrderDetail_WMSSeq", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
        public string WMSSeq { get; set; }
        [Display(Name = "MiscOrderDetail_Item", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
		public string Item { get; set; }
        [Display(Name = "MiscOrderDetail_ItemDescription", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
		public string ItemDescription { get; set; }
        [Display(Name = "MiscOrderDetail_ReferenceItemCode", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
		public string ReferenceItemCode { get; set; }
        [Display(Name = "MiscOrderDetail_Uom", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
		public string Uom { get; set; }
		//[Display(Name = "BaseUom", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
		public string BaseUom { get; set; }
        [Display(Name = "MiscOrderDetail_UnitCount", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
		public Decimal UnitCount { get; set; }
        [Display(Name = "MiscOrderDetail_UnitQty", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
		public Decimal UnitQty { get; set; }
        [Display(Name = "MiscOrderDetail_Location", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
		public string Location { get; set; }
       [Display(Name = "MiscOrderDetail_ReserveNo", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
		public string ReserveNo { get; set; }
       [Display(Name = "MiscOrderDetail_ReserveLine", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
		public string ReserveLine { get; set; }
       [Display(Name = "MiscOrderDetail_Qty", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
		public Decimal Qty { get; set; }
		//[Display(Name = "CreateUserId", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
		public Int32 CreateUserId { get; set; }
		//[Display(Name = "CreateUserName", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
		public string CreateUserName { get; set; }
		//[Display(Name = "CreateDate", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
		public DateTime CreateDate { get; set; }
		//[Display(Name = "LastModifyUserId", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
		public Int32 LastModifyUserId { get; set; }
		//[Display(Name = "LastModifyUserName", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
		public string LastModifyUserName { get; set; }
		//[Display(Name = "LastModifyDate", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
		public DateTime LastModifyDate { get; set; }

        [Display(Name = "MiscOrderDetail_ManufactureParty", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
        public string ManufactureParty { get; set; }

        [Display(Name = "MiscOrderDetail_EBELN", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
        public string EBELN { get; set; }
        [Display(Name = "MiscOrderDetail_EBELP", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
        public string EBELP { get; set; }

        [Display(Name = "MiscOrderDetail_SapOrderNo", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
        public string SapOrderNo { get; set; }
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
            MiscOrderDetail another = obj as MiscOrderDetail;

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
