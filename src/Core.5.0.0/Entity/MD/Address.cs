using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;
//TODO: Add other using statements here

namespace com.Sconit.Entity.MD
{
    public partial class Address
    {
        #region Non O/R Mapping Properties


        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.AddressType, ValueField = "Type")]
        [Display(Name = "Address_Type", ResourceType = typeof(Resources.MD.Address))]
        public string AddressTypeDescription { get; set; }

        [Display(Name = "PartyAddress_IsPrimary", ResourceType = typeof(Resources.MD.PartyAddress))]
        public bool IsPrimary { get; set; }
        [Display(Name = "PartyAddress_Sequence", ResourceType = typeof(Resources.MD.PartyAddress))]
        public int Sequence { get; set; }

        public string CodeAddressContent
        {
            get
            {
                return string.IsNullOrEmpty(this.Code) ? "" : this.Code + " [" + this.AddressContent + "]";
            }
        }
        #endregion
    }
}