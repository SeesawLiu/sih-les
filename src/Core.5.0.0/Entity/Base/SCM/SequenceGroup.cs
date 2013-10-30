using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.SCM
{
    [Serializable]
    public partial class SequenceGroup : EntityBase, IAuditable
    {
        #region O/R Mapping Properties

        [Export(ExportName = "SequenceGroup", ExportSeq = 10)]
        [Display(Name = "SequenceGroup_Code", ResourceType = typeof(Resources.SCM.SequenceGroup))]
        public string Code { get; set; }
        [Export(ExportName = "SequenceGroup", ExportSeq = 20)]
        [Display(Name = "SequenceGroup_ProdLine", ResourceType = typeof(Resources.SCM.SequenceGroup))]
        public string ProductLine { get; set; }
        [Export(ExportName = "SequenceGroup", ExportSeq = 30)]
        [Display(Name = "SequenceGroup_SequenceBatch", ResourceType = typeof(Resources.SCM.SequenceGroup))]
        public Int32 SequenceBatch { get; set; }

        [Export(ExportName = "SequenceGroup", ExportSeq = 50)]
        [Display(Name = "SequenceGroup_PreviousOrderNo", ResourceType = typeof(Resources.SCM.SequenceGroup))]
        public string PreviousOrderNo { get; set; }
        [Export(ExportName = "SequenceGroup", ExportSeq = 60)]
        [Display(Name = "SequenceGroup_PreviousTraceCode", ResourceType = typeof(Resources.SCM.SequenceGroup))]
        public string PreviousTraceCode { get; set; }
        [Export(ExportName = "SequenceGroup", ExportSeq = 70)]
        [Display(Name = "SequenceGroup_PreviousSeq", ResourceType = typeof(Resources.SCM.SequenceGroup))]
        public Int64? PreviousSeq { get; set; }
        [Export(ExportName = "SequenceGroup", ExportSeq = 80)]
        [Display(Name = "SequenceGroup_PreviousSubSeq", ResourceType = typeof(Resources.SCM.SequenceGroup))]
        public Int32? PreviousSubSeq { get; set; }
        public DateTime? PreviousDeliveryDate { get; set; }
        public Int32? PreviousDeliveryCount { get; set; }
        public Int32 CreateUserId { get; set; }
        public string CreateUserName { get; set; }
        public DateTime CreateDate { get; set; }
        public Int32 LastModifyUserId { get; set; }
        public string LastModifyUserName { get; set; }
        public DateTime LastModifyDate { get; set; }
        public Int32 Version { get; set; }
        [Export(ExportName = "SequenceGroup", ExportSeq = 40)]
        [Display(Name = "SequenceGroup_OpReference", ResourceType = typeof(Resources.SCM.SequenceGroup))]
        public string OpReference { get; set; }
        [Export(ExportName = "SequenceGroup", ExportSeq = 100)]
        [Display(Name = "SequenceGroup_IsActive", ResourceType = typeof(Resources.SCM.SequenceGroup))]
        public Boolean IsActive { get; set; }
        #endregion

        public override int GetHashCode()
        {
            if (Code != null)
            {
                return Code.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            SequenceGroup another = obj as SequenceGroup;

            if (another == null)
            {
                return false;
            }
            else
            {
                return (this.Code == another.Code);
            }
        }
    }

}
