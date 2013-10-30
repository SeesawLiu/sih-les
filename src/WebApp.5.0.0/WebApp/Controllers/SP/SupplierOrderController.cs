
namespace com.Sconit.Web.Controllers.SP
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using com.Sconit.Entity.ORD;
    using com.Sconit.Service;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Models.SearchModels.ORD;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;
    using com.Sconit.Entity.SCM;
    using System;
    using AutoMapper;
    using com.Sconit.Entity.MD;
    using NHibernate.Criterion;
    using com.Sconit.Entity.Exception;
    using com.Sconit.Utility;
    using System.Text;
    using com.Sconit.Entity.SYS;
    using System.ComponentModel;
    using com.Sconit.Utility.Report;
    using System.Web.Routing;

    public class SupplierOrderController : WebAppBaseController
    {
        /// <summary>
        /// 
        /// </summary>
        private static string selectCountStatement = "select count(*) from OrderMaster as o";

        /// <summary>
        /// 
        /// </summary>
        private static string selectStatement = "select o from OrderMaster as o";

        //private static string selectFlowDetailStatement = "select f from FlowDetail as f where f.Flow = ?";

        private static string selectOrderDetailStatement = "select d from OrderDetail as d where d.OrderNo=?";

        private static string selectFlowDetailStatement = "select d from FlowDetail as d where d.Flow = ?";

        private static string selectOneFlowDetailStatement = "select d from FlowDetail as d where d.Flow = ? and d.Item = ?";


        private static string selecLocationStatement = "select l from Location as l where l.Region = ? and l.IsActive = ?";

        //public WCFPrintMgrImpl wcfPrintMgr { get; set; }

        public IOrderMgr orderMgr { get; set; }

        public IFlowMgr flowMgr { get; set; }

        public IReportGen reportGen { get; set; }

        //private WCFServices.IPublishing proxy;

        /// <summary>
        /// 
        /// </summary>
        public SupplierOrderController()
        {
        }

        #region view
        [SconitAuthorize(Permissions = "Url_Supplier_Lading_Query")]
        public ActionResult Index()
        {
            return View();
        }
        [SconitAuthorize(Permissions = "Url_Supplier_Lading_Detail_Query")]
        public ActionResult DetaiIndex()
        {
            return View();
        }

        public ActionResult ReturnDetailIndex()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_Single_Return_Detail_Query")]
        public ActionResult ReturnDetailList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            TempData["OrderMasterSearchModel"] = searchModel;
            if (this.CheckSearchModelIsNull(searchModel))
            {
                TempData["_AjaxMessage"] = "";
                IList<OrderDetail> list = base.genericMgr.FindAll<OrderDetail>(PrepareSearchDetailReturnStatement(command, searchModel)); //GetPageData<OrderDetail>(searchStatementModel, command);

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
                return View(new List<OrderDetail>());
            }
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Supplier_Lading_Query")]
        public ActionResult List(GridCommand command, OrderMasterSearchModel searchModel)
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
        [SconitAuthorize(Permissions = "Url_Supplier_Lading_Query")]
        public ActionResult _AjaxList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<OrderMaster>()));
            }
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchStatement_1(command, searchModel);
            //SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command));
        }

        [GridAction]
        public ActionResult _AjaxOrderDetail(string orderNo)
        {
            var orderMaster = this.genericMgr.FindById<OrderMaster>(orderNo);
            orderMaster.IsListPrice = true;
            this.genericMgr.Update(orderMaster);
            IList<OrderDetail> orderDetList = this.genericMgr.FindAll<OrderDetail>(" select d from OrderDetail as d where d.OrderNo=? order by Sequence asc ", orderNo);
            return PartialView(new GridModel<OrderDetail>(orderDetList));
        }
        

        [SconitAuthorize(Permissions = "Url_Single_Return_Query")]
        public ActionResult ReturnIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Single_Return_Query")]
        public ActionResult ReturnList(GridCommand command, OrderMasterSearchModel searchModel)
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
        [SconitAuthorize(Permissions = "Url_Single_Return_Query")]
        public ActionResult _ReturnAjaxList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<OrderMaster>()));
            }
            string whereStatement = " where o.Type in (" + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.Procurement
                        + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract + "," + (int)com.Sconit.CodeMaster.OrderType.ScheduleLine + ")"
                        + " and o.SubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Return;
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel, whereStatement);
            return PartialView(GetAjaxPageData<OrderMaster>(searchStatementModel, command));
        }

        #endregion

        #region edit

        [SconitAuthorize(Permissions = "Url_Supplier_Lading_Query")]
        public ActionResult Edit(string orderNo)
        {
            var orderMaster = this.genericMgr.FindById<OrderMaster>(orderNo);
            orderMaster.IsListPrice = true;
            this.genericMgr.Update(orderMaster);
            if (string.IsNullOrWhiteSpace(orderNo))
            {
                return HttpNotFound();
            }
            else
            {
                return View("Edit", string.Empty, orderNo);
            }
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_Supplier_Lading_Query")]
        public ActionResult _Edit(string orderNo)
        {
            if (string.IsNullOrWhiteSpace(orderNo))
            {
                return HttpNotFound();
            }
            OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);
            ViewBag.Status = orderMaster.Status;
            return PartialView(orderMaster);
        }

        public ActionResult _OrderDetailList(string orderNo)
        {
            OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);
            ViewBag.Status = orderMaster.Status;
            ViewBag.orderNo = orderNo;
            return PartialView();
        }

        [GridAction]
        public ActionResult _SelectBatchEditing(string orderNo)
        {
            IList<OrderDetail> orderDetailList = new List<OrderDetail>();
            orderDetailList = base.genericMgr.FindAll<OrderDetail>(selectOrderDetailStatement, orderNo);
            return View(new GridModel(orderDetailList));
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult SupplierShipOrder(string idStr, string qtyStr)
        {
            try
            {
                if (!string.IsNullOrEmpty(idStr))
                {
                    IList<OrderDetail> orderDetailList = new List<OrderDetail>();
                    string[] idArray = idStr.Split(',');
                    string[] qtyArray = qtyStr.Split(',');
                    for (int i = 0; i < qtyArray.Count(); i++)
                    {
                        if (Convert.ToDecimal(qtyArray[i]) > 0)
                        {
                            OrderDetail od = base.genericMgr.FindById<OrderDetail>(Convert.ToInt32(idArray[i]));

                            OrderDetailInput input = new OrderDetailInput();
                            input.ShipQty = Convert.ToDecimal(qtyArray[i]);
                            od.AddOrderDetailInput(input);
                            orderDetailList.Add(od);
                        }
                    }
                    IpMaster ipMaster= orderMgr.ShipOrder(orderDetailList);

                    if (ipMaster!=null)
                    {
                        SaveSuccessMessage(string.Format("发货成功，生成送货单号{0}。", ipMaster.IpNo));
                        return new RedirectToRouteResult(new RouteValueDictionary  
                                                               { 
                                                                   { "action", "_Edit" }, 
                                                                   { "controller", "SupplierIpMaster" },
                                                                   { "IpNo", ipMaster.IpNo },
                                                                   { "UrlId", "OrderShipEdit" },
                                                                   {"OrderNo",orderDetailList.First().OrderNo}
                                                               });
                    }
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
            return Content("");
        }


        #endregion

        #region ship
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Supplier_Lading_Deliver")]
        public ActionResult ShipIndex()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Supplier_Lading_Deliver")]
        public ActionResult Ship(GridCommand command, OrderMasterSearchModel searchModel)
        {

            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            if (string.IsNullOrWhiteSpace(searchModel.OrderNo) && string.IsNullOrWhiteSpace(searchModel.Flow))
            {
                SaveWarningMessage("请选择查询条件");
            }
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxShipOrderList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (string.IsNullOrWhiteSpace(searchModel.OrderNo) && string.IsNullOrWhiteSpace(searchModel.Flow))
            {
                return PartialView(new GridModel(new List<OrderDetail>()));
            }
            else
            {
                IList<OrderDetail> orderDetList = new List<OrderDetail>();
                ProcedureSearchStatementModel procedureSearchStatementModel = PrepareShipSearchStatement(command, searchModel);
                procedureSearchStatementModel.SelectProcedure = "USP_Search_PrintOrderDet";
                GridModel<object[]> gridModel = GetAjaxPageDataProcedure<object[]>(procedureSearchStatementModel, command);
                if (gridModel.Data != null && gridModel.Data.Count() > 0)
                {
                    #region
                    orderDetList = (from tak in gridModel.Data
                                    select new OrderDetail
                                    {
                                        Id = (int)tak[0],
                                        OrderNo = (string)tak[1],
                                        ExternalOrderNo = (string)tak[2],
                                        ExternalSequence = (string)tak[3],
                                        Item = (string)tak[4],
                                        ReferenceItemCode = (string)tak[5],
                                        ItemDescription = (string)tak[6],
                                        Uom = (string)tak[7],
                                        UnitCount = (decimal)tak[8],
                                        LocationFrom = (string)tak[9],
                                        LocationTo = (string)tak[10],
                                        OrderedQty = (decimal)tak[11],
                                        ShippedQty = (decimal)tak[12],
                                        ReceivedQty = (decimal)tak[13],
                                        ManufactureParty = (string)tak[14],
                                        MastRefOrderNo = (string)tak[15],
                                        MastExtOrderNo = (string)tak[16],
                                        MastPartyFrom = (string)tak[17],
                                        MastPartyTo = (string)tak[18],
                                        MastFlow = (string)tak[19],
                                        MastType = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.OrderType, int.Parse((tak[20]).ToString())),
                                        MastStatus = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.OrderStatus, int.Parse((tak[21]).ToString())),
                                        MastCreateDate = (DateTime)tak[22],
                                        SAPLocation = (string)tak[23],
                                        OrderStrategyDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.FlowStrategy, int.Parse((tak[24]).ToString())),
                                        MastWindowTime = (DateTime)tak[25],
                                        PickedQty = (decimal)tak[30],
                                        BinTo = (string)tak[31],
                                    }).ToList();
                    #endregion
                }
                GridModel<OrderDetail> gridModelOrderDet = new GridModel<OrderDetail>();
                gridModelOrderDet.Total = gridModel.Total;
                gridModelOrderDet.Data = orderDetList;
                return PartialView(gridModelOrderDet);
            }
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Supplier_Lading_Deliver")]
        public ActionResult ShipOrder(string idStr, string qtyStr)
        {
            try
            {
                if (!string.IsNullOrEmpty(idStr))
                {
                    IList<OrderDetail> orderDetailList = new List<OrderDetail>();
                    string[] idArray = idStr.Split(',');
                    string[] qtyArray = qtyStr.Split(',');
                    for (int i = 0; i < qtyArray.Count(); i++)
                    {
                        if (Convert.ToDecimal(qtyArray[i]) > 0)
                        {
                            OrderDetail od = base.genericMgr.FindById<OrderDetail>(Convert.ToInt32(idArray[i]));

                            OrderDetailInput input = new OrderDetailInput();
                            input.ShipQty = Convert.ToDecimal(qtyArray[i]);
                            od.AddOrderDetailInput(input);
                            orderDetailList.Add(od);
                        }
                    }
                    IpMaster ipMaster = orderMgr.ShipOrder(orderDetailList);

                    if (ipMaster != null)
                    {
                        SaveSuccessMessage(string.Format("发货成功，生成送货单号{0}。", ipMaster.IpNo));
                        return new RedirectToRouteResult(new RouteValueDictionary  
                                                               { 
                                                                   { "action", "_Edit" }, 
                                                                   { "controller", "SupplierIpMaster" },
                                                                   { "IpNo", ipMaster.IpNo },
                                                                   { "UrlId", "SupplierShipOrder" },
                                                                   { "OrderNo",orderDetailList.First().OrderNo}
                                                               });
                    }
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
            return Content("");
        }


        private ProcedureSearchStatementModel PrepareShipSearchStatement(GridCommand command, OrderMasterSearchModel searchModel)
        {
            string whereStatement = string.Format(" and d.ShipQty<d.OrderQty and exists (select 1 from OrderMaster  as o where   o.OrderStrategy <> 4 and  o.SubType ={0} and o.OrderNo=d.OrderNo and o.Status in({1},{2}) ) ",
                (int)com.Sconit.CodeMaster.OrderSubType.Normal, (int)com.Sconit.CodeMaster.OrderStatus.Submit, (int)com.Sconit.CodeMaster.OrderStatus.InProcess);
            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.OrderNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Flow, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter
            {
                Parameter = (int)com.Sconit.CodeMaster.OrderType.Distribution + "," + (int)com.Sconit.CodeMaster.OrderType.Procurement
                  + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + ",",
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

        #region 明细菜单 明细报表
        [SconitAuthorize(Permissions = "Url_Supplier_Lading_Detail_Query")]
        public ActionResult DetailList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            TempData["OrderMasterSearchModel"] = searchModel;
            if (this.CheckSearchModelIsNull(searchModel))
            {
                ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
                return View();
            }
            else
            {
                SaveWarningMessage(Resources.ErrorMessage.Errors_NoConditions);
                return View(new List<OrderDetail>());
            }

        }


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Supplier_Lading_Detail_Query")]
        public ActionResult _AjaxOrderDetailList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<OrderDetail>()));
            }
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchDetailStatement(command, searchModel);
            //return PartialView(GetAjaxPageDataProcedure<OrderDetail>(procedureSearchStatementModel, command));
            IList<OrderDetail> orderDetList = new List<OrderDetail>();
            procedureSearchStatementModel.SelectProcedure = "USP_Search_PrintOrderDet";
            GridModel<object[]> gridModel = GetAjaxPageDataProcedure<object[]>(procedureSearchStatementModel, command);
            if (gridModel.Data != null && gridModel.Data.Count() > 0)
            {
                #region
                orderDetList = (from tak in gridModel.Data
                                select new OrderDetail
                                {
                                    Id = (int)tak[0],
                                    OrderNo = (string)tak[1],
                                    ExternalOrderNo =(string)tak[2],
                                    ExternalSequence = (string)tak[3],
                                    Item = (string)tak[4],
                                    ReferenceItemCode = (string)tak[5],
                                    ItemDescription = (string)tak[6],
                                    Uom = (string)tak[7],
                                    UnitCount = (decimal)tak[8],
                                    LocationFrom = (string)tak[9],
                                    LocationTo = (string)tak[10],
                                    OrderedQty = (decimal)tak[11],
                                    ShippedQty = (decimal)tak[12],
                                    ReceivedQty = (decimal)tak[13],
                                    ManufactureParty = (string)tak[14],
                                    MastRefOrderNo = (string)tak[15],
                                    MastExtOrderNo = (string)tak[16],
                                    MastPartyFrom = (string)tak[17],
                                    MastPartyTo = (string)tak[18],
                                    MastFlow = (string)tak[19],
                                    MastType = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.OrderType, int.Parse((tak[20]).ToString())),
                                    MastStatus = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.OrderStatus, int.Parse((tak[21]).ToString())),
                                    MastCreateDate = (DateTime)tak[22],
                                    SAPLocation = (string)tak[23],
                                    MastWindowTime = (DateTime)tak[25],
                                    BillAddressDescription = (string)tak[33],
                                }).ToList();
                #endregion
            }
            procedureSearchStatementModel.PageParameters[2].Parameter = gridModel.Total;
            TempData["OrderMasterPrintSearchModel"] = procedureSearchStatementModel;

            GridModel<OrderDetail> gridModelOrderDet = new GridModel<OrderDetail>();
            gridModelOrderDet.Total = gridModel.Total;
            gridModelOrderDet.Data = orderDetList;

            return PartialView(gridModelOrderDet);
        }

        #region 导出明细
        public void ExportSupplierDetXLS(OrderMasterSearchModel searchModel)
        {
            ProcedureSearchStatementModel procedureSearchStatementModel = TempData["OrderMasterPrintSearchModel"] as ProcedureSearchStatementModel;
            TempData["OrderMasterPrintSearchModel"] = procedureSearchStatementModel;

            GridCommand command = new GridCommand();
            command.Page = 1;
            command.PageSize = (int)procedureSearchStatementModel.PageParameters[2].Parameter !=0?(int)procedureSearchStatementModel.PageParameters[2].Parameter :60000;
            procedureSearchStatementModel.PageParameters[3].Parameter = 1;
            ProcedureSearchStatementModel SearchStatementModel = PrepareSearchDetailStatement(command, searchModel);
            SearchStatementModel.SelectProcedure = "USP_Search_PrintOrderDet";
            GridModel<object[]> gridModel = GetAjaxPageDataProcedure<object[]>(SearchStatementModel, command);

            IList<OrderDetail> orderDetList = new List<OrderDetail>();
            if (gridModel.Data != null && gridModel.Data.Count() > 0)
            {
                #region
                orderDetList = (from tak in gridModel.Data
                                select new OrderDetail
                                {
                                    Id = (int)tak[0],
                                    OrderNo = (string)tak[1],
                                    ExternalOrderNo = (string)tak[2],
                                    ExternalSequence = (string)tak[3],
                                    Item = (string)tak[4],
                                    ReferenceItemCode = (string)tak[5],
                                    ItemDescription = (string)tak[6],
                                    Uom = (string)tak[7],
                                    UnitCount = (decimal)tak[8],
                                    LocationFrom = (string)tak[9],
                                    LocationTo = (string)tak[10],
                                    OrderedQty = (decimal)tak[11],
                                    ShippedQty = (decimal)tak[12],
                                    ReceivedQty = (decimal)tak[13],
                                    ManufactureParty = (string)tak[14],
                                    MastRefOrderNo = (string)tak[15],
                                    MastExtOrderNo = (string)tak[16],
                                    MastPartyFrom = (string)tak[17],
                                    MastPartyTo = (string)tak[18],
                                    MastFlow = (string)tak[19],
                                    MastType = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.OrderType, int.Parse((tak[20]).ToString())),
                                    MastStatus = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.OrderStatus, int.Parse((tak[21]).ToString())),
                                    MastCreateDate = (DateTime)tak[22],
                                    SAPLocation = (string)tak[23],
                                    MastWindowTime = (DateTime)tak[25],
                                }).ToList();
                #endregion
            }
            ExportToXLS<OrderDetail>("ExportSupplierDetXLS", "xls", orderDetList);
            // IList<PrintOrderDetail> printOrderetails = Mapper.Map<IList<OrderDetail>, IList<PrintOrderDetail>>(orderDetList);
            //IList<object> data = new List<object>();
            //data.Add(orderDetList);
            //reportGen.WriteToClient("LogisticOrderDetView.xls", data, "LogisticOrderDetView.xls");
        }
        #endregion


        #endregion

        #region receive

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Receive")]
        public ActionResult ReceiveIndex()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Receive")]
        public ActionResult Receive(GridCommand command, OrderMasterSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = ProcessSearchModel(command, searchModel);
            SearchStatementModel searchStatementModel = PrepareReceiveSearchStatement(command, (OrderMasterSearchModel)searchCacheModel.SearchObject);
            return View(GetPageData<OrderMaster>(searchStatementModel, command));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Receive")]
        public ActionResult _AjaxReceiveOrderList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareReceiveSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<OrderMaster>(searchStatementModel, command));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Receive")]
        public ActionResult _ReceiveOrderDetailList(string checkedOrders)
        {
            string[] checkedOrderArray = checkedOrders.Split(',');
            DetachedCriteria criteria = DetachedCriteria.For<OrderDetail>();
            criteria.Add(Expression.In("OrderNo", checkedOrderArray));
            criteria.Add(Expression.LtProperty("ReceivedQty", "OrderedQty"));
            IList<OrderDetail> orderDetailList = base.genericMgr.FindAll<OrderDetail>(criteria);
            return PartialView(orderDetailList);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Receive")]
        public ActionResult ReceiveEdit(string checkedOrders)
        {
            string[] checkedOrderArray = checkedOrders.Split(',');
            OrderMaster order = base.genericMgr.FindById<OrderMaster>(checkedOrderArray[0]);
            return View(order);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Receive")]
        public ActionResult ReceiveOrder(int[] id, decimal[] currentReceiveQty)
        {
            IList<OrderDetail> orderDetailList = new List<OrderDetail>();
            for (int i = 0; i < currentReceiveQty.Count(); i++)
            {
                if (currentReceiveQty[i] > 0)
                {
                    OrderDetail od = base.genericMgr.FindById<OrderDetail>(id[i]);

                    OrderDetailInput input = new OrderDetailInput();
                    input.ReceiveQty = currentReceiveQty[i];
                    od.AddOrderDetailInput(input);
                    orderDetailList.Add(od);
                }
            }

            if (orderDetailList.Count() == 0)
            {
                return HttpNotFound();
            }
            else
            {
                try
                {
                    orderMgr.ReceiveOrder(orderDetailList);
                    SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_Received);
                    return RedirectToAction("ReceiveIndex");
                }
                catch (BusinessException ex)
                {
                    SaveBusinessExceptionMessage(ex);
                    return View();
                }
            }
        }

        #endregion

        #region batch
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_BatchProcess")]
        public ActionResult BatchProcessIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_BatchProcess")]
        public ActionResult BatchProcessList()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_BatchProcess")]
        public ActionResult BatchSubmit(string orderStr)
        {
            if (string.IsNullOrEmpty(orderStr))
            {
                SaveErrorMessage("请选择一条明细");
            }
            else
            {
                string[] orderArray = orderStr.Split(',');
                try
                {
                    foreach (string orderNo in orderArray)
                    {
                        orderMgr.ReleaseOrder(orderNo);
                        SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_Submited, orderNo);
                    }
                }
                catch (BusinessException ex)
                {
                    SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
                }
            }

            return RedirectToAction("BatchProcessIndex");
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_BatchProcess")]
        public ActionResult BatchDelete(string orderStr)
        {
            if (string.IsNullOrEmpty(orderStr))
            {
                SaveErrorMessage("请选择一条明细");
            }
            else
            {
                string[] orderArray = orderStr.Split(',');
                try
                {
                    foreach (string orderNo in orderArray)
                    {
                        orderMgr.DeleteOrder(orderNo);
                        SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_Deleted, orderNo);
                    }
                }
                catch (BusinessException ex)
                {
                    SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
                }
            }

            return RedirectToAction("BatchProcessIndex");
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_BatchProcess")]
        public ActionResult BatchCancel(string orderStr)
        {
            if (string.IsNullOrEmpty(orderStr))
            {
                SaveErrorMessage("请选择一条明细");
            }
            else
            {
                string[] orderArray = orderStr.Split(',');
                try
                {
                    foreach (string orderNo in orderArray)
                    {
                        orderMgr.CancelOrder(orderNo);
                        SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_Canceled, orderNo);
                    }
                }
                catch (BusinessException ex)
                {
                    SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
                }
            }

            return RedirectToAction("BatchProcessIndex");
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_BatchProcess")]
        public ActionResult BatchClose(string orderStr)
        {
            if (string.IsNullOrEmpty(orderStr))
            {
                SaveErrorMessage("请选择一条明细");
            }
            else
            {
                string[] orderArray = orderStr.Split(',');
                try
                {
                    foreach (string orderNo in orderArray)
                    {
                        orderMgr.ManualCloseOrder(orderNo);
                        SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_Completed, orderNo);
                    }
                }
                catch (BusinessException ex)
                {
                    SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
                }
            }

            return RedirectToAction("BatchProcessIndex");
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_BatchProcess")]
        public ActionResult BatchExport(string orderStr)
        {
            if (string.IsNullOrEmpty(orderStr))
            {
                SaveErrorMessage("请选择一条明细");
            }
            else
            {
                string[] orderArray = orderStr.Split(',');
                try
                {
                    foreach (string orderNo in orderArray)
                    {
                        // orderMgr.ManualCloseOrder(orderNo);
                    }
                }
                catch (BusinessException ex)
                {
                    SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
                }
            }

            return RedirectToAction("BatchProcessIndex");
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_BatchProcess")]
        public ActionResult BatchPrint(string orderStr)
        {
            if (string.IsNullOrEmpty(orderStr))
            {
                SaveErrorMessage("请选择一条明细");
            }
            else
            {
                string[] orderArray = orderStr.Split(',');
                try
                {
                    foreach (string orderNo in orderArray)
                    {
                        //  orderMgr.ManualCloseOrder(orderNo);

                    }
                }
                catch (BusinessException ex)
                {
                    SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
                }
            }

            return RedirectToAction("BatchProcessIndex");
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_BatchProcess")]
        public ActionResult _AjaxBatchProcessList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<OrderMaster>(searchStatementModel, command));
        }
        #endregion

        #region private method
        private IList<OrderDetail> TransformFlowDetailList2OrderDetailList(string flow)
        {
            IList<FlowDetail> flowDetailList = flowMgr.GetFlowDetailList(flow);
            IList<OrderDetail> orderDetailList = new List<OrderDetail>();
            foreach (FlowDetail flowDetail in flowDetailList)
            {
                OrderDetail orderDetail = new OrderDetail();
                Mapper.Map<FlowDetail, OrderDetail>(flowDetail, orderDetail);
                orderDetail.Id = flowDetail.Id;
                orderDetail.ItemDescription = base.genericMgr.FindById<Item>(flowDetail.Item).Description;
                orderDetailList.Add(orderDetail);
            }
            return orderDetailList;
        }

        private ProcedureSearchStatementModel PrepareSearchStatement_1(GridCommand command, OrderMasterSearchModel searchModel)
        {
            string whereStatement = string.Format(" and o.OrderStrategy <> 4 and o.Status<>'{0}' and o.SubType='{1}' ", (int)com.Sconit.CodeMaster.OrderStatus.Create, (int)com.Sconit.CodeMaster.OrderSubType.Normal);
            if (searchModel.IsListPrice)
            {
                whereStatement += string.Format(" and IsListPrice=0 and o.Status not in ({0},{1})", (int)com.Sconit.CodeMaster.OrderStatus.Cancel, (int)com.Sconit.CodeMaster.OrderStatus.Close);
            }
            if (searchModel.IsPrintOrder)
            {
                whereStatement += string.Format(" and IsPrintOrder=0 and o.Status not in ({0},{1})", (int)com.Sconit.CodeMaster.OrderStatus.Cancel, (int)com.Sconit.CodeMaster.OrderStatus.Close);
            }
            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.OrderNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Flow, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter
            {
                Parameter = (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.Procurement,
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
            paraList.Add(new ProcedureParameter { Parameter = true, Type = NHibernate.NHibernateUtil.Boolean });
            paraList.Add(new ProcedureParameter { Parameter = false, Type = NHibernate.NHibernateUtil.Boolean });
            paraList.Add(new ProcedureParameter { Parameter = CurrentUser.Id, Type = NHibernate.NHibernateUtil.Int32 });
            paraList.Add(new ProcedureParameter { Parameter = whereStatement, Type = NHibernate.NHibernateUtil.String });


            if (command.SortDescriptors.Count > 0)
            {
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
                else if (command.SortDescriptors[0].Member == "ExternalOrderNo")
                {
                    command.SortDescriptors[0].Member = "ExtOrderNo";
                }
            }
            pageParaList.Add(new ProcedureParameter { Parameter = command.SortDescriptors.Count > 0 ? command.SortDescriptors[0].Member : null, Type = NHibernate.NHibernateUtil.String });
            pageParaList.Add(new ProcedureParameter { Parameter = command.SortDescriptors.Count > 0 ? (command.SortDescriptors[0].SortDirection == ListSortDirection.Descending ? "desc" : "asc") : "asc", Type = NHibernate.NHibernateUtil.String });
            pageParaList.Add(new ProcedureParameter { Parameter = command.PageSize, Type = NHibernate.NHibernateUtil.Int32 });
            pageParaList.Add(new ProcedureParameter { Parameter = command.Page, Type = NHibernate.NHibernateUtil.Int32 });

            var procedureSearchStatementModel = new ProcedureSearchStatementModel();
            procedureSearchStatementModel.Parameters = paraList;
            procedureSearchStatementModel.PageParameters = pageParaList;
            procedureSearchStatementModel.CountProcedure = "USP_Search_ProcurementOrderCount";
            procedureSearchStatementModel.SelectProcedure = "USP_Search_ProcurementOrder";

            return procedureSearchStatementModel;
        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, OrderMasterSearchModel searchModel)
        {
            string whereStatement = " where o.Type in (" + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.Procurement
                                    + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract + ")"
                                    + "and o.Status<>" + (int)com.Sconit.CodeMaster.OrderStatus.Create
                                    + " and o.SubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal;
            IList<object> param = new List<object>();
            HqlStatementHelper.AddLikeStatement("WMSNo", searchModel.WMSNO, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("ReferenceOrderNo", searchModel.ReferenceOrderNo, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("ExternalOrderNo", searchModel.ExternalOrderNo, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("OrderNo", searchModel.OrderNo, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("PartyFrom", searchModel.PartyFrom, "o", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Dock", searchModel.Dock, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Status", searchModel.Status, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Priority", searchModel.Priority, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Sequence", searchModel.Sequence, "o", ref whereStatement, param);

            //SecurityHelper.AddPartyFromPermissionStatement(ref whereStatement, "o", "PartyFrom", com.Sconit.CodeMaster.OrderType.Procurement, true);
            SecurityHelper.AddPartyFromAndPartyToPermissionStatement(ref whereStatement, "o", "Type", "o", "PartyFrom", "o", "PartyTo", com.Sconit.CodeMaster.OrderType.Procurement, true);

            if (searchModel.DateFrom != null & searchModel.DateTo != null)
            {
                HqlStatementHelper.AddBetweenStatement("StartTime", searchModel.DateFrom, searchModel.DateTo, "o", ref whereStatement, param);
            }
            else if (searchModel.DateFrom != null & searchModel.DateTo == null)
            {
                HqlStatementHelper.AddGeStatement("StartTime", searchModel.DateFrom, "o", ref whereStatement, param);
            }
            else if (searchModel.DateFrom == null & searchModel.DateTo != null)
            {
                HqlStatementHelper.AddLeStatement("StartTime", searchModel.DateTo, "o", ref whereStatement, param);
            }

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by CreateDate desc";
            }
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;

            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        private SearchStatementModel PrepareReceiveSearchStatement(GridCommand command, OrderMasterSearchModel searchModel)
        {

            string whereStatement = " where o.Type in (" + (int)com.Sconit.CodeMaster.OrderType.Procurement + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + "," + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract + "," + (int)com.Sconit.CodeMaster.OrderType.ScheduleLine + ")"
                                    + " and o.IsReceiveScanHu = 0 and o.Status in (" + (int)com.Sconit.CodeMaster.OrderStatus.InProcess + "," + (int)com.Sconit.CodeMaster.OrderStatus.Submit + ")"
                                    + " and o.SubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal
                                    + " and exists (select 1 from OrderDetail as d where d.ReceivedQty < d.OrderedQty and d.OrderNo = o.OrderNo) ";
            //SecurityHelper.AddPartyFromPermissionStatement(ref whereStatement, "o", "PartyFrom", com.Sconit.CodeMaster.OrderType.Procurement, true);
            SecurityHelper.AddPartyFromAndPartyToPermissionStatement(ref whereStatement, "o", "Type", "o", "PartyFrom", "o", "PartyTo", com.Sconit.CodeMaster.OrderType.Procurement, true);

            IList<object> param = new List<object>();

            if (!string.IsNullOrEmpty(searchModel.OrderNo))
            {
                HqlStatementHelper.AddLikeStatement("OrderNo", searchModel.OrderNo, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            }
            else if (!string.IsNullOrEmpty(searchModel.Flow))
            {
                HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "o", ref whereStatement, param);
            }
            else if (!string.IsNullOrEmpty(searchModel.PartyFrom) && !string.IsNullOrEmpty(searchModel.PartyTo))
            {
                HqlStatementHelper.AddEqStatement("PartyFrom", searchModel.PartyFrom, "o", ref whereStatement, param);
                HqlStatementHelper.AddEqStatement("PartyTo", searchModel.PartyTo, "o", ref whereStatement, param);
            }

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by CreateDate desc";
            }
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        private OrderDetail RefreshOrderDetail(string flow, OrderDetail orderDetail)
        {

            OrderDetail newOrderDetail = new OrderDetail();
            IList<FlowDetail> flowDetailList = base.genericMgr.FindAll<FlowDetail>(selectFlowDetailStatement, flow);
            FlowDetail flowDetail = flowDetailList.Where<FlowDetail>(q => q.Item == orderDetail.Item).SingleOrDefault();
            if (flowDetail != null)
            {
                Mapper.Map<FlowDetail, OrderDetail>(flowDetail, newOrderDetail);
                newOrderDetail.Sequence = orderDetail.Sequence == 0 ? newOrderDetail.Sequence : orderDetail.Sequence;
                newOrderDetail.UnitCount = orderDetail.UnitCount == 0 ? orderDetail.UnitCount : orderDetail.UnitCount;
                newOrderDetail.Uom = String.IsNullOrWhiteSpace(orderDetail.Uom) ? orderDetail.Uom : orderDetail.Uom;
                newOrderDetail.ItemDescription = base.genericMgr.FindById<Item>(orderDetail.Item).Description;

            }
            else
            {
                Item item = base.genericMgr.FindById<Item>(orderDetail.Item);
                if (item != null)
                {
                    newOrderDetail.Item = item.Code;
                    newOrderDetail.UnitCount = newOrderDetail.UnitCount == 0 ? item.UnitCount : orderDetail.UnitCount;
                    newOrderDetail.Uom = String.IsNullOrWhiteSpace(newOrderDetail.Uom) ? item.Uom : orderDetail.Uom;
                    newOrderDetail.ItemDescription = item.Description;
                    newOrderDetail.Sequence = orderDetail.Sequence;
                }
            }
            newOrderDetail.OrderedQty = orderDetail.OrderedQty;
            if (!string.IsNullOrEmpty(orderDetail.LocationTo))
            {
                newOrderDetail.LocationTo = orderDetail.LocationTo;
                newOrderDetail.LocationToName = base.genericMgr.FindById<Location>(orderDetail.LocationTo).Name;
            }
            return newOrderDetail;
        }

        /// <summary>
        /// 明细查询
        /// </summary>
        /// <param name="command"></param>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        private ProcedureSearchStatementModel PrepareSearchDetailStatement(GridCommand command, OrderMasterSearchModel searchModel)
        {
            string whereStatement = string.Format(" and exists (select 1 from OrderMaster  as o where o.OrderStrategy <> 4 and o.SubType ={0} and o.OrderNo=d.OrderNo ) ",
                (int)com.Sconit.CodeMaster.OrderSubType.Normal);

            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.OrderNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Flow, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter
            {
                Parameter = (int)com.Sconit.CodeMaster.OrderType.Procurement,
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
            paraList.Add(new ProcedureParameter { Parameter = true, Type = NHibernate.NHibernateUtil.Boolean });
            paraList.Add(new ProcedureParameter { Parameter = false, Type = NHibernate.NHibernateUtil.Boolean });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Item, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.ManufactureParty, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.WmSSeq, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Picker, Type = NHibernate.NHibernateUtil.String });

            paraList.Add(new ProcedureParameter { Parameter = CurrentUser.Id, Type = NHibernate.NHibernateUtil.Int32 });

            paraList.Add(new ProcedureParameter { Parameter = whereStatement, Type = NHibernate.NHibernateUtil.String });


            if (command.SortDescriptors.Count > 0)
            {
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

        /// <summary>
        /// 退货单SearchStatement
        /// </summary>
        /// <param name="command"></param>
        /// <param name="searchModel"></param>
        /// <param name="whereStatement"></param>
        /// <returns></returns>
        private SearchStatementModel PrepareSearchStatement(GridCommand command, OrderMasterSearchModel searchModel, string whereStatement)
        {

            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("OrderNo", searchModel.OrderNo, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "o", ref whereStatement, param);

            //SecurityHelper.AddPartyFromPermissionStatement(ref whereStatement, "o", "PartyFrom", com.Sconit.CodeMaster.OrderType.Procurement, true);
            SecurityHelper.AddPartyFromAndPartyToPermissionStatement(ref whereStatement, "o", "Type", "o", "PartyFrom", "o", "PartyTo", com.Sconit.CodeMaster.OrderType.Procurement, true);

            HqlStatementHelper.AddEqStatement("PartyFrom", searchModel.PartyFrom, "o", ref whereStatement, param);

            HqlStatementHelper.AddEqStatement("PartyTo", searchModel.PartyTo, "o", ref whereStatement, param);

            HqlStatementHelper.AddLikeStatement("WMSNo", searchModel.WMSNO, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("CreateUserName", searchModel.CreateUserName, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Status", searchModel.Status, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Priority", searchModel.Priority, "o", ref whereStatement, param);


            if (searchModel.DateFrom != null & searchModel.DateTo != null)
            {
                HqlStatementHelper.AddBetweenStatement("StartTime", searchModel.DateFrom, searchModel.DateTo, "o", ref whereStatement, param);
            }
            else if (searchModel.DateFrom != null & searchModel.DateTo == null)
            {
                HqlStatementHelper.AddGeStatement("StartTime", searchModel.DateFrom, "o", ref whereStatement, param);
            }
            else if (searchModel.DateFrom == null & searchModel.DateTo != null)
            {
                HqlStatementHelper.AddLeStatement("StartTime", searchModel.DateTo, "o", ref whereStatement, param);
            }
            if (command.SortDescriptors.Count > 0)
            {
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
            }


            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by o.CreateDate desc";
            }
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        /// <summary>
        /// 退货单DetailSearchStatement
        /// </summary>
        /// <param name="command"></param>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        private string PrepareSearchDetailReturnStatement(GridCommand command, OrderMasterSearchModel searchModel)
        {
            StringBuilder Sb = new StringBuilder();
            string whereStatement = " select  d from OrderDetail as d  where exists (select 1 from OrderMaster  as o where o.Type in (" + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.Procurement
                                    + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract + ")"
                                    + " and o.SubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Return + " and o.OrderNo=d.OrderNo ";

            Sb.Append(whereStatement);

            if (searchModel.Flow != null)
            {
                Sb.Append(string.Format(" and o.Flow = '{0}'", searchModel.Flow));
            }

            if (searchModel.Status != null)
            {
                Sb.Append(string.Format(" and o.Status = '{0}'", searchModel.Status));
            }
            if (!string.IsNullOrEmpty(searchModel.OrderNo))
            {
                Sb.Append(string.Format(" and o.OrderNo like '%{0}%'", searchModel.OrderNo));
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

            if (searchModel.DateFrom != null & searchModel.DateTo != null)
            {
                Sb.Append(string.Format(" and o.CreateDate between '{0}' and '{1}'", searchModel.DateFrom, searchModel.DateTo));
                // HqlStatementHelper.AddBetweenStatement("StartTime", searchModel.DateFrom, searchModel.DateTo, "o", ref whereStatement, param);
            }
            else if (searchModel.DateFrom != null & searchModel.DateTo == null)
            {
                Sb.Append(string.Format(" and o.CreateDate >= '{0}'", searchModel.DateFrom));

            }
            else if (searchModel.DateFrom == null & searchModel.DateTo != null)
            {
                Sb.Append(string.Format(" and o.CreateDate <= '{0}'", searchModel.DateTo));

            }
            if (!string.IsNullOrEmpty(searchModel.WMSNO))
            {
                Sb.Append(string.Format(" and  o.WMSNo like '%{0}%'", searchModel.WMSNO));

            }

            Sb.Append(" )");

            if (!string.IsNullOrEmpty(searchModel.Item))
            {
                Sb.Append(string.Format(" and  d.Item like '{0}%'", searchModel.Item));

            }

            if (!string.IsNullOrEmpty(searchModel.WmSSeq))
            {
                Sb.Append(string.Format(" and  d.WMSSeq like '%{0}%'", searchModel.WmSSeq));

            }


            return Sb.ToString();
        }
        #endregion








    }
}
