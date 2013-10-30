using System;

namespace com.Sconit.Entity.SAP.ORD
{
    [Serializable]
    public partial class ProdOrderBomDet : SAPEntityBase, ITraceable
    {
        #region O/R Mapping Properties
		
		//[Display(Name = "Id", ResourceType = typeof(Resources.ORD.ProdOrderBomDet))]
		public Int64 Id { get; set; }
		//[Display(Name = "AUFNR", ResourceType = typeof(Resources.ORD.ProdOrderBomDet))]
		public string AUFNR { get; set; }
        public string ORDER_MATERIAL { get; set; }
        //[Display(Name = "MATERIAL", ResourceType = typeof(Resources.ORD.ProdOrderBomDet))]
		public string MATERIAL { get; set; }
		//[Display(Name = "BISMT", ResourceType = typeof(Resources.ORD.ProdOrderBomDet))]
		public string BISMT { get; set; }
		//[Display(Name = "MAKTX", ResourceType = typeof(Resources.ORD.ProdOrderBomDet))]
		public string MAKTX { get; set; }
		//[Display(Name = "BASE_UOM", ResourceType = typeof(Resources.ORD.ProdOrderBomDet))]
		public string BASE_UOM { get; set; }
		//[Display(Name = "REQ_QUAN", ResourceType = typeof(Resources.ORD.ProdOrderBomDet))]
		public Decimal? REQ_QUAN { get; set; }
		//[Display(Name = "STORAGE_LOCATION", ResourceType = typeof(Resources.ORD.ProdOrderBomDet))]
		public string STORAGE_LOCATION { get; set; }
		//[Display(Name = "GW", ResourceType = typeof(Resources.ORD.ProdOrderBomDet))]
		public string GW { get; set; }
		//[Display(Name = "SEQUENCE", ResourceType = typeof(Resources.ORD.ProdOrderBomDet))]
		public string SEQUENCE { get; set; }
		//[Display(Name = "OPERATION", ResourceType = typeof(Resources.ORD.ProdOrderBomDet))]
		public string OPERATION { get; set; }
		//[Display(Name = "RESERVATION_NUMBER", ResourceType = typeof(Resources.ORD.ProdOrderBomDet))]
		public string RESERVATION_NUMBER { get; set; }
		//[Display(Name = "RESERVATION_ITEM", ResourceType = typeof(Resources.ORD.ProdOrderBomDet))]
		public string RESERVATION_ITEM { get; set; }
		//[Display(Name = "BESKZ", ResourceType = typeof(Resources.ORD.ProdOrderBomDet))]
		public string BESKZ { get; set; }
		//[Display(Name = "SOBSL", ResourceType = typeof(Resources.ORD.ProdOrderBomDet))]
		public string SOBSL { get; set; }
		//[Display(Name = "WZ", ResourceType = typeof(Resources.ORD.ProdOrderBomDet))]
		public string WZ { get; set; }
		//[Display(Name = "ZOPID", ResourceType = typeof(Resources.ORD.ProdOrderBomDet))]
		public string ZOPID { get; set; }
		//[Display(Name = "ZOPDS", ResourceType = typeof(Resources.ORD.ProdOrderBomDet))]
		public string ZOPDS { get; set; }
		//[Display(Name = "LIFNR", ResourceType = typeof(Resources.ORD.ProdOrderBomDet))]
		public string LIFNR { get; set; }
        public string WORK_CENTER { get; set; }
        public string ICHARG { get; set; }
        public string BWART { get; set; }
        //[Display(Name = "Status", ResourceType = typeof(Resources.ORD.ProdOrderBomDet))]
        public StatusEnum Status { get; set; }
		//[Display(Name = "CreateDate", ResourceType = typeof(Resources.ORD.ProdOrderBomDet))]
		public DateTime CreateDate { get; set; }
		//[Display(Name = "LastModifyDate", ResourceType = typeof(Resources.ORD.ProdOrderBomDet))]
		public DateTime LastModifyDate { get; set; }
		//[Display(Name = "ErrorCount", ResourceType = typeof(Resources.ORD.ProdOrderBomDet))]
		public Int32 ErrorCount { get; set; }
        
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
            ProdOrderBomDet another = obj as ProdOrderBomDet;

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
