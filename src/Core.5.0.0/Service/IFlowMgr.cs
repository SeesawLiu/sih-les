using System.Collections.Generic;
using com.Sconit.Entity.SCM;
using System.IO;
using System;

namespace com.Sconit.Service
{
    public interface IFlowMgr
    {
        IList<FlowBinding> GetFlowBinding(string flow);
        FlowStrategy GetFlowStrategy(string flow);
        IList<FlowDetail> GetFlowDetailList(string flow);
        IList<FlowDetail> GetFlowDetailList(string flow, bool includeInactive);
        IList<FlowDetail> GetFlowDetailList(string flow, bool includeInactive, bool includeReferenceFlow);
        FlowMaster GetReverseFlow(FlowMaster flow, IList<string> itemCodeList);
        void CreateFlow(FlowMaster flowMaster);
        void UpdateFlowStrategy(FlowStrategy flowstrategy);
        void DeleteFlow(string flow);
        void CreateFlowDetail(FlowDetail flowDetail);
        void UpdateFlowDetail(FlowDetail flowDetail);
        void UpdateFlow(FlowMaster flowMaster, bool isChangeSL);
        #region 路线明细导入
        void CreateFlowDetailXls(Stream inputStream);
        #endregion

        void ImportKanBanFlow(Stream inputStream, com.Sconit.CodeMaster.OrderType type);
        IList<FlowDetail> GetFlowDetails(IList<Int32> flowDetailIdList);
        void DeleteKBFlowDetail(Int32 flowDetailId);
        void UpdateFlowShiftDetails(string flow, IList<FlowShiftDetail> addFlowShiftDet, IList<FlowShiftDetail> updateFlowShiftDetail, IList<FlowShiftDetail> deleteFlowShiftDet);

        void BatchTransferDetailXls(Stream stream);
    }
}
