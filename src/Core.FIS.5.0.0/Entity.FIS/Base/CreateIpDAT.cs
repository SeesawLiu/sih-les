using System;

namespace com.Sconit.Entity.FIS
{
    [Serializable]
    public partial class CreateIpDAT : EntityBase
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }
        /// <summary>
        /// 提前到货通知单
        /// </summary>
        public string ASN_NO { get; set; }

        /// <summary>
        /// 提前到货通知单行号
        /// </summary>
        public string ASN_ITEM { get; set; }

        /// <summary>
        /// 收货仓库
        /// </summary>
        public string WH_CODE { get; set; }
        /// <summary>
        /// 仓库收货道口
        /// </summary>
        public string WH_DOCK { get; set; }

        /// <summary>
        /// 零件仓库库位代码
        /// </summary>
        public string WH_LOCATION { get; set; }

        /// <summary>
        /// 零件编码
        /// </summary>
        public string ITEM_CODE { get; set; }

        /// <summary>
        /// 供应商代码
        /// </summary>
        public string SUPPLIER_CODE { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public string QTY { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        public string UOM { get; set; }

        /// <summary>
        /// 根据BASE UNIT转换的数量
        /// </summary>
        public string BASE_UNIT_QTY { get; set; }

        /// <summary>
        /// BASE UNIT单位
        /// </summary>
        public string BASE_UNIT_UOM { get; set; }

        /// <summary>
        /// 质量标识(Y:免检/N:非免检)
        /// </summary>
        public string QC_FLAG { get; set; }

        /// <summary>
        /// 预计到货日期
        /// </summary>
        public DateTime DELIVERY_DATE { get; set; }

        /// <summary>
        /// 到货时间窗
        /// </summary>
        public string TIME_WINDOW { get; set; }

        /// <summary>
        /// PO号
        /// </summary>
        public string PO { get; set; }

        /// <summary>
        /// 结算标识，(Y:寄宿/N:非寄宿)
        /// </summary>
        public string FINANCE_FLAG { get; set; }

        /// <summary>
        /// 是否物料总成标识，(Y:总成/N:非总成)
        /// </summary>
        public string COMPONENT_FLAG { get; set; }

        /// <summary>
        /// 运输车辆号
        /// </summary>
        public string TRACKID { get; set; }

        /// <summary>
        /// PO行号
        /// </summary>
        public string PO_LINE { get; set; }

        /// <summary>
        /// 工厂信息
        /// </summary>
        public string FactoryInfo { get; set; }

        /// <summary>
        /// Y 或者空 当为Y时需要特殊处理，见下文
        /// </summary>
        public string F80XBJ { get; set; }

        /// <summary>
        /// 是F80X件在0084工厂下的收货地点，用于做301操作的时候 中TO的地点
        /// </summary>
        public string F80X_LOCATION { get; set; }


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

        /// <summary>
        /// 上传时间
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
