using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.CUST
{
    [Serializable]
    public partial class ItemDailyConsume : EntityBase
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }

        [Export(ExportName = "ItemDailyConsume",ExportSeq=30)]
        [Display(Name = "ItemDailyConsume_Item", ResourceType = typeof(Resources.CUST.ItemDailyConsume))]
		public string Item { get; set; }

        [Export(ExportName = "ItemDailyConsume", ExportSeq = 35)]
        [Display(Name = "ItemDailyConsume_ItemDesc", ResourceType = typeof(Resources.CUST.ItemDailyConsume))]
        public string ItemDesc { get; set; }

        [Export(ExportName = "ItemDailyConsume", ExportSeq = 10)]
        [Display(Name = "ItemDailyConsume_Location", ResourceType = typeof(Resources.CUST.ItemDailyConsume))]
		public string Location { get; set; }

        [Export(ExportName = "ItemDailyConsume", ExportSeq = 40)]
        [Display(Name = "ItemDailyConsume_Qty", ResourceType = typeof(Resources.CUST.ItemDailyConsume))]
		public Decimal Qty { get; set; }

        [Export(ExportName = "ItemDailyConsume", ExportSeq = 50)]
        [Display(Name = "ItemDailyConsume_OriginalQty", ResourceType = typeof(Resources.CUST.ItemDailyConsume))]
        public Decimal OriginalQty { get; set; }

        [Export(ExportName = "ItemDailyConsume", ExportSeq = 20)]
        [Display(Name = "ItemDailyConsume_ConsumeDate", ResourceType = typeof(Resources.CUST.ItemDailyConsume))]
		public DateTime ConsumeDate { get; set; }

        [Export(ExportName = "ItemDailyConsume", ExportSeq = 70)]
        [Display(Name = "ItemDailyConsume_SubstituteGroup", ResourceType = typeof(Resources.CUST.ItemDailyConsume))]
        public string SubstituteGroup { get; set; }

        [Export(ExportName = "ItemDailyConsume", ExportSeq = 60)]
        [Display(Name = "ItemDailyConsume_MultiSupplyGroup", ResourceType = typeof(Resources.CUST.ItemDailyConsume))]
        public string MultiSupplyGroup { get; set; }

        [Display(Name = "ItemDailyConsume_CreateDate", ResourceType = typeof(Resources.CUST.ItemDailyConsume))]
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
            ItemDailyConsume another = obj as ItemDailyConsume;

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
