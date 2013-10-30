using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.Sconit.Web.Models.SearchModels.PRD
{
    public class MultiSupplyGroupSearchModel : SearchModelBase
    {
        public string GroupNo { get; set; }
        public string Supplier { get; set; }
        public string Item { get; set; }
        public string SubstituteGroup { get; set; }
    }
}