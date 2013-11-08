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
using com.Sconit.Entity.ORD;
namespace com.Sconit.Web.Controllers.LOG
{
    public class VanOrderTraceController : WebAppBaseController
    {
        //
        // GET: /ItemTrace/

        private static string selectCountStatement = "select count(*) from VanOrderTrace as j";

        private static string selectStatement = "select j from VanOrderTrace as j";


        //
        // GET: /FailCode/
        #region  public
        public ActionResult Index()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_VanOrderTrace_View")]
        [GridAction]
        public ActionResult List(GridCommand command, VanOrderTraceSearchModel searchModel)
        {
            TempData["VanOrderTraceSearchModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [SconitAuthorize(Permissions = "Url_VanOrderTrace_View")]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxList(GridCommand command, VanOrderTraceSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<VanOrderTrace>(searchStatementModel, command));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_VanOrderTrace_View")]
        public ActionResult _AjaxOrderBomTrace(GridCommand command, string uUID)
        {
            IList<VanOrderBomTrace> vanOrderBomTraceList = genericMgr.FindAll<VanOrderBomTrace>("from VanOrderBomTrace as d where d.UUID=? order by CPTime ", uUID);
            if (vanOrderBomTraceList != null && vanOrderBomTraceList.Count > 0)
            {
                foreach (VanOrderBomTrace vanOrderBomTrace in vanOrderBomTraceList)
                {
                    OrderMaster orderMstr = genericMgr.FindById<OrderMaster>(vanOrderBomTrace.VanOrderNo);
                    vanOrderBomTrace.TraceCode = orderMstr.TraceCode;
                }
            }
            return PartialView(new GridModel(vanOrderBomTraceList));

        }


        #region 导出
        public void ExportJITInfo(VanOrderTraceSearchModel searchModel)
        {

            string selectSql = @" select top 65530 vt.Flow,vt.Item,i.RefCode,i.Desc1,om.TraceCode,bt.OrderQty,vt.UC,i.Container,vt.CreateDate,vt.WindowTime ,vt.OpRef 
 from LOG_VanOrderTrace as vt  with(nolock)
 inner join LOG_VanOrderBomTrace as bt  with(nolock) on vt.UUID=bt.UUID
 inner join MD_Item as i  with(nolock) on vt.Item=i.Code 
 inner join ORD_OrderMstr_4 as om  with(nolock) on om.OrderNo=bt.VanOrderNo where 1=1 ";
            if (!string.IsNullOrWhiteSpace(searchModel.OrderNo))
            {
                selectSql +=string.Format( " and vt.OrderNo='{0}' ",searchModel.OrderNo);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Flow))
            {
                selectSql += string.Format(" and vt.Flow='{0}' ", searchModel.Flow);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Item))
            {
                selectSql += string.Format(" and vt.Item='{0}' ", searchModel.Item);
            }
          
            if (!string.IsNullOrWhiteSpace(searchModel.OpReference))
            {
                selectSql += string.Format(" and vt.OpReference='{0}' ", searchModel.OpReference);
            }
            if (searchModel.CreateDateFrom!=null)
            {
                selectSql += string.Format(" and vt.CreateDate>='{0}' ", searchModel.CreateDateFrom.Value);
            }
            if (searchModel.CreateDateTo != null)
            {
                selectSql += string.Format(" and vt.CreateDate<='{0}' ", searchModel.CreateDateTo.Value);
            }
            var searchList = this.genericMgr.FindAllWithNativeSql<object[]>(selectSql);
            //vt.Flow,vt.Item,i.RefCode,i.Desc1,om.TraceCode,bt.OrderQty,vt.UC,i.Container,vt.CreateDate,vt.WindowTime ,vt.OpRef
            var returnList = (from take in searchList
                              select new VanOrderTrace
                              {
                                  Flow = (string)take[0],
                                  Item = (string)take[1],
                                  RefItemCode = (string)take[2],
                                  ItemDesc = (string)take[3],
                                  TraceCode = (string)take[4],
                                  OrderQty = Convert.ToDecimal((take[5]).ToString()),
                                  UnitCount = Convert.ToDecimal((take[6]).ToString()),
                                  Container = (string)take[7],
                                  CreateDate = (DateTime)take[8],
                                  WindowTime = (DateTime)take[9],
                                  OpReference = (string)take[10],
                              }).ToList();
            ExportToXLS<VanOrderTrace>("ExportJITInfo", "xls", returnList);
        }
        #endregion

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
                if (command.SortDescriptors[0].Member == "ReqTimeFromTo")
                {
                    command.SortDescriptors[0].Member = "ReqTimeTo";
                }
                else if (command.SortDescriptors[0].Member == "OrderPriorityDescription")
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
