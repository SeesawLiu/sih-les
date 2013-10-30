using System;
using com.Sconit.Entity.SYS;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.MD;

//TODO: Add other using statements here

namespace com.Sconit.Entity.PRD
{
    public partial class BomDetail
    {
        #region Non O/R Mapping Properties

        //缓存单位成品的BOM用量
        public decimal CalculatedQty { get; set; }

        //缓存投料/退料的库位
        [Display(Name = "BomDetail_FeedLocation", ResourceType = typeof(Resources.PRD.Bom))]
        public string FeedLocation { get; set; }

        //缓存本次投料数
        [Display(Name = "BomDetail_FeedQty", ResourceType = typeof(Resources.PRD.Bom))]
        public decimal FeedQty { get; set; }

        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.BomStructureType, ValueField = "StructureType")]
        [Display(Name = "BomDetail_StructureType", ResourceType = typeof(Resources.PRD.Bom))]
        public string StructureTypeDescription { get; set; }

        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.BackFlushMethod, ValueField = "BackFlushMethod")]
        [Display(Name = "BomDetail_BackFlushMethod", ResourceType = typeof(Resources.PRD.Bom))]
        public string BackFlushMethodDescription { get; set; }

        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.FeedMethod, ValueField = "FeedMethod")]
        [Display(Name = "BomDetail_FeedMethod", ResourceType = typeof(Resources.PRD.Bom))]
        public string FeedMethodDescription { get; set; }

        public Item CurrentItem { get; set; }

        [Display(Name = "BomDetail_ItemDescription", ResourceType = typeof(Resources.PRD.Bom))]
        public string ItemDescription { get; set; }

        [Display(Name = "BomDetail_ReferenceItemCode", ResourceType = typeof(Resources.PRD.Bom))]
        public string ReferenceItemCode { get; set; }

        [Display(Name = "BomDetail_UnitCount", ResourceType = typeof(Resources.PRD.Bom))]
        public Decimal UnitCount { get; set; }
        [Display(Name = "BomDetail_UnitCountDescription", ResourceType = typeof(Resources.PRD.Bom))]
        public string UnitCountDescription { get; set; }
        [Display(Name = "BomDetail_MinUnitCount", ResourceType = typeof(Resources.PRD.Bom))]
        public Decimal MinUnitCount { get; set; }
            [Display(Name = "BomDetail_ManufactureParty", ResourceType = typeof(Resources.PRD.Bom))]
        public Decimal ManufactureParty { get; set; }
        #endregion
    }
}