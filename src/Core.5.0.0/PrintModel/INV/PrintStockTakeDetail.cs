using System;
using System.Runtime.Serialization;

namespace com.Sconit.PrintModel.INV
{
    [Serializable]
    [DataContract]
    public partial class PrintStockTakeDetail
    {
        #region
        public Int32 Id { get; set; }
        [DataMember]
        public string StNo { get; set; }

        [DataMember]
        public string Item { get; set; }
        [DataMember]
        public string ItemDescription { get; set; }
        [DataMember]
        public Int16 QualityType { get; set; }
        [DataMember]
        public string Uom { get; set; }
        public string BaseUom { get; set; }
        public Decimal UnitQty { get; set; }
        public string HuId { get; set; }
        public string LotNo { get; set; }
        [DataMember]
        public Decimal Qty { get; set; }
        [DataMember]
        public string Location { get; set; }
        public string Bin { get; set; }
		public Int32 CreateUserId { get; set; }
		public string CreateUserName { get; set; }
		public DateTime CreateDate { get; set; }
		public Int32 LastModifyUserId { get; set; }
		public string LastModifyUserName { get; set; }
		public DateTime LastModifyDate { get; set; }
        [DataMember]
        public string CSSupplier { get; set; }
        [DataMember]
        public Boolean IsConsigement { get; set; }

        [DataMember]
        public string RefItemCode { get; set; }
        
        
        #endregion

    }
	
}
