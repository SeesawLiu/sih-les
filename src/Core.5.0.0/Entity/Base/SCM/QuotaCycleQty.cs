using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity;

namespace com.Sconit.Entity.SCM
{
    [Serializable]
    public partial class QuotaCycleQty : EntityBase, IAuditable
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }
        [Display(Name = "Quota_Item", ResourceType = typeof(Resources.SCM.Quota))]
        public string Item { get; set; }
        [Display(Name = "Quota_RefItemCode", ResourceType = typeof(Resources.SCM.Quota))]
        public string RefItemCode { get; set; }
        [Display(Name = "Quota_ItemDesc", ResourceType = typeof(Resources.SCM.Quota))]
        public string ItemDesc { get; set; }
        [Display(Name = "Quota_CycleQty", ResourceType = typeof(Resources.SCM.Quota))]
        public Decimal? CycleQty { get; set; }
		public Int32 CreateUserId { get; set; }
		public string CreateUserName { get; set; }
		public DateTime CreateDate { get; set; }
        public Int32 LastModifyUserId { get; set; }
        [Display(Name = "Quota_LastModifyUserName", ResourceType = typeof(Resources.SCM.Quota))]
        public string LastModifyUserName { get; set; }
        [Display(Name = "Quota_LastModifyDate", ResourceType = typeof(Resources.SCM.Quota))]
        public DateTime LastModifyDate { get; set; }
		public Int32 Version { get; set; }
        
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
            QuotaCycleQty another = obj as QuotaCycleQty;

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
