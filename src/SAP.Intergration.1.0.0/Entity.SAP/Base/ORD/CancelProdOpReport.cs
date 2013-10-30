using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.SAP.ORD
{
    [Serializable]
    public partial class CancelProdOpReport : SAPEntityBase, ITraceable
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }
		public string AUFNR { get; set; }
        public string TEXT { get; set; }
        public StatusEnum Status { get; set; }
		public DateTime CreateDate { get; set; }
		public DateTime LastModifyDate { get; set; }
		public Int32 ErrorCount { get; set; }
        public string ReceiptNo { get; set; }
        public string OrderNo { get; set; }
        public Int32 OrderOpId { get; set; }
        
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
            CancelProdOpReport another = obj as CancelProdOpReport;

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
