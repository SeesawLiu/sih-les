using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;
//TODO: Add other using statements here

namespace com.Sconit.Entity.INP
{
    public partial class InspectResult
    {
        #region Non O/R Mapping Properties

        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.JudgeResult, ValueField = "JudgeResult")]
        [Display(Name = "InspectResult_JudgeResult", ResourceType = typeof(Resources.INP.InspectResult))]
        public string JudgeResultDescription { get; set; }

        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.InspectDefect, ValueField = "Defect")]
        [Display(Name = "InspectResult_Defect", ResourceType = typeof(Resources.INP.InspectResult))]
        public string DefectDescription { get; set; }

        [Display(Name = "InspectResult_FailCode", ResourceType = typeof(Resources.INP.InspectResult))]
        public string FailCodeDescription { get; set; }

        public bool IsCheck { get; set; }

        #region 报验单退货 移动类型需要字段

        [Display(Name = "MiscOrderDetail_EBELP", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
        public string EBELP { get; set; }

        [Display(Name = "MiscOrderDetail_ReserveNo", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
        public string ReserveNo { get; set; }

        [Display(Name = "MiscOrderDetail_ReserveLine", ResourceType = typeof(Resources.ORD.MiscOrderDetail))]
        public string ReserveLine { get; set; }
        #endregion

        public decimal TobeHandleQty
        {
            get
            {
                return this.JudgeQty - this.HandleQty;
            }
        }

        [Display(Name = "InspectResult_CurrentHandleQty", ResourceType = typeof(Resources.INP.InspectResult))]
        public decimal CurrentHandleQty { get; set; }

        [Display(Name = "InspectResult_CurrentFailCode", ResourceType = typeof(Resources.INP.InspectResult))]
        public string CurrentFailCode { get; set; }        

        #endregion
    }
}