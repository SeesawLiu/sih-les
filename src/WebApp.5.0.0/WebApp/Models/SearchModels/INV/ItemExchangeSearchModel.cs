using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.Sconit.Web.Models.SearchModels.INV
{
    public class ItemExchangeSearchModel : SearchModelBase
    {
        public string ItemFrom { get; set; }
        public string ItemTo { get; set; }
        public string RegionFrom { get; set; }
        public string RegionTo { get; set; }
        public string LocationFrom { get; set; }
        public string LocationTo { get; set; }
    }
}