using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.SCM
{
    [Serializable]
    public partial class OpReferenceBalance : EntityBase, IAuditable
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }
        [Export(ExportName = "ExporOpReferenceBalancetList", ExportSeq = 10)]
        [Display(Name = "OpReferenceBalance_Item", ResourceType = typeof(Resources.SCM.OpReferenceBalance))]
        public string Item { get; set; }
        [Export(ExportName = "ExporOpReferenceBalancetList", ExportSeq = 40)]
        [Display(Name = "OpReferenceBalance_OpReference", ResourceType = typeof(Resources.SCM.OpReferenceBalance))]
        public string OpReference { get; set; }
        [Export(ExportName = "ExporOpReferenceBalancetList", ExportSeq = 50)]
        [Display(Name = "OpReferenceBalance_Qty", ResourceType = typeof(Resources.SCM.OpReferenceBalance))]
        public Decimal Qty { get; set; }
		public Int32 CreateUserId { get; set; }
        [Export(ExportName = "ExporOpReferenceBalancetList", ExportSeq = 70)]
        [Display(Name = "OpReferenceBalance_CreateUserName", ResourceType = typeof(Resources.SCM.OpReferenceBalance))]
        public string CreateUserName { get; set; }
        [Export(ExportName = "ExporOpReferenceBalancetList", ExportSeq = 60)]
        [Display(Name = "OpReferenceBalance_CreateDate", ResourceType = typeof(Resources.SCM.OpReferenceBalance))]
        public DateTime CreateDate { get; set; }
		public Int32 LastModifyUserId { get; set; }
        [Export(ExportName = "ExporOpReferenceBalancetList", ExportSeq = 90)]
        [Display(Name = "OpReferenceBalance_LastModifyUserName", ResourceType = typeof(Resources.SCM.OpReferenceBalance))]
        public string LastModifyUserName { get; set; }
        [Export(ExportName = "ExporOpReferenceBalancetList", ExportSeq = 80)]
        [Display(Name = "OpReferenceBalance_LastModifyDate", ResourceType = typeof(Resources.SCM.OpReferenceBalance))]
        public DateTime LastModifyDate { get; set; }
		public Int32? Version { get; set; }
        
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
            OpReferenceBalance another = obj as OpReferenceBalance;

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
