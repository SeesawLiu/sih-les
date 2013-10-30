using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;
using System;

namespace com.Sconit.Entity.PRD
{
    public partial class WorkingCalendar
    {
        #region Non O/R Mapping Properties

        [CodeDetailDescription(CodeMaster = com.Sconit.CodeMaster.CodeMaster.WorkingCalendarType, ValueField = "Type")]
        [Display(Name = "WorkingCalendar_Type", ResourceType = typeof(Resources.PRD.WorkingCalendar))]
        public string TypeDescription { get; set; }

        [CodeDetailDescription(CodeMaster = com.Sconit.CodeMaster.CodeMaster.WorkingCalendarCategory, ValueField = "Category")]
        [Display(Name = "WorkingCalendar_Category", ResourceType = typeof(Resources.PRD.WorkingCalendar))]
        public string CategoryDescription { get; set; }

        [CodeDetailDescription(CodeMaster = com.Sconit.CodeMaster.CodeMaster.DayOfWeek, ValueField = "DayOfWeek")]
        [Display(Name = "WorkingCalendar_DayOfWeekDescription", ResourceType = typeof(Resources.PRD.WorkingCalendar))]
        public string DayOfWeekDescription { get; set; }

        [Display(Name = "WorkingCalendar_EndDate", ResourceType = typeof(Resources.PRD.WorkingCalendar))]
        public DateTime? EndDate { get; set; }

        [Display(Name = "WorkingCalendar_StartWorkingDate", ResourceType = typeof(Resources.PRD.WorkingCalendar))]
        public DateTime? StartWorkingDate { get; set; }
        #endregion
    }
}