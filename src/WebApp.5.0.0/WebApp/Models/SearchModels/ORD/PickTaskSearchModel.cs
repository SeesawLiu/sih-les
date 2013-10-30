using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.Sconit.Web.Models.SearchModels.ORD
{
    public class PickTaskSearchModel : SearchModelBase
    {
        public string PickId { get; set; }
        public string LocationFrom { get; set; }
        public string LocationTo { get; set; }
        public string OrderNo { get; set; }
        public string Picker { get; set; }
        public string Item { get; set; }
        public string Flow { get; set; }
        public DateTime? ReleaseStart { get; set; }
        public DateTime? ReleaseEnd { get; set; }
        public DateTime? WindowStart { get; set; }
        public DateTime? WindowEnd { get; set; }
        public Boolean IncludeFinished { get; set; }
        public Boolean ShowHold { get; set; }
    }
}