using System;
using System.ComponentModel.DataAnnotations;
//TODO: Add other using statements here

namespace com.Sconit.Entity.MD
{
    public partial class UomConversion
    {
        #region Non O/R Mapping Properties
        [Display(Name = "UomConvert_Item", ResourceType = typeof(Resources.MD.UomConvert))]
        public string ItemCode {get;set;}
        [Display(Name = "Item_Description", ResourceType = typeof(Resources.MD.Item))]
        public string ItemDescription { get; set; }
        #endregion
    }
}