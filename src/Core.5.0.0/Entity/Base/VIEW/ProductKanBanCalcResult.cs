using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.VIEW
{
    [Serializable]
    public partial class ProductKanBanCalcResult : EntityBase
    {
        #region O/R Mapping Properties

		public Int32 Id { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 1)]
        [Display(Name = "ProductKanBanCalcResult_FlowDetId", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
		public Int32 FlowDetId { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 2)]
        [Display(Name = "ProductKanBanCalcResult_Item", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public string Item { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 3)]
        [Display(Name = "ProductKanBanCalcResult_ItemDesc", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public string ItemDesc { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 4)]
        [Display(Name = "ProductKanBanCalcResult_Flow", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public string Flow { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 4)]
        [Display(Name = "ProductKanBanCalcResult_VanSeries", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public string VanSeries { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 5)]
        [Display(Name = "ProductKanBanCalcResult_SupplyPoint", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public string SupplyPoint { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 6)]
        [Display(Name = "ProductKanBanCalcResult_LocBin", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public string LocBin { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 7)]
        [Display(Name = "ProductKanBanCalcResult_KBRestorePoint", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public string KBRestorePoint { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 8)]
        [Display(Name = "ProductKanBanCalcResult_GroupNo", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public string GroupNo { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 9)]
        [Display(Name = "ProductKanBanCalcResult_GroupDescription", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public string GroupDescription { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 10)]
        [Display(Name = "ProductKanBanCalcResult_UintCount", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public Decimal UnitCount { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 11)]
        [Display(Name = "ProductKanBanCalcResult_BatchSize", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public Decimal BatchSize { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 12)]
        [Display(Name = "ProductKanBanCalcResult_DailyQty", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public Decimal DailyQty { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 13)]
        [Display(Name = "ProductKanBanCalcResult_Shift", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public string Shift { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 14)]
        [Display(Name = "ProductKanBanCalcResult_SafeTime", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public Decimal SafeTime { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 15)]
        [Display(Name = "ProductKanBanCalcResult_SafeStock", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public Decimal SafeStock { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 16)]
        [Display(Name = "ProductKanBanCalcResult_SafeQty", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public Decimal SafeQty { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 17)]
        [Display(Name = "ProductKanBanCalcResult_BatchQty", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public Decimal BatchQty { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 18)]
        [Display(Name = "ProductKanBanCalcResult_IsInput", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public string IsInput { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 19)]
        [Display(Name = "ProductKanBanCalcResult_InputBfTime", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public Int32 InputBfTime { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 20)]
        [Display(Name = "ProductKanBanCalcResult_InputCmd", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public Int32 InputCmd { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 21)]
        [Display(Name = "ProductKanBanCalcResult_Scqzsj", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public Int32 Scqzsj { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 22)]
        [Display(Name = "ProductKanBanCalcResult_RequireCmd", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public String RequireCmd { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 23)]
        [Display(Name = "ProductKanBanCalcResult_CurrKanbanNum", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public Int32 CurrKanbanNum { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 24)]
        [Display(Name = "ProductKanBanCalcResult_CmdPoint", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public Int32 CmdPoint { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 25)]
        [Display(Name = "ProductKanBanCalcResult_KanbanNum", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public Int32 KanbanNum { get; set; }

        [Export(ExportName = "ProductKanBanCalcResult", ExportSeq = 26)]
        [Display(Name = "ProductKanBanCalcResult_KanbanDeltaNum", ResourceType = typeof(Resources.View.ProductKanBanCalcResult))]
        public Int32 KanbanDeltaNum { get; set; }

        public string BatchNo { get; set; }
        
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
            ProductKanBanCalcResult another = obj as ProductKanBanCalcResult;

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
