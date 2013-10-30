using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Iesi.Collections;
using com.Sconit.Entity.VIEW;
using com.Sconit.Web.Util;
using Telerik.Web.Mvc;
using com.Sconit.Web.Models;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.CUST;
using com.Sconit.Service;
using com.Sconit.Entity.Exception;

namespace com.Sconit.Web.Controllers.CUST
{
    public class DeferredFeedCounterController : WebAppBaseController
    {
        private static string selectCountStatement = "select count(*) from VanOrderSeqView as o";

        private static string selectStatement = "from VanOrderSeqView as o";

        public IOrderMgr orderMgr { get; set; }

        #region 空车上线
        public ActionResult Index()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_DeferredFeedCounter")]
        [GridAction]
        public ActionResult List(GridCommand command, string Flow)
        {
            if (string.IsNullOrWhiteSpace(Flow))
            {
                SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, @Resources.ORD.OrderMaster.OrderMaster_Flow_Production);
            }
            ViewBag.Flow = Flow;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [SconitAuthorize(Permissions = "Url_DeferredFeedCounter")]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxList(GridCommand command, string Flow)
        {
            if (string.IsNullOrWhiteSpace(Flow))
            {
                return PartialView(new GridModel { Data = new List<VanOrderSeqView>() });
            }
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, Flow);
            return PartialView(GetAjaxPageData<VanOrderSeqView>(searchStatementModel, command));
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_DeferredFeedCounter")]
        public JsonResult StartEmptyVanOrder(string Flow)
        {
            try
            {
                orderMgr.StartEmptyVanOrder(Flow);
                SaveSuccessMessage("生产线" + Flow + "空车上线成功。");
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

        private SearchStatementModel PrepareSearchStatement(GridCommand command, string Flow)
        {
            string whereStatement = "where (o.Status = ? or o.Status is null)";
            IList<object> param = new List<object>();
            param.Add(CodeMaster.OrderStatus.InProcess);

            HqlStatementHelper.AddEqStatement("Flow", Flow, "o", ref whereStatement, param);

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = "order by Sequence asc,SubSequence asc";
            }

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }
        #endregion

        #region 空车上线取消
        [SconitAuthorize(Permissions = "Url_DeferredFeedCounter_Cancel")]
        public ActionResult CancelIndex()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_DeferredFeedCounter_Cancel")]
        [GridAction]
        public ActionResult CancelList(GridCommand command, string Flow)
        {
            if (string.IsNullOrWhiteSpace(Flow))
            {
                SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, @Resources.ORD.OrderMaster.OrderMaster_Flow_Production);
            }
            ViewBag.Flow = Flow;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [SconitAuthorize(Permissions = "Url_DeferredFeedCounter_Cancel")]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _CancelAjaxList(GridCommand command, string Flow)
        {
            if (string.IsNullOrWhiteSpace(Flow))
            {
                return PartialView(new GridModel { Data = new List<VanOrderSeqView>() });
            }
            SearchStatementModel searchStatementModel = PrepareCancelSearchStatement(command, Flow);
            return PartialView(GetAjaxPageData<VanOrderSeqView>(searchStatementModel, command));
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_DeferredFeedCounter_Cancel")]
        public JsonResult CancelEmptyVanOrder(string flow, int id)
        {
            try
            {
                orderMgr.CancelEmptyVanOrder(flow, id);
                SaveSuccessMessage("生产线" + flow + "空车上线取消成功。");
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

        private SearchStatementModel PrepareCancelSearchStatement(GridCommand command, string Flow)
        {
            string whereStatement = "where (o.Status = ? or o.Status is null)";
            IList<object> param = new List<object>();
            param.Add(CodeMaster.OrderStatus.InProcess);

            HqlStatementHelper.AddEqStatement("Flow", Flow, "o", ref whereStatement, param);

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = "order by Sequence asc,SubSequence asc";
            }

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }
        #endregion
    }
}
