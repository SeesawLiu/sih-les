using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.CUST
{
    [Serializable]
    public partial class StockTakeLocationLotDet : EntityBase, IAuditable
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }

        [Export(ExportName = "ExportBackUpInvXLS", ExportSeq =30)]
        [Display(Name = "LocationDetailView_Location", ResourceType = typeof(Resources.View.LocationDetailView))]
        public string Location { get; set; }

        [Export(ExportName = "ExportBackUpInvXLS", ExportSeq = 10)]
        [Display(Name = "LocationDetailView_Item", ResourceType = typeof(Resources.View.LocationDetailView))]
		public string Item { get; set; }


        [Export(ExportName = "ExportBackUpInvXLS", ExportSeq = 20)]
        [Display(Name = "LocationDetailView_Description", ResourceType = typeof(Resources.View.LocationDetailView))]
        public string ItemDesc { get; set; }

        [Export(ExportName = "ExportBackUpInvXLS", ExportSeq = 40)]
        [Display(Name = "LocationDetailView_Qty", ResourceType = typeof(Resources.View.LocationDetailView))]
        public Decimal Qty { get; set; }

        [Display(Name = "LocationDetailView_QualityType", ResourceType = typeof(Resources.View.LocationDetailView))]
        public com.Sconit.CodeMaster.QualityType QualityType { get; set; }

        public Int32 CreateUserId { get; set; }

        [Display(Name = "Common_CreateUserName", ResourceType = typeof(Resources.Global))]
        public string CreateUserName { get; set; }

        [Display(Name = "Common_CreateDate", ResourceType = typeof(Resources.Global))]
        public DateTime CreateDate { get; set; }

        public Int32 LastModifyUserId { get; set; }

        [Display(Name = "Common_LastModifyUserName", ResourceType = typeof(Resources.Global))]
        public string LastModifyUserName { get; set; }

        [Display(Name = "Common_LastModifyDate", ResourceType = typeof(Resources.Global))]
        public DateTime LastModifyDate { get; set; }

        [Export(ExportName = "ExportBackUpInvXLS", ExportSeq = 50)]
        [Display(Name = "LocationDetailView_RefNo", ResourceType = typeof(Resources.View.LocationDetailView))]
        public string RefNo { get; set; }

        [Export(ExportName = "ExportBackUpInvXLS", ExportSeq = 70)]
        [Display(Name = "LocationDetailView_suppliers", ResourceType = typeof(Resources.View.LocationDetailView))]
        public string CSSupplier { get; set; }

        [Export(ExportName = "ExportBackUpInvXLS", ExportSeq = 60)]
        [Display(Name = "LocationDetailView_IsCS", ResourceType = typeof(Resources.View.LocationDetailView))]
        public Boolean IsConsigement { get; set; }

        //[Display(Name = "StockTakeDetail_Uom", ResourceType = typeof(Resources.INV.StockTake))]
        public string Uom { get; set; }
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
            StockTakeLocationLotDet another = obj as StockTakeLocationLotDet;

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
