using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

//TODO: Add other using statements here

namespace com.Sconit.Entity.MD
{
    [Serializable]
    public partial class Supplier : Party
    {
        #region Non O/R Mapping Properties

        [Export(ExportName = "ExportSupplierXLS", ExportSeq = 30)]
        [Display(Name = "Party_Supplier_ShortCode", ResourceType = typeof(Resources.MD.Party))]
        public string ShortCode { get; set; }

        public DateTime? LastRefreshDate { get; set; }
        #endregion
    }
}