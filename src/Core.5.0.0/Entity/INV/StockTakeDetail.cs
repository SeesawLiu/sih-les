using System;

//TODO: Add other using statements here

namespace com.Sconit.Entity.INV
{
    public partial class StockTakeDetail
    {
        #region Non O/R Mapping Properties

        //�Ѿ������ϵļ�¼
        public decimal? OldQty { get; set; }

        /// <summary>
        /// null new, true update, false delete
        /// </summary>
        public bool? IsUpdate { get; set; }

        //TODO: Add Non O/R Mapping Properties here. 

        #endregion
    }
}