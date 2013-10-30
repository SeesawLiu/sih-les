using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.LOG
{
    [Serializable]
    public partial class SeqOrderChange : EntityBase
    {
        public Int32 Id { get; set; }

        [Export(ExportName = "ExportXLS", ExportSeq = 60)]
        public Int32 OrderDetId { get; set; }

        [Export(ExportName = "ExportXLS", ExportSeq = 10)]
        [Display(Name = "OrderDetail_OrderNo", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string OrderNo { get; set; }

        [Export(ExportName = "ExportXLS", ExportSeq = 50)]
        [Display(Name = "OrderDetail_Sequence", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Int32 Sequence { get; set; }

        [Export(ExportName = "ExportXLS", ExportSeq = 30)]
        [Display(Name = "SeqOrderChange_ExternalOrderNo", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ExternalOrderNo { get; set; }

        [Export(ExportName = "ExportXLS", ExportSeq = 40)]
        [Display(Name = "SeqOrderChange_ExternalSequence", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ExternalSequence { get; set; }

        [Export(ExportName = "ExportXLS", ExportSeq = 70)]
        [Display(Name = "OrderDetail_Item", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string Item { get; set; }

        [Export(ExportName = "ExportXLS", ExportSeq = 90)]
        [Display(Name = "OrderDetail_ItemDescription", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ItemDescription { get; set; }

        [Export(ExportName = "ExportXLS", ExportSeq = 80)]
        [Display(Name = "OrderDetail_ReferenceItemCode", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ReferenceItemCode { get; set; }

        [Display(Name = "OrderDetail_Uom", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string Uom { get; set; }

        [Export(ExportName = "ExportXLS", ExportSeq = 120)]
        [Display(Name = "OrderDetail_ManufactureParty", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string ManufactureParty { get; set; }

        [Export(ExportName = "ExportXLS", ExportSeq = 140)]
        [Display(Name = "OrderDetail_OrderedQty", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public Decimal OrderedQty { get; set; }

        public string BinTo { get; set; }

        public DateTime? StartDate { get; set; }

        [Export(ExportName = "ExportXLS", ExportSeq = 100)]
        [Display(Name = "OrderDetail_LocationFrom", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string LocationFrom { get; set; }

        [Display(Name = "OrderDetail_LocationFromName", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string LocationFromName { get; set; }

        [Export(ExportName = "ExportXLS", ExportSeq = 110)]
        [Display(Name = "OrderDetail_LocationTo", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string LocationTo { get; set; }

        [Display(Name = "OrderDetail_LocationToName", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string LocationToName { get; set; }

        [Export(ExportName = "ExportXLS", ExportSeq = 20)]
        [Display(Name = "OrderMaster_Flow", ResourceType = typeof(Resources.ORD.OrderMaster))]
        public string Flow { get; set; }

        public Int16 Status { get; set; }

        public Int32 CreateUserId { get; set; }

        [Export(ExportName = "ExportXLS", ExportSeq = 160)]
        [Display(Name = "OrderDetail_CreateUserName", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public string CreateUserName { get; set; }

        [Export(ExportName = "ExportXLS", ExportSeq = 170)]
        [Display(Name = "OrderDetail_CreateDate", ResourceType = typeof(Resources.ORD.OrderDetail))]
        public DateTime CreateDate { get; set; }

        [Export(ExportName = "ExportXLS", ExportSeq = 130)]
        [Display(Name = "OrderDetail_ConsignmentSupplier", ResourceType = typeof(Resources.ORD.PickListDetail))]
        public string ICHARG { get; set; }

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
            SeqOrderChange another = obj as SeqOrderChange;

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
