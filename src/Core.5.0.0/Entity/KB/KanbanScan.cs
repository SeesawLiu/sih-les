using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.KB
{
    public partial class KanbanScan
    {
        #region Non O/R Mapping Properties

        // uc
        //[Display(Name = "KanbanScan_Qty", ResourceType = typeof(Resources.KB.KanbanScan))]
        //public Decimal Qty { get; set; }

        // ucdesc
        [Display(Name = "KanbanScan_Container", ResourceType = typeof(Resources.KB.KanbanScan))]
        public string Container { get; set; }

        // location bin
        [Display(Name = "KanbanScan_LocBin", ResourceType = typeof(Resources.KB.KanbanScan))]
        public string LocBin { get; set; }

        // shelf
        [Display(Name = "KanbanScan_Shelf", ResourceType = typeof(Resources.KB.KanbanScan))]
        public string Shelf { get; set; }

        // qitiaobian
        [Display(Name = "KanbanScan_QiTiaoBian", ResourceType = typeof(Resources.KB.KanbanScan))]
        public string QiTiaoBian { get; set; }

        public CodeMaster.KBProcessCode Ret { get; set; }
        [Display(Name = "Msg", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string Msg { get; set; }

        [Display(Name = "OpTime", ResourceType = typeof(Resources.KB.KanbanCard))]
        public DateTime OpTime { get; set; }

        [Display(Name = "CalcedOrderTime", ResourceType = typeof(Resources.KB.KanbanCard))]
        public DateTime CalcedOrderTime { get; set; }


        public Decimal? UnitCount { get; set; }


        [Export(ExportName = "ExportKanbanScanXLS", ExportSeq = 130)]
        [Display(Name = "KanbanCard_OpRefSequence", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string KanbanNo { get; set; }
        #endregion
    }
}