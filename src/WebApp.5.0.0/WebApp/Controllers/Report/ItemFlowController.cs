using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using com.Sconit.Web.Util;
using com.Sconit.Web.Models;
using com.Sconit.Entity.SCM;
using com.Sconit.Service;
using com.Sconit.Web.Models.SearchModels.SCM;
using com.Sconit.Entity.MD;

namespace com.Sconit.Web.Controllers.Report
{
    public class ItemFlowController : WebAppBaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        #region 物料路线
        [GridAction]
        [SconitAuthorize(Permissions = "Url_ItemFlow_View")]
        public ActionResult List(GridCommand command, FlowDetailSearchModel searchModel)
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
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ItemFlow_View")]
        public ActionResult _AjaxList(GridCommand command, FlowDetailSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<FlowDetail>()));
            }
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);

            GridModel<FlowDetail> List = GetAjaxPageData<FlowDetail>(searchStatementModel, command);
            foreach (FlowDetail flowDetail in List.Data)
            {
                Item item = this.genericMgr.FindById<Item>(flowDetail.Item);
                flowDetail.ItemDescription = item.Description;
                FlowMaster flowMaster = base.genericMgr.FindById<FlowMaster>(flowDetail.Flow);
                flowDetail.PartyFrom = flowMaster.PartyFrom;
                flowDetail.PartyTo = flowMaster.PartyTo;
                flowDetail.LocationFrom = flowMaster.LocationFrom;
                flowDetail.LocationTo = flowMaster.LocationTo;
                var flowStrategys = this.genericMgr.FindEntityWithNativeSql<FlowStrategy>("select * from SCM_FlowStrategy where Flow=?", flowDetail.Flow);
                var strategyDesc = flowStrategys != null && flowStrategys.Count > 0 ? flowStrategys.First().Strategy : flowMaster.FlowStrategy;
                flowDetail.FlowStrategy = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.FlowStrategy, (int)strategyDesc);
                flowDetail.Type = this.systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderType, ((int)flowMaster.Type).ToString());
                flowDetail.IsActive = flowMaster.IsActive && flowDetail.IsActive ? true : false;
            }

            return PartialView(List);
        }


        public void ExportXLS(FlowDetailSearchModel searchModel)
        {
            string sql = @"select d.Flow,d.Item,d.RefItemCode,d.IsChangeUC,d.IsActive,d.Uom,d.UC,d.UCDesc,d.MinUC,m.LocFrom,m.LocTo ,m.PartyFrom,m.PartyTo,
                            isnull(fs.Strategy,m.FlowStrategy) as flowStrategy,m.Type,i.Desc1,d.RoundUpOpt  
                            from SCM_FlowDet as d inner join SCM_FlowMstr as m on d.Flow=m.Code 
                                                  inner join MD_Item as i on d.Item=i.Code 
                                                  inner join SCM_FlowStrategy as fs on m.Code=fs.Flow where 1=1 ";
            IList<object> param = new List<object>();
            if (!string.IsNullOrWhiteSpace(searchModel.Item))
            {
                sql += " and  d.Item=?";
                param.Add(searchModel.Item);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.flowCode))
            {
                sql += " and  d.Flow = ?";
                param.Add(searchModel.flowCode);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.PartyTo))
            {
                sql += " and m.PartyTo=?";
                param.Add(searchModel.PartyTo);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.PartyFrom))
            {
                sql += " and m.PartyFrom=?";
                param.Add(searchModel.PartyFrom);
            }
            if (searchModel.Strategy != null)
            {
                sql += " and fs.Strategy=?";
                param.Add(searchModel.Strategy.Value);
            }
            if (searchModel.Type != null)
            {
                sql += " and m.Type=?";
                param.Add(searchModel.Type.Value);
            }
            IList<object[]> searchList = this.genericMgr.FindAllWithNativeSql<object[]>(sql, param.ToArray());
            // d.Flow,d.Item,d.RefItemCode,d.IsChangeUC,d.IsActive,d.Uom,d.UC,d.UCDesc,d.MinUC,
            //d.LocFrom,d.LocTo ,m.PartyFrom,m.PartyTo,m.FlowStrategy,m.Type
            IList<FlowDetail> exportList = (from tak in searchList
                                            select new FlowDetail
                                            {
                                                Flow = (string)tak[0],
                                                Item = (string)tak[1],
                                                ReferenceItemCode = (string)tak[2],
                                                IsChangeUnitCount = (Boolean)tak[3],
                                                IsActive = (Boolean)tak[4],
                                                Uom = (string)tak[5],
                                                UnitCount = (decimal)tak[6],
                                                UnitCountDescription = (string)tak[7],
                                                MinUnitCount = (decimal)tak[8],
                                                LocationFrom = (string)tak[9],
                                                LocationTo = (string)tak[10],
                                                PartyFrom = (string)tak[11],
                                                PartyTo = (string)tak[12],
                                                FlowStrategy = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.FlowStrategy, Convert.ToInt32(((object)tak[13]).ToString())),
                                                Type = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.OrderType, Convert.ToInt32(((object)tak[14]).ToString())),
                                                ItemDescription = (string)tak[15],
                                                RoundUpOptionDescription = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.RoundUpOption, Convert.ToInt32(((object)tak[16]).ToString())),
                                            }).ToList();
            ExportToXLS<FlowDetail>("ExportList", "xls", exportList);
        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, FlowDetailSearchModel searchModel)
        {
            string whereStatement = " where 1=1 ";
            IList<object> param = new List<object>();

            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "d", ref  whereStatement, param);
            HqlStatementHelper.AddEqStatement("Flow", searchModel.flowCode, "d", ref  whereStatement, param);

            if (!string.IsNullOrWhiteSpace(searchModel.PartyTo))
            {
                whereStatement += " and exists( select 1 from FlowMaster as m where m.Code=d.Flow and m.PartyTo=? ) ";
                param.Add(searchModel.PartyTo);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.PartyFrom))
            {
                whereStatement += " and exists( select 1 from FlowMaster as m where m.Code=d.Flow and m.PartyFrom=? ) ";
                param.Add(searchModel.PartyFrom);
            }
            if (searchModel.Type != null)
            {
                whereStatement += " and exists( select 1 from FlowMaster as m where m.Code=d.Flow and m.Type=? ) ";
                param.Add(searchModel.Type);
            }
            if (searchModel.Strategy != null)
            {
                whereStatement += " and exists( select 1 from FlowStrategy as m where m.Flow=d.Flow and m.Strategy=? ) ";
                param.Add(searchModel.Strategy.Value);
            }

            string sortingStatement = string.Empty;
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by d.CreateDate desc";
            }
            else
            {
                sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            }
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = "select count(*) from FlowDetail as d";
            searchStatementModel.SelectStatement = "select d from FlowDetail as d";
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();
            return searchStatementModel;
        }
        #endregion

        #region 物流路线检查
        [GridAction]
        [SconitAuthorize(Permissions = "Url_CheckFlowList_View")]
        public ActionResult CheckFlowList(GridCommand command, FlowDetailSearchModel searchModel)
        {
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_CheckFlowList_View")]
        public ActionResult _AjaxCheckFlowList(GridCommand command, FlowDetailSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<FlowDetail>()));
            }
            string searchSql = @"select  rr.* from (
select t1.*, (case   when t1.MinUC=0 then 4  when t2.Item is null then 1 when t1.PartyFrom<>t2.PartyTo then 2 when t1.LocFrom<>t2.LocTo then 3 
when t1.Item=t3.Item and t1.PartyFrom=t3.PartyFrom and t1.PartyTo=t3.PartyTo and t1.Strategy<>t3.Strategy then 5  else 0 end ) as errorType   from (
select m.Code,m.Desc1 as flowDesc,m.PartyFrom,m.PartyTo,m.LocFrom,m.LocTo,fs.Strategy,d.Item,d.RefItemCode,i.Desc1,d.MinUC from SCM_FlowDet as d 
inner join SCM_FlowMstr as m  on d.Flow=m.Code
inner join SCM_FlowStrategy as fs on m.Code=fs.Flow
inner join MD_Item as i on i.Code=d.Item
where m.Type=2 ) as t1 left join (
select m.Type,m.Code,m.Desc1 as flowDesc,m.PartyFrom,m.PartyTo,m.LocFrom,m.LocTo,fs.Strategy,d.Item,d.RefItemCode,i.Desc1,d.IsChangeUC,d.IsActive,d.UC,d.UCDesc,d.MinUC,d.RoundUpOpt,d.Container,d.ContainerDesc from SCM_FlowDet as d 
inner join SCM_FlowMstr as m  on d.Flow=m.Code
inner join SCM_FlowStrategy as fs on m.Code=fs.Flow
inner join MD_Item as i on i.Code=d.Item
where m.Type=1 
) as t2  on t1.Item=t2.Item  left join
(
select m.Code,m.Desc1 as flowDesc,m.PartyFrom,m.PartyTo,m.LocFrom,m.LocTo,fs.Strategy,d.Item,d.RefItemCode,i.Desc1,d.MinUC from SCM_FlowDet as d 
inner join SCM_FlowMstr as m  on d.Flow=m.Code
inner join SCM_FlowStrategy as fs on m.Code=fs.Flow
inner join MD_Item as i on i.Code=d.Item
where m.Type=2 ) as t3 on t1.Item=t3.Item
where  
(( t2.Item is null  or t1.PartyFrom<>t2.PartyTo or t1.LocFrom<>t2.LocTo  ) and t1.PartyFrom not in('240','230','HB1','280','TB2') ) or t1.MinUC=0 or 
(t1.Item=t3.Item and t1.PartyFrom=t3.PartyFrom and t1.PartyTo=t3.PartyTo and t1.Strategy<>t3.Strategy)
) as rr group by rr.Code,rr.flowDesc,rr.PartyFrom,rr.PartyTo,rr.LocFrom,rr.LocTo,rr.Strategy,rr.Item,rr.RefItemCode,rr.Desc1,rr.MinUC,rr.errorType ";
            if (command.SortDescriptors.Count == 0)
            {
                searchSql += " order by Code asc";
            }
            else
            {
                searchSql += HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            }
            TempData["searchSql"] = searchSql;
            IList<object[]> searchResult = this.genericMgr.FindAllWithNativeSql<object[]>(searchSql);
            var returnResult = new List<FlowDetail>();
            if (searchResult != null && searchResult.Count > 0)
            {
                //m.Type,m.Code,m.Desc1 as flowDesc,m.PartyFrom,m.PartyTo,m.LocFrom,m.LocTo,fs.Strategy,d.Item,d.RefItemCode,i.Desc1,d.MinUC
                returnResult = (from tak in searchResult
                                select new FlowDetail
                                {
                                    Flow = (string)tak[0],
                                    FlowDescription = (string)tak[1],
                                    PartyFrom = (string)tak[2],
                                    PartyTo = (string)tak[3],
                                    LocationFrom = (string)tak[4],
                                    LocationTo = (string)tak[5],
                                    FlowStrategy = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.FlowStrategy, Convert.ToInt32((tak[6]).ToString())),
                                    Item = (string)tak[7],
                                    ReferenceItemCode = (string)tak[8],
                                    ItemDescription = (string)tak[9],
                                    MinUnitCount = (decimal)tak[10],
                                    ErrorType = (int)tak[11],
                                }).ToList();
            }

            GridModel<FlowDetail> returnGrid = new GridModel<FlowDetail>();
            returnGrid.Total = returnResult.Count;
            returnGrid.Data = returnResult.Skip((command.Page - 1) * command.PageSize).Take(command.PageSize);
            return PartialView(returnGrid);
        }

        public void ExportCheckFlowXLS()
        {
            string searchSql = TempData["searchSql"] as string;
            TempData["searchSql"] = searchSql;
            if (string.IsNullOrWhiteSpace(searchSql))
            {
                searchSql = @"select  rr.* from (
select t1.*, (case   when t1.MinUC=0 then 4  when t2.Item is null then 1 when t1.PartyFrom<>t2.PartyTo then 2 when t1.LocFrom<>t2.LocTo then 3 
when t1.Item=t3.Item and t1.PartyFrom=t3.PartyFrom and t1.PartyTo=t3.PartyTo and t1.Strategy<>t3.Strategy then 5  else 0 end ) as errorType   from (
select m.Code,m.Desc1 as flowDesc,m.PartyFrom,m.PartyTo,m.LocFrom,m.LocTo,fs.Strategy,d.Item,d.RefItemCode,i.Desc1,d.MinUC from SCM_FlowDet as d 
inner join SCM_FlowMstr as m  on d.Flow=m.Code
inner join SCM_FlowStrategy as fs on m.Code=fs.Flow
inner join MD_Item as i on i.Code=d.Item
where m.Type=2 ) as t1 left join (
select m.Type,m.Code,m.Desc1 as flowDesc,m.PartyFrom,m.PartyTo,m.LocFrom,m.LocTo,fs.Strategy,d.Item,d.RefItemCode,i.Desc1,d.IsChangeUC,d.IsActive,d.UC,d.UCDesc,d.MinUC,d.RoundUpOpt,d.Container,d.ContainerDesc from SCM_FlowDet as d 
inner join SCM_FlowMstr as m  on d.Flow=m.Code
inner join SCM_FlowStrategy as fs on m.Code=fs.Flow
inner join MD_Item as i on i.Code=d.Item
where m.Type=1 
) as t2  on t1.Item=t2.Item  left join
(
select m.Code,m.Desc1 as flowDesc,m.PartyFrom,m.PartyTo,m.LocFrom,m.LocTo,fs.Strategy,d.Item,d.RefItemCode,i.Desc1,d.MinUC from SCM_FlowDet as d 
inner join SCM_FlowMstr as m  on d.Flow=m.Code
inner join SCM_FlowStrategy as fs on m.Code=fs.Flow
inner join MD_Item as i on i.Code=d.Item
where m.Type=2 ) as t3 on t1.Item=t3.Item
where  
(( t2.Item is null  or t1.PartyFrom<>t2.PartyTo or t1.LocFrom<>t2.LocTo  ) and t1.PartyFrom not in('240','230','HB1','280','TB2') ) or t1.MinUC=0 or 
(t1.Item=t3.Item and t1.PartyFrom=t3.PartyFrom and t1.PartyTo=t3.PartyTo and t1.Strategy<>t3.Strategy)
) as rr group by rr.Code,rr.flowDesc,rr.PartyFrom,rr.PartyTo,rr.LocFrom,rr.LocTo,rr.Strategy,rr.Item,rr.RefItemCode,rr.Desc1,rr.MinUC,rr.errorType
 order by Code asc";
            }
                IList<object[]> searchResult = this.genericMgr.FindAllWithNativeSql<object[]>(searchSql);
                var returnResult = new List<FlowDetail>();
                if (searchResult != null && searchResult.Count > 0)
                {
                    //m.Type,m.Code,m.Desc1 as flowDesc,m.PartyFrom,m.PartyTo,m.LocFrom,m.LocTo,fs.Strategy,d.Item,d.RefItemCode,i.Desc1,d.MinUC
                    returnResult = (from tak in searchResult
                                    select new FlowDetail
                                    {
                                        Flow = (string)tak[0],
                                        FlowDescription = (string)tak[1],
                                        PartyFrom = (string)tak[2],
                                        PartyTo = (string)tak[3],
                                        LocationFrom = (string)tak[4],
                                        LocationTo = (string)tak[5],
                                        FlowStrategy = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.FlowStrategy, Convert.ToInt32((tak[6]).ToString())),
                                        Item = (string)tak[7],
                                        ReferenceItemCode = (string)tak[8],
                                        ItemDescription = (string)tak[9],
                                        MinUnitCount = (decimal)tak[10],
                                        ErrorType = (int)tak[11],
                                    }).ToList();
                }
                ExportToXLS<FlowDetail>("ExportCheckFlowXLS", "xls", returnResult);
        }
        #endregion
    }
}
