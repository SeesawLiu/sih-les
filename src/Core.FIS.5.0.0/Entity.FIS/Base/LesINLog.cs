using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.FIS
{
    [Serializable]
    public partial class LesINLog : EntityBase
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }
        /// <summary>
        /// 业务类型
        /// </summary>
        [Display(Name = "LesINLog_Type", ResourceType = typeof(Resources.FIS.LesInLog))]
        public string Type { get; set; }

        /// <summary>
        /// 移动类型
        /// </summary>
        [Display(Name = "LesINLog_MoveType", ResourceType = typeof(Resources.FIS.LesInLog))]
        public string MoveType { get; set; }

        /// <summary>
        /// 序列号码	16	SAP进程序列号，系统自动生成流水号	LES流水号
        /// </summary>
        public string Sequense { get; set; }
        /// <summary>
        /// SAP物料凭证号码	1.	采购收货：收货单号  2.	移库：移库ASN号
        /// </summary>
        [Display(Name = "LesINLog_PO", ResourceType = typeof(Resources.FIS.LesInLog))]
        public string PO { get; set; }

        /// <summary>
        /// SAP物料凭证行号	1．	采购收货：收货单行号  2．移库：移库ASN行号
        /// </summary>
        [Display(Name = "LesINLog_POLine", ResourceType = typeof(Resources.FIS.LesInLog))]
        public string POLine { get; set; }

        /// <summary>
        /// WMS号码
        /// </summary>
        [Display(Name = "LesINLog_WMSNo", ResourceType = typeof(Resources.FIS.LesInLog))]
        public string WMSNo { get; set; }

        /// <summary>
        /// WMS行号
        /// </summary>
        public string WMSLine { get; set; }

        /// <summary>
        /// 处理时间
        /// </summary>
        [Display(Name = "LesINLog_HandTime", ResourceType = typeof(Resources.FIS.LesInLog))]
        public DateTime HandTime { get; set; }

        /// <summary>
        /// 物料号码
        /// </summary>
        [Display(Name = "LesINLog_Item", ResourceType = typeof(Resources.FIS.LesInLog))]
        public string Item { get; set; }

        /// <summary>
        /// 处理结果
        /// </summary>
        [Display(Name = "LesINLog_HandResult", ResourceType = typeof(Resources.FIS.LesInLog))]
        public string HandResult { get; set; }

        /// <summary>
        /// 失败原因
        /// </summary>
        [Display(Name = "LesINLog_ErrorCause", ResourceType = typeof(Resources.FIS.LesInLog))]
        public string ErrorCause { get; set; }

        /// <summary>
        /// 是否创建DAT
        /// </summary>
        [Display(Name = "LesINLog_IsCreateDat", ResourceType = typeof(Resources.FIS.LesInLog))]
        public bool IsCreateDat { get; set; }

        [Display(Name = "LesINLog_FileName", ResourceType = typeof(Resources.FIS.LesInLog))]
        public string FileName { get; set; }

        [Display(Name = "LesINLog_ASNNo", ResourceType = typeof(Resources.FIS.LesInLog))]
        public string ASNNo { get; set; }

        [Display(Name = "LesINLog_ExtNo", ResourceType = typeof(Resources.FIS.LesInLog))]
        public string ExtNo { get; set; }

        public Decimal? Qty { get; set; }

        public Boolean? QtyMark { get; set; }

        /// <summary>
        /// 上传时间
        /// </summary>
        public DateTime? UploadDate { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string LesFileName { get; set; }

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
            LesINLog another = obj as LesINLog;

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
