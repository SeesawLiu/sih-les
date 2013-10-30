using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;
using System;

namespace com.Sconit.Entity.VIEW
{
    public partial class FlowMasterView
    {
        [Export(ExportName = "SEQTransferFlow", ExportSeq = 10)]
        [Export(ExportName = "SEQProcurementFlow", ExportSeq = 9)]
        [Export(ExportName = "JITFlow", ExportSeq = 13)]
        [Export(ExportName = "KanBanFlow", ExportSeq = 14)]
        [Export(ExportName = "KBProductionFlow", ExportSeq = 18)]
        [Export(ExportName = "HemProductionFlow", ExportSeq = 21)]
        [Display(Name = "FlowMaster_IsActive", ResourceType = typeof(Resources.SCM.FlowMaster))]
        public string IsActiveDescription { get { return IsActive ? Resources.Global.Export_Yes : Resources.Global.Export_No; } }

        [Export(ExportName = "KBProductionFlow", ExportSeq = 15)]
        [Display(Name = "FlowDetail_IsCut", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string IsCutDescription { get { return IsCut ? Resources.Global.Export_Yes : Resources.Global.Export_No; } }

        [Export(ExportName = "SEQProcurementFlow", ExportSeq = 2, ExportTitle = "FlowMaster_ProductionLine", ExportTitleResourceType = typeof(Resources.SCM.FlowMaster))]
        public string ProdLine { get; set; }

        [Export(ExportName = "SEQProcurementFlow", ExportSeq = 11)]
        [Display(Name = "FlowMaster_LastModifyUserName", ResourceType = typeof(Resources.SCM.FlowMaster))]
        public string LastModifyUser { get; set; }

        [Export(ExportName = "SEQProcurementFlow", ExportSeq = 12)]
        [Display(Name = "FlowMaster_LastModifyDate", ResourceType = typeof(Resources.SCM.FlowMaster))]
        public DateTime LastModifyDate { get; set; }

        
    }
}
