using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

//TODO: Add other using statements here

namespace com.Sconit.Entity.INV
{
    public partial class LocationDetailPref
    {
        [Export(ExportName = "ExportXls", ExportSeq = 35)]
        [Display(Name = "FlowDetail_ReferenceItemCode", ResourceType = typeof(Resources.SCM.FlowDetail))]
        public string ReferenceItemCode { get; set; }

        public bool IsUpdate { get; set; }

    }
}