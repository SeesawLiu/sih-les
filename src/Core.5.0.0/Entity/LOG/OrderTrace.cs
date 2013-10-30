using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

//TODO: Add other using statements here

namespace com.Sconit.Entity.LOG
{
    public partial class OrderTrace
    {
        #region Non O/R Mapping Properties

        [Display(Name = "VanOrderTrace_ReqTimeFromTo", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
        public string ReqTimeFromTo
        {
            get
            {
                if (this.ReqTimeFrom.HasValue && this.ReqTimeTo.HasValue)
                {
                    return this.ReqTimeFrom.Value.ToString(BusinessConstants.LONG_DATE_FORMAT) + " - " + ReqTimeTo.Value.ToString(BusinessConstants.LONG_DATE_FORMAT);
                }
                else if (this.ReqTimeTo.HasValue)
                {
                    return " < " + ReqTimeTo.Value.ToString(BusinessConstants.LONG_DATE_FORMAT);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.OrderPriority, ValueField = "Priority")]
        [Display(Name = "VanOrderTrace_Priority", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
        public string OrderPriorityDescription { get; set; }

        #endregion
    }
}