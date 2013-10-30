using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.ORD
{
    public partial class PickTask
    {
        [Display(Name = "PickTask_NewPicker", ResourceType = typeof(Resources.ORD.PickTask))]
        public string NewPicker { get; set; }
    }
}
