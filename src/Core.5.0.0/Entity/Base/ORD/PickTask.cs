using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.ORD
{
    [Serializable]
    public partial class PickTask : EntityBase, IAuditable
    {
        [Display(Name = "PickTask_PickId", ResourceType = typeof(Resources.ORD.PickTask))]
        public string PickId { get; set; }
        [Display(Name = "PickTask_OrderNo", ResourceType = typeof(Resources.ORD.PickTask))]
        public string OrderNo { get; set; }
        public Int32 OrdDetId { get; set; }
        public CodeMaster.PickDemandType DemandType { get; set; }
        [Display(Name = "PickTask_IsHold", ResourceType = typeof(Resources.ORD.PickTask))]
        public Boolean IsHold { get; set; }
        [Display(Name = "PickTask_Flow", ResourceType = typeof(Resources.ORD.PickTask))]
        public string Flow { get; set; }
        [Display(Name = "PickTask_FlowDesc", ResourceType = typeof(Resources.ORD.PickTask))]
        public string FlowDesc { get; set; }

        [Display(Name = "PickTask_Item", ResourceType = typeof(Resources.ORD.PickTask))]
        public string Item { get; set; }
        [Display(Name = "PickTask_ItemDesc", ResourceType = typeof(Resources.ORD.PickTask))]
        public string ItemDesc { get; set; }
        [Display(Name = "PickTask_Uom", ResourceType = typeof(Resources.ORD.PickTask))]
        public string Uom { get; set; }
        [Display(Name = "PickTask_BaseUom", ResourceType = typeof(Resources.ORD.PickTask))]
        public string BaseUom { get; set; }
        [Display(Name = "PickTask_PartyFrom", ResourceType = typeof(Resources.ORD.PickTask))]
        public string PartyFrom { get; set; }
        [Display(Name = "PickTask_PartyFromName", ResourceType = typeof(Resources.ORD.PickTask))]
        public string PartyFromName { get; set; }
        [Display(Name = "PickTask_PartyTo", ResourceType = typeof(Resources.ORD.PickTask))]
        public string PartyTo { get; set; }
        [Display(Name = "PickTask_PartyToName", ResourceType = typeof(Resources.ORD.PickTask))]
        public string PartyToName { get; set; }
        [Display(Name = "PickTask_LocationFrom", ResourceType = typeof(Resources.ORD.PickTask))]
        public string LocationFrom { get; set; }
        [Display(Name = "PickTask_LocationFromName", ResourceType = typeof(Resources.ORD.PickTask))]
        public string LocationFromName { get; set; }
        [Display(Name = "PickTask_LocationTo", ResourceType = typeof(Resources.ORD.PickTask))]
        public string LocationTo { get; set; }
        [Display(Name = "PickTask_LocationToName", ResourceType = typeof(Resources.ORD.PickTask))]
        public string LocationToName { get; set; }

        [Display(Name = "PickTask_WindowTime", ResourceType = typeof(Resources.ORD.PickTask))]
        public DateTime WindowTime { get; set; }
        [Display(Name = "PickTask_ReleaseDate", ResourceType = typeof(Resources.ORD.PickTask))]
        public DateTime ReleaseDate { get; set; }

        [Display(Name = "PickTask_Supplier", ResourceType = typeof(Resources.ORD.PickTask))]
        public string Supplier { get; set; }
        [Display(Name = "PickTask_SupplierName", ResourceType = typeof(Resources.ORD.PickTask))]
        public string SupplierName { get; set; }

        [Display(Name = "PickTask_UnitCount", ResourceType = typeof(Resources.ORD.PickTask))]
        public Decimal UnitCount { get; set; }
        [Display(Name = "PickTask_OrderedQty", ResourceType = typeof(Resources.ORD.PickTask))]
        public Decimal OrderedQty { get; set; }
        [Display(Name = "PickTask_PickedQty", ResourceType = typeof(Resources.ORD.PickTask))]
        public Decimal PickedQty { get; set; }
        [Display(Name = "PickTask_ShippedQty", ResourceType = typeof(Resources.ORD.PickTask))]
        public Decimal ShippedQty { get; set; }
        [Display(Name = "PickTask_Picker", ResourceType = typeof(Resources.ORD.PickTask))]
        public string Picker { get; set; }
        [Display(Name = "PickTask_PrintCount", ResourceType = typeof(Resources.ORD.PickTask))]
        public Int32 PrintCount { get; set; }
        [Display(Name = "PickTask_Memo", ResourceType = typeof(Resources.ORD.PickTask))]
        public string Memo { get; set; }

        public Int32 CreateUserId { get; set; }
        public string CreateUserName { get; set; }
        public DateTime CreateDate { get; set; }
        public Int32 LastModifyUserId { get; set; }
        public string LastModifyUserName { get; set; }
        public DateTime LastModifyDate { get; set; }

        public override int GetHashCode()
        {
            if (PickId != null)
            {
                return PickId.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            PickTask another = obj as PickTask;

            if (another == null)
            {
                return false;
            }
            else
            {
                return (this.PickId == another.PickId);
            }
        } 
    }
}
