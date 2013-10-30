using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.SAP.MD
{
    [Serializable]
    public partial class SAPQuota : SAPEntityBase
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }
		public string QUNUM { get; set; }
		public string QUPOS { get; set; }
		public string LIFNR { get; set; }
		public string WERKS { get; set; }
		public string BEWRK { get; set; }
		public string MATNR { get; set; }
		public string VDATU { get; set; }
		public string BDATU { get; set; }
		public string BESKZ { get; set; }
		public string SOBES { get; set; }
		public Decimal? QUOTE { get; set; }
		public DateTime? CreateDate { get; set; }
		public Int32? BatchNo { get; set; }
        
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
            SAPQuota another = obj as SAPQuota;

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
