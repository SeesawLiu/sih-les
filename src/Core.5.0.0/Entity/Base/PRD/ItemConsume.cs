using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.PRD
{
    [Serializable]
    public partial class ItemConsume : EntityBase, IAuditable
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }


        [Display(Name = "ItemConsume_Item", ResourceType = typeof(Resources.PRD.ItemConsume))]
        [Export(ExportName = "ItemConsume")]
        public string Item { get; set; }

        [Display(Name = "ItemConsume_ItemDesc", ResourceType = typeof(Resources.PRD.ItemConsume))]
        public string ItemDesc { get; set; }

        [Display(Name = "Item_ReferenceCode", ResourceType = typeof(Resources.MD.Item))]
        public string RefItemCode { get; set; }

        [Display(Name = "ItemConsume_Qty", ResourceType = typeof(Resources.PRD.ItemConsume))]
        public Decimal Qty { get; set; }

        [Display(Name = "ItemConsume_ConsumedQty", ResourceType = typeof(Resources.PRD.ItemConsume))]
        public Decimal ConsumedQty { get; set; }

        public Int32 CreateUserId { get; set; }

        [Display(Name = "ItemConsume_CreateUserName", ResourceType = typeof(Resources.PRD.ItemConsume))]
        public string CreateUserName { get; set; }

        [Display(Name = "ItemConsume_CreateDate", ResourceType = typeof(Resources.PRD.ItemConsume))]
        public DateTime CreateDate { get; set; }

        public Int32 LastModifyUserId { get; set; }
        [Display(Name = "ItemConsume_LastModifyUserName", ResourceType = typeof(Resources.PRD.ItemConsume))]
        public string LastModifyUserName { get; set; }

        [Display(Name = "ItemConsume_LastModifyDate", ResourceType = typeof(Resources.PRD.ItemConsume))]
        public DateTime LastModifyDate { get; set; }

        public Int32 Version { get; set; }


        #endregion

        public override int GetHashCode()
        {
            if (Id != 0)
            {
                return Id.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            ItemConsume another = obj as ItemConsume;

            if (another == null)
            {
                return false;
            }
            else
            {
                return (this.Id == another.Id);
            }
        }
    }

}
