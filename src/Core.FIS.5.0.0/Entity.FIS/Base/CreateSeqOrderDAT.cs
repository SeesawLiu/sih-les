using System;

namespace com.Sconit.Entity.FIS
{
    [Serializable]
    public partial class CreateSeqOrderDAT : EntityBase
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }
        /// <summary>
        /// 排序单号
        /// </summary>
        public string SeqNo { get; set; }

        /// <summary>
        /// 行号
        /// </summary>
        public int Seq { get; set; }

        /// <summary>
        /// 排序大类代码
        /// </summary>
        public string Flow { get; set; }

        /// <summary>
        /// 要求发货时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 要求到货时间
        /// </summary>
        public DateTime WindowTime { get; set; }

        /// <summary>
        /// 发出方代码
        /// </summary>
        public string PartyFrom { get; set; }

        /// <summary>
        /// 接收方代码
        /// </summary>
        public string PartyTo { get; set; }

        /// <summary>
        /// 接收方库位
        /// </summary>
        public string LocationTo { get; set; }

        /// <summary>
        /// 容器代码
        /// </summary>
        public string Container { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 零件号
        /// </summary>
        public string Item { get; set; }

        /// <summary>
        /// 制造商/品牌
        /// </summary>
        public string ManufactureParty { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }

        /// <summary>
        /// 车辆流水号
        /// </summary>
        public string SequenceNumber { get; set; }

        /// <summary>
        /// Van号
        /// </summary>
        public string Van { get; set; }

        /// <summary>
        /// 线体
        /// </summary>
        public string Line { get; set; }

        /// <summary>
        /// 工位
        /// </summary>
        public string Station { get; set; }

        /// <summary>
        /// 失败次数
        /// </summary>
        public Int32 ErrorCount { get; set; }

        /// <summary>
        /// 上传时间
        /// </summary>
        public DateTime? UploadDate { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

        public bool IsCreateDat { get; set; }

        /// <summary>
        /// 订单明细Id
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
