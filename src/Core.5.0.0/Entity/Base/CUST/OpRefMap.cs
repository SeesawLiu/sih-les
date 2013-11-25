using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.CUST
{
    [Serializable]
    public partial class OpRefMap : EntityBase, IAuditable
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }
		public string SAPProdLine { get; set; }
		public string ProdLine { get; set; }
		public string Item { get; set; }
		public string ItemDesc { get; set; }
		public string ItemRefCode { get; set; }
		public string OpReference { get; set; }
		public string RefOpReference { get; set; }
		public Boolean? IsPrimary { get; set; }
		public Int32? CreateUserId { get; set; }
		public string CreateUserName { get; set; }
		public DateTime? CreateDate { get; set; }
		public Int32? LastModifyUserId { get; set; }
		public string LastModifyUserName { get; set; }
		public DateTime? LastModifyDate { get; set; }
        
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
            OpRefMap another = obj as OpRefMap;

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
