using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.BIL
{
    [Serializable]
    public partial class BillDetail : EntityBase, IAuditable
    {
        #region O/R Mapping Properties
		
		//[Display(Name = "Id", ResourceType = typeof(Resources.BIL.BillDetail))]
		public Int32 Id { get; set; }
		//[Display(Name = "BillNo", ResourceType = typeof(Resources.BIL.BillDetail))]
		public string BillNo { get; set; }
		//[Display(Name = "ActingBillId", ResourceType = typeof(Resources.BIL.BillDetail))]
		public Int32 ActingBillId { get; set; }
		//[Display(Name = "Item", ResourceType = typeof(Resources.BIL.BillDetail))]
		public string Item { get; set; }
		//[Display(Name = "ItemDescription", ResourceType = typeof(Resources.BIL.BillDetail))]
		public string ItemDescription { get; set; }
		//[Display(Name = "Uom", ResourceType = typeof(Resources.BIL.BillDetail))]
		public string Uom { get; set; }
		//[Display(Name = "UnitCount", ResourceType = typeof(Resources.BIL.BillDetail))]
		public Decimal UnitCount { get; set; }
		//[Display(Name = "Qty", ResourceType = typeof(Resources.BIL.BillDetail))]
		public Decimal Qty { get; set; }
		//[Display(Name = "PriceList", ResourceType = typeof(Resources.BIL.BillDetail))]
		public string PriceList { get; set; }
		//[Display(Name = "Amount", ResourceType = typeof(Resources.BIL.BillDetail))]
		public Decimal Amount { get; set; }
		//[Display(Name = "UnitPrice", ResourceType = typeof(Resources.BIL.BillDetail))]
		public Decimal UnitPrice { get; set; }
		//[Display(Name = "OrderNo", ResourceType = typeof(Resources.BIL.BillDetail))]
		public string OrderNo { get; set; }
		//[Display(Name = "IpNo", ResourceType = typeof(Resources.BIL.BillDetail))]
		public string IpNo { get; set; }
		//[Display(Name = "ExternalIpNo", ResourceType = typeof(Resources.BIL.BillDetail))]
		public string ExternalIpNo { get; set; }
		//[Display(Name = "ReceiptNo", ResourceType = typeof(Resources.BIL.BillDetail))]
		public string ReceiptNo { get; set; }
		//[Display(Name = "ExtReceiptNo", ResourceType = typeof(Resources.BIL.BillDetail))]
		public string ExtReceiptNo { get; set; }
		//[Display(Name = "CreateUserId", ResourceType = typeof(Resources.BIL.BillDetail))]
		public Int32 CreateUserId { get; set; }
		//[Display(Name = "CreateUserName", ResourceType = typeof(Resources.BIL.BillDetail))]
		public string CreateUserName { get; set; }
		//[Display(Name = "CreateDate", ResourceType = typeof(Resources.BIL.BillDetail))]
		public DateTime CreateDate { get; set; }
		//[Display(Name = "LastModifyUserId", ResourceType = typeof(Resources.BIL.BillDetail))]
		public Int32 LastModifyUserId { get; set; }
		//[Display(Name = "LastModifyUserName", ResourceType = typeof(Resources.BIL.BillDetail))]
		public string LastModifyUserName { get; set; }
		//[Display(Name = "LastModifyDate", ResourceType = typeof(Resources.BIL.BillDetail))]
		public DateTime LastModifyDate { get; set; }
		//[Display(Name = "Version", ResourceType = typeof(Resources.BIL.BillDetail))]
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
            BillDetail another = obj as BillDetail;

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
