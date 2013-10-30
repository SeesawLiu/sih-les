using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Services.Transaction;
using NHibernate.Criterion;
using com.Sconit.Entity.KB;
using AutoMapper;
using com.Sconit.Entity.ACC;
using com.Sconit.Entity.SCM;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.PRD;
using com.Sconit.Entity.Exception;
using com.Sconit.CodeMaster;
using com.Sconit.Utility;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.SYS;
using com.Sconit.Entity;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.Collections;
using com.Sconit.PrintModel.ORD;
using System.Net.Mail;

namespace com.Sconit.Service.Impl
{
    [Transactional]
    public class KanbanScanMgrImpl : BaseMgr, IKanbanScanMgr
    {
        #region 变量
        public IGenericMgr genericMgr { get; set; }
        public IFlowMgr flowMgr { get; set; }
        public IOrderMgr orderMgr { get; set; }
        public IKanbanCardMgr kanbanCardMgr { get; set; }
        public IKanbanTransactionMgr kanbanTransactionMgr { get; set; }
        public IMultiSupplyGroupMgr multiSupplyGroupMgr { get; set; }
        public IWorkingCalendarMgr workingCalendarMgr { get; set; }
        public ISystemMgr systemMgr { get; set; }
        public IItemMgr itemMgr { get; set; }
        public IKanbanScanOrderMgr kanbanScanOrderMgr { get; set; }
        #endregion

        //public void CreateKanbanScan(KanbanCard card, FlowDetail matchDetail, User scanUser, DateTime? scanTime)
        //{
        //    if (card != null && matchDetail != null && scanUser != null)
        //    {
        //        KanbanScan scan = Mapper.Map<KanbanCard, KanbanScan>(card);
        //        scan.ScanUserId = scanUser.Id;
        //        scan.ScanUserName = scanUser.Name;
        //        scan.ScanTime = scanTime;

        //        //FlowDetail fd = this.genericMgr.FindAll<FlowDetail>("from FlowDetail where Id = ? ", card.FlowDetailId).SingleOrDefault();
        //        scan.ScanQty = matchDetail.UnitCount;
        //        scan.IsOrdered = false;

        //        this.genericMgr.Create(scan);
        //    }
        //}

        public void CreateKanbanScan(KanbanCard card, decimal qty, User scanUser, DateTime? scanTime)
        {
            if (card != null && scanUser != null)
            {
                KanbanScan scan = Mapper.Map<KanbanCard, KanbanScan>(card);
                scan.ScanUserId = scanUser.Id;
                scan.ScanUserName = scanUser.Name;
                scan.ScanTime = scanTime;
                scan.ReferenceItemCode = card.ReferenceItemCode;
                //用物流中心记工位
                scan.LogisticCenterCode = card.OpRef;
                //FlowDetail fd = this.genericMgr.FindAll<FlowDetail>("from FlowDetail where Id = ? ", card.FlowDetailId).SingleOrDefault();
                scan.ScanQty = qty;
                scan.IsOrdered = false;

                this.genericMgr.Create(scan);
                card.ScanId = scan.Id;
            }
        }

        public KanbanScan CreateKanbanScan(KanbanCard card, FlowDetail matchDetail, User scanUser, DateTime? scanTime)
        {
            KanbanScan scan = new KanbanScan();
            if (card != null && matchDetail != null && scanUser != null)
            {
                scan = Mapper.Map<KanbanCard, KanbanScan>(card);
                scan.ScanUserId = scanUser.Id;
                scan.ScanUserName = scanUser.Name;
                scan.ScanTime = scanTime;
                scan.ReferenceItemCode = card.ReferenceItemCode;
                //用物流中心记工位
                scan.LogisticCenterCode = matchDetail.BinTo;
                //FlowDetail fd = this.genericMgr.FindAll<FlowDetail>("from FlowDetail where Id = ? ", card.FlowDetailId).SingleOrDefault();
                scan.ScanQty = matchDetail.MinUnitCount;
                scan.IsOrdered = false;

                this.genericMgr.Create(scan);
                card.ScanId = scan.Id;
                return scan;
            }
            return scan;
        }

        [Transaction(TransactionMode.Requires)]
        public void DeleteKanbanScan(KanbanScan scan, User deleteUser)
        {
            if (scan.IsOrdered)
            {
                throw new BusinessException("扫描记录已结转，不能删除");
            }

            KanbanCard card = this.kanbanCardMgr.GetByCardNo(scan.CardNo);
            if (card != null)
            {
                card.Status = KBStatus.Loop;
                this.genericMgr.Update(card);
                this.kanbanTransactionMgr.RecordKanbanTransaction(card, deleteUser, DateTime.Now, KBTransType.UnScan);

                #region 记到双轨溢出量
                //if (!string.IsNullOrEmpty(card.MultiSupplyGroup))
                //{
                //    MultiSupplyGroup msg = genericMgr.FindAll<MultiSupplyGroup>("from MultiSupplyGroup m where m.GroupNo = ?", card.MultiSupplyGroup).FirstOrDefault();
                //    if (msg != null)
                //    {
                //        if (msg.KBEffSupplier == card.Supplier && msg.KBAccumulateQty >= scan.ScanQty)
                //        {
                //            msg.KBAccumulateQty -= scan.ScanQty;
                //            genericMgr.Update(msg);
                //        }
                //        else
                //        {
                //            MultiSupplySupplier mss = genericMgr.FindAll<MultiSupplySupplier>("from MultiSupplySupplier s where s.GroupNo = ? and s.Supplier = ?", new object[] { card.MultiSupplyGroup, card.Supplier }).FirstOrDefault();
                //            mss.KanbanSpillQty -= Convert.ToInt32(scan.ScanQty);
                //            genericMgr.Update(mss);
                //        }
                //    }
                //}
                #endregion
            }
            this.genericMgr.Delete(scan);
            this.kanbanTransactionMgr.RecordKanbanTransaction(card, deleteUser, DateTime.Now, KBTransType.DeleteScan);
        }

        #region 扫描
        [Transaction(TransactionMode.Requires)]
        public KanbanCard Scan(string cardNo, User scanUser)
        {
            KanbanCard card = this.kanbanCardMgr.GetByCardNo(cardNo);
            //是否存在
            if (card == null)
            {
                #region 处理遗失
                IList<KanbanLost> kanbanLostList = genericMgr.FindAll<KanbanLost>();
                var q = kanbanLostList.Where(k => k.CardNo == cardNo);
                if (q != null && q.Count() > 0)
                {
                    KanbanLost kanbanLost = q.First();
                    genericMgr.Delete(kanbanLost);
                }
                #endregion

                card = new KanbanCard();
                card.CardNo = cardNo;
                card.OpTime = DateTime.Now;

                SetCardRetMsg(card, KBProcessCode.NonExistingCard, Resources.KB.KanbanCard.Process_NonExistingCard);
            }
            else
            {
                //是否有效(未冻结)
                if (this.kanbanCardMgr.IsEffectiveCard(card))
                {
                    if (this.kanbanCardMgr.IsFrozenCard(card))
                    {
                        card.OpTime = DateTime.Now;
                        SetCardRetMsg(card, KBProcessCode.Frozen, Resources.KB.KanbanCard.Process_Frozen);
                    }
                    else
                    {
                        //是否已经扫描过
                        if (this.kanbanCardMgr.IsScanned(card))
                        {
                            card.OpTime = DateTime.Now;
                            SetCardRetMsg(card, KBProcessCode.AlreadyScanned, Resources.KB.KanbanCard.Process_AlreadyScanned);
                        }
                        else
                        {
                            //多轨,是否超循环量,这个时间计算多轨
                            decimal oldQty = card.Qty;
                            decimal scanQty = card.Qty;
                            //if (!multiSupplyGroupMgr.ScanMultiSupplyGroup(card))
                            //{
                            //    #region 获取下一个双轨厂商零件，新华说现在看板都是双轨，直接取另一个厂商好了
                            //    MultiSupplyItem multiSupplyItem = genericMgr.FindAll<MultiSupplyItem>("from MultiSupplyItem where GroupNo = ? and Supplier != ? ", new object[] { card.MultiSupplyGroup, card.Supplier }).FirstOrDefault();

                            //    #endregion
                            //    if (multiSupplyItem != null)
                            //    {
                            //        SetCardRetMsg(card, KBProcessCode.ExceedCycleAmount, "当前生效的多轨厂商不是" + card.Supplier + "请扫描厂商:" + multiSupplyItem.Supplier + "件号:" + multiSupplyItem.Item);
                            //    }
                            //    else
                            //    {
                            //        SetCardRetMsg(card, KBProcessCode.ExceedCycleAmount, "当前生效的多轨厂商不是" + card.Supplier);
                            //    }
                            //}
                            //else
                            //{
                            //if (oldQty != card.Qty)
                            //{
                            //    SetCardRetMsg(card, KBProcessCode.ExceedCycleAmount, "扫描量超过剩余量，请制作临时看板卡，数量" + card.Qty);
                            //    scanQty = card.Qty;
                            //    card.Qty = oldQty;
                            //}

                            FlowDetail fd = this.genericMgr.FindAll<FlowDetail>("from FlowDetail where Id = ? ", card.FlowDetailId).SingleOrDefault();
                            if (fd == null)
                            {
                                card.OpTime = DateTime.Now;
                                SetCardRetMsg(card, KBProcessCode.NoFlowDetail, Resources.KB.KanbanCard.Process_NoFlowDetail);
                            }
                            else
                            {
                                //判断kanbancard相关字段与item，fd及supplier等其他表字段是否有变化
                                string validText = kanbanCardMgr.ValidateKanbanCardFields(card);
                                if (!String.IsNullOrEmpty(validText))
                                {
                                    card.OpTime = DateTime.Now;
                                    SetCardRetMsg(card, KBProcessCode.InvalidFields, Resources.KB.KanbanCard.Process_InvalidFields + ":" + validText);
                                }
                                else
                                {
                                    // 可以扫描,创建扫描记录,kb事务记录,设置卡片状态到scanned,不知道为什么会有0的情况
                                    if (scanQty > 0)
                                    {
                                        DateTime dt = DateTime.Now;
                                        if (!string.IsNullOrEmpty(card.MultiSupplyGroup))
                                        {
                                            CreateKanbanScan(card, scanQty, scanUser, dt);
                                        }
                                        else
                                        {
                                            CreateKanbanScan(card, fd, scanUser, dt);
                                        }
                                        card.Status = KBStatus.Scanned;
                                        card.LastUseDate = dt;
                                        card.OpTime = dt;
                                        this.genericMgr.Update(card);
                                        this.kanbanTransactionMgr.RecordKanbanTransaction(card, scanUser, dt, KBTransType.Scan);

                                        if (oldQty == scanQty)
                                        {
                                            SetCardRetMsg(card, KBProcessCode.Ok, Resources.KB.KanbanCard.Process_Ok);
                                        }
                                    }
                                }
                            }
                        }
                        //}
                    }
                }
                else
                {
                    card.OpTime = DateTime.Now;
                    SetCardRetMsg(card, KBProcessCode.NotEffective, Resources.KB.KanbanCard.Process_NotEffective);
                }
            }
            return card;
        }


        [Transaction(TransactionMode.Requires)]
        public KanbanScan Scan(string cardNo, User scanUser, Boolean isScanBySD)
        {
            KanbanScan kbscan = new KanbanScan();
            KanbanCard card = this.kanbanCardMgr.GetByCardNo(cardNo);

            //是否有效(未冻结)
            if (this.kanbanCardMgr.IsEffectiveCard(card))
            {
                if (this.kanbanCardMgr.IsFrozenCard(card))
                {
                    card.OpTime = DateTime.Now;
                    SetCardRetMsg(card, KBProcessCode.Frozen, Resources.KB.KanbanCard.Process_Frozen);
                }
                else
                {
                    FlowDetail fd = this.genericMgr.FindAll<FlowDetail>("from FlowDetail where Id = ? ", card.FlowDetailId).SingleOrDefault();
                    DateTime dt = DateTime.Now;
                    //if (!string.IsNullOrEmpty(card.MultiSupplyGroup))
                    //{
                    //    CreateKanbanScan(card, card.Qty, scanUser, dt);
                    //}
                    //else
                    //{
                    kbscan = CreateKanbanScan(card, fd, scanUser, dt);
                    //}
                    card.Status = KBStatus.Scanned;
                    card.LastUseDate = dt;
                    card.OpTime = dt;
                    this.genericMgr.Update(card);
                    this.kanbanTransactionMgr.RecordKanbanTransaction(card, scanUser, dt, KBTransType.Scan);
                }
            }
            else
            {
                card.OpTime = DateTime.Now;
                SetCardRetMsg(card, KBProcessCode.NotEffective, Resources.KB.KanbanCard.Process_NotEffective);
            }
            return kbscan;
        }


        [Transaction(TransactionMode.Requires)]
        public void Scan(IList<KanbanScan> kanbanScanList)
        {
            BusinessException bussinessException = new BusinessException();
            IList<KanbanScan> exactKanbanScanList = new List<KanbanScan>();
            foreach (KanbanScan kanbanScan in kanbanScanList)
            {
                FlowDetail flowDetail = genericMgr.FindAll<FlowDetail>("from FlowDetail d where d.Flow = ? and d.Item = ?", new object[] { kanbanScan.Flow, kanbanScan.Item }).FirstOrDefault();

                if (flowDetail == null)
                {
                    bussinessException.AddMessage("没有维护路线{0}物料代码{1}", kanbanScan.Flow, kanbanScan.Item);
                }
                else
                {
                    FlowMaster flowMaster = genericMgr.FindById<FlowMaster>(flowDetail.Flow);
                    User scanUser = SecurityContextHolder.Get();
                    KanbanScan scan = new KanbanScan();
                    scan.CardNo = kanbanScan.TempKanbanCard;
                    scan.Flow = flowMaster.Code;
                    scan.Item = flowDetail.Item;
                    scan.FlowDetailId = flowDetail.Id;
                    scan.ItemDescription = genericMgr.FindById<Item>(flowDetail.Item).Description;
                    Party fromParty = genericMgr.FindById<Party>(flowMaster.PartyFrom);
                    scan.Supplier = flowMaster.PartyFrom;
                    scan.SupplierName = fromParty.Name;
                    //if (fromParty is Supplier)
                    //{
                    //    scan.LogisticCenterCode = ((Supplier)fromParty).LogisticsCentre;
                    //}
                    scan.Region = flowMaster.PartyTo;
                    scan.RegionName = genericMgr.FindById<Party>(flowMaster.PartyTo).Name;
                    scan.ScanUserId = scanUser.Id;
                    scan.ScanUserName = scanUser.Name;
                    scan.ScanTime = DateTime.Now;
                    scan.ScanQty = kanbanScan.OrderQty;
                    scan.OrderQty = 0;
                    scan.TempKanbanCard = kanbanScan.TempKanbanCard;
                    scan.Sequence = "0";
                    scan.IsOrdered = false;

                    exactKanbanScanList.Add(scan);
                }
            }
            if (bussinessException.HasMessage)
            {
                throw bussinessException;
            }
            foreach (KanbanScan kb in exactKanbanScanList)
            {
                genericMgr.Create(kb);
            }
        }

        [Transaction(TransactionMode.Requires)]
        public void ModifyScanQty(KanbanScan scan, decimal newQty, string tempKanbanCard, User modifyUser)
        {
            DateTime dt = DateTime.Now;
            scan.ScanQty = newQty;
            scan.TempKanbanCard = tempKanbanCard;
            this.genericMgr.Update(scan);

            KanbanCard card = this.kanbanCardMgr.GetByCardNo(scan.CardNo);
            this.kanbanTransactionMgr.RecordKanbanTransaction(card, modifyUser, dt, KBTransType.ModifyScan);
        }
        #endregion

        #region 结转
        public IList<KanbanScan> GetOrderableScans(string region, string supplier, string lccode)
        {
            if (string.IsNullOrEmpty(lccode))
            {
                //物流中心为空，按供应商
                return this.genericMgr.FindAll<KanbanScan>("from KanbanScan where IsOrdered = 0 and Region = ? and Supplier = ? ", new object[] { region, supplier });
            }
            else
            {
                //按物流中心
                return this.genericMgr.FindAll<KanbanScan>("from KanbanScan where IsOrdered = 0 and Region = ? and LogisticCenterCode = ? Order by Supplier ", new object[] { region, lccode });
            }
        }

        //public DateTime GetOrderTime(DateTime baseTime, KanbanScan scan) {
        //    return DateTime.Now;
        //}
        [Transaction(TransactionMode.Requires)]
        public IList<KanbanScan> OrderCard(string region, string supplier, string lccode, string orderTime, string scanIds, User orderUser, ref string orderNos)
        {
            DateTime dt = DateTime.Now;
            IList<KanbanScan> result = new List<KanbanScan>();

            string hql = "from KanbanScan where IsOrdered = ? and Id in (" + scanIds + ")";
            IList<object> paraList = new List<object>();
            paraList.Add(false);

            IList<KanbanScan> scans = null;
            scans = this.genericMgr.FindAll<KanbanScan>(hql, paraList.ToArray());

            if (scans != null)
            {
                IDictionary<string, IList<KanbanScan>> flowScanDict = new Dictionary<string, IList<KanbanScan>>();
                //flow-item-qty
                IDictionary<string, IDictionary<string, decimal>> flowItemQty = new Dictionary<string, IDictionary<string, decimal>>();
                // 把同一个flow的kanbanscan放到一起
                IDictionary<string, KanbanScan> flowItemPONo = new Dictionary<string, KanbanScan>();
                //flow-item-iskit
                IDictionary<string, HashSet<string>> flowNonKitItems = new Dictionary<string, HashSet<string>>();
                IDictionary<string, HashSet<string>> flowKitItems = new Dictionary<string, HashSet<string>>();

                foreach (KanbanScan scan in scans)
                {
                    if (flowScanDict.ContainsKey(scan.Flow))
                    {
                        flowScanDict[scan.Flow].Add(scan);
                    }
                    else
                    {
                        IList<KanbanScan> scanSameFlow = new List<KanbanScan>();
                        scanSameFlow.Add(scan);
                        flowScanDict.Add(scan.Flow, scanSameFlow);
                    }

                    if (scan.IsKit)
                    {
                        if (flowKitItems.ContainsKey(scan.Flow))
                        {
                            flowKitItems[scan.Flow].Add(scan.Item);
                        }
                        else
                        {
                            HashSet<string> kitItems = new HashSet<string>();
                            kitItems.Add(scan.Item);

                            flowKitItems.Add(scan.Flow, kitItems);
                        }
                    }
                    else
                    {
                        if (flowNonKitItems.ContainsKey(scan.Flow))
                        {
                            flowNonKitItems[scan.Flow].Add(scan.Item);
                        }
                        else
                        {
                            HashSet<string> nonKitItems = new HashSet<string>();
                            nonKitItems.Add(scan.Item);

                            flowNonKitItems.Add(scan.Flow, nonKitItems);
                        }
                    }

                    if (!string.IsNullOrEmpty(scan.PONo) && !string.IsNullOrEmpty(scan.POLineNo)
                         && !flowItemPONo.ContainsKey(scan.Flow + scan.Item))
                    {
                        flowItemPONo.Add(scan.Flow + scan.Item, scan);
                    }

                    if (flowItemQty.ContainsKey(scan.Flow))
                    {
                        if (flowItemQty[scan.Flow].ContainsKey(scan.Item))
                        {
                            flowItemQty[scan.Flow][scan.Item] += scan.ScanQty;
                        }
                        else
                        {
                            flowItemQty[scan.Flow].Add(scan.Item, scan.ScanQty);
                        }
                    }
                    else
                    {
                        IDictionary<string, decimal> itemQty = new Dictionary<string, decimal>();
                        itemQty.Add(scan.Item, scan.ScanQty);

                        flowItemQty.Add(scan.Flow, itemQty);
                    }
                }

                //IDictionary<string, WorkingCalendar> regionWcCache = new Dictionary<string, WorkingCalendar>();
                foreach (KeyValuePair<string, IList<KanbanScan>> k in flowScanDict)
                {
                    IList<KanbanScan> sameFlow = k.Value;

                    FlowMaster fm = this.genericMgr.FindAll<FlowMaster>("from FlowMaster where Code = ? ", k.Key).SingleOrDefault();
                    if (fm == null)
                    {
                        KanbanScan res = new KanbanScan();
                        res.Flow = k.Key;
                        res.Ret = KBProcessCode.NoFlowMaster;
                        res.Msg = Resources.KB.KanbanCard.Process_NoFlowMaster;
                        result.Add(res);
                    }
                    else
                    {
                        //考虑跨天,fm.partyto等于kanbanscan上的region
                        //WorkingCalendar currentWorkcalendar = null;
                        //if (regionWcCache.ContainsKey(fm.PartyTo))
                        //{
                        //    currentWorkcalendar = regionWcCache[fm.PartyTo];
                        //}
                        //else
                        //{
                        //    currentWorkcalendar = workingCalendarMgr.GetWorkingCalendarByDatetimeAndRegion(dt, fm.PartyTo);
                        //    if (currentWorkcalendar == null)
                        //    {
                        //        KanbanScan res = new KanbanScan();
                        //        res.Region = region;
                        //        res.Ret = KBProcessCode.NoWorkingCalendar;
                        //        res.Msg = Resources.KB.KanbanCard.Process_NoWorkingCalendar + ":Region=" + fm.PartyTo + " Date=" + dt;
                        //        result.Add(res);
                        //        continue;
                        //    }
                        //    else {
                        //        regionWcCache.Add(fm.PartyTo, currentWorkcalendar);
                        //    }
                        //}

                        //交货时段开始和结束时间
                        //考虑跨天
                        //StartEndTime seTime = ForwardCalculateNextDeliveryForSpecificWorkCalendar(currentWorkcalendar, dt, fm, this.flowMgr.GetFlowStrategy(fm.Code));
                        //if (KanbanUtility.IsMinAndMaxDateTime(seTime.StartTime, seTime.EndTime))
                        //{
                        //    KanbanScan res = new KanbanScan();
                        //    res.Flow = k.Key;
                        //    res.Ret = KBProcessCode.NoFlowShiftDetail;
                        //    res.Msg = Resources.KB.KanbanCard.Process_NoFlowShiftDetail + ":WorkDate-" + currentWorkcalendar.WorkingDate.Date;
                        //    result.Add(res);
                        //}
                        //else
                        //{

                        //IList<string> items = flowItemQty[k.Key].Keys.ToList<string>();
                        OrderMaster newOrder = null;
                        if (flowNonKitItems.ContainsKey(k.Key))
                        {
                            IList<string> items = flowNonKitItems[k.Key].ToList<string>();
                            newOrder = orderMgr.TransferFlow2Order(fm, items, dt, true);
                        }
                        else if (flowKitItems.ContainsKey(k.Key))
                        {
                            newOrder = orderMgr.TransferFlow2Order(fm, null, dt, false);
                        }

                        if (newOrder != null)
                        {
                            string[] setimestring = orderTime.Split('~');
                            newOrder.IsAutoRelease = true;
                            newOrder.StartTime = DateTime.Parse(setimestring[0]);
                            newOrder.WindowTime = DateTime.Parse(setimestring[1]);

                            //non kit items
                            if (newOrder.OrderDetails != null)
                            {
                                for (int i = 0; i < newOrder.OrderDetails.Count; i++)
                                {
                                    newOrder.OrderDetails[i].RequiredQty = flowItemQty[k.Key][newOrder.OrderDetails[i].Item];
                                    newOrder.OrderDetails[i].OrderedQty = flowItemQty[k.Key][newOrder.OrderDetails[i].Item];

                                    //if (flowItemPONo.ContainsKey(k.Key + newOrder.OrderDetails[i].Item))
                                    //{
                                    //    newOrder.OrderDetails[i].PONo = flowItemPONo[k.Key + newOrder.OrderDetails[i].Item].PONo;
                                    //    newOrder.OrderDetails[i].POLineNo = flowItemPONo[k.Key + newOrder.OrderDetails[i].Item].POLineNo;
                                    //}
                                }
                            }

                            //kit items
                            if (flowKitItems.ContainsKey(k.Key))
                            {
                                IList<string> kitItems = flowKitItems[k.Key].ToList<string>();
                                foreach (string kitItem in kitItems)
                                {
                                    decimal kitQty = flowItemQty[k.Key][kitItem];
                                    IList<ItemKit> itemKits = itemMgr.GetKitItemChildren(kitItem);
                                    if (itemKits != null && itemKits.Count > 0)
                                    {
                                        foreach (ItemKit ik in itemKits)
                                        {
                                            OrderDetail od = new OrderDetail();
                                            od.Item = ik.ChildItem.Code;
                                            Item itemkkk = this.genericMgr.FindById<Item>(od.Item);
                                            od.ItemDescription = itemkkk.Description;
                                            od.Uom = itemkkk.Uom;
                                            od.BillAddress = newOrder.BillAddress;
                                            od.BillAddressDescription = newOrder.BillAddressDescription;
                                            //od.TraceCode = kitItem;
                                            od.RequiredQty = kitQty * ik.Qty;
                                            od.OrderedQty = kitQty * ik.Qty;

                                            //if (flowItemPONo.ContainsKey(k.Key + kitItem))
                                            //{
                                            //    od.PONo = flowItemPONo[k.Key + kitItem].PONo;
                                            //    od.POLineNo = flowItemPONo[k.Key + kitItem].POLineNo;
                                            //}

                                            newOrder.AddOrderDetail(od);
                                        }
                                    }
                                }
                            }

                            try
                            {
                                orderMgr.CreateOrder(newOrder);
                                orderNos = orderNos + newOrder.OrderNo + ";";
                                for (int j = 0; j < sameFlow.Count; j++)
                                {
                                    //设置kanbanscan上的结转数量,日期等
                                    sameFlow[j].IsOrdered = true;
                                    sameFlow[j].OrderQty = sameFlow[j].ScanQty;
                                    sameFlow[j].OrderUserId = orderUser.Id;
                                    sameFlow[j].OrderUserName = orderUser.Name;
                                    sameFlow[j].OrderTime = dt;
                                    sameFlow[j].OrderNo = newOrder.OrderNo;
                                    this.genericMgr.Update(sameFlow[j]);
                                    //设置看板卡状态到Ordered
                                    KanbanCard card = this.kanbanCardMgr.GetByCardNo(sameFlow[j].CardNo);
                                    card.Status = KBStatus.Ordered;
                                    this.genericMgr.Update(card);
                                    //如果看板卡是多轨,结转时累加多轨循环量,在扫描的时候处理
                                    //if (this.multiSupplyGroupMgr.IsKanbanCardMultiSupplyGroupEnabled(card))
                                    //{
                                    //    this.multiSupplyGroupMgr.KBAccumulateCycleAmount(card, sameFlow[j].OrderQty);
                                    //}
                                    //记录看板order事务
                                    this.kanbanTransactionMgr.RecordKanbanTransaction(card, orderUser, dt, KBTransType.Order);

                                    sameFlow[j].Ret = KBProcessCode.Ok;
                                    sameFlow[j].Msg = Resources.KB.KanbanCard.Process_Ok;
                                    result.Add(sameFlow[j]);
                                }
                            }
                            catch (BusinessException be)
                            {
                                KanbanScan res = new KanbanScan();
                                res.Flow = k.Key;
                                res.Ret = KBProcessCode.CreateOrderFailed;
                                res.Msg = Resources.KB.KanbanCard.Process_CreateOrderFailed + ":" + be.GetMessages()[0].GetMessageString();
                                result.Add(res);
                            }
                        }
                        else
                        {
                            KanbanScan res = new KanbanScan();
                            res.Flow = k.Key;
                            res.Ret = KBProcessCode.OrderTransferError;
                            res.Msg = Resources.KB.KanbanCard.Process_OrderTransferError;
                            result.Add(res);
                        }
                        //}
                    }
                }
            }

            return result;
        }


        public string OrderCard(string[] flowArray, DateTime[] windowTime)
        {
            string orderNo = string.Empty;

            for (int i = 0; i < flowArray.Count(); i++)
            {
                if (windowTime == null || windowTime.Count() == 0)
                {
                    throw new BusinessException("窗口时间不能为空。");
                }

                IList<KanbanScan> kanbanScanList = genericMgr.FindAll<KanbanScan>("from KanbanScan k where k.IsOrdered = 0 and k.Flow = ?", flowArray[i]);
                try
                {
                    if (string.IsNullOrEmpty(orderNo))
                    {
                        orderNo = kanbanScanOrderMgr.OrderCard(kanbanScanList, windowTime[i]);
                    }
                    else
                    {
                        orderNo += "," + kanbanScanOrderMgr.OrderCard(kanbanScanList, windowTime[i]);
                    }
                }
                catch (BusinessException ex)
                {
                    genericMgr.CleanSession();
                    MessageHolder.AddErrorMessage(ex.Message);
                }
                catch (Exception ex)
                {
                    MessageHolder.AddErrorMessage("路线{0}结转出错，错误信息：{1}", new string[] { flowArray[i], ex.Message });
                }

            }
            return orderNo;
        }
        #endregion

        //public StartEndTime ForwardCalculateNextDeliveryForSpecificWorkCalendar(WorkingCalendar nStart, DateTime currentOrderTime, FlowMaster matchFlow, Entity.SCM.FlowStrategy matchStrategy)
        //{
        //    //首先算出当前的交货时段
        //    StartEndTime startEndTime = new StartEndTime(DateTime.Parse("1900-01-01 00:00:00"), DateTime.Parse("2999-1-1 00:00:00"));
        //    string sysTime = systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.StandardWorkTime);
        //    for (int k = 0; k < 2; k++)
        //    {
        //        if (nStart != null)
        //        {
        //            IList<FlowShiftDetail> flowShiftDetailList = this.genericMgr.FindAll<FlowShiftDetail>("from FlowShiftDetail where Flow = ? and Shift = ? Order by Sequence asc",
        //                 new object[] { matchFlow.Code, nStart.Shift });
        //            if (flowShiftDetailList != null)
        //            {
        //                int currentOrderTimeIndex = -1;
        //                for (int i = 0; i < flowShiftDetailList.Count; i++)
        //                {
        //                    ShiftDetail sd = this.genericMgr.FindAll<ShiftDetail>("from ShiftDetail where Id = ?",
        //                        new object[] { flowShiftDetailList[i].ShiftDetailId }).SingleOrDefault();

        //                    DateTime startTime = KanbanUtility.ConvertDateTimeFromWorkDateAndShiftTime(nStart.WorkingDate, flowShiftDetailList[i].StartTime, sysTime);
        //                    DateTime endTime = KanbanUtility.ConvertDateTimeFromWorkDateAndShiftTime(nStart.WorkingDate, flowShiftDetailList[i].EndTime, sysTime);

        //                    if (currentOrderTime < startTime || (currentOrderTime > startTime && currentOrderTime < endTime))
        //                    {
        //                        currentOrderTimeIndex = i;
        //                        startEndTime = RecursiveForwardCalculateNextDeliveryForSpecificWorkCalendar(nStart, currentOrderTimeIndex, matchFlow.Code, (int)(matchStrategy.LeadTime));
        //                        break;
        //                    }
        //                }
        //                if (!KanbanUtility.IsMinAndMaxDateTime(startEndTime.StartTime, startEndTime.EndTime))
        //                {
        //                    break;
        //                }
        //                nStart = genericMgr.FindAll<WorkingCalendar>("from WorkingCalendar  where Type = ? and WorkingDate > ? and Region = ? and FlowStrategy = ?", new object[] { (int)com.Sconit.CodeMaster.WorkingCalendarType.Work, nStart.WorkingDate, matchFlow.PartyTo, CodeMaster.FlowStrategy.KB }).OrderBy(p => p.WorkingDate).FirstOrDefault();
        //            }
        //        }
        //    }
        //    return startEndTime;

        //}

        //private StartEndTime RecursiveForwardCalculateNextDeliveryForSpecificWorkCalendar(WorkingCalendar nStart, int startIndex, string flow, int leftN)
        //{
        //    string sysTime = systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.StandardWorkTime);
        //    IList<FlowShiftDetail> flowShiftDetail = this.genericMgr.FindAll<FlowShiftDetail>("from FlowShiftDetail where Flow = ? and Shift = ? Order by Sequence",
        //         new object[] { flow, nStart.Shift });

        //    if (flowShiftDetail == null)
        //    {
        //        return new StartEndTime(DateTime.Parse("1900-01-01 00:00:00"), DateTime.Parse("2999-1-1 00:00:00"));
        //    }
        //    else
        //    {
        //        if (startIndex + leftN <= flowShiftDetail.Count - 1)
        //        {
        //            //在当天内
        //            ShiftDetail sd = this.genericMgr.FindAll<ShiftDetail>("from ShiftDetail where Id = ? and Shift = ? ",
        //                    new object[] { flowShiftDetail[startIndex + leftN].ShiftDetailId, flowShiftDetail[startIndex + leftN].Shift }).SingleOrDefault();
        //            DateTime startTime = KanbanUtility.ConvertDateTimeFromWorkDateAndShiftTime(nStart.WorkingDate, flowShiftDetail[startIndex + leftN].StartTime, sysTime);
        //            DateTime endTime = KanbanUtility.ConvertDateTimeFromWorkDateAndShiftTime(nStart.WorkingDate, flowShiftDetail[startIndex + leftN].EndTime, sysTime);

        //            return new StartEndTime(startTime, endTime);
        //        }
        //        else
        //        {
        //            //不在当天,找下一个工作日
        //            WorkingCalendar w = genericMgr.FindAll<WorkingCalendar>("from WorkingCalendar  where Type = ? and WorkingDate > ? and Region = ? ", new object[] { (int)com.Sconit.CodeMaster.WorkingCalendarType.Work, nStart.WorkingDate, nStart.Region }).OrderBy(p => p.WorkingDate).FirstOrDefault();
        //            if (w != null)
        //            {
        //                return RecursiveForwardCalculateNextDeliveryForSpecificWorkCalendar(w, 0, flow, leftN - 1 - (flowShiftDetail.Count - 1 - startIndex));
        //            }
        //            else
        //            {
        //                return new StartEndTime(DateTime.Parse("1900-01-01 00:00:00"), DateTime.Parse("2999-1-1 00:00:00"));
        //            }
        //        }
        //    }
        //}

        private void SetCardRetMsg(KanbanCard card, KBProcessCode ret, string msg)
        {
            card.Ret = ret;
            card.Msg = msg;
        }

        #region 导入
        [Transaction(TransactionMode.Requires)]
        public void ImportkanbanScanXls(Stream inputStream)
        {
            if (inputStream.Length == 0)
            {
                throw new BusinessException("Import.Stream.Empty");
            }
            BusinessException businessException = new BusinessException();
            HSSFWorkbook workbook = new HSSFWorkbook(inputStream);
            ISheet sheet = workbook.GetSheetAt(0);
            IEnumerator rows = sheet.GetRowEnumerator();
            ImportHelper.JumpRows(rows, 10);

            #region 列定义
            int colFlowCode = 1; // 路线代码
            int colTempCardNo = 2; // 临时看板卡号
            int colItemCode = 3; // 物料
            int colQty = 4; // 数量

            #endregion
            IList<KanbanScan> exactKanbanScanList = new List<KanbanScan>();
            int i = 0;
            while (rows.MoveNext())
            {
                i++;
                HSSFRow row = (HSSFRow)rows.Current;
                if (!ImportHelper.CheckValidDataRow(row, 0, 2))
                {
                    break;//边界
                }
                KanbanScan kanbanScan = new KanbanScan();
                string flowCode = string.Empty;
                string tempCardNo = string.Empty;
                string itemCode = string.Empty;
                int qty = 0;

                #region 读取数据

                #region 读取路线代码
                flowCode = ImportHelper.GetCellStringValue(row.GetCell(colFlowCode));
                if (string.IsNullOrWhiteSpace(flowCode))
                {
                    businessException.AddMessage(string.Format("第{0}行路线不能为空", i));
                }
                else
                {
                }
                kanbanScan.Flow = flowCode;
                #endregion

                #region 读取临时看板卡号
                tempCardNo = ImportHelper.GetCellStringValue(row.GetCell(colTempCardNo));
                if (string.IsNullOrWhiteSpace(tempCardNo))
                {
                    businessException.AddMessage(string.Format("第{0}行临时看板卡号不能为空", i));
                }
                else
                {


                }
                kanbanScan.TempKanbanCard = tempCardNo;
                #endregion
                #region 件号
                itemCode = ImportHelper.GetCellStringValue(row.GetCell(colItemCode));
                if (string.IsNullOrWhiteSpace(itemCode))
                {
                    businessException.AddMessage(string.Format("第{0}行数量不能为空", i));
                }
                else
                {

                }
                kanbanScan.Item = itemCode;
                #endregion

                #region 读取数量
                string readQty = ImportHelper.GetCellStringValue(row.GetCell(colQty));
                if (string.IsNullOrWhiteSpace(readQty))
                {
                    businessException.AddMessage(string.Format("第{0}行数量不能为空", i));
                }
                else
                {
                    if (!int.TryParse(readQty, out qty))
                    {
                        businessException.AddMessage(string.Format("第{0}行数量输入有误。", i));
                    }
                    if (qty < 0)
                    {
                        businessException.AddMessage(string.Format("第{0}行数量输入有误。", i));
                    }
                }
                kanbanScan.OrderQty = qty;
                #endregion

                exactKanbanScanList.Add(kanbanScan);

                #endregion
            }
            if (businessException.HasMessage)
            {
                throw businessException;
            }
            this.Scan(exactKanbanScanList);
        }
        #endregion

    }

    [Transactional]
    public class KanbanScanOrderMgrImpl : BaseMgr, IKanbanScanOrderMgr
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("Log.OrderMaster");
        public IGenericMgr genericMgr { get; set; }
        public IOrderMgr orderMgr { get; set; }
        public IKanbanCardMgr kanbanCardMgr { get; set; }
        public IKanbanTransactionMgr kanbanTransactionMgr { get; set; }
        private IPublishing proxy;
        public IPubSubMgr pubSubMgr { get; set; }
        public IEmailMgr emailMgr { get; set; }
        public NVelocityTemplateRepository vmReporsitory { get; set; }

        public string AutoGenAnDonOrder(User user)
        {
            try
            {
                var orderNos = this.genericMgr.FindAllWithNativeSql<string>("USP_Busi_GenAnDonOrder ?,?", new object[] { user.Id, user.FullName });
                if (orderNos != null && orderNos.Count > 0)
                {
                    string hqlOrderMaster = string.Empty;
                    string hqlOrderDetail = string.Empty;
                    object[] paras = new object[orderNos.Count];
                    for (int i = 0; i < orderNos.Count; i++)
                    {
                        if (i == 0)
                        {
                            hqlOrderMaster = "from OrderMaster om where om.OrderNo in(?";
                            hqlOrderDetail = "from OrderDetail od where od.OrderNo in(?";
                        }
                        else
                        {
                            hqlOrderMaster += ",?";
                            hqlOrderDetail += ",?";
                        }
                        paras[i] = orderNos[i];
                    }

                    hqlOrderMaster += ")";
                    hqlOrderDetail += ")";

                    var orderMasterList = this.genericMgr.FindAll<OrderMaster>(hqlOrderMaster, paras);
                    var orderDetailList = this.genericMgr.FindAll<OrderDetail>(hqlOrderDetail, paras);


                    foreach (var orderMaster in orderMasterList)
                    {
                        orderMaster.OrderDetails = orderDetailList.Where(o => o.OrderNo == orderMaster.OrderNo).ToList();
                        #region 发送打印
                        this.AsyncSendPrintData(orderMaster);
                        #endregion
                    }
                }
                return string.Empty;
            }
            catch(Exception ex)
            {
                string errorMessage = string.Empty;
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage = ex.InnerException.InnerException.Message;
                    }
                    else
                    {
                        errorMessage = ex.InnerException.Message;
                    }
                }
                else
                {
                    errorMessage = ex.Message;
                }
                log.Error(errorMessage);
                IList<ErrorMessage> errorMessageList = new List<ErrorMessage>();
                errorMessageList.Add(new ErrorMessage
                {
                    Template = NVelocityTemplateRepository.TemplateEnum.GenAnDonOrderFail,
                    Message = errorMessage,
                });
                SendErrorMessage(errorMessageList);
                return errorMessage;
            }
            
        }

        #region 异步打印
        public void SendPrintData(com.Sconit.Entity.EntityBase masterData)
        {
            try
            {
                PrintOrderMaster printOrderMstr = Mapper.Map<OrderMaster, PrintOrderMaster>((OrderMaster)masterData);
                proxy = pubSubMgr.CreateProxy();
                proxy.Publish(printOrderMstr);
            }
            catch (Exception ex)
            {
                pubSubLog.Error("Send data to print sevrer error:", ex);
            }
        }

        public void AsyncSendPrintData(com.Sconit.Entity.EntityBase masterData)
        {
            AsyncSend asyncSend = new AsyncSend(this.SendPrintData);
            asyncSend.BeginInvoke(masterData, null, null);
        }

        public delegate void AsyncSend(com.Sconit.Entity.EntityBase masterData);
        #endregion

        [Transaction(TransactionMode.Requires)]
        public string OrderCard(string scanIds, DateTime orderTime)
        {
            string orderNo = string.Empty;
            if (!string.IsNullOrWhiteSpace(scanIds))
            {
                StringBuilder hql = null;
                IList<object> parms = new List<object>();
                foreach (string overFlowId in scanIds.Split(','))
                {
                    if (hql == null)
                    {
                        hql = new StringBuilder("from KanbanScan where IsOrdered = 0 and Id in (?");
                    }
                    else
                    {
                        hql.Append(",?");
                    }
                    parms.Add(int.Parse(overFlowId));
                }
                hql.Append(")");

                IList<KanbanScan> kanbanScanList = genericMgr.FindAll<KanbanScan>(hql.ToString(), parms.ToArray());
                orderNo = OrderCard(kanbanScanList, orderTime);
            }
            return orderNo;
        }

        [Transaction(TransactionMode.Requires)]
        public string OrderCard(IList<KanbanScan> scans, DateTime orderTime)
        {
            DateTime dt = DateTime.Now;
            string orderNos = string.Empty;
            User orderUser = SecurityContextHolder.Get();

            if (scans != null)
            {
                #region 整理扫描记录
                IList<string> flowList = scans.Select(p => p.Flow).Distinct().ToList();
                var scanResult = from scan in scans
                                 group scan by new { Flow = scan.Flow, Item = scan.Item, PONo = string.IsNullOrEmpty(scan.PONo) ? string.Empty : scan.PONo, OpRef = scan.LogisticCenterCode }
                                     into r
                                     select new { r.Key.Flow, r.Key.Item, r.Key.PONo, r.Key.OpRef, Qty = r.Sum(t => t.ScanQty) };
                #endregion

                foreach (string flow in flowList)
                {
                    #region 转订单
                    FlowMaster flowMaster = this.genericMgr.FindById<FlowMaster>(flow);
                    var flowScanResultList = scanResult.Where(p => p.Flow == flow).ToList();
                    OrderMaster newOrder = orderMgr.TransferFlow2Order(flowMaster, false);
                    //string[] setimestring = orderTime.Split('~');
                    //newOrder.IsAsnUniqueReceive = false;
                    newOrder.IsAutoRelease = true;
                    newOrder.StartTime = DateTime.Now;
                    newOrder.WindowTime = orderTime;

                    //IList<LocationBinItem> locationBinItemList = genericMgr.FindAll<LocationBinItem>("from LocationBinItem l where l.Location = ?", newOrder.LocationTo);

                    #region 订单明细
                    foreach (var flowScanResult in flowScanResultList)
                    {

                        //if (Convert.ToBoolean(flowScanResult.IsKit))
                        //{
                        //    decimal kitQty = Convert.ToDecimal(flowScanResult.Qty.ToString());
                        //    IList<ItemKit> itemKits = genericMgr.FindAll<ItemKit>("from ItemKit k where k.KitItem = ?", flowScanResult.Item);
                        //    foreach (ItemKit itemKit in itemKits)
                        //    {
                        //        OrderDetail od = new OrderDetail();
                        //        Item item = genericMgr.FindById<Item>(itemKit.ChildItem);
                        //        od.Item = item.Code;
                        //        od.ItemDescription = item.Description;
                        //        od.Uom = item.Uom;
                        //        od.UnitCount = item.UnitCount;
                        //        //od.UnitCountDescription = item.UnitCountDescription;
                        //        od.RequiredQty = kitQty * itemKit.Qty;
                        //        od.OrderedQty = kitQty * itemKit.Qty;
                        //      //  od.TraceCode = itemKit.KitItem;

                        //        //var q = locationBinItemList.Where(p => p.Item == item.Code).FirstOrDefault();
                        //        //if (q != null)
                        //        //{
                        //        //    od.BinTo = q.Bin;
                        //        //}
                        //        newOrder.AddOrderDetail(od);
                        //    }
                        //}
                        //else
                        //{
                        OrderDetail od = new OrderDetail();
                        Item item = genericMgr.FindById<Item>(flowScanResult.Item);
                        od.Item = item.Code;
                        od.ReferenceItemCode = item.ReferenceCode;
                        od.ItemDescription = item.Description;
                        od.Uom = item.Uom;
                        od.UnitCount = item.UnitCount;
                        od.BinTo = flowScanResult.OpRef;
                        od.LocationFrom = newOrder.LocationFrom;
                        od.LocationFromName = newOrder.LocationFromName;
                        od.LocationTo = newOrder.LocationTo;
                        od.LocationToName = newOrder.LocationToName;
                        //od.UnitCountDescription = item.UnitCountDescription;
                        //od.PONo = string.IsNullOrEmpty(flowScanResult.PONo) ? string.Empty : flowScanResult.PONo;
                        od.RequiredQty = Convert.ToDecimal(flowScanResult.Qty.ToString());
                        od.OrderedQty = Convert.ToDecimal(flowScanResult.Qty.ToString());

                        //var q = locationBinItemList.Where(p => p.Item == item.Code).FirstOrDefault();
                        //if (q != null)
                        //{
                        //    od.BinTo = q.Bin;
                        //}

                        newOrder.AddOrderDetail(od);
                        //}

                    }
                    #endregion

                    #region 创建订单
                    try
                    {
                        #region  按照库格排序
                        newOrder.OrderDetails = newOrder.OrderDetails.OrderBy(p => p.BinTo).ToList();
                        #endregion
                        orderMgr.CreateOrder(newOrder);
                        orderNos += newOrder.OrderNo + ";";
                        IList<KanbanScan> kanbanScanList = scans.Where(s => s.Flow == flow).ToList();
                        foreach (KanbanScan kbScan in kanbanScanList)
                        {
                            //设置kanbanscan上的结转数量,日期等
                            kbScan.IsOrdered = true;
                            kbScan.OrderQty = kbScan.ScanQty;
                            kbScan.OrderUserId = orderUser.Id;
                            kbScan.OrderUserName = orderUser.Name;
                            kbScan.OrderTime = dt;
                            kbScan.OrderNo = newOrder.OrderNo;
                            this.genericMgr.Update(kbScan);
                            //设置看板卡状态到Ordered,临时看板不用
                            KanbanCard card = this.kanbanCardMgr.GetByCardNo(kbScan.CardNo);
                            if (card != null)
                            {
                                card.Status = KBStatus.Ordered;
                                this.genericMgr.Update(card);
                            }
                            //记录看板order事务
                            this.kanbanTransactionMgr.RecordKanbanTransaction(card, orderUser, dt, KBTransType.Order);
                        }
                    }
                    catch (BusinessException ex)
                    {
                        throw ex;
                    }
                    #endregion

                    #endregion
                }
            }

            return orderNos;
        }


        [Transaction(TransactionMode.Requires)]
        public string OrderCard(IList<KanbanScan> scans, IList<FlowMaster> flowMasterList, DateTime orderTime)
        {
            DateTime dt = DateTime.Now;
            string orderNos = string.Empty;
            User orderUser = SecurityContextHolder.Get();

            if (scans != null)
            {
                #region 整理扫描记录
                IList<string> flowList = scans.Select(p => p.Flow).Distinct().ToList();
                var scanResult = from scan in scans
                                 group scan by new { Flow = scan.Flow, Item = scan.Item, PONo = string.IsNullOrEmpty(scan.PONo) ? string.Empty : scan.PONo, OpRef = scan.LogisticCenterCode }
                                     into r
                                     select new { r.Key.Flow, r.Key.Item, r.Key.PONo, r.Key.OpRef, Qty = r.Sum(t => t.ScanQty) };
                #endregion

                foreach (string flow in flowList)
                {
                    #region 转订单
                    //FlowMaster flowMaster = this.genericMgr.FindById<FlowMaster>(flow);
                    FlowMaster flowMaster = flowMasterList.FirstOrDefault(f => f.Code == flow);
                    var flowScanResultList = scanResult.Where(p => p.Flow == flow).ToList();
                    OrderMaster newOrder = orderMgr.TransferFlow2Order(flowMaster, false);
                    //string[] setimestring = orderTime.Split('~');
                    //newOrder.IsAsnUniqueReceive = false;
                    newOrder.IsAutoRelease = true;
                    newOrder.StartTime = DateTime.Now;
                    newOrder.WindowTime = orderTime;

                    #region 订单明细
                    foreach (var flowScanResult in flowScanResultList)
                    {
                        OrderDetail od = new OrderDetail();
                        Item item = genericMgr.FindById<Item>(flowScanResult.Item);
                        od.Item = item.Code;
                        od.ReferenceItemCode = item.ReferenceCode;
                        od.ItemDescription = item.Description;
                        od.LocationFrom = newOrder.LocationFrom;
                        od.LocationFromName = newOrder.LocationFromName;
                        od.LocationTo = newOrder.LocationTo;
                        od.LocationToName = newOrder.LocationToName;
                        od.Uom = item.Uom;
                        od.BinTo = flowScanResult.OpRef;
                        od.UnitCount = item.UnitCount;
                        od.RequiredQty = Convert.ToDecimal(flowScanResult.Qty.ToString());
                        od.OrderedQty = Convert.ToDecimal(flowScanResult.Qty.ToString());
                        
                        newOrder.AddOrderDetail(od);

                    }
                    #endregion

                    #region 创建订单
                    try
                    {
                        #region  按照库格排序
                        newOrder.OrderDetails = newOrder.OrderDetails.OrderBy(p => p.BinTo).ToList();
                        #endregion
                        orderMgr.CreateOrder(newOrder);
                        orderNos += newOrder.OrderNo + ";";
                        IList<KanbanScan> kanbanScanList = scans.Where(s => s.Flow == flow).ToList();
                        foreach (KanbanScan kbScan in kanbanScanList)
                        {
                            //设置kanbanscan上的结转数量,日期等
                            kbScan.IsOrdered = true;
                            kbScan.OrderQty = kbScan.ScanQty;
                            kbScan.OrderUserId = orderUser.Id;
                            kbScan.OrderUserName = orderUser.Name;
                            kbScan.OrderTime = dt;
                            kbScan.OrderNo = newOrder.OrderNo;
                            this.genericMgr.Update(kbScan);
                            //设置看板卡状态到Ordered,临时看板不用
                            KanbanCard card = this.kanbanCardMgr.GetByCardNo(kbScan.CardNo);
                            if (card != null)
                            {
                                card.Status = KBStatus.Ordered;
                                this.genericMgr.Update(card);
                            }
                            //记录看板order事务
                            this.kanbanTransactionMgr.RecordKanbanTransaction(card, orderUser, dt, KBTransType.Order);
                        }
                    }
                    catch (BusinessException ex)
                    {
                        throw ex;
                    }
                    #endregion

                    #endregion
                }
            }

            return orderNos;
        }

        private void SendErrorMessage(IList<ErrorMessage> errorMessageList)
        {
            var distinctTemplates = errorMessageList.Select(t => t.Template).Distinct();
            foreach (var nVelocityTemplate in distinctTemplates)
            {
                MessageSubscirber messageSubscriber = genericMgr.FindById<MessageSubscirber>((int)nVelocityTemplate);
                var q_ItemErrors = errorMessageList.Where(t => (int)t.Template == (int)nVelocityTemplate).Take(messageSubscriber.MaxMessageSize);

                if (!string.IsNullOrWhiteSpace(messageSubscriber.Emails))
                {
                    IDictionary<string, object> data = new Dictionary<string, object>();
                    data.Add("Title", messageSubscriber.Description);
                    data.Add("ItemErrors", q_ItemErrors);
                    string content = vmReporsitory.RenderTemplate(nVelocityTemplate, data);
                    emailMgr.AsyncSendEmail(messageSubscriber.Description, content, messageSubscriber.Emails, MailPriority.High);
                }
            }
        }
    }
}
