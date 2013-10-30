using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;
//TODO: Add other using statements here

namespace com.Sconit.Entity.INV
{
    public partial class StockTakeResult
    {
        #region Non O/R Mapping Properties
        [Display(Name = "StockTakeResult_Qty", ResourceType = typeof(Resources.INV.StockTake))]
        public Decimal Qty {
            get
            {
                return this.DifferenceQty >= 0 ? this.StockTakeQty : this.InventoryQty;
            }
            }

        #endregion
    }

    public class StockTakeResultSummary
    {
       [Display(Name = "StockTakeResultSummary_StNo", ResourceType = typeof(Resources.INV.StockTakeResultSummary))]
        public String StNo { get; set; }
       // [Display(Name = "StockTakeResultSummary_IsAdjust", ResourceType = typeof(Resources.INV.StockTakeResultSummary))]
        public Boolean IsAdjust { get; set; }
        [Display(Name = "StockTakeResultSummary_Id", ResourceType = typeof(Resources.INV.StockTakeResultSummary))]
        public int Id { get; set; }
        [Display(Name = "StockTakeResultSummary_Item", ResourceType = typeof(Resources.INV.StockTakeResultSummary))]
        public string Item { get; set; }
        [Display(Name = "StockTakeResultSummary_ItemDescription", ResourceType = typeof(Resources.INV.StockTakeResultSummary))]
        public string ItemDescription { get; set; }
        [Display(Name = "StockTakeResultSummary_Uom", ResourceType = typeof(Resources.INV.StockTakeResultSummary))]
        public string Uom { get; set; }
        [Display(Name = "StockTakeResultSummary_Location", ResourceType = typeof(Resources.INV.StockTakeResultSummary))]
        public string Location { get; set; }

        #region 条码盘点
        [Display(Name = "StockTakeResultSummary_LotNo", ResourceType = typeof(Resources.INV.StockTakeResultSummary))]
        public string LotNo { get; set; }
        [Display(Name = "StockTakeResultSummary_Bin", ResourceType = typeof(Resources.INV.StockTakeResultSummary))]
        public string Bin { get; set; }
        [Display(Name = "StockTakeResultSummary_MatchQty", ResourceType = typeof(Resources.INV.StockTakeResultSummary))]
        public decimal? MatchQty { get; set; }
        [Display(Name = "StockTakeResultSummary_ShortageQty", ResourceType = typeof(Resources.INV.StockTakeResultSummary))]
        public decimal? ShortageQty { get; set; }
        [Display(Name = "StockTakeResultSummary_ProfitQty", ResourceType = typeof(Resources.INV.StockTakeResultSummary))]
        public decimal? ProfitQty { get; set; }
        #endregion

        #region 数量盘点
        public com.Sconit.CodeMaster.QualityType QualityType { get; set; }
        [Display(Name = "StockTakeResultSummary_InventoryQty", ResourceType = typeof(Resources.INV.StockTakeResultSummary))]
        public decimal InventoryQty { get; set; }
        [Display(Name = "StockTakeResultSummary_StockTakeQty", ResourceType = typeof(Resources.INV.StockTakeResultSummary))]
        public decimal StockTakeQty { get; set; }
        [Display(Name = "StockTakeResultSummary_DifferenceQty", ResourceType = typeof(Resources.INV.StockTakeResultSummary))]
        public decimal DifferenceQty { get; set; }
        #endregion
    }
}