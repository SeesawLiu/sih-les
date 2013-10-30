using System;
using com.Sconit.Entity.SYS;
using System.ComponentModel.DataAnnotations;

//TODO: Add other using statements here

namespace com.Sconit.Entity.MD
{
    public partial class LocationBinItem
    {
        #region Non O/R Mapping Properties

        //TODO: Add Non O/R Mapping Properties here. 

        #endregion

        [Export(ExportName = "LocationBinItem",ExportSeq=20)]
        [Display(Name = "LocationBinItem_Region", ResourceType = typeof(Resources.MD.LocationBinItem))]
        public string Region { get; set; }
    }
}