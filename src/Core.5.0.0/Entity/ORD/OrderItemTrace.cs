using System;
using System.ComponentModel.DataAnnotations;

//TODO: Add other using statements here

namespace com.Sconit.Entity.ORD
{
    public partial class OrderItemTrace
    {
        #region Non O/R Mapping Properties

        //TODO: Add Non O/R Mapping Properties here. 

        [Display(Name = "OrderItemTraceResult_ItemShortCode", ResourceType = typeof(Resources.ORD.OrderItemTraceResult))]
        public string ItemShortCode { get; set; }

        #endregion
    }
}