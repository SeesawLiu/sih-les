namespace com.Sconit.Web.Controllers.MD
{
    #region reference
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Security;
    using com.Sconit.Entity.MD;
    using com.Sconit.Entity.SYS;
    using com.Sconit.Service;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Models.SearchModels.MD;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;
    #endregion

    /// <summary>
    /// This controller response to control the PickRule.
    /// </summary>
    public class PickRuleController : WebAppBaseController
    {
        private static string selectCountStatement = "select count(*) from PickRule as u";

        private static string selectStatement = "select u from PickRule as u";

        private static string duiplicateVerifyStatement = @"select count(*) from PickRule as u where u.Item = ? and u.Location =? and u.Picker= ?";

        #region public actions
        /// <summary>
        /// Index action for PickRule controller
        /// </summary>
        /// <returns>Index view</returns>
        [SconitAuthorize(Permissions = "Url_PickRule_View")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// List action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">PickRule Search model</param>
        /// <returns>return the result view</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_PickRule_View")]
        public ActionResult List(GridCommand command, PickRuleSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        /// <summary>
        ///  AjaxList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">PickRule Search Model</param>
        /// <returns>return the result action</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_PickRule_View")]
        public ActionResult _AjaxList(GridCommand command, PickRuleSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<PickRule>(searchStatementModel, command));
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <returns>New view</returns>
        [SconitAuthorize(Permissions = "Url_PickRule_View")]
        public ActionResult New()
        {
            return View();
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <param name="pickRule">PickRule Model</param>
        /// <returns>return the result view</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_PickRule_View")]
        public ActionResult New(PickRule pickRule)
        {
            if (ModelState.IsValid)
            {
                if (base.genericMgr.FindAll<long>(duiplicateVerifyStatement, new object[] { pickRule.Item, pickRule.Location, pickRule.Picker })[0] > 0)
                {
                    SaveErrorMessage(Resources.MD.Picker.PickerErrors_Existing_ItemLocationPicker, pickRule.Item, pickRule.Location, pickRule.Picker);
                }
                else
                {
                    var item = base.genericMgr.FindById<Item>(pickRule.Item);
                    pickRule.ItemDescription = item.Description;

                    base.genericMgr.Create(pickRule);
                    SaveSuccessMessage(Resources.MD.Picker.PickRule_Added);
                    return RedirectToAction("Edit/" + pickRule.Id);
                }
            }

            return View(pickRule);
        }

        /// <summary>
        /// Edit view
        /// </summary>
        /// <param name="id">PickRule id for edit</param>
        /// <returns>return the result view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_PickRule_View")]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }

            PickRule PickRule = base.genericMgr.FindById<PickRule>(id);
            return View(PickRule);
        }

        /// <summary>
        /// Edit view
        /// </summary>
        /// <param name="pickRule">PickRule Model</param>
        /// <returns>return the result view</returns>
        [SconitAuthorize(Permissions = "Url_PickRule_View")]
        public ActionResult Edit(PickRule pickRule)
        {
            if (ModelState.IsValid)
            {
                if (base.genericMgr.FindAll<long>(duiplicateVerifyStatement + " and Id != ?", new object[] { pickRule.Item, pickRule.Location, pickRule.Picker, pickRule.Id })[0] > 0)
                {
                    SaveErrorMessage(Resources.MD.Picker.PickerErrors_Existing_ItemLocationPicker, pickRule.Item, pickRule.Location, pickRule.Picker);
                }
                else
                {
                    var item = base.genericMgr.FindById<Item>(pickRule.Item);
                    pickRule.ItemDescription = item.Description;
                    base.genericMgr.Update(pickRule);
                    SaveSuccessMessage(Resources.MD.Picker.PickRule_Updated);
                }
            }

            return View(pickRule);
        }

        /// <summary>
        /// Delete action
        /// </summary>
        /// <param name="id">PickRule id for delete</param>
        /// <returns>return to List action</returns>
        [SconitAuthorize(Permissions = "Url_PickRule_View")]
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }

            base.genericMgr.DeleteById<PickRule>(id);
            SaveSuccessMessage(Resources.MD.Picker.PickRule_Deleted);
            return RedirectToAction("List");
        }
        #endregion

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">PickRule Search Model</param>
        /// <returns>return PickRule search model</returns>
        private SearchStatementModel PrepareSearchStatement(GridCommand command, PickRuleSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("Item", searchModel.SearchItem, HqlStatementHelper.LikeMatchMode.Start, "u", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Location", searchModel.SearchLocation, HqlStatementHelper.LikeMatchMode.Start, "u", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Picker", searchModel.SearchPicker, HqlStatementHelper.LikeMatchMode.Start, "u", ref whereStatement, param);

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
