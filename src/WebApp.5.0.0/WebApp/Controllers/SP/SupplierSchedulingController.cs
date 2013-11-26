
namespace com.Sconit.Web.Controllers.SP
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;
    using System.Web.Services.Protocols;
    using com.Sconit.Entity;
    using com.Sconit.Entity.CUST;
    using com.Sconit.Entity.Exception;
    using com.Sconit.Entity.MD;
    using com.Sconit.Entity.ORD;
    using com.Sconit.Entity.SCM;
    using com.Sconit.Service;
    using com.Sconit.Utility.Report;
    using com.Sconit.Web.Models.ORD;
    using com.Sconit.Web.Models.SearchModels.ORD;
    using com.Sconit.Web.Util;
    using NHibernate;
    using NHibernate.Type;
    using Telerik.Web.Mvc;
    using Telerik.Web.Mvc.UI;
    using com.Sconit.Persistence;
    using System.Data.SqlClient;
    using System.Data;


    public class SupplierSchedulingController : WebAppBaseController
    {
        //public IIpMgr ipMgr { get; set; }
        public IFlowMgr flowMgr { get; set; }
        public IOrderMgr orderMgr { get; set; }
        public IReportGen reportGen { get; set; }
        public ISqlDao sqlDao { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public SupplierSchedulingController()
        {
        }

        [SconitAuthorize(Permissions = "Url_Scheduling_Agreement_Query")]
        public ActionResult Index()
        {
            return View();
        }


        [SconitAuthorize(Permissions = "Url_Scheduling_Agreement_Query")]
        public ActionResult _ScheduleLineList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            //TempData["OrderMasterSearchModel"] = searchModel;

            try
            {
                if (string.IsNullOrWhiteSpace(searchModel.Flow))
                {
                    ViewBag.IsShow = false;
                    SaveWarningMessage("请选择采购路线。");
                    return View();
                }
                ViewBag.IsShow = !this.genericMgr.FindById<FlowMaster>(searchModel.Flow).IsAutoCreate;
                #region 重新调用取最新的
                string item = searchModel.Item != string.Empty && searchModel.Item != null ? searchModel.Item : string.Empty;
                SAPService.SAPService sapService = new SAPService.SAPService();
                sapService.Url = ReplaceSIServiceUrl(sapService.Url);
                com.Sconit.Entity.ACC.User user = SecurityContextHolder.Get();
                IList<com.Sconit.Web.SAPService.OrderDetail> scheduleList = sapService.GetProcOrders(searchModel.Flow, null, item, "0084", user.Code);
                #endregion

                DateTime dateTimeNow = DateTime.Now;
                int listDays = searchModel.ListDays == null ? 21 : (searchModel.ListDays.Value > 0 ? searchModel.ListDays.Value : 0);
                ScheduleView scheduleView = PrepareScheduleView(searchModel.Flow, searchModel.Item, dateTimeNow, listDays, scheduleList, searchModel.NotIncludeZeroShipQty);

                #region  grid column
                var columns = new List<GridColumnSettings>();
                columns.Add(new GridColumnSettings
                    {
                        Member = scheduleView.ScheduleHead.OrderNoHead,
                        Title = Resources.ORD.OrderDetail.OrderDetail_ScheduleLineNo,
                        Sortable = false
                    });
                columns.Add(new GridColumnSettings
                {
                    Member = scheduleView.ScheduleHead.SequenceHead,
                    Title = Resources.ORD.OrderDetail.OrderDetail_ScheduleLineSeq,
                    Sortable = false
                });
                columns.Add(new GridColumnSettings
                {
                    Member = scheduleView.ScheduleHead.LesOrderNoHead,
                    Title = Resources.ORD.OrderDetail.OrderDetail_OrderNo,
                    Sortable = false
                });
                columns.Add(new GridColumnSettings
                {
                    Member = scheduleView.ScheduleHead.BillTerm,
                    Title = Resources.ORD.OrderDetail.OrderDetail_BillTerm,
                    Sortable = false
                });
                columns.Add(new GridColumnSettings
                {
                    Member = scheduleView.ScheduleHead.FlowHead,
                    Title = Resources.ORD.OrderDetail.OrderDetail_Flow,
                    Sortable = false
                });
                columns.Add(new GridColumnSettings
                {
                    Member = scheduleView.ScheduleHead.SupplierHead,
                    Title = Resources.ORD.OrderMaster.OrderMaster_PartyFrom,
                    Sortable = false
                });
                columns.Add(new GridColumnSettings
                {
                    Member = scheduleView.ScheduleHead.ItemHead,
                    Title = Resources.ORD.OrderDetail.OrderDetail_Item,
                    Sortable = false
                });
                columns.Add(new GridColumnSettings
                {
                    Member = scheduleView.ScheduleHead.ItemDescriptionHead,
                    Title = Resources.ORD.OrderDetail.OrderDetail_ItemDescription,
                    Sortable = false
                });
                columns.Add(new GridColumnSettings
                {
                    Member = scheduleView.ScheduleHead.ReferenceItemCodeHead,
                    Title = Resources.ORD.OrderDetail.OrderDetail_ReferenceItemCode,
                    Sortable = false
                });
                columns.Add(new GridColumnSettings
                {
                    Member = scheduleView.ScheduleHead.UomHead,
                    Title = Resources.ORD.OrderDetail.OrderDetail_Uom,
                    Sortable = false
                });
                columns.Add(new GridColumnSettings
                {
                    Member = scheduleView.ScheduleHead.UnitCountHead,
                    Title = Resources.ORD.OrderDetail.OrderDetail_UnitCount,
                    Sortable = false
                });
                columns.Add(new GridColumnSettings
                {
                    Member = scheduleView.ScheduleHead.LocationToHead,
                    Title = Resources.ORD.OrderDetail.OrderDetail_LocationTo,
                    Sortable = false
                });
                columns.Add(new GridColumnSettings
                {
                    Member = scheduleView.ScheduleHead.CurrentShipQtyHead,
                    Title = Resources.ORD.OrderDetail.OrderDetail_CurrentShipQty,
                    Sortable = false
                });

                //可发货数
                columns.Add(new GridColumnSettings
                {
                    Member = scheduleView.ScheduleHead.ShipQtyHead,
                    Title = Resources.ORD.OrderDetail.OrderDetail_ShipQty,
                    Sortable = false
                });
                //已发货数
                //columns.Add(new GridColumnSettings
                //{
                //    Member = scheduleView.ScheduleHead.ShippedQtyHead,
                //    Title = Resources.ORD.OrderDetail.OrderDetail_ShippedQty,
                //    Sortable = false
                //});
                //已收货数
                columns.Add(new GridColumnSettings
                {
                    Member = scheduleView.ScheduleHead.ReceivedQtyHead,
                    Title = Resources.ORD.OrderDetail.OrderDetail_ReceivedQty,
                    Sortable = false
                });
                //总计划数
                columns.Add(new GridColumnSettings
                {
                    Member = scheduleView.ScheduleHead.OrderQtyHead,
                    Title = Resources.ORD.OrderDetail.OrderDetail_TotalOrderQty,
                    Sortable = false
                });
                //历史计划数
                columns.Add(new GridColumnSettings
                {
                    Member = scheduleView.ScheduleHead.BackOrderQtyHead,
                    Title = Resources.ORD.OrderDetail.OrderDetail_BackOrderQty,
                    Sortable = false
                });
                //未来汇总
                columns.Add(new GridColumnSettings
                {
                    Member = scheduleView.ScheduleHead.ForecastQtyHead,
                    Title = Resources.ORD.OrderDetail.OrderDetail_ForecastQty,
                    Sortable = false
                });

                //冻结期计划数
                columns.Add(new GridColumnSettings
                {
                    Member = scheduleView.ScheduleHead.OrderQtyInFreeze,
                    Title = "冻结期计划数",
                    Sortable = false
                    // Visible = false
                });
                //已处理数
                columns.Add(new GridColumnSettings
                {
                    Member = scheduleView.ScheduleHead.HandledQty,
                    Title = "已处理数",
                    Sortable = false,
                    Visible = false
                });

                //类型
                columns.Add(new GridColumnSettings
                {
                    Member = "TaxDesc",
                    Title = "类型",
                    Sortable = false
                });
                #endregion

                #region
                if (scheduleView.ScheduleHead.ColumnCellList != null && scheduleView.ScheduleHead.ColumnCellList.Count > 0)
                {
                    for (int i = 0; i < listDays; i++)
                    {
                        columns.Add(new GridColumnSettings
                        {
                            Member = "RowCellList[" + i + "].DisplayQty",
                            MemberType = typeof(string),
                            Title = (scheduleView.ScheduleHead.ColumnCellList[i].EndDate.Value < dateTimeNow.Date) ? "欠交" : scheduleView.ScheduleHead.ColumnCellList[i].EndDate.Value.Date.ToShortDateString(),
                            Sortable = false
                        });

                    }
                }
                #endregion

                ViewData["columns"] = columns.ToArray();

                IList<ScheduleBody> scheduleBodyList = scheduleView.ScheduleBodyList != null && scheduleView.ScheduleBodyList.Count > 0 ? scheduleView.ScheduleBodyList.OrderBy(s => s.ReferenceItemCode).ThenBy(s => s.OrderNo).ToList() : new List<ScheduleBody>();
                TempData["scheduleBodyList"] = scheduleBodyList;
                return PartialView(scheduleBodyList);
            }
            catch (SoapException ex)
            {
                SaveErrorMessage(ex.Actor);
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

        [SconitAuthorize(Permissions = "Url_Scheduling_Agreement_Query")]
        public ActionResult _PreviewShipDetailWindow(string OrderNoStr, string SequenceStr, string CurrentShipQtyStr)
        {

            if (TempData["scheduleBodyList"] == null || OrderNoStr == string.Empty || SequenceStr == string.Empty)
            {
                return PartialView();
            }
            string[] orderNoArr = OrderNoStr.Split(',');
            string[] sequencesArr = SequenceStr.Split(',');
            string[] CurrentShipQtyArr = CurrentShipQtyStr.Split(',');
            IList<ScheduleBody> scheduleBodyList = TempData["scheduleBodyList"] as IList<ScheduleBody>;
            TempData["scheduleBodyList"] = scheduleBodyList;
            IList<ScheduleBody> returnScheduleBodyList = new List<ScheduleBody>();
            if (scheduleBodyList != null && scheduleBodyList.Count > 0)
            {
                foreach (ScheduleBody scheduleBody in scheduleBodyList)
                {
                    for (int i = 0; i < orderNoArr.Length; i++)
                    {
                        if (scheduleBody.OrderNo == orderNoArr[i] && scheduleBody.Sequence == sequencesArr[i])
                        {
                            scheduleBody.CurrentShipQty = CurrentShipQtyArr[i];
                            returnScheduleBodyList.Add(scheduleBody);
                        }
                    }
                }
            }
            return PartialView(returnScheduleBodyList);
        }

        [SconitAuthorize(Permissions = "Url_Scheduling_Agreement_Query")]
        public ActionResult Refresh(string flow)
        {
            try
            {
                if (string.IsNullOrEmpty(flow))
                {
                    ViewBag.IsShow = false;
                    throw new BusinessException("路线代码不能为空");
                }
                ViewBag.IsShow = !this.genericMgr.FindById<FlowMaster>(flow).IsAutoCreate;
                FlowMaster flowMaster = base.genericMgr.FindById<FlowMaster>(flow);
                Region region = base.genericMgr.FindById<Region>(flowMaster.PartyTo);
                SAPService.SAPService sapService = new SAPService.SAPService();
                sapService.Url = ReplaceSIServiceUrl(sapService.Url);
                com.Sconit.Entity.ACC.User user = SecurityContextHolder.Get();
                Supplier supplier = base.genericMgr.FindById<Supplier>(flowMaster.PartyFrom);
                DateTime lastModifyDate = supplier.LastRefreshDate == null ? DateTime.Now : supplier.LastRefreshDate.Value;
                sapService.GetProcOrders(flow, null, null, "0084", user.Code);
                SaveSuccessMessage("计划协议刷新成功。");
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }

            TempData["OrderMasterSearchModel"] = new OrderMasterSearchModel { Flow = flow };
            return View("Index");
        }

        #region old ship
        //[SconitAuthorize(Permissions = "Url_OrderMstr_ScheduleLine_Ship")]
        //public ActionResult ShipEdit(string flow, string dateStr)
        //{
        //    string[] dateArray = dateStr.Split(',');
        //    IList<object> param = new List<object>();
        //    IList<IType> types = new List<IType>();

        //    string hql = "from OrderMaster as o  where o.Type = ? and o.Flow = ?  and exists (select 1 from OrderDetail as d where d.OrderNo = o.OrderNo and d.ShippedQty < d.OrderedQty  and d.EndDate in (?";
        //    param.Add((int)com.Sconit.CodeMaster.OrderType.ScheduleLine);
        //    param.Add(flow);
        //    param.Add(DateTime.Parse(dateArray[0]));
        //    types.Add(NHibernateUtil.Int32);
        //    types.Add(NHibernateUtil.String);
        //    types.Add(NHibernateUtil.DateTime);

        //    for (int i = 1; i < dateArray.Count(); i++)
        //    {
        //        hql += ",?";
        //        param.Add(DateTime.Parse(dateArray[i]));
        //        types.Add(NHibernateUtil.DateTime);
        //    }

        //    hql += ")) ";
        //    IList<OrderMaster> orderMasterList = base.genericMgr.FindAll<OrderMaster>(hql, param.ToArray(), types.ToArray());
        //    IpMaster ipMaster = ipMgr.MergeOrderMaster2IpMaster(orderMasterList);
        //    ViewBag.Flow = flow;
        //    ViewBag.DateStr = dateStr;
        //    return View(ipMaster);

        //}

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_ScheduleLine_Ship")]
        public ActionResult _ShipOrderDetailList(string flow, string dateStr)
        {
            string[] dateArray = dateStr.Split(',');
            string hql = "select d from OrderDetail as d  where d.OrderType = ? and d.ScheduleType = ? and  d.ShippedQty < d.OrderedQty and exists (select 1 from OrderMaster as o where o.Flow = ? and d.OrderNo = o.OrderNo) and d.EndDate in (?";

            IList<object> param = new List<object>();
            IList<IType> types = new List<IType>();
            param.Add((int)com.Sconit.CodeMaster.OrderType.ScheduleLine);
            param.Add((int)com.Sconit.CodeMaster.ScheduleType.Firm);
            param.Add(flow);
            param.Add(DateTime.Parse(dateArray[0]));

            types.Add(NHibernateUtil.Int32);
            types.Add(NHibernateUtil.Int32);
            types.Add(NHibernateUtil.String);
            types.Add(NHibernateUtil.DateTime);

            for (int i = 1; i < dateArray.Count(); i++)
            {
                hql += ",?";
                param.Add(DateTime.Parse(dateArray[i]));
                types.Add(NHibernateUtil.DateTime);
            }

            hql += ")) ";

            IList<OrderDetail> orderDetailList = base.genericMgr.FindAll<OrderDetail>(hql, param.ToArray(), types.ToArray());
            return PartialView(orderDetailList);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_ScheduleLine_Ship")]
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

                IpMaster ipMaster = orderMgr.ShipOrder(orderDetailList);
                SaveSuccessMessage(Resources.ORD.OrderMaster.ScheduleLine_Shipped);
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
        #endregion

        #region 需求计划
        [SconitAuthorize(Permissions = "Url_Scheduling_DemandPlan")]
        public ActionResult DemandPlanIndex()
        {
            return View();
        }


        [GridAction]
        [SconitAuthorize(Permissions = "Url_Scheduling_DemandPlan")]
        public ActionResult DemandPlanList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            TempData["OrderMasterSearchModel"] = searchModel;
            //if (!string.IsNullOrWhiteSpace(searchModel.Item))
            //{
            //    TempData["_AjaxMessage"] = "";
            //}
            //else
            //{
            //    SaveWarningMessage(Resources.ErrorMessage.Errors_NoConditions);
            //}
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Scheduling_DemandPlan")]
        public ActionResult _AjaxDemandPlanList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            try
            {
                //if (string.IsNullOrWhiteSpace(searchModel.Item))
                //{
                //    return PartialView(new GridModel(new List<ScheduleBody>()));
                //}
                #region 重新调用取最新的
                string item = searchModel.Item != string.Empty && searchModel.Item != null ? searchModel.Item : string.Empty;
                SAPService.SAPService sapService = new SAPService.SAPService();
                sapService.Url = ReplaceSIServiceUrl(sapService.Url);
                com.Sconit.Entity.ACC.User user = SecurityContextHolder.Get();
                IList<com.Sconit.Web.SAPService.OrderDetail> scheduleList = sapService.GetProcOrders(null, user.Code.Trim(), item, "0084", user.Code);
                #endregion

                DateTime dateTimeNow = DateTime.Now;
                int listDays = searchModel.ListDays == null ? 21 : (searchModel.ListDays.Value > 0 ? searchModel.ListDays.Value : 0);
                ScheduleView scheduleView = PrepareScheduleViewBySupplier(searchModel.Flow, searchModel.Item, dateTimeNow, listDays, scheduleList, searchModel.NotIncludeZeroShipQty);

                var returnList = new List<ScheduleBody>();
                IList<ScheduleBody> scheduleBodyList = scheduleView.ScheduleBodyList != null && scheduleView.ScheduleBodyList.Count > 0 ? scheduleView.ScheduleBodyList.OrderBy(s => s.ReferenceItemCode).ThenBy(s => s.OrderNo).ToList() : new List<ScheduleBody>();
                foreach (var scheduleBody in scheduleBodyList)
                {
                    if (scheduleBody.RowCellList != null && scheduleBody.RowCellList.Count > 0)
                    {
                        //需求预测的已收数要以sap的为准
                        scheduleBody.ReceivedQty = scheduleBody.RowCellList.Sum(r => r.ReceivedQty).ToString("0.###");
                        scheduleBody.DemandDate = scheduleBody.RowCellList.Min(r => r.EndDate).Value.ToShortDateString() + "-" + scheduleBody.RowCellList.Max(r => r.EndDate).Value.ToShortDateString();
                        scheduleBody.DemandQty = Convert.ToDecimal(scheduleBody.ForecastQty) + Convert.ToDecimal(scheduleBody.BackOrderQty);
                    }
                    else
                    {
                        //需求预测的已收数要以sap的为准
                        scheduleBody.ReceivedQty = scheduleBody.RowCellList.Sum(r => r.ReceivedQty).ToString("0.###");
                        scheduleBody.DemandDate = DateTime.Now.ToString() + "-" + DateTime.Now.ToString();
                        scheduleBody.DemandQty = Convert.ToDecimal(scheduleBody.BackOrderQty);
                    }

                }
                TempData["DemandPlanList"] = scheduleBodyList;
                scheduleBodyList = scheduleBodyList.Where(r => r.DemandQty.ToString("0.###") != r.ReceivedQty).ToList();
                GridModel<ScheduleBody> gridmodel = new GridModel<ScheduleBody>();
                gridmodel.Total = scheduleBodyList.Count;
                gridmodel.Data = scheduleBodyList.Skip((command.Page - 1) * command.PageSize).Take(command.PageSize);
                return PartialView(gridmodel);
            }
            catch (SoapException ex)
            {
                SaveErrorMessage(ex.Actor);
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }
            return PartialView(new GridModel(new List<ScheduleBody>()));
        }


        [GridAction]
        public ActionResult _AjaxDemandPlanDetail(string extNo, string extSeq, string supplier, string item)
        {
            DateTime dateTimeNow = System.DateTime.Now;
            IList<ScheduleBody> scheduleBodyList = TempData["DemandPlanList"] != null ? TempData["DemandPlanList"] as IList<ScheduleBody> : new List<ScheduleBody>();
            TempData["DemandPlanList"] = scheduleBodyList;
            var findBody = scheduleBodyList.FirstOrDefault(f => f.OrderNo == extNo && f.Sequence == extSeq && f.Supplier == supplier && f.Item == item);
            var returnList = new List<ScheduleBody>();
            if (findBody != null)
            {

                if (findBody.RowCellList != null && findBody.RowCellList.Count > 0)
                {
                    var historySheculine = findBody.RowCellList.Where(r => r.EndDate < dateTimeNow.Date);
                    if (historySheculine != null && historySheculine.Count() > 0)
                    {
                        var historyReceivedQty = historySheculine.Sum(r => r.ReceivedQty);
                        var historyDemandQty = historySheculine.Sum(r => r.OrderQty);
                        var historyDemandDate = historySheculine.Min(r => r.EndDate).Value.ToShortDateString() + "-" + DateTime.Now.ToShortDateString();
                        returnList.Add(new ScheduleBody
                            {
                                ReceivedQty = historyReceivedQty.ToString("0.###"),
                                DemandDate = historyDemandDate,
                                DemandQty = historyDemandQty,
                            });
                    }
                    foreach (var roeCell in findBody.RowCellList.Where(r => r.EndDate >= dateTimeNow.Date))
                    {
                        returnList.Add(new ScheduleBody
                        {
                            ReceivedQty = roeCell.ReceivedQty.ToString("0.###"),
                            DemandDate = roeCell.EndDate.Value.ToShortDateString(),
                            DemandQty = Convert.ToDecimal(roeCell.OrderQty),
                        });
                    }
                }
            //scheduleView.ScheduleBodyList = scheduleView.ScheduleBodyList.Where(s => s.DemandQty != s.ReceivedQty);
            }
            return PartialView(new GridModel(returnList));
        }

        public void ExporDemandPlanListXls()
        {
            DateTime dateTimeNow = System.DateTime.Now;
            IList<ScheduleBody> scheduleBodyList = TempData["DemandPlanList"] != null ? TempData["DemandPlanList"] as IList<ScheduleBody> : new List<ScheduleBody>();
            if (scheduleBodyList != null && scheduleBodyList.Count > 0)
            {
                TempData["DemandPlanList"] = scheduleBodyList;
            }
            else
            {
                scheduleBodyList = new List<ScheduleBody>();
            }
            var returnList = new List<ScheduleBody>();
            foreach (var scheduleBody in scheduleBodyList)
            {
                if (scheduleBody.RowCellList != null && scheduleBody.RowCellList.Count > 0)
                {
                    var historySheculine = scheduleBody.RowCellList.Where(r => r.EndDate < dateTimeNow.Date);
                    if (historySheculine != null && historySheculine.Count() > 0)
                    {
                        var historyReceivedQty = historySheculine.Sum(r => r.ReceivedQty);
                        var historyDemandQty = historySheculine.Sum(r => r.OrderQty);
                        var historyDemandDate = historySheculine.Min(r => r.EndDate).Value.ToShortDateString() + "-" + DateTime.Now.ToShortDateString();
                        returnList.Add(new ScheduleBody
                        {
                            OrderNo = scheduleBody.OrderNo,
                            Sequence = scheduleBody.Sequence,
                            Item = scheduleBody.Item,
                            ItemDescription = scheduleBody.ItemDescription,
                            ReferenceItemCode = scheduleBody.ReferenceItemCode,
                            Uom = scheduleBody.Uom,
                            ReceivedQty = historyReceivedQty.ToString("0.###"),
                            DemandDate = historyDemandDate,
                            DemandQty = historyDemandQty,
                        });
                    }
                   
                    foreach (var roeCell in scheduleBody.RowCellList.Where(r => r.EndDate >= dateTimeNow.Date))
                    {
                        returnList.Add(new ScheduleBody
                        {
                            OrderNo = scheduleBody.OrderNo,
                            Sequence = scheduleBody.Sequence,
                            Item = scheduleBody.Item,
                            ItemDescription = scheduleBody.ItemDescription,
                            ReferenceItemCode = scheduleBody.ReferenceItemCode,
                            Uom = scheduleBody.Uom,
                            ReceivedQty = roeCell.ReceivedQty.ToString("0.###"),
                            DemandDate = roeCell.EndDate.Value.ToShortDateString(),
                            DemandQty = Convert.ToDecimal(roeCell.OrderQty),
                        });
                    }
                }
            }

            ExportToXLS("DemandPlanListXls", "XLS", returnList);
        }
        #endregion


        public ActionResult ScheduleLineItemIndex()
        {
            TempData["scheduleLineItemList"] = null;
            return View();
        }

        public ActionResult ScheduleLineItem(GridCommand command, string Item, string Supplier)
        {

            ViewBag.PageSize = base.ProcessPageSize(20);
            ViewBag.Item = Item;
            ViewBag.Supplier = Supplier;
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxScheduleLineItem(GridCommand command, string Item, string Supplier)
        {
            try
            {
                if (command.Page > 1 && TempData["scheduleLineItemList"] != null)
                {
                    IList<ScheduleLineItem> scheduleLineItemListReturn = TempData["scheduleLineItemList"] != null ? TempData["scheduleLineItemList"] as IList<ScheduleLineItem> : null;
                    TempData["scheduleLineItemList"] = scheduleLineItemListReturn;
                    GridModel<ScheduleLineItem> returnGrid = new GridModel<ScheduleLineItem>();
                    returnGrid.Total = scheduleLineItemListReturn.Count;
                    returnGrid.Data = scheduleLineItemListReturn.Skip((command.Page - 1) * command.PageSize).Take(command.PageSize);
                    return PartialView(returnGrid);

                }
                var user = SecurityContextHolder.Get();
                SAPService.SAPService sapService = new SAPService.SAPService();
                sapService.Url = ReplaceSIServiceUrl(sapService.Url);
                sapService.Timeout = int.Parse(this.systemMgr.GetEntityPreferenceValue(com.Sconit.Entity.SYS.EntityPreference.CodeEnum.SAPServiceTimeOut));
                IList<object[]> objList = sapService.GetScheduleLineItem(user.Code, Item, Supplier, "0084");

                IList<ScheduleLineItem> scheduleLineItemList = new List<ScheduleLineItem>();
                // IList<Item> itemList = new List<Item>();
                string[] deleteSupplierArr = new string[] { "9000000013", "1000002749", "1000002523", "1000002192", "1000002191", "1000002190", "1000002184", "1000001780", "1000000619", "1000000482", "1000000630", "1000000759", "1000001680", "1000000787" };
                StringBuilder hql = new StringBuilder();
                IList<object> parm = new List<object>();
                List<Item> itemList = new List<Item>();
                StringBuilder supplierHql = new StringBuilder();
                IList<object> supplierParm = new List<object>();
                List<Supplier> supplierList = new List<Supplier>();

                int j = 0;
                if (objList != null && objList.Count > 0)
                {
                    #region
                    int i = 0;
                    while (i < objList.Count)
                    {
                        for (int ii = 0; ii < 2000; ii++)
                        {
                            if (i == objList.Count) { break; }
                            ScheduleLineItem scheduLineItem = new ScheduleLineItem();

                            scheduLineItem.Supplier = (string)objList[i][0];
                            scheduLineItem.Item = (string)objList[i][1];
                            scheduLineItem.EBELN = (string)objList[i][2];
                            scheduLineItem.EBELP = (string)objList[i][3];
                            if (string.IsNullOrEmpty(scheduLineItem.Supplier) || scheduLineItem.Supplier == null || string.IsNullOrEmpty(scheduLineItem.Item) || scheduLineItem.Item == null)
                            {
                                j++;
                                i++;
                                continue;
                            }
                            bool deleteSup = false;
                            //数组中的供应商过滤掉
                            deleteSup = deleteSupplierArr.Where(d => d == scheduLineItem.Supplier).Count() > 0;
                            if (deleteSup)
                            {
                                j++;
                                i++;
                                continue;
                            }

                            if (hql.Length == 0)
                            {
                                hql.Append("from Item where Code in (?");
                                supplierHql.Append(" from Supplier as s where s.Code in(?");
                            }
                            else
                            {
                                hql.Append(", ?");
                                supplierHql.Append(", ?");
                            }
                            parm.Add(scheduLineItem.Item);
                            supplierParm.Add(scheduLineItem.Supplier);
                            scheduleLineItemList.Add(scheduLineItem);
                            i++;
                        }
                        if (hql != null && hql.ToString() != string.Empty)
                        {
                            IList<Item> CurrentItemList = base.genericMgr.FindAll<Item>(hql.ToString() + ")", parm.ToArray());
                            if (CurrentItemList != null && CurrentItemList.Count > 0)
                            {
                                itemList.AddRange(CurrentItemList);
                            }
                            IList<Supplier> CurrentSupplierList = base.genericMgr.FindAll<Supplier>(supplierHql.ToString() + ")", supplierParm.ToArray());
                            {
                                supplierList.AddRange(CurrentSupplierList);
                            }
                        }
                        hql = new StringBuilder();
                        parm = new List<object>();
                        supplierHql = new StringBuilder();
                        supplierParm = new List<object>();

                        // itemList.Add( base.genericMgr.FindAll<Item>(hql + ")", parm.ToArray()));

                    }


                    if (itemList != null && itemList.Count > 0)
                    {
                        foreach (ScheduleLineItem scheduleLineItem in scheduleLineItemList)
                        {
                            //var item = itemList.FirstOrDefault(t => t.Code == scheduleLineItem.Item);

                            var items = itemList.Where(t => t.Code == scheduleLineItem.Item);
                            if (items != null && items.Count() > 0)
                            {
                                var item = items.First();
                                scheduleLineItem.Description = item.Description;
                                scheduleLineItem.Uom = item.Uom;
                                scheduleLineItem.Container = item.Container;
                                scheduleLineItem.ReferenceCode = item.ReferenceCode;
                                scheduleLineItem.UnitCount = item.UnitCount;
                            }
                            //scheduleLineItem.ContainerDesc = item.ContainerDesc;
                            //var supplier = supplierList.FirstOrDefault(s => s.Code == scheduleLineItem.Supplier);
                            var suppliers = supplierList.Where(s => s.Code == scheduleLineItem.Supplier);
                            if (suppliers != null && suppliers.Count() > 0)
                            {
                                var supplier = suppliers.First();
                                scheduleLineItem.Name = supplier.Name;
                                scheduleLineItem.ShortCode = supplier.ShortCode;
                            }
                        }
                    }
                    #endregion
                }

                GridModel<ScheduleLineItem> grid = new GridModel<ScheduleLineItem>();
                grid.Total = objList.Count - j;
                TempData["scheduleLineItemList"] = scheduleLineItemList;
                grid.Data = scheduleLineItemList.Skip((command.Page - 1) * command.PageSize).Take(command.PageSize);
                return PartialView(grid);
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }
            return PartialView(new GridModel<ScheduleLineItem>() { Data = new List<ScheduleLineItem>(), Total = 0 });
            #region
            //com.Sconit.Entity.ACC.User user = SecurityContextHolder.Get();
            //string whereStatement = "where exists (" +
            // " select 1 from UserPermissionView as p where p.UserId =" + user.Id + " and p.PermissionCode=s.Supplier)";
            //IList<object> param = new List<object>();
            //HqlStatementHelper.AddLikeStatement("Item", Item, HqlStatementHelper.LikeMatchMode.Start, "s", ref whereStatement, param);
            //HqlStatementHelper.AddEqStatement("Supplier", Supplier, "s", ref whereStatement, param);

            //string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            //SearchStatementModel searchStatementModel = new SearchStatementModel();
            //searchStatementModel.SelectCountStatement = "select count(*) from ScheduleLineItem as s ";
            //searchStatementModel.SelectStatement = "select s from ScheduleLineItem as s";
            //searchStatementModel.WhereStatement = whereStatement;
            //searchStatementModel.SortingStatement = sortingStatement;
            //searchStatementModel.Parameters = param.ToArray<object>();
            //GridModel<ScheduleLineItem> grid = GetAjaxPageData<ScheduleLineItem>(searchStatementModel, command);
            //if (grid.Data != null && grid.Data.Count() > 0)
            //{
            //    var items = grid.Data.Select(o => o.Item).ToArray();
            //    IList<Item> itemList = new List<Item>();

            //    string hql = string.Empty;
            //    IList<object> parm = new List<object>();

            //    foreach (string itemCode in items.Distinct())
            //    {
            //        if (hql == string.Empty)
            //        {
            //            hql = "from Item where Code in (?";
            //        }
            //        else
            //        {
            //            hql += ", ?";
            //        }
            //        parm.Add(itemCode);
            //    }
            //    itemList = base.genericMgr.FindAll<Item>(hql + ")", parm.ToArray());
            //    if (itemList != null && itemList.Count > 0)
            //    {
            //        foreach (ScheduleLineItem scheduleLineItem in grid.Data)
            //        {
            //            foreach (var item in itemList)
            //            {
            //                if (scheduleLineItem.Item == item.Code)
            //                {
            //                    scheduleLineItem.Description = item.Description;
            //                    scheduleLineItem.Uom = item.Uom;
            //                    scheduleLineItem.ReferenceCode = item.ReferenceCode;
            //                    scheduleLineItem.UnitCount = item.UnitCount;

            //                }
            //            }
            //        }
            //    }
            //}
            //return PartialView(grid);
            #endregion
        }

        #region 导出
        public void SaveToClient()
        {
            IList<ScheduleLineItem> scheduleLineItemList = TempData["scheduleLineItemList"] != null ? TempData["scheduleLineItemList"] as IList<ScheduleLineItem> : null;
            if (scheduleLineItemList != null && scheduleLineItemList.Count > 0)
            {
                TempData["scheduleLineItemList"] = scheduleLineItemList;
            }
            else
            {
                scheduleLineItemList = this.genericMgr.FindAll<ScheduleLineItem>();
            }
            //IList<object> data = new List<object>();
            //data.Add(scheduleLineItemList);
            //reportGen.WriteToClient("ScheduleLineItem.xls", data, "ScheduleLineItem.xls");
            ExportToXLS("ExportXls", "XLS", scheduleLineItemList);
        }
        #endregion

        //发货
        [SconitAuthorize(Permissions = "Url_OrderMstr_ScheduleLine_Ship")]
        public JsonResult ShipOrderByQty(string flow, string OrderNoStr, string SequenceStr, string CurrentShipQtyStr, string SureShipQtyStr)
        {
            try
            {
                if (!string.IsNullOrEmpty(OrderNoStr))
                {
                    string[] orderNoArray = OrderNoStr.Split(',');
                    string[] sequenceArray = SequenceStr.Split(',');
                    string[] currentShipQtyArray = CurrentShipQtyStr.Split(',');
                    string[] SureshipQtyArray = SureShipQtyStr.Split(',');
                    IList<ScheduleLineInput> scheduleLineInputList = new List<ScheduleLineInput>();
                    int i = 0;
                    foreach (string orderNo in orderNoArray)
                    {

                        ScheduleLineInput scheduleLineInput = new ScheduleLineInput();
                        scheduleLineInput.EBELN = orderNoArray[i];
                        scheduleLineInput.EBELP = sequenceArray[i];
                        scheduleLineInput.ShipQty = Convert.ToDecimal((currentShipQtyArray[i]));
                        scheduleLineInput.SureShipQty = Convert.ToDecimal((SureshipQtyArray[i]));
                        scheduleLineInputList.Add(scheduleLineInput);
                        i++;
                    }
                    IpMaster ipMaster = this.orderMgr.ShipScheduleLine(flow, scheduleLineInputList);
                    SaveSuccessMessage(Resources.ORD.OrderMaster.ScheduleLine_Shipped);
                    return Json(new { IpNo = ipMaster.IpNo });
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
            return Json(null);
        }

        private RowCell GetRowCell(ScheduleBody scheduleBody, ColumnCell columnCell, IList<RowCell> rowCellList, DateTime dateTimeNow)
        {
            RowCell rowCell = new RowCell();
            var q = rowCellList.Where(r => r.OrderNo == scheduleBody.OrderNo && r.Sequence == scheduleBody.Sequence && r.Tax == scheduleBody.Tax
                                       && (columnCell.EndDate < dateTimeNow ? r.EndDate < dateTimeNow : r.EndDate == columnCell.EndDate));

            if (q != null && q.Count() > 0)
            {
                rowCell = q.First();
                rowCell.OrderQty = q.Sum(oq => oq.OrderQty);
                rowCell.ShippedQty = q.Sum(oq => oq.ShippedQty);
                if (rowCell.EndDate < dateTimeNow && rowCell.OrderQty == rowCell.ShippedQty)
                {
                    rowCell.OrderQty = 0;
                    rowCell.ShippedQty = 0;
                }
            }
            else
            {
                rowCell.OrderNo = scheduleBody.OrderNo;
                rowCell.Sequence = scheduleBody.Sequence;
                rowCell.LesOrderNo = scheduleBody.LesOrderNo;
                rowCell.Flow = scheduleBody.Flow;
                rowCell.Tax = scheduleBody.Tax;
                rowCell.ScheduleType = com.Sconit.CodeMaster.ScheduleType.Forecast;
                rowCell.EndDate = columnCell.EndDate;
                rowCell.OrderQty = 0;
                rowCell.ShippedQty = 0;
            }
            return rowCell;
        }

        private ScheduleView PrepareScheduleView(string flow, string item, DateTime dateTimeNow, int intervalDays, IList<com.Sconit.Web.SAPService.OrderDetail> scheduleList, bool notIncludeZeroShipQty)
        {
            ScheduleView scheduleView = new ScheduleView();
            ScheduleHead scheduleHead = new ScheduleHead();

            if (!string.IsNullOrWhiteSpace(flow))
            {

                #region webservice取到拼好的orderdet
                var orderDetailList = from sl in scheduleList
                                      select new
                                      {
                                          EndDate = sl.EndDate,
                                          FreezeDays = sl.FreezeDays,
                                          OrderNo = sl.OrderNo,
                                          ExternalOrderNo = sl.ExternalOrderNo,
                                          ExternalSequence = sl.ExternalSequence,
                                          BillTerm = sl.BillTerm == com.Sconit.Web.SAPService.OrderBillTerm.OnlineBilling ? "寄售" : "非寄售",
                                          Item = sl.Item,
                                          ItemDesc = sl.ItemDescription,
                                          RefItemCode = sl.ReferenceItemCode,
                                          Uom = sl.Uom,
                                          UnitCount = sl.UnitCount,
                                          OrderedQty = sl.OrderedQty,
                                          ShipQty = sl.ShippedQty,
                                          ReceiveQty = sl.ReceivedQty,//Sap已收数
                                          LocationTo = sl.LocationTo,
                                          Flow = sl.Flow,
                                          PartyFrom = sl.ManufactureParty,
                                          Tax = sl.Tax
                                      };
                #endregion

                #region head
                IList<ColumnCell> columnCellList = new List<ColumnCell>();

                for (int i = 0; i < intervalDays; i++)
                {
                    ColumnCell c = new ColumnCell();
                    c.EndDate = dateTimeNow.Date.AddDays(i);
                    columnCellList.Add(c);
                }
                scheduleHead.ColumnCellList = columnCellList;
                #endregion

                #region body
                var s = from p in orderDetailList
                        group p by new
                        {
                            OrderNo = p.OrderNo,
                            ExternalOrderNo = p.ExternalOrderNo,
                            ScheduleLineSeq = p.ExternalSequence,
                            BillTerm = p.BillTerm,
                            Item = p.Item,
                            ItemDesc = p.ItemDesc,
                            RefItemCode = p.RefItemCode,
                            Uom = p.Uom,
                            UnitCount = p.UnitCount,
                            FreezeDays = p.FreezeDays,
                            LocationTo = p.LocationTo,
                            Flow = p.Flow,
                            PartyFrom = p.PartyFrom,
                            Tax = p.Tax
                        } into g
                        //where g.Count(det => det.OrderedQty != det.ShipQty) > 0
                        select new ScheduleBody
                        {
                            OrderNo = g.Key.ExternalOrderNo,
                            Sequence = g.Key.ScheduleLineSeq,
                            LesOrderNo = g.Key.OrderNo,
                            BillTerm = g.Key.BillTerm,
                            Flow = g.Key.Flow,
                            Item = g.Key.Item,
                            ItemDescription = g.Key.ItemDesc,
                            ReferenceItemCode = g.Key.RefItemCode,
                            Uom = g.Key.Uom,
                            UnitCount = g.Key.UnitCount,
                            LocationTo = g.Key.LocationTo,
                            FreezeDays = g.Key.FreezeDays,
                            Supplier = g.Key.PartyFrom,
                            Tax = g.Key.Tax
                        };


                var r = from p in orderDetailList
                        group p by new
                        {
                            OrderNo = p.ExternalOrderNo,
                            ScheduleLineSeq = p.ExternalSequence,
                            LesOrderNo = p.OrderNo,
                            Flow = p.Flow,
                            PartyFrom = p.PartyFrom,
                            EndDate = p.EndDate,
                            Tax = p.Tax,
                            ScheduleType = dateTimeNow.AddDays(Convert.ToDouble(p.FreezeDays)) >= p.EndDate ? com.Sconit.CodeMaster.ScheduleType.Firm : com.Sconit.CodeMaster.ScheduleType.Forecast,
                        } into g
                        select new RowCell
                        {
                            OrderNo = g.Key.OrderNo,
                            Sequence = g.Key.ScheduleLineSeq,
                            LesOrderNo = g.Key.LesOrderNo,
                            Flow = g.Key.Flow,
                            Supplier = g.Key.PartyFrom,
                            EndDate = g.Key.EndDate,
                            Tax = g.Key.Tax,
                            ScheduleType = (com.Sconit.CodeMaster.ScheduleType)g.Key.ScheduleType,
                            OrderQty = g.Sum(p => p.OrderedQty),
                            ShippedQty = g.Sum(p => p.ShipQty),
                            ReceivedQty = g.Sum(p => p.ReceiveQty)//sap已收数
                        };

                IList<ScheduleBody> scheduleBodyList = s.ToList();
                IList<RowCell> allRowCellList = r.ToList();

                if (scheduleBodyList != null && scheduleBodyList.Count > 0)
                {
                    //查找IpDetailList
                    StringBuilder selectIpDetailStatement = new StringBuilder();
                    SqlParameter[] parm = new SqlParameter[scheduleBodyList.Count + 1];
                    foreach (string ebeln_ebelp in scheduleBodyList.Select(b => b.OrderNo + "&" + b.Sequence).Distinct())
                    {
                        if (selectIpDetailStatement.Length == 0)
                        {
                            selectIpDetailStatement.Append(@"select ipd.extno,ipd.extseq,ipd.qty,ipd.recqty,ipd.isclose 
                                                        from ord_ipdet_8 ipd with(NOLOCK) inner join ord_ipmstr_8 ipm with(NOLOCK) on ipd.ipno = ipm.ipno 
                                                        where ipd.EBELN_EBELP in ('" + ebeln_ebelp + "'");
                        }
                        else
                        {
                            selectIpDetailStatement.Append(",'" + ebeln_ebelp + "'");
                        }
                    }
                    selectIpDetailStatement.Append(") and ipm.status <> 3 and ipd.type <> 1");
                    DataSet dataSet = sqlDao.GetDatasetBySql(selectIpDetailStatement.ToString(), null);
                    EnumerableRowCollection<DataRow> ipDetailList = dataSet.Tables[0].AsEnumerable();

                    foreach (ScheduleBody scheduleBody in scheduleBodyList)
                    {
                        //可发货数=在冻结期内的计划数-已处理数
                        //已处理数的计算逻辑如下：
                        //获取该计划协议号+行号相关的所有送货单明细（不包含差异类型和已冲销的送货单）
                        //明细未关闭时则取发货数
                        //明细关闭时全部取收货数

                        //在冻结期内的计划数
                        decimal orderQtyInFreeze = allRowCellList.Where(q => q.OrderNo == scheduleBody.OrderNo && q.Sequence == scheduleBody.Sequence && q.Tax == scheduleBody.Tax
                                                                 && q.EndDate <= dateTimeNow.AddDays(Convert.ToDouble(scheduleBody.FreezeDays))).Sum(q => q.OrderQty);
                        scheduleBody.OrderQtyInFreeze = orderQtyInFreeze.ToString("0.########");
                        //获取与该计划协议号+行号相关的所有送货单明细
                        //IList<IpDetail> ipDetail = base.genericMgr.FindAll<IpDetail>("select i from IpDetail as i where ExternalOrderNo = '" + scheduleBody.OrderNo + "' and ExternalSequence = '" + scheduleBody.Sequence + "' and Type = " + (int)CodeMaster.IpDetailType.Normal + " and exists (select 1 from IpMaster as im where im.IpNo = i.IpNo and im.Status != " + (int)CodeMaster.IpStatus.Cancel + ")");
                        List<DataRow> ipDetail = (from ipd in ipDetailList
                                                  where (string)ipd[0] == scheduleBody.OrderNo && (string)ipd[1] == scheduleBody.Sequence
                                                  select ipd).ToList();
                        //汇总已处理数
                        decimal handledQty = ipDetail.Select(o => (bool)o[4] ? (decimal)o[3] : ((decimal)o[3] == 0 ? (decimal)o[2] : (decimal)o[3])).Sum();
                        scheduleBody.HandledQty = handledQty.ToString("0.########");
                        //scheduleBody.ShipQty = allRowCellList.Where(q => q.OrderNo == scheduleBody.OrderNo && q.Sequence == scheduleBody.Sequence
                        //                                     && q.OrderQty > q.ShippedQty && q.EndDate <= dateTimeNow.AddDays(Convert.ToDouble(scheduleBody.FreezeDays))).Sum(q => (q.OrderQty - q.ShippedQty)).ToString("0.########");
                        scheduleBody.ShipQty = (orderQtyInFreeze - handledQty).ToString("0.########");

                        //已发货数
                        //scheduleBody.ShippedQty = allRowCellList.Where(q => q.OrderNo == scheduleBody.OrderNo && q.Sequence == scheduleBody.Sequence
                        //                                   && q.ShippedQty > 0).Sum(q => q.ShippedQty).ToString("0.########");
                        decimal shippedQty = ipDetail.Select(o => (bool)o[4] && (decimal)o[3] == 0 ? 0 : (decimal)o[2]).Sum();
                        scheduleBody.ShippedQty = shippedQty.ToString("0.########");

                        //已收货数
                        decimal receivedQty = ipDetail.Select(o => (decimal)o[3]).Sum();
                        scheduleBody.ReceivedQty = receivedQty.ToString("0.########");

                        //总计划数
                        scheduleBody.OrderQty = allRowCellList.Where(q => q.OrderNo == scheduleBody.OrderNo && q.Sequence == scheduleBody.Sequence && q.Tax == scheduleBody.Tax
                                                           && q.OrderQty > 0).Sum(q => q.OrderQty).ToString("0.########");

                        //未来汇总
                        scheduleBody.ForecastQty = allRowCellList.Where(q => q.OrderNo == scheduleBody.OrderNo && q.Sequence == scheduleBody.Sequence && q.Tax == scheduleBody.Tax
                                       && q.EndDate >= dateTimeNow.Date.AddDays(Convert.ToDouble(intervalDays)) && q.OrderQty > 0).Sum(q => q.OrderQty).ToString("0.########");

                        //历史总计划数
                        scheduleBody.BackOrderQty = allRowCellList.Where(q => q.OrderNo == scheduleBody.OrderNo && q.Sequence == scheduleBody.Sequence && q.Tax == scheduleBody.Tax
                                       && q.EndDate < dateTimeNow.Date && q.OrderQty > 0).Sum(q => q.OrderQty).ToString("0.########");

                        if (scheduleHead.ColumnCellList != null && scheduleHead.ColumnCellList.Count > 0)
                        {
                            List<RowCell> rowCellList = new List<RowCell>();
                            foreach (ColumnCell columnCell in scheduleHead.ColumnCellList)
                            {
                                RowCell rowCell = GetRowCell(scheduleBody, columnCell, allRowCellList, dateTimeNow);
                                rowCellList.Add(rowCell);
                            }
                            scheduleBody.RowCellList = rowCellList;
                        }
                    }
                }
                //过滤可发货数为0的行
                if (notIncludeZeroShipQty)
                    scheduleView.ScheduleBodyList = (from sl in scheduleBodyList
                                                     where Convert.ToDecimal(sl.ShipQty) > 0
                                                     select sl).ToList();
                else
                    scheduleView.ScheduleBodyList = scheduleBodyList;
                #endregion
            }

            scheduleView.ScheduleHead = scheduleHead;
            return scheduleView;
        }


        private ScheduleView PrepareScheduleViewBySupplier(string PartyFrom, string item, DateTime dateTimeNow, int intervalDays, IList<com.Sconit.Web.SAPService.OrderDetail> scheduleList, bool notIncludeZeroShipQty)
        {
            ScheduleView scheduleView = new ScheduleView();
            ScheduleHead scheduleHead = new ScheduleHead();
            //if (!string.IsNullOrWhiteSpace(PartyFrom) || !string.IsNullOrWhiteSpace(item))
            //{

            #region webservice取到拼好的orderdet
            var orderDetailList = from sl in scheduleList
                                  select new
                                  {
                                      EndDate = sl.EndDate,
                                      FreezeDays = sl.FreezeDays,
                                      OrderNo = sl.OrderNo,
                                      ExternalOrderNo = sl.ExternalOrderNo,
                                      ExternalSequence = sl.ExternalSequence,
                                      Item = sl.Item,
                                      ItemDesc = sl.ItemDescription,
                                      RefItemCode = sl.ReferenceItemCode,
                                      Uom = sl.Uom,
                                      UnitCount = sl.UnitCount,
                                      OrderedQty = sl.OrderedQty,
                                      ShipQty = sl.ShippedQty,
                                      ReceiveQty = sl.ReceivedQty,
                                      LocationTo = sl.LocationTo,
                                      Flow = sl.Flow,
                                      PartyFrom = sl.ManufactureParty
                                  };
            #endregion

            #region body
            IList<ScheduleBody> scheduleBodyList = (from p in orderDetailList
                                                    group p by new
                                                    {
                                                        OrderNo = p.OrderNo,
                                                        //Flow = p.Flow,
                                                        ExternalOrderNo = p.ExternalOrderNo,
                                                        ScheduleLineSeq = p.ExternalSequence,
                                                        Item = p.Item,
                                                        ItemDesc = p.ItemDesc,
                                                        RefItemCode = p.RefItemCode,
                                                        Uom = p.Uom,
                                                        //UnitCount = p.UnitCount,
                                                        FreezeDays = p.FreezeDays,
                                                        //LocationTo = p.LocationTo,
                                                        PartyFrom = p.PartyFrom
                                                    } into g
                                                    //where g.Count(det => det.OrderedQty != det.ShipQty) > 0
                                                    select new ScheduleBody
                                                    {
                                                        OrderNo = g.Key.ExternalOrderNo,
                                                        Sequence = g.Key.ScheduleLineSeq,
                                                        LesOrderNo = g.Key.OrderNo,
                                                        Flow = string.Join(",", g.Select(s => s.Flow)),
                                                        Item = g.Key.Item,
                                                        ItemDescription = g.Key.ItemDesc,
                                                        ReferenceItemCode = g.Key.RefItemCode,
                                                        Uom = g.Key.Uom,
                                                        //UnitCount = g.Key.UnitCount,
                                                        LocationTo = string.Join(",", g.Select(s => s.LocationTo)),
                                                        FreezeDays = g.Key.FreezeDays,
                                                        Supplier = g.Key.PartyFrom
                                                    }).ToList();


            IList<RowCell> allRowCellList = (from p in orderDetailList
                                             group p by new
                                                  {
                                                      OrderNo = p.ExternalOrderNo,
                                                      ScheduleLineSeq = p.ExternalSequence,
                                                      LesOrderNo = p.OrderNo,
                                                      PartyFrom = p.PartyFrom,
                                                      EndDate = p.EndDate,
                                                      ScheduleType = dateTimeNow.AddDays(Convert.ToDouble(p.FreezeDays)) >= p.EndDate ? com.Sconit.CodeMaster.ScheduleType.Firm : com.Sconit.CodeMaster.ScheduleType.Forecast,
                                                      OrderQty = p.OrderedQty,
                                                      ShippedQty = p.ShipQty,
                                                      ReceivedQty = p.ReceiveQty,//sap已收数
                                                      Item = p.Item
                                                  }
                                                 into g
                                                 //group p by new
                                                 //{
                                                 //    OrderNo = p.OrderNo,
                                                 //    ScheduleLineSeq = p.ScheduleLineSeq,
                                                 //    LesOrderNo = p.OrderNo,
                                                 //    //Flow = p.Flow,
                                                 //    PartyFrom = p.PartyFrom,
                                                 //    EndDate = p.EndDate,
                                                 //    ScheduleType = p.ScheduleType,
                                                 //    Item = p.Item
                                                 //} into g
                                                 select new RowCell
                                                 {
                                                     OrderNo = g.Key.OrderNo,
                                                     Sequence = g.Key.ScheduleLineSeq,
                                                     LesOrderNo = g.Key.LesOrderNo,
                                                     //Flow = g.Key.Flow,
                                                     Supplier = g.Key.PartyFrom,
                                                     EndDate = g.Key.EndDate,
                                                     ScheduleType = (com.Sconit.CodeMaster.ScheduleType)g.Key.ScheduleType,
                                                     OrderQty = g.Key.OrderQty,
                                                     ShippedQty = g.Key.ShippedQty,
                                                     ReceivedQty = g.Key.ReceivedQty,//sap已收数
                                                     Item = g.Key.Item
                                                 }).ToList();

            //查找IpDetailList
            if (scheduleBodyList != null && scheduleBodyList.Count > 0)
            {
                StringBuilder selectIpDetailStatement = new StringBuilder();
                IList<object> selectIpDetailParms = new List<object>();
                foreach (string ebeln_ebelp in scheduleBodyList.Select(b => b.OrderNo + "&" + b.Sequence).Distinct())
                {
                    if (selectIpDetailStatement.Length == 0)
                    {
                        selectIpDetailStatement.Append(@"select ipd.extno,ipd.extseq,ipd.qty,ipd.recqty,ipd.isclose 
                                                        from ord_ipdet_8 ipd with(NOLOCK) inner join ord_ipmstr_8 ipm with(NOLOCK) on ipd.ipno = ipm.ipno 
                                                    where ipd.EBELN_EBELP in (?");

                        selectIpDetailParms.Add(ebeln_ebelp);
                    }
                    else
                    {
                        selectIpDetailStatement.Append(",?");
                        selectIpDetailParms.Add(ebeln_ebelp);
                    }
                }
                selectIpDetailStatement.Append(") and ipm.status <> ? and ipd.type <> 1");
                selectIpDetailParms.Add(CodeMaster.IpStatus.Cancel);
                IList<object[]> ipDetailList = base.genericMgr.FindAllWithNativeSql<object[]>(selectIpDetailStatement.ToString(), selectIpDetailParms.ToArray());

                foreach (ScheduleBody scheduleBody in scheduleBodyList)
                {
                    //可发货数=在冻结期内的计划数-已处理数
                    //已处理数的计算逻辑如下：
                    //获取该计划协议号+行号相关的所有送货单明细（不包含差异类型和已冲销的送货单）
                    //明细未关闭时则取发货数
                    //明细关闭时全部取收货数

                    //在冻结期内的计划数
                    decimal orderQtyInFreeze = allRowCellList.Where(q => q.OrderNo == scheduleBody.OrderNo && q.Sequence == scheduleBody.Sequence //&& q.Flow == scheduleBody.Flow
                                                             && q.EndDate <= dateTimeNow.AddDays(Convert.ToDouble(scheduleBody.FreezeDays))).Sum(q => q.OrderQty);
                    scheduleBody.OrderQtyInFreeze = orderQtyInFreeze.ToString("0.########");
                    //获取与该计划协议号+行号相关的所有送货单明细
                    //IList<IpDetail> ipDetail = base.genericMgr.FindAll<IpDetail>("select i from IpDetail as i where ExternalOrderNo = '" + scheduleBody.OrderNo + "' and ExternalSequence = '" + scheduleBody.Sequence + "' and Type = " + (int)CodeMaster.IpDetailType.Normal + " and exists (select 1 from IpMaster as im where im.IpNo = i.IpNo and im.Status != " + (int)CodeMaster.IpStatus.Cancel + ")");
                    List<object[]> ipDetail = (from ipd in ipDetailList
                                               where (string)ipd[0] == scheduleBody.OrderNo && (string)ipd[1] == scheduleBody.Sequence
                                               select ipd).ToList();
                    //汇总已处理数
                    decimal handledQty = ipDetail.Select(o => (bool)o[4] ? (decimal)o[3] : ((decimal)o[3] == 0 ? (decimal)o[2] : (decimal)o[3])).Sum();
                    scheduleBody.HandledQty = handledQty.ToString("0.########");
                    //scheduleBody.ShipQty = allRowCellList.Where(q => q.OrderNo == scheduleBody.OrderNo && q.Sequence == scheduleBody.Sequence
                    //                                     && q.OrderQty > q.ShippedQty && q.EndDate <= dateTimeNow.AddDays(Convert.ToDouble(scheduleBody.FreezeDays))).Sum(q => (q.OrderQty - q.ShippedQty)).ToString("0.########");
                    scheduleBody.ShipQty = (orderQtyInFreeze - handledQty).ToString("0.########");

                    //已发货数
                    //scheduleBody.ShippedQty = allRowCellList.Where(q => q.OrderNo == scheduleBody.OrderNo && q.Sequence == scheduleBody.Sequence
                    //                                   && q.ShippedQty > 0).Sum(q => q.ShippedQty).ToString("0.########");
                    decimal shippedQty = ipDetail.Select(o => (bool)o[4] && (decimal)o[3] == 0 ? 0 : (decimal)o[2]).Sum();
                    scheduleBody.ShippedQty = shippedQty.ToString("0.########");

                    //已收货数
                    decimal receivedQty = ipDetail.Select(o => (decimal)o[3]).Sum();
                    scheduleBody.ReceivedQty = receivedQty.ToString("0.########");

                    //总计划数
                    scheduleBody.OrderQty = allRowCellList.Where(q => q.OrderNo == scheduleBody.OrderNo && q.Sequence == scheduleBody.Sequence //&& q.Flow == scheduleBody.Flow
                                                       && q.OrderQty > 0).Sum(q => q.OrderQty).ToString("0.########");

                    //未来汇总
                    scheduleBody.ForecastQty = allRowCellList.Where(q => q.OrderNo == scheduleBody.OrderNo && q.Sequence == scheduleBody.Sequence //&& q.Flow == scheduleBody.Flow
                                  && q.EndDate >= dateTimeNow.Date
                                   && q.OrderQty > 0
                                   ).Sum(q => q.OrderQty).ToString("0.########");

                    //历史总计划数
                    scheduleBody.BackOrderQty = allRowCellList.Where(q => q.OrderNo == scheduleBody.OrderNo && q.Sequence == scheduleBody.Sequence //&& q.Flow == scheduleBody.Flow
                                   && q.EndDate < dateTimeNow.Date && q.OrderQty > 0).Sum(q => q.OrderQty).ToString("0.########");


                    //if (scheduleHead.ColumnCellList != null && scheduleHead.ColumnCellList.Count > 0)
                    //{
                    //    List<RowCell> rowCellList = new List<RowCell>();
                    //    foreach (ColumnCell columnCell in scheduleHead.ColumnCellList)
                    //    {
                    //        RowCell rowCell = GetRowCell(scheduleBody, columnCell, allRowCellList, dateTimeNow);
                    //        rowCellList.Add(rowCell);
                    //    }
                    //    scheduleBody.RowCellList = rowCellList;
                    //}

                    scheduleBody.RowCellList = allRowCellList.Where(r => r.OrderNo == scheduleBody.OrderNo && r.Sequence == scheduleBody.Sequence && r.Item == scheduleBody.Item
                        //&& r.EndDate >= dateTimeNow
                                   ).OrderBy(r => r.EndDate).ToList();


                }
            }
            //过滤可发货数为0的行
            if (notIncludeZeroShipQty)
                scheduleView.ScheduleBodyList = (from sl in scheduleBodyList
                                                 where Convert.ToDecimal(sl.ShipQty) > 0
                                                 select sl).ToList();
            else
                scheduleView.ScheduleBodyList = scheduleBodyList;
            #endregion

            //}
            scheduleView.ScheduleHead = scheduleHead;
            return scheduleView;
        }


    }
}
