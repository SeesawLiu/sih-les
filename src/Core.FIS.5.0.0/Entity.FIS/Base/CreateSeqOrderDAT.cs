using System;

namespace com.Sconit.Entity.FIS
{
    [Serializable]
    public partial class CreateSeqOrderDAT : EntityBase
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }
        /// <summary>
        /// ���򵥺�
        /// </summary>
        public string SeqNo { get; set; }

        /// <summary>
        /// �к�
        /// </summary>
        public int Seq { get; set; }

        /// <summary>
        /// ����������
        /// </summary>
        public string Flow { get; set; }

        /// <summary>
        /// Ҫ�󷢻�ʱ��
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Ҫ�󵽻�ʱ��
        /// </summary>
        public DateTime WindowTime { get; set; }

        /// <summary>
        /// ����������
        /// </summary>
        public string PartyFrom { get; set; }

        /// <summary>
        /// ���շ�����
        /// </summary>
        public string PartyTo { get; set; }

        /// <summary>
        /// ���շ���λ
        /// </summary>
        public string LocationTo { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        public string Container { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// �����
        /// </summary>
        public string Item { get; set; }

        /// <summary>
        /// ������/Ʒ��
        /// </summary>
        public string ManufactureParty { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public decimal Qty { get; set; }

        /// <summary>
        /// ������ˮ��
        /// </summary>
        public string SequenceNumber { get; set; }

        /// <summary>
        /// Van��
        /// </summary>
        public string Van { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string Line { get; set; }

        /// <summary>
        /// ��λ
        /// </summary>
        public string Station { get; set; }

        /// <summary>
        /// ʧ�ܴ���
        /// </summary>
        public Int32 ErrorCount { get; set; }

        /// <summary>
        /// �ϴ�ʱ��
        /// </summary>
        public DateTime? UploadDate { get; set; }

        /// <summary>
        /// �ļ���
        /// </summary>
        public string FileName { get; set; }

        public bool IsCreateDat { get; set; }

        /// <summary>
        /// ������ϸId
        /// </summary>
        public int OrderDetId { get; set; }
        
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
            CreateSeqOrderDAT another = obj as CreateSeqOrderDAT;

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
