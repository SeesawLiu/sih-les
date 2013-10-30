using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.CUST
{
    [Serializable]
    public partial class ProdBomDetForecast : EntityBase
    {
        #region O/R Mapping Properties
		
		public Int32 RSNUM { get; set; }
		public Int32 RSPOS { get; set; }
		public string AUFNR { get; set; }
		public string WERKS { get; set; }
		public string MATNR { get; set; }
		public string MEINS { get; set; }
		public Decimal? BDMNG { get; set; }
		public string CHARG { get; set; }
		public DateTime? BDTER { get; set; }
		public DateTime CreateDate { get; set; }
        
        #endregion

		public override int GetHashCode()
        {
			if (RSNUM != 0 && RSPOS != null)
            {
                return RSNUM.GetHashCode() ^ RSPOS.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            ProdBomDetForecast another = obj as ProdBomDetForecast;

            if (another == null)
            {
                return false;
            }
            else
            {
            	return (this.RSNUM == another.RSNUM) && (this.RSPOS == another.RSPOS);
            }
        } 
    }
	
}
