using System;

namespace com.Sconit.Entity.FIS
{
    [Serializable]
    public partial class CreateOrderDAT : EntityBase
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// 物料号
        /// </summary>
        public string MATNR { get; set; }

        /// <summary>
        /// 指定供应商
        /// </summary>
        public string LIFNR { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public string ENMNG { get; set; }
        /// <summary>
        /// 整车Van号
        /// </summary>
        public string CHARG { get; set; }

        /// <summary>
        /// 外饰颜色
        /// </summary>
        public string COLOR { get; set; }
    
        /// <summary>
        /// 拉动时间
        /// </summary>
        public string TIME_STAMP { get; set; }

        /// <summary>
        /// 生产顺序
        /// </summary>
        public string CY_SEQNR { get; set; }

        /// <summary>
        /// 上传时间
        /// </summary>
        public string TIME_STAMP1 { get; set; }

    
        /// <summary>
        /// 生产订单号
        /// </summary>
        public string AUFNR { get; set; }

        /// <summary>
        /// 发货仓库
        /// </summary>
        public string LGORT { get; set; }

        /// <summary>
        /// 收货仓库
        /// </summary>
        public string UMLGO { get; set; }
        
        /// <summary>
        /// 目的工位
        /// </summary>
        public string LGPBE { get; set; }

        /// <summary>
        /// 需求时间
        /// </summary>
        public string REQ_TIME_STAMP { get; set; }

        /// <summary>
        /// 是否排序
        /// </summary>
        public string FLG_SORT { get; set; }
        
        /// <summary>
        /// 上级物料号
        /// </summary>
        public string PLNBEZ { get; set; }

        /// <summary>
        /// 分装线描述
        /// </summary>
        public string KTEXT { get; set; }

        /// <summary>
        /// 配单号
        /// </summary>
        public string ZPLISTNO { get; set; }

        /// <summary>
        /// 是否已创建EDI
        /// </summary>
        public Boolean IsCreateDat { get; set; }


        /// <summary>
        /// 创建用户
        /// </summary>
        public string CreateUserName { get; set; }

        /// <summary>
        /// 失败次数
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
