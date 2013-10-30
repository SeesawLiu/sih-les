using System;

namespace com.Sconit.Entity.FIS
{
    [Serializable]
    public partial class CancelReceiptMasterDAT : EntityBase
    {
        public Int32 Id { get; set; }

        public string WMSNo { get; set; }

        public Int32? WMSSeq { get; set; }

        public Decimal ReceivedQty { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? CreateDATDate { get; set; }

        public string DATFileName { get; set; }

        public Boolean IsCreateDat { get; set; }
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
            CancelReceiptMasterDAT another = obj as CancelReceiptMasterDAT;

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
