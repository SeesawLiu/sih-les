using System;
using System.Collections.Generic;

namespace com.Sconit.Entity.SAP.ORD
{
    [Serializable]
    public partial class OrderDetail : SAPEntityBase
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }

        public string OrderNo { get; set; }

        public com.Sconit.CodeMaster.OrderType OrderType { get; set; }

        public com.Sconit.CodeMaster.OrderSubType OrderSubType { get; set; }

        public Int32 Sequence { get; set; }

        public string Item { get; set; }

        public string ItemDescription { get; set; }

        public string ReferenceItemCode { get; set; }

        public string BaseUom { get; set; }

        public string Uom { get; set; }

         public string PartyTo { get; set; }

        public Decimal UnitCount { get; set; }
        public string UnitCountDescription { get; set; }
        public Decimal MinUnitCount { get; set; }

        public com.Sconit.CodeMaster.QualityType QualityType { get; set; }

        public string ManufactureParty { get; set; }

        public Decimal RequiredQty { get; set; }

        public Decimal OrderedQty { get; set; }

        public Decimal ShippedQty { get; set; }

        public Decimal ReceivedQty { get; set; }

        public Decimal RejectedQty { get; set; }

        public Decimal ScrapQty { get; set; }

        public Decimal PickedQty { get; set; }

        public Decimal UnitQty { get; set; }

        public Decimal? ReceiveLotSize { get; set; }

        public string LocationFrom { get; set; }

        public string LocationFromName { get; set; }

        public string LocationTo { get; set; }

        public string LocationToName { get; set; }

        public Boolean IsInspect { get; set; }


        public string BillAddress { get; set; }

        public string BillAddressDescription { get; set; }

        public string PriceList { get; set; }

        public Decimal? UnitPrice { get; set; }

        public Boolean IsProvisionalEstimate { get; set; }

        public string Tax { get; set; }

        public Boolean IsIncludeTax { get; set; }

        public string Bom { get; set; }

        public string Routing { get; set; }

        public com.Sconit.CodeMaster.OrderBillTerm BillTerm { get; set; }

        public string ZOPWZ { get; set; }
        public string ZOPID { get; set; }
        public string ZOPDS { get; set; }

        public Int32 CreateUserId { get; set; }

        public string CreateUserName { get; set; }

        public DateTime CreateDate { get; set; }

        public Int32 LastModifyUserId { get; set; }
        public string LastModifyUserName { get; set; }

        public DateTime LastModifyDate { get; set; }

        public Int32 Version { get; set; }
        public string Container { get; set; }
        public string ContainerDescription { get; set; }

        public string Currency { get; set; }

        public string PickStrategy { get; set; }

        public string ExtraDemandSource { get; set; }

        public Boolean IsScanHu { get; set; }
        public string ExternalOrderNo { get; set; }

        public string ExternalSequence { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        //public CodeMaster.ScheduleType ScheduleType { get; set; }

        public string ReserveNo { get; set; }

        public string ReserveLine { get; set; }

        public string BinTo { get; set; }
        public string WMSSeq { get; set; }

        public Boolean IsChangeUnitCount { get; set; }

        public string AUFNR { get; set; }
        public string ICHARG { get; set; }
        public string BWART { get; set; }

        public Int32 FreezeDays { get; set; }
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
            OrderDetail another = obj as OrderDetail;

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
