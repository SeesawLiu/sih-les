using System;

namespace com.Sconit.Entity.SAP.MAP
{
    [Serializable]
    public partial class MapMoveTypeTCode : SAPEntityBase
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }
        public string BWART { get; set; }
        public string SOBKZ { get; set; }
        public string TCODE { get; set; }
        public string Description { get; set; }

        //public string CreateUser { get; set; }
        //public DateTime CreateDate { get; set; }
        //public string LastModifyUser { get; set; }
        //public DateTime LastModifyDate { get; set; }

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
            MapMoveTypeTCode another = obj as MapMoveTypeTCode;

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
