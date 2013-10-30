using System;
using System.ComponentModel.DataAnnotations;

//TODO: Add other using statements here

namespace com.Sconit.Entity.MD
{
    public partial class ItemCategory
    {
        #region Non O/R Mapping Properties

        [Display(Name = "ItemCategory_ParentCategory", ResourceType = typeof(Resources.MD.ItemCategory))]
        public string ParentCategoryDescription { get; set; }
        
        #endregion
    }
}