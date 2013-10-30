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
    using System.Reflection;
    using com.Sconit.Entity.INV;
    using System.Web;
    using System.ComponentModel;
    using NHibernate.Type;
    using NHibernate;
    using System.Web.Routing;
    using com.Sconit.Entity.FIS;

    public class ProcurementOrderController : WebAppBaseController
    {
        /// <summary>
        /// 
        /// </summary>
        private static string selectCountStatement = "select count(*) from OrderMaster as o";

        private static string selectStatement = "select o from OrderMaster as o";

        private static string selectOrderDetailStatement = "select d from OrderDetail as d where d.OrderNo=?";

        private static string selectFlowDetailStatement = "select d from FlowDetail as d where d.Flow = ?";

        private static string selectOneFlowDetailStatement = "select d from FlowDetail as d where d.Flow = ? and d.Item = ?";

        public IOrderMgr orderMgr { get; set; }

        public IFlowMgr flowMgr { get; set; }

        public IReportGen reportGen { get; set; }

        public IHuMgr huMgr { get; set; }

        public com.Sconit.Persistence.INHQueryDao dao { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ProcurementOrderController()
        {
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_View")]
        public ActionResult Index()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OrderDetail_Procurement_Return")]
        public ActionResult ReturnDetailIndex()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OrderDetail_Procurement_View")]
        public ActionResult DetailIndex()
        {
            return View();
        }

        #region edit
        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_View")]
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
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_View")]
        public ActionResult _AjaxList(GridCommand command, OrderMasterSearchModel searchModel)
        {

            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<OrderMaster>()));
            }

            //string whereStatement = " where o.OrderStrategy != " + (int)com.Sconit.CodeMaster.FlowStrategy.SEQ + " and o.Type in (" + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.Procurement
            //            + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + (int)com.Sconit.CodeMaster.OrderType.ScheduleLine + ")"
            //            + " and o.SubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal;
            string whereStatement = string.Empty;
            if (searchModel.Item != null && searchModel.Item != string.Empty)
            {
                whereStatement += " and exists(select 1 from OrderDetail as d where d.OrderNo=o.OrderNo and d.Item ='" + searchModel.Item + "')";
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
            if (!string.IsNullOrWhiteSpace(searchModel.Shift))
            {
                whereStatement += " and o.Shift="+searchModel.Shift;
            }
            //whereStatement += " and o.OrderStrategy != " + (int)com.Sconit.CodeMaster.FlowStrategy.SEQ;
            searchModel.SubType = (int)com.Sconit.CodeMaster.OrderSubType.Normal;
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchStatement_1(command, searchModel, whereStatement, false);
            return PartialView(GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command));
        }

        #region 明细菜单 明细报表
        [SconitAuthorize(Permissions = "Url_OrderDetail_Procurement_View")]
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
        [SconitAuthorize(Permissions = "Url_OrderDetail_Procurement_View")]
        public ActionResult AjaxOrderDetailList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<OrderDetail>()));
            }
            string whereStatement = string.Format(" and exists (select 1 from OrderMaster as o  with(nolock) where  o.SubType ={0} and o.OrderNo=d.OrderNo ) ",
               (int)com.Sconit.CodeMaster.OrderSubType.Normal);
            if (searchModel.OrderStrategy != null && !string.IsNullOrWhiteSpace(searchModel.Shift))
            {

                if (searchModel.OrderStrategy.Value == 1 || searchModel.OrderStrategy.Value == 0)
                {
                    whereStatement = string.Format(" and exists (select 1 from OrderMaster as o  with(nolock) where  o.SubType ={0} and o.OrderStrategy in (0,1) and o.OrderNo=d.OrderNo and o.Shift={1} ) ",
                (int)com.Sconit.CodeMaster.OrderSubType.Normal, searchModel.Shift);
                }
                else
                {
                    whereStatement = string.Format(" and exists (select 1 from OrderMaster as o  with(nolock) where  o.SubType ={0} and o.OrderStrategy={1} and o.OrderNo=d.OrderNo  and o.Shift={2}) ",
                (int)com.Sconit.CodeMaster.OrderSubType.Normal, searchModel.OrderStrategy, searchModel.Shift);
                }

            }
            else if (searchModel.OrderStrategy == null && !string.IsNullOrWhiteSpace(searchModel.Shift))
            {
                whereStatement = string.Format(" and exists (select 1 from OrderMaster as o  with(nolock) where  o.SubType ={0} and o.OrderNo=d.OrderNo and o.Shift={1} ) ",
               (int)com.Sconit.CodeMaster.OrderSubType.Normal, searchModel.Shift);
            }
            else if (searchModel.OrderStrategy != null && string.IsNullOrWhiteSpace(searchModel.Shift))
            {
                if (searchModel.OrderStrategy.Value == 1 || searchModel.OrderStrategy.Value == 0)
                {
                    whereStatement = string.Format(" and exists (select 1 from OrderMaster as o  with(nolock) where  o.SubType ={0} and o.OrderStrategy in (0,1) and o.OrderNo=d.OrderNo ) ",
                (int)com.Sconit.CodeMaster.OrderSubType.Normal);
                }
                else
                {
                    whereStatement = string.Format(" and exists (select 1 from OrderMaster as o  with(nolock) where  o.SubType ={0} and o.OrderStrategy={1} and o.OrderNo=d.OrderNo ) ",
                (int)com.Sconit.CodeMaster.OrderSubType.Normal, searchModel.OrderStrategy);
                }
            }

            if (!string.IsNullOrWhiteSpace(searchModel.TraceCode))
            {
                whereStatement += " and d.ReserveNo='" + searchModel.TraceCode + "' ";
            }
            if (!string.IsNullOrWhiteSpace(searchModel.ZOPWZ))
            {
                whereStatement += " and d.ZOPWZ like '" + searchModel.ZOPWZ + "%' ";
            }
            if (searchModel.IsNoneClsoe)
            {
                whereStatement += " and d.OrderQty>d.RecQty and exists(select 1 from OrderMaster as o  with(nolock) where   o.Status not in (4,5) and o.OrderNo=d.OrderNo ) ";
            }
            if (!string.IsNullOrWhiteSpace(searchModel.LocationTo))
            {
                whereStatement += " and d.LocTo='" + searchModel.LocationTo + "' ";
            }
            if (!string.IsNullOrWhiteSpace(searchModel.LocationFrom))
            {
                whereStatement += " and d.LocFrom='"+searchModel.LocationFrom+"' ";
            }
            if (searchModel.IsClsoe)
            {
                whereStatement += " and d.RecLotSize=1 ";
            }
           

            IList<OrderDetail> orderDetList = new List<OrderDetail>();
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchDetailStatement(command, searchModel, whereStatement);
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
                                                     OrderedQty = tak[36] != null && (decimal?)tak[36] == 1 ? (decimal)tak[37] : (decimal)tak[11],
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
                                                     OrderPriorityDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.OrderPriority, int.Parse((tak[26]).ToString())),
                                                     ContainerDescription = (string)tak[27],
                                                     MinUnitCount = (decimal)tak[28],
                                                     ReserveNo = (string)tak[29],
                                                     BinTo = (string)tak[31],
                                                     ZOPWZ = (string)tak[34],
                                                     CreateOrderCode = (string)tak[35],
                                                     ReceiveLotSize = (decimal?)tak[36],
                                                     UnitPrice = (decimal?)tak[37],
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
        #endregion

        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Edit")]
        public ActionResult New()
        {
            ViewBag.flow = null;
            ViewBag.orderNo = null;
            return View();
        }


        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Edit,Url_OrderMstr_Procurement_ReturnNew,Url_OrderMstr_Procurement_ReturnQuickNew,Url_OrderMstr_Procurement_QuickNew")]
        public JsonResult CreateOrder(OrderMaster orderMaster,
            [Bind(Prefix = "inserted")]IEnumerable<OrderDetail> insertedOrderDetails,
            [Bind(Prefix = "updated")]IEnumerable<OrderDetail> updatedOrderDetails)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderMaster.Shift))
                {
                    throw new BusinessException("要货原因不能为空。");
                }
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
                        if (!string.IsNullOrEmpty(orderDetail.LocationTo))
                        {
                            orderDetail.LocationToName = base.genericMgr.FindById<Location>(orderDetail.LocationTo).Name;
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
                        if (orderDetail.OrderedQty > 0)
                        {
                            orderDetailList.Add(orderDetail);
                        }
                    }
                }
                #endregion

                if (orderDetailList.Count == 0)
                {
                    throw new BusinessException(Resources.ORD.OrderMaster.Errors_OrderDetailIsEmpty);
                }

                FlowMaster flow = base.genericMgr.FindById<FlowMaster>(orderMaster.Flow);

                #region 策略是SEQ的明细只能有一条
                if (flow.FlowStrategy == com.Sconit.CodeMaster.FlowStrategy.SEQ && orderDetailList.Count > 1)
                {
                    throw new BusinessException(Resources.ORD.OrderMaster.Errors_SEQOrderDetailMoreThanOne);
                }
                #endregion
                if (orderMaster.SubType == com.Sconit.CodeMaster.OrderSubType.Return)
                {
                    flow = flowMgr.GetReverseFlow(flow, null);
                    //  base.genericMgr.CleanSession();
                }
                DateTime effectiveDate = orderMaster.EffectiveDate.HasValue ? orderMaster.EffectiveDate.Value : DateTime.Now;
                OrderMaster newOrder = orderMgr.TransferFlow2Order(flow, null, effectiveDate, false);
                newOrder.IsQuick = orderMaster.IsQuick;
                newOrder.IsShipByOrder = orderMaster.IsShipByOrder;
                newOrder.WindowTime = orderMaster.IsQuick ? DateTime.Now : orderMaster.WindowTime;
                newOrder.StartTime = orderMaster.IsQuick ? DateTime.Now : orderMaster.StartTime;
                newOrder.ReferenceOrderNo = orderMaster.ReferenceOrderNo;
                newOrder.ExternalOrderNo = orderMaster.ExternalOrderNo;
                //要货原因
                newOrder.Shift = orderMaster.Shift;
                #region 策略是SEQ的排序组赋值
                if (flow.FlowStrategy == com.Sconit.CodeMaster.FlowStrategy.SEQ)
                {
                    newOrder.SequenceGroup = this.genericMgr.FindAll<FlowStrategy>(" select f from FlowStrategy as f where f.Flow=? ", flow.Code).First().SeqGroup;
                }
                #endregion

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

        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_ReturnNew")]
        public ActionResult ReturnNew()
        {

            return View();
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_ReturnQuickNew")]
        public ActionResult ReturnQuickNew()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_QuickNew")]
        public ActionResult QuickNew()
        {

            return View();
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Edit")]
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
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Edit")]
        public ActionResult _Edit(string orderNo)
        {

            OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);

            ViewBag.isEditable = orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.Create;
            ViewBag.editorTemplate = orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.Create ? "" : "ReadonlyTextBox";

            return PartialView(orderMaster);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Edit")]
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

        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Delete")]
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

        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Submit")]
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


        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Close")]
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

        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Cancel")]
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

        public ActionResult _CurrencyValue(string value)
        {
            PriceListMaster master = null;

            try
            {
                master = base.genericMgr.FindById<PriceListMaster>(value);
                return new JsonResult { Data = master.Currency };
            }
            catch (Exception)
            {
                return new JsonResult { Data = "" };
            }


        }

        public ActionResult _OrderDetailList(string flow, string orderNo, int? orderSubType,string items)
        {
            ViewBag.isManualCreateDetail = false;
            ViewBag.flow = flow;
            ViewBag.orderNo = orderNo;
            ViewBag.newOrEdit = "New";
            ViewBag.status = null;
            ViewBag.orderSubType = (orderSubType != null && orderSubType.Value == (int)com.Sconit.CodeMaster.OrderSubType.Return) ? com.Sconit.CodeMaster.OrderSubType.Return : com.Sconit.CodeMaster.OrderSubType.Normal;
            ViewBag.items = items;
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
                //IList<Location> locationTos = new List<Location>();
                //if (flowMaster.PartyTo != null)
                //{
                //    locationTos = base.genericMgr.FindAll<Location>(selecLocationStatement, new object[] { flowMaster.PartyTo, true });
                //}
                //ViewData.Add("locationTos", locationTos);
                #endregion
            }

            return PartialView();
        }

        [GridAction]
        public ActionResult _SelectBatchEditing(GridCommand command, string orderNo, string flow, com.Sconit.CodeMaster.OrderSubType orderSubType,string items)
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
                        orderDetailList = TransformFlowDetailList2OrderDetailList(flow, orderSubType,items);
                    }
                }
            }
            GridModel<OrderDetail> orderDets = new GridModel<OrderDetail>();
            //ViewBag.Total = orderDetailList.Count();
            orderDets.Total = orderDetailList.Count();
            orderDets.Data = orderDetailList;
                //orderDetailList.Skip((command.Page - 1) * command.PageSize).Take(40).ToList();
            return PartialView(orderDets);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Edit")]
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
                    //现在控件控制不住，改了Item也默认是之前的,加好的只能改数量和库位
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
            try
            {
                DateTime startDate = DateTime.Parse(windowTime);
                FlowMaster flowMaster = base.genericMgr.FindById<FlowMaster>(flow);

                FlowStrategy flowStrategy = base.genericMgr.FindById<FlowStrategy>(flow);
                if (flowStrategy != null)
                {
                    startDate = startDate.AddHours(Convert.ToDouble(0 - flowStrategy.LeadTime));
                }
                return startDate.ToString("yyyy-MM-dd HH:mm");
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion

        #region ReCreateOrderDat
        public ActionResult ReCreateOrderDat(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return HttpNotFound();
                }
                orderMgr.ReCreateOrderDAT(id);
                SaveSuccessMessage("Dat文件创建成功。");
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex.Message);
            }
            return RedirectToAction("Edit", new { orderNo = id });
        }
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
            this.ProcessSearchModel(command, searchModel);
            if (string.IsNullOrWhiteSpace(searchModel.OrderNo) && string.IsNullOrWhiteSpace(searchModel.Flow) && (string.IsNullOrWhiteSpace(searchModel.PartyFrom) || string.IsNullOrWhiteSpace(searchModel.PartyTo)))
            {
                SaveWarningMessage("请选择查询条件");
            }

            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Receive")]
        public ActionResult _AjaxReceiveOrderList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (string.IsNullOrWhiteSpace(searchModel.OrderNo) && string.IsNullOrWhiteSpace(searchModel.Flow) && (string.IsNullOrWhiteSpace(searchModel.PartyFrom) || string.IsNullOrWhiteSpace(searchModel.PartyTo)))
            {
                return PartialView(new GridModel(new List<OrderMaster>()));
            }
            else
            {
                //SearchStatementModel searchStatementModel = PrepareReceiveSearchStatement(command, searchModel);
                //return PartialView(GetAjaxPageData<OrderMaster>(searchStatementModel, command));
                ProcedureSearchStatementModel procedureSearchStatementModel = PrepareReceiveSearchStatement_1(command, searchModel);
                return PartialView(GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command));

            }
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Receive")]
        public ActionResult _ReceiveOrderDetailList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            //string[] checkedOrderArray = checkedOrders.Split(',');
            //DetachedCriteria criteria = DetachedCriteria.For<OrderDetail>();
            //criteria.Add(Expression.In("OrderNo", checkedOrderArray));
            //criteria.Add(Expression.LtProperty("ReceivedQty", "OrderedQty"));
            //IList<OrderDetail> orderDetailList = base.genericMgr.FindAll<OrderDetail>(criteria);
            //return PartialView(orderDetailList);
            this.ProcessSearchModel(command, searchModel);
            if (string.IsNullOrWhiteSpace(searchModel.OrderNo) && string.IsNullOrWhiteSpace(searchModel.Flow) && (string.IsNullOrWhiteSpace(searchModel.PartyFrom) || string.IsNullOrWhiteSpace(searchModel.PartyTo)))
            {
                SaveWarningMessage("请选择查询条件");
            }

            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Receive")]
        public ActionResult _AjaxReceiveDetailList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (string.IsNullOrWhiteSpace(searchModel.OrderNo) && string.IsNullOrWhiteSpace(searchModel.Flow) && (string.IsNullOrWhiteSpace(searchModel.PartyFrom) || string.IsNullOrWhiteSpace(searchModel.PartyTo)))
            {
                return PartialView(new GridModel<OrderDetail>(new List<OrderDetail>()));
            }

            //string whereStatement = string.Format(" and exists (select 1 from OrderMaster  as o where  o.SubType ={0} and o.PartyFrom not in ('SQC','LOC') and o.OrderNo=d.OrderNo ) ",
            //  (int)com.Sconit.CodeMaster.OrderSubType.Normal);

            string whereStatement = " and d.RecQty < d.OrderQty  and exists ( select 1 from OrderMaster as o where 1=1 and d.OrderNo=o.OrderNo and o.IsRecScanHu = 0 and o.PartyFrom not in ('SQC','LOC') and o.Status in (" + (int)com.Sconit.CodeMaster.OrderStatus.InProcess + "," + (int)com.Sconit.CodeMaster.OrderStatus.Submit + ")"
                                   + " and o.SubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal + ")";

            IList<OrderDetail> orderDetList = new List<OrderDetail>();
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchDetailStatement(command, searchModel, whereStatement);
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
                                    OrderPriorityDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.OrderPriority, int.Parse((tak[26]).ToString())),
                                    ContainerDescription = (string)tak[27],
                                    MinUnitCount = (decimal)tak[28],
                                    ReserveNo = (string)tak[29],
                                    BinTo = (string)tak[31],
                                    ZOPWZ = (string)tak[34],
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

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Receive")]
        public ActionResult ReceiveEdit(string checkedOrders)
        {
            ViewBag.CheckedOrders = checkedOrders;
            string[] checkedOrderArray = checkedOrders.Split(',');
            OrderMaster order = base.genericMgr.FindById<OrderMaster>(checkedOrderArray[0]);
            return View(order);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Receive")]
        public JsonResult ReceiveOrder(string idStr, string qtyStr)
        {
            try
            {
                IList<OrderDetail> orderDetailList = new List<OrderDetail>();
                if (!string.IsNullOrEmpty(idStr))
                {
                    string[] idArray = idStr.Split(',');
                    string[] qtyArray = qtyStr.Split(',');

                    for (int i = 0; i < idArray.Count(); i++)
                    {
                        if (Convert.ToDecimal(qtyArray[i]) > 0)
                        {
                            OrderDetail od = base.genericMgr.FindById<OrderDetail>(Convert.ToInt32(idArray[i]));
                            OrderDetailInput input = new OrderDetailInput();
                            input.ReceiveQty = Convert.ToDecimal(qtyArray[i]);
                            od.AddOrderDetailInput(input);
                            orderDetailList.Add(od);
                        }
                    }
                }
                if (orderDetailList.Count() == 0)
                {
                    throw new BusinessException("收货明细不能为空");
                }

                orderMgr.ReceiveOrder(orderDetailList);
                SaveSuccessMessage("收货成功。");
                return Json(new {  });

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

        #region batch


        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_BatchProcess")]
        public ActionResult BatchProcessIndex()
        {
            return View();
        }


        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_BatchProcess")]
        public ActionResult BatchProcessList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_BatchProcess")]
        public ActionResult _AjaxBatchProcessList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            //string whereStatement = " where o.Type in (" + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.Procurement
            //           + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + ")"
            //           + " and o.SubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal;
            //SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel, whereStatement, false);
            //return PartialView(GetAjaxPageData<OrderMaster>(searchStatementModel, command));
            string whereStatement = string.Empty;
            searchModel.SubType = (int)com.Sconit.CodeMaster.OrderSubType.Normal;
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchStatement_1(command, searchModel, whereStatement, false);
            return PartialView(GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command));
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Submit")]
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

        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Delete")]
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

        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Cancel")]
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

        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Close")]
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

            return RedirectToAction("BatchProcessList");
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

            return RedirectToAction("BatchProcessList");
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
            //string reportFileUrl = reportGen.WriteToFile(orderMaster.OrderTemplate, data);
            if (string.IsNullOrWhiteSpace(orderMaster.OrderTemplate))
            {
                reportGen.WriteToClient("ORD_Transfer.xls", data, "ORD_Transfer.xls");
            }
            else
            {
                reportGen.WriteToClient(orderMaster.OrderTemplate, data, orderMaster.OrderTemplate);
            }

        }

        public string Print(string orderNo)
        {
            string orderTemplate = string.Empty;
            OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);
            IList<OrderDetail> orderDetails = base.genericMgr.FindAll<OrderDetail>("select od from OrderDetail as od where od.OrderNo=?", orderNo);
            orderMaster.OrderDetails = orderDetails;
            PrintOrderMaster printOrderMstr = Mapper.Map<OrderMaster, PrintOrderMaster>(orderMaster);
            IList<object> data = new List<object>();
            data.Add(printOrderMstr);
            data.Add(printOrderMstr.OrderDetails);
            if (orderMaster.Type == Sconit.CodeMaster.OrderType.Transfer && orderMaster.OrderStrategy == Sconit.CodeMaster.FlowStrategy.Manual)
            {
                orderTemplate = "ORD_Transfer.xls";
            }
            else if (orderMaster.Type == Sconit.CodeMaster.OrderType.Procurement && orderMaster.OrderStrategy == Sconit.CodeMaster.FlowStrategy.Manual)
            {
                orderTemplate = "ORD_Procurement.xls";
            }
            else
            {
                orderTemplate = orderMaster.OrderTemplate;
            }
            string reportFileUrl = reportGen.WriteToFile(orderTemplate, data);
            //reportGen.WriteToClient(orderMaster.OrderTemplate, data, orderMaster.OrderTemplate);

            return reportFileUrl;
            //reportGen.WriteToFile(orderMaster.OrderTemplate, data);
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
                                    OrderedQty =  tak[36]!=null && (decimal?)tak[36]==1?(decimal)tak[37]:(decimal)tak[11],
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
                                    OrderPriorityDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.OrderPriority, int.Parse((tak[26]).ToString())),
                                    ContainerDescription = (string)tak[27],
                                    MinUnitCount = (decimal)tak[28],
                                    ReserveNo = (string)tak[29],
                                    BinTo = (string)tak[31],
                                    ZOPWZ = (string)tak[34],
                                    CreateOrderCode = (string)tak[35],
                                    ReceiveLotSize = (decimal?)tak[36],
                                    UnitPrice = (decimal?)tak[37],
                                }).ToList();
                #endregion
            }
            ExportToXLS<OrderDetail>("ProcurementDetailXls", "xls", orderDetList);
            // IList<PrintOrderDetail> printOrderetails = Mapper.Map<IList<OrderDetail>, IList<PrintOrderDetail>>(orderDetList);
            //IList<object> data = new List<object>();
            //data.Add(orderDetList);
            //reportGen.WriteToClient("LogisticOrderDetView.xls", data, "LogisticOrderDetView.xls");
        }
        #endregion

        #endregion

        #region 打印配送标签
        //public string PrintDistributeLabel(string orderNo)
        //{
        //    IList<Hu> huList = new List<Hu>();
        //    OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);
        //    IList<OrderDetail> orderDetails = base.genericMgr.FindAll<OrderDetail>("select od from OrderDetail as od where od.OrderNo=?", orderNo);

        //    huList = huMgr.MatchNewHuForRepack(orderDetails, orderMaster.OrderStrategy == Sconit.CodeMaster.FlowStrategy.JIT ? true : false);

        //    IList<PrintOrderDetail> printOrderetails= Mapper.Map<IList<OrderDetail>,IList<PrintOrderDetail>>(orderDetails);
        //    IList<object> data = new List<object>();
        //    data.Add(printOrderetails);
        //    string reportFileUrl = reportGen.WriteToFile("DistributeLabel.xls", data);
        //    return reportFileUrl;
        //}
        #endregion

        #region return order
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_ReturnIndex")]
        public ActionResult ReturnIndex()
        {
            return View();
        }

        #region return edit

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_ReturnIndex")]
        public ActionResult ReturnEdit(string orderNo)
        {
            OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);

            ViewBag.isEditable = orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.Create;
            ViewBag.editorTemplate = orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.Create ? "" : "ReadonlyTextBox";

            return View(orderMaster);
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_ReturnIndex")]
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
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_ReturnIndex")]
        public ActionResult _ReturnAjaxList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<OrderMaster>()));
            }
            //string whereStatement = " where o.Type in (" + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.Procurement
            //            + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + ")"
            //            + " and o.SubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Return;
            //SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel, whereStatement, true);
            //return PartialView(GetAjaxPageData<OrderMaster>(searchStatementModel, command));
            string whereStatement = string.Empty;
            //string whereStatement = "and o.OrderStrategy != " + (int)com.Sconit.CodeMaster.FlowStrategy.SEQ;
            searchModel.SubType = (int)com.Sconit.CodeMaster.OrderSubType.Return;
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchStatement_1(command, searchModel, whereStatement, true);
            return PartialView(GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command));
        }
        #endregion

        #endregion

        #region private method
        private IList<OrderDetail> TransformFlowDetailList2OrderDetailList(string flow, com.Sconit.CodeMaster.OrderSubType orderSubType,string items)
        {

            IList<FlowDetail> flowDetailList = flowMgr.GetFlowDetailList(flow, false, true);

            if (!string.IsNullOrWhiteSpace(items))
            {
                 items = items.Replace("\r\n", ",");
                items = items.Replace("\n", ",");
                string[] itemArr = items.Split(',');
               
                flowDetailList = (from c in flowDetailList
                                  where (from o in itemArr
                                          select o).Contains(c.Item)
                                  select c).ToList();
            }
            var flowMaster = this.genericMgr.FindById<FlowMaster>(flow);
            IList<OrderDetail> orderDetailList = new List<OrderDetail>();
            foreach (FlowDetail flowDetail in flowDetailList)
            {
                OrderDetail orderDetail = new OrderDetail();
                Mapper.Map<FlowDetail, OrderDetail>(flowDetail, orderDetail);
                if (orderSubType == com.Sconit.CodeMaster.OrderSubType.Return)
                {
                    orderDetail.LocationFrom = flowMaster.LocationTo;
                    orderDetail.LocationTo = flowMaster.LocationFrom;
                }
                else {
                    orderDetail.LocationFrom = flowMaster.LocationFrom;
                    orderDetail.LocationTo = flowMaster.LocationTo;
                }
                orderDetail.Id = flowDetail.Id;
                orderDetail.ItemDescription = base.genericMgr.FindById<Item>(flowDetail.Item).Description;
                orderDetailList.Add(orderDetail);
            }
            return orderDetailList;
        }


        private ProcedureSearchStatementModel PrepareSearchStatement_1(GridCommand command, OrderMasterSearchModel searchModel, string whereStatement, bool isReturn)
        {
            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.OrderNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Flow, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter
            {
                Parameter = (int)com.Sconit.CodeMaster.OrderType.Procurement+ "," +(int)com.Sconit.CodeMaster.OrderType.Transfer,
                Type = NHibernate.NHibernateUtil.String
            });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.SubType, Type = NHibernate.NHibernateUtil.Int16 });
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
                else if (command.SortDescriptors[0].Member == "ProductLineFacility")
                {
                    command.SortDescriptors[0].Member = "ProdLineFact";
                }
                else if (command.SortDescriptors[0].Member == "ReferenceOrderNo")
                {
                    command.SortDescriptors[0].Member = "RefOrderNo";
                }
                else if (command.SortDescriptors[0].Member == "ExternalOrderNo")
                {
                    command.SortDescriptors[0].Member = "ExtOrderNo";
                }
                else if (command.SortDescriptors[0].Member == "EffectiveDate")
                {
                    command.SortDescriptors[0].Member = "EffDate";
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

        private SearchStatementModel PrepareSearchStatement(GridCommand command, OrderMasterSearchModel searchModel, string whereStatement, bool isReturn)
        {

            IList<object> param = new List<object>();
            if (isReturn)
            {
                //SecurityHelper.AddPartyFromPermissionStatement(ref whereStatement, "o", "PartyTo", com.Sconit.CodeMaster.OrderType.Procurement, false);
                //SecurityHelper.AddPartyToPermissionStatement(ref whereStatement, "o", "PartyFrom", com.Sconit.CodeMaster.OrderType.Procurement);
                SecurityHelper.AddPartyFromAndPartyToPermissionStatement(ref whereStatement, "o", "Type", "o", "PartyTo", "o", "PartyFrom", com.Sconit.CodeMaster.OrderType.Procurement, false);
            }
            else
            {
                //SecurityHelper.AddPartyFromPermissionStatement(ref whereStatement, "o", "PartyFrom", com.Sconit.CodeMaster.OrderType.Procurement, false);
                //SecurityHelper.AddPartyToPermissionStatement(ref whereStatement, "o", "PartyTo", com.Sconit.CodeMaster.OrderType.Procurement);
                SecurityHelper.AddPartyFromAndPartyToPermissionStatement(ref whereStatement, "o", "Type", "o", "PartyFrom", "o", "PartyTo", com.Sconit.CodeMaster.OrderType.Procurement, false);
            }
            HqlStatementHelper.AddLikeStatement("WMSNo", searchModel.WMSNO, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Sequence", searchModel.Sequence, "o", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("ReferenceOrderNo", searchModel.ReferenceOrderNo, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("ExternalOrderNo", searchModel.ExternalOrderNo, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("OrderNo", searchModel.OrderNo, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("TraceCode", searchModel.TraceCode, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);

            HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "o", ref whereStatement, param);


            if (!string.IsNullOrEmpty(searchModel.PartyFrom))
            {
                HqlStatementHelper.AddEqStatement("PartyFrom", searchModel.PartyFrom, "o", ref whereStatement, param);
            }
            if (!string.IsNullOrEmpty(searchModel.PartyTo))
            {
                HqlStatementHelper.AddEqStatement("PartyTo", searchModel.PartyTo, "o", ref whereStatement, param);
            }
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

        private ProcedureSearchStatementModel PrepareReceiveSearchStatement_1(GridCommand command, OrderMasterSearchModel searchModel)
        {

            string whereStatement = " and o.IsRecScanHu = 0 and o.Status in (" + (int)com.Sconit.CodeMaster.OrderStatus.InProcess + "," + (int)com.Sconit.CodeMaster.OrderStatus.Submit + ")"
                                    + " and o.SubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal
                                    + " and exists (select 1 from OrderDetail as d where d.RecQty < d.OrderQty and d.OrderNo = o.OrderNo) ";
            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.OrderNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Flow, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter
            {
                Parameter = (int)com.Sconit.CodeMaster.OrderType.Procurement + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer
                    + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + "," + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract + "," + (int)com.Sconit.CodeMaster.OrderType.ScheduleLine,
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

        private SearchStatementModel PrepareReceiveSearchStatement(GridCommand command, OrderMasterSearchModel searchModel)
        {

            string whereStatement = " where o.Type in (" + (int)com.Sconit.CodeMaster.OrderType.Procurement + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + "," + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract + ")"
                                    + " and o.IsReceiveScanHu = 0 and o.Status in (" + (int)com.Sconit.CodeMaster.OrderStatus.InProcess + "," + (int)com.Sconit.CodeMaster.OrderStatus.Submit + ")"
                                    + " and o.SubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal
                                    + " and exists (select 1 from OrderDetail as d where d.ReceivedQty < d.OrderedQty and d.OrderNo = o.OrderNo) ";
            IList<object> param = new List<object>();

            //SecurityHelper.AddPartyFromPermissionStatement(ref whereStatement, "o", "PartyFrom", com.Sconit.CodeMaster.OrderType.Procurement, false);
            //SecurityHelper.AddPartyToPermissionStatement(ref whereStatement, "o", "PartyTo", com.Sconit.CodeMaster.OrderType.Procurement);
            SecurityHelper.AddPartyFromAndPartyToPermissionStatement(ref whereStatement, "o", "Type", "o", "PartyFrom", "o", "PartyTo", com.Sconit.CodeMaster.OrderType.Procurement, false);

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
            if (string.IsNullOrEmpty(searchModel.PartyTo))
            {
                HqlStatementHelper.AddEqStatement("PartyTo", searchModel.PartyTo, "o", ref whereStatement, param);
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

        private ProcedureSearchStatementModel PrepareSearchDetailStatement(GridCommand command, OrderMasterSearchModel searchModel,string whereStatement)
        {
           
            //if (!string.IsNullOrWhiteSpace(searchModel.Picker))
            //{
            //    //var pickRules = this.genericMgr.FindAll<PickRule>(" select pr from PickRule as pr where pr.Picker=?  ", searchModel.Picker);
            //    //string items = string.Join("'',''", pickRules.Select(c => c.Item));
            //    //whereStatement += string.Format(" and d.Item in ( ''" + items + "'' ) ", searchModel.Picker);
            //   // whereStatement += string.Format(" and exists( select 1 from MD_PickRule as pr where pr.Item=d.Item and pr.Picker=''{0}'' ) ", searchModel.Picker);
            //}
            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.OrderNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Flow, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter
            {
                Parameter = (int)com.Sconit.CodeMaster.OrderType.Procurement+ "," +(int)com.Sconit.CodeMaster.OrderType.Transfer,
                Type = NHibernate.NHibernateUtil.String
            });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.SubType, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.PartyFrom, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.PartyTo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Status, Type = NHibernate.NHibernateUtil.Int16 });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Priority, Type = NHibernate.NHibernateUtil.Int16 });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.ExternalOrderNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.ReferenceOrderNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = string.Empty, Type = NHibernate.NHibernateUtil.String });
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
            pageParaList.Add(new ProcedureParameter { Parameter = command.SortDescriptors.Count > 0 ? command.SortDescriptors[0].Member : "MastWinTime", Type = NHibernate.NHibernateUtil.String });
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

        #region Nondelivery 要货未到信息
        [SconitAuthorize(Permissions = "Url_Nondelivery_Procurement_View")]
        public ActionResult NondeliveryIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Nondelivery_Procurement_View")]
        public ActionResult NondeliveryList(GridCommand command, OrderMasterSearchModel searchModel)
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
        public ActionResult _AjaxNondeliveryList(GridCommand command, OrderMasterSearchModel searchModel)
        {

            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<OrderDetail>()));
            }
            SearchStatementModel searchStatementModel = PrepareNondeliverySearchStatement(command, searchModel);
            GridModel<OrderDetail> gridList = GetAjaxPageData<OrderDetail>(searchStatementModel, command);
            if (gridList.Data != null && gridList.Data.Count() > 0)
            {
                string[] orderNoArray = gridList.Data.Select(o => o.OrderNo).ToArray();
                string hql = string.Empty;
                IList<object> param = new List<object>();
                foreach (string orderNo in orderNoArray)
                {
                    if (string.IsNullOrEmpty(hql))
                    {
                        hql = "select o from  OrderMaster as o where o.OrderNo in (?";

                    }
                    else
                    {
                        hql += ",?";
                    }
                    param.Add(orderNo);
                }
                hql += ")";
                IList<OrderMaster> OrderMstrList = base.genericMgr.FindAll<OrderMaster>(hql, param.ToArray());
                if (OrderMstrList != null && OrderMstrList.Count > 0)
                {
                    foreach (OrderMaster orM in OrderMstrList)
                    {
                        foreach (OrderDetail orD in gridList.Data)
                        {
                            if (orD.OrderNo == orM.OrderNo)
                            {
                                orD.WindowTime = orM.WindowTime;
                                orD.PartyFromName = orM.PartyFromName + "[" + orM.PartyFrom + "]";
                                orD.PartyToName = orM.PartyToName + "[" + orM.PartyTo + "]";
                            }
                            else
                            {
                                continue;
                            }
                        }

                    }
                }
            }
            return PartialView(gridList);
        }

        private SearchStatementModel PrepareNondeliverySearchStatement(GridCommand command, OrderMasterSearchModel searchModel)
        {
            StringBuilder sb = new StringBuilder();
            string whereStatement = "where exists(select 1 from OrderMaster as o where o.OrderNo=d.OrderNo and o.Type in (" + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.Procurement
                       + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + "," + (int)com.Sconit.CodeMaster.OrderType.ScheduleLine + ")"
                       + " and o.SubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal;
            sb.Append(whereStatement);
            if (searchModel.DateFrom != null & searchModel.DateTo != null)
            {
                sb.Append(string.Format(" and o.WindowTime between '{0}' and '{1}'", searchModel.DateFrom, searchModel.DateTo));

            }
            else if (searchModel.DateFrom != null & searchModel.DateTo == null)
            {
                sb.Append(string.Format(" and o.WindowTime >= '{0}'", searchModel.DateFrom));

            }
            else if (searchModel.DateFrom == null & searchModel.DateTo != null)
            {
                sb.Append(string.Format(" and o.WindowTime <= '{0}'", searchModel.DateTo));

            }
            if (!string.IsNullOrEmpty(searchModel.Flow))
            {
                sb.Append(string.Format(" and o.Flow = '{0}'", searchModel.Flow));
            }
            if (!string.IsNullOrEmpty(searchModel.PartyFrom))
            {
                sb.Append(string.Format(" and o.PartyFrom = '{0}'", searchModel.PartyFrom));
            }
            if (!string.IsNullOrEmpty(searchModel.PartyTo))
            {
                sb.Append(string.Format(" and o.PartyTo = '{0}'", searchModel.PartyTo));
            }
            if (searchModel.LocationTo != null)
            {
                sb.Append(string.Format(" and o.LocationTo = '{0}'", searchModel.LocationTo));
            }
            sb.Append(")");
            whereStatement = sb.ToString();
            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("OrderNo", searchModel.OrderNo, HqlStatementHelper.LikeMatchMode.Start, "d", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "d", ref whereStatement, param);

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " and d.ReceivedQty<d.OrderedQty order by d.CreateDate desc";
            }

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = "select count(*) from OrderDetail as d";
            searchStatementModel.SelectStatement = "select d from OrderDetail as d";
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }
        #endregion


        #region 退货单 发货
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Ship")]
        public ActionResult ShipIndex()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Ship")]
        public ActionResult Ship(GridCommand command, OrderMasterSearchModel searchModel)
        {
            this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Ship")]
        public ActionResult _AjaxShipOrderList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            searchModel.SubType = (int)Sconit.CodeMaster.OrderSubType.Return;
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareShipSearchStatement_1(command, searchModel);
            return PartialView(GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Ship")]
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
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Ship")]
        public ActionResult ShipEdit(string checkedOrders)
        {
            ViewBag.CheckedOrders = checkedOrders;
            string[] checkedOrderArray = checkedOrders.Split(',');
            OrderMaster order = base.genericMgr.FindById<OrderMaster>(checkedOrderArray[0]);
            return View(order);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Ship")]
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
                        if (Convert.ToDecimal(qtyArray[i]) > 0)
                        {
                            OrderDetail od = base.genericMgr.FindById<OrderDetail>(Convert.ToInt32(idArray[i]));

                            OrderDetailInput input = new OrderDetailInput();
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

        private ProcedureSearchStatementModel PrepareShipSearchStatement_1(GridCommand command, OrderMasterSearchModel searchModel)
        {
            string whereStatement = " and o.IsShipScanHu = 0 and o.IsShipByOrder = 1 and o.Status in (" + (int)com.Sconit.CodeMaster.OrderStatus.Submit + "," + (int)com.Sconit.CodeMaster.OrderStatus.InProcess + ")"
                                    + " and exists (select 1 from OrderDetail as d where d.ShipQty < d.OrderQty and d.OrderNo = o.OrderNo) ";
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
                Parameter = (int)com.Sconit.CodeMaster.OrderType.Procurement + "," + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," +
                            (int)com.Sconit.CodeMaster.OrderType.ScheduleLine + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract + "," +
                             (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer,
                Type = NHibernate.NHibernateUtil.String
            });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.SubType, Type = NHibernate.NHibernateUtil.Int16 });
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
        #endregion

        [SconitAuthorize(Permissions = "Url_OrderDetail_Procurement_Return")]
        public ActionResult ReturnDetailList(GridCommand command, OrderMasterSearchModel searchModel)
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
        [SconitAuthorize(Permissions = "Url_OrderDetail_Procurement_Return")]
        public ActionResult _AjaxReturnOrderDetailList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<OrderDetail>()));
            }
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchReturnDetailStatement(command, searchModel);
            return PartialView(GetAjaxPageDataProcedure<OrderDetail>(procedureSearchStatementModel, command));
        }


        private ProcedureSearchStatementModel PrepareSearchReturnDetailStatement(GridCommand command, OrderMasterSearchModel searchModel)
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
                else if (command.SortDescriptors[0].Member == "Sequence")
                {
                    command.SortDescriptors[0].Member = "Seq";
                }
                else if (command.SortDescriptors[0].Member == "ItemDescription")
                {
                    command.SortDescriptors[0].Member = "ItemDesc";
                }
                else if (command.SortDescriptors[0].Member == "ReferenceItemCode")
                {
                    command.SortDescriptors[0].Member = "RefItemCode";
                }
                else if (command.SortDescriptors[0].Member == "UnitCountDescription")
                {
                    command.SortDescriptors[0].Member = "UCDesc";
                }
                else if (command.SortDescriptors[0].Member == "MinUnitCount")
                {
                    command.SortDescriptors[0].Member = "MinUC";
                }
                else if (command.SortDescriptors[0].Member == "UnitCount")
                {
                    command.SortDescriptors[0].Member = "UC";
                }
                else if (command.SortDescriptors[0].Member == "RequiredQty")
                {
                    command.SortDescriptors[0].Member = "ReqQty";
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
                else if (command.SortDescriptors[0].Member == "ReceiveLotSize")
                {
                    command.SortDescriptors[0].Member = "RecLotSize";
                }
                else if (command.SortDescriptors[0].Member == "LocationTo")
                {
                    command.SortDescriptors[0].Member = "LocTo";
                }
                else if (command.SortDescriptors[0].Member == "ContainerDescription")
                {
                    command.SortDescriptors[0].Member = "ContainerDesc";
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

        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Import")]
        public ActionResult ImportProcurementOrder(IEnumerable<HttpPostedFileBase> attachments,
          string Flow, DateTime? StartTime, DateTime? WindowTime, string EffectiveDate,
          string ReferenceOrderNo, string ExternalOrderNo, com.Sconit.CodeMaster.OrderPriority? Priority)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Flow))
                {
                    throw new BusinessException("路线不能为空！");
                }

                if (!StartTime.HasValue)
                {
                    throw new BusinessException("开始时间不能为空！");

                }

                if (!WindowTime.HasValue)
                {
                    throw new BusinessException("窗口时间不能为空！");
                }
                foreach (var file in attachments)
                {
                    string orderNo = orderMgr.CreateProcurementOrderFromXls(file.InputStream, Flow, ExternalOrderNo, ReferenceOrderNo,
                           StartTime.Value, WindowTime.Value, Priority.Value);

                    SaveSuccessMessage("要货单：" + orderNo + "生成成功！");
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


        #region 路线明细导入

        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Import")]
        public ActionResult Import()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_FlowDetailImport_View")]
        public ActionResult FlowDetailImportIndex()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_FlowDetailImport_View")]
        public ActionResult ImportFlowDetail(IEnumerable<HttpPostedFileBase> attachments)
        {
            try
            {
                foreach (var file in attachments)
                {
                    flowMgr.CreateFlowDetailXls(file.InputStream);
                    SaveSuccessMessage("路线明细成功！");
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


        #region  新的收货菜单
        [SconitAuthorize(Permissions = "Url_Procurement_MergeReceive")]
        public ActionResult MergeIndex()
        {
            return View();
        }

        public JsonResult CheckOrderType(string orderNo)
        {
            try
            {
                string type = "ORD";
                int orderType = 1;
                bool isTrue = true;
                if (orderNo.Substring(0, 1).ToUpper() == "D")
                {
                    IList<OrderMaster> orderMasterList = this.genericMgr.FindAll<OrderMaster>(@" select o from OrderMaster as o where   o.SubType =0 
                and o.IsReceiveScanHu = 0 and o.PartyFrom not in ('SQC','LOC') and o.Status in (1,2) and exists(select 1 from OrderDetail as d where d.OrderNo=o.OrderNo and  d.ReceivedQty < d.OrderedQty) and o.OrderNo=? ",orderNo);
                    if (orderMasterList == null || orderMasterList.Count == 0)
                    {
                        isTrue = false;
                        throw new BusinessException(string.Format("订单号{0}不是收货状态，或者没有要收货的明细。", orderNo));
                    }
                    else
                    {
                        if (orderMasterList.First().OrderStrategy == com.Sconit.CodeMaster.FlowStrategy.SEQ)
                        {
                            type = "SEQ";
                        }
                        else
                        {
                            type = "ORD";
                        }
                        orderType = (int)orderMasterList.First().Type;
                    }
                }
                else if (orderNo.Substring(0, 1) == "A")
                {
                    string whereStatement = " select i from IpMaster as i where i.IpNo=?   and i.IsReceiveScanHu=0 and i.Status in(0,1) and exists (select 1 from IpDetail as d where d.IsClose = 0 and d.Type = 0 and d.IpNo = i.IpNo)";
                    IList<IpMaster> ipMasterList = this.genericMgr.FindAll<IpMaster>(whereStatement, orderNo);
                    if (ipMasterList == null || ipMasterList.Count == 0)
                    {
                        isTrue = false;
                        throw new BusinessException(string.Format("ASN号{0}不是收货状态，或者没有要收货的明细。", orderNo));
                    }
                    else
                    {
                        type = "ASN";
                        orderType = (int)ipMasterList.First().OrderType;
                    }
                }
                else
                {
                    string whereStatement = "select COUNT(*) as checkCount from FIS_WMSDatFile where WmsNo =?";
                    isTrue = this.genericMgr.FindAllWithNativeSql<int>(whereStatement, orderNo)[0]>0;
                    if (isTrue)
                    {
                        type = "WMS";
                        //throw new BusinessException(string.Format("订单号{0}不是要货单，也不是ASN不能收货。", orderNo));
                    }
                    else
                    {
                        throw new BusinessException(string.Format("单号{0}没有找到要收货的记录，请确认。", orderNo));
                    }
                }
                return Json(new { Type = type, IsTrue = isTrue, OrderType = orderType });
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex.Message);
            }
            return Json(null);
        }


        #region  要货单收货
        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Receive")]
        public ActionResult _MergeOrderDetail(GridCommand command, string orderNo,string orderType)
        {
            if (string.IsNullOrWhiteSpace(orderNo))
            {
                SaveWarningMessage("单号不能为空。");
            }
            ViewBag.IsTransfer = orderType == "2";
            ViewBag.IsProcurement = orderType == "1";
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxMergeOrderDetailList(GridCommand command, string orderNo)
        {
            IList<OrderDetail> orderDetails = this.genericMgr.FindAll<OrderDetail>(" select d from OrderDetail as d where d.OrderNo=? and d.ReceivedQty < d.OrderedQty ", orderNo);
            GridModel<OrderDetail> returnGrid = new GridModel<OrderDetail>();
            returnGrid.Total = orderDetails.Count();
            returnGrid.Data = orderDetails.Skip((command.Page - 1) * command.PageSize).Take(command.PageSize);
            return PartialView(returnGrid);
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Receive")]
        public JsonResult MergeReceiveOrderDetail(string idStr, string qtyStr, string csSupplierStr)
        {
            try
            {
                IList<OrderDetail> orderDetailList = new List<OrderDetail>();
                if (!string.IsNullOrEmpty(idStr))
                {
                    string[] idArray = idStr.Split(',');
                    string[] qtyArray = qtyStr.Split(',');
                    string[] csSupplierArr = csSupplierStr.Split(',');

                    for (int i = 0; i < idArray.Count(); i++)
                    {
                        if (Convert.ToDecimal(qtyArray[i]) > 0)
                        {
                            OrderDetail od = base.genericMgr.FindById<OrderDetail>(Convert.ToInt32(idArray[i]));
                            bool isColse = this.genericMgr.FindAllWithNativeSql<int>("select COUNT(*) as sumCount from LOG_SeqOrderChange where Status=4 and OrderDetId=?", od.Id)[0] > 0;
                            if (isColse)
                            {
                                throw new BusinessException(string.Format("单号{0}中物料{1}明细行已经关闭，不能收货。",od.OrderNo,od.Item));
                            }
                            OrderDetailInput input = new OrderDetailInput();
                            input.ReceiveQty = Convert.ToDecimal(qtyArray[i]);
                            if (!string.IsNullOrWhiteSpace(csSupplierArr[i]))
                            {
                                input.ConsignmentParty = csSupplierArr[i];
                            }
                            od.AddOrderDetailInput(input);
                            orderDetailList.Add(od);
                        }
                    }
                }
                if (orderDetailList.Count() == 0)
                {
                    throw new BusinessException("收货明细不能为空");
                }

                orderMgr.ReceiveOrder(orderDetailList);
                SaveSuccessMessage("收货成功。");
                return Json(new {  });

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

        #region  排序单收货
        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Receive")]
        public ActionResult _MergeSEQOrderDetail(GridCommand command, string orderNo, string orderType)
        {
            if (string.IsNullOrWhiteSpace(orderNo))
            {
                SaveWarningMessage("单号不能为空。");
            }
            ViewBag.IsTransfer = orderType == "2";
            ViewBag.IsProcurement = orderType == "1";
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxMergeSEQOrderDetailList(GridCommand command, string orderNo)
        {
            IList<OrderDetail> orderDetails = this.genericMgr.FindAll<OrderDetail>(" select d from OrderDetail as d where d.OrderNo=? and d.ReceivedQty < d.OrderedQty ", orderNo);
            GridModel<OrderDetail> returnGrid = new GridModel<OrderDetail>();
            returnGrid.Total = orderDetails.Count();
            returnGrid.Data = orderDetails.Skip((command.Page - 1) * command.PageSize).Take(command.PageSize);
            return PartialView(returnGrid);
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Receive")]
        public JsonResult MergeSEQReceiveOrderDetail(string idStr, string qtyStr, string csSupplierStr)
        {
            try
            {
                IList<OrderDetail> orderDetailList = new List<OrderDetail>();
                if (!string.IsNullOrEmpty(idStr))
                {
                    string[] idArray = idStr.Split(',');
                    string[] qtyArray = qtyStr.Split(',');
                    string[] csSupplierArr = csSupplierStr.Split(',');

                    for (int i = 0; i < idArray.Count(); i++)
                    {
                        if (Convert.ToDecimal(qtyArray[i]) > 0)
                        {
                            OrderDetail od = base.genericMgr.FindById<OrderDetail>(Convert.ToInt32(idArray[i]));
                            //if (od.OrderedQty == od.ReceivedQty && od.UnitPrice > od.OrderedQty)
                            //{
                            //    throw new BusinessException(string.Format("单号{0}中物料{1}明细行已经关闭，不能收货。", od.OrderNo, od.Item));
                            //}
                            OrderDetailInput input = new OrderDetailInput();
                            input.ReceiveQty = Convert.ToDecimal(qtyArray[i]);
                            if (!string.IsNullOrWhiteSpace(csSupplierArr[i]))
                            {
                                input.ConsignmentParty = csSupplierArr[i];
                            }
                            od.AddOrderDetailInput(input);
                            orderDetailList.Add(od);
                        }
                    }
                }
                if (orderDetailList.Count() == 0)
                {
                    throw new BusinessException("收货明细不能为空");
                }

                orderMgr.ReceiveOrder(orderDetailList);
                SaveSuccessMessage("收货成功。");
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
        #endregion

        #region  送货单收货
        [GridAction]
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_Receive")]
        public ActionResult _MergeASNOrderDetail(GridCommand command, string orderNo, string orderType)
        {
            if (string.IsNullOrWhiteSpace(orderNo))
            {
                SaveWarningMessage("单号不能为空。");
            }
            ViewBag.IsTransfer = orderType == "2";
            ViewBag.IsProcurement = orderType == "1";
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxMergeASNDetailList(GridCommand command, string orderNo)
        {
            string searchSql = "select i from IpDetail as i where i.IsClose = ? and i.Type = ? and i.IpNo = ? and i.ReceivedQty=0";
            IList<IpDetail> ipDetailList = base.genericMgr.FindAll<IpDetail>(searchSql, new object[] { false, (int)com.Sconit.CodeMaster.IpDetailType.Normal, orderNo });
            GridModel<IpDetail> returnGrid = new GridModel<IpDetail>();
            returnGrid.Total = ipDetailList.Count();
            returnGrid.Data = ipDetailList.Skip((command.Page - 1) * command.PageSize).Take(command.PageSize);
            return PartialView(returnGrid);
        }

        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_Receive")]
        public JsonResult MergeReceiveASNOrderDetail(string idStr, string qtyStr, string csSupplierStr)
        {
            try
            {
                IList<IpDetail> ipDetailList = new List<IpDetail>();
                if (!string.IsNullOrEmpty(idStr))
                {
                    string[] idArray = idStr.Split(',');
                    string[] qtyArray = qtyStr.Split(',');
                    string[] csSupplierArr = csSupplierStr.Split(',');

                    for (int i = 0; i < idArray.Count(); i++)
                    {
                        if (Convert.ToDecimal(qtyArray[i]) > 0)
                        {
                            IpDetail ipDetail = base.genericMgr.FindById<IpDetail>(Convert.ToInt32(idArray[i]));
                            IpDetailInput input = new IpDetailInput();
                            input.ReceiveQty = Convert.ToDecimal(qtyArray[i]);
                            if (!string.IsNullOrWhiteSpace(csSupplierArr[i]))
                            {
                                input.ConsignmentParty = csSupplierArr[i];
                            }
                            ipDetail.AddIpDetailInput(input);
                            ipDetailList.Add(ipDetail);
                        }
                    }
                }
                if (ipDetailList.Count() == 0)
                {
                    throw new BusinessException("收货明细不能为空");
                }

                orderMgr.ReceiveIp(ipDetailList, false, DateTime.Now);
                SaveSuccessMessage(Resources.ORD.IpMaster.IpMaster_Received, ipDetailList[0].IpNo);
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
        #endregion

        #region  安吉配送收货
        [GridAction]
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_ReceiveWMS")]
        public ActionResult _MergeWMSDat(GridCommand command, string orderNo, string orderType)
        {
            ViewBag.WmsNo = orderNo;
            if (string.IsNullOrWhiteSpace(orderNo))
            {
                SaveWarningMessage("单号不能为空。");
            }
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxMergeWMSDatList(GridCommand command, string orderNo)
        {
            if (string.IsNullOrWhiteSpace(orderNo))
            {
                return PartialView(new GridModel<WMSDatFile>(new List<WMSDatFile>()));
            }

            IList<WMSDatFile> wMSDatFileList = this.genericMgr.FindEntityWithNativeSql<WMSDatFile>(" select * from FIS_WMSDatFile where WmsNo=? and ReceiveTotal<(CancelQty+Qty) ", orderNo);

            #region 冲销的相互抵消
            foreach (WMSDatFile wMSDatFile in wMSDatFileList)
            {
                if (wMSDatFile.MoveType == null)
                {
                    continue;
                }
                foreach (WMSDatFile wmsFile in wMSDatFileList)
                {
                    if (wmsFile.MoveType == null)
                    {
                        continue;
                    }
                    if (wMSDatFile.MoveType + wMSDatFile.SOBKZ == "311" && wmsFile.MoveType + wmsFile.SOBKZ == "312" && wmsFile.Qty == wMSDatFile.Qty && wmsFile.WmsLine == wMSDatFile.WmsLine)
                    {
                        wmsFile.MoveType = null;
                        wMSDatFile.MoveType = null;
                        break;
                    }
                    else if (wMSDatFile.MoveType + wMSDatFile.SOBKZ == "311K" && wmsFile.MoveType + wmsFile.SOBKZ == "312K" && wmsFile.Qty == wMSDatFile.Qty && wmsFile.WmsLine == wMSDatFile.WmsLine)
                    {
                        wmsFile.MoveType = null;
                        wMSDatFile.MoveType = null;
                        break;
                    }

                    else if (wMSDatFile.MoveType + wMSDatFile.SOBKZ == "411" && wmsFile.MoveType + wMSDatFile.SOBKZ == "412" && wmsFile.Qty == wMSDatFile.Qty && wmsFile.WmsLine == wMSDatFile.WmsLine)
                    {
                        wmsFile.MoveType = null;
                        wMSDatFile.MoveType = null;
                        break;
                    }
                    else if (wMSDatFile.MoveType + wMSDatFile.SOBKZ == "411K" && wmsFile.MoveType + wMSDatFile.SOBKZ == "412K" && wmsFile.Qty == wMSDatFile.Qty && wmsFile.WmsLine == wMSDatFile.WmsLine)
                    {
                        wmsFile.MoveType = null;
                        wMSDatFile.MoveType = null;
                        break;
                    }
                }
            }
            #endregion


            //默认按LES订单id号排序
            IEnumerable<WMSDatFile> wmsList = wMSDatFileList.Where(o => o.MoveType != null && o.MoveType != "312" && o.MoveType != "412").OrderBy(o => o.WmsLine);
            foreach (WMSDatFile wmsDatFile in wmsList)
            {
                OrderDetail ordDet = base.genericMgr.FindById<OrderDetail>(int.Parse(wmsDatFile.WmsLine));
                wmsDatFile.OrderNo = ordDet.OrderNo;
                wmsDatFile.OrderQty = ordDet.OrderedQty;
                wmsDatFile.ReferenceItemCode = ordDet.ReferenceItemCode;
                wmsDatFile.ItemDescription = ordDet.ItemDescription;
                wmsDatFile.LocationTo = ordDet.LocationTo;
            }
            GridModel<WMSDatFile> returnGrid = new GridModel<WMSDatFile>();
            returnGrid.Total = wmsList.Count();
            returnGrid.Data = wmsList.Skip((command.Page - 1) * command.PageSize).Take(command.PageSize);
            return PartialView(returnGrid);
        }

        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_ReceiveWMS")]
        public JsonResult MergeReceiveWMSDat(string idStr, string qtyStr, string csSupplierStr)
        {
            try
            {
                if (!string.IsNullOrEmpty(idStr))
                {
                    string[] idArray = idStr.Split(',');
                    string[] qtyArray = qtyStr.Split(',');
                    for (int i = 0; i < idArray.Count(); i++)
                    {
                        try
                        {
                            WMSDatFile wMSDatFile = base.genericMgr.FindById<WMSDatFile>(Convert.ToInt32(idArray[i]));
                            wMSDatFile.CurrentReceiveQty = Convert.ToDecimal(qtyArray[i]);
                            if (wMSDatFile.CurrentReceiveQty + wMSDatFile.ReceiveTotal > (wMSDatFile.Qty + wMSDatFile.CancelQty))
                            {
                                throw new BusinessException(string.Format("物料{0}唯一标识{1}本次收货数{2}大于最大收货数{3}。", wMSDatFile.Item, wMSDatFile.WMSId, wMSDatFile.CurrentReceiveQty, wMSDatFile.Qty + wMSDatFile.CancelQty - wMSDatFile.ReceiveTotal));
                            }
                            orderMgr.ReceiveWMSIpMaster(wMSDatFile);
                            SaveSuccessMessage(string.Format("物料{0}唯一标识{1}收货数{2}收货成功。", wMSDatFile.Item, wMSDatFile.WMSId, wMSDatFile.CurrentReceiveQty));
                        }
                        catch (BusinessException ex)
                        {
                            SaveBusinessExceptionMessage(ex);
                        }
                        catch (Exception ex)
                        {
                            SaveErrorMessage(ex);
                        }
                    }
                }
                else
                {
                    throw new BusinessException("收货明细不能为空。");
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
            return Json(new { });
        }
        #endregion


        //指定寄售供应商
        public JsonResult SpecifySequenceOrderCSSupplier(string orderNo,string action)
        {
            try
            {
                this.genericMgr.UpdateWithNativeQuery("exec USP_LE_SpecifySequenceOrderCSSupplier ?",
                    new object[] { orderNo },
                    new IType[] { NHibernateUtil.String});
                //return new RedirectToRouteResult(new RouteValueDictionary  
                //                                       { 
                //                                           { "action", action }, 
                //                                           { "controller", "ProcurementOrder" },
                //                                           { "orderNo", orderNo }
                //                                       });
                SaveSuccessMessage("操作成功。");
                return Json(new { });
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.InnerException != null)
                    {
                        SaveErrorMessage(ex.InnerException.InnerException.Message);
                    }
                    else
                    {
                        SaveErrorMessage(ex.InnerException.Message);
                    }
                }
                else
                {
                    SaveErrorMessage(ex.Message);
                }
            }
            return Json(null);
        }
        #endregion

        #region 按灯的、EK板、手工的要货需求关闭
        [SconitAuthorize(Permissions = "Url_ProcurementOrder_CloseDetail")]
        public ActionResult CloseDetailIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_ProcurementOrder_CloseDetail")]
        public ActionResult CloseDetailList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxCloseDetail(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (!string.IsNullOrWhiteSpace(searchModel.successMesage))
            {
                SaveSuccessMessage(searchModel.successMesage);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.errorMesage))
            {
                SaveErrorMessage(searchModel.errorMesage);
            }
            string searchSql = PrepareNewDetailStatement(command, searchModel);
            int total = this.genericMgr.FindAllWithNativeSql<int>(string.Format("select count(*) as rowTotal from( {0} ) as t", searchSql)).First();
            IList<object[]> serchResult = this.genericMgr.FindAllWithNativeSql<object[]>(string.Format(" select t.* from ( {0} ) as t where t.RowId between {1} and {2} ", searchSql, (command.Page - 1) * command.PageSize, command.Page * command.PageSize));
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
                                    OrderStrategyDescription = systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.FlowStrategy, Convert.ToInt32((object)tak[17])),
                                    ReceivedQty = (decimal)tak[18],
                                }).ToList();
                #endregion
            }

            gridModelOrderDet.Total = total;
            gridModelOrderDet.Data = orderDetList;
            return PartialView(gridModelOrderDet);
        }


        private string PrepareNewDetailStatement(GridCommand command, OrderMasterSearchModel searchModel)
        {
            //left join VIEW_LocationDet as vl on vl.Location=d.LocFrom and vl.Item=d.Item  isnull(vl.Qty,0) as InvQty
            string sql = @"select d.Id,d.OrderNo,d.Item,d.RefItemCode,d.ItemDesc,d.ManufactureParty,isnull(d.OrderQty,0) as OrderQty,isnull(d.ShipQty,0) as ShipQty ,isnull(rr.qty,0) as OccupyQty, pk.Decs1,d.LocFrom,d.LocTo,d.BinTo,m.WindowTime,d.CreateDate,pr.Picker,d.PickQty,m.OrderStrategy,isnull(d.RecQty,0) as RecQty
                 from ORD_OrderDet_2 as d with(nolock) inner join ORD_OrderMstr_2 as m with(nolock) on d.OrderNo=m.OrderNo 
						                 left join MD_PickRule as pr with(nolock) on pr.Location=d.LocFrom and pr.Item=d.Item
                                         left join MD_Picker as pk with(nolock) on pr.Picker=pk.Code
						                 left join (select pd.LocFrom,pd.Item,sum(pd.Qty) as qty from ORD_PickListDet as pd where exists(select 1 from ORD_PickListMstr as pm where pd.PLNo=pm.PLNo and pm.Status=0 )  group by pd.LocFrom,pd.Item) as rr on rr.LocFrom=d.LocFrom and rr.Item=d.Item  
                                         where m.SubType=0 and (d.ShipPickQty)<d.OrderQty and m.Status in( 1,2 ) and m.OrderStrategy in(0,1,2,7) ";
            sql += string.Format(" and exists ( select 1 from VIEW_UserPermission as p where p.UserId={0} and p.CategoryType={1} and (p.PermissionCode=m.PartyFrom  or p.PermissionCode=m.PartyTo)) ", CurrentUser.Id, (int)com.Sconit.CodeMaster.PermissionCategoryType.Region);
            if (!string.IsNullOrWhiteSpace(searchModel.Picker))
            {
                sql += string.Format(" and pr.Picker='{0}' ", searchModel.Picker);
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
            if (searchModel.DateFrom != null)
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
                orderBysql = " order by " + command.SortDescriptors[0].Member + (command.SortDescriptors[0].SortDirection == ListSortDirection.Descending ? " desc" : " asc");
            }
            return " select result.*,RowId=ROW_NUMBER()OVER(order by Id asc)  from (" + sql + " ) as  result";
        }


        [SconitAuthorize(Permissions = "Url_ProcurementOrder_CloseDetail")]
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
                errorMesage = be.GetMessages()[0].GetMessageString();
            }
            catch (Exception e)
            {
                errorMesage = "要货需求关闭失败，" + e.Message;
            }
            return new RedirectToRouteResult(new RouteValueDictionary  
                                                   { 
                                                       { "action", "CloseDetailList" }, 
                                                       { "controller", "ProcurementOrder" },
                                                       { "successMesage", successMesage},
                                                       { "errorMesage", errorMesage}
                                                   });
        }
        #endregion

    }
}
