using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.LOG
{
    [Serializable]
    public partial class ProdOrderPauseResume : EntityBase
    {
        public Int32 Id { get; set; }

        [Display(Name = "ProdOrderPauseResume_ProdLine", ResourceType = typeof(Resources.LOG.ProdOrderPauseResume))]
        public string ProdLine { get; set; }

        [Display(Name = "ProdOrderPauseResume_ProdLineDesc", ResourceType = typeof(Resources.LOG.ProdOrderPauseResume))]
        public string ProdLineDesc { get; set; }

        [Display(Name = "ProdOrderPauseResume_VanCode", ResourceType = typeof(Resources.LOG.ProdOrderPauseResume))]
        public string VanCode { get; set; }

        [Display(Name = "ProdOrderPauseResume_OrderNo", ResourceType = typeof(Resources.LOG.ProdOrderPauseResume))]
        public string OrderNo { get; set; }

        [Display(Name = "ProdOrderPauseResume_BeforeOrderNo", ResourceType = typeof(Resources.LOG.ProdOrderPauseResume))]
        public string BeforeOrderNo { get; set; }

        [Display(Name = "ProdOrderPauseResume_CurrentOperation", ResourceType = typeof(Resources.LOG.ProdOrderPauseResume))]
        public string CurrentOperation { get; set; }

        [Display(Name = "ProdOrderPauseResume_OprateType", ResourceType = typeof(Resources.LOG.ProdOrderPauseResume))]
        public com.Sconit.CodeMaster.OrderPauseType OprateType { get; set; }

        [Display(Name = "ProdOrderPauseResume_CreateDate", ResourceType = typeof(Resources.LOG.ProdOrderPauseResume))]
        public DateTime CreateDate { get; set; }

        [Display(Name = "ProdOrderPauseResume_CreateUserName", ResourceType = typeof(Resources.LOG.ProdOrderPauseResume))]
        public string CreateUserName { get; set; }

        [Display(Name = "ProdOrderPauseResume_BeforeVanCode", ResourceType = typeof(Resources.LOG.ProdOrderPauseResume))]
        public string BeforeVanCode { get; set; }
        [Display(Name = "ProdOrderPauseResume_Seq", ResourceType = typeof(Resources.LOG.ProdOrderPauseResume))]
        public Int64 Seq { get; set; }
        [Display(Name = "ProdOrderPauseResume_SubSeq", ResourceType = typeof(Resources.LOG.ProdOrderPauseResume))]
        public Int32 SubSeq { get; set; }

        public Int64 BeforeSeq { get; set; }
        public Int32 BeforeSubSeq { get; set; }


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
            ProdOrderPauseResume another = obj as ProdOrderPauseResume;

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
