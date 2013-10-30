using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using com.Sconit.Entity.Exception;
using com.Sconit.Service;
using com.Sconit.Web.Util;
using Telerik.Web.Mvc;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels.LOG;
using com.Sconit.Entity.LOG;
namespace com.Sconit.Web.Controllers.LOG
{
    public class OrderTraceController : WebAppBaseController
    {
        //
        // GET: /ItemTrace/

        private static string selectCountStatement = "select count(*) from OrderTrace as j";

        private static string selectStatement = "select j from OrderTrace as j";


        //
        // GET: /FailCode/
        #region  public
        public ActionResult Index()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OrderTrace_View")]
        [GridAction]
        public ActionResult List(GridCommand command, VanOrderTraceSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OrderTrace_View")]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxList(GridCommand command, VanOrderTraceSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<OrderTrace>(searchStatementModel, command));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderTrace_View")]
        public ActionResult _AjaxOrderTraceDetail(GridCommand command, string uUID)
        {
            IList<OrderTraceDetail> orderTraceDetailList = genericMgr.FindAll<OrderTraceDetail>("from OrderTraceDetail as d where d.UUID=? order by CreateDate ", uUID);
            return PartialView(new GridModel(orderTraceDetailList));

        }

        #endregion

        #region private
        private SearchStatementModel PrepareSearchStatement(GridCommand command, VanOrderTraceSearchModel searchModel)
        {
            string whereStatement = string.Empty;
            IList<object> param = new List<object>();

            HqlStatementHelper.AddEqStatement("Item", searchModel.Item,  "j", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "j", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("OrderNo", searchModel.OrderNo, HqlStatementHelper.LikeMatchMode.Anywhere, "j", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("OpReference", searchModel.OpReference, HqlStatementHelper.LikeMatchMode.Anywhere, "j", ref whereStatement, param);

            if (searchModel.CreateDateFrom.HasValue)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.CreateDateFrom.Value, "j", ref whereStatement, param);
            }
            if (searchModel.CreateDateTo.HasValue)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.CreateDateTo.Value, "j", ref whereStatement, param);
            }

            if (command.SortDescriptors.Count > 0)
            {
                 if (command.SortDescriptors[0].Member == "OrderPriorityDescription")
                {
                    command.SortDescriptors[0].Member = "Priority";
                }
            }

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            sortingStatement = string.IsNullOrWhiteSpace(sortingStatement) ? " order by j.CreateDate asc" : sortingStatement;
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
