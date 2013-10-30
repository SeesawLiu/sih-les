using System;
using System.ComponentModel.DataAnnotations;
namespace com.Sconit.Entity.PRD
{
    [Serializable]
    public partial class ShiftDetail : EntityBase, IAuditable
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "ShiftDetail_Shift", ResourceType = typeof(Resources.PRD.ShiftDetail))]
        public string Shift { get; set; }

        [Display(Name = "ShiftDetail_Sequence", ResourceType = typeof(Resources.PRD.ShiftDetail))]
        public Int16 Sequence { get; set; }

        [Display(Name = "ShiftDetail_StartTime", ResourceType = typeof(Resources.PRD.ShiftDetail))]
        public string StartTime { get; set; }

        [Display(Name = "ShiftDetail_EndTime", ResourceType = typeof(Resources.PRD.ShiftDetail))]
        public string EndTime { get; set; }

        [Display(Name = "ShiftDetail_IsOvernight", ResourceType = typeof(Resources.PRD.ShiftDetail))]
        public Boolean IsOvernight { get; set; }

        public Int32 CreateUserId { get; set; }
        public string CreateUserName { get; set; }
        public DateTime CreateDate { get; set; }
        public Int32 LastModifyUserId { get; set; }
        public string LastModifyUserName { get; set; }
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
            ShiftDetail another = obj as ShiftDetail;

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
