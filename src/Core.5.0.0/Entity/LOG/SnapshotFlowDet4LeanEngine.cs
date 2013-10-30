using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.LOG
{
    public partial class SnapshotFlowDet4LeanEngine
    {

        

        [Export(ExportSeq = 30, ExportName = "ExportInfo")]
        [Display(Name = "VanOrderTrace_RefItemCode", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
        public string ReferenceItemCode { get; set; }


        [Export(ExportSeq = 40, ExportName = "ExportInfo")]
        [Display(Name = "VanOrderTrace_ItemDesc", ResourceType = typeof(Resources.LOG.VanOrderTrace))]
        public string ItemDesc { get; set; }
       
    }
}
