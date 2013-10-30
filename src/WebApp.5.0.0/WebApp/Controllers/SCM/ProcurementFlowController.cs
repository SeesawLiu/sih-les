using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using com.Sconit.Entity.SCM;
using com.Sconit.Service;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels.SCM;
using com.Sconit.Web.Util;
using Telerik.Web.Mvc;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.BIL;
using com.Sconit.Entity.SYS;
using com.Sconit.Utility;
using System;
using com.Sconit.Entity.Exception;

namespace com.Sconit.Web.Controllers.SCM
{
    public class ProcurementFlowController : WebAppBaseController
    {
        public IFlowMgr flowMgr { get; set; }
        public INumberControlMgr numberControlMgr { get; set; }

        private static string selectCountStatement = "select count(*) from FlowMaster as f ";
        private static string selectStatement = "select f from FlowMaster as f";

        private static string selectCountDetailStatement = "select count(*) from FlowDetail as f ";
        private static string selectDetailStatement = "select f from FlowDetail as f";

        private static string selectCountBindStatement = @"select count(*) 
                                                      from FlowBinding as f join f.MasterFlow as mf join f.BindedFlow as bf";
        private static string selectBindStatement = @"select f
                                                      from FlowBinding as f join f.MasterFlow as mf join f.BindedFlow as bf";
        //private static string userNameDuiplicateVerifyStatement = @"select count(*) from FlowMaster as u where u.Code = ?";
        private static string selectCountFlowShiftDetailStatement = "select count(*) from FlowShiftDetail as f ";

        private static string selectFlowShiftDetailStatement = "select f from FlowShiftDetail as f ";

        public ProcurementFlowController()
        {
        }

        [SconitAuthorize(Permissions = "Url_ProcurementFlow_View")]
        public ActionResult Index()
        {
            return View();
        }

        #region FlowMaster

        [GridAction]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_View")]
        public ActionResult List(GridCommand command, FlowSearchModel searchModel)
        {
            this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_View")]
        public ActionResult _AjaxList(GridCommand command, FlowSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<FlowMaster>(searchStatementModel, command));
        }

        [SconitAuthorize(Permissions = "Url_ProcurementFlow_Edit")]
        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_Edit")]
        public ActionResult New(FlowMaster flow)
        {
            if (ModelState.IsValid)
            {
                if (base.genericMgr.FindAll<long>("select count(*) from FlowMaster as f where f.Code = ?", flow.Code)[0] > 0)
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Existing_Code, flow.Code);
                }
                else if (string.IsNullOrEmpty(flow.PartyFrom))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_PartyFrom);
                }
                else if (string.IsNullOrEmpty(flow.PartyTo))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_PartyTo);
                }
                else if (string.IsNullOrEmpty(flow.LocationTo))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_LocationTo);
                }
                else if (string.IsNullOrEmpty(flow.ShipFrom))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_ShipFrom);
                }
                else if (string.IsNullOrEmpty(flow.ShipTo))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_ShipTo);
                }
                else if (string.IsNullOrEmpty(flow.BillAddress))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_BillAddress);
                }
                //else if (string.IsNullOrEmpty(flow.PriceList))
                //{
                //    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_PriceList);
                //}
                else
                {
                    flow.FlowStrategy = com.Sconit.CodeMaster.FlowStrategy.Manual;
                    flow.Type = com.Sconit.CodeMaster.OrderType.Procurement;
                    flowMgr.CreateFlow(flow);
                    SaveSuccessMessage(Resources.SCM.FlowMaster.FlowMaster_Added);
                    return RedirectToAction("Edit/" + flow.Code);
                }
            }
            return View(flow);
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_Edit")]
        public ActionResult Edit(string id, bool? isReturn)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            else
            {
                TempData["IsReturn"] = isReturn == null ? false : isReturn;
                if (isReturn.HasValue == true && isReturn == true)
                {
                    TempData["TabIndex"] = 2;
                }
                return View("Edit", string.Empty, id);
            }
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_Edit")]
        public ActionResult _Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            FlowMaster flow = base.genericMgr.FindById<FlowMaster>(id);
            return PartialView(flow);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_Edit")]
        public ActionResult _Edit(FlowMaster flow)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(flow.PartyFrom))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_PartyFrom);
                }
                else if (string.IsNullOrEmpty(flow.PartyTo))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_PartyTo);
                }
                else if (string.IsNullOrEmpty(flow.LocationTo))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_LocationTo);
                }
                else if (string.IsNullOrEmpty(flow.ShipFrom))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_ShipFrom);
                }
                else if (string.IsNullOrEmpty(flow.ShipTo))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_ShipTo);
                }
                else if (string.IsNullOrEmpty(flow.BillAddress))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_BillAddress);
                }
                //else if (string.IsNullOrEmpty(flow.PriceList))
                //{
                //    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_PriceList);
                //}
                else
                {
                    if (flow.Type == 0)
                    {
                        //base.SaveErrorMessage("路线类型不正确。");
                        flow.Type = CodeMaster.OrderType.Procurement;
                    }
                    base.genericMgr.Update(flow);
                    SaveSuccessMessage(Resources.SCM.FlowMaster.FlowMaster_Updated);
                    //flow.Type = com.Sconit.CodeMaster.OrderType.Procurement;
                    //FlowMaster oldFlow = base.genericMgr.FindById<FlowMaster>(flow.Code);

                    ////更新flow时要去判断是否更新过目的库位或者有效标志
                    ////这两个数据更新的话要影响订单
                    //if (flow.LocationTo != oldFlow.LocationTo || flow.IsActive != oldFlow.IsActive)
                    //{
                    //    string errorMessage = string.Empty;
                    //    //仅仅是库位的更新
                    //    if (flow.IsActive == oldFlow.IsActive)
                    //    {
                    //        Location location = base.genericMgr.FindById<Location>(flow.LocationTo);
                    //        Party partyTo = base.genericMgr.FindById<Party>(flow.PartyTo);
                    //        Address address = base.genericMgr.FindById<Address>(flow.ShipTo);

                    //        string locationName = base.genericMgr.FindById<Location>(flow.LocationTo).Name;
                    //        string updateMstr = "Update Ord_OrderMstr_8 set PartyTo = ?,PartyToNm = ?,ShipTo = ?,ShipToAddr = ?,LocTo = ?,LocToNm = ? where  Flow=?";
                    //        base.genericMgr.FindAllWithNativeSql(updateMstr, new object[] { partyTo.Code, partyTo.Name, flow.ShipTo, address.AddressContent, flow.LocationTo, locationName, flow.Code });
                    //        string updateStr = "Update Ord_OrderDet_8 set LocTo = ?,LocToNm = ? where  exists (select 1 from Ord_OrderMstr_8 as o where o.Flow = ? and Ord_OrderDet_8.OrderNo=o.OrderNo)";
                    //        base.genericMgr.FindAllWithNativeSql(updateStr, new object[] { flow.LocationTo, locationName, flow.Code });

                    //        base.genericMgr.Update(flow);

                    //        SaveSuccessMessage(Resources.SCM.FlowMaster.FlowMaster_Updated);
                    //    }
                    //    //如果有效标志有更新
                    //    else
                    //    {
                    //        //先判断是更新成有效还是无效
                    //        //如果更新成无效
                    //        if (!flow.IsActive)
                    //        {
                    //            string locationName = base.genericMgr.FindById<Location>(flow.LocationTo).Name;
                    //            string updateMstr = "Update Ord_OrderMstr_8 set Flow = null where Flow=?";
                    //            base.genericMgr.FindAllWithNativeSql(updateMstr, new object[] { flow.Code });

                    //            base.genericMgr.Update(flow);

                    //            SaveSuccessMessage(Resources.SCM.FlowMaster.FlowMaster_Updated);
                    //        }
                    //        //如果更新成有效，则先判断供应商+物料的有效性是否和其他路线有冲突
                    //        //有冲突的返回错误，没有冲突将同步更新订单和订单明细
                    //        else
                    //        {
                    //            //获取目前所有有效的采购路线明细
                    //            //查找所有有效的采购路线明细
                    //            IList<object[]> flowDetailList = base.genericMgr.FindAllWithNativeSql<object[]>("select m.Code,m.partyfrom,d.item from scm_flowdet d left join scm_flowmstr m on d.flow = m.code where d.isactive = 1 and m.isactive = 1 and m.type = 1");
                    //            IList<FlowDetail> currentFlowDetailList = base.genericMgr.FindAll<FlowDetail>("select m from FlowDetail as m where Flow = ?", new object[] { flow.Code });

                    //            Location location = base.genericMgr.FindById<Location>(flow.LocationTo);
                    //            Party partyTo = base.genericMgr.FindById<Party>(flow.PartyTo);
                    //            Address address = base.genericMgr.FindById<Address>(flow.ShipTo);

                    //            //先遍历所有明细行，检查供应商+物料是否已存在其他的有效路线明细
                    //            //记录所有重复明细行

                    //            foreach (FlowDetail currentFlowDetail in currentFlowDetailList)
                    //            {
                    //                if (currentFlowDetail.IsActive)
                    //                {

                    //                    int count = (from f in flowDetailList
                    //                                 where f[1].ToString() == flow.PartyFrom && f[2].ToString() == currentFlowDetail.Item
                    //                                 select f).Count();

                    //                    //如果不存在
                    //                    //if (count == 0)
                    //                    //{
                    //                    //    string updateMstr = "Update Ord_OrderMstr_8 set Flow = ?,FlowDesc = ?,PartyTo = ?,PartyToNm = ?,ShipTo = ?,ShipToAddr = ?,LocTo = ?,LocToNm = ? where PartyFrom = ? and exists (select 1 from Ord_OrderDet_8 as d where d.Item = ? and Ord_OrderMstr_8.OrderNo=d.OrderNo)";
                    //                    //    base.genericMgr.FindAllWithNativeSql(updateMstr, new object[] { flow.Code, flow.Description, flow.PartyTo, partyTo.Name, flow.ShipTo, address.AddressContent, flow.LocationTo, location.Name, flow.PartyFrom, currentFlowDetail.Item });
                    //                    //    string updateStr = "Update Ord_OrderDet_8 set LocTo = ?,LocToNm = ?,UC = ? where Item = ? and exists (select 1 from Ord_OrderMstr_8 as o where o.PartyFrom = ? and Ord_OrderDet_8.OrderNo=o.OrderNo)";
                    //                    //    base.genericMgr.FindAllWithNativeSql(updateStr, new object[] { flow.LocationTo, location.Name, currentFlowDetail.UnitCount, currentFlowDetail.Item, flow.PartyFrom });
                    //                    //}
                    //                    if (count > 0)
                    //                    {
                    //                        if (errorMessage == string.Empty)
                    //                            errorMessage += "路线" + flow.Code + ",供应商" + flow.PartyFrom + ",物料" + currentFlowDetail.Item + "已存在有效的路线明细";
                    //                        else
                    //                            errorMessage += "<br />路线" + flow.Code + ",供应商" + flow.PartyFrom + ",物料" + currentFlowDetail.Item + "已存在有效的路线明细";

                    //                    }
                    //                }
                    //            }
                    //            //如果没有报错记录，即当前路线的所有明细都不与其他路线明细冲突，则循环去更新订单
                    //            if (errorMessage == string.Empty)
                    //            {
                    //                foreach (FlowDetail currentFlowDetail in currentFlowDetailList)
                    //                {
                    //                    string updateMstr = "Update Ord_OrderMstr_8 set Flow = ?,FlowDesc = ?,PartyTo = ?,PartyToNm = ?,ShipTo = ?,ShipToAddr = ?,LocTo = ?,LocToNm = ? where PartyFrom = ? and exists (select 1 from Ord_OrderDet_8 as d where d.Item = ? and Ord_OrderMstr_8.OrderNo=d.OrderNo)";
                    //                    base.genericMgr.FindAllWithNativeSql(updateMstr, new object[] { flow.Code, flow.Description, flow.PartyTo, partyTo.Name, flow.ShipTo, address.AddressContent, flow.LocationTo, location.Name, flow.PartyFrom, currentFlowDetail.Item });
                    //                    string updateStr = "Update Ord_OrderDet_8 set LocTo = ?,LocToNm = ?,UC = ? where Item = ? and exists (select 1 from Ord_OrderMstr_8 as o where o.PartyFrom = ? and Ord_OrderDet_8.OrderNo=o.OrderNo)";
                    //                    base.genericMgr.FindAllWithNativeSql(updateStr, new object[] { flow.LocationTo, location.Name, currentFlowDetail.UnitCount, currentFlowDetail.Item, flow.PartyFrom });
                    //                }

                    //                base.genericMgr.Update(flow);
                    //                SaveSuccessMessage(Resources.SCM.FlowMaster.FlowMaster_Updated);
                    //            }
                    //            else
                    //                SaveErrorMessage(errorMessage);
                    //        }
                    //    }
                    //}

                }
            }

            return PartialView(flow);
        }

        [SconitAuthorize(Permissions = "Url_ProcurementFlow_Delete")]
        public ActionResult FlowDel(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                flowMgr.DeleteFlow(id);
                SaveSuccessMessage(Resources.SCM.FlowMaster.FlowMaster_Deleted);
                return RedirectToAction("List");
            }
        }

        [SconitAuthorize(Permissions = "Url_ProcurementFlow_Delete")]
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<FlowMaster>(id);
                SaveSuccessMessage(Resources.ACC.User.User_Deleted);
                return RedirectToAction("List");
            }
        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, FlowSearchModel searchModel)
        {
            string whereStatement = " where f.Type=1 ";

            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("Code", searchModel.Code, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Description", searchModel.Description, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("LocationTo", searchModel.LocationTo, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("PartyFrom", searchModel.PartyFrom, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("PartyTo", searchModel.PartyTo, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("IsActive", searchModel.IsActive, "f", ref whereStatement, param);

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        #endregion

        #region Strategy
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_Edit")]
        public ActionResult _Strategy(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            FlowStrategy flowStrategy = this.flowMgr.GetFlowStrategy(id);
            if (flowStrategy == null)
            {
                flowStrategy = new FlowStrategy();
                flowStrategy.Flow = id;
            }
            ViewBag.NextWindowTime = flowStrategy.NextWindowTime;
            ViewBag.NextOrderTime = flowStrategy.NextOrderTime;
            ViewBag.WindowTimeType = flowStrategy.WindowTimeType;
            return PartialView(flowStrategy);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_Edit")]
        public ActionResult _Strategy(FlowStrategy flowStrategy)
        {
            ViewBag.WindowTimeType = flowStrategy.WindowTimeType;
            if (ModelState.IsValid)
            {
                var hasError = false;
                if (flowStrategy.Strategy == CodeMaster.FlowStrategy.JIT || flowStrategy.Strategy == CodeMaster.FlowStrategy.KB)
                {
                    //if (flowStrategy.WindowTimeType == CodeMaster.WindowTimeType.FixedWindowTime)
                    //{
                    //    if (string.IsNullOrWhiteSpace(flowStrategy.WindowTime1) &&
                    //    string.IsNullOrWhiteSpace(flowStrategy.WindowTime2) && string.IsNullOrWhiteSpace(flowStrategy.WindowTime3) &&
                    //    string.IsNullOrWhiteSpace(flowStrategy.WindowTime4) && string.IsNullOrWhiteSpace(flowStrategy.WindowTime5) &&
                    //    string.IsNullOrWhiteSpace(flowStrategy.WindowTime6) && string.IsNullOrWhiteSpace(flowStrategy.WindowTime7))
                    //    {
                    //        hasError = true;
                    //        SaveErrorMessage(Resources.SCM.FlowStrategy.FlowStrategy_WhenCheckFixedTypeWindowTimeCanNotAllEmpty);
                    //    }
                    //}
                    if (flowStrategy.WindowTimeType == CodeMaster.WindowTimeType.CycledWindowTime)
                    {
                        if (flowStrategy.WeekInterval <= 0)
                        {
                            hasError = true;
                            SaveErrorMessage(Resources.SCM.FlowStrategy.FlowStrategy_WeekIntervalCanNotLessThanZero);
                        }
                    }
                }

                if (flowStrategy.Strategy == CodeMaster.FlowStrategy.SEQ)
                {
                    if (string.IsNullOrWhiteSpace(flowStrategy.SeqGroup))
                    {
                        hasError = true;
                        SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowStrategy.FlowStrategy_SeqGroup);
                    }
                }

                if (!hasError)
                {
                    flowMgr.UpdateFlowStrategy(flowStrategy);
                    this.genericMgr.UpdateWithNativeQuery("update SCM_FlowStrategy set MRPTotalAdj=? ,MRPWeight=? where Flow=?", new object[] { flowStrategy.MrpTotalAdjust, flowStrategy.MrpWeight, flowStrategy.Flow});
                    SaveSuccessMessage(Resources.SCM.FlowStrategy.FlowStrategy_Updated);
                }
            }
            return PartialView(flowStrategy);
        }
        #endregion

        #region FlowDetail
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_View")]
        public ActionResult _DetailSearch(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            ViewBag.flow = id;
            return PartialView();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_View")]
        public ActionResult _Detail(GridCommand command, FlowDetailSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_View")]
        public ActionResult _AjaxDetailList(GridCommand command, FlowDetailSearchModel searchModel)
        {
            string referenceFlow = base.genericMgr.FindAll<string>("select ReferenceFlow from FlowMaster where Code=?", searchModel.flowCode)[0];
            SearchStatementModel searchStatementModel = PrepareDetailSearchStatement(command, searchModel, referenceFlow);
            GridModel<FlowDetail> gridList = GetAjaxPageData<FlowDetail>(searchStatementModel, command);
            if (gridList.Data != null && gridList.Data.Count() > 0)
            {
                foreach (FlowDetail flowDetail in gridList.Data)
                {
                    flowDetail.ItemDescription = base.genericMgr.FindById<Item>(flowDetail.Item).Description;
                }
            }
            return PartialView(gridList);
        }

        [SconitAuthorize(Permissions = "Url_ProcurementFlow_Edit")]
        public ActionResult _DetailNew(string id)
        {
            FlowDetail flowDetail = new FlowDetail();
            FlowMaster flowMaster = base.genericMgr.FindById<FlowMaster>(id);
            flowDetail.LocationTo = flowMaster.LocationTo;
            flowDetail.Flow = id;
            return PartialView(flowDetail);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_Edit")]
        public ActionResult _DetailNew(FlowDetail flowDetail, string id)
        {
            if (ModelState.IsValid)
            {
                if (false)//暂不做控制
                {
                    // base.SaveErrorMessage(Resources.ErrorMessage.Errors_Existing_Code, flowDetail.Code);
                }
                else
                {
                    //IList<FlowDetail> flowDetailList = base.genericMgr.FindAll<FlowDetail>("select fd from FlowDetail as fd where fd.Flow='" + flowDetail.Flow + "' and fd.Item='" + flowDetail.Item + "'" );
                    //对于采购路线如果物料在该路线明细中已经存在则报错
                    IList<FlowDetail> flowDetailList = base.genericMgr.FindAll<FlowDetail>("select fd from FlowDetail as fd where fd.Item='" + flowDetail.Item + "' and fd.IsActive <> 0 and fd.Flow = '" + flowDetail.Flow + "'");

                    if (flowDetailList.Count > 0)
                    {
                        SaveErrorMessage("当前路线已存在物料:{0}的有效路线明细。", new string[] { flowDetail.Item });

                    }
                    else
                    {
                        try
                        {

                            #region andon的要做校验
                            FlowMaster flow = genericMgr.FindById<FlowMaster>(flowDetail.Flow);
                            int checkeSame = this.genericMgr.FindAllWithNativeSql<int>(" select COUNT(*) as countSum  from SCM_FlowDet as d where d.Item=? and exists( select 1 from SCM_FlowMstr as m where m.Code=d.Flow and m.PartyFrom=? and m.LocTo=? and m.Type=1  )", new object[] { flowDetail.Item, flow.PartyFrom, flow.LocationTo })[0];
                            if (checkeSame > 0)
                            {
                                throw new BusinessException(string.Format("来源区域{0}+物料{1}+目的库位{2}已经存在数据库。", flow.PartyFrom, flowDetail.Item, flow.LocationTo));
                            }
                            if (flow.FlowStrategy == com.Sconit.CodeMaster.FlowStrategy.ANDON)
                            {
                                if (string.IsNullOrEmpty(flowDetail.BinTo))
                                {
                                    throw new BusinessException("工位不能为空。");
                                }
                                if (flowDetail.BinTo.Length < 5)
                                {
                                    throw new BusinessException("工位长度小于5位。");
                                }

                                int currentSeq = int.Parse(numberControlMgr.GetNextSequence(flowDetail.BinTo.Substring(0, 5)));
                                if (currentSeq >= 999)
                                {
                                    throw new BusinessException("已达到999，不能继续创建");
                                }
                                //string nextSeqString = currentSeq.ToString();
                                //if (nextSeqString.Length < 3)
                                //{
                                //    nextSeqString = nextSeqString.PadLeft(3, '0');
                                //}
                                //flowDetail.OprefSequence = "KB" + flowDetail.BinTo.Substring(0, 5) + nextSeqString;
                            }
                            #endregion

                            Item item = base.genericMgr.FindById<Item>(flowDetail.Item);
                            flowDetail.BaseUom = item.Uom;
                            flowDetail.MinUnitCount = item.MinUnitCount;
                            flowDetail.Container = item.Container;
                            flowDetail.IsActive = true;
                            flowMgr.CreateFlowDetail(flowDetail);
                            SaveSuccessMessage(Resources.SCM.FlowDetail.FlowDetail_Added);
                            return RedirectToAction("_Detail/" + flowDetail.Flow);
                        }
                        catch (Exception ex)
                        {
                            SaveErrorMessage(ex.Message);
                        }
                    }
                }
            }
            return PartialView(flowDetail);
        }


        [GridAction]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_Edit")]
        public ActionResult btnDel(int? id, string Flow)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                //删除前先获取物料号和供应商
                string item = base.genericMgr.FindById<FlowDetail>(id).Item;
                string partyFrom = base.genericMgr.FindById<FlowMaster>(Flow).PartyFrom;
                base.genericMgr.DeleteById<FlowDetail>(id);

                #region 更新计划协议历史数据,按照路线更新
                //IList<FlowDetail> newFlowDetailList = base.genericMgr.FindAll<FlowDetail>("select fd from FlowDetail as fd where fd.Item='" + item + "' and IsActive <> 0 and exists (select 1 from FlowMaster as fm where fm.Code=fd.Flow and fm.PartyFrom='" + partyFrom + "' and fm.IsActive = 1)");
                //if (newFlowDetailList.Count == 1)
                //{
                //    FlowMaster flowmstr = base.genericMgr.FindById<FlowMaster>(newFlowDetailList[0].Flow);
                //    Location location = base.genericMgr.FindById<Location>(flowmstr.LocationTo);
                //    Party partyTo = base.genericMgr.FindById<Party>(flowmstr.PartyTo);
                //    Address address = base.genericMgr.FindById<Address>(flowmstr.ShipTo);

                //    string updateMstr = "Update Ord_OrderMstr_8 set Flow = ?,FlowDesc = ?,PartyTo = ?,PartyToNm = ?,ShipTo = ?,ShipToAddr = ?,LocTo = ?,LocToNm = ? where PartyFrom = ? and exists (select 1 from Ord_OrderDet_8 as d where d.Item = ? and Ord_OrderMstr_8.OrderNo=d.OrderNo)";
                //    base.genericMgr.FindAllWithNativeSql(updateMstr, new object[] { flowmstr.Code, flowmstr.Description, flowmstr.PartyTo, partyTo.Name, flowmstr.ShipTo, address.AddressContent, flowmstr.LocationTo, location.Name, partyFrom, item });
                //    string updateStr = "Update Ord_OrderDet_8 set LocTo = ?,LocToNm = ?,UC = ? where Item = ? and exists (select 1 from Ord_OrderMstr_8 as o where o.PartyFrom = ? and Ord_OrderDet_8.OrderNo=o.OrderNo)";
                //    base.genericMgr.FindAllWithNativeSql(updateStr, new object[] { flowmstr.LocationTo, location.Name, newFlowDetailList[0].UnitCount, item, partyFrom });
                //}
                //else if (newFlowDetailList.Count == 0)
                //{
                //    string updateMstr = "Update Ord_OrderMstr_8 set Flow = null where PartyFrom = ? and exists (select 1 from Ord_OrderDet_8 as d where d.Item = ? and Ord_OrderMstr_8.OrderNo=d.OrderNo)";
                //    base.genericMgr.FindAllWithNativeSql(updateMstr, new object[] { partyFrom, item });

                //}

                #endregion
                SaveSuccessMessage(Resources.SCM.FlowDetail.FlowDetail_Deleted);
                return RedirectToAction("_Detail/" + Flow);
            }
        }

        public ActionResult GetRefItemCode(string item)
        {
            Item itemEntity = base.genericMgr.FindById<Item>(item);
            if (itemEntity == null)
            {
                itemEntity = new Item(); ;
            }
            return Json(itemEntity);
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_Edit")]
        public ActionResult _DetailEdit(int? id)
        {
            if (!id.HasValue || id.Value == 0)
            {
                return HttpNotFound();
            }

            FlowDetail flowDetail = base.genericMgr.FindById<FlowDetail>(id);
            ViewBag.Item = base.genericMgr.FindById<Item>(flowDetail.Item);
            //flowDetail.IsActive = true;
            return PartialView(flowDetail);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_Edit")]
        public ActionResult _DetailEdit(FlowDetail flowDetail, int? id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //IList<FlowDetail> flowDetailList = base.genericMgr.FindAll<FlowDetail>("select fd from FlowDetail as fd where fd.Flow='" + flowDetail.Flow + "' and fd.Item='" + flowDetail.Item + "'" );
                    //如果当前明细更新为有效的，则根据供应商+物料将其他明细更新为无效的
                    FlowMaster flow = genericMgr.FindById<FlowMaster>(flowDetail.Flow);
                    //string partyFrom = base.genericMgr.FindAll<FlowMaster>("select fm from FlowMaster as fm where fm.Code='" + flowDetail.Flow + "'")[0].PartyFrom;
                    //if (flowDetail.IsActive)
                    //{
                    //    string updateMstr = "Update SCM_FlowDet set IsActive = ? where Item = ? and exists (select 1 from SCM_FlowMstr as m where m.PartyFrom = ? and m.Code = SCM_FlowDet.Flow)";
                    //    base.genericMgr.FindAllWithNativeSql(updateMstr, new object[] { false, flowDetail.Item, flow.PartyFrom });
                    //}
                    int checkeSame = this.genericMgr.FindAllWithNativeSql<int>(" select COUNT(*) as countSum  from SCM_FlowDet as d where d.Item=? and exists( select 1 from SCM_FlowMstr as m where m.Code=d.Flow and m.PartyFrom=? and m.LocTo=? and m.Type=1  ) and d.Id<>?", new object[] { flowDetail.Item, flow.PartyFrom, flow.LocationTo, flowDetail.Id })[0];
                    if (checkeSame > 0)
                    {
                        throw new BusinessException(string.Format("来源区域{0}+物料{1}+目的库位{2}已经存在数据库。", flow.PartyFrom, flowDetail.Item, flow.LocationTo));
                    }
                    flowDetail.BaseUom = base.genericMgr.FindById<Item>(flowDetail.Item).Uom;
                    flowMgr.UpdateFlowDetail(flowDetail);
                    ViewBag.Item = base.genericMgr.FindById<Item>(flowDetail.Item);

                    #region 更新计划协议历史数据,按照路线更新
                    //FlowMaster flow = base.genericMgr.FindById<FlowMaster>(flowDetail.Flow);
                    //IList<FlowDetail> newFlowDetailList = base.genericMgr.FindAll<FlowDetail>("select fd from FlowDetail as fd where fd.Item='" + flowDetail.Item + "' and IsActive <> 0 and exists (select 1 from FlowMaster as fm where fm.Code=fd.Flow and fm.PartyFrom='" + flow.PartyFrom + "' and fm.IsActive = 1)");
                    //if (newFlowDetailList.Count == 1)
                    //{
                    //    Location location = flowDetail.IsActive ? base.genericMgr.FindById<Location>(flow.LocationTo) : null;
                    //    Party partyTo = flowDetail.IsActive ? base.genericMgr.FindById<Party>(flow.PartyTo) : null;
                    //    Address address = flowDetail.IsActive ? base.genericMgr.FindById<Address>(flow.ShipTo) : null;

                    //    string updateMstr = "Update Ord_OrderMstr_8 set Flow = ?,FlowDesc = ?,PartyTo = ?,PartyToNm = ?,ShipTo = ?,ShipToAddr = ?,LocTo = ?,LocToNm = ? where PartyFrom = ? and exists (select 1 from Ord_OrderDet_8 as d where d.Item = ? and Ord_OrderMstr_8.OrderNo=d.OrderNo)";
                    //    base.genericMgr.FindAllWithNativeSql(updateMstr, new object[] { flow.Code, flow.Description, flow.PartyTo, partyTo.Name, flow.ShipTo, address.AddressContent, flow.LocationTo, location.Name, flow.PartyFrom, flowDetail.Item });
                    //    string updateStr = "Update Ord_OrderDet_8 set LocTo = ?,LocToNm = ?,UC = ? where Item = ? and exists (select 1 from Ord_OrderMstr_8 as o where o.PartyFrom = ? and Ord_OrderDet_8.OrderNo=o.OrderNo)";
                    //    base.genericMgr.FindAllWithNativeSql(updateStr, new object[] { flow.LocationTo, location.Name, flowDetail.UnitCount, flowDetail.Item, flow.PartyFrom });
                    //}
                    //else if (newFlowDetailList.Count == 0)
                    //{
                    //    string updateMstr = "Update Ord_OrderMstr_8 set Flow = null where  PartyFrom = ? and exists (select 1 from Ord_OrderDet_8 as d where d.Item = ? and Ord_OrderMstr_8.OrderNo=d.OrderNo)";
                    //    base.genericMgr.FindAllWithNativeSql(updateMstr, new object[] { flow.PartyFrom, flowDetail.Item });

                    //}

                    #endregion
                    SaveSuccessMessage(Resources.SCM.FlowDetail.FlowDetail_Updated);
                }
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }

            TempData["TabIndex"] = 2;
            return PartialView(flowDetail);
        }

        private SearchStatementModel PrepareDetailSearchStatement(GridCommand command, FlowDetailSearchModel searchModel, string referenceFlow)
        {
            //string str = string.IsNullOrEmpty(referenceFlow) ? string.Empty : string.Format(" or f.Flow='{0}'",referenceFlow);
            string whereStatement = " where f.Flow='" + searchModel.flowCode + "'" + (string.IsNullOrEmpty(referenceFlow) ? string.Empty : string.Format(" or f.Flow='{0}'", referenceFlow));
            if (string.IsNullOrEmpty(referenceFlow))
            {
                whereStatement = whereStatement + " ";
            }
            IList<object> param = new List<object>();
            HqlStatementHelper.AddLikeStatement("Item", searchModel.Item, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "f", ref whereStatement, param);
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountDetailStatement;
            searchStatementModel.SelectStatement = selectDetailStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        #endregion

        #region FlowDetail 报表
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_DetailView")]
        public ActionResult DetailIndex()
        {
            return View();
        }


        [GridAction]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_DetailView")]
        public ActionResult FlowDetailList(GridCommand command, FlowDetailSearchModel searchModel)
        {
            this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_DetailView")]
        public ActionResult _AjaxFlowDetailList(GridCommand command, FlowDetailSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareSearchDetailStatement(command, searchModel);
            // return PartialView(GetAjaxPageData<FlowDetail>(searchStatementModel, command));
            GridModel<FlowDetail> gridList = GetAjaxPageData<FlowDetail>(searchStatementModel, command);
            if (gridList.Data != null && gridList.Data.Count() > 0)
            {
                foreach (FlowDetail flowDetail in gridList.Data)
                {
                    flowDetail.ItemDescription = base.genericMgr.FindById<Item>(flowDetail.Item).Description;
                    FlowMaster flowMaster = base.genericMgr.FindById<FlowMaster>(flowDetail.Flow);
                    flowDetail.ManufactureParty = flowMaster.PartyFrom;
                    Supplier supplier = base.genericMgr.FindById<Supplier>(flowDetail.ManufactureParty);
                    flowDetail.ManufacturePartyShortCode = supplier.ShortCode;
                    flowDetail.ManufacturePartyDesc = supplier.CodeDescription;
                }

                foreach (FlowDetail flowDetail in gridList.Data)
                {
                    flowDetail.ItemDescription = base.genericMgr.FindById<Item>(flowDetail.Item).Description;
                }
            }
            return PartialView(gridList);
        }

        private SearchStatementModel PrepareSearchDetailStatement(GridCommand command, FlowDetailSearchModel searchModel)
        {
            //string str = string.IsNullOrEmpty(referenceFlow) ? string.Empty : string.Format(" or f.Flow='{0}'",referenceFlow);
            string whereStatement = "where exists (select m from FlowMaster as m where m.IsActive=" + searchModel.IsActive + " and m.Code=f.Flow)";
            IList<object> param = new List<object>();
            if (searchModel.IsActive)
            {
                HqlStatementHelper.AddEqStatement("IsActive", searchModel.IsActive, "f", ref whereStatement, param);
            }

            HqlStatementHelper.AddLikeStatement("Item", searchModel.Item, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "f", ref whereStatement, param);

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountDetailStatement;
            searchStatementModel.SelectStatement = selectDetailStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        #endregion

        #region Bind
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_Edit")]
        public ActionResult _Binding(GridCommand command, FlowBindModel searchModel)
        {
            if (string.IsNullOrWhiteSpace(searchModel.id))
            {
                return HttpNotFound();
            }
            ViewBag.flow = searchModel.id;
            TempData["FlowBindModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_Edit")]
        public ActionResult _Binded(GridCommand command, FlowBindModel searchModel, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            ViewBag.flow = id;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            SearchCacheModel searchCacheModel = ProcessSearchModel(command, searchModel);
            SearchStatementModel searchStatementModel = PrepareBindedSearchStatement(command, (FlowBindModel)searchCacheModel.SearchObject, id);
            return PartialView(GetPageData<FlowBinding>(searchStatementModel, command));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_View")]
        public ActionResult _AjaxBinding(GridCommand command, FlowBindModel searchModel, string id)
        {

            SearchStatementModel searchStatementModel = PrepareBindSearchStatement(command, searchModel, id);
            return PartialView(GetAjaxPageData<FlowBinding>(searchStatementModel, command));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_View")]
        public ActionResult _AjaxBinded(GridCommand command, FlowBindModel searchModel, string id)
        {

            SearchStatementModel searchStatementModel = PrepareBindedSearchStatement(command, searchModel, id);
            return PartialView(GetAjaxPageData<FlowBinding>(searchStatementModel, command));
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_Edit")]
        public ActionResult _BindingEdit(int? id)
        {
            if (!id.HasValue || id.Value == 0)
            {
                return HttpNotFound();
            }
            FlowBinding flowBinding = base.genericMgr.FindById<FlowBinding>(id);
            return PartialView(flowBinding);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_Edit")]
        public ActionResult _BindingEdit(FlowBinding flowBinding)
        {
            ModelState.Remove("MasterFlow.Description");
            ModelState.Remove("BindedFlow.Description");
            if (ModelState.IsValid)
            {
                base.genericMgr.Update(flowBinding);
                SaveSuccessMessage(Resources.SCM.FlowBinding.FlowBinding_Updated);
            }

            return PartialView(flowBinding);
        }

        [SconitAuthorize(Permissions = "Url_ProcurementFlow_Edit")]
        public ActionResult _BindingNew(string id)
        {
            FlowMaster flow = base.genericMgr.FindById<FlowMaster>(id);
            FlowBinding flowBinding = new FlowBinding();
            flowBinding.MasterFlow = flow;
            return PartialView(flowBinding);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_Edit")]
        public ActionResult _BindingNew(FlowBinding flowBinding)
        {
            ModelState.Remove("MasterFlow.Description");
            ModelState.Remove("BindedFlow.Description");
            if (ModelState.IsValid)
            {
                if (false)//暂不做控制&& base.genericMgr.FindAll<long>("sql")[0] > 0
                {
                    // base.SaveErrorMessage(Resources.ErrorMessage.Errors_Existing_Code, flowDetail.Code);
                }
                else
                {
                    base.genericMgr.Create(flowBinding);
                    SaveSuccessMessage(Resources.SCM.FlowDetail.FlowDetail_Added);
                    return RedirectToAction("_Binding/" + flowBinding.MasterFlow.Code);
                }
            }
            return PartialView(flowBinding);
        }

        [SconitAuthorize(Permissions = "Url_DistributionFlow_Edit")]
        public ActionResult _BindingDelete(string id)
        {
            FlowBinding flowBinding = base.genericMgr.FindById<FlowBinding>(int.Parse(id));
            base.genericMgr.DeleteById<FlowBinding>(int.Parse(id));
            SaveSuccessMessage(Resources.SCM.FlowBinding.FlowDetail_Deleted);
            return RedirectToAction("_Binding/" + flowBinding.MasterFlow.Code);
        }

        private SearchStatementModel PrepareBindSearchStatement(GridCommand command, FlowBindModel searchModel, string id)
        {
            string whereStatement = " where mf.Code='" + id + "'";
            IList<object> param = new List<object>();

            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "BindedFlow.Code")
                {
                    command.SortDescriptors[0].Member = "bf.Code";
                }
                else if (command.SortDescriptors[0].Member == "BindedFlow.Description")
                {
                    command.SortDescriptors[0].Member = "bf.Description";
                }
                else if (command.SortDescriptors[0].Member == "BindTypeDescription")
                {
                    command.SortDescriptors[0].Member = "f.BindType";
                }
            }

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountBindStatement;
            searchStatementModel.SelectStatement = selectBindStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        private SearchStatementModel PrepareBindedSearchStatement(GridCommand command, FlowBindModel searchModel, string id)
        {
            string whereStatement = " where bf.Code='" + id + "'";
            IList<object> param = new List<object>();



            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountBindStatement;
            searchStatementModel.SelectStatement = selectBindStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        #endregion

        #region FlowShiftDet
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_View")]
        public ActionResult _FlowShiftDetailSearch(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            ViewBag.flow = id;
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_View")]
        public ActionResult _AjaxFlowShiftDetailList(GridCommand command, string Shift, string FlowCode)
        {

            SearchStatementModel searchStatementModel = PrepareFlowShiftDetailSearchStatement(command, Shift, FlowCode);
            return PartialView(GetAjaxPageData<FlowShiftDetail>(searchStatementModel, command));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_ProcurementFlow_View")]
        public ActionResult _FlowShiftDetailList(GridCommand command, string FlowCode, string Shift)
        {
            ViewBag.Shift = Shift;
            ViewBag.Flow = FlowCode;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [SconitAuthorize(Permissions = "Url_ProcurementFlow_Edit")]
        public JsonResult _SaveflowShoftDetailEdit([Bind(Prefix = "inserted")]IEnumerable<FlowShiftDetail> insertedFlowShiftDetails,
            [Bind(Prefix = "updated")]IEnumerable<FlowShiftDetail> updatedFlowShiftDetails,
            [Bind(Prefix = "deleted")]IEnumerable<FlowShiftDetail> deletedFlowShiftDetails, string flow)
        {
            try
            {
                IList<FlowShiftDetail> existFlowShiftDetailList = genericMgr.FindAll<FlowShiftDetail>("from FlowShiftDetail s where s.Flow = ?", flow);

                IList<FlowShiftDetail> deleted = new List<FlowShiftDetail>();
                if (deletedFlowShiftDetails != null)
                {
                    deleted = deletedFlowShiftDetails.ToList();
                }

                IList<FlowShiftDetail> inserted = new List<FlowShiftDetail>();
                if (insertedFlowShiftDetails != null)
                {
                    foreach (var flowShiftDet in insertedFlowShiftDetails.ToList())
                    {
                        if (string.IsNullOrEmpty(flowShiftDet.Shift))
                        {
                            throw new BusinessException(Resources.ErrorMessage.Errors_Common_FieldCanNotBeNull, Resources.SCM.FlowShiftDetail.FlowShiftDetail_Shift);
                        }
                        if (string.IsNullOrEmpty(flowShiftDet.WindowTime))
                        {
                            throw new BusinessException(Resources.ErrorMessage.Errors_Common_FieldCanNotBeNull, Resources.SCM.FlowShiftDetail.FlowShiftDetail_WindowTime);
                        }


                        var insertedEquals = insertedFlowShiftDetails.Where(c => c.Shift == flowShiftDet.Shift);

                        if (insertedEquals.Count() > 1)
                        {
                            throw new BusinessException(Resources.PRD.ShiftDetail.ShiftDetail_SameShift);
                        }

                        if (updatedFlowShiftDetails != null)
                        {
                            var updatedEquals = updatedFlowShiftDetails.Where(c => c.Shift == flowShiftDet.Shift);
                            if (updatedEquals.Count() > 0)
                            {
                                throw new BusinessException(Resources.PRD.ShiftDetail.ShiftDetail_SameShift);
                            }
                        }

                        if (existFlowShiftDetailList != null)
                        {
                            var existEquals = existFlowShiftDetailList.Where(c => c.Shift == flowShiftDet.Shift);
                            if (existEquals.Count() > 0)
                            {
                                throw new BusinessException(Resources.PRD.ShiftDetail.ShiftDetail_SameShift);
                            }
                        }

                        inserted.Add(flowShiftDet);
                    }
                }

                IList<FlowShiftDetail> updated = new List<FlowShiftDetail>();
                if (updatedFlowShiftDetails != null)
                {
                    foreach (var flowShiftDet in updatedFlowShiftDetails.ToList())
                    {
                        if (string.IsNullOrEmpty(flowShiftDet.Shift))
                        {
                            throw new BusinessException(Resources.ErrorMessage.Errors_Common_FieldCanNotBeNull, Resources.SCM.FlowShiftDetail.FlowShiftDetail_Shift);
                        }
                        if (string.IsNullOrEmpty(flowShiftDet.WindowTime))
                        {
                            throw new BusinessException(Resources.ErrorMessage.Errors_Common_FieldCanNotBeNull, Resources.SCM.FlowShiftDetail.FlowShiftDetail_WindowTime);
                        }
                        if (insertedFlowShiftDetails != null)
                        {
                            var insertedEquals = insertedFlowShiftDetails.Where(c => c.Shift == flowShiftDet.Shift);
                            if (insertedEquals.Count() > 0)
                            {
                                throw new BusinessException(Resources.PRD.ShiftDetail.ShiftDetail_SameShift);
                            }
                        }

                        var updatedEquals = updatedFlowShiftDetails.Where(c => c.Shift == flowShiftDet.Shift && c.Id != flowShiftDet.Id);
                        if (updatedEquals.Count() > 0)
                        {
                            throw new BusinessException(Resources.PRD.ShiftDetail.ShiftDetail_SameShift);
                        }

                        if (existFlowShiftDetailList != null)
                        {
                            var existEquals = existFlowShiftDetailList.Where(c => c.Shift == flowShiftDet.Shift && c.Id != flowShiftDet.Id);
                            if (existEquals.Count() > 0)
                            {
                                throw new BusinessException(Resources.PRD.ShiftDetail.ShiftDetail_SameShift);
                            }
                        }
                        updated.Add(flowShiftDet);
                    }
                }

                flowMgr.UpdateFlowShiftDetails(flow, inserted, updated, deleted);

                object obj = new { };
                SaveSuccessMessage(Resources.SCM.FlowShiftDetail.FlowShiftDetail_Save);
                return Json(obj);
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                return Json(null);
            }
        }

        private SearchStatementModel PrepareFlowShiftDetailSearchStatement(GridCommand command, string Shift, string Flow)
        {
            string whereStatement = string.Empty;
            IList<object> param = new List<object>();
            HqlStatementHelper.AddEqStatement("Shift", Shift, "f", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Flow", Flow, "f", ref whereStatement, param);
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountFlowShiftDetailStatement;
            searchStatementModel.SelectStatement = selectFlowShiftDetailStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }
        #endregion

        public ActionResult _GetSeqGroup(string seqGroup)
        {
            if (!string.IsNullOrWhiteSpace(seqGroup))
            {
                var SsequenceGroup = this.genericMgr.FindById<SequenceGroup>(seqGroup);
                return this.Json(SsequenceGroup);
            }
            else
            {
                return null;
            }
        }

    }
}
