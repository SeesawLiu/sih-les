using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.VIEW
{
    public partial class VanOrderSeqView
    {
        [Export(ExportName = "VanOrderSeqView", ExportSeq = 61)]
        [CodeDetailDescription(CodeMaster = com.Sconit.CodeMaster.CodeMaster.OrderStatus, ValueField = "Status")]
        [Display(Name = "OrderMaster_Status", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string OrderStatusDescription { get; set; }

        [Export(ExportName = "VanOrderSeqView", ExportSeq = 81)]
        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.PauseStatus, ValueField = "PauseStatus")]
        [Display(Name = "OrderMaster_PauseStatus", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string PauseStatusDescription { get; set; }
    }
}
