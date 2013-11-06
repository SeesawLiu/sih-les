using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Castle.Services.Transaction;
using com.Sconit.Entity;
using com.Sconit.Entity.ACC;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.INV;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.VIEW;
using System.IO;
using NHibernate.Type;
using NHibernate;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using com.Sconit.Utility;
using com.Sconit.Entity.CUST;

namespace com.Sconit.Service.Impl
{
    [Transactional]
    public class MiscOrderMgrImpl : BaseMgr, IMiscOrderMgr
    {
        #region 变量
        public IGenericMgr genericMgr { get; set; }
        public INumberControlMgr numberControlMgr { get; set; }
        public ISystemMgr systemMgr { get; set; }
        public IHuMgr huMgr { get; set; }
        public ILocationDetailMgr locationDetailMgr { get; set; }
        public IItemMgr itemMgr { get; set; }
        #endregion

        #region 头明细一起新建保存

        [Transaction(TransactionMode.Requires)]
        public void CreateOrUpdateMiscOrderAndDetails(MiscOrderMaster miscOrderMaster,
            IList<MiscOrderDetail> addMiscOrderDetailList, IList<MiscOrderDetail> updateMiscOrderDetailList, IList<MiscOrderDetail> deleteMiscOrderDetailList)
        {
            if (string.IsNullOrEmpty(miscOrderMaster.MiscOrderNo))
            {
                this.CreateMiscOrder(miscOrderMaster);
                this.BatchUpdateMiscOrderDetails(miscOrderMaster.MiscOrderNo, addMiscOrderDetailList, updateMiscOrderDetailList, deleteMiscOrderDetailList);
                //if (miscOrderMaster.IsQuick)
                //{
                //    this.CloseMiscOrder(miscOrderMaster);
                //}
            }
            else
            {
                this.UpdateMiscOrder(miscOrderMaster);
                BatchUpdateMiscOrderDetails(miscOrderMaster.MiscOrderNo, addMiscOrderDetailList, updateMiscOrderDetailList, deleteMiscOrderDetailList);
            }
        }

        #endregion

        [Transaction(TransactionMode.Requires)]
        public void CreateMiscOrder(MiscOrderMaster miscOrderMaster)
        {
            miscOrderMaster.MiscOrderNo = this.numberControlMgr.GetMiscOrderNo(miscOrderMaster);
            if (string.IsNullOrWhiteSpace(miscOrderMaster.DeliverRegion))
            {
                //非跨工厂移库时，会从Region字段找到对应的Plant填到该字段上面
                miscOrderMaster.DeliverRegion = this.genericMgr.FindById<Region>(miscOrderMaster.Region).Plant;
            }
            this.genericMgr.Create(miscOrderMaster);

            if (miscOrderMaster.MiscOrderDetails != null && miscOrderMaster.MiscOrderDetails.Count > 0)
            {
                BatchUpdateMiscOrderDetails(miscOrderMaster, miscOrderMaster.MiscOrderDetails, null, null);
            }
        }

        [Transaction(TransactionMode.Requires)]
        public void QuickCreateMiscOrder(MiscOrderMaster miscOrderMaster, DateTime effectiveDate)
        {
            miscOrderMaster.MiscOrderNo = this.numberControlMgr.GetMiscOrderNo(miscOrderMaster);
            this.genericMgr.Create(miscOrderMaster);
            this.CreateMiscOrderDetail(miscOrderMaster.MiscOrderDetails, miscOrderMaster.MiscOrderNo);
            this.CloseMiscOrder(miscOrderMaster, effectiveDate);
        }

        [Transaction(TransactionMode.Requires)]
        public void UpdateMiscOrder(MiscOrderMaster miscOrderMaster)
        {
            if (miscOrderMaster.Status != CodeMaster.MiscOrderStatus.Create)
            {
                throw new BusinessException("计划外出入库单{0}的状态为{1}不能修改。",
                      miscOrderMaster.MiscOrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.MiscOrderStatus, ((int)miscOrderMaster.Status).ToString()));
            }
            this.genericMgr.Update(miscOrderMaster);
        }

        [Transaction(TransactionMode.Requires)]
        public void BatchUpdateMiscOrderDetails(string miscOrderNo,
            IList<MiscOrderDetail> addMiscOrderDetailList, IList<MiscOrderDetail> updateMiscOrderDetailList, IList<MiscOrderDetail> deleteMiscOrderDetailList)
        {
            BatchUpdateMiscOrderDetails(this.genericMgr.FindById<MiscOrderMaster>(miscOrderNo), addMiscOrderDetailList, updateMiscOrderDetailList, deleteMiscOrderDetailList);
        }

        [Transaction(TransactionMode.Requires)]
        public void BatchUpdateMiscOrderDetails(MiscOrderMaster miscOrderMaster,
            IList<MiscOrderDetail> addMiscOrderDetailList, IList<MiscOrderDetail> updateMiscOrderDetailList, IList<MiscOrderDetail> deleteMiscOrderDetailList)
        {
            if (miscOrderMaster.Status != CodeMaster.MiscOrderStatus.Create)
            {
                if (miscOrderMaster.Type == CodeMaster.MiscOrderType.GI)
                {
                    throw new BusinessException("计划外出库单{0}的状态为{1}不能修改明细。",
                          miscOrderMaster.MiscOrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.MiscOrderStatus, ((int)miscOrderMaster.Status).ToString()));
                }
                else
                {
                    throw new BusinessException("计划外入库单{0}的状态为{1}不能修改明细。",
                        miscOrderMaster.MiscOrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.MiscOrderStatus, ((int)miscOrderMaster.Status).ToString()));
                }
            }

            #region 新增计划外出入库明细
            if (addMiscOrderDetailList != null && addMiscOrderDetailList.Count > 0)
            {
                #region 获取最大订单明细序号
                string hql = "select max(Sequence) as seq from MiscOrderDetail where MiscOrderNo = ?";
                IList<object> maxSeqList = genericMgr.FindAll<object>(hql, miscOrderMaster.MiscOrderNo);
                int maxSeq = maxSeqList[0] != null ? (int)(maxSeqList[0]) : 0;
                #endregion

                #region 数量处理
                foreach (MiscOrderDetail miscOrderDetail in addMiscOrderDetailList)
                {
                    //Item item = this.genericMgr.FindById<Item>(miscOrderDetail.Item);

                    miscOrderDetail.MiscOrderNo = miscOrderMaster.MiscOrderNo;
                    miscOrderDetail.Sequence = ++maxSeq;
                    //miscOrderDetail.Item = miscOrderDetail.Item;
                    //miscOrderDetail.ItemDescription = item.Description;
                    //miscOrderDetail.ReferenceItemCode = item.ReferenceCode;
                    //miscOrderDetail.Uom = miscOrderDetail.Uom;
                    //miscOrderDetail.BaseUom = item.Uom;
                    //miscOrderDetail.UnitCount = miscOrderDetail.UnitCount;
                    if (miscOrderDetail.Uom != miscOrderDetail.BaseUom)
                    {
                        miscOrderDetail.UnitQty = this.itemMgr.ConvertItemUomQty(miscOrderDetail.Item, miscOrderDetail.BaseUom, 1, miscOrderDetail.Uom);
                    }
                    else
                    {
                        miscOrderDetail.UnitQty = 1;
                    }
                    //miscOrderDetail.ReserveNo = miscOrderDetail.ReserveNo;
                    //miscOrderDetail.ReserveLine = miscOrderDetail.ReserveLine;
                    //miscOrderDetail.Qty = miscOrderDetail.Qty;

                    this.genericMgr.Create(miscOrderDetail);

                    if (miscOrderMaster.MiscOrderDetails == null)
                    {
                        miscOrderMaster.MiscOrderDetails = new List<MiscOrderDetail>();
                    }
                    miscOrderMaster.MiscOrderDetails.Add(miscOrderDetail);
                }
                #endregion
            }
            #endregion

            #region 修改计划外出入库明细
            if (updateMiscOrderDetailList != null && updateMiscOrderDetailList.Count > 0)
            {
                foreach (MiscOrderDetail miscOrderDetail in updateMiscOrderDetailList)
                {
                    if (miscOrderDetail.Uom != miscOrderDetail.BaseUom)
                    {
                        miscOrderDetail.UnitQty = this.itemMgr.ConvertItemUomQty(miscOrderDetail.Item, miscOrderDetail.BaseUom, 1, miscOrderDetail.Uom);
                    }
                    else
                    {
                        miscOrderDetail.UnitQty = 1;
                    }
                    this.genericMgr.Update(miscOrderDetail);
                }
            }
            #endregion

            #region 删除计划外出入库明细
            if (deleteMiscOrderDetailList != null && deleteMiscOrderDetailList.Count > 0)
            {
                #region 数量处理
                foreach (MiscOrderDetail miscOrderDetail in deleteMiscOrderDetailList)
                {
                    //删除locationdet
                    this.genericMgr.Delete("from MiscOrderLocationDetail as l where l.MiscOrderDetailId=" + miscOrderDetail.Id);
                    this.genericMgr.Delete(miscOrderDetail);
                }
                #endregion
            }
            #endregion
        }

        [Transaction(TransactionMode.Requires)]
        public void BatchUpdateMiscOrderDetails(string miscOrderNo,
            IList<string> addHuIdList, IList<string> deleteHuIdList)
        {
            BatchUpdateMiscOrderDetails(this.genericMgr.FindById<MiscOrderMaster>(miscOrderNo), addHuIdList, deleteHuIdList);
        }

        [Transaction(TransactionMode.Requires)]
        public void BatchUpdateMiscOrderDetails(MiscOrderMaster miscOrderMaster,
            IList<string> addHuIdList, IList<string> deleteHuIdList)
        {
            if (miscOrderMaster.Status != CodeMaster.MiscOrderStatus.Create)
            {
                if (miscOrderMaster.Type == CodeMaster.MiscOrderType.GI)
                {
                    throw new BusinessException("计划外出库单{0}的状态为{1}不能修改明细。",
                          miscOrderMaster.MiscOrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.MiscOrderStatus, ((int)miscOrderMaster.Status).ToString()));
                }
                else
                {
                    throw new BusinessException("计划外入库单{0}的状态为{1}不能修改明细。",
                        miscOrderMaster.MiscOrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.MiscOrderStatus, ((int)miscOrderMaster.Status).ToString()));
                }
            }

            TryLoadMiscOrderDetails(miscOrderMaster);
            IList<MiscOrderLocationDetail> miscOrderLocationDetailList = TryLoadMiscOrderLocationDetails(miscOrderMaster);

            #region 新增计划外出入库明细
            if (addHuIdList != null && addHuIdList.Count > 0)
            {
                #region 获取最大订单明细序号
                string hql = "select max(Sequence) as seq from MiscOrderDetail where MiscOrderNo = ?";
                IList maxSeqList = genericMgr.FindAll(hql, miscOrderMaster.MiscOrderNo);
                int maxSeq = maxSeqList != null && maxSeqList.Count > 0 && maxSeqList[0] != null ? (int)maxSeqList[0] : 0;
                #endregion

                #region 条码处理
                #region 明细重复输入校验
                #region 合并新增的HuId和原有的HuId
                IList<string> huIdList = addHuIdList;
                if (miscOrderLocationDetailList != null && miscOrderLocationDetailList.Count > 0)
                {
                    ((List<string>)huIdList).AddRange(miscOrderLocationDetailList.Select(det => det.HuId).ToList());
                }
                #endregion

                #region 检查是否重复
                BusinessException businessException = new BusinessException();
                var groupedHuIds = from huId in huIdList
                                   group huId by huId into result
                                   select new
                                   {
                                       HuId = result.Key,
                                       Count = result.Count()
                                   };

                foreach (var groupedHuId in groupedHuIds.Where(g => g.Count > 1))
                {
                    businessException.AddMessage("重复扫描条码{0}。", groupedHuId.HuId);
                }

                if (businessException.HasMessage)
                {
                    throw businessException;
                }
                #endregion
                #endregion

                if (miscOrderMaster.Type == CodeMaster.MiscOrderType.GI)
                {
                    #region 计划外出库
                    #region 库存占用
                    IList<InventoryOccupy> inventoryOccupyList = (from huId in addHuIdList
                                                                  select new InventoryOccupy
                                                                  {
                                                                      HuId = huId,
                                                                      //Location = miscOrderMaster.Location,  不指定库位
                                                                      QualityType = CodeMaster.QualityType.Qualified,
                                                                      OccupyType = CodeMaster.OccupyType.MiscOrder,
                                                                      OccupyReferenceNo = miscOrderMaster.MiscOrderNo
                                                                  }).ToList();

                    IList<LocationLotDetail> locationLotDetailList = this.locationDetailMgr.InventoryOccupy(inventoryOccupyList);
                    #endregion

                    #region 新增明细
                    foreach (LocationLotDetail locationLotDetail in locationLotDetailList)
                    {
                        MiscOrderDetail matchedMiscOrderDetail = null;

                        #region 明细处理
                        if (miscOrderMaster.MiscOrderDetails != null && miscOrderMaster.MiscOrderDetails.Count > 0)
                        {
                            //查找匹配的明细行
                            matchedMiscOrderDetail = miscOrderMaster.MiscOrderDetails.Where(det => det.Item == locationLotDetail.Item
                                                                                                && det.Uom == locationLotDetail.HuUom
                                                                                                && det.UnitCount == locationLotDetail.UnitCount
                                                                                                && det.Location == locationLotDetail.Location).SingleOrDefault();
                        }

                        if (matchedMiscOrderDetail == null)
                        {
                            //没有找到明细行，新增明细
                            Item item = this.genericMgr.FindById<Item>(locationLotDetail.Item);                                //没有找到匹配的明细行，新增一行
                            matchedMiscOrderDetail = new MiscOrderDetail();

                            matchedMiscOrderDetail.MiscOrderNo = miscOrderMaster.MiscOrderNo;
                            matchedMiscOrderDetail.Sequence = ++maxSeq;
                            matchedMiscOrderDetail.Item = locationLotDetail.Item;
                            matchedMiscOrderDetail.ItemDescription = item.Description;
                            matchedMiscOrderDetail.ReferenceItemCode = item.ReferenceCode;
                            matchedMiscOrderDetail.Uom = locationLotDetail.HuUom;
                            matchedMiscOrderDetail.BaseUom = locationLotDetail.BaseUom;
                            matchedMiscOrderDetail.UnitCount = locationLotDetail.UnitCount;
                            matchedMiscOrderDetail.UnitQty = locationLotDetail.UnitQty;
                            matchedMiscOrderDetail.Location = locationLotDetail.Location;
                            //matchedMiscOrderDetail.ReserveNo = addMiscOrderDetail.ReserveNo;
                            //matchedMiscOrderDetail.ReserveLine = addMiscOrderDetail.ReserveLine;
                            matchedMiscOrderDetail.Qty = locationLotDetail.Qty;

                            this.genericMgr.Create(matchedMiscOrderDetail);

                            miscOrderMaster.MiscOrderDetails.Add(matchedMiscOrderDetail);
                        }
                        else
                        {
                            //找到明细行，更新数量
                            matchedMiscOrderDetail.Qty += locationLotDetail.Qty;
                            this.genericMgr.Update(matchedMiscOrderDetail);
                        }
                        #endregion

                        #region 库存明细新增
                        MiscOrderLocationDetail miscOrderLocationDetail = new MiscOrderLocationDetail();

                        miscOrderLocationDetail.MiscOrderNo = miscOrderMaster.MiscOrderNo;
                        miscOrderLocationDetail.MiscOrderDetailId = matchedMiscOrderDetail.Id;
                        miscOrderLocationDetail.MiscOrderDetailSequence = matchedMiscOrderDetail.Sequence;
                        miscOrderLocationDetail.Item = locationLotDetail.Item;
                        miscOrderLocationDetail.Uom = locationLotDetail.HuUom;
                        miscOrderLocationDetail.HuId = locationLotDetail.HuId;
                        miscOrderLocationDetail.LotNo = locationLotDetail.LotNo;
                        miscOrderLocationDetail.IsCreatePlanBill = false;
                        miscOrderLocationDetail.IsConsignment = locationLotDetail.IsConsignment;
                        miscOrderLocationDetail.PlanBill = locationLotDetail.PlanBill;
                        //#region 查找寄售供应商
                        //if (locationLotDetail.IsConsignment && locationLotDetail.PlanBill.HasValue)
                        //{
                        //    miscOrderLocationDetail.ConsignmentSupplier = this.genericMgr.FindAll<string>("select Party from PartyAddress as pa join pa.Address as a where a.Code in (select BillAddress from PlanBill where Id = ?)", locationLotDetail.PlanBill.Value)[0];
                        //}
                        //#endregion
                        miscOrderLocationDetail.ActingBill = null;
                        miscOrderLocationDetail.QualityType = locationLotDetail.QualityType;
                        miscOrderLocationDetail.IsFreeze = locationLotDetail.IsFreeze;
                        miscOrderLocationDetail.IsATP = locationLotDetail.IsATP;
                        miscOrderLocationDetail.OccupyType = locationLotDetail.OccupyType;
                        miscOrderLocationDetail.OccupyReferenceNo = locationLotDetail.OccupyReferenceNo;
                        miscOrderLocationDetail.Qty = locationLotDetail.Qty;

                        this.genericMgr.Create(miscOrderLocationDetail);
                        #endregion
                    }
                    #endregion
                    #endregion
                }
                else
                {
                    #region 计划外入库
                    #region 检查条码状态
                    IList<HuStatus> huStatusList = this.huMgr.GetHuStatus(addHuIdList);

                    foreach (string huId in addHuIdList)
                    {
                        HuStatus huStatus = huStatusList.Where(h => h.HuId == huId).SingleOrDefault();
                        if (huStatus == null)
                        {
                            businessException.AddMessage("条码{0}不存在。", huId);
                        }
                        else if (huStatus.Status == CodeMaster.HuStatus.Location)
                        {
                            businessException.AddMessage("条码{0}在库位{1}中，不能计划外入库。", huStatus.HuId, huStatus.Location);
                        }
                        else if (huStatus.Status == CodeMaster.HuStatus.Ip)
                        {
                            businessException.AddMessage("条码{0}为库位{1}至库位{2}的在途库存，不能计划外入库。", huStatus.HuId, huStatus.LocationFrom, huStatus.LocationTo);
                        }
                    }

                    if (businessException.HasMessage)
                    {
                        throw businessException;
                    }
                    #endregion

                    #region 新增明细
                    foreach (HuStatus huStatus in huStatusList)
                    {
                        MiscOrderDetail matchedMiscOrderDetail = null;

                        #region 明细处理
                        if (miscOrderMaster.MiscOrderDetails != null && miscOrderMaster.MiscOrderDetails.Count > 0)
                        {
                            //查找匹配的明细行
                            matchedMiscOrderDetail = miscOrderMaster.MiscOrderDetails.Where(det => det.Item == huStatus.Item
                                                                                                && det.Uom == huStatus.Uom
                                                                                                && det.UnitCount == huStatus.UnitCount).SingleOrDefault();
                        }

                        if (matchedMiscOrderDetail == null)
                        {
                            //没有找到明细行，新增明细
                            Item item = this.genericMgr.FindById<Item>(huStatus.Item);                                //没有找到匹配的明细行，新增一行
                            matchedMiscOrderDetail = new MiscOrderDetail();

                            matchedMiscOrderDetail.MiscOrderNo = miscOrderMaster.MiscOrderNo;
                            matchedMiscOrderDetail.Sequence = ++maxSeq;
                            matchedMiscOrderDetail.Item = huStatus.Item;
                            matchedMiscOrderDetail.ItemDescription = item.Description;
                            matchedMiscOrderDetail.ReferenceItemCode = item.ReferenceCode;
                            matchedMiscOrderDetail.Uom = huStatus.Uom;
                            matchedMiscOrderDetail.BaseUom = huStatus.BaseUom;
                            matchedMiscOrderDetail.UnitCount = huStatus.UnitCount;
                            matchedMiscOrderDetail.UnitQty = huStatus.UnitQty;
                            //matchedMiscOrderDetail.Location = 
                            //matchedMiscOrderDetail.ReserveNo = addMiscOrderDetail.ReserveNo;
                            //matchedMiscOrderDetail.ReserveLine = addMiscOrderDetail.ReserveLine;
                            matchedMiscOrderDetail.Qty = huStatus.Qty;

                            this.genericMgr.Create(matchedMiscOrderDetail);

                            miscOrderMaster.MiscOrderDetails.Add(matchedMiscOrderDetail);
                        }
                        else
                        {
                            //找到明细行，更新数量
                            matchedMiscOrderDetail.Qty += huStatus.Qty;
                            this.genericMgr.Update(matchedMiscOrderDetail);
                        }
                        #endregion

                        #region 库存明细新增
                        MiscOrderLocationDetail miscOrderLocationDetail = new MiscOrderLocationDetail();

                        miscOrderLocationDetail.MiscOrderNo = miscOrderMaster.MiscOrderNo;
                        miscOrderLocationDetail.MiscOrderDetailId = matchedMiscOrderDetail.Id;
                        miscOrderLocationDetail.MiscOrderDetailSequence = matchedMiscOrderDetail.Sequence;
                        miscOrderLocationDetail.Item = huStatus.Item;
                        miscOrderLocationDetail.Uom = huStatus.Uom;
                        miscOrderLocationDetail.HuId = huStatus.HuId;
                        miscOrderLocationDetail.LotNo = huStatus.LotNo;
                        miscOrderLocationDetail.IsCreatePlanBill = false;
                        miscOrderLocationDetail.IsConsignment = false;
                        miscOrderLocationDetail.PlanBill = null;
                        miscOrderLocationDetail.ConsignmentSupplier = null;
                        miscOrderLocationDetail.ActingBill = null;
                        miscOrderLocationDetail.QualityType = huStatus.QualityType;
                        miscOrderLocationDetail.IsFreeze = false;
                        miscOrderLocationDetail.IsATP = true;
                        miscOrderLocationDetail.OccupyType = CodeMaster.OccupyType.None;
                        miscOrderLocationDetail.OccupyReferenceNo = null;
                        miscOrderLocationDetail.Qty = huStatus.Qty * huStatus.UnitQty;

                        this.genericMgr.Create(miscOrderLocationDetail);
                        #endregion
                    }
                    #endregion
                    #endregion
                }
                #endregion
            }
            #endregion

            #region 删除计划外出入库明细
            if (deleteHuIdList != null && deleteHuIdList.Count > 0)
            {
                #region 条码处理
                #region 条码是否在计划外出入库单中存在检查
                BusinessException businessException = new BusinessException();
                foreach (string huId in deleteHuIdList)
                {
                    if (miscOrderLocationDetailList == null || miscOrderLocationDetailList.Where(m => m.HuId == huId).Count() == 0)
                    {
                        if (miscOrderMaster.Type == CodeMaster.MiscOrderType.GI)
                        {
                            businessException.AddMessage("条码{0}在计划外出库单{1}中不存在。", huId, miscOrderMaster.MiscOrderNo);
                        }
                        else
                        {
                            businessException.AddMessage("条码{0}在计划外入库单{1}中不存在。", huId, miscOrderMaster.MiscOrderNo);
                        }
                    }
                }

                if (businessException.HasMessage)
                {
                    throw businessException;
                }
                #endregion

                #region 循环删除
                #region 取消占用
                if (miscOrderMaster.Type == CodeMaster.MiscOrderType.GI)
                {
                    this.locationDetailMgr.CancelInventoryOccupy(CodeMaster.OccupyType.MiscOrder, miscOrderMaster.MiscOrderNo, deleteHuIdList);
                }
                #endregion

                foreach (string huId in deleteHuIdList)
                {
                    #region 扣减明细数量，删除库存明细
                    MiscOrderLocationDetail miscOrderLocationDetail = miscOrderLocationDetailList.Where(det => det.HuId == huId).Single();
                    MiscOrderDetail miscOrderDetail = miscOrderMaster.MiscOrderDetails.Where(det => det.Id == miscOrderLocationDetail.MiscOrderDetailId).Single();
                    miscOrderDetail.Qty -= miscOrderLocationDetail.Qty / miscOrderDetail.UnitQty;

                    this.genericMgr.Update(miscOrderDetail);
                    this.genericMgr.Delete(miscOrderLocationDetail);
                    #endregion
                }
                #endregion
                #endregion
            }
            #endregion
        }

        [Transaction(TransactionMode.Requires)]
        public void DeleteMiscOrder(string miscOrderNo)
        {
            this.DeleteMiscOrder(this.genericMgr.FindById<MiscOrderMaster>(miscOrderNo));
        }

        [Transaction(TransactionMode.Requires)]
        public void DeleteMiscOrder(MiscOrderMaster miscOrderMaster)
        {
            if (miscOrderMaster.Status != CodeMaster.MiscOrderStatus.Create)
            {
                if (miscOrderMaster.Type == CodeMaster.MiscOrderType.GI)
                {
                    throw new BusinessException("计划外出库单{0}的状态为{1}不能删除。",
                          miscOrderMaster.MiscOrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.MiscOrderStatus, ((int)miscOrderMaster.Status).ToString()));
                }
                else
                {
                    throw new BusinessException("计划外入库单{0}的状态为{1}不能删除。",
                         miscOrderMaster.MiscOrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.MiscOrderStatus, ((int)miscOrderMaster.Status).ToString()));
                }
            }

            if (miscOrderMaster.IsScanHu)
            {
                IList<MiscOrderLocationDetail> miscOrderLocationDetailList = TryLoadMiscOrderLocationDetails(miscOrderMaster);
                if (miscOrderLocationDetailList != null && miscOrderLocationDetailList.Count > 0)
                {
                    this.genericMgr.Delete<MiscOrderLocationDetail>(miscOrderLocationDetailList);
                }
            }

            IList<MiscOrderDetail> miscOrderDetailList = TryLoadMiscOrderDetails(miscOrderMaster);
            if (miscOrderDetailList != null && miscOrderDetailList.Count > 0)
            {
                this.genericMgr.Delete<MiscOrderDetail>(miscOrderDetailList);
            }

            this.genericMgr.Delete(miscOrderMaster);
        }

        [Transaction(TransactionMode.Requires)]
        public void CloseMiscOrder(string miscOrderNo)
        {
            this.CloseMiscOrder(this.genericMgr.FindById<MiscOrderMaster>(miscOrderNo), DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public void CloseMiscOrder(string miscOrderNo, DateTime effectiveDate)
        {
            this.CloseMiscOrder(this.genericMgr.FindById<MiscOrderMaster>(miscOrderNo), effectiveDate);
        }

        [Transaction(TransactionMode.Requires)]
        public void CloseMiscOrder(MiscOrderMaster miscOrderMaster)
        {
            this.CloseMiscOrder(miscOrderMaster, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public void CloseMiscOrder(MiscOrderMaster miscOrderMaster, DateTime effectiveDate)
        {
            #region 检查
            BusinessException businessException = new BusinessException();
            if (miscOrderMaster.Status != CodeMaster.MiscOrderStatus.Create)
            {
                if (miscOrderMaster.Type == CodeMaster.MiscOrderType.GI)
                {
                    businessException.AddMessage("计划外出库单{0}的状态为{1}不能确认。",
                          miscOrderMaster.MiscOrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.MiscOrderStatus, ((int)miscOrderMaster.Status).ToString()));
                }
                else
                {
                    businessException.AddMessage("计划外入库单{0}的状态为{1}不能确认。",
                         miscOrderMaster.MiscOrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.MiscOrderStatus, ((int)miscOrderMaster.Status).ToString()));
                }
            }

            IList<MiscOrderDetail> miscOrderDetailList = TryLoadMiscOrderDetails(miscOrderMaster);
            if (miscOrderDetailList == null || miscOrderDetailList.Count() == 0)
            {
                if (miscOrderMaster.Type == CodeMaster.MiscOrderType.GI)
                {
                    businessException.AddMessage("计划外出库单{0}明细为空。", miscOrderMaster.MiscOrderNo);
                }
                else
                {
                    businessException.AddMessage("计划外入库单{0}明细为空。", miscOrderMaster.MiscOrderNo);
                }
            }
            else
            {
                foreach (MiscOrderDetail miscOrderDetail in miscOrderDetailList)
                {
                    if (miscOrderDetail.Qty <= 0)
                    {
                        businessException.AddMessage("计划外入库单{0}明细行{1}的数量不能小于0。", miscOrderMaster.MiscOrderNo, miscOrderDetail.Sequence.ToString());
                    }
                }
            }

            if (businessException.HasMessage)
            {
                throw businessException;
            }
            #endregion

            User user = SecurityContextHolder.Get();
            miscOrderMaster.CloseDate = DateTime.Now;
            miscOrderMaster.CloseUserId = user.Id;
            miscOrderMaster.CloseUserName = user.FullName;
            miscOrderMaster.Status = com.Sconit.CodeMaster.MiscOrderStatus.Close;
            this.genericMgr.Update(miscOrderMaster);
            //如果是退货的 直接转为出库
            miscOrderMaster.Type = miscOrderMaster.Type == com.Sconit.CodeMaster.MiscOrderType.Return ? com.Sconit.CodeMaster.MiscOrderType.GI : miscOrderMaster.Type;
            IList<MiscOrderLocationDetail> miscOrderLocationDetailList = TryLoadMiscOrderLocationDetails(miscOrderMaster);

            foreach (MiscOrderDetail miscOrderDetail in miscOrderDetailList.OrderByDescending(det => det.ManufactureParty))
            {
                IList<InventoryTransaction> inventoryTransactionList = this.locationDetailMgr.InventoryOtherInOut(miscOrderMaster, miscOrderDetail, effectiveDate);

                #region 新增、更新订单库存明细
                var groupInventoryTransactionList = from trans in inventoryTransactionList
                                                    group trans by new
                                                    {
                                                        Item = trans.Item,
                                                        HuId = trans.HuId,
                                                        IsCreatePlanBill = trans.IsCreatePlanBill,
                                                        IsConsignment = trans.IsConsignment,
                                                        PlanBill = trans.PlanBill,
                                                        ActingBill = trans.ActingBill,
                                                        QualityType = trans.QualityType,
                                                        IsFreeze = trans.IsFreeze,
                                                        IsATP = trans.IsATP,
                                                        OccupyType = trans.OccupyType,
                                                        OccupyReferenceNo = trans.OccupyReferenceNo,
                                                    } into result
                                                    select new
                                                    {
                                                        Item = result.Key.Item,
                                                        HuId = result.Key.HuId,
                                                        IsCreatePlanBill = result.Key.IsCreatePlanBill,
                                                        IsConsignment = result.Key.IsConsignment,
                                                        PlanBill = result.Key.PlanBill,
                                                        ActingBill = result.Key.ActingBill,
                                                        QualityType = result.Key.QualityType,
                                                        IsFreeze = result.Key.IsFreeze,
                                                        IsATP = result.Key.IsATP,
                                                        OccupyType = result.Key.OccupyType,
                                                        OccupyReferenceNo = result.Key.OccupyReferenceNo,
                                                        Qty = result.Sum(trans => trans.Qty)
                                                    };

                foreach (var groupInventoryTransaction in groupInventoryTransactionList)
                {
                    if (miscOrderMaster.IsScanHu)
                    {
                        #region 条码
                        MiscOrderLocationDetail miscOrderLocationDetail = miscOrderLocationDetailList.Where(m => m.HuId == groupInventoryTransaction.HuId).Single();
                        if (groupInventoryTransaction.ActingBill.HasValue)
                        {
                            miscOrderLocationDetail.IsConsignment = false;
                            miscOrderLocationDetail.PlanBill = null;
                            miscOrderLocationDetail.ActingBill = groupInventoryTransaction.ActingBill;
                        }

                        this.genericMgr.Update(miscOrderLocationDetail);
                        #endregion
                    }
                    else
                    {
                        #region 数量
                        MiscOrderLocationDetail miscOrderLocationDetail = new MiscOrderLocationDetail();

                        miscOrderLocationDetail.MiscOrderNo = miscOrderMaster.MiscOrderNo;
                        miscOrderLocationDetail.MiscOrderDetailId = miscOrderDetail.Id;
                        miscOrderLocationDetail.MiscOrderDetailSequence = miscOrderDetail.Sequence;
                        miscOrderLocationDetail.Item = groupInventoryTransaction.Item;
                        miscOrderLocationDetail.Uom = miscOrderDetail.Uom;
                        //miscOrderLocationDetail.HuId = locationLotDetail.HuId;
                        //miscOrderLocationDetail.LotNo = locationLotDetail.LotNo;
                        miscOrderLocationDetail.IsCreatePlanBill = groupInventoryTransaction.IsCreatePlanBill;
                        miscOrderLocationDetail.IsConsignment = groupInventoryTransaction.IsConsignment;
                        miscOrderLocationDetail.PlanBill = groupInventoryTransaction.PlanBill;
                        //#region 查找寄售供应商
                        //if (inventoryTransaction.IsConsignment && inventoryTransaction.PlanBill.HasValue)
                        //{
                        //miscOrderLocationDetail.ConsignmentSupplier = this.genericMgr.FindAll<string>("select Party from PlanBill where Id = ?", inventoryTransaction.PlanBill.Value).Single();
                        //}
                        //#endregion
                        miscOrderLocationDetail.ActingBill = groupInventoryTransaction.ActingBill;
                        miscOrderLocationDetail.QualityType = groupInventoryTransaction.QualityType;
                        miscOrderLocationDetail.IsFreeze = groupInventoryTransaction.IsFreeze;
                        miscOrderLocationDetail.IsATP = groupInventoryTransaction.IsATP;
                        miscOrderLocationDetail.OccupyType = groupInventoryTransaction.OccupyType;
                        miscOrderLocationDetail.OccupyReferenceNo = groupInventoryTransaction.OccupyReferenceNo;
                        miscOrderLocationDetail.Qty = groupInventoryTransaction.Qty;

                        this.genericMgr.Create(miscOrderLocationDetail);
                        #endregion
                    }
                }
                #endregion
            }
        }

        [Transaction(TransactionMode.Requires)]
        public void CancelMiscOrder(string miscOrderNo)
        {
            CancelMiscOrder(miscOrderNo, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public void CancelMiscOrder(string miscOrderNo, DateTime effectiveDate)
        {
            this.CancelMiscOrder(this.genericMgr.FindById<MiscOrderMaster>(miscOrderNo), effectiveDate);
        }

        [Transaction(TransactionMode.Requires)]
        public void CancelMiscOrder(MiscOrderMaster miscOrderMaster)
        {
            this.CancelMiscOrder(miscOrderMaster, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public void CancelMiscOrder(MiscOrderMaster miscOrderMaster, DateTime effectiveDate)
        {
            if (miscOrderMaster.Status != CodeMaster.MiscOrderStatus.Close)
            {
                if (miscOrderMaster.Type == CodeMaster.MiscOrderType.GI)
                {
                    throw new BusinessException("计划外出库单{0}的状态为{1}不能冲销。",
                          miscOrderMaster.MiscOrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.MiscOrderStatus, ((int)miscOrderMaster.Status).ToString()));
                }
                else
                {
                    throw new BusinessException("计划外入库单{0}的状态为{1}不能冲销。",
                         miscOrderMaster.MiscOrderNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.MiscOrderStatus, ((int)miscOrderMaster.Status).ToString()));
                }
            }

            TryLoadMiscOrderLocationDetails(miscOrderMaster);
            User user = SecurityContextHolder.Get();
            miscOrderMaster.CancelDate = DateTime.Now;
            miscOrderMaster.CancelUserId = user.Id;
            miscOrderMaster.CancelUserName = user.FullName;
            miscOrderMaster.Status = com.Sconit.CodeMaster.MiscOrderStatus.Cancel;

            this.genericMgr.Update(miscOrderMaster);
            //如果是退货的 直接转为出库
            miscOrderMaster.Type = miscOrderMaster.Type == com.Sconit.CodeMaster.MiscOrderType.Return ? com.Sconit.CodeMaster.MiscOrderType.GI : miscOrderMaster.Type;
            this.locationDetailMgr.CancelInventoryOtherInOut(miscOrderMaster, effectiveDate);
        }

        [Transaction(TransactionMode.Requires)]
        public void CreateMiscOrderDetailFromXls(Stream inputStream, string miscOrderNo)
        {
            //导入的话以导入的为准，之前的全部干掉
            #region 清空明细
            var dets = this.genericMgr.FindAll<MiscOrderDetail>(" from MiscOrderDetail as m where m.MiscOrderNo = ?", miscOrderNo);
            if (dets != null && dets.Count > 0)
            {
                genericMgr.Delete(string.Format("from MiscOrderLocationDetail as d where d.MiscOrderDetailId in ({0})", string.Join(",", dets.Select(c => c.Id))));
            }

            string hql = @"from MiscOrderDetail as m where m.MiscOrderNo = ?";
            genericMgr.Delete(hql, new object[] { miscOrderNo }, new IType[] { NHibernateUtil.String });
            #endregion


            MiscOrderMaster miscOrder = genericMgr.FindById<MiscOrderMaster>(miscOrderNo);

            #region 导入数据
            if (inputStream.Length == 0)
            {
                throw new BusinessException("Import.Stream.Empty");
            }

            HSSFWorkbook workbook = new HSSFWorkbook(inputStream);

            ISheet sheet = workbook.GetSheetAt(0);
            IEnumerator rows = sheet.GetRowEnumerator();

            ImportHelper.JumpRows(rows, 10);
            int rowCount=10;
            BusinessException businessException = new BusinessException();

            #region 列定义
            int colItem = 1;//物料代码
            int colLocation = 2;//库位
            int colQty = 3;//数量
            int colReverseLine = 4;//预留行
            int colReverseNo = 5;//预留号
            #endregion

            DateTime dateTimeNow = DateTime.Now;

            IList<MiscOrderDetail> miscOrderDetailList = new List<MiscOrderDetail>();
            while (rows.MoveNext())
            {
                rowCount++;
                HSSFRow row = (HSSFRow)rows.Current;
                if (!ImportHelper.CheckValidDataRow(row, 1, 9))
                {
                    break;//边界
                }
                string itemCode = string.Empty;
                string locationCode = string.Empty;
                decimal qty = 0;
                string reverseLine = string.Empty;
                string reverseNo = string.Empty;
                Item item = new Item();

                #region 读取数据
                #region 读取物料代码
                itemCode = ImportHelper.GetCellStringValue(row.GetCell(colItem));
                if (string.IsNullOrWhiteSpace(itemCode))
                {
                    businessException.AddMessage(string.Format("第{0}行：物料代码不能为空。", rowCount));
                }
                else {
                    try
                    {
                        item = this.genericMgr.FindById<Item>(itemCode);
                    }
                    catch (Exception ex)
                    {

                        businessException.AddMessage(string.Format("第{0}行：物料代码{1}不存在。", rowCount,itemCode));
                    }
                }
                #endregion
              

                #endregion

                #region 读取库位,如果没填则取MiscOrderMaster上的库位
                locationCode = ImportHelper.GetCellStringValue(row.GetCell(colLocation));
                if (!string.IsNullOrEmpty(locationCode))
                {
                    try
                    {
                        Location location = genericMgr.FindById<Location>(locationCode);
                        if (location.Region != miscOrder.Region)
                        {
                            businessException.AddMessage(string.Format("第{0}行：指定区域{1}不存在此库位{2}。", rowCount, miscOrder.Region, locationCode));
                        }
                    }
                    catch (Exception ex)
                    {
                        businessException.AddMessage(string.Format("第{0}行：库位{1}不存在。", rowCount, locationCode));
                    }
                   
                }
                #endregion


                #region 读取数量
              
                string  ReadQty = ImportHelper.GetCellStringValue(row.GetCell(colQty));
                if (string.IsNullOrEmpty(ReadQty))
                {
                    businessException.AddMessage(string.Format("第{0}行：数量不能为空。", rowCount));
                }
                decimal.TryParse(ReadQty, out qty);
                if (qty <= 0)
                {
                    businessException.AddMessage(string.Format("第{0}行：数量{1}填写有误。", rowCount, ReadQty));
                }
               
                #endregion

                reverseLine = ImportHelper.GetCellStringValue(row.GetCell(colReverseLine));

                reverseNo = ImportHelper.GetCellStringValue(row.GetCell(colReverseNo));


                #region 填充数据
                MiscOrderDetail mod = new MiscOrderDetail();
                mod.Location = !string.IsNullOrWhiteSpace(locationCode) ? locationCode : miscOrder.Location;
                mod.Item = itemCode;
                mod.Uom = item.Uom;
                mod.UnitCount = item.UnitCount;
                mod.ItemDescription = item.Description;
                mod.ReferenceItemCode = item.ReferenceCode;
                mod.BaseUom =  item.Uom;
                mod.Qty = qty;
                MiscOrderMoveType miscOrderMoveType = genericMgr.FindAll<MiscOrderMoveType>("from MiscOrderMoveType as m where m.MoveType=? and m.IOType=?", new object[] { miscOrder.MoveType, miscOrder.Type })[0];
                if (miscOrderMoveType.CheckReserveLine)
                {
                    mod.ReserveLine = reverseLine;
                }
                if (miscOrderMoveType.CheckReserveNo)
                {
                    mod.ReserveNo = reverseNo;
                }
                //如果是寄售，则需要将供应商填充到明细的manufactureparty
                if (miscOrder.Consignment)
                {
                    if (string.IsNullOrWhiteSpace(mod.ManufactureParty))
                    {
                        mod.ManufactureParty = miscOrder.ManufactureParty;
                    }
                }
                miscOrderDetailList.Add(mod);
                #endregion
            }

            #endregion

            #region 新增明细
            CreateMiscOrderDetail(miscOrderDetailList, miscOrder.MiscOrderNo);
            #endregion
        }


        private IList<MiscOrderDetail> TryLoadMiscOrderDetails(MiscOrderMaster miscOrderMaster)
        {
            if (!string.IsNullOrWhiteSpace(miscOrderMaster.MiscOrderNo))
            {
                if (miscOrderMaster.MiscOrderDetails == null)
                {
                    string hql = "from MiscOrderDetail where MiscOrderNo = ? order by Sequence";

                    miscOrderMaster.MiscOrderDetails = this.genericMgr.FindAll<MiscOrderDetail>(hql, miscOrderMaster.MiscOrderNo);
                }

                return miscOrderMaster.MiscOrderDetails;
            }
            else
            {
                return null;
            }
        }

        private IList<MiscOrderLocationDetail> TryLoadMiscOrderLocationDetails(MiscOrderMaster miscOrderMaster)
        {
            if (miscOrderMaster.MiscOrderNo != null)
            {
                TryLoadMiscOrderDetails(miscOrderMaster);

                IList<MiscOrderLocationDetail> miscOrderLocationDetailList = new List<MiscOrderLocationDetail>();

                string hql = string.Empty;
                IList<object> para = new List<object>();
                foreach (MiscOrderDetail miscOrderDetail in miscOrderMaster.MiscOrderDetails)
                {
                    if (miscOrderDetail.MiscOrderLocationDetails != null && miscOrderDetail.MiscOrderLocationDetails.Count > 0)
                    {
                        ((List<MiscOrderLocationDetail>)miscOrderLocationDetailList).AddRange(miscOrderDetail.MiscOrderLocationDetails);
                    }
                    else
                    {
                        if (hql == string.Empty)
                        {
                            hql = "from MiscOrderLocationDetail where MiscOrderDetailId in (?";
                        }
                        else
                        {
                            hql += ",?";
                        }
                        para.Add(miscOrderDetail.Id);
                    }
                }

                if (hql != string.Empty)
                {
                    hql += ") order by MiscOrderDetailId";

                    ((List<MiscOrderLocationDetail>)miscOrderLocationDetailList).AddRange(this.genericMgr.FindAll<MiscOrderLocationDetail>(hql, para.ToArray()));
                }

                foreach (MiscOrderDetail miscOrderDetail in miscOrderMaster.MiscOrderDetails)
                {
                    if (miscOrderDetail.MiscOrderLocationDetails == null || miscOrderDetail.MiscOrderLocationDetails.Count == 0)
                    {
                        miscOrderDetail.MiscOrderLocationDetails = miscOrderLocationDetailList.Where(o => o.MiscOrderDetailId == miscOrderDetail.Id).ToList();
                    }
                }

                return miscOrderLocationDetailList;
            }
            else
            {
                return null;
            }
        }

        private void CreateMiscOrderDetail(IList<MiscOrderDetail> miscOrderDetailList, string miscOrderNo)
        {
            int maxSeq = 0;
            foreach (MiscOrderDetail miscOrderDetail in miscOrderDetailList)
            {

                miscOrderDetail.MiscOrderNo = miscOrderNo;
                miscOrderDetail.Sequence = ++maxSeq;
                if (miscOrderDetail.Uom != miscOrderDetail.BaseUom)
                {
                    miscOrderDetail.UnitQty = this.itemMgr.ConvertItemUomQty(miscOrderDetail.Item, miscOrderDetail.BaseUom, 1, miscOrderDetail.Uom);
                }
                else
                {
                    miscOrderDetail.UnitQty = 1;
                }
                this.genericMgr.Create(miscOrderDetail);
            }
        }


        [Transaction(TransactionMode.Requires)]
        public void Import261262MiscOrder(Stream inputStream)
        {
            #region 导入数据
            if (inputStream.Length == 0)
            {
                throw new BusinessException("Import.Stream.Empty");
            }

            HSSFWorkbook workbook = new HSSFWorkbook(inputStream);

            ISheet sheet = workbook.GetSheetAt(0);
            IEnumerator rows = sheet.GetRowEnumerator();

            ImportHelper.JumpRows(rows, 10);

            #region 列定义
            int colMoveType = 1;//移动类型
            int colEffectiveDate = 2;//生效日期
            int colRegion = 3;//区域
            int colLocation = 4;//库位
            int colReferenceNo = 5;//Sap订单号
            int colItem = 6;//物料编号
            int colQty = 7;//数量
            DateTime? prevEffeDate = null;
            string prevRegion = string.Empty;
            #endregion

            BusinessException businessException = new BusinessException();
            int rowCount = 10;
            IList<MiscOrderDetail> activeDetailList = new List<MiscOrderDetail>();
            IList<MiscOrderMaster> activeMasterList = new List<MiscOrderMaster>();
            while (rows.MoveNext())
            {
                rowCount++;
                HSSFRow row = (HSSFRow)rows.Current;
                if (!ImportHelper.CheckValidDataRow(row, 1, 9))
                {
                    break;//边界
                }
                string moveType = string.Empty;
                DateTime effectiveDate = System.DateTime.Now;
                string regionCode = string.Empty;

                string locationCode = string.Empty;
                string referenceNo = string.Empty;
                string itemCode = string.Empty;
                decimal qty = 0;
                Item item = new Item();

                #region 读取数据
                #region 移动类型
                moveType = ImportHelper.GetCellStringValue(row.GetCell(colMoveType));
                if (string.IsNullOrWhiteSpace(moveType))
                {
                    businessException.AddMessage(string.Format("第{0}行:移动类型不能为空。", rowCount));
                }
                else
                {
                    if (moveType != "261" && moveType != "262")
                    {
                        businessException.AddMessage(string.Format("第{0}行:移动类型{1}填写有误，只能填261、262。", rowCount, moveType));
                    }
                }
                #endregion

                #region 生效日期
                string readEffectiveDate = ImportHelper.GetCellStringValue(row.GetCell(colEffectiveDate));
                if (string.IsNullOrWhiteSpace(readEffectiveDate))
                {
                    businessException.AddMessage(string.Format("第{0}行:生效日期不能为空。", rowCount));
                }
                else
                {
                    if (!DateTime.TryParse(readEffectiveDate, out effectiveDate))
                    {
                        businessException.AddMessage(string.Format("第{0}行:生效日期{1}填写有误.", rowCount, moveType));
                        continue;
                    }
                    if (prevEffeDate != null)
                    {
                        if (prevEffeDate.Value != effectiveDate)
                        {
                            businessException.AddMessage(string.Format("第{0}行:生效日期{1}与前一行生效日期{2}不同。", rowCount, effectiveDate, prevEffeDate.Value));
                            continue;
                        }
                    }
                    prevEffeDate = effectiveDate;

                }
                #endregion

                #region 区域
                regionCode = ImportHelper.GetCellStringValue(row.GetCell(colRegion));
                if (string.IsNullOrWhiteSpace(regionCode))
                {
                    businessException.AddMessage(string.Format("第{0}行:区域不能为空。", rowCount));
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(prevRegion))
                    {
                        var regions = this.genericMgr.FindAll<Region>(" select r from Region as r where r.Code=? ", regionCode);
                        if (regions == null || regions.Count == 0)
                        {
                            businessException.AddMessage(string.Format("第{0}行:区域{1}填写有误.", rowCount, regionCode));
                        }
                    }
                    else
                    {
                        if (regionCode != prevRegion)
                        {
                            businessException.AddMessage(string.Format("第{0}行:区域{1}与前一行区域{2}不同。", rowCount, regionCode, prevRegion));
                            continue;
                        }
                    }
                    prevRegion = regionCode;
                }
                #endregion

                #region 读取库位
                locationCode = ImportHelper.GetCellStringValue(row.GetCell(colLocation));
                if (!string.IsNullOrEmpty(locationCode))
                {
                    var locations = genericMgr.FindAll<Location>(" select l from Location as l where l.Code=? ", locationCode);
                    if (locations == null || locations.Count == 0)
                    {
                        businessException.AddMessage(string.Format("第{0}行:库位{1}不存在。", rowCount, locationCode));
                    }
                    else if (locations.First().Region != regionCode)
                    {
                        businessException.AddMessage(string.Format("第{0}行:区域{1}不存在库位{2}。", rowCount, regionCode, locationCode));
                    }
                }
                else
                {
                    businessException.AddMessage(string.Format("第{0}行:区域不能为空。", rowCount));
                }
                #endregion

                #region Sap订单号
                referenceNo = ImportHelper.GetCellStringValue(row.GetCell(colReferenceNo));
                if (string.IsNullOrEmpty(referenceNo))
                {
                    businessException.AddMessage(string.Format("第{0}行:Sap订单号不能为空。", rowCount));
                }
                else
                {
                    //if (this.genericMgr.FindAllWithNativeSql<int>("select count(*) from SAP_ProdBomDet where AUFNR=? ", referenceNo.PadLeft(12, '0'))[0] == 0)
                    //{
                    //    businessException.AddMessage(string.Format("第{0}行:Sap订单号不存在ORD_OrderMstr_4表中。", rowCount));
                    //}
                }
                #endregion

                #region 物料编号
                itemCode = ImportHelper.GetCellStringValue(row.GetCell(colItem));
                if (string.IsNullOrWhiteSpace(itemCode))
                {
                    businessException.AddMessage(string.Format("第{0}行:物料编号不能为空。", rowCount));
                }
                else
                {
                    var items = this.genericMgr.FindAll<Item>(" select i from Item as i where i.Code=? ", itemCode);
                    if (items == null || items.Count == 0)
                    {
                        businessException.AddMessage(string.Format("第{0}行:物料编号{1}不存在.", rowCount, itemCode));
                    }
                    else
                    {
                        item = items.First();
                    }
                }
                #endregion

                #region 数量
                string readQty = ImportHelper.GetCellStringValue(row.GetCell(colQty));
                if (string.IsNullOrEmpty(readQty))
                {
                    businessException.AddMessage(string.Format("第{0}行:数量不能为空。", rowCount));
                }
                else
                {
                    decimal.TryParse(readQty, out qty);
                    if (qty <= 0)
                    {
                        businessException.AddMessage(string.Format("第{0}行:数量{1}只能为大于等于0的数字。", rowCount, readQty));
                    }
                }
                #endregion

                #endregion

                #region 填充数据
                if (!businessException.HasMessage)
                {
                    MiscOrderDetail miscOrderDetail = new MiscOrderDetail();
                    miscOrderDetail.MoveType = moveType;
                    miscOrderDetail.EffectiveDate = effectiveDate;
                    miscOrderDetail.Region = regionCode;
                    miscOrderDetail.Location = locationCode;
                    miscOrderDetail.SapOrderNo = string.IsNullOrWhiteSpace(referenceNo) ? null : referenceNo.PadLeft(12, '0');
                    miscOrderDetail.Item = item.Code;
                    miscOrderDetail.ItemDescription = item.Description;
                    miscOrderDetail.ReferenceItemCode = item.ReferenceCode;
                    miscOrderDetail.Uom = item.Uom;
                    miscOrderDetail.BaseUom = item.Uom;
                    miscOrderDetail.UnitCount = item.UnitCount;
                    miscOrderDetail.Qty = qty;
                    activeDetailList.Add(miscOrderDetail);
                }
                #endregion
            }

            if (businessException.HasMessage)
            {
                throw businessException;
            }

            if (activeDetailList.Count == 0)
            {
                throw new BusinessException("导入的有效数据为0，请确实。");
            }
            //261一张单
            var outDetail = activeDetailList.Where(a => a.MoveType == "261").ToList();
            if (outDetail != null && outDetail.Count > 0)
            {
                MiscOrderDetail fisrDet = outDetail.First();
                MiscOrderMoveType miscOrderMoveType = genericMgr.FindAll<MiscOrderMoveType>("from MiscOrderMoveType as m where m.MoveType='261'")[0];
                MiscOrderMaster miscMaster = new MiscOrderMaster();
                miscMaster.Type = miscOrderMoveType.IOType;
                miscMaster.MoveType = miscOrderMoveType.MoveType;
                miscMaster.CancelMoveType = miscOrderMoveType.CancelMoveType;
                miscMaster.Region = fisrDet.Region;
                miscMaster.Location = fisrDet.Location;
                miscMaster.EffectiveDate = fisrDet.EffectiveDate;
                miscMaster.Consignment = false;
                miscMaster.ManufactureParty = null;
                miscMaster.IsScanHu = false;
                miscMaster.ReferenceNo = null;
                miscMaster.MiscOrderDetails = outDetail;
                activeMasterList.Add(miscMaster);
            }

            //262 一张单
            var inDetail = activeDetailList.Where(a => a.MoveType == "262").ToList();
            if (inDetail != null && inDetail.Count > 0)
            {
                MiscOrderDetail fisrInDet = inDetail.First();
                MiscOrderMoveType miscOrderInMoveType = genericMgr.FindAll<MiscOrderMoveType>("from MiscOrderMoveType as m where m.MoveType='262'")[0];
                var inMiscOrder = new MiscOrderMaster();
                inMiscOrder.Type = miscOrderInMoveType.IOType;
                inMiscOrder.MoveType = miscOrderInMoveType.MoveType;
                inMiscOrder.CancelMoveType = miscOrderInMoveType.CancelMoveType;
                inMiscOrder.Region = fisrInDet.Region;
                inMiscOrder.Location = fisrInDet.Location;
                inMiscOrder.EffectiveDate = fisrInDet.EffectiveDate;
                inMiscOrder.Consignment = false;
                inMiscOrder.ManufactureParty = null;
                inMiscOrder.IsScanHu = false;
                inMiscOrder.ReferenceNo = null;
                inMiscOrder.MiscOrderDetails = inDetail;
                activeMasterList.Add(inMiscOrder);
            }

            if (businessException.HasMessage)
            {
                throw businessException;
            }

            string message = "生成单号";
            foreach (var master in activeMasterList)
            {
                master.QualityType = com.Sconit.CodeMaster.QualityType.Qualified;
                activeDetailList = master.MiscOrderDetails;
                master.MiscOrderDetails = new List<MiscOrderDetail>();
                this.CreateMiscOrder(master);
                BatchUpdateMiscOrderDetails(master, activeDetailList, null, null);
                this.genericMgr.FlushSession();
                CloseMiscOrder(master);
                message += " " + master.MiscOrderNo + ";";
            }
            MessageHolder.AddMessage(new Message(CodeMaster.MessageType.Info, message));
            #endregion
        }

        /// <summary>
        /// 库存初始化
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="miscOrderNo"></param>
        /// <param name="checkRegionContainsLocation"></param>
        [Transaction(TransactionMode.Requires)]
        public void CreateMiscInvInitDetailFromXls(Stream inputStream, string miscOrderNo)
        {
            //导入的话以导入的为准，之前的全部干掉
            #region 清空明细
            var dets = this.genericMgr.FindAll<MiscOrderDetail>(" from MiscOrderDetail as m where m.MiscOrderNo = ?", miscOrderNo);
            if (dets != null && dets.Count > 0)
            {
                genericMgr.Delete("from MiscOrderLocationDetail as d where d.MiscOrderNo =?", miscOrderNo, NHibernate.NHibernateUtil.String);
            }

            string hql = @"from MiscOrderDetail as m where m.MiscOrderNo = ?";
            genericMgr.Delete(hql, new object[] { miscOrderNo }, new IType[] { NHibernateUtil.String });
            #endregion


            MiscOrderMaster miscOrder = genericMgr.FindById<MiscOrderMaster>(miscOrderNo);

            #region 导入数据
            if (inputStream.Length == 0)
            {
                throw new BusinessException("Import.Stream.Empty");
            }

            BusinessException businessException = new BusinessException();

            HSSFWorkbook workbook = new HSSFWorkbook(inputStream);

            ISheet sheet = workbook.GetSheetAt(0);
            IEnumerator rows = sheet.GetRowEnumerator();

            ImportHelper.JumpRows(rows, 10);

            #region 列定义
            int colItem = 1;//物料代码
            int colLocation = 2;//库位
            int colSupplier = 3;//寄售供应商
            int colQty = 4;//数量
            int rowCount = 10;
            #endregion

            DateTime dateTimeNow = DateTime.Now;

            IList<MiscOrderDetail> miscOrderDetailList = new List<MiscOrderDetail>();
            while (rows.MoveNext())
            {
                rowCount++;
                HSSFRow row = (HSSFRow)rows.Current;
                if (!ImportHelper.CheckValidDataRow(row, 1, 9))
                {
                    break;//边界
                }
                string itemCode = string.Empty;
                decimal qty = 0;
                string manufactureParty = string.Empty;
                string locationCode = string.Empty;
                Item item = new Item();

                #region 读取数据
                #region 读取物料代码
                itemCode = ImportHelper.GetCellStringValue(row.GetCell(colItem));
                if (string.IsNullOrWhiteSpace(itemCode))
                {
                    businessException.AddMessage(string.Format("第{0}行：物料编号不能为空。", rowCount));
                }
                else
                {
                    var items = this.genericMgr.FindAll<Item>(" select i from Item as i where i.Code=? ", itemCode);
                    if (items == null || items.Count == 0)
                    {
                        businessException.AddMessage(string.Format("第{0}行：物料编号{1}不存在。", rowCount, itemCode));
                        continue;
                    }
                    item = items.First();
                }

                #endregion

                #region 读取库位,如果没填则取MiscOrderMaster上的库位
                locationCode = ImportHelper.GetCellStringValue(row.GetCell(colLocation));
                if (!string.IsNullOrEmpty(locationCode))
                {
                    var locs = this.genericMgr.FindAll<Location>(" select l from Location as l where l.Code=? ", locationCode);
                    if (locs == null || locs.Count == 0)
                    {
                        businessException.AddMessage(string.Format("第{0}行：库位{1}不存在。", rowCount, locationCode));
                    }
                    else
                    {
                        if (locs.First().Region != miscOrder.Region)
                        {
                            businessException.AddMessage(string.Format("第{0}行：指定区域{1}不存在此库位{2}", rowCount, miscOrder.Region, locationCode));
                        }
                    }
                }
                #endregion

                #region 寄售供应商
                manufactureParty = ImportHelper.GetCellStringValue(row.GetCell(colSupplier));
                if (!string.IsNullOrWhiteSpace(manufactureParty))
                {
                    var manufacturePartys = this.genericMgr.FindAll<Supplier>(" select i from Supplier as i where i.Code=? ", manufactureParty);
                    if (manufacturePartys == null || manufacturePartys.Count == 0)
                    {
                        businessException.AddMessage(string.Format("第{0}行：寄售供应商{1}不存在。", rowCount, manufacturePartys));
                    }
                }

                #endregion

                #region 读取数量
                try
                {
                    qty = Convert.ToDecimal(row.GetCell(colQty).NumericCellValue);
                    if (qty <= 0)
                    {
                        businessException.AddMessage(string.Format("第{0}行：数量填写有误。", rowCount));
                    }
                }
                catch
                {
                    businessException.AddMessage(string.Format("第{0}行：数量填写有误。"));
                }
                #endregion

                #endregion

                #region 填充数据
                MiscOrderDetail mod = new MiscOrderDetail();
                mod.Location = !string.IsNullOrWhiteSpace(locationCode) ? locationCode : miscOrder.Location;
                mod.Item = itemCode;
                mod.Uom = item.Uom;
                mod.ItemDescription = item.Description;
                mod.BaseUom = item.Uom;
                mod.Qty = qty;
                mod.ManufactureParty = string.IsNullOrWhiteSpace(manufactureParty) ? null : manufactureParty;
                miscOrderDetailList.Add(mod);
                #endregion

            }

            if (businessException.HasMessage)
            {
                throw businessException;
            }

            #region 新增明细
            CreateMiscOrderDetail(miscOrderDetailList, miscOrder.MiscOrderNo);
            #endregion
            #endregion
        }

        [Transaction(TransactionMode.Requires)]
        public void BatchUpdateMiscOrderDetailsAndClose(string miscOrderNo,
            IList<MiscOrderDetail> addMiscOrderDetailList, IList<MiscOrderDetail> updateMiscOrderDetailList, IList<MiscOrderDetail> deleteMiscOrderDetailList)
        {
            MiscOrderMaster miscOrderMaster = this.genericMgr.FindById<MiscOrderMaster>(miscOrderNo);
            BatchUpdateMiscOrderDetails(miscOrderMaster, addMiscOrderDetailList, updateMiscOrderDetailList, deleteMiscOrderDetailList);
            this.genericMgr.FlushSession();
            this.CloseMiscOrder(miscOrderMaster);
        }
    }
}
