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

namespace com.Sconit.Web.Controllers.SCM
{
    public class ProductionFlowController : WebAppBaseController
    {
        public IFlowMgr flowMgr { get; set; }

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




    }
}
