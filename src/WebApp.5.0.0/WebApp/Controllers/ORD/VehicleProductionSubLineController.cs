using System.Web.Mvc;
using Telerik.Web.Mvc;
using com.Sconit.Entity.ORD;
using com.Sconit.Service;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels;
using com.Sconit.Web.Util;

namespace com.Sconit.Web.Controllers.ORD
{
    public class VehicleProductionSubLineController : WebAppBaseController
    {
        //todo: 将m.PartyFrom,m.PartyFromNm 改为车型，车型描述
        /// <summary>
        /// 生产线视图
        /// </summary>
        private static string selectStatement = @"select m.TraceCode,m.PartyFrom as Model,m.PartyFromNm as ModelDescription,d.Item,d.ItemDesc,m.StartTime,m.Flow,m.Type,m.Status,d.ExtSeq,d.Id as OrderDetailId from ORD_OrderMstr_4 m WITH(NOLOCK) left join  ORD_OrderDet_4 d WITH(NOLOCK) on m.OrderNo=d.OrderNo left join ORD_OrderBomDet bd WITH(NOLOCK) on bd.OrderNo=m.OrderNo where m.Status=" + (int)CodeMaster.OrderStatus.Submit;

        [SconitAuthorize(Permissions = "Url_VehicleProductionSubLineView_View")]
        public ActionResult Index()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_VehicleProductionSubLineView_View")]
        public ActionResult List()
        {
            return View();
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_VehicleProductionSubLineView_View")]
        public ActionResult _View(GridCommand command, CabProductionViewSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_VehicleProductionSubLineView_View")]
        public ActionResult _View2(GridCommand command, CabProductionViewSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_VehicleProductionSubLineView_View")]
        public ActionResult _AjaxList(GridCommand command, CabProductionViewSearchModel searchModel)
        {
            SearchNativeSqlStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            var list = GetPageDataEntityWithNativeSql<CabProductionView>(searchStatementModel, command);
            ViewBag.Total1 = list.Total;
            return PartialView(list);
        }

        private SearchNativeSqlStatementModel PrepareSearchStatement(GridCommand command, CabProductionViewSearchModel searchModel)
        {
            string statement = selectStatement;
            if (!string.IsNullOrWhiteSpace(searchModel.Flow))
            {
                statement += string.Format(" and m.Flow = '{0}'", searchModel.Flow);
            }

            if (searchModel.Type != null)
            {
                statement += string.Format(" and m.Type = {0}", searchModel.Type);
            }

            if (searchModel.IsOut)
            {
                statement += " and exists(select 1 from ORD_OrderMstr_2 m2 where m2.OrderNo = m.ExtOrderNo)";
            }

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            if (string.IsNullOrEmpty(sortingStatement))
            {
                sortingStatement = " order by ExtSeq asc";
            }
            SearchNativeSqlStatementModel searchStatementModel = new SearchNativeSqlStatementModel();
            searchStatementModel.SelectSql = statement;
            searchStatementModel.SortingStatement = sortingStatement;
            return searchStatementModel;
        }
    }
}
