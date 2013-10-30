using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace com.Sconit.Web.Models.SearchModels.KB
{
    public class KanbanScanSearchModel : SearchModelBase
    {
        public string CardNo { get; set; }
        public string Supplier { get; set; }
        public string LcCode { get; set; }
        public string Item { get; set; }
        public string PONo { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Boolean IsNotOrdered { get; set; }
        public string TempKanbanCard { get; set; }
        public Boolean? IsTempKanbanCard { get; set; }
    }
}
