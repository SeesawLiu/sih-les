using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.VIEW
{
    public partial class AliquotStartTaskView
    {
        public Int32 RowId { get; set; }

        [Export(ExportName = "AliquotStartTaskView",ExportSeq = 1)]
        [Display(Name = "AliquotStartTask_Flow", ResourceType = typeof(Resources.ORD.AliquotStartTask))]
        public string Flow { get; set; }

        public string OrderNo { get; set; }

        [Export(ExportName = "AliquotStartTaskView", ExportSeq = 2)]
        [Display(Name = "AliquotStartTask_TraceCode", ResourceType = typeof(Resources.ORD.AliquotStartTask))]
        public string TraceCode { get; set; }

        [Export(ExportName = "AliquotStartTaskView", ExportSeq = 3)]
        [Display(Name = "AliquotStartTask_Item", ResourceType = typeof(Resources.ORD.AliquotStartTask))]
        public string Item { get; set; }

        [Export(ExportName = "AliquotStartTaskView", ExportSeq = 4)]
        [Display(Name = "AliquotStartTask_ItemDesc", ResourceType = typeof(Resources.ORD.AliquotStartTask))]
        public string ItemDesc { get; set; }

        [Export(ExportName = "AliquotStartTaskView", ExportSeq = 5)]
        [Display(Name = "AliquotStartTask_StartTime", ResourceType = typeof(Resources.ORD.AliquotStartTask))]
        public DateTime? StartTime { get; set; }
    }
}
