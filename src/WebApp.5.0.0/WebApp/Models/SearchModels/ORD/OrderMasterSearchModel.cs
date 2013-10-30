using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.Sconit.Web.Models.SearchModels.ORD
{
    public class OrderMasterSearchModel : SearchModelBase
    {
        public string OrderNo { get; set; }
        public string Flow { get; set; }
        public Int16? Type { get; set; }
        public Int16? SubType { get; set; }
        public string PartyFrom { get; set; }
        public string PartyTo { get; set; }
        public Int16? Status { get; set; }
        public Int16? Priority { get; set; }
        public string ReferenceOrderNo { get; set; }
        public string ExternalOrderNo { get; set; }
        public string TraceCode { get; set; }
        public string CreateUserName { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public Int64? Sequence { get; set; }
        public string WMSNO { get; set; }

        public string Item { get; set; }
        public string ItemDescription { get; set; }
        public string Shift { get; set; }
        public Int16? IsPause { get; set; }
        public string Dock { get; set; }
        public string LocationTo { get; set; }
        public string LocationFrom { get; set; }
        public string LocationToTo { get; set; }
        public string LocationFromTo { get; set; }
        public com.Sconit.CodeMaster.ScheduleType? ScheduleType { get; set; }
        public string WmSSeq { get; set; }
        public string ManufactureParty { get; set; }
        public string Checker { get; set; }
        public int? ListDays { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? WindowTimeFrom { get; set; }
        public DateTime? WindowTimeTo { get; set; }
        public bool NotIncludeZeroShipQty { get; set; }

        public string DetailItem { get; set; }
        public string successMesage { get; set; }
        public string errorMesage { get; set; }

        public string ProdLine { get; set; }
        public Int16? OrderStrategy { get; set; }
        public string SequenceGroup { get; set; }
        public string Picker { get; set; }

        public bool IsListPrice { get; set; }
        public bool IsPrintOrder { get; set; }

        public string ZOPWZ { get; set; }

        public bool IsClsoe { get; set; }
        public bool IsNoneClsoe { get; set; }

        public string WorkCenter { get; set; }
    }
}