using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.Sconit.Web.Models.SearchModels.KB
{
    public class ItemDailyConsumeSearchModel : SearchModelBase
    {
        public string Region { get; set; }
        public string Location { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Item { get; set; }
    }
}