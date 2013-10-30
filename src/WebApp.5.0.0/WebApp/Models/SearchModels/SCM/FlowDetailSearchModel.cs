using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.Sconit.Web.Models.SearchModels.SCM
{
    public class FlowDetailSearchModel : SearchModelBase
    {
        public string Item { get; set; }
        public string Flow { get; set; }
        public string flowCode { get; set; }
        public bool? IsChangeUnitCount { get; set; }
        public string PartyFrom { get; set; }
        public string PartyTo { get; set; }
        public Boolean IsActive { get; set; }
        public string BinTo { get; set; }
        public string ReferenceItemCode { get; set; }
        public int? Strategy { get; set; }
        public int? Type { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}