using System;
using com.Sconit.Entity;

namespace com.Sconit.Entity.BatchJob.BAT
{
    [Serializable]
    public partial class JobParameter : EntityBase
    {
        #region O/R Mapping Properties
		
		//[Display(Name = "Id", ResourceType = typeof(Resources.BAT.JobParameter))]
		public Int32 Id { get; set; }
		//[Display(Name = "Job", ResourceType = typeof(Resources.BAT.JobParameter))]
		public Int32 JobId { get; set; }
		//[Display(Name = "Key", ResourceType = typeof(Resources.BAT.JobParameter))]
		public string Key { get; set; }
		//[Display(Name = "Value", ResourceType = typeof(Resources.BAT.JobParameter))]
		public string Value { get; set; }
        
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
            JobParameter another = obj as JobParameter;

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
