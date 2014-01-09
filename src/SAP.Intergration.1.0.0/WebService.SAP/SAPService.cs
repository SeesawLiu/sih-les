namespace com.Sconit.WebService.SAP
{
    using System;
    using System.Collections.Generic;
    using System.Web.Services;
    using System.Web.Services.Protocols;
    using com.Sconit.Entity;
    using com.Sconit.Entity.Exception;
    using com.Sconit.Entity.SAP.ORD;
    using com.Sconit.Service.SAP;

    [WebService(Namespace = "http://com.Sconit.WebService.SAP.SAPService/")]
    public class SAPService : BaseWebService
    {
        #region public properties
        private IMasterDataMgr masterDataMgr { get { return GetService<IMasterDataMgr>(); } }
        private ITransMgr transMgr { get { return GetService<ITransMgr>(); } }
        private IProductionMgr productionMgr { get { return GetService<IProductionMgr>(); } }
        private IReportProdOrderOperationMgr reportProdOrderOperationMgr { get { return GetService<IReportProdOrderOperationMgr>(); } }
        private IProcurementMgr procurementMgr { get { return GetService<IProcurementMgr>(); } }
        private IDistributionMgr distributionMgr { get { return GetService<IDistributionMgr>(); } }
        private ILocationLotDetailMgr locationLotDetailMgr { get { return GetService<ILocationLotDetailMgr>(); } }
        #endregion

        #region 导入SAP物料
        [WebMethod]
        public void ImportItem(List<Entity.SAP.MD.Item> itemList)
        {
            SecurityContextHolder.Set(securityMgr.GetUser("su"));
            masterDataMgr.ImportSAPItem(itemList);
        }
        #endregion

        #region 手工抓取SAP物料
        [WebMethod]
        public void GetItems(string itemCode, string plantCode, string userCode)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));
            masterDataMgr.LoadSAPItems(itemCode, plantCode);
        }
        #endregion

        #region 导入SAP供应商
        [WebMethod]
        public void GetSuppliers(string supplierCode, string userCode)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));
            masterDataMgr.LoadSAPSuppliers(supplierCode);
        }
        #endregion

        #region 导入SAP配额
        [WebMethod]
        public void GetSAPQuota(string itemCode, string plantCode, string userCode)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));
            masterDataMgr.GetSAPQuota(itemCode, plantCode);
        }
        #endregion

        [WebMethod]
        public void ExchangeMoveType(string userCode)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));

            transMgr.ExchangeMoveType();
        }

        [WebMethod]
        public void ReExchangeMoveType(string userCode)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));

            transMgr.ReExchangeMoveType();
        }

        //[WebMethod]
        //void ExchangeStockTake(string stNo, string userCode)
        //{
        //    SecurityContextHolder.Set(securityMgr.GetUser(userCode));
        //    transMgr.ExchangeStockTake(stNo);
        //}

        #region 根据驾驶室出库数量自动创建整车生产单
        [WebMethod]
        public void AutoCreateVanOrder(string prodLine, string userCode)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));
            productionMgr.AutoCreateVanOrder(prodLine);
        }
        #endregion

        #region 整车生产单更新
        [WebMethod]
        public List<string> UpdateVanOrder(string plant, string sapOrderNo, string prodlLine, string userCode)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));
            return (List<string>)productionMgr.UpdateVanOrder(plant, sapOrderNo, prodlLine);
        }
        #endregion

        #region 生产单报工
        [WebMethod]
        public void ReportProdOrderOperation(string userCode)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));

            productionMgr.ReportProdOrderOperation();
        }
        #endregion

        #region 生产单取消报工
        [WebMethod]
        public List<string> CancelReportProdOrderOperation(string AUFNR, string TEXT, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(securityMgr.GetUser(userCode));
                reportProdOrderOperationMgr.CancelReportProdOrderOperation(AUFNR, TEXT);
                return null;
            }
            catch (BusinessException ex)
            {
                IList<string> errorMessages = new List<string>();
                foreach (Message message in ex.GetMessages())
                {
                    errorMessages.Add(message.GetMessageString());
                }
                return ((List<string>)errorMessages);
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format("取消生产报工异常，异常信息：{0}。", ex.Message);
                throw new SoapException(string.Empty, SoapException.ServerFaultCode, errorMessage, ex);
            }
        }
        #endregion

        [WebMethod]
        public List<com.Sconit.Entity.SAP.ORD.OrderDetail> GetProcOrders(string flow, string supplier, string item, string plant, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(securityMgr.GetUser(userCode));
                return procurementMgr.GetProcOrders(flow, supplier, item, plant);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(string.Empty, SoapException.ServerFaultCode, ex.GetMessages()[0].GetMessageString(), ex);
            }
            catch (Exception ex)
            {
                log4net.ILog log = log4net.LogManager.GetLogger("SAP_Procurement");
                log.Error(ex);
                string errorMessage = string.Format("从SAP获取计划协议出现异常，异常信息：{0}。", ex.Message);
                throw new SoapException(string.Empty, SoapException.ServerFaultCode, errorMessage, ex);
            }
        }

        [WebMethod]
        public List<object[]> GetScheduleLineItem(string userCode, string item, string supplier, string plant)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));
            return procurementMgr.GetScheduleLineItem(item, supplier, plant);
        }

        [WebMethod]
        public string AlterDistributionOrder(List<AlterDO> alterDOs)
        {
            SecurityContextHolder.Set(securityMgr.GetUser("su"));
            return distributionMgr.AlterDistributionOrder(alterDOs);
        }

        [WebMethod]
        public void PostDistributionOrder(string userCode)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));
            distributionMgr.PostDistributionOrder();
        }

        [WebMethod]
        public void RePostDistributionOrder(string userCode)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));
            distributionMgr.RePostDistributionOrder();
        }

        [WebMethod]
        public void ReportLocLotDet(string userCode, string ftpServer, int ftpPort, string ftpUser, string ftpPass, string ftpFolder,
                               string localFolder, string localTempFolder)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));
            locationLotDetailMgr.ReportLocationLotDetail(ftpServer, ftpPort, ftpUser, ftpPass, ftpFolder,
                                localFolder, localTempFolder);
        }

        #region 获取非整车生产单
        [WebMethod]
        public List<string> GetProductOrder(string plant, List<string> sapOrderNoList, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(securityMgr.GetUser(userCode));
                return (List<string>)productionMgr.GetProductOrder(plant, sapOrderNoList);
            }
            catch (BusinessException ex)
            {
                List<string> errorMsgList = new List<string>();
                foreach (Message message in ex.GetMessages())
                {
                    errorMsgList.Add(message.GetMessageString());
                }
                return errorMsgList;
            }
            catch (Exception ex)
            {
                List<string> errorMsgList = new List<string>();
                errorMsgList.Add(string.Format("获取SAP非整车生产单失败，错误消息为：{0}。", ex.Message));

                return errorMsgList;
            }
        }

        [WebMethod]
        public List<string> GetProductOrder2(string plant, List<string> sapOrderTypeList, DateOption dateOption, DateTime? dateFrom, DateTime? dateTo, List<string> mrpCtrlList, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(securityMgr.GetUser(userCode));
                return (List<string>)productionMgr.GetProductOrder(plant, sapOrderTypeList, dateOption, dateFrom, dateTo, mrpCtrlList);
            }
            catch (BusinessException ex)
            {
                List<string> errorMsgList = new List<string>();
                foreach (Message message in ex.GetMessages())
                {
                    errorMsgList.Add(message.GetMessageString());
                }
                return errorMsgList;
            }
            catch (Exception ex)
            {
                List<string> errorMsgList = new List<string>();
                errorMsgList.Add(string.Format("获取SAP非整车生产单失败，错误消息为：{0}。", ex.Message));

                return errorMsgList;
            }
        }
        #endregion

        #region 反写SAP计划协议
        [WebMethod]
        public void CreateSAPScheduleLineFromLes(string userCode)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));
            this.procurementMgr.CreateSAPScheduleLineFromLes();
        }

        [WebMethod]
        public void CRSLSummaryFromLes(string userCode)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));
            this.procurementMgr.CRSLSummaryFromLes();
        }
        #endregion

        #region 获取整车生产单
        [WebMethod]
        public void GetCurrentVanOrder(string plant, string sapOrderNo, string prodlLine, string userCode)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));
            productionMgr.GetCurrentVanOrder(plant, sapOrderNo, prodlLine);
        }
        #endregion

        #region 获取CKD生产单
        [WebMethod]
        public List<string> GetCKDProductOrder(string plant, List<string> sapOrderNoList, string sapProdLine, string sapOrderType, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(securityMgr.GetUser(userCode));
                return (List<string>)productionMgr.GetCKDProductOrder(plant, sapOrderNoList, sapProdLine, sapOrderType);
            }
            catch (BusinessException ex)
            {
                List<string> errorMsgList = new List<string>();
                foreach (Message message in ex.GetMessages())
                {
                    errorMsgList.Add(message.GetMessageString());
                }
                return errorMsgList;
            }
            catch (Exception ex)
            {
                List<string> errorMsgList = new List<string>();
                errorMsgList.Add(string.Format("获取CKD生产单失败，错误消息为：{0}。", ex.Message));

                return errorMsgList;
            }
        }

        [WebMethod]
        public List<string> GetSingleCKDProductOrder(string plant, string sapOrderNo, string sapProdLine, string sapOrderType, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(securityMgr.GetUser(userCode));
                IList<string> sapOrderNoList = new List<string>();
                sapOrderNoList.Add(sapOrderNo);
                return (List<string>)productionMgr.GetCKDProductOrder(plant, sapOrderNoList, sapProdLine, sapOrderType);
            }
            catch (BusinessException ex)
            {
                List<string> errorMsgList = new List<string>();
                foreach (Message message in ex.GetMessages())
                {
                    errorMsgList.Add(message.GetMessageString());
                }
                return errorMsgList;
            }
            catch (Exception ex)
            {
                List<string> errorMsgList = new List<string>();
                errorMsgList.Add(string.Format("获取CKD生产单失败，错误消息为：{0}。", ex.Message));

                return errorMsgList;
            }
        }
        #endregion

        #region 自制件物料反冲
        [WebMethod]
        public void BackflushProductionOrder(string userCode)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));
            productionMgr.BackflushProductionOrder();
        }
        #endregion
    }
}
