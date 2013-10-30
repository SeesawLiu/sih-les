using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.CUST
{
    [Serializable]
    public partial class CreateOrderCode : EntityBase, IAuditable
    {
        #region O/R Mapping Properties
        [Display(Name = "CreateOrderCode_Code", ResourceType = typeof(Resources.CUST.FailCode))]
		public string Code { get; set; }
        [Display(Name = "CreateOrderCode_Description", ResourceType = typeof(Resources.CUST.FailCode))]
		public string Description { get; set; }
		//[Display(Name = "CreateUserId", ResourceType = typeof(Resources.CUST.CreateOrderCode))]
		public Int32 CreateUserId { get; set; }
		//[Display(Name = "CreateUserName", ResourceType = typeof(Resources.CUST.CreateOrderCode))]
		public string CreateUserName { get; set; }
		//[Display(Name = "CreateDate", ResourceType = typeof(Resources.CUST.CreateOrderCode))]
		public DateTime CreateDate { get; set; }
		//[Display(Name = "LastModifyUserId", ResourceType = typeof(Resources.CUST.CreateOrderCode))]
		public Int32 LastModifyUserId { get; set; }
		//[Display(Name = "LastModifyUserName", ResourceType = typeof(Resources.CUST.CreateOrderCode))]
		public string LastModifyUserName { get; set; }
		//[Display(Name = "LastModifyDate", ResourceType = typeof(Resources.CUST.CreateOrderCode))]
		public DateTime LastModifyDate { get; set; }
        
        #endregion

		public override int GetHashCode()
        {
			if (Code != null)
            {
                return Code.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            CreateOrderCode another = obj as CreateOrderCode;

            if (another == null)
            {
                return false;
            }
            else
            {
                return (this.Code == another.Code);
            }
        } 
    }
	
}
