using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.SAP.ORD
{
    [Serializable]
    public partial class CRSLSummary : EntityBase, ITraceable
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }

        public string BatchNo { get; set; }

        [Display(Name = "CRSL_EINDT", ResourceType = typeof(Resources.SI.CRSL))]
		public string EINDT { get; set; }

        [Display(Name = "CRSL_FRBNR", ResourceType = typeof(Resources.SI.CRSL))]
		public string FRBNR { get; set; }

        [Display(Name = "CRSL_LIFNR", ResourceType = typeof(Resources.SI.CRSL))]
		public string LIFNR { get; set; }

        [Display(Name = "CRSL_MATNR", ResourceType = typeof(Resources.SI.CRSL))]
		public string MATNR { get; set; }

        [Display(Name = "CRSL_MENGE", ResourceType = typeof(Resources.SI.CRSL))]
		public Decimal MENGE { get; set; }

		public string SGTXT { get; set; }

        [Display(Name = "CRSL_WERKS", ResourceType = typeof(Resources.SI.CRSL))]
		public string WERKS { get; set; }

        [Display(Name = "CRSL_EBELN", ResourceType = typeof(Resources.SI.CRSL))]
		public string EBELN { get; set; }

        [Display(Name = "CRSL_EBELP", ResourceType = typeof(Resources.SI.CRSL))]
		public string EBELP { get; set; }

        [Display(Name = "CRSL_MESSAGE", ResourceType = typeof(Resources.SI.CRSL))]
		public string MESSAGE { get; set; }

        [Display(Name = "CRSL_Status", ResourceType = typeof(Resources.SI.CRSL))]
        public StatusEnum Status { get; set; }

        [Display(Name = "CRSL_CreateDate", ResourceType = typeof(Resources.SI.CRSL))]
        public DateTime CreateDate { get; set; }

        [Display(Name = "CRSL_LastModifyDate", ResourceType = typeof(Resources.SI.CRSL))]
        public DateTime LastModifyDate { get; set; }

        [Display(Name = "CRSL_ErrorCount", ResourceType = typeof(Resources.SI.CRSL))]
        public Int32 ErrorCount { get; set; }

        public string ProcessBatchNo { get; set; }
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
            CRSLSummary another = obj as CRSLSummary;

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
