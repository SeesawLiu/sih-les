using System;
using System.ComponentModel.DataAnnotations;
//TODO: Add other using statements here

namespace com.Sconit.Entity.MD
{
    public partial class Item
    {
        public Item()
        {
        }

        public Item(string code)
        {
            this.Code = code;
        }
        #region Non O/R Mapping Properties

        //[Display(Name = "Item_MinUnitCount", ResourceType = typeof(Resources.MD.Item))]
        //public Decimal MinUnitCount { get; set; }

        [Display(Name = "Item_supplierLotNo", ResourceType = typeof(Resources.MD.Item))]
        public string supplierLotNo { get; set; }

        [Display(Name = "Item_LotNo", ResourceType = typeof(Resources.MD.Item))]
        public string LotNo { get; set; }

        [Display(Name = "Item_HuQty", ResourceType = typeof(Resources.MD.Item))]
        public Decimal HuQty { get; set; }

        [Display(Name = "Item_HuUnitCount", ResourceType = typeof(Resources.MD.Item))]
        public Decimal HuUnitCount { get; set; }

        [Display(Name = "Item_HuUom", ResourceType = typeof(Resources.MD.Item))]
        public string HuUom { get; set; }

        [Display(Name = "Item_ManufactureParty", ResourceType = typeof(Resources.MD.Item))]
        public string ManufactureParty { get; set; }

        public string CodeDescription
        {
            get
            {
                return this.Code + " [" + this.Description + "]";
            }
        }

        #endregion
    }
}