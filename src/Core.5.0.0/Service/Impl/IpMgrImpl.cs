using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Castle.Services.Transaction;
using com.Sconit.Entity;
using com.Sconit.Entity.CUST;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.INV;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.SCM;
using com.Sconit.PrintModel.ORD;
using com.Sconit.Utility;
using com.Sconit.Entity.MD;

namespace com.Sconit.Service.Impl
{
    [Transactional]
    public class IpMgrImpl : BaseMgr, IIpMgr
    {
        #region 变量
        private IPublishing proxy;
        public IPubSubMgr pubSubMgr { get; set; }
        public IGenericMgr genericMgr { get; set; }
        public INumberControlMgr numberControlMgr { get; set; }
        public ILocationDetailMgr locationDetailMgr { get; set; }
        public ISystemMgr systemMgr { get; set; }
        public IVehicleInFactoryMgrImpl vehicleInFactoryMgr { get; set; }
        public IHuMgr huMgr { get; set; }
        public IOrderMgr orderMgr { get; set; }
        #endregion

        public IpMaster TransferFlow2Ip(FlowMaster flowMaster, IList<OrderDetail> orderDetailList)
        {
            IpMaster ipMaster = Mapper.Map<FlowMaster, IpMaster>(flowMaster);
            ipMaster.OrderType = CodeMaster.OrderType.ScheduleLine;
            ipMaster.OrderSubType = CodeMaster.OrderSubType.Normal;
            ipMaster.QualityType = CodeMaster.QualityType.Qualified;
            ipMaster.Status = CodeMaster.IpStatus.Submit;

            #region 查找IpMaster相关对象
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

            #region 查找Address
            #region 收集所有地址代码
            IList<string> addressCodeList = new List<string>();

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
            #endregion

            #region PartyFrom和PartyTo赋值
            ipMaster.PartyFromName = partyList.Where(p => p.Code == ipMaster.PartyFrom).Single().Name;
            ipMaster.PartyToName = partyList.Where(p => p.Code == ipMaster.PartyTo).Single().Name;
            #endregion

            #region ShipFrom赋值
            if (!string.IsNullOrEmpty(flowMaster.ShipFrom))
            {
                Address shipFrom = addressList.Where(a => a.Code == flowMaster.ShipFrom).Single();
                ipMaster.ShipFromAddress = shipFrom.AddressContent;
                ipMaster.ShipFromCell = shipFrom.MobilePhone;
                ipMaster.ShipFromTel = shipFrom.TelPhone;
                ipMaster.ShipFromFax = shipFrom.Fax;
                ipMaster.ShipFromContact = shipFrom.ContactPersonName;
            }
            #endregion

            #region ShipTo赋值
            if (!string.IsNullOrEmpty(flowMaster.ShipTo))
            {
                Address shipTo = addressList.Where(a => a.Code == flowMaster.ShipTo).Single();
                ipMaster.ShipToAddress = shipTo.AddressContent;
                ipMaster.ShipToCell = shipTo.MobilePhone;
                ipMaster.ShipToTel = shipTo.TelPhone;
                ipMaster.ShipToFax = shipTo.Fax;
                ipMaster.ShipToContact = shipTo.ContactPersonName;
            }
            #endregion

            #region 状态
            ipMaster.Status = com.Sconit.CodeMaster.IpStatus.Submit;
            #endregion

            #region 发出时间
            ipMaster.DepartTime = DateTime.Now;
            #endregion

            #region 到达时间
            ipMaster.ArriveTime = DateTime.Now;
            #endregion

            #region 发货单明细
            foreach (OrderDetail orderDetail in orderDetailList)
            {
                OrderMaster orderMaster = orderDetail.CurrentOrderMaster;
                FlowDetail flowDetail = flowMaster.FlowDetails.Where(det => det.Item == orderDetail.Item).Single();
                IpDetail ipDetail = Mapper.Map<FlowDetail, IpDetail>(flowDetail);
                ipDetail.Type = CodeMaster.IpDetailType.Normal;
                ipDetail.OrderNo = orderDetail.OrderNo;
                ipDetail.OrderType = orderDetail.OrderType;
                ipDetail.OrderSubType = orderDetail.OrderSubType;
                ipDetail.OrderDetailId = orderDetail.Id;
                ipDetail.OrderDetailSequence = orderDetail.Sequence;
                ipDetail.ItemDescription = orderDetail.ItemDescription;
                ipDetail.ReferenceItemCode = orderDetail.ReferenceItemCode;
                ipDetail.ExternalOrderNo = orderDetail.ExternalOrderNo;
                ipDetail.ExternalSequence = orderDetail.ExternalSequence;
                if (ipDetail.LocationFrom == null)
                {
                    ipDetail.LocationFrom = flowMaster.LocationFrom;
                }

                if (ipDetail.LocationFrom != null)
                {
                    ipDetail.LocationFromName = locationList.Where(l => l.Code == ipDetail.LocationFrom).Single().Name;
                }

                if (ipDetail.LocationTo == null)
                {
                    ipDetail.LocationTo = flowMaster.LocationTo;
                }

                if (ipDetail.LocationTo != null)
                {
                    ipDetail.LocationToName = locationList.Where(l => l.Code == ipDetail.LocationTo).Single().Name;
                }

                if (orderDetail.StartDate.HasValue)
                {
                    ipDetail.StartTime = orderDetail.StartDate;
                }
                else
                {
                    ipDetail.StartTime = orderMaster.StartTime;
                }
                if (orderDetail.EndDate.HasValue)
                {
                    ipDetail.WindowTime = orderDetail.EndDate;
                }
                else
                {
                    ipDetail.WindowTime = orderMaster.WindowTime;
                }
                ipDetail.Flow = flowMaster.Code;

                //记录sap采购类型
                ipDetail.PSTYP = orderDetail.AUFNR;

                //ZC1：军车    ZC2：出口车     ZC3：特殊车     ZC5：CKD
                ipDetail.Tax = orderDetail.Tax;

                //ipDetail.EffectiveDate = orderMaster.EffectiveDate;
                ipDetail.IsInspect = orderMaster.IsInspect;

                ipDetail.UnitQty = orderDetail.UnitQty;
                //2013-9-22,单位换算出错，此处要去订单中的单位
                ipDetail.Uom = orderDetail.Uom;

                ipDetail.BillTerm = orderDetail.BillTerm;

                ipDetail.BillAddress = orderDetail.BillAddress;

                foreach (OrderDetailInput orderDetailInput in orderDetail.OrderDetailInputs)
                {
                    IpDetailInput ipDetailInput = new IpDetailInput();
                    ipDetailInput.ShipQty = orderDetailInput.ShipQty;
                    ipDetailInput.WMSIpSeq = orderDetailInput.WMSIpSeq;
                    ipDetailInput.ConsignmentParty = orderDetailInput.ConsignmentParty;
                    if (string.IsNullOrWhiteSpace(ipDetailInput.ConsignmentParty))
                    {
                        //如果orderDetailInput没有指定寄售供应商，取orderDetail上的指定寄售供应商
                        ipDetailInput.ConsignmentParty = orderDetail.ICHARG;
                    }
                    ipDetailInput.OccupyType = orderDetailInput.OccupyType;
                    ipDetailInput.OccupyReferenceNo = orderDetailInput.OccupyReferenceNo;

                    ipDetail.AddIpDetailInput(ipDetailInput);
                }

                //计划协议发货检验整包装
                if (orderMaster.IsReceiveFulfillUC && ipDetail.ShipQtyInput % ipDetail.UnitCount != 0)
                {
                    //不是整包装
                    throw new BusinessException(Resources.ORD.OrderMaster.Errors_ShipQtyNotFulfillUnitCount, orderDetail.Item, orderDetail.UnitCount.ToString("0.##"));
                }

                ipMaster.AddIpDetail(ipDetail);
            }
            #endregion

            return ipMaster;
        }

        public IpMaster TransferOrder2Ip(IList<OrderMaster> orderMasterList)
        {
            #region 发货单头
            IpMaster ipMaster = MergeOrderMaster2IpMaster(orderMasterList);
            string WMSNo = string.Empty;
            foreach (OrderMaster orderMaster in orderMasterList)
            {
                foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                {
                    if (orderDetail.OrderDetailInputs.Select(i => i.WMSIpNo).Distinct().Count() > 1)
                    {
                        throw new TechnicalException("WMS发货单号不一致。");
                    }

                    if (string.IsNullOrWhiteSpace(WMSNo))
                    {
                        WMSNo = orderDetail.OrderDetailInputs.First().WMSIpNo;
                    }
                    else if (WMSNo != orderDetail.OrderDetailInputs.First().WMSIpNo)
                    {
                        throw new TechnicalException("WMS发货单号不一致。");
                    }
                }
            }
            ipMaster.WMSNo = WMSNo;
            #endregion

            #region 发货单明细
            foreach (OrderMaster orderMaster in orderMasterList)
            {
                if (orderMaster.Type != CodeMaster.OrderType.ScheduleLine)
                {
                    #region 非计划协议
                    if (orderMaster.OrderDetails != null && orderMaster.OrderDetails.Count > 0)
                    {
                        foreach (OrderDetail orderDetail in orderMaster.OrderDetails)
                        {
                            IpDetail ipDetail = Mapper.Map<OrderDetail, IpDetail>(orderDetail);
                            ipDetail.Flow = orderMaster.Flow;
                            //针对同一个订单明细转成送货单明细，其移动类型默认只有一个
                            //不支持如下情况：订单需求100，本次发货50，其中40个移动类型为311，还有10个为411k
                            ipDetail.BWART = orderDetail.OrderDetailInputs[0].MoveType;

                            #region 采购送货单明细需要合同号。
                            //if (orderMaster.Type == CodeMaster.OrderType.Procurement
                            //    || orderMaster.Type == CodeMaster.OrderType.ScheduleLine)
                            //{
                            //    ipDetail.ExternalOrderNo = orderMaster.ExternalOrderNo;
                            //}
                            ipDetail.ExternalOrderNo = orderDetail.ExternalOrderNo;
                            ipDetail.ExternalSequence = orderDetail.ExternalSequence;
                            #endregion

                            //ipDetail.EffectiveDate = orderMaster.EffectiveDate;
                            ipDetail.IsInspect = orderMaster.IsInspect && ipDetail.IsInspect; //头和明细都选择报验才报验
                            if (orderDetail.StartDate.HasValue)
                            {
                                ipDetail.StartTime = orderDetail.StartDate;
                            }
                            else
                            {
                                ipDetail.StartTime = orderMaster.StartTime;
                            }
                            if (orderDetail.EndDate.HasValue)
                            {
                                ipDetail.WindowTime = orderDetail.EndDate;
                            }
                            else
                            {
                                ipDetail.WindowTime = orderMaster.WindowTime;
                            }
                            foreach (OrderDetailInput orderDetailInput in orderDetail.OrderDetailInputs)
                            {
                                IpDetailInput ipDetailInput = new IpDetailInput();
                                ipDetailInput.ShipQty = orderDetailInput.ShipQty;
                                if (orderMaster.IsShipScanHu)
                                {
                                    ipDetailInput.HuId = orderDetailInput.HuId;
                                    ipDetailInput.LotNo = orderDetailInput.LotNo;
                                }
                                ipDetailInput.WMSIpSeq = orderDetailInput.WMSIpSeq;
                                ipDetailInput.WMSRecNo = WMSNo;//把安吉的wmsId记录到收货单头的WMSNo字段
                                ipDetailInput.ConsignmentParty = orderDetailInput.ConsignmentParty;
                                if (string.IsNullOrWhiteSpace(ipDetailInput.ConsignmentParty))
                                {
                                    //如果orderDetailInput没有指定寄售供应商，取orderDetail上的指定寄售供应商
                                    ipDetailInput.ConsignmentParty = orderDetail.ICHARG;
                                }
                                ipDetailInput.OccupyType = orderDetailInput.OccupyType;
                                ipDetailInput.OccupyReferenceNo = orderDetailInput.OccupyReferenceNo;

                                ipDetail.AddIpDetailInput(ipDetailInput);
                            }

                            ipMaster.AddIpDetail(ipDetail);
                        }
                    }
                    #endregion
                }
                else
                {
                    #region 计划协议
                    if (orderMaster.OrderDetails != null && orderMaster.OrderDetails.Count > 0)
                    {
                        var groupedOrderDetailList = from det in orderMaster.OrderDetails
                                                     group det by
                                                     new
                                                     {
                                                         Item = det.Item,
                                                         EBELN = det.ExternalOrderNo,
                                                         EBELP = det.ExternalSequence.Split('-')[0],
                                                     } into gj
                                                     select new
                                                     {
                                                         Item = gj.Key.Item,
                                                         EBELN = gj.Key.EBELN,
                                                         EBELP = gj.Key.EBELP,
                                                         List = gj.ToList(),
                                                     };

                        foreach (var groupdOrderDetail in groupedOrderDetailList)
                        {
                            OrderDetail orderDetail = groupdOrderDetail.List.First();

                            IpDetail ipDetail = Mapper.Map<OrderDetail, IpDetail>(orderDetail);
                            ipDetail.Flow = orderMaster.Flow;
                            ipDetail.ExternalOrderNo = groupdOrderDetail.EBELN;
                            ipDetail.ExternalSequence = groupdOrderDetail.EBELP;
                            //记录sap采购类型
                            ipDetail.PSTYP = orderDetail.AUFNR;

                            //ipDetail.EffectiveDate = orderMaster.EffectiveDate;
                            ipDetail.IsInspect = orderMaster.IsInspect && ipDetail.IsInspect; //头和明细都选择报验才报验
                            if (orderDetail.StartDate.HasValue)
                            {
                                ipDetail.StartTime = orderDetail.StartDate;
                            }
                            else
                            {
                                ipDetail.StartTime = orderMaster.StartTime;
                            }
                            if (orderDetail.EndDate.HasValue)
                            {
                                ipDetail.WindowTime = orderDetail.EndDate;
                            }
                            else
                            {
                                ipDetail.WindowTime = orderMaster.WindowTime;
                            }

                            foreach (OrderDetail eachOrderDetail in groupdOrderDetail.List)
                            {
                                foreach (OrderDetailInput orderDetailInput in eachOrderDetail.OrderDetailInputs)
                                {
                                    IpDetailInput ipDetailInput = new IpDetailInput();
                                    ipDetailInput.ShipQty = orderDetailInput.ShipQty;
                                    if (orderMaster.IsShipScanHu)
                                    {
                                        ipDetailInput.HuId = orderDetailInput.HuId;
                                        ipDetailInput.LotNo = orderDetailInput.LotNo;
                                    }
                                    ipDetailInput.WMSIpSeq = orderDetailInput.WMSIpSeq;
                                    ipDetailInput.WMSRecNo = WMSNo;//把安吉的wmsId记录到收货单头的WMSNo字段
                                    ipDetailInput.ConsignmentParty = orderDetailInput.ConsignmentParty;
                                    if (string.IsNullOrWhiteSpace(ipDetailInput.ConsignmentParty))
                                    {
                                        //如果orderDetailInput没有指定寄售供应商，取orderDetail上的指定寄售供应商
                                        ipDetailInput.ConsignmentParty = eachOrderDetail.ICHARG;
                                    }
                                    ipDetailInput.OccupyType = orderDetailInput.OccupyType;
                                    ipDetailInput.OccupyReferenceNo = orderDetailInput.OccupyReferenceNo;

                                    ipDetail.AddIpDetailInput(ipDetailInput);
                                }
                            }

                            //计划协议发货检验整包装
                            if (orderMaster.IsReceiveFulfillUC && ipDetail.ShipQtyInput % ipDetail.UnitCount != 0)
                            {
                                //不是整包装
                                throw new BusinessException(Resources.ORD.OrderMaster.Errors_ShipQtyNotFulfillUnitCount, orderDetail.Item, orderDetail.UnitCount.ToString("0.##"));
                            }

                            ipMaster.AddIpDetail(ipDetail);
                        }
                    }
                    #endregion
                }
            }
            #endregion

            return ipMaster;
        }

        public IpMaster MergeOrderMaster2IpMaster(IList<OrderMaster> orderMasterList)
        {
            IpMaster ipMaster = new IpMaster();

            #region 路线代码
            //var flow = from om in orderMasterList select om.Flow;
            //if (flow.Distinct().Count() > 1)
            //{
            //    throw new BusinessException("路线代码不同不能合并发货。");
            //}
            //ipMaster.Flow = flow.Distinct().Single();
            ipMaster.Flow = (orderMasterList.OrderBy(om => om.Flow).Select(om => om.Flow)).First();
            #endregion

            #region 发货单类型
            ipMaster.Type = com.Sconit.CodeMaster.IpType.Normal;
            #endregion

            #region 订单类型
            var orderType = from om in orderMasterList select om.Type;
            if (orderType.Distinct().Count() > 1)
            {
                throw new BusinessException("订单类型不同不能合并发货。");
            }
            ipMaster.OrderType = orderType.Distinct().Single();
            #endregion

            #region 订单类型
            var orderSubType = from om in orderMasterList select om.SubType;
            if (orderSubType.Distinct().Count() > 1)
            {
                throw new BusinessException("订单子类型不同不能合并发货。");
            }
            ipMaster.OrderSubType = orderSubType.Distinct().Single();
            #endregion

            #region 订单质量类型
            var qualityType = from om in orderMasterList select om.QualityType;
            if (qualityType.Distinct().Count() > 1)
            {
                throw new BusinessException("订单质量状态不同不能合并发货。");
            }
            ipMaster.QualityType = qualityType.Distinct().Single();
            #endregion

            #region 状态
            ipMaster.Status = com.Sconit.CodeMaster.IpStatus.Submit;
            #endregion

            #region 发出时间
            ipMaster.DepartTime = (from om in orderMasterList select om.StartTime).Min();
            #endregion

            #region 到达时间
            ipMaster.ArriveTime = (from om in orderMasterList select om.WindowTime).Min();
            #endregion

            #region PartyFrom
            var partyFrom = from om in orderMasterList select om.PartyFrom;
            if (partyFrom.Distinct().Count() > 1)
            {
                throw new BusinessException("来源组织不同不能合并发货。");
            }
            ipMaster.PartyFrom = partyFrom.Distinct().Single();
            #endregion

            #region PartyFromName
            ipMaster.PartyFromName = (from om in orderMasterList select om.PartyFromName).First();
            #endregion

            #region PartyTo
            var partyTo = from om in orderMasterList select om.PartyTo;
            if (partyTo.Distinct().Count() > 1)
            {
                throw new BusinessException("目的组织不同不能合并发货。");
            }
            ipMaster.PartyTo = partyTo.Distinct().Single();
            #endregion

            #region PartyToName
            ipMaster.PartyToName = (from om in orderMasterList select om.PartyToName).First();
            #endregion

            #region ShipFrom
            var shipFrom = from om in orderMasterList select om.ShipFrom;
            if (shipFrom.Distinct().Count() > 1)
            {
                throw new BusinessException("发货地址不同不能合并发货。");
            }
            ipMaster.ShipFrom = shipFrom.Distinct().Single();
            #endregion

            #region ShipFromAddr
            ipMaster.ShipFromAddress = (from om in orderMasterList select om.ShipFromAddress).First();
            #endregion

            #region ShipFromTel
            ipMaster.ShipFromTel = (from om in orderMasterList select om.ShipFromTel).First();
            #endregion

            #region ShipFromCell
            ipMaster.ShipFromCell = (from om in orderMasterList select om.ShipFromCell).First();
            #endregion

            #region ShipFromFax
            ipMaster.ShipFromFax = (from om in orderMasterList select om.ShipFromFax).First();
            #endregion

            #region ShipFromContact
            ipMaster.ShipFromContact = (from om in orderMasterList select om.ShipFromContact).First();
            #endregion

            #region ShipTo
            var shipTo = from om in orderMasterList select om.ShipTo;
            if (shipTo.Distinct().Count() > 1)
            {
                throw new BusinessException("收货地址不同不能合并发货。");
            }
            ipMaster.ShipTo = shipTo.Distinct().Single();
            #endregion

            #region ShipToAddr
            ipMaster.ShipToAddress = (from om in orderMasterList select om.ShipToAddress).First();
            #endregion

            #region ShipToTel
            ipMaster.ShipToTel = (from om in orderMasterList select om.ShipToTel).First();
            #endregion

            #region ShipToCell
            ipMaster.ShipToCell = (from om in orderMasterList select om.ShipToCell).First();
            #endregion

            #region ShipToFax
            ipMaster.ShipToFax = (from om in orderMasterList select om.ShipToFax).First();
            #endregion

            #region ShipToContact
            ipMaster.ShipToContact = (from om in orderMasterList select om.ShipToContact).First();
            #endregion

            #region Dock
            var dock = from om in orderMasterList select om.Dock;
            if (dock.Distinct().Count() > 1)
            {
                throw new BusinessException("道口不同不能合并发货。");
            }
            ipMaster.Dock = dock.Distinct().Single();
            #endregion

            #region IsAutoReceive
            var isAutoReceive = from om in orderMasterList select om.IsAutoReceive;
            if (isAutoReceive.Distinct().Count() > 1)
            {
                throw new BusinessException("自动收货选项不同不能合并发货。");
            }
            ipMaster.IsAutoReceive = isAutoReceive.Distinct().Single();
            #endregion

            #region IsShipScanHu
            var isShipScanHu = from om in orderMasterList select om.IsShipScanHu;
            if (isShipScanHu.Distinct().Count() > 1)
            {
                throw new BusinessException("发货扫描条码选项不同不能合并发货。");
            }
            ipMaster.IsShipScanHu = isShipScanHu.Distinct().Single();
            #endregion

            #region IsRecScanHu
            var isRecScanHu = from om in orderMasterList select om.IsReceiveScanHu;
            if (isRecScanHu.Distinct().Count() > 1)
            {
                throw new BusinessException("收货扫描条码选项不同不能合并发货。");
            }
            ipMaster.IsReceiveScanHu = isRecScanHu.Distinct().Single();
            #endregion

            #region IsPrintAsn
            ipMaster.IsPrintAsn = orderMasterList.Where(om => om.IsPrintAsn == true) != null;
            #endregion

            #region IsAsnPrinted
            ipMaster.IsAsnPrinted = false;
            #endregion

            #region IsPrintRec
            ipMaster.IsPrintReceipt = orderMasterList.Where(om => om.IsPrintReceipt == true) != null;
            #endregion

            #region IsRecExceed
            var isRecExceed = from om in orderMasterList select om.IsReceiveExceed;
            if (isRecExceed.Distinct().Count() > 1)
            {
                throw new BusinessException("允许超收选项不同不能合并发货。");
            }
            ipMaster.IsReceiveExceed = isRecExceed.Distinct().Single();
            #endregion

            #region IsRecFulfillUC
            var isRecFulfillUC = from om in orderMasterList select om.IsReceiveFulfillUC;
            if (isRecFulfillUC.Distinct().Count() > 1)
            {
                throw new BusinessException("收货满足包装选项不同不能合并发货。");
            }
            ipMaster.IsReceiveFulfillUC = isRecFulfillUC.Distinct().Single();
            #endregion

            #region IsRecFifo
            var isRecFifo = from om in orderMasterList select om.IsReceiveFifo;
            if (isRecFifo.Distinct().Count() > 1)
            {
                throw new BusinessException("收货先进先出选项不同不能合并发货。");
            }
            ipMaster.IsReceiveFifo = isRecFifo.Distinct().Single();
            #endregion

            #region IsAsnUniqueRec
            var isAsnUniqueRec = from om in orderMasterList select om.IsAsnUniqueReceive;
            if (isAsnUniqueRec.Distinct().Count() > 1)
            {
                throw new BusinessException("ASN一次性收货选项不同不能合并发货。");
            }
            ipMaster.IsAsnUniqueReceive = isAsnUniqueRec.Distinct().Single();
            #endregion

            #region IsRecCreateHu
            var createHuOption = (from om in orderMasterList
                                  select om.CreateHuOption).Distinct();
            if (createHuOption != null && createHuOption.Count() > 1)
            {
                throw new BusinessException("创建条码选项不同不能合并发货。");
            }
            ipMaster.CreateHuOption = createHuOption.Single();
            #endregion

            #region IsCheckPartyFromAuth
            ipMaster.IsCheckPartyFromAuthority = orderMasterList.Where(om => om.IsCheckPartyFromAuthority == true) != null;
            #endregion

            #region IsCheckPartyToAuth
            ipMaster.IsCheckPartyToAuthority = orderMasterList.Where(om => om.IsCheckPartyToAuthority == true) != null;
            #endregion

            #region RecGapTo
            var recGapTo = from om in orderMasterList select om.ReceiveGapTo;
            if (recGapTo.Distinct().Count() > 1)
            {
                throw new BusinessException("收货差异调整选项不同不能合并发货。");
            }
            ipMaster.ReceiveGapTo = recGapTo.Distinct().Single();
            #endregion

            #region AsnTemplate
            var asnTemplate = orderMasterList.Select(om => om.AsnTemplate).First();
            ipMaster.AsnTemplate = asnTemplate;
            #endregion

            #region RecTemplate
            var recTemplate = orderMasterList.Select(om => om.ReceiptTemplate).First();
            ipMaster.ReceiptTemplate = recTemplate;
            #endregion

            #region HuTemplate
            var huTemplate = orderMasterList.Select(om => om.HuTemplate).First();
            ipMaster.HuTemplate = huTemplate;
            #endregion

            return ipMaster;
        }

        public IpMaster TransferPickList2Ip(PickListMaster pickListMaster)
        {
            #region 发货单头
            IpMaster ipMaster = Mapper.Map<PickListMaster, IpMaster>(pickListMaster);
            ipMaster.IsShipScanHu = true;
            ipMaster.Status = CodeMaster.IpStatus.Submit;
            #endregion

            #region 发货单明细
            IList<OrderMaster> orderMasterList = this.LoadOrderMasters(pickListMaster.OrderDetails.Select(det => det.OrderNo).ToArray());
            foreach (OrderDetail orderDetail in pickListMaster.OrderDetails)
            {
                IList<PickListResult> pickListResultList = pickListMaster.PickListResults.Where(p => p.OrderDetailId == orderDetail.Id).ToList();
                if (pickListResultList.Count > 0)
                {
                    IpDetail ipDetail = Mapper.Map<OrderDetail, IpDetail>(orderDetail);
                    ipDetail.Flow = orderMasterList.Where(mstr => mstr.OrderNo == orderDetail.OrderNo).Single().Flow;

                    //ipDetail.EffectiveDate = orderMaster.EffectiveDate;
                    PickListDetail pickListDetail = pickListMaster.PickListDetails.Where(p => p.OrderDetailId == orderDetail.Id).First();
                    ipDetail.IsInspect = pickListDetail.IsInspect;
                    ipDetail.StartTime = pickListDetail.StartTime;
                    ipDetail.WindowTime = pickListDetail.WindowTime;

                    foreach (PickListResult pickListResult in pickListResultList)
                    {
                        IpDetailInput ipDetailInput = new IpDetailInput();
                        ipDetailInput.ShipQty = pickListResult.Qty;
                        ipDetailInput.HuId = pickListResult.HuId;
                        ipDetailInput.LotNo = pickListResult.LotNo;
                        ipDetailInput.OccupyType = CodeMaster.OccupyType.Pick;
                        ipDetailInput.OccupyReferenceNo = pickListMaster.PickListNo;

                        ipDetail.AddIpDetailInput(ipDetailInput);
                    }

                    ipMaster.AddIpDetail(ipDetail);
                }
            }
            #endregion

            return ipMaster;
        }

        [Transaction(TransactionMode.Requires)]
        public void CreateIp(IpMaster ipMaster)
        {
            CreateIp(ipMaster, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public void CreateIp(IpMaster ipMaster, DateTime effectiveDate)
        {
            #region 发货明细不能为空
            if (ipMaster.IpDetails == null || ipMaster.IpDetails.Count == 0)
            {
                throw new BusinessException(Resources.ORD.IpMaster.Errors_IpDetailIsEmpty);
            }
            #endregion

            #region 保存发货单头
            ipMaster.IpNo = numberControlMgr.GetIpNo(ipMaster);
            ipMaster.EffectiveDate = effectiveDate;
            this.genericMgr.Create(ipMaster);
            #endregion

            //#region 按订单明细汇总发货数，按条码发货一条订单明细会对应多条发货记录
            //var summaryIpDet = from det in ipMaster.IpDetails
            //                   group det by det.OrderDetailId into g           
            //                   select new
            //                   {
            //                       OrderDetailId = g.Key,
            //                       List = g.ToList()
            //                   };
            //#endregion

            #region 保存发货单明细
            int seqCount = 1;
            foreach (IpDetail ipDetail in ipMaster.IpDetails.OrderBy(i => i.OrderNo).ThenBy(i => i.OrderDetailSequence))
            {
                ipDetail.Qty = ipDetail.ShipQtyInput;
                ipDetail.Sequence = seqCount++;
                ipDetail.IpNo = ipMaster.IpNo;

                this.genericMgr.Create(ipDetail);
            }
            #endregion

            #region 发货创建条码
            if ((ipMaster.OrderType == CodeMaster.OrderType.Procurement
                || ipMaster.OrderType == CodeMaster.OrderType.CustomerGoods)
                && ipMaster.CreateHuOption == CodeMaster.CreateHuOption.Ship)
            {
                foreach (IpDetail ipDetail in ipMaster.IpDetails)
                {
                    IList<IpDetail> ipDetailList = new List<IpDetail>();
                    ipDetail.ManufactureParty = ipMaster.PartyFrom;
                    ipDetail.HuQty = ipDetail.ShipQtyInput;
                    ipDetail.LotNo = LotNoHelper.GenerateLotNo();
                    ipDetailList.Add(ipDetail);
                    IList<Hu> huList = huMgr.CreateHu(ipMaster, ipDetailList);

                    ipDetail.IpDetailInputs = (from hu in huList
                                               select new IpDetailInput
                                               {
                                                   ShipQty = hu.Qty,    //订单单位
                                                   HuId = hu.HuId,
                                                   LotNo = hu.LotNo,
                                                   IsCreatePlanBill = false,
                                                   IsConsignment = false,
                                                   PlanBill = null,
                                                   ActingBill = null,
                                                   IsFreeze = false,
                                                   IsATP = true,
                                                   OccupyType = com.Sconit.CodeMaster.OccupyType.None,
                                                   OccupyReferenceNo = null
                                               }).ToList();
                }
            }
            #endregion

            #region 销售出库的需要先检查库存（寄售+非寄售）是否满足，再非寄售数是否满足，不满足，否则拆分IpDetInput
            if (ipMaster.OrderType == CodeMaster.OrderType.Distribution)
            {
                foreach (var ipDetail in ipMaster.IpDetails)
                {
                    var csIpDetailInputs = new List<IpDetailInput>();
                    foreach (var ipDetailInput in ipDetail.IpDetailInputs)
                    {
                        var locationLotDetailList = genericMgr.FindAllWithNamedQuery<LocationLotDetail>("USP_Busi_GetAvailableInventory", new Object[] { ipDetail.LocationFrom, ipDetail.Item });
                        if (ipDetailInput.ShipQty <= locationLotDetailList.Sum(o => o.Qty))
                        {
                            //非寄售数
                            var nCSQty = locationLotDetailList.Where(o => o.IsConsignment == false).Sum(o => o.Qty);
                            if (ipDetailInput.ShipQty > nCSQty)
                            {
                                //发货数大于非寄售数量的，需要根据先进先出取寄售库存
                                var csQty = ipDetailInput.ShipQty - nCSQty;
                                foreach (var locationLotDetail in locationLotDetailList.Where(o=>o.IsConsignment == true).OrderBy(o=>o.LotNo).OrderBy(o=>o.Qty))
                                {
                                    if (csQty - locationLotDetail.Qty > 0)
                                    {
                                        IpDetailInput csIpDetInput = new IpDetailInput();
                                        //csIpDetInput = ipDetailInput;
                                        csIpDetInput.ShipQty = locationLotDetail.Qty;
                                        csIpDetInput.PlanBill = locationLotDetail.PlanBill;
                                        csIpDetInput.WMSIpSeq = ipDetailInput.WMSIpSeq;
                                        csIpDetInput.WMSRecNo = ipDetailInput.WMSRecNo;
                                        csIpDetInput.IsConsignment = true;
                                        csIpDetInput.OccupyType = ipDetailInput.OccupyType;
                                        csIpDetInput.OccupyReferenceNo = ipDetailInput.OccupyReferenceNo;
                                        csIpDetailInputs.Add(csIpDetInput);
                                        csQty = csQty - locationLotDetail.Qty;
                                    }
                                    else
                                    {
                                        IpDetailInput csIpDetInput = new IpDetailInput();
                                        //csIpDetInput = ipDetailInput;
                                        csIpDetInput.ShipQty = csQty;
                                        csIpDetInput.PlanBill = locationLotDetail.PlanBill;
                                        csIpDetInput.WMSIpSeq = ipDetailInput.WMSIpSeq;
                                        csIpDetInput.WMSRecNo = ipDetailInput.WMSRecNo;
                                        csIpDetInput.IsConsignment = true;
                                        csIpDetInput.OccupyType = ipDetailInput.OccupyType;
                                        csIpDetInput.OccupyReferenceNo = ipDetailInput.OccupyReferenceNo;
                                        csIpDetailInputs.Add(csIpDetInput);
                                        break;
                                    }
                                }
                                //先发非寄售数量
                                ipDetailInput.ShipQty = nCSQty;
                            }
                        }
                        else
                        {
                            throw new BusinessException(Resources.INV.LocationLotDetail.Errors_NotEnoughInventory, ipDetail.Item, ipDetail.LocationFrom);
                        }
                    }
                    foreach (var csIpDetailInput in csIpDetailInputs)
                    {
                        ipDetail.IpDetailInputs.Add(csIpDetailInput);
                    }
                    //ipDetail.IpDetailInputs.ToList().AddRange(csIpDetailInputs);
                }
            }
            #endregion


            #region 出库
            //条码上不带状态库位等信息，状态全部通过查找库存明细来获得。
            //暂不支持发货创建条码
            foreach (IpDetail ipDetail in ipMaster.IpDetails.OrderByDescending(det => det.ManufactureParty))
            {
                ipDetail.CurrentPartyFrom = ipMaster.PartyFrom;  //为了记录库存事务
                ipDetail.CurrentPartyFromName = ipMaster.PartyFromName;  //为了记录库存事务
                ipDetail.CurrentPartyTo = ipMaster.PartyTo;      //为了记录库存事务
                ipDetail.CurrentPartyToName = ipMaster.PartyToName;      //为了记录库存事务
                //inputIpDetail.CurrentIsATP = inputIpDetail.QualityType == com.Sconit.CodeMaster.QualityType.Qualified;
                //inputIpDetail.CurrentIsFreeze = false;              //默认只出库未冻结库存
                //ipDetail.CurrentOccupyType = com.Sconit.CodeMaster.OccupyType.None; //todo-默认出库未占用库存，除非拣货或检验的出库

                IList<InventoryTransaction> inventoryTransactionList = this.locationDetailMgr.InventoryOut(ipDetail);

                if (inventoryTransactionList != null && inventoryTransactionList.Count > 0)
                {
                    IList<IpLocationDetail> ipLocationDetailList = (from trans in inventoryTransactionList
                                                                    group trans by new
                                                                    {
                                                                        HuId = trans.HuId,
                                                                        LotNo = trans.LotNo,
                                                                        IsCreatePlanBill = trans.IsCreatePlanBill,
                                                                        IsConsignment = trans.IsConsignment,
                                                                        PlanBill = trans.PlanBill,
                                                                        ActingBill = trans.ActingBill,
                                                                        IsFreeze = trans.IsFreeze,
                                                                        IsATP = trans.IsATP,
                                                                        OccupyType = trans.OccupyType,
                                                                        OccupyReferenceNo = trans.OccupyReferenceNo,
                                                                        WMSSeq = trans.WMSIpSeq,
                                                                    } into g
                                                                    select new IpLocationDetail
                                                                    {
                                                                        Item = ipDetail.Item,
                                                                        HuId = g.Key.HuId,
                                                                        LotNo = g.Key.LotNo,
                                                                        IsCreatePlanBill = g.Key.IsCreatePlanBill,
                                                                        IsConsignment = g.Key.IsConsignment,
                                                                        PlanBill = g.Key.PlanBill,
                                                                        ActingBill = g.Key.ActingBill,
                                                                        QualityType = ipDetail.QualityType,
                                                                        IsFreeze = g.Key.IsFreeze,
                                                                        IsATP = g.Key.IsATP,
                                                                        OccupyType = g.Key.OccupyType == CodeMaster.OccupyType.Inspect ? g.Key.OccupyType : CodeMaster.OccupyType.None, //只有检验才保留占用状态
                                                                        OccupyReferenceNo = g.Key.OccupyType == CodeMaster.OccupyType.Inspect ? g.Key.OccupyReferenceNo : null,
                                                                        Qty = g.Sum(t => -t.Qty),       //出库的inventoryTrans为负数，转为ipLocationDetail需要为正数
                                                                        WMSSeq = g.Key.WMSSeq,
                                                                    }).ToList();

                    ipDetail.AddIpLocationDetail(ipLocationDetailList);
                }
            }
            #endregion

            #region 保存发货单库存明细
            foreach (IpDetail ipDetail in ipMaster.IpDetails)
            {
                //if (!string.IsNullOrWhiteSpace(ipDetail.LocationFrom))
                //{
                if (ipDetail.IpLocationDetails == null || ipDetail.IpLocationDetails.Count == 0)
                {
                    throw new TechnicalException("IpLocationDetails is empty.");
                }

                foreach (IpLocationDetail ipLocationDetail in ipDetail.IpLocationDetails)
                {
                    ipLocationDetail.IpNo = ipMaster.IpNo;
                    ipLocationDetail.IpDetailId = ipDetail.Id;
                    ipLocationDetail.OrderType = ipDetail.OrderType;
                    ipLocationDetail.OrderDetailId = ipDetail.OrderDetailId;
                    genericMgr.Create(ipLocationDetail);
                }
                //}
            }
            #endregion

            this.AsyncSendPrintData(ipMaster);
        }

        [Transaction(TransactionMode.Requires)]
        public void ManualCloseIp(string IpNo)
        {
            ManualCloseIp(this.genericMgr.FindById<IpMaster>(IpNo));
        }

        [Transaction(TransactionMode.Requires)]
        public void ManualCloseIp(IpMaster ipMaster)
        {
            #region 查找未关闭IpDetail
            IList<IpDetail> openIpDetailList = this.genericMgr.FindAll<IpDetail>(
                "from IpDetail where IpNo = ? and Type = ? and IsClose = ?",
                new Object[] { ipMaster.IpNo, CodeMaster.IpDetailType.Normal, false });
            #endregion

            #region 记录未收货差异
            if (openIpDetailList != null && openIpDetailList.Count > 0)
            {
                #region 查找未关闭ASN库存对象
                string hql = string.Empty;
                IList<object> paras = new List<object>();
                foreach (IpDetail ipDetail in openIpDetailList)
                {
                    if (hql == string.Empty)
                    {
                        hql = "from IpLocationDetail where IsClose = 0 and IpDetailId in (?";
                    }
                    else
                    {
                        hql += ", ?";

                    }
                    paras.Add(ipDetail.Id);
                }
                hql += ")";
                IList<IpLocationDetail> openIpLocationDetailList = this.genericMgr.FindAll<IpLocationDetail>(hql, paras.ToArray());
                #endregion

                #region 生成未收货差异
                IList<IpDetail> gapIpDetailList = new List<IpDetail>();
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

                #region 关闭未收货ASN明细和库存明细
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

                #region 记录收货差异
                if (gapIpDetailList != null && gapIpDetailList.Count > 0)
                {
                    foreach (IpDetail gapIpDetail in gapIpDetailList)
                    {
                        // gapIpDetail.GapReceiptNo = receiptMaster.ReceiptNo;
                        this.genericMgr.Create(gapIpDetail);

                        foreach (IpLocationDetail gapIpLocationDetail in gapIpDetail.IpLocationDetails)
                        {
                            gapIpLocationDetail.IpDetailId = gapIpDetail.Id;
                            this.genericMgr.Create(gapIpLocationDetail);
                        }
                    }
                }
                #endregion

                #region 关闭差异ASN,对于ASN多次收货的，手工关闭ASN默认按调整发货方库存处理
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

                this.orderMgr.AdjustIpGap(gapIpDetailList, CodeMaster.IpGapAdjustOption.GI);
                #endregion
                DoCloseIpMaster(ipMaster);
            }
            else
            {
                DoCloseIpMaster(ipMaster);
            }
            #endregion
        }

        [Transaction(TransactionMode.Requires)]
        public void TryCloseIp(IpMaster ipMaster)
        {
            if (ipMaster.Status == com.Sconit.CodeMaster.IpStatus.Submit
                || ipMaster.Status == com.Sconit.CodeMaster.IpStatus.InProcess)
            {
                this.genericMgr.FlushSession();
                //string hql = "update from IpMaster set Status = ?, CloseDate = ? where IpNo = ? and 0 = (select count(*) as counter from IpDetail where IpNo = ? and IsClose = ?)";

                //this.genericMgr.Update(hql, new Object[] { 
                //    (int)com.Sconit.CodeMaster.IpStatus.Close,
                //    DateTime.Now,
                //    ipMaster.IpNo,
                //    ipMaster.IpNo,
                //    false});
                string hql = "select count(*) as counter from IpDetail where IpNo = ? and IsClose = ?";
                long counter = this.genericMgr.FindAll<long>(hql, new Object[] { ipMaster.IpNo, false })[0];
                if (counter == 0)
                {
                    DoCloseIpMaster(ipMaster);
                }

                #region 关入厂证
                IList<VehicleInFactoryDetail> vehicleInFactoryDetailList = genericMgr.FindAll<VehicleInFactoryDetail>(" from VehicleInFactoryDetail as v where v.IpNo = ? and v.IsClose = ?", new object[] { ipMaster.IpNo, false });
                if (vehicleInFactoryDetailList != null && vehicleInFactoryDetailList.Count > 0)
                {
                    //理论上应该没有多个的，有也不能影响收货，多个就先关多个吧
                    foreach (VehicleInFactoryDetail vehicleInFactoryDetail in vehicleInFactoryDetailList)
                    {
                        vehicleInFactoryMgr.CloseVehicleInFactoryDetail(vehicleInFactoryDetail);
                    }
                }
                #endregion
            }
        }

        [Transaction(TransactionMode.Requires)]
        public void TryCloseExpiredScheduleLineIp(DateTime dateTime)
        {
            //获取创建日期小于指定时间，并且状态为Submit或In-Process的计划协议Ip
            IList<IpMaster> ipMasterList = genericMgr.FindAll<IpMaster>("from IpMaster as i where i.CreateDate < ? and (i.Status = ? or i.Status = ?) and i.OrderType = ?",
                new object[] { dateTime, (int)com.Sconit.CodeMaster.IpStatus.Submit, (int)com.Sconit.CodeMaster.IpStatus.InProcess, (int)com.Sconit.CodeMaster.OrderType.ScheduleLine });

            if (ipMasterList != null && ipMasterList.Count > 0)
            {

                this.genericMgr.FlushSession();

                foreach (IpMaster ipMaster in ipMasterList)
                {
                    //当未关闭且未收货的行>0时不能关闭，其余都能关闭
                    IList<IpDetail> ipDetailList = genericMgr.FindAll<IpDetail>("from IpDetail as d where d.IpNo = ?", new object[] { ipMaster.IpNo });

                    int counter = (from i in ipDetailList
                                   where i.ReceivedQty == 0 && !i.IsClose
                                   select i).Count();
                    if (counter == 0)
                    {
                        //先关闭所有的明细行
                        string updateMstr = "Update Ord_IpDet_8 set IsClose = 1,LastModifyUser = ?,LastModifyUserNm = ?,LastModifyDate = ? where IpNo = ?";
                        genericMgr.FindAllWithNativeSql(updateMstr, new object[] { SecurityContextHolder.Get().Id, SecurityContextHolder.Get().Name, DateTime.Now, ipMaster.IpNo });
                        DoCloseIpMaster(ipMaster);
                    }
                    #region 关入厂证
                    IList<VehicleInFactoryDetail> vehicleInFactoryDetailList = genericMgr.FindAll<VehicleInFactoryDetail>(" from VehicleInFactoryDetail as v where v.IpNo = ? and v.IsClose = ?", new object[] { ipMaster.IpNo, false });
                    if (vehicleInFactoryDetailList != null && vehicleInFactoryDetailList.Count > 0)
                    {
                        //理论上应该没有多个的，有也不能影响收货，多个就先关多个吧
                        foreach (VehicleInFactoryDetail vehicleInFactoryDetail in vehicleInFactoryDetailList)
                        {
                            vehicleInFactoryMgr.CloseVehicleInFactoryDetail(vehicleInFactoryDetail);
                        }
                    }
                    #endregion
                }
            }
        }

        //临时解决方案，暂时只支持计划协议asn，asn必须配置为允许多次收货
        [Transaction(TransactionMode.Requires)]
        public bool TryCloseExpiredScheduleLineIpDetail(IpDetail ipDetail)
        {

            //检查ipmstr的订单类型和允许多次收货选项
            IpMaster ipmstr = genericMgr.FindById<IpMaster>(ipDetail.IpNo);

            if (ipmstr.OrderType != CodeMaster.OrderType.ScheduleLine)
            {
                throw new BusinessException("非计划协议类送货单不能按行关闭");
            }
            else if (ipmstr.IsAsnUniqueReceive)
            {
                throw new BusinessException("ASN设置为一次收货不能按行关闭");
            }
            else if (ipmstr.Status == CodeMaster.IpStatus.Close)
            {
                throw new BusinessException("送货单已关闭");
            }
            else if (ipmstr.Status == CodeMaster.IpStatus.Cancel)
            {
                throw new BusinessException("送货单已取消");
            }
            else
            {
                ipDetail.IsClose = true;
                ipDetail.LastModifyDate = DateTime.Now;
                ipDetail.LastModifyUserId = SecurityContextHolder.Get().Id;
                ipDetail.LastModifyUserName = SecurityContextHolder.Get().Name;

                try
                {
                    genericMgr.Update(ipDetail);
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }

            }
        }

        //临时解决方案，暂时只支持计划协议asn，asn必须配置为允许多次收货
        [Transaction(TransactionMode.Requires)]
        public bool TryResumeClosedScheduleLineIpDetail(IpDetail ipDetail)
        {

            //检查ipmstr的订单类型和允许多次收货选项
            IpMaster ipmstr = genericMgr.FindById<IpMaster>(ipDetail.IpNo);

            if (ipmstr.OrderType != CodeMaster.OrderType.ScheduleLine)
            {
                throw new BusinessException("非计划协议类送货单不能按行恢复");
            }
            else if (ipmstr.IsAsnUniqueReceive)
            {
                throw new BusinessException("ASN设置为一次收货不能按行恢复");
            }
            else if (ipmstr.Status == CodeMaster.IpStatus.Close)
            {
                throw new BusinessException("送货单已关闭，不能恢复行");
            }
            else if (ipmstr.Status == CodeMaster.IpStatus.Cancel)
            {
                throw new BusinessException("送货单已取消，不能恢复行");
            }
            else if (ipDetail.ReceivedQty > 0)
            {
                throw new BusinessException("明细行有收货记录不能恢复");
            }
            else if (!ipDetail.IsClose)
            {
                throw new BusinessException("明细行并未关闭不需恢复");
            }
            else
            {
                ipDetail.IsClose = false;
                ipDetail.LastModifyDate = DateTime.Now;
                ipDetail.LastModifyUserId = SecurityContextHolder.Get().Id;
                ipDetail.LastModifyUserName = SecurityContextHolder.Get().Name;

                try
                {
                    genericMgr.Update(ipDetail);
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }

            }


        }

        [Transaction(TransactionMode.Requires)]
        public void CancelIp(string ipNo)
        {
            IpMaster ipMaster = this.genericMgr.FindById<IpMaster>(ipNo);
            CancelIp(ipMaster, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public void CancelIp(string ipNo, DateTime effectiveDate)
        {
            IpMaster ipMaster = this.genericMgr.FindById<IpMaster>(ipNo);
            CancelIp(ipMaster, effectiveDate);
        }

        [Transaction(TransactionMode.Requires)]
        public void CancelIp(IpMaster ipMaster)
        {
            CancelIp(ipMaster, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public void CancelIp(IpMaster ipMaster, DateTime effectiveDate)
        {
            #region 判断发货单状态，只有Submit，一点货都没有收的才能冲销
            if (ipMaster.Status != com.Sconit.CodeMaster.IpStatus.Submit)
            {
                throw new BusinessException("状态为{1}的送货单{0}不能冲销。", ipMaster.IpNo,
                    systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.IpStatus, ((int)ipMaster.Status).ToString()));
            }
            #endregion

            #region 获取送货单库存明细
            string selectIpLocationDetailStatement = "from IpLocationDetail where IpNo = ? and IsClose = ?";
            IList<IpLocationDetail> ipLocationDetailList = this.genericMgr.FindAll<IpLocationDetail>(selectIpLocationDetailStatement, new object[] { ipMaster.IpNo, false });
            #endregion

            #region 获取送货单明细
            if (ipMaster.IpDetails == null)
            {
                string selectIpDetailStatement = "from IpDetail where IpNo = ? and IsClose = ?";
                ipMaster.IpDetails = this.genericMgr.FindAll<IpDetail>(selectIpDetailStatement, new object[] { ipMaster.IpNo, false });
            }
            #endregion

            #region 一点货都没有收的才能冲销
            var recIpDetails = ipMaster.IpDetails.Count(i => i.ReceivedQty > 0);
            if (recIpDetails > 0)
            {
                throw new BusinessException("已收过货的送货单不能冲销。", ipMaster.IpNo,
                    systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.IpStatus, ((int)ipMaster.Status).ToString()));
            }
            #endregion

            #region 关闭送货单库存明细
            foreach (IpLocationDetail ipLocationDetail in ipLocationDetailList)
            {
                ipLocationDetail.IsClose = true;
                this.genericMgr.Update(ipLocationDetail);
            }
            #endregion

            #region 关闭送货单明细
            foreach (IpDetail ipDetail in ipMaster.IpDetails)
            {
                ipDetail.IsClose = true;
                this.genericMgr.Update(ipDetail);
            }
            #endregion

            #region 更新发货单状态
            ipMaster.Status = com.Sconit.CodeMaster.IpStatus.Cancel;
            this.genericMgr.Update(ipMaster);
            #endregion

            #region 更新订单明细
            if (ipMaster.OrderType != CodeMaster.OrderType.ScheduleLine)
            {
                #region 非计划协议
                #region 获取订单明细
                string selectOrderDetailDetailStatement = "from OrderDetail where Id in (select OrderDetailId from IpDetail where IpNo = ? and IsClose = ?)";
                IList<OrderDetail> orderDetailList = this.genericMgr.FindAll<OrderDetail>(selectOrderDetailDetailStatement, new object[] { ipMaster.IpNo, false });
                #endregion

                foreach (OrderDetail orderDetail in orderDetailList)
                {
                    #region 更新订单数量
                    orderDetail.ShippedQty -= ipMaster.IpDetails.Where(det => det.OrderDetailId == orderDetail.Id).Sum(det => det.Qty);
                    this.genericMgr.Update(orderDetail);
                    #endregion
                }
                #endregion
            }
            else
            {
                #region 计划协议
                BusinessException businessException = new BusinessException();
                foreach (IpDetail ipDetail in ipMaster.IpDetails)
                {
                    decimal remainQty = ipDetail.Qty;

                    IList<OrderDetail> scheduleOrderDetailList = this.genericMgr.FindEntityWithNativeSql<OrderDetail>("select * from ORD_OrderDet_8 where ExtNo = ? and ExtSeq = ? and ScheduleType = ? and ShipQty > RecQty order by EndDate desc",
                                                new object[] { ipDetail.ExternalOrderNo, ipDetail.ExternalSequence, CodeMaster.ScheduleType.Firm });

                    if (scheduleOrderDetailList != null && scheduleOrderDetailList.Count > 0)
                    {
                        foreach (OrderDetail scheduleOrderDetail in scheduleOrderDetailList)
                        {
                            //更新订单的发货数
                            if (remainQty > (scheduleOrderDetail.ShippedQty - scheduleOrderDetail.ReceivedQty))
                            {
                                remainQty -= (scheduleOrderDetail.ShippedQty - scheduleOrderDetail.ReceivedQty);
                                scheduleOrderDetail.ShippedQty = scheduleOrderDetail.ReceivedQty;
                            }
                            else
                            {
                                scheduleOrderDetail.ShippedQty -= remainQty;
                                remainQty = 0;
                                break;
                            }

                            this.genericMgr.Update(scheduleOrderDetail);
                        }
                    }

                    if (remainQty > 0)
                    {
                        businessException.AddMessage(Resources.ORD.IpMaster.Errors_ReceiveQtyExcceedOrderQty, ipMaster.IpNo, ipDetail.Item);
                    }
                }
                #endregion
            }
            #endregion

            this.genericMgr.FlushSession();

            #region 更新库存
            foreach (IpDetail ipDetail in ipMaster.IpDetails)
            {
                ipDetail.IsVoid = true;
                //？？？？？？？ipDetail.OrderSubType = com.Sconit.CodeMaster.OrderSubType.Return;
                var targetIpLocationDetail = from locDet in ipLocationDetailList
                                             where locDet.IpDetailId == ipDetail.Id
                                             select locDet;

                ipDetail.CurrentPartyFrom = ipMaster.PartyFrom;  //为了记录库存事务
                ipDetail.CurrentPartyFromName = ipMaster.PartyFromName;  //为了记录库存事务
                ipDetail.CurrentPartyTo = ipMaster.PartyTo;      //为了记录库存事务
                ipDetail.CurrentPartyToName = ipMaster.PartyToName;      //为了记录库存事务

                foreach (IpLocationDetail ipLocationDetail in targetIpLocationDetail)
                {
                    IpDetailInput ipDetailInput = new IpDetailInput();
                    ipDetailInput.HuId = ipLocationDetail.HuId;
                    ipDetailInput.ShipQty = -ipLocationDetail.Qty / ipDetail.UnitQty;  //转为订单单位
                    ipDetailInput.LotNo = ipLocationDetail.LotNo;
                    ipDetailInput.IsCreatePlanBill = ipLocationDetail.IsCreatePlanBill;
                    ipDetailInput.IsConsignment = ipLocationDetail.IsConsignment;
                    ipDetailInput.PlanBill = ipLocationDetail.PlanBill;
                    ipDetailInput.ActingBill = ipLocationDetail.ActingBill;
                    ipDetailInput.IsATP = ipLocationDetail.IsATP;
                    ipDetailInput.IsFreeze = ipLocationDetail.IsFreeze;
                    ipDetailInput.OccupyType = ipLocationDetail.OccupyType;
                    ipDetailInput.OccupyReferenceNo = ipLocationDetail.OccupyReferenceNo;

                    ipDetail.AddIpDetailInput(ipDetailInput);
                }
                #region 更新库存、记库存事务
                this.locationDetailMgr.InventoryOut(ipDetail, effectiveDate);
                #endregion
            }
            #endregion
        }

        [Transaction(TransactionMode.Requires)]
        public void BatchUpIpDetLocationTo(IList<IpDetail> updatedIpDetails)
        {
            foreach (var ipDet in updatedIpDetails)
            {
                IpDetail oldIpDet = this.genericMgr.FindById<IpDetail>(ipDet.Id);
                if (oldIpDet.LocationTo != ipDet.LocationTo)
                {
                    oldIpDet.LocationTo = ipDet.LocationTo;
                    this.genericMgr.Update(oldIpDet);
                    this.genericMgr.FlushSession();
                }
            }
        }


        #region private methods

        private void DoCloseIpMaster(IpMaster ipMaster)
        {
            ipMaster.Status = com.Sconit.CodeMaster.IpStatus.Close;
            ipMaster.CloseDate = DateTime.Now;
            ipMaster.CloseUserId = SecurityContextHolder.Get().Id;
            ipMaster.CloseUserName = SecurityContextHolder.Get().FullName;

            this.genericMgr.Update(ipMaster);
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
        #endregion

        #region 异步打印
        public void SendPrintData(IpMaster ipMaster)
        {
            try
            {
                PrintIpMaster printIpMaster = Mapper.Map<IpMaster, PrintIpMaster>(ipMaster);
                proxy = pubSubMgr.CreateProxy();
                proxy.Publish(printIpMaster);
            }
            catch (Exception ex)
            {
                pubSubLog.Error("Send data to print sevrer error:", ex);
            }
        }

        public void AsyncSendPrintData(IpMaster ipMaster)
        {
            AsyncSend asyncSend = new AsyncSend(this.SendPrintData);
            asyncSend.BeginInvoke(ipMaster, null, null);
        }


        public delegate void AsyncSend(IpMaster ipMaster);

        #endregion
    }
}
