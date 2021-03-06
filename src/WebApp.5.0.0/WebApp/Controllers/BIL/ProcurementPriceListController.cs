﻿/// <summary>
/// Summary description for LocationController
/// </summary>
namespace com.Sconit.Web.Controllers.BIL
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Security;
    using com.Sconit.Entity.BIL;
    using com.Sconit.Entity.MD;
    using com.Sconit.Entity.SYS;
    using com.Sconit.Service;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Models.SearchModels.BIL;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;

    /// <summary>
    /// This controller response to control the Location.
    /// </summary>
    public class ProcurementPriceListController : WebAppBaseController
    {
        /// <summary>
        /// hql 
        /// </summary>
        private static string selectCountStatement = "select count(*) from PriceListMaster as p";

        /// <summary>
        /// hql 
        /// </summary>
        private static string selectStatement = "select p from PriceListMaster as p";

        /// <summary>
        /// hql 
        /// </summary>
        private static string duiplicateVerifyStatement = @"select count(*) from PriceListMaster as p where p.Code = ?";

        /// <summary>
        /// hql 
        /// </summary>
        private static string priceListDetailSelectCountStatement = "select count(*) from PriceListDetail as p";

        /// <summary>
        /// hql
        /// </summary>
        private static string priceListDetailSelectStatement = "select p from PriceListDetail as p";

        /// <summary>
        /// hql 
        /// </summary>
        private static string priceListDetailDuiplicateVerifyStatement = @"select count(*) from PriceListDetail as p where p.Id = ?";

        
        #region PriceListMaster
        /// <summary>
        /// Index action for PriceListMaster controller
        /// </summary>
        /// <returns>rediret view</returns>
        [SconitAuthorize(Permissions = "Url_ProcurementPriceList_View")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// List acion
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">PriceListMaster Search Model</param>
        /// <returns>return to the result action</returns>
        [GridAction]
        [SconitAuthorize(Permissions = "Url_ProcurementPriceList_View")]
        public ActionResult List(GridCommand command, PriceListMasterSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        /// <summary>
        /// AjaxList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">PriceListMaster Search Model</param>
        /// <returns>return to the result action</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProcurementPriceList_View")]
        public ActionResult _AjaxList(GridCommand command, PriceListMasterSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<PriceListMaster>(searchStatementModel, command));
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <returns>rediret view</returns>
        [SconitAuthorize(Permissions = "Url_ProcurementPriceList_Edit")]
        public ActionResult New()
        {
            return View();
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <param name="location">PriceListMaster model</param>
        /// <returns>return to edit view</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_ProcurementPriceList_Edit")]
        public ActionResult New(PriceListMaster priceListMaster)
        {
            if (ModelState.IsValid)
            {
                if (base.genericMgr.FindAll<long>(duiplicateVerifyStatement, new object[] { priceListMaster.Code })[0] > 0)
                {
                    SaveErrorMessage(Resources.ErrorMessage.Errors_Existing_Code, priceListMaster.Code);
                }
                else
                {
                    if (base.genericMgr.FindAll<Supplier>("from Supplier where Code=?", priceListMaster.Party).Count < 1)
                    {
                        SaveErrorMessage(Resources.BIL.PriceListMaster.Errors_NotExisting_Party);
                    }
                    else
                    {
                        priceListMaster.Type = com.Sconit.CodeMaster.PriceListType.Procuement;
                        base.genericMgr.Create(priceListMaster);
                        SaveSuccessMessage(Resources.BIL.PriceListMaster.PriceListMaster_Added);
                        return RedirectToAction("Edit/" + priceListMaster.Code);
                    }
                }
            }

            return View(priceListMaster);
        }

        /// <summary>
        /// Edit action
        /// </summary>
        /// <param name="id">PriceListMaster id for edit</param>
        /// <returns>return to edit view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProcurementPriceList_Edit")]
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                return View("Edit", string.Empty, id);
            }
        }

        /// <summary>
        /// Edit action
        /// </summary>
        /// <param name="id">PriceListMaster id for edit</param>
        /// <returns>return to the result view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProcurementPriceList_Edit")]
        public ActionResult _Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                PriceListMaster priceListMaster = base.genericMgr.FindById<PriceListMaster>(id);
                return PartialView(priceListMaster);
            }
        }

        /// <summary>
        /// Edit action
        /// </summary>
        /// <param name="location">PriceListMaster model</param>
        /// <returns>return to edit action</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_ProcurementPriceList_Edit")]
        public ActionResult _Edit(PriceListMaster priceListMaster)
        {
            if (ModelState.IsValid)
            {
                priceListMaster.Type = com.Sconit.CodeMaster.PriceListType.Procuement;
                base.genericMgr.Update(priceListMaster);
                SaveSuccessMessage(Resources.BIL.PriceListMaster.PriceListMaster_Updated);
            }

            return PartialView(priceListMaster);
        }

        /// <summary>
        /// Delete action
        /// </summary>
        /// <param name="id">PriceListMaster id for delete</param>
        /// <returns>return to list action</returns>
        [SconitAuthorize(Permissions = "Url_ProcurementPriceList_Delete")]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<PriceListMaster>(id);
                SaveSuccessMessage(Resources.BIL.PriceListMaster.PriceListMaster_Deleted);
                return RedirectToAction("List");
            }
        }
        #endregion

        #region PriceListDetail
        /// <summary>
        /// _PriceListDetail action
        /// </summary>
        /// <param name="id">PriceList Code</param>
        /// <returns>rediret view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProcurementPriceList_View")]
        public ActionResult _PriceListDetail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                ViewBag.PriceListCode = id;
                return PartialView();
            }
        }

        /// <summary>
        /// PriceListDetailList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">PriceListDetail Search Model</param>
        /// <param name="locationCode">PriceList Code</param>
        /// <returns>return to the result view</returns>
        [GridAction]
        [SconitAuthorize(Permissions = "Url_ProcurementPriceList_View")]
        public ActionResult _PriceListDetailList(GridCommand command, PriceListDetailSearchModel searchModel, string priceListCode)
        {
            ViewBag.LocationCode = priceListCode;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);

            return PartialView();
        }

        /// <summary>
        /// AjaxPriceListDetailList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">PriceListDetail Search Model</param>
        /// <param name="locationCode">PriceList Code</param>
        /// <returns>return to the result view</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProcurementPriceList_View")]
        public ActionResult _AjaxPriceListDetailList(GridCommand command, PriceListDetailSearchModel searchModel, string priceListCode)
        {
            SearchStatementModel searchStatementModel = this.LocationAreaPrepareSearchStatement(command, searchModel, priceListCode);
            GridModel<PriceListDetail> priceListDetailList = GetAjaxPageData<PriceListDetail>(searchStatementModel, command);
            foreach (var priceList in priceListDetailList.Data)
            {
                if (priceList.PriceList != null)
                {
                    priceList.PriceListCode = priceList.PriceList.Code;
                }
            }

            return PartialView(priceListDetailList);
        }

        /// <summary>
        /// PriceListDetailNew action
        /// </summary>
        /// <param name="locationCode">PriceList Code</param>
        /// <returns>rediret view</returns>
        [SconitAuthorize(Permissions = "Url_ProcurementPriceList_Edit")]
        public ActionResult _PriceListDetailNew(string priceListCode)
        {
            ViewBag.PriceListCode = priceListCode;
            return PartialView();
        }

        /// <summary>
        /// PriceListDetailNew action
        /// </summary>
        /// <param name="locationArea">PriceListDetail Model</param>
        /// <param name="locationCode">PriceList Code</param>
        /// <returns>return to the result view</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_ProcurementPriceList_Edit")]
        public ActionResult _PriceListDetailNew(PriceListDetail priceListDetail, string priceListCode)
        {
            if (!string.IsNullOrEmpty(priceListCode))
            {
                ViewBag.PriceListCode = priceListCode;
                PriceListMaster priceListMaster = base.genericMgr.FindById<PriceListMaster>(priceListCode);
                priceListDetail.PriceList = priceListMaster;
                ModelState.Remove("PriceList");
            }
            if (ModelState.IsValid)
            {
                if (base.genericMgr.FindAll<long>(priceListDetailDuiplicateVerifyStatement, new object[] { priceListDetail.Id })[0] > 0)
                {
                    SaveErrorMessage(Resources.ErrorMessage.Errors_Existing_Code, priceListDetail.Id.ToString());
                }
                else
                {
                    bool isError = false;
                    if (base.genericMgr.FindAll<Item>("from Item where Code=?", priceListDetail.Item).Count < 1)
                    {
                        SaveErrorMessage(Resources.BIL.PriceListDetail.Errors_NotExisting_Item);
                        isError = true;
                    }
                    else if(priceListDetail.EndDate!=null)
                    {
                        if (System.DateTime.Compare((System.DateTime)priceListDetail.EndDate, (System.DateTime)priceListDetail.StartDate) < 1)
                        {
                            SaveErrorMessage(Resources.MD.WorkingCalendar.Errors_StartDateGreaterThanEndDate);
                            isError = true;
                        }
                    }
                    
                    if(!isError)
                    {
                        base.genericMgr.Create(priceListDetail);
                        SaveSuccessMessage(Resources.BIL.PriceListDetail.PriceListDetail_Added);
                        //return RedirectToAction("_PriceListDetailList" + priceListCode);
                        return new RedirectToRouteResult(new RouteValueDictionary  
                                                   { 
                                                       { "action", "_PriceListDetailList" }, 
                                                       { "controller", "ProcurementPriceList" },
                                                       { "PriceListCode", priceListCode }
                                                   });
                    }
                }
            }

            return PartialView(priceListDetail);
        }

        /// <summary>
        /// PriceListDetailEdit action
        /// </summary>
        /// <param name="id">PriceListDetail id for Edit</param>
        /// <returns>return to the result view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProcurementPriceList_Edit")]
        public ActionResult _PriceListDetailEdit(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                
                PriceListDetail priceListDetail = base.genericMgr.FindById<PriceListDetail>(id);
                if (priceListDetail.PriceList != null)
                {
                    priceListDetail.PriceListCode = priceListDetail.PriceList.Code;
                }

                return PartialView(priceListDetail);
            }
        }

        /// <summary>
        /// PriceListDetailEdit action
        /// </summary>
        /// <param name="locationArea">PriceListDetail Model</param>
        /// <returns>return to PriceListDetailEdit action</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_ProcurementPriceList_Edit")]
        public ActionResult _PriceListDetailEdit(PriceListDetail priceListDetail)
        {
            if (priceListDetail.PriceListCode != null)
            {
                PriceListMaster priceListMaster = base.genericMgr.FindById<PriceListMaster>(priceListDetail.PriceListCode);
                priceListDetail.PriceList = priceListMaster;
                ModelState.Remove("PriceList");
            }
            
            if (ModelState.IsValid)
            {
                bool isError = false;
                if (priceListDetail.EndDate != null)
                {
                    if (System.DateTime.Compare((System.DateTime)priceListDetail.EndDate, (System.DateTime)priceListDetail.StartDate) < 1)
                    {
                        SaveErrorMessage(Resources.MD.WorkingCalendar.Errors_StartDateGreaterThanEndDate);
                        isError = true;
                    }
                }
                if(!isError)
                {
                    base.genericMgr.Update(priceListDetail);
                    SaveSuccessMessage(Resources.BIL.PriceListDetail.PriceListDetail_Updated);
                }
            }

            return PartialView(priceListDetail);
        }

        /// <summary>
        /// PriceListDetailDelete action
        /// </summary>
        /// <param name="id">PriceListDetail id for delete</param>
        /// <param name="locationCode">PriceList Code</param>
        /// <returns>return to LocationAreaList action</returns>
        [SconitAuthorize(Permissions = "Url_ProcurementPriceList_Delete")]
        public ActionResult _PriceListDetailDelete(int? id, string priceListCode)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<PriceListDetail>(id);
                SaveSuccessMessage(Resources.BIL.PriceListDetail.PriceListDetail_Deleted);
                return new RedirectToRouteResult(new RouteValueDictionary  
                                                   { 
                                                       { "action", "_PriceListDetailList" }, 
                                                       { "controller", "ProcurementPriceList" },
                                                       { "PriceListCode", priceListCode }
                                                   });
            }
        }
        #endregion

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">PriceListDetail Search Model</param>
        /// <param name="locationCode">PriceList Code</param>
        /// <returns>Search Statement</returns>
        private SearchStatementModel LocationAreaPrepareSearchStatement(GridCommand command, PriceListDetailSearchModel searchModel, string priceListDetail)
        {
            string whereLocationStatement = "where p.PriceList = '" + priceListDetail + "'";
            IList<object> param = new List<object>();
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "p", ref whereLocationStatement, param);
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = priceListDetailSelectCountStatement;
            searchStatementModel.SelectStatement = priceListDetailSelectStatement;
            searchStatementModel.WhereStatement = whereLocationStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();
            return searchStatementModel;
        }

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">PriceListMaster Search Model</param>
        /// <returns>Search Statement</returns>
        private SearchStatementModel PrepareSearchStatement(GridCommand command, PriceListMasterSearchModel searchModel)
        {
            string whereStatement = "where p.Type='1'";

            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("Code", searchModel.Code, HqlStatementHelper.LikeMatchMode.Start, "p", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Party", searchModel.Party, "p", ref whereStatement, param);
            
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
