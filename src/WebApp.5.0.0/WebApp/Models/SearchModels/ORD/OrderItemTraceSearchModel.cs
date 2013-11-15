namespace com.Sconit.Web.Models.SearchModels.ORD
{
    public class OrderItemTraceSearchModel : SearchModelBase
    {
        public string OrderNo { get; set; }
        public string OpReference { get; set; }
        public string BarCode { get; set; }
        public string Item { get; set; }
        public string TraceCode { get; set; }
        public string Suppliers { get; set; }
        public string LotNos { get; set; }
    }
}