using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.BIL
{
    [Serializable]
    public partial class BillTransaction : EntityBase
    {
        #region O/R Mapping Properties
		
		//[Display(Name = "Id", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public Int32 Id { get; set; }
		//[Display(Name = "OrderNo", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public string OrderNo { get; set; }
		//[Display(Name = "IpNo", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public string IpNo { get; set; }
		//[Display(Name = "ExternalIpNo", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public string ExternalIpNo { get; set; }
		//[Display(Name = "ReceiptNo", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public string ReceiptNo { get; set; }
		//[Display(Name = "ExternalReceiptNo", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public string ExternalReceiptNo { get; set; }
		//[Display(Name = "IsIncludeTax", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public Boolean IsIncludeTax { get; set; }
		//[Display(Name = "Item", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public string Item { get; set; }
		//[Display(Name = "ItemDescription", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public string ItemDescription { get; set; }
		//[Display(Name = "Uom", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public string Uom { get; set; }
		//[Display(Name = "UnitCount", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public Decimal UnitCount { get; set; }
		//[Display(Name = "HuId", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public string HuId { get; set; }
		//[Display(Name = "TransactionType", ResourceType = typeof(Resources.BIL.BillTransaction))]
        public com.Sconit.CodeMaster.BillTransactionType TransactionType { get; set; }
		//[Display(Name = "BillAddress", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public string BillAddress { get; set; }
		//[Display(Name = "BillAddressDescription", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public string BillAddressDescription { get; set; }
		//[Display(Name = "Party", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public string Party { get; set; }
		//[Display(Name = "PartyName", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public string PartyName { get; set; }
		//[Display(Name = "PriceList", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public string PriceList { get; set; }
		//[Display(Name = "Currency", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public string Currency { get; set; }
		//[Display(Name = "UnitPrice", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public Decimal UnitPrice { get; set; }
		//[Display(Name = "IsProvisionalEstimate", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public Boolean IsProvisionalEstimate { get; set; }
		//[Display(Name = "Tax", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public string Tax { get; set; }
		//[Display(Name = "BillQty", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public Decimal BillQty { get; set; }
		//[Display(Name = "BillAmount", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public Decimal BillAmount { get; set; }
		//[Display(Name = "UnitQty", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public Decimal UnitQty { get; set; }
		//[Display(Name = "LocationFrom", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public string LocationFrom { get; set; }
		//[Display(Name = "SettleLocation", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public string SettleLocation { get; set; }
		//[Display(Name = "EffectiveDate", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public DateTime EffectiveDate { get; set; }
		//[Display(Name = "PlanBill", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public Int32 PlanBill { get; set; }
        //[Display(Name = "ActingBill", ResourceType = typeof(Resources.BIL.BillTransaction))]
        public Int32 ActingBill { get; set; }
		//[Display(Name = "CreateUserId", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public Int32 CreateUserId { get; set; }
		//[Display(Name = "CreateUserName", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public string CreateUserName { get; set; }
		//[Display(Name = "CreateDate", ResourceType = typeof(Resources.BIL.BillTransaction))]
		public DateTime CreateDate { get; set; }
        
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
            BillTransaction another = obj as BillTransaction;

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
