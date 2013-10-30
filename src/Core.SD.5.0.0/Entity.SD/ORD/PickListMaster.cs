namespace com.Sconit.Entity.SD.ORD
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public partial class PickListMaster
    {
        #region O/R Mapping Properties

        public string PickListNo { get; set; }
        public com.Sconit.CodeMaster.PickListStatus Status { get; set; }
        public com.Sconit.CodeMaster.OrderType OrderType { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime WindowTime { get; set; }
        public string PartyFrom { get; set; }
        //public string PartyFromName { get; set; }
        public string PartyTo { get; set; }
        //public string PartyToName { get; set; }
        //public string ShipFrom { get; set; }
        //public string ShipFromAddress { get; set; }
        //public string ShipFromTel { get; set; }
        //public string ShipFromCell { get; set; }
        //public string ShipFromFax { get; set; }
        //public string ShipFromContact { get; set; }
        //public string ShipTo { get; set; }
        //public string ShipToAddress { get; set; }
        //public string ShipToTel { get; set; }
        //public string ShipToCell { get; set; }
        //public string ShipToFax { get; set; }
        //public string ShipToContact { get; set; }
        public string Dock { get; set; }
        public Boolean IsAutoReceive { get; set; }
        public Boolean IsReceiveScan { get; set; }
        public Boolean IsPrintAsn { get; set; }
        public Boolean IsPrintReceipt { get; set; }
        public Boolean IsReceiveExceed { get; set; }
        public Boolean IsReceiveFulfillUC { get; set; }
        public Boolean IsReceiveFifo { get; set; }
        //public Boolean IsAsnAuotClose { get; set; }
        public Boolean IsAsnUniqueReceive { get; set; }
        public Boolean IsCheckPartyFromAuthority { get; set; }
        public Boolean IsCheckPartyToAuthority { get; set; }
        public CodeMaster.CreateHuOption CreateHuOption { get; set; }
        public com.Sconit.CodeMaster.ReceiveGapTo ReceiveGapTo { get; set; }
        //public string AsnTemplate { get; set; }
        //public string ReceiptTemplate { get; set; }
        //public string HuTemplate { get; set; }
        public DateTime EffectiveDate { get; set; }
        //public Int32 CreateUserId { get; set; }
        //public string CreateUserName { get; set; }
        //public DateTime CreateDate { get; set; }
        //public Int32 LastModifyUserId { get; set; }
        //public string LastModifyUserName { get; set; }
        //public DateTime LastModifyDate { get; set; }
        //public DateTime? StartDate { get; set; }
        //public Int32? StartUser { get; set; }
        //public string StartUserName { get; set; }
        //public DateTime? CompleteDate { get; set; }
        //public Int32? CompleteUser { get; set; }
        //public string CompleteUserName { get; set; }
        //public DateTime? CloseDate { get; set; }
        //public Int32? CloseUser { get; set; }
        //public string CloseUserName { get; set; }
        //public DateTime? CancelDate { get; set; }
        //public Int32? CancelUser { get; set; }
        //public string CancelUserName { get; set; }
        //public string CancelReason { get; set; }
        //public Int32 Version { get; set; }

        #endregion

        #region ¼ð»õµ¥
        public List<PickListDetail> PickListDetails { get; set; }

        #endregion

    }

}
