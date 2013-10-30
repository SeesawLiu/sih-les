using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.ORD
{
    [Serializable]
    public partial class IpMaster : EntityBase, IAuditable
    {
        #region O/R Mapping Properties

        [Display(Name = "IpMaster_IpNo", ResourceType = typeof(Resources.ORD.IpMaster))]
        public string IpNo { get; set; }

        public string ExternalIpNo { get; set; }

        public string GapIpNo { get; set; }

        public string SequenceNo { get; set; }
        [Display(Name = "IpMaster_Flow", ResourceType = typeof(Resources.ORD.IpMaster))]
        public string Flow { get; set; }

        [Display(Name = "IpMaster_Type", ResourceType = typeof(Resources.ORD.IpMaster))]
        public com.Sconit.CodeMaster.IpType Type { get; set; }

        [Display(Name = "IpMaster_OrderType", ResourceType = typeof(Resources.ORD.IpMaster))]
        public com.Sconit.CodeMaster.OrderType OrderType { get; set; }

        public com.Sconit.CodeMaster.OrderSubType OrderSubType { get; set; }

        public com.Sconit.CodeMaster.QualityType QualityType { get; set; }

        [Display(Name = "IpMaster_Status", ResourceType = typeof(Resources.ORD.IpMaster))]
        public com.Sconit.CodeMaster.IpStatus Status { get; set; }

        public DateTime DepartTime { get; set; }

        public DateTime ArriveTime { get; set; }

        [Display(Name = "IpMaster_PartyFrom", ResourceType = typeof(Resources.ORD.IpMaster))]
        public string PartyFrom { get; set; }

        [Display(Name = "IpMaster_PartyFromName", ResourceType = typeof(Resources.ORD.IpMaster))]
        public string PartyFromName { get; set; }

        [Display(Name = "IpMaster_PartyTo", ResourceType = typeof(Resources.ORD.IpMaster))]
        public string PartyTo { get; set; }

        [Display(Name = "IpMaster_PartyToName", ResourceType = typeof(Resources.ORD.IpMaster))]
        public string PartyToName { get; set; }

        [Display(Name = "IpMaster_ShipFrom", ResourceType = typeof(Resources.ORD.IpMaster))]
        public string ShipFrom { get; set; }

        [Display(Name = "IpMaster_ShipFromAddress", ResourceType = typeof(Resources.ORD.IpMaster))]
        public string ShipFromAddress { get; set; }
        [Display(Name = "IpMaster_ShipFromTel", ResourceType = typeof(Resources.ORD.IpMaster))]
        public string ShipFromTel { get; set; }

        public string ShipFromCell { get; set; }

        public string ShipFromFax { get; set; }
        [Display(Name = "IpMaster_ShipFromContact", ResourceType = typeof(Resources.ORD.IpMaster))]
        public string ShipFromContact { get; set; }

        [Display(Name = "IpMaster_ShipTo", ResourceType = typeof(Resources.ORD.IpMaster))]
        public string ShipTo { get; set; }

        [Display(Name = "IpMaster_ShipToAddress", ResourceType = typeof(Resources.ORD.IpMaster))]
        public string ShipToAddress { get; set; }
        [Display(Name = "IpMaster_ShipToTel", ResourceType = typeof(Resources.ORD.IpMaster))]
        public string ShipToTel { get; set; }

        public string ShipToCell { get; set; }

        public string ShipToFax { get; set; }
        [Display(Name = "IpMaster_ShipToContact", ResourceType = typeof(Resources.ORD.IpMaster))]
        public string ShipToContact { get; set; }

        [Display(Name = "IpMaster_Dock", ResourceType = typeof(Resources.ORD.IpMaster))]
        public string Dock { get; set; }
        [Display(Name = "IpMaster_IsAutoReceive", ResourceType = typeof(Resources.ORD.IpMaster))]
        public Boolean IsAutoReceive { get; set; }
        [Display(Name = "IpMaster_IsShipScanHu", ResourceType = typeof(Resources.ORD.IpMaster))]
        public Boolean IsShipScanHu { get; set; }
        [Display(Name = "IpMaster_IsReceiveScanHu", ResourceType = typeof(Resources.ORD.IpMaster))]
        public Boolean IsReceiveScanHu { get; set; }

        public Boolean IsPrintAsn { get; set; }

        public Boolean IsAsnPrinted { get; set; }
        [Display(Name = "IpMaster_IsPrintReceipt", ResourceType = typeof(Resources.ORD.IpMaster))]
        public Boolean IsPrintReceipt { get; set; }
        [Display(Name = "IpMaster_IsReceiveExceed", ResourceType = typeof(Resources.ORD.IpMaster))]
        public Boolean IsReceiveExceed { get; set; }
        [Display(Name = "IpMaster_IsReceiveFulfillUC", ResourceType = typeof(Resources.ORD.IpMaster))]
        public Boolean IsReceiveFulfillUC { get; set; }
        [Display(Name = "IpMaster_IsReceiveFifo", ResourceType = typeof(Resources.ORD.IpMaster))]
        public Boolean IsReceiveFifo { get; set; }
        [Display(Name = "IpMaster_IsAsnUniqueReceive", ResourceType = typeof(Resources.ORD.IpMaster))]
        public Boolean IsAsnUniqueReceive { get; set; }

        public CodeMaster.CreateHuOption CreateHuOption { get; set; }

        public com.Sconit.CodeMaster.ReceiveGapTo ReceiveGapTo { get; set; }
        [Display(Name = "IpMaster_AsnTemplate", ResourceType = typeof(Resources.ORD.IpMaster))]
        public string AsnTemplate { get; set; }

        public string ReceiptTemplate { get; set; }

        public string HuTemplate { get; set; }

        public Int32 CreateUserId { get; set; }

        [Display(Name = "IpMaster_CreateUserName", ResourceType = typeof(Resources.ORD.IpMaster))]
        public string CreateUserName { get; set; }

        [Display(Name = "IpMaster_CreateDate", ResourceType = typeof(Resources.ORD.IpMaster))]
        public DateTime CreateDate { get; set; }

        public Int32 LastModifyUserId { get; set; }

        [Display(Name = "IpMaster_LastModifyUserName", ResourceType = typeof(Resources.ORD.IpMaster))]
        public string LastModifyUserName { get; set; }
        [Display(Name = "IpMaster_LastModifyDate", ResourceType = typeof(Resources.ORD.IpMaster))]
        public DateTime LastModifyDate { get; set; }
        [Display(Name = "IpMaster_CloseDate", ResourceType = typeof(Resources.ORD.IpMaster))]
        public DateTime? CloseDate { get; set; }

        public Int32? CloseUserId { get; set; }
        [Display(Name = "IpMaster_CloseUserName", ResourceType = typeof(Resources.ORD.IpMaster))]
        public string CloseUserName { get; set; }

        public string CloseReason { get; set; }

        public Int32 Version { get; set; }
        [Display(Name = "IpMaster_IsCheckPartyFromAuthority", ResourceType = typeof(Resources.ORD.IpMaster))]
        public Boolean IsCheckPartyFromAuthority { get; set; }
        [Display(Name = "IpMaster_IsCheckPartyToAuthority", ResourceType = typeof(Resources.ORD.IpMaster))]
        public Boolean IsCheckPartyToAuthority { get; set; }

        public DateTime EffectiveDate { get; set; }
        [Display(Name = "IpMaster_WMSNo", ResourceType = typeof(Resources.ORD.IpMaster))]
        public string WMSNo { get; set; }

        [Display(Name = "ShipList_ShipNo", ResourceType = typeof(Resources.ORD.ShipList))]
        public string ShipNo { get; set; }
        [Display(Name = "ShipList_Vehicle", ResourceType = typeof(Resources.ORD.ShipList))]
        public string Vehicle { get; set; }
        #endregion

        public override int GetHashCode()
        {
            if (IpNo != null)
            {
                return IpNo.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            IpMaster another = obj as IpMaster;

            if (another == null)
            {
                return false;
            }
            else
            {
                return (this.IpNo == another.IpNo);
            }
        }
    }

}
