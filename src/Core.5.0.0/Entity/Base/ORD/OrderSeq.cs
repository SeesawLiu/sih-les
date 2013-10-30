using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.ORD
{
    [Serializable]
    public partial class OrderSeq : EntityBase, IAuditable
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }

        [Display(Name = "OrderSeq_OrderNo", ResourceType = typeof(Resources.ORD.OrderSeq))]
        public string OrderNo { get; set; }

        [Display(Name = "OrderSeq_ProdLine", ResourceType = typeof(Resources.ORD.OrderSeq))]
        public string ProductLine { get; set; }

        [Display(Name = "OrderSeq_TraceCode", ResourceType = typeof(Resources.ORD.OrderSeq))]
        public string TraceCode { get; set; }

        [Display(Name = "OrderSeq_Seq", ResourceType = typeof(Resources.ORD.OrderSeq))]
        public Int64 Sequence { get; set; }

        [Display(Name = "OrderSeq_SapSeq", ResourceType = typeof(Resources.ORD.OrderSeq))]
        public Int64 SapSequence { get; set; }

        [Display(Name = "OrderSeq_SubSeq", ResourceType = typeof(Resources.ORD.OrderSeq))]
        public Int32 SubSequence { get; set; }

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
            OrderSeq another = obj as OrderSeq;

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
