using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Web.Models.KB
{
    public class ItemDailyConsumeView
    {
        public ItemDailyConsumeHead ItemDailyConsumeHead { get; set; }
        public IList<ItemDailyConsumeBody> ItemDailyConsumeBodyList { get; set; }
    }

    public class ItemDailyConsumeHead {
        public string Item = "Item";
        public string ItemDesc = "ItemDesc";
        public string Location = "Location";
        public string MultiSupplyGroup = "MultiSupplyGroup";
        public string MaxConsumeQty = "MaxConsumeQty";
        public string NewMaxConsumeQty = "NewMaxConsumeQty";
        public IList<ColumnCell> ColumnCellList { get; set; }
    }

    public class ItemDailyConsumeBody {
       [Display(Name = "KanbanCard_Item", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string Item { get; set; }
       [Display(Name = "ItemDailyConsume_ItemDesc", ResourceType = typeof(Resources.CUST.ItemDailyConsume))]
       public string ItemDesc { get; set; }
        [Display(Name = "KanbanCard_Location", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string Location { get; set; }
        [Display(Name = "KanbanCard_MultiSupplyGroup", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string MultiSupplyGroup { get; set; }
        [Display(Name = "KanbanCard_MaxConsumeQty", ResourceType = typeof(Resources.KB.KanbanCard))]
        public Decimal MaxConsumeQty { get; set; }
        public string NewMaxConsumeQty { get; set; }
        public List<RowCell> RowCellList { get; set; }
    }

    public class RowCell
    {
        public DateTime ConsumeDate { get; set; }
        public Decimal Qty { get; set; }
        public Decimal MaxQty { get; set; }
    }

    public class ColumnCell
    {
        public DateTime? ConsumeDate { get; set; }
    }
}