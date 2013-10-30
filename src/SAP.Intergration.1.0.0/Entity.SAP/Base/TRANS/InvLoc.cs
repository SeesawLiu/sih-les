using System;

namespace com.Sconit.Entity.SAP.TRANS
{
    [Serializable]
    public partial class InvLoc : SAPEntityBase
    {
        #region O/R Mapping Properties

        public Int64 Id { get; set; }
        public Int32 SourceType { get; set; }
		public Int64 SourceId { get; set; }
		public string FRBNR { get; set; }
		public string SGTXT { get; set; }
        public string CreateUser { get; set; }
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
            InvLoc another = obj as InvLoc;

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
