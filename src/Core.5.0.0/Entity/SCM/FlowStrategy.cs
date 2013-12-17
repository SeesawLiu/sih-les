using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;
//TODO: Add other using statements here

namespace com.Sconit.Entity.SCM
{
    public partial class FlowStrategy
    {
        #region Non O/R Mapping Properties
        //[CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.Strategy, ValueField = "Strategy")]
        //[Display(Name = "FlowStrategy_Strategy", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        //public string StrategyDescription { get; set; }
        public bool IsCreate { get; set; }

        [Display(Name = "FlowStrategy_ShipOrderTime", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        public DateTime? ShipOrderTime { get; set; }
        [Display(Name = "FlowStrategy_ReqStartTime", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        public DateTime? ReqStartTime { get; set; }
        [Display(Name = "FlowStrategy_ReqEndTime", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        public DateTime? ReqEndTime { get; set; }

        [Display(Name = "FlowStrategy_WindowTime", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        public DateTime? WindowTime { get; set; }
        [Display(Name = "FlowStrategy_Region", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        public string Region { get; set; }
        [Display(Name = "FlowStrategy_CalculateType", ResourceType = typeof(Resources.SCM.FlowStrategy))]
        public string CalculateType { get; set; }
        #endregion
    }
}