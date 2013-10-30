using System;

namespace com.Sconit.Entity.FIS
{
    [Serializable]
    public partial class CreateOrderDAT : EntityBase
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }

        /// <summary>
        /// ������
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// ���Ϻ�
        /// </summary>
        public string MATNR { get; set; }

        /// <summary>
        /// ָ����Ӧ��
        /// </summary>
        public string LIFNR { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string ENMNG { get; set; }
        /// <summary>
        /// ����Van��
        /// </summary>
        public string CHARG { get; set; }

        /// <summary>
        /// ������ɫ
        /// </summary>
        public string COLOR { get; set; }
    
        /// <summary>
        /// ����ʱ��
        /// </summary>
        public string TIME_STAMP { get; set; }

        /// <summary>
        /// ����˳��
        /// </summary>
        public string CY_SEQNR { get; set; }

        /// <summary>
        /// �ϴ�ʱ��
        /// </summary>
        public string TIME_STAMP1 { get; set; }

    
        /// <summary>
        /// ����������
        /// </summary>
        public string AUFNR { get; set; }

        /// <summary>
        /// �����ֿ�
        /// </summary>
        public string LGORT { get; set; }

        /// <summary>
        /// �ջ��ֿ�
        /// </summary>
        public string UMLGO { get; set; }
        
        /// <summary>
        /// Ŀ�Ĺ�λ
        /// </summary>
        public string LGPBE { get; set; }

        /// <summary>
        /// ����ʱ��
        /// </summary>
        public string REQ_TIME_STAMP { get; set; }

        /// <summary>
        /// �Ƿ�����
        /// </summary>
        public string FLG_SORT { get; set; }
        
        /// <summary>
        /// �ϼ����Ϻ�
        /// </summary>
        public string PLNBEZ { get; set; }

        /// <summary>
        /// ��װ������
        /// </summary>
        public string KTEXT { get; set; }

        /// <summary>
        /// �䵥��
        /// </summary>
        public string ZPLISTNO { get; set; }

        /// <summary>
        /// �Ƿ��Ѵ���EDI
        /// </summary>
        public Boolean IsCreateDat { get; set; }


        /// <summary>
        /// �����û�
        /// </summary>
        public string CreateUserName { get; set; }

        /// <summary>
        /// ʧ�ܴ���
        /// </summary>
        public Int32 ErrorCount { get; set; }

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
            CreateOrderDAT another = obj as CreateOrderDAT;

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
