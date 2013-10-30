using System;

//TODO: Add other using statements here

namespace com.Sconit.Entity.CUST
{
    public partial class FailCode
    {
        #region Non O/R Mapping Properties

        public string CodeDescription
        {
            get
            {
                return this.Code + " [" + this.CHNDescription + "]";
            }
        }

        #endregion
    }
}