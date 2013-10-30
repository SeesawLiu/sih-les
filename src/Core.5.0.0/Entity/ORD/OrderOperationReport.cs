using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

//TODO: Add other using statements here

namespace com.Sconit.Entity.ORD
{
    public partial class OrderOperationReport
    {
        #region Non O/R Mapping Properties

        //TODO: Add Non O/R Mapping Properties here. 

        #endregion

        [Export(ExportName = "ExportDetailXLS", ExportSeq = 10)]
        [Export(ExportName = "SearchExportXLS", ExportSeq = 10)]
        [Display(Name = "OrderOperation_Flow", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public string Flow { get; set; }

        [Export(ExportName = "ExportDetailXLS", ExportSeq = 15)]
        [Display(Name = "OrderOperation_VanNo", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public string VanNo { get; set; }
    }
}