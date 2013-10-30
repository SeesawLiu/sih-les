using System;

namespace com.Sconit.Entity.SAP.ORD
{
    [Serializable]
    public partial class ProcOrder : SAPEntityBase, ITraceable
    {
        #region O/R Mapping Properties
		
		//[Display(Name = "Id", ResourceType = typeof(Resources.ORD.ProcOrder))]
		public Int32 Id { get; set; }
		//[Display(Name = "EBELN", ResourceType = typeof(Resources.ORD.ProcOrder))]
		public string EBELN { get; set; }
		//[Display(Name = "EBELP", ResourceType = typeof(Resources.ORD.ProcOrder))]
		public string EBELP { get; set; }
		//[Display(Name = "BSART", ResourceType = typeof(Resources.ORD.ProcOrder))]
		public string BSART { get; set; }
		//[Display(Name = "LIFNR", ResourceType = typeof(Resources.ORD.ProcOrder))]
		public string LIFNR { get; set; }
		//[Display(Name = "RESWK", ResourceType = typeof(Resources.ORD.ProcOrder))]
		public string RESWK { get; set; }
		//[Display(Name = "WERKS", ResourceType = typeof(Resources.ORD.ProcOrder))]
		public string WERKS { get; set; }
		//[Display(Name = "MATNR", ResourceType = typeof(Resources.ORD.ProcOrder))]
		public string MATNR { get; set; }
		//[Display(Name = "BISMT", ResourceType = typeof(Resources.ORD.ProcOrder))]
		public string BISMT { get; set; }
		//[Display(Name = "TXZ01", ResourceType = typeof(Resources.ORD.ProcOrder))]
		public string TXZ01 { get; set; }
		//[Display(Name = "LGFSB", ResourceType = typeof(Resources.ORD.ProcOrder))]
		public string LGFSB { get; set; }
		//[Display(Name = "MEINS", ResourceType = typeof(Resources.ORD.ProcOrder))]
		public string MEINS { get; set; }
		//[Display(Name = "LMEIN", ResourceType = typeof(Resources.ORD.ProcOrder))]
		public string LMEIN { get; set; }
		//[Display(Name = "UMREZ", ResourceType = typeof(Resources.ORD.ProcOrder))]
		public Decimal UMREZ { get; set; }
		//[Display(Name = "UMREN", ResourceType = typeof(Resources.ORD.ProcOrder))]
        public Decimal UMREN { get; set; }
		//[Display(Name = "PSTYP", ResourceType = typeof(Resources.ORD.ProcOrder))]
		public string PSTYP { get; set; }
		//[Display(Name = "ETFZ1", ResourceType = typeof(Resources.ORD.ProcOrder))]
		public string ETFZ1 { get; set; }
		//[Display(Name = "LOEKZ", ResourceType = typeof(Resources.ORD.ProcOrder))]
		public string LOEKZ { get; set; }
		//[Display(Name = "Status", ResourceType = typeof(Resources.ORD.ProcOrder))]
		public StatusEnum Status { get; set; }
		//[Display(Name = "CreateDate", ResourceType = typeof(Resources.ORD.ProcOrder))]
		public DateTime CreateDate { get; set; }
		//[Display(Name = "LastModifyDate", ResourceType = typeof(Resources.ORD.ProcOrder))]
		public DateTime LastModifyDate { get; set; }
		//[Display(Name = "ErrorCount", ResourceType = typeof(Resources.ORD.ProcOrder))]
		public Int32 ErrorCount { get; set; }
        //[Display(Name = "NOTQC", ResourceType = typeof(Resources.ORD.ProcOrderDetail))]
        public string NOTQC { get; set; }
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
            ProcOrder another = obj as ProcOrder;

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
