using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.ORD
{
    public partial class OrderOperation
    {
        #region Non O/R Mapping Properties

        private string _displayOperation;
        public string DisplayOperation
        {
            get
            {
                if (Operation != 0)
                    _displayOperation = Operation.ToString();
                return _displayOperation;
            }
            set { _displayOperation = value; }
        }

        [Display(Name = "OrderDetail_Item", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string Item { get; set; }

        [Display(Name = "OrderDetail_ItemDescription", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ItemDescription { get; set; }

        [Display(Name = "OrderDetail_Uom", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string Uom { get; set; }

        [Display(Name = "OrderDetail_UnitCount", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public decimal UnitCount { get; set; }

        [Display(Name = "OrderDetail_OrderedQty", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public decimal OrderedQty { get; set; }

        [Display(Name = "OrderOperation_CurrentReportQty", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public decimal CurrentReportQty { get; set; }

        [Display(Name = "OrderOperation_CurrentScrapQty", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public decimal CurrentScrapQty { get; set; }

        public string OperationWorkCenter { get { return  this.WorkCenter+"-"+this.Operation ; } }
        

        #endregion
    }
}