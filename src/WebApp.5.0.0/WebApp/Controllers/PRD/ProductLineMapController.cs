namespace com.Sconit.Web.Controllers.PRD
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using Service;
    using Models;
    using Util;
    using Telerik.Web.Mvc;
    using Models.SearchModels.CUST;
    using Entity.CUST;

    /// <summary>
    /// This controller response to control the Routing.
    /// </summary>
    public class ProductLineMapController : WebAppBaseController
    {
        /// <summary>
        /// hql to get count of the RoutingMaster
        /// </summary>
        private static string selectCountStatement = "select count(*) from ProductLineMap  r";

        /// <summary>
        /// hql to get all of the RoutingMaster
        /// </summary>
        private static string selectStatement = "select r from ProductLineMap as r";

        private static string CodeDuiplicateVerifyStatement = @"select count(*) from ProductLineMap as c where c.SAPProductLine = ?";

        /// <summary>
        /// Index action for Routing controller
        /// </summary>
        /// <returns>Index view</returns>
        [SconitAuthorize(Permissions = "Url_CUST_ProductLineMap_View")]
        public ActionResult Index()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_CUST_ProductLineMap_Edit")]
        public ActionResult New()
        {
            return View();
        }
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_CUST_ProductLineMap_Edit")]
        public ActionResult New(ProductLineMap productLineMap)
        {
            if (ModelState.IsValid)
            {
                //判断描述不能重复
                try
                {
                    var hasError = Validate(productLineMap);
                    if (!hasError)
                    {
                        if (base.genericMgr.FindAll<long>(CodeDuiplicateVerifyStatement, new object[] { productLineMap.SAPProductLine })[0] > 0)
                        {
                            base.SaveErrorMessage(Resources.CUST.ProductLineMap.ProductLineMap_Added_Existing_Code, productLineMap.SAPProductLine);
                        }
                        else
                        {
                            productLineMap.Type = com.Sconit.CodeMaster.ProductLineMapType.NotVan;
                            base.genericMgr.Create(productLineMap);
                            SaveSuccessMessage(Resources.CUST.ProductLineMap.ProductLineMap_Added);
                            return RedirectToAction("Edit", new { SAPProductLine = productLineMap.SAPProductLine });
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    SaveErrorMessage(ex.Message);
                }
            }
            return View(productLineMap);

        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_CUST_ProductLineMap_Edit")]
        public ActionResult Edit(string SAPProductLine)
        {
            if (string.IsNullOrEmpty(SAPProductLine))
            {
                return HttpNotFound();
            }
            else
            {
                ProductLineMap code = base.genericMgr.FindById<ProductLineMap>(SAPProductLine);
                return View(code);
            }
        }

        [SconitAuthorize(Permissions = "Url_CUST_ProductLineMap_Edit")]
        public ActionResult Edit(GridCommand command, ProductLineMap productLineMap)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var hasError = Validate(productLineMap);
                    if (!hasError)
                    {
                        productLineMap.Type = com.Sconit.CodeMaster.ProductLineMapType.NotVan;
                        base.genericMgr.Update(productLineMap);
                        SaveSuccessMessage(Resources.CUST.ProductLineMap.ProductLineMap_Added);
                        return RedirectToAction("Edit", new { SAPProductLine = productLineMap.SAPProductLine });
                    }
                }
                catch (System.Exception ex)
                {

                    SaveErrorMessage(ex.Message);
                }
            }
            return View(productLineMap);
        }

        [SconitAuthorize(Permissions = "Url_CUST_ProductLineMap_Edit")]
        public ActionResult ProductLineMapDeleteId(string id)
        {
            try
            {
                base.genericMgr.DeleteById<ProductLineMap>(id);
                SaveSuccessMessage(Resources.CUST.ProductLineMap.ProductLineMap_Deletedsuccessful);
            }
            catch (System.Exception ex)
            {
                SaveErrorMessage(ex.Message);
            }
            return RedirectToAction("List");

        }

        /// <summary>
        /// List acion
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">RoutingMaster Search Model</param>
        /// <returns>return to the result action</returns>
        [GridAction]
        [SconitAuthorize(Permissions = "Url_CUST_ProductLineMap_View")]
        public ActionResult List(GridCommand command, ProductLineMapSearchModel searchModel)
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
        [SconitAuthorize(Permissions = "Url_CUST_ProductLineMap_View")]
        public ActionResult _AjaxList(GridCommand command, ProductLineMapSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<ProductLineMap>(searchStatementModel, command));
        }

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">RoutingMaster Search Model</param>
        /// <returns>Search Statement</returns>
        private SearchStatementModel PrepareSearchStatement(GridCommand command, ProductLineMapSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();
            HqlStatementHelper.AddEqStatement("Type", (int)com.Sconit.CodeMaster.ProductLineMapType.NotVan, "r", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("SAPProductLine", searchModel.SAPProductLine, HqlStatementHelper.LikeMatchMode.Start, "r", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("ProductLine", searchModel.ProductLine, "r", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("IsActive", searchModel.SearchIsActive, "r", ref whereStatement, param);

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        private bool Validate(ProductLineMap productLineMap)
        {
            var hasError = false;
            if (string.IsNullOrWhiteSpace(productLineMap.ProductLine))
            {
                hasError = true;
                SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.CUST.ProductLineMap.ProductLineMap_ProductLine);
            }
            if (string.IsNullOrWhiteSpace(productLineMap.Plant))
            {
                hasError = true;
                SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.CUST.ProductLineMap.ProductLineMap_Plant);
            }
            return hasError;
        }
    }
}

