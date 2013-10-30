using System;
using System.ComponentModel.DataAnnotations;
namespace com.Sconit.Entity.PRD
{
    [Serializable]
    public partial class SpecialTime : EntityBase, IAuditable
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }
        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "SpecialTime_Region", ResourceType = typeof(Resources.PRD.SpecialTime))]
        public string Region { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "SpecialTime_StartTime", ResourceType = typeof(Resources.PRD.SpecialTime))]
        public DateTime StartTime { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "SpecialTime_EndTime", ResourceType = typeof(Resources.PRD.SpecialTime))]
        public DateTime EndTime { get; set; }
        [Display(Name = "SpecialTime_Description", ResourceType = typeof(Resources.PRD.SpecialTime))]
        public string Description { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "SpecialTime_Type", ResourceType = typeof(Resources.PRD.SpecialTime))]
        public CodeMaster.WorkingCalendarType Type { get; set; }

        [Display(Name = "SpecialTime_ProdLine", ResourceType = typeof(Resources.PRD.SpecialTime))]
        public string ProdLine { get; set; }

        public CodeMaster.WorkingCalendarCategory Category { get; set; }

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
            SpecialTime another = obj as SpecialTime;

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
