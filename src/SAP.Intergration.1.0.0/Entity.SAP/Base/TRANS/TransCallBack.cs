using System;

namespace com.Sconit.Entity.SAP.TRANS
{
    [Serializable]
    public partial class TransCallBack : SAPEntityBase
    {
        #region O/R Mapping Properties

        public Int64 Id { get; set; }
        public string FRBNR { get; set; }
        public string SGTXT { get; set; }
        public string MBLNR { get; set; }
        public string ZEILE { get; set; }
        public string BUDAT { get; set; }
        public string CPUDT { get; set; }
        public string MTYPE { get; set; }
        public string MSTXT { get; set; }
        public DateTime CreateDate { get; set; }

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
            TransCallBack another = obj as TransCallBack;

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
