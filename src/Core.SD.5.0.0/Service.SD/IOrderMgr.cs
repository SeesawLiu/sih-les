using System.Collections.Generic;
using System;
using com.Sconit.Entity.SD.ORD;
using com.Sconit.Entity.SD.INV;
using com.Sconit.Entity.FIS;
using com.Sconit.Entity.ACC;

namespace com.Sconit.Service.SD
{
    public interface IOrderMgr
    {
        Entity.SD.ORD.OrderMaster GetOrder(string orderNo, bool includeDetail);

        Entity.SD.ORD.IpMaster GetIp(string ipNo, bool includeDetail);

        Entity.SD.ORD.IpMaster GetIpByWmsIpNo(string wmsIpNo, bool includeDetail);

        Entity.SD.ORD.PickListMaster GetPickList(string pickListNo, bool includeDetail);

        void ShipPickList(string pickListNo);

        Entity.SD.ORD.MiscOrderMaster GetMis(string MisNo);

        void StartPickList(string pickListNo);

        void BatchUpdateMiscOrderDetails(string miscOrderNo, IList<string> addHuIdList);


        Entity.SD.ORD.InspectMaster GetInspect(string inspectNo, bool includeDetail);

        Boolean VerifyOrderCompareToHu(string orderNo, string huId);

        List<string> GetProdLineStation(string orderNo, string huId);

        #region 投料
        //投料到生产线
        //void FeedProdLineRawMaterial(string productLine, string productLineFacility, string[][] huDetails, bool isForceFeed, DateTime? effectiveDate);
        void FeedProdLineRawMaterial(string productLine, string productLineFacility, string location, List<com.Sconit.Entity.SD.INV.Hu> hus, bool isForceFeed, DateTime? effectiveDate);
        //投料到生产单
        //void FeedOrderRawMaterial(string orderNo, string[][] huDetails, bool isForceFeed, DateTime? effectiveDate);
        void FeedOrderRawMaterial(string orderNo, string location, List<com.Sconit.Entity.SD.INV.Hu> hus, bool isForceFeed, DateTime? effectiveDate);
        //KIT投料到生产单
        void FeedKitOrder(string orderNo, string kitOrderNo, bool isForceFeed, DateTime? effectiveDate);
        //生产单投料到生产单
        void FeedProductOrder(string orderNo, string productOrderNo, bool isForceFeed, DateTime? effectiveDate);
        #endregion

        void ReturnOrderRawMaterial(string orderNo, string traceCode, int? operation, string opReference, string[][] huDetails, DateTime? effectiveDate);

        void ReturnProdLineRawMaterial(string productLine, string productLineFacility, string[][] huDetails, DateTime? effectiveDate);

        void DoShipOrder(List<Entity.SD.ORD.OrderDetailInput> orderDetailInputList, DateTime? effDate);

        void DoReceiveOrder(List<Entity.SD.ORD.OrderDetailInput> orderDetailInputList, DateTime? effDate);

        void ReceiveWmsDatFile(IList<WMSDatFile> wmsDatFiles);

        void DoReceiveIp(List<Entity.SD.ORD.IpDetailInput> ipDetailInputList, DateTime? effDate);

        void DoReceiveKit(string kitNo, DateTime? effDate);

        void DoTransfer(Entity.SD.SCM.FlowMaster flowMaster, List<Entity.SD.ORD.OrderDetailInput> orderDetailInputList);

        void DoPickList(List<Entity.SD.ORD.PickListDetailInput> pickListDetailInputList);

        void DoAnDon(List<AnDonInput> anDonInputList, User scanUser);

        void DoKitOrderScanKeyPart(string[][] huDetails, string orderNo);

        AnDonInput GetKanBanCard(string cardNo);

        void DoInspect(List<string> huIdList, DateTime? effDate);

        void DoWorkersWaste(List<string> huIdList, DateTime? effDate);

        void DoRepackAndShipOrder(List<Entity.SD.INV.Hu> huList, DateTime? effDate);

        void DoJudgeInspect(Entity.SD.ORD.InspectMaster inspectMaster, List<string> HuIdList, DateTime? effDate);

        void DoReturnOrder(string flowCode, List<string> huIdList, DateTime? effectiveDate);

        #region 整车生产单上线
        void StartVanOrder(string orderNo);
        #endregion

        #region 整车生产单上线并扫描关键件
        void ScanQualityBarCodeAndStartVanOrder(string orderNo, string qualityBarcode);
        #endregion

        #region 关键件扫描
        void ScanQualityBarCode(string orderNo, string qualityBarcode, string opRef, bool isForce, bool isVI);
        #endregion

        #region 关键件退料
        void WithdrawQualityBarCode(string qualityBarcode);
        #endregion

        #region 关键件替换
        void ReplaceQualityBarCode(string withdrawQualityBarcode, string scanQualityBarcode);
        #endregion

        #region 获取关键件信息
        QualityBarcode GetQualityBarCode(string qualityBarcode);
        #endregion

        #region 驾驶室移库并投料
        void TansferCab(string orderNo, string flowCode, string qualityBarcode);
        #endregion

        #region 整车入库
        void ReceiveVanOrder(string orderNo, bool isCheckIssue, bool isCheckItemTrace);

        void ReceiveVanOrder(string traceCode, string prodLine, bool isCheckIssue, bool isCheckItemTrace);

        #endregion

        #region 获取安吉发货明细
        List<WMSDatFile> GetWMSDatFileByAnJiHuId(string huId);

        List<WMSDatFile> GetWMSDatFileByAnJiSeqOrder(string seqOrder);

        IpDetailInput GetIpDetailInputByPickHu(string pickHu);
        #endregion

        #region LotNoScan
        void LotNoScan(string opRef, string traceCode, string barCode);
        void LotNoDelete(string barCode);
        #endregion

        #region 扫描发动机
        void ScanEngineTraceBarCode(string engineTrace, string traceCode);
        #endregion
    }
}
