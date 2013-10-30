using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using com.Sconit.Entity.LOG;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels.LOG;
using com.Sconit.Web.Util;
using System;

namespace com.Sconit.Web.Controllers.LOG
{
    public class SnapshotFlowDet4LeanEngineController : WebAppBaseController
    {
        private static string selectCountStatement = "select count(*) from SnapshotFlowDet4LeanEngine where BatchNo = 96 and ErrorId in (13,14,15) order by Id desc";

        private static string selectStatement = "select u from SnapshotFlowDet4LeanEngine as u";

        public ActionResult Index()
        {
            return View();
        }

        #region 查询
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_SnapshotFlowDet4LeanEngine_View")]
        public ActionResult List(GridCommand command, SnapshotFlowDet4LeanEngineSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_SnapshotFlowDet4LeanEngine_View")]
        public ActionResult _AjaxList(GridCommand command, SnapshotFlowDet4LeanEngineSearchModel searchModel)
        {
            string selectSql = this.PrepareSearchStatement(command, searchModel);
            var total = this.genericMgr.FindAllWithNativeSql<int>(string.Format("select count(*) from ({0}) result", selectSql))[0];
            string whereStatement = string.Format(" select * from ( {0} ) result where result.RowId between {1} and {2}", selectSql, command.PageSize * (command.Page - 1), command.PageSize * command.Page);
            //var returnList = this.genericMgr.FindEntityWithNativeSql<SnapshotFlowDet4LeanEngine>(whereStatement);
            var searchList = this.genericMgr.FindAllWithNativeSql<object[]>(whereStatement);
            var returnList = (from take in searchList
                              select new SnapshotFlowDet4LeanEngine
                              {
                                  Id = (Int64)take[1],
                                  Flow = (string)take[2],
                                  Item = (string)take[3],
                                  LocationFrom = (string)take[4],
                                  LocationTo = (string)take[5],
                                  OrderNo = (string)take[6],
                                  Lvl = Convert.ToInt16((take[7]).ToString()),
                                  ErrorId = Convert.ToInt16((take[8]).ToString()),
                                  Message = (string)take[9],
                                  CreateDate = (DateTime)take[10],
                                  BatchNo = (int)take[11],
                                  ItemDesc = (string)take[12],
                                  ReferenceItemCode = (string)take[13],
                              }).ToList();

            GridModel<SnapshotFlowDet4LeanEngine> gridMode = new GridModel<SnapshotFlowDet4LeanEngine>();
            gridMode.Total = total;
            gridMode.Data = returnList;
            return PartialView(gridMode);
            //string 
            //SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            //return PartialView(GetAjaxPageData<SnapshotFlowDet4LeanEngine>(searchStatementModel, command));
        }

        #endregion

        #region 导出
        public void ExportInfo(SnapshotFlowDet4LeanEngineSearchModel searchModel)
        {

            string selectSql = this.PrepareSearchStatement(null, searchModel);
            var searchList = this.genericMgr.FindAllWithNativeSql<object[]>(selectSql);
            var returnList = (from take in searchList
                              select new SnapshotFlowDet4LeanEngine
                              {
                                  Id = (Int64)take[1],
                                  Flow = (string)take[2],
                                  Item = (string)take[3],
                                  LocationFrom = (string)take[4],
                                  LocationTo = (string)take[5],
                                  OrderNo = (string)take[6],
                                  Lvl = Convert.ToInt16((take[7]).ToString()),
                                  ErrorId = Convert.ToInt16((take[8]).ToString()),
                                  Message = (string)take[9],
                                  CreateDate = (DateTime)take[10],
                                  BatchNo = (int)take[11],
                                  ItemDesc = (string)take[12],
                                  ReferenceItemCode = (string)take[13],
                              }).ToList();
            ExportToXLS<SnapshotFlowDet4LeanEngine>("ExportInfo", "xls", returnList);
        }
        #endregion

        private string PrepareSearchStatement(GridCommand command, SnapshotFlowDet4LeanEngineSearchModel searchModel)
        {
            IList<object> param = new List<object>();
            //            string whereStatement = @"select t1.*,RowId=ROW_NUMBER()OVER(order by t1.id desc) from  LOG_SnapshotFlowDet4LeanEngine as t1 where 1=1 and  t1.ErrorId in (13,14,15) 
            //                                    and t1.BatchNo=(select MAX(t2.BatchNo) from LOG_SnapshotFlowDet4LeanEngine as t2 where 
            //                                    (t1.Flow=t2.Flow or ( t1.Flow is null and t2.Flow is null )) and 
            //                                    (t1.Item=t2.Item  or ( t1.Item is null and t2.Item is null ))and 
            //                                    (t1.LocFrom=t2.LocFrom  or ( t1.LocFrom is null and t2.LocFrom is null )) and 
            //                                    (t1.LocTo=t2.LocTo  or ( t1.LocTo is null and t2.LocTo is null )) and 
            //                                    (t1.OrderNo=t2.OrderNo  or ( t1.OrderNo is null and t2.OrderNo is null )) and 
            //                                    (t1.Lvl=t2.Lvl  or ( t1.Lvl is null and t2.Lvl is null )) and 
            //                                    (t1.ErrorId=t2.ErrorId  or ( t1.ErrorId is null and t2.ErrorId is null )) ) ";

            string whereStatement = @" select top 65532 RowId=ROW_NUMBER()OVER(order by t1.id desc),t1.Id, t1.Flow, t1.Item, t1.LocFrom, t1.LocTo, t1.OrderNo, t1.Lvl, isnull(t1.ErrorId,0) as ErrorId, t1.Msg, t1.CreateDate, t1.BatchNo,mi.Desc1,mi.RefCode from  LOG_SnapshotFlowDet4LeanEngine as t1  left join MD_Item as mi on mi.Code=t1.Item
where 1=1  and t1.BatchNo=(select MAX(t2.BatchNo) from LOG_SnapshotFlowDet4LeanEngine as t2) ";
            //and  t1.ErrorId in (13,14,15)

            if (!string.IsNullOrWhiteSpace(searchModel.Item))
            {
                whereStatement += string.Format(" and t1.Item='{0}' ", searchModel.Item);
            }
            //if (!string.IsNullOrWhiteSpace(searchModel.Flow))
            //{
            //    whereStatement += string.Format(" and t1.Flow='{0}' ", searchModel.Flow);
            //}
            if (!string.IsNullOrWhiteSpace(searchModel.LocationFrom))
            {
                whereStatement += string.Format(" and t1.LocFrom='{0}' ", searchModel.LocationFrom);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.LocationTo))
            {
                whereStatement += string.Format(" and t1.LocTo='{0}' ", searchModel.LocationTo);
            }
            if (searchModel.ErrorId.HasValue)
            {
                if (searchModel.ErrorId.Value == 0)
                {
                    whereStatement += string.Format(" and t1.ErrorId is null");
                }
                else
                {
                    whereStatement += string.Format(" and t1.ErrorId='{0}' ", searchModel.ErrorId.Value);
                }
            }
            //if (!string.IsNullOrWhiteSpace(searchModel.OrderNo))
            //{
            //    whereStatement += string.Format(" and t1.OrderNo='{0}' ", searchModel.OrderNo);
            //}
            //if (searchModel.CreateDateFrom.HasValue)
            //{
            //    whereStatement += string.Format(" and t1.CreateDate>='{0}' ", searchModel.CreateDateFrom.Value);
            //}
            //if (searchModel.CreateDateTo.HasValue)
            //{
            //    whereStatement += string.Format(" and t1.CreateDate<='{0}' ", searchModel.CreateDateTo.Value);
            //}

            return whereStatement;
        }
    }
}
