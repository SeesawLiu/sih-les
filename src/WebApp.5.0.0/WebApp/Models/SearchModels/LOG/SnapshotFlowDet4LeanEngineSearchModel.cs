using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.Sconit.Web.Models.SearchModels.LOG
{
    public class SnapshotFlowDet4LeanEngineSearchModel : SearchModelBase
    {
        public string Item { get; set; }
        public string Flow { get; set; }
        public string OrderNo { get; set; }
        public DateTime? CreateDateFrom { get; set; }
        public DateTime? CreateDateTo { get; set; }
        public string LocationFrom { get; set; }
        public string LocationTo { get; set; }
        public int? ErrorId { get; set; }
    }
}