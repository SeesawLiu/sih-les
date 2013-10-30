using System;

namespace com.Sconit.Entity.FIS
{
    [Serializable]
    public partial class CreateProcurementOrderDAT : EntityBase
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }

        /// <summary>
        /// ������
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// ����Van��
        /// </summary>
        public string Van { get; set; }
        /// <summary>
        /// ������ʽ
        /// </summary>
        public int OrderStrategy { get; set; }
        /// <summary>
        /// Ҫ�󷢻�ʱ��
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// Ҫ�󵽻�ʱ��
        /// </summary>
        public DateTime WindowTime { get; set; }
        /// <summary>
        /// ���ȼ�
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// ˳���
        /// </summary>
        public string Sequence { get; set; }
        /// <summary>
        /// ����������
        /// </summary>
        public string PartyFrom { get; set; }
        /// <summary>
        /// ���շ�����
        /// </summary>
        public string PartyTo { get; set; }
        /// <summary>
        /// �ջ�����
        /// </summary>
        public string Dock { get; set; }
       /// <summary>
        /// ��������
       /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// ·��
        /// </summary>
        public string Flow { get; set; }
        /// <summary>
        /// �к�
        /// </summary>
        public int LineSeq { get; set; }
        /// <summary>
        /// �����
        /// </summary>
        public string Item { get; set; }
        /// <summary>
        /// ������/Ʒ��
        /// </summary>
        public string ManufactureParty { get; set; }
        /// <summary>
        /// ���շ���λ
        /// </summary>
        public string LocationTo { get; set; }
        /// <summary>
        /// ��λ
        /// </summary>
        public string Bin { get; set; }
        /// <summary>
        /// Ҫ������
        /// </summary>
        public decimal OrderedQty { get; set; }
        /// <summary>
        /// �Ƿ񳬷�
        /// </summary>
        public bool IsShipExceed { get; set; }

        public bool IsCreateDat { get; set; }

        public string FileName { get; set; }


        
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
            CreateProcurementOrderDAT another = obj as CreateProcurementOrderDAT;

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
