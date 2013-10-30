using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using com.Sconit.Service;
using com.Sconit.Web.Util;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.UI;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels.ORD;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.INV;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.Exception;
using com.Sconit.PrintModel.INV;
using AutoMapper;
using com.Sconit.Utility.Report;
using com.Sconit.Utility;

namespace com.Sconit.Web.Controllers.ORD
{
    public class ShipListController : WebAppBaseController
    {
        public IShipListMgr shipListMgr { get; set; }

        [SconitAuthorize(Permissions = "Url_ShipList_View")]
        public ActionResult Index()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_ShipList_View")]
        public ActionResult Edit(string shipNo)
        {
            if (string.IsNullOrWhiteSpace(shipNo))
            {
                return HttpNotFound();
            }
            else
            {
                return View("Edit", string.Empty, shipNo);
            }
        }

        [SconitAuthorize(Permissions = "Url_ShipList_View")]
        public ActionResult _Edit(string shipNo)
        {

            ShipList shipList = base.genericMgr.FindById<ShipList>(shipNo);

            //ViewBag.isEditable = shipList.Status == com.Sconit.CodeMaster.OrderStatus.Create;
            //ViewBag.editorTemplate = orderMaster.Status == com.Sconit.CodeMaster.OrderStatus.Create ? "" : "ReadonlyTextBox";

            return PartialView(shipList);
        }

        [SconitAuthorize(Permissions = "Url_ShipList_Cancel")]
        public ActionResult Cancel(string id)
        {
            try
            {
                this.shipListMgr.CancelShipList(id);
                SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_Canceled, id);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            return RedirectToAction("Edit", new { shipNo = id });
        }

        [SconitAuthorize(Permissions = "Url_ShipList_Close")]
        public ActionResult Close(string id)
        {
            try
            {
                this.shipListMgr.CloseShipList(id);
                SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_Closed, id);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            return RedirectToAction("Edit", new { shipNo = id });
        }

        [SconitAuthorize(Permissions = "Url_ShipList_New")]
        public ActionResult New()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_ShipList_New")]
        public ActionResult ListNew(GridCommand command, ShipListSearchModel searchModel)
        {
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.SearchModel = searchModel;
            if (String.IsNullOrEmpty(searchModel.Vehicle))
            {
                SaveWarningMessage("车牌号不能为空");
            }
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ShipList_New")]
        public ActionResult _AjaxListNew(GridCommand command, ShipListSearchModel searchModel)
        {
            IList<IpMaster> ips = new List<IpMaster>();
            if (!String.IsNullOrEmpty(searchModel.Vehicle))
            {
                ips =
                    base.genericMgr.FindAll<IpMaster>(
                        "from IpMaster where Vehicle = ? and (ShipNo is null or ShipNo = '')", searchModel.Vehicle);
            }

            GridModel<IpMaster> GridModel = new GridModel<IpMaster>();
            GridModel.Total = ips.Count;
            GridModel.Data = ips;
            ViewBag.Total = GridModel.Total;
            
            return PartialView(GridModel);
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_ShipList_View")]
        public ActionResult List(GridCommand command, ShipListSearchModel searchModel)
        {
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.SearchModel = searchModel;
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ShipList_View")]
        public ActionResult _AjaxList(GridCommand command, ShipListSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<ShipList>(searchStatementModel, command));
        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, ShipListSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            HqlStatementHelper.AddEqStatement("ShipNo", searchModel.ShipNo, "u", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Vehicle", searchModel.Vehicle, "u", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Shipper", searchModel.Shipper, "u", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Status", searchModel.Status, "u", ref whereStatement, param);

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = "select count(*) from ShipList as u";
            searchStatementModel.SelectStatement = "select u from ShipList as u";
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        public ActionResult _IpList(string shipNo)
        {
            ViewBag.ShipNo = shipNo;

            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ShipList_View")]
        public ActionResult _AjaxLoadIpMaster(string shipNo)
        {
            IList<IpMaster> ips = base.genericMgr.FindAll<IpMaster>("from IpMaster where ShipNo = ?", shipNo);

            GridModel<IpMaster> GridModel = new GridModel<IpMaster>();
            GridModel.Total = ips.Count;
            GridModel.Data = ips;
            ViewBag.Total = GridModel.Total;
            return PartialView(GridModel);
        }

        public JsonResult CreateShipList(string Vehicle, string Shipper, string ChosenIds)
        {
            try
            {
                if (String.IsNullOrEmpty(ChosenIds))
                {
                    throw new BusinessException("请先选择送货单!");
                }

                string shipno = shipListMgr.CreateShipList(Vehicle, Shipper, ChosenIds.Split(','));

                string re = "运单创建成功，单号：" + shipno;
                return Json(new { re });
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                return Json(null);
            }
        }
    }
}
