using System;
using System.Collections.Generic;
using com.Sconit.Entity.KB;
using com.Sconit.Entity.ACC;
using com.Sconit.CodeMaster;

namespace com.Sconit.Service
{
    public interface IKanbanTransactionMgr
    {
        void RecordKanbanTransaction(KanbanCard card, User transUser, DateTime transDate, KBTransType transType);
    }
}
