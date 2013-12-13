using System;
namespace com.Sconit.Web.Models.SearchModels.INV
{
    public class OrderItemTraceResultSearchModel : SearchModelBase
    {
        public string TraceCode { get; set; }
        public string OrderNo { get; set; }
        public string OpReference { get; set; }
        public string Item { get; set; }
        public string BarCode { get; set; }
        public string OpRefArea { get; set; }
        public DateTime? TraceDateFrom { get; set; }
        public DateTime? TraceDateTo { get; set; }
        public string TraceCodeSearch { get; set; }
        public string EngineCodeSearch { get; set; }

    }
}