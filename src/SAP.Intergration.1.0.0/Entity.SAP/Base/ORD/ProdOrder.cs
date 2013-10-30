using System;
using com.Sconit.Entity.SAP;

namespace com.Sconit.Entity.SAP.ORD
{
    [Serializable]
    public partial class ProdOrder : SAPEntityBase, ITraceable
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }
        //[Display(Name = "AUFNR", ResourceType = typeof(Resources.ORD.ProdOrder))]
		public string AUFNR { get; set; }
		//[Display(Name = "CHARG", ResourceType = typeof(Resources.ORD.ProdOrder))]
		public string CHARG { get; set; }
		//[Display(Name = "DWERK", ResourceType = typeof(Resources.ORD.ProdOrder))]
		public string DWERK { get; set; }
		//[Display(Name = "DAUAT", ResourceType = typeof(Resources.ORD.ProdOrder))]
		public string DAUAT { get; set; }
		//[Display(Name = "MATNR", ResourceType = typeof(Resources.ORD.ProdOrder))]
		public string MATNR { get; set; }
		//[Display(Name = "MAKTX", ResourceType = typeof(Resources.ORD.ProdOrder))]
		public string MAKTX { get; set; }
		//[Display(Name = "ZLINE", ResourceType = typeof(Resources.ORD.ProdOrder))]
		public string ZLINE { get; set; }
		//[Display(Name = "GSTRS", ResourceType = typeof(Resources.ORD.ProdOrder))]
		public string GSTRS { get; set; }
		//[Display(Name = "CY_SEQNR", ResourceType = typeof(Resources.ORD.ProdOrder))]
		public string CY_SEQNR { get; set; }
		//[Display(Name = "GMEIN", ResourceType = typeof(Resources.ORD.ProdOrder))]
		public string GMEIN { get; set; }
		//[Display(Name = "GAMNG", ResourceType = typeof(Resources.ORD.ProdOrder))]
		public Decimal GAMNG { get; set; }
		//[Display(Name = "LGORT", ResourceType = typeof(Resources.ORD.ProdOrder))]
		public string LGORT { get; set; }
		//[Display(Name = "ERDAT", ResourceType = typeof(Resources.ORD.ProdOrder))]
		public string ERDAT { get; set; }
		//[Display(Name = "AEDAT", ResourceType = typeof(Resources.ORD.ProdOrder))]
		public string AEDAT { get; set; }
        public string LTEXT { get; set; }
        public string WORKCENTER { get; set; }
        //[Display(Name = "Status", ResourceType = typeof(Resources.ORD.ProdOrder))]
		public StatusEnum Status { get; set; }
        //[Display(Name = "CreateDate", ResourceType = typeof(Resources.ORD.ProdOrder))]
		public DateTime CreateDate { get; set; }
		//[Display(Name = "LastModifyDate", ResourceType = typeof(Resources.ORD.ProdOrder))]
		public DateTime LastModifyDate { get; set; }
		//[Display(Name = "ErrorCount", ResourceType = typeof(Resources.ORD.ProdOrder))]
        public Int32 ErrorCount { get; set; }
        
        #endregion

		public override int GetHashCode()
        {
			if (AUFNR != null)
            {
                return AUFNR.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            ProdOrder another = obj as ProdOrder;

            if (another == null)
            {
                return false;
            }
            else
            {
            	return (this.AUFNR == another.AUFNR);
            }
        } 
    }

    [Serializable]
    public class VanOrderSeqAdjIn
    {
        public string ExternalOrderNo { get; set; }
        public string OldStartTime { get; set; }
        public string OldSeq { get; set; }
        public string OldProdLine { get; set; }
        public string NewStartTime { get; set; }
        public string NewSeq { get; set; }
        public string NewProdLine { get; set; }
    }

    [Serializable]
    public class VanOrderSeqAdjOut
    {
        public string ExternalOrderNo { get; set; }
        public string IsSuccess { get; set; }
        public string Note { get; set; }
    }
}
