using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.ORD
{
    public partial class VanOrderSeq
    {
        [CodeDetailDescription(CodeMaster = com.Sconit.CodeMaster.CodeMaster.OrderStatus, ValueField = "Status")]
        [Display(Name = "OrderMaster_Status", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string OrderStatusDescription { get; set; }

        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.PauseStatus, ValueField = "PauseStatus")]
        [Display(Name = "OrderMaster_PauseStatus", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string PauseStatusDescription { get; set; }
    }
}
