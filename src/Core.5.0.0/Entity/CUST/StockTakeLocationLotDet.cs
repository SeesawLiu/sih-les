using System;
using com.Sconit.Entity.SYS;
using System.ComponentModel.DataAnnotations;

//TODO: Add other using statements here

namespace com.Sconit.Entity.CUST
{
    public partial class StockTakeLocationLotDet
    {
        #region Non O/R Mapping Properties

        //TODO: Add Non O/R Mapping Properties here. 

        #endregion



        [Export(ExportName = "ExportBackUpInvXLS", ExportSeq = 55)]
        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.OrderPriority, ValueField = "QualityType")]
        [Display(Name = "LocationDetailView_QualityType", ResourceType = typeof(Resources.View.LocationDetailView))]
        public string QualityTypeDescription { get; set; }
    }
}