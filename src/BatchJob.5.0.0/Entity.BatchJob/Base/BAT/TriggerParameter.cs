using System;
using com.Sconit.Entity;

namespace com.Sconit.Entity.BatchJob.BAT
{
    [Serializable]
    public partial class TriggerParameter : EntityBase
    {
        #region O/R Mapping Properties
		
		//[Display(Name = "Id", ResourceType = typeof(Resources.BAT.TriggerParameter))]
		public Int32 Id { get; set; }
		//[Display(Name = "Trigger", ResourceType = typeof(Resources.BAT.TriggerParameter))]
        public Int32 TriggerId { get; set; }
		//[Display(Name = "Key", ResourceType = typeof(Resources.BAT.TriggerParameter))]
		public string Key { get; set; }
		//[Display(Name = "Value", ResourceType = typeof(Resources.BAT.TriggerParameter))]
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
            TriggerParameter another = obj as TriggerParameter;

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
