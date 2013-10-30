using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using com.Sconit.Entity.LOG;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels.LOG;
using com.Sconit.Web.Util;

namespace com.Sconit.Web.Controllers.LOG
{
    public class GenVanProdOrderController : WebAppBaseController
    {
        private static string selectCountStatement = "select count(*) from GenVanProdOrder as u";

        private static string selectStatement = "select u from GenVanProdOrder as u";

        public ActionResult Index()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_GenVanProdOrder_View")]
        public ActionResult List(GridCommand command, GenVanProdOrderSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_GenVanProdOrder_View")]
        public ActionResult _AjaxList(GridCommand command, GenVanProdOrderSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<GenVanProdOrder>(searchStatementModel, command));
        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, GenVanProdOrderSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("ZLINE", searchModel.SearchZLINE, HqlStatementHelper.LikeMatchMode.Start, "u", ref whereStatement, param);

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
