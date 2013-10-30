using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using AutoMapper;
using Castle.Services.Transaction;
using com.Sconit.Entity;
using com.Sconit.Entity.ACC;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.INV;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.SCM;
using com.Sconit.Persistence;
using com.Sconit.PrintModel.ORD;

namespace com.Sconit.Service.Impl
{
    [Transactional]
    public class PickListMgrImpl : BaseMgr, IPickListMgr
    {
        #region 变量
        private IPublishing proxy;
        public IPubSubMgr pubSubMgr { get; set; }
        public IGenericMgr genericMgr { get; set; }
        public INumberControlMgr numberControlMgr { get; set; }
        public ISystemMgr systemMgr { get; set; }
        public ILocationDetailMgr locationDetailMgr { get; set; }
        public ISqlDao sqlDao { get; set; }
        #endregion

        #region public methods
        #region 按数量
        public string CreatePickList4Qty(string deliveryGroup, string[] orderDetailIdArray, string[] pickQtyArray)
        {
            DataTable dataTable = new DataTable("CreatePickListType");
            dataTable.Columns.Add("OrderDetId", typeof(Int32));
            dataTable.Columns.Add("PickQty", typeof(decimal));
            for (int i = 0; i < orderDetailIdArray.Length; i++)
            {
                dataTable.Rows.Add(int.Parse(orderDetailIdArray[i]), decimal.Parse(pickQtyArray[i]));
            }

            User user = SecurityContextHolder.Get();
            SqlParameter[] parameters = new SqlParameter[5];
            parameters[0] = new SqlParameter("@CreatePickListTable", System.Data.SqlDbType.Structured);
            parameters[0].Value = dataTable;

            parameters[1] = new SqlParameter("@DeliveryGroup", System.Data.SqlDbType.VarChar, 50);
            parameters[1].Value = deliveryGroup;

            parameters[2] = new SqlParameter("@UserId", System.Data.SqlDbType.Int);
            parameters[2].Value = user.Id;

            parameters[3] = new SqlParameter("@UserName", System.Data.SqlDbType.VarChar, 50);
            parameters[3].Value = user.FullName;

            parameters[4] = new SqlParameter("@PLNo", System.Data.SqlDbType.VarChar, 50);
            parameters[4].Direction = ParameterDirection.Output;

            try
            {
                DataSet dataSet = sqlDao.GetDatasetByStoredProcedure("USP_PIK_CreatePickList4Qty", parameters, false);

                if (dataSet.Tables[0] != null && dataSet.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        MessageHolder.AddErrorMessage(row.ItemArray[0].ToString());
                    }
                }
                return parameters[4].Value.ToString();
            }
            catch (Exception ex)
            {
                BusinessException businessException = new BusinessException();
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

                throw businessException;
            }
        }

        public void CancelPickList4Qty(string pickListNo)
        {
            User user = SecurityContextHolder.Get();
            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("@PLNo", System.Data.SqlDbType.VarChar, 50);
            parameters[0].Value = pickListNo;

            parameters[1] = new SqlParameter("@UserId", System.Data.SqlDbType.Int);
            parameters[1].Value = user.Id;

            parameters[2] = new SqlParameter("@UserName", System.Data.SqlDbType.VarChar, 50);
            parameters[2].Value = user.FullName;

            try
            {
                sqlDao.ExecuteStoredProcedure("USP_PIK_CancelPickList4Qty", parameters);
            }
            catch (Exception ex)
            {
                BusinessException businessException = new BusinessException();
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

                throw businessException;
            }
        }       
        #endregion

        [Transaction(TransactionMode.Requires)]
        public PickListMaster CreatePickList(IList<OrderDetail> orderDetailList)
        {
            #region 判断是否全0收货
            if (orderDetailList == null || orderDetailList.Count == 0)
            {
                throw new BusinessException("订单明细不能为空。");
            }

            IList<OrderDetail> nonZeroOrderDetailList = orderDetailList.Where(o => o.PickQtyInput > 0).ToList();

            if (nonZeroOrderDetailList.Count == 0)
            {
                throw new BusinessException("订单明细不能为空。");
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
                throw new BusinessException(Resources.ORD.OrderMaster.Errors_CannotMixOrderTypePick);
            }

            com.Sconit.CodeMaster.OrderType orderType = orderTypeList.Single();

            #region 判断是否超发
            foreach (OrderMaster orderMaster in orderMasterList)
            {
                orderMaster.OrderDetails = nonZeroOrderDetailList.Where(det => det.OrderNo == orderMaster.OrderNo).ToList();

                foreach (OrderDetail orderDetail in nonZeroOrderDetailList)
                {
                    if (!orderMaster.IsOpenOrder)
                    {
                        if (Math.Abs(orderDetail.ShippedQty + orderDetail.PickedQty) >= Math.Abs(orderDetail.OrderedQty))
                        {
                            //订单的发货数已经大于等于订单数
                            throw new BusinessException(Resources.ORD.OrderMaster.Errors_ShipQtyExcceedOrderQty, orderDetail.OrderNo, orderDetail.Item);
                        }
                        else if (!orderMaster.IsShipExceed && Math.Abs(orderDetail.ShippedQty + orderDetail.PickedQty + orderDetail.PickQtyInput) > Math.Abs(orderDetail.OrderedQty))   //不允许过量发货
                        {
                            //订单的发货数 + 本次发货数大于订单数
                            throw new BusinessException(Resources.ORD.OrderMaster.Errors_ShipQtyExcceedOrderQty, orderDetail.OrderNo, orderDetail.Item);
                        }
                    }
                }
            }
            #endregion
            #endregion

            #region 生成拣货单头
            PickListMaster pickListMaster = new PickListMaster();

            #region 订单质量类型
            //var flow = from om in orderMasterList select om.Flow;
            //if (flow.Distinct().Count() > 1)
            //{
            //    throw new BusinessException("路线代码不同不能合并拣货。");
            //}
            //pickListMaster.Flow = flow.Distinct().Single();
            pickListMaster.Flow = (orderMasterList.OrderBy(om => om.Flow).Select(om => om.Flow)).First();
            #endregion

            #region 订单类型
            pickListMaster.OrderType = orderType;
            #endregion

            #region 订单质量类型
            var qualityType = from om in orderMasterList select om.QualityType;
            if (qualityType.Distinct().Count() > 1)
            {
                throw new BusinessException("订单质量状态不同不能合并拣货。");
            }
            pickListMaster.QualityType = qualityType.Distinct().Single();
            #endregion

            #region 状态
            //pickListMaster.Status = com.Sconit.CodeMaster.PickListStatus.Create;
            pickListMaster.Status = com.Sconit.CodeMaster.PickListStatus.Submit;
            #endregion

            #region 发出时间
            pickListMaster.StartTime = (from om in orderMasterList select om.StartTime).Min();
            #endregion

            #region 到达时间
            pickListMaster.WindowTime = (from om in orderMasterList select om.WindowTime).Min();
            #endregion

            #region PartyFrom
            var partyFrom = from om in orderMasterList select om.PartyFrom;
            if (partyFrom.Distinct().Count() > 1)
            {
                throw new BusinessException("来源组织不同不能合并拣货。");
            }
            pickListMaster.PartyFrom = partyFrom.Distinct().Single();
            #endregion

            #region PartyFromName
            pickListMaster.PartyFromName = (from om in orderMasterList select om.PartyFromName).First();
            #endregion

            #region PartyTo
            var partyTo = from om in orderMasterList select om.PartyTo;
            if (partyTo.Distinct().Count() > 1)
            {
                throw new BusinessException("目的组织不同不能合并拣货。");
            }
            pickListMaster.PartyTo = partyTo.Distinct().Single();
            #endregion

            #region PartyToName
            pickListMaster.PartyToName = (from om in orderMasterList select om.PartyToName).First();
            #endregion

            #region ShipFrom
            var shipFrom = from om in orderMasterList select om.ShipFrom;
            if (shipFrom.Distinct().Count() > 1)
            {
                throw new BusinessException("发货地址不同不能合并拣货。");
            }
            pickListMaster.ShipFrom = shipFrom.Distinct().Single();
            #endregion

            #region ShipFromAddr
            pickListMaster.ShipFromAddress = (from om in orderMasterList select om.ShipFromAddress).First();
            #endregion

            #region ShipFromTel
            pickListMaster.ShipFromTel = (from om in orderMasterList select om.ShipFromTel).First();
            #endregion

            #region ShipFromCell
            pickListMaster.ShipFromCell = (from om in orderMasterList select om.ShipFromCell).First();
            #endregion

            #region ShipFromFax
            pickListMaster.ShipFromFax = (from om in orderMasterList select om.ShipFromFax).First();
            #endregion

            #region ShipFromContact
            pickListMaster.ShipFromContact = (from om in orderMasterList select om.ShipFromContact).First();
            #endregion

            #region ShipTo
            var shipTo = from om in orderMasterList select om.ShipTo;
            if (shipTo.Distinct().Count() > 1)
            {
                throw new BusinessException("收货地址不同不能合并拣货。");
            }
            pickListMaster.ShipTo = shipTo.Distinct().Single();
            #endregion

            #region ShipToAddr
            pickListMaster.ShipToAddress = (from om in orderMasterList select om.ShipToAddress).First();
            #endregion

            #region ShipToTel
            pickListMaster.ShipToTel = (from om in orderMasterList select om.ShipToTel).First();
            #endregion

            #region ShipToCell
            pickListMaster.ShipToCell = (from om in orderMasterList select om.ShipToCell).First();
            #endregion

            #region ShipToFax
            pickListMaster.ShipToFax = (from om in orderMasterList select om.ShipToFax).First();
            #endregion

            #region ShipToContact
            pickListMaster.ShipToContact = (from om in orderMasterList select om.ShipToContact).First();
            #endregion

            #region Dock
            var dock = from om in orderMasterList select om.Dock;
            if (dock.Distinct().Count() > 1)
            {
                throw new BusinessException("道口不同不能合并拣货。");
            }
            pickListMaster.Dock = dock.Distinct().Single();
            #endregion

            #region IsAutoReceive
            var isAutoReceive = from om in orderMasterList select om.IsAutoReceive;
            if (isAutoReceive.Distinct().Count() > 1)
            {
                throw new BusinessException("自动收货选项不同不能合并拣货。");
            }
            pickListMaster.IsAutoReceive = isAutoReceive.Distinct().Single();
            #endregion

            #region IsRecScanHu
            var isRecScanHu = from om in orderMasterList select om.IsReceiveScanHu;
            if (isRecScanHu.Distinct().Count() > 1)
            {
                throw new BusinessException("收货扫描条码选项不同不能合并拣货。");
            }
            pickListMaster.IsReceiveScanHu = isRecScanHu.Distinct().Single();
            #endregion

            #region IsPrintAsn
            pickListMaster.IsPrintAsn = orderMasterList.Where(om => om.IsPrintAsn == true) != null;
            #endregion

            #region IsPrintRec
            pickListMaster.IsPrintReceipt = orderMasterList.Where(om => om.IsPrintReceipt == true) != null;
            #endregion

            #region IsRecExceed
            var isRecExceed = from om in orderMasterList select om.IsReceiveExceed;
            if (isRecExceed.Distinct().Count() > 1)
            {
                throw new BusinessException("允许超收选项不同不能合并拣货。");
            }
            pickListMaster.IsReceiveExceed = isRecExceed.Distinct().Single();
            #endregion

            #region IsRecFulfillUC
            var isRecFulfillUC = from om in orderMasterList select om.IsReceiveFulfillUC;
            if (isRecFulfillUC.Distinct().Count() > 1)
            {
                throw new BusinessException("收货满足包装选项不同不能合并拣货。");
            }
            pickListMaster.IsReceiveFulfillUC = isRecFulfillUC.Distinct().Single();
            #endregion

            #region IsRecFifo
            var isRecFifo = from om in orderMasterList select om.IsReceiveFifo;
            if (isRecFifo.Distinct().Count() > 1)
            {
                throw new BusinessException("收货先进先出选项不同不能合并拣货。");
            }
            pickListMaster.IsReceiveFifo = isRecFifo.Distinct().Single();
            #endregion

            #region IsAsnUniqueRec
            var isAsnUniqueRec = from om in orderMasterList select om.IsAsnUniqueReceive;
            if (isAsnUniqueRec.Distinct().Count() > 1)
            {
                throw new BusinessException("ASN一次性收货选项不同不能合并拣货。");
            }
            pickListMaster.IsAsnUniqueReceive = isAsnUniqueRec.Distinct().Single();
            #endregion

            #region IsRecCreateHu
            //var createHuOption = from om in orderMasterList
            //                     where om.CreateHuOption == com.Sconit.CodeMaster.CreateHuOption.Receive
            //                     select om.CreateHuOption;
            //if (createHuOption != null && createHuOption.Count() > 0 && createHuOption.Count() != orderMasterList.Count())
            //{
            //    throw new BusinessException("收货创建条码选项不同不能合并拣货。");
            //}
            pickListMaster.CreateHuOption = CodeMaster.CreateHuOption.None;
            #endregion

            #region IsCheckPartyFromAuth
            pickListMaster.IsCheckPartyFromAuthority = orderMasterList.Where(om => om.IsCheckPartyFromAuthority == true) != null;
            #endregion

            #region IsCheckPartyToAuth
            pickListMaster.IsCheckPartyToAuthority = orderMasterList.Where(om => om.IsCheckPartyToAuthority == true) != null;
            #endregion

            #region RecGapTo
            IList<CodeMaster.ReceiveGapTo> recGapTo = (from om in orderMasterList select om.ReceiveGapTo).ToList();
            if (recGapTo.Distinct().Count() > 1)
            {
                throw new BusinessException("收货差异调整选项不同不能合并拣货。");
            }
            pickListMaster.ReceiveGapTo = recGapTo.Distinct().Single();
            #endregion

            #region AsnTemplate
            var asnTemplate = orderMasterList.Select(om => om.AsnTemplate).First();
            pickListMaster.AsnTemplate = asnTemplate;
            #endregion

            #region RecTemplate
            var recTemplate = orderMasterList.Select(om => om.ReceiptTemplate).First();
            pickListMaster.ReceiptTemplate = recTemplate;
            #endregion

            #region HuTemplate
            var huTemplate = orderMasterList.Select(om => om.HuTemplate).First();
            pickListMaster.HuTemplate = huTemplate;
            #endregion

            #region EffectiveDate
            pickListMaster.EffectiveDate = DateTime.Now;
            #endregion
            #endregion

            #region 生成拣货单明细
            #region 根据拣货的库位和策略分组待拣货明细
            var groupedOrderDetailList = from det in nonZeroOrderDetailList
                                         group det by new { LocaitonFrom = det.LocationFrom, PickStrategy = det.PickStrategy } into result
                                         select new
                                         {
                                             LocationFrom = result.Key.LocaitonFrom,
                                             PickStrategy = result.Key.PickStrategy,
                                             List = result.ToList()
                                         };
            #endregion

            IList<PickLocationDetail> pickLocationLotDetailList = new List<PickLocationDetail>();
            foreach (var groupedOrderDetail in groupedOrderDetailList)
            {
                #region 查找拣货策略
                string pickStrategyCode = groupedOrderDetail.PickStrategy;
                if (string.IsNullOrWhiteSpace(pickStrategyCode))
                {
                    //如果没有拣货策略，从企业选项中取
                    pickStrategyCode = this.systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.DefaultPickStrategy);

                    if (string.IsNullOrWhiteSpace(pickStrategyCode))
                    {
                        throw new BusinessException("没有找到拣货策略，请在订单或企业选项中维护拣货策略。");
                    }
                }

                PickStrategy pickStrategy = this.genericMgr.FindById<PickStrategy>(pickStrategyCode);
                #endregion

                if (pickStrategy.IsSimple)
                {
                    #region 简单模式
                    int seqS = 1;
                    foreach (OrderDetail orderDetail in groupedOrderDetail.List)
                    {
                        PickListDetail pickListDetail = new PickListDetail();
                        pickListDetail.Sequence = seqS++;
                        pickListDetail.OrderNo = orderDetail.OrderNo;
                        pickListDetail.OrderType = orderDetail.OrderType;
                        pickListDetail.OrderSubType = orderDetail.OrderSubType;
                        pickListDetail.OrderDetailId = orderDetail.Id;
                        pickListDetail.OrderDetailSequence = orderDetail.Sequence;
                        pickListDetail.StartTime = orderDetail.StartDate.HasValue ? orderDetail.StartDate : orderMasterList.Where(mstr => mstr.OrderNo == orderDetail.OrderNo).Single().StartTime;
                        pickListDetail.WindowTime = orderDetail.EndDate.HasValue ? orderDetail.EndDate : orderMasterList.Where(mstr => mstr.OrderNo == orderDetail.OrderNo).Single().WindowTime;
                        pickListDetail.Item = orderDetail.Item;
                        pickListDetail.ItemDescription = orderDetail.ItemDescription;
                        pickListDetail.ReferenceItemCode = orderDetail.ReferenceItemCode;
                        pickListDetail.Uom = orderDetail.Uom;
                        pickListDetail.BaseUom = orderDetail.BaseUom;
                        pickListDetail.UnitQty = orderDetail.UnitQty;
                        pickListDetail.UnitCount = orderDetail.UnitCount;
                        pickListDetail.QualityType = orderDetail.QualityType;
                        pickListDetail.ManufactureParty = orderDetail.ManufactureParty;
                        pickListDetail.LocationFrom = orderDetail.LocationFrom;
                        pickListDetail.LocationFromName = orderDetail.LocationFromName;
                        //pickListDetail.Area = g.Key.Area;
                        //pickListDetail.Bin = g.Key.Bin;
                        pickListDetail.LocationTo = orderDetail.LocationTo;
                        pickListDetail.LocationToName = orderDetail.LocationToName;
                        pickListDetail.Qty = orderDetail.PickQtyInput;
                        pickListDetail.PickedQty = 0;
                        //pickListDetail.LotNo = g.Key.LotNo;
                        pickListDetail.IsInspect = orderDetail.IsInspect && orderMasterList.Where(o => o.OrderNo == orderDetail.OrderNo).Single().IsInspect;
                        //pickListDetail.PickStrategy = g.Key.PickStrategy;
                        pickListDetail.IsClose = false;
                        //pickListDetail.IsOdd = g.Key.IsOdd;
                        //pickListDetail.IsDevan = g.Key.IsDevan;
                        pickListDetail.IsInventory = true;

                        pickListMaster.AddPickListDetail(pickListDetail);
                    }
                    #endregion
                }
                else
                {
                    #region 查找库存
                    #region 拼SQL
                    string selectLocationLotDetailStatement = string.Empty;
                    IList<object> selectLocationLotDetailPara = new List<object>();
                    foreach (OrderDetail orderDetail in groupedOrderDetail.List)
                    {
                        if (selectLocationLotDetailStatement == string.Empty)
                        {
                            selectLocationLotDetailStatement = @"select Item, UnitCount, HuUom, ManufactureParty, LotNo, Bin, HuQty, IsOdd, Area
                                                                from LocationLotDetail where HuId is not null and Location = ? and QualityType = ? and OccupyType = ? and IsATP = ? and IsFreeze = ?";
                            selectLocationLotDetailPara.Add(groupedOrderDetail.LocationFrom);
                            selectLocationLotDetailPara.Add(pickListMaster.QualityType);
                            selectLocationLotDetailPara.Add(CodeMaster.OccupyType.None);
                            selectLocationLotDetailPara.Add(true);
                            selectLocationLotDetailPara.Add(false);

                            if (pickStrategy.IsPickFromBin)
                            {
                                selectLocationLotDetailStatement += " and Bin is not null";
                            }
                            selectLocationLotDetailStatement += " and Item in (?";
                        }
                        else
                        {
                            selectLocationLotDetailStatement += ", ?";
                        }
                        selectLocationLotDetailPara.Add(orderDetail.Item);
                    }

                    selectLocationLotDetailStatement += ") order by HuQty Asc";

                    if (pickStrategy.ShipStrategy == CodeMaster.ShipStrategy.FIFO)
                    {
                        selectLocationLotDetailStatement += ", ManufactureDate Asc";
                    }
                    else if (pickStrategy.ShipStrategy == CodeMaster.ShipStrategy.LIFO)
                    {
                        selectLocationLotDetailStatement += ", ManufactureDate Desc";
                    }

                    selectLocationLotDetailStatement += ", BinSequence Asc";
                    #endregion

                    IList<object[]> sumLocationLotDetailList = this.genericMgr.FindAll<object[]>(selectLocationLotDetailStatement, selectLocationLotDetailPara.ToArray());
                    #endregion

                    #region 查找未关闭的拣货单明细
                    #region 拼SQL
                    string selectUnclosePickListDetailStatement = string.Empty;
                    IList<object> selectUnclosePickListDetailPara = new List<object>();
                    foreach (OrderDetail orderDetail in groupedOrderDetail.List)
                    {
                        if (selectUnclosePickListDetailStatement == string.Empty)
                        {
                            selectUnclosePickListDetailStatement = @"select Item, UnitCount, Uom, ManufactureParty, LotNo, Bin, Qty, PickedQty, IsOdd, UnitQty
                                                            from PickListDetail where LocationFrom = ? and QualityType = ? and IsInventory = ? and IsClose = ? and Item in (?";
                            selectUnclosePickListDetailPara.Add(groupedOrderDetail.LocationFrom);
                            selectUnclosePickListDetailPara.Add(pickListMaster.QualityType);
                            selectUnclosePickListDetailPara.Add(true);
                            selectUnclosePickListDetailPara.Add(false);
                        }
                        else
                        {
                            selectUnclosePickListDetailStatement += ", ?";
                        }
                        selectUnclosePickListDetailPara.Add(orderDetail.Item);
                    }

                    selectUnclosePickListDetailStatement += ")";
                    #endregion

                    IList<object[]> unPickListDetailList = this.genericMgr.FindAll<object[]>(selectUnclosePickListDetailStatement, selectUnclosePickListDetailPara.ToArray());
                    #endregion

                    #region 过滤已经被拣货单占用的库存
                    if (unPickListDetailList != null && unPickListDetailList.Count > 0)
                    {
                        foreach (object[] unPickListDetail in unPickListDetailList)
                        {
                            decimal unPickedQty = ((decimal)unPickListDetail[6] - (decimal)unPickListDetail[7]) * (decimal)unPickListDetail[9];
                            if (unPickedQty == 0)
                            {
                                continue;
                            }

                            bool isOdd = (bool)unPickListDetail[8];

                            IList<object[]> machedLocationLotDetailList = sumLocationLotDetailList.Where(
                                                locDet => (string)locDet[0] == (string)unPickListDetail[0]   //Item
                                                && (decimal)locDet[1] == (decimal)unPickListDetail[1]      //UC
                                                && (string)locDet[2] == (string)unPickListDetail[2]        //Uom
                                                && string.Compare((string)locDet[3], (string)unPickListDetail[3]) == 0  //ManufactureParty
                                                && (string)locDet[4] == (string)unPickListDetail[4]        //LotNo
                                                && (string)locDet[5] == (string)unPickListDetail[5]        //Bin
                                                && (decimal)locDet[6] > 0
                                                && (bool)locDet[7] == (bool)unPickListDetail[8]        //IsOdd
                                                 ).ToList();

                            if (machedLocationLotDetailList != null && machedLocationLotDetailList.Count > 0)
                            {
                                foreach (object[] machedLocationLotDetail in machedLocationLotDetailList)
                                {
                                    if ((decimal)machedLocationLotDetail[6] >= unPickedQty)
                                    {
                                        machedLocationLotDetail[6] = (decimal)machedLocationLotDetail[6] - unPickedQty;
                                        unPickedQty = 0;
                                        break;
                                    }
                                    else
                                    {
                                        unPickedQty -= (decimal)machedLocationLotDetail[6];
                                        machedLocationLotDetail[6] = 0;
                                    }
                                }
                            }
                        }
                    }

                    IList<PickLocationDetail> filterLocationLotDetailList = (from locDet in sumLocationLotDetailList
                                                                             where decimal.Parse(locDet[6].ToString()) > 0
                                                                             select new PickLocationDetail
                                                                             {
                                                                                 Item = (string)locDet[0],
                                                                                 UnitCount = (decimal)locDet[1],
                                                                                 Uom = (string)locDet[2],
                                                                                 ManufactureParty = (string.IsNullOrWhiteSpace((string)locDet[3]) ? (string)null : (string)locDet[3]),
                                                                                 LotNo = (string)locDet[4],
                                                                                 Bin = (string)locDet[5],
                                                                                 Qty = (decimal)locDet[6],
                                                                                 IsOdd = (bool)locDet[7],
                                                                                 Area = (string)locDet[8],
                                                                             }).ToList();

                    //var filterLocationLotDetailList = from locDet in sumLocationLotDetailList
                    //                                  join pickDet in unPickListDetailList.DefaultIfEmpty()
                    //                                  on new
                    //                                  {
                    //                                      Item = (string)locDet[0],
                    //                                      UnitCount = (decimal)locDet[1],
                    //                                      Uom = (string)locDet[2],
                    //                                      ManufactureParty = (string.IsNullOrWhiteSpace((string)locDet[3]) ? (string)null : (string)locDet[3]),
                    //                                      LotNo = (string)locDet[4],
                    //                                      Bin = (string)locDet[5]
                    //                                  }
                    //                                  equals new
                    //                                  {
                    //                                      Item = (string)pickDet[0],
                    //                                      UnitCount = (decimal)pickDet[1],
                    //                                      Uom = (string)pickDet[2],
                    //                                      ManufactureParty = (string.IsNullOrWhiteSpace((string)pickDet[3]) ? (string)null : (string)pickDet[3]),
                    //                                      LotNo = (string)pickDet[4],
                    //                                      Bin = (string)pickDet[5]
                    //                                  }
                    //                                  select new
                    //                                  {
                    //                                      Item = (string)locDet[0],
                    //                                      UnitCount = (decimal)locDet[1],
                    //                                      Uom = (string)locDet[2],
                    //                                      ManufactureParty = (string.IsNullOrWhiteSpace((string)locDet[3]) ? (string)null : (string)locDet[3]),
                    //                                      LotNo = (string)locDet[4],
                    //                                      Bin = (string)locDet[5],
                    //                                      RemainQty = (decimal)locDet[8] - (pickDet != null ? (decimal)pickDet[6] : 0)
                    //                                  };
                    #endregion

                    #region 循环匹配拣货项
                    foreach (OrderDetail orderDetail in groupedOrderDetail.List.OrderByDescending(o => o.ManufactureParty).OrderBy(o => o.Item))  //把指定供应商的待拣货项放前面先匹配
                    {
                        OrderDetailInput orderDetailInput = orderDetail.OrderDetailInputs[0];
                        decimal pickQty = orderDetailInput.PickQty;

                        #region 按匹配的选项过滤
                        IList<PickLocationDetail> matchedLocationLotDetailList = filterLocationLotDetailList.Where(l => l.Qty > 0
                                                                                                && l.Item == orderDetail.Item
                                                                                                && l.Uom == orderDetail.Uom
                                                                                                && (string.IsNullOrWhiteSpace(orderDetail.ManufactureParty) || l.ManufactureParty == orderDetail.ManufactureParty)  //指定供应商
                                                                                                && (!pickStrategy.IsFulfillUC || l.UnitCount == orderDetail.UnitCount)  //匹配包装
                                                                                                && (pickStrategy.OddOption == CodeMaster.PickOddOption.OddFirst || !l.IsOdd)   //零头先发
                                                                                                ).OrderBy(l => l.ManufactureParty).ThenBy(l => l.Qty).ToList(); //匹配的库存有供应商的放后面，为了先匹配没有供应商的再匹配有供应商的。当然如果已经指定了供应商这个排序则无效
                        #endregion

                        //#region 指定供应商赋值，没有指定供应商的要赋空值。
                        //foreach (PickLocationDetail matchedLocationLotDetail in matchedLocationLotDetailList)
                        //{
                        //    matchedLocationLotDetail.ManufactureParty = orderDetail.ManufactureParty;
                        //}
                        //#endregion

                        #region 零头不占用发货数
                        if (!pickStrategy.IsOddOccupy)
                        {
                            //零头全部发走
                            foreach (PickLocationDetail matchedLocationLotDetail in matchedLocationLotDetailList.Where(l => l.IsOdd && l.UnitCount <= orderDetail.UnitCount))
                            {
                                PickLocationDetail pickLocationDetail = Mapper.Map<PickLocationDetail, PickLocationDetail>(matchedLocationLotDetail);
                                pickLocationDetail.OrderDetail = orderDetail;
                                pickLocationDetail.PickStrategy = pickStrategyCode;
                                pickLocationDetail.IsInventory = true;
                                pickLocationLotDetailList.Add(pickLocationDetail);
                                matchedLocationLotDetail.Qty = 0; //库存明细数量置零
                            }
                        }
                        #endregion

                        #region 循环拣货
                        foreach (PickLocationDetail matchedLocationLotDetail in matchedLocationLotDetailList.Where(l => l.Qty > 0))
                        {
                            if (pickQty <= 0)
                            {
                                break;
                            }

                            if (pickQty >= matchedLocationLotDetail.Qty)
                            {
                                PickLocationDetail pickLocationDetail = Mapper.Map<PickLocationDetail, PickLocationDetail>(matchedLocationLotDetail);
                                pickLocationDetail.OrderDetail = orderDetail;
                                pickLocationDetail.PickStrategy = pickStrategyCode;
                                pickLocationDetail.IsInventory = true;
                                pickLocationLotDetailList.Add(pickLocationDetail);
                                pickQty -= matchedLocationLotDetail.Qty;
                                matchedLocationLotDetail.Qty = 0; //库存明细数量置零
                            }
                            else
                            {
                                if (pickStrategy.IsDevan)
                                {
                                    #region 允许拆箱
                                    PickLocationDetail pickLocationDetail = Mapper.Map<PickLocationDetail, PickLocationDetail>(matchedLocationLotDetail);
                                    pickLocationDetail.OrderDetail = orderDetail;
                                    pickLocationDetail.PickStrategy = pickStrategyCode;
                                    pickLocationDetail.IsInventory = true;
                                    pickLocationLotDetailList.Add(pickLocationDetail);
                                    pickLocationDetail.Qty = pickQty;
                                    pickLocationDetail.IsDevan = true;
                                    pickLocationLotDetailList.Add(pickLocationDetail);
                                    matchedLocationLotDetail.Qty -= pickQty;
                                    pickQty = 0;
                                    #endregion
                                }
                                else
                                {
                                    #region 不允许拆箱
                                    PickLocationDetail pickLocationDetail = Mapper.Map<PickLocationDetail, PickLocationDetail>(matchedLocationLotDetail);
                                    pickLocationDetail.OrderDetail = orderDetail;
                                    pickLocationDetail.PickStrategy = pickStrategyCode;
                                    pickLocationDetail.IsInventory = true;
                                    pickLocationLotDetailList.Add(pickLocationDetail);
                                    matchedLocationLotDetail.Qty = 0; //库存明细数量置零
                                    pickQty -= matchedLocationLotDetail.Qty;
                                    #endregion
                                }
                            }
                        }
                        #endregion

                        #region 未满足的发货数
                        if (pickQty > 0)
                        {
                            PickLocationDetail pickLocationDetail = new PickLocationDetail();
                            pickLocationDetail.OrderDetail = orderDetail;
                            pickLocationDetail.PickStrategy = pickStrategyCode;
                            pickLocationDetail.Item = orderDetail.Item;
                            pickLocationDetail.UnitCount = orderDetail.UnitCount;
                            pickLocationDetail.Uom = orderDetail.Uom;
                            pickLocationDetail.ManufactureParty = orderDetail.ManufactureParty;
                            //pickLocationDetail.LotNo = matchedLocationLotDetail.LotNo;
                            //pickLocationDetail.Bin = matchedLocationLotDetail.Bin;
                            pickLocationDetail.Qty = pickQty;
                            pickLocationDetail.IsOdd = false;
                            pickLocationDetail.OrderDetail = orderDetail;
                            pickLocationDetail.IsInventory = false;
                            pickLocationLotDetailList.Add(pickLocationDetail);
                        }
                        #endregion
                    }
                    #endregion
                }
            }
            #endregion

            #region 创建拣货单头
            pickListMaster.PickListNo = this.numberControlMgr.GetPickListNo(pickListMaster);
            this.genericMgr.Create(pickListMaster);
            #endregion

            #region 创建拣货明细
            IList<PickListDetail> pickListDetailList = (from det in pickLocationLotDetailList
                                                        group det by new
                                                        {
                                                            OrderDetail = det.OrderDetail,
                                                            PickStrategy = det.PickStrategy,
                                                            LotNo = det.LotNo,
                                                            UnitCount = det.UnitCount,
                                                            ManufactureParty = det.ManufactureParty,
                                                            Area = det.Area,
                                                            Bin = det.Bin,
                                                            IsOdd = det.IsOdd,
                                                            IsDevan = det.IsDevan,
                                                            IsInventory = det.IsInventory
                                                        } into g
                                                        select new PickListDetail
                                                        {
                                                            //PickListNo = pickListMaster.PickListNo,
                                                            OrderNo = g.Key.OrderDetail.OrderNo,
                                                            OrderType = g.Key.OrderDetail.OrderType,
                                                            OrderSubType = g.Key.OrderDetail.OrderSubType,
                                                            OrderDetailId = g.Key.OrderDetail.Id,
                                                            OrderDetailSequence = g.Key.OrderDetail.Sequence,
                                                            StartTime = g.Key.OrderDetail.StartDate.HasValue ? g.Key.OrderDetail.StartDate : orderMasterList.Where(mstr => mstr.OrderNo == g.Key.OrderDetail.OrderNo).Single().StartTime,
                                                            WindowTime = g.Key.OrderDetail.EndDate.HasValue ? g.Key.OrderDetail.EndDate : orderMasterList.Where(mstr => mstr.OrderNo == g.Key.OrderDetail.OrderNo).Single().WindowTime,
                                                            Item = g.Key.OrderDetail.Item,
                                                            ItemDescription = g.Key.OrderDetail.ItemDescription,
                                                            ReferenceItemCode = g.Key.OrderDetail.ReferenceItemCode,
                                                            Uom = g.Key.OrderDetail.Uom,
                                                            BaseUom = g.Key.OrderDetail.BaseUom,
                                                            UnitQty = g.Key.OrderDetail.UnitQty,
                                                            UnitCount = g.Key.UnitCount,
                                                            QualityType = g.Key.OrderDetail.QualityType,
                                                            ManufactureParty = g.Key.ManufactureParty,
                                                            LocationFrom = g.Key.OrderDetail.LocationFrom,
                                                            LocationFromName = g.Key.OrderDetail.LocationFromName,
                                                            Area = g.Key.Area,
                                                            Bin = g.Key.Bin,
                                                            LocationTo = g.Key.OrderDetail.LocationTo,
                                                            LocationToName = g.Key.OrderDetail.LocationToName,
                                                            Qty = g.Sum(det => det.Qty),
                                                            PickedQty = 0,
                                                            LotNo = g.Key.LotNo,
                                                            IsInspect = g.Key.OrderDetail.IsInspect && orderMasterList.Where(o => o.OrderNo == g.Key.OrderDetail.OrderNo).Single().IsInspect,
                                                            PickStrategy = g.Key.PickStrategy,
                                                            IsClose = false,
                                                            IsOdd = g.Key.IsOdd,
                                                            IsDevan = g.Key.IsDevan,
                                                            IsInventory = g.Key.IsInventory
                                                        }).ToList();

            int seq = 1;
            foreach (PickListDetail pickListDetail in pickListDetailList.OrderBy(d => d.OrderNo).ThenBy(d => d.OrderDetailSequence))
            {
                pickListDetail.Sequence = seq++;
                pickListMaster.AddPickListDetail(pickListDetail);
            }

            #region 保存拣货明细
            foreach (PickListDetail pickListDetail in pickListMaster.PickListDetails)
            {
                pickListDetail.PickListNo = pickListMaster.PickListNo;
                this.genericMgr.Create(pickListDetail);
            }
            #endregion
            #endregion

            #region 更新订单明细拣货数
            foreach (var det in (from det in pickListMaster.PickListDetails
                                 group det by new
                                 {
                                     OrderDetailId = det.OrderDetailId,
                                 } into g
                                 select new
                                 {
                                     OrderDetailId = g.Key.OrderDetailId,
                                     Qty = g.Sum(det => det.Qty),
                                 }).ToList())
            {
                OrderDetail orderDetail = orderDetailList.Where(orderDet => orderDet.Id == det.OrderDetailId).Single();
                orderDetail.PickedQty += det.Qty;

                this.genericMgr.Update(orderDetail);
            }
            #endregion

            this.AsyncSendPrintData(pickListMaster);

            return pickListMaster;
        }

        //[Transaction(TransactionMode.Requires)]
        //public void DeletePickList(string pickListNo)
        //{
        //    PickListMaster pickListMaster = genericMgr.FindById<PickListMaster>(pickListNo);
        //    DeletePickList(pickListMaster);
        //}

        //[Transaction(TransactionMode.Requires)]
        //public void DeletePickList(PickListMaster pickListMaster)
        //{
        //    if (pickListMaster.Status != CodeMaster.PickListStatus.Create)
        //    {
        //        throw new BusinessException("不能删除状态为{1}的拣货单{0}。",
        //            pickListMaster.PickListNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.PickListStatus, pickListMaster.Status.ToString()));
        //    }

        //    string hql = "from PickListDetail where PickListNo = ?";
        //    this.genericMgr.Delete(hql, pickListMaster.PickListNo, NHibernateUtil.String);

        //    hql = "from PickListMaster where PickListNo = ?";
        //    this.genericMgr.Delete(hql, pickListMaster.PickListNo, NHibernateUtil.String);
        //}

        //[Transaction(TransactionMode.Requires)]
        //public void ReleasePickList(string pickListNo)
        //{
        //    PickListMaster pickListMaster = genericMgr.FindById<PickListMaster>(pickListNo);
        //    this.ReleasePickList(pickListMaster);
        //}

        //[Transaction(TransactionMode.Requires)]
        //public void ReleasePickList(PickListMaster pickListMaster)
        //{
        //    if (pickListMaster.Status == com.Sconit.CodeMaster.PickListStatus.Create)
        //    {
        //        //验证OrderDetail不能为空
        //        TryLoadPickListDetails(pickListMaster);
        //        if (pickListMaster.PickListDetails == null || pickListMaster.PickListDetails.Count == 0)
        //        {
        //            throw new BusinessException("拣货列表不能为空。");
        //        }
        //    }
        //    else
        //    {
        //        throw new BusinessException("不能释放状态为{1}的拣货单{0}。",
        //           pickListMaster.PickListNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.PickListStatus, pickListMaster.Status.ToString()));
        //    }

        //    pickListMaster.Status = CodeMaster.PickListStatus.Submit;
        //    pickListMaster.ReleaseDate = DateTime.Now;
        //    pickListMaster.ReleaseUserId = SecurityContextHolder.Get().Id;
        //    pickListMaster.ReleaseUserName = SecurityContextHolder.Get().FullName;

        //    this.genericMgr.Update(pickListMaster);
        //}

        [Transaction(TransactionMode.Requires)]
        public void CancelPickList(string pickListNo)
        {
            PickListMaster pickListMaster = this.genericMgr.FindById<PickListMaster>(pickListNo);
            CancelPickList(pickListMaster);
        }

        [Transaction(TransactionMode.Requires)]
        public void CancelPickList(PickListMaster pickListMaster)
        {
            if (pickListMaster.Status != com.Sconit.CodeMaster.PickListStatus.Submit
                && pickListMaster.Status != com.Sconit.CodeMaster.PickListStatus.InProcess)
            {
                throw new BusinessException("不能取消状态为{1}的拣货单{0}。",
                   pickListMaster.PickListNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.PickListStatus, ((int)pickListMaster.Status).ToString()));
            }

            VoidPickListDetail(pickListMaster);

            #region 更新拣货单头
            pickListMaster.Status = CodeMaster.PickListStatus.Cancel;
            pickListMaster.CancelDate = DateTime.Now;
            pickListMaster.CancelUserId = SecurityContextHolder.Get().Id;
            pickListMaster.CancelUserName = SecurityContextHolder.Get().FullName;

            this.genericMgr.Update(pickListMaster);
            #endregion
        }

        [Transaction(TransactionMode.Requires)]
        public void StartPickList(string pickListNo)
        {
            PickListMaster pickListMaster = genericMgr.FindById<PickListMaster>(pickListNo);
            this.StartPickList(pickListMaster);
        }

        [Transaction(TransactionMode.Requires)]
        public void StartPickList(PickListMaster pickListMaster)
        {
            if (pickListMaster.Status != com.Sconit.CodeMaster.PickListStatus.Submit)
            {
                throw new BusinessException("不能开始状态为{1}的拣货单{0}。",
                   pickListMaster.PickListNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.PickListStatus, ((int)pickListMaster.Status).ToString()));
            }

            pickListMaster.Status = CodeMaster.PickListStatus.InProcess;
            pickListMaster.StartDate = DateTime.Now;
            pickListMaster.StartUserId = SecurityContextHolder.Get().Id;
            pickListMaster.StartUserName = SecurityContextHolder.Get().FullName;

            this.genericMgr.Update(pickListMaster);
        }

        [Transaction(TransactionMode.Requires)]
        public void DoPick(IList<PickListDetail> pickListDetailList)
        {
            if (pickListDetailList.Select(det => det.PickListNo).Distinct().Count() > 1)
            {
                throw new TechnicalException("不能跨拣货单拣货。");
            }

            string pickListNo = pickListDetailList.Select(det => det.PickListNo).Distinct().Single();

            #region 判断是否全0拣货
            if (pickListDetailList == null || pickListDetailList.Count == 0)
            {
                throw new BusinessException("拣货明细不能为空。");
            }

            IList<PickListDetail> nonZeroPickListDetailList = pickListDetailList.Where(o => o.PickListDetailInputs != null && o.PickListDetailInputs.Count > 0).ToList();

            if (nonZeroPickListDetailList.Count == 0)
            {
                throw new BusinessException("拣货明细不能为空。");
            }
            #endregion

            #region 库存占用
            IList<InventoryOccupy> inventoryOccupyList = new List<InventoryOccupy>();
            foreach (PickListDetail pickListDetail in nonZeroPickListDetailList)
            {
                foreach (PickListDetailInput pickListDetailInput in pickListDetail.PickListDetailInputs)
                {
                    InventoryOccupy inventoryOccupy = new InventoryOccupy();
                    inventoryOccupy.HuId = pickListDetailInput.HuId;
                    inventoryOccupy.Location = pickListDetail.LocationFrom;
                    inventoryOccupy.QualityType = CodeMaster.QualityType.Qualified;
                    inventoryOccupy.OccupyType = CodeMaster.OccupyType.Pick;
                    inventoryOccupy.OccupyReferenceNo = pickListDetail.PickListNo;
                    inventoryOccupyList.Add(inventoryOccupy);
                }
            }

            IList<LocationLotDetail> locationLotDetailList = this.locationDetailMgr.InventoryOccupy(inventoryOccupyList);
            #endregion

            #region 检查是否超拣
            BusinessException businessException = new BusinessException();
            foreach (PickListDetail pickListDetail in nonZeroPickListDetailList)
            {
                decimal pickedQty = locationLotDetailList.Where(l => pickListDetail.GetPickedHuList().Contains(l.HuId)).Sum(l => l.HuQty);

                if (pickListDetail.Qty < pickListDetail.PickedQty + pickedQty)
                {
                    businessException.AddMessage("拣货行{0}的拣货数已经超过待拣数。", pickListDetail.Sequence.ToString());
                }

                pickListDetail.PickedQty += pickedQty;
            }
            #endregion

            if (businessException.HasMessage)
            {
                throw businessException;
            }

            #region 更新拣货单头
            PickListMaster pickListMaster = this.genericMgr.FindById<PickListMaster>(pickListNo);
            pickListMaster.CompleteDate = DateTime.Now;
            pickListMaster.CompleteUserId = SecurityContextHolder.Get().Id;
            pickListMaster.CompleteUserName = SecurityContextHolder.Get().FullName;
            #endregion

            #region 更新拣货明细
            foreach (PickListDetail pickListDetail in nonZeroPickListDetailList)
            {
                this.genericMgr.Update(pickListDetail);

                foreach (PickListDetailInput pickListDetailInput in pickListDetail.PickListDetailInputs)
                {
                    LocationLotDetail huLocationLotDetail = locationLotDetailList.Where(l => l.HuId == pickListDetailInput.HuId).Single();

                    PickListResult pickListResult = new PickListResult();

                    pickListResult.PickListNo = pickListMaster.PickListNo;
                    pickListResult.PickListDetailId = pickListDetail.Id;
                    pickListResult.OrderDetailId = pickListDetail.OrderDetailId;
                    pickListResult.Item = pickListDetail.Item;
                    pickListResult.ItemDescription = pickListDetail.ItemDescription;
                    pickListResult.ReferenceItemCode = pickListDetail.ReferenceItemCode;
                    pickListResult.Uom = huLocationLotDetail.HuUom;
                    pickListResult.BaseUom = huLocationLotDetail.BaseUom;
                    pickListResult.UnitCount = huLocationLotDetail.UnitCount;
                    pickListResult.UnitQty = huLocationLotDetail.UnitQty;
                    pickListResult.HuId = huLocationLotDetail.HuId;
                    pickListResult.LotNo = huLocationLotDetail.LotNo;
                    pickListResult.IsConsignment = huLocationLotDetail.IsConsignment;
                    pickListResult.PlanBill = huLocationLotDetail.PlanBill;
                    pickListResult.QualityType = huLocationLotDetail.QualityType;
                    pickListResult.IsFreeze = huLocationLotDetail.IsFreeze;
                    pickListResult.IsATP = huLocationLotDetail.IsATP;
                    pickListResult.Qty = huLocationLotDetail.Qty / huLocationLotDetail.UnitQty;

                    this.genericMgr.Create(pickListResult);
                }
            }
            #endregion

            #region 新增拣货明细

            #endregion
        }

        [Transaction(TransactionMode.Requires)]
        public void DeletePickListResult(IList<PickListResult> pickListResultList)
        {
            foreach (PickListResult pickListResult in pickListResultList)
            {
                this.genericMgr.DeleteById<PickListResult>(pickListResult.Id);
            }
        }

        //[Transaction(TransactionMode.Requires)]
        //public void ManualClosePickList(string pickListNo)
        //{
        //    PickListMaster pickListMaster = genericMgr.FindById<PickListMaster>(pickListNo);
        //    ManualClosePickList(pickListMaster);
        //}

        //[Transaction(TransactionMode.Requires)]
        //public void ManualClosePickList(PickListMaster pickListMaster)
        //{
        //    if (pickListMaster.Status != CodeMaster.PickListStatus.InProcess)
        //    {
        //        throw new BusinessException("不能关闭状态为{1}的拣货单{0}。",
        //         pickListMaster.PickListNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.PickListStatus, pickListMaster.Status.ToString()));
        //    }

        //    VoidPickListDetail(pickListMaster);
        //    this.locationDetailMgr.VoidPickHu(pickListMaster.PickListNo);

        //    pickListMaster.Status = CodeMaster.PickListStatus.Close;
        //    pickListMaster.CloseDate = DateTime.Now;
        //    pickListMaster.CloseUserId = SecurityContextHolder.Get().Id;
        //    pickListMaster.CloseUserName = SecurityContextHolder.Get().FullName;

        //    this.genericMgr.Update(pickListMaster);
        //}        
        #endregion

        #region private methods
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

                return pickListMaster.OrderDetails;
            }

            return null;
        }

        private void VoidPickListDetail(PickListMaster pickListMaster)
        {
            #region 获取拣货单明细
            TryLoadPickListDetails(pickListMaster);
            #endregion

            #region 获取订单明细
            IList<OrderDetail> orderDetailList = TryLoadOrderDetails(pickListMaster);
            #endregion

            #region 更新拣货单明细
            foreach (PickListDetail pickListDetail in pickListMaster.PickListDetails)
            {
                pickListDetail.IsClose = true;
                this.genericMgr.Update(pickListDetail);
            }
            #endregion

            #region 更新订单明细的拣货数
            foreach (OrderDetail orderDetail in orderDetailList)
            {
                orderDetail.PickedQty -= pickListMaster.PickListDetails.Where(pd => pd.OrderDetailId == orderDetail.Id).Sum(p => p.Qty);
                this.genericMgr.Update(orderDetail);
            }
            #endregion

            #region 取消拣货单库存占用
            if (pickListMaster.Status == CodeMaster.PickListStatus.InProcess)
            {
                this.locationDetailMgr.CancelInventoryOccupy(CodeMaster.OccupyType.Pick, pickListMaster.PickListNo);
            }
            #endregion
        }
        #endregion

        #region 异步打印
        public void SendPrintData(PickListMaster pickListMaster)
        {
            try
            {
                PrintPickListMaster printPickListMaster = Mapper.Map<PickListMaster, PrintPickListMaster>(pickListMaster);
                proxy = pubSubMgr.CreateProxy();
                proxy.Publish(printPickListMaster);
            }
            catch (Exception ex)
            {
                pubSubLog.Error("Send data to print sevrer error:", ex);
            }
        }

        public void AsyncSendPrintData(PickListMaster pickListMaster)
        {
            AsyncSend asyncSend = new AsyncSend(this.SendPrintData);
            asyncSend.BeginInvoke(pickListMaster, null, null);
        }

        public delegate void AsyncSend(PickListMaster pickListMaster);
        #endregion
    }
}
