using System;

namespace com.Sconit.Web.Models.SearchModels.ORD
{
    public class AliquotStartTaskSearchModel : SearchModelBase
    {
        public string SearchFlow { get; set; }
        public string SearchVanFlow { get; set; }
        public Boolean SearchIsStart { get; set; }
        public DateTime? StartTimeFrom { get; set; }
        public DateTime? StartTimeTo { get; set; }
    }
}