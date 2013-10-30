namespace com.Sconit.Web.Controllers.PRD
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using com.Sconit.Entity.PRD;
    using com.Sconit.Service;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Models.SearchModels.PRD;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;
    using com.Sconit.Entity.SCM;
    using System;
    using com.Sconit.Entity.Exception;
    using com.Sconit.Entity.INV;
    using com.Sconit.Entity.MD;
    using com.Sconit.Utility;
    using com.Sconit.Entity.ORD;
    using com.Sconit.Web.Models.SearchModels.ORD;

    public class ProdIOController : WebAppBaseController
    {     
        private static string selectDetailCountStatement = "select count(*) from OrderDetail as d";

        private static string selectDetailStatement = "select d from OrderDetail as d";

        private static string selectOrderBackflushDetailStatement = "select b from OrderBackflushDetail as b";

        [SconitAuthorize(Permissions = "Url_Production_ProdIO")]
        public ActionResult Index()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_Production_ProdIO")]
        public ActionResult List(GridCommand command, OrderMasterSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            if (this.CheckSearchModelIsNull(searchCacheModel.SearchObject))
            {
                TempData["_AjaxMessage"] = "";
            }
            else
            {
                SaveWarningMessage(Resources.ErrorMessage.Errors_NoConditions);
            }
            ViewBag.DisplayType = searchModel.DisplayType;
            ViewBag.DetailItem = searchModel.DetailItem;
            return View();
        }

        #region detail
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Production_ProdIO")]
        public ActionResult _OrderDetailHierarchyAjax(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<OrderDetail>()));
            }
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel, false);
            return PartialView(GetAjaxPageData<OrderDetail>(searchStatementModel, command));

        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Production_ProdIO")]
        public ActionResult _OrderBackflushDetailHierarchyAjax(int orderDetailId,string item)
        {
            if (string.IsNullOrEmpty(item))
            {
                string hql = "select f from OrderBackflushDetail as f where f.OrderDetailId = ?";
                IList<OrderBackflushDetail> orderBackflushDetailList = base.genericMgr.FindAll<OrderBackflushDetail>(hql, orderDetailId);
                return PartialView(new GridModel(orderBackflushDetailList));
            }
            else
            {
                string hql = "select f from OrderBackflushDetail as f where f.OrderDetailId = ? and f.Item=?";
                IList<OrderBackflushDetail> orderBackflushDetailList = base.genericMgr.FindAll<OrderBackflushDetail>(hql, new object[] { orderDetailId, item });
                return PartialView(new GridModel(orderBackflushDetailList));
            }
        }
        #endregion

        #region summary
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Production_ProdIO")]
        public ActionResult _OrderSummaryHierarchyAjax(GridCommand command, OrderMasterSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel, true);
            IList<OrderDetail> orderDetailList = base.genericMgr.FindAll<OrderDetail>(searchStatementModel.GetSearchStatement(), searchStatementModel.Parameters);
            var summaryOrderDetailList = from d in orderDetailList
                                         group d by new { d.Item, d.ItemDescription, d.ReferenceItemCode, d.Uom } into result
                                         select new OrderDetail
                                         {
                                             Item = result.Key.Item,
                                             ItemDescription = result.Key.ItemDescription,
                                             ReferenceItemCode = result.Key.ReferenceItemCode,
                                             Uom = result.Key.Uom,
                                             ReceivedQty = result.Sum(r => r.ReceivedQty)
                                         };

            return PartialView(new GridModel(summaryOrderDetailList));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Production_ProdIO")]
        public ActionResult _OrderBackflushSummaryHierarchyAjax(GridCommand command, OrderMasterSearchModel searchModel, string item)
        {
            searchModel.Item = item;
            SearchStatementModel searchStatementModel = this.PrepareSearchSummaryStatement(command, searchModel);
            IList<OrderBackflushDetail> orderBackflushDetailList = base.genericMgr.FindAll<OrderBackflushDetail>(searchStatementModel.GetSearchStatement(), searchStatementModel.Parameters);
            var summaryOrderBackflushDetailList = from d in orderBackflushDetailList
                                                  group d by new { d.Item, d.ItemDescription, d.ReferenceItemCode, d.Uom } into result
                                                  select new OrderBackflushDetail
                                         {
                                             Item = result.Key.Item,
                                             ItemDescription = result.Key.ItemDescription,
                                             ReferenceItemCode = result.Key.ReferenceItemCode,
                                             Uom = result.Key.Uom,
                                             BackflushedQty = result.Sum(r => r.BackflushedQty)
                                         };

            return PartialView(new GridModel(summaryOrderBackflushDetailList));
        }
        #endregion

        private SearchStatementModel PrepareSearchStatement(GridCommand command, OrderMasterSearchModel searchModel, bool isSummary)
        {
            string whereStatement = "where d.OrderType = ?  and d.ReceivedQty > 0 and exists(select 1 from OrderMaster as m where d.OrderNo = m.OrderNo ";

            IList<object> param = new List<object>();

            param.Add((int)com.Sconit.CodeMaster.OrderType.Production);

            HqlStatementHelper.AddEqStatement("OrderNo", searchModel.OrderNo, "m", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "m", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("PartyTo", searchModel.PartyTo, "m", ref whereStatement, param);

            if (searchModel.DateFrom != null)
            {
                HqlStatementHelper.AddGeStatement("StartDate", searchModel.DateFrom, "m", ref whereStatement, param);
            }
            if (searchModel.DateTo != null)
            {
                HqlStatementHelper.AddLeStatement("StartDate", searchModel.DateTo, "m", ref whereStatement, param);
            }
            SecurityHelper.AddRegionPermissionStatement(ref whereStatement, "m", "PartyTo");
            whereStatement += ")";
            if (!string.IsNullOrEmpty(searchModel.DetailItem))
            {
                whereStatement += "and exists (select 1 from OrderBackflushDetail as f where f.OrderDetailId=d.Id and f.Item='"+searchModel.DetailItem+"' )";
            }
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "d", ref whereStatement, param);
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            if (!isSummary)
            {
                if (command.SortDescriptors.Count == 0)
                {
                    sortingStatement = " order by d.OrderNo desc";
                }
            }

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectDetailCountStatement;
            searchStatementModel.SelectStatement = selectDetailStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }


        private SearchStatementModel PrepareSearchSummaryStatement(GridCommand command, OrderMasterSearchModel searchModel)
        {
            string whereStatement = "where b.Item = ? and exists(select 1 from OrderMaster as m,OrderDetail as d where d.OrderNo = m.OrderNo and d.OrderType = m.Type and m.Type = ? and d.Item = ? and b.OrderDetailId = d.Id";
            IList<object> param = new List<object>();
            param.Add(searchModel.Item);
            param.Add((int)com.Sconit.CodeMaster.OrderType.Production);
            param.Add(searchModel.Item);

            HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "m", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("PartyTo", searchModel.PartyTo, "m", ref whereStatement, param);

            if (searchModel.DateFrom != null)
            {
                HqlStatementHelper.AddGeStatement("StartDate", searchModel.DateFrom, "m", ref whereStatement, param);
            }
            if (searchModel.DateTo != null)
            {
                HqlStatementHelper.AddLeStatement("StartDate", searchModel.DateTo, "m", ref whereStatement, param);
            }
            SecurityHelper.AddRegionPermissionStatement(ref whereStatement, "m", "PartyTo");
            whereStatement += ")";


            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = string.Empty;
            searchStatementModel.SelectStatement = selectOrderBackflushDetailStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = string.Empty;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }
    }
}
