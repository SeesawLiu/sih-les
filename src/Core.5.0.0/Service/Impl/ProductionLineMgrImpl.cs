using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Castle.Services.Transaction;
using com.Sconit.Entity;
using com.Sconit.Entity.ACC;
using com.Sconit.Entity.CUST;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.INV;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.PRD;
using com.Sconit.Entity.SCM;
using com.Sconit.Entity.VIEW;
using com.Sconit.Persistence;
using com.Sconit.Utility;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace com.Sconit.Service.Impl
{
    [Transactional]
    public class ProductionLineMgrImpl : BaseMgr, IProductionLineMgr
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("Log.ProductionLine");

        public IGenericMgr genericMgr { get; set; }
        public ISystemMgr systemMgr { get; set; }
        public IBomMgr bomMgr { get; set; }
        public ILocationDetailMgr locationDetailMgr { get; set; }
        public IItemMgr itemMgr { get; set; }
        public IHuMgr huMgr { get; set; }
        public ISqlDao sqlDao { get; set; }
        public IBackflushVanOrderMgr backflushVanOrderMgr { get; set; }

        #region 生产投料
        [Transaction(TransactionMode.Requires)]
        public void FeedRawMaterial(string productLine, string productLineFacility, IList<FeedInput> feedInputList)
        {
            FeedRawMaterial(productLine, productLineFacility, null, feedInputList, false, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public void FeedRawMaterial(string productLine, string productLineFacility, IList<FeedInput> feedInputList, DateTime effectiveDate)
        {
            FeedRawMaterial(productLine, productLineFacility, null, feedInputList, false, effectiveDate);
        }

        [Transaction(TransactionMode.Requires)]
        public void FeedRawMaterial(string productLine, string productLineFacility, IList<FeedInput> feedInputList, bool isForceFeed)
        {
            FeedRawMaterial(productLine, productLineFacility, null, feedInputList, isForceFeed, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public void FeedRawMaterial(string productLine, string productLineFacility, IList<FeedInput> feedInputList, bool isForceFeed, DateTime effectiveDate)
        {
            FeedRawMaterial(productLine, productLineFacility, null, feedInputList, isForceFeed, effectiveDate);
        }

        [Transaction(TransactionMode.Requires)]
        public void FeedRawMaterial(string orderNo, IList<FeedInput> feedInputList)
        {
            FeedRawMaterial(orderNo, feedInputList, false, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public void FeedRawMaterial(string orderNo, IList<FeedInput> feedInputList, DateTime effectiveDate)
        {
            FeedRawMaterial(orderNo, feedInputList, false, effectiveDate);
        }

        [Transaction(TransactionMode.Requires)]
        public void FeedRawMaterial(string orderNo, IList<FeedInput> feedInputList, bool isForceFeed)
        {
            FeedRawMaterial(orderNo, feedInputList, isForceFeed, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public void FeedRawMaterial(string orderNo, IList<FeedInput> feedInputList, bool isForceFeed, DateTime effectiveDate)
        {
            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderNo);
            //暂时只支持生产单一条明细
            int orderDetailId = genericMgr.FindAll<int>("select Id from OrderDetail where OrderNo = ?", orderNo).First();

            if (orderMaster.Type != com.Sconit.CodeMaster.OrderType.Production && orderMaster.Type != com.Sconit.CodeMaster.OrderType.SubContract)
            {
                throw new TechnicalException("非生产单不能进行投料。");
            }

            #region 检查生产单
            if (orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.InProcess && orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Complete)
            {
                throw new BusinessException("状态为{0}的生产单不能投料。", systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
            }
            #endregion

            #region 生产单暂停检查
            if (orderMaster.PauseStatus == CodeMaster.PauseStatus.Paused)
            {
                throw new BusinessException("生产单{0}已经暂停，不能投料。", orderMaster.OrderNo);
            }
            #endregion

            #region 批量对投料对象赋订单的值
            if (feedInputList != null)
            {
                foreach (FeedInput feedInput in feedInputList)
                {
                    feedInput.OrderNo = orderNo;
                    feedInput.OrderDetailId = orderDetailId;
                    feedInput.TraceCode = orderMaster.TraceCode;
                    feedInput.CurrentOrderMaster = orderMaster;
                    feedInput.OrderType = orderMaster.Type;
                    feedInput.OrderSubType = orderMaster.SubType;
                    feedInput.AUFNR = orderMaster.ExternalOrderNo;  //SAP工单号
                }
            }
            #endregion

            FeedRawMaterial(orderMaster.Flow, orderMaster.ProductLineFacility, orderNo, feedInputList, isForceFeed, effectiveDate);
        }

        #region 旧FeedRawMaterial
        //private void FeedRawMaterial(string productLine, string productLineFacility, string orderNo, int? operation, string opReference, IList<FeedInput> feedInputList, bool isForceFeed, DateTime effectiveDate)
        //{
        //    #region 投料不能为空检验
        //    IList<FeedInput> noneZeroFeedInputList = null;
        //    if (feedInputList != null)
        //    {
        //        noneZeroFeedInputList = feedInputList.Where(f => f.Qty > 0 || !string.IsNullOrWhiteSpace(f.HuId)).ToList();
        //    }
        //    if (noneZeroFeedInputList == null || noneZeroFeedInputList.Count == 0)
        //    {
        //        throw new BusinessException("投料零件不能为空。");
        //    }
        //    #endregion

        //    #region 查找条码填充库位、数量和质量状态等数据
        //    foreach (FeedInput feedInput in noneZeroFeedInputList)
        //    {
        //        if (!string.IsNullOrWhiteSpace(feedInput.HuId))
        //        {
        //            string hql = "from LocationLotDetail where HuId = ?";
        //            IList<LocationLotDetail> huLocationList = this.genericMgr.FindAll<LocationLotDetail>(hql, feedInput.HuId);
        //            huLocationList = huLocationList.Where(h => h.Qty > 0).ToList();

        //            if (huLocationList == null || huLocationList.Count == 0)
        //            {
        //                Hu hu = this.genericMgr.FindAll<Hu>("from Hu where HuId = ?", feedInput.HuId).SingleOrDefault();

        //                if (hu == null)
        //                {
        //                    hu = this.huMgr.ResolveHu(feedInput.HuId);
        //                }

        //                #region 装箱
        ////                 public string Location { get; set; }
        ////public string HuId { get; set; }
        ////public CodeMaster.OccupyType OccupyType { get; set; }
        ////public string OccupyReferenceNo { get; set; }
        ////public IList<Int32> LocationLotDetailIdList { get; set; }

        ////public HuStatus CurrentHu { get; set; }
        ////public Location CurrentLocation { get; set; }


        ////                this.locationDetailMgr.InventoryPack(
        //                #endregion
        //                //ResolveHu(string extHuId);
        //                throw new BusinessException("投料的条码{0}不在任何库位中。", feedInput.HuId);
        //            }
        //            else if (huLocationList.Count > 1)
        //            {
        //                throw new TechnicalException("Hu " + feedInput.HuId + " in more than 1 location.");
        //            }

        //            LocationLotDetail locationLotDetail = huLocationList[0];
        //            //todo 检验投料的条码是否是OrderBomDetail上指定的库位投料

        //            //不用判断冻结和占用，出库方法中会判断
        //            feedInput.Item = locationLotDetail.Item;
        //            feedInput.LocationFrom = locationLotDetail.Location;
        //            feedInput.LotNo = locationLotDetail.LotNo;
        //            feedInput.QualityType = locationLotDetail.QualityType;
        //            feedInput.Qty = locationLotDetail.HuQty;
        //            feedInput.Uom = locationLotDetail.HuUom;
        //            feedInput.BaseUom = locationLotDetail.BaseUom;
        //            feedInput.UnitQty = locationLotDetail.UnitQty;
        //        }
        //    }
        //    #endregion

        //    #region 非强制投料，检验投料零件是否有效
        //    if (!isForceFeed)
        //    {
        //        if (orderNo == null)
        //        {
        //            #region 检验生产线是否有投料的零件
        //            IList<BomDetail> bomDetailList = this.bomMgr.GetProductLineWeightAverageBomDetail(productLine);

        //            foreach (FeedInput feedInput in noneZeroFeedInputList)
        //            {
        //                #region 查找物料是否是生产线上投料的
        //                if (bomDetailList != null && bomDetailList.Count > 0)
        //                {
        //                    bool findMatch = (from det in bomDetailList
        //                                      where det.Item == feedInput.Item
        //                                      select det).ToList().Count > 0;

        //                    #region 判断是否后续物料
        //                    if (!findMatch)
        //                    {
        //                        IList<ItemDiscontinue> disConItems = this.itemMgr.GetParentItemDiscontinues(feedInput.Item, effectiveDate);
        //                        if (disConItems != null && disConItems.Count > 0)
        //                        {
        //                            findMatch = (from det in bomDetailList
        //                                         join disConItem in disConItems
        //                                         on det.Item equals disConItem.Item
        //                                         where disConItem.Bom == null || disConItem.Bom == det.Bom
        //                                         select det).ToList().Count > 0;
        //                        }
        //                    }
        //                    #endregion

        //                    if (!findMatch)
        //                    {
        //                        throw new BusinessException("生产线{0}中没有需要投料的零件{1}。", productLine, feedInput.Item);
        //                    }
        //                }
        //                else
        //                {
        //                    throw new BusinessException("生产线{0}上没有需要投料的零件。", productLine);
        //                }
        //                #endregion
        //            }
        //            #endregion
        //        }
        //        else
        //        {
        //            #region 检验生产单上是否有投料的零件
        //            //todo 需要考虑一张生产单多个成品的情况，指定成品零件号/明细行号

        //            #region 查找生产单Bom
        //            string hql = "from OrderBomDetail where OrderNo = ?";
        //            IList<object> para = new List<object>();
        //            para.Add(orderNo);
        //            if (operation.HasValue)
        //            {
        //                hql += " and Operation = ? and OpReference = ?";
        //                para.Add(operation.Value);
        //                para.Add(opReference);
        //            }
        //            IList<OrderBomDetail> orderBomDetailList = this.genericMgr.FindAll<OrderBomDetail>(hql, para.ToArray());
        //            #endregion

        //            #region 查找已投料关键件
        //            hql = "from ProductLineLocationDetail where OrderNo = ? and HuId is not null and ReserveNo is not null and ReserveLine is not null";
        //            para = new List<object>();
        //            para.Add(orderNo);
        //            if (operation.HasValue)
        //            {
        //                hql += " and Operation = ? and OpReference = ?";
        //                para.Add(operation.Value);
        //                para.Add(opReference);
        //            }
        //            IList<ProductLineLocationDetail> feedProductLineLocationDetailList = this.genericMgr.FindAll<ProductLineLocationDetail>(hql, para.ToArray());
        //            #endregion

        //            foreach (FeedInput feedInput in noneZeroFeedInputList)
        //            {
        //                #region 找到候选的生产单Bom
        //                IList<OrderBomDetail> matchedOrderBomDetailList = orderBomDetailList.Where(det => det.Item == feedInput.Item).ToList();

        //                #region 判断是否后续物料
        //                if (matchedOrderBomDetailList == null || matchedOrderBomDetailList.Count == 0)
        //                {
        //                    IList<ItemDiscontinue> disConItems = this.itemMgr.GetParentItemDiscontinues(feedInput.Item, effectiveDate);
        //                    matchedOrderBomDetailList = (from det in orderBomDetailList
        //                                                 join disConItem in disConItems
        //                                                 on det.Item equals disConItem.Item
        //                                                 where disConItem.Bom == null || disConItem.Bom == det.Bom
        //                                                 select det).ToList();
        //                }
        //                #endregion
        //                #endregion

        //                if (matchedOrderBomDetailList != null && matchedOrderBomDetailList.Count > 0)
        //                {
        //                    #region 关键件扫描，查找对应的生产单Bom，取Bom上的SAP生产单号、生产批号、预留号、行号、移动类型
        //                    if (!string.IsNullOrWhiteSpace(feedInput.HuId))
        //                    {
        //                        //投料的零件和Bom必须单位一致
        //                        matchedOrderBomDetailList = matchedOrderBomDetailList.Where(m => m.Uom == feedInput.Uom).ToList();
        //                        if (matchedOrderBomDetailList != null && matchedOrderBomDetailList.Count > 0)
        //                        {
        //                            #region 投料物料和生产单BOM循环匹配
        //                            bool findMatch = false;  //是否找到对应的生产单
        //                            foreach (OrderBomDetail matchedOrderBomDetail in matchedOrderBomDetailList.OrderBy(d => d.Operation)) //按照工序的顺序先后匹配
        //                            {
        //                                #region 根据预留号和行号汇总已投料数量
        //                                //已投料量
        //                                decimal feedQty = feedProductLineLocationDetailList.Where(locDet => locDet.ReserveNo == matchedOrderBomDetail.ReserveNo
        //                                    && locDet.ReserveLine == matchedOrderBomDetail.ReserveLine)
        //                                    .Sum(locDet => locDet.Qty);  //库存单位

        //                                //todo考虑指定预留号和行号投料
        //                                ////+本次投料量
        //                                //feedQty += noneZeroFeedInputList.Where(i => i.ReserveNo == matchedOrderBomDetail.ReserveNo
        //                                //    && i.ReserveLine == matchedOrderBomDetail.ReserveLine)
        //                                //    .Sum(i => i.Qty * i.UnitQty);  //库存单位
        //                                #endregion

        //                                #region 生产单Bom用量是否大于已投料+本次投料量
        //                                if (matchedOrderBomDetail.OrderedQty * matchedOrderBomDetail.UnitQty >= (feedQty + feedInput.Qty * feedInput.UnitQty))
        //                                {
        //                                    feedInput.ReserveNo = matchedOrderBomDetail.ReserveNo;
        //                                    feedInput.ReserveLine = matchedOrderBomDetail.ReserveLine;
        //                                    feedInput.AUFNR = matchedOrderBomDetail.AUFNR;
        //                                    feedInput.BWART = matchedOrderBomDetail.BWART;
        //                                    feedInput.ICHARG = matchedOrderBomDetail.ICHARG;

        //                                    findMatch = true;
        //                                }
        //                                #endregion
        //                            }

        //                            if (!findMatch)
        //                            {
        //                                throw new BusinessException("生产单{0}的零件{1}已经投料。", orderNo, feedInput.Item);
        //                            }
        //                            #endregion
        //                        }
        //                        else
        //                        {
        //                            throw new BusinessException("投料的零件{1}和生产单{0}中的Bom单位不一致。", feedInput.Item, orderNo);
        //                        }
        //                    }
        //                    #endregion
        //                }
        //                else
        //                {
        //                    throw new BusinessException("生产单{0}中没有需要投料的零件{1}。", orderNo, feedInput.Item);
        //                }
        //            }
        //            #endregion
        //        }
        //    }
        //    else
        //    {
        //        #region 强制投料，检查投料至工单的零件是否都有SAP生产单号和移动类型，没有需要赋工单上的SAP工单号和默认261移动类型
        //        if (orderNo != null)
        //        {
        //            string AUFNR = string.Empty;
        //            foreach (FeedInput feedInput in noneZeroFeedInputList)
        //            {
        //                if (!string.IsNullOrWhiteSpace(feedInput.AUFNR))
        //                {
        //                    if (AUFNR == string.Empty)
        //                    {
        //                        AUFNR = this.genericMgr.FindAll<string>("select ExternalOrderNo from OrderMaster where OrderNo = ?", orderNo).Single();
        //                    }
        //                    feedInput.AUFNR = AUFNR;
        //                }

        //                if (!string.IsNullOrWhiteSpace(feedInput.BWART))
        //                {
        //                    feedInput.BWART = "261";
        //                }
        //            }
        //        }
        //        #endregion
        //    }
        //    #endregion

        //    #region 缓存出库需要的字段
        //    foreach (FeedInput feedInput in noneZeroFeedInputList)
        //    {
        //        feedInput.ProductLine = productLine;
        //        feedInput.ProductLineFacility = productLineFacility;
        //        feedInput.CurrentProductLine = this.genericMgr.FindById<FlowMaster>(productLine);
        //        #region 生产线暂停检查
        //        if (feedInput.CurrentProductLine.IsPause)
        //        {
        //            throw new BusinessException("生产线{0}已经暂停，不能投料。", feedInput.ProductLine);
        //        }
        //        #endregion
        //        if (string.IsNullOrWhiteSpace(feedInput.LocationFrom))
        //        {
        //            throw new TechnicalException("LocationFrom not specified in FeedInput.");
        //        }
        //        feedInput.CurrentLocationFrom = this.genericMgr.FindById<Location>(feedInput.LocationFrom);
        //        feedInput.CurrentItem = this.genericMgr.FindById<Item>(feedInput.Item);

        //        if (string.IsNullOrWhiteSpace(feedInput.HuId))
        //        {

        //            if (string.IsNullOrWhiteSpace(feedInput.Uom))
        //            {
        //                throw new TechnicalException("Uom not specified in FeedInput.");
        //            }
        //            feedInput.BaseUom = feedInput.CurrentItem.Uom;
        //            if (feedInput.CurrentItem.Uom != feedInput.Uom)
        //            {
        //                feedInput.UnitQty = this.itemMgr.ConvertItemUomQty(feedInput.Item, feedInput.Uom, 1, feedInput.CurrentItem.Uom);  //记录单位转换系数
        //            }
        //            else
        //            {
        //                feedInput.UnitQty = 1;
        //            }
        //        }
        //    }
        //    #endregion

        //    #region 循环操作库存
        //    foreach (FeedInput feedInput in noneZeroFeedInputList)
        //    {
        //        #region 投料
        //        IList<InventoryTransaction> inventoryTransactionList = this.locationDetailMgr.FeedProductRawMaterial(feedInput, effectiveDate);
        //        #endregion
        //    }
        //    #endregion
        //}
        #endregion

        #region 新FeedRawMaterial
        private void FeedRawMaterial(string productLine, string productLineFacility, string orderNo, IList<FeedInput> feedInputList, bool isForceFeed, DateTime effectiveDate)
        {
            #region 投料不能为空检验
            IList<FeedInput> noneZeroFeedInputList = null;
            if (feedInputList != null)
            {
                noneZeroFeedInputList = feedInputList.Where(f => f.Qty > 0 || !string.IsNullOrWhiteSpace(f.HuId)).ToList();
            }
            if (noneZeroFeedInputList == null || noneZeroFeedInputList.Count == 0)
            {
                throw new BusinessException("投料零件不能为空。");
            }
            #endregion

            #region 查找条码填充数量和质量状态等数据
            foreach (FeedInput feedInput in noneZeroFeedInputList)
            {
                if (!string.IsNullOrWhiteSpace(feedInput.HuId))
                {
                    HuStatus huStatus = this.huMgr.GetHuStatus(feedInput.HuId);
                    //string hql = "from LocationLotDetail where HuId = ?";
                    //IList<LocationLotDetail> huLocationList = this.genericMgr.FindAll<LocationLotDetail>(hql, feedInput.HuId);
                    //huLocationList = huLocationList.Where(h => h.Qty > 0).ToList();

                    if (string.IsNullOrWhiteSpace(huStatus.Location))
                    {
                        throw new BusinessException("投料的条码{0}不在任何库位中。", feedInput.HuId);
                    }
                    else if (!string.IsNullOrWhiteSpace(huStatus.LocationFrom) || !string.IsNullOrWhiteSpace(huStatus.LocationTo))
                    {
                        throw new BusinessException("投料的条码{0}是在途库存，不能投料。", feedInput.HuId);
                    }
                    //else if (huLocationList.Count > 1)
                    //{
                    //    throw new TechnicalException("Hu " + feedInput.HuId + " in more than 1 location.");
                    //}

                    //LocationLotDetail locationLotDetail = huLocationList[0];
                    //todo 检验投料的条码是否是OrderBomDetail上指定的库位投料

                    //不用判断冻结和占用，出库方法中会判断
                    feedInput.Item = huStatus.Item;
                    feedInput.LocationFrom = huStatus.Location;
                    feedInput.LotNo = huStatus.LotNo;
                    feedInput.QualityType = huStatus.QualityType;
                    feedInput.Qty = huStatus.Qty;
                    feedInput.Uom = huStatus.Uom;
                    feedInput.BaseUom = huStatus.BaseUom;
                    feedInput.UnitQty = huStatus.UnitQty;
                }
            }
            #endregion

            #region 非强制投料，检验投料零件是否有效
            if (!isForceFeed)
            {
                if (orderNo == null)
                {
                    #region 检验生产线是否有投料的零件
                    IList<BomDetail> bomDetailList = this.bomMgr.GetProductLineWeightAverageBomDetail(productLine);

                    foreach (FeedInput feedInput in noneZeroFeedInputList)
                    {
                        #region 查找物料是否是生产线上投料的
                        if (bomDetailList != null && bomDetailList.Count > 0)
                        {
                            bool findMatch = (from det in bomDetailList
                                              where det.Item == feedInput.Item
                                              select det).ToList().Count > 0;

                            #region 判断是否后续物料
                            if (!findMatch)
                            {
                                IList<ItemDiscontinue> disConItems = this.itemMgr.GetParentItemDiscontinues(feedInput.Item, effectiveDate);
                                if (disConItems != null && disConItems.Count > 0)
                                {
                                    findMatch = (from det in bomDetailList
                                                 join disConItem in disConItems
                                                 on det.Item equals disConItem.Item
                                                 where disConItem.Bom == null || disConItem.Bom == det.Bom
                                                 select det).ToList().Count > 0;
                                }
                            }
                            #endregion

                            if (!findMatch)
                            {
                                throw new BusinessException("生产线{0}中没有需要投料的零件{1}。", productLine, feedInput.Item);
                            }
                        }
                        else
                        {
                            throw new BusinessException("生产线{0}上没有需要投料的零件。", productLine);
                        }
                        #endregion
                    }
                    #endregion
                }
                else
                {
                    #region 检验生产单上是否有投料的零件
                    //todo 需要考虑一张生产单多个成品的情况，指定成品零件号/明细行号
                    foreach (FeedInput feedInput in noneZeroFeedInputList)
                    {
                        #region 查找生产单Bom
                        string hql = "from OrderBomDetail where OrderNo = ? and Item = ?";
                        IList<object> para = new List<object>();
                        para.Add(orderNo);
                        para.Add(feedInput.Item);
                        if (string.IsNullOrWhiteSpace(feedInput.HuId))
                        {
                            hql += " and Location = ?";
                            para.Add(feedInput.LocationFrom);
                        }
                        IList<OrderBomDetail> matchedOrderBomDetailList = this.genericMgr.FindAll<OrderBomDetail>(hql, para.ToArray());
                        #endregion

                        #region 查找已投料关键件
                        hql = "from ProductLineLocationDetail where OrderNo = ? and Item = ? and HuId is not null and ReserveNo is not null and ReserveLine is not null and IsClose = ?";
                        para = new List<object>();
                        para.Add(orderNo);
                        para.Add(feedInput.Item);
                        para.Add(false);
                        if (string.IsNullOrWhiteSpace(feedInput.HuId))
                        {
                            hql += " and LocationFrom = ?";
                            para.Add(feedInput.LocationFrom);
                        }
                        IList<ProductLineLocationDetail> feedProductLineLocationDetailList = this.genericMgr.FindAll<ProductLineLocationDetail>(hql, para.ToArray());
                        #endregion

                        if (matchedOrderBomDetailList != null && matchedOrderBomDetailList.Count > 0)
                        {
                            #region 关键件扫描，查找对应的生产单Bom，取Bom上的SAP生产单号、生产批号、预留号、行号、移动类型
                            //投料的零件和Bom必须单位一致
                            matchedOrderBomDetailList = matchedOrderBomDetailList.Where(m => m.Uom == feedInput.Uom).ToList();
                            if (matchedOrderBomDetailList != null && matchedOrderBomDetailList.Count > 0)
                            {
                                #region 投料物料和生产单BOM循环匹配
                                bool findMatch = false;  //是否找到对应的生产单
                                foreach (OrderBomDetail matchedOrderBomDetail in matchedOrderBomDetailList) //按照工序的顺序先后匹配
                                {
                                    #region 根据预留号和行号汇总已投料数量
                                    //已投料量
                                    decimal feedQty = feedProductLineLocationDetailList.Where(locDet => locDet.ReserveNo == matchedOrderBomDetail.ReserveNo
                                        && locDet.ReserveLine == matchedOrderBomDetail.ReserveLine)
                                        .Sum(locDet => locDet.Qty - locDet.BackFlushQty - locDet.VoidQty);  //库存单位
                                    #endregion

                                    #region 生产单Bom用量是否大于已投料+本次投料量
                                    if (matchedOrderBomDetail.OrderedQty * matchedOrderBomDetail.UnitQty >= (feedQty + feedInput.Qty * feedInput.UnitQty))
                                    {
                                        feedInput.Operation = matchedOrderBomDetail.Operation;
                                        feedInput.OpReference = matchedOrderBomDetail.OpReference;
                                        feedInput.ReserveNo = matchedOrderBomDetail.ReserveNo;
                                        feedInput.ReserveLine = matchedOrderBomDetail.ReserveLine;
                                        feedInput.AUFNR = matchedOrderBomDetail.AUFNR;
                                        feedInput.BWART = matchedOrderBomDetail.BWART;
                                        feedInput.ICHARG = matchedOrderBomDetail.ICHARG;

                                        findMatch = true;
                                        break;
                                    }
                                    #endregion
                                }

                                if (!findMatch)
                                {
                                    throw new BusinessException("生产单{0}的零件{1}已经投料。", orderNo, feedInput.Item);
                                }
                                #endregion
                            }
                            else
                            {
                                throw new BusinessException("投料的零件{1}和生产单{0}中的Bom单位不一致。", orderNo, feedInput.Item);
                            }
                            #endregion
                        }
                        else
                        {
                            throw new BusinessException("生产单{0}中没有需要投料的零件{1}。", orderNo, feedInput.Item);
                        }
                    }
                    #endregion
                }
            }
            else
            {
                #region 强制投料，检查投料至工单的零件是否都有SAP生产单号和移动类型，没有需要赋工单上的SAP工单号和默认261移动类型
                if (orderNo != null)
                {
                    string AUFNR = string.Empty;
                    foreach (FeedInput feedInput in noneZeroFeedInputList)
                    {
                        if (!string.IsNullOrWhiteSpace(feedInput.AUFNR))
                        {
                            if (AUFNR == string.Empty)
                            {
                                AUFNR = this.genericMgr.FindAll<string>("select ExternalOrderNo from OrderMaster where OrderNo = ?", orderNo).Single();
                            }
                            feedInput.AUFNR = AUFNR;
                        }

                        if (!string.IsNullOrWhiteSpace(feedInput.BWART))
                        {
                            feedInput.BWART = "261";
                        }
                    }
                }
                #endregion
            }
            #endregion

            #region 缓存出库需要的字段
            IList<Location> locationList = this.GetLocations(noneZeroFeedInputList.Select(f => f.LocationFrom).Distinct().ToList());
            IList<Item> feedItemList = this.itemMgr.GetItems(noneZeroFeedInputList.Select(f => f.Item).Distinct().ToList());
            foreach (FeedInput feedInput in noneZeroFeedInputList)
            {
                feedInput.ProductLine = productLine;
                feedInput.ProductLineFacility = productLineFacility;
                feedInput.CurrentProductLine = this.genericMgr.FindById<FlowMaster>(productLine);
                #region 生产线暂停检查
                if (feedInput.CurrentProductLine.IsPause)
                {
                    throw new BusinessException("生产线{0}已经暂停，不能投料。", feedInput.ProductLine);
                }
                #endregion
                if (string.IsNullOrWhiteSpace(feedInput.LocationFrom))
                {
                    throw new TechnicalException("LocationFrom not specified in FeedInput.");
                }
                //feedInput.CurrentLocationFrom = this.genericMgr.FindById<Location>(feedInput.LocationFrom);
                //feedInput.CurrentItem = this.genericMgr.FindById<Item>(feedInput.Item);
                feedInput.CurrentLocationFrom = locationList.Where(i => i.Code == feedInput.LocationFrom).Single();
                feedInput.CurrentItem = feedItemList.Where(i => i.Code == feedInput.Item).Single();

                if (string.IsNullOrWhiteSpace(feedInput.HuId))
                {

                    if (string.IsNullOrWhiteSpace(feedInput.Uom))
                    {
                        throw new TechnicalException("Uom not specified in FeedInput.");
                    }
                    feedInput.BaseUom = feedInput.CurrentItem.Uom;
                    if (feedInput.CurrentItem.Uom != feedInput.Uom)
                    {
                        feedInput.UnitQty = this.itemMgr.ConvertItemUomQty(feedInput.Item, feedInput.Uom, 1, feedInput.CurrentItem.Uom);  //记录单位转换系数
                    }
                    else
                    {
                        feedInput.UnitQty = 1;
                    }
                }
            }
            #endregion

            #region 循环操作库存
            foreach (FeedInput feedInput in noneZeroFeedInputList)
            {
                #region 投料
                IList<InventoryTransaction> inventoryTransactionList = this.locationDetailMgr.FeedProductRawMaterial(feedInput, effectiveDate);
                #endregion
            }
            #endregion
        }
        #endregion

        #region 导入投料
        public void FeedRawMaterialFromXls(Stream inputStream, string productLine, string productLineFacility, bool isForceFeed, DateTime effectiveDate)
        {
            FeedRawMaterialFromXls(inputStream, productLine, productLineFacility, null, isForceFeed, effectiveDate);
        }

        public void FeedRawMaterialFromXls(Stream inputStream, string orderNo, bool isForceFeed, DateTime effectiveDate)
        {
            FeedRawMaterialFromXls(inputStream, null, null, orderNo, isForceFeed, effectiveDate);
        }

        private void FeedRawMaterialFromXls(Stream inputStream, string productLine, string productLineFacility, string orderNo, bool isForceFeed, DateTime effectiveDate)
        {
            #region 导入数据
            if (inputStream.Length == 0)
            {
                throw new BusinessException("Import.Stream.Empty");
            }

            HSSFWorkbook workbook = new HSSFWorkbook(inputStream);

            ISheet sheet = workbook.GetSheetAt(0);
            IEnumerator rows = sheet.GetRowEnumerator();

            ImportHelper.JumpRows(rows, 11);

            #region 列定义
            int colItem = 1;//物料代码   
            int colUom = 3;//单位
            int colLocFrom = 4;// 来源库位
            int colQty = 5;//数量
            #endregion

            IList<FeedInput> feedInputList = new List<FeedInput>();
            while (rows.MoveNext())
            {
                HSSFRow row = (HSSFRow)rows.Current;
                if (!ImportHelper.CheckValidDataRow(row, 1, 9))
                {
                    break;//边界
                }
                string itemCode = string.Empty;
                decimal qty = 0;
                string uomCode = string.Empty;
                string locationFromCode = string.Empty;
                string locationToCode = string.Empty;

                #region 读取数据
                #region 读取物料代码
                itemCode = ImportHelper.GetCellStringValue(row.GetCell(colItem));
                if (itemCode == null || itemCode.Trim() == string.Empty)
                {
                    ImportHelper.ThrowCommonError(row.RowNum, colItem, row.GetCell(colItem));
                }

                #endregion

                #region 读取单位
                uomCode = row.GetCell(colUom) != null ? row.GetCell(colUom).StringCellValue : string.Empty;
                if (uomCode == null || uomCode.Trim() == string.Empty)
                {
                    throw new BusinessException("Import.Read.Error.Empty", (row.RowNum + 1).ToString(), colUom.ToString());
                }
                #endregion

                #region 读取来源库位
                locationFromCode = row.GetCell(colLocFrom) != null ? row.GetCell(colLocFrom).StringCellValue : string.Empty;
                if (string.IsNullOrEmpty(locationFromCode))
                {
                    throw new BusinessException("Import.Read.Error.Empty", (row.RowNum + 1).ToString(), colLocFrom.ToString());
                }

                Location locationFrom = genericMgr.FindById<Location>(locationFromCode);
                #endregion

                #region 读取数量
                try
                {
                    qty = Convert.ToDecimal(row.GetCell(colQty).NumericCellValue);
                }
                catch
                {
                    ImportHelper.ThrowCommonError(row.RowNum, colQty, row.GetCell(colQty));
                }
                #endregion
                #endregion

                #region 填充数据
                FeedInput feedInput = new FeedInput();
                feedInput.LocationFrom = locationFromCode;

                feedInput.Item = itemCode;
                feedInput.Uom = uomCode;
                feedInput.Qty = qty;
                feedInput.BaseUom = uomCode;

                feedInputList.Add(feedInput);
                #endregion
            }

            #endregion
            #region 投料
            FeedRawMaterial(productLine, productLineFacility, orderNo, feedInputList, isForceFeed, effectiveDate);
            #endregion
        }
        #endregion

        #endregion

        #region 生产单投料，投Kit单料
        [Transaction(TransactionMode.Requires)]
        public void FeedKitOrder(string orderNo, string kitOrderNo)
        {
            FeedKitOrder(orderNo, kitOrderNo, false, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public void FeedKitOrder(string orderNo, string kitOrderNo, bool isForceFeed)
        {
            FeedKitOrder(orderNo, kitOrderNo, isForceFeed, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public void FeedKitOrder(string orderNo, string kitOrderNo, DateTime effectiveDate)
        {
            FeedKitOrder(orderNo, kitOrderNo, false, effectiveDate);
        }

        [Transaction(TransactionMode.Requires)]
        public void FeedKitOrder(string orderNo, string kitOrderNo, bool isForceFeed, DateTime effectiveDate)
        {
            #region 查询订单
            IList<OrderMaster> orderMasterList = this.genericMgr.FindAll<OrderMaster>("from OrderMaster where OrderNo in (?, ?)",
                new object[] { orderNo, kitOrderNo });
            #endregion

            #region 检查
            OrderMaster productOrder = orderMasterList.Where(o => o.OrderNo == orderNo).Single();
            OrderMaster kitOrder = orderMasterList.Where(o => o.OrderNo == kitOrderNo).Single();

            if (productOrder.Type != CodeMaster.OrderType.Production
                || productOrder.Type == CodeMaster.OrderType.SubContract)
            {
                throw new TechnicalException("ProductOrder type is not correct.");
            }

            if (productOrder.Status != CodeMaster.OrderStatus.InProcess
                && productOrder.Status != CodeMaster.OrderStatus.Complete)
            {
                throw new BusinessException("生产单{0}的状态为{1}，不能投料。", productOrder.OrderNo,
                    systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)productOrder.Status).ToString()));
            }

            if (kitOrder.Status != CodeMaster.OrderStatus.Close
                && kitOrder.Status != CodeMaster.OrderStatus.Complete)
            {
                throw new BusinessException("KIT单{0}没有收货，不能投料。", kitOrder.OrderNo);
            }

            if (!isForceFeed && productOrder.TraceCode != kitOrder.TraceCode)
            {
                throw new BusinessException("KIT单{0}的VAN号{1}和生产单的VAN号{2}不一致。", kitOrder.OrderNo, kitOrder.TraceCode, productOrder.TraceCode);
            }

            #region 查询Kit单是否已经投料
            if (this.genericMgr.FindAll<long>("select count(*) as counter from ProductFeed where FeedOrder = ?", kitOrder.OrderNo)[0] > 0)
            {
                throw new BusinessException("KIT单{0}已经投料。", kitOrder.OrderNo);
            }
            #endregion
            #endregion

            #region 根据工位找工序
            OrderOperation orderOperation = this.genericMgr.FindAll<OrderOperation>("from OrderOperation where OrderNo = ? and OpReference = ?",
                new object[] { productOrder.OrderNo, kitOrder.LocationTo }).FirstOrDefault();
            #endregion

            #region 查询Kit收货单明细
            //循环查找kit单的绑定订单，要全部投料至生产线上。
            IList<string> childKitOrderNoList = NestGetChildKitOrderNo(kitOrder.OrderNo);
            string selectKitReceiptStatement = NativeSqlStatement.SELECT_KIT_RECEIPT_STATEMENT;
            IList<object> selectKitReceiptParm = new List<object>();
            selectKitReceiptParm.Add(kitOrder.OrderNo);
            if (childKitOrderNoList != null && childKitOrderNoList.Count > 0)
            {
                foreach (string childKitOrderNo in childKitOrderNoList)
                {
                    selectKitReceiptStatement += ",?";
                    selectKitReceiptParm.Add(childKitOrderNo);
                }
            }
            selectKitReceiptStatement += ")";
            IList<ReceiptLocationDetail> receiptLocationDetailList = this.genericMgr.FindEntityWithNativeSql<ReceiptLocationDetail>(selectKitReceiptStatement, selectKitReceiptParm.ToArray());
            IList<Item> itemList = this.itemMgr.GetItems(receiptLocationDetailList.Select(det => det.Item).Distinct().ToList());
            //查找订单明细，需要取Kit单的预留号、行号、SAP生产单号、批号、移动类型
            IList<OrderDetail> orderDetailList = this.LoadOrderDetails(receiptLocationDetailList.Select(r => r.OrderDetailId.Value).Distinct().ToArray());

            IList<FeedInput> feedInputList = (from det in receiptLocationDetailList
                                              group det by new
                                              {
                                                  Item = det.Item,
                                                  QualityType = det.QualityType,
                                                  HuId = det.HuId,
                                                  ReserveNo = orderDetailList.Where(od => od.Id == det.OrderDetailId).Single().ReserveNo,
                                                  ReserveLine = orderDetailList.Where(od => od.Id == det.OrderDetailId).Single().ReserveLine,
                                                  AUFNR = orderDetailList.Where(od => od.Id == det.OrderDetailId).Single().AUFNR,
                                                  ICHARG = orderDetailList.Where(od => od.Id == det.OrderDetailId).Single().ICHARG,
                                                  BWART = orderDetailList.Where(od => od.Id == det.OrderDetailId).Single().BWART,
                                              } into result
                                              select new FeedInput
                                              {
                                                  Item = result.Key.Item,
                                                  QualityType = result.Key.QualityType,
                                                  HuId = result.Key.HuId,
                                                  LocationFrom = kitOrder.LocationTo,  //投料的扣料库位为Kit单的目的库位
                                                  Uom = itemList.Where(i => i.Code == result.Key.Item).Single().Uom,
                                                  ReserveNo = result.Key.ReserveNo,
                                                  ReserveLine = result.Key.ReserveLine,
                                                  AUFNR = result.Key.AUFNR,
                                                  ICHARG = result.Key.ICHARG,
                                                  BWART = result.Key.BWART,
                                                  Qty = result.Sum(det => det.Qty)
                                              }).ToList();

            #region FeedInput的ITEM赋基本单位
            SetUom4FeedInput(feedInputList);
            #endregion
            #endregion

            #region 记录Kit单投料
            ProductFeed productFeed = new ProductFeed();
            productFeed.TraceCode = productOrder.TraceCode;
            productFeed.FeedOrder = kitOrderNo;
            productFeed.ProductOrder = orderNo;

            this.genericMgr.Create(productFeed);
            #endregion

            #region 投料
            FeedRawMaterial(orderNo, feedInputList, true, effectiveDate);
            #endregion
        }

        private IList<string> NestGetChildKitOrderNo(string orderNo)
        {
            IList<string> childKitOrderNoList = this.genericMgr.FindAll<string>("select FeedOrder from ProductFeed where ProductOrder = ?", orderNo);

            if (childKitOrderNoList != null && childKitOrderNoList.Count > 0)
            {
                foreach (string childKitOrderNo in childKitOrderNoList)
                {
                    IList<string> nextChildKitOrderNoList = NestGetChildKitOrderNo(childKitOrderNo);
                    if (nextChildKitOrderNoList != null && nextChildKitOrderNoList.Count > 0)
                    {
                        ((List<string>)childKitOrderNoList).AddRange(nextChildKitOrderNoList);
                    }
                }
            }

            return childKitOrderNoList;
        }

        private void SetUom4FeedInput(IList<FeedInput> feedInputList)
        {
            #region FeedInput的ITEM赋基本单位
            IList<string> itemCodeList = feedInputList.Where(f => !string.IsNullOrWhiteSpace(f.HuId)).Select(f => f.Item).Distinct().ToList();
            if (itemCodeList != null && itemCodeList.Count > 0)
            {
                string selectItemStatement = string.Empty;
                IList<object> selectItemParas = new List<object>();
                foreach (string itemCode in itemCodeList)
                {
                    if (selectItemStatement == string.Empty)
                    {
                        selectItemStatement = "from Item where Code in (?";
                    }
                    else
                    {
                        selectItemStatement += ", ?";
                    }
                    selectItemParas.Add(itemCode);
                }
                selectItemStatement += ")";

                IList<Item> itemList = this.genericMgr.FindAll<Item>(selectItemStatement, selectItemParas.ToArray());

                foreach (FeedInput feedInput in feedInputList.Where(f => !string.IsNullOrWhiteSpace(f.HuId)))
                {
                    feedInput.Uom = itemList.Where(i => i.Code == feedInput.Item).Single().Uom; //基本单位
                }
            }
            #endregion
        }

        private IList<OrderDetail> LoadOrderDetails(int[] orderDetIdList)
        {
            IList<object> para = new List<object>();

            string selectOrderDetailStatement = string.Empty;
            foreach (int id in orderDetIdList)
            {
                if (selectOrderDetailStatement == string.Empty)
                {
                    selectOrderDetailStatement = "from OrderDetail where Id in (?";
                }
                else
                {
                    selectOrderDetailStatement += ",?";
                }
                para.Add(id);
            }
            selectOrderDetailStatement += ")";

            return this.genericMgr.FindAll<OrderDetail>(selectOrderDetailStatement, para.ToArray());
        }
        #endregion

        #region 生产单投料，投工单
        [Transaction(TransactionMode.Requires)]
        public void FeedProductOrder(string orderNo, string feedOrderNo)
        {
            FeedProductOrder(orderNo, feedOrderNo, false, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public void FeedProductOrder(string orderNo, string feedOrderNo, bool isForceFeed)
        {
            FeedProductOrder(orderNo, feedOrderNo, isForceFeed, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public void FeedProductOrder(string orderNo, string feedOrderNo, DateTime effectiveDate)
        {
            FeedProductOrder(orderNo, feedOrderNo, false, effectiveDate);
        }

        [Transaction(TransactionMode.Requires)]
        public void FeedProductOrder(string orderNo, string feedOrderNo, bool isForceFeed, DateTime effectiveDate)
        {
            #region 查询订单
            IList<OrderMaster> orderMasterList = this.genericMgr.FindAll<OrderMaster>("from OrderMaster where OrderNo in (?, ?)",
                new object[] { orderNo, feedOrderNo });

            #endregion

            #region 检查
            OrderMaster productOrder = orderMasterList.Where(o => o.OrderNo == orderNo).Single();
            OrderMaster feedProductOrder = orderMasterList.Where(o => o.OrderNo == feedOrderNo).Single();

            if (productOrder.Type != CodeMaster.OrderType.Production
                || productOrder.Type == CodeMaster.OrderType.SubContract)
            {
                throw new TechnicalException("ProductOrder type is not correct.");
            }

            if (feedProductOrder.Type != CodeMaster.OrderType.Production
                || feedProductOrder.Type == CodeMaster.OrderType.SubContract)
            {
                throw new TechnicalException("FeedProductOrder type is not correct.");
            }

            if (productOrder.Status != CodeMaster.OrderStatus.InProcess
                && productOrder.Status != CodeMaster.OrderStatus.Complete)
            {
                throw new BusinessException("父生产单{0}的状态为{1}，不能投料。", productOrder.OrderNo,
                    systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)productOrder.Status).ToString()));
            }

            if (feedProductOrder.Status != CodeMaster.OrderStatus.Complete && feedProductOrder.Status != CodeMaster.OrderStatus.Close)
            {
                throw new BusinessException("子生产单{0}没有完工，不能投料。", feedProductOrder.OrderNo);
            }

            if (!isForceFeed && productOrder.TraceCode != feedProductOrder.TraceCode)
            {
                throw new BusinessException("子生产单{0}的VAN号{1}和父生产单的VAN号{2}不一致。", feedProductOrder.OrderNo, feedProductOrder.TraceCode, productOrder.TraceCode);
            }

            #region 查询生产单是否已经投料
            if (this.genericMgr.FindAll<long>("select count(*) as counter from ProductFeed where FeedOrder = ?", feedProductOrder.OrderNo)[0] > 0)
            {
                throw new BusinessException("子生产单{0}已经投料。", feedProductOrder.OrderNo);
            }
            #endregion
            #endregion

            #region 根据工位找工序
            OrderOperation orderOperation = this.genericMgr.FindAll<OrderOperation>("from OrderOperation where OrderNo = ? and OpReference = ?",
                new object[] { productOrder.OrderNo, feedProductOrder.LocationTo }).FirstOrDefault();
            #endregion

            #region 查找投料工单的收货记录
            string selectFeedOrderReceiptStatement = "from ReceiptLocationDetail where OrderDetailId in (select Id from OrderDetail where OrderNo = ?))";
            IList<ReceiptLocationDetail> receiptLocationDetailList = this.genericMgr.FindAll<ReceiptLocationDetail>(selectFeedOrderReceiptStatement, feedProductOrder.OrderNo);
            IList<Item> itemList = this.itemMgr.GetItems(receiptLocationDetailList.Select(det => det.Item).Distinct().ToList());

            IList<FeedInput> feedInputList = (from det in receiptLocationDetailList
                                              group det by new { Item = det.Item, QualityType = det.QualityType, HuId = det.HuId } into result
                                              select new FeedInput
                                              {
                                                  Item = result.Key.Item,
                                                  QualityType = result.Key.QualityType,
                                                  HuId = result.Key.HuId,
                                                  LocationFrom = feedProductOrder.LocationTo,  //投料的扣料库位为Kit单的目的库位
                                                  Uom = itemList.Where(i => i.Code == result.Key.Item).Single().Uom,
                                                  Qty = result.Sum(det => det.Qty),
                                                  NotReport = true,                         //过滤掉驾驶室和底盘总成，不需要传给SAP
                                              }).ToList();

            #region FeedInput的ITEM赋基本单位
            SetUom4FeedInput(feedInputList);
            #endregion
            #endregion

            #region 记录生产单投料
            ProductFeed productFeed = new ProductFeed();
            productFeed.TraceCode = productOrder.TraceCode;
            productFeed.FeedOrder = feedOrderNo;
            productFeed.ProductOrder = orderNo;

            this.genericMgr.Create(productFeed);
            #endregion

            #region 投料
            //投子工单一定是强制投料，因为父工单的Bom不包含子工单的成品
            FeedRawMaterial(orderNo, feedInputList, true, effectiveDate);
            #endregion
        }
        #endregion

        #region 生产退料
        [Transaction(TransactionMode.Requires)]
        public void ReturnRawMaterial(string productLine, string productLineFacility, IList<ReturnInput> returnInputList)
        {
            ReturnRawMaterial(productLine, productLineFacility, null, null, null, null, returnInputList, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public void ReturnRawMaterial(string productLine, string productLineFacility, IList<ReturnInput> returnInputList, DateTime effectiveDate)
        {
            ReturnRawMaterial(productLine, productLineFacility, null, null, null, null, returnInputList, effectiveDate);
        }

        [Transaction(TransactionMode.Requires)]
        public void ReturnRawMaterial(string orderNo, string traceCode, int? operation, string opReference, IList<ReturnInput> returnInputList)
        {
            ReturnRawMaterial(orderNo, traceCode, operation, opReference, returnInputList, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public void ReturnRawMaterial(string orderNo, string traceCode, int? operation, string opReference, IList<ReturnInput> returnInputList, DateTime effectiveDate)
        {
            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderNo);

            if (orderMaster.Type != com.Sconit.CodeMaster.OrderType.Production && orderMaster.Type != com.Sconit.CodeMaster.OrderType.SubContract)
            {
                throw new TechnicalException("非生产单不能进行退料。");
            }

            #region 检查生产单
            if (orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.InProcess && orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Complete)
            {
                throw new BusinessException("状态为{0}的生产单不能退料。", systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
            }
            #endregion

            #region 批量对投料对象赋订单的值
            if (returnInputList != null)
            {
                foreach (ReturnInput returnInput in returnInputList)
                {
                    returnInput.OrderNo = orderNo;
                    returnInput.CurrentOrderMaster = orderMaster;
                    returnInput.OrderType = orderMaster.Type;
                    returnInput.OrderSubType = orderMaster.SubType;
                    returnInput.Operation = operation;
                    returnInput.OpReference = opReference;
                }
            }
            #endregion

            ReturnRawMaterial(orderMaster.Flow, orderMaster.ProductLineFacility, orderNo, traceCode, operation, opReference, returnInputList, effectiveDate);
        }

        [Transaction(TransactionMode.Requires)]
        public void ReturnRawMaterial(IList<ReturnInput> returnInputList)
        {
            ReturnRawMaterial(returnInputList, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public void ReturnRawMaterial(IList<ReturnInput> returnInputList, DateTime effectiveDate)
        {
            #region 投料不能为空检验
            IList<ReturnInput> noneZeroReturnInputList = null;
            if (returnInputList != null)
            {
                noneZeroReturnInputList = returnInputList.Where(f => f.Qty > 0 && f.UnitQty > 0 && f.ProductLineLocationDetailId.HasValue).ToList();
            }
            if (noneZeroReturnInputList == null || noneZeroReturnInputList.Count == 0)
            {
                throw new BusinessException("退料零件不能为空。");
            }
            #endregion

            #region 循环操作库存
            foreach (ReturnInput returnInput in returnInputList)
            {
                #region 退料
                IList<InventoryTransaction> inventoryTransactionList = this.locationDetailMgr.ReturnProductRawMaterial(returnInput, effectiveDate);
                #endregion
            }
            #endregion
        }

        private void ReturnRawMaterial(string productLine, string productLineFacility, string orderNo, string traceCode, int? operation, string opReference, IList<ReturnInput> returnInputList, DateTime effectiveDate)
        {
            #region 投料不能为空检验
            IList<ReturnInput> noneZeroReturnInputList = null;
            if (returnInputList != null)
            {
                noneZeroReturnInputList = returnInputList.Where(f => f.Qty > 0).ToList();
            }
            if (noneZeroReturnInputList == null || noneZeroReturnInputList.Count == 0)
            {
                throw new BusinessException("退料零件不能为空。");
            }
            #endregion

            #region 查找条码填充零件、数量和质量状态等数据
            foreach (ReturnInput returnInput in noneZeroReturnInputList)
            {
                if (!string.IsNullOrWhiteSpace(returnInput.HuId))
                {
                    Hu hu = this.genericMgr.FindById<Hu>(returnInput.HuId);

                    //todo 检验投料的条码是否是OrderBomDetail上指定的库位投料

                    //不用判断冻结和占用，出库方法中会判断
                    returnInput.Item = hu.Item;
                    returnInput.LotNo = hu.LotNo;
                    //returnInput.QualityType = hu.QualityType;
                    returnInput.QualityType = com.Sconit.CodeMaster.QualityType.Qualified;
                    returnInput.Qty = hu.Qty;
                    returnInput.Uom = hu.Uom;
                    returnInput.BaseUom = hu.BaseUom;
                    returnInput.UnitQty = hu.UnitQty;
                }
            }
            #endregion

            #region 缓存出库需要的字段
            //IList<Item> feedItemList = this.itemMgr.GetItems(noneZeroReturnInputList.Select(r => r.Item).Distinct().ToList());
            foreach (ReturnInput returnInput in noneZeroReturnInputList)
            {
                returnInput.ProductLine = productLine;
                returnInput.ProductLineFacility = productLineFacility;
                returnInput.CurrentProductLine = this.genericMgr.FindById<FlowMaster>(productLine);
                //if (string.IsNullOrWhiteSpace(returnInput.LocationTo) && string.IsNullOrWhiteSpace(returnInput.HuId))
                //{
                //    throw new TechnicalException("LocationTo not specified in FeedInput.");
                //}

                if (!string.IsNullOrWhiteSpace(returnInput.LocationTo))
                {
                    returnInput.CurrentLocationTo = this.genericMgr.FindById<Location>(returnInput.LocationTo);
                }

                if (string.IsNullOrWhiteSpace(returnInput.HuId))
                {
                    Item item = this.genericMgr.FindById<Item>(returnInput.Item);
                    if (string.IsNullOrWhiteSpace(returnInput.Uom))
                    {
                        throw new TechnicalException("Uom not specified in FeedInput.");
                    }
                    returnInput.BaseUom = item.Uom;
                    if (item.Uom != returnInput.Uom)
                    {
                        returnInput.UnitQty = this.itemMgr.ConvertItemUomQty(returnInput.Item, returnInput.Uom, 1, item.Uom);  //记录单位转换系数
                    }
                    else
                    {
                        returnInput.UnitQty = 1;
                    }
                }
            }
            #endregion

            #region 循环操作库存
            foreach (ReturnInput returnInput in noneZeroReturnInputList)
            {
                #region 退料
                IList<InventoryTransaction> inventoryTransactionList = this.locationDetailMgr.ReturnProductRawMaterial(returnInput, effectiveDate);
                #endregion
            }
            #endregion
        }
        #endregion

        #region 加权平均回冲
        public void BackflushWeightAverage(string productLine, string productLineFacility, IList<WeightAverageBackflushInput> backflushInputList)
        {
            BackflushWeightAverage(productLine, productLineFacility, backflushInputList, DateTime.Now);
        }

        public void BackflushWeightAverage(string productLine, string productLineFacility, IList<WeightAverageBackflushInput> backflushInputList, DateTime effectiveDate)
        {
            if ((from f in backflushInputList
                 group f by f.Item into result
                 where result.Count() > 1
                 select result).Count() > 0)
            {
                throw new TechnicalException("Backflush item duplicate.");
            }

            #region 投料不能为空检验
            IList<WeightAverageBackflushInput> noneZeroBackflushInputList = null;
            if (backflushInputList != null)
            {
                noneZeroBackflushInputList = backflushInputList.Where(f => f.Qty > 0).ToList();
            }
            if (noneZeroBackflushInputList == null || noneZeroBackflushInputList.Count == 0)
            {
                throw new BusinessException("回冲零件不能为空。");
            }
            #endregion

            FlowMaster CurrentProductLine = this.genericMgr.FindById<FlowMaster>(productLine);
            IList<Item> feedItemList = this.itemMgr.GetItems(noneZeroBackflushInputList.Select(f => f.Item).Distinct().ToList());
            foreach (WeightAverageBackflushInput backflushInput in noneZeroBackflushInputList)
            {
                backflushInput.ProductLine = productLine;
                backflushInput.ProductLineFacility = productLineFacility;
                backflushInput.CurrentProductLine = CurrentProductLine;
                //Item item = this.genericMgr.FindById<Item>(backflushInput.Item);
                Item item = feedItemList.Where(f => f.Code == backflushInput.Item).Single();
                backflushInput.BaseUom = item.Uom;

                if (backflushInput.Uom != backflushInput.Uom)
                {
                    backflushInput.UnitQty = this.itemMgr.ConvertItemUomQty(backflushInput.Item, backflushInput.Uom, 1, item.Uom);  //记录单位转换系数
                }
                else
                {
                    backflushInput.UnitQty = 1;
                }
            }

            this.locationDetailMgr.BackflushProductWeightAverageRawMaterial(backflushInputList, effectiveDate);
        }
        #endregion

        #region 回冲物料
        public void BackflushProductOrder(IList<OrderDetail> orderDetailList)
        {
            this.BackflushProductOrder(orderDetailList, DateTime.Now);
        }

        public void BackflushProductOrder(IList<OrderDetail> orderDetailList, DateTime effectiveDate)
        {
            #region 判断是否全0收货
            if (orderDetailList == null || orderDetailList.Count == 0)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_ReceiveDetailIsEmpty);
            }

            IList<OrderDetail> nonZeroOrderDetailList = orderDetailList.Where(o => o.ReceiveQtyInput != 0 || o.ScrapQtyInput != 0).ToList();

            if (nonZeroOrderDetailList.Count == 0)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_ReceiveDetailIsEmpty);
            }

            if (nonZeroOrderDetailList.Count > 1)
            {
                throw new BusinessException("收货明细大于1。");
            }
            #endregion

            #region 查询生产单和生产线
            //不能跨生产单同时收货
            string orderNo = nonZeroOrderDetailList.Select(o => o.OrderNo).Distinct().Single();
            OrderMaster orderMaster = this.genericMgr.FindById<OrderMaster>(orderNo);
            FlowMaster productLine = this.genericMgr.FindById<FlowMaster>(orderMaster.Flow);
            #endregion

            #region 查询订单Bom
            string selectOrderBomDetailStatement = string.Empty;
            IList<object> selectOrderBomDetailPara = new List<object>();
            foreach (OrderDetail orderDetail in nonZeroOrderDetailList)
            {
                if (selectOrderBomDetailStatement == string.Empty)
                {
                    selectOrderBomDetailStatement = "from OrderBomDetail where OrderDetailId in(?";
                }
                else
                {
                    selectOrderBomDetailStatement += ",?";
                }
                selectOrderBomDetailPara.Add(orderDetail.Id);
            }
            selectOrderBomDetailStatement += ")";
            IList<OrderBomDetail> orderBomDetailList = this.genericMgr.FindAll<OrderBomDetail>(selectOrderBomDetailStatement, selectOrderBomDetailPara.ToArray());
            #endregion

            #region 查询待回冲物料
            #region 查询冲投料至工单的物料
            string selectProductLineLocationDetailInOrderStatement = "from ProductLineLocationDetail where OrderNo = ?";
            IList<object> selectProductLineLocationDetailInOrderPara = new List<object>();
            selectProductLineLocationDetailInOrderPara.Add(orderNo);
            #region 判断是否有Van号
            foreach (OrderDetail orderDetail in nonZeroOrderDetailList)
            {
                foreach (OrderDetailInput orderDetailInput in orderDetail.OrderDetailInputs)
                {
                    if (!string.IsNullOrWhiteSpace(orderDetailInput.TraceCode))
                    {
                        if (selectProductLineLocationDetailInOrderPara.Count == 1)
                        {
                            selectProductLineLocationDetailInOrderStatement += " and TraceCode in (?";
                        }
                        else
                        {
                            selectProductLineLocationDetailInOrderStatement += ",?";
                        }
                        selectProductLineLocationDetailInOrderPara.Add(orderDetailInput.TraceCode);
                    }
                }
            }
            if (selectProductLineLocationDetailInOrderPara.Count > 1)
            {
                selectProductLineLocationDetailInOrderStatement += ")";
            }
            #endregion
            IList<ProductLineLocationDetail> productLineLocationDetailInOrderList = this.genericMgr.FindAll<ProductLineLocationDetail>(selectProductLineLocationDetailInOrderStatement, selectProductLineLocationDetailInOrderPara.ToArray());
            #endregion

            #region 查询冲投料至生产线的物料
            string selectProductLineLocationDetailStatement = "from ProductLineLocationDetail where ProductLine = ? and OrderNo is null and Item in (select Item from OrderBomDetail where OrderNo = ?)";
            IList<object> selectProductLineLocationDetailPara = new List<object>();
            selectProductLineLocationDetailPara.Add(orderMaster.Flow);
            selectProductLineLocationDetailPara.Add(orderNo);
            if (!string.IsNullOrWhiteSpace(orderMaster.ProductLineFacility))
            {
                selectProductLineLocationDetailStatement += " and ProductLineFacility = ?";
                selectProductLineLocationDetailPara.Add(orderMaster.ProductLineFacility);
            }
            IList<ProductLineLocationDetail> productLineLocationDetailList = this.genericMgr.FindAll<ProductLineLocationDetail>(selectProductLineLocationDetailStatement, selectProductLineLocationDetailPara.ToArray());
            #endregion

            IList<BackflushInput> backflushInputList = new List<BackflushInput>();

            #region 根据OrderBomDetail生成收货回冲和加权平均的回冲记录
            foreach (OrderBomDetail orderBomDetail in orderBomDetailList.Where(bomDet => bomDet.BackFlushMethod != CodeMaster.BackFlushMethod.BackFlushOrder))
            {
                //OrderDetail orderDetail = nonZeroOrderDetailList.Where(o => o.Id == orderBomDetail.OrderDetailId).Single();
                OrderDetail orderDetail = nonZeroOrderDetailList[0];
                OrderDetailInput orderDetailInput = orderDetail.OrderDetailInputs[0];

                BackflushInput backFlushInput = new BackflushInput();

                backFlushInput.ProductLine = orderMaster.Flow;
                backFlushInput.ProductLineFacility = orderMaster.ProductLineFacility;
                backFlushInput.CurrentProductLine = productLine;
                backFlushInput.OrderNo = orderMaster.OrderNo;
                backFlushInput.OrderType = orderMaster.Type;
                backFlushInput.OrderSubType = orderMaster.SubType;
                backFlushInput.OrderDetailId = orderDetail.Id;
                backFlushInput.OrderDetailSequence = orderDetail.Sequence;
                backFlushInput.OrderBomDetail = orderBomDetail;
                backFlushInput.ReceiptNo = orderDetailInput.ReceiptDetail.ReceiptNo;
                backFlushInput.ReceiptDetailId = orderDetailInput.ReceiptDetail.Id;
                backFlushInput.ReceiptDetailSequence = orderDetailInput.ReceiptDetail.Sequence;
                backFlushInput.FGItem = orderDetail.Item;
                backFlushInput.Item = orderBomDetail.Item;
                backFlushInput.ItemDescription = orderBomDetail.ItemDescription;
                backFlushInput.ReferenceItemCode = orderBomDetail.ReferenceItemCode;
                backFlushInput.Uom = orderBomDetail.Uom;
                backFlushInput.BaseUom = orderBomDetail.BaseUom;
                backFlushInput.UnitQty = orderBomDetail.UnitQty;   //基本单位转换率 = 订单单位/库存单位，转换为库存单位消耗 = 单位用量（订单单位） / 基本单位转换率
                backFlushInput.TraceCode = orderDetailInput.TraceCode;
                //backFlushInput.QualityType = CodeMaster.QualityType.Qualified;
                backFlushInput.FGQualityType = orderDetailInput.QualityType;  //收货成品的质量状态

                if (orderBomDetail.BackFlushMethod == CodeMaster.BackFlushMethod.GoodsReceive)
                {
                    #region 按比例回冲
                    backFlushInput.ProductLineLocationDetailList = productLineLocationDetailList.Where(p => p.Item == orderBomDetail.Item).ToList();
                    backFlushInput.Qty = (orderDetailInput.ReceiveQty + orderDetailInput.ScrapQty) * orderBomDetail.BomUnitQty;
                    backFlushInput.Operation = orderBomDetail.Operation;
                    backFlushInput.OpReference = orderBomDetail.OpReference;
                    #endregion
                }
                else if (orderBomDetail.BackFlushMethod == CodeMaster.BackFlushMethod.WeightAverage)
                {
                    #region 加权平均回冲
                    backFlushInput.Qty = (orderDetailInput.ReceiveQty + orderDetailInput.ScrapQty) * orderBomDetail.BomUnitQty;
                    #endregion
                }

                backflushInputList.Add(backFlushInput);
            }
            #endregion

            #region 投料至工单的在制品全部要回冲
            var groupedProductLineLocationDetailList = from locDet in productLineLocationDetailInOrderList
                                                       group locDet by new
                                                       {
                                                           OrderDetailId = locDet.OrderDetailId,
                                                           Item = locDet.Item,
                                                       } into gj
                                                       select new
                                                       {
                                                           OrderDetailId = gj.Key.OrderDetailId,
                                                           Item = gj.Key.Item,
                                                           ProductLineLocationDetailList = gj.ToList()
                                                       };

            IList<Item> itemList = this.itemMgr.GetItems(groupedProductLineLocationDetailList.Select(d => d.Item).Distinct().ToList());

            foreach (var groupedProductLineLocationDetail in groupedProductLineLocationDetailList)
            {
                //OrderDetail orderDetail = orderDetailList.Where(o => o.Id == groupedProductLineLocationDetail.OrderDetailId).Single();
                OrderDetail orderDetail = nonZeroOrderDetailList[0];
                OrderDetailInput orderDetailInput = orderDetail.OrderDetailInputs[0];

                Item item = itemList.Where(i => i.Code == groupedProductLineLocationDetail.Item).Single();

                BackflushInput backFlushInput = new BackflushInput();

                backFlushInput.ProductLine = orderMaster.Flow;
                backFlushInput.ProductLineFacility = orderMaster.ProductLineFacility;
                backFlushInput.CurrentProductLine = productLine;
                backFlushInput.OrderNo = orderMaster.OrderNo;
                backFlushInput.OrderType = orderMaster.Type;
                backFlushInput.OrderSubType = orderMaster.SubType;
                backFlushInput.OrderDetailId = orderDetail.Id;
                backFlushInput.OrderDetailSequence = orderDetail.Sequence;
                backFlushInput.ReceiptNo = orderDetailInput.ReceiptDetail.ReceiptNo;
                backFlushInput.ReceiptDetailId = orderDetailInput.ReceiptDetail.Id;
                backFlushInput.ReceiptDetailSequence = orderDetailInput.ReceiptDetail.Sequence;
                //backFlushInput.ReceiptDetailSequence = orderDetailInput.ReceiptDetail.Sequence;
                backFlushInput.FGItem = orderDetail.Item;
                backFlushInput.Item = item.Code;
                backFlushInput.ItemDescription = item.Description;
                backFlushInput.ReferenceItemCode = item.ReferenceCode;
                backFlushInput.Uom = item.Uom;
                backFlushInput.BaseUom = item.Uom;
                backFlushInput.UnitQty = 1;
                backFlushInput.TraceCode = orderDetailInput.TraceCode;
                //backFlushInput.QualityType = CodeMaster.QualityType.Qualified;
                backFlushInput.FGQualityType = orderDetailInput.QualityType;
                backFlushInput.Qty = groupedProductLineLocationDetail.ProductLineLocationDetailList.Sum(g => g.RemainBackFlushQty);
                backFlushInput.ProductLineLocationDetailList = groupedProductLineLocationDetail.ProductLineLocationDetailList;

                backflushInputList.Add(backFlushInput);
            }
            #endregion
            #endregion

            #region 记录工单投料明细/旧
            //DateTime dateTimeNow = DateTime.Now;
            //User currentUser = SecurityContextHolder.Get();

            //#region 汇总收货回冲投料明细
            //IList<OrderBackflushDetail> orderBackflushDetailList = (from input in backflushInputList
            //                                                        where input.OrderBomDetail != null && input.OrderBomDetail.BackFlushMethod == CodeMaster.BackFlushMethod.GoodsReceive
            //                                                        select new OrderBackflushDetail
            //                                                        {
            //                                                            OrderNo = input.OrderNo,
            //                                                            OrderDetailId = input.OrderDetailId,
            //                                                            OrderDetailSequence = input.OrderDetailSequence,
            //                                                            OrderBomDetailId = input.OrderBomDetail.Id,
            //                                                            OrderBomDetailSequence = input.OrderBomDetail.Sequence,
            //                                                            ReceiptNo = input.ReceiptNo,
            //                                                            ReceiptDetailId = input.ReceiptDetailId,
            //                                                            ReceiptDetailSequence = input.ReceiptDetailSequence,
            //                                                            Bom = input.OrderBomDetail != null ? input.OrderBomDetail.Bom : null,
            //                                                            Item = input.Item,
            //                                                            ItemDescription = input.OrderBomDetail.ItemDescription,
            //                                                            ReferenceItemCode = input.OrderBomDetail.ReferenceItemCode,
            //                                                            Uom = input.Uom,
            //                                                            BaseUom = input.BaseUom,
            //                                                            UnitQty = input.UnitQty,
            //                                                            ManufactureParty = input.OrderBomDetail.ManufactureParty,
            //                                                            TraceCode = input.TraceCode,
            //                                                            //HuId = result.Key.HuId,
            //                                                            //LotNo = result.Key.LotNo,
            //                                                            Operation = input.Operation,
            //                                                            OpReference = input.OpReference,
            //                                                            BackflushedQty = input.FGQualityType == CodeMaster.QualityType.Qualified ? input.Qty : 0,   //根据收货成品的质量状态记录至不同的回冲数量中
            //                                                            BackflushedRejectQty = input.FGQualityType == CodeMaster.QualityType.Reject ? input.Qty : 0,
            //                                                            //BackflushedScrapQty = input.BackflushedQty,
            //                                                            LocationFrom = input.OrderBomDetail.Location,
            //                                                            ProductLine = input.ProductLine,
            //                                                            ProductLineFacility = input.ProductLineFacility,
            //                                                            ReserveNo = input.OrderBomDetail.ReserveNo,
            //                                                            ReserveLine = input.OrderBomDetail.ReserveLine,
            //                                                            AUFNR = input.OrderBomDetail.AUFNR,
            //                                                            EffectiveDate = effectiveDate,
            //                                                            CreateUserId = currentUser.Id,
            //                                                            CreateUserName = currentUser.FullName,
            //                                                            CreateDate = dateTimeNow,
            //                                                        }).ToList();
            //#endregion

            //#region 汇总工单在制品投料明细
            //foreach (BackflushInput backflushInput in backflushInputList.Where(i => i.OrderBomDetail == null
            //                                        || i.OrderBomDetail.BackFlushMethod == CodeMaster.BackFlushMethod.BackFlushOrder))
            //{
            //    ((List<OrderBackflushDetail>)orderBackflushDetailList).AddRange(from p in backflushInput.ProductLineLocationDetailList
            //                                                                    group p by new
            //                                                                    {
            //                                                                        HuId = p.HuId,
            //                                                                        LotNo = p.LotNo,
            //                                                                        Operation = p.Operation,
            //                                                                        OpReference = p.OpReference,
            //                                                                        LocationFrom = p.LocationFrom
            //                                                                    } into result
            //                                                                    select new OrderBackflushDetail
            //                                                                    {
            //                                                                        OrderNo = backflushInput.OrderNo,
            //                                                                        OrderDetailId = backflushInput.OrderDetailId,
            //                                                                        OrderDetailSequence = backflushInput.OrderDetailSequence,
            //                                                                        OrderBomDetailId = backflushInput.OrderBomDetail != null ? (int?)backflushInput.OrderBomDetail.Id : null,
            //                                                                        OrderBomDetailSequence = backflushInput.OrderBomDetail != null ? (int?)backflushInput.OrderBomDetail.Sequence : null,
            //                                                                        ReceiptNo = backflushInput.ReceiptNo,
            //                                                                        ReceiptDetailId = backflushInput.ReceiptDetailId,
            //                                                                        ReceiptDetailSequence = backflushInput.ReceiptDetailSequence,
            //                                                                        Bom = backflushInput.OrderBomDetail != null ? backflushInput.OrderBomDetail.Bom : null,
            //                                                                        Item = backflushInput.Item,
            //                                                                        ItemDescription = backflushInput.OrderBomDetail != null ? backflushInput.OrderBomDetail.ItemDescription : null,
            //                                                                        ReferenceItemCode = backflushInput.OrderBomDetail != null ? backflushInput.OrderBomDetail.ReferenceItemCode : null,
            //                                                                        Uom = backflushInput.Uom,
            //                                                                        BaseUom = backflushInput.BaseUom,
            //                                                                        UnitQty = backflushInput.UnitQty,
            //                                                                        ManufactureParty = backflushInput.OrderBomDetail != null ? backflushInput.OrderBomDetail.ManufactureParty : null,
            //                                                                        TraceCode = backflushInput.TraceCode,
            //                                                                        HuId = result.Key.HuId,
            //                                                                        LotNo = result.Key.LotNo,
            //                                                                        Operation = result.Key.Operation,
            //                                                                        OpReference = result.Key.OpReference,
            //                                                                        BackflushedQty = backflushInput.FGQualityType == CodeMaster.QualityType.Qualified ? result.Sum(p => p.Qty) : 0,   //根据收货成品的质量状态记录至不同的回冲数量中
            //                                                                        BackflushedRejectQty = backflushInput.FGQualityType == CodeMaster.QualityType.Reject ? result.Sum(p => p.Qty) : 0,
            //                                                                        //BackflushedScrapQty = input.BackflushedQty,
            //                                                                        LocationFrom = result.Key.LocationFrom,
            //                                                                        ProductLine = backflushInput.ProductLine,
            //                                                                        ProductLineFacility = backflushInput.ProductLineFacility,
            //                                                                        ReserveNo = backflushInput.OrderBomDetail != null ? backflushInput.OrderBomDetail.ReserveNo : null,
            //                                                                        ReserveLine = backflushInput.OrderBomDetail != null ? backflushInput.OrderBomDetail.ReserveLine : null,
            //                                                                        AUFNR = backflushInput.OrderBomDetail != null ? backflushInput.OrderBomDetail.AUFNR : orderMaster.ExternalOrderNo, //如果OrderBomDetail为空，说明是强制投料，用ExternalOrderNo作为SAP生产单号
            //                                                                        EffectiveDate = effectiveDate,
            //                                                                        CreateUserId = currentUser.Id,
            //                                                                        CreateUserName = currentUser.FullName,
            //                                                                        CreateDate = dateTimeNow,
            //                                                                    });
            //}
            //#endregion

            //foreach (OrderBackflushDetail orderBackflushDetail in orderBackflushDetailList)
            //{
            //    this.genericMgr.Create(orderBackflushDetail);
            //}

            #endregion

            #region 回冲物料
            this.locationDetailMgr.BackflushProductMaterial(backflushInputList);
            #endregion

            #region 记录工单投料明细/新
            DateTime dateTimeNow = DateTime.Now;
            User currentUser = SecurityContextHolder.Get();
            IList<OrderBackflushDetail> orderBackflushDetailList = new List<OrderBackflushDetail>();

            #region 汇总收货回冲投料明细
            foreach (BackflushInput input in backflushInputList.Where(input => input.OrderBomDetail != null && input.OrderBomDetail.BackFlushMethod == CodeMaster.BackFlushMethod.GoodsReceive))
            {
                ((List<OrderBackflushDetail>)orderBackflushDetailList).AddRange(from trans in input.InventoryTransactionList
                                                                                group trans by (trans.IsConsignment ? trans.PlanBill : null) into g
                                                                                select new OrderBackflushDetail
                                                                                {
                                                                                    OrderNo = input.OrderNo,
                                                                                    OrderDetailId = input.OrderDetailId,
                                                                                    OrderDetailSequence = input.OrderDetailSequence,
                                                                                    OrderBomDetailId = input.OrderBomDetail.Id,
                                                                                    OrderBomDetailSequence = input.OrderBomDetail.Sequence,
                                                                                    ReceiptNo = input.ReceiptNo,
                                                                                    ReceiptDetailId = input.ReceiptDetailId,
                                                                                    ReceiptDetailSequence = input.ReceiptDetailSequence,
                                                                                    Bom = input.OrderBomDetail != null ? input.OrderBomDetail.Bom : null,
                                                                                    FGItem = input.FGItem,
                                                                                    Item = input.Item,
                                                                                    ItemDescription = input.ItemDescription,
                                                                                    ReferenceItemCode = input.ReferenceItemCode,
                                                                                    Uom = input.Uom,
                                                                                    BaseUom = input.BaseUom,
                                                                                    UnitQty = input.UnitQty,
                                                                                    ManufactureParty = input.OrderBomDetail.ManufactureParty,
                                                                                    TraceCode = input.TraceCode,
                                                                                    //HuId = result.Key.HuId,
                                                                                    //LotNo = result.Key.LotNo,
                                                                                    Operation = input.Operation,
                                                                                    OpReference = input.OpReference,
                                                                                    BackflushedQty = input.FGQualityType == CodeMaster.QualityType.Qualified ? g.Sum(trans => trans.Qty) / input.UnitQty : 0,   //根据收货成品的质量状态记录至不同的回冲数量中
                                                                                    BackflushedRejectQty = input.FGQualityType == CodeMaster.QualityType.Reject ? g.Sum(trans => trans.Qty) / input.UnitQty : 0,
                                                                                    //BackflushedScrapQty = input.BackflushedQty,
                                                                                    LocationFrom = input.OrderBomDetail.Location,
                                                                                    ProductLine = input.ProductLine,
                                                                                    ProductLineFacility = input.ProductLineFacility,
                                                                                    ReserveNo = input.OrderBomDetail.ReserveNo,
                                                                                    ReserveLine = input.OrderBomDetail.ReserveLine,
                                                                                    AUFNR = input.OrderBomDetail.AUFNR,
                                                                                    ICHARG = input.OrderBomDetail.ICHARG,
                                                                                    BWART = input.OrderBomDetail.BWART,
                                                                                    NotReport = false,  //理论都需要汇报
                                                                                    PlanBill = g.Key,
                                                                                    EffectiveDate = effectiveDate,
                                                                                    CreateUserId = currentUser.Id,
                                                                                    CreateUserName = currentUser.FullName,
                                                                                    CreateDate = dateTimeNow,
                                                                                    IsVoid = false,
                                                                                });
            }
            #endregion

            #region 汇总工单在制品投料明细
            foreach (BackflushInput backflushInput in backflushInputList.Where(i => i.OrderBomDetail == null
                                                    || i.OrderBomDetail.BackFlushMethod == CodeMaster.BackFlushMethod.BackFlushOrder))
            {
                ((List<OrderBackflushDetail>)orderBackflushDetailList).AddRange(from p in backflushInput.InventoryTransactionList
                                                                                group p by new
                                                                                {
                                                                                    HuId = p.HuId,
                                                                                    LotNo = p.LotNo,
                                                                                    Operation = p.Operation,
                                                                                    OpReference = p.OpReference,
                                                                                    LocationFrom = p.OrgLocation,
                                                                                    PlanBill = p.IsConsignment ? p.PlanBill : null,
                                                                                    ReserveNo = p.ReserveNo,
                                                                                    ReserveLine = p.ReserveLine,
                                                                                    AUFNR = p.AUFNR,
                                                                                    ICHARG = p.ICHARG,
                                                                                    BWART = p.BWART,
                                                                                    NotReport = p.NotReport,
                                                                                } into result
                                                                                select new OrderBackflushDetail
                                                                                {
                                                                                    OrderNo = backflushInput.OrderNo,
                                                                                    OrderDetailId = backflushInput.OrderDetailId,
                                                                                    OrderDetailSequence = backflushInput.OrderDetailSequence,
                                                                                    OrderBomDetailId = backflushInput.OrderBomDetail != null ? (int?)backflushInput.OrderBomDetail.Id : null,
                                                                                    OrderBomDetailSequence = backflushInput.OrderBomDetail != null ? (int?)backflushInput.OrderBomDetail.Sequence : null,
                                                                                    ReceiptNo = backflushInput.ReceiptNo,
                                                                                    ReceiptDetailId = backflushInput.ReceiptDetailId,
                                                                                    ReceiptDetailSequence = backflushInput.ReceiptDetailSequence,
                                                                                    Bom = backflushInput.OrderBomDetail != null ? backflushInput.OrderBomDetail.Bom : null,
                                                                                    FGItem = backflushInput.FGItem,
                                                                                    Item = backflushInput.Item,
                                                                                    ItemDescription = backflushInput.ItemDescription,
                                                                                    ReferenceItemCode = backflushInput.ReferenceItemCode,
                                                                                    Uom = backflushInput.Uom,
                                                                                    BaseUom = backflushInput.BaseUom,
                                                                                    UnitQty = backflushInput.UnitQty,
                                                                                    ManufactureParty = backflushInput.OrderBomDetail != null ? backflushInput.OrderBomDetail.ManufactureParty : null,
                                                                                    TraceCode = backflushInput.TraceCode,
                                                                                    HuId = result.Key.HuId,
                                                                                    LotNo = result.Key.LotNo,
                                                                                    Operation = result.Key.Operation,
                                                                                    OpReference = result.Key.OpReference,
                                                                                    BackflushedQty = backflushInput.FGQualityType == CodeMaster.QualityType.Qualified ? result.Sum(p => p.Qty / backflushInput.UnitQty) : 0,   //根据收货成品的质量状态记录至不同的回冲数量中
                                                                                    BackflushedRejectQty = backflushInput.FGQualityType == CodeMaster.QualityType.Reject ? result.Sum(p => p.Qty / backflushInput.UnitQty) : 0,
                                                                                    //BackflushedScrapQty = input.BackflushedQty,
                                                                                    LocationFrom = result.Key.LocationFrom,
                                                                                    ProductLine = backflushInput.ProductLine,
                                                                                    ProductLineFacility = backflushInput.ProductLineFacility,
                                                                                    ReserveNo = result.Key.ReserveNo,
                                                                                    ReserveLine = result.Key.ReserveLine,
                                                                                    AUFNR = result.Key.AUFNR,
                                                                                    ICHARG = result.Key.ICHARG,
                                                                                    BWART = result.Key.BWART,
                                                                                    NotReport = result.Key.NotReport,   //过滤掉驾驶室和底盘总成
                                                                                    PlanBill = result.Key.PlanBill,
                                                                                    EffectiveDate = effectiveDate,
                                                                                    CreateUserId = currentUser.Id,
                                                                                    CreateUserName = currentUser.FullName,
                                                                                    CreateDate = dateTimeNow,
                                                                                    IsVoid = false,
                                                                                });
            }
            #endregion

            foreach (OrderBackflushDetail orderBackflushDetail in orderBackflushDetailList)
            {
                this.genericMgr.Create(orderBackflushDetail);
            }
            #endregion
        }

        private IList<Location> GetLocations(IList<string> locationCodeList)
        {
            string hql = string.Empty;
            IList<object> paras = new List<object>();
            foreach (string locationCode in locationCodeList)
            {
                if (hql == string.Empty)
                {
                    hql = "from Location where Code in (?";
                }
                else
                {
                    hql += ", ?";
                }
                paras.Add(locationCode);
            }
            hql += ")";
            return this.genericMgr.FindAll<Location>(hql, paras.ToArray());
        }
        #endregion

        #region 工序回冲物料
        [Transaction(TransactionMode.Requires)]
        public void BackflushProductOrder(OrderOperation orderOperation, OrderOperationReport orderOperationReport)
        {
            IList<OrderBomDetail> orderBomDetailList = this.genericMgr.FindEntityWithNativeSql<OrderBomDetail>(@"select * from ORD_OrderBomDet WITH(NOLOCK) where OrderNo = ? and Op = ? and OrderQty <> 0", new object[] { orderOperation.OrderNo, orderOperation.Operation });
            if (orderBomDetailList != null && orderBomDetailList.Count > 0)
            {
                FlowMaster prodLine = this.genericMgr.FindEntityWithNativeSql<FlowMaster>(@"select * from SCM_FlowMstr WITH(NOLOCK) where Code in (select Flow from ORD_OrderMstr_4 WITH(NOLOCK) where OrderNo = ?)", orderOperation.OrderNo).Single();
                string fgItem = this.genericMgr.FindAllWithNativeSql<string>(@"select Item from ORD_OrderDet_4 WITH(NOLOCK) where OrderNo = ?", orderOperation.OrderNo).Single();

                IList<BackflushInput> backflushInputList = (from bom in orderBomDetailList
                                                            select new BackflushInput
                                                            {
                                                                OrderNo = bom.OrderNo,
                                                                OrderDetailId = bom.OrderDetailId,
                                                                OrderDetailSequence = bom.OrderDetailSequence,
                                                                OrderBomDetail = bom,
                                                                OrderType = CodeMaster.OrderType.Production,
                                                                OrderSubType = CodeMaster.OrderSubType.Normal,
                                                                FGItem = fgItem,
                                                                Item = bom.Item,
                                                                ItemDescription = bom.ItemDescription,
                                                                ReferenceItemCode = bom.ReferenceItemCode,
                                                                Uom = bom.Uom,
                                                                BaseUom = bom.BaseUom,
                                                                UnitQty = bom.UnitQty,
                                                                Operation = bom.Operation,
                                                                OpReference = bom.OpReference,
                                                                Location = bom.Location,
                                                                ProductLine = prodLine.Code,
                                                                Qty = bom.BomUnitQty * (orderOperationReport.ReportQty + orderOperationReport.ScrapQty),
                                                                CurrentProductLine = prodLine,
                                                                EffectiveDate = orderOperationReport.EffectiveDate,
                                                                OrderOpReportId = orderOperationReport.Id,
                                                                OrderOpId = orderOperation.Id,
                                                                WorkCenter = orderOperation.WorkCenter,
                                                            }).ToList();

                locationDetailMgr.BackflushProductMaterial(backflushInputList);
            }
        }
        #endregion

        #region 工序物料反回冲
        [Transaction(TransactionMode.Requires)]
        public void AntiBackflushProductOrder(OrderOperation orderOperation, OrderOperationReport orderOperationReport)
        {
            IList<OrderBomDetail> orderBomDetailList = this.genericMgr.FindEntityWithNativeSql<OrderBomDetail>(@"select * from ORD_OrderBomDet where OrderNo = ? and Op = ? and OrderQty <> 0", new object[] { orderOperation.OrderNo, orderOperation.Operation });
            if (orderBomDetailList != null && orderBomDetailList.Count > 0)
            {
                FlowMaster prodLine = this.genericMgr.FindEntityWithNativeSql<FlowMaster>(@"select * from SCM_FlowMstr where Code in (select Flow from ORD_OrderMstr_4 where OrderNo = ?)", orderOperation.OrderNo).Single();
                string fgItem = this.genericMgr.FindAllWithNativeSql<string>(@"select Item from ORD_OrderDet_4 where OrderNo = ?", orderOperation.OrderNo).Single();

                IList<BackflushInput> backflushInputList = (from bom in orderBomDetailList
                                                            where bom.OrderedQty != 0
                                                            select new BackflushInput
                                                            {
                                                                OrderNo = bom.OrderNo,
                                                                OrderDetailId = bom.OrderDetailId,
                                                                OrderDetailSequence = bom.OrderDetailSequence,
                                                                OrderBomDetail = bom,
                                                                OrderType = CodeMaster.OrderType.Production,
                                                                OrderSubType = CodeMaster.OrderSubType.Normal,
                                                                FGItem = fgItem,
                                                                Item = bom.Item,
                                                                ItemDescription = bom.ItemDescription,
                                                                ReferenceItemCode = bom.ReferenceItemCode,
                                                                Uom = bom.Uom,
                                                                BaseUom = bom.BaseUom,
                                                                UnitQty = bom.UnitQty,
                                                                Operation = bom.Operation,
                                                                OpReference = bom.OpReference,
                                                                Location = bom.Location,
                                                                ProductLine = prodLine.Code,
                                                                Qty = bom.BomUnitQty * (orderOperationReport.ReportQty + orderOperationReport.ScrapQty),
                                                                CurrentProductLine = prodLine,
                                                                EffectiveDate = orderOperationReport.EffectiveDate,
                                                                OrderOpReportId = orderOperationReport.Id,
                                                                OrderOpId = orderOperation.Id,
                                                                WorkCenter = orderOperation.WorkCenter,
                                                            }).ToList();

                IList<InventoryTransaction> inventoryTransactionList = this.locationDetailMgr.CancelBackflushProductMaterial(backflushInputList);
            }
        }
        #endregion

        #region SAP工序回冲物料
        [Transaction(TransactionMode.Requires)]
        public void BackflushProductOrder(com.Sconit.Entity.SAP.ORD.ProdOpBackflush prodOpBackflush)
        {
            IList<OrderBomDetail> orderBomDetailList = this.genericMgr.FindEntityWithNativeSql<OrderBomDetail>(@"select * from ORD_OrderBomDet WITH(NOLOCK) where OrderNo = ? and AUFPL = ? and PLNFL = ? and VORNR = ? and OrderQty <> 0", new object[] { prodOpBackflush.OrderNo, prodOpBackflush.AUFPL, prodOpBackflush.PLNFL, prodOpBackflush.VORNR });
            if (orderBomDetailList != null && orderBomDetailList.Count > 0)
            {
                FlowMaster prodLine = this.genericMgr.FindEntityWithNativeSql<FlowMaster>(@"select * from SCM_FlowMstr WITH(NOLOCK) where Code in (select Flow from ORD_OrderMstr_4 WITH(NOLOCK) where OrderNo = ?)", prodOpBackflush.OrderNo).Single();
                string fgItem = this.genericMgr.FindAllWithNativeSql<string>(@"select Item from ORD_OrderDet_4 WITH(NOLOCK) where OrderNo = ?", prodOpBackflush.OrderNo).Single();

                IList<BackflushInput> backflushInputList = (from bom in orderBomDetailList
                                                            select new BackflushInput
                                                            {
                                                                OrderNo = bom.OrderNo,
                                                                OrderDetailId = bom.OrderDetailId,
                                                                OrderDetailSequence = bom.OrderDetailSequence,
                                                                OrderBomDetail = bom,
                                                                OrderType = CodeMaster.OrderType.Production,
                                                                OrderSubType = CodeMaster.OrderSubType.Normal,
                                                                FGItem = fgItem,
                                                                Item = bom.Item,
                                                                ItemDescription = bom.ItemDescription,
                                                                ReferenceItemCode = bom.ReferenceItemCode,
                                                                Uom = bom.Uom,
                                                                BaseUom = bom.BaseUom,
                                                                UnitQty = bom.UnitQty,
                                                                Operation = bom.Operation,
                                                                OpReference = bom.OpReference,
                                                                Location = bom.Location,
                                                                ProductLine = prodLine.Code,
                                                                Qty = bom.BomUnitQty * (prodOpBackflush.GAMNG + prodOpBackflush.SCRAP),
                                                                CurrentProductLine = prodLine,
                                                                EffectiveDate = prodOpBackflush.EffectiveDate,
                                                                OrderOpReportId = prodOpBackflush.OrderOpReportId,
                                                                OrderOpId = prodOpBackflush.OrderOpId,
                                                                WorkCenter = prodOpBackflush.WORKCENTER,
                                                            }).ToList();

                locationDetailMgr.BackflushProductMaterial(backflushInputList);
            }

            prodOpBackflush.Status = com.Sconit.Entity.SAP.StatusEnum.Success;
            this.genericMgr.Update(prodOpBackflush);
        }
        #endregion

        #region SAP工序物料反回冲
        [Transaction(TransactionMode.Requires)]
        public void AntiBackflushProductOrder(com.Sconit.Entity.SAP.ORD.ProdOpBackflush prodOpBackflush)
        {
            IList<OrderBomDetail> orderBomDetailList = this.genericMgr.FindEntityWithNativeSql<OrderBomDetail>(@"select * from ORD_OrderBomDet where OrderNo = ? and AUFPL = ? and PLNFL = ? and VORNR = ? and OrderQty <> 0", new object[] { prodOpBackflush.OrderNo, prodOpBackflush.AUFPL, prodOpBackflush.PLNFL, prodOpBackflush.VORNR });
            if (orderBomDetailList != null && orderBomDetailList.Count > 0)
            {
                FlowMaster prodLine = this.genericMgr.FindEntityWithNativeSql<FlowMaster>(@"select * from SCM_FlowMstr where Code in (select Flow from ORD_OrderMstr_4 where OrderNo = ?)", prodOpBackflush.OrderNo).Single();
                string fgItem = this.genericMgr.FindAllWithNativeSql<string>(@"select Item from ORD_OrderDet_4 where OrderNo = ?", prodOpBackflush.OrderNo).Single();

                IList<BackflushInput> backflushInputList = (from bom in orderBomDetailList
                                                            where bom.OrderedQty != 0
                                                            select new BackflushInput
                                                            {
                                                                OrderNo = bom.OrderNo,
                                                                OrderDetailId = bom.OrderDetailId,
                                                                OrderDetailSequence = bom.OrderDetailSequence,
                                                                OrderBomDetail = bom,
                                                                OrderType = CodeMaster.OrderType.Production,
                                                                OrderSubType = CodeMaster.OrderSubType.Normal,
                                                                FGItem = fgItem,
                                                                Item = bom.Item,
                                                                ItemDescription = bom.ItemDescription,
                                                                ReferenceItemCode = bom.ReferenceItemCode,
                                                                Uom = bom.Uom,
                                                                BaseUom = bom.BaseUom,
                                                                UnitQty = bom.UnitQty,
                                                                Operation = bom.Operation,
                                                                OpReference = bom.OpReference,
                                                                Location = bom.Location,
                                                                ProductLine = prodLine.Code,
                                                                Qty = bom.BomUnitQty * (prodOpBackflush.GAMNG + prodOpBackflush.SCRAP),
                                                                CurrentProductLine = prodLine,
                                                                EffectiveDate = prodOpBackflush.EffectiveDate,
                                                                OrderOpReportId = prodOpBackflush.OrderOpReportId,
                                                                OrderOpId = prodOpBackflush.OrderOpId,
                                                                WorkCenter = prodOpBackflush.WORKCENTER,
                                                            }).ToList();

                IList<InventoryTransaction> inventoryTransactionList = this.locationDetailMgr.CancelBackflushProductMaterial(backflushInputList);
            }
        }
        #endregion

        #region 生产线节拍调整
        public void AdjustProductLineTaktTime(string productLineCode, int taktTime)
        {
            try
            {
                User user = SecurityContextHolder.Get();
                this.genericMgr.UpdateWithNativeQuery("exec USP_Busi_AdjProdLineTaktTime ?,?,?,?", new object[] { productLineCode, taktTime, user.Id, user.FullName });
                this.AsyncUpdateOrderBomCPTime(productLineCode, user);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.InnerException != null)
                    {
                        throw new BusinessException(ex.InnerException.InnerException.Message);
                    }
                    else
                    {
                        throw new BusinessException(ex.InnerException.Message);
                    }
                }
                else
                {
                    throw new BusinessException(ex.Message);
                }
            }
        }
        #endregion

        #region 生产线暂停
        public void PauseProductLine(string productLineCode)
        {
            try
            {
                User user = SecurityContextHolder.Get();
                this.genericMgr.UpdateWithNativeQuery("exec USP_Busi_PauseProdLine ?,?,?", new object[] { productLineCode, user.Id, user.FullName });
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.InnerException != null)
                    {
                        throw new BusinessException(ex.InnerException.InnerException.Message);
                    }
                    else
                    {
                        throw new BusinessException(ex.InnerException.Message);
                    }
                }
                else
                {
                    throw new BusinessException(ex.Message);
                }
            }
        }
        #endregion

        #region 生产线恢复暂停
        public void ReStartProductLine(string productLineCode)
        {
            try
            {
                User user = SecurityContextHolder.Get();
                this.genericMgr.FindAllWithNamedQuery("exec USP_Busi_RestartProdLine ?,?,?", new object[] { productLineCode, user.Id, user.FullName });
                this.AsyncUpdateOrderBomCPTime(productLineCode, user);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.InnerException != null)
                    {
                        throw new BusinessException(ex.InnerException.InnerException.Message);
                    }
                    else
                    {
                        throw new BusinessException(ex.InnerException.Message);
                    }
                }
                else
                {
                    throw new BusinessException(ex.Message);
                }
            }
            //#region 获取所有活动生产单
            //IList<OrderMaster> productOrderList = this.genericMgr.FindAll<OrderMaster>("from OrderMaster where Flow = ? and Status in (?, ?, ?)",
            //    new object[] { productLineCode, CodeMaster.OrderStatus.Create, CodeMaster.OrderStatus.Submit, CodeMaster.OrderStatus.InProcess });
            //#endregion

            //#region 更新生产线
            //productLine.IsPause = false;
            //productLine.PauseTime = null;
            //this.genericMgr.Update(productLine);
            //#endregion

            //#region 更新生产单排序单和Kit单
            //if (productOrderList != null && productOrderList.Count > 0)
            //{
            //    #region 获取所有活动排序单和Kit单
            //    string updateSeqAndKitOrderStatement = string.Empty;
            //    IList<object> updateSeqAndKitOrderParas = new List<object>();
            //    foreach (OrderMaster productOrder in productOrderList)
            //    {
            //        if (updateSeqAndKitOrderStatement == string.Empty)
            //        {
            //            updateSeqAndKitOrderStatement = "update from OrderMaster set IsProductLinePause = ?  and PauseTime = ? where Status in (?, ?, ?) and TraceCode in (?";
            //            updateSeqAndKitOrderParas.Add(false);
            //            updateSeqAndKitOrderParas.Add(null);
            //            updateSeqAndKitOrderParas.Add(CodeMaster.OrderStatus.Create);
            //            updateSeqAndKitOrderParas.Add(CodeMaster.OrderStatus.Submit);
            //            updateSeqAndKitOrderParas.Add(CodeMaster.OrderStatus.InProcess);
            //        }
            //        else
            //        {
            //            updateSeqAndKitOrderStatement += ", ?";
            //        }
            //        updateSeqAndKitOrderParas.Add(productOrder.TraceCode);
            //    }

            //    this.genericMgr.Update(updateSeqAndKitOrderStatement, updateSeqAndKitOrderParas.ToArray());
            //    #endregion
            //}
            //#endregion
        }
        #endregion

        #region 记录物料追溯表
        //[Transaction(TransactionMode.Requires)]
        //public void MaterialTracer(string orderNo, IList<MaterialTracer> materialTracertList, bool isForceFeed)
        //{
        //}
        #endregion

        #region 整车物料回冲
        [Transaction(TransactionMode.Requires)]
        public void BackFlushVanOrder()
        {
            log.Debug("整车物料回冲开始。");

            User user = SecurityContextHolder.Get();
            DateTime dateTimeNow = DateTime.Now;

            StringBuilder sql = new StringBuilder();
            log.DebugFormat("开始更新生产单状态和工序回冲数量。");

            DataSet dataSet = this.sqlDao.GetDatasetBySql(@"select OrderNo into #tempOrderNo from ORD_OrderMstr_4 WITH(NOLOCK) where ProdLineType in (1,2,3,4,9) and Status = 3
                                                            select det.Item as FGItem,
                                                                bom.Item, bom.ItemDesc, bom.Uom, bom.Location,
                                                                mstr.Flow as ProdLine,
                                                                sum(bom.OrderQty) as OrderQty--, mstr.CompleteDate
                                                                from ORD_OrderBomDet as bom WITH(NOLOCK)    
                                                                inner join ORD_OrderDet_4 as det WITH(NOLOCK) on bom.OrderDetId = det.Id
                                                                inner join ORD_OrderMstr_4 as mstr WITH(NOLOCK) on bom.OrderNo = mstr.OrderNo 
                                                                inner join #tempOrderNo as t on mstr.OrderNo = t.OrderNo
                                                                group by det.Item, bom.Item, bom.ItemDesc, bom.Uom, bom.Location,
                                                                mstr.Flow, mstr.Type--, mstr.CompleteDate
                                                            select OrderNo from #tempOrderNo", null);

            DataRowCollection orderNoDataRow = dataSet.Tables[1].Rows;
            IList<string> orderNoList = new List<string>();
            foreach (DataRow dr in orderNoDataRow)
            {
                sql.Append("update ORD_OrderMstr_4 set Status = 4, LastModifyDate='" + dateTimeNow.ToString("yyyy-MM-dd HH:ss:mm") + "',LastModifyUser = " + user.Id.ToString() + ",LastModifyUserNm = '" + user.FullName + "',CloseDate='" + dateTimeNow.ToString("yyyy-MM-dd HH:ss:mm") + "',CloseUser = " + user.Id.ToString() + ",CloseUserNm = '" + user.FullName + "',Version=Version + 1 where OrderNo = '" + (string)dr[0] + "';");
                sql.Append("update ORD_OrderOp set LastModifyDate='" + dateTimeNow.ToString("yyyy-MM-dd HH:ss:mm") + "',LastModifyUser = " + user.Id.ToString() + ",LastModifyUserNm = '" + user.FullName + "',BackflushQty = ReportQty,Version=Version + 1 where OrderNo = '" + (string)dr[0] + "';");
                sql.Append("update ORD_OrderOpReport set LastModifyDate='" + dateTimeNow.ToString("yyyy-MM-dd HH:ss:mm") + "',LastModifyUser = " + user.Id.ToString() + ",LastModifyUserNm = '" + user.FullName + "',BackflushQty = ReportQty,Version=Version + 1 where OrderNo = '" + (string)dr[0] + "';");
            }

            this.genericMgr.UpdateWithNativeQuery(sql.ToString());
            log.DebugFormat("更新生产单状态和工序回冲数量完成。");

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables[0].Rows.Count > 0)
            {
                IList<FlowMaster> prodLineList = this.genericMgr.FindAll<FlowMaster>("from FlowMaster where ProdLineType in (?,?,?,?,?)",
                    new object[] { CodeMaster.ProdLineType.Cab, CodeMaster.ProdLineType.Chassis, CodeMaster.ProdLineType.Assembly, CodeMaster.ProdLineType.Special, CodeMaster.ProdLineType.Check });

                DataRowCollection orderBomDataRow = dataSet.Tables[0].Rows;
                IList<BackflushInput> backflushInputList = new List<BackflushInput>();
                foreach (DataRow dr in orderBomDataRow)
                {
                    BackflushInput backflushInput = new BackflushInput();
                    backflushInput.OrderType = CodeMaster.OrderType.Production;
                    backflushInput.OrderSubType = CodeMaster.OrderSubType.Normal;
                    backflushInput.FGItem = (string)dr[0];
                    backflushInput.Item = (string)dr[1];
                    backflushInput.ItemDescription = (string)dr[2];
                    backflushInput.Uom = (string)dr[3];
                    backflushInput.BaseUom = (string)dr[3];
                    backflushInput.UnitQty = 1;
                    backflushInput.Location = (string)dr[4];
                    backflushInput.ProductLine = (string)dr[5];
                    backflushInput.Qty = (decimal)dr[6];
                    backflushInput.CurrentProductLine = prodLineList.Where(pl => pl.Code == (string)dr[5]).Single();
                    backflushInput.EffectiveDate = dateTimeNow;

                    backflushInputList.Add(backflushInput);
                }

                log.DebugFormat("整车生产单待回冲物料汇总条数{0}，开始回冲物料。", backflushInputList.Count());
                locationDetailMgr.BackflushVanProductMaterial(backflushInputList);
                log.DebugFormat("整车生产单物料回冲完成。");

              
            }

            log.Debug("整车物料回冲结束。");
        }

        public void BackFlushVanOrderByOp()
        {
            log.Debug("整车物料回冲开始。");
            DataSet dataSet = this.sqlDao.GetDatasetBySql(@"select mstr.OrderNo, op.Op, det.Item into #tempOrderOp
                                                            from ORD_OrderOp as op WITH(NOLOCK)
                                                            inner join ORD_OrderMstr_4 as mstr WITH(NOLOCK) on op.OrderNo = mstr.OrderNo
                                                            inner join ORD_OrderDet_4 as det WITH(NOLOCK) on mstr.OrderNo = det.OrderNo
                                                            where op.ReportQty = 1 and op.BackflushQty = 0 and mstr.ProdLineType in (1, 2, 3, 4, 9) 
                                                            group by mstr.OrderNo, op.Op, det.Item
                                                            select det.Item as FGItem,
                                                            bom.Item, bom.ItemDesc, bom.Uom, bom.Location,
                                                            mstr.Flow as ProdLine,
                                                            sum(bom.OrderQty) as OrderQty
                                                            from ORD_OrderBomDet as bom WITH(NOLOCK)
                                                            inner join #tempOrderOp as op on bom.OrderNo = op.OrderNo and bom.Op = op.Op
                                                            inner join ORD_OrderMstr_4 as mstr WITH(NOLOCK) on bom.OrderNo = mstr.OrderNo
                                                            inner join ORD_OrderDet_4 as det WITH(NOLOCK) on mstr.OrderNo = det.OrderNo
                                                            group by det.Item,
                                                            bom.Item, bom.ItemDesc, bom.Uom, bom.Location,
                                                            mstr.Flow
                                                            select OrderNo, Op, Item from #tempOrderOp
                                                            ", null);

            if (dataSet != null && dataSet.Tables != null)
            {
                DateTime dateTimeNow = DateTime.Now;
                IList<FlowMaster> prodLineList = this.genericMgr.FindAll<FlowMaster>("from FlowMaster where ProdLineType in (?,?,?,?,?)",
                    new object[] { CodeMaster.ProdLineType.Cab, CodeMaster.ProdLineType.Chassis, CodeMaster.ProdLineType.Assembly, CodeMaster.ProdLineType.Special, CodeMaster.ProdLineType.Check });

                DataRowCollection orderBomDataRow = dataSet.Tables[0].Rows;
                IList<BackflushInput> backflushInputList = new List<BackflushInput>();
                foreach (DataRow dr in orderBomDataRow)
                {
                    BackflushInput backflushInput = new BackflushInput();
                    backflushInput.OrderType = CodeMaster.OrderType.Production;
                    backflushInput.OrderSubType = CodeMaster.OrderSubType.Normal;
                    backflushInput.FGItem = (string)dr[0];
                    backflushInput.Item = (string)dr[1];
                    backflushInput.ItemDescription = (string)dr[2];
                    backflushInput.Uom = (string)dr[3];
                    backflushInput.BaseUom = (string)dr[3];
                    backflushInput.UnitQty = 1;
                    backflushInput.Location = (string)dr[4];
                    backflushInput.ProductLine = (string)dr[5];
                    backflushInput.Qty = (decimal)dr[6];
                    backflushInput.CurrentProductLine = prodLineList.Where(pl => pl.Code == (string)dr[5]).Single();
                    backflushInput.EffectiveDate = dateTimeNow;

                    backflushInputList.Add(backflushInput);
                }

                DataRowCollection orderOpDataRow = dataSet.Tables[1].Rows;
                IList<object[]> orderOpList = new List<object[]>();
                foreach (DataRow dr in orderOpDataRow)
                {
                    object[] orderOp = new object[3];
                    orderOp[0] = dr[0];
                    orderOp[1] = dr[1];
                    orderOp[2] = dr[2];

                    orderOpList.Add(orderOp);
                }

                IList<string> fgItemList = (from op in orderOpList
                                            group op by (string)op[2] into result
                                            select result.Key).ToList();

                foreach (string fgItem in fgItemList)
                {
                    try
                    {
                        backflushVanOrderMgr.BackflushOp(orderOpList.Where(op => (string)op[2] == fgItem).ToList(), backflushInputList.Where(bom => bom.FGItem == fgItem).ToList());
                    }
                    catch (Exception)
                    {
                    }

                    //睡30秒
                    Thread.Sleep(30000);
                }
            }
            log.Debug("整车物料回冲结束。");
        }
        #endregion

        #region 更新物料消耗时间
        public delegate void AsyncUpdateOrderBomConsumeTime(string prodLine, User user);

        public void AsyncUpdateOrderBomCPTime(string prodLine, User user)
        {
            AsyncUpdateOrderBomConsumeTime asayncUpdateOrderBomConsumeTime = new AsyncUpdateOrderBomConsumeTime(this.UpdateOrderBomConsumeTime);
            asayncUpdateOrderBomConsumeTime.BeginInvoke(prodLine, user, null, null);
        }

        public void UpdateOrderBomConsumeTime(string prodLine, User user)
        {
            this.genericMgr.UpdateWithNativeQuery("exec USP_Busi_UpdateOrderBomConsumeTime ?,?,?", new object[] { prodLine, user.Id, user.FullName });
        }
        #endregion

        #region 工位映射导入
        [Transaction(TransactionMode.Requires)]
        public void ImportOpRefMap(Stream inputStream)
        {
            if (inputStream.Length == 0)
            {
                throw new BusinessException("Import.Stream.Empty");
            }

            HSSFWorkbook workbook = new HSSFWorkbook(inputStream);
            ISheet sheet = workbook.GetSheetAt(0);
            IEnumerator rows = sheet.GetRowEnumerator();
            ImportHelper.JumpRows(rows, 10);
            BusinessException businessException = new BusinessException();
            #region 列定义
            int colSAPProdLine = 1;//SAP生产线
            int colProdLine = 2;//生产线
            int colItem = 3;//物料
            int colOpReference = 4;//工位
            int colRefOpReference = 5;//工位
            int colIsPrimary = 6;//是否主键
            #endregion
            IList<OpRefMap> exactOpRefMapList = new List<OpRefMap>();
            //IList<Item> allItemList = this.genericMgr.FindAll<Item>();
            IList<FlowMaster> allProdLineList = this.genericMgr.FindAll<FlowMaster>(" select fm from FlowMaster as fm where fm.Type=? ",com.Sconit.CodeMaster.OrderType.Production);
            IList<OpRefMap> allOpRefMap = this.genericMgr.FindAll<OpRefMap>();
            int i = 10;
            while (rows.MoveNext())
            {
                i++;
                HSSFRow row = (HSSFRow)rows.Current;
                if (!ImportHelper.CheckValidDataRow(row, 1, 5))
                {
                    break;//边界
                }
                string sAPProdLine = string.Empty;
                string prodLine = string.Empty;
                string itemCode = string.Empty;
                string opReference = string.Empty;
                string refOpReference = string.Empty;
                string isPrimary = string.Empty;
                OpRefMap opRefMap = new OpRefMap();
                #region 读取数据
                #region sAPProdLine
                sAPProdLine = ImportHelper.GetCellStringValue(row.GetCell(colSAPProdLine));
                if (string.IsNullOrWhiteSpace(sAPProdLine))
                {
                    businessException.AddMessage(string.Format("第{0}行SAP生产线不能为空", i));
                }
                else
                {
                    opRefMap.SAPProdLine = sAPProdLine;
                }
                #endregion

                #region prodLine
                prodLine = ImportHelper.GetCellStringValue(row.GetCell(colProdLine));
                if (string.IsNullOrWhiteSpace(prodLine))
                {
                    businessException.AddMessage(string.Format("第{0}行生产线不能为空", i));
                }
                else
                {
                    var prodLines = allProdLineList.Where(a => a.Code == prodLine);
                    //var duplicateItemTrace=
                    if (prodLines == null)
                    {
                        businessException.AddMessage(string.Format("第{0}行生产线{1}不存在。", i, prodLine));
                    }
                    else
                    {
                        opRefMap.ProdLine = prodLine;
                    }
                }
                #endregion

                #region Item
                itemCode = ImportHelper.GetCellStringValue(row.GetCell(colItem));
                if (string.IsNullOrWhiteSpace(itemCode))
                {
                    businessException.AddMessage(string.Format("第{0}行物料编号不能为空", i));
                }
                else
                {
                    var items = this.genericMgr.FindAll<Item>("select i from Item as i where i.Code=? ",itemCode);
                    //var duplicateItemTrace=
                    if (items == null)
                    {
                        businessException.AddMessage(string.Format("第{0}行{1}物料编号不存在。", i, itemCode));
                    }
                    else
                    {
                        opRefMap.Item = items.First().Code;
                        opRefMap.ItemDesc = items.First().Description;
                        opRefMap.ItemRefCode = items.First().ReferenceCode;
                    }
                }
                #endregion

                #region opReference
                opReference = ImportHelper.GetCellStringValue(row.GetCell(colOpReference));
                if (string.IsNullOrWhiteSpace(opReference))
                {
                    businessException.AddMessage(string.Format("第{0}行工位不能为空", i));
                }
                else
                {

                    opRefMap.OpReference = opReference;
                }
                #endregion

                #region refOpReference
                refOpReference = ImportHelper.GetCellStringValue(row.GetCell(colRefOpReference));
                if (string.IsNullOrWhiteSpace(refOpReference))
                {
                    businessException.AddMessage(string.Format("第{0}行引用工位不能为空", i));
                }
                else
                {

                    opRefMap.RefOpReference = refOpReference;
                }
                #endregion

                #region isPrimary
                isPrimary = ImportHelper.GetCellStringValue(row.GetCell(colIsPrimary));
                if (string.IsNullOrWhiteSpace(isPrimary))
                {
                    businessException.AddMessage(string.Format("第{0}行是否主键不能为空", i));
                }
                else
                {
                    switch (isPrimary)
                    {
                        case "1":
                            opRefMap.IsPrimary = true;
                            break;
                        case "0":
                            opRefMap.IsPrimary = false;
                            break;
                        default:
                            businessException.AddMessage(string.Format("第{0}行是否主键{1}填写有误", i,isPrimary));
                            break;
                    }
                }

                #endregion

                if (exactOpRefMapList != null && exactOpRefMapList.Count > 0)
                {
                    if (exactOpRefMapList.Where(a => a.Item == opRefMap.Item && a.ProdLine == opRefMap.ProdLine).Count() > 0)
                    {
                        businessException.AddMessage(string.Format("第{0}行【物料编号+生产线】在模板中重复", i));
                    }
                }
                
                if (allOpRefMap != null && allOpRefMap.Count > 0)
                {
                    var updateOprefMaps = allOpRefMap.Where(a => a.Item == opRefMap.Item && a.ProdLine == opRefMap.ProdLine);
                    if (updateOprefMaps != null && updateOprefMaps.Count() > 0)
                    {
                        var updateOprefMap = updateOprefMaps.First();
                        updateOprefMap.ProdLine = opRefMap.ProdLine;
                        updateOprefMap.SAPProdLine = opRefMap.SAPProdLine;
                        updateOprefMap.Item = opRefMap.Item;
                        updateOprefMap.ItemDesc = opRefMap.ItemDesc;
                        updateOprefMap.ItemRefCode = opRefMap.ItemRefCode;
                        updateOprefMap.OpReference = opRefMap.OpReference;
                        updateOprefMap.RefOpReference = opRefMap.RefOpReference;
                        updateOprefMap.IsPrimary = opRefMap.IsPrimary;
                        updateOprefMap.IsUpdate = opRefMap.IsUpdate;
                        exactOpRefMapList.Add(updateOprefMap);
                    }
                    else
                    {
                        exactOpRefMapList.Add(opRefMap);
                    }
                }
                else
                {
                    exactOpRefMapList.Add(opRefMap);
                }
                

                #endregion
            }
            if (businessException.HasMessage)
            {
                throw businessException;
            }
            if (exactOpRefMapList == null || exactOpRefMapList.Count == 0)
            {
                throw new BusinessException("模版为空，请确认。");
            }
            foreach (OpRefMap oprefMap in exactOpRefMapList)
            {
                if (oprefMap.IsUpdate)
                {
                    genericMgr.Update(oprefMap);
                }
                else
                {
                    genericMgr.Create(oprefMap);
                }
            }
        }
        #endregion
    }

    [Transactional]
    public class BackflushVanOrderMgrImpl : BaseMgr, IBackflushVanOrderMgr
    {
        public IGenericMgr genericMgr { get; set; }
        public ILocationDetailMgr locationDetailMgr { get; set; }

        private static log4net.ILog log = log4net.LogManager.GetLogger("Log.ProductionLine");

        [Transaction(TransactionMode.Requires)]
        public void BackflushOp(IList<object[]> orderOpList, IList<BackflushInput> backflushInputList)
        {
            try
            {
                if (backflushInputList != null && backflushInputList.Count > 0)
                {
                    log.DebugFormat("整车生产单待回冲物料汇总条数{0}，开始回冲物料。", backflushInputList.Count());
                    locationDetailMgr.BackflushVanProductMaterial(backflushInputList);
                    log.DebugFormat("整车生产单物料回冲完成。");
                }

                User user = SecurityContextHolder.Get();
                StringBuilder updateOrderOp = new StringBuilder("create table #tempOrderOp(OrderNo varchar(50), Op int)");
                int recordAccumulateCount = 0;
                foreach (object[] orderOp in orderOpList)
                {
                    string orderNo = (string)orderOp[0];
                    int op = (int)orderOp[1];

                    if (recordAccumulateCount == 2000)
                    {
                        recordAccumulateCount = 0;
                    }

                    if (recordAccumulateCount == 0)
                    {
                        updateOrderOp.Append("insert into #tempOrderOp(OrderNo, Op) values('" + orderNo + "', " + op + ")");
                    }
                    else
                    {
                        updateOrderOp.Append(",('" + orderNo + "', " + op + ")");
                    }

                    log.DebugFormat("整车生产单{0}工序{1}的物料反冲完成。", orderNo, op);
                }
                updateOrderOp.Append(@"update ORD_OrderOp set LastModifyDate = GETDATE(),LastModifyUser = ?,LastModifyUserNm = ?, BackflushQty = 1,Version=Version + 1 
		                                from ORD_OrderOp as op inner join #tempOrderOp as t on op.OrderNo = t.OrderNo and op.Op = t.Op");
                genericMgr.UpdateWithNativeQuery(updateOrderOp.ToString(), new object[] { user.Id, user.FullName });

                IList<string> orderNoList = orderOpList.Select(op => (string)op[0]).Distinct().ToList();
                foreach (string orderNo in orderNoList)
                {
                    int count = genericMgr.FindAllWithNativeSql<int>("select count(1) from ORD_OrderOp where OrderNo = ? and BackflushQty = 0", orderNo).Single();
                    if (count == 0)
                    {
                        genericMgr.UpdateWithNativeQuery(@"update ORD_OrderMstr_4 set Status = ?, LastModifyDate=GETDATE(),LastModifyUser = ?,
                                            LastModifyUserNm = ?,CloseDate=GETDATE(),CloseUser = ?,CloseUserNm = ?,Version=Version + 1 where OrderNo = ?",
                                                new object[] { CodeMaster.OrderStatus.Close, user.Id, user.FullName, user.Id, user.FullName, orderNo }); ;

                        log.DebugFormat("整车生产单{0}关闭。", orderNo);
                    }
                }

                this.genericMgr.FlushSession();
            }
            catch (Exception ex)
            {
                log.Error("整车生产单物料回冲出现异常。", ex);
                this.genericMgr.CleanSession();
                throw ex;
            }
        }
    }
}
