using System;
using System.Xml.Serialization;

namespace com.Sconit.Entity.SAP.ORD
{
    [Serializable]
    public partial class AlterDO : SAPEntityBase, ITraceable
    {
        #region O/R Mapping Properties

        [XmlIgnore]
        public Int32 Id { get; set; }
        /// <summary>
        /// LIKP-VBELN 交货单号
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// LIPS-POSNR 行项目号
        /// </summary>
        public Int32 Sequence { get; set; }
        /// <summary>
        /// LIPS-MATNR 物料号
        /// </summary>
        public string Item { get; set; }
        /// <summary>
        /// LIPS-WERKS 工厂
        /// </summary>
        public string Plant { get; set; }
        /// <summary>
        /// LIPS-LGORT 库存地点
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// LIPS-LFIMG 计划数量
        /// </summary>
        public Decimal Qty { get; set; }
        /// <summary>
        /// LIPS-VRKME 单位
        /// </summary>
        public string Uom { get; set; }
        /// <summary>
        ///售达方编码 KUNAG
        /// </summary>
        public string KUNAG { get; set; }
        /// <summary>
        /// 送达方编码 KUNNR
        /// </summary>
        public string KUNNR { get; set; }
        /// <summary>
        /// 变更标识 ZDEL
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// 计划交货时间
        /// </summary>
        public DateTime WindowTime { get; set; }
        /// <summary>
        /// 采购订单号码
        /// </summary>
        public string ExternalOrderNo { get; set; }

        [XmlIgnore]
        public StatusEnum Status { get; set; }
        [XmlIgnore]
        public DateTime CreateDate { get; set; }
        [XmlIgnore]
        public DateTime LastModifyDate { get; set; }
        [XmlIgnore]
        public Int32 ErrorCount { get; set; }

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
            AlterDO another = obj as AlterDO;

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
