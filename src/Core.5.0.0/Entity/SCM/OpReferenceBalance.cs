using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

//TODO: Add other using statements here

namespace com.Sconit.Entity.SCM
{
    public partial class OpReferenceBalance
    {
        [Export(ExportName = "ExporOpReferenceBalancetList", ExportSeq = 30)]
        [Display(Name = "OpReferenceBalance_ItemDescription", ResourceType = typeof(Resources.SCM.OpReferenceBalance))]
        public string ItemDescription { get; set; }
        [Export(ExportName = "ExporOpReferenceBalancetList", ExportSeq = 20)]
        [Display(Name = "OpReferenceBalance_ReferenceItemCode", ResourceType = typeof(Resources.SCM.OpReferenceBalance))]
        public string ReferenceItemCode { get; set; }

        public decimal CurrentAdjustQty { get; set; }
        
       
    }
}