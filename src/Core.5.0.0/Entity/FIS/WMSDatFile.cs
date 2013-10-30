using System;
using com.Sconit.Entity.SYS;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.FIS
{
    public partial class WMSDatFile
    {
        [Export(ExportName = "ExportWMSDatFile", ExportSeq = 20)]
        [Display(Name = "WMSDatFile_ReferenceItemCode", ResourceType = typeof(Resources.FIS.WMSDatFile))]
        public string ReferenceItemCode { get; set; }

        [Export(ExportName = "ExportWMSDatFile", ExportSeq = 30)]
        [Display(Name = "WMSDatFile_ItemDescription", ResourceType = typeof(Resources.FIS.WMSDatFile))]
        public string ItemDescription { get; set; }

        [Export(ExportName = "ExportWMSDatFile", ExportSeq = 50)]
        [Display(Name = "WMSDatFile_OrderQty", ResourceType = typeof(Resources.FIS.WMSDatFile))]
        public decimal OrderQty { get; set; }

        [Display(Name = "WMSDatFile_ReceivedQty", ResourceType = typeof(Resources.FIS.WMSDatFile))]
        public decimal ReceivedQty { get; set; }

        [Display(Name = "WMSDatFile_CurrentReceiveQty", ResourceType = typeof(Resources.FIS.WMSDatFile))]
        public decimal CurrentReceiveQty { get; set; }

        [Display(Name = "LesINLog_HandResult", ResourceType = typeof(Resources.FIS.LesInLog))]
        public string HandResult { get; set; }

        [Display(Name = "LesINLog_ErrorCause", ResourceType = typeof(Resources.FIS.LesInLog))]
        public string ErrorCause { get; set; }

        [Display(Name = "WMSDatFile_RecNo", ResourceType = typeof(Resources.FIS.WMSDatFile))]
        public string RecNo { get; set; }

        [Export(ExportName = "ExportWMSDatFile", ExportSeq = 1)]
        [Display(Name = "OrderDetail_OrderNo", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string OrderNo { get; set; }

        [Display(Name = "OrderDetail_LocationTo", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string LocationTo { get; set; }

        [Export(ExportName = "ExportWMSDatFile", ExportSeq = 110)]
        [Display(Name = "WMSDatFile_CreateDate", ResourceType = typeof(Resources.FIS.WMSDatFile))]
        public DateTime? CreateDateFormat { get; set; }

        [Export(ExportName = "ExportWMSDatFile", ExportSeq = 100)]
        [Display(Name = "WMSDatFile_RequirementDate", ResourceType = typeof(Resources.FIS.WMSDatFile))]
        public DateTime? RequirementDate { get; set; }

        [Export(ExportName = "ExportWMSDatFile", ExportSeq = 5)]
        [Display(Name = "OrderMaster_PartyTo", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string PartyTo { get; set; }

        [Export(ExportName = "ExportWMSDatFile", ExportSeq = 3)]
        [Display(Name = "OrderMaster_PartyFrom", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string PartyFrom { get; set; }

        [Export(ExportName = "ExportWMSDatFile", ExportSeq = 90)]
        [Display(Name = "OrderMaster_WindowTime", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public DateTime WindowTime { get; set; }

       

        [Export(ExportName = "ExportWMSDatFile", ExportSeq = 2)]
        [Display(Name = "OrderMaster_OrderStrategy", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string OrderStrategyDescription { get; set; }



    }
	
}
