using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Castle.Services.Transaction;
using com.Sconit.Utility;
using com.Sconit.Entity.SAP.ORD;
using com.Sconit.Entity.SCM;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.MD;
using AutoMapper;
using com.Sconit.Service.SAP.MI_LES2SAP_POST_DELIV_DOC;

namespace com.Sconit.Service.SAP.Impl
{
    [Transactional]
    public class DistributionMgrImpl : BaseMgr, IDistributionMgr
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("SAP_Distribution");

        public IOrderMgr orderMgr { get; set; }

        #region AlterDO
        public string AlterDistributionOrder(IList<AlterDO> alterDOs)
        {
            #region Create SISAPDO
            if (alterDOs == null || alterDOs.Count == 0)
            {
                throw new Exception("没有交货单明细。");
            }
            LogDebugTList(alterDOs, "DistributionMgr.AlterDistributionOrder");

            //CreateSiSap(alterDOs);
            var locations = alterDOs.Select(o => o.Location).Distinct();
            if (locations.Count() > 1)
            {
                throw new Exception("一次导入的交货单不能有多个库位。");
            }
            #endregion

            if (alterDOs.Select(t => t.OrderNo).Distinct().Count() > 1)
            {
                throw new Exception("一次导入的交货单号多于一个。");
            }

            try
            {
                var firstAlterDO = alterDOs.First();

                if (firstAlterDO.Action != "X")
                {
                    #region  修改
                    var orderMasterList = this.genericMgr.FindEntityWithNativeSql<Entity.ORD.OrderMaster>
                        ("select * from ORD_OrderMstr_3 where RefOrderNo = ? ", firstAlterDO.OrderNo);
                    if (orderMasterList != null && orderMasterList.Count > 0)
                    {
                        foreach (var orderMaster in orderMasterList)
                        {
                            if (orderMaster.Status != CodeMaster.OrderStatus.Create && orderMaster.Status != CodeMaster.OrderStatus.Submit)
                            {
                                throw new Exception(string.Format("交货单{0}已经发货不能修改。", firstAlterDO.OrderNo));
                            }
                            else
                            {
                                orderMgr.DeleteOrder(orderMaster.OrderNo);
                            }
                        }
                    }
                    #endregion

                    #region 创建交货单
                    string sql = @"select distinct f.* 
                            from scm_flowmstr f
                            inner join md_Location l on f.PartyFrom = l.Region and l.Code=f.LocFrom
                            where f.IsActive = 1 and f.Type = ? and f.PartyTo =? and l.SAPLocation = ? ";

                    var flowMasterList = this.genericMgr.FindEntityWithNativeSql<Entity.SCM.FlowMaster>
                        (sql, new object[] { CodeMaster.OrderType.Distribution, firstAlterDO.KUNAG, firstAlterDO.Location });

                    if (flowMasterList == null || flowMasterList.Count == 0)
                    {
                        if (firstAlterDO.Location == "1000")
                        {
                            return "F";
                        }
                        throw new Exception(string.Format("交货单{0}找不到销售路线。", firstAlterDO.OrderNo));
                    }
                 
                    FlowMaster flowMaster = flowMasterList.OrderBy(f => f.Code).First();
                    IList<Location> locationList = this.genericMgr.FindAll<Location>("from Location where Region = ?", flowMaster.PartyFrom);

                    OrderMaster newOrderMaster = this.orderMgr.TransferFlow2Order(flowMaster, true);
                    foreach (var alterDO in alterDOs)
                    {
                        //不需要用路线明细来创建订单明细
                        //FlowDetail flowDetail = new FlowDetail();
                        //flowDetail.OrderQty = alterDO.Qty;

                        //var item = this.genericMgr.FindById<Entity.MD.Item>(alterDO.Item);
                        //flowDetail.BaseUom = item.Uom;
                        //flowDetail.Item = alterDO.Item;
                        //flowDetail.ExternalSequence = alterDO.Sequence;
                        //flowDetail.UnitCount = item.UnitCount;
                        //flowDetail.Uom = alterDO.Uom;
                        //Location location = locationList.Where(l => l.SAPLocation != null && alterDO.Location != null && l.SAPLocation.Trim() == alterDO.Location.Trim()).FirstOrDefault();
                        //flowDetail.LocationFrom = location != null ? location.Code : flowMaster.LocationFrom;

                        //flowMaster.AddFlowDetail(flowDetail);
                        Location location = locationList.Where(l => l.SAPLocation != null && alterDO.Location != null && l.SAPLocation.Trim() == alterDO.Location.Trim()).FirstOrDefault();
                        var item = this.genericMgr.FindById<Entity.MD.Item>(alterDO.Item);
                        com.Sconit.Entity.ORD.OrderDetail orderDetail = new Entity.ORD.OrderDetail();
                        orderDetail.OrderedQty = alterDO.Qty;
                        orderDetail.RequiredQty = alterDO.Qty;
                        orderDetail.Item = item.Code;
                        orderDetail.ItemDescription = item.Description;
                        orderDetail.ReferenceItemCode = item.ReferenceCode;
                        orderDetail.SAPLocation = alterDO.Location;
                        orderDetail.Uom = alterDO.Uom;
                        orderDetail.UnitCount = item.UnitCount;
                        orderDetail.LocationFrom = location != null ? location.Code : flowMaster.LocationFrom;
                        orderDetail.LocationFromName = location != null ? location.Name : string.Empty;
                        orderDetail.ExternalOrderNo = alterDO.OrderNo;
                        orderDetail.ExternalSequence = alterDO.Sequence.ToString();

                        newOrderMaster.AddOrderDetail(orderDetail);

                    }

                    //OrderMaster newOrderMaster = this.orderMgr.TransferFlow2Order(flowMaster, true);
                    newOrderMaster.ReferenceOrderNo = firstAlterDO.OrderNo;
                    newOrderMaster.ExternalOrderNo = firstAlterDO.ExternalOrderNo;
                    newOrderMaster.StartTime = DateTime.Now;
                    newOrderMaster.WindowTime = firstAlterDO.WindowTime;

                    this.orderMgr.CreateOrder(newOrderMaster);
                    #endregion
                }
                #region Delete
                else
                {
                    var orderMasterList = this.genericMgr.FindAll<Entity.ORD.OrderMaster>
                        ("from OrderMaster as a where a.ReferenceOrderNo = ? ", firstAlterDO.OrderNo);
                    if (orderMasterList != null && orderMasterList.Count > 0)
                    {
                        foreach (var orderMaster in orderMasterList)
                        {
                            if (orderMaster.Status == CodeMaster.OrderStatus.Create)
                            {
                                orderMgr.DeleteOrder(orderMaster.OrderNo);
                            }
                            else if (orderMaster.Status == CodeMaster.OrderStatus.Submit)
                            {
                                orderMgr.CancelOrder(orderMaster);
                            }
                            else
                            {
                                throw new Exception(string.Format("交货单{0}已经执行不能删除。", orderMaster.ReferenceOrderNo));
                            }
                        }
                    }
                }
                #endregion

                //foreach (var alterDO in alterDOs)
                //{
                //    alterDO.Status = Entity.SAP.StatusEnum.Success;
                //    this.UpdateSiSap(alterDO);
                //}
                return "S";
            }
            catch (Exception ex)
            {
                //foreach (var alterDO in alterDOs)
                //{
                //    alterDO.Status = Entity.SAP.StatusEnum.Fail;
                //    alterDO.ErrorCount++;
                //    this.UpdateSiSap(alterDO);
                //}
                log.Error(NVelocityTemplateRepository.TemplateEnum.ImportSapDO_ImportFail, ex);
                string errorMessage = "创建交货单失败。"+ex.Message;

                var errorMessageList = new List<ErrorMessage>();
                errorMessageList.Add(new ErrorMessage
                {
                    Template = NVelocityTemplateRepository.TemplateEnum.ImportSapDO_ImportFail,
                    Message = errorMessage,
                    Exception = ex
                });
                this.SendErrorMessage(errorMessageList);
                return ex.Message;
            }
        }
        #endregion

        #region PostDO
        public void PostDistributionOrder()
        {
            DoPostDistributionOrder();
            DoRePostDistributionOrder();
        }

        private void DoPostDistributionOrder()
        {
            var errorMessageList = new List<ErrorMessage>();
            try
            {
                DateTime dateTimeNow = DateTime.Now;
                var tableIndex = this.genericMgr.FindById<TableIndex>(Entity.BusinessConstants.TABLEINDEX_MI_LES2SAP_POST_DELIV_DOC);
                var receiptMasterList = this.genericMgr.FindEntityWithNativeSql<ReceiptMaster>(@"select * from ORD_RecMstr_3 as r where r.OrderType = ? and r.CreateDate > ? ", 
                    new object[] { CodeMaster.OrderType.Distribution, tableIndex.LastModifyDate });
                var cancelRecMasterList = this.genericMgr.FindEntityWithNativeSql<ReceiptMaster>(@"select * from ORD_RecMstr_3 as r where r.OrderType = ? and r.Status=? and r.LastModifyDate > ? ",
                    new object[] { CodeMaster.OrderType.Distribution, CodeMaster.ReceiptStatus.Cancel, tableIndex.LastModifyDate });
                IList<PostDO> postDOList = new List<PostDO>();
                if (receiptMasterList != null && receiptMasterList.Count > 0)
                {
                    
                    foreach (var receiptMaster in receiptMasterList)
                    {
                        PostDO postDO = new PostDO();
                        postDO.Status = Entity.SAP.StatusEnum.Pending;
                        postDO.ReceiptNo = receiptMaster.ReceiptNo;
                        postDO.ZTCODE = "VL02N";
                        postDO.LastModifyDate = receiptMaster.LastModifyDate;
                        //if (receiptMaster.Status == CodeMaster.ReceiptStatus.Close)
                        //{
                        //    postDO.ZTCODE = "VL02N";
                        //}
                        //else if (receiptMaster.Status == CodeMaster.ReceiptStatus.Cancel)
                        //{
                        //    postDO.ZTCODE = "VL09";
                        //}
                        postDOList.Add(postDO);
                    }
                }

                if (cancelRecMasterList != null && cancelRecMasterList.Count > 0)
                {
                    foreach (var cancelRecMaster in cancelRecMasterList)
                    {
                        PostDO postDO = new PostDO();
                        postDO.Status = Entity.SAP.StatusEnum.Pending;
                        postDO.ReceiptNo = cancelRecMaster.ReceiptNo;
                        postDO.ZTCODE = "VL09";
                        postDO.LastModifyDate = cancelRecMaster.LastModifyDate;
                        postDOList.Add(postDO);
                    }
                }

                if (postDOList.Count > 0)
                {
                    LogDebugTList(postDOList, "DistributionMgr.DoPostDistributionOrder");

                    this.CreateSiSap<PostDO>(postDOList);

                    foreach (PostDO postDO in postDOList.OrderBy(o=>o.LastModifyDate))
                    {
                        try
                        {
                            PostOrder(postDO);

                            if (postDO.Success == "F")
                            {
                                postDO.Status = Entity.SAP.StatusEnum.Fail;
                                postDO.ErrorCount++;
                            }
                            else
                            {
                                postDO.Status = Entity.SAP.StatusEnum.Success;
                            }
                        }
                        catch (Exception ex)
                        {
                            string errorMessage = "交货单过账失败:" + postDO.ReceiptNo;
                            postDO.Result = errorMessage;
                            postDO.ErrorCount = postDO.ErrorCount + 1;
                            postDO.Status = Entity.SAP.StatusEnum.Fail;
                            log.Error(errorMessage, ex);
                            errorMessageList.Add(new ErrorMessage
                            {
                                Template = NVelocityTemplateRepository.TemplateEnum.ImportSapDO_PostFail,
                                Message = errorMessage,
                                Exception = ex
                            });

                            this.genericMgr.CleanSession();
                        }

                        this.UpdateSiSap(postDO);
                    }
                    tableIndex.Id++;
                    tableIndex.LastModifyDate = dateTimeNow;
                    this.UpdateSiSap(tableIndex);
                }
                
            }
            catch (Exception ex)
            {
                log.Error(NVelocityTemplateRepository.TemplateEnum.ImportSapDO_PostFail, ex);
                string errorMessage = "交货单过账失败。";
                errorMessageList.Add(new ErrorMessage
                {
                    Template = NVelocityTemplateRepository.TemplateEnum.ImportSapDO_PostFail,
                    Message = errorMessage,
                    Exception = ex
                });
            }

            this.SendErrorMessage(errorMessageList);
        }

        public void RePostDistributionOrder()
        {
            DoRePostDistributionOrder();
        }

        private void DoRePostDistributionOrder()
        {
            try
            {
                int maxFailCount = int.Parse(this.systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.SAPDataExchangeMaxFailCount));

                var postDOList = this.genericMgr.FindAll<PostDO>("from PostDO as p where p.ErrorCount <= ? and  p.Status = ? order by p.CreateDate asc",
                    new object[] { maxFailCount, Entity.SAP.StatusEnum.Fail });

                LogDebugTList(postDOList, "DistributionMgr.DoRePostDistributionOrder");

                foreach (var postDO in postDOList)
                {
                    try
                    {
                        PostOrder(postDO);
                        if (postDO.Success == "F")
                        {
                            postDO.Status = Entity.SAP.StatusEnum.Fail;
                            postDO.ErrorCount++;
                        }
                        else
                        {
                            postDO.Status = Entity.SAP.StatusEnum.Success;
                        }
                        this.UpdateSiSap(postDO);
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = "交货单重新过账失败:" + postDO.ReceiptNo;
                        postDO.Result = errorMessage;
                        postDO.ErrorCount = postDO.ErrorCount + 1;
                        postDO.Status = Entity.SAP.StatusEnum.Fail;
                        this.UpdateSiSap(postDO);
                        log.Error(errorMessage, ex);

                        this.genericMgr.CleanSession();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("交货单重新过账失败。", ex);
            }
        }
        #endregion

        private void PostOrder(PostDO postDO)
        {
            var receiptDetailList = this.genericMgr.FindEntityWithNativeSql<ReceiptDetail>("select * from ORD_RecDet_3 as rd where rd.RecNo = ?", postDO.ReceiptNo);
            var orderMaster = this.genericMgr.FindById<OrderMaster>(receiptDetailList.First().OrderNo);
            postDO.OrderNo = orderMaster.ReferenceOrderNo;

            List<ZSTM_ZSDDNFLES> zSTM_ZSDDNFLESList = new List<ZSTM_ZSDDNFLES>();
            foreach (var receiptDetail in receiptDetailList)
            {
                if (receiptDetail.ReceivedQty > 0)
                {
                    Location location = this.genericMgr.FindById<Location>(receiptDetail.LocationFrom);

                    ZSTM_ZSDDNFLES zSTM_ZSDDNFLES = new ZSTM_ZSDDNFLES();

                    var orderDetail = this.genericMgr.FindById<com.Sconit.Entity.ORD.OrderDetail>(receiptDetail.OrderDetailId);

                    zSTM_ZSDDNFLES.LGORT = location.SAPLocation;
                    zSTM_ZSDDNFLES.MATNR = receiptDetail.Item;
                    zSTM_ZSDDNFLES.PIKMG = receiptDetail.ReceivedQty;
                    zSTM_ZSDDNFLES.PIKMGSpecified = true;
                    zSTM_ZSDDNFLES.POSNR = orderDetail.ExternalSequence;
                    zSTM_ZSDDNFLES.VBELN = orderDetail.ExternalOrderNo;
                    zSTM_ZSDDNFLES.VRKME = receiptDetail.Uom;
                    zSTM_ZSDDNFLES.WADAT_IST = receiptDetail.CreateDate.ToString("yyyyMMdd");
                    zSTM_ZSDDNFLES.WERKS = "0084";
                    zSTM_ZSDDNFLES.WMSNR = receiptDetail.ReceiptNo;
                    zSTM_ZSDDNFLES.WMSPONR = receiptDetail.Sequence.ToString();
                    zSTM_ZSDDNFLES.ZTCODE = postDO.ZTCODE;
                    zSTM_ZSDDNFLESList.Add(zSTM_ZSDDNFLES);
                }
            }

            #region 调用SAPWebService
            string success = string.Empty;
            ZSTM_ZSDDNFLES[] dELIV_DOCs = zSTM_ZSDDNFLESList.ToArray();
            MI_LES2SAP_POST_DELIV_DOCService mI_LES2SAP_POST_DELIV_DOCService = new MI_LES2SAP_POST_DELIV_DOCService();
            mI_LES2SAP_POST_DELIV_DOCService.Credentials = base.Credentials;
            mI_LES2SAP_POST_DELIV_DOCService.Timeout = base.TimeOut;
            mI_LES2SAP_POST_DELIV_DOCService.Url = ReplaceSAPServiceUrl(mI_LES2SAP_POST_DELIV_DOCService.Url);

            string result = mI_LES2SAP_POST_DELIV_DOCService.MI_LES2SAP_POST_DELIV_DOC(ref dELIV_DOCs, out success);
            #endregion

            postDO.Result = result;
            postDO.Success = success;
        }

        private void LogDebugTList<T>(IList<T> tList, string message)
        {
            foreach (var t in tList)
            {
                log.Debug(GetTLog(t, message));
            }
        }
    }
}
