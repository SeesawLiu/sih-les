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
    // [SconitAuthorize(Permissions = "Url_SI_SAP_Supplier_View")]
    public class CancelProdOpReportController : WebAppBaseController
    {
        private static string selectCountStatement = "select count(*) from CancelProdOpReport as t";

        private static string selectStatement = "select t from CancelProdOpReport as t";

        [SconitAuthorize(Permissions = "Url_SI_SAP_CancelProdOpReport_View")]
        public ActionResult Index(GridCommand command, SearchModel searchModel)
        {
            return View();
//            TempData["SearchModel"] = searchModel;
//            //command.PageSize = 100;
//            int startSize = command.Page == 0 ? 0 : (command.Page - 1) * command.PageSize;
//            int endSize = command.Page == 0 ? command.PageSize : command.Page * command.PageSize;

//            SqlParameter[] sqlParam = new SqlParameter[5];
//            string sql = @"select RowId=ROW_NUMBER()OVER(order by   c.CreateDate  asc),* from SAP_CancelProdOpReport as c where 
//                            c.Status = @p0 and c.CreateDate > @p1 and c.CreateDate < @p2 ";

//            sqlParam[0] = new SqlParameter("@p0", searchModel.Status.HasValue ? searchModel.Status.Value : 2);

//            if (searchModel.StartDate.HasValue)
//            {
//                sqlParam[1] = new SqlParameter("@p1", searchModel.StartDate);
//            }
//            else
//            {
//                sqlParam[1] = new SqlParameter("@p1", "1900-1-1");
//            }
//            if (searchModel.EndDate.HasValue)
//            {
//                sqlParam[2] = new SqlParameter("@p2", searchModel.EndDate);
//            }
//            else
//            {
//                sqlParam[2] = new SqlParameter("@p2", DateTime.Now);
//            }

//            sqlParam[3] = new SqlParameter("@p3", startSize);
//            sqlParam[4] = new SqlParameter("@p4", endSize);

//            string execSql = " select t1.* from ("+sql+") as t1 where RowId between @p3 and @p4";

//            DataSet entity = genericMgr.GetDatasetBySql(execSql, sqlParam);
//            sqlParam[3].Value = 0;
//            sqlParam[4].Value=999999999999999;
//            DataSet countEntity = genericMgr.GetDatasetBySql(" select isnull(count(*),0) as count from (" + sql + ") as t1 where RowId between @p3 and @p4", sqlParam);

//            ViewModel model = new ViewModel();
//            model.Data = entity.Tables[0];
//            model.Columns = IListHelper.GetColumns(entity.Tables[0]);
//            ViewBag.Total = int.Parse((countEntity.Tables[0].Rows[0].ItemArray[0]).ToString());
//            return View(model);
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_SI_SAP_CancelProdOpReport_View")]
        public ActionResult List(GridCommand command, SearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_SI_SAP_CancelProdOpReport_View")]
        public ActionResult _AjaxList(GridCommand command, SearchModel searchModel)
        {

            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            GridModel<CancelProdOpReport> gridlist = GetAjaxPageData<CancelProdOpReport>(searchStatementModel, command);
            return PartialView(gridlist);
            //return PartialView(GetAjaxPageData<ReceiptMaster>(searchStatementModel, command));
        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, SearchModel searchModel)
        {
            string whereStatement = "";

            IList<object> param = new List<object>();
            HqlStatementHelper.AddEqStatement("Status", searchModel.Status, "t", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("AUFNR", searchModel.AUFNR, "t", ref whereStatement, param);

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