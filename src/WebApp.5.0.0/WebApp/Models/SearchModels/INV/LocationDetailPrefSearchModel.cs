using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.Sconit.Web.Models.SearchModels.INV
{
    public class LocationDetailPrefSearchModel : SearchModelBase
    {
        public string LocationCode { get; set; }
        public string ItemCode { get; set; }
    }
}