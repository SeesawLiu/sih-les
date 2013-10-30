using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.LOG
{
    public partial class ProdOrderPauseResume
    {
        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.OrderPauseType, ValueField = "OprateType")]
        [Display(Name = "ProdOrderPauseResume_OprateType", ResourceType = typeof(Resources.LOG.ProdOrderPauseResume))]
        public string OprateTypeDescription { get; set; }
    }
}
