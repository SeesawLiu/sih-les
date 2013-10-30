using System;

namespace com.Sconit.Entity.SAP.ORD
{
    [Serializable]
    public partial class ProcOrderDetail : SAPEntityBase, ITraceable
    {
        #region O/R Mapping Properties
		
		//[Display(Name = "Id", ResourceType = typeof(Resources.ORD.ProcOrderDetail))]
		public Int32 Id { get; set; }
		//[Display(Name = "EBELN", ResourceType = typeof(Resources.ORD.ProcOrderDetail))]
		public string EBELN { get; set; }
		//[Display(Name = "EBELP", ResourceType = typeof(Resources.ORD.ProcOrderDetail))]
		public string EBELP { get; set; }
		//[Display(Name = "ETENR", ResourceType = typeof(Resources.ORD.ProcOrderDetail))]
		public string ETENR { get; set; }
		//[Display(Name = "EINDT", ResourceType = typeof(Resources.ORD.ProcOrderDetail))]
        public string EINDT { get; set; }
		//[Display(Name = "MATNR", ResourceType = typeof(Resources.ORD.ProcOrderDetail))]
		public string MATNR { get; set; }
		//[Display(Name = "MENGE", ResourceType = typeof(Resources.ORD.ProcOrderDetail))]
        public Decimal MENGE { get; set; }
		//[Display(Name = "WEMNG", ResourceType = typeof(Resources.ORD.ProcOrderDetail))]
		public Decimal? WEMNG { get; set; }
		//[Display(Name = "WAMNG", ResourceType = typeof(Resources.ORD.ProcOrderDetail))]
		public Decimal? WAMNG { get; set; }
		//[Display(Name = "Status", ResourceType = typeof(Resources.ORD.ProcOrderDetail))]
		public StatusEnum Status { get; set; }
		//[Display(Name = "CreateDate", ResourceType = typeof(Resources.ORD.ProcOrderDetail))]
		public DateTime CreateDate { get; set; }
		//[Display(Name = "LastModifyDate", ResourceType = typeof(Resources.ORD.ProcOrderDetail))]
		public DateTime LastModifyDate { get; set; }
		//[Display(Name = "ErrorCount", ResourceType = typeof(Resources.ORD.ProcOrderDetail))]
		public Int32 ErrorCount { get; set; }
        //[Display(Name = "EKPOBJ", ResourceType = typeof(Resources.ORD.ProcOrderDetail))]
        public string EKPOBJ { get; set; }
     
        
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
            ProcOrderDetail another = obj as ProcOrderDetail;

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
