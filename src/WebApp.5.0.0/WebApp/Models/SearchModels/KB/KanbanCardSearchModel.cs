using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace com.Sconit.Web.Models.SearchModels.KB
{
    public class KanbanCardSearchModel : SearchModelBase
    {
        public string Region { get; set; }
        public string Supplier { get; set; }
        public string Item { get; set; }
        public string CardNo { get; set; }
        public string ChildItemCount { get; set; }
        public Boolean? NeedReprint { get; set; }
        public DateTime? FreezeDate { get; set; }
        public Int16? KBCalc { get; set; }
        public Int32? KitCount { get; set; }
        public string BinTo { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string OpRefSequence { get; set; }
    }
}
