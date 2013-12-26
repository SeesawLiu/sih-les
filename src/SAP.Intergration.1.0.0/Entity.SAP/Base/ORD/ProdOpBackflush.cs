using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.SAP.ORD
{
    [Serializable]
    public partial class ProdOpBackflush : EntityBase, ITraceable
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }
        public Int32 SAPOpReportId { get; set; }

        [Export(ExportName = "ExportProdOpBackflush", ExportSeq = 10)]
        [Display(Name = "ProdOpBackflush_AUFNR", ResourceType = typeof(Resources.SI.ProdOpBackflush))]
        public string AUFNR { get; set; }
		public string WERKS { get; set; }
        public string AUFPL { get; set; }
        public string APLZL { get; set; }
		public string PLNTY { get; set; }
		public string PLNNR { get; set; }
        public string PLNAL { get; set; }
        public string PLNFL { get; set; }
		public string VORNR { get; set; }
		public string ARBPL { get; set; }
		public string RUEK { get; set; }
		public string AUTWE { get; set; }
        [Export(ExportName = "ExportProdOpBackflush", ExportSeq = 20)]
        [Display(Name = "ProdOpBackflush_WORKCENTER", ResourceType = typeof(Resources.SI.ProdOpBackflush))]
        public string WORKCENTER { get; set; }
        [Export(ExportName = "ExportProdOpBackflush", ExportSeq = 30)]
        [Display(Name = "ProdOpBackflush_GAMNG", ResourceType = typeof(Resources.SI.ProdOpBackflush))]
        public Decimal GAMNG { get; set; }
        [Export(ExportName = "ExportProdOpBackflush", ExportSeq = 40)]
        [Display(Name = "ProdOpBackflush_SCRAP", ResourceType = typeof(Resources.SI.ProdOpBackflush))]
        public Decimal SCRAP { get; set; }
        [Export(ExportName = "ExportProdOpBackflush", ExportSeq = 50)]
        [Display(Name = "ProdOpBackflush_Status", ResourceType = typeof(Resources.SI.ProdOpBackflush))]
        public StatusEnum Status { get; set; }
        [Export(ExportName = "ExportProdOpBackflush", ExportSeq = 90)]
        [Display(Name = "ProdOpBackflush_CreateDate", ResourceType = typeof(Resources.SI.ProdOpBackflush))]
        public DateTime CreateDate { get; set; }
		public DateTime LastModifyDate { get; set; }
        [Export(ExportName = "ExportProdOpBackflush", ExportSeq = 60)]
        [Display(Name = "ProdOpBackflush_ErrorCount", ResourceType = typeof(Resources.SI.ProdOpBackflush))]
        public Int32 ErrorCount { get; set; }
        [Export(ExportName = "ExportProdOpBackflush", ExportSeq = 80)]
        [Display(Name = "ProdOpBackflush_ProdLine", ResourceType = typeof(Resources.SI.ProdOpBackflush))]
        public string ProdLine { get; set; }
        [Export(ExportName = "ExportProdOpBackflush", ExportSeq = 70)]
        [Display(Name = "ProdOpBackflush_OrderNo", ResourceType = typeof(Resources.SI.ProdOpBackflush))]
        public string OrderNo { get; set; }
        public string ReceiptNo { get; set; }
		public Int32 OrderOpId { get; set; }
		public Int32 OrderOpReportId { get; set; }
		public Int32 Version { get; set; }
        public DateTime EffectiveDate { get; set; }
        
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
            ProdOpBackflush another = obj as ProdOpBackflush;

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
