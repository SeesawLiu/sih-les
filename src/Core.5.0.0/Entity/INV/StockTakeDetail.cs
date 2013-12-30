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

        #endregion
    }
}