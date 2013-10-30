using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.VIEW
{
    [Serializable]
    public partial class KanBanCalcResult : EntityBase
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=1)]
        [Display(Name = "KanBanCalcResult_FlowDetId", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public Int32? FlowDetId { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=2)]
        [Display(Name = "KanBanCalcResult_Item", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public string Item { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=3)]
        [Display(Name = "KanBanCalcResult_ItemDesc", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public string ItemDesc { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=4)]
        [Display(Name = "KanBanCalcResult_QiTiaoBian", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public string QiTiaoBian { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=5)]
        [Display(Name = "KanBanCalcResult_UnitCount", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public Decimal UnitCount { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=6)]
        [Display(Name = "KanBanCalcResult_UCDesc", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public string UCDesc { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=7)]
        [Display(Name = "KanBanCalcResult_PartyTo", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public string Region { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq = 8)]
        [Display(Name = "KanBanCalcResult_LocTo", ResourceType = typeof(Resources.View.KanBanCalcResult))]
        public string Location { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq = 9)]
        [Display(Name = "KanBanCalcResult_LocBin", ResourceType = typeof(Resources.View.KanBanCalcResult))]
        public string LocBin { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq = 10)]
        [Display(Name = "KanBanCalcResult_OpRef", ResourceType = typeof(Resources.View.KanBanCalcResult))]
        public string OpRef { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=11)]
        [Display(Name = "KanBanCalcResult_Shelf", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public string Shelf { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=12)]
        [Display(Name = "KanBanCalcResult_BarCode", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public string BarCode { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq = 13)]
        [Display(Name = "KanBanCalcResult_BianHao", ResourceType = typeof(Resources.View.KanBanCalcResult))]
        public string BianHao { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=14)]
        [Display(Name = "KanBanCalcResult_Lot", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public string Lot { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=15)]
        [Display(Name = "KanBanCalcResult_Supplier", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public string Supplier { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=16)]
        [Display(Name = "KanBanCalcResult_SupplierName", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public string SupplierName { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=17)]
        [Display(Name = "KanBanCalcResult_EffFreDate", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public DateTime EffFreDate { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=18)]
        [Display(Name = "KanBanCalcResult_LCCode", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public string LCCode { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=19)]
        [Display(Name = "KanBanCalcResult_MaxQty", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public Decimal MaxQty { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=20)]
        [Display(Name = "KanBanCalcResult_Type", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public Int32 Type { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=21)]
        [Display(Name = "KanBanCalcResult_TravelTimes", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public string TravelTimes { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=22)]
        [Display(Name = "KanBanCalcResult_LeadTime", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public Decimal LeadTime { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=23)]
        [Display(Name = "KanBanCalcResult_SafeTime", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public Decimal SafeTime { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=24)]
        [Display(Name = "KanBanCalcResult_CalcResult", ResourceType = typeof(Resources.View.KanBanCalcResult))]
        public Decimal CalcResult { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=25)]
        [Display(Name = "KanBanCalcResult_Qty", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public Decimal Qty { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=26)]
        [Display(Name = "KanBanCalcResult_CodeType", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public string CodeType { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=27)]
        [Display(Name = "KanBanCalcResult_KanbanNum", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public Int32 KanbanNum { get; set; }

        [Export(ExportName = "KanBanCalcResult", ExportSeq=28)]
        [Display(Name = "KanBanCalcResult_KanbanDeltaNum", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public Int32 KanbanDeltaNum { get; set; }

        [Display(Name = "KanBanCalcResult_NewNo", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public string NewNo { get; set; }

        [Display(Name = "KanBanCalcResult_FreezeNo", ResourceType = typeof(Resources.View.KanBanCalcResult))]
		public string FreezeNo { get; set; }

        //[Export(ExportName = "KanBanCalcResult", ExportSeq=31)]
        //[Display(Name = "KanBanCalcResult_BatchNo", ResourceType = typeof(Resources.View.KanBanCalcResult))]
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
            KanBanCalcResult another = obj as KanBanCalcResult;

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
