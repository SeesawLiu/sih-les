using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.FIS
{
    [Serializable]
    public partial class WMSDatFile : EntityBase
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }
        /// <summary>
        /// 移动类型
        /// </summary>
        [Export(ExportName = "ExportWMSDatFile", ExportSeq = 130)]
        [Display(Name = "WMSDatFile_MoveType", ResourceType = typeof(Resources.FIS.WMSDatFile))]
        public string MoveType { get; set; }

        /// <summary>
        /// 凭证日期
        /// </summary>
        public string BLDAT { get; set; }

        /// <summary>
        /// 过账日期
        /// </summary>
        public string BUDAT { get; set; }
        /// <summary>
        /// Po
        /// </summary>
        public string PO { get; set; }

        /// <summary>
        /// Po行号
        /// </summary>
        public string POLine { get; set; }

        /// <summary>
        /// DN号码
        /// </summary>
        public string VBELN { get; set; }

        /// <summary>
        /// DN行数
        /// </summary>
        public string POSNR { get; set; }

        /// <summary>
        /// 厂商代码
        /// </summary>
        public string LIFNR { get; set; }

        /// <summary>
        /// 工厂
        /// </summary>
        public string WERKS { get; set; }

        /// <summary>
        /// 库存地点
        /// </summary>
        [Export(ExportName = "ExportWMSDatFile", ExportSeq = 150)]
        public string LGORT { get; set; }

        /// <summary>
        /// 特殊库存标志
        /// </summary>
        public string SOBKZ { get; set; }

        /// <summary>
        /// 物料号码
        /// </summary>
        [Export(ExportName = "ExportWMSDatFile", ExportSeq = 10)]
        [Display(Name = "WMSDatFile_Item", ResourceType = typeof(Resources.FIS.WMSDatFile))]
        public string Item { get; set; }

        /// <summary>
        /// 数量 
        /// </summary>
        [Export(ExportName = "ExportWMSDatFile", ExportSeq = 60)]
        [Display(Name = "WMSDatFile_Qty", ResourceType = typeof(Resources.FIS.WMSDatFile))]
        public decimal Qty { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
         [Export(ExportName = "ExportWMSDatFile", ExportSeq = 40)]
         [Display(Name = "WMSDatFile_Uom", ResourceType = typeof(Resources.FIS.WMSDatFile))]
        public string Uom { get; set; }

        /// <summary>
        /// 收货地点
        /// </summary>
         [Export(ExportName = "ExportWMSDatFile", ExportSeq = 160)]
         [Display(Name = "WMSDatFile_UMLGO", ResourceType = typeof(Resources.FIS.WMSDatFile))]
        public string UMLGO { get; set; }

        /// <summary>
        /// 移动原因
        /// </summary>
        public string GRUND { get; set; }

        /// <summary>
        /// 成本中心
        /// </summary>
        public string KOSTL { get; set; }

        /// <summary>
        /// 出货通知  对应安吉拣货单号 
        /// </summary>
        [Export(ExportName="ExportWMSDatFile",ExportSeq=120)]
        [Display(Name = "WMSDatFile_WmsNo", ResourceType = typeof(Resources.FIS.WMSDatFile))]
        public string WmsNo { get; set; }

        /// <summary>
        /// 预留号码
        /// </summary>
        public string RSNUM { get; set; }

        /// <summary>
        /// 预留行数
        /// </summary>
        public string RSPOS { get; set; }

        /// <summary>
        /// WMS号码 安吉唯一标识
        /// </summary>
        [Export(ExportName = "ExportWMSDatFile", ExportSeq = 140)]
        [Display(Name = "WMSDatFile_WMSId", ResourceType = typeof(Resources.FIS.WMSDatFile))]
        public string WMSId { get; set; }

        /// <summary>
        /// WMS行数  对应我们的orderDetail ID
        /// </summary>
        public string WmsLine { get; set; }


        /// <summary>
        /// 操作类型
        /// </summary>
        public string OLD { get; set; }


        /// <summary>
        /// 库存类型
        /// </summary>
        public string INSMK { get; set; }

        /// <summary>
        ///送货单号
        /// </summary>
        public string XABLN { get; set; }

        /// <summary>
        /// 内部订单
        /// </summary>
        public string AUFNR { get; set; }


        /// <summary>
        /// 收货物料
        /// </summary>
        public string UMMAT { get; set; }


        /// <summary>
        /// 收货工厂
        /// </summary>
        public string UMWRK { get; set; }

        /// <summary>
        /// 是F否处理
        /// </summary>
        [Display(Name = "WMSDatFile_IsHand", ResourceType = typeof(Resources.FIS.WMSDatFile))]
        public Boolean IsHand { get; set; }


        /// <summary>
        /// 创建时间
        /// </summary>
        [Export(ExportName = "ExportWMSDatFile", ExportSeq = 150)]
        [Display(Name = "WMSDatFile_CreateDate", ResourceType = typeof(Resources.FIS.WMSDatFile))]
        public DateTime CreateDate { get; set; }

        public string FileName { get; set; }

        /// <summary>
        /// WBS元素
        /// </summary>
        public string WBS { get; set; }

        /// <summary>
        /// HuId
        /// </summary>
        public string HuId { get; set; }


        [Display(Name = "OrderMaster_Version", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public Int32 Version { get; set; }

        /// <summary>
        /// 已收数
        /// </summary>
        [Export(ExportName = "ExportWMSDatFile", ExportSeq = 70)]
        [Display(Name = "WMSDatFile_ReceiveTotal", ResourceType = typeof(Resources.FIS.WMSDatFile))]
        public decimal ReceiveTotal { get; set; }
        /// <summary>
        /// 冲销数
        /// </summary>
        [Export(ExportName = "ExportWMSDatFile", ExportSeq = 80)]
        [Display(Name = "WMSDatFile_CancelQty", ResourceType = typeof(Resources.FIS.WMSDatFile))]
        public decimal CancelQty { get; set; }
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
            WMSDatFile another = obj as WMSDatFile;

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
