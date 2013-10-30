using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using com.Sconit.Entity.CUST;
using com.Sconit.Entity.SYS;
using com.Sconit.Service;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels.CUST;
using com.Sconit.Web.Util;
using com.Sconit.Entity.SCM;
using com.Sconit.Web.Models.SearchModels.SCM;

namespace com.Sconit.Web.Controllers.SCM
{
    /// <summary>
    ///  生产节拍调整
    /// </summary>
    public class FlowTaktAjustController : WebAppBaseController
    {
        private static string selectCountStatement = "select count(*) from FlowMaster as r";

        private static string selectStatement = "select r from FlowMaster as r";

        public IProductionLineMgr productionLineMgr { get; set; }

        [SconitAuthorize(Permissions = "Url_FlowTaktAjust_View")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [GridAction]
        [SconitAuthorize(Permissions = "Url_FlowTaktAjust_View")]
        public ActionResult List(GridCommand command, FlowTaktAjustSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            TempData["FlowTaktAjustSearchModel"] = searchModel;
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_FlowTaktAjust_View")]
        public ActionResult _AjaxList(GridCommand command, FlowTaktAjustSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<FlowMaster>(searchStatementModel, command));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_FlowTaktAjust_Edit")]
        public ActionResult _Update(string id, int? TaktTime, FlowTaktAjustSearchModel searchModel)
        {
            if (!TaktTime.HasValue)
            {
                SaveErrorMessage("节拍不能为空。");
            }
            else
            {
                this.productionLineMgr.AdjustProductLineTaktTime(id, TaktTime.Value);
            }

            var paramValues = new List<object>();

            var hql = "select r from FlowMaster as r where Type = 4 and ProdLineType in (1,2,3,4,9)";
            if (!string.IsNullOrWhiteSpace(searchModel.Code))
            {
                hql += " and r.Code = ? ";
                paramValues.Add(searchModel.Code + "%");
            }
            if (!string.IsNullOrWhiteSpace(searchModel.PartyFrom))
            {
                hql += " and r.PartyFrom like ? ";
                paramValues.Add(searchModel.PartyFrom + "%");
            }

            IList<FlowMaster> miscOrderMoveTypes = base.genericMgr.FindAll<FlowMaster>(hql, paramValues.ToArray());
            return View(new GridModel(miscOrderMoveTypes));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        private SearchStatementModel PrepareSearchStatement(GridCommand command, FlowTaktAjustSearchModel searchModel)
        {
            string whereStatement = " where Type = 4 and ProdLineType in (1,2,3,4,9)";
            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("PartyFrom", searchModel.PartyFrom, HqlStatementHelper.LikeMatchMode.Start, "r", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Code", searchModel.Code, "r", ref whereStatement, param);

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
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
