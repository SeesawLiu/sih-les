using System;

namespace com.Sconit.Entity.FIS
{
    [Serializable]
    public partial class CreateBarCode : EntityBase
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }

        public string HuId { get; set; }

        public string LotNo { get; set; }

        public string Item { get; set; }

        public decimal Qty { get; set; }

        public string ManufactureParty { get; set; }

        public string ASN { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? CreateDATDate { get; set; }

        public string DATFileName { get; set; }

        public Boolean IsCreateDat { get; set; }


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
            CreateBarCode another = obj as CreateBarCode;

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
