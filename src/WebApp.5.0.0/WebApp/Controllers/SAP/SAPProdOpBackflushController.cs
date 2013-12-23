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
            #region
            string hql = @"select sp.* from SAP_ProdOpBackflush as sp with(nolock) inner join ORD_OrderMstr_4 as o  with(nolock) on sp.OrderNo=o.OrderNo
where   o.ProdLineType not in  (1 , 2 , 3 , 4 , 9) ";
            IList<object> paramArr = new List<object>();
            if (!string.IsNullOrWhiteSpace(searchModel.OrderNo))
            {
                hql += " and o.OrderNo = ? ";
                paramArr.Add(searchModel.OrderNo);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.AUFNR))
            {
                hql += " and sp.AUFNR = ? ";
                paramArr.Add(searchModel.AUFNR);
            }
            if (searchModel.Status != null)
            {
                hql += " and sp.Status = ? ";
                paramArr.Add(searchModel.Status);
            }
            if (searchModel.StartDate != null)
            {
                hql += " and sp.CreateDate >= ? ";
                paramArr.Add(searchModel.StartDate.Value);
            }
            if (searchModel.EndDate != null)
            {
                hql += " and sp.CreateDate <= ? ";
                paramArr.Add(searchModel.EndDate);
            }
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by CreateDate desc ";
            }
            #endregion

            IList<object> countList = this.genericMgr.FindAllWithNativeSql<object>("select count(*) from (" + hql + ") as tt3", paramArr.ToArray());

            IList<ProdOpBackflush> searchResultList = this.genericMgr.FindEntityWithNativeSql<ProdOpBackflush>("select * from ( select RowId=ROW_NUMBER()OVER(" + sortingStatement + "),* from (" + hql + " ) as tt2 ) as tt3 where tt3.RowId between " + (command.Page - 1) * command.PageSize + " and " + command.Page * command.PageSize + "", paramArr.ToArray());


            ViewBag.Total = Convert.ToInt32(countList[0]);
            GridModel<ProdOpBackflush> gridModel = new GridModel<ProdOpBackflush>();
            gridModel.Total = Convert.ToInt32(countList[0]);
            gridModel.Data = searchResultList;
            return PartialView(gridModel); 
        }

        public void ExportXLS(SearchModel searchModel)
        {
            string hql = @"select sp.* from SAP_ProdOpBackflush as sp with(nolock) inner join ORD_OrderMstr_4 as o  with(nolock) on sp.OrderNo=o.OrderNo
where   o.ProdLineType not in  (1 , 2 , 3 , 4 , 9) ";
            IList<object> paramArr = new List<object>();
            if (!string.IsNullOrWhiteSpace(searchModel.OrderNo))
            {
                hql += " and o.OrderNo = ? ";
                paramArr.Add(searchModel.OrderNo);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.AUFNR))
            {
                hql += " and sp.AUFNR = ? ";
                paramArr.Add(searchModel.AUFNR);
            }
            if (searchModel.Status!=null)
            {
                hql += " and sp.Status = ? ";
                paramArr.Add(searchModel.Status);
            }
            if (searchModel.StartDate!=null)
            {
                hql += " and sp.CreateDate >= ? ";
                paramArr.Add(searchModel.StartDate.Value);
            }
            if (searchModel.EndDate!=null )
            {
                hql += " and sp.CreateDate <= ? ";
                paramArr.Add(searchModel.EndDate);
            }
            hql += " order by sp.CreateDate desc ";
            IList<ProdOpBackflush> exportList = this.genericMgr.FindEntityWithNativeSql<ProdOpBackflush>(hql, paramArr.ToArray());
            ExportToXLS<ProdOpBackflush>("ExportProdOpBackflush", "xls", exportList);
        }

        public ActionResult ReSendSAPProdOpBackflush(string idStr)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(idStr))
                {
                    this.genericMgr.UpdateWithNativeQuery(string.Format(@"update sp set sp.ErrorCount=0 from SAP_ProdOpBackflush as sp,ORD_OrderMstr_4 as o  where sp.OrderNo=o.OrderNo
and   o.ProdLineType not in  (1 , 2 , 3 , 4 , 9) and sp.Status=2 and sp.id in({0})", idStr));
                }
                else
                {
                    this.genericMgr.UpdateWithNativeQuery(@"update sp set sp.ErrorCount=0 from SAP_ProdOpBackflush as sp,ORD_OrderMstr_4 as o  where sp.OrderNo=o.OrderNo
and   o.ProdLineType not in  (1 , 2 , 3 , 4 , 9) and sp.Status=2");
                }
                SaveSuccessMessage("重发成功。");

            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.Message);
            }
            return RedirectToAction("List");
        }


        private string PrepareSearchStatement(GridCommand command, SearchModel searchModel)
        {
            string hql = @"select sp.* from SAP_ProdOpBackflush as sp with(nolock) inner join ORD_OrderMstr_4 as o  with(nolock) on sp.OrderNo=o.OrderNo
where   o.ProdLineType not in  (1 , 2 , 3 , 4 , 9) ";
            IList<object> paramArr = new List<object>();
            if (!string.IsNullOrWhiteSpace(searchModel.OrderNo))
            {
                hql += " and o.OrderNo = ? ";
                paramArr.Add(searchModel.OrderNo);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.AUFNR))
            {
                hql += " and sp.AUFNR = ? ";
                paramArr.Add(searchModel.AUFNR);
            }
            if (searchModel.Status != null)
            {
                hql += " and sp.Status = ? ";
                paramArr.Add(searchModel.Status);
            }
            if (searchModel.StartDate != null)
            {
                hql += " and sp.CreateDate >= ? ";
                paramArr.Add(searchModel.StartDate.Value);
            }
            if (searchModel.EndDate != null)
            {
                hql += " and sp.CreateDate <= ? ";
                paramArr.Add(searchModel.EndDate);
            }
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by sp.CreateDate desc ";
            }

            return hql;
        }

    }
}