using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.CUST
{
    [Serializable]
    public partial class ProdLineLocationMap : EntityBase
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }
		public string ProdLine { get; set; }
		public string SapLocation { get; set; }
		public string Location { get; set; }
        
        #endregion

		public override int GetHashCode()
        {
			if (Id != 0)
            {
                return Id.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            ProdLineLocationMap another = obj as ProdLineLocationMap;

            if (another == null)
            {
                return false;
            }
            else
            {
            	return (this.Id == another.Id);
            }
        } 
    }
	
}
