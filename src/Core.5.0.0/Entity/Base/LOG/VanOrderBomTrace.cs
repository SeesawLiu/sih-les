using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.LOG
{
    [Serializable]
    public partial class VanOrderBomTrace : EntityBase
    {
        #region O/R Mapping Properties
		
		public Int64 Id { get; set; }
		public string UUID { get; set; }
        [Display(Name = "VanOrderBomTrace_ProdLine", ResourceType = typeof(Resources.LOG.VanOrderBomTrace))]
		public string ProdLine { get; set; }
        [Display(Name = "VanOrderBomTrace_OrderNo", ResourceType = typeof(Resources.LOG.VanOrderBomTrace))]
        public string VanOrderNo { get; set; }
        public Int32? VanOrderBomDetId { get; set; }
        [Display(Name = "VanOrderTrace_Item", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
        public string Item { get; set; }

        [Display(Name = "VanOrderTrace_RefItemCode", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
        public string RefItemCode { get; set; }

        [Display(Name = "VanOrderTrace_ItemDesc", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
        public string ItemDesc { get; set; }
        //[Display(Name = "VanOrderBomTrace_ProdCode", ResourceType = typeof(Resources.LOG.VanOrderBomTrace))]
        //public string ProdCode { get; set; }
        //[Display(Name = "VanOrderBomTrace_ProdSeq", ResourceType = typeof(Resources.LOG.VanOrderBomTrace))]
        //public string ProdSeq { get; set; }
        public string OpReference { get; set; }
        public string LocFrom { get; set; }
        public string LocTo { get; set; }
        // [Display(Name = "VanOrderBomTrace_UPGCode", ResourceType = typeof(Resources.LOG.VanOrderBomTrace))]
        //public string UPGCode { get; set; }
        //[Display(Name = "VanOrderBomTrace_Location", ResourceType = typeof(Resources.LOG.VanOrderBomTrace))]
        //public string Location { get; set; }
        [Display(Name = "VanOrderBomTrace_OrderQty", ResourceType = typeof(Resources.LOG.VanOrderBomTrace))]
		public Decimal OrderQty { get; set; }
        [Display(Name = "VanOrderBomTrace_CPTime", ResourceType = typeof(Resources.LOG.VanOrderBomTrace))]
		public DateTime CPTime { get; set; }
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
            VanOrderBomTrace another = obj as VanOrderBomTrace;

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
