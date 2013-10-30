using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.SCM
{
    [Serializable]
    public partial class FlowStrategy : EntityBase, IAuditable
    {
        #region O/R Mapping Properties

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "FlowStrategy_Flow", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        public string Flow { get; set; }

        [StringLength(100, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "FlowStrategy_Desc1", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        public string Description { get; set; }

        [Display(Name = "FlowStrategy_Strategy", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        public com.Sconit.CodeMaster.FlowStrategy Strategy { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        public Decimal LeadTime { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        public Decimal EmergencyLeadTime { get; set; }
        public com.Sconit.CodeMaster.TimeUnit TimeUnit { get; set; }
        public string WindowTime1 { get; set; }
        public string WindowTime2 { get; set; }
        public string WindowTime3 { get; set; }
        public string WindowTime4 { get; set; }
        public string WindowTime5 { get; set; }
        public string WindowTime6 { get; set; }
        public string WindowTime7 { get; set; }
        public Int32 WeekInterval { get; set; }
        [Display(Name = "FlowStrategy_WaitTime", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        public Decimal WaitTime { get; set; }
        [Display(Name = "FlowStrategy_WaitBatch", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        public Int32 WaitBatch { get; set; }
        public Int32 CreateUserId { get; set; }
        public string CreateUserName { get; set; }
        public DateTime CreateDate { get; set; }
        public Int32 LastModifyUserId { get; set; }
        public string LastModifyUserName { get; set; }
        public DateTime LastModifyDate { get; set; }
        public DateTime? NextOrderTime { get; set; }
        public DateTime? NextWindowTime { get; set; }
        [Display(Name = "FlowStrategy_WindowTimeType", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        public com.Sconit.CodeMaster.WindowTimeType WindowTimeType { get; set; }
        [Display(Name = "FlowStrategy_WinTimeDiff", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        public Decimal WinTimeDiff { get; set; }

        [Display(Name = "FlowStrategy_WinTimeInterval", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        public Decimal WinTimeInterval { get; set; }

        [Display(Name = "FlowStrategy_SeqGroup", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        public string SeqGroup { get; set; }

        public Int32 MrpWeight { get; set; }

        public decimal MrpTotal { get; set; }

        public decimal MrpTotalAdjust { get; set; }

        public string MrpTag { get; set; }
        public DateTime? PreWinTime { get; set; }

        [Display(Name = "FlowStrategy_SafeTime", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        public decimal? SafeTime { get; set; }

        public Boolean IsCreatePickList { get; set; }

        [Display(Name = "FlowStrategy_IsOrderNow", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        public Boolean IsOrderNow { get; set; }
        #endregion

        public override int GetHashCode()
        {
            if (Flow != null)
            {
                return Flow.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            FlowStrategy another = obj as FlowStrategy;

            if (another == null)
            {
                return false;
            }
            else
            {
                return (this.Flow == another.Flow);
            }
        }
    }

}
