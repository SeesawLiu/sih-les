
//TODO: Add other using statements here

using com.Sconit.Entity.SYS;
using System.ComponentModel.DataAnnotations;
namespace com.Sconit.Entity.INV
{
    public partial class LocationTransaction
    {
        #region Non O/R Mapping Properties

        [Export(ExportName = "ExportList", ExportSeq = 110)]
        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.TransactionIOType, ValueField = "IOType")]
        [Display(Name = "LocationTransaction_IoType", ResourceType = typeof(Resources.INV.LocationTransaction))]
        public string IOTypeDescription { get; set; }

        [Export(ExportName = "ExportList", ExportSeq = 10)]
        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.TransactionType, ValueField = "TransactionType")]
        [Display(Name = "LocationTransaction_TransactionType", ResourceType = typeof(Resources.INV.LocationTransaction))]
        public string TransactionTypeDescription { get; set; }

        [Export(ExportName = "ExportList", ExportSeq = 150)]
        [Display(Name = "LocationTransaction_CreateUserName", ResourceType = typeof(Resources.INV.LocationTransaction))]
        public string CreateUserName { get; set; }

        [Display(Name = "LocationTransaction_SapOrderNo", ResourceType = typeof(Resources.INV.LocationTransaction))]
        public string SapOrderNo { get; set; }

        [Display(Name = "LocationTransaction_Supplier", ResourceType = typeof(Resources.INV.LocationTransaction))]
        public string Supplier { get; set; }

        #region ±®±Ì”√

         [Display(Name = "LocationTransaction_Location", ResourceType = typeof(Resources.INV.LocationTransaction))]
        public string Location
        {
            get
            {
                return this.IOType == com.Sconit.CodeMaster.TransactionIOType.In ? this.LocationTo : this.LocationFrom;
            }
        }
        public decimal ProcurementInQty
        {
            get
            {
                decimal qty = 0;
                if (this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_PO
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_PO_VOID
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.ISS_PO
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.ISS_PO_VOID)
                {
                    qty = this.Qty;
                }
                return qty;
            }
        }

        public decimal DistributionOutQty
        {
            get
            {
                decimal qty = 0;
                if (this.TransactionType == com.Sconit.CodeMaster.TransactionType.ISS_SO
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.ISS_SO_VOID
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.ISS_SO_IGI
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.ISS_SO_IGI_VOID
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_SO
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_SO_VOID)
                {
                    qty = this.Qty;
                }
                return qty;
            }
        }

        public decimal TransferInQty
        {
            get
            {
                decimal qty = 0;
                if (this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_TR
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_TR_VOID
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_TR_RTN
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_TR_RTN_VOID
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_TR_IGI
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_TR_IGI_VOID
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_TR_RTN_IGI
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_TR_RTN_IGI_VOID
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_STR
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_STR_VOID
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_STR_RTN
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_STR_RTN_VOID
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_STR_IGI
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_STR_IGI_VOID
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_STR_RTN_IGI
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_STR_RTN_IGI_VOID)
                {
                    qty = this.Qty;
                }
                return qty;
            }
        }

        public decimal TransferOutQty
        {
            get
            {
                decimal qty = 0;
                if (this.TransactionType == com.Sconit.CodeMaster.TransactionType.ISS_TR
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.ISS_TR_VOID
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.ISS_TR_RTN
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.ISS_TR_RTN_VOID
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.ISS_STR
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.ISS_STR_VOID
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.ISS_STR_RTN
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.ISS_STR_RTN_VOID)
                {
                    qty = this.Qty;
                }
                return qty;
            }
        }

        public decimal ProductionInQty
        {
            get
            {
                decimal qty = 0;
                if (this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_WO
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_WO_VOID
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_SWO
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.RCT_SWO_VOID)
                {
                    qty = this.Qty;
                }
                return qty;
            }
        }

        public decimal ProductionOutQty
        {
            get
            {
                decimal qty = 0;
                if (this.TransactionType == com.Sconit.CodeMaster.TransactionType.ISS_WO
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.ISS_WO_VOID
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.ISS_WO_BF
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.ISS_WO_BF_VOID
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.ISS_SWO
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.ISS_SWO_VOID
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.ISS_SWO_BF
                    || this.TransactionType == com.Sconit.CodeMaster.TransactionType.ISS_SWO_BF_VOID)
                {
                    qty = this.Qty;
                }
                return qty;
            }
        }
        #endregion
        #endregion
    }
}