using System;
using System.Collections.Generic;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.PRD;

namespace com.Sconit.Service
{
    public interface IBomMgr
    {
        IList<BomDetail> GetNextLevelBomDetail(string bomCode, DateTime? effectiveDate);

        IList<BomDetail> GetFlatBomDetail(string bomCode, DateTime? effectiveDate);

        string FindItemBom(Item item);

        string FindItemBom(string itemCode);

        IList<BomDetail> GetProductLineWeightAverageBomDetail(string flow);
    }
}
