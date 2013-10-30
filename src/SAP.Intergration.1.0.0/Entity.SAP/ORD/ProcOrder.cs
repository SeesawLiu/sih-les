using System;
using System.Collections.Generic;

//TODO: Add other using statements here

namespace com.Sconit.Entity.SAP.ORD
{
    public partial class ProcOrder
    {
        #region Non O/R Mapping Properties

        public IList<ProcOrderDetail> ProcOrderDetails { get; set; }

        #endregion
    }
}