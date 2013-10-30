using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.Sconit.Web.Models.SearchModels.PRD
{
    public class ItemConsumeSearchModel : SearchModelBase
    {
        public string SourceType { get; set; }
        public string ItemCode { get; set; }
        public string PONo { get; set; }
        public Boolean IsClose { get; set; }
        public DateTime? EffFrom { get; set; }
        public DateTime? EffTo { get; set; }
        public bool IsConsumed { get; set; }
        public int? IsConsume { get; set; }

        public Int32? OperationType { get; set; }

        public string UpdateUserName { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}