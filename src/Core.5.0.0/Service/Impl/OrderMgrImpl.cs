using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web.Services.Protocols;
using AutoMapper;
using Castle.Services.Transaction;
using com.Sconit.Entity;
using com.Sconit.Entity.ACC;
using com.Sconit.Entity.BIL;
using com.Sconit.Entity.CUST;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.FIS;
using com.Sconit.Entity.INP;
using com.Sconit.Entity.INV;
using com.Sconit.Entity.LOG;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.PRD;
using com.Sconit.Entity.SAP;
using com.Sconit.Entity.SCM;
using com.Sconit.Entity.SYS;
using com.Sconit.Entity.VIEW;
using com.Sconit.PrintModel.ORD;
using com.Sconit.Utility;
using NHibernate;
using NHibernate.Type;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace com.Sconit.Service.Impl
{
    [Transactional]
    public class OrderMgrImpl : BaseMgr, IOrderMgr
    {
        #region 变量
        private static log4net.ILog log = log4net.LogManager.GetLogger("Log.OrderMaster");
        private IPublishing proxy;
        public IPubSubMgr pubSubMgr { get; set; }
        public IGenericMgr genericMgr { get; set; }
        public INumberControlMgr numberControlMgr { get; set; }
        public IItemMgr itemMgr { get; set; }
        public IBomMgr bomMgr { get; set; }
        public IRoutingMgr routingMgr { get; set; }
        public IFlowMgr flowMgr { get; set; }
        public ISystemMgr systemMgr { get; set; }
        public IIpMgr ipMgr { get; set; }
        public IReceiptMgr receiptMgr { get; set; }
        public ILocationDetailMgr locationDetailMgr { get; set; }
        public IProductionLineMgr productionLineMgr { get; set; }
        public IPickListMgr pickListMgr { get; set; }
        public IHuMgr huMgr { get; set; }
        public IEmailMgr emailMgr { get; set; }
        public IWorkingCalendarMgr workingCalendarMgr { get; set; }
        public IPickTaskMgr pickTaskMgr { get; set; }
        public NVelocityTemplateRepository vmReporsitory { get; set; }
        #endregion

        #region Delegate
        public delegate void OrderReleasedHandler(OrderMaster orderMaster);
        #endregion

        #region Event
        public event OrderReleasedHandler OrderReleased;
        #endregion

        #region public methods
        #region 路线转订单

        public OrderMaster TransferFlow2Order(FlowMaster flowMaster, bool isTransferDetail)
        {
            return TransferFlow2Order(flowMaster, null, DateTime.Now, isTransferDetail);
        }

        public OrderMaster TransferFlow2Order(FlowMaster flowMaster, IList<string> itemCodeList)
        {
            return TransferFlow2Order(flowMaster, itemCodeList, DateTime.Now);
        }

        public OrderMaster TransferFlow2Order(FlowMaster flowMaster, IList<string> itemCodeList, DateTime effectivedate)
        {
            return TransferFlow2Order(flowMaster, itemCodeList, effectivedate, true);
        }

        public OrderMaster TransferFlow2Order(FlowMaster flowMaster, IList<string> itemCodeList, DateTime effectivedate, bool isTransferDetail)
        {
            OrderMaster orderMaster = Mapper.Map<FlowMaster, OrderMaster>(flowMaster);

            if (isTransferDetail && (flowMaster.FlowDetails == null || flowMaster.FlowDetails.Count == 0))
            {
                TryLoadFlowDetails(flowMaster, itemCodeList);
            }

            #region 查找OrderMaster相关对象
            #region 查找Party
            string selectPartyStatement = "from Party where Code in (?";
            IList<object> selectPartyPara = new List<object>();
            selectPartyPara.Add(flowMaster.PartyFrom);
            if (flowMaster.PartyFrom != flowMaster.PartyTo)
            {
                selectPartyStatement += ", ?";
                selectPartyPara.Add(flowMaster.PartyTo);
            }
            selectPartyStatement += ")";
            IList<Party> partyList = this.genericMgr.FindAll<Party>(selectPartyStatement, selectPartyPara.ToArray());
            #endregion

            #region 查找Address
            #region 收集所有地址代码
            IList<string> addressCodeList = new List<string>();

            if (!string.IsNullOrEmpty(flowMaster.BillAddress))
            {
                addressCodeList.Add(flowMaster.BillAddress);
            }

            if (!string.IsNullOrEmpty(flowMaster.ShipFrom))
            {
                addressCodeList.Add(flowMaster.ShipFrom);
            }

            if (!string.IsNullOrEmpty(flowMaster.ShipTo))
            {
                addressCodeList.Add(flowMaster.ShipTo);
            }

            if (flowMaster.FlowDetails != null)
            {
                foreach (string billAddress in flowMaster.FlowDetails.Where(d => !string.IsNullOrEmpty(d.BillAddress)).Select(d => d.BillAddress).Distinct())
                {
                    addressCodeList.Add(billAddress);
                }
            }
            #endregion

            #region 查找Address
            IList<Address> addressList = null;
            if (addressCodeList.Count > 0)
            {
                string selectAddressStatement = string.Empty;
                IList<object> selectAddressPara = new List<object>();
                foreach (string addressCode in addressCodeList.Distinct())
                {
                    if (selectAddressStatement == string.Empty)
                    {
                        selectAddressStatement = "from Address where Code in (?";
                    }
                    else
                    {
                        selectAddressStatement += ",?";
                    }
                    selectAddressPara.Add(addressCode);
                }
                selectAddressStatement += ")";
                addressList = this.genericMgr.FindAll<Address>(selectAddressStatement, selectAddressPara.ToArray());
            }
            #endregion
            #endregion

            #region 查找Location
            #region 收集所有Location代码
            IList<string> locationCodeList = new List<string>();

            if (!string.IsNullOrEmpty(flowMaster.LocationFrom))
            {
                locationCodeList.Add(flowMaster.LocationFrom);
            }

            if (!string.IsNullOrEmpty(flowMaster.LocationTo))
            {
                locationCodeList.Add(flowMaster.LocationTo);
            }

            if (flowMaster.FlowDetails != null)
            {
                foreach (string locationFromCode in flowMaster.FlowDetails.Where(d => !string.IsNullOrEmpty(d.LocationFrom)).Select(d => d.LocationFrom).Distinct())
                {
                    locationCodeList.Add(locationFromCode);
                }

                foreach (string locationToCode in flowMaster.FlowDetails.Where(d => !string.IsNullOrEmpty(d.LocationTo)).Select(d => d.LocationTo).Distinct())
                {
                    locationCodeList.Add(locationToCode);
                }
            }
            #endregion

            #region 查找Location
            IList<Location> locationList = null;
            if (locationCodeList.Count > 0)
            {
                string selectLocationStatement = string.Empty;
                IList<object> selectLocationPara = new List<object>();
                foreach (string locationCode in locationCodeList.Distinct())
                {
                    if (selectLocationStatement == string.Empty)
                    {
                        selectLocationStatement = "from Location where Code in (?";
                    }
                    else
                    {
                        selectLocationStatement += ",?";
                    }
                    selectLocationPara.Add(locationCode);
                }
                selectLocationStatement += ")";
                locationList = this.genericMgr.FindAll<Location>(selectLocationStatement, selectLocationPara.ToArray());
            }
            #endregion
            #endregion

            #region 查找PriceList
            #region 收集所有PriceList代码
            IList<string> priceListCodeList = new List<string>();

            if (!string.IsNullOrEmpty(flowMaster.PriceList))
            {
                priceListCodeList.Add(flowMaster.PriceList);
            }

            if (flowMaster.FlowDetails != null)
            {
                foreach (string priceListCode in flowMaster.FlowDetails.Where(d => !string.IsNullOrEmpty(d.PriceList)).Select(d => d.PriceList).Distinct())
                {
                    priceListCodeList.Add(priceListCode);
                }
            }
            #endregion

            #region 查找PriceList
            IList<PriceListMaster> priceListList = null;
            if (priceListCodeList.Count > 0)
            {
                string selectPriceListStatement = string.Empty;
                IList<object> selectPriceListPara = new List<object>();
                foreach (string priceListCode in priceListCodeList.Distinct())
                {
                    if (selectPriceListStatement == string.Empty)
                    {
                        selectPriceListStatement = "from PriceListMaster where Code in (?";
                    }
                    else
                    {
                        selectPriceListStatement += ",?";
                    }
                    selectPriceListPara.Add(priceListCode);
                }
                selectPriceListStatement += ")";
                priceListList = this.genericMgr.FindAll<PriceListMaster>(selectPriceListStatement, selectPriceListPara.ToArray());
            }
            #endregion
            #endregion
            #endregion

            #region OrderMaster赋默认值

            #region PartyFrom和PartyTo赋值
            orderMaster.PartyFromName = partyList.Where(p => p.Code == orderMaster.PartyFrom).Single().Name;
            orderMaster.PartyToName = partyList.Where(p => p.Code == orderMaster.PartyTo).Single().Name;
            #endregion

            #region BillAddress赋值
            if (!string.IsNullOrEmpty(flowMaster.BillAddress))
            {
                orderMaster.BillAddressDescription = addressList.Where(a => a.Code == flowMaster.BillAddress).Single().AddressContent;
            }
            #endregion

            #region ShipFrom赋值
            if (!string.IsNullOrEmpty(flowMaster.ShipFrom))
            {
                Address shipFrom = addressList.Where(a => a.Code == flowMaster.ShipFrom).Single();
                orderMaster.ShipFromAddress = shipFrom.AddressContent;
                orderMaster.ShipFromCell = shipFrom.MobilePhone;
                orderMaster.ShipFromTel = shipFrom.TelPhone;
                orderMaster.ShipFromFax = shipFrom.Fax;
                orderMaster.ShipFromContact = shipFrom.ContactPersonName;
            }
            #endregion

            #region ShipTo赋值
            if (!string.IsNullOrEmpty(flowMaster.ShipTo))
            {
                Address shipTo = addressList.Where(a => a.Code == flowMaster.ShipTo).Single();
                orderMaster.ShipToAddress = shipTo.AddressContent;
                orderMaster.ShipToCell = shipTo.MobilePhone;
                orderMaster.ShipToTel = shipTo.TelPhone;
                orderMaster.ShipToFax = shipTo.Fax;
                orderMaster.ShipToContact = shipTo.ContactPersonName;
            }
            #endregion

            #region LocationFrom赋值
            if (!string.IsNullOrWhiteSpace(flowMaster.LocationFrom))
            {
                //   orderMaster.LocationFromName = locationList.Where(a => a.Code == flowMaster.LocationFrom).Single().Name;
                orderMaster.LocationFromName = genericMgr.FindById<Location>(flowMaster.LocationFrom).Name;
            }
            #endregion

            #region LocationTo赋值
            if (!string.IsNullOrWhiteSpace(flowMaster.LocationTo))
            {
                // orderMaster.LocationToName = locationList.Where(a => a.Code == flowMaster.LocationTo).Single().Name;
                orderMaster.LocationToName = genericMgr.FindById<Location>(flowMaster.LocationTo).Name;
            }
            #endregion

            #region PriceList赋值
            if (!string.IsNullOrWhiteSpace(flowMaster.PriceList))
            {
                orderMaster.Currency = priceListList.Where(a => a.Code == flowMaster.PriceList).Single().Currency;
            }
            #endregion

            orderMaster.EffectiveDate = effectivedate;
            orderMaster.SubType = com.Sconit.CodeMaster.OrderSubType.Normal;
            orderMaster.QualityType = com.Sconit.CodeMaster.QualityType.Qualified;
            orderMaster.Priority = com.Sconit.CodeMaster.OrderPriority.Normal;
            orderMaster.Status = com.Sconit.CodeMaster.OrderStatus.Create;
            orderMaster.OrderStrategy = flowMaster.FlowStrategy;
            #endregion

            #region OrderDetail

            if (isTransferDetail && flowMaster.FlowDetails != null && flowMaster.FlowDetails.Count > 0)
            {
                IList<Item> itemList = this.itemMgr.GetItems(flowMaster.FlowDetails.Select(det => det.Item).Distinct().ToList());

                int seq = 1;
                foreach (FlowDetail flowDetail in flowMaster.FlowDetails.OrderBy(d => d.Sequence))
                {
                    OrderDetail orderDetail = Mapper.Map<FlowDetail, OrderDetail>(flowDetail);

                    if (flowDetail.ExternalSequence == 0)
                    {
                        orderDetail.Sequence = seq++; //重新记录顺序
                    }
                    else
                    {
                        orderDetail.Sequence = flowDetail.ExternalSequence;
                    }

                    //物料描述
                    orderDetail.ItemDescription = itemList.Where(a => a.Code == flowDetail.Item).Single().Description;

                    if (flowDetail.BindDemand != null)
                    {
                        //订单绑定取被绑定订单相关字段
                        orderDetail.ManufactureParty = flowDetail.BindDemand.ManufactureParty;
                        orderDetail.QualityType = flowDetail.BindDemand.QualityType;
                    }
                    else
                    {
                        orderDetail.QualityType = CodeMaster.QualityType.Qualified;
                    }
                    //orderDetail.UnitQty =  创建订单时会计算

                    if (!string.IsNullOrWhiteSpace(flowDetail.LocationFrom))
                    {
                        orderDetail.LocationFromName = locationList.Where(a => a.Code == flowDetail.LocationFrom).Single().Name;
                        //orderDetail.LocationFromName = genericMgr.FindById<Location>(flowDetail.LocationFrom).Name;
                    }

                    if (!string.IsNullOrWhiteSpace(flowDetail.LocationTo))
                    {
                        orderDetail.LocationToName = locationList.Where(a => a.Code == flowDetail.LocationTo).Single().Name;
                        //orderDetail.LocationToName = genericMgr.FindById<Location>(flowDetail.LocationTo).Name;
                    }

                    if (!string.IsNullOrWhiteSpace(flowDetail.BillAddress))
                    {
                        orderDetail.BillAddressDescription = addressList.Where(a => a.Code == flowDetail.BillAddress).Single().AddressContent;
                    }

                    //if (orderMaster.Type == com.Sconit.CodeMaster.OrderType.Procurement
                    //   || orderMaster.Type == com.Sconit.CodeMaster.OrderType.ScheduleLine
                    //   || orderMaster.Type == com.Sconit.CodeMaster.OrderType.Distribution
                    //   || orderMaster.Type == com.Sconit.CodeMaster.OrderType.SubContract)
                    //{
                    //    //计算价格
                    //    this.CalculateOrderDetailPrice(orderDetail, orderMaster, effectivedate);
                    //}

                    orderDetail.RequiredQty = flowDetail.OrderQty;
                    orderDetail.OrderedQty = flowDetail.OrderQty;

                    orderMaster.AddOrderDetail(orderDetail);
                }
            }

            #endregion

            #region OrderBinding
            TryLoadFlowBindings(flowMaster);
            if (flowMaster.FlowBindings != null && flowMaster.FlowBindings.Count > 0)
            {
                orderMaster.OrderBindings = (from b in flowMaster.FlowBindings
                                             select new OrderBinding
                                             {
                                                 BindFlow = b.BindedFlow.Code,
                                                 BindFlowStrategy = b.BindedFlow.FlowStrategy,
                                                 BindType = b.BindType,
                                             }).ToList();
            }
            #endregion

            return orderMaster;
        }
        #endregion

        #region 订单新增/修改操作
        [Transaction(TransactionMode.Requires)]
        public void CreateOrder(OrderMaster orderMaster)
        {
            CreateOrder(orderMaster, true);
        }

        [Transaction(TransactionMode.Requires)]
        public void CreateOrder(OrderMaster orderMaster, bool expandOrderBomDetail)
        {
            #region 检查
            if (orderMaster.OrderDetails != null && orderMaster.OrderDetails.Count() > 0)
            {
                //按seq排序
                orderMaster.OrderDetails = orderMaster.OrderDetails.OrderBy(det => det.Sequence).ToList();
                int seq = 0; //新的序号
                IList<OrderDetail> activeOrderDetails = new List<OrderDetail>();
                IList<Item> itemList = this.itemMgr.GetItems(orderMaster.OrderDetails.Select(det => det.Item).Distinct().ToList());

                foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                {
                    orderDetail.CurrentItem = itemList.Where(a => a.Code == orderDetail.Item).Single();
                    ((List<OrderDetail>)activeOrderDetails).AddRange(ProcessNewOrderDetail(orderDetail, orderMaster, ref seq));
                }

                orderMaster.OrderDetails = activeOrderDetails;
            }

            #region 生产线暂停不允许创建生产单
            if ((orderMaster.Type == CodeMaster.OrderType.Production
                || orderMaster.Type == CodeMaster.OrderType.SubContract) && !string.IsNullOrWhiteSpace(orderMaster.Flow))
            {
                FlowMaster flowMaster = this.genericMgr.FindById<FlowMaster>(orderMaster.Flow);
                if (flowMaster.IsPause)
                {
                    throw new BusinessException("生产线{0}已经暂停，不能创建生产单。", orderMaster.Flow);
                }
            }
            #endregion

            //if (orderMaster.OrderDetails == null || orderMaster.OrderDetails.Count() == 0)
            //{
            //    throw new BusinessErrorException(Resources.ORD.OrderMaster.Errors_OrderDetailEmpty);
            //}
            #endregion

            #region 计算价格
            //if (orderMaster.Type == com.Sconit.CodeMaster.OrderType.Procurement
            //    || orderMaster.Type == com.Sconit.CodeMaster.OrderType.ScheduleLine
            //    || orderMaster.Type == com.Sconit.CodeMaster.OrderType.Distribution
            //    || orderMaster.Type == com.Sconit.CodeMaster.OrderType.SubContract)
            //{
            //    foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
            //    {
            //        //没有指定价格才需要计价
            //        if (!orderDetail.UnitPrice.HasValue)
            //        {
            //            CalculateOrderDetailPrice(orderDetail, orderMaster, orderMaster.EffectiveDate);
            //        }
            //    }
            //}
            #endregion

            #region 循环OrderDetail
            if (orderMaster.OrderDetails != null && orderMaster.OrderDetails.Count > 0)
            {
                foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                {
                    #region OrderDetail来源库位设定  //订单释放的时候在设置
                    //if (string.IsNullOrEmpty(orderDetail.LocationFrom)
                    //    && orderMaster.Type != com.Sconit.CodeMaster.OrderType.Procurement)
                    //{
                    //    orderDetail.LocationFrom = orderMaster.LocationFrom;
                    //    orderDetail.LocationFromName = orderMaster.LocationFromName;
                    //}
                    #endregion

                    #region OrderDetail目的库位设定 //订单释放的时候在设置
                    //if (string.IsNullOrEmpty(orderDetail.LocationTo)
                    //    && orderMaster.Type != com.Sconit.CodeMaster.OrderType.Distribution)
                    //{
                    //    orderDetail.LocationTo = orderMaster.LocationTo;
                    //    orderDetail.LocationToName = orderMaster.LocationToName;
                    //}
                    #endregion

                    #region 生成OrderOperation和OrderBomDetail
                    if (orderMaster.Type == com.Sconit.CodeMaster.OrderType.Production
                        || orderMaster.Type == com.Sconit.CodeMaster.OrderType.SubContract)
                    {
                        if (orderDetail.OrderOperations == null || orderDetail.OrderOperations.Count == 0)
                        {
                            GenerateOrderOperation(orderDetail, orderMaster);
                        }

                        if (expandOrderBomDetail)
                        {
                            if (orderDetail.OrderBomDetails == null || orderDetail.OrderBomDetails.Count == 0)
                            {
                                GenerateOrderBomDetail(orderDetail, orderMaster);
                            }
                        }
                    }
                    #endregion
                }
            }
            #endregion

            #region 生成OrderBinding
            if (!string.IsNullOrWhiteSpace(orderMaster.Flow))
            {
                IList<FlowBinding> flowBindingList = flowMgr.GetFlowBinding(orderMaster.Flow);

                if (flowBindingList != null && flowBindingList.Count() > 0)
                {
                    IList<OrderBinding> orderBindingList = (from binding in flowBindingList
                                                            select new OrderBinding
                                                            {
                                                                BindFlow = binding.BindedFlow.Code,
                                                                BindFlowStrategy = binding.BindedFlow.FlowStrategy,
                                                                BindType = binding.BindType,
                                                            }).ToList();

                    orderMaster.OrderBindings = orderBindingList;
                }
            }
            #endregion

            #region 创建OrderHead
            if (string.IsNullOrWhiteSpace(orderMaster.OrderNo))
            {
                orderMaster.OrderNo = numberControlMgr.GetOrderNo(orderMaster);
            }
            if (orderMaster.IsQuick)
            {
                if (orderMaster.WindowTime == DateTime.MinValue)
                {
                    orderMaster.WindowTime = DateTime.Now;
                }
                if (orderMaster.StartTime == DateTime.MinValue)
                {
                    orderMaster.StartTime = DateTime.Now;
                }
            }
            //orderMaster.OrderNo = numberControlMgr.GenerateOrderNo(orderMaster);
            orderMaster.Status = com.Sconit.CodeMaster.OrderStatus.Create;
            genericMgr.Create(orderMaster);

            #endregion

            #region 创建OrderBinding
            if (orderMaster.SubType == CodeMaster.OrderSubType.Normal  //退货没有绑定
                && orderMaster.OrderBindings != null && orderMaster.OrderBindings.Count() > 0)
            {
                foreach (OrderBinding orderBinding in orderMaster.OrderBindings)
                {
                    orderBinding.OrderNo = orderMaster.OrderNo;
                    genericMgr.Create(orderBinding);
                }
            }
            #endregion

            #region 创建OrderDetail
            if (orderMaster.OrderDetails != null && orderMaster.OrderDetails.Count > 0)
            {
                foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                {
                    orderDetail.OrderNo = orderMaster.OrderNo;
                    orderDetail.QualityType = orderMaster.QualityType;
                    genericMgr.Create(orderDetail);
                    ProcessAddOrderOperations(orderDetail, orderDetail.OrderOperations);
                    ProcessAddOrderBomDetails(orderDetail, orderDetail.OrderBomDetails);
                }
            }
            #endregion

            //#region 发布到订阅端
            //try
            //{
            //    PrintOrderMaster printOrderMstr = Mapper.Map<OrderMaster, PrintOrderMaster>(orderMaster);
            //    IList<PrintOrderDetail> printOrderDetails = Mapper.Map<IList<OrderDetail>, IList<PrintOrderDetail>>(orderMaster.OrderDetails);
            //    printOrderMstr.OrderDetails = printOrderDetails;
            //    proxy = pubSubMgr.CreateProxy();
            //    proxy.Publish(printOrderMstr);
            //}
            //catch (Exception)
            //{
            //    //todo: 记录日志
            //    //throw ex;
            //}
            //#endregion          

            #region 自动Release
            if (orderMaster.IsAutoRelease || orderMaster.IsQuick)
            {
                ReleaseOrder(orderMaster);
            }
            #endregion
        }

        [Transaction(TransactionMode.Requires)]
        public void UpdateOrder(OrderMaster orderMaster)
        {
            if (orderMaster.OrderNo == null)
            {
                throw new TechnicalException("OrderNo not specified for OrderMaster.");
            }

            if (orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.Create)
            {
                this.genericMgr.Update(orderMaster);
            }
            else
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_StatusErrorWhenModify, orderMaster.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
            }
        }

        [Transaction(TransactionMode.Requires)]
        public void DeleteOrder(string orderNo)
        {
            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderNo);
            //销售订单submit状态的也可以删除
            if ((orderMaster.Type != CodeMaster.OrderType.Distribution && orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.Create)
                || (orderMaster.Type == CodeMaster.OrderType.Distribution && (orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.Create || orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.Submit)))
            {
                if (orderMaster.Type == com.Sconit.CodeMaster.OrderType.Production
                    || orderMaster.Type == com.Sconit.CodeMaster.OrderType.SubContract)
                {
                    IList<OrderBomDetail> orderBomDetailList = TryLoadOrderBomDetails(orderMaster);
                    if (orderBomDetailList != null && orderBomDetailList.Count > 0)
                    {
                        this.genericMgr.Delete<OrderBomDetail>(orderBomDetailList);
                    }

                    IList<OrderOperation> orderOperationList = TryLoadOrderOperations(orderMaster);
                    if (orderOperationList != null && orderOperationList.Count > 0)
                    {
                        this.genericMgr.Delete<OrderOperation>(orderOperationList);
                    }
                }

                IList<OrderBinding> orderBindingList = TryLoadOrderBindings(orderMaster);
                if (orderBindingList != null && orderBindingList.Count > 0)
                {
                    this.genericMgr.Delete<OrderBinding>(orderBindingList);
                }

                IList<OrderDetail> orderDetailList = TryLoadOrderDetails(orderMaster);
                if (orderDetailList != null && orderDetailList.Count > 0)
                {
                    this.genericMgr.Delete<OrderDetail>(orderDetailList);
                }

                this.genericMgr.Delete(orderMaster);
            }
            else
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_StatusErrorWhenDelete, orderMaster.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
            }
        }
        #endregion

        #region 订单明细操作
        #region 批量更新订单明细
        [Transaction(TransactionMode.Requires)]
        public IList<OrderDetail> BatchUpdateOrderDetails(string orderNo, IList<OrderDetail> addOrderDetailList, IList<OrderDetail> updateOrderDetailList, IList<OrderDetail> deleteOrderDetailList)
        {
            if (string.IsNullOrWhiteSpace(orderNo))
            {
                throw new ArgumentNullException("OrderNo not specified.");
            }

            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderNo);
            BeforeBatchUpdateOrderDetails(orderMaster);
            TryLoadOrderDetails(orderMaster);
            IList<OrderDetail> resultOrderDetailList = new List<OrderDetail>();
            if (addOrderDetailList != null && addOrderDetailList.Count > 0)
            {
                ((List<OrderDetail>)resultOrderDetailList).AddRange(ProcessAddOrderDetails(orderMaster, addOrderDetailList));
            }

            if (updateOrderDetailList != null && updateOrderDetailList.Count > 0)
            {
                ((List<OrderDetail>)resultOrderDetailList).AddRange(ProcessUpdateOrderDetails(orderMaster, updateOrderDetailList));
            }

            if (deleteOrderDetailList != null && deleteOrderDetailList.Count > 0)
            {
                IList<int> orderBindingIds = (from orderDetail in deleteOrderDetailList
                                              select orderDetail.Id).ToList();
                ProcessDeleteOrderDetails(orderMaster, orderBindingIds);
            }

            return resultOrderDetailList;
        }

        private void BeforeBatchUpdateOrderDetails(OrderMaster orderMaster)
        {
            BeforeUpdateOrderDetails(orderMaster);
        }
        #endregion

        #region 添加订单明细
        [Transaction(TransactionMode.Requires)]
        public IList<OrderDetail> AddOrderDetails(string orderNo, IList<OrderDetail> orderDetailList)
        {
            if (string.IsNullOrWhiteSpace(orderNo))
            {
                throw new ArgumentNullException("OrderNo not specified.");
            }

            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderNo);

            BeforeAddOrderDetails(orderMaster);
            return ProcessAddOrderDetails(orderMaster, orderDetailList);
        }

        private void BeforeAddOrderDetails(OrderMaster orderMaster)
        {
            if (orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Create)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_StatusErrorWhenAddOrderDetail, orderMaster.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
            }
        }

        private IList<OrderDetail> ProcessAddOrderDetails(OrderMaster orderMaster, IList<OrderDetail> orderDetailList)
        {
            #region 获取最大订单明细序号
            string hql = "select max(Sequence) as seq from OrderDetail where OrderNo = ?";
            IList<int?> maxSeqList = genericMgr.FindAll<int?>(hql, orderMaster.OrderNo);
            int maxSeq = maxSeqList[0] != null ? maxSeqList[0].Value : 0;
            #endregion

            IList<OrderDetail> returnOrderDetailList = new List<OrderDetail>();
            IList<Item> itemList = new List<Item>();
            if ((orderMaster.OrderDetails != null && orderMaster.OrderDetails.Count > 0) || (orderDetailList != null && orderDetailList.Count > 0))
            {
                //新增的订单明细的Item要加进来
                itemList = this.itemMgr.GetItems(orderMaster.OrderDetails.Union(orderDetailList).Select(det => det.Item).Distinct().ToList());

                foreach (OrderDetail orderDetail in orderDetailList)
                {
                    #region 处理订单明细
                    //新增的话物料不在itemList中，重新查一把吧
                    var item = itemList.Where(a => a.Code == orderDetail.Item).FirstOrDefault();
                    orderDetail.CurrentItem = itemList.Where(a => a.Code == orderDetail.Item).Single();
                    IList<OrderDetail> newOrderDetailList = ProcessNewOrderDetail(orderDetail, orderMaster, ref maxSeq);
                    #endregion

                    #region 计算价格
                    //if (orderMaster.Type == com.Sconit.CodeMaster.OrderType.Procurement
                    //    || orderMaster.Type == com.Sconit.CodeMaster.OrderType.ScheduleLine
                    //    || orderMaster.Type == com.Sconit.CodeMaster.OrderType.Distribution
                    //    || orderMaster.Type == com.Sconit.CodeMaster.OrderType.SubContract)
                    //{
                    //    foreach (OrderDetail newOrderDetail in newOrderDetailList)
                    //    {
                    //        //没有指定价格才需要计价
                    //        if (!newOrderDetail.UnitPrice.HasValue || string.IsNullOrWhiteSpace(newOrderDetail.Currency))
                    //        {
                    //            CalculateOrderDetailPrice(newOrderDetail, orderMaster, orderMaster.EffectiveDate);
                    //        }
                    //    }
                    //}
                    #endregion

                    #region 生成OrderOperation和OrderBomDetail
                    if (orderMaster.Type == com.Sconit.CodeMaster.OrderType.Production
                        || orderMaster.Type == com.Sconit.CodeMaster.OrderType.SubContract)
                    {
                        foreach (OrderDetail newOrderDetail in newOrderDetailList)
                        {
                            GenerateOrderOperation(newOrderDetail, orderMaster);
                            GenerateOrderBomDetail(newOrderDetail, orderMaster);
                        }
                    }
                    #endregion

                    #region 创建OrderDetail
                    foreach (OrderDetail newOrderDetail in newOrderDetailList)
                    {
                        newOrderDetail.OrderNo = orderMaster.OrderNo;
                        newOrderDetail.QualityType = orderMaster.QualityType;
                        genericMgr.Create(newOrderDetail);
                        ProcessAddOrderOperations(orderDetail, orderDetail.OrderOperations);
                        ProcessAddOrderBomDetails(newOrderDetail, newOrderDetail.OrderBomDetails);
                    }
                    #endregion

                    ((List<OrderDetail>)returnOrderDetailList).AddRange(newOrderDetailList);
                }
            }

            return returnOrderDetailList;
        }
        #endregion

        #region 修改订单明细
        [Transaction(TransactionMode.Requires)]
        public IList<OrderDetail> UpdateOrderDetails(IList<OrderDetail> orderDetailList)
        {
            if (orderDetailList[0].OrderNo == null)
            {
                throw new TechnicalException("OrderNo not specified for OrderDetail.");
            }

            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderDetailList[0].OrderNo);
            BeforeUpdateOrderDetails(orderMaster);
            return ProcessUpdateOrderDetails(orderMaster, orderDetailList);
        }

        #region 轮胎分装发货更新订单明细
        [Transaction(TransactionMode.Requires)]
        public IList<OrderDetail> UpdateTireOrderDetails(string ordreNo, IList<OrderDetail> orderDetailList)
        {
            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(ordreNo);
            if (orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Submit)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_StatusErrorWhenModifyOrderDetail, orderMaster.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, int.Parse(((int)orderMaster.Status).ToString())));
            }
            return ProcessUpdateOrderDetails(orderMaster, orderDetailList);
        }
        #endregion

        private void BeforeUpdateOrderDetails(OrderMaster orderMaster)
        {
            if (orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Create)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_StatusErrorWhenModifyOrderDetail, orderMaster.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, int.Parse(((int)orderMaster.Status).ToString())));
            }
        }

        private IList<OrderDetail> ProcessUpdateOrderDetails(OrderMaster orderMaster, IList<OrderDetail> orderDetailList)
        {
            IList<OrderDetail> returnOrderDetailList = new List<OrderDetail>();
            IList<OrderDetail> destinationOrderDetailList = this.LoadOrderDetails(orderDetailList.Select(det => det.Id).ToArray());
            IList<Item> itemList = this.itemMgr.GetItems(destinationOrderDetailList.Select(det => det.Item).Distinct().ToList());

            foreach (OrderDetail orderDetail in orderDetailList)
            {
                OrderDetail destinationOrderDetail = destinationOrderDetailList.Where(det => det.Id == orderDetail.Id).Single();

                //最好不要支持修改零件，如果改为Kit就不好办了
                string oldDetItem = destinationOrderDetail.Item;
                string oldDetUom = destinationOrderDetail.Uom;
                string oldDetRouting = destinationOrderDetail.Routing;
                string oldDetBom = destinationOrderDetail.Bom;
                //string oldProductionScan = destinationOrderDetail.ProductionScan;
                decimal oldDetOrderedQty = destinationOrderDetail.OrderedQty;

                Mapper.Map<OrderDetail, OrderDetail>(orderDetail, destinationOrderDetail);

                #region 整包校验
                CheckOrderedQtyFulfillment(orderMaster, destinationOrderDetail);
                #endregion

                #region 单位改变或者零件改变，重新生成UnitQty
                if (string.Compare(oldDetUom, destinationOrderDetail.Uom) != 0 || string.Compare(oldDetItem, destinationOrderDetail.Item) != 0)
                {
                    Item item = itemList.Where(a => a.Code == orderDetail.Item).Single();
                    if (destinationOrderDetail.Uom != item.Uom)
                    {
                        destinationOrderDetail.UnitQty = itemMgr.ConvertItemUomQty(destinationOrderDetail.Item, item.Uom, 1, destinationOrderDetail.Uom);
                    }
                    else
                    {
                        destinationOrderDetail.UnitQty = 1;
                    }
                }
                #endregion

                #region 判断是否需要重新生成OrderOperation和OrderBomDetail
                if (orderMaster.Type == com.Sconit.CodeMaster.OrderType.Production
                    || orderMaster.Type == com.Sconit.CodeMaster.OrderType.SubContract)
                {
                    //明细的Routing改变了，或者零件改变了
                    //重新计算OrderOperation
                    if (string.Compare(oldDetItem, destinationOrderDetail.Item) != 0
                        || string.Compare(oldDetRouting, destinationOrderDetail.Routing) != 0)
                    {
                        ExpandOrderOperation(orderMaster, destinationOrderDetail);
                    }

                    //明细的Bom改变了，或者订单单位改变了，或者明细的Routing改变了，，或者零件改变了, //或者明细的防错扫描改变了
                    //重新计算OrderBomDetail
                    if (string.Compare(oldDetItem, destinationOrderDetail.Item) != 0
                        || string.Compare(oldDetBom, destinationOrderDetail.Bom) != 0
                        || string.Compare(oldDetUom, destinationOrderDetail.Uom) != 0
                        || string.Compare(oldDetRouting, destinationOrderDetail.Routing) != 0
                        //|| destinationOrderDetail.ProductionScan != oldProductionScan
                        )
                    {
                        ExpandOrderBomDetail(orderMaster, destinationOrderDetail);
                    }
                    else if (destinationOrderDetail.OrderedQty != oldDetOrderedQty)
                    {
                        //如果订单量发生变化，需要重新计算Bom用量
                        TryLoadOrderBomDetails(destinationOrderDetail);
                        foreach (OrderBomDetail orderBomDetail in destinationOrderDetail.OrderBomDetails)
                        {
                            orderBomDetail.OrderedQty = destinationOrderDetail.OrderedQty * orderBomDetail.BomUnitQty;
                            genericMgr.Update(orderBomDetail);
                        }
                    }
                }
                #endregion

                this.genericMgr.Update(destinationOrderDetail);

                returnOrderDetailList.Add(destinationOrderDetail);
            }

            return returnOrderDetailList;
        }
        #endregion

        #region 删除订单明细
        [Transaction(TransactionMode.Requires)]
        public void DeleteOrderDetails(IList<int> orderDetailIds)
        {
            if (orderDetailIds == null && orderDetailIds.Count == 0)
            {
                throw new TechnicalException("OrderDetailIds is null.");
            }

            OrderDetail orderDetail = genericMgr.FindById<OrderDetail>(orderDetailIds[0]);
            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderDetail.OrderNo);

            BeforeDeleteOrderDetails(orderMaster);
            ProcessDeleteOrderDetails(orderMaster, orderDetailIds);
        }

        private void BeforeDeleteOrderDetails(OrderMaster orderMaster)
        {
            if (orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Create)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_StatusErrorWhenDeleteOrderDetail,
                    orderMaster.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
            }
        }

        private void ProcessDeleteOrderDetails(OrderMaster orderMaster, IList<int> orderDetailIds)
        {
            string selectOrderDetailHql = "from OrderDetail where Id in (";
            object[] para = new object[orderDetailIds.Count];
            IType[] type = new IType[orderDetailIds.Count];
            for (int i = 0; i < orderDetailIds.Count; i++)
            {
                if (i == 0)
                {
                    selectOrderDetailHql += "?";
                }
                else
                {
                    selectOrderDetailHql += ", ?";
                }
                para[i] = orderDetailIds[i];
                // type[i] = NHibernateUtil.UInt32;
            }
            selectOrderDetailHql += ")";

            IList<OrderDetail> orderDetailList = genericMgr.FindAll<OrderDetail>(selectOrderDetailHql, para, type);

            foreach (OrderDetail orderDetail in orderDetailList)
            {
                IList<OrderBomDetail> orderBomDetailList = TryLoadOrderBomDetails(orderDetail);
                if (orderBomDetailList != null && orderBomDetailList.Count > 0)
                {
                    this.genericMgr.Delete<OrderBomDetail>(orderBomDetailList);
                }

                IList<OrderOperation> orderOperationList = TryLoadOrderOperations(orderDetail);
                if (orderOperationList != null && orderOperationList.Count > 0)
                {
                    this.genericMgr.Delete<OrderOperation>(orderOperationList);
                }
            }

            this.genericMgr.Delete<OrderDetail>(orderDetailList);
        }
        #endregion
        #endregion

        #region 订单绑定操作
        #region 批量更新订单绑定
        [Transaction(TransactionMode.Requires)]
        public void BatchUpdateOrderBindings(string orderNo, IList<OrderBinding> addOrderBindingList, IList<OrderBinding> deleteOrderBindingList)
        {
            if (string.IsNullOrWhiteSpace(orderNo))
            {
                throw new ArgumentNullException("OrderNo not specified.");
            }

            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderNo);
            BeforeBatchUpdateOrderBinding(orderMaster);

            if (addOrderBindingList != null && addOrderBindingList.Count > 0)
            {
                ProcessAddOrderBindings(orderMaster, addOrderBindingList);
            }

            if (deleteOrderBindingList != null && deleteOrderBindingList.Count > 0)
            {
                IList<int> deleteOrderBindingIds = (from orderBinding in deleteOrderBindingList
                                                    select orderBinding.Id).ToList();
                ProcessDeleteOrderBindings(deleteOrderBindingIds);
            }
        }

        private void BeforeBatchUpdateOrderBinding(OrderMaster orderMaster)
        {
            if (orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Create)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_StatusErrorWhenUpdateOrderBinding, orderMaster.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
            }
        }
        #endregion

        #region 添加订单绑定
        [Transaction(TransactionMode.Requires)]
        public void AddOrderBindings(string orderNo, IList<OrderBinding> orderBindingList)
        {
            if (string.IsNullOrWhiteSpace(orderNo))
            {
                throw new ArgumentNullException("OrderNo not specified.");
            }

            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderNo);
            BeforeAddOrderBindings(orderMaster);
            ProcessAddOrderBindings(orderMaster, orderBindingList);
        }

        private void BeforeAddOrderBindings(OrderMaster orderMaster)
        {
            if (orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Create)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_StatusErrorWhenAddOrderBinding, orderMaster.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
            }
        }

        private void ProcessAddOrderBindings(OrderMaster orderMaster, IList<OrderBinding> orderBindingList)
        {
            foreach (OrderBinding orderBinding in orderBindingList)
            {
                orderBinding.OrderNo = orderMaster.OrderNo;
                this.genericMgr.Create(orderBinding);
            }
        }
        #endregion

        #region 删除订单绑定
        [Transaction(TransactionMode.Requires)]
        public void DeleteOrderBindings(IList<int> orderBindingIds)
        {

            if (orderBindingIds == null && orderBindingIds.Count == 0)
            {
                throw new TechnicalException("OrderBindingIds is null.");
            }

            OrderBinding orderBinding = genericMgr.FindById<OrderBinding>(orderBindingIds[0]);
            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderBinding.OrderNo);

            BeforeDeleteOrderBindings(orderMaster);
            ProcessDeleteOrderBindings(orderBindingIds);
        }

        private void BeforeDeleteOrderBindings(OrderMaster orderMaster)
        {
            if (orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Create)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_StatusErrorWhenDeleteOrderBinding, orderMaster.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
            }
        }

        private void ProcessDeleteOrderBindings(IList<int> orderBindingIds)
        {
            string hql = "from OrderBinding where Id in (";
            object[] para = new object[orderBindingIds.Count];
            IType[] type = new IType[orderBindingIds.Count];

            for (int i = 0; i < orderBindingIds.Count; i++)
            {
                if (i == 0)
                {
                    hql += "?";
                }
                else
                {
                    hql += ", ?";
                }
                para[i] = orderBindingIds[i];
                type[i] = NHibernateUtil.UInt32;
            }

            hql += ")";

            genericMgr.Delete(genericMgr.FindAll<OrderBinding>(hql, para, type));
        }
        #endregion

        #region 创建绑定订单
        ////[Transaction(TransactionMode.Requires)]
        //private void AsyncCreateBindOrder(OrderMaster orderMaster, CodeMaster.BindType bindType)
        //{
        //    AsyncCreateBindOrderDelegate asyncDelegate = new AsyncCreateBindOrderDelegate(this.CreateBindOrder);
        //    asyncDelegate.BeginInvoke(orderMaster, bindType, SecurityContextHolder.Get(), null, null);
        //}

        //private delegate void AsyncCreateBindOrderDelegate(OrderMaster orderMaster, CodeMaster.BindType bindType, User user);

        //private void CreateBindOrder(OrderMaster orderMaster, CodeMaster.BindType bindType, User user)
        private void CreateBindOrder(OrderMaster orderMaster, CodeMaster.BindType bindType)
        {
            //SecurityContextHolder.Set(user);
            if (orderMaster.SubType == CodeMaster.OrderSubType.Normal)  //只有普通订单才支持绑定
            {
                TryLoadOrderBindings(orderMaster);

                #region 根据绑定类型
                IList<OrderBinding> orderBindingList = new List<OrderBinding>();
                if (orderMaster.OrderBindings != null && orderMaster.OrderBindings.Count > 0)
                {
                    foreach (OrderBinding orderBinding in orderMaster.OrderBindings)
                    {
                        //过滤掉已经创建订单的绑定
                        if (orderBinding.BindType == bindType && string.IsNullOrWhiteSpace(orderBinding.BindOrderNo))
                        {
                            orderBindingList.Add(orderBinding);
                        }
                    }
                }
                #endregion

                #region 创建绑定订单
                DoCreateBindingOrder(orderMaster, orderBindingList);
                #endregion
            }
        }

        private void DoCreateBindingOrder(OrderMaster orderMaster, IList<OrderBinding> orderBindingList)
        {
            if (orderBindingList != null && orderBindingList.Count > 0)
            {
                if (orderMaster.Type == CodeMaster.OrderType.Production || orderMaster.Type == CodeMaster.OrderType.SubContract)
                {
                    TryLoadOrderBomDetails(orderMaster);
                    TryLoadOrderOperations(orderMaster);
                }

                #region 汇总被拉动需求
                IList<BindDemand> bindRequireList = new List<BindDemand>();
                if (orderMaster.Type == CodeMaster.OrderType.Production || orderMaster.Type == CodeMaster.OrderType.SubContract)
                {
                    #region 生产单拉动
                    foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                    {
                        ((List<BindDemand>)bindRequireList).AddRange(from bomDet in orderDetail.OrderBomDetails
                                                                     select new BindDemand
                                                                     {
                                                                         OrderDetailId = orderDetail.Id,
                                                                         OrderBomDetailId = bomDet.Id,
                                                                         Item = bomDet.Item,
                                                                         Uom = bomDet.Uom,
                                                                         BaseUom = bomDet.BaseUom,
                                                                         UnitQty = bomDet.UnitQty,
                                                                         ManufactureParty = bomDet.ManufactureParty,
                                                                         QualityType = CodeMaster.QualityType.Qualified,
                                                                         Location = bomDet.Location,
                                                                         Qty = bomDet.BomUnitQty * orderDetail.OrderedQty,
                                                                         //考虑生产单明细的其它需求源作为生产单Bom的其它需求源
                                                                         ExtraDemandSource = !string.IsNullOrWhiteSpace(orderDetail.ExtraDemandSource) ? orderDetail.ExtraDemandSource : orderMaster.ExtraDemandSource,
                                                                         WindowTime = bomDet.EstimateConsumeTime,
                                                                     });
                    }
                    #endregion
                }
                else
                {
                    #region 物流单拉动
                    ((List<BindDemand>)bindRequireList).AddRange(from det in orderMaster.OrderDetails
                                                                 select new BindDemand
                                                                 {
                                                                     OrderDetailId = det.Id,
                                                                     Item = det.Item,
                                                                     Uom = det.Uom,
                                                                     BaseUom = det.BaseUom,
                                                                     UnitQty = det.UnitQty,
                                                                     ManufactureParty = det.ManufactureParty,
                                                                     QualityType = det.QualityType,
                                                                     UnitCount = det.UnitCount,
                                                                     Location = det.LocationFrom,
                                                                     Qty = det.OrderedQty,
                                                                     ExtraDemandSource = !string.IsNullOrWhiteSpace(det.ExtraDemandSource) ? det.ExtraDemandSource : orderMaster.ExtraDemandSource,
                                                                     WindowTime = orderMaster.StartTime
                                                                 });
                    #endregion
                }
                #endregion

                IList<FlowMaster> flowMasterList = LoadFlowMaster(orderBindingList, orderMaster.EffectiveDate.HasValue ? orderMaster.EffectiveDate.Value : DateTime.Now);
                foreach (OrderBinding orderBinding in orderBindingList)
                {
                    orderBinding.CurrentBindFlowMaster = flowMasterList.Where(f => f.Code == orderBinding.BindFlow).Single();

                    IList<FlowDetail> bindFlowDetailList = new List<FlowDetail>();
                    #region 匹配需求
                    foreach (BindDemand bindDemand in bindRequireList)
                    {
                        //先匹配零件号和库位
                        //考虑了其它需求源，即需求的其它需求源和FlowDetail的目的库位相同
                        var matchedFlowDetailList = orderBinding.CurrentBindFlowMaster.FlowDetails.Where(d => d.Item == bindDemand.Item
                              && (d.LocationTo == bindDemand.Location //FlowDetail的目的库位等于需求库位
                              || (!string.IsNullOrEmpty(bindDemand.ExtraDemandSource) && bindDemand.ExtraDemandSource.IndexOf(d.LocationTo) != -1) //FlowDetail的目的库位等于其它需求源
                              || (string.IsNullOrWhiteSpace(d.LocationTo) && orderBinding.CurrentBindFlowMaster.LocationTo == bindDemand.Location) //FlowDetail的目的库位为空 and FlowMaster的目的库位等于需求库位
                              || (string.IsNullOrWhiteSpace(d.LocationTo) && !string.IsNullOrEmpty(bindDemand.ExtraDemandSource) && bindDemand.ExtraDemandSource.IndexOf(orderBinding.CurrentBindFlowMaster.LocationTo) != -1)));  //FlowDetail的目的库位为空 and FlowMaster的目的库位等于其它需求源

                        //1. 一个需求只能匹配一条FlowDetail，找出最佳匹配项
                        //2. 优先顺序为库位/其它需求源、单位、单包装
                        if (matchedFlowDetailList != null && matchedFlowDetailList.Count() > 0)
                        {
                            var matcheLocationList = matchedFlowDetailList.Where(d => d.LocationTo == bindDemand.Location);

                            if (matcheLocationList != null && matcheLocationList.Count() > 0)
                            {
                                matchedFlowDetailList = matcheLocationList;
                            }

                            var matcheUomList = matchedFlowDetailList.Where(d => d.Uom == bindDemand.Uom);

                            if (matcheUomList != null && matcheUomList.Count() > 0)
                            {
                                matchedFlowDetailList = matcheUomList;
                            }

                            var matcheUCList = matchedFlowDetailList.Where(d => d.UnitCount == bindDemand.UnitCount);

                            if (matcheUCList != null && matcheUCList.Count() > 0)
                            {
                                matchedFlowDetailList = matcheUCList;
                            }

                            FlowDetail flowDetail = Mapper.Map<FlowDetail, FlowDetail>(matchedFlowDetailList.OrderBy(d => d.Sequence).First());
                            if (flowDetail.Uom != bindDemand.Uom)
                            {
                                flowDetail.OrderQty = bindDemand.Qty * bindDemand.UnitQty;//先转为基本单位

                                if (flowDetail.Uom != flowDetail.BaseUom)
                                {
                                    //在转为FlowDetail单位
                                    flowDetail.OrderQty = this.itemMgr.ConvertItemUomQty(flowDetail.Item, flowDetail.BaseUom, flowDetail.OrderQty, flowDetail.Uom);
                                }
                            }
                            else
                            {
                                flowDetail.OrderQty = bindDemand.Qty;
                            }
                            flowDetail.BindDemand = bindDemand;
                            bindFlowDetailList.Add(flowDetail);
                        }
                    }
                    #endregion

                    #region 创建绑定订单
                    if (bindFlowDetailList.Count > 0)
                    {
                        #region 创建绑定订单
                        FlowMaster flowMaster = Mapper.Map<FlowMaster, FlowMaster>(orderBinding.CurrentBindFlowMaster);
                        FlowStrategy flowStrategry = this.genericMgr.FindById<FlowStrategy>(flowMaster.Code);
                        flowMaster.FlowDetails = bindFlowDetailList;

                        OrderMaster bindOrderMaster = TransferFlow2Order(flowMaster, null, orderMaster.EffectiveDate.HasValue ? orderMaster.EffectiveDate.Value : DateTime.Now);
                        bindOrderMaster.ExternalOrderNo = orderMaster.ExternalOrderNo;
                        bindOrderMaster.ReferenceOrderNo = orderMaster.OrderNo;
                        bindOrderMaster.TraceCode = orderMaster.TraceCode;
                        bindOrderMaster.WindowTime = flowMaster.FlowDetails.Min(d => d.BindDemand.WindowTime);  //取绑定明细最小的窗口时间

                        IList<WorkingCalendarView> workingCalendarViewList = this.workingCalendarMgr.GetWorkingCalendarView(bindOrderMaster.PartyFrom, bindOrderMaster.WindowTime.Add(TimeSpan.FromDays(-7)), bindOrderMaster.WindowTime);
                        DateTime startTime = this.workingCalendarMgr.GetStartTimeAtWorkingDate(bindOrderMaster.WindowTime, (double)flowStrategry.LeadTime, CodeMaster.TimeUnit.Hour, bindOrderMaster.PartyFrom, workingCalendarViewList);
                        if (startTime < DateTime.Now)
                        {
                            DateTime emStartTime = this.workingCalendarMgr.GetStartTimeAtWorkingDate(bindOrderMaster.WindowTime, (double)flowStrategry.EmergencyLeadTime, CodeMaster.TimeUnit.Hour, bindOrderMaster.PartyFrom, workingCalendarViewList);// bindOrderMaster.WindowTime.AddHours(-(double)flowStrategry.EmergencyLeadTime);
                            if (emStartTime < DateTime.Now)
                            {
                                bindOrderMaster.StartTime = emStartTime;
                                bindOrderMaster.Priority = CodeMaster.OrderPriority.Urgent;
                            }
                            else
                            {
                                bindOrderMaster.StartTime = startTime;
                                bindOrderMaster.Priority = CodeMaster.OrderPriority.Normal;
                            }
                        }
                        else
                        {
                            bindOrderMaster.StartTime = startTime;
                            bindOrderMaster.Priority = CodeMaster.OrderPriority.Normal;
                        }
                        bindOrderMaster.QualityType = orderMaster.QualityType;
                        bindOrderMaster.Sequence = orderMaster.Sequence;
                        bindOrderMaster.EffectiveDate = orderMaster.EffectiveDate;
                        bindOrderMaster.OrderStrategy = flowStrategry.Strategy;

                        this.CreateOrder(bindOrderMaster);


                        #region 创建OrderBindingDetail
                        UpdateOrderBinding(orderBinding, bindOrderMaster);
                        #endregion
                        #endregion
                    }
                    #endregion
                }
            }
        }

        #region 重新绑定路线
        [Transaction(TransactionMode.Requires)]
        public void ReCreateBindOrder(OrderBinding orderBinding)
        {
            if (!string.IsNullOrEmpty(orderBinding.BindOrderNo))
            {
                OrderMaster bindOrder = this.genericMgr.FindById<OrderMaster>(orderBinding.BindOrderNo);

                if (bindOrder.Type == CodeMaster.OrderType.Production)
                {
                    throw new BusinessException("被绑定订单{0}为生产单，不能重新创建绑定。", bindOrder.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)bindOrder.Status).ToString()));
                }

                if (bindOrder.Status == CodeMaster.OrderStatus.Create)
                {
                    this.DeleteOrder(bindOrder.OrderNo);
                }
                else if (bindOrder.Status == CodeMaster.OrderStatus.Submit)
                {
                    this.CancelOrder(bindOrder);
                }
                else
                {
                    throw new BusinessException("被绑定订单{0}的状态为{1}不能重新创建绑定。", bindOrder.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)bindOrder.Status).ToString()));
                }
            }

            OrderMaster orderMaster = this.genericMgr.FindById<OrderMaster>(orderBinding.OrderNo);
            IList<OrderBinding> orderBindingList = new List<OrderBinding>();
            orderBindingList.Add(orderBinding);
            DoCreateBindingOrder(orderMaster, orderBindingList);
        }
        #endregion

        #region 创建OrderBindingDetail
        private void UpdateOrderBinding(OrderBinding orderBinding, OrderMaster orderMaster)
        {
            #region 更新OrderBinding
            orderBinding.BindOrderNo = orderMaster.OrderNo;
            orderBinding.CurrentBindOrderMaster = orderMaster;
            this.genericMgr.Update(orderBinding);
            #endregion

            #region 创建OrderBindingDetail
            foreach (OrderDetail bindOrderDetail in orderMaster.OrderDetails)
            {
                OrderBindingDetail orderBindingDetail = new OrderBindingDetail();
                orderBindingDetail.OrderBindingId = orderBinding.Id;
                orderBindingDetail.OrderNo = orderMaster.OrderNo;
                orderBindingDetail.OrderDetailId = bindOrderDetail.BindDemand.OrderDetailId;
                orderBindingDetail.OrderBomDetailId = bindOrderDetail.BindDemand.OrderBomDetailId;
                orderBindingDetail.BindOrderNo = orderMaster.OrderNo;
                orderBindingDetail.BindOrderDetailId = bindOrderDetail.Id;

                this.genericMgr.Create(bindOrderDetail);
            }
            #endregion
        }
        #endregion

        #endregion
        #endregion

        #region 订单工艺流程/BOM操作

        #region 工艺流程操作
        #region 批量更新工序
        [Transaction(TransactionMode.Requires)]
        public void BatchUpdateOrderOperations(int orderDetailId, IList<OrderOperation> addOrderOperationList, IList<OrderOperation> updateOrderOperationList, IList<OrderOperation> deleteOrderOperationList)
        {
            OrderDetail orderDetail = genericMgr.FindById<OrderDetail>(orderDetailId);
            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderDetail.OrderNo);
            BeforeBatchUpdateOrderOperations(orderMaster);

            if (addOrderOperationList != null && addOrderOperationList.Count > 0)
            {
                ProcessAddOrderOperations(orderDetail, addOrderOperationList);
            }

            if (updateOrderOperationList != null && updateOrderOperationList.Count > 0)
            {
                ProcessUpdateOrderOperations(updateOrderOperationList);
            }

            if (deleteOrderOperationList != null && deleteOrderOperationList.Count > 0)
            {
                IList<int> deleteOrderOperationIds = (from orderOp in deleteOrderOperationList
                                                      select orderOp.Id).ToList();
                ProcessDeleteOrderOperations(deleteOrderOperationIds);
            }
        }

        private void BeforeBatchUpdateOrderOperations(OrderMaster orderMaster)
        {
            BeforeUpdateOrderOperations(orderMaster);
        }
        #endregion

        #region 添加工序
        [Transaction(TransactionMode.Requires)]
        public void AddOrderOperations(int orderDetailId, IList<OrderOperation> orderOperationList)
        {
            OrderDetail orderDetail = genericMgr.FindById<OrderDetail>(orderDetailId);
            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderDetail.OrderNo);
            BeforeAddOrderOperations(orderMaster);
            ProcessAddOrderOperations(orderDetail, orderOperationList);
        }

        private void BeforeAddOrderOperations(OrderMaster orderMaster)
        {
            if (orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Create)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_StatusErrorWhenAddOrderOperation, orderMaster.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
            }
        }

        private void ProcessAddOrderOperations(OrderDetail orderDetail, IList<OrderOperation> orderOperationList)
        {
            if (orderOperationList != null && orderOperationList.Count > 0)
            {
                foreach (OrderOperation orderOperation in orderOperationList)
                {
                    orderOperation.OrderNo = orderDetail.OrderNo;
                    orderOperation.OrderDetailId = orderDetail.Id;
                    this.genericMgr.Create(orderOperation);
                }
            }
        }
        #endregion

        #region 更新工序
        [Transaction(TransactionMode.Requires)]
        public void UpdateOrderOperations(IList<OrderOperation> orderOperations)
        {
            if (orderOperations == null && orderOperations.Count == 0)
            {
                throw new TechnicalException("OrderOperations is null.");
            }

            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderOperations[0].OrderNo);
            BeforeUpdateOrderOperations(orderMaster);
            ProcessUpdateOrderOperations(orderOperations);
        }

        private void BeforeUpdateOrderOperations(OrderMaster orderMaster)
        {
            if (orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Create)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_StatusErrorWhenModifyOrderOperation,
                    orderMaster.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
            }
        }

        private void ProcessUpdateOrderOperations(IList<OrderOperation> orderOperations)
        {
            foreach (OrderOperation orderOperation in orderOperations)
            {
                this.genericMgr.Update(orderOperation);
            }
        }
        #endregion

        #region 删除工序
        [Transaction(TransactionMode.Requires)]
        public void DeleteOrderOperations(IList<int> orderOperationIds)
        {
            if (orderOperationIds == null && orderOperationIds.Count == 0)
            {
                throw new TechnicalException("OrderOperationIds is null.");
            }

            OrderOperation orderOperation = genericMgr.FindById<OrderOperation>(orderOperationIds[0]);
            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderOperation.OrderNo);
            BeforeDeleteOrderOperations(orderMaster);
            ProcessDeleteOrderOperations(orderOperationIds);
        }

        private void BeforeDeleteOrderOperations(OrderMaster orderMaster)
        {
            if (orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Create)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_StatusErrorWhenDeleteOrderOperation,
                    orderMaster.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
            }
        }

        private void ProcessDeleteOrderOperations(IList<int> orderOperationIds)
        {
            string hql = "from OrderOperation where Id in (";
            object[] para = new object[orderOperationIds.Count];
            IType[] type = new IType[orderOperationIds.Count];

            for (int i = 0; i < orderOperationIds.Count; i++)
            {
                if (i == 0)
                {
                    hql += "?";
                }
                else
                {
                    hql += ", ?";
                }
                para[i] = orderOperationIds[i];
                type[i] = NHibernateUtil.UInt32;
            }

            hql += ")";

            genericMgr.Delete(genericMgr.FindAll<OrderOperation>(hql, para, type));
        }
        #endregion

        #region 展开工序
        [Transaction(TransactionMode.Requires)]
        public IList<OrderOperation> ExpandOrderOperation(int orderDetailId)
        {
            OrderDetail orderDetail = genericMgr.FindById<OrderDetail>(orderDetailId);
            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderDetail.OrderNo);

            return ExpandOrderOperation(orderMaster, orderDetail);
        }

        private IList<OrderOperation> ExpandOrderOperation(OrderMaster orderMaster, OrderDetail orderDetail)
        {
            BeforeExpandOrderOperation(orderMaster);
            return ProcessExpandOrderOperation(orderMaster, orderDetail);
        }

        private void BeforeExpandOrderOperation(OrderMaster orderMaster)
        {
            if (!(orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Complete
                //&& orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Close
                && orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Cancel))
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_StatusErrorWhenExpandOrderOperation,
                    orderMaster.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
            }
        }

        private IList<OrderOperation> ProcessExpandOrderOperation(OrderMaster orderMaster, OrderDetail orderDetail)
        {
            string hql = "from OrderOperation where OrderDetailId = ?";
            genericMgr.Delete(genericMgr.FindAll<OrderOperation>(hql, orderDetail.Id));
            GenerateOrderOperation(orderDetail, orderMaster);
            ProcessAddOrderOperations(orderDetail, orderDetail.OrderOperations);

            return orderDetail.OrderOperations;
        }
        #endregion
        #endregion

        #region BOM操作
        #region 批量更新BOM
        public void BatchUpdateOrderBomDetails(int orderDetailId, IList<OrderBomDetail> addOrderBomDetailList, IList<OrderBomDetail> updateOrderBomDetailList, IList<OrderBomDetail> deleteOrderBomDetailList)
        {
            OrderDetail orderDetail = genericMgr.FindById<OrderDetail>(orderDetailId);
            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderDetail.OrderNo);
            BeforeBatchUpdateOrderBomDetails(orderMaster);

            if (addOrderBomDetailList != null && addOrderBomDetailList.Count > 0)
            {
                ProcessAddOrderBomDetails(orderDetail, addOrderBomDetailList);
            }

            if (updateOrderBomDetailList != null && updateOrderBomDetailList.Count > 0)
            {
                ProcessUpdateOrderBomDetails(updateOrderBomDetailList);
            }

            if (deleteOrderBomDetailList != null && deleteOrderBomDetailList.Count > 0)
            {
                IList<int> deleteOrderBomDetailIds = (from orderBomDet in deleteOrderBomDetailList
                                                      select orderBomDet.Id).ToList();
                ProcessDeleteOrderBomDetails(deleteOrderBomDetailIds);
            }
        }

        private void BeforeBatchUpdateOrderBomDetails(OrderMaster orderMaster)
        {
            BeforeUpdateOrderBomDetails(orderMaster);
        }
        #endregion

        #region 添加BOM
        [Transaction(TransactionMode.Requires)]
        public void AddOrderBomDetails(int orderDetailId, IList<OrderBomDetail> orderBomDetailList)
        {
            OrderDetail orderDetail = genericMgr.FindById<OrderDetail>(orderDetailId);
            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderDetail.OrderNo);
            BeforeAddOrderBomDetails(orderMaster);
            ProcessAddOrderBomDetails(orderDetail, orderBomDetailList);
        }

        private void BeforeAddOrderBomDetails(OrderMaster orderMaster)
        {
            if (!(orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Complete   //释放和执行中都能修改BOM Detail
                //&& orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Close
                   && orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Cancel))
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_StatusErrorWhenAddOrderOperation, orderMaster.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
            }
        }

        private void ProcessAddOrderBomDetails(OrderDetail orderDetail, IList<OrderBomDetail> orderBomDetailList)
        {
            if (orderBomDetailList != null && orderBomDetailList.Count > 0)
            {
                #region 查找最大序号的OrderBomDetail
                string hql = "select max(Sequence) as MaxSeq from OrderBomDetail where OrderDetailId = ?";

                IList<int?> maxSeqList = this.genericMgr.FindAll<int?>(hql, orderDetail.Id);
                int maxSeq = 0;
                if (maxSeqList != null && maxSeqList.Count > 0 && maxSeqList[0] != null)
                {
                    maxSeq = maxSeqList[0].Value;
                }
                #endregion

                foreach (OrderBomDetail orderBomDetail in orderBomDetailList.OrderBy(o => o.Operation).ThenBy(o => o.OpReference))
                {
                    orderBomDetail.Sequence = ++maxSeq;
                    orderBomDetail.OrderNo = orderDetail.OrderNo;
                    orderBomDetail.OrderType = orderDetail.OrderType;
                    orderBomDetail.OrderSubType = orderDetail.OrderSubType;
                    orderBomDetail.OrderDetailId = orderDetail.Id;
                    orderBomDetail.OrderDetailSequence = orderDetail.Sequence;
                    this.genericMgr.Create(orderBomDetail);
                }
            }
        }
        #endregion

        #region 更新BOM
        [Transaction(TransactionMode.Requires)]
        public void UpdateOrderBomDetails(IList<OrderBomDetail> orderBomDetails)
        {
            if (orderBomDetails == null && orderBomDetails.Count == 0)
            {
                throw new TechnicalException("OrderBomDetails is null.");
            }

            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderBomDetails[0].OrderNo);
            BeforeUpdateOrderBomDetails(orderMaster);
            ProcessUpdateOrderBomDetails(orderBomDetails);
        }

        private void BeforeUpdateOrderBomDetails(OrderMaster orderMaster)
        {
            if (!(orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Complete   //释放和执行中都能修改BOM Detail
                //&& orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Close
                && orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Cancel))
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_StatusErrorWhenModifyOrderOperation,
                    orderMaster.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
            }
        }

        private void ProcessUpdateOrderBomDetails(IList<OrderBomDetail> orderBomDetails)
        {
            foreach (OrderBomDetail orderBomDetail in orderBomDetails)
            {
                this.genericMgr.Update(orderBomDetail);
            }
        }
        #endregion

        #region 删除BOM
        [Transaction(TransactionMode.Requires)]
        public void DeleteOrderBomDetails(IList<int> orderBomDetailIds)
        {
            if (orderBomDetailIds == null && orderBomDetailIds.Count == 0)
            {
                throw new TechnicalException("OrderBomDetailIds is null.");
            }

            OrderBomDetail orderBomDetail = genericMgr.FindById<OrderBomDetail>(orderBomDetailIds[0]);
            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderBomDetail.OrderNo);
            BeforeDeleteOrderBomDetails(orderMaster);
            ProcessDeleteOrderBomDetails(orderBomDetailIds);
        }

        private void BeforeDeleteOrderBomDetails(OrderMaster orderMaster)
        {
            if (!(orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Complete   //释放和执行中都能修改BOM Detail
                //&& orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Close
                && orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Cancel))
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_StatusErrorWhenModifyOrderOperation,
                    orderMaster.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
            }
        }

        private void ProcessDeleteOrderBomDetails(IList<int> orderBomDetailIds)
        {
            string hql = "from OrderBomDetail where Id in (";
            object[] para = new object[orderBomDetailIds.Count];

            for (int i = 0; i < orderBomDetailIds.Count; i++)
            {
                if (i == 0)
                {
                    hql += "?";
                }
                else
                {
                    hql += ", ?";
                }
                para[i] = orderBomDetailIds[i];
            }

            hql += ")";

            genericMgr.Delete(genericMgr.FindAll<OrderBomDetail>(hql, para));
        }
        #endregion

        #region 展开BOM
        [Transaction(TransactionMode.Requires)]
        public IList<OrderBomDetail> ExpandOrderBomDetail(int orderDetailId)
        {
            OrderDetail orderDetail = genericMgr.FindById<OrderDetail>(orderDetailId);
            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderDetail.OrderNo);

            return ExpandOrderBomDetail(orderMaster, orderDetail);
        }

        private IList<OrderBomDetail> ExpandOrderBomDetail(OrderMaster orderMaster, OrderDetail orderDetail)
        {
            BeforeExpandOrderBomDetail(orderMaster);
            return ProcessExpandOrderBomDetail(orderMaster, orderDetail);
        }

        private void BeforeExpandOrderBomDetail(OrderMaster orderMaster)
        {
            if (orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Create)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_StatusErrorWhenExpandOrderBomDetail,
                    orderMaster.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
            }
        }

        private IList<OrderBomDetail> ProcessExpandOrderBomDetail(OrderMaster orderMaster, OrderDetail orderDetail)
        {
            string hql = "from OrderBomDetail where OrderDetailId = ?";
            genericMgr.Delete(genericMgr.FindAll<OrderBomDetail>(hql, orderDetail.Id));
            GenerateOrderBomDetail(orderDetail, orderMaster);
            ProcessAddOrderBomDetails(orderDetail, orderDetail.OrderBomDetails);

            return orderDetail.OrderBomDetails;
        }
        #endregion

        #region 展开工艺流程和BOM
        [Transaction(TransactionMode.Requires)]
        public object[] ExpandOrderOperationAndBomDetail(int orderDetailId)
        {
            OrderDetail orderDetail = genericMgr.FindById<OrderDetail>(orderDetailId);
            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderDetail.OrderNo);

            return ExpandOrderOperationAndBomDetail(orderMaster, orderDetail);
        }

        private object[] ExpandOrderOperationAndBomDetail(OrderMaster orderMaster, OrderDetail orderDetail)
        {
            BeforeExpandOrderOperationAndBomDetail(orderMaster);
            object[] returnList = new object[2];
            returnList[0] = this.ProcessExpandOrderOperation(orderMaster, orderDetail);
            returnList[1] = this.ProcessExpandOrderBomDetail(orderMaster, orderDetail);

            return returnList;
        }

        private void BeforeExpandOrderOperationAndBomDetail(OrderMaster orderMaster)
        {
            if (orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Create)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_StatusErrorWhenExpandOrderOperationAndBomDetail,
                    orderMaster.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
            }
        }
        #endregion
        #endregion
        #endregion

        #region 订单释放
        [Transaction(TransactionMode.Requires)]
        public void ReleaseOrder(string orderNo)
        {
            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderNo);
            this.ReleaseOrder(orderMaster);
        }

        [Transaction(TransactionMode.Requires)]
        public void ReleaseOrder(OrderMaster orderMaster)
        {
            if (orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.Create)
            {
                #region 验证OrderDetail不能为空
                TryLoadOrderDetails(orderMaster);
                if (orderMaster.OrderDetails == null || orderMaster.OrderDetails.Count == 0)
                {
                    throw new BusinessException(Resources.ORD.OrderMaster.Errors_OrderDetailIsEmpty);
                }
                #endregion

                #region 验证OrderBomDetail不能为空
                if (orderMaster.Type == com.Sconit.CodeMaster.OrderType.Production
                    || orderMaster.Type == com.Sconit.CodeMaster.OrderType.SubContract)
                {
                    TryLoadOrderBomDetails(orderMaster);
                    foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                    {
                        if (orderDetail.OrderBomDetails == null || orderDetail.OrderBomDetails.Count == 0)
                        {
                            throw new BusinessException(Resources.ORD.OrderMaster.Errors_OrderBomDetailIsEmpty, orderDetail.Item);
                        }
                    }
                }
                #endregion

                #region 订单、生产线暂停检查
                if (orderMaster.PauseStatus == CodeMaster.PauseStatus.Paused)
                {
                    if (orderMaster.Type == CodeMaster.OrderType.Production)
                    {
                        throw new BusinessException("生产单{0}已经暂停，不能释放。", orderMaster.OrderNo);
                    }
                    else
                    {
                        throw new BusinessException("订单{0}已经暂停，不能释放。", orderMaster.OrderNo);
                    }
                }

                if ((orderMaster.Type == CodeMaster.OrderType.Production
                    || orderMaster.Type == CodeMaster.OrderType.SubContract)
                    && !string.IsNullOrWhiteSpace(orderMaster.Flow))
                {
                    FlowMaster flowMaster = this.genericMgr.FindById<FlowMaster>(orderMaster.Flow);
                    if (flowMaster.IsPause)
                    {
                        throw new BusinessException("生产线{0}已经暂停，不能释放。", orderMaster.Flow);
                    }
                }
                #endregion

                #region 释放的时候要把明细上locfrom，locto，Routing，BillAddr，PriceList，BillTerm为空的保存为头上的默认值
                //可以考虑写成存储过程
                foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                {
                    bool isUpdate = false;
                    if (string.IsNullOrWhiteSpace(orderDetail.LocationFrom)
                        && !string.IsNullOrWhiteSpace(orderMaster.LocationFrom))
                    {
                        orderDetail.LocationFrom = orderMaster.LocationFrom;
                        orderDetail.LocationFromName = orderMaster.LocationFromName;
                        isUpdate = true;
                    }

                    if (string.IsNullOrWhiteSpace(orderDetail.LocationTo)
                        && !string.IsNullOrWhiteSpace(orderMaster.LocationTo))
                    {
                        orderDetail.LocationTo = orderMaster.LocationTo;
                        orderDetail.LocationToName = orderMaster.LocationToName;

                        isUpdate = true;
                    }

                    if (string.IsNullOrWhiteSpace(orderDetail.Routing)
                        && !string.IsNullOrWhiteSpace(orderMaster.Routing))
                    {
                        orderDetail.Routing = orderMaster.Routing;
                        isUpdate = true;
                    }

                    if (string.IsNullOrWhiteSpace(orderDetail.BillAddress)
                        && !string.IsNullOrWhiteSpace(orderMaster.BillAddress))
                    {
                        orderDetail.BillAddress = orderMaster.BillAddress;
                        orderDetail.BillAddressDescription = orderMaster.BillAddressDescription;
                        isUpdate = true;
                    }

                    if (string.IsNullOrWhiteSpace(orderDetail.PriceList)
                        && !string.IsNullOrWhiteSpace(orderMaster.PriceList))
                    {
                        orderDetail.PriceList = orderMaster.PriceList;
                        isUpdate = true;
                    }

                    if (orderDetail.BillTerm == com.Sconit.CodeMaster.OrderBillTerm.NA
                        && orderMaster.BillTerm != com.Sconit.CodeMaster.OrderBillTerm.NA)
                    {
                        orderDetail.BillTerm = orderMaster.BillTerm;
                        isUpdate = true;
                    }

                    if ((orderMaster.Type == com.Sconit.CodeMaster.OrderType.Procurement
                        || orderMaster.Type == com.Sconit.CodeMaster.OrderType.ScheduleLine
                        || orderMaster.Type == com.Sconit.CodeMaster.OrderType.Distribution
                         || orderMaster.Type == com.Sconit.CodeMaster.OrderType.SubContract)
                        && string.IsNullOrWhiteSpace(orderDetail.Currency))
                    {
                        if (string.IsNullOrWhiteSpace(orderMaster.Currency))
                        {
                            //throw new BusinessException("订单{0}没有指定币种", orderMaster.OrderNo);
                        }
                        orderDetail.Currency = orderMaster.Currency;
                        isUpdate = true;
                    }

                    if ((orderMaster.Type == com.Sconit.CodeMaster.OrderType.Transfer
                        || orderMaster.Type == com.Sconit.CodeMaster.OrderType.SubContractTransfer
                        || orderMaster.Type == com.Sconit.CodeMaster.OrderType.Distribution)
                        && orderMaster.IsShipScanHu && string.IsNullOrWhiteSpace(orderDetail.PickStrategy))
                    {
                        orderDetail.PickStrategy = orderMaster.PickStrategy;
                    }

                    if (isUpdate)
                    {
                        genericMgr.Update(orderDetail);
                    }
                }
                #endregion

                #region 更新订单
                orderMaster.Status = com.Sconit.CodeMaster.OrderStatus.Submit;
                orderMaster.ReleaseDate = DateTime.Now;
                User user = SecurityContextHolder.Get();
                orderMaster.ReleaseUserId = user.Id;
                orderMaster.ReleaseUserName = user.FullName;
                genericMgr.Update(orderMaster);
                #endregion

                #region 生成拣货任务
                pickTaskMgr.CreatePickTask(orderMaster);
                #endregion

                #region 发送打印
                this.AsyncSendPrintData(orderMaster);
                #endregion

                #region 自动Start
                if (orderMaster.IsQuick || orderMaster.IsAutoStart)
                {
                    StartOrder(orderMaster);
                }
                #endregion

                #region 自动捡货/发货/收货
                //AutoShipAndReceive(orderMaster);
                #endregion

                #region 触发订单释放事件
                if (OrderReleased != null)
                {
                    //Call the Event
                    OrderReleased(orderMaster);
                }
                #endregion

                #region 创建绑定的订单
                //AsyncCreateBindOrder(orderMaster, CodeMaster.BindType.Submit);
                CreateBindOrder(orderMaster, CodeMaster.BindType.Submit);
                #endregion

                #region 采购单创建计划协议记录
                if (orderMaster.Type == CodeMaster.OrderType.Procurement
                    && orderMaster.SubType == CodeMaster.OrderSubType.Normal)
                {
                    string plant = this.genericMgr.FindById<Region>(orderMaster.PartyTo).Plant;
                    foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                    {
                        if (orderDetail.OrderedQty > 0)
                        {
                            com.Sconit.Entity.SAP.ORD.CreateScheduleLine crsl = new com.Sconit.Entity.SAP.ORD.CreateScheduleLine();
                            crsl.EINDT = orderMaster.WindowTime.ToString("yyyyMMdd");
                            crsl.FRBNR = orderMaster.OrderNo;
                            crsl.LIFNR = orderMaster.PartyFrom;
                            crsl.MATNR = orderDetail.Item;
                            crsl.MENGE = orderDetail.OrderedQty;
                            crsl.SGTXT = orderDetail.Sequence.ToString();
                            crsl.WERKS = plant;
                            crsl.OrderDetId = orderDetail.Id;

                            this.genericMgr.Create(crsl);
                        }
                    }
                }
                #endregion

                #region 来源区域是LOC,SQC 的 释放后生产DAT
                if (((orderMaster.PartyFrom == systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.WMSAnjiRegion)
                    || orderMaster.PartyFrom == "SQC"))
                    && orderMaster.SubType == com.Sconit.CodeMaster.OrderSubType.Normal && orderMaster.Type != com.Sconit.CodeMaster.OrderType.Distribution)
                {
                    //2013-9-30 集货库因为未上wms新系统所以还是用老接口格式
                    //if (orderMaster.PartyFrom == "SQC")
                    //    this.CreateOrderDAT(orderMaster);
                    //else
                        //this.CreateProcurementOrderDAT(orderMaster);
                    //this.CreateProcurementOrderDAT(orderMaster); 全面上线后要调用此方法

                    if (orderMaster.OrderStrategy != com.Sconit.CodeMaster.FlowStrategy.SEQ)
                    {
                        //this.CreateOrderDAT(orderMaster);
                        this.CreateProcurementOrderDAT(orderMaster);
                    }
                    else
                    {
                        this.CreateSeqOrderDAT(orderMaster);
                    }
                }
                #endregion

                #region 良品退货单接口（写入中间表）
                if (orderMaster.SubType == CodeMaster.OrderSubType.Return && (orderMaster.PartyTo.ToUpper() == "LOC" || orderMaster.PartyTo.ToUpper() == "SQC") && orderMaster.Type != com.Sconit.CodeMaster.OrderType.Distribution)
                {
                    this.CreateReturnOrderDAT(orderMaster);
                }
                #endregion

                OrderHolder.AddOrder(orderMaster.OrderNo);
            }
            else
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_StatusErrorWhenRelease,
                    orderMaster.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
            }
        }
        #endregion

        #region 订单上线
        [Transaction(TransactionMode.Requires)]
        public void StartOrder(string orderNo)
        {
            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderNo);
            this.StartOrder(orderMaster);
        }

        [Transaction(TransactionMode.Requires)]
        public void StartOrder(OrderMaster orderMaster)
        {
            if (orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.Submit)
            {
                TryLoadOrderDetails(orderMaster);

                #region 更新订单
                UpdateOrderMasterStatus2InProcess(orderMaster);
                #endregion

                #region 自动捡货/发货/收货
                AutoShipAndReceive(orderMaster);
                #endregion

                #region 绑定同步
                //AsyncCreateBindOrder(orderMaster, CodeMaster.BindType.Start);
                CreateBindOrder(orderMaster, CodeMaster.BindType.Start);
                #endregion
            }
            else
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_StatusErrorWhenStart,
                       orderMaster.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
            }
        }

        private void AutoShipAndReceive(OrderMaster orderMaster)
        {
            #region 自动捡货/发货/收货
            if (!orderMaster.IsQuick && orderMaster.Type != com.Sconit.CodeMaster.OrderType.Production          //自动生成捡货单
                && orderMaster.Type != com.Sconit.CodeMaster.OrderType.SubContract
                && orderMaster.IsCreatePickList && orderMaster.IsShipScanHu
                && orderMaster.SubType == com.Sconit.CodeMaster.OrderSubType.Normal //过滤掉退货
                && orderMaster.QualityType == com.Sconit.CodeMaster.QualityType.Qualified)   //过滤掉不合格品和待验物料
            {
                foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                {
                    OrderDetailInput orderDetailInput = new OrderDetailInput();
                    orderDetailInput.PickQty = orderDetail.OrderedQty;
                    orderDetail.AddOrderDetailInput(orderDetailInput);
                }
                pickListMgr.CreatePickList(orderMaster.OrderDetails);
            }
            else if (!orderMaster.IsQuick && orderMaster.Type != com.Sconit.CodeMaster.OrderType.Production       //生产单和委外加工没有发货概念                
                && orderMaster.Type != com.Sconit.CodeMaster.OrderType.SubContract
                && orderMaster.IsAutoShip
                && orderMaster.SubType == com.Sconit.CodeMaster.OrderSubType.Normal                         //过滤掉退货
                && orderMaster.QualityType == com.Sconit.CodeMaster.QualityType.Qualified                      //过滤掉不合格品和待验物料
                && !(orderMaster.IsCreatePickList && orderMaster.IsShipScanHu))  //自动捡货和自动发货/自动收货冲突，如果设置了自动捡货将不考虑自动发货/自动收货选项
            {
                foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                {
                    if (orderDetail.OrderDetailInputs == null || orderDetail.OrderDetailInputs.Count == 0)
                    {
                        OrderDetailInput orderDetailInput = new OrderDetailInput();
                        orderDetailInput.ShipQty = orderDetail.OrderedQty;
                        orderDetail.AddOrderDetailInput(orderDetailInput);
                    }
                }
                ShipOrder(orderMaster.OrderDetails, orderMaster.EffectiveDate.HasValue ? orderMaster.EffectiveDate.Value : DateTime.Now);
            }
            else if (orderMaster.IsQuick //&& !orderMaster.IsShipScanHu && !orderMaster.IsReceiveScanHu        //快速订单直接收货，跳过发货和捡货
                || (orderMaster.IsAutoReceive
                 && orderMaster.SubType == com.Sconit.CodeMaster.OrderSubType.Normal                         //过滤掉退货
                && orderMaster.QualityType == com.Sconit.CodeMaster.QualityType.Qualified                      //过滤掉不合格品和待验物料
                && !(orderMaster.IsCreatePickList && orderMaster.IsShipScanHu)))  //支持不发货直接收货
            {
                foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                {
                    if (orderDetail.OrderDetailInputs == null || orderDetail.OrderDetailInputs.Count == 0)
                    {
                        OrderDetailInput orderDetailInput = new OrderDetailInput();
                        orderDetailInput.ReceiveQty = orderDetail.OrderedQty;
                        orderDetail.AddOrderDetailInput(orderDetailInput);
                    }
                }
                ReceiveOrder(orderMaster.OrderDetails, orderMaster.EffectiveDate.HasValue ? orderMaster.EffectiveDate.Value : DateTime.Now);
            }
            #endregion
        }

        private void UpdateOrderMasterStatus2InProcess(OrderMaster orderMaster)
        {
            orderMaster.Status = com.Sconit.CodeMaster.OrderStatus.InProcess;
            orderMaster.StartDate = DateTime.Now;
            User user = SecurityContextHolder.Get();
            orderMaster.StartUserId = user.Id;
            orderMaster.StartUserName = user.FullName;
            genericMgr.Update(orderMaster);
        }
        #endregion

        #region 计划协议发货
        [Transaction(TransactionMode.Requires)]
        public IpMaster ShipScheduleLine(string flow, IList<ScheduleLineInput> scheduleLineInputList)
        {
            IList<OrderDetail> shipOrderDetailList = new List<OrderDetail>();
            StringBuilder selectOrderDetailStatement = new StringBuilder();
            IList<object> selectOrderDetailParam = new List<object>();
            StringBuilder selectOrderMasterStatement = new StringBuilder();
            IList<object> selectOrderMasterParam = new List<object>();
            StringBuilder selectShippedQtyStatement = new StringBuilder();
            IList<object> selectShippedQtyParam = new List<object>();

            foreach (ScheduleLineInput scheduleLineInput in scheduleLineInputList)
            {
                if (selectOrderDetailStatement.Length == 0)
                {
                    selectOrderDetailStatement.Append("select * from ORD_OrderDet_8 where EBELN_EBELP in (?");
                    selectOrderMasterStatement.Append("select mstr.* from ORD_OrderMstr_8 as mstr inner join ORD_OrderDet_8 as det on mstr.OrderNo = det.OrderNo where det.EBELN_EBELP in (?");
                    selectShippedQtyStatement.Append(@"select EBELN_EBELP, SUM(CASE WHEN IsClose = 1 THEN RecQty ELSE (CASE WHEN RecQty = 0 THEN Qty ELSE RecQty END) END) as shipQty
                                                    from ORD_IpDet_8 with(NOLOCK) where type <> 1 and EBELN_EBELP in (?");
                }
                else
                {
                    selectOrderDetailStatement.Append(",?");
                    selectOrderMasterStatement.Append(",?");
                    selectShippedQtyStatement.Append(",?");
                }
                selectOrderDetailParam.Add(scheduleLineInput.EBELN + "&" + scheduleLineInput.EBELP);
                selectOrderMasterParam.Add(scheduleLineInput.EBELN + "&" + scheduleLineInput.EBELP);
                selectShippedQtyParam.Add(scheduleLineInput.EBELN + "&" + scheduleLineInput.EBELP);
            }
            selectOrderDetailStatement.Append(")");
            selectOrderMasterStatement.Append(")");
            selectShippedQtyStatement.Append(") group by EBELN_EBELP");

            IList<OrderDetail> orderDetailList = this.genericMgr.FindEntityWithNativeSql<OrderDetail>(selectOrderDetailStatement.ToString(), selectOrderDetailParam.ToArray());
            IList<OrderMaster> orderMasterList = this.genericMgr.FindEntityWithNativeSql<OrderMaster>(selectOrderMasterStatement.ToString(), selectOrderMasterParam.ToArray());
            IList<object[]> shippedQtyList = this.genericMgr.FindAllWithNativeSql<object[]>(selectShippedQtyStatement.ToString(), selectShippedQtyParam.ToArray());

            BusinessException businessException = new BusinessException();
            foreach (ScheduleLineInput scheduleLineInput in scheduleLineInputList)
            {
                OrderDetail orderDetail = orderDetailList.Where(det => det.ExternalOrderNo == scheduleLineInput.EBELN && det.ExternalSequence == scheduleLineInput.EBELP).Single();
                OrderMaster orderMaster = orderMasterList.Where(mstr => mstr.OrderNo == orderDetail.OrderNo).First();
                decimal shippedQty = shippedQtyList.Where(s => (string)s[0] == scheduleLineInput.EBELN + "&" + scheduleLineInput.EBELP).Select(s => (decimal)s[1]).SingleOrDefault();

                if (shippedQty + scheduleLineInput.ShipQty > scheduleLineInput.SureShipQty)
                {
                    businessException.AddMessage("计划协议号{0}行号{1}的需求数不足。", scheduleLineInput.EBELN, scheduleLineInput.EBELP);
                }

                OrderDetailInput orderDetailInput = new OrderDetailInput();
                orderDetail.AddOrderDetailInput(orderDetailInput);
                shipOrderDetailList.Add(orderDetail);
                orderDetail.CurrentOrderMaster = orderMaster;
                orderDetailInput.ShipQty = scheduleLineInput.ShipQty;
            }

            //计划协议发货判断待发货明细是否存在同一物料号既有寄售合同又有非寄售合同
            shipOrderDetailList.OrderBy(s => s.Item);
            for (int i = 1; i < shipOrderDetailList.Count(); i++)
            {
                if (shipOrderDetailList[i].Item == shipOrderDetailList[i - 1].Item)
                {
                    if (shipOrderDetailList[i].BillTerm != shipOrderDetailList[i - 1].BillTerm)
                    {
                        businessException.AddMessage("物料号{0}发货时存在两种结算状态", shipOrderDetailList[i].Item);
                        break;
                    }
                }
            }

            if (businessException.HasMessage)
            {
                throw businessException;
            }

            FlowMaster flowMaster = this.genericMgr.FindById<FlowMaster>(flow);
            flowMaster.FlowDetails = this.TryLoadFlowDetails(flowMaster, shipOrderDetailList.Select(det => det.Item).ToList());
            IpMaster ipMaster = ShipOrder(flowMaster, shipOrderDetailList, DateTime.Now);
            this.CreateIpDat(ipMaster);
            return ipMaster;
        }
        #endregion

        #region 订单发货
        [Transaction(TransactionMode.Requires)]
        public IpMaster ShipOrder(IList<OrderDetail> orderDetailList)
        {
            return ShipOrder(null, orderDetailList, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public IpMaster ShipOrder(IList<OrderDetail> orderDetailList, DateTime effectiveDate)
        {
            return ShipOrder(null, orderDetailList, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public IpMaster ShipOrder(FlowMaster flowMaster, IList<OrderDetail> orderDetailList, DateTime effectiveDate)
        {
            #region 判断是否全0发货
            if (orderDetailList == null || orderDetailList.Count == 0)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_ShipDetailIsEmpty);
            }

            IList<OrderDetail> nonZeroOrderDetailList = orderDetailList.Where(o => o.ShipQtyInput != 0).ToList();

            if (nonZeroOrderDetailList.Count == 0)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_ShipDetailIsEmpty);
            }
            #endregion

            #region 查询订单头对象
            IList<OrderMaster> orderMasterList = LoadOrderMasters((from det in nonZeroOrderDetailList
                                                                   select det.OrderNo).Distinct().ToArray());
            #endregion

            #region 循环订单头检查
            IList<com.Sconit.CodeMaster.OrderType> orderTypeList = (from orderMaster in orderMasterList
                                                                    group orderMaster by orderMaster.Type into result
                                                                    select result.Key).ToList();

            if (orderTypeList.Count > 1)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_CannotMixOrderTypeShip);
            }

            com.Sconit.CodeMaster.OrderType orderType = orderTypeList.First();

            foreach (OrderMaster orderMaster in orderMasterList)
            {
                orderMaster.OrderDetails = nonZeroOrderDetailList.Where(det => det.OrderNo == orderMaster.OrderNo).ToList();

                //生产单不支持发货
                if (orderMaster.Type == com.Sconit.CodeMaster.OrderType.Production
                    || orderMaster.Type == com.Sconit.CodeMaster.OrderType.SubContract)
                {
                    throw new TechnicalException("Production Order not support ship operation.");
                }

                //如果非生产，把Submit状态改为InProcess
                if (orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.Submit
                    && orderMaster.Type != com.Sconit.CodeMaster.OrderType.Production
                    && orderMaster.Type != com.Sconit.CodeMaster.OrderType.SubContract)
                {
                    UpdateOrderMasterStatus2InProcess(orderMaster);
                }

                //判断OrderHead状态
                if (orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.InProcess)
                {
                    throw new BusinessException(Resources.ORD.OrderMaster.Errors_StatusErrorWhenShip,
                          orderMaster.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
                }

                #region 整包装发货判断,快速的不要判断
                if (orderMaster.IsShipFulfillUC
                    && orderMaster.SubType == com.Sconit.CodeMaster.OrderSubType.Normal
                    && !(orderMaster.IsAutoRelease && orderMaster.IsAutoStart)
                    && orderMaster.Type != CodeMaster.OrderType.ScheduleLine)   //计划协议不检查整包装发货选项。
                {
                    foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                    {
                        foreach (OrderDetailInput orderDetailInput in orderDetail.OrderDetailInputs)
                        {
                            if (orderDetailInput.ShipQty % orderDetail.UnitCount != 0)
                            {
                                //不是整包装
                                throw new BusinessException(Resources.ORD.OrderMaster.Errors_ShipQtyNotFulfillUnitCount, orderDetail.Item, orderDetail.UnitCount.ToString("0.##"));
                            }
                        }
                    }
                }
                #endregion

                #region 非计划协议订单是否过量发货判断,对于允许超发的不做此判断
                if (orderMaster.Type != com.Sconit.CodeMaster.OrderType.ScheduleLine)
                {
                    if (!orderMaster.IsShipExceed)
                    {
                        foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                        {
                            if (!orderMaster.IsOpenOrder)
                            {
                                if (Math.Abs(orderDetail.ShippedQty) >= Math.Abs(orderDetail.OrderedQty))
                                {
                                    //订单的发货数已经大于等于订单数
                                    throw new BusinessException(Resources.ORD.OrderMaster.Errors_ShipQtyExcceedOrderQty, orderDetail.OrderNo, orderDetail.Item);
                                }
                                else if (!orderMaster.IsShipExceed && Math.Abs(orderDetail.ShippedQty + orderDetail.ShipQtyInput) > Math.Abs(orderDetail.OrderedQty))   //不允许过量发货
                                {
                                    //订单的发货数 + 本次发货数大于订单数
                                    throw new BusinessException(Resources.ORD.OrderMaster.Errors_ShipQtyExcceedOrderQty, orderDetail.OrderNo, orderDetail.Item);
                                }
                            }
                        }
                    }
                }
                //计划协议订单是否过量发货的判断逻辑
                else
                {

                }
                #endregion

                #region 按数量发货检查指定供应商寄售库存出库
                //if (!orderMaster.IsShipScanHu)
                //{
                //    foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                //    {
                //        if (!string.IsNullOrWhiteSpace(orderDetail.ManufactureParty))
                //        {
                //            foreach (OrderDetailInput orderDetailInput in orderDetail.OrderDetailInputs)
                //            {
                //                if (!string.IsNullOrWhiteSpace(orderDetailInput.ManufactureParty))
                //                {
                //                    if (orderDetail.ManufactureParty != orderDetailInput.ManufactureParty)
                //                    {
                //                        //发货零件的供应商和和订单明细的指定供应商不一致
                //                        throw new BusinessException("发货零件{0}的供应商{1}和和订单明细的指定供应商{2}不一致。", orderDetail.Item, orderDetailInput.ManufactureParty, orderDetail.ManufactureParty);
                //                    }
                //                }
                //                else
                //                {
                //                    //发货零件的供应商赋值为订单明细的供应商
                //                    orderDetailInput.ManufactureParty = orderDetail.ManufactureParty;
                //                }
                //            }
                //        }
                //    }
                //}
                #endregion
            }
            #endregion

            #region 循环更新订单明细
            foreach (OrderDetail orderDetail in nonZeroOrderDetailList)
            {
                orderDetail.ShippedQty += orderDetail.ShipQtyInput;
                genericMgr.Update(orderDetail);
            }
            #endregion

            #region 发货
            IpMaster ipMaster = flowMaster != null ? ipMgr.TransferFlow2Ip(flowMaster, nonZeroOrderDetailList) : ipMgr.TransferOrder2Ip(orderMasterList);
            ipMgr.CreateIp(ipMaster, effectiveDate);
            #endregion

            #region 自动收货
            AutoReceiveIp(ipMaster, effectiveDate);
            #endregion

            //#region 良品退货单接口（写入中间表）
            //if (ipMaster.PartyTo != null && ipMaster.PartyTo.ToUpper() == "LOC")
            //{
            //    foreach (var detail in ipMaster.IpDetails)
            //    {
            //        this.genericMgr.Create(new YieldReturn
            //                                   {
            //                                       IpNo = ipMaster.IpNo,
            //                                       ArriveTime = ipMaster.ArriveTime,
            //                                       PartyFrom = ipMaster.PartyFrom,
            //                                       PartyTo = ipMaster.PartyTo,
            //                                       Dock = ipMaster.Dock,
            //                                       IpCreateDate = ipMaster.CreateDate,
            //                                       Seq = detail.Sequence.ToString(),
            //                                       Item = detail.Item,
            //                                       ManufactureParty = detail.ManufactureParty,
            //                                       Qty = detail.Qty,
            //                                       IsConsignment = detail.BillTerm != CodeMaster.OrderBillTerm.ReceivingSettlement,// 只有等于收货结算时为false，否则为true
            //                                       CreateDate = DateTime.Now
            //                                   });
            //    }
            //}

            //#endregion

            return ipMaster;
        }

        private long GetSpecifiedStatusOrderCount(OrderMaster orderMaster, CodeMaster.OrderStatus orderStatus)
        {
            string hql = "select count(*) as counter from OrderMaster where Type = ? and Flow = ? and Status = ? and Sequence <= ? and IsPause = ?";
            return this.genericMgr.FindAll<long>(hql, new object[] { orderMaster.Type, orderMaster.Flow, orderStatus, orderMaster.Sequence, false })[0];
        }

        private void CheckKitOrderDetail(OrderMaster orderMaster, bool isCheckKitTraceItem, bool isShip)
        {
            BusinessException businessException = new BusinessException();

            #region 明细行是否收/发货判断
            IList<OrderDetail> unReceivedOrderDetailList = LoadExceptOrderDetail(orderMaster);
            if (unReceivedOrderDetailList != null && unReceivedOrderDetailList.Count > 0)
            {
                foreach (OrderDetail unReceivedOrderDetail in unReceivedOrderDetailList)
                {
                    if (isShip)
                    {
                        businessException.AddMessage("KIT单{0}行号{1}零件号{2}没有发货。", orderMaster.OrderNo, unReceivedOrderDetail.Sequence.ToString(), unReceivedOrderDetail.Item);
                    }
                    else
                    {
                        businessException.AddMessage("KIT单{0}行号{1}零件号{2}没有收货。", orderMaster.OrderNo, unReceivedOrderDetail.Sequence.ToString(), unReceivedOrderDetail.Item);
                    }
                }
            }
            #endregion

            #region 收/发货数是否等于订单数判断
            foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
            {
                if (isShip)
                {
                    if (orderDetail.OrderedQty != orderDetail.ShipQtyInput)
                    {
                        businessException.AddMessage("KIT单{0}行号{1}零件号{2}的发货数和订单数不一致。", orderMaster.OrderNo, orderDetail.Sequence.ToString(), orderDetail.Item);
                    }
                }
                else
                {
                    if (orderDetail.OrderedQty != orderDetail.ReceiveQtyInput)
                    {
                        businessException.AddMessage("KIT单{0}行号{1}零件号{2}的收货数和订单数不一致。", orderMaster.OrderNo, orderDetail.Sequence.ToString(), orderDetail.Item);
                    }
                }
            }
            #endregion

            #region KIT中的关键件是否扫描
            if (isCheckKitTraceItem)
            {
                foreach (OrderDetail orderDetail in orderMaster.OrderDetails.Where(o => o.IsScanHu))
                {
                    if (orderDetail.OrderedQty != orderDetail.OrderDetailInputs.Where(o => !string.IsNullOrWhiteSpace(o.HuId)).Count())
                    {
                        businessException.AddMessage("KIT单{0}行号{1}的关键零件{1}没有扫描。", orderMaster.OrderNo, orderDetail.Sequence.ToString(), orderDetail.Item);
                    }
                }
            }
            #endregion

            if (businessException.HasMessage)
            {
                throw businessException;
            }
        }

        private void CheckKitIpDetail(IpMaster ipMaster, bool isCheckKitTraceItem)
        {
            BusinessException businessException = new BusinessException();

            #region 明细行是否收/发货判断
            IList<IpDetail> unReceivedIpDetailList = LoadExceptIpDetails(ipMaster.IpNo, ipMaster.IpDetails.Select(det => det.Id).ToArray());
            if (unReceivedIpDetailList != null && unReceivedIpDetailList.Count > 0)
            {
                foreach (IpDetail unReceivedIpDetail in unReceivedIpDetailList)
                {
                    businessException.AddMessage("KIT送货单{0}行号{1}零件号{2}没有收货。", ipMaster.IpNo, unReceivedIpDetail.Sequence.ToString(), unReceivedIpDetail.Item);
                }
            }
            #endregion

            #region 收/发货数是否等于订单数判断
            foreach (IpDetail ipDetail in ipMaster.IpDetails)
            {
                if (ipDetail.Qty != ipDetail.ReceiveQtyInput)
                {
                    businessException.AddMessage("KIT送货单{0}行号{1}零件号{2}的收货数和送货数不一致。", ipMaster.IpNo, ipDetail.Sequence.ToString(), ipDetail.Item);
                }
            }
            #endregion

            #region KIT中的关键件是否扫描
            if (isCheckKitTraceItem)
            {
                foreach (IpDetail ipDetail in ipMaster.IpDetails.Where(o => o.IsScanHu))
                {
                    if (ipDetail.Qty != ipDetail.IpDetailInputs.Where(o => !string.IsNullOrWhiteSpace(o.HuId)).Count())
                    {
                        businessException.AddMessage("KIT送货单{0}行号{1}的关键零件{1}没有扫描。", ipMaster.IpNo, ipDetail.Sequence.ToString(), ipDetail.Item);
                    }
                }
            }
            #endregion

            if (businessException.HasMessage)
            {
                throw businessException;
            }
        }
        #endregion

        #region 捡货单发货
        [Transaction(TransactionMode.Requires)]
        public ReceiptMaster ShipPickList(string pickListNo, string[] pickListDetailIdArray, string[] shipQtyArray)
        {
            if (pickListDetailIdArray == null || shipQtyArray == null)
            {
                throw new TechnicalException("拣货明细Id或发货数不能为空。");
            }

            if (pickListDetailIdArray.Length != shipQtyArray.Length)
            {
                throw new TechnicalException("拣货明细Id和发货数不匹配。");
            }

            PickListMaster pickListMaster = this.genericMgr.FindAll<PickListMaster>("from PickListMaster where PickListNo = ?", pickListNo).SingleOrDefault();

            if (pickListMaster == null)
            {
                throw new BusinessException("拣货单号{0}不存在。", pickListNo);
            }

            #region 获取捡货单明细
            TryLoadPickListDetails(pickListMaster);
            #endregion

            #region 获取订单明细
            IList<OrderDetail> orderDetailList = TryLoadOrderDetails(pickListMaster);
            #endregion

            #region 获取订单头
            //IList<OrderMaster> orderMasterList = LoadOrderMasters(orderDetailList.Select(det => det.OrderNo).Distinct().ToArray());
            #endregion

            #region 更新捡货单明细
            //2013-10-17,存储过程只考虑明细上的是否关闭标志,拣货单只能一次发货,该拣货单只要执行发货所有的明细行都要关闭
            foreach (PickListDetail pickListDetail in pickListMaster.PickListDetails)
            {
                pickListDetail.IsClose = true;
                for (int i = 0; i < pickListDetailIdArray.Length; i++)
                {
                    if (int.Parse(pickListDetailIdArray[i]) == pickListDetail.Id)
                    {
                        pickListDetail.PickedQty = decimal.Parse(shipQtyArray[i]);
                        pickListDetail.CurrentOrderDetail = orderDetailList.Where(d => d.Id == pickListDetail.OrderDetailId).Single();
                    }
                }
                this.genericMgr.Update(pickListDetail);
            }
            #endregion

            #region 更新捡货单头
            pickListMaster.Status = CodeMaster.PickListStatus.Close;
            pickListMaster.CloseDate = DateTime.Now;
            pickListMaster.CloseUserId = SecurityContextHolder.Get().Id;
            pickListMaster.CloseUserName = SecurityContextHolder.Get().FullName;

            this.genericMgr.Update(pickListMaster);
            #endregion

            #region 更新订单明细的捡货数和发货数
            foreach (OrderDetail orderDetail in orderDetailList)
            {
                orderDetail.PickedQty -= pickListMaster.PickListDetails.Where(p => p.OrderDetailId == orderDetail.Id).Sum(p => p.Qty);
                //orderDetail.ShippedQty += pickListMaster.PickListDetails.Where(p => p.OrderDetailId == orderDetail.Id).Sum(p => p.PickedQty);
                //this.genericMgr.Update(orderDetail);

                IList<PickListDetail> pickListDetailList = pickListMaster.PickListDetails.Where(p => p.OrderDetailId == orderDetail.Id).ToList();
                foreach (PickListDetail pickListDetail in pickListDetailList)
                {
                    OrderDetailInput orderDetailInput = new OrderDetailInput();
                    orderDetailInput.ReceiveQty = pickListDetail.PickedQty;
                    orderDetailInput.ConsignmentParty = pickListDetail.ConsignmentSupplier;
                    orderDetail.AddOrderDetailInput(orderDetailInput);
                }
            }
            #endregion

            #region 发货/收货
            return this.ReceiveOrder(orderDetailList);
            #endregion
        }

        [Transaction(TransactionMode.Requires)]
        public IpMaster ShipPickList(string pickListNo)
        {
            return ShipPickList(this.genericMgr.FindById<PickListMaster>(pickListNo), DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public IpMaster ShipPickList(string pickListNo, DateTime effectiveDate)
        {
            return ShipPickList(this.genericMgr.FindById<PickListMaster>(pickListNo), effectiveDate);
        }

        [Transaction(TransactionMode.Requires)]
        public IpMaster ShipPickList(PickListMaster pickListMaster)
        {
            return ShipPickList(pickListMaster, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public IpMaster ShipPickList(IList<PickListDetail> pickListDetailList)
        {
            return ShipPickList(pickListDetailList, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public IpMaster ShipPickList(IList<PickListDetail> pickListDetailList, DateTime effectiveDate)
        {
            var pickListNoList = pickListDetailList.Select(p => p.PickListNo).Distinct();
            if (pickListNoList.Count() > 1)
            {
                throw new BusinessException("多个拣货单不能同时发货");
            }
            string pickListNo = pickListNoList.First();
            PickListMaster pickListMaster = genericMgr.FindById<PickListMaster>(pickListNo);
            return ShipPickList(pickListMaster, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public IpMaster ShipPickList(PickListMaster pickListMaster, DateTime effectiveDate)
        {
            TryLoadPickListResults(pickListMaster);
            if (pickListMaster.PickListResults == null || pickListMaster.PickListResults.Count == 0)
            {
                throw new BusinessException("捡货明细不能为空。");
            }

            #region 获取捡货单明细
            TryLoadPickListDetails(pickListMaster);
            #endregion

            #region 获取订单明细
            IList<OrderDetail> orderDetailList = TryLoadOrderDetails(pickListMaster);
            #endregion

            #region 获取订单头
            IList<OrderMaster> orderMasterList = LoadOrderMasters(orderDetailList.Select(det => det.OrderNo).Distinct().ToArray());
            #endregion

            #region 更新捡货单明细
            foreach (PickListDetail pickListDetail in pickListMaster.PickListDetails)
            {
                pickListDetail.IsClose = true;
                this.genericMgr.Update(pickListDetail);
            }
            #endregion

            #region 更新订单头
            foreach (OrderMaster orderMaster in orderMasterList)
            {
                if (orderMaster.Status == CodeMaster.OrderStatus.Submit)
                {
                    UpdateOrderMasterStatus2InProcess(orderMaster);
                }
            }
            #endregion

            #region 更新订单明细的捡货数和发货数
            foreach (OrderDetail orderDetail in orderDetailList)
            {
                orderDetail.PickedQty -= pickListMaster.PickListDetails.Where(p => p.OrderDetailId == orderDetail.Id).Sum(p => p.Qty);
                orderDetail.ShippedQty += pickListMaster.PickListResults.Where(p => p.OrderDetailId == orderDetail.Id).Sum(p => p.Qty);
                this.genericMgr.Update(orderDetail);
            }
            #endregion

            #region 更新捡货单头
            pickListMaster.Status = CodeMaster.PickListStatus.Close;
            pickListMaster.CloseDate = DateTime.Now;
            pickListMaster.CloseUserId = SecurityContextHolder.Get().Id;
            pickListMaster.CloseUserName = SecurityContextHolder.Get().FullName;

            this.genericMgr.Update(pickListMaster);
            #endregion

            #region 发货
            IpMaster ipMaster = ipMgr.TransferPickList2Ip(pickListMaster);
            ipMgr.CreateIp(ipMaster, effectiveDate);
            #endregion

            #region 自动收货
            AutoReceiveIp(ipMaster, effectiveDate);
            #endregion

            return ipMaster;
        }
        #endregion

        #region 订单收货
        [Transaction(TransactionMode.Requires)]
        public ReceiptMaster ReceiveOrder(IList<OrderDetail> orderDetailList)
        {
            return ReceiveOrder(orderDetailList, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public ReceiptMaster ReceiveOrder(IList<OrderDetail> orderDetailList, DateTime effectiveDate)
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
            #endregion

            #region 查询订单头对象
            IList<OrderMaster> orderMasterList = LoadOrderMasters((from det in nonZeroOrderDetailList
                                                                   //where !string.IsNullOrWhiteSpace(det.OrderNo)   todo支持无订单收货
                                                                   select det.OrderNo).Distinct().ToArray());
            #endregion

            //#region 按订单明细汇总发货数，按条码发货一条订单明细会对应多条发货记录
            //var summaryOrderDet = from det in orderDetailList
            //                      group det by new { Id = det.Id, OrderNo = det.OrderNo } into g
            //                      select new
            //                      {
            //                          Id = g.Key.Id,
            //                          OrderNo = g.Key.OrderNo,
            //                          ReceiveQty = g.Sum(det => det.CurrentReceiveQty),
            //                          RejectQty = g.Sum(det => det.CurrentRejectQty)
            //                      };
            //#endregion

            #region 获取收货订单类型
            IList<com.Sconit.CodeMaster.OrderType> orderTypeList = (from orderMaster in orderMasterList
                                                                    group orderMaster by orderMaster.Type into result
                                                                    select result.Key).ToList();

            if (orderTypeList.Count > 1)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_CannotMixOrderTypeReceive);
            }

            com.Sconit.CodeMaster.OrderType orderType = orderTypeList.First();
            #endregion

            #region 计划协议不能按收货校验
            if (orderType == CodeMaster.OrderType.ScheduleLine)
            {
                throw new BusinessException("计划协议不能按订单收货。");
            }
            #endregion

            #region 循环订单头检查
            foreach (OrderMaster orderMaster in orderMasterList)
            {
                orderMaster.OrderDetails = nonZeroOrderDetailList.Where(det => det.OrderNo == orderMaster.OrderNo).ToList();

                //如果非生产，把Submit状态改为InProcess
                if (orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.Submit
                    && orderMaster.Type != com.Sconit.CodeMaster.OrderType.Production
                    && orderMaster.Type != com.Sconit.CodeMaster.OrderType.SubContract)
                {
                    UpdateOrderMasterStatus2InProcess(orderMaster);
                }

                //判断OrderHead状态
                if (orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.InProcess)
                {
                    throw new BusinessException(Resources.ORD.OrderMaster.Errors_StatusErrorWhenReceive,
                            orderMaster.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
                }

                #region 订单、生产线暂停检查
                if (orderMaster.PauseStatus == CodeMaster.PauseStatus.Paused)
                {
                    if (orderMaster.Type == CodeMaster.OrderType.Production)
                    {
                        throw new BusinessException("生产单{0}已经暂停，不能收货。", orderMaster.OrderNo);
                    }
                    else
                    {
                        throw new BusinessException("订单{0}已经暂停，不能收货。", orderMaster.OrderNo);
                    }
                }

                if ((orderMaster.Type == CodeMaster.OrderType.Production
                    || orderMaster.Type == CodeMaster.OrderType.SubContract
                    )
                    && !string.IsNullOrWhiteSpace(orderMaster.Flow))
                {
                    FlowMaster flowMaster = this.genericMgr.FindById<FlowMaster>(orderMaster.Flow);
                    if (flowMaster.IsPause)
                    {
                        throw new BusinessException("生产线{0}已经暂停，不能收货。", orderMaster.Flow);
                    }
                }
                #endregion

                #region 整包装收货判断,快速的不要判断
                if (orderMaster.IsReceiveFulfillUC
                    && orderMaster.SubType == com.Sconit.CodeMaster.OrderSubType.Normal
                    && !(orderMaster.IsAutoRelease && orderMaster.IsAutoStart))
                {
                    foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                    {
                        //不合格品不作为收货数
                        //if (orderDetail.ReceiveQualifiedQtyInput % orderDetail.UnitCount != 0)
                        if (orderDetail.ReceiveQtyInput % orderDetail.UnitCount != 0)
                        {
                            //不是整包装
                            throw new BusinessException(Resources.ORD.OrderMaster.Errors_ReceiveQtyNotFulfillUnitCount, orderDetail.Item);
                        }
                    }
                }
                #endregion

                #region 是否过量发货判断，未发货即收货也要判断是否过量发货
                foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                {
                    if (!orderMaster.IsOpenOrder)
                    {
                        if (Math.Abs(orderDetail.ShippedQty) > Math.Abs(orderDetail.OrderedQty))
                        {
                            //订单的发货数已经大于等于订单数
                            throw new BusinessException(Resources.ORD.OrderMaster.Errors_ShipQtyExcceedOrderQty, orderDetail.OrderNo, orderDetail.Item);
                        }
                        else if (orderMaster.Type != com.Sconit.CodeMaster.OrderType.Production   //生产和委外不需要判断
                           && orderMaster.Type != com.Sconit.CodeMaster.OrderType.SubContract
                           && !orderMaster.IsShipExceed
                            && Math.Abs(orderDetail.ShippedQty + orderDetail.ReceiveQtyInput) > Math.Abs(orderDetail.OrderedQty))   //不允许过量收货
                        {
                            //订单的发货数 + 本次发货数大于订单数
                            throw new BusinessException(Resources.ORD.OrderMaster.Errors_ShipQtyExcceedOrderQty, orderDetail.OrderNo, orderDetail.Item);
                        }
                    }
                }
                #endregion

                #region 是否过量收货判断
                foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                {
                    if (!orderMaster.IsOpenOrder)
                    {
                        //订单的收货数已经大于等于订单数
                        if (Math.Abs(orderDetail.ReceivedQty) >= Math.Abs(orderDetail.OrderedQty))
                        {
                            throw new BusinessException(Resources.ORD.OrderMaster.Errors_ReceiveQtyExcceedOrderQty, orderDetail.OrderNo, orderDetail.Item);
                        }
                        //sih要求，废品数和不合格品数不占用入库数量
                        else if (!orderMaster.IsReceiveExceed && Math.Abs(orderDetail.ReceivedQty + orderDetail.ReceiveQtyInput) > Math.Abs(orderDetail.OrderedQty))   //不允许过量收货
                        {
                            //订单的收货数 + 本次收货数大于订单数
                            throw new BusinessException(Resources.ORD.OrderMaster.Errors_ReceiveQtyExcceedOrderQty, orderDetail.OrderNo, orderDetail.Item);
                        }
                        //else if (!orderMaster.IsReceiveExceed && Math.Abs(orderDetail.ReceivedQty + orderDetail.RejectedQty + orderDetail.ScrapQty + orderDetail.ReceiveQtyInput + orderDetail.ScrapQtyInput) > Math.Abs(orderDetail.OrderedQty))   //不允许过量收货
                        //{
                        //    //订单的收货数 + 本次收货数大于订单数
                        //    throw new BusinessException(Resources.ORD.OrderMaster.Errors_ReceiveQtyExcceedOrderQty, orderDetail.OrderNo, orderDetail.Item);
                        //}
                    }
                }
                #endregion

                #region 采购收货是否有价格单判断
                //if (orderHead.Type == BusinessConstants.CODE_MASTER_ORDER_TYPE_VALUE_PROCUREMENT
                //    && !bool.Parse(entityPreference.Value))
                //{
                //    if (orderDetail.UnitPrice == Decimal.Zero)
                //    {
                //        //重新查找一次价格
                //        PriceListDetail priceListDetail = priceListDetailMgrE.GetLastestPriceListDetail(
                //            orderDetail.DefaultPriceList,
                //            orderDetail.Item,
                //            orderHead.StartTime,
                //            orderHead.Currency,
                //            orderDetail.Uom);

                //        if (priceListDetail != null)
                //        {
                //            orderDetail.UnitPrice = priceListDetail.UnitPrice;
                //            orderDetail.IsProvisionalEstimate = priceListDetail.IsProvisionalEstimate;
                //            orderDetail.IsIncludeTax = priceListDetail.IsIncludeTax;
                //            orderDetail.TaxCode = priceListDetail.TaxCode;
                //        }
                //        else
                //        {
                //            throw new BusinessErrorException("Order.Error.NoPriceListReceipt", orderDetail.Item.Code);
                //        }
                //    }
                //}
                #endregion

                #region 采购收货是否有计划协议行或PO号
                if (orderMaster.Type == com.Sconit.CodeMaster.OrderType.Procurement)
                {
                    foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                    {
                        if (string.IsNullOrWhiteSpace(orderDetail.ExternalOrderNo)
                            || string.IsNullOrWhiteSpace(orderDetail.ExternalSequence))
                        {
                            throw new BusinessException("供应商直供要货单{0}物料号{1}的计划协议号或PO号不能为空。", orderDetail.OrderNo, orderDetail.Item);
                        }
                    }
                }
                #endregion
            }
            #endregion

            #region 循环更新订单明细
            foreach (OrderDetail orderDetail in nonZeroOrderDetailList)
            {
                //未发货直接收货需要累加已发货数量
                if (orderDetail.OrderType != com.Sconit.CodeMaster.OrderType.Production
                    && orderDetail.OrderType != com.Sconit.CodeMaster.OrderType.SubContract)
                {
                    orderDetail.ShippedQty += orderDetail.ReceiveQtyInput;
                }

                if (orderDetail.OrderType != com.Sconit.CodeMaster.OrderType.Production
                    && orderDetail.OrderType != com.Sconit.CodeMaster.OrderType.SubContract)
                {
                    //orderDetail.ReceivedQty += orderDetail.ReceiveQualifiedQtyInput;
                    orderDetail.ReceivedQty += orderDetail.ReceiveQtyInput;
                }
                else
                {
                    //生产收货更新合格数和不合格数量
                    //orderDetail.ReceivedQty += orderDetail.ReceiveQualifiedQtyInput;
                    orderDetail.ReceivedQty += orderDetail.ReceiveQtyInput;
                    orderDetail.ScrapQty += orderDetail.ScrapQtyInput;
                }
                genericMgr.Update(orderDetail);
            }
            #endregion

            #region 发货
            #region OrderDetailInput赋发货数量
            foreach (OrderDetail orderDetail in nonZeroOrderDetailList)
            {
                foreach (OrderDetailInput orderDetailInput in orderDetail.OrderDetailInputs)
                {
                    orderDetailInput.ShipQty = orderDetailInput.ReceiveQty;
                }
            }
            #endregion

            IpMaster ipMaster = null;
            if (orderType != com.Sconit.CodeMaster.OrderType.Production
                && orderType != com.Sconit.CodeMaster.OrderType.SubContract)
            {
                ipMaster = this.ipMgr.TransferOrder2Ip(orderMasterList);

                #region 循环发货
                foreach (IpDetail ipDetail in ipMaster.IpDetails)
                {
                    ipDetail.CurrentPartyFrom = ipMaster.PartyFrom;  //为了记录库存事务
                    ipDetail.CurrentPartyFromName = ipMaster.PartyFromName;  //为了记录库存事务
                    ipDetail.CurrentPartyTo = ipMaster.PartyTo;      //为了记录库存事务
                    ipDetail.CurrentPartyToName = ipMaster.PartyToName;      //为了记录库存事务
                    //ipDetail.CurrentOccupyType = com.Sconit.CodeMaster.OccupyType.None; //todo-默认出库未占用库存，除非捡货或检验的出库

                    IList<InventoryTransaction> inventoryTransactionList = this.locationDetailMgr.InventoryOut(ipDetail, effectiveDate);

                    #region 建立发货库明细和IpDetailInput的关系
                    if (inventoryTransactionList != null && inventoryTransactionList.Count > 0)
                    {
                        ipDetail.IpLocationDetails = (from trans in inventoryTransactionList
                                                      group trans by new
                                                      {
                                                          HuId = trans.HuId,
                                                          LotNo = trans.LotNo,
                                                          IsCreatePlanBill = trans.IsCreatePlanBill,
                                                          IsConsignment = trans.IsConsignment,
                                                          PlanBill = trans.PlanBill,
                                                          ActingBill = trans.ActingBill,
                                                          QualityType = trans.QualityType,
                                                          IsFreeze = trans.IsFreeze,
                                                          IsATP = trans.IsATP,
                                                          OccupyType = trans.OccupyType,
                                                          OccupyReferenceNo = trans.OccupyReferenceNo
                                                      } into g
                                                      select new IpLocationDetail
                                                      {
                                                          HuId = g.Key.HuId,
                                                          LotNo = g.Key.LotNo,
                                                          IsCreatePlanBill = g.Key.IsCreatePlanBill,
                                                          IsConsignment = g.Key.IsConsignment,
                                                          PlanBill = g.Key.PlanBill,
                                                          ActingBill = g.Key.ActingBill,
                                                          QualityType = g.Key.QualityType,
                                                          IsFreeze = g.Key.IsFreeze,
                                                          IsATP = g.Key.IsATP,
                                                          OccupyType = g.Key.OccupyType,
                                                          OccupyReferenceNo = g.Key.OccupyReferenceNo,
                                                          Qty = g.Sum(t => -t.Qty),                      //发货的库存事务为负数，转为收货数应该取负数
                                                          //PlanBillQty = g.Sum(t=>-t.PlanBillQty),
                                                          //ActingBillQty = g.Sum(t=>-t.ActingBillQty)
                                                      }).ToList();
                    }
                    #endregion
                }
                #endregion

                #region 生成收货的IpInput
                foreach (OrderDetail orderDetail in nonZeroOrderDetailList)
                {
                    OrderMaster orderMaster = orderMasterList.Where(o => o.OrderNo == orderDetail.OrderNo).Single();
                    //订单收货一定是一条订单明细对应一条发货单明细
                    IpDetail ipDetail = ipMaster.IpDetails.Where(det => det.OrderDetailId == orderDetail.Id).Single();
                    ipDetail.IpDetailInputs = null;  //清空Ip的发货数据，准备添加收货数据

                    foreach (OrderDetailInput orderDetailInput in orderDetail.OrderDetailInputs)
                    {
                        IpDetailInput ipDetailInput = new IpDetailInput();
                        ipDetailInput.ReceiveQty = orderDetailInput.ShipQty;
                        if (orderMaster.IsReceiveScanHu)
                        {
                            ipDetailInput.HuId = orderDetailInput.HuId;
                            ipDetailInput.LotNo = orderDetailInput.LotNo;
                        }

                        ipDetail.AddIpDetailInput(ipDetailInput);
                    }
                }

                #region 订单生成收货Input
                if (ipMaster.IsShipScanHu && ipMaster.IsReceiveScanHu)
                {
                    #region 按条码匹配
                    foreach (IpDetail ipDetail in ipMaster.IpDetails)
                    {
                        foreach (IpDetailInput ipDetailInput in ipDetail.IpDetailInputs)
                        {
                            IpLocationDetail matchedIpLocationDetail = ipDetail.IpLocationDetails.Where(locDet => locDet.HuId == ipDetailInput.HuId).SingleOrDefault();
                            matchedIpLocationDetail.ReceivedQty = matchedIpLocationDetail.Qty;
                            if (matchedIpLocationDetail != null)
                            {
                                ipDetailInput.AddReceivedIpLocationDetail(matchedIpLocationDetail);
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    #region 按数量匹配
                    foreach (IpDetail ipDetail in ipMaster.IpDetails)
                    {
                        CycleMatchIpDetailInput(ipDetail, ipDetail.IpDetailInputs, ipDetail.IpLocationDetails);
                    }
                    #endregion
                }
                #endregion
                #endregion
            }
            #endregion

            #region 收货
            ReceiptMaster receiptMaster = null;
            if (orderType == com.Sconit.CodeMaster.OrderType.Production
               || orderType == com.Sconit.CodeMaster.OrderType.SubContract)
            {
                #region 生产收货
                if (orderMasterList.Count > 1)
                {
                    throw new TechnicalException("生产单不能合并收货。");
                }

                foreach (OrderMaster orderMaster in orderMasterList)
                {
                    receiptMaster = this.receiptMgr.TransferOrder2Receipt(orderMaster);
                    this.receiptMgr.CreateReceipt(receiptMaster);

                    //在报工的时候反冲物料，收货时不反冲
                    //foreach (OrderDetail orderDetail in nonZeroOrderDetailList)
                    //{
                    //    OrderDetailInput orderDetailInput = orderDetail.OrderDetailInputs[0];
                    //    orderDetailInput.ReceiptDetail = receiptMaster.ReceiptDetails.Where(r => r.OrderDetailId == orderDetail.Id).Single();
                    //}
                    //this.productionLineMgr.BackflushProductOrder(nonZeroOrderDetailList);
                }
                #endregion
            }
            else if (orderType != com.Sconit.CodeMaster.OrderType.Production
                && orderType != com.Sconit.CodeMaster.OrderType.SubContract)
            {
                #region 物流收货
                receiptMaster = this.receiptMgr.TransferIp2Receipt(ipMaster);
                this.receiptMgr.CreateReceipt(receiptMaster, false, effectiveDate);
                #endregion
            }
            #endregion

            #region 尝试关闭订单
            foreach (OrderMaster orderMaster in orderMasterList)
            {
                if (orderMaster.Type == CodeMaster.OrderType.Production)
                {
                    #region 生产
                    TryCloseOrder(orderMaster);
                    #endregion
                }
                else
                {
                    #region 物流
                    TryCloseOrder(orderMaster);
                    #endregion
                }
            }
            #endregion

            return receiptMaster;
        }

        public void MakeupReceiveOrder()
        {
            IList<OrderMaster> orderMasterList = this.genericMgr.FindEntityWithNativeSql<OrderMaster>(@"select * from ORD_OrderMstr_2 where OrderNo in (
                                                                                                select ord.OrderNo from ORD_OrderDet_2 as ord
                                                                                                left join ORD_RecDet_2 as rec on ord.Id = rec.OrderDetId
                                                                                                where ord.RecQty <> 0 and rec.Id is null)");

            foreach (OrderMaster orderMaster in orderMasterList)
            {
                SecurityContextHolder.Set(this.genericMgr.FindById<User>(orderMaster.CreateUserId));
                try
                {
                    IList<OrderDetail> orderDetailList = this.genericMgr.FindEntityWithNativeSql<OrderDetail>(@"select * from ORD_OrderDet_2 where OrderNo = ?", orderMaster.OrderNo);
                    orderMaster.OrderDetails = orderDetailList;
                    #region 发货
                    #region OrderDetailInput赋发货数量
                    foreach (OrderDetail orderDetail in orderDetailList)
                    {
                        OrderDetailInput orderDetailInput = new OrderDetailInput();
                        orderDetailInput.ShipQty = orderDetail.OrderedQty;
                        orderDetailInput.ReceiveQty = orderDetail.OrderedQty;
                        orderDetail.AddOrderDetailInput(orderDetailInput);
                    }
                    #endregion

                    IpMaster ipMaster = null;
                    IList<OrderMaster> omList = new List<OrderMaster>();
                    omList.Add(orderMaster);
                    ipMaster = this.ipMgr.TransferOrder2Ip(omList);

                    #region 循环发货
                    foreach (IpDetail ipDetail in ipMaster.IpDetails)
                    {
                        ipDetail.CurrentPartyFrom = ipMaster.PartyFrom;  //为了记录库存事务
                        ipDetail.CurrentPartyFromName = ipMaster.PartyFromName;  //为了记录库存事务
                        ipDetail.CurrentPartyTo = ipMaster.PartyTo;      //为了记录库存事务
                        ipDetail.CurrentPartyToName = ipMaster.PartyToName;      //为了记录库存事务
                        OrderDetail orderDetail = orderDetailList.Where(det => det.Id == ipDetail.OrderDetailId).Single();

                        #region 建立发货库明细和IpDetailInput的关系
                        ipDetail.IpLocationDetails = new List<IpLocationDetail>();
                        IpLocationDetail ipLocationDetail = new IpLocationDetail();
                        ipLocationDetail.HuId = null;
                        ipLocationDetail.LotNo = null;
                        ipLocationDetail.IsCreatePlanBill = false;
                        ipLocationDetail.IsConsignment = false;
                        ipLocationDetail.PlanBill = null;
                        ipLocationDetail.ActingBill = null;
                        ipLocationDetail.QualityType = CodeMaster.QualityType.Qualified;
                        ipLocationDetail.IsFreeze = false;
                        ipLocationDetail.IsATP = true;
                        ipLocationDetail.OccupyType = CodeMaster.OccupyType.None;
                        ipLocationDetail.OccupyReferenceNo = null;
                        ipLocationDetail.Qty = orderDetail.OrderedQty;

                        ipDetail.AddIpLocationDetail(ipLocationDetail);
                        #endregion
                    }
                    #endregion

                    #region 生成收货的IpInput
                    foreach (OrderDetail orderDetail in orderDetailList)
                    {
                        //订单收货一定是一条订单明细对应一条发货单明细
                        IpDetail ipDetail = ipMaster.IpDetails.Where(det => det.OrderDetailId == orderDetail.Id).Single();
                        ipDetail.IpDetailInputs = null;  //清空Ip的发货数据，准备添加收货数据

                        foreach (OrderDetailInput orderDetailInput in orderDetail.OrderDetailInputs)
                        {
                            IpDetailInput ipDetailInput = new IpDetailInput();
                            ipDetailInput.ReceiveQty = orderDetailInput.ShipQty;
                            ipDetail.AddIpDetailInput(ipDetailInput);
                        }
                    }

                    foreach (IpDetail ipDetail in ipMaster.IpDetails)
                    {
                        CycleMatchIpDetailInput(ipDetail, ipDetail.IpDetailInputs, ipDetail.IpLocationDetails);
                    }
                    #endregion
                    #endregion

                    #region 收货
                    ReceiptMaster receiptMaster = null;
                    receiptMaster = this.receiptMgr.TransferIp2Receipt(ipMaster);
                    this.receiptMgr.CreateReceipt(receiptMaster, false, orderMaster.CreateDate);
                    #endregion
                    this.genericMgr.FlushSession();
                }
                catch (Exception ex)
                {
                    this.genericMgr.CleanSession();
                    log.Error(ex);
                }
            }
        }

        private long GetReleasedOrStartedOrderCount(OrderMaster orderMaster)
        {
            string hql = "select count(*) as counter from OrderMaster where Type = ? and Flow = ? and Status in (?, ?) and Sequence <= ? and IsPause = ?";
            return this.genericMgr.FindAll<long>(hql, new object[] { orderMaster.Type, orderMaster.Flow, CodeMaster.OrderStatus.Submit, CodeMaster.OrderStatus.InProcess, orderMaster.Sequence, false })[0];
        }
        #endregion

        #region 送货单收货
        [Transaction(TransactionMode.Requires)]
        public ReceiptMaster ReceiveIp(IList<IpDetail> ipDetailList)
        {
            return ReceiveIp(ipDetailList, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public ReceiptMaster ReceiveIp(IList<IpDetail> ipDetailList, DateTime effectiveDate)
        {
            return ReceiveIp(ipDetailList, true, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public ReceiptMaster ReceiveIp(IList<IpDetail> ipDetailList, bool isCheckKitTraceItem, DateTime effectiveDate)
        {
            #region 判断送货单是否合并收货
            if ((from det in ipDetailList select det.IpNo).Distinct().Count() > 1)
            {
                throw new TechnicalException("送货单不能合并收货。");
            }
            #endregion

            #region 判断是否全0发货
            if (ipDetailList == null || ipDetailList.Count == 0)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_ReceiveDetailIsEmpty);
            }

            IList<IpDetail> nonZeroIpDetailList = ipDetailList.Where(o => o.ReceiveQtyInput != 0).ToList();

            if (nonZeroIpDetailList.Count == 0)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_ReceiveDetailIsEmpty);
            }
            #endregion

            #region 查询送货单头对象
            string ipNo = (from det in nonZeroIpDetailList select det.IpNo).Distinct().Single();
            IpMaster ipMaster = this.genericMgr.FindById<IpMaster>(ipNo);
            ipMaster.IpDetails = nonZeroIpDetailList;
            #endregion

            #region 查询订单头对象
            IList<OrderMaster> orderMasterList = LoadOrderMasters((from det in nonZeroIpDetailList
                                                                   select det.OrderNo).Distinct().ToArray());
            #endregion

            #region 获取收货订单类型
            IList<com.Sconit.CodeMaster.OrderType> orderTypeList = (from orderMaster in orderMasterList
                                                                    group orderMaster by orderMaster.Type into result
                                                                    select result.Key).ToList();

            if (orderTypeList.Count > 1)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_CannotMixOrderTypeReceive);
            }

            com.Sconit.CodeMaster.OrderType orderType = orderTypeList.First();
            #endregion

            #region 查询订单明细对象
            IList<OrderDetail> orderDetailList = LoadOrderDetails((from det in nonZeroIpDetailList
                                                                   where det.OrderDetailId.HasValue
                                                                   select det.OrderDetailId.Value).Distinct().ToArray());
            #endregion

            #region 查询送货单库存对象
            IList<IpLocationDetail> ipLocationDetailList = LoadIpLocationDetails((from det in nonZeroIpDetailList
                                                                                  select det.Id).ToArray());
            #endregion

            #region 循环检查发货明细
            foreach (IpDetail ipDetail in nonZeroIpDetailList)
            {
                #region 整包装收货判断
                if (ipMaster.IsReceiveFulfillUC)
                {
                    //不合格品不作为收货数
                    if (ipDetail.ReceiveQtyInput % ipDetail.UnitCount != 0)
                    {
                        //不是整包装
                        throw new BusinessException(Resources.ORD.OrderMaster.Errors_ReceiveQtyNotFulfillUnitCount, ipDetail.Item, ipDetail.UnitCount.ToString());
                    }
                }
                #endregion

                #region 是否过量收货判断
                if (orderType != CodeMaster.OrderType.ScheduleLine)
                {
                    if (Math.Abs(ipDetail.ReceivedQty) >= Math.Abs(ipDetail.Qty))
                    {
                        //送货单的收货数已经大于等于发货数
                        throw new BusinessException(Resources.ORD.IpMaster.Errors_ReceiveQtyExcceedOrderQty, ipMaster.IpNo, ipDetail.Item);
                    }
                    else if (!ipMaster.IsReceiveExceed && Math.Abs(ipDetail.ReceivedQty + ipDetail.ReceiveQtyInput) > Math.Abs(ipDetail.Qty))
                    {
                        //送货单的收货数 + 本次收货数大于发货数
                        throw new BusinessException(Resources.ORD.IpMaster.Errors_ReceiveQtyExcceedOrderQty, ipMaster.IpNo, ipDetail.Item);
                    }
                }
                #endregion

                #region 计划协议送货单即使设置asn多次收货，但对于每一行还是只能收一次货，否则安吉进行收货冲销会有问题
                //2013-09-11 条码收货 可以每一行还多次收货
                //if (orderType == CodeMaster.OrderType.ScheduleLine)
                //{
                //    if (ipDetail.ReceivedQty > 0)
                //        throw new BusinessException("送货单{0}中计划协议号{1}行号{2}已经收过货物，不能再次收货", ipDetail.IpNo, ipDetail.ExternalOrderNo, ipDetail.ExternalSequence);
                //}
                #endregion

                #region 发货明细是否已经关闭
                if (ipDetail.IsClose)
                {
                    throw new BusinessException("送货单{0}零件{1}已经关闭，不能进行收货。", ipMaster.IpNo, ipDetail.Item);
                }
                #endregion

                //可能发货时计划协议反写未成功但仍可以发货，收货时计划协议反写成功
                //但asn上没有更新到计划协议号和行号，且结算方式可能会错
                if (orderType == CodeMaster.OrderType.Procurement)
                {
                    if (string.IsNullOrWhiteSpace(ipDetail.ExternalOrderNo)
                    || string.IsNullOrWhiteSpace(ipDetail.ExternalSequence))
                    {
                        throw new BusinessException("采购送货单{0}物料号{1}的计划协议号或PO号不能为空。", ipDetail.IpNo, ipDetail.Item);
                    }
                }

                #region 采购收货是否有价格单判断
                //if (orderHead.Type == BusinessConstants.CODE_MASTER_ORDER_TYPE_VALUE_PROCUREMENT
                //    && !bool.Parse(entityPreference.Value))
                //{
                //    if (orderDetail.UnitPrice == Decimal.Zero)
                //    {
                //        //重新查找一次价格
                //        PriceListDetail priceListDetail = priceListDetailMgrE.GetLastestPriceListDetail(
                //            orderDetail.DefaultPriceList,
                //            orderDetail.Item,
                //            orderHead.StartTime,
                //            orderHead.Currency,
                //            orderDetail.Uom);

                //        if (priceListDetail != null)
                //        {
                //            orderDetail.UnitPrice = priceListDetail.UnitPrice;
                //            orderDetail.IsProvisionalEstimate = priceListDetail.IsProvisionalEstimate;
                //            orderDetail.IsIncludeTax = priceListDetail.IsIncludeTax;
                //            orderDetail.TaxCode = priceListDetail.TaxCode;
                //        }
                //        else
                //        {
                //            throw new BusinessErrorException("Order.Error.NoPriceListReceipt", orderDetail.Item.Code);
                //        }
                //    }
                //}
                #endregion
            }
            #endregion

            #region KIT单收货判断
            //if (ipMaster.Type == CodeMaster.IpType.KIT)
            //{
            //    string anjiRegion = systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.WMSAnjiRegion);
            //    if (ipMaster.PartyFrom != anjiRegion)
            //    {
            //        CheckKitIpDetail(ipMaster, isCheckKitTraceItem);
            //    }
            //}
            #endregion

            #region 循环更新订单明细
            if (orderType != CodeMaster.OrderType.ScheduleLine)
            {
                foreach (OrderDetail orderDetail in orderDetailList)
                {
                    #region 采购收货是否有计划协议号或PO号
                    OrderMaster orderMaster = orderMasterList.Where(om => om.OrderNo == orderDetail.OrderNo).Single();
                    if (orderMaster.Type == com.Sconit.CodeMaster.OrderType.Procurement)
                    {
                        if (string.IsNullOrWhiteSpace(orderDetail.ExternalOrderNo)
                            || string.IsNullOrWhiteSpace(orderDetail.ExternalSequence))
                        {
                            throw new BusinessException("供应商直供要货单{0}物料号{1}的计划协议号或PO号不能为空。", orderDetail.OrderNo, orderDetail.Item);
                        }
                    }
                    #endregion

                    IList<IpDetail> targetIpDetailList = (from det in nonZeroIpDetailList
                                                          where det.OrderDetailId == orderDetail.Id
                                                          select det).ToList();

                    //更新订单的收货数
                    orderDetail.ReceivedQty += targetIpDetailList.Sum(det => det.ReceiveQtyInput);
                    genericMgr.Update(orderDetail);
                }
            }
            else
            {
                //foreach (IpDetail ipDetail in nonZeroIpDetailList)
                //{
                //    decimal remainReceiveQty = ipDetail.ReceiveQtyInput;

                //    IList<OrderDetail> scheduleOrderDetailList = this.genericMgr.FindEntityWithNativeSql<OrderDetail>("select * from ORD_OrderDet_8 where ExtNo = ? and ExtSeq = ? and ScheduleType = ? and ShipQty > RecQty order by EndDate",
                //                                new object[] { ipDetail.ExternalOrderNo, ipDetail.ExternalSequence, CodeMaster.ScheduleType.Firm });

                //    if (scheduleOrderDetailList != null && scheduleOrderDetailList.Count > 0)
                //    {
                //        foreach (OrderDetail scheduleOrderDetail in scheduleOrderDetailList)
                //        {

                //            if (remainReceiveQty > (scheduleOrderDetail.ShippedQty - scheduleOrderDetail.ReceivedQty))
                //            {
                //                remainReceiveQty -= (scheduleOrderDetail.ShippedQty - scheduleOrderDetail.ReceivedQty);
                //                scheduleOrderDetail.ReceivedQty = scheduleOrderDetail.ShippedQty;
                //            }
                //            else
                //            {
                //                scheduleOrderDetail.ReceivedQty += remainReceiveQty;
                //                remainReceiveQty = 0;
                //                break;
                //            }

                //            this.genericMgr.Update(scheduleOrderDetail);
                //        }
                //    }

                //    if (remainReceiveQty > 0)
                //    {
                //        throw new BusinessException(Resources.ORD.IpMaster.Errors_ReceiveQtyExcceedOrderQty, ipMaster.IpNo, ipDetail.Item);
                //    }
                //}
            }
            #endregion

            #region 循环IpDetail，处理超收差异
            IList<IpDetail> gapIpDetailList = new List<IpDetail>();
            foreach (IpDetail targetIpDetail in nonZeroIpDetailList)
            {
                #region 收货输入和送货单库存明细匹配
                IList<IpLocationDetail> targetIpLocationDetailList = (from ipLocDet in ipLocationDetailList
                                                                      where ipLocDet.IpDetailId == targetIpDetail.Id
                                                                      select ipLocDet).OrderByDescending(d => d.IsConsignment).ToList();  //排序为了先匹配寄售的

                bool isContainHu = targetIpLocationDetailList.Where(ipLocDet => !string.IsNullOrWhiteSpace(ipLocDet.HuId)).Count() > 0;

                if (ipMaster.IsReceiveScanHu)
                {
                    #region 收货扫描条码
                    if (isContainHu)
                    {
                        #region 条码匹配条码

                        #region 匹配到的条码
                        IList<IpLocationDetail> matchedIpLocationDetailList = new List<IpLocationDetail>();
                        foreach (IpDetailInput ipDetailInput in targetIpDetail.IpDetailInputs)
                        {
                            IpLocationDetail matchedIpLocationDetail = targetIpLocationDetailList.Where(locDet => locDet.HuId == ipDetailInput.HuId).SingleOrDefault();

                            if (matchedIpLocationDetail != null)
                            {
                                ipDetailInput.AddReceivedIpLocationDetail(matchedIpLocationDetail);
                                matchedIpLocationDetailList.Add(matchedIpLocationDetail);

                                #region 更新库存状态
                                matchedIpLocationDetail.ReceivedQty = matchedIpLocationDetail.Qty;
                                matchedIpLocationDetail.IsClose = true;

                                genericMgr.Update(matchedIpLocationDetail);
                                #endregion
                            }
                        }
                        #endregion

                        #region 未匹配到的条码，记录差异
                        var matchedHuIdList = matchedIpLocationDetailList.Select(locDet => locDet.HuId);
                        var gapIpDetailInputList = targetIpDetail.IpDetailInputs.Where(input => !matchedHuIdList.Contains(input.HuId));

                        #region 记录差异
                        if (gapIpDetailInputList != null && gapIpDetailInputList.Count() > 0)
                        {
                            IpDetail gapIpDetail = Mapper.Map<IpDetail, IpDetail>(targetIpDetail);
                            gapIpDetail.Type = com.Sconit.CodeMaster.IpDetailType.Gap;
                            gapIpDetail.GapReceiptNo = string.Empty;                            //todo 记录产生差异的收货单号
                            //gapIpDetail.Qty = gapIpDetailInputList.Sum(gap => -gap.ReceiveQty / targetIpDetail.UnitQty); //多收的条码，数量为负，转为订单单位
                            gapIpDetail.Qty = gapIpDetailInputList.Sum(gap => -gap.ReceiveQty); //多收的条码，数量为负
                            gapIpDetail.ReceivedQty = 0;
                            gapIpDetail.IsClose = false;
                            gapIpDetail.GapIpDetailId = targetIpDetail.Id;

                            gapIpDetail.IpLocationDetails = (from gap in gapIpDetailInputList
                                                             select new IpLocationDetail
                                                             {
                                                                 IpNo = ipMaster.IpNo,
                                                                 OrderType = targetIpDetail.OrderType,
                                                                 OrderDetailId = targetIpDetail.OrderDetailId,
                                                                 Item = targetIpDetail.Item,
                                                                 //HuId = gap.HuId,      //多收的条码按数量记录差异
                                                                 //LotNo = gap.LotNo,
                                                                 IsCreatePlanBill = false,
                                                                 PlanBill = null,
                                                                 ActingBill = null,
                                                                 IsFreeze = false,
                                                                 IsATP = false,          //差异不能进行MRP运算
                                                                 QualityType = targetIpDetail.QualityType,
                                                                 OccupyType = com.Sconit.CodeMaster.OccupyType.None,
                                                                 OccupyReferenceNo = null,
                                                                 Qty = -gap.ReceiveQty * gapIpDetail.UnitQty,    //多收，产生负数的差异
                                                                 ReceivedQty = 0,
                                                                 IsClose = false
                                                             }).ToList();

                            gapIpDetailList.Add(gapIpDetail);
                        }
                        #endregion
                        #endregion
                        #endregion
                    }
                    else
                    {
                        #region 数量匹配条码
                        IpDetail gapIpDetail = CycleMatchIpDetailInput(targetIpDetail, targetIpDetail.IpDetailInputs, targetIpLocationDetailList);
                        if (gapIpDetail != null)
                        {
                            gapIpDetailList.Add(gapIpDetail);
                        }
                        #endregion
                    }
                    #endregion
                }
                else
                {
                    #region 收货不扫描条码
                    if (isContainHu)
                    {
                        #region 条码匹配数量
                        IpDetail gapIpDetail = CycleMatchIpDetailInput(targetIpDetail, targetIpDetail.IpDetailInputs, targetIpLocationDetailList);
                        if (gapIpDetail != null)
                        {
                            gapIpDetailList.Add(gapIpDetail);
                        }
                        #endregion
                    }
                    else
                    {
                        #region 数量匹配数量
                        IpDetail gapIpDetail = CycleMatchIpDetailInput(targetIpDetail, targetIpDetail.IpDetailInputs, targetIpLocationDetailList);
                        if (gapIpDetail != null)
                        {
                            gapIpDetailList.Add(gapIpDetail);
                        }
                        #endregion
                    }
                    #endregion
                }
                #endregion

                #region 更新IpDetail上的收货数
                targetIpDetail.ReceivedQty += targetIpDetail.ReceiveQtyInput;
                if (targetIpLocationDetailList.Where(i => !i.IsClose).Count() == 0)
                {
                    //只有所有的IpLocationDetail关闭才能关闭
                    targetIpDetail.IsClose = true;
                }
                genericMgr.Update(targetIpDetail);
                #endregion
            }
            #endregion

            #region 送货单一次性收货，未收货差异
            if (ipMaster.IsAsnUniqueReceive)
            {
                #region 查找未关闭送货单明细
                List<IpDetail> openIpDetailList = (from det in nonZeroIpDetailList where !det.IsClose select det).ToList();

                #region 查询剩余的未关闭送货单明细
                IList<IpDetail> exceptIpDetailList = LoadExceptIpDetails(ipMaster.IpNo, (nonZeroIpDetailList.Select(det => det.Id)).ToArray());
                #endregion

                #region 合并未关闭送货单明细
                if (exceptIpDetailList != null && exceptIpDetailList.Count > 0)
                {
                    openIpDetailList.AddRange(exceptIpDetailList);
                }
                #endregion
                #endregion

                #region 查找未关闭送货单库存对象
                List<IpLocationDetail> openIpLocationDetailList = (from det in ipLocationDetailList where !det.IsClose select det).ToList();

                #region 查询剩余未关闭送货单库存对象
                IList<IpLocationDetail> expectIpLocationDetailList = LoadExceptIpLocationDetails(ipMaster.IpNo, (nonZeroIpDetailList.Select(det => det.Id)).ToArray());
                #endregion

                #region 合并未关闭送货单库存对象
                if (expectIpLocationDetailList != null && expectIpLocationDetailList.Count > 0)
                {
                    openIpLocationDetailList.AddRange(expectIpLocationDetailList);
                }
                #endregion
                #endregion

                #region 生成未收货差异
                if (openIpDetailList != null && openIpDetailList.Count > 0)
                {
                    foreach (IpDetail openIpDetail in openIpDetailList)
                    {
                        var targetOpenIpLocationDetailList = openIpLocationDetailList.Where(o => o.IpDetailId == openIpDetail.Id);

                        IpDetail gapIpDetail = Mapper.Map<IpDetail, IpDetail>(openIpDetail);
                        gapIpDetail.Type = com.Sconit.CodeMaster.IpDetailType.Gap;
                        gapIpDetail.GapReceiptNo = string.Empty;                            //todo 记录产生差异的收货单号
                        gapIpDetail.Qty = targetOpenIpLocationDetailList.Sum(o => o.RemainReceiveQty / openIpDetail.UnitQty);
                        gapIpDetail.ReceivedQty = 0;
                        gapIpDetail.IsClose = false;
                        gapIpDetail.GapIpDetailId = openIpDetail.Id;

                        gapIpDetail.IpLocationDetails = (from locDet in targetOpenIpLocationDetailList
                                                         select new IpLocationDetail
                                                         {
                                                             IpNo = locDet.IpNo,
                                                             OrderType = locDet.OrderType,
                                                             OrderDetailId = locDet.OrderDetailId,
                                                             Item = locDet.Item,
                                                             HuId = locDet.HuId,
                                                             LotNo = locDet.LotNo,
                                                             IsCreatePlanBill = locDet.IsCreatePlanBill,
                                                             IsConsignment = locDet.IsConsignment,
                                                             PlanBill = locDet.PlanBill,
                                                             ActingBill = locDet.ActingBill,
                                                             IsFreeze = locDet.IsFreeze,
                                                             //IsATP = locDet.IsATP,
                                                             IsATP = false,
                                                             QualityType = locDet.QualityType,
                                                             OccupyType = locDet.OccupyType,
                                                             OccupyReferenceNo = locDet.OccupyReferenceNo,
                                                             Qty = locDet.RemainReceiveQty,
                                                             ReceivedQty = 0,
                                                             IsClose = false
                                                         }).ToList();

                        gapIpDetailList.Add(gapIpDetail);
                    }
                }
                #endregion

                #region 关闭未收货送货单明细和库存明细
                if (openIpDetailList != null && openIpDetailList.Count > 0)
                {
                    foreach (IpDetail openIpDetail in openIpDetailList)
                    {
                        openIpDetail.IsClose = true;
                        this.genericMgr.Update(openIpDetail);
                    }
                }

                if (openIpLocationDetailList != null && openIpLocationDetailList.Count > 0)
                {
                    foreach (IpLocationDetail openIpLocationDetail in openIpLocationDetailList)
                    {
                        openIpLocationDetail.IsClose = true;
                        this.genericMgr.Update(openIpLocationDetail);
                    }
                }
                //string batchupdateipdetailstatement = "update from ipdetail set isclose = true where ipno = ? and isclose = false";
                //genericmgr.update(batchupdateipdetailstatement, ipmaster.ipno);

                //string batchUpdateIpLocationDetailStatement = "update from IpLocationDetail set IsClose = True where IpNo = ? and IsClose = False";
                //genericMgr.Update(batchUpdateIpLocationDetailStatement, ipMaster.IpNo);                
                #endregion
            }
            #endregion

            #region 收货
            ReceiptMaster receiptMaster = this.receiptMgr.TransferIp2Receipt(ipMaster);
            this.receiptMgr.CreateReceipt(receiptMaster, effectiveDate);
            #endregion

            #region 记录收货差异
            if (gapIpDetailList != null && gapIpDetailList.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(ipMaster.SequenceNo))
                {
                    throw new BusinessException("排序单装箱单{0}的收货数和发货数不一致。", ipMaster.SequenceNo);
                }

                foreach (IpDetail gapIpDetail in gapIpDetailList)
                {
                    gapIpDetail.GapReceiptNo = receiptMaster.ReceiptNo;
                    this.genericMgr.Create(gapIpDetail);

                    foreach (IpLocationDetail gapIpLocationDetail in gapIpDetail.IpLocationDetails)
                    {
                        gapIpLocationDetail.IpDetailId = gapIpDetail.Id;
                        this.genericMgr.Create(gapIpLocationDetail);
                    }
                }

                #region 调整发货方库存
                if (ipMaster.ReceiveGapTo == CodeMaster.ReceiveGapTo.AdjectLocFromInv)
                {
                    foreach (IpDetail gapIpDetail in gapIpDetailList)
                    {
                        gapIpDetail.IpDetailInputs = null;

                        foreach (IpLocationDetail gapIpLocationDetail in gapIpDetail.IpLocationDetails)
                        {
                            IpDetailInput input = new IpDetailInput();
                            input.ReceiveQty = gapIpLocationDetail.Qty / gapIpDetail.UnitQty; //转为订单单位
                            input.HuId = gapIpLocationDetail.HuId;
                            input.LotNo = gapIpLocationDetail.LotNo;
                            gapIpDetail.AddIpDetailInput(input);
                        }
                    }

                    this.AdjustIpGap(gapIpDetailList, CodeMaster.IpGapAdjustOption.GI);
                }
                #endregion
            }
            #endregion

            #region 更新送货单状态
            ipMaster.Status = CodeMaster.IpStatus.InProcess;
            this.genericMgr.Update(ipMaster);
            #endregion

            #region 尝试关闭送货单
            this.ipMgr.TryCloseIp(ipMaster);
            #endregion

            #region 尝试关闭订单
            foreach (OrderMaster orderMaster in orderMasterList)
            {
                TryCloseOrder(orderMaster);
            }
            #endregion

            return receiptMaster;
        }

        private IpDetail CycleMatchIpDetailInput(IpDetail ipDetail, IList<IpDetailInput> ipDetailInputList, IList<IpLocationDetail> ipLocationDetailList)
        {
            IpDetail gapIpDetail = null;
            #region 循环匹配收货记录和送货单库存明细
            foreach (IpDetailInput ipDetailInput in ipDetailInputList)
            {
                MatchIpDetailInput(ipDetail, ipDetailInput, ipLocationDetailList, gapIpDetail);
            }
            #endregion

            return gapIpDetail;
        }

        private void MatchIpDetailInput(IpDetail ipDetail, IpDetailInput ipDetailInput, IList<IpLocationDetail> ipLocationDetailList, IpDetail gapIpDetail)
        {
            decimal remainQty = ipDetailInput.ReceiveQty * ipDetail.UnitQty;  //转为库存单位
            foreach (IpLocationDetail ipLocationDetail in ipLocationDetailList)
            {
                if (ipLocationDetail.IsClose)
                {
                    continue;
                }

                //iplocationdet只可能为正数
                if (ipLocationDetail.RemainReceiveQty >= remainQty)
                {
                    //收货明细匹配完
                    #region 添加收货记录和IpLocationDetail的映射关系
                    IpLocationDetail receivedIpLocationDetail = Mapper.Map<IpLocationDetail, IpLocationDetail>(ipLocationDetail);
                    receivedIpLocationDetail.ReceivedQty = remainQty;
                    ipDetailInput.AddReceivedIpLocationDetail(receivedIpLocationDetail);
                    #endregion

                    ipLocationDetail.ReceivedQty += remainQty;
                    remainQty = 0;
                    if (ipLocationDetail.Qty == ipLocationDetail.ReceivedQty)
                    {
                        ipLocationDetail.IsClose = true;
                    }
                }
                else
                {
                    //收货明细未匹配完
                    #region 添加收货记录和IpLocationDetail的映射关系
                    IpLocationDetail receivedIpLocationDetail = Mapper.Map<IpLocationDetail, IpLocationDetail>(ipLocationDetail);
                    receivedIpLocationDetail.ReceivedQty = ipLocationDetail.RemainReceiveQty;
                    ipDetailInput.AddReceivedIpLocationDetail(receivedIpLocationDetail);
                    #endregion

                    remainQty -= ipLocationDetail.RemainReceiveQty;
                    ipLocationDetail.ReceivedQty = ipLocationDetail.Qty;
                    ipLocationDetail.IsClose = true;
                }

                //更新
                if (ipLocationDetail.Id > 0)
                {
                    genericMgr.Update(ipLocationDetail);
                }

                if (remainQty == 0)
                {
                    //匹配完，跳出循环
                    break;
                }
            }

            //超收，还有未匹配完的数量
            if (remainQty > 0)
            {
                #region 记录差异
                if (gapIpDetail == null)
                {
                    gapIpDetail = Mapper.Map<IpDetail, IpDetail>(ipDetail);
                    gapIpDetail.Type = com.Sconit.CodeMaster.IpDetailType.Gap;
                    gapIpDetail.GapReceiptNo = string.Empty;
                    gapIpDetail.ReceivedQty = 0;
                    gapIpDetail.IsClose = false;
                    gapIpDetail.GapIpDetailId = ipDetail.Id;
                }

                gapIpDetail.Qty += -(remainQty / ipDetail.UnitQty);          //多收，数量为负，转为订单单位

                IpLocationDetail gapIpLocationDetail = new IpLocationDetail();
                gapIpLocationDetail.IpNo = ipDetail.IpNo;
                gapIpLocationDetail.OrderType = ipDetail.OrderType;
                gapIpLocationDetail.OrderDetailId = ipDetail.OrderDetailId;
                gapIpLocationDetail.Item = ipDetail.Item;
                gapIpLocationDetail.HuId = null;  //收货未匹配产生的差异全部为数量（除了条码匹配条码）
                gapIpLocationDetail.LotNo = null;
                gapIpLocationDetail.IsCreatePlanBill = false;
                gapIpLocationDetail.PlanBill = null;
                gapIpLocationDetail.ActingBill = null;
                gapIpLocationDetail.IsFreeze = false;
                gapIpLocationDetail.IsATP = false;          //差异不能进行MRP运算
                gapIpLocationDetail.QualityType = ipDetail.QualityType;
                gapIpLocationDetail.OccupyType = com.Sconit.CodeMaster.OccupyType.None;
                gapIpLocationDetail.OccupyReferenceNo = null;
                gapIpLocationDetail.Qty = -remainQty;   //多收，产生负数的差异
                gapIpLocationDetail.ReceivedQty = 0;
                gapIpLocationDetail.IsClose = false;

                gapIpDetail.AddIpLocationDetail(gapIpLocationDetail);
                #endregion
            }
        }
        #endregion

        #region 送货单差异调整
        [Transaction(TransactionMode.Requires)]
        public ReceiptMaster AdjustIpGap(IList<IpDetail> ipDetailList, CodeMaster.IpGapAdjustOption ipGapAdjustOption)
        {
            return AdjustIpGap(ipDetailList, ipGapAdjustOption, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public ReceiptMaster AdjustIpGap(IList<IpDetail> ipDetailList, CodeMaster.IpGapAdjustOption ipGapAdjustOption, DateTime effectiveDate)
        {
            #region 判断送货单明细是否差异类型是否合并收货
            if (ipDetailList.Where(det => det.Type == CodeMaster.IpDetailType.Normal).Count() > 0)
            {
                throw new TechnicalException("送货单差异明细类型不正确。");
            }
            #endregion

            #region 判断送货单是否合并调整
            if ((from det in ipDetailList select det.IpNo).Distinct().Count() > 1)
            {
                throw new TechnicalException("送货单不能合并调整差异。");
            }
            #endregion

            #region 判断是否全0发货
            if (ipDetailList == null || ipDetailList.Count == 0)
            {
                throw new BusinessException("收货差异调整明细不能为空。");
            }

            IList<IpDetail> nonZeroIpDetailList = ipDetailList.Where(o => o.ReceiveQtyInput != 0).ToList();

            if (nonZeroIpDetailList.Count == 0)
            {
                throw new BusinessException("收货差异调整明细不能为空。");
            }
            #endregion

            #region 查询送货单头对象
            string ipNo = (from det in ipDetailList select det.IpNo).Distinct().Single();
            IpMaster ipMaster = this.genericMgr.FindById<IpMaster>(ipNo);
            ipMaster.IpDetails = nonZeroIpDetailList;
            #endregion

            #region 查询订单头对象
            IList<OrderMaster> orderMasterList = LoadOrderMasters((from det in nonZeroIpDetailList
                                                                   select det.OrderNo).Distinct().ToArray());
            #endregion

            #region 获取收货订单类型
            IList<com.Sconit.CodeMaster.OrderType> orderTypeList = (from orderMaster in orderMasterList
                                                                    group orderMaster by orderMaster.Type into result
                                                                    select result.Key).ToList();

            if (orderTypeList.Count > 1)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_CannotMixOrderTypeReceive);
            }

            com.Sconit.CodeMaster.OrderType orderType = orderTypeList.First();
            #endregion

            #region 查询送货单库存对象
            IList<IpLocationDetail> ipLocationDetailList = LoadIpLocationDetails((from det in nonZeroIpDetailList
                                                                                  select det.Id).ToArray());
            #endregion

            #region 循环检查发货明细
            foreach (IpDetail ipDetail in nonZeroIpDetailList)
            {
                #region 是否过量收货判断
                if (Math.Abs(ipDetail.ReceivedQty) >= Math.Abs(ipDetail.Qty))
                {
                    //送货单的收货数已经大于等于发货数
                    throw new BusinessException("送货单{0}行号{1}零件{2}的收货数量超出了差异数量。", ipMaster.IpNo, ipDetail.Sequence.ToString(), ipDetail.Item);
                }
                else if (!ipMaster.IsReceiveExceed && Math.Abs(ipDetail.ReceivedQty + ipDetail.ReceiveQtyInput) > Math.Abs(ipDetail.Qty))
                {
                    //送货单的收货数 + 本次收货数大于发货数
                    throw new BusinessException("送货单{0}行号{1}零件{2}的收货数量超出了差异数量。", ipMaster.IpNo, ipDetail.Sequence.ToString(), ipDetail.Item);
                }
                #endregion

                #region 差异明细是否已经关闭
                if (ipDetail.IsClose)
                {
                    throw new BusinessException("送货单{0}行号{1}零件{2}已经关闭，不能进行差异调整。", ipMaster.IpNo, ipDetail.Sequence.ToString(), ipDetail.Item);
                }
                #endregion
            }
            #endregion

            #region 循环更新订单明细
            BusinessException businessException = new BusinessException();
            if (orderType != CodeMaster.OrderType.ScheduleLine)
            {
                #region 非计划协议
                #region 查询订单明细对象
                IList<OrderDetail> orderDetailList = LoadOrderDetails((from det in nonZeroIpDetailList
                                                                       where det.OrderDetailId.HasValue
                                                                       select det.OrderDetailId.Value).Distinct().ToArray());
                #endregion

                foreach (OrderDetail orderDetail in orderDetailList)
                {
                    IList<IpDetail> targetIpDetailList = (from det in nonZeroIpDetailList
                                                          where det.OrderDetailId == orderDetail.Id
                                                          select det).ToList();

                    #region 调整发货方库存
                    if (ipGapAdjustOption == CodeMaster.IpGapAdjustOption.GI)
                    {
                        //更新订单的发货数
                        orderDetail.ShippedQty -= targetIpDetailList.Sum(det => det.ReceiveQtyInput);
                        genericMgr.Update(orderDetail);
                    }
                    #endregion

                    #region 调整收货方库存
                    else
                    {
                        //更新订单的收货数
                        orderDetail.ReceivedQty += targetIpDetailList.Sum(det => det.ReceiveQtyInput);
                        genericMgr.Update(orderDetail);
                    }
                    #endregion
                }
                #endregion
            }
            else
            {
                #region 计划协议
                foreach (IpDetail ipDetail in nonZeroIpDetailList)
                {
                    decimal remainReceiveQty = ipDetail.ReceiveQtyInput;

                    if (ipGapAdjustOption == CodeMaster.IpGapAdjustOption.GI)
                    {
                        #region 调整发货方库存
                        //更新订单的发货数
                        IList<OrderDetail> scheduleOrderDetailList = this.genericMgr.FindEntityWithNativeSql<OrderDetail>("select * from ORD_OrderDet_8 where ExtNo = ? and ExtSeq = ? and ScheduleType = ? and ShipQty > RecQty order by EndDate desc",
                                                new object[] { ipDetail.ExternalOrderNo, ipDetail.ExternalSequence, CodeMaster.ScheduleType.Firm });

                        if (scheduleOrderDetailList != null && scheduleOrderDetailList.Count > 0)
                        {
                            foreach (OrderDetail scheduleOrderDetail in scheduleOrderDetailList)
                            {
                                if (remainReceiveQty > (scheduleOrderDetail.ShippedQty - scheduleOrderDetail.ReceivedQty))
                                {
                                    remainReceiveQty -= (scheduleOrderDetail.ShippedQty - scheduleOrderDetail.ReceivedQty);
                                    scheduleOrderDetail.ShippedQty = scheduleOrderDetail.ReceivedQty;
                                }
                                else
                                {
                                    scheduleOrderDetail.ShippedQty -= remainReceiveQty;
                                    remainReceiveQty = 0;
                                    break;
                                }

                                this.genericMgr.Update(scheduleOrderDetail);
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region 调整收货方库存
                        //更新订单的收货数
                        IList<OrderDetail> scheduleOrderDetailList = this.genericMgr.FindEntityWithNativeSql<OrderDetail>("select * from ORD_OrderDet_8 where ExtNo = ? and ExtSeq = ? and ScheduleType = ? and ShipQty > RecQty order by EndDate",
                                                new object[] { ipDetail.ExternalOrderNo, ipDetail.ExternalSequence, CodeMaster.ScheduleType.Firm });

                        if (scheduleOrderDetailList != null && scheduleOrderDetailList.Count > 0)
                        {
                            foreach (OrderDetail scheduleOrderDetail in scheduleOrderDetailList)
                            {
                                if (remainReceiveQty > (scheduleOrderDetail.ShippedQty - scheduleOrderDetail.ReceivedQty))
                                {
                                    remainReceiveQty -= (scheduleOrderDetail.ShippedQty - scheduleOrderDetail.ReceivedQty);
                                    scheduleOrderDetail.ReceivedQty = scheduleOrderDetail.ShippedQty;
                                }
                                else
                                {
                                    scheduleOrderDetail.ReceivedQty += remainReceiveQty;
                                    remainReceiveQty = 0;
                                    break;
                                }

                                this.genericMgr.Update(scheduleOrderDetail);
                            }
                        }
                        #endregion
                    }

                    if (remainReceiveQty > 0)
                    {
                        businessException.AddMessage(Resources.ORD.IpMaster.Errors_ReceiveQtyExcceedOrderQty, ipMaster.IpNo, ipDetail.Item);
                    }
                }
                #endregion
            }

            #region 循环IpDetail，更新收货数
            foreach (IpDetail targetIpDetail in nonZeroIpDetailList)
            {
                #region 收货输入和送货单库存明细匹配
                IList<IpLocationDetail> targetIpLocationDetailList = (from ipLocDet in ipLocationDetailList
                                                                      where ipLocDet.IpDetailId == targetIpDetail.Id
                                                                      select ipLocDet).OrderByDescending(d => d.IsConsignment).ToList();  //排序为了先匹配寄售的

                bool isContainHu = targetIpLocationDetailList.Where(ipLocDet => !string.IsNullOrWhiteSpace(ipLocDet.HuId)).Count() > 0;

                if (ipMaster.IsReceiveScanHu)
                {
                    #region 收货扫描条码
                    if (isContainHu)
                    {
                        #region 条码匹配条码
                        foreach (IpDetailInput targetIpDetailInput in targetIpDetail.IpDetailInputs)
                        {
                            IpLocationDetail matchedIpLocationDetail = targetIpLocationDetailList.Where(ipLocDet => ipLocDet.HuId == targetIpDetailInput.HuId).SingleOrDefault();
                            if (matchedIpLocationDetail != null)
                            {
                                #region 更新库存状态
                                matchedIpLocationDetail.ReceivedQty = matchedIpLocationDetail.Qty;
                                matchedIpLocationDetail.IsClose = true;
                                targetIpDetailInput.AddReceivedIpLocationDetail(matchedIpLocationDetail);

                                genericMgr.Update(matchedIpLocationDetail);
                                #endregion
                            }
                            else
                            {
                                #region 未匹配到的条码，报错
                                businessException.AddMessage("条码{0}不在送货单差异调整明细中。", targetIpDetailInput.HuId);
                                #endregion
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region 数量匹配条码
                        IpDetail gapIpDetail = CycleMatchIpDetailInput(targetIpDetail, targetIpDetail.IpDetailInputs, targetIpLocationDetailList);
                        if (gapIpDetail != null)
                        {
                            businessException.AddMessage("送货单{0}行号{1}零件{2}差异调整不正确。", ipMaster.IpNo, targetIpDetail.Sequence.ToString(), targetIpDetail.Item);
                        }
                        #endregion
                    }
                    #endregion
                }
                else
                {
                    #region 收货不扫描条码
                    if (isContainHu)
                    {
                        #region 条码匹配数量
                        IpDetail gapIpDetail = CycleMatchIpDetailInput(targetIpDetail, targetIpDetail.IpDetailInputs, targetIpLocationDetailList);
                        if (gapIpDetail != null)
                        {
                            businessException.AddMessage("送货单{0}行号{1}零件{2}差异调整不正确。", ipMaster.IpNo, targetIpDetail.Sequence.ToString(), targetIpDetail.Item);
                        }
                        #endregion
                    }
                    else
                    {
                        #region 数量匹配数量
                        IpDetail gapIpDetail = CycleMatchIpDetailInput(targetIpDetail, targetIpDetail.IpDetailInputs, targetIpLocationDetailList);
                        if (gapIpDetail != null)
                        {
                            businessException.AddMessage("送货单{0}行号{1}零件{2}差异调整不正确。", ipMaster.IpNo, targetIpDetail.Sequence.ToString(), targetIpDetail.Item);
                        }
                        #endregion
                    }
                    #endregion
                }
                #endregion

                #region 更新IpDetail上的收货数
                targetIpDetail.ReceivedQty += targetIpDetail.ReceiveQtyInput;
                if (targetIpLocationDetailList.Where(i => !i.IsClose).Count() == 0)
                {
                    //只有所有的IpLocationDetail关闭才能关闭
                    targetIpDetail.IsClose = true;
                }
                genericMgr.Update(targetIpDetail);
                #endregion
            }
            #endregion

            if (businessException.HasMessage)
            {
                throw businessException;
            }
            #endregion

            #region 差异调整
            ReceiptMaster receiptMaster = null;
            receiptMaster = this.receiptMgr.TransferIpGap2Receipt(ipMaster, ipGapAdjustOption);
            this.receiptMgr.CreateReceipt(receiptMaster, effectiveDate);
            #endregion

            #region 尝试关闭送货单
            this.ipMgr.TryCloseIp(ipMaster);
            #endregion

            #region 尝试关闭订单
            foreach (OrderMaster orderMaster in orderMasterList)
            {
                TryCloseOrder(orderMaster);
            }
            #endregion

            return receiptMaster;
        }

        #endregion

        #region 取消订单
        [Transaction(TransactionMode.Requires)]
        public void CancelOrder(string orderNo)
        {
            CancelOrder(this.genericMgr.FindById<OrderMaster>(orderNo));
        }

        [Transaction(TransactionMode.Requires)]
        public void CancelOrder(OrderMaster orderMaster)
        {
            if (orderMaster.Status != CodeMaster.OrderStatus.Submit && orderMaster.Status != CodeMaster.OrderStatus.Create)
            {
                throw new BusinessException("不能取消状态为{1}的订单{0}。", orderMaster.OrderNo,
                       systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
            }
            else
            {
                if (pickTaskMgr.IsOrderPicked(orderMaster.OrderNo))
                {
                    throw new BusinessException("订单{0}已经拣货，请先取消拣货。", orderMaster.OrderNo);
                }
                else
                {
                    //将已有的拣货任务冻结
                    pickTaskMgr.CancelAllPickTask(orderMaster.OrderNo);

                    orderMaster.Status = CodeMaster.OrderStatus.Cancel;
                    orderMaster.CancelDate = DateTime.Now;
                    User user = SecurityContextHolder.Get();
                    orderMaster.CancelUserId = user.Id;
                    orderMaster.CancelUserName = user.FullName;
                    this.genericMgr.Update(orderMaster);
                }
            }
        }
        #endregion

        #region 关闭订单
        public void ManualCloseOrder(string orderNo)
        {
            this.ManualCloseOrder(this.genericMgr.FindById<OrderMaster>(orderNo));
        }

        public void ManualCloseOrder(OrderMaster orderMaster)
        {
            CloseOrder(orderMaster, true);
        }

        public void AutoCloseOrder()
        {
            IList<OrderMaster> orderMasterList = this.genericMgr.FindAll<OrderMaster>("from OrderMaster where Status = ?", CodeMaster.OrderStatus.Complete);

            if (orderMasterList != null && orderMasterList.Count > 0)
            {
                foreach (OrderMaster orderMaster in orderMasterList)
                {
                    try
                    {
                        CloseOrder(orderMaster, false);
                    }
                    catch (BusinessException ex)
                    {
                        log.Debug(string.Format("Close Order:{0} fail, Message {2}", orderMaster.OrderNo, ex.Message));
                    }
                    catch (Exception ex)
                    {
                        this.genericMgr.CleanSession();
                        log.Error(string.Format("Close Order:{0} error, Message {2}", orderMaster.OrderNo, ex.Message));
                    }
                }
            }
        }

        public void AutoCloseASN(DateTime dateTime)
        {
            this.ipMgr.TryCloseExpiredScheduleLineIp(dateTime);
        }

        private void TryCloseOrder(string orderNo)
        {
            this.TryCloseOrder(this.genericMgr.FindById<OrderMaster>(orderNo));
        }

        private void TryCloseOrder(OrderMaster orderMaster)
        {
            if (!orderMaster.IsOpenOrder)
            {
                CloseOrder(orderMaster, false);
            }
        }

        private void CloseOrder(OrderMaster orderMaster, bool isForce)
        {
            //对于计划协议类型的订单将永不关闭
            if (orderMaster.Type != CodeMaster.OrderType.ScheduleLine)
            {
                if (orderMaster.Status == CodeMaster.OrderStatus.InProcess
                    || orderMaster.Status == CodeMaster.OrderStatus.Complete)
                {
                    DateTime dateTimeNow = DateTime.Now;
                    User user = SecurityContextHolder.Get();

                    this.genericMgr.FlushSession();
                    BusinessException businessException = new BusinessException();

                    #region 强制关闭生产单，先把状态改为Complete
                    if (isForce && orderMaster.Status == CodeMaster.OrderStatus.InProcess)
                    {
                        if (orderMaster.Type == CodeMaster.OrderType.Production
                            || orderMaster.Type == CodeMaster.OrderType.SubContract)
                        {
                            //生产先做订单完工
                            orderMaster.Status = CodeMaster.OrderStatus.Complete;
                            orderMaster.CompleteDate = dateTimeNow;
                            orderMaster.CompleteUserId = user.Id;
                            orderMaster.CompleteUserName = user.FullName;
                            this.genericMgr.Update(orderMaster);
                        }
                    }
                    #endregion

                    #region 条件1所有订单明细收货数大于等于订单数
                    if (!isForce || ((orderMaster.Type == CodeMaster.OrderType.Production
                                        || orderMaster.Type == CodeMaster.OrderType.SubContract)
                                      && orderMaster.Status != CodeMaster.OrderStatus.Complete))  //强制关闭不用校验这条，工单已经完工也不用校验
                    {
                        string hql = "select count(*) as counter from OrderDetail where OrderNo = ? and ReceivedQty < OrderedQty";
                        long counter = this.genericMgr.FindAll<long>(hql, new Object[] { orderMaster.OrderNo })[0];
                        if (counter > 0)
                        {
                            return;
                        }
                    }
                    #endregion

                    #region 条件2所有发货单明细全部关闭
                    if (orderMaster.Type != CodeMaster.OrderType.Production
                        && orderMaster.Type != CodeMaster.OrderType.SubContract)
                    {
                        string hql = "select count(*) as counter from IpDetail where OrderNo = ? and IsClose = ?";
                        long counter = this.genericMgr.FindAll<long>(hql, new Object[] { orderMaster.OrderNo, false })[0];
                        if (counter > 0)
                        {
                            if (!isForce)
                            {
                                //非强制关闭直接返回
                                return;
                            }
                            businessException.AddMessage("和订单相关的送货单明细没有全部关闭，不能关闭订单{0}。", orderMaster.OrderNo);
                        }
                    }
                    #endregion

                    #region 条件3所有的拣货单全部关闭
                    if (orderMaster.Type == CodeMaster.OrderType.Transfer
                       || orderMaster.Type == CodeMaster.OrderType.SubContractTransfer
                        || orderMaster.Type == CodeMaster.OrderType.Distribution)
                    {
                        string hql = "select count(*) as counter from PickListDetail where OrderNo = ? and IsClose = ?";
                        long counter = this.genericMgr.FindAll<long>(hql, new Object[] { orderMaster.OrderNo, false })[0];
                        if (counter > 0)
                        {
                            if (!isForce)
                            {
                                //非强制关闭直接返回
                                return;
                            }
                            businessException.AddMessage("和订单相关的捡货单明细没有全部关闭，不能关闭订单{0}。", orderMaster.OrderNo);
                        }
                    }
                    #endregion

                    #region 条件4所有的排序装箱单全部关闭
                    //if (orderMaster.Type == CodeMaster.OrderType.Transfer
                    //    || orderMaster.Type == CodeMaster.OrderType.SubContractTransfer
                    //    || orderMaster.Type == CodeMaster.OrderType.Procurement
                    //    || orderMaster.Type == CodeMaster.OrderType.ScheduleLine)
                    //{
                    //    string hql = "select count(*) as counter from SequenceDetail where OrderNo = ? and IsClose = ?";
                    //    long counter = this.genericMgr.FindAll<long>(hql, new Object[] { orderMaster.OrderNo, false })[0];
                    //    if (counter > 0)
                    //    {
                    //        if (!isForce)
                    //        {
                    //            //非强制关闭直接返回
                    //            return;
                    //        }
                    //        businessException.AddMessage("和订单相关的排序单明细没有全部关闭，不能关闭订单{0}。", orderMaster.OrderNo);
                    //    }
                    //}
                    #endregion

                    #region 生产单关闭校验
                    #region 条件5生产单的PlanBackflush全部关闭
                    if (orderMaster.Type == CodeMaster.OrderType.Production
                        || orderMaster.Type == CodeMaster.OrderType.SubContract)
                    {
                        string hql = "select count(*) as counter from PlanBackflush where OrderNo = ? and IsClose = ?";
                        long counter = this.genericMgr.FindAll<long>(hql, new Object[] { orderMaster.OrderNo, false })[0];
                        if (counter > 0)
                        {
                            if (!isForce)
                            {
                                //非强制关闭直接返回
                                return;
                            }
                            businessException.AddMessage("加权平均扣料的零件还没有进行回冲，不能关闭订单{0}。", orderMaster.OrderNo);
                        }
                    }
                    #endregion

                    #region 条件6生产线上没有订单的投料
                    if (orderMaster.Type == CodeMaster.OrderType.Production
                        || orderMaster.Type == CodeMaster.OrderType.SubContract)
                    {
                        string hql = "select count(*) as counter from ProductLineLocationDetail where OrderNo = ? and IsClose = ?";
                        long counter = this.genericMgr.FindAll<long>(hql, new Object[] { orderMaster.OrderNo, false })[0];
                        if (counter > 0)
                        {
                            if (!isForce)
                            {
                                //非强制关闭直接返回
                                return;
                            }
                            businessException.AddMessage("生产线上还有投料的零件没有回冲，不能关闭订单{0}。", orderMaster.OrderNo);
                        }
                    }
                    #endregion
                    #endregion

                    if (businessException.HasMessage)
                    {
                        throw businessException;
                    }

                    orderMaster.Status = CodeMaster.OrderStatus.Close;
                    orderMaster.CloseDate = dateTimeNow;
                    orderMaster.CloseUserId = user.Id;
                    orderMaster.CloseUserName = user.FullName;
                    this.genericMgr.Update(orderMaster);
                }
                else if (!isForce)
                {
                    throw new BusinessException("不能关闭状态为{1}的订单{0}。", orderMaster.OrderNo,
                        systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
                }
            }
        }

        private void CompleteVanSubOrder(OrderMaster vanSubOrder)
        {
            DateTime dateTimeNow = DateTime.Now;
            User user = SecurityContextHolder.Get();

            vanSubOrder.Status = CodeMaster.OrderStatus.Complete;
            vanSubOrder.CompleteDate = dateTimeNow;
            vanSubOrder.CompleteUserId = user.Id;
            vanSubOrder.CompleteUserName = user.FullName;
            this.genericMgr.Update(vanSubOrder);
        }

        private BusinessException VerifyVanOrderClose(OrderMaster orderMaster, ProductLineMap productLineMap)
        {
            BusinessException businessException = new BusinessException();
            #region 生产单关闭校验
            #region 条件7和TraceCode所有的失效模式关闭
            if ((orderMaster.Type == CodeMaster.OrderType.Production
               || orderMaster.Type == CodeMaster.OrderType.SubContract)
               && !string.IsNullOrWhiteSpace(orderMaster.TraceCode))
            {
                #region 只有总装的生产单关闭才需要校验，驾驶室和底盘的不需要
                string hql = "select count(*) as counter from IssueMaster where BackYards = ? and Status in (?,?,?)";
                long issueCounter = this.genericMgr.FindAll<long>(hql, new Object[] { orderMaster.TraceCode, CodeMaster.IssueStatus.Create, CodeMaster.IssueStatus.Submit, CodeMaster.IssueStatus.InProcess })[0];
                if (issueCounter > 0)
                {
                    businessException.AddMessage("失效模式没有全部关闭，不能关闭订单{0}。", orderMaster.OrderNo);
                }
                #endregion
            }
            #endregion

            #region 条件8生产单上的所有关键件全部投料，考虑到搭错的关键件Bom，不需要校验这条，可以给出警告信息
            if ((orderMaster.Type == CodeMaster.OrderType.Production
               || orderMaster.Type == CodeMaster.OrderType.SubContract))
            {
                //throw new NotImplementedException();
            }
            #endregion
            #endregion

            return businessException;
        }
        #endregion

        #region 生产单暂停
        public void PauseProductOrder(string orderNo, int? pauseOperation)
        {
            try
            {
                User user = SecurityContextHolder.Get();
                this.genericMgr.UpdateWithNativeQuery("exec USP_Busi_PauseVanProdOrder ?, ? ,?,?",
                    new object[] { orderNo, pauseOperation, user.Id, user.FullName },
                    new IType[] { NHibernateUtil.String, NHibernateUtil.Int32, NHibernateUtil.Int32, NHibernateUtil.String });

                object[] obj = this.genericMgr.FindAllWithNativeSql<object[]>("select Flow, TraceCode from ORD_OrderMstr_4 WITH(NOLOCK) where OrderNo = ?", orderNo).Single();
                string prodLine = (string)obj[0];
                string traceCode = (string)obj[1];

                this.productionLineMgr.AsyncUpdateOrderBomCPTime(prodLine, user);

                #region 发送邮件
                string errorMessage = string.Empty;
                if (!pauseOperation.HasValue)
                {
                    IList<object[]> refOrderList = this.genericMgr.FindAllWithNativeSql<object[]>(
                        "select OrderNo, Flow from ORD_OrderMstr_4 WITH(NOLOCK) where TraceCode in (select TraceCode from ORD_OrderMstr_4 WITH(NOLOCK) where OrderNo = ?) and OrderNo <> ? and PauseStatus = ?",
                            new object[] { orderNo, orderNo, CodeMaster.PauseStatus.Paused });

                    if (refOrderList != null && refOrderList.Count > 0)
                    {
                        errorMessage = string.Format("整车生产线{0}Van号{1}生产单号{2}暂停成功，关联的", prodLine, traceCode, orderNo);
                        for (int i = 0; i < refOrderList.Count; i++)
                        {
                            object[] refOrder = refOrderList[i];
                            if (i == 0)
                            {
                                errorMessage += "整车生产线" + (string)refOrder[1] + "生产单号" + (string)refOrder[0];
                            }
                            else
                            {
                                errorMessage += "，整车生产线" + (string)refOrder[1] + "生产单号" + (string)refOrder[0];
                            }
                        }
                        errorMessage += "一起暂停成功。";
                    }
                    else
                    {
                        errorMessage = string.Format("整车生产线{0}Van号{1}生产单号{2}暂停成功。", prodLine, traceCode, orderNo);
                    }
                }
                else
                {
                    errorMessage = string.Format("整车生产线{0}Van号{1}生产单号{2}暂停成功，预计暂停工序为{3}。", prodLine, traceCode, orderNo, pauseOperation.Value);
                }
                log.Debug(errorMessage);

                IList<ErrorMessage> errorMessageList = new List<ErrorMessage>();
                errorMessageList.Add(new ErrorMessage
                {
                    Template = NVelocityTemplateRepository.TemplateEnum.PauseVanProdOrder,
                    Message = errorMessage,
                });

                this.SendErrorMessage(errorMessageList);
                #endregion

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

        public void BatchPauseProductOrder(string orderNos, int? pauseOp)
        {
            IList<OrderMaster> pauseOrders = this.genericMgr.FindAll<OrderMaster>(" select o from OrderMaster as o where o.OrderNo in (" + orderNos + ") ");
            if (pauseOrders.Where(o => o.Status == com.Sconit.CodeMaster.OrderStatus.InProcess).Count() > 0 && pauseOp == null)
            {
                throw new BusinessException("有上线车，请输入暂停工位。");
            }
            this.BatchPauseProductOrder(pauseOrders.Where(o => o.Status != com.Sconit.CodeMaster.OrderStatus.InProcess).Select(o => o.OrderNo).ToList(), null);
            this.BatchPauseProductOrder(pauseOrders.Where(o => o.Status == com.Sconit.CodeMaster.OrderStatus.InProcess).Select(o => o.OrderNo).ToList(), pauseOp);
        }

        public void BatchPauseProductOrder(IList<string> orderNoList, int? pauseOperation)
        {
            try
            {
                if (orderNoList != null && orderNoList.Count > 0)
                {
                    IList<ErrorMessage> errorMessageList = new List<ErrorMessage>();

                    User user = SecurityContextHolder.Get();
                    IList<string> prodLineList = new List<string>();
                    foreach (string orderNo in orderNoList)
                    {
                        this.genericMgr.UpdateWithNativeQuery("exec USP_Busi_PauseVanProdOrder ?, ?, ?, ?",
                            new object[] { orderNo, pauseOperation, user.Id, user.FullName },
                            new IType[] { NHibernateUtil.String, NHibernateUtil.Int32, NHibernateUtil.Int32, NHibernateUtil.String });
                        object[] obj = this.genericMgr.FindAllWithNativeSql<object[]>("select Flow, TraceCode from ORD_OrderMstr_4 WITH(NOLOCK) where OrderNo = ?", orderNo).Single();
                        string prodLine = (string)obj[0];
                        string traceCode = (string)obj[1];

                        if (!prodLineList.Contains(prodLine))
                        {
                            prodLineList.Add(prodLine);
                        }

                        #region 发送短消息
                        string errorMessage = string.Empty;
                        if (!pauseOperation.HasValue)
                        {
                            IList<object[]> refOrderList = this.genericMgr.FindAllWithNativeSql<object[]>(
                                "select OrderNo, Flow from ORD_OrderMstr_4 WITH(NOLOCK) where TraceCode in (select TraceCode from ORD_OrderMstr_4 WITH(NOLOCK) where OrderNo = ?) and OrderNo <> ? and PauseStatus = ?",
                                    new object[] { orderNo, orderNo, CodeMaster.PauseStatus.Paused });

                            if (refOrderList != null && refOrderList.Count > 0)
                            {
                                errorMessage = string.Format("整车生产线{0}Van号{1}生产单号{2}暂停成功，关联的", prodLine, traceCode, orderNo);
                                for (int i = 0; i < refOrderList.Count; i++)
                                {
                                    object[] refOrder = refOrderList[i];
                                    if (i == 0)
                                    {
                                        errorMessage += "整车生产线" + (string)refOrder[1] + "生产单号" + (string)refOrder[0];
                                    }
                                    else
                                    {
                                        errorMessage += "，整车生产线" + (string)refOrder[1] + "生产单号" + (string)refOrder[0];
                                    }
                                }
                                errorMessage += "一起暂停成功。";
                            }
                            else
                            {
                                errorMessage = string.Format("整车生产线{0}Van号{1}生产单号{2}暂停成功。", prodLine, traceCode, orderNo);
                            }
                        }
                        else
                        {
                            errorMessage = string.Format("整车生产线{0}Van号{1}生产单号{2}暂停成功，预计暂停工序为{3}。", prodLine, traceCode, orderNo, pauseOperation.Value);
                        }
                        log.Debug(errorMessage);

                        errorMessageList.Add(new ErrorMessage
                        {
                            Template = NVelocityTemplateRepository.TemplateEnum.PauseVanProdOrder,
                            Message = errorMessage,
                        });
                        #endregion
                    }

                    #region 更新物料消耗时间
                    foreach (string prodLine in prodLineList)
                    {
                        this.productionLineMgr.AsyncUpdateOrderBomCPTime(prodLine, user);
                    }
                    #endregion

                    this.SendErrorMessage(errorMessageList);
                }
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

        #region 生产单暂停恢复
        public void RestartProductOrder(string orderNo, string insertOrderNoBefore, bool isForce)
        {
            try
            {
                User user = SecurityContextHolder.Get();
                IList<string> msgList = this.genericMgr.FindAllWithNativeSql<string>("exec USP_Busi_RestartVanProdOrder ?,?,?,?,?",
                    new object[] { orderNo, insertOrderNoBefore, isForce, user.Id, user.FullName },
                    new IType[] { NHibernateUtil.String, NHibernateUtil.String, NHibernateUtil.Boolean, NHibernateUtil.Int32, NHibernateUtil.String });
                object[] obj = this.genericMgr.FindAllWithNativeSql<object[]>("select Flow, TraceCode from ORD_OrderMstr_4 WITH(NOLOCK) where OrderNo = ?", orderNo).Single();
                string prodLine = (string)obj[0];
                string traceCode = (string)obj[1];
                string insertTraceCodeBefore = this.genericMgr.FindAllWithNativeSql<string>("select TraceCode from ORD_OrderMstr_4 WITH(NOLOCK) where OrderNo = ?", insertOrderNoBefore).Single();

                this.productionLineMgr.AsyncUpdateOrderBomCPTime(prodLine, user);

                string errorMessage = string.Format("整车生产线{0}Van号{1}生产单号{2}成功恢复至Van号{3}生产单号{4}后。", prodLine, traceCode, orderNo, insertTraceCodeBefore, insertOrderNoBefore);
                log.Debug(errorMessage);
                IList<ErrorMessage> errorMessageList = new List<ErrorMessage>();
                errorMessageList.Add(new ErrorMessage
                {
                    Template = NVelocityTemplateRepository.TemplateEnum.RestartVanProdOrder,
                    Message = errorMessage,
                });

                if (msgList != null && msgList.Count() > 0)
                {
                    foreach (string msg in msgList)
                    {
                        MessageHolder.AddErrorMessage(msg);
                        errorMessageList.Add(new ErrorMessage
                        {
                            Template = NVelocityTemplateRepository.TemplateEnum.RestartVanProdOrder,
                            Message = msg,
                        });
                    }
                }

                SendErrorMessage(errorMessageList);
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

        [Transaction(TransactionMode.Requires)]
        public void BatchRestartProductOrder(IList<string> orderNoList, string insertOrderNoBefore, bool isForce)
        {
            try
            {
                if (orderNoList != null && orderNoList.Count > 0)
                {
                    User user = SecurityContextHolder.Get();

                    StringBuilder sql = new StringBuilder();
                    IList<object> param = new List<object>();
                    foreach (string orderNo in orderNoList)
                    {
                        if (sql.Length == 0)
                        {
                            sql.Append("select OrderNo from ORD_OrderSeq where OrderNo in (?");
                        }
                        else
                        {
                            sql.Append(",?");
                        }
                        param.Add(orderNo);
                    }
                    sql.Append(") order by Seq, SubSeq");
                    IList<string> insertOrderNoList = this.genericMgr.FindAllWithNativeSql<string>(sql.ToString(), param.ToArray());

                    string currentInsertOrderNoBefore = insertOrderNoBefore;
                    IList<string> prodLineList = new List<string>();
                    string errorMessage = string.Empty;
                    List<string> allMsgList = new List<string>();
                    foreach (string orderNo in insertOrderNoList)
                    {
                        IList<string> msgList = this.genericMgr.FindAllWithNativeSql<string>("exec USP_Busi_RestartVanProdOrder ?, ?, ?, ?, ?",
                            new object[] { orderNo, currentInsertOrderNoBefore, isForce, user.Id, user.FullName },
                            new IType[] { NHibernateUtil.String, NHibernateUtil.String, NHibernateUtil.Boolean, NHibernateUtil.Int32, NHibernateUtil.String });
                        currentInsertOrderNoBefore = orderNo;

                        object[] obj = this.genericMgr.FindAllWithNativeSql<object[]>("select Flow, TraceCode from ORD_OrderMstr_4 WITH(NOLOCK) where OrderNo = ?", orderNo).Single();
                        string prodLine = (string)obj[0];
                        string traceCode = (string)obj[1];
                        if (!prodLineList.Contains(prodLine))
                        {
                            prodLineList.Add(prodLine);
                        }

                        if (errorMessage == string.Empty)
                        {
                            errorMessage = string.Format("整车生产线{0}Van号{1}生产单号{2}", prodLine, traceCode, orderNo);
                        }
                        else
                        {
                            errorMessage += string.Format("、Van号{0}生产单号{1}", prodLine, traceCode, orderNo);
                        }

                        if (msgList != null && msgList.Count > 0)
                        {
                            allMsgList.AddRange(msgList);
                        }
                    }

                    foreach (string prodLine in prodLineList)
                    {
                        this.productionLineMgr.AsyncUpdateOrderBomCPTime(prodLine, user);
                    }

                    string insertTraceCodeBefore = this.genericMgr.FindAllWithNativeSql<string>("select TraceCode from ORD_OrderMstr_4 WITH(NOLOCK) where OrderNo = ?", insertOrderNoBefore).Single();
                    errorMessage += string.Format("成功恢复至Van号{0}生产单号{1}后。", insertTraceCodeBefore, insertOrderNoBefore);
                    log.Debug(errorMessage);
                    IList<ErrorMessage> errorMessageList = new List<ErrorMessage>();
                    errorMessageList.Add(new ErrorMessage
                    {
                        Template = NVelocityTemplateRepository.TemplateEnum.RestartVanProdOrder,
                        Message = errorMessage,
                    });

                    if (allMsgList != null && allMsgList.Count() > 0)
                    {
                        foreach (string msg in allMsgList)
                        {
                            MessageHolder.AddErrorMessage(msg);
                            errorMessageList.Add(new ErrorMessage
                            {
                                Template = NVelocityTemplateRepository.TemplateEnum.RestartVanProdOrder,
                                Message = msg,
                            });
                        }
                    }

                    SendErrorMessage(errorMessageList);
                }
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

        #region 加载订单
        public OrderMaster LoadOrderMaster(string orderNo, bool includeDetail, bool includeOperation, bool includeBomDetail)
        {
            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderNo);

            if (includeDetail || includeOperation || includeBomDetail)
            {
                orderMaster.OrderDetails = this.genericMgr.FindAll<OrderDetail>("from OrderDetail o where o.OrderNo=?", orderNo);
                foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                {
                    if (includeBomDetail)
                    {
                        orderDetail.OrderBomDetails = this.genericMgr.FindAll<OrderBomDetail>("from OrderBomDetail o where o.OrderDetailId=?", orderDetail.Id);
                    }
                    if (includeOperation)
                    {
                        orderDetail.OrderOperations = this.genericMgr.FindAll<OrderOperation>("from OrderOperation o where o.OrderDetailId=?", orderDetail.Id);
                    }
                }
            }
            return orderMaster;
        }
        #endregion

        #region 根据投料的条码查找投料的工位
        public IList<OrderOperation> FindFeedOrderOperation(string orderNo, string huId)
        {
            throw null;
        }
        #endregion

        #region 路线可用性检查
        [Transaction(TransactionMode.Requires)]
        public void CheckOrder(string orderNo)
        {
            OrderMaster orderMaster = genericMgr.FindById<OrderMaster>(orderNo);
            this.CheckOrder(orderMaster);
        }

        [Transaction(TransactionMode.Requires)]
        public void CheckOrder(OrderMaster orderMaster)
        {
            BusinessException ex = new BusinessException();
            if (orderMaster.Type != com.Sconit.CodeMaster.OrderType.Production)
            {
                ex.AddMessage("订单" + orderMaster.OrderNo + "不是生产单");
            }
            else
            {
                TryLoadOrderBomDetails(orderMaster);
                if (orderMaster.OrderDetails != null && orderMaster.OrderDetails.Count > 0)
                {
                    foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                    {
                        if (orderDetail.OrderBomDetails != null && orderDetail.OrderBomDetails.Count > 0)
                        {
                            //循环检查太慢了，先检查一层,而且只查有明细的
                            var locationList = orderDetail.OrderBomDetails.Select(b => b.Location).Distinct().ToList();
                            foreach (string location in locationList)
                            {

                                FlowDetail transferFlowDetail = null;

                                string hql = "from FlowDetail as d where exists (select 1 from FlowMaster as f where f.Code = d.Flow and f.IsActive = ? and (d.LocationTo = ? or (d.LocationTo is null and f.LocationTo = ?)))";
                                IList<FlowDetail> transferFlowDetailList = genericMgr.FindAll<FlowDetail>(hql, new object[] { true, location, location });

                                var lob = orderDetail.OrderBomDetails.Where(p => p.Location == location).ToList(); //发到此库位的bom明细

                                #region 找到有路线明细的
                                foreach (OrderBomDetail orderBomDetail in lob)
                                {
                                    transferFlowDetail = transferFlowDetailList.Where(f => f.Item == orderBomDetail.Item).ToList().FirstOrDefault();
                                    if (transferFlowDetail == null)
                                    {
                                        ex.AddMessage("物料" + orderBomDetail.Item + "没有找到对应的" + orderBomDetail.Location + "库位的路线");
                                        continue;
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                    if (ex.GetMessages() != null && ex.GetMessages().Count > 0)
                    {
                        throw ex;
                    }
                }
            }
        }

        private FlowMaster GetSourceFlow(string item, string location, IList<string> flowLocations)
        {
            FlowMaster sourceFlow = null;

            IList<FlowMaster> flowList = genericMgr.FindAll<FlowMaster>("from FlowMaster f where (f.Type = ? and f.IsManualCreateDetail = ?) or exists (select 1 from FlowDetail d where f.Code = d.Flow and d.Item = ? and (d.LocationTo = ? or (d.LocationTo is null and f.LocationTo = ?)) )", new object[] { (int)com.Sconit.CodeMaster.OrderType.Transfer, true, item, location, location });
            if (flowList != null && flowList.Count > 0)
            {
                //找到客供，采购，生产，委外都可以
                var q = flowList.Where(f => f.Type == com.Sconit.CodeMaster.OrderType.Procurement || f.Type == com.Sconit.CodeMaster.OrderType.CustomerGoods || f.Type == com.Sconit.CodeMaster.OrderType.Production || f.Type == com.Sconit.CodeMaster.OrderType.SubContract);
                if (q.ToList() != null && q.ToList().Count > 0)
                {
                    sourceFlow = q.ToList().First();
                }
                else
                {
                    flowLocations.Add(location);
                    foreach (FlowMaster flow in flowList)
                    {
                        if (!flowLocations.Contains(flow.LocationFrom))
                        {
                            sourceFlow = GetSourceFlow(item, flow.LocationFrom, flowLocations);
                        }
                        if (sourceFlow != null)
                        {
                            break;
                        }
                    }
                }
            }

            return sourceFlow;
        }
        #endregion

        #region 创建不合格品退货单
        [Transaction(TransactionMode.Requires)]
        public void CreateReturnOrder(FlowMaster flowMaster, IList<RejectDetail> rejectDetailList)
        {
            if (rejectDetailList == null || rejectDetailList.Count == 0)
            {
                throw new BusinessException("退货明细不能为空.");
            }

            #region 根据路线生成退货单
            FlowMaster returnFlow = flowMgr.GetReverseFlow(flowMaster, rejectDetailList.Select(r => r.Item).Distinct().ToList());
            OrderMaster orderMaster = TransferFlow2Order(returnFlow, null);
            orderMaster.IsQuick = true;
            orderMaster.SubType = CodeMaster.OrderSubType.Return;
            orderMaster.WindowTime = DateTime.Now;
            orderMaster.StartTime = DateTime.Now;
            orderMaster.EffectiveDate = DateTime.Now;
            orderMaster.QualityType = CodeMaster.QualityType.Reject;
            orderMaster.ReferenceOrderNo = rejectDetailList[0].RejectNo;
            #endregion

            #region 不合格品单明细变成退货单明细
            IList<OrderDetail> nonZeroOrderDetailList = new List<OrderDetail>();
            foreach (RejectDetail rejectDetail in rejectDetailList)
            {
                if (rejectDetail.HandleQty < rejectDetail.HandledQty + rejectDetail.CurrentHandleQty)
                {
                    throw new BusinessException("不合格品单物料{0}本次处理数加已处理数超过总处理数.", rejectDetail.Item);
                }

                if (orderMaster.OrderDetails != null && orderMaster.OrderDetails.Count > 0)
                {
                    var q = orderMaster.OrderDetails.Where(o => o.Item == rejectDetail.Item).ToList();
                    if (q.ToList() == null || q.ToList().Count == 0)
                    {
                        throw new BusinessException("不合格品单物料{0}没找到对应的路线明细.", rejectDetail.Item);
                    }
                    OrderDetail orderDetail = Mapper.Map<OrderDetail, OrderDetail>(q.ToList().First());
                    orderDetail.QualityType = com.Sconit.CodeMaster.QualityType.Reject;
                    orderDetail.Uom = rejectDetail.Uom;
                    orderDetail.OrderedQty = rejectDetail.CurrentHandleQty;
                    orderDetail.LocationFrom = rejectDetail.LocationFrom;
                    orderDetail.LocationFromName = genericMgr.FindById<Location>(rejectDetail.LocationFrom).Name;
                    orderDetail.ManufactureParty = rejectDetail.ManufactureParty;
                    nonZeroOrderDetailList.Add(orderDetail);
                }
                else
                {
                    throw new BusinessException("不合格品单物料{0}没找到对应的路线明细.", rejectDetail.Item);
                }

            }
            #endregion

            #region 创建退货单
            orderMaster.OrderDetails = nonZeroOrderDetailList;
            CreateOrder(orderMaster);
            #endregion

            #region 更新明细的已处理不合格品处理单状态
            foreach (RejectDetail rejectDetail in rejectDetailList)
            {
                rejectDetail.HandledQty += rejectDetail.CurrentHandleQty;
                genericMgr.Update(rejectDetail);
            }
            string hql = "from RejectDetail as r where r.RejectNo = ?";
            IList<RejectDetail> remainRejectDetailList = genericMgr.FindAll<RejectDetail>(hql, rejectDetailList[0].RejectNo);
            var m = remainRejectDetailList.Where(r => (r.HandledQty < r.HandleQty)).ToList();
            if (m == null || m.Count == 0)
            {
                RejectMaster rejectMaster = genericMgr.FindById<RejectMaster>(rejectDetailList[0].RejectNo);
                rejectMaster.Status = CodeMaster.RejectStatus.Close;
                genericMgr.Update(rejectMaster);
            }
            #endregion
        }
        #endregion

        #region 创建不合格品移库单
        [Transaction(TransactionMode.Requires)]
        public void CreateRejectTransfer(Location location, IList<RejectDetail> rejectDetailList)
        {
            var orderMaster = new Entity.ORD.OrderMaster();
            var rejectNoList = (from rej in rejectDetailList select rej.RejectNo).Distinct().ToList();
            if (rejectNoList.Count() > 1)
            {
                throw new BusinessException("多个不合格品处理单不能合并移库。");
            }

            RejectMaster rejectMaster = genericMgr.FindById<RejectMaster>(rejectNoList[0]);

            var locationFrom = this.genericMgr.FindById<Entity.MD.Location>(rejectDetailList[0].LocationFrom);
            var partyFrom = this.genericMgr.FindById<Entity.MD.Party>(locationFrom.Region);
            var partyTo = this.genericMgr.FindById<Entity.MD.Party>(location.Region);

            orderMaster.LocationFrom = locationFrom.Code;
            orderMaster.IsShipScanHu = rejectMaster.InspectType == com.Sconit.CodeMaster.InspectType.Barcode;
            orderMaster.IsReceiveScanHu = rejectMaster.InspectType == com.Sconit.CodeMaster.InspectType.Barcode;
            orderMaster.LocationFromName = locationFrom.Name;
            orderMaster.LocationTo = location.Code;
            orderMaster.LocationToName = location.Name;
            orderMaster.PartyFrom = partyFrom.Code;
            orderMaster.PartyFromName = partyFrom.Name;
            orderMaster.PartyTo = partyTo.Code;
            orderMaster.PartyToName = partyTo.Name;
            orderMaster.Type = CodeMaster.OrderType.Transfer;
            orderMaster.StartTime = DateTime.Now;
            orderMaster.WindowTime = DateTime.Now;
            orderMaster.EffectiveDate = DateTime.Now;
            orderMaster.QualityType = com.Sconit.CodeMaster.QualityType.Reject;

            orderMaster.IsQuick = true;
            orderMaster.OrderDetails = new List<OrderDetail>();
            int seq = 1;

            var groupRejectDetailList = from r in rejectDetailList group r by new { r.Item, r.CurrentLocation } into result select result;


            foreach (var rejectDetail in groupRejectDetailList)
            {
                var currentRejectDetailList = rejectDetailList.Where(p => p.Item == rejectDetail.Key.Item && p.CurrentLocation == rejectDetail.Key.CurrentLocation).ToList();

                var orderDetail = new OrderDetail();
                var orderDetailInputList = new List<OrderDetailInput>();
                Mapper.Map(currentRejectDetailList[0], orderDetail);
                orderDetail.OrderType = com.Sconit.CodeMaster.OrderType.Transfer;
                orderDetail.QualityType = com.Sconit.CodeMaster.QualityType.Inspect;
                orderDetail.LocationFrom = rejectDetail.Key.CurrentLocation;
                orderDetail.LocationFromName = genericMgr.FindById<Location>(rejectDetail.Key.CurrentLocation).Name;
                orderDetail.LocationTo = location.Code;
                orderDetail.LocationToName = location.Name;
                orderDetail.Sequence = seq++;

                foreach (RejectDetail rej in currentRejectDetailList)
                {
                    var orderDetailInput = new OrderDetailInput();
                    if (rejectMaster.InspectType == com.Sconit.CodeMaster.InspectType.Barcode)
                    {
                        orderDetailInput.HuId = rej.HuId;
                        orderDetailInput.LotNo = rej.LotNo;
                    }

                    orderDetailInput.QualityType = com.Sconit.CodeMaster.QualityType.Reject;
                    orderDetailInput.ReceiveQty = rej.CurrentTransferQty;

                    orderDetail.RequiredQty += rej.CurrentTransferQty;
                    orderDetail.OrderedQty += rej.CurrentTransferQty;
                    orderDetailInputList.Add(orderDetailInput);

                }
                orderDetail.OrderDetailInputs = orderDetailInputList;
                orderMaster.OrderDetails.Add(orderDetail);
            }

            CreateOrder(orderMaster);

            #region 更新检验明细
            foreach (RejectDetail rej in rejectDetailList)
            {
                rej.CurrentLocation = location.Code;
                genericMgr.Update(rej);
            }
            #endregion
        }
        #endregion

        #region 创建待验品移库单
        [Transaction(TransactionMode.Requires)]
        public void CreateInspectTransfer(Location location, IList<InspectDetail> inspectDetailList)
        {
            var orderMaster = new Entity.ORD.OrderMaster();
            var inspectNoList = (from inp in inspectDetailList select inp.InspectNo).Distinct().ToList();
            if (inspectNoList.Count() > 1)
            {
                throw new BusinessException("多个报验单待验明细不能合并移库。");
            }

            InspectMaster inspectMaster = genericMgr.FindById<InspectMaster>(inspectNoList[0]);

            var locationFrom = this.genericMgr.FindById<Entity.MD.Location>(inspectDetailList[0].LocationFrom);
            var partyFrom = this.genericMgr.FindById<Entity.MD.Party>(locationFrom.Region);
            var partyTo = this.genericMgr.FindById<Entity.MD.Party>(location.Region);

            orderMaster.LocationFrom = locationFrom.Code;
            orderMaster.IsShipScanHu = inspectMaster.Type == com.Sconit.CodeMaster.InspectType.Barcode;
            orderMaster.IsReceiveScanHu = inspectMaster.Type == com.Sconit.CodeMaster.InspectType.Barcode;
            orderMaster.LocationFromName = locationFrom.Name;
            orderMaster.LocationTo = location.Code;
            orderMaster.LocationToName = location.Name;
            orderMaster.PartyFrom = partyFrom.Code;
            orderMaster.PartyFromName = partyFrom.Name;
            orderMaster.PartyTo = partyTo.Code;
            orderMaster.PartyToName = partyTo.Name;
            orderMaster.Type = CodeMaster.OrderType.Transfer;
            orderMaster.StartTime = DateTime.Now;
            orderMaster.WindowTime = DateTime.Now;
            orderMaster.EffectiveDate = DateTime.Now;
            orderMaster.QualityType = com.Sconit.CodeMaster.QualityType.Inspect;

            orderMaster.IsQuick = true;
            orderMaster.OrderDetails = new List<OrderDetail>();
            int seq = 1;

            var groupInspectDetailList = from d in inspectDetailList group d by new { d.Item, d.CurrentLocation } into result select result;


            foreach (var inspectDetail in groupInspectDetailList)
            {
                var currentInspectDetailList = inspectDetailList.Where(p => p.Item == inspectDetail.Key.Item && p.CurrentLocation == inspectDetail.Key.CurrentLocation).ToList();

                var orderDetail = new OrderDetail();
                var orderDetailInputList = new List<OrderDetailInput>();
                Mapper.Map(currentInspectDetailList[0], orderDetail);
                orderDetail.OrderType = com.Sconit.CodeMaster.OrderType.Transfer;
                orderDetail.QualityType = com.Sconit.CodeMaster.QualityType.Inspect;
                orderDetail.LocationFrom = inspectDetail.Key.CurrentLocation;
                orderDetail.LocationFromName = genericMgr.FindById<Location>(inspectDetail.Key.CurrentLocation).Name;
                orderDetail.LocationTo = location.Code;
                orderDetail.LocationToName = location.Name;
                orderDetail.Sequence = seq++;

                foreach (InspectDetail insp in currentInspectDetailList)
                {
                    var orderDetailInput = new OrderDetailInput();
                    if (inspectMaster.Type == com.Sconit.CodeMaster.InspectType.Barcode)
                    {
                        orderDetailInput.HuId = insp.HuId;
                        orderDetailInput.LotNo = insp.LotNo;
                    }

                    orderDetailInput.QualityType = com.Sconit.CodeMaster.QualityType.Inspect;
                    orderDetailInput.OccupyType = com.Sconit.CodeMaster.OccupyType.Inspect;
                    orderDetailInput.OccupyReferenceNo = inspectMaster.InspectNo;
                    orderDetailInput.ReceiveQty = insp.CurrentTransferQty;

                    orderDetail.RequiredQty += insp.CurrentTransferQty;
                    orderDetail.OrderedQty += insp.CurrentTransferQty;
                    orderDetailInputList.Add(orderDetailInput);

                }
                orderDetail.OrderDetailInputs = orderDetailInputList;
                orderMaster.OrderDetails.Add(orderDetail);
            }


            CreateOrder(orderMaster);

            #region 更新检验明细
            foreach (InspectDetail insp in inspectDetailList)
            {
                insp.CurrentLocation = location.Code;
                genericMgr.Update(insp);
            }
            #endregion
        }
        #endregion

        #region 生产单手工拉料
        [Transaction(TransactionMode.Requires)]
        public string[] CreateRequisitionList(string orderNo)
        {
            return this.CreateRequisitionList(this.genericMgr.FindById<OrderMaster>(orderNo));
        }

        [Transaction(TransactionMode.Requires)]
        //返回2个列表，1是生成的订单，2是没拉出来的物料
        public string[] CreateRequisitionList(OrderMaster orderMaster)
        {
            string orderString = string.Empty;
            string itemString = string.Empty;

            if (orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Submit && orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.InProcess)
            {
                throw new BusinessException("状态为{1}的试制生产单{0}不能产生拉料单。", orderMaster.OrderNo,
                    systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, (int)orderMaster.Status));
            }

            #region 去掉KB件，保存到表里，供查询
            string kbCountSql = "select count(*) as count from ORD_OrderBomdet as b inner join ORD_KBOrderBomDet as k on b.Id = k.OrderBomDetId where b.OrderNo = ?";
            IList<object> kbBomDetailCount = genericMgr.FindAllWithNativeSql<object>(kbCountSql, new object[] { orderMaster.OrderNo });
            if ((int)kbBomDetailCount[0] == 0)
            {
                string kbSql = "select d.Id,A.Flow from ORD_OrderBomdet as d inner join (select f.Item,f.Flow,case when f.LocTo is null then m.LocTo else f.LocTo end as LocTo from  SCM_FlowDet as f  inner join SCM_FlowMstr as m on f.Flow = m.Code where  m.FlowStrategy = ? and f.StartDate < ? and (f.EndDate is null or f.EndDate > ?))A on d.Item = A.Item and d.Location = A.LocTo where d.OrderNo = ?";
                IList<object[]> kbBomDetailList = genericMgr.FindAllWithNativeSql<object[]>(kbSql, new object[] { (int)com.Sconit.CodeMaster.FlowStrategy.KB, DateTime.Now, DateTime.Now, orderMaster.OrderNo });
                if (kbBomDetailList.Count > 0)
                {
                    foreach (object[] ob in kbBomDetailList)
                    {
                        KBOrderBomDetail kbOrderBomDetail = new KBOrderBomDetail();
                        kbOrderBomDetail.OrderBomDetId = (int)ob[0];
                        kbOrderBomDetail.Flow = (string)ob[1];
                        genericMgr.Create(kbOrderBomDetail);
                    }
                }
            }

            #endregion

            #region 得出去掉KB件的其他件
            string bomDetailSql = "select d.Item,d.Location,d.Uom,d.OrderQty from ORD_OrderBomdet as d where d.OrderNo = ? and not exists (select 1 from  SCM_FlowDet as f  inner join SCM_FlowMstr as m on f.Flow = m.Code where d.Item = f.Item and  ((f.LocTo is not null and d.Location = f.LocTo) or (f.LocTo is null and d.Location = m.LocTo)) and m.FlowStrategy = ?)";
            IList<object[]> orderBomDetailList = genericMgr.FindAllWithNativeSql<object[]>(bomDetailSql, new object[] { orderMaster.OrderNo, (int)com.Sconit.CodeMaster.FlowStrategy.KB });
            // IList<OrderBomDetail> orderBomDetailList = TryLoadOrderBomDetails(orderMaster);
            #endregion

            #region 过滤掉负的
            var groupOrderBomDetailList = (from det in orderBomDetailList
                                           where (decimal)det[3] > 0
                                           group det by new { Item = (string)det[0], Location = (string)det[1], Uom = (string)det[2] } into result
                                           select new OrderBomDetail
                                           {
                                               Item = result.Key.Item,
                                               Location = result.Key.Location,
                                               Uom = result.Key.Uom,
                                               OrderedQty = result.Sum(t => (decimal)t[3])
                                           }).ToList();
            #endregion

            #region 已经拉过料的
            IList<OrderDetail> orderDetailList = genericMgr.FindAll<OrderDetail>("from OrderDetail as d where exists (select 1 from OrderMaster as m where m.OrderNo = d.OrderNo and m.Type = ? and m.ReferenceOrderNo = ?)", new object[] { (int)com.Sconit.CodeMaster.OrderType.Transfer, orderMaster.OrderNo });

            var groupOrderDetailList = (from det in orderDetailList
                                        group det by new { Item = det.Item, Location = det.LocationTo, Uom = det.Uom } into result
                                        select new OrderBomDetail
                                        {
                                            Item = result.Key.Item,
                                            Location = result.Key.Location,
                                            OrderedQty = result.Sum(t => (t.OrderedQty * t.UnitQty))
                                        }).ToList();
            #endregion

            #region 求出实际需求
            var exactOrderBomDetailList = (from b in groupOrderBomDetailList
                                           join d in groupOrderDetailList
                                           on new
                                                {
                                                    Location = b.Location,
                                                    Item = b.Item
                                                }
                                               equals
                                               new
                                                {
                                                    Location = d.Location,
                                                    Item = d.Item
                                                }
                                           into bd
                                           from result in bd.DefaultIfEmpty()
                                           select new OrderBomDetail
                                           {
                                               Location = b.Location,
                                               Item = b.Item,
                                               Uom = b.Uom,
                                               OrderedQty = b.OrderedQty - (result != null ? result.OrderedQty * itemMgr.ConvertItemUomQty(b.Item, genericMgr.FindById<Item>(result.Item).Uom, 1, b.Uom) : 0)
                                           }).ToList().Where(p => p.OrderedQty > 0);

            if (exactOrderBomDetailList == null || exactOrderBomDetailList.Count() == 0)
            {
                throw new BusinessException("试制车生产单{0}物料清单为空。", orderMaster.OrderNo);
            }
            #endregion

            #region 把需求按库位分一下，应该会少一些
            IList<OrderMaster> orderMasterList = new List<OrderMaster>();
            var locationList = exactOrderBomDetailList.Select(b => b.Location).Distinct().ToList();
            foreach (string location in locationList)
            {
                FlowMaster transferFlow = null;
                FlowDetail transferFlowDetail = null;
                bool isShipScanHu = false;
                bool isReceiveScanHu = false;

                string hql = "from FlowDetail as d where exists (select 1 from FlowMaster as f where f.Code = d.Flow and f.Type = ? and f.IsActive = ? and (d.LocationTo = ? or (d.LocationTo is null and f.LocationTo = ?)))";
                IList<FlowDetail> transferFlowDetailList = genericMgr.FindAll<FlowDetail>(hql, new object[] { com.Sconit.CodeMaster.OrderType.Transfer, true, location, location });

                var lob = exactOrderBomDetailList.Where(p => p.Location == location).ToList(); //发到此库位的bom明细
                var nlob = new List<OrderBomDetail>();                                        //没找到路线明细的orderbomdet
                #region 找到有路线明细的
                foreach (OrderBomDetail orderBomDetail in lob)
                {
                    transferFlowDetail = transferFlowDetailList.Where(f => f.Item == orderBomDetail.Item).ToList().FirstOrDefault();
                    if (transferFlowDetail != null)
                    {
                        transferFlow = genericMgr.FindById<FlowMaster>(transferFlowDetail.Flow);
                        isShipScanHu = transferFlow.IsShipScanHu;
                        isReceiveScanHu = transferFlow.IsReceiveScanHu;

                        if (transferFlow.IsAutoCreate && transferFlowDetail.IsAutoCreate)
                        {
                            #region 自动拉料的不需要拉
                            continue;
                            #endregion
                        }

                        #region 建订单
                        OrderMaster transferOrderMaster = orderMasterList.Where(o => o.Flow == transferFlow.Code && o.IsShipScanHu == isShipScanHu && o.IsReceiveScanHu == isReceiveScanHu).ToList().SingleOrDefault<OrderMaster>();
                        if (transferOrderMaster == null)
                        {
                            OrderMaster productionOrder = genericMgr.FindById<OrderMaster>(orderMaster.OrderNo);
                            transferOrderMaster = TransferFlow2Order(transferFlow, null);
                            transferOrderMaster.ReferenceOrderNo = orderMaster.OrderNo;
                            transferOrderMaster.StartTime = DateTime.Now;
                            transferOrderMaster.WindowTime = DateTime.Now;
                            transferOrderMaster.TraceCode = productionOrder.TraceCode;
                            transferOrderMaster.IsShipScanHu = isShipScanHu;
                            transferOrderMaster.IsReceiveScanHu = isReceiveScanHu;
                            transferOrderMaster.IsOrderFulfillUC = false;
                            transferOrderMaster.IsShipFulfillUC = false;
                            transferOrderMaster.IsReceiveFulfillUC = false;
                            if (transferOrderMaster.OrderDetails == null)
                            {
                                transferOrderMaster.OrderDetails = new List<OrderDetail>();
                            }

                            orderMasterList.Add(transferOrderMaster);
                        }

                        OrderDetail orderDetail = Mapper.Map<OrderDetail, OrderDetail>(transferOrderMaster.OrderDetails.Where(d => d.Item == orderBomDetail.Item && (d.LocationTo == orderBomDetail.Location || (d.LocationTo == null && transferOrderMaster.LocationTo == orderBomDetail.Location))).First());
                        if (orderDetail.Uom != orderBomDetail.Uom)
                        {
                            orderDetail.UnitQty = this.itemMgr.ConvertItemUomQty(orderBomDetail.Item, orderDetail.Uom, 1, orderBomDetail.Uom);
                            orderDetail.OrderedQty = orderBomDetail.OrderedQty / orderDetail.UnitQty;
                        }
                        else
                        {
                            orderDetail.UnitQty = 1;
                            orderDetail.OrderedQty = orderBomDetail.OrderedQty;
                        }
                        transferOrderMaster.OrderDetails.Add(orderDetail);
                        #endregion
                    }
                    else
                    {
                        nlob.Add(orderBomDetail);
                    }
                }
                #endregion

                #region 没有路线明细的
                if (nlob.Count > 0)
                {
                    #region
                    //根据采购路线查找生产单BOM物料的采购入库地点，作为来源库位，取生产单BOM的库位为目的库位，生成要货单。
                    //因为一条路线上可能包含关键件和非关键件,收货入库可能条码也可能数量
                    //发货是否扫描条码要跟据采购路线的收货扫描条码选项
                    //收货是否扫描条码要根据是否关键件（itemtrace）
                    foreach (OrderBomDetail orderBomDetail in nlob)
                    {
                        //采购的以头上的为准
                        string procuremenSql = "select case when d.LocTo is null then m.LocTo else d.LocTo end as LocTo,m.IsRecScanHu from SCM_FlowDet as d inner join SCM_FlowMstr as m on d.Flow = m.Code  where m.Type = ? and m.IsActive = ? and d.Item = ?";
                        IList<object[]> procurementFlowList = genericMgr.FindAllWithNativeSql<object[]>(procuremenSql, new object[] { com.Sconit.CodeMaster.OrderType.Procurement, true, orderBomDetail.Item });
                        if (procurementFlowList == null || procurementFlowList.Count == 0)
                        {
                            // throw new BusinessException("找不到物料{0}对应的采购路线", orderBomDetail.Item);
                            itemString += string.IsNullOrEmpty(itemString) ? orderBomDetail.Item : "," + orderBomDetail.Item;
                            continue;
                        }
                        object[] procurementFlow = procurementFlowList[0];
                        hql = "from FlowMaster as f where f.Type = ? and  f.LocationFrom = ? and f.LocationTo = ? and f.IsActive = ? and f.IsAutoCreate = ?";
                        IList<FlowMaster> transferFlowList = genericMgr.FindAll<FlowMaster>(hql, new object[] { com.Sconit.CodeMaster.OrderType.Transfer, (string)procurementFlow[0], orderBomDetail.Location, true, false });
                        if (transferFlowList == null || transferFlowList.Count == 0)
                        {
                            // throw new BusinessException("找不到物料{0}对应的来源库位{1},目的库位{2}的移库路线", orderBomDetail.Item);
                            itemString += string.IsNullOrEmpty(itemString) ? (string)procurementFlow[0] + ":" + orderBomDetail.Item : "," + (string)procurementFlow[0] + ":" + orderBomDetail.Item;
                            continue;
                        }

                        transferFlow = transferFlowList[0];
                        isShipScanHu = (bool)procurementFlow[1];
                        //IList<ItemTrace> itemTraceList = genericMgr.FindAll<ItemTrace>("from ItemTrace as i where i.Item = ?", orderBomDetail.Item);
                        //isReceiveScanHu = (itemTraceList == null || itemTraceList.Count() == 0) ? false : true;

                        #region 建订单
                        OrderMaster transferOrderMaster = orderMasterList.Where(o => o.Flow == transferFlow.Code && o.IsShipScanHu == isShipScanHu && o.IsReceiveScanHu == isReceiveScanHu).ToList().SingleOrDefault<OrderMaster>();
                        if (transferOrderMaster == null)
                        {
                            OrderMaster productionOrder = genericMgr.FindById<OrderMaster>(orderMaster.OrderNo);
                            transferOrderMaster = TransferFlow2Order(transferFlow, null);
                            transferOrderMaster.ReferenceOrderNo = orderMaster.OrderNo;
                            transferOrderMaster.StartTime = DateTime.Now;
                            transferOrderMaster.WindowTime = DateTime.Now;
                            transferOrderMaster.TraceCode = productionOrder.TraceCode;
                            transferOrderMaster.IsShipScanHu = isShipScanHu;
                            transferOrderMaster.IsReceiveScanHu = isReceiveScanHu;
                            transferOrderMaster.IsOrderFulfillUC = false;
                            transferOrderMaster.IsShipFulfillUC = false;
                            transferOrderMaster.IsReceiveFulfillUC = false;
                            if (transferOrderMaster.OrderDetails == null)
                            {
                                transferOrderMaster.OrderDetails = new List<OrderDetail>();
                            }

                            orderMasterList.Add(transferOrderMaster);
                        }

                        OrderDetail orderDetail = new OrderDetail();

                        Mapper.Map(orderBomDetail, orderDetail);
                        Item item = genericMgr.FindById<Item>(orderBomDetail.Item);
                        orderDetail.ItemDescription = item.Description;
                        orderDetail.UnitCount = item.UnitCount;
                        orderDetail.BaseUom = item.Uom;
                        orderDetail.LocationFrom = transferOrderMaster.LocationFrom;
                        orderDetail.LocationTo = transferOrderMaster.LocationTo;
                        orderDetail.LocationFromName = transferOrderMaster.LocationFromName;
                        orderDetail.LocationToName = transferOrderMaster.LocationToName;
                        transferOrderMaster.OrderDetails.Add(orderDetail);

                        #endregion
                    }
                    #endregion
                }
                #endregion

                #region 老代码
                //foreach (OrderBomDetail orderBomDetail in exactOrderBomDetailList)
                //{
                //    //如果根据bom中的子物料以及库位能够对应到路线明细，则以该路线明细来生成要货单
                //    //如果根据以上无法得到路线明细，则根据采购路线查找生产单BOM物料的采购入库地点，作为来源库位，取生产单BOM的库位为目的库位，生成要货单。

                //    FlowMaster transferFlow = null;
                //    FlowDetail transferFlowDetail = null;

                //    string hql = "from FlowDetail as d where d.Item = ? and exists (select 1 from FlowMaster as f where f.Code = d.Flow and f.Type = ? and f.IsActive = ? and (d.LocationTo = ? or (d.LocationTo is null and f.LocationTo = ?))) order by d.IsAutoCreate desc";
                //    IList<FlowDetail> transferFlowDetailList = genericMgr.FindAll<FlowDetail>(hql, new object[] { orderBomDetail.Item, com.Sconit.CodeMaster.OrderType.Transfer, true, orderBomDetail.Location, orderBomDetail.Location });

                //    bool isShipScanHu = false;
                //    bool isReceiveScanHu = false;
                //    if (transferFlowDetailList != null && transferFlowDetailList.Count > 0)
                //    {
                //        transferFlow = genericMgr.FindById<FlowMaster>(transferFlowDetailList[0].Flow);
                //        isShipScanHu = transferFlow.IsShipScanHu;
                //        isReceiveScanHu = transferFlow.IsReceiveScanHu;
                //        transferFlowDetail = transferFlowDetailList[0];
                //        if (transferFlow.IsAutoCreate && transferFlowDetail.IsAutoCreate)
                //        {
                //            #region 自动拉料的不需要拉
                //            continue;
                //            #endregion
                //        }
                //    }
                //    else
                //    {
                //        #region 则根据采购路线查找生产单BOM物料的采购入库地点，作为来源库位，取生产单BOM的库位为目的库位，生成要货单。
                //        //因为一条路线上可能包含关键件和非关键件,收货入库可能条码也可能数量
                //        //发货是否扫描条码要跟据采购路线的收货扫描条码选项
                //        //收货是否扫描条码要根据是否关键件（itemtrace）
                //        FlowMaster procurementFlow = GetSourceFlow(orderBomDetail.Item, orderBomDetail.Location, new List<string>());
                //        if (procurementFlow == null)
                //        {
                //            // throw new BusinessException("找不到物料{0}对应的采购路线", orderBomDetail.Item);
                //            itemString += string.IsNullOrEmpty(itemString) ? orderBomDetail.Item : "," + orderBomDetail.Item;
                //            continue;
                //        }
                //        hql = "from FlowMaster as f where f.Type = ? and  f.LocationFrom = ? and f.LocationTo = ? and f.IsActive = ?";
                //        IList<FlowMaster> transferFlowList = genericMgr.FindAll<FlowMaster>(hql, new object[] { com.Sconit.CodeMaster.OrderType.Transfer, procurementFlow.LocationTo, orderBomDetail.Location, true });
                //        if (transferFlowList == null || transferFlowList.Count == 0)
                //        {
                //            // throw new BusinessException("找不到物料{0}对应的来源库位{1},目的库位{2}的移库路线", orderBomDetail.Item);
                //            itemString += string.IsNullOrEmpty(itemString) ? orderBomDetail.Item : "," + orderBomDetail.Item;
                //            continue;
                //        }
                //        #endregion

                //        transferFlow = transferFlowList[0];
                //        isShipScanHu = procurementFlow.IsReceiveScanHu;
                //        IList<ItemTrace> itemTraceList = genericMgr.FindAll<ItemTrace>("from ItemTrace as i where i.Item = ?", orderBomDetail.Item);
                //        isReceiveScanHu = (itemTraceList == null || itemTraceList.Count() == 0) ? false : true;

                //    }

                #endregion
            }

            #endregion

            foreach (OrderMaster om in orderMasterList)
            {
                CreateOrder(om);
                orderString += string.IsNullOrEmpty(orderString) ? om.OrderNo : "," + om.OrderNo;
            }
            return new string[2] { orderString, itemString };

        }
        #endregion

        #region 导入生成紧急拉料单
        [Transaction(TransactionMode.Requires)]
        public string[] CreateEmTransferOrderFromXls(Stream inputStream)
        {
            string orderStr = string.Empty;
            string itemStr = string.Empty;
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
            int colLocTo = 4;// 目的库位
            int colQty = 5;//数量
            int colWindowTime = 6;//窗口时间
            #endregion

            DateTime dateTimeNow = DateTime.Now;
            IList<OrderDetail> exactOrderDetailList = new List<OrderDetail>();
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
                string locationCode = string.Empty;
                DateTime windowTime = DateTime.Now;

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

                #endregion

                #region 读取库位
                locationCode = row.GetCell(colLocTo) != null ? row.GetCell(colLocTo).StringCellValue : string.Empty;
                if (locationCode == null || locationCode.Trim() == string.Empty)
                {
                    throw new BusinessException("Import.Read.Error.Empty", (row.RowNum + 1).ToString(), colLocTo.ToString());
                }
                #endregion

                #region 读取窗口时间
                try
                {
                    windowTime = row.GetCell(colWindowTime).DateCellValue;
                }
                catch
                {
                    ImportHelper.ThrowCommonError(row.RowNum, colWindowTime, row.GetCell(colWindowTime));
                }
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

                #region 填充数据

                OrderDetail od = new OrderDetail();
                od.LocationTo = locationCode;
                od.Item = itemCode;
                od.Uom = uomCode;
                od.OrderedQty = qty;
                od.WindowTime = windowTime;
                exactOrderDetailList.Add(od);
                #endregion
            }

            #region 创建要货单
            //IList<WorkingCalendarView> workView=genericMgr.FindAll <WorkingCalendarView>("select v from WorkingCalendarView as v");
            IList<OrderMaster> orderMasterList = new List<OrderMaster>();
            var locationList = exactOrderDetailList.Select(b => b.LocationTo).Distinct().ToList();
            foreach (string location in locationList)
            {
                FlowMaster transferFlow = null;
                FlowDetail transferFlowDetail = null;
                FlowStrategy transferFlowStrategy = null;
                string hql = "from FlowDetail as d where exists (select 1 from FlowMaster as f where f.Code = d.Flow and f.Type = ? and f.IsActive = ? and f.FlowStrategy not in (?,?) and (d.LocationTo = ? or (d.LocationTo is null and f.LocationTo = ?)))";
            }
            foreach (OrderMaster om in orderMasterList)
            {
                CreateOrder(om);
                orderStr += string.IsNullOrEmpty(orderStr) ? om.OrderNo : "," + om.OrderNo;
            }
            #endregion

            return new string[] { orderStr, itemStr };
        }
        #endregion

        #region 自由移库
        //[Transaction(TransactionMode.Requires)]
        //public string CreateTransferOrderFromXls(Stream inputStream, string regionFromCode, string regionToCode, DateTime effectiveDate)
        //{
        //    #region 导入数据
        //    if (inputStream.Length == 0)
        //    {
        //        throw new BusinessException("Import.Stream.Empty");
        //    }

        //    HSSFWorkbook workbook = new HSSFWorkbook(inputStream);

        //    ISheet sheet = workbook.GetSheetAt(0);
        //    IEnumerator rows = sheet.GetRowEnumerator();

        //    ImportHelper.JumpRows(rows, 11);

        //    #region 列定义
        //    int colItem = 1;//物料代码
        //    int colUom = 3;//单位
        //    int colLocFrom = 4;// 来源库位
        //    int colLocTo = 5;// 目的库位
        //    int colQty = 6;//数量
        //    #endregion

        //    DateTime dateTimeNow = DateTime.Now;
        //    if (string.IsNullOrEmpty(regionToCode))
        //    {
        //        regionToCode = regionFromCode;
        //    }

        //    IList<OrderDetail> exactOrderDetailList = new List<OrderDetail>();
        //    while (rows.MoveNext())
        //    {
        //        HSSFRow row = (HSSFRow)rows.Current;
        //        if (!ImportHelper.CheckValidDataRow(row, 1, 9))
        //        {
        //            break;//边界
        //        }
        //        string itemCode = string.Empty;
        //        decimal qty = 0;
        //        string uomCode = string.Empty;
        //        string locationFromCode = string.Empty;
        //        string locationToCode = string.Empty;

        //        #region 读取数据
        //        #region 读取物料代码
        //        itemCode = ImportHelper.GetCellStringValue(row.GetCell(colItem));
        //        if (itemCode == null || itemCode.Trim() == string.Empty)
        //        {
        //            ImportHelper.ThrowCommonError(row.RowNum, colItem, row.GetCell(colItem));
        //        }

        //        #endregion
        //        #region 读取单位
        //        uomCode = row.GetCell(colUom) != null ? row.GetCell(colUom).StringCellValue : string.Empty;
        //        if (uomCode == null || uomCode.Trim() == string.Empty)
        //        {
        //            throw new BusinessException("Import.Read.Error.Empty", (row.RowNum + 1).ToString(), colUom.ToString());
        //        }
        //        #endregion

        //        #endregion

        //        #region 读取来源库位
        //        locationFromCode = row.GetCell(colLocFrom) != null ? row.GetCell(colLocFrom).StringCellValue : string.Empty;
        //        if (string.IsNullOrEmpty(locationFromCode))
        //        {
        //            throw new BusinessException("Import.Read.Error.Empty", (row.RowNum + 1).ToString(), colLocFrom.ToString());
        //        }

        //        IList<Location> locationFromList = genericMgr.FindAll<Location>("select l from Location as l where l.Code=?", locationFromCode);
        //        if (locationFromList != null && locationFromList.Count > 0)
        //        {

        //            if (locationFromList[0].Region != regionFromCode)
        //            {
        //                throw new BusinessException("指定区域不存在此库位" + locationFromCode, (row.RowNum + 1).ToString(), colLocFrom.ToString());
        //            }
        //        }
        //        else
        //        {
        //            throw new BusinessException("指定区域不存在此库位" + regionToCode, (row.RowNum + 1).ToString(), colLocFrom.ToString());
        //        }
        //        //  Location locationFrom = genericMgr.FindById<Location>(locationFromCode);


        //        #endregion

        //        #region 读取目的库位
        //        locationToCode = row.GetCell(colLocTo) != null ? row.GetCell(colLocTo).StringCellValue : string.Empty;
        //        if (string.IsNullOrEmpty(locationFromCode))
        //        {
        //            throw new BusinessException("Import.Read.Error.Empty", (row.RowNum + 1).ToString(), colLocTo.ToString());
        //        }

        //        IList<Location> locationToList = genericMgr.FindAll<Location>("select l from Location as l where l.Code=?", locationToCode);
        //        if (locationToList != null && locationToList.Count > 0)
        //        {
        //            if (locationToList[0].Region != regionToCode)
        //            {
        //                throw new BusinessException("指定区域不存在此库位" + regionToCode, (row.RowNum + 1).ToString(), colLocFrom.ToString());
        //            }
        //        }
        //        else
        //        {
        //            throw new BusinessException("指定区域不存在此库位" + regionToCode, (row.RowNum + 1).ToString(), colLocFrom.ToString());
        //        }

        //        //Location locationTo = genericMgr.FindById<Location>(locationToCode);
        //        //if (locationTo.Region != regionToCode)
        //        //{
        //        //    throw new BusinessException("指定区域不存在此库位" + locationTo, (row.RowNum + 1).ToString(), colLocFrom.ToString());
        //        //}
        //        #endregion

        //        #region 读取数量
        //        try
        //        {
        //            qty = Convert.ToDecimal(row.GetCell(colQty).NumericCellValue);
        //        }
        //        catch
        //        {
        //            ImportHelper.ThrowCommonError(row.RowNum, colQty, row.GetCell(colQty));
        //        }
        //        #endregion

        //        #region 填充数据
        //        OrderDetail od = new OrderDetail();
        //        od.LocationFrom = locationFromCode;
        //        od.LocationTo = locationToCode;
        //        od.Item = itemCode;
        //        od.Uom = uomCode;
        //        od.OrderedQty = qty;


        //        exactOrderDetailList.Add(od);
        //        #endregion
        //    }

        //    #endregion

        //    return CreateFreeTransferOrderMaster(regionFromCode, regionToCode, exactOrderDetailList, effectiveDate);
        //}

        [Transaction(TransactionMode.Requires)]
        public string CreateTransferOrderFromXls(string shift, Stream inputStream, string manuCode)
        {
            bool isReturn = false;
            bool isQuick = true;
            if (manuCode == "Url_TransferOrder_View")
            {
                isReturn = false;
                isQuick = true;
            }
            else if (manuCode == "Url_OrderMstr_Procurement_Import")
            {
                isReturn = false;
                isQuick = false;
            }
            else if (manuCode == "Url_OrderMstr_Procurement_ReturnNew")
            {
                isReturn = true;
                isQuick = false;
            }
            else if (manuCode == "Url_OrderMstr_Procurement_ReturnQuickNew")
            {
                isReturn = true;
                isQuick = true;
            }
            return CreateTransferOrderFromXls(shift, inputStream, isQuick, isReturn);
        }

        [Transaction(TransactionMode.Requires)]
        //public string CreateTransferOrderFromXls(Stream inputStream, string regionFromCode, string regionToCode, DateTime effectiveDate, string manufactureParty, string Consignment, bool isQuick, bool isReturn)
        public string CreateTransferOrderFromXls(string shift, Stream inputStream, bool isQuick, bool isReturn)
        {

            #region 导入数据
            if (inputStream.Length == 0)
            {
                throw new BusinessException("Import.Stream.Empty");
            }

            HSSFWorkbook workbook = new HSSFWorkbook(inputStream);

            ISheet sheet = workbook.GetSheetAt(0);
            IEnumerator rows = sheet.GetRowEnumerator();

            BusinessException businessException = new BusinessException();

            ImportHelper.JumpRows(rows, 10);

            #region 列定义
            int colItem = 1;//物料代码
            int colManufuctureParty = 3;//寄售供应商
            int colLocFrom = 4;// 来源库位
            int colLocTo = 5;// 目的库位
            int colQty = 6;//数量
            int colWindowTime = 7;//窗口时间
            int colZOPWZ = 8;//备注
            #endregion

            DateTime dateTimeNow = DateTime.Now;

            int rowCount = 10;

            IList<OrderDetail> exactOrderDetailList = new List<OrderDetail>();
            DateTime windowTime = System.DateTime.Now;
            while (rows.MoveNext())
            {
                rowCount++;
                HSSFRow row = (HSSFRow)rows.Current;
                if (!ImportHelper.CheckValidDataRow(row, 1, 6))
                {
                    break;//边界
                }
                OrderDetail od = new OrderDetail();
                string itemCode = string.Empty;
                decimal qty = 0;
                string manufactureParty = string.Empty;
                string locationFromCode = string.Empty;
                string locationToCode = string.Empty;
                string masterPartyFrom = string.Empty;
                string masterPartyTo = string.Empty;
                string ZOPWZ = string.Empty;

                #region 读取数据
                #region 读取物料代码
                itemCode = ImportHelper.GetCellStringValue(row.GetCell(colItem));
                if (itemCode == null || itemCode.Trim() == string.Empty)
                {
                    businessException.AddMessage(string.Format("第{0}行：物料编号不能为空。", rowCount));
                }
                else
                {
                    var items = this.genericMgr.FindAll<Item>(" select i from Item as i where i.Code=? ", itemCode);
                    if (items == null || items.Count == 0)
                    {
                        businessException.AddMessage(string.Format("第{0}行：物料编号{1}不存在。", rowCount, itemCode));
                    }
                    else
                    {
                        od.Item = items.First().Code;
                        od.ItemDescription = items.First().Description;
                        od.ReferenceItemCode = items.First().ReferenceCode;
                        od.Uom = items.First().Uom;
                        od.BaseUom = items.First().Uom;
                    }
                }
                #endregion

                #region 读取来源库位
                locationFromCode = ImportHelper.GetCellStringValue(row.GetCell(colLocFrom));
                if (string.IsNullOrWhiteSpace(locationFromCode))
                {
                    businessException.AddMessage(string.Format("第{0}行：来源库位不能为空。", rowCount));
                }
                else
                {
                    IList<Location> locationFromList = genericMgr.FindAll<Location>("select l from Location as l where l.Code=?", locationFromCode);
                    if (locationFromList != null && locationFromList.Count > 0)
                    {
                        od.LocationFrom = locationFromCode;
                        od.MastPartyFrom = locationFromList.First().Region;
                    }
                    else
                    {
                        businessException.AddMessage(string.Format("第{0}行：来源库位{1}不存在", rowCount, locationFromCode));
                    }
                }

                //  Location locationFrom = genericMgr.FindById<Location>(locationFromCode);


                #endregion

                #region 读取目的库位
                locationToCode = ImportHelper.GetCellStringValue(row.GetCell(colLocTo));
                if (string.IsNullOrWhiteSpace(locationToCode))
                {
                    businessException.AddMessage(string.Format("第{0}行：目的库位不能为空。", rowCount));
                }
                else
                {
                    IList<Location> locationToList = genericMgr.FindAll<Location>("select l from Location as l where l.Code=?", locationToCode);
                    if (locationToList != null && locationToList.Count > 0)
                    {
                        od.LocationTo = locationToCode;
                        od.MastPartyTo = locationToList.First().Region;
                    }
                    else
                    {
                        businessException.AddMessage(string.Format("第{0}行：目的库位{1}不存在", rowCount, locationToCode));
                    }
                }
                #endregion

                #region 读取数量
                try
                {
                    qty = Convert.ToDecimal(row.GetCell(colQty).NumericCellValue);
                    od.OrderedQty = qty;

                    if (!isQuick && !isReturn)
                    {
                        string hql = " select fd from FlowDetail as fd where Item=? and exists( select 1 from FlowMaster as fm where fm.Code=fd.Flow and fm.PartyFrom=? and fm.PartyTo=? and fm.Type=2 ) ";
                        var flowDetails = this.genericMgr.FindAll<FlowDetail>(hql, new object[] { od.Item, od.MastPartyFrom, od.MastPartyTo }, new IType[] { NHibernateUtil.String, NHibernateUtil.String, NHibernateUtil.String });
                        if (flowDetails != null && flowDetails.Count > 0)
                        {
                            if (flowDetails.FirstOrDefault().RoundUpOption != com.Sconit.CodeMaster.RoundUpOption.None)
                            {
                                if (od.OrderedQty % flowDetails.FirstOrDefault().MinUnitCount > 0)
                                {
                                    businessException.AddMessage(string.Format("第{0}行明细物料{1}的要货数{2}不符合圆整包装，包装系数为{3}不能为空！", rowCount, od.Item, od.OrderedQty, flowDetails.FirstOrDefault().MinUnitCount));
                                }
                            }
                        }
                    }
                }
                catch
                {
                    businessException.AddMessage(string.Format("第{0}行：数量{1}填写有误", rowCount, qty));
                }
                #endregion

                #region 窗口时间
                if (!isQuick && !isQuick)
                {
                    string readWindowTime = ImportHelper.GetCellStringValue(row.GetCell(colWindowTime));
                    if (string.IsNullOrWhiteSpace(readWindowTime))
                    {
                        businessException.AddMessage(string.Format("第{0}行：窗口时间不能为空。", rowCount));
                    }
                    else
                    {
                        if (!DateTime.TryParse(readWindowTime, out windowTime))
                        {
                            businessException.AddMessage(string.Format("第{0}行：窗口时间{1}填写有误", rowCount, readWindowTime));
                            continue;
                        }
                        if (string.IsNullOrWhiteSpace(od.MastPartyTo))
                        {
                            continue;
                        }
                        var workingCalendars = this.genericMgr.FindAll<WorkingCalendar>(" select w from WorkingCalendar as w where w.Region=? and w.WorkingDate=? ", new object[] { od.MastPartyTo, windowTime.Date });
                        if (workingCalendars != null && workingCalendars.Count > 0)
                        {
                            if (workingCalendars.First().Type == com.Sconit.CodeMaster.WorkingCalendarType.Rest)
                            {
                                businessException.AddMessage(string.Format("第{0}行：窗口时间{1}是休息时间，请确认。", rowCount, readWindowTime));
                                continue;
                            }
                            string times = (windowTime.Hour).ToString().PadLeft(2, '0') + ":" + (windowTime.Minute).ToString().PadLeft(2, '0');

                            var shiftDet = this.genericMgr.FindAll<ShiftDetail>(" select s from ShiftDetail as s where s.Shift=? ", new object[] { workingCalendars.FirstOrDefault().Shift });
                            if (shiftDet != null || shiftDet.Count > 0)
                            {
                                DateTime nowTime = this.ParseDateTime(times);
                                bool isTure = true;
                                foreach (var det in shiftDet)
                                {
                                    DateTime prevTime = this.ParseDateTime(det.StartTime);
                                    DateTime nextTime = this.ParseDateTime(det.EndTime);
                                    if (nextTime < prevTime) nextTime = nextTime.AddDays(1);
                                    if (nowTime >= prevTime && nowTime <= nextTime)
                                    {
                                        isTure = true;
                                        break;
                                    }
                                }
                                if (!isTure)
                                {
                                    throw new BusinessException(string.Format("窗口时间{0}是休息时间，请确认。", windowTime));
                                }
                            }
                            else
                            {
                                throw new BusinessException(string.Format("没有找到区域的工作日历。"));
                            }
                        }
                    }
                }
                if (windowTime < dateTimeNow)
                {
                    businessException.AddMessage(string.Format("第{0}行：窗口时间{1}不能小于当前时间，请确认。", rowCount, windowTime));
                    continue;
                }
                od.MastWindowTime = windowTime;
                #endregion

                #region 备注
                ZOPWZ = ImportHelper.GetCellStringValue(row.GetCell(colZOPWZ));
                od.ZOPWZ = ZOPWZ;
                #endregion

                #region 填充数据
                exactOrderDetailList.Add(od);
                #endregion

                od.QualityType = com.Sconit.CodeMaster.QualityType.Qualified;

                #region 读取寄售供应商
                manufactureParty = ImportHelper.GetCellStringValue(row.GetCell(colManufuctureParty));
                if (!string.IsNullOrWhiteSpace(manufactureParty))
                {
                    IList<Supplier> suppliers = genericMgr.FindAll<Supplier>("select l from Supplier as l where l.Code=?", manufactureParty);
                    if (suppliers == null || suppliers.Count == 0)
                    {
                        businessException.AddMessage(string.Format("第{0}行：读取寄售供应商{1}填写有误", rowCount, qty));
                        continue;
                    }

                }
                if (!isReturn && isQuick) //快速移库 寄售供应商 存orderdetailinput 
                {
                    if (od.MastPartyFrom == "SQC" || od.MastPartyFrom == "LOC" || od.MastPartyTo == "LOC" || od.MastPartyTo == "SQC")
                    {
                        businessException.AddMessage(string.Format("第{0}行：厂内快速移库不能对LOC,SQC 移库", rowCount));
                        continue;
                    }
                    OrderDetailInput orderDetailInput = new OrderDetailInput();
                    orderDetailInput.ReceiveQty = od.OrderedQty;
                    orderDetailInput.ConsignmentParty = !string.IsNullOrWhiteSpace(manufactureParty) ? manufactureParty : null;
                    orderDetailInput.QualityType = od.QualityType;
                    od.AddOrderDetailInput(orderDetailInput);
                }
                else if (!isReturn && !isQuick)//要货单导入 供应商 存orderdetail
                {
                    od.ManufactureParty = !string.IsNullOrWhiteSpace(manufactureParty) ? manufactureParty : null;
                    OrderDetailInput orderDetailInput = new OrderDetailInput();
                    orderDetailInput.ReceiveQty = od.OrderedQty;
                    orderDetailInput.QualityType = od.QualityType;
                    od.AddOrderDetailInput(orderDetailInput);
                }
                else if (isReturn && !isQuick)//退货
                {
                    OrderDetailInput orderDetailInput = new OrderDetailInput();
                    orderDetailInput.ReceiveQty = od.OrderedQty;
                    orderDetailInput.QualityType = od.QualityType;
                    od.AddOrderDetailInput(orderDetailInput);
                }
                else if (isReturn && isQuick) //快速退货 
                {
                    OrderDetailInput orderDetailInput = new OrderDetailInput();
                    orderDetailInput.ReceiveQty = od.OrderedQty;
                    orderDetailInput.QualityType = od.QualityType;
                    od.AddOrderDetailInput(orderDetailInput);
                }
                #endregion
                #endregion

            }

            #endregion


            if (businessException.HasMessage)
            {
                throw businessException;
            }

            if (exactOrderDetailList == null || exactOrderDetailList.Count == 0)
            {
                throw new BusinessException("导入的有效数据为0,。");
            }
            else
            {
                if (!isReturn && !isQuick)
                {
                    var groups = (from tak in exactOrderDetailList
                                  group tak by new
                                  {
                                      tak.MastWindowTime,
                                      tak.MastPartyFrom,
                                      tak.MastPartyTo,
                                      tak.LocationFrom,
                                  }
                                      into result
                                      select new
                                      {
                                          PartyFrom = result.Key.MastPartyFrom,
                                          PartyTo = result.Key.MastPartyTo,
                                          WindowTime = result.Key.MastWindowTime,
                                          LocationFrom = result.Key.LocationFrom,
                                          list = result.ToList()
                                      }
                        ).ToList();
                    string orderNos = string.Empty;
                    foreach (var order in groups)
                    {
                        orderNos += CreateFreeTransferOrderMaster(shift, order.PartyFrom, order.PartyTo, order.list, System.DateTime.Now, order.WindowTime, isQuick, isReturn) + "*";
                    }
                    return orderNos.Substring(0, orderNos.Length - 1);
                }
                else
                {
                    var groups = (from tak in exactOrderDetailList
                                  group tak by new
                                  {
                                      tak.MastWindowTime,
                                      tak.MastPartyFrom,
                                      tak.MastPartyTo,
                                  }
                                      into result
                                      select new
                                      {
                                          PartyFrom = result.Key.MastPartyFrom,
                                          PartyTo = result.Key.MastPartyTo,
                                          WindowTime = result.Key.MastWindowTime,
                                          list = result.ToList()
                                      }
                           ).ToList();
                    string orderNos = string.Empty;
                    foreach (var order in groups)
                    {
                        orderNos += CreateFreeTransferOrderMaster(shift, order.PartyFrom, order.PartyTo, order.list, System.DateTime.Now, order.WindowTime, isQuick, isReturn) + "*";
                    }
                    return orderNos.Substring(0, orderNos.Length - 1);
                }

            }


            //return CreateFreeTransferOrderMaster(regionFromCode, regionToCode, exactOrderDetailList, effectiveDate, windowTime, isQuick, isReturn);
        }

        //[Transaction(TransactionMode.Requires)]
        //public string CreateFreeTransferOrderMaster(string regionFromCode, string regionToCode, IList<OrderDetail> orderDetailList, DateTime effectiveDate, bool isQuick, bool isReturn)
        //{
        //    return CreateFreeTransferOrderMaster(regionFromCode, regionToCode, orderDetailList, effectiveDate, isQuick, isReturn);
        //}

        [Transaction(TransactionMode.Requires)]
        public string CreateFreeTransferOrderMaster(string shift, string regionFromCode, string regionToCode, IList<OrderDetail> orderDetailList, DateTime effectiveDate, DateTime windowTime, bool isQuick, bool isReturn)
        {
            if (orderDetailList == null || orderDetailList.Count == 0)
            {
                throw new BusinessException("移库明细不能为空");
            }
            if (string.IsNullOrEmpty(regionFromCode))
            {
                throw new BusinessException("来源区域不能为空。");
            }
            if (string.IsNullOrEmpty(regionToCode))
            {
                throw new BusinessException("目的区域不能为空。");
            }
            if (string.IsNullOrEmpty(orderDetailList[0].LocationFrom))
            {
                throw new BusinessException("第1行来源库位不能为空。");
            }
            if (string.IsNullOrEmpty(orderDetailList[0].LocationTo))
            {
                throw new BusinessException("第1行目的库位不能为空。");
            }
            var orderMaster = new OrderMaster();

            var regionFrom = genericMgr.FindById<Region>(regionFromCode);
            var regionTo = genericMgr.FindById<Region>(regionToCode);

            Location locFrom = genericMgr.FindById<Location>(orderDetailList[0].LocationFrom);
            Location locTo = genericMgr.FindById<Location>(orderDetailList[0].LocationTo);
            orderMaster.SubType = isReturn ? CodeMaster.OrderSubType.Return : CodeMaster.OrderSubType.Normal;
            orderMaster.LocationFrom = locFrom.Code;
            orderMaster.IsShipScanHu = false;
            orderMaster.IsReceiveScanHu = false;
            orderMaster.LocationFromName = locFrom.Name;
            orderMaster.LocationTo = locTo.Code;
            orderMaster.LocationToName = locTo.Name;
            orderMaster.PartyFrom = regionFrom.Code;
            orderMaster.PartyFromName = regionFrom.Name;
            orderMaster.PartyTo = regionTo.Code;
            orderMaster.PartyToName = regionTo.Name;
            orderMaster.Type = CodeMaster.OrderType.Transfer;
            orderMaster.StartTime = DateTime.Now;
            orderMaster.WindowTime = windowTime;
            orderMaster.EffectiveDate = effectiveDate;
            orderMaster.OrderStrategy = com.Sconit.CodeMaster.FlowStrategy.Manual;
            orderMaster.Shift = shift;
            if (orderDetailList.First().QualityType == com.Sconit.CodeMaster.QualityType.Reject)
            {
                orderMaster.QualityType = com.Sconit.CodeMaster.QualityType.Reject;
            }
            orderMaster.IsQuick = isQuick;
            //默认为自动
            orderMaster.IsAutoRelease = true;
            int i = 0;
            foreach (OrderDetail od in orderDetailList)
            {
                i++;
                if (string.IsNullOrEmpty(od.LocationFrom))
                {
                    throw new BusinessException("第" + i + "行来源库位不能为空。");
                }
                if (string.IsNullOrEmpty(od.LocationTo))
                {
                    throw new BusinessException("第" + i + "行目的库位不能为空。");
                }
                Item item = genericMgr.FindById<Item>(od.Item);
                Location dLocFrom = genericMgr.FindById<Location>(od.LocationFrom);
                Location dLocTo = genericMgr.FindById<Location>(od.LocationTo);
                //Location dLocFrom = genericMgr.FindById<Location>(od.LocationFrom);
                od.Uom = item.Uom;
                od.ItemDescription = item.Description;
                od.ReferenceItemCode = item.ReferenceCode;
                od.LocationFromName = dLocFrom.Name;
                od.LocationToName = dLocTo.Name;
                od.UnitCount = item.UnitCount;
            }

            orderMaster.OrderDetails = orderDetailList;
            this.CreateOrder(orderMaster);
            return orderMaster.OrderNo;
        }

        #endregion

        #region 导入要货单
        [Transaction(TransactionMode.Requires)]
        public string CreateProcurementOrderFromXls(Stream inputStream,
            string flowCode, string extOrderNo, string refOrderNo,
            DateTime startTime, DateTime windowTime, CodeMaster.OrderPriority priority)
        {
            #region 导入数据
            if (inputStream.Length == 0)
            {
                throw new BusinessException("Import.Stream.Empty");
            }

            HSSFWorkbook workbook = new HSSFWorkbook(inputStream);

            ISheet sheet = workbook.GetSheetAt(0);
            IEnumerator rows = sheet.GetRowEnumerator();

            ImportHelper.JumpRows(rows, 1);

            #region 列定义
            int colSeqNo = 0;//序号
            int colItem = 1;//物料代码
            //int colItemDescription = 2;// 物料描述
            int colQty = 3;//数量
            int colUom = 4;//单位
            int colUnitCount = 5;//单包装
            int colLocationTo = 6;//来源库位
            #endregion

            IList<OrderDetail> exactOrderDetailList = new List<OrderDetail>();

            var flowMaster = this.genericMgr.FindById<FlowMaster>(flowCode);

            var flowDetailList = this.flowMgr.GetFlowDetailList(flowCode, false, true);

            while (rows.MoveNext())
            {
                HSSFRow row = (HSSFRow)rows.Current;
                if (!ImportHelper.CheckValidDataRow(row, 1, 9))
                {
                    break;//边界
                }
                int seqNo = 0;
                string itemCode = string.Empty;
                decimal qty = 0;
                string uom = string.Empty;
                decimal unitCount = 0;
                Location locationTo = null;

                #region 读取数据

                #region 读取序号
                try
                {
                    seqNo = Convert.ToInt32(row.GetCell(colSeqNo).NumericCellValue);
                }
                catch
                {
                    seqNo = 0;
                }
                #endregion

                #region 读取物料代码
                itemCode = ImportHelper.GetCellStringValue(row.GetCell(colItem));
                if (string.IsNullOrWhiteSpace(itemCode))
                {
                    ImportHelper.ThrowCommonError(row.RowNum, colItem, row.GetCell(colItem));
                }
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

                #region 读取单位
                uom = ImportHelper.GetCellStringValue(row.GetCell(colUom));
                #endregion

                #region 读取单包装
                try
                {
                    unitCount = Convert.ToDecimal(row.GetCell(colUnitCount).NumericCellValue);
                }
                catch
                {
                    unitCount = 0;
                }
                #endregion

                #region 读取目的库位
                string locationToCode = ImportHelper.GetCellStringValue(row.GetCell(colLocationTo));
                if (!string.IsNullOrWhiteSpace(locationToCode))
                {
                    locationTo = this.genericMgr.FindById<Location>(locationToCode);
                    if (locationTo.Region != flowMaster.PartyTo)
                    {
                        throw new BusinessException("库位{0}不在区域{1}下", locationTo.Code, flowMaster.PartyTo);
                    }
                }
                #endregion

                #endregion

                #region 填充数据

                #region 不允许同一物料 同一库位出现多行
                if (flowMaster.FlowDetails != null)
                {
                    var checkFlowDetail = flowMaster.FlowDetails.Where(f => f.Item == itemCode && f.LocationTo == locationToCode).FirstOrDefault();
                    if (checkFlowDetail != null)
                    {
                        throw new BusinessException(string.Format("相同的物料{0}库位{1}出现多行。", itemCode, locationToCode));
                    }
                }
                #endregion

                var flowDetail = flowDetailList.Where(f => f.Item == itemCode && f.LocationTo == locationToCode).FirstOrDefault();
                if (flowDetail == null)
                {
                    if (flowMaster.IsManualCreateDetail)
                    {
                        flowDetail = new FlowDetail();

                        var item = this.genericMgr.FindById<Entity.MD.Item>(itemCode);
                        flowDetail.OrderQty = qty;
                        flowDetail.BaseUom = item.Uom;
                        flowDetail.Item = itemCode;
                        if (seqNo > 0)
                        {
                            flowDetail.ExternalSequence = seqNo;
                        }
                        if (!string.IsNullOrWhiteSpace(uom))
                        {
                            flowDetail.Uom = uom;
                        }
                        else
                        {
                            flowDetail.Uom = item.Uom;
                        }
                        if (unitCount > 0)
                        {
                            flowDetail.UnitCount = unitCount;
                        }
                        else
                        {
                            flowDetail.UnitCount = item.UnitCount;
                        }
                        if (locationToCode == null)
                        {
                            flowDetail.LocationTo = flowMaster.LocationTo;
                        }
                        else
                        {
                            flowDetail.LocationTo = locationTo.Code;
                        }
                    }
                    else
                    {
                        throw new BusinessException("没有找到匹配的物流路线明细", itemCode, uom, unitCount.ToString());
                    }
                }
                else
                {
                    if (seqNo > 0)
                    {
                        flowDetail.ExternalSequence = seqNo;
                    }
                    flowDetail.OrderQty = qty;
                }
                flowMaster.AddFlowDetail(flowDetail);

                #endregion
            }

            #endregion

            #region 创建要货单
            OrderMaster orderMaster = TransferFlow2Order(flowMaster, true);

            orderMaster.ReferenceOrderNo = refOrderNo;
            orderMaster.ExternalOrderNo = extOrderNo;
            orderMaster.StartTime = startTime;
            orderMaster.WindowTime = windowTime;
            orderMaster.Priority = priority;
            this.CreateOrder(orderMaster);

            #endregion

            return orderMaster.OrderNo;
        }
        #endregion

        #region 页面条码移库
        [Transaction(TransactionMode.Requires)]
        public string CreateHuTransferOrder(string flowCode, IList<string> huIdList, DateTime effectiveDate)
        {
            FlowMaster flow = genericMgr.FindById<FlowMaster>(flowCode);
            OrderMaster order = TransferFlow2Order(flow, false);
            IList<OrderDetail> orderDetailList = new List<OrderDetail>();

            IList<HuStatus> huStatusList = huMgr.GetHuStatus(huIdList);
            IList<HuStatus> notInLocHu = huStatusList.Where(h => h.Status != CodeMaster.HuStatus.Location).ToList();
            //新条码逻辑
            if (flow.IsShipScanHu)
            {
                if (notInLocHu != null && notInLocHu.Count > 0)
                {
                    string strExeception = string.Empty;
                    foreach (HuStatus hs in notInLocHu)
                    {
                        if (string.IsNullOrEmpty(strExeception))
                            strExeception = "条码" + hs.HuId;
                        else
                            strExeception += "," + hs.HuId;

                    }
                    strExeception += "不在库存中";
                    throw new BusinessException(strExeception);
                }
            }
            else if (!flow.IsShipScanHu && flow.IsReceiveScanHu)
            {
                //快速移库暂不支持，逻辑还没想清楚
                throw new BusinessException("快速移库不支持数量发货条码收货的配置");
            }

            foreach (HuStatus huStatus in huStatusList)
            {
                var h = orderDetailList.Where(o => o.Item == huStatus.Item && o.Uom == huStatus.Uom && o.LocationFrom == huStatus.Location).ToList();
                if (h == null || h.Count == 0)
                {
                    OrderDetail od = new OrderDetail();
                    od.Item = huStatus.Item;
                    od.Uom = huStatus.Uom;
                    od.UnitCount = huStatus.UnitCount;
                    od.ItemDescription = huStatus.ItemDescription;
                    od.ReferenceItemCode = huStatus.ReferenceItemCode;
                    od.LocationFrom = huStatus.Location;
                    od.OrderedQty = huStatus.Qty;

                    IList<OrderDetailInput> orderDetailInputList = new List<OrderDetailInput>();
                    OrderDetailInput orderDetailInput = new OrderDetailInput();
                    orderDetailInput.HuId = huStatus.HuId;
                    orderDetailInput.HuQty = huStatus.Qty;
                    orderDetailInput.LotNo = huStatus.LotNo;
                    orderDetailInput.ReceiveQty = huStatus.Qty;
                    orderDetailInputList.Add(orderDetailInput);

                    od.OrderDetailInputs = orderDetailInputList;
                    orderDetailList.Add(od);
                }
                else
                {
                    OrderDetail od = h[0];
                    od.OrderedQty += huStatus.Qty;

                    OrderDetailInput orderDetailInput = new OrderDetailInput();
                    orderDetailInput.HuId = huStatus.HuId;
                    orderDetailInput.HuQty = huStatus.Qty;
                    orderDetailInput.LotNo = huStatus.LotNo;
                    orderDetailInput.ReceiveQty = huStatus.Qty;
                    od.OrderDetailInputs.Add(orderDetailInput);
                }

            }
            order.OrderDetails = orderDetailList;
            order.EffectiveDate = effectiveDate;
            order.WindowTime = DateTime.Now;
            order.StartDate = DateTime.Now;
            order.IsQuick = true;
            CreateOrder(order);
            return order.OrderNo;

        }
        #endregion

        #region 生成DAT文件数据
        public void ReCreateDat(string ipNo)
        {
            IpMaster ipMaster = this.genericMgr.FindById<IpMaster>(ipNo);

            #region 检查状态
            if (ipMaster.Status != com.Sconit.CodeMaster.IpStatus.Submit)
            {
                throw new BusinessException(string.Format("不能对{0}状态的ASN生成DAT.", ipMaster.Status));
            }
            #endregion

            #region 检查明细
            ipMaster.IpDetails = this.genericMgr.FindAll<IpDetail>("select d from  IpDetail as d where d.IpNo=?", ipNo);
            if (ipMaster.IpDetails == null || ipMaster.IpDetails.Count() == 0)
            {
                throw new BusinessException("送货单明细为空不能生成Dat文件，请确认。");
            }
            #endregion

            #region 检查是不是都是计划协议的ASN
            IList<string> orderNos = ipMaster.IpDetails.Select(o => o.OrderNo).Distinct().ToList();
            string selectOrderHql = "select o from OrderMaster as o where o.OrderNo in (";
            foreach (string orderNo in orderNos)
            {
                selectOrderHql += "?,";
            }
            selectOrderHql = selectOrderHql.Substring(0, selectOrderHql.Length - 1) + ")";

            IList<OrderMaster> orderMasterList = this.genericMgr.FindAll<OrderMaster>(selectOrderHql + ")", orderNos.ToArray());

            IList<com.Sconit.CodeMaster.OrderType> orderTypeList = (from orderMaster in orderMasterList
                                                                    group orderMaster by orderMaster.Type into result
                                                                    select result.Key).ToList();
            if (orderTypeList.Count > 1 || orderTypeList.First() != com.Sconit.CodeMaster.OrderType.ScheduleLine)
            {
                throw new BusinessException("不是计划协议生成的送货单不能生成Dat文件，请确认。");
            }
            #endregion

            #region 检查是否已经创建过
            bool isCreateIp = this.genericMgr.FindAllWithNativeSql<int>("select count(*) from FIS_CreateIpDAT where ASN_NO = ?", ipNo).SingleOrDefault() > 0;

            if (isCreateIp)
            {
                //string FileName = "ASNLE" + DateTime.Now.ToString("yyMMddHHmmss");
                try
                {
                    this.genericMgr.UpdateWithNativeQuery("update FIS_CreateIpDAT set IsCreateDat = ? where ASN_NO = ?", new object[] { false, ipNo });
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
                return;
            }
            else
            {
                this.CreateIpDat(ipMaster);
            }
            #endregion
        }

        public void ReCreateOrderDAT(string orderNo)
        {
            OrderMaster orderMaster = this.genericMgr.FindById<OrderMaster>(orderNo);
            if (orderMaster.Type == com.Sconit.CodeMaster.OrderType.Distribution)
            {
                throw new BusinessException(string.Format("不能对销售的订单生成DAT.", orderMaster.Status));
            }

            #region 检查状态
            if (orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Submit)
            {
                throw new BusinessException(string.Format("不能对{0}状态的订单生成DAT.", orderMaster.Status));
            }
            #endregion

            #region 检查明细
            orderMaster.OrderDetails = this.genericMgr.FindAll<OrderDetail>("select d from  OrderDetail as d where d.OrderNo=?", orderNo);
            if (orderMaster.OrderDetails == null || orderMaster.OrderDetails.Count() == 0)
            {
                throw new BusinessException("订单明细为空不能生成Dat文件，请确认。");
            }
            #endregion

            #region 来源区域是LOC,SQC 的 释放后才能生产DAT
            if (!((orderMaster.PartyFrom == systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.WMSAnjiRegion) || orderMaster.PartyFrom == "SQC")
                && orderMaster.SubType == com.Sconit.CodeMaster.OrderSubType.Normal))
            {
                throw new BusinessException("来源区域是LOC,SQC才能生成Dat文件，请确认。");
            }
            #endregion

            #region 检查是否已经创建过
            //bool isCreateOrder = this.genericMgr.FindAllWithNativeSql<int>("select count(*) as counter from FIS_CreateProcurementOrderDAT where OrderNo = ?", orderNo).SingleOrDefault() > 0;
            bool isProcurementCreateOrder = this.genericMgr.FindAllWithNativeSql<int>("select count(*) as counter from FIS_CreateProcurementOrderDAT where OrderNo = ?", orderNo).SingleOrDefault() > 0;
            bool isCreateSeqOrder = this.genericMgr.FindAllWithNativeSql<int>("select count(*) as counter from FIS_CreateSeqOrderDAT where OrderNo = ?", orderNo).SingleOrDefault() > 0;
            if (isProcurementCreateOrder || isCreateSeqOrder)
            {
                //string FileName = "SEQ1LE" + DateTime.Now.ToString("yyMMddHHmmss");
                try
                {
                    //if (isCreateOrder)
                    //{
                    //    this.genericMgr.UpdateWithNativeQuery("update FIS_CreateOrderDAT set IsCreateDat = ? where OrderNo = ?", new object[] { false, orderNo });
                    //}
                    //else
                    if (isProcurementCreateOrder)
                    {
                        this.genericMgr.UpdateWithNativeQuery("update FIS_CreateProcurementOrderDAT set IsCreateDat = ? where OrderNo = ?", new object[] { false, orderNo });
                        //this.genericMgr.UpdateWithNativeQuery("update FIS_CreateProcurementOrderDAT set IsCreateDat = ? where OrderNo = ?", new object[] { false, orderNo });
                    }
                    else if (isCreateSeqOrder)
                    {
                        this.genericMgr.UpdateWithNativeQuery("update FIS_CreateSeqOrderDAT set IsCreateDat = ? where SeqNo = ?", new object[] { false, orderNo });
                    }
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
            else
            {
                //this.CreateProcurementOrderDAT(orderMaster);
                if (orderMaster.OrderStrategy != com.Sconit.CodeMaster.FlowStrategy.SEQ)
                {
                    //if (orderMaster.PartyFrom == "SQC")
                    //    this.CreateOrderDAT(orderMaster);
                    //else
                        this.CreateProcurementOrderDAT(orderMaster);
                    //this.CreateOrderDAT(orderMaster);
                    //this.CreateProcurementOrderDAT(orderMaster);
                }
                else
                {
                    this.CreateSeqOrderDAT(orderMaster);
                }
            }
            #endregion
        }

        /// <summary>
        /// 送货单
        /// </summary>
        /// <param name="ipMaster"></param>
        [Transaction(TransactionMode.Requires)]
        private void CreateIpDat(IpMaster ipMaster)
        {

            if (ipMaster.IpDetails == null || ipMaster.IpDetails.Count == 0)
            {
                throw new BusinessException("明细为空不能创建。");
            }
            //string FileName = "ASNLE" + DateTime.Now.ToString("yyMMddHHmmss");
            foreach (IpDetail ipDetail in ipMaster.IpDetails.OrderBy(i => i.Sequence).ToList())
            {
                //if (string.IsNullOrEmpty(receiptNo) || (!string.IsNullOrEmpty(receiptNo) && orderDetail.RejectedQty > 0))
                //{
                CreateIpDAT createIpDAT = new CreateIpDAT();

                createIpDAT.ASN_NO = ipDetail.IpNo;
                createIpDAT.ASN_ITEM = ipDetail.Sequence.ToString();
                createIpDAT.WH_CODE = this.genericMgr.FindById<Location>(ipDetail.LocationTo).SAPLocation;
                createIpDAT.WH_LOCATION = "";
                createIpDAT.WH_DOCK = ipMaster.Dock;
                createIpDAT.ITEM_CODE = ipDetail.Item;
                createIpDAT.SUPPLIER_CODE = ipMaster.PartyFrom;
                createIpDAT.UOM = ipDetail.Uom;
                createIpDAT.QTY = ipDetail.Qty.ToString("0.000");
                createIpDAT.BASE_UNIT_UOM = ipDetail.BaseUom;
                createIpDAT.BASE_UNIT_QTY = (ipDetail.Qty * ipDetail.UnitQty).ToString("0.000");//
                createIpDAT.QC_FLAG = ipDetail.IsInspect == true ? "N" : "Y";

                createIpDAT.DELIVERY_DATE = System.DateTime.Now;
                createIpDAT.TIME_WINDOW = "";
                createIpDAT.PO = ipDetail.ExternalOrderNo;
                createIpDAT.FINANCE_FLAG = ipDetail.BillTerm == com.Sconit.CodeMaster.OrderBillTerm.OnlineBilling ? "Y" : "N";
                createIpDAT.COMPONENT_FLAG = "N";

                createIpDAT.TRACKID = "";
                createIpDAT.PO_LINE = ipDetail.ExternalSequence;
                createIpDAT.FactoryInfo = "0084";
                createIpDAT.F80XBJ = "";
                createIpDAT.F80X_LOCATION = "";
                createIpDAT.IsCreateDat = false;
                createIpDAT.CreateUserName = ipDetail.CreateUserName;
                createIpDAT.ErrorCount = 0;
                //createIpDAT.FileName = FileName;
                this.genericMgr.Create(createIpDAT);
            }
        }

        /// <summary>
        /// 要货单
        /// </summary>
        /// <param name="orderMaster"></param>
        [Transaction(TransactionMode.Requires)]
        private void CreateOrderDAT(OrderMaster orderMaster)
        {
            if (orderMaster.OrderDetails == null || orderMaster.OrderDetails.Count == 0)
            {
                throw new BusinessException("明细为空不能创建。");
            }
            //string FileName = "SEQ1LE" + DateTime.Now.ToString("yyMMddHHmmss");
            foreach (OrderDetail det in orderMaster.OrderDetails.OrderBy(od => od.Sequence).ToList())
            {
                CreateOrderDAT createOrderDAT = new CreateOrderDAT();
                createOrderDAT.OrderNo = det.OrderNo;
                createOrderDAT.MATNR = det.Item;
                createOrderDAT.LIFNR = det.ManufactureParty;
                createOrderDAT.ENMNG = det.OrderedQty.ToString("0.000");
                createOrderDAT.CHARG = orderMaster.TraceCode;
                createOrderDAT.COLOR = string.Empty;
                createOrderDAT.TIME_STAMP = orderMaster.CreateDate.ToString("yyyyMMddHHmmss");
                createOrderDAT.CY_SEQNR = string.Empty;
                createOrderDAT.TIME_STAMP1 = null;
                createOrderDAT.AUFNR = string.Empty;
                //2013-9-30 现在不管新老接口库位都采用les代码，由安吉翻译为sap代码
                createOrderDAT.LGORT = det.LocationFrom;
                createOrderDAT.UMLGO = det.LocationTo;
                //createOrderDAT.LGORT = this.genericMgr.FindById<Location>(det.LocationFrom).SAPLocation;
                //createOrderDAT.UMLGO = this.genericMgr.FindById<Location>(det.LocationTo).SAPLocation;
                //2013-10-08传入工位信息
                createOrderDAT.LGPBE = det.BinTo;
                createOrderDAT.REQ_TIME_STAMP = orderMaster.WindowTime.ToString("yyyyMMddHHmmss");
                createOrderDAT.FLG_SORT = "N";
                createOrderDAT.PLNBEZ = string.Empty;
                createOrderDAT.KTEXT = string.Empty;
                createOrderDAT.ZPLISTNO = det.Id.ToString();
                createOrderDAT.ErrorCount = 0;
                createOrderDAT.IsCreateDat = false;
                createOrderDAT.CreateUserName = det.CreateUserName;
                //createOrderDAT.FileName = FileName;
                this.genericMgr.Create(createOrderDAT);
            }
        }

        /// <summary>
        /// 新格式要货单
        /// </summary>
        /// <param name="orderMaster"></param>
        [Transaction(TransactionMode.Requires)]
        private void CreateProcurementOrderDAT(OrderMaster orderMaster)
        {
            if (orderMaster.OrderDetails == null || orderMaster.OrderDetails.Count == 0)
            {
                throw new BusinessException("明细为空不能创建。");
            }
            //string FileName = "SHIP" + DateTime.Now.ToString("yyMMddHHmmss");
            foreach (OrderDetail det in orderMaster.OrderDetails.OrderBy(od => od.Sequence).ToList())
            {
                CreateProcurementOrderDAT orderDAT = new CreateProcurementOrderDAT();
                orderDAT.OrderNo = det.OrderNo;
                //orderDAT.Van = string.Empty;//整车Van号,只有排序单才有
                orderDAT.OrderStrategy = (int)orderMaster.OrderStrategy == 0 || (int)orderMaster.OrderStrategy == 1 ? 1 : (int)orderMaster.OrderStrategy;
                orderDAT.StartTime = det.StartDate != null ? det.StartDate.Value : System.DateTime.Now;//要求发货时间
                orderDAT.WindowTime = orderMaster.WindowTime;//要求到货时间
                orderDAT.Priority = (int)orderMaster.Priority;
                //orderDAT.Sequence = string.Empty;//整车顺序号,只有排序单才有
                orderDAT.PartyFrom = det.LocationFrom;//安吉要求这里要放库位
                orderDAT.PartyTo = det.LocationTo;//安吉要求这里要放库位
                orderDAT.Dock = orderMaster.Dock;
                orderDAT.CreateDate = System.DateTime.Now;
                orderDAT.Flow = orderMaster.Flow;
                orderDAT.LineSeq = det.Id; //行号
                orderDAT.Item = det.Item;
                orderDAT.ManufactureParty = det.ManufactureParty;
                orderDAT.LocationTo = det.BinTo;//安吉要求这里要放工位
                //orderDAT.Bin = string.Empty;
                orderDAT.OrderedQty = det.OrderedQty;
                orderDAT.IsShipExceed = orderMaster.IsShipExceed;
                orderDAT.IsCreateDat = false;
                this.genericMgr.Create(orderDAT);
            }
        }

        /// <summary>
        /// 排序单
        /// </summary>
        /// <param name="orderMaster"></param>
        [Transaction(TransactionMode.Requires)]
        private void CreateSeqOrderDAT(OrderMaster orderMaster)
        {
            if (orderMaster.OrderDetails == null || orderMaster.OrderDetails.Count == 0)
            {
                throw new BusinessException("明细为空不能创建。");
            }
            //string FileName = "SEQ" + DateTime.Now.ToString("yyMMddHHmmss");
            foreach (OrderDetail det in orderMaster.OrderDetails.OrderBy(od => od.Sequence).ToList())
            {
                CreateSeqOrderDAT createSeqOrderDAT = new CreateSeqOrderDAT();
                createSeqOrderDAT.SeqNo = det.OrderNo;
                createSeqOrderDAT.Seq = det.Sequence;
                createSeqOrderDAT.Flow = orderMaster.SequenceGroup;
                createSeqOrderDAT.StartTime = orderMaster.StartTime;
                createSeqOrderDAT.WindowTime = orderMaster.WindowTime;
                createSeqOrderDAT.PartyFrom = orderMaster.LocationFrom;//安吉就是这么要求的
                createSeqOrderDAT.PartyTo = orderMaster.LocationTo;//安吉就是这么要求的
                createSeqOrderDAT.LocationTo = det.BinTo;//安吉就是这么要求的
                createSeqOrderDAT.Container = det.Container;
                createSeqOrderDAT.CreateDate = det.CreateDate;
                createSeqOrderDAT.Item = det.Item;
                createSeqOrderDAT.ManufactureParty = det.ManufactureParty;
                createSeqOrderDAT.Qty = det.OrderedQty;
                createSeqOrderDAT.SequenceNumber = det.ExternalSequence;
                createSeqOrderDAT.Van = det.ExternalOrderNo;
                createSeqOrderDAT.Line = string.Empty;
                createSeqOrderDAT.Station = det.BinTo;
                createSeqOrderDAT.ErrorCount = 0;
                createSeqOrderDAT.UploadDate = null;
                createSeqOrderDAT.IsCreateDat = false;
                createSeqOrderDAT.FileName = string.Empty;
                createSeqOrderDAT.OrderDetId = det.Id;
                this.genericMgr.Create(createSeqOrderDAT);
            }
        }

        /// <summary>
        /// 退货单
        /// </summary>
        /// <param name="orderMaster"></param>
        [Transaction(TransactionMode.Requires)]
        private void CreateReturnOrderDAT(OrderMaster orderMaster)
        {
            if (orderMaster.OrderDetails == null || orderMaster.OrderDetails.Count == 0)
            {
                throw new BusinessException("明细为空不能创建。");
            }
            //string FileName = "RETU" + DateTime.Now.ToString("yyMMddHHmmss");
            foreach (OrderDetail det in orderMaster.OrderDetails.OrderBy(od => od.Sequence).ToList())
            {
                YieldReturn yieldReturn = new YieldReturn();
                yieldReturn.IpNo = orderMaster.OrderNo;
                yieldReturn.ArriveTime = orderMaster.WindowTime;
                yieldReturn.PartyFrom = orderMaster.LocationFrom;//安吉就是这么要求的
                yieldReturn.PartyTo = orderMaster.LocationTo;//安吉就是这么要求的
                yieldReturn.Dock = orderMaster.Dock;
                yieldReturn.IpCreateDate = orderMaster.CreateDate;
                yieldReturn.Seq = det.Sequence.ToString();
                yieldReturn.Item = det.Item;
                yieldReturn.ManufactureParty = det.ManufactureParty;
                yieldReturn.Qty = det.OrderedQty;
                yieldReturn.IsConsignment = false;// 2013-8-12 赵冬梅说良品退库一定是退的非寄售的东西
                yieldReturn.CreateDate = DateTime.Now;

                this.genericMgr.Create(yieldReturn);
            }
        }
        #endregion

        #region 客户化功能
        #region 整车上线
        public void StartVanOrder(string orderNo)
        {
            try
            {
                User user = SecurityContextHolder.Get();
                this.genericMgr.UpdateWithNativeQuery("exec USP_Busi_StartVanProdOrder ?,?,?", new object[] { orderNo, user.Id, user.FullName });
                string prodLine = this.genericMgr.FindAllWithNativeSql<string>("select Flow from ORD_OrderMstr_4 WITH(NOLOCK) where OrderNo = ?", orderNo).Single();
                this.productionLineMgr.AsyncUpdateOrderBomCPTime(prodLine, user);
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

        #region 空车上线
        public void StartEmptyVanOrder(string prodLine)
        {
            try
            {
                User user = SecurityContextHolder.Get();
                this.genericMgr.UpdateWithNativeQuery("exec USP_Busi_StartEmptyVanProdOrder ?,?,?", new object[] { prodLine, user.Id, user.FullName });
                this.productionLineMgr.AsyncUpdateOrderBomCPTime(prodLine, user);
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

        public void CancelEmptyVanOrder(string prodLine, int orderSeqId)
        {
            try
            {
                User user = SecurityContextHolder.Get();
                this.genericMgr.UpdateWithNativeQuery("exec USP_Busi_CancelStartEmptyVanProdOrder ?,?,?,?", new object[] { prodLine, orderSeqId, user.Id, user.FullName });
                this.productionLineMgr.AsyncUpdateOrderBomCPTime(prodLine, user);
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

        #region 整车/驾驶室/底盘下线
        public void ReceiveVanOrder(string orderNo, bool isCheckIssue, bool isCheckItemTrace, bool isForce)
        {
            try
            {
                User user = SecurityContextHolder.Get();
                IList<string> errorMsgList = this.genericMgr.FindAllWithNativeSql<string>("exec USP_Busi_ReceiveVanProdOrderVerify ?,?,?,?", new object[] { orderNo, isCheckIssue, isCheckItemTrace, isForce });

                if (errorMsgList != null && errorMsgList.Count > 0)
                {
                    BusinessException businessException = new BusinessException();

                    foreach (string errorMsg in errorMsgList)
                    {
                        businessException.AddMessage(errorMsg);
                    }

                    throw businessException;
                }
                else
                {
                    this.genericMgr.UpdateWithNativeQuery("exec USP_Busi_ReceiveVanProdOrder ?,?,?", new object[] { orderNo, user.Id, user.FullName });
                }
            }
            catch (BusinessException ex)
            {
                throw ex;
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

        #region 精益引擎
        private static object RunLeanEngineLock = new object();
        public void RunLeanEngine()
        {
            lock (RunLeanEngineLock)
            {
                try
                {
                    User user = SecurityContextHolder.Get();
                    this.genericMgr.UpdateWithNativeQuery("exec USP_LE_RunLeanEngine ?,?", new object[] { user.Id, user.FullName });
                }
                catch (BusinessException ex)
                {
                    log.Error(ex.Message);
                    throw ex;
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        if (ex.InnerException.InnerException != null)
                        {
                            log.Error(ex.InnerException.InnerException.Message);
                            throw new BusinessException(ex.InnerException.InnerException.Message);
                        }
                        else
                        {
                            log.Error(ex.InnerException.Message);
                            throw new BusinessException(ex.InnerException.Message);
                        }
                    }
                    else
                    {
                        log.Error(ex.Message);
                        throw new BusinessException(ex.Message);
                    }
                }
            }
        }
        #endregion
        #endregion

        #region 交货单过账
        [Transaction(TransactionMode.Requires)]
        public void DistributionReceiveOrder(OrderMaster orderMaster)
        {
            #region 检查交货单明细是否一起过账
            IList<OrderDetail> selectOrderDetailList = this.genericMgr.FindAll<OrderDetail>("select d from OrderDetail as d where d.OrderNo='" + orderMaster.OrderNo + "'");

            foreach (OrderDetail orderDetail in selectOrderDetailList)
            {
                bool b = false;
                foreach (OrderDetail item in orderMaster.OrderDetails)
                {
                    if (orderDetail.Id == item.Id)
                    {
                        b = true;
                        break;
                    }
                }

                if (b == false)
                {
                    string externalNo = this.genericMgr.FindById<OrderMaster>(orderDetail.OrderNo).ExternalOrderNo;
                    throw new BusinessException("交货单的明细必须一起过账。订单号为" + orderDetail.OrderNo);
                }
            }
            #endregion

            #region 把创建状态的OrderMaster释放
            this.genericMgr.CleanSession();
            if (orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.Create)
            {
                this.ReleaseOrder(orderMaster);
            }

            #endregion

            #region 过账
            this.ReceiveOrder(orderMaster.OrderDetails);
            #endregion

            #region 把OrderMaster的状态关闭
            if (orderMaster.Status != com.Sconit.CodeMaster.OrderStatus.Close)
            {
                this.ManualCloseOrder(orderMaster);
            }
            #endregion
        }
        #endregion

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

        #region 生产单报工
        [Transaction(TransactionMode.Requires)]
        public void ReportOrderOp(int orderOpId, decimal reportQty, decimal scrapQty)
        {
            this.ReportOrderOp(orderOpId, reportQty, scrapQty, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public void ReportOrderOp(int orderOpId, decimal reportQty, decimal scrapQty, DateTime effectiveDate)
        {
            OrderOperation orderOperation = this.genericMgr.FindById<OrderOperation>(orderOpId);
            OrderMaster orderMaster = this.genericMgr.FindEntityWithNativeSql<OrderMaster>("select * from ORD_OrderMstr_4 WITH(NOLOCK) where OrderNo = ?", orderOperation.OrderNo).Single();
            OrderDetail orderDetail = this.genericMgr.FindEntityWithNativeSql<OrderDetail>("select * from ORD_OrderDet_4 WITH(NOLOCK) where OrderNo = ?", orderOperation.OrderNo).Single();
            IList<ProductLineMap> prodLineMapList = this.genericMgr.FindAll<ProductLineMap>("select s from ProductLineMap as s where s.ProductLine = ?", orderMaster.Flow);
//            IList<OrderOperation> refOrderOperationList = null;
//            if (orderOperation.NeedReport)
//            {
//                refOrderOperationList = this.genericMgr.FindEntityWithNativeSql<OrderOperation>(@"select * from ORD_OrderOp WITH(NOLOCK) where OrderNo= ?
//                    and Op > ISNULL((select Op from ORD_OrderOp WITH(NOLOCK) where OrderNo = ? and Op < ? and NeedReport = ?), 0) and Op < ?"
//                        , new object[] { orderMaster.OrderNo, orderMaster.OrderNo, orderOperation.Operation, true, orderOperation.Operation });
//            }

            if (orderMaster.Status != CodeMaster.OrderStatus.InProcess)
            {
                throw new BusinessException("状态为{1}的生产单{0}不能报工",
                            orderMaster.OrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
            }

            if (!orderOperation.NeedReport)
            {
                throw new BusinessException("不是报工工序不能报工。");
            }

            #region 检查比报工工序小的工序是否报工
            //2013-10-23 三勇说对于试制订单来说不要去严格限制报工顺序
            int istrial = (from prodLine in prodLineMapList
                           where prodLine.SAPProductLine == "ZP01"
                           select prodLine).Count();
            if (istrial == 0)
            {
                int count = this.genericMgr.FindAllWithNativeSql<int>(@"select count(*) as counter 
                                                        from ORD_OrderOp as op 
                                                        left join SAP_ProdOpReport as rp on op.Id = rp.OrderOpId and rp.IsCancel = ?
                                                        where op.OrderNo = ? and op.NeedReport = ? and op.Op < ? and rp.Id is null", new object[] { false, orderOperation.OrderNo, true, orderOperation.Operation }).SingleOrDefault();

                if (count > 0)
                {
                    throw new BusinessException("有小于报工工序的工序还未报工。");
                }
            }
            #endregion

            DateTime dateTimeNow = DateTime.Now;

            #region 报工
            if (reportQty > 0)
            {
                OrderOperationReport orderOperationReport = new OrderOperationReport();
                orderOperationReport.OrderNo = orderMaster.OrderNo;
                orderOperationReport.OrderDetailId = orderDetail.Id;
                orderOperationReport.OrderOperationId = orderOperation.Id;
                orderOperationReport.Operation = orderOperation.Operation;
                orderOperationReport.ReportQty = reportQty;
                orderOperationReport.ScrapQty = 0;
                orderOperationReport.BackflushQty = reportQty;
                orderOperationReport.WorkCenter = orderOperation.WorkCenter;
                orderOperationReport.Status = CodeMaster.OrderOpReportStatus.Close;
                orderOperationReport.EffectiveDate = effectiveDate;

                com.Sconit.Entity.SAP.ORD.ProdOpReport prodOpReport = new com.Sconit.Entity.SAP.ORD.ProdOpReport();
                prodOpReport.AUFNR = orderMaster.ExternalOrderNo;
                prodOpReport.WORKCENTER = orderOperation.WorkCenter;
                prodOpReport.GAMNG = reportQty;
                prodOpReport.Status = Entity.SAP.StatusEnum.Pending;
                prodOpReport.CreateDate = dateTimeNow;
                prodOpReport.LastModifyDate = dateTimeNow;
                prodOpReport.ErrorCount = 0;
                prodOpReport.SCRAP = 0;
                prodOpReport.IsCancel = false;
                prodOpReport.OrderNo = orderOperation.OrderNo;
                prodOpReport.OrderOpId = orderOperation.Id;
                prodOpReport.EffectiveDate = effectiveDate;
                prodOpReport.ProdLine = orderMaster.Flow;

                #region 成品收货
                if (orderOperation.IsReceiveFinishGoods)
                {
                    orderDetail.OrderDetailInputs = new List<OrderDetailInput>();
                    OrderDetailInput orderDetailInput = new OrderDetailInput();
                    orderDetailInput.ReceiveQty = reportQty;
                    orderDetail.OrderDetailInputs.Add(orderDetailInput);
                    IList<OrderDetail> orderDetailList = new List<OrderDetail>();
                    orderDetailList.Add(orderDetail);

                    ReceiptMaster receiptMaster = this.ReceiveOrder(orderDetailList);

                    orderOperationReport.ReceiptNo = receiptMaster.ReceiptNo;
                    prodOpReport.ReceiptNo = receiptMaster.ReceiptNo;
                }
                #endregion

                this.genericMgr.Create(orderOperationReport);

                prodOpReport.OrderOpReportId = orderOperationReport.Id;
                this.genericMgr.Create(prodOpReport);

                #region 反冲物料
                //if (orderOperation.NeedReport)
                //{
                //    orderOperation.CurrentReportQty = reportQty;
                //    this.productionLineMgr.BackflushProductOrder(orderOperation, orderOperationReport);

                //    if (refOrderOperationList != null && refOrderOperationList.Count > 0)
                //    {
                //        foreach (OrderOperation refOrderOperation in refOrderOperationList)
                //        {
                //            refOrderOperation.CurrentReportQty = reportQty;
                //            this.productionLineMgr.BackflushProductOrder(refOrderOperation, orderOperationReport);

                //            refOrderOperation.BackflushQty += reportQty;
                //            refOrderOperation.ReportQty += reportQty;
                //        }
                //    }
                //}
                #endregion

                //orderOperation.BackflushQty += reportQty;
                orderOperation.ReportQty += reportQty;
            }
            else if (reportQty < 0)
            {
                throw new BusinessException("报工数量不能小于0。");
            }
            #endregion

            #region 废品
            if (scrapQty > 0)
            {
                OrderOperationReport orderOperationReport = new OrderOperationReport();
                orderOperationReport.OrderNo = orderMaster.OrderNo;
                orderOperationReport.OrderDetailId = orderDetail.Id;
                orderOperationReport.OrderOperationId = orderOperation.Id;
                orderOperationReport.Operation = orderOperation.Operation;
                orderOperationReport.ReportQty = 0;
                orderOperationReport.ScrapQty = scrapQty;
                orderOperationReport.BackflushQty = scrapQty;
                orderOperationReport.WorkCenter = orderOperation.WorkCenter;
                orderOperationReport.Status = CodeMaster.OrderOpReportStatus.Close;
                orderOperationReport.EffectiveDate = effectiveDate;

                com.Sconit.Entity.SAP.ORD.ProdOpReport prodOpReport = new com.Sconit.Entity.SAP.ORD.ProdOpReport();
                prodOpReport.AUFNR = orderMaster.ExternalOrderNo;
                prodOpReport.WORKCENTER = orderOperation.WorkCenter;
                prodOpReport.GAMNG = 0;
                prodOpReport.Status = Entity.SAP.StatusEnum.Pending;
                prodOpReport.CreateDate = dateTimeNow;
                prodOpReport.LastModifyDate = dateTimeNow;
                prodOpReport.ErrorCount = 0;
                prodOpReport.SCRAP = scrapQty;
                prodOpReport.IsCancel = false;
                prodOpReport.OrderNo = orderOperation.OrderNo;
                prodOpReport.OrderOpId = orderOperation.Id;
                prodOpReport.EffectiveDate = effectiveDate;
                prodOpReport.ProdLine = orderMaster.Flow;

                #region 成品收货
                if (orderOperation.IsReceiveFinishGoods)
                {
                    orderDetail.OrderDetailInputs = new List<OrderDetailInput>();
                    OrderDetailInput orderDetailInput = new OrderDetailInput();
                    orderDetailInput.ScrapQty = scrapQty;
                    orderDetail.OrderDetailInputs.Add(orderDetailInput);
                    IList<OrderDetail> orderDetailList = new List<OrderDetail>();
                    orderDetailList.Add(orderDetail);

                    ReceiptMaster receiptMaster = this.ReceiveOrder(orderDetailList);

                    orderOperationReport.ReceiptNo = receiptMaster.ReceiptNo;
                    prodOpReport.ReceiptNo = receiptMaster.ReceiptNo;
                }
                #endregion

                this.genericMgr.Create(orderOperationReport);

                prodOpReport.OrderOpReportId = orderOperationReport.Id;
                this.genericMgr.Create(prodOpReport);

                #region 反冲物料
                //if (orderOperation.NeedReport)
                //{
                //    orderOperation.CurrentReportQty = 0;
                //    orderOperation.CurrentScrapQty = scrapQty;
                //    this.productionLineMgr.BackflushProductOrder(orderOperation, orderOperationReport);

                //    if (refOrderOperationList != null && refOrderOperationList.Count > 0)
                //    {
                //        foreach (OrderOperation refOrderOperation in refOrderOperationList)
                //        {
                //            refOrderOperation.CurrentReportQty = 0;
                //            refOrderOperation.CurrentScrapQty = scrapQty;
                //            this.productionLineMgr.BackflushProductOrder(refOrderOperation, orderOperationReport);

                //            refOrderOperation.BackflushQty += scrapQty;
                //            refOrderOperation.ScrapQty += scrapQty;
                //        }
                //    }
                //}
                #endregion

                //orderOperation.BackflushQty += scrapQty;
                orderOperation.ScrapQty += scrapQty;
            }
            else if (scrapQty < 0)
            {
                throw new BusinessException("废品数量不能小于0。");
            }
            #endregion

            this.genericMgr.Update(orderOperation);
            //if (refOrderOperationList != null && refOrderOperationList.Count > 0)
            //{
            //    foreach (OrderOperation refOrderOperation in refOrderOperationList)
            //    {
            //        this.genericMgr.Update(refOrderOperation);
            //    }
            //}
        }

        public void MakeupReportOrderOp()
        {
            IList<OrderOperationReport> orderOperationReportList = this.genericMgr.FindEntityWithNativeSql<OrderOperationReport>(@"select distinct opr.* from ORD_OrderOpReport as opr 
                inner join ORD_OrderOp as op on opr.OrderOpId = op.Id
                inner join ORD_OrderOp as op2 on op.OrderNo = op2.OrderNo and op.Op > op2.Op and op2.NeedReport = 0 and op2.ReportQty = 0
                where opr.[Status] = 0");

            //IList<OrderOperationReport> orderOperationReportList = this.genericMgr.FindEntityWithNativeSql<OrderOperationReport>(@"select * from ORD_OrderOpReport where Id in (29829,30680,30681,32164,33947,35003)");
            //IList<OrderOperationReport> orderOperationReportList = this.genericMgr.FindEntityWithNativeSql<OrderOperationReport>(@"select * from ORD_OrderOpReport where Id in (30681, 32164)");

            foreach (OrderOperationReport orderOperationReport in orderOperationReportList)
            {
                SecurityContextHolder.Set(this.genericMgr.FindById<User>(orderOperationReport.CreateUserId));
                OrderOperation orderOperation = this.genericMgr.FindById<OrderOperation>(orderOperationReport.OrderOperationId);

                IList<OrderOperation> refOrderOperationList = this.genericMgr.FindEntityWithNativeSql<OrderOperation>(@"select * from ORD_OrderOp where OrderNo= ?
                    and Op > ISNULL((select Op from ORD_OrderOp where OrderNo = ? and Op < ? and NeedReport = ?), 0) and Op < ?"
                            , new object[] { orderOperation.OrderNo, orderOperation.OrderNo, orderOperation.Operation, true, orderOperation.Operation });

                if (refOrderOperationList != null && refOrderOperationList.Count > 0)
                {
                    foreach (OrderOperation refOrderOperation in refOrderOperationList)
                    {
                        refOrderOperation.CurrentReportQty = orderOperationReport.ReportQty;
                        try
                        {
                            try
                            {
                                this.productionLineMgr.BackflushProductOrder(refOrderOperation, orderOperationReport);
                                this.genericMgr.FlushSession();
                            }
                            catch (Exception ex)
                            {
                                this.genericMgr.CleanSession();
                                log.Error(ex);
                                throw ex;
                            }

                            refOrderOperation.BackflushQty += orderOperationReport.ReportQty;
                            refOrderOperation.ReportQty += orderOperationReport.ReportQty;

                            this.genericMgr.Update(refOrderOperation);
                            this.genericMgr.FlushSession();
                        }
                        catch (Exception ex)
                        {
                            this.genericMgr.CleanSession();
                            log.Error(ex);
                        }
                    }
                }
            }
        }
        #endregion

        #region 生产单报工取消
        [Transaction(TransactionMode.Requires)]
        public void CancelReportOrderOp(int orderOpReportId)
        {
            OrderOperationReport orderOperationReport = this.genericMgr.FindById<OrderOperationReport>(orderOpReportId);
            if (orderOperationReport.Status == CodeMaster.OrderOpReportStatus.Cancel)
            {
                throw new BusinessException("工序报工已经取消。");
            }

            DateTime dateTimeNow = DateTime.Now;
            User user = SecurityContextHolder.Get();

            OrderMaster orderMaster = this.genericMgr.FindEntityWithNativeSql<OrderMaster>("select * from ORD_OrderMstr_4 WITH(NOLOCK) where OrderNo = ?", orderOperationReport.OrderNo).Single();

            OrderOperation orderOperation = this.genericMgr.FindById<OrderOperation>(orderOperationReport.OrderOperationId);
            com.Sconit.Entity.SAP.ORD.ProdOpReport prodOpReport = this.genericMgr.FindEntityWithNativeSql<com.Sconit.Entity.SAP.ORD.ProdOpReport>("select * from SAP_ProdOpReport WITH(NOLOCK) where OrderOpReportId = ?", orderOperationReport.Id).Single();

            //IList<OrderOperation> refOrderOperationList = null;
//            if (orderOperation.NeedReport &&
//                !(orderMaster.ProdLineType == CodeMaster.ProdLineType.Cab
//                || orderMaster.ProdLineType == CodeMaster.ProdLineType.Chassis
//                || orderMaster.ProdLineType == CodeMaster.ProdLineType.Assembly
//                || orderMaster.ProdLineType == CodeMaster.ProdLineType.Special
//                || orderMaster.ProdLineType == CodeMaster.ProdLineType.Check))
//            {
//                refOrderOperationList = this.genericMgr.FindEntityWithNativeSql<OrderOperation>(@"select * from ORD_OrderOp where OrderNo= ?
//                    and Op > ISNULL((select Op from ORD_OrderOp where OrderNo = ? and Op < ? and NeedReport = ?), 0) and Op < ?"
//                        , new object[] { orderMaster.OrderNo, orderMaster.OrderNo, orderOperation.Operation, true, orderOperation.Operation });
//            }

            if (orderMaster.ProdLineType == CodeMaster.ProdLineType.Cab
                || orderMaster.ProdLineType == CodeMaster.ProdLineType.Chassis
                || orderMaster.ProdLineType == CodeMaster.ProdLineType.Assembly
                || orderMaster.ProdLineType == CodeMaster.ProdLineType.Special
                || orderMaster.ProdLineType == CodeMaster.ProdLineType.Check)
            {
                if (orderMaster.Status == CodeMaster.OrderStatus.Close)
                {
                    throw new BusinessException("整车物料已经反冲，不能取消报工。");
                }
            }
            else
            {
                #region 物料反消耗
                if (orderOperation.NeedReport)
                {
                    IList<com.Sconit.Entity.SAP.ORD.ProdOpBackflush> prodOpBackflushList =
                        this.genericMgr.FindEntityWithNativeSql<com.Sconit.Entity.SAP.ORD.ProdOpBackflush>(
                        "select * from SAP_ProdOpBackflush where OrderOpReportId = ?", orderOperationReport.Id);

                    if (prodOpBackflushList != null && prodOpBackflushList.Count > 0)
                    {
                        foreach (com.Sconit.Entity.SAP.ORD.ProdOpBackflush prodOpBackflush in prodOpBackflushList)
                        {
                            this.productionLineMgr.AntiBackflushProductOrder(prodOpBackflush);
                        }
                    }

                    //if (refOrderOperationList != null && refOrderOperationList.Count > 0)
                    //{
                    //    foreach (OrderOperation refOrderOperation in refOrderOperationList)
                    //    {
                    //        this.productionLineMgr.AntiBackflushProductOrder(refOrderOperation, orderOperationReport);

                    //        if (!(orderMaster.ProdLineType == CodeMaster.ProdLineType.Cab
                    //          || orderMaster.ProdLineType == CodeMaster.ProdLineType.Chassis
                    //          || orderMaster.ProdLineType == CodeMaster.ProdLineType.Assembly
                    //          || orderMaster.ProdLineType == CodeMaster.ProdLineType.Special
                    //          || orderMaster.ProdLineType == CodeMaster.ProdLineType.Check))
                    //        {
                    //            refOrderOperation.BackflushQty -= prodOpReport.GAMNG;
                    //            refOrderOperation.BackflushQty -= prodOpReport.SCRAP;
                    //        }
                    //        refOrderOperation.ReportQty -= prodOpReport.GAMNG;
                    //        refOrderOperation.ScrapQty -= prodOpReport.SCRAP;
                    //    }
                    //}
                }
                #endregion

                #region 成品反收货
                if (!string.IsNullOrWhiteSpace(orderOperationReport.ReceiptNo))
                {
                    this.receiptMgr.CancelReceipt(orderOperationReport.ReceiptNo);
                }
                #endregion
            }

            #region 更新生产单状态为执行中
            if (orderMaster.Status != CodeMaster.OrderStatus.InProcess)
            {
                if (orderMaster.ProdLineType == CodeMaster.ProdLineType.Cab
               || orderMaster.ProdLineType == CodeMaster.ProdLineType.Chassis
               || orderMaster.ProdLineType == CodeMaster.ProdLineType.Assembly
               || orderMaster.ProdLineType == CodeMaster.ProdLineType.Special
               || orderMaster.ProdLineType == CodeMaster.ProdLineType.Check)
                {
                    orderMaster.Status = CodeMaster.OrderStatus.InProcess;
                    orderMaster.CompleteDate = null;
                    orderMaster.CompleteUserId = null;
                    orderMaster.CompleteUserName = null;
                    this.genericMgr.Update(orderMaster);
                }
                else
                {
                    orderMaster.Status = CodeMaster.OrderStatus.InProcess;
                    orderMaster.CloseDate = null;
                    orderMaster.CloseUserId = null;
                    orderMaster.CloseUserName = null;
                    this.genericMgr.Update(orderMaster);
                }
            }
            #endregion

            #region 更新工序反冲数量
            if (!(orderMaster.ProdLineType == CodeMaster.ProdLineType.Cab
              || orderMaster.ProdLineType == CodeMaster.ProdLineType.Chassis
              || orderMaster.ProdLineType == CodeMaster.ProdLineType.Assembly
              || orderMaster.ProdLineType == CodeMaster.ProdLineType.Special
              || orderMaster.ProdLineType == CodeMaster.ProdLineType.Check))
            {
                orderOperation.BackflushQty -= prodOpReport.GAMNG;
                orderOperation.BackflushQty -= prodOpReport.SCRAP;
            }
            orderOperation.ReportQty -= prodOpReport.GAMNG;
            orderOperation.ScrapQty -= prodOpReport.SCRAP;
            this.genericMgr.Update(orderOperation);
            //if (refOrderOperationList != null && refOrderOperationList.Count > 0)
            //{
            //    foreach (OrderOperation refOrderOperation in refOrderOperationList)
            //    {
            //        this.genericMgr.Update(refOrderOperation);
            //    }
            //}
            #endregion

            #region 更新工序报工记录状态
            orderOperationReport.Status = CodeMaster.OrderOpReportStatus.Cancel;
            orderOperationReport.CancelDate = dateTimeNow;
            orderOperationReport.CancelUser = user.Id;
            orderOperationReport.CancelUserName = user.FullName;
            this.genericMgr.Update(orderOperationReport);
            #endregion

            #region 插入取消报工记录
            com.Sconit.Entity.SAP.ORD.CancelProdOpReport cancelProdOpReport = new Entity.SAP.ORD.CancelProdOpReport();
            cancelProdOpReport.AUFNR = prodOpReport.AUFNR;
            cancelProdOpReport.TEXT = prodOpReport.Id.ToString();
            cancelProdOpReport.Status = Entity.SAP.StatusEnum.Pending;
            cancelProdOpReport.CreateDate = dateTimeNow;
            cancelProdOpReport.LastModifyDate = dateTimeNow;
            cancelProdOpReport.ErrorCount = 0;
            cancelProdOpReport.ReceiptNo = prodOpReport.ReceiptNo;
            cancelProdOpReport.OrderNo = prodOpReport.OrderNo;
            cancelProdOpReport.OrderOpId = prodOpReport.OrderOpId;

            if (prodOpReport.Status != StatusEnum.Success)
            {
                #region 生产单报工还未传给SAP
                #region 更新报工记录为成功，不用再传给SAP
                prodOpReport.Status = StatusEnum.Success;
                prodOpReport.IsCancel = true;
                this.genericMgr.Update(prodOpReport);
                #endregion

                #region 新增取消报工记录
                cancelProdOpReport.Status = Entity.SAP.StatusEnum.Success;
                this.genericMgr.Create(cancelProdOpReport);
                this.genericMgr.FlushSession();
                #endregion
                #endregion
            }
            else
            {
                #region 生产单报工已经传给SAP
                #region 报工记录设置取消标记
                prodOpReport.IsCancel = true;
                this.genericMgr.Update(prodOpReport);
                this.genericMgr.FlushSession();
                #endregion

                #region 取消报工传给SAP
                try
                {
                    SAPService.SAPService sapService = new SAPService.SAPService();
                    sapService.Url = this.systemMgr.ReplaceSIServiceUrl(sapService.Url);
                    IList<string> errorMsgList = sapService.CancelReportProdOrderOperation(cancelProdOpReport.AUFNR, cancelProdOpReport.TEXT, user.Code);
                    if (errorMsgList != null && errorMsgList.Count > 0)
                    {
                        BusinessException businessException = new BusinessException();
                        foreach (string errorMsg in errorMsgList)
                        {
                            businessException.AddMessage(errorMsg);
                        }

                        throw businessException;
                    }

                    this.genericMgr.Create(cancelProdOpReport);
                    this.genericMgr.FlushSession();
                }
                catch (SoapException sex)
                {
                    throw new BusinessException(sex.Actor);
                }
                catch (BusinessException bex)
                {
                    throw bex;
                }
                catch (Exception ex)
                {
                    throw new BusinessException("取消报工异常，异常信息：{0}。", ex.Message);
                }
                #endregion
                #endregion
            }
            #endregion
        }

        [Transaction(TransactionMode.Requires)]
        public void MakeupCancelReportOrderOp()
        {
            OrderOperationReport orderOperationReport = this.genericMgr.FindEntityWithNativeSql<OrderOperationReport>("select * from ORD_OrderOpReport where Id = 33947").Single();
            OrderOperation refOrderOperation = this.genericMgr.FindEntityWithNativeSql<OrderOperation>("select * from ORD_OrderOp where Id = 66380").Single();
            SecurityContextHolder.Set(this.genericMgr.FindById<User>(orderOperationReport.CreateUserId));
            this.productionLineMgr.AntiBackflushProductOrder(refOrderOperation, orderOperationReport);

            refOrderOperation.BackflushQty -= orderOperationReport.ReportQty;
            refOrderOperation.BackflushQty -= orderOperationReport.ScrapQty;
            refOrderOperation.ReportQty -= orderOperationReport.ReportQty;
            refOrderOperation.ScrapQty -= orderOperationReport.ScrapQty;

            this.genericMgr.Update(refOrderOperation);

            orderOperationReport = this.genericMgr.FindEntityWithNativeSql<OrderOperationReport>("select * from ORD_OrderOpReport where Id = 35003").Single();
            SecurityContextHolder.Set(this.genericMgr.FindById<User>(orderOperationReport.CreateUserId));
            this.productionLineMgr.AntiBackflushProductOrder(refOrderOperation, orderOperationReport);

            refOrderOperation.BackflushQty -= orderOperationReport.ReportQty;
            refOrderOperation.BackflushQty -= orderOperationReport.ScrapQty;
            refOrderOperation.ReportQty -= orderOperationReport.ReportQty;
            refOrderOperation.ScrapQty -= orderOperationReport.ScrapQty;

            this.genericMgr.Update(refOrderOperation);
        }
        #endregion

        #region 生产单关闭
        [Transaction(TransactionMode.Requires)]
        public void CloseProdOrder(string orderNo)
        {
            OrderMaster orderMaster = this.genericMgr.FindEntityWithNativeSql<OrderMaster>("select * from ORD_OrderMstr_4 WITH(NOLOCK) where OrderNo = ?", orderNo).SingleOrDefault();

            if (orderMaster == null)
            {
                throw new BusinessException("生产单{0}不存在。", orderNo);
            }

            if (orderMaster.ProdLineType == CodeMaster.ProdLineType.Assembly
                || orderMaster.ProdLineType == CodeMaster.ProdLineType.Cab
                || orderMaster.ProdLineType == CodeMaster.ProdLineType.Chassis
                || orderMaster.ProdLineType == CodeMaster.ProdLineType.Special
                || orderMaster.ProdLineType == CodeMaster.ProdLineType.Check)
            {
                throw new BusinessException("生产单{0}是整车生产单不能手工关闭。", orderNo);
            }

            if (orderMaster.Status != CodeMaster.OrderStatus.InProcess)
            {
                throw new BusinessException("不能关闭状态为{1}的生产单{0}。", orderMaster.OrderNo,
                        systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)orderMaster.Status).ToString()));
            }

            int count = this.genericMgr.FindAllWithNativeSql<int>(@"select count(1) as count from(
                                                                select op.ReportQty
                                                                from ORD_OrderOp as op 
                                                                where op.OrderNo = ?
                                                                group by op.ReportQty) as a", new object[] { false, orderNo }).SingleOrDefault();

            if (count > 1)
            {
                throw new BusinessException("生产单{0}工序报工的数量不一致不能关闭。");
            }

            DateTime dateTimeNow = DateTime.Now;
            User user = SecurityContextHolder.Get();

            orderMaster.Status = CodeMaster.OrderStatus.Close;
            orderMaster.CloseDate = dateTimeNow;
            orderMaster.CloseUserId = user.Id;
            orderMaster.CloseUserName = user.FullName;
            this.genericMgr.Update(orderMaster);
        }
        #endregion

        #region 关键件扫描
        [Transaction(TransactionMode.Requires)]
        public void ScanQualityBarCode(string orderNo, string qualityBarcode, string opRef, int? orderItemTraceId, bool isForce, bool isVI)
        {
            try
            {
                User user = SecurityContextHolder.Get();
                this.genericMgr.UpdateWithNativeQuery("exec USP_Busi_ScanQualityBarcode ?,?,?,?,?,?,?,?",
                    new object[] { orderNo, qualityBarcode, opRef, orderItemTraceId, isForce, isVI, user.Id, user.FullName },
                    new IType[] { NHibernateUtil.String, NHibernateUtil.String, NHibernateUtil.String, NHibernateUtil.Int32, NHibernateUtil.Boolean, NHibernateUtil.Boolean, NHibernateUtil.Int32, NHibernateUtil.String });
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

        [Transaction(TransactionMode.Requires)]
        public void WithdrawQualityBarCode(string qualityBarcode)
        {
            try
            {
                User user = SecurityContextHolder.Get();
                this.genericMgr.UpdateWithNativeQuery("exec USP_Busi_WithdrawQualityBarcode ?,?,?",
                    new object[] { qualityBarcode, user.Id, user.FullName },
                    new IType[] { NHibernateUtil.String, NHibernateUtil.Int32, NHibernateUtil.String });
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

        [Transaction(TransactionMode.Requires)]
        public void ReplaceQualityBarCode(string withdrawQualityBarcode, string scanQualityBarcode)
        {
            try
            {
                User user = SecurityContextHolder.Get();
                this.genericMgr.UpdateWithNativeQuery("exec USP_Busi_ReplaceQualityBarcode ?,?,?,?",
                    new object[] { withdrawQualityBarcode, scanQualityBarcode, user.Id, user.FullName },
                    new IType[] { NHibernateUtil.String, NHibernateUtil.String, NHibernateUtil.Int32, NHibernateUtil.String });
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

        #region 驾驶室出库
        [Transaction(TransactionMode.Requires)]
        public void OutCab(string orderNo)
        {
            #region 校验
            if (string.IsNullOrWhiteSpace(orderNo))
            {
                throw new BusinessException("驾驶室生产单条码不能为空。");
            }

            CabOut cabOut = this.genericMgr.FindAll<CabOut>("from CabOut where OrderNo = ?", orderNo).SingleOrDefault();

            if (cabOut == null)
            {
                throw new BusinessException("生产单号不存在或不是驾驶室的生产单号。");
            }
            else
            {
                if (cabOut.Status == CodeMaster.CabOutStatus.Out)
                {
                    throw new BusinessException("驾驶室已经出库。");
                }
                else if (cabOut.Status == CodeMaster.CabOutStatus.Transfer)
                {
                    throw new BusinessException("驾驶室已经移库。");
                }
                else
                {
                    #region 生产单暂停校验
                    int pauseStatus = int.Parse(this.genericMgr.FindAllWithNativeSql("select PauseStatus from ORD_OrderMstr_4 WITH(NOLOCK) where OrderNo = ?", orderNo)[0].ToString());
                    if (pauseStatus == (int)CodeMaster.PauseStatus.Paused)
                    {
                        throw new BusinessException("驾驶室生产单{0}已经暂停，不能出库。", orderNo);
                    }
                    #endregion
                    #region 移库顺序校验
                    //                    else
                    //                    {
                    //                        string firstTransferOrderNo = this.genericMgr.FindAllWithNativeSql<string>(@"select top 1 op.OrderNo from CUST_CabOut as co inner join ORD_OrderOpCPTime as op on co.OrderNo = op.OrderNo
                    //                                                                                                    where co.Status = ? and co.CabType = ? and op.VanProdLine = op.AssProdLine and op.Op = 1
                    //                                                                                                    order by op.CPTime", new object[] { CodeMaster.CabOutStatus.NotOut, cabOut.CabType }).SingleOrDefault();
                    //                        if (!string.IsNullOrWhiteSpace(firstTransferOrderNo)
                    //                            && firstTransferOrderNo != orderNo)
                    //                        {
                    //                            throw new BusinessException("驾驶室生产单{0}不是第一张要出库的生产单。", orderNo);
                    //                        }
                    //                    }
                    #endregion
                }
            }
            #endregion

            #region 记录出库信息
            DateTime dateTimeNow = DateTime.Now;
            User user = SecurityContextHolder.Get();
            cabOut.Status = CodeMaster.CabOutStatus.Out;
            cabOut.OutDate = dateTimeNow;
            cabOut.OutUser = user.Id;
            cabOut.OutUserName = user.FullName;
            this.genericMgr.Update(cabOut);
            #endregion
        }
        #endregion

        #region 驾驶室移库并投料
        [Transaction(TransactionMode.Requires)]
        public void TansferCab(string orderNo, string flowCode, string qualityBarcode)
        {
            #region 校验
            if (string.IsNullOrWhiteSpace(orderNo))
            {
                throw new BusinessException("驾驶室生产单条码不能为空。");
            }

            if (string.IsNullOrWhiteSpace(qualityBarcode))
            {
                throw new BusinessException("驾驶室条码不能为空。");
            }

            CabOut cabOut = this.genericMgr.FindAll<CabOut>("from CabOut where OrderNo = ?", orderNo).SingleOrDefault();

            if (cabOut == null)
            {
                throw new BusinessException("生产单号不存在或不是驾驶室的生产单号。");
            }
            else
            {
                if (cabOut.Status == CodeMaster.CabOutStatus.NotOut)
                {
                    throw new BusinessException("驾驶室还未出库。");
                }
                else if (cabOut.Status == CodeMaster.CabOutStatus.Transfer)
                {
                    throw new BusinessException("驾驶室已经移库。");
                }
                else
                {
                    #region 生产单暂停校验
                    int pauseStatus = int.Parse(this.genericMgr.FindAllWithNativeSql("select PauseStatus from ORD_OrderMstr_4 WITH(NOLOCK) where OrderNo = ?", orderNo)[0].ToString());
                    if (pauseStatus == (int)CodeMaster.PauseStatus.Paused)
                    {
                        throw new BusinessException("驾驶室生产单{0}已经暂停，不能移库。", orderNo);
                    }
                    #endregion
                    #region 移库顺序校验
                    //                    else
                    //                    {
                    //                        string firstTransferOrderNo = this.genericMgr.FindAllWithNativeSql<string>(@"select top 1 op.OrderNo from CUST_CabOut as co inner join ORD_OrderOpCPTime as op on co.OrderNo = op.OrderNo
                    //                                                                                                    where co.Status = ? and co.CabType = ? and op.VanProdLine = op.AssProdLine and op.Op = 1
                    //                                                                                                    order by op.CPTime", new object[] { CodeMaster.CabOutStatus.Out, cabOut.CabType }).SingleOrDefault();
                    //                        if (!string.IsNullOrWhiteSpace(firstTransferOrderNo)
                    //                            && firstTransferOrderNo != orderNo)
                    //                        {
                    //                            throw new BusinessException("驾驶室生产单{0}不是第一张要移库的生产单。", orderNo);
                    //                        }
                    //                    }
                    #endregion
                }
            }
            string huPreFix = this.genericMgr.FindAllWithNativeSql<string>("select PreFixed from SYS_SNRule where Code = ?", CodeMaster.DocumentsType.INV_Hu).Single();

            com.Sconit.Entity.MD.Item item = null;
            if (qualityBarcode.StartsWith(huPreFix, StringComparison.OrdinalIgnoreCase))
            {
                #region Hu
                item = this.genericMgr.FindEntityWithNativeSql<com.Sconit.Entity.MD.Item>("select * from MD_Item where Code in (select Item from INV_Hu where HuId = ?)", qualityBarcode).Single();
                if (item.Code != cabOut.CabItem)
                {
                    throw new BusinessException("条码{0}不是驾驶室生产单", qualityBarcode);
                }
                #endregion
            }
            else
            {
                #region 关键件条码
                if (qualityBarcode.Length == 17)
                {
                    com.Sconit.Entity.MD.Item i = this.genericMgr.FindById<com.Sconit.Entity.MD.Item>(cabOut.CabItem);
                    if (i.ShortCode != qualityBarcode.Substring(4, 5))
                    {
                        throw new BusinessException("驾驶室条码的零件短代码和关键件短代码不一致。");
                    }
                    else
                    {
                        item = i;
                    }
                }
                else
                {
                    throw new BusinessException("驾驶室条码长度不是17位。");
                }
                #endregion
            }

            FlowMaster flow = this.genericMgr.FindById<FlowMaster>(flowCode);
            object[] bom = this.genericMgr.FindAllWithNativeSql<object[]>("select Id, Location from ORD_OrderBomDet where Item = ? and OrderNo = ?", new object[] { item.Code, orderNo }).Single();

            if (flow.LocationTo != (string)bom[1])
            {
                throw new BusinessException("驾驶室的消耗库位{0}和路线的目的库位{1}不一致。", (string)bom[1], flow.LocationTo);
            }
            #endregion

            #region 记录移库信息
            DateTime dateTimeNow = DateTime.Now;
            User user = SecurityContextHolder.Get();
            cabOut.Status = CodeMaster.CabOutStatus.Transfer;
            cabOut.QulityBarcode = qualityBarcode;
            cabOut.TransferDate = dateTimeNow;
            cabOut.TransferUser = user.Id;
            cabOut.TransferUserName = user.FullName;
            this.genericMgr.Update(cabOut);
            #endregion

            #region 移库
            com.Sconit.Entity.ORD.OrderMaster transferOrder = this.TransferFlow2Order(flow, false);
            transferOrder.IsQuick = true;
            transferOrder.WindowTime = dateTimeNow;
            transferOrder.StartTime = dateTimeNow;

            com.Sconit.Entity.ORD.OrderDetail transferOrderDetail = new com.Sconit.Entity.ORD.OrderDetail();
            transferOrderDetail.Item = item.Code;
            transferOrderDetail.ItemDescription = item.Description;
            transferOrderDetail.RequiredQty = 1;
            transferOrderDetail.OrderedQty = 1;
            transferOrderDetail.Uom = item.Uom;
            transferOrderDetail.BaseUom = item.Uom;
            transferOrderDetail.UnitCount = item.UnitCount;
            transferOrderDetail.UnitQty = 1;
            transferOrderDetail.LocationFrom = flow.LocationFrom;
            transferOrderDetail.LocationTo = flow.LocationTo;
            transferOrder.AddOrderDetail(transferOrderDetail);

            this.CreateOrder(transferOrder);
            #endregion

            #region 扫描关键件
            int orderItemTraceId = this.genericMgr.FindAllWithNativeSql<int>("select Id from ORD_OrderItemTrace where OrderBomId = ?", bom[0]).SingleOrDefault();
            if (orderItemTraceId > 0)
            {
                this.ScanQualityBarCode(orderNo, qualityBarcode, null, orderItemTraceId, false, false);
            }
            #endregion
        }
        #endregion

        #region Kit明细修改
        [Transaction(TransactionMode.Requires)]
        public void BatchSeqOrderChange(IList<OrderDetail> orderDetailList, int status)
        {
            OrderMaster mster = this.genericMgr.FindById<OrderMaster>(orderDetailList.First().OrderNo);
            foreach (var det in orderDetailList)
            {
                det.Flow = mster.Flow;
                AllSeqOrderChange(status, det);
            }
        }

        [Transaction(TransactionMode.Requires)]
        public void AllSeqOrderChange(int status, OrderDetail orderDetail)
        {
            string errorMessage = string.Empty;
            User user = SecurityContextHolder.Get();
            if (status == 2)
            {
                var seqOrderChanges = this.genericMgr.FindAll<SeqOrderChange>(" select s from SeqOrderChange as s where s.OrderDetId=? ", orderDetail.Id);
                if (seqOrderChanges == null || seqOrderChanges.Count == 0)
                {
                    var oldOrderDet = this.genericMgr.FindAllWithNativeSql<object[]>(" select Item,OrderQty from VIEW_OrderDet where id=? ", orderDetail.Id)[0];
                    var newSeqOrderChange = Mapper.Map<OrderDetail, SeqOrderChange>(orderDetail);
                    newSeqOrderChange.ExternalOrderNo = orderDetail.ReserveNo;
                    newSeqOrderChange.ExternalSequence = orderDetail.ReserveLine;
                    newSeqOrderChange.OrderedQty = (decimal)oldOrderDet[1];
                    newSeqOrderChange.Status = 0;
                    newSeqOrderChange.OrderDetId = orderDetail.Id;
                    newSeqOrderChange.CreateDate = System.DateTime.Now;
                    newSeqOrderChange.CreateUserId = user.Id;
                    newSeqOrderChange.CreateUserName = user.FirstName + user.LastName;
                    this.genericMgr.Create(newSeqOrderChange);
                }
            }
            switch (status)
            {
                case 1://增加
                    string seqSql = " select max(seq) as seq from VIEW_OrderDet where orderno=? ";
                    var seq = this.genericMgr.FindAllWithNativeSql<int>(seqSql, orderDetail.OrderNo)[0];
                    orderDetail.Sequence = ++seq;
                    this.genericMgr.Create(orderDetail);
                    var addSeqOrderChange = Mapper.Map<OrderDetail, SeqOrderChange>(orderDetail);
                    addSeqOrderChange.ExternalOrderNo = orderDetail.ReserveNo;
                    addSeqOrderChange.ExternalSequence = orderDetail.ReserveLine;
                    addSeqOrderChange.Sequence = ++seq;
                    addSeqOrderChange.Status = 1;
                    addSeqOrderChange.OrderDetId = orderDetail.Id;
                    addSeqOrderChange.CreateDate = System.DateTime.Now;
                    addSeqOrderChange.CreateUserId = user.Id;
                    addSeqOrderChange.CreateUserName = user.FirstName + user.LastName;
                    this.genericMgr.Create(addSeqOrderChange);
                    errorMessage = string.Format("尊敬的用户:\n\t\t Kit单号{0}Van号{1}车架流水号{2}新增物料{3}数量{4}。操作人：{5}", orderDetail.OrderNo, orderDetail.ReserveNo, orderDetail.ReserveLine, orderDetail.Item, orderDetail.OrderedQty, user.FullName);
                    break;
                case 2://修改
                    this.genericMgr.Update(orderDetail);
                    var updateSeqOrderChange = Mapper.Map<OrderDetail, SeqOrderChange>(orderDetail);
                    updateSeqOrderChange.ExternalOrderNo = orderDetail.ReserveNo;
                    updateSeqOrderChange.ExternalSequence = orderDetail.ReserveLine;
                    updateSeqOrderChange.Status = 2;
                    updateSeqOrderChange.OrderDetId = orderDetail.Id;
                    updateSeqOrderChange.CreateDate = System.DateTime.Now;
                    updateSeqOrderChange.CreateUserId = user.Id;
                    updateSeqOrderChange.CreateUserName = user.FirstName + user.LastName;
                    this.genericMgr.Create(updateSeqOrderChange);
                    var oldOrderDet = this.genericMgr.FindAllWithNativeSql<object[]>(" select Item,OrderQty from VIEW_OrderDet where id=? ", orderDetail.Id)[0];
                    errorMessage = string.Format("尊敬的用户:\n\t\t Kit单号{0}Van号{1}车架流水号{2}修改物料{3}变更成{4}，数量{5}变更成{6}。操作人：{7}", orderDetail.OrderNo, orderDetail.ReserveNo, orderDetail.ReserveLine, oldOrderDet[0], orderDetail.Item, oldOrderDet[1], orderDetail.OrderedQty, user.FullName);
                    break;
                case 3://删除
                    var deleteSeqOrderChange = Mapper.Map<OrderDetail, SeqOrderChange>(orderDetail);
                    deleteSeqOrderChange.ExternalOrderNo = orderDetail.ReserveNo;
                    deleteSeqOrderChange.ExternalSequence = orderDetail.ReserveLine;
                    deleteSeqOrderChange.Status = 3;
                    deleteSeqOrderChange.OrderDetId = orderDetail.Id;
                    deleteSeqOrderChange.CreateDate = System.DateTime.Now;
                    deleteSeqOrderChange.CreateUserId = user.Id;
                    deleteSeqOrderChange.CreateUserName = user.FirstName + user.LastName;
                    this.genericMgr.Create(deleteSeqOrderChange);
                    this.genericMgr.DeleteById<OrderDetail>(orderDetail.Id);
                    errorMessage = string.Format("尊敬的用户:\n\t\t Kit单号{0}Van号{1}车架流水号{2}删除物料{3}，数量{4}。操作人：{5}", orderDetail.OrderNo, orderDetail.ReserveNo, orderDetail.ReserveLine, orderDetail.Item, orderDetail.OrderedQty, user.FullName);
                    break;
                default:
                    break;
            }

            #region 发送邮件
            //if (!string.IsNullOrWhiteSpace(errorMessage))
            //{
            //    log.Debug(errorMessage);
            //    IList<ErrorMessage> errorMessageList = new List<ErrorMessage>();
            //    errorMessageList.Add(new ErrorMessage
            //    {
            //        Template = NVelocityTemplateRepository.TemplateEnum.SeqOrderChange,
            //        Message = errorMessage,
            //    });

            //    this.SendErrorMessage(errorMessageList);
            //}
            #endregion
        }
        #endregion

        #region Wms拣货单收货
        [Transaction(TransactionMode.Requires)]
        public void ReceiveWMSIpMaster(WMSDatFile wMSDatFile)
        {
            try
            {
                LesINLog lesInLog = new LesINLog();

                #region 获得orderdetail
                IList<OrderDetail> orderDetailList = new List<OrderDetail>();
                OrderDetail orderDetail = this.genericMgr.FindById<OrderDetail>(Convert.ToInt32(wMSDatFile.WmsLine));

                if (orderDetail.ReceiveLotSize == 1)
                {
                    throw new BusinessException(string.Format("单号{0}中物料{1}明细行已经关闭，不能收货。", orderDetail.OrderNo, orderDetail.Item));
                }

                //#region 订单头要配置成自动收货
                //OrderMaster ordermaster = this.genericMgr.FindById<OrderMaster>(orderDetail.OrderNo);
                //if (!ordermaster.IsAutoReceive)
                //{
                //    throw new BusinessException();
                //}
                //#endregion

                orderDetail.WmsFileID = wMSDatFile.WMSId;
                orderDetail.ManufactureParty = wMSDatFile.LIFNR;
                orderDetail.ExternalOrderNo = wMSDatFile.WMSId;
                //orderDetail.ExternalSequence = wMSDatFile.WBS;//项目代码
                OrderDetailInput orderDetailInput = new OrderDetailInput();
                orderDetailInput.ShipQty = wMSDatFile.CurrentReceiveQty;
                orderDetailInput.WMSIpNo = wMSDatFile.WMSId;//先记录WMSId号，目前安吉拣货单号只用在接口日志查询中
                orderDetailInput.WMSIpSeq = wMSDatFile.HuId;//WMS行
                orderDetailInput.MoveType = wMSDatFile.MoveType + wMSDatFile.SOBKZ;//移动类型
                //只有311K才传寄售供应商，如果是411K也不传，防止两边库位结算方式设置不一致造成差异
                if (wMSDatFile.SOBKZ.ToUpper() == "K" && wMSDatFile.MoveType == "311")
                    orderDetailInput.ConsignmentParty = wMSDatFile.LIFNR;//厂商代码
                orderDetail.AddOrderDetailInput(orderDetailInput);
                orderDetailList.Add(orderDetail);
                #endregion

                #region 调用发货
                var ipMstr = this.ShipOrder(orderDetailList);

                if (!ipMstr.IsAutoReceive)
                {
                    foreach (IpDetail ipDetail in ipMstr.IpDetails)
                    {
                        if (ipDetail.IpDetailInputs != null && ipDetail.IpDetailInputs.Count > 0)
                        {
                            foreach (IpDetailInput ipDetailInput in ipDetail.IpDetailInputs)
                            {
                                ipDetailInput.ReceiveQty = ipDetailInput.ShipQty;
                            }
                        }
                        else
                        {
                            IpDetailInput ipDetailInput = new IpDetailInput();
                            ipDetailInput.ReceiveQty = ipDetail.Qty;
                            ipDetail.AddIpDetailInput(ipDetailInput);
                        }
                    }
                    this.genericMgr.FlushSession();
                    ReceiveIp(ipMstr.IpDetails, DateTime.Now);
                }
                //var ipDetList = base.genericMgr.FindAll<IpDetail>("from IpDetail as d where d.IpNo=?", ipMstr.IpNo);
                //if (ipDetList != null && ipDetList.Count > 0)
                //{
                //    lesInLog.Qty = ipDetList.FirstOrDefault().ReceivedQty;
                //    lesInLog.QtyMark = true;
                //}
                lesInLog.Qty = wMSDatFile.CurrentReceiveQty;
                lesInLog.QtyMark = true;
                #endregion

                #region 新建Log记录
                lesInLog.Type = "MB1B";
                lesInLog.MoveType = wMSDatFile.MoveType + wMSDatFile.SOBKZ;
                lesInLog.Sequense = "";
                lesInLog.WMSNo = wMSDatFile.WMSId;
                lesInLog.WMSLine = wMSDatFile.WmsLine;
                lesInLog.Item = wMSDatFile.Item;
                lesInLog.HandResult = "S";
                lesInLog.FileName = wMSDatFile.FileName;
                lesInLog.HandTime = System.DateTime.Now;
                lesInLog.IsCreateDat = false;
                lesInLog.ASNNo = wMSDatFile.WmsNo;
                this.genericMgr.Create(lesInLog);
                #endregion

                #region 修改中间表
                wMSDatFile.ReceiveTotal = wMSDatFile.ReceiveTotal + wMSDatFile.CurrentReceiveQty;
                this.genericMgr.Update(wMSDatFile);
                #endregion

                this.genericMgr.FlushSession();
            }
            catch (BusinessException ex)
            {
                throw new BusinessException(ex.GetMessages()[0].GetMessageString());
            }
        }
        #endregion

        #region 批量导入发货
        public void BatchImportShipXls(Stream inputStream)
        {
            if (inputStream.Length == 0)
            {
                throw new BusinessException("Import.Stream.Empty");
            }

            HSSFWorkbook workbook = new HSSFWorkbook(inputStream);

            ISheet sheet = workbook.GetSheetAt(0);
            IEnumerator rows = sheet.GetRowEnumerator();

            ImportHelper.JumpRows(rows, 1);
            BusinessException businessException = new BusinessException();
            #region 列定义

            int colShipQty = 0;//发货数
            int colId = 1;// 明细ID
            int rowCount = 1;
            #endregion
            IList<OrderDetail> exactDetailList = new List<OrderDetail>();
            while (rows.MoveNext())
            {
                rowCount++;
                HSSFRow row = (HSSFRow)rows.Current;
                if (!ImportHelper.CheckValidDataRow(row, 0, 2))
                {
                    break;//边界
                }

                int detailId = 0;//明细Id
                decimal currentShipQty = 0;// 本次发货数

                OrderDetail readDet = null;
                OrderDetailInput input = new OrderDetailInput();

                #region 读取数据


                #region 读取明细Id
                string readId = ImportHelper.GetCellStringValue(row.GetCell(colId));
                if (string.IsNullOrWhiteSpace(readId))
                {
                    businessException.AddMessage(string.Format("第{0}行:明细Id不能为空。", rowCount));
                    continue;
                }
                else
                {
                    if (int.TryParse(readId, out detailId))
                    {
                        if (detailId <= 0)
                        {
                            businessException.AddMessage(string.Format("第{0}行:明细Id{1}填写不正确。", rowCount, readId));
                            continue;
                        }
                        else
                        {
                            try
                            {
                                readDet = this.genericMgr.FindById<OrderDetail>(detailId);
                            }
                            catch (Exception)
                            {
                                businessException.AddMessage(string.Format("第{0}行:明细Id{1}不存在。", rowCount, readId));
                                continue;
                            }
                        }
                    }
                    else
                    {
                        businessException.AddMessage(string.Format("第{0}行:明细Id{1}填写不正确。", rowCount, readId));
                        continue;
                    }
                }
                #endregion

                #region 读取发货数
                string readShipQty = ImportHelper.GetCellStringValue(row.GetCell(colShipQty));
                if (string.IsNullOrWhiteSpace(readShipQty))
                {
                    businessException.AddMessage(string.Format("第{0}行:发货数不能为空。", rowCount));
                    continue;
                }
                else
                {
                    if (decimal.TryParse(readShipQty, out currentShipQty))
                    {
                        if (currentShipQty < 0)
                        {
                            businessException.AddMessage(string.Format("第{0}行:发货数{1}填写不正确。", rowCount, currentShipQty));
                            continue;
                        }
                        else if (currentShipQty == 0)
                        {
                            continue;
                        }
                        else
                        {
                            input.ShipQty = currentShipQty;
                            readDet.AddOrderDetailInput(input);
                        }
                    }
                    else
                    {
                        businessException.AddMessage(string.Format("第{0}行:发货数{1}填写不正确。", rowCount, currentShipQty));
                        continue;
                    }
                }
                #endregion




                var checkDet = exactDetailList.Where(p => p.Id == detailId);

                if (checkDet.Count() <= 0)
                {
                    exactDetailList.Add(readDet);
                }
                else
                {
                    throw new BusinessException(string.Format("第{0}行：明细Id出现重复行请检查数据的准确性", rowCount));
                }

                #endregion


            }

            if (exactDetailList != null && exactDetailList.Count > 0)
            {
                ShipOrder(exactDetailList, DateTime.Now);
            }
            else
            {
                throw new BusinessException(string.Format("有效的数据行为0，可能是模板问题"));
            }

            if (businessException.HasMessage)
            {
                throw businessException;
            }
        }
        #endregion

        #region 试制备件批量拉料
        public void ImportCreateRequisitionXls(Stream inputStream, DateTime windowTime)
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
            int colOrderNo = 1;//生产单号
            int colPriority = 2;//优先级
            int rowCount = 10;
            #endregion
            IList<OrderMaster> exactMasterList = new List<OrderMaster>();
            while (rows.MoveNext())
            {
                rowCount++;
                HSSFRow row = (HSSFRow)rows.Current;
                if (!ImportHelper.CheckValidDataRow(row, 0, 2))
                {
                    break;//边界
                }

                string orderNo = string.Empty;//生产单号

                OrderMaster orderMaster = null;

                #region 生产单号
                orderNo = ImportHelper.GetCellStringValue(row.GetCell(colOrderNo));
                if (string.IsNullOrWhiteSpace(orderNo))
                {
                    businessException.AddMessage(string.Format("第{0}行:生产单号不能为空。", rowCount));
                    continue;
                }
                else
                {
                    var checkExists = exactMasterList.Where(p => p.OrderNo == orderNo);

                    if (checkExists.Count() <= 0)
                    {
                        try
                        {
                            orderMaster = this.genericMgr.FindById<OrderMaster>(orderNo);
                        }
                        catch (Exception)
                        {
                            businessException.AddMessage(string.Format("第{0}行:生产单号{1}不存在。", rowCount, orderNo));
                            continue;
                        }
                    }
                    else
                    {
                        throw new BusinessException(string.Format("第{0}行：生产单号出现重复行请检查数据的准确性", rowCount));
                    }

                }
                #endregion

                #region 优先级
                string readPriority = ImportHelper.GetCellStringValue(row.GetCell(colPriority));
                if (string.IsNullOrWhiteSpace(readPriority))
                {
                    businessException.AddMessage(string.Format("第{0}行:优先级不能为空。", rowCount));
                    continue;
                }
                else
                {
                    if (readPriority == "0")
                    {
                        orderMaster.Priority = com.Sconit.CodeMaster.OrderPriority.Normal;
                    }
                    else if (readPriority == "1")
                    {
                        orderMaster.Priority = com.Sconit.CodeMaster.OrderPriority.Urgent;
                    }
                    else
                    {
                        businessException.AddMessage(string.Format("第{0}行:优先级{1}填写不正确。", rowCount, readPriority));
                        continue;
                    }
                }
                #endregion
                exactMasterList.Add(orderMaster);

            }

            if (exactMasterList != null && exactMasterList.Count > 0)
            {
                User CurrentUser = SecurityContextHolder.Get();
                foreach (var master in exactMasterList)
                {
                    try
                    {
                       IList<object[]> returnMessages= this.genericMgr.FindAllWithNativeSql<object[]>("exec USP_LE_ManualGenOrder ?,?,?,?,?",
                            new object[] { master.OrderNo, windowTime, (int)master.Priority, CurrentUser.Id, CurrentUser.FullName },
                            new IType[] { NHibernateUtil.String, NHibernateUtil.DateTime, NHibernateUtil.Int16, NHibernateUtil.Int32, NHibernateUtil.String });
                   //     IList<object[]> returnMessages = this.genericMgr.FindAllWithNativeSql<object[]>("exec USP_LE_ManualGenOrder ?,?,?,?,?",
                   //new object[] { orderNo, windowTime.Value, priority, CurrentUser.Id, CurrentUser.FullName },
                   //new IType[] { NHibernateUtil.String, NHibernateUtil.DateTime, NHibernateUtil.Int16, NHibernateUtil.Int32, NHibernateUtil.String });
                        for (int i = 0; i < returnMessages.Count; i++)
                        {
                            if (Convert.ToInt16(returnMessages[i][0]) == 0)
                            {
                                if (returnMessages[i][1] != null)
                                {
                                    MessageHolder.AddInfoMessage((string)(returnMessages[i][1]));
                                }
                            }
                            else
                            {
                                if (returnMessages[i][1] != null)
                                {
                                    businessException.AddMessage((string)(returnMessages[i][1]));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null)
                        {
                            if (ex.InnerException.InnerException != null)
                            {
                                businessException.AddMessage(ex.InnerException.InnerException.Message);
                            }
                            else
                            {
                                businessException.AddMessage(ex.InnerException.Message);
                            }
                        }
                        else
                        {
                            businessException.AddMessage(ex.Message);
                        }
                    }
                }
            }
            else
            {
                throw new BusinessException(string.Format("有效的数据行为0，可能是模板问题"));
            }
            if (businessException.HasMessage)
            {
                throw businessException;
            }
        }
        #endregion

        #region LotNoScan
        public void LotNoScan(string opRef, string traceCode, string barCode)
        {
            try
            {
                User user = SecurityContextHolder.Get();
                this.genericMgr.FindAllWithNativeSql("exec USP_Busi_LotNoScan ?,?,?,?,?",
                    new object[] { opRef, traceCode, barCode, user.Id, user.FullName },
                    new IType[] { NHibernateUtil.String, NHibernateUtil.String, NHibernateUtil.String, NHibernateUtil.Int32, NHibernateUtil.String });
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

        [Transaction(TransactionMode.Requires)]
        public void LotNoDelete(string barCode)
        {
            try
            {
                var lotNoScanByBarCode = this.genericMgr.FindAll<LotNoScan>(" select l from LotNoScan as l where l.BarCode=? ", barCode);
                if (lotNoScanByBarCode == null || lotNoScanByBarCode.Count == 0)
                {
                    throw new BusinessException(string.Format("删除失败，此条码{0}找到扫描记录。", barCode));
                }
                else
                {
                    this.genericMgr.Delete(" delete l from LotNoScan as l where l.BarCode=? ", barCode, NHibernateUtil.String);
                    this.genericMgr.Delete(" delete l from LotNoTrace as l where l.BarCode=? ", barCode, NHibernateUtil.String);
                }
            }
            catch (BusinessException ex)
            {
                throw ex;
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

        #region 整车生产单导入
        public void GetCurrentVanOrder(string plant, string sapOrderNo, string prodlLine, string userCode)
        {
            SAPService.SAPService sapService = new SAPService.SAPService();
            sapService.GetCurrentVanOrder(plant, sapOrderNo, prodlLine, userCode);
        }
        #endregion

        #region 要货单明细关闭
        [Transaction(TransactionMode.Requires)]
        public void DeleteDetailById(int id)
        {
            OrderDetail ordDetail = this.genericMgr.FindById<OrderDetail>(id);
            OrderMaster ordMaster = this.genericMgr.FindById<OrderMaster>(ordDetail.OrderNo);
            if (ordDetail.PickedQty > 0)
            {
                throw new BusinessException(string.Format("订单号{0}，物料号{1}已经生成拣货单，不能关闭。", ordDetail.OrderNo, ordDetail.Item));
            }
            if (ordDetail.ShippedQty > ordDetail.ReceivedQty)
            {
                throw new BusinessException(string.Format("订单号{0}，物料号{1}有未收完的发货数，不能关闭。", ordDetail.OrderNo, ordDetail.Item));
            }

            User user = SecurityContextHolder.Get();
            var seqOrderChanges = this.genericMgr.FindAll<SeqOrderChange>(" select s from SeqOrderChange as s where s.OrderDetId=? ", id);
            if (seqOrderChanges == null || seqOrderChanges.Count == 0)
            {
                var newSeqOrderChange = Mapper.Map<OrderDetail, SeqOrderChange>(ordDetail);
                newSeqOrderChange.ExternalOrderNo = null;
                newSeqOrderChange.ExternalSequence = null;
                newSeqOrderChange.OrderedQty = ordDetail.OrderedQty;
                newSeqOrderChange.Status = 0;
                newSeqOrderChange.OrderDetId = ordDetail.Id;
                newSeqOrderChange.CreateDate = System.DateTime.Now;
                newSeqOrderChange.CreateUserId = user.Id;
                newSeqOrderChange.CreateUserName = user.FirstName + user.LastName;
                this.genericMgr.Create(newSeqOrderChange);
            }
            ordDetail.UnitPrice = ordDetail.OrderedQty;
            ordDetail.OrderedQty = ordDetail.ReceivedQty;
            ordDetail.ReceiveLotSize = 1;
            this.genericMgr.Update(ordDetail);
            this.genericMgr.FlushSession();
            bool allowColse = this.genericMgr.FindAllWithNativeSql<int>("select COUNT(*) as sumCount from ORD_OrderDet_2 where OrderQty>RecQty and OrderNo=?", ordMaster.OrderNo)[0] == 0;
            if (ordMaster.Status == com.Sconit.CodeMaster.OrderStatus.InProcess && allowColse)
            {
                this.CloseOrder(ordMaster, false);
            }

            var updateSeqOrderChange = Mapper.Map<OrderDetail, SeqOrderChange>(ordDetail);
            updateSeqOrderChange.ExternalOrderNo = null;
            updateSeqOrderChange.ExternalSequence = null;
            updateSeqOrderChange.Status = ordMaster.OrderStrategy == com.Sconit.CodeMaster.FlowStrategy.JIT ? (short)5 : (short)4;
            updateSeqOrderChange.OrderedQty = ordDetail.OrderedQty;
            updateSeqOrderChange.OrderDetId = ordDetail.Id;
            updateSeqOrderChange.CreateDate = System.DateTime.Now;
            updateSeqOrderChange.CreateUserId = user.Id;
            updateSeqOrderChange.CreateUserName = user.FirstName + user.LastName;
            this.genericMgr.Create(updateSeqOrderChange);
        }
        #endregion

        #region 创建拣货单
        [Transaction(TransactionMode.Requires)]
        public string[] PickShipOrder(string idStr, string qtyStr, string deliveryGroup, bool isAutoReceive)
        {
            string [] successNos=new string[2];
            try
            {
                IList<OrderDetail> orderDetailList = new List<OrderDetail>();
                if (!string.IsNullOrEmpty(idStr))
                {
                    string[] idArray = idStr.Split(',');
                    string[] qtyArray = qtyStr.Split(',');
                    string pickNo = pickListMgr.CreatePickList4Qty(deliveryGroup, idArray, qtyArray);
                    this.genericMgr.FlushSession();
                    if (!string.IsNullOrWhiteSpace(pickNo))
                    {
                        successNos[0] = pickNo;
                        if (isAutoReceive)
                        {
                            IList<PickListDetail> detailList = this.genericMgr.FindAll<PickListDetail>(" select pd from PickListDetail as pd where pd.PickListNo=? ", pickNo);
                            ReceiptMaster receiptMaster = ShipPickList(pickNo, detailList.Select(d => d.Id.ToString()).ToArray(), detailList.Select(d => d.Qty.ToString()).ToArray());
                            successNos[1] = receiptMaster.ReceiptNo;
                        }
                    }
                }
                else
                {
                    throw new BusinessException("拣货明细不能为空。");
                }

            }
            catch (BusinessException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return successNos;
        }
        #endregion

        #region 销售单手工拉料
        [Transaction(TransactionMode.Requires)]
        public string CreateDistritutionRequsiton(string idStr, DateTime WindowTime, com.Sconit.CodeMaster.OrderPriority Priority,IList<OrderDetail> details)
        {
            string orderNos=string.Empty;
            try
            {
                IList<OrderDetail> orderDetailList = new List<OrderDetail>();
                if (!string.IsNullOrEmpty(idStr))
                {
                    string[] idArray = idStr.Split(',');
                   
                    orderDetailList = (from tak in details
                                        where (from id in idArray select id).Contains(tak.Id.ToString())
                                        select tak).ToList();
                    if (orderDetailList != null && orderDetailList.Count > 0)
                    { 
                        var sumQtyDetails = (from tak in orderDetailList
                                      group tak by new
                                      {
                                          tak.Item,
                                          tak.ItemDescription,
                                          tak.ReferenceItemCode,
                                          tak.Uom,
                                          tak.UnitCount,
                                          tak.LocationFrom,
                                          tak.LocationTo,
                                          tak.Flow,
                                          tak.FlowDescription,
                                          tak.MastPartyFrom,
                                          tak.MastPartyTo,
                                          tak.Container,
                                          tak.ContainerDescription,
                                      } into result
                                      select new OrderDetail
                                      {
                                          Item = result.Key.Item,
                                          ItemDescription = result.Key.ItemDescription,
                                          ReferenceItemCode = result.Key.ReferenceItemCode,
                                          Uom = result.Key.Uom,
                                          UnitCount = result.Key.UnitCount,
                                          MinUnitCount = result.Key.UnitCount,
                                          LocationFrom = result.Key.LocationFrom,
                                          LocationTo = result.Key.LocationTo,
                                          Flow = result.Key.Flow,
                                          FlowDescription = result.Key.FlowDescription,
                                          MastPartyFrom = result.Key.MastPartyFrom,
                                          MastPartyTo = result.Key.MastPartyTo,
                                          Container = result.Key.Container,
                                          ContainerDescription = result.Key.ContainerDescription,
                                          OrderedQty=result.Sum(r=>r.OrderedQty),
                                      }).ToList();

                        var groups = (from tak in sumQtyDetails
                                      group tak by new
                                      {
                                          tak.Flow,
                                          tak.FlowDescription,
                                          tak.MastPartyFrom,
                                          tak.MastPartyTo,
                                      } into result
                                      select new
                                      {
                                          Flow = result.Key.Flow,
                                          FlowDescription = result.Key.FlowDescription,
                                          MastPartyFrom = result.Key.MastPartyFrom,
                                          MastPartyTo = result.Key.MastPartyTo,
                                          Details = result.ToList()
                                      }).ToList();

                        var effectiveDate = System.DateTime.Now;
                        
                        foreach (var group in groups)
                        {
                            OrderMaster newOrder = this.TransferFlow2Order(this.genericMgr.FindById<FlowMaster>(group.Flow), null, effectiveDate, false);
                            newOrder.IsQuick = false;
                            newOrder.IsShipByOrder = false;
                            newOrder.WindowTime = WindowTime;
                            newOrder.StartTime = effectiveDate;
                            newOrder.Priority = Priority;
                            newOrder.OrderStrategy = com.Sconit.CodeMaster.FlowStrategy.SPARTPART;
                            newOrder.OrderDetails = group.Details;
                            newOrder.IsAutoRelease = true;
                            this.CreateOrder(newOrder);
                            orderNos += newOrder.OrderNo+"*";
                        }
                        IList<DistributionRequisition> distributionRequisitions = Mapper.Map<IList<OrderDetail>, IList<DistributionRequisition>>(orderDetailList);
                        foreach (var distributionRequisition in distributionRequisitions)
                        {
                            distributionRequisition.Id = 0;
                            this.genericMgr.Create(distributionRequisition);
                        }
                    }
                }
                else
                {
                    throw new BusinessException("拉料明细不能为空。");
                }

            }
            catch (BusinessException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return orderNos;
        }
        #endregion

        #region 00:02 转换时间
        public DateTime ParseDateTime(string time)
        {
            if (string.IsNullOrWhiteSpace(time))
                return DateTime.MinValue;

            var t = time.Split(':');
            if (t.Length != 2)
                return DateTime.MinValue;

            int h;
            int.TryParse(t[0], out h);
            if (h >= 24)
                return DateTime.MinValue;

            int m;
            int.TryParse(t[1], out m);

            if (h > 59)
                return DateTime.MinValue;

            return new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, System.DateTime.Now.Day, h, m, 0);
        }
        #endregion
        #endregion

        #region private methods
        private void GenerateOrderOperation(OrderDetail orderDetail, OrderMaster orderMaster)
        {
            string routingCode = !string.IsNullOrWhiteSpace(orderDetail.Routing) ? orderDetail.Routing : orderMaster.Routing;
            if (!string.IsNullOrWhiteSpace(routingCode))
            {
                RoutingMaster routing = this.genericMgr.FindById<RoutingMaster>(routingCode);
                IList<RoutingDetail> routingDetailList = routingMgr.GetRoutingDetails(routingCode, orderMaster.StartTime);

                if (routingDetailList != null && routingDetailList.Count() > 0)
                {
                    IList<OrderOperation> orderOperationList = (from det in routingDetailList
                                                                select new OrderOperation
                                                                {
                                                                    Operation = det.Operation,
                                                                    OpReference = det.OpReference,
                                                                    //LeadTime = det.LeadTime > 0 ? det.LeadTime : routing.TaktTime,   //默认取工序上的提前期，如果为0择取工艺流程上的节拍时间
                                                                    //TimeUnit = det.LeadTime > 0 ? det.TimeUnit : routing.TaktTimeUnit,
                                                                    Location = det.Location
                                                                }).OrderBy(det => det.Operation).ThenBy(det => det.OpReference).ToList();

                    orderDetail.OrderOperations = orderOperationList;
                }
                else
                {
                    throw new BusinessException(Resources.PRD.Routing.Errors_RoutingDetailNotFound, routingCode);
                }
            }
        }

        private void GenerateOrderBomDetail(OrderDetail orderDetail, OrderMaster orderMaster)
        {
            #region 查找成品单位和Bom单位的转换关系
            //把OrderDetail的收货单位和单位用量转换为BOM单位和单位用量
            //fgUom，fgUnityQty代表接收一个orderDetail.Uom单位(等于订单的收货单位)的FG，等于单位(fgUom)有多少(fgUnityQty)值
            string fgUom = orderDetail.Uom;
            decimal fgUnityQty = 1;
            //如果和Bom上的单位不一致，转化为Bom上的单位，不然会导致物料回冲不正确。  

            //查找Bom
            BomMaster orderDetailBom = FindOrderDetailBom(orderDetail);
            #region 判断Bom是否有效
            if (!orderDetailBom.IsActive)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_BomInActive, orderDetail.Bom);
            }
            #endregion

            //订单单位和Bom单位不一致，需要做单位转换
            if (string.Compare(orderDetail.Uom, orderDetailBom.Uom) != 0)
            {
                fgUom = orderDetailBom.Uom;
                fgUnityQty = itemMgr.ConvertItemUomQty(orderDetail.Item, orderDetail.Uom, fgUnityQty, fgUom);
            }
            #endregion

            #region 创建OrderBomDetail
            Item fgItem = genericMgr.FindById<Item>(orderDetail.Item);

            #region 查询Bom明细
            IList<BomDetail> bomDetailList = bomMgr.GetFlatBomDetail(orderDetail.Bom, orderMaster.StartTime);
            #endregion

            #region 查询Bom Item
            IList<Item> bomItemList = null;
            if (bomDetailList != null && bomDetailList.Count > 0)
            {
                string hql = string.Empty;
                IList<object> para = new List<object>();
                foreach (BomDetail bomDetail in bomDetailList)
                {
                    if (hql == string.Empty)
                    {
                        hql = "from Item where Code in(?";
                    }
                    else
                    {
                        hql += ",?";
                    }
                    para.Add(bomDetail.Item);
                }
                hql += ")";

                bomItemList = this.genericMgr.FindAll<Item>(hql, para.ToArray());
            }
            #endregion

            //#region 查询工艺流程明细
            //IList<RoutingDetail> routingDetailList = null;
            //if (!string.IsNullOrEmpty(orderDetail.Routing))
            //{
            //    RoutingMaster routing = this.genericMgr.FindById<RoutingMaster>(orderDetail.Routing);
            //    if (!routing.IsActive)
            //    {
            //        throw new BusinessErrorException(Resources.ORD.OrderMaster.Errors_RoutingInActive, orderDetail.Routing);
            //    }
            //    routingDetailList = routingMgr.GetRoutingDetails(orderDetail.Routing, orderMaster.StartTime);
            //}
            //#endregion

            #region 查询生产防错明细 SIH客户化是从零件追溯表中取需要扫描的零件。
            //string itemTraceHql = "select it.Item From ItemTrace as it";
            //IList<string> itemTraceList = this.genericMgr.FindAll<string>(itemTraceHql);
            //IList<ProductionScanDetail> productionScanDetailList = null;
            //if (!string.IsNullOrEmpty(orderDetail.ProductionScan))
            //{
            //    ProductionScanMaster productionScanMaster = this.genericMgr.FindById<ProductionScanMaster>(orderDetail.ProductionScan);
            //    if (!productionScanMaster.IsActive)
            //    {
            //        throw new BusinessErrorException(Resources.ORD.OrderMaster.Errors_ScanInActive, orderDetail.ProductionScan);
            //    }

            //    DetachedCriteria criteria = DetachedCriteria.For<ProductionScanDetail>();
            //    criteria.Add(Expression.Eq("ProductionScan", orderDetail.ProductionScan));
            //    productionScanDetailList = this.genericMgr.FindAll<ProductionScanDetail>(criteria);
            //}
            #endregion

            foreach (BomDetail bomDetail in bomDetailList)
            {
                #region 查找物料的来源库位和提前期
                string bomLocFrom = string.Empty;
                //来源库位查找逻辑RoutingDetail-->OrderDetail-->Order-->BomDetail 
                //工序的优先级最大，因为同一个OrderMaster可以有不同的工艺流程，其次OrderMaster，最后BomDetail
                TryLoadOrderOperations(orderDetail);
                if (orderDetail.OrderOperations != null && orderDetail.OrderOperations.Count > 0)
                {
                    //取RoutingDetail上的
                    OrderOperation orderOperation = orderDetail.OrderOperations.Where(
                            p => p.Operation == bomDetail.Operation
                            && p.OpReference == bomDetail.OpReference).SingleOrDefault();

                    if (orderOperation != null)
                    {
                        bomLocFrom = orderOperation.Location;
                    }
                }

                if (string.IsNullOrEmpty(bomLocFrom))
                {
                    //在取OrderDetail上，然后是OrderHead上取
                    //取默认库位FlowDetail-->Flow
                    if (!string.IsNullOrEmpty(orderDetail.LocationFrom))
                    {
                        bomLocFrom = orderDetail.LocationFrom;
                    }
                    else
                    {
                        bomLocFrom = orderMaster.LocationFrom;
                    }
                }

                if (string.IsNullOrEmpty(bomLocFrom))
                {
                    //最后取BomDetail上的Location
                    bomLocFrom = bomDetail.Location;
                }
                #endregion

                #region 创建生产单物料明细
                OrderBomDetail orderBomDetail = new OrderBomDetail();

                Item bomItem = bomItemList.Where(i => i.Code == bomDetail.Item).Single();

                orderBomDetail.Bom = bomDetail.Bom;
                orderBomDetail.Item = bomDetail.Item;
                orderBomDetail.ItemDescription = bomItem.Description;
                orderBomDetail.Uom = bomDetail.Uom;   //Bom单位
                //todo 检查Bom Operation和Routing Operation 不匹配的情况       
                orderBomDetail.Operation = bomDetail.Operation;
                orderBomDetail.OpReference = bomDetail.OpReference;
                //可以考虑增加库存单位的转换系数
                orderBomDetail.BomUnitQty = bomDetail.CalculatedQty * fgUnityQty; //单位成品（订单单位），需要消耗物料数量（Bom单位）。
                orderBomDetail.OrderedQty = orderDetail.OrderedQty * orderBomDetail.BomUnitQty;
                orderBomDetail.Location = bomLocFrom;
                orderBomDetail.IsPrint = bomDetail.IsPrint;
                //orderBomDetail.IsScanHu = itemTraceList.Contains(orderBomDetail.Item);   //生产防错标记
                orderBomDetail.IsScanHu = false;
                orderBomDetail.BackFlushMethod = bomDetail.BackFlushMethod;
                orderBomDetail.FeedMethod = bomDetail.FeedMethod;
                orderBomDetail.IsAutoFeed = bomDetail.IsAutoFeed;
                //orderBomDetail.BackFlushInShortHandle = bomDetail.BackFlushInShortHandle;
                orderBomDetail.EstimateConsumeTime = orderMaster.StartTime;

                //BomDetail的基本单位
                orderBomDetail.BaseUom = bomItem.Uom;
                if (orderBomDetail.BaseUom != orderBomDetail.Uom)
                {
                    orderBomDetail.UnitQty = this.itemMgr.ConvertItemUomQty(orderBomDetail.Item, orderBomDetail.Uom, 1, orderBomDetail.BaseUom);
                }
                else
                {
                    orderBomDetail.UnitQty = 1;
                }

                orderDetail.AddOrderBomDetail(orderBomDetail);
                #endregion

                #region 查找零件消耗提前期，累加所有工序小于等于当前工序的提前期
                if (orderDetail.OrderOperations != null)
                {
                    IList<OrderOperation> orderOperationList = orderDetail.OrderOperations.Where(
                               p => p.Operation < bomDetail.Operation
                        //每道工序对应一个工位，不考虑一道工序多工位的情况
                        //|| (p.Operation == bomDetail.Operation              //同道工序多工位的情况
                        //&& string.Compare(p.OpReference, bomDetail.OpReference) <= 0)
                               ).ToList();

                    if (orderOperationList != null && orderOperationList.Count > 0)
                    {
                        foreach (OrderOperation orderOperation in orderOperationList)
                        {
                            switch (orderOperation.TimeUnit)
                            {
                                case com.Sconit.CodeMaster.TimeUnit.Day:
                                    orderBomDetail.EstimateConsumeTime = orderBomDetail.EstimateConsumeTime.Add(TimeSpan.FromDays(orderOperation.LeadTime));
                                    break;
                                case com.Sconit.CodeMaster.TimeUnit.Hour:
                                    orderBomDetail.EstimateConsumeTime = orderBomDetail.EstimateConsumeTime.Add(TimeSpan.FromHours(orderOperation.LeadTime));
                                    break;
                                case com.Sconit.CodeMaster.TimeUnit.Minute:
                                    orderBomDetail.EstimateConsumeTime = orderBomDetail.EstimateConsumeTime.Add(TimeSpan.FromMinutes(orderOperation.LeadTime));
                                    break;
                                case com.Sconit.CodeMaster.TimeUnit.Second:
                                    orderBomDetail.EstimateConsumeTime = orderBomDetail.EstimateConsumeTime.Add(TimeSpan.FromSeconds(orderOperation.LeadTime));
                                    break;
                            };
                        }
                    }
                }
                #endregion

                #region 更新生产防错标记
                //if (productionScanDetailList != null && productionScanDetailList.Count > 0)
                //{
                //    ProductionScanDetail productionScanDetail = productionScanDetailList.Where(
                //           p => p.Operation == orderBomDetail.Operation
                //               && p.OpReference == orderBomDetail.OpReference
                //               && p.Item == orderBomDetail.Item).SingleOrDefault();

                //    if (productionScanDetail != null)
                //    {
                //        orderBomDetail.IsScanHu = true;
                //    }
                //    else
                //    {
                //        orderBomDetail.IsScanHu = false;
                //    }
                //}
                #endregion
            }

            #region 返工，把自己添加到物料中。
            //回用不需要，回用产出的负数，投入也是负数。
            //为了投入产出报表数据合理
            if (orderMaster.SubType == com.Sconit.CodeMaster.OrderSubType.Return)
            {
                OrderBomDetail orderBomDetail = new OrderBomDetail();

                orderBomDetail.Item = orderDetail.Item;
                orderBomDetail.ItemDescription = orderDetail.ItemDescription;
                orderBomDetail.Uom = orderDetail.Uom;
                #region 取工序，先取RoutingDetail上最小工序，如果没有取BomDetail的最小工序
                if (orderDetail.OrderOperations != null && orderDetail.OrderOperations.Count() > 0)
                {
                    //先根据工序排序，在根据工位排序
                    OrderOperation orderOperation = orderDetail.OrderOperations
                        .OrderBy(op => op.Operation)
                        .ThenBy(op => op.OpReference)
                        .First();

                    orderBomDetail.Operation = orderOperation.Operation;
                    orderBomDetail.OpReference = orderOperation.OpReference;
                    //orderBomDetail.OpReference = string.Empty;
                }
                else
                {
                    BomDetail bomDetail = bomDetailList.OrderBy(det => det.Operation).ThenBy(det => det.OpReference).First();

                    orderBomDetail.Operation = bomDetail.Operation;
                    orderBomDetail.OpReference = bomDetail.OpReference;
                }
                #endregion
                orderBomDetail.BomUnitQty = 1;
                orderBomDetail.OrderedQty = orderDetail.OrderedQty;
                orderBomDetail.Location = !string.IsNullOrWhiteSpace(orderDetail.LocationTo) ? orderDetail.LocationTo : orderMaster.LocationTo;
                orderBomDetail.IsPrint = false;
                orderBomDetail.IsScanHu = orderMaster.IsReceiveScanHu || orderMaster.CreateHuOption == com.Sconit.CodeMaster.CreateHuOption.Receive;  //收获扫描Hu或者收货时创建条码，返工时要扫描成品条码
                orderBomDetail.BackFlushMethod = com.Sconit.CodeMaster.BackFlushMethod.GoodsReceive;
                orderBomDetail.FeedMethod = com.Sconit.CodeMaster.FeedMethod.None;
                orderBomDetail.IsAutoFeed = false;
                //orderBomDetail.BackFlushInShortHandle = BomDetail.BackFlushInShortHandleEnum.Nothing;
                orderBomDetail.EstimateConsumeTime = orderMaster.StartTime;   //预计消耗时间等于开始时间

                orderDetail.AddOrderBomDetail(orderBomDetail);
            }
            #endregion
            #endregion
        }

        private BomMaster FindOrderDetailBom(OrderDetail orderDetail)
        {
            //Bom的选取顺序orderDetail.Bom(Copy from 路线明细) --> orderDetail.Item.Bom--> 用orderDetail.Item.Code作为BomCode
            if (string.IsNullOrWhiteSpace(orderDetail.Bom))
            {
                orderDetail.Bom = bomMgr.FindItemBom(orderDetail.Item);
            }

            try
            {
                BomMaster bom = genericMgr.FindById<BomMaster>(orderDetail.Bom);
                orderDetail.Bom = bom.Code;
                return bom;
            }
            catch (ObjectNotFoundException)
            {
                throw new BusinessException(Resources.PRD.Bom.Errors_ItemBomNotFound, orderDetail.Item);
            }
        }

        private void CheckOrderedQtyFulfillment(OrderMaster orderMaster, OrderDetail orderDetail)
        {
            if (orderMaster.IsOrderFulfillUC
                && !(orderMaster.IsAutoRelease && orderMaster.IsAutoStart) //快速的不考虑
                && orderMaster.SubType == com.Sconit.CodeMaster.OrderSubType.Normal)  //只考虑正常订单，退货/返工等不考虑
            {
                if (orderDetail.OrderedQty % orderDetail.UnitCount != 0)
                {
                    throw new BusinessException(Resources.ORD.OrderMaster.Errors_OrderQtyNotFulfillUnitCount, orderDetail.Item, orderDetail.UnitCount.ToString());
                }
            }
        }

        private IList<OrderDetail> ProcessNewOrderDetail(OrderDetail orderDetail, OrderMaster orderMaster, ref int seq)
        {
            IList<OrderDetail> activeOrderDetails = new List<OrderDetail>();

            if (orderDetail.OrderedQty != 0) //过滤数量为0的明细
            {
                #region 整包校验
                CheckOrderedQtyFulfillment(orderMaster, orderDetail);
                #endregion

                Item item = orderDetail.CurrentItem != null ? orderDetail.CurrentItem : genericMgr.FindById<Item>(orderDetail.Item);

                if (item.IsKit && false)  //暂时不支持套件
                {
                    #region 分解套件
                    //没有考虑套件下面还是套件的情况
                    IList<ItemKit> itemKitList = itemMgr.GetKitItemChildren(item.Code);

                    if (itemKitList != null && itemKitList.Count() > 0)
                    {
                        foreach (ItemKit kit in itemKitList)
                        {
                            //检查订单明细的零件类型
                            CheckOrderDetailItemType(kit.ChildItem, (com.Sconit.CodeMaster.OrderType)orderMaster.Type);

                            OrderDetail activeOrderDetail = new OrderDetail();
                            activeOrderDetail.OrderType = orderMaster.Type;
                            activeOrderDetail.OrderSubType = orderMaster.SubType;
                            activeOrderDetail.Sequence = ++seq;
                            activeOrderDetail.Item = kit.ChildItem.Code;
                            activeOrderDetail.ItemDescription = kit.ChildItem.Description;
                            activeOrderDetail.Uom = orderDetail.Uom;
                            activeOrderDetail.BaseUom = kit.ChildItem.Uom;
                            activeOrderDetail.UnitCount = orderDetail.UnitCount;
                            activeOrderDetail.RequiredQty = orderDetail.RequiredQty * kit.Qty;
                            activeOrderDetail.OrderedQty = orderDetail.OrderedQty * kit.Qty;
                            if (activeOrderDetail.Uom != kit.ChildItem.Uom)
                            {
                                activeOrderDetail.UnitQty = kit.Qty;
                            }
                            else
                            {
                                activeOrderDetail.UnitQty = itemMgr.ConvertItemUomQty(kit.ChildItem.Code, kit.ChildItem.Uom, kit.Qty, activeOrderDetail.Uom);
                            }
                            activeOrderDetail.ReceiveLotSize = orderDetail.ReceiveLotSize * kit.Qty;
                            activeOrderDetail.LocationFrom = orderDetail.LocationFrom;
                            activeOrderDetail.LocationFromName = orderDetail.LocationFromName;
                            activeOrderDetail.LocationTo = orderDetail.LocationTo;
                            activeOrderDetail.LocationToName = orderDetail.LocationToName;
                            activeOrderDetail.IsInspect = orderDetail.IsInspect;
                            //activeOrderDetail.InspectLocation = orderDetail.InspectLocation;
                            //activeOrderDetail.InspectLocationName = orderDetail.InspectLocationName;
                            //activeOrderDetail.RejectLocation = orderDetail.RejectLocation;
                            //activeOrderDetail.RejectLocationName = orderDetail.RejectLocationName;
                            activeOrderDetail.BillAddress = orderDetail.BillAddress;
                            activeOrderDetail.BillAddressDescription = orderDetail.BillAddressDescription;
                            activeOrderDetail.PriceList = orderDetail.PriceList;
                            activeOrderDetail.Routing = activeOrderDetail.Routing;
                            //activeOrderDetail.HuLotSize = activeOrderDetail.HuLotSize * kit.Qty;
                            activeOrderDetail.BillTerm = activeOrderDetail.BillTerm;

                            activeOrderDetails.Add(activeOrderDetail);
                        }
                    }
                    else
                    {
                        throw new BusinessException(Resources.MD.Item.Errors_ItemKit_ChildrenItemNotFound, orderDetail.Item);
                    }
                    #endregion
                }
                else
                {
                    orderDetail.Sequence = ++seq;
                    orderDetail.OrderType = orderMaster.Type;
                    orderDetail.OrderSubType = orderMaster.SubType;
                    orderDetail.BaseUom = item.Uom;

                    #region 零件类型校验
                    CheckOrderDetailItemType(item, (com.Sconit.CodeMaster.OrderType)orderMaster.Type);
                    activeOrderDetails.Add(orderDetail);
                    #endregion

                    #region 设置和库存单位的转换
                    if (string.Compare(orderDetail.Uom, item.Uom) != 0)
                    {
                        orderDetail.UnitQty = itemMgr.ConvertItemUomQty(orderDetail.Item, orderDetail.Uom, 1, item.Uom);
                    }
                    else
                    {
                        orderDetail.UnitQty = 1;
                    }
                    #endregion
                }
            }

            return activeOrderDetails;
        }

        private void CheckOrderDetailItemType(Item item, com.Sconit.CodeMaster.OrderType orderType)
        {
            #region 零件类型校验
            if (!item.IsActive)
            {
                //零件不启用
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_ItemInActive, item.Code);
            }
            //暂时不启用零件校验 
            else if (true)
            {
            }
            else if (item.IsVirtual)
            {
                //虚零件
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_ItemIsVirtual, item.Code);
            }
            else if (orderType == com.Sconit.CodeMaster.OrderType.CustomerGoods && !item.IsCustomerGoods)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_ItemNotCustomerGoods, item.Code);
            }
            else if (orderType == com.Sconit.CodeMaster.OrderType.Production && !item.IsManufacture)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_ItemNotManufacture, item.Code);
            }
            else if ((orderType == com.Sconit.CodeMaster.OrderType.Procurement
                || orderType == com.Sconit.CodeMaster.OrderType.ScheduleLine) && !item.IsPurchase)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_ItemNotPurchase, item.Code);
            }
            else if (orderType == com.Sconit.CodeMaster.OrderType.Distribution && !item.IsSales)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_ItemNotSales, item.Code);
            }
            else if (orderType == com.Sconit.CodeMaster.OrderType.SubContract && !item.IsSubContract)
            {
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_ItemNotSubContract, item.Code);
            }
            #endregion
        }

        private void CalculateOrderDetailPrice(OrderDetail orderDetail, OrderMaster orderMaster, DateTime? effectiveDate)
        {
            string priceList = !string.IsNullOrWhiteSpace(orderDetail.PriceList) ? orderDetail.PriceList : orderMaster.PriceList;

            if (string.IsNullOrWhiteSpace(priceList))
            {
                throw new BusinessException("没有指定价格单。");
            }

            #region 币种
            PriceListMaster priceListMaster = orderDetail.CurrentPriceListMaster != null ? orderDetail.CurrentPriceListMaster : orderMaster.CurrentPriceListMaster;
            if (priceListMaster == null)
            {
                if (!string.IsNullOrWhiteSpace(orderDetail.PriceList))
                {
                    orderDetail.CurrentPriceListMaster = this.genericMgr.FindById<PriceListMaster>(orderDetail.PriceList);
                    priceListMaster = orderDetail.CurrentPriceListMaster;
                }
                else if (!string.IsNullOrWhiteSpace(orderMaster.PriceList))
                {
                    orderMaster.CurrentPriceListMaster = this.genericMgr.FindById<PriceListMaster>(orderMaster.PriceList);
                    priceListMaster = orderMaster.CurrentPriceListMaster;
                }
            }

            orderDetail.Currency = priceListMaster.Currency;
            #endregion

            #region 价格
            PriceListDetail priceListDetail = itemMgr.GetItemPrice(orderDetail.Item, orderDetail.Uom, priceList, orderMaster.Currency, effectiveDate);
            if (priceListDetail != null)
            {
                orderDetail.UnitPrice = priceListDetail.UnitPrice;
                orderDetail.IsProvisionalEstimate = priceListDetail.IsProvisionalEstimate;
                orderDetail.Tax = priceListDetail.PriceList.Tax;
                orderDetail.IsIncludeTax = priceListDetail.PriceList.IsIncludeTax;
            }
            #endregion
        }

        //private void CreateOrderOperation(OrderDetail orderDetail)
        //{
        //    if (orderDetail.OrderOperations != null && orderDetail.OrderOperations.Count() > 0)
        //    {
        //        foreach (OrderOperation orderOperation in orderDetail.OrderOperations)
        //        {
        //            orderOperation.OrderDetailId = orderDetail.Id;
        //            orderOperation.OrderNo = orderDetail.OrderNo;

        //            genericMgr.Create(orderOperation);
        //        }
        //    }
        //}

        //private void CreateOrderBomDetail(OrderDetail orderDetail)
        //{
        //    if (orderDetail.OrderBomDetails != null && orderDetail.OrderBomDetails.Count() > 0)
        //    {
        //        foreach (OrderBomDetail orderBomDetail in orderDetail.OrderBomDetails)
        //        {
        //            orderBomDetail.OrderNo = orderDetail.OrderNo;
        //            orderBomDetail.OrderType = orderDetail.OrderType;
        //            orderBomDetail.OrderSubType = orderDetail.OrderSubType;
        //            orderBomDetail.OrderDetailId = orderDetail.Id;
        //            orderBomDetail.OrderDetailSequence = orderDetail.Sequence;

        //            genericMgr.Create(orderBomDetail);
        //        }
        //    }
        //}

        private void AutoReceiveIp(IpMaster ipMaster, DateTime effectiveDate)
        {
            if (ipMaster.IsAutoReceive)
            {
                foreach (IpDetail ipDetail in ipMaster.IpDetails)
                {
                    if (ipDetail.IpDetailInputs != null && ipDetail.IpDetailInputs.Count > 0)
                    {
                        foreach (IpDetailInput ipDetailInput in ipDetail.IpDetailInputs)
                        {
                            ipDetailInput.ReceiveQty = ipDetailInput.ShipQty;
                        }
                    }
                    else
                    {
                        IpDetailInput ipDetailInput = new IpDetailInput();
                        ipDetailInput.ReceiveQty = ipDetail.Qty;
                        ipDetail.AddIpDetailInput(ipDetailInput);
                    }
                }

                this.genericMgr.FlushSession();
                ReceiveIp(ipMaster.IpDetails, effectiveDate);
            }
        }

        private IList<FlowDetail> TryLoadFlowDetails(FlowMaster flowMaster)
        {
            if (!string.IsNullOrWhiteSpace(flowMaster.Code))
            {
                if (flowMaster.FlowDetails == null)
                {
                    string hql = "from FlowDetail where Flow = ? order by Sequence";

                    flowMaster.FlowDetails = this.genericMgr.FindAll<FlowDetail>(hql, flowMaster.Code);
                }

                return flowMaster.FlowDetails;
            }
            else
            {
                return null;
            }
        }

        private IList<FlowDetail> TryLoadFlowDetails(FlowMaster flowMaster, IList<string> itemCodeList)
        {
            if (!string.IsNullOrWhiteSpace(flowMaster.Code))
            {
                if (flowMaster.FlowDetails == null)
                {
                    string hql = "from FlowDetail where IsActive = 1 and Flow = ?";
                    IList<object> parm = new List<object>();
                    parm.Add(flowMaster.Code);

                    if (itemCodeList != null && itemCodeList.Count > 0 && itemCodeList.Count < 2000)
                    {
                        string whereHql = string.Empty;
                        foreach (string itemCode in itemCodeList.Distinct())
                        {
                            if (whereHql == string.Empty)
                            {
                                whereHql = " and Item in (?";
                            }
                            else
                            {
                                whereHql += ",?";
                            }
                            parm.Add(itemCode);
                        }
                        whereHql += ")";
                        hql += whereHql;
                    }

                    hql += " order by Sequence";

                    flowMaster.FlowDetails = this.genericMgr.FindAll<FlowDetail>(hql, parm.ToArray());
                    if (itemCodeList != null && itemCodeList.Count >= 2000)
                    {
                        flowMaster.FlowDetails = flowMaster.FlowDetails.Where(f => itemCodeList.Contains(f.Item)).ToList();
                    }
                }

                return flowMaster.FlowDetails;
            }
            else
            {
                return null;
            }
        }

        private IList<FlowBinding> TryLoadFlowBindings(FlowMaster flowMaster)
        {
            if (!string.IsNullOrWhiteSpace(flowMaster.Code))
            {
                if (flowMaster.FlowBindings == null)
                {
                    string hql = "from FlowBinding where MasterFlow.Code = ?";

                    flowMaster.FlowBindings = this.genericMgr.FindAll<FlowBinding>(hql, flowMaster.Code);
                }

                return flowMaster.FlowBindings;
            }
            else
            {
                return null;
            }
        }

        private IList<OrderDetail> TryLoadOrderDetails(OrderMaster orderMaster)
        {
            if (!string.IsNullOrWhiteSpace(orderMaster.OrderNo))
            {
                if (orderMaster.OrderDetails == null)
                {
                    string hql = "from OrderDetail where OrderNo = ? order by Sequence";

                    orderMaster.OrderDetails = this.genericMgr.FindAll<OrderDetail>(hql, orderMaster.OrderNo);
                }

                return orderMaster.OrderDetails;
            }
            else
            {
                return null;
            }
        }

        private IList<OrderBinding> TryLoadOrderBindings(OrderMaster orderMaster)
        {
            if (!string.IsNullOrWhiteSpace(orderMaster.OrderNo))
            {
                if (orderMaster.OrderBindings == null)
                {
                    string hql = "from OrderBinding where OrderNo = ?";

                    orderMaster.OrderBindings = this.genericMgr.FindAll<OrderBinding>(hql, orderMaster.OrderNo);
                }

                return orderMaster.OrderBindings;
            }
            else
            {
                return null;
            }
        }

        private IList<OrderOperation> TryLoadOrderOperations(OrderDetail orderDetail)
        {
            if (orderDetail.Id != 0)
            {
                if (orderDetail.OrderOperations == null)
                {
                    string hql = "from OrderOperation where OrderDetailId = ? order by Operation, OpReference";

                    orderDetail.OrderOperations = this.genericMgr.FindAll<OrderOperation>(hql, orderDetail.Id);
                }

                return orderDetail.OrderOperations;
            }
            else
            {
                return null;
            }
        }

        private IList<OrderOperation> TryLoadOrderOperations(OrderMaster orderMaster)
        {
            if (orderMaster.OrderNo != null)
            {
                TryLoadOrderDetails(orderMaster);

                IList<OrderOperation> orderOperationList = new List<OrderOperation>();

                string hql = string.Empty;
                IList<object> para = new List<object>();
                foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                {
                    if (orderDetail.OrderOperations != null && orderDetail.OrderOperations.Count > 0)
                    {
                        ((List<OrderOperation>)orderOperationList).AddRange(orderDetail.OrderOperations);
                    }
                    else
                    {
                        if (hql == string.Empty)
                        {
                            hql = "from OrderOperation where OrderDetailId in (?";
                        }
                        else
                        {
                            hql += ",?";
                        }
                        para.Add(orderDetail.Id);
                    }
                }

                if (hql != string.Empty)
                {
                    hql += ") order by OrderDetailId, Operation, OpReference";

                    ((List<OrderOperation>)orderOperationList).AddRange(this.genericMgr.FindAll<OrderOperation>(hql, para.ToArray()));
                }

                foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                {
                    if (orderDetail.OrderOperations == null || orderDetail.OrderOperations.Count == 0)
                    {
                        orderDetail.OrderOperations = orderOperationList.Where(o => o.OrderDetailId == orderDetail.Id).ToList();
                    }
                }

                return orderOperationList;
            }
            else
            {
                return null;
            }
        }

        private IList<OrderBomDetail> TryLoadOrderBomDetails(OrderMaster orderMaster)
        {
            if (orderMaster.OrderNo != null)
            {
                TryLoadOrderDetails(orderMaster);

                IList<OrderBomDetail> orderBomDetailList = new List<OrderBomDetail>();

                string hql = string.Empty;
                IList<object> para = new List<object>();
                foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                {
                    if (orderDetail.OrderBomDetails != null && orderDetail.OrderBomDetails.Count > 0)
                    {
                        ((List<OrderBomDetail>)orderBomDetailList).AddRange(orderDetail.OrderBomDetails);
                    }
                    else
                    {
                        if (hql == string.Empty)
                        {
                            hql = "from OrderBomDetail where OrderDetailId in (?";
                        }
                        else
                        {
                            hql += ",?";
                        }
                        para.Add(orderDetail.Id);
                    }
                }

                if (hql != string.Empty)
                {
                    hql += ") order by OrderDetailId, Operation, OpReference";

                    ((List<OrderBomDetail>)orderBomDetailList).AddRange(this.genericMgr.FindAll<OrderBomDetail>(hql, para.ToArray()));
                }

                foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                {
                    if (orderDetail.OrderBomDetails == null || orderDetail.OrderBomDetails.Count == 0)
                    {
                        orderDetail.OrderBomDetails = orderBomDetailList.Where(o => o.OrderDetailId == orderDetail.Id).ToList();
                    }
                }

                return orderBomDetailList;
            }
            else
            {
                return null;
            }
        }

        private IList<OrderBomDetail> TryLoadOrderBomDetails(OrderDetail orderDetail)
        {
            if (orderDetail.Id != 0)
            {
                if (orderDetail.OrderBomDetails == null)
                {
                    string hql = "from OrderBomDetail where OrderDetailId = ? order by Operation, OpReference";

                    orderDetail.OrderBomDetails = this.genericMgr.FindAll<OrderBomDetail>(hql, orderDetail.Id);
                }

                return orderDetail.OrderBomDetails;
            }
            else
            {
                return null;
            }
        }

        private IList<OrderMaster> LoadOrderMasters(string[] orderNoList)
        {
            IList<object> para = new List<object>();
            string selectOrderMasterStatement = string.Empty;
            foreach (string orderNo in orderNoList)
            {
                if (selectOrderMasterStatement == string.Empty)
                {
                    selectOrderMasterStatement = "from OrderMaster where OrderNo in (?";
                }
                else
                {
                    selectOrderMasterStatement += ",?";
                }
                para.Add(orderNo);
            }
            selectOrderMasterStatement += ")";

            return this.genericMgr.FindAll<OrderMaster>(selectOrderMasterStatement, para.ToArray());
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

        private IList<OrderDetail> LoadExceptOrderDetail(OrderMaster orderMaster)
        {
            if (orderMaster.OrderDetails != null && orderMaster.OrderDetails.Count > 0)
            {
                string hql = string.Empty;
                IList<object> paras = new List<object>();
                foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                {
                    if (hql == string.Empty)
                    {
                        hql = "from OrderDetail where OrderNo = ? and Id not in (?";
                        paras.Add(orderMaster.OrderNo);
                    }
                    else
                    {
                        hql += ",?";
                    }
                    paras.Add(orderDetail.Id);
                }
                hql += ")";
                return this.genericMgr.FindAll<OrderDetail>(hql, paras.ToArray());
            }
            else
            {
                return this.TryLoadOrderDetails(orderMaster);
            }
        }

        private IList<IpDetail> LoadExceptIpDetails(string ipNo, int[] ipDetIdList)
        {
            IList<object> para = new List<object>();
            para.Add(ipNo);
            para.Add(false);   //未关闭的
            para.Add(com.Sconit.CodeMaster.IpDetailType.Normal);    //过滤掉差异明细，其实没有必要，因为一次性收货的送货单，在第一次收货时还没有生成差异明细
            string selectExceptIpDetailStatement = string.Empty;
            foreach (int id in ipDetIdList)
            {
                if (selectExceptIpDetailStatement == string.Empty)
                {
                    selectExceptIpDetailStatement = "from IpDetail where IpNo = ? and IsClose = ? and Type = ? and Id not in (?";
                }
                else
                {
                    selectExceptIpDetailStatement += ",?";
                }

                para.Add(id);
            }
            selectExceptIpDetailStatement += ")";

            return this.genericMgr.FindAll<IpDetail>(selectExceptIpDetailStatement, para.ToArray());
        }

        private IList<IpLocationDetail> LoadIpLocationDetails(int[] ipDetIdList)
        {
            IList<object> para = new List<object>();
            string selectIpLocationDetailStatement = string.Empty;
            foreach (int id in ipDetIdList)
            {
                if (selectIpLocationDetailStatement == string.Empty)
                {
                    selectIpLocationDetailStatement = "from IpLocationDetail where IpDetailId in (?";
                }
                else
                {
                    selectIpLocationDetailStatement += ",?";
                }
                para.Add(id);
            }
            selectIpLocationDetailStatement += ")";

            return this.genericMgr.FindAll<IpLocationDetail>(selectIpLocationDetailStatement, para.ToArray());
        }

        private IList<IpLocationDetail> LoadExceptIpLocationDetails(string ipNo, int[] ipDetIdList)
        {
            IList<object> para = new List<object>();
            para.Add(ipNo);   //未关闭的
            para.Add(false);   //未关闭的
            string selectExceptIpLocationDetailStatement = string.Empty;
            foreach (int id in ipDetIdList)
            {
                if (selectExceptIpLocationDetailStatement == string.Empty)
                {
                    selectExceptIpLocationDetailStatement = "from IpLocationDetail where IpNo = ? and IsClose = ? and IpDetailId not in (?";
                }
                else
                {
                    selectExceptIpLocationDetailStatement += ",?";
                }

                para.Add(id);
            }
            selectExceptIpLocationDetailStatement += ")";

            return this.genericMgr.FindAll<IpLocationDetail>(selectExceptIpLocationDetailStatement, para.ToArray());
        }

        private IList<PickListDetail> TryLoadPickListDetails(PickListMaster pickListMaster)
        {
            if (!string.IsNullOrWhiteSpace(pickListMaster.PickListNo))
            {
                if (pickListMaster.PickListDetails == null)
                {
                    string hql = "from PickListDetail where PickListNo = ?";

                    pickListMaster.PickListDetails = this.genericMgr.FindAll<PickListDetail>(hql, pickListMaster.PickListNo);
                }

                return pickListMaster.PickListDetails;
            }
            else
            {
                return null;
            }
        }

        private IList<PickListResult> TryLoadPickListResults(PickListMaster pickListMaster)
        {
            if (!string.IsNullOrWhiteSpace(pickListMaster.PickListNo))
            {
                if (pickListMaster.PickListResults == null)
                {
                    string hql = "from PickListResult where PickListNo = ?";

                    pickListMaster.PickListResults = this.genericMgr.FindAll<PickListResult>(hql, pickListMaster.PickListNo);
                }

                return pickListMaster.PickListResults;
            }
            else
            {
                return null;
            }
        }

        private IList<OrderDetail> TryLoadOrderDetails(PickListMaster pickListMaster)
        {
            if (pickListMaster.OrderDetails == null)
            {
                if (pickListMaster.PickListDetails == null)
                {
                    TryLoadPickListDetails(pickListMaster);
                }

                #region 获取订单明细
                string hql = string.Empty;
                IList<object> para = new List<object>();
                foreach (int orderDetailId in pickListMaster.PickListDetails.Select(p => p.OrderDetailId).Distinct())
                {
                    if (hql == string.Empty)
                    {
                        hql = "from OrderDetail where Id in (?";
                    }
                    else
                    {
                        hql += ",?";
                    }

                    para.Add(orderDetailId);
                }
                hql += ")";

                pickListMaster.OrderDetails = this.genericMgr.FindAll<OrderDetail>(hql, para.ToArray());
                #endregion
            }

            return pickListMaster.OrderDetails;
        }

        private IList<FlowMaster> LoadFlowMaster(IList<OrderBinding> orderBindingList, DateTime effectiveDate)
        {
            if (orderBindingList != null && orderBindingList.Count > 0)
            {
                string selectFlowStatement = string.Empty;
                string selectFlowDetailStatement = string.Empty;
                IList<object> selectFlowPara = new List<object>();
                IList<object> selectFlowDetailPara = new List<object>();
                foreach (string flowCode in orderBindingList.Select(o => o.BindFlow).Distinct())
                {
                    if (selectFlowStatement == string.Empty)
                    {
                        selectFlowStatement = "from FlowMaster where Code in (?";
                        selectFlowDetailStatement = "from FlowDetail where (StartDate is null or StartDate <= ?) and (EndDate is null or EndDate >= ?) and Flow in (?";
                        selectFlowDetailPara.Add(effectiveDate);
                        selectFlowDetailPara.Add(effectiveDate);
                    }
                    else
                    {
                        selectFlowStatement += ",?";
                        selectFlowDetailStatement += ",?";
                    }

                    selectFlowPara.Add(flowCode);
                    selectFlowDetailPara.Add(flowCode);
                }

                selectFlowStatement += ")";
                selectFlowDetailStatement += ")";


                IList<FlowMaster> flowMasterList = this.genericMgr.FindAll<FlowMaster>(selectFlowStatement, selectFlowPara.ToArray());
                IList<FlowDetail> flowDetailList = this.genericMgr.FindAll<FlowDetail>(selectFlowDetailStatement, selectFlowDetailPara.ToArray());

                foreach (FlowMaster flowMaster in flowMasterList)
                {
                    flowMaster.FlowDetails = flowDetailList.Where(f => f.Flow == flowMaster.Code).ToList();
                }

                return flowMasterList;
            }

            return null;
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
        #endregion
    }
}

