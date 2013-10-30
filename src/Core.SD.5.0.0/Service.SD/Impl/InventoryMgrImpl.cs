namespace com.Sconit.Service.SD.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Castle.Services.Transaction;
    using com.Sconit.Entity.Exception;
    using com.Sconit.Entity.SD.INV;
    using com.Sconit.Entity.VIEW;
    using com.Sconit.Service;
    using NHibernate;

    public class InventoryMgrImpl : com.Sconit.Service.SD.IInventoryMgr
    {
        public IGenericMgr genericMgr { get; set; }
        public IHuMgr huMgr { get; set; }
        public ILocationDetailMgr locationDetailMgr { get; set; }
        public IStockTakeMgr stockTakeMgr { get; set; }

        [Transaction(TransactionMode.Requires)]
        public Hu GetHu(string huId)
        {
            try
            {
                HuStatus huStatus = huMgr.GetHuStatus(huId);
                return Mapper.Map<HuStatus, Hu>(huStatus);
            }
            catch (ObjectNotFoundException)
            {
                throw new BusinessException(string.Format("条码{0}不在任何库位中。", huId));
            }
        }

        [Transaction(TransactionMode.Requires)]
        public Hu CloneHu(string huId, int qty)
        {
            try
            {
                Sconit.Entity.INV.Hu oldHu = genericMgr.FindById<Sconit.Entity.INV.Hu>(huId);
                return Mapper.Map <Sconit.Entity.INV.Hu,Hu>(huMgr.CloneHu(oldHu, qty));
            }
            catch (ObjectNotFoundException)
            {
                throw new BusinessException("条码克隆失败。", huId);
            }
        }

        [Transaction(TransactionMode.Requires)]
        public com.Sconit.Entity.SD.INV.StockTakeMaster GetStockTake(string stNo)
        {
            Entity.INV.StockTakeMaster stockTakeMaster = genericMgr.FindById<Entity.INV.StockTakeMaster>(stNo);
            IList<string> stockTakeLocationList = genericMgr.FindAll<string>("select stl.Location from StockTakeLocation stl where stNo=?", stNo);
            StockTakeMaster sdStockTakeMaster = Mapper.Map<Entity.INV.StockTakeMaster, StockTakeMaster>(stockTakeMaster);
            foreach (var location in stockTakeLocationList)
            {
                if (sdStockTakeMaster.Location == null)
                {
                    sdStockTakeMaster.Location = location + ";";
                }
                else
                {
                    sdStockTakeMaster.Location += location + ";";
                }
            }
            
            return sdStockTakeMaster;
        }

        /// <summary>
        /// 提交盘点结果
        /// </summary>
        /// <param name="stNo">盘点单号</param>
        /// <param name="StockTakeDetails">由多个<HuId,Bin>组成的数组</param>
        public void DoStockTake(string stNo, string[][] stockTakeDetails)
        {
            //StockTakeMaster stockTakeMaster = this.GetStockTake(stNo);
            IList<Entity.INV.StockTakeDetail> StockTakeDetailList = new List<Entity.INV.StockTakeDetail>();
            IList<string> huIds = new List<string>();
            IList<Entity.VIEW.HuStatus> huStatuses = new List<Entity.VIEW.HuStatus>();
            foreach (var item in stockTakeDetails)
            {
                huIds.Add(item[0]);
                //Entity.VIEW.HuStatus huStatus = huMgr.GetHuStatus(item[0]);
                //Entity.INV.StockTakeDetail stockTakeDetail = new Entity.INV.StockTakeDetail
                //    { 
                //      HuId = item[0], Bin = item[1], Location = item[2], 
                //      StNo = stNo,
                //      QualityType = huStatus.QualityType,
                //      Item = huStatus.Item,
                //      ItemDescription = huStatus.ItemDescription,
                //      Qty = huStatus.Qty,
                //      BaseUom = huStatus.BaseUom,
                //      Uom = huStatus.Uom,
                //      UnitQty = huStatus.UnitQty,
                //      LotNo = huStatus.LotNo
                //    };
                //StockTakeDetailList.Add(stockTakeDetail);
            }
            huStatuses = huMgr.GetHuStatus(huIds);
            for (int i = 0; i < stockTakeDetails.Length; ++i)
            {
                Entity.VIEW.HuStatus huStatus = huStatuses.Single(h => h.HuId == stockTakeDetails[i][0]);
                Entity.INV.StockTakeDetail stockTakeDetail = new Entity.INV.StockTakeDetail
                    {
                        HuId = stockTakeDetails[i][0],
                        Bin = stockTakeDetails[i][1],
                        Location = stockTakeDetails[i][2],
                        StNo = stNo,
                        QualityType = huStatus.QualityType,
                        Item = huStatus.Item,
                        ItemDescription = huStatus.ItemDescription,
                        Qty = huStatus.Qty,
                        BaseUom = huStatus.BaseUom,
                        Uom = huStatus.Uom,
                        UnitQty = huStatus.UnitQty,
                        LotNo = huStatus.LotNo
                    };
                StockTakeDetailList.Add(stockTakeDetail);
            }
            stockTakeMgr.RecordStockTakeDetail(stNo, StockTakeDetailList);
        }

        [Transaction(TransactionMode.Requires)]
        public void DoPutAway(string huId, string binCode)
        {
            if (string.IsNullOrWhiteSpace(huId))
            {
                throw new Entity.Exception.BusinessException("条码不能为空");
            }
            if (string.IsNullOrWhiteSpace(binCode))
            {
                throw new Entity.Exception.BusinessException("库格不能为空");
            }
            
            var inventoryPutList = new List<Entity.INV.InventoryPut>();
            var inventoryPut = new Entity.INV.InventoryPut();

            inventoryPut.Bin = binCode;
            inventoryPut.HuId = huId;
            inventoryPutList.Add(inventoryPut);

            this.locationDetailMgr.InventoryPut(inventoryPutList);

            //var huStatus = this.huMgr.GetHuStatus(huId);
            ////todo PickUp

            //var hu = Mapper.Map<Entity.VIEW.HuStatus, Entity.SD.INV.Hu>(huStatus);
            //return hu;
        }

        [Transaction(TransactionMode.Requires)]
        public void DoPickUp(string huId)
        {
            //todo PickUp
            var inventoryPickList = new List<Entity.INV.InventoryPick>();
            var inventoryPick = new Entity.INV.InventoryPick();
            inventoryPick.HuId = huId;
            inventoryPickList.Add(inventoryPick);
            this.locationDetailMgr.InventoryPick(inventoryPickList);

            //var huStatus = this.huMgr.GetHuStatus(huId);
            //var hu = Mapper.Map<Entity.VIEW.HuStatus, Entity.SD.INV.Hu>(huStatus);
            //return hu;
        }

        [Transaction(TransactionMode.Requires)]
        public void DoPut(List<string> huIdlist, string binCode)
        {

        }

        [Transaction(TransactionMode.Requires)]
        public void DoPick(List<string> huIdList)
        {

        }

        [Transaction(TransactionMode.Requires)]
        public void DoPack(List<string> huIdList, string location, DateTime? effDate)
        {
            if (huIdList == null || huIdList.Count == 0)
            {
                throw new Entity.Exception.BusinessException("没有装箱的明细");
            }
            var inventoryPackList = new List<Entity.INV.InventoryPack>();
            foreach (var huId in huIdList)
            {
                var inventoryPack = new Entity.INV.InventoryPack();
                inventoryPack.HuId = huId;
                inventoryPack.Location = location;
                inventoryPackList.Add(inventoryPack);
            }
            if (effDate.HasValue)
            {
                locationDetailMgr.InventoryPack(inventoryPackList, effDate.Value);
            }
            else
            {
                locationDetailMgr.InventoryPack(inventoryPackList);
            }
        }

        [Transaction(TransactionMode.Requires)]
        public void DoUnPack(List<string> huIdList, DateTime? effDate)
        {
            if (huIdList == null || huIdList.Count == 0)
            {
                throw new Entity.Exception.BusinessException("没有拆箱的明细");
            }
            var inventoryPackList = new List<Entity.INV.InventoryUnPack>();
            foreach (var huId in huIdList)
            {
                var inventoryUnPack = new Entity.INV.InventoryUnPack();
                inventoryUnPack.HuId = huId;
                inventoryPackList.Add(inventoryUnPack);
            }
            if (effDate.HasValue)
            {
                locationDetailMgr.InventoryUnPack(inventoryPackList, effDate.Value);
            }
            else
            {
                locationDetailMgr.InventoryUnPack(inventoryPackList);
            }
        }

        [Transaction(TransactionMode.Requires)]
        public void DoRePack(List<string> oldHuList, List<string> newHuList, DateTime? effDate)
        {
            if (oldHuList == null || newHuList.Count == 0)
            {
                throw new Entity.Exception.BusinessException("没有翻箱前的明细");
            }
            if (newHuList == null || newHuList.Count == 0)
            {
                throw new Entity.Exception.BusinessException("没有翻箱后的明细");
            }
            var inventoryPackList = new List<Entity.INV.InventoryRePack>();
            foreach (var huId in oldHuList)
            {
                var inventoryUnPack = new Entity.INV.InventoryRePack();
                inventoryUnPack.HuId = huId;
                inventoryUnPack.Type = CodeMaster.RePackType.Out;
                inventoryPackList.Add(inventoryUnPack);
            }
            foreach (var huId in newHuList)
            {
                var inventoryUnPack = new Entity.INV.InventoryRePack();
                inventoryUnPack.HuId = huId;
                inventoryUnPack.Type = CodeMaster.RePackType.In;
                inventoryPackList.Add(inventoryUnPack);
            }
            if (effDate.HasValue)
            {
                locationDetailMgr.InventoryRePack(inventoryPackList, effDate.Value);
            }
            else
            {
                locationDetailMgr.InventoryRePack(inventoryPackList);
            }
        }

        [Transaction(TransactionMode.Requires)]
        public void InventoryFreeze(IList<string> huIdList)
        {
            this.locationDetailMgr.InventoryFreeze(huIdList);
        }

        [Transaction(TransactionMode.Requires)]
        public void InventoryUnFreeze(IList<string> huIdList)
        {
            this.locationDetailMgr.InventoryUnFreeze(huIdList);
        }

        #region 客户化代码
        [Transaction(TransactionMode.Requires)]
        public Hu GetDistHu(string huId)
        {
            try
            {
                HuStatus huStatus = huMgr.GetHuStatus(huId);
                com.Sconit.Entity.INV.HuMapping huMapping = this.genericMgr.FindAll<com.Sconit.Entity.INV.HuMapping>("from HuMapping h where h.HuId = ? and h.IsEffective = 0", huId).FirstOrDefault();
                Hu hu = Mapper.Map<HuStatus, Hu>(huStatus);
                hu.OrderNo = huMapping.OrderNo;
                hu.OrderDetId = huMapping.OrderDetId;
                hu.IsEffective = huMapping.IsEffective;
                return hu;
            }
            catch (ObjectNotFoundException)
            {
                throw new BusinessException(string.Format("没有找到条码{0}。", huId));
            }
        }


        [Transaction(TransactionMode.Requires)]
        public Hu ResolveHu(string extHuId)
        {
            if (extHuId == null && extHuId.Length != 17)
            {
                throw new BusinessException("关键件条码长度不正确。");
            }

            string supplierShortCode = extHuId.Substring(0, 4);
            string itemShortCode = extHuId.Substring(4, 5);
            string lotNo = extHuId.Substring(9, 4);

            com.Sconit.Entity.MD.Supplier supplier = this.genericMgr.FindAll<com.Sconit.Entity.MD.Supplier>("from Supplier where ShortCode = ?", supplierShortCode).SingleOrDefault();

            if (supplier == null)
            {
                throw new BusinessException("关键件条码中的供应商短代码{0}不存在。", supplierShortCode);
            }

            com.Sconit.Entity.MD.Item item = this.genericMgr.FindAll<com.Sconit.Entity.MD.Item>("from Item where ShortCode = ?", itemShortCode).SingleOrDefault();

            if (item == null)
            {
                throw new BusinessException("关键件条码中的零件短代码{0}不存在。", itemShortCode);
            }

            item.HuQty = 1;
            item.HuUnitCount = 1;
            item.HuUom = item.Uom;
            item.LotNo = lotNo;
            item.ManufactureParty = supplier.Code;

            return Mapper.Map<com.Sconit.Entity.INV.Hu, Hu>(this.huMgr.CreateHu(item, extHuId));
        }
        #endregion
    }
}
