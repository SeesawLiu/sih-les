using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.PrintModel;
using System.ComponentModel;

namespace PrintClient
{
    [Serializable]
    public class SubscriberData
    {
        public string SubscribeType { get; set; }

        public string Flow { get; set; }

        public string Region { get; set; }

        public string UserName { get; set; }

        public string PrinterName { get; set; }

        public UInt16 IsPrinted { get; set; }

        public Boolean IsAutoPrint { get; set; }

        public string GUID { get; set; }
    }

    [Serializable]
    public class ClientData
    {
        public string No { get; set; }

        public string SubscribeType { get; set; }

        public string PartFrom { get; set; }

        public string PartTo { get; set; }

        public string UserName { get; set; }

        public string PrinterName { get; set; }

        public bool hasShowed { get; set; }

        public bool isPrinted { get; set; }

        public DateTime PublishDateTime { get; set; }

        public PrintBase PrintData { get; set; }

    }

    public enum DocumentsType
    {
        [Description("采购订单")]
        ORD_Procurement,
        [Description("移库单")]
        ORD_Transfer,
        [Description("发货单")]
        ORD_Distribution,
        [Description("生产订单")]
        ORD_Production,
        [Description("委外加工单")]
        ORD_SubContract,
        [Description("客供品订单")]
        ORD_CustomerGoods,
        [Description("送货单")]
        ASN,
        [Description("收货单")]
        REC,
        [Description("拣货单")]
        PIK,
        [Description("帐单")]
        BIL,
        [Description("红冲单")]
        RED,
        [Description("计划外出入库单")]
        MIS,
        [Description("检验单")]
        INS,
        [Description("不合格品处理单")]
        REJ,
        [Description("盘点单")]
        STT,
        [Description("排序单")]
        SEQ,
        [Description("克隆条码")]
        CloneHu
    }
}
