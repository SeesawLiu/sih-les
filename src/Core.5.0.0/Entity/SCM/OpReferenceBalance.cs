using System;
using System.ComponentModel.DataAnnotations;

//TODO: Add other using statements here

namespace com.Sconit.Entity.SCM
{
    public partial class OpReferenceBalance
    {
        [Display(Name = "OpReferenceBalance_ItemDescription", ResourceType = typeof(Resources.SCM.OpReferenceBalance))]
        public string ItemDescription { get; set; }
        [Display(Name = "OpReferenceBalance_ReferenceItemCode", ResourceType = typeof(Resources.SCM.OpReferenceBalance))]
        public string ReferenceItemCode { get; set; }
       
    }
}