using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.ACC
{
    [Serializable]
    public partial class User : EntityBase, IAuditable
    {
        #region O/R Mapping Properties
		public Int32 Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName  = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [StringLength(50, ErrorMessageResourceName  = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "User_Code", ResourceType = typeof(Resources.ACC.User))]
		public string Code { get; set; }

		public string Password { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName  = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "User_FirstName", ResourceType = typeof(Resources.ACC.User))]
		public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "User_LastName", ResourceType = typeof(Resources.ACC.User))]
		public string LastName { get; set; }

        public com.Sconit.CodeMaster.UserType Type { get; set; }

        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "User_Email", ResourceType = typeof(Resources.ACC.User))]
		public string Email { get; set; }

        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "User_TelPhone", ResourceType = typeof(Resources.ACC.User))]
		public string TelPhone { get; set; }

        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "User_MobilePhone", ResourceType = typeof(Resources.ACC.User))]
		public string MobilePhone { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "User_Language", ResourceType = typeof(Resources.ACC.User))]
		public string Language { get; set; }

        [Display(Name = "User_IsActive", ResourceType = typeof(Resources.ACC.User))]
		public Boolean IsActive { get; set; }

        [Display(Name = "User_AccountExpired", ResourceType = typeof(Resources.ACC.User))]
		public Boolean AccountExpired { get; set; }

        [Display(Name = "User_AccountLocked", ResourceType = typeof(Resources.ACC.User))]
		public Boolean AccountLocked { get; set; }

        [Display(Name = "User_PasswordExpired", ResourceType = typeof(Resources.ACC.User))]
		public Boolean PasswordExpired { get; set; }

		public Int32 CreateUserId { get; set; }

        [Display(Name = "Common_CreateUserName", ResourceType = typeof(Resources.Global))]
		public string CreateUserName { get; set; }

        [Display(Name = "Common_CreateDate", ResourceType = typeof(Resources.Global))]
		public DateTime CreateDate { get; set; }

		public Int32 LastModifyUserId { get; set; }

        [Display(Name = "Common_LastModifyUserName", ResourceType = typeof(Resources.Global))]
		public string LastModifyUserName { get; set; }

        [Display(Name = "Common_LastModifyDate", ResourceType = typeof(Resources.Global))]
		public DateTime LastModifyDate { get; set; }
        
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
            User another = obj as User;

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
