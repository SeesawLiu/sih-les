using System.Data;
using System.Web.Mvc;
using com.Sconit.Web.Util;
using Telerik.Web.Mvc;
using com.Sconit.Web.Models.SearchModels.SI;
using System.Data.SqlClient;
using System;
using System.Linq;
using com.Sconit.Entity.SAP.ORD;
using com.Sconit.Web.Models;
using System.Collections.Generic;
using com.Sconit.Service.SAP;
using com.Sconit.Entity.Exception;

/// <summary>
///MainController 的摘要说明
/// </summary>
namespace com.Sconit.Web.Controllers.SAP
{
    [SconitAuthorize]
    // [SconitAuthorize(Permissions = "Url_SI_SAP_Supplier_View")]
    public class SAPProdOpBackflushController : WebAppBaseController
    {
        public IProductionMgr productionMgr { get; set; }

        private static string selectCountStatement = "select count(*) from ProdOpBackflush as t";

        private static string selectStatement = "select t from ProdOpBackflush as t";

        [SconitAuthorize(Permissions = "Url_SI_SAP_ProdOpBackflush_View")]
        public ActionResult Index(GridCommand command, SearchModel searchModel)
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_SI_SAP_ProdOpBackflush_View")]
        public ActionResult List(GridCommand command, SearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_SI_SAP_ProdOpBackflush_View")]
        public ActionResult _AjaxList(GridCommand command, SearchModel searchModel)
        {

            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            GridModel<ProdOpBackflush> gridlist = GetAjaxPageData<ProdOpBackflush>(searchStatementModel, command);
            return PartialView(gridlist);
        }

        public void ExportXLS(SearchModel searchModel)
        {
            string hql = selectStatement;
            hql += string.Format("where exists (select 1 from OrderMaster as o  with(nolock) where o.OrderNo=t.OrderNo and o.ProdLineType not in (1,2,3,4,9) and o.Type={0}) ", (int)com.Sconit.CodeMaster.OrderType.Production);
            IList<object> paramArr = new List<object>();
            if (!string.IsNullOrWhiteSpace(searchModel.OrderNo))
            {
                hql += " and t.OrderNo = ? ";
                paramArr.Add(searchModel.OrderNo);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.AUFNR))
            {
                hql += " and t.AUFNR = ? ";
                paramArr.Add(searchModel.AUFNR);
            }
            if (searchModel.Status!=null)
            {
                hql += " and t.Status = ? ";
                paramArr.Add(searchModel.Status);
            }
            if (searchModel.StartDate!=null)
            {
                hql += " and t.StartDate >= ? ";
                paramArr.Add(searchModel.StartDate.Value);
            }
            if (searchModel.EndDate!=null )
            {
                hql += " and t.EndDate <= ? ";
                paramArr.Add(searchModel.EndDate);
            }
            hql += " order by i.CreateDate desc ";
            IList<ProdOpBackflush> exportList = this.genericMgr.FindAll<ProdOpBackflush>(hql, paramArr.ToArray());
            ExportToXLS<ProdOpBackflush>("ExportProdOpBackflush", "xls", exportList);
        }

        public ActionResult ReSendSAPProdOpBackflush(string idStr)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(idStr))
                {
                    this.genericMgr.UpdateWithNativeQuery("update SAP_ProdOpBackflush set ErrorCount=0 where id in(?)", idStr);
                }
                else
                {
                    this.genericMgr.UpdateWithNativeQuery("update SAP_ProdOpBackflush set ErrorCount=0 ");
                }
                SaveSuccessMessage("重发成功。");

            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.Message);
            }
            return RedirectToAction("List");
        }


        private SearchStatementModel PrepareSearchStatement(GridCommand command, SearchModel searchModel)
        {
            string whereStatement = string.Format("where exists (select 1 from OrderMaster as o  with(nolock) where o.OrderNo=t.OrderNo and o.ProdLineType not in (1,2,3,4,9) and o.Type={0}) ", (int)com.Sconit.CodeMaster.OrderType.Production);

            IList<object> param = new List<object>();
            HqlStatementHelper.AddEqStatement("Status", searchModel.Status, "t", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("OrderNo", searchModel.OrderNo, "t", ref whereStatement, param);
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