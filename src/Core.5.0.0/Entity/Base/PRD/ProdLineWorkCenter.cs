using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.PRD
{
    [Serializable]
    public partial class ProdLineWorkCenter : EntityBase, IAuditable
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }
		public string Flow { get; set; }
        [Display(Name = "FlowMaster_WorkCenter", ResourceType = typeof(Resources.SCM.FlowMaster))]
        public string WorkCenter { get; set; }
		public Int32 CreateUserId { get; set; }
		public string CreateUserName { get; set; }
		public DateTime CreateDate { get; set; }
		public Int32 LastModifyUserId { get; set; }
		public string LastModifyUserName { get; set; }
		public DateTime LastModifyDate { get; set; }
        
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
            ProdLineWorkCenter another = obj as ProdLineWorkCenter;

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
