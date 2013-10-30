using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using com.Sconit.Entity.SYS;
using System.ComponentModel.DataAnnotations;

//TODO: Add other using statements here

namespace com.Sconit.Entity.ORD
{
    public partial class ReceiptMaster
    {
        #region Non O/R Mapping Properties

        [DataMember]
        public IList<ReceiptDetail> ReceiptDetails { get; set; }

        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.ReceiptStatus, ValueField = "Status")]
        [Display(Name = "ReceiptMaster_StatusDescription", ResourceType = typeof(Resources.ORD.ReceiptMaster))]
        public string ReceiptMasterStatusDescription { get; set; }

        [Display(Name = "ReceiptDetail_OrderNo", ResourceType = typeof(Resources.ORD.ReceiptDetail))]
        public string OrderNo { get; set; }
        #endregion

        #region methods
        public void AddReceiptDetail(ReceiptDetail receiptDetail)
        {
            if (ReceiptDetails == null)
            {
                ReceiptDetails = new List<ReceiptDetail>();
            }
            ReceiptDetails.Add(receiptDetail);
        }
        #endregion
    }
}