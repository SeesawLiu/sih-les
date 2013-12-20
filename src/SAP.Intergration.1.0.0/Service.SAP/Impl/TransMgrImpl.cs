using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Castle.Services.Transaction;
using com.Sconit.Entity;
using com.Sconit.Entity.BIL;
using com.Sconit.Entity.INP;
using com.Sconit.Entity.INV;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.SAP.ORD;
using com.Sconit.Entity.SAP.TRANS;
using com.Sconit.Service.SAP.MI_LES;
using com.Sconit.Utility;
using com.Sconit.Entity.MD;
using System.Text;
using com.Sconit.Entity.ACC;

namespace com.Sconit.Service.SAP.Impl
{
    [Transactional]
    public class TransMgrImpl : BaseMgr, ITransMgr
    {
        #region
        private static log4net.ILog log = log4net.LogManager.GetLogger("SAP_Trans");

        public IDistributionMgr distributionMgr { get; set; }
        public ITrans1p5Mgr trans1p5Mgr { get; set; }
        public ITrans2Mgr trans2Mgr { get; set; }
        private static Int32? NumberControlLength { get; set; }
        private static Int32? SAPTranSi2SapBatch { get; set; }
        private Int32 OutBatch
        {
            get
            {
                if (!SAPTranSi2SapBatch.HasValue)
                {
                    SAPTranSi2SapBatch = int.Parse(this.systemMgr.GetEntityPreferenceValue(com.Sconit.Entity.SYS.EntityPreference.CodeEnum.SAPTransPost2SAPBatchSize)
                    );
                }
                return SAPTranSi2SapBatch.Value;
            }
        }

        #endregion

        private static object ExchangeMoveTypeLock = new object();
        //[Transaction(TransactionMode.Requires)]
        public void ExchangeMoveType()
        {
            lock (ExchangeMoveTypeLock)
            {
                var errorMessageList = new List<ErrorMessage>();
                #region
                int batchNo = int.Parse(this.numberControlMgr.GetNextSequence(Entity.BusinessConstants.NUMBERCONTROL_SISAPTRANSBATCHNO));
                #endregion
                try
                {
                    log.Debug(batchNo.ToString() + " -------------------------------无敌的分割线------------------------------------ " + batchNo.ToString());
                    log.Debug("开始导出移动类型 " + batchNo.ToString());

                    string hql = "select distinct BWART,TCODE from MapMoveTypeTCode group by BWART,TCODE ";
                    IList<object[]> tcodeMoveTypes = this.genericMgr.FindAll<object[]>(hql);
                    IList<Entity.MD.Region> regionList = this.genericMgr.FindAll<Entity.MD.Region>();
                    IList<Entity.MD.Location> locationList = this.genericMgr.FindAll<Entity.MD.Location>();

                    #region 计划外出入库
                    try
                    {
                        log.Debug("开始导出计划外出入库 " + batchNo.ToString());
                        trans1p5Mgr.ExchangeSAPMiscOrder(errorMessageList, batchNo, tcodeMoveTypes, regionList, locationList);
                        log.Debug("完成导出计划外出入库 " + batchNo.ToString());
                    }
                    catch (Exception ex)
                    {
                        this.genericMgr.CleanSession();
                        log.Debug("导出计划外出入库出现异常 " + batchNo.ToString(), ex);
                        errorMessageList.Add(new ErrorMessage
                        {
                            Template = NVelocityTemplateRepository.TemplateEnum.ImportSapMoveType_LesError,
                            Exception = ex
                        });
                    }
                    #endregion

                    #region 库存事务,过滤和安吉有关的业务
                    try
                    {
                        log.Debug("开始导出库存事务 " + batchNo.ToString());
                        trans1p5Mgr.ExchangeSAPTrans(errorMessageList, batchNo, tcodeMoveTypes, regionList, locationList);
                        log.Debug("完成导出库存事务 " + batchNo.ToString());
                    }
                    catch (Exception ex)
                    {
                        this.genericMgr.CleanSession();
                        log.Debug("导出库存事务出现异常 " + batchNo.ToString(), ex);
                        errorMessageList.Add(new ErrorMessage
                        {
                            Template = NVelocityTemplateRepository.TemplateEnum.ImportSapMoveType_LesError,
                            Exception = ex
                        });
                    }
                    #endregion

                    #region 移动类型传SAP
                    log.Debug("开始同步移动类型至SAP " + batchNo.ToString());
                    ReExchangeMoveType();
                    log.Debug("完成同步移动类型至SAP " + batchNo.ToString());
                    #endregion

                    log.Debug("完成导出移动类型 " + batchNo.ToString());
                }
                catch (Exception ex)
                {
                    log.Debug("导出移动类型出现异常 " + batchNo.ToString(), ex);
                    errorMessageList.Add(new ErrorMessage
                    {
                        Template = NVelocityTemplateRepository.TemplateEnum.ImportSapMoveType_LesError,
                        Exception = ex
                    });
                }

                this.SendErrorMessage(errorMessageList);
            }
        }

        private static object ReExchangeMoveTypeLock = new object();
        public void ReExchangeMoveType()
        {
            lock (ReExchangeMoveTypeLock)
            {
                #region
                var errorMessageList = new List<ErrorMessage>();
                #endregion

                int maxFailCount = int.Parse(this.systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.SAPDataExchangeMaxFailCount));

                IList<Entity.SAP.TRANS.InvTrans> invTransList = this.genericMgr.FindEntityWithNativeSql<Entity.SAP.TRANS.InvTrans>
                    ("select * from SAP_InvTrans WITH(NOLOCK) where Status = ? or (Status = ? and ErrorCount < ?) order by CreateDate asc",
                    new object[] { Entity.SAP.StatusEnum.Pending, Entity.SAP.StatusEnum.Fail, maxFailCount });

                #region 分批调用SAP接口
                int skipCount = 0;
                var batchInvTransList = invTransList.Skip(skipCount).Take(OutBatch).ToList();
                while (batchInvTransList.Count() > 0)
                {
                    try
                    {
                        trans2Mgr.CallSapTransService(batchInvTransList, errorMessageList);
                    }
                    catch (Exception ex)
                    {
                        this.genericMgr.CleanSession();
                        log.Error("连接SAP导出移动类型出现异常", ex);
                        errorMessageList.Add(new ErrorMessage
                        {
                            Template = NVelocityTemplateRepository.TemplateEnum.ImportSapMoveType_WebServiceNotAccess,
                            Exception = ex
                        });
                    }
                    finally
                    {
                        skipCount += OutBatch;
                        batchInvTransList = invTransList.Skip(skipCount).Take(OutBatch).ToList();
                    }
                }
                #endregion
            }
        }

        public void ManuallyReExchangeMoveType(IList<Entity.SAP.TRANS.InvTrans> invTransList)
        {
            lock (ReExchangeMoveTypeLock)
            {
                #region
                var errorMessageList = new List<ErrorMessage>();
                #endregion

                #region 分批调用SAP接口
                int skipCount = 0;
                var batchInvTransList = invTransList.Skip(skipCount).Take(OutBatch).ToList();
                while (batchInvTransList.Count() > 0)
                {
                    try
                    {
                        trans2Mgr.CallSapTransService(batchInvTransList, errorMessageList);
                    }
                    catch (Exception ex)
                    {
                        this.genericMgr.CleanSession();
                        log.Error("连接SAP导出移动类型出现异常", ex);
                        errorMessageList.Add(new ErrorMessage
                        {
                            Template = NVelocityTemplateRepository.TemplateEnum.ImportSapMoveType_WebServiceNotAccess,
                            Exception = ex
                        });
                    }
                    finally
                    {
                        skipCount += OutBatch;
                        batchInvTransList = invTransList.Skip(skipCount).Take(OutBatch).ToList();
                    }
                }
                #endregion
            }
        }

        [Transaction(TransactionMode.Requires)]
        public void ManuallyCloseMoveType(IList<Entity.SAP.TRANS.InvTrans> invTransList)
        {
            #region
            var errorMessageList = new List<ErrorMessage>();
            #endregion

            #region 关闭SAP接口
            try
            {
                foreach (var invTrans in invTransList)
                {
                    //如果之前已成功则不做任何处理
                    if (invTrans.Status != Entity.SAP.StatusEnum.Success)
                    {
                        invTrans.Status = Entity.SAP.StatusEnum.Success;
                        invTrans.ErrorMessage = "该移动类型被手工关闭";
                        //invTrans.ErrorCount++;
                        this.UpdateSiSap(invTrans);
                    }
                }
            }
            catch (Exception ex)
            {
                this.genericMgr.CleanSession();
                log.Error(NVelocityTemplateRepository.TemplateEnum.ImportSapMoveType_UpdateInvTransFail, ex);
                errorMessageList.Add(new ErrorMessage
                {
                    Template = NVelocityTemplateRepository.TemplateEnum.ImportSapMoveType_UpdateInvTransFail,
                    Exception = ex
                });
            }
            #endregion

        }
    }

    [Transactional]
    public class Trans1p5MgrImpl : BaseMgr, ITrans1p5Mgr
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("SAP_Trans");
        public ITrans2Mgr trans2Mgr { get; set; }
        private static Int32? SAPTransLes2SiBatch { get; set; }
        private Int32 InBatch
        {
            get
            {
                if (!SAPTransLes2SiBatch.HasValue)
                {
                    SAPTransLes2SiBatch = int.Parse(
                        this.systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.SAPTransSave2TempTableBatchSize)
                    );
                }
                return SAPTransLes2SiBatch.Value;
            }
        }

        public void ExchangeSAPMiscOrder(List<ErrorMessage> errorMessageList, int batchNo, IList<object[]> tcodeMoveTypes, IList<Entity.MD.Region> regionList, IList<Entity.MD.Location> locationList)
        {
            TableIndex tableIndex = this.genericMgr.FindById<TableIndex>(Entity.BusinessConstants.TABLEINDEX_MISCORDERMASTER);

            //过滤当天冲销的，即状态为Cancel并且关闭日期大于上次更新日期
            string selectMiscOrderMstrSql = "select top 1 * from ORD_MiscOrderMstr WITH(NOLOCK) where LastModifyDate > ? and MoveType <> '999' and (Status = ? or (Status = ? and CloseDate <= ? )) order by LastModifyDate";
            string selectMiscOrderDetSql = "select * from ORD_MiscOrderDet WITH(NOLOCK) where MiscOrderNo = ?";
            string selectMiscOrderLocationDetSql = "select * from ORD_MiscOrderLocationDet WITH(NOLOCK) where MiscOrderNo = ?";
            
            object[] pamaDate = new object[] { tableIndex.LastModifyDate, CodeMaster.MiscOrderStatus.Close, CodeMaster.MiscOrderStatus.Cancel, tableIndex.LastModifyDate };
            MiscOrderMaster miscOrderMaster = this.genericMgr.FindEntityWithNativeSql<MiscOrderMaster>(selectMiscOrderMstrSql, pamaDate).SingleOrDefault();

            while (miscOrderMaster != null)
            {
                IList<MiscOrderDetail> miscOrderDetailList = this.genericMgr.FindEntityWithNativeSql<MiscOrderDetail>(selectMiscOrderDetSql, miscOrderMaster.MiscOrderNo);
                IList<MiscOrderLocationDetail> miscOrderLocationDetailList = this.genericMgr.FindEntityWithNativeSql<MiscOrderLocationDetail>(selectMiscOrderLocationDetSql, miscOrderMaster.MiscOrderNo);
                trans2Mgr.MiscOrder2InvTrans(tableIndex, miscOrderMaster, miscOrderDetailList, miscOrderLocationDetailList, errorMessageList, batchNo, tcodeMoveTypes, regionList, locationList);

                pamaDate = new object[] { tableIndex.LastModifyDate, CodeMaster.MiscOrderStatus.Close, CodeMaster.MiscOrderStatus.Cancel, tableIndex.LastModifyDate };
                miscOrderMaster = this.genericMgr.FindEntityWithNativeSql<MiscOrderMaster>(selectMiscOrderMstrSql, pamaDate).SingleOrDefault();
            }
        }

        public void ExchangeSAPTrans(List<ErrorMessage> errorMessageList, int batchNo, IList<object[]> tcodeMoveTypes, IList<Entity.MD.Region> regionList, IList<Entity.MD.Location> locationList)
        {
            DateTime dateTimeNow = DateTime.Now;
            var tableIndex = this.genericMgr.FindById<TableIndex>(Entity.BusinessConstants.TABLEINDEX_LOCATIONTRANSACTION);
            string sql = "select top " + InBatch + " * from VIEW_LocTrans WITH(NOLOCK) where Id > ? order by Id ";
            IList<LocationTransaction> transList = this.genericMgr.FindEntityWithNativeSql<LocationTransaction>(sql, tableIndex.Id);

            while (transList != null && transList.Count() > 0)
            {
                trans2Mgr.CreateInvTrans(transList, errorMessageList, batchNo, dateTimeNow, tcodeMoveTypes, regionList, locationList, tableIndex);

                transList = this.genericMgr.FindEntityWithNativeSql<LocationTransaction>(sql, tableIndex.Id);
            }
        }
    }

    [Transactional]
    public class Trans2MgrImpl : BaseMgr, ITrans2Mgr
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("SAP_Trans");
        private static Int32? SAPTranSi2SapBatch { get; set; }
        private static string[] NotSettleBillBWARTArray = new string[] { "ZR1", "ZR2", "ZR5", "ZR6" };
        private Int32 OutBatch
        {
            get
            {
                if (!SAPTranSi2SapBatch.HasValue)
                {
                    SAPTranSi2SapBatch = int.Parse(this.systemMgr.GetEntityPreferenceValue(com.Sconit.Entity.SYS.EntityPreference.CodeEnum.SAPTransPost2SAPBatchSize)
                    );
                }
                return SAPTranSi2SapBatch.Value;
            }
        }

        [Transaction(TransactionMode.Requires)]
        public void CreateInvTrans(IList<LocationTransaction> transList, List<ErrorMessage> errorMessageList, int batchNo, DateTime dateTimeNow, IList<object[]> tcodeMoveTypes, IList<Entity.MD.Region> regionList, IList<Entity.MD.Location> locationList, TableIndex tableIndex)
        {
            IList<InvTrans> invTransList = new List<InvTrans>();

            IList<string> createInvTransRegonList = this.genericMgr.FindAllWithNativeSql<string>("select Code from MD_Region where IsCreateInvTrans = ?", true);

            #region 非安吉相关生成移动类型
            foreach (var locTrans in transList)
            {
                IList<InvTrans> thisInvTransList = new List<InvTrans>();

                InvTrans invTrans = new InvTrans();
                invTrans.BatchNo = batchNo;
                invTrans.LocTransId = locTrans.Id;

                #region 获取工厂/库位
                string plantFrom = locTrans.PartyFrom != null ? GetPlant(regionList, locTrans.PartyFrom) : null;
                string plantTo = locTrans.PartyTo != null ? GetPlant(regionList, locTrans.PartyTo) : null;
                string sapLocationFrom = locTrans.LocationFrom != null ? GetSapLocation(locationList, locTrans.LocationFrom) : null;
                string sapLocationTo = locTrans.LocationTo != null ? GetSapLocation(locationList, locTrans.LocationTo) : null;
                #endregion

                #region 采购
                #region 采购收货
                //采购收货	101	201	有采购单的 
                //采购单或计划协议号取ExternalOrderNo
                //采购单行号取ExternalSequence,计划协议号取ExternalSequence.Split('|')[0]

                //根据配置文件决定是否过滤业务
                bool isPartyToExchangeMoveType = false;
                bool isPartyFromExchangeMoveType = false;
                //由于asn可以更改入库地点，所以要根据实际的地点来取区域进行判断
                string realRegionTo = locTrans.LocationTo != null ? GetRealRegion(locTrans.LocationTo) : string.Empty;
                if (createInvTransRegonList != null && createInvTransRegonList.Where(region => string.Equals(region, realRegionTo, StringComparison.OrdinalIgnoreCase)).Count() > 0)
                {
                    isPartyToExchangeMoveType = true;
                }
                string realRegionFrom = locTrans.LocationFrom != null ? GetRealRegion(locTrans.LocationFrom) : string.Empty;
                if (createInvTransRegonList != null && createInvTransRegonList.Where(region => string.Equals(region, realRegionFrom, StringComparison.OrdinalIgnoreCase)).Count() > 0)
                {
                    isPartyFromExchangeMoveType = true;
                }

                if (locTrans.TransactionType == CodeMaster.TransactionType.RCT_PO
                    || locTrans.TransactionType == CodeMaster.TransactionType.RCT_SL)
                {
                    if (isPartyToExchangeMoveType)
                    {
                        object[] EBELN_EBELP = null;

                        if (locTrans.OrderType == CodeMaster.OrderType.ScheduleLine)
                        {
                            EBELN_EBELP = genericMgr.FindAllWithNativeSql<object[]>(NativeSqlStatement.SELECT_SL_EBELN_AND_EBELP_STATEMENT, locTrans.OrderDetailId).Single();
                        }
                        else
                        {
                            EBELN_EBELP = genericMgr.FindAllWithNativeSql<object[]>(NativeSqlStatement.SELECT_PO_EBELN_AND_EBELP_STATEMENT, locTrans.OrderDetailId).Single();
                        }

                        invTrans.BWART = "101";
                        invTrans.WERKS = plantTo;
                        invTrans.LIFNR = locTrans.PartyFrom;
                        invTrans.LGORT = sapLocationTo;
                        invTrans.SOBKZ = locTrans.ActingBillQty > 0 ? null : "K";  //发生结算为空，不发生结算为K
                        if (EBELN_EBELP[0] != null)
                        {
                            invTrans.EBELN = (string)EBELN_EBELP[0];
                        }
                        else
                        {
                            invTrans.ErrorId = InvTrans.ErrorIdEnum.E101;
                            log.Error(string.Format("库存事务{0}的PO单号/计划协议号为空。", locTrans.Id.ToString()));
                        }

                        if (EBELN_EBELP[1] == null)
                        {
                            invTrans.ErrorId = InvTrans.ErrorIdEnum.E102;
                            log.Error(string.Format("库存事务{0}的PO单号行号/计划协议行号为空。", locTrans.Id.ToString()));
                        }
                        //新逻辑：计划协议行号不需要判断分隔符
                        //else if (locTrans.OrderType == CodeMaster.OrderType.ScheduleLine)
                        //{
                        //    if (!((string)EBELN_EBELP[1]).Contains(SplitSymbol))
                        //    {
                        //        invTrans.ErrorId = InvTrans.ErrorIdEnum.E103;
                        //        log.Error(string.Format("交货计划行收货时库存事务{0}的计划协议行号不包含分隔符{1}。", locTrans.Id.ToString(), SplitSymbol));
                        //    }
                        //    else
                        //    {
                        //        invTrans.EBELP = ((string)EBELN_EBELP[1]).Split(SplitSymbol)[0];
                        //    }
                        //}
                        else
                        {
                            invTrans.EBELP = (string)EBELN_EBELP[1];
                        }
                        invTrans.GRUND = locTrans.LocationIOReason;
                        invTrans.XBLNR = locTrans.ReceiptNo;
                        invTrans.XABLN = locTrans.IpNo;
                        //2013-10-16以后不再传322了，而是根据收货明细中的IsInspect字段来判断，并记录到INSMK=2
                        //ReceiptDetail receipDetail = genericMgr.FindEntityWithNativeSql<ReceiptDetail>("select * from ORD_RecDet_1 WITH(NOLOCK) where Id = ?", locTrans.ReceiptDetailId).Single();
                        //if (receipDetail.IsInspect)
                        //    invTrans.INSMK = "2";
                        //else
                        //    invTrans.INSMK = null;
                        invTrans.INSMK = locTrans.QualityType == CodeMaster.QualityType.Qualified ? null : "3";
                        invTrans.OrderNo = locTrans.OrderNo;
                        invTrans.DetailId = locTrans.OrderDetailId;

                        thisInvTransList.Add(invTrans);

                        #region 寄售物料，采购就发生结算，收货库位补做411K
                        if (locTrans.ActingBillQty > 0)
                        {
                            ActingBill actingBill = genericMgr.FindEntityWithNativeSql<ActingBill>("select * from BIL_ActBill WITH(NOLOCK) where Id = ?", locTrans.ActingBill).Single();
                            if (actingBill != null && actingBill.BillTerm != CodeMaster.OrderBillTerm.ReceivingSettlement)
                            {

                                InvTrans invTrans411K = new InvTrans();

                                invTrans411K.BWART = "411";
                                invTrans411K.LIFNR = actingBill.Party;
                                invTrans411K.SOBKZ = "K";
                                invTrans411K.XBLNR = actingBill.ReceiptNo;
                                invTrans411K.WERKS = plantTo;
                                invTrans411K.LGORT = sapLocationTo;
                                invTrans411K.UMWRK = plantTo;
                                invTrans411K.UMLGO = sapLocationTo;
                                invTrans411K.OrderNo = locTrans.OrderNo;
                                invTrans411K.DetailId = locTrans.OrderDetailId;

                                thisInvTransList.Add(invTrans411K);

                                invTrans.SOBKZ = "K";
                            }
                        }
                        #endregion
                    }
                }
                #endregion

                #region 采购冲销
                //采购冲销	102	202	//采购入库冲销
                //采购单或计划协议号取ExternalOrderNo
                //采购单行号取ExternalSequence,计划协议号取ExternalSequence.Split('|')[0]
                else if (locTrans.TransactionType == CodeMaster.TransactionType.RCT_PO_VOID
                    || locTrans.TransactionType == CodeMaster.TransactionType.RCT_SL_VOID)
                {
                    if (isPartyToExchangeMoveType)
                    {
                        object[] EBELN_EBELP = null;
                        if (locTrans.OrderType == CodeMaster.OrderType.ScheduleLine)
                        {
                            EBELN_EBELP = genericMgr.FindAllWithNativeSql<object[]>(NativeSqlStatement.SELECT_SL_EBELN_AND_EBELP_STATEMENT, locTrans.OrderDetailId).Single();
                        }
                        else
                        {
                            EBELN_EBELP = genericMgr.FindAllWithNativeSql<object[]>(NativeSqlStatement.SELECT_PO_EBELN_AND_EBELP_STATEMENT, locTrans.OrderDetailId).Single();
                        }

                        invTrans.BWART = "102";
                        invTrans.WERKS = plantTo;
                        invTrans.LIFNR = locTrans.PartyFrom;
                        invTrans.LGORT = sapLocationTo;
                        if (EBELN_EBELP[0] != null)
                        {
                            invTrans.EBELN = (string)EBELN_EBELP[0];
                        }
                        else
                        {
                            invTrans.ErrorId = InvTrans.ErrorIdEnum.E101;
                            log.Error(string.Format("库存事务{0}的PO单号/计划协议号为空。", locTrans.Id.ToString()));
                        }

                        if (EBELN_EBELP[1] == null)
                        {
                            invTrans.ErrorId = InvTrans.ErrorIdEnum.E102;
                            log.Error(string.Format("库存事务{0}的PO单号行号/计划协议行号为空。", locTrans.Id.ToString()));
                        }
                        //else if (locTrans.OrderType == CodeMaster.OrderType.ScheduleLine)
                        //{
                        //    if (!((string)EBELN_EBELP[1]).Contains(SplitSymbol))
                        //    {
                        //        invTrans.ErrorId = InvTrans.ErrorIdEnum.E103;
                        //        log.Error(string.Format("交货计划行收货时库存事务{0}的计划协议行号不包含分隔符{1}。", locTrans.Id.ToString(), SplitSymbol));
                        //    }
                        //    else
                        //    {
                        //        invTrans.EBELP = ((string)EBELN_EBELP[1]).Split(SplitSymbol)[0];
                        //    }
                        //}
                        else
                        {
                            invTrans.EBELP = (string)EBELN_EBELP[1];
                        }
                        invTrans.GRUND = locTrans.LocationIOReason;
                        invTrans.XBLNR = locTrans.ReceiptNo;
                        invTrans.SOBKZ = locTrans.IsConsignment && locTrans.PlanBillQty < 0 ? "K" : null;    //寄售收货冲销，planbillqty为负数
                        invTrans.XABLN = locTrans.IpNo;
                        invTrans.INSMK = locTrans.QualityType == CodeMaster.QualityType.Qualified ? null : "3";

                        #region 寄售物料，采购就发生结算，冲销时先做412K
                        if (locTrans.ActingBillQty < 0)
                        {
                            ActingBill actingBill = genericMgr.FindEntityWithNativeSql<ActingBill>("select * from BIL_ActBill WITH(NOLOCK) where Id = ?", locTrans.ActingBill).Single();

                            if (actingBill != null && actingBill.BillTerm != CodeMaster.OrderBillTerm.ReceivingSettlement)
                            {
                                InvTrans invTrans412K = new InvTrans();

                                invTrans412K.BWART = "412";
                                invTrans412K.LIFNR = actingBill.Party;
                                invTrans412K.SOBKZ = "K";
                                invTrans412K.XBLNR = actingBill.ReceiptNo;
                                invTrans412K.WERKS = plantTo;
                                invTrans412K.LGORT = sapLocationTo;
                                invTrans412K.UMWRK = plantTo;
                                invTrans412K.UMLGO = sapLocationTo;

                                invTrans412K.OrderNo = locTrans.OrderNo;
                                invTrans412K.DetailId = locTrans.OrderDetailId;

                                thisInvTransList.Add(invTrans412K);

                                invTrans.SOBKZ = "K";
                            }
                        }
                        #endregion

                        invTrans.OrderNo = locTrans.OrderNo;
                        invTrans.DetailId = locTrans.OrderDetailId;
                        thisInvTransList.Add(invTrans);
                    }
                }
                #endregion

                #region 采购退货
                //采购退货
                /*  1.	寄售MoveType:ZR1K、ZR5K
                    2.	非寄售MoveType:161
                */
                else if (locTrans.TransactionType == CodeMaster.TransactionType.ISS_PO
                    || locTrans.TransactionType == CodeMaster.TransactionType.ISS_SL)
                {
                    if (isPartyFromExchangeMoveType)
                    {
                        #region 非寄售MoveType:161
                        //如果是寄售库存就算有退货单号也不做161
                        if (!(locTrans.IsConsignment && locTrans.PlanBillQty < 0))//161
                        {
                            if (locTrans.QualityType == CodeMaster.QualityType.Qualified)
                            {
                                //采购退货-自有	161	203	//采购退货			0		0
                                invTrans.BWART = "161";
                                invTrans.WERKS = plantFrom;
                                invTrans.LIFNR = locTrans.PartyTo;
                                invTrans.LGORT = sapLocationFrom;
                                invTrans.GRUND = locTrans.LocationIOReason;
                                invTrans.XBLNR = locTrans.ReceiptNo;
                            }
                            //其他的不考虑 无162 冲销.无 有订单,冻结的退货
                        }
                        #endregion

                        #region 寄售MoveType:ZR1K、ZR5K
                        else
                        {
                            #region 合格品退货做ZR1
                            //采购退货-寄售	ZR1	203	//采购退货			1		0
                            if (locTrans.QualityType == CodeMaster.QualityType.Qualified)
                            {
                                invTrans.BWART = "ZR1";
                                invTrans.WERKS = plantFrom;
                                invTrans.LIFNR = locTrans.PartyTo;
                                invTrans.LGORT = sapLocationFrom;
                                invTrans.SOBKZ = "K";
                            }
                            #endregion

                            #region 不合格品退货做ZR5
                            //采购退货-寄售-冻结	ZR5	203	//采购退货			1		2
                            else if (locTrans.QualityType == CodeMaster.QualityType.Reject)
                            {
                                invTrans.BWART = "ZR5";
                                invTrans.WERKS = plantFrom;
                                invTrans.LIFNR = locTrans.PartyTo;
                                invTrans.LGORT = sapLocationFrom;
                                invTrans.SOBKZ = "K";
                            }
                            #endregion

                            invTrans.OrderNo = locTrans.OrderNo;
                            invTrans.DetailId = locTrans.OrderDetailId;
                            thisInvTransList.Add(invTrans);
                        }
                        #endregion
                    }
                }
                #endregion

                #region 采购退货冲销
                //采购退货冲销-寄售	ZR2/ZR6	204	//采购退货冲销			1		0
                else if (locTrans.TransactionType == CodeMaster.TransactionType.ISS_PO_VOID
                    || locTrans.TransactionType == CodeMaster.TransactionType.ISS_SL_VOID)
                {
                    if (isPartyFromExchangeMoveType)
                    {
                        if (locTrans.IsConsignment && locTrans.PlanBillQty > 0)
                        {
                            #region 非寄售MoveType:162，没有这种情况
                            #endregion
                        }
                        else
                        {
                            #region 合格品退货冲销做ZR2
                            if (locTrans.QualityType == CodeMaster.QualityType.Qualified)
                            {
                                invTrans.BWART = "ZR2";
                                invTrans.WERKS = plantFrom;
                                invTrans.LIFNR = locTrans.PartyTo;
                                invTrans.LGORT = sapLocationFrom;
                                invTrans.SOBKZ = "K";
                            }
                            #endregion

                            #region 不合格品退货冲销做ZR6
                            else if (locTrans.QualityType == CodeMaster.QualityType.Reject)
                            {
                                invTrans.BWART = "ZR6";
                                invTrans.WERKS = plantFrom;
                                invTrans.LIFNR = locTrans.PartyTo;
                                invTrans.LGORT = sapLocationFrom;
                                invTrans.SOBKZ = "K";
                            }
                            #endregion
                        }

                        invTrans.OrderNo = locTrans.OrderNo;
                        invTrans.DetailId = locTrans.OrderDetailId;
                        thisInvTransList.Add(invTrans);
                    }
                }
                #endregion
                #endregion

                #region  移库 只导同一工厂内部的移库,其他的通过计划外出入库
                //安吉相关事务只处理buff对安吉的收货/收货冲销/退货
                /*
                Transtype	     Loc出现位置	   解释	                移动类型接口是否处理
                RCT-TR	         PartyFrom	       buff正常收货	        y
                RCT-TR	         PartyTo	       buff差异收货	        ?
                RCT-TR-Void	     PartyFrom	       buff收货冲销	        y
                RCT-TR-Rtn	     PartyTo	       buff向安吉退货       y
                RCT-TR-Rtn-Void	 PartyTo	       buff向安吉退货冲销	y
                ISS-TR-Void      PartyFrom         安吉发货冲销         n
                */

                /*        IsCs PlanBill  ActingBill    TransType                       解释
                * 311      0       /          =0       RCT_TR/RCT_TR_RTN               自有物资移库
                * 312      0       /          =0       RCT_TR_VOID/RCT_TR_RTN_VOID     自有物资移库移库冲销
                * 311K     1       >0          /       RCT_TR/RCT_TR_RTN               寄售移库不结算
                * 312K     1       <0          /       RCT_TR_VOID/RCT_TR_RTN_VOID     寄售移库冲销不结算
                * 411K     0       /          >0       RCT_TR/RCT_TR_RTN               寄售移库结算
                * 412K     1       /          <0       RCT_TR_VOID/RCT_TR_RTN_VOID     寄售移库结算冲销
                * 325                                                                  冻结库存移库
                */
                #region 移库/移库退货
                else if ((locTrans.TransactionType == CodeMaster.TransactionType.RCT_TR
                    || locTrans.TransactionType == CodeMaster.TransactionType.RCT_TR_RTN)
                    && locTrans.QualityType == CodeMaster.QualityType.Qualified
                    && plantFrom == plantTo && sapLocationFrom != sapLocationTo)//&& SihLoc.Contains(sapLocationTo)
                {
                    //安吉或双桥-buff的移库事务做特殊处理
                    if ((locTrans.PartyFrom.ToUpper() == "LOC" || locTrans.PartyFrom.ToUpper() == "SQC") 
                        && locTrans.TransactionType == CodeMaster.TransactionType.RCT_TR
                        && locTrans.IpDetailId != 0)
                    {
                        IpDetail ipDetail = genericMgr.FindEntityWithNativeSql<IpDetail>("select * from ORD_IpDet_2 WITH(NOLOCK) where Id = ?", locTrans.IpDetailId).Single();
                        if (ipDetail.BWART == "411K")
                        {
                            invTrans.BWART = "411";
                            invTrans.LIFNR = ipDetail.ManufactureParty;
                            invTrans.SOBKZ = "K";
                            invTrans.XBLNR = locTrans.ReceiptNo;
                            invTrans.WERKS = plantTo;
                            invTrans.LGORT = sapLocationFrom;
                            invTrans.UMWRK = plantFrom;
                            invTrans.UMLGO = sapLocationTo;
                        }
                        else if (ipDetail.BWART == "311K")
                        {
                            invTrans.BWART = "311";
                            invTrans.LIFNR = ipDetail.ManufactureParty;
                            invTrans.SOBKZ = "K";
                            invTrans.XBLNR = locTrans.ReceiptNo;
                            invTrans.WERKS = plantTo;
                            invTrans.LGORT = sapLocationFrom;
                            invTrans.UMWRK = plantFrom;
                            invTrans.UMLGO = sapLocationTo;
                        }
                        else if (ipDetail.BWART == "311")
                        {
                            invTrans.BWART = "311";
                            invTrans.XBLNR = locTrans.ReceiptNo;
                            invTrans.WERKS = plantTo;
                            invTrans.LGORT = sapLocationFrom;
                            invTrans.UMWRK = plantFrom;
                            invTrans.UMLGO = sapLocationTo;
                        }

                        invTrans.OrderNo = locTrans.OrderNo;
                        invTrans.DetailId = locTrans.OrderDetailId;
                        thisInvTransList.Add(invTrans);
                    }
                    else
                    {
                        if (locTrans.ActingBillQty > 0)
                        {
                            //LOC移库(退货)至线旁-寄售结算	411K	303	//移库入库(退货)			
                            ActingBill actingBill = genericMgr.FindEntityWithNativeSql<ActingBill>("select * from BIL_ActBill WITH(NOLOCK) where Id = ?", locTrans.ActingBill).Single();
                            invTrans.BWART = "411";
                            invTrans.LIFNR = actingBill.Party;
                            invTrans.SOBKZ = "K";
                            invTrans.XBLNR = actingBill.ReceiptNo;
                            invTrans.WERKS = plantTo;
                            invTrans.LGORT = sapLocationFrom;
                            invTrans.UMWRK = plantFrom;
                            invTrans.UMLGO = sapLocationTo;
                        }
                        else if (locTrans.IsConsignment && locTrans.PlanBillQty > 0)
                        {
                            //todo-不考虑差异收货，如buff收安吉的物料产生的退回原库位的收货差异。

                            //LOC移库(退货)至线旁-寄售不结算	311K	303	//移库入库(退货)
                            PlanBill planBill = genericMgr.FindEntityWithNativeSql<PlanBill>("select * from BIL_PlanBill WITH(NOLOCK) where Id = ?", locTrans.PlanBill).Single();
                            invTrans.BWART = "311";
                            invTrans.LIFNR = planBill.Party;
                            invTrans.SOBKZ = "K";
                            invTrans.XBLNR = planBill.ReceiptNo;
                            invTrans.WERKS = plantTo;
                            invTrans.LGORT = sapLocationFrom;
                            invTrans.UMWRK = plantFrom;
                            invTrans.UMLGO = sapLocationTo;
                        }
                        else if (locTrans.ActingBillQty < 0)
                        {
                            log.Error(GetTLog(locTrans, "移库/移库退货结算数量小于0。"));
                            invTrans.ErrorId = InvTrans.ErrorIdEnum.E104;
                        }
                        else if (locTrans.IsConsignment && locTrans.PlanBillQty < 0)
                        {
                            log.Error(GetTLog(locTrans, "移库/移库退货寄售数量小于0。"));
                            invTrans.ErrorId = InvTrans.ErrorIdEnum.E105;
                        }
                        else
                        {
                            //LOC移库(退货)至线旁(自有物资)	311	303	//移库(退货)入库
                            invTrans.BWART = "311";
                            if (locTrans.Qty >= 0)
                            {
                                invTrans.XBLNR = locTrans.ReceiptNo;
                                invTrans.WERKS = plantTo;
                                invTrans.LGORT = sapLocationFrom;
                                invTrans.UMWRK = plantFrom;
                                invTrans.UMLGO = sapLocationTo;
                            }
                            else
                            {
                                invTrans.XBLNR = locTrans.ReceiptNo;
                                invTrans.WERKS = plantFrom;
                                invTrans.LGORT = sapLocationTo;
                                invTrans.UMWRK = plantTo;
                                invTrans.UMLGO = sapLocationFrom;
                            }
                        }

                        invTrans.OrderNo = locTrans.OrderNo;
                        invTrans.DetailId = locTrans.OrderDetailId;
                        thisInvTransList.Add(invTrans);
                    }
                }
                #endregion

                #region 移库冲销/移库退货冲销
                else if ((locTrans.TransactionType == CodeMaster.TransactionType.RCT_TR_VOID
                    || locTrans.TransactionType == CodeMaster.TransactionType.RCT_TR_RTN_VOID)
                    //&& !locTrans.IsConsignment && locTrans.ActingBill == 0
                    && locTrans.QualityType == CodeMaster.QualityType.Qualified
                    && plantFrom == plantTo && sapLocationFrom != sapLocationTo) //&& SihLoc.Contains(sapLocationTo)
                {
                    //安吉-buff的移库冲销要特殊处理
                    if ((locTrans.PartyFrom.ToUpper() == "LOC" || locTrans.PartyFrom.ToUpper() == "SQC") 
                        && locTrans.TransactionType == CodeMaster.TransactionType.RCT_TR_VOID
                        && locTrans.IpDetailId > 0)
                    {
                        IpDetail ipDetail = genericMgr.FindEntityWithNativeSql<IpDetail>("select * from ORD_IpDet_2 WITH(NOLOCK) where Id = ?", locTrans.IpDetailId).Single();
                        if (ipDetail.BWART == "411K")
                        {
                            invTrans.BWART = "412";
                            invTrans.LIFNR = ipDetail.ManufactureParty;
                            invTrans.SOBKZ = "K";
                            invTrans.XBLNR = locTrans.ReceiptNo;
                            invTrans.WERKS = plantTo;
                            invTrans.LGORT = sapLocationFrom;
                            invTrans.UMWRK = plantFrom;
                            invTrans.UMLGO = sapLocationTo;
                        }
                        else if (ipDetail.BWART == "311K")
                        {
                            invTrans.BWART = "312";
                            invTrans.LIFNR = ipDetail.ManufactureParty;
                            invTrans.SOBKZ = "K";
                            invTrans.XBLNR = locTrans.ReceiptNo;
                            invTrans.WERKS = plantTo;
                            invTrans.LGORT = sapLocationFrom;
                            invTrans.UMWRK = plantFrom;
                            invTrans.UMLGO = sapLocationTo;
                        }
                        else if (ipDetail.BWART == "311")
                        {
                            invTrans.BWART = "312";
                            invTrans.XBLNR = locTrans.ReceiptNo;
                            invTrans.WERKS = plantTo;
                            invTrans.LGORT = sapLocationFrom;
                            invTrans.UMWRK = plantFrom;
                            invTrans.UMLGO = sapLocationTo;
                        }

                        invTrans.OrderNo = locTrans.OrderNo;
                        invTrans.DetailId = locTrans.OrderDetailId;
                        thisInvTransList.Add(invTrans);
                    }
                    else
                    {
                        if (locTrans.ActingBillQty < 0)
                        {
                            //LOC移库(退货)至线旁冲销-寄售	412K	304	//移库入库冲销(退货)	
                            ActingBill actingBill = genericMgr.FindEntityWithNativeSql<ActingBill>("select * from BIL_ActBill WITH(NOLOCK) where Id = ?", locTrans.ActingBill).Single();
                            invTrans.BWART = "412";
                            invTrans.LIFNR = actingBill.Party;
                            invTrans.SOBKZ = "K";
                            invTrans.XBLNR = actingBill.ReceiptNo;
                            invTrans.WERKS = plantTo;
                            invTrans.LGORT = sapLocationFrom;
                            invTrans.UMWRK = plantFrom;
                            invTrans.UMLGO = sapLocationTo;
                        }
                        else if (locTrans.IsConsignment && locTrans.PlanBillQty < 0)
                        {
                            //LOC移库冲销(退货)至线旁-寄售不结算	312K	303	//移库入库(退货)
                            PlanBill planBill = genericMgr.FindEntityWithNativeSql<PlanBill>("select * from BIL_PlanBill WITH(NOLOCK) where Id = ?", locTrans.PlanBill).Single();
                            invTrans.BWART = "312";
                            invTrans.LIFNR = planBill.Party;
                            invTrans.SOBKZ = "K";
                            invTrans.XBLNR = planBill.ReceiptNo;
                            invTrans.WERKS = plantTo;
                            invTrans.LGORT = sapLocationFrom;
                            invTrans.UMWRK = plantFrom;
                            invTrans.UMLGO = sapLocationTo;
                        }
                        else if (locTrans.ActingBillQty > 0)
                        {
                            log.Error(GetTLog(locTrans, "移库/移库退货冲销结算数量小于0。"));
                            invTrans.ErrorId = InvTrans.ErrorIdEnum.E106;
                        }
                        else if (locTrans.IsConsignment && locTrans.PlanBillQty > 0)
                        {
                            log.Error(GetTLog(locTrans, "移库/移库退货冲销寄售数量小于0。"));
                            invTrans.ErrorId = InvTrans.ErrorIdEnum.E107;
                        }
                        else
                        {
                            //LOC移库(退货)至线旁冲销(自有物资)	312	304	//移库入库冲销(退货)			0	
                            invTrans.BWART = "312";
                            if (locTrans.Qty < 0)
                            {
                                invTrans.XBLNR = locTrans.ReceiptNo;
                                invTrans.WERKS = plantTo;
                                invTrans.LGORT = sapLocationFrom;
                                invTrans.UMWRK = plantFrom;
                                invTrans.UMLGO = sapLocationTo;
                            }
                            else
                            {
                                invTrans.XBLNR = locTrans.ReceiptNo;
                                invTrans.WERKS = plantFrom;
                                invTrans.LGORT = sapLocationTo;
                                invTrans.UMWRK = plantTo;
                                invTrans.UMLGO = sapLocationFrom;
                            }
                        }
                        invTrans.OrderNo = locTrans.OrderNo;
                        invTrans.DetailId = locTrans.OrderDetailId;
                        thisInvTransList.Add(invTrans);
                    }
                }
                #endregion

                #region 库内结算
                #region 翻箱结算
                else if (locTrans.TransactionType == CodeMaster.TransactionType.ISS_REP
                    && locTrans.QualityType == CodeMaster.QualityType.Qualified
                    && locTrans.ActingBillQty > 0
                    && plantFrom == plantTo) //&& SihLoc.Contains(sapLocationTo)
                {
                    ActingBill actingBill = genericMgr.FindEntityWithNativeSql<ActingBill>("select * from BIL_ActBill WITH(NOLOCK) where Id = ?", locTrans.ActingBill).Single();
                    invTrans.BWART = "411";
                    invTrans.LIFNR = actingBill.Party;
                    invTrans.SOBKZ = "K";
                    invTrans.XBLNR = actingBill.ReceiptNo;
                    invTrans.WERKS = plantTo;
                    invTrans.LGORT = sapLocationFrom;
                    invTrans.UMWRK = plantFrom;
                    invTrans.UMLGO = sapLocationTo;

                    invTrans.OrderNo = locTrans.OrderNo;
                    invTrans.DetailId = locTrans.OrderDetailId;
                    thisInvTransList.Add(invTrans);
                }
                #endregion

                #region 内部移库结算
                else if ((locTrans.TransactionType == CodeMaster.TransactionType.RCT_TR
                    || locTrans.TransactionType == CodeMaster.TransactionType.RCT_TR_RTN)
                   && locTrans.QualityType == CodeMaster.QualityType.Qualified
                   && locTrans.ActingBillQty > 0
                   && plantFrom == plantTo && sapLocationFrom == sapLocationTo)
                {
                    ActingBill actingBill = genericMgr.FindEntityWithNativeSql<ActingBill>("select * from BIL_ActBill WITH(NOLOCK) where Id = ?", locTrans.ActingBill).Single();
                    invTrans.BWART = "411";
                    invTrans.LIFNR = actingBill.Party;
                    invTrans.SOBKZ = "K";
                    invTrans.XBLNR = actingBill.ReceiptNo;
                    invTrans.WERKS = plantTo;
                    invTrans.LGORT = sapLocationFrom;
                    invTrans.UMWRK = plantFrom;
                    invTrans.UMLGO = sapLocationTo;

                    invTrans.OrderNo = locTrans.OrderNo;
                    invTrans.DetailId = locTrans.OrderDetailId;
                    thisInvTransList.Add(invTrans);
                }
                #endregion

                #region 内部移库结算冲销
                else if ((locTrans.TransactionType == CodeMaster.TransactionType.RCT_TR_VOID
                    || locTrans.TransactionType == CodeMaster.TransactionType.RCT_TR_RTN_VOID)
                   && locTrans.QualityType == CodeMaster.QualityType.Qualified
                   && locTrans.ActingBillQty < 0
                   && plantFrom == plantTo && sapLocationFrom == sapLocationTo)
                {
                    ActingBill actingBill = genericMgr.FindEntityWithNativeSql<ActingBill>("select * from BIL_ActBill WITH(NOLOCK) where Id = ?", locTrans.ActingBill).Single();
                    invTrans.BWART = "412";
                    invTrans.LIFNR = actingBill.Party;
                    invTrans.SOBKZ = "K";
                    invTrans.XBLNR = actingBill.ReceiptNo;
                    invTrans.WERKS = plantTo;
                    invTrans.LGORT = sapLocationFrom;
                    invTrans.UMWRK = plantFrom;
                    invTrans.UMLGO = sapLocationTo;

                    invTrans.OrderNo = locTrans.OrderNo;
                    invTrans.DetailId = locTrans.OrderDetailId;
                    thisInvTransList.Add(invTrans);
                }
                #endregion
                #endregion

                #region 库内结算冲销
                else if (locTrans.TransactionType == CodeMaster.TransactionType.ISS_REP
                   && locTrans.QualityType == CodeMaster.QualityType.Qualified
                   && locTrans.ActingBillQty < 0
                   && plantFrom == plantTo) //&& SihLoc.Contains(sapLocationTo)
                {
                    ActingBill actingBill = genericMgr.FindEntityWithNativeSql<ActingBill>("select * from BIL_ActBill WITH(NOLOCK) where Id = ?", locTrans.ActingBill).Single();
                    invTrans.BWART = "412";
                    invTrans.LIFNR = actingBill.Party;
                    invTrans.SOBKZ = "K";
                    invTrans.XBLNR = actingBill.ReceiptNo;
                    invTrans.WERKS = plantTo;
                    invTrans.LGORT = sapLocationFrom;
                    invTrans.UMWRK = plantFrom;
                    invTrans.UMLGO = sapLocationTo;

                    invTrans.OrderNo = locTrans.OrderNo;
                    invTrans.DetailId = locTrans.OrderDetailId;
                    thisInvTransList.Add(invTrans);
                }
                #endregion

                #endregion

                #region 检验
                #region 报验
                //如果不传322的话，会出现这种情况101K+INSMK=2成功,411K失败,如果判定不合格350K,但411K就一直卡住了
                else if (locTrans.TransactionType == CodeMaster.TransactionType.RCT_INP
                    || locTrans.TransactionType == CodeMaster.TransactionType.RCT_ISL)
                {
                    invTrans.BWART = "322";
                    invTrans.WERKS = plantFrom;
                    invTrans.LGORT = sapLocationFrom;
                    invTrans.UMLGO = sapLocationTo;
                    invTrans.XABLN = locTrans.OrderNo;//记录报验单号，防止跨报验单汇总
                    if (locTrans.IsConsignment && locTrans.PlanBillQty > 0)
                    {
                        PlanBill planBill = genericMgr.FindEntityWithNativeSql<PlanBill>("select * from BIL_PlanBill WITH(NOLOCK) where Id = ?", locTrans.PlanBill).Single();
                        invTrans.LIFNR = planBill.Party;
                        invTrans.SOBKZ = "K";
                        invTrans.XBLNR = planBill.ReceiptNo;
                    }

                    invTrans.OrderNo = locTrans.OrderNo;
                    invTrans.DetailId = locTrans.OrderDetailId;
                    thisInvTransList.Add(invTrans);
                }
                #endregion

                #region 待验状态移库
                else if ((locTrans.TransactionType == CodeMaster.TransactionType.RCT_TR
                    || locTrans.TransactionType == CodeMaster.TransactionType.RCT_TR_RTN)
                   && locTrans.QualityType == CodeMaster.QualityType.Inspect
                   && plantFrom == plantTo && sapLocationFrom != sapLocationTo)//&& SihLoc.Contains(sapLocationTo)
                {
                    if (locTrans.IsConsignment && locTrans.PlanBillQty > 0)
                    {
                        PlanBill planBill = genericMgr.FindEntityWithNativeSql<PlanBill>("select * from BIL_PlanBill WITH(NOLOCK) where Id = ?", locTrans.PlanBill).Single();
                        invTrans.BWART = "323";
                        invTrans.LIFNR = planBill.Party;
                        invTrans.SOBKZ = "K";
                        invTrans.XBLNR = planBill.ReceiptNo;
                    }
                    else
                    {
                        invTrans.BWART = "323";
                    }
                    invTrans.WERKS = plantTo;
                    invTrans.LGORT = sapLocationFrom;
                    invTrans.UMWRK = plantFrom;
                    invTrans.UMLGO = sapLocationTo;

                    invTrans.OrderNo = locTrans.OrderNo;
                    invTrans.DetailId = locTrans.OrderDetailId;
                    thisInvTransList.Add(invTrans);
                }
                #endregion

                #region 报验移库冲销
                else if (locTrans.TransactionType == CodeMaster.TransactionType.RCT_TR_VOID
                   && locTrans.QualityType == CodeMaster.QualityType.Inspect
                   && plantFrom == plantTo && sapLocationFrom != sapLocationTo)//&& SihLoc.Contains(sapLocationTo)
                {
                    if (locTrans.IsConsignment && locTrans.PlanBillQty < 0)
                    {
                        PlanBill planBill = genericMgr.FindEntityWithNativeSql<PlanBill>("select * from BIL_PlanBill WITH(NOLOCK) where Id = ?", locTrans.PlanBill).Single();
                        invTrans.BWART = "324";
                        invTrans.LIFNR = planBill.Party;
                        invTrans.SOBKZ = "K";
                        invTrans.XBLNR = planBill.ReceiptNo;
                    }
                    else
                    {
                        invTrans.BWART = "324";
                    }
                    invTrans.WERKS = plantTo;
                    invTrans.LGORT = sapLocationFrom;
                    invTrans.UMWRK = plantFrom;
                    invTrans.UMLGO = sapLocationTo;

                    invTrans.OrderNo = locTrans.OrderNo;
                    invTrans.DetailId = locTrans.OrderDetailId;
                    thisInvTransList.Add(invTrans);
                }
                #endregion

                #region 判定合格
                ////质检到非限制的转帐	321	506	//检验合格入库			0	
                ////质检到非限制的转帐-寄售	321	506	//检验合格入库			1
                else if (locTrans.TransactionType == CodeMaster.TransactionType.RCT_INP_QDII)
                {
                    //InspectResult inspectResult = genericMgr.FindEntityWithNativeSql<InspectResult>("select * from INP_InspectResult WITH(NOLOCK) where Id = ?", locTrans.OrderDetailId).Single();
                    //ReceiptDetail receiptDetail = genericMgr.FindEntityWithNativeSql<ReceiptDetail>("select * from ORD_RecDet_1 WITH(NOLOCK) where RecNo = ? and Seq = ?", new object[] { inspectResult.ReceiptNo, inspectResult.ReceiptDetailSequence }).SingleOrDefault();
                    //LocationTransaction recLocTrans = genericMgr.FindEntityWithNativeSql<LocationTransaction>("select * from VIEW_LocTrans WITH(NOLOCK) where RecDetId = ? and TransType in (?, ?)", new object[] { receiptDetail.Id, com.Sconit.CodeMaster.TransactionType.RCT_PO, com.Sconit.CodeMaster.TransactionType.RCT_SL })[0];
                    invTrans.BWART = "321";
                    invTrans.WERKS = plantFrom;
                    invTrans.LGORT = sapLocationFrom;
                    invTrans.UMLGO = sapLocationTo;
                    invTrans.XABLN = locTrans.OrderNo;//记录报验单号，防止跨报验单汇总
                    if (locTrans.IsConsignment && locTrans.PlanBillQty > 0)
                    {
                        PlanBill planBill = genericMgr.FindEntityWithNativeSql<PlanBill>("select * from BIL_PlanBill WITH(NOLOCK) where Id = ?", locTrans.PlanBill).Single();
                        invTrans.LIFNR = planBill.Party;
                        invTrans.SOBKZ = "K";
                        invTrans.XBLNR = planBill.ReceiptNo;
                    }

                    invTrans.OrderNo = locTrans.OrderNo;
                    invTrans.DetailId = locTrans.OrderDetailId;
                    thisInvTransList.Add(invTrans);
                    //因为现在在收货时不再传322，,而是101或101K以后直接sap转待验
                    //所以入库时如果为寄售或寄售结算时，报验合格需要传321K
                    //if (recLocTrans.PlanBill != 0 || recLocTrans.ActingBill != 0)
                    //{
                    //    PlanBill planBill = recLocTrans.PlanBill != 0 ? genericMgr.FindEntityWithNativeSql<PlanBill>("select * from BIL_PlanBill WITH(NOLOCK) where Id = ?", recLocTrans.PlanBill).Single() : null;
                    //    ActingBill actBill = recLocTrans.ActingBill != 0 ? genericMgr.FindEntityWithNativeSql<ActingBill>("select * from BIL_ActBill WITH(NOLOCK) where Id = ?", recLocTrans.ActingBill).Single() : null;

                    //    //对于采购收货来说planbill或actbill一定有且两者中只能有一个存在值
                    //    if (planBill != null)
                    //    {
                    //        invTrans.SOBKZ = "K";
                    //        invTrans.LIFNR = planBill.Party;
                    //        invTrans.XBLNR = planBill.ReceiptNo;
                    //    }
                    //    else if (actBill != null && actBill.BillTerm != CodeMaster.OrderBillTerm.ReceivingSettlement)
                    //    {
                    //        invTrans.SOBKZ = "K";
                    //        invTrans.LIFNR = actBill.Party;
                    //        invTrans.XBLNR = actBill.ReceiptNo;
                    //    }
                    //}

                    //invTrans.OrderNo = locTrans.OrderNo;
                    //invTrans.DetailId = locTrans.OrderDetailId;
                    //thisInvTransList.Add(invTrans);
                }
                //质检到冻结	350		//检验不合格入库			0	
                //质检到冻结-寄售	350		//检验不合格入库			1
                #endregion

                #region 判定不合格
                else if (locTrans.TransactionType == CodeMaster.TransactionType.RCT_INP_REJ)
                {
                    //InspectResult inspectResult = genericMgr.FindEntityWithNativeSql<InspectResult>("select * from INP_InspectResult WITH(NOLOCK) where Id = ?", locTrans.OrderDetailId).Single();
                    //InspectDetail inspectDetail = genericMgr.FindEntityWithNativeSql<InspectDetail>("select * from INP_InspectDet WITH(NOLOCK) where Id = ?", inspectResult.InspectDetailId).SingleOrDefault();

                    //if (inspectDetail.IpDetailSequence == 0 && inspectDetail.ReceiptDetailSequence == 0)
                    //    invTrans.BWART = "344";
                    //else
                    invTrans.BWART = "350";
                    invTrans.WERKS = plantFrom;
                    invTrans.LGORT = sapLocationFrom;
                    invTrans.UMLGO = sapLocationTo;
                    invTrans.XABLN = locTrans.OrderNo;//记录报验单号，防止跨报验单汇总
                    if (locTrans.IsConsignment && locTrans.PlanBillQty > 0)
                    {
                        PlanBill planBill = genericMgr.FindEntityWithNativeSql<PlanBill>("select * from BIL_PlanBill WITH(NOLOCK) where Id = ?", locTrans.PlanBill).Single();
                        invTrans.LIFNR = planBill.Party;
                        invTrans.SOBKZ = "K";
                        invTrans.XBLNR = planBill.ReceiptNo;
                    }

                    invTrans.OrderNo = locTrans.OrderNo;
                    invTrans.DetailId = locTrans.OrderDetailId;
                    thisInvTransList.Add(invTrans);
                }
                #endregion

                #region 不合格品移库
                else if ((locTrans.TransactionType == CodeMaster.TransactionType.RCT_TR
                    || locTrans.TransactionType == CodeMaster.TransactionType.RCT_TR_RTN)
                   && locTrans.QualityType == CodeMaster.QualityType.Reject
                   && plantFrom == plantTo && sapLocationFrom != sapLocationTo)//&& SihLoc.Contains(sapLocationTo)
                {
                    if (locTrans.IsConsignment && locTrans.PlanBillQty > 0)
                    {
                        PlanBill planBill = genericMgr.FindEntityWithNativeSql<PlanBill>("select * from BIL_PlanBill WITH(NOLOCK) where Id = ?", locTrans.PlanBill).Single();
                        invTrans.BWART = "325";
                        invTrans.LIFNR = planBill.Party;
                        invTrans.SOBKZ = "K";
                        invTrans.XBLNR = planBill.ReceiptNo;
                    }
                    else
                    {
                        invTrans.BWART = "325";
                    }
                    invTrans.WERKS = plantTo;
                    invTrans.LGORT = sapLocationFrom;
                    invTrans.UMWRK = plantFrom;
                    invTrans.UMLGO = sapLocationTo;

                    invTrans.OrderNo = locTrans.OrderNo;
                    invTrans.DetailId = locTrans.OrderDetailId;
                    thisInvTransList.Add(invTrans);
                }
                #endregion

                #region 不合格品移库冲销
                else if (locTrans.TransactionType == CodeMaster.TransactionType.RCT_TR_VOID
                   && locTrans.QualityType == CodeMaster.QualityType.Reject
                   && plantFrom == plantTo && sapLocationFrom != sapLocationTo)//&& SihLoc.Contains(sapLocationTo)
                {
                    if (locTrans.IsConsignment && locTrans.PlanBillQty < 0)
                    {
                        PlanBill planBill = genericMgr.FindEntityWithNativeSql<PlanBill>("select * from BIL_PlanBill WITH(NOLOCK) where Id = ?", locTrans.PlanBill).Single();
                        invTrans.BWART = "326";
                        invTrans.LIFNR = planBill.Party;
                        invTrans.SOBKZ = "K";
                        invTrans.XBLNR = planBill.ReceiptNo;
                    }
                    else
                    {
                        invTrans.BWART = "326";
                    }
                    invTrans.WERKS = plantTo;
                    invTrans.LGORT = sapLocationFrom;
                    invTrans.UMWRK = plantFrom;
                    invTrans.UMLGO = sapLocationTo;

                    invTrans.OrderNo = locTrans.OrderNo;
                    invTrans.DetailId = locTrans.OrderDetailId;
                    thisInvTransList.Add(invTrans);
                }
                #endregion

                #region 让步使用
                //冻结库存到非限制	343	510	//让步使用入库			0	
                //冻结库存到非限制-寄售	343	510	//让步使用入库			1		
                else if (locTrans.TransactionType == CodeMaster.TransactionType.RCT_INP_CCS)
                {
                    invTrans.BWART = "343";
                    invTrans.WERKS = plantFrom;
                    invTrans.LGORT = sapLocationFrom;
                    invTrans.UMLGO = sapLocationTo;
                    if (locTrans.IsConsignment && locTrans.PlanBillQty > 0)
                    {
                        PlanBill planBill = genericMgr.FindEntityWithNativeSql<PlanBill>("select * from BIL_PlanBill WITH(NOLOCK) where Id = ?", locTrans.PlanBill).Single();
                        invTrans.LIFNR = planBill.Party;
                        invTrans.SOBKZ = "K";
                        invTrans.XBLNR = planBill.ReceiptNo;
                    }

                    invTrans.OrderNo = locTrans.OrderNo;
                    invTrans.DetailId = locTrans.OrderDetailId;
                    thisInvTransList.Add(invTrans);
                }
                #endregion
                #endregion

                #region 寄售物料的销售出库和冲销要传411K/412K
                else if (locTrans.TransactionType == CodeMaster.TransactionType.ISS_SO
                || locTrans.TransactionType == CodeMaster.TransactionType.ISS_SO_VOID)
                {
                    if (locTrans.PlanBill != 0)
                    {
                        PlanBill planBill = genericMgr.FindEntityWithNativeSql<PlanBill>("select * from BIL_PlanBill WITH(NOLOCK) where Id = ?", locTrans.PlanBill).Single();
                        invTrans.LIFNR = planBill.Party;
                        invTrans.XBLNR = planBill.ReceiptNo;
                        invTrans.WERKS = plantFrom;
                        invTrans.LGORT = sapLocationFrom;
                        invTrans.UMWRK = plantFrom;
                        invTrans.UMLGO = sapLocationTo;

                        invTrans.OrderNo = locTrans.OrderNo;
                        invTrans.DetailId = locTrans.OrderDetailId;

                        //销售出库PlanBill小于0为结算，大于0为反结算
                        if (locTrans.PlanBillQty < 0)
                        {
                            invTrans.BWART = "411";
                            invTrans.SOBKZ = "K";
                        }
                        else
                        {
                            invTrans.BWART = "412";
                            invTrans.SOBKZ = "K";
                        }
                        thisInvTransList.Add(invTrans);
                    }
                }
                #endregion

                #region 计划外出库的结算已经考虑，如果还有其他未知的结算
                else if (locTrans.ActingBillQty != 0
                    && locTrans.TransactionType != CodeMaster.TransactionType.ISS_UNP
                    && locTrans.TransactionType != CodeMaster.TransactionType.ISS_UNP_VOID)
                {
                    ActingBill actingBill = genericMgr.FindEntityWithNativeSql<ActingBill>("select * from BIL_ActBill WITH(NOLOCK) where Id = ?", locTrans.ActingBill).Single();
                    //invTrans.BWART = "411";
                    invTrans.LIFNR = actingBill.Party;
                    //invTrans.SOBKZ = "K";
                    invTrans.XBLNR = actingBill.ReceiptNo;
                    invTrans.WERKS = plantTo;
                    invTrans.LGORT = sapLocationFrom;
                    invTrans.UMWRK = plantFrom;
                    invTrans.UMLGO = sapLocationTo;
                    invTrans.ErrorId = InvTrans.ErrorIdEnum.E108;

                    invTrans.OrderNo = locTrans.OrderNo;
                    invTrans.DetailId = locTrans.OrderDetailId;
                    thisInvTransList.Add(invTrans);
                }
                #endregion

                #region 记录没有导入的库存事务
                else
                {
                    //记录不要导入的
                    //log.Debug(GetTLog(locTrans, "NotNeedExchange"));
                }
                #endregion

                #region 其他字段 locTrans=>invTrans
                foreach (InvTrans thisInvTrans in thisInvTransList)
                {
                    ////TCode
                    thisInvTrans.TCODE = GetTcode(tcodeMoveTypes, thisInvTrans.BWART);
                    ////凭证日期	
                    thisInvTrans.BLDAT = locTrans.CreateDate.ToString("yyyyMMdd");
                    ////过账日期	
                    thisInvTrans.BUDAT = locTrans.EffectiveDate.ToString("yyyyMMdd");
                    ////物料号码	
                    thisInvTrans.MATNR = locTrans.Item;
                    ////数量	
                    thisInvTrans.ERFMG = Math.Abs(locTrans.Qty);
                    ////单位	
                    thisInvTrans.ERFME = locTrans.Uom;
                }
                #endregion

                ((List<InvTrans>)invTransList).AddRange(thisInvTransList);
            }
            #endregion

            #region 移动类型汇总
            IList<InvTrans> groupedInvTransList = (from invTrans in invTransList
                                                   group invTrans by new
                                                   {
                                                       TCODE = invTrans.TCODE,   //TCODE
                                                       BWART = invTrans.BWART,   //移动类型
                                                       BLDAT = invTrans.BLDAT,   //凭证日期
                                                       BUDAT = invTrans.BUDAT,   //过账日期
                                                       EBELN = invTrans.EBELN,   //PO号码 采购单或计划协议号
                                                       EBELP = invTrans.EBELP,   //PO行号 采购单或计划协议行号
                                                       LIFNR = invTrans.LIFNR,   //厂商代码 供应商
                                                       WERKS = invTrans.WERKS,   //工厂代码
                                                       LGORT = invTrans.LGORT,   //库存地点 采购：目的库位代码 移库：来源库位代码
                                                       SOBKZ = invTrans.SOBKZ,   //寄售K/但采购寄售收货和冲销除外(!101/!102)
                                                       MATNR = invTrans.MATNR,   //物料号
                                                       ERFME = invTrans.ERFME,   //单位
                                                       UMLGO = invTrans.UMLGO,   //收货地点 移库目的库位/质检
                                                       GRUND = invTrans.GRUND,   //移动原因 采购和冲销
                                                       KOSTL = invTrans.KOSTL,   //成本中心 质量部破坏性抽检领用
                                                       XBLNR = invTrans.XBLNR,   //出货通知 回执对账用
                                                       RSNUM = invTrans.RSNUM,   //预留号 部门领用物料-配件
                                                       RSPOS = invTrans.RSPOS,   //预留行号 部门领用物料-配件
                                                       INSMK = invTrans.INSMK,   //库存类型 3：冻结 2：质检 空：非限制使用 LES只会用到3和空
                                                       XABLN = invTrans.XABLN,   //送货单号 ASN
                                                       AUFNR = invTrans.AUFNR,   //内部订单 计划外出入库 有内部订单号的部门领用（技术中心计划外领用）
                                                       UMMAT = invTrans.UMMAT,   //收货物料 物料替换
                                                       UMWRK = invTrans.UMWRK,   //收货工厂 跨工厂移库
                                                       POSID = invTrans.POSID,   //WBS元素 计划外出入库 有内部订单号的部门领用（技术中心计划外领用）
                                                       KZEAR = invTrans.KZEAR,   //生产收货最终确认标识
                                                       CHARG = invTrans.CHARG,   //SAP批次号
                                                       ErrorId = invTrans.ErrorId, //错误代码
                                                   } into gj
                                                   select new InvTrans
                                                   {
                                                       TCODE = gj.Key.TCODE,   //TCODE
                                                       BWART = gj.Key.BWART,   //移动类型
                                                       BLDAT = gj.Key.BLDAT,   //凭证日期
                                                       BUDAT = gj.Key.BUDAT,   //过账日期
                                                       EBELN = gj.Key.EBELN,   //PO号码 采购单或计划协议号
                                                       EBELP = gj.Key.EBELP,   //PO行号 采购单或计划协议行号
                                                       LIFNR = gj.Key.LIFNR,   //厂商代码 供应商
                                                       WERKS = gj.Key.WERKS,   //工厂代码
                                                       LGORT = gj.Key.LGORT,   //库存地点 采购：目的库位代码 移库：来源库位代码
                                                       SOBKZ = gj.Key.SOBKZ,   //寄售K/但采购寄售收货和冲销除外(!101/!102)
                                                       OLD = "I",
                                                       MATNR = gj.Key.MATNR,   //物料号
                                                       ERFMG = gj.Sum(g => g.ERFMG),  //数量
                                                       ERFME = gj.Key.ERFME,   //单位
                                                       UMLGO = gj.Key.UMLGO,   //收货地点 移库目的库位/质检
                                                       GRUND = gj.Key.GRUND,   //移动原因 采购和冲销
                                                       KOSTL = gj.Key.KOSTL,   //成本中心 质量部破坏性抽检领用
                                                       XBLNR = gj.Key.XBLNR,   //出货通知 回执对账用
                                                       RSNUM = gj.Key.RSNUM,   //预留号 部门领用物料-配件
                                                       RSPOS = gj.Key.RSPOS,   //预留行号 部门领用物料-配件
                                                       //FRBNR = fRBNR,//WMS号
                                                       //SGTXT = (sGTXT++).ToString(),//WMS行号
                                                       INSMK = gj.Key.INSMK,   //库存类型 3：冻结 2：质检 空：非限制使用 LES只会用到3和空
                                                       XABLN = gj.Key.XABLN,   //送货单号 ASN
                                                       AUFNR = gj.Key.AUFNR,   //内部订单 计划外出入库 有内部订单号的部门领用（技术中心计划外领用）
                                                       UMMAT = gj.Key.UMMAT,   //收货物料 物料替换
                                                       UMWRK = gj.Key.UMWRK,   //收货工厂 跨工厂移库
                                                       POSID = gj.Key.POSID,   //WBS元素 计划外出入库 有内部订单号的部门领用（技术中心计划外领用）
                                                       KZEAR = gj.Key.KZEAR,   //生产收货最终确认标识
                                                       CHARG = gj.Key.CHARG,   //SAP批次号
                                                       ErrorId = gj.Key.ErrorId, //错误代码
                                                       InvTransList = gj.ToList(),
                                                       OrderNo = gj.First().OrderNo,
                                                       DetailId = gj.First().DetailId,
                                                   }).ToList();
            #endregion

            #region 记录映射表
            string fRBNR = this.GenerateLocTransNo();  //WMS号
            int sGTXT = 1;                             //WMS行号

            IList<InvLoc> invLocList = new List<InvLoc>();
            foreach (InvTrans groupedInvTrans in groupedInvTransList)
            {
                groupedInvTrans.FRBNR = fRBNR;
                groupedInvTrans.SGTXT = (sGTXT++).ToString();
                groupedInvTrans.Status = Entity.SAP.StatusEnum.Pending;

                #region 记录数据关系
                foreach (InvTrans subInvTrans in groupedInvTrans.InvTransList)
                {
                    InvLoc invLoc = new InvLoc();
                    invLoc.SourceType = (int)InvLoc.SourceTypeEnum.LocTrans;
                    invLoc.SourceId = subInvTrans.LocTransId;
                    invLoc.FRBNR = groupedInvTrans.FRBNR;
                    invLoc.SGTXT = groupedInvTrans.SGTXT;
                    invLoc.CreateDate = dateTimeNow;
                    invLoc.CreateUser = SecurityContextHolder.Get().Code;
                    invLoc.BWART = groupedInvTrans.BWART;

                    invLocList.Add(invLoc);
                }
                #endregion
            }

            InsertInvTrans(groupedInvTransList, invLocList, false);
            #endregion

            #region 更新tableIndex
            tableIndex.Id = transList.Last().Id;
            tableIndex.LastModifyDate = DateTime.Now;
            this.UpdateSiSap(tableIndex);
            this.genericMgr.FlushSession();
            #endregion
        }

        [Transaction(TransactionMode.Requires)]
        public void CallSapTransService(IList<InvTrans> invTransList, IList<ErrorMessage> errorMessageList)
        {

            if (invTransList == null)
            {
                return;
            }

            DateTime dateTimeNow = DateTime.Now;
            List<ZSMIGO> zSMIGOList = new List<ZSMIGO>();

            foreach (var invTrans in invTransList)
            {
                var zSMIGO = Mapper.Map<InvTrans, ZSMIGO>(invTrans);
                zSMIGO.ERFMGSpecified = true;
                zSMIGOList.Add(zSMIGO);
            }

            //zSMIGOList.Add(zSMIGO);
            MI_LESService lesService = new MI_LESService();
            lesService.Credentials = base.Credentials;
            lesService.Timeout = base.TimeOut;
            lesService.Url = ReplaceSAPServiceUrl(lesService.Url);

            ZSMIGORT[] zSMIGORTArray = lesService.MI_LES(zSMIGOList.ToArray());

            if (zSMIGORTArray != null && zSMIGORTArray.Length > 0)
            {
                foreach (var zSMIGOR in zSMIGORTArray)
                {
                    try
                    {
                        var invTrans = invTransList.Where(i => i.FRBNR == zSMIGOR.FRBNR && i.SGTXT == zSMIGOR.SGTXT).Single();
                        if (zSMIGOR.MTYPE == "S")
                        {
                            invTrans.Status = Entity.SAP.StatusEnum.Success;
                            invTrans.ErrorMessage = zSMIGOR.MBLNR;
                        }
                        else
                        {
                            invTrans.Status = Entity.SAP.StatusEnum.Fail;
                            invTrans.ErrorId = InvTrans.ErrorIdEnum.E201;
                            invTrans.ErrorMessage = zSMIGOR.MSTXT;
                            invTrans.ErrorCount++;
                            //string errMessgage = this.GetTLog(zSMIGOR, "移动类型传输失败，" + zSMIGOR.MSTXT + "。");
                            //log.Error(errMessgage);
                            //errorMessageList.Add(new ErrorMessage
                            //{
                            //    Template = NVelocityTemplateRepository.TemplateEnum.ImportSapMoveType_UpdateInvTransFail,
                            //    Message = errMessgage,
                            //});
                        }

                        UpdateSiSap(invTrans);

                        var transCallBack = Mapper.Map<ZSMIGORT, TransCallBack>(zSMIGOR);
                        transCallBack.CreateDate = dateTimeNow;

                        CreateSiSap(transCallBack);
                    }
                    catch (Exception ex)
                    {
                        this.genericMgr.CleanSession();
                        string errMessgage = this.GetTLog(zSMIGOR, "更新InvTrans状态失败");
                        log.Error(errMessgage, ex);
                        errorMessageList.Add(new ErrorMessage
                        {
                            Template = NVelocityTemplateRepository.TemplateEnum.ImportSapMoveType_UpdateInvTransFail,
                            Message = errMessgage,
                            Exception = ex
                        });
                    }
                }
            }

        }

        //对于计划外出库如果出库的是寄售库存，那么转换移动类型时要补做411K，冲销则要补做412K
        [Transaction(TransactionMode.Requires)]
        public void MiscOrder2InvTrans(TableIndex tableIndex, MiscOrderMaster miscOrderMaster,
            IList<MiscOrderDetail> miscOrderDetailList, IList<MiscOrderLocationDetail> miscOrderLocationDetailList,
            List<ErrorMessage> errorMessageList, int batchNo, IList<object[]> tcodeMoveTypes, IList<Entity.MD.Region> regionList, IList<Entity.MD.Location> locationList)
        {
            IList<InvTrans> invTransList = new List<InvTrans>();
            IList<InvLoc> invLocList = new List<InvLoc>();

            if (miscOrderLocationDetailList != null)
            {
                string fRBNR = this.GenerateMiscOrderNo();
                foreach (var miscOrderLocationDetail in miscOrderLocationDetailList)
                {
                    MiscOrderDetail miscOrderDetail = miscOrderDetailList.Where(det => det.Id == miscOrderLocationDetail.MiscOrderDetailId).Single();
                    InvTrans invTrans = new InvTrans();
                    invTrans.BatchNo = batchNo;
                    invTrans.TCODE = GetTcode(tcodeMoveTypes, miscOrderMaster.MoveType);

                    ////移动类型	
                    if (miscOrderMaster.Status == CodeMaster.MiscOrderStatus.Cancel)
                    {
                        invTrans.BWART = miscOrderMaster.CancelMoveType;
                    }
                    else
                    {
                        invTrans.BWART = miscOrderMaster.MoveType;
                    }
                    ////凭证日期	
                    invTrans.BLDAT = miscOrderMaster.CreateDate.ToString("yyyyMMdd");
                    ////过账日期	
                    invTrans.BUDAT = miscOrderMaster.EffectiveDate.ToString("yyyyMMdd");
                    ////PO号码	
                    //invTrans.EBELN = miscOrderMaster.MiscOrderNo;
                    ////PO行数	
                    //invTrans.EBELP = miscOrderLocationDetail.MiscOrderDetailId.ToString();
                    ////DN号码	
                    //invTrans.VBELN =miscOrderLocationDetail.
                    ////DN行数	
                    //invTrans.POSNR =miscOrderLocationDetail.
                    //string location = miscOrderLocationDetail.

                    if (invTrans.BWART == "301" || invTrans.BWART == "302" ||
                        invTrans.BWART == "303" || invTrans.BWART == "304" ||
                        invTrans.BWART == "305" || invTrans.BWART == "306")
                    {
                        if (miscOrderMaster.Type == CodeMaster.MiscOrderType.GI)//出库
                        {
                            //例子
                            // 301/302 对于从江北发往双桥
                            // 工厂WERKS:0084,   库位LGORT:1000,  收货工厂UMWRK:0085,   收货地点UMLGO:F80X
                            // 跨工厂移库 来源工厂
                            invTrans.WERKS = GetPlant(regionList, miscOrderMaster.Region);//0084
                            ////库存地点	
                            invTrans.LGORT = GetSapLocation(locationList, miscOrderLocationDetail.Location);//1000
                            ////跨工厂移库 收货地点	
                            invTrans.UMLGO = miscOrderMaster.ReceiveLocation;//0085
                            ////收货工厂	
                            invTrans.UMWRK = miscOrderMaster.DeliverRegion;//F80X
                        }
                        else
                        {
                            //例子:从双桥发往江北的
                            //计划外入库 区域Region:LOC(0084),发货库位ReceiveLocation:F80X,发货工厂DeliverRegion:0085,库位Loc:LOC(1000)
                            //SAP字段   工厂WERKS:0085,   库位LGORT:F80X,  收货工厂UMWRK:0084,   收货地点UMLGO:1000
                            ////跨工厂移库 来源工厂
                            invTrans.WERKS = miscOrderMaster.DeliverRegion;//0085
                            ////库存地点	
                            invTrans.LGORT = miscOrderMaster.ReceiveLocation;//F80X
                            ////收货工厂	
                            invTrans.UMWRK = GetPlant(regionList, miscOrderMaster.Region);//0084
                            ////跨工厂移库 收货地点	
                            invTrans.UMLGO = GetSapLocation(locationList, miscOrderLocationDetail.Location);//1000
                        }
                    }
                    else
                    {
                        ////跨工厂移库 来源工厂
                        invTrans.WERKS = GetPlant(regionList, miscOrderMaster.Region);
                        ////库存地点	
                        invTrans.LGORT = GetSapLocation(locationList, miscOrderLocationDetail.Location);
                    }

                    ////操作类型	
                    invTrans.OLD = "I";
                    ////物料号码	
                    invTrans.MATNR = miscOrderLocationDetail.Item;
                    ////数量	
                    invTrans.ERFMG = Math.Abs(miscOrderLocationDetail.Qty); //取绝对值
                    ////单位	
                    invTrans.ERFME = miscOrderLocationDetail.Uom;
                    ////移动原因	
                    invTrans.GRUND = miscOrderMaster.Note;
                    ////成本中心	
                    invTrans.KOSTL = miscOrderMaster.CostCenter != null ? this.GenerateSapCostCenter(miscOrderMaster.CostCenter) : null;
                    ////WBS
                    invTrans.POSID = miscOrderMaster.WBS;
                    ////根据MiscOrderDetail是否记录供应商来判断是否要记录K
                    if (miscOrderDetail.ManufactureParty != null && miscOrderDetail.ManufactureParty != string.Empty)
                    {
                        invTrans.LIFNR = miscOrderDetail.ManufactureParty;
                        invTrans.SOBKZ = "K";
                    }
                    else
                    {
                        invTrans.LIFNR = miscOrderMaster.ManufactureParty;
                    }
                    ////WMS号码	
                    invTrans.FRBNR = fRBNR; //如果是联合主键
                    ////WMS行数	
                    invTrans.SGTXT = miscOrderLocationDetail.Id.ToString();
                    ////库存类型	
                    //invTrans.INSMK =miscOrderLocationDetail.
                    ////送货单号	
                    //invTrans.XABLN = miscOrderLocationDetail.IpNo;
                    ////内部订单
                    if (invTrans.BWART == "261" || invTrans.BWART == "262")
                        invTrans.AUFNR = miscOrderDetail.SapOrderNo;
                    else
                        invTrans.AUFNR = miscOrderMaster.ReferenceNo;
                    ////收货物料	
                    //invTrans.UMMAT =miscOrderLocationDetail.
                    invTrans.XBLNR = miscOrderMaster.MiscOrderNo;

                    invTrans.OrderNo = miscOrderMaster.MiscOrderNo;
                    invTrans.DetailId = miscOrderLocationDetail.Id;

                    invTransList.Add(invTrans);

                    #region 计划外出库产生结算
                    if (miscOrderLocationDetail.ActingBill > 0
                        //退货类的寄售出库（zr1,zr2,zr5,zr6）不应该产生或者冲销结算
                        && !NotSettleBillBWARTArray.Contains(invTrans.BWART.ToUpper()))
                    {
                        #region 计划外出库，补做411K
                        if (miscOrderMaster.Status == CodeMaster.MiscOrderStatus.Close)
                        {
                            ActingBill actingBill = genericMgr.FindEntityWithNativeSql<ActingBill>("select * from BIL_ActBill WITH(NOLOCK) where Id = ?", miscOrderLocationDetail.ActingBill).Single();

                            var invTrans411 = Mapper.Map<InvTrans, InvTrans>(invTrans);
                            invTrans411.BWART = "411";
                            invTrans411.SOBKZ = "K";
                            invTrans411.LIFNR = actingBill.Party;
                            invTrans411.XBLNR = actingBill.ReceiptNo;
                            invTrans411.SGTXT = invTrans411.SGTXT + "K";

                            CreateSiSap(invTrans411);

                            invTransList.Add(invTrans411);
                            invTransList.Add(invTrans);
                        }
                        #endregion

                        #region 计划外出库冲销，补做412K
                        else
                        {
                            ActingBill actingBill = genericMgr.FindEntityWithNativeSql<ActingBill>("select * from BIL_ActBill WITH(NOLOCK) where Id = ?", miscOrderLocationDetail.ActingBill).Single();

                            var invTrans412 = Mapper.Map<InvTrans, InvTrans>(invTrans);
                            invTrans412.BWART = "412";
                            invTrans412.SOBKZ = "K";
                            invTrans412.LIFNR = actingBill.Party;
                            invTrans412.XBLNR = actingBill.ReceiptNo;
                            invTrans412.SGTXT = invTrans412.SGTXT + "K";

                            CreateSiSap(invTrans412);

                            invTransList.Add(invTrans);
                            invTransList.Add(invTrans412);
                        }
                        #endregion
                    }
                    #endregion

                    #region 101/102
                    //302 301 工厂WERKS:0085,   库位LGORT:F80X,  收货工厂UMWRK:0084,   收货地点UMLGO:1000
                    if (invTrans.LGORT == "F80X" && (invTrans.BWART == "301" || invTrans.BWART == "302")
                        && invTrans.UMWRK == "0084" && invTrans.WERKS == "0085")
                    {
                        /*
                        对于101 收货，需要进行判断，如果ASN中的特殊字符标示 F80XBJ=’X’ 则在收货传接口时候，需要传递如下两条接口信息。
                        1）基于ASN的101收货
                        2）跨工厂转储：移动类型301 收货工厂（新增的接口字段）字段为0085。出货通知字段维护ASN，以便于追踪。
                        对于F80XBJ=空的，与原接口操作保持一致
                        301/302 工厂WERKS:0085,   库位LGORT:F80X,  收货工厂UMWRK:0084,   收货地点UMLGO:1000
                         */
                        //todo加101双桥收货
                        //var miscOrderDetail = genericMgr.FindById<MiscOrderDetail>(miscOrderLocationDetail.MiscOrderDetailId);
                        InvTrans newInvTrans = Mapper.Map<InvTrans, InvTrans>(invTrans);
                        //预留号码	
                        newInvTrans.EBELN = miscOrderDetail.ReserveNo;
                        //预留行数	
                        newInvTrans.EBELP = miscOrderDetail.ReserveLine;

                        newInvTrans.LIFNR = miscOrderDetail.ManufactureParty;
                        newInvTrans.UMWRK = null;
                        newInvTrans.XBLNR = miscOrderMaster.MiscOrderNo;
                        newInvTrans.SGTXT = "0" + miscOrderLocationDetail.Id.ToString();
                        newInvTrans.OrderNo = miscOrderMaster.MiscOrderNo;
                        newInvTrans.DetailId = miscOrderLocationDetail.Id;
                        //invTrans
                        if (invTrans.BWART == "301")
                        {
                            /*
                              移动类型	凭证日期	过账日期	PO号码	PO行数	DN号码	DN行数	厂商代码	工厂代码	库存地点	特殊标志	物料号码	数量	单位	收货地点	移动原因	成本中心	出货通知	预留号码	预留行数	WMS号码	WMS行号	操作类型	库存类型	送货单号	内部订单	收货物料	收货工厂
                              301	20111227	20111227						0085	F80X		99	3	ST	1000			10048151			16674261	10048151						0084
                             */
                            newInvTrans.BWART = "101";
                            invTransList.Insert(0, newInvTrans);//注意顺序
                        }
                        else
                        {
                            /*
                            WMS号码	移动类型	凭证日期	过账日期	PO号码	PO行数	DN号码	DN行数	厂商代码	工厂代码	库存地点	特殊标志	物料号码	数量	单位	收货地点	移动原因	成本中心	出货通知	预留号码	预留行数	WMS号码	WMS行号	操作类型	库存类型	送货单号	内部订单	收货物料	收货工厂
                            16674271	302	20111228	20111228						0085	F80X		99	3	ST	1000			10048151			16674271	10048151						0084
                            16674272	102	20111228	20111228	5500004144	10			1000000645	0085	F80X		99	3	ST	F80X			10048151			16674272	10048151	O		10048151			
                            */
                            newInvTrans.BWART = "102";
                            invTransList.Add(newInvTrans);
                        }
                        CreateSiSap(newInvTrans);
                    }
                    #endregion

                    #region 记录数据关系
                    InvLoc invLoc = new InvLoc();
                    invLoc.SourceType = (int)InvLoc.SourceTypeEnum.MiscOrder;
                    invLoc.SourceId = miscOrderLocationDetail.Id;
                    invLoc.FRBNR = invTrans.FRBNR;
                    invLoc.SGTXT = invTrans.SGTXT;
                    invLoc.CreateDate = DateTime.Now;
                    invLoc.CreateUser = SecurityContextHolder.Get().Code;
                    invLoc.BWART = invTrans.BWART;
                    invLocList.Add(invLoc);
                    #endregion
                }

                InsertInvTrans(invTransList, invLocList, true);
            }

            #region 更新TableIndex，记录最后更新日期
            tableIndex.LastModifyDate = miscOrderMaster.LastModifyDate;
            UpdateSiSap<TableIndex>(tableIndex);
            this.genericMgr.FlushSession();
            #endregion
        }

        private void InsertInvTrans(IList<InvTrans> invTransList, IList<InvLoc> invLocList, bool isMiscOrder)
        {
            if (invLocList != null && invLocList.Count > 0)
            {
                User user = SecurityContextHolder.Get();
                StringBuilder sql = new StringBuilder();
                sql.Append("Create table #tempInvLoc(SourceType int,SourceId bigint,FRBNR varchar(16),SGTXT varchar(50),BWART varchar(3));");
                int count = 1;
                foreach (InvLoc invLoc in invLocList)
                {
                    if (count == 1)
                    {
                        sql.Append("insert into #tempInvLoc(SourceType,SourceId,FRBNR,SGTXT,BWART)");
                        sql.Append("values(" + invLoc.SourceType + "," + invLoc.SourceId + ",'" + invLoc.FRBNR + "','" + invLoc.SGTXT + "','" + invLoc.BWART + "')");
                    }
                    else
                    {
                        sql.Append(",(" + invLoc.SourceType + "," + invLoc.SourceId + ",'" + invLoc.FRBNR + "','" + invLoc.SGTXT + "','" + invLoc.BWART + "')");
                    }

                    count++;
                    if (count == 1000)
                    {
                        sql.Append(";");
                        count = 1;
                    }
                }

                if (count < 1000)
                {
                    sql.Append(";");
                }

                if (isMiscOrder)
                {
                    sql.Append(" IF EXISTS(SELECT TOP 1 1 FROM SAP_InvLoc as a inner join #tempInvLoc as b on a.SourceId = b.SourceId and a.SourceType = b.SourceType and a.BWART = b.BWART where b.SourceId > 0)");
                    sql.Append(" BEGIN");
                    sql.Append(" RAISERROR('SourceId重复', 16, 1)");
                    sql.Append(" RETURN");
                    sql.Append(" END;");
                }
                else
                {
                    sql.Append(" IF EXISTS(SELECT TOP 1 1 FROM SAP_InvLoc as a inner join #tempInvLoc as b on a.SourceId = b.SourceId and a.SourceType = b.SourceType where b.SourceId > 0)");
                    sql.Append(" BEGIN");
                    sql.Append(" RAISERROR('SourceId重复', 16, 1)");
                    sql.Append(" RETURN");
                    sql.Append(" END;");
                }

                sql.Append("insert into SAP_InvLoc(SourceType,SourceId,FRBNR,SGTXT,BWART,CreateUser,CreateDate) select SourceType,SourceId,FRBNR,SGTXT,BWART,'" + user.FullName + "',GETDATE() from #tempInvLoc;drop table #tempInvLoc;");
                
                count = 1;
                foreach (InvTrans invTrans in invTransList)
                {
                    if (count == 1)
                    {
                        sql.Append("INSERT INTO SAP_InvTrans(");
                        sql.Append("TCODE,"); // 1
                        sql.Append("BWART,"); // 2
                        sql.Append("BLDAT,"); // 3
                        sql.Append("BUDAT,"); // 4
                        sql.Append("EBELN,"); // 5
                        sql.Append("EBELP,"); // 6
                        sql.Append("VBELN,"); // 7
                        sql.Append("POSNR,"); // 8
                        sql.Append("LIFNR,"); // 9
                        sql.Append("WERKS,"); // 10
                        sql.Append("LGORT,"); // 11
                        sql.Append("SOBKZ,"); // 12
                        sql.Append("MATNR,"); // 13
                        sql.Append("ERFMG,"); // 14
                        sql.Append("ERFME,"); // 15
                        sql.Append("UMLGO,"); // 16
                        sql.Append("GRUND,"); // 17
                        sql.Append("KOSTL,"); // 18
                        sql.Append("XBLNR,"); // 19
                        sql.Append("RSNUM,"); // 20
                        sql.Append("RSPOS,"); // 21
                        sql.Append("FRBNR,"); // 22
                        sql.Append("SGTXT,"); // 23
                        sql.Append("OLD,");   // 24
                        sql.Append("INSMK,"); // 26
                        sql.Append("XABLN,"); // 27
                        sql.Append("AUFNR,"); // 28
                        sql.Append("UMMAT,"); // 29
                        sql.Append("UMWRK,"); // 30
                        sql.Append("POSID,"); // 31
                        sql.Append("CreateDate,");  // 32
                        sql.Append("LastModifyDate,");  // 33
                        sql.Append("Status,"); // 34
                        sql.Append("ErrorCount,"); // 35
                        sql.Append("BatchNo,"); // 36
                        sql.Append("CHARG,");  // 37
                        sql.Append("KZEAR,");  // 38
                        sql.Append("ErrorId,"); // 39
                        sql.Append("ErrorMessage,"); //40
                        sql.Append("OrderNo,"); //41
                        sql.Append("DetailId,"); //42
                        sql.Append("[Version])");//43
                        sql.Append("values(");
                        sql.Append("'" + (invTrans.TCODE != null ? invTrans.TCODE : string.Empty) + "',"); // 1
                        sql.Append("'" + (invTrans.BWART != null ? invTrans.BWART : string.Empty) + "',"); // 2
                        sql.Append("'" + (invTrans.BLDAT != null ? invTrans.BLDAT : string.Empty) + "',"); // 3
                        sql.Append("'" + (invTrans.BUDAT != null ? invTrans.BUDAT : string.Empty) + "',"); // 4
                        sql.Append("'" + (invTrans.EBELN != null ? invTrans.EBELN : string.Empty) + "',"); // 5
                        sql.Append("'" + (invTrans.EBELP != null ? invTrans.EBELP : string.Empty) + "',"); // 6
                        sql.Append("'" + (invTrans.VBELN != null ? invTrans.VBELN : string.Empty) + "',"); // 7
                        sql.Append("'" + (invTrans.POSNR != null ? invTrans.POSNR : string.Empty) + "',"); // 8
                        sql.Append("'" + (invTrans.LIFNR != null ? invTrans.LIFNR : string.Empty) + "',"); // 9
                        sql.Append("'" + (invTrans.WERKS != null ? invTrans.WERKS : string.Empty) + "',"); // 10
                        sql.Append("'" + (invTrans.LGORT != null ? invTrans.LGORT : string.Empty) + "',"); // 11
                        sql.Append("'" + (invTrans.SOBKZ != null ? invTrans.SOBKZ : string.Empty) + "',"); // 12
                        sql.Append("'" + (invTrans.MATNR != null ? invTrans.MATNR : string.Empty) + "',"); // 13
                        sql.Append(invTrans.ERFMG + ","); // 14
                        sql.Append("'" + (invTrans.ERFME != null ? invTrans.ERFME : string.Empty) + "',"); // 15
                        sql.Append("'" + (invTrans.UMLGO != null ? invTrans.UMLGO : string.Empty) + "',"); // 16
                        sql.Append("'" + (invTrans.GRUND != null ? invTrans.GRUND : string.Empty) + "',"); // 17
                        sql.Append("'" + (invTrans.KOSTL != null ? (invTrans.KOSTL.Length > 10 ? invTrans.KOSTL.Substring(0, 10) : invTrans.KOSTL) : string.Empty) + "',"); // 18
                        sql.Append("'" + (invTrans.XBLNR != null ? invTrans.XBLNR : string.Empty) + "',"); // 19
                        sql.Append("'" + (invTrans.RSNUM != null ? invTrans.RSNUM : string.Empty) + "',"); // 20
                        sql.Append("'" + (invTrans.RSPOS != null ? invTrans.RSPOS : string.Empty) + "',"); // 21
                        sql.Append("'" + (invTrans.FRBNR != null ? invTrans.FRBNR : string.Empty) + "',"); // 22
                        sql.Append("'" + (invTrans.SGTXT != null ? invTrans.SGTXT : string.Empty) + "',"); // 23
                        sql.Append("'" + (invTrans.OLD != null ? invTrans.OLD : string.Empty) + "',");   // 24
                        sql.Append("'" + (invTrans.INSMK != null ? invTrans.INSMK : string.Empty) + "',"); // 26
                        sql.Append("'" + (invTrans.XABLN != null ? invTrans.XABLN : string.Empty) + "',"); // 27
                        sql.Append("'" + (invTrans.AUFNR != null ? invTrans.AUFNR : string.Empty) + "',"); // 28
                        sql.Append("'" + (invTrans.UMMAT != null ? invTrans.UMMAT : string.Empty) + "',"); // 29
                        sql.Append("'" + (invTrans.UMWRK != null ? invTrans.UMWRK : string.Empty) + "',"); // 30
                        sql.Append("'" + (invTrans.POSID != null ? invTrans.POSID : string.Empty) + "',"); // 31
                        sql.Append("GETDATE(),");  // 32
                        sql.Append("GETDATE(),");  // 33
                        sql.Append((int)invTrans.Status + ","); // 34
                        sql.Append(invTrans.ErrorCount + ","); // 35
                        sql.Append(invTrans.BatchNo + ","); // 36
                        sql.Append("'" + (invTrans.CHARG != null ? invTrans.CHARG : string.Empty) + "',");  // 37
                        sql.Append("'" + (invTrans.KZEAR != null ? invTrans.KZEAR : string.Empty) + "',");  // 38
                        sql.Append((int)invTrans.ErrorId + ","); // 39
                        sql.Append("'" + (invTrans.ErrorMessage != null ? invTrans.ErrorMessage : string.Empty) + "',"); //40
                        sql.Append("'" + (invTrans.OrderNo != null ? invTrans.OrderNo : string.Empty) + "',"); //41
                        sql.Append(invTrans.DetailId + ","); //42
                        sql.Append("1)");//43
                    }
                    else
                    {
                        sql.Append(",(");
                        sql.Append("'" + (invTrans.TCODE != null ? invTrans.TCODE : string.Empty) + "',"); // 1
                        sql.Append("'" + (invTrans.BWART != null ? invTrans.BWART : string.Empty) + "',"); // 2
                        sql.Append("'" + (invTrans.BLDAT != null ? invTrans.BLDAT : string.Empty) + "',"); // 3
                        sql.Append("'" + (invTrans.BUDAT != null ? invTrans.BUDAT : string.Empty) + "',"); // 4
                        sql.Append("'" + (invTrans.EBELN != null ? invTrans.EBELN : string.Empty) + "',"); // 5
                        sql.Append("'" + (invTrans.EBELP != null ? invTrans.EBELP : string.Empty) + "',"); // 6
                        sql.Append("'" + (invTrans.VBELN != null ? invTrans.VBELN : string.Empty) + "',"); // 7
                        sql.Append("'" + (invTrans.POSNR != null ? invTrans.POSNR : string.Empty) + "',"); // 8
                        sql.Append("'" + (invTrans.LIFNR != null ? invTrans.LIFNR : string.Empty) + "',"); // 9
                        sql.Append("'" + (invTrans.WERKS != null ? invTrans.WERKS : string.Empty) + "',"); // 10
                        sql.Append("'" + (invTrans.LGORT != null ? invTrans.LGORT : string.Empty) + "',"); // 11
                        sql.Append("'" + (invTrans.SOBKZ != null ? invTrans.SOBKZ : string.Empty) + "',"); // 12
                        sql.Append("'" + (invTrans.MATNR != null ? invTrans.MATNR : string.Empty) + "',"); // 13
                        sql.Append(invTrans.ERFMG + ","); // 14
                        sql.Append("'" + (invTrans.ERFME != null ? invTrans.ERFME : string.Empty) + "',"); // 15
                        sql.Append("'" + (invTrans.UMLGO != null ? invTrans.UMLGO : string.Empty) + "',"); // 16
                        sql.Append("'" + (invTrans.GRUND != null ? invTrans.GRUND : string.Empty) + "',"); // 17
                        sql.Append("'" + (invTrans.KOSTL != null ? (invTrans.KOSTL.Length > 10 ? invTrans.KOSTL.Substring(0, 10) : invTrans.KOSTL) : string.Empty) + "',"); // 18
                        sql.Append("'" + (invTrans.XBLNR != null ? invTrans.XBLNR : string.Empty) + "',"); // 19
                        sql.Append("'" + (invTrans.RSNUM != null ? invTrans.RSNUM : string.Empty) + "',"); // 20
                        sql.Append("'" + (invTrans.RSPOS != null ? invTrans.RSPOS : string.Empty) + "',"); // 21
                        sql.Append("'" + (invTrans.FRBNR != null ? invTrans.FRBNR : string.Empty) + "',"); // 22
                        sql.Append("'" + (invTrans.SGTXT != null ? invTrans.SGTXT : string.Empty) + "',"); // 23
                        sql.Append("'" + (invTrans.OLD != null ? invTrans.OLD : string.Empty) + "',");   // 24
                        sql.Append("'" + (invTrans.INSMK != null ? invTrans.INSMK : string.Empty) + "',"); // 26
                        sql.Append("'" + (invTrans.XABLN != null ? invTrans.XABLN : string.Empty) + "',"); // 27
                        sql.Append("'" + (invTrans.AUFNR != null ? invTrans.AUFNR : string.Empty) + "',"); // 28
                        sql.Append("'" + (invTrans.UMMAT != null ? invTrans.UMMAT : string.Empty) + "',"); // 29
                        sql.Append("'" + (invTrans.UMWRK != null ? invTrans.UMWRK : string.Empty) + "',"); // 30
                        sql.Append("'" + (invTrans.POSID != null ? invTrans.POSID : string.Empty) + "',"); // 31
                        sql.Append("GETDATE(),");  // 32
                        sql.Append("GETDATE(),");  // 33
                        sql.Append((int)invTrans.Status + ","); // 34
                        sql.Append(invTrans.ErrorCount + ","); // 35
                        sql.Append(invTrans.BatchNo + ","); // 36
                        sql.Append("'" + (invTrans.CHARG != null ? invTrans.CHARG : string.Empty) + "',");  // 37
                        sql.Append("'" + (invTrans.KZEAR != null ? invTrans.KZEAR : string.Empty) + "',");  // 38
                        sql.Append((int)invTrans.ErrorId + ","); // 39
                        sql.Append("'" + (invTrans.ErrorMessage != null ? invTrans.ErrorMessage : string.Empty) + "',"); //40
                        sql.Append("'" + (invTrans.OrderNo != null ? invTrans.OrderNo : string.Empty) + "',"); //41
                        sql.Append(invTrans.DetailId + ","); //42
                        sql.Append("1)");//43
                    }

                    count++;
                    if (count == 1000)
                    {
                        sql.Append(";");
                        count = 1;
                    }
                }

                if (count < 1000)
                {
                    sql.Append(";");
                }

                this.genericMgr.UpdateWithNativeQuery(sql.ToString());
            }
        }

        private string GetSapLocation(IList<Entity.MD.Location> locationList, string location)
        {
            var q_location = locationList.Where(r => r.Code.ToUpper() == location.ToUpper());
            if (q_location != null && q_location.Count() > 0)
            {
                return q_location.First().SAPLocation;
            }
            return null;
        }

        private string GetPlant(IList<Entity.MD.Region> regionList, string region)
        {
            var q_region = regionList.Where(r => r.Code == region);
            if (q_region != null && q_region.Count() > 0)
            {
                return q_region.First().Plant;
            }
            return null;
        }

        private string GetTcode(IList<object[]> tcodeMoveTypes, string moveType)
        {
            var q_tcodeMoveTypes = tcodeMoveTypes.Where(t => t[0].ToString() == moveType);
            if (q_tcodeMoveTypes != null && q_tcodeMoveTypes.Count() > 0)
            {
                return q_tcodeMoveTypes.ToList()[0][1].ToString();
            }
            return null;
            //throw new Exception("没有找到移动类型为:" + moveType + "对应的TCode");
        }

        private string GenerateLocTransNo()
        {
            string numberSuffix = numberControlMgr.GetNextSequence
                (Entity.BusinessConstants.NUMBERCONTROL_SISAP + Entity.BusinessConstants.NUMBERCONTROL_LOCTRANS);
            if (numberSuffix.Length > Entity.BusinessConstants.DATA_EXCHANGE_ORDER_NO_LENTH)
            {
                throw new Exception("numberSuffix.length > " + Entity.BusinessConstants.DATA_EXCHANGE_ORDER_NO_LENTH);
            }
            numberSuffix = numberSuffix.PadLeft(Entity.BusinessConstants.DATA_EXCHANGE_ORDER_NO_LENTH, '0');
            return (Entity.BusinessConstants.NUMBERCONTROL_LOCTRANS + numberSuffix);
        }

        private string GenerateMiscOrderNo()
        {
            string numberSuffix = numberControlMgr.GetNextSequence
                (Entity.BusinessConstants.NUMBERCONTROL_SISAP + Entity.BusinessConstants.NUMBERCONTROL_MISCORDER);
            if (numberSuffix.Length > Entity.BusinessConstants.DATA_EXCHANGE_ORDER_NO_LENTH)
            {
                throw new Exception("numberSuffix.length > " + Entity.BusinessConstants.DATA_EXCHANGE_ORDER_NO_LENTH);
            }
            numberSuffix = numberSuffix.PadLeft(Entity.BusinessConstants.DATA_EXCHANGE_ORDER_NO_LENTH, '0');
            return (Entity.BusinessConstants.NUMBERCONTROL_MISCORDER + numberSuffix);
        }

        private string GenerateFlushBackNo()
        {
            string numberSuffix = numberControlMgr.GetNextSequence
                (Entity.BusinessConstants.NUMBERCONTROL_SISAP + Entity.BusinessConstants.NUMBERCONTROL_BACKFLUSH);
            if (numberSuffix.Length > Entity.BusinessConstants.DATA_EXCHANGE_ORDER_NO_LENTH)
            {
                throw new Exception("numberSuffix.length > " + Entity.BusinessConstants.DATA_EXCHANGE_ORDER_NO_LENTH);
            }
            numberSuffix = numberSuffix.PadLeft(Entity.BusinessConstants.DATA_EXCHANGE_ORDER_NO_LENTH, '0');
            return (Entity.BusinessConstants.NUMBERCONTROL_BACKFLUSH + numberSuffix);
        }

        //传给sap的成本中心必须10位，如果不足需要再前面补0
        private string GenerateSapCostCenter(string lesCostCenter)
        {
            return lesCostCenter = lesCostCenter.PadLeft(10, '0');
        }

        private string GetRealRegion(string locaiton)
        {
            return genericMgr.FindById<Location>(locaiton).Region;
        }
    }
}
