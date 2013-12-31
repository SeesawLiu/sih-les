using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.Services.Transaction;
using com.Sconit.Entity;
using com.Sconit.Entity.ACC;
using com.Sconit.Entity.CUST;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.INV;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.SCM;
using com.Sconit.Entity.VIEW;
using com.Sconit.Utility;
using NHibernate;
using NHibernate.Type;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace com.Sconit.Service.Impl
{
    [Transactional]
    public class StockTakeMgrImpl : BaseMgr, IStockTakeMgr
    {
        public IGenericMgr genericMgr { get; set; }
        public INumberControlMgr numberControlMgr { get; set; }
        public ISystemMgr systemMgr { get; set; }
        public IHuMgr huMgr { get; set; }
        public ILocationDetailMgr locationDetailMgr { get; set; }
        public IItemMgr itemMgr { get; set; }


        private static string SelectStockTakeDetailStatement = "from StockTakeDetail as s where StNo = ? ";
        //private static string SelectStockTakeResultStatement = "from StockTakeResult as s where StNo = ? ";
        private static string SelectStockTakeLocationStatement = "select Location from StockTakeLocation as s where StNo = ? ";
        private static string SelectStockTakeItemStatement = "select Item from StockTakeItem as s where StNo = ? ";

        #region 创建盘点单
        [Transaction(TransactionMode.Requires)]
        public void CreateStockTakeMaster(StockTakeMaster stockTakeMaster)
        {
            stockTakeMaster.StNo = numberControlMgr.GetStockTakeNo(stockTakeMaster);
            this.genericMgr.Create(stockTakeMaster);
            this.genericMgr.FlushSession();
            IList<Location> locs = genericMgr.FindAll<Location>("from Location where IsActive = 1 and Region = ?", stockTakeMaster.Region);
            if (locs != null && locs.Count > 0)
            {
                foreach (var loc in locs)
                {
                    this.genericMgr.Create(new StockTakeLocation
                    {
                        StNo = stockTakeMaster.StNo,
                        Location = loc.Code,
                        LocationName = loc.Name,
                    });
                }
            }
        }
        #endregion

        #region 添加盘点库位
        [Transaction(TransactionMode.Requires)]
        public void BatchUpdateStockTakeLocations(string stNo, IList<Location> addLocaitons, IList<Location> deleteLocations)
        {
            StockTakeMaster stockTakeMaster = this.genericMgr.FindById<StockTakeMaster>(stNo);
            BatchUpdateStockTakeLocations(stockTakeMaster, addLocaitons, deleteLocations);
        }

        [Transaction(TransactionMode.Requires)]
        public void BatchUpdateStockTakeLocations(StockTakeMaster stockTakeMaster, IList<Location> addLocaitons, IList<Location> deleteLocations)
        {
            #region 检查
            if (stockTakeMaster.Status != CodeMaster.StockTakeStatus.Create)
            {
                throw new BusinessException("状态为{1}的盘点单{0}不能添加库位。", stockTakeMaster.StNo,
                    this.systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.StockTakeStatus, ((int)stockTakeMaster.Status).ToString()));
            }

            #region 判断是否有重复库位
            if (addLocaitons != null && addLocaitons.Count > 0)
            {
                IList<string> locCodes = addLocaitons.Select(l => l.Code).ToList();

                #region 查询盘点库位列表
                IList<string> stockTakeLocationList = this.genericMgr.FindAll<string>(SelectStockTakeLocationStatement, stockTakeMaster.StNo);
                #endregion
                if (stockTakeLocationList != null && stockTakeLocationList.Count > 0)
                {
                    ((List<string>)locCodes).AddRange(stockTakeLocationList);
                }

                var locCounts = from loc in locCodes
                                group loc by loc into result
                                select new
                                {
                                    LocationCode = result.Key,
                                    Count = result.Count()
                                };

                BusinessException businessException = new BusinessException();
                foreach (var locCount in locCounts.Where(l => l.Count > 1))
                {
                    businessException.AddMessage("不能重复添加库位{0}", locCount.LocationCode);
                }

                if (businessException.HasMessage)
                {
                    throw businessException;
                }
            }
            #endregion
            #endregion

            #region 新增盘点库位
            if (addLocaitons != null && addLocaitons.Count > 0)
            {
                IList<StockTakeLocation> addStockTakeLocationList = (from loc in addLocaitons
                                                                     select new StockTakeLocation
                                                                     {
                                                                         StNo = stockTakeMaster.StNo,
                                                                         Location = loc.Code,
                                                                         LocationName = loc.Name
                                                                     }).ToList();

                foreach (StockTakeLocation addStockTakeLocation in addStockTakeLocationList)
                {
                    this.genericMgr.Create(addStockTakeLocation);
                }
            }
            #endregion

            #region 删除盘点库位
            if (deleteLocations != null && deleteLocations.Count > 0)
            {
                string deleteStockTakeLocationStatement = string.Empty;
                IList<object> deleteStockTakeLocationParas = new List<object>();
                IList<IType> deleteStockTakeLocationTypes = new List<IType>();

                foreach (Location location in deleteLocations)
                {
                    if (deleteStockTakeLocationStatement == string.Empty)
                    {
                        deleteStockTakeLocationStatement = "from StockTakeLocation where StNo = ?  and Location in (?";
                        deleteStockTakeLocationParas.Add(stockTakeMaster.StNo);
                        deleteStockTakeLocationTypes.Add(NHibernateUtil.String);
                    }
                    else
                    {
                        deleteStockTakeLocationStatement += ", ?";
                    }
                    deleteStockTakeLocationParas.Add(location.Code);
                    deleteStockTakeLocationTypes.Add(NHibernateUtil.String);
                }

                this.genericMgr.Delete(deleteStockTakeLocationStatement, deleteStockTakeLocationParas.ToArray(), deleteStockTakeLocationTypes.ToArray());
            }
            #endregion
        }
        #endregion

        #region 添加盘点零件
        [Transaction(TransactionMode.Requires)]
        public void BatchUpdateStockTakeItems(string stNo, IList<Item> addItems, IList<Item> deleteItems)
        {
            StockTakeMaster stockTakeMaster = this.genericMgr.FindById<StockTakeMaster>(stNo);
            BatchUpdateStockTakeItems(stockTakeMaster, addItems, deleteItems);
        }

        [Transaction(TransactionMode.Requires)]
        public void BatchUpdateStockTakeItems(StockTakeMaster stockTakeMaster, IList<Item> addItems, IList<Item> deleteItems)
        {
            #region 检查
            if (stockTakeMaster.Status != CodeMaster.StockTakeStatus.Create)
            {
                throw new BusinessException("状态为{1}的盘点单{0}不能添加零件。", stockTakeMaster.StNo,
                    this.systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.StockTakeStatus, ((int)stockTakeMaster.Status).ToString()));
            }

            if (stockTakeMaster.Type == CodeMaster.StockTakeType.All)
            {
                throw new BusinessException("盘点单{0}类型为全盘不能添加零件。", stockTakeMaster.StNo);
            }

            #region 判断是否有重复库位
            if (addItems != null && addItems.Count > 0)
            {
                IList<string> itemCodes = addItems.Select(l => l.Code).ToList();

                #region 查询盘点零件列表
                IList<string> stockTakeItemList = this.genericMgr.FindAll<string>(SelectStockTakeItemStatement, stockTakeMaster.StNo);
                #endregion
                if (stockTakeItemList != null && stockTakeItemList.Count > 0)
                {
                    ((List<string>)itemCodes).AddRange(stockTakeItemList);
                }

                var itemCounts = from itemCode in itemCodes
                                 group itemCode by itemCode into result
                                 select new
                                 {
                                     ItemCode = result.Key,
                                     Count = result.Count()
                                 };

                BusinessException businessException = new BusinessException();
                foreach (var itemCount in itemCounts.Where(l => l.Count > 1))
                {
                    businessException.AddMessage("不能重复添加零件{0}", itemCount.ItemCode);
                }

                if (businessException.HasMessage)
                {
                    throw businessException;
                }
            }
            #endregion
            #endregion

            #region 新增盘点零件
            if (addItems != null && addItems.Count > 0)
            {
                IList<StockTakeItem> addStockTakeItemList = (from item in addItems
                                                             select new StockTakeItem
                                                             {
                                                                 StNo = stockTakeMaster.StNo,
                                                                 Item = item.Code,
                                                                 ItemDescription = item.Description
                                                             }).ToList();

                foreach (StockTakeItem addStockTakeItem in addStockTakeItemList)
                {
                    this.genericMgr.Create(addStockTakeItem);
                }
            }
            #endregion

            #region 删除盘点零件
            if (deleteItems != null && deleteItems.Count > 0)
            {
                string deleteStockTakeItemStatement = string.Empty;
                IList<object> deleteStockTakeItemParas = new List<object>();
                IList<IType> deleteStockTakeItemTypes = new List<IType>();

                foreach (Item item in deleteItems)
                {
                    if (deleteStockTakeItemStatement == string.Empty)
                    {
                        deleteStockTakeItemStatement = "from StockTakeItem where StNo = ? and Item in (?";
                        deleteStockTakeItemParas.Add(stockTakeMaster.StNo);
                        deleteStockTakeItemTypes.Add(NHibernateUtil.String);
                    }
                    else
                    {
                        deleteStockTakeItemStatement += ", ?";
                    }
                    deleteStockTakeItemParas.Add(item.Code);
                    deleteStockTakeItemTypes.Add(NHibernateUtil.String);
                }

                this.genericMgr.Delete(deleteStockTakeItemStatement, deleteStockTakeItemParas.ToArray(), deleteStockTakeItemTypes.ToArray());
            }
            #endregion
        }
        #endregion

        #region 删除盘点单
        [Transaction(TransactionMode.Requires)]
        public void DeleteStockTakeMaster(string stNo)
        {
            StockTakeMaster stockTakeMaster = this.genericMgr.FindById<StockTakeMaster>(stNo);
            DeleteStockTakeMaster(stockTakeMaster);
        }

        [Transaction(TransactionMode.Requires)]
        public void DeleteStockTakeMaster(StockTakeMaster stockTakeMaster)
        {
            if (stockTakeMaster.Status != CodeMaster.StockTakeStatus.Create)
            {
                throw new BusinessException(Resources.INV.StockTake.Error_StatusErrorWhenDelete,
                    stockTakeMaster.StNo,
                    this.systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.StockTakeStatus, ((int)stockTakeMaster.Status).ToString()));
            }

            this.genericMgr.Delete("from StockTakeItem where StNo = ?", stockTakeMaster.StNo, NHibernateUtil.String);
            this.genericMgr.Delete("from StockTakeLocation where StNo = ?", stockTakeMaster.StNo, NHibernateUtil.String);
            this.genericMgr.Delete("from StockTakeMaster where StNo = ?", stockTakeMaster.StNo, NHibernateUtil.String);
        }
        #endregion

        #region 释放执行盘点单一步操作
        [Transaction(TransactionMode.Requires)]
        public void ReleaseAndStartStockTakeMaster(string stNo)
        {
            StockTakeMaster stockTakeMaster = this.genericMgr.FindById<StockTakeMaster>(stNo);
            if (stockTakeMaster.Status != CodeMaster.StockTakeStatus.Create)
            {
                throw new BusinessException(Resources.INV.StockTake.Error_StatusErrorWhenSubmit,
                    stockTakeMaster.StNo,
                    this.systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.StockTakeStatus, ((int)stockTakeMaster.Status).ToString()));
            }

            IList<string> stockTakeLocationList = this.genericMgr.FindAll<string>(SelectStockTakeLocationStatement, stockTakeMaster.StNo);

            if (stockTakeLocationList == null || stockTakeLocationList.Count == 0)
            {
                throw new BusinessException("请选择盘点的库位。");
            }
            User user = SecurityContextHolder.Get();

            stockTakeMaster.Status = CodeMaster.StockTakeStatus.InProcess;
            stockTakeMaster.StartUserId = user.Id;
            stockTakeMaster.StartUserName = user.FullName;
            stockTakeMaster.StartDate = DateTime.Now;
            this.genericMgr.Update(stockTakeMaster);
            IList<StockTakeLocationLotDet> locLotDets = this.genericMgr.FindAll<StockTakeLocationLotDet>(" from StockTakeLocationLotDet as s where exists( select 1 from StockTakeLocation as l where l.Location=s.Location and l.StNo=? ) and s.RefNo=? order by Location asc ", new object[] { stNo, stockTakeMaster.RefNo });
            if (locLotDets != null && locLotDets.Count > 0)
            {
                int i = 1;
                foreach (var locLotDet in locLotDets)
                {
                    StockTakeDetail stockTakeDetail = new StockTakeDetail();
                    stockTakeDetail.StNo = stNo;
                    stockTakeDetail.Item = locLotDet.Item;
                    stockTakeDetail.RefItemCode = locLotDet.RefItemCode;
                    stockTakeDetail.ItemDescription = locLotDet.ItemDesc;
                    stockTakeDetail.Uom = locLotDet.Uom;
                    stockTakeDetail.BaseUom = locLotDet.Uom;
                    stockTakeDetail.Location = locLotDet.Location;
                    stockTakeDetail.QualityType = locLotDet.QualityType;
                    stockTakeDetail.IsConsigement = locLotDet.IsConsigement;
                    stockTakeDetail.CSSupplier = locLotDet.CSSupplier;
                    stockTakeDetail.UnitQty = 1;
                    stockTakeDetail.Qty = 0;
                    this.genericMgr.Create(stockTakeDetail);
                }
            }
        }
        #endregion

        #region 释放盘点单
        [Transaction(TransactionMode.Requires)]
        public void ReleaseStockTakeMaster(string stNo)
        {
            StockTakeMaster stockTakeMaster = this.genericMgr.FindById<StockTakeMaster>(stNo);
            ReleaseStockTakeMaster(stockTakeMaster);
        }

        [Transaction(TransactionMode.Requires)]
        public void ReleaseStockTakeMaster(StockTakeMaster stockTakeMaster)
        {
            if (stockTakeMaster.Status != CodeMaster.StockTakeStatus.Create)
            {
                throw new BusinessException(Resources.INV.StockTake.Error_StatusErrorWhenSubmit,
                    stockTakeMaster.StNo,
                    this.systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.StockTakeStatus, ((int)stockTakeMaster.Status).ToString()));
            }

            IList<string> stockTakeLocationList = this.genericMgr.FindAll<string>(SelectStockTakeLocationStatement, stockTakeMaster.StNo);

            if (stockTakeLocationList == null || stockTakeLocationList.Count == 0)
            {
                throw new BusinessException("请选择盘点的库位。");
            }

            if (stockTakeMaster.Type == CodeMaster.StockTakeType.Part)
            {
                IList<string> stockTakeItemList = this.genericMgr.FindAll<string>(SelectStockTakeItemStatement, stockTakeMaster.StNo);

                if (stockTakeItemList == null || stockTakeItemList.Count == 0)
                {
                    throw new BusinessException("请选择盘点的零件。");
                }
            }

            User user = SecurityContextHolder.Get();

            stockTakeMaster.Status = CodeMaster.StockTakeStatus.Submit;
            stockTakeMaster.ReleaseUserId = user.Id;
            stockTakeMaster.ReleaseUserName = user.FullName;
            stockTakeMaster.ReleaseDate = DateTime.Now;

            this.genericMgr.Update(stockTakeMaster);
        }
        #endregion

        #region 开始盘点
        [Transaction(TransactionMode.Requires)]
        public void StartStockTakeMaster(string stNo)
        {
            StockTakeMaster stockTakeMaster = this.genericMgr.FindById<StockTakeMaster>(stNo);
            StartStockTakeMaster(stockTakeMaster);
        }

        [Transaction(TransactionMode.Requires)]
        public void StartStockTakeMaster(StockTakeMaster stockTakeMaster)
        {
            if (stockTakeMaster.Status != CodeMaster.StockTakeStatus.Submit)
            {
                throw new BusinessException(Resources.INV.StockTake.Error_StatusErrorWhenSubmit,
                    stockTakeMaster.StNo,
                    this.systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.StockTakeStatus, ((int)stockTakeMaster.Status).ToString()));
            }

            User user = SecurityContextHolder.Get();

            stockTakeMaster.Status = CodeMaster.StockTakeStatus.InProcess;
            stockTakeMaster.StartUserId = user.Id;
            stockTakeMaster.StartUserName = user.FullName;
            stockTakeMaster.StartDate = DateTime.Now;

            this.genericMgr.Update(stockTakeMaster);
        }
        #endregion

        #region 取消盘点
        [Transaction(TransactionMode.Requires)]
        public void CancelStockTakeMaster(string stNo)
        {
            StockTakeMaster stockTakeMaster = this.genericMgr.FindById<StockTakeMaster>(stNo);
            CancelStockTakeMaster(stockTakeMaster);
        }

        [Transaction(TransactionMode.Requires)]
        public void CancelStockTakeMaster(StockTakeMaster stockTakeMaster)
        {
            if (stockTakeMaster.Status != CodeMaster.StockTakeStatus.Submit)
            {
                throw new BusinessException("盘点单{0}的状态为{1}，不能取消。",
                    stockTakeMaster.StNo,
                    this.systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.StockTakeStatus, ((int)stockTakeMaster.Status).ToString()));
            }

            User user = SecurityContextHolder.Get();

            stockTakeMaster.Status = CodeMaster.StockTakeStatus.Cancel;
            stockTakeMaster.CancelUserId = user.Id;
            stockTakeMaster.CancelUserName = user.FullName;
            stockTakeMaster.CancelDate = DateTime.Now;

            this.genericMgr.Update(stockTakeMaster);
        }
        #endregion

        #region 记录盘点结果
        [Transaction(TransactionMode.Requires)]
        public void RecordStockTakeDetail(string stNo, IList<StockTakeDetail> stockTakeDetailList)
        {
            StockTakeMaster stockTakeMaster = this.genericMgr.FindById<StockTakeMaster>(stNo);
            RecordStockTakeDetail(stockTakeMaster, stockTakeDetailList);
        }

        [Transaction(TransactionMode.Requires)]
        public void RecordStockTakeDetail(StockTakeMaster stockTakeMaster, IList<StockTakeDetail> stockTakeDetailList)
        {
            #region 校验
            if (stockTakeDetailList == null || stockTakeDetailList.Count == 0)
            {
                throw new BusinessException("盘点明细不能为空。");
            }
            IList<StockTakeDetail> noneZeroStockTakeDetailList = stockTakeDetailList.Where(d => d.Qty > 0).ToList();

            if (stockTakeMaster.Status != CodeMaster.StockTakeStatus.InProcess)
            {
                throw new BusinessException("盘点单{0}的状态为{1}，不能盘点。", stockTakeMaster.StNo,
                    this.systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.StockTakeStatus, ((int)stockTakeMaster.Status).ToString()));
            }

            #region 检查库位是否正确
            BusinessException businessException = new BusinessException();
            IList<string> stockTakeLocationList = this.genericMgr.FindAll<string>(SelectStockTakeLocationStatement, stockTakeMaster.StNo);

            foreach (string location in noneZeroStockTakeDetailList.Select(s => s.Location).Distinct())
            {
                if (!stockTakeLocationList.Contains(location))
                {
                    businessException.AddMessage("库位{0}不在待盘点的库位列表中。", location);
                }
            }
            #endregion

            #region 检查零件是否正确
            if (stockTakeMaster.Type == CodeMaster.StockTakeType.Part)
            {
                IList<string> stockTakeItemList = this.genericMgr.FindAll<string>(SelectStockTakeItemStatement, stockTakeMaster.StNo);
                foreach (string item in noneZeroStockTakeDetailList.Select(s => s.Item).Distinct())
                {
                    if (!stockTakeItemList.Contains(item))
                    {
                        businessException.AddMessage("零件{0}不在待盘点的零件列表中。", item);
                    }
                }
            }
            #endregion

            #region 检查盘点明细是否重复
            if (stockTakeMaster.IsScanHu)
            {
                //按条码盘点查询条码
                IList<string> takedDetailList = this.genericMgr.FindAll<string>("select HuId from StockTakeDetail where StNo = ?", stockTakeMaster.StNo);
                if (takedDetailList != null && takedDetailList.Count > 0)
                {
                    foreach (StockTakeDetail stockTakeDetail in noneZeroStockTakeDetailList)
                    {
                        //判断条码是否重复
                        if (takedDetailList.Contains(stockTakeDetail.HuId))
                        {
                            businessException.AddMessage("条码{0}重复盘点。", stockTakeDetail.HuId);
                        }
                        takedDetailList.Add(stockTakeDetail.HuId);
                    }
                }
            }
            else
            {
                //按数量盘点查询零件号
                IList<object[]> takedDetailList = this.genericMgr.FindAll<object[]>("select distinct Item, QualityType from StockTakeDetail where StNo = ?", stockTakeMaster.StNo);
                if (takedDetailList != null && takedDetailList.Count > 0)
                {
                    var duplicatedTakeDetailList = from takedDet in takedDetailList
                                                   join stockTakeDetail in noneZeroStockTakeDetailList
                                                   on new
                                                   {
                                                       Item = (string)takedDet[0],
                                                       QualityType = (CodeMaster.QualityType)takedDet[1]
                                                   } equals new { Item = stockTakeDetail.Item, QualityType = stockTakeDetail.QualityType }
                                                   select new
                                                   {
                                                       Item = stockTakeDetail.Item,
                                                       QualityType = stockTakeDetail.QualityType,
                                                   };

                    if (duplicatedTakeDetailList != null && duplicatedTakeDetailList.Count() > 0)
                    {
                        foreach (var duplicatedTakeDetail in duplicatedTakeDetailList)
                        {
                            businessException.AddMessage("质量状态为{1}的零件{0}重复盘点。", duplicatedTakeDetail.Item,
                                this.systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.QualityType, ((int)duplicatedTakeDetail.QualityType).ToString()));
                        }
                    }
                }
            }
            #endregion

            if (businessException.HasMessage)
            {
                throw businessException;
            }
            #endregion

            #region 上架
            if (stockTakeMaster.IsScanHu)
            {
                this.locationDetailMgr.InventoryPut(noneZeroStockTakeDetailList);
            }
            #endregion

            #region 循环更新盘点明细
            string selectItemStatement = string.Empty;
            IList<object> selectItemParas = new List<object>();
            foreach (StockTakeDetail stockTakeDetail in stockTakeDetailList)
            {
                if (selectItemStatement == string.Empty)
                {
                    selectItemStatement = "from Item where Code in(?";
                }
                else
                {
                    selectItemStatement += ",?";
                }
                selectItemParas.Add(stockTakeDetail.Item);
            }
            selectItemStatement += ")";

            IList<Item> itemList = this.genericMgr.FindAll<Item>(selectItemStatement, selectItemParas.ToArray());

            foreach (StockTakeDetail stockTakeDetail in stockTakeDetailList)
            {
                stockTakeDetail.StNo = stockTakeMaster.StNo;
                #region 计算基本单位和转换率
                stockTakeDetail.BaseUom = itemList.Where(i => i.Code == stockTakeDetail.Item).Single().Uom;
                if (stockTakeDetail.BaseUom != stockTakeDetail.Uom)
                {
                    stockTakeDetail.UnitQty = this.itemMgr.ConvertItemUomQty(stockTakeDetail.Item, stockTakeDetail.Uom, 1, stockTakeDetail.BaseUom);
                }
                else
                {
                    stockTakeDetail.UnitQty = 1;
                }
                #endregion

                this.genericMgr.Create(stockTakeDetail);
            }
            //this.genericMgr.Update(stockTakeMaster);
            #endregion
        }
        #endregion

        #region 显示盘点结果
        public IList<StockTakeResultSummary> ListStockTakeResult(string stNo, bool listShortage, bool listProfit, bool listMatch, IList<string> locationList, IList<string> binList, IList<string> itemList, DateTime? BaseInventoryDate)
        {
            StockTakeMaster stockTakeMaster = this.genericMgr.FindById<StockTakeMaster>(stNo);
            return ListStockTakeResult(stockTakeMaster, listShortage, listProfit, listMatch, locationList, binList, itemList, BaseInventoryDate);
        }

        public IList<StockTakeResultSummary> ListStockTakeResult(StockTakeMaster stockTakeMaster, bool listShortage, bool listProfit, bool listMatch, IList<string> locationList, IList<string> binList, IList<string> itemList, DateTime? BaseInventoryDate)
        {
            if (stockTakeMaster.Status == CodeMaster.StockTakeStatus.Create
                || stockTakeMaster.Status == CodeMaster.StockTakeStatus.Cancel
                || stockTakeMaster.Status == CodeMaster.StockTakeStatus.Submit)
            {
                throw new BusinessException("盘点单{0}的状态为{1}，不能显示盘点结果。", stockTakeMaster.StNo,
                    this.systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.StockTakeStatus, ((int)stockTakeMaster.Status).ToString()));
            }

            IList<StockTakeResultSummary> stockTakeResultSummaryList = new List<StockTakeResultSummary>();
            if (stockTakeMaster.Status == CodeMaster.StockTakeStatus.InProcess)
            {
                #region 执行中返回库存和盘点结果比较值
                IList<StockTakeResult> stockTakeResultList = CalStockTakeResult(stockTakeMaster, listShortage, listProfit, listMatch, locationList, binList, itemList, BaseInventoryDate);
                if (stockTakeMaster.IsScanHu)
                {
                    stockTakeResultSummaryList = (from tak in stockTakeResultList
                                                  group tak by new
                                                  {
                                                      Item = tak.Item,
                                                      ItemDescription = tak.ItemDescription,
                                                      Uom = tak.Uom,
                                                      Location = tak.Location,
                                                      LotNo = tak.LotNo,
                                                      Bin = tak.Bin,
                                                      StNo = tak.StNo,
                                                  }
                                                      into result
                                                      select new StockTakeResultSummary
                                                      {
                                                          StNo = result.Key.StNo,
                                                          Bin = result.Key.Bin,
                                                          Item = result.Key.Item,
                                                          ItemDescription = result.Key.ItemDescription,
                                                          Uom = result.Key.Uom,
                                                          Location = result.Key.Location,
                                                          LotNo = result.Key.LotNo,
                                                          MatchQty = result.Sum(t => t.DifferenceQty == 0 ? t.InventoryQty : 0),
                                                          ShortageQty = result.Sum(t => t.DifferenceQty < 0 ? t.InventoryQty : 0),
                                                          ProfitQty = result.Sum(t => t.DifferenceQty > 0 ? t.StockTakeQty : 0),
                                                      }).ToList();
                }
                else
                {
                    stockTakeResultSummaryList = (from tak in stockTakeResultList
                                                  select new StockTakeResultSummary
                                                  {
                                                      Item = tak.Item,
                                                      ItemDescription = tak.ItemDescription,
                                                      Uom = tak.Uom,
                                                      Location = tak.Location,
                                                      QualityType = tak.QualityType,
                                                      InventoryQty = tak.InventoryQty,
                                                      StockTakeQty = tak.StockTakeQty,
                                                      DifferenceQty = tak.DifferenceQty,
                                                  }).ToList();
                }
                #endregion
            }
            else
            {
                #region 完成后
                if (stockTakeMaster.IsScanHu)
                {
                    #region 按条码

                    #region 盘亏
                    if (listShortage)
                    {
                        #region 查询盘亏结果
                        string selectShortageStatement = @"select Item, ItemDescription, QualityType, Uom, Location, Bin, LotNo, DifferenceQty as ShortageQty ,StNo,Id,IsAdjust
                                                        from StockTakeResult where StNo = ? and DifferenceQty < 0";
                        IList<object> selectShortageParas = new List<object>();
                        selectShortageParas.Add(stockTakeMaster.StNo);
                        selectShortageStatement += GetWhereStatement(selectShortageParas, itemList, locationList, binList);
                        IList<object[]> shorageList = this.genericMgr.FindAll<object[]>(selectShortageStatement, selectShortageParas.ToArray());
                        #endregion

                        #region 转换为StockTakeResultSummary
                        ((List<StockTakeResultSummary>)stockTakeResultSummaryList).AddRange
                            ((from shorage in shorageList
                              select new StockTakeResultSummary
                              {
                                  Item = (string)shorage[0],
                                  ItemDescription = (string)shorage[1],
                                  QualityType = (CodeMaster.QualityType)shorage[2],
                                  Uom = (string)shorage[3],
                                  Location = (string)shorage[4],
                                  Bin = (string)shorage[5],
                                  LotNo = (string)shorage[6],
                                  MatchQty = 0,
                                  ShortageQty = (decimal)shorage[7],
                                  StNo = (string)shorage[8],
                                  Id = (int)shorage[9],
                                  IsAdjust = (Boolean)shorage[10],
                                  ProfitQty = 0
                              }).ToList());
                        #endregion
                    }
                    #endregion

                    #region 盘盈
                    if (listProfit)
                    {
                        #region 查询盘盈结果
                        string selectProfitStatement = @"select Item, ItemDescription, QualityType, Uom, Location, Bin, LotNo, DifferenceQty as ProfitQty ,StNo,Id,IsAdjust
                                                        from StockTakeResult where StNo = ? and DifferenceQty > 0";
                        IList<object> selectProfitParas = new List<object>();
                        selectProfitParas.Add(stockTakeMaster.StNo);
                        selectProfitStatement += GetWhereStatement(selectProfitParas, itemList, locationList, binList);
                        IList<object[]> profitList = this.genericMgr.FindAll<object[]>(selectProfitStatement, selectProfitParas.ToArray());
                        #endregion

                        #region 转换为StockTakeResultSummary
                        ((List<StockTakeResultSummary>)stockTakeResultSummaryList).AddRange
                            ((from profit in profitList
                              select new StockTakeResultSummary
                              {
                                  Item = (string)profit[0],
                                  ItemDescription = (string)profit[1],
                                  QualityType = (CodeMaster.QualityType)profit[2],
                                  Uom = (string)profit[3],
                                  Location = (string)profit[4],
                                  Bin = (string)profit[5],
                                  LotNo = (string)profit[6],
                                  MatchQty = 0,
                                  ShortageQty = 0,
                                  ProfitQty = (decimal)profit[7],
                                  StNo = (string)profit[8],
                                  Id = (int)profit[9],
                                  IsAdjust = (Boolean)profit[10],
                              }).ToList());
                        #endregion
                    }
                    #endregion

                    #region 账实相符
                    if (listMatch)
                    {
                        #region 查询盘盈结果
                        string selectMatchStatement = @"select Item, ItemDescription, QualityType, Uom, Location, Bin, LotNo, DifferenceQty as MatchQty  ,StNo,Id,IsAdjust
                                                        from StockTakeResult where StNo = ? and DifferenceQty = 0";
                        IList<object> selectMatchParas = new List<object>();
                        selectMatchParas.Add(stockTakeMaster.StNo);
                        selectMatchStatement += GetWhereStatement(selectMatchParas, itemList, locationList, binList);
                        IList<object[]> matchList = this.genericMgr.FindAll<object[]>(selectMatchStatement, selectMatchParas.ToArray());
                        #endregion

                        #region 转换为StockTakeResultSummary
                        ((List<StockTakeResultSummary>)stockTakeResultSummaryList).AddRange
                            ((from match in matchList
                              select new StockTakeResultSummary
                              {
                                  Item = (string)match[0],
                                  ItemDescription = (string)match[1],
                                  QualityType = (CodeMaster.QualityType)match[2],
                                  Uom = (string)match[3],
                                  Location = (string)match[4],
                                  Bin = (string)match[5],
                                  LotNo = (string)match[6],
                                  MatchQty = (decimal)match[7],
                                  StNo = (string)match[8],
                                  Id = (int)match[9],
                                  IsAdjust = (Boolean)match[10],
                                  ShortageQty = 0,
                                  ProfitQty = 0,
                              }).ToList());
                        #endregion
                    }
                    #endregion

                    #region 汇总
                    stockTakeResultSummaryList = (from sum in stockTakeResultSummaryList
                                                  group sum by new
                                                  {
                                                      Item = sum.Item,
                                                      ItemDescription = sum.ItemDescription,
                                                      QualityType = sum.QualityType,
                                                      Uom = sum.Uom,
                                                      Location = sum.Location,
                                                      Bin = sum.Bin,
                                                      LotNo = sum.LotNo,
                                                      StNo = sum.StNo
                                                  } into result
                                                  select new StockTakeResultSummary
                                                  {
                                                      StNo = result.Key.StNo,
                                                      Item = result.Key.Item,
                                                      ItemDescription = result.Key.ItemDescription,
                                                      Uom = result.Key.Uom,
                                                      QualityType = result.Key.QualityType,
                                                      Location = result.Key.Location,
                                                      Bin = result.Key.Bin,
                                                      ProfitQty = result.Sum(sum => sum.ProfitQty),
                                                      ShortageQty = 0 - result.Sum(sum => sum.ShortageQty),
                                                      MatchQty = result.Sum(sum => sum.MatchQty),
                                                  }
                                                  ).OrderBy(c => c.Item).ThenBy(c => c.LotNo).ToList();
                    #endregion
                    #endregion
                }
                else
                {
                    #region 按数量
                    string selectStockTakeResultStatement = "from StockTakeResult where StNo = ?";
                    IList<object> selectStockTakeResultParas = new List<object>();
                    selectStockTakeResultParas.Add(stockTakeMaster.StNo);
                    if (!(listShortage && listProfit && listMatch))
                    {
                        if (listShortage)
                        {
                            if (listProfit)
                            {
                                selectStockTakeResultStatement += " DifferenceQty <> 0";
                            }
                            else if (listMatch)
                            {
                                selectStockTakeResultStatement += " DifferenceQty <= 0";
                            }
                            else
                            {
                                selectStockTakeResultStatement += " DifferenceQty < 0";
                            }
                        }
                        else if (listProfit)
                        {
                            if (listMatch)
                            {
                                selectStockTakeResultStatement += " DifferenceQty >= 0";
                            }
                            else
                            {
                                selectStockTakeResultStatement += " DifferenceQty > 0";
                            }
                        }
                        else
                        {
                            selectStockTakeResultStatement += " DifferenceQty = 0";
                        }
                    }
                    selectStockTakeResultStatement += GetWhereStatement(selectStockTakeResultParas, itemList, locationList, binList);
                    IList<StockTakeResult> stockTakeResultList = this.genericMgr.FindAll<StockTakeResult>(selectStockTakeResultStatement, selectStockTakeResultParas.ToArray());

                    #region 汇总
                    stockTakeResultSummaryList = (from rst in stockTakeResultList
                                                  select new StockTakeResultSummary
                                                  {
                                                      IsAdjust = rst.IsAdjust,
                                                      Id = rst.Id,
                                                      Item = rst.Item,
                                                      ItemDescription = rst.ItemDescription,
                                                      QualityType = rst.QualityType,
                                                      Uom = rst.Uom,
                                                      Location = rst.Location,
                                                      InventoryQty = rst.InventoryQty,
                                                      StockTakeQty = rst.StockTakeQty,
                                                      DifferenceQty = rst.DifferenceQty,
                                                  }
                                                  ).OrderBy(c => c.Item).ThenBy(c => c.LotNo).ToList();
                    #endregion
                    #endregion
                }
                #endregion
            }

            return stockTakeResultSummaryList;
        }

        public IList<StockTakeResult> ListStockTakeResultDetail(string stNo, bool listShortage, bool listProfit, bool listMatch, IList<string> locationList, IList<string> binList, IList<string> itemList, DateTime? BaseInventoryDate)
        {
            StockTakeMaster stockTakeMaster = this.genericMgr.FindById<StockTakeMaster>(stNo);
            return ListStockTakeResultDetail(stockTakeMaster, listShortage, listProfit, listMatch, locationList, binList, itemList, BaseInventoryDate);
        }

        public IList<StockTakeResult> ListStockTakeResultDetail(StockTakeMaster stockTakeMaster, bool listShortage, bool listProfit, bool listMatch, IList<string> locationList, IList<string> binList, IList<string> itemList, DateTime? BaseInventoryDate)
        {
            if (stockTakeMaster.Status == CodeMaster.StockTakeStatus.Create
                || stockTakeMaster.Status == CodeMaster.StockTakeStatus.Cancel
                || stockTakeMaster.Status == CodeMaster.StockTakeStatus.Submit)
            {
                throw new BusinessException("盘点单{0}的状态为{1}，不能显示盘点结果。", stockTakeMaster.StNo,
                    this.systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.StockTakeStatus, ((int)stockTakeMaster.Status).ToString()));
            }

            IList<StockTakeResult> stockTakeResultList = new List<StockTakeResult>();
            if (stockTakeMaster.Status == CodeMaster.StockTakeStatus.InProcess)
            {
                #region 执行中返回库存和盘点结果比较值
                stockTakeResultList = CalStockTakeResult(stockTakeMaster, listShortage, listProfit, listMatch, locationList, binList, itemList);
                #endregion
            }
            else
            {
                #region 查询点明细结果，只有条码才有
                string selectStatement = @"from StockTakeResult where StNo = ? ";
                IList<object> selectParas = new List<object>();
                selectParas.Add(stockTakeMaster.StNo);
                if (listShortage)
                {
                    selectStatement += " and DifferenceQty<0";
                }
                else if (listProfit)
                {
                    selectStatement += " and DifferenceQty>0";
                }
                else
                {
                    selectStatement += " and DifferenceQty=0";
                }
                selectStatement += GetWhereStatement(selectParas, itemList, locationList, binList);
                stockTakeResultList = this.genericMgr.FindAll<StockTakeResult>(selectStatement, selectParas.ToArray());
                #endregion
            }
            return stockTakeResultList;
        }

        private string GetWhereStatement(IList<object> selectParas, IList<string> itemList, IList<string> locationList, IList<string> binList)
        {
            string whereStatement = string.Empty;
            if (itemList != null && itemList.Count > 0)
            {
                foreach (string item in itemList)
                {
                    if (itemList.First() == item)
                    {
                        whereStatement += " and Item in (?";
                    }
                    else
                    {
                        whereStatement += ", ?";
                    }
                    selectParas.Add(item);
                }
                whereStatement += ")";
            }

            if (locationList != null && locationList.Count > 0)
            {
                foreach (string location in locationList)
                {
                    if (locationList.First() == location)
                    {
                        whereStatement += " and Location in (?";
                    }
                    else
                    {
                        whereStatement += ", ?";
                    }
                    selectParas.Add(location);
                }
                whereStatement += ")";
            }

            if (binList != null && binList.Count > 0)
            {
                foreach (string bin in binList)
                {
                    if (binList.First() == bin)
                    {
                        whereStatement += " and Bin in (?";
                    }
                    else
                    {
                        whereStatement += ", ?";
                    }
                    selectParas.Add(bin);
                }
                whereStatement += ")";
            }

            return whereStatement;
        }

        private IList<StockTakeResult> CalStockTakeResult(StockTakeMaster stockTakeMaster, bool listShortage, bool listProfit, bool listMatch, IList<string> locationList, IList<string> binList, IList<string> itemList, DateTime? baseInventoryDate)
        {
            IList<StockTakeResult> stockTakeResultList = new List<StockTakeResult>();
            baseInventoryDate = baseInventoryDate.HasValue ? baseInventoryDate : stockTakeMaster.BaseInventoryDate;
            if (baseInventoryDate == null)
            {
                //默认取当天
                baseInventoryDate = DateTime.Now;
            }
            DateTime dateTimeNow = DateTime.Now;
            if (stockTakeMaster.IsScanHu)
            {
                #region 按条码盘点
                #region 查找库存
                IList<LocationLotDetail> locationLotDetailList = null;
                //if (baseInventoryDate.HasValue)
                if (1 != 1)
                {
                    if (binList != null && binList.Count > 0)
                    {
                        throw new BusinessException("不支持按库格查看条码历史库存。");
                    }

                    IList<HuHistoryInventory> historyInventoryList = new List<HuHistoryInventory>();
                    foreach (string location in locationList)
                    {
                        IList<HuHistoryInventory> huHistoryLocationDetails = this.locationDetailMgr.GetHuHistoryLocationDetails(location, itemList, baseInventoryDate.Value);
                        if (huHistoryLocationDetails != null && huHistoryLocationDetails.Count > 0)
                        {
                            ((List<HuHistoryInventory>)historyInventoryList).AddRange(huHistoryLocationDetails);
                        }
                    }

                    locationLotDetailList = (from inv in historyInventoryList
                                             select new LocationLotDetail
                                             {
                                                 Location = inv.Location,
                                                 Item = inv.Item,
                                                 HuId = inv.HuId,
                                                 LotNo = inv.LotNo,
                                                 Qty = inv.Qty,
                                                 QualityType = inv.QualityType
                                             }).ToList();
                }
                else
                {
                    string selectHuLocationLotDetailStatement = "from LocationLotDetail where HuId is not null";
                    IList<object> selectHuLocationLotDetailParm = new List<object>();
                    selectHuLocationLotDetailStatement += GetWhereStatement(selectHuLocationLotDetailParm, itemList, locationList, binList);
                    locationLotDetailList = this.genericMgr.FindAll<LocationLotDetail>(selectHuLocationLotDetailStatement, selectHuLocationLotDetailParm.ToArray());
                }
                #endregion

                #region 查找盘点结果
                IList<object> selectStockTakeDetailParm = new List<object>();
                selectStockTakeDetailParm.Add(stockTakeMaster.StNo);
                string thisSelectStockTakeDetailStatement = SelectStockTakeDetailStatement + GetWhereStatement(selectStockTakeDetailParm, itemList, locationList, binList);
                IList<StockTakeDetail> stockTakeDetailList = this.genericMgr.FindAll<StockTakeDetail>(thisSelectStockTakeDetailStatement, selectStockTakeDetailParm.ToArray());
                #endregion

                #region 盘亏\盘赢\账实相符
                #region 盘亏
                if (listShortage)
                {
                    if (baseInventoryDate.HasValue)
                    {
                        #region 比较历史库存，不看Bin
                        ((List<StockTakeResult>)stockTakeResultList).AddRange((from loc in locationLotDetailList
                                                                               join tak in stockTakeDetailList
                                                                               on
                                                                               new
                                                                               {
                                                                                   Location = loc.Location,
                                                                                   Item = loc.Item,
                                                                                   HuId = loc.HuId
                                                                               }
                                                                               equals
                                                                               new
                                                                               {
                                                                                   Location = tak.Location,
                                                                                   Item = tak.Item,
                                                                                   HuId = tak.HuId
                                                                               }
                                                                               into gj
                                                                               from result in gj.DefaultIfEmpty()
                                                                               where result == null
                                                                               select new StockTakeResult
                                                                               {
                                                                                   Item = loc.Item,
                                                                                   HuId = loc.HuId,
                                                                                   LotNo = loc.LotNo,
                                                                                   StockTakeQty = 0,
                                                                                   InventoryQty = loc.Qty,
                                                                                   DifferenceQty = 0 - loc.Qty,
                                                                                   Location = loc.Location,
                                                                                   Bin = string.Empty,
                                                                                   BaseInventoryDate = baseInventoryDate.Value,
                                                                                   QualityType = loc.QualityType,
                                                                               }).ToList());
                        #endregion
                    }
                    else
                    {
                        #region 比较当前库存，看Bin
                        ((List<StockTakeResult>)stockTakeResultList).AddRange((from loc in locationLotDetailList
                                                                               join tak in stockTakeDetailList
                                                                               on
                                                                               new
                                                                               {
                                                                                   Location = loc.Location,
                                                                                   Bin = loc.Bin,
                                                                                   Item = loc.Item,
                                                                                   HuId = loc.HuId
                                                                               }
                                                                               equals
                                                                                new
                                                                                {
                                                                                    Location = tak.Location,
                                                                                    Bin = tak.Bin,
                                                                                    Item = tak.Item,
                                                                                    HuId = tak.HuId
                                                                                }
                                                                               into gj
                                                                               from result in gj.DefaultIfEmpty()
                                                                               where result == null
                                                                               select new StockTakeResult
                                                                               {
                                                                                   Item = loc.Item,
                                                                                   HuId = loc.HuId,
                                                                                   LotNo = loc.LotNo,
                                                                                   StockTakeQty = 0,
                                                                                   InventoryQty = loc.Qty,
                                                                                   DifferenceQty = 0 - loc.Qty,
                                                                                   Location = loc.Location,
                                                                                   Bin = loc.Bin,
                                                                                   BaseInventoryDate = dateTimeNow,
                                                                                   QualityType = loc.QualityType,
                                                                               }).ToList());
                        #endregion
                    }
                }
                #endregion

                #region 盘盈
                if (listProfit)
                {
                    if (baseInventoryDate.HasValue)
                    {
                        #region 比较历史库存，不看Bin
                        ((List<StockTakeResult>)stockTakeResultList).AddRange((from tak in stockTakeDetailList
                                                                               join loc in locationLotDetailList
                                                                               on
                                                                               new
                                                                               {
                                                                                   Location = tak.Location,
                                                                                   Item = tak.Item,
                                                                                   HuId = tak.HuId
                                                                               }
                                                                               equals
                                                                               new
                                                                               {
                                                                                   Location = loc.Location,
                                                                                   Item = loc.Item,
                                                                                   HuId = loc.HuId
                                                                               }
                                                                               into gj
                                                                               from result in gj.DefaultIfEmpty()
                                                                               where result == null
                                                                               select new StockTakeResult
                                                                               {
                                                                                   Item = tak.Item,
                                                                                   ItemDescription = tak.ItemDescription,
                                                                                   Uom = tak.BaseUom,
                                                                                   HuId = tak.HuId,
                                                                                   LotNo = tak.LotNo,
                                                                                   StockTakeQty = tak.Qty * tak.UnitQty,
                                                                                   InventoryQty = 0,
                                                                                   DifferenceQty = tak.Qty * tak.UnitQty,
                                                                                   Location = tak.Location,
                                                                                   //Bin = string.Empty,
                                                                                   BaseInventoryDate = baseInventoryDate.Value,
                                                                                   QualityType = tak.QualityType,
                                                                               }).ToList());
                        #endregion
                    }
                    else
                    {
                        #region 比较当前库存，看Bin
                        ((List<StockTakeResult>)stockTakeResultList).AddRange((from tak in stockTakeDetailList
                                                                               join loc in locationLotDetailList
                                                                               on
                                                                               new
                                                                               {
                                                                                   Location = tak.Location,
                                                                                   Bin = tak.Bin,
                                                                                   Item = tak.Item,
                                                                                   HuId = tak.HuId
                                                                               }
                                                                               equals
                                                                               new
                                                                               {
                                                                                   Location = loc.Location,
                                                                                   Bin = loc.Bin,
                                                                                   Item = loc.Item,
                                                                                   HuId = loc.HuId
                                                                               }
                                                                               into gj
                                                                               from result in gj.DefaultIfEmpty()
                                                                               select new StockTakeResult
                                                                               {
                                                                                   Item = tak.Item,
                                                                                   ItemDescription = tak.ItemDescription,
                                                                                   Uom = tak.BaseUom,
                                                                                   HuId = tak.HuId,
                                                                                   LotNo = tak.LotNo,
                                                                                   StockTakeQty = tak.Qty * tak.UnitQty,
                                                                                   InventoryQty = 0,
                                                                                   DifferenceQty = tak.Qty * tak.UnitQty,
                                                                                   Location = tak.Location,
                                                                                   Bin = tak.Bin,
                                                                                   BaseInventoryDate = dateTimeNow,
                                                                                   QualityType = tak.QualityType,
                                                                               }).ToList());
                        #endregion
                    }
                }
                #endregion

                #region 账实相符
                if (listMatch)
                {

                    if (baseInventoryDate.HasValue)
                    {
                        #region 比较历史库存，不看Bin
                        ((List<StockTakeResult>)stockTakeResultList).AddRange((from loc in locationLotDetailList
                                                                               join tak in stockTakeDetailList
                                                                               on
                                                                               new
                                                                               {
                                                                                   Location = loc.Location,
                                                                                   Item = loc.Item,
                                                                                   HuId = loc.HuId
                                                                               }
                                                                               equals
                                                                               new
                                                                               {
                                                                                   Location = tak.Location,
                                                                                   Item = tak.Item,
                                                                                   HuId = tak.HuId
                                                                               }
                                                                               select new StockTakeResult
                                                                               {
                                                                                   Item = loc.Item,
                                                                                   ItemDescription = tak.ItemDescription,
                                                                                   Uom = tak.BaseUom,
                                                                                   HuId = loc.HuId,
                                                                                   LotNo = loc.LotNo,
                                                                                   StockTakeQty = loc.Qty,
                                                                                   InventoryQty = loc.Qty,
                                                                                   DifferenceQty = 0,
                                                                                   Location = loc.Location,
                                                                                   Bin = string.Empty,
                                                                                   BaseInventoryDate = baseInventoryDate.Value,
                                                                                   QualityType = loc.QualityType,
                                                                               }).ToList());
                        #endregion
                    }
                    else
                    {
                        #region 比较当前库存，看Bin
                        ((List<StockTakeResult>)stockTakeResultList).AddRange((from loc in locationLotDetailList
                                                                               join tak in stockTakeDetailList
                                                                               on
                                                                               new
                                                                               {
                                                                                   Location = loc.Location,
                                                                                   Bin = loc.Bin,
                                                                                   Item = loc.Item,
                                                                                   HuId = loc.HuId
                                                                               }
                                                                               equals
                                                                                new
                                                                                {
                                                                                    Location = tak.Location,
                                                                                    Bin = tak.Bin,
                                                                                    Item = tak.Item,
                                                                                    HuId = tak.HuId
                                                                                }
                                                                               select new StockTakeResult
                                                                               {
                                                                                   Item = loc.Item,
                                                                                   ItemDescription = tak.ItemDescription,
                                                                                   Uom = tak.BaseUom,
                                                                                   HuId = loc.HuId,
                                                                                   LotNo = loc.LotNo,
                                                                                   StockTakeQty = loc.Qty,
                                                                                   InventoryQty = loc.Qty,
                                                                                   DifferenceQty = 0,
                                                                                   Location = loc.Location,
                                                                                   Bin = loc.Bin,
                                                                                   BaseInventoryDate = dateTimeNow,
                                                                                   QualityType = loc.QualityType,
                                                                               }).ToList());
                        #endregion
                    }
                }
                #endregion
                #endregion
                #endregion
            }
            else
            {
                #region 按数量盘点
                #region 查找库存
                IList<LocationDetailView> locationDetailList = null;
                //if (baseInventoryDate.HasValue)
                if (false)
                {
                    IList<HistoryInventory> historyInventoryList = new List<HistoryInventory>();
                    foreach (string location in locationList)
                    {
                        ((List<HistoryInventory>)historyInventoryList).AddRange(this.locationDetailMgr.GetHistoryLocationDetails(location, itemList, baseInventoryDate.Value, "", 20, 1));
                    }

                    locationDetailList = (from inv in historyInventoryList
                                          select new LocationDetailView
                                             {
                                                 Location = inv.Location,
                                                 Item = inv.Item,
                                                 QualifyQty = inv.QualifyQty,
                                                 InspectQty = inv.InspectQty,
                                                 RejectQty = inv.RejectQty,
                                             }).ToList();
                }
                else
                {
                    string selectLocationDetailStatement = "from LocationDetailView where 1 = 1";
                    IList<object> selectHuLocationDetailParm = new List<object>();
                    selectLocationDetailStatement += GetWhereStatement(selectHuLocationDetailParm, itemList, locationList, binList);
                    locationDetailList = this.genericMgr.FindAll<LocationDetailView>(selectLocationDetailStatement, selectHuLocationDetailParm.ToArray());
                }

                #region 转换为LocationLotDetail
                #region 合格品
                IList<LocationLotDetail> locationLotDetailList = (from det in locationDetailList
                                                                  where det.QualifyQty != 0
                                                                  select new LocationLotDetail
                                                                  {
                                                                      Location = det.Location,
                                                                      Item = det.Item,
                                                                      Qty = det.QualifyQty,
                                                                      QualityType = CodeMaster.QualityType.Qualified,
                                                                  }).ToList();
                #endregion

                #region 待验品
                ((List<LocationLotDetail>)locationLotDetailList).AddRange((from det in locationDetailList
                                                                           where det.InspectQty != 0
                                                                           select new LocationLotDetail
                                                                           {
                                                                               Location = det.Location,
                                                                               Item = det.Item,
                                                                               Qty = det.InspectQty,
                                                                               QualityType = CodeMaster.QualityType.Inspect,
                                                                           }).ToList());
                #endregion

                #region 不合格品
                ((List<LocationLotDetail>)locationLotDetailList).AddRange((from det in locationDetailList
                                                                           where det.RejectQty != 0
                                                                           select new LocationLotDetail
                                                                           {
                                                                               Location = det.Location,
                                                                               Item = det.Item,
                                                                               Qty = det.RejectQty,
                                                                               QualityType = CodeMaster.QualityType.Reject,
                                                                           }).ToList());
                #endregion
                #endregion
                #endregion

                #region 查找盘点结果
                IList<object> selectStockTakeDetailParm = new List<object>();
                selectStockTakeDetailParm.Add(stockTakeMaster.StNo);
                string thisSelectStockTakeDetailStatement = SelectStockTakeDetailStatement + GetWhereStatement(selectStockTakeDetailParm, itemList, locationList, binList);
                IList<StockTakeDetail> stockTakeDetailList = this.genericMgr.FindAll<StockTakeDetail>(thisSelectStockTakeDetailStatement, selectStockTakeDetailParm.ToArray());
                #endregion

                #region 计算盘点差异
                ((List<StockTakeResult>)stockTakeResultList).AddRange(from tak in stockTakeDetailList
                                                                      join inv in locationLotDetailList
                                                                      on new { Location = tak.Location, Item = tak.Item, QualityType = tak.QualityType }
                                                                      equals new { Location = inv.Location, Item = inv.Item, QualityType = inv.QualityType }
                                                                      into gj
                                                                      from result in gj.DefaultIfEmpty()
                                                                      select new StockTakeResult
                                                                      {
                                                                          Location = tak.Location,
                                                                          Item = tak.Item,
                                                                          ItemDescription = tak.ItemDescription,
                                                                          Uom = tak.BaseUom,
                                                                          StockTakeQty = tak.Qty * tak.UnitQty,
                                                                          InventoryQty = result != null ? result.Qty : 0,
                                                                          DifferenceQty = tak.Qty * tak.UnitQty - (result != null ? result.Qty : 0),
                                                                          BaseInventoryDate = baseInventoryDate.Value,
                                                                          QualityType = tak.QualityType,
                                                                      });

                ((List<StockTakeResult>)stockTakeResultList).AddRange(from inv in locationLotDetailList
                                                                      join tak in stockTakeDetailList
                                                                      on new { Location = inv.Location, Item = inv.Item, QualityType = inv.QualityType }
                                                                      equals new { Location = tak.Location, Item = tak.Item, QualityType = tak.QualityType }
                                                                      into gj
                                                                      from result2 in gj.DefaultIfEmpty()
                                                                      where result2 == null
                                                                      select new StockTakeResult
                                                                      {
                                                                          Location = inv.Location,
                                                                          Item = inv.Item,
                                                                          StockTakeQty = 0,
                                                                          InventoryQty = inv.Qty,
                                                                          DifferenceQty = 0 - inv.Qty,
                                                                          BaseInventoryDate = baseInventoryDate.Value,
                                                                          QualityType = inv.QualityType,
                                                                      });
                #endregion

                #region 根据查询条件过滤
                if (!(listShortage && listProfit && listMatch))
                {
                    if (listShortage)
                    {
                        if (listProfit)
                        {
                            stockTakeResultList = stockTakeResultList.Where(c => c.DifferenceQty != 0).ToList();
                        }
                        else if (listMatch)
                        {
                            stockTakeResultList = stockTakeResultList.Where(c => c.DifferenceQty <= 0).ToList();
                        }
                        else
                        {
                            stockTakeResultList = stockTakeResultList.Where(c => c.DifferenceQty < 0).ToList();
                        }
                    }
                    else if (listProfit)
                    {
                        if (listMatch)
                        {
                            stockTakeResultList = stockTakeResultList.Where(c => c.DifferenceQty >= 0).ToList();
                        }
                        else
                        {
                            stockTakeResultList = stockTakeResultList.Where(c => c.DifferenceQty > 0).ToList();
                        }
                    }
                    else
                    {
                        stockTakeResultList = stockTakeResultList.Where(c => c.DifferenceQty == 0).ToList();
                    }
                }
                #endregion
                #endregion
            }

            #region 查询零件描述和基本单位
            IList<string> noDescItemList = stockTakeResultList.Where(s => string.IsNullOrWhiteSpace(s.Uom)).Select(s => s.Item).ToList();
            if (noDescItemList != null && noDescItemList.Count > 0)
            {
                string selectItemStatement = string.Empty;
                IList<object> selectItemParm = new List<object>();

                foreach (string item in noDescItemList)
                {
                    if (selectItemStatement == string.Empty)
                    {
                        selectItemStatement = "from Item where Code in (?";
                    }
                    else
                    {
                        selectItemStatement += ",?";
                    }
                    selectItemParm.Add(item);
                }
                selectItemStatement += ")";

                IList<Item> itemList2 = this.genericMgr.FindAll<Item>(selectItemStatement, selectItemParm.ToArray());

                foreach (StockTakeResult stockTakeResult in stockTakeResultList.Where(s => string.IsNullOrWhiteSpace(s.Uom)))
                {
                    Item item = itemList2.Where(i => i.Code == stockTakeResult.Item).Single();
                    stockTakeResult.ItemDescription = item.Description;
                    stockTakeResult.Uom = item.Uom;
                }
            }
            #endregion

            stockTakeResultList = stockTakeResultList.OrderBy(c => c.Item).ThenBy(c => c.DifferenceQty).ToList();

            return stockTakeResultList;
        }


        private IList<StockTakeResult> CalStockTakeResult(StockTakeMaster stockTakeMaster, bool listShortage, bool listProfit, bool listMatch, IList<string> locationList, IList<string> binList, IList<string> itemList)
        {
            IList<StockTakeResult> stockTakeResultList = new List<StockTakeResult>();
            DateTime dateTimeNow = DateTime.Now;
            if (true)
            {
                //if (locationList == null || locationList.Count == 0)
                //{
                //    var stockLocs = this.genericMgr.FindAll<StockTakeLocation>(" from StockTakeLocation as s where s.StNo=? ",stockTakeMaster.StNo);
                //    locationList = stockLocs.Select(s => s.Location).ToList();
                //}
                #region 按数量盘点
                #region 查找库存
                IList<StockTakeLocationLotDet> locationDetailList = null;

                string selectLocationDetailStatement = "from StockTakeLocationLotDet as lot where 1 = 1 and lot.RefNo=? and exists( select 1 from StockTakeLocation as s where s.Location=lot.Location and s.StNo=? )";
                IList<object> selectHuLocationDetailParm = new List<object>();
                selectHuLocationDetailParm.Add(stockTakeMaster.RefNo);
                selectHuLocationDetailParm.Add(stockTakeMaster.StNo);
                selectLocationDetailStatement += GetWhereStatement(selectHuLocationDetailParm, itemList, locationList, binList);
                locationDetailList = this.genericMgr.FindAll<StockTakeLocationLotDet>(selectLocationDetailStatement, selectHuLocationDetailParm.ToArray());

                #endregion

                #region 查找盘点结果
                IList<object> selectStockTakeDetailParm = new List<object>();
                selectStockTakeDetailParm.Add(stockTakeMaster.StNo);
                string thisSelectStockTakeDetailStatement = SelectStockTakeDetailStatement + GetWhereStatement(selectStockTakeDetailParm, itemList, locationList, binList);
                IList<StockTakeDetail> stockTakeDetailList = this.genericMgr.FindAll<StockTakeDetail>(thisSelectStockTakeDetailStatement, selectStockTakeDetailParm.ToArray());
                #endregion

                #region 计算盘点差异
                ((List<StockTakeResult>)stockTakeResultList).AddRange(from tak in stockTakeDetailList
                                                                      join inv in locationDetailList
                                                                      on new { Location = tak.Location, Item = tak.Item, QualityType = tak.QualityType, IsConsigement = tak.IsConsigement, CSSupplier = tak.CSSupplier }
                                                                      equals new { Location = inv.Location, Item = inv.Item, QualityType = inv.QualityType, IsConsigement = inv.IsConsigement, CSSupplier = inv.CSSupplier }
                                                                      into gj
                                                                      from result in gj.DefaultIfEmpty()
                                                                      select new StockTakeResult
                                                                      {
                                                                          Location = tak.Location,
                                                                          Item = tak.Item,
                                                                          ItemDescription = tak.ItemDescription,
                                                                          RefItemCode = tak.RefItemCode,
                                                                          Uom = tak.BaseUom,
                                                                          StockTakeQty = tak.Qty * tak.UnitQty,
                                                                          InventoryQty = result != null ? result.Qty : 0,
                                                                          DifferenceQty = tak.Qty * tak.UnitQty - (result != null ? result.Qty : 0),
                                                                          //BaseInventoryDate = baseInventoryDate.Value,
                                                                          QualityType = tak.QualityType,
                                                                          IsConsigement = tak.IsConsigement,
                                                                          CSSupplier = tak.CSSupplier,
                                                                      });

                //库存中有 盘点明细中没有
                ((List<StockTakeResult>)stockTakeResultList).AddRange(from inv in locationDetailList
                                                                      join tak in stockTakeDetailList
                                                                      on new { Location = inv.Location, Item = inv.Item, QualityType = inv.QualityType }
                                                                      equals new { Location = tak.Location, Item = tak.Item, QualityType = tak.QualityType }
                                                                      into gj
                                                                      from result2 in gj.DefaultIfEmpty()
                                                                      where result2 == null
                                                                      select new StockTakeResult
                                                                      {
                                                                          Location = inv.Location,
                                                                          Item = inv.Item,
                                                                          ItemDescription = inv.ItemDesc,
                                                                          RefItemCode = inv.RefItemCode,
                                                                          Uom = inv.Uom,
                                                                          StockTakeQty = 0,
                                                                          InventoryQty = inv.Qty,
                                                                          DifferenceQty = 0 - inv.Qty,
                                                                          //BaseInventoryDate = baseInventoryDate.Value,
                                                                          QualityType = inv.QualityType,
                                                                          IsConsigement = inv.IsConsigement,
                                                                          CSSupplier = inv.CSSupplier,
                                                                      });
                #endregion

                #region 根据查询条件过滤
                if (!(listShortage && listProfit && listMatch))
                {
                    if (listShortage)
                    {
                        if (listProfit)
                        {
                            stockTakeResultList = stockTakeResultList.Where(c => c.DifferenceQty != 0).ToList();
                        }
                        else if (listMatch)
                        {
                            stockTakeResultList = stockTakeResultList.Where(c => c.DifferenceQty <= 0).ToList();
                        }
                        else
                        {
                            stockTakeResultList = stockTakeResultList.Where(c => c.DifferenceQty < 0).ToList();
                        }
                    }
                    else if (listProfit)
                    {
                        if (listMatch)
                        {
                            stockTakeResultList = stockTakeResultList.Where(c => c.DifferenceQty >= 0).ToList();
                        }
                        else
                        {
                            stockTakeResultList = stockTakeResultList.Where(c => c.DifferenceQty > 0).ToList();
                        }
                    }
                    else
                    {
                        stockTakeResultList = stockTakeResultList.Where(c => c.DifferenceQty == 0).ToList();
                    }
                }
                #endregion
                #endregion
            }

            stockTakeResultList = stockTakeResultList.OrderBy(c => c.Item).ThenBy(c => c.DifferenceQty).ToList();

            return stockTakeResultList;
        }
        #endregion

        #region 盘点完成
        [Transaction(TransactionMode.Requires)]
        public void CompleteStockTakeMaster(string stNo, DateTime? baseInventoryDate)
        {
            StockTakeMaster stockTakeMaster = this.genericMgr.FindById<StockTakeMaster>(stNo);
            CompleteStockTakeMaster(stockTakeMaster, baseInventoryDate);
        }

        [Transaction(TransactionMode.Requires)]
        public void CompleteStockTakeMaster(StockTakeMaster stockTakeMaster, DateTime? baseInventoryDate)
        {
            if (stockTakeMaster.Status != CodeMaster.StockTakeStatus.InProcess)
            {
                throw new BusinessException("盘点单{0}的状态为{1}，不能完工。", stockTakeMaster.StNo,
                    this.systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.StockTakeStatus, ((int)stockTakeMaster.Status).ToString()));
            }

            IList<string> stockTakeLocationList = this.genericMgr.FindAll<string>(SelectStockTakeLocationStatement, stockTakeMaster.StNo);

            IList<StockTakeResult> resultList = CalStockTakeResult(stockTakeMaster, true, true, true, stockTakeLocationList.ToList(), null, null);
            if (resultList != null && resultList.Count > 0)
            {
                foreach (StockTakeResult result in resultList)
                {
                    result.StNo = stockTakeMaster.StNo;
                    result.EffectiveDate = stockTakeMaster.EffectiveDate;
                    this.genericMgr.Create(result);
                }
            }

            stockTakeMaster.CompleteDate = DateTime.Now;
            User user = SecurityContextHolder.Get();
            stockTakeMaster.CompleteUserId = user.Id;
            stockTakeMaster.Status = CodeMaster.StockTakeStatus.Complete;
            stockTakeMaster.CompleteUserName = user.FullName;

            this.genericMgr.Update(stockTakeMaster);

            TryCloseStockTakeMaster(stockTakeMaster);
        }
        #endregion

        #region 盘点调整
        [Transaction(TransactionMode.Requires)]
        public void AdjustStockTakeResult(string stNo, DateTime? effectiveDate)
        {
            IList<StockTakeResult> stockTakeResultList = this.genericMgr.FindAll<StockTakeResult>("from StockTakeResult where StNo = ? and IsAdjust = ?", new object[] { stNo, false });
            AdjustStockTakeResult(stockTakeResultList, effectiveDate);
        }

        [Transaction(TransactionMode.Requires)]
        public void AdjustStockTakeResult(IList<int> stockTakeResultIdList, DateTime? effectiveDate)
        {
            if (stockTakeResultIdList != null && stockTakeResultIdList.Count > 0)
            {
                string hql = string.Empty;
                IList<object> parm = new List<object>();
                List<StockTakeResult> stockTakeResultList = new List<StockTakeResult>();

                int count = 0;
                foreach (int id in stockTakeResultIdList)
                {
                    if (hql == string.Empty)
                    {
                        hql = "from StockTakeResult where Id in (?";
                    }
                    else
                    {
                        hql += ",?";
                    }
                    parm.Add(id);
                    count++;

                    if (count == 1000)
                    {
                        hql += ")";
                        stockTakeResultList.AddRange(this.genericMgr.FindAll<StockTakeResult>(hql, parm.ToArray()));

                        count = 0;
                        hql = string.Empty;
                        parm = new List<object>();
                    }
                }

                if (count > 0)
                {
                    hql += ")";
                    stockTakeResultList.AddRange(this.genericMgr.FindAll<StockTakeResult>(hql, parm.ToArray()));
                }

                AdjustStockTakeResult(stockTakeResultList, effectiveDate);
            }
            else
            {
                throw new BusinessException("盘点调整结果不能为空。");
            }
        }

        [Transaction(TransactionMode.Requires)]
        public void AdjustStockTakeResult(IList<StockTakeResult> stockTakeResultList, DateTime? effectiveDate)
        {
            #region 检查
            IList<StockTakeResult> notAjustStockTakeResultList = stockTakeResultList.Where(s => !s.IsAdjust).ToList();

            if (notAjustStockTakeResultList == null || notAjustStockTakeResultList.Count == 0)
            {
                throw new BusinessException("盘点调整结果不能为空。");
            }

            if (notAjustStockTakeResultList.Where(s => s.QualityType == CodeMaster.QualityType.Inspect).Count() > 0)
            {
                throw new BusinessException("不能调整质量状态为{0}的盘点结果。",
                    this.systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.QualityType, ((int)CodeMaster.QualityType.Inspect).ToString()));
            }
            string stNo = notAjustStockTakeResultList.Select(s => s.StNo).Distinct().Single();
            StockTakeMaster stockTakeMaster = this.genericMgr.FindById<StockTakeMaster>(stNo);

            if (stockTakeMaster.Status != CodeMaster.StockTakeStatus.Complete)
            {
                throw new BusinessException("盘点单{0}的状态为{1}，不能调整盘点结果。", stockTakeMaster.StNo,
                    this.systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.StockTakeStatus, ((int)stockTakeMaster.Status).ToString()));
            }
            #endregion

            #region 更新盘点结果
            foreach (StockTakeResult stockTakeResult in notAjustStockTakeResultList)
            {
                stockTakeResult.IsAdjust = true;
                this.genericMgr.Update(stockTakeResult);
            }
            #endregion

            #region 尝试关闭订单
            TryCloseStockTakeMaster(stockTakeMaster);
            #endregion

            #region 更新库存
            if (effectiveDate.HasValue)
            {
                this.locationDetailMgr.StockTakeAdjust(stockTakeMaster, notAjustStockTakeResultList, effectiveDate.Value);
            }
            else
            {
                this.locationDetailMgr.StockTakeAdjust(stockTakeMaster, notAjustStockTakeResultList);
            }
            #endregion
        }
        #endregion

        #region 关闭盘点单
        [Transaction(TransactionMode.Requires)]
        public void ManualCloseStockTakeMaster(string stNo)
        {
            this.ManualCloseStockTakeMaster(this.genericMgr.FindById<StockTakeMaster>(stNo));
        }

        [Transaction(TransactionMode.Requires)]
        public void ManualCloseStockTakeMaster(StockTakeMaster stockTakeMaster)
        {
            if (stockTakeMaster.Status != CodeMaster.StockTakeStatus.Complete)
            {
                throw new BusinessException("盘点单{0}的状态为{1}，不能关闭。", stockTakeMaster.StNo,
                    this.systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.StockTakeStatus, ((int)stockTakeMaster.Status).ToString()));
            }

            DoCloseStockTakeMaster(stockTakeMaster);
        }

        private void TryCloseStockTakeMaster(StockTakeMaster stockTakeMaster)
        {
            if (stockTakeMaster.Status == CodeMaster.StockTakeStatus.Complete)
            {
                this.genericMgr.FlushSession();

                long counter = this.genericMgr.FindAll<long>("select count(*) as counter from StockTakeResult where StNo = ? and IsAdjust = ?",
                    new Object[] { stockTakeMaster.StNo, false })[0];

                if (counter == 0)
                {
                    DoCloseStockTakeMaster(stockTakeMaster);
                }
            }
        }

        private void DoCloseStockTakeMaster(StockTakeMaster stockTakeMaster)
        {
            stockTakeMaster.CloseDate = DateTime.Now;
            stockTakeMaster.Status = CodeMaster.StockTakeStatus.Close;
            User user = SecurityContextHolder.Get();
            stockTakeMaster.CloseUserId = user.Id;
            stockTakeMaster.CloseUserName = user.FullName;

            this.genericMgr.Update(stockTakeMaster);
        }
        #endregion

        #region 添加盘点明细
        public void BatchUpdateStockTakeDetails(string stNo,
           IList<StockTakeDetail> addStockDetailList, IList<StockTakeDetail> updateStockDetailList, IList<StockTakeDetail> deleteStockDetailList)
        {
            BatchUpdateStockTakeDetails(this.genericMgr.FindById<StockTakeMaster>(stNo), addStockDetailList, updateStockDetailList, deleteStockDetailList);
        }

        [Transaction(TransactionMode.Requires)]
        public void BatchUpdateStockTakeDetails(StockTakeMaster stockTakeMaster,
            IList<StockTakeDetail> addStockDetailList, IList<StockTakeDetail> updateStockDetailList, IList<StockTakeDetail> deleteStockDetailList)
        {
            if (stockTakeMaster.Status != CodeMaster.StockTakeStatus.InProcess)
            {

                throw new BusinessException("盘点单{0}的状态为{1}不能修改明细。",
                      stockTakeMaster.StNo, systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.StockTakeStatus, ((int)stockTakeMaster.Status).ToString()));

            }

            #region 新增盘点明细
            if (addStockDetailList != null && addStockDetailList.Count > 0)
            {
                #region 数量处理
                foreach (StockTakeDetail stockTakeDetail in addStockDetailList)
                {
                    Item item = this.genericMgr.FindById<Item>(stockTakeDetail.Item);

                    stockTakeDetail.StNo = stockTakeMaster.StNo;
                    stockTakeDetail.ItemDescription = item.Description;
                    if (stockTakeDetail.Uom == null)
                    {
                        stockTakeDetail.Uom = item.Uom;
                    }
                    stockTakeDetail.BaseUom = item.Uom;
                    if (stockTakeDetail.Uom != stockTakeDetail.BaseUom)
                    {
                        stockTakeDetail.UnitQty = this.itemMgr.ConvertItemUomQty(stockTakeDetail.Item, stockTakeDetail.BaseUom, 1, stockTakeDetail.Uom);
                    }
                    else
                    {
                        stockTakeDetail.UnitQty = 1;
                    }

                    this.genericMgr.Create(stockTakeDetail);

                    if (stockTakeMaster.StockTakeDetails == null)
                    {
                        stockTakeMaster.StockTakeDetails = new List<StockTakeDetail>();
                    }
                    stockTakeMaster.StockTakeDetails.Add(stockTakeDetail);
                }
                #endregion
            }
            #endregion

            #region 修改盘点明细
            if (updateStockDetailList != null && updateStockDetailList.Count > 0)
            {
                foreach (StockTakeDetail stockTakeDetail in updateStockDetailList)
                {
                    if (stockTakeDetail.Uom != stockTakeDetail.BaseUom)
                    {
                        stockTakeDetail.UnitQty = this.itemMgr.ConvertItemUomQty(stockTakeDetail.Item, stockTakeDetail.BaseUom, 1, stockTakeDetail.Uom);
                    }
                    else
                    {
                        stockTakeDetail.UnitQty = 1;
                    }
                    this.genericMgr.Update(stockTakeDetail);
                }
            }
            #endregion

            #region 删除盘点明细
            if (deleteStockDetailList != null && deleteStockDetailList.Count > 0)
            {
                #region 数量处理
                foreach (StockTakeDetail stockTakeDetail in deleteStockDetailList)
                {
                    this.genericMgr.Delete(stockTakeDetail);
                }
                #endregion
            }
            #endregion
        }
        #endregion

        #region 导入盘点明细
        //[Transaction(TransactionMode.Unspecified)]
        //public void ImportStockTakeDetailFromXls(Stream inputStream, string stNo)
        //{
        //    if (inputStream.Length == 0)
        //    {
        //        throw new BusinessException("Import.Stream.Empty");
        //    }

        //    #region 清空明细
        //    string hql = @"from StockTakeDetail as s where s.StNo = ?";
        //    genericMgr.Delete(hql, new object[] { stNo }, new IType[] { NHibernateUtil.String });
        //    #endregion

        //    HSSFWorkbook workbook = new HSSFWorkbook(inputStream);

        //    ISheet sheet = workbook.GetSheetAt(0);
        //    IEnumerator rows = sheet.GetRowEnumerator();

        //    ImportHelper.JumpRows(rows, 11);

        //    #region 列定义
        //    int colItem = 1;//物料代码
        //    int colUom = 3;//单位
        //    int colLocation = 4;// 库位
        //    int colQty = 5;//数量
        //    int colHu = 6;//条码
        //    int colBin = 7;//库格
        //    #endregion

        //    DateTime dateTimeNow = DateTime.Now;
        //    IList<StockTakeDetail> stockTakeDetailList = new List<StockTakeDetail>();
        //    while (rows.MoveNext())
        //    {
        //        HSSFRow row = (HSSFRow)rows.Current;
        //        if (!ImportHelper.CheckValidDataRow(row, 1, 9))
        //        {
        //            break;//边界
        //        }

        //        if (row.GetCell(colHu) == null || row.GetCell(colHu).ToString() == string.Empty)
        //        {
        //            string itemCode = string.Empty;
        //            decimal qty = 0;
        //            string uomCode = string.Empty;
        //            string locationCode = string.Empty;

        //            #region 读取数据
        //            #region 读取物料代码
        //            itemCode = ImportHelper.GetCellStringValue(row.GetCell(colItem));
        //            if (itemCode == null || itemCode.Trim() == string.Empty)
        //            {
        //                ImportHelper.ThrowCommonError(row.RowNum, colItem, row.GetCell(colItem));
        //            }

        //            #endregion
        //            #region 读取单位
        //            uomCode = row.GetCell(colUom) != null ? row.GetCell(colUom).StringCellValue : string.Empty;
        //            if (uomCode == null || uomCode.Trim() == string.Empty)
        //            {
        //                throw new BusinessException("Import.Read.Error.Empty", (row.RowNum + 1).ToString(), colUom.ToString());
        //            }
        //            #endregion

        //            #endregion

        //            #region 读取库位
        //            locationCode = row.GetCell(colLocation) != null ? row.GetCell(colLocation).StringCellValue : string.Empty;
        //            if (locationCode == null || locationCode.Trim() == string.Empty)
        //            {
        //                throw new BusinessException("Import.Read.Error.Empty", (row.RowNum + 1).ToString(), colUom.ToString());
        //            }

        //            #region 读取数量
        //            try
        //            {
        //                qty = Convert.ToDecimal(row.GetCell(colQty).NumericCellValue);
        //            }
        //            catch
        //            {
        //                ImportHelper.ThrowCommonError(row.RowNum, colQty, row.GetCell(colQty));
        //            }
        //            #endregion

        //            #endregion

        //            #region 校验物料,库位
        //            var i = (
        //               from c in stockTakeDetailList
        //               where c.Item.Trim().ToUpper() == itemCode.Trim().ToUpper() && c.Location.Trim().ToUpper() == locationCode.Trim().ToUpper()
        //               select c).Count();

        //            if (i > 0)
        //            {
        //                throw new BusinessException("Import.Business.Error.Duplicate", itemCode, (row.RowNum + 1).ToString(), (colItem + 1).ToString());
        //            }
        //            #endregion

        //            #region 填充数据
        //            Item item = genericMgr.FindById<Item>(itemCode);
        //            Uom uom = genericMgr.FindById<Uom>(uomCode);
        //            Location location = genericMgr.FindById<Location>(locationCode);


        //            StockTakeDetail stockTakeDetail = new StockTakeDetail();

        //                stockTakeDetail.StNo = stNo;
        //                stockTakeDetail.Item = itemCode;
        //                stockTakeDetail.Uom = uomCode;
        //                stockTakeDetail.Location = location.Code;
        //                stockTakeDetail.BaseUom = item.Uom;
        //                stockTakeDetail.Qty = qty;
        //                stockTakeDetailList.Add(stockTakeDetail);


        //            #endregion
        //        }
        //        else
        //        {
        //            string huId = string.Empty;
        //            string binCode = string.Empty;
        //            string locationCode = string.Empty;

        //            #region 读取数据
        //            #region 读取条码
        //            huId = row.GetCell(colHu) != null ? row.GetCell(colHu).StringCellValue : string.Empty;
        //            if (string.IsNullOrEmpty(huId))
        //            {
        //                throw new BusinessException("Import.Read.Error.Empty", (row.RowNum + 1).ToString(), colHu.ToString());
        //            }
        //            var i = (
        //                from c in stockTakeDetailList
        //                where c.HuId != null && c.HuId.Trim().ToUpper() == huId.Trim().ToUpper()
        //                select c).Count();

        //            if (i > 0)
        //            {
        //                throw new BusinessException("Import.Business.Error.Duplicate", huId, (row.RowNum + 1).ToString(), colHu.ToString());
        //            }
        //            #endregion

        //            #region 读取库位
        //            locationCode = row.GetCell(colLocation) != null ? row.GetCell(colLocation).StringCellValue : string.Empty;
        //            if (locationCode == null || locationCode.Trim() == string.Empty)
        //            {
        //                throw new BusinessException("Import.Read.Error.Empty", (row.RowNum + 1).ToString(), colUom.ToString());
        //            }

        //            #region 读取库格
        //            binCode = row.GetCell(colBin) != null ? row.GetCell(colBin).StringCellValue : null;
        //            #endregion
        //            #endregion

        //            #endregion

        //            #region 填充数据
        //            Hu hu = genericMgr.FindById<Hu>(huId);

        //            Location location = genericMgr.FindById<Location>(locationCode);

        //            LocationBin bin = null;
        //            if (binCode != null && binCode.Trim() != string.Empty)
        //            {
        //                bin = genericMgr.FindById<LocationBin>(binCode);
        //            }


        //            StockTakeDetail stockTakeDetail = new StockTakeDetail();
        //            stockTakeDetail.StNo = stNo;
        //            stockTakeDetail.Item = hu.Item;
        //            stockTakeDetail.Qty = hu.Qty;
        //            stockTakeDetail.Uom = hu.Uom;
        //            stockTakeDetail.BaseUom = hu.BaseUom;
        //            stockTakeDetail.HuId = hu.HuId;
        //            stockTakeDetail.LotNo = hu.LotNo;
        //            stockTakeDetail.Location = location.Code;
        //            stockTakeDetail.Bin = bin.Code;
        //            stockTakeDetailList.Add(stockTakeDetail);
        //            #endregion
        //        }
        //    }

        //    if (stockTakeDetailList.Count == 0)
        //    {
        //        throw new BusinessException("Import.Result.Error.ImportNothing");
        //    }
        //    BatchUpdateStockTakeDetails(stNo, stockTakeDetailList, null, null);

        //}
        #endregion

        #region 导入盘点明细
        [Transaction(TransactionMode.Requires)]
        public void ImportStockTakeDetailFromXls(Stream inputStream, string stNo)
        {
            if (inputStream.Length == 0)
            {
                throw new BusinessException("Import.Stream.Empty");
            }

            #region 清空明细
            //string hql = @"from StockTakeDetail as s where s.StNo = ?";
            //genericMgr.Delete(hql, new object[] { stNo }, new IType[] { NHibernateUtil.String });
            //genericMgr.FlushSession();
            //genericMgr.CleanSession();
            #endregion

            HSSFWorkbook workbook = new HSSFWorkbook(inputStream);

            ISheet sheet = workbook.GetSheetAt(0);
            IEnumerator rows = sheet.GetRowEnumerator();

            var cell = sheet.GetRow(1).Cells[0];
            var cellStNo = ImportHelper.GetCellStringValue(cell);
            if (cellStNo != stNo)
            {
                throw new BusinessException("导入的模板不正确,盘点单号不一致");
            }

            ImportHelper.JumpRows(rows, 11);

            #region 列定义
            //质量类型	寄售	寄售供应商	盘点数

            int colLocation = 1;// 库位
            int colItem = 2;//物料代码
            int colQualitype = 5;//质量类型
            int colIsConsigement = 6;//是否寄售
            int colCSSupplier = 7;//寄售供应商
            int colQty = 8;//盘点数
            #endregion

            DateTime dateTimeNow = DateTime.Now;
            BusinessException errorMessage = new BusinessException();
            IList<StockTakeDetail> stockTakeDetailList = new List<StockTakeDetail>();
            IList<StockTakeDetail> existsList = this.genericMgr.FindAll<StockTakeDetail>(" from StockTakeDetail as d where d.StNo=?", stNo);
            IList<StockTakeLocation> allLocs = genericMgr.FindAll<StockTakeLocation>(" from StockTakeLocation as s where s.StNo=? ", stNo);
            IList<Item> allItems = genericMgr.FindAll<Item>();
            int rowCount = 11;
            while (rows.MoveNext())
            {
                rowCount++;
                HSSFRow row = (HSSFRow)rows.Current;
                if (!ImportHelper.CheckValidDataRow(row, 1, 8))
                {
                    break;//边界
                }

                StockTakeDetail stockTakeDetail = new StockTakeDetail();
                string locationCode = string.Empty;
                string itemCode = string.Empty;
                string qualityType = string.Empty;
                bool isConsigement = false;
                string csSupplier = string.Empty;
                decimal qty = 0;

                #region 读取数据
                #region 读取库位
                locationCode = ImportHelper.GetCellStringValue(row.GetCell(colLocation));
                if (string.IsNullOrWhiteSpace(locationCode))
                {
                    errorMessage.AddMessage(string.Format("第{0}行：库位不能为空。", rowCount));
                    continue;
                }
                else
                {
                    var locationList = allLocs.Where(a => a.Location == locationCode);
                    if (locationList == null && locationList.Count() == 0)
                    {
                        errorMessage.AddMessage(string.Format("第{0}行：库位{1}不存在盘点单的盘点库位中", rowCount, locationCode));
                        continue;
                    }
                    else
                    {
                        stockTakeDetail.Location = locationCode;
                    }
                }

                //  Location locationFrom = genericMgr.FindById<Location>(locationFromCode);


                #endregion

                #region 读取物料代码
                itemCode = ImportHelper.GetCellStringValue(row.GetCell(colItem));
                if (string.IsNullOrWhiteSpace(itemCode))
                {
                    errorMessage.AddMessage(string.Format("第{0}行：物料代码不能为空。", rowCount));
                    continue;
                }
                else
                {
                    var items = allItems.Where(a => a.Code == itemCode);
                    if (items == null && items.Count() == 0)
                    {
                        errorMessage.AddMessage(string.Format("第{0}行：物料代码{1}不存在", rowCount, itemCode));
                        continue;
                    }
                    else
                    {
                        var currentItem = items.First();
                        stockTakeDetail.Item = currentItem.Code;
                        stockTakeDetail.ItemDescription = currentItem.Description;
                        stockTakeDetail.RefItemCode = currentItem.ReferenceCode;
                        stockTakeDetail.Uom = currentItem.Uom;
                        stockTakeDetail.BaseUom = currentItem.Uom;
                    }
                }

                #endregion

                #region 质量类型
                qualityType = ImportHelper.GetCellStringValue(row.GetCell(colQualitype));
                if (qualityType == "0" || qualityType == "正常")
                {
                    stockTakeDetail.QualityType = com.Sconit.CodeMaster.QualityType.Qualified;
                }
                else if (qualityType == "1" || qualityType == "待验")
                {
                    stockTakeDetail.QualityType = com.Sconit.CodeMaster.QualityType.Inspect;
                }
                else if (qualityType == "2" || qualityType == "不合格")
                {
                    stockTakeDetail.QualityType = com.Sconit.CodeMaster.QualityType.Reject;
                }
                else
                {
                    errorMessage.AddMessage(string.Format("第{0}行：质量类型{1}填写有误。", rowCount, qualityType));
                    continue;
                }
                #endregion

                #region 是否寄售
                string isConsigementRead = ImportHelper.GetCellStringValue(row.GetCell(colIsConsigement));
                if (isConsigementRead == "0" || string.IsNullOrWhiteSpace(isConsigementRead))
                {
                    stockTakeDetail.IsConsigement = false;
                }
                else if (isConsigementRead == "1" || isConsigementRead == "√")
                {
                    stockTakeDetail.IsConsigement = true;
                }
                else
                {
                    errorMessage.AddMessage(string.Format("第{0}行：是否寄售填写有误。", rowCount));
                    continue;
                }
                #endregion

                #region 寄售的才有寄售供应商
                if (stockTakeDetail.IsConsigement)
                {
                    csSupplier = ImportHelper.GetCellStringValue(row.GetCell(colCSSupplier));
                    if (string.IsNullOrWhiteSpace(csSupplier))
                    {
                        errorMessage.AddMessage(string.Format("第{0}行：寄售物料请填写寄售供应商。", rowCount));
                        continue;
                    }
                    else
                    {
                        var supplis = this.genericMgr.FindAll<Party>(" from Party as p where p.Code=? ", csSupplier);
                        if (supplis == null || supplis.Count() == 0)
                        {
                            errorMessage.AddMessage(string.Format("第{0}行：寄售供应商{1}不存在", rowCount, csSupplier));
                            continue;
                        }
                        else
                        {
                            stockTakeDetail.CSSupplier = csSupplier;
                        }
                    }
                }

                #endregion

                #region 读取数量
                try
                {
                    string qtyRead = ImportHelper.GetCellStringValue(row.GetCell(colQty));
                    if (string.IsNullOrWhiteSpace(qtyRead))
                    {
                        stockTakeDetail.Qty = qty;
                    }
                    else
                    {
                        if (decimal.TryParse(qtyRead, out qty))
                        {
                            stockTakeDetail.Qty = qty;
                        }
                        else
                        {
                            errorMessage.AddMessage(string.Format("第{0}行：数量{1}填写有误", rowCount, qty));
                            continue;
                        }
                    }
                }
                catch
                {
                    errorMessage.AddMessage(string.Format("第{0}行：数量{1}填写有误", rowCount, qty));
                    continue;
                }
                #endregion
                #endregion
                #region 检验模版中的重复
                if (stockTakeDetailList != null && stockTakeDetailList.Count > 0)
                {
                    bool isExists = stockTakeDetailList.Where(s => s.Item == stockTakeDetail.Item && s.Location == stockTakeDetail.Location && s.QualityType == stockTakeDetail.QualityType
                        && s.IsConsigement == stockTakeDetail.IsConsigement && s.CSSupplier == stockTakeDetail.CSSupplier).Count() > 0;
                    if (isExists)
                    {
                        errorMessage.AddMessage(string.Format("第{0}行：物料{1}+库位{2}+质量状态{3}+寄售类型{4}+寄售供应商{5}在模版中重复。", rowCount, stockTakeDetail.Item, stockTakeDetail.Location, stockTakeDetail.QualityType, stockTakeDetail.IsConsigement, stockTakeDetail.CSSupplier));
                        continue;
                    }
                }

                if (existsList != null && existsList.Count > 0)
                {
                    var currentDetail = existsList.Where(s => s.Item == stockTakeDetail.Item && s.Location == stockTakeDetail.Location && s.QualityType == stockTakeDetail.QualityType
                       && s.IsConsigement == stockTakeDetail.IsConsigement && s.CSSupplier == stockTakeDetail.CSSupplier);
                    if (currentDetail != null && currentDetail.Count() > 0)
                    {
                        currentDetail.First().Qty = stockTakeDetail.Qty;
                        currentDetail.First().IsUpdate = true;
                        stockTakeDetailList.Add(currentDetail.First());
                        continue;
                    }
                }
                #endregion
                stockTakeDetail.StNo = stNo;
                stockTakeDetailList.Add(stockTakeDetail);
            }


            if (errorMessage.HasMessage)
            {
                throw errorMessage;
            }

            if (stockTakeDetailList.Count == 0)
            {
                throw new BusinessException("Import.Result.Error.ImportNothing");
            }

            foreach (var stockDet in stockTakeDetailList)
            {
                if (stockDet.IsUpdate.HasValue && stockDet.IsUpdate.Value)
                {
                    this.genericMgr.Update(stockDet);
                }
                else
                {
                    this.genericMgr.Create(stockDet);
                }
            }

            BatchUpdateStockTakeDetails(stNo, stockTakeDetailList, null, null);
        }
        #endregion

        #region 工位余量盘点导入
        [Transaction(TransactionMode.Requires)]
        public void ImportOpReferenceBalanceStockXls(Stream inputStream)
        {
            if (inputStream.Length == 0)
            {
                throw new BusinessException("Import.Stream.Empty");
            }

            HSSFWorkbook workbook = new HSSFWorkbook(inputStream);

            ISheet sheet = workbook.GetSheetAt(0);
            IEnumerator rows = sheet.GetRowEnumerator();

            ImportHelper.JumpRows(rows, 10);

            #region 列定义
            int colItem = 1;//物料代码
            int colOpRef = 2;//工位
            int colQty = 3;//数量
            #endregion

            BusinessException businessException = new BusinessException();
            int rowCount = 10;
            DateTime dateTimeNow = DateTime.Now;
            IList<OpReferenceBalance> allOpReferenceBalance = this.genericMgr.FindAll<OpReferenceBalance>();
            IList<OpReferenceBalance> updateOpReferenceBalance = new List<OpReferenceBalance>();
            IList<OpReferenceBalance> InsertOpReferenceBalance = new List<OpReferenceBalance>();
            IList<OpReferenceBalance> importAll = new List<OpReferenceBalance>();
            while (rows.MoveNext())
            {
                rowCount++;
                HSSFRow row = (HSSFRow)rows.Current;
                if (!ImportHelper.CheckValidDataRow(row, 1, 9))
                {
                    break;//边界
                }
                OpReferenceBalance newOprefBlance = new OpReferenceBalance();
                string itemCode = string.Empty;
                string opRef = string.Empty;
                decimal qty = 0;

                #region 读取数据
                #region 读取物料代码
                itemCode = ImportHelper.GetCellStringValue(row.GetCell(colItem));
                if (string.IsNullOrWhiteSpace(itemCode))
                {
                    businessException.AddMessage(string.Format("第{0}行：物料代码不能为空。", rowCount));
                    continue;
                }
                else
                {
                    var items = this.genericMgr.FindAll<Item>(" select i from Item as i where i.Code=? ", itemCode);
                    if (items == null || items.Count == 0)
                    {
                        businessException.AddMessage(string.Format("第{0}行：物料代码{1}不存在。", rowCount, itemCode));
                        continue;
                    }
                }
                #endregion

                #region 读取工位
                opRef = ImportHelper.GetCellStringValue(row.GetCell(colOpRef));
                if (string.IsNullOrWhiteSpace(opRef))
                {
                    businessException.AddMessage(string.Format("第{0}行：工位不能为空。", rowCount));
                    continue;
                }


                #endregion

                #region 读取数量

                string qtyRead = ImportHelper.GetCellStringValue(row.GetCell(colQty));
                if (string.IsNullOrWhiteSpace(qtyRead))
                {
                    businessException.AddMessage(string.Format("第{0}行：库存数不能为空。", rowCount));
                    continue;
                }
                else
                {
                    if (decimal.TryParse(qtyRead, out qty))
                    {
                        if (qty < 0)
                        {
                            businessException.AddMessage(string.Format("第{0}行：库存数{1}填写不正确。", rowCount, qty));
                            continue;
                        }
                    }
                    else
                    {
                        businessException.AddMessage(string.Format("第{0}行：库存数{1}填写不正确。", rowCount, qtyRead));
                        continue;
                    }
                }
                #endregion
                #endregion

                #region 校验
                var isExistsImport = importAll != null ? importAll.Where(a => a.Item == itemCode && a.OpReference == opRef) : null;
                if (isExistsImport != null && isExistsImport.Count() > 0)
                {
                    businessException.AddMessage(string.Format("第{0}行：物料+工位+库存数在模板中重复。", rowCount));
                    continue;
                }
                var isExistsDatabase = allOpReferenceBalance.Where(a => a.Item == itemCode && a.OpReference == opRef);
                if (isExistsDatabase != null && isExistsDatabase.Count() > 0)
                {
                    newOprefBlance = isExistsDatabase.First();
                    newOprefBlance.Qty = qty;
                    updateOpReferenceBalance.Add(newOprefBlance);

                }
                else
                {
                    newOprefBlance.Qty = qty;
                    newOprefBlance.Item = itemCode;
                    newOprefBlance.OpReference = opRef;
                    InsertOpReferenceBalance.Add(newOprefBlance);
                }
                importAll.Add(newOprefBlance);
                #endregion
            }

            if (updateOpReferenceBalance.Count > 0)
            {
                foreach (var op in updateOpReferenceBalance)
                {
                    try
                    {
                        this.UpdateOpReferenceBalance(op);
                    }
                    catch (Exception ex)
                    {
                        businessException.AddMessage(string.Format("物料代码{0}+ 工位{1} +库存数{2} 导入失败，" + ex.Message, op.Item, op.OpReference, op.Qty));
                    }
                }
            }
            if (InsertOpReferenceBalance.Count > 0)
            {
                foreach (var op in InsertOpReferenceBalance)
                {
                    try
                    {
                        this.CreateOpReferenceBalance(op);
                    }
                    catch (Exception ex)
                    {
                        businessException.AddMessage(string.Format("物料代码{0}+ 工位{1} +库存数{2} 导入失败，" + ex.Message, op.Item, op.OpReference, op.Qty));
                    }
                }
            }

            if (businessException.HasMessage)
            {
                throw businessException;
            }
        }

        [Transaction(TransactionMode.Requires)]
        public void CreateOpReferenceBalance(OpReferenceBalance opReferenceBalance)
        {
            User user = SecurityContextHolder.Get();
            this.genericMgr.Create(opReferenceBalance);
            this.genericMgr.FindAllWithNativeSql(@"insert into LOG_OpRefBalanceChange(Item, OpRef, Qty, [Status], [Version], CreateDate, CreateUserId, CreateUserNm)
			values(?, ?,?, 0, 1, ?, ?, ?) ", new object[] { opReferenceBalance.Item, opReferenceBalance.OpReference, opReferenceBalance.Qty, System.DateTime.Now, user.Id, user.FullName });
        }

        [Transaction(TransactionMode.Requires)]
        public void UpdateOpReferenceBalance(OpReferenceBalance opReferenceBalance)
        {
            User user = SecurityContextHolder.Get();
            this.genericMgr.Update(opReferenceBalance);
            this.genericMgr.FindAllWithNativeSql(@"insert into LOG_OpRefBalanceChange(Item, OpRef, Qty, [Status], [Version], CreateDate, CreateUserId, CreateUserNm)
			values(?, ?,?, 1, ?, ?, ?, ?) ", new object[] { opReferenceBalance.Item, opReferenceBalance.OpReference, opReferenceBalance.Qty, opReferenceBalance.Version + 1, System.DateTime.Now, user.Id, user.FullName });
        }
        #endregion

        #region  调整导入
        [Transaction(TransactionMode.Requires)]
        public void ImportOpReferenceBalanceAdjustXls(Stream inputStream)
        {
            if (inputStream.Length == 0)
            {
                throw new BusinessException("Import.Stream.Empty");
            }

            HSSFWorkbook workbook = new HSSFWorkbook(inputStream);

            ISheet sheet = workbook.GetSheetAt(0);
            IEnumerator rows = sheet.GetRowEnumerator();

            ImportHelper.JumpRows(rows, 10);

            #region 列定义
            int colItem = 1;//物料代码
            int colOpRef = 2;//工位
            int colQty = 3;//数量
            #endregion

            BusinessException businessException = new BusinessException();
            int rowCount = 10;
            DateTime dateTimeNow = DateTime.Now;
            IList<OpReferenceBalance> allOpReferenceBalance = this.genericMgr.FindAll<OpReferenceBalance>();
            IList<OpReferenceBalance> updateOpReferenceBalance = new List<OpReferenceBalance>();
            while (rows.MoveNext())
            {
                rowCount++;
                HSSFRow row = (HSSFRow)rows.Current;
                if (!ImportHelper.CheckValidDataRow(row, 1, 9))
                {
                    break;//边界
                }
                OpReferenceBalance newOprefBlance = new OpReferenceBalance();
                string itemCode = string.Empty;
                string opRef = string.Empty;
                decimal qty = 0;

                #region 读取数据
                #region 读取物料代码
                itemCode = ImportHelper.GetCellStringValue(row.GetCell(colItem));
                if (string.IsNullOrWhiteSpace(itemCode))
                {
                    businessException.AddMessage(string.Format("第{0}行：物料代码不能为空。", rowCount));
                    continue;
                }
                else
                {
                    var items = this.genericMgr.FindAll<Item>(" select i from Item as i where i.Code=? ", itemCode);
                    if (items == null || items.Count == 0)
                    {
                        businessException.AddMessage(string.Format("第{0}行：物料代码{1}不存在。", rowCount, itemCode));
                        continue;
                    }
                }
                #endregion

                #region 读取工位
                opRef = ImportHelper.GetCellStringValue(row.GetCell(colOpRef));
                if (string.IsNullOrWhiteSpace(opRef))
                {
                    businessException.AddMessage(string.Format("第{0}行：工位不能为空。", rowCount));
                    continue;
                }


                #endregion

                #region 调整数

                string qtyRead = ImportHelper.GetCellStringValue(row.GetCell(colQty));
                if (string.IsNullOrWhiteSpace(qtyRead))
                {
                    businessException.AddMessage(string.Format("第{0}行：调整数不能为空。", rowCount));
                    continue;
                }
                else
                {
                    if (decimal.TryParse(qtyRead, out qty))
                    {

                    }
                    else
                    {
                        businessException.AddMessage(string.Format("第{0}行：调整数{1}填写不正确。", rowCount, qtyRead));
                        continue;
                    }
                }
                #endregion
                #endregion

                #region 校验
                var isExistsImport = updateOpReferenceBalance != null ? updateOpReferenceBalance.Where(a => a.Item == itemCode && a.OpReference == opRef) : null;
                if (isExistsImport != null && isExistsImport.Count() > 0)
                {
                    businessException.AddMessage(string.Format("第{0}行：物料+工位在模板中重复。", rowCount));
                    continue;
                }
                var isExistsDatabase = allOpReferenceBalance.Where(a => a.Item == itemCode && a.OpReference == opRef);
                if (isExistsDatabase != null && isExistsDatabase.Count() > 0)
                {
                    newOprefBlance = isExistsDatabase.First();
                    newOprefBlance.CurrentAdjustQty = qty;
                    updateOpReferenceBalance.Add(newOprefBlance);

                }
                else
                {
                    businessException.AddMessage(string.Format("第{0}行：物料+工位不存在。", rowCount));
                    continue;
                }
                #endregion
            }

            if (updateOpReferenceBalance.Count > 0)
            {
                foreach (var op in updateOpReferenceBalance)
                {
                    try
                    {
                        var up = this.genericMgr.FindAllWithNativeSql<object[]>(" select Qty,isnull(Version,0) as version from SCM_OpRefBalance where id=? ", op.Id)[0];
                        op.Qty = op.CurrentAdjustQty + Convert.ToDecimal(up[0]);
                        op.Version = Convert.ToInt32(up[1]);
                        this.UpdateOpReferenceBalance(op);
                    }
                    catch (Exception ex)
                    {
                        businessException.AddMessage(string.Format("物料代码{0}+ 工位{1} +调整数{2} 导入失败，" + ex.Message, op.Item, op.OpReference, op.CurrentAdjustQty));
                    }
                }
            }

            if (businessException.HasMessage)
            {
                throw businessException;
            }
        }
        #endregion


    }
}

