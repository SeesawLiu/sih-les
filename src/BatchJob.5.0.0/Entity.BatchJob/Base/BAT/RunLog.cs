using System;
using com.Sconit.Entity;

namespace com.Sconit.Entity.BatchJob.BAT
{
    [Serializable]
    public partial class RunLog : EntityBase
    {
        #region O/R Mapping Properties
		
		//[Display(Name = "Id", ResourceType = typeof(Resources.BAT.RunLog))]
		public Decimal Id { get; set; }
		//[Display(Name = "Job", ResourceType = typeof(Resources.BAT.RunLog))]
        public Int32 JobDetailId { get; set; }
		//[Display(Name = "Trigger", ResourceType = typeof(Resources.BAT.RunLog))]
		public Int32 TriggerId { get; set; }
		//[Display(Name = "StartTime", ResourceType = typeof(Resources.BAT.RunLog))]
		public DateTime StartTime { get; set; }
		//[Display(Name = "EndTime", ResourceType = typeof(Resources.BAT.RunLog))]
		public DateTime? EndTime { get; set; }
		//[Display(Name = "Status", ResourceType = typeof(Resources.BAT.RunLog))]
        public CodeMaster.JobRunStatus Status { get; set; }
		//[Display(Name = "Message", ResourceType = typeof(Resources.BAT.RunLog))]
		public string Message { get; set; }
        
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
            RunLog another = obj as RunLog;

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
