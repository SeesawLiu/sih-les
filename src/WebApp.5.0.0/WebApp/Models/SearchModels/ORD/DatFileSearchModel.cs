using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.Sconit.Web.Models.SearchModels.ORD
{
    public class DatFileSearchModel : SearchModelBase
    {
        public string WmsPickNo { get; set; }
        public string OrderNo { get; set; }
        public string IpNo { get; set; }
        public string Supplier { get; set; }
        public string Location { get; set; }
        public string Item { get; set; }
        public string WmsNo { get; set; }
        public string HandResult { get; set; }
        public string MoveType { get; set; }
        public Int16? IsCs { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string RecNo { get; set; }
        public string AsnNo { get; set; }
        public string Type { get; set; }
        public string ExtNo { get; set; }
        public string PoLine { get; set; }
        public Boolean? IsCreateDat { get; set; }
        public Boolean? IsHand { get; set; }
        public string WMSId { get; set; }
        public string LGORT { get; set; }
        public string UMLGO { get; set; }
        public bool IsClsoe { get; set; }
        public bool IsNoneOut { get; set; }
        public string PartyTo { get; set; }
        public string PartyFrom { get; set; }
        public Int16? OrderStrategy { get; set; }
    }
}