using System;
using System.ComponentModel.DataAnnotations;

//TODO: Add other using statements here

namespace com.Sconit.Entity.ORD
{
    public partial class OrderItemTraceResult
    {

        public Decimal Qty { get; set; }

        [Display(Name = "OrderItemTraceResult_NewBarCode", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public string NewBarCode { get; set; }

        [Display(Name = "OrderItemTraceResult_ItemShortCode", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public string ItemShortCode { get; set; }
       
    }

    public class ErrorBarCode
    {
        [Display(Name = "ErrorBarCode_BarCode", ResourceType = typeof(Resources.ORD.ErrorBarCode))]
        public string BarCode { get; set; }
        [Display(Name = "ErrorBarCode_Message", ResourceType = typeof(Resources.ORD.ErrorBarCode))]
        public string Message { get; set; }
        [Display(Name = "ErrorBarCode_ProdCodeSeq", ResourceType = typeof(Resources.ORD.ErrorBarCode))]
        public string ProdCodeSeq { get; set; }
        [Display(Name = "ErrorBarCode_Sequence", ResourceType = typeof(Resources.ORD.ErrorBarCode))]
        public int Sequence { get; set; }

        [Display(Name = "OrderItemTraceResult_IsUpdateItem", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public bool IsUpdateItem { get; set; }

        [Display(Name = "OrderItemTraceResult_IsUpdateBarCode", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public bool IsUpdateBarCode { get; set; }

        public string Delete { get; set; }

        [Display(Name = "OrderItemTraceResult_WithdrawBarcode", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public string WithdrawBarcode { get; set; }


    }
}