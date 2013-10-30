using System;

namespace com.Sconit.Entity.PRD
{
    [Serializable]
    public partial class LotNoScan : EntityBase
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }
        public string ProdLine { get; set; }
        public string OpReference { get; set; }
        public string Item { get; set; }
        public string ItemDesc { get; set; }
        public string LotNo { get; set; }
        public string OrderNo { get; set; }
        public string TraceCode { get; set; }
        public Int32 CreateUser { get; set; }
        public string CreateUserNm { get; set; }
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
            LotNoScan another = obj as LotNoScan;

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
