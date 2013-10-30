using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.Sconit.Web.Models.SearchModels.KB
{
    public class KanbanOrderSearchModel : SearchModelBase
    {
        public string Region { get; set; }
        public string Supplier { get; set; }
        public string LcCode { get; set; }
        public string Item { get; set; }
        public DateTime? OrderTime { get; set; }
        public string ChosenScans { get; set; }
        public string Flow { get; set; }
    }
}