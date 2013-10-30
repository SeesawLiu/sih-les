using System;

//TODO: Add other using statements here

namespace com.Sconit.Entity.BIL
{
    public partial class PlanBill
    {
        #region Non O/R Mapping Properties

        public decimal CurrentVoidQty { get; set; }
        public decimal CurrentCancelVoidQty { get; set; }
        public decimal CurrentActingQty { get; set; }
        public string CurrentLocation { get; set; }
        public string CurrentHuId { get; set; }

        public Int32? CurrentActingBill { get; set; }
        public Int32? CurrentBillTransaction { get; set; }
        #endregion
    }
}