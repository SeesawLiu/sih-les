namespace com.Sconit.Web.Models.SearchModels.PRD
{
    public class StandardWorkingCalendarSearchModel : SearchModelBase
    {
        public string SearchRegion { get; set; }
        public string SearchShift { get; set; }
        public int DayOfWeek { get; set; }
        public int FlowStrategy { get; set; }
    }
}