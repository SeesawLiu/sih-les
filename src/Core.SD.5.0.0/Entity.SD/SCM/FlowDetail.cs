namespace com.Sconit.Entity.SD.SCM
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class FlowDetail
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }

        public string Flow { get; set; }

        //public com.Sconit.CodeMaster.FlowStrategy Strategy { get; set; }

        public Int32 Sequence { get; set; }

        public string Item { get; set; }

        public string ReferenceItemCode { get; set; }

        //public string BaseUom { get; set; }

        public string Uom { get; set; }

        public Decimal? UnitCount { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        //public string Bom { get; set; }

        public string LocationFrom { get; set; }

        public string LocationTo { get; set; }

        //public string BillAddress { get; set; }

        //public string PriceList { get; set; }

        //public string Routing { get; set; }

        //public string ReturnRouting { get; set; }

        //public Boolean IsAutoCreate { get; set; }

        //public Boolean IsInspect { get; set; }

        //public Boolean IsRejectInspect { get; set; }

        //public Decimal? SafeStock { get; set; }

        //public Decimal? MaxStock { get; set; }

        //public Decimal? MinLotSize { get; set; }

        //public Decimal? OrderLotSize { get; set; }

        //public Decimal? ReceiveLotSize { get; set; }

        //public Decimal? BatchSize { get; set; }

        //public com.Sconit.CodeMaster.RoundUpOption RoundUpOption { get; set; }

        //public com.Sconit.CodeMaster.OrderBillTerm BillTerm { get; set; }

        //public Int32 MrpWeight { get; set; }

        //public decimal MrpTotal { get; set; }

        //public decimal MrpTotalAdjust { get; set; }

        //public string ExtraDemandSource { get; set; }

        //public string Container { get; set; }

        //public string ProductionScan { get; set; }

        //public string PickStrategy { get; set; }
        #endregion

        #region ¸¨Öú×Ö¶Î
        public List<FlowDetailInput> FlowDetailInputs { get; set; }
        public decimal CurrentQty { get; set; }
        public int Carton { get; set; }
        #endregion
    }
    public class FlowDetailInput
    {
        public string HuId { get; set; }
        public decimal Qty { get; set; }
        public string LotNo { get; set; }
    }

}
