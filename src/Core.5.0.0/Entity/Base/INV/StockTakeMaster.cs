using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
namespace com.Sconit.Entity.INV
{
    [Serializable]
    public partial class StockTakeMaster : EntityBase, IAuditable
    {
        #region O/R Mapping Properties

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [StringLength(50, ErrorMessageResourceName = "Errors_Common_FieldLengthExceed", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "StockTakeMaster_StNo", ResourceType = typeof(Resources.INV.StockTake))]
        public String StNo { get; set; }

        //全盘,抽盘
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "StockTakeMaster_Type", ResourceType = typeof(Resources.INV.StockTake))]
        public com.Sconit.CodeMaster.StockTakeType Type { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "StockTakeMaster_Region", ResourceType = typeof(Resources.INV.StockTake))]
        public String Region { get; set; }

        [Display(Name = "StockTakeMaster_Status", ResourceType = typeof(Resources.INV.StockTake))]
        public com.Sconit.CodeMaster.StockTakeStatus Status { get; set; }

        //是否需要扫描条码
        [Display(Name = "StockTakeMaster_IsScanHu", ResourceType = typeof(Resources.INV.StockTake))]
        public Boolean IsScanHu { get; set; }

        //生效时间,用于调节历史库存
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "StockTakeMaster_EffectiveDate", ResourceType = typeof(Resources.INV.StockTake))]
        public DateTime? EffectiveDate { get; set; }

        [Display(Name = "StockTakeMaster_BaseInventoryDate", ResourceType = typeof(Resources.INV.StockTake))]
        public DateTime? BaseInventoryDate { get; set; }
        [Display(Name = "StockTakeMaster_CostCenter", ResourceType = typeof(Resources.INV.StockTake))]
        public string CostCenter { get; set; }

        #region 审计字段
        public Int32 CreateUserId { get; set; }
        public string CreateUserName { get; set; }
        public DateTime CreateDate { get; set; }
        public Int32 LastModifyUserId { get; set; }
        public string LastModifyUserName { get; set; }
        public DateTime LastModifyDate { get; set; }
        public Int32? ReleaseUserId { get; set; }
        public string ReleaseUserName { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public Int32? StartUserId { get; set; }
        public string StartUserName { get; set; }
        public DateTime? StartDate { get; set; }
        public Int32? CompleteUserId { get; set; }
        public string CompleteUserName { get; set; }
        public DateTime? CompleteDate { get; set; }
        public Int32? CloseUserId { get; set; }
        public string CloseUserName { get; set; }
        public DateTime? CloseDate { get; set; }
        public Int32? CancelUserId { get; set; }
        public string CancelUserName { get; set; }
        public DateTime? CancelDate { get; set; }
        public string CancelReason { get; set; }
        public Int32 Version { get; set; }       
        #endregion

        #endregion

        public override int GetHashCode()
        {
            if (StNo != null)
            {
                return StNo.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            StockTakeMaster another = obj as StockTakeMaster;

            if (another == null)
            {
                return false;
            }
            else
            {
                return (this.StNo == another.StNo);
            }
        }
    }

}
