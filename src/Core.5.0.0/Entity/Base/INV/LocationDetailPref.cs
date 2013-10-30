using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.INV
{
    [Serializable]
    public partial class LocationDetailPref : EntityBase,IAuditable
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }
        [Export(ExportName = "ExportXls", ExportSeq = 20)]
        [Display(Name = "LocationLotDetail_Item", ResourceType = typeof(Resources.INV.LocationDetailPref))]
        public string Item { get; set; }
        [Export(ExportName = "ExportXls", ExportSeq = 30)]
        [Display(Name = "LocationLotDetail_ItemDesc", ResourceType = typeof(Resources.INV.LocationDetailPref))]
        public string ItemDesc { get; set; }
        [Export(ExportName="ExportXls",ExportSeq=10)]
        [Display(Name = "LocationLotDetail_Location", ResourceType = typeof(Resources.INV.LocationDetailPref))]
        public string Location { get; set; }
        [Export(ExportName = "ExportXls", ExportSeq = 40)]
        [Display(Name = "LocationLotDetail_SafeStock", ResourceType = typeof(Resources.INV.LocationDetailPref))]
        public decimal SafeStock { get; set; }
        [Export(ExportName = "ExportXls", ExportSeq = 50)]
        [Display(Name = "LocationLotDetail_MaxStock", ResourceType = typeof(Resources.INV.LocationDetailPref))]
        public decimal MaxStock { get; set; }
		public Int32 CreateUserId { get; set; }
		public string CreateUserName { get; set; }
		public DateTime CreateDate { get; set; }
		public Int32 LastModifyUserId { get; set; }
		public string LastModifyUserName { get; set; }
		public DateTime LastModifyDate { get; set; }
		public Int32 Version { get; set; }
        
        #endregion

		public override int GetHashCode()
        {
            if (Id != null)
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
            LocationDetailPref another = obj as LocationDetailPref;

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
