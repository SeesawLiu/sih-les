using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;
//TODO: Add other using statements here

namespace com.Sconit.Entity.BIL
{
    public partial class PriceListMaster
    {
        #region Non O/R Mapping Properties

        
        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.PriceListType, ValueField = "Type")]
        [Display(Name = "PriceListMaster_Type", ResourceType = typeof(Resources.BIL.PriceListMaster))]
        public string PriceListTypeDescription { get; set; }
        #endregion
    }
}