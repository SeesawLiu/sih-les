/// <summary>
/// Summary description for LocationController
/// </summary>
namespace com.Sconit.Web.Controllers.PRD
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Security;
    using com.Sconit.Entity.PRD;
    using com.Sconit.Entity.SYS;
    using com.Sconit.Service;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Models.SearchModels.PRD;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;

    /// <summary>
    /// This controller response to control the Routing.
    /// </summary>
    public class RoutingController : WebAppBaseController
    {
        #region Properties   
        public IRoutingMgr RoutingMgr { get; set; }
        #endregion

        /// <summary>
        /// hql to get count of the RoutingMaster
        /// </summary>
        private static string selectCountStatement = "select count(*) from RoutingMaster as r";

        /// <summary>
        /// hql to get all of the RoutingMaster
        /// </summary>
        private static string selectStatement = "select r from RoutingMaster as r";

        /// <summary>
        /// hql to get count of the RoutingMaster by RoutingMaster's code
        /// </summary>
        private static string duiplicateVerifyStatement = @"select count(*) from RoutingMaster as r where r.Code = ?";

        /// <summary>
        /// hql to get count of the RoutingDetail
        /// </summary>
        private static string routingDetailSelectCountStatement = "select count(*) from RoutingDetail as r";

        /// <summary>
        /// hql to get total of the RoutingDetail
        /// </summary>
        private static string routingDetailSelectStatement = "select r from RoutingDetail as r";

        /// <summary>
        /// hql to get count of the RoutingDetail by code
        /// </summary>
        private static string routingDetailDuiplicateVerifyStatement = @"select count(*) from RoutingDetail as r where r.Id = ?";

        /// <summary>
        /// hql 
        /// </summary>
        private static string routingDetailIsExistVerifyStatement = @"select count(*) from RoutingDetail as r where r.Routing = ? and r.Operation = ? and r.OpReference = ?";

        private static string routingDetailVerifyStatement = @"select r from RoutingDetail as r where r.Routing = ? and r.Operation = ? ";

        #region RoutingMaster
        /// <summary>
        /// Index action for Routing controller
        /// </summary>
        /// <returns>Index view</returns>
        [SconitAuthorize(Permissions = "Url_Routing_View")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// List acion
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">RoutingMaster Search Model</param>
        /// <returns>return to the result action</returns>
        [GridAction]
        [SconitAuthorize(Permissions = "Url_Routing_View")]
        public ActionResult RoutingMasterList(GridCommand command, RoutingMasterSearchModel searchModel)
        {

            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        /// <summary>
        /// AjaxList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">RoutingMaster Search Model</param>
        /// <returns>return to the result action</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Routing_View")]
        public ActionResult _AjaxList(GridCommand command, RoutingMasterSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<RoutingMaster>(searchStatementModel, command));
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <returns>RoutingMasterNew view</returns>
        [SconitAuthorize(Permissions = "Url_Routing_Edit")]
        public ActionResult RoutingMasterNew()
        {
            return View();
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <param name="routingMaster">RoutingMaster model</param>
        /// <returns>return to edit view</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_Routing_Edit")]
        public ActionResult RoutingMasterNew(RoutingMaster routingMaster)
        {
            if (ModelState.IsValid)
            {
                if (base.genericMgr.FindAll<long>(duiplicateVerifyStatement, new object[] { routingMaster.Code })[0] > 0)
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Existing_Code, routingMaster.Code);
                }
                else
                {
                    base.genericMgr.Create(routingMaster);
                    SaveSuccessMessage(Resources.PRD.Routing.RoutingMaster_Added);
                    return RedirectToAction("RoutingList/" + routingMaster.Code);
                }
            }

            return View(routingMaster);
        }

        /// <summary>
        /// Edit action
        /// </summary>
        /// <param name="id">RoutingMaster id for edit</param>
        /// <returns>return to edit view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_Routing_Edit")]
        public ActionResult RoutingList(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                ViewBag.RoutingMasterCode = id;
                return View("RoutingList", string.Empty, id);
            }
        }

        /// <summary>
        /// Edit action
        /// </summary>
        /// <param name="id">location id for edit</param>
        /// <returns>return to the result view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_Routing_Edit")]
        public ActionResult RoutingMasterEdit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                RoutingMaster routingMaster = base.genericMgr.FindById<RoutingMaster>(id);
                return PartialView(routingMaster);
            }
        }

        /// <summary>
        /// Edit action
        /// </summary>
        /// <param name="routingMaster">RoutingMaster model</param>
        /// <returns>return to edit action</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_Routing_Edit")]
        public ActionResult RoutingMasterEdit(RoutingMaster routingMaster)
        {
            if (ModelState.IsValid)
            {
                base.genericMgr.Update(routingMaster);
                SaveSuccessMessage(Resources.PRD.Routing.RoutingMaster_Updated);
            }

            //return new RedirectToRouteResult(new RouteValueDictionary  
            //                                       { 
            //                                           { "action", "RoutingList" }, 
            //                                           { "controller", "Routing" },
            //                                           { "id", routingMaster.Code }
            //                                       });
            return PartialView(routingMaster);
        }

        /// <summary>
        /// Delete action
        /// </summary>
        /// <param name="id">RoutingMaster id for delete</param>
        /// <returns>return to list action</returns>
        [SconitAuthorize(Permissions = "Url_Routing_Delete")]
        public ActionResult RoutingMasterDelete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                RoutingMgr.DeleteRouting(id);
                //base.genericMgr.DeleteById<RoutingMaster>(id);
                SaveSuccessMessage(Resources.PRD.Routing.RoutingMaster_Deleted);
                return RedirectToAction("RoutingMasterList");
            }
        }
        #endregion

        #region RoutingDetail

        /// <summary>
        /// RoutingDetailList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">RoutingDetail Search Model</param>
        /// <param name="routingMasterCode">routingMaster Code</param>
        /// <returns>return to the result view</returns>
        [GridAction]
        [SconitAuthorize(Permissions = "Url_Routing_View")]
        public ActionResult RoutingDetailResult(GridCommand command, RoutingDetailSearchModel searchModel)
        {

            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        /// <summary>
        /// _AjaxRoutingDetailList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">RoutingDetail Search Model</param>
        /// <param name="routingMasterCode">routingMaster Code</param>
        /// <returns>return to the result view</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Routing_View")]
        public ActionResult _AjaxRoutingDetailList(GridCommand command, RoutingDetailSearchModel searchModel)
        {

            SearchStatementModel searchStatementModel = this.RoutingDetailPrepareSearchStatement(command, searchModel, searchModel.routingMasterCode);
            return PartialView(GetAjaxPageData<RoutingDetail>(searchStatementModel, command));
        }

        /// <summary>
        /// RoutingDetailNew action
        /// </summary>
        /// <param name="routingMasterCode">RoutingDetail routingMasterCode for new</param>
        /// <returns>RoutingDetailNew view</returns>
        [SconitAuthorize(Permissions = "Url_Routing_Edit")]
        public ActionResult RoutingDetailNew(string routingMasterCode)
        {
            RoutingDetail routingDetail = new RoutingDetail();
            routingDetail.Routing = routingMasterCode;
            return PartialView(routingDetail);
        }

        /// <summary>
        /// RoutingDetailNew action
        /// </summary>
        /// <param name="routingDetail">RoutingDetail Model</param>
        /// <returns>return to the result view</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_Routing_Edit")]
        public ActionResult RoutingDetailNew(RoutingDetail routingDetail)
        {
            if (ModelState.IsValid)
            {
                if (base.genericMgr.FindAll<long>(routingDetailDuiplicateVerifyStatement, new object[] { routingDetail.Id })[0] > 0)
                {
                    SaveErrorMessage(Resources.ErrorMessage.Errors_Existing_Code);
                }
                else if (base.genericMgr.FindAll<long>(routingDetailIsExistVerifyStatement,
                         new object[] { routingDetail.Routing, routingDetail.Operation, routingDetail.OpReference })[0] > 0)
                {
                    SaveErrorMessage(Resources.PRD.Routing.Errors_ExistingRoutingDetail);
                }
                else if (routingDetail.IsReport && routingDetail.WorkCenter == null)
                {
                    SaveErrorMessage(Resources.PRD.Routing.Errors_AutoReportMustHaveWorkCenter);
                }
                else
                {
                    var sameRoutingDets = base.genericMgr.FindAll<RoutingDetail>(routingDetailVerifyStatement, new object[] { routingDetail.Routing, routingDetail.Operation });
                    var sameDetail = sameRoutingDets.FirstOrDefault(c => c.WorkCenter != routingDetail.WorkCenter || c.IsReport != routingDetail.IsReport);
                    if (sameDetail != null)
                    {
                        SaveErrorMessage(Resources.PRD.Routing.RoutingMaster_ConfirmeSameRoutingAndOp,
                            routingDetail.Routing, routingDetail.Operation.ToString(), sameDetail.WorkCenter, sameDetail.IsReport ? Resources.Global.True : Resources.Global.False);
                    }
                    else
                    {
                        base.genericMgr.Create(routingDetail);
                        SaveSuccessMessage(Resources.PRD.Routing.RoutingDetail_Added);
                        return RedirectToAction("RoutingDetailEdit/" + routingDetail.Id);
                    }
                }
            }

            return PartialView(routingDetail);
        }

        /// <summary>
        /// RoutingDetailEdit action
        /// </summary>
        /// <param name="id">RoutingDetail id for Edit</param>
        /// <returns>return to the result view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_Routing_Edit")]
        public ActionResult RoutingDetailEdit(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                RoutingDetail routingDetail = base.genericMgr.FindById<RoutingDetail>(id);
                return PartialView(routingDetail);
            }
        }

        /// <summary>
        /// RoutingDetailEdit action
        /// </summary>
        /// <param name="routingDetail">RoutingDetail Model</param>
        /// <returns>return to RoutingDetailEdit action</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_Routing_Edit")]
        public ActionResult RoutingDetailEdit(RoutingDetail routingDetail)
        {
            if (ModelState.IsValid)
            {
                var sameRoutingDets = base.genericMgr.FindAll<RoutingDetail>(routingDetailVerifyStatement + " and Id!=?", new object[] { routingDetail.Routing, routingDetail.Operation, routingDetail.Id });
                var sameDetail = sameRoutingDets.FirstOrDefault(c => c.WorkCenter != routingDetail.WorkCenter || c.IsReport != routingDetail.IsReport);
                if (sameDetail != null)
                {
                    SaveErrorMessage(Resources.PRD.Routing.RoutingMaster_ConfirmeSameRoutingAndOp,
                        routingDetail.Routing, routingDetail.Operation.ToString(), sameDetail.WorkCenter, sameDetail.IsReport ? Resources.Global.True : Resources.Global.False);
                }
                else if (routingDetail.IsReport && routingDetail.WorkCenter == null)
                {
                    SaveErrorMessage(Resources.PRD.Routing.Errors_AutoReportMustHaveWorkCenter);
                }
                else
                {
                    base.genericMgr.Update(routingDetail);
                    SaveSuccessMessage(Resources.PRD.Routing.RoutingDetail_Updated);
                }
            }

            return PartialView(routingDetail);
        }

        /// <summary>
        /// RoutingDetailDelete action
        /// </summary>
        /// <param name="id">RoutingDetail id for delete</param>
        /// <param name="routingMasterCode">routingMaster Code</param>
        /// <returns>return to RoutingDetailList action</returns>
        [SconitAuthorize(Permissions = "Url_Routing_Delete")]
        public ActionResult RoutingDetailDelete(int? id, string routingMasterCode)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<RoutingDetail>(id);
                SaveSuccessMessage(Resources.PRD.Routing.RoutingDetail_Deleted);
                return new RedirectToRouteResult(new RouteValueDictionary
                                                                   {
                                                                       { "action", "RoutingDetailResult" }, 
                                                                       { "controller", "Routing" },
                                                                       {"RoutingMasterCode", routingMasterCode}
                                                                   });
            }
        }
        #endregion

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">RoutingDetail Search Model</param>
        /// <param name="routingMasterCode">routingMaster Code</param>
        /// <returns>Search Statement</returns>
        private SearchStatementModel RoutingDetailPrepareSearchStatement(GridCommand command, RoutingDetailSearchModel searchModel, string routingMasterCode)
        {
            string whereStatement = "where r.Routing = '" + routingMasterCode + "'";
            IList<object> param = new List<object>();
            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "TimeUnitDescription")
                {
                    command.SortDescriptors[0].Member = "TimeUnit";
                }
            }

            HqlStatementHelper.AddEqStatement("Operation", searchModel.Operation, "r", ref whereStatement, param);

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = routingDetailSelectCountStatement;
            searchStatementModel.SelectStatement = routingDetailSelectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();
            return searchStatementModel;
        }

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">RoutingMaster Search Model</param>
        /// <returns>Search Statement</returns>
        private SearchStatementModel PrepareSearchStatement(GridCommand command, RoutingMasterSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("Code", searchModel.Code, HqlStatementHelper.LikeMatchMode.Start, "r", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Name", searchModel.Name, HqlStatementHelper.LikeMatchMode.Start, "r", ref whereStatement, param);
            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "RoutingTimeUnitDescription")
                {
                    command.SortDescriptors[0].Member = "TaktTimeUnit";
                }
            }
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }
    }
}
