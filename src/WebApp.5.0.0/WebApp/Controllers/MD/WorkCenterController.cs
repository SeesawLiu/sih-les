using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.PRD;
using com.Sconit.Service;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels.MD;
using com.Sconit.Web.Util;

namespace com.Sconit.Web.Controllers.MD
{
    public class WorkCenterController : WebAppBaseController
    {
        private static string selectCountStatement = "select count(*) from WorkCenter as u";

        private static string selectStatement = "select u from WorkCenter as u";

        private static string duiplicateVerifyStatement = @"select count(*) from WorkCenter as u where u.Code = ? ";

        #region public actions
        /// <summary>
        /// Index action for WorkCenter controller
        /// </summary>
        /// <returns>Index view</returns>
        [SconitAuthorize(Permissions = "Url_WorkCenter_View")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// List action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">WorkCenter Search model</param>
        /// <returns>return the result view</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_WorkCenter_View")]
        public ActionResult List(GridCommand command, WorkCenterSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        /// <summary>
        ///  AjaxList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">WorkCenter Search Model</param>
        /// <returns>return the result action</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_WorkCenter_View")]
        public ActionResult _AjaxList(GridCommand command, WorkCenterSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<WorkCenter>(searchStatementModel, command));
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <returns>New view</returns>
        [SconitAuthorize(Permissions = "Url_WorkCenter_View")]
        public ActionResult New()
        {
            return View();
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <param name="WorkCenter">WorkCenter Model</param>
        /// <returns>return the result view</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_WorkCenter_View")]
        public ActionResult New(WorkCenter WorkCenter)
        {
            if (ModelState.IsValid)
            {
                if (base.genericMgr.FindAll<long>(duiplicateVerifyStatement, new object[] { WorkCenter.Code })[0] > 0)
                {
                    SaveErrorMessage(Resources.MD.WorkCenter.WorkCenterErrors_Existing_Code, WorkCenter.Code);
                }
                else
                {
                    base.genericMgr.Create(WorkCenter);
                    SaveSuccessMessage(Resources.MD.WorkCenter.WorkCenter_Added);
                    return RedirectToAction("Edit/" + WorkCenter.Code);
                }
            }

            return View(WorkCenter);
        }

        /// <summary>
        /// Edit view
        /// </summary>
        /// <param name="id">WorkCenter id for edit</param>
        /// <returns>return the result view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_WorkCenter_View")]
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            else
            {
                WorkCenter WorkCenter = base.genericMgr.FindById<WorkCenter>(id);
                return View(WorkCenter);
            }
        }

        /// <summary>
        /// Edit view
        /// </summary>
        /// <param name="WorkCenter">WorkCenter Model</param>
        /// <returns>return the result view</returns>
        [SconitAuthorize(Permissions = "Url_WorkCenter_View")]
        public ActionResult Edit(WorkCenter WorkCenter)
        {
            if (ModelState.IsValid)
            {
                base.genericMgr.Update(WorkCenter);
                SaveSuccessMessage(Resources.MD.WorkCenter.WorkCenter_Updated);
            }

            return View(WorkCenter);
        }

        /// <summary>
        /// Delete action
        /// </summary>
        /// <param name="id">WorkCenter id for delete</param>
        /// <returns>return to List action</returns>
        [SconitAuthorize(Permissions = "Url_WorkCenter_View")]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }

            var prodLineWorkCenterList = base.genericMgr.FindAll<ProdLineWorkCenter>("from ProdLineWorkCenter p where p.WorkCenter=?", id);
            if (prodLineWorkCenterList != null && prodLineWorkCenterList.Count > 0)
            {
                SaveErrorMessage("工作中心被整车生产线{0}引用，不可以删除。", String.Join(",", prodLineWorkCenterList.Select(c => c.Flow).Distinct()));
                return RedirectToAction("Edit/" + id);
            }

            base.genericMgr.DeleteById<WorkCenter>(id);
            SaveSuccessMessage(Resources.MD.WorkCenter.WorkCenter_Deleted);
            return RedirectToAction("List");
        }
        #endregion

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">WorkCenter Search Model</param>
        /// <returns>return WorkCenter search model</returns>
        private SearchStatementModel PrepareSearchStatement(GridCommand command, WorkCenterSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("Code", searchModel.SearchCode, HqlStatementHelper.LikeMatchMode.Start, "u", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Location", searchModel.SearchLocation, HqlStatementHelper.LikeMatchMode.Start, "u", ref whereStatement, param);

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