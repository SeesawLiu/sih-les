using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.Sconit.Web.Models.SearchModels.INV
{
    public class OpReferenceBalanceSearchModel : SearchModelBase
    {
        //public string Item { get; set; }
        public string ItemCode { get; set; }
        public string OpReference { get; set; }
        public string CreateUserName { get; set; }
        public string LastModifyUserName { get; set; }
        public DateTime? CreateStartDate { get; set; }
        public DateTime? CreateEndDate { get; set; }
        public DateTime? ModifyStartDate { get; set; }
        public DateTime? ModifyEndDate { get; set; }
    }
}