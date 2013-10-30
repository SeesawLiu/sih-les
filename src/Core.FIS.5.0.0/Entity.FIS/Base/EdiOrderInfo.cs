using System;

namespace com.Sconit.Entity.FIS
{
    [Serializable]
    public partial class EdiOrderInfo : EntityBase
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }
        /// <summary>
        /// ���ϵ���
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// ԭ���ϵ���
        /// </summary>
        public string RefOrderNo { get; set; }

        /// <summary>
        /// ·��
        /// </summary>
        public string Flow { get; set; }
        /// <summary>
        /// ����
        /// </summary>
        public string Item { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string ItemDescription { get; set; }

        /// <summary>
        /// �����ߣ���Լ���
        /// </summary>
        public string Supplier { get; set; }

        /// <summary>
        /// ��������+ʱ��
        /// </summary>
        public DateTime WindowTime { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        public Decimal OrderQty { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        public Decimal ReciveQty { get; set; }

        /// <summary>
        /// ˳��
        /// </summary>
        public Int32 Sequence { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        public Decimal ShipQty { get; set; }

        /// <summary>
        /// ���
        /// </summary>
        public string Bin { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        public DateTime? ReciveDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string EONo { get; set; }

        /// <summary>
        /// �빺����
        /// </summary>
        public string PONo { get; set; }

        /// <summary>
        /// ������ͷ
        /// </summary>
        public string Dock { get; set; }

        /// <summary>
        /// ��װ���
        /// </summary>
        public string UCDesc { get; set; }

        /// <summary>
        /// ��װ����
        /// </summary>
        public Decimal UnitCount { get; set; }

        /// <summary>
        /// �Ƿ����
        /// </summary>
        public Boolean IsInspect { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Code1 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Code2 { get; set; }

        /// <summary>
        /// ·�߲��ԣ�KB,SEQ,JIT��
        /// </summary>
        public Int16 OrderStrategy { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        public Int16 Type { get; set; }

        /// <summary>
        /// ����������
        /// </summary>
        public Int16 SubType { get; set; }

        /// <summary>
        /// �Ƿ��Ѵ���EDI
        /// </summary>
        public Boolean IsCreateEdi { get; set; }

        /// <summary>
        /// Ƿ��
        /// </summary>
        public Boolean IsDelayOrder { get; set; }

        /// <summary>
        /// ����Ʒ
        /// </summary>
        public Boolean IsChangeOrder { get; set; }

        /// <summary>
        /// �빺
        /// </summary>
        public Boolean IsCreateByPO { get; set; }

        /// <summary>
        /// ��Լ
        /// </summary>
        public Boolean IsCreateBySA { get; set; }

        /// <summary>
        /// ���ȼ�
        /// </summary>
        public Int16 Priority { get; set; }

        /// <summary>
        /// ܇��̖
        /// </summary>
        public string ProdSeq { get; set; }
        /// <summary>
        /// ܇�ʹ�̖
        /// </summary>
        public string ProdCode { get; set; }
        /// <summary>
        /// ��һ��܇̖
        /// </summary>
        public string LastVanNo { get; set; }
        /// <summary>
        /// ���˳���
        /// </summary>
        public string CurrVanNo { get; set; }
        /// <summary>
        /// ����
        /// </summary>
        public string SeqGroup { get; set; }
        /// <summary>
        /// �˴�
        /// </summary>
        public string TripTimes { get; set; }
        
        /// <summary>
        /// ״̬��0δ��,1�Ѵ�,2�ش���
        /// </summary>
        public Int16 Status { get; set; }

        /// <summary>
        /// �����û�
        /// </summary>
        public string CreateUserName { get; set; }

        /// <summary>
        /// �Ƿ�����
        /// </summary>
        public Boolean IsReject { get; set; }

        /// <summary>
        /// ���˵���
        /// </summary>
        public string ReceiptNo { get; set; }

        public Decimal RejectedQty { get; set; }
        
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
            EdiOrderInfo another = obj as EdiOrderInfo;

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
