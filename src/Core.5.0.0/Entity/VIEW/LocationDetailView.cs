using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

//TODO: Add other using statements here

namespace com.Sconit.Entity.VIEW
{
    public partial class LocationDetailView
    {
        #region Non O/R Mapping Properties
        [Export(ExportName = "ExportLocationDetailXLS", ExportSeq = 20)]
        [Display(Name = "LocationDetailView_Description", ResourceType = typeof(Resources.View.LocationDetailView))]
        public string ItemDescription { get; set; }
        [Export(ExportName = "ExportLocationDetailXLS", ExportSeq = 40)]
        [Display(Name = "LocationDetailView_Uom", ResourceType = typeof(Resources.View.LocationDetailView))]
        public string Uom { get; set; }
        [Display(Name = "LocationDetailView_Name", ResourceType = typeof(Resources.View.LocationDetailView))]
        public string Name { get; set; }

        [Export(ExportName = "ExportLocationDetailXLS", ExportSeq = 30)]
        [Display(Name = "Item_ReferenceCode", ResourceType = typeof(Resources.MD.Item))]
        public string RefrenceItemCode { get; set; }

        [Display(Name = "LocationDetailView_suppliers", ResourceType = typeof(Resources.View.LocationDetailView))]
        public string suppliers { get; set; }
        [Display(Name = "LocationDetailView_LotNo", ResourceType = typeof(Resources.View.LocationDetailView))]
        public string LotNo { get; set; }
        //TODO: Add Non O/R Mapping Properties here. 

        #endregion
    }
}