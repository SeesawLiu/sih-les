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
    /// This controller response to control the Shipper.
    /// </summary>
    public class ShipperController : WebAppBaseController
    {       
        private static string selectCountStatement = "select count(*) from Shipper as u";

        private static string selectStatement = "select u from Shipper as u";

        private static string duiplicateVerifyStatement = @"select count(*) from Shipper as u where u.Code = ?";

        #region public actions
        /// <summary>
        /// Index action for Shipper controller
        /// </summary>
        /// <returns>Index view</returns>
        [SconitAuthorize(Permissions = "Url_Shipper_View")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// List action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">Shipper Search model</param>
        /// <returns>return the result view</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Shipper_View")]
        public ActionResult List(GridCommand command, ShipperSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        /// <summary>
        ///  AjaxList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">Shipper Search Model</param>
        /// <returns>return the result action</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Shipper_View")]
        public ActionResult _AjaxList(GridCommand command, ShipperSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<Shipper>(searchStatementModel, command));
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <returns>New view</returns>
        [SconitAuthorize(Permissions = "Url_Shipper_View")]
        public ActionResult New()
        {
            return View();
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <param name="shipper">Shipper Model</param>
        /// <returns>return the result view</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_Shipper_View")]
        public ActionResult New(Shipper shipper)
        {
            if (ModelState.IsValid)
            {
                if (base.genericMgr.FindAll<long>(duiplicateVerifyStatement, new object[] { shipper.Code })[0] > 0)
                {
                    SaveErrorMessage(Resources.MD.Shipper.ShipperErrors_Existing_Code, shipper.Code);
                }
                else
                {
                    base.genericMgr.Create(shipper);
                    SaveSuccessMessage(Resources.MD.Shipper.Shipper_Added);
                    return RedirectToAction("Edit/" + shipper.Code);
                }
            }

            return View(shipper);
        }

        /// <summary>
        /// Edit view
        /// </summary>
        /// <param name="id">Shipper id for edit</param>
        /// <returns>return the result view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_Shipper_View")]
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            else
            {
                Shipper Shipper = base.genericMgr.FindById<Shipper>(id);
                return View(Shipper);
            }
        }

        /// <summary>
        /// Edit view
        /// </summary>
        /// <param name="shipper">Shipper Model</param>
        /// <returns>return the result view</returns>
        [SconitAuthorize(Permissions = "Url_Shipper_View")]
        public ActionResult Edit(Shipper shipper)
        {
            if (ModelState.IsValid)
            {
                base.genericMgr.Update(shipper);
                SaveSuccessMessage(Resources.MD.Shipper.Shipper_Updated);
            }

            return View(shipper);
        }

        /// <summary>
        /// Delete action
        /// </summary>
        /// <param name="id">Shipper id for delete</param>
        /// <returns>return to List action</returns>
        [SconitAuthorize(Permissions = "Url_Shipper_View")]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<Shipper>(id);
                SaveSuccessMessage(Resources.MD.Shipper.Shipper_Deleted);
                return RedirectToAction("List");
            }
        }
        #endregion

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">Shipper Search Model</param>
        /// <returns>return Shipper search model</returns>
        private SearchStatementModel PrepareSearchStatement(GridCommand command, ShipperSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("Code", searchModel.SearchCode, HqlStatementHelper.LikeMatchMode.Start, "u", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Description", searchModel.SearchDescription, HqlStatementHelper.LikeMatchMode.Start, "u", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Location", searchModel.SearchLocation, HqlStatementHelper.LikeMatchMode.Start, "u", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Address", searchModel.SearchAddress, HqlStatementHelper.LikeMatchMode.Start, "u", ref whereStatement, param);

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
