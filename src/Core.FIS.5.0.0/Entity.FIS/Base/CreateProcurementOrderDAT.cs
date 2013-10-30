using System;

namespace com.Sconit.Entity.FIS
{
    [Serializable]
    public partial class CreateProcurementOrderDAT : EntityBase
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// 整车Van号
        /// </summary>
        public string Van { get; set; }
        /// <summary>
        /// 拉动方式
        /// </summary>
        public int OrderStrategy { get; set; }
        /// <summary>
        /// 要求发货时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 要求到货时间
        /// </summary>
        public DateTime WindowTime { get; set; }
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// 顺序号
        /// </summary>
        public string Sequence { get; set; }
        /// <summary>
        /// 发出方代码
        /// </summary>
        public string PartyFrom { get; set; }
        /// <summary>
        /// 接收方代码
        /// </summary>
        public string PartyTo { get; set; }
        /// <summary>
        /// 收货道口
        /// </summary>
        public string Dock { get; set; }
       /// <summary>
        /// 创建日期
       /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 路线
        /// </summary>
        public string Flow { get; set; }
        /// <summary>
        /// 行号
        /// </summary>
        public int LineSeq { get; set; }
        /// <summary>
        /// 零件号
        /// </summary>
        public string Item { get; set; }
        /// <summary>
        /// 制造商/品牌
        /// </summary>
        public string ManufactureParty { get; set; }
        /// <summary>
        /// 接收方库位
        /// </summary>
        public string LocationTo { get; set; }
        /// <summary>
        /// 工位
        /// </summary>
        public string Bin { get; set; }
        /// <summary>
        /// 要货数量
        /// </summary>
        public decimal OrderedQty { get; set; }
        /// <summary>
        /// 是否超发
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
