using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.Sconit.Web.Models.SearchModels.CUST
{
    public class OpRefMapSearchModel : SearchModelBase
    {
        public string SAPProdLineSearch { get; set; }
        public string ProdLineSearch { get; set; }
        public string ItemSearch { get; set; }
        public string OpReferenceSearch { get; set; }
        public string CreateUserNameSearch { get; set; }
        public bool? IsPrimarySearch { get; set; }
        public DateTime? CreateStartDate { get; set; }
        public DateTime? CreateEndDate { get; set; }
    }
}