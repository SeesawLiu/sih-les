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
using com.Sconit.Entity.PRD;
using com.Sconit.Entity.VIEW;
using System.Web;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace com.Sconit.Web.Controllers.SCM
{
    public class ProductionFlowController : WebAppBaseController
    {
        public IFlowMgr flowMgr { get; set; }

        public IWorkingCalendarMgr workingCalendarMgr { get; set; }


        private static string selectCountStatement = "select count(*) from FlowMaster as f ";
        private static string selectStatement = "select f from FlowMaster as f";

        private static string selectCountDetailStatement = "select count(*) from FlowDetail as f ";
        private static string selectDetailStatement = "select f from FlowDetail as f";

        private static string selectCountBindStatement = @"select count(*) 
                                                      from FlowBinding as f join f.MasterFlow as mf join f.BindedFlow as bf";
        private static string selectBindStatement = @"select f
                                                      from FlowBinding as f join f.MasterFlow as mf join f.BindedFlow as bf";
        //private static string userNameDuiplicateVerifyStatement = @"select count(*) from FlowMaster as u where u.Code = ?";

        public ProductionFlowController()
        {
        }

        [SconitAuthorize(Permissions = "Url_ProductionFlow_View")]
        public ActionResult Index()
        {
            return View();
        }

        #region FlowMaster

        [GridAction]
        [SconitAuthorize(Permissions = "Url_ProductionFlow_View")]
        public ActionResult List(GridCommand command, FlowSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProductionFlow_View")]
        public ActionResult _AjaxList(GridCommand command, FlowSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<FlowMaster>(searchStatementModel, command));
        }

        [SconitAuthorize(Permissions = "Url_ProductionFlow_Edit")]
        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_ProductionFlow_Edit")]
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
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_ProductionPartyFrom);
                }
                else if (string.IsNullOrEmpty(flow.LocationFrom))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_ProductionLocationFrom);
                }
                else if (string.IsNullOrEmpty(flow.LocationTo))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_ProductionLocationTo);
                }
                else
                {
                    flow.PartyTo = flow.PartyFrom;
                    flow.IsCheckPartyToAuthority = flow.IsCheckPartyFromAuthority;
                    flow.FlowStrategy = com.Sconit.CodeMaster.FlowStrategy.Manual;
                    flow.Type = com.Sconit.CodeMaster.OrderType.Production;
                    flowMgr.CreateFlow(flow);
                    SaveSuccessMessage(Resources.SCM.FlowMaster.FlowMaster_Added);
                    return RedirectToAction("Edit/" + flow.Code);
                }
            }
            return View(flow);
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProductionFlow_Edit")]
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            else
            {
                return View("Edit", string.Empty, id);
            }
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProductionFlow_Edit")]
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
        [SconitAuthorize(Permissions = "Url_ProductionFlow_Edit")]
        public ActionResult _Edit(FlowMaster flow)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(flow.PartyFrom))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_ProductionPartyFrom);
                }
                else if (string.IsNullOrEmpty(flow.LocationFrom))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_ProductionLocationFrom);
                }
                else if (string.IsNullOrEmpty(flow.LocationTo))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_ProductionLocationTo);
                }
                else
                {
                    flow.PartyTo = flow.PartyFrom;
                    flow.IsCheckPartyFromAuthority = flow.IsCheckPartyToAuthority;
                    flow.Type = com.Sconit.CodeMaster.OrderType.Production;
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

        [SconitAuthorize(Permissions = "Url_ProductionFlow_Delete")]
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
            string whereStatement = " where f.Type=4 and f.ProdLineType not in (1,2,3,4,9)";

            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("Code", searchModel.Code, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Description", searchModel.Description, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("LocationFrom", searchModel.LocationFrom, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("LocationTo", searchModel.LocationTo, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("PartyFrom", searchModel.PartyFrom, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("IsActive", searchModel.IsActive, "f", ref whereStatement, param);
            //SecurityHelper.AddPartyFromPermissionStatement(ref whereStatement, "f", "PartyFrom", com.Sconit.CodeMaster.OrderType.Production, false);
            SecurityHelper.AddPartyFromAndPartyToPermissionStatement(ref whereStatement, "f", "Type", "f", "PartyFrom", "f", "PartyTo", com.Sconit.CodeMaster.OrderType.Production, false);
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
        [SconitAuthorize(Permissions = "Url_ProductionFlow_Edit")]
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
        [SconitAuthorize(Permissions = "Url_ProductionFlow_Edit")]
        public ActionResult _Strategy(FlowStrategy flowStrategy)
        {
            ViewBag.WindowTimeType = flowStrategy.WindowTimeType;
            if (ModelState.IsValid)
            {
                var hasError = false;
                if (flowStrategy.Strategy == CodeMaster.FlowStrategy.JIT || flowStrategy.Strategy == CodeMaster.FlowStrategy.KB)
                {
                    if (flowStrategy.WindowTimeType == CodeMaster.WindowTimeType.FixedWindowTime)
                    {
                        if (string.IsNullOrWhiteSpace(flowStrategy.WindowTime1) &&
                        string.IsNullOrWhiteSpace(flowStrategy.WindowTime2) && string.IsNullOrWhiteSpace(flowStrategy.WindowTime3) &&
                        string.IsNullOrWhiteSpace(flowStrategy.WindowTime4) && string.IsNullOrWhiteSpace(flowStrategy.WindowTime5) &&
                        string.IsNullOrWhiteSpace(flowStrategy.WindowTime6) && string.IsNullOrWhiteSpace(flowStrategy.WindowTime7))
                        {
                            hasError = true;
                            SaveErrorMessage(Resources.SCM.FlowStrategy.FlowStrategy_WhenCheckFixedTypeWindowTimeCanNotAllEmpty);
                        }
                    }
                    else
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
        [SconitAuthorize(Permissions = "Url_ProductionFlow_View")]
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
        [SconitAuthorize(Permissions = "Url_ProductionFlow_View")]
        public ActionResult _Detail(GridCommand command, FlowDetailSearchModel searchModel, string flowCode)
        {
            searchModel.flowCode = flowCode;
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProductionFlow_View")]
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

        [SconitAuthorize(Permissions = "Url_ProductionFlow_Edit")]
        public ActionResult _DetailNew(string id)
        {
            FlowDetail flowDetail = new FlowDetail();
            FlowMaster flowMaster = base.genericMgr.FindById<FlowMaster>(id);
            flowDetail.LocationTo = flowMaster.LocationTo;
            flowDetail.Flow = id;
            return PartialView(flowDetail);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_ProductionFlow_Edit")]
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
                    flowDetail.BaseUom = base.genericMgr.FindById<Item>(flowDetail.Item).Uom;
                    flowMgr.CreateFlowDetail(flowDetail);
                    SaveSuccessMessage(Resources.SCM.FlowDetail.FlowDetail_Added);
                    return RedirectToAction("_Detail/" + flowDetail.Flow);
                }
            }
            return PartialView(flowDetail);
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_ProductionFlow_Edit")]
        public ActionResult btnDel(int? id, string Flow)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
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
        [SconitAuthorize(Permissions = "Url_ProductionFlow_Edit")]
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
        [SconitAuthorize(Permissions = "Url_ProductionFlow_Edit")]
        public ActionResult _DetailEdit(FlowDetail flowDetail, int? id)
        {
            if (ModelState.IsValid)
            {
                flowDetail.BaseUom = base.genericMgr.FindById<Item>(flowDetail.Item).Uom;
                flowMgr.UpdateFlowDetail(flowDetail);
                SaveSuccessMessage(Resources.SCM.FlowDetail.FlowDetail_Updated);
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
        [SconitAuthorize(Permissions = "Url_ProductionFlow_Edit")]
        public ActionResult _Binding(GridCommand command, FlowBindModel searchModel, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            ViewBag.flow = id;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProductionFlow_Edit")]
        public ActionResult _Binded(GridCommand command, FlowBindModel searchModel, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            ViewBag.flow = id;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProductionFlow_View")]
        public ActionResult _AjaxBinding(GridCommand command, FlowBindModel searchModel, string id)
        {
            SearchStatementModel searchStatementModel = PrepareBindSearchStatement(command, searchModel, id);
            return PartialView(GetAjaxPageData<FlowBinding>(searchStatementModel, command));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProductionFlow_View")]
        public ActionResult _AjaxBinded(GridCommand command, FlowBindModel searchModel, string id)
        {
            SearchStatementModel searchStatementModel = PrepareBindedSearchStatement(command, searchModel, id);
            return PartialView(GetAjaxPageData<FlowBinding>(searchStatementModel, command));
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProductionFlow_Edit")]
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
        [SconitAuthorize(Permissions = "Url_ProductionFlow_Edit")]
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

        [SconitAuthorize(Permissions = "Url_ProductionFlow_Edit")]
        public ActionResult _BindingNew(string id)
        {
            FlowMaster flow = base.genericMgr.FindById<FlowMaster>(id);
            FlowBinding flowBinding = new FlowBinding();
            flowBinding.MasterFlow = flow;
            return PartialView(flowBinding);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_ProductionFlow_Edit")]
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
                    SaveSuccessMessage(Resources.ORD.OrderBinding.OrderBinding_Added);
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

        #region FacilitySearch

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProductionFlow_Edit")]
        public ActionResult _FacilitySearch(string id)
        {
            ViewBag.ProductLine = id;
            return PartialView();
        }


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProductionFlow_View")]
        public ActionResult _Facility(string code, string ProductLine)
        {
            ViewBag.code = code;
            ViewBag.ProductLine = ProductLine;
            TempData["Code"] = code;
            return PartialView();

        }


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_StockTake_Edit")]
        public ActionResult _SelectFacility(string code, string ProductLine)
        {
            ViewBag.ProductLine = ProductLine;
            IList<ProductLineFacility> ProductLineFacilityList = base.genericMgr.FindAll<ProductLineFacility>("from ProductLineFacility as p where p.ProductLine=?", ProductLine);
            if (!string.IsNullOrEmpty(code))
            {
                ProductLineFacilityList = base.genericMgr.FindAll<ProductLineFacility>("from ProductLineFacility as p where p.Code=? and p.ProductLine=?", new object[] {code
                ,ProductLine});
            }
            return PartialView(new GridModel(ProductLineFacilityList));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_StockTake_Edit")]
        public ActionResult _InsertFacility(string id, bool IsActive, string ProductLine)
        {
            if (ModelState.IsValid)
            {
                IList<ProductLineFacility> ProductLineFacilityByCode = base.genericMgr.FindAll<ProductLineFacility>("from ProductLineFacility as p where p.Code=?", id); ;
                if (ProductLineFacilityByCode.Count > 0)
                {
                    SaveSuccessMessage("设备已经存在,重新填写");
                }
                else
                {
                    ProductLineFacility ProductLineFacility = new ProductLineFacility();
                    ProductLineFacility.IsActive = IsActive;
                    ProductLineFacility.Code = id;
                    ProductLineFacility.ProductLine = ProductLine;
                    base.genericMgr.Create(ProductLineFacility);
                    SaveSuccessMessage("设备添加成功");
                }
            }
            IList<ProductLineFacility> ProductLineFacilityList = base.genericMgr.FindAll<ProductLineFacility>("from ProductLineFacility as p where p.ProductLine=?", ProductLine);
            return PartialView(new GridModel(ProductLineFacilityList));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_StockTake_Edit")]
        public ActionResult _DeleteFacility(string id, string ProductLine)
        {

            base.genericMgr.DeleteById<ProductLineFacility>(id);
            SaveSuccessMessage("设备删除成功");
            IList<ProductLineFacility> ProductLineFacilityList = base.genericMgr.FindAll<ProductLineFacility>("from ProductLineFacility as p where p.ProductLine=?", ProductLine);
            return PartialView(new GridModel(ProductLineFacilityList));
        }


        [GridAction]
        [SconitAuthorize(Permissions = "Url_StockTake_Edit")]
        public ActionResult _UpdateFacility(string id, bool IsActive)
        {

            ProductLineFacility ProductLineFacility = base.genericMgr.FindById<ProductLineFacility>(id);
            ProductLineFacility.IsActive = IsActive;
            base.genericMgr.Update(ProductLineFacility);
            SaveSuccessMessage("设备修改成功");
            IList<ProductLineFacility> ProductLineFacilityList = base.genericMgr.FindAll<ProductLineFacility>("from ProductLineFacility as p where p.ProductLine=?", ProductLineFacility.ProductLine);
            return PartialView(new GridModel(ProductLineFacilityList));
        }

        #endregion

        #region 发单时间计算
        public ActionResult ShipOrderTimeIndex()
        {
            return View();
        }

        public DateTime GetShipOrderTime(DateTime windowTime, double leadTimeMinutes, string region, string calcuType)
        {
            bool isChangeWindowTime = false;
            var workingCalendars = this.genericMgr.FindAll<WorkingCalendar>(" select w from WorkingCalendar as w where w.Region=? and w.WorkingDate=? and w.Type=? ", new object[] { region, windowTime.Date,com.Sconit.CodeMaster.WorkingCalendarType.Work });
            if (workingCalendars == null || workingCalendars.Count == 0)
            {
                return GetShipOrderTime(windowTime.AddDays(-1), leadTimeMinutes, region, calcuType);
            }
            var shiftDet = this.genericMgr.FindAll<ShiftDetail>(" select s from ShiftDetail as s where s.Shift=? order by s.Sequence desc ", new object[] { workingCalendars.FirstOrDefault().Shift });
            if (shiftDet != null || shiftDet.Count > 0)
            {
                if (calcuType == "0")
                {
                    for (int i = 0; i < shiftDet.Count;i++ )
                    {
                        
                        var det=shiftDet[i];
                        DateTime prevTime = this.ParseDateTime(det.StartTime, windowTime);
                        DateTime nextTime = this.ParseDateTime(det.EndTime, windowTime);
                        if (nextTime < prevTime)
                        {
                            nextTime = nextTime.AddDays(1);
                        }
                        if (windowTime.AddDays(1) == nextTime)
                        {
                            windowTime = windowTime.AddDays(1);
                        }
                        if (windowTime >= prevTime && windowTime <= nextTime)
                        {
                            double minutes = (windowTime - prevTime).Minutes + (windowTime - prevTime).Hours*60;
                            leadTimeMinutes = leadTimeMinutes - minutes;
                            if (leadTimeMinutes > 0 && (i + 1) < shiftDet.Count)
                            {
                                var prevDet = shiftDet[i + 1];
                                windowTime = this.ParseDateTime(prevDet.EndTime, windowTime);
                                if (windowTime > prevTime)
                                {
                                    windowTime = windowTime.AddDays(-1);
                                    isChangeWindowTime = true;
                                }
                                continue;
                                //return GetShipOrderTime(prevTime, leadTime, region, calcuType);
                            }
                            else if (leadTimeMinutes > 0 && (i + 1) == shiftDet.Count)
                            {
                                windowTime=isChangeWindowTime ? windowTime : windowTime.AddDays(-1);
                                workingCalendars = this.genericMgr.FindAll<WorkingCalendar>(" select w from WorkingCalendar as w where w.Region=? and w.WorkingDate=? and w.Type=? ", new object[] { region, windowTime.Date, com.Sconit.CodeMaster.WorkingCalendarType.Work });
                                if (workingCalendars == null || workingCalendars.Count == 0)
                                {
                                    return GetShipOrderTime(windowTime.AddDays(-1), leadTimeMinutes, region, calcuType);
                                }
                                shiftDet = this.genericMgr.FindAll<ShiftDetail>(" select s from ShiftDetail as s where s.Shift=? order by s.Sequence desc ", new object[] { workingCalendars.FirstOrDefault().Shift });
                                if (shiftDet == null || shiftDet.Count == 0)
                                {
                                    return GetShipOrderTime(windowTime.AddDays(-1), leadTimeMinutes, region, calcuType);
                                }
                                else
                                {
                                    DateTime thisWindowTime = this.ParseDateTime(shiftDet.FirstOrDefault().EndTime, windowTime);
                                    if (thisWindowTime > windowTime) thisWindowTime = thisWindowTime.AddDays(-1);
                                    return GetShipOrderTime(thisWindowTime, leadTimeMinutes, region, calcuType);
                                }
                            }
                            else
                            {
                                return windowTime.AddMinutes(-(leadTimeMinutes + minutes));
                            }
                        }
                    }
                }
                else
                {
                    shiftDet = shiftDet.OrderBy(s => s.Sequence).ToList();
                    for (int i = 0; i < shiftDet.Count; i++)
                    {
                        var det = shiftDet[i];
                        DateTime prevTime = this.ParseDateTime(det.StartTime, windowTime);
                        DateTime nextTime = this.ParseDateTime(det.EndTime, windowTime);
                        if (nextTime < prevTime) nextTime = nextTime.AddDays(1);
                        
                        if (windowTime >= prevTime && windowTime <= nextTime)
                        {
                            double minutes = (nextTime - windowTime).Minutes + (nextTime - windowTime).Hours * 60;
                            leadTimeMinutes = leadTimeMinutes - minutes;
                            if (leadTimeMinutes > 0 && (i + 1) < shiftDet.Count)
                            {
                                var nextDet = shiftDet[i + 1];
                                windowTime = this.ParseDateTime(nextDet.StartTime, windowTime);
                                if (windowTime < nextTime)
                                {
                                    windowTime = windowTime.AddDays(1);
                                    isChangeWindowTime = true;
                                }
                                continue;
                                //return GetShipOrderTime(prevTime, leadTime, region, calcuType);
                            }
                            else if (leadTimeMinutes > 0 && (i + 1) == shiftDet.Count)
                            {
                                windowTime =isChangeWindowTime?windowTime: windowTime.AddDays(1);
                                workingCalendars = this.genericMgr.FindAll<WorkingCalendar>(" select w from WorkingCalendar as w where w.Region=? and w.WorkingDate=? and w.Type=? ", new object[] { region, windowTime.Date, com.Sconit.CodeMaster.WorkingCalendarType.Work });
                                if (workingCalendars == null || workingCalendars.Count == 0)
                                {
                                    return GetShipOrderTime(windowTime.AddDays(1), leadTimeMinutes, region, calcuType);
                                }
                                shiftDet = this.genericMgr.FindAll<ShiftDetail>(" select s from ShiftDetail as s where s.Shift=? order by s.Sequence asc ", new object[] { workingCalendars.FirstOrDefault().Shift });
                                if (shiftDet == null || shiftDet.Count == 0)
                                {
                                    return GetShipOrderTime(windowTime.AddDays(1), leadTimeMinutes, region, calcuType);
                                }
                                else
                                {
                                    DateTime thisWindowTime = this.ParseDateTime(shiftDet.FirstOrDefault().StartTime, windowTime);
                                    if (thisWindowTime < windowTime) thisWindowTime = thisWindowTime.AddDays(1);
                                    return GetShipOrderTime(thisWindowTime, leadTimeMinutes, region, calcuType);
                                }
                            }
                            else
                            {
                                return windowTime.AddMinutes(leadTimeMinutes + minutes);
                            }
                        }
                    }
                }
            }
            else
            {
                return GetShipOrderTime(windowTime.AddDays(-1), leadTimeMinutes, region, calcuType);
            }
            return System.DateTime.Now;
        }

        private DateTime ParseDateTime(string time, DateTime windowTime)
        {
            if (string.IsNullOrWhiteSpace(time))
                return DateTime.MinValue;

            var t = time.Split(':');
            if (t.Length != 2)
                return DateTime.MinValue;

            int h;
            int.TryParse(t[0], out h);
            if (h >= 24)
                return DateTime.MinValue;

            int m;
            int.TryParse(t[1], out m);

            if (h > 59)
                return DateTime.MinValue;

            return new DateTime(windowTime.Year, windowTime.Month, windowTime.Day, h, m, 0);
        }

        #region 获取发单时间
        public ActionResult ImportShipOrderTime(IEnumerable<HttpPostedFileBase> attachments)
        {
            try
            {
                IList<FlowStrategy> flowStrategyList = new List<FlowStrategy>();
                foreach (var file in attachments)
                {

                    if (file.InputStream.Length == 0)
                    {
                        throw new BusinessException("Import.Stream.Empty");
                    }

                    HSSFWorkbook workbook = new HSSFWorkbook(file.InputStream);

                    ISheet sheet = workbook.GetSheetAt(0);
                    IEnumerator rows = sheet.GetRowEnumerator();

                    BusinessException businessException = new BusinessException();

                    ImportHelper.JumpRows(rows, 10);

                    #region 列定义
                    int colWindowTime = 1;//窗口时间
                    int colRegion = 2;//区域
                    int colleadTime = 3;//发单提前期
                    int colWinTimeDiff = 4;// 进料提前期
                    int colSafeTime = 5;// 安全提前期
                    int colCalculateType = 6;//计算类型
                    #endregion

                    DateTime dateTimeNow = DateTime.Now;

                    int rowCount = 10;


                    while (rows.MoveNext())
                    {
                        rowCount++;
                        HSSFRow row = (HSSFRow)rows.Current;
                        if (!ImportHelper.CheckValidDataRow(row, 1, 6))
                        {
                            break;//边界
                        }
                        FlowStrategy flowStrategy = new FlowStrategy();
                        DateTime windowTime = System.DateTime.Now;
                        string regionCode = string.Empty;
                        decimal leadTime = 0;
                        decimal winTimeDiff = 0;
                        decimal safeTime = 0;
                        string calculateType = string.Empty;

                        #region 读取数据
                        #region 计算类型
                        calculateType = ImportHelper.GetCellStringValue(row.GetCell(colCalculateType));
                        if (string.IsNullOrWhiteSpace(calculateType))
                        {
                            businessException.AddMessage(string.Format("第{0}行：计算类型不能为空。", rowCount));
                            continue;
                        }
                        else
                        {
                            flowStrategy.CalculateType = calculateType;
                        }
                        #endregion

                        #region 读取区域
                        regionCode = ImportHelper.GetCellStringValue(row.GetCell(colRegion));
                        if (string.IsNullOrWhiteSpace(regionCode))
                        {
                            businessException.AddMessage(string.Format("第{0}行：区域不能为空。", rowCount));
                            flowStrategyList.Add(flowStrategy);
                            continue;
                        }
                        else
                        {
                            IList<Region> regionList = genericMgr.FindAll<Region>("select l from Region as l where l.Code=?", regionCode);
                            if (regionList != null && regionList.Count > 0)
                            {
                                flowStrategy.Region = regionCode;
                            }
                            else
                            {
                                businessException.AddMessage(string.Format("第{0}行：区域{1}不存在", rowCount, regionCode));
                                flowStrategyList.Add(flowStrategy);
                                continue;
                            }
                        }

                        #endregion

                        #region 窗口时间

                        string readWindowTime = ImportHelper.GetCellStringValue(row.GetCell(colWindowTime));
                        if (string.IsNullOrWhiteSpace(readWindowTime))
                        {
                            businessException.AddMessage(string.Format("第{0}行：窗口时间不能为空。", rowCount));
                            flowStrategyList.Add(flowStrategy);
                            continue;
                        }
                        else
                        {
                            if (!DateTime.TryParse(readWindowTime, out windowTime))
                            {
                                businessException.AddMessage(string.Format("第{0}行：窗口时间{1}填写有误", rowCount, readWindowTime));
                                flowStrategyList.Add(flowStrategy);
                                continue;
                            }
                            var workingCalendars = this.genericMgr.FindAll<WorkingCalendar>(" select w from WorkingCalendar as w where w.Region=? and w.WorkingDate=? ", new object[] { regionCode, windowTime.Date });
                            if (workingCalendars != null && workingCalendars.Count > 0)
                            {
                                if (workingCalendars.First().Type == com.Sconit.CodeMaster.WorkingCalendarType.Rest)
                                {
                                    businessException.AddMessage(string.Format("第{0}行：窗口时间{1}是休息时间，请确认。", rowCount, readWindowTime));
                                    flowStrategyList.Add(flowStrategy);
                                    continue;
                                }
                                var shiftDet = this.genericMgr.FindAll<ShiftDetail>(" select s from ShiftDetail as s where s.Shift=? ", new object[] { workingCalendars.FirstOrDefault().Shift });
                                if (shiftDet != null || shiftDet.Count > 0)
                                {
                                    DateTime nowTime = windowTime;
                                    bool isTure = false;
                                    foreach (var det in shiftDet)
                                    {
                                        DateTime prevTime = this.ParseDateTime(det.StartTime, windowTime);
                                        DateTime nextTime = this.ParseDateTime(det.EndTime, windowTime);
                                        if (nextTime < prevTime) nextTime = nextTime.AddDays(1);
                                        if (nowTime >= prevTime && nowTime <= nextTime)
                                        {
                                            isTure = true;
                                            break;
                                        }

                                    }
                                    if (!isTure)
                                    {
                                        businessException.AddMessage(string.Format("窗口时间{0}是休息时间，请确认。", windowTime));
                                        flowStrategyList.Add(flowStrategy);
                                        continue;
                                    }
                                }
                                else
                                {
                                    businessException.AddMessage(string.Format("没有找到区域的工作日历。"));
                                }
                            }
                        }
                        flowStrategy.WindowTime = windowTime;
                        #endregion

                        if (flowStrategy.CalculateType == "0")
                        {
                            #region 发单提前期
                            try
                            {
                                string leadTimeRead = ImportHelper.GetCellStringValue(row.GetCell(colleadTime));
                                leadTime = Convert.ToDecimal(leadTimeRead);
                                flowStrategy.LeadTime = leadTime;
                                if (windowTime <= windowTime.Date.AddHours(8) && windowTime >= windowTime.Date) { windowTime = windowTime.AddDays(-1); }
                                flowStrategy.ShipOrderTime = GetShipOrderTime(windowTime, Convert.ToDouble(leadTime) * 60, regionCode, flowStrategy.CalculateType);
                                //IList<WorkingCalendarView> workingCalendarViewList = this.workingCalendarMgr.GetWorkingCalendarView(regionCode, windowTime.Add(TimeSpan.FromDays(-7)), windowTime);
                                //flowStrategy.ShipOrderTime = this.workingCalendarMgr.GetStartTimeAtWorkingDate(windowTime, Convert.ToDouble(leadTime), CodeMaster.TimeUnit.Hour, regionCode, workingCalendarViewList);
                            }
                            catch
                            {
                                businessException.AddMessage(string.Format("第{0}行：发单提前期{1}填写有误", rowCount, leadTime));
                                flowStrategyList.Add(flowStrategy);
                                continue;
                            }
                            #endregion
                        }
                        else if (flowStrategy.CalculateType == "1")
                        {
                            #region 进料提前期
                            try
                            {
                                winTimeDiff = Convert.ToDecimal(ImportHelper.GetCellStringValue(row.GetCell(colWinTimeDiff)));
                                flowStrategy.WinTimeDiff = winTimeDiff;
                                if (windowTime <= windowTime.Date.AddHours(8) && windowTime >= windowTime.Date) { windowTime = windowTime.AddDays(-1); }
                                flowStrategy.ReqStartTime = GetShipOrderTime(windowTime, Convert.ToDouble(winTimeDiff) * 60, regionCode, flowStrategy.CalculateType);
                                //IList<WorkingCalendarView> workingCalendarViewList = this.workingCalendarMgr.GetWorkingCalendarView(regionCode, windowTime, windowTime.Add(TimeSpan.FromDays(7)));
                                //flowStrategy.ShipOrderTime = this.workingCalendarMgr.GetStartTimeAtWorkingDate(windowTime, Convert.ToDouble(-winTimeDiff), CodeMaster.TimeUnit.Hour, regionCode, workingCalendarViewList);

                            }
                            catch
                            {
                                businessException.AddMessage(string.Format("第{0}行：进料提前期{1}填写有误", rowCount, winTimeDiff));
                                flowStrategyList.Add(flowStrategy);
                                continue;
                            }
                            #endregion

                        }
                        else if (flowStrategy.CalculateType == "2")
                        {
                            #region 进料提前期
                            try
                            {
                                winTimeDiff = Convert.ToDecimal(ImportHelper.GetCellStringValue(row.GetCell(colWinTimeDiff)));
                                flowStrategy.WinTimeDiff = winTimeDiff;
                            }
                            catch
                            {
                                businessException.AddMessage(string.Format("第{0}行：进料提前期{1}填写有误", rowCount, winTimeDiff));
                                flowStrategyList.Add(flowStrategy);
                                continue;
                            }
                            #endregion

                            #region 安全提前期
                            try
                            {
                                safeTime = Convert.ToDecimal(ImportHelper.GetCellStringValue(row.GetCell(colSafeTime)));
                                flowStrategy.SafeTime = safeTime;
                            }
                            catch
                            {
                                businessException.AddMessage(string.Format("第{0}行：安全提前期{1}填写有误", rowCount, safeTime));
                                flowStrategyList.Add(flowStrategy);
                                continue;
                            }
                            #endregion

                            if (windowTime <= windowTime.Date.AddHours(8) && windowTime >= windowTime.Date) { windowTime = windowTime.AddDays(-1); }
                            flowStrategy.ReqEndTime = GetShipOrderTime(windowTime, Convert.ToDouble(flowStrategy.WinTimeDiff + flowStrategy.SafeTime) * 60, regionCode, flowStrategy.CalculateType);
                        }

                        #region 填充数据
                        flowStrategyList.Add(flowStrategy);
                        #endregion

                    }

                        #endregion
                    TempData["ShipOrderTimeView"] = flowStrategyList;

                    if (businessException.HasMessage)
                    {
                        throw businessException;
                    }
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

        public ActionResult ShipOrderTimeView()
        {
            return PartialView((IList<FlowStrategy>)TempData["ShipOrderTimeView"]);
        }

        #endregion

    }
}
