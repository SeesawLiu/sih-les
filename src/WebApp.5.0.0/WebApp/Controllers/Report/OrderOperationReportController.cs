using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using com.Sconit.Web.Util;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels.ORD;
using com.Sconit.Entity.ORD;

namespace com.Sconit.Web.Controllers.Report
{
    public class OrderOperationReportController : WebAppBaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderOperationReport_View")]
        public ActionResult List(GridCommand command, OrderMasterSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            if (searchModel.DateFrom !=null && searchModel.DateTo!=null)
            {
                TempData["_AjaxMessage"] = "";
                ViewBag.DateFrom = searchModel.DateFrom.Value;
                ViewBag.DateTo = searchModel.DateTo.Value;
            }
            else
            {
                SaveWarningMessage("开始结束时间不能为空。");
            }
            
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderOperationReport_View")]
        public ActionResult _AjaxList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (searchModel.DateFrom == null || searchModel.DateTo == null)
            {
                return PartialView(new GridModel(new List<OrderOperationReport>()));
            }

            string searchSql = @"select op.WorkCenter,om.Flow,SUM(op.ReportQty) as ReportQty from ORD_OrderOpReport as op
inner join ORD_OrderMstr_4  as om on op.OrderNo=om.OrderNo 
where om.ProdLineType in (1,2,3,4,9) and (om.Status = 3 or om.Status = 4) and op.Status=0 ";
            IList<object> paramArr = new List<object>();
            if (!string.IsNullOrWhiteSpace(searchModel.WorkCenter))
            {
                searchSql += " and WorkCenter=? ";
                paramArr.Add(searchModel.WorkCenter);
            }
            else
            {
                searchSql += " and op.WorkCenter is not null and op.WorkCenter<>'' ";

            }
            if (!string.IsNullOrWhiteSpace(searchModel.Flow))
            {
                searchSql += " and Flow=? ";
                paramArr.Add(searchModel.Flow);
            }

            searchSql += " and om.CompleteDate between ? and ?  group by op.WorkCenter,om.Flow ";
            paramArr.Add(searchModel.DateFrom.Value);
            paramArr.Add(searchModel.DateTo.Value);
            if (command.SortDescriptors.Count == 0)
            {
                searchSql += " order by op.WorkCenter asc";
            }
            else
            {
                searchSql += HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            }

            IList<object[]> searchResult = this.genericMgr.FindAllWithNativeSql<object[]>(searchSql,paramArr.ToArray());
            var returnResult = new List<OrderOperationReport>();
            if (searchResult != null && searchResult.Count > 0)
            {
                returnResult = (from tak in searchResult
                                select new OrderOperationReport
                                {
                                    WorkCenter = (string)tak[0],
                                    Flow = (string)tak[1],
                                    ReportQty = (decimal)tak[2],
                                }).ToList();
            }

            GridModel<OrderOperationReport> returnGrid = new GridModel<OrderOperationReport>();
            returnGrid.Total = returnResult.Count;
            returnGrid.Data = returnResult.Skip((command.Page - 1) * command.PageSize).Take(command.PageSize);
            return PartialView(returnGrid);
        }

        [GridAction]
        public ActionResult _AjaxDetailList(string flow,string workCenter,DateTime dateFrom,DateTime dateTo)
        {
            string searchSql = @"select om.Flow,op.WorkCenter,om.CompleteDate,op.ReportQty,om.TraceCode from ORD_OrderOpReport as op inner join ORD_OrderMstr_4 as om on op.OrderNo=om.OrderNo 
  where om.ProdLineType in (1,2,3,4,9) and (om.Status = 3 or om.Status = 4) and op.Status=0  ";
            IList<object> paramArr = new List<object>();
            if (!string.IsNullOrWhiteSpace(workCenter))
            {
                searchSql += " and WorkCenter=? ";
                paramArr.Add(workCenter);
            }
            else
            {
                searchSql += " and op.WorkCenter is not null and op.WorkCenter<>'' ";

            }
            if (!string.IsNullOrWhiteSpace(flow))
            {
                searchSql += " and Flow=? ";
                paramArr.Add(flow);
            }

            searchSql += " and om.CompleteDate between ? and ?  ";
            paramArr.Add(dateFrom);
            paramArr.Add(dateTo);

            searchSql += " order by om.CompleteDate asc";

            IList<object[]> searchResult = this.genericMgr.FindAllWithNativeSql<object[]>(searchSql, paramArr.ToArray());
            var returnResult = new List<OrderOperationReport>();
            if (searchResult != null && searchResult.Count > 0)
            {
                returnResult = (from tak in searchResult
                                select new OrderOperationReport
                                {
                                    Flow = (string)tak[0],
                                    WorkCenter = (string)tak[1],
                                    EffectiveDate = (DateTime)tak[2],
                                    ReportQty = (decimal)tak[3],
                                    VanNo = (string)tak[4],
                                }).ToList();
            }

            return PartialView(new GridModel<OrderOperationReport>(returnResult));
        }

        public void SearchExportXLS(OrderMasterSearchModel searchModel)
        {
            string searchSql = @"select op.WorkCenter,om.Flow,SUM(op.ReportQty) as ReportQty from ORD_OrderOpReport as op
inner join ORD_OrderMstr_4  as om on op.OrderNo=om.OrderNo 
where om.ProdLineType in (1,2,3,4,9) and (om.Status = 3 or om.Status = 4) and op.Status=0 ";
            IList<object> paramArr = new List<object>();
            if (!string.IsNullOrWhiteSpace(searchModel.WorkCenter))
            {
                searchSql += " and WorkCenter=? ";
                paramArr.Add(searchModel.WorkCenter);
            }
            else
            {
                searchSql += " and op.WorkCenter is not null and op.WorkCenter<>'' ";

            }
            if (!string.IsNullOrWhiteSpace(searchModel.Flow))
            {
                searchSql += " and Flow=? ";
                paramArr.Add(searchModel.Flow);
            }

            searchSql += " and om.CompleteDate between ? and ?  group by op.WorkCenter,om.Flow ";
            paramArr.Add(searchModel.DateFrom.Value);
            paramArr.Add(searchModel.DateTo.Value);
           
            searchSql += " order by op.WorkCenter asc";

            IList<object[]> searchResult = this.genericMgr.FindAllWithNativeSql<object[]>(searchSql, paramArr.ToArray());
            var returnResult = new List<OrderOperationReport>();
            if (searchResult != null && searchResult.Count > 0)
            {
                returnResult = (from tak in searchResult
                                select new OrderOperationReport
                                {
                                    WorkCenter = (string)tak[0],
                                    Flow = (string)tak[1],
                                    ReportQty = (decimal)tak[2],
                                }).ToList();
            }
            ExportToXLS<OrderOperationReport>("SearchExportXLS", "xls", returnResult);
        }


        public void ExportDetail(OrderMasterSearchModel searchModel)
        {
            string searchSql = @"select om.Flow,op.WorkCenter,op.EffDate,op.ReportQty,om.TraceCode  from ORD_OrderOpReport as op inner join ORD_OrderMstr_4 as om on op.OrderNo=om.OrderNo 
  where om.ProdLineType in (1,2,3,4,9) and (om.Status = 3 or om.Status = 4) and op.Status=0   ";
            IList<object> paramArr = new List<object>();
            if (!string.IsNullOrWhiteSpace(searchModel.WorkCenter))
            {
                searchSql += " and WorkCenter=? ";
                paramArr.Add(searchModel.WorkCenter);
            }
            else
            {
                searchSql += " and op.WorkCenter is not null and op.WorkCenter<>'' ";

            }
            if (!string.IsNullOrWhiteSpace(searchModel.Flow))
            {
                searchSql += " and Flow=? ";
                paramArr.Add(searchModel.Flow);
            }

            searchSql += " and om.CompleteDate between ? and ?   ";
            paramArr.Add(searchModel.DateFrom.Value);
            paramArr.Add(searchModel.DateTo.Value);

            searchSql += " order by om.CompleteDate asc";

            IList<object[]> searchResult = this.genericMgr.FindAllWithNativeSql<object[]>(searchSql, paramArr.ToArray());
            var returnResult = new List<OrderOperationReport>();
            if (searchResult != null && searchResult.Count > 0)
            {
                returnResult = (from tak in searchResult
                                select new OrderOperationReport
                                {
                                    Flow = (string)tak[0],
                                    WorkCenter = (string)tak[1],
                                    EffectiveDate = (DateTime)tak[2],
                                    ReportQty = (decimal)tak[3],
                                    VanNo = (string)tak[4],
                                }).ToList();
            }
            ExportToXLS<OrderOperationReport>("ExportDetailXLS", "xls", returnResult);
        }


    }
}
