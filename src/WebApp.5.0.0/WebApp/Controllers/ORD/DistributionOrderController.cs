
namespace com.Sconit.Web.Controllers.ORD
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;
    using AutoMapper;
    using com.Sconit.Entity.BIL;
    using com.Sconit.Entity.Exception;
    using com.Sconit.Entity.MD;
    using com.Sconit.Entity.ORD;
    using com.Sconit.Entity.SCM;
    using com.Sconit.Entity.SYS;
    using com.Sconit.PrintModel.ORD;
    using com.Sconit.Service;
    using com.Sconit.Utility;
    using com.Sconit.Utility.Report;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Models.SearchModels.ORD;
    using com.Sconit.Web.Util;
    using NHibernate.Criterion;
    using Telerik.Web.Mvc;
    using com.Sconit.Entity;
    using System.ComponentModel;
    using System.Web;

    public class DistributionOrderController : WebAppBaseController
    {
        private static string selectCountStatement = "select count(*) from OrderMaster as o";
        private static string selectStatement = "select o from OrderMaster as o";
        private static string selectFlowDetailStatement = "select f from FlowDetail as f where f.Flow = ?";
        private static string selectOrderDetailStatement = "select d from OrderDetail as d where d.OrderNo=?";
        private static string selectOneFlowDetailStatement = "select d from FlowDetail as d where d.Flow = ? and d.Item = ?";

        private static string selectReceiptCountStatement = "select count(*) from OrderDetail as d";
        private static string selectReceiptStatement = "select d from OrderDetail as d";

        private static string selectIpDetailCountStatement = "select count(*) from IpDetail as i";
        private static string selectIpDetailStatement = "select i from IpDetail as i";

        public IOrderMgr orderMgr { get; set; }
        public IFlowMgr flowMgr { get; set; }
        public IReportGen reportGen { get; set; }
        public IPickListMgr pickListMgr { get; set; }

        public DistributionOrderController()
        {
        }

        #region edit

        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_QuickNew")]
        public ActionResult QuickNew()
        {
            return View();
        }



        [SconitAuthorize(Permissions = "Url_OrderDetail_Distribution")]
        public ActionResult DetailIndex()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OrderDetail_Distribution")]
        public ActionResult ReturnDetailIndex()
        {
            return View();
        }


        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_ReturnNew")]
        public ActionResult ReturnNew()
        {
            return View();
        }


        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_ReturnQuickNew")]
        public ActionResult ReturnQuickNew()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_View")]
        public ActionResult Index()
        {
            return View();
        }


        //[SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_ReturnShip")]
        //public ActionResult ReturnShipEdit() { 

        //}

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_View")]
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
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_View")]
        public ActionResult _AjaxList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<OrderMaster>()));
            }
            //string whereStatement = " where o.Type in (" + (int)com.Sconit.CodeMaster.OrderType.Distribution + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + ")"
            //          + " and o.SubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal;
            //SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel, whereStatement);
            //return PartialView(GetAjaxPageData<OrderMaster>(searchStatementModel, command));
            string whereStatement = string.Empty;
            if (searchModel.Item != null && searchModel.Item != string.Empty)
            {
                whereStatement += "  and exists(select 1 from OrderDetail as d where d.OrderNo=o.OrderNo and d.Item ='" + searchModel.Item + "')";
            }
            if (searchModel.OrderStrategy != null)
            {
                if (searchModel.OrderStrategy.Value == 1 || searchModel.OrderStrategy.Value == 0)
                {
                    whereStatement += "and o.OrderStrategy in(0,1)";
                }
                else
                {
                    whereStatement += "and o.OrderStrategy=" + searchModel.OrderStrategy;
                }
            }
            //whereStatement += " and o.OrderStrategy != " + (int)com.Sconit.CodeMaster.FlowStrategy.SEQ;
            searchModel.SubType = (int)com.Sconit.CodeMaster.OrderSubType.Normal;
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchStatement_1(command, searchModel, whereStatement, false);
            return PartialView(GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command));
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_New")]
        public ActionResult New()
        {
            ViewBag.flow = null;
            ViewBag.orderNo = null;
            return View();
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_New,Url_OrderMstr_Distribution_QuickNew,Url_OrderMstr_Distribution_ReturnNew")]
        public JsonResult CreateOrder(OrderMaster orderMaster, [Bind(Prefix =
             "inserted")]IEnumerable<OrderDetail> insertedOrderDetails, [Bind(Prefix =
             "updated")]IEnumerable<OrderDetail> updatedOrderDetails)
        {
            try
            {
                if (string.IsNullOrEmpty(orderMaster.Flow))
                {
                    throw new BusinessException(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.ORD.OrderMaster.OrderMaster_Flow);
                }
                if (!orderMaster.IsQuick && orderMaster.WindowTime == DateTime.MinValue)
                {
                    throw new BusinessException(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.ORD.OrderMaster.OrderMaster_WindowTime);
                }
                if (!orderMaster.IsQuick && orderMaster.StartTime == DateTime.MinValue)
                {
                    throw new BusinessException(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.ORD.OrderMaster.OrderMaster_StartTime);
                }
                #region orderDetailList
                IList<OrderDetail> orderDetailList = new List<OrderDetail>();
                if (insertedOrderDetails != null && insertedOrderDetails.Count() > 0)
                {
                    foreach (OrderDetail orderDetail in insertedOrderDetails)
                    {
                        OrderDetail newOrderDetail = RefreshOrderDetail(orderMaster.Flow, orderDetail, orderMaster.SubType);
                        orderDetailList.Add(newOrderDetail);
                    }
                }
                if (updatedOrderDetails != null && updatedOrderDetails.Count() > 0)
                {
                    foreach (OrderDetail orderDetail in updatedOrderDetails)
                    {

                        if (!string.IsNullOrEmpty(orderDetail.LocationFrom))
                        {
                            orderDetail.LocationFromName = base.genericMgr.FindById<Location>(orderDetail.LocationFrom).Name;
                        }

                        if (!string.IsNullOrWhiteSpace(orderDetail.ManufactureParty))
                        {
                            string sql = "select m from FlowMaster as m where m.Type=" + (int)com.Sconit.CodeMaster.OrderType.Procurement + " and m.PartyFrom='" + orderDetail.ManufactureParty + "' and  m.Code in (select distinct d.Flow from FlowDetail as d where d.Item='" + orderDetail.Item + "' )";

                            IList<FlowMaster> flowMasterCount = base.genericMgr.FindAll<FlowMaster>(sql);
                            if (flowMasterCount.Count == 0)
                            {
                                orderDetail.ManufactureParty = null;
                            }
                        }
                        else
                        {
                            orderDetail.ManufactureParty = null;
                        }

                        orderDetailList.Add(orderDetail);
                    }
                }
                #endregion

                if (orderDetailList.Count == 0)
                {
                    throw new BusinessException(Resources.ORD.OrderMaster.Errors_OrderDetailIsEmpty);
                }


                FlowMaster flow = base.genericMgr.FindById<FlowMaster>(orderMaster.Flow);
                if (orderMaster.SubType == com.Sconit.CodeMaster.OrderSubType.Return)
                {
                    flow = flowMgr.GetReverseFlow(flow, null);
                    //  base.genericMgr.CleanSession();
                }
                DateTime effectiveDate = orderMaster.EffectiveDate.HasValue ? orderMaster.EffectiveDate.Value : DateTime.Now;
                OrderMaster newOrder = orderMgr.TransferFlow2Order(flow, null, effectiveDate, false);
                newOrder.IsQuick = orderMaster.IsQuick;

                newOrder.WindowTime = orderMaster.IsQuick ? DateTime.Now : orderMaster.WindowTime;
                newOrder.StartTime = orderMaster.IsQuick ? DateTime.Now : orderMaster.StartTime;
                newOrder.ReferenceOrderNo = orderMaster.ReferenceOrderNo;
                newOrder.ExternalOrderNo = orderMaster.ExternalOrderNo;
                newOrder.Priority = orderMaster.Priority;


                if (orderMaster.SubType == com.Sconit.CodeMaster.OrderSubType.Return)
                {
                    newOrder.SubType = orderMaster.SubType;
                }

                newOrder.OrderDetails = orderDetailList;

                orderMgr.CreateOrder(newOrder);
                SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_Added, newOrder.OrderNo);
                return Json(new { OrderNo = newOrder.OrderNo });
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }
            return Json(null);
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Edit")]
        public ActionResult Edit(string orderNo, int? SubType)
        {
            if (string.IsNullOrWhiteSpace(orderNo))
            {
                return HttpNotFound();
            }
            else
            {
                ViewBag.SubType = SubType;
                return View("Edit", string.Empty, orderNo);
            }
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Edit")]
        public ActionResult _Edit(string orderNo)
        {
            OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);

            ViewBag.isEditable = orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.Create;
            ViewBag.editorTemplate = orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.Create ? "" : "ReadonlyTextBox";

            return PartialView(orderMaster);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Edit")]
        public ActionResult _Edit(OrderMaster orderMaster)
        {
            try
            {

                #region 默认值
                OrderMaster oldOrderMaster = base.genericMgr.FindById<OrderMaster>(orderMaster.OrderNo);
                #region BillAddress赋值

                if (oldOrderMaster.BillAddress != orderMaster.BillAddress)
                {
                    oldOrderMaster.BillAddressDescription = base.genericMgr.FindById<Address>(orderMaster.BillAddress).AddressContent;
                }
                #endregion

                #region ShipFrom赋值
                if (oldOrderMaster.ShipFrom != orderMaster.ShipFrom)
                {
                    Address shipFrom = base.genericMgr.FindById<Address>(orderMaster.ShipFrom);
                    oldOrderMaster.ShipFromAddress = shipFrom.AddressContent;
                    oldOrderMaster.ShipFromCell = shipFrom.MobilePhone;
                    oldOrderMaster.ShipFromTel = shipFrom.TelPhone;
                    oldOrderMaster.ShipFromFax = shipFrom.Fax;
                    oldOrderMaster.ShipFromContact = shipFrom.ContactPersonName;
                }
                #endregion

                #region ShipTo赋值
                if (oldOrderMaster.ShipTo != orderMaster.ShipTo)
                {
                    Address shipTo = base.genericMgr.FindById<Address>(orderMaster.ShipTo);
                    oldOrderMaster.ShipToAddress = shipTo.AddressContent;
                    oldOrderMaster.ShipToCell = shipTo.MobilePhone;
                    oldOrderMaster.ShipToTel = shipTo.TelPhone;
                    oldOrderMaster.ShipToFax = shipTo.Fax;
                    oldOrderMaster.ShipToContact = shipTo.ContactPersonName;
                }
                #endregion

                #region LocationFrom赋值
                if (!string.IsNullOrEmpty(orderMaster.LocationFrom) && (oldOrderMaster.LocationFrom != orderMaster.LocationFrom))
                {
                    oldOrderMaster.LocationFrom = orderMaster.LocationFrom;
                    oldOrderMaster.LocationFromName = base.genericMgr.FindById<Location>(orderMaster.LocationFrom).Name;
                }
                #endregion

                #region LocationTo赋值
                if (!string.IsNullOrEmpty(orderMaster.LocationTo) && (oldOrderMaster.LocationTo != orderMaster.LocationTo))
                {
                    oldOrderMaster.LocationTo = orderMaster.LocationTo;
                    oldOrderMaster.LocationToName = base.genericMgr.FindById<Location>(orderMaster.LocationTo).Name;
                }
                #endregion

                #region PriceList赋值
                if (oldOrderMaster.PriceList != orderMaster.PriceList)
                {
                    oldOrderMaster.Currency = base.genericMgr.FindById<PriceListMaster>(orderMaster.PriceList).Currency;
                }
                #endregion

                #region 新增值
                oldOrderMaster.Priority = orderMaster.Priority;
                oldOrderMaster.IsOpenOrder = orderMaster.IsOpenOrder;
                oldOrderMaster.Sequence = orderMaster.Sequence;
                oldOrderMaster.WindowTime = orderMaster.WindowTime;
                oldOrderMaster.StartTime = orderMaster.StartTime;
                oldOrderMaster.Dock = orderMaster.Dock;
                oldOrderMaster.BillTerm = orderMaster.BillTerm;
                oldOrderMaster.Currency = orderMaster.Currency;
                oldOrderMaster.HuTemplate = orderMaster.HuTemplate;
                oldOrderMaster.OrderTemplate = orderMaster.OrderTemplate;
                oldOrderMaster.AsnTemplate = orderMaster.AsnTemplate;
                oldOrderMaster.ReceiptTemplate = orderMaster.ReceiptTemplate;
                oldOrderMaster.IsReceiveScanHu = orderMaster.IsReceiveScanHu;
                oldOrderMaster.IsShipScanHu = orderMaster.IsShipScanHu;
                oldOrderMaster.IsReceiveFulfillUC = orderMaster.IsReceiveFulfillUC;
                oldOrderMaster.IsPrintOrder = orderMaster.IsPrintOrder;
                oldOrderMaster.IsPrintAsn = orderMaster.IsPrintAsn;
                oldOrderMaster.IsPrintReceipt = orderMaster.IsPrintReceipt;
                oldOrderMaster.IsOrderFulfillUC = orderMaster.IsOrderFulfillUC;
                oldOrderMaster.IsShipFulfillUC = orderMaster.IsShipFulfillUC;
                oldOrderMaster.IsReceiveFulfillUC = orderMaster.IsReceiveFulfillUC;
                oldOrderMaster.IsManualCreateDetail = orderMaster.IsManualCreateDetail;
                oldOrderMaster.IsListPrice = orderMaster.IsListPrice;
                oldOrderMaster.IsCreatePickList = orderMaster.IsCreatePickList;
                oldOrderMaster.IsShipByOrder = orderMaster.IsShipByOrder;
                oldOrderMaster.IsReceiveExceed = orderMaster.IsReceiveExceed;
                oldOrderMaster.IsShipExceed = orderMaster.IsShipExceed;
                oldOrderMaster.IsAsnUniqueReceive = orderMaster.IsAsnUniqueReceive;
                oldOrderMaster.IsAutoRelease = orderMaster.IsAutoRelease;
                oldOrderMaster.IsAutoStart = orderMaster.IsAutoStart;
                oldOrderMaster.IsAutoShip = orderMaster.IsAutoShip;
                oldOrderMaster.IsAutoReceive = orderMaster.IsAutoReceive;
                oldOrderMaster.IsAutoBill = orderMaster.IsAutoBill;
                oldOrderMaster.IsInspect = orderMaster.IsInspect;

                #endregion
                #endregion

                orderMgr.UpdateOrder(oldOrderMaster);
                SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_Saved, orderMaster.OrderNo);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            return RedirectToAction("Edit", new { orderNo = orderMaster.OrderNo });
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Delete")]
        public ActionResult Delete(string id)
        {
            try
            {
                orderMgr.DeleteOrder(id);
                SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_Deleted, id);
                return RedirectToAction("List");
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
                return RedirectToAction("Edit", new { orderNo = id });
            }
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Submit")]
        public ActionResult Submit(string id)
        {
            try
            {
                orderMgr.ReleaseOrder(id);
                SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_Submited, id);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            return RedirectToAction("Edit", new { orderNo = id });
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Start")]
        public ActionResult Start(string id)
        {
            try
            {
                orderMgr.StartOrder(id);
                SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_Started, id);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            return RedirectToAction("Edit", new { orderNo = id });
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Close")]
        public ActionResult Close(string id)
        {
            try
            {
                orderMgr.ManualCloseOrder(id);
                SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_Closed, id);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            return RedirectToAction("Edit", new { orderNo = id });
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Cancel")]
        public ActionResult Cancel(string id)
        {
            try
            {
                orderMgr.CancelOrder(id);
                SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_Canceled, id);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            return RedirectToAction("Edit", new { orderNo = id });
        }

        public ActionResult _OrderDetailList(string flow, string orderNo, int? orderSubType)
        {
            ViewBag.isManualCreateDetail = false;
            ViewBag.flow = flow;
            ViewBag.orderNo = orderNo;
            ViewBag.newOrEdit = "New";
            ViewBag.status = null;
            ViewBag.orderSubType = (orderSubType != null && orderSubType.Value == (int)com.Sconit.CodeMaster.OrderSubType.Return) ? com.Sconit.CodeMaster.OrderSubType.Return : com.Sconit.CodeMaster.OrderSubType.Normal;

            FlowMaster flowMaster = null;

            if (!string.IsNullOrEmpty(flow))
            {
                flowMaster = base.genericMgr.FindById<FlowMaster>(flow);

                ViewBag.PartyFrom = ViewBag.orderSubType == com.Sconit.CodeMaster.OrderSubType.Normal ? flowMaster.PartyFrom : flowMaster.PartyTo;
                ViewBag.PartyTo = ViewBag.orderSubType == com.Sconit.CodeMaster.OrderSubType.Normal ? flowMaster.PartyTo : flowMaster.PartyFrom;
                ViewBag.isManualCreateDetail = flowMaster.IsManualCreateDetail;
                ViewBag.status = com.Sconit.CodeMaster.OrderStatus.Create;

            }
            if (!string.IsNullOrEmpty(orderNo))
            {
                OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);
                ViewBag.status = orderMaster.Status;
                ViewBag.PartyFrom = ViewBag.orderSubType == com.Sconit.CodeMaster.OrderSubType.Normal ? orderMaster.PartyFrom : orderMaster.PartyTo;
                ViewBag.PartyTo = ViewBag.orderSubType == com.Sconit.CodeMaster.OrderSubType.Normal ? orderMaster.PartyTo : orderMaster.PartyFrom;

                ViewBag.newOrEdit = "Edit";
                ViewBag.isManualCreateDetail = ViewBag.status == com.Sconit.CodeMaster.OrderStatus.Create;
            }
            if (ViewBag.Status == com.Sconit.CodeMaster.OrderStatus.Create)
            {
                #region comboBox
                IList<Uom> uoms = base.genericMgr.FindAll<Uom>();
                ViewData.Add("uoms", uoms);

                #endregion
            }

            return PartialView();
        }

        [GridAction]
        public ActionResult _SelectBatchEditing(string orderNo, string flow, com.Sconit.CodeMaster.OrderSubType orderSubType)
        {
            IList<OrderDetail> orderDetailList = new List<OrderDetail>();

            if (!string.IsNullOrEmpty(flow) || !string.IsNullOrEmpty(orderNo))
            {
                if (!string.IsNullOrEmpty(orderNo))
                {
                    orderDetailList = base.genericMgr.FindAll<OrderDetail>(selectOrderDetailStatement, orderNo);
                }
                else
                {
                    FlowMaster flowMaster = base.genericMgr.FindById<FlowMaster>(flow);
                    if (flowMaster.IsListDet)
                    {
                        orderDetailList = TransformFlowDetailList2OrderDetailList(flow, orderSubType);
                    }
                }
            }
            //ViewBag.Total = orderDetailList.Count();
            //return View(new GridModel(orderDetailList));
            GridModel<OrderDetail> orderDets = new GridModel<OrderDetail>();
            //ViewBag.Total = orderDetailList.Count();
            orderDets.Total = orderDetailList.Count();
            orderDets.Data = orderDetailList;
            //orderDetailList.Skip((command.Page - 1) * command.PageSize).Take(40).ToList();
            return PartialView(orderDets);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Edit")]
        public JsonResult _SaveBatchEditing([Bind(Prefix =
            "inserted")]IEnumerable<OrderDetail> insertedOrderDetails,
            [Bind(Prefix = "updated")]IEnumerable<OrderDetail> updatedOrderDetails,
            [Bind(Prefix = "deleted")]IEnumerable<OrderDetail> deletedOrderDetails,
            string flow, string orderNo, com.Sconit.CodeMaster.OrderSubType orderSubType)
        {
            try
            {
                IList<OrderDetail> newOrderDetailList = new List<OrderDetail>();
                IList<OrderDetail> updateOrderDetailList = new List<OrderDetail>();
                if (insertedOrderDetails != null)
                {
                    foreach (var orderDetail in insertedOrderDetails)
                    {
                        OrderDetail newOrderDetail = RefreshOrderDetail(flow, orderDetail, orderSubType);
                        newOrderDetail.OrderNo = orderNo;
                        newOrderDetailList.Add(newOrderDetail);
                    }
                }
                if (updatedOrderDetails != null)
                {
                    //现在控件控制不住，改了Item也默认是之前的,加好的只能改数量,库位
                    foreach (var orderDetail in updatedOrderDetails)
                    {
                        decimal qty = orderDetail.OrderedQty;
                        OrderDetail updateOrderDetail = base.genericMgr.FindById<OrderDetail>(orderDetail.Id);
                        updateOrderDetail.OrderedQty = qty;
                        if (!string.IsNullOrEmpty(orderDetail.LocationFrom) && updateOrderDetail.LocationFrom != orderDetail.LocationFrom)
                        {
                            updateOrderDetail.LocationFrom = orderDetail.LocationFrom;
                            updateOrderDetail.LocationFromName = base.genericMgr.FindById<Location>(orderDetail.LocationFrom).Name;
                        }
                        if (!string.IsNullOrEmpty(orderDetail.LocationTo) && updateOrderDetail.LocationTo != orderDetail.LocationTo)
                        {
                            updateOrderDetail.LocationTo = orderDetail.LocationTo;
                            updateOrderDetail.LocationToName = base.genericMgr.FindById<Location>(orderDetail.LocationTo).Name;
                        }
                        if (!string.IsNullOrWhiteSpace(orderDetail.ManufactureParty))
                        {
                            string sql = "select m from FlowMaster as m where m.Type=" + (int)com.Sconit.CodeMaster.OrderType.Procurement + " and m.PartyFrom='" + orderDetail.ManufactureParty + "' and  m.Code in (select distinct d.Flow from FlowDetail as d where d.Item='" + orderDetail.Item + "' )";

                            IList<FlowMaster> flowMasterCount = base.genericMgr.FindAll<FlowMaster>(sql);
                            if (flowMasterCount.Count > 0)
                            {
                                updateOrderDetail.ManufactureParty = orderDetail.ManufactureParty;
                            }
                            else
                            {
                                updateOrderDetail.ManufactureParty = null;
                            }
                        }
                        else
                        {
                            updateOrderDetail.ManufactureParty = null;
                        }
                        updateOrderDetailList.Add(updateOrderDetail);
                    }
                }

                orderMgr.BatchUpdateOrderDetails(orderNo, newOrderDetailList, updateOrderDetailList, (IList<OrderDetail>)deletedOrderDetails);
                SaveSuccessMessage(Resources.ORD.OrderDetail.OrderDetail_Saved);
                return Json(new { });
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }
            return Json(null);
        }

        public ActionResult _WebOrderDetail(string flow, string itemCode, com.Sconit.CodeMaster.OrderSubType orderSubType)
        {
            if (!string.IsNullOrEmpty(flow) && !string.IsNullOrEmpty(itemCode))
            {
                WebOrderDetail webOrderDetail = new WebOrderDetail();
                IList<FlowDetail> flowDetailList = flowMgr.GetFlowDetailList(flow, false, true);
                FlowDetail flowDetail = flowDetailList.Where(d => d.Item == itemCode).FirstOrDefault<FlowDetail>();

                if (flowDetail != null)
                {
                    webOrderDetail.Item = flowDetail.Item;
                    webOrderDetail.ItemDescription = base.genericMgr.FindById<Item>(flowDetail.Item).Description;
                    webOrderDetail.UnitCount = flowDetail.UnitCount;
                    webOrderDetail.Uom = flowDetail.Uom;
                    webOrderDetail.Sequence = flowDetail.Sequence;
                    webOrderDetail.ReferenceItemCode = flowDetail.ReferenceItemCode;
                    webOrderDetail.MinUnitCount = flowDetail.MinUnitCount;
                    webOrderDetail.UnitCountDescription = flowDetail.UnitCountDescription;
                    webOrderDetail.Container = flowDetail.Container;
                    webOrderDetail.ContainerDescription = flowDetail.ContainerDescription;
                    //默认库位
                    webOrderDetail.LocationFrom = orderSubType == com.Sconit.CodeMaster.OrderSubType.Normal ? flowDetail.LocationFrom : flowDetail.LocationTo;
                    webOrderDetail.LocationTo = orderSubType == com.Sconit.CodeMaster.OrderSubType.Normal ? flowDetail.LocationTo : flowDetail.LocationFrom;
                }
                else
                {
                    Item item = base.genericMgr.FindById<Item>(itemCode);
                    if (item != null)
                    {
                        webOrderDetail.Item = item.Code;
                        webOrderDetail.ItemDescription = item.Description;
                        webOrderDetail.UnitCount = item.UnitCount;
                        webOrderDetail.Uom = item.Uom;
                        webOrderDetail.MinUnitCount = item.UnitCount;
                        webOrderDetail.LocationFrom = string.Empty;
                        webOrderDetail.LocationTo = string.Empty;
                    }
                }
                return this.Json(webOrderDetail);
            }
            return null;
        }

        public String _WindowTime(string flow, string windowTime)
        {
            DateTime startDate = DateTime.Parse(windowTime);
            FlowMaster flowMaster = base.genericMgr.FindById<FlowMaster>(flow);
            if (flowMaster != null)
            {
                FlowStrategy flowStrategy = base.genericMgr.FindById<FlowStrategy>(flow);
                if (flowStrategy != null)
                {
                    startDate = startDate.AddHours(Convert.ToDouble(0 - flowStrategy.LeadTime));
                }
            }
            return startDate.ToString("yyyy-MM-dd HH:mm");
        }
        #endregion

        #region  明细查询
        [SconitAuthorize(Permissions = "Url_OrderDetail_Distribution")]
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
        [SconitAuthorize(Permissions = "Url_OrderDetail_Distribution")]
        public ActionResult _AjaxOrderDetailList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<OrderDetail>()));
            }
            IList<OrderDetail> orderDetList = new List<OrderDetail>();
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchDetailStatement(command, searchModel);
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
                                }).ToList();
                #endregion
            }
            procedureSearchStatementModel.PageParameters[2].Parameter = gridModel.Total;
            TempData["OrderMasterPrintSearchModel"] = procedureSearchStatementModel;

            GridModel<OrderDetail> gridModelOrderDet = new GridModel<OrderDetail>();
            gridModelOrderDet.Total = gridModel.Total;
            gridModelOrderDet.Data = orderDetList;

            return PartialView(gridModelOrderDet);
            //  return PartialView(GetAjaxPageDataProcedure<OrderDetail>(procedureSearchStatementModel, command));
        }

        [SconitAuthorize(Permissions = "Url_OrderDetail_Distribution_Return")]
        public ActionResult ReturnDetailList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (this.CheckSearchModelIsNull(searchModel))
            {
                SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
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
        [SconitAuthorize(Permissions = "Url_OrderDetail_Distribution_Return")]
        public ActionResult _AjaxReturnOrderDetailList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<OrderMaster>()));
            }
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareReturnOrderDetailSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageDataProcedure<OrderDetail>(procedureSearchStatementModel, command));
        }

        #endregion

        #region ship
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Ship")]
        public ActionResult ShipIndex()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Ship")]
        public ActionResult Ship(GridCommand command, OrderMasterSearchModel searchModel)
        {
            this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Ship")]
        public ActionResult _AjaxShipOrderList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            //SearchStatementModel searchStatementModel = PrepareShipSearchStatement(command, searchModel);
            //return PartialView(GetAjaxPageData<OrderMaster>(searchStatementModel, command));
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareShipSearchStatement_1(command, searchModel);
            return PartialView(GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Ship")]
        public ActionResult _ShipOrderDetailList(string checkedOrders)
        {
            string[] checkedOrderArray = checkedOrders.Split(',');
            DetachedCriteria criteria = DetachedCriteria.For<OrderDetail>();
            criteria.Add(Expression.In("OrderNo", checkedOrderArray));
            criteria.Add(Expression.LtProperty("ShippedQty", "OrderedQty"));
            IList<OrderDetail> orderDetailList = base.genericMgr.FindAll<OrderDetail>(criteria);
            return PartialView(orderDetailList);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Ship")]
        public ActionResult ShipEdit(string checkedOrders)
        {
            ViewBag.CheckedOrders = checkedOrders;
            string[] checkedOrderArray = checkedOrders.Split(',');
            OrderMaster order = base.genericMgr.FindById<OrderMaster>(checkedOrderArray[0]);
            return View(order);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Ship")]
        public JsonResult ShipOrder(string idStr, string qtyStr, string checkedOrders)
        {
            try
            {
                IList<OrderDetail> orderDetailList = new List<OrderDetail>();
                if (!string.IsNullOrEmpty(idStr))
                {
                    string[] idArray = idStr.Split(',');
                    string[] qtyArray = qtyStr.Split(',');

                    for (int i = 0; i < qtyArray.Count(); i++)
                    {
                        if (Convert.ToDecimal(qtyArray[i]) <= 0)
                        {
                            throw new BusinessException("发货明细所有行发货都不能为零");
                        }
                        if (Convert.ToDecimal(qtyArray[i]) > 0)
                        {
                            OrderDetail od = base.genericMgr.FindById<OrderDetail>(Convert.ToInt32(idArray[i]));

                            OrderDetailInput input = new OrderDetailInput();
                            //把交货单号记录到WMS号上面
                            input.WMSIpNo = od.ExternalOrderNo;
                            input.ShipQty = Convert.ToDecimal(qtyArray[i]);
                            od.AddOrderDetailInput(input);
                            orderDetailList.Add(od);
                        }
                    }
                }
                if (orderDetailList.Count() == 0)
                {
                    throw new BusinessException("发货明细不能为空");
                }

                IpMaster ipMaster = orderMgr.ShipOrder(orderDetailList, DateTime.Now);
                if (ipMaster.OrderType == Sconit.CodeMaster.OrderType.Distribution)
                {
                    orderMgr.ManualCloseOrder(orderDetailList[0].OrderNo);
                }
                SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_Shipped, checkedOrders);
                return Json(new { IpNo = ipMaster.IpNo });
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }
            return Json(null);
        }

        #region Ip页面
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Ship")]
        public ActionResult IpEdit(string IpNo)
        {
            if (string.IsNullOrEmpty(IpNo))
            {
                return HttpNotFound();
            }
            else
            {
                IpMaster ipMaster = base.genericMgr.FindById<IpMaster>(IpNo);
                return View(ipMaster);
            }
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Ship")]
        public ActionResult IpDetail(GridCommand command, IpDetailSearchModel searchModel, string ipNo)
        {
            IpMaster ipMaster = base.genericMgr.FindById<IpMaster>(ipNo);
            ViewBag.IsCancel = ((ipMaster.Status == com.Sconit.CodeMaster.IpStatus.Cancel || ipMaster.Status == com.Sconit.CodeMaster.IpStatus.Close) ? false : true)
                && ipMaster.OrderType == com.Sconit.CodeMaster.OrderType.ScheduleLine && !ipMaster.IsAsnUniqueReceive;
            searchModel.IpNo = ipNo;
            TempData["OrderMasterSearchModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Ship")]
        public ActionResult _AjaxIpDetailList(GridCommand command, IpDetailSearchModel searchModel, string ipNo)
        {
            //SearchStatementModel searchStatementModel = this.IpDetailPrepareSearchStatement(command, searchModel, ipNo);
            //return PartialView(GetAjaxPageData<IpDetail>(searchStatementModel, command));
            SearchStatementModel searchStatementModel = this.IpDetailPrepareSearchStatement(command, searchModel, ipNo);
            GridModel<IpDetail> gridList = GetAjaxPageData<IpDetail>(searchStatementModel, command);
            int i = 0;
            foreach (IpDetail ipDetail in gridList.Data)
            {
                if (i > command.PageSize)
                {
                    break;
                }
                if (!string.IsNullOrEmpty(ipDetail.LocationTo))
                {
                    ipDetail.SAPLocationTo = base.genericMgr.FindById<Location>(ipDetail.LocationTo).SAPLocation;
                }
            }
            gridList.Data = gridList.Data.Where(o => o.Type == com.Sconit.CodeMaster.IpDetailType.Normal);
            return PartialView(gridList);
        }

        private SearchStatementModel IpDetailPrepareSearchStatement(GridCommand command, IpDetailSearchModel searchModel, string ipNo)
        {
            string whereStatement = " where i.IpNo='" + ipNo + "'";

            IList<object> param = new List<object>();

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectIpDetailCountStatement;
            searchStatementModel.SelectStatement = selectIpDetailStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }
        #endregion

        #endregion

        #region PickShip
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Distribution_PickShip")]
        public ActionResult PickShipIndex()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Distribution_PickShip")]
        public ActionResult PickShipList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Distribution_PickShip")]
        public ActionResult _AjaxPickShipList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<OrderDetail>()));
            }
            IList<OrderDetail> orderDetList = new List<OrderDetail>();
            ProcedureSearchStatementModel procedureSearchStatementModel = PreparePickShipStatement(command, searchModel);
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
            procedureSearchStatementModel.PageParameters[2].Parameter = gridModel.Total;
            TempData["OrderMasterPrintSearchModel"] = procedureSearchStatementModel;

            GridModel<OrderDetail> gridModelOrderDet = new GridModel<OrderDetail>();
            gridModelOrderDet.Total = gridModel.Total;
            gridModelOrderDet.Data = orderDetList;

            return PartialView(gridModelOrderDet);
        }


        #region  老的暂时保留 9-22
        //[GridAction(EnableCustomBinding = true)]
        //[SconitAuthorize(Permissions = "Url_Distribution_PickShip")]
        //public ActionResult PickShipOrder(string idStr, string qtyStr)
        //{
        //    try
        //    {
        //        IList<OrderDetail> orderDetailList = new List<OrderDetail>();
        //        if (!string.IsNullOrEmpty(idStr))
        //        {
        //            string[] idArray = idStr.Split(',');
        //            string[] qtyArray = qtyStr.Split(',');

        //            for (int i = 0; i < qtyArray.Count(); i++)
        //            {
        //                if (Convert.ToDecimal(qtyArray[i]) > 0)
        //                {
        //                    OrderDetail od = base.genericMgr.FindById<OrderDetail>(Convert.ToInt32(idArray[i]));

        //                    OrderDetailInput input = new OrderDetailInput();
        //                    input.ShipQty = Convert.ToDecimal(qtyArray[i]);
        //                    od.AddOrderDetailInput(input);
        //                    orderDetailList.Add(od);
        //                }
        //            }
        //        }
        //        if (orderDetailList.Count() == 0)
        //        {
        //            throw new BusinessException("发货明细不能为空");
        //        }

        //        IpMaster ipMaster = orderMgr.ShipOrder(orderDetailList, DateTime.Now);
        //        SaveSuccessMessage(string.Format("操作成功，生成送货单号{0}。",ipMaster.IpNo));

        //    }
        //    catch (BusinessException ex)
        //    {
        //        SaveBusinessExceptionMessage(ex);
        //    }
        //    catch (Exception ex)
        //    {
        //        SaveErrorMessage(ex);
        //    }
        //    return RedirectToAction("PickShipList");
        //}
        #endregion

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Distribution_PickShip")]
        public ActionResult PickShipOrder(string idStr, string qtyStr)
        {
            try
            {
                IList<OrderDetail> orderDetailList = new List<OrderDetail>();
                if (!string.IsNullOrEmpty(idStr))
                {
                    string[] idArray = idStr.Split(',');
                    string[] qtyArray = qtyStr.Split(',');
                    //string pickNo = pickListMgr.CreatePickList4Qty(idArray, qtyArray);
                    //SaveSuccessMessage(string.Format("操作成功，生成拣货单号{0}。", pickNo));
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
            return RedirectToAction("PickShipList");
        }

        #endregion

        #region 批量导入发货
        public ActionResult BatchImportShip(IEnumerable<HttpPostedFileBase> attachments)
        {
            try
            {
                foreach (var file in attachments)
                {
                    orderMgr.BatchImportShipXls(file.InputStream);
                    SaveSuccessMessage("批量发货成功。");
                }
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage("导入失败。 - " + ex.Message);
            }

            return Content(string.Empty);
        }
        #endregion

        #region 退货单 发货
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_ReturnShip")]
        public ActionResult ReturnShip()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_ReturnShip")]
        public ActionResult ReturnShipList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        #endregion

        #region batch

        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_BatchProcess")]
        public ActionResult BatchProcessIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_BatchProcess")]
        public ActionResult BatchProcessList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            TempData["OrderMasterSearchModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Submit")]
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

                    }
                    SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_BatchSubmited);
                }
                catch (BusinessException ex)
                {
                    SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
                }
            }

            return RedirectToAction("BatchProcessList");
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Start")]
        public ActionResult BatchStart(string orderStr)
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
                        orderMgr.StartOrder(orderNo);

                    }
                    SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_BatchStarted);
                }
                catch (BusinessException ex)
                {
                    SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
                }
            }

            return RedirectToAction("BatchProcessList");
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Delete")]
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

                    }
                    SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_BatchDeleted);
                }
                catch (BusinessException ex)
                {
                    SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
                }
            }

            return RedirectToAction("BatchProcessList");
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Cancel")]
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

                    }
                    SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_BatchCanceled);
                }
                catch (BusinessException ex)
                {
                    SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
                }
            }

            return RedirectToAction("BatchProcessList");
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Close")]
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

                    }
                    SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_BatchClosed);
                }
                catch (BusinessException ex)
                {
                    SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
                }
            }

            return RedirectToAction("BatchProcessList");
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_BatchProcess")]
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

            return RedirectToAction("BatchProcessList");
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_BatchProcess")]
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

            return RedirectToAction("BatchProcessList");
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_BatchProcess")]
        public ActionResult _AjaxBatchProcessList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            //string whereStatement = " where o.Type in (" + (int)com.Sconit.CodeMaster.OrderType.Distribution + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + ")"
            //          + " and o.SubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal;
            //SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel, whereStatement);
            //return PartialView(GetAjaxPageData<OrderMaster>(searchStatementModel, command));
            string whereStatement = string.Empty;
            searchModel.SubType = (int)com.Sconit.CodeMaster.OrderSubType.Normal;
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchStatement_1(command, searchModel, whereStatement, false);
            return PartialView(GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command));
        }
        #endregion

        #region 打印导出
        public void SaveToClient(string orderNo)
        {
            OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);
            IList<OrderDetail> orderDetails = base.genericMgr.FindAll<OrderDetail>("select od from OrderDetail as od where od.OrderNo=?", orderNo);
            orderMaster.OrderDetails = orderDetails;
            PrintOrderMaster printOrderMstr = Mapper.Map<OrderMaster, PrintOrderMaster>(orderMaster);
            IList<object> data = new List<object>();
            data.Add(printOrderMstr);
            data.Add(printOrderMstr.OrderDetails);
            reportGen.WriteToClient(orderMaster.OrderTemplate, data, orderMaster.OrderTemplate);
        }

        public string Print(string orderNo)
        {
            OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);
            IList<OrderDetail> orderDetails = base.genericMgr.FindAll<OrderDetail>("select od from OrderDetail as od where od.OrderNo=?", orderNo);
            IList<OrderBomDetail> orderBomDetails = new List<OrderBomDetail>();
            orderMaster.OrderDetails = orderDetails;
            PrintOrderMaster printOrderMstr = Mapper.Map<OrderMaster, PrintOrderMaster>(orderMaster);
            IList<object> data = new List<object>();
            data.Add(printOrderMstr);
            data.Add(printOrderMstr.OrderDetails);
            if (orderMaster.Type == com.Sconit.CodeMaster.OrderType.Production)
            {
                string selectOrderBomDetailStatement = "select d from OrderBomDetail as d where d.OrderNo = ?";
                orderBomDetails = base.genericMgr.FindAll<OrderBomDetail>(selectOrderBomDetailStatement, new object[] { orderNo });
                data.Add(Mapper.Map<IList<OrderBomDetail>, IList<PrintOrderBomDetail>>(orderBomDetails));
            }
            return reportGen.WriteToFile(orderMaster.OrderTemplate, data);
        }

        #region 导出明细
        public void SaveOrderDetailViewToClient()
        {
            ProcedureSearchStatementModel procedureSearchStatementModel = TempData["OrderMasterPrintSearchModel"] as ProcedureSearchStatementModel;
            TempData["OrderMasterPrintSearchModel"] = procedureSearchStatementModel;

            GridCommand command = new GridCommand();
            command.Page = 1;
            command.PageSize = (int)procedureSearchStatementModel.PageParameters[2].Parameter;
            procedureSearchStatementModel.PageParameters[3].Parameter = 1;
            GridModel<object[]> gridModel = GetAjaxPageDataProcedure<object[]>(procedureSearchStatementModel, command);

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
                                    OrderStrategyDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.FlowStrategy, int.Parse((tak[24]).ToString())),
                                    MastWindowTime = (DateTime)tak[25],
                                }).ToList();
                #endregion
            }

            // IList<PrintOrderDetail> printOrderetails = Mapper.Map<IList<OrderDetail>, IList<PrintOrderDetail>>(orderDetList);
            //IList<object> data = new List<object>();
            //data.Add(orderDetList);
            //reportGen.WriteToClient("LogisticOrderDetView.xls", data, "LogisticOrderDetView.xls");
            ExportToXLS<OrderDetail>("ExportShipDetail", "xls", orderDetList);
        }
        #endregion

        #endregion

        #region return order
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_ReturnIndex")]
        public ActionResult ReturnIndex()
        {
            return View();
        }

        #region return edit

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_ReturnIndex")]
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
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_ReturnIndex")]
        public ActionResult _ReturnAjaxList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<OrderMaster>()));
            }
            //string whereStatement = " where o.Type in (" + (int)com.Sconit.CodeMaster.OrderType.Distribution + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + ")"
            //                      + " and o.SubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Return;
            //SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel, whereStatement);
            //return PartialView(GetAjaxPageData<OrderMaster>(searchStatementModel, command));
            string whereStatement = string.Empty;
            searchModel.SubType = (int)com.Sconit.CodeMaster.OrderSubType.Return;
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchStatement_1(command, searchModel, whereStatement, true);
            return PartialView(GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command));
        }
        #endregion

        #endregion

        #region private method
        private IList<OrderDetail> TransformFlowDetailList2OrderDetailList(string flow, com.Sconit.CodeMaster.OrderSubType orderSubType)
        {

            IList<FlowDetail> flowDetailList = flowMgr.GetFlowDetailList(flow);
            IList<OrderDetail> orderDetailList = new List<OrderDetail>();
            foreach (FlowDetail flowDetail in flowDetailList)
            {
                OrderDetail orderDetail = new OrderDetail();

                Mapper.Map<FlowDetail, OrderDetail>(flowDetail, orderDetail);
                if (orderSubType == com.Sconit.CodeMaster.OrderSubType.Return)
                {
                    orderDetail.LocationFrom = flowDetail.LocationTo;
                    orderDetail.LocationTo = flowDetail.LocationFrom;
                }
                orderDetail.Id = flowDetail.Id;
                orderDetail.ItemDescription = base.genericMgr.FindById<Item>(flowDetail.Item).Description;
                orderDetailList.Add(orderDetail);
            }
            return orderDetailList;
        }

        private OrderDetail RefreshOrderDetail(string flow, OrderDetail orderDetail, com.Sconit.CodeMaster.OrderSubType orderSubType)
        {

            OrderDetail newOrderDetail = new OrderDetail();
            IList<FlowDetail> flowDetailList = flowMgr.GetFlowDetailList(flow, false, true);
            FlowDetail flowDetail = flowDetailList.Where<FlowDetail>(q => q.Item == orderDetail.Item).FirstOrDefault();
            if (flowDetail != null)
            {
                Mapper.Map<FlowDetail, OrderDetail>(flowDetail, newOrderDetail);
                if (orderSubType == Sconit.CodeMaster.OrderSubType.Return)
                {
                    newOrderDetail.LocationFrom = flowDetail.LocationTo;
                    newOrderDetail.LocationTo = flowDetail.LocationFrom;
                    newOrderDetail.IsInspect = flowDetail.IsRejectInspect;
                }
                newOrderDetail.Sequence = orderDetail.Sequence == 0 ? newOrderDetail.Sequence : orderDetail.Sequence;
                newOrderDetail.UnitCount = orderDetail.UnitCount == 0 ? newOrderDetail.UnitCount : orderDetail.UnitCount;
                newOrderDetail.Uom = String.IsNullOrWhiteSpace(orderDetail.Uom) ? newOrderDetail.Uom : orderDetail.Uom;
                newOrderDetail.ItemDescription = base.genericMgr.FindById<Item>(orderDetail.Item).Description;
                newOrderDetail.MinUnitCount = orderDetail.MinUnitCount == 0 ? newOrderDetail.MinUnitCount : orderDetail.MinUnitCount;
            }
            else
            {
                Item item = base.genericMgr.FindById<Item>(orderDetail.Item);
                if (item != null)
                {
                    newOrderDetail.Item = item.Code;
                    newOrderDetail.UnitCount = orderDetail.UnitCount == 0 ? item.UnitCount : orderDetail.UnitCount;
                    newOrderDetail.Uom = String.IsNullOrWhiteSpace(orderDetail.Uom) ? item.Uom : orderDetail.Uom;
                    newOrderDetail.ItemDescription = item.Description;
                    newOrderDetail.Sequence = orderDetail.Sequence;
                    newOrderDetail.MinUnitCount = orderDetail.MinUnitCount == 0 ? item.UnitCount : orderDetail.MinUnitCount;
                    FlowMaster flowMaster = base.genericMgr.FindById<FlowMaster>(flow);
                    if (flowMaster != null)
                    {
                        newOrderDetail.IsCreatePickList = flowMaster.IsCreatePickList;
                    }
                }
            }

            newOrderDetail.OrderedQty = orderDetail.OrderedQty;
            if (!string.IsNullOrEmpty(orderDetail.LocationFrom))
            {
                newOrderDetail.LocationFrom = orderDetail.LocationFrom;
            }
            if (!string.IsNullOrEmpty(orderDetail.LocationTo))
            {
                newOrderDetail.LocationTo = orderDetail.LocationTo;
            }
            if (!string.IsNullOrEmpty(newOrderDetail.LocationFrom))
            {
                newOrderDetail.LocationFromName = base.genericMgr.FindById<Location>(newOrderDetail.LocationFrom).Name;
            }
            if (!string.IsNullOrEmpty(newOrderDetail.LocationTo))
            {
                newOrderDetail.LocationToName = base.genericMgr.FindById<Location>(newOrderDetail.LocationTo).Name;
            }
            if (!string.IsNullOrWhiteSpace(orderDetail.ManufactureParty))
            {
                string sql = "select count(*) as counter from FlowMaster as m where m.Type=" + (int)com.Sconit.CodeMaster.OrderType.Procurement + " and m.PartyFrom='" + orderDetail.ManufactureParty + "' and  m.Code in (select distinct d.Flow from FlowDetail as d where d.Item='" + orderDetail.Item + "' )";

                IList flowMasterCount = base.genericMgr.FindAll(sql);
                if (flowMasterCount != null && flowMasterCount.Count > 0 && flowMasterCount[0] != null && (long)flowMasterCount[0] > 0)
                {
                    newOrderDetail.ManufactureParty = orderDetail.ManufactureParty;
                }
                else
                {
                    newOrderDetail.ManufactureParty = null;
                }
            }
            else
            {
                newOrderDetail.ManufactureParty = null;
            }

            return newOrderDetail;
        }

        private ProcedureSearchStatementModel PrepareSearchStatement_1(GridCommand command, OrderMasterSearchModel searchModel, string whereStatement, bool isReturn)
        {
            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.OrderNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Flow, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter
            {
                Parameter = (int)com.Sconit.CodeMaster.OrderType.Distribution,
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
            paraList.Add(new ProcedureParameter { Parameter = isReturn, Type = NHibernate.NHibernateUtil.Boolean });
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
                else if (command.SortDescriptors[0].Member == "FlowDescription")
                {
                    command.SortDescriptors[0].Member = "FlowDesc";
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

        private SearchStatementModel PrepareSearchStatement(GridCommand command, OrderMasterSearchModel searchModel, string whereStatement)
        {

            IList<object> param = new List<object>();

            //SecurityHelper.AddPartyFromPermissionStatement(ref whereStatement, "o", "PartyFrom", com.Sconit.CodeMaster.OrderType.Distribution, false);
            //SecurityHelper.AddPartyToPermissionStatement(ref whereStatement, "o", "PartyTo", com.Sconit.CodeMaster.OrderType.Distribution);
            SecurityHelper.AddPartyFromAndPartyToPermissionStatement(ref whereStatement, "o", "Type", "o", "PartyFrom", "o", "PartyTo", com.Sconit.CodeMaster.OrderType.Distribution, false);

            HqlStatementHelper.AddLikeStatement("WMSNo", searchModel.WMSNO, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("ReferenceOrderNo", searchModel.ReferenceOrderNo, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("ExternalOrderNo", searchModel.ExternalOrderNo, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("OrderNo", searchModel.OrderNo, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("PartyFrom", searchModel.PartyFrom, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("PartyTo", searchModel.PartyTo, "o", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("CreateUserName", searchModel.CreateUserName, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Status", searchModel.Status, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Priority", searchModel.Priority, "o", ref whereStatement, param);

            if (searchModel.DateFrom != null & searchModel.DateTo != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.DateFrom, searchModel.DateTo, "o", ref whereStatement, param);
            }
            else if (searchModel.DateFrom != null & searchModel.DateTo == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.DateFrom, "o", ref whereStatement, param);
            }
            else if (searchModel.DateFrom == null & searchModel.DateTo != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.DateTo, "o", ref whereStatement, param);
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

        private SearchStatementModel PrepareShipSearchStatement(GridCommand command, OrderMasterSearchModel searchModel)
        {

            string whereStatement = " where o.Type in (" + (int)com.Sconit.CodeMaster.OrderType.Distribution + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + ")"
                                    + " and o.IsShipScanHu = 0 and o.IsShipByOrder = 1 and o.Status in (" + (int)com.Sconit.CodeMaster.OrderStatus.Submit + "," + (int)com.Sconit.CodeMaster.OrderStatus.InProcess + ")"
                                    + " and o.SubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal
                                    + " and exists (select 1 from OrderDetail as d where d.ShippedQty < d.OrderedQty and d.OrderNo = o.OrderNo) ";

            IList<object> param = new List<object>();

            //SecurityHelper.AddPartyFromPermissionStatement(ref whereStatement, "o", "PartyFrom", com.Sconit.CodeMaster.OrderType.Distribution, false);
            //SecurityHelper.AddPartyToPermissionStatement(ref whereStatement, "o", "PartyTo", com.Sconit.CodeMaster.OrderType.Distribution);
            SecurityHelper.AddPartyFromAndPartyToPermissionStatement(ref whereStatement, "o", "Type", "o", "PartyFrom", "o", "PartyTo", com.Sconit.CodeMaster.OrderType.Distribution, false);

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
            if (!string.IsNullOrEmpty(searchModel.Dock))
            {
                HqlStatementHelper.AddLikeStatement("Dock", searchModel.Dock, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            }
            if (!string.IsNullOrEmpty(searchModel.ExternalOrderNo))
            {
                HqlStatementHelper.AddEqStatement("ExternalOrderNo", searchModel.ExternalOrderNo, "o", ref whereStatement, param);
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

        private ProcedureSearchStatementModel PrepareShipSearchStatement_1(GridCommand command, OrderMasterSearchModel searchModel)
        {

            string whereStatement = " and o.IsShipScanHu = 0 and o.IsShipByOrder = 1 and o.Status in (" + (int)com.Sconit.CodeMaster.OrderStatus.Submit + "," + (int)com.Sconit.CodeMaster.OrderStatus.InProcess + ")"
                                    + " and o.SubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal;
            // + " and exists (select 1 from OrderDetail as d where d.ShipQty < d.OrderQty and d.OrderNo = o.OrderNo) ";
            if (!string.IsNullOrEmpty(searchModel.Picker))
            {
                whereStatement += " and exists (select 1 from OrderDetail as d where d.ShipQty =0 and d.OrderNo = o.OrderNo)";
            }
            else
            {
                whereStatement += " and exists (select 1 from OrderDetail as d where d.ShipQty < d.OrderQty and d.OrderNo = o.OrderNo)";
            }
            if (!string.IsNullOrEmpty(searchModel.Dock))
            {
                whereStatement += "and o.Dock='" + searchModel.Dock + "'";
            }
            if (!string.IsNullOrEmpty(searchModel.ExternalOrderNo))
            {
                whereStatement += "and o.ExtOrderNo='" + searchModel.ExternalOrderNo + "'";
            }

            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.OrderNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Flow, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter
            {
                Parameter = (int)com.Sconit.CodeMaster.OrderType.Distribution,
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

        private ProcedureSearchStatementModel PrepareSearchDetailStatement(GridCommand command, OrderMasterSearchModel searchModel)
        {
            string whereStatement = string.Format(" and exists (select 1 from OrderMaster  as o where  o.SubType ={0} and o.OrderNo=d.OrderNo ) ",
                (int)com.Sconit.CodeMaster.OrderSubType.Normal);
            if (searchModel.OrderStrategy != null)
            {
                if (searchModel.OrderStrategy.Value == 1 || searchModel.OrderStrategy.Value == 0)
                {
                    whereStatement = string.Format(" and exists (select 1 from OrderMaster  as o where  o.SubType ={0} and o.OrderStrategy in(0,1) and o.OrderNo=d.OrderNo ) ",
                (int)com.Sconit.CodeMaster.OrderSubType.Normal);
                }
                else
                {
                    whereStatement = string.Format(" and exists (select 1 from OrderMaster  as o where  o.SubType ={0} and o.OrderStrategy={1} and o.OrderNo=d.OrderNo ) ",
                 (int)com.Sconit.CodeMaster.OrderSubType.Normal, searchModel.OrderStrategy);
                }

            }
            //if (!string.IsNullOrWhiteSpace(searchModel.Picker))
            //{
            //    var pickRules = this.genericMgr.FindAll<PickRule>(" select pr from PickRule as pr where pr.Picker=?  ", searchModel.Picker);
            //    string items = string.Join("','", pickRules.Select(c => c.Item));
            //    whereStatement += string.Format(" and d.Item in ( '" + items + "' ) ", searchModel.Picker);
            //}
            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.OrderNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Flow, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter
            {
                Parameter = (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + "," + (int)com.Sconit.CodeMaster.OrderType.Distribution
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

        private ProcedureSearchStatementModel PrepareReturnOrderDetailSearchStatement(GridCommand command, OrderMasterSearchModel searchModel)
        {
            string whereStatement = string.Format(" and exists (select 1 from OrderMaster  as o where  o.SubType ={0} and o.OrderNo=d.OrderNo ) ",
                (int)com.Sconit.CodeMaster.OrderSubType.Return);

            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.OrderNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Flow, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter
            {
                Parameter = (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.Procurement
                   + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + ","
                   + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer,
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
            paraList.Add(new ProcedureParameter { Parameter = true, Type = NHibernate.NHibernateUtil.Boolean });
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
                Parameter = (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + "," + (int)com.Sconit.CodeMaster.OrderType.Distribution
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

        #region 交货单过账
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Receipt")]
        public ActionResult ReceiptIndex()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Receipt")]
        public ActionResult ReceiptList(GridCommand command, OrderMasterSearchModel searchModel)
        {

            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            List<string> exterNoList = this.getExterOrderNo(((OrderMasterSearchModel)searchCacheModel.SearchObject).ExternalOrderNo, true);
            if (exterNoList.Count == 0)
            {
                return View(new List<OrderDetail>());
            }
            IList<OrderDetail> list = base.genericMgr.FindAll<OrderDetail>(PrepareReceiptSearchStatement(command, exterNoList));
            if (list.Count > 0)
            {
                IList<string> OrderNos = list.Select(i => i.OrderNo).Distinct().ToList();
                string sql = "select OrderNo,ExtOrderNo from view_ordermstr where OrderNo in (";
                foreach (string orderNo in OrderNos)
                {
                    sql += "'" + orderNo + "',";
                }
                sql = sql.Substring(0, sql.Length - 1) + ")";

                IList<object[]> objectList = base.genericMgr.FindAllWithNativeSql<object[]>(sql);
                foreach (var item in objectList)
                {
                    foreach (OrderDetail orderDetail in list)
                    {
                        if ((string)item[0] == orderDetail.OrderNo)
                        {
                            orderDetail.ExternalOrderNo = (string)item[1];
                        }
                    }
                }
            }
            return View(list);
        }

        private List<string> getExterOrderNo(string exterNo, bool bb)
        {
            if (!string.IsNullOrEmpty(exterNo))
            {

                string externo = exterNo.Replace("\r\n", ",");
                string[] externalOrderNo = externo.Split(',');
                List<string> newExterOrderno = new List<string>();
                foreach (string item in externalOrderNo)
                {
                    if (string.IsNullOrEmpty(item))
                    {
                        continue;
                    }
                    newExterOrderno.Add(item);
                }
                if (newExterOrderno.Count >= 100)
                {
                    if (bb)
                    {
                        SaveWarningMessage(string.Format("输入的交货单号过多为{0}，超过最大数量100。", newExterOrderno.Count));
                    }
                }
                else
                {
                    TempData["_AjaxMessage"] = "";
                    return newExterOrderno;
                }

            }
            else
            {
                if (bb)
                {
                    SaveWarningMessage("请输入交货单号。");
                }
            }
            return new List<string>();
        }


        private string PrepareReceiptSearchStatement(GridCommand command, List<string> exterNoList)
        {
            StringBuilder sb = new StringBuilder();
            string whereStatement = "select d from OrderDetail as d where  exists (select 1 from OrderMaster as o where o.Type ='" + (int)com.Sconit.CodeMaster.OrderType.Distribution
                                    + "'and o.Status in (" + (int)com.Sconit.CodeMaster.OrderStatus.Create + "," + (int)com.Sconit.CodeMaster.OrderStatus.Submit + "," + (int)com.Sconit.CodeMaster.OrderStatus.InProcess
                                    + ") and o.ExternalOrderNo in (";
            sb.Append(whereStatement);
            foreach (string item in exterNoList)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }
                else
                {
                    sb.Append("'" + item + "',");
                }
            }

            sb.Remove(sb.Length - 1, 1);

            sb.Append(") and d.OrderNo = o.OrderNo)");

            return sb.ToString();

        }


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Distribution_Ship")]
        public JsonResult ReceiveOrder(string idStr, string qtyStr, string orderNoStr)
        {
            StringBuilder ExterNoSb = new StringBuilder();
            try
            {
                if (!string.IsNullOrEmpty(idStr))
                {
                    string[] idArray = idStr.Split(',');
                    string[] qtyArray = qtyStr.Split(',');
                    string[] orderNoArray = orderNoStr.Split(',');
                    #region 将交货单明细分组
                    IList<OrderDetail> detailList = new List<OrderDetail>();
                    IList<OrderMaster> masterList = new List<OrderMaster>();
                    //所有的交货明细
                    for (int i = 0; i < idArray.Count(); i++)
                    {
                        if (Convert.ToDecimal(qtyArray[i]) > 0)
                        {
                            OrderDetail od = base.genericMgr.FindById<OrderDetail>(Convert.ToInt32(idArray[i]));
                            //od.CurrentReceiveQty = int.Parse(qtyArray[i]);
                            OrderDetailInput input = new OrderDetailInput();
                            input.ReceiveQty = Convert.ToDecimal(qtyArray[i]);
                            od.AddOrderDetailInput(input);
                            detailList.Add(od);
                        }
                    }
                    //所有的交货单头
                    foreach (string orderNo in orderNoArray)
                    {
                        OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);
                        if (!masterList.Contains(orderMaster))
                        {
                            masterList.Add(orderMaster);
                        }
                    }
                    //所有的明细存到头里面 进行分组
                    foreach (OrderMaster orderMaster in masterList)
                    {
                        IList<OrderDetail> ordDetailList = new List<OrderDetail>();
                        foreach (OrderDetail orderDetail in detailList)
                        {
                            if (orderMaster.OrderNo == orderDetail.OrderNo)
                            {
                                ordDetailList.Add(orderDetail);
                            }
                        }
                        orderMaster.OrderDetails = ordDetailList;
                    }
                    #endregion

                    BusinessException businessException = new BusinessException();

                    foreach (OrderMaster orderMaster in masterList)
                    {
                        try
                        {
                            orderMgr.DistributionReceiveOrder(orderMaster);
                            ExterNoSb.Append(orderMaster.ExternalOrderNo + ",");
                        }
                        catch (BusinessException ex)
                        {
                            businessException.AddMessage(ex.GetMessages()[0].GetMessageString() + "交货单号为：" + orderMaster.ExternalOrderNo);
                        }
                    }
                    if (businessException.HasMessage)
                    {
                        throw businessException;
                    }
                    SaveSuccessMessage("交货单" + ExterNoSb.Remove(ExterNoSb.Length - 1, 1) + "过账成功。");
                    return Json(new { });
                }
                else
                {
                    throw new BusinessException("过账明细不能为空。");
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
            return Json(null);
        }

        #endregion

    }
}
