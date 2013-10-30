using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.Sconit.Web.Models.SearchModels.INV
{
    public class HuSearchModel : SearchModelBase
    {
        public String HuId { get; set; }
        public String Flow { get; set; }
        public String OrderNo { get; set; }
        public String Item { get; set; }
        public String CreateUserName { get; set; }
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
        public string lotNo { get; set; }
        public string ManufactureParty { get; set; }
        public DateTime? RemindExpireDate_start { get; set; }

        public DateTime? RemindExpireDate_End { get; set; }

        public string SupplierLotNo { get; set; }
    }
}