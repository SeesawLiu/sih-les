using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using com.Sconit.Entity.SYS;
using com.Sconit.Entity.VIEW;

//TODO: Add other using statements here

namespace com.Sconit.Entity.ACC
{
    public partial class User : IIdentity
    {
        public User()
        {
        }

        public User(Int32 id)
        {
            this.Id = id;
        }

        #region Non O/R Mapping Properties

        [Display(Name = "User_FullName", ResourceType = typeof(Resources.ACC.User))]
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthNotInRange", ErrorMessageResourceType = typeof(Resources.ErrorMessage), MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "User_NewPassword", ResourceType = typeof(Resources.ACC.User))]
        public string NewPassword { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [DataType(DataType.Password)]
        [Display(Name = "User_ConfirmNewPassword", ResourceType = typeof(Resources.ACC.User))]
        public string ConfirmPassword { get; set; }

        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.Language, ValueField = "Language")]
        [Display(Name = "User_Language", ResourceType = typeof(Resources.ACC.User))]
        public string LanguageDescription { get; set; }

        public string CodeDescription
        {
            get
            {
                return this.Code + " [" + this.FullName + "]";
            }
        }

        public IList<UserPermissionView> Permissions { get; set; }

        public IList<string> UrlPermissions
        {
            get
            {

                if (Permissions != null && Permissions.Count > 0)
                {
                    return (from p in this.Permissions
                            where p.PermissionCategoryType == com.Sconit.CodeMaster.PermissionCategoryType.Url
                            select p.PermissionCode).ToList();
                }

                return null;
            }
        }

        public IList<string> SupplierPersmissions
        {
            get
            {

                if (Permissions != null && Permissions.Count > 0)
                {
                    return (from p in this.Permissions
                            where p.PermissionCategoryType == com.Sconit.CodeMaster.PermissionCategoryType.Supplier
                            select p.PermissionCode).ToList();
                }

                return null;
            }
        }

        public IList<string> CustomerPermissions
        {
            get
            {

                if (Permissions != null && Permissions.Count > 0)
                {
                    return (from p in this.Permissions
                            where p.PermissionCategoryType == com.Sconit.CodeMaster.PermissionCategoryType.Customer
                            select p.PermissionCode).ToList();
                }

                return null;
            }
        }

        public IList<string> RegionPermissions
        {
            get
            {

                if (Permissions != null && Permissions.Count > 0)
                {
                    return (from p in this.Permissions
                            where p.PermissionCategoryType == com.Sconit.CodeMaster.PermissionCategoryType.Region
                            select p.PermissionCode).ToList();
                }

                return null;
            }
        }

        public bool HasEmail
        {
            get
            {

                if (string.IsNullOrEmpty(this.Email))
                {
                    return false;
                }
                return true;
            }
        }

        public bool HasMobilePhone
        {
            get
            {

                if (string.IsNullOrEmpty(this.MobilePhone))
                {
                    return false;
                }
                return true;
            }
        }

        #endregion

        #region IIdentity Implement
        public string AuthenticationType
        {
            get
            {

                //throw new NotImplementedException();
                return string.Empty;
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                //throw new NotImplementedException(); 
                //always return true;
                return true;
            }
        }

        public string Name
        {
            get { return Code; }
        }
        #endregion
    }
}