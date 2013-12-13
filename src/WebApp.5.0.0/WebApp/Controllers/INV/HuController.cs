using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using com.Sconit.Entity.Exception;
using com.Sconit.Web.Util;
using Telerik.Web.Mvc;
using com.Sconit.Web.Models.SearchModels.INV;
using com.Sconit.Web.Models;
using com.Sconit.Entity.INV;
using com.Sconit.Entity.VIEW;
using com.Sconit.Entity.SCM;
using com.Sconit.Service;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.ORD;
using com.Sconit.PrintModel.INV;
using AutoMapper;
using com.Sconit.Utility.Report;
using com.Sconit.Utility;
using com.Sconit.Web.Models.SearchModels.BIL;
using com.Sconit.Entity.BIL;
using com.Sconit.Web.Models.SearchModels.SCM;
using com.Sconit.Web.Models.SearchModels.ORD;
using com.Sconit.Entity;

namespace com.Sconit.Web.Controllers.INV
{
    public class HuController : WebAppBaseController
    {

        private static string selectCountInventoryStatement = "select count(*) from PlanBill as p";
        private static string selectInventoryStatement = "select p from PlanBill as p";

        private static string selectCountStatement = "select count(*) from Hu as h";
        private static string selectStatement = "select h from Hu as h";

        private static string selectCountFlowStatement = "select count(*) from FlowDetail as h";
        private static string selectFlowStatement = "select h from FlowDetail as h";

        private static string selectIpCountStatement = "select count(*) from IpDetail as h";
        private static string selectIpStatement = "select h from IpDetail as h";

        private static string selectLocationCountStatement = "select count(*) from IpLocationDetail as h";
        private static string selectLocationStatement = "select h from IpLocationDetail as h";

        private static string selectOrderCountStatement = "select count(*) from OrderDetail as o";
        private static string selectOrderStatement = "select o from OrderDetail as o";

        public IFlowMgr flowMgr { get; set; }
        public IOrderMgr orderMgr { get; set; }
        public IHuMgr huMgr { get; set; }
        public IReportGen reportGen { get; set; }

        #region public method
        public ActionResult Index()
        {
            return View();
        }
        [SconitAuthorize(Permissions = "Url_Supplier_Consignment_Inventory")]
        public ActionResult InventoryIndex()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_Inventory_Hu_New")]
        public ActionResult New()
        {
            TempData["FlowDetailSearchModel"] = null;
            return View();
        }
        [SconitAuthorize(Permissions = "Inventory_ShelfLifeWarning")]
        public ActionResult ShelfLifeWarningIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Supplier_Consignment_Inventory")]
        public ActionResult InventoryList(GridCommand command, PlanBillSearchModel searchModel)
        {
            TempData["PlanBillSearchModel"] = searchModel;
            if (this.CheckSearchModelIsNull(searchModel))
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
        [SconitAuthorize(Permissions = "Url_Supplier_Consignment_Inventory")]
        public ActionResult _InventoryAjaxList(GridCommand command, PlanBillSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<PlanBill>()));
            }
            SearchStatementModel searchStatementModel = PrepareSearchInventoryStatement(command, searchModel);
            return PartialView(GetAjaxPageData<PlanBill>(searchStatementModel, command));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Inventory_ShelfLifeWarning")]
        public ActionResult ShelfLifeWarningList(GridCommand command, HuSearchModel searchModel)
        {
            TempData["HuSearchModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Inventory_ShelfLifeWarning")]
        public ActionResult _AjaxShelfLifeWarningList(GridCommand command, HuSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareSearchStatementTime(command, searchModel);
            return PartialView(GetAjaxPageData<Hu>(searchStatementModel, command));
        }


        [GridAction]
        [SconitAuthorize(Permissions = "Url_Inventory_Hu_View")]
        public ActionResult List(GridCommand command, HuSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Inventory_Hu_View")]
        public ActionResult _AjaxList(GridCommand command, HuSearchModel searchModel)
        {
            TempData["HuSearchModel"] = searchModel;
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<Hu>(searchStatementModel, command));
        }

        public ActionResult HuDetail(string id)
        {
            Hu hu = base.genericMgr.FindById<Hu>(id);
           
            return View("HuDetail", string.Empty, hu);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _Loctrans(string HuId)
        {
            if (string.IsNullOrEmpty(HuId))
            {
                return HttpNotFound();
            }
            else
            {
                string selectStatement = "from LocationTransaction as l where l.HuId=? order by EffectiveDate desc";
                IList<LocationTransaction> loTranDetailList = base.genericMgr.FindAll<LocationTransaction>(selectStatement, HuId);
                return PartialView(loTranDetailList);
            }
        }
        #region  FlowMaster
        [SconitAuthorize(Permissions = "Url_Inventory_Hu_New")]
        public ActionResult FlowMaster()
        {
            return PartialView();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Inventory_Hu_New")]
        public ActionResult _FlowDetailList(GridCommand command, FlowDetailSearchModel searchModel)
        {

            TempData["FlowDetailSearchModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Inventory_Hu_New")]
        public ActionResult _AjaxFlowDetailList(GridCommand command, FlowDetailSearchModel searchModel)
        {

                SearchStatementModel searchStatementModel = PrepareDetailFlowSearchStatement(command, searchModel);
                GridModel<FlowDetail> List = GetAjaxPageData<FlowDetail>(searchStatementModel, command);
                FlowMaster flowMaster = base.genericMgr.FindById<FlowMaster>(searchModel.Flow);

                foreach (FlowDetail flow in List.Data)
                {
                    flow.ManufactureParty = flowMaster.PartyFrom;
                    flow.LotNo = LotNoHelper.GenerateLotNo();
                }
                foreach (FlowDetail flowDetail in List.Data)
                {
                    Item item = base.genericMgr.FindById<Item>(flowDetail.Item);
                    flowDetail.ItemDescription = item.Description;
                }


                return PartialView(List);
            
        }

        [SconitAuthorize(Permissions = "Url_Inventory_Hu_New")]
        public JsonResult CreateHuByFlow(string FlowidStr, string FlowucStr, string FlowsupplierLotNoStr, string FlowqtyStr, bool FlowisExport)
        {
            try
            {
                IList<FlowDetail> nonZeroFlowDetailList = new List<FlowDetail>();
                if (!string.IsNullOrEmpty(FlowidStr))
                {
                    string[] idArray = FlowidStr.Split(',');
                    string[] ucArray = FlowucStr.Split(',');
                    string[] supplierLotNoArray = FlowsupplierLotNoStr.Split(',');
                    string[] qtyArray = FlowqtyStr.Split(',');
                    FlowMaster flowMaster = null;

                    if (idArray != null && idArray.Count() > 0)
                    {
                        for (int i = 0; i < idArray.Count(); i++)
                        {
                            FlowDetail flowDetail = base.genericMgr.FindById<FlowDetail>(Convert.ToInt32(idArray[i]));
                            if (flowMaster == null)
                            {
                                flowMaster = base.genericMgr.FindById<FlowMaster>(flowDetail.Flow);
                            }

                            flowDetail.UnitCount = Convert.ToDecimal(ucArray[i]);
                            flowDetail.SupplierLotNo = supplierLotNoArray[i];
                            flowDetail.LotNo = LotNoHelper.GenerateLotNo();
                            flowDetail.ManufactureParty = flowMaster.PartyFrom;
                            flowDetail.HuQty = Convert.ToDecimal(qtyArray[i]);
                            nonZeroFlowDetailList.Add(flowDetail);
                        }
                    }


                    base.genericMgr.CleanSession();
                    if (flowMaster != null)
                    {
                        IList<Hu> huList = huMgr.CreateHu(flowMaster, nonZeroFlowDetailList);

                        if (FlowisExport)
                        {
                            foreach (var hu in huList)
                            {
                                hu.ManufacturePartyDescription = base.genericMgr.FindById<Party>(hu.ManufactureParty).Name;
                            }
                            IList<PrintHu> printHuList = Mapper.Map<IList<Hu>, IList<PrintHu>>(huList);

                            IList<object> data = new List<object>();
                            data.Add(printHuList);
                            data.Add(CurrentUser.FullName);
                            reportGen.WriteToClient(flowMaster.HuTemplate, data, flowMaster.HuTemplate);
                            return Json(null);
                        }

                        string printUrl = PrintHuList(huList, flowMaster.HuTemplate);
                        SaveSuccessMessage(Resources.INV.Hu.Hu_Template_Point, huList.Count.ToString());
                        return Json(new {PrintUrl = printUrl});
                    }
                }
                return Json(null);
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

        #region OrderMaster

        [SconitAuthorize(Permissions = "Url_Inventory_Hu_New")]
        public ActionResult OrderMaster()
        {
            return PartialView();
        }



        [SconitAuthorize(Permissions = "Url_Inventory_Hu_New")]
        public ActionResult OrderDetailList(GridCommand command, string orderNo)
        {
            ViewBag.OrderNo = orderNo;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            com.Sconit.Entity.ACC.User user = SecurityContextHolder.Get();
            IList<OrderMaster> orderMasterList = null;
            if (user.Code.Trim().ToLower() != "su")
            {
                orderMasterList = base.genericMgr.FindAll<OrderMaster>("from OrderMaster as o where o.OrderNo=?  and exists (select 1 from UserPermissionView as up where up.UserId =" + user.Id + " and up.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + ") and up.PermissionCode = o.PartyFrom)", orderNo);
                if (orderMasterList.Count <= 0)
                {
                    SaveErrorMessage("订单号不存在或您没有权限，请重新输入！");
                }
            }
            return PartialView();
        }


        [GridAction]
        [SconitAuthorize(Permissions = "Url_Inventory_Hu_New")]
        public ActionResult _AjaxOrderDetailList(GridCommand command, string orderNo)
        {
            com.Sconit.Entity.ACC.User user = SecurityContextHolder.Get();
            IList<OrderMaster> orderMasterList=null;
            if(user.Code.Trim().ToLower() != "su")
            {
                //orderMasterList = base.genericMgr.FindAll<OrderMaster>("from OrderMaster as o where o.OrderNo=?  and exists (select 1 from UserPermissionView as up where up.UserId =" + user.Id + " and up.PermissionCategoryType = " + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + " and up.PermissionCode = o.PartyFrom)", orderNo);
                //按订单打印时临时去掉权限判断
                orderMasterList = base.genericMgr.FindAll<OrderMaster>("from OrderMaster as o where o.OrderNo=?", orderNo);
                 if (orderMasterList.Count<= 0)
                {
                     return PartialView(new GridModel(new List<OrderDetail>()));
                }
            }
            SearchStatementModel searchStatementModel = PrepareOrderDetailSearchStatement(command, orderNo);
            GridModel<OrderDetail> List = GetAjaxPageData<OrderDetail>(searchStatementModel, command);
            try
            {
                OrderMaster order = base.genericMgr.FindById<OrderMaster>(orderNo);
                foreach (OrderDetail orderDetail in List.Data)
                {
                    orderDetail.ManufactureParty = order.PartyFrom;
                    orderDetail.HuQty = orderDetail.OrderedQty;
                    orderDetail.LotNo = LotNoHelper.GenerateLotNo();
                }
                return PartialView(List);
            }
            catch (Exception)
            {

                return PartialView(new GridModel(new List<IpLocationDetail>()));
            }
        }

        [SconitAuthorize(Permissions = "Url_Inventory_Hu_New")]
        public JsonResult CreateHuByOrderDetail(string OrderDetailidStr, string OrderDetailucStr, string OrderDetailsupplierLotNoStr, string OrderDetailqtyStr, bool OrderDetailisExport)
        {
            try
            {
                IList<OrderDetail> nonZeroOrderDetailList = new List<OrderDetail>();
                if (!string.IsNullOrEmpty(OrderDetailidStr))
                {
                    string[] idArray = OrderDetailidStr.Split(',');
                    string[] ucArray = OrderDetailucStr.Split(',');
                    string[] supplierLotNoArray = OrderDetailsupplierLotNoStr.Split(',');
                    string[] qtyArray = OrderDetailqtyStr.Split(',');
                    OrderMaster orderMaster = null;

                    if (idArray != null && idArray.Count() > 0)
                    {
                        for (int i = 0; i < idArray.Count(); i++)
                        {
                            OrderDetail orderDetail = base.genericMgr.FindById<OrderDetail>(Convert.ToInt32(idArray[i]));
                            if (orderMaster == null)
                            {
                                orderMaster = base.genericMgr.FindById<OrderMaster>(orderDetail.OrderNo);
                                orderMaster.HuTemplate = orderMaster.HuTemplate.Trim();
                            }

                            orderDetail.UnitCount = Convert.ToDecimal(ucArray[i]);
                            orderDetail.SupplierLotNo = supplierLotNoArray[i];
                            orderDetail.LotNo = LotNoHelper.GenerateLotNo();
                            orderDetail.ManufactureParty = orderMaster.PartyFrom;
                            orderDetail.HuQty = Convert.ToDecimal(qtyArray[i]);
                            nonZeroOrderDetailList.Add(orderDetail);
                        }
                    }
                    base.genericMgr.CleanSession();
                    if (orderMaster != null)
                    {
                        IList<Hu> huList = huMgr.CreateHu(orderMaster, nonZeroOrderDetailList);
                        foreach (var hu in huList)
                        {
                            hu.ManufacturePartyDescription = base.genericMgr.FindById<Party>(hu.ManufactureParty).Name;
                        }
                        if (string.IsNullOrEmpty(orderMaster.HuTemplate))
                        {
                            orderMaster.HuTemplate = this.systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.DefaultBarCodeTemplate);
                        }
                        if (OrderDetailisExport)
                        {
                            IList<PrintHu> printHuList = Mapper.Map<IList<Hu>, IList<PrintHu>>(huList);
                            IList<object> data = new List<object>();
                            data.Add(printHuList);
                            data.Add(CurrentUser.FullName);
                           
                            reportGen.WriteToClient(orderMaster.HuTemplate, data, orderMaster.HuTemplate);
                            return Json(null);
                        }
                        else
                        {
                            string printUrl = PrintHuList(huList, orderMaster.HuTemplate);
                            SaveSuccessMessage(Resources.INV.Hu.Hu_Template_Point, huList.Count.ToString());
                            return Json( new { PrintUrl = printUrl });
                        }
                    }
                }
                return Json(null);
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

        #region IpMaster
        [SconitAuthorize(Permissions = "Url_Inventory_Hu_New")]
        public ActionResult IpMaster()
        {
            return PartialView();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Inventory_Hu_New")]
        public ActionResult IpDetailList(GridCommand command, IpDetailSearchModel searchModel)
        {
            IpMaster Ipmaster = null;
            TempData["IpDetailSearchModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            if (string.IsNullOrEmpty(searchModel.IpNo))
            {
                SaveWarningMessage("请根据送货单查询");
            }
            else
            {
                TempData["_AjaxMessage"] = "";
                try
                {
                    Ipmaster = base.genericMgr.FindById<IpMaster>(searchModel.IpNo);
                }
                catch (System.Exception)
                {
                    SaveWarningMessage("送货单号不存在，请重新输入！");
                    return PartialView();
                }
                if (Ipmaster.IsShipScanHu)
                {
                    return PartialView("IpLocationDetailList");
                }
            }
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Inventory_Hu_New")]
        public ActionResult _AjaxIpDetailList(GridCommand command, IpDetailSearchModel searchModel)
        {

            if (string.IsNullOrEmpty(searchModel.IpNo))
            {
                return PartialView(new GridModel(new List<IpDetail>()));
            }
            else
            {
                SearchStatementModel searchStatementModel = PrepareIpDetailSearchStatement(command, searchModel);
                GridModel<IpDetail> List = GetAjaxPageData<IpDetail>(searchStatementModel, command);
                foreach (IpDetail ipDetail in List.Data)
                {
                    IpMaster item = base.genericMgr.FindById<IpMaster>(ipDetail.IpNo);
                    ipDetail.ManufactureParty = item.PartyFrom;
                    ipDetail.LotNo = LotNoHelper.GenerateLotNo();
                    ipDetail.HuQty = ipDetail.Qty;
                }
                return PartialView(List);
            }
        }

        [SconitAuthorize(Permissions = "Url_Inventory_Hu_New")]
        public JsonResult CreateHuByIpDetail(string IpDetailidStr, string IpDetailucStr, string IpDetailsupplierLotNoStr, string IpDetailqtyStr, bool IpDetailisExport)
        {
            try
            {
                IList<IpDetail> nonZeroIpDetailList = new List<IpDetail>();
                if (!string.IsNullOrEmpty(IpDetailidStr))
                {
                    string[] idArray = IpDetailidStr.Split(',');
                    string[] ucArray = IpDetailucStr.Split(',');
                    string[] supplierLotNoArray = IpDetailsupplierLotNoStr.Split(',');
                    string[] qtyArray = IpDetailqtyStr.Split(',');
                    IpMaster ipMaster = null;

                    if (idArray != null && idArray.Count() > 0)
                    {
                        for (int i = 0; i < idArray.Count(); i++)
                        {
                            IpDetail ipDetail = base.genericMgr.FindById<IpDetail>(Convert.ToInt32(idArray[i]));
                            if (ipMaster == null)
                            {
                                ipMaster = base.genericMgr.FindById<IpMaster>(ipDetail.IpNo);
                                ipMaster.HuTemplate = ipMaster.HuTemplate.Trim();
                            }

                            ipDetail.UnitCount = Convert.ToDecimal(ucArray[i]);
                            ipDetail.SupplierLotNo = supplierLotNoArray[i];
                            ipDetail.LotNo = LotNoHelper.GenerateLotNo();
                            ipDetail.ManufactureParty = ipMaster.PartyFrom;
                            ipDetail.HuQty = Convert.ToDecimal(qtyArray[i]);
                            nonZeroIpDetailList.Add(ipDetail);
                        }
                    }
                    base.genericMgr.CleanSession();

                    if (ipMaster != null)
                    {
                        IList<Hu> huList = huMgr.CreateHu(ipMaster, nonZeroIpDetailList);
                        foreach (var hu in huList)
                        {
                            hu.ManufacturePartyDescription = base.genericMgr.FindById<Party>(hu.ManufactureParty).Name;
                        }
                        if (string.IsNullOrEmpty(ipMaster.HuTemplate))
                        {
                            ipMaster.HuTemplate = this.systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.DefaultBarCodeTemplate);
                        }


                        if (IpDetailisExport)
                        {
                            IList<PrintHu> printHuList = Mapper.Map<IList<Hu>, IList<PrintHu>>(huList);
                            IList<object> data = new List<object>();
                            data.Add(printHuList);
                            data.Add(CurrentUser.FullName);
                            reportGen.WriteToClient(ipMaster.HuTemplate, data, ipMaster.HuTemplate);
                            return Json(null);
                        }

                        string printUrl = PrintHuList(huList, ipMaster.HuTemplate);
                        SaveSuccessMessage(Resources.INV.Hu.Hu_Template_Point, huList.Count.ToString());
                        return Json( new { PrintUrl = printUrl });
                    }
                }
                return Json(null);
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch(Exception ex)
            {
                SaveErrorMessage(ex);
            }
            return Json(null);
        }

        #region IpLocationDetail

        [SconitAuthorize(Permissions = "Url_Inventory_Hu_New")]
        public JsonResult PrintByIpLocationDetail(string checkedHuIds)
        {
            try
            {
                string[] checkedHuIdArray = checkedHuIds.Split(',');
                string selectStatement = string.Empty;
                IList<object> param = new List<object>();
                foreach (var huId in checkedHuIdArray)
                {
                    if (selectStatement == string.Empty)
                    {
                        selectStatement = "from Hu where HuId in (?";
                    }
                    else
                    {
                        selectStatement += ",?";
                    }
                    param.Add(huId);
                }
                selectStatement += ")";

                IList<Hu> huList = base.genericMgr.FindAll<Hu>(selectStatement, param.ToArray());
                SaveSuccessMessage(Resources.INV.Hu.Hu_HuPrintedByIpMaster);
                string huTemplate = this.systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.DefaultBarCodeTemplate);
                string printUrl = PrintHuList(huList, huTemplate);
                return Json(new { PrintUrl = printUrl });
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch(Exception ex)
            {
                SaveErrorMessage(ex);
            }
            return Json(null);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Inventory_Hu_New")]
        public ActionResult _AjaxIpLocationDetailList(GridCommand command, IpDetailSearchModel searchModel)
        {

            if (string.IsNullOrEmpty(searchModel.IpNo))
            {
                return PartialView(new GridModel(new List<IpLocationDetail>()));
            }
            else
            {
                SearchStatementModel searchStatementModel = PrepareLocationDetailSearchStatement(command, searchModel);
                return PartialView(GetAjaxPageData<IpLocationDetail>(searchStatementModel, command));
            }
        }
        #endregion

        #endregion

        #region Item
        [SconitAuthorize(Permissions = "Url_Inventory_Hu_New")]
        public ActionResult Item()
        {
            return PartialView();
        }

        public ActionResult _GetItemDetail(string itemCode)
        {
            Item item = base.genericMgr.FindById<Item>(itemCode);
            if (item != null)
            {
                item.MinUnitCount = item.UnitCount;
            }

            return this.Json(item);
        }

        [SconitAuthorize(Permissions = "Url_Inventory_Hu_New")]
        public JsonResult CreateHuByItem(string ItemCode, string HuUom, decimal HuUnitCount, string LotNo, decimal HuQty, string ManufactureParty, bool isExport, string supplierLotNo)
        {
            Item item = base.genericMgr.FindById<Item>(ItemCode);
            item.HuUom = HuUom;
            item.HuUnitCount = HuUnitCount;
           // item.supplierLotNo = supplierLotNo;
            item.HuQty = HuQty;
            item.ManufactureParty = ManufactureParty;
            item.LotNo = LotNo;
            item.supplierLotNo = supplierLotNo;
            IList<Hu> huList = huMgr.CreateHu(item);
            string huTemplate = this.systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.DefaultBarCodeTemplate);
            foreach (var hu in huList)
            {
                hu.ManufacturePartyDescription = base.genericMgr.FindById<Party>(hu.ManufactureParty).Name;
            }
            if (isExport)
            {
                IList<PrintHu> printHuList = Mapper.Map<IList<Hu>, IList<PrintHu>>(huList);
                IList<object> data = new List<object>();
                data.Add(printHuList);
                data.Add(CurrentUser.FullName);
               
                reportGen.WriteToClient(huTemplate, data, huTemplate);
                return Json(null);
            }
            else
            {
                string printUrl = PrintHuList(huList, huTemplate);
                SaveSuccessMessage(Resources.INV.Hu.Hu_Template_Point, huList.Count.ToString());
                return Json(new { PrintUrl = printUrl });
            }
        }

        #endregion

        #region SortOrder

        [SconitAuthorize(Permissions = "Url_Inventory_Hu_New")]
        public ActionResult SortOrderHu()
        {
            return PartialView();
        }


        [SconitAuthorize(Permissions = "Url_Inventory_Hu_New")]
        public JsonResult onCreateHuBySortOrder(string orderNo, bool isExport)
        {
           
                IList<OrderDetail> orderDetails = new List<OrderDetail>();
                string hqlDet = @"select  m.PartyFrom,det.OrderQty,ce.ZENGINE,i.Code,i.Desc1,i.RefCode,i.Uom,i.UC,hu.HuId 
                            from ORD_OrderDet_2 as det with(nolock) 
                            left join CUST_EngineTrace as ce  with(nolock)  on ce.TraceCode=det.ReserveNo 
                            inner join md_item as i  with(nolock)  on det.Item=i.Code
                            inner join ord_ordermstr_2 as m  with(nolock)  on det.OrderNo=m.OrderNo
                            left join INV_Hu as hu  with(nolock)  on ce.ZENGINE=hu.HuId  
                            where ce.ZENGINE is not null and det.OrderNo=?
                            union all
                            select  m.PartyFrom,det.OrderQty,ce.ZENGINE,i.Code,i.Desc1,i.RefCode,i.Uom,i.UC,hu.HuId 
                            from ORD_OrderDet_1 as det with(nolock) 
                            left join CUST_EngineTrace as ce  with(nolock)  on ce.TraceCode=det.ReserveNo 
                            inner join md_item as i  with(nolock)  on det.Item=i.Code
                            inner join ord_ordermstr_1 as m  with(nolock)  on det.OrderNo=m.OrderNo
                            left join INV_Hu as hu  with(nolock)  on ce.ZENGINE=hu.HuId  
                            where ce.ZENGINE is not null and det.OrderNo=?";
                IList<object[]> searResultList = this.genericMgr.FindAllWithNativeSql<object[]>(hqlDet,new object[]{ orderNo,orderNo});
                //det.ManufactureParty,det.OrderQty,ce.ZENGINE,i.Code,i.Desc1,i.RefCode,i.Uom,i.UC
                if (searResultList == null || searResultList.Count == 0)
                {
                    SaveErrorMessage(string.Format("排序单号{0}没有找到要打印的明细。",orderNo));
                    return Json(null);
                }
                var itmeList = (from tak in searResultList
                                select new Item
                                {
                                    ManufactureParty = (string)tak[0],
                                    HuQty = (decimal)tak[1],
                                    ZENGINE = (string)tak[2],
                                    Code = (string)tak[3],
                                    Description = (string)tak[4],
                                    ReferenceCode = (string)tak[5],
                                    HuUom = (string)tak[6],
                                    Uom = (string)tak[6],
                                    HuUnitCount = (decimal)tak[7],
                                    HuId = (string)tak[8],
                                }).ToList();
                IList<Hu> huList = new List<Hu>();
                foreach (var item in itmeList)
                {
                    if (string.IsNullOrWhiteSpace(item.HuId))
                    {
                        huList.Add(huMgr.CreateHu(item, item.ZENGINE));
                    }
                    else
                    {
                        huList.Add(this.genericMgr.FindById<Hu>(item.HuId));
                    }
                }
                foreach (var hu in huList)
                {
                    if (!string.IsNullOrWhiteSpace(hu.ManufactureParty))
                    {
                        hu.ManufacturePartyDescription = base.genericMgr.FindById<Party>(hu.ManufactureParty).Name;
                    }
                }
                string huTemplate = this.systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.DefaultBarCodeTemplate);
                if (isExport)
                {
                    IList<PrintHu> printHuList = Mapper.Map<IList<Hu>, IList<PrintHu>>(huList);
                    IList<object> data = new List<object>();
                    data.Add(printHuList);
                    data.Add(CurrentUser.FullName);
                    reportGen.WriteToClient(huTemplate, data, huTemplate);
                    return Json(null);
                }
                else
                {
                    string printUrl = PrintHuList(huList, huTemplate);
                    SaveSuccessMessage(Resources.INV.Hu.Hu_Template_Point, huList.Count.ToString());
                    return Json(new { PrintUrl = printUrl });
                }
        }

        #endregion



        #region 打印导出
        
        public void SaveToClient(string huId)
        {
            string huTemplate = this.systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.DefaultBarCodeTemplate);
            string[] checkedOrderArray = huId.Split(',');
            string selectStatement = string.Empty;
            IList<object> selectPartyPara = new List<object>();
            foreach (var para in checkedOrderArray)
            {
                if (selectStatement == string.Empty)
                {
                    selectStatement = "from Hu where HuId in (?";
                }
                else
                {
                    selectStatement += ",?";
                }
                selectPartyPara.Add(para);
            }
            selectStatement += ")";

            IList<Hu> hu = base.genericMgr.FindAll<Hu>(selectStatement, selectPartyPara.ToArray());

            IList<PrintHu> printHu = Mapper.Map<IList<Hu>, IList<PrintHu>>(hu);
            IList<object> data = new List<object>();
            data.Add(printHu);
            //data.Add(CurrentUser.FullName);
            reportGen.WriteToClient(huTemplate, data, huTemplate);
        }
        public void SaveToClientTo(string checkedOrders)
        {
            string huTemplate = this.systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.DefaultBarCodeTemplate);
            string[] checkedOrderArray = checkedOrders.Split(',');
            string selectStatement = string.Empty;
            IList<object> selectPartyPara = new List<object>();
            foreach (var para in checkedOrderArray)
            {
                if (selectStatement == string.Empty)
                {
                    selectStatement = "from Hu where HuId in (?";
                }
                else
                {
                    selectStatement += ",?";
                }
                selectPartyPara.Add(para);
            }
            selectStatement += ")";

            IList<Hu> huList = base.genericMgr.FindAll<Hu>(selectStatement, selectPartyPara.ToArray());
            foreach (var hu in huList)
            {
                hu.ManufacturePartyDescription = base.genericMgr.FindById<Party>(hu.ManufactureParty).Name;
            }
            IList<PrintHu> printHu = Mapper.Map<IList<Hu>, IList<PrintHu>>(huList);
            IList<object> data = new List<object>();
            data.Add(printHu);
            data.Add(CurrentUser.FullName);
            reportGen.WriteToClient(huTemplate, data, huTemplate);
        }


        public string Print(string huId)
        {
            Hu hu = base.genericMgr.FindById<Hu>(huId);
            string HuTemplate = this.systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.DefaultBarCodeTemplate);
            //Hu hu1 = base.genericMgr.FindById<Hu>("HU0105");
            IList<PrintHu> huList = new List<PrintHu>();

            PrintHu printHu = Mapper.Map<Hu, PrintHu>(hu);
            //PrintHu printHu1 = Mapper.Map<Hu, PrintHu>(hu1);
            printHu.ManufacturePartyDescription = base.genericMgr.FindById<Supplier>(hu.ManufactureParty).Name;
            //printHu.ManufacturePartyDescription = base.genericMgr.FindById<Supplier>(hu1.ManufactureParty).Name;
            huList.Add(printHu);
            //huList.Add(printHu1);
            //huList.Add(printHu);
            IList<object> data = new List<object>();
            data.Add(huList);
            data.Add(CurrentUser.FullName);
            return reportGen.WriteToFile(HuTemplate, data);
        }
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Inventory_Hu_New")]
        public JsonResult FlowPrint(string checkedOrders)
        {
            string[] checkedOrderArray = checkedOrders.Split(',');
            string selectStatement = string.Empty;
            IList<object> selectPartyPara = new List<object>();
            foreach (var para in checkedOrderArray)
            {
                if (selectStatement == string.Empty)
                {
                    selectStatement = "from Hu where HuId in (?";
                }
                else
                {
                    selectStatement += ",?";
                }
                selectPartyPara.Add(para);
            }
            selectStatement += ")";

            IList<Hu> huList = base.genericMgr.FindAll<Hu>(selectStatement, selectPartyPara.ToArray());
            foreach (var hu in huList)
            {
                hu.ManufacturePartyDescription = base.genericMgr.FindById<Party>(hu.ManufactureParty).Name;
            }
            string template = systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.DefaultBarCodeTemplate);
            string reportFileUrl = PrintHuList(huList, template);

            SaveSuccessMessage(Resources.INV.Hu.Hu_HuCreatedByOrder);
            return Json( new { PrintUrl = reportFileUrl });
        }

        public string PrintHuList(IList<Hu> huList, string huTemplate)
        {

            IList<PrintHu> printHuList = Mapper.Map<IList<Hu>, IList<PrintHu>>(huList);

            IList<object> data = new List<object>();
            data.Add(printHuList);
            data.Add(CurrentUser.FullName);
            return reportGen.WriteToFile(huTemplate, data);
        }

        #endregion

        #endregion

        #region private method
        private SearchStatementModel PrepareSearchStatement(GridCommand command, HuSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("HuId", searchModel.HuId, HqlStatementHelper.LikeMatchMode.Start, "h", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("CreateUserName", searchModel.CreateUserName, HqlStatementHelper.LikeMatchMode.Start, "h", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "h", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("SupplierLotNo", searchModel.SupplierLotNo, "h", ref whereStatement, param);


            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.StartDate, searchModel.EndDate, "h", ref whereStatement, param);
            }
            else if (searchModel.StartDate != null & searchModel.EndDate == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.StartDate, "h", ref whereStatement, param);
            }
            else if (searchModel.StartDate == null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.EndDate, "h", ref whereStatement, param);
            }

            if (searchModel.RemindExpireDate_start != null & searchModel.RemindExpireDate_End != null)
            {
                HqlStatementHelper.AddBetweenStatement("RemindExpireDate", searchModel.RemindExpireDate_start, searchModel.RemindExpireDate_End, "h", ref whereStatement, param);
            }
            else if (searchModel.RemindExpireDate_start != null & searchModel.RemindExpireDate_End == null)
            {
                HqlStatementHelper.AddGeStatement("RemindExpireDate", searchModel.RemindExpireDate_start, "h", ref whereStatement, param);
            }
            else if (searchModel.RemindExpireDate_start == null & searchModel.RemindExpireDate_End != null)
            {
                HqlStatementHelper.AddLeStatement("RemindExpireDate", searchModel.RemindExpireDate_End, "h", ref whereStatement, param);
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


        private SearchStatementModel PrepareSearchStatementTime(GridCommand command, HuSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            HqlStatementHelper.AddEqStatement("Item", searchModel.HuId, "h", ref whereStatement, param);

            //if (searchModel.RemindExpireDate_start != null & searchModel.RemindExpireDate_End != null)
            //{
            //    HqlStatementHelper.AddBetweenStatement("RemindExpireDate", searchModel.RemindExpireDate_start, searchModel.RemindExpireDate_End, "h", ref whereStatement, param);
            //}
            //else if (searchModel.RemindExpireDate_start != null & searchModel.RemindExpireDate_End == null)
            //{
            //    HqlStatementHelper.AddGeStatement("RemindExpireDate", searchModel.RemindExpireDate_start, "h", ref whereStatement, param);
            //}
            //else if (searchModel.RemindExpireDate_start == null & searchModel.RemindExpireDate_End != null)
            //{
            //    HqlStatementHelper.AddLeStatement("RemindExpireDate", searchModel.RemindExpireDate_End, "h", ref whereStatement, param);
            //}
            //else
            //{
            HqlStatementHelper.AddLeStatement("RemindExpireDate", DateTime.Now, "h", ref whereStatement, param);
            //}

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }


        #endregion

        #region private


        private SearchStatementModel PrepareSearchInventoryStatement(GridCommand command, PlanBillSearchModel searchModel)
        {
            string whereStatement = string.Empty;
            IList<object> param = new List<object>();

            SecurityHelper.AddPartyFromPermissionStatement(ref whereStatement, "p", "Party", com.Sconit.CodeMaster.OrderType.Procurement, true);

            HqlStatementHelper.AddLikeStatement("OrderNo", searchModel.OrderNo, HqlStatementHelper.LikeMatchMode.Start, "p", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("ReceiptNo", searchModel.ReceiptNo, HqlStatementHelper.LikeMatchMode.Start, "p", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "p", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Party", searchModel.Party, "p", ref whereStatement, param);


            if (searchModel.CreateDate_start != null & searchModel.CreateDate_End != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.CreateDate_start, searchModel.CreateDate_End, "p", ref whereStatement, param);
            }
            else if (searchModel.CreateDate_start != null & searchModel.CreateDate_End == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.CreateDate_start, "p", ref whereStatement, param);
            }
            else if (searchModel.CreateDate_start == null & searchModel.CreateDate_End != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.CreateDate_End, "p", ref whereStatement, param);
            }
            if (whereStatement == string.Empty)
                whereStatement += " where p.PlanQty>p.ActingQty";
            else
                whereStatement += " and p.PlanQty>p.ActingQty";
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountInventoryStatement;
            searchStatementModel.SelectStatement = selectInventoryStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }


        private SearchStatementModel PrepareDetailFlowSearchStatement(GridCommand command, FlowDetailSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "h", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "h", ref whereStatement, param);


            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountFlowStatement;
            searchStatementModel.SelectStatement = selectFlowStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }


        private SearchStatementModel PrepareOrderDetailSearchStatement(GridCommand command, string orderNo)
        {
            
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();
            //SecurityHelper.AddPartyFromPermissionStatement(ref whereStatement, "p", "Party", com.Sconit.CodeMaster.OrderType.Procurement, false);
            HqlStatementHelper.AddEqStatement("OrderNo", orderNo, "o", ref whereStatement, param);


            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectOrderCountStatement;
            searchStatementModel.SelectStatement = selectOrderStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }


        private SearchStatementModel PrepareIpDetailSearchStatement(GridCommand command, IpDetailSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "h", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("IpNo", searchModel.IpNo, "h", ref whereStatement, param);


            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectIpCountStatement;
            searchStatementModel.SelectStatement = selectIpStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }


        private SearchStatementModel PrepareLocationDetailSearchStatement(GridCommand command, IpDetailSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "h", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("IpNo", searchModel.IpNo, "h", ref whereStatement, param);


            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectLocationCountStatement;
            searchStatementModel.SelectStatement = selectLocationStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        #endregion

    }
}
