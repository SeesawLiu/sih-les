using System;
using com.Sconit.Entity.SYS;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.MD;

//TODO: Add other using statements here

namespace com.Sconit.Entity.PRD
{
    public partial class ItemConsume
    {
        [Display(Name = "ItemConsume_IsClose", ResourceType = typeof(Resources.PRD.ItemConsume))]
        public bool IsClose { get; set; }

    }
}