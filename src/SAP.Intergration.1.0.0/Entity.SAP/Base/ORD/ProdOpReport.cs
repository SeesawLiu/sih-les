using System;

namespace com.Sconit.Entity.SAP.ORD
{
    [Serializable]
    public partial class ProdOpReport : SAPEntityBase, ITraceable, IAuditable
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }
		public string AUFNR { get; set; }
		public string WORKCENTER { get; set; }
		public Decimal GAMNG { get; set; }
        public Decimal SCRAP { get; set; }
        public string TEXT { get; set; }
        public StatusEnum Status { get; set; }
        public Int32 CreateUserId { get; set; }
        public string CreateUserName { get; set; }
        public DateTime CreateDate { get; set; }
        public Int32 LastModifyUserId { get; set; }
        public string LastModifyUserName { get; set; }
        public DateTime LastModifyDate { get; set; }
        public Int32 Version { get; set; }
		public Int32 ErrorCount { get; set; }
        public string ReceiptNo { get; set; }
        public Boolean IsCancel { get; set; }
        public string OrderNo { get; set; }
        public Int32 OrderOpId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string ProdLine { get; set; }
        public Int32 OrderOpReportId { get; set; }
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
            ProdOpReport another = obj as ProdOpReport;

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
