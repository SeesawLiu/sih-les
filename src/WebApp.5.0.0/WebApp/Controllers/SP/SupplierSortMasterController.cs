using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using com.Sconit.Web.Util;
using com.Sconit.Web.Models.SearchModels.ORD;
using com.Sconit.Web.Models;
using com.Sconit.Entity.ORD;
using com.Sconit.Utility;
using com.Sconit.Service;
using com.Sconit.Service.Impl;
using System.Text;
using com.Sconit.PrintModel.ORD;
using AutoMapper;
using com.Sconit.Utility.Report;
using com.Sconit.Entity.SYS;
using com.Sconit.Entity.Exception;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using com.Sconit.Entity.MD;
using com.Sconit.Entity;
using NHibernate.Criterion;
using NHibernate.Type;
using NHibernate;
using com.Sconit.Entity.SCM;
using com.Sconit.Entity.CUST;

namespace com.Sconit.Web.Controllers.SP
{
    public class SupplierSortMasterController : WebAppBaseController
    {
        //
        // GET: /SequenceOrder/

        private static string selectDetailCountStatement = "select count(*) from OrderDetail as d";

        private static string selectDetailStatement = "select d from OrderDetail as d";

        public IReportGen reportGen { get; set; }
        public IOrderMgr orderMgr { get; set; }




        #region View
        [SconitAuthorize(Permissions = "Url_Sort_List_Query")]
        public ActionResult Index()
        {
            return View();
        }
        [GridAction]
        [SconitAuthorize(Permissions = "Url_Sort_List_Query")]
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
        [SconitAuthorize(Permissions = "Url_Sort_List_Query")]
        public ActionResult _AjaxList(GridCommand command, OrderMasterSearchModel searchModel)
        {

            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<OrderMaster>()));
            }

            string whereStatement = "and o.OrderStrategy=" + (int)com.Sconit.CodeMaster.FlowStrategy.SEQ;
           
            if (!string.IsNullOrWhiteSpace(searchModel.SequenceGroup))
            {
                whereStatement += " and o.SeqGroup='" + searchModel.SequenceGroup + "'";
            }
            if (searchModel.IsListPrice)
            {
                whereStatement += string.Format(" and IsListPrice=0 and o.Status not in ({0},{1})", (int)com.Sconit.CodeMaster.OrderStatus.Cancel, (int)com.Sconit.CodeMaster.OrderStatus.Close);
            }
            if (searchModel.IsPrintOrder)
            {
                whereStatement += string.Format(" and IsPrintOrder=0 and o.Status not in ({0},{1})", (int)com.Sconit.CodeMaster.OrderStatus.Cancel, (int)com.Sconit.CodeMaster.OrderStatus.Close);
            }
            searchModel.SubType = (int)com.Sconit.CodeMaster.OrderSubType.Normal;
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchStatement(command, searchModel, whereStatement, false);
            GridModel<OrderMaster> gridModel = GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command);
            foreach (var ordermstr in gridModel.Data)
            {
                ordermstr.SeqOrderStrategyDescription = ordermstr.OrderTemplate == "KitOrder.xls" ? "Kit" : "排序";
            }
            return PartialView(gridModel);
        }

        [GridAction]
        public ActionResult _AjaxOrderDetail(string orderNo)
        {
           
            Entity.ACC.User user = SecurityContextHolder.Get();
            var orderMaster = this.genericMgr.FindById<OrderMaster>(orderNo);
            orderMaster.IsListPrice = true;
            this.genericMgr.Update(orderMaster);
            bool isChangeDetail = (orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.Submit || orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.InProcess) && CurrentUser.Permissions.Where(p => p.PermissionCode == "Url_SequenceOrder_Update").Count() > 0;
            //bool isChangeDetail = (orderMaster.Status == CodeMaster.OrderStatus.InProcess || orderMaster.Status == CodeMaster.OrderStatus.Submit) && user.Permissions.Where(p => p.PermissionCode == "Url_SequenceOrder_Update").Count() > 0;
            IList<OrderDetail> orderDetList = this.genericMgr.FindAll<OrderDetail>(" select d from OrderDetail as d where d.OrderNo=? order by Sequence asc ", orderNo);
            IList<OrderDetail> returnDetList = new List<OrderDetail>();
            string extOrderno = string.Empty;
            string extSeq = string.Empty;
            for (int i = 0; i < orderDetList.Count; i++)
            {
                var ordDet = orderDetList[i];
                if (i > 0)
                {
                    if (ordDet.ReserveNo == extOrderno && ordDet.ReserveLine == extSeq)
                    {
                        ordDet.ReserveNo = null;
                        ordDet.ReserveLine = null;
                        ordDet.StartDate = null;
                    }
                    else
                    {
                        extOrderno = ordDet.ReserveNo;
                        extSeq = ordDet.ReserveLine;
                    }
                }
                else
                {
                    extOrderno = ordDet.ReserveNo;
                    extSeq = ordDet.ReserveLine;
                }
                ordDet.StartDateFormat = ordDet.StartDate.HasValue ? ordDet.StartDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty;
                if (ordDet.OrderedQty == 0)
                {
                    if (ordDet.IsIncludeTax)
                    {
                        ordDet.StartDateFormat = "厂内消化(" + decimal.Truncate(ordDet.RequiredQty) + ")";
                        ordDet.ManufactureParty = string.Empty;
                    }
                    else if (!ordDet.IsIncludeTax && ordDet.IsProvisionalEstimate)
                    {
                        ordDet.StartDateFormat = "已配送(" + decimal.Truncate(ordDet.RequiredQty) + ")";
                    }
                    else
                    {
                        ordDet.Item = string.Empty;
                        ordDet.ItemDescription = string.Empty;
                        ordDet.ReferenceItemCode = string.Empty;
                        ordDet.ManufactureParty = string.Empty;
                        ordDet.Uom = string.Empty;
                        ordDet.BinTo = string.Empty;
                        ordDet.StartDateFormat = string.Empty;
                        ordDet.ManufactureParty = string.Empty;
                    }
                }
                ordDet.ItemCode = ordDet.Item;
                ordDet.IsChangeDetail = isChangeDetail;
                returnDetList.Add(ordDet);
            }
            return PartialView(new GridModel<OrderDetail>(returnDetList));
        }

        [SconitAuthorize(Permissions = "Url_Sort_List_Prev")]
        public ActionResult Preview()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_Sort_List_Prev")]
        public ActionResult _PreviewDetailList(GridCommand command, string flow, DateTime? cpTimeFrom, DateTime? cpTimeTo, string traceCode, string item)
        {
            ViewBag.PageSize = 50;
            return PartialView();
        }


        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxPreviewList(GridCommand command, string flow, DateTime? cpTimeFrom, DateTime? cpTimeTo, string traceCode, string item)
        {
            SqlParameter[] param = new SqlParameter[7];
            param[0] = new SqlParameter("@Flow", SqlDbType.VarChar, 50);
            param[1] = new SqlParameter("@TraceCode", SqlDbType.VarChar, 50);
            param[2] = new SqlParameter("@Item", SqlDbType.VarChar, 50);
            param[3] = new SqlParameter("@CPTimeFrom", SqlDbType.DateTime);
            param[4] = new SqlParameter("@CPTimeTo", SqlDbType.DateTime);
            param[5] = new SqlParameter("@PageSize", SqlDbType.Int);
            param[6] = new SqlParameter("@PageCount", SqlDbType.Int);

            param[0].Value = flow;
            param[1].Value = traceCode;
            param[2].Value = item;
            param[3].Value = cpTimeFrom;
            param[4].Value = cpTimeTo;
            param[5].Value = command.PageSize;
            param[6].Value = command.Page;

            DataSet ds = this.genericMgr.GetDatasetBySql("exec USP_LE_PreviewSequenceOrder @Flow,@TraceCode,@Item,@CPTimeFrom,@CPTimeTo,@PageSize,@PageCount", param);
            var gridMode = new GridModel<OrderDetail>();
            gridMode.Total = (int)ds.Tables[0].Rows[0].ItemArray[0];
            ViewBag.Total = gridMode.Total;
            //IList<object[]> dataList = this.genericMgr.FindAllWithNativeSql<object[]>("exec USP_LE_PreviewSequenceOrder ?,?,?,?,?,?,?",
            //    new object[] { flow, traceCode, item, cpTimeFrom, cpTimeTo, 20,1 });
            var getDetList = (from t in ds.Tables[1].AsEnumerable()
                              select new OrderDetail
                              {
                                  Sequence = t.Field<int>("Seq"),
                                  ReserveNo = t.Field<string>("ExtNo"),
                                  ReserveLine = t.Field<string>("ExtSeq"),
                                  Item = t.Field<string>("Item"),
                                  ItemDescription = t.Field<string>("ItemDesc"),
                                  ReferenceItemCode = t.Field<string>("RefItemCode"),
                                  OrderedQty = decimal.Truncate(t.Field<decimal>("OrderQty")),
                                  Uom = t.Field<string>("Uom"),
                                  ManufactureParty = t.Field<string>("ManufactureParty"),
                                  BinTo = t.Field<string>("OpRef"),
                                  StartDate = t.Field<DateTime?>("CPTime"),
                                  LocationFrom = t.Field<string>("LocFrom"),
                                  LocationTo = t.Field<string>("LocTo"),
                                  IsProvisionalEstimate = t.Field<Boolean>("IsCreateSeq"),//配送
                                  RequiredQty = decimal.Truncate(t.Field<Decimal>("ReqQty")),
                                  ExtraDemandSource = t.Field<string>("CreatedOrderNo"),//配单号
                                  IsIncludeTax = t.Field<Boolean>("IsItemConsume"),//厂内消化
                              }).ToList();
            var returnList = new List<OrderDetail>();
            string extOrderno = string.Empty;
            string extSeq = string.Empty;
            for (int i = 0; i < getDetList.Count; i++)
            {
                var ordDet = getDetList[i];
                if (i > 0)
                {
                    if (ordDet.ReserveNo == extOrderno && ordDet.ReserveLine == extSeq)
                    {
                        ordDet.ReserveNo = null;
                        ordDet.ReserveLine = null;
                        ordDet.StartDate = null;
                    }
                    else
                    {
                        extOrderno = ordDet.ReserveNo;
                        extSeq = ordDet.ReserveLine;
                    }
                }
                else
                {
                    extOrderno = ordDet.ReserveNo;
                    extSeq = ordDet.ReserveLine;
                }
                ordDet.StartDateFormat = ordDet.StartDate.HasValue ? ordDet.StartDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty;
                if (ordDet.OrderedQty == 0)
                {
                    if (ordDet.IsIncludeTax)
                    {
                        ordDet.StartDateFormat = "厂内消化(" + ordDet.RequiredQty + ")";
                        ordDet.ManufactureParty = string.Empty;
                    }
                    else if (!ordDet.IsIncludeTax && ordDet.IsProvisionalEstimate)
                    {
                        //已经生成过排序单的单号ExtraDmdSource
                        ordDet.StartDateFormat = "已配送(" + ordDet.RequiredQty + ")";
                        ordDet.ManufactureParty = string.IsNullOrWhiteSpace(ordDet.ExtraDemandSource) ? "" : "单号 " + ordDet.ExtraDemandSource;
                        //ordDet.ManufactureParty = "单号 " + ordDet.ExtraDemandSource;
                    }
                    else
                    {
                        ordDet.Item = string.Empty;
                        ordDet.ItemDescription = string.Empty;
                        ordDet.ReferenceItemCode = string.Empty;
                        ordDet.ManufactureParty = string.Empty;
                        ordDet.Uom = string.Empty;
                        ordDet.BinTo = string.Empty;
                        ordDet.StartDateFormat = string.Empty;
                        ordDet.ManufactureParty = string.Empty;
                    }
                }
                ordDet.ItemCode = ordDet.Item;
                returnList.Add(ordDet);
            }
            gridMode.Data = returnList;
            return PartialView(gridMode);
            //return PartialView(new GridModel<OrderDetail>(returnDetList));
        }


        #endregion

        #region 发货  Edit

        public ActionResult Edit(string orderNo)
        {
            OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);
            orderMaster.IsListPrice = true;
            this.genericMgr.Update(orderMaster);
            return View(orderMaster);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_SequenceMaster_SupplierShip")]
        public ActionResult _ShipOrderDetailList(string orderNo)
        {
            string hql = " select d from OrderDetail as d where d.OrderNo=? and d.ShippedQty<d.OrderedQty ";
            IList<OrderDetail> orderDetailList = base.genericMgr.FindAll<OrderDetail>(hql, orderNo);
            return PartialView(orderDetailList);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_SequenceMaster_SupplierShip")]
        public JsonResult ShipOrder(string idStr, string qtyStr)
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
                            input.ReceiveQty = Convert.ToDecimal(qtyArray[i]);
                            od.AddOrderDetailInput(input);
                            orderDetailList.Add(od);
                        }
                    }
                }
                if (orderDetailList.Count() == 0)
                {
                    throw new BusinessException("发货明细不能为空");
                }

                var recMaster = orderMgr.ReceiveOrder(orderDetailList, DateTime.Now);
                SaveSuccessMessage("发货成功，生成收货单号:" + recMaster.ReceiptNo);
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

        public ActionResult _OrderDetailList(string orderNo)
        {
            ViewBag.orderNo = orderNo;
            var orderMaster=this.genericMgr.FindById<OrderMaster>(orderNo);
            ViewBag.status = orderMaster.Status;
            ViewBag.IsAllowUpdate = (orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.Submit || orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.InProcess) && CurrentUser.Permissions.Where(p => p.PermissionCode == "Url_SequenceOrder_Update").Count() > 0;
            return PartialView();
        }

        [GridAction]
        public ActionResult _SelectBatchEditing(string orderNo)
        {
            string hql = " select d from OrderDetail as d where d.OrderNo=?";
            IList<OrderDetail> orderDetailList = base.genericMgr.FindAll<OrderDetail>(hql, orderNo);
            GridModel<OrderDetail> orderDets = new GridModel<OrderDetail>();
            orderDets.Total = orderDetailList.Count();
            orderDets.Data = orderDetailList;
            return PartialView(orderDets);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        public ActionResult _SaveBatchEditing(
            [Bind(Prefix = "updated")]IEnumerable<OrderDetail> updatedOrderDetails, string orderNo)
        {
            try
            {
                if (updatedOrderDetails != null && updatedOrderDetails.Count() > 0)
                {
                    orderMgr.BatchSeqOrderChange((IList<OrderDetail>)updatedOrderDetails, 2);
                }
                else
                {
                    throw new BusinessException("请选择要修改的明细。");
                }

                SaveSuccessMessage(Resources.ORD.OrderDetail.OrderDetail_Saved);
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }
            OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);
            return View("Edit", orderMaster);
        }


        /// <summary>
        /// 发货
        /// </summary>
        /// <param name="orderNo"></param>
        /// <returns></returns>
        [SconitAuthorize(Permissions = "Url_SequenceMaster_SupplierShip")]
        public ActionResult ShipOrderByOrderNo(string orderNo)
        {
            string hql = " select d from OrderDetail as d where d.OrderNo=? ";
            IList<OrderDetail> orderDetailList = base.genericMgr.FindAll<OrderDetail>(hql, orderNo);
            try
            {
                foreach (var det in orderDetailList)
                {
                    OrderDetailInput input = new OrderDetailInput();
                    input.ReceiveQty = det.OrderedQty;
                    det.AddOrderDetailInput(input);
                }
                orderMgr.ReceiveOrder(orderDetailList, System.DateTime.Now);
                SaveSuccessMessage("发货成功。");
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }
            OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);
            return View("Edit", orderMaster);
        }

        #endregion

        #region Kit单修改
        [GridAction]
        [SconitAuthorize(Permissions = "Url_SequenceOrder_Update")]
        public ActionResult _EditOrderDetailList(string id)
        {
            ViewBag.DetailId = id;
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_SequenceOrder_Update")]
        public ActionResult _SelectOrderDetail(string detId)
        {
            OrderDetail ordeDetail = this.genericMgr.FindById<OrderDetail>(Convert.ToInt32(detId));
            ViewBag.DetailId = detId;
            return PartialView(new GridModel<OrderDetail>(_GetOrderDetail(ordeDetail.OrderNo, ordeDetail.ReserveNo)));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_SequenceOrder_Update")]
        public ActionResult _InsertOrderDetail(string detId, OrderDetail addOrderDet)
        {
            ModelState.Remove("Item");
            OrderDetail ordeDetail = this.genericMgr.FindById<OrderDetail>(Convert.ToInt32(detId));
            try
            {
                ViewBag.DetailId = detId;
                addOrderDet.Item = addOrderDet.ItemCode;
                if (string.IsNullOrWhiteSpace(addOrderDet.Item))
                {
                    throw new BusinessException("物料编号不能为空。");
                }
                if (addOrderDet.OrderedQty < 0)
                {
                    throw new BusinessException("数量不能小于0。");
                }
                Item item = this.genericMgr.FindById<Item>(addOrderDet.Item);
                decimal qty = addOrderDet.OrderedQty;
                addOrderDet = Mapper.Map<OrderDetail, OrderDetail>(ordeDetail);
                addOrderDet.OrderNo = ordeDetail.OrderNo;
                addOrderDet.OrderedQty = qty;
                addOrderDet.Item = item.Code;
                addOrderDet.Uom = item.Uom;
                addOrderDet.BaseUom = item.Uom;
                addOrderDet.ItemDescription = item.Description;
                addOrderDet.ReferenceItemCode = item.ReferenceCode;
                addOrderDet.Flow = this.genericMgr.FindById<OrderMaster>(ordeDetail.OrderNo).Flow;
                orderMgr.AllSeqOrderChange(1, addOrderDet);
                SaveSuccessMessage("添加成功。");
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage("添加失败。" + ex.Message);
            }


            return PartialView(new GridModel<OrderDetail>(_GetOrderDetail(ordeDetail.OrderNo, ordeDetail.ReserveNo)));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_SequenceOrder_Update")]
        public ActionResult _UpdateOrderDetail(string detId, OrderDetail orderDet)
        {
            ModelState.Remove("Item");
            OrderDetail oldDet = this.genericMgr.FindById<OrderDetail>(orderDet.Id);
            try
            {
                oldDet.ICHARG = orderDet.ICHARG;
                orderDet.Item = orderDet.ItemCode;
                ViewBag.DetailId = detId;
                if (string.IsNullOrWhiteSpace(orderDet.Item))
                {
                    throw new BusinessException("物料编号不能为空。");
                }
                if (orderDet.OrderedQty < 0)
                {
                    throw new BusinessException("数量不能小于0。");
                }
                Item item = this.genericMgr.FindById<Item>(orderDet.Item);
                oldDet.Item = item.Code;
                oldDet.ItemDescription = item.Description;
                oldDet.ReferenceItemCode = item.ReferenceCode;
                oldDet.OrderedQty = orderDet.OrderedQty;
                oldDet.Flow = this.genericMgr.FindById<OrderMaster>(oldDet.OrderNo).Flow;
                orderMgr.AllSeqOrderChange(2, oldDet);
                SaveSuccessMessage("修改成功。");
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage("修改失败。" + ex.Message);
            }

            return PartialView(new GridModel<OrderDetail>(_GetOrderDetail(oldDet.OrderNo, oldDet.ReserveNo)));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_SequenceOrder_Update")]
        public ActionResult _DeleteOrderDetail(string detId, OrderDetail orderDet)
        {
            ModelState.Remove("Item");
            OrderDetail ordeDetail = this.genericMgr.FindById<OrderDetail>(orderDet.Id);
            try
            {
                ViewBag.DetailId = detId;
                ordeDetail.Flow = this.genericMgr.FindById<OrderMaster>(ordeDetail.OrderNo).Flow;
                orderMgr.AllSeqOrderChange(3, ordeDetail);
                SaveSuccessMessage("删除成功。");
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage("删除失败。" + ex.Message);
            }

            return PartialView(new GridModel<OrderDetail>(_GetOrderDetail(ordeDetail.OrderNo, ordeDetail.ReserveNo)));
        }

        private IList<OrderDetail> _GetOrderDetail(string orderNo, string reserveNo)
        {
            IList<OrderDetail> orderDetList = this.genericMgr.FindAll<OrderDetail>(" select d from OrderDetail as d where d.OrderNo=? and d.ReserveNo=? order by ReserveNo asc ", new object[] { orderNo, reserveNo });
            IList<OrderDetail> returnDetList = new List<OrderDetail>();
            string extOrderno = string.Empty;
            string extSeq = string.Empty;
            for (int i = 0; i < orderDetList.Count; i++)
            {
                var ordDet = orderDetList[i];
                if (i > 0)
                {
                    if (ordDet.ReserveNo == extOrderno && ordDet.ReserveLine == extSeq)
                    {
                        ordDet.ReserveNo = null;
                        ordDet.ReserveLine = null;
                        ordDet.StartDate = null;
                    }
                    else
                    {
                        extOrderno = ordDet.ReserveNo;
                        extSeq = ordDet.ReserveLine;
                    }
                }
                else
                {
                    extOrderno = ordDet.ReserveNo;
                    extSeq = ordDet.ReserveLine;
                }
                ordDet.ItemCode = ordDet.Item;
                returnDetList.Add(ordDet);
            }
            return returnDetList;
            //foreach (OrderDetail ordDet in orderDetList)
            //{
            //    foreach (var retDet in returnDetList)
            //    {
            //        if (ordDet.ExternalOrderNo == retDet.ExternalOrderNo && ordDet.ExternalSequence == retDet.ExternalSequence)
            //        {
            //            ordDet.ExternalOrderNo = null;
            //            ordDet.ExternalSequence = null;
            //            ordDet.StartDate = null;
            //        }
            //    }
            //    ordDet.ItemCode = ordDet.Item;
            //    returnDetList.Add(ordDet);
            //}
            //return returnDetList;
        }

        public ActionResult _WebOrderDetail(string itemCode)
        {
            if (!string.IsNullOrWhiteSpace(itemCode))
            {
                var itemEntity = this.genericMgr.FindById<Item>(itemCode);
                return this.Json(itemEntity);
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region 打印导出
        public void ExportMasterXLS(OrderMasterSearchModel searchModel)
        {
            string whereStatement = "and o.OrderStrategy=" + (int)com.Sconit.CodeMaster.FlowStrategy.SEQ;
            //if (!string.IsNullOrWhiteSpace(searchModel.Item))
            //{
            //    whereStatement += " and exists(select 1 from OrderDetail as d where d.OrderNo=o.OrderNo and d.Item ='" + searchModel.Item + "')";
            //}
            //if (!string.IsNullOrWhiteSpace(searchModel.Item) && !string.IsNullOrWhiteSpace(searchModel.TraceCode))
            //{
            //    whereStatement += " and exists(select 1 from OrderDetail as d where d.OrderNo=o.OrderNo and d.Item ='" + searchModel.Item + "' and d.ReserveNo='" + searchModel.TraceCode + "')";
            //}
            //if (!string.IsNullOrWhiteSpace(searchModel.Item) && string.IsNullOrWhiteSpace(searchModel.TraceCode))
            //{
            //    whereStatement += " and exists(select 1 from OrderDetail as d where d.OrderNo=o.OrderNo and d.Item ='" + searchModel.Item + "')";
            //}
            //if (string.IsNullOrWhiteSpace(searchModel.Item) && !string.IsNullOrWhiteSpace(searchModel.TraceCode))
            //{
            //    whereStatement += " and exists(select 1 from OrderDetail as d where d.OrderNo=o.OrderNo and d.ReserveNo ='" + searchModel.TraceCode + "')";
            //}
            if (!string.IsNullOrWhiteSpace(searchModel.SequenceGroup))
            {
                whereStatement += " and o.SeqGroup='" + searchModel.SequenceGroup + "'";
            }
            GridCommand command = new GridCommand();
            command.Page = 1;
            command.PageSize = 65500;
            searchModel.SubType = (int)com.Sconit.CodeMaster.OrderSubType.Normal;
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchStatement(command, searchModel, whereStatement, false);
            var data = GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command).Data;
            IList<OrderMaster> exportMasterList = data != null && data.Count() > 0 ? data.ToList() : null;
            ExportToXLS<OrderMaster>("ExportSeqOrder", "xls", exportMasterList);
        }

        public void ExportDetailXLS(string orderNos)
        {
            var exportDetails = this.genericMgr.FindAll<OrderDetail>(" select d from OrderDetail as d where d.OrderNo in (" + orderNos + ") order by OrderNo asc,ExternalSequence asc ");
            var masterList = this.genericMgr.FindAll<OrderMaster>(" select m from OrderMaster as m where m.OrderNo in(" + orderNos + ") ");

            IList<OrderDetail> returnDetList = new List<OrderDetail>();
            string extOrderno = string.Empty;
            string extSeq = string.Empty;
            for (int i = 0; i < exportDetails.Count; i++)
            {
                var ordDet = exportDetails[i];
                var master = masterList.FirstOrDefault(m => m.OrderNo == ordDet.OrderNo);
                ordDet.Flow = master.Flow;
                ordDet.FlowDescription = master.FlowDescription;
                ordDet.SequenceGroup = master.SequenceGroup;
                ordDet.TraceCode = master.TraceCode;
                if (i > 0)
                {
                    if (ordDet.ReserveNo == extOrderno && ordDet.ReserveLine == extSeq)
                    {
                        ordDet.ReserveNo = null;
                        ordDet.ReserveLine = null;
                        ordDet.StartDate = null;
                    }
                    else
                    {
                        extOrderno = ordDet.ReserveNo;
                        extSeq = ordDet.ReserveLine;
                    }
                }
                else
                {
                    extOrderno = ordDet.ReserveNo;
                    extSeq = ordDet.ReserveLine;
                }
                ordDet.StartDateFormat = ordDet.StartDate.HasValue ? ordDet.StartDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty;
                if (ordDet.OrderedQty == 0)
                {
                    if (ordDet.IsIncludeTax)
                    {
                        ordDet.StartDateFormat = "厂内消化(" + decimal.Truncate(ordDet.RequiredQty) + ")";
                        ordDet.ManufactureParty = string.Empty;
                    }
                    else if (!ordDet.IsIncludeTax && ordDet.IsProvisionalEstimate)
                    {
                        //已经生成过排序单的单号ExtraDmdSource
                        ordDet.StartDateFormat = "已配送(" + decimal.Truncate(ordDet.RequiredQty) + ")";
                        //ordDet.ManufactureParty = string.IsNullOrWhiteSpace(ordDet.ExtraDemandSource) ? "" : "单号 " + ordDet.ExtraDemandSource;
                        //ordDet.ManufactureParty = "单号 " + ordDet.ExtraDemandSource;
                    }
                    else
                    {
                        ordDet.Item = string.Empty;
                        ordDet.ItemDescription = string.Empty;
                        ordDet.ReferenceItemCode = string.Empty;
                        ordDet.ManufactureParty = string.Empty;
                        ordDet.Uom = string.Empty;
                        ordDet.BinTo = string.Empty;
                        ordDet.StartDateFormat = string.Empty;
                        ordDet.ManufactureParty = string.Empty;
                    }
                }
                ordDet.ItemCode = ordDet.Item;
                returnDetList.Add(ordDet);
            }
            ExportToXLS<OrderDetail>("ExportSeqDet", "xls", returnDetList);
        }


        public string PrintOrders(string orderNos)
        {
            string[] orderNoArray = orderNos.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string hqlMstr = "select om from OrderMaster om where OrderNo in(?";
            string hqlDet = "select  det.Seq,det.OrderNo,det.Item,det.ItemDesc,det.RefItemCode,det.ManufactureParty,det.Uom,det.BinTo,det.StartDate,det.ReqQty,det.ReserveNo,det.ReserveLine ,det.OrderQty,ce.ZENGINE from ORD_OrderDet_2 as det with(nolock) left join CUST_EngineTrace as ce with(nolock) on ce.TraceCode=det.ReserveNo where det.OrderNo in(?";
            List<object> paras = new List<object>();
            List<object> parasDet = new List<object>();
            paras.Add(orderNoArray[0]);
            parasDet.Add(orderNoArray[0]);
            for (int i = 1; i < orderNoArray.Length; i++)
            {
                hqlMstr = hqlMstr + ",?";
                hqlDet = hqlDet + ",?";
                paras.Add(orderNoArray[i]);
                parasDet.Add(orderNoArray[i]);
            }
            hqlDet = hqlDet + ")  ";
            hqlMstr = hqlMstr + ") ";
            parasDet.AddRange(paras);
            IList<OrderMaster> orderMasterList = this.genericMgr.FindAll<OrderMaster>(hqlMstr, paras.ToArray());
            IList<object[]> searResultList = this.genericMgr.FindAllWithNativeSql<object[]>(hqlDet + " union all " + hqlDet.Replace("ORD_OrderDet_2", "ORD_OrderDet_1"), parasDet.ToArray());
            var orderDetailList = (from tak in searResultList
                                   select new OrderDetail
                                   {
                                       Sequence = (int)tak[0],
                                       OrderNo = (string)tak[1],
                                       Item = (string)tak[2],
                                       ItemDescription = (string)tak[3],
                                       ReferenceItemCode = (string)tak[4],
                                       ManufactureParty = (string)tak[5],
                                       Uom = (string)tak[6],
                                       BinTo = (string)tak[7],
                                       StartDate = (DateTime?)tak[8],
                                       RequiredQty = (decimal)tak[9],
                                       ReserveNo = (string)tak[10],
                                       ReserveLine = (string)tak[11],
                                       OrderedQty = (decimal)tak[12],
                                       ZENGINE = (string)tak[13],
                                   }).ToList();


            StringBuilder printUrls = new StringBuilder();
            foreach (var orderMaster in orderMasterList)
            {
                var printDetails = orderDetailList.Where(o => o.OrderNo == orderMaster.OrderNo).ToList();
                string extNo = "";
                string extSeq = "";
                foreach (var ordDet in printDetails)
                {

                    if (ordDet.ReserveNo == extNo && ordDet.ReserveLine == extSeq)
                    {
                        ordDet.ReserveNo = null;
                        ordDet.ReserveLine = null;
                        ordDet.StartDate = null;
                    }
                    else
                    {
                        extNo = ordDet.ReserveNo;
                        extSeq = ordDet.ReserveLine;
                    }

                    ordDet.StartDateFormat = ordDet.StartDate.HasValue ? ordDet.StartDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty;
                    if (ordDet.OrderedQty == 0)
                    {

                        if (ordDet.IsIncludeTax)
                        {
                            ordDet.StartDateFormat = "厂内消化(" + decimal.Truncate(ordDet.RequiredQty) + ")";
                            ordDet.ManufactureParty = string.Empty;
                        }
                        else if (!ordDet.IsIncludeTax && ordDet.IsProvisionalEstimate)
                        {
                            //已经生成过排序单的单号ExtraDmdSource
                            ordDet.StartDateFormat = "已配送(" + decimal.Truncate(ordDet.RequiredQty) + ")";
                            //ordDet.ManufactureParty = string.IsNullOrWhiteSpace(ordDet.ExtraDemandSource) ? "" : "单号 " + ordDet.ExtraDemandSource;
                        }
                        else
                        {
                            ordDet.Item = string.Empty;
                            ordDet.ItemDescription = string.Empty;
                            ordDet.ReferenceItemCode = string.Empty;
                            ordDet.ManufactureParty = string.Empty;
                            ordDet.Uom = string.Empty;
                            ordDet.BinTo = string.Empty;
                            ordDet.StartDateFormat = string.Empty;
                            ordDet.ManufactureParty = string.Empty;
                        }
                    }

                }
                orderMaster.OrderDetails = printDetails;
                if (orderMaster.OrderDetails.Count > 0)
                {
                    PrintOrderMaster printOrderMstr = Mapper.Map<OrderMaster, PrintOrderMaster>(orderMaster);
                    IList<object> data = new List<object>();
                    data.Add(printOrderMstr);
                    data.Add(printOrderMstr.OrderDetails);
                    string reportFileUrl = string.Empty;
                    if (!string.IsNullOrWhiteSpace(orderMaster.OrderTemplate))
                    {
                        reportFileUrl = reportGen.WriteToFile(orderMaster.OrderTemplate, data);
                    }
                    else
                    {
                        reportFileUrl = reportGen.WriteToFile("SequenceOrder.xls", data);
                    }
                    printUrls.Append(reportFileUrl);
                    printUrls.Append("||");
                }
                //this.genericMgr.Update("update OrderMaster set IsPrintOrder=1 where OrderNo=?", orderMaster.OrderNo);
                orderMaster.IsPrintOrder = true;
                this.genericMgr.Update(orderMaster);
            }
            return printUrls.ToString();
        }

        //Van号合并打印
        public string composePrintOrders(string orderNos)
        {
            string[] orderNoArray = orderNos.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string hqlMstr = "select om from OrderMaster om where OrderNo in(?";
            string hqlDet = "select od from OrderDetail od where OrderNo in(?";
            List<object> paras = new List<object>();
            paras.Add(orderNoArray[0]);
            for (int i = 1; i < orderNoArray.Length; i++)
            {
                hqlMstr = hqlMstr + ",?";
                hqlDet = hqlDet + ",?";
                paras.Add(orderNoArray[i]);
            }
            hqlDet = hqlDet + ") and od.OrderedQty>0  order by Sequence asc";
            hqlMstr = hqlMstr + ") ";

            IList<OrderMaster> orderMasterList = this.genericMgr.FindAll<OrderMaster>(hqlMstr, paras.ToArray());
            IList<OrderDetail> orderDetailList = this.genericMgr.FindAll<OrderDetail>(hqlDet, paras.ToArray());

            StringBuilder printUrls = new StringBuilder();
            foreach (var orderMaster in orderMasterList)
            {
                var orderDetails = orderDetailList.Where(o => o.OrderNo == orderMaster.OrderNo).ToList();
                var printDetails = (from tak in orderDetails
                                    group tak by new
                                    {
                                        tak.ICHARG,
                                        tak.ManufactureParty,
                                        tak.Item
                                    } into result
                                    select new OrderDetail
                                    {
                                        ICHARG = result.Key.ICHARG,
                                        ManufactureParty = result.Key.ManufactureParty,
                                        Item = result.Key.Item,
                                        ReferenceItemCode = result.First().ReferenceItemCode,
                                        ItemDescription = result.First().ItemDescription,
                                        OrderedQty = result.Sum(r => r.OrderedQty),
                                    }
                        ).ToList();

                orderMaster.OrderDetails = printDetails;
                if (orderMaster.OrderDetails.Count > 0)
                {
                    PrintOrderMaster printOrderMstr = Mapper.Map<OrderMaster, PrintOrderMaster>(orderMaster);
                    printOrderMstr.VanNoCount = orderDetails.Select(o => o.ReserveNo).Count() + "";
                    IList<object> data = new List<object>();
                    data.Add(printOrderMstr);
                    data.Add(printOrderMstr.OrderDetails);
                    printUrls.Append(reportGen.WriteToFile("ComposeSequenceOrder.xls", data));
                    printUrls.Append("||");
                }
                //this.genericMgr.Update("update OrderMaster set IsPrintOrder=1 where OrderNo=?", orderMaster.OrderNo);
                orderMaster.IsPrintOrder = true;
                this.genericMgr.Update(orderMaster);
            }
            return printUrls.ToString();
        }


        #region 预览导出
        public void ExportPreviewXLS(string flow, DateTime? cpTimeFrom, DateTime? cpTimeTo, string traceCode, string item)
        {

            SqlParameter[] param = new SqlParameter[7];
            param[0] = new SqlParameter("@Flow", SqlDbType.VarChar, 50);
            param[1] = new SqlParameter("@TraceCode", SqlDbType.VarChar, 50);
            param[2] = new SqlParameter("@Item", SqlDbType.VarChar, 50);
            param[3] = new SqlParameter("@CPTimeFrom", SqlDbType.DateTime);
            param[4] = new SqlParameter("@CPTimeTo", SqlDbType.DateTime);
            param[5] = new SqlParameter("@PageSize", SqlDbType.Int);
            param[6] = new SqlParameter("@PageCount", SqlDbType.Int);

            param[0].Value = flow;
            param[1].Value = traceCode;
            param[2].Value = item;
            param[3].Value = cpTimeFrom;
            param[4].Value = cpTimeTo;
            param[5].Value = 65355;
            param[6].Value = 1;

            DataSet ds = this.genericMgr.GetDatasetBySql("exec USP_LE_PreviewSequenceOrder @Flow,@TraceCode,@Item,@CPTimeFrom,@CPTimeTo,@PageSize,@PageCount", param);
            var gridMode = new GridModel<OrderDetail>();
            gridMode.Total = (int)ds.Tables[0].Rows[0].ItemArray[0];
            ViewBag.Total = gridMode.Total;
            var returnDetList = (from t in ds.Tables[1].AsEnumerable()
                                 select new OrderDetail
                                 {
                                     Sequence = t.Field<int>("Seq"),
                                     ExternalOrderNo = t.Field<string>("ExtNo"),
                                     ExternalSequence = t.Field<string>("ExtSeq"),
                                     Item = t.Field<string>("Item"),
                                     ItemDescription = t.Field<string>("ItemDesc"),
                                     ReferenceItemCode = t.Field<string>("RefItemCode"),
                                     OrderedQty = decimal.Truncate(t.Field<decimal>("OrderQty")),
                                     Uom = t.Field<string>("Uom"),
                                     ManufactureParty = t.Field<string>("ManufactureParty"),
                                     OpRef = t.Field<string>("OpRef"),
                                     CPTime = t.Field<DateTime?>("CPTime"),
                                     LocationFrom = t.Field<string>("LocFrom"),
                                     LocationTo = t.Field<string>("LocTo"),
                                     IsCreateSeq = t.Field<Boolean>("IsCreateSeq")
                                 }).ToList();
            ExportToXLS<OrderDetail>("ExportPreviewXLS", "xls", returnDetList);
        }
        #endregion

        #endregion

        #region 明细菜单 明细报表
        [SconitAuthorize(Permissions = "Url_Sort_List_Detail_Query")]
        public ActionResult DetailIndex()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_Sort_List_Detail_Query")]
        public ActionResult DetailList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            return View();

        }


        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxOrderDetailList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchDetailStatement(command, searchModel);
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
                                    OrderStrategyDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.FlowStrategy, int.Parse((tak[24]).ToString())),
                                    MastWindowTime = (DateTime)tak[25],
                                    PickedQty = (decimal)tak[30],
                                    BinTo = (string)tak[31],
                                    ReserveLine = (string)tak[32],
                                    ReserveNo = (string)tak[29],
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

        public void ExportSupplierDetXLS(OrderMasterSearchModel searchModel)
        {
            ProcedureSearchStatementModel procedureSearchStatementModel = TempData["OrderMasterPrintSearchModel"] as ProcedureSearchStatementModel;
            TempData["OrderMasterPrintSearchModel"] = procedureSearchStatementModel;

            GridCommand command = new GridCommand();
            command.Page = 1;
            command.PageSize = (int)procedureSearchStatementModel.PageParameters[2].Parameter != 0 ? (int)procedureSearchStatementModel.PageParameters[2].Parameter : 60000;
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
                                    OrderStrategyDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.FlowStrategy, int.Parse((tak[24]).ToString())),
                                    MastWindowTime = (DateTime)tak[25],
                                    PickedQty = (decimal)tak[30],
                                    BinTo = (string)tak[31],
                                    ReserveLine = (string)tak[32],
                                    ReserveNo = (string)tak[29],
                                }).ToList();
                #endregion
            }
            ExportToXLS<OrderDetail>("ExportSeqDet", "xls", orderDetList);
        }


        private ProcedureSearchStatementModel PrepareSearchDetailStatement(GridCommand command, OrderMasterSearchModel searchModel)
        {
            //string whereStatement = string.Format(" and exists (select 1 from OrderMaster  as o where o.OrderStrategy <> 4 and o.SubType ={0} and o.OrderNo=d.OrderNo ) ",
            //    (int)com.Sconit.CodeMaster.OrderSubType.Normal);
            string whereStatement = " and exists( select 1 from OrderMaster as o where o.OrderNo=d.OrderNo and o.OrderStrategy = 4  and o.SubType=0) ";
            if (!string.IsNullOrWhiteSpace(searchModel.Flow))
            {
                whereStatement = "  and exists( select 1 from OrderMaster as o where o.OrderNo=d.OrderNo and o.OrderStrategy = 4  and o.SubType=0 and o.Flow='" + searchModel.Flow + "' ) ";
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

        //private SearchStatementModel PrepareDetailSearchStatement(GridCommand command, OrderMasterSearchModel searchModel)
        //{
        //    string whereStatement = " where exists( select 1 from OrderMaster as m where m.OrderNo=d.OrderNo and m.OrderStrategy = 4  and m.SubType=0) ";
        //    if (!string.IsNullOrWhiteSpace(searchModel.Flow))
        //    {
        //        whereStatement = "  where exists( select 1 from OrderMaster as m where m.OrderNo=d.OrderNo and m.OrderStrategy = 4  and m.SubType=0 and m.Flow='"+searchModel.Flow+"' ) ";
        //    }

        //    IList<object> param = new List<object>();

        //    HqlStatementHelper.AddEqStatement("OrderNo", searchModel.OrderNo, "d", ref whereStatement, param);
        //    HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "d", ref whereStatement, param);
        //    if (searchModel.DateFrom != null & searchModel.DateTo != null)
        //    {
        //        HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.DateFrom, searchModel.DateTo, "d", ref whereStatement, param);
        //    }
        //    else if (searchModel.DateFrom != null & searchModel.DateTo == null)
        //    {
        //        HqlStatementHelper.AddGeStatement("CreateDate", searchModel.DateFrom, "d", ref whereStatement, param);
        //    }
        //    else if (searchModel.DateFrom == null & searchModel.DateTo != null)
        //    {
        //        HqlStatementHelper.AddLeStatement("CreateDate", searchModel.DateTo, "d", ref whereStatement, param);
        //    }
        //    string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

        //    SearchStatementModel searchStatementModel = new SearchStatementModel();
        //    searchStatementModel.SelectCountStatement = selectDetailCountStatement;
        //    searchStatementModel.SelectStatement = selectDetailStatement;
        //    searchStatementModel.WhereStatement = whereStatement;
        //    searchStatementModel.SortingStatement = sortingStatement;
        //    searchStatementModel.Parameters = param.ToArray<object>();

        //    return searchStatementModel;
        //}

        #endregion

        #region receive

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_SequenceOrder_Receive")]
        public ActionResult ReceiveIndex(int? id)
        {
            int sequence = id.HasValue ? id.Value : 10;
            switch (sequence)
            {
                case 20:
                    ViewBag.Url = "Url_SequenceOrder_View_Distribution";
                    break;
                case 30:
                    ViewBag.Url = "Url_Supplier_SequenceOrder_View";
                    break;
                default:
                    ViewBag.Url = "Url_SequenceOrder_Receive";
                    break;
            }

            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_SequenceOrder_Receive")]
        public ActionResult Receive(GridCommand command, OrderMasterSearchModel searchModel)
        {
            this.ProcessSearchModel(command, searchModel);
            if (!CheckSearchModelIsNull(searchModel))
            {
                SaveWarningMessage("请选择查询条件");
            }

            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_SequenceOrder_Receive")]
        public ActionResult _AjaxReceiveOrderList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (string.IsNullOrWhiteSpace(searchModel.OrderNo) && string.IsNullOrWhiteSpace(searchModel.Flow) && (string.IsNullOrWhiteSpace(searchModel.PartyFrom) || string.IsNullOrWhiteSpace(searchModel.PartyTo)))
            {
                return PartialView(new GridModel(new List<OrderMaster>()));
            }
            else
            {
                string whereStatement = " and o.IsRecScanHu = 0 and o.Status in (" + (int)com.Sconit.CodeMaster.OrderStatus.InProcess + "," + (int)com.Sconit.CodeMaster.OrderStatus.Submit + ")"
                                   + " and o.SubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal
                                   + " and exists (select 1 from OrderDetail as d where d.RecQty < d.OrderQty and d.OrderNo = o.OrderNo) "
                                   + " and o.OrderStrategy=" + (int)com.Sconit.CodeMaster.FlowStrategy.SEQ + " ";
                ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchStatement(command, searchModel, whereStatement, false);
                return PartialView(GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command));

            }
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_SequenceOrder_Receive")]
        public ActionResult _ReceiveOrderDetailList(string checkedOrders)
        {
            string[] checkedOrderArray = checkedOrders.Split(',');
            checkedOrders = checkedOrders.Replace(",", "','");
            string hql = " select d from OrderDetail as d where d.OrderNo in ('" + checkedOrders + "') and d.ReceivedQty<d.OrderedQty";
            IList<OrderDetail> orderDetailList = base.genericMgr.FindAll<OrderDetail>(hql);
            return PartialView(orderDetailList);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_SequenceOrder_Receive")]
        public ActionResult ReceiveEdit(string checkedOrders)
        {
            ViewBag.CheckedOrders = checkedOrders;
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_SequenceOrder_Receive")]
        public JsonResult ReceiveOrder(string idStr, string qtyStr, string checkedOrders)
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
                SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_Received, checkedOrders);
                return Json(new { SuccessData = checkedOrders });

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


        #region Private
        private ProcedureSearchStatementModel PrepareSearchStatement(GridCommand command, OrderMasterSearchModel searchModel, string whereStatement, bool isReturn)
        {
            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.OrderNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Flow, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter
            {
                Parameter = (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.Procurement,
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
            paraList.Add(new ProcedureParameter { Parameter = true, Type = NHibernate.NHibernateUtil.Boolean });
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
            pageParaList.Add(new ProcedureParameter { Parameter = command.SortDescriptors.Count > 0 ? command.SortDescriptors[0].Member : "OrderNo", Type = NHibernate.NHibernateUtil.String });
            pageParaList.Add(new ProcedureParameter { Parameter = command.SortDescriptors.Count > 0 ? (command.SortDescriptors[0].SortDirection == ListSortDirection.Descending ? "desc" : "asc") : "desc", Type = NHibernate.NHibernateUtil.String });
            pageParaList.Add(new ProcedureParameter { Parameter = command.PageSize, Type = NHibernate.NHibernateUtil.Int32 });
            pageParaList.Add(new ProcedureParameter { Parameter = command.Page, Type = NHibernate.NHibernateUtil.Int32 });

            var procedureSearchStatementModel = new ProcedureSearchStatementModel();
            procedureSearchStatementModel.Parameters = paraList;
            procedureSearchStatementModel.PageParameters = pageParaList;
            procedureSearchStatementModel.CountProcedure = "USP_Search_ProcurementOrderCount";
            procedureSearchStatementModel.SelectProcedure = "USP_Search_ProcurementOrder";

            return procedureSearchStatementModel;
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

        #endregion



    }
}
