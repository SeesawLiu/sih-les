using System;
using System.Security.AccessControl;

namespace com.Sconit.Entity.SAP.TRANS
{
    [Serializable]
    public partial class InvTrans : SAPEntityBase, ITraceable
    {
        #region O/R Mapping Properties
        /// <summary>
        /// TCODE
        /// 全部必须
        /// </summary>
        //[Display(Name = "Hu_HuId", ResourceType = typeof(Resources.INV.Hu))]
        public string TCODE { get; set; }
        /// <summary>
        /// 移动类型1
        /// 全部必须
        /// </summary>
        public string BWART { get; set; }
        /// <summary>
        /// 凭证日期
        /// 全部必须
        /// </summary>
        public string BLDAT { get; set; }
        /// <summary>
        /// 过账日期1
        /// 全部必须
        /// </summary>
        public string BUDAT { get; set; }
        /// <summary>
        /// PO号码1 采购单或计划协议号
        /// 101/102
        /// </summary>
        public string EBELN { get; set; }
        /// <summary>
        /// PO行号1 采购单或计划协议行号
        /// 101/102
        /// </summary>
        public string EBELP { get; set; }
        /// <summary>
        /// DN号码 无效
        /// </summary>
        public string VBELN { get; set; }
        /// <summary>
        /// DN行数 无效
        /// </summary>
        public string POSNR { get; set; }
        /// <summary>
        /// 厂商代码 1供应商
        /// 采购101/102/161/162/ZR1/ZR2/ZR5/ZR6 其他寄售 K
        /// </summary>
        public string LIFNR { get; set; }
        /// <summary>
        /// 工厂代码
        /// 全部必须
        /// </summary>
        public string WERKS { get; set; }
        /// <summary>
        /// 库存地点1 采购：目的库位代码 移库：来源库位代码
        /// 全部必须
        /// </summary>
        public string LGORT { get; set; }
        /// <summary>
        /// 特殊标志
        /// 寄售K/但采购寄售收货和冲销除外(!101/!102)
        /// </summary>
        public string SOBKZ { get; set; }
        /// <summary>
        /// 操作类型 无效 I/O
        /// </summary>
        public string OLD { get; set; }
        /// <summary>
        /// 物料号1
        /// 全部必须
        /// </summary>
        public string MATNR { get; set; }
        /// <summary>
        /// 数量
        /// 全部必须
        /// </summary>
        public Decimal ERFMG { get; set; }
        /// <summary>
        /// 单位
        /// 全部必须
        /// </summary>
        public string ERFME { get; set; }
        /// <summary>
        /// 收货地点 移库目的库位/质检
        /// 311/312/411/412/343/309/310/321/301/302/Z01/Z02/303/304/305/306
        /// </summary>
        public string UMLGO { get; set; }
        /// <summary>
        /// 移动原因 采购和冲销
        /// 101/102/551/552
        /// </summary>
        public string GRUND { get; set; }
        /// <summary>
        /// 成本中心 质量部破坏性抽检领用
        /// Z05/Z06/Z76/Z75
        /// </summary>
        public string KOSTL { get; set; }
        /// <summary>
        /// 出货通知1 回执对账用
        /// 采购/寄售
        /// </summary>
        public string XBLNR { get; set; }
        /// <summary>
        /// 预留号1 部门领用物料-配件
        /// 201/202
        /// </summary>
        public string RSNUM { get; set; }
        /// <summary>
        /// 预留行号 1部门领用物料-配件
        /// 201/202
        /// </summary>
        public string RSPOS { get; set; }
        /// <summary>
        /// WMS号1
        /// 必填
        /// </summary>
        public string FRBNR { get; set; }
        /// <summary>
        /// WMS行号1
        /// 必填
        /// </summary>
        public string SGTXT { get; set; }
        /// <summary>
        /// 库存类型 3：冻结 2：质检 空：非限制使用 LES只会用到3和空
        /// 101/102/161/162
        /// </summary>
        public string INSMK { get; set; }
        /// <summary>
        /// 送货单号1 ASN
        /// 101/102
        /// </summary>
        public string XABLN { get; set; }
        /// <summary>
        /// 内部订单 计划外出入库 有内部订单号的部门领用（技术中心计划外领用）
        /// Z03/Z04
        /// </summary>
        public string AUFNR { get; set; }
        /// <summary>
        /// 收货物料 物料替换
        /// 309/310
        /// </summary>
        public string UMMAT { get; set; }
        /// <summary>
        /// 收货工厂 跨工厂移库
        /// 301/302/303/304/305/306
        /// </summary>
        public string UMWRK { get; set; }
        /// <summary>
        /// WBS元素 计划外出入库 有内部订单号的部门领用（技术中心计划外领用）
        /// Z03/Z04
        /// </summary>
        public string POSID { get; set; }

        /// <summary>
        /// 生产收货最终确认标识
        /// </summary>
        public string KZEAR { get; set; }

        /// <summary>
        /// SAP批次号
        /// </summary>
        public string CHARG { get; set; }

        public StatusEnum Status { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastModifyDate { get; set; }
        public Int32 ErrorCount { get; set; }
        public ErrorIdEnum ErrorId { get; set; }

        public Int32 BatchNo { get; set; }
        public string ErrorMessage { get; set; }

        public string OrderNo { get; set; }
        public Int32 DetailId { get; set; }
        public Int32 Version { get; set; }
        #endregion

        public override int GetHashCode()
        {
            if (FRBNR != null && SGTXT != null)
            {
                return FRBNR.GetHashCode() ^ SGTXT.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            InvTrans another = obj as InvTrans;

            if (another == null)
            {
                return false;
            }
            else
            {
                return (this.FRBNR == another.FRBNR) && (this.SGTXT == another.SGTXT);
            }
        }
    }

}
