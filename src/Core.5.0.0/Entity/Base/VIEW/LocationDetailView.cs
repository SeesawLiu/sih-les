using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.VIEW
{
    [Serializable]
    public partial class LocationDetailView : EntityBase
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }
        [Export(ExportName = "ExportLocationDetailXLS", ExportSeq = 50)]
        [Display(Name = "LocationDetailView_Location", ResourceType = typeof(Resources.View.LocationDetailView))]
        public string Location { get; set; }
        [Export(ExportName = "ExportLocationDetailXLS", ExportSeq = 10)]
        [Display(Name = "LocationDetailView_Item", ResourceType = typeof(Resources.View.LocationDetailView))]
        public string Item { get; set; }
        [Display(Name = "LocationDetailView_ManufactureParty", ResourceType = typeof(Resources.View.LocationDetailView))]
        public string ManufactureParty { get; set; }
        [Export(ExportName = "ExportLocationDetailXLS", ExportSeq = 60)]
        [Display(Name = "LocationDetailView_Qty", ResourceType = typeof(Resources.View.LocationDetailView))]
        public Decimal Qty { get; set; }
        [Export(ExportName = "ExportLocationDetailXLS", ExportSeq = 70)]
        [Display(Name = "LocationDetailView_ConsignmentQty", ResourceType = typeof(Resources.View.LocationDetailView))]
        public Decimal ConsignmentQty { get; set; }
        [Export(ExportName = "ExportLocationDetailXLS", ExportSeq = 80)]
        [Display(Name = "LocationDetailView_QualifyQty", ResourceType = typeof(Resources.View.LocationDetailView))]
        public Decimal QualifyQty { get; set; }
        [Export(ExportName = "ExportLocationDetailXLS", ExportSeq = 90)]
        [Display(Name = "LocationDetailView_InspectQty", ResourceType = typeof(Resources.View.LocationDetailView))]
        public Decimal InspectQty { get; set; }
        [Export(ExportName = "ExportLocationDetailXLS", ExportSeq = 100)]
        [Display(Name = "LocationDetailView_RejectQty", ResourceType = typeof(Resources.View.LocationDetailView))]
        public Decimal RejectQty { get; set; }
        [Display(Name = "LocationDetailView_ATPQty", ResourceType = typeof(Resources.View.LocationDetailView))]
        public Decimal ATPQty { get; set; }
        [Display(Name = "LocationDetailView_FreezeQty", ResourceType = typeof(Resources.View.LocationDetailView))]
        public Decimal FreezeQty { get; set; }
        [Display(Name = "LocationDetailView_IsCS", ResourceType = typeof(Resources.View.LocationDetailView))]
        public bool IsCS { get; set; }


        public Decimal CsQty { get; set; }

        #endregion

        public override int GetHashCode()
        {
            if (Id != 0)
            {
                return Id.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            LocationDetailView another = obj as LocationDetailView;

            if (another == null)
            {
                return false;
            }
            else
            {
                return (this.Id == another.Id);
            }
        }
    }

}
