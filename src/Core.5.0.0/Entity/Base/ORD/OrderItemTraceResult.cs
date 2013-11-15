using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.ORD
{
    [Serializable]
    public partial class OrderItemTraceResult : EntityBase, IAuditable
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }
		public Int32? OrderItemTraceId { get; set; }
        [Export(ExportName = "OrderItemTraceResultXLS", ExportSeq = 80)]
        [Display(Name = "OrderItemTraceResult_BarCode", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public string BarCode { get; set; }

        [Export(ExportName = "OrderItemTraceResultXLS", ExportSeq = 10)]
        [Display(Name = "OrderItemTraceResult_Supplier", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public string Supplier { get; set; }
        [Export(ExportName = "OrderItemTraceResultXLS", ExportSeq = 50)]
        [Display(Name = "OrderItemTraceResult_LotNo", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public string LotNo { get; set; }
        [Export(ExportName = "OrderItemTraceResultXLS", ExportSeq = 70)]
        [Display(Name = "OrderItemTraceResult_OpReference", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public string OpReference { get; set; }
        [Display(Name = "OrderItemTraceResult_Item", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        [Export(ExportName = "OrderItemTraceResultXLS", ExportSeq = 20)]
        public string Item { get; set; }
        [Export(ExportName = "OrderItemTraceResultXLS", ExportSeq = 40)]
        [Display(Name = "OrderItemTraceResult_ItemDesc", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public string ItemDescription { get; set; }
        [Export(ExportName = "OrderItemTraceResultXLS", ExportSeq = 30)]
        [Display(Name = "OrderItemTraceResult_ReferenceItemCode", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public string ReferenceItemCode { get; set; }
        [Export(ExportName = "OrderItemTraceResultXLS", ExportSeq = 110)]
        [Display(Name = "ErrorBarCode_IsWithdraw", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public Boolean IsWithdraw { get; set; }
        [Display(Name = "OrderItemTraceResult_OrderNo", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public string OrderNo { get; set; }
		public Int32 CreateUserId { get; set; }
        [Export(ExportName = "OrderItemTraceResultXLS", ExportSeq = 100)]
        [Display(Name = "OrderItemTraceResult_CreateUserName", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public string CreateUserName { get; set; }
        [Export(ExportName = "OrderItemTraceResultXLS", ExportSeq = 90)]
        [Display(Name = "OrderItemTraceResult_CreateDate", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public DateTime CreateDate { get; set; }
		public Int32 LastModifyUserId { get; set; }
		public string LastModifyUserName { get; set; }
		public DateTime LastModifyDate { get; set; }
        [Export(ExportName = "OrderItemTraceResultXLS", ExportSeq = 60)]
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
