using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.SAP.ORD
{
    [Serializable]
    public partial class ProdOpBackflush : EntityBase, ITraceable
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }
        public Int32 SAPOpReportId { get; set; }
        public string AUFNR { get; set; }
		public string WERKS { get; set; }
        public string AUFPL { get; set; }
        public string APLZL { get; set; }
		public string PLNTY { get; set; }
		public string PLNNR { get; set; }
        public string PLNAL { get; set; }
        public string PLNFL { get; set; }
		public string VORNR { get; set; }
		public string ARBPL { get; set; }
		public string RUEK { get; set; }
		public string AUTWE { get; set; }
		public string WORKCENTER { get; set; }
		public Decimal GAMNG { get; set; }
		public Decimal SCRAP { get; set; }
		public StatusEnum Status { get; set; }
		public DateTime CreateDate { get; set; }
		public DateTime LastModifyDate { get; set; }
		public Int32 ErrorCount { get; set; }
		public string ProdLine { get; set; }
		public string OrderNo { get; set; }
        public string ReceiptNo { get; set; }
		public Int32 OrderOpId { get; set; }
		public Int32 OrderOpReportId { get; set; }
		public Int32 Version { get; set; }
        public DateTime EffectiveDate { get; set; }
        
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
            ProdOpBackflush another = obj as ProdOpBackflush;

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
