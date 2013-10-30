using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Services.Transaction;
using NHibernate.Criterion;
using com.Sconit.Entity.KB;
using com.Sconit.Entity.ACC;
using com.Sconit.CodeMaster;
using com.Sconit.Entity.SCM;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.CUST;
using com.Sconit.Entity.PRD;
using com.Sconit.Utility;
using com.Sconit.Entity.Exception;
using AutoMapper;
using System.Data;
using com.Sconit.Entity;
using com.Sconit.Entity.SYS;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.Collections;

namespace com.Sconit.Service.Impl
{

    public class StartEndTime
    {
        public DateTime StartTime;
        public DateTime EndTime;

        public StartEndTime(DateTime s, DateTime e)
        {
            StartTime = s;
            EndTime = e;
        }
    }

    [Transactional]
    public class KanbanCardMgrImpl : BaseMgr, IKanbanCardMgr
    {
        #region 变量
        public IGenericMgr genericMgr { get; set; }
        //public IKanbanScanMgr kanbanScanMgr { get; set; }
        public IKanbanTransactionMgr kanbanTransactionMgr { get; set; }
        public IFlowMgr flowMgr { get; set; }
        public INumberControlMgr numberControlMgr { get; set; }
        public IWorkingCalendarMgr workingCalendarMgr { get; set; }
        public ISystemMgr systemMgr { get; set; }
        #endregion

        public KanbanCard GetByCardNo(string cardNo)
        {
            //return genericMgr.FindById<KanbanCard>(cardNo);
            KanbanCard card = genericMgr.FindAll<KanbanCard>("from KanbanCard where CardNo = ?", cardNo).FirstOrDefault();

            //get attributes from kanban flow
            //if (card != null)
            //{
            //    FlowMaster fm = genericMgr.FindAll<FlowMaster>("from FlowMaster where Code = ?", card.Flow).FirstOrDefault();
            //    FlowDetail fd = genericMgr.FindAll<FlowDetail>("from FlowDetail where Id = ?", card.FlowDetailId).FirstOrDefault();
            //    com.Sconit.Entity.SCM.FlowStrategy fs = flowMgr.GetFlowStrategy(card.Flow);

            //    if (fd != null && fm != null && fs != null)
            //    {
            //        card.Qty = fd.UnitCount;
            //        card.Container = fd.UnitCountDescription;
            //        card.QiTiaoBian = fs.QiTiaoBian;
            //        card.LocBin = fd.BinTo;
            //        card.Shelf = fd.Shelf;
            //    }
            //}

            return card;
        }

        public IList<KanbanCard> GetKanbanCards(string region, string supplier, string item, string multiSupply)
        {
            IList<KanbanCard> cards;
            if (multiSupply == string.Empty)
            {
                cards = this.genericMgr.FindAll<KanbanCard>("from KanbanCard where Region = ? and Supplier = ? and Item = ? ",
                    new object[] { region, supplier, item });
            }
            else
            {
                cards = this.genericMgr.FindAll<KanbanCard>("from KanbanCard where Region = ? and Supplier = ? and Item = ? and MultiSupplyGroup = ? ",
                    new object[] { region, supplier, item, multiSupply });
            }

            if (cards == null)
            {
                return new List<KanbanCard>();
            }
            else
            {
                return cards;
            }
        }

        #region 手工删除看板,每个路线明细
        [Transaction(TransactionMode.Requires)]
        public void DeleteManuallyByKanbanFlow(string[] idArray, string[] qtyArray)
        {
            IList<KanbanCard> cards = new List<KanbanCard>();
            if (idArray != null && idArray.Length > 0)
            {
                for (int i = 0; i < idArray.Length; i++)
                {
                    FlowDetail flowDetail = genericMgr.FindById<FlowDetail>(Convert.ToInt32(idArray[i]));

                    int delta = Convert.ToInt32(qtyArray[i]);
                    IList<KanbanCard> existKanbanCardList = genericMgr.FindAll<KanbanCard>("from KanbanCard c where c.FlowDetailId = ?", flowDetail.Id);
                    int totalCount = 0;
                    int existKanbanCardCount = 0;
                    if (existKanbanCardList != null && existKanbanCardList.Count > 0)
                    {
                        existKanbanCardCount = existKanbanCardList.Select(p => p.Number).Max();
                    }
                    if (existKanbanCardCount > delta)
                    {
                        totalCount = existKanbanCardCount - delta;
                    }
                    #region 删掉看板卡，更新总张数
                    if (totalCount > 0)
                    {
                        genericMgr.UpdateWithNativeQuery("Update KB_KanbanCard Set TotalCount = ?  where FlowDetId = ? ", new object[] { totalCount, flowDetail.Id });
                    }
                    genericMgr.FindAllWithNativeSql<KanbanCard>("Delete from KB_KanbanCard  where Number > ? and  FlowDetId = ?", new object[] { totalCount, flowDetail.Id });
                    #endregion

                    #region 更新路线上的看板数
                    flowDetail.CycloidAmount = totalCount;
                    genericMgr.Update(flowDetail);
                    #endregion
                }
            }
        }
        #endregion

        #region 手工创建看板,每个路线明细
        [Transaction(TransactionMode.Requires)]
        public IList<KanbanCard> AddManuallyByKanbanFlow(string flowCode, IList<Entity.SCM.FlowDetail> flowDetails, DateTime effDate, User calcUser, bool createNow)
        {
            IList<KanbanCard> cards = new List<KanbanCard>();

            FlowMaster fm = genericMgr.FindAll<FlowMaster>("from FlowMaster where Code = ?", flowCode).FirstOrDefault();
            com.Sconit.Entity.SCM.FlowStrategy fs = flowMgr.GetFlowStrategy(flowCode);
            Region r = genericMgr.FindById<Region>(fm.PartyTo);
            if (fm != null && fs != null && flowDetails != null)
            {

                foreach (FlowDetail fd in flowDetails)
                {
                    int delta = (int)(fd.OrderQty);
                    int currentSeq = 1;
                    int totalCount = 0;
                    totalCount = fd.CycloidAmount + delta;
                    currentSeq = fd.CycloidAmount + 1;

                    for (int i = 0; i < delta; i++)
                    {
                        if (currentSeq == 999)
                        {
                            throw new BusinessException("看板当前为999张,不能新增看板");
                        }
                        string nextSeqString = currentSeq.ToString();
                        if (nextSeqString.Length < 3)
                        {
                            nextSeqString = nextSeqString.PadLeft(3, '0');
                        }
                        KanbanCard card = GetKanbanCardFromKanbanFlow(fm, fs, fd, null, effDate, calcUser, createNow);
                        if (string.IsNullOrEmpty(card.OpRef))
                        {
                            throw new BusinessException("路线{0}件号{1}工位不能为空",fm.Code,fd.Item);
                        }
                        //KB + 工位 + 件号+ 整个工位的顺序号
                        //card.CardNo = "KB" + card.OpRef.Substring(5) + nextSeqString;
                        //张敏说工位+零件就不会重复，所以拿掉路线
                        card.CardNo = card.OpRef + card.Item + nextSeqString;
                        IList<KanbanCard> exKanbanList = genericMgr.FindAll<KanbanCard>("from KanbanCard k where k.CardNo = ?", card.CardNo);
                        if (exKanbanList != null && exKanbanList.Count > 0)
                        {
                            throw new BusinessException("看板卡{0}已存在!", card.CardNo);
                        }
                        card.Sequence = nextSeqString;

                        card.TotalCount = totalCount;
                        card.Number = currentSeq;
                        currentSeq++;
                        genericMgr.Create(card);
                        cards.Add(card);
                    }

                    #region 更新路线上的看板数
                    FlowDetail oldFlowDetail = genericMgr.FindById<FlowDetail>(fd.Id);
                    oldFlowDetail.CycloidAmount = totalCount;
                    genericMgr.Update(oldFlowDetail);

                    genericMgr.Update("Update KanbanCard Set TotalCount = ?  where FlowDetailId = ? ", new object[] { totalCount, fd.Id });
                    #endregion
                }
            }

            return cards;
        }
        #endregion

        #region 核算

        //private void GetMatchFlowAllInfo(CodeMaster.KBCalculation kbcalc, string region, string item, string location,
        //    ref FlowDetail matchDetail, ref FlowMaster matchFlow, ref com.Sconit.Entity.SCM.FlowStrategy matchStrategy,
        //    ref IDictionary<string, FlowDetail> flowDetailCache, ref IDictionary<string, FlowMaster> flowCache, ref IDictionary<string, com.Sconit.Entity.SCM.FlowStrategy> strategyCache,
        //    ref IList<KanbanCard> lerror)
        //{
        //    IList<FlowDetail> fd = null;
        //    if (kbcalc == KBCalculation.Normal)
        //    {
        //        //物流,非生产
        //        string hql = "from FlowDetail as d where d.Item = ? and exists (select 1 from FlowStrategy as s where s.Flow = d.Flow and s.KBCalc = ? ) and exists (select 1 from FlowMaster as f where f.Code = d.Flow and f.IsActive = ? and f.FlowStrategy = ? and (d.LocationTo = ? or (d.LocationTo is null and f.LocationTo = ?)))";
        //        fd = genericMgr.FindAll<FlowDetail>(hql, new object[] { item, (int)kbcalc, true, com.Sconit.CodeMaster.FlowStrategy.KB, location, location });
        //    }
        //    else if (kbcalc == KBCalculation.CatItem)
        //    {
        //        //生产
        //        fd = this.genericMgr.FindEntityWithNativeSql<FlowDetail>("select d.* from SCM_FlowDet d inner join SCM_FlowMstr f on d.flow = f.code inner join SCM_FlowStrategy s on d.flow = s.flow where d.item = '"
        //                                                             + item + "' and f.partyto = '" + region + "' and s.kbcalc = '" + (int)kbcalc + "'");
        //    }

        //    if (fd == null)
        //    {
        //        lerror.Add(CreateKanbanCardError(item, KBProcessCode.NoFlowDetail, Resources.KB.KanbanCard.Process_NoFlowDetail));
        //    }
        //    else if (fd.Count == 0)
        //    {
        //        lerror.Add(CreateKanbanCardError(item, KBProcessCode.NoFlowDetail, Resources.KB.KanbanCard.Process_NoFlowDetail));
        //    }
        //    else
        //    {
        //        //FlowMaster matchFlow = null;
        //        //FlowDetail matchDetail = null;
        //        //com.Sconit.Entity.SCM.FlowStrategy matchStrategy = null;
        //        foreach (FlowDetail flowDetail in fd)
        //        {
        //            matchDetail = flowDetail;


        //            if (!flowCache.ContainsKey(flowDetail.Flow))
        //            {
        //                //按照flowcode和region查找
        //                matchFlow = genericMgr.FindAll<FlowMaster>("from FlowMaster where FlowStrategy = ? and Code = ? and PartyTo = ?",
        //                    new object[] { CodeMaster.FlowStrategy.KB, flowDetail.Flow, region }).FirstOrDefault();
        //                //com.Sconit.Entity.SCM.FlowStrategy fs = flowMgr.GetFlowStrategy(flowCode);
        //                if (matchFlow == null)
        //                {
        //                    continue;
        //                }
        //                else
        //                {
        //                    flowCache.Add(flowDetail.Flow, matchFlow);
        //                }
        //            }
        //            else
        //            {
        //                matchFlow = flowCache[flowDetail.Flow];
        //            }


        //            if (!strategyCache.ContainsKey(flowDetail.Flow))
        //            {
        //                matchStrategy = flowMgr.GetFlowStrategy(flowDetail.Flow);
        //                if (matchStrategy == null)
        //                {
        //                    continue;
        //                }
        //                else
        //                {
        //                    strategyCache.Add(flowDetail.Flow, matchStrategy);
        //                }
        //            }
        //            else
        //            {
        //                matchStrategy = strategyCache[flowDetail.Flow];
        //            }

        //            //到这里master,detail,strategy都有了
        //            if (!flowDetailCache.ContainsKey(region + location + item))
        //            {
        //                flowDetailCache.Add(region + location + item, matchDetail);
        //            }
        //            break;
        //        }

        //        if (matchDetail == null)
        //        {
        //            lerror.Add(CreateKanbanCardError(item, KBProcessCode.NoFlowDetail, Resources.KB.KanbanCard.Process_NoFlowDetail));
        //        }
        //        else if (matchFlow == null)
        //        {
        //            lerror.Add(CreateKanbanCardError(item, KBProcessCode.NoFlowMaster, Resources.KB.KanbanCard.Process_NoFlowMaster));
        //        }
        //        else if (matchStrategy == null)
        //        {
        //            lerror.Add(CreateKanbanCardError(item, KBProcessCode.NoFlowStrategy, Resources.KB.KanbanCard.Process_NoFlowStrategy));
        //        }
        //    }
        //}

        //private void CompareCurrentCardsWithCalculated(Boolean useDesignatedTimeRange, StartEndTime startTime,
        //    WorkingCalendar nSub1, string region, FlowDetail matchDetail, FlowMaster matchFlow, com.Sconit.Entity.SCM.FlowStrategy matchStrategy,
        //    int calculatedKanbanNum, decimal calculateKanbanNumRaw, DateTime calculateMaxConsumeDate, decimal calculateMaxConsumeQty, string multiSupplyGroup, User calcUser, ref IList<KanbanCard> lnew, ref IList<KanbanCard> lerror)
        //{
        //    StartEndTime nSub1Last = null;
        //    if (useDesignatedTimeRange)
        //    {
        //        nSub1Last = startTime;
        //    }
        //    else
        //    {
        //        nSub1Last = BackCalculateLastDeliveryForSpecificWorkCalendar(nSub1, matchFlow, matchDetail, matchStrategy);
        //        if (KanbanUtility.IsMinAndMaxDateTime(nSub1Last.StartTime, nSub1Last.EndTime))
        //        {
        //            lerror.Add(CreateKanbanCardError(matchDetail.Item, KBProcessCode.NoFlowShiftDetail,
        //                Resources.KB.KanbanCard.Process_NoFlowShiftDetail + ":Flow-" + matchFlow.Code + " Date:" + nSub1.WorkingDate.Date));

        //            return;
        //        }
        //    }

        //    // 获取系统中当前流通的看板卡
        //    int currentKanbanNum = GetCurrentKanbanNum(region, matchFlow.PartyFrom, matchDetail.Item, multiSupplyGroup);

        //    KanbanCard resultCard = GetKanbanCardFromKanbanFlow(matchFlow, matchStrategy, matchDetail,
        //                multiSupplyGroup, nSub1Last.StartTime, calcUser, false);
        //    resultCard.KanbanNum = currentKanbanNum;
        //    resultCard.KanbanDeltaNum = calculatedKanbanNum - currentKanbanNum;
        //    resultCard.CalcKanbanNum = calculateKanbanNumRaw;
        //    resultCard.CalcConsumeDate = calculateMaxConsumeDate;
        //    resultCard.CalcConsumeQty = calculateMaxConsumeQty;

        //    if (resultCard.KanbanDeltaNum > 0)
        //    {
        //        SetCardRetMsg(resultCard, KBProcessCode.CalcNeedAdd, Resources.KB.KanbanCard.CalcNeedAdd);
        //    }
        //    else if (resultCard.KanbanDeltaNum < 0)
        //    {
        //        SetCardRetMsg(resultCard, KBProcessCode.CalcNeedFreeze, Resources.KB.KanbanCard.CalcNeedFreeze);
        //        resultCard.FreezeDate = nSub1Last.StartTime;
        //    }
        //    else
        //    {
        //        SetCardRetMsg(resultCard, KBProcessCode.Ok, Resources.KB.KanbanCard.Process_Ok);
        //    }

        //    lnew.Add(resultCard);
        //}

        private int GetCurrentKanbanNum(string region, string supplier, string item, string multiSupplyGroup)
        {
            int currentKanbanNum = 0;
            IList<KanbanCard> currentCards = GetKanbanCards(region, supplier, item, multiSupplyGroup);
            foreach (KanbanCard c in currentCards)
            {
                int seq = KanbanUtility.GetSeqFromKanbanSeq(c.Sequence);
                if (seq > currentKanbanNum)
                {
                    currentKanbanNum = seq;
                }
            }
            return currentKanbanNum;
        }

        // 根据输入区域,日用量起止时间,计算出看板，及看板变化张数，
        // 分别放到3个List：新增list，冻结list,错误list
        // 这时，还没有生成看板卡号，看板编号字段
        // startDate=2012-11-19 00:00:00, endDate=2012-11-23 00:00:00，需要将endDate改到2012-11-23 23:59:59
        //public string TryCalculate(string multiregion, string regionIn, string locationIn, DateTime startDate, DateTime endDate, int kbCalc, User calcUser)
        //{
        //    endDate = endDate.Add(new TimeSpan(23, 59, 59));
        //    CodeMaster.KBCalculation kbCalc2 = (CodeMaster.KBCalculation)kbCalc;

        //    IDictionary<string, IList<KanbanCard>> trials = new Dictionary<string, IList<KanbanCard>>();
        //    IList<KanbanCard> lnew = new List<KanbanCard>();
        //    //IList<KanbanCard> lfreeze = new List<KanbanCard>();
        //    //IList<KanbanCard> lunfreeze = new List<KanbanCard>();
        //    IList<KanbanCard> lerror = new List<KanbanCard>();

        //    // 需要新增
        //    trials.Add("new", lnew);
        //    // 需要冻结
        //    //trials.Add("freeze", lfreeze);
        //    // 需要更改投出时间和冻结时间
        //    //trials.Add("unfreeze", lunfreeze);
        //    // 有错的
        //    trials.Add("error", lerror);

        //    // 构建区域和库位列表
        //    IList<string> calcRegions = new List<string>();
        //    Boolean isMultiRegion = false;
        //    if (String.IsNullOrEmpty(multiregion))
        //    {
        //        Region r = this.genericMgr.FindAll<Region>("from Party where Code = ? ", regionIn).FirstOrDefault();
        //        if (String.IsNullOrEmpty(r.KbStandardCode) || String.IsNullOrEmpty(r.KbAlternativeCode))
        //        {
        //            lerror.Add(CreateKanbanCardError("Item", KBProcessCode.RegionKBCodeNotSet, Resources.KB.KanbanCard.Process_RegionKBCodeNotSet + ":" + r.Code));
        //        }
        //        calcRegions.Add(regionIn);
        //    }
        //    else
        //    {
        //        isMultiRegion = true;
        //        string[] regions = multiregion.Split(',');
        //        for (int ir = 0; ir < regions.Length; ir++)
        //        {
        //            Region r = this.genericMgr.FindAll<Region>("from Party where Code = ? ", regions[ir]).FirstOrDefault();
        //            if (String.IsNullOrEmpty(r.KbStandardCode) || String.IsNullOrEmpty(r.KbAlternativeCode))
        //            {
        //                lerror.Add(CreateKanbanCardError("Item", KBProcessCode.RegionKBCodeNotSet, Resources.KB.KanbanCard.Process_RegionKBCodeNotSet + ":" + r.Code));
        //            }
        //            calcRegions.Add(regions[ir]);
        //        }
        //    }

        //    if (lerror.Count == 0)
        //    {
        //        foreach (string region in calcRegions)
        //        {
        //            IList<string> calcLocations = new List<string>();
        //            if (kbCalc2 == KBCalculation.Normal)
        //            {
        //                //物流
        //                if (!isMultiRegion && !String.IsNullOrEmpty(locationIn))
        //                {
        //                    calcLocations.Add(locationIn);
        //                }
        //                else
        //                {
        //                    IList<Location> locs = genericMgr.FindAll<Location>("from Location where Region = ?", region);
        //                    if (locs != null)
        //                    {
        //                        foreach (Location loc in locs)
        //                        {
        //                            calcLocations.Add(loc.Code);
        //                        }
        //                    }
        //                }
        //            }
        //            else if (kbCalc2 == KBCalculation.CatItem)
        //            {
        //                //生产,不考虑库位
        //                calcLocations.Add("");
        //            }

        //            foreach (string location in calcLocations)
        //            {
        //                IList<ItemDailyConsume> itemDailyConsumes = new List<ItemDailyConsume>();

        //                // 根据kbcalc, flowdet, flowmaster取合适的itemdailyconsume
        //                //itemDailyConsumes = genericMgr.FindAll<ItemDailyConsume>
        //                //    ("from ItemDailyConsume where Location = ? and ConsumeDate >= ? and ConsumeDate <= ?", 
        //                //    new object[] { location, startDate, endDate });
        //                if (kbCalc2 == KBCalculation.Normal)
        //                {
        //                    //物流
        //                    string where = " and c.ConsumeDate >= '" + startDate + "'"
        //                    + " and c.ConsumeDate <= '" + endDate + "'" + " and c.Location = '" + location + "'"
        //                    + " and g.KBCalc = '" + kbCalc + "'";
        //                    itemDailyConsumes = this.genericMgr.FindEntityWithNativeSql<ItemDailyConsume>("select c.* from cust_itemdailyconsume c inner join scm_flowdet d on c.item=d.item inner join scm_flowmstr m on d.flow=m.code inner join scm_flowstrategy g on m.code=g.flow where c.location = isnull(d.locto,m.locto) and g.strategy =  " + (int)com.Sconit.CodeMaster.FlowStrategy.KB + " and m.isactive=1 " + where);
        //                }
        //                else if (kbCalc2 == KBCalculation.CatItem)
        //                {
        //                    //生产,不考虑库位
        //                    string where = " and c.ConsumeDate >= '" + startDate + "'"
        //                    + " and c.ConsumeDate <= '" + endDate + "'"
        //                    + " and g.KBCalc = '" + kbCalc + "'" + " and m.partyto = '" + region + "'";
        //                    itemDailyConsumes = this.genericMgr.FindEntityWithNativeSql<ItemDailyConsume>("select c.* from cust_itemdailyconsume c inner join scm_flowdet d on c.item=d.item inner join scm_flowmstr m on d.flow=m.code inner join scm_flowstrategy g on m.code=g.flow where g.strategy =  " + (int)com.Sconit.CodeMaster.FlowStrategy.KB + " and m.isactive=1 " + where);
        //                }

        //                //取上一个工作日历及最后一天的工作日历
        //                WorkingCalendar nSub1 = workingCalendarMgr.GetLastWorkingCalendar(startDate, region, 7);
        //                //WorkingCalendar nEnd = workingCalendarMgr.GetLastWorkingCalendar(endDate.AddDays(1), region, 7);

        //                //可以找到相应的工作日历
        //                if (itemDailyConsumes.Count > 0)
        //                {
        //                    //取region的看板计算方式字段
        //                    //Region r = this.genericMgr.FindAll<Region>("from Region as r where r.Code = ? ", region).FirstOrDefault();

        //                    //处理下一周不在日用量表中的零件，找出这些零件相关的看板，冻结
        //                    string allItems = "'x1y2z3z7u'";

        //                    //建立flowdetail,flowmaster,flowstrategy缓存
        //                    IDictionary<string, FlowDetail> flowDetailCache = new Dictionary<string, FlowDetail>();//item为key
        //                    IDictionary<string, FlowMaster> flowCache = new Dictionary<string, FlowMaster>();//flow为key
        //                    IDictionary<string, com.Sconit.Entity.SCM.FlowStrategy> strategyCache = new Dictionary<string, com.Sconit.Entity.SCM.FlowStrategy>();//flow为key
        //                    IDictionary<string, IList<FlowShiftDetail>> flowShiftDetailCache = new Dictionary<string, IList<FlowShiftDetail>>();//flow+shiftcode为key
        //                    IDictionary<string, int> shiftNumCache = new Dictionary<string, int>();//region + workdate.date为key

        //                    if (kbCalc2 == KBCalculation.Normal)
        //                    {
        //                        if (nSub1 != null)
        //                        {
        //                            //按多轨，正常零件分别计算
        //                            Dictionary<string, IList<ItemDailyConsume>> dailyConsumePerItem = new Dictionary<string, IList<ItemDailyConsume>>();
        //                            Dictionary<string, Dictionary<string, IList<ItemDailyConsume>>> dailyConsumePerItemMultiSupply = new Dictionary<string, Dictionary<string, IList<ItemDailyConsume>>>();
        //                            foreach (ItemDailyConsume idc in itemDailyConsumes)
        //                            {
        //                                allItems += ",'" + idc.Item + "'";

        //                                if (idc.MultiSupplyGroup != null && idc.MultiSupplyGroup.Trim() != string.Empty)
        //                                {
        //                                    //多轨零件
        //                                    if (dailyConsumePerItemMultiSupply.ContainsKey(idc.MultiSupplyGroup.Trim()))
        //                                    {
        //                                        if (dailyConsumePerItemMultiSupply[idc.MultiSupplyGroup.Trim()].ContainsKey(idc.Item))
        //                                        {
        //                                            dailyConsumePerItemMultiSupply[idc.MultiSupplyGroup.Trim()][idc.Item].Add(idc);
        //                                        }
        //                                        else
        //                                        {
        //                                            IList<ItemDailyConsume> multil2 = new List<ItemDailyConsume>();
        //                                            multil2.Add(idc);

        //                                            dailyConsumePerItemMultiSupply[idc.MultiSupplyGroup.Trim()].Add(idc.Item, multil2);
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        Dictionary<string, IList<ItemDailyConsume>> multid = new Dictionary<string, IList<ItemDailyConsume>>();
        //                                        IList<ItemDailyConsume> multil = new List<ItemDailyConsume>();
        //                                        multil.Add(idc);
        //                                        multid.Add(idc.Item, multil);

        //                                        dailyConsumePerItemMultiSupply.Add(idc.MultiSupplyGroup.Trim(), multid);
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    //正常零件
        //                                    if (dailyConsumePerItem.ContainsKey(idc.Item))
        //                                    {
        //                                        dailyConsumePerItem[idc.Item].Add(idc);
        //                                    }
        //                                    else
        //                                    {
        //                                        IList<ItemDailyConsume> l = new List<ItemDailyConsume>();
        //                                        l.Add(idc);
        //                                        dailyConsumePerItem.Add(idc.Item, l);
        //                                    }
        //                                }
        //                            }

        //                            //多轨
        //                            if (dailyConsumePerItemMultiSupply.Count > 0)
        //                            {
        //                                foreach (KeyValuePair<string, Dictionary<string, IList<ItemDailyConsume>>> k in dailyConsumePerItemMultiSupply)
        //                                {
        //                                    //处理每一个多轨组
        //                                    Dictionary<string, IList<ItemDailyConsume>> multiGroup = k.Value;
        //                                    //遍历所有多轨零件的所有日用量，计算最大看板数
        //                                    int calculatedKanbanNum = 0;
        //                                    foreach (KeyValuePair<string, IList<ItemDailyConsume>> itemk in multiGroup)
        //                                    {
        //                                        string item = itemk.Key;
        //                                        FlowDetail matchDetail = null;
        //                                        FlowMaster matchFlow = null;
        //                                        com.Sconit.Entity.SCM.FlowStrategy matchStrategy = null;
        //                                        GetMatchFlowAllInfo(kbCalc2, region, item, location, ref matchDetail, ref matchFlow, ref matchStrategy, ref flowDetailCache, ref flowCache, ref strategyCache, ref lerror);

        //                                        if (matchDetail != null && matchFlow != null && matchStrategy != null)
        //                                        {
        //                                            // do the calculation
        //                                            //int qtbM = KanbanUtility.ExtractMFromQiTiaoBian(matchStrategy.QiTiaoBian);
        //                                            //int qtbN = KanbanUtility.ExtractNFromQiTiaoBian(matchStrategy.QiTiaoBian);
        //                                            int qtbN = (int)(matchStrategy.LeadTime);

        //                                            foreach (ItemDailyConsume idc in itemk.Value)
        //                                            {
        //                                                //取当日生产时段数
        //                                                decimal workTimeNum = GetShiftWorkTimeNum(region, idc.ConsumeDate, ref shiftNumCache);
        //                                                if (workTimeNum > 0)
        //                                                {
        //                                                    //根据消耗当天，区域，计算出当天的flowshiftdetail个数(交货时段数)
        //                                                    int qtbM = CalculateM(matchFlow.Code, region, idc.ConsumeDate, ref flowShiftDetailCache);
        //                                                    if (qtbM > 0)
        //                                                    {
        //                                                        //int dailyKanbanNum = (int)Math.Ceiling((idc.Qty / matchDetail.UnitCount)
        //                                                        //* (Convert.ToDecimal((1 + qtbN) / qtbM) + Convert.ToDecimal(matchDetail.SafeStock / workTimeNum)));
        //                                                        int dailyKanbanNum = (int)Math.Ceiling(Convert.ToDecimal((idc.Qty * ((1 + qtbN) * workTimeNum + matchDetail.SafeStock * qtbM)) / (qtbM * workTimeNum * matchDetail.UnitCount)));

        //                                                        if (dailyKanbanNum > calculatedKanbanNum)
        //                                                        {
        //                                                            calculatedKanbanNum = dailyKanbanNum;
        //                                                        }

        //                                                    }
        //                                                    else
        //                                                    {
        //                                                        lerror.Add(CreateKanbanCardError(idc.Item, KBProcessCode.NoFlowShiftDetail,
        //                                                            Resources.KB.KanbanCard.Process_NoFlowShiftDetail + ":Flow-" + matchFlow.Code + " WorkDate:" + idc.ConsumeDate.Date));
        //                                                    }
        //                                                }
        //                                                //找不到工作日历或者是休息日则跳过
        //                                                //else
        //                                                //{
        //                                                //    lerror.Add(CreateKanbanCardError(idc.Item, KBProcessCode.NoWorkingCalendar,
        //                                                //            Resources.KB.KanbanCard.Process_NoWorkingCalendar + ":Flow-" + matchFlow.Code + " WorkDate:" + idc.ConsumeDate.Date));
        //                                                //}
        //                                            }
        //                                        }
        //                                    }

        //                                    if (calculatedKanbanNum == 0)
        //                                    {
        //                                        continue;
        //                                    }

        //                                    //已经计算到最大看板数，可以分别尝试创建看板
        //                                    foreach (KeyValuePair<string, IList<ItemDailyConsume>> itemk in multiGroup)
        //                                    {
        //                                        // 从cache中取detail,master,stratety
        //                                        if (flowDetailCache.ContainsKey(region + location + itemk.Key))
        //                                        {
        //                                            FlowDetail matchDetail = flowDetailCache[region + location + itemk.Key];

        //                                            if (flowCache.ContainsKey(matchDetail.Flow))
        //                                            {
        //                                                FlowMaster matchFlow = flowCache[matchDetail.Flow];

        //                                                if (strategyCache.ContainsKey(matchDetail.Flow))
        //                                                {
        //                                                    com.Sconit.Entity.SCM.FlowStrategy matchStrategy = strategyCache[matchDetail.Flow];

        //                                                    // 计算投出时间
        //                                                    //DateTime calculatedEffectiveDate = CalculateEffDate(wStart, matchFlow, matchDetail, matchStrategy);
        //                                                    //DateTime calculatedFreezeDate = CalculateFreezeDate(wEnd, matchFlow, matchDetail, matchStrategy);

        //                                                    // 比对已存在看板，生成新增，冻结，解冻列表
        //                                                    CompareCurrentCardsWithCalculated(false, null, nSub1, region, matchDetail, matchFlow, matchStrategy, calculatedKanbanNum, 0, DateTime.Now, 0, k.Key, calcUser, ref lnew, ref lerror);
        //                                                }
        //                                                else
        //                                                {
        //                                                    continue;
        //                                                }
        //                                            }
        //                                            else
        //                                            {
        //                                                continue;
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            continue;
        //                                        }
        //                                    }
        //                                } //最外层foreach
        //                            }

        //                            //正常
        //                            if (dailyConsumePerItem.Count > 0)
        //                            {
        //                                //零件最大日消耗量dictionary > 0

        //                                foreach (KeyValuePair<string, IList<ItemDailyConsume>> k in dailyConsumePerItem)
        //                                {
        //                                    //string[] keys = k.Key.Split(split.ToCharArray());
        //                                    string item = k.Key;

        //                                    FlowDetail matchDetail = null;
        //                                    FlowMaster matchFlow = null;
        //                                    com.Sconit.Entity.SCM.FlowStrategy matchStrategy = null;
        //                                    GetMatchFlowAllInfo(kbCalc2, region, item, location, ref matchDetail, ref matchFlow, ref matchStrategy, ref flowDetailCache, ref flowCache, ref strategyCache, ref lerror);

        //                                    if (matchDetail != null && matchFlow != null && matchStrategy != null)
        //                                    {
        //                                        // finally, do the calculation
        //                                        //int qtbM = KanbanUtility.ExtractMFromQiTiaoBian(matchStrategy.QiTiaoBian);
        //                                        //int qtbN = KanbanUtility.ExtractNFromQiTiaoBian(matchStrategy.QiTiaoBian);
        //                                        int qtbN = (int)(matchStrategy.LeadTime);

        //                                        int calculatedKanbanNum = 0;
        //                                        decimal calculateKanbanNumRaw = 0;
        //                                        DateTime calcaulteMaxConsumeDate = DateTime.Now;
        //                                        decimal calculateMaxConsumeQty = 0;
        //                                        //计算出每天看板数，取最大
        //                                        foreach (ItemDailyConsume idc in k.Value)
        //                                        {
        //                                            //取当日生产时段数
        //                                            decimal workTimeNum = GetShiftWorkTimeNum(region, idc.ConsumeDate, ref shiftNumCache);
        //                                            if (workTimeNum > 0)
        //                                            {
        //                                                //取当日交货时段数
        //                                                int qtbM = CalculateM(matchFlow.Code, region, idc.ConsumeDate, ref flowShiftDetailCache);
        //                                                if (qtbM > 0)
        //                                                {
        //                                                    //int dailyKanbanNum = (int)Math.Ceiling((idc.Qty / matchDetail.UnitCount)
        //                                                    //* (Convert.ToDecimal((1 + qtbN) / qtbM) + Convert.ToDecimal(matchDetail.SafeStock / workTimeNum)));
        //                                                    decimal dailyKanbanNumRaw = Convert.ToDecimal((idc.Qty * ((1 + qtbN) * workTimeNum + (matchDetail.SafeStock == null ? 0 : matchDetail.SafeStock.Value) * qtbM)) / (qtbM * workTimeNum * matchDetail.UnitCount));
        //                                                    int dailyKanbanNum = (int)Math.Ceiling(dailyKanbanNumRaw);

        //                                                    if (dailyKanbanNum > calculatedKanbanNum)
        //                                                    {
        //                                                        calculatedKanbanNum = dailyKanbanNum;
        //                                                        calculateKanbanNumRaw = dailyKanbanNumRaw;
        //                                                        calcaulteMaxConsumeDate = idc.ConsumeDate;
        //                                                        calculateMaxConsumeQty = idc.Qty;
        //                                                    }
        //                                                }
        //                                                else
        //                                                {
        //                                                    lerror.Add(CreateKanbanCardError(idc.Item, KBProcessCode.NoFlowShiftDetail,
        //                                                            Resources.KB.KanbanCard.Process_NoFlowShiftDetail + ":Flow-" + matchFlow.Code + " WorkDate:" + idc.ConsumeDate.Date));
        //                                                }
        //                                            }
        //                                            //找不到工作日历或者是休息日则跳过
        //                                            //else
        //                                            //{
        //                                            //    lerror.Add(CreateKanbanCardError(idc.Item, KBProcessCode.NoWorkingCalendar,
        //                                            //            Resources.KB.KanbanCard.Process_NoWorkingCalendar + ":Flow-" + matchFlow.Code + " WorkDate:" + idc.ConsumeDate.Date));
        //                                            //}
        //                                        }

        //                                        // 计算投出时间
        //                                        //DateTime calculatedEffectiveDate = CalculateEffDate(wStart, matchFlow, matchDetail, matchStrategy);
        //                                        //DateTime calculatedFreezeDate = CalculateFreezeDate(wEnd, matchFlow, matchDetail, matchStrategy);

        //                                        // 比对已存在看板，生成新增，冻结，解冻列表
        //                                        CompareCurrentCardsWithCalculated(false, null, nSub1, region, matchDetail, matchFlow, matchStrategy, calculatedKanbanNum, calculateKanbanNumRaw, calcaulteMaxConsumeDate, calculateMaxConsumeQty, string.Empty, calcUser, ref lnew, ref lerror);
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            lerror.Add(CreateKanbanCardError("No Item", KBProcessCode.NoStartOrEndWorkingCalendar, Resources.KB.KanbanCard.Process_NoStartOrEndWorkingCalendar));
        //                        }
        //                    }
        //                    else if (kbCalc2 == KBCalculation.CatItem)
        //                    {
        //                        //冲压，种别件
        //                        //假设所有冲压区域的零件，在路线上都有GroupNo字段，当前组织有一个零件的，使用物料编号作为组号
        //                        //假设冲压区域没有多轨零件

        //                        //计算每个零件的最大日用量
        //                        IDictionary<string, decimal> itemMaxConsume = new Dictionary<string, decimal>();
        //                        IDictionary<string, DateTime> itemMaxConsumeDate = new Dictionary<string, DateTime>();
        //                        foreach (ItemDailyConsume idc in itemDailyConsumes)
        //                        {
        //                            allItems += ",'" + idc.Item + "'";
        //                            //allItems += idc.Item + ",";

        //                            if (itemMaxConsume.ContainsKey(idc.Item))
        //                            {
        //                                if (itemMaxConsume[idc.Item] < idc.Qty)
        //                                {
        //                                    itemMaxConsume[idc.Item] = idc.Qty;
        //                                    itemMaxConsumeDate[idc.Item] = idc.ConsumeDate;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                itemMaxConsume.Add(idc.Item, idc.Qty);
        //                                itemMaxConsumeDate.Add(idc.Item, idc.ConsumeDate);
        //                            }
        //                        }

        //                        //计算每个零件的生产批量
        //                        //遍历一边，获得flow信息，并计算出每个组的最大日用量之和
        //                        IDictionary<string, decimal> groupSum = new Dictionary<string, decimal>();
        //                        foreach (KeyValuePair<string, decimal> k in itemMaxConsume)
        //                        {
        //                            string item = k.Key;
        //                            FlowDetail matchDetail = null;
        //                            FlowMaster matchFlow = null;
        //                            com.Sconit.Entity.SCM.FlowStrategy matchStrategy = null;
        //                            GetMatchFlowAllInfo(kbCalc2, region, item, location, ref matchDetail, ref matchFlow, ref matchStrategy, ref flowDetailCache, ref flowCache, ref strategyCache, ref lerror);

        //                            if (matchDetail != null && matchFlow != null && matchStrategy != null)
        //                            {
        //                                #region 没组的默认用自己的物料编号作为组号
        //                                if (string.IsNullOrEmpty(matchDetail.GroupNo))
        //                                {
        //                                    matchDetail.GroupNo = matchDetail.Item;
        //                                }
        //                                #endregion

        //                                if (groupSum.ContainsKey(matchDetail.GroupNo))
        //                                {
        //                                    groupSum[matchDetail.GroupNo] += k.Value;
        //                                }
        //                                else
        //                                {
        //                                    groupSum.Add(matchDetail.GroupNo, k.Value);
        //                                }
        //                            }
        //                        }

        //                        //计算看板张数
        //                        foreach (KeyValuePair<string, decimal> k in itemMaxConsume)
        //                        {
        //                            if (!flowDetailCache.ContainsKey(region + location + k.Key))
        //                            {
        //                                continue;
        //                            }
        //                            else
        //                            {
        //                                FlowDetail matchDetail = flowDetailCache[region + location + k.Key];

        //                                if (!flowCache.ContainsKey(matchDetail.Flow))
        //                                {
        //                                    continue;
        //                                }
        //                                else
        //                                {
        //                                    FlowMaster matchFlow = flowCache[matchDetail.Flow];

        //                                    if (!strategyCache.ContainsKey(matchDetail.Flow))
        //                                    {
        //                                        continue;
        //                                    }
        //                                    else
        //                                    {
        //                                        com.Sconit.Entity.SCM.FlowStrategy matchStrategy = strategyCache[matchDetail.Flow];

        //                                        //int calculatedKanbanNum = (int)Math.Ceiling(Convert.ToDecimal(matchDetail.BatchSize * k.Value / groupSum[matchDetail.GroupNo] + k.Value * matchDetail.SafeStock)
        //                                        //                          / matchDetail.UnitCount);
        //                                        decimal calculateKanbanNumRaw = Convert.ToDecimal((matchDetail.BatchSize * k.Value + k.Value * matchDetail.SafeStock * groupSum[matchDetail.GroupNo]) / (groupSum[matchDetail.GroupNo] * matchDetail.UnitCount));
        //                                        int calculatedKanbanNum = (int)Math.Ceiling(calculateKanbanNumRaw);

        //                                        // 比对已存在看板，生成新增，冻结，解冻列表
        //                                        CompareCurrentCardsWithCalculated(true, new StartEndTime(DateTime.Now, DateTime.Now),
        //                                            nSub1, region, matchDetail, matchFlow, matchStrategy, calculatedKanbanNum, calculateKanbanNumRaw, itemMaxConsumeDate[k.Key], k.Value, string.Empty, calcUser, ref lnew, ref lerror);
        //                                    }
        //                                }
        //                            }
        //                        }

        //                    }// kbcalc = catitem

        //                    //根据allItems处理不在下周计划里的零件
        //                    if (lerror.Count == 0)
        //                    {
        //                        string findNotInNextWeekKanbanSql = "select k.* from kb_kanbancard k inner join SCM_FlowStrategy s on k.flow = s.flow where k.region = '"
        //                                                          + region + "' and k.item not in (" + allItems + ") " + " and s.KBCalc = '" + (int)kbCalc2 + "'";
        //                        IList<KanbanCard> notInNextWeekPlanItems = this.genericMgr.FindEntityWithNativeSql<KanbanCard>(findNotInNextWeekKanbanSql);
        //                        if (notInNextWeekPlanItems != null)
        //                        {
        //                            IDictionary<string, Boolean> handledResultCards = new Dictionary<string, Boolean>();
        //                            for (int i = 0; i < notInNextWeekPlanItems.Count; i++)
        //                            {
        //                                if (!handledResultCards.ContainsKey(notInNextWeekPlanItems[i].Flow + notInNextWeekPlanItems[i].FlowDetailId))
        //                                {
        //                                    handledResultCards.Add(notInNextWeekPlanItems[i].Flow + notInNextWeekPlanItems[i].FlowDetailId, true);
        //                                    notInNextWeekPlanItems[i].MultiSupplyGroup = notInNextWeekPlanItems[i].MultiSupplyGroup == null ? string.Empty : notInNextWeekPlanItems[i].MultiSupplyGroup;
        //                                    int currentKanbanNum = GetCurrentKanbanNum(region, notInNextWeekPlanItems[i].Supplier, notInNextWeekPlanItems[i].Item, notInNextWeekPlanItems[i].MultiSupplyGroup);

        //                                    FlowDetail matchDetail = null;
        //                                    FlowMaster matchFlow = null;
        //                                    com.Sconit.Entity.SCM.FlowStrategy matchStrategy = null;
        //                                    GetMatchFlowAllInfo(kbCalc2, region, notInNextWeekPlanItems[i].Item, location, ref matchDetail, ref matchFlow, ref matchStrategy, ref flowDetailCache, ref flowCache, ref strategyCache, ref lerror);

        //                                    if (matchDetail != null && matchFlow != null && matchStrategy != null)
        //                                    {
        //                                        StartEndTime nSub1Last = null;
        //                                        if (kbCalc2 == KBCalculation.Normal)
        //                                        {
        //                                            nSub1Last = BackCalculateLastDeliveryForSpecificWorkCalendar(nSub1, matchFlow, matchDetail, matchStrategy);
        //                                            //DateTime.Parse("1900-01-01 00:00:00"), DateTime.Parse("2999-1-1 00:00:00")
        //                                            if (KanbanUtility.IsMinAndMaxDateTime(nSub1Last.StartTime, nSub1Last.EndTime))
        //                                            {
        //                                                //找不到flowshiftdetail或工作日历
        //                                                //lerror.Add(CreateKanbanCardError(idc.Item, KBProcessCode.NoFlowShiftDetail,
        //                                                //                Resources.KB.KanbanCard.Process_NoFlowShiftDetail + ":Flow-" + matchFlow.Code + " WorkDate:" + idc.ConsumeDate.Date));
        //                                                SetCardRetMsg(notInNextWeekPlanItems[i], KBProcessCode.NoFlowShiftDetail, Resources.KB.KanbanCard.Process_NoFlowShiftDetail);
        //                                                lerror.Add(notInNextWeekPlanItems[i]);
        //                                            }
        //                                            else
        //                                            {
        //                                                //冻结
        //                                                SetCardRetMsg(notInNextWeekPlanItems[i], KBProcessCode.CalcNotInPlanNeedFreeze, Resources.KB.KanbanCard.CalcNotInPlanNeedFreeze);
        //                                                notInNextWeekPlanItems[i].KanbanNum = currentKanbanNum;
        //                                                notInNextWeekPlanItems[i].KanbanDeltaNum = -1 * currentKanbanNum;
        //                                                notInNextWeekPlanItems[i].FreezeDate = nSub1Last.StartTime;
        //                                                notInNextWeekPlanItems[i].CalcKanbanNum = 0;
        //                                                notInNextWeekPlanItems[i].CalcConsumeDate = DateTime.Now;

        //                                                lnew.Add(notInNextWeekPlanItems[i]);
        //                                            }
        //                                        }
        //                                        else if (kbCalc2 == KBCalculation.CatItem)
        //                                        {
        //                                            nSub1Last = new StartEndTime(DateTime.Now, DateTime.Now);
        //                                            //冻结
        //                                            SetCardRetMsg(notInNextWeekPlanItems[i], KBProcessCode.CalcNotInPlanNeedFreeze, Resources.KB.KanbanCard.CalcNotInPlanNeedFreeze);
        //                                            notInNextWeekPlanItems[i].KanbanNum = currentKanbanNum;
        //                                            notInNextWeekPlanItems[i].KanbanDeltaNum = -1 * currentKanbanNum;
        //                                            notInNextWeekPlanItems[i].FreezeDate = nSub1Last.StartTime;
        //                                            notInNextWeekPlanItems[i].CalcKanbanNum = 0;
        //                                            notInNextWeekPlanItems[i].CalcConsumeDate = DateTime.Now;

        //                                            lnew.Add(notInNextWeekPlanItems[i]);
        //                                        }
        //                                    }
        //                                }

        //                            }
        //                        }
        //                    }
        //                }
        //            }

        //        } //foreach region
        //    } //lerror.count ==0

        //    //保存到中间结果表
        //    //生成batchno
        //    if (trials.Count > 0)
        //    {
        //        DateTime dt = DateTime.Now;
        //        string batchno = KanbanUtility.GetBatchNo(multiregion, regionIn, locationIn, startDate, endDate, kbCalc, calcUser);
        //        //首先删除同一个批号的
        //        this.genericMgr.Delete("from KanbanCalc where BatchNo = ?", batchno, NHibernate.NHibernateUtil.String);

        //        if (lerror.Count > 0)
        //        {
        //            for (int i = 0; i < lerror.Count; i++)
        //            {
        //                KanbanCalc calc = Mapper.Map<KanbanCard, KanbanCalc>(lerror[i]);
        //                calc.OpTime = DateTime.Now;
        //                if (calc.LastUseDate != null && calc.LastUseDate == DateTime.MinValue)
        //                {
        //                    calc.LastUseDate = DateTime.Parse("1900-1-1 00:00:00");
        //                }
        //                if (calc.EffectiveDate != null && calc.EffectiveDate == DateTime.MinValue)
        //                {
        //                    calc.EffectiveDate = DateTime.Parse("1900-1-1 00:00:00");
        //                }
        //                if (calc.FreezeDate != null && calc.FreezeDate == DateTime.MinValue)
        //                {
        //                    calc.FreezeDate = DateTime.Parse("1900-1-1 00:00:00");
        //                }
        //                calc.BatchNo = batchno;
        //                this.genericMgr.Create(calc);
        //            }
        //        }
        //        else
        //        {
        //            //string batchno = dt.ToString("yyyyMMddhhmmss");
        //            foreach (KeyValuePair<string, IList<KanbanCard>> k in trials)
        //            {
        //                IList<KanbanCard> l = k.Value;
        //                for (int i = 0; i < l.Count; i++)
        //                {
        //                    KanbanCalc calc = Mapper.Map<KanbanCard, KanbanCalc>(l[i]);
        //                    calc.OpTime = DateTime.Now;
        //                    if (calc.LastUseDate != null && calc.LastUseDate == DateTime.MinValue)
        //                    {
        //                        calc.LastUseDate = DateTime.Parse("1900-1-1 00:00:00");
        //                    }
        //                    if (calc.EffectiveDate != null && calc.EffectiveDate == DateTime.MinValue)
        //                    {
        //                        calc.EffectiveDate = DateTime.Parse("1900-1-1 00:00:00");
        //                    }
        //                    if (calc.FreezeDate != null && calc.FreezeDate == DateTime.MinValue)
        //                    {
        //                        calc.FreezeDate = DateTime.Parse("1900-1-1 00:00:00");
        //                    }
        //                    calc.BatchNo = batchno;
        //                    this.genericMgr.Create(calc);
        //                }
        //            }
        //        }

        //        return batchno;
        //    }
        //    else
        //    {
        //        return string.Empty;
        //    }
        //}

        // 重构的方法

        [Transaction(TransactionMode.Requires)]
        public void TryCalculate(string multiregion, DateTime startDate, DateTime endDate, int kbCalc, User calcUser)
        {
            #region 删除同批号的，按照区域，开始日期，结束日期
            string batchno = KanbanUtility.GetBatchNo(multiregion, null, null, startDate, endDate, kbCalc, calcUser);
            this.genericMgr.Delete("from KanbanCalc where BatchNo = ?", batchno, NHibernate.NHibernateUtil.String);
            genericMgr.FlushSession();
            genericMgr.CleanSession();
            #endregion

            List<KanbanCalc> kanbanCalcList = new List<KanbanCalc>();
            IList<KanbanCalc> lerror = new List<KanbanCalc>();

            IList<Region> calcRegions = new List<Region>();
            DateTime dateTimeNow = DateTime.Now;

            #region 检查区域的标准和特殊代码有没有设置
            string[] regions = multiregion.Split(',');
            for (int ir = 0; ir < regions.Length; ir++)
            {
                Region r = this.genericMgr.FindAll<Region>("from Region where Code = ? ", regions[ir]).FirstOrDefault();

                //if (String.IsNullOrEmpty(r.KbStandardCode) || String.IsNullOrEmpty(r.KbAlternativeCode))
                //{
                //    throw new BusinessException("没找到区域" + r.Code + "的看板代码和异常代码！");
                //}
                calcRegions.Add(r);
            }
            #endregion

            if (calcRegions.Count() > 0)
            {
                IList<ShiftMaster> shiftMasterList = genericMgr.FindAll<ShiftMaster>(); //先这样吧，全部的班次，反正也不多
                foreach (Region calcRegion in calcRegions)
                {
                    #region 把生效时间未到和待冻结的都删掉

                    genericMgr.FindAllWithNativeSql<KanbanCard>("Delete from KB_KanbanCard  where Region = ? and KBCalc = ? and (EffDate > ? or FreezeDate < ?) ", new object[] { calcRegion.Code, kbCalc, dateTimeNow, dateTimeNow });
                    genericMgr.FindAllWithNativeSql<KanbanCard>("Update KB_KanbanCard set TotalCount = k.TotalCount from (select FlowDetId, max(Number) as TotalCount from KB_KanbanCard where Region = ? and KBCalc = ? group by FlowDetId ) k where k.FlowDetId = KB_KanbanCard.FlowDetId and KB_KanbanCard.Region = ? and KB_KanbanCard.KBCalc = ?", new object[] { calcRegion.Code, kbCalc, calcRegion.Code, kbCalc });
                    genericMgr.FindAllWithNativeSql<FlowDetail>("Update SCM_FlowDet set CycloidAmount =  c.TotalCount from KB_KanbanCard c where c.FlowDetId = SCM_FlowDet.Id");
                    genericMgr.FindAllWithNativeSql<FlowDetail>("Update SCM_FlowDet set CycloidAmount = 0 where not exists (select 1 from KB_KanbanCard c where c.FlowDetId = SCM_FlowDet.Id) and CycloidAmount>0");
                    genericMgr.FlushSession();
                    genericMgr.CleanSession();

                    #endregion

                    #region 取此区域下所有的路线
                    //WorkingCalendar nSub1 = workingCalendarMgr.GetLastWorkingCalendar(startDate, region, 7);
                    IList<FlowMaster> flowList = genericMgr.FindEntityWithNativeSql<FlowMaster>(@"select c.* from scm_flowmstr c inner join scm_flowstrategy as g on c.Code = g.Flow where c.IsActive = ? and  c.PartyTo = ? and g.Strategy = ? and g.KBCalc = ?", new object[] { true, calcRegion.Code, (int)com.Sconit.CodeMaster.FlowStrategy.KB, kbCalc });
                    List<ItemDailyConsume> itemDailyConsumeList = new List<ItemDailyConsume>();
                    IList<WorkingCalendar> workingCalendatList = genericMgr.FindAll<WorkingCalendar>(" from WorkingCalendar w where w.FlowStrategy = ? and w.Region = ? and w.WorkingDate >= ? and w.WorkingDate <= ?", new object[] { (int)com.Sconit.CodeMaster.FlowStrategy.KB, calcRegion.Code, startDate, endDate });
                    IList<Party> partyList = genericMgr.FindAll<Party>();
                    IList<LocationBinItem> locationBinItemList = genericMgr.FindEntityWithNativeSql<LocationBinItem>(@"select l.* from md_locationbinitem l inner join md_location d on l.Location = d.Code where d.Region = ?", calcRegion.Code);
                    //IList<Item> kitItemList = genericMgr.FindAll<Item>(" from Item i where i.IsKit = ?",true);
                    IList<ItemTrace> itemTraceList = genericMgr.FindAll<ItemTrace>();
                    IList<Item> itemList = genericMgr.FindEntityWithNativeSql<Item>(@"select i.* from md_item i inner join scm_flowdet d on i.code = d.item inner join scm_flowmstr c on d.flow = c.code inner join scm_flowstrategy as g on c.Code = g.Flow where c.IsActive = ? and  c.PartyTo = ? and g.Strategy = ? and g.KBCalc = ?", new object[] { true, calcRegion.Code, (int)com.Sconit.CodeMaster.FlowStrategy.KB, kbCalc });
                    IList<Supplier> supplierList = genericMgr.FindAll<Supplier>();
                    IList<ItemKit> itemKitList = genericMgr.FindAll<ItemKit>();
                    #endregion

                    if (flowList != null && flowList.Count() > 0)
                    {
                        #region 获取站别日用量
                        if (kbCalc == (int)com.Sconit.CodeMaster.KBCalculation.Normal)
                        {
                            //采购的
                            IList<ItemDailyConsume> procurementItemDailyConsumeList = this.genericMgr.FindEntityWithNativeSql<ItemDailyConsume>(@"select max(ic.id) as id,ic.item,max(ic.itemdesc) as itemdesc, ic.location,max(qty) as qty,max(ic.consumedate) as consumedate,max(ic.substitutegroup) as substitutegroup,max(ic.multisupplygroup) as multisupplygroup,max(ic.createdate) as createdate,max(ic.originalqty) as originalqty
                                                                                from cust_itemdailyconsume ic 
                                                                                inner join scm_flowdet fd on ic.item = fd.item 
                                                                                inner join scm_flowmstr fm on fd.flow = fm.code 
                                                                                inner join scm_flowstrategy fg on fm.code = fg.flow
                                                                                inner join prd_workingcalendar pw on ic.ConsumeDate = pw.workingdate 
                                                                                where fm.isactive = ? and ic.Location = fm.LocTo and fm.Type = ?
                                                                                and fm.FlowStrategy = ? and fg.kbcalc = ? 
                                                                                and ic.ConsumeDate >= ? and ic.ConsumeDate <= ?
                                                                                and fm.PartyTo = ? and pw.type = ? and pw.flowstrategy = ? and pw.region = fm.PartyTo
                                                                                and ic.qty > 0
                                                                                group by ic.location,ic.item,pw.shift",
                                                                                new object[] { true, (int)com.Sconit.CodeMaster.OrderType.Procurement, (int)com.Sconit.CodeMaster.FlowStrategy.KB, kbCalc, startDate, endDate, calcRegion.Code, (int)com.Sconit.CodeMaster.WorkingCalendarType.Work, (int)com.Sconit.CodeMaster.FlowStrategy.KB });
                            //移库的，有些是用来源库位来算的，如KD件，有些使用目的库位来算的，如闭口订单,真变态
                            IList<ItemDailyConsume> transferItemDailyConsumeList = this.genericMgr.FindEntityWithNativeSql<ItemDailyConsume>(@"select max(ic.id) as id,ic.item,max(ic.itemdesc) as itemdesc, ic.location,max(qty) as qty,max(ic.consumedate) as consumedate,max(ic.substitutegroup) as substitutegroup,max(ic.multisupplygroup) as multisupplygroup,max(ic.createdate) as createdate,max(ic.originalqty) as originalqty
                                                                                from cust_itemdailyconsume ic 
                                                                                inner join scm_flowdet fd on ic.item = fd.item 
                                                                                inner join scm_flowmstr fm on fd.flow = fm.code 
                                                                                inner join scm_flowstrategy fg on fm.code = fg.flow
                                                                                inner join prd_workingcalendar pw on ic.ConsumeDate = pw.workingdate 
                                                                                where fm.isactive = ? and (ic.Location = fm.LocTo or ic.Location = fm.LocFrom) and fm.Type = ?
                                                                                and fm.FlowStrategy = ? and fg.kbcalc = ? 
                                                                                and ic.ConsumeDate >= ? and ic.ConsumeDate <= ?
                                                                                and fm.PartyTo = ? and pw.type = ? and pw.flowstrategy = ? and pw.region = fm.PartyTo
                                                                                and ic.qty > 0
                                                                                group by ic.location,ic.item,pw.shift",
                                                                                new object[] { true, (int)com.Sconit.CodeMaster.OrderType.Transfer, (int)com.Sconit.CodeMaster.FlowStrategy.KB, kbCalc, startDate, endDate, calcRegion.Code, (int)com.Sconit.CodeMaster.WorkingCalendarType.Work, (int)com.Sconit.CodeMaster.FlowStrategy.KB });

                            if (procurementItemDailyConsumeList != null && procurementItemDailyConsumeList.Count > 0)
                            {
                                itemDailyConsumeList.AddRange(procurementItemDailyConsumeList);
                            }

                            if (transferItemDailyConsumeList != null && transferItemDailyConsumeList.Count > 0)
                            {
                                itemDailyConsumeList.AddRange(transferItemDailyConsumeList);
                            }
                        }
                        else if (kbCalc == (int)com.Sconit.CodeMaster.KBCalculation.CatItem)
                        {
                            //生产,因为冲压送焊装的部分站别日用量在焊装，不是冲压，不考虑库位，按照生产线明细匹配
                            IList<ItemDailyConsume> productionItemDailyConsumeList = this.genericMgr.FindEntityWithNativeSql<ItemDailyConsume>(@"select ic.* from cust_itemdailyconsume ic 
                                                                                inner join scm_flowdet fd on ic.item = fd.item inner join scm_flowmstr fm on fd.flow = fm.code inner join scm_flowstrategy fg on fm.code = fg.flow
                                                                                where fm.isactive = ? and fm.FlowStrategy = ? and fg.kbcalc = ? and ic.ConsumeDate >= ? and ic.ConsumeDate <= ? and fm.PartyTo = ? and ic.Qty > 0",
                                                                                  new object[] { true, (int)com.Sconit.CodeMaster.FlowStrategy.KB, kbCalc, startDate, endDate, calcRegion.Code });
                            if (productionItemDailyConsumeList != null && productionItemDailyConsumeList.Count > 0)
                            {
                                itemDailyConsumeList.AddRange(productionItemDailyConsumeList);
                            }
                        }
                        #endregion

                    }

                    foreach (FlowMaster flow in flowList)
                    {
                        #region 物流
                        if (kbCalc == (int)com.Sconit.CodeMaster.KBCalculation.Normal)
                        {
                            IList<FlowShiftDetail> flowShiftDetailList = genericMgr.FindAll<FlowShiftDetail>(" from FlowShiftDetail f where f.Flow = ?", flow.Code);
                            IList<FlowDetail> flowDetailList = genericMgr.FindAll<FlowDetail>(" from FlowDetail d where d.Flow = ? and d.IsRejectInspect = ?", new object[] { flow.Code, false });
                            com.Sconit.Entity.SCM.FlowStrategy flowStrategy = genericMgr.FindById<com.Sconit.Entity.SCM.FlowStrategy>(flow.Code);

                            #region 冻结日期和投出日期时间，冻结取计算时段第一个时段不能交进来，投出取计算前一个工作日的倒数4个时段作为交货时段，先做倒数第4个时段好了，平均分配太麻烦
                            DateTime freezeDate = DateTime.Parse("2999-1-1 00:00:00");
                            DateTime effDate = DateTime.Parse("1900-1-1 00:00:00");
                            #endregion

                            #region 按路线明细计算看板
                            //   string matchLocation = flow.Type == com.Sconit.CodeMaster.OrderType.Procurement ? flow.LocationTo : flow.LocationFrom;
                            foreach (FlowDetail flowDetail in flowDetailList)
                            {
                                IList<ItemDailyConsume> flowDetailItemDailyConsumeList = itemDailyConsumeList.Where(p => p.Item == flowDetail.Item && (p.Location == flow.LocationFrom || p.Location == flow.LocationTo)).ToList();
                                //IList<KanbanCard> existKanbanCardList = genericMgr.FindAll<KanbanCard>("from KanbanCard c where c.FlowDetailId = ?", flowDetail.Id);
                                int existKanbanNum = flowDetail.CycloidAmount;

                                decimal calculatedKanbanNum = 0;
                                DateTime? calcDate = null;
                                Decimal calcQty = 0;
                                string groupNo = string.Empty;
                                int tiao = 0;

                                IList<string> shiftList = workingCalendatList.Select(i => i.Shift).Distinct().ToList();

                                #region 计算
                                if (flowDetailItemDailyConsumeList != null && flowDetailItemDailyConsumeList.Count > 0)
                                {
                                    foreach (ItemDailyConsume idc in flowDetailItemDailyConsumeList)
                                    {
                                        WorkingCalendar wc = workingCalendatList.Where(w => w.Region == flow.PartyTo && w.WorkingDate == idc.ConsumeDate).ToList().FirstOrDefault();

                                        if (wc != null && wc.Type == (int)com.Sconit.CodeMaster.WorkingCalendarType.Work)
                                        {
                                            int qtbM = flowShiftDetailList.Where(f => f.Shift == wc.Shift).Count();
                                            int workTimeNum = shiftMasterList.Where(s => s.Code == wc.Shift).FirstOrDefault().ShiftCount;
                                            if (qtbM > 0 && workTimeNum > 0 && flowDetail.UnitCount > 0)
                                            {
                                                decimal dailyKanbanNum = Convert.ToDecimal((idc.Qty * ((1 + flowStrategy.LeadTime) * workTimeNum + (flowDetail.SafeStock == null ? 0 : flowDetail.SafeStock.Value) * qtbM)) / (qtbM * workTimeNum * flowDetail.UnitCount));

                                                if (dailyKanbanNum > calculatedKanbanNum)
                                                {
                                                    calculatedKanbanNum = dailyKanbanNum;
                                                    calcQty = idc.Qty;
                                                    calcDate = idc.ConsumeDate;
                                                    groupNo = idc.MultiSupplyGroup;
                                                    tiao = qtbM;
                                                }
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    #region 本次无日用量的，应该所有的都冻结
                                    #endregion
                                }
                                #endregion

                                #region 记录计算结果
                                KanbanCalc kanbanCalc = new KanbanCalc();
                                kanbanCalc.Flow = flow.Code;
                                kanbanCalc.KBCalc = (com.Sconit.CodeMaster.KBCalculation)kbCalc;
                                kanbanCalc.CalcConsumeDate = calcDate;
                                kanbanCalc.CalcConsumeQty = calcQty;
                                kanbanCalc.CalcKanbanNum = calculatedKanbanNum;
                                kanbanCalc.Flow = flow.Code;
                                kanbanCalc.FlowDetailId = flowDetail.Id;
                                kanbanCalc.Item = flowDetail.Item;

                                Item item = itemList.Where(k => k.Code == kanbanCalc.Item).FirstOrDefault();
                                kanbanCalc.ItemDescription = item.Description;
                                //  kanbanCalc.Container = item.UnitCountDescription;
                                if (item.IsKit)
                                {
                                    kanbanCalc.IsKit = item.IsKit;
                                    kanbanCalc.KitCount = itemKitList.Where(p => p.KitItem == item.Code).Count();
                                }
                                ItemTrace itemTrace = itemTraceList.Where(t => t.Item == flowDetail.Item).FirstOrDefault();
                                if (itemTrace != null)
                                {
                                    kanbanCalc.IsTrace = true;
                                }
                                Supplier supplier = supplierList.Where(s => s.Code == flow.PartyFrom).FirstOrDefault();
                                //if (supplier != null)
                                //{
                                //    kanbanCalc.LogisticCenterCode = supplier.LogisticsCentre;
                                //    kanbanCalc.LogisticCenterName = supplier.LogisticsCentreName;
                                //}

                                //kanbanCalc.GroupNo = flowDetail.GroupNo;
                                //kanbanCalc.GroupDesc = flowDetail.GroupDesc;
                                kanbanCalc.KanbanNum = existKanbanNum;
                                kanbanCalc.Qty = flowDetail.UnitCount;
                                kanbanCalc.KanbanDeltaNum = (int)Math.Ceiling(kanbanCalc.CalcKanbanNum < 1 ? 1 : kanbanCalc.CalcKanbanNum) - existKanbanNum;
                                kanbanCalc.MultiSupplyGroup = groupNo;
                                kanbanCalc.Region = calcRegion.Code;
                                kanbanCalc.RegionName = calcRegion.Name;
                                kanbanCalc.Supplier = flow.PartyFrom;
                                var sup = partyList.Where(p => p.Code == flow.PartyFrom).FirstOrDefault();
                                kanbanCalc.SupplierName = sup.Name;
                                if (sup is Supplier)
                                {
                                    Supplier su = sup as Supplier;
                                    kanbanCalc.SupplierName = string.IsNullOrEmpty(su.ShortCode) ? su.Name.Substring(0, 4) : su.ShortCode;
                                }

                                // kanbanCalc.QiTiaoBian = flowStrategy.QiTiaoBian;
                                kanbanCalc.OpRef = flowDetail.BinTo;
                                kanbanCalc.Location = flow.LocationTo;
                                //if (tiao == 0)
                                //{
                                //    string[] tiaoArray = flowStrategy.QiTiaoBian.Split('-');
                                //    if (tiaoArray.Count() == 3)
                                //    {
                                //        tiao = Convert.ToInt32(tiaoArray[1]);
                                //    }
                                //}
                                kanbanCalc.Tiao = tiao;
                                kanbanCalc.Bian = Convert.ToInt32(flowStrategy.LeadTime);

                                LocationBinItem locationBinItem = locationBinItemList.Where(l => l.Location == flow.LocationTo && l.Item == flowDetail.Item).FirstOrDefault();
                                if (locationBinItem != null)
                                {
                                    kanbanCalc.LocBin = locationBinItem.Bin;
                                }
                                // kanbanCalc.Shelf = flowDetail.Shelf;
                                kanbanCalc.LastUseDate = DateTime.Parse("1900-1-1 00:00:00");
                                //if (!calcRegion.IsAutoExportKanBanCard)
                                //{
                                //    if (freezeDate == DateTime.Parse("2999-1-1 00:00:00") && kanbanCalc.KanbanDeltaNum < 0)
                                //    {
                                //        freezeDate = GetFreezeDate(startDate, flow, Convert.ToInt32(flowStrategy.LeadTime));
                                //    }
                                //    //  kanbanCalc.FreezeDate = freezeDate;
                                //    if (effDate == DateTime.Parse("1900-1-1 00:00:00") && kanbanCalc.KanbanDeltaNum > 0)
                                //    {
                                //        effDate = GetEffDate(startDate, flow, Convert.ToInt32(flowStrategy.LeadTime));
                                //    }
                                //}
                                //else
                                //{
                                if (freezeDate == DateTime.Parse("2999-1-1 00:00:00") && kanbanCalc.KanbanDeltaNum <= 0)
                                {
                                    freezeDate = dateTimeNow;
                                }
                                if (effDate == DateTime.Parse("1900-1-1 00:00:00") && kanbanCalc.KanbanDeltaNum >= 0)
                                {
                                    effDate = dateTimeNow;
                                }
                                //}
                                kanbanCalc.FreezeDate = freezeDate;
                                kanbanCalc.EffectiveDate = effDate;
                                kanbanCalc.OpTime = DateTime.Parse("1900-1-1 00:00:00");
                                kanbanCalc.BatchNo = batchno;

                                #region 处理双轨
                                if (!string.IsNullOrEmpty(groupNo))
                                {
                                    IList<KanbanCalc> kCalcList = kanbanCalcList.Where(k => k.MultiSupplyGroup == groupNo & k.Region == calcRegion.Code).ToList();

                                    if (kCalcList != null && kCalcList.Count > 0)
                                    {
                                        decimal gKanbanNum = kCalcList.Max(k => k.CalcKanbanNum);
                                        if (gKanbanNum > calculatedKanbanNum)
                                        {
                                            kanbanCalc.CalcKanbanNum = gKanbanNum;
                                            kanbanCalc.KanbanDeltaNum = (int)Math.Ceiling(gKanbanNum) - existKanbanNum;
                                        }
                                        else
                                        {
                                            foreach (KanbanCalc k in kCalcList)
                                            {
                                                k.CalcKanbanNum = calculatedKanbanNum;
                                                k.KanbanDeltaNum = (int)Math.Ceiling(calculatedKanbanNum) - existKanbanNum;
                                            }
                                        }
                                    }

                                }

                                kanbanCalcList.Add(kanbanCalc);
                                #endregion

                                #endregion

                                #region 记录路线明细上的日产量,交货趟次
                                flowDetail.MaxStock = kanbanCalc.CalcConsumeQty;
                                flowDetail.MrpWeight = kanbanCalc.Tiao;
                                genericMgr.Update(flowDetail);
                                #endregion

                            }
                            #endregion
                        }
                        #endregion

                        #region 生产
                        if (kbCalc == (int)com.Sconit.CodeMaster.KBCalculation.CatItem)
                        {
                            IList<FlowDetail> flowDetailList = genericMgr.FindAll<FlowDetail>(" from FlowDetail d where d.Flow = ?", flow.Code);
                            com.Sconit.Entity.SCM.FlowStrategy flowStrategy = genericMgr.FindById<com.Sconit.Entity.SCM.FlowStrategy>(flow.Code);

                            DateTime freezeDate = DateTime.Parse("2999-1-1 00:00:00");
                            DateTime effDate = DateTime.Parse("1900-1-1 00:00:00");

                            foreach (FlowDetail flowDetail in flowDetailList)
                            {
                                IList<ItemDailyConsume> flowDetailItemDailyConsumeList = itemDailyConsumeList.Where(p => p.Item == flowDetail.Item).ToList();
                                // IList<KanbanCard> existKanbanCardList = genericMgr.FindAll<KanbanCard>("from KanbanCard c where c.FlowDetailId = ?", flowDetail.Id);
                                int existKanbanNum = flowDetail.CycloidAmount;

                                decimal calculatedKanbanNum = 0;
                                Decimal calcQty = 0;

                                decimal batchSizeKanbanNum = 0;
                                decimal safeStockKanbanNum = 0;

                                if (flowDetailItemDailyConsumeList != null && flowDetailItemDailyConsumeList.Count > 0)
                                {
                                    if (flowDetail.UnitCount != 0)
                                    {
                                        decimal maxQty = flowDetailItemDailyConsumeList.Max(f => f.Qty);
                                        decimal batchsizeQty = flowDetail.BatchSize.HasValue ? flowDetail.BatchSize.Value : 0;

                                        #region 正常零件
                                        //if (string.IsNullOrEmpty(flowDetail.GroupNo))
                                        //{
                                        calcQty = maxQty;
                                        //}
                                        #endregion

                                        #region 种别件
                                        //else
                                        //{
                                        //    IList<FlowDetail> gFlowDetailList = flowDetailList.Where(f => f.GroupNo == flowDetail.GroupNo).ToList();
                                        //    decimal groupQty = 0;
                                        //    foreach (FlowDetail g in gFlowDetailList)
                                        //    {
                                        //        IList<ItemDailyConsume> l = itemDailyConsumeList.Where(p => p.Item == g.Item).ToList();
                                        //        if (l != null && l.Count > 0)
                                        //        {
                                        //            groupQty += l.Max(f => f.Qty);
                                        //        }
                                        //    }
                                        //    batchsizeQty = batchsizeQty * maxQty / groupQty;
                                        //}
                                        #endregion

                                        #region 计算


                                        decimal safeStock = flowDetail.SafeStock == null ? 0 : flowDetail.SafeStock.Value;
                                        //  decimal shiftType = string.IsNullOrEmpty(flowDetail.Shift) ? 1 : Convert.ToInt32(flowDetail.Shift);
                                        decimal shiftType = 1;
                                        //安全张数, 需求数*安全时数/(8*焊装班次）
                                        if (flowDetail.UnitCount < 2 * maxQty)
                                        {
                                            int safeQty = (int)Math.Round(maxQty * safeStock / (8 * shiftType), 0, MidpointRounding.AwayFromZero);
                                            safeStockKanbanNum = (int)Math.Round(safeQty / flowDetail.UnitCount, 0, MidpointRounding.AwayFromZero);
                                        }
                                        //批量张数
                                        batchSizeKanbanNum = (int)Math.Ceiling(batchsizeQty / flowDetail.UnitCount);

                                        calculatedKanbanNum = batchSizeKanbanNum + safeStockKanbanNum;
                                        calcQty = maxQty;
                                        #endregion
                                    }

                                }
                                else
                                {
                                    #region 本次无日用量的，应该所有的都冻结
                                    #endregion
                                }

                                #region 记录计算结果
                                KanbanCalc kanbanCalc = new KanbanCalc();
                                kanbanCalc.Flow = flow.Code;
                                kanbanCalc.KBCalc = (com.Sconit.CodeMaster.KBCalculation)kbCalc;
                                kanbanCalc.CalcConsumeQty = calcQty;
                                kanbanCalc.CalcKanbanNum = calculatedKanbanNum;

                                kanbanCalc.Flow = flow.Code;
                                kanbanCalc.FlowDetailId = flowDetail.Id;
                                kanbanCalc.Item = flowDetail.Item;

                                Item item = itemList.Where(k => k.Code == kanbanCalc.Item).FirstOrDefault();
                                kanbanCalc.ItemDescription = item.Description;
                                kanbanCalc.Container = item.Description;
                                if (item.IsKit)
                                {
                                    kanbanCalc.IsKit = item.IsKit;
                                    kanbanCalc.KitCount = itemKitList.Where(p => p.KitItem == item.Code).Count();
                                }
                                ItemTrace itemTrace = itemTraceList.Where(t => t.Item == flowDetail.Item).FirstOrDefault();
                                if (itemTrace != null)
                                {
                                    kanbanCalc.IsTrace = true;
                                }
                                //kanbanCalc.GroupNo = flowDetail.GroupNo;
                                //kanbanCalc.GroupDesc = flowDetail.GroupDesc;
                                kanbanCalc.KanbanNum = existKanbanNum;
                                kanbanCalc.Qty = flowDetail.UnitCount;
                                kanbanCalc.KanbanDeltaNum = (int)Math.Ceiling(calculatedKanbanNum) - existKanbanNum;
                                kanbanCalc.Region = calcRegion.Code;
                                kanbanCalc.RegionName = calcRegion.Name;
                                kanbanCalc.Supplier = flow.PartyFrom;
                                var sup = partyList.Where(p => p.Code == flow.PartyFrom).FirstOrDefault();
                                kanbanCalc.SupplierName = sup.Name;
                                if (sup is Supplier)
                                {
                                    Supplier su = sup as Supplier;
                                    kanbanCalc.SupplierName = string.IsNullOrEmpty(su.ShortCode) ? su.Name.Substring(0, 4) : su.ShortCode;
                                }

                                kanbanCalc.LastUseDate = DateTime.Parse("1900-1-1 00:00:00");
                                LocationBinItem locationBinItem = locationBinItemList.Where(l => l.Location == flow.LocationTo && l.Item == flowDetail.Item).FirstOrDefault();
                                if (locationBinItem != null)
                                {
                                    kanbanCalc.LocBin = locationBinItem.Bin;
                                }
                                kanbanCalc.BatchNo = batchno;
                                kanbanCalc.BatchKanbanNum = batchSizeKanbanNum;
                                kanbanCalc.SafeKanbanNum = safeStockKanbanNum;
                                kanbanCalc.Location = flow.LocationTo;
                                if (kanbanCalc.KanbanDeltaNum > 0)
                                {
                                    effDate = DateTime.Now;
                                }
                                if (kanbanCalc.KanbanDeltaNum < 0)
                                {
                                    freezeDate = DateTime.Now;
                                }
                                kanbanCalc.EffectiveDate = effDate;
                                kanbanCalc.FreezeDate = freezeDate;

                                kanbanCalcList.Add(kanbanCalc);

                                #endregion
                            }

                        }
                        #endregion
                    }
                }
            }

            #region 生成结果
            // kanbanCalcList.AddRange(lerror);
            if (kanbanCalcList.Count > 0)
            {
                foreach (KanbanCalc kc in kanbanCalcList)
                {
                    this.genericMgr.Create(kc);
                }
            }
            #endregion
        }

        [Transaction(TransactionMode.Requires)]
        public KanbanCard RotateKanbanCard(KanbanCard card, Boolean deleteAnother)
        {
            //Region r = genericMgr.FindAll<Region>("from Party where Code = ?", card.Region).FirstOrDefault();
            ////if (String.IsNullOrEmpty(r.KbStandardCode) || String.IsNullOrEmpty(r.KbAlternativeCode))
            ////{
            ////    throw new BusinessException("没有设置KB标准代码或异常代码,区域=" + r.Code);
            ////}

            //string oldCardNo = card.CardNo;
            //string oldSeq = card.Sequence;
            //string kbcode = oldSeq.Substring(0, 1);
            //string seqString = oldSeq.Substring(1, 3);
            //if (kbcode.Equals(r.KbStandardCode))
            //{
            //    card.CardNo = card.Flow + card.Item + r.KbAlternativeCode + seqString;
            //    card.Sequence = r.KbAlternativeCode + seqString;
            //}
            //else if (kbcode.Equals(r.KbAlternativeCode))
            //{
            //    card.CardNo = card.Flow + card.Item + r.KbStandardCode + seqString;
            //    card.Sequence = r.KbStandardCode + seqString;
            //}

            //KanbanCard card2 = GetByCardNo(card.CardNo);
            //if (card2 == null)
            //{
            //    card.NeedReprint = true;
            //    card.Status = com.Sconit.CodeMaster.KBStatus.Initial;
            //    this.genericMgr.Create(card);
            //    if (deleteAnother)
            //    {
            //        this.genericMgr.DeleteById<KanbanCard>(oldCardNo);
            //    }
            //}

            return card;
        }

        public DataSet GetUserKanbanCalcBatch(string multiregion, string region, string location, DateTime startDate, DateTime endDate, int kbCalc, User calcUser)
        {
            string batchno = KanbanUtility.GetBatchNo(multiregion, region, location, startDate, endDate, kbCalc, calcUser);
            return this.genericMgr.GetDatasetBySql("Select Flow,FlowDetId,Region,RegionName,LCCode,Item,ItemDesc,Supplier,SupplierName,Qty,Container,LocBin,Shelf,QiTiaoBian,MultiSupplyGroup,KanbanNum,KanbanDeltaNum,EffDate,FreezeDate,Msg from KB_KanbanCalc where BatchNo = @BatchNo",
                new System.Data.SqlClient.SqlParameter[] { new System.Data.SqlClient.SqlParameter("@BatchNo", batchno) });
        }

        public void UpdateBatch(string batchno, User calcUser, IDictionary<string, KanbanCalc> calcsIn)
        {
            IList<KanbanCalc> calcs = this.genericMgr.FindAll<KanbanCalc>("from KanbanCalc where BatchNo = ? ", batchno);
            if (calcs == null)
            {
                calcs = new List<KanbanCalc>();
            }
            for (int i = 0; i < calcs.Count; i++)
            {
                KanbanCalc oldCalc = calcs[i];

                if (calcsIn.ContainsKey(oldCalc.FlowDetailId.ToString()))
                {
                    KanbanCalc inCalc = calcsIn[oldCalc.FlowDetailId.ToString()];
                    if (oldCalc.KanbanDeltaNum == inCalc.KanbanDeltaNum)
                    //&& oldCalc.EffectiveDate == inCalc.EffectiveDate
                    //&& oldCalc.FreezeDate == inCalc.FreezeDate)
                    {
                        // do nothing
                    }
                    else
                    {
                        this.genericMgr.Update("Update KanbanCalc set KanbanDeltaNum = ? where BatchNo = ? and FlowDetailId = ? ",
                            new object[] { inCalc.KanbanDeltaNum, batchno, oldCalc.FlowDetailId });
                    }

                    calcsIn.Remove(oldCalc.FlowDetailId.ToString());
                }
                else
                {
                    this.genericMgr.Delete("from KanbanCalc where BatchNo = '" + batchno + "' and FlowDetailId = '" + oldCalc.FlowDetailId + "'");
                }
            }

            //处理新增
            //foreach (KeyValuePair<string, KanbanCalc> k in calcsIn) { 
            //    KanbanCalc newCalc = k.Value;
            //    FlowMaster fm = genericMgr.FindAll<FlowMaster>("from FlowMaster where Code = ?", newCalc.Flow).FirstOrDefault();
            //    com.Sconit.Entity.SCM.FlowStrategy fs = flowMgr.GetFlowStrategy(newCalc.Flow);
            //    FlowDetail fd = genericMgr.FindAll<FlowDetail>("from FlowDetail where Flow = ? and Id = ?", new object[] {newCalc.Flow, newCalc.FlowDetailId}).FirstOrDefault();
            //    KanbanCard newCard = GetKanbanCardFromKanbanFlow(
            //}
        }

        [Transaction(TransactionMode.Requires)]
        public void RunBatch(string multiregion, string region, string location, DateTime startDate, DateTime endDate, int kbCalc, User calcUser)
        {
            string batchno = KanbanUtility.GetBatchNo(multiregion, region, location, startDate, endDate, kbCalc, calcUser);
            IList<KanbanCalc> calcs = this.genericMgr.FindAll<KanbanCalc>("from KanbanCalc where BatchNo = ? ", batchno);
            IDictionary<string, Region> regionCache = new Dictionary<string, Region>();
            foreach (KanbanCalc calc in calcs)
            {
                if (!regionCache.ContainsKey(calc.Region))
                {
                    regionCache.Add(calc.Region, genericMgr.FindAll<Region>("from Party where Code = ?", calc.Region).FirstOrDefault());
                }

                if (calc.KanbanDeltaNum > 0)
                {
                    //add
                    int nextSeq = calc.KanbanNum + 1;
                    for (int i = 0; i < calc.KanbanDeltaNum; i++)
                    {
                        GenerateToDb(Mapper.Map<KanbanCalc, KanbanCard>(calc), true, nextSeq, calcUser, false, regionCache[calc.Region]);
                        nextSeq++;
                    }
                }
                else
                {
                    // freeze
                    int prevSeq = calc.KanbanNum;
                    for (int i = 0; i < (calc.KanbanDeltaNum * -1); i++)
                    {
                        string prevSeqString = prevSeq.ToString();
                        if (prevSeqString.Length == 1)
                        {
                            prevSeqString = "0" + prevSeqString;
                        }
                        string cardnoStandard = calc.Flow + calc.Item + prevSeqString;
                        //  string cardnoAlternative = calc.Flow + calc.Item + prevSeqString;
                        this.genericMgr.Update("Update KanbanCard set FreezeDate = ? where CardNo = ? ",
                            new object[] { calc.FreezeDate, cardnoStandard });
                        //this.genericMgr.Update("Update KanbanCard set FreezeDate = ? where CardNo = ? ",
                        //    new object[] { calc.FreezeDate, cardnoAlternative });

                        prevSeq--;
                    }
                }

                #region 更新看板卡上的最大张数
                this.genericMgr.Update("Update KanbanCard Set TotalCount = ?  where FlowDetailId = ? ", new object[] { calc.KanbanNum + calc.KanbanDeltaNum, calc.FlowDetailId });
                this.genericMgr.Update("Update FlowDetail Set CycloidAmount = ?  where Id = ? ", new object[] { calc.KanbanNum + calc.KanbanDeltaNum, calc.FlowDetailId });
                #endregion

                this.genericMgr.Delete(calc);
            }
        }

        [Transaction(TransactionMode.Requires)]
        public void RunBatch(string multiregion, string location, DateTime startDate, DateTime endDate, int kbCalc)
        {
            User user = SecurityContextHolder.Get();
            string batchno = KanbanUtility.GetBatchNo(multiregion, string.Empty, location, startDate, endDate, kbCalc, user);
            IList<KanbanCalc> kanbanCalcList = this.genericMgr.FindAll<KanbanCalc>("from KanbanCalc where BatchNo = ? ", batchno);
            IList<KanbanLost> kanbanLostList = genericMgr.FindAll<KanbanLost>();
            IList<Region> calcRegions = new List<Region>();
            string[] regions = multiregion.Split(',');
            for (int ir = 0; ir < regions.Length; ir++)
            {
                Region r = this.genericMgr.FindAll<Region>("from Region where Code = ? ", regions[ir]).FirstOrDefault();

                //if (!String.IsNullOrEmpty(r.KbStandardCode) && !String.IsNullOrEmpty(r.KbAlternativeCode))
                //{
                calcRegions.Add(r);
                //}
            }
            foreach (Region region in calcRegions)
            {
                IList<Int32> flowDetailIdList = kanbanCalcList.Where(p => p.Region == region.Code && p.FlowDetailId > 0 && p.KanbanDeltaNum != 0).Select(p => p.FlowDetailId).Distinct().ToList();

                #region 路线明细
                IList<FlowDetail> flowDetailList = flowMgr.GetFlowDetails(flowDetailIdList);
                #endregion
                if (flowDetailList != null && flowDetailList.Count > 0)
                {
                    foreach (FlowDetail fd in flowDetailList)
                    {
                        KanbanCalc calc = kanbanCalcList.Where(p => p.FlowDetailId == fd.Id).ToList().FirstOrDefault();
                        int totalCount = 0;

                        #region 新增
                        if (calc.KanbanDeltaNum > 0)
                        {
                            totalCount = calc.KanbanNum + calc.KanbanDeltaNum;
                            int nextSeq = calc.KanbanNum + 1;
                            if (nextSeq == 999)
                            {
                                continue;
                            }
                            for (int i = 0; i < calc.KanbanDeltaNum; i++)
                            {
                                #region 生成看板卡
                                KanbanCard kb = Mapper.Map<KanbanCalc, KanbanCard>(calc);
                                string nextSeqString = nextSeq.ToString();

                                if (nextSeqString.Length < 3)
                                {
                                    nextSeqString = nextSeqString.PadLeft(3, '0');
                                }
                                kb.Sequence = nextSeqString;
                                kb.CardNo = calc.Flow.Trim() + calc.Item.Trim() + nextSeqString;
                                kb.Sequence = nextSeqString;
                                var lostList = kanbanLostList.Where(p => p.CardNo == kb.CardNo).ToList();
                                if (lostList != null && lostList.Count > 0)
                                {
                                    kb.CardNo = calc.Flow.Trim() + calc.Item.Trim() + nextSeqString;
                                    kb.Sequence = nextSeqString;
                                }
                                var alternativeList = kanbanLostList.Where(p => p.CardNo == kb.CardNo).ToList();
                                if (alternativeList != null && alternativeList.Count > 0)
                                {
                                    throw new BusinessException("看板卡{0}正常和异常都在遗失列表", kb.CardNo);
                                }

                                kb.Number = nextSeq;
                                kb.TotalCount = totalCount;
                                kb.NeedReprint = true;
                                kb.Status = KBStatus.Initial;
                                kb.FreezeDate = DateTime.Parse("2999-1-1 00:00:00");
                                genericMgr.Create(kb);
                                nextSeq++;
                                #endregion
                            }
                        }
                        #endregion

                        #region 冻结
                        else
                        {
                            int prevSeq = calc.KanbanNum;
                            totalCount = calc.KanbanNum + calc.KanbanDeltaNum > 0 ? calc.KanbanNum + calc.KanbanDeltaNum : 0;
                            IList<KanbanCard> existKanbanCardList = genericMgr.FindAll<KanbanCard>("from KanbanCard k where k.FlowDetailId = ? and k.Number >?", new object[] { fd.Id, totalCount });
                            if (existKanbanCardList != null && existKanbanCardList.Count > 0)
                            {
                                foreach (KanbanCard kb in existKanbanCardList)
                                {
                                    kb.TotalCount = totalCount;
                                    kb.FreezeDate = calc.FreezeDate.Value;
                                    genericMgr.Update(kb);
                                }
                            }
                        }

                        #endregion

                        //#region 更新看板路线最大张数
                        //fd.CycloidAmount = totalCount;
                        //genericMgr.Update(fd);

                        //genericMgr.FindAllWithNativeSql<KanbanCard>("Update KB_KanbanCard set TotalCount = k.TotalCount from (select FlowDetId, max(Number) as TotalCount from KB_KanbanCard where Region = ? and KBCalc = ? group by FlowDetId ) k where k.FlowDetId = KB_KanbanCard.FlowDetId and KB_KanbanCard.Region = ? and KB_KanbanCard.KBCalc = ?", new object[] { region.Code, kbCalc, region.Code, kbCalc });
                        // genericMgr.Update("Update KanbanCard Set TotalCount = ?  where FlowDetailId = ? ", new object[] { totalCount, fd.Id });
                        //#endregion


                    }
                }

                #region 更新看板卡最大张数
                genericMgr.FlushSession();
                genericMgr.FindAllWithNativeSql<KanbanCard>("Update KB_KanbanCard set TotalCount = k.TotalCount from (select FlowDetId, max(Number) as TotalCount from KB_KanbanCard where Region = ? and KBCalc = ? group by FlowDetId ) k where k.FlowDetId = KB_KanbanCard.FlowDetId ", new object[] { region.Code, kbCalc });
                genericMgr.FindAllWithNativeSql<FlowDetail>("Update SCM_FlowDet set CycloidAmount = c.TotalCount from (select FlowDetId, max(Number) as TotalCount from KB_KanbanCard where Region = ? and KBCalc = ? group by FlowDetId )  c where c.FlowDetId = SCM_FlowDet.Id", new object[] { region.Code, kbCalc });
                #endregion
            }
            #region 删掉计算结果
            this.genericMgr.Delete("from KanbanCalc where BatchNo = ?", batchno, NHibernate.NHibernateUtil.String);
            #endregion
        }


        // 真正在数据库中创建看板
        [Transaction(TransactionMode.Requires)]
        public KanbanCard GenerateToDb(KanbanCard cardWithoutCardNoAndSeq, Boolean useNextSeq, int nextSeq, User calcUser, Boolean isAlternative, Region region)
        {
            int currentSeq = 0;
            if (cardWithoutCardNoAndSeq.MultiSupplyGroup == null)
            {
                cardWithoutCardNoAndSeq.MultiSupplyGroup = string.Empty;
            }

            //if (String.IsNullOrEmpty(region.KbStandardCode) || String.IsNullOrEmpty(region.KbAlternativeCode))
            //{
            //    throw new BusinessException("区域KB标准或异常代码未设置,区域=" + region.Code);
            //}

            //   string kbcode = isAlternative ? region.KbAlternativeCode : region.KbStandardCode;

            string nextSeqString = "";
            if (useNextSeq)
            {
                nextSeqString = nextSeq.ToString();
            }
            else
            {
                currentSeq = GetCurrentKanbanNum(cardWithoutCardNoAndSeq.Region, cardWithoutCardNoAndSeq.Supplier, cardWithoutCardNoAndSeq.Item, cardWithoutCardNoAndSeq.MultiSupplyGroup);
                nextSeqString = (currentSeq + 1).ToString();
            }
            if (currentSeq == 999)
            {
                throw new BusinessException("看板当前为999张,不能新增看板");
            }
            if (nextSeqString.Length < 3)
            {
                nextSeqString = nextSeqString.PadLeft(3, '0');
            }


            cardWithoutCardNoAndSeq.CardNo = cardWithoutCardNoAndSeq.Flow.Trim() + cardWithoutCardNoAndSeq.Item.Trim() + nextSeqString;
            cardWithoutCardNoAndSeq.Sequence = nextSeqString;
            cardWithoutCardNoAndSeq.TotalCount = currentSeq + 1;
            cardWithoutCardNoAndSeq.NeedReprint = true;
            this.genericMgr.Create(cardWithoutCardNoAndSeq);

            this.kanbanTransactionMgr.RecordKanbanTransaction(cardWithoutCardNoAndSeq, calcUser, DateTime.Now, KBTransType.Initialize);

            return cardWithoutCardNoAndSeq;
        }
        #endregion

        #region 冻结/删除
        public KanbanCard FreezeByCard(string cardNo, User freezeUser)
        {
            KanbanCard card = GetByCardNo(cardNo);
            //是否存在
            if (card == null)
            {
                card = new KanbanCard();
                card.CardNo = cardNo;
                card.OpTime = DateTime.Now;

                SetCardRetMsg(card, KBProcessCode.NonExistingCard, Resources.KB.KanbanCard.Process_NonExistingCard);
            }
            else
            {
                DateTime dt = DateTime.Now;
                card.FreezeDate = DateTime.Parse("1900-01-01 00:00:00");
                //card.Status = KBStatus.Loop;
                this.genericMgr.Update(card);

                card.OpTime = dt;
                this.kanbanTransactionMgr.RecordKanbanTransaction(card, freezeUser, dt, KBTransType.Freeze);

                SetCardRetMsg(card, KBProcessCode.Ok, Resources.KB.KanbanCard.Process_Ok);

            }

            return card;
        }

        public KanbanCard UnfreezeByCard(string cardNo, User freezeUser)
        {
            KanbanCard card = GetByCardNo(cardNo);
            //是否存在
            if (card == null)
            {
                card = new KanbanCard();
                card.CardNo = cardNo;
                card.OpTime = DateTime.Now;

                SetCardRetMsg(card, KBProcessCode.NonExistingCard, Resources.KB.KanbanCard.Process_NonExistingCard);
            }
            else
            {
                DateTime dt = DateTime.Now;

                card.FreezeDate = DateTime.Parse("2999-1-1 00:00:00");
                //card.Status = KBStatus.Loop;
                this.genericMgr.Update(card);

                card.OpTime = dt;
                this.kanbanTransactionMgr.RecordKanbanTransaction(card, freezeUser, dt, KBTransType.UnFreeze);

                SetCardRetMsg(card, KBProcessCode.Ok, Resources.KB.KanbanCard.Process_Ok);

            }

            return card;
        }

        #endregion

        #region 校验
        public bool IsEffectiveCard(KanbanCard card)
        {
            if (card != null)
            {
                DateTime dt = DateTime.Now;
                return (card.EffectiveDate <= dt);
            }

            return false;
        }

        public bool IsFrozenCard(KanbanCard card)
        {
            if (card != null)
            {
                DateTime dt = DateTime.Now;
                return (dt >= card.FreezeDate);
            }

            return false;
        }

        public bool IsScanned(KanbanCard card)
        {
            if (card != null)
            {
                return (card.Status == KBStatus.Scanned);
            }

            return false;
        }

        public bool HasPassOrderTime(KanbanCard card)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region 私有方法
        private void SetCardRetMsg(KanbanCard card, KBProcessCode ret, string msg)
        {
            card.Ret = ret;
            card.Msg = msg;
        }

        private KanbanCard CreateKanbanCardError(string item, KBProcessCode ret, string msg)
        {
            KanbanCard card = new KanbanCard();
            card.Flow = "";
            card.FlowDetailId = 0;
            card.Region = "";
            card.RegionName = "";
            card.ItemDescription = "";
            card.Supplier = "";
            card.SupplierName = "";
            card.Qty = 0;
            card.Status = KBStatus.Initial;
            card.KanbanDeltaNum = 0;
            card.KanbanNum = 0;
            card.NeedReprint = false;
            card.Item = item;
            SetCardRetMsg(card, ret, msg);
            return card;
        }

        private KanbanCard GetKanbanCardFromKanbanFlow(FlowMaster fm, Entity.SCM.FlowStrategy fs, FlowDetail fd, string multiSupplyGroup,
            DateTime effDate, User calcUser, Boolean createNow)
        {
            KanbanCard card = new KanbanCard();
            card.Flow = fm.Code;
            card.FlowDetailId = fd.Id;
            //card.KBCalc = fs.KBCalc;

            card.OpRef = fd.BinTo; //工位
            card.LocBin = fd.Dock; // 配送路径
            card.Region = fm.PartyTo;
            card.GroupDesc = fd.ProductionScan;//工位中文描述
            Region r = genericMgr.FindById<Region>(fm.PartyTo);
            card.RegionName = genericMgr.FindById<Region>(fm.PartyTo).Name;

            card.Supplier = fm.PartyFrom;
            Party s = genericMgr.FindById<Party>(fm.PartyFrom);
            if (s != null)
            {
                card.SupplierName = s.Name;
            }
            if (s is Supplier)
            {
                Supplier su = s as Supplier;
                card.SupplierName = string.IsNullOrEmpty(su.ShortCode) ? s.Name.Substring(0, 4) : su.ShortCode;
            }
            card.Item = fd.Item;
            card.ItemType = fd.ItemType;
            Item item = genericMgr.FindById<Item>(card.Item);
            if (item != null)
            {
                card.ItemDescription = item.Description;
                card.ReferenceItemCode = item.ReferenceCode;
                //零件类型
              
            }

            card.Status = KBStatus.Initial;
            card.EffectiveDate = effDate;
            card.FreezeDate = DateTime.Parse("2999-1-1 00:00:00");
            card.LastUseDate = DateTime.Parse("1900-1-1 00:00:00");
            card.NeedReprint = true;
            //上线包装
            card.Qty = fd.MinUnitCount;
            //容器代码
            card.Container = fd.Container;
            card.OpRefSequence = fd.OprefSequence; //看板号

            card.KanbanDeltaNum = 1;
            card.Location = fm.LocationTo;
            return card;
        }

        private decimal GetShiftWorkTimeNum(string region, DateTime workDate, ref IDictionary<string, int> shiftNumCache)
        {
            string k = region + workDate.Date.ToString("yyyyMMdd");
            if (shiftNumCache.ContainsKey(k))
            {
                return Convert.ToDecimal(shiftNumCache[k]);
            }
            else
            {
                IList<ShiftDetail> shiftDetailList = this.genericMgr.FindAll<ShiftDetail>(
                    "from ShiftDetail where Shift in (select Shift from WorkingCalendar where WorkingDate = ? and Region = ? and Type = ? and FlowStrategy = ?)", new object[] { workDate.Date, region, WorkingCalendarType.Work, CodeMaster.FlowStrategy.KB });
                if (shiftDetailList == null)
                {
                    return 0;
                }
                else
                {
                    shiftNumCache.Add(k, shiftDetailList.Count);

                    return Convert.ToDecimal(shiftDetailList.Count);
                }
            }
        }

        private decimal GetKBMinShiftWorkTimeNum(string region, DateTime startDate, DateTime endDate)
        {
            IList<ShiftDetail> shiftDetailList = this.genericMgr.FindAll<ShiftDetail>(
                "from ShiftDetail where Shift in (select Shift from WorkingCalendar where WorkingDate between ? and ? and Region = ? and Type = ? and FlowStrategy = ?)", new object[] { startDate.Date, endDate.Date, region, WorkingCalendarType.Work, CodeMaster.FlowStrategy.KB });
            Dictionary<string, int> shiftNumDic = new Dictionary<string, int>();
            foreach (ShiftDetail sd in shiftDetailList)
            {
                if (shiftNumDic.ContainsKey(sd.Shift))
                {
                    shiftNumDic[sd.Shift]++;
                }
                else
                {
                    shiftNumDic.Add(sd.Shift, 1);
                }
            }

            int min = 100;
            foreach (KeyValuePair<string, int> k in shiftNumDic)
            {
                if (k.Value < min)
                {
                    min = k.Value;
                }
            }

            return Convert.ToDecimal(min);
        }

        //private StartEndTime BackCalculateLastDeliveryForSpecificWorkCalendar(WorkingCalendar nEnd, FlowMaster matchFlow, FlowDetail matchDetail, Entity.SCM.FlowStrategy matchStrategy)
        //{
        //    return RecursiveBackCalculateLastDeliveryForSpecificWorkCalendar(nEnd, matchFlow.Code, (int)(matchStrategy.LeadTime));
        //}

        //private StartEndTime RecursiveBackCalculateLastDeliveryForSpecificWorkCalendar(WorkingCalendar nEnd, string flow, int leftN)
        //{
        //    //只要取nEnd当天的最后一个交货时段结束时间即可
        //    IList<FlowShiftDetail> flowShiftDetail = this.genericMgr.FindAll<FlowShiftDetail>("from FlowShiftDetail where Flow = ? and Shift = ? Order by Sequence",
        //         new object[] { flow, nEnd.Shift });

        //    if (flowShiftDetail == null)
        //    {
        //        return new StartEndTime(DateTime.Parse("1900-01-01 00:00:00"), DateTime.Parse("2999-1-1 00:00:00"));
        //    }
        //    else
        //    {
        //        if (flowShiftDetail.Count - 1 - leftN >= 0)
        //        {
        //            //在当天交货时段内，需要考虑跨天
        //            ShiftDetail sd = this.genericMgr.FindAll<ShiftDetail>("from ShiftDetail where Id = ? and Shift = ?", new object[] { flowShiftDetail[flowShiftDetail.Count - 1 - leftN].ShiftDetailId, flowShiftDetail[flowShiftDetail.Count - 1 - leftN].Shift }).FirstOrDefault();
        //            DateTime wd = nEnd.WorkingDate;
        //            if (sd.IsOvernight)
        //            {
        //                wd = wd.AddDays(1);
        //            }
        //            return new StartEndTime(KanbanUtility.ConvertDateTimeFromWorkDateAndShiftTime(wd, flowShiftDetail[flowShiftDetail.Count - 1 - leftN].StartTime, false),
        //               KanbanUtility.ConvertDateTimeFromWorkDateAndShiftTime(wd, flowShiftDetail[flowShiftDetail.Count - 1 - leftN].EndTime, true));
        //        }
        //        else
        //        {
        //            //不在当天,找上一个工作日
        //            WorkingCalendar w = workingCalendarMgr.GetLastWorkingCalendar(nEnd.WorkingDate, nEnd.Region, 7);
        //            if (w != null)
        //            {
        //                return RecursiveBackCalculateLastDeliveryForSpecificWorkCalendar(w, flow, (flowShiftDetail.Count - leftN) * -1);
        //            }
        //            else
        //            {
        //                return new StartEndTime(DateTime.Parse("1900-01-01 00:00:00"), DateTime.Parse("2999-1-1 00:00:00"));
        //            }
        //        }
        //    }
        //}

        //private DateTime CalculateEffDate(WorkingCalendar nSub1, FlowMaster matchFlow, FlowDetail matchDetail, Entity.SCM.FlowStrategy matchStrategy)
        //{
        //    IList<ShiftDetail> shiftDetailList = this.genericMgr.FindAll<ShiftDetail>(
        //        "from ShiftDetail where Shift = ?", nSub1.Shift);
        //    IList<FlowShiftDetail> flowShiftDetail = this.genericMgr.FindAll<FlowShiftDetail>("from FlowShiftDetail where Flow = ?", matchFlow.Code);

        //    //当前数据结构,只能解决供应商每天都有交货


        //    return DateTime.Now;
        //}

        //private int CalculateM(string flow, string region, DateTime consumeDate, ref IDictionary<string, IList<FlowShiftDetail>> flowShiftDetailCache)
        //{
        //    WorkingCalendar currentWc = workingCalendarMgr.GetWorkingCalendar(consumeDate, region);
        //    if (currentWc == null)
        //    {
        //        return 0;
        //    }
        //    else
        //    {
        //        string fskey = flow + currentWc.Shift;
        //        if (flowShiftDetailCache.ContainsKey(fskey))
        //        {
        //            return flowShiftDetailCache[fskey].Count;
        //        }
        //        else
        //        {
        //            IList<FlowShiftDetail> flowShiftDetail = this.genericMgr.FindAll<FlowShiftDetail>("from FlowShiftDetail where Flow = ? and Shift = ? Order by Sequence",
        //             new object[] { flow, currentWc.Shift });
        //            if (flowShiftDetail == null)
        //            {
        //                return 0;
        //            }
        //            else
        //            {
        //                flowShiftDetailCache.Add(fskey, flowShiftDetail);

        //                return flowShiftDetail.Count;
        //            }
        //        }
        //    }
        //}

        private string GetMultiSupplyGroup(string supplier, string item)
        {
            MultiSupplyItem i = genericMgr.FindAll<MultiSupplyItem>("from MultiSupplyItem where Supplier = ? and Item = ?", new object[] { supplier, item }).FirstOrDefault();

            if (i != null)
            {
                return i.GroupNo;
            }
            else
            {
                return null;
            }
        }

        //取冻结日期
        //private DateTime GetFreezeDate(DateTime startDate, FlowMaster flow, int leadTime)
        //{
        //    string sysTime = systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.StandardWorkTime);
        //    DateTime freezeDate = DateTime.Now;
        //    WorkingCalendar wc = genericMgr.FindAll<WorkingCalendar>("from WorkingCalendar w where w.WorkingDate < ? and w.Type = ? and w.FlowStrategy = ? and w.Region = ?", new object[] { startDate, (int)com.Sconit.CodeMaster.WorkingCalendarType.Work, (int)com.Sconit.CodeMaster.FlowStrategy.KB, flow.PartyTo }).OrderByDescending(p => p.WorkingDate).FirstOrDefault();
        //    if (wc != null)
        //    {
        //        IList<FlowShiftDetail> fsdList = genericMgr.FindAll<FlowShiftDetail>(" from FlowShiftDetail d where d.Flow = ? and d.Shift = ? ", new object[] { flow.Code, wc.Shift }).OrderByDescending(p => p.Sequence).ToList();
        //        if (fsdList.Count > 0)
        //        {
        //            if (fsdList.Count > leadTime - 1)
        //            {
        //                FlowShiftDetail fsd = leadTime - 1 > 0 ? fsdList[leadTime - 1] : fsdList[0];
        //                freezeDate = KanbanUtility.ConvertDateTimeFromWorkDateAndShiftTime(wc.WorkingDate, fsd.StartTime, sysTime);
        //            }
        //            else
        //            {
        //                return GetFreezeDate(wc.WorkingDate, flow, leadTime - 1 - fsdList.Count);
        //            }
        //        }
        //    }

        //    return freezeDate;
        //}

        //取生效日期，计算时段前一天的倒数第4个时段对应的窗口时间
        //private DateTime GetEffDate(DateTime startDate, FlowMaster flow, int leadTime)
        //{
        //    string sysTime = systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.StandardWorkTime);
        //    DateTime effDate = DateTime.Now;
        //    WorkingCalendar wc = genericMgr.FindAll<WorkingCalendar>("from WorkingCalendar w where w.WorkingDate < ? and w.Type = ? and w.FlowStrategy = ? and w.Region = ?", new object[] { startDate, (int)com.Sconit.CodeMaster.WorkingCalendarType.Work, (int)com.Sconit.CodeMaster.FlowStrategy.KB, flow.PartyTo }).OrderByDescending(p => p.WorkingDate).FirstOrDefault();
        //    if (wc != null)
        //    {
        //        IList<ShiftDetail> wsdList = genericMgr.FindAll<ShiftDetail>(" from ShiftDetail d where d.Shift = ?", wc.Shift).OrderByDescending(s => s.Sequence).ToList();
        //        ShiftDetail inShiftDetail = wsdList[3];   //取第4个交货时段
        //        IList<FlowShiftDetail> fsdList = genericMgr.FindAll<FlowShiftDetail>(" from FlowShiftDetail d where d.Flow = ? and d.Shift = ? and d.Sequence <= ?", new object[] { flow.Code, wc.Shift, inShiftDetail.Sequence }).OrderByDescending(p => p.Sequence).ToList();
        //        if (fsdList.Count > leadTime)
        //        {
        //            FlowShiftDetail fsd = fsdList[leadTime];
        //            effDate = KanbanUtility.ConvertDateTimeFromWorkDateAndShiftTime(wc.WorkingDate, fsd.StartTime, sysTime);
        //        }
        //        else
        //        {
        //            //好像直接调冻结那个日期方法就行了
        //            effDate = GetFreezeDate(wc.WorkingDate, flow, (leadTime - fsdList.Count) == 0 ? 1 : (leadTime - fsdList.Count));
        //        }
        //    }

        //    return effDate;
        //}
        #endregion

        public string ValidateKanbanCardFields(KanbanCard card)
        {
            FlowDetail fd = this.genericMgr.FindAll<FlowDetail>("from FlowDetail where Id = ? ", card.FlowDetailId).FirstOrDefault();
            FlowMaster fm = this.genericMgr.FindAll<FlowMaster>("from FlowMaster where Code = ? ", card.Flow).FirstOrDefault();
            com.Sconit.Entity.SCM.FlowStrategy fs = flowMgr.GetFlowStrategy(card.Flow);
            //Region p = genericMgr.FindAll<Region>("from Party where Code = ?", card.Region).FirstOrDefault();
            //Party s = genericMgr.FindAll<Party>("from Party where Code = ?", card.Supplier).FirstOrDefault();
            Item i = genericMgr.FindAll<Item>("from Item where Code = ?", card.Item).FirstOrDefault();
            // LocationBinItem l = genericMgr.FindAll<LocationBinItem>("from LocationBinItem  where Location = ? and Item = ?", new object[] { card.Location, card.Item }).FirstOrDefault();
            if (fd == null || fs == null || i == null)
            {
                return "找不到部分主数据";
            }
            else
            {
                string validText = "";

                if (card.Item != i.Code)
                {
                    validText += "原物料代码：" + card.Item + ",现在物料代码：" + i.Code + ",";
                }
                if (fm.PartyTo != card.Region)
                {
                    validText += "原区域代码: " + card.Region + ",现在区域代码:" + fm.PartyTo + ",";
                }
                if (fm.LocationTo != card.Location)
                {
                    validText += "原库位代码: " + card.Location + ",现在库位代码:" + fm.Code + ",";
                }
                if (fm.Type == com.Sconit.CodeMaster.OrderType.Procurement)
                {
                    Supplier s = genericMgr.FindById<Supplier>(fm.PartyFrom);
                    if (s.Code != card.Supplier)
                    {
                        validText += "原厂商代码: " + card.Supplier + ",现在厂商代码:" + s.Code + ",";
                    }
                    //if (s.LogisticsCentre != card.LogisticCenterCode)
                    //{
                    //    validText += "原物流中心代码: " + card.LogisticCenterCode + ",现在物流中心代码:" + s.LogisticsCentre + ",";
                    //}
                }
                //if (fs.QiTiaoBian != card.QiTiaoBian)
                //{
                //    validText += "原起跳便: " + card.QiTiaoBian + ",现在起跳便:" + fs.QiTiaoBian + ",";
                //}
                if (fd.MinUnitCount != card.Qty)
                {
                    validText += "原包装: " + card.Qty + ",现在包装:" + fd.UnitCount + ",";
                }
                if (fd.Container != card.Container)
                {
                    validText += "原包装规格: " + card.Container + ",现在包装规格:" + fd.UnitCountDescription + ",";
                }
                if (fd.BinTo != card.OpRef)
                {
                    validText += "原站别: " + card.OpRef + ",现在站别:" + fd.BinTo + ",";
                }
                //if (fd.Shelf != card.Shelf)
                //{
                //    validText += "原架位: " + card.Shelf + ",现在架位:" + fd.Shelf + ",";
                //}

                //if (l != null && l.Bin != card.LocBin)
                //{
                //    validText += "原库房架位: " + card.LocBin + ",现在库房架位:" + l.Bin + ",";
                //}

                return validText;
            }
        }

        public void SyncKanbanCardFields(KanbanCard card)
        {
            FlowDetail fd = this.genericMgr.FindAll<FlowDetail>("from FlowDetail where Id = ? ", card.FlowDetailId).FirstOrDefault();
            FlowMaster fm = this.genericMgr.FindAll<FlowMaster>("from FlowMaster where Code = ? ", card.Flow).FirstOrDefault();
            com.Sconit.Entity.SCM.FlowStrategy fs = flowMgr.GetFlowStrategy(card.Flow);
            //Region p = genericMgr.FindAll<Region>("from Party where Code = ?", card.Region).FirstOrDefault();
            //Party s = genericMgr.FindAll<Party>("from Party where Code = ?", card.Supplier).FirstOrDefault();
            Item i = genericMgr.FindAll<Item>("from Item where Code = ?", card.Item).FirstOrDefault();
            LocationBinItem l = genericMgr.FindAll<LocationBinItem>("from LocationBinItem  where Location = ? and Item = ?", new object[] { card.Location, card.Item }).FirstOrDefault();
            if (fd == null || fs == null || i == null)
            {
                throw new BusinessException("找不到部分主数据");
            }
            else
            {
                //if (l == null)
                //{
                //    throw new BusinessException("没有找到库位" + fm.LocationTo + ",物料编号" + fd.Item + "的库格");
                //}
                if (card.Item != i.Code)
                {
                    card.Item = i.Code;
                    card.ItemDescription = i.Description;
                }
                if (fm.PartyTo != card.Region)
                {
                    card.Region = fm.PartyTo;
                    card.RegionName = genericMgr.FindById<Region>(fm.PartyTo).Name;
                }
                if (fm.LocationTo != card.Location)
                {
                    card.Location = fm.LocationTo;
                }
                if (fm.Type == com.Sconit.CodeMaster.OrderType.Procurement)
                {
                    Supplier s = genericMgr.FindById<Supplier>(fm.PartyFrom);

                    if (s.Code != card.Supplier)
                    {
                        card.Supplier = s.Code;
                        card.SupplierName = string.IsNullOrEmpty(s.ShortCode) ? s.Name.Substring(0, 4) : s.ShortCode;
                    }
                    //if (s.LogisticsCentre != card.LogisticCenterCode)
                    //{
                    //    card.LogisticCenterCode = s.LogisticsCentre;
                    //}
                }
                //if (fs.QiTiaoBian != card.QiTiaoBian)
                //{
                //    card.QiTiaoBian = fs.QiTiaoBian;
                //}
                if (fd.UnitCount != card.Qty)
                {
                    card.Qty = fd.UnitCount;
                }
                if (fd.UnitCountDescription != card.Container)
                {
                    card.Container = fd.UnitCountDescription;
                }
                if (fd.BinTo != card.OpRef)
                {
                    card.OpRef = fd.BinTo;
                }
                //if (fd.Shelf != card.Shelf)
                //{
                //    card.Shelf = fd.Shelf;
                //}
                if (l != null && l.Bin != card.LocBin)
                {
                    card.LocBin = l.Bin;
                }
                this.genericMgr.Update(card);

            }
        }

        [Transaction(TransactionMode.Requires)]
        public void DeleteKanbanCard(KanbanCard card, User user)
        {
            this.genericMgr.Delete(card);
            this.kanbanTransactionMgr.RecordKanbanTransaction(card, user, DateTime.Now, KBTransType.Scan);
            this.genericMgr.Update("Update KanbanCard Set TotalCount = ?  where FlowDetailId = ? ", new object[] { card.TotalCount - 1, card.FlowDetailId });
            this.genericMgr.Update("Update FlowDetail Set CycloidAmount = ?  where Id = ? ", new object[] { card.TotalCount - 1, card.FlowDetailId });
        }

        public void DeleteFrozenCards(DateTime freezeDate)
        {
            this.genericMgr.Delete("from KanbanCard where FreezeDate < '" + freezeDate + "'");
        }

        #region 看板核算导入
        public void ImportKanbanCalc(Stream inputStream, string batchno, User currentUser)
        {

            if (inputStream.Length == 0)
            {
                throw new BusinessException("Import.Stream.Empty");
            }
            BusinessException businessException = new BusinessException();
            HSSFWorkbook workbook = new HSSFWorkbook(inputStream);

            ISheet sheet = workbook.GetSheetAt(0);
            IEnumerator rows = sheet.GetRowEnumerator();
            IDictionary<string, KanbanCalc> importedKanbanCalcs = new Dictionary<string, KanbanCalc>();
            ImportHelper.JumpRows(rows, 1);

            #region 列定义
            int colFlowDet = 0;//路线明细
            int colKanbanDeltaNum = 27;//变化张数
            #endregion
            int i = 1;
            while (rows.MoveNext())
            {
                i++;
                HSSFRow row = (HSSFRow)rows.Current;
                if (!ImportHelper.CheckValidDataRow(row, 1, 27))
                {
                    break;//边界
                }
                int flowDetId = 0;
                int kanbanDeltaNum = 0;
                #region 路线明细Id
                try
                {
                    flowDetId = Convert.ToInt32(row.GetCell(colFlowDet).ToString());
                }
                catch
                {
                    businessException.AddMessage("无法获取导入数据的路线ID,第{0}行。\t", i.ToString());
                    continue;
                }
                #endregion

                #region 变化张数
                try
                {
                    try
                    {
                        kanbanDeltaNum = Convert.ToInt32(row.GetCell(colKanbanDeltaNum).NumericCellValue);
                    }
                    catch (Exception)
                    {
                        kanbanDeltaNum = Convert.ToInt32(row.GetCell(colKanbanDeltaNum).ToString());
                    }
                }
                catch
                {
                    businessException.AddMessage("无法获取导入数据差异,第{0}行。\t", i.ToString());
                    continue;
                }
                #endregion

                if (importedKanbanCalcs.ContainsKey(flowDetId.ToString()))
                {
                    businessException.AddMessage("Import.Stream.DuplicateRow, id为{0}。\t", flowDetId.ToString());
                }
                else
                {
                    KanbanCalc calc = new KanbanCalc();
                    calc.BatchNo = batchno;
                    calc.FlowDetailId = flowDetId;
                    calc.KanbanDeltaNum = kanbanDeltaNum;
                    importedKanbanCalcs.Add(flowDetId.ToString(), calc);
                }
            }
            if (businessException.HasMessage)
            {
                throw businessException;
            }
            this.UpdateBatch(batchno, currentUser, importedKanbanCalcs);
        }
        #endregion

        #region 看板核算导入(生产)
        public void ImportKanbanCalc2(Stream inputStream, string batchno, User currentUser)
        {

            if (inputStream.Length == 0)
            {
                throw new BusinessException("Import.Stream.Empty");
            }
            BusinessException businessException = new BusinessException();
            HSSFWorkbook workbook = new HSSFWorkbook(inputStream);

            ISheet sheet = workbook.GetSheetAt(0);
            IEnumerator rows = sheet.GetRowEnumerator();
            IDictionary<string, KanbanCalc> importedKanbanCalcs = new Dictionary<string, KanbanCalc>();
            ImportHelper.JumpRows(rows, 1);

            #region 列定义
            int colFlowDet = 0;//路线明细
            int colKanbanDeltaNum = 26;//变化张数
            #endregion
            int i = 1;
            while (rows.MoveNext())
            {
                i++;
                HSSFRow row = (HSSFRow)rows.Current;
                if (!ImportHelper.CheckValidDataRow(row, 1, 26))
                {
                    break;//边界
                }
                int flowDetId = 0;
                int kanbanDeltaNum = 0;
                #region 路线明细Id
                try
                {
                    try
                    {
                        flowDetId = Convert.ToInt32(row.GetCell(colFlowDet).NumericCellValue);
                    }
                    catch (Exception)
                    {
                        flowDetId = Convert.ToInt32(row.GetCell(colFlowDet).ToString());
                    }
                }
                catch
                {
                    businessException.AddMessage("无法获取导入数据的路线ID,第{0}行。\t", i.ToString());
                    continue;
                }
                #endregion

                #region 变化张数
                try
                {
                    try
                    {
                        kanbanDeltaNum = Convert.ToInt32(row.GetCell(colKanbanDeltaNum).NumericCellValue);
                    }
                    catch (Exception)
                    {
                        kanbanDeltaNum = Convert.ToInt32(row.GetCell(colKanbanDeltaNum).ToString());
                    }
                }
                catch
                {
                    businessException.AddMessage("无法获取导入数据差异,第{0}行。\t", i.ToString());
                    continue;
                }
                #endregion

                if (importedKanbanCalcs.ContainsKey(flowDetId.ToString()))
                {
                    businessException.AddMessage("Import.Stream.DuplicateRow, id为{0}", flowDetId.ToString());
                }
                else
                {
                    KanbanCalc calc = new KanbanCalc();
                    calc.BatchNo = batchno;
                    calc.FlowDetailId = flowDetId;
                    calc.KanbanDeltaNum = kanbanDeltaNum;
                    importedKanbanCalcs.Add(flowDetId.ToString(), calc);
                }
            }
            if (businessException.HasMessage)
            {
                throw businessException;
            }
            this.UpdateBatch(batchno, currentUser, importedKanbanCalcs);
        }
        #endregion

        #region 看板卡遗失导入
        [Transaction(TransactionMode.Requires)]
        public void CreateKanbanLostXls(Stream inputStream)
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
            int colCardNo = 1; // 看板卡号

            #endregion
            IList<KanbanLost> exactKanbanLostList = new List<KanbanLost>();
            int i = 0;
            while (rows.MoveNext())
            {
                i++;
                HSSFRow row = (HSSFRow)rows.Current;
                if (!ImportHelper.CheckValidDataRow(row, 1, 2))
                {
                    break;//边界
                }

                string cardNo = string.Empty;

                #region 读取数据
                cardNo = ImportHelper.GetCellStringValue(row.GetCell(colCardNo));
                if (cardNo == null || cardNo.Trim() == string.Empty)
                {
                    businessException.AddMessage(string.Format("第{0}行卡号不能为空", i));
                    continue;
                }
                #endregion

                #region KanbanLost
                var checkKanbanCards = this.genericMgr.FindAll<KanbanCard>(" select k from KanbanCard as k where k.CardNo=?", cardNo);
                if (checkKanbanCards != null && checkKanbanCards.Count > 0)
                {
                    businessException.AddMessage(string.Format("第{0}行卡号{1}已经存在看板卡中", i, cardNo));
                }

                var checkKanbanLosts = this.genericMgr.FindAll<KanbanLost>(" select k from KanbanLost as k where k.CardNo=?", cardNo);
                if (checkKanbanLosts != null && checkKanbanLosts.Count > 0)
                {
                    businessException.AddMessage(string.Format("第{0}行卡号{1}已经存在记录。", i, cardNo));
                }
                else
                {
                    var kanbanLostByCard = this.genericMgr.FindEntityWithNativeSql<KanbanLost>(@" select * from KB_KanbanLost where LEFT(CardNo,LEN(CardNo)-4)=LEFT('" + cardNo + "',LEN('" + cardNo + "')-4) "
     + "and RIGHT(CardNo,3)=RIGHT('" + cardNo + "',3)");
                    if (kanbanLostByCard != null && kanbanLostByCard.Count > 0)
                    {
                        businessException.AddMessage(string.Format("第{0}行卡号{1}已经存在已经存在相似的卡号。", i, cardNo));
                    }
                }

                var existsKanbanLosts = exactKanbanLostList.Where(k => k.CardNo == cardNo).ToList();
                if (existsKanbanLosts.Count > 0)
                {
                    businessException.AddMessage(string.Format("模板中第{0}行卡号{1}存在重复记录。", i, cardNo));
                }
                else
                {
                    var similarInstances = exactKanbanLostList.Where(c => c.CardNo.Length == cardNo.Length &&
                                                               c.CardNo.Substring(0, c.CardNo.Length - 4) == cardNo.Substring(0, cardNo.Length - 4) &&
                                                               c.CardNo.Substring(c.CardNo.Length - 4 + 1) == cardNo.Substring(cardNo.Length - 4 + 1));

                    if (similarInstances.Count() > 0)
                    {
                        businessException.AddMessage(string.Format("第{0}行，待导入数据中，已经存在相似的卡号{1}。", i, cardNo));
                    }
                }

                exactKanbanLostList.Add(new KanbanLost { CardNo = cardNo });

                #endregion
            }

            if (businessException.HasMessage)
            {
                throw businessException;
            }

            foreach (var kanbanLost in exactKanbanLostList)
            {
                this.genericMgr.Create(kanbanLost);
            }
        }
        #endregion

        #region 看板遗失核算
        [Transaction(TransactionMode.Requires)]
        public void TryCalcKanbanLost(string multiregion, int ignoreTimeNumber)
        {
            //#region 将之前的遗失都计算掉
            //this.genericMgr.Update("Update KanbanCard set IsLost = ? where IsLost = ? ", new object[] { false, true });
            //genericMgr.FlushSession();
            //genericMgr.CleanSession();
            //#endregion

            //IList<Region> calcRegions = new List<Region>();
            //string[] regions = multiregion.Split(',');

            //foreach (string calcRegion in regions)
            //{
            //    IList<FlowMaster> flowList = genericMgr.FindEntityWithNativeSql<FlowMaster>(@"select c.* from scm_flowmstr c inner join scm_flowstrategy as g on c.Code = g.Flow where c.IsActive = ? and  c.PartyTo = ? and g.Strategy = ? and g.KBCalc = ?", new object[] { true, calcRegion, (int)com.Sconit.CodeMaster.FlowStrategy.KB, (int)com.Sconit.CodeMaster.KBCalculation.Normal });
            //    IList<WorkingCalendar> workingCalendatList = genericMgr.FindAll<WorkingCalendar>(" from WorkingCalendar w where w.FlowStrategy = ? and w.Region = ? and w.WorkingDate <= ? and w.Type = ?", new object[] { (int)com.Sconit.CodeMaster.FlowStrategy.KB, calcRegion, DateTime.Now.Date, (int)WorkingCalendarType.Work });
            //    IList<KanbanCard> kanbanCardList = genericMgr.FindAll<KanbanCard>(" from KanbanCard k where k.Region = ? and k.KBCalc = ?", new object[] { calcRegion, (int)com.Sconit.CodeMaster.KBCalculation.Normal });
            //    string sysTime = systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.StandardWorkTime);

            //    foreach (FlowMaster flow in flowList)
            //    {
            //        IList<FlowShiftDetail> flowShiftDetailList = genericMgr.FindAll<FlowShiftDetail>(" from FlowShiftDetail f where f.Flow = ?", flow.Code);
            //        IList<FlowDetail> flowDetailList = genericMgr.FindAll<FlowDetail>(" from FlowDetail d where d.Flow = ? and d.MaxStock > 0", flow.Code);

            //        #region 路线明细
            //        if (flowDetailList != null && flowDetailList.Count > 0)
            //        {
            //            foreach (FlowDetail fd in flowDetailList)
            //            {
            //                int alpha = Convert.ToInt32(Math.Ceiling(fd.UnitCount * fd.CycloidAmount * fd.MrpWeight / fd.MaxStock.Value));
            //                IList<KanbanCard> matchKanbanCardList = kanbanCardList.Where(p => p.FlowDetailId == fd.Id).ToList();
            //                foreach (KanbanCard k in matchKanbanCardList)
            //                {
            //                    if (k.LostCount > 0)
            //                    {
            //                        k.LostCount = k.LostCount - 1;
            //                        genericMgr.Update(k);
            //                    }
            //                    else
            //                    {
            //                        if (k.LastUseDate == DateTime.Parse("1900-01-01 00:00:00"))
            //                        {
            //                            k.IsLost = true;
            //                            k.LostCount = ignoreTimeNumber;
            //                            genericMgr.Update(k);
            //                        }
            //                        else
            //                        {

            //                            int interval = GetShiftCount(k.LastUseDate.Value, DateTime.Now, workingCalendatList, flowShiftDetailList, sysTime);
            //                            if (interval > alpha)
            //                            {
            //                                k.IsLost = true;
            //                                k.LostCount = ignoreTimeNumber;
            //                                genericMgr.Update(k);
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //        #endregion
            //    }
            //}
        }

        //取上次扫描时间和本次时间间隔的
        //private int GetShiftCount(DateTime startDate, DateTime endDate, IList<WorkingCalendar> workingCalendarList, IList<FlowShiftDetail> flowShiftDetailList, string sysTime)
        //{
        //    int shiftCount = 0;

        //    string[] ss = sysTime.Split(':');
        //    int hour = Convert.ToInt32(ss[0]);
        //    int minute = Convert.ToInt32(ss[1]);
        //    //#region 属于同一天的
        //    //if (startDate.AddHours(-hour).AddMinutes(-minute).Date == endDate.AddDays(-hour).AddMinutes(-minute).Date)
        //    //{
        //    //    int beginIndex = 0;
        //    //    int endIndex = 0;
        //    //    WorkingCalendar nStart = workingCalendarList.Where(p => p.WorkingDate == startDate.AddHours(-hour).AddMinutes(-minute).Date).SingleOrDefault();
        //    //    if (nStart != null)
        //    //    {
        //    //        IList<FlowShiftDetail> matchFlowShiftDetailList = flowShiftDetailList.Where(p => p.Shift == nStart.Shift).ToList();
        //    //        for (int i = 0; i < matchFlowShiftDetailList.Count; i++)
        //    //        {
        //    //            DateTime startTime = KanbanUtility.ConvertDateTimeFromWorkDateAndShiftTime(nStart.WorkingDate, flowShiftDetailList[i].StartTime, sysTime);
        //    //            DateTime endTime = KanbanUtility.ConvertDateTimeFromWorkDateAndShiftTime(nStart.WorkingDate, flowShiftDetailList[i].EndTime, sysTime);
        //    //            if (beginIndex == 0 && (startDate < startTime || (startDate > startTime && startDate < endTime)))
        //    //            {
        //    //                beginIndex = i;
        //    //            }
        //    //            if (endIndex == 0 && (endDate < startTime || (endDate > startTime && endDate < endTime)))
        //    //            {
        //    //                endIndex = i;
        //    //            }
        //    //            if (beginIndex > 0 && endIndex > 0)
        //    //            {
        //    //                break;
        //    //            }
        //    //        }
        //    //    }
        //    //    shiftCount = endIndex - beginIndex;
        //    //}
        //    //#endregion

        //    #region 1天之内的不考虑，新华说的
        //    if (startDate.AddHours(-hour).AddMinutes(-minute).Date.AddDays(1) >= endDate.AddDays(-hour).AddMinutes(-minute).Date)
        //    {
        //    }
        //    #endregion

        //    #region 不属于同一天的
        //    else
        //    {
        //        int beginIndex = 0;
        //        int endIndex = 0;

        //        #region 开始那天的
        //        WorkingCalendar nStart = workingCalendarList.Where(p => p.WorkingDate == startDate.AddHours(-hour).AddMinutes(-minute).Date).SingleOrDefault();
        //        if (nStart != null)
        //        {
        //            IList<FlowShiftDetail> matchStartFlowShiftDetailList = flowShiftDetailList.Where(p => p.Shift == nStart.Shift).ToList();
        //            for (int i = 0; i < matchStartFlowShiftDetailList.Count; i++)
        //            {
        //                DateTime startTime = KanbanUtility.ConvertDateTimeFromWorkDateAndShiftTime(nStart.WorkingDate, flowShiftDetailList[i].StartTime, sysTime);
        //                DateTime endTime = KanbanUtility.ConvertDateTimeFromWorkDateAndShiftTime(nStart.WorkingDate, flowShiftDetailList[i].EndTime, sysTime);
        //                if (beginIndex == 0 && (startDate < startTime || (startDate > startTime && startDate < endTime)))
        //                {
        //                    beginIndex = i + 1;
        //                    break;
        //                }
        //            }
        //            shiftCount = matchStartFlowShiftDetailList.Count - beginIndex;
        //        }
        //        #endregion

        //        #region  结束那天的
        //        WorkingCalendar nEnd = workingCalendarList.Where(p => p.WorkingDate == endDate.AddHours(-hour).AddMinutes(-minute).Date).SingleOrDefault();
        //        if (nEnd != null)
        //        {
        //            IList<FlowShiftDetail> matchEndFlowShiftDetailList = flowShiftDetailList.Where(p => p.Shift == nEnd.Shift).ToList();
        //            for (int i = 0; i < matchEndFlowShiftDetailList.Count; i++)
        //            {
        //                DateTime startTime = KanbanUtility.ConvertDateTimeFromWorkDateAndShiftTime(nEnd.WorkingDate, flowShiftDetailList[i].StartTime, sysTime);
        //                DateTime endTime = KanbanUtility.ConvertDateTimeFromWorkDateAndShiftTime(nEnd.WorkingDate, flowShiftDetailList[i].EndTime, sysTime);
        //                if (beginIndex == 0 && (endDate < startTime || (endDate > startTime && endDate < endTime)))
        //                {
        //                    endIndex = i + 1;
        //                }
        //            }
        //            shiftCount += endIndex;
        //        }
        //        #endregion

        //        #region 当中几天的
        //        var dateList = workingCalendarList.Where(p => p.WorkingDate > startDate.AddHours(-hour).AddMinutes(-minute).Date && p.WorkingDate < endDate.AddHours(-hour).AddMinutes(-minute).Date);
        //        if (dateList != null && dateList.Count() > 0)
        //        {
        //            foreach (WorkingCalendar w in dateList)
        //            {
        //                shiftCount += flowShiftDetailList.Where(p => p.Shift == w.Shift).Count();
        //            }
        //        }
        //        #endregion
        //    }
        //    #endregion
        //    return shiftCount;
        //}


        #region 删除冻结且过了有效期的
        [Transaction(TransactionMode.Requires)]
        public void DeleteFreezeKanbanCard()
        {
            IList<KanbanCard> freezeKanbanCardList = genericMgr.FindAll<KanbanCard>("from KanbanCard k where k.FreezeDate < ?", DateTime.Now);
            if (freezeKanbanCardList != null && freezeKanbanCardList.Count > 0)
            {
                IList<Int32> flowDetailIdList = freezeKanbanCardList.Select(p => p.FlowDetailId).Distinct().ToList();
                foreach (Int32 flowDetailId in flowDetailIdList)
                {
                    FlowDetail fd = genericMgr.FindById<FlowDetail>(flowDetailId);
                    fd.CycloidAmount -= freezeKanbanCardList.Where(p => p.FlowDetailId == flowDetailId).Count();
                    genericMgr.Update(fd);

                    genericMgr.FindAllWithNativeSql<KanbanCard>("Update KB_KanbanCard set TotalCount = ? where FlowDetId = ?", new object[] { fd.CycloidAmount, fd.Id });
                }
                genericMgr.FindAllWithNativeSql<KanbanCard>("Delete from KB_KanbanCard  where  FreezeDate < ? ", DateTime.Now);
            }
        }
        #endregion 删除冻结且过了有效期的

        #endregion

        #region 看板卡导入
        /// <summary>
        /// 看板路线导入
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="type">采购/移库</param>
        [Transaction(TransactionMode.Requires)]
        public void ImportKanBanCard(Stream inputStream, com.Sconit.CodeMaster.OrderType type, User currUser)
        {
            if (inputStream.Length == 0)
            {
                throw new BusinessException("Import.Stream.Empty");
            }

            HSSFWorkbook workbook = new HSSFWorkbook(inputStream);

            ISheet sheet = workbook.GetSheetAt(0);
            IEnumerator rows = sheet.GetRowEnumerator();

            ImportHelper.JumpRows(rows, 10);

            #region 列定义
            // FlowMaster
            int colCode = 1; // 路线代码
            int colItem = 2; // 物料号										
            int colRefItemCode = 3; // 旧图号
            int colItemDesc = 4; // 零件名称
            int colOpRef = 5; // 工位代码
            int colOpUseQty = 6; // 工位用量
            int colItemType = 7; // 零件类型
            int colOpRefSeq = 8; // 看板编号
            int colUC = 9; // 单包装
            int colUCDesc = 10; // 包装描述

            int colContainerDesc = 11; // 容器描述
            int colDock = 12; // 配送路径
            int colCardQty = 13; // 看板卡数量
            int colIsActive = 14; // 是否生效
            //int colShelf = 13; // 架位
            #endregion

            var errorMessage = new BusinessException();

            //IList<Item> ItemsList = new List<Item>();
            //IList<FlowMaster> exactFlowList = new List<FlowMaster>();
            IList<FlowDetail> exactFlowDetailList = new List<FlowDetail>();
            //IList<FlowDetail> deleteFlowDetailList = new List<FlowDetail>();
            int colCount = 0;

            #region 取所有的路线和路线明细
            //var totalFlowMasterList = genericMgr.FindAll<FlowMaster>("from FlowMaster f where f.Type = ? and f.FlowStrategy = ?", new object[] { (int)type, (int)CodeMaster.FlowStrategy.ANDON });
            var totalFlowDetailList = genericMgr.FindEntityWithNativeSql<FlowDetail>("select d.* from scm_FlowDet d inner join scm_FlowMstr f on f.Code = d.Flow and f.Type = ? and exists(select 1 from SCM_FlowStrategy as fs where fs.Flow=f.Code and fs.Strategy=?)", new object[] { (int)type, (int)CodeMaster.FlowStrategy.ANDON });
            //var totalFlowStrategyList = genericMgr.FindEntityWithNativeSql<FlowStrategy>("select g.* from scm_FlowStrategy g inner join scm_FlowMstr f on f.Code = g.Flow and f.Type = ? and f.FlowStrategy = ?", new object[] { (int)type, (int)CodeMaster.FlowStrategy.ANDON });
            //var totalSupplierList = genericMgr.FindAll<Supplier>();
            //var totalRegionList = genericMgr.FindAll<Region>();
            //var totalLocationList = genericMgr.FindAll<Location>();
            //var totalItemList = genericMgr.FindAll<Item>("from Item where IsActive = ?", true);
            #endregion

            while (rows.MoveNext())
            {
                HSSFRow row = (HSSFRow)rows.Current;
                if (!ImportHelper.CheckValidDataRow(row, 1, 14))
                {
                    break;//边界
                }
                colCount++;

                var rowErrors = new List<Message>();

                string code = string.Empty;
                string item = string.Empty;
                string refItemCode = string.Empty;
                string itemDesc = string.Empty;
                string opRef = string.Empty;
                string opUseQty = string.Empty;
                string itemType = string.Empty;
                string opRefSeq = string.Empty;
                string uc = string.Empty;
                string ucDesc = string.Empty;
                string dock = string.Empty;
                string containerDesc = string.Empty;
                string cardQty = string.Empty;
                string isActive = string.Empty;


                #region 读取数据

                code = ImportHelper.GetCellStringValue(row.GetCell(colCode));
                if (code == null || code.Trim() == string.Empty)
                {
                    rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineFieldCanNotBeNull, colCount.ToString(), Resources.SCM.FlowMaster.FlowMaster_Code));
                }

                opRef = ImportHelper.GetCellStringValue(row.GetCell(colOpRef));
                if (opRef == null || opRef.Trim() == string.Empty)
                {
                    rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineFieldCanNotBeNull, colCount.ToString(), "工位"));
                }

                cardQty = ImportHelper.GetCellStringValue(row.GetCell(colCardQty));
                decimal qtyVar = 0;
                if (cardQty == null || cardQty.Trim() == string.Empty || !decimal.TryParse(cardQty,out qtyVar))
                {
                    rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineFieldCanNotBeNull, colCount.ToString(), "看板张数"));
                }
                
                item = ImportHelper.GetCellStringValue(row.GetCell(colItem));
                if (item == null || item.Trim() == string.Empty)
                {
                    rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineFieldCanNotBeNull, colCount.ToString(), Resources.SCM.FlowDetail.FlowDetail_Item));
                }
                
                #endregion

                #region 检查路线明细是否都在数据库中
                var flowDet = totalFlowDetailList.FirstOrDefault(f => f.Flow.ToUpper() == code.ToUpper() && f.Item.ToUpper() == item.ToUpper() && f.BinTo.ToUpper() == opRef.ToUpper());
                if (flowDet == null)
                {
                    rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, "导入的数据的第{0}行路线明细不存在", colCount.ToString()));
                }
                else
                {
                    flowDet.OrderQty = qtyVar;
                    exactFlowDetailList.Add(flowDet);
                }
                #endregion


                errorMessage.AddMessages(rowErrors);
            }

            if (errorMessage.HasMessage)
            {
                throw errorMessage;
            }

            if (exactFlowDetailList.Count > 0)
            {
                var flowCodes = exactFlowDetailList.Select(f => f.Flow).Distinct();
                foreach (var flow in flowCodes)
                {
                    this.AddManuallyByKanbanFlow(flow, exactFlowDetailList.Where(f => f.Flow == flow).ToList(), DateTime.Now, currUser, true);
                }
            }
        }

        #endregion

    }
}