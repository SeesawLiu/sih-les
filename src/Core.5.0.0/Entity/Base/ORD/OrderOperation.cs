using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.ORD
{
    [Serializable]
    public partial class OrderOperation : EntityBase, IAuditable
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }
        public string OrderNo { get; set; }
        public Int32 OrderDetailId { get; set; }
        [Display(Name = "OrderOperation_Operation", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public Int32 Operation { get; set; }
        [Display(Name = "OrderOperation_OpReference", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public string OpReference { get; set; }
        [Display(Name = "OrderOperation_WorkCenter", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public string WorkCenter { get; set; }
        [Display(Name = "OrderOperation_LeadTime", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public Double LeadTime { get; set; }
        [Display(Name = "OrderOperation_TimeUnit", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public com.Sconit.CodeMaster.TimeUnit TimeUnit { get; set; }
        public string Location { get; set; }
        public Boolean IsAutoReport { get; set; }
        public Int32 CreateUserId { get; set; }
        public string CreateUserName { get; set; }
        public DateTime CreateDate { get; set; }
        public Int32 LastModifyUserId { get; set; }
        public string LastModifyUserName { get; set; }
        public DateTime LastModifyDate { get; set; }
        public Int32 Version { get; set; }

        [Display(Name = "OrderOperation_ReportQty", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public Decimal ReportQty { get; set; }
        [Display(Name = "OrderOperation_BackflushQty", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public Decimal BackflushQty { get; set; }

        [Display(Name = "OrderOperation_AUFPL", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public string AUFPL { get; set; }

        [Display(Name = "OrderOperation_PLNFL", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public string PLNFL { get; set; }

        [Display(Name = "OrderOperation_VORNR", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public string VORNR { get; set; }

        public Boolean NeedReport { get; set; }

        [Display(Name = "OrderOperation_IsReceiveFinishGoods", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public Boolean IsReceiveFinishGoods { get; set; }

        [Display(Name = "OrderOperation_ScrapQty", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public Decimal ScrapQty { get; set; }

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
            OrderOperation another = obj as OrderOperation;

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
