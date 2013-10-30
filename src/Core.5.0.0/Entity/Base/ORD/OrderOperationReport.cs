using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.CodeMaster;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.ORD
{
    [Serializable]
    public partial class OrderOperationReport : EntityBase, IAuditable
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }
        public string OrderNo { get; set; }
        public Int32 OrderDetailId { get; set; }
        public Int32 OrderOperationId { get; set; }
        public Int32 Operation { get; set; }

        [Export(ExportName = "ExportDetailXLS", ExportSeq = 40)]
        [Export(ExportName = "SearchExportXLS", ExportSeq = 30)]
        [Display(Name = "OrderOperation_ReportQty", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public Decimal ReportQty { get; set; }

        [Display(Name = "OrderOperation_ScrapQty", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public Decimal ScrapQty { get; set; }

        public Decimal BackflushQty { get; set; }
        [Export(ExportName = "ExportDetailXLS", ExportSeq = 20)]
        [Export(ExportName = "SearchExportXLS", ExportSeq = 20)]
        [Display(Name = "OrderOperation_WorkCenter", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public string WorkCenter { get; set; }
        public OrderOpReportStatus Status { get; set; }
        [Export(ExportName = "ExportDetailXLS", ExportSeq = 30)]
        [Display(Name = "OrderOperationReport_CreateDate", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public DateTime EffectiveDate { get; set; }
        public string ReceiptNo { get; set; }
        public Int32 CreateUserId { get; set; }

        [Display(Name = "OrderOperationReport_CreateUserName", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public string CreateUserName { get; set; }

        [Display(Name = "OrderOperationReport_CreateDate", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public DateTime CreateDate { get; set; }

        public Int32 LastModifyUserId { get; set; }
        public string LastModifyUserName { get; set; }
        public DateTime LastModifyDate { get; set; }

        [Display(Name = "OrderOperationReport_CancelDate", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public DateTime? CancelDate { get; set; }

        public Int32? CancelUser { get; set; }

        [Display(Name = "OrderOperationReport_CancelUserName", ResourceType = typeof(Resources.ORD.OrderOperation))]
        public string CancelUserName { get; set; }

        public Int32 Version { get; set; }

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
            OrderOperationReport another = obj as OrderOperationReport;

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
