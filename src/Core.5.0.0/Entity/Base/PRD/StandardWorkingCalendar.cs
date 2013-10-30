using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.PRD
{
    [Serializable]
    public partial class StandardWorkingCalendar : EntityBase, IAuditable
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }

        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "StandardWorkingCalendar_Region", ResourceType = typeof(Resources.PRD.StandardWorkingCalendar))]
        public string Region { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "StandardWorkingCalendar_Shift", ResourceType = typeof(Resources.PRD.StandardWorkingCalendar))]
        public string Shift { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "StandardWorkingCalendar_DayOfWeek", ResourceType = typeof(Resources.PRD.StandardWorkingCalendar))]
        public DayOfWeek DayOfWeek { get; set; }

        [Display(Name = "StandardWorkingCalendar_Type", ResourceType = typeof(Resources.PRD.StandardWorkingCalendar))]
        public CodeMaster.WorkingCalendarType Type { get; set; }

        public Int32 CreateUserId { get; set; }
        public string CreateUserName { get; set; }
        public DateTime CreateDate { get; set; }
        public Int32 LastModifyUserId { get; set; }
        public string LastModifyUserName { get; set; }
        public DateTime LastModifyDate { get; set; }

        [Display(Name = "StandardWorkingCalendar_RegionName", ResourceType = typeof(Resources.PRD.StandardWorkingCalendar))]
        public string RegionName { get; set; }

        public CodeMaster.WorkingCalendarCategory Category { get; set; }

        [Display(Name = "StandardWorkingCalendar_ProdLine", ResourceType = typeof(Resources.PRD.StandardWorkingCalendar))]
        public string ProdLine { get; set; }

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
            StandardWorkingCalendar another = obj as StandardWorkingCalendar;

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
