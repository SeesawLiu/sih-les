using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.LOG
{
    [Serializable]
    public partial class SnapshotFlowDet4LeanEngine : EntityBase
    {
        public Int64 Id { get; set; }
        //<!--Id, Flow, Item, LocFrom, LocTo, OrderNo, Lvl, ErrorId, Msg, CreateDate, BatchNo-->

        //[Export(ExportSeq = 10, ExportName = "ExportInfo")]
        [Display(Name = "SnapshotFlowDet4LeanEngine_Flow", ResourceType = typeof(Resources.LOG.SnapshotFlowDet4LeanEngine))]
        public string Flow { get; set; }

        [Export(ExportSeq = 20, ExportName = "ExportInfo")]
        [Display(Name = "SnapshotFlowDet4LeanEngine_Item", ResourceType = typeof(Resources.LOG.SnapshotFlowDet4LeanEngine))]
        public string Item { get; set; }

        [Export(ExportSeq = 50, ExportName = "ExportInfo")]
        [Display(Name = "SnapshotFlowDet4LeanEngine_LocationFrom", ResourceType = typeof(Resources.LOG.SnapshotFlowDet4LeanEngine))]
        public string LocationFrom { get; set; }

        [Export(ExportSeq = 60, ExportName = "ExportInfo")]
        [Display(Name = "SnapshotFlowDet4LeanEngine_LocationTo", ResourceType = typeof(Resources.LOG.SnapshotFlowDet4LeanEngine))]
        public string LocationTo { get; set; }

        //[Export(ExportSeq = 80, ExportName = "ExportInfo")]
        [Display(Name = "SnapshotFlowDet4LeanEngine_OrderNo", ResourceType = typeof(Resources.LOG.SnapshotFlowDet4LeanEngine))]
        public string OrderNo { get; set; }

        //[Export(ExportSeq = 70, ExportName = "ExportInfo")]
        //[Display(Name = "SnapshotFlowDet4LeanEngine_AUFNR", ResourceType = typeof(Resources.LOG.SnapshotFlowDet4LeanEngine))]
        public Int16 Lvl { get; set; }

        [Export(ExportSeq = 90, ExportName = "ExportInfo")]
        [Display(Name = "SnapshotFlowDet4LeanEngine_ErrorId", ResourceType = typeof(Resources.LOG.SnapshotFlowDet4LeanEngine))]
        public Int16 ErrorId { get; set; }


        [Export(ExportSeq = 100, ExportName = "ExportInfo")]
        [Display(Name = "SnapshotFlowDet4LeanEngine_Message", ResourceType = typeof(Resources.LOG.SnapshotFlowDet4LeanEngine))]
        public string Message { get; set; }

        [Export(ExportSeq = 120, ExportName = "ExportInfo")]
        [Display(Name = "SnapshotFlowDet4LeanEngine_CreateDate", ResourceType = typeof(Resources.LOG.SnapshotFlowDet4LeanEngine))]
        public DateTime CreateDate { get; set; }

        [Export(ExportSeq = 110, ExportName = "ExportInfo")]
        [Display(Name = "SnapshotFlowDet4LeanEngine_BatchNo", ResourceType = typeof(Resources.LOG.SnapshotFlowDet4LeanEngine))]
        public Int32 BatchNo { get; set; }

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
            SnapshotFlowDet4LeanEngine another = obj as SnapshotFlowDet4LeanEngine;

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
