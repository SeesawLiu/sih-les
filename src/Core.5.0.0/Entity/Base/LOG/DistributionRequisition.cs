using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.LOG
{
    [Serializable]
    public partial class DistributionRequisition :  EntityBase, IAuditable
    {
        public Int32 Version { get; set; }
        public Int32 Id { get; set; }
        public Int32 OrderDetId { get; set; }
        
        public string OrderNo { get; set; }

        [Display(Name = "OrderDetail_Item", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string Item { get; set; }

        [Display(Name = "OrderDetail_ItemDescription", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ItemDescription { get; set; }

        [Display(Name = "OrderDetail_ReferenceItemCode", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ReferenceItemCode { get; set; }

        public string BaseUom { get; set; }

        [Display(Name = "OrderDetail_Uom", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string Uom { get; set; }

        [Display(Name = "OrderDetail_UnitCount", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Decimal UnitCount { get; set; }

        public string UnitCountDescription { get; set; }

        public Decimal MinUnitCount { get; set; }

        [Display(Name = "OrderDetail_OrderedQty", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Decimal OrderedQty { get; set; }

        public Decimal ReceivedQty { get; set; }

        public string LocationFrom { get; set; }

        public string LocationFromName { get; set; }

        [Display(Name = "OrderDetail_LocationTo", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string LocationTo { get; set; }

        public string LocationToName { get; set; }

        public Int32 CreateUserId { get; set; }

        [Display(Name = "OrderDetail_CreateUserName", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string CreateUserName { get; set; }

        [Display(Name = "OrderDetail_CreateDate", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public DateTime CreateDate { get; set; }

        public Int32 LastModifyUserId { get; set; }
        public string LastModifyUserName { get; set; }

        public DateTime LastModifyDate { get; set; }

        [Display(Name = "OrderDetail_Container", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string Container { get; set; }
        [Display(Name = "OrderDetail_ContainerDescription", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ContainerDescription { get; set; }

        public string ExternalOrderNo { get; set; }

        public string ExternalSequence { get; set; }

        public string BinTo { get; set; }

        public com.Sconit.CodeMaster.OrderPriority Priority { get; set; }

        [Display(Name = "OrderMaster_PartyFrom", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string PartyFrom { get; set; }

        public string PartyFromName { get; set; }

        [Display(Name = "OrderMaster_PartyTo", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string PartyTo { get; set; }

        public string PartyToName { get; set; }

        [Display(Name = "OrderMaster_Flow", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string Flow { get; set; }

        [Display(Name = "OrderMaster_FlowDescription", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string FlowDescription { get; set; }

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
            DistributionRequisition another = obj as DistributionRequisition;

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
