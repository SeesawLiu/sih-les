using System.Runtime.Serialization;

namespace com.Sconit.CodeMaster
{

    public enum PermissionCategoryType
    {
        Url = 1,
        Region = 2,
        Customer = 3,
        Supplier = 4,
        Terminal = 5
    }

    public enum BillMasterStatus
    {
        Create = 0,
        Submit = 1,
        Close = 2,
        Cancel = 3
    }
    public enum RoleType
    {
        Normal = 0,
        System = 1
    }

    public enum UserType
    {
        Normal = 0,
        System = 1
    }

    public enum BillType
    {
        Procurement = 0,
        Distribution = 1
    }

    public enum BillTransactionType
    {
        POSettle = 0,
        POSettleVoid = 1,
        SOSettle = 2,
        SOSettleVoid = 3,
        POBilling = 4,
        SOBilling = 5
    }

    public enum PriceListType
    {
        Procuement = 1,
        Distribution = 2
    }

    public enum InspectType
    {
        Quantity = 1,
        Barcode = 2
    }

    public enum InspectStatus
    {
        Submit = 0,
        InProcess = 1,
        Close = 2
    }


    public enum JudgeResult
    {
        Qualified = 0,
        Rejected = 1
    }

    [DataContract]
    public enum OrderType
    {
        [EnumMember]
        Procurement = 1,
        [EnumMember]
        Transfer = 2,
        [EnumMember]
        Distribution = 3,
        [EnumMember]
        Production = 4,
        [EnumMember]
        SubContract = 5,
        [EnumMember]
        CustomerGoods = 6,
        [EnumMember]
        SubContractTransfer = 7,
        [EnumMember]
        ScheduleLine = 8,
        [EnumMember]
        OnlySupplier = 9
    }


    [DataContract]
    public enum OrderPriority
    {
        [EnumMember]
        Normal = 0,
        [EnumMember]
        Urgent = 1
    }

    [DataContract]
    public enum LocationLevel
    {
        [EnumMember]
        Donotcollect = 0,//不汇总
        [EnumMember]
        Factory = 3,  //工厂
        [EnumMember]
        Region = 1,  //区域
        [EnumMember]
        SinglePlant = 2  //分厂

    }

    public enum SendStatus
    {
        NotSend = 0,
        Success = 1,
        Fail = 2
    }

    [DataContract]
    public enum OrderStatus
    {
        [EnumMember]
        Create = 0,
        [EnumMember]
        Submit = 1,
        [EnumMember]
        InProcess = 2,
        [EnumMember]
        Complete = 3,
        [EnumMember]
        Close = 4,
        [EnumMember]
        Cancel = 5
    }

    [DataContract]
    public enum ReceiveGapTo
    {
        [EnumMember]
        AdjectLocFromInv = 0,      //调整发货方库存
        [EnumMember]
        RecordIpGap = 1           //记录收货差异
    }

    [DataContract]
    public enum CreateHuOption
    {
        //NoCreate = 0,  //不创建
        //Release = 1,   //释放
        //Start =2,      //上线
        //Ship = 3,      //发货
        //Receive = 4    //收货

        [EnumMember]
        None = 0,  //不创建
        //Submit = 1,  //释放创建
        //Start = 2,   //上线创建
        [EnumMember]
        Ship = 3, //发货创建
        [EnumMember]
        Receive = 4   //收货创建
    }

    [DataContract]
    public enum ReCalculatePriceOption
    {
        [EnumMember]
        Manual = 0,  //手工
        [EnumMember]
        Submit = 1,   //释放
        [EnumMember]
        Start = 2,      //上线
        [EnumMember]
        Ship = 3,      //发货
        [EnumMember]
        Receive = 4    //收货
    }

    [DataContract]
    public enum OrderBillTerm
    {
        [EnumMember]
        NA = 0,                    //空
        [EnumMember]
        ReceivingSettlement = 1,   //收货结算
        [EnumMember]
        AfterInspection = 2,       //检验结算
        [EnumMember]
        OnlineBilling = 3,         //上线结算
        [EnumMember]
        LinearClearing = 4,        //下线结算
        [EnumMember]
        ConsignmentBilling = 5     //寄售结算
    }

    [DataContract]
    public enum OrderSubType
    {
        [EnumMember]
        Normal = 0,
        [EnumMember]
        Return = 1  //退货、返工////    采购、销售、移库、生产、客供、委外
        //Reject = 2,  //不合格品退货    采购、销售、移库、生产、客供、委外
        //Scrap = 2,   //原材料报废
        //Reuse = 3,    //原材料回用
        //Void = 9  //冲销 采购、销售、移库、生产、客供、委外
    }

    public enum AddressType
    {
        ShipAddress = 0,
        BillAddress = 1
    }

    public enum InventoryType
    {
        Quality = 0,  //按数量
        Barcode = 1   //按条码
    }

    //public enum ItemType
    //{
    //    Purchase = 1,
    //    Sales = 2,
    //    Manufacture = 3,
    //    SubContract = 4,
    //    CustomerGoods = 5,
    //    Virtual = 6,
    //    Kit = 7
    //}

    //public enum PartyType
    //{
    //    Region = 0,
    //    Supplier = 1,
    //    Customer = 2
    //}

    public enum WorkingCalendarType
    {
        Work = 0,
        Rest = 1
    }

    public enum WorkingCalendarCategory
    {
        Region = 0,
        ProdLine = 1
    }
    //public enum SpecialTimeType
    //{
    //    Work = 0,
    //    Rest = 1
    //}

    public enum RejectStatus
    {
        Create = 0,
        Submit = 1,
        InProcess = 2,
        Close = 3
    }

    [DataContract]
    public enum QualityType  //库存质量状态
    {
        [EnumMember]
        Qualified = 0,   //正常
        [EnumMember]
        Inspect = 1,  //待验
        [EnumMember]
        Reject = 2,    //不合格品
    }

    [DataContract]
    public enum CheckConsignment  //库存质量状态
    {
        [EnumMember]
        Normal = 0,   //指定寄售
        [EnumMember]
        Consignment = 1,  //指定非寄售
        [EnumMember]
        None = 2,    //手工指定
    }

    public enum OccupyType  //库存质量状态
    {
        None = 0,   //Not Occupy
        Pick = 1,   //拣货
        Inspect = 2,//检验
        Sequence = 3,//排序
        MiscOrder = 4,//计划外出入库
        //AutoPick = 5//系统拣货，只占用库存，没有拣货单
    }


    public enum TimeUnit
    {
        Second = 1,
        Minute = 2,
        Hour = 3,
        Day = 4,
        Week = 5,
        Month = 6,
        Quarter = 7,
        Year = 8
    }

    public enum WindowTimeType
    {
        FixedWindowTime = 0,
        CycledWindowTime = 1,
    }

    public enum StockTakeType
    {
        Part = 0,
        All = 1
    }

    public enum StockTakeStatus
    {
        Create = 0,
        Submit = 1,
        InProcess = 2,
        Complete = 3,
        Close = 4,
        Cancel = 5
    }

    public enum IssuePriority
    {
        Normal = 0,
        Urgent = 1
    }
    public enum IssueStatus
    {
        Create = 0,
        Submit = 1,
        InProcess = 2,
        Complete = 3,
        Close = 4,
        Cancel = 5
    }
    public enum IssueType
    {
        Issue = 0,
        Improvement = 1,
        Changepoint = 2
    }


    public enum TransactionType
    {
        LOC_INI = 99,          //库存初始化
        LOC_INI_VOID = 100,          //库存初始化冲销
        ISS_SO = 101,           //销售出库
        ISS_SO_VOID = 102,      //销售出库冲销
        RCT_SO = 103,           //销售退货入库
        RCT_SO_VOID = 104,      //销售退货入库冲销     
        ISS_SO_IGI = 105,       //销售差异调整至发货方
        ISS_SO_IGI_VOID = 106,  //销售差异调整至发货方冲销
        RCT_PO = 201,           //采购入库
        RCT_PO_VOID = 202,      //采购入库冲销
        ISS_PO = 203,           //采购退货
        ISS_PO_VOID = 204,      //采购退货冲销
        RCT_SL = 205,           //计划协议入库
        RCT_SL_VOID = 206,      //计划协议入库冲销
        ISS_SL = 207,           //计划协议退货
        ISS_SL_VOID = 208,      //计划协议退货冲销
        ISS_TR = 301,           //移库出库
        ISS_TR_VOID = 302,      //移库出库冲销
        RCT_TR = 303,           //移库入库
        RCT_TR_VOID = 304,      //移库入库冲销
        ISS_STR = 305,          //委外移库出库
        ISS_STR_VOID = 306,     //委外移库出库冲销
        RCT_STR = 307,          //委外移库入库
        RCT_STR_VOID = 308,     //委外移库入库冲销
        ISS_TR_RTN = 311,       //移库退货出库
        ISS_TR_RTN_VOID = 312,  //移库退货出库冲销
        RCT_TR_RTN = 313,       //移库退货入库
        RCT_TR_RTN_VOID = 314,  //移库退货入库冲销
        ISS_STR_RTN = 315,      //委外移库退货出库
        ISS_STR_RTN_VOID = 316, //委外移库退货出库冲销
        RCT_STR_RTN = 317,      //委外移库退货入库
        RCT_STR_RTN_VOID = 318, //委外移库退货入库冲销
        RCT_TR_IGI = 319,       //移库差异调整至发货方
        RCT_TR_IGI_VOID = 320,  //移库差异调整至发货方冲销
        RCT_TR_RTN_IGI = 321,   //移库退货差异调整至发货方
        RCT_TR_RTN_IGI_VOID = 322, //移库退货差异调整至发货方冲销
        RCT_STR_IGI = 323,      //委外移库差异调整至发货方
        RCT_STR_IGI_VOID = 324, //委外移库差异调整至发货方冲销
        RCT_STR_RTN_IGI = 325,  //委外移库退货差异调整至发货方
        RCT_STR_RTN_IGI_VOID = 326, //委外移库退货差异调整至发货方冲销
        ISS_WO = 401,           //生产出库/原材料
        ISS_WO_VOID = 402,      //生产出库/原材料冲销
        ISS_WO_BF = 403,        //生产投料回冲出库/出生产线
        ISS_WO_BF_VOID = 404,   //生产投料回冲出库/出生产线冲销
        RCT_WO = 405,           //生产入库/成品
        RCT_WO_VOID = 406,      //生产入库/成品冲销
        ISS_MIN = 407,          //生产投料出库
        ISS_MIN_RTN = 408,      //生产投料退库出库
        RCT_MIN = 409,          //生产投料入库/入生产线
        RCT_MIN_RTN = 410,      //生产投料出库/出生产线
        ISS_SWO = 411,          //委外生产出库/原材料
        ISS_SWO_VOID = 412,     //委外生产出库/原材料冲销
        ISS_SWO_BF = 413,       //委外生产投料回冲出库/出生产线
        ISS_SWO_BF_VOID = 414,  //委外生产投料回冲出库/出生产线冲销
        RCT_SWO = 415,          //委外生产入库/成品
        RCT_SWO_VOID = 416,     //委外生产入库/成品冲销
        ISS_INP = 501,          //报验出库
        RCT_INP = 502,          //报验入库
        ISS_ISL = 503,          //隔离出库
        RCT_ISL = 504,          //隔离入库
        ISS_INP_QDII = 505,     //检验合格出库 
        RCT_INP_QDII = 506,     //检验合格入库 
        ISS_INP_REJ = 507,      //检验不合格出库 
        RCT_INP_REJ = 508,      //检验不合格入库 
        ISS_INP_CCS = 509,      //让步使用出库
        RCT_INP_CCS = 510,      //让步使用入库
        CYC_CNT = 601,          //盘点差异出入库
        CYC_CNT_VOID = 602,     //盘点差异出入库
        ISS_UNP = 603,          //计划外出库
        ISS_UNP_VOID = 604,     //计划外出库冲销
        RCT_UNP = 605,          //计划外入库
        RCT_UNP_VOID = 606,     //计划外入库冲销
        ISS_REP = 607,          //翻箱出库
        RCT_REP = 608,          //翻箱入库
        ISS_PUT = 609,          //上架出库
        RCT_PUT = 610,          //上架入库
        ISS_PIK = 611,          //下架出库
        RCT_PIK = 612,          //下架入库
        ISS_IIC = 613,          //库存物料替换出库
        ISS_IIC_VOID = 614,     //库存物料替换出库冲销
        RCT_IIC = 615,          //库存物料替换入库
        RCT_IIC_VOID = 616,     //库存物料替换入库冲销
    }

    public enum HandleResult
    {
        Return = 0,   //退货
        Concession = 1,  //让步
        SelfmadeMaterialWaste = 2,//自制料废
        //   Rework = 2,   //返工
        //Scrap = 3,    //报废
        //Disassembly = 4   //拆装回用
        WorkersWaste = 5,  //工废
        // MaterialWaste=6  //料废
    }

    public enum BarCodeType
    {
        ORD,
        ASN,
        PIK,
        STT,
        INS,
        SEQ,
        MIS,
        /// <summary>
        /// 生产线实例
        /// </summary>
        Z,
        /// <summary>
        /// 生产线
        /// </summary>
        F,
        /// <summary>
        /// 库格
        /// </summary>
        B,
        /// <summary>
        /// 检验
        /// </summary>
        I,
        /// <summary>
        /// 库位
        /// </summary>
        L,
        /// <summary>
        /// 周转箱
        /// </summary>
        C,
        /// <summary>
        /// 看板
        /// </summary>
        K,
        /// <summary>
        /// WMS送货单号
        /// </summary>
        W,
        /// <summary>
        /// WMS排序送货单号
        /// </summary>
        SP,
        HU,
        DATE,
    }

    public enum ModuleType
    {
        Client_Receive,
        Client_PickListOnline,
        Client_PickList,
        Client_PickListShip,
        Client_OrderShip,
        Client_SeqPack,

        Client_Transfer,
        Client_RePack,
        Client_Pack,
        Client_UnPack,
        Client_PutAway,
        Client_Pickup,
        Client_StockTaking,
        Client_HuStatus,
        Client_MiscIn,
        Client_MiscOut,

        Client_Inspect,
        Client_Qualify,
        Client_Reject,

        Client_AnDon,
        Client_ProductionOnline,   //生产上线
        Client_MaterialIn,         //生产投料
        Client_ProductionOffline,      //生产下线
        Client_ChassisOnline,   //底盘上线
        Client_CabOnline,   //驾驶室上线
        Client_AssemblyOnline,  //总装上线
        Client_MaterialReturn,         //生产投料

        //周转箱
        Client_EmptyContainerShip,//空箱发货
        Client_EmptyContainerReceive,//空箱收货
        Client_FullContainerReceive,//满箱收货

        M_Switch
    }

    public enum IpType
    {
        Normal = 0,
        SEQ = 1,
        KIT = 2,
    }

    public enum IpDetailType
    {
        Normal = 0,
        Gap = 1
    }
    public enum IpStatus
    {
        Submit = 0,
        InProcess = 1,
        Close = 2,
        Cancel = 3
    }

    public enum PickListStatus
    {
        //Create = 0,
        Submit = 0,
        InProcess = 1,
        Close = 2,
        Cancel = 3
    }

    public enum BomStructureType
    {
        Normal = 0,   //普通结构
        Virtual = 1   //虚结构
    }

    public enum FeedMethod
    {
        None = 0,             //不投料
        ProductionOrder = 1,  //投料至工单成品
        ProductionLine = 2    //投料至生产线
    }

    public enum BackFlushMethod
    {
        GoodsReceive = 0,   //收货回冲，按标准BOM线冲生产线在制品，不够冲线边
        BackFlushOrder = 1, //只回冲工单在制品
        WeightAverage = 2   //加权平均
    }


    public enum BindType
    {
        Submit = 1,	//提交
        Start = 2	//上线
    }

    public enum RoundUpOption
    {
        ToUp = 1,    //向上圆整
        None = 0,    //不圆整
        ToDown = 2  //向下圆整
    }

    public enum MRPOption
    {
        OrderBeforePlan = 0,   //订单优先
        OrderOnly = 1,         //只看订单
        PlanAddOrder = 2,        //订单计划一起考虑
        PlanMinusOrder = 3,     //订单减掉计划
        PlanOnly = 4,             //只看计划
        SafeStockOnly = 5         //只看安全库存
    }

    [DataContract]
    public enum FlowStrategy
    {
        [EnumMember]
        NA = 0,
        [EnumMember]
        Manual = 1,
        [EnumMember]
        KB = 2,
        [EnumMember]
        JIT = 3,
        [EnumMember]
        SEQ = 4,
        [EnumMember]
        ANDON = 7,
        [EnumMember]
        TRIAL = 8,     //试制
        [EnumMember]
        SPARTPART = 9,  //备件
        [EnumMember]
        REPAIR = 10,     //返修
        [EnumMember]
        CKD = 11,    //CKD
        [EnumMember]
        Semi = 12,      //自制件
    }

    //public enum SpecialValue
    //{
    //    BlankValue,
    //    AllValue
    //}

    public enum DocumentsType
    {
        ORD_Procurement = 1001,
        ORD_Transfer = 1002,
        ORD_Distribution = 1003,
        ORD_Production = 1004,
        ORD_SubContract = 1005,
        ORD_CustomerGoods = 1006,
        ORD_SubContractTransfer = 1007,
        ORD_ScheduleLine = 1008,
        ASN_Procurement = 1101,
        ASN_Transfer = 1102,
        ASN_Distribution = 1103,
        ASN_SubContract = 1105,
        ASN_CustomerGoods = 1106,
        ASN_SubContractTransfer = 1107,
        ASN_ScheduleLine = 1108,
        REC_Procurement = 1201,
        REC_Transfer = 1202,
        REC_Distribution = 1203,
        REC_Production = 1204,
        REC_SubContract = 1205,
        REC_CustomerGoods = 1206,
        REC_SubContractTransfer = 1207,
        REC_ScheduleLine = 1208,
        PIK_Transfer = 1302,
        PIK_Distribution = 1303,
        BIL_Procurement = 1401,
        BIL_Distribution = 1403,
        RED_Procurement = 1411,
        RED_Distribution = 1413,
        MIS_Out = 1501,
        MIS_In = 1502,
        INS = 1601,
        REJ = 1611,
        STT = 1701,
        SEQ = 1801,
        CON = 1901,
        INV_Hu = 2001,
        VEH = 2011,
    }

    public enum LikeMatchMode
    {
        Anywhere,
        Start,
        End,
    }
    public enum HuStatus
    {
        NA = 0,
        Location = 1,
        Ip = 2,
    }

    public enum CodeMasterType
    {
        System = 0,
        Editable = 1
    }

    public enum PickOddOption
    {
        OddFirst = 0,     //零头先发
        NotPick = 1       //零头不发
    }

    public enum ShipStrategy
    {
        FIFO = 0,         //先进先出
        LIFO = 1          //后进先出
    }

    public enum CodeMaster
    {
        PermissionCategoryType,
        RoleType,
        UserType,
        BillType,
        BillTransactionType,
        PriceListType,
        InspectType,
        InspectStatus,
        JudgeResult,
        OrderType,
        OrderSubType,
        BillTerm,
        ReceiveGapTo,
        CreateHuOption,
        ReCalculatePriceOption,
        OrderPriority,
        OrderStatus,
        SendStatus,
        AddressType,
        InventoryType,
        //ItemType,
        WorkingCalendarType,
        WorkingCalendarCategory,
        RejectStatus,
        QualityType,
        OccupyType,
        TimeUnit,
        HandleResult,
        StockTakeType,
        StockTakeStatus,
        IssueType,
        IssuePriority,
        IssueStatus,
        IpType,
        IpDetailType,
        IpStatus,
        PickListStatus,
        BomStructureType,
        FeedMethod,
        BackFlushMethod,
        BindType,
        RoundUpOption,
        MRPOption,
        FlowStrategy,
        DocumentsType,
        HuStatus,
        LocationType,
        Language,
        LogLevel,
        DayOfWeek,
        HuTemplate,
        OrderTemplate,
        AsnTemplate,
        ReceiptTemplate,
        //FlowType,
        Strategy,
        InspectDefect,
        RoutingTimeUnit,
        BackFlushInShortHandle,
        SMSEventHeadler,
        SequenceStatus,
        ReceiptStatus,
        ConcessionStatus,
        MiscOrderType,
        MiscOrderStatus,
        PickOddOption,
        ShipStrategy,
        OrderBillTerm,
        CodeMasterType,
        MessageType,
        RePackType,
        JobRunStatus,
        TriggerStatus,
        MessagePriority,
        MessageStatus,
        EmailStatus,
        ScheduleType,
        TransactionIOType,
        TransactionType,
        IpGapAdjustOption,
        VehicleInFactoryStatus,
        LocationLevel,
        WindowTimeType,
        BillMasterStatus,
        PlusMoveType,
        MinusMoveType,
        CheckConsignment,
        ProdLineType,
        PauseStatus,
        ProductLineMapType,
        SequenceGroupType,
        CabType,
        CabOutStatus,
        OrderOpReportStatus,
        SapOrderType,
        DateOption,
        KBCalculation,
        KBStatus,
        KBTransType,
        KBProcessCode,
        OrderPauseType,
        SupplierOrderStatus,
    }

    public enum MessageType
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }

    public enum SequenceStatus
    {
        Submit = 0,
        Pack = 1,
        Ship = 2,
        Close = 3,
        Cancel = 4
    }

    public enum ReceiptStatus
    {
        Close = 0,
        Cancel = 1
    }

    public enum OrderOpReportStatus
    {
        Close = 0,
        Cancel = 1
    }

    public enum RePackType
    {
        In = 0,
        Out = 1
    }

    public enum TransactionIOType
    {
        In = 0,
        Out = 1
    }
    //public enum DocumentTypeEnum
    //{
    //    ORD,
    //    ASN,
    //    PIK,
    //    STT,
    //    INS
    //}

    public enum ConcessionStatus
    {
        Create = 0,
        Submit = 1,
        Close = 2
    }

    public enum JobRunStatus
    {
        InProcess = 0,
        Success = 1,
        Failure = 2,
    }

    public enum TriggerStatus
    {
        Open = 0,
        Close = 1,
    }

    public enum MessagePriority
    {
        Normal = 0,
        Low = 1,
        High = 2,
    }

    public enum MessageStatus
    {
        Open = 0,
        Close = 1
    }

    public enum EmailStatus
    {
        Open = 0,
        Close = 1
    }

    public enum MiscOrderType
    {
        GI = 0,
        GR = 1,
        Return = 2
    }

    public enum MiscOrderStatus
    {
        Create = 0,
        Close = 1,
        Cancel = 2
    }

    public enum ScheduleType
    {
        Firm = 0,
        Forecast = 1,
        BackLog = 2
    }

    public enum IpGapAdjustOption
    {
        GI = 0,
        GR = 1
    }

    public enum VehicleInFactoryStatus
    {
        Submit = 0,
        InProcess = 1,
        Close = 2
    }

    public enum GridDisplayType
    {
        Summary = 0,
        Detail = 1
    }

    public enum MQStatusEnum
    {
        Pending = 0,
        Success = 1,
        Fail = 2,
        Cancel = 3
    }

    public enum ProdLineType
    {
        Semi = 0,      //自制件生产线
        Cab = 1,       //驾驶室生产线
        Chassis = 2,   //底盘生产线
        Assembly = 3,  //总装生产线
        Special = 4,    //特装生产线
        SparePart = 5,  //备件
        Repair = 6,     //返修
        Trial = 7,     //试制
        Subassembly = 8, //分装生产线
        Check = 9,    //检测生产线
        CKD = 10    //CKD
    }

    public enum PauseStatus
    {
        None = 0,
        PlanPause = 1,
        Paused = 2,
    }

    public enum ProductLineMapType
    {
        NotVan = 0,  //非整车
        Van = 1    //整车
    }

    public enum SequenceGroupType
    {
        Factory = 0,
        OutsideFactory = 1
    }

    public enum PickDemandType
    {
        Purchase = 0 // 要货单
    }

    public enum CabType
    {
        SelfMade = 0,  //自制
        Purchase = 1   //外购
    }

    public enum CabOutStatus
    {
        NotOut = 0,  //未出库
        Out = 1,     //已出库
        Transfer = 2   //已移库
    }

    public enum SapOrderType
    {
        Z901,
        Z902,
        Z903,
        Z90R,
        ZP01
    }

    public enum DateOption
    {
        /// <summary>
        /// 等于
        /// </summary>
        EQ,

        /// <summary>
        /// 大于
        /// </summary>
        GT,

        /// <summary>
        /// 大于等于
        /// </summary>
        GE,

        /// <summary>
        /// 小于
        /// </summary>
        LT,

        /// <summary>
        /// 小于等于
        /// </summary>
        LE,

        /// <summary>
        /// 大于等于且小于等于
        /// </summary>
        BT,
    }

    [DataContract]
    public enum KBStatus
    {
        [EnumMember]
        Initial = 100, //初始状态,刚刚创建出来，还未打印，打印后转入loop，以后都不会回到Initial状态
        [EnumMember]
        Loop = 0, //可循环,也就是看板被打印后，实体初始状态
        [EnumMember]
        Scanned = 1, //已扫描成功
        [EnumMember]
        Ordered = 2 //已结转成功
        //[EnumMember]
        //Frozen = 3 //已冻结(已删除)
    }

    [DataContract]
    public enum KBTransType
    {
        [EnumMember]
        Initialize = 0, //创建
        [EnumMember]
        Print = 51, //打印

        [EnumMember]
        Submit = 11, //投出
        [EnumMember]
        UnSubmit = 12, //-投出

        [EnumMember]
        Scan = 21, //扫描
        [EnumMember]
        UnScan = 22, //-扫描
        [EnumMember]
        ModifyScan = 23, //修改扫描记录
        [EnumMember]
        DeleteScan = 24, //删除扫描记录

        [EnumMember]
        Order = 31, //结转
        [EnumMember]
        UnOrder = 32, //-结转

        [EnumMember]
        Freeze = 41, //冻结(删除)
        [EnumMember]
        UnFreeze = 42//-冻结(删除)
    }

    [DataContract]
    public enum KBProcessCode
    {
        [EnumMember]
        Ok = 0,
        [EnumMember]
        NotEffective = 1, // 未投出,red
        [EnumMember]
        AlreadyScanned = 2, // 已扫描成功,red
        [EnumMember]
        Frozen = 3, // 已冻结,red
        [EnumMember]
        PassOrderTime = 4, // 已过可结转时段,red
        [EnumMember]
        ExceedCycleAmount = 5, // 已超循环量,red
        [EnumMember]
        NonExistingCard = 6, // 没有这张卡,red
        [EnumMember]
        NotInScannedStatus = 7, // 没有扫描,red
        [EnumMember]
        DeleteScanFail = 8, // 删除扫描失败,red
        [EnumMember]
        NoFlowDetail = 9, // 找不到路线明细,red
        [EnumMember]
        OverOneFlowDetail = 10, // 找到多于一条路线明细,red
        [EnumMember]
        NoFlowMaster = 11, // 找不到路线,red
        [EnumMember]
        NoFlowStrategy = 12, // 找不到路线策略,red
        [EnumMember]
        OrderTransferError = 13, // 路线转订单错误,red
        [EnumMember]
        NoWorkingCalendar = 14, // 找不到工作日历
        [EnumMember]
        CreateOrderFailed = 15, // 创建订单失败
        [EnumMember]
        NoFlowShiftDetail = 16, //找不到交货时段
        [EnumMember]
        NoStartOrEndWorkingCalendar = 17, // 找不到开始或结束日期的工作日历
        [EnumMember]
        InvalidFields = 18, // 看板中带过来的字段发生了变化
        [EnumMember]
        RegionKBCodeNotSet = 19, // 区域未设置kb标准代码和异常代码
        [EnumMember]
        FlowDetailUnitCountNotSet = 20, // 路线明细包装为0

        [EnumMember]
        CalcNeedFreeze = 90, //
        [EnumMember]
        CalcNeedAdd = 91, // 
        [EnumMember]
        CalcNeedChangeEff = 92, // 
        [EnumMember]
        CalcNeedChangeFreeze = 93, //
        [EnumMember]
        CalcNeedChangeEffAndFreeze = 94, // 
        [EnumMember]
        CalcNonWorkableNeedFreeze = 95,
        [EnumMember]
        CalcNonWorkableNeedChangeFreeze = 96,
        [EnumMember]
        CalcNotInPlanNeedFreeze = 99
    }

    public enum KBCalculation
    {
        Normal = 0, // 物流
        CatItem = 1 // 生产
    }

    public enum OrderPauseType
    {
        NormalPause = 0,//正常暂停
        OperationPause = 1,//工位暂停
        MovePause = 2,//移动暂停
        Resume = 3//恢复
    }


    [DataContract]
    public enum SupplierOrderStatus
    {
        
        [EnumMember]
        Submit = 1,
        [EnumMember]
        InProcess = 2,
        [EnumMember]
        Close = 4,
        [EnumMember]
        Cancel = 5
    }
}
