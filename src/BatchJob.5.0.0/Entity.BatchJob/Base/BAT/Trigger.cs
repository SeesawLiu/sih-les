using System;
using com.Sconit.Entity;

namespace com.Sconit.Entity.BatchJob.BAT
{
    [Serializable]
    public partial class Trigger : EntityBase
    {
        #region O/R Mapping Properties
		
		//[Display(Name = "Id", ResourceType = typeof(Resources.BAT.Trigger))]
		public Int32 Id { get; set; }
		//[Display(Name = "Job", ResourceType = typeof(Resources.BAT.Trigger))]
        public JobDetail JobDetail { get; set; }
		//[Display(Name = "Name", ResourceType = typeof(Resources.BAT.Trigger))]
		public string Name { get; set; }
		//[Display(Name = "Description", ResourceType = typeof(Resources.BAT.Trigger))]
		public string Description { get; set; }
		//[Display(Name = "PreviousFireTime", ResourceType = typeof(Resources.BAT.Trigger))]
		public DateTime? PreviousFireTime { get; set; }
		//[Display(Name = "NextFireTime", ResourceType = typeof(Resources.BAT.Trigger))]
		public DateTime? NextFireTime { get; set; }
		//[Display(Name = "RepeatCount", ResourceType = typeof(Resources.BAT.Trigger))]
		public Int32 RepeatCount { get; set; }
		//[Display(Name = "Interval", ResourceType = typeof(Resources.BAT.Trigger))]
		public Int32 Interval { get; set; }
		//[Display(Name = "IntervalType", ResourceType = typeof(Resources.BAT.Trigger))]
        public CodeMaster.TimeUnit IntervalType { get; set; }
		//[Display(Name = "TimesTriggered", ResourceType = typeof(Resources.BAT.Trigger))]
		public Int32 TimesTriggered { get; set; }
		//[Display(Name = "Status", ResourceType = typeof(Resources.BAT.Trigger))]
        public CodeMaster.TriggerStatus Status { get; set; }
        
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
            Trigger another = obj as Trigger;

            if (another == null)
            {
                return false;
            }
            else
            {
            	return (this.Id == another.Id);
            }
        }

        public string StatusName { get; set; }
    }
	
}
