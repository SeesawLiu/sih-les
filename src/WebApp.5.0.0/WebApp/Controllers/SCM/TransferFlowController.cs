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
using com.Sconit.Entity.Exception;
using System.Web;
using System;

namespace com.Sconit.Web.Controllers.SCM
{
    public class TransferFlowController : WebAppBaseController
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
        public TransferFlowController()
        {
        }

        [SconitAuthorize(Permissions = "Url_TransferFlow_View")]
        public ActionResult Index()
        {
            return View();
        }

        #region FlowMaster

        [GridAction]
        [SconitAuthorize(Permissions = "Url_TransferFlow_View")]
        public ActionResult List(GridCommand command, FlowSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_TransferFlow_View")]
        public ActionResult _AjaxList(GridCommand command, FlowSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<FlowMaster>(searchStatementModel, command));
        }

        [SconitAuthorize(Permissions = "Url_TransferFlow_Edit")]
        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_TransferFlow_Edit")]
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
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_TransferPartyFrom);
                }
                else if (string.IsNullOrEmpty(flow.PartyTo))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_TransferPartyTo);
                }
                else if (string.IsNullOrEmpty(flow.LocationFrom))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_LocationFrom);
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
                else
                {
                    
                    flow.FlowStrategy = com.Sconit.CodeMaster.FlowStrategy.Manual;
                    flow.Type = com.Sconit.CodeMaster.OrderType.Transfer;
                    flowMgr.CreateFlow(flow);
                    SaveSuccessMessage(Resources.SCM.FlowMaster.FlowMaster_Added);
                    return RedirectToAction("Edit/" + flow.Code);
                }
            }
            return View(flow);
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_TransferFlow_Edit")]
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
        [SconitAuthorize(Permissions = "Url_TransferFlow_Edit")]
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
        [SconitAuthorize(Permissions = "Url_TransferFlow_Edit")]
        public ActionResult _Edit(FlowMaster flow)
        {



            if (ModelState.IsValid)
            {

                if (string.IsNullOrEmpty(flow.PartyFrom))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_TransferPartyFrom);
                }
                else if (string.IsNullOrEmpty(flow.PartyTo))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_TransferPartyTo);
                }
                else if (string.IsNullOrEmpty(flow.LocationFrom))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_LocationFrom);
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
                else
                {
                    flow.Type = com.Sconit.CodeMaster.OrderType.Transfer;
                    base.genericMgr.Update(flow);
                    SaveSuccessMessage(Resources.SCM.FlowMaster.FlowMaster_Updated);
                }
            }

            return PartialView(flow);
        }

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

        [SconitAuthorize(Permissions = "Url_TransferFlow_Delete")]
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
            string whereStatement = " where f.Type=2 ";

            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("Code", searchModel.Code, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Description", searchModel.Description, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("LocationTo", searchModel.LocationTo, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("LocationFrom", searchModel.LocationFrom, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
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
        [SconitAuthorize(Permissions = "Url_TransferFlow_Edit")]
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
        [SconitAuthorize(Permissions = "Url_TransferFlow_Edit")]
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
                    SaveSuccessMessage(Resources.SCM.FlowStrategy.FlowStrategy_Updated);
                }
            }
            return PartialView(flowStrategy);
        }
        #endregion

        #region FlowDetail
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_TransferFlow_View")]
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
        [SconitAuthorize(Permissions = "Url_TransferFlow_View")]
        public ActionResult _Detail(GridCommand command, FlowDetailSearchModel searchModel, string flowCode)
        {
            searchModel.flowCode = flowCode;
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_TransferFlow_View")]
        public ActionResult _AjaxDetailList(GridCommand command, FlowDetailSearchModel searchModel, string flowCode)
        {
            SearchStatementModel searchStatementModel = PrepareDetailSearchStatement(command, searchModel, flowCode);
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

        [SconitAuthorize(Permissions = "Url_TransferFlow_Edit")]
        public ActionResult _DetailNew(string id)
        {
            FlowDetail flowDetail = new FlowDetail();
            FlowMaster flowMaster = base.genericMgr.FindById<FlowMaster>(id);
            flowDetail.LocationTo = flowMaster.LocationTo;
            flowDetail.LocationFrom = flowMaster.LocationFrom;
            flowDetail.IsCreatePickList = flowMaster.IsCreatePickList;
            flowDetail.Flow = id;
            return PartialView(flowDetail);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_TransferFlow_Edit")]
        public ActionResult _DetailNew(FlowDetail flowDetail, string id)
        {
            ModelState.Remove("UnitCount");
            ModelState.Remove("MinUnitCount");
            ModelState.Remove("ItemDescription");
            ModelState.Remove("ReferenceItemCode");
            ModelState.Remove("Container");
            ModelState.Remove("ContainerDescription");
            ModelState.Remove("Uom");
            //ModelState.Clear();
            if (ModelState.IsValid)
            {
                Item item = this.genericMgr.FindById<Item>(flowDetail.Item);
                flowDetail.UnitCount = item.UnitCount;
                flowDetail.MinUnitCount = item.MinUnitCount;
                flowDetail.ItemDescription = item.Description;
                flowDetail.ReferenceItemCode = item.ReferenceCode;
                flowDetail.Container = item.Container;
                flowDetail.ContainerDescription = item.ContainerDesc;
                flowDetail.Uom = item.Uom;
                if (false)//暂不做控制
                {
                    // base.SaveErrorMessage(Resources.ErrorMessage.Errors_Existing_Code, flowDetail.Code);
                }
                else
                {
                    try
                    {
                        #region andon的要做校验
                        FlowMaster flow = genericMgr.FindById<FlowMaster>(flowDetail.Flow);
                        int checkeSame = this.genericMgr.FindAllWithNativeSql<int>(" select COUNT(*) as countSum  from SCM_FlowDet as d where d.Item=? and exists( select 1 from SCM_FlowMstr as m where m.Code=d.Flow and m.PartyFrom=? and m.PartyTo=? and m.Type=2  ) ", new object[] { flowDetail.Item, flow.PartyFrom, flow.PartyTo})[0];
                        if (checkeSame > 0)
                        {
                            throw new BusinessException(string.Format("来源区域{0}+物料{1}+目的区域{2}已经存在数据库。", flow.PartyFrom, flowDetail.Item, flow.PartyTo));
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

                        flowDetail.BaseUom = base.genericMgr.FindById<Item>(flowDetail.Item).Uom;
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
            return PartialView(flowDetail);
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_TransferFlow_Edit")]
        public ActionResult btnDel(int? id, string Flow)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                // FlowDetail FlowDetail=genericMgr.FindById<FlowDetail>(id);
                base.genericMgr.DeleteById<FlowDetail>(id);
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
        [SconitAuthorize(Permissions = "Url_TransferFlow_Edit")]
        public ActionResult _DetailEdit(int? id)
        {
            if (!id.HasValue || id.Value == 0)
            {
                return HttpNotFound();
            }
            FlowDetail flowDetail = base.genericMgr.FindById<FlowDetail>(id);
            return PartialView(flowDetail);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_TransferFlow_Edit")]
        public ActionResult _DetailEdit(FlowDetail flowDetail, int? id)
        {
            ModelState.Remove("UnitCount");
            ModelState.Remove("MinUnitCount");
            ModelState.Remove("ItemDescription");
            ModelState.Remove("ReferenceItemCode");
            ModelState.Remove("Container");
            ModelState.Remove("ContainerDescription");
            ModelState.Remove("Uom");
            ModelState.Remove("Strategy");
            if (ModelState.IsValid)
            {
                try
                {
                    FlowMaster flow = genericMgr.FindById<FlowMaster>(flowDetail.Flow);
                    int checkeSame = this.genericMgr.FindAllWithNativeSql<int>(" select COUNT(*) as countSum  from SCM_FlowDet as d where d.Item=? and exists( select 1 from SCM_FlowMstr as m where m.Code=d.Flow and m.PartyFrom=? and m.PartyTo=? and m.Type=2  ) and d.Id <> ?", new object[] { flowDetail.Item, flow.PartyFrom, flow.PartyTo, flowDetail.Id })[0];
                    if (checkeSame > 0)
                    {
                        throw new BusinessException(string.Format("来源区域{0}+物料{1}+目的区域{2}已经存在数据库。", flow.PartyFrom, flowDetail.Item, flow.PartyTo));
                    }
                    Item item = this.genericMgr.FindById<Item>(flowDetail.Item);
                    flowDetail.UnitCount = item.UnitCount;
                    flowDetail.MinUnitCount = item.MinUnitCount;
                    flowDetail.ItemDescription = item.Description;
                    flowDetail.ReferenceItemCode = item.ReferenceCode;
                    flowDetail.Container = item.Container;
                    flowDetail.ContainerDescription = item.ContainerDesc;
                    flowDetail.Uom = item.Uom;
                    flowDetail.BaseUom = base.genericMgr.FindById<Item>(flowDetail.Item).Uom;
                    flowDetail.IsActive = true;
                    flowMgr.UpdateFlowDetail(flowDetail);
                    SaveSuccessMessage(Resources.SCM.FlowDetail.FlowDetail_Updated);
                }
                catch (Exception ex)
                {
                    SaveErrorMessage(ex.Message);
                }
            }
            TempData["TabIndex"] = 2;
            return PartialView(flowDetail);
        }

        private SearchStatementModel PrepareDetailSearchStatement(GridCommand command, FlowDetailSearchModel searchModel, string flowCode)
        {
            string whereStatement = " where f.Flow='" + flowCode + "'";
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

        #region Bind
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_TransferFlow_Edit")]
        public ActionResult _Binding(GridCommand command, FlowBindModel searchModel, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            ViewBag.flow = id;
            searchModel.BindedFlow = id;
            TempData["FlowBindModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_TransferFlow_Edit")]
        public ActionResult _Binded(GridCommand command, FlowBindModel searchModel, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            ViewBag.flow = id;
            searchModel.BindedFlow = id;
            TempData["FlowBindModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_TransferFlow_View")]
        public ActionResult _AjaxBinding(GridCommand command, FlowBindModel searchModel, string id)
        {
            SearchStatementModel searchStatementModel = PrepareBindSearchStatement(command, searchModel, id);
            return PartialView(GetAjaxPageData<FlowBinding>(searchStatementModel, command));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_TransferFlow_View")]
        public ActionResult _AjaxBinded(GridCommand command, FlowBindModel searchModel, string id)
        {
            SearchStatementModel searchStatementModel = PrepareBindedSearchStatement(command, searchModel, id);
            return PartialView(GetAjaxPageData<FlowBinding>(searchStatementModel, command));
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_TransferFlow_Edit")]
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
        [SconitAuthorize(Permissions = "Url_TransferFlow_Edit")]
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

        [SconitAuthorize(Permissions = "Url_TransferFlow_Edit")]
        public ActionResult _BindingNew(string id)
        {
            FlowMaster flow = base.genericMgr.FindById<FlowMaster>(id);
            FlowBinding flowBinding = new FlowBinding();
            flowBinding.MasterFlow = flow;
            return PartialView(flowBinding);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_TransferFlow_Edit")]
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

        #region 移库明细批导
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Import")]
        public ActionResult ImportDetailXLS(IEnumerable<HttpPostedFileBase> attachments)
        {
            try
            {
                foreach (var file in attachments)
                {
                    flowMgr.BatchTransferDetailXls(file.InputStream);
                    SaveSuccessMessage("路线明细成功！");
                }
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage("导入失败。 - " + ex.Message);
            }

            return Content(string.Empty);
        }
        #endregion

        #region KanbanFlow
        public void ImportKanbanFlow()
        {
 
        }
        #endregion
    }
}
