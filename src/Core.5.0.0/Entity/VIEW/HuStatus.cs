using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

//TODO: Add other using statements here

namespace com.Sconit.Entity.VIEW
{
    public partial class HuStatus
    {
        #region Non O/R Mapping Properties

        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.HuStatus, ValueField = "Status")]
        [Display(Name = "Hu_HuStatusDescription", ResourceType = typeof(Resources.INV.Hu))]
        public string HuStatusDescription { get; set; }

        #endregion
    }
}