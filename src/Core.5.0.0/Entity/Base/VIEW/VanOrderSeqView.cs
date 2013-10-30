using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.VIEW
{
    [Serializable]
    public partial class VanOrderSeqView : EntityBase
    {
        public Int32 Id { get; set; }

        [Export(ExportName = "VanOrderSeqView", ExportSeq = 1)]
        [Display(Name = "OrderMaster_Flow", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string Flow { get; set; }

        [Export(ExportName = "VanOrderSeqView", ExportSeq = 11)]
        [Display(Name = "OrderMaster_OrderNo", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string OrderNo { get; set; }

        [Export(ExportName = "VanOrderSeqView", ExportSeq = 31)]
        [Display(Name = "OrderMaster_TraceCode", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string TraceCode { get; set; }

        [Export(ExportName = "VanOrderSeqView", ExportSeq = 21)]
        [Display(Name = "OrderMaster_ExternalOrderNo", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string ExternalOrderNo { get; set; }

        [Export(ExportName = "VanOrderSeqView", ExportSeq = 71)]
        [Display(Name = "OrderMaster_CurrentOperation", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public Int32? CurrentOperation { get; set; }

        [Display(Name = "OrderMaster_Status", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public CodeMaster.OrderStatus? Status { get; set; }

        [Display(Name = "OrderMaster_PauseStatus", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public CodeMaster.PauseStatus? PauseStatus { get; set; }

        [Export(ExportName = "VanOrderSeqView", ExportSeq = 41)]
        [Display(Name = "OrderMaster_Sequence", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public Int64 Sequence { get; set; }

        [Export(ExportName = "VanOrderSeqView", ExportSeq = 51)]
        [Display(Name = "OrderMaster_SubSequence", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public Int64 SubSequence { get; set; }
    }
}
