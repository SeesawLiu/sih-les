using System;

namespace com.Sconit.Entity.FIS
{
    [Serializable]
    public partial class YieldReturn : EntityBase
    {
        public Int32 Id { get; set; }

        public string IpNo { get; set; }
        public DateTime ArriveTime { get; set; }
        public string PartyFrom { get; set; }
        public string PartyTo { get; set; }
        public string Dock { get; set; }
        public DateTime IpCreateDate { get; set; }

        public string Seq { get; set; }
        public string Item { get; set; }
        public string ManufactureParty { get; set; }
        public Decimal Qty { get; set; }
        public Boolean IsConsignment { get; set; }

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
            YieldReturn another = obj as YieldReturn;

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
