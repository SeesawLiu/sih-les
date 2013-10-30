using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.PRD
{
    [Serializable]
    public partial class WorkingCalendar : EntityBase, IAuditable
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }
        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "WorkingCalendar_Region", ResourceType = typeof(Resources.PRD.WorkingCalendar))]
        public string Region { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "WorkingCalendar_Shift", ResourceType = typeof(Resources.PRD.WorkingCalendar))]
        public string Shift { get; set; }

        [Display(Name = "WorkingCalendar_WorkingDate", ResourceType = typeof(Resources.PRD.WorkingCalendar))]
        public DateTime WorkingDate { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "WorkingCalendar_Type", ResourceType = typeof(Resources.PRD.WorkingCalendar))]
        public CodeMaster.WorkingCalendarType Type { get; set; }

        [Display(Name = "WorkingCalendar_DayOfWeek", ResourceType = typeof(Resources.PRD.WorkingCalendar))]
        public DayOfWeek DayOfWeek { get; set; }

        public Int32 CreateUserId { get; set; }
        public string CreateUserName { get; set; }
        public DateTime CreateDate { get; set; }
        public Int32 LastModifyUserId { get; set; }
        public string LastModifyUserName { get; set; }
        public DateTime LastModifyDate { get; set; }

        [Display(Name = "WorkingCalendar_RegionName", ResourceType = typeof(Resources.PRD.WorkingCalendar))]
        public string RegionName { get; set; }

        public CodeMaster.WorkingCalendarCategory Category { get; set; }

        [Display(Name = "WorkingCalendar_ProdLine", ResourceType = typeof(Resources.PRD.WorkingCalendar))]
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
            WorkingCalendar another = obj as WorkingCalendar;

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
