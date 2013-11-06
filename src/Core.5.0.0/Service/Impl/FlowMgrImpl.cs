using com.Sconit.Entity.FIS;

namespace com.Sconit.Service.Impl
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AutoMapper;
    using Castle.Services.Transaction;
    using com.Sconit.Entity.Exception;
    using com.Sconit.Entity.MD;
    using com.Sconit.Entity.SCM;
    using com.Sconit.Utility;
    using NHibernate.Criterion;
    using NPOI.HSSF.UserModel;
    using NPOI.SS.UserModel;
    using com.Sconit.Entity;
    using NHibernate.Type;
    using NHibernate;

    [Transactional]
    public class FlowMgrImpl : BaseMgr, IFlowMgr
    {
        #region 变量
        public IGenericMgr genericMgr { get; set; }
        #endregion

        public IList<FlowBinding> GetFlowBinding(string flow)
        {
            DetachedCriteria detachedCriteria = DetachedCriteria.For<FlowBinding>();
            detachedCriteria.Add(Expression.Eq("MasterFlow.Code", flow));

            return this.genericMgr.FindAll<FlowBinding>(detachedCriteria);
        }

        public FlowStrategy GetFlowStrategy(string flow)
        {
            try
            {
                return this.genericMgr.FindById<FlowStrategy>(flow);
            }
            catch (Exception)
            { }
            return null;
        }

        public IList<FlowDetail> GetFlowDetailList(string flow)
        {
            return GetFlowDetailList(flow, false, true);
        }

        public IList<FlowDetail> GetFlowDetailList(string flow, bool includeInactive)
        {
            DateTime dateTimeNow = DateTime.Now;
            DetachedCriteria criteria = DetachedCriteria.For<FlowDetail>();
            criteria.Add(Expression.Eq("Flow", flow));
            if (!includeInactive)
            {
                criteria.Add(Expression.Or(Expression.IsNull("StartDate"), Expression.Le("StartDate", dateTimeNow)));
                criteria.Add(Expression.Or(Expression.IsNull("EndDate"), Expression.Ge("EndDate", dateTimeNow)));
            }
            var list = this.genericMgr.FindAll<FlowDetail>(criteria, 0, 500);

            return list;
        }

        public IList<FlowDetail> GetFlowDetailList(string flowCode, bool includeInactive, bool includeReferenceFlow)
        {
            var flowDetails = this.GetFlowDetailList(flowCode, includeInactive).ToList();

            if (includeReferenceFlow)
            {
                var flowMaster = this.genericMgr.FindById<Entity.SCM.FlowMaster>(flowCode);
                if (!string.IsNullOrWhiteSpace(flowMaster.ReferenceFlow) && flowMaster.IsActive)
                {
                    var refFlowDetails = this.GetFlowDetailList(flowMaster.ReferenceFlow, false, true, flowMaster);
                    var newFlowDetails = Mapper.Map<IList<FlowDetail>, IList<FlowDetail>>(refFlowDetails);
                    foreach (var detail in newFlowDetails)
                    {
                        detail.CurrentFlowMaster = flowMaster;
                    }

                    flowDetails.AddRange(newFlowDetails);
                }
            }
            return flowDetails;
        }

        public FlowMaster GetReverseFlow(FlowMaster flow, IList<string> itemCodeList)
        {
            FlowMaster reverseFlow = new FlowMaster();
            Mapper.Map(flow, reverseFlow);
            reverseFlow.PartyFrom = flow.PartyTo;
            reverseFlow.PartyTo = flow.PartyFrom;
            reverseFlow.IsCheckPartyFromAuthority = flow.IsCheckPartyToAuthority;
            reverseFlow.IsCheckPartyToAuthority = flow.IsCheckPartyFromAuthority;
            reverseFlow.ShipFrom = flow.ShipTo;
            reverseFlow.ShipTo = flow.ShipFrom;
            reverseFlow.LocationFrom = flow.LocationTo;
            reverseFlow.LocationTo = flow.LocationFrom;
            reverseFlow.IsShipScanHu = flow.IsReceiveScanHu;
            reverseFlow.IsReceiveScanHu = flow.IsShipScanHu;
            reverseFlow.IsShipFifo = flow.IsReceiveFifo;
            reverseFlow.IsReceiveFifo = flow.IsShipFifo;
            reverseFlow.IsShipExceed = flow.IsReceiveExceed;
            reverseFlow.IsReceiveExceed = flow.IsShipExceed;
            reverseFlow.IsShipFulfillUC = flow.IsReceiveFulfillUC;
            reverseFlow.IsReceiveFulfillUC = flow.IsShipFulfillUC;
            reverseFlow.IsInspect = flow.IsRejectInspect;
            reverseFlow.IsRejectInspect = flow.IsInspect;
            //以后有什么字段再加

            #region 路线明细
            if (flow.FlowDetails == null || flow.FlowDetails.Count == 0)
            {
                string hql = "from FlowDetail where Flow = ?";
                IList<object> parm = new List<object>();
                parm.Add(flow.Code);
                if (itemCodeList != null && itemCodeList.Count() > 0)
                {
                    string whereHql = string.Empty;
                    foreach (string itemCode in itemCodeList.Distinct())
                    {
                        if (whereHql == string.Empty)
                        {
                            whereHql = " and Item in (?";
                        }
                        else
                        {
                            whereHql += ",?";
                        }
                        parm.Add(itemCode);
                    }
                    whereHql += ")";
                    hql += whereHql;
                }
                hql += " order by Sequence";

                flow.FlowDetails = this.genericMgr.FindAll<FlowDetail>(hql, parm.ToArray());
            }

            if (flow.FlowDetails != null && flow.FlowDetails.Count > 0)
            {
                IList<FlowDetail> reverseFlowDetailList = new List<FlowDetail>();
                foreach (FlowDetail flowDetail in flow.FlowDetails)
                {
                    FlowDetail reverseFlowDetail = new FlowDetail();
                    Mapper.Map(flowDetail, reverseFlowDetail);
                    reverseFlowDetail.LocationFrom = flowDetail.LocationTo;
                    reverseFlowDetail.LocationTo = flowDetail.LocationFrom;
                    reverseFlowDetail.IsRejectInspect = flowDetail.IsInspect;
                    reverseFlowDetail.IsInspect = flowDetail.IsRejectInspect;
                    //PartyFrom?
                    reverseFlowDetailList.Add(reverseFlowDetail);
                }
                reverseFlow.FlowDetails = reverseFlowDetailList;
            }
            #endregion

            return reverseFlow;
        }

        [Transaction(TransactionMode.Requires)]
        public void CreateFlow(FlowMaster flowMaster)
        {
            genericMgr.Create(flowMaster);

            #region flow strategy
            FlowStrategy flowStrategy = new FlowStrategy();
            flowStrategy.Flow = flowMaster.Code;
            flowStrategy.Strategy = flowMaster.FlowStrategy;
            flowStrategy.Description = flowMaster.Description;
            //默认取小时，以后再改
            flowStrategy.TimeUnit = CodeMaster.TimeUnit.Hour;
            genericMgr.Create(flowStrategy);
            #endregion

        }

        [Transaction(TransactionMode.Requires)]
        public void UpdateFlowStrategy(FlowStrategy flowstrategy)
        {
            FlowStrategy oldFlowStrategy = genericMgr.FindById<FlowStrategy>(flowstrategy.Flow);
            if (oldFlowStrategy.Strategy != flowstrategy.Strategy)
            {
                #region 反向更新FlowMaster上的策略
                FlowMaster flowMaster = genericMgr.FindById<FlowMaster>(flowstrategy.Flow);
                flowMaster.FlowStrategy = flowstrategy.Strategy;
                genericMgr.Update(flowMaster);
            }

            //默认取小时，以后再改
            Mapper.Map<FlowStrategy, FlowStrategy>(flowstrategy, oldFlowStrategy);
            oldFlowStrategy.TimeUnit = CodeMaster.TimeUnit.Hour;
            genericMgr.Update(oldFlowStrategy);

                #endregion
        }

        [Transaction(TransactionMode.Requires)]
        public void DeleteFlow(string flow)
        {
            FlowMaster flowMaster = genericMgr.FindById<FlowMaster>(flow);

            #region 删除绑定
            IList<FlowBinding> flowBindingList = GetFlowBinding(flow);
            if (flowBindingList != null && flowBindingList.Count > 0)
            {
                genericMgr.Delete<FlowBinding>(flowBindingList);
            }
            #endregion

            #region 删除策略
            FlowStrategy flowStrategy = GetFlowStrategy(flow);
            genericMgr.Delete(flowStrategy);
            #endregion

            #region 删除明细
            IList<FlowDetail> flowDetailList = GetFlowDetailList(flow, true);
            if (flowDetailList != null && flowDetailList.Count > 0)
            {
                genericMgr.Delete<FlowDetail>(flowDetailList);
            }
            #endregion

            #region 删除头
            genericMgr.Delete(flowMaster);
            #endregion
        }

        [Transaction(TransactionMode.Requires)]
        public void CreateFlowDetail(FlowDetail flowDetail)
        {
            IList<ItemPackage> itemPackageList = genericMgr.FindAll<ItemPackage>("from ItemPackage as i where i.Item = ? and i.UnitCount = ?", new object[] { flowDetail.Item, flowDetail.UnitCount });
            if (itemPackageList == null || itemPackageList.Count == 0)
            {
                ItemPackage itemPackage = new ItemPackage();
                itemPackage.Item = flowDetail.Item;
                itemPackage.UnitCount = flowDetail.UnitCount;
                itemPackage.Description = string.IsNullOrEmpty(flowDetail.UnitCountDescription) ? string.Empty : flowDetail.UnitCountDescription;
                genericMgr.Create(itemPackage);
            }
            genericMgr.Create(flowDetail);

            #region 更新计划协议历史数据,按照路线更新
            FlowMaster flow = genericMgr.FindById<FlowMaster>(flowDetail.Flow);
            ////允许新建的明细肯定是唯一有效的
            //IList<FlowDetail> flowDetailList = this.genericMgr.FindAll<FlowDetail>("select fd from FlowDetail as fd where fd.Item='" + flowDetail.Item + "' and IsActive <> 0 and exists (select 1 from FlowMaster as fm where fm.Code=fd.Flow and fm.PartyFrom='" + flow.PartyFrom + "' and fm.IsActive = 1)");
            //if (flowDetailList.Count == 1)
            //{
            //    Location location = flowDetail.IsActive ? genericMgr.FindById<Location>(flow.LocationTo) : null;
            //    Party partyTo = flowDetail.IsActive ? genericMgr.FindById<Party>(flow.PartyTo) : null;
            //    Address address = flowDetail.IsActive ? genericMgr.FindById<Address>(flow.ShipTo) : null;

            //    string updateMstr = "Update Ord_OrderMstr_8 set Flow = ?,FlowDesc = ?,PartyTo = ?,PartyToNm = ?,ShipTo = ?,ShipToAddr = ?,LocTo = ?,LocToNm = ? where  PartyFrom = ? and exists (select 1 from Ord_OrderDet_8 as d where d.Item = ? and Ord_OrderMstr_8.OrderNo=d.OrderNo)";
            //    genericMgr.FindAllWithNativeSql(updateMstr, new object[] { flow.Code, flow.Description, flow.PartyTo, partyTo.Name, flow.ShipTo, address.AddressContent, flow.LocationTo, location.Name, flow.PartyFrom, flowDetail.Item });
            //    string updateStr = "Update Ord_OrderDet_8 set LocTo = ?,LocToNm = ?,UC = ? where Item = ? and exists (select 1 from Ord_OrderMstr_8 as o where o.PartyFrom = ? and Ord_OrderDet_8.OrderNo=o.OrderNo)";
            //    genericMgr.FindAllWithNativeSql(updateStr, new object[] { flow.LocationTo, location.Name, flowDetail.UnitCount, flowDetail.Item, flow.PartyFrom });
            //}
            #endregion

            #region 将数据插入零件标准包装中间表
            //采购路线只有入库包装，移库路线才有出库/入库包装
            if (flow.Type == CodeMaster.OrderType.Transfer
                || flow.Type == CodeMaster.OrderType.Distribution
                || flow.Type == CodeMaster.OrderType.SubContractTransfer)
            {
                string locationFrom = string.IsNullOrWhiteSpace(flowDetail.LocationFrom) ? flow.LocationFrom : flowDetail.LocationFrom;
                if (string.IsNullOrWhiteSpace(locationFrom))
                {
                    var locTo = this.genericMgr.FindById<Location>(locationFrom);
                    CreateItemStandardPackByType(flowDetail, locTo, "I");
                }
            }

            if (flow.Type == CodeMaster.OrderType.Transfer
                || flow.Type == CodeMaster.OrderType.SubContractTransfer
                || flow.Type == CodeMaster.OrderType.SubContract
                || flow.Type == CodeMaster.OrderType.CustomerGoods
                || flow.Type == CodeMaster.OrderType.Procurement)
            {
                string locationTo = string.IsNullOrWhiteSpace(flowDetail.LocationTo) ? flow.LocationTo : flowDetail.LocationTo;
                if (string.IsNullOrWhiteSpace(locationTo))
                {
                    var locTo = this.genericMgr.FindById<Location>(locationTo);
                    CreateItemStandardPackByType(flowDetail, locTo, "I");
                }
            }
            #endregion
        }

        [Transaction(TransactionMode.Requires)]
        public void UpdateFlowDetail(FlowDetail flowDetail)
        {
            #region 零件标准包装
            var dbFlowDetail = this.genericMgr.FindById<FlowDetail>(flowDetail.Id);
            this.genericMgr.CleanSession();

            var flow = genericMgr.FindById<FlowMaster>(flowDetail.Flow);
            var locFrom = string.IsNullOrWhiteSpace(flowDetail.LocationFrom) ? flow.LocationFrom : flowDetail.LocationFrom;
            var locTo = string.IsNullOrWhiteSpace(flowDetail.LocationTo) ? flow.LocationTo : flowDetail.LocationTo;
            if (dbFlowDetail.Item != flowDetail.Item || dbFlowDetail.UnitCount != flowDetail.UnitCount || dbFlowDetail.UnitCountDescription != flowDetail.UnitCountDescription)
            {
                if (!string.IsNullOrWhiteSpace(locFrom))
                {
                    UpdateItemStandardPackByType(flowDetail, locFrom, "O");
                }
                if (!string.IsNullOrWhiteSpace(locTo))
                {
                    UpdateItemStandardPackByType(flowDetail, locTo, "I");
                }
            }
            else if (dbFlowDetail.LocationFrom != flowDetail.LocationFrom)
            {
                if (!string.IsNullOrWhiteSpace(locFrom))
                {
                    UpdateItemStandardPackByType(flowDetail, locFrom, "O");
                }
            }
            else if (dbFlowDetail.LocationTo != flowDetail.LocationTo)
            {
                if (!string.IsNullOrWhiteSpace(locTo))
                {
                    UpdateItemStandardPackByType(flowDetail, locTo, "I");
                }
            }

            #endregion

            IList<ItemPackage> itemPackageList = genericMgr.FindAll<ItemPackage>("from ItemPackage as i where i.Item = ? and i.UnitCount = ?", new object[] { flowDetail.Item, flowDetail.UnitCount });
            if (itemPackageList == null || itemPackageList.Count == 0)
            {
                ItemPackage itemPackage = new ItemPackage();
                itemPackage.Item = flowDetail.Item;
                itemPackage.UnitCount = flowDetail.UnitCount;
                itemPackage.Description = string.IsNullOrEmpty(flowDetail.UnitCountDescription) ? string.Empty : flowDetail.UnitCountDescription;
                genericMgr.Create(itemPackage);
            }
            genericMgr.Update(flowDetail);
        }

        [Transaction(TransactionMode.Requires)]
        public void UpdateFlow(FlowMaster flowMaster, bool isChangeSL)
        {
            var dbFlow = this.genericMgr.FindById<FlowMaster>(flowMaster.Code);
            this.genericMgr.CleanSession();

            genericMgr.Update(flowMaster);

            // 更改明细中的包装
            var flowDets = this.genericMgr.FindAll<FlowDetail>("from FlowDetail as d where d.Flow =? ", flowMaster.Code);
            if (!string.IsNullOrWhiteSpace(dbFlow.LocationFrom) && !string.IsNullOrWhiteSpace(flowMaster.LocationFrom) && dbFlow.LocationFrom != flowMaster.LocationFrom)
            {
                var locfrom = this.genericMgr.FindById<Location>(flowMaster.LocationFrom);
                var dbLocfrom = this.genericMgr.FindById<Location>(dbFlow.LocationFrom);
                if ((locfrom.SAPLocation == "1000" || dbLocfrom.SAPLocation == "1000") && locfrom.SAPLocation != dbLocfrom.SAPLocation)
                {
                    foreach (var det in flowDets.Where(c => c.LocationFrom == null))
                    {
                        SaveItemStandardPack(det, "O", locfrom.SAPLocation);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(dbFlow.LocationTo) && !string.IsNullOrWhiteSpace(flowMaster.LocationTo) && dbFlow.LocationTo != flowMaster.LocationTo)
            {
                var locTo = this.genericMgr.FindById<Location>(flowMaster.LocationTo);
                var dbLocTo = this.genericMgr.FindById<Location>(dbFlow.LocationTo);
                if ((locTo.SAPLocation == "1000" || dbLocTo.SAPLocation == "1000") && locTo.SAPLocation != dbLocTo.SAPLocation)
                {
                    foreach (var det in flowDets.Where(c => c.LocationTo == null))
                    {
                        SaveItemStandardPack(det, "I", locTo.SAPLocation);
                    }
                }
            }

            //该方法不再使用
            //#region 当路线为有效时，更新计划协议历史数据,按照路线更新
            //if (isChangeSL && flowMaster.IsActive)
            //{
            //    string locationName = genericMgr.FindById<Location>(flowMaster.LocationTo).Name;
            //    string updateMstr = "Update Ord_OrderMstr_8 set LocTo = ?,LocToNm = ? where  Flow=?";
            //    genericMgr.FindAllWithNativeSql(updateMstr, new object[] { flowMaster.LocationTo, locationName, flowMaster.Code });
            //    string updateStr = "Update Ord_OrderDet_8 set LocTo = ?,LocToNm = ? where  exists (select 1 from Ord_OrderMstr_8 as o where o.Flow = ? and Ord_OrderDet_8.OrderNo=o.OrderNo)";
            //    genericMgr.FindAllWithNativeSql(updateStr, new object[] { flowMaster.LocationTo, locationName, flowMaster.Code });
            //}
            //#endregion



        }

        #region 删除看板明细
        [Transaction(TransactionMode.Requires)]
        public void DeleteKBFlowDetail(Int32 flowDetailId)
        {
            #region 删除看板
            // genericMgr.DeleteById<FlowDetail>(flowDetailId);
            genericMgr.Delete("from KanbanCard k where k.FlowDetailId = ?", flowDetailId, NHibernate.NHibernateUtil.Int32);
            #endregion

            genericMgr.DeleteById<FlowDetail>(flowDetailId);

        }
        #endregion


        public void UpdateFlowShiftDetails(string flow, IList<FlowShiftDetail> addFlowShiftDet,
         IList<FlowShiftDetail> updateFlowShiftDetail, IList<FlowShiftDetail> deleteFlowShiftDet)
        {
            if (addFlowShiftDet != null)
            {
                foreach (var detail in addFlowShiftDet)
                {
                    detail.Flow = flow.ToString();
                    this.genericMgr.Create(detail);
                }
            }
            if (updateFlowShiftDetail != null)
            {
                foreach (var detail in updateFlowShiftDetail)
                {
                    this.genericMgr.Update(detail);
                }
            }
            if (deleteFlowShiftDet != null)
            {
                foreach (var detail in deleteFlowShiftDet)
                {
                    this.genericMgr.Delete(detail);
                }
            }

        }

        public IList<FlowDetail> GetFlowDetails(IList<Int32> flowDetailIdList)
        {
            if (flowDetailIdList != null && flowDetailIdList.Count > 0)
            {
                IList<FlowDetail> flowDetailList = new List<FlowDetail>();

                string hql = string.Empty;
                IList<object> parm = new List<object>();

                foreach (int flowDetailId in flowDetailIdList.Distinct())
                {
                    if (hql == string.Empty)
                    {
                        hql = "from FlowDetail where Id in (?";
                    }
                    else
                    {
                        hql += ", ?";
                    }
                    parm.Add(flowDetailId);

                    if (parm.Count() >= 2000)
                    {
                        hql += ")";
                        ((List<FlowDetail>)flowDetailList).AddRange(this.genericMgr.FindAll<FlowDetail>(hql, parm.ToArray()));
                        hql = string.Empty;
                        parm = new List<object>();
                    }
                }

                hql += ")";
                if (parm.Count() > 0)
                {
                    ((List<FlowDetail>)flowDetailList).AddRange(this.genericMgr.FindAll<FlowDetail>(hql, parm.ToArray()));
                }

                return flowDetailList;
            }

            return null;
        }

        private void SaveItemStandardPack(FlowDetail flowDetail, string type, string sapLoc)
        {
            var itemStandardPackList = this.genericMgr.FindAll<ItemStandardPackDAT>("from ItemStandardPackDAT as p where p.FlowDetId=?", flowDetail.Id);

            var itemStandardPackO = itemStandardPackList.FirstOrDefault(c => c.IOType == type);
            if (itemStandardPackO != null)
            {
                itemStandardPackO.Item = flowDetail.Item;
                itemStandardPackO.Pack = flowDetail.UnitCountDescription;
                itemStandardPackO.UC = flowDetail.UnitCount;
                itemStandardPackO.CreateDate = DateTime.Now;
                itemStandardPackO.CreateDATDate = null;
                itemStandardPackO.DATFileName = null;
                itemStandardPackO.Location = sapLoc;
                genericMgr.Update(itemStandardPackO);
            }
            else
            {
                this.genericMgr.Create(new ItemStandardPackDAT
                                           {
                                               Item = flowDetail.Item,
                                               FlowDetId = flowDetail.Id,
                                               Pack = flowDetail.UnitCountDescription,
                                               UC = flowDetail.UnitCount,
                                               IOType = type,
                                               Location = sapLoc,
                                               Plant = "0084",
                                               CreateDate = DateTime.Now
                                           });
            }
        }

        private void CreateItemStandardPackByType(FlowDetail flowDetail, Location loc, string type)
        {
            if (loc.SAPLocation == "1000")
            {
                this.genericMgr.Create(new ItemStandardPackDAT
                                           {
                                               Item = flowDetail.Item,
                                               FlowDetId = flowDetail.Id,
                                               Pack = flowDetail.UnitCountDescription,
                                               UC = flowDetail.UnitCount,
                                               IOType = type,
                                               Location = loc.SAPLocation,
                                               Plant = "0084",
                                               CreateDate = DateTime.Now
                                           });
            }
        }

        private void UpdateItemStandardPackByType(FlowDetail flowDetail, string loc, string type)
        {
            var locfrom = this.genericMgr.FindById<Location>(loc);
            SaveItemStandardPack(flowDetail, type, locfrom.SAPLocation);
        }

        private IList<FlowDetail> GetFlowDetailList(string refFlowCode, bool includeInactive, bool includeReferenceFlow, FlowMaster baseFlow)
        {
            if (refFlowCode != baseFlow.Code)
            {
                var flowDetails = this.GetFlowDetailList(refFlowCode, includeInactive).ToList();

                if (includeReferenceFlow)
                {
                    var refflowMaster = this.genericMgr.FindById<Entity.SCM.FlowMaster>(refFlowCode);
                    if (!string.IsNullOrWhiteSpace(refflowMaster.ReferenceFlow) && refflowMaster.IsActive)
                    {
                        var refFlowDetails = this.GetFlowDetailList(refflowMaster.ReferenceFlow, false, true, baseFlow);
                        var newFlowDetails = Mapper.Map<IList<FlowDetail>, IList<FlowDetail>>(refFlowDetails);
                        foreach (var detail in newFlowDetails)
                        {
                            detail.CurrentFlowMaster = baseFlow;
                        }
                        flowDetails.AddRange(newFlowDetails);
                    }
                }
                return flowDetails;
            }
            return new List<FlowDetail>();
        }

        #region 采购路线明细批倒
        [Transaction(TransactionMode.Requires)]
        public void CreateFlowDetailXls(Stream inputStream)
        {
            if (inputStream.Length == 0)
            {
                throw new BusinessException("Import.Stream.Empty");
            }

            HSSFWorkbook workbook = new HSSFWorkbook(inputStream);

            ISheet sheet = workbook.GetSheetAt(0);
            IEnumerator rows = sheet.GetRowEnumerator();

            ImportHelper.JumpRows(rows, 10);
            BusinessException businessException = new BusinessException();
            #region 列定义

            int colItem = 6;// 物料
            //int colMinUnitCount = 10;//上线包装
            int colFlow = 11;//路线
            int colUnitCount = 12;//单包装
            int colUnitCountDescription = 13;//包装描述
            int colContainerDescription = 14;//容器描述
            int colIsChangeUnitCount = 15;//是否修改包装
            int colIsDelete = 16;//是否修改包装
            int rowCount = 10;
            #endregion
            //IList<FlowDetail> insertFlowDetailList = new List<FlowDetail>();
            //IList<FlowDetail> updateFlowDetailList = new List<FlowDetail>();
            IList<FlowDetail> exactFlowDetailList = new List<FlowDetail>();
            IList<FlowMaster> allFlowMaster = this.genericMgr.FindAll<FlowMaster>();
            IList<FlowDetail> allFlowDet = this.genericMgr.FindEntityWithNativeSql<FlowDetail>("select d.* from SCM_FlowDet as d where exists( select 1 from SCM_FlowMstr as m where m.code=d.Flow and m.Type=1 )");
            while (rows.MoveNext())
            {
                rowCount++;
                HSSFRow row = (HSSFRow)rows.Current;
                if (!ImportHelper.CheckValidDataRow(row, 5, 14))
                {
                    break;//边界
                }

                FlowMaster flowMaster = null;
                string flow = string.Empty;//路线
                string item = string.Empty; ;// 物料
                double unitCount = 0;//单包装
                string unitCountDescription = string.Empty;//包装描述
                string containerDescription = string.Empty;//容器描述
                string changeUnitCount = string.Empty;//是否修改包装
                bool isDelete = true;//上线包装

                #region 读取数据
                #region 读取Flow
                flow = ImportHelper.GetCellStringValue(row.GetCell(colFlow));
                if (string.IsNullOrWhiteSpace(flow))
                {
                    businessException.AddMessage(string.Format("第{0}行:路线不能为空。", rowCount));
                    continue;
                }
                else
                {
                    flowMaster = allFlowMaster.FirstOrDefault(f => f.Code == flow);
                    if (flowMaster==null)
                    {
                        businessException.AddMessage(string.Format("第{0}行:导入路线{1}不存在系统中，请检查数据的准确性", rowCount, flow));
                        continue;
                    }
                }

                #endregion

                #region Item
                item = ImportHelper.GetCellStringValue(row.GetCell(colItem));
                if (string.IsNullOrWhiteSpace(item))
                {
                    businessException.AddMessage(string.Format("第{0}行:物料编号不能为空。", rowCount));
                    continue;
                }
                IList<Item> itemList = genericMgr.FindAll<Item>(" from Item as i  where i.Code=?", item);
                if (itemList.Count <= 0)
                {
                    businessException.AddMessage(string.Format("第{0}行:导入物料代码{1}不存在系统中，请检查数据的准确性", rowCount, item));
                    continue;
                }
                #endregion

                var e = exactFlowDetailList.Where(p => p.Flow == flow && p.Item == item);
                //文件内也不能出现重复物料
                if (e.Count() > 0)
                {
                    businessException.AddMessage(string.Format("第{2}行：路线{0}存在相同的物料代码{1}超过1行，请检查数据的准确性", flow, item, rowCount));
                    continue;
                }

                #region 读取删除标示
                string isDeleteRead = ImportHelper.GetCellStringValue(row.GetCell(colIsDelete));
                if (isDeleteRead == null || isDeleteRead.Trim() == string.Empty)
                {
                    businessException.AddMessage(string.Format("第{0}行:删除标识不能为空。", rowCount));
                    continue;
                    //rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineFieldCanNotBeNull, colCount.ToString(), Resources.SCM.FlowMaster.FlowMaster_IsActive));
                }
                else
                {
                    switch (isDeleteRead.ToUpper())
                    {
                        case "Y":
                            isDelete = true; //新增
                            break;
                        case "N":
                            isDelete = false;//删除
                            break;
                        default:
                            businessException.AddMessage(string.Format("第{0}行:删除标识{1}不正确。", rowCount, isDeleteRead));
                            break;
                    }
                }
                #endregion

                if (!isDelete)
                {
                   #region 删除
                    var isExists = allFlowDet.Where(a => a.Flow == flow && a.Item == item);
                    if (isExists != null && isExists.Count() > 0)
                    {
                        exactFlowDetailList.Add(isExists.First());
                        continue;
                    }
                    else
                    {
                        businessException.AddMessage(string.Format("第{0}行:路线代码{1}+物料{2}不存在,请确认。", rowCount, flow, item));
                        continue;
                    }
                    #endregion
                }
                else
                {

                    #region 单包装
                    string ucRead = ImportHelper.GetCellStringValue(row.GetCell(colUnitCount));
                    //if (string.IsNullOrWhiteSpace(ucRead))
                    //{
                    //    businessException.AddMessage(string.Format("第{0}行:单包装不能为空。", rowCount));
                    //    continue;
                    //}
                    //else
                    //{
                    //    double.TryParse(ucRead, out unitCount);
                    //    if (unitCount <= 0)
                    //    {
                    //        businessException.AddMessage(string.Format("第{0}行:单包装{1}填写有误。", rowCount, ucRead));
                    //        continue;
                    //    }
                    //}
                    if (!string.IsNullOrWhiteSpace(ucRead))
                    {
                        if (!double.TryParse(ucRead, out unitCount))
                        {
                            businessException.AddMessage(string.Format("第{0}行:单包装{1}填写有误。", rowCount, ucRead));
                            continue;
                        }
                    }
                    #endregion
                    #region 包装描述

                    unitCountDescription = ImportHelper.GetCellStringValue(row.GetCell(colUnitCountDescription));
                    //if (string.IsNullOrWhiteSpace(unitCountDescription))
                    //{
                    //    businessException.AddMessage(string.Format("第{0}行:包装描述不能为空。", rowCount));
                    //}
                    #endregion
                    #region 容器描述
                    containerDescription = ImportHelper.GetCellStringValue(row.GetCell(colContainerDescription));
                    //if (string.IsNullOrWhiteSpace(containerDescription))
                    //{
                    //    businessException.AddMessage(string.Format("第{0}行:容器描述不能为空。", rowCount));
                    //}
                    #endregion
                    #region 是否允许修改包装
                    changeUnitCount = ImportHelper.GetCellStringValue(row.GetCell(colIsChangeUnitCount));
                    //if (string.IsNullOrWhiteSpace(changeUnitCount))
                    //{
                    //    businessException.AddMessage(string.Format("第{0}行:是否允许修改包装不能为空。", rowCount));
                    //    continue;
                    //}
                    if (!string.IsNullOrWhiteSpace(changeUnitCount))
                    {
                        try
                        {
                            Convert.ToBoolean(changeUnitCount);
                        }
                        catch (Exception ex)
                        {
                            businessException.AddMessage(string.Format("第{0}行:导入是否允许修改包装错误，只能导入True，False。", rowCount));
                            continue;
                        }
                    }
                    #endregion


                    FlowDetail flowDetail = new FlowDetail();
                    var flowdets = this.genericMgr.FindAll<FlowDetail>(" select d from FlowDetail as d where d.Item=? and d.Flow=? ", new object[] { item, flow }, new IType[] { NHibernate.NHibernateUtil.String, NHibernate.NHibernateUtil.String });
                    if (flowdets != null && flowdets.Count > 0)
                    {
                        flowDetail = flowdets.First();
                        flowDetail.IsUpdate = true;
                    }
                    flowDetail.Flow = flow;
                    flowDetail.Item = item;
                    if (flowDetail.IsUpdate)
                    {
                        flowDetail.UnitCount = unitCount != 0 ? Convert.ToDecimal(unitCount) : flowDetail.UnitCount;
                        flowDetail.UnitCountDescription = !string.IsNullOrWhiteSpace(unitCountDescription) ? unitCountDescription : flowDetail.UnitCountDescription;
                        flowDetail.ContainerDescription = !string.IsNullOrWhiteSpace(containerDescription) ? containerDescription : flowDetail.ContainerDescription;
                        flowDetail.IsChangeUnitCount = !string.IsNullOrWhiteSpace(changeUnitCount) ? Convert.ToBoolean(changeUnitCount) : flowDetail.IsChangeUnitCount;
                    }
                    else
                    {
                        if (unitCount == 0)
                        { 
                            businessException.AddMessage(string.Format("第{0}行:新增行单包装不能为空。", rowCount));
                            continue;
                        }
                        if (string.IsNullOrWhiteSpace(unitCountDescription))
                        {
                            businessException.AddMessage(string.Format("第{0}行:新增行包装描述不能为空。", rowCount));
                            continue;
                        }
                        if (string.IsNullOrWhiteSpace(containerDescription))
                        {
                            businessException.AddMessage(string.Format("第{0}行:新增行容器描述不能为空。", rowCount));
                            continue;
                        }
                        if (string.IsNullOrWhiteSpace(changeUnitCount))
                        {
                            businessException.AddMessage(string.Format("第{0}行:是否允许修改包装不能为空。", rowCount));
                            continue;
                        }
                        flowDetail.UnitCount =  Convert.ToDecimal(unitCount);
                        flowDetail.UnitCountDescription =  unitCountDescription; 
                        flowDetail.ContainerDescription =  containerDescription;
                    }
                    flowDetail.IsChangeUnitCount = Convert.ToBoolean(changeUnitCount);
                    //flowDetail.MinUnitCount = Convert.ToDecimal(minUnitCount);
                    flowDetail.ItemDescription = itemList[0].Description;
                    flowDetail.Container = itemList[0].Container;
                    flowDetail.MinUnitCount = itemList[0].MinUnitCount;
                    flowDetail.Uom = itemList[0].Uom;
                    flowDetail.BaseUom = itemList[0].Uom;
                    flowDetail.ReferenceItemCode = itemList[0].ReferenceCode;
                    flowDetail.IsActive = true;
                    flowDetail.PartyFrom = flowMaster.PartyFrom;
                    flowDetail.LocationTo = flowMaster.LocationTo;

                    #region 来源区域 目的库位 物料 不能在路线中存在
                    //int checkeSame = this.genericMgr.FindAllWithNativeSql<int>(" select COUNT(*) as countSum  from SCM_FlowDet as d where d.Item=? and exists( select 1 from SCM_FlowMstr as m where m.Code=d.Flow and m.PartyFrom=? and m.LocTo=? and m.Type=1  ) and d.Id <> ?", new object[] { item, flowMaster.PartyFrom, flowMaster.LocationTo, flowDetail.Id })[0];
                    //if (checkeSame > 0)
                    //{
                    //    businessException.AddMessage(string.Format("第{0}行:来源区域{1}+物料{2}+目的库位{3}已经存在数据库。", rowCount, flowMaster.PartyFrom, item, flowMaster.LocationTo));
                    //    continue;
                    //}

                    //if (exactFlowDetailList.Where(ef => ef.PartyFrom == flowMaster.PartyFrom && ef.LocationTo == flowMaster.LocationTo && ef.Item == flowDetail.Item && ef.Id != flowDetail.Id && (ef.Id == 0 || ef.IsUpdate)).Count() > 0)
                    //{
                    //    businessException.AddMessage(string.Format("第{0}行:来源区域{1}+物料{2}+目的库位{3}在模板中重复。", rowCount, flowMaster.PartyFrom, item, flowMaster.LocationTo));
                    //    continue;
                    //}

                    #endregion 
                    exactFlowDetailList.Add(flowDetail);

                }


                ////更新物料主数据以及物料所在所有路线的上线包装 2012-11-20 张敏提出
                //Item newItem = genericMgr.FindAll<Item>(" from Item as i where i.Code=?", itemList[0].Code)[0];
                //newItem.MinUnitCount = Convert.ToDecimal(minUnitCount);
                //genericMgr.Update(newItem);

                #endregion
            }

            if (businessException.HasMessage)
            {
                throw businessException;
            }

            if (exactFlowDetailList != null && exactFlowDetailList.Count > 0 )
            {
                foreach (FlowDetail flowDet in exactFlowDetailList)
                {
                    try
                    {
                        if (flowDet.Id == 0)
                        {
                            this.CreateFlowDetail(flowDet);
                        }
                        else if (flowDet.IsUpdate)
                        {
                            this.UpdateFlowDetail(flowDet);
                        }
                        else
                        {
                            this.genericMgr.Delete(flowDet);
                        }
                        this.genericMgr.FlushSession();
                    }
                    catch (Exception ex)
                    {
                        businessException.AddMessage(ex.Message);
                    }
                    //genericMgr.Update("Update from FlowDetail set MinUnitCount = ? where Item = ?", new object[] { flowDet.MinUnitCount, flowDet.Item });
                }
            }
            else
            {
                throw new BusinessException(string.Format("有效的数据行为0，可能是模板问题"));
              }

        }
        #endregion

        #region 移库路线明细批倒
        [Transaction(TransactionMode.Requires)]
        public void BatchTransferDetailXls(Stream inputStream)
        {
            if (inputStream.Length == 0)
            {
                throw new BusinessException("Import.Stream.Empty");
            }

            HSSFWorkbook workbook = new HSSFWorkbook(inputStream);

            ISheet sheet = workbook.GetSheetAt(0);
            IEnumerator rows = sheet.GetRowEnumerator();

            ImportHelper.JumpRows(rows, 10);
            BusinessException businessException = new BusinessException();
            #region 列定义

            int colFlow = 1;//路线代码
            int colItem = 2;// 物料代码
            int colSafeStock = 5;//安全库存
            int colMaxStock = 6;//最大库存
            int colRoundUpOption = 7;//圆整系数
            int colIsChangeUnitCount = 8;//是否修改包装
            int colIsDelete = 9;//删除
            int rowCount = 10;
            #endregion
            IList<FlowDetail> insertFlowDetailList = new List<FlowDetail>();
            IList<FlowDetail> updateFlowDetailList = new List<FlowDetail>();
            IList<FlowDetail> exactFlowDetailList = new List<FlowDetail>();
            IList<FlowMaster> allFlowMaster = this.genericMgr.FindAll<FlowMaster>();
            IList<FlowDetail> allFlowDet = this.genericMgr.FindEntityWithNativeSql<FlowDetail>("select d.* from SCM_FlowDet as d where exists( select 1 from SCM_FlowMstr as m where m.code=d.Flow and m.Type=2 )");
            while (rows.MoveNext())
            {
                rowCount++;
                HSSFRow row = (HSSFRow)rows.Current;
                if (!ImportHelper.CheckValidDataRow(row, 1, 8))
                {
                    break;//边界
                }
                FlowMaster flowMaster = null;
                string flow = string.Empty;//路线
                string item = string.Empty; ;// 物料
                decimal safeStock = 0;//单包装
                decimal maxStock = 0;//包装描述
                int roundUpOption = 0;//上线包装
                bool isDelete = true;//删除
                string changeUnitCount = string.Empty;//是否修改包装
                #region 读取数据

                #region 读取Flow
                flow = ImportHelper.GetCellStringValue(row.GetCell(colFlow));
                if (string.IsNullOrWhiteSpace(flow))
                {
                    businessException.AddMessage(string.Format("第{0}行:路线不能为空。", rowCount));
                    continue;
                }
                else
                {
                    flowMaster = allFlowMaster.FirstOrDefault(f => f.Code == flow);
                    if (flowMaster==null)
                    {
                        businessException.AddMessage(string.Format("第{0}行:导入路线{1}不存在系统中，请检查数据的准确性", rowCount, flow));
                        continue;
                    }
                }

                #endregion

                #region Item
                item = ImportHelper.GetCellStringValue(row.GetCell(colItem));
                if (string.IsNullOrWhiteSpace(item))
                {
                    businessException.AddMessage(string.Format("第{0}行:物料编号不能为空。", rowCount));
                    continue;
                }
                IList<Item> itemList = genericMgr.FindAll<Item>(" from Item as i  where i.Code=?", item);
                if (itemList.Count <= 0)
                {
                    businessException.AddMessage(string.Format("第{0}行:导入物料代码{1}不存在系统中，请检查数据的准确性", rowCount, item));
                    continue;
                }
                #endregion

                var e = exactFlowDetailList.Where(p => p.Flow == flow && p.Item == item);
                //文件内也不能出现重复物料
                if (e.Count() > 0)
                {
                    businessException.AddMessage(string.Format("第{2}行：路线{0}存在相同的物料代码{1}超过1行，请检查数据的准确性", flow, item, rowCount));
                    continue;
                }

                #region 读取删除标示
                string isDeleteRead = ImportHelper.GetCellStringValue(row.GetCell(colIsDelete));
                if (isDeleteRead == null || isDeleteRead.Trim() == string.Empty)
                {
                    businessException.AddMessage(string.Format("第{0}行:删除标识不能为空。", rowCount));
                    continue;
                    //rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineFieldCanNotBeNull, colCount.ToString(), Resources.SCM.FlowMaster.FlowMaster_IsActive));
                }
                else
                {
                    switch (isDeleteRead.ToUpper())
                    {
                        case "Y":
                            isDelete = true; //新增
                            break;
                        case "N":
                            isDelete = false;//删除
                            break;
                        default:
                            businessException.AddMessage(string.Format("第{0}行:删除标识{1}不正确。", rowCount, isDeleteRead));
                            break;
                    }
                }
                #endregion


                if (!isDelete)
                {
                    #region 删除
                    var isExists = allFlowDet.Where(a => a.Flow == flow && a.Item == item);
                    if (isExists != null && isExists.Count() > 0)
                    {
                        exactFlowDetailList.Add(isExists.First());
                        continue;
                    }
                    else
                    {
                        businessException.AddMessage(string.Format("第{0}行:路线代码{1}+物料{2}找不到对应的数据。", rowCount, flow, item));
                        continue;
                    }
                    #endregion
                }
                else
                {

                    #region 安全库存
                    //string readSafe = ImportHelper.GetCellStringValue(row.GetCell(colSafeStock));
                    //if (!string.IsNullOrWhiteSpace(readSafe))
                    //{
                    //    decimal.TryParse(readSafe, out safeStock);
                    //    if (safeStock < 0)
                    //    {
                    //        businessException.AddMessage(string.Format("第{0}行:安全库存{1}填写有误。", rowCount, safeStock));
                    //        continue;
                    //    }
                    //}
                    #endregion

                    #region 最大库存
                    //string readMax = ImportHelper.GetCellStringValue(row.GetCell(colMaxStock));
                    //if (!string.IsNullOrWhiteSpace(readMax))
                    //{
                    //    decimal.TryParse(readMax, out maxStock);
                    //    if (maxStock < 0)
                    //    {
                    //        businessException.AddMessage(string.Format("第{0}行:最大库存{1}填写有误。", rowCount, maxStock));
                    //        continue;
                    //    }
                    //}
                    #endregion

                    #region 圆整系数
                    string readRound = ImportHelper.GetCellStringValue(row.GetCell(colRoundUpOption));
                    if (!string.IsNullOrWhiteSpace(readRound))
                    {
                        switch (readRound)
                        {
                            case "0":
                                roundUpOption = 0;
                                break;
                            case "1":
                                roundUpOption = 1;
                                break;
                            case "2":
                                roundUpOption = 2;
                                break;
                            default:
                                businessException.AddMessage(string.Format("第{0}行:圆整系数{1}填写有误。", rowCount, readRound));
                                break;
                        }
                    }
                    else
                    {
                        businessException.AddMessage(string.Format("第{0}行:圆整系数不能为空。", rowCount));
                        continue;
                    }
                    #endregion

                    #region 是否允许修改包装
                    changeUnitCount = ImportHelper.GetCellStringValue(row.GetCell(colIsChangeUnitCount));
                    if (string.IsNullOrWhiteSpace(changeUnitCount))
                    {
                        businessException.AddMessage(string.Format("第{0}行:是否允许修改包装不能为空。", rowCount));
                        continue;
                    }
                    try
                    {
                        Convert.ToBoolean(changeUnitCount);
                    }
                    catch (Exception ex)
                    {
                        businessException.AddMessage(string.Format("第{0}行:导入是否允许修改包装错误，只能导入True，False。", rowCount));
                        continue;
                    }
                    #endregion



                    FlowDetail flowDetail = new FlowDetail();
                    var flowdets = allFlowDet.Where(a => a.Item == item && a.Flow == flow).ToList();
                        //this.genericMgr.FindAll<FlowDetail>(" select d from FlowDetail as d where d.Item=? and d.Flow=? ", new object[] { item, flow }, new IType[] { NHibernate.NHibernateUtil.String, NHibernate.NHibernateUtil.String });
                    if (flowdets != null && flowdets.Count > 0)
                    {
                        flowDetail = flowdets.First();
                        flowDetail.IsUpdate = true;
                    }
                    flowDetail.Flow = flow;
                    flowDetail.Item = item;
                    flowDetail.ItemDescription = itemList[0].Description;
                    flowDetail.ReferenceItemCode = itemList[0].ReferenceCode;
                    //flowDetail.SafeStock = safeStock;
                    //flowDetail.MaxStock = maxStock;
                    flowDetail.RoundUpOption = (CodeMaster.RoundUpOption)roundUpOption;
                    flowDetail.IsChangeUnitCount = Convert.ToBoolean(changeUnitCount);
                    flowDetail.IsActive = true;
                    //if (flowDetail.Id == 0)
                    //{
                    flowDetail.Container = itemList[0].Container;
                    flowDetail.MinUnitCount = itemList[0].MinUnitCount;
                    flowDetail.Uom = itemList[0].Uom;
                    flowDetail.BaseUom = itemList[0].Uom;
                    flowDetail.PartyFrom = flowMaster.PartyFrom;
                    flowDetail.LocationTo = flowMaster.LocationTo;

                    //}
                    //if (allFlowDet.Where(ef => ef.PartyFrom == flowMaster.PartyFrom && ef.LocationTo == flowMaster.LocationTo && ef.Item == flowDetail.Item && ef.Id != flowDetail.Id).Count() > 0)
                    //{
                    //    businessException.AddMessage(string.Format("第{0}行:来源区域{1}+物料{2}+目的库位{3}已经存在数据库。", rowCount, flowMaster.PartyFrom, item, flowMaster.LocationTo));
                    //    continue;
                    //}

                    #region 来源区域 目的库位 物料 不能在移库路线中存在
                    int checkeSame = this.genericMgr.FindAllWithNativeSql<int>(" select COUNT(*) as countSum  from SCM_FlowDet as d where d.Item=? and exists( select 1 from SCM_FlowMstr as m where m.Code=d.Flow and m.PartyFrom=? and m.PartyTo=? and m.Type=2  ) and d.Id <> ?", new object[] { item, flowMaster.PartyFrom, flowMaster.PartyTo, flowDetail.Id })[0];
                    if (checkeSame > 0)
                    {
                        businessException.AddMessage(string.Format("第{0}行:来源区域{1}+物料{2}+目的区域{3}已经存在数据库。", rowCount, flowMaster.PartyFrom, item, flowMaster.PartyTo));
                        continue;
                    }

                    if (exactFlowDetailList.Where(ef => ef.PartyFrom == flowMaster.PartyFrom && ef.PartyTo == flowMaster.PartyTo && ef.Item == flowDetail.Item && ef.Id != flowDetail.Id && (ef.Id == 0 || ef.IsUpdate)).Count() > 0)
                    {
                        businessException.AddMessage(string.Format("第{0}行:来源区域{1}+物料{2}+目的区域{3}在模板中重复。", rowCount, flowMaster.PartyFrom, item, flowMaster.PartyTo));
                        continue;
                    }

                    #endregion 
                    exactFlowDetailList.Add(flowDetail);
                }
               



                //var flowdets = allFlowDet.Where(a => a.Item == item && a.Flow == flow).ToList();
                ////this.genericMgr.FindAll<FlowDetail>(" select d from FlowDetail as d where d.Item=? and d.Flow=? ", new object[] { item, flow }, new IType[] { NHibernate.NHibernateUtil.String, NHibernate.NHibernateUtil.String });
                //if (flowdets != null && flowdets.Count > 0)
                //{
                //    flowDetail = flowdets.First();
                //    flowDetail.IsUpdate = true;
                //}
                #endregion


            }

            if (businessException.HasMessage)
            {
                throw businessException;
            }

            if (exactFlowDetailList != null && exactFlowDetailList.Count > 0 )
            {
                foreach (FlowDetail flowDet in exactFlowDetailList)
                {
                    try
                    {
                        if (flowDet.Id == 0)
                        {
                            this.CreateFlowDetail(flowDet);
                        }
                        else if (flowDet.IsUpdate)
                        {
                            this.UpdateFlowDetail(flowDet);
                        }
                        else
                        {
                            this.genericMgr.Delete(flowDet);
                        }
                        this.genericMgr.FlushSession();
                    }
                    catch (Exception ex)
                    {
                        businessException.AddMessage(ex.Message);
                    }
                    //genericMgr.Update("Update from FlowDetail set MinUnitCount = ? where Item = ?", new object[] { flowDet.MinUnitCount, flowDet.Item });
                }
            }
            else
            {
                throw new BusinessException(string.Format("有效的数据行为0，可能是模板问题"));
            }

        }
        #endregion

        /// <summary>
        /// 看板路线导入
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="type">采购/移库</param>
        [Transaction(TransactionMode.Requires)]
        public void ImportKanBanFlow(Stream inputStream, com.Sconit.CodeMaster.OrderType type)
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
            // FlowMaster
            int colCode = 1; // 路线代码
            int colItem = 2; // 物料号										
            int colRefItemCode = 3; // 旧图号
            int colItemDesc = 4; // 零件名称
            int colOpRef = 5; // 工位代码
            int colOpUseQty = 6; // 工位用量
            int colItemType = 7; // 零件类型
            int colOpRefSeq = 8; // 看板编号
            int colUC = 9; // 单包装
            int colContainer = 10; // 容器代码

            int colContainerDesc = 11; // 容器描述
            int colDock = 12; // 配送路径
            int colCardQty = 13; // 看板卡数量
            int colIsActive = 14; // 是否生效
            int colOpRefDesc = 15; //工位中文描述
            //int colShelf = 13; // 架位
            #endregion

            var errorMessage = new BusinessException();

            IList<Item> ItemsList = new List<Item>();
            IList<FlowMaster> exactFlowList = new List<FlowMaster>();
            IList<FlowStrategy> exactFlowStrategyList = new List<FlowStrategy>();
            IList<FlowDetail> exactFlowDetailList = new List<FlowDetail>();
            IList<FlowDetail> deleteFlowDetailList = new List<FlowDetail>();
            int colCount = 10;

            #region 取所有的路线和路线明细
            var totalFlowMasterList = genericMgr.FindAll<FlowMaster>("from FlowMaster f where f.Type = ? and exists(select 1 from FlowStrategy as fs where fs.Flow=f.Code and fs.Strategy=? )", new object[] { (int)type, (int)CodeMaster.FlowStrategy.ANDON });
            var totalFlowDetailList = genericMgr.FindEntityWithNativeSql<FlowDetail>("select d.* from scm_FlowDet d inner join scm_FlowMstr f on f.Code = d.Flow where f.Type = ? and exists(select 1 from SCM_FlowStrategy as fs where fs.Flow=f.Code and fs.Strategy=?)", new object[] { (int)type, (int)CodeMaster.FlowStrategy.ANDON });
            //var totalFlowStrategyList = genericMgr.FindEntityWithNativeSql<FlowStrategy>("select g.* from scm_FlowStrategy g inner join scm_FlowMstr f on f.Code = g.Flow and f.Type = ? and f.FlowStrategy = ?", new object[] { (int)type, (int)CodeMaster.FlowStrategy.ANDON });
            //var totalSupplierList = genericMgr.FindAll<Supplier>();
            var totalRegionList = genericMgr.FindAll<Region>();
            var totalLocationList = genericMgr.FindAll<Location>();
            //var totalItemList = genericMgr.FindAll<Item>("from Item where IsActive = ?", true);
            #endregion

            while (rows.MoveNext())
            {
                HSSFRow row = (HSSFRow)rows.Current;
                if (!ImportHelper.CheckValidDataRow(row, 1, 14))
                {
                    break;//边界
                }
                colCount++;

                var rowErrors = new List<Message>();

                string code = string.Empty;
                string item = string.Empty;
                string refItemCode = string.Empty;
                string itemDesc = string.Empty;
                string opRef = string.Empty;
                string opUseQty = string.Empty;
                string itemType = string.Empty;
                string opRefSeq = string.Empty;
                string uc = string.Empty;
                string container = string.Empty;
                string dock = string.Empty;
                string containerDesc = string.Empty;
                string cardQty = string.Empty;
                string isActive = string.Empty;
                string opRefDesc = string.Empty;


                #region 读取数据
                //路线
                code = ImportHelper.GetCellStringValue(row.GetCell(colCode));
                if (code == null || code.Trim() == string.Empty)
                {
                    rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineFieldCanNotBeNull, colCount.ToString(), Resources.SCM.FlowMaster.FlowMaster_Code));
                }

                //工位
                opRef = ImportHelper.GetCellStringValue(row.GetCell(colOpRef));
                if (opRef == null || opRef.Trim() == string.Empty)
                {
                    rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineFieldCanNotBeNull, colCount.ToString(), "工位"));
                }

                //零件类型
                itemType = ImportHelper.GetCellStringValue(row.GetCell(colItemType));

                //工位
                opRefSeq = ImportHelper.GetCellStringValue(row.GetCell(colOpRefSeq));
                if (opRefSeq == null || opRefSeq.Trim() == string.Empty)
                {
                    rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineFieldCanNotBeNull, colCount.ToString(), "看板号"));
                }

                uc = ImportHelper.GetCellStringValue(row.GetCell(colUC));
                //cardQty = ImportHelper.GetCellStringValue(row.GetCell(colCardQty));
                decimal ucVar = 0;
                if (uc == null || uc.Trim() == string.Empty || !decimal.TryParse(uc, out ucVar))
                {
                    rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineFieldCanNotBeNull, colCount.ToString(), "单包装数量"));
                }

                //容器代码
                container = ImportHelper.GetCellStringValue(row.GetCell(colContainer));
                if (container == null || container.Trim() == string.Empty)
                {
                    rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineFieldCanNotBeNull, colCount.ToString(), "容器代码"));
                }

                //容器描述
                containerDesc = ImportHelper.GetCellStringValue(row.GetCell(colContainerDesc));

                dock = ImportHelper.GetCellStringValue(row.GetCell(colDock));

                item = ImportHelper.GetCellStringValue(row.GetCell(colItem));
                if (item == null || item.Trim() == string.Empty)
                {
                    rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineFieldCanNotBeNull, colCount.ToString(), Resources.SCM.FlowDetail.FlowDetail_Item));
                }



                opRefDesc = ImportHelper.GetCellStringValue(row.GetCell(colOpRefDesc));

                //containerDesc = ImportHelper.GetCellStringValue(row.GetCell(colContainerDesc));

                //dock = ImportHelper.GetCellStringValue(row.GetCell(colDock));

                isActive = ImportHelper.GetCellStringValue(row.GetCell(colIsActive));
                var isActiveVal = false;
                if (isActive == null || isActive.Trim() == string.Empty)
                {
                    rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineFieldCanNotBeNull, colCount.ToString(), Resources.SCM.FlowMaster.FlowMaster_IsActive));
                }
                else
                {
                    switch (isActive.ToUpper())
                    {
                        case "Y":
                            isActiveVal = true;
                            break;
                        case "N":
                            isActiveVal = false;
                            break;
                        default:
                            rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineBooleanFieldValueError, colCount.ToString(), Resources.SCM.FlowMaster.FlowMaster_IsActive, isActive));
                            break;
                    }
                }
                #endregion

                #region 验证
                if (totalFlowMasterList.FirstOrDefault(c => c.Code.ToUpper() == code.ToUpper()) == null)
                {
                    rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineError, colCount.ToString(), string.Format("路线头{0}不存在，请确认。", code)));
                }

                //物料编号数据存在性
                Item itemInstance = null;
                if (!string.IsNullOrEmpty(item))
                {
                    itemInstance = ValidatItem(ItemsList, item, colCount, rowErrors);
                }

                //数据库中重复性验证
                FlowDetail sameFlowDet = null;
                if (itemInstance != null)
                {
                    var dbFlowDet = totalFlowDetailList.FirstOrDefault(c => c.Flow.ToUpper() == code.ToUpper() && c.Item.ToUpper() == item.ToUpper() && (c.BinTo==null && string.IsNullOrWhiteSpace(opRef) || c.BinTo!=null && c.BinTo.ToUpper() == opRef.ToUpper()));
                    //if (dbFlowDet != null && (dbFlowDet.BinTo != null && dbFlowDet.BinTo.Equals(opRef, StringComparison.OrdinalIgnoreCase)))
                    if (dbFlowDet != null)
                    {
                        sameFlowDet = dbFlowDet;
                    }
                }
                
                FlowMaster flowMaster = this.genericMgr.FindById<FlowMaster>(code);
                
              

                #endregion

                #region Instance



                if (itemInstance != null)
                {
                    if (sameFlowDet != null)
                    {
                        sameFlowDet.ItemType = itemType;
                        //sameFlowDet.ContainerDescription = containerDesc;
                        sameFlowDet.BinTo = opRef;
                        sameFlowDet.OprefSequence = opRefSeq;
                        sameFlowDet.Dock = dock;
                        sameFlowDet.ProductionScan = opRefDesc;
                        sameFlowDet.Container = container;
                        sameFlowDet.ContainerDescription = containerDesc;
                        sameFlowDet.MinUnitCount = ucVar;
                        sameFlowDet.PartyFrom = flowMaster.PartyFrom;
                        sameFlowDet.LocationTo = flowMaster.LocationTo;
                        sameFlowDet.FlowMasterStrategy = (int)flowMaster.FlowStrategy;
                        //sameFlowDet.BinTo = binTo;
                        sameFlowDet.IsCreate = false;
                        //sameFlowDet.IsRejectInspect = isRejectInspectVal;
                        //模板中 路线代码+物料编号+工位 不能重复
                        if (exactFlowDetailList.Where(f => f.Flow.ToUpper() == code.ToUpper() && f.BinTo.ToUpper() == opRef.ToUpper() && f.Item.ToUpper() == item.ToUpper()).Count() > 0
                         || deleteFlowDetailList.Where(f => f.Flow.ToUpper() == code.ToUpper() && f.BinTo.ToUpper() == opRef.ToUpper() && f.Item.ToUpper() == item.ToUpper()).Count() > 0)
                        {
                            rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineError, colCount.ToString(), string.Format("路线头{0}+工位{1}+物料编号{2}在模板中重复，请确认。", code, opRef, item)));
                        }

                        if (isActiveVal)
                        {
                            //sameFlowMaster.AddFlowDetail(sameFlowDet);
                            #region 来源区域 目的库位 物料 不能再不同的策略上出现
                            int checkeSame = this.genericMgr.FindAllWithNativeSql<int>(" select COUNT(*) as countSum  from SCM_FlowDet as d where d.Item=? and exists( select 1 from SCM_FlowMstr as m where m.Code=d.Flow and m.PartyFrom=? and m.LocTo=? and m.Type=2 and not exists(select 1 from SCM_FlowStrategy as fs where fs.Flow=m.Code and fs.Strategy=7) ) ", new object[] { item, flowMaster.PartyFrom, flowMaster.LocationTo })[0];
                            if (checkeSame > 0)
                            {
                                rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineError, colCount.ToString(), (string.Format("来源区域{0}+物料{1}+目的库位{2}+在其他策略中已经存在。", flowMaster.PartyFrom, item, flowMaster.LocationTo, sameFlowDet.FlowMasterStrategy))));
                            }
                            if (exactFlowDetailList.Where(ef => ef.PartyFrom == flowMaster.PartyFrom && ef.LocationTo == flowMaster.LocationTo && ef.Item == item && ef.Id != sameFlowDet.Id && ef.FlowMasterStrategy != sameFlowDet.FlowMasterStrategy).Count() > 0)
                            {
                                rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineError, colCount.ToString(), (string.Format("来源区域{0}+物料{1}+目的库位{2}+策略不同{3}在模板中重复。", flowMaster.PartyFrom, item, flowMaster.LocationTo, sameFlowDet.FlowMasterStrategy))));
                            }
                            #endregion

                            exactFlowDetailList.Add(sameFlowDet);
                        }
                        else
                        {
                            deleteFlowDetailList.Add(sameFlowDet);
                        }

                    }
                    else
                    {
                        //if (exactFlowDetailList.FirstOrDefault(f => f.Flow.ToUpper() == code.ToUpper() && f.BinTo.ToUpper() == opRef.ToUpper() && f.Item.ToUpper() == item.ToUpper()) == null)
                        //{
                            sameFlowDet = new FlowDetail
                            {
                                Flow = code,
                                Item = itemInstance.Code,
                                ItemDescription = itemInstance.Description,
                                BaseUom = itemInstance.Uom,
                                Uom = itemInstance.Uom,
                                ReferenceItemCode = itemInstance.ReferenceCode,
                                UnitCount = itemInstance.UnitCount,
                                MinUnitCount = ucVar,
                                //UnitCountDescription = ucDesc,
                                Container = container,
                                ContainerDescription = containerDesc,
                                //SafeStock = safeStockVal,
                                ItemType = itemType,
                                Dock = dock,
                                OprefSequence = opRefSeq,
                                BinTo = opRef,
                                IsCreate = true,
                                PartyFrom = flowMaster.PartyFrom,
                                LocationTo = flowMaster.LocationTo,
                                FlowMasterStrategy = (int)flowMaster.FlowStrategy,
                                ProductionScan = opRefDesc,
                                //IsRejectInspect = isRejectInspectVal
                            };
                            //sameFlowMaster.AddFlowDetail(sameFlowDet);
                            #region 来源区域 目的库位 物料 不能再不同的策略上出现
                            int checkeSame = this.genericMgr.FindAllWithNativeSql<int>(" select COUNT(*) as countSum  from SCM_FlowDet as d where d.Item=? and exists( select 1 from SCM_FlowMstr as m where m.Code=d.Flow and m.PartyFrom=? and m.LocTo=? and m.Type=2 and exists(select 1 from SCM_FlowStrategy as fs where fs.Flow=m.Code and fs.Strategy<>7)) ", new object[] { item, flowMaster.PartyFrom, flowMaster.LocationTo })[0];
                            if (checkeSame > 0)
                            {
                                rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineError, colCount.ToString(), (string.Format("来源区域{0}+物料{1}+目的库位{2}+在其他策略中已经存在", flowMaster.PartyFrom, item, flowMaster.LocationTo, sameFlowDet.FlowMasterStrategy))));
                            }
                            if (exactFlowDetailList.Where(ef => ef.PartyFrom == flowMaster.PartyFrom && ef.LocationTo == flowMaster.LocationTo && ef.Item == item && ef.Id != sameFlowDet.Id && ef.FlowMasterStrategy != sameFlowDet.FlowMasterStrategy).Count() > 0)
                            {
                                rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineError, colCount.ToString(), (string.Format("来源区域{0}+物料{1}+目的库位{2}+策略不同{3}在模板中重复。", flowMaster.PartyFrom, item, flowMaster.LocationTo, sameFlowDet.FlowMasterStrategy))));
                            }
                            #endregion
                            //模板中 路线代码+物料编号+工位 不能重复
                            if (exactFlowDetailList.Where(f => f.Flow.ToUpper() == code.ToUpper() && f.BinTo.ToUpper() == opRef.ToUpper() && f.Item.ToUpper() == item.ToUpper()).Count() > 0
                             || deleteFlowDetailList.Where(f => f.Flow.ToUpper() == code.ToUpper() && f.BinTo.ToUpper() == opRef.ToUpper() && f.Item.ToUpper() == item.ToUpper()).Count() > 0)
                            {
                                rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineError, colCount.ToString(), string.Format("路线头{0}+工位{1}+物料编号{2}在模板中重复，请确认", code, opRef, item)));
                            }
                            exactFlowDetailList.Add(sameFlowDet);
                        //}
                       
                    }

                }
                else
                {
                    rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineError, colCount.ToString(), string.Format("第{0}行：物料编号{1}不存在，请确认", code, opRef, item)));
                    
                }
                #endregion
               
                errorMessage.AddMessages(rowErrors);
            }

            if (errorMessage.HasMessage)
            {
                throw errorMessage;
            }
            if ((exactFlowDetailList == null || exactFlowDetailList.Count == 0) && (deleteFlowDetailList == null || deleteFlowDetailList.Count==0))
            {
                throw new BusinessException("模板为空，请确认。");
            }
            #region Save Data
            foreach (FlowDetail instance in exactFlowDetailList)
            {
                instance.IsActive = true;
                if (instance.IsCreate)
                {
                    CreateFlowDetail(instance);
                }
                else
                {
                    UpdateFlowDetail(instance);
                    this.genericMgr.UpdateWithNativeQuery("update KB_KanbanCard set Qty=?,Container=? where FlowDetId=?", 
                        new object[] { instance.MinUnitCount, instance.Container, instance.Id },
                        new IType[] { NHibernateUtil.Decimal, NHibernateUtil.String, NHibernateUtil.Int32 });
                }
                this.genericMgr.FlushSession();
            }
            foreach (FlowDetail instance in deleteFlowDetailList)
            {
                genericMgr.Delete(instance);
                genericMgr.Delete("from KanbanCard k where k.FlowDetailId = ?", instance.Id, NHibernate.NHibernateUtil.Int32);
            }
          
            #endregion
        }

        #region 验证物料编号存在性
        /// <summary>
        /// 验证物料编号存在性
        /// </summary>
        /// <param name="ItemList"></param>
        /// <param name="code">物料编号</param>
        /// <param name="colCount">行数</param>
        private Item ValidatItem(IList<Item> ItemList, string code, int colCount, List<Message> rowErrors)
        {
            var itemInstance = ItemList.FirstOrDefault(i => i.Code.ToUpper() == code);
            if (itemInstance == null)
            {
                var Items = this.genericMgr.FindAll<Item>("from Item as i where i.Code= ?", new object[] { code });
                if (Items == null || Items.Count == 0)
                {
                    rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineFieldValueNotExist, colCount.ToString(), Resources.SCM.FlowDetail.FlowDetail_Item, code));
                }
                else
                {
                    itemInstance = Items[0];
                    ItemList.Add(Items[0]);
                }
            }
            return itemInstance;
        }


        private Item ValidatItem(IList<Item> totalItemList, IList<Item> itemList, string code, int colCount, List<Message> rowErrors)
        {
            var itemInstance = itemList.FirstOrDefault(i => i.Code.ToUpper() == code.ToUpper());
            if (itemInstance == null)
            {
                itemInstance = totalItemList.Where(p => p.Code.ToUpper() == code.ToUpper()).FirstOrDefault();
                if (itemInstance == null)
                {
                    rowErrors.Add(new Message(com.Sconit.CodeMaster.MessageType.Error, Resources.ErrorMessage.Errors_Import_LineFieldValueNotExist, colCount.ToString(), Resources.SCM.FlowDetail.FlowDetail_Item, code));
                }
                else
                {
                    itemList.Add(itemInstance);
                }
            }
            return itemInstance;
        }
        #endregion
    }
}
