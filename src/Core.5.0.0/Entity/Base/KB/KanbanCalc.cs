using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.KB
{
    [Serializable]
    public partial class KanbanCalc : EntityBase, IAuditable
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }
        public string BatchNo { get; set; }

        // uc
        [Display(Name = "KanbanCard_Qty", ResourceType = typeof(Resources.KB.KanbanCard))]
        public Decimal Qty { get; set; }

        // ucdesc
        [Display(Name = "KanbanCard_Container", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string Container { get; set; }

        // location bin
        [Display(Name = "KanbanCard_LocBin", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string LocBin { get; set; }

        // shelf
        [Display(Name = "KanbanCard_Shelf", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string Shelf { get; set; }

        // qitiaobian
        [Display(Name = "KanbanCard_QiTiaoBian", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string QiTiaoBian { get; set; }

        public CodeMaster.KBProcessCode Ret { get; set; }
        [Display(Name = "Msg", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string Msg { get; set; }

        [Display(Name = "OpTime", ResourceType = typeof(Resources.KB.KanbanCard))]
        public DateTime? OpTime { get; set; }

        [Display(Name = "KanbanNum", ResourceType = typeof(Resources.KB.KanbanCard))]
        public Int32 KanbanNum { get; set; }

        [Display(Name = "KanbanDeltaNum", ResourceType = typeof(Resources.KB.KanbanCard))]
        public Int32 KanbanDeltaNum { get; set; }

        public Decimal CalcKanbanNum { get; set; }

        public DateTime? CalcConsumeDate { get; set; }

        public Decimal CalcConsumeQty { get; set; }

        public Decimal SafeKanbanNum { get; set; }

        public Decimal BatchKanbanNum { get; set; }

        public Int32 Tiao { get; set; }

        public Int32 Bian { get; set; }

        /// <summary>
        /// ¿´°å±ê×¼×Ö¶Î
        /// </summary>
        [Display(Name = "KanbanCard_CardNo", ResourceType = typeof(Resources.KB.KanbanCard))]
		public string CardNo { get; set; }

        [Display(Name = "KanbanCard_Sequence", ResourceType = typeof(Resources.KB.KanbanCard))]
		public string Sequence { get; set; }

        [Display(Name = "KanbanCard_Flow", ResourceType = typeof(Resources.KB.KanbanCard))]
		public string Flow { get; set; }
		public Int32 FlowDetailId { get; set; }

        [Display(Name = "KanbanCard_Region", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string Region { get; set; }

        [Display(Name = "KanbanCard_RegionName", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string RegionName { get; set; }

        [Display(Name = "KanbanCard_LogisticCenterCode", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string LogisticCenterCode { get; set; }

        [Display(Name = "KanbanCard_Supplier", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string Supplier { get; set; }

        [Display(Name = "KanbanCard_SupplierName", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string SupplierName { get; set; }

        [Display(Name = "KanbanCard_Item", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string Item { get; set; }

        [Display(Name = "KanbanCard_ItemDescription", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string ItemDescription { get; set; }

        public com.Sconit.CodeMaster.KBStatus Status { get; set; }

        [Display(Name = "KanbanCard_MultiSupplyGroup", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string MultiSupplyGroup { get; set; }

        [Display(Name = "KanbanCard_EffectiveDate", ResourceType = typeof(Resources.KB.KanbanCard))]
		public DateTime? EffectiveDate { get; set; }

        [Display(Name = "KanbanCard_FreezeDate", ResourceType = typeof(Resources.KB.KanbanCard))]
        public DateTime? FreezeDate { get; set; }

        [Display(Name = "KanbanCard_LastUseDate", ResourceType = typeof(Resources.KB.KanbanCard))]
		public DateTime? LastUseDate { get; set; }

        [Display(Name = "KanbanCard_NeedReprint", ResourceType = typeof(Resources.KB.KanbanCard))]
        public Boolean NeedReprint { get; set; }

        [Display(Name = "KanbanCard_PONo", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string PONo { get; set; }

        [Display(Name = "KanbanCard_POLineNo", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string POLineNo { get; set; }

        [Display(Name = "KanbanCard_IsKit", ResourceType = typeof(Resources.KB.KanbanCard))]
        public Boolean IsKit { get; set; }

        [Display(Name = "KanbanCard_KBCalc", ResourceType = typeof(Resources.KB.KanbanCard))]
        public CodeMaster.KBCalculation? KBCalc { get; set; }

        [Display(Name = "KanbanCard_Location", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string Location { get; set; }

        [Display(Name = "KanbanCard_OpRef", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string OpRef { get; set; }

        [Display(Name = "KanbanCard_IsTrace", ResourceType = typeof(Resources.KB.KanbanCard))]
        public Boolean IsTrace { get; set; }

        [Display(Name = "KanbanCard_GroupNo", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string GroupNo { get; set; }

        [Display(Name = "KanbanCard_GroupDesc", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string GroupDesc { get; set; }

        [Display(Name = "KanbanCard_LogisticCenterName", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string LogisticCenterName { get; set; }

        [Display(Name = "KanbanCard_KitCount", ResourceType = typeof(Resources.KB.KanbanCard))]
        public Int32 KitCount { get; set; }

        public Int32 CreateUserId { get; set; }
        public string CreateUserName { get; set; }
        public DateTime CreateDate { get; set; }
        public Int32 LastModifyUserId { get; set; }
        public string LastModifyUserName { get; set; }
        public DateTime LastModifyDate { get; set; }

        #endregion

		public override int GetHashCode()
        {
			if (CardNo != null)
            {
                return CardNo.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            KanbanCard another = obj as KanbanCard;

            if (another == null)
            {
                return false;
            }
            else
            {
            	return (this.CardNo == another.CardNo);
            }
        } 
    }
	
}
