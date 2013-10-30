using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.BIL
{
    [Serializable]
    public partial class ActingBill : EntityBase, IAuditable
    {
        #region O/R Mapping Properties
		
		//[Display(Name = "Id", ResourceType = typeof(Resources.BIL.ActingBill))]
		public Int32 Id { get; set; }
        public Int32 PlanBill { get; set; }
		//[Display(Name = "OrderNo", ResourceType = typeof(Resources.BIL.ActingBill))]
		public string OrderNo { get; set; }
		//[Display(Name = "IpNo", ResourceType = typeof(Resources.BIL.ActingBill))]
		public string IpNo { get; set; }
		//[Display(Name = "ExternalIpNo", ResourceType = typeof(Resources.BIL.ActingBill))]
		public string ExternalIpNo { get; set; }
		//[Display(Name = "ReceiptNo", ResourceType = typeof(Resources.BIL.ActingBill))]
		public string ReceiptNo { get; set; }
		//[Display(Name = "ExternalReceiptNo", ResourceType = typeof(Resources.BIL.ActingBill))]
		public string ExternalReceiptNo { get; set; }
		//[Display(Name = "Type", ResourceType = typeof(Resources.BIL.ActingBill))]
        public com.Sconit.CodeMaster.BillType Type { get; set; }
		//[Display(Name = "Item", ResourceType = typeof(Resources.BIL.ActingBill))]
		public string Item { get; set; }
		//[Display(Name = "ItemDescription", ResourceType = typeof(Resources.BIL.ActingBill))]
		public string ItemDescription { get; set; }
		//[Display(Name = "Uom", ResourceType = typeof(Resources.BIL.ActingBill))]
		public string Uom { get; set; }
		//[Display(Name = "UnitCount", ResourceType = typeof(Resources.BIL.ActingBill))]
		public Decimal UnitCount { get; set; }
		//[Display(Name = "BillAddress", ResourceType = typeof(Resources.BIL.ActingBill))]
        public CodeMaster.OrderBillTerm BillTerm { get; set; }
		public string BillAddress { get; set; }
		//[Display(Name = "BillAddressDescription", ResourceType = typeof(Resources.BIL.ActingBill))]
		public string BillAddressDescription { get; set; }
		//[Display(Name = "Party", ResourceType = typeof(Resources.BIL.ActingBill))]
		public string Party { get; set; }
		//[Display(Name = "PartyName", ResourceType = typeof(Resources.BIL.ActingBill))]
		public string PartyName { get; set; }
		//[Display(Name = "PriceList", ResourceType = typeof(Resources.BIL.ActingBill))]
		public string PriceList { get; set; }
		//[Display(Name = "Currency", ResourceType = typeof(Resources.BIL.ActingBill))]
		public string Currency { get; set; }
		//[Display(Name = "UnitPrice", ResourceType = typeof(Resources.BIL.ActingBill))]
		public Decimal UnitPrice { get; set; }
		//[Display(Name = "IsProvisionalEstimate", ResourceType = typeof(Resources.BIL.ActingBill))]
		public Boolean IsProvisionalEstimate { get; set; }
		//[Display(Name = "Tax", ResourceType = typeof(Resources.BIL.ActingBill))]
		public string Tax { get; set; }
		//[Display(Name = "IsIncludeTax", ResourceType = typeof(Resources.BIL.ActingBill))]
		public Boolean IsIncludeTax { get; set; }
		//[Display(Name = "BillAmount", ResourceType = typeof(Resources.BIL.ActingBill))]
		public Decimal BillAmount { get; set; }
		//[Display(Name = "BilledAmount", ResourceType = typeof(Resources.BIL.ActingBill))]
		public Decimal BilledAmount { get; set; }
        public Decimal VoidAmount { get; set; }
		//[Display(Name = "BillQty", ResourceType = typeof(Resources.BIL.ActingBill))]
		public Decimal BillQty { get; set; }
		//[Display(Name = "BilledQty", ResourceType = typeof(Resources.BIL.ActingBill))]
		public Decimal BilledQty { get; set; }
        public Decimal VoidQty { get; set; }
		//[Display(Name = "UnitQty", ResourceType = typeof(Resources.BIL.ActingBill))]
		public Decimal UnitQty { get; set; }
		//[Display(Name = "LocationFrom", ResourceType = typeof(Resources.BIL.ActingBill))]
		public string LocationFrom { get; set; }
		//[Display(Name = "EffectiveDate", ResourceType = typeof(Resources.BIL.ActingBill))]
		public DateTime EffectiveDate { get; set; }
        public Boolean IsClose { get; set; }
		//[Display(Name = "CreateUserId", ResourceType = typeof(Resources.BIL.ActingBill))]
		public Int32 CreateUserId { get; set; }
		//[Display(Name = "CreateUserName", ResourceType = typeof(Resources.BIL.ActingBill))]
		public string CreateUserName { get; set; }
		//[Display(Name = "CreateDate", ResourceType = typeof(Resources.BIL.ActingBill))]
		public DateTime CreateDate { get; set; }
		//[Display(Name = "LastModifyUserId", ResourceType = typeof(Resources.BIL.ActingBill))]
		public Int32 LastModifyUserId { get; set; }
		//[Display(Name = "LastModifyUserName", ResourceType = typeof(Resources.BIL.ActingBill))]
		public string LastModifyUserName { get; set; }
		//[Display(Name = "LastModifyDate", ResourceType = typeof(Resources.BIL.ActingBill))]
		public DateTime LastModifyDate { get; set; }
		//[Display(Name = "Version", ResourceType = typeof(Resources.BIL.ActingBill))]
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
            ActingBill another = obj as ActingBill;

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
