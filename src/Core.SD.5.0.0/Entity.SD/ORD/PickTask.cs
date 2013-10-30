using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.Sconit.Entity.SD.ORD
{
    [Serializable]
    public partial class PickTask
    {
        public string PickId { get; set; }
        public string OrderNo { get; set; }
        public Int32 OrdDetId { get; set; }
        public CodeMaster.PickDemandType DemandType { get; set; }
        public Boolean IsHold { get; set; }
        public string Flow { get; set; }
        public string FlowDesc { get; set; }

        public string Item { get; set; }
        public string ItemDesc { get; set; }
        public string Uom { get; set; }
        public string BaseUom { get; set; }
        public string PartyFrom { get; set; }
        public string PartyFromName { get; set; }
        public string PartyTo { get; set; }
        public string PartyToName { get; set; }
        public string LocationFrom { get; set; }
        public string LocationFromName { get; set; }
        public string LocationTo { get; set; }
        public string LocationToName { get; set; }

        public DateTime WindowTime { get; set; }
        public DateTime ReleaseDate { get; set; }

        public string Supplier { get; set; }
        public string SupplierName { get; set; }

        public Decimal UnitCount { get; set; }
        public Decimal OrderedQty { get; set; }
        public Decimal PickedQty { get; set; }
        public string Picker { get; set; }
        public Int32 PrintCount { get; set; }
        public string Memo { get; set; }
    }
}
