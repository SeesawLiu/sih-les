using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using com.Sconit.PrintModel;

namespace com.Sconit.PrintModel.INV
{
    [Serializable]
    [DataContract]
    public partial class PrintHu : PrintBase
    {
        #region O/R Mapping Properties

        [DataMember]
        public string HuId { get; set; }

        [DataMember]
        public string LotNo { get; set; }

        [DataMember]
        public string Item { get; set; }

        [DataMember]
        public string ItemDescription { get; set; }

        [DataMember]
        public string ReferenceItemCode { get; set; }

        [DataMember]
        public string Uom { get; set; }

        [DataMember]
        public string BaseUom { get; set; }

        [DataMember]
        public Decimal UnitCount { get; set; }

        [DataMember]
        public Decimal Qty { get; set; }

        [DataMember]
        public Decimal UnitQty { get; set; }

        [DataMember]
        public DateTime ManufactureDate { get; set; }

        [DataMember]
        public string ManufactureParty { get; set; }

        [DataMember]
        public string ManufacturePartyDescription { get; set; }

        [DataMember]
        public DateTime? ExpireDate { get; set; }

        [DataMember]
        public DateTime? RemindExpireDate { get; set; }

        //[DataMember]
        //public string HuTemplate { get; set; }

        [DataMember]
        public Int16 PrintCount { get; set; }

        [DataMember]
        public DateTime? FirstInventoryDate { get; set; }

        [DataMember]
        public Boolean IsOdd { get; set; }

        [DataMember]
        public string SupplierLotNo { get; set; }

        [DataMember]
        public Boolean IsChangeUnitCount { get; set; }

        [DataMember]
        public Int32 CreateUserId { get; set; }

        [DataMember]
        public string CreateUserName { get; set; }

        [DataMember]
        public DateTime CreateDate { get; set; }

        [DataMember]
        public Int32 LastModifyUserId { get; set; }

        [DataMember]
        public string LastModifyUserName { get; set; }

        [DataMember]
        public DateTime LastModifyDate { get; set; }

        [DataMember]
        public string UnitCountDescription { get; set; }

        [DataMember]
        public string ContainerDesc { get; set; }

        [DataMember]
        public string LocationTo { get; set; }

        [DataMember]
        public string BinTo { get; set; }

        [DataMember]
        public string OldHus { get; set; }
        #endregion

    }
	
}
