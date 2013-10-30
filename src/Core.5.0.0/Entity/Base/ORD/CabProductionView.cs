using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.CodeMaster;

namespace com.Sconit.Entity.ORD
{
    [Serializable]
    public partial class CabProductionView : EntityBase
    {
        public Int64 RowId { get; set; }

        public string OrderNo { get; set; }

        [Display(Name = "OrderMaster_TraceCode", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string TraceCode { get; set; }

        [Display(Name = "OrderMaster_Model", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string Model { get; set; }

        [Display(Name = "OrderMaster_ModelDescription", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string ModelDescription { get; set; }

        [Display(Name = "OrderBomDetail_Item", ResourceType = typeof(Resources.ORD.OrderBomDetail))]
        public string Item { get; set; }

        [Display(Name = "OrderBomDetail_ItemDescription", ResourceType = typeof(Resources.ORD.OrderBomDetail))]
        public string ItemDesc { get; set; }

        [Display(Name = "OrderMaster_StartTime_Cab", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public DateTime StartTime { get; set; }

        [Display(Name = "OrderMaster_Flow", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string Flow { get; set; }

        [Display(Name = "OrderMaster_Type", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string Type { get; set; }

        public OrderStatus Status { get; set; }

        public CabOutStatus CabOutStatus { get; set; }

        public string ExtSeq { get; set; }

        public Int32 OrderDetailId { get; set; }

        [Display(Name = "OrderMaster_Sequence", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public Int64 Sequence { get; set; }

        [Display(Name = "OrderMaster_SubSequence", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public Int64 SubSequence { get; set; }
    }
}
