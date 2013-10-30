using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.ORD
{
    public partial class CabProductionView
    {
        [CodeDetailDescription(CodeMaster = com.Sconit.CodeMaster.CodeMaster.CabOutStatus, ValueField = "CabOutStatus")]
        [Display(Name = "OrderMaster_Status", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string CabOutStatusDescription { get; set; }
    }
}
