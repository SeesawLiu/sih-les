using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Castle.Services.Transaction;
using com.Sconit.Entity.CUST;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.SCM;
using com.Sconit.Service.SAP.MI_SL_OUT;
using com.Sconit.Utility;
using System.Threading;


namespace com.Sconit.Service.SAP.Impl
{
    [Transactional]
    public class ProcurementMgrImpl : BaseMgr, IProcurementMgr
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("SAP_Procurement");
        //private static log4net.ILog log = log4net.LogManager.GetLogger("SAP_Distribution");
        public IOrderMgr orderMgr { get; set; }
        public IItemMgr itemMgr { get; set; }

        public IProcurementOperatorMgr procurementOperatorMgrImpl { get; set; }

        private static object GetProcOrdersLock = new object();
        public List<com.Sconit.Entity.SAP.ORD.OrderDetail> GetProcOrders(string flow, string supplier, string item, string plant)
        {
            if (string.IsNullOrWhiteSpace(supplier))
            {
                if (!string.IsNullOrWhiteSpace(flow))
                {
                    supplier = this.genericMgr.FindAllWithNativeSql<string>("select PartyFrom from SCM_FlowMstr where Code = ? and [Type] = ?",
                        new object[] { flow, CodeMaster.OrderType.Procurement }).SingleOrDefault();

                    if (string.IsNullOrWhiteSpace(supplier))
                    {
                        throw new BusinessException("采购路线{0}不存在。", flow);
                    }
                }
                else if (string.IsNullOrWhiteSpace(item))
                {
                    throw new TechnicalException("采购路线、供应商和物料代码不能全部为空。");
                }
            }

            ZSEKPTH[] headOut = null;
            ZSEKPTI[] detailOut = null;
            lock (GetProcOrdersLock)
            {
                try
                {
                    #region 获取SAP计划协议
                    log.Debug("开始连接Web服务获取计划协议，工厂" + plant + (string.IsNullOrWhiteSpace(supplier) ? ", 供应商: " + supplier : "") + (string.IsNullOrWhiteSpace(item) ? ", 物料号: " + item : ""));
                    MI_SL_OUTService slOutService = new MI_SL_OUTService();
                    slOutService.Credentials = base.Credentials;
                    slOutService.Timeout = base.TimeOut;
                    slOutService.Url = ReplaceSAPServiceUrl(slOutService.Url);

                    ZSDELIVERY deliveryIn = new ZSDELIVERY();
                    deliveryIn.LIFNR = supplier;
                    deliveryIn.WERKS = plant;
                    deliveryIn.MATNR = item;
                    headOut = slOutService.MI_SL_OUT(deliveryIn, out detailOut);
                    log.Debug("连接Web服务获取计划协议完成，工厂" + plant + (string.IsNullOrWhiteSpace(supplier) ? ", 供应商: " + supplier : "") + (string.IsNullOrWhiteSpace(item) ? ", 物料号: " + item : ""));
                    #endregion
                }
                catch (Exception ex)
                {
                    log.Error("服务获取计划协议完成出现异常，工厂" + plant + (string.IsNullOrWhiteSpace(supplier) ? ", 供应商: " + supplier : "") + (string.IsNullOrWhiteSpace(item) ? ", 物料号: " + item : "") + "，异常信息：" + ex.Message, ex);
                    throw new BusinessException("从SAP获取计划协议出现异常，异常信息：{0}", ex.Message);
                }
            }

            return ProcessPoOut(flow, supplier, item, plant, headOut, detailOut);
        }

        //根据sap计划协议订单头显示路线明细缺失信息（包括明细不存在和明细无效）
        public List<object[]> GetScheduleLineItem(string item, string supplier, string plant)
        {
            IList<ErrorMessage> errorMessageList = new List<ErrorMessage>();
            ZSEKPTH[] headOut = null;
            ZSEKPTI[] detailOut = null;
            lock (GetProcOrdersLock)
            {
                try
                {
                    #region 获取指定日期的采购订单物料和计划协议
                    log.Debug("开始连接Web服务获取采购订单物料和计划协议, " + "物料号: " + item + "，供应商：" + supplier + ", 工厂" + plant);
                    MI_SL_OUTService slOutService = new MI_SL_OUTService();
                    slOutService.Credentials = base.Credentials;
                    slOutService.Timeout = base.TimeOut;
                    slOutService.Url = ReplaceSAPServiceUrl(slOutService.Url);

                    ZSDELIVERY deliveryIn = new ZSDELIVERY();
                    deliveryIn.LIFNR = supplier;
                    deliveryIn.WERKS = plant;
                    deliveryIn.MATNR = item;
                    headOut = slOutService.MI_SL_OUT(deliveryIn, out detailOut);
                    log.Debug("从Web服务获取采购订单物料和计划协议完成。");
                    #endregion
                }
                catch (Exception ex)
                {
                    string logMessage = "连接Web服务获取采购订单物料和计划协议失败。";
                    log.Error(logMessage, ex);
                    errorMessageList.Add(new ErrorMessage
                    {
                        Template = NVelocityTemplateRepository.TemplateEnum.ImportProcOrderFail,
                        Exception = ex,
                        Message = logMessage
                    });
                }
            }
            this.SendErrorMessage(errorMessageList);

            //过滤供应商+物料重复的计划协议
            var distinctSaList = from d in headOut
                                 group d by new
                                 {
                                     PartyFrom = d.LIFNR != null ? d.LIFNR : d.RESWK,
                                     Item = d.MATNR,
                                     EBELN = d.EBELN,//新增了计划协议号 行号
                                     EBELP = d.EBELP
                                 } into result
                                 select new
                                 {
                                     PartyFrom = result.Key.PartyFrom,
                                     Item = result.Key.Item,
                                     EBELN = result.Key.EBELN,
                                     EBELP = result.Key.EBELP,
                                     List = result.First()
                                 };
            //查找所有有效的采购路线明细
            IList<object[]> flowDetailList = this.genericMgr.FindAllWithNativeSql<object[]>("select m.partyfrom,d.item from scm_flowdet d left join scm_flowmstr m on d.flow = m.code where d.isactive = 1 and m.isactive = 1 and m.type = 1");

            //查找物料缺失表
            IList<ScheduleLineItem> scheduleLineItemList = this.genericMgr.FindAll<ScheduleLineItem>("select s from ScheduleLineItem as s");

            List<object[]> missedFlowDetail = new List<object[]>();
            if (distinctSaList != null && distinctSaList.Count() > 0)
            {
                foreach (var distinctSa in distinctSaList)
                {
                    var matchedFlowDetail = from f in flowDetailList
                                            where f[0].ToString() == distinctSa.PartyFrom && f[1].ToString() == distinctSa.Item
                                            select f;

                    //没找到有效的路线明细
                    if (matchedFlowDetail == null || matchedFlowDetail.Count() == 0)
                    {

                        missedFlowDetail.Add(new object[] { distinctSa.PartyFrom, distinctSa.Item, distinctSa.EBELN, distinctSa.EBELP });
                        ////缺失表中不存在，则新增
                        //if (matchedScheduleLineItem == null || matchedScheduleLineItem.Count() == 0)
                        //{
                        //    ScheduleLineItem scheduleLineItem = new ScheduleLineItem();
                        //    scheduleLineItem.Supplier = distinctSa.PartyFrom;
                        //    scheduleLineItem.Item = distinctSa.Item;
                        //    scheduleLineItem.IsClose = false;
                        //    lesMgr.Create(scheduleLineItem);
                        //}
                        ////如果缺失表已存在，但状态为关闭则重新打开
                        //else
                        //{
                        //    ScheduleLineItem scheduleLineItem = scheduleLineItemList[0];
                        //    if (scheduleLineItem.IsClose)
                        //    {
                        //        scheduleLineItem.IsClose = false;
                        //        lesMgr.Update(scheduleLineItem);
                        //    }
                        //}
                    }
                    //找到路线明细
                    //else
                    //{
                    //    //如果缺失表有记录，且状态不是关闭，则关闭
                    //    if (scheduleLineItemList != null && scheduleLineItemList.Count > 0)
                    //    {
                    //        if (!scheduleLineItemList[0].IsClose)
                    //        {
                    //            scheduleLineItemList[0].IsClose = true;
                    //            lesMgr.Update(scheduleLineItemList[0]);
                    //        }
                    //    }
                    //}

                }
            }
            return missedFlowDetail;
        }

        private List<com.Sconit.Entity.SAP.ORD.OrderDetail> ProcessPoOut(string flow, string supplier, string item, string plant, ZSEKPTH[] headOut, ZSEKPTI[] detailOut)
        {
            //LES创建订单，每一个sa对应LES中的一张订单，如果订单已存在则不用处理（订单上会记录sa的EBELN和EBELP字段值，可据此判断唯一性）；然后将sa下对应的sal计划数量汇总后（sal汇总后只会有一行记录），插入订单明细表中，如果明细已存在则不作处理，默认订单数和需求数都为1，写入订单及订单明细其他信息的逻辑保持不变（比如入库库位，单包装等都根据路线头或路线明细的对应信息）
            IList<Entity.SAP.ORD.ProcOrder> insertedProcOrderList = new List<Entity.SAP.ORD.ProcOrder>();
            #region 头数据转换
            if (headOut != null && headOut.Length > 0)
            {
                foreach (var head in headOut)
                {
                    Entity.SAP.ORD.ProcOrder procOrder = Mapper.Map<ZSEKPTH, Entity.SAP.ORD.ProcOrder>(head);
                    procOrder.ProcOrderDetails = new List<Entity.SAP.ORD.ProcOrderDetail>();
                    insertedProcOrderList.Add(procOrder);
                }
            }
            else
            {
                throw new BusinessException((string.IsNullOrWhiteSpace(flow) ? string.Format("采购路线{0}", flow) : "") + (string.IsNullOrWhiteSpace(supplier) ? string.Format("供应商{0}", supplier) : "") + (string.IsNullOrWhiteSpace(item) ? string.Format("物料代码{0}", item) : "") + "在工厂{0}的计划协议不存在。", plant);
            }
            #endregion

            #region 明细数据转换
            if (detailOut != null && detailOut.Length > 0)
            {
                IList<Entity.SAP.ORD.ProcOrderDetail> procOrderDetailList = new List<Entity.SAP.ORD.ProcOrderDetail>();
                foreach (var detail in detailOut)
                {
                    Entity.SAP.ORD.ProcOrderDetail procOrderDetail = Mapper.Map<ZSEKPTI, Entity.SAP.ORD.ProcOrderDetail>(detail);
                    procOrderDetailList.Add(procOrderDetail);
                    //明细必然找到一个计划协议头
                    insertedProcOrderList.Where(o => o.EBELN == procOrderDetail.EBELN && o.EBELP == procOrderDetail.EBELP).Single().ProcOrderDetails.Add(procOrderDetail);
                }
            }
            else
            {
                throw new BusinessException((string.IsNullOrWhiteSpace(flow) ? string.Format("采购路线{0}", flow) : "") + (string.IsNullOrWhiteSpace(supplier) ? string.Format("供应商{0}", supplier) : "") + (string.IsNullOrWhiteSpace(item) ? string.Format("物料代码{0}", item) : "") + "在工厂{0}的计划协议明细行不存在。", plant);
            }
            #endregion

            #region 生成LES订单
            List<OrderDetail> returnOrderDetailList = new List<OrderDetail>();

            #region 查找符合条件的采购路线头
            StringBuilder selectFlowMstrSql = new StringBuilder("select * from SCM_FlowMstr where 1 = 1");
            IList<object> selectFlowMstrParam = new List<object>();

            if (!string.IsNullOrWhiteSpace(flow))
            {
                selectFlowMstrSql.Append(" and Code = ?");
                selectFlowMstrParam.Add(flow);
            }
            else if (!string.IsNullOrWhiteSpace(supplier))
            {
                selectFlowMstrSql.Append(" and PartyFrom = ?");
                selectFlowMstrParam.Add(supplier);
            }

            selectFlowMstrSql.Append(" and [Type] = ?");
            selectFlowMstrParam.Add(CodeMaster.OrderType.Procurement);

            IList<FlowMaster> flowMasterList = this.genericMgr.FindEntityWithNativeSql<FlowMaster>(selectFlowMstrSql.ToString(), selectFlowMstrParam.ToArray());
            #endregion

            #region 查找符合条件的采购路线明细
            StringBuilder selectFlowDetSql = new StringBuilder(@"select m.Code,m.partyfrom,d.item,d.UC,d.Uom,d.BaseUom,d.RefItemCode,i.Desc1 from SCM_FlowMstr as m 
                                                                inner join SCM_FlowDet as d on d.flow = m.code 
                                                                inner join MD_Item as i on d.Item = i.Code
                                                                where 1 = 1");
            IList<object> selectFlowDetParam = new List<object>();

            if (!string.IsNullOrWhiteSpace(flow))
            {
                selectFlowDetSql.Append(" and m.Code = ?");
                selectFlowDetParam.Add(flow);
            }
            else if (!string.IsNullOrWhiteSpace(supplier))
            {
                selectFlowDetSql.Append(" and m.PartyFrom = ?");
                selectFlowDetParam.Add(supplier);
            }

            if (!string.IsNullOrWhiteSpace(item))
            {
                selectFlowDetSql.Append(" and d.Item = ?");
                selectFlowDetParam.Add(item);
            }

            selectFlowDetSql.Append(" and m.type = ? and d.IsActive = ? and m.IsActive = ?");
            selectFlowDetParam.Add(CodeMaster.OrderType.Procurement);
            selectFlowDetParam.Add(true);
            selectFlowDetParam.Add(true);

            IList<object[]> flowDetailList = this.genericMgr.FindAllWithNativeSql<object[]>(selectFlowDetSql.ToString(), selectFlowDetParam.ToArray());
            #endregion

            #region 由于insertedProcOrderList对象里计划协议号+行号是唯一，先获取所有计划协议订单
            StringBuilder selectOrderMstrSql = new StringBuilder(@"select * from ORD_OrderMstr_8 as m where 1 = 1");
            IList<object> selectOrderMstrParam = new List<object>();

            //if (!string.IsNullOrWhiteSpace(flow))
            //{
            //    selectOrderMstrSql.Append(" and m.Flow = ?");
            //    selectOrderMstrParam.Add(flow);
            //}
            //else 
            if (!string.IsNullOrWhiteSpace(supplier))
            {
                selectOrderMstrSql.Append(" and m.PartyFrom = ?");
                selectOrderMstrParam.Add(supplier);
            }

            if (!string.IsNullOrWhiteSpace(item))
            {
                selectOrderMstrSql.Append(" and exists(select top 1 1 from ORD_OrderDet_8 as o where o.OrderNo = m.OrderNo and o.Item = ?)");
                selectOrderMstrParam.Add(item);
            }
            IList<OrderMaster> orderMasterList = this.genericMgr.FindEntityWithNativeSql<OrderMaster>(selectOrderMstrSql.ToString(), selectOrderMstrParam.ToArray());
            #endregion

            #region 查找符合条件的计划协议订单明细
            StringBuilder selectOrderDetSql = new StringBuilder(@"select d.* from ORD_OrderDet_8 as d 
                                                                    inner join ORD_OrderMstr_8 as m on d.OrderNo = m.OrderNo where 1 = 1");
            IList<object> selectOrderDetParam = new List<object>();

            //if (!string.IsNullOrWhiteSpace(flow))
            //{
            //    selectOrderDetSql.Append(" and m.Flow = ?");
            //    selectOrderDetParam.Add(flow);
            //}
            //else 
            if (!string.IsNullOrWhiteSpace(supplier))
            {
                selectOrderDetSql.Append(" and m.PartyFrom = ?");
                selectOrderDetParam.Add(supplier);
            }

            if (!string.IsNullOrWhiteSpace(item))
            {
                selectOrderDetSql.Append(" and d.Item = ?");
                selectOrderDetParam.Add(item);
            }

            IList<OrderDetail> orderDetailList = this.genericMgr.FindEntityWithNativeSql<OrderDetail>(selectOrderDetSql.ToString(), selectOrderDetParam.ToArray());
            #endregion

            foreach (Entity.SAP.ORD.ProcOrder procOrder in insertedProcOrderList)
            {
                try
                {
                    string partyFrom = !string.IsNullOrWhiteSpace(supplier) ? supplier : (!string.IsNullOrWhiteSpace(procOrder.LIFNR) ? procOrder.LIFNR : procOrder.RESWK);

                    //根据partyfrom+item匹配有效的采购路线明细，一定要控制item在同一条采购路线不能出现多次
                    IList<object[]> matchedFlowDetailList = (from f in flowDetailList
                                                             where f[1].ToString() == partyFrom && f[2].ToString() == procOrder.MATNR
                                                             select f).ToList<object[]>();

                    if (matchedFlowDetailList != null && matchedFlowDetailList.Count() > 0)
                    {
                        foreach (object[] matchedFlowDetail in matchedFlowDetailList)
                        {
                            //查找满足条件的order
                            List<OrderMaster> matchedOrderMaster = null;
                            if (orderMasterList != null && orderMasterList.Count > 0)
                            {
                                matchedOrderMaster = (from o in orderMasterList
                                                      where o.ExternalOrderNo == procOrder.EBELN && o.ReferenceOrderNo == procOrder.EBELP
                                                      select o).ToList();
                            }
                            FlowMaster matchedFlowMaster = (from f in flowMasterList
                                                            where f.Code == matchedFlowDetail[0].ToString()
                                                            select f).Single();

                            //创建LES订单
                            if (matchedOrderMaster != null && matchedOrderMaster.Count == 1)
                                returnOrderDetailList.AddRange(CreateLesProcOrder(procOrder, matchedFlowMaster, matchedFlowDetail, matchedOrderMaster[0], orderDetailList));
                            else if (matchedOrderMaster != null && matchedOrderMaster.Count > 1)
                                continue;
                            else if (matchedOrderMaster == null || matchedOrderMaster.Count == 0)
                                returnOrderDetailList.AddRange(CreateLesProcOrder(procOrder, matchedFlowMaster, matchedFlowDetail, null, orderDetailList));
                        }
                    }
                    else
                    {
                        //throw new BusinessException("和计划协议匹配的采购路线{0}物料代码{1}不存在，请联系计划员。", flow, item);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                }

            }
            return Mapper.Map<List<com.Sconit.Entity.ORD.OrderDetail>, List<com.Sconit.Entity.SAP.ORD.OrderDetail>>(returnOrderDetailList);
            #endregion
        }

        //新方法
        //仍然采用一个计划协议+协议行号对应一张les订单
        //因为物料切换目的库位时，光改明细上的库位会有问题
        //订单头上很多信息打印在单据上都是错误的
        private IList<OrderDetail> CreateLesProcOrder(Entity.SAP.ORD.ProcOrder procOrder, FlowMaster flowMaster, object[] flowDetail, OrderMaster orderMaster, IList<OrderDetail> orderDetailList)
        {
            IList<OrderDetail> returnOrderDetailList = new List<OrderDetail>();
            try
            {
                #region 创建订单头
                if (orderMaster == null)
                {
                    orderMaster = this.orderMgr.TransferFlow2Order(flowMaster, null);
                    orderMaster.ExternalOrderNo = procOrder.EBELN;                     //计划协议号
                    orderMaster.ReferenceOrderNo = procOrder.EBELP;                    //采购凭证的项目编号 
                    orderMaster.WindowTime = DateTime.Now;             //窗口时间
                    orderMaster.StartTime = DateTime.Now;  //开始日期
                    orderMaster.IsInspect = procOrder.NOTQC == "Y" ? false : true;   //检验标记
                    orderMaster.Type = com.Sconit.CodeMaster.OrderType.ScheduleLine;
                    orderMaster.OrderNo = numberControlMgr.GetOrderNo(orderMaster);

                    this.orderMgr.CreateOrder(orderMaster);
                }
                else
                {
                    //提升效率
                    //当les订单显示为需要检验，但sap显示为免检或les显示为免检，但sap显示为检验时才更新
                    if (procOrder.NOTQC == "Y" && orderMaster.IsInspect || procOrder.NOTQC == "N" && !orderMaster.IsInspect)
                    {
                        orderMaster.IsInspect = procOrder.NOTQC == "Y" ? false : true;
                        genericMgr.Update(orderMaster);
                        this.genericMgr.FlushSession();
                    }

                    //if (orderMaster.Flow == flowMaster.Code)
                    //{
                    //    //提升效率
                    //    //当les订单显示为需要检验，但sap显示为免检或les显示为免检，但sap显示为检验时才更新
                    //    if (procOrder.NOTQC == "Y" && orderMaster.IsInspect || procOrder.NOTQC == "N" && !orderMaster.IsInspect)
                    //    {
                    //        orderMaster.IsInspect = procOrder.NOTQC == "Y" ? false : true;
                    //        genericMgr.Update(orderMaster);
                    //    }
                    //}
                    //如果路线发生变化，则更新订单头和订单明细的主要信息
                    //为了避免不必要的错误，路线变化后其他选项信息暂时不copy
                    //切记新维护采购路线时一定要设置允许超发，允许超收，允许asn多次收货
                    //else
                    //{
                    //更新订单
                    //orderMaster.Flow = flowMaster.Code;
                    //orderMaster.FlowDescription = flowMaster != null ? flowMaster.Description : null;
                    //orderMaster.PartyTo = flowMaster != null ? flowMaster.PartyTo : null;
                    //Party partyTo = flowMaster != null ? this.genericMgr.FindById<Party>(flowMaster.PartyTo) : null;
                    //orderMaster.PartyToName = flowMaster != null ? partyTo.Name : null;
                    //orderMaster.ShipTo = flowMaster != null ? flowMaster.ShipTo : null;
                    //Address address = flowMaster != null ? this.genericMgr.FindById<Address>(flowMaster.ShipTo) : null;
                    //orderMaster.ShipToAddress = flowMaster != null ? address.AddressContent : null;
                    //orderMaster.ShipToCell = flowMaster != null ? address.MobilePhone : null; ;
                    //orderMaster.ShipToTel = flowMaster != null ? address.TelPhone : null;
                    //orderMaster.ShipToContact = flowMaster != null ? address.ContactPersonName : null;
                    //orderMaster.ShipToFax = flowMaster != null ? address.Fax : null;
                    //orderMaster.LocationTo = flowMaster != null ? flowMaster.LocationTo : null;
                    //Location location = flowMaster != null ? this.genericMgr.FindById<Location>(flowMaster.LocationTo) : null;
                    //orderMaster.LocationToName = flowMaster != null ? location.Name : null;
                    //orderMaster.IsInspect = procOrder.NOTQC == "Y" ? false : true;
                    //genericMgr.Update(orderMaster);

                    //取最新路线明细数据更新订单明细
                    //OrderDetail orderDetail = (from o in orderDetailList
                    //                           where o.ExternalOrderNo == orderMaster.ExternalOrderNo && o.ExternalSequence == orderMaster.ReferenceOrderNo
                    //                           select o).ToList()[0];
                    //FlowDetail flowDetail = this.genericMgr.FindAll<FlowDetail>("from FlowDetail where Flow = ? and Item = ?", new object[] { orderMaster.Flow, orderDetail.Item })[0];
                    //orderDetail.UnitCount = flowDetail.UnitCount;
                    //orderDetail.Uom = flowDetail.Uom;
                    //orderDetail.BaseUom = flowDetail.BaseUom;
                    ////库位取路线头上的
                    //orderDetail.LocationTo = flowMaster.LocationTo;
                    //orderDetail.LocationToName = genericMgr.FindById<Location>(flowMaster.LocationTo).Name;
                    //genericMgr.Update(orderDetail);
                    //}
                }
                #endregion

                #region 创建订单明细
                //int id = 0;
                //提升效率，但存在风险
                IList<OrderDetail> existedOrderDetail = (from o in orderDetailList
                                                         where o.ExternalOrderNo == procOrder.EBELN && o.ExternalSequence == procOrder.EBELP
                                                         select o).ToList();

                //定义宿主订单明细用来copy,一旦首次插入失败，宿主订单将无法赋值可能引起出错
                OrderDetail hostOrderDetail = new OrderDetail();
                for (int i = 0; i < procOrder.ProcOrderDetails.Count(); i++)
                {
                    try
                    {
                        #region 只在第一次执行插入，如果失败会存在风险
                        if (i == 0 && (existedOrderDetail == null || existedOrderDetail.Count == 0))
                        {
                            //if (flowDetailList == null || flowDetailList.Count == 0)
                            //{
                            //    throw new BusinessException("和计划协议匹配的采购路线{0}物料代码{1}不存在，请联系计划员。", flowMaster.Code, procOrder.ProcOrderDetails[i].MATNR);
                            //}
                            //object[] flowDetail = flowDetailList[0];
                            OrderDetail orderDetail = new OrderDetail();
                            orderDetail.UnitCount = (decimal)flowDetail[3];
                            //orderDetail.IsInspect = flowDetail.IsInspect;
                            //if (!string.IsNullOrEmpty(flowDetail.LocationTo))
                            //{
                            //    orderDetail.LocationTo = flowDetail.LocationTo;
                            //    orderDetail.LocationToName = lesMgr.FindById<Location>(flowDetailList[0].LocationTo).Name;
                            //}
                            PrepareOrderDetail(orderDetail, orderMaster, procOrder.ProcOrderDetails[i], procOrder, DateTime.Now);
                            if (string.IsNullOrWhiteSpace(orderDetail.Uom))
                            {
                                orderDetail.Uom = (string)flowDetail[4];
                            }
                            if (string.IsNullOrWhiteSpace(orderDetail.BaseUom))
                            {
                                orderDetail.BaseUom = (string)flowDetail[5];
                            }
                            if (string.IsNullOrWhiteSpace(orderDetail.ItemDescription))
                            {
                                orderDetail.ItemDescription = (string)flowDetail[7];
                            }
                            if (string.IsNullOrWhiteSpace(orderDetail.ReferenceItemCode))
                            {
                                orderDetail.ReferenceItemCode = (string)flowDetail[6];
                            }
                            if (string.IsNullOrWhiteSpace(orderDetail.BillAddress)
                                && !string.IsNullOrWhiteSpace(orderMaster.BillAddress))
                            {
                                orderDetail.BillAddress = orderMaster.BillAddress;
                                orderDetail.BillAddressDescription = orderMaster.BillAddressDescription;
                            }
                            genericMgr.Create(orderDetail);
                            this.genericMgr.FlushSession();

                            #region 自己拼Orderdetail,显示用不更新数据库
                            DateTime EINDT = DateTime.Parse(procOrder.ProcOrderDetails[0].EINDT);
                            //orderDetail.Id = orderDetail.Id + id;
                            orderDetail.StartDate = EINDT;
                            orderDetail.EndDate = EINDT;
                            orderDetail.OrderedQty = procOrder.ProcOrderDetails[0].MENGE;
                            orderDetail.FreezeDays = Convert.ToInt32(procOrder.ETFZ1);
                            orderDetail.Flow = flowMaster.Code;
                            orderDetail.ManufactureParty = flowMaster != null ? flowMaster.PartyFrom : null;
                            //sap的收货数,仅仅是用于需求预测报表，和计划协议的已收数可能有误差
                            orderDetail.ReceivedQty = procOrder.ProcOrderDetails[i].WEMNG.HasValue ? procOrder.ProcOrderDetails[i].WEMNG.Value : decimal.Zero;
                            returnOrderDetailList.Add(orderDetail);

                            //记录到宿主订单用于复制
                            hostOrderDetail = orderDetail;
                            //id++;
                            #endregion
                        }
                        else if (existedOrderDetail != null && existedOrderDetail.Count > 0)
                        {
                            //先更新原有订单
                            PrepareOrderDetail(existedOrderDetail[0], orderMaster, procOrder.ProcOrderDetails[i], procOrder, DateTime.Now);
                            OrderDetail newOrderDetail = new OrderDetail();

                            #region 自己拼Orderdetail,显示用不更新数据库
                            newOrderDetail.Id = existedOrderDetail[0].Id;
                            newOrderDetail.Item = existedOrderDetail[0].Item;
                            newOrderDetail.ReferenceItemCode = existedOrderDetail[0].ReferenceItemCode;
                            newOrderDetail.ItemDescription = existedOrderDetail[0].ItemDescription;
                            newOrderDetail.ExternalOrderNo = existedOrderDetail[0].ExternalOrderNo;
                            newOrderDetail.ExternalSequence = existedOrderDetail[0].ExternalSequence;
                            newOrderDetail.OrderNo = existedOrderDetail[0].OrderNo;
                            DateTime EINDT = DateTime.Parse(procOrder.ProcOrderDetails[i].EINDT);
                            newOrderDetail.OrderType = CodeMaster.OrderType.ScheduleLine;
                            newOrderDetail.LocationTo = flowMaster.LocationTo;
                            newOrderDetail.ShippedQty = existedOrderDetail[0].ShippedQty;
                            newOrderDetail.ReceivedQty = existedOrderDetail[0].ReceivedQty;
                            newOrderDetail.UnitCount = (decimal)flowDetail[3];
                            newOrderDetail.Uom = existedOrderDetail[0].Uom;
                            newOrderDetail.StartDate = EINDT;
                            newOrderDetail.EndDate = EINDT;
                            newOrderDetail.OrderedQty = procOrder.ProcOrderDetails[i].MENGE;
                            newOrderDetail.FreezeDays = Convert.ToInt32(procOrder.ETFZ1);
                            newOrderDetail.Flow = flowMaster.Code;
                            newOrderDetail.ManufactureParty = flowMaster != null ? flowMaster.PartyFrom : null;
                            newOrderDetail.BillTerm = existedOrderDetail[0].BillTerm;
                            //sap的收货数,仅仅是用于需求预测报表，和计划协议的已收数可能有误差
                            newOrderDetail.ReceivedQty = procOrder.ProcOrderDetails[i].WEMNG.HasValue ? procOrder.ProcOrderDetails[i].WEMNG.Value : decimal.Zero;
                            returnOrderDetailList.Add(newOrderDetail);
                            //id++;

                            genericMgr.Update(existedOrderDetail[0]);
                            this.genericMgr.FlushSession();
                            #endregion
                        }
                        else if (existedOrderDetail == null || existedOrderDetail.Count == 0)
                        {
                            OrderDetail newOrderDetail = new OrderDetail();
                            //OrderDetail existedDetail = (from o in lesOrderDetailList
                            //                                     where o.ExternalOrderNo == procOrderDetail.EBELN && o.ExternalSequence == procOrderDetail.EBELP
                            //                                     select o).ToList()[0];
                            #region 自己拼Orderdetail,显示用不更新数据库
                            newOrderDetail.Id = 0;
                            newOrderDetail.Item = hostOrderDetail.Item;
                            newOrderDetail.ReferenceItemCode = hostOrderDetail.ReferenceItemCode;
                            newOrderDetail.ItemDescription = hostOrderDetail.ItemDescription;
                            newOrderDetail.ExternalOrderNo = hostOrderDetail.ExternalOrderNo;
                            newOrderDetail.ExternalSequence = hostOrderDetail.ExternalSequence;
                            newOrderDetail.OrderNo = hostOrderDetail.OrderNo;
                            DateTime EINDT = DateTime.Parse(procOrder.ProcOrderDetails[i].EINDT);
                            newOrderDetail.OrderType = CodeMaster.OrderType.ScheduleLine;
                            newOrderDetail.LocationTo = flowMaster.LocationTo;
                            newOrderDetail.ShippedQty = 0;
                            newOrderDetail.ReceivedQty = 0;
                            newOrderDetail.UnitCount = hostOrderDetail.UnitCount;
                            newOrderDetail.Uom = hostOrderDetail.Uom;
                            newOrderDetail.StartDate = EINDT;
                            newOrderDetail.EndDate = EINDT;
                            newOrderDetail.OrderedQty = procOrder.ProcOrderDetails[i].MENGE;
                            newOrderDetail.FreezeDays = Convert.ToInt32(procOrder.ETFZ1);
                            newOrderDetail.Flow = flowMaster.Code;
                            newOrderDetail.ManufactureParty = flowMaster != null ? flowMaster.PartyFrom : null;
                            newOrderDetail.BillTerm = hostOrderDetail.BillTerm;
                            //sap的收货数,仅仅是用于需求预测报表，和计划协议的已收数可能有误差
                            newOrderDetail.ReceivedQty = procOrder.ProcOrderDetails[i].WEMNG.HasValue ? procOrder.ProcOrderDetails[i].WEMNG.Value : decimal.Zero;
                            returnOrderDetailList.Add(newOrderDetail);
                            //id++;
                            #endregion
                        }
                        #endregion

                    }
                    catch (Exception ex)
                    {
                        string logMessage = string.Format("创建采购路线{0}物料代码{1}计划协议号{2}行号{3}的计划协议明细出现异常，异常信息：{4}", orderMaster.Flow, procOrder.ProcOrderDetails[i].MATNR, procOrder.EBELN, procOrder.EBELP, ex.Message);
                        log.Error(logMessage, ex);
                        this.genericMgr.CleanSession();
                        throw new BusinessException(logMessage);
                    }
                }

                #endregion
                //明细创建成功后，更新订单状态
                if (orderMaster.Status == CodeMaster.OrderStatus.Create)
                {
                    orderMaster.Status = CodeMaster.OrderStatus.Submit;
                    this.genericMgr.Update(orderMaster);
                    this.genericMgr.FlushSession();
                }

                //返回订单明细用于portal显示
                return returnOrderDetailList;
            }
            catch (Exception ex)
            {
                string logMessage = string.Format("创建协议号{0}行号{1}的计划协议出现异常，异常信息：{2}", procOrder.EBELN, procOrder.EBELP, ex.Message);
                log.Error(logMessage, ex);
                this.genericMgr.CleanSession();
                throw new BusinessException(logMessage);
            }
        }

        //一张计划协议可能对应2个目的库位，在我们系统中会对应2条路线，因此可能会变成2张订单，订单号也要重新生成
        //private void CreateLesProcOrder(Entity.SAP.ORD.ProcOrder procOrder, FlowMaster flowMaster, DateTime dateTimeNow, IList<ErrorMessage> errorMessageList)
        //{
        //    try
        //    {
        //        if (procOrder.ProcOrderDetails.Count == 0)
        //        {
        //            throw new Exception(GetTLog<Entity.SAP.ORD.ProcOrder>(procOrder, "采购单明细为空。"));
        //        }

        //        #region 创建订单头
        //        //com.Sconit.CodeMaster.OrderType orderType = (procOrder.BSART == "ZOPO" || procOrder.BSART == "LU") ? com.Sconit.CodeMaster.OrderType.ScheduleLine : com.Sconit.CodeMaster.OrderType.Procurement;
        //        com.Sconit.CodeMaster.OrderType orderType = com.Sconit.CodeMaster.OrderType.ScheduleLine;
        //        bool isCreate = false;
        //        IList<OrderMaster> orderList = new List<OrderMaster>();

        //        if (orderType == com.Sconit.CodeMaster.OrderType.ScheduleLine)
        //        {
        //            orderList = this.lesMgr.FindAll<OrderMaster>("from OrderMaster where ExternalOrderNo = ? and Flow = ?", new object[] { procOrder.EBELN, flowMaster.Code });
        //        }

        //        OrderMaster orderMaster = orderList != null && orderList.Count > 0 ? orderList[0] : null;

        //        if (orderMaster == null)
        //        {
        //            isCreate = true;

        //            string minWindowTime = (from p in procOrder.ProcOrderDetails where p.EINDT != null select p.EINDT).Min();
        //            FlowStrategy flowStrategy = this.lesMgr.FindById<FlowStrategy>(flowMaster.Code);
        //            DateTime windowTime = string.IsNullOrEmpty(minWindowTime) ? DateTime.Now : DateTime.Parse(minWindowTime);

        //            orderMaster = this.orderMgr.TransferFlow2Order(flowMaster, null);
        //            orderMaster.ExternalOrderNo = procOrder.EBELN;                     //计划协议号
        //            orderMaster.WindowTime = windowTime;             //窗口时间
        //            orderMaster.StartTime = windowTime.AddHours((double)flowStrategy.LeadTime);  //开始日期
        //            //orderMaster.IsInspect = procOrder.NOTQC == "Y" ? true : false;
        //            orderMaster.IsInspect = procOrder.NOTQC == "Y" ? false : true;
        //            orderMaster.Type = orderType;
        //            orderMaster.OrderNo = numberControlMgr.GetOrderNo(orderMaster);
        //            orderMaster.FreezeDay = Int32.Parse(procOrder.ETFZ1);  //冻结日期放头上
        //        }

        //        if (!isCreate)
        //        {
        //            //if (orderMaster.Status != CodeMaster.OrderStatus.Create)
        //            //{
        //            //    throw new Exception(GetTLog<Entity.SAP.ORD.ProcOrder>(procOrder, procOrder.EBELN + "状态不是Create，不能更新。"));
        //            //}
        //            orderMaster.IsInspect = procOrder.NOTQC == "Y" ? false : true;
        //            lesMgr.Update(orderMaster);
        //        }
        //        #endregion

        //        #region 创建订单明细
        //        //先处理新增的，后处理更新的
        //        var qNew = procOrder.ProcOrderDetails.Where(p => string.IsNullOrWhiteSpace(p.EKPOBJ));
        //        var qUpdate = procOrder.ProcOrderDetails.Where(p => p.EKPOBJ == "X");

        //        OrderDetail orderDetail = null;
        //        if (isCreate)
        //        {
        //            #region 新增的
        //            if (qNew != null && qNew.ToList().Count > 0)
        //            {
        //                foreach (Entity.SAP.ORD.ProcOrderDetail procOrderDetail in qNew.ToList())
        //                {
        //                    try
        //                    {
        //                        orderDetail = GetExistOrderDetail(orderMaster, procOrderDetail);
        //                        if (orderDetail != null)
        //                        {
        //                            throw new Exception(GetTLog<Entity.SAP.ORD.ProcOrder>(procOrder, "项目号：" + procOrderDetail.EBELP + "行号：" + procOrderDetail.ETENR + "已存在对应的订单明细。"));
        //                        }

        //                        IList<FlowDetail> flowDetailList = this.lesMgr.FindAll<FlowDetail>("from FlowDetail where Flow = ? and Item = ?", new object[] { orderMaster.Flow, procOrderDetail.MATNR });
        //                        if (flowDetailList == null || flowDetailList.Count == 0)
        //                        {
        //                            throw new Exception(GetTLog<Entity.SAP.ORD.ProcOrder>(procOrder, "找不到对应的路线明细。"));
        //                        }

        //                        #region 冻结期放在flowdet上
        //                        //FlowDetail flowDetail = flowDetailList[0];
        //                        //flowDetail.FreezeDays = Int32.Parse(procOrder.ETFZ1);
        //                        //lesMgr.Update(flowDetail);
        //                        #endregion

        //                        orderDetail = new OrderDetail();
        //                        //orderDetail.PartyFrom = orderMaster.PartyFrom;
        //                        //orderDetail.PartyTo = orderMaster.PartyTo;
        //                        orderDetail.UnitCount = flowDetail.UnitCount;
        //                        orderDetail.IsInspect = flowDetail.IsInspect;
        //                        if (!string.IsNullOrEmpty(flowDetail.LocationTo))
        //                        {
        //                            orderDetail.LocationTo = flowDetail.LocationTo;
        //                            orderDetail.LocationToName = lesMgr.FindById<Location>(flowDetailList[0].LocationTo).Name;
        //                        }
        //                        PrepareOrderDetail(orderDetail, orderMaster, procOrderDetail, procOrder, dateTimeNow);
        //                        if (string.IsNullOrWhiteSpace(orderDetail.Uom))
        //                        {
        //                            orderDetail.Uom = flowDetail.Uom;
        //                        }
        //                        if (string.IsNullOrWhiteSpace(orderDetail.BaseUom))
        //                        {
        //                            orderDetail.BaseUom = flowDetail.BaseUom;
        //                        }
        //                        if (string.IsNullOrWhiteSpace(orderDetail.ItemDescription))
        //                        {
        //                            orderDetail.ItemDescription = this.lesMgr.FindAll<string>("select Description from Item where Code = ?", orderDetail.Item).Single();
        //                        }
        //                        if (string.IsNullOrWhiteSpace(orderDetail.ReferenceItemCode))
        //                        {
        //                            orderDetail.ReferenceItemCode = flowDetail.ReferenceItemCode;
        //                        }
        //                        orderMaster.AddOrderDetail(orderDetail);
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        string logMessage = string.IsNullOrEmpty(ex.Message) ? GetTLog<Entity.SAP.ORD.ProcOrder>(procOrder, "创建订单明细失败。") : ex.Message;
        //                        log.Error(logMessage, ex);
        //                        errorMessageList.Add(new ErrorMessage
        //                        {
        //                            Template = NVelocityTemplateRepository.TemplateEnum.ImportProcOrder_CreateProcOrder,
        //                            Exception = ex,
        //                            Message = logMessage
        //                        });

        //                        procOrderDetail.Status = Entity.SAP.StatusEnum.Fail;
        //                        procOrderDetail.ErrorCount++;
        //                        //this.UpdateSiSap(procOrderDetail);
        //                        continue;
        //                    }
        //                }
        //            }
        //            #endregion

        //            #region 更新的，更新新增的数据
        //            if (qUpdate != null && qUpdate.ToList().Count > 0)
        //            {
        //                foreach (Entity.SAP.ORD.ProcOrderDetail procOrderDetail in qUpdate.ToList())
        //                {
        //                    try
        //                    {
        //                        orderDetail = GetExistOrderDetail(orderMaster, procOrderDetail);
        //                        if (orderDetail == null)
        //                        {
        //                            IList<FlowDetail> flowDetailList = this.lesMgr.FindAll<FlowDetail>("from FlowDetail where Flow = ? and Item = ?", new object[] { orderMaster.Flow, procOrderDetail.MATNR });
        //                            if (flowDetailList == null || flowDetailList.Count == 0)
        //                            {
        //                                throw new Exception(GetTLog<Entity.SAP.ORD.ProcOrder>(procOrder, "找不到对应的路线明细。"));
        //                            }

        //                            #region 冻结期放在flowdet上
        //                            //FlowDetail flowDetail = flowDetailList[0];
        //                            //if (flowDetail.FreezeDays != Int32.Parse(procOrder.ETFZ1))
        //                            //{
        //                            //    flowDetail.FreezeDays = Int32.Parse(procOrder.ETFZ1);
        //                            //    lesMgr.Update(flowDetail);
        //                            //}

        //                            #endregion

        //                            orderDetail = new OrderDetail();
        //                            //orderDetail.PartyFrom = orderMaster.PartyFrom;
        //                            //orderDetail.PartyTo = orderMaster.PartyTo;
        //                            orderDetail.UnitCount = flowDetailList[0].UnitCount;
        //                            orderDetail.IsInspect = flowDetailList[0].IsInspect;
        //                            if (!string.IsNullOrEmpty(flowDetailList[0].LocationTo))
        //                            {
        //                                orderDetail.LocationTo = flowDetailList[0].LocationTo;
        //                                orderDetail.LocationToName = lesMgr.FindById<Location>(flowDetailList[0].LocationTo).Name;
        //                            }
        //                            PrepareOrderDetail(orderDetail, orderMaster, procOrderDetail, procOrder, dateTimeNow);

        //                            if (string.IsNullOrWhiteSpace(orderDetail.Uom))
        //                            {
        //                                orderDetail.Uom = flowDetailList[0].Uom;
        //                            }
        //                            if (string.IsNullOrWhiteSpace(orderDetail.BaseUom))
        //                            {
        //                                orderDetail.BaseUom = flowDetailList[0].BaseUom;
        //                            }
        //                            if (string.IsNullOrWhiteSpace(orderDetail.ItemDescription))
        //                            {
        //                                orderDetail.ItemDescription = this.lesMgr.FindAll<string>("select Description from Item where Code = ?", orderDetail.Item).Single();
        //                            }
        //                            if (string.IsNullOrWhiteSpace(orderDetail.ReferenceItemCode))
        //                            {
        //                                orderDetail.ReferenceItemCode = flowDetailList[0].ReferenceItemCode;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            PrepareOrderDetail(orderDetail, orderMaster, procOrderDetail, procOrder, dateTimeNow);
        //                        }

        //                        orderMaster.AddOrderDetail(orderDetail);
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        string logMessage = string.IsNullOrEmpty(ex.Message) ? GetTLog<Entity.SAP.ORD.ProcOrder>(procOrder, "创建订单明细失败。") : ex.Message;
        //                        log.Error(logMessage, ex);
        //                        errorMessageList.Add(new ErrorMessage
        //                        {
        //                            Template = NVelocityTemplateRepository.TemplateEnum.ImportProcOrder_CreateProcOrder,
        //                            Exception = ex,
        //                            Message = logMessage
        //                        });

        //                        procOrderDetail.Status = Entity.SAP.StatusEnum.Fail;
        //                        procOrderDetail.ErrorCount++;
        //                        //this.UpdateSiSap(procOrderDetail);
        //                        continue;
        //                    }
        //                }
        //            }
        //            #endregion
        //        }
        //        else
        //        {
        //            IList<OrderDetail> orderDetailList = this.lesMgr.FindAll<OrderDetail>("from OrderDetail where OrderNo = ?  ", orderMaster.OrderNo);
        //            orderMaster.OrderDetails = orderDetailList;
        //            #region 新增的
        //            if (qNew != null && qNew.ToList().Count > 0)
        //            {
        //                foreach (Entity.SAP.ORD.ProcOrderDetail procOrderDetail in qNew.ToList())
        //                {
        //                    try
        //                    {
        //                        IList<FlowDetail> flowDetailList = this.lesMgr.FindAll<FlowDetail>("from FlowDetail where Flow = ? and Item = ?", new object[] { orderMaster.Flow, procOrderDetail.MATNR });
        //                        //if (flowDetailList != null && flowDetailList.Count > 0)
        //                        //{
        //                        //    FlowDetail flowDetail = flowDetailList[0];
        //                        //    flowDetail.FreezeDays = Int32.Parse(procOrder.ETFZ1);
        //                        //    lesMgr.Update(flowDetail);
        //                        //}

        //                        orderDetail = GetExistOrderDetail(orderMaster, procOrderDetail);
        //                        if (orderDetail != null)
        //                        {
        //                            continue;
        //                            //throw new Exception(GetTLog<Entity.SAP.ORD.ProcOrder>(procOrder, "项目号：" + procOrderDetail.EBELP + "行号：" + procOrderDetail.ETENR + "已存在对应的订单明细。"));
        //                        }

        //                        orderDetail = CreateOrderDetail(orderMaster, procOrder, procOrderDetail, dateTimeNow);
        //                        orderMaster.OrderDetails.Add(orderDetail);
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        string logMessage = string.IsNullOrEmpty(ex.Message) ? GetTLog<Entity.SAP.ORD.ProcOrder>(procOrder, "创建订单明细失败。") : ex.Message;
        //                        log.Error(logMessage, ex);
        //                        errorMessageList.Add(new ErrorMessage
        //                        {
        //                            Template = NVelocityTemplateRepository.TemplateEnum.ImportProcOrder_CreateProcOrder,
        //                            Exception = ex,
        //                            Message = logMessage
        //                        });

        //                        procOrderDetail.Status = Entity.SAP.StatusEnum.Fail;
        //                        procOrderDetail.ErrorCount++;
        //                        //this.UpdateSiSap(procOrderDetail);
        //                        continue;
        //                    }
        //                }
        //            }
        //            #endregion

        //            #region 更新的
        //            if (qUpdate != null && qUpdate.ToList().Count > 0)
        //            {
        //                foreach (Entity.SAP.ORD.ProcOrderDetail procOrderDetail in qUpdate.ToList())
        //                {
        //                    try
        //                    {
        //                        IList<FlowDetail> flowDetailList = this.lesMgr.FindAll<FlowDetail>("from FlowDetail where Flow = ? and Item = ?", new object[] { orderMaster.Flow, procOrderDetail.MATNR });
        //                        //if (flowDetailList != null && flowDetailList.Count > 0)
        //                        //{
        //                        //    FlowDetail flowDetail = flowDetailList[0];
        //                        //    flowDetail.FreezeDays = Int32.Parse(procOrder.ETFZ1);
        //                        //    lesMgr.Update(flowDetail);
        //                        //}

        //                        orderDetail = GetExistOrderDetail(orderMaster, procOrderDetail);
        //                        if (orderDetail == null)
        //                        {
        //                            orderDetail = CreateOrderDetail(orderMaster, procOrder, procOrderDetail, dateTimeNow);
        //                        }
        //                        else
        //                        {
        //                            PrepareOrderDetail(orderDetail, orderMaster, procOrderDetail, procOrder, dateTimeNow);
        //                            lesMgr.Update(orderDetail);
        //                        }

        //                        orderMaster.OrderDetails.Add(orderDetail);
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        string logMessage = string.IsNullOrEmpty(ex.Message) ? GetTLog<Entity.SAP.ORD.ProcOrder>(procOrder, "创建订单明细失败。") : ex.Message;
        //                        log.Error(logMessage, ex);
        //                        errorMessageList.Add(new ErrorMessage
        //                        {
        //                            Template = NVelocityTemplateRepository.TemplateEnum.ImportProcOrder_CreateProcOrder,
        //                            Exception = ex,
        //                            Message = logMessage
        //                        });

        //                        procOrderDetail.Status = Entity.SAP.StatusEnum.Fail;
        //                        procOrderDetail.ErrorCount++;
        //                        //this.UpdateSiSap(procOrderDetail);
        //                        continue;
        //                    }
        //                }
        //            }
        //            #endregion
        //        }

        //        #endregion

        //        if (isCreate)
        //        {
        //            if (orderMaster.OrderDetails != null && orderMaster.OrderDetails.Count > 0
        //                && orderMaster.OrderDetails.Where(ord => ord.OrderedQty > 0).Count() > 0)
        //            {
        //                orderMaster.IsAutoRelease = true;
        //                this.orderMgr.CreateOrder(orderMaster);
        //            }
        //        }

        //        this.lesMgr.FlushSession();

        //        //this.UpdateSiSap(procOrder.ProcOrderDetails);
        //    }
        //    catch (Exception ex)
        //    {
        //        string logMessage = string.IsNullOrEmpty(ex.Message) ? GetTLog<Entity.SAP.ORD.ProcOrder>(procOrder, "创建采购单失败。") : ex.Message;
        //        log.Error(logMessage, ex);
        //        errorMessageList.Add(new ErrorMessage
        //        {
        //            Template = NVelocityTemplateRepository.TemplateEnum.ImportProcOrder_CreateProcOrder,
        //            Exception = ex,
        //            Message = logMessage
        //        });

        //        this.lesMgr.CleanSession();
        //    }
        //}

        //private OrderDetail CreateOrderDetail(OrderMaster orderMaster, Entity.SAP.ORD.ProcOrder procOrder, Entity.SAP.ORD.ProcOrderDetail procOrderDetail, DateTime dateTimeNow)
        //{
        //    IList<FlowDetail> flowDetailList = this.lesMgr.FindAll<FlowDetail>("from FlowDetail where Flow = ? and Item = ?", new object[] { orderMaster.Flow, procOrderDetail.MATNR });
        //    if (flowDetailList == null || flowDetailList.Count == 0)
        //    {

        //        throw new Exception(GetTLog<Entity.SAP.ORD.ProcOrder>(procOrder, "找不到对应的路线明细。"));
        //    }

        //    OrderDetail orderDetail = new OrderDetail();

        //    orderDetail.UnitCount = flowDetailList[0].UnitCount;
        //    orderDetail.IsInspect = flowDetailList[0].IsInspect;
        //    if (!string.IsNullOrEmpty(flowDetailList[0].LocationTo))
        //    {
        //        orderDetail.LocationTo = flowDetailList[0].LocationTo;
        //        orderDetail.LocationToName = lesMgr.FindById<Location>(flowDetailList[0].LocationTo).Name;

        //    }
        //    PrepareOrderDetail(orderDetail, orderMaster, procOrderDetail, procOrder, dateTimeNow);
        //    if (string.IsNullOrWhiteSpace(orderDetail.Uom))
        //    {
        //        orderDetail.Uom = flowDetailList[0].Uom;
        //    }
        //    if (string.IsNullOrWhiteSpace(orderDetail.BaseUom))
        //    {
        //        orderDetail.BaseUom = flowDetailList[0].BaseUom;
        //    }
        //    if (string.IsNullOrWhiteSpace(orderDetail.ItemDescription))
        //    {
        //        orderDetail.ItemDescription = this.lesMgr.FindAll<string>("select Description from Item where Code = ?", orderDetail.Item).Single();
        //    }
        //    if (string.IsNullOrWhiteSpace(orderDetail.ReferenceItemCode))
        //    {
        //        orderDetail.ReferenceItemCode = flowDetailList[0].ReferenceItemCode;
        //    }
        //    if (string.IsNullOrWhiteSpace(orderDetail.BillAddress)
        //              && !string.IsNullOrWhiteSpace(orderMaster.BillAddress))
        //    {
        //        orderDetail.BillAddress = orderMaster.BillAddress;
        //        orderDetail.BillAddressDescription = orderMaster.BillAddressDescription;
        //    }

        //    lesMgr.Create(orderDetail);

        //    return orderDetail;
        //}

        private OrderDetail GetExistOrderDetail(OrderMaster orderMaster, Entity.SAP.ORD.ProcOrderDetail procOrderDetail)
        {
            OrderDetail orderDetail = null;
            if (orderMaster.OrderDetails != null && orderMaster.OrderDetails.Count > 0)
            {
                orderDetail = orderMaster.OrderDetails.Where(o => o.ExternalOrderNo == procOrderDetail.EBELN && o.ExternalSequence == procOrderDetail.EBELP && o.Item == procOrderDetail.MATNR).FirstOrDefault();
            }
            return orderDetail;
        }

        private void PrepareOrderDetail(OrderDetail orderDetail, OrderMaster orderMaster, Entity.SAP.ORD.ProcOrderDetail procOrderDetail, Entity.SAP.ORD.ProcOrder procOrder, DateTime dateTimeNow)
        {
            orderDetail.OrderNo = orderMaster.OrderNo;
            orderDetail.ExternalOrderNo = procOrder.EBELN;
            orderDetail.ExternalSequence = procOrder.EBELP;
            orderDetail.OrderType = orderMaster.Type;
            orderDetail.OrderSubType = orderMaster.SubType;
            orderDetail.Item = procOrderDetail.MATNR;
            orderDetail.ItemDescription = procOrder.TXZ01;
            orderDetail.ReferenceItemCode = procOrder.BISMT;
            orderDetail.Uom = procOrder.MEINS;
            orderDetail.BaseUom = procOrder.LMEIN;
            orderDetail.UnitQty = (procOrder.UMREN == 0 || procOrder.UMREZ == 0) ? 1 : (procOrder.UMREZ / procOrder.UMREN);
            orderDetail.QualityType = CodeMaster.QualityType.Qualified;
            orderDetail.RequiredQty = 1;
            orderDetail.OrderedQty = 1;
            //if (orderDetail.LocationTo == null)
            //{
            //    orderDetail.LocationTo = orderMaster.LocationTo;
            //}
            //计划协议类订单目的库位以订单头为准
            orderDetail.LocationTo = orderMaster.LocationTo;
            orderDetail.LocationToName = orderMaster.LocationToName;
            if (procOrder.PSTYP == "2")  //寄售标识，设置为2的为上线计算，其余的为收货结算
            {
                orderDetail.BillTerm = CodeMaster.OrderBillTerm.OnlineBilling;
            }
            else
            {
                orderDetail.BillTerm = CodeMaster.OrderBillTerm.ReceivingSettlement;
            }
            //临时借用AUFNR暂存PSTYP
            orderDetail.AUFNR = procOrder.PSTYP;
            //ZC1：军车    ZC2：出口车     ZC3：特殊车     ZC5：CKD
            orderDetail.Tax = procOrder.BSART;

            orderDetail.IsInspect = true;

            DateTime EINDT = DateTime.Parse(procOrderDetail.EINDT);
            orderDetail.StartDate = EINDT;
            orderDetail.EndDate = EINDT;

        }

        /// <summary>
        /// 反写计划协议到SAP
        /// </summary>
        public static object lockCreateSAPScheduleLineFromLes = new object();
        public void CreateSAPScheduleLineFromLes()
        {
            lock (lockCreateSAPScheduleLineFromLes)
            {
                string guid = System.Guid.NewGuid().ToString().Replace("-","");
                IList<ErrorMessage> errorMessageList = new List<ErrorMessage>();
                try
                {
                    //实例化webservice
                    log.Info(string.Format("-----------------------------{0}----------------------------", guid));
                    log.Info(string.Format("{0}：连接Web服务反写计划协议开始",guid));

                    IList<com.Sconit.Entity.SAP.ORD.CreateScheduleLine> createScheduleLineList = this.genericMgr.FindAll<com.Sconit.Entity.SAP.ORD.CreateScheduleLine>(
                        "from CreateScheduleLine where Status in(?,?) and ErrorCount < 3 and LIFNR<>?",
                        new object[] { Entity.SAP.StatusEnum.Pending, Entity.SAP.StatusEnum.Fail, "1000000746" }, 0, 1000);
                    log.Info(string.Format("{0}：开始处理共{1}行数据。", guid, createScheduleLineList.Count.ToString()));
                    if (createScheduleLineList != null && createScheduleLineList.Count > 0)
                    {
                        int i = 0;
                        foreach (var createScheduleLine in createScheduleLineList)
                        {
                            i++;
                            log.Info(string.Format("{0}：开始处理第{1}行数据，供应商{2}，物料号{3}。", guid, i.ToString(), createScheduleLine.FRBNR, createScheduleLine.MATNR));
                            procurementOperatorMgrImpl.CreateOneCRSL(createScheduleLine, errorMessageList);
                        }
                    }
                    log.Info(string.Format("{0}：连接Web服务反写计划协议成功",guid));
                }
                catch (Exception ex)
                {
                    string logMessage = string.Format("{0}：连接Web服务反写计划协议出现异常，异常信息：" + ex.Message, guid);
                    log.Error(logMessage, ex);
                    errorMessageList.Add(new ErrorMessage
                    {
                        Template = NVelocityTemplateRepository.TemplateEnum.GenerateSAPScheduleLine,
                        Exception = ex,
                        Message = logMessage
                    });
                }

                this.SendErrorMessage(errorMessageList);
            }
        }

        /// <summary>
        /// 汇总数据反写计划协议到SAP
        /// </summary>
        public static object lockCRSLSummaryFromLes = new object();
        public void CRSLSummaryFromLes()
        {
            lock (lockCRSLSummaryFromLes)
            {
                string guid = System.Guid.NewGuid().ToString().Replace("-", "");
                IList<ErrorMessage> errorMessageList = new List<ErrorMessage>();
                try
                {
                    MI_CRSL_LES.MI_CRSL_LESService serviceProxy = new MI_CRSL_LES.MI_CRSL_LESService();
                    serviceProxy.Credentials = base.Credentials;
                    serviceProxy.Timeout = base.TimeOut;
                    serviceProxy.Url = ReplaceSAPServiceUrl(serviceProxy.Url);

                    IList<com.Sconit.Entity.SAP.ORD.CRSLSummary> crslSummaryList = this.genericMgr.FindEntityWithNativeSql<com.Sconit.Entity.SAP.ORD.CRSLSummary>("exec USP_IF_GenCRSLSummary");
                    log.Info(string.Format("-----------------------------{0}----------------------------", guid));
                    log.Info(string.Format("{0}：连接Web服务反写计划协议开始", guid));
                    int i = 0;
                    if (crslSummaryList != null && crslSummaryList.Count > 0)
                    {
                        log.Info(string.Format("{0}：开始处理共{1}行数据。", guid, crslSummaryList.Count.ToString()));
                        foreach (var crslSummary in crslSummaryList)
                        {
                            i++;
                            log.Info(string.Format("{0}：开始处理第{1}行数据，供应商{2}，物料号{3}。", guid, i.ToString(), crslSummary.FRBNR, crslSummary.MATNR));
                            try
                            {
                                MI_CRSL_LES.ZLSCHE_IN zlscheIn = new MI_CRSL_LES.ZLSCHE_IN();
                                zlscheIn.EINDT = crslSummary.EINDT;
                                zlscheIn.FRBNR = crslSummary.FRBNR;
                                zlscheIn.LIFNR = crslSummary.LIFNR;
                                zlscheIn.MATNR = crslSummary.MATNR;
                                zlscheIn.MENGE = crslSummary.MENGE;
                                zlscheIn.SGTXT = crslSummary.SGTXT;
                                zlscheIn.WERKS = crslSummary.WERKS;
                                zlscheIn.MENGESpecified = true;
                                var zlscheOut = serviceProxy.MI_CRSL_LES(zlscheIn);
                                crslSummary.ProcessBatchNo = guid;
                                if (zlscheOut.STATUS == "S")
                                {
                                    //成功
                                    crslSummary.EBELN = zlscheOut.EBELN;
                                    crslSummary.EBELP = zlscheOut.EBELP;
                                    //crslSummary.ETENR = zlscheOut.ETENR;
                                    crslSummary.MESSAGE = zlscheOut.MESSAGE;
                                    crslSummary.Status = Entity.SAP.StatusEnum.Success;

                                    this.genericMgr.Update(crslSummary);

                                }
                                else
                                {
                                    crslSummary.ErrorCount = crslSummary.ErrorCount + 1;
                                    crslSummary.Status = Entity.SAP.StatusEnum.Fail;
                                    crslSummary.MESSAGE = zlscheOut.MESSAGE;
                                    this.genericMgr.Update(crslSummary);

                                    //失败
                                    string logMessage = "SAP反写计划协议失败，失败信息：" + zlscheOut.MESSAGE;
                                    log.Error(logMessage);

                                    if (crslSummary.ErrorCount == 3)
                                    {
                                        errorMessageList.Add(new ErrorMessage
                                        {
                                            Template = NVelocityTemplateRepository.TemplateEnum.GenerateSAPScheduleLine,
                                            Message = logMessage
                                        });
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                string logMessage = string.Format("连接Web服务反写计划协议出现异常，异常信息：" + ex.Message);
                                log.Error(logMessage, ex);
                                errorMessageList.Add(new ErrorMessage
                                {
                                    Template = NVelocityTemplateRepository.TemplateEnum.GenerateSAPScheduleLine,
                                    Exception = ex,
                                    Message = logMessage
                                });
                            }
                        }
                    }

                    this.genericMgr.FindAllWithNativeSql("exec USP_IF_ProcessCRSL ?",guid);
                }
                catch (Exception ex)
                {
                    string logMessage = string.Format("反写计划协议出现异常，异常信息：" + ex.Message);
                    log.Error(logMessage, ex);
                    errorMessageList.Add(new ErrorMessage
                    {
                        Template = NVelocityTemplateRepository.TemplateEnum.GenerateSAPScheduleLine,
                        Exception = ex,
                        Message = logMessage
                    });
                }

                this.SendErrorMessage(errorMessageList);
            }
        }
    }


    [Transactional]
    public class ProcurementOperatorMgrImpl :BaseMgr, IProcurementOperatorMgr
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("SAP_Procurement");
        //private static log4net.ILog log = log4net.LogManager.GetLogger("SAP_Distribution");

        public static object lockCreateSAPScheduleLineFromLes = new object();
        [Transaction(TransactionMode.Requires)]
        public void CreateOneCRSL(com.Sconit.Entity.SAP.ORD.CreateScheduleLine createScheduleLine, IList<ErrorMessage> errorMessageList)
        {
            lock (lockCreateSAPScheduleLineFromLes)
            {
                MI_CRSL_LES.MI_CRSL_LESService serviceProxy = new MI_CRSL_LES.MI_CRSL_LESService();
                serviceProxy.Credentials = base.Credentials;
                serviceProxy.Timeout = base.TimeOut;
                serviceProxy.Url = ReplaceSAPServiceUrl(serviceProxy.Url);

                MI_CRSL_LES.ZLSCHE_IN zlscheIn = new MI_CRSL_LES.ZLSCHE_IN();
                zlscheIn.EINDT = createScheduleLine.EINDT;
                zlscheIn.FRBNR = createScheduleLine.FRBNR;
                zlscheIn.LIFNR = createScheduleLine.LIFNR;
                zlscheIn.MATNR = createScheduleLine.MATNR;
                zlscheIn.MENGE = createScheduleLine.MENGE;
                zlscheIn.SGTXT = createScheduleLine.SGTXT;
                zlscheIn.WERKS = createScheduleLine.WERKS;
                zlscheIn.MENGESpecified = true;
                var zlscheOut = serviceProxy.MI_CRSL_LES(zlscheIn);
                if (zlscheOut.STATUS == "S")
                {
                    //成功
                    createScheduleLine.EBELN = zlscheOut.EBELN;
                    createScheduleLine.EBELP = zlscheOut.EBELP;
                    createScheduleLine.ETENR = zlscheOut.ETENR;
                    createScheduleLine.MESSAGE = zlscheOut.MESSAGE;
                    createScheduleLine.Status = Entity.SAP.StatusEnum.Success;
                    //根据消息判断是否是寄售
                    if (!string.IsNullOrEmpty(zlscheOut.MESSAGE))
                    {
                        createScheduleLine.PSTYP = zlscheOut.MESSAGE;
                    }
                    this.genericMgr.Update(createScheduleLine);

                    OrderDetail orderDetail = this.genericMgr.FindById<OrderDetail>(createScheduleLine.OrderDetId);
                    orderDetail.ExternalOrderNo = zlscheOut.EBELN;
                    orderDetail.ExternalSequence = zlscheOut.EBELP;
                    if (!string.IsNullOrEmpty(zlscheOut.MESSAGE))
                    {
                        orderDetail.BillTerm = CodeMaster.OrderBillTerm.OnlineBilling;
                    }
                    else
                    {
                        orderDetail.BillTerm = CodeMaster.OrderBillTerm.ReceivingSettlement;
                    }
                    this.genericMgr.Update(orderDetail);
                }
                else
                {
                    createScheduleLine.ErrorCount = createScheduleLine.ErrorCount + 1;
                    createScheduleLine.Status = Entity.SAP.StatusEnum.Fail;
                    createScheduleLine.MESSAGE = zlscheOut.MESSAGE;
                    this.genericMgr.Update(createScheduleLine);
                    OrderDetail orderDetail = this.genericMgr.FindById<OrderDetail>(createScheduleLine.OrderDetId);
                    orderDetail.BillAddressDescription = zlscheOut.MESSAGE;
                    //orderDetail.ExternalSequence = zlscheOut.EBELP;
                    this.genericMgr.Update(orderDetail);
                    //失败
                    string logMessage = "SAP反写计划协议失败，失败信息：" + zlscheOut.MESSAGE;
                    log.Error(logMessage);

                    if (createScheduleLine.ErrorCount == 10)
                    {
                        errorMessageList.Add(new ErrorMessage
                        {
                            Template = NVelocityTemplateRepository.TemplateEnum.GenerateSAPScheduleLine,
                            Message = logMessage
                        });
                    }
                }
            }
        }
    }
}
