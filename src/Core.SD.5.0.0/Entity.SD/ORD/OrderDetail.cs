namespace com.Sconit.Entity.SD.ORD
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class OrderDetail
    {
        #region O/R Mapping Properties
        public Int32 Id { get; set; }
        public string OrderNo { get; set; }
        //public com.Sconit.CodeMaster.OrderType OrderType { get; set; }
        //public OrderMaster.SubTypeEnum OrderSubType { get; set; }
        public Int32 Sequence { get; set; }
        public string Item { get; set; }
        public string ItemDescription { get; set; }
        public string ReferenceItemCode { get; set; }
        public string BaseUom { get; set; }
        public string Uom { get; set; }
        public Decimal UnitCount { get; set; }
        public com.Sconit.CodeMaster.QualityType QualityType { get; set; }
        public string ManufactureParty { get; set; }
        //public Decimal RequiredQty { get; set; }
        public Decimal OrderedQty { get; set; }
        public Decimal ShippedQty { get; set; }
        public Decimal ReceivedQty { get; set; }
        //public Decimal RejectedQty { get; set; }
        //public Decimal ScrapQty { get; set; }
        //public Decimal PickedQty { get; set; }
        //public Decimal UnitQty { get; set; }
        //public Decimal? ReceiveLotSize { get; set; }
        public string LocationFrom { get; set; }
        //public string LocationFromName { get; set; }
        public string LocationTo { get; set; }
        //public string LocationToName { get; set; }
        //public Boolean IsInspect { get; set; }
        //public string BillAddress { get; set; }
        //public string BillAddressDescription { get; set; }
        //public string PriceList { get; set; }
        //public Decimal? UnitPrice { get; set; }
        //public Boolean IsProvisionalEstimate { get; set; }
        //public string Tax { get; set; }
        //public Boolean IsIncludeTax { get; set; }
        //public string Bom { get; set; }
        //public string Routing { get; set; }
        //public OrderMaster.BillTermEnum BillTerm { get; set; }
        //public Int32 CreateUserId { get; set; }
        //public string CreateUserName { get; set; }
        //public DateTime CreateDate { get; set; }
        //public Int32 LastModifyUserId { get; set; }
        //public string LastModifyUserName { get; set; }
        //public DateTime LastModifyDate { get; set; }
        //public Int32 Version { get; set; }
        //public string Container { get; set; }
        //public string Currency { get; set; } 
        public Boolean IsScanHu { get; set; }
        public Boolean IsChangeUnitCount { get; set; }
        #endregion

        #region 辅助字段
        public List<OrderDetailInput> OrderDetailInputs { get; set; }
        public decimal CurrentQty { get; set; }
        public int Carton { get; set; }
        public decimal RemainShippedQty { get; set; }
        public decimal RemainReceivedQty { get; set; }
        #endregion
    }

    public class OrderDetailInput
    {
        public int Id { get; set; }
        public string HuId { get; set; }
        public decimal Qty { get; set; }
        public decimal ShipQty { get; set; }
        public decimal ReceiveQty { get; set; }
        public string LotNo { get; set; }
        public string Bin { get; set; }
        public bool IsHuInLocation { get; set; }
    }

    public class AnDonInput
    {
        public string CardNo { get; set; }
        public string Flow { get; set; }
        public string OpRef { get; set; }
        public string Item { get; set; }
        public string Uom { get; set; }
        public string Supplier { get; set; }
        public decimal UnitCount { get; set; }
        //public string Note { get; set; }
    }
}
