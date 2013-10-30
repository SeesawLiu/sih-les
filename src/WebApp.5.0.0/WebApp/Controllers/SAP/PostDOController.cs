using System.Data;
using System.Web.Mvc;
using com.Sconit.Web.Util;
using Telerik.Web.Mvc;
using com.Sconit.Web.Models.SearchModels.SI;
using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using com.Sconit.Web.Models;
using com.Sconit.Entity.SAP.ORD;
using System.Linq;

/// <summary>
///MainController 的摘要说明
/// </summary>
namespace com.Sconit.Web.Controllers.SAP
{
    [SconitAuthorize]
    public class PostDOController : WebAppBaseController
    {
        private static string selectCountStatement = "select count(*) from PostDO as t";

        private static string selectStatement = "select t from PostDO as t";

        [SconitAuthorize(Permissions = "Url_SAP_PostDO_View")]
        public ActionResult Index(GridCommand command, SearchModel searchModel)
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_SAP_PostDO_View")]
        public ActionResult List(GridCommand command, SearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_SAP_PostDO_View")]
        public ActionResult _AjaxList(GridCommand command, SearchModel searchModel)
        {

            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            GridModel<PostDO> gridlist = GetAjaxPageData<PostDO>(searchStatementModel, command);
            return PartialView(gridlist);
        }

        public ActionResult ReSend(int id)
        {
            this.genericMgr.UpdateWithNativeQuery("update  SAP_CRSLSummary set ErrorCount=0 where id=?",id);
            SaveSuccessMessage("操作成功。");
            return RedirectToAction("List");
        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, SearchModel searchModel)
        {
            string whereStatement = "";

            IList<object> param = new List<object>();
            HqlStatementHelper.AddEqStatement("OrderNo", searchModel.OrderNo, "t", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("ReceiptNo", searchModel.ReceiptNo, "t", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Status", searchModel.Status, "t", ref whereStatement, param);

            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.StartDate, searchModel.EndDate, "t", ref whereStatement, param);
            }
            else if (searchModel.StartDate != null & searchModel.EndDate == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.StartDate, "t", ref whereStatement, param);
            }
            else if (searchModel.StartDate == null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.EndDate, "t", ref whereStatement, param);
            }
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by t.CreateDate desc";
            }
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