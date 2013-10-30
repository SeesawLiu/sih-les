using System;

//TODO: Add other using statements here

namespace com.Sconit.Entity.SAP.TRANS
{
    public partial class InvLoc
    {
        #region Non O/R Mapping Properties

        //TODO: Add Non O/R Mapping Properties here. 

        public enum SourceTypeEnum
        {
            LocTrans = 0,
            MiscOrder = 1,
            StockTake = 2,
            BackFlush = 3
        }       

        #endregion
    }
}