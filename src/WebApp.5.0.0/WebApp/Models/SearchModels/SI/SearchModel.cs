using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.Sconit.Web.Models.SearchModels.SI
{
    public class SearchModel : SearchModelBase
    {
        public int? Id { get; set; }
        public string Code { get; set; }
        public int? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string AUFNR { get; set; }
        public string EINDT { get; set; }
        public string FRBNR { get; set; }
        public string LIFNR { get; set; }
        public string MATNR { get; set; }

        public string OrderNo { get; set; }
        public string ReceiptNo { get; set; }
    }
}