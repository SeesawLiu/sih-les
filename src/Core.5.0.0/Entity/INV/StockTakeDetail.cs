using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

//TODO: Add other using statements here

namespace com.Sconit.Entity.INV
{
    public partial class StockTakeDetail
    {
        #region Non O/R Mapping Properties

        //已经存在老的记录
        public decimal? OldQty { get; set; }

        /// <summary>
        /// null new, true update, false delete
        /// </summary>
        public bool? IsUpdate { get; set; }

        //TODO: Add Non O/R Mapping Properties here. 

        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.QualityType, ValueField = "QualityType")]
        [Display(Name = "LocationDetailView_QualityType", ResourceType = typeof(Resources.View.LocationDetailView))]
        public string QualityTypeDescription { get; set; }

        [Export(ExportName = "ExportStockTakeDetail", ExportSeq = 60)]
        [Display(Name = "LocationDetailView_IsCS", ResourceType = typeof(Resources.View.LocationDetailView))]
        public string IsConsigementFromat { get { return this.IsConsigement ? "1" : "0"; } }

        [Export(ExportName = "ExportStockTakeDetail", ExportSeq = 80)]
        [Display(Name = "StockTakeDetail_Qty", ResourceType = typeof(Resources.INV.StockTake))]
        public string QtyFormat { 
            get {
                return this.Qty != 0 ? ((Math.Ceiling(this.Qty) == this.Qty && Math.Floor(this.Qty) == this.Qty) ? ((int)this.Qty).ToString() : this.Qty.ToString("0.######")) : string.Empty; 
             }
        }

        [Export(ExportName = "ExportStockTakeDetail", ExportSeq = 50)]
        [Display(Name = "LocationDetailView_QualityType", ResourceType = typeof(Resources.View.LocationDetailView))]
        public string QualityTypeFromat
        {
            get { 
                return  ((int)this.QualityType).ToString();
            }
        }

        public static bool IsInterate(decimal dl)
        {
            return Math.Ceiling(dl) == dl && Math.Floor(dl) == dl;
        }
        #endregion
    }
}