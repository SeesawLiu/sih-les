using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.LOG
{
    [Serializable]
    public partial class VanOrderTrace : EntityBase
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }
		public string UUID { get; set; }
        [Display(Name = "VanOrderTrace_Flow", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
		public string Flow { get; set; }
        [Display(Name = "VanOrderTrace_OrderNo", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
		public string OrderNo { get; set; }
        [Display(Name = "VanOrderTrace_Priority", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
		public com.Sconit.CodeMaster.OrderPriority? Priority { get; set; }
        public Int32? OrderDetId { get; set; }
        public Int32? OrderDetSeq { get; set; }
        //[Display(Name = "VanOrderTrace_StartTime", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
        public DateTime? StartTime { get; set; }
        [Display(Name = "VanOrderTrace_WindowTime", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
        public DateTime? WindowTime { get; set; }

        public DateTime? EMWindowTime { get; set; }

        public DateTime? ReqTimeFrom { get; set; }
        public DateTime? ReqTimeTo { get; set; }

        [Display(Name = "VanOrderTrace_Item", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
		public string Item { get; set; }

        [Display(Name = "VanOrderTrace_RefItemCode", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
        public string RefItemCode { get; set; }

        [Display(Name = "VanOrderTrace_ItemDesc", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
        public string ItemDesc { get; set; }

		public string Uom { get; set; }
        [Display(Name = "VanOrderTrace_UnitCount", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
		public Decimal? UnitCount { get; set; }

        public string ManufactureParty { get; set; }
        public string Location { get; set; }
        
        [Display(Name = "VanOrderTrace_OpReference", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
		public string OpReference { get; set; }
        [Display(Name = "VanOrderTrace_NetOrderQty", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
		public Decimal? NetOrderQty { get; set; }
        [Display(Name = "VanOrderTrace_OrgOpRefQty", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
		public Decimal? OrgOpRefQty { get; set; }
        [Display(Name = "VanOrderTrace_GrossOrderQty", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
		public Decimal? GrossOrderQty { get; set; }
        [Display(Name = "VanOrderTrace_OpRefQty", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
		public Decimal? OpRefQty { get; set; }
        [Display(Name = "VanOrderTrace_CreateDate", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
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
            VanOrderTrace another = obj as VanOrderTrace;

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
