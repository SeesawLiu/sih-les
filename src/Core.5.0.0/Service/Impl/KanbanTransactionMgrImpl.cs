using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Services.Transaction;
using NHibernate.Criterion;
using com.Sconit.Entity.KB;
using com.Sconit.Entity.ACC;
using com.Sconit.CodeMaster;

namespace com.Sconit.Service.Impl
{
    [Transactional]
    public class KanbanTransactionMgrImpl : BaseMgr, IKanbanTransactionMgr
    {
        #region 变量
        public IGenericMgr genericMgr { get; set; }
        #endregion

        public void RecordKanbanTransaction(KanbanCard card, User transUser, DateTime transDate, KBTransType transType)
        {
             if (card != null && transUser != null) {
                KanbanTransaction trans = new KanbanTransaction();
                trans.CardNo = card.CardNo;
                trans.TransactionType = transType;
                trans.TransactionDate = transDate;
                trans.CreateUserId = transUser.Id;
                trans.CreateUserName = transUser.Name;

                this.genericMgr.Create(trans);
            }
        }
    }
}
