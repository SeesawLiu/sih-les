using System;
using System.Xml.Serialization;

namespace com.Sconit.Entity.SAP.MD
{
    [Serializable]
    public partial class SAPItem : SAPEntityBase
    {
        #region O/R Mapping Properties
        [XmlIgnore]
        public Int32 Id { get; set; }
        /// <summary>
        /// 物料号
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 旧图号
        /// </summary>
        public string ReferenceCode { get; set; }
        /// <summary>
        /// 短号码
        /// </summary>
        public string ShortCode { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string Uom { get; set; }
        /// <summary>
        /// 工厂
        /// </summary>
        public string Plant { get; set; }
        /// <summary>
        /// MRP控制者
        /// </summary>
        public string DISPO { get; set; }
        /// <summary>
        /// 计划交货时间
        /// </summary>
        public string PLIFZ { get; set; }
        /// <summary>
        /// 采购类型
        /// </summary>
        public string BESKZ { get; set; }
        /// <summary>
        /// 特殊采购类型
        /// </summary>
        public string SOBSL { get; set; }

        [XmlIgnore]
        public Int32 BatchNo { get; set; }

        [XmlIgnore]
        public DateTime? CreateDate { get; set; }
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
            SAPItem another = obj as SAPItem;

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