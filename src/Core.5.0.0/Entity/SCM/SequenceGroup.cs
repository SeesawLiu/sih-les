using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

//TODO: Add other using statements here

namespace com.Sconit.Entity.SCM
{
    public partial class SequenceGroup
    {
        #region Non O/R Mapping Properties

        //TODO: Add Non O/R Mapping Properties here. 

        #endregion

        [Export(ExportName = "SequenceGroup", ExportSeq = 90)]
        [Display(Name = "SequenceGroup_PrevDateCount", ResourceType = typeof(Resources.SCM.SequenceGroup))]
        public string PrevDateCount
        {
            get{
                return this.PreviousDeliveryDate.HasValue && this.PreviousDeliveryCount.HasValue? 
                    this.PreviousDeliveryDate.Value.Day.ToString()+"-"+this.PreviousDeliveryCount.Value:string.Empty;
            }
        }

    }
}