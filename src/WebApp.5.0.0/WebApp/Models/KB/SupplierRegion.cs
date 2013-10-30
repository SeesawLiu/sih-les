using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Web.Models.KB
{
    public class SupplierRegion
    {
        [Display(Name = "KanbanCard_Region", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string Region { get; set; }
        [Display(Name = "KanbanCard_RegionName", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string RegionName { get; set; }
        [Display(Name = "KanbanCard_Supplier", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string Supplier { get; set; }
        [Display(Name = "KanbanCard_SupplierName", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string SupplierName { get; set; }
        [Display(Name = "KanbanCard_WindowTime", ResourceType = typeof(Resources.KB.KanbanCard))]
        public DateTime? WindowTime { get; set; }
        [Display(Name = "KanbanCard_Flow", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string Flow { get; set; }
        [Display(Name = "KanbanCard_Memo", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string Memo { get; set; }
    }
}