
namespace com.Sconit.Web.Controllers.ORD
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Services.Protocols;
    using com.Sconit.Entity;
    using com.Sconit.Entity.Exception;
    using com.Sconit.Entity.MD;
    using com.Sconit.Entity.ORD;
    using com.Sconit.Service;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Models.ORD;
    using com.Sconit.Web.Models.SearchModels.ORD;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;
    using Telerik.Web.Mvc.UI;
    using System.Text;


    public class ScheduleLineController : WebAppBaseController
    {
        public IIpMgr ipMgr { get; set; }
        public IOrderMgr orderMgr { get; set; }

        public ScheduleLineController()
        {
        }

        [SconitAuthorize(Permissions = "Url_OrderMstr_ScheduleLine_Search")]
        public ActionResult Index()
        {
            return View();
        }
        [SconitAuthorize(Permissions = "ScheduleLine_view")]
        public ActionResult ScheduleLineIndex()
        {
            return View();
        }

        //[GridAction]
        //[SconitAuthorize(Permissions = "Url_OrderMstr_ScheduleLine_Search")]
        //public ActionResult List(GridCommand command, OrderMasterSearchModel searchModel)
        //{
        //    TempData["OrderMasterSearchModel"] = searchModel;

        //    if (string.IsNullOrWhiteSpace(searchModel.Flow))
        //    {
        //        SaveWarningMessage("请选择一条路线");
        //    }

        //    DateTime dateTimeNow = DateTime.Now;
        //    ScheduleView scheduleView = PrepareScheduleView(searchModel.Flow, searchModel.Item, searchModel.ScheduleType, searchModel.DateFrom, searchModel.DateTo, dateTimeNow);

        //    #region  grid column
        //    var columns = new List<GridColumnSettings>();
        //    columns.Add(new GridColumnSettings
        //    {
        //        Member = scheduleView.ScheduleHead.OrderNoHead,
        //        Title = Resources.ORD.OrderDetail.OrderDetail_ScheduleLineNo,
        //        Sortable = false
        //    });
        //    columns.Add(new GridColumnSettings
        //    {
        //        Member = scheduleView.ScheduleHead.SequenceHead,
        //        Title = Resources.ORD.OrderDetail.OrderDetail_ScheduleLineSeq,
        //        Sortable = false
        //    });
        //    columns.Add(new GridColumnSettings
        //    {
        //        Member = scheduleView.ScheduleHead.ItemHead,
        //        Title = Resources.ORD.OrderDetail.OrderDetail_Item,
        //        Sortable = false
        //    });
        //    columns.Add(new GridColumnSettings
        //    {
        //        Member = scheduleView.ScheduleHead.ItemDescriptionHead,
        //        Title = Resources.ORD.OrderDetail.OrderDetail_ItemDescription,
        //        Sortable = false
        //    });
        //    columns.Add(new GridColumnSettings
        //    {
        //        Member = scheduleView.ScheduleHead.ReferenceItemCodeHead,
        //        Title = Resources.ORD.OrderDetail.OrderDetail_ReferenceItemCode,
        //        Sortable = false
        //    });
        //    columns.Add(new GridColumnSettings
        //    {
        //        Member = scheduleView.ScheduleHead.UomHead,
        //        Title = Resources.ORD.OrderDetail.OrderDetail_Uom,
        //        Sortable = false
        //    });
        //    columns.Add(new GridColumnSettings
        //    {
        //        Member = scheduleView.ScheduleHead.UnitCountHead,
        //        Title = Resources.ORD.OrderDetail.OrderDetail_UnitCount,
        //        Sortable = false
        //    });
        //    columns.Add(new GridColumnSettings
        //    {
        //        Member = scheduleView.ScheduleHead.LocationToHead,
        //        Title = Resources.ORD.OrderDetail.OrderDetail_LocationTo,
        //        Sortable = false
        //    });
        //    columns.Add(new GridColumnSettings
        //    {
        //        Member = scheduleView.ScheduleHead.CurrentShipQtyHead,
        //        Title = Resources.ORD.OrderDetail.OrderDetail_CurrentShipQty,
        //        Sortable = false
        //    });
        //    #endregion

        //    #region
        //    if (scheduleView.ScheduleHead.ColumnCellList != null && scheduleView.ScheduleHead.ColumnCellList.Count > 0)
        //    {
        //        for (int i = 0; i < scheduleView.ScheduleHead.ColumnCellList.Count; i++)
        //        {
        //            //string ScheduleType = scheduleView.ScheduleHead.ColumnCellList[i].ScheduleType.ToString() == "Firm" ? "固定" : "预测";
        //            //columns.Add(new GridColumnSettings
        //            //{
        //            //    Member = "RowCellList[" + i + "].DisplayQty",
        //            //    MemberType = typeof(string),
        //            //    Title = (scheduleView.ScheduleHead.ColumnCellList[i].EndDate.Value < dateTimeNow) ? "欠交" : (scheduleView.ScheduleHead.ColumnCellList[i].EndDate.Value.ToString() + "(" + ScheduleType + ")"),
        //            //    Sortable = false
        //            //});
        //        }
        //    }
        //    #endregion

        //    ViewData["columns"] = columns.ToArray();

        //    IList<ScheduleBody> scheduleBodyList = scheduleView.ScheduleBodyList != null && scheduleView.ScheduleBodyList.Count > 0 ? scheduleView.ScheduleBodyList.OrderBy(s => s.Item).ThenBy(s => s.OrderNo).ToList() : new List<ScheduleBody>();
        //    return View(scheduleBodyList);
        //}

        #region
        //[SconitAuthorize(Permissions = "Url_OrderMstr_ScheduleLine_Search")]
        //public ActionResult _ScheduleLineList(GridCommand command, OrderMasterSearchModel searchModel)
        //{
        //    TempData["OrderMasterSearchModel"] = searchModel;

        //    if (string.IsNullOrWhiteSpace(searchModel.Flow))
        //    {
        //        SaveWarningMessage("请选择一条路线");
        //        return View();
        //    }
        //    #region 重新调用取最新的
        //    FlowMaster flowMaster = base.genericMgr.FindById<FlowMaster>(searchModel.Flow);
        //    string item = searchModel.Item != string.Empty && searchModel.Item != null ? searchModel.Item : string.Empty;
        //    Region region = base.genericMgr.FindById<Region>(flowMaster.PartyTo);
        //    SAPService.SAPService sapService = new SAPService.SAPService();
        //    com.Sconit.Entity.ACC.User user = SecurityContextHolder.Get();
        //    Supplier supplier = base.genericMgr.FindById<Supplier>(flowMaster.PartyFrom);
        //    DateTime lastModifyDate = supplier.LastRefreshDate == null ? DateTime.Now : supplier.LastRefreshDate.Value;
        //    IList<com.Sconit.Web.SAPService.OrderDetail> scheduleList = sapService.GetProcOrders(user.Code, item, flowMaster.PartyFrom, region.Plant, lastModifyDate,false);


        //    #endregion

        //    DateTime dateTimeNow = DateTime.Now;
        //    int listDays = searchModel.ListDays == null ? 21 : (searchModel.ListDays.Value > 0 ? searchModel.ListDays.Value : 0);
        //    ScheduleView scheduleView = PrepareScheduleView(searchModel.Flow, searchModel.Item, dateTimeNow, listDays, scheduleList.Where(s => s.Flow == searchModel.Flow).ToList());

        //    #region  grid column
        //    var columns = new List<GridColumnSettings>();
        //    columns.Add(new GridColumnSettings
        //    {
        //        Member = scheduleView.ScheduleHead.OrderNoHead,
        //        Title = Resources.ORD.OrderDetail.OrderDetail_ScheduleLineNo,
        //        Sortable = false
        //    });
        //    columns.Add(new GridColumnSettings
        //    {
        //        Member = scheduleView.ScheduleHead.SequenceHead,
        //        Title = Resources.ORD.OrderDetail.OrderDetail_ScheduleLineSeq,
        //        Sortable = false
        //    });
        //    columns.Add(new GridColumnSettings
        //    {
        //        Member = scheduleView.ScheduleHead.ItemHead,
        //        Title = Resources.ORD.OrderDetail.OrderDetail_Item,
        //        Sortable = false
        //    });
        //    columns.Add(new GridColumnSettings
        //    {
        //        Member = scheduleView.ScheduleHead.ItemDescriptionHead,
        //        Title = Resources.ORD.OrderDetail.OrderDetail_ItemDescription,
        //        Sortable = false
        //    });
        //    columns.Add(new GridColumnSettings
        //    {
        //        Member = scheduleView.ScheduleHead.ReferenceItemCodeHead,
        //        Title = Resources.ORD.OrderDetail.OrderDetail_ReferenceItemCode,
        //        Sortable = false
        //    });
        //    columns.Add(new GridColumnSettings
        //    {
        //        Member = scheduleView.ScheduleHead.UomHead,
        //        Title = Resources.ORD.OrderDetail.OrderDetail_Uom,
        //        Sortable = false
        //    });
        //    columns.Add(new GridColumnSettings
        //    {
        //        Member = scheduleView.ScheduleHead.UnitCountHead,
        //        Title = Resources.ORD.OrderDetail.OrderDetail_UnitCount,
        //        Sortable = false
        //    });
        //    columns.Add(new GridColumnSettings
        //    {
        //        Member = scheduleView.ScheduleHead.LocationToHead,
        //        Title = Resources.ORD.OrderDetail.OrderDetail_LocationTo,
        //        Sortable = false
        //    });
        //    //columns.Add(new GridColumnSettings
        //    //{
        //    //    Member = scheduleView.ScheduleHead.CurrentShipQtyHead,
        //    //    Title = Resources.ORD.OrderDetail.OrderDetail_CurrentShipQty,
        //    //    Sortable = false
        //    //});

        //    //可发货数
        //    columns.Add(new GridColumnSettings
        //    {
        //        Member = scheduleView.ScheduleHead.ShipQtyHead,
        //        Title = Resources.ORD.OrderDetail.OrderDetail_ShipQty,
        //        Sortable = false
        //    });
        //    //已发货数
        //    columns.Add(new GridColumnSettings
        //    {
        //        Member = scheduleView.ScheduleHead.ShippedQtyHead,
        //        Title = Resources.ORD.OrderDetail.OrderDetail_ShippedQty,
        //        Sortable = false
        //    });
        //    //已收货数
        //    columns.Add(new GridColumnSettings
        //    {
        //        Member = scheduleView.ScheduleHead.ReceivedQtyHead,
        //        Title = Resources.ORD.OrderDetail.OrderDetail_ReceivedQty,
        //        Sortable = false
        //    });
        //    //总计划数
        //    columns.Add(new GridColumnSettings
        //    {
        //        Member = scheduleView.ScheduleHead.OrderQtyHead,
        //        Title = Resources.ORD.OrderDetail.OrderDetail_TotalOrderQty,
        //        Sortable = false
        //    });
        //    //历史计划数
        //    columns.Add(new GridColumnSettings
        //    {
        //        Member = scheduleView.ScheduleHead.BackOrderQtyHead,
        //        Title = Resources.ORD.OrderDetail.OrderDetail_BackOrderQty,
        //        Sortable = false
        //    });
        //    //未来汇总
        //    columns.Add(new GridColumnSettings
        //    {
        //        Member = scheduleView.ScheduleHead.ForecastQtyHead,
        //        Title = Resources.ORD.OrderDetail.OrderDetail_ForecastQty,
        //        Sortable = false
        //    });
        //    #endregion

        //    #region
        //    if (scheduleView.ScheduleHead.ColumnCellList != null && scheduleView.ScheduleHead.ColumnCellList.Count > 0)
        //    {
        //        for (int i = 0; i < listDays; i++)
        //        {
        //            columns.Add(new GridColumnSettings
        //            {
        //                Member = "RowCellList[" + i + "].DisplayQty",
        //                MemberType = typeof(string),
        //                Title = (scheduleView.ScheduleHead.ColumnCellList[i].EndDate.Value < dateTimeNow.Date) ? "欠交" : scheduleView.ScheduleHead.ColumnCellList[i].EndDate.Value.Date.ToShortDateString(),
        //                Sortable = false
        //            });

        //        }
        //    }
        //    #endregion

        //    ViewData["columns"] = columns.ToArray();

        //    IList<ScheduleBody> scheduleBodyList = scheduleView.ScheduleBodyList != null && scheduleView.ScheduleBodyList.Count > 0 ? scheduleView.ScheduleBodyList.OrderBy(s => s.Item).ThenBy(s => s.OrderNo).ToList() : new List<ScheduleBody>();
        //    return PartialView(scheduleBodyList);
        //}


        //[SconitAuthorize(Permissions = "Url_OrderMstr_ScheduleLine_Search")]
        //public ActionResult Refresh(string flow)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(flow))
        //        {
        //            throw new BusinessException("路线代码不能为空");
        //        }
        //        FlowMaster flowMaster = base.genericMgr.FindById<FlowMaster>(flow);
        //        Region region = base.genericMgr.FindById<Region>(flowMaster.PartyTo);
        //        SAPService.SAPService sapService = new SAPService.SAPService();
        //        com.Sconit.Entity.ACC.User user = SecurityContextHolder.Get();
        //        Supplier supplier = base.genericMgr.FindById<Supplier>(flowMaster.PartyFrom);
        //        DateTime lastModifyDate = supplier.LastRefreshDate == null ? DateTime.Now : supplier.LastRefreshDate.Value;
        //        sapService.GetProcOrders(user.Code, string.Empty, flowMaster.PartyFrom, region.Plant, lastModifyDate,true);
        //        SaveSuccessMessage("计划协议刷新成功。");
        //    }
        //    catch (BusinessException ex)
        //    {
        //        SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
        //    }
        //    catch (Exception ex)
        //    {
        //        SaveErrorMessage(ex.Message);
        //    }

        //    TempData["OrderMasterSearchModel"] = new OrderMasterSearchModel { Flow = flow };
        //    return View("Index");
        //}
        #endregion

        [SconitAuthorize(Permissions = "Url_OrderMstr_ScheduleLine_Search")]
        public ActionResult _ScheduleLineList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            try
            {
                TempData["OrderMasterSearchModel"] = searchModel;

                if (string.IsNullOrWhiteSpace(searchModel.PartyFrom) && string.IsNullOrWhiteSpace(searchModel.Item))
                {
                    SaveWarningMessage("请选择一个供应商或物料进行查询。");
                    return View();
                }
                #region 重新调用取最新的
                // FlowMaster flowMaster = base.genericMgr.FindById<FlowMaster>(searchModel.Flow);
                string item = searchModel.Item != string.Empty && searchModel.Item != null ? searchModel.Item : string.Empty;
                // Region region = base.genericMgr.FindById<Region>(flowMaster.PartyTo);
                SAPService.SAPService sapService = new SAPService.SAPService();
                sapService.Url = ReplaceSIServiceUrl(sapService.Url);
                com.Sconit.Entity.ACC.User user = SecurityContextHolder.Get();
                // Supplier supplier = base.genericMgr.FindById<Supplier>(searchModel.PartyFrom);
                // DateTime lastModifyDate = supplier.LastRefreshDate == null ? DateTime.Now : supplier.LastRefreshDate.Value;
                IList<com.Sconit.Web.SAPService.OrderDetail> scheduleList = sapService.GetProcOrders(null, searchModel.PartyFrom, item, "0084", user.Code);
                #endregion

                DateTime dateTimeNow = DateTime.Now;
                int listDays = searchModel.ListDays == null ? 21 : (searchModel.ListDays.Value > 0 ? searchModel.ListDays.Value : 0);
                ScheduleView scheduleView = PrepareScheduleView(searchModel.PartyFrom, searchModel.Item, dateTimeNow, listDays, scheduleList, searchModel.NotIncludeZeroShipQty);

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
                //columns.Add(new GridColumnSettings
                //{
                //    Member = scheduleView.ScheduleHead.CurrentShipQtyHead,
                //    Title = Resources.ORD.OrderDetail.OrderDetail_CurrentShipQty,
                //    Sortable = false
                //});

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
                //    Sortable = false,
                //    Visible = false
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
                    Sortable = false,
                    Visible = false
                });
                //已处理数
                columns.Add(new GridColumnSettings
                {
                    Member = scheduleView.ScheduleHead.HandledQty,
                    Title = "已处理数",
                    Sortable = false,
                    Visible = false
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

        [SconitAuthorize(Permissions = "Url_OrderMstr_ScheduleLine_Search")]
        public ActionResult Refresh(string PartyFrom, string Item)
        {
            try
            {
                if (string.IsNullOrEmpty(PartyFrom))
                {
                    throw new BusinessException("供应商代码不能为空");
                }
                //  FlowMaster flowMaster = base.genericMgr.FindById<FlowMaster>(flow);
                // Region region = base.genericMgr.FindById<Region>(flowMaster.PartyTo);
                SAPService.SAPService sapService = new SAPService.SAPService();
                sapService.Url = ReplaceSIServiceUrl(sapService.Url);
                com.Sconit.Entity.ACC.User user = SecurityContextHolder.Get();
                Supplier supplier = base.genericMgr.FindById<Supplier>(PartyFrom);
                DateTime lastModifyDate = supplier.LastRefreshDate == null ? DateTime.Now : supplier.LastRefreshDate.Value;
                sapService.GetProcOrders(null, PartyFrom, Item, "0084", user.Code);
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

            TempData["OrderMasterSearchModel"] = new OrderMasterSearchModel { PartyFrom = PartyFrom, Item = Item };
            return View("Index");
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderMstr_ScheduleLine_Ship")]
        public JsonResult ShipOrderByQty(string Flow, string OrderNoStr, string SequenceStr, string CurrentShipQtyStr)
        {
            try
            {
                if (!string.IsNullOrEmpty(OrderNoStr))
                {
                    string[] orderNoArray = OrderNoStr.Split(',');
                    string[] sequenceArray = SequenceStr.Split(',');
                    string[] currentShipQtyArray = CurrentShipQtyStr.Split(',');
                    IList<ScheduleLineInput> scheduleLineInputList = new List<ScheduleLineInput>();
                    int i = 0;
                    foreach (string orderNo in orderNoArray)
                    {

                        ScheduleLineInput scheduleLineInput = new ScheduleLineInput();
                        scheduleLineInput.EBELN = orderNoArray[i];
                        scheduleLineInput.EBELP = sequenceArray[i];
                        scheduleLineInput.ShipQty = int.Parse(currentShipQtyArray[i]);
                        scheduleLineInputList.Add(scheduleLineInput);
                        i++;
                    }
                    IpMaster ipMaster = this.orderMgr.ShipScheduleLine(Flow, scheduleLineInputList);
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
            var q = rowCellList.Where(r => r.OrderNo == scheduleBody.OrderNo && r.Sequence == scheduleBody.Sequence
                // && r.ScheduleType == columnCell.ScheduleType
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
                rowCell.Supplier = scheduleBody.Supplier;
                //  rowCell.ScheduleType = columnCell.ScheduleType;
                rowCell.EndDate = columnCell.EndDate;
                rowCell.OrderQty = 0;
                rowCell.ShippedQty = 0;
            }
            return rowCell;
        }

        private ScheduleView PrepareScheduleView(string PartyFrom, string item, DateTime dateTimeNow, int intervalDays, IList<com.Sconit.Web.SAPService.OrderDetail> scheduleList, bool notIncludeZeroShipQty)
        {
            ScheduleView scheduleView = new ScheduleView();
            ScheduleHead scheduleHead = new ScheduleHead();
            if (!string.IsNullOrWhiteSpace(PartyFrom) || !string.IsNullOrWhiteSpace(item))
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
                                                            UnitCount = p.UnitCount,
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
                                                            UnitCount = g.Key.UnitCount,
                                                            LocationTo = string.Join(",", g.Select(s => s.LocationTo)),
                                                            FreezeDays = g.Key.FreezeDays,
                                                            Supplier = g.Key.PartyFrom
                                                        }).ToList();


                IList<RowCell> allRowCellList = (from p in
                                                     (from p in orderDetailList
                                                      select new
                                                      {
                                                          OrderNo = p.ExternalOrderNo,
                                                          ScheduleLineSeq = p.ExternalSequence,
                                                          LesOrderNo = p.OrderNo,
                                                          PartyFrom = p.PartyFrom,
                                                          EndDate = p.EndDate,
                                                          ScheduleType = dateTimeNow.AddDays(Convert.ToDouble(p.FreezeDays)) >= p.EndDate ? com.Sconit.CodeMaster.ScheduleType.Firm : com.Sconit.CodeMaster.ScheduleType.Forecast,
                                                          OrderQty = p.OrderedQty,
                                                          ShippedQty = p.ShipQty,
                                                          ReceivedQty = p.ReceiveQty
                                                      }).Distinct()
                                                 group p by new
                                                 {
                                                     OrderNo = p.OrderNo,
                                                     ScheduleLineSeq = p.ScheduleLineSeq,
                                                     LesOrderNo = p.OrderNo,
                                                     //Flow = p.Flow,
                                                     PartyFrom = p.PartyFrom,
                                                     EndDate = p.EndDate,
                                                     ScheduleType = p.ScheduleType
                                                 } into g
                                                 select new RowCell
                                                 {
                                                     OrderNo = g.Key.OrderNo,
                                                     Sequence = g.Key.ScheduleLineSeq,
                                                     LesOrderNo = g.Key.LesOrderNo,
                                                     //Flow = g.Key.Flow,
                                                     Supplier = g.Key.PartyFrom,
                                                     EndDate = g.Key.EndDate,
                                                     ScheduleType = (com.Sconit.CodeMaster.ScheduleType)g.Key.ScheduleType,
                                                     OrderQty = g.Sum(p => p.OrderQty),
                                                     ShippedQty = g.Sum(p => p.ShippedQty),
                                                     ReceivedQty = g.Sum(p => p.ReceivedQty)
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
                        //明细未关闭时如果已收过货，则取收货数；如果未收过货则取发货数
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
                                       && q.EndDate >= dateTimeNow.Date.AddDays(Convert.ToDouble(intervalDays)) && q.OrderQty > 0).Sum(q => q.OrderQty).ToString("0.########");

                        //历史总计划数
                        scheduleBody.BackOrderQty = allRowCellList.Where(q => q.OrderNo == scheduleBody.OrderNo && q.Sequence == scheduleBody.Sequence //&& q.Flow == scheduleBody.Flow
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

        [GridAction]
        [SconitAuthorize(Permissions = "ScheduleLine_view")]
        public ActionResult ScheduleLineList(GridCommand command, OrderMasterSearchModel searchModel)
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
        [SconitAuthorize(Permissions = "ScheduleLine_view")]
        public ActionResult _AjaxList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<OrderMaster>()));
            }
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchStatement_1(command, searchModel, false);
            return PartialView(GetAjaxPageDataProcedure<OrderMaster>(procedureSearchStatementModel, command));
        }

        #region private method
        private ProcedureSearchStatementModel PrepareSearchStatement_1(GridCommand command, OrderMasterSearchModel searchModel, bool isReturn)
        {
            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.OrderNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Flow, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter
            {
                Parameter = (int)com.Sconit.CodeMaster.OrderType.ScheduleLine,
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
            paraList.Add(new ProcedureParameter { Parameter = isReturn, Type = NHibernate.NHibernateUtil.Boolean });
            paraList.Add(new ProcedureParameter { Parameter = false, Type = NHibernate.NHibernateUtil.Boolean });
            paraList.Add(new ProcedureParameter { Parameter = CurrentUser.Id, Type = NHibernate.NHibernateUtil.Int32 });
            paraList.Add(new ProcedureParameter { Parameter = string.Empty, Type = NHibernate.NHibernateUtil.String });


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
        [SconitAuthorize(Permissions = "ScheduleLine_view")]
        public ActionResult _Edit(string orderNo)
        {
            OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);
            ViewBag.OrderNo = orderNo;
            return View(orderMaster);
        }
        [SconitAuthorize(Permissions = "ScheduleLine_view")]
        public ActionResult _OrderDetailList(string orderNo)
        {
            //string hql =string.Format( "from OrderDetail o where o.OrderNo='{0}'",orderNo);
            //IList<OrderDetail> orderDetailList = base.genericMgr.FindAll<OrderDetail>(hql);
            ViewBag.OrderNo = orderNo;
            return PartialView();
        }
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "ScheduleLine_view")]
        public ActionResult _Update(string orderNo, OrderDetail orderDetail)
        {

            IList<OrderDetail> orderDetailList = genericMgr.FindAll<OrderDetail>(" from OrderDetail as o  where o.OrderNo=? and o.Item=?", new object[] { orderNo, orderDetail.Item });
            foreach (var od in orderDetailList)
            {
                od.UnitCount = orderDetail.UnitCount;
                od.LocationTo = orderDetail.LocationTo;
                base.genericMgr.Update(od);
            }
            string hql = string.Format("from OrderDetail o where o.OrderNo='{0}'", orderNo);
            IList<OrderDetail> orderDetails = base.genericMgr.FindAll<OrderDetail>(hql);
            var groupOrderDetail = from b in orderDetails
                                   group b by new
                                   {
                                       b.Item
                                   } into g
                                   select new OrderDetail
                                   {
                                       OrderNo = g.First().OrderNo,
                                       Item = g.Key.Item,
                                       ItemDescription = g.First().ItemDescription,
                                       UnitCount = g.Sum(p => g.First().UnitCount),
                                       UnitCountDescription = g.First().UnitCountDescription,
                                       LocationTo = g.First().LocationTo,
                                       ReferenceItemCode = g.First().ReferenceItemCode,
                                       Uom = g.First().Uom,
                                       OrderedQty = g.Sum(p => g.First().OrderedQty),
                                       ShippedQty = g.Sum(p => g.First().ShippedQty)
                                   };

            return PartialView(new GridModel(groupOrderDetail));

        }
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "ScheduleLine_view")]
        public ActionResult _Delete(string orderNo, string Item)
        {
            IList<OrderDetail> orderDetailList = base.genericMgr.FindAll<OrderDetail>(" from OrderDetail as o where o.OrderNo=? and o.Item=?", new object[] { orderNo, Item });
            base.genericMgr.Delete(orderDetailList);

            string hql = string.Format("from OrderDetail o where o.OrderNo='{0}'", orderNo);
            IList<OrderDetail> orderDetails = base.genericMgr.FindAll<OrderDetail>(hql);
            var groupOrderDetail = from b in orderDetails
                                   group b by new
                                   {
                                       b.Item
                                   } into g
                                   select new OrderDetail
                                   {
                                       OrderNo = g.First().OrderNo,
                                       Item = g.Key.Item,
                                       ItemDescription = g.First().ItemDescription,
                                       UnitCount = g.Sum(p => g.First().UnitCount),
                                       UnitCountDescription = g.First().UnitCountDescription,
                                       LocationTo = g.First().LocationTo,
                                       ReferenceItemCode = g.First().ReferenceItemCode,
                                       Uom = g.First().Uom,
                                       OrderedQty = g.Sum(p => g.First().OrderedQty),
                                       ShippedQty = g.Sum(p => g.First().ShippedQty)
                                   };

            return PartialView(new GridModel(groupOrderDetail));

        }
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxListTo(string orderNo)
        {
            string hql = string.Format("from OrderDetail o where o.OrderNo='{0}'", orderNo);
            IList<OrderDetail> orderDetailList = base.genericMgr.FindAll<OrderDetail>(hql);
            var groupOrderDetail = from b in orderDetailList
                                   group b by new
                                   {
                                       b.Item
                                   } into g
                                   select new OrderDetail
                                   {
                                       OrderNo = g.First().OrderNo,
                                       Item = g.Key.Item,
                                       ItemDescription = g.First().ItemDescription,
                                       UnitCount = g.Sum(p => g.First().UnitCount),
                                       UnitCountDescription = g.First().UnitCountDescription,
                                       LocationTo = g.First().LocationTo,
                                       ReferenceItemCode = g.First().ReferenceItemCode,
                                       Uom = g.First().Uom,
                                       OrderedQty = g.Sum(p => g.First().OrderedQty),
                                       ShippedQty = g.Sum(p => g.First().ShippedQty)
                                   };

            return PartialView(new GridModel(groupOrderDetail));
        }





        #endregion
    }
}
