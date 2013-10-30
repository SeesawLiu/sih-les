using System;
using com.Sconit.Entity.SYS;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

//TODO: Add other using statements here

namespace com.Sconit.Entity.SCM
{
    public partial class FlowMaster
    {
        #region Non O/R Mapping Properties

        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.OrderType, ValueField = "Type")]
        [Display(Name = "OrderMaster_Type", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string FlowTypeDescription { get; set; }

        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.FlowStrategy, ValueField = "FlowStrategy")]
        public string FlowStrategyDescription { get; set; }
        public string CheckFlowCode { get; set; }

        public string CodeDescription
        {
            get
            {
                return this.Code + " [" + this.Description + "]";
            }
        }

        public IList<FlowDetail> FlowDetails { get; set; }
        public IList<FlowBinding> FlowBindings { get; set; }

        public bool IsCreate { get; set; }
        #endregion

        #region methods
        public void AddFlowDetail(FlowDetail flowDetail)
        {
            if (FlowDetails == null)
            {
                FlowDetails = new List<FlowDetail>();
            }
            FlowDetails.Add(flowDetail);
        }
        #endregion
    }
}