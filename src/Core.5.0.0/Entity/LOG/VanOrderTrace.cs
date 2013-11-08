using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

//TODO: Add other using statements here

namespace com.Sconit.Entity.LOG
{
    public partial class VanOrderTrace
    {
        #region Non O/R Mapping Properties

        //TODO: Add Non O/R Mapping Properties here. 
        //[Display(Name = "VanOrderTrace_WindowTimeFormTo", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
        //public string WindowTimeFromTo {
        //    get {
        //        return this.WindowTime.Value.ToString(BusinessConstants.LONG_DATE_FORMAT) + " - " + WindowTime2.Value.ToString(BusinessConstants.LONG_DATE_FORMAT);
        //    }
        //}

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

        [Export(ExportName = "ExportJITInfo", ExportSeq = 50)]
        [Display(Name = "OrderMaster_TraceCode", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string TraceCode { get; set; }

        [Export(ExportName = "ExportJITInfo", ExportSeq = 60)]
        [Display(Name = "VanOrderBomTrace_OrderQty", ResourceType = typeof(Resources.LOG.VanOrderBomTrace))]
        public Decimal OrderQty { get; set; }


        [Export(ExportName = "ExportJITInfo", ExportSeq = 80)]
        [Display(Name = "Item_Container", ResourceType = typeof(Resources.MD.Item))]
        public string Container { get; set; }

        #endregion
    }
}