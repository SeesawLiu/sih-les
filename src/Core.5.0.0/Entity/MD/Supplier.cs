using System;
using System.ComponentModel.DataAnnotations;

//TODO: Add other using statements here

namespace com.Sconit.Entity.MD
{
    [Serializable]
    public partial class Supplier : Party
    {
        #region Non O/R Mapping Properties

        [Display(Name = "Party_Supplier_ShortCode", ResourceType = typeof(Resources.MD.Party))]
        public string ShortCode { get; set; }

        public DateTime? LastRefreshDate { get; set; }
        #endregion
    }
}