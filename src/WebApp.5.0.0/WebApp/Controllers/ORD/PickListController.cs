/// <summary>
/// Summary description for PickListController
/// </summary>
namespace com.Sconit.Web.Controllers.ORD
{
    #region reference
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Security;
    using com.Sconit.Entity.ORD;
    using com.Sconit.Entity.SYS;
    using com.Sconit.Service;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Models.SearchModels.ORD;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;
    using NHibernate.Criterion;
    using System;
    using com.Sconit.Entity.Exception;
    using com.Sconit.Utility.Report;
    using AutoMapper;
    using com.Sconit.PrintModel.ORD;
    using com.Sconit.Utility;
    using System.Text;
    using com.Sconit.Entity.INV;
    using System.ComponentModel;
    using com.Sconit.Entity.MD;
    #endregion

    /// <summary>
    /// This controller response to control the PickList.
    /// </summary>
    public class PickListController : WebAppBaseController
    {
        #region Properties

        public IReportGen reportGen { get; set; }
        #endregion

        /// <summary>
        /// hql to get count of the PickList
        /// </summary>
        private static string selectCountStatement = "select count(*) from PickListMaster as p";

        /// <summary>
        /// hql to get all of the PickList
        /// </summary>
        private static string selectStatement = "select p from PickListMaster as p";


        private static string selectOrderCountStatement = "select count(*) from OrderMaster as o";

        private static string selectOrderStatement = "select o from OrderMaster as o";

        public IPickListMgr pickListMgr { get; set; }

        public IOrderMgr orderMgr { get; set; }

        public IIpMgr IpMgr { get; set; }
        #region public actions

        #region 查询
        [SconitAuthorize(Permissions = "Url_PickList_View")]
        public ActionResult Index()
        {
            return View();
        }


        [GridAction]
        [SconitAuthorize(Permissions = "Url_PickList_View")]
        public ActionResult List(GridCommand command, PickListSearchModel searchModel)
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
        [SconitAuthorize(Permissions = "Url_PickList_View")]
        public ActionResult _AjaxList(GridCommand command, PickListSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<PickListMaster>()));
            }
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<PickListMaster>(searchStatementModel, command));
        }

        [GridAction]
        public ActionResult _AjaxDetailView(string pickListNo)
        {
            IList<PickListDetail> pickDetailList = base.genericMgr.FindAll<PickListDetail>("from PickListDetail as d where d.PickListNo=?", pickListNo);
            return PartialView(new GridModel<PickListDetail>(pickDetailList));
        }

        [SconitAuthorize(Permissions = "Url_PickList_Cancel")]
        public ActionResult Cancel(string id)
        {
            try
            {
                pickListMgr.CancelPickList(id);
                SaveSuccessMessage(Resources.ORD.PickListMaster.PickListMaster_Canceled);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            return RedirectToAction("List");

        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, PickListSearchModel searchModel)
        {
            string whereStatement = " where 1=1 ";
            if (!string.IsNullOrWhiteSpace(searchModel.Item))
            {
                whereStatement += " and exists( select 1 from PickListDetail as d where d.PickListNo=pl.PickListNo and d.Item='"+searchModel.Item+"' ) ";
            }
            IList<object> param = new List<object>();
            HqlStatementHelper.AddEqStatement("PickListNo", searchModel.PickListNo, "pl", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Status", searchModel.Status, "pl", ref whereStatement, param);
            //HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "pl", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("PartyFrom", searchModel.PartyFrom, "pl", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("PartyTo", searchModel.PartyTo, "pl", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("DeliveryGroup", searchModel.DeliveryGroup, "pl", ref whereStatement, param);
            if (searchModel.IsPrint)
            {
                HqlStatementHelper.AddEqStatement("IsPrintAsn", searchModel.IsPrint, "pl", ref whereStatement, param);
            }

            if (searchModel.StartTime != null & searchModel.EndTime != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.StartTime, searchModel.EndTime, "pl", ref whereStatement, param);
            }
            else if (searchModel.StartTime != null & searchModel.EndTime == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.StartTime, "pl", ref whereStatement, param);
            }
            else if (searchModel.StartTime == null & searchModel.EndTime != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.EndTime, "pl", ref whereStatement, param);
            }

            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "OrderTypeDescription")
                {
                    command.SortDescriptors[0].Member = "OrderType";
                }
                else if (command.SortDescriptors[0].Member == "OrderStatusDescription")
                {
                    command.SortDescriptors[0].Member = "Status";
                }
            }
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by pl.CreateDate desc";
            }
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = "select count(*) from PickListMaster as pl";
            searchStatementModel.SelectStatement = "select pl from PickListMaster as pl";
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }
        #endregion

        #region 新增
        [SconitAuthorize(Permissions = "Url_PickList_New")]
        public ActionResult NewDetailIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_PickList_New")]
        public ActionResult DetailNew(GridCommand command, OrderMasterSearchModel searchModel)
        {
            //if (string.IsNullOrWhiteSpace(searchModel.Picker))
            //{
            //    SaveWarningMessage("配送组不能为空。");
            //}
            this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        public ActionResult _NewDetailSearchResult(GridCommand command, OrderMasterSearchModel searchModel)
        {
           
            this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = 20;
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxNewDetail(GridCommand command, OrderMasterSearchModel searchModel)
        {
             if (!string.IsNullOrWhiteSpace(searchModel.successMesage))
            {
                SaveSuccessMessage(searchModel.successMesage);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.errorMesage))
            {
                SaveErrorMessage(searchModel.errorMesage);
            }
            #region
            //IList<OrderDetail> orderDetList = new List<OrderDetail>();
            //ProcedureSearchStatementModel procedureSearchStatementModel = PreparePickShipStatement(command, searchModel);
            //procedureSearchStatementModel.SelectProcedure = "USP_Search_PrintOrderDet";
            //GridModel<object[]> gridModel = GetAjaxPageDataProcedure<object[]>(procedureSearchStatementModel, command);
            //if (gridModel.Data != null && gridModel.Data.Count() > 0)
            //{
            //    #region
            //    orderDetList = (from tak in gridModel.Data
            //                    select new OrderDetail
            //                    {
            //                        Id = (int)tak[0],
            //                        OrderNo = (string)tak[1],
            //                        ExternalOrderNo = (string)tak[2],
            //                        ExternalSequence = (string)tak[3],
            //                        Item = (string)tak[4],
            //                        ReferenceItemCode = (string)tak[5],
            //                        ItemDescription = (string)tak[6],
            //                        Uom = (string)tak[7],
            //                        UnitCount = (decimal)tak[8],
            //                        LocationFrom = (string)tak[9],
            //                        LocationTo = (string)tak[10],
            //                        OrderedQty = (decimal)tak[11],
            //                        ShippedQty = (decimal)tak[12],
            //                        ReceivedQty = (decimal)tak[13],
            //                        ManufactureParty = (string)tak[14],
            //                        MastRefOrderNo = (string)tak[15],
            //                        MastExtOrderNo = (string)tak[16],
            //                        MastPartyFrom = (string)tak[17],
            //                        MastPartyTo = (string)tak[18],
            //                        MastFlow = (string)tak[19],
            //                        MastType = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.OrderType, int.Parse((tak[20]).ToString())),
            //                        MastStatus = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.OrderStatus, int.Parse((tak[21]).ToString())),
            //                        MastCreateDate = (DateTime)tak[22],
            //                        SAPLocation = (string)tak[23],
            //                        OrderStrategyDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.FlowStrategy, int.Parse((tak[24]).ToString())),
            //                        MastWindowTime = (DateTime)tak[25],
            //                        PickedQty = (decimal)tak[30],
            //                        BinTo = (string)tak[31],
            //                    }).ToList();
            //    #endregion
            //}
            //procedureSearchStatementModel.PageParameters[2].Parameter = gridModel.Total;
            //TempData["OrderMasterPrintSearchModel"] = procedureSearchStatementModel;
            //if (orderDetList.Count > 0)
            //{
            //    foreach (var det in orderDetList)
            //    {
            //        IList<decimal> inventoryQtys=this.genericMgr.FindAllWithNativeSql<decimal>("select isnull(Qty,0) as invQty from VIEW_LocationDet where Location=? and Item=?",new object[]{det.LocationFrom,det.Item});
            //        IList<decimal> ocuuryQtys = this.genericMgr.FindAllWithNativeSql<decimal>(" select isnull(d.Qty,0) as ocuupyQty from ORD_PickListDet as d where d.LocFrom=? and d.Item=? and exists(select 1 from ORD_PickListMstr as m where m.PLNo=d.PLNo and m.Status=0) ", new object[] { det.LocationFrom, det.Item });
            //        decimal inventoryQty = inventoryQtys != null && inventoryQtys.Count > 0 ? inventoryQtys.First() : 0;
            //        decimal ocuuryQty = ocuuryQtys != null && ocuuryQtys.Count > 0 ? ocuuryQtys.First() : 0;
            //        det.InventoryQty = inventoryQty - ocuuryQty;
                    
            //    }
            //}
            #endregion

            string searchSql = PrepareNewDetailStatement(command, searchModel);
            int total = this.genericMgr.FindAllWithNativeSql<int>(string.Format("select count(*) as rowTotal from( {0} ) as t", searchSql)).First();
            IList<object[]> serchResult = this.genericMgr.FindAllWithNativeSql<object[]>( string.Format(" select t.* from ( {0} ) as t where t.RowId between {1} and {2} ",searchSql,(command.Page-1)*command.PageSize,command.Page*command.PageSize) );
            GridModel<OrderDetail> gridModelOrderDet = new GridModel<OrderDetail>();
            IList<OrderDetail> orderDetList = new List<OrderDetail>();
            if (serchResult != null && serchResult.Count > 0)
            {
                #region
                //d.Id,d.OrderNo,d.Item,d.RefItemCode,d.ItemDesc,d.ManufactureParty,d.OrderQty,
                //d.ShipQty,rr.qty,vl.Qty,d.LocFrom,d.LocTo,d.BinTo,m.WindowTime,d.CreateDate,pr.Picker
                orderDetList = (from tak in serchResult
                                select new OrderDetail
                                {
                                    Id = (int)tak[0],
                                    OrderNo = (string)tak[1],
                                    Item = (string)tak[2],
                                    ReferenceItemCode = (string)tak[3],
                                    ItemDescription = (string)tak[4],
                                    ManufactureParty = (string)tak[5],
                                    OrderedQty = (decimal)tak[6],
                                    ShippedQty = (decimal)tak[7],
                                    OccupyQty = (decimal)tak[8],
                                    //InventoryQty = (decimal)tak[9],
                                    LocationFrom = (string)tak[10],
                                    LocationTo = (string)tak[11],
                                    BinTo = (string)tak[12],
                                    WindowTime = (DateTime?)tak[13],
                                    CreateDate = (DateTime)tak[14],
                                    Picker = (string)tak[15],
                                    PickerDesc = (string)tak[9],
                                    PickedQty = (decimal)tak[16],
                                }).ToList();
                #endregion
            }
            if (orderDetList != null && orderDetList.Count > 0)
            {
                IList<object[]> partSuffixs=this.genericMgr.FindAllWithNativeSql<object[]>(string.Format("select Code,PartSuffix from MD_Location where Code in('{0}')",string.Join("','" ,orderDetList.Select(o=>o.LocationFrom).ToArray())));
                var allLocations=(from tak in partSuffixs
                                      select new Location{
                                        Code=(string)tak[0],
                                        PartSuffix=(string)tak[1]
                                      }).ToList();
                string getInvQtySql = string.Empty;
                foreach (var locFrom in orderDetList.Select(o => o.LocationFrom).Distinct())
                {
                    string partSuffix = allLocations.FirstOrDefault(p => p.Code == locFrom).PartSuffix;
                    partSuffix = string.IsNullOrWhiteSpace(partSuffix) ? "0" : partSuffix;
                    if (string.IsNullOrWhiteSpace(getInvQtySql))
                    {
                        getInvQtySql = " select  Item, Location, SUM(Qty) as Qty from INV_LocationLotDet_" + partSuffix + " with(nolock)  where Location = '" + locFrom + "' and Qty > 0 and OccupyType = 0 and IsATP = 1 and Item in ( ";
                    }else{

                        getInvQtySql += " UNION all select Item, Location, SUM(Qty) as Qty from INV_LocationLotDet_" + partSuffix + " with(nolock) where Location = '" + locFrom + "' and Qty > 0 and OccupyType = 0 and IsATP = 1 and Item in ( ";
                        //getInvQtySql += " UNION all select MIN(Id) as Id, Item, Location, SUM(Qty) as Qty, CSSupplier from INV_LocationLotDet_ + CASE WHEN ISNULL(" + partSuffix + ", '') = '' THEN '0' ELSE " + partSuffix + " END +  where Location = " + locFrom + " and Qty > 0 and OccupyType = 0 and IsATP = 1  and Item in (  ";
                    }
                    foreach (var det in orderDetList.Where(o=>o.LocationFrom==locFrom).Distinct())
                    {
                        getInvQtySql +="'"+ det.Item+"',";
                    }
                    getInvQtySql = getInvQtySql.Substring(0, getInvQtySql.Length - 1) + ")  group by  Item, Location ";
                }
                var invQtys = this.genericMgr.FindAllWithNativeSql<object[]>(getInvQtySql);
                if (invQtys != null && invQtys.Count > 0)
                {
                    var allInvQtys = (from tak in invQtys
                                      select new LocationLotDetail
                                      {
                                          Item = (string)tak[0],
                                          Location = (string)tak[1],
                                          Qty = (decimal)tak[2],
                                      }).ToList();
                    foreach (var det in orderDetList)
                    {
                        var d=allInvQtys.Where(a => a.Location == det.LocationFrom && a.Item == det.Item);
                        det.InventoryQty = d != null && d.Count() > 0 ? d.First().Qty : 0;
                    }
                }
            }
            gridModelOrderDet.Total = total;
            gridModelOrderDet.Data = orderDetList;
            return PartialView(gridModelOrderDet);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_PickList_New_button")]
        public JsonResult PickShipOrder(string idStr, string qtyStr, string deliveryGroup, bool isAutoReceive)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(deliveryGroup))
                {
                    throw new BusinessException("配送组不能为空。");
                }
                IList<OrderDetail> orderDetailList = new List<OrderDetail>();
                
                if (!string.IsNullOrEmpty(idStr))
                {
                    string[] idArray = idStr.Split(',');
                    string[] qtyArray = qtyStr.Split(',');
                    //string pickNo = pickListMgr.CreatePickList4Qty(deliveryGroup,idArray, qtyArray);
                    //if (!string.IsNullOrWhiteSpace(pickNo))
                    //{
                    //    SaveSuccessMessage(string.Format("操作成功，生成拣货单号{0}。", pickNo));
                    //    //return RedirectToAction("Edit/?id=" + pickNo + "& UrlId=New ");
                    //    //return new RedirectToRouteResult(new RouteValueDictionary  
                    //    //                                       { 
                    //    //                                           { "action", "Edit" }, 
                    //    //                                           { "controller", "PickList" },
                    //    //                                           { "id", pickNo },
                    //    //                                           { "UrlId", "New" }
                    //    //                                       });
                    //    return Json(new { url = "/PickList/Edit/", id = pickNo, UrlId = "New" });
                    //}
                    string[] successNos=orderMgr.PickShipOrder(idStr,  qtyStr,  deliveryGroup,  isAutoReceive);
                   
                    
                    if (!string.IsNullOrWhiteSpace(successNos[1]))
                    {
                        SaveSuccessMessage(string.Format("发货成功，生成收货单号{0}。", successNos[1]));
                    }
                    if (!string.IsNullOrWhiteSpace(successNos[0]))
                    {
                        SaveSuccessMessage(string.Format("拣货成功，生成拣货单号{0}。", successNos[0]));
                        return Json(new { url = "/PickList/Edit/", id = successNos[0], UrlId = "New" });
                    }
                }
                else
                {
                    throw new BusinessException("拣货明细不能为空。");
                }

            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }
            //return RedirectToAction("DetailNew");
            return Json(new { url = "/PickList/_NewDetailSearchResult/", id = "", UrlId = "New" });
        }

        private string PrepareNewDetailStatement(GridCommand command, OrderMasterSearchModel searchModel)
        {
            //left join VIEW_LocationDet as vl on vl.Location=d.LocFrom and vl.Item=d.Item  isnull(vl.Qty,0) as InvQty
            string sql = @"select d.Id,d.OrderNo,d.Item,d.RefItemCode,d.ItemDesc,d.ManufactureParty,isnull(d.OrderQty,0) as OrderQty,isnull(d.ShipQty,0) as ShipQty ,isnull(rr.qty,0) as OccupyQty, pk.Decs1,d.LocFrom,d.LocTo,d.BinTo,m.WindowTime,d.CreateDate,pr.Picker,d.PickQty
                 from ORD_OrderDet_2 as d with(nolock) inner join ORD_OrderMstr_2 as m with(nolock) on d.OrderNo=m.OrderNo 
						                 left join MD_PickRule as pr with(nolock) on pr.Location=d.LocFrom and pr.Item=d.Item
                                         left join MD_Picker as pk with(nolock) on pr.Picker=pk.Code
						                 left join (select pd.LocFrom,pd.Item,sum(pd.Qty) as qty from ORD_PickListDet as pd where exists(select 1 from ORD_PickListMstr as pm where pd.PLNo=pm.PLNo and pm.Status=0 )  group by pd.LocFrom,pd.Item) as rr on rr.LocFrom=d.LocFrom and rr.Item=d.Item  
                                         where m.SubType=0 and (d.ShipPickQty)<d.OrderQty and m.Status in( 1,2 ) ";
            sql += string.Format(" and exists ( select 1 from VIEW_UserPermission as p where p.UserId={0} and p.CategoryType={1} and p.PermissionCode=m.PartyFrom ) ", CurrentUser.Id, (int)com.Sconit.CodeMaster.PermissionCategoryType.Region);
            if (!string.IsNullOrWhiteSpace(searchModel.Picker))
            {
                sql +=string.Format( " and pr.Picker='{0}' ",searchModel.Picker);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.OrderNo))
            {
                sql += string.Format(" and d.OrderNo='{0}' ", searchModel.OrderNo);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Flow))
            {
                sql += string.Format(" and m.Flow='{0}' ", searchModel.Flow);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Item))
            {
                sql += string.Format(" and d.Item='{0}' ", searchModel.Item);
            }
            if (searchModel.DateFrom!=null)
            {
                sql += string.Format(" and d.CreateDate>='{0}' ", searchModel.DateFrom.Value);
            }
            if (searchModel.DateTo != null)
            {
                sql += string.Format(" and d.CreateDate<='{0}' ", searchModel.DateTo.Value);
            }
            string orderBysql = " order by CreateDate asc ";
            if (command.SortDescriptors.Count > 0)
            {
                orderBysql = " order by " + command.SortDescriptors[0].Member +(command.SortDescriptors[0].SortDirection == ListSortDirection.Descending ? " desc" : " asc");
            }
            return " select result.*,RowId=ROW_NUMBER()OVER(order by Id asc)  from (" + sql + " ) as  result";
        }

        private ProcedureSearchStatementModel PreparePickShipStatement(GridCommand command, OrderMasterSearchModel searchModel)
        {

            string whereStatement = string.Format(" and (d.ShipQty+d.PickQty)<d.OrderQty and exists (select 1 from OrderMaster  as o where  o.SubType ={0} and o.OrderNo=d.OrderNo ) ",
                (int)com.Sconit.CodeMaster.OrderSubType.Normal);
            if (searchModel.OrderStrategy != null)
            {
                if (searchModel.OrderStrategy.Value == 1 || searchModel.OrderStrategy.Value == 0)
                {
                    whereStatement = string.Format(" and (d.ShipQty+d.PickQty)<d.OrderQty and exists (select 1 from OrderMaster  as o where  o.SubType ={0} and o.OrderStrategy in(0,1) and o.OrderNo=d.OrderNo ) ",
                 (int)com.Sconit.CodeMaster.OrderSubType.Normal);
                }
                else
                {
                    whereStatement = string.Format(" and (d.ShipQty+d.PickQty)<d.OrderQty and exists (select 1 from OrderMaster  as o where  o.SubType ={0} and o.OrderStrategy={1} and o.OrderNo=d.OrderNo ) ",
                (int)com.Sconit.CodeMaster.OrderSubType.Normal, searchModel.OrderStrategy);
                }
                
            }
            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.OrderNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Flow, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter
            {
                //Parameter = (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + "," + (int)com.Sconit.CodeMaster.OrderType.Distribution
                //  + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + ",",          
                Parameter =  (int)com.Sconit.CodeMaster.OrderType.Transfer + ",",
                Type = NHibernate.NHibernateUtil.String
            });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.SubType, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.PartyFrom, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.PartyTo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Status, Type = NHibernate.NHibernateUtil.Int16 });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Priority, Type = NHibernate.NHibernateUtil.Int16 });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.ExternalOrderNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.ReferenceOrderNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.TraceCode, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.CreateUserName, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.DateFrom, Type = NHibernate.NHibernateUtil.DateTime });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.DateTo, Type = NHibernate.NHibernateUtil.DateTime });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.StartTime, Type = NHibernate.NHibernateUtil.DateTime });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.EndTime, Type = NHibernate.NHibernateUtil.DateTime });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.WindowTimeFrom, Type = NHibernate.NHibernateUtil.DateTime });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.WindowTimeTo, Type = NHibernate.NHibernateUtil.DateTime });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Sequence, Type = NHibernate.NHibernateUtil.Int64 });
            paraList.Add(new ProcedureParameter { Parameter = false, Type = NHibernate.NHibernateUtil.Boolean });
            paraList.Add(new ProcedureParameter { Parameter = false, Type = NHibernate.NHibernateUtil.Boolean });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Item, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.ManufactureParty, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.WmSSeq, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Picker, Type = NHibernate.NHibernateUtil.String });

            paraList.Add(new ProcedureParameter { Parameter = CurrentUser.Id, Type = NHibernate.NHibernateUtil.Int32 });

            paraList.Add(new ProcedureParameter { Parameter = whereStatement, Type = NHibernate.NHibernateUtil.String });


            if (command.SortDescriptors.Count > 0)
            {
                #region
                if (command.SortDescriptors[0].Member == "OrderTypeDescription")
                {
                    command.SortDescriptors[0].Member = "Type";
                }
                else if (command.SortDescriptors[0].Member == "OrderPriorityDescription")
                {
                    command.SortDescriptors[0].Member = "Priority";
                }
                else if (command.SortDescriptors[0].Member == "OrderStatusDescription")
                {
                    command.SortDescriptors[0].Member = "Status";
                }
                else if (command.SortDescriptors[0].Member == "ItemDescription")
                {
                    command.SortDescriptors[0].Member = "Item";
                }
                else if (command.SortDescriptors[0].Member == "UnitCountDescription")
                {
                    command.SortDescriptors[0].Member = "UnitCount";
                }
                else if (command.SortDescriptors[0].Member == "ContainerDescription")
                {
                    command.SortDescriptors[0].Member = "Container";
                }
                else if (command.SortDescriptors[0].Member == "LotNo")
                {
                    command.SortDescriptors[0].Member = "Container";
                }
                else if (command.SortDescriptors[0].Member == "LocationTo")
                {
                    command.SortDescriptors[0].Member = "LocTo";
                }
                else if (command.SortDescriptors[0].Member == "OrderedQty")
                {
                    command.SortDescriptors[0].Member = "OrderQty";
                }
                else if (command.SortDescriptors[0].Member == "ShippedQty")
                {
                    command.SortDescriptors[0].Member = "ShipQty";
                }
                else if (command.SortDescriptors[0].Member == "ReceivedQty")
                {
                    command.SortDescriptors[0].Member = "RecQty";
                }
                else if (command.SortDescriptors[0].Member == "ExternalOrderNo")
                {
                    command.SortDescriptors[0].Member = "ExtNo";
                }
                else if (command.SortDescriptors[0].Member == "ExternalSequence")
                {
                    command.SortDescriptors[0].Member = "ExtSeq";
                }
                #endregion
            }
            pageParaList.Add(new ProcedureParameter { Parameter = command.SortDescriptors.Count > 0 ? command.SortDescriptors[0].Member : null, Type = NHibernate.NHibernateUtil.String });
            pageParaList.Add(new ProcedureParameter { Parameter = command.SortDescriptors.Count > 0 ? (command.SortDescriptors[0].SortDirection == ListSortDirection.Descending ? "desc" : "asc") : "asc", Type = NHibernate.NHibernateUtil.String });
            pageParaList.Add(new ProcedureParameter { Parameter = command.PageSize, Type = NHibernate.NHibernateUtil.Int32 });
            pageParaList.Add(new ProcedureParameter { Parameter = command.Page, Type = NHibernate.NHibernateUtil.Int32 });

            var procedureSearchStatementModel = new ProcedureSearchStatementModel();
            procedureSearchStatementModel.Parameters = paraList;
            procedureSearchStatementModel.PageParameters = pageParaList;
            procedureSearchStatementModel.CountProcedure = "USP_Search_ProcurementOrderDetCount";
            procedureSearchStatementModel.SelectProcedure = "USP_Search_ProcurementOrderDet";

            return procedureSearchStatementModel;
        }

        #endregion

        #region 明细删除
        public ActionResult DeleteDetailById(int id, OrderMasterSearchModel searchModel)
        {
            string successMesage = string.Empty;
            string errorMesage = string.Empty;
            try
            {
                orderMgr.DeleteDetailById(id);
                successMesage = "要货需求关闭成功。";

            }
            catch (BusinessException be)
            {
                //SaveBusinessExceptionMessage(be);
                errorMesage = be.GetMessages()[0].GetMessageString();
            }
            catch (Exception e)
            {
                //SaveErrorMessage("要货需求关闭失败，" + e.Message);
                errorMesage = "要货需求关闭失败，" + e.Message;
            }
            return new RedirectToRouteResult(new RouteValueDictionary  
                                                   { 
                                                       { "action", "_NewDetailSearchResult" }, 
                                                       { "controller", "PickList" },
                                                       { "successMesage", successMesage},
                                                       { "errorMesage", errorMesage}
                                                   });
        }
        #endregion

        #region 发货
        [SconitAuthorize(Permissions = "Url_PickList_Ship")]
        public ActionResult ShipIndex()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_PickList_Ship")]
        public ActionResult _PickDetailList(string pickListNo)
        {
            IList<PickListDetail> pickDetailList = new List<PickListDetail>();
            try
            {
                ViewBag.PickListNo = pickListNo;

                IList<PickListMaster> pickListMasterList = base.genericMgr.FindAll<PickListMaster>("from PickListMaster as p where p.PickListNo=?", pickListNo);

                if (pickListMasterList.Count > 0)
                {
                    if (pickListMasterList[0].Status != com.Sconit.CodeMaster.PickListStatus.Submit)
                    {
                        throw new BusinessException("只有状态为释放的拣货单才能发货。");
                    }
                    pickDetailList = base.genericMgr.FindAll<PickListDetail>("from PickListDetail as d where d.PickListNo=?", pickListNo);
                }
                else
                {
                    throw new BusinessException("拣货单号不存在，请确认。");
                }
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }
            return View(pickDetailList);
        }

        [SconitAuthorize(Permissions = "Url_PickList_Ship")]
        public ActionResult PickListship(string idStr, string qtyStr, string pickListNo)
        {
            try
            {
                IList<OrderDetail> orderDetailList = new List<OrderDetail>();
                if (!string.IsNullOrEmpty(idStr))
                {
                    string[] idArray = idStr.Split(',');
                    string[] CurrentPickQtyArray = qtyStr.Split(',');
                    orderMgr.ShipPickList(pickListNo, idArray, CurrentPickQtyArray);
                    SaveSuccessMessage(string.Format("拣货单{0}发货成功。",pickListNo));
                }
                else
                {
                    throw new BusinessException("发货明细不能为空。");
                }

            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }
            return RedirectToAction("ShipIndex");
        }
        #endregion

        #region Edit
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_PickList_View")]
        public ActionResult Edit(string id, string UrlId)
        {

            PickListMaster pickListMaster = base.genericMgr.FindById<PickListMaster>(id);
            ViewBag.status = pickListMaster.Status;
            ViewBag.UrlId = UrlId;
            return View(pickListMaster);

        }
        #endregion

        #region 打印
        public string PrintOrders(string pickListNos)
        {
           
            string[] pickListNoArray = pickListNos.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string hqlMstr = "select p from PickListMaster p where p.PickListNo in('" + string.Join("','", pickListNoArray) + "')";
            string hqlDet = "select od from PickListDetail od where od.PickListNo in('" + string.Join("','", pickListNoArray) + "')";

            IList<PickListMaster> pickListMasterList = this.genericMgr.FindAll<PickListMaster>(hqlMstr);
            IList<PickListDetail> pickListDetailList = this.genericMgr.FindAll<PickListDetail>(hqlDet);


            StringBuilder printUrls = new StringBuilder();
            foreach (var pickListMaster in pickListMasterList)
            {
                pickListMaster.PickListDetails = pickListDetailList.Where(p => p.PickListNo == pickListMaster.PickListNo).OrderBy(p=>p.Sequence).ToList();
                if (pickListMaster.PickListDetails.Count > 0)
                {
                    PrintPickListMaster printPickListMaster = Mapper.Map<PickListMaster, PrintPickListMaster>(pickListMaster);
                    IList<object> data = new List<object>();
                    data.Add(printPickListMaster);
                    data.Add(printPickListMaster.PickListDetails);
                    printUrls.Append(reportGen.WriteToFile("PickList.xls", data));
                    printUrls.Append("||");
                    this.genericMgr.UpdateWithNativeQuery("update ORD_PickListMstr set IsPrintAsn=1 where PLNo=? ", pickListMaster.PickListNo);
                }
            }
            return printUrls.ToString();
        }
        #endregion

        [SconitAuthorize(Permissions = "Url_PickListDetail_View")]
        public ActionResult DetailIndex()
        {
            return View();
        }


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_PickList_Ship")]
        public ActionResult ShipList(GridCommand command, PickListSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_PickList_Ship")]
        public ActionResult _AjaxShipList(GridCommand command, PickListSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareShipSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<PickListMaster>(searchStatementModel, command));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_PickList_View")]
        public ActionResult _PickListDetailHierarchyAjax(string pickListNo)
        {
            string hql = "select d from PickListDetail as d where d.PickListNo = ?";
            IList<PickListDetail> pickListDetailList = base.genericMgr.FindAll<PickListDetail>(hql, pickListNo);
            return View(new GridModel(pickListDetailList));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_PickList_View")]
        public ActionResult _PickListResultHierarchyAjax(int PickListDetailId)
        {
            string hql = "select d from PickListResult as d where d.PickListDetailId = ?";
            IList<PickListResult> pickListResultList = base.genericMgr.FindAll<PickListResult>(hql, PickListDetailId);
            return View(new GridModel(pickListResultList));
        }

        

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_PickList_Ship")]
        public ActionResult ShipEdit(string id)
        {
            PickListMaster pickListMaster = base.genericMgr.FindById<PickListMaster>(id);
            ViewBag.status = pickListMaster.Status;
            return View(pickListMaster);
        }


        [SconitAuthorize(Permissions = "Url_PickList_Start")]
        public ActionResult Start(string id)
        {
            try
            {
                pickListMgr.StartPickList(id);

                SaveSuccessMessage(Resources.ORD.PickListMaster.PickListMaster_Started);
                return RedirectToAction("Edit/" + id);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
                return RedirectToAction("Edit/" + id);
            }
        }

        

        [SconitAuthorize(Permissions = "Url_PickList_Ship")]
        public ActionResult Ship(string id)
        {
            try
            {
                PickListMaster pickListMaster = base.genericMgr.FindById<PickListMaster>(id);
                orderMgr.ShipPickList(pickListMaster);
                SaveSuccessMessage(Resources.ORD.PickListMaster.PickListMaster_Shipped);
                return RedirectToAction("ShipEdit/" + id);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
                return RedirectToAction("ShipEdit/" + id);
            }

        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_PickList_View")]
        public ActionResult PickListDetail(string pickListNo)
        {
            ViewBag.pickListNo = pickListNo;
            return View();
        }


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_PickList_Ship")]
        public ActionResult ShipPickListDetail(string pickListNo)
        {
            ViewBag.pickListNo = pickListNo;
            return PartialView();
        }


        #region  Old New PickList
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_PickList_New")]
        public ActionResult NewIndex()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_PickList_New")]
        public ActionResult New(GridCommand command, OrderMasterSearchModel searchModel)
        {
            TempData["OrderMasterSearchModel"] = searchModel;
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_PickList_New")]
        public ActionResult _AjaxNewOrderList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PreparePickListSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<OrderMaster>(searchStatementModel, command));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_PickList_New")]
        public ActionResult NewEdit(string checkedOrders)
        {
            string[] checkedOrderArray = checkedOrders.Split(',');
            string selectStatement = string.Empty;
            IList<object> selectPartyPara = new List<object>();
            foreach (var para in checkedOrderArray)
            {
                if (selectStatement == string.Empty)
                {
                    selectStatement = "from OrderMaster where OrderNo in (?";
                }
                else
                {
                    selectStatement += ",?";
                }
                selectPartyPara.Add(para);
            }
            selectStatement += ")";

            IList<OrderMaster> Lists = base.genericMgr.FindAll<OrderMaster>(selectStatement, selectPartyPara.ToArray());
            IpMaster order = null;
            try
            {
                order = IpMgr.MergeOrderMaster2IpMaster(Lists);
            }
            catch (BusinessException ex)
            {

                SaveWarningMessage(ex.GetMessages()[0].GetMessageString());
            }
            return View(order);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_PickList_New")]
        public ActionResult _NewOrderDetailList(string checkedOrders)
        {
            string[] checkedOrderArray = checkedOrders.Split(',');
            DetachedCriteria criteria = DetachedCriteria.For<OrderDetail>();
            criteria.Add(Expression.In("OrderNo", checkedOrderArray));
            criteria.Add(Expression.GtProperty("OrderedQty", "PickedQty"));

            IList<OrderDetail> orderDetailList = base.genericMgr.FindAll<OrderDetail>(criteria);
            return PartialView(orderDetailList);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_PickList_New")]
        public ActionResult CreatePickList(string idStr, string qtyStr, OrderMasterSearchModel searchModel)
        {
            try
            {
                if (string.IsNullOrEmpty(idStr))
                {
                    throw new BusinessException("拣货明细不能为空");
                }
                string[] idArr = idStr.Split(',');
                string[] qtyArr = qtyStr.Split(',');

                IList<OrderDetail> orderDetailList = new List<OrderDetail>();
                for (int i = 0; i < idArr.Count(); i++)
                {

                    OrderDetail orderDetail = base.genericMgr.FindById<OrderDetail>(Convert.ToInt32(idArr[i]));
                    OrderDetailInput orderDetailInput = new OrderDetailInput();
                    orderDetailInput.PickQty = Convert.ToDecimal(qtyArr[i]);
                    orderDetail.AddOrderDetailInput(orderDetailInput);
                    //校验还没发
                    orderDetailList.Add(orderDetail);
                }

                PickListMaster pickListMaster = pickListMgr.CreatePickList(orderDetailList);
                SaveSuccessMessage(Resources.ORD.PickListMaster.PickListMaster_Created);
                return RedirectToAction("Edit/" + pickListMaster.PickListNo);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
                TempData["OrderMasterSearchModel"] = searchModel;
                return RedirectToAction("DetailNew");

            }

        }

        #endregion


        public ActionResult DeletePickListResult(int[] checkedResults, string pickListNo)
        {
            try
            {
                if (checkedResults == null || checkedResults.Count() == 0)
                {
                    throw new BusinessException("請先選擇一條明細");
                }
                DetachedCriteria criteria = DetachedCriteria.For<PickListResult>();
                criteria.Add(Expression.In("Id", checkedResults.ToArray()));
                IList<PickListResult> pickListResultList = base.genericMgr.FindAll<PickListResult>(criteria);
                pickListMgr.DeletePickListResult(pickListResultList);
                SaveSuccessMessage(Resources.ORD.PickListMaster.PickListMaster_PickListResultDeleted);
                return RedirectToAction("Edit/" + pickListNo);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
                return RedirectToAction("Edit/" + pickListNo);
            }
        }

        #region 打印导出
        public void SaveToClient(string pickListNo)
        {
            PickListMaster pickListMaster = base.genericMgr.FindById<PickListMaster>(pickListNo);
            IList<PickListDetail> pickListDetails = base.genericMgr.FindAll<PickListDetail>("select pl from PickListDetail as pl where pl.PickListNo=?", pickListNo);
            pickListMaster.PickListDetails = pickListDetails;
            PrintPickListMaster printPickListMaster = Mapper.Map<PickListMaster, PrintPickListMaster>(pickListMaster);
            IList<object> data = new List<object>();
            data.Add(printPickListMaster);
            data.Add(printPickListMaster.PickListDetails);
            reportGen.WriteToClient("PickList.xls", data, "PickList.xls");

        }

        public string Print(string pickListNo)
        {
            PickListMaster pickListMaster = base.genericMgr.FindById<PickListMaster>(pickListNo);
            IList<PickListDetail> pickListDetails = base.genericMgr.FindAll<PickListDetail>("select pl from PickListDetail as pl where pl.PickListNo=?", pickListNo);
            pickListMaster.PickListDetails = pickListDetails;
            PrintPickListMaster printPickListMaster = Mapper.Map<PickListMaster, PrintPickListMaster>(pickListMaster);
            IList<object> data = new List<object>();
            data.Add(printPickListMaster);
            data.Add(printPickListMaster.PickListDetails);
            return reportGen.WriteToFile("PickList.xls", data);
        }
        #endregion

        #endregion

        #region private actions
        


        private SearchStatementModel PrepareShipSearchStatement(GridCommand command, PickListSearchModel searchModel)
        {
            string whereStatement = " where p.Status =" + (int)com.Sconit.CodeMaster.PickListStatus.InProcess;

            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("PickListNo", searchModel.PickListNo, HqlStatementHelper.LikeMatchMode.Start, "p", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "p", ref whereStatement, param);

            if (searchModel.StartTime != null & searchModel.EndTime != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.StartTime, searchModel.EndTime, "p", ref whereStatement, param);
            }
            else if (searchModel.StartTime != null & searchModel.EndTime == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.StartTime, "p", ref whereStatement, param);
            }
            else if (searchModel.StartTime == null & searchModel.EndTime != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.EndTime, "p", ref whereStatement, param);
            }

            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "OrderTypeDescription")
                {
                    command.SortDescriptors[0].Member = "OrderType";
                }
                else if (command.SortDescriptors[0].Member == "OrderStatusDescription")
                {
                    command.SortDescriptors[0].Member = "Status";
                }
            }
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by p.CreateDate desc";
            }
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        private SearchStatementModel PreparePickListSearchStatement(GridCommand command, OrderMasterSearchModel searchModel)
        {

            string whereStatement = " where o.Type in (" + (int)com.Sconit.CodeMaster.OrderType.Distribution + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + ")"
                                    + " and o.IsShipScanHu = 1 and o.Status in (" + (int)com.Sconit.CodeMaster.OrderStatus.InProcess + "," + (int)com.Sconit.CodeMaster.OrderStatus.Submit + ")"
                                    + " and exists (select 1 from OrderDetail as d where d.PickedQty < d.OrderedQty and d.OrderNo = o.OrderNo) ";

            IList<object> param = new List<object>();
            if (!string.IsNullOrEmpty(searchModel.OrderNo))
            {
                HqlStatementHelper.AddLikeStatement("OrderNo", searchModel.OrderNo, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            }
            if (!string.IsNullOrEmpty(searchModel.Flow))
            {
                HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "o", ref whereStatement, param);
            }
            if (!string.IsNullOrEmpty(searchModel.PartyFrom))
            {
                HqlStatementHelper.AddEqStatement("PartyFrom", searchModel.PartyFrom, "o", ref whereStatement, param);

            }
            if (!string.IsNullOrEmpty(searchModel.PartyTo))
            {
                HqlStatementHelper.AddEqStatement("PartyTo", searchModel.PartyTo, "o", ref whereStatement, param);
            }


            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by o.CreateDate desc";
            }

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectOrderCountStatement;
            searchStatementModel.SelectStatement = selectOrderStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        #endregion

        [SconitAuthorize(Permissions = "Url_OrderDetail_Distribution")]
        public ActionResult DetailList(GridCommand command, PickListSearchModel searchModel)
        {

            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            if (this.CheckSearchModelIsNull(searchCacheModel.SearchObject))
            {
                TempData["_AjaxMessage"] = "";


                IList<PickListDetail> list = base.genericMgr.FindAll<PickListDetail>(PrepareSearchDetailStatement(command, searchModel)); //GetPageData<OrderDetail>(searchStatementModel, command);

                int value = Convert.ToInt32(base.systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.MaxRowSizeOnPage));
                if (list.Count > value)
                {
                    SaveWarningMessage(string.Format("数据超过{0}行", value));
                }
                return View(list.Take(value));
            }
            else
            {
                SaveWarningMessage(Resources.ErrorMessage.Errors_NoConditions);
                return View(new List<PickListDetail>());
            }
        }

        private string PrepareSearchDetailStatement(GridCommand command, PickListSearchModel searchModel)
        {
            StringBuilder Sb = new StringBuilder();
            string whereStatement = " select  d from PickListDetail as d  where exists (select 1 from PickListMaster  as o"
                                     + " where o.PickListNo=d.PickListNo ";

            Sb.Append(whereStatement);

            if (searchModel.Flow != null)
            {
                Sb.Append(string.Format(" and o.Flow = '{0}'", searchModel.Flow));
            }

            if (searchModel.Status != null)
            {
                Sb.Append(string.Format(" and o.Status = '{0}'", searchModel.Status));
            }

            if (!string.IsNullOrEmpty(searchModel.PickListNo))
            {
                Sb.Append(string.Format(" and o.PickListNo like '{0}%'", searchModel.PickListNo));
            }
            if (!string.IsNullOrEmpty(searchModel.PartyFrom))
            {
                Sb.Append(string.Format(" and o.PartyFrom = '{0}'", searchModel.PartyFrom));
            }
            if (!string.IsNullOrEmpty(searchModel.PartyTo))
            {
                Sb.Append(string.Format(" and o.PartyTo = '{0}'", searchModel.PartyTo));

            }


            string str = Sb.ToString();
            //SecurityHelper.AddPartyFromPermissionStatement(ref str, "o", "PartyFrom", com.Sconit.CodeMaster.OrderType.Procurement, true);
            SecurityHelper.AddPartyFromAndPartyToPermissionStatement(ref str, "o", "Type", "o", "PartyFrom", "o", "PartyTo", com.Sconit.CodeMaster.OrderType.Procurement, true);
            if (searchModel.StartTime != null & searchModel.EndTime != null)
            {
                Sb.Append(string.Format(" and o.CreateDate between '{0}' and '{1}'", searchModel.StartTime, searchModel.EndTime));

            }
            else if (searchModel.StartTime != null & searchModel.EndTime == null)
            {
                Sb.Append(string.Format(" and o.CreateDate >= '{0}'", searchModel.StartTime));

            }
            else if (searchModel.StartTime == null & searchModel.EndTime != null)
            {
                Sb.Append(string.Format(" and o.CreateDate <= '{0}'", searchModel.EndTime));

            }

            Sb.Append(" )");

            if (!string.IsNullOrEmpty(searchModel.Item))
            {
                Sb.Append(string.Format(" and  d.Item like '{0}%'", searchModel.Item));

            }

            return Sb.ToString();
        }

        private SearchStatementModel PrepareOrderDetailSearchStatement(GridCommand command, OrderMasterSearchModel searchModel)
        {
            IList<object> param = new List<object>();
            StringBuilder Sb = new StringBuilder();
            string whereStatement = " where exists (select 1 from OrderMaster  as o where o.OrderNo=d.OrderNo ";

            Sb.Append(whereStatement);

            if (searchModel.Flow != null)
            {
                Sb.Append(string.Format(" and o.Flow = '{0}'", searchModel.Flow));
            }

            if (searchModel.OrderNo != null)
            {
                Sb.Append(string.Format(" and o.OrderNo like '{0}%'", searchModel.OrderNo));
            }
            if (!string.IsNullOrEmpty(searchModel.PartyFrom))
            {
                Sb.Append(string.Format(" and o.PartyFrom = '{0}'", searchModel.PartyFrom));

            }
            if (!string.IsNullOrEmpty(searchModel.PartyTo))
            {
                Sb.Append(string.Format(" and o.PartyTo = '{0}'", searchModel.PartyTo));

            }
            if (searchModel.DateFrom != null & searchModel.DateTo != null)
            {
                Sb.Append(string.Format(" and o.CreateDate between '{0}' and '{1}'", searchModel.DateFrom, searchModel.DateTo));

            }
            else if (searchModel.DateFrom != null & searchModel.DateTo == null)
            {
                Sb.Append(string.Format(" and o.CreateDate >= '{0}'", searchModel.DateTo));

            }
            else if (searchModel.DateFrom == null & searchModel.DateTo != null)
            {
                Sb.Append(string.Format(" and o.CreateDate <= '{0}'", searchModel.DateTo));

            }
            //满足条件1.移库或销售类型订单;2.订单数-已发数-拣货数〉0;3.排序单和分装生产单不能创建拣货单
            Sb.Append(" and o.Type in (2,3,7) and (OrderQty-ShipQty-PickQty)>0 and (o.OrderStrategy in(0,1,2,3,6,7,8) or (o.OrderStrategy = 5 and o.Routing is null)) ");
            Sb.Append("and o.Status in (" + (int)com.Sconit.CodeMaster.OrderStatus.Submit + "," + (int)com.Sconit.CodeMaster.OrderStatus.InProcess + "))");
            whereStatement = Sb.ToString();
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "d", ref whereStatement, param);
            if (searchModel.LocationFrom != null && searchModel.LocationFromTo == null)
            {
                HqlStatementHelper.AddEqStatement("LocationFrom", searchModel.LocationFrom, "d", ref whereStatement, param);
            }
            else if (searchModel.LocationFrom != null && searchModel.LocationFromTo != null)
            {
                HqlStatementHelper.AddBetweenStatement("LocationFrom", searchModel.LocationFrom, searchModel.LocationFromTo, "d", ref whereStatement, param);
            }

            if (searchModel.LocationTo != null && searchModel.LocationToTo == null)
            {
                HqlStatementHelper.AddEqStatement("LocationTo", searchModel.LocationTo, "d", ref whereStatement, param);
            }
            else if (searchModel.LocationTo != null && searchModel.LocationToTo != null)
            {
                HqlStatementHelper.AddBetweenStatement("LocationTo", searchModel.LocationTo, searchModel.LocationToTo, "d", ref whereStatement, param);
            }

            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "WindowTime")
                {
                    command.SortDescriptors.Remove(command.SortDescriptors[0]);
                    // command.SortDescriptors[0].Member = "Type";
                }
                else if (command.SortDescriptors[0].Member == "CurrentPickListQty")
                {
                    command.SortDescriptors.Remove(command.SortDescriptors[0]);
                }
                else if (command.SortDescriptors[0].Member == "CurrentPickQty")
                {
                    command.SortDescriptors.Remove(command.SortDescriptors[0]);
                }
            }

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by d.CreateDate desc";
            }


            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = "select count(*) from OrderDetail as d";
            searchStatementModel.SelectStatement = "select d from OrderDetail as d";
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;



        }

    }
}

