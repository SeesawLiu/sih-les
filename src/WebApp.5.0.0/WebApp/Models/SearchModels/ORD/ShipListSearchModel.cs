using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.Sconit.Web.Models.SearchModels.ORD
{
    public class ShipListSearchModel : SearchModelBase
    {
        public string ShipNo { get; set; }
        public string Vehicle { get; set; }
        public string Shipper { get; set; }
        public Int16? Status { get; set; }
    }
}