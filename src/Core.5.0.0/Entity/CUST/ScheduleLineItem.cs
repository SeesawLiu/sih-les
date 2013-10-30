using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

//TODO: Add other using statements here

namespace com.Sconit.Entity.CUST
{
    public partial class ScheduleLineItem
    {
        #region Non O/R Mapping Properties

        //TODO: Add Non O/R Mapping Properties here. 

        #endregion

        [Export(ExportName = "ExportXls", ExportSeq = 70)]
        [Display(Name = "Item_ReferenceCode", ResourceType = typeof(Resources.MD.Item))]
        public string ReferenceCode { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "Item_Uom", ResourceType = typeof(Resources.MD.Item))]
        public string Uom { get; set; }


        [Export(ExportName = "ExportXls", ExportSeq = 80)]
        [Display(Name = "Item_Description", ResourceType = typeof(Resources.MD.Item))]
        public string Description { get; set; }
        [Export(ExportName = "ExportXls", ExportSeq = 100)]
        [Display(Name = "Item_UC", ResourceType = typeof(Resources.MD.Item))]
        public Decimal UnitCount { get; set; }

        [Export(ExportName = "ExportXls", ExportSeq = 20)]
        [Display(Name = "Party_Supplier_ShortCode", ResourceType = typeof(Resources.MD.Party))]
        public string ShortCode { get; set; }
        [Export(ExportName = "ExportXls", ExportSeq = 30)]
        [Display(Name = "Party_Name", ResourceType = typeof(Resources.MD.Party))]
        public string Name { get; set; }
        [Export(ExportName = "ExportXls", ExportSeq = 40)]
        [Display(Name = "OrderDetail_ScheduleLineNo", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string EBELN { get; set; }
        [Export(ExportName = "ExportXls", ExportSeq = 50)]
        [Display(Name = "OrderDetail_ScheduleLineSeq", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string EBELP { get; set; }

        [Export(ExportName = "ExportXls", ExportSeq = 90)]
        [Display(Name = "Item_Container", ResourceType = typeof(Resources.MD.Item))]
        public string Container { get; set; }

        //[Display(Name = "Item_ContainerDesc", ResourceType = typeof(Resources.MD.Item))]
        //public string ContainerDesc { get; set; }
    }
}