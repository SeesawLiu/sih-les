using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.CUST
{
    [Serializable]
    public partial class EngineTraceDet : EntityBase
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }
        public string TraceCode { get; set; }
        public string VHVIN { get; set; }
        public string ZENGINE { get; set; }
        public string ScanZENGINE { get; set; }
		public Int32 CreateUserId { get; set; }
		public string CreateUserName { get; set; }
		public DateTime CreateDate { get; set; }
        
        #endregion

		public override int GetHashCode()
        {
            if (Id != null)
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
            EngineTraceDet another = obj as EngineTraceDet;

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
