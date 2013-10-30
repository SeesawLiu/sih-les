namespace com.Sconit.WebService.SD
{
    using System.Collections.Generic;
    using System.Web.Services;
    using System.Web.Services.Protocols;
    using com.Sconit.Entity.Exception;
    using com.Sconit.Entity.SD.INV;
    using com.Sconit.Entity.SD.ORD;
    using com.Sconit.Service.SD;
    using com.Sconit.Entity;
    using com.Sconit.Entity.FIS;
    using System;

    [WebService(Namespace = "http://com.Sconit.WebService.SD.SmartDeviceService/")]
    public class SmartDeviceService : BaseWebService
    {
        #region public methods
        [WebMethod]
        public Entity.SD.ACC.User GetUser(string userCode)
        {
            try
            {
                return sdSecurityMgr.GetUser(userCode);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        #region Get
        [WebMethod]
        public OrderMaster GetOrder(string orderNo, bool includeDetail)
        {
            try
            {
                return this.orderMgr.GetOrder(orderNo, includeDetail);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public List<WMSDatFile> GetWMSDatFileByAnJiHuId(string huId)
        {
            try
            {
                return this.orderMgr.GetWMSDatFileByAnJiHuId(huId);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public List<WMSDatFile> GetWMSDatFileByAnJiSeqOrder(string seqOrder)
        {
            try
            {
                return this.orderMgr.GetWMSDatFileByAnJiSeqOrder(seqOrder);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public Boolean VerifyOrderCompareToHu(string orderNo, string huId)
        {
            try
            {
                return this.orderMgr.VerifyOrderCompareToHu(orderNo,huId);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public Hu GetMaterialInHu(string orderNo, string hu, string station)
        {
            try
            {
                return this.inventoryMgr.GetHu(hu);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public IpMaster GetIp(string ipNo, bool includeDetail)
        {
            try
            {
                return this.orderMgr.GetIp(ipNo, includeDetail);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public IpMaster GetIpByWmsIpNo(string wmsIpNo, bool includeDetail)
        {
            try
            {
                return this.orderMgr.GetIpByWmsIpNo(wmsIpNo, includeDetail);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public PickListMaster GetPickList(string pickListNo, bool includeDetail)
        {
            try
            {
                return this.orderMgr.GetPickList(pickListNo, includeDetail);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public InspectMaster GetInspect(string inspectNo, bool includeDetail)
        {
            try
            {
                return this.orderMgr.GetInspect(inspectNo, includeDetail);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public Hu GetHu(string huId)
        {
            try
            {
                return this.inventoryMgr.GetHu(huId);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public Hu GetDistHu(string huId)
        {
            try
            {
                return this.inventoryMgr.GetDistHu(huId);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public Hu CloneHu(string huId, int qty, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                return this.inventoryMgr.CloneHu(huId, qty);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public MiscOrderMaster GetMisOrder(string misNo)
        {
            try
            {
                return this.orderMgr.GetMis(misNo);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void BatchUpdateMiscOrderDetails(string miscOrderNo,
            List<string> addHuIdList, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.orderMgr.BatchUpdateMiscOrderDetails(miscOrderNo, addHuIdList);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }
        #endregion

        #region Do
        [WebMethod]
        public void DoShipOrder(List<Entity.SD.ORD.OrderDetailInput> orderDetailInputList, DateTime? effDate, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.orderMgr.DoShipOrder(orderDetailInputList, effDate);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void DoPickList(List<Entity.SD.ORD.PickListDetailInput> pickListDetailInputList, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.orderMgr.DoPickList(pickListDetailInputList);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void DoReceiveOrder(List<Entity.SD.ORD.OrderDetailInput> orderDetailInputList, DateTime? effDate, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.orderMgr.DoReceiveOrder(orderDetailInputList, effDate);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void DoReceiveIp(List<Entity.SD.ORD.IpDetailInput> ipDetailInputList, DateTime? effDate, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.orderMgr.DoReceiveIp(ipDetailInputList, effDate);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void DoReceiveKit(string kitNo, DateTime? effDate, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.orderMgr.DoReceiveKit(kitNo, effDate);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public IpDetailInput GetIpDetailInputByPickHu(string pickHu)
        {
            try
            {
                return orderMgr.GetIpDetailInputByPickHu(pickHu);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void ReceiveWMSDatFile(List<WMSDatFile> wmsDatFile,string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                orderMgr.ReceiveWmsDatFile(wmsDatFile);
            }
            catch (BusinessException ex)
            {
                string businessExMsg = string.Empty;
                foreach(Message msg in ex.GetMessages())
                {
                    businessExMsg += msg.GetMessageString() + ";";
                }
                throw new SoapException(businessExMsg, SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void DoPut(List<string> huIdList, string binCode, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.inventoryMgr.DoPut(huIdList, binCode);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void DoPick(List<string> huIdList, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.inventoryMgr.DoPick(huIdList);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void DoPutAway(string huId, string binCode, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.inventoryMgr.DoPutAway(huId, binCode);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void DoPickUp(string huId, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.inventoryMgr.DoPickUp(huId);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void DoPack(List<string> huIdList, string location, DateTime? effDate, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.inventoryMgr.DoPack(huIdList, location, effDate);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void DoUnPack(List<string> huIdList, DateTime? effDate, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.inventoryMgr.DoUnPack(huIdList, effDate);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void DoRePack(List<string> oldHuList, List<string> newHuList, DateTime? effDate, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.inventoryMgr.DoRePack(oldHuList, newHuList, effDate);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }


        [WebMethod]
        public void DoInspect(List<string> huIdList, DateTime? effDate, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.orderMgr.DoInspect(huIdList, effDate);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void DoWorkersWaste(List<string> huIdList, DateTime? effDate, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.orderMgr.DoWorkersWaste(huIdList, effDate);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void DoRepackAndShipOrder(List<Entity.SD.INV.Hu> huList, DateTime? effDate, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.orderMgr.DoRepackAndShipOrder(huList, effDate);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }
        #endregion

        #region MasterData
        [WebMethod]
        public string GetEntityPreference(Entity.SYS.EntityPreference.CodeEnum entityEnum)
        {
            try
            {
                return this.masterDataMgr.GetEntityPreference(entityEnum);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public Entity.SD.MD.Bin GetBin(string binCode)
        {
            try
            {
                return this.masterDataMgr.GetBin(binCode);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public Entity.SD.MD.Location GetLocation(string locationCode)
        {
            try
            {
                return this.masterDataMgr.GetLocation(locationCode);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public Entity.SD.MD.Item GetItem(string itemCode)
        {
            try
            {
                return this.masterDataMgr.GetItem(itemCode);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public DateTime GetEffDate(string date)
        {
            try
            {
                return this.masterDataMgr.GetEffDate(date);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }
        #endregion

        [WebMethod]
        public Entity.SD.SCM.FlowMaster GetFlowMaster(string flowCode, bool includeDetail)
        {
            try
            {
                return this.flowMgr.GetFlowMaster(flowCode, includeDetail);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }
        #endregion

        [WebMethod]
        public void DoTransfer(Entity.SD.SCM.FlowMaster flowMaster, List<Entity.SD.ORD.OrderDetailInput> orderDetailInputList, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.orderMgr.DoTransfer(flowMaster, orderDetailInputList);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void DoTransferCab(string orderNo, string flowCode, string qualityBarCode,string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.orderMgr.TansferCab(orderNo, flowCode, qualityBarCode);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void DoAnDon(List<AnDonInput> anDonInputList, string userCode)
        {
            try
            {
                Entity.ACC.User user = sdSecurityMgr.GetBaseUser(userCode);
                SecurityContextHolder.Set(user);
                this.orderMgr.DoAnDon(anDonInputList, user);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public AnDonInput GetKanBanCard(string cardNo)
        {
            try
            {
                return this.orderMgr.GetKanBanCard(cardNo);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void StartPickList(string pickListNo, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.orderMgr.StartPickList(pickListNo);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void ShipPickList(string pickListNo, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.orderMgr.ShipPickList(pickListNo);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public Entity.SD.INV.StockTakeMaster GetStockTake(string stNo)
        {
            try
            {
                return this.inventoryMgr.GetStockTake(stNo);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void DoStockTake(string stNo, string[][] stockTakeDetails, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.inventoryMgr.DoStockTake(stNo, stockTakeDetails);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void DoJudgeInspect(InspectMaster inspectMaster, List<string> HuIdList, DateTime? effDate, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.orderMgr.DoJudgeInspect(inspectMaster, HuIdList, effDate);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void InventoryFreeze(List<string> huIds, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.inventoryMgr.InventoryFreeze(huIds);         
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void InventoryUnFreeze(List<string> huIds, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.inventoryMgr.InventoryUnFreeze(huIds);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void DoReturnOrder(string flowCode, List<string> huIdList, DateTime? effectiveDate, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.orderMgr.DoReturnOrder(flowCode, huIdList, effectiveDate);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public Hu ResolveHu(string extHuId, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                return this.inventoryMgr.ResolveHu(extHuId);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void ReceiveVanOrder(string orderNo, bool isCheckIssue, bool isCheckItemTrace, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.orderMgr.ReceiveVanOrder(orderNo, isCheckIssue, isCheckItemTrace);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void ReceiveVanOrderTwo(string traceCode, bool isCheckIssue, bool isCheckItemTrace, string userCode,string prodLine)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.orderMgr.ReceiveVanOrder(traceCode,prodLine, isCheckIssue, isCheckItemTrace);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        #region 整车生产单上线
        [WebMethod]
        public void StartVanOrder(string orderNo, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.orderMgr.StartVanOrder(orderNo);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }
        #endregion

        #region 整车生产单上线并扫描关键件
        [WebMethod]
        public void ScanQualityBarCodeAndStartVanOrder(string orderNo, string qualityBarcode, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.orderMgr.ScanQualityBarCodeAndStartVanOrder(orderNo, qualityBarcode);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }
        #endregion

        #region 关键件扫描
        [WebMethod]
        public QualityBarcode GetQualityBarCode(string qualityBarcode)
        {
            try
            {
                return this.orderMgr.GetQualityBarCode(qualityBarcode);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void ScanQualityBarCode(string orderNo, string qualityBarcode, string opRef, bool isForce, bool isVI, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.orderMgr.ScanQualityBarCode(orderNo, qualityBarcode, opRef, isForce, isVI);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void WithdrawQualityBarCode(string qualityBarcode, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.orderMgr.WithdrawQualityBarCode(qualityBarcode);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void ReplaceQualityBarCode(string withdrawQualityBarcode, string scanQualityBarcode, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));
                this.orderMgr.ReplaceQualityBarCode(withdrawQualityBarcode, scanQualityBarcode);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }
        #endregion

        #region 拣货任务
        [WebMethod]
        public void Pick(string pickedhu, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));

                this.pickTaskMgr.Pick(pickedhu, userCode);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void CheckHuOnShip(string pickedhu, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));

                this.pickTaskMgr.CheckHuOnShip(pickedhu, userCode);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public string Ship(List<string> pickedhus, string vehicleno, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));

                return this.pickTaskMgr.Ship(pickedhus, vehicleno, userCode);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public List<com.Sconit.Entity.SD.ORD.PickTask> GetPickerTasks(string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));

                return this.pickTaskMgr.GetPickerTasks(userCode) as List<com.Sconit.Entity.SD.ORD.PickTask>;
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public List<string> GetUnpickedHu(string pickid, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));

                return this.pickTaskMgr.GetUnpickedHu(pickid) as List<string>;
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }
        #endregion

        #region LotNoScan
        [WebMethod]
        public void LotNoScan(string opRef,string traceCode,string barCode, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));

                this.orderMgr.LotNoScan(opRef, traceCode, barCode);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }

        [WebMethod]
        public void LotNoDelete(string barCode, string userCode)
        {
            try
            {
                SecurityContextHolder.Set(sdSecurityMgr.GetBaseUser(userCode));

                this.orderMgr.LotNoDelete(barCode);
            }
            catch (BusinessException ex)
            {
                throw new SoapException(GetBusinessExMessage(ex), SoapException.ServerFaultCode, string.Empty);
            }
        }
        #endregion

        private string GetBusinessExMessage(BusinessException ex)
        {
            string messageString = "";
            IList<Message> messages = ex.GetMessages();
            foreach (Message message in messages)
            {
                messageString += message.GetMessageString() + ";";
            }
            return messageString;
        }
    }
}
