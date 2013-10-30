using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

//TODO: Add other using statements here

namespace com.Sconit.Entity.PRD
{
    public partial class StandardWorkingCalendar : EntityBase, IAuditable
    {
        #region Non O/R Mapping Properties

        [CodeDetailDescription(CodeMaster = com.Sconit.CodeMaster.CodeMaster.WorkingCalendarType, ValueField = "Type")]
        [Display(Name = "StandardWorkingCalendar_Type", ResourceType = typeof(Resources.PRD.StandardWorkingCalendar))]
        public string TypeDescription { get; set; }

        [CodeDetailDescription(CodeMaster = com.Sconit.CodeMaster.CodeMaster.DayOfWeek, ValueField = "DayOfWeek")]
        [Display(Name = "StandardWorkingCalendar_DayOfWeek", ResourceType = typeof(Resources.PRD.StandardWorkingCalendar))]
        public string DayOfWeekDescription { get; set; }

        [CodeDetailDescription(CodeMaster = com.Sconit.CodeMaster.CodeMaster.WorkingCalendarCategory, ValueField = "Category")]
        [Display(Name = "StandardWorkingCalendar_Category", ResourceType = typeof(Resources.PRD.StandardWorkingCalendar))]
        public string CategoryDescription { get; set; }
        #endregion
    }
}
