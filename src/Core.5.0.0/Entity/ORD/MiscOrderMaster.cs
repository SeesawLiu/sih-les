using System;
using System.Collections.Generic;
using com.Sconit.Entity.SYS;
using System.ComponentModel.DataAnnotations;

//TODO: Add other using statements here

namespace com.Sconit.Entity.ORD
{
    public partial class MiscOrderMaster
    {
        #region Non O/R Mapping Properties

        public IList<MiscOrderDetail> MiscOrderDetails;

        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.QualityType, ValueField = "QualityType")]
        [Display(Name = "MiscOrderMstr_QualityTypeDescription", ResourceType = typeof(Resources.ORD.MiscOrderMstr))]
        public string QualityTypeDescription { get; set; }


        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.MiscOrderStatus, ValueField = "Status")]
        [Display(Name = "MiscOrderMstr_StatusDescription", ResourceType = typeof(Resources.ORD.MiscOrderMstr))]
        public string StatusDescription { get; set; }

        public com.Sconit.CodeMaster.CheckConsignment? CheckConsignment { get; set; }
        

        #endregion

        public void AddMiscOrderDetail(MiscOrderDetail miscOrderDetail)
        {
            if (this.MiscOrderDetails == null)
            {
                this.MiscOrderDetails = new List<MiscOrderDetail>();
            }
            this.MiscOrderDetails.Add(miscOrderDetail);
        }
    }
}