using System;

//TODO: Add other using statements here

namespace com.Sconit.Entity.PRD
{
    public partial class BomMaster
    {
        #region Non O/R Mapping Properties

        //TODO: Add Non O/R Mapping Properties here. 
        public BomMaster()
        {
        }

        public BomMaster(string code)
        {
            this.Code = code;
        }

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