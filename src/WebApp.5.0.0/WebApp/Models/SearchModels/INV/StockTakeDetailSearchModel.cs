using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.Sconit.Web.Models.SearchModels.INV
{
    public class StockTakeDetailSearchModel : SearchModelBase
    {
        public string ItemCode { get; set; }
        public string Location { get; set; }
        public string LocationBin { get; set; }
        public string stNo { get; set; }
    }
}