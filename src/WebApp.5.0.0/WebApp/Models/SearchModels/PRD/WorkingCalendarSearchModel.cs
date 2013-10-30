using System;

namespace com.Sconit.Web.Models.SearchModels.PRD
{
    public class WorkingCalendarSearchModel : SearchModelBase
    {
        public string SearchRegion { get; set; }
        public string SearchShift { get; set; }
        public int FlowStrategy { get; set; }
        public DateTime? StartWorkingDate { get; set; }
        public DateTime? EndWorkingDate { get; set; }
    }
}