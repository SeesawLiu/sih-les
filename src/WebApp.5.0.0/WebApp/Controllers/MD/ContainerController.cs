/// <summary>
/// Summary description for ContainerController
/// </summary>
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
    /// This controller response to control the Container.
    /// </summary>
    public class ContainerController : WebAppBaseController
    {
        /// <summary>
        /// hql to get count of the Container
        /// </summary>
        private static string selectCountStatement = "select count(*) from Container as c";

        /// <summary>
        /// hql to get all of the Container
        /// </summary>
        private static string selectStatement = "select c from Container as c";

        /// <summary>
        /// hql to get count of the Container by Container's code
        /// </summary>
        private static string duiplicateVerifyStatement = @"select count(*) from Container as c where c.Code = ?";

        #region public actions
        /// <summary>
        /// Index action for Container controller
        /// </summary>
        /// <returns>Index view</returns>
        [SconitAuthorize(Permissions = "Url_Container_View")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// List action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">Container Search model</param>
        /// <returns>return the result view</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Container_View")]
        public ActionResult List(GridCommand command, ContainerSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        /// <summary>
        ///  AjaxList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">Container Search Model</param>
        /// <returns>return the result action</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Container_View")]
        public ActionResult _AjaxList(GridCommand command, ContainerSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<Container>(searchStatementModel, command));
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <returns>New view</returns>
        [SconitAuthorize(Permissions = "Url_Container_Edit")]
        public ActionResult New()
        {
            return View();
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <param name="container">Container Model</param>
        /// <returns>return the result view</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_Container_Edit")]
        public ActionResult New(Container container)
        {
            if (ModelState.IsValid)
            {
                if (base.genericMgr.FindAll<long>(duiplicateVerifyStatement, new object[] { container.Code })[0] > 0)
                {
                    SaveErrorMessage(Resources.ErrorMessage.Errors_Existing_Code, container.Code);
                }
                else
                {
                    base.genericMgr.Create(container);
                    SaveSuccessMessage(Resources.MD.Container.Container_Added);
                    return RedirectToAction("Edit/" + container.Code);
                }
            }

            return View(container);
        }

        /// <summary>
        /// Edit view
        /// </summary>
        /// <param name="id">container id for edit</param>
        /// <returns>return the result view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_Container_Edit")]
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                Container container = base.genericMgr.FindById<Container>(id);
                return View(container);
            }
        }

        /// <summary>
        /// Edit view
        /// </summary>
        /// <param name="container">Container Model</param>
        /// <returns>return the result view</returns>
        [SconitAuthorize(Permissions = "Url_Container_Edit")]
        public ActionResult Edit(Container container)
        {
            if (ModelState.IsValid)
            {
                base.genericMgr.Update(container);
                SaveSuccessMessage(Resources.MD.Container.Container_Updated);
            }

            return View(container);
        }

        /// <summary>
        /// Delete action
        /// </summary>
        /// <param name="id">Container id for delete</param>
        /// <returns>return to List action</returns>
        [SconitAuthorize(Permissions = "Url_Container_Delete")]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<Container>(id);
                SaveSuccessMessage(Resources.MD.Container.Container_Deleted);
                return RedirectToAction("List");
            }
        }
        #endregion

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">Container Search Model</param>
        /// <returns>return Container search model</returns>
        private SearchStatementModel PrepareSearchStatement(GridCommand command, ContainerSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("Code", searchModel.Code, HqlStatementHelper.LikeMatchMode.Start, "c", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Description", searchModel.Description, HqlStatementHelper.LikeMatchMode.Start, "c", ref whereStatement, param);
           
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
