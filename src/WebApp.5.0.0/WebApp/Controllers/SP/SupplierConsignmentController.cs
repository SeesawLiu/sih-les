using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using com.Sconit.Web.Util;
using com.Sconit.Web.Models.SearchModels.ORD;
using com.Sconit.Web.Models;
using com.Sconit.Entity.ORD;
using System.Text;
using com.Sconit.Service;
using com.Sconit.Utility;
using com.Sconit.Web.Models.SearchModels.BIL;
using com.Sconit.Entity.BIL;


namespace com.Sconit.Web.Controllers.SP
{
    public class SupplierConsignmentController : WebAppBaseController
    {


        private static string selectCountStatement = "select count(*) from PlanBill as p";

        /// <summary>
        /// 
        /// </summary>
        private static string selectStatement = "select p from PlanBill as p";

        #region public
        public ActionResult Index()
        {
            return View();
        }


        [GridAction]
        [SconitAuthorize(Permissions = "Url_Supplier_Consignment")]
        public ActionResult List(GridCommand command, PlanBillSearchModel searchModel)
        {
            TempData["PlanBillSearchModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Supplier_Consignment")]
        public ActionResult _AjaxList(GridCommand command, PlanBillSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<PlanBill>(searchStatementModel, command));
        }



        #endregion

        #region private


        private SearchStatementModel PrepareSearchStatement(GridCommand command, PlanBillSearchModel searchModel)
        {
            string whereStatement = string.Empty;
            IList<object> param = new List<object>();

            SecurityHelper.AddPartyFromPermissionStatement(ref whereStatement, "p", "Party", com.Sconit.CodeMaster.OrderType.Procurement, true);

            HqlStatementHelper.AddLikeStatement("OrderNo", searchModel.OrderNo, HqlStatementHelper.LikeMatchMode.Start, "p", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("ReceiptNo", searchModel.ReceiptNo, HqlStatementHelper.LikeMatchMode.Start, "p", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "p", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Party", searchModel.Party, "p", ref whereStatement, param);


            if (searchModel.CreateDate_start != null & searchModel.CreateDate_End != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.CreateDate_start, searchModel.CreateDate_End, "p", ref whereStatement, param);
            }
            else if (searchModel.CreateDate_start != null & searchModel.CreateDate_End == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.CreateDate_start, "p", ref whereStatement, param);
            }
            else if (searchModel.CreateDate_start == null & searchModel.CreateDate_End != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.CreateDate_End, "p", ref whereStatement, param);
            }
            if (whereStatement == string.Empty)
                whereStatement += " where p.PlanQty>p.ActingQty";
            else
                whereStatement += " and p.PlanQty>p.ActingQty";
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

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
