using System;
using System.Collections.Generic;
using com.Sconit.Entity.INP;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.SCM;
using System.IO;
using com.Sconit.Entity.FIS;

namespace com.Sconit.Service
{
    public interface IOrderMgr
    {
        #region 路线转订单
        OrderMaster TransferFlow2Order(FlowMaster flowMaster, bool isTransferDetail);
        OrderMaster TransferFlow2Order(FlowMaster flowMaster, IList<string> itemCodeList);
        OrderMaster TransferFlow2Order(FlowMaster flowMaster, IList<string> itemCodeList, DateTime effectiveDate);
        OrderMaster TransferFlow2Order(FlowMaster flowMaster, IList<string> itemCodeList, DateTime effectiveDate, bool isTransferDetail);
        #endregion

        #region 订单增删改
        void CreateOrder(OrderMaster orderMaster);
        void CreateOrder(OrderMaster orderMaster, bool expandOrderBomDetail);
        void UpdateOrder(OrderMaster orderMaster);
        void DeleteOrder(string orderNo);
        #endregion

        #region 重新绑定路线
        //IList<OrderBinding> CreateBindOrder(OrderMaster orderMaster);
        void ReCreateBindOrder(OrderBinding orderBinding);
        #endregion

        #region 批量更新订单明细
        IList<OrderDetail> BatchUpdateOrderDetails(string orderNo, IList<OrderDetail> addOrderDetailList, IList<OrderDetail> updateOrderDetailList, IList<OrderDetail> deleteOrderDetailList);
        IList<OrderDetail> AddOrderDetails(string orderNo, IList<OrderDetail> orderDetailList);
        IList<OrderDetail> UpdateOrderDetails(IList<OrderDetail> orderDetailList);
        void DeleteOrderDetails(IList<int> orderDetailIds);
        IList<OrderDetail> UpdateTireOrderDetails(string ordreNo, IList<OrderDetail> orderDetailList);
        #endregion

        #region 批量更新订单绑定
        void BatchUpdateOrderBindings(string orderNo, IList<OrderBinding> addOrderBindingList, IList<OrderBinding> deleteOrderBindingList);
        void AddOrderBindings(string orderNo, IList<OrderBinding> orderBindingList);
        void DeleteOrderBindings(IList<int> orderBindingIds);
        #endregion

        #region 批量更新工序
        void BatchUpdateOrderOperations(int orderDetailId, IList<OrderOperation> addOrderOperationList, IList<OrderOperation> updateOrderOperationList, IList<OrderOperation> deleteOrderOperationList);
        void AddOrderOperations(int orderDetailId, IList<OrderOperation> orderOperationList);
        void UpdateOrderOperations(IList<OrderOperation> orderOperations);
        void DeleteOrderOperations(IList<int> orderOperationIds);
        IList<OrderOperation> ExpandOrderOperation(int orderDetailId);
        #endregion

        #region 批量更新订单Bom
        void BatchUpdateOrderBomDetails(int orderDetailId, IList<OrderBomDetail> addOrderBomDetailList, IList<OrderBomDetail> updateOrderBomDetailList, IList<OrderBomDetail> deleteOrderBomDetailList);
        void AddOrderBomDetails(int orderDetailId, IList<OrderBomDetail> orderBomDetailList);
        void UpdateOrderBomDetails(IList<OrderBomDetail> orderBomDetails);
        void DeleteOrderBomDetails(IList<int> orderBomDetailIds);
        IList<OrderBomDetail> ExpandOrderBomDetail(int orderDetailId);
        object[] ExpandOrderOperationAndBomDetail(int orderDetailId);
        #endregion

        #region 释放订单
        void ReleaseOrder(string orderNo);
        void ReleaseOrder(OrderMaster orderMaster);
        //void ReleaseOrder(OrderMaster orderMaster, bool isCreateBindOrder);
        #endregion

        #region 订单上线
        void StartOrder(string orderNo);
        void StartOrder(OrderMaster orderMaster);
        #endregion

        #region 计划协议发货
        IpMaster ShipScheduleLine(string flow, IList<ScheduleLineInput> scheduleLineInputList);
        #endregion

        #region 订单发货
        IpMaster ShipOrder(IList<OrderDetail> orderDetailList);
        IpMaster ShipOrder(IList<OrderDetail> orderDetailList, DateTime effectiveDate);
        #endregion

        #region 拣货单发货
        ReceiptMaster ShipPickList(string pickListNo, string[] pickListDetailIdArray, string[] shipQtyArray);
        IpMaster ShipPickList(string pickListNo);
        IpMaster ShipPickList(string pickListNo, DateTime effectiveDate);
        IpMaster ShipPickList(PickListMaster pickListMaster);
        IpMaster ShipPickList(PickListMaster pickListMaster, DateTime effectiveDate);
        IpMaster ShipPickList(IList<PickListDetail> pickListDetailList);
        IpMaster ShipPickList(IList<PickListDetail> pickListDetailList, DateTime effectiveDate);
        #endregion

        #region 订单收货
        ReceiptMaster ReceiveOrder(IList<OrderDetail> orderDetailList);
        ReceiptMaster ReceiveOrder(IList<OrderDetail> orderDetailList, DateTime effectiveDate);
        void MakeupReceiveOrder();
        #endregion

        #region 送货单收货
        ReceiptMaster ReceiveIp(IList<IpDetail> ipDetailList);
        ReceiptMaster ReceiveIp(IList<IpDetail> ipDetailList, DateTime effectiveDate);
        ReceiptMaster ReceiveIp(IList<IpDetail> ipDetailList, bool isCheckKitTraceItem, DateTime effectiveDate);
        #endregion

        #region 送货单差异调整
        ReceiptMaster AdjustIpGap(IList<IpDetail> ipDetailList, CodeMaster.IpGapAdjustOption ipGapAdjustOption);
        ReceiptMaster AdjustIpGap(IList<IpDetail> ipDetailList, CodeMaster.IpGapAdjustOption ipGapAdjustOption, DateTime effectiveDate);
        #endregion

        #region 取消订单
        void CancelOrder(string orderNo);
        void CancelOrder(OrderMaster orderMaster);
        #endregion

        #region 关闭订单
        void ManualCloseOrder(string orderNo);
        void ManualCloseOrder(OrderMaster orderMaster);
        void AutoCloseOrder();
        void AutoCloseASN(DateTime dateTime);
        #endregion

        #region 生产单暂停
        void PauseProductOrder(string orderNo, int? pauseOperation);
        void BatchPauseProductOrder(IList<string> orderNoList, int? pauseOperation);
        void BatchPauseProductOrder(string orderNos, int? pauseOp);

        #endregion

        #region 生产单暂停恢复
        void RestartProductOrder(string orderNo, string insertOrderNoBefore, bool isForce);
        void BatchRestartProductOrder(IList<string> orderNoList, string insertOrderNoBefore, bool isForce);
        #endregion

        #region 加载订单
        OrderMaster LoadOrderMaster(string orderNo, bool includeDetail, bool includeOperation, bool includeBomDetail);
        #endregion

        #region 根据投料的条码查找投料的工位
        IList<OrderOperation> FindFeedOrderOperation(string orderNo, string huId);
        #endregion

        #region 路线可用性检查
        void CheckOrder(string orderNo);

        void CheckOrder(OrderMaster orderMaster);
        #endregion

        #region 创建不合格品移库单
        void CreateRejectTransfer(Location location, IList<RejectDetail> rejectDetailList);
        #endregion

        #region 创建不合格品退货单
        void CreateReturnOrder(FlowMaster flowMaster, IList<RejectDetail> rejectDetailList);
        #endregion

        #region 创建待验品移库单
        void CreateInspectTransfer(Location location, IList<InspectDetail> inspectDetailList);
        #endregion

        #region 试制车拉料
        string[] CreateRequisitionList(string orderNo);

        string[] CreateRequisitionList(OrderMaster orderMaster);
        #endregion

        #region 紧急拉料
        string[] CreateEmTransferOrderFromXls(Stream inputStream);
        #endregion

        #region 自由移库
        //string CreateTransferOrderFromXls(Stream inputStream, string regionFromCode, string regionToCode, DateTime effectiveDate);
        string CreateTransferOrderFromXls(string shift, Stream inputStream, string manuCode);
        string CreateTransferOrderFromXls(string shift, Stream inputStream, bool isQuick, bool isReturn);
        //string CreateFreeTransferOrderMaster(string regionFromCode, string regionToCode, IList<OrderDetail> orderDetailList, DateTime effectiveDate);
        string CreateFreeTransferOrderMaster(string shift,string regionFromCode, string regionToCode, IList<OrderDetail> orderDetailList, DateTime effectiveDate,DateTime windowTime, bool isQuick, bool isReturn);
        #endregion

        #region 页面条码移库
        string CreateHuTransferOrder(string flowCode, IList<string> huIdList, DateTime effectiveDate);
        #endregion

        #region 客户化功能
        #region 整车上线
        void StartVanOrder(string orderNo);
        #endregion

        #region 空车上线
        void StartEmptyVanOrder(string prodLine);

        void CancelEmptyVanOrder(string prodLine, int orderSeqId);
        #endregion

        #region 整车/驾驶室/底盘下线
        void ReceiveVanOrder(string orderNo, bool isCheckIssue, bool isCheckItemTrace, bool isForce);
        #endregion
        #endregion

        #region 交货单过账
        void DistributionReceiveOrder(OrderMaster orderMaster);
        #endregion

        #region 手动调用生成DAT
        void ReCreateDat(string ipNo);
        void ReCreateOrderDAT(string orderNo);
        #endregion

        #region 生产单报工
        void ReportOrderOp(int orderOpId, decimal reportQty, decimal scrapQty);
        void ReportOrderOp(int orderOpId, decimal reportQty, decimal scrapQty, DateTime effectiveDate);
        void MakeupReportOrderOp();
        #endregion

        #region 生产单报工取消
        void CancelReportOrderOp(int orderOpReportId);
        void MakeupCancelReportOrderOp();
        #endregion

        #region 生产单关闭
        void CloseProdOrder(string orderNo);
        #endregion

        #region 导入要货单
        string CreateProcurementOrderFromXls(Stream inputStream, string flowCode, string extOrderNo, string refOrderNo,
            DateTime startTime, DateTime windowTime, CodeMaster.OrderPriority priority);
        #endregion

        #region 关键件扫描
        void ScanQualityBarCode(string orderNo, string qualityBarcode, string opRef, int? orderItemTraceId, bool isForce, bool isVI);
        void WithdrawQualityBarCode(string qualityBarcode);
        void ReplaceQualityBarCode(string withdrawQualityBarcode, string scanQualityBarcode);
        #endregion

        #region 驾驶室出库
        void OutCab(string orderNo);
        #endregion

        #region 驾驶室移库并投料
        void TansferCab(string orderNo, string flowCode, string qualityBarcode);
        #endregion

        #region Kit明细修改
        void BatchSeqOrderChange(IList<OrderDetail> orderDetailList, int status);

        void AllSeqOrderChange(int status, OrderDetail orderDetail);
        #endregion

        #region Wms拣货单收货
        void ReceiveWMSIpMaster(WMSDatFile wMSDatFile);
        #endregion

        #region 精益引擎
        void RunLeanEngine();
        #endregion

        #region 批量导入发货
        void BatchImportShipXls(Stream inputStream);
        #endregion

        #region 试制备件批量拉料
        void ImportCreateRequisitionXls(Stream inputStream, DateTime windowTim);
        #endregion

        #region LotNoScan
        void LotNoScan(string opRef, string traceCode, string barCode);
        void LotNoDelete(string barCode);
        #endregion

        #region 整车生产单导入
        void GetCurrentVanOrder(string plant, string sapOrderNo, string prodlLine, string userCode);
        #endregion

        #region 在拣货的时候删除明细
        void DeleteDetailById(int id);
        #endregion

        #region 创建拣货单
        string[] PickShipOrder(string idStr, string qtyStr, string deliveryGroup, bool isAutoReceive);
        #endregion

        #region 销售手工拉料
        string CreateDistritutionRequsiton(string idStr, DateTime WindowTime, com.Sconit.CodeMaster.OrderPriority Priority, IList<OrderDetail> details);
        #endregion

        #region 00:02 转换时间
        DateTime ParseDateTime(string time);
        #endregion
    }
}
