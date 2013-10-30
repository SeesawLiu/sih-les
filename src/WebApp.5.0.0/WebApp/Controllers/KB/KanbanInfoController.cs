namespace com.Sconit.Web.Controllers.KB
{
    #region reference
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Security;
    using com.Sconit.Entity.KB;
    using com.Sconit.Service;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;
    using NHibernate.Type;
    using com.Sconit.Web.Models.SearchModels.KB;
    using com.Sconit.Utility;
    using com.Sconit.Entity.Exception;
    #endregion

    public class KanbanInfoController : WebAppBaseController
    {
        private static string selectCountStatement = "select count(*) from KanbanCard  k";

        private static string selectStatement = "select k from KanbanCard as k";


        public IKanbanCardMgr kanbanCardMgr { get; set; }

        [SconitAuthorize(Permissions = "Url_KanbanInfo_Pace")]
        public ActionResult Pace()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_KanbanInfo_LostView")]
        public ActionResult Lost()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_KanbanInfo_LostView")]
        public ActionResult Index()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_KanbanInfo_LostView")]
        public ActionResult List(GridCommand command, KanbanCardSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_KanbanInfo_LostView")]
        public ActionResult _AjaxList(GridCommand command, KanbanCardSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<KanbanCard>(searchStatementModel, command));
        }

        [SconitAuthorize(Permissions = "Url_KanbanInfo_LostCalc")]
        public JsonResult TryCalc(string region, int ignoreTimeNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(region))
                {
                    throw new BusinessException("区域为必填项。");
                }
                kanbanCardMgr.TryCalcKanbanLost(region, ignoreTimeNumber);
                SaveSuccessMessage("看板遗失核算成功。");
                return Json(null);
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                return Json(null);
            }
        }

        [SconitAuthorize(Permissions = "Url_KanbanInfo_LostCalc")]
        public JsonResult TrySync()
        {
            try
            {
                genericMgr.FindAllWithNativeSql<KanbanCard>("Update KB_KanbanCard set LocBin = l.Bin from MD_LocationBinItem l where l.Location = KB_KanbanCard.Location and l.Item = KB_KanbanCard.Item and KB_KanbanCard.IsLost = ?", true);
                SaveSuccessMessage("库房架位更新成功。");
                return Json(null);
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                return Json(null);
            }
        }


        #region 查询导出
        public void ExportLostView(KanbanCardSearchModel searchModel)
        {
            
            IList<object> param = new List<object>();
            string hql =string.Format( "  select k from KanbanCard as k where IsLost={0} ",true);
            SecurityHelper.AddRegionPermissionStatement(ref selectStatement, "k", "Region");
            if (!string.IsNullOrEmpty(searchModel.Region))
            {
                // System.Text.StringBuilder sb = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(searchModel.Region))
                {
                    hql+=" and k.Region in (";
                    string[] regions = searchModel.Region.Split(',');
                    for (int ir = 0; ir < regions.Length; ir++)
                    {
                        hql+="?,";
                        param.Add(regions[ir]);
                    }
                    hql = hql.Substring(0, hql.Length - 1) + ")";
                }

            }
            if (!string.IsNullOrWhiteSpace(searchModel.BinTo))
            {
                hql += " and k.LocBin=? ";
                param.Add(searchModel.BinTo);
            }

            IList<KanbanCard> kanbanCardList = this.genericMgr.FindAll<KanbanCard>(hql, param.ToArray());
            ExportToXLS<KanbanCard>("ExportKanbanCard", "xls", kanbanCardList);

        }
        #endregion

        private SearchStatementModel PrepareSearchStatement(GridCommand command, KanbanCardSearchModel searchModel)
        {
            string whereStatement = string.Format(" where k.IsLost={0} ",true);

            IList<object> param = new List<object>();
            //区域权限
            SecurityHelper.AddRegionPermissionStatement(ref whereStatement, "k", "Region");
            if (!string.IsNullOrEmpty(searchModel.Region))
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(searchModel.Region))
                {
                    sb.Append(" and k.Region in (");
                    string[] regions = searchModel.Region.Split(',');
                    for (int ir = 0; ir < regions.Length; ir++)
                    {
                        sb.Append("?,");
                        param.Add(regions[ir]);
                    }
                    sb = sb.Remove(sb.Length - 1, 1);
                    whereStatement += sb.ToString() + ")";
                }

            }
            HqlStatementHelper.AddLikeStatement("BinTo", searchModel.BinTo, HqlStatementHelper.LikeMatchMode.Start, "k", ref whereStatement, param);
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