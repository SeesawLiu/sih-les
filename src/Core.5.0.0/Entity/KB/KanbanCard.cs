using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.KB
{
    public partial class KanbanCard
    {
        #region Non O/R Mapping Properties
        // 核算结果: 新增看板, 冻结(删除)看板
        // 扫描结果: 可扫描，不可扫描
        // 结转结果: 
        // 冻结结果：
        public CodeMaster.KBProcessCode Ret { get; set; }
        [Display(Name = "Msg", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string Msg { get; set; }

        [Display(Name = "OpTime", ResourceType = typeof(Resources.KB.KanbanCard))]
        public DateTime OpTime { get; set; }

        [Display(Name = "KanbanNum", ResourceType = typeof(Resources.KB.KanbanCard))]
        public Int32 KanbanNum { get; set; }

        [Display(Name = "KanbanDeltaNum", ResourceType = typeof(Resources.KB.KanbanCard))]
        public Int32 KanbanDeltaNum { get; set; }

        public Decimal CalcKanbanNum { get; set; }

        public DateTime CalcConsumeDate { get; set; }

        public Decimal CalcConsumeQty { get; set; }

        public string BatchNo { get; set; }
        //使用qty,container,locbin字段
        //[Display(Name = "KanbanCard_UnitCount", ResourceType = typeof(Resources.KB.KanbanCard))]
        //public decimal? UnitCount { get; set; }
        //[Display(Name = "KanbanCard_UnitCountDescription", ResourceType = typeof(Resources.KB.KanbanCard))]
        //public string UnitCountDescription { get; set; }

        //[Display(Name = "KanbanCard_Bin", ResourceType = typeof(Resources.KB.KanbanCard))]
        //public string Bin { get; set; }

        public IList<ItemKit> ItemKitList { get; set; }

        public Boolean IsFreeze { get; set; }

        public string CheckedCardNo { get; set; }

        [Export(ExportName = "ExportKanbanCard", ExportSeq = 8)]
        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.KBCalculation, ValueField = "KBCalc")]
        [Display(Name = "KanbanCard_KBCalc", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string KBCalcDescription { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        [Export(ExportName = "ExportKanbanCard", ExportSeq = 30)]
        [Display(Name = "KanbanCard_Type", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string Type
        {
            get { return this.Sequence != null ? this.Sequence.Substring(0, 1) : string.Empty; }
        }

        /// <summary>
        /// 编号
        /// </summary>
        [Export(ExportName = "ExportKanbanCard", ExportSeq = 40)]
        [Display(Name = "KanbanCard_SequenceNo", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string SequenceNo
        {
            get
            {
                return this.Sequence != null ? this.Sequence.Substring(1) : string.Empty;
            }
        }

        #endregion
    }
}