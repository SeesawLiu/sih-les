using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.Sconit.Web.Models.SearchModels.INV
{
    public class LocationLotDetailSearchModel : SearchModelBase
    {

        public string itemFrom { get; set; }
        public string itemTo { get; set; }
        public string ManufactureParty { get; set; }
        public Boolean IsFreeze { get; set; }
        public string plantFrom { get; set; }
        public string plantTo { get; set; }
        public string locationFrom { get; set; }
        public string locationTo { get; set; }
        public string regionFrom { get; set; }
        public string regionTo { get; set; }
        public string Level { get; set; }
        public string Location { get; set; }
        public string Item { get; set; }
        public string LotNo { get; set; }
        public string TheFactory { get; set; }
        public string TheFactoryTo { get; set; }
        public bool hideSupper { get; set; }
        public bool hideLotNo { get; set; }
        public string HuId { get; set; }

        public string TypeLocation { get; set; }
        public string SAPLocation { get; set; }

        public string LocationCode { get; set; }
        public string ItemCode { get; set; }
        public string locations { get; set; }
        public string items { get; set; }
        public bool IsSapLocation { get; set; }
        public bool IsShowCSSupplier { get; set; }
        

        

    }
}