using System;

namespace com.Sconit.Entity.FIS
{
    [Serializable]
    public partial class CreateIpDAT : EntityBase
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }
        /// <summary>
        /// ��ǰ����֪ͨ��
        /// </summary>
        public string ASN_NO { get; set; }

        /// <summary>
        /// ��ǰ����֪ͨ���к�
        /// </summary>
        public string ASN_ITEM { get; set; }

        /// <summary>
        /// �ջ��ֿ�
        /// </summary>
        public string WH_CODE { get; set; }
        /// <summary>
        /// �ֿ��ջ�����
        /// </summary>
        public string WH_DOCK { get; set; }

        /// <summary>
        /// ����ֿ��λ����
        /// </summary>
        public string WH_LOCATION { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        public string ITEM_CODE { get; set; }

        /// <summary>
        /// ��Ӧ�̴���
        /// </summary>
        public string SUPPLIER_CODE { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string QTY { get; set; }

        /// <summary>
        /// ������λ
        /// </summary>
        public string UOM { get; set; }

        /// <summary>
        /// ����BASE UNITת��������
        /// </summary>
        public string BASE_UNIT_QTY { get; set; }

        /// <summary>
        /// BASE UNIT��λ
        /// </summary>
        public string BASE_UNIT_UOM { get; set; }

        /// <summary>
        /// ������ʶ(Y:���/N:�����)
        /// </summary>
        public string QC_FLAG { get; set; }

        /// <summary>
        /// Ԥ�Ƶ�������
        /// </summary>
        public DateTime DELIVERY_DATE { get; set; }

        /// <summary>
        /// ����ʱ�䴰
        /// </summary>
        public string TIME_WINDOW { get; set; }

        /// <summary>
        /// PO��
        /// </summary>
        public string PO { get; set; }

        /// <summary>
        /// �����ʶ��(Y:����/N:�Ǽ���)
        /// </summary>
        public string FINANCE_FLAG { get; set; }

        /// <summary>
        /// �Ƿ������ܳɱ�ʶ��(Y:�ܳ�/N:���ܳ�)
        /// </summary>
        public string COMPONENT_FLAG { get; set; }

        /// <summary>
        /// ���䳵����
        /// </summary>
        public string TRACKID { get; set; }

        /// <summary>
        /// PO�к�
        /// </summary>
        public string PO_LINE { get; set; }

        /// <summary>
        /// ������Ϣ
        /// </summary>
        public string FactoryInfo { get; set; }

        /// <summary>
        /// Y ���߿� ��ΪYʱ��Ҫ���⴦��������
        /// </summary>
        public string F80XBJ { get; set; }

        /// <summary>
        /// ��F80X����0084�����µ��ջ��ص㣬������301������ʱ�� ��TO�ĵص�
        /// </summary>
        public string F80X_LOCATION { get; set; }


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

        /// <summary>
        /// �ϴ�ʱ��
        /// </summary>
        public DateTime? TIME_STAMP1 { get; set; }

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
            CreateIpDAT another = obj as CreateIpDAT;

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
