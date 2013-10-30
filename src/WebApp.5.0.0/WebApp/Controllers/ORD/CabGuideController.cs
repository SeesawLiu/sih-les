using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Telerik.Web.Mvc;
using com.Sconit.CodeMaster;
using com.Sconit.Entity.CUST;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.ORD;
using com.Sconit.PrintModel.ORD;
using com.Sconit.Service;
using com.Sconit.Utility.Report;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels;
using com.Sconit.Web.Models.SearchModels.MD;
using com.Sconit.Web.Util;

namespace com.Sconit.Web.Controllers.ORD
{
    public class CabGuideController : WebAppBaseController
    {
        public IOrderMgr OrderMgr { get; set; }
        public IReportGen reportGen { get; set; }

        private static string selectStatement =
            "select mstr.OrderNo,mstr.TraceCode,det.Item as Model,det.ItemDesc as ModelDescription,co.CabItem as Item,co.CabItemDesc as ItemDesc,cp.CPTime as StartTime,mstr.Flow,mstr.Type,mstr.Status,det.ExtSeq,det.Id as OrderDetailId,co.Status as CabOutStatus,os.Seq,os.SubSeq " +
            "from CUST_CabOut as co WITH(NOLOCK) " +
            "inner join (select OrderNo, MIN(CPTime) as CPTime from ORD_OrderOpCPTime WITH(NOLOCK) where VanProdLine = AssProdLine and Op = 1 group by OrderNo) as cp on cp.OrderNo = co.OrderNo " +
            "inner join ORD_OrderMstr_4 as mstr WITH(NOLOCK) on co.OrderNo=mstr.OrderNo " +
            "inner join ORD_OrderDet_4 as det WITH(NOLOCK) on det.OrderNo=mstr.OrderNo " +
            "inner join ORD_OrderSeq as os WITH(NOLOCK) on co.OrderNo=os.OrderNo " +
            "where mstr.PauseStatus = 0 ";

        [SconitAuthorize(Permissions = "Url_CabGuideOutSoureSubView_View")]
        public ActionResult Index()
        {
            return View();
        }

        #region 外购
        [SconitAuthorize(Permissions = "Url_CabGuideOutSoureView_View")]
        public ActionResult OutSoureViewIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_CabGuideOutSoureView_View")]
        public ActionResult OutSoureList(GridCommand command, CabProductionViewSearchModel searchModel)
        {
            this.ProcessSearchModel(command, searchModel);
            return View();
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_CabGuideOutSoureView_View")]
        public ActionResult _OutSoureView(GridCommand command, CabProductionViewSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_CabGuideOutSoureView_View")]
        public ActionResult _OutSoureView2(GridCommand command, CabProductionViewSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_CabGuideOutSoureView_View,Url_CabGuideOutSoureSubView_View")]
        public ActionResult _AjaxOutSoureViewList(GridCommand command, CabProductionViewSearchModel searchModel)
        {
            SearchNativeSqlStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            var list = GetPageDataEntityWithNativeSql<CabProductionView>(searchStatementModel, command);
            ViewBag.Total1 = list.Total;
            return PartialView(list);
        }
        #endregion

        #region 外购分线
        [SconitAuthorize(Permissions = "Url_CabGuideOutSoureSubView_View")]
        public ActionResult OutSoureSubViewIndex()
        {
            return View();
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_CabGuideOutSoureSubView_View")]
        public ActionResult _OutSoureSubView(GridCommand command, CabProductionViewSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_CabGuideOutSoureSubView_View")]
        public ActionResult _OutSoureSubView2(GridCommand command, CabProductionViewSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }
        #endregion

        #region 自制
        [SconitAuthorize(Permissions = "Url_CabGuideHomeMadeView_View")]
        public ActionResult HomeMadeViewIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_CabGuideHomeMadeView_View")]
        public ActionResult HomeMadeList(GridCommand command, CabProductionViewSearchModel searchModel)
        {
            this.ProcessSearchModel(command, searchModel);
            return View();
        }

        [HttpGet]
        [GridAction]
        [SconitAuthorize(Permissions = "Url_CabGuideHomeMadeView_View")]
        public ActionResult _HomeMadeView(GridCommand command, CabProductionViewSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [HttpGet]
        [GridAction]
        [SconitAuthorize(Permissions = "Url_CabGuideHomeMadeView_View")]
        public ActionResult _HomeMadeView2(GridCommand command, CabProductionViewSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_CabGuideHomeMadeView_View,Url_CabGuideHomeMadeSubView_View")]
        public ActionResult _AjaxHomeMadeViewList(GridCommand command, CabProductionViewSearchModel searchModel)
        {
            SearchNativeSqlStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            var list = GetPageDataEntityWithNativeSql<CabProductionView>(searchStatementModel, command);
            ViewBag.Total1 = list.Total;
            return PartialView(list);
        }
        #endregion

        #region 自制分线
        [SconitAuthorize(Permissions = "Url_CabGuideHomeMadeSubView_View")]
        public ActionResult HomeMadeSubViewIndex()
        {
            return View();
        }

        [HttpGet]
        [GridAction]
        [SconitAuthorize(Permissions = "Url_CabGuideHomeMadeSubView_View")]
        public ActionResult _HomeMadeSubView(GridCommand command, CabProductionViewSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [HttpGet]
        [GridAction]
        [SconitAuthorize(Permissions = "Url_CabGuideHomeMadeSubView_View")]
        public ActionResult _HomeMadeSubView2(GridCommand command, CabProductionViewSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }
        #endregion

        [SconitAuthorize(Permissions = "Url_CabGuideHomeMadeView_View,Url_CabGuideOutSoureView_View,Url_CabGuideHomeMadeSubView_View,Url_CabGuideOutSoureSubView_View")]
        public JsonResult CabOut(string orderNo, CabType? type)
        {
            try
            {
                OrderMgr.OutCab(orderNo);
                SaveSuccessMessage("驾驶室生产单{0}出库成功", orderNo);

                // 外购出库打印生产单
                if (type != null && type == CabType.Purchase)
                {
                    OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);
                    IList<OrderDetail> orderDetails = base.genericMgr.FindAll<OrderDetail>("select od from OrderDetail as od where od.OrderNo=?", orderNo);
                    IList<OrderBomDetail> orderBomDetails = base.genericMgr.FindAll<OrderBomDetail>("select bm from OrderBomDetail as bm where bm.OrderNo=?", orderNo);
                    IList<PrintOrderBomDetail> printOrderBomDetails = Mapper.Map<IList<OrderBomDetail>, IList<PrintOrderBomDetail>>(orderBomDetails);
                    orderMaster.OrderDetails = orderDetails;
                    PrintOrderMaster printOrderMstr = Mapper.Map<OrderMaster, PrintOrderMaster>(orderMaster);
                    IList<object> data = new List<object>();
                    data.Add(printOrderMstr);
                    data.Add(printOrderMstr.OrderDetails);
                    data.Add(printOrderBomDetails);
                    string reportFileUrl = reportGen.WriteToFile(orderMaster.OrderTemplate, data);

                    return Json(new { PrintUrl = reportFileUrl });
                }

                return Json(new { });
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }
            return Json(null);
        }

        private SearchNativeSqlStatementModel PrepareSearchStatement(GridCommand command, CabProductionViewSearchModel searchModel)
        {
            string statement = selectStatement;
            if (!string.IsNullOrWhiteSpace(searchModel.Flow))
            {
                statement += string.Format(" and mstr.Flow = '{0}'", searchModel.Flow);
            }

            //驾驶室已出库记录覆盖时长
            int coveredDays = int.Parse(systemMgr.GetEntityPreferenceValue(com.Sconit.Entity.SYS.EntityPreference.CodeEnum.CoveredDaysOfCabOut));
            statement += string.Format(" and co.CabType = {0} and co.Status in ({1})", (int)searchModel.Type, searchModel.IsOut ? "1,2" : "0");

            if (searchModel.IsOut)
                statement += string.Format(" and co.OutDate Between '" + DateTime.Today.AddDays(1 - coveredDays) + "' and '" + DateTime.Today.AddDays(1) + "'");

            if (command.SortDescriptors != null && command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "CabOutStatusDescription")
                {
                    command.SortDescriptors[0].Member = "CabOutStatus";
                }
            }
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            if (string.IsNullOrEmpty(sortingStatement))
            {
                sortingStatement = " order by StartTime asc";
            }
            SearchNativeSqlStatementModel searchStatementModel = new SearchNativeSqlStatementModel();
            searchStatementModel.SelectSql = statement;
            searchStatementModel.SortingStatement = sortingStatement;
            return searchStatementModel;
        }
    }
}
