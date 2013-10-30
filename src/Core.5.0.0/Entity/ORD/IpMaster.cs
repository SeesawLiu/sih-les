using System;
using com.Sconit.Entity.SYS;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Runtime.Serialization;
//TODO: Add other using statements here

namespace com.Sconit.Entity.ORD
{
    public partial class IpMaster
    {
        public string ShipmentNo { get; set; }
        #region Non O/R Mapping Properties

        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.IpStatus, ValueField = "Status")]
        [Display(Name = "IpMaster_Status", ResourceType = typeof(Resources.ORD.IpMaster))]
        public string IpMasterStatusDescription { get; set; }

        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.IpDetailType, ValueField = "Type")]
        [Display(Name = "IpMaster_Type", ResourceType = typeof(Resources.ORD.IpMaster))]
        public string IpMasterTypeDescription { get; set; }

        [DataMember]
        public IList<IpDetail> IpDetails { get; set; }

        #endregion

        #region methods
        public void AddIpDetail(IpDetail ipDetail)
        {
            if (IpDetails == null)
            {
                IpDetails = new List<IpDetail>();
            }
            IpDetails.Add(ipDetail);
        }
        #endregion
    }
}