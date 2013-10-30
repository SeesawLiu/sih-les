using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using com.Sconit.Entity.LOG;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels.LOG;
using com.Sconit.Web.Util;
using com.Sconit.Web.Models.SearchModels.ORD;

namespace com.Sconit.Web.Controllers.LOG
{
    public class ProdOrderPauseResumeController : WebAppBaseController
    {
        private static string selectCountStatement = "select count(*) from ProdOrderPauseResume as p";

        private static string selectStatement = "select p from ProdOrderPauseResume as p";

        public ActionResult Index()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProdOrderPauseResume_View")]
        public ActionResult List(GridCommand command, OrderMasterSearchModel searchModel )
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProdOrderPauseResume_View")]
        public ActionResult _AjaxList(GridCommand command, OrderMasterSearchModel  searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<ProdOrderPauseResume>(searchStatementModel, command));
        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, OrderMasterSearchModel searchModel )
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();
            if (!string.IsNullOrWhiteSpace(searchModel.TraceCode))
            {
                searchModel.TraceCode = searchModel.TraceCode.PadLeft(10, '0').Substring(2);
            }
            HqlStatementHelper.AddLikeStatement("VanCode", searchModel.TraceCode,HqlStatementHelper.LikeMatchMode.End, "p", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("OrderNo", searchModel.OrderNo, "p", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("ProdLine", searchModel.ProdLine, "p", ref whereStatement, param);

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
