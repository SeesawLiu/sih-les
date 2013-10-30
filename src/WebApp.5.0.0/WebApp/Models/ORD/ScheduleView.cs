using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Web.Models.ORD
{
    public class ScheduleView
    {
        public ScheduleHead ScheduleHead { get; set; }

        public IList<ScheduleBody> ScheduleBodyList { get; set; }

        public DateTime? MinDate { get; set; }
    }


    public class ScheduleHead
    {
        //计划协议号
        public string OrderNoHead = "OrderNo";

        //序号
        public string SequenceHead = "Sequence";

        //Les订单号
        public string LesOrderNoHead = "LesOrderNo";

        //寄售-非寄售标识
        public string BillTerm = "BillTerm";

        //路线
        public string FlowHead = "Flow";

        //来源供应商
        public string SupplierHead = "Supplier";

        //零件号
        public string ItemHead = "Item";

        //描述
        public string ItemDescriptionHead = "ItemDescription";

        //旧图号
        public string ReferenceItemCodeHead = "ReferenceItemCode";

        //单位
        public string UomHead = "Uom";

        //包装数
        public string UnitCountHead = "UnitCount";

        //目的库位
        public string LocationToHead = "LocationTo";

        //发货数
        public string CurrentShipQtyHead = "CurrentShipQty";

        //冻结期计划数
        public string OrderQtyInFreeze = "OrderQtyInFreeze";

        //已处理数
        public string HandledQty = "HandledQty";

        //可发货数
        public string ShipQtyHead = "ShipQty";

        //已发货数
        public string ShippedQtyHead = "ShippedQty";

        //已收货数
        public string ReceivedQtyHead = "ReceivedQty";

        //总计划数
        public string OrderQtyHead = "OrderQty";

        //历史总计划数
        public string BackOrderQtyHead = "BackOrderQty";

        //未来汇总
        public string ForecastQtyHead = "ForecastQty";

        public IList<ColumnCell> ColumnCellList { get; set; }
    }


    public class ScheduleBody
    {

        //计划协议号
        [Display(Name = "OrderDetail_ScheduleLineNo", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string OrderNo { get; set; }

        //序号
        [Display(Name = "OrderDetail_Sequence", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string Sequence { get; set; }

        //零件号
        [Display(Name = "OrderDetail_Item", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string Item { get; set; }

        //描述
        [Display(Name = "OrderDetail_ItemDescription", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ItemDescription { get; set; }

        //旧图号
        [Display(Name = "OrderDetail_ReferenceItemCode", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ReferenceItemCode { get; set; }

        //单位
        [Display(Name = "OrderDetail_Uom", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string Uom { get; set; }

        //包装数
        [Display(Name = "OrderDetail_UnitCount", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Decimal UnitCount { get; set; }

        //目的库位
        [Display(Name = "OrderDetail_LocationTo", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string LocationTo { get; set; }

        //本次发货数
        [Display(Name = "OrderDetail_CurrentShipQty", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string CurrentShipQty { get; set; }

        //冻结期内计划数
        public string OrderQtyInFreeze { get; set; }

        //已处理数
        public string HandledQty { get; set; }

        //可发货数=冻结期内计划数-已处理数
        [Display(Name = "OrderDetail_ToShipQty", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ShipQty { get; set; }

        //已发货数
        [Display(Name = "OrderDetail_ShippedQty", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ShippedQty { get; set; }

        //已收货数
        [Display(Name = "OrderDetail_ReceivedQty", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ReceivedQty { get; set; }

        //总计划数
        [Display(Name = "OrderDetail_TotalOrderQty", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string OrderQty { get; set; }

        //历史欠交->历史总计划数
        [Display(Name = "OrderDetail_BackOrderQty", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string BackOrderQty { get; set; }

        //订单明细
        public List<RowCell> RowCellList { get; set; }

        //未来汇总
        [Display(Name = "OrderDetail_ForecastQty", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ForecastQty { get; set; }

        //冻结天数
        [Display(Name = "OrderDetail_FreezeDays", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Int32 FreezeDays { get; set; }

        //Les订单号
        [Display(Name = "OrderDetail_OrderNo", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string LesOrderNo { get; set; }

        //路线代码
        [Display(Name = "OrderDetail_Flow", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string Flow { get; set; }

        //供应商
        [Display(Name = "OrderMaster_PartyFrom", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string Supplier { get; set; }

        //寄售标识
        [Display(Name = "OrderDetail_BillTerm", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string BillTerm { get; set; }

        [Display(Name = "OrderDetail_Tax", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string Tax { get; set; }

        [Display(Name = "OrderDetail_Tax", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string TaxDesc
        {
            get
            {
                switch (this.Tax)
                {
                    case    "ZC1":
                        return "军车";
                    case "ZC2":
                        return "出口车";
                    case "ZC3":
                        return "特殊车";
                    case "ZC4":
                        return "特殊车";
                    default:
                        return this.Tax;
                }
            }
        }

        public decimal DemandQty { get; set; }
        public string DemandDate { get; set; }

    }

    public class RowCell
    {
        public string OrderNo { get; set; }

        public string Sequence { get; set; }

        public CodeMaster.ScheduleType ScheduleType { get; set; }

        public DateTime? EndDate { get; set; }

        public Decimal OrderQty { get; set; }

        public Decimal ShippedQty { get; set; }

        public Decimal ReceivedQty { get; set; }

        public string DisplayQty
        {
            get
            {
                string displayQty = string.Empty;
                if (this.ScheduleType.ToString() == "Firm")
                {
                    displayQty = this.OrderQty.ToString("F2") + "(*)";
                }
                else
                {
                    displayQty = this.OrderQty.ToString("F2");
                }
                return displayQty;
            }
        }

        public string LesOrderNo { get; set; }

        public string Flow { get; set; }

        public string Supplier { get; set; }

        public string Tax { get; set; }

        public string Item { get; set; }
    }

    public class ColumnCell
    {
       // public CodeMaster.ScheduleType ScheduleType { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
