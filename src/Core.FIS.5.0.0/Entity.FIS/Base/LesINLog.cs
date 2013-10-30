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
        /// ҵ������
        /// </summary>
        [Display(Name = "LesINLog_Type", ResourceType = typeof(Resources.FIS.LesInLog))]
        public string Type { get; set; }

        /// <summary>
        /// �ƶ�����
        /// </summary>
        [Display(Name = "LesINLog_MoveType", ResourceType = typeof(Resources.FIS.LesInLog))]
        public string MoveType { get; set; }

        /// <summary>
        /// ���к���	16	SAP�������кţ�ϵͳ�Զ�������ˮ��	LES��ˮ��
        /// </summary>
        public string Sequense { get; set; }
        /// <summary>
        /// SAP����ƾ֤����	1.	�ɹ��ջ����ջ�����  2.	�ƿ⣺�ƿ�ASN��
        /// </summary>
        [Display(Name = "LesINLog_PO", ResourceType = typeof(Resources.FIS.LesInLog))]
        public string PO { get; set; }

        /// <summary>
        /// SAP����ƾ֤�к�	1��	�ɹ��ջ����ջ����к�  2���ƿ⣺�ƿ�ASN�к�
        /// </summary>
        [Display(Name = "LesINLog_POLine", ResourceType = typeof(Resources.FIS.LesInLog))]
        public string POLine { get; set; }

        /// <summary>
        /// WMS����
        /// </summary>
        [Display(Name = "LesINLog_WMSNo", ResourceType = typeof(Resources.FIS.LesInLog))]
        public string WMSNo { get; set; }

        /// <summary>
        /// WMS�к�
        /// </summary>
        public string WMSLine { get; set; }

        /// <summary>
        /// ����ʱ��
        /// </summary>
        [Display(Name = "LesINLog_HandTime", ResourceType = typeof(Resources.FIS.LesInLog))]
        public DateTime HandTime { get; set; }

        /// <summary>
        /// ���Ϻ���
        /// </summary>
        [Display(Name = "LesINLog_Item", ResourceType = typeof(Resources.FIS.LesInLog))]
        public string Item { get; set; }

        /// <summary>
        /// ������
        /// </summary>
        [Display(Name = "LesINLog_HandResult", ResourceType = typeof(Resources.FIS.LesInLog))]
        public string HandResult { get; set; }

        /// <summary>
        /// ʧ��ԭ��
        /// </summary>
        [Display(Name = "LesINLog_ErrorCause", ResourceType = typeof(Resources.FIS.LesInLog))]
        public string ErrorCause { get; set; }

        /// <summary>
        /// �Ƿ񴴽�DAT
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
        /// �ϴ�ʱ��
        /// </summary>
        public DateTime? UploadDate { get; set; }

        /// <summary>
        /// �ļ���
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
