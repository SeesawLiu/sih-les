using System;

namespace com.Sconit.Entity.FIS
{
    [Serializable]
    public partial class EdiOrderInfo : EntityBase
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }
        /// <summary>
        /// 收料单号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 原收料单号
        /// </summary>
        public string RefOrderNo { get; set; }

        /// <summary>
        /// 路线
        /// </summary>
        public string Flow { get; set; }
        /// <summary>
        /// 件号
        /// </summary>
        public string Item { get; set; }

        /// <summary>
        /// 件名
        /// </summary>
        public string ItemDescription { get; set; }

        /// <summary>
        /// 接受者，合约编号
        /// </summary>
        public string Supplier { get; set; }

        /// <summary>
        /// 交货日期+时间
        /// </summary>
        public DateTime WindowTime { get; set; }

        /// <summary>
        /// 订单数量
        /// </summary>
        public Decimal OrderQty { get; set; }

        /// <summary>
        /// 验收数量
        /// </summary>
        public Decimal ReciveQty { get; set; }

        /// <summary>
        /// 顺序
        /// </summary>
        public Int32 Sequence { get; set; }

        /// <summary>
        /// 交货数量
        /// </summary>
        public Decimal ShipQty { get; set; }

        /// <summary>
        /// 库格
        /// </summary>
        public string Bin { get; set; }

        /// <summary>
        /// 入库日期
        /// </summary>
        public DateTime? ReciveDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string EONo { get; set; }

        /// <summary>
        /// 请购单号
        /// </summary>
        public string PONo { get; set; }

        /// <summary>
        /// 交货码头
        /// </summary>
        public string Dock { get; set; }

        /// <summary>
        /// 包装规格
        /// </summary>
        public string UCDesc { get; set; }

        /// <summary>
        /// 包装容量
        /// </summary>
        public Decimal UnitCount { get; set; }

        /// <summary>
        /// 是否免检
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
        /// 路线策略（KB,SEQ,JIT）
        /// </summary>
        public Int16 OrderStrategy { get; set; }

        /// <summary>
        /// 订单类型
        /// </summary>
        public Int16 Type { get; set; }

        /// <summary>
        /// 订单子类型
        /// </summary>
        public Int16 SubType { get; set; }

        /// <summary>
        /// 是否已创建EDI
        /// </summary>
        public Boolean IsCreateEdi { get; set; }

        /// <summary>
        /// 欠交
        /// </summary>
        public Boolean IsDelayOrder { get; set; }

        /// <summary>
        /// 交换品
        /// </summary>
        public Boolean IsChangeOrder { get; set; }

        /// <summary>
        /// 请购
        /// </summary>
        public Boolean IsCreateByPO { get; set; }

        /// <summary>
        /// 合约
        /// </summary>
        public Boolean IsCreateBySA { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public Int16 Priority { get; set; }

        /// <summary>
        /// 車身號
        /// </summary>
        public string ProdSeq { get; set; }
        /// <summary>
        /// 車型代號
        /// </summary>
        public string ProdCode { get; set; }
        /// <summary>
        /// 上一趟車號
        /// </summary>
        public string LastVanNo { get; set; }
        /// <summary>
        /// 本趟车号
        /// </summary>
        public string CurrVanNo { get; set; }
        /// <summary>
        /// 件名
        /// </summary>
        public string SeqGroup { get; set; }
        /// <summary>
        /// 趟次
        /// </summary>
        public string TripTimes { get; set; }
        
        /// <summary>
        /// 状态（0未传,1已传,2重传）
        /// </summary>
        public Int16 Status { get; set; }

        /// <summary>
        /// 创建用户
        /// </summary>
        public string CreateUserName { get; set; }

        /// <summary>
        /// 是否剃退
        /// </summary>
        public Boolean IsReject { get; set; }

        /// <summary>
        /// 剃退单号
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
