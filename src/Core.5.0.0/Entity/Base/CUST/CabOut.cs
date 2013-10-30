using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.CUST
{
    [Serializable]
    public partial class CabOut : EntityBase, IAuditable
    {
        #region O/R Mapping Properties
		
		public string OrderNo { get; set; }
		public string ProdLine { get; set; }
        public CodeMaster.CabType CabType { get; set; }
		public string CabItem { get; set; }
		public string QulityBarcode { get; set; }
		public CodeMaster.CabOutStatus Status { get; set; }
		public Int32 CreateUserId { get; set; }
		public string CreateUserName { get; set; }
		public DateTime CreateDate { get; set; }
		public Int32 LastModifyUserId { get; set; }
		public string LastModifyUserName { get; set; }
		public DateTime LastModifyDate { get; set; }
		public Int32? OutUser { get; set; }
		public string OutUserName { get; set; }
		public DateTime? OutDate { get; set; }
		public Int32? TransferUser { get; set; }
        public string TransferUserName { get; set; }
		public DateTime? TransferDate { get; set; }
        
        #endregion

		public override int GetHashCode()
        {
			if (OrderNo != null)
            {
                return OrderNo.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            CabOut another = obj as CabOut;

            if (another == null)
            {
                return false;
            }
            else
            {
            	return (this.OrderNo == another.OrderNo);
            }
        } 
    }
	
}
