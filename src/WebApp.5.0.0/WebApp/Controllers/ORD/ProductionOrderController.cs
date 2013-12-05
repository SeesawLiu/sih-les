using com.Sconit.Entity.VIEW;
using com.Sconit.Web.SAPService;

namespace com.Sconit.Web.Controllers.ORD
{
    using System;
    using System.Text;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using AutoMapper;
    using com.Sconit.Entity;
    using com.Sconit.Entity.Exception;
    using com.Sconit.Entity.INV;
    using com.Sconit.Entity.MD;
    using com.Sconit.Entity.ORD;
    using com.Sconit.Entity.PRD;
    using com.Sconit.Entity.SCM;
    using com.Sconit.Entity.SYS;
    using com.Sconit.PrintModel.INV;
    using com.Sconit.PrintModel.ORD;
    using com.Sconit.Service;
    using com.Sconit.Utility;
    using com.Sconit.Utility.Report;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Models.SearchModels.ORD;
    using com.Sconit.Web.Models.SearchModels.PRD;
    using com.Sconit.Web.Util;
    using NHibernate.Criterion;
    using NPOI.HSSF.UserModel;
    using NPOI.SS.UserModel;
    using Telerik.Web.Mvc;
    using NHibernate.Type;
    using NHibernate;

    public class ProductionOrderController : WebAppBaseController
    {
        #region  hql
        /// <summary>
        /// 
        /// </summary>
        private static string selectCountStatement = "select count(*) from OrderMaster as o";

        /// <summary>
        /// 
        /// </summary>
        private static string selectStatement = "select o from OrderMaster as o";


        private static string selectOrderDetailStatement = "select d from OrderDetail as d where d.OrderNo=?";


        private static string selectOrderOperationStatement = "select d from OrderOperation as d where d.OrderDetailId = ?";

        #endregion

        //private WCFServices.IPublishing proxy;
        public IOrderMgr orderMgr { get; set; }
        public IFlowMgr flowMgr { get; set; }
        public IProductionLineMgr productionLineMgr { get; set; }
        public IBomMgr bomMgr { get; set; }
        public IReportGen reportGen { get; set; }
        public IItemMgr itemMgr { get; set; }


        #region public method
        public ProductionOrderController()
        {
        }

        #region view
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_View")]
        public ActionResult Index()
        {
            return View();
        }


        [SconitAuthorize(Permissions = "Url_OrderDetail_Production")]
        public ActionResult DetailIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_View")]
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
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_View")]
        public ActionResult _AjaxList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<OrderMaster>()));
            }
            string whereStatement = " and o.ProdLineType not in (1,2,3,4,9)";
            if (searchModel.Item != null && searchModel.Item != string.Empty)
                whereStatement += " and exists(select 1 from OrderDetail as d where d.OrderNo=o.OrderNo and d.Item ='" + searchModel.Item + "')";
            whereStatement += " and o.OrderStrategy != " + (int)com.Sconit.CodeMaster.FlowStrategy.SEQ;
            searchModel.SubType = (int)com.Sconit.CodeMaster.OrderSubType.Normal;
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchStatement_1(command, searchModel, whereStatement, false);
            GridModel<OrderMaster> orderGridList = GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command);
            if (orderGridList.Data != null && orderGridList.Data.Count() > 0)
            {
                foreach (var orderMaster in orderGridList.Data)
                {
                    // orderMaster.Item=base.genericMgr.FindAll<OrderDetail>("select d from OrderDetail as d where d.OrderNo=?",orderMaster.OrderNo).First().Item;
                    var orderdetail = base.genericMgr.FindAll<OrderDetail>("select d from OrderDetail as d where d.OrderNo=?", orderMaster.OrderNo).FirstOrDefault();
                    if (orderdetail == null) continue;
                    orderMaster.Item = orderdetail.Item;
                    orderMaster.ReferenceItemCode = this.genericMgr.FindById<Item>(orderdetail.Item).ReferenceCode;
                    orderMaster.OrderedQty = orderdetail.OrderedQty;
                }

            }

            return PartialView(orderGridList);
        }

        public ActionResult ExportXLS(GridCommand command, OrderMasterSearchModel searchModel)
        {
            string whereStatement = " and o.ProdLineType not in (1,2,3,4,9)";
            if (searchModel.Item != null && searchModel.Item != string.Empty)
                whereStatement += " and exists(select 1 from OrderDetail as d where d.OrderNo=o.OrderNo and d.Item ='" + searchModel.Item + "')";
            whereStatement += " and o.OrderStrategy != " + (int)com.Sconit.CodeMaster.FlowStrategy.SEQ;
            searchModel.SubType = (int)com.Sconit.CodeMaster.OrderSubType.Normal;
            command.PageSize = 66500;
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchStatement_1(command, searchModel, whereStatement, false);
            GridModel<OrderMaster> orderGridList = GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command);
            if (orderGridList.Data.Count() > 0)
            {
                foreach (var orderMaster in orderGridList.Data)
                {
                    var orderdetail = base.genericMgr.FindAll<OrderDetail>("select d from OrderDetail as d where d.OrderNo=?", orderMaster.OrderNo).FirstOrDefault();
                    if (orderdetail == null) continue;
                    orderMaster.Item = orderdetail.Item;
                    //orderMaster.ReferenceItemCode =this.genericMgr.FindById<Item>(orderdetail.Item).ReferenceCode;
                    orderMaster.OrderedQty = orderdetail.OrderedQty;
                }
            }
            ExportToXLS<OrderMaster>("ProductionOrder", "XLS", orderGridList.Data.ToList());
            return null;
        }

        public ActionResult ExportVanXLS(GridCommand command, OrderMasterSearchModel searchModel)
        {
            string whereStatement = " and o.ProdLineType in (1,2,3,4,9)";
            if (searchModel.Item != null && searchModel.Item != string.Empty)
                whereStatement += " and exists(select 1 from OrderDetail as d where d.OrderNo=o.OrderNo and d.Item ='" + searchModel.Item + "')";
            whereStatement += " and o.OrderStrategy != " + (int)com.Sconit.CodeMaster.FlowStrategy.SEQ;
            searchModel.SubType = (int)com.Sconit.CodeMaster.OrderSubType.Normal;
            if (searchModel.IsPause.HasValue)
            {
                whereStatement += " and o.PauseStatus =" + searchModel.IsPause.Value;
            }
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareVanSearchStatement_1(command, searchModel, whereStatement, new SortDescriptor { Member = "Seq asc,SubSeq ", SortDirection = ListSortDirection.Ascending }, false);
            GridModel<OrderMaster> list = GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command);

            ExportToXLS<OrderMaster>("VanProductionOrder", "XLS", list.Data.ToList());
            return null;
        }

        #region 明细菜单 报表
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_View")]
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
                // IList<OrderDetail> list = base.genericMgr.FindAll<OrderDetail>(PrepareSearchDetailStatement(command, searchModel)); //GetPageData<OrderDetail>(searchStatementModel, command
                SaveWarningMessage(Resources.ErrorMessage.Errors_NoConditions);
                return View(new List<OrderDetail>());
            }
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_View")]
        public ActionResult _AjaxOrderDetailList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<OrderDetail>()));
            }
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchDetailStatement(command, searchModel);
            // return PartialView(GetAjaxPageDataProcedure<OrderDetail>(procedureSearchStatementModel, command));
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


        #endregion

        #region edit
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Edit")]
        public ActionResult New()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Receive")]
        public JsonResult ReceiveOrder(string idStr, string qtyStr, string sQtyStr)
        {
            try
            {
                if (string.IsNullOrEmpty(idStr))
                {
                    throw new BusinessException("明细不能为空");
                }
                string[] idArr = idStr.Split(',');
                string[] qtyArr = qtyStr.Split(',');
                string[] sQtyArr = sQtyStr.Split(',');

                IList<OrderDetail> orderDetailList = new List<OrderDetail>();
                for (int i = 0; i < idArr.Count(); i++)
                {
                    OrderDetail od = base.genericMgr.FindById<OrderDetail>(Convert.ToInt32(idArr[i]));
                    OrderDetailInput input = new OrderDetailInput();
                    input.ReceiveQty = Convert.ToDecimal(qtyArr[i]);
                    input.ScrapQty = Convert.ToDecimal(sQtyArr[i]);
                    od.AddOrderDetailInput(input);
                    orderDetailList.Add(od);
                }
                string printUrl = "";
                ReceiptMaster receiptMaster = orderMgr.ReceiveOrder(orderDetailList);
                if (receiptMaster.CreateHuOption == Sconit.CodeMaster.CreateHuOption.Receive)
                {
                    //打印
                    IList<Hu> huList = base.genericMgr.FindAll<Hu>("from Hu as h where h.ReceiptNo = ?", receiptMaster.ReceiptNo);
                    string orderNo = orderDetailList.Select(i => i.OrderNo).Distinct().Single();
                    string huTemplate = base.genericMgr.FindAll<string>("select HuTemplate from OrderMaster where OrderNo = ?", orderNo).Single();
                    if (string.IsNullOrWhiteSpace(huTemplate))
                    {
                        huTemplate = this.systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.DefaultBarCodeTemplate);
                    }
                    printUrl = PrintHuList(huList, huTemplate);
                }
                SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_Received, orderDetailList[0].OrderNo);
                return Json(new { OrderNo = orderDetailList[0].OrderNo, PrintUrl = printUrl });
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

        public ActionResult _WebOrderDetail(string itemCode)
        {
            if (!string.IsNullOrEmpty(itemCode))
            {
                WebOrderDetail webOrderDetail = new WebOrderDetail();
                Item item = base.genericMgr.FindById<Item>(itemCode);
                if (item != null)
                {
                    webOrderDetail.Item = item.Code;
                    webOrderDetail.ItemDescription = item.Description;
                    webOrderDetail.UnitCount = item.UnitCount;
                    webOrderDetail.Uom = item.Uom;
                    webOrderDetail.MinUnitCount = item.UnitCount;
                    webOrderDetail.ReferenceItemCode = item.ReferenceCode;
                }

                return this.Json(webOrderDetail);
            }
            return null;
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Edit")]
        public JsonResult New(OrderMaster orderMaster, [Bind(Prefix = "updated")]IEnumerable<OrderDetail> updatedOrderDetails)
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
                if (updatedOrderDetails != null && updatedOrderDetails.Count() > 0)
                {
                    foreach (OrderDetail orderDetail in updatedOrderDetails)
                    {
                        if (orderDetail.OrderedQty > 0)
                        {
                            orderDetail.ItemDescription = base.genericMgr.FindById<Item>(orderDetail.Item).Description;
                            if (!string.IsNullOrEmpty(orderDetail.LocationFrom))
                            {
                                orderDetail.LocationFromName = base.genericMgr.FindById<Location>(orderDetail.LocationFrom).Name;
                            }
                            if (!string.IsNullOrEmpty(orderDetail.LocationTo))
                            {
                                orderDetail.LocationToName = base.genericMgr.FindById<Location>(orderDetail.LocationTo).Name;
                            }
                            orderDetail.Bom = bomMgr.FindItemBom(orderDetail.Item);
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

                DateTime effectiveDate = orderMaster.EffectiveDate.HasValue ? orderMaster.EffectiveDate.Value : DateTime.Now;
                OrderMaster newOrder = orderMgr.TransferFlow2Order(flow, null, effectiveDate, false);

                newOrder.WindowTime = orderMaster.WindowTime;
                newOrder.StartTime = orderMaster.StartTime;
                newOrder.ReferenceOrderNo = orderMaster.ReferenceOrderNo;
                newOrder.ExternalOrderNo = orderMaster.ExternalOrderNo;
                newOrder.Dock = orderMaster.Dock;

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

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_QuickNew")]
        public ActionResult QuickNew()
        {

            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_QuickNew")]
        public JsonResult QuickNew(OrderMaster orderMaster, [Bind(Prefix =
             "updated")]IEnumerable<OrderDetail> updatedOrderDetails)
        {
            try
            {
                if (string.IsNullOrEmpty(orderMaster.Flow))
                {
                    throw new BusinessException(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.ORD.OrderMaster.OrderMaster_Flow);
                }
                if (orderMaster.EffectiveDate == null)
                {
                    throw new BusinessException(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.ORD.OrderMaster.OrderMaster_EffectiveDate);
                }

                #region orderDetailList
                IList<OrderDetail> orderDetailList = new List<OrderDetail>();

                if (updatedOrderDetails != null && updatedOrderDetails.Count() > 0)
                {
                    foreach (OrderDetail orderDetail in updatedOrderDetails)
                    {
                        if (orderDetail.OrderedQty > 0)
                        {
                            orderDetail.ItemDescription = base.genericMgr.FindById<Item>(orderDetail.Item).Description;
                            if (!string.IsNullOrEmpty(orderDetail.LocationFrom))
                            {
                                orderDetail.LocationFromName = base.genericMgr.FindById<Location>(orderDetail.LocationFrom).Name;
                            }
                            if (!string.IsNullOrEmpty(orderDetail.LocationTo))
                            {
                                orderDetail.LocationToName = base.genericMgr.FindById<Location>(orderDetail.LocationTo).Name;
                            }
                            orderDetail.Bom = bomMgr.FindItemBom(orderDetail.Item);
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
                DateTime effectiveDate = orderMaster.EffectiveDate.HasValue ? orderMaster.EffectiveDate.Value : DateTime.Now;
                OrderMaster newOrder = orderMgr.TransferFlow2Order(flow, null, effectiveDate, false);

                newOrder.ReferenceOrderNo = orderMaster.ReferenceOrderNo;
                newOrder.ExternalOrderNo = orderMaster.ExternalOrderNo;
                newOrder.IsQuick = true;
                newOrder.OrderDetails = orderDetailList;
                newOrder.WindowTime = DateTime.Now;
                newOrder.StartTime = DateTime.Now;

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

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Edit")]
        public ActionResult Edit(string orderNo)
        {
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
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Edit")]
        public ActionResult _Edit(string orderNo)
        {
            if (string.IsNullOrWhiteSpace(orderNo))
            {
                return HttpNotFound();
            }
            OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);
            ViewBag.flow = orderMaster.Flow;
            ViewBag.orderNo = orderMaster.OrderNo;
            ViewBag.Status = orderMaster.Status;
            ViewBag.editorTemplate = orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.Create ? "" : "ReadonlyTextBox";
            return PartialView(orderMaster);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Edit")]
        public ActionResult _Edit(OrderMaster orderMaster)
        {
            try
            {
                OrderMaster newOrder = base.genericMgr.FindById<OrderMaster>(orderMaster.OrderNo);
                newOrder.WindowTime = orderMaster.WindowTime;
                newOrder.StartTime = orderMaster.StartTime;
                orderMgr.UpdateOrder(newOrder);
                SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_Saved, orderMaster.OrderNo);
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }
            return RedirectToAction("Edit", new { orderNo = orderMaster.OrderNo });
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Edit")]
        public JsonResult _SaveBatchEditing(
            [Bind(Prefix = "updated")]IEnumerable<OrderDetail> updatedOrderDetails,
            [Bind(Prefix = "deleted")]IEnumerable<OrderDetail> deletedOrderDetails,
             string orderNo)
        {

            //orderMgr.BatchUpdateOrderDetails(orderNo, null, (IList<OrderDetail>)updatedOrderDetails, (IList<OrderDetail>)deletedOrderDetails);
            //IList<OrderDetail> orderDetailList = base.genericMgr.FindAll<OrderDetail>(selectOrderDetailStatement, orderNo);
            //return View(new GridModel(orderDetailList));
            try
            {
                orderMgr.BatchUpdateOrderDetails(orderNo, null, (IList<OrderDetail>)updatedOrderDetails, (IList<OrderDetail>)deletedOrderDetails);
                SaveErrorMessage(Resources.INV.StockTake.StockTakeDetail_Saved, orderNo);
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

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Delete")]
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

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Submit")]
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

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Start")]
        public ActionResult Start(string id)
        {
            try
            {
                if (IsVanOrder(id))
                {
                    orderMgr.StartVanOrder(id);
                }
                else
                {
                    orderMgr.StartOrder(id);
                }

                SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_Started, id);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            if (!IsVanOrder(id))
                return RedirectToAction("Edit", new { orderNo = id });
            else
                return RedirectToAction("VanEdit", new { orderNo = id });
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Receive")]
        public JsonResult VanReceive(string id, bool isForce)
        {
            try
            {
                var checkItemTrace = systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.CheckItemTrace);

                bool isCheckItemTrace;//整车入库是否强制关键件扫描
                bool.TryParse(checkItemTrace.ToString(), out isCheckItemTrace);
                orderMgr.ReceiveVanOrder(id, false, isCheckItemTrace, isForce);
                SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_Received, id);
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

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Close")]
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

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Cancel")]
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


        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Check")]
        public ActionResult Check(string id)
        {
            try
            {
                orderMgr.CheckOrder(id);
                SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_Checked, id);
            }
            catch (BusinessException ex)
            {
                foreach (Message message in ex.GetMessages())
                {
                    SaveErrorMessage(message.GetMessageString());
                }
            }
            return RedirectToAction("Edit", new { orderNo = id });
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_CreateRequisitionList")]
        public ActionResult CreateRequisitionList(string orderNo, DateTime? windowTime, int priority, string sapProdLine)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                {
                    throw new BusinessException("单号不能为空。");
                }
                
                else
                {
                    if (this.genericMgr.FindAll<int>("select COUNT(*) from CUST_ProductLineMap where type = 1 and SAPProdLine=?", sapProdLine)[0] == 0)
                    {
                        throw new BusinessException(string.Format("SAP生产线{0}无效。", sapProdLine));
                    }
                }
                if (windowTime == null)
                {
                    throw new BusinessException("窗口时间不能为空。");
                }

                if (sapProdLine == null)
                {
                    sapProdLine = this.systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.DefaultSAPProdLine);
                }
                else
                {
                    if (this.genericMgr.FindAll<int>("select COUNT(*) from CUST_ProductLineMap where type = 1 and SAPProdLine=?", sapProdLine)[0] == 0)
                    {
                        throw new BusinessException(string.Format("SAP生产线{0}无效,请重新输入。", sapProdLine));
                    }
                }
                IList<object[]> returnMessages = this.genericMgr.FindAllWithNativeSql<object[]>("exec USP_LE_ManualGenOrder ?,?,?,?,?,?",
                    new object[] { orderNo, windowTime.Value, priority, sapProdLine, CurrentUser.Id, CurrentUser.FullName },
                    new IType[] { NHibernateUtil.String, NHibernateUtil.DateTime, NHibernateUtil.Int16, NHibernateUtil.String, NHibernateUtil.Int32, NHibernateUtil.String });
                for (int i = 0; i < returnMessages.Count; i++)
                {
                    if (Convert.ToInt16(returnMessages[i][0]) == 0)
                    {
                        if (returnMessages[i][1] != null)
                        {
                            SaveSuccessMessage((string)(returnMessages[i][1]));
                        }
                    }
                    else
                    {
                        if (returnMessages[i][1] != null)
                        {
                            SaveErrorMessage((string)(returnMessages[i][1]));
                        }
                    }
                }
            }
            catch (BusinessException be)
            {
                SaveBusinessExceptionMessage(be);
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
            return RedirectToAction("Edit", new { orderNo = orderNo });
            //return PartialView("_Edit", this.genericMgr.FindById<OrderMaster>(orderNo));
            #region
            //try
            //{
            //    string[] objectString = orderMgr.CreateRequisitionList(id);
            //    if (!string.IsNullOrEmpty(objectString[0]))
            //    {
            //        SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_RequisitionList_Created, objectString[0]);
            //    }
            //    if (!string.IsNullOrEmpty(objectString[1]))
            //    {
            //        SaveWarningMessage(Resources.ORD.OrderMaster.OrderMaster_RequisitionList_ItemNotCreated, objectString[1]);
            //    }
            //    if (string.IsNullOrEmpty(objectString[0]) && string.IsNullOrEmpty(objectString[1]))
            //    {
            //        SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_RequisitionList_NoOrderCreate);
            //    }
            //}
            //catch (BusinessException ex)
            //{
            //    foreach (Message message in ex.GetMessages())
            //    {
            //        SaveErrorMessage(message.GetMessageString());
            //    }
            //}
            #endregion
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_CreateRequisitionList")]
        public ActionResult _RequisitionDetailWindow(string orderNo)
        {

            ViewBag.MasterorderNo = orderNo;

            return PartialView();

        }



        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_CreateRequisitionList")]
        public ActionResult _RequisitionDetailList(string MasterorderNo, string Item, string LocationTo, string LocationFrom, string DetailOrderNo)
        {
            ViewBag.Item = Item;
            ViewBag.MasterorderNo = MasterorderNo;
            ViewBag.DetailOrderNo = DetailOrderNo;
            ViewBag.LocationTo = LocationTo;
            ViewBag.LocationTo = LocationFrom;
            return PartialView();

        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_KBOrderBomDetailList")]
        public ActionResult _KBOrderBomDetWindow(string orderNo)
        {
            IList<OrderDetail> list = base.genericMgr.FindAll<OrderDetail>(" from OrderDetail o where o.OrderNo=?", orderNo);
            ViewBag.OrderDetailid = string.Empty;
            foreach (OrderDetail od in list)
            {
                if (ViewBag.OrderDetailid == string.Empty)
                {
                    ViewBag.OrderDetailid = od.Id.ToString();
                }
                else
                {
                    ViewBag.OrderDetailid += "," + od.Id.ToString();
                }
            }
            ViewBag.MasterorderNo = orderNo;

            return PartialView();

        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_KBOrderBomDetailList")]
        public ActionResult _KBOrderBomDetList(string MasterorderNo, string Item, string Flow, string StartTime, string OrderNo, string EndTime, string OrderDetailid)
        {
            ViewBag.Item = Item;
            ViewBag.MasterorderNo = MasterorderNo;
            ViewBag.OrderNo = OrderNo;
            ViewBag.Flow = Flow;
            ViewBag.StartTime = StartTime;
            ViewBag.EndTime = EndTime;
            ViewBag.OrderDetailid = OrderDetailid;

            return PartialView();

        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_KBOrderBomDetailList")]
        public ActionResult _SelectKBOrderBomDetail(string MasterorderNo, string Item, string Flow, string StartTime, string OrderNo, string EndTime, string OrderDetailid)
        {
            string selectStatement = string.Empty;

            IList<object> param = new List<object>();

            string[] strArray = OrderDetailid.Split(',');


            foreach (var para in strArray)
            {
                if (selectStatement == string.Empty)
                {
                    selectStatement = @"select od.*,kb.Flow from ORD_OrderBomDet as od  
                           inner join  ORD_KBOrderBomDet kb
                           on od.OrderDetId= kb.OrderBomDetId 
                           where  od.OrderDetId in (?";
                }
                else
                {
                    selectStatement += ",?";
                }
                param.Add(para);
            }
            selectStatement += ")";
            HqlStatementHelper.AddEqStatement("Item", Item, "od", ref selectStatement, param);
            HqlStatementHelper.AddEqStatement("Flow", Flow, "kb", ref selectStatement, param);
            if (!string.IsNullOrEmpty(StartTime) & !string.IsNullOrEmpty(EndTime))
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", StartTime, EndTime, "kb", ref selectStatement, param);
            }
            else if (!string.IsNullOrEmpty(StartTime) & string.IsNullOrEmpty(EndTime))
            {
                HqlStatementHelper.AddGeStatement("CreateDate", StartTime, "kb", ref selectStatement, param);
            }
            else if (string.IsNullOrEmpty(StartTime) & !string.IsNullOrEmpty(EndTime))
            {
                HqlStatementHelper.AddLeStatement("CreateDate", EndTime, "kb", ref selectStatement, param);
            }
            IList<OrderBomDetail> orderDetailList = base.genericMgr.FindAllWithNativeSql<OrderBomDetail>(selectStatement, param.ToArray());
            return PartialView(new GridModel(orderDetailList));
        }
        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_CreateRequisitionList")]
        public ActionResult _SelectRequisitionDetail(string MasterorderNo, string Item, string LocationTo, string LocationFrom, string DetailOrderNo)
        {
            StringBuilder selectOrderDetSql = new StringBuilder(@"select od.* from View_OrderDet od 
                                                               inner join View_OrderMstr om on om.OrderNo = od.OrderNo 
                                                               inner join CUST_ManualGenOrderTrace mgo on om.OrderNo = mgo.OrderNo 
                                                               where mgo.ProdOrderNo = '" + MasterorderNo + "'");
            IList<object> selectOrderDetParam = new List<object>();

            if (!string.IsNullOrWhiteSpace(Item))
            {
                selectOrderDetSql.Append(" and od.Item = ? ");
                selectOrderDetParam.Add(Item);
            }
            else if (!string.IsNullOrWhiteSpace(LocationTo))
            {
                selectOrderDetSql.Append(" and od.LocTo = ? ");
                selectOrderDetParam.Add(LocationTo);
            }

            if (!string.IsNullOrWhiteSpace(LocationFrom))
            {
                selectOrderDetSql.Append(" and od.LocFrom = ? ");
                selectOrderDetParam.Add(LocationFrom);
            }
            if (!string.IsNullOrWhiteSpace(DetailOrderNo))
            {
                selectOrderDetSql.Append(" and od.OrderNo = ? ");
                selectOrderDetParam.Add(DetailOrderNo);
            }

            selectOrderDetSql.Append(" order by od.OrderNo desc ");

            IList<OrderDetail> orderDetailList = base.genericMgr.FindEntityWithNativeSql<OrderDetail>(selectOrderDetSql.ToString(), selectOrderDetParam.ToArray());
            return PartialView(new GridModel(orderDetailList));
        }





        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Edit")]
        public ActionResult _OrderDetailList(string flow, string orderNo)
        {
            ViewBag.flow = flow;
            ViewBag.orderNo = orderNo;
            ViewBag.Status = null;
            FlowMaster flowMaster = null;

            if (!string.IsNullOrEmpty(orderNo))
            {
                OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);
                ViewBag.Status = orderMaster.Status;
                ViewBag.Region = orderMaster.PartyFrom;
            }
            else if (!string.IsNullOrEmpty(flow))
            {
                flowMaster = base.genericMgr.FindById<FlowMaster>(flow);

                ViewBag.Status = com.Sconit.CodeMaster.OrderStatus.Create;
                ViewBag.Region = flowMaster.PartyFrom;
            }
            if (ViewBag.Status == com.Sconit.CodeMaster.OrderStatus.Create)
            {
                #region combox
                IList<Uom> uoms = base.genericMgr.FindAll<Uom>();
                ViewData.Add("uoms", uoms);
                IList<BomMaster> boms = base.genericMgr.FindAll<BomMaster>();
                ViewData.Add("boms", boms);
                IList<RoutingMaster> routings = base.genericMgr.FindAll<RoutingMaster>();
                ViewData.Add("routings", routings);

                //IList<Location> locationFroms = new List<Location>();
                //IList<Location> locationTos = new List<Location>();
                #endregion


            }
            return PartialView();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Edit")]
        public ActionResult _SelectBatchEditing(string orderNo, string flow)
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
                        orderDetailList = TransformFlowDetailList2OrderDetailList(flow);
                    }
                }
            }
            return PartialView(new GridModel(orderDetailList));
        }


        public String _WindowTime(string flow, string windowTime)
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

        [HttpGet]
        public ActionResult _OrderBomDetail(string id, int orderStatus, string region)
        {
            ViewBag.OrderDetailId = id;
            ViewBag.OrderStatus = orderStatus;
            ViewBag.Region = region;
            return PartialView();
        }

        public ActionResult _OrderBomDetailList(GridCommand command, OrderBomDetailSearchModel searchModel)
        {
            ViewBag.OrderDetailId = searchModel.OrderDetailId;
            ViewBag.OrderStatus = searchModel.OrderStatus;
            ViewBag.ReadOnly = searchModel.OrderStatus != (int)com.Sconit.CodeMaster.OrderStatus.Create && searchModel.OrderStatus != (int)com.Sconit.CodeMaster.OrderStatus.Submit;
            IList<CodeDetail> backFlushMethodList = systemMgr.GetCodeDetails(Sconit.CodeMaster.CodeMaster.BackFlushMethod);
            IList<CodeDetail> feedMethodList = systemMgr.GetCodeDetails(Sconit.CodeMaster.CodeMaster.FeedMethod);

            ViewData["uoms"] = base.genericMgr.FindAll<Uom>();
            ViewData["BackFlushMethod"] = base.Transfer2DropDownList(com.Sconit.CodeMaster.CodeMaster.BackFlushMethod, backFlushMethodList);
            ViewData["FeedMethod"] = base.Transfer2DropDownList(com.Sconit.CodeMaster.CodeMaster.FeedMethod, feedMethodList);
            TempData["OrderBomDetailSearchModel"] = searchModel;
            return PartialView();
        }

        [GridAction]
        public ActionResult _SelectBomDetailBatchEditing(GridCommand command, OrderBomDetailSearchModel searchModel)
        {
            string whereStatement = "select b from OrderBomDetail as b where b.OrderDetailId = ?";
            IList<object> param = new List<object>();
            param.Add(searchModel.OrderDetailId);

            HqlStatementHelper.AddEqStatement("Operation", searchModel.Operation, "b", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("OpReference", searchModel.OpReference, "b", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "b", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Location", searchModel.Location, "b", ref whereStatement, param);

            IList<OrderBomDetail> orderBomDetailList = base.genericMgr.FindAll<OrderBomDetail>(whereStatement, param.ToArray());
            return PartialView(new GridModel(orderBomDetailList));
        }

        public ActionResult _OrderOperationList(string id)
        {
            ViewBag.OrderDetailId = id;

            return PartialView();
        }

        [GridAction]
        public ActionResult _SelectOperationBatchEditing(GridCommand command, string orderDetailId)
        {
            IList<OrderOperation> orderOperationList = base.genericMgr.FindAll<OrderOperation>(selectOrderOperationStatement, orderDetailId);
            return View(new GridModel(orderOperationList));
        }


        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        public JsonResult _SaveBomDetailBatchEditing([Bind(Prefix =
            "inserted")]IEnumerable<OrderBomDetail> insertedBomDetails,
            [Bind(Prefix = "updated")]IEnumerable<OrderBomDetail> updatedBomDetails,
            [Bind(Prefix = "deleted")]IEnumerable<OrderBomDetail> deletedBomDetails, string OrderDetailId)
        {
            try
            {

                IList<OrderBomDetail> newOrderBomDetailList = new List<OrderBomDetail>();
                IList<OrderBomDetail> updateOrderBomDetailList = new List<OrderBomDetail>();
                if (insertedBomDetails != null && insertedBomDetails.Count() > 0)
                {
                    foreach (OrderBomDetail orderBomDetail in insertedBomDetails)
                    {
                        PrepareOrderBomDetail(orderBomDetail);
                        newOrderBomDetailList.Add(orderBomDetail);
                    }
                }
                if (updatedBomDetails != null && updatedBomDetails.Count() > 0)
                {
                    foreach (OrderBomDetail orderBomDetail in updatedBomDetails)
                    {
                        PrepareOrderBomDetail(orderBomDetail);
                        updateOrderBomDetailList.Add(orderBomDetail);
                    }
                }

                orderMgr.BatchUpdateOrderBomDetails(int.Parse(OrderDetailId), newOrderBomDetailList, updateOrderBomDetailList, (IList<OrderBomDetail>)deletedBomDetails);

                SaveSuccessMessage(Resources.ORD.OrderBomDetail.OrderBomDetail_Saved, OrderDetailId);
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


        public void ExportOrderBomDetailXLS(GridCommand command, OrderBomDetailSearchModel searchModel)
        {
            string maxRows = this.systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.ExportMaxRows);
            command.PageSize = int.Parse(maxRows);
            string whereStatement = "select b from OrderBomDetail as b where b.OrderDetailId = ?";
            IList<object> param = new List<object>();
            param.Add(searchModel.OrderDetailId);

            HqlStatementHelper.AddEqStatement("Operation", searchModel.Operation, "b", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("OpReference", searchModel.OpReference, "b", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "b", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Location", searchModel.Location, "b", ref whereStatement, param);

            IList<OrderBomDetail> orderBomDetailList = base.genericMgr.FindAll<OrderBomDetail>(whereStatement, param.ToArray());
            FillCodeDetailDescription(orderBomDetailList);
            ExportToXLS<OrderBomDetail>("OrderBomDetail", "XLS", orderBomDetailList);
        }

        #region 先注掉，保存物料清单和工艺流程的
        //[AcceptVerbs(HttpVerbs.Post)]
        //[GridAction]
        //public ActionResult _SaveBomDetailBatchEditing([Bind(Prefix =
        //    "inserted")]IEnumerable<OrderBomDetail> insertedBomDetails,
        //    [Bind(Prefix = "updated")]IEnumerable<OrderBomDetail> updatedBomDetails,
        //    [Bind(Prefix = "deleted")]IEnumerable<OrderBomDetail> deletedBomDetails, string id)
        //{
        //    if (insertedBomDetails != null)
        //    {
        //        orderMgr.AddOrderBomDetails(Convert.ToInt32(id), (IList<OrderBomDetail>)insertedBomDetails);
        //    }
        //    if (updatedBomDetails != null)
        //    {
        //        orderMgr.UpdateOrderBomDetails((IList<OrderBomDetail>)updatedBomDetails);
        //    }
        //    if (deletedBomDetails != null)
        //    {
        //        IList<int> deletedBomDetailIds = deletedBomDetails.Select(q => q.Id).ToList<int>();
        //        orderMgr.DeleteOrderBomDetails(deletedBomDetailIds);
        //    }
        //    IList<OrderBomDetail> orderBomDetailList = base.genericMgr.FindAll<OrderBomDetail>("select b from OrderBomDetail as b where b.OrderDetailId = ?", id);
        //    return View(new GridModel(orderBomDetailList));
        //}

        //[AcceptVerbs(HttpVerbs.Post)]
        //[GridAction]
        //public ActionResult _SaveOperationBatchEditing([Bind(Prefix =
        //    "inserted")]IEnumerable<OrderOperation> insertedOperations,
        //    [Bind(Prefix = "updated")]IEnumerable<OrderOperation> updatedOperations,
        //    [Bind(Prefix = "deleted")]IEnumerable<OrderOperation> deletedOperations, string id)
        //{
        //    if (insertedOperations != null)
        //    {
        //        orderMgr.AddOrderOperations(Convert.ToInt32(id), (IList<OrderOperation>)insertedOperations);
        //    }
        //    if (updatedOperations != null)
        //    {
        //        orderMgr.UpdateOrderOperations((IList<OrderOperation>)updatedOperations);
        //    }
        //    if (deletedOperations != null)
        //    {
        //        IList<int> deletedOperationIds = deletedOperations.Select(q => q.Id).ToList<int>();
        //        orderMgr.DeleteOrderBomDetails(deletedOperationIds);
        //    }
        //    IList<OrderOperation> orderOperationList = base.genericMgr.FindAll<OrderOperation>(selectOrderOperationStatement, id);
        //    return View(new GridModel(orderOperationList));
        //}

        //public ActionResult _WebBomDetail(string itemCode)
        //{
        //    WebOrderDetail webOrderDetail = new WebOrderDetail();

        //    Item item = base.genericMgr.FindById<Item>(itemCode);
        //    if (item != null)
        //    {
        //        webOrderDetail.Item = item.Code;
        //        webOrderDetail.ItemDescription = item.Description;
        //        webOrderDetail.Uom = item.Uom;
        //    }
        //    return this.Json(webOrderDetail);
        //}
        #endregion

        #endregion

        #region import
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Import")]
        public ActionResult Import()
        {
            return View();
        }
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Import")]
        public ActionResult ImportSapOrder(OrderMaster orderMaster)
        {
            try
            {
                if (string.IsNullOrEmpty(orderMaster.ExternalOrderNo))
                {
                    throw new BusinessException("SAP生产单号不能为空");
                }
                SAPService.SAPService sapService = new SAPService.SAPService();
                sapService.Url = ReplaceSIServiceUrl(sapService.Url);
                sapService.Timeout = int.Parse(SIService_TimeOut);
                com.Sconit.Entity.ACC.User user = SecurityContextHolder.Get();

                List<string> sapNoList = new List<string>();
                sapNoList.Add(orderMaster.ExternalOrderNo);
                IList<string> msgList = sapService.GetProductOrder("0084", sapNoList.ToArray(), user.Code);
                if (msgList != null && msgList.Count > 0)
                {
                    foreach (var m in msgList)
                    {
                        SaveErrorMessage(m);
                    }
                }
                else
                {
                    SaveSuccessMessage("获取非整车生产单成功");
                }

                //SaveSuccessMessage(string.Format("SAP生产单{0}导入成功"));
                //if (string.IsNullOrWhiteSpace(orderNo))
                //{
                //    SaveErrorMessage(string.Format("SAP生产单{0}不存在或没有释放或没有维护LES生产线。", orderMaster.ExternalOrderNo));
                //}
                //else
                //{
                //    SaveSuccessMessage(string.Format("SAP生产单{0}导入成功，生成LES生产单号为{1}。", orderMaster.ExternalOrderNo, orderNo));
                //}
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex.Message);
            }

            return View("Import");
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Import")]
        public ActionResult ImportSapOrders(OrderMaster orderMaster)
        {
            //try
            //{
            //    if (!orderMaster.EffectiveDate.HasValue || orderMaster.EffectiveDate == null)
            //    {
            //        throw new BusinessException("开始时间不能为空");
            //    }
            //    SAPService.SAPService sapService = new SAPService.SAPService();
            //    com.Sconit.Entity.ACC.User user = SecurityContextHolder.Get();

            //    orderMaster.EffectiveDate = orderMaster.EffectiveDate.HasValue ? orderMaster.EffectiveDate : System.DateTime.Now;
            //    sapService.GetProdOrders(user.Code, orderMaster.EffectiveDate.Value, orderMaster.Flow);

            //    SaveSuccessMessage("SAP生产单导入成功。");

            //}
            //catch (BusinessException ex)
            //{
            //    SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            //}
            //catch (Exception ex)
            //{
            //    SaveErrorMessage(ex.Message);
            //}

            return View("Import");
        }



        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Import")]
        public ActionResult ImportSapOrderByExcel(IEnumerable<HttpPostedFileBase> attachments)
        {
            try
            {
                List<string> sapNoList = new List<string>();
                foreach (var file in attachments)
                {
                    #region 导入数据
                    if (file.InputStream.Length == 0)
                    {
                        throw new BusinessException("Import.Stream.Empty");
                    }

                    HSSFWorkbook workbook = new HSSFWorkbook(file.InputStream);

                    ISheet sheet = workbook.GetSheetAt(0);
                    IEnumerator rows = sheet.GetRowEnumerator();

                    ImportHelper.JumpRows(rows, 1);

                    while (rows.MoveNext())
                    {
                        HSSFRow row = (HSSFRow)rows.Current;
                        if (!ImportHelper.CheckValidDataRow(row, 0, 1))
                        {
                            break;
                        }
                        string sapNo = ImportHelper.GetCellStringValue(row.GetCell(0));
                        if (string.IsNullOrWhiteSpace(sapNo))
                        {
                            ImportHelper.ThrowCommonError(row.RowNum, 0, row.GetCell(0));
                        }
                        sapNoList.Add(sapNo);
                    }

                    #endregion
                }
                if (sapNoList == null || sapNoList.Count == 0)
                {
                    throw new BusinessException("导入失败，模版为空或者数据填写错误 请确认！");
                }
                SAPService.SAPService sapService = new SAPService.SAPService();
                sapService.Url = ReplaceSIServiceUrl(sapService.Url);
                sapService.Timeout = int.Parse(SIService_TimeOut);
                com.Sconit.Entity.ACC.User user = SecurityContextHolder.Get();

                IList<string> msgList = sapService.GetProductOrder("0084", sapNoList.ToArray(), user.Code);
                if (msgList != null && msgList.Count > 0)
                {
                    foreach (var m in msgList)
                    {
                        SaveErrorMessage(m);
                    }
                }
                else
                {
                    SaveSuccessMessage("获取非整车生产单成功");
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

        #region Condition Import 非整车生产单条件导入
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_ConditionImport")]
        public ActionResult ConditionImport()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Import")]
        public ActionResult ConditionImportSapOrders(DateOption? dateOption, string sapOrderTypeList, DateTime? dateFrom, DateTime? dateTo, string mrpCtrlList)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sapOrderTypeList))
                {
                    throw new BusinessException("请选择Sap生产单类型。");
                }
                if (!dateOption.HasValue)
                {
                    throw new BusinessException("请选择日期选项。");
                }
                if (!dateFrom.HasValue)
                {
                    throw new BusinessException("开始日期不能为空。");
                }
                if (dateOption == DateOption.BT)
                {
                    if (!dateTo.HasValue)
                    {
                        throw new BusinessException("结束日期不能为空。");
                    }
                    if (dateFrom.Value > dateTo.Value)
                    {
                        throw new BusinessException("开始日期不能大于结束日期。");
                    }
                }
                if (string.IsNullOrWhiteSpace(mrpCtrlList))
                {
                    throw new BusinessException("MRP控制者不能为空。");
                }
                com.Sconit.Entity.ACC.User user = SecurityContextHolder.Get();
                SAPService.SAPService sapService = new SAPService.SAPService();
                sapService.Url = ReplaceSIServiceUrl(sapService.Url);
                sapService.Timeout = int.Parse(SIService_TimeOut);
                var returnMsgList = sapService.GetProductOrder2("0084", sapOrderTypeList.Split(',').ToArray(), dateOption.Value,
                                                               dateFrom, dateTo,
                                                               new[] { mrpCtrlList }, user.Code);
                if (returnMsgList != null && returnMsgList.Count() > 0)
                {
                    foreach (var s in returnMsgList)
                    {
                        SaveSuccessMessage(s);
                    }
                }
                else
                {
                    SaveSuccessMessage("SAP生产单导入成功。");
                    return Json(new { });
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

        #region sequece import
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_SeqImport")]
        public ActionResult SeqImport()
        {
            return View();
        }
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_SeqImport")]
        public ActionResult ImportSeq(string plant, string ZLINE, string ExternalOrderNo)
        {
            try
            {
                ModelState.Clear(); BusinessException businessException = new BusinessException();
                if (string.IsNullOrWhiteSpace(plant))
                {
                    businessException.AddMessage("工厂不能为空。");
                }

                if (string.IsNullOrWhiteSpace(ZLINE))
                {
                    businessException.AddMessage("SAP生产线不能为空。");
                }

                if (string.IsNullOrWhiteSpace(ExternalOrderNo))
                {
                    businessException.AddMessage("SAP整车生产单号不能为空。");
                }

                if (businessException.HasMessage)
                {
                    throw businessException;
                }

                SAPService.SAPService sapService = new SAPService.SAPService();
                sapService.Timeout = int.Parse(SIService_TimeOut);
                sapService.Url = ReplaceSIServiceUrl(sapService.Url);
                com.Sconit.Entity.ACC.User user = SecurityContextHolder.Get();
                var errorMessageList = sapService.UpdateVanOrder(plant, ExternalOrderNo, ZLINE, user.Code);
                if (errorMessageList != null && errorMessageList.Count() > 0)
                {
                    foreach (var msg in errorMessageList)
                    {
                        SaveErrorMessage(msg);
                    }
                }
                else
                {
                    SaveSuccessMessage("整车生产单更新成功。");
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

            return View("SeqImport");
        }
        #endregion

        #region receive 整车下线
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Receive")]
        public ActionResult ReceiveIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Receive")]
        public ActionResult ReceiveList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Receive")]
        public ActionResult _ReceiveAjaxList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            string whereStatement = " and o.Status = " + (int)com.Sconit.CodeMaster.OrderStatus.InProcess +
                                    " and o.SubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal +
                                    " and o.ProdLineType in (1,2,3,4,9)" +
                                    " and exists (select 1 from OrderDetail as d where d.RecQty + d.ScrapQty  < d.OrderQty and d.OrderNo = o.OrderNo) ";
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareVanSearchStatement_1(command, searchModel, whereStatement, new SortDescriptor { Member = "Seq asc,SubSeq ", SortDirection = ListSortDirection.Ascending }, false);
            return PartialView(GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Receive")]
        public ActionResult _ReceiveOrderDetailList(string orderNo)
        {
            ViewBag.IsVanOrder = IsVanOrder(orderNo);
            OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);

            DetachedCriteria criteria = DetachedCriteria.For<OrderDetail>();
            criteria.Add(Expression.Eq("OrderNo", orderNo));
            if (!orderMaster.IsOpenOrder)
            {
                criteria.Add(Expression.LtProperty("ReceivedQty", "OrderedQty"));
            }
            IList<OrderDetail> orderDetailList = base.genericMgr.FindAll<OrderDetail>(criteria);
            return PartialView(orderDetailList);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Receive")]
        public ActionResult ReceiveEdit(string orderNo)
        {
            OrderMaster order = base.genericMgr.FindById<OrderMaster>(orderNo);
            ViewBag.IsVanOrder = IsVanOrder(orderNo);
            return View(order);
        }
        #endregion

        #region  整车生产单强制下线
        [SconitAuthorize(Permissions = "Url_Production_ForceReceive")]
        public ActionResult ForceReceiveIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Production_ForceReceive")]
        public ActionResult ForceReceiveList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Production_ForceReceive")]
        public ActionResult _ForceReceiveAjaxList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            string whereStatement = " and o.Status = " + (int)com.Sconit.CodeMaster.OrderStatus.InProcess +
                                    " and o.SubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal +
                                    " and o.ProdLineType in (1,2,3,4,9)" +
                                    " and exists (select 1 from OrderDetail as d where d.RecQty + d.ScrapQty  < d.OrderQty and d.OrderNo = o.OrderNo) ";
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareVanSearchStatement_1(command, searchModel, whereStatement, new SortDescriptor { Member = "Seq asc,SubSeq ", SortDirection = ListSortDirection.Ascending }, false);
            return PartialView(GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Production_ForceReceive")]
        public ActionResult _ForceReceiveOrderDetailList(string orderNo)
        {
            ViewBag.IsVanOrder = IsVanOrder(orderNo);
            OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);

            DetachedCriteria criteria = DetachedCriteria.For<OrderDetail>();
            criteria.Add(Expression.Eq("OrderNo", orderNo));
            if (!orderMaster.IsOpenOrder)
            {
                criteria.Add(Expression.LtProperty("ReceivedQty", "OrderedQty"));
            }
            IList<OrderDetail> orderDetailList = base.genericMgr.FindAll<OrderDetail>(criteria);
            return PartialView(orderDetailList);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Receive")]
        public ActionResult ForceReceiveEdit(string orderNo)
        {
            OrderMaster order = base.genericMgr.FindById<OrderMaster>(orderNo);
            ViewBag.IsVanOrder = IsVanOrder(orderNo);
            return View(order);
        }

        //[SconitAuthorize(Permissions = "Url_OrderMstr_Production_Receive")]
        //public JsonResult VanForceReceive(string id)
        //{
        //    try
        //    {
        //        var checkItemTrace = systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.CheckItemTrace);

        //        bool isCheckItemTrace;//整车入库是否强制关键件扫描
        //        bool.TryParse(checkItemTrace.ToString(), out isCheckItemTrace);
        //        orderMgr.ReceiveVanOrder(id, false, isCheckItemTrace);
        //        orderMgr.ReceiveVanOrder(id, false, isCheckItemTrace);

        //        SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_Received, id);
        //        return Json(new { });
        //    }
        //    catch (BusinessException ex)
        //    {
        //        SaveBusinessExceptionMessage(ex);
        //    }
        //    catch (Exception ex)
        //    {
        //        SaveErrorMessage(ex);
        //    }
        //    return Json(null);
        //}
        #endregion

        #region materialin
        [SconitAuthorize(Permissions = "Url_Production_MaterialIn_FeedOrderMaster")]
        public ActionResult MaterialInIndex()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_Production_MaterialIn_ForceFeedOrderMaster")]
        public ActionResult ForceMaterialInIndex()
        {
            return View();
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_Production_MaterialIn_FeedOrderMaster")]
        public JsonResult FeedQty(OrderMaster orderMaster, [Bind(Prefix = "updated")]IEnumerable<OrderBomDetail> updatedOrderBomDetails)
        {
            try
            {
                if (string.IsNullOrEmpty(orderMaster.OrderNo))
                {
                    throw new BusinessException("生产单不能为空");
                }
                string ordercheckStr = SecurityHelper.CheckOrderStatement(orderMaster.OrderNo, com.Sconit.CodeMaster.OrderType.Production);
                orderMaster = base.genericMgr.FindAll<OrderMaster>(ordercheckStr).SingleOrDefault<OrderMaster>();
                if (orderMaster == null)
                {
                    throw new BusinessException("生产单不正确或没有权限");
                }

                #region orderBomdetailList
                IList<FeedInput> feedInputList = new List<FeedInput>();
                if (updatedOrderBomDetails != null && updatedOrderBomDetails.Count() > 0)
                {
                    foreach (OrderBomDetail orderBomDetail in updatedOrderBomDetails)
                    {
                        FeedInput feedInput = new FeedInput();

                        if (!string.IsNullOrEmpty(orderBomDetail.FeedLocation))
                        {
                            string locationcheckStr = "from Location as l where l.Region = ? and l.Code = ?";
                            Location location = base.genericMgr.FindAll<Location>(locationcheckStr, new object[] { orderMaster.PartyFrom, orderBomDetail.FeedLocation }).SingleOrDefault<Location>();
                            if (location == null)
                            {
                                throw new BusinessException("物料" + orderBomDetail.Item + "的库位不正确");
                            }
                            feedInput.LocationFrom = orderBomDetail.FeedLocation;
                        }
                        else
                        {
                            feedInput.LocationFrom = orderBomDetail.Location;
                        }

                        feedInput.OrderNo = orderMaster.OrderNo;
                        feedInput.Qty = orderBomDetail.FeedQty;
                        feedInput.Item = orderBomDetail.Item;
                        if (string.IsNullOrEmpty(orderBomDetail.Uom))
                        {
                            Item item = base.genericMgr.FindById<Item>(orderBomDetail.Item);
                            feedInput.Uom = item.Uom;
                        }
                        else
                        {
                            feedInput.Uom = orderBomDetail.Uom;
                        }
                        feedInput.Operation = orderBomDetail.Operation;
                        feedInput.OpReference = orderBomDetail.OpReference;
                        feedInputList.Add(feedInput);
                    }
                }
                #endregion

                if (feedInputList.Count == 0)
                {
                    throw new BusinessException("投料明细为空");
                }

                productionLineMgr.FeedRawMaterial(orderMaster.OrderNo, feedInputList, false);
                SaveSuccessMessage(Resources.PRD.ProductLineLocationDetail.ProductLineLocationDetail_FeedIn);
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

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_Production_MaterialIn_ForceFeedOrderMaster")]
        public JsonResult ForceFeedQty(OrderMaster orderMaster, [Bind(Prefix =
             "inserted")]IEnumerable<OrderBomDetail> insertedOrderBomDetails, [Bind(Prefix =
             "updated")]IEnumerable<OrderBomDetail> updatedOrderBomDetails)
        {
            try
            {
                if (string.IsNullOrEmpty(orderMaster.OrderNo))
                {
                    throw new BusinessException("生产单不能为空");
                }
                orderMaster = base.genericMgr.FindById<OrderMaster>(orderMaster.OrderNo);

                #region orderBomdetailList

                IList<FeedInput> feedInputList = new List<FeedInput>();
                List<OrderBomDetail> orderBomDetailList = new List<OrderBomDetail>();
                if (insertedOrderBomDetails != null && insertedOrderBomDetails.Count() > 0)
                {
                    orderBomDetailList.AddRange(insertedOrderBomDetails);

                }
                if (updatedOrderBomDetails != null && updatedOrderBomDetails.Count() > 0)
                {
                    orderBomDetailList.AddRange(updatedOrderBomDetails);

                }
                if (orderBomDetailList.Count > 0)
                {
                    foreach (OrderBomDetail orderBomDetail in orderBomDetailList)
                    {
                        FeedInput feedInput = new FeedInput();
                        if (!string.IsNullOrEmpty(orderBomDetail.FeedLocation))
                        {
                            string locationcheckStr = "from Location as l where l.Region = ? and l.Code = ?";
                            Location location = base.genericMgr.FindAll<Location>(locationcheckStr, new object[] { orderMaster.PartyFrom, orderBomDetail.FeedLocation }).SingleOrDefault<Location>();
                            if (location == null)
                            {
                                throw new BusinessException("物料{0}的库位不正确", orderBomDetail.Item);
                            }
                            feedInput.LocationFrom = orderBomDetail.FeedLocation;
                        }
                        else
                        {
                            feedInput.LocationFrom = orderMaster.LocationFrom;
                        }
                        feedInput.OrderNo = orderMaster.OrderNo;
                        feedInput.Qty = orderBomDetail.FeedQty;
                        feedInput.Item = orderBomDetail.Item;
                        if (string.IsNullOrEmpty(orderBomDetail.Uom))
                        {
                            Item item = base.genericMgr.FindById<Item>(orderBomDetail.Item);
                            feedInput.Uom = item.Uom;
                        }
                        else
                        {
                            feedInput.Uom = orderBomDetail.Uom;
                        }
                        feedInput.QualityType = com.Sconit.CodeMaster.QualityType.Qualified;
                        feedInputList.Add(feedInput);
                    }
                }
                #endregion

                if (feedInputList.Count == 0)
                {
                    throw new BusinessException("投料明细为空");
                }

                productionLineMgr.FeedRawMaterial(orderMaster.OrderNo, feedInputList, true);
                SaveSuccessMessage(Resources.PRD.ProductLineLocationDetail.ProductLineLocationDetail_FeedIn);
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


        [GridAction]
        public ActionResult _SelectForceFeedQtyBatchEditing()
        {
            return PartialView(new GridModel(new List<OrderBomDetail>()));
        }

        [GridAction]
        public ActionResult _SelectFeedQtyBatchEditing(string orderNo)
        {
            IList<OrderBomDetail> orderBomDetails = new List<OrderBomDetail>();
            string ordercheckStr = SecurityHelper.CheckOrderStatement(orderNo, com.Sconit.CodeMaster.OrderType.Production);
            OrderMaster orderMaster = base.genericMgr.FindAll<OrderMaster>(ordercheckStr).SingleOrDefault<OrderMaster>();
            if (orderMaster != null)
            {
                if (orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.Submit || orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.InProcess || orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.Complete)
                {
                    string selectOrderBomDetailStatement = "select d from OrderBomDetail as d where d.OrderNo = ? and d.BackFlushMethod = ?";
                    orderBomDetails = base.genericMgr.FindAll<OrderBomDetail>(selectOrderBomDetailStatement, new object[] { orderNo, (int)com.Sconit.CodeMaster.BackFlushMethod.WeightAverage });
                    if (orderBomDetails != null && orderBomDetails.Count > 0)
                    {
                        foreach (OrderBomDetail bomDetail in orderBomDetails)
                        {
                            Item item = base.genericMgr.FindById<Item>(bomDetail.Item);
                            if (item != null)
                            {
                                bomDetail.Item = item.Code;
                                bomDetail.ReferenceItemCode = item.ReferenceCode;
                                bomDetail.ItemDescription = item.Description;
                                bomDetail.UnitCount = item.UnitCount;
                                bomDetail.Uom = item.Uom;
                            }
                        }
                    }
                }
            }
            return PartialView(new GridModel(orderBomDetails));
        }



        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Production_MaterialIn_FeedOrderMaster")]
        public ActionResult _QtyMaterialInEdit()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Production_MaterialIn_ForceFeedOrderMaster")]
        public ActionResult _ForceQtyMaterialInEdit()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Production_MaterialIn_FeedOrderMaster")]
        public ActionResult _FeedQtyDetailList(string orderNo)
        {

            IList<Uom> uoms = base.genericMgr.FindAll<Uom>();
            ViewData.Add("uoms", uoms);

            #region 选默认库位的
            string ordercheckStr = SecurityHelper.CheckOrderStatement(orderNo, com.Sconit.CodeMaster.OrderType.Production);
            OrderMaster orderMaster = base.genericMgr.FindAll<OrderMaster>(ordercheckStr).SingleOrDefault<OrderMaster>();
            if (orderMaster != null)
            {
                ViewBag.Region = orderMaster.PartyFrom;
            }
            #endregion

            ViewBag.orderNo = orderNo;
            return PartialView();

        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Production_MaterialIn_ForceFeedOrderMaster")]
        public ActionResult _ForceFeedQtyDetailList(string orderNo)
        {
            #region combox
            IList<Uom> uoms = base.genericMgr.FindAll<Uom>();
            ViewData.Add("uoms", uoms);
            #endregion

            #region 选默认库位的
            string ordercheckStr = SecurityHelper.CheckOrderStatement(orderNo, com.Sconit.CodeMaster.OrderType.Production);
            OrderMaster orderMaster = base.genericMgr.FindAll<OrderMaster>(ordercheckStr).SingleOrDefault<OrderMaster>();
            if (orderMaster != null)
            {
                ViewBag.Region = orderMaster.PartyFrom;
            }
            #endregion

            return PartialView();
        }
        #endregion

        #region return
        [SconitAuthorize(Permissions = "Url_Production_MaterialIn_ReturnOrderMaster")]
        public ActionResult MaterialInReturnIndex()
        {
            return View();
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_Production_MaterialIn_ReturnOrderMaster")]
        public JsonResult ReturnQty(OrderMaster orderMaster, [Bind(Prefix =
             "inserted")]IEnumerable<OrderBomDetail> insertedOrderBomDetails, [Bind(Prefix =
             "updated")]IEnumerable<OrderBomDetail> updatedOrderBomDetails)
        {
            try
            {
                if (string.IsNullOrEmpty(orderMaster.OrderNo))
                {
                    throw new BusinessException("生产单不能为空");
                }
                string ordercheckStr = SecurityHelper.CheckOrderStatement(orderMaster.OrderNo, com.Sconit.CodeMaster.OrderType.Production);
                orderMaster = base.genericMgr.FindAll<OrderMaster>(ordercheckStr).SingleOrDefault<OrderMaster>();
                if (orderMaster == null)
                {
                    throw new BusinessException("生产单不正确或没有权限");
                }
                #region orderBomdetailList

                IList<ReturnInput> returnInputList = new List<ReturnInput>();
                List<OrderBomDetail> orderBomDetailList = new List<OrderBomDetail>();
                if (insertedOrderBomDetails != null && insertedOrderBomDetails.Count() > 0)
                {
                    orderBomDetailList.AddRange(insertedOrderBomDetails);
                }
                if (updatedOrderBomDetails != null && updatedOrderBomDetails.Count() > 0)
                {
                    orderBomDetailList.AddRange(updatedOrderBomDetails);
                }
                if (orderBomDetailList.Count > 0)
                {
                    foreach (OrderBomDetail orderBomDetail in orderBomDetailList)
                    {
                        ReturnInput returnInput = new ReturnInput();
                        if (!string.IsNullOrEmpty(orderBomDetail.FeedLocation))
                        {
                            string locationcheckStr = "from Location as l where l.Region = ? and l.Code = ?";
                            Location location = base.genericMgr.FindAll<Location>(locationcheckStr, new object[] { orderMaster.PartyFrom, orderBomDetail.FeedLocation }).SingleOrDefault<Location>();
                            if (location == null)
                            {
                                throw new BusinessException("物料" + orderBomDetail.Item + "的库位不正确");
                            }
                            returnInput.LocationTo = orderBomDetail.FeedLocation;
                        }
                        else
                        {
                            returnInput.LocationTo = orderMaster.LocationTo;
                        }
                        returnInput.OrderNo = orderMaster.OrderNo;
                        returnInput.Qty = orderBomDetail.FeedQty;
                        returnInput.Item = orderBomDetail.Item;
                        if (string.IsNullOrEmpty(orderBomDetail.Uom))
                        {
                            Item item = base.genericMgr.FindById<Item>(orderBomDetail.Item);
                            returnInput.Uom = item.Uom;
                        }
                        else
                        {
                            returnInput.Uom = orderBomDetail.Uom;
                        }
                        returnInput.QualityType = com.Sconit.CodeMaster.QualityType.Qualified;
                        returnInputList.Add(returnInput);
                    }
                }
                #endregion

                if (returnInputList.Count == 0)
                {
                    throw new BusinessException("退料明细为空");
                }

                productionLineMgr.ReturnRawMaterial(orderMaster.OrderNo, orderMaster.TraceCode, null, null, returnInputList);
                SaveSuccessMessage(Resources.PRD.ProductLineLocationDetail.OrderMasterLocationDetail_ReturnOut);
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

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Production_MaterialIn_ReturnOrderMaster")]
        public ActionResult _ReturnQtyDetailList(GridCommand command, ProductLineLocationDetailSearchModel searchModel)
        {
            //#region combox
            //IList<Uom> uoms = base.genericMgr.FindAll<Uom>();
            //ViewData.Add("uoms", uoms);
            //#endregion

            //#region 选默认库位的
            //string ordercheckStr = SecurityHelper.CheckOrderStatement(orderNo, com.Sconit.CodeMaster.OrderType.Production);
            //OrderMaster orderMaster = base.genericMgr.FindAll<OrderMaster>(ordercheckStr).SingleOrDefault<OrderMaster>();
            //if (orderMaster != null)
            //{
            //    ViewBag.Region = orderMaster.PartyFrom;
            //}
            //#endregion
            ViewBag.PageSize = this.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _SelectReturnQtyBatchEditing(GridCommand command, ProductLineLocationDetailSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareSearchReturnStatement(command, searchModel);
            return PartialView(GetAjaxPageData<ProductLineLocationDetail>(searchStatementModel, command));
            // return PartialView(new GridModel(new List<OrderBomDetail>()));
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_Production_MaterialIn_ReturnOrderMaster")]
        public JsonResult _ReturnQty(string OrderNo, string IdStr, string CurrentReturnQtyStr)
        {
            try
            {
                if (string.IsNullOrEmpty(OrderNo))
                {
                    throw new BusinessException("生产单不能为空");
                }

                string ordercheckStr = SecurityHelper.CheckOrderStatement(OrderNo, com.Sconit.CodeMaster.OrderType.Production);
                OrderMaster orderMaster = base.genericMgr.FindAll<OrderMaster>(ordercheckStr).SingleOrDefault<OrderMaster>();

                if (orderMaster == null)
                {
                    throw new BusinessException("生产单不正确或没有权限");
                }

                #region ProdLineLocationDetList
                string[] idArr = IdStr.Split(',');
                string[] currentReturnQtyArr = CurrentReturnQtyStr.Split(',');
                IList<ReturnInput> returnInputList = new List<ReturnInput>();
                // IList<ProductLineLocationDetail> prodLineLocationDetList = new List<ProductLineLocationDetail>();
                string hql = string.Empty;
                IList<object> pare = new List<object>();
                //foreach (string id in idArr)
                //{
                //    if (string.IsNullOrEmpty(hql))
                //    {
                //        hql = "select p from ProductLineLocationDetail as p where p.Id in (?";
                //    }
                //    else {
                //        hql += ",?";
                //    }
                //    pare.Add(id);
                //}
                //hql += ")";
                //prodLineLocationDetList = base.genericMgr.FindAll<ProductLineLocationDetail>(hql,pare.ToArray());
                //if (prodLineLocationDetList!=null && prodLineLocationDetList.Count > 0)
                //{
                //    #region 退货数
                //    foreach (ProductLineLocationDetail productLineLocationDetail in prodLineLocationDetList)
                //    {
                //        for(int i=0;i<idArr.Length;i++)
                //        {
                //            if (idArr[i] == productLineLocationDetail.Id.ToString()) { 
                //                productLineLocationDetail.CurrentReturnQty=int.Parse(currentReturnQtyArr[i]);
                //            }
                //        }
                //    }
                //    #endregion

                //    foreach (ProductLineLocationDetail productLineLocationDetail in prodLineLocationDetList)
                //    {
                //        ReturnInput returnInput = new ReturnInput();
                //        if (!string.IsNullOrEmpty(productLineLocationDetail.LocationFrom))
                //        {
                //            string locationcheckStr = "from Location as l where l.Region = ? and l.Code = ?";
                //            Location location = base.genericMgr.FindAll<Location>(locationcheckStr, new object[] { orderMaster.PartyFrom, productLineLocationDetail.LocationFrom }).SingleOrDefault<Location>();
                //            if (location == null)
                //            {
                //                throw new BusinessException("物料" + productLineLocationDetail.Item + "的库位不正确");
                //            }
                //            returnInput.LocationTo = productLineLocationDetail.LocationFrom;
                //        }
                //        else
                //        {
                //            returnInput.LocationTo = orderMaster.LocationTo;
                //        }
                //        returnInput.OrderNo = orderMaster.OrderNo;
                //        returnInput.Qty = productLineLocationDetail.Qty;
                //        returnInput.Item = productLineLocationDetail.Item;
                //        returnInput.UnitQty = 1;
                //        if (string.IsNullOrEmpty(productLineLocationDetail.Uom))
                //        {
                //            Item item = base.genericMgr.FindById<Item>(productLineLocationDetail.Item);
                //            returnInput.Uom = item.Uom;
                //        }
                //        else
                //        {
                //            returnInput.Uom = productLineLocationDetail.Uom;
                //        }
                //        returnInput.QualityType = com.Sconit.CodeMaster.QualityType.Qualified;
                //        returnInput.ProductLineLocationDetailId = productLineLocationDetail.Id;
                //        returnInputList.Add(returnInput);
                //    }
                //}
                #endregion
                #region
                for (int i = 0; i < idArr.Length; i++)
                {
                    ReturnInput returnInput = new ReturnInput();
                    returnInput.ProductLineLocationDetailId = int.Parse(idArr[i]);
                    returnInput.Qty = Convert.ToDecimal(currentReturnQtyArr[i]);
                    returnInput.UnitQty = 1;
                    returnInputList.Add(returnInput);
                }

                #endregion

                if (returnInputList.Count == 0)
                {
                    throw new BusinessException("退料明细为空");
                }

                productionLineMgr.ReturnRawMaterial(returnInputList);
                SaveErrorMessage(Resources.PRD.ProductLineLocationDetail.OrderMasterLocationDetail_ReturnOut);
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

        #region batch
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_BatchProcess")]
        public ActionResult BatchProcessIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_BatchProcess")]
        public ActionResult BatchProcessList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Submit")]
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

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Start")]
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
                        if (IsVanOrder(orderNo))
                        {
                            orderMgr.StartVanOrder(orderNo);
                        }
                        else
                        {
                            orderMgr.StartOrder(orderNo);
                        }
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

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Delete")]
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

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Cancel")]
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

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_Close")]
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

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_BatchProcess")]
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

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_BatchProcess")]
        public string BatchPrint(string orderStr)
        {
            if (string.IsNullOrEmpty(orderStr))
            {
                SaveErrorMessage("请选择一条明细");
                return "";
            }
            else
            {
                string[] orderArray = orderStr.Split(',');

                //IList<object> datas = new List<object>();
                string returnFileUrl = string.Empty;
                foreach (string orderNo in orderArray)
                {
                    OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);
                    IList<OrderDetail> orderDetails = base.genericMgr.FindAll<OrderDetail>("select od from OrderDetail as od where od.OrderNo=?", orderNo);
                    IList<OrderBomDetail> orderBomDetails = base.genericMgr.FindAll<OrderBomDetail>("select bm from OrderBomDetail as bm where bm.OrderNo=?", orderNo);
                    IList<PrintOrderBomDetail> printOrderBomDetails = Mapper.Map<IList<OrderBomDetail>, IList<PrintOrderBomDetail>>(orderBomDetails);
                    orderMaster.OrderDetails = orderDetails;
                    PrintOrderMaster printOrderMstr = Mapper.Map<OrderMaster, PrintOrderMaster>(orderMaster);
                    IList<object> data = new List<object>();
                    data.Add(printOrderMstr);
                    data.Add(printOrderMstr.OrderDetails);
                    data.Add(printOrderBomDetails);
                    // datas.Add(data);
                    // string reportFileUrl = reportGen.WriteToFile(orderMaster.OrderTemplate, data);
                    //reportGen.WriteToClient(orderMaster.OrderTemplate, data, orderMaster.OrderTemplate);

                    // return reportFileUrl;
                    string reportFileUrl = reportGen.WriteToFile(orderMaster.OrderTemplate, data);
                    returnFileUrl += reportFileUrl + "*";
                }
                // string reportFileUrl = reportGen.WriteToFile("ORD_Production.xls", datas);
                return returnFileUrl != "" ? returnFileUrl.Substring(0, returnFileUrl.Length - 1) : returnFileUrl;

            }
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_BatchProcess")]
        public ActionResult _AjaxBatchProcessList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            string whereStatement = " and o.ProdLineType not in (1,2,3,4,9)";
            searchModel.SubType = (int)com.Sconit.CodeMaster.OrderSubType.Normal;
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchStatement_1(command, searchModel, whereStatement, false);
            // return PartialView(GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command));
            GridModel<OrderMaster> orderGridList = GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command);
            if (orderGridList.Data != null && orderGridList.Data.Count() > 0)
            {
                foreach (var orderMaster in orderGridList.Data)
                {
                    var orderdetail = base.genericMgr.FindAll<OrderDetail>("select d from OrderDetail as d where d.OrderNo=?", orderMaster.OrderNo).First();
                    orderMaster.Item = orderdetail.Item;
                    orderMaster.OrderedQty = orderdetail.OrderedQty;
                }

            }

            return PartialView(orderGridList);
        }
        #endregion

        #region pl pause
        [SconitAuthorize(Permissions = "Url_Production_ProductLine_Pause")]
        public ActionResult ProductLinePause()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_Production_ProductLine_Pause")]
        public ActionResult ProductLinePauseList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            this.ProcessSearchModel(command, searchModel);
            return View();
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_Production_ProductLine_Pause")]
        public ActionResult _PauseFlowList(string flow)
        {
            ViewBag.flow = flow;
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Production_ProductLine_Pause")]
        public ActionResult _AjaxPauseFlowList(string flow)
        {
            IList<FlowMaster> productLineList = new List<FlowMaster>();
            IList<object> para = new List<object>();
            string flowStr = "select f from FlowMaster as f where  f.Type in (" + (int)com.Sconit.CodeMaster.OrderType.Production + ") and f.IsActive = " + true;
            if (!string.IsNullOrEmpty(flow))
            {
                flowStr += " and f.Code like ? ";
                para.Add(flow + "%");
            }
            productLineList = base.genericMgr.FindAll<FlowMaster>(flowStr, para.ToArray());
            return View(new GridModel<FlowMaster>(productLineList));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Production_ProductLine_Pause")]
        public JsonResult BatchPause(string flowStr, bool isPause)
        {
            try
            {
                string[] flowArray = flowStr.Split(',');
                foreach (string f in flowArray)
                {
                    if (isPause)
                    {
                        this.productionLineMgr.PauseProductLine(f);
                        SaveSuccessMessage("生产线{0}暂停成功。", f);
                    }
                    else
                    {
                        this.productionLineMgr.ReStartProductLine(f);
                        SaveSuccessMessage("生产线{0}恢复成功。", f);
                    }

                    return Json(new { });
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

        #region pause

        #region single pause
        [SconitAuthorize(Permissions = "Url_Production_OrderMaster_Pause")]
        public ActionResult PauseIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Production_OrderMaster_Pause")]
        public ActionResult PauseList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            TempData["OrderMasterSearchModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Production_OrderMaster_Pause")]
        public ActionResult _AjaxPauseOrderList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            searchModel.IsPause = (int)Sconit.CodeMaster.PauseStatus.None;
            string whereStatement = " and o.Type = " + (int)com.Sconit.CodeMaster.OrderType.Production +
                                    " and o.SubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal +
                                    " and o.Status in (" + (int)com.Sconit.CodeMaster.OrderStatus.Submit + "," + (int)com.Sconit.CodeMaster.OrderStatus.InProcess + ")" +
                                    " and o.ProdLineType in (" + (int)Sconit.CodeMaster.ProdLineType.Special + "," + (int)Sconit.CodeMaster.ProdLineType.Cab + "," +
                                    (int)Sconit.CodeMaster.ProdLineType.Chassis + "," + (int)Sconit.CodeMaster.ProdLineType.Assembly + ")" +
                                    " and o.PauseStatus = " + (int)Sconit.CodeMaster.PauseStatus.None;

            //SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel, whereStatement);
            //return PartialView(GetAjaxPageData<OrderMaster>(searchStatementModel, command));

            //SearchNativeSqlStatementModel searchNativeSqlStatementModel = PrepareSearchStatement1(command, searchModel, whereStatement);
            //return PartialView(GetPageDataEntityWithNativeSql<OrderMaster>(searchNativeSqlStatementModel, command));

            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareVanSearchStatement_1(command, searchModel, whereStatement, new SortDescriptor { Member = "Seq asc,SubSeq ", SortDirection = ListSortDirection.Ascending }, false);
            GridModel<OrderMaster> orderGridList = GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command);
            return PartialView(orderGridList);
        }

        [SconitAuthorize(Permissions = "Url_Production_OrderMaster_Pause")]
        public JsonResult PopPauseOrder(string orderNo)
        {
            try
            {
                OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);
                orderMgr.PauseProductOrder(orderNo, null);
                SaveSuccessMessage(string.Format(Resources.ORD.OrderMaster.OrderMaster_Paused, orderNo));
                return Json(new { orderNo = orderNo, Routing = orderMaster.Routing, CurrentOperation = orderMaster.CurrentOperation, status = (int)orderMaster.Status });
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



        [SconitAuthorize(Permissions = "Url_Production_OrderMaster_Pause")]
        public JsonResult Pause(string orderNo, int? pauseOperation)
        {
            try
            {
                if (pauseOperation == null)
                {
                    throw new BusinessException("工序不能为空");
                }
                orderMgr.PauseProductOrder(orderNo, pauseOperation);
                SaveSuccessMessage("生产单{0}暂停成功。", orderNo);
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

        #region batch Pause
        public JsonResult CheckOrderStatus(string orderNos)
        {
            int existsProcessStart = this.genericMgr.FindAllWithNativeSql<int>(" select isnull(count(*),0) as counts from ORD_OrderMstr_4 WITH(NOLOCK) where status=" + (int)com.Sconit.CodeMaster.OrderStatus.InProcess + " and orderno in (" + orderNos + ") ")[0];
            return Json(new { exists = (existsProcessStart > 0) });
        }

        [SconitAuthorize(Permissions = "Url_Production_OrderMaster_Pause")]
        public JsonResult BatchPauseProductOrder(string orderNos, int? pauseOp)
        {
            try
            {
                orderMgr.BatchPauseProductOrder(orderNos, pauseOp);
                SaveSuccessMessage("生产单{0}暂停成功。", orderNos);
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
        #endregion

        #region Resume

        [SconitAuthorize(Permissions = "Url_Production_OrderMaster_Resume")]
        public ActionResult ResumeIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Production_OrderMaster_Resume")]
        public ActionResult ResumeList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            if (!string.IsNullOrWhiteSpace(searchModel.successMesage))
            {
                SaveSuccessMessage(searchModel.successMesage);
            }
            return View();
        }


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Production_OrderMaster_Resume")]
        public ActionResult _AjaxResumeOrderList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            searchModel.IsPause = (int)Sconit.CodeMaster.PauseStatus.Paused;
            string whereStatement = " where o.Type = " + (int)com.Sconit.CodeMaster.OrderType.Production +
                                    " and o.SubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal +
                                    " and o.Status in (" + (int)com.Sconit.CodeMaster.OrderStatus.Create + "," + (int)com.Sconit.CodeMaster.OrderStatus.Submit + "," + (int)com.Sconit.CodeMaster.OrderStatus.InProcess + ")" +
                                   " and o.ProdLineType in (" + (int)Sconit.CodeMaster.ProdLineType.Special + "," + (int)Sconit.CodeMaster.ProdLineType.Cab + ","
                                    + (int)Sconit.CodeMaster.ProdLineType.Chassis + "," + (int)Sconit.CodeMaster.ProdLineType.Assembly + ")";
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel, whereStatement);
            return PartialView(GetAjaxPageData<OrderMaster>(searchStatementModel, command));
        }

        [SconitAuthorize(Permissions = "Url_Production_OrderMaster_Pause")]
        public ActionResult PopResumeOrder(string orderNo)
        {
            return Json(new { orderNo = orderNo });
        }

        [SconitAuthorize(Permissions = "Url_Production_OrderMaster_Pause")]
        public JsonResult Resume(string orderNo, string sequence, bool IsForceResume)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sequence))
                {
                    throw new BusinessException("序号不能为空");
                }
                orderMgr.RestartProductOrder(orderNo, sequence, IsForceResume);
                SaveSuccessMessage("生产单{0}恢复成功。", orderNo);
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

        #region SelectResumeOrderNo
        public ActionResult _SelectResumeOrderNo(string currentOrderNo)
        {
            com.Sconit.Entity.ACC.User user = SecurityContextHolder.Get();
            ViewBag.IsForceResume = user.Permissions.Where(p => p.PermissionCode == "Url_Production_ForceResume").Count() > 0;
            ViewBag.CurrentOrderNo = currentOrderNo;
            ViewBag.Flow = this.genericMgr.FindById<OrderMaster>(currentOrderNo).Flow;
            return PartialView();
        }

        [GridAction]
        public ActionResult _SelectResumeOrderNoList(string OrderNO, string TraceCode, string Flow, string currentOrderNo, bool IsForceResume)
        {
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _SelectResumeOrderAjaxList(GridCommand command, string OrderNo, string TraceCode, string Flow, string currentOrderNo, bool IsForceResume)
        {
            string currentTraceCode = this.genericMgr.FindById<OrderMaster>(currentOrderNo).TraceCode;

            IList<object> param = new List<object>();
            string sql = @" select om.* from ORD_OrderMstr_4 as om WITH(NOLOCK) left join ORD_OrderSeq as os WITH(NOLOCK) ON om.TraceCode=os.TraceCode AND om.Flow=os.ProdLine AND om.OrderNo=os.OrderNo  
                            where om.Status not in(?,?,?) and  om.PauseStatus <>? and om.Flow=? ";
            //string hql = "select o from OrderMaster as o where Status not in (?,?,?) and PauseStatus <>?  and Flow=? ";
            //string hql = "select o from OrderMaster as o where Status not in (?,?,?) and PauseStatus <>?  and Flow=? and  o.Status<= ( select m.Status from OrderMaster as m where m.OrderNo=?  )";
            param.Add((int)com.Sconit.CodeMaster.OrderStatus.Close);
            param.Add((int)com.Sconit.CodeMaster.OrderStatus.Complete);
            param.Add((int)com.Sconit.CodeMaster.OrderStatus.Cancel);
            param.Add((int)com.Sconit.CodeMaster.PauseStatus.Paused);
            param.Add(Flow);
            if (!string.IsNullOrWhiteSpace(TraceCode))
            {
                sql += " and om.TraceCode=? ";
                param.Add(TraceCode);
            }
            if (!string.IsNullOrWhiteSpace(OrderNo))
            {
                sql += " and om.OrderNo=? ";
                param.Add(OrderNo);
            }
            if (!IsForceResume)
            {
                var orderSeqs = this.genericMgr.FindEntityWithNativeSql<OrderSeq>(@"select top 1 seq.* from SCM_SeqGroup as sg
			                                                                left join ORD_SeqOrderTrace as trace on trace.TraceCode = ? and sg.Code = trace.SeqGroup
			                                                                inner join ORD_OrderSeq as seq on sg.PrevSeq = seq.Seq and sg.PrevSubSeq = seq.SubSeq and sg.ProdLine = seq.ProdLine
			                                                                where sg.ProdLine = ? and trace.Id is null 
			                                                                order by seq.Seq desc, seq.SubSeq desc", new object[] { currentTraceCode, Flow });
                if (orderSeqs != null && orderSeqs.Count > 0)
                {
                    sql += " and os.Seq>=? ";
                    param.Add(orderSeqs.FirstOrDefault().Sequence);
                }
            }


            sql += "  order by os.Seq asc,os.SubSeq asc ";
            IList<OrderMaster> returnList = this.genericMgr.FindEntityWithNativeSql<OrderMaster>(sql, param.ToArray());
            GridModel<OrderMaster> gridModel = new GridModel<OrderMaster>();
            gridModel.Total = returnList.Count();
            returnList = returnList.Skip((command.Page - 1) * 50).Take(50).ToList();
            this.FillCodeDetailDescription(returnList);
            gridModel.Data = returnList;
            return PartialView(gridModel);

        }

        #endregion

        #region 打印导出
        public void SaveToClient(string orderNo)
        {
            OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);
            IList<OrderDetail> orderDetails = base.genericMgr.FindAll<OrderDetail>("select od from OrderDetail as od where od.OrderNo=?", orderNo);
            IList<OrderBomDetail> orderBomDetails = base.genericMgr.FindAll<OrderBomDetail>("select bm from OrderBomDetail as bm where bm.OrderNo=?", orderNo);
            IList<PrintOrderBomDetail> printOrderBomDetails = Mapper.Map<IList<OrderBomDetail>, IList<PrintOrderBomDetail>>(orderBomDetails);
            orderMaster.OrderDetails = orderDetails;
            PrintOrderMaster printOrderMstr = Mapper.Map<OrderMaster, PrintOrderMaster>(orderMaster);
            IList<object> data = new List<object>();
            data.Add(printOrderMstr);
            data.Add(printOrderMstr.OrderDetails);
            data.Add(printOrderBomDetails);
            //string reportFileUrl = reportGen.WriteToFile(orderMaster.OrderTemplate, data);
            reportGen.WriteToClient(orderMaster.OrderTemplate, data, orderMaster.OrderTemplate);


            //return reportFileUrl;
            //reportGen.WriteToFile(orderMaster.OrderTemplate, data);
        }

        public JsonResult Print(string orderNos)
        {
            var printUrl = string.Empty;

            var orderNoList = orderNos.Split(',');
            foreach (var orderNo in orderNoList)
            {
                OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);
                IList<OrderDetail> orderDetails = base.genericMgr.FindAll<OrderDetail>("select od from OrderDetail as od where od.OrderNo=?", orderNo);
                IList<OrderBomDetail> orderBomDetails = base.genericMgr.FindAll<OrderBomDetail>("select bm from OrderBomDetail as bm where bm.OrderNo=?", orderNo);
                IList<PrintOrderBomDetail> printOrderBomDetails = Mapper.Map<IList<OrderBomDetail>, IList<PrintOrderBomDetail>>(orderBomDetails);
                orderMaster.OrderDetails = orderDetails;
                PrintOrderMaster printOrderMstr = Mapper.Map<OrderMaster, PrintOrderMaster>(orderMaster);
                IList<object> data = new List<object>();
                data.Add(printOrderMstr);
                data.Add(printOrderMstr.OrderDetails);
                data.Add(printOrderBomDetails);
                printUrl += reportGen.WriteToFile(orderMaster.OrderTemplate, data) + ",";
            }

            //reportGen.WriteToClient(orderMaster.OrderTemplate, data, orderMaster.OrderTemplate);

            return Json(new { PrintUrl = printUrl.TrimEnd(',') });
            //reportGen.WriteToFile(orderMaster.OrderTemplate, data);
        }

        #region 导出明细
        public ActionResult SaveOrderDetailViewToClient()
        {
            ProcedureSearchStatementModel procedureSearchStatementModel = TempData["OrderMasterPrintSearchModel"] as ProcedureSearchStatementModel;
            if (procedureSearchStatementModel == null)
            {
                SaveErrorMessage("请先查询再导出数据。");
                return RedirectToAction("DetailIndex/");
            }
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

                                }).ToList();
                #endregion
            }

            // IList<PrintOrderDetail> printOrderetails = Mapper.Map<IList<OrderDetail>, IList<PrintOrderDetail>>(orderDetList);
            IList<object> data = new List<object>();
            data.Add(orderDetList);
            reportGen.WriteToClient("ProductOrderDetView.xls", data, "ProductOrderDetView.xls");
            return null;
        }
        #endregion

        #region 整车生产单导出

        public void ExportVanOrderMasterXLS(OrderMasterSearchModel searchModel)
        {
            string sql = @"select m.OrderNo,m.Flow,m.FlowDesc,d.Item,m.TraceCode,m.ExtOrderNo,m.PartyFromNm,m.StartDate,m.CompleteDate,Status,m.CreateUserNm,m.CreateDate,m.CurtOp,m.PauseSeq,m.PauseTime,m.PauseStatus,m.StartUserNm,m.CompleteUserNm from ORD_OrderMstr_4 as m WITH(NOLOCK) inner join ORD_OrderDet_4 as d WITH(NOLOCK) on m.OrderNo=d.OrderNo  where 1=1
and m.ProdLineType in (1,2,3,4,9) and m.OrderStrategy <>4 and m.SubType=0 ";
            IList<object> param = new List<object>();
            if (!string.IsNullOrWhiteSpace(searchModel.Item))
            {
                sql += " and  d.Item=?";
                param.Add(searchModel.Item);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.OrderNo))
            {
                sql += " and  m.OrderNo = ?";
                param.Add(searchModel.OrderNo);
            }
            if (searchModel.IsPause.HasValue)
            {
                sql += " and m.PauseStatus =" + searchModel.IsPause.Value;
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Flow))
            {
                sql += " and  m.Flow = ?";
                param.Add(searchModel.Flow);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.PartyFrom))
            {
                sql += " and  m.PartyFrom = ?";
                param.Add(searchModel.PartyFrom);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.PartyTo))
            {
                sql += " and  m.PartyTo = ?";
                param.Add(searchModel.PartyTo);
            }
            //if (searchModel.Status.HasValue)
            //{
            //    sql += " and m.Status =" + searchModel.Status.Value;
            //}
            if (!string.IsNullOrWhiteSpace(searchModel.MultiStatus))
            {
                string statusSql = " and m.Status in( ";
                string[] statusArr = searchModel.MultiStatus.Split(',');
                for (int st = 0; st < statusArr.Length; st++)
                {
                    statusSql += "'" + statusArr[st] + "',";
                }
                statusSql = statusSql.Substring(0, statusSql.Length - 1) + ")";
                sql += statusSql;
            }
            if (searchModel.Priority.HasValue)
            {
                sql += " and m.Priority =" + searchModel.Priority.Value;
            }
            if (!string.IsNullOrWhiteSpace(searchModel.ExternalOrderNo))
            {
                sql += " and  m.ExternalOrderNo = ?";
                param.Add(searchModel.ExternalOrderNo.PadLeft(12, '0'));
            }
            if (!string.IsNullOrWhiteSpace(searchModel.ReferenceOrderNo))
            {
                sql += " and  m.ReferenceOrderNo like ?";
                param.Add(searchModel.ReferenceOrderNo + "%");
            }
            if (!string.IsNullOrWhiteSpace(searchModel.TraceCode))
            {
                sql += " and  m.TraceCode = ?";
                param.Add(searchModel.TraceCode);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.CreateUserName))
            {
                sql += " and  m.CreateUserName like ?";
                param.Add(searchModel.CreateUserName + "%");
            }
            if (searchModel.DateFrom.HasValue)
            {
                sql += " and m.CreateDate >=?";
                param.Add(searchModel.DateFrom.Value);
            }
            if (searchModel.DateTo.HasValue)
            {
                sql += " and m.CreateDate <=?";
                param.Add(searchModel.DateTo.Value);
            }
            if (searchModel.StartTime.HasValue)
            {
                sql += " and m.StartDate >=?";
                param.Add(searchModel.StartTime.Value);
            }
            if (searchModel.EndTime.HasValue)
            {
                sql += " and m.StartDate <=?";
                param.Add(searchModel.EndTime.Value);
            }
            if (searchModel.WindowTimeFrom.HasValue)
            {
                sql += " and m.CompleteDate >=?";
                param.Add(searchModel.WindowTimeFrom.Value);
            }
            if (searchModel.WindowTimeTo.HasValue)
            {
                sql += " and m.CompleteDate <=?";
                param.Add(searchModel.WindowTimeTo.Value);
            }
            var user = SecurityContextHolder.Get();
            if (user.Code.ToUpper() != "SU")
            {
                sql += " AND (m.PartyFrom IN (SELECT PermissionCode FROM #Temp) OR m.PartyTo IN (SELECT PermissionCode FROM #Temp)) ";
            }
            string permissionSql = "SELECT PermissionCode INTO #Temp FROM VIEW_UserPermission WHERE UserId=" + user.Id;
            IList<object[]> searchList = this.genericMgr.FindAllWithNativeSql<object[]>(permissionSql + ";" + sql + ";drop table #Temp", param.ToArray());
            //m.OrderNo,m.Flow,m.FlowDesc,d.Item,m.TraceCode,m.ExtOrderNo,m.PartyFromNm,m.StartDate
            //,m.ReleaseDate,Status,m.CreateUserNm,m.CreateDate,m.CurtOp,m.PauseSeq,m.PauseTime,
            //m.PauseStatus,m.StartUserNm,m.CompleteUserNm
            IList<OrderMaster> exportMasterList = (from tak in searchList
                                                   select new OrderMaster
                                                   {
                                                       OrderNo = (string)tak[0],
                                                       Flow = (string)tak[1],
                                                       FlowDescription = (string)tak[2],
                                                       Item = (string)tak[3],
                                                       TraceCode = (string)tak[4],
                                                       ExternalOrderNo = (string)tak[5],
                                                       PartyFromName = (string)tak[6],
                                                       StartDate = (DateTime?)tak[7],
                                                       CompleteDate = (DateTime?)tak[8],
                                                       OrderStatusDescription = systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, (tak[9]).ToString()),
                                                       CreateUserName = (string)tak[10],
                                                       CreateDate = (DateTime)tak[11],
                                                       CurrentOperation = (int?)tak[12],
                                                       PauseSequence = (int)tak[13],
                                                       PauseTime = (DateTime?)tak[14],
                                                       PauseStatusDescription = systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.PauseStatus, (tak[15]).ToString()),
                                                       StartUserName = (string)tak[16],
                                                       CompleteUserName = (string)tak[17],
                                                   }).ToList();
            //this.FillCodeDetailDescription<Item>(itemList);
            ExportToXLS<OrderMaster>("ExportVanOrder", "xls", exportMasterList);
        }

        #endregion

        #region 非整车生产单导出

        public void ExportProductionOrderXLS(OrderMasterSearchModel searchModel)
        {
            string sql = @"select m.OrderNo,m.Flow,m.FlowDesc,d.Item,d.OrderQty,m.RefOrderNo,m.ExtOrderNo,m.PartyFromNm,m.StartUserNm,m.StartDate,m.CompleteUserNm,m.CompleteDate,Status,m.CreateUserNm,m.CreateDate,m.CurtOp,m.TraceCode,m.Type from ORD_OrderMstr_4 as m WITH(NOLOCK) inner join ORD_OrderDet_4 as d WITH(NOLOCK) on m.OrderNo=d.OrderNo  where 1=1
and m.ProdLineType not in (1,2,3,4,9) and m.OrderStrategy <>4 and m.SubType=0 ";
            IList<object> param = new List<object>();

            if (!string.IsNullOrWhiteSpace(searchModel.OrderNo))
            {
                sql += " and  m.OrderNo = ?";
                param.Add(searchModel.OrderNo);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Flow))
            {
                sql += " and  m.Flow = ?";
                param.Add(searchModel.Flow);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.PartyFrom))
            {
                sql += " and  m.PartyFrom = ?";
                param.Add(searchModel.PartyFrom);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.TraceCode))
            {
                sql += " and  m.TraceCode = ?";
                param.Add(searchModel.TraceCode);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.ExternalOrderNo))
            {
                sql += " and  m.ExternalOrderNo =?";
                param.Add(searchModel.ExternalOrderNo.PadLeft(12, '0'));
            }
            if (!string.IsNullOrWhiteSpace(searchModel.ReferenceOrderNo))
            {
                sql += " and  m.ReferenceOrderNo like ?";
                param.Add(searchModel.ReferenceOrderNo + "%");
            }
            //if (searchModel.Status.HasValue)
            //{
            //    sql += " and m.Status =" + searchModel.Status.Value;
            //}
            if (!string.IsNullOrWhiteSpace(searchModel.MultiStatus))
            {
                string statusSql = " and m.Status in( ";
                string[] statusArr = searchModel.MultiStatus.Split(',');
                for (int st = 0; st < statusArr.Length; st++)
                {
                    statusSql += "'" + statusArr[st] + "',";
                }
                statusSql = statusSql.Substring(0, statusSql.Length - 1) + ")";
                sql += statusSql;
            }
            if (!string.IsNullOrWhiteSpace(searchModel.CreateUserName))
            {
                sql += " and  m.CreateUserName like ?";
                param.Add(searchModel.CreateUserName + "%");
            }
            if (searchModel.DateFrom.HasValue)
            {
                sql += " and m.CreateDate >=?";
                param.Add(searchModel.DateFrom.Value);
            }
            if (searchModel.DateTo.HasValue)
            {
                sql += " and m.CreateDate <?";
                param.Add(searchModel.DateTo.Value.AddDays(1));
            }
            if (searchModel.StartTime.HasValue)
            {
                sql += " and m.StartDate >=?";
                param.Add(searchModel.StartTime.Value);
            }
            if (searchModel.EndTime.HasValue)
            {
                sql += " and m.StartDate <?";
                param.Add(searchModel.EndTime.Value.AddDays(1));
            }
            if (searchModel.WindowTimeFrom.HasValue)
            {
                sql += " and m.CompleteDate >=?";
                param.Add(searchModel.WindowTimeFrom.Value);
            }
            if (searchModel.WindowTimeTo.HasValue)
            {
                sql += " and m.CompleteDate <?";
                param.Add(searchModel.WindowTimeTo.Value.AddDays(1));
            }
            if (searchModel.Type.HasValue)
            {
                sql += " and m.Type =" + searchModel.Type.Value;
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Dock))
            {
                sql += " and  m.Dock = ?";
                param.Add(searchModel.Dock);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Item))
            {
                sql += " and  d.Item=?";
                param.Add(searchModel.Item);
            }
            var user = SecurityContextHolder.Get();
            if (user.Code.ToUpper() != "SU")
            {
                sql += " AND (m.PartyFrom IN (SELECT PermissionCode FROM #Temp) OR m.PartyTo IN (SELECT PermissionCode FROM #Temp)) ";
            }
            string permissionSql = "SELECT PermissionCode INTO #Temp FROM VIEW_UserPermission WHERE UserId=" + user.Id;
            IList<object[]> searchList = this.genericMgr.FindAllWithNativeSql<object[]>(permissionSql + ";" + sql + ";drop table #Temp", param.ToArray());
            //m.OrderNo,m.Flow,m.FlowDesc,d.Item,d.OrderedQty,m.RefOrderNo,m.ExtOrderNo,m.PartyFromNm,m.StartUserNm,m.StartDate,m.CompleteUserNm,
            //m.CompleteDate,Status,m.CreateUserNm,m.CreateDate,m.CurtOp,m.TraceCode,m.Type
            IList<OrderMaster> exportMasterList = (from tak in searchList
                                                   select new OrderMaster
                                                   {
                                                       OrderNo = (string)tak[0],
                                                       Flow = (string)tak[1],
                                                       FlowDescription = (string)tak[2],
                                                       Item = (string)tak[3],
                                                       OrderedQty = (decimal)tak[4],
                                                       ReferenceOrderNo = (string)tak[5],
                                                       ExternalOrderNo = (string)tak[6],
                                                       PartyFromName = (string)tak[7],
                                                       StartUserName = (string)tak[8],
                                                       StartDate = (DateTime?)tak[9],
                                                       CompleteUserName = (string)tak[10],
                                                       CompleteDate = (DateTime?)tak[11],
                                                       OrderStatusDescription = systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, (tak[12]).ToString()),
                                                       CreateUserName = (string)tak[13],
                                                       CreateDate = (DateTime)tak[14],
                                                       CurrentOperation = (int?)tak[15],
                                                       TraceCode = (string)tak[16],
                                                       OrderTypeDescription = systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderType, (tak[17]).ToString()),
                                                   }).ToList();
            //this.FillCodeDetailDescription<Item>(itemList);
            ExportToXLS<OrderMaster>("ProductionOrder", "xls", exportMasterList);
        }

        #endregion

        #endregion

        #region 分装生产单下线
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_FZComplete")]
        public ActionResult FZComplete()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_FZComplete")]
        public void OrdeNoScan(string orderNo)
        {
            try
            {
                string hql = "select o from OrderMaster as o where o.Type = " + (int)com.Sconit.CodeMaster.OrderType.Transfer +
                                        " and o.SubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal +
                                        " and o.Status =" + (int)com.Sconit.CodeMaster.OrderStatus.InProcess +
                    //" and o.OrderStrategy =" + (int)com.Sconit.CodeMaster.FlowStrategy.KIT +
                                        " and o.OrderNo = ?";
                //SecurityHelper.AddPartyFromPermissionStatement(ref hql, "o", "PartyFrom", com.Sconit.CodeMaster.OrderType.Transfer, false);
                //SecurityHelper.AddPartyToPermissionStatement(ref hql, "o", "PartyTo", com.Sconit.CodeMaster.OrderType.Transfer);
                SecurityHelper.AddPartyFromAndPartyToPermissionStatement(ref hql, "o", "Type", "o", "PartyFrom", "o", "PartyTo", com.Sconit.CodeMaster.OrderType.Transfer, false);
                IList<OrderMaster> orderList = base.genericMgr.FindAll<OrderMaster>(hql, orderNo);
                if (orderList == null || orderList.Count == 0)
                {
                    throw new BusinessException("没有找到对应的分装生产单");
                }
            }
            catch (BusinessException ex)
            {
                //Response.TrySkipIisCustomErrors = true;
                //Response.StatusCode = 500;
                //Response.Write(ex.GetMessages()[0].GetMessageString());
            }

        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_FZComplete")]
        public ActionResult _FZOrderDetailList(string orderNo)
        {
            IList<OrderDetail> orderDetailList = base.genericMgr.FindAll<OrderDetail>("select d from OrderDetail as d where d.OrderNo = ?", orderNo);
            return View(orderDetailList);
        }
        #endregion

        #region 整车生产单查询
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_VanView")]
        public ActionResult VanEdit(string orderNo)
        {
            if (string.IsNullOrWhiteSpace(orderNo))
            {
                return HttpNotFound();
            }
            else
            {
                return View("VanEdit", string.Empty, orderNo);
            }
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_VanView")]
        public ActionResult _VanEdit(string orderNo)
        {
            if (string.IsNullOrWhiteSpace(orderNo))
            {
                return HttpNotFound();
            }
            OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);
            ViewBag.flow = orderMaster.Flow;
            ViewBag.orderNo = orderMaster.OrderNo;
            ViewBag.Status = orderMaster.Status;
            ViewBag.editorTemplate = orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.Create ? "" : "ReadonlyTextBox";
            return PartialView(orderMaster);
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_VanView")]
        public ActionResult VanIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_VanView")]
        public ActionResult VanList(GridCommand command, OrderMasterSearchModel searchModel)
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
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_VanView")]
        public ActionResult _VanAjaxList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<OrderMaster>()));
            }
            string whereStatement = " and o.ProdLineType in (1,2,3,4,9)";
            if (searchModel.Item != null && searchModel.Item != string.Empty)
                whereStatement += " and exists(select 1 from OrderDetail as d where d.OrderNo=o.OrderNo and d.Item ='" + searchModel.Item + "')";
            whereStatement += " and o.OrderStrategy != " + (int)com.Sconit.CodeMaster.FlowStrategy.SEQ;
            searchModel.SubType = (int)com.Sconit.CodeMaster.OrderSubType.Normal;
            if (searchModel.IsPause.HasValue)
            {
                whereStatement += " and o.PauseStatus =" + searchModel.IsPause.Value;
            }
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareVanSearchStatement_1(command, searchModel, whereStatement, new SortDescriptor { Member = "Seq asc,SubSeq ", SortDirection = ListSortDirection.Ascending }, false);
            GridModel<OrderMaster> orderGridList = GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command);

            return PartialView(orderGridList);
        }

        private ProcedureSearchStatementModel PrepareVanSearchStatement_1(GridCommand command, OrderMasterSearchModel searchModel, string whereStatement, SortDescriptor defaultSort, bool isReturn)
        {
            if (!string.IsNullOrWhiteSpace(searchModel.MultiStatus))
            {
                string statusSql = " and o.Status in( ";
                string[] statusArr = searchModel.MultiStatus.Split(',');
                for (int st = 0; st < statusArr.Length; st++)
                {
                    statusSql += "'" + statusArr[st] + "',";
                }
                statusSql = statusSql.Substring(0, statusSql.Length - 1) + ")";
                whereStatement += statusSql;
            }
            searchModel.ExternalOrderNo = searchModel.ExternalOrderNo != null ? searchModel.ExternalOrderNo.PadLeft(12, '0') : null;
            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.OrderNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Flow, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter
            {
                Parameter = (int)com.Sconit.CodeMaster.OrderType.Production,
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

            string sortMember = null;
            var sortDirection = "";
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
                sortMember = command.SortDescriptors[0].Member;
                sortDirection = command.SortDescriptors[0].SortDirection == ListSortDirection.Descending ? "desc" : "asc";
            }
            else
            {
                if (defaultSort != null)
                {
                    sortMember = defaultSort.Member;
                    sortDirection = defaultSort.SortDirection == ListSortDirection.Descending ? "desc" : "asc";
                }
            }

            pageParaList.Add(new ProcedureParameter { Parameter = sortMember, Type = NHibernate.NHibernateUtil.String });
            pageParaList.Add(new ProcedureParameter { Parameter = sortDirection, Type = NHibernate.NHibernateUtil.String });
            pageParaList.Add(new ProcedureParameter { Parameter = command.PageSize, Type = NHibernate.NHibernateUtil.Int32 });
            pageParaList.Add(new ProcedureParameter { Parameter = command.Page, Type = NHibernate.NHibernateUtil.Int32 });

            var procedureSearchStatementModel = new ProcedureSearchStatementModel();
            procedureSearchStatementModel.Parameters = paraList;
            procedureSearchStatementModel.PageParameters = pageParaList;
            procedureSearchStatementModel.CountProcedure = "USP_Search_VanProductionOrderCount";
            procedureSearchStatementModel.SelectProcedure = "USP_Search_VanProductionOrder";

            return procedureSearchStatementModel;
        }



        #endregion

        #region 非整车生产单报工
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_NonVanReceive")]
        public ActionResult NonVanReceiveIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_NonVanReceive")]
        public ActionResult NonVanReceiveList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_NonVanReceive")]
        public ActionResult _NonVanReceiveAjaxList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            string whereStatement = " and o.Status = " + (int)com.Sconit.CodeMaster.OrderStatus.InProcess +
                                    " and o.SubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal +
                                    " and o.ProdLineType not in (1,2,3,4,9)" +
                                    " and exists (select 1 from OrderDetail as d where d.RecQty + d.ScrapQty  < d.OrderQty and d.OrderNo = o.OrderNo) ";
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchStatement_1(command, searchModel, whereStatement, false);

            var orderGridList = GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command);
            if (orderGridList.Data != null && orderGridList.Data.Count() > 0)
            {
                foreach (var orderMaster in orderGridList.Data)
                {
                    var orderdetail = base.genericMgr.FindAll<OrderDetail>("select d from OrderDetail as d where d.OrderNo=?", orderMaster.OrderNo).FirstOrDefault();
                    if (orderdetail == null) continue;
                    orderMaster.Item = orderdetail.Item;
                    orderMaster.ItemDescription = orderdetail.ItemDescription;
                    orderMaster.Uom = orderdetail.Uom;
                    orderMaster.OrderedQty = orderdetail.OrderedQty;
                    orderMaster.ReceivedQty = orderdetail.ReceivedQty;
                    orderMaster.ScrapQty = orderdetail.ScrapQty;
                }
            }
            return PartialView(orderGridList);
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_NonVanReceive")]
        public ActionResult _NonVanReceiveOrderOperationList(string orderNo)
        {
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_NonVanReceive")]
        public ActionResult _NonVanReceiveOrderOperationAjaxList(string orderNo)
        {
            var ops = base.genericMgr.FindAll<OrderOperation>("from OrderOperation where OrderNo = ? and NeedReport = 1", orderNo);
            return PartialView(new GridModel<OrderOperation> { Data = ops, Total = ops.Count });
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_NonVanReceive")]
        public ActionResult NonVanReceiveEdit(string orderNo)
        {
            OrderMaster order = base.genericMgr.FindById<OrderMaster>(orderNo);
            ViewBag.IsVanOrder = false;
            return View(order);
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_NonVanReceive")]
        public ActionResult _NonVanCurrentOrderOperationList(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return PartialView(new List<OrderOperation>());

            var list = base.genericMgr.FindAll<OrderOperation>("select p from OrderOperation as p where p.Id =" + id);

            var orderDets = base.genericMgr.FindAll<OrderDetail>(string.Format("from OrderDetail as d where d.Id in ({0})", string.Join(",", list.Select(c => c.OrderDetailId))));

            foreach (var op in list)
            {
                var det = orderDets.FirstOrDefault(c => c.Id == op.OrderDetailId);
                if (det == null) continue;
                op.Item = det.Item;
                op.ItemDescription = det.ItemDescription;
                op.Uom = det.Uom;
                op.UnitCount = det.UnitCount;
                op.OrderedQty = det.OrderedQty;
            }

            return PartialView(list);
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_NonVanReceive")]
        public JsonResult _NonVanReport(string orderNo, int? orderOpId, decimal? currentReportQty, decimal? currentScrapQty)
        {
            try
            {
                if (orderOpId == null)
                {
                    throw new BusinessException("请选择工作中心。");
                }

                if (currentReportQty <= 0 && currentScrapQty <= 0)
                {
                    throw new BusinessException("报工数和废品数不能都小于或等于0");
                }

                this.orderMgr.ReportOrderOp(Convert.ToInt32(orderOpId), Convert.ToDecimal(currentReportQty), Convert.ToDecimal(currentScrapQty));
                SaveSuccessMessage("订单{0}报工成功。", orderNo);
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

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_NonVanReceive")]
        public ActionResult _AjaxOrderOperationReportList(int orderOpId)
        {
            var list = this.genericMgr.FindAll<OrderOperationReport>("from OrderOperationReport as p where p.OrderOperationId=?", orderOpId);
            return PartialView(new GridModel<OrderOperationReport> { Data = list, Total = list.Count });
        }

        #endregion

        #region 非整车生产单取消报工
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_NonVanReceiveCancel")]
        public ActionResult NonVanReceiveCancelIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_NonVanReceive")]
        public ActionResult NonVanReceiveCancelList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_NonVanReceive")]
        public ActionResult _NonVanReceiveOrderOperationCancelAjaxList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            var sql = "select o.* from ORD_OrderOp as o WITH(NOLOCK) inner join ORD_OrderMstr_4 m WITH(NOLOCK) on o.OrderNo = m.OrderNo Where m.ProdLineType not in (1,2,3,4,9) and o.NeedReport = 1";
            var paramList = new List<object>();
            if (!string.IsNullOrWhiteSpace(searchModel.OrderNo))
            {
                sql += string.Format(" and m.OrderNo like '{0}'", searchModel.OrderNo + "%");
            }
            if (!string.IsNullOrWhiteSpace(searchModel.ExternalOrderNo))
            {
                sql += string.Format(" and m.ExtOrderNo like '{0}'", searchModel.ExternalOrderNo + "%");
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Flow))
            {
                sql += string.Format(" and m.Flow= '{0}'", searchModel.Flow);
            }
            var ops = base.genericMgr.FindEntityWithNativeSql<OrderOperation>(sql);
            return PartialView(new GridModel<OrderOperation> { Data = ops, Total = ops.Count });
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_NonVanReceive")]
        public JsonResult _NonVanReportCancel(int orderOpReportId, string orderNo, string extOrderNo)
        {
            try
            {
                this.orderMgr.CancelReportOrderOp(orderOpReportId);
                SaveSuccessMessage("工序报工取消成功。");
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

        #region 整车生产单取消报工
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_NonVanReceiveCancel")]
        public ActionResult VanReceiveCancelIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_NonVanReceive")]
        public ActionResult VanReceiveCancelList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_NonVanReceive")]
        public ActionResult _VanReceiveOrderOperationCancelAjaxList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            var sql = "select o.* from ORD_OrderOp as o WITH(NOLOCK) inner join ORD_OrderMstr_4 m WITH(NOLOCK) on o.OrderNo=m.OrderNo Where m.ProdLineType in (1,2,3,4,9) and NeedReport = 1 and WorkCenter is not null";
            var paramList = new List<object>();
            if (!string.IsNullOrWhiteSpace(searchModel.OrderNo))
            {
                sql += string.Format(" and m.OrderNo = '{0}'", searchModel.OrderNo);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.TraceCode))
            {
                sql += string.Format(" and m.TraceCode = '{0}'", searchModel.TraceCode);
            }
            var ops = base.genericMgr.FindEntityWithNativeSql<OrderOperation>(sql);
            return PartialView(new GridModel<OrderOperation> { Data = ops, Total = ops.Count });
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_NonVanReceive")]
        public JsonResult _VanReportCancel(int orderOpReportId, string orderNo, string extOrderNo)
        {
            try
            {
                this.orderMgr.CancelReportOrderOp(orderOpReportId);
                SaveSuccessMessage("工序报工取消成功。");
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

        #region 整车上线
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_StartVanOrder")]
        public ActionResult StartVanOrderIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_StartVanOrder")]
        public ActionResult StartVanOrderList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (string.IsNullOrWhiteSpace(searchModel.Flow) && string.IsNullOrWhiteSpace(searchModel.OrderNo))
            {
                SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.ORD.OrderMaster.OrderMaster_Flow_Production + "或" + Resources.ORD.OrderMaster.OrderMaster_OrderNo);
            }
            else
            {
                var search = this.ProcessSearchModel(command, searchModel);
            }

            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_StartVanOrder")]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxStartVanOrderList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (string.IsNullOrWhiteSpace(searchModel.Flow) && string.IsNullOrWhiteSpace(searchModel.OrderNo))
            {
                return PartialView(new GridModel { Data = new List<VanOrderSeqView>() });
            }

            SearchNativeSqlStatementModel searchStatementModel = PrepareStartVanOrderSearchStatement(command, searchModel);
            return PartialView(GetPageDataEntityWithNativeSql<VanOrderSeqView>(searchStatementModel, command));
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Production_StartVanOrder")]
        public JsonResult StartVanOrder(string orderNo)
        {
            try
            {
                orderMgr.StartVanOrder(orderNo);
                SaveSuccessMessage("整车" + orderNo + "上线成功。");
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

        private SearchNativeSqlStatementModel PrepareStartVanOrderSearchStatement(GridCommand command, OrderMasterSearchModel searchModel)
        {
            string whereStatement =
                "SELECT  os.Id, om.Flow,om.OrderNo ,om.TraceCode,om.ExtOrderNo,om.Status, om.PauseStatus, om.CurtOp,os.SubSeq,os.Seq " +
                "FROM ORD_OrderSeq os WITH(NOLOCK) LEFT JOIN ORD_OrderMstr_4 om WITH(NOLOCK) " +
                "ON om.TraceCode=os.TraceCode AND om.Flow=os.ProdLine AND om.OrderNo=os.OrderNo AND om.Status =1 " +
                "where om.ProdLineType in (1,2,3,4,9) and  om.PauseStatus=0 ";
            if (!string.IsNullOrWhiteSpace(searchModel.Flow))
            {
                whereStatement += string.Format(" and om.Flow= '{0}'", searchModel.Flow);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.OrderNo))
            {
                whereStatement += string.Format(" and om.OrderNo= '{0}'", searchModel.OrderNo);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.TraceCode))
            {
                whereStatement += string.Format(" and om.TraceCode= '{0}'", searchModel.TraceCode);
            }

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = "ORDER BY Seq ASC,SubSeq ASC";
            }

            SearchNativeSqlStatementModel searchStatementModel = new SearchNativeSqlStatementModel();
            searchStatementModel.SelectSql = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;

            return searchStatementModel;
        }

        #endregion
        #endregion

        #region private method
        private SearchStatementModel PrepareSearchStatement(GridCommand command, OrderMasterSearchModel searchModel, string whereStatement)
        {
            IList<object> param = new List<object>();

            //SecurityHelper.AddPartyFromPermissionStatement(ref whereStatement, "o", "PartyFrom", com.Sconit.CodeMaster.OrderType.Production, false);
            //SecurityHelper.AddPartyToPermissionStatement(ref whereStatement, "o", "PartyTo", com.Sconit.CodeMaster.OrderType.Production);
            SecurityHelper.AddPartyFromAndPartyToPermissionStatement(ref whereStatement, "o", "Type", "o", "PartyFrom", "o", "PartyTo", com.Sconit.CodeMaster.OrderType.Production, false);

            HqlStatementHelper.AddLikeStatement("ReferenceOrderNo", searchModel.ReferenceOrderNo, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("ExternalOrderNo", searchModel.ExternalOrderNo, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("OrderNo", searchModel.OrderNo, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("PartyFrom", searchModel.PartyFrom, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("PartyTo", searchModel.PartyTo, "o", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("CreateUserName", searchModel.CreateUserName, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Status", searchModel.Status, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Priority", searchModel.Priority, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("PauseStatus", searchModel.IsPause, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("TraceCode", searchModel.TraceCode, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Type", searchModel.Type, "o", ref whereStatement, param);

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

        private SearchNativeSqlStatementModel PrepareSearchStatement1(GridCommand command, OrderMasterSearchModel searchModel, string whereStatement)
        {
            string selectStatement = "select o.*,s.Seq,s.SubSeq from ORD_OrderMstr_4 as o WITH(NOLOCK) left join ORD_OrderSeq as s WITH(NOLOCK) on o.OrderNo=s.OrderNo ";

            IList<object> param = new List<object>();
            HqlStatementHelper.AddLikeStatement("OrderNo", searchModel.OrderNo, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "o", ref whereStatement, param);

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

            //whereStatement += " order by s.Seq asc,s.SubSeq asc";

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by Seq asc,SubSeq asc";
            }
            SearchNativeSqlStatementModel searchStatementModel = new SearchNativeSqlStatementModel();
            searchStatementModel.SelectSql = selectStatement + whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        private IList<OrderDetail> TransformFlowDetailList2OrderDetailList(string flow)
        {
            IList<FlowDetail> flowDetailList = flowMgr.GetFlowDetailList(flow);
            IList<OrderDetail> orderDetailList = new List<OrderDetail>();
            foreach (FlowDetail flowDetail in flowDetailList)
            {
                OrderDetail orderDetail = new OrderDetail();
                Mapper.Map<FlowDetail, OrderDetail>(flowDetail, orderDetail);
                orderDetail.ItemDescription = base.genericMgr.FindById<Item>(flowDetail.Item).Description;
                orderDetailList.Add(orderDetail);
                //  SessionOrderDetailRepository.Insert(orderDetail);
            }
            return orderDetailList;
        }

        private void PrepareOrderBomDetail(OrderBomDetail orderBomDetail)
        {
            if (string.IsNullOrEmpty(orderBomDetail.Item))
            {
                throw new BusinessException("物料不能为空");
            }
            if (string.IsNullOrEmpty(orderBomDetail.Location))
            {
                throw new BusinessException("库位不能为空");
            }
            if (orderBomDetail.Operation == 0)
            {
                throw new BusinessException("工序不能为空");
            }
            if (orderBomDetail.BomUnitQty == 0)
            {
                throw new BusinessException("单位用量不能为空");
            }
            if (orderBomDetail.OrderedQty == 0)
            {
                throw new BusinessException("总用量不能为空");
            }
            Item item = base.genericMgr.FindById<Item>(orderBomDetail.Item);
            orderBomDetail.ItemDescription = item.Description;
            orderBomDetail.BaseUom = item.Uom;
            if (string.IsNullOrEmpty(orderBomDetail.Uom))
            {
                orderBomDetail.Uom = item.Uom;
            }
            if (string.IsNullOrEmpty(orderBomDetail.ReferenceItemCode))
            {
                orderBomDetail.ReferenceItemCode = item.ReferenceCode;
            }
            if (string.IsNullOrEmpty(orderBomDetail.OpReference))
            {
                orderBomDetail.OpReference = string.Empty;
            }

            if (orderBomDetail.BaseUom != orderBomDetail.Uom)
            {
                orderBomDetail.UnitQty = itemMgr.ConvertItemUomQty(orderBomDetail.Item, orderBomDetail.Uom, 1, orderBomDetail.BaseUom);
            }
            else
            {
                orderBomDetail.UnitQty = 1;
            }
        }

        private ProcedureSearchStatementModel PrepareSearchStatement_1(GridCommand command, OrderMasterSearchModel searchModel, string whereStatement, bool isReturn)
        {
            if (!string.IsNullOrWhiteSpace(searchModel.MultiStatus))
            {
                string statusSql = " and o.Status in( ";
                string[] statusArr = searchModel.MultiStatus.Split(',');
                for (int st = 0; st < statusArr.Length; st++)
                {
                    statusSql += "'" + statusArr[st] + "',";
                }
                statusSql = statusSql.Substring(0, statusSql.Length - 1) + ")";
                whereStatement += statusSql;
            }
            searchModel.ExternalOrderNo = searchModel.ExternalOrderNo != null ? searchModel.ExternalOrderNo.PadLeft(12, '0') : searchModel.ExternalOrderNo;
            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.OrderNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Flow, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter
            {
                Parameter = (int)com.Sconit.CodeMaster.OrderType.Production,
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

            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.OrderNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Flow, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter
            {
                Parameter = (int)com.Sconit.CodeMaster.OrderType.Production,
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

        private bool IsVanOrder(string orderNo)
        {
            var orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);
            var prodLineType = orderMaster.ProdLineType;
            return prodLineType == Sconit.CodeMaster.ProdLineType.Cab || prodLineType == Sconit.CodeMaster.ProdLineType.Chassis ||
                 prodLineType == Sconit.CodeMaster.ProdLineType.Assembly || prodLineType == Sconit.CodeMaster.ProdLineType.Special || prodLineType == Sconit.CodeMaster.ProdLineType.Check;
        }

        private string PrintHuList(IList<Hu> huList, string huTemplate)
        {
            foreach (var item in huList)
            {
                item.ManufacturePartyDescription = base.genericMgr.FindById<Party>(item.ManufactureParty).Name;
            }
            IList<PrintHu> printHuList = Mapper.Map<IList<Hu>, IList<PrintHu>>(huList);

            IList<object> data = new List<object>();
            data.Add(printHuList);
            data.Add(CurrentUser.FullName);
            return reportGen.WriteToFile(huTemplate, data);
        }

        private SearchStatementModel PrepareSearchReturnStatement(GridCommand command, ProductLineLocationDetailSearchModel searchModel)
        {
            IList<object> param = new List<object>();
            string whereStatement = "where exists (select 1 from OrderMaster  as o where o.OrderNo=p.OrderNo and o.Status in ("
                + (int)com.Sconit.CodeMaster.OrderStatus.Complete + "," + (int)com.Sconit.CodeMaster.OrderStatus.InProcess + "))";
            HqlStatementHelper.AddLikeStatement("OrderNo", searchModel.OrderNo, HqlStatementHelper.LikeMatchMode.Start, "p", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("LocationFrom", searchModel.LocationFrom, "p", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Operation", searchModel.Operation, "p", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "p", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("IsClose", false, "p", ref whereStatement, param);

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by p.CreateDate desc";
            }
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = "select count(*) from  ProductLineLocationDetail as p";
            searchStatementModel.SelectStatement = "select p from  ProductLineLocationDetail as p";
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        /// <summary>
        /// 工单投料
        /// </summary>
        /// <returns></returns>
        [SconitAuthorize(Permissions = "Url_Production_MaterialIn_FeedOrderMaster")]
        public ActionResult ImportProductionOrderDetail(IEnumerable<HttpPostedFileBase> attachments, string OrderNo)
        {
            try
            {
                if (string.IsNullOrEmpty(OrderNo))
                {
                    throw new BusinessException("订单号不能为空！");
                }

                foreach (var file in attachments)
                {
                    productionLineMgr.FeedRawMaterialFromXls(file.InputStream, OrderNo, false, DateTime.Now);
                    object obj = "投料明细投入成功";
                    return Json(new { status = obj }, "text/plain");
                }
            }
            catch (BusinessException ex)
            {
                Response.TrySkipIisCustomErrors = true;
                Response.StatusCode = 500;
                Response.Write(ex.GetMessages()[0].GetMessageString());
            }
            return Json(null);


        }
        /// <summary>
        /// 工单强制投料
        /// </summary>
        /// <param name="attachments"></param>
        /// <param name="OrderNo"></param>
        /// <returns></returns>
        [SconitAuthorize(Permissions = "Url_Production_MaterialIn_ForceFeedOrderMaster")]
        public ActionResult ImportForceProductionOrderDetail(IEnumerable<HttpPostedFileBase> attachments, string OrderNo)
        {
            try
            {
                if (string.IsNullOrEmpty(OrderNo))
                {
                    throw new BusinessException("订单号不能为空！");
                }

                foreach (var file in attachments)
                {
                    productionLineMgr.FeedRawMaterialFromXls(file.InputStream, OrderNo, true, DateTime.Now);
                    SaveSuccessMessage("投料明细投入成功");
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

        [SconitAuthorize(Permissions = "Url_Production_ImportCreateRequisition")]
        public ActionResult RequisitionIndex()
        {
            return View();
        }

        /// <summary>
        /// 试制备件批量拉料
        /// </summary>
        /// <param name="attachments"></param>
        /// <param name="windowTim"></param>
        /// <returns></returns>
        [SconitAuthorize(Permissions = "Url_Production_ImportCreateRequisition")]
        public ActionResult ImportCreateRequisition(IEnumerable<HttpPostedFileBase> attachments, DateTime? windowTim)
        {
            try
            {
                if (windowTim == null)
                {
                    throw new BusinessException("窗口时间不能为空！");
                }

                foreach (var file in attachments)
                {
                    orderMgr.ImportCreateRequisitionXls(file.InputStream, windowTim.Value);
                    SaveSuccessMessage("导入成功。");
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

        #region 整车生产单删除
        [SconitAuthorize(Permissions = "Url_Production_DeleteVanOrder")]
        public ActionResult DeleteVanOrderIndex()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_Production_DeleteVanOrder")]
        public JsonResult DeleteVanOrder(string vanCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(vanCode))
                {
                    throw new BusinessException("Van号不能为空。");
                }
                this.genericMgr.FindAllWithNativeSql<object[]>("exec USP_Busi_DeleteVanProdOrder ?,?,?",
                    new object[] { vanCode, CurrentUser.Id, CurrentUser.FullName },
                    new IType[] { NHibernateUtil.String, NHibernateUtil.Int32, NHibernateUtil.String });
                SaveSuccessMessage(string.Format("Van号{0}删除成功。",vanCode));
            }
            catch (BusinessException be)
            {
                SaveBusinessExceptionMessage(be);
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

        /// <summary>
        /// 批量导入 Van号删除
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        [SconitAuthorize(Permissions = "Url_Production_DeleteVanOrder")]
        public ActionResult ImportDeleteVanOrder(IEnumerable<HttpPostedFileBase> attachments)
        {
            try
            {
                foreach (var file in attachments)
                {
                    if (file.InputStream.Length == 0)
                    {
                        throw new BusinessException("Import.Stream.Empty");
                    }

                    HSSFWorkbook workbook = new HSSFWorkbook(file.InputStream);

                    ISheet sheet = workbook.GetSheetAt(0);
                    IEnumerator rows = sheet.GetRowEnumerator();

                    ImportHelper.JumpRows(rows, 10);
                    BusinessException businessException = new BusinessException();
                    #region 列定义
                    int colTraceCode = 1;//整车Van号
                    int rowCount = 10;
                    #endregion
                    HashSet<string> exactVanOrderNoList = new HashSet<string>();
                    while (rows.MoveNext())
                    {
                        rowCount++;
                        HSSFRow row = (HSSFRow)rows.Current;
                        if (!ImportHelper.CheckValidDataRow(row, 0, 2))
                        {
                            break;//边界
                        }

                        string traceCode = string.Empty;//生产单号

                        #region 整车Van号
                        traceCode = ImportHelper.GetCellStringValue(row.GetCell(colTraceCode));
                        if (string.IsNullOrWhiteSpace(traceCode))
                        {
                            businessException.AddMessage(string.Format("第{0}行:整车Van号不能为空。", rowCount));
                            continue;
                        }
                        else
                        {
                            //exactVanOrderNoList.Contains(traceCode);

                            if (!exactVanOrderNoList.Contains(traceCode))
                            {
                                try
                                {
                                    exactVanOrderNoList.Add(traceCode);
                                }
                                catch (Exception)
                                {
                                    businessException.AddMessage(string.Format("第{0}行:整车Van号{1}不存在。", rowCount, traceCode));
                                    continue;
                                }
                            }
                            else
                            {
                                throw new BusinessException(string.Format("第{0}行：整车Van号{1}出现重复行请检查数据的准确性", rowCount, traceCode));
                            }

                        }
                        #endregion
                    }
                    if (businessException.HasMessage)
                    {
                        throw businessException;
                    }
                    #region 调用删除存储过程
                    if (exactVanOrderNoList != null && exactVanOrderNoList.Count > 0)
                    {
                        foreach (var traceCode in exactVanOrderNoList)
                        {
                            try
                            {
                                this.genericMgr.FindAllWithNativeSql<object[]>("exec USP_Busi_DeleteVanProdOrder ?,?,?",
                                       new object[] { traceCode, CurrentUser.Id, CurrentUser.FullName },
                                       new IType[] { NHibernateUtil.String, NHibernateUtil.Int32, NHibernateUtil.String });
                                MessageHolder.AddInfoMessage("Van号{0}，删除成功。", traceCode);
                            }
                            catch (Exception ex)
                            {
                                if (ex.InnerException != null)
                                {
                                    if (ex.InnerException.InnerException != null)
                                    {
                                        businessException.AddMessage(ex.InnerException.InnerException.Message);
                                    }
                                    else
                                    {
                                        businessException.AddMessage(ex.InnerException.Message);
                                    }
                                }
                                else
                                {
                                    businessException.AddMessage(ex.Message);
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new BusinessException(string.Format("有效的数据行为0，可能是模板问题"));
                    }
                    #endregion

                    if (businessException.HasMessage)
                    {
                        throw businessException;
                    }
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

        #region  整车生产单导入
        [SconitAuthorize(Permissions = "Url_Production_GetCurrentVanOrder")]
        public ActionResult GetCurrentVanOrderIndex()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_Production_GetCurrentVanOrder")]
        public JsonResult GetCurrentVanOrder(string plant, string sapOrderNo, string prodLine)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(plant))
                {
                    throw new BusinessException("Van号不能为空。");
                }
                if (string.IsNullOrWhiteSpace(sapOrderNo))
                {
                    throw new BusinessException("Sap生产单号不能为空。");
                }
                if (string.IsNullOrWhiteSpace(prodLine))
                {
                    throw new BusinessException("生产线不能为空。");
                }
                orderMgr.GetCurrentVanOrder(plant, sapOrderNo, prodLine, CurrentUser.Code);
            }
            catch (BusinessException be)
            {
                SaveBusinessExceptionMessage(be);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex.Message);
            }
            return Json(null);
        }

       /// <summary>
        /// 批量导入整车生产单
       /// </summary>
       /// <param name="attachments"></param>
       /// <returns></returns>
        [SconitAuthorize(Permissions = "Url_Production_GetCurrentVanOrder")]
        public ActionResult ImportGetCurrentVanOrder(IEnumerable<HttpPostedFileBase> attachments)
        {
            try
            {
                if (attachments.First().InputStream.Length == 0)
                {
                    throw new BusinessException("Import.Stream.Empty");
                }

                HSSFWorkbook workbook = new HSSFWorkbook(attachments.First().InputStream);

                ISheet sheet = workbook.GetSheetAt(0);
                IEnumerator rows = sheet.GetRowEnumerator();

                ImportHelper.JumpRows(rows, 10);
                BusinessException businessException = new BusinessException();
                #region 列定义
                int colPlan = 1;//工厂
                int colSapOrderNo = 2;//Sap生产单
                int colProdLine = 3;//生产线
                int rowCount = 10;
                #endregion
                while (rows.MoveNext())
                {
                    rowCount++;
                    HSSFRow row = (HSSFRow)rows.Current;
                    if (!ImportHelper.CheckValidDataRow(row, 1, 4))
                    {
                        break;//边界
                    }

                    string plant = string.Empty;//工厂
                    string sapOrderNo = string.Empty;//Sap生产单
                    string prodlLine = string.Empty;//生产线

                    #region 整车Van号
                    plant = ImportHelper.GetCellStringValue(row.GetCell(colPlan));
                    if (string.IsNullOrWhiteSpace(plant))
                    {
                        businessException.AddMessage(string.Format("第{0}行:整车Van号不能为空。", rowCount));
                        continue;
                    }
                    #endregion

                    #region Sap生产单
                    sapOrderNo = ImportHelper.GetCellStringValue(row.GetCell(colSapOrderNo));
                    if (string.IsNullOrWhiteSpace(sapOrderNo))
                    {
                        businessException.AddMessage(string.Format("第{0}行:Sap生产单不能为空。", rowCount));
                        continue;
                    }
                    #endregion

                    #region 生产线
                    prodlLine = ImportHelper.GetCellStringValue(row.GetCell(colProdLine));
                    if (string.IsNullOrWhiteSpace(prodlLine))
                    {
                        businessException.AddMessage(string.Format("第{0}行:生产线不能为空。", rowCount));
                        continue;
                    }
                    #endregion

                    #region 调用导入方法
                    try
                    {
                        orderMgr.GetCurrentVanOrder(plant, sapOrderNo, prodlLine, CurrentUser.Code);
                    }
                    catch (BusinessException bx)
                    {
                        businessException.AddMessage(bx.GetMessages()[0].GetMessageString());
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null)
                        {
                            if (ex.InnerException.InnerException != null)
                            {
                                businessException.AddMessage(ex.InnerException.InnerException.Message);
                            }
                            else
                            {
                                businessException.AddMessage(ex.InnerException.Message);
                            }
                        }
                        else
                        {
                            businessException.AddMessage(ex.Message);
                        }
                    }
                    #endregion
                }

                if (businessException.HasMessage)
                {
                    throw businessException;
                }
                if (rowCount == 10)
                {

                }
            }
            catch (BusinessException bx)
            {
                SaveBusinessExceptionMessage(bx);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex.Message);
            }
            return Content(string.Empty);
        }
        #endregion
    }
}
