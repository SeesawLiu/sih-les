using System;

//TODO: Add other using statements here

namespace com.Sconit.Entity.CUST
{
    public partial class CreateOrderCode
    {
        #region Non O/R Mapping Properties

        public string CodeDescription
        {
            get
            {
                return this.Code + " [" + this.Description + "]";
            }
        }

        #endregion
    }
}