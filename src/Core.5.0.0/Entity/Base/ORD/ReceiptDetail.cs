using System;
using com.Sconit.Entity.INV;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.ORD
{
    [Serializable]
    public partial class ReceiptDetail : EntityBase, IAuditable
    {
        #region O/R Mapping Properties

        
        public Int32 Id { get; set; }
        
        [Display(Name = "ReceiptDetail_ReceiptNo", ResourceType = typeof(Resources.ORD.ReceiptDetail))]
        public string ReceiptNo { get; set; }

        [Display(Name = "ReceiptDetail_Sequence", ResourceType = typeof(Resources.ORD.ReceiptDetail))]
        public Int32 Sequence { get; set; }

        [Display(Name = "ReceiptDetail_OrderNo", ResourceType = typeof(Resources.ORD.ReceiptDetail))]
        public string OrderNo { get; set; }
        
        public com.Sconit.CodeMaster.OrderType OrderType { get; set; }
        
        public com.Sconit.CodeMaster.OrderSubType OrderSubType { get; set; }
        
        public Int32? OrderDetailId { get; set; }
        
        public Int32 OrderDetailSequence { get; set; }

         [Display(Name = "ReceiptDetail_IpNo", ResourceType = typeof(Resources.ORD.ReceiptDetail))]
        public string IpNo { get; set; }
        
        public Int32? IpDetailId { get; set; }

        public Int32 IpDetailSequence { get; set; }

        [Display(Name = "ReceiptDetail_IpDetailType", ResourceType = typeof(Resources.ORD.ReceiptDetail))]
        public CodeMaster.IpDetailType IpDetailType { get; set; }

        public CodeMaster.IpGapAdjustOption IpGapAdjustOption { get; set; }

        [Display(Name = "ReceiptDetail_Item", ResourceType = typeof(Resources.ORD.ReceiptDetail))]
        public string Item { get; set; }
        
        [Display(Name = "ReceiptDetail_ItemDescription", ResourceType = typeof(Resources.ORD.ReceiptDetail))]
        public string ItemDescription { get; set; }

        [Display(Name = "ReceiptDetail_ReferenceItemCode", ResourceType = typeof(Resources.ORD.ReceiptDetail))]
        public string ReferenceItemCode { get; set; }

        public string BaseUom { get; set; }

        [Display(Name = "ReceiptDetail_Uom", ResourceType = typeof(Resources.ORD.ReceiptDetail))]
        public string Uom { get; set; }

        [Display(Name = "ReceiptDetail_UnitCount", ResourceType = typeof(Resources.ORD.ReceiptDetail))]
        public Decimal UnitCount { get; set; }

        public com.Sconit.CodeMaster.QualityType QualityType { get; set; }

        [Display(Name = "ReceiptDetail_ReceivedQty", ResourceType = typeof(Resources.ORD.ReceiptDetail))]
        public Decimal ReceivedQty { get; set; }
        //public Decimal RejectedQty { get; set; }
        public Decimal ScrapQty { get; set; }
		
        public Decimal UnitQty { get; set; }

        public string LocationFrom { get; set; }

        public string LocationFromName { get; set; }

        [Display(Name = "ReceiptDetail_LocationTo", ResourceType = typeof(Resources.ORD.ReceiptDetail))]
        public string LocationTo { get; set; }

        public string LocationToName { get; set; }

        public Boolean IsInspect { get; set; }

        public string BillAddress { get; set; }
        
        public string PriceList { get; set; }
        
        public Decimal? UnitPrice { get; set; }
        
        public string Currency { get; set; }
        
        public Boolean IsProvisionalEstimate { get; set; }
        
        public string Tax { get; set; }
        
        public Boolean IsIncludeTax { get; set; }
        
        public com.Sconit.CodeMaster.OrderBillTerm BillTerm { get; set; }

        public Int32 CreateUserId { get; set; }

        public string CreateUserName { get; set; }

        public DateTime CreateDate { get; set; }
        public Int32 LastModifyUserId { get; set; }
        public string LastModifyUserName { get; set; }
        public DateTime LastModifyDate { get; set; }
        public Int32 Version { get; set; }
        [Display(Name = "ReceiptDetail_ManufactureParty", ResourceType = typeof(Resources.ORD.ReceiptDetail))]
        public string ManufactureParty { get; set; }

        public string ExternalOrderNo { get; set; }

        public string ExternalSequence { get; set; }

        public string Flow { get; set; }
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
            ReceiptDetail another = obj as ReceiptDetail;

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
