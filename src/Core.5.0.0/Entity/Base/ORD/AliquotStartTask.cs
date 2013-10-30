using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.ORD
{
    [Serializable]
    public partial class AliquotStartTask : EntityBase, IAuditable
    {
        public Int32 Id { get; set; }

        [Display(Name = "AliquotStartTask_Flow", ResourceType = typeof(Resources.ORD.AliquotStartTask))]
        public string Flow { get; set; }

        [Display(Name = "AliquotStartTask_VanNo", ResourceType = typeof(Resources.ORD.AliquotStartTask))]
        public string VanNo { get; set; }

        [Display(Name = "AliquotStartTask_IsStart", ResourceType = typeof(Resources.ORD.AliquotStartTask))]
        public Boolean IsStart { get; set; }

        [Display(Name = "AliquotStartTask_StartTime", ResourceType = typeof(Resources.ORD.AliquotStartTask))]
        public DateTime? StartTime { get; set; }

        public int CreateUserId { get; set; }
        public string CreateUserName { get; set; }
        public DateTime CreateDate { get; set; }
        public int LastModifyUserId { get; set; }
        public string LastModifyUserName { get; set; }
        public DateTime LastModifyDate { get; set; }

        public string OrderNo { get; set; }
    }
}
