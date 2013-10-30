using System;
using com.Sconit.Entity;

namespace com.Sconit.Entity.BatchJob.BAT
{
    [Serializable]
    public partial class JobDetail : EntityBase
    {
        #region O/R Mapping Properties
		
		//[Display(Name = "Id", ResourceType = typeof(Resources.BAT.Job))]
		public Int32 Id { get; set; }
		//[Display(Name = "Name", ResourceType = typeof(Resources.BAT.Job))]
		public string Name { get; set; }
		//[Display(Name = "Description", ResourceType = typeof(Resources.BAT.Job))]
		public string Description { get; set; }
		//[Display(Name = "ServiceType", ResourceType = typeof(Resources.BAT.Job))]
		public string ServiceType { get; set; }
        
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
            JobDetail another = obj as JobDetail;

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
