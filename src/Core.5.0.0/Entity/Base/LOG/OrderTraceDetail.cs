using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.LOG
{
    [Serializable]
    public partial class OrderTraceDetail : EntityBase
    {
        #region O/R Mapping Properties
		
		public Int64 Id { get; set; }
		public string UUID { get; set; }
        [Display(Name = "OrderTraceDetail_Type", ResourceType = typeof(Resources.LOG.VanOrderBomTrace))]
        public string Type { get; set; }
        [Display(Name = "VanOrderTrace_Item", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
        public string Item { get; set; }
        [Display(Name = "VanOrderTrace_RefItemCode", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
        public string RefItemCode { get; set; }

        [Display(Name = "VanOrderTrace_ItemDesc", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
        public string ItemDesc { get; set; }
        public string ManufactureParty { get; set; }
        [Display(Name = "OrderTraceDetail_Location", ResourceType = typeof(Resources.LOG.VanOrderBomTrace))]
        public string Location { get; set; }
        [Display(Name = "VanOrderTrace_OrderNo", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
        public string OrderNo { get; set; }
        [Display(Name = "OrderTraceDetail_RequestTime", ResourceType = typeof(Resources.LOG.VanOrderBomTrace))]
        public DateTime? RequestTime { get; set; }
        [Display(Name = "OrderTraceDetail_OrderQty", ResourceType = typeof(Resources.LOG.VanOrderBomTrace))]
        public Decimal OrderQty { get; set; }
        [Display(Name = "OrderTraceDetail_FinishQty", ResourceType = typeof(Resources.LOG.VanOrderBomTrace))]
        public Decimal FinishQty { get; set; }
		public DateTime CreateDate { get; set; }
        
        #endregion

		public override int GetHashCode()
        {
			if (Id != null)
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
            OrderTraceDetail another = obj as OrderTraceDetail;

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
