using System;
using System.Collections.Generic;
using com.Sconit.Entity.KB;
using com.Sconit.Entity.ACC;
using com.Sconit.Entity.SCM;
using com.Sconit.Entity.PRD;
using com.Sconit.Service.Impl;
using System.IO;

namespace com.Sconit.Service
{
    public interface IKanbanScanMgr
    {
        KanbanScan CreateKanbanScan(KanbanCard card, FlowDetail matchDetail, User scanUser, DateTime? scanTime);
        void DeleteKanbanScan(KanbanScan scan, User deleteUser);

        KanbanCard Scan(string cardNo, User scanUser);
        void ModifyScanQty(KanbanScan scan, decimal newQty, string tempKanbanCard, User modifyUser);

        //IList<KanbanCard> OrderCard(IList<KanbanCard> cards, User orderUser);
        //DateTime GetOrderTime(DateTime baseTime, KanbanScan scan);
        //StartEndTime ForwardCalculateNextDeliveryForSpecificWorkCalendar(WorkingCalendar nStart, DateTime currentOrderTime, FlowMaster matchFlow, Entity.SCM.FlowStrategy matchStrategy);

        IList<KanbanScan> GetOrderableScans(string region, string supplier, string lccode);
        IList<KanbanScan> OrderCard(string region, string supplier, string lccode, string orderTime, string scanIds, User orderUser, ref string orderNos);

      
        string OrderCard(string[] flowArray, DateTime[] windowTimeArray);
  
        void ImportkanbanScanXls(Stream inputStream);


        KanbanScan Scan(string cardNo, User scanUser, Boolean isScanBySD);
        //临时看板扫描
        void Scan(IList<KanbanScan> kanbanScanList);
    }

    public interface IKanbanScanOrderMgr
    {
        string AutoGenAnDonOrder(User user);
        string OrderCard(string scanIds, DateTime orderTime);
        string OrderCard(IList<KanbanScan> scans, DateTime orderTime);
        string OrderCard(IList<KanbanScan> scans, IList<FlowMaster> flowMasterList, DateTime orderTime);
    }
}
