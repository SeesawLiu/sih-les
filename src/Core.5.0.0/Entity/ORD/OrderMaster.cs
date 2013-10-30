using System;
using System.Collections.Generic;
using com.Sconit.Entity.SYS;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.BIL;
using System.Runtime.Serialization;

//TODO: Add other using statements here

namespace com.Sconit.Entity.ORD
{
    public partial class OrderMaster
    {
        #region Non O/R Mapping Properties

        [Export(ExportName = "ExportSeqOrder", ExportSeq = 70)]
        [Export(ExportName = "VanProductionOrder", ExportSeq = 180)]
        [Export(ExportName = "ProductionOrder", ExportSeq = 180)]
        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.OrderType, ValueField = "Type")]
        [Display(Name = "OrderMaster_Type", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string OrderTypeDescription { get; set; }

        [Export(ExportName = "ExportSeqOrder", ExportSeq = 80)]
        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.OrderPriority, ValueField = "Priority")]
        [Display(Name = "OrderMaster_Priority", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string OrderPriorityDescription { get; set; }

        [Export(ExportName = "ExportSeqOrder", ExportSeq = 130)]
        [Export(ExportName = "VanProductionOrder", ExportSeq = 100)]
        [Export(ExportName = "ProductionOrder", ExportSeq = 130)]
        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.OrderStatus, ValueField = "Status")]
        [Display(Name = "OrderMaster_Status", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string OrderStatusDescription { get; set; }

        [Export(ExportName = "ExportVanOrder", ExportSeq = 160)]
        [Export(ExportName = "VanProductionOrder", ExportSeq = 170)]
        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.PauseStatus, ValueField = "PauseStatus")]
        [Display(Name = "OrderMaster_PauseStatus", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string PauseStatusDescription { get; set; }

        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.FlowStrategy, ValueField = "OrderStrategy")]
        [Display(Name = "OrderMaster_OrderStrategy", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string OrderStrategyDescription { get; set; }

        [Export(ExportName = "ExportSeqOrder", ExportSeq = 150)]
        [Display(Name = "OrderMaster_OrderStrategy", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string SeqOrderStrategyDescription { get; set; }

        public bool IsCheck { get; set; }

        //用来做checkbox的头
        public string CheckOrderNo { get; set; }

        [DataMember]
        public IList<OrderDetail> OrderDetails { get; set; }
        [DataMember]
        public IList<OrderBomDetail> OrderBomDetails { get; set; }
        //[DataMember]
        public IList<OrderBinding> OrderBindings { get; set; }

        [Display(Name = "OrderMaster_Checker", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string Checker { get; set; }

        [Export(ExportName = "ExportVanOrder", ExportSeq = 40, ExportTitle = "OrderDetail_Item", ExportTitleResourceType = typeof(Resources.ORD.OrderDetail))]
        [Export(ExportName = "ProductionOrder", ExportSeq = 40, ExportTitle = "OrderDetail_Item", ExportTitleResourceType = typeof(Resources.ORD.OrderDetail))]
        [Display(Name = "OrderDetail_Item", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string Item { get; set; }

        [Display(Name = "OrderDetail_ItemDescription", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ItemDescription { get; set; }

        [Display(Name = "OrderDetail_Uom", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string Uom { get; set; }

        [Export(ExportName = "ProductionOrder", ExportSeq = 50)]
        [Display(Name = "OrderDetail_OrderedQty", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Decimal OrderedQty { get; set; }

        [Display(Name = "OrderDetail_ReceivedQty", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Decimal ReceivedQty { get; set; }

        [Display(Name = "OrderDetail_ScrapQty", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Decimal ScrapQty { get; set; }

        public string ProdLine { get; set; }

        //[Export(ExportName = "ProductionOrder", ExportSeq = 45)]
        [Display(Name = "Item_ReferenceCode", ResourceType = typeof(Resources.MD.Item))]
        public string ReferenceItemCode { get; set; }


        //供应商查看状态
        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.SupplierOrderStatus, ValueField = "Status")]
        [Display(Name = "OrderMaster_Status", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string SupplierOrderStatusDescription { get; set; }

        #endregion

        #region 辅助字段
        public PriceListMaster CurrentPriceListMaster { get; set; }
        public string DummyLocation { get; set; }  //虚拟库位，从ProductMap表获取，往下传递给分装生产单（KIT单）
        #endregion

        #region methods
        public void AddOrderDetail(OrderDetail orderDetail)
        {
            if (OrderDetails == null)
            {
                OrderDetails = new List<OrderDetail>();
            }
            OrderDetails.Add(orderDetail);
        }

        public void AddOrderBinding(OrderBinding orderBinding)
        {
            if (OrderBindings == null)
            {
                OrderBindings = new List<OrderBinding>();
            }
            OrderBindings.Add(orderBinding);
        }
        #endregion
    }
}