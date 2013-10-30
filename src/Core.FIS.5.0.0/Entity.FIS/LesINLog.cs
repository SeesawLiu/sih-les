using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.FIS
{
    public partial class LesINLog
    {

        [Display(Name = "LesINLog_ShipQty", ResourceType = typeof(Resources.FIS.LesInLog))]
        public decimal ShipQty { get; set; }

        [Display(Name = "LesINLog_ReceivedQty", ResourceType = typeof(Resources.FIS.LesInLog))]
        public decimal ReceivedQty { get; set; }

        [Display(Name = "LesINLog_LocTo", ResourceType = typeof(Resources.FIS.LesInLog))]
        public string LocTo { get; set; }

        [Display(Name = "LesINLog_ReceiptStatus", ResourceType = typeof(Resources.FIS.LesInLog))]
        public string ReceiptStatus { get; set; }
    }
	
}
