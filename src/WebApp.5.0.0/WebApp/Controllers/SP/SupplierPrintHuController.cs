using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.FIS;
using com.Sconit.Entity.INV;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.SCM;
using com.Sconit.Service;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels.INV;
using com.Sconit.Web.Util;
using Telerik.Web.Mvc;
using System.Collections;
using com.Sconit.Utility.Report;
using com.Sconit.Entity;
using com.Sconit.Utility;
using com.Sconit.PrintModel.INV;
using AutoMapper;
using NHibernate.Criterion;
using com.Sconit.Web.Models.SearchModels.SCM;
using com.Sconit.Web.Models.SearchModels.ORD;
using System;

namespace com.Sconit.Web.Controllers.SP
{
    public class SupplierPrintHuController : WebAppBaseController
    {

        private static string selectCountStatement = "select count(*) from Hu as h";
        private static string selectStatement = "select h from Hu as h";


        private static string selectCountFlowStatement = "select count(*) from FlowDetail as h";
        private static string selectFlowStatement = "select h from FlowDetail as h";

        private static string selectIpCountStatement = "select count(*) from IpDetail as h";
        private static string selectIpStatement = "select h from IpDetail as h";

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

        [SconitAuthorize(Permissions = "Url_Supplier_Print_ADD")]
        public ActionResult New()
        {
            TempData["FlowDetailSearchModel"] = null;
            return View();
        }


        [SconitAuthorize(Permissions = "Url_Supplier_Print_ADD")]
        public ActionResult List(GridCommand command, HuSearchModel searchModel)
        {

            TempData["HuSearchModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Supplier_Print_ADD")]
        public ActionResult _AjaxList(GridCommand command, HuSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<Hu>(searchStatementModel, command));
        }

        #region FlowMaster

        [SconitAuthorize(Permissions = "Url_Supplier_Print_ADD")]
        public ActionResult FlowMaster()
        {
            return PartialView();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Supplier_Print_ADD")]
        public ActionResult _FlowDetailList(GridCommand command, FlowDetailSearchModel searchModel)
        {
            TempData["FlowDetailSearchModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Supplier_Print_ADD")]
        public ActionResult _AjaxFlowDetailList(GridCommand command, FlowDetailSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareDetailFlowSearchStatement(command, searchModel);
            GridModel<FlowDetail> List = GetAjaxPageData<FlowDetail>(searchStatementModel, command);

            if (!string.IsNullOrEmpty(searchModel.Flow))
            {
                FlowMaster flowMaster = base.genericMgr.FindById<FlowMaster>(searchModel.Flow);

                foreach (FlowDetail flowDetail in List.Data)
                {
                    flowDetail.ManufactureParty = flowMaster.PartyFrom;
                    flowDetail.LotNo = LotNoHelper.GenerateLotNo();
                }

                foreach (FlowDetail flowDetail in List.Data)
                {
                    Item item = base.genericMgr.FindById<Item>(flowDetail.Item);
                    flowDetail.ItemDescription = item.Description;
                }
            }
            return PartialView(List);

        }

        [SconitAuthorize(Permissions = "Url_Supplier_Print_ADD")]
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
                        foreach (var hu in huList)
                        {
                            hu.ManufacturePartyDescription = base.genericMgr.FindById<Party>(hu.ManufactureParty).Name;
                        }

                        if (FlowisExport)
                        {
                            IList<PrintHu> printHuList = Mapper.Map<IList<Hu>, IList<PrintHu>>(huList);

                            IList<object> data = new List<object>();
                            data.Add(printHuList);
                            data.Add(CurrentUser.FullName);
                            reportGen.WriteToClient(flowMaster.HuTemplate, data, flowMaster.HuTemplate);
                            return Json(null);
                        }
                        else
                        {
                            //CreateBarCode(huList, null);
                            string printUrl = PrintHuList(huList, flowMaster.HuTemplate);
                            SaveSuccessMessage("条码打印成功,共打印了{0}张条码", huList.Count.ToString());
                            return Json(new { PrintUrl = printUrl });
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


        #endregion

        #region  IpMaster
        [SconitAuthorize(Permissions = "Url_Supplier_Print_ADD")]
        public ActionResult IpMaster()
        {
            return PartialView();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Supplier_Print_ADD")]
        public ActionResult IpDetailList(GridCommand command, IpDetailSearchModel searchModel)
        {
            IpMaster Ipmaster = null;
            TempData["IpDetailSearchModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            if (string.IsNullOrEmpty(searchModel.IpNo))
            {
                SaveErrorMessage("请根据送货单查询");
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
                    SaveErrorMessage("送货单号不存在，请重新输入！");
                }
            }
            return PartialView();
        }


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Supplier_Print_ADD")]
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
                foreach (IpDetail flow in List.Data)
                {
                    IpMaster item = base.genericMgr.FindById<IpMaster>(flow.IpNo);
                    flow.ManufactureParty = item.PartyFrom;
                    flow.LotNo = LotNoHelper.GenerateLotNo();
                    flow.HuQty = flow.Qty;
                }
                return PartialView(List);
            }
        }

        [SconitAuthorize(Permissions = "Url_Supplier_Print_ADD")]
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
                    if (string.IsNullOrEmpty(ipMaster.HuTemplate))
                    {
                        ipMaster.HuTemplate = this.systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.DefaultBarCodeTemplate);
                    }


                    if (ipMaster != null)
                    {
                        IList<Hu> huList = huMgr.CreateHu(ipMaster, nonZeroIpDetailList);
                        foreach (var hu in huList)
                        {
                            hu.ManufacturePartyDescription = base.genericMgr.FindById<Party>(hu.ManufactureParty).Name;
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
                        else
                        {
                            //CreateBarCode(huList, ipMaster.IpNo);
                            string printUrl = PrintHuList(huList, ipMaster.HuTemplate);
                            SaveSuccessMessage("条码打印成功,共打印了{0}张条码", huList.Count.ToString());
                            return Json(new { PrintUrl = printUrl });
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


        #endregion

        #region OrderMaster

        [SconitAuthorize(Permissions = "Url_Supplier_Print_ADD")]
        public ActionResult OrderMaster()
        {
            return PartialView();
        }

        [SconitAuthorize(Permissions = "Url_Supplier_Print_ADD")]
        public ActionResult OrderDetailList(GridCommand command, string orderNo)
        {
            ViewBag.OrderNo = orderNo;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            com.Sconit.Entity.ACC.User user = SecurityContextHolder.Get();
            IList<OrderMaster> orderMasterList = null;
            if (user.Code.Trim().ToLower() != "su")
            {
                orderMasterList = base.genericMgr.FindAll<OrderMaster>("from OrderMaster as o where o.OrderNo=?  and exists (select 1 from UserPermissionView as up where up.UserId =" + user.Id + " and up.PermissionCategoryType = " + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + " and up.PermissionCode = o.PartyFrom)", orderNo);
                if (orderMasterList.Count <= 0)
                {
                    SaveErrorMessage("订单号不存在或您没有权限，请重新输入！");
                }
            }
            return PartialView();
        }

        [GridAction]
        public ActionResult _AjaxOrderDetailList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            com.Sconit.Entity.ACC.User user = SecurityContextHolder.Get();
            IList<OrderMaster> orderMasterList = null;
            if (user.Code.Trim().ToLower() != "su")
            {
                orderMasterList = base.genericMgr.FindAll<OrderMaster>("from OrderMaster as o where o.OrderNo=?  and exists (select 1 from UserPermissionView as up where up.UserId =" + user.Id + " and up.PermissionCategoryType = " + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + " and up.PermissionCode = o.PartyFrom)", searchModel.OrderNo);
                if (orderMasterList.Count <= 0)
                {
                    return PartialView(new GridModel(new List<OrderDetail>()));
                }
            }
            SearchStatementModel searchStatementModel = PrepareOrderDetailSearchStatement(command, searchModel);
            GridModel<OrderDetail> List = GetAjaxPageData<OrderDetail>(searchStatementModel, command);
            try
            {
                foreach (OrderDetail orderDetail in List.Data)
                {
                    orderDetail.LotNo = LotNoHelper.GenerateLotNo();
                }
                OrderMaster order = base.genericMgr.FindById<OrderMaster>(searchModel.OrderNo);
                foreach (OrderDetail orderDetail in List.Data)
                {
                    orderDetail.ManufactureParty = order.PartyFrom;
                    orderDetail.HuQty = orderDetail.OrderedQty;
                }

                return View(List);
            }
            catch (Exception)
            {

                return PartialView(new GridModel(new List<IpLocationDetail>()));
            }

        }

        [SconitAuthorize(Permissions = "Url_Supplier_Print_ADD")]
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
                    if (string.IsNullOrEmpty(orderMaster.HuTemplate))
                    {
                        orderMaster.HuTemplate = this.systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.DefaultBarCodeTemplate);
                    }

                    if (orderMaster != null)
                    {
                        IList<Hu> huList = huMgr.CreateHu(orderMaster, nonZeroOrderDetailList);
                        foreach (var hu in huList)
                        {
                            hu.ManufacturePartyDescription = base.genericMgr.FindById<Party>(hu.ManufactureParty).Name;
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
                            //CreateBarCode(huList, null);
                            string printUrl = PrintHuList(huList, orderMaster.HuTemplate);

                            SaveSuccessMessage("条码打印成功,共打印了{0}张条码", huList.Count.ToString());
                            return Json(new { PrintUrl = printUrl });
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


        private SearchStatementModel PrepareOrderDetailSearchStatement(GridCommand command, OrderMasterSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();
            HqlStatementHelper.AddEqStatement("OrderNo", searchModel.OrderNo, "o", ref whereStatement, param);


            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectOrderCountStatement;
            searchStatementModel.SelectStatement = selectOrderStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        #endregion

        #region FX80件
        [SconitAuthorize(Permissions = "Url_Supplier_Print_FX80Hu")]
        public ActionResult FX80HuIndex()
        {
            return View();
        }

        public ActionResult _GetItemDetail(string itemCode)
        {
            Item item = genericMgr.FindById<Item>(itemCode);
            if (item != null)
            {
                item.MinUnitCount = item.UnitCount;
            }

            return this.Json(item);
        }


        [SconitAuthorize(Permissions = "Url_Supplier_Print_FX80Hu")]
        public JsonResult CreateHuFX80(string ItemCode, string HuUom, decimal HuUnitCount, string LotNo, decimal HuQty, string ManufactureParty, bool isExport, string supplierLotNo)
        {
            var user = SecurityContextHolder.Get();
            Item item = genericMgr.FindById<Item>(ItemCode);
            item.HuUom = HuUom;
            item.HuUnitCount = HuUnitCount;
            // item.supplierLotNo = supplierLotNo;
            item.HuQty = HuQty;
            item.ManufactureParty = user.Code;
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
                object obj = new { SuccessMessage = string.Format("条码打印成功,共打印了{0}张条码", huList.Count), PrintUrl = printUrl };
                return Json(obj);
            }
        }

        #endregion

        #region 打印
        public string PrintHuList(IList<Hu> huList, string huTemplate)
        {

            IList<PrintHu> printHuList = Mapper.Map<IList<Hu>, IList<PrintHu>>(huList);

            IList<object> data = new List<object>();
            data.Add(printHuList);
            data.Add(CurrentUser.FullName);
            return reportGen.WriteToFile(huTemplate, data);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Supplier_Print_ADD")]
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
            object obj = new { SuccessMessages = string.Format(Resources.INV.Hu.Hu_HuCreatedByOrder), PrintUrl = reportFileUrl };
            return Json(obj);
        }

        public void SaveToClient(string checkedOrders)
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

        public string Print(string huId, string huTemplate)
        {
            Hu hu = base.genericMgr.FindById<Hu>(huId);
            IList<PrintHu> huList = new List<PrintHu>();

            PrintHu printHu = Mapper.Map<Hu, PrintHu>(hu);
            printHu.ManufacturePartyDescription = base.genericMgr.FindById<Supplier>(hu.ManufactureParty).Name;
            huList.Add(printHu);
            IList<object> data = new List<object>();
            data.Add(huList);
            data.Add(CurrentUser.FullName);
            return reportGen.WriteToFile(huTemplate, data);
        }

        #endregion

        #endregion

        #region private method
        private SearchStatementModel PrepareSearchStatement(GridCommand command, HuSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            com.Sconit.Entity.ACC.User user = SecurityContextHolder.Get();
            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("HuId", searchModel.HuId, HqlStatementHelper.LikeMatchMode.Start, "h", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "h", ref whereStatement, param);

            HqlStatementHelper.AddLikeStatement("LotNo", searchModel.lotNo, HqlStatementHelper.LikeMatchMode.Start, "h", ref whereStatement, param);

            HqlStatementHelper.AddEqStatement("ManufactureParty", searchModel.ManufactureParty, "h", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("CreateUserId", user.Id, "h", ref whereStatement, param);
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

        //private void CreateBarCode(IList<Hu> huList, string asn)
        //{
        //    foreach (var hu in huList)
        //    {
        //        base.genericMgr.Create(new CreateBarCode
        //                                   {
        //                                       HuId = hu.HuId,
        //                                       LotNo = hu.LotNo,
        //                                       Item = hu.Item,
        //                                       Qty = hu.Qty,
        //                                       ManufactureParty = hu.ManufactureParty,
        //                                       ASN = asn,
        //                                       CreateDate = DateTime.Now
        //                                   });
        //    }
        //}
        #endregion



    }
}
