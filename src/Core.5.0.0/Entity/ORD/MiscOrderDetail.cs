using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

//TODO: Add other using statements here

namespace com.Sconit.Entity.ORD
{
    public partial class MiscOrderDetail
    {
        #region Non O/R Mapping Properties

        public IList<MiscOrderLocationDetail> MiscOrderLocationDetails { get; set; }

        [Display(Name = "MiscOrderMstr_Region", ResourceType = typeof(Resources.ORD.MiscOrderMstr))]
        public string Region { get; set; }


        [Display(Name = "MiscOrderMstr_EffectiveDate", ResourceType = typeof(Resources.ORD.MiscOrderMstr))]
        public DateTime EffectiveDate { get; set; }

        [Display(Name = "MiscOrderMstr_MoveType", ResourceType = typeof(Resources.ORD.MiscOrderMstr))]
        public string MoveType { get; set; }
        #endregion

        public void AddMiscOrderLocationDetail(MiscOrderLocationDetail miscOrderLocationDetail)
        {
            if (this.MiscOrderLocationDetails == null)
            {
                this.MiscOrderLocationDetails = new List<MiscOrderLocationDetail>();
            }
            this.MiscOrderLocationDetails.Add(miscOrderLocationDetail);
        }
    }
}