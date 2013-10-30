
namespace com.Sconit.Entity
{
    public static class BusinessConstants
    {
        public static readonly int SYSTEM_USER_MONITOR = 2;
        public static readonly string BARCODE_HEAD_DEFAULT = "Default";
        public static readonly string BARCODE_SPECIAL_MARK = "$";
        public static readonly string BARCODE_HEAD_FLOW = "F";
        public static readonly string BARCODE_HEAD_FLOW_FACILITY = "f";
        public static readonly string BARCODE_HEAD_BIN = "B";
        public static readonly string BARCODE_HEAD_INSPECT = "I";
        public static readonly string BARCODE_HEAD_LOCATION = "L";
        public static readonly string BARCODE_HEAD_CONTAINER = "C";

        public static readonly string TEMPLATE_EXCEL_FILE_PATH = "Report/Templates/ExcelTemplates/";
        public static readonly string TEMP_FILE_PATH = "TempFiles/";

        public static readonly string NUMBERCONTROL_BILLOFLADINGNO = "ORD_BILLOFLADINGNO";

        public static readonly string FIS_RECALLTIMES = "FIS_RECALLTIMES";

        public static readonly string IO_TYPE_IN = "IN";
        public static readonly string IO_TYPE_OUT = "OUT";

        public static readonly string SHORT_DATE_FORMAT = "yyyy-MM-dd";
        public static readonly string LONG_DATE_FORMAT = "yyyy-MM-dd HH:mm";
        public static readonly string TELERIK_SHORT_DATE_FORMAT = "{0:yyyy-MM-dd}";
        public static readonly string TELERIK_LONG_DATE_FORMAT = "{0:yyyy-MM-dd HH:mm}";

        public static readonly string TABLEINDEX_MISCORDERMASTER = "MiscOrderMaster";               //计划外出入库
        public static readonly string TABLEINDEX_LOCATIONTRANSACTION = "LocationTransaction";       //库存事务
        public static readonly string TABLEINDEX_PRODUCTORDERBACKFLUSH = "ProductOrderBackflush";   //工单回冲
        public static readonly string TABLEINDEX_PRODUCTORDEROPREPORT = "ProductOrderOpReport";     //工单报工
        public static readonly string TABLEINDEX_CANCELPRODUCTORDEROPREPORT = "CancelProductOrderOpReport";//工单报工取消
        public static readonly string TABLEINDEX_VANORDER = "VanOrder";                             //整车序列
        public static readonly string TABLEINDEX_MI_LES2SAP_POST_DELIV_DOC = "MI_LES2SAP_POST_DELIV_DOC";

        public static readonly string NUMBERCONTROL_SISAP = "SiSap";
        public static readonly string NUMBERCONTROL_LOCTRANS = "L";
        public static readonly string NUMBERCONTROL_MISCORDER = "M";
        public static readonly string NUMBERCONTROL_BACKFLUSH = "B";
        public static readonly string NUMBERCONTROL_SISAPTRANSBATCHNO = "SiSapTransBatchNo";

        public static readonly int DATA_EXCHANGE_ORDER_NO_LENTH = 10;
    }
}
