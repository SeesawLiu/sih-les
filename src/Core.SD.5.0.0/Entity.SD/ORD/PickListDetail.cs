using System;
using System.Collections.Generic;

namespace com.Sconit.Entity.SD.ORD
{
    [Serializable]
    public partial class PickListDetail
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }
        public string PickListNo { get; set; }
        public Int32 OrderDetailId { get; set; }
        public string Item { get; set; }
        public string ItemDescription { get; set; }
        public string ReferenceItemCode { get; set; }
        public string Uom { get; set; }
        public Decimal UnitCount { get; set; }
        public string LocationFrom { get; set; }
        //public string LocationFromName { get; set; }
        //public string Area { get; set; }
        public string Bin { get; set; }
        public Decimal Qty { get; set; }
        public Decimal PickedQty { get; set; }
        public string HuId { get; set; }
        public string LotNo { get; set; }
        public string LocationTo { get; set; }
        //public string LocationToName { get; set; }
        public Boolean IsInspect { get; set; }
        public Boolean IsOdd { get; set; }
        public Boolean IsInventory { get; set; }
        //public string Memo { get; set; }
        //public Int32 CreateUserId { get; set; }
        //public string CreateUserName { get; set; }
        //public DateTime CreateDate { get; set; }
        //public Int32 LastModifyUserId { get; set; }
        //public string LastModifyUserName { get; set; }
        //public DateTime LastModifyDate { get; set; }
        //public Int32 Version { get; set; }
        public string ManufactureParty { get; set; }
        public string OrderNo { get; set; }
        public com.Sconit.CodeMaster.QualityType QualityStatus { get; set; }

        #endregion

        #region ¸¨Öú×Ö¶Î
        public List<PickListDetailInput> PickListDetailInputs { get; set; }
        public decimal CurrentQty { get; set; }
        public int Carton { get; set; }
        public decimal RemainShippedQty { get; set; }
        #endregion

    }
    public class PickListDetailInput
    {
        public int Id { get; set; }
        public string HuId { get; set; }
    }
}
