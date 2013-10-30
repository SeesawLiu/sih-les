using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Castle.Services.Transaction;
using com.Sconit.Entity.INV;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.SCM;
using com.Sconit.Entity.VIEW;
using com.Sconit.PrintModel.INV;
using com.Sconit.Utility;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.FIS;

namespace com.Sconit.Service.Impl
{
    [Transactional]
    public class HuMgrImpl : BaseMgr, IHuMgr
    {
        private IPublishing proxy;
        public IPubSubMgr pubSubMgr { get; set; }
        public IGenericMgr genericMgr { get; set; }
        public INumberControlMgr numberControlMgr { get; set; }

        public IItemMgr itemMgr { get; set; }

        [Transaction(TransactionMode.Requires)]
        public IList<Hu> CreateHu(ReceiptMaster receiptMaster, ReceiptDetail receiptDetail, DateTime effectiveDate)
        {
            IList<Hu> huList = new List<Hu>();
            decimal remainReceivedQty = receiptDetail.ReceivedQty;
            receiptDetail.LotNo = LotNoHelper.GenerateLotNo(effectiveDate); //取生效日期为批号生成日期
            IDictionary<string, decimal> huIdDic = numberControlMgr.GetHuId(receiptDetail);

            if (huIdDic != null && huIdDic.Count > 0)
            {
                foreach (string huId in huIdDic.Keys)
                {
                    Hu hu = new Hu();
                    hu.HuId = huId;
                    hu.LotNo = receiptDetail.LotNo;
                    hu.Item = receiptDetail.Item;
                    hu.ItemDescription = receiptDetail.ItemDescription;
                    hu.BaseUom = receiptDetail.BaseUom;
                    hu.Qty = huIdDic[huId];
                    hu.ManufactureParty = receiptMaster.PartyFrom;   //取区域代码为制造商代码
                    hu.ManufactureDate = LotNoHelper.ResolveLotNo(receiptDetail.LotNo);
                    hu.PrintCount = 0;
                    hu.ConcessionCount = 0;
                    hu.ReferenceItemCode = receiptDetail.ReferenceItemCode;
                    hu.UnitCount = receiptDetail.UnitCount;
                    hu.UnitQty = itemMgr.ConvertItemUomQty(receiptDetail.Item, receiptDetail.Uom, 1, receiptDetail.BaseUom);
                    hu.Uom = receiptDetail.Uom;
                    hu.IsOdd = hu.Qty < hu.UnitCount;
                    hu.OrderNo = receiptDetail.OrderNo;
                    hu.ReceiptNo = receiptMaster.ReceiptNo;
                    hu.IsChangeUnitCount = false;
                    //hu.UnitCountDescription = receiptDetail.UnitCountDescription;
                    genericMgr.Create(hu);
                    //创建条码中间表
                    this.CreateBarCode(hu,string.Empty);
                    //this.AsyncSendPrintData(hu);
                    huList.Add(hu);
                }
            }

            return huList;
        }

        [Transaction(TransactionMode.Requires)]
        public IList<Hu> CreateHu(FlowMaster flowMaster, IList<FlowDetail> flowDetailList)
        {
            IList<Hu> huList = new List<Hu>();
            foreach (FlowDetail flowDetail in flowDetailList)
            {
                IDictionary<string, decimal> huIdDic = numberControlMgr.GetHuId(flowDetail);
                if (huIdDic != null && huIdDic.Count > 0)
                {
                    foreach (string huId in huIdDic.Keys)
                    {
                        Hu hu = new Hu();
                        hu.HuId = huId;
                        hu.LotNo = flowDetail.LotNo;
                        hu.Item = flowDetail.Item;
                        hu.ItemDescription = genericMgr.FindById<Item>(flowDetail.Item).Description;
                        hu.BaseUom = flowDetail.BaseUom;
                        hu.Qty = huIdDic[huId];
                        hu.ManufactureParty = flowDetail.ManufactureParty;
                        hu.ManufactureDate = LotNoHelper.ResolveLotNo(flowDetail.LotNo);
                        hu.PrintCount = 0;
                        hu.ConcessionCount = 0;
                        hu.ReferenceItemCode = flowDetail.ReferenceItemCode;
                        hu.UnitCount = flowDetail.UnitCount;
                        hu.UnitQty = itemMgr.ConvertItemUomQty(flowDetail.Item, flowDetail.Uom, 1, flowDetail.BaseUom);
                        hu.Uom = flowDetail.Uom;
                        hu.IsOdd = hu.Qty < hu.UnitCount;
                        hu.SupplierLotNo = flowDetail.SupplierLotNo;
                        hu.IsChangeUnitCount = flowDetail.IsChangeUnitCount;
                        hu.UnitCountDescription = flowDetail.UnitCountDescription;
                        hu.ContainerDesc = flowDetail.ContainerDescription;
                        hu.LocationTo = flowMaster.PartyTo;
                        genericMgr.Create(hu);
                        //this.AsyncSendPrintData(hu);
                        //创建条码中间表
                        this.CreateBarCode(hu, string.Empty);
                        huList.Add(hu);
                    }
                }
            }
            return huList;

        }

        [Transaction(TransactionMode.Requires)]
        public IList<Hu> CreateHu(OrderMaster orderMaster, IList<OrderDetail> orderDetailList)
        {
            IList<Hu> huList = new List<Hu>();
            foreach (OrderDetail orderDetail in orderDetailList)
            {
                IDictionary<string, decimal> huIdDic = numberControlMgr.GetHuId(orderDetail);
                if (huIdDic != null && huIdDic.Count > 0)
                {
                    foreach (string huId in huIdDic.Keys)
                    {
                        Hu hu = new Hu();
                        hu.HuId = huId;
                        hu.LotNo = orderDetail.LotNo;
                        hu.Item = orderDetail.Item;
                        hu.ItemDescription = orderDetail.ItemDescription;
                        hu.BaseUom = orderDetail.BaseUom;
                        hu.Qty = huIdDic[huId];
                        hu.ManufactureParty = orderDetail.ManufactureParty;
                        hu.ManufactureDate = LotNoHelper.ResolveLotNo(orderDetail.LotNo);
                        hu.PrintCount = 0;
                        hu.ConcessionCount = 0;
                        hu.ReferenceItemCode = orderDetail.ReferenceItemCode;
                        hu.UnitCount = orderDetail.UnitCount;
                        hu.UnitQty = orderDetail.UnitQty;
                        hu.Uom = orderDetail.Uom;
                        hu.IsOdd = hu.Qty < hu.UnitCount;
                        hu.IsChangeUnitCount = orderDetail.IsChangeUnitCount;
                        hu.UnitCountDescription = orderDetail.UnitCountDescription;
                        hu.SupplierLotNo = orderDetail.SupplierLotNo;
                        hu.ContainerDesc = orderDetail.ContainerDescription;
                        hu.LocationTo = orderMaster.PartyTo;
                        genericMgr.Create(hu);
                        //this.AsyncSendPrintData(hu);
                        //创建条码中间表
                        this.CreateBarCode(hu, string.Empty);
                        huList.Add(hu);
                    }
                }
            }
            return huList;
        }

        [Transaction(TransactionMode.Requires)]
        public IList<Hu> CreateHu(IpMaster ipMaster, IList<IpDetail> ipDetailList)
        {
            IList<Hu> huList = new List<Hu>();
            foreach (IpDetail ipDetail in ipDetailList)
            {
                IDictionary<string, decimal> huIdDic = numberControlMgr.GetHuId(ipDetail);
                if (huIdDic != null && huIdDic.Count > 0)
                {
                    foreach (string huId in huIdDic.Keys)
                    {
                        Hu hu = new Hu();
                        hu.HuId = huId;
                        hu.LotNo = ipDetail.LotNo;
                        hu.Item = ipDetail.Item;
                        hu.ItemDescription = ipDetail.ItemDescription;
                        hu.BaseUom = ipDetail.BaseUom;
                        hu.Qty = huIdDic[huId];
                        hu.ManufactureParty = ipDetail.ManufactureParty;
                        hu.ManufactureDate = LotNoHelper.ResolveLotNo(ipDetail.LotNo);
                        hu.PrintCount = 0;
                        hu.ConcessionCount = 0;
                        hu.ReferenceItemCode = ipDetail.ReferenceItemCode;
                        hu.UnitCount = ipDetail.UnitCount;
                        hu.UnitQty = ipDetail.UnitQty;
                        hu.Uom = ipDetail.Uom;
                        hu.IsOdd = hu.Qty < hu.UnitCount;
                        hu.IsChangeUnitCount = ipDetail.IsChangeUnitCount;
                        hu.UnitCountDescription = ipDetail.UnitCountDescription;
                        hu.SupplierLotNo = ipDetail.SupplierLotNo;
                        hu.ContainerDesc = ipDetail.ContainerDescription;
                        hu.LocationTo = ipMaster.PartyTo;
                        genericMgr.Create(hu);
                        //this.AsyncSendPrintData(hu);
                        //创建条码中间表
                        this.CreateBarCode(hu, ipMaster.IpNo);
                        huList.Add(hu);
                    }
                }
            }
            return huList;
        }

        [Transaction(TransactionMode.Requires)]
        public IList<Hu> CreateHu(Item item)
        {
            IList<Hu> huList = new List<Hu>();

            IDictionary<string, decimal> huIdDic = numberControlMgr.GetHuId(item);
            if (huIdDic != null && huIdDic.Count > 0)
            {
                foreach (string huId in huIdDic.Keys)
                {
                    Hu hu = new Hu();
                    hu.HuId = huId;
                    hu.LotNo = item.LotNo;
                    hu.Item = item.Code;
                    hu.ItemDescription = item.Description;
                    hu.BaseUom = item.Uom;
                    hu.Qty = huIdDic[huId];
                    hu.ManufactureParty = item.ManufactureParty;
                    hu.ManufactureDate = LotNoHelper.ResolveLotNo(item.LotNo);
                    hu.PrintCount = 0;
                    hu.ConcessionCount = 0;
                    hu.ReferenceItemCode = item.ReferenceCode;
                    hu.UnitCount = item.HuUnitCount;
                    hu.UnitQty = itemMgr.ConvertItemUomQty(item.Code, item.Uom, 1, item.HuUom);
                    hu.Uom = item.HuUom;
                    hu.IsOdd = hu.Qty < hu.UnitCount;
                    hu.SupplierLotNo = item.supplierLotNo;
                    hu.IsChangeUnitCount = true;
                    genericMgr.Create(hu);
                    hu.ContainerDesc = item.Container;
                    //this.AsyncSendPrintData(hu);
                    //创建条码中间表
                    this.CreateBarCode(hu, string.Empty);
                    huList.Add(hu);
                }

            }
            return huList;
        }

        [Transaction(TransactionMode.Requires)]
        public Hu CreateHu(Item item, string huId)
        {
            Hu hu = new Hu();
            hu.HuId = huId;
            hu.LotNo = item.LotNo;
            hu.Item = item.Code;
            hu.ItemDescription = item.Description;
            hu.BaseUom = item.Uom;
            hu.Qty = item.HuQty;
            hu.ManufactureParty = item.ManufactureParty;
            hu.ManufactureDate = LotNoHelper.ResolveLotNo(item.LotNo);
            hu.PrintCount = 0;
            hu.ConcessionCount = 0;
            hu.ReferenceItemCode = item.ReferenceCode;
            hu.UnitCount = item.HuUnitCount;
            hu.UnitQty = itemMgr.ConvertItemUomQty(item.Code, item.Uom, 1, item.HuUom);
            hu.Uom = item.HuUom;
            hu.IsOdd = hu.Qty < hu.UnitCount;
            hu.SupplierLotNo = item.supplierLotNo;
            hu.IsChangeUnitCount = true;
            genericMgr.Create(hu);
            //创建条码中间表
            this.CreateBarCode(hu, string.Empty);
            return hu;
        }

        [Transaction(TransactionMode.Requires)]
        public Hu CloneHu(Hu oldHu, decimal qty)
        {
            IList<Hu> huList = new List<Hu>();
            IDictionary<string, decimal> huIdDic = numberControlMgr.GetHuId(oldHu.LotNo, oldHu.Item, oldHu.ManufactureParty, oldHu.UnitQty, oldHu.UnitQty);
            if (huIdDic != null && huIdDic.Count > 0)
            {
                foreach (string huId in huIdDic.Keys)
                {
                    Hu hu = new Hu();
                    hu.HuId = huId;
                    hu.LotNo = oldHu.LotNo;
                    hu.Item = oldHu.Item;
                    hu.ItemDescription = oldHu.ItemDescription;
                    hu.BaseUom = oldHu.BaseUom;
                    hu.Qty = qty;
                    hu.ManufactureParty = oldHu.ManufactureParty;
                    hu.ManufactureDate = LotNoHelper.ResolveLotNo(oldHu.LotNo);
                    hu.PrintCount = 0;
                    hu.ConcessionCount = 0;
                    hu.ReferenceItemCode = oldHu.ReferenceItemCode;
                    hu.UnitCount = oldHu.UnitCount;
                    hu.UnitQty = oldHu.UnitQty;
                    hu.Uom = oldHu.Uom;
                    hu.IsOdd = hu.Qty < hu.UnitCount;
                    hu.IsChangeUnitCount = oldHu.IsChangeUnitCount;
                    hu.UnitCountDescription = oldHu.UnitCountDescription;
                    genericMgr.Create(hu);
                    this.AsyncSendPrintData(hu);
                    huList.Add(hu);
                }
            }
            return huList[0];
        }

        public IList<Hu> LoadHus(string[] huIdList)
        {
            IList<object> para = new List<object>();
            string selectHuStatement = string.Empty;
            foreach (string huId in huIdList)
            {
                if (selectHuStatement == string.Empty)
                {
                    selectHuStatement = "from Hu where HuId in (?";
                }
                else
                {
                    selectHuStatement += ",?";
                }
                para.Add(huId);
            }
            selectHuStatement += ")";
            return this.genericMgr.FindAll<Hu>(selectHuStatement, para.ToArray());
        }

        public IList<Hu> LoadHus(IList<string> huIdList)
        {
            return LoadHus(huIdList.ToArray());
        }

        public HuStatus GetHuStatus(string huId)
        {
            Hu hu = this.genericMgr.FindById<Hu>(huId);

            HuStatus huStatus = Mapper.Map<Hu, HuStatus>(hu);
            huStatus.Status = CodeMaster.HuStatus.NA;

            string hql = "from LocationLotDetail where HuId = ?";

            IList<LocationLotDetail> locationLotDetailList = this.genericMgr.FindAll<LocationLotDetail>(hql, huId);

            if (locationLotDetailList != null && locationLotDetailList.Count > 0)
            {
                LocationLotDetail locationLotDetail = locationLotDetailList[0];
                WrapHuStatus(huStatus, locationLotDetail);
            }

            if (huStatus.Status == CodeMaster.HuStatus.NA)
            {

                hql = "from IpLocationDetail where HuId = ? and IsClose = ?";

                IList<IpLocationDetail> ipLocationDetailList = this.genericMgr.FindAll<IpLocationDetail>(hql, new object[] { huId, false });

                if (ipLocationDetailList != null && ipLocationDetailList.Count > 0)
                {
                    IpLocationDetail ipLocationDetail = ipLocationDetailList[0];
                    WrapHuStatus(huStatus, ipLocationDetail);
                }
            }

            return huStatus;
        }

        public IList<HuStatus> GetHuStatus(IList<string> huIdList)
        {
            if (huIdList != null && huIdList.Count > 0)
            {
                IList<Hu> huList = LoadHus(((List<string>)huIdList).ToArray());

                string selectLocationLotDetailStatement = string.Empty;
                string selectIpLocationDetailStatement = string.Empty;
                IList<object> paras = new List<object>();
                foreach (string huId in huIdList)
                {
                    if (selectLocationLotDetailStatement == string.Empty)
                    {
                        selectLocationLotDetailStatement = "from LocationLotDetail where HuId in (?";
                        selectIpLocationDetailStatement = "from IpLocationDetail where IsClose = false and HuId in (?";
                    }
                    else
                    {
                        selectLocationLotDetailStatement += ", ?";
                        selectIpLocationDetailStatement += ", ?";
                    }
                    paras.Add(huId);
                }
                selectLocationLotDetailStatement += ")";
                selectIpLocationDetailStatement += ")";

                IList<LocationLotDetail> locationLotDetailList = this.genericMgr.FindAll<LocationLotDetail>(selectLocationLotDetailStatement, paras.ToArray());
                IList<IpLocationDetail> ipLocationDetailList = this.genericMgr.FindAll<IpLocationDetail>(selectIpLocationDetailStatement, paras.ToArray());

                IList<HuStatus> huStatusList = new List<HuStatus>();
                foreach (Hu hu in huList)
                {
                    HuStatus huStatus = Mapper.Map<Hu, HuStatus>(hu);
                    huStatus.Status = CodeMaster.HuStatus.NA;

                    if (locationLotDetailList != null && locationLotDetailList.Count > 0)
                    {
                        LocationLotDetail locationLotDetail = locationLotDetailList.Where(locDet => locDet.HuId == hu.HuId).SingleOrDefault();
                        WrapHuStatus(huStatus, locationLotDetail);
                    }

                    if (huStatus.Status == CodeMaster.HuStatus.NA)
                    {
                        IpLocationDetail ipLocationDetail = ipLocationDetailList.Where(locDet => locDet.HuId == hu.HuId).SingleOrDefault();
                        WrapHuStatus(huStatus, ipLocationDetail);
                    }
                    huStatusList.Add(huStatus);
                }

                return huStatusList;
            }

            return null;
        }

        #region private methods
        private void WrapHuStatus(HuStatus huStatus, LocationLotDetail locationLotDetail)
        {
            if (locationLotDetail != null && locationLotDetail.Qty > 0)
            {
                Location location = this.genericMgr.FindById<Location>(locationLotDetail.Location);

                huStatus.Status = CodeMaster.HuStatus.Location;
                huStatus.Region = location.Region;
                huStatus.Location = locationLotDetail.Location;
                huStatus.Bin = locationLotDetail.Bin;
                huStatus.IsConsignment = locationLotDetail.IsConsignment;
                huStatus.PlanBill = locationLotDetail.PlanBill;
                huStatus.QualityType = locationLotDetail.QualityType;
                huStatus.IsFreeze = locationLotDetail.IsFreeze;
                huStatus.IsATP = locationLotDetail.IsATP;
                huStatus.OccupyType = locationLotDetail.OccupyType;
                huStatus.OccupyReferenceNo = locationLotDetail.OccupyReferenceNo;
            }
        }

        private void WrapHuStatus(HuStatus huStatus, IpLocationDetail ipLocationDetail)
        {
            if (ipLocationDetail != null && !ipLocationDetail.IsClose)
            {
                IpDetail ipDetail = this.genericMgr.FindById<IpDetail>(ipLocationDetail.IpDetailId);

                huStatus.IpNo = ipLocationDetail.IpNo;
                huStatus.LocationFrom = ipDetail.LocationFrom;
                huStatus.LocationTo = ipDetail.LocationTo;
                huStatus.Status = CodeMaster.HuStatus.Ip;
                huStatus.IsConsignment = ipLocationDetail.IsConsignment;
                huStatus.PlanBill = ipLocationDetail.PlanBill;
                huStatus.QualityType = ipLocationDetail.QualityType;
                huStatus.IsFreeze = ipLocationDetail.IsFreeze;
                huStatus.IsATP = ipLocationDetail.IsATP;
                huStatus.OccupyType = ipLocationDetail.OccupyType;
                huStatus.OccupyReferenceNo = ipLocationDetail.OccupyReferenceNo;
            }
        }


        private void CreateBarCode(Hu hu, string asn)
        {
            genericMgr.Create(new CreateBarCode
                                {
                                    HuId = hu.HuId,
                                    LotNo = hu.LotNo,
                                    Item = hu.Item,
                                    Qty = hu.Qty,
                                    ManufactureParty = hu.ManufactureParty,
                                    ASN = asn,
                                    CreateDate = DateTime.Now,
                                    IsCreateDat=false
                                });
        }

        #endregion

        #region 异步打印
        public void SendPrintData(Hu hu)
        {
            try
            {
                PrintHu printHu = Mapper.Map<Hu, PrintHu>(hu);
                proxy = pubSubMgr.CreateProxy();
                proxy.Publish(printHu);
            }
            catch (Exception ex)
            {
                pubSubLog.Error("Send data to print sevrer error:", ex);
            }
        }

        public void AsyncSendPrintData(Hu hu)
        {
            AsyncSend asyncSend = new AsyncSend(this.SendPrintData);
            asyncSend.BeginInvoke(hu, null, null);
        }

        public delegate void AsyncSend(Hu hu);
        #endregion


        #region 客户化代码
        [Transaction(TransactionMode.Requires)]
        public IList<Hu> CreateHu(OrderDetail orderDetail, Boolean isRepack, string manufactureParty, string lotNo, decimal totalQty, decimal unitQty, decimal huQty, string oldHus, string binTo, Boolean isRepackForOrder)
        {
            IList<Hu> huList = new List<Hu>();

            IDictionary<string, decimal> huIdDic = numberControlMgr.GetHuId(lotNo, orderDetail.Item, manufactureParty, totalQty, unitQty);
            if (huIdDic != null && huIdDic.Count > 0)
            {
                Hu hu = new Hu();
                hu.HuId = huIdDic.SingleOrDefault().Key;
                hu.LotNo = lotNo;
                hu.Item = orderDetail.Item;
                hu.ItemDescription = orderDetail.ItemDescription;
                hu.BaseUom = orderDetail.BaseUom;
                hu.Qty = huQty;
                hu.ManufactureParty = manufactureParty;
                hu.ManufactureDate = LotNoHelper.ResolveLotNo(lotNo);
                hu.PrintCount = 0;
                hu.ConcessionCount = 0;
                hu.ReferenceItemCode = orderDetail.ReferenceItemCode;
                hu.UnitCount = orderDetail.UnitCount;
                hu.UnitQty = orderDetail.UnitQty;
                hu.Uom = orderDetail.Uom;
                hu.IsOdd = huQty != hu.UnitCount;
                hu.IsChangeUnitCount = orderDetail.IsChangeUnitCount;
                hu.UnitCountDescription = orderDetail.UnitCountDescription;
                hu.Bin = binTo;
                hu.LocationTo = orderDetail.LocationTo;
                hu.OldHus = oldHus;
                genericMgr.Create(hu);
                //this.AsyncSendPrintData(hu);
                this.genericMgr.Create(hu);
                huList.Add(hu);

                HuMapping huMapping = new HuMapping();
                huMapping.HuId = huIdDic.SingleOrDefault().Key;
                huMapping.OldHus = oldHus;
                huMapping.Item = orderDetail.Item;
                if (isRepackForOrder == true)
                {
                    huMapping.OrderNo = orderDetail.OrderNo;
                    huMapping.OrderDetId = orderDetail.Id;
                }
                else
                {
                    huMapping.OrderNo = "";
                    huMapping.OrderDetId = 0;
                }
                huMapping.IsRepack = isRepack;
                huMapping.Qty = huQty;
                huMapping.IsEffective = false;
                this.genericMgr.Create(huMapping);
            }
            return huList;
        }



        #endregion


        public IList<Hu> CreateHu(PickTask pickTask, string lotNo)
        {
            IList<Hu> huList = new List<Hu>();

            IDictionary<string, decimal> huIdDic = numberControlMgr.GetHuId(lotNo, pickTask.Item, 
                pickTask.Supplier, pickTask.OrderedQty, pickTask.UnitCount);

            OrderDetail od = this.genericMgr.FindById<OrderDetail>(pickTask.OrdDetId);

            if (huIdDic != null && huIdDic.Count > 0)
            {
                foreach (string huId in huIdDic.Keys)
                {
                    Hu hu = new Hu();
                    hu.HuId = huId;
                    hu.LotNo = lotNo;
                    hu.Item = pickTask.Item;
                    hu.ItemDescription = pickTask.ItemDesc;
                    hu.BaseUom = pickTask.BaseUom;
                    hu.Qty = huIdDic[huId];
                    if(String.IsNullOrEmpty(pickTask.Supplier)) {
                        hu.ManufactureParty = pickTask.PartyFrom;
                    } else {
                        hu.ManufactureParty = pickTask.Supplier;
                    }
                    if(String.IsNullOrEmpty(lotNo)) {
                        hu.ManufactureDate = DateTime.Now;
                    } else {
                        hu.ManufactureDate = LotNoHelper.ResolveLotNo(lotNo);
                    }
                    
                    hu.PrintCount = 0;
                    hu.ConcessionCount = 0;
                    hu.ReferenceItemCode = od.ReferenceItemCode;
                    hu.UnitCount = pickTask.UnitCount;
                    hu.UnitQty = od.UnitQty;
                    hu.Uom = pickTask.Uom;
                    hu.IsOdd = hu.Qty < hu.UnitCount;
                    hu.IsChangeUnitCount = od.IsChangeUnitCount;
                    hu.UnitCountDescription = od.UnitCountDescription;
                    hu.SupplierLotNo = od.SupplierLotNo;
                    hu.ContainerDesc = od.ContainerDescription;
                    hu.LocationTo = pickTask.LocationTo;
                    hu.Flow = pickTask.Flow;
                    hu.OrderNo = pickTask.OrderNo;
                    hu.Bin = od.BinTo;
                    genericMgr.Create(hu);
                    //this.AsyncSendPrintData(hu);
                    huList.Add(hu);
                }
            }

            return huList;
        }
    }
}
