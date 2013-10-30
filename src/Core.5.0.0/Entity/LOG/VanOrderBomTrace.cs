using System;
using System.Collections.Generic;
using com.Sconit.Entity.SYS;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.BIL;
using System.Runtime.Serialization;

//TODO: Add other using statements here

namespace com.Sconit.Entity.LOG
{                        
    public partial class VanOrderBomTrace
    {
        #region Non O/R Mapping Properties

        //TODO: Add Non O/R Mapping Properties here.
        [Display(Name = "OrderMaster_TraceCode", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string TraceCode { get; set; }

        #endregion
    }
}