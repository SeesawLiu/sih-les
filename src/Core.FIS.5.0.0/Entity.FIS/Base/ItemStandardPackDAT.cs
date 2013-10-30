using System;

namespace com.Sconit.Entity.FIS
{
    [Serializable]
    public partial class ItemStandardPackDAT : EntityBase
    {
        public Int32 Id { get; set; }

        public Int32 FlowDetId { get; set; }

        public string Item { get; set; }

        public string Pack { get; set; }

        public Decimal UC { get; set; }

        public string IOType { get; set; }

        public string Location { get; set; }

        public string Plant { get; set; }

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
            ItemStandardPackDAT another = obj as ItemStandardPackDAT;

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
