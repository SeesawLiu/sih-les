using System;
using com.Sconit.Entity.SYS;
using System.ComponentModel.DataAnnotations;

//TODO: Add other using statements here

namespace com.Sconit.Entity.SCM
{
    public partial class FlowBinding
    {
        #region Non O/R Mapping Properties

        public string MasterFlowType
        {
            get
            {
                if (MasterFlow == null)
                {
                    return null;
                }
                else
                {
                    return this.MasterFlow.FlowTypeDescription;
                }
            }
        }

        public string BindedFlowType
        {
            get
            {
                if (BindedFlow == null)
                {
                    return null;
                }
                else
                {
                    return this.BindedFlow.FlowTypeDescription;
                }
            }
        }

        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.BindType, ValueField = "BindType")]
        [Display(Name = "FlowBinding_BindType", ResourceType = typeof(Resources.SCM.FlowBinding))]
        public string BindTypeDescription { get; set; }

        //[Display(Name = "FlowMaster_Code", ResourceType = typeof(Resources.SCM.FlowMaster))]
        //public string MasterFlowCode
        //{
        //    get
        //    {
        //        return this.MasterFlow != null ? this.MasterFlow.Code : string.Empty;
        //    }
        //    set;
        //}

        [Display(Name = "FlowMaster_Description", ResourceType = typeof(Resources.SCM.FlowMaster))]
        public string BindedFlowDescription
        {
            get
            {
                return this.BindedFlow != null ? this.BindedFlow.Description : string.Empty;
            }
        }

        #endregion
    }
}