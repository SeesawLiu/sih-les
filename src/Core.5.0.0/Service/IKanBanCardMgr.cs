using System;
using System.Data;
using System.Collections.Generic;
using com.Sconit.Entity.KB;
using com.Sconit.Entity.ACC;
using com.Sconit.Entity.SCM;
using com.Sconit.Entity.MD;
using System.IO;

namespace com.Sconit.Service
{
    public interface IKanbanCardMgr
    {
        KanbanCard GetByCardNo(string cardNo);
        IList<KanbanCard> GetKanbanCards(string region, string supplier, string item, string multiSupply);

        IList<KanbanCard> AddManuallyByKanbanFlow(string flowCode, IList<FlowDetail> flowDetails, DateTime effDate, User calcUser, Boolean createNow);
        void DeleteManuallyByKanbanFlow(string[] idArray, string[] qtyArray);
        //string TryCalculate(string multiregion, string region, string location, DateTime startDate, DateTime endDate, int kbCalc, User calcUser);
        void TryCalculate(string multiregion, DateTime startDate, DateTime endDate, int kbCalc, User calcUser);

        void UpdateBatch(string batchno, User calcUser, IDictionary<string, KanbanCalc> calcsIn);
        DataSet GetUserKanbanCalcBatch(string multiregion, string region, string location, DateTime startDate, DateTime endDate, int kbCalc, User calcUser);
        void RunBatch(string multiregion, string region, string location, DateTime startDate, DateTime endDate, int kbCalc, User calcUser);
        void RunBatch(string multiregion, string location, DateTime startDate, DateTime endDate, int kbCalc);
        KanbanCard GenerateToDb(KanbanCard cardWithoutCardNoAndSeq, Boolean useNextSeq, int nextSeq, User calcUser, Boolean isAlternative, Region region);
        KanbanCard RotateKanbanCard(KanbanCard card, Boolean deleteAnother);

        KanbanCard FreezeByCard(string cardNo, User freezeUser);
        KanbanCard UnfreezeByCard(string cardNo, User freezeUser);
        void DeleteFrozenCards(DateTime freezeDate);

        Boolean IsEffectiveCard(KanbanCard card);
        Boolean IsFrozenCard(KanbanCard card);
        Boolean IsScanned(KanbanCard card);
        Boolean HasPassOrderTime(KanbanCard card);

        string ValidateKanbanCardFields(KanbanCard card);
        void SyncKanbanCardFields(KanbanCard card);
        void DeleteKanbanCard(KanbanCard card, User user);

        void ImportKanbanCalc(Stream inputStream, string batchno, User currentUser);
        void ImportKanbanCalc2(Stream inputStream, string batchno, User currentUser);

        void TryCalcKanbanLost(string multiregion, int ignoreTimeNumber);

        void DeleteFreezeKanbanCard();

        #region 看板卡遗失导入
        void CreateKanbanLostXls(Stream inputStream);
        #endregion

        void ImportKanBanCard(Stream inputStream, com.Sconit.CodeMaster.OrderType type, User currUser);
    }
}
