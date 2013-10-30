using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.BIL
{
    [Serializable]
    public partial class BillMaster : EntityBase, IAuditable
    {
        #region O/R Mapping Properties
		
		//[Display(Name = "BillNo", ResourceType = typeof(Resources.BIL.BillMaster))]
		public string BillNo { get; set; }
		//[Display(Name = "ExteralBillNo", ResourceType = typeof(Resources.BIL.BillMaster))]
		public string ExteralBillNo { get; set; }
		//[Display(Name = "ReferenceBillNo", ResourceType = typeof(Resources.BIL.BillMaster))]
		public string ReferenceBillNo { get; set; }
		//[Display(Name = "Type", ResourceType = typeof(Resources.BIL.BillMaster))]
        public com.Sconit.CodeMaster.BillType Type { get; set; }
		//[Display(Name = "SubType", ResourceType = typeof(Resources.BIL.BillMaster))]
		public Int16 SubType { get; set; }
		//[Display(Name = "Status", ResourceType = typeof(Resources.BIL.BillMaster))]
		public Int16 Status { get; set; }
		//[Display(Name = "BillAddress", ResourceType = typeof(Resources.BIL.BillMaster))]
		public string BillAddress { get; set; }
		//[Display(Name = "BillAddressDescription", ResourceType = typeof(Resources.BIL.BillMaster))]
		public string BillAddressDescription { get; set; }
		//[Display(Name = "Party", ResourceType = typeof(Resources.BIL.BillMaster))]
		public string Party { get; set; }
		//[Display(Name = "PartyName", ResourceType = typeof(Resources.BIL.BillMaster))]
		public string PartyName { get; set; }
		//[Display(Name = "Currency", ResourceType = typeof(Resources.BIL.BillMaster))]
		public string Currency { get; set; }
		//[Display(Name = "IsIncludeTax", ResourceType = typeof(Resources.BIL.BillMaster))]
		public Boolean IsIncludeTax { get; set; }
		//[Display(Name = "Tax", ResourceType = typeof(Resources.BIL.BillMaster))]
		public string Tax { get; set; }
		//[Display(Name = "EffectiveDate", ResourceType = typeof(Resources.BIL.BillMaster))]
		public DateTime EffectiveDate { get; set; }
		//[Display(Name = "CreateUserId", ResourceType = typeof(Resources.BIL.BillMaster))]
		public Int32 CreateUserId { get; set; }
		//[Display(Name = "CreateUserName", ResourceType = typeof(Resources.BIL.BillMaster))]
		public string CreateUserName { get; set; }
		//[Display(Name = "CreateDate", ResourceType = typeof(Resources.BIL.BillMaster))]
		public DateTime CreateDate { get; set; }
		//[Display(Name = "LastModifyUserId", ResourceType = typeof(Resources.BIL.BillMaster))]
		public Int32 LastModifyUserId { get; set; }
		//[Display(Name = "LastModifyUserName", ResourceType = typeof(Resources.BIL.BillMaster))]
		public string LastModifyUserName { get; set; }
		//[Display(Name = "LastModifyDate", ResourceType = typeof(Resources.BIL.BillMaster))]
		public DateTime LastModifyDate { get; set; }
		//[Display(Name = "Version", ResourceType = typeof(Resources.BIL.BillMaster))]
		public Int32 Version { get; set; }
        
        #endregion

		public override int GetHashCode()
        {
			if (BillNo != null)
            {
                return BillNo.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            BillMaster another = obj as BillMaster;

            if (another == null)
            {
                return false;
            }
            else
            {
            	return (this.BillNo == another.BillNo);
            }
        } 
    }
	
}
