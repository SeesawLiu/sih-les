namespace com.Sconit.Service.SD.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Castle.Services.Transaction;
    using com.Sconit.Entity.Exception;
    using com.Sconit.Entity.INV;
    using com.Sconit.Entity.SCM;
    using com.Sconit.Entity.SD.INV;
    using com.Sconit.Entity.SD.ORD;
    using com.Sconit.Entity.VIEW;
    using com.Sconit.Entity.CUST;
    using com.Sconit.Entity.SD.MD;
    using com.Sconit.Entity.ACC;
    using com.Sconit.Entity;
    using com.Sconit.Entity.FIS;

    [Transactional]
    public class OrderMgrImpl : IOrderMgr
    {
        public IGenericMgr genericMgr { get; set; }
        public Service.IOrderMgr orderMgr { get; set; }
        public Service.IFlowMgr flowMgr { get; set; }
        public Service.IIpMgr ipMgr { get; set; }
        public Service.IMiscOrderMgr miscOrderMgr { get; set; }
        public Service.IPickListMgr pickListMgr { get; set; }
        public Service.IInspectMgr inspectMgr { get; set; }
        public Service.IHuMgr huMgr { get; set; }
        public Service.IProductionLineMgr productionLineMgr { get; set; }
        public Service.IItemMgr itemMgr { get; set; }
        public ILocationDetailMgr locationDetailMgr { get; set; }
        public Service.IKanbanScanMgr kanbanScanMgr { get; set; }
        public Service.IKanbanScanOrderMgr kanbanScanOrderMgr { get; set; }

        public com.Sconit.Entity.SD.ORD.IpDetailInput GetIpDetailInputByPickHu(string pickHu)
        {
            IList<com.Sconit.Entity.ORD.IpLocationDetail> ipLocationDetail = genericMgr.FindAll<com.Sconit.Entity.ORD.IpLocationDetail>("select i from IpLocationDetail where i.WMSSeq = ?", pickHu);
            
            IpDetailInput sdIpDetailInput = new IpDetailInput();
            if (ipLocationDetail != null && ipLocationDetail.Count == 1)
            {
                com.Sconit.Entity.ORD.IpDetail ipDetail = genericMgr.FindById<com.Sconit.Entity.ORD.IpDetail>(ipLocationDetail[0].IpDetailId);
     
                sdIpDetailInput.Id = ipDetail.Id;
                sdIpDetailInput.Item = ipDetail.Item;
                sdIpDetailInput.ItemDescription = ipDetail.ItemDescription;
                sdIpDetailInput.ReferenceItemCode = ipDetail.ReferenceItemCode;
                sdIpDetailInput.WMSSeq = ipLocationDetail[0].WMSSeq;
                sdIpDetailInput.Qty = ipLocationDetail[0].Qty;
            }
            else
                throw new BusinessException("拣货条码无法对应到唯一一条送货明细");

            return sdIpDetailInput;
        }

        public List<WMSDatFile> GetWMSDatFileByAnJiHuId(string huId)
        {
            IList<WMSDatFile> wmsDatFiles = genericMgr.FindAll<WMSDatFile>("select l from WMSDatFile as l where (l.Qty - l.ReceiveTotal + l.CancelQty > 0) and l.HuId=? ", huId);

            if (wmsDatFiles != null && wmsDatFiles.Count > 0)
            {
                foreach (WMSDatFile wmsDatFile in wmsDatFiles)
                {
                    Entity.MD.Item item = genericMgr.FindById<Entity.MD.Item>(wmsDatFile.Item);
                    wmsDatFile.ReferenceItemCode = item.ReferenceCode;
                    wmsDatFile.ItemDescription = item.Description;
                }
                return wmsDatFiles.ToList();
            }
            else
                throw new BusinessException("数据文件中不存在该安吉条码的待收货记录");
        }

        public List<WMSDatFile> GetWMSDatFileByAnJiSeqOrder(string seqOrder)
        {
            List<WMSDatFile> wmsDatFiles = genericMgr.FindAll<WMSDatFile>("select l from WMSDatFile as l where (l.Qty - l.ReceiveTotal + l.CancelQty > 0) and l.WmsNo=? ", seqOrder).ToList();

            #region 冲销的相互抵消
            foreach (WMSDatFile wMSDatFile in wmsDatFiles)
            {
                foreach (WMSDatFile wmsFile in wmsDatFiles)
                {
                    if (wMSDatFile.MoveType == null || wmsFile.MoveType == null)
                    {
                        continue;
                    }
                    if (wMSDatFile.MoveType + wMSDatFile.SOBKZ == "311" && wmsFile.MoveType + wmsFile.SOBKZ == "312" && wmsFile.Qty == wMSDatFile.Qty && wmsFile.WmsLine == wMSDatFile.WmsLine)
                    {
                        wmsFile.MoveType = null;
                        wMSDatFile.MoveType = null;
                        break;
                    }
                    else if (wMSDatFile.MoveType + wMSDatFile.SOBKZ == "311K" && wmsFile.MoveType + wmsFile.SOBKZ == "312K" && wmsFile.Qty == wMSDatFile.Qty && wmsFile.WmsLine == wMSDatFile.WmsLine)
                    {
                        wmsFile.MoveType = null;
                        wMSDatFile.MoveType = null;
                        break;
                    }

                    else if (wMSDatFile.MoveType + wMSDatFile.SOBKZ == "411" && wmsFile.MoveType + wMSDatFile.SOBKZ == "412" && wmsFile.Qty == wMSDatFile.Qty && wmsFile.WmsLine == wMSDatFile.WmsLine)
                    {
                        wmsFile.MoveType = null;
                        wMSDatFile.MoveType = null;
                        break;
                    }
                    else if (wMSDatFile.MoveType + wMSDatFile.SOBKZ == "411K" && wmsFile.MoveType + wMSDatFile.SOBKZ == "412K" && wmsFile.Qty == wMSDatFile.Qty && wmsFile.WmsLine == wMSDatFile.WmsLine)
                    {
                        wmsFile.MoveType = null;
                        wMSDatFile.MoveType = null;
                        break;
                    }
                }
            }
            #endregion
            foreach (WMSDatFile wMSDatFile in wmsDatFiles)
            {
                //if (wMSDatFile.MoveType == "312" || wMSDatFile.MoveType == "412" || wMSDatFile.MoveType == "412k" || wMSDatFile.MoveType == null)
                if (wMSDatFile.MoveType == null)
                {
                    #region 将冲销掉的改成已经处理 记录Log
                    LesINLog lesExistenceLog = genericMgr.FindAll<LesINLog>("select l from LesINLog as l where l.WMSNo=?", wMSDatFile.WMSId).FirstOrDefault();

                    #region 已经处理成功 重新发送Log
                    if (lesExistenceLog != null)
                    {

                        lesExistenceLog.IsCreateDat = false;
                        lesExistenceLog.HandResult = "S";
                        genericMgr.Update(lesExistenceLog);
                        genericMgr.Update("update WMSDatFile set IsHand=1 where Id=" + wMSDatFile.Id);
                        continue;

                    }
                    #endregion

                    #region 记录Log 改成已经处理
                    LesINLog lesInLog = new LesINLog();
                    lesInLog.Type = "MB1B";
                    if (wMSDatFile.MoveType == null)
                    {
                        lesInLog.MoveType = genericMgr.FindById<WMSDatFile>(wMSDatFile.Id).MoveType + wMSDatFile.SOBKZ;
                    }
                    else
                    {
                        lesInLog.MoveType = wMSDatFile.MoveType + wMSDatFile.SOBKZ;
                    }
                    lesInLog.Sequense = "";
                    // lesInLog.PO = (string)line[3];//
                    //lesInLog.POLine = (string)line[4];//
                    lesInLog.WMSNo = wMSDatFile.WMSId;
                    lesInLog.WMSLine = wMSDatFile.WmsLine;
                    lesInLog.Item = wMSDatFile.Item;
                    lesInLog.HandResult = "S";
                    lesInLog.FileName = wMSDatFile.FileName;
                    lesInLog.HandTime = System.DateTime.Now;
                    lesInLog.IsCreateDat = false;
                    lesInLog.ASNNo = wMSDatFile.WmsNo;
                    lesInLog.ExtNo = wMSDatFile.WmsNo;
                    wMSDatFile.MoveType = null;
                    genericMgr.Create(lesInLog);
                    genericMgr.Update("update WMSDatFile set IsHand=1 where Id=" + wMSDatFile.Id);
                    #endregion
                    #endregion
                }
            }
            List<WMSDatFile> returnWmsDatFiles = wmsDatFiles.Where(o => o.MoveType != null && o.MoveType != "312" && o.MoveType != "412").ToList();
            foreach (WMSDatFile wmsDatFile in returnWmsDatFiles)
            {
                Entity.MD.Item item = genericMgr.FindById<Entity.MD.Item>(wmsDatFile.Item);
                wmsDatFile.ReferenceItemCode = item.ReferenceCode;
                wmsDatFile.ItemDescription = item.Description;
            }
            if (returnWmsDatFiles != null && returnWmsDatFiles.Count > 0)
            {
                return returnWmsDatFiles;
            }
            else
                throw new BusinessException("数据文件中不存在该双桥拣货单的待收货记录");
        }

        public Entity.SD.ORD.OrderMaster GetOrder(string orderNo, bool includeDetail)
        {
            Entity.ORD.OrderMaster orderMaster = orderMgr.LoadOrderMaster(orderNo, includeDetail, false, false);
            var sdOrderMaster = Mapper.Map<Entity.ORD.OrderMaster, Entity.SD.ORD.OrderMaster>(orderMaster);

            if (sdOrderMaster.OrderDetails != null)
            {
                sdOrderMaster.OrderDetails = Mapper.Map<IList<Entity.ORD.OrderDetail>, List<Entity.SD.ORD.OrderDetail>>(orderMaster.OrderDetails).OrderBy(s => s.Sequence).ToList();
            }

            return sdOrderMaster;
        }

        public Entity.SD.ORD.MiscOrderMaster GetMis(string MisNo)
        {
            Entity.ORD.MiscOrderMaster miscOrderMaster = genericMgr.FindById<Entity.ORD.MiscOrderMaster>(MisNo);
            var sdMiscOrderMaster = Mapper.Map<Entity.ORD.MiscOrderMaster, Entity.SD.ORD.MiscOrderMaster>(miscOrderMaster);
            return sdMiscOrderMaster;
        }

        [Transaction(TransactionMode.Requires)]
        public void BatchUpdateMiscOrderDetails(string miscOrderNo,
            IList<string> addHuIdList)
        {
            this.miscOrderMgr.BatchUpdateMiscOrderDetails(miscOrderNo, addHuIdList, null);
        }

        [Transaction(TransactionMode.Requires)]
        public Entity.SD.ORD.IpMaster GetIp(string ipNo, bool includeDetail)
        {
            try
            {
                Entity.ORD.IpMaster ipMaster = genericMgr.FindById<Entity.ORD.IpMaster>(ipNo);
                var sdIpMaster = Mapper.Map<Entity.ORD.IpMaster, Entity.SD.ORD.IpMaster>(ipMaster);
                if (includeDetail)
                {
                    var ipDetails = this.genericMgr.FindAll<Entity.ORD.IpDetail>("from IpDetail i where i.IpNo=? and i.Type=? and i.IsClose=? order by Sequence asc",
                            new object[] { ipNo, (int)CodeMaster.IpDetailType.Normal, false });
                    sdIpMaster.IpDetails = Mapper.Map<IList<Entity.ORD.IpDetail>, List<Entity.SD.ORD.IpDetail>>(ipDetails);
                    sdIpMaster.IpDetails = sdIpMaster.IpDetails.OrderBy(i => i.Id).ToList();
                    var ipLocationDetails = this.genericMgr.FindAll<Entity.ORD.IpLocationDetail>
                            ("from IpLocationDetail i where i.IpNo = ? and i.IsClose = ? ", new object[] { ipNo, false });

                    var IpDetailInputs = new List<Entity.SD.ORD.IpDetailInput>();

                    if (ipLocationDetails != null)
                    {
                        foreach (var ipDetail in sdIpMaster.IpDetails)
                        {
                            //ipDetail.IpDetailInputs = new List<Entity.SD.ORD.IpDetailInput>();
                            foreach (var ipLocationDetail in ipLocationDetails)
                            {
                                if (ipLocationDetail.IpDetailId == ipDetail.Id
                                    && !string.IsNullOrWhiteSpace(ipLocationDetail.HuId))
                                {
                                    var ipdi = new Entity.SD.ORD.IpDetailInput();
                                    ipdi.Id = ipDetail.Id;
                                    ipdi.Qty = ipLocationDetail.Qty;
                                    ipdi.ReceiveQty = ipLocationDetail.ReceivedQty;
                                    ipdi.HuId = ipLocationDetail.HuId;
                                    ipdi.IsOriginal = true;
                                    IpDetailInputs.Add(ipdi);
                                    //ipDetail.IpDetailInputs.Add(ipdi);
                                }
                            }
                        }
                    }
                    sdIpMaster.IpDetailInputs = IpDetailInputs;
                }
                return sdIpMaster;
            }
            catch
            {
                throw new BusinessException(string.Format("送货单号{0}没有找到", ipNo));
            }
        }

        [Transaction(TransactionMode.Requires)]
        public Entity.SD.ORD.IpMaster GetIpByWmsIpNo(string wmsIpNo, bool includeDetail)
        {
            try
            {
                Entity.ORD.IpMaster ipMaster = genericMgr.FindAll<Entity.ORD.IpMaster>("from IpMaster im where im.WMSNo=?", wmsIpNo).FirstOrDefault();
                var ipNo = ipMaster.IpNo;
                var sdIpMaster = Mapper.Map<Entity.ORD.IpMaster, Entity.SD.ORD.IpMaster>(ipMaster);
                if (includeDetail)
                {
                    var ipDetails = this.genericMgr.FindAll<Entity.ORD.IpDetail>("from IpDetail i where i.IpNo=? and i.Type=? and i.IsClose=? order by Sequence asc",
                            new object[] { ipNo, (int)CodeMaster.IpDetailType.Normal, false });
                    sdIpMaster.IpDetails = Mapper.Map<IList<Entity.ORD.IpDetail>, List<Entity.SD.ORD.IpDetail>>(ipDetails);
                    sdIpMaster.IpDetails = sdIpMaster.IpDetails.OrderBy(i => i.Id).ToList();
                    var ipLocationDetails = this.genericMgr.FindAll<Entity.ORD.IpLocationDetail>
                            ("from IpLocationDetail i where i.IpNo = ? and i.IsClose = ? ", new object[] { ipNo, false });

                    var IpDetailInputs = new List<Entity.SD.ORD.IpDetailInput>();

                    if (ipLocationDetails != null)
                    {
                        foreach (var ipDetail in sdIpMaster.IpDetails)
                        {
                            //ipDetail.IpDetailInputs = new List<Entity.SD.ORD.IpDetailInput>();
                            foreach (var ipLocationDetail in ipLocationDetails)
                            {
                                if (ipLocationDetail.IpDetailId == ipDetail.Id
                                    && !string.IsNullOrWhiteSpace(ipLocationDetail.HuId))
                                {
                                    var ipdi = new Entity.SD.ORD.IpDetailInput();
                                    ipdi.Id = ipDetail.Id;
                                    ipdi.Qty = ipLocationDetail.Qty;
                                    ipdi.ReceiveQty = ipLocationDetail.ReceivedQty;
                                    ipdi.HuId = ipLocationDetail.HuId;
                                    ipdi.IsOriginal = true;
                                    IpDetailInputs.Add(ipdi);
                                    //ipDetail.IpDetailInputs.Add(ipdi);
                                }
                            }
                        }
                    }
                    sdIpMaster.IpDetailInputs = IpDetailInputs;
                }
                return sdIpMaster;
            }
            catch
            {
                throw new BusinessException(string.Format("送货单号{0}没有找到", wmsIpNo));
            }
        }

        [Transaction(TransactionMode.Requires)]
        public Entity.SD.ORD.PickListMaster GetPickList(string pickListNo, bool includeDetail)
        {
            var basePickListMaster = genericMgr.FindById<Entity.ORD.PickListMaster>(pickListNo);
            var pickListMaster = Mapper.Map<Entity.ORD.PickListMaster, Entity.SD.ORD.PickListMaster>(basePickListMaster);
            if (includeDetail)
            {
                var pickListDetails = this.genericMgr.FindAll<Entity.ORD.PickListDetail>("from PickListDetail i where i.PickListNo=?", new object[] { pickListNo });
                pickListMaster.PickListDetails = Mapper.Map<IList<Entity.ORD.PickListDetail>, List<Entity.SD.ORD.PickListDetail>>(pickListDetails);
            }
            return pickListMaster;
        }

        [Transaction(TransactionMode.Requires)]
        public Entity.SD.ORD.InspectMaster GetInspect(string inspectNo, bool includeDetail)
        {
            var baseInspectMaster = genericMgr.FindById<Entity.INP.InspectMaster>(inspectNo);
            var inspectMaster = Mapper.Map<Entity.INP.InspectMaster, Entity.SD.ORD.InspectMaster>(baseInspectMaster);
            if (includeDetail)
            {
                var baseInspectDetails = this.genericMgr.FindAll<Entity.INP.InspectDetail>("from InspectDetail i where i.IsJudge = ? and  i.InspectNo= ?  ", new object[] { false, inspectNo });
                inspectMaster.InspectDetails = Mapper.Map<IList<Entity.INP.InspectDetail>, List<Entity.SD.ORD.InspectDetail>>(baseInspectDetails);
            }
            return inspectMaster;
        }

        /// <summary>
        /// 发货
        /// </summary>
        [Transaction(TransactionMode.Requires)]
        public void DoShipOrder(List<Entity.SD.ORD.OrderDetailInput> orderDetailInputList, DateTime? effDate)
        {
            if (orderDetailInputList == null || orderDetailInputList.Count == 0)
            {
                throw new com.Sconit.Entity.Exception.BusinessException("没有要发货的明细");
            }
            IList<Entity.ORD.OrderDetail> baseOrderDetailList = new List<Entity.ORD.OrderDetail>();
            var ids = orderDetailInputList.Select(o => o.Id).Distinct();

            foreach (var id in ids)
            {
                var baseOrderDatail = genericMgr.FindById<Entity.ORD.OrderDetail>(id);
                var selectedOrderDetailInputList = orderDetailInputList.Where(o => o.Id == id);
                if (selectedOrderDetailInputList != null)
                {
                    baseOrderDatail.OrderDetailInputs = new List<Entity.ORD.OrderDetailInput>();
                    foreach (var orderDetailInput in selectedOrderDetailInputList)
                    {
                        Entity.ORD.OrderDetailInput baseOrderDetailInput = new Entity.ORD.OrderDetailInput();
                        baseOrderDetailInput.HuId = orderDetailInput.HuId;
                        baseOrderDetailInput.ShipQty = orderDetailInput.ShipQty;
                        baseOrderDetailInput.LotNo = orderDetailInput.LotNo;

                        baseOrderDatail.OrderDetailInputs.Add(baseOrderDetailInput);
                    }
                }
                baseOrderDetailList.Add(baseOrderDatail);
            }
            if (effDate.HasValue)
            {
                this.orderMgr.ShipOrder(baseOrderDetailList, effDate.Value);
            }
            else
            {
                this.orderMgr.ShipOrder(baseOrderDetailList);
            }
        }

        [Transaction(TransactionMode.Requires)]
        public void DoRepackAndShipOrder(List<Entity.SD.INV.Hu> huList, DateTime? effDate)
        {
            List<Entity.SD.ORD.OrderDetailInput> orderDetailInputList = new List<OrderDetailInput>();
            foreach (var hu in huList)
            {
                var orderDetailInput = new OrderDetailInput();
                orderDetailInput.HuId = hu.HuId;
                orderDetailInput.LotNo = hu.LotNo;
                orderDetailInput.ShipQty = hu.Qty;
                orderDetailInput.Id = hu.OrderDetId;
                orderDetailInputList.Add(orderDetailInput);
            }

            IList<Entity.ORD.OrderDetail> baseOrderDetailList = new List<Entity.ORD.OrderDetail>();
            var ids = orderDetailInputList.Select(o => o.Id).Distinct();

            foreach (var id in ids)
            {
                var baseOrderDatail = genericMgr.FindById<Entity.ORD.OrderDetail>(id);
                var selectedOrderDetailInputList = orderDetailInputList.Where(o => o.Id == id);
                if (selectedOrderDetailInputList != null)
                {
                    baseOrderDatail.OrderDetailInputs = new List<Entity.ORD.OrderDetailInput>();
                    foreach (var orderDetailInput in selectedOrderDetailInputList)
                    {
                        Entity.ORD.OrderDetailInput baseOrderDetailInput = new Entity.ORD.OrderDetailInput();
                        baseOrderDetailInput.HuId = orderDetailInput.HuId;
                        //翻包
                        HuMapping huMapping = this.genericMgr.FindAll<HuMapping>("select h from HuMapping as h where HuId = ?", orderDetailInput.HuId).SingleOrDefault();
                        if (huMapping.IsEffective == false)
                        {
                            IList<HuMapping> huMappingList = this.genericMgr.FindAll<HuMapping>("select h from HuMapping as h where OldHus = ?", huMapping.OldHus);
                            var inventoryPackList = new List<Entity.INV.InventoryRePack>();
                            foreach (var huId in huMappingList.Select(h => h.HuId))
                            {
                                var inventoryUnPack = new Entity.INV.InventoryRePack();
                                inventoryUnPack.HuId = huId;
                                inventoryUnPack.Type = CodeMaster.RePackType.In;
                                inventoryPackList.Add(inventoryUnPack);
                            }
                            foreach (var huId in huMappingList.FirstOrDefault().OldHus.Split(new char[] { ';' }))
                            {
                                if (!string.IsNullOrEmpty(huId))
                                {
                                    var inventoryUnPack = new Entity.INV.InventoryRePack();
                                    inventoryUnPack.HuId = huId;
                                    inventoryUnPack.Type = CodeMaster.RePackType.Out;
                                    inventoryPackList.Add(inventoryUnPack);
                                }
                            }
                            foreach (var hum in huMappingList)
                            {
                                hum.IsEffective = true;
                                this.genericMgr.Update(hum);
                            }
                            locationDetailMgr.InventoryRePack(inventoryPackList, false, effDate.HasValue ? effDate.Value : DateTime.Now);

                            //else
                            //{
                            //    var inventoryPackList = new List<Entity.INV.InventoryRePack>();

                            //    var inventoryPacked = new Entity.INV.InventoryRePack();
                            //    inventoryPacked.HuId = huMapping.HuId;
                            //    inventoryPacked.Type = CodeMaster.RePackType.Out;
                            //    inventoryPackList.Add(inventoryPacked);

                            //    var inventoryUnPack = new Entity.INV.InventoryRePack();
                            //    inventoryUnPack.HuId = huMapping.OldHus;
                            //    inventoryUnPack.Type = CodeMaster.RePackType.In;
                            //    inventoryPackList.Add(inventoryUnPack);

                            //    locationDetailMgr.InventoryRePack(inventoryPackList);
                            //}
                        }

                        baseOrderDetailInput.ShipQty = orderDetailInput.ShipQty;
                        baseOrderDetailInput.LotNo = orderDetailInput.LotNo;

                        baseOrderDatail.OrderDetailInputs.Add(baseOrderDetailInput);
                    }
                }
                baseOrderDetailList.Add(baseOrderDatail);
            }
            if (effDate.HasValue)
            {
                this.orderMgr.ShipOrder(baseOrderDetailList, effDate.Value);
            }
            else
            {
                this.orderMgr.ShipOrder(baseOrderDetailList);
            }
        }

        /// <summary>
        /// 收货
        /// </summary>
        public void ReceiveWmsDatFile(IList<WMSDatFile> wmsDatFiles)
        {
            //IList<LesINLog> lesINLogList = new List<LesINLog>();
            string exMsg = string.Empty;
            //string updateSql = "update WMSDatFile set IsHand=1 where WMSId in(";
            //IList<string> updatePram = new List<string>();

            foreach (WMSDatFile wmsDatFile in wmsDatFiles)
            {
                //IList<Entity.ORD.OrderDetail> orderDetailList = new List<Entity.ORD.OrderDetail>();
                //LesINLog lesInLog = new LesINLog();
                try
                {
                    orderMgr.ReceiveWMSIpMaster(wmsDatFile);
                    //#region 可能已经成功处理，但安吉又发了数据文件
                    //LesINLog lesExistenceLog = genericMgr.FindAll<LesINLog>("select l from LesINLog as l where l.WMSNo=? ", wmsDatFile.WMSId).SingleOrDefault();
                    //if (lesExistenceLog != null && lesExistenceLog.HandResult == "S")
                    //{
                    //    lesExistenceLog.IsCreateDat = false;
                    //    genericMgr.Update(lesExistenceLog);
                    //    genericMgr.Update("update WMSDatFile set IsHand=1 where Id=" + wmsDatFile.Id);
                    //    continue;
                    //}
                    //#endregion

                    //#region 获得orderdetail
                    //Entity.ORD.OrderDetail orderDetail = genericMgr.FindById<Entity.ORD.OrderDetail>(Convert.ToInt32(wmsDatFile.WmsLine));
                    //orderDetail.WmsFileID = wmsDatFile.WMSId;
                    //orderDetail.ManufactureParty = wmsDatFile.LIFNR;
                    //orderDetail.ExternalOrderNo = wmsDatFile.WMSId;
                    //orderDetail.ExternalSequence = wmsDatFile.WBS;//项目代码
                    //Entity.ORD.OrderDetailInput orderDetailInput = new Entity.ORD.OrderDetailInput();
                    //orderDetailInput.ShipQty = wmsDatFile.Qty;
                    //orderDetailInput.WMSIpNo = wmsDatFile.WmsNo;//WMSNo
                    //orderDetailInput.WMSIpSeq = wmsDatFile.WMSId;//WMS行
                    //orderDetailInput.MoveType = wmsDatFile.MoveType + wmsDatFile.SOBKZ;//移动类型
                    //orderDetailInput.ManufactureParty = wmsDatFile.LIFNR;//厂商代码
                    //orderDetail.AddOrderDetailInput(orderDetailInput);
                    //orderDetailList.Add(orderDetail);
                    //#endregion

                    //#region 新建Log记录
                    //lesInLog.Type = "MB1B";
                    //lesInLog.MoveType = wmsDatFile.MoveType + wmsDatFile.SOBKZ;
                    //lesInLog.Sequense = "";
                    //lesInLog.WMSNo = wmsDatFile.WMSId;
                    //lesInLog.WMSLine = wmsDatFile.WmsLine;
                    //lesInLog.Item = wmsDatFile.Item;
                    //lesInLog.HandResult = "S";
                    //lesInLog.FileName = wmsDatFile.FileName;
                    //lesInLog.HandTime = System.DateTime.Now;//.ToString("yyMMddHHmmss");
                    //lesInLog.IsCreateDat = false;
                    //lesInLog.ASNNo = wmsDatFile.WmsNo;
                    //lesInLog.Qty = wmsDatFile.Qty;//扫描条码收货，数量不能更改
                    //lesInLog.QtyMark = true;//以后退货单也是用这类文件，true表示安吉需要根据这个log反馈来更新收货数，false表示不用更新（比如退货单）
                    //#endregion

                    //#region 拼成修改中间表Sql
                    //updateSql += "?,";
                    //updatePram.Add(wmsDatFile.WMSId.ToString());
                    //#endregion

                    //orderMgr.ShipOrder(orderDetailList);
                    //lesINLogList.Add(lesInLog);

                    //genericMgr.FlushSession();
                    //genericMgr.CleanSession();
                }
                catch (Exception ex)
                {
                    if (ex is BusinessException)
                        exMsg += "WmsId:" + wmsDatFile.WMSId + "收货失败！" + ((BusinessException)ex).GetMessages()[0].GetMessageString();
                    else
                        exMsg += "WmsId:" + wmsDatFile.WMSId + "收货失败！" + ex.Message;
                    //lesInLog.ErrorCause += "WmsId:" + wmsDatFile.WMSId + "收货失败！" + exMsg;
                }

            }
            //if (updatePram != null && updatePram.Count > 0)
            //{
            //    updateSql = updateSql.Substring(0, updateSql.Length - 1) + ")";
            //    genericMgr.Update(updateSql, updatePram.ToArray());
            //}

            #region Log
            //现在记录的一定是成功的日志
            //foreach (var lesInLog in lesINLogList)
            //{
            //    //#region 添加错误信息 显示到前台
            //    //if (lesInLog.ErrorCause != null && lesInLog.ErrorCause != string.Empty)
            //    //{
            //    //    businessException.AddMessage(lesInLog.ErrorCause);
            //    //}
            //    //#endregion

            //    //LesINLog lesExistenceLog = genericMgr.FindAll<LesINLog>("select l from LesINLog as l where l.WMSNo=? and l.HandResult = 'F'", lesInLog.WMSNo).SingleOrDefault();

            //    //#region 如果之前存在失败的记录则用本次处理结果更新原记录
            //    //if (lesExistenceLog != null)
            //    //{
            //    //    lesExistenceLog.ErrorCause = lesInLog.ErrorCause;
            //    //    lesExistenceLog.IsCreateDat = false;
            //    //    lesExistenceLog.HandResult = lesInLog.HandResult;
            //    //    lesExistenceLog.HandTime = lesInLog.HandTime;
            //    //    lesExistenceLog.FileName = lesInLog.FileName;
            //    //    lesExistenceLog.Qty = lesInLog.Qty;
            //    //    lesExistenceLog.QtyMark = lesInLog.QtyMark;
            //    //    genericMgr.Update(lesExistenceLog);
            //    //    continue;
            //    //}
            //    genericMgr.Create(lesInLog);
            //    //#endregion
            //}
            if (exMsg != string.Empty)
            {
                throw new BusinessException(exMsg);
            }
            #endregion
        }

        //[Transaction(TransactionMode.Requires)]
        //public IList<IpDetailInput> GetIpDetailInputByPickHu(string pickHu)
        //{
        //    IList<com.Sconit.Entity.ORD.IpLocationDetail> ipLocationDetail = genericMgr.FindAll<com.Sconit.Entity.ORD.IpLocationDetail>("select i from IpLocationDetail where i.WMSSeq = ?", pickHu);
        //    if (ipLocationDetail != null && ipLocationDetail.Count == 1)
        //    {
        //        com.Sconit.Entity.ORD.IpDetail ipDetail = genericMgr.FindById<com.Sconit.Entity.ORD.IpDetail>(ipLocationDetail[0].IpDetailId);

        //        var sdIpDetail = Mapper.Map<Entity.ORD.IpDetail, Entity.SD.ORD.IpDetail>(ipDetail);
        //    }
        //}

        public void DoReceiveOrder(List<Entity.SD.ORD.OrderDetailInput> orderDetailInputList, DateTime? effDate)
        {
            if (orderDetailInputList == null || orderDetailInputList.Count == 0)
            {
                throw new com.Sconit.Entity.Exception.BusinessException("没有要收货的明细");
            }
            IList<Entity.ORD.OrderDetail> orderDetailList = new List<Entity.ORD.OrderDetail>();
            var ids = orderDetailInputList.Select(o => o.Id).Distinct();

            foreach (var id in ids)
            {
                var baseOrderDatail = genericMgr.FindById<Entity.ORD.OrderDetail>(id);
                var selectedrderDetailInputList = orderDetailInputList.Where(o => o.Id == id);
                if (selectedrderDetailInputList != null)
                {
                    baseOrderDatail.OrderDetailInputs = new List<Entity.ORD.OrderDetailInput>();
                    foreach (var orderDetailInput in selectedrderDetailInputList)
                    {
                        Entity.ORD.OrderDetailInput baseOrderDetailInput = new Entity.ORD.OrderDetailInput();
                        baseOrderDetailInput.HuId = orderDetailInput.HuId;
                        baseOrderDetailInput.ReceiveQty = orderDetailInput.ReceiveQty;
                        baseOrderDetailInput.LotNo = orderDetailInput.LotNo;
                        baseOrderDetailInput.Bin = orderDetailInput.Bin;

                        baseOrderDatail.OrderDetailInputs.Add(baseOrderDetailInput);
                    }
                }
                orderDetailList.Add(baseOrderDatail);
            }
            if (effDate.HasValue)
            {
                this.orderMgr.ReceiveOrder(orderDetailList, effDate.Value);
            }
            else
            {
                this.orderMgr.ReceiveOrder(orderDetailList);
            }
        }

        /// <summary>
        /// 收货
        /// </summary>
        [Transaction(TransactionMode.Requires)]
        public void DoReceiveIp(List<Entity.SD.ORD.IpDetailInput> ipDetailInputList, DateTime? effDate)
        {
            if (ipDetailInputList == null || ipDetailInputList.Count() == 0)
            {
                throw new com.Sconit.Entity.Exception.BusinessException("没有要收货的明细");
            }
            IList<Entity.ORD.IpDetail> baseIpDetailList = new List<Entity.ORD.IpDetail>();

            var detIds = ipDetailInputList.Select(i => i.Id).Distinct();

            foreach (var id in detIds)
            {
                var ipDatail = genericMgr.FindById<Entity.ORD.IpDetail>(id);
                var q_1 = ipDetailInputList.Where(o => o.Id == id);
                if (q_1 != null)
                {
                    ipDatail.IpDetailInputs = new List<Entity.ORD.IpDetailInput>();
                    foreach (var odi in q_1)
                    {
                        var baseIpDetailInput = new Entity.ORD.IpDetailInput();
                        baseIpDetailInput.HuId = odi.HuId;
                        baseIpDetailInput.ReceiveQty = odi.ReceiveQty;
                        baseIpDetailInput.ShipQty = odi.ShipQty;
                        baseIpDetailInput.LotNo = odi.LotNo;
                        baseIpDetailInput.Bin = odi.Bin;

                        ipDatail.IpDetailInputs.Add(baseIpDetailInput);
                    }
                }
                baseIpDetailList.Add(ipDatail);
            }

            //可能这些ipDetail分属于多个ipMstr，这里要分开调用
            IList<string> ipNoList = (from det in baseIpDetailList select det.IpNo).Distinct().ToList();
            foreach (string ipNo in ipNoList)
            {
                var recIpDetail = (from ipdetail in baseIpDetailList
                                   where ipdetail.IpNo == ipNo
                                   select ipdetail).ToList();
                if (effDate.HasValue)
                {
                    this.orderMgr.ReceiveIp(recIpDetail, effDate.Value);
                }
                else
                {
                    this.orderMgr.ReceiveIp(recIpDetail);
                }
            }
        }

        [Transaction(TransactionMode.Requires)]
        public void DoReceiveKit(string kitNo, DateTime? effDate)
        {
            var baseIpDetails = this.genericMgr.FindAll<Entity.ORD.IpDetail>(" from IpDetail i where i.OrderNo = ? ", kitNo);
            foreach (var baseIpDetail in baseIpDetails)
            {
                baseIpDetail.IpLocationDetails =
                    this.genericMgr.FindAll<Entity.ORD.IpLocationDetail>(" from IpLocationDetail i where i.IpDetailI d= ? ", baseIpDetail.Id);

                foreach (var ipLocationDetail in baseIpDetail.IpLocationDetails)
                {
                    var baseIpDetailInput = new Entity.ORD.IpDetailInput();
                    baseIpDetailInput.HuId = ipLocationDetail.HuId;
                    baseIpDetailInput.ReceiveQty = ipLocationDetail.Qty / baseIpDetail.UnitQty;
                    baseIpDetailInput.LotNo = ipLocationDetail.LotNo;

                    baseIpDetail.IpDetailInputs.Add(baseIpDetailInput);
                }
            }

            effDate = effDate.HasValue ? effDate.Value : DateTime.Now;

            this.orderMgr.ReceiveIp(baseIpDetails, effDate.Value);
        }

        /// <summary>
        /// 移库
        /// </summary>List<Entity.SD.ORD.OrderDetailInput> orderDetailInputList
        [Transaction(TransactionMode.Requires)]
        public void DoTransfer(Entity.SD.SCM.FlowMaster flowMaster, List<Entity.SD.ORD.OrderDetailInput> orderDetailInputList)
        {
            var orderMaster = new Entity.ORD.OrderMaster();

            var locationFrom = this.genericMgr.FindById<Entity.MD.Location>(flowMaster.LocationFrom);
            var locationTo = this.genericMgr.FindById<Entity.MD.Location>(flowMaster.LocationTo);
            var partyFrom = this.genericMgr.FindById<Entity.MD.Party>(flowMaster.PartyFrom);
            var partyTo = this.genericMgr.FindById<Entity.MD.Party>(flowMaster.PartyTo);

            orderMaster.LocationFrom = locationFrom.Code;
            orderMaster.IsShipScanHu = (flowMaster != null ? flowMaster.IsShipScanHu : true);
            orderMaster.IsReceiveScanHu = (flowMaster != null ? flowMaster.IsReceiveScanHu : true);
            orderMaster.IsAutoReceive = true;
            orderMaster.LocationFromName = locationFrom.Name;
            orderMaster.LocationTo = locationTo.Code;
            orderMaster.LocationToName = locationTo.Name;
            orderMaster.PartyFrom = partyFrom.Code;
            orderMaster.PartyFromName = partyFrom.Name;
            orderMaster.PartyTo = partyTo.Code;
            orderMaster.PartyToName = partyTo.Name;
            orderMaster.Type = CodeMaster.OrderType.Transfer;
            orderMaster.StartTime = DateTime.Now;
            orderMaster.WindowTime = DateTime.Now;
            orderMaster.EffectiveDate = flowMaster.EffectiveDate;
            orderMaster.Flow = flowMaster != null ? flowMaster.Code : null;

            orderMaster.IsQuick = true;
            orderMaster.OrderDetails = new List<Entity.ORD.OrderDetail>();
            int seq = 1;

            var ids = orderDetailInputList.Select(o => o.Id).Distinct();

            foreach (var id in ids)
            {
                var selectedOrderDetailInputList = orderDetailInputList.Where(o => o.Id == id);

                if (selectedOrderDetailInputList != null && selectedOrderDetailInputList.Count() > 0)
                {
                    var firstInput = selectedOrderDetailInputList.First();

                    var hu = this.genericMgr.FindById<Entity.INV.Hu>(firstInput.HuId);
                    var item = this.genericMgr.FindById<Entity.MD.Item>(hu.Item);

                    var baseOrderDetail = new Entity.ORD.OrderDetail();
                    baseOrderDetail.BaseUom = item.Uom;
                    baseOrderDetail.Item = item.Code;
                    baseOrderDetail.ItemDescription = item.Description;
                    baseOrderDetail.OrderType = flowMaster.Type;
                    baseOrderDetail.QualityType = orderMaster.QualityType;
                    baseOrderDetail.Sequence = seq++;
                    baseOrderDetail.UnitCount = item.UnitCount;
                    baseOrderDetail.Uom = hu.Uom;
                    baseOrderDetail.OrderDetailInputs = new List<Entity.ORD.OrderDetailInput>();

                    foreach (Entity.SD.ORD.OrderDetailInput orderDetailInput in selectedOrderDetailInputList)
                    {
                        var baseOrderDetailInput = new Entity.ORD.OrderDetailInput();
                        //支持新的条码逻辑
                        if (flowMaster.IsShipScanHu)
                            baseOrderDetailInput.HuId = orderDetailInput.HuId;
                        baseOrderDetailInput.ReceiveQty = orderDetailInput.Qty;
                        baseOrderDetailInput.Bin = orderDetailInput.Bin;
                        baseOrderDetailInput.LotNo = orderDetailInput.LotNo;
                        baseOrderDetail.OrderDetailInputs.Add(baseOrderDetailInput);
                        baseOrderDetail.RequiredQty += orderDetailInput.Qty;
                        baseOrderDetail.OrderedQty += orderDetailInput.Qty;
                    }
                    orderMaster.OrderDetails.Add(baseOrderDetail);
                }
            }

            this.orderMgr.CreateOrder(orderMaster);

            //todo上架
            if (!string.IsNullOrWhiteSpace(flowMaster.Bin))
            {
                //
            }
        }

        //[Transaction(TransactionMode.Requires)]
        public void DoAnDon(List<AnDonInput> anDonInputList,User scanUser)
        {
            if (anDonInputList != null && anDonInputList.Count > 0)
            {
                List<string> flowCodeList = anDonInputList.Select(a => a.Flow).Distinct().ToList();

                #region 以前的代码
                //foreach (var flowCode in flowCodeList)
                //{
                //    //string flowCode = anDonInputList.Select(a => a.Flow).Distinct().Single();

                //    Entity.SCM.FlowMaster flowMaster = this.genericMgr.FindById<Entity.SCM.FlowMaster>(flowCode);
                //    Entity.ORD.OrderMaster orderMaster = orderMgr.TransferFlow2Order(flowMaster, anDonInputList.Select(a => a.Item).Distinct().ToList());
                //    orderMaster.StartTime = DateTime.Now;

                //    FlowStrategy flowStrategy = genericMgr.FindById<FlowStrategy>(flowCode);
                //    orderMaster.WindowTime = orderMaster.StartTime.AddHours((double)flowStrategy.LeadTime);
                //    orderMaster.IsAutoRelease = true;

                //    foreach (AnDonInput anDonInput in anDonInputList)
                //    {
                //        Entity.ORD.OrderDetail orderDetail = orderMaster.OrderDetails != null ?
                //            orderMaster.OrderDetails.Where(f => f.BinTo == anDonInput.OpRef
                //            && f.Item == anDonInput.Item).FirstOrDefault() : null;

                //        if (orderDetail != null)
                //        {
                //            orderDetail.RequiredQty += anDonInput.UnitCount;
                //            orderDetail.OrderedQty += anDonInput.UnitCount;
                //        }
                //        else
                //        {
                //            if (anDonInput.Flow == orderMaster.Flow)
                //            {
                //                orderDetail = new Entity.ORD.OrderDetail();
                //                orderDetail.OrderType = orderMaster.Type;
                //                orderDetail.OrderSubType = orderMaster.SubType;
                //                //orderDetail.Sequence  = ;
                //                Entity.MD.Item item = this.genericMgr.FindById<Entity.MD.Item>(anDonInput.Item);
                //                orderDetail.Item = item.Code;
                //                orderDetail.ItemDescription = item.Description;
                //                orderDetail.ReferenceItemCode = item.ReferenceCode;
                //                orderDetail.BaseUom = item.Uom;
                //                orderDetail.Uom = anDonInput.Uom;
                //                //orderDetail.PartyFrom=
                //                orderDetail.UnitCount = anDonInput.UnitCount;
                //                //orderDetail.UnitCountDescription=
                //                //orderDetail.MinUnitCount=
                //                orderDetail.QualityType = CodeMaster.QualityType.Qualified;
                //                orderDetail.ManufactureParty = anDonInput.Supplier;
                //                orderDetail.RequiredQty += anDonInput.UnitCount;
                //                orderDetail.OrderedQty += anDonInput.UnitCount;
                //                //orderDetail.ShippedQty = ;
                //                //orderDetail.ReceivedQty = ;
                //                //orderDetail.RejectedQty = ;
                //                //orderDetail.ScrapQty = ;
                //                //orderDetail.PickedQty = ;
                //                if (orderDetail.BaseUom != orderDetail.Uom)
                //                {
                //                    orderDetail.UnitQty = this.itemMgr.ConvertItemUomQty(orderDetail.Item, orderDetail.BaseUom, 1, orderDetail.Uom);
                //                }
                //                else
                //                {
                //                    orderDetail.UnitQty = 1;
                //                }
                //                //public string LocationFrom { get; set; }
                //                //public string LocationFromName { get; set; }
                //                Entity.MD.Location locationTo = this.genericMgr.FindById<Entity.MD.Location>(anDonInput.LocationTo);
                //                orderDetail.LocationTo = locationTo.Code;
                //                orderDetail.LocationToName = locationTo.Name;
                //                orderDetail.IsInspect = false;
                //                //public string Container { get; set; }
                //                //public string ContainerDescription { get; set; }
                //                //public Boolean IsScanHu { get; set; }
                //                orderDetail.BinTo = anDonInput.OpRef;

                //                orderMaster.AddOrderDetail(orderDetail);
                //            }
                //        }
                //    }

                //    this.orderMgr.CreateOrder(orderMaster);
                //}
                #endregion
                #region 新的看板代码
                string hqlFlowMaster = string.Empty;
                string hqlFlowStra = string.Empty;

                List<object> para = new List<object>();
                foreach (var flowCode in flowCodeList)
                {
                    if (string.IsNullOrEmpty(hqlFlowMaster))
                    {
                        hqlFlowMaster = "from FlowMaster fm where fm.Code in(?";
                        hqlFlowStra = "from FlowStrategy fs where fs.Flow in(?";
                    }
                    else
                    {
                        hqlFlowMaster += ",?";
                        hqlFlowStra += ",?";
                    }
                    para.Add(flowCode);
                }
                hqlFlowMaster += ")";
                hqlFlowStra += ")";

                IList<FlowMaster> flowList = this.genericMgr.FindAll<FlowMaster>(hqlFlowMaster, para.ToArray());
                IList<FlowStrategy> flowStrategyList = this.genericMgr.FindAll<FlowStrategy>(hqlFlowStra, para.ToArray());

                List<Entity.KB.KanbanScan> kbScanOrderNowList = new List<Entity.KB.KanbanScan>();
                List<Entity.KB.KanbanScan> kbScanList = new List<Entity.KB.KanbanScan>();
                foreach (var anDonInput in anDonInputList)
                {
                    var scan = kanbanScanMgr.Scan(anDonInput.CardNo, scanUser, true);
                    var flowStrategy = flowStrategyList.FirstOrDefault(f => f.Flow == anDonInput.Flow);
                    if (!string.IsNullOrEmpty(scan.CardNo))
                    {
                        if (flowStrategy.IsOrderNow)
                        {
                            kbScanOrderNowList.Add(scan);
                        }
                        else
                        {
                            kbScanList.Add(scan);
                        }
                    }
                }
                if (kbScanOrderNowList.Count > 0)
                {
                    kanbanScanOrderMgr.OrderCard(kbScanOrderNowList, flowList, DateTime.Now);
                }
                #endregion
            }
            else
            {
                throw new BusinessException("请刷入看板卡。");
            }
        }

        [Transaction(TransactionMode.Requires)]
        public AnDonInput GetKanBanCard(string cardNo)
        {
            AnDonInput anDonInput = new AnDonInput();
            Entity.KB.KanbanCard kanbanCard = this.genericMgr.FindById<Entity.KB.KanbanCard>(cardNo);

            IList<FlowDetail> flowDetails = this.genericMgr.FindAll<FlowDetail>("from FlowDetail fd where fd.Id=?", kanbanCard.FlowDetailId);
            if (flowDetails == null || flowDetails.Count == 0)
            {
                throw new BusinessException("看板路线明细不存在不能刷入");
            }

            Entity.KB.KanbanScan banbanScan = this.genericMgr.FindAll<Entity.KB.KanbanScan>("from KanbanScan ks where ks.CardNo=? and ks.IsOrdered=?", new object[] { cardNo, false }).FirstOrDefault();
            if (banbanScan != null)
            {
                throw new BusinessException("看板卡已经刷入，没有生成订单，请不要重读刷入");
            }

            //Entity.INV.KanBanCard kanBanCard = this.genericMgr.FindById<Entity.INV.KanBanCard>(kanBanCardInfo.KBICode);
            anDonInput = Mapper.Map<Entity.KB.KanbanCard, AnDonInput>(kanbanCard);
            //anDonInput.CardNo = kanBanCardInfo.CardNo;
            return anDonInput;
        }

        [Transaction(TransactionMode.Requires)]
        public void StartPickList(string pickListNo)
        {
            this.pickListMgr.StartPickList(pickListNo);
            //var basePickListMaster = genericMgr.FindById<Entity.ORD.PickListMaster>(pickListNo);
            //var pickListMaster = Mapper.Map<Entity.ORD.PickListMaster, Entity.SD.ORD.PickListMaster>(basePickListMaster);

            //return pickListMaster;
        }

        [Transaction(TransactionMode.Requires)]
        public void ShipPickList(string pickListNo)
        {
            this.orderMgr.ShipPickList(pickListNo);
        }

        /// <summary>
        /// 拣货
        /// </summary>
        /// <param name="orderDetails">由多个<PickListDetaiId,HuId>组成的数组</param>
        /// <param name="userCode"></param>
        [Transaction(TransactionMode.Requires)]
        public void DoPickList(List<Entity.SD.ORD.PickListDetailInput> pickListDetailInputList)
        {
            IList<Entity.ORD.PickListDetail> basePickListDetailList = new List<Entity.ORD.PickListDetail>();
            //todo
            var ids = pickListDetailInputList.Select(p => p.Id).Distinct();

            foreach (var id in ids)
            {
                var basePickListDatail = this.genericMgr.FindById<Entity.ORD.PickListDetail>(id);
                var selectedpickListDetailInputList = pickListDetailInputList.Where(o => o.Id == id);
                if (selectedpickListDetailInputList != null)
                {
                    basePickListDatail.PickListDetailInputs = new List<Entity.ORD.PickListDetailInput>();
                    foreach (var pickListDetailInput in selectedpickListDetailInputList)
                    {
                        Entity.ORD.PickListDetailInput basePickListDetailInput = new Entity.ORD.PickListDetailInput();
                        basePickListDetailInput.HuId = pickListDetailInput.HuId;

                        basePickListDatail.PickListDetailInputs.Add(basePickListDetailInput);
                    }
                }
                basePickListDetailList.Add(basePickListDatail);
            }
            this.pickListMgr.DoPick(basePickListDetailList);
        }

        [Transaction(TransactionMode.Requires)]
        public Entity.SD.ORD.OrderBomDetail DoMaterialIn(string orderNo, Entity.SD.INV.Hu hu)
        {
            //todo
            throw new Exception();
        }

        [Transaction(TransactionMode.Requires)]
        public void DoInspect(List<string> huIdList, DateTime? effDate)
        {
            if (huIdList == null || huIdList.Count == 0)
            {
                throw new Entity.Exception.BusinessException("没有要检验的明细");
            }

            var inspectMaster = new Entity.INP.InspectMaster();
            inspectMaster.Status = CodeMaster.InspectStatus.Submit;
            inspectMaster.Type = CodeMaster.InspectType.Barcode;
            var inspectDetailList = new List<Entity.INP.InspectDetail>();
            foreach (var huId in huIdList)
            {
                var inspectDetail = new Entity.INP.InspectDetail();
                inspectDetail.HuId = huId;
                //var hu = this.genericMgr.FindById<Entity.INV.Hu>(huId);
                //var item = this.genericMgr.FindById<Entity.MD.Item>(hu.Item);
                //inspectDetail.BaseUom = hu.BaseUom;
                ////inspectDetail.InspectNo = 
                //inspectDetail.InspectQty = hu.Qty;
                //inspectDetail.Item = hu.Item;
                //inspectDetail.ItemDescription = item.Description;
                ////inspectDetail.LocationFrom = hu.
                //inspectDetail.LotNo = hu.LotNo;
                //inspectDetail.ReferenceItemCode = hu.ReferenceItemCode;
                //inspectDetail.UnitCount = hu.UnitCount;
                //inspectDetail.UnitQty = hu.UnitQty;
                //inspectDetail.Uom = hu.Uom;
                inspectDetailList.Add(inspectDetail);
            }
            inspectMaster.InspectDetails = inspectDetailList;
            inspectMgr.CreateInspectMaster(inspectMaster, effDate.HasValue ? effDate.Value : DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public void DoWorkersWaste(List<string> huIdList, DateTime? effDate)
        {
            if (huIdList == null || huIdList.Count == 0)
            {
                throw new Entity.Exception.BusinessException("没有要检验的明细");
            }

            var inspectMaster = new Entity.INP.InspectMaster();
            inspectMaster.Status = CodeMaster.InspectStatus.Submit;
            inspectMaster.Type = CodeMaster.InspectType.Barcode;
            var inspectDetailList = new List<Entity.INP.InspectDetail>();
            foreach (var huId in huIdList)
            {
                var inspectDetail = new Entity.INP.InspectDetail();
                inspectDetail.HuId = huId;
                inspectDetailList.Add(inspectDetail);
            }
            inspectMaster.InspectDetails = inspectDetailList;
            inspectMgr.CreateWorkersWaste(inspectMaster, effDate.HasValue ? effDate.Value : DateTime.Now);
        }


        [Transaction(TransactionMode.Requires)]
        public void DoJudgeInspect(Entity.SD.ORD.InspectMaster inspectMaster, List<string> HuIdList, DateTime? effDate)
        {

            var baseInspectMaster = this.GetInspectMaster(inspectMaster.InspectNo, true, false);
            if (baseInspectMaster.InspectDetails == null || baseInspectMaster.InspectDetails.Count == 0)
            {
                throw new Entity.Exception.BusinessException("没有检验明细");
            }

            if (HuIdList == null || HuIdList.Count == 0)
            {
                throw new Entity.Exception.BusinessException("没有判定明细");
            }

            foreach (var baseInspectDetail in baseInspectMaster.InspectDetails)
            {
                foreach (var huId in HuIdList)
                {
                    if (huId.Equals(baseInspectDetail.HuId, StringComparison.OrdinalIgnoreCase))
                    {
                        //baseInspectDetail.CurrentInspectQty = baseInspectDetail.InspectQty;
                        baseInspectDetail.JudgeResult = inspectMaster.JudgeResult;
                        if (baseInspectDetail.JudgeResult == CodeMaster.JudgeResult.Qualified)
                        {
                            baseInspectDetail.CurrentQualifyQty = baseInspectDetail.InspectQty;
                        }
                        else
                        {
                            baseInspectDetail.CurrentRejectQty = baseInspectDetail.InspectQty;
                        }
                    }
                }
            }
            effDate = effDate.HasValue ? effDate.Value : DateTime.Now;
            if (effDate.HasValue)
            {
                this.inspectMgr.JudgeInspectDetail(baseInspectMaster.InspectDetails, effDate.Value);
            }
        }

        [Transaction(TransactionMode.Requires)]
        public List<string> GetProdLineStation(string orderNo, string huId)
        {
            return null;
        }

        #region verify
        [Transaction(TransactionMode.Requires)]
        public Boolean VerifyOrderCompareToHu(string orderNo, string huId)
        {
            Entity.INV.Hu hu = genericMgr.FindById<Entity.INV.Hu>(huId);

            long counter = this.genericMgr.FindAll<long>("select count(*) as counter from OrderBomDetail where OrderNo = ? and Item = ?", new object[] { orderNo, hu.Item }).Single();

            if (counter > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region 投料
        //投料到生产线
        [Transaction(TransactionMode.Requires)]
        public void FeedProdLineRawMaterial(string productLine, string productLineFacility, string location, List<com.Sconit.Entity.SD.INV.Hu> hus, bool isForceFeed, DateTime? effectiveDate)
        {
            if (hus == null || hus.Count == 0)
            {
                throw new Entity.Exception.BusinessException("投料的条码明细不能为空");
            }

            var feedInputs = new List<Entity.PRD.FeedInput>();
            foreach (var hu in hus)
            {
                if (hu.IsEffective == false)
                {
                    TryPackBarcode(hu.HuId, location);
                }
                var feedInput = new Entity.PRD.FeedInput();

                feedInput.HuId = hu.HuId;
                feedInput.LotNo = hu.LotNo;
                feedInput.Qty = hu.Qty;
                feedInputs.Add(feedInput);
            }
            effectiveDate = effectiveDate.HasValue ? effectiveDate.Value : DateTime.Now;

            this.genericMgr.FlushSession();
            this.productionLineMgr.FeedRawMaterial(productLine, productLineFacility, feedInputs, isForceFeed, effectiveDate.Value);
        }

        #region 关键件投料到生产单
        [Transaction(TransactionMode.Requires)]
        public void FeedOrderRawMaterial(string orderNo, string location, List<com.Sconit.Entity.SD.INV.Hu> hus, bool isForceFeed, DateTime? effectiveDate)
        {
            if (hus == null || hus.Count == 0)
            {
                throw new Entity.Exception.BusinessException("投料的条码明细不能为空");
            }

            var feedInputs = new List<Entity.PRD.FeedInput>();
            foreach (var hu in hus)
            {
                if (hu.IsEffective == false)
                {
                    #region 校验投料的库位是否Bom物料的来源库位
                    if (!isForceFeed)
                    {
                        IList<string> orderBomLoationList = this.genericMgr.FindAll<string>("select distinct Location from OrderBomDetail where OrderNo = ? and Item = ? and IsScanHu = ?",
                            new object[] { orderNo, hu.Item, true });
                        if (orderBomLoationList == null || orderBomLoationList.Count == 0 || !orderBomLoationList.Contains(location))
                        {
                            string bomLocations = string.Empty;
                            foreach (string bomLocation in orderBomLoationList)
                            {
                                if (orderBomLoationList.IndexOf(bomLocation) == 0)
                                {
                                    bomLocations += bomLocation;
                                }
                                else
                                {
                                    bomLocations += ", " + bomLocation;
                                }
                            }
                            if (bomLocations != string.Empty)
                            {
                                throw new BusinessException("关键件{0}不在库位{1}上投料，可投料的库位有{2}", hu.Item, location, bomLocations);
                            }
                            else
                            {
                                throw new BusinessException("物料{1}不是生产单{0}的关键件。", orderNo, hu.Item);
                            }
                        }
                    }
                    #endregion

                    TryPackBarcode(hu.HuId, location);
                }
                var feedInput = new Entity.PRD.FeedInput();
                feedInput.HuId = hu.HuId;
                feedInput.LotNo = hu.LotNo;
                feedInput.Qty = hu.Qty;
                feedInputs.Add(feedInput);
            }
            effectiveDate = effectiveDate.HasValue ? effectiveDate.Value : DateTime.Now;
            this.genericMgr.FlushSession();
            this.productionLineMgr.FeedRawMaterial(orderNo, feedInputs, isForceFeed, effectiveDate.Value);
        }
        #endregion

        #region 生产单投料，投Kit单料
        [Transaction(TransactionMode.Requires)]
        public void FeedKitOrder(string orderNo, string kitOrderNo, bool isForceFeed, DateTime? effectiveDate)
        {
            effectiveDate = effectiveDate.HasValue ? effectiveDate.Value : DateTime.Now;
            this.productionLineMgr.FeedKitOrder(orderNo, kitOrderNo, isForceFeed, effectiveDate.Value);
        }
        #endregion

        #region 生产单投料，投工单
        [Transaction(TransactionMode.Requires)]
        public void FeedProductOrder(string orderNo, string productOrderNo, bool isForceFeed, DateTime? effectiveDate)
        {
            effectiveDate = effectiveDate.HasValue ? effectiveDate.Value : DateTime.Now;
            this.productionLineMgr.FeedProductOrder(orderNo, productOrderNo, isForceFeed, effectiveDate.Value);
        }
        #endregion

        #endregion

        public void ReturnOrderRawMaterial(string orderNo, string traceCode, int? operation, string opReference, string[][] huDetails, DateTime? effectiveDate)
        {
            if (huDetails == null)
            {
                throw new Entity.Exception.BusinessException("投料的条码明细不能为空");
            }

            var returnInputs = new List<Entity.PRD.ReturnInput>();
            foreach (var huDetail in huDetails)
            {
                var returnInput = new Entity.PRD.ReturnInput();
                returnInput.HuId = huDetail[0];
                returnInput.Qty = decimal.Parse(huDetail[1]);
                returnInputs.Add(returnInput);
            }
            effectiveDate = effectiveDate.HasValue ? effectiveDate.Value : DateTime.Now;
            this.productionLineMgr.ReturnRawMaterial(orderNo, traceCode, operation, opReference, returnInputs, effectiveDate.Value);
        }

        public void ReturnProdLineRawMaterial(string productLine, string productLineFacility, string[][] huDetails, DateTime? effectiveDate)
        {
            if (huDetails == null)
            {
                throw new Entity.Exception.BusinessException("投料的条码明细不能为空");
            }

            var returnInputs = new List<Entity.PRD.ReturnInput>();
            foreach (var huDetail in huDetails)
            {
                var returnInput = new Entity.PRD.ReturnInput();
                returnInput.HuId = huDetail[0];
                returnInput.Qty = decimal.Parse(huDetail[1]);
                returnInputs.Add(returnInput);
            }
            effectiveDate = effectiveDate.HasValue ? effectiveDate.Value : DateTime.Now;
            this.productionLineMgr.ReturnRawMaterial(productLine, productLineFacility, returnInputs, effectiveDate.Value);
        }

        [Transaction(TransactionMode.Requires)]
        public void DoKitOrderScanKeyPart(string[][] huDetails, string orderNo)
        {
            var receiptDetails = this.genericMgr.FindAll<com.Sconit.Entity.ORD.ReceiptDetail>("select rd from ReceiptDetail rd where rd.OrderNo=?", orderNo);

            var inventoryPackList = new List<Entity.INV.InventoryPack>();
            foreach (var hu in huDetails)
            {
                if (receiptDetails.All(rd => rd.Item != hu[1]))
                {
                    throw new BusinessException("分装生产单{0}上的物料{1}没有收货.", receiptDetails[0].ReceiptNo, hu[1]);
                }
                var receiptDetail = receiptDetails.FirstOrDefault(rd => rd.Item == hu[1]);
                var receiptLocationDet = this.genericMgr.FindAll<com.Sconit.Entity.ORD.ReceiptLocationDetail>("select rld from ReceiptLocationDetail rld where rld.ReceiptDetailId=?", receiptDetail.Id).FirstOrDefault(r => r.ReceiptDetailId == receiptDetail.Id);
                if (!string.IsNullOrEmpty(receiptLocationDet.HuId))
                {
                    throw new BusinessException("分装生产单{0}上的物料{1}已扫描.", receiptDetails[0].ReceiptNo, hu[1]);
                }
                receiptLocationDet.HuId = hu[0];
                this.genericMgr.Update(receiptLocationDet);
                var inventoryPack = new Entity.INV.InventoryPack();
                inventoryPack.HuId = hu[0];
                inventoryPack.Location = receiptDetails[0].LocationTo;
                inventoryPackList.Add(inventoryPack);
            }
            locationDetailMgr.InventoryPack(inventoryPackList);
        }

        #region 快速退货
        [Transaction(TransactionMode.Requires)]
        //生产线退库，把数量变为条码移库
        public void DoReturnOrder(string flowCode, List<string> huIdList, DateTime? effectiveDate)
        {
            if (huIdList == null || huIdList.Count == 0)
            {
                throw new BusinessException("退库条码不能为空。");
            }
            IList<com.Sconit.Entity.VIEW.HuStatus> huStatusList = this.huMgr.GetHuStatus(huIdList);

            FlowMaster flowMaster = this.genericMgr.FindById<FlowMaster>(flowCode);
            FlowMaster returnflowMaster = this.flowMgr.GetReverseFlow(flowMaster, huStatusList.Select(h => h.Item).Distinct().ToList());
            com.Sconit.Entity.ORD.OrderMaster orderMaster = this.orderMgr.TransferFlow2Order(returnflowMaster, null);

            orderMaster.StartTime = DateTime.Now;
            orderMaster.WindowTime = DateTime.Now;
            orderMaster.EffectiveDate = effectiveDate.HasValue ? effectiveDate : DateTime.Now;
            orderMaster.IsQuick = true;
            //orderMaster.IsAutoRelease = true;
            //orderMaster.IsAutoShip = false;
            //orderMaster.IsAutoReceive = true;
            orderMaster.IsShipScanHu = true;
            orderMaster.IsReceiveScanHu = true;

            IList<InventoryPack> inventoryPackList = new List<InventoryPack>();
            BusinessException businessException = new BusinessException();
            foreach (com.Sconit.Entity.VIEW.HuStatus huStatus in huStatusList)
            {
                if (huStatus.Status == CodeMaster.HuStatus.Ip)
                {
                    businessException.AddMessage("条码{0}为库位{1}至库位{2}的在途库存，不能退库。", huStatus.HuId, huStatus.LocationFrom, huStatus.LocationTo);
                }
                else if (huStatus.Status == CodeMaster.HuStatus.Location)
                {
                    businessException.AddMessage("条码{0}已经在库位{1}中，不能装箱。", huStatus.HuId, huStatus.Location);
                }
                else
                {
                    InventoryPack inventoryPack = new InventoryPack();
                    inventoryPack.Location = returnflowMaster.LocationFrom;
                    inventoryPack.HuId = huStatus.HuId;
                    inventoryPack.OccupyType = CodeMaster.OccupyType.None;
                    inventoryPack.OccupyReferenceNo = null;

                    inventoryPackList.Add(inventoryPack);
                }
            }

            if (businessException.HasMessage)
            {
                throw businessException;
            }

            //先装箱
            this.locationDetailMgr.InventoryPack(inventoryPackList);

            var groupedHuList = from hu in huStatusList
                                group hu by new
                                {
                                    Item = hu.Item,
                                    ItemDescription = hu.ItemDescription,
                                    ReferenceItemCode = hu.ReferenceItemCode,
                                    Uom = hu.Uom,
                                    BaseUom = hu.BaseUom,
                                    UnitQty = hu.UnitQty,
                                    UnitCount = hu.UnitCount
                                } into gj
                                select new
                                {
                                    Item = gj.Key.Item,
                                    ItemDescription = gj.Key.ItemDescription,
                                    ReferenceItemCode = gj.Key.ReferenceItemCode,
                                    Uom = gj.Key.Uom,
                                    BaseUom = gj.Key.BaseUom,
                                    UnitQty = gj.Key.UnitQty,
                                    UnitCount = gj.Key.UnitCount,
                                    Qty = gj.Sum(hu => hu.Qty),
                                    List = gj.ToList()
                                };

            foreach (var groupedHu in groupedHuList)
            {
                Entity.ORD.OrderDetail orderDetail = new Entity.ORD.OrderDetail();
                orderDetail.OrderNo = orderMaster.OrderNo;
                orderDetail.OrderType = orderMaster.Type;
                orderDetail.OrderSubType = orderMaster.SubType;
                orderDetail.Item = groupedHu.Item;
                orderDetail.ItemDescription = groupedHu.ItemDescription;
                orderDetail.ReferenceItemCode = groupedHu.ReferenceItemCode;
                orderDetail.Uom = groupedHu.Uom;
                orderDetail.BaseUom = groupedHu.BaseUom;
                orderDetail.UnitQty = groupedHu.UnitQty;
                orderDetail.UnitCount = groupedHu.UnitCount;
                orderDetail.QualityType = CodeMaster.QualityType.Qualified;
                orderDetail.RequiredQty = groupedHu.Qty;
                orderDetail.OrderedQty = groupedHu.Qty;

                orderMaster.AddOrderDetail(orderDetail);

                foreach (com.Sconit.Entity.VIEW.HuStatus huStatus in groupedHu.List)
                {
                    Entity.ORD.OrderDetailInput orderDetailInput = new Entity.ORD.OrderDetailInput();
                    orderDetailInput.HuId = huStatus.HuId;
                    orderDetailInput.ReceiveQty = huStatus.Qty;
                    orderDetailInput.LotNo = huStatus.LotNo;

                    orderDetail.AddOrderDetailInput(orderDetailInput);
                }
            }

            this.orderMgr.CreateOrder(orderMaster);
        }
        #endregion

        private com.Sconit.Entity.INP.InspectMaster GetInspectMaster(string inspectNo, bool includeDetail, bool includeJudge)
        {
            var inspectMaster = genericMgr.FindById<com.Sconit.Entity.INP.InspectMaster>(inspectNo);
            if (includeDetail)
            {
                inspectMaster.InspectDetails = this.genericMgr.FindAll<com.Sconit.Entity.INP.InspectDetail>("from InspectDetail i where i.IsJudge = ? and  i.InspectNo= ?  ", new object[] { includeJudge, inspectNo });
            }
            return inspectMaster;
        }

        private void TryPackBarcode(string huId, string location)
        {
            HuStatus huStatus = this.huMgr.GetHuStatus(huId);

            if (string.IsNullOrWhiteSpace(huStatus.Location) && string.IsNullOrWhiteSpace(huStatus.LocationTo))
            {
                #region 装箱
                InventoryPack inventoryPack = new InventoryPack();

                inventoryPack.Location = location;
                inventoryPack.HuId = huId;
                inventoryPack.OccupyType = CodeMaster.OccupyType.None;
                inventoryPack.OccupyReferenceNo = null;

                IList<InventoryPack> inventoryPackList = new List<InventoryPack>();
                inventoryPackList.Add(inventoryPack);
                this.locationDetailMgr.InventoryPack(inventoryPackList);
                #endregion

                this.genericMgr.FlushSession();
            }
        }

        #region 整车生产单上线
        [Transaction(TransactionMode.Requires)]
        public void StartVanOrder(string orderNo)
        {
            this.orderMgr.StartVanOrder(orderNo);
        }
        #endregion

        #region 整车生产单上线并扫描关键件
        [Transaction(TransactionMode.Requires)]
        public void ScanQualityBarCodeAndStartVanOrder(string orderNo, string qualityBarcode)
        {
            this.orderMgr.StartVanOrder(orderNo);
            this.orderMgr.ScanQualityBarCode(orderNo, qualityBarcode, null, null, false, false);
        }
        #endregion

        #region 驾驶室移库并投料
        [Transaction(TransactionMode.Requires)]
        public void TansferCab(string orderNo, string flowCode, string qualityBarcode)
        {
            this.orderMgr.TansferCab(orderNo, flowCode, qualityBarcode);
        }
        #endregion

        #region 关键件扫描
        [Transaction(TransactionMode.Requires)]
        public void ScanQualityBarCode(string orderNo, string qualityBarcode, string opRef, bool isForce, bool isVI)
        {
            this.orderMgr.ScanQualityBarCode(orderNo, qualityBarcode, opRef, null, isForce, isVI);
        }
        #endregion

        #region 关键件退料
        [Transaction(TransactionMode.Requires)]
        public void WithdrawQualityBarCode(string qualityBarcode)
        {
            this.orderMgr.WithdrawQualityBarCode(qualityBarcode);
        }
        #endregion

        #region 关键件替换
        [Transaction(TransactionMode.Requires)]
        public void ReplaceQualityBarCode(string withdrawQualityBarcode, string scanQualityBarcode)
        {
            this.orderMgr.ReplaceQualityBarCode(withdrawQualityBarcode, scanQualityBarcode);
        }
        #endregion

        #region 获取关键件信息
        public QualityBarcode GetQualityBarCode(string qualityBarcode)
        {
            if (string.IsNullOrWhiteSpace(qualityBarcode))
            {
                throw new BusinessException("条码不能为空。");
            }

            string huPrefix = this.genericMgr.FindAllWithNativeSql<string>("select PreFixed from SYS_SNRule where Code = ?", com.Sconit.CodeMaster.DocumentsType.INV_Hu).Single();

            if (qualityBarcode.StartsWith(huPrefix))
            {
                com.Sconit.Entity.INV.Hu hu = this.genericMgr.FindAllWithNativeSql<com.Sconit.Entity.INV.Hu>("select * from INV_HU where HuId = ?", qualityBarcode).SingleOrDefault();
                if (hu == null)
                {
                    throw new BusinessException("条码不存在。");
                }

                QualityBarcode barCode = new QualityBarcode();
                barCode.Item = hu.Item;
                barCode.ItemDescription = hu.ItemDescription;
                barCode.ReferenceItemCode = hu.ReferenceItemCode;
                //barCode.Supplier = supplier.Code;
                //barCode.SupplierName = supplier.Name;
                barCode.LotNo = hu.LotNo;

                return barCode;
            }
            else
            {
                if (qualityBarcode.Length != 17)
                {
                    throw new BusinessException("条码长度不是17位。");
                }

                string supplierShortCode = qualityBarcode.Substring(0, 4);
                string itemShortCode = qualityBarcode.Substring(4, 5);
                string lotNo = qualityBarcode.Substring(9, 4);

                com.Sconit.Entity.MD.Supplier supplier = this.genericMgr.FindAll<com.Sconit.Entity.MD.Supplier>("from com.Sconit.Entity.MD.Supplier where ShortCode = ?", supplierShortCode).SingleOrDefault();
                if (supplier == null)
                {
                    throw new BusinessException("条码中的供应商短代码不正确。");
                }

                com.Sconit.Entity.MD.Item item = this.genericMgr.FindAll<com.Sconit.Entity.MD.Item>("from com.Sconit.Entity.MD.Item where ShortCode = ?", itemShortCode).SingleOrDefault();
                if (item == null)
                {
                    throw new BusinessException("条码中的零件短代码不正确。");
                }

                QualityBarcode barCode = new QualityBarcode();
                barCode.Item = item.Code;
                barCode.ItemDescription = item.Description;
                barCode.ReferenceItemCode = item.ReferenceCode;
                barCode.Supplier = supplier.Code;
                barCode.SupplierName = supplier.Name;
                barCode.LotNo = lotNo;

                return barCode;
            }
        }
        #endregion

        #region LotNoScan
        public void LotNoScan(string opRef, string traceCode, string barCode)
        {
            this.orderMgr.LotNoScan(opRef, traceCode, barCode);
        }

        public void LotNoDelete(string barCode)
        {
            this.orderMgr.LotNoDelete(barCode);
        }
        #endregion

        #region 扫描发动机
        public void ScanEngineTraceBarCode(string engineTrace, string traceCode)
        {
            this.orderMgr.ScanEngineTraceBarCode(engineTrace, traceCode);
        }
        #endregion

        [Transaction(TransactionMode.Requires)]
        public void ReceiveVanOrder(string orderNo, bool isCheckIssue, bool isCheckItemTrace)
        {
            this.orderMgr.ReceiveVanOrder(orderNo, isCheckIssue, isCheckItemTrace, false);
        }

        [Transaction(TransactionMode.Requires)]
        public void ReceiveVanOrder(string traceCode, string prodLine, bool isCheckIssue, bool isCheckItemTrace)
        {
            var orderMasters = this.genericMgr.FindAll<com.Sconit.Entity.ORD.OrderMaster>("select o from OrderMaster as o where o.Flow=? and o.TraceCode=?", new object[] { prodLine, traceCode });
            if (orderMasters == null || orderMasters.Count == 0)
            {
                throw new BusinessException(string.Format("生产线{0}+Van好{1}找不到对应的生产单，请确认。", prodLine, traceCode));
            }
            this.orderMgr.ReceiveVanOrder(orderMasters.First().OrderNo, isCheckIssue, isCheckItemTrace, false);
        }


    }
}
