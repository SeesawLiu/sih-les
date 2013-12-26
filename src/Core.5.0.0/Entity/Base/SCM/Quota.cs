using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.SCM
{
    [Serializable]
    public partial class Quota : EntityBase, IAuditable
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }
        //public string QuotaNo { get; set; }
        //public string QuotaLine { get; set; }

        [Export(ExportName = "ExportSupplierXLS", ExportSeq = 10)]
        [Display(Name = "Quota_Supplier", ResourceType = typeof(Resources.SCM.Quota))]
        public string Supplier { get; set; }
        [Export(ExportName = "ExportSupplierXLS", ExportSeq = 30)]
        [Display(Name = "Quota_SupplierShortCode", ResourceType = typeof(Resources.SCM.Quota))]
        public string SupplierShortCode { get; set; }
        [Export(ExportName = "ExportSupplierXLS", ExportSeq = 20)]
        [Display(Name = "Quota_SupplierName", ResourceType = typeof(Resources.SCM.Quota))]
        public string SupplierName { get; set; }
        [Export(ExportName = "ExportSupplierXLS", ExportSeq = 40)]
        [Display(Name = "Quota_Item", ResourceType = typeof(Resources.SCM.Quota))]
        public string Item { get; set; }
        [Export(ExportName = "ExportSupplierXLS", ExportSeq = 60)]
        [Display(Name = "Quota_RefItemCode", ResourceType = typeof(Resources.SCM.Quota))]
        public string RefItemCode { get; set; }
        [Export(ExportName = "ExportSupplierXLS", ExportSeq = 50)]
        [Display(Name = "Quota_ItemDesc", ResourceType = typeof(Resources.SCM.Quota))]
        public string ItemDesc { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [Export(ExportName = "ExportSupplierXLS", ExportSeq = 70)]
        public Decimal? Weight { get; set; }
        public Decimal? Rate { get; set; }
        [Export(ExportName = "ExportSupplierXLS", ExportSeq = 90)]
        [Display(Name = "Quota_CycleQty", ResourceType = typeof(Resources.SCM.Quota))]
        public Decimal? CycleQty { get; set; }
        [Export(ExportName = "ExportSupplierXLS", ExportSeq = 80)]
        [Display(Name = "Quota_AccumulateQty", ResourceType = typeof(Resources.SCM.Quota))]
        public Decimal? AccumulateQty { get; set; }
        [Export(ExportName = "ExportSupplierXLS", ExportSeq = 100)]
        [Display(Name = "Quota_AdjQty", ResourceType = typeof(Resources.SCM.Quota))]
        public Decimal? AdjQty { get; set; }
        public Boolean? IsActive { get; set; }
        public Int32 CreateUserId { get; set; }
        public string CreateUserName { get; set; }
        public DateTime CreateDate { get; set; }
        public Int32 LastModifyUserId { get; set; }
        [Display(Name = "Quota_LastModifyUserName", ResourceType = typeof(Resources.SCM.Quota))]
        public string LastModifyUserName { get; set; }
        [Display(Name = "Quota_LastModifyDate", ResourceType = typeof(Resources.SCM.Quota))]
        public DateTime LastModifyDate { get; set; }
        public Int32 Version { get; set; }

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
            Quota another = obj as Quota;

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
