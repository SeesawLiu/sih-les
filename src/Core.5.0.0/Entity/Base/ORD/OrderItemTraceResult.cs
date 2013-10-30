using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.ORD
{
    [Serializable]
    public partial class OrderItemTraceResult : EntityBase, IAuditable
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }
		public Int32? OrderItemTraceId { get; set; }
        [Display(Name = "OrderItemTraceResult_BarCode", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public string BarCode { get; set; }
		public string Supplier { get; set; }
		public string LotNo { get; set; }
        [Display(Name = "OrderItemTraceResult_OpReference", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public string OpReference { get; set; }
        [Display(Name = "OrderItemTraceResult_Item", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public string Item { get; set; }
        [Display(Name = "OrderItemTraceResult_ItemDesc", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public string ItemDescription { get; set; }
        public string ReferenceItemCode { get; set; }
        [Display(Name = "ErrorBarCode_IsWithdraw", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public Boolean IsWithdraw { get; set; }
        [Display(Name = "OrderItemTraceResult_OrderNo", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public string OrderNo { get; set; }
		public Int32 CreateUserId { get; set; }
		public string CreateUserName { get; set; }
		public DateTime CreateDate { get; set; }
		public Int32 LastModifyUserId { get; set; }
		public string LastModifyUserName { get; set; }
		public DateTime LastModifyDate { get; set; }
        [Display(Name = "OrderItemTraceResult_TraceCode", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public string TraceCode { get; set; }
        
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
            OrderItemTraceResult another = obj as OrderItemTraceResult;

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
