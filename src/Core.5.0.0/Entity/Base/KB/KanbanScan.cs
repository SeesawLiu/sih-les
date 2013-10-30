using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.KB
{
    [Serializable]
    public partial class KanbanScan : EntityBase, IAuditable
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }

        [Export(ExportName="ExportKanbanScanXLS",ExportSeq=10)]
        [Display(Name = "KanbanScan_CardNo", ResourceType = typeof(Resources.KB.KanbanScan))]
		public string CardNo { get; set; }

        [Export(ExportName = "ExportKanbanScanXLS", ExportSeq = 20)]
        [Display(Name = "KanbanScan_Sequence", ResourceType = typeof(Resources.KB.KanbanScan))]
        public string Sequence { get; set; }

        [Export(ExportName = "ExportKanbanScanXLS", ExportSeq = 35)]
        [Display(Name = "KanbanScan_Flow", ResourceType = typeof(Resources.KB.KanbanScan))]
        public string Flow { get; set; }
        public Int32 FlowDetailId { get; set; }

        [Display(Name = "KanbanScan_LogisticCenterCode", ResourceType = typeof(Resources.KB.KanbanScan))]
        public string LogisticCenterCode { get; set; }

        [Display(Name = "KanbanScan_Region", ResourceType = typeof(Resources.KB.KanbanScan))]
        public string Region { get; set; }

        [Display(Name = "KanbanScan_RegionName", ResourceType = typeof(Resources.KB.KanbanScan))]
        public string RegionName { get; set; }

        [Export(ExportName = "ExportKanbanScanXLS", ExportSeq = 30)]
        [Display(Name = "KanbanScan_Supplier", ResourceType = typeof(Resources.KB.KanbanScan))]
        public string Supplier { get; set; }

        [Export(ExportName = "ExportKanbanScanXLS", ExportSeq = 40)]
        [Display(Name = "KanbanScan_SupplierName", ResourceType = typeof(Resources.KB.KanbanScan))]
        public string SupplierName { get; set; }

        [Export(ExportName = "ExportKanbanScanXLS", ExportSeq = 50)]
        [Display(Name = "KanbanScan_Item", ResourceType = typeof(Resources.KB.KanbanScan))]
        public string Item { get; set; }

        [Export(ExportName = "ExportKanbanScanXLS", ExportSeq = 60)]
        [Display(Name = "KanbanScan_ItemDescription", ResourceType = typeof(Resources.KB.KanbanScan))]
        public string ItemDescription { get; set; }

        [Display(Name = "KanbanScan_PONo", ResourceType = typeof(Resources.KB.KanbanScan))]
        public string PONo { get; set; }

        [Display(Name = "KanbanScan_POLineNo", ResourceType = typeof(Resources.KB.KanbanScan))]
        public string POLineNo { get; set; }

        [Export(ExportName = "ExportKanbanScanXLS", ExportSeq = 80)]
        [Display(Name = "KanbanScan_ScanTime", ResourceType = typeof(Resources.KB.KanbanScan))]
		public DateTime? ScanTime { get; set; }

        [Export(ExportName = "ExportKanbanScanXLS", ExportSeq = 70)]
        [Display(Name = "KanbanScan_ScanQty", ResourceType = typeof(Resources.KB.KanbanScan))]
        public decimal ScanQty { get; set; }

       
        public Int32 ScanUserId { get; set; }
         [Export(ExportName = "ExportKanbanScanXLS", ExportSeq = 90)]
        [Display(Name = "KanbanScan_ScanUserName", ResourceType = typeof(Resources.KB.KanbanScan))]
		public string ScanUserName { get; set; }

        public Boolean IsOrdered { get; set; }

        [Export(ExportName = "ExportKanbanScanXLS", ExportSeq = 100)]
        [Display(Name = "KanbanScan_OrderTime", ResourceType = typeof(Resources.KB.KanbanScan))]
		public DateTime? OrderTime { get; set; }

        [Export(ExportName = "ExportKanbanScanXLS", ExportSeq = 120)]
        [Display(Name = "KanbanScan_OrderQty", ResourceType = typeof(Resources.KB.KanbanScan))]
        public decimal OrderQty { get; set; }

		public Int32 OrderUserId { get; set; }

        [Export(ExportName = "ExportKanbanScanXLS", ExportSeq = 110)]
        [Display(Name = "KanbanScan_OrderUserName", ResourceType = typeof(Resources.KB.KanbanScan))]
		public string OrderUserName { get; set; }

        
        [Display(Name = "KanbanScan_OrderNo", ResourceType = typeof(Resources.KB.KanbanScan))]
        public string OrderNo { get; set; }

        [Display(Name = "KanbanCard_IsKit", ResourceType = typeof(Resources.KB.KanbanCard))]
        public Boolean IsKit { get; set; }

        [Display(Name = "FlowStrategy_KBCalc", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        public CodeMaster.KBCalculation? KBCalc { get; set; }

        [Display(Name = "KanbanScan_TempKanbanCard", ResourceType = typeof(Resources.KB.KanbanScan))]
        public string TempKanbanCard { get; set; }


        [Export(ExportName = "ExportKanbanScanXLS", ExportSeq = 55)]
        [Display(Name = "KanbanScan_ReferenceItemCode", ResourceType = typeof(Resources.KB.KanbanScan))]
        public string ReferenceItemCode { get; set; }

        public Int32 CreateUserId { get; set; }
        public string CreateUserName { get; set; }
        public DateTime CreateDate { get; set; }
        public Int32 LastModifyUserId { get; set; }
        public string LastModifyUserName { get; set; }
        public DateTime LastModifyDate { get; set; }

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
            KanbanScan another = obj as KanbanScan;

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
