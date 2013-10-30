using com.Sconit.Entity.ACC;

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
    /// This controller response to control the Picker.
    /// </summary>
    public class PickerController : WebAppBaseController
    {
        private static string selectCountStatement = "select count(*) from Picker as u";

        private static string selectStatement = "select u from Picker as u";

        private static string duiplicateVerifyStatement = @"select count(*) from Picker as u where u.Code = ? and u.UserCode =?";

        private static string getUser = "from User where Code = ?";
        #region public actions
        /// <summary>
        /// Index action for Picker controller
        /// </summary>
        /// <returns>Index view</returns>
        [SconitAuthorize(Permissions = "Url_Picker_View")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// List action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">Picker Search model</param>
        /// <returns>return the result view</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Picker_View")]
        public ActionResult List(GridCommand command, PickerSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        /// <summary>
        ///  AjaxList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">Picker Search Model</param>
        /// <returns>return the result action</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Picker_View")]
        public ActionResult _AjaxList(GridCommand command, PickerSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<Picker>(searchStatementModel, command));
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <returns>New view</returns>
        [SconitAuthorize(Permissions = "Url_Picker_View")]
        public ActionResult New()
        {
            var picker = new Picker { IsActive = true };
            return View(picker);
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <param name="picker">Picker Model</param>
        /// <returns>return the result view</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_Picker_View")]
        public ActionResult New(Picker picker)
        {
            if (ModelState.IsValid)
            {
                if (base.genericMgr.FindAll<long>(duiplicateVerifyStatement, new object[] { picker.Code, picker.UserCode })[0] > 0)
                {
                    SaveErrorMessage(Resources.MD.Picker.PickerErrors_Existing_CodeAndUserCode, picker.Code, picker.UserCode);
                }
                else
                {
                    IList<User> users = base.genericMgr.FindAll<User>(getUser, picker.UserCode);
                    if (users != null && users.Count > 0)
                    {
                        picker.UserNm = users[0].Name;
                        base.genericMgr.Create(picker);
                        SaveSuccessMessage(Resources.MD.Picker.Picker_Added);
                        return RedirectToAction("Edit/" + picker.Id);
                    }

                    SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldValueNotExist, Resources.MD.Picker.Picker_UserCode, picker.UserCode);
                }
            }

            return View(picker);
        }

        /// <summary>
        /// Edit view
        /// </summary>
        /// <param name="id">picker id for edit</param>
        /// <returns>return the result view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_Picker_View")]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                Picker picker = base.genericMgr.FindById<Picker>(id);
                return View(picker);
            }
        }

        /// <summary>
        /// Edit view
        /// </summary>
        /// <param name="picker">Picker Model</param>
        /// <returns>return the result view</returns>
        [SconitAuthorize(Permissions = "Url_Picker_View")]
        public ActionResult Edit(Picker picker)
        {
            if (ModelState.IsValid)
            {
                if (base.genericMgr.FindAll<long>(duiplicateVerifyStatement + " and Id != ?", new object[] { picker.Code, picker.UserCode, picker.Id })[0] > 0)
                {
                    SaveErrorMessage(Resources.MD.Picker.PickerErrors_Existing_CodeAndUserCode, picker.Code, picker.UserCode);
                }
                else
                {
                    IList<User> users = base.genericMgr.FindAll<User>(getUser, picker.UserCode);
                    if (users != null && users.Count > 0)
                    {
                        picker.UserNm = users[0].Name;
                        base.genericMgr.Update(picker);
                        SaveSuccessMessage(Resources.MD.Picker.Picker_Updated);
                    }
                    else
                    {
                        SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldValueNotExist, Resources.MD.Picker.Picker_UserCode, picker.UserCode);
                    }
                }
            }

            return View(picker);
        }

        /// <summary>
        /// Delete action
        /// </summary>
        /// <param name="id">picker id for delete</param>
        /// <returns>return to List action</returns>
        [SconitAuthorize(Permissions = "Url_Picker_View")]
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }

            var instance = base.genericMgr.FindById<Picker>(id);
            base.genericMgr.Delete(string.Format("from PickRule r where r.Location = '{0}' and r.Picker ='{1}' ", instance.Location, instance.Code));

            base.genericMgr.DeleteById<Picker>(id);

            SaveSuccessMessage(Resources.MD.Picker.Picker_Deleted);
            return RedirectToAction("List");
        }
        #endregion

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">Picker Search Model</param>
        /// <returns>return picker search model</returns>
        private SearchStatementModel PrepareSearchStatement(GridCommand command, PickerSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("Code", searchModel.SearchCode, HqlStatementHelper.LikeMatchMode.Start, "u", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Description", searchModel.SearchDescription, HqlStatementHelper.LikeMatchMode.Start, "u", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Location", searchModel.SearchLocation, HqlStatementHelper.LikeMatchMode.Start, "u", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("UserCode", searchModel.SearchUserCode, HqlStatementHelper.LikeMatchMode.Start, "u", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("IsActive", searchModel.SearchIsActive, "u", ref whereStatement, param);

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
