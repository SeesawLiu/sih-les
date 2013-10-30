using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.INV
{
    [Serializable]
    public partial class Hu : EntityBase, IAuditable
    {
        #region O/R Mapping Properties

        [Display(Name = "Hu_HuId", ResourceType = typeof(Resources.INV.Hu))]
		public string HuId { get; set; }
		//[Display(Name = "Type", ResourceType = typeof(Resources.INV.Hu))]
        //public Int16 Type { get; set; }
        [Display(Name = "Hu_lotNo", ResourceType = typeof(Resources.INV.Hu))]
		public string LotNo { get; set; }
        [Display(Name = "Hu_Item", ResourceType = typeof(Resources.INV.Hu))]
		public string Item { get; set; }
        [Display(Name = "Hu_ItemDescription", ResourceType = typeof(Resources.INV.Hu))]
        public string ItemDescription { get; set; }
        [Display(Name = "Hu_ReferenceItemCode", ResourceType = typeof(Resources.INV.Hu))]
        public string ReferenceItemCode { get; set; }
		[Display(Name = "Hu_Uom", ResourceType = typeof(Resources.INV.Hu))]
		public string Uom { get; set; }
        public string BaseUom { get; set; }
		[Display(Name = "Hu_UnitCount", ResourceType = typeof(Resources.INV.Hu))]
		public Decimal UnitCount { get; set; }
        [Display(Name = "Hu_Qty", ResourceType = typeof(Resources.INV.Hu))]
		public Decimal Qty { get; set; }
		//[Display(Name = "UnitQty", ResourceType = typeof(Resources.INV.Hu))]
		public Decimal UnitQty { get; set; }
        [Display(Name = "Hu_manufacture_date", ResourceType = typeof(Resources.INV.Hu))]
		public DateTime ManufactureDate { get; set; }
        [Display(Name = "Hu_ManufactureParty", ResourceType = typeof(Resources.INV.Hu))]
        public string ManufactureParty { get; set; }
		//[Display(Name = "ExpireDate", ResourceType = typeof(Resources.INV.Hu))]
		public DateTime? ExpireDate { get; set; }
        [Display(Name = "Hu_RemindExpireDate", ResourceType = typeof(Resources.INV.Hu))]
        public DateTime? RemindExpireDate { get; set; }
		//[Display(Name = "HuTemplate", ResourceType = typeof(Resources.INV.Hu))]
		//public string HuTemplate { get; set; }
		//[Display(Name = "PrintCount", ResourceType = typeof(Resources.INV.Hu))]
		public Int16 PrintCount { get; set; }
		//[Display(Name = "Location", ResourceType = typeof(Resources.INV.Hu))]
        [Display(Name = "Bin", ResourceType = typeof(Resources.INV.Hu))]
        public string Bin { get; set; }
		//[Display(Name = "Status", ResourceType = typeof(Resources.INV.Hu))]
        //public Int16 Status { get; set; }
        [Display(Name = "Hu_FirstInventoryDate", ResourceType = typeof(Resources.INV.Hu))]
        public DateTime? FirstInventoryDate { get; set; }
        [Display(Name = "Hu_IsOdd", ResourceType = typeof(Resources.INV.Hu))]
        public Boolean IsOdd { get; set; }
        public Int32 CreateUserId { get; set; }
        [Display(Name = "Hu_CreateUserName", ResourceType = typeof(Resources.INV.Hu))]
		public string CreateUserName { get; set; }
        [Display(Name = "Hu_CreateDate", ResourceType = typeof(Resources.INV.Hu))]
		public DateTime CreateDate { get; set; }
		//[Display(Name = "LastModifyUserId", ResourceType = typeof(Resources.INV.Hu))]
		public Int32 LastModifyUserId { get; set; }
		//[Display(Name = "LastModifyUserName", ResourceType = typeof(Resources.INV.Hu))]
		public string LastModifyUserName { get; set; }
		//[Display(Name = "LastModifyDate", ResourceType = typeof(Resources.INV.Hu))]
		public DateTime LastModifyDate { get; set; }
		//[Display(Name = "Version", ResourceType = typeof(Resources.INV.Hu))]
        //public Int32 Version { get; set; }
        public Int16 ConcessionCount { get; set; }
        public string OrderNo { get; set; }
        public string ReceiptNo { get; set; }
        [Display(Name = "Hu_SupplierLotNo", ResourceType = typeof(Resources.INV.Hu))]
        public string SupplierLotNo { get; set; }
        public string ContainerDesc { get; set; }
        [Display(Name = "Hu_LocationTo", ResourceType = typeof(Resources.INV.Hu))]
        public string LocationTo { get; set; }
        public Boolean IsChangeUnitCount { get; set; }
        public string UnitCountDescription { get; set; }
        public Int32 Id { get; set; }
        #endregion

		public override int GetHashCode()
        {
			if (HuId != null)
            {
                return HuId.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }
        //用来做checkbox的头
        public string CheckHuId { get; set; }

        public override bool Equals(object obj)
        {
            Hu another = obj as Hu;

            if (another == null)
            {
                return false;
            }
            else
            {
            	return (this.HuId == another.HuId);
            }
        } 
    }
	
}
