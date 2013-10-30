using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Telerik.Web.Mvc;
using com.Sconit.Entity.SYS;
using com.Sconit.Entity.VIEW;
using com.Sconit.Entity.ORD;
using com.Sconit.Service;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels.ORD;
using com.Sconit.Web.Util;

namespace com.Sconit.Web.Controllers.ORD
{
    public class AliquotStartTaskController : WebAppBaseController
    {
        #region public actions

        /// <summary>
        /// Index action for AliquotStartTask controller
        /// </summary>
        /// <returns>Index view</returns>
        [SconitAuthorize(Permissions = "Url_AliquotStartTask_View")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// List action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">AliquotStartTask Search model</param>
        /// <returns>return the result view</returns>
        [SconitAuthorize(Permissions = "Url_AliquotStartTask_View")]
        public ActionResult List(GridCommand command, AliquotStartTaskSearchModel searchModel)
        {
            if (string.IsNullOrWhiteSpace(searchModel.SearchFlow))
            {
                SaveWarningMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.ORD.AliquotStartTask.AliquotStartTask_KitFlow);
            }
            this.ProcessSearchModel(command, searchModel);
            return View();
        }

        /// <summary>
        ///  AjaxList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">AliquotStartTask Search Model</param>
        /// <returns>return the result action</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_AliquotStartTask_View")]
        public ActionResult _AjaxList(GridCommand command, AliquotStartTaskSearchModel searchModel)
        {
            if (string.IsNullOrWhiteSpace(searchModel.SearchFlow))
            {
                return PartialView(new GridModel<AliquotStartTask> { Data = new List<AliquotStartTask>() });
            }

            SearchNativeSqlStatementModel searchStatementModel = this.PrepareStartSearchStatement(command, searchModel);
            var list = GetPageDataEntityWithNativeSql<AliquotStartTaskView>(searchStatementModel, command);
            return PartialView(list);
        }

        [SconitAuthorize(Permissions = "Url_AliquotStartTask_View")]
        public ActionResult _SearchResult(GridCommand command, AliquotStartTaskSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [SconitAuthorize(Permissions = "Url_AliquotStartTask_View")]
        public ActionResult _StartSearchResult(GridCommand command, AliquotStartTaskSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [SconitAuthorize(Permissions = "Url_AliquotStartTask_View")]
        public ActionResult Start(string orderNos, string flow)
        {
            var orderNoTraceCodeList = orderNos.Split(',');
            foreach (var obj in orderNoTraceCodeList)
            {
                var objs = obj.Split('|');
                base.genericMgr.Create(new AliquotStartTask
                                        {
                                            Flow = flow,
                                            OrderNo = objs[0],
                                            VanNo = objs[1],
                                            StartTime = DateTime.Now,
                                        });
            }

            SaveSuccessMessage("上线成功。");
            return RedirectToAction("List", new RouteValueDictionary { { "SearchFlow", flow } });
        }

        [SconitAuthorize(Permissions = "Url_AliquotStartTask_View")]
        public ActionResult Cancel(string orderNos, string flow)
        {
            var orderNoTraceCodeList = orderNos.Split(',');
            foreach (var obj in orderNoTraceCodeList)
            {
                var objs = obj.Split('|');
                genericMgr.UpdateWithNativeQuery(
                    string.Format("delete from ORD_AliquotStartTask where OrderNo ='{0}' and VanNo='{1}'", objs[0], objs[1]));
            }

            SaveSuccessMessage("取消成功。");
            return RedirectToAction("List", new RouteValueDictionary { { "SearchFlow", flow } });
        }

        public void ExportXLS(GridCommand command, AliquotStartTaskSearchModel searchModel)
        {
            string maxRows = this.systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.ExportMaxRows);
            command.PageSize = int.Parse(maxRows);
            SearchNativeSqlStatementModel searchStatementModel = this.PrepareStartSearchStatement(command, searchModel);
            var list = GetPageDataEntityWithNativeSql<AliquotStartTaskView>(searchStatementModel, command);
            ExportToXLS<AliquotStartTaskView>("AliquotStartTaskView", "XLS", list.Data.ToList());
        }
        #endregion

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">AliquotStartTask Search Model</param>
        /// <returns>return AliquotStartTask search model</returns>
        private SearchNativeSqlStatementModel PrepareStartSearchStatement(GridCommand command, AliquotStartTaskSearchModel searchModel)
        {
            int coveredDays = int.Parse(systemMgr.GetEntityPreferenceValue(com.Sconit.Entity.SYS.EntityPreference.CodeEnum.CoveredDaysOfKitProduction));
            var selectStatement = string.Empty;
            if (searchModel.SearchIsStart)
            {
                selectStatement = " select m.OrderNo,m.Flow,m.TraceCode,d.Item,d.ItemDesc,t.StartTime from ORD_AliquotStartTask t WITH(NOLOCK) " +
                                          " inner join ORD_OrderMstr_4 m WITH(NOLOCK) on t.OrderNo=m.OrderNo inner join ORD_OrderDet_4 d WITH(NOLOCK) on m.Orderno=d.OrderNo" +
                                          " where t.Flow='" + searchModel.SearchFlow + "' and t.StartTime Between '" + DateTime.Today.AddDays(1 - coveredDays) + "' and '" + DateTime.Today.AddDays(1) + "'";
            }
            else
            {
                selectStatement = "select m.OrderNo,m.Flow,m.TraceCode,d.Item,d.ItemDesc,Min(p.CPTime) as StartTime from ORD_OrderOpCPTime p WITH(NOLOCK) " +
                    " inner join ORD_OrderMstr_4 m WITH(NOLOCK) on p.OrderNo=m.OrderNo inner join ORD_OrderDet_4 d WITH(NOLOCK) on m.OrderNo=d.OrderNo" +
                    " where not exists(select 1 from ORD_AliquotStartTask t WITH(NOLOCK) where t.Flow=p.AssProdLine and m.TraceCode =t.VanNo and m.OrderNo=t.OrderNo) " +
                    " and p.AssProdLine ='" + searchModel.SearchFlow + "'";
                if (!string.IsNullOrWhiteSpace(searchModel.SearchVanFlow))
                    selectStatement += string.Format(" and p.VanProdLine ='{0}'", searchModel.SearchVanFlow);

                selectStatement += " group by m.OrderNo,m.Flow,m.TraceCode,d.Item,d.ItemDesc";
            }

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            if (string.IsNullOrWhiteSpace(sortingStatement))
            {
                if (!searchModel.SearchIsStart)
                    sortingStatement = " order by StartTime asc ";
                else
                    sortingStatement = " order by StartTime desc ";
            }

            SearchNativeSqlStatementModel searchStatementModel = new SearchNativeSqlStatementModel();
            searchStatementModel.SelectSql = selectStatement;
            searchStatementModel.SortingStatement = sortingStatement;

            return searchStatementModel;
        }
    }
}
