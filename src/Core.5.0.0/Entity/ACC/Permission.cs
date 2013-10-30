using System;

//TODO: Add other using statements here

namespace com.Sconit.Entity.ACC
{
    public partial class Permission
    {
        #region Non O/R Mapping Properties

        public string LongDescription
        {
            get
            {
                if (this.PermissionCategory == "Supplier" || this.PermissionCategory == "Customer")
                    return "[" + this.Code + "]";
                else
                    return this.Description;
            }
        }

        #endregion
    }
}