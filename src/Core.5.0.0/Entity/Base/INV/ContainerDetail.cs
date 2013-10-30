using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.INV
{
    [Serializable]
    public partial class ContainerDetail : EntityBase
    {
        #region O/R Mapping Properties
		
		//[Display(Name = "ContId", ResourceType = typeof(Resources.INV.ContainerDetail))]
		public string ContId { get; set; }
		//[Display(Name = "Container", ResourceType = typeof(Resources.INV.ContainerDetail))]
		public string Container { get; set; }
		//[Display(Name = "Location", ResourceType = typeof(Resources.INV.ContainerDetail))]
		public string Location { get; set; }
		//[Display(Name = "IsEmpty", ResourceType = typeof(Resources.INV.ContainerDetail))]
		public Boolean IsEmpty { get; set; }
		//[Display(Name = "ActiveDate", ResourceType = typeof(Resources.INV.ContainerDetail))]
		public DateTime ActiveDate { get; set; }
        //[Display(Name = "CreateUserId", ResourceType = typeof(Resources.INV.ContainerDetail))]
		public Int32 CreateUserId { get; set; }
        //[Display(Name = "CreateUserName", ResourceType = typeof(Resources.INV.ContainerDetail))]
		public string CreateUserName { get; set; }
        //[Display(Name = "CreateDate", ResourceType = typeof(Resources.INV.ContainerDetail))]
		public DateTime CreateDate { get; set; }
        //[Display(Name = "LastModifyUserId", ResourceType = typeof(Resources.INV.ContainerDetail))]
		public Int32 LastModifyUserId { get; set; }
        //[Display(Name = "LastModifyUserName", ResourceType = typeof(Resources.INV.ContainerDetail))]
		public string LastModifyUserName { get; set; }
        //[Display(Name = "LastModifyDate", ResourceType = typeof(Resources.INV.ContainerDetail))]
		public DateTime LastModifyDate { get; set; }
        //[Display(Name = "Version", ResourceType = typeof(Resources.INV.ContainerDetail))]
		public Int32 Version { get; set; }
        
        #endregion

		public override int GetHashCode()
        {
			if (ContId != null)
            {
                return ContId.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            ContainerDetail another = obj as ContainerDetail;

            if (another == null)
            {
                return false;
            }
            else
            {
            	return (this.ContId == another.ContId);
            }
        } 
    }
	
}
