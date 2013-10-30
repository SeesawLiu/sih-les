using System;

namespace com.Sconit.Web.Models.SearchModels.Report
{
    public class PPMProcurementSearchModel : SearchModelBase
    {
        public string Item { get; set; }
        public string Supplier { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}