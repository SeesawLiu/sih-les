using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.ORD
{
    [Serializable]
    public partial class OrderItemTrace : EntityBase
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }
        [Display(Name = "OrderItemTraceResult_OrderNo", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public string OrderNo { get; set; }
		public Int32 OrderBomId { get; set; }
        [Display(Name = "OrderItemTraceResult_Item", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public string Item { get; set; }
        [Display(Name = "OrderItemTraceResult_ItemDesc", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public string ItemDescription { get; set; }
        public string ReferenceItemCode { get; set; }
        public Int32 Op { get; set; }
        [Display(Name = "OrderItemTraceResult_OpReference", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public string OpReference { get; set; }
        [Display(Name = "OrderItemTraceResult_Qty", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public Decimal Qty { get; set; }
		public Decimal ScanQty { get; set; }
		public Int32 CreateUserId { get; set; }
		public string CreateUserName { get; set; }
		public DateTime CreateDate { get; set; }
		public Int32 LastModifyUserId { get; set; }
		public string LastModifyUserName { get; set; }
		public DateTime LastModifyDate { get; set; }
		public Int32 Version { get; set; }

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
            OrderItemTrace another = obj as OrderItemTrace;

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
