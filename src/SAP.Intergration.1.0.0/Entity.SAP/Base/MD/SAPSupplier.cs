using System;

namespace com.Sconit.Entity.SAP.MD
{
    [Serializable]
    public partial class SAPSupplier : SAPEntityBase
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }

		public string Code { get; set; }

        public string ShortCode { get; set; }

		public string Name { get; set; }

        public Int32 BatchNo { get; set; }

        public DateTime? CreateDate { get; set; }
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
            SAPSupplier another = obj as SAPSupplier;

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
