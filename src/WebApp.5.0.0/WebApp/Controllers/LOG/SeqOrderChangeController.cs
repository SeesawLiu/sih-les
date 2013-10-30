using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using com.Sconit.Entity.LOG;
using com.Sconit.Web.Models;
using com.Sconit.Web.Util;
using com.Sconit.Web.Models.SearchModels.ORD;

namespace com.Sconit.Web.Controllers.LOG
{
    public class SeqOrderChangeController : WebAppBaseController
    {
        private static string selectCountStatement = "select count(*) from SeqOrderChange as u";

        private static string selectStatement = "select u from SeqOrderChange as u";

        public ActionResult Index()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_SeqOrderChange_View")]
        public ActionResult List(GridCommand command, OrderMasterSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_SeqOrderChange_View")]
        public ActionResult _AjaxList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            var gridData = GetAjaxPageData<SeqOrderChange>(searchStatementModel, command);
            gridData.Data = gridData.Data.OrderBy(o => o.OrderNo).OrderBy(o => o.OrderDetId).ToList();
            var returnList = new List<SeqOrderChange>();
            foreach (var data in gridData.Data)
            {
                foreach (var ret in returnList)
                {
                    if (data.ExternalOrderNo == ret.ExternalOrderNo && data.OrderNo == ret.OrderNo&& data.Sequence == ret.Sequence)
                    {
                        data.ExternalOrderNo = null;
                        data.ExternalSequence = null;
                        data.OrderNo = null;
                        data.Flow = null;
                        data.Sequence = 0;
                        data.OrderDetId = 0;
                    }
                }
                returnList.Add(data);
            }
            gridData.Data = returnList;
            return PartialView(gridData);
        }

        #region 导出
        public void ExportXLS(OrderMasterSearchModel searchModel)
        {
            string hql = " select s from SeqOrderChange as s  where 1=1";
            if (!string.IsNullOrWhiteSpace(searchModel.OrderNo))
            {
                hql += string.Format(" and s.OrderNo like '{0}%' ", searchModel.OrderNo);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.ExternalOrderNo))
            {
                hql += string.Format(" and s.ExternalOrderNo = '{0}' ", searchModel.ExternalOrderNo);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Item))
            {
                hql += string.Format(" and s.Item like '{0}' ", searchModel.Item);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.CreateUserName))
            {
                hql += string.Format(" and s.CreateUserName like '%{0}%' ", searchModel.CreateUserName);
            }
            if (searchModel.DateFrom != null)
            {
                hql += string.Format(" and s.CreateDate >= '{0}' ", searchModel.DateFrom);
            }
            if (searchModel.DateTo != null)
            {
                hql += string.Format(" and s.CreateDate <= '{0}' ", searchModel.DateTo);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Flow))
            {
                hql += string.Format(" and s.Flow = '{0}' ", searchModel.Flow);
            }
         
            hql += " order by s.CreateDate asc ";
            IList<SeqOrderChange> exportList = this.genericMgr.FindAll<SeqOrderChange>(hql);
            var returnList = new List<SeqOrderChange>();
            string extNo = string.Empty;
            string orderNo = string.Empty;
            int seq = 0;
            foreach (var data in exportList)
            {
                if (data.ExternalOrderNo == extNo && data.OrderNo == orderNo && data.Sequence == seq)
                {
                    data.ExternalOrderNo = null;
                    data.ExternalSequence = null;
                    data.OrderNo = null;
                    data.Flow = null;
                    data.Sequence = 0;
                    data.OrderDetId = 0;
                }
                else
                {
                    extNo = data.ExternalOrderNo;
                    orderNo = data.OrderNo;
                    seq = data.Sequence;
                }
            }
            ExportToXLS<SeqOrderChange>("ExportXLS", "xls", exportList);
        }
        #endregion

        private SearchStatementModel PrepareSearchStatement(GridCommand command, OrderMasterSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            HqlStatementHelper.AddEqStatement("OrderNo", searchModel.OrderNo, "u", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("ExternalOrderNo", searchModel.ExternalOrderNo, "u", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "u", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("CreateUserName", searchModel.CreateUserName,HqlStatementHelper.LikeMatchMode.Start, "u", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "u", ref whereStatement, param);
            if (searchModel.DateFrom != null & searchModel.DateTo != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.DateFrom, searchModel.DateTo, "u", ref whereStatement, param);
            }
            else if (searchModel.DateFrom != null & searchModel.DateTo == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.DateFrom, "u", ref whereStatement, param);
            }
            else if (searchModel.DateFrom == null & searchModel.DateTo != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.DateTo, "u", ref whereStatement, param);
            }
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
