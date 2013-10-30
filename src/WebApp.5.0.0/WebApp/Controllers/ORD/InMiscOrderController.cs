using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using com.Sconit.Web.Models.SearchModels.INV;
using com.Sconit.Web.Util;
using com.Sconit.Web.Models;
using com.Sconit.Entity.ORD;
using com.Sconit.Service;
using com.Sconit.Entity.CUST;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.Exception;
using com.Sconit.Utility;
using com.Sconit.Entity.INV;

namespace com.Sconit.Web.Controllers.ORD
{
    public class InMiscOrderController : WebAppBaseController
    {

        private static string selectCountStatement = "select count(*) from MiscOrderMaster as m";

        private static string selectStatement = "select m from MiscOrderMaster as m";

        public IMiscOrderMgr miscOrderMgr { get; set; }


        #region public method

        #region view
        [SconitAuthorize(Permissions = "Url_Inventory_InMiscOrder_View")]
        public ActionResult Index()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Inventory_InMiscOrder_View")]
        public ActionResult List(GridCommand GridCommand, InMiscOrderSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(GridCommand, searchModel);
            if (this.CheckSearchModelIsNull(searchCacheModel.SearchObject))
            {
                TempData["_AjaxMessage"] = "";
            }
            else
            {
                SaveWarningMessage(Resources.ErrorMessage.Errors_NoConditions);
            }
            ViewBag.PageSize = base.ProcessPageSize(GridCommand.PageSize);
            return View();

        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Inventory_InMiscOrder_View")]
        public ActionResult _AjaxList(GridCommand command, InMiscOrderSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<MiscOrderMaster>()));
            }
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<MiscOrderMaster>(searchStatementModel, command));
        }
        #endregion

        #region new

        [SconitAuthorize(Permissions = "Url_Inventory_InMiscOrder_New")]
        public ActionResult New(string MoveType)
        {

            if (!string.IsNullOrWhiteSpace(MoveType))
            {
                MiscOrderMoveType miscOrderMoveType = base.genericMgr.FindAll<MiscOrderMoveType>("from MiscOrderMoveType as m where m.MoveType=? and m.IOType=?", new object[] { MoveType, com.Sconit.CodeMaster.MiscOrderType.GR })[0];
                TempData["MiscOrderMoveType"] = miscOrderMoveType;
            }
            MiscOrderMaster miscOrderMaster = new MiscOrderMaster();
            miscOrderMaster.EffectiveDate = System.DateTime.Now;
            miscOrderMaster.MoveType = MoveType;
            return View(miscOrderMaster);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_Inventory_InMiscOrder_New")]
        public ActionResult New(MiscOrderMaster miscOrderMaster)
        {
            MiscOrderMoveType MiscOrderMoveType = null;
            if (ModelState.IsValid)
            {
                MiscOrderMoveType = base.genericMgr.FindAll<MiscOrderMoveType>("from MiscOrderMoveType as m where m.MoveType=? and m.IOType=?", new object[] { miscOrderMaster.MoveType, com.Sconit.CodeMaster.MiscOrderType.GR })[0];

                if (MiscOrderMoveType.CheckAsn && miscOrderMaster.Asn == null)
                {
                    SaveErrorMessage("送货单不能为空");
                }
                else if (MiscOrderMoveType.CheckRecLoc && miscOrderMaster.ReceiveLocation == null)
                {
                    SaveErrorMessage("发货库位不能为空");
                }
                else if (MiscOrderMoveType.CheckNote && miscOrderMaster.Note == null)
                {
                    SaveErrorMessage("移动原因不能为空");
                }
                else if (MiscOrderMoveType.CheckCostCenter && miscOrderMaster.CostCenter == null)
                {
                    SaveErrorMessage("成本中心不能为空");
                }
                //else if (MiscOrderMoveType.CheckRefNo && miscOrderMaster.ReferenceNo == null)
                //{
                //    SaveErrorMessage("参考数不能为空");
                //}
                else if (MiscOrderMoveType.CheckDeliverRegion && miscOrderMaster.DeliverRegion == null)
                {
                    SaveErrorMessage("发货工厂不能为空");
                }
                else if (MiscOrderMoveType.CheckWBS && miscOrderMaster.WBS == null)
                {
                    SaveErrorMessage("WBS不能为空");
                }
                else if (MiscOrderMoveType.CheckManufactureParty && (miscOrderMaster.ManufactureParty == string.Empty || miscOrderMaster.ManufactureParty == null))
                {
                    SaveErrorMessage("供应商不能为空");
                }
                else if (MiscOrderMoveType.CheckConsignment == com.Sconit.CodeMaster.CheckConsignment.Consignment && (miscOrderMaster.ManufactureParty == string.Empty || miscOrderMaster.ManufactureParty == null))
                {
                    SaveErrorMessage("供应商不能为空");
                }
                else if (miscOrderMaster.CheckConsignment == com.Sconit.CodeMaster.CheckConsignment.Consignment && (miscOrderMaster.ManufactureParty == string.Empty || miscOrderMaster.ManufactureParty == null))
                {
                    SaveErrorMessage("供应商不能为空");
                }
                else if (miscOrderMaster.CheckConsignment == null)
                {
                    SaveErrorMessage("请选择寄售状态。");
                }
                else
                {
                    
                    try
                    {
                        miscOrderMaster.MoveType = MiscOrderMoveType.MoveType;
                        miscOrderMaster.CancelMoveType = MiscOrderMoveType.CancelMoveType;
                        miscOrderMaster.Consignment = miscOrderMaster.CheckConsignment == com.Sconit.CodeMaster.CheckConsignment.Consignment ? true : false;
                        miscOrderMgr.CreateMiscOrder(miscOrderMaster);
                        SaveSuccessMessage("添加成功");
                        return RedirectToAction("Edit/" + miscOrderMaster.MiscOrderNo);

                    }
                    catch (Exception ex)
                    {
                        SaveErrorMessage(ex.Message);
                    }

                }
            }
            else
            {
                if (miscOrderMaster.MoveType != null)
                {
                    MiscOrderMoveType = base.genericMgr.FindAll<MiscOrderMoveType>("from MiscOrderMoveType as m where m.MoveType=? and m.IOType=?", new object[] { miscOrderMaster.MoveType, com.Sconit.CodeMaster.MiscOrderType.GR })[0];
                }
            }

            TempData["MiscOrderMoveType"] = MiscOrderMoveType;
            return View(miscOrderMaster);
        }
        #endregion

        #region  Edit
        [SconitAuthorize(Permissions = "Url_Inventory_InMiscOrder_New")]
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                MiscOrderMaster miscOrderMaster = base.genericMgr.FindById<MiscOrderMaster>(id);
                miscOrderMaster.StatusDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.MiscOrderStatus, ((int)miscOrderMaster.Status).ToString());
                miscOrderMaster.QualityTypeDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.QualityType, ((int)miscOrderMaster.QualityType).ToString());
                MiscOrderMoveType miscOrderMoveType = base.genericMgr.FindAll<MiscOrderMoveType>("from MiscOrderMoveType as m where m.MoveType=? and m.IOType=?", new object[] { miscOrderMaster.MoveType, com.Sconit.CodeMaster.MiscOrderType.GR })[0];
                ViewBag.editorTemplate = miscOrderMaster.Status == com.Sconit.CodeMaster.MiscOrderStatus.Create ? "" : "ReadonlyTextBox";
                TempData["MiscOrderMoveType"] = miscOrderMoveType;
                return View(miscOrderMaster);
            }
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_Inventory_InMiscOrder_New")]
        public ActionResult Edit(MiscOrderMaster miscOrderMaster)
        {

            if (ModelState.IsValid)
            {
                MiscOrderMoveType MiscOrderMoveType = base.genericMgr.FindAll<MiscOrderMoveType>("from MiscOrderMoveType as m where m.MoveType=? and m.IOType=?", new object[] { miscOrderMaster.MoveType, com.Sconit.CodeMaster.MiscOrderType.GR })[0];
                if (MiscOrderMoveType.CheckAsn && miscOrderMaster.Asn == null)
                {
                    SaveErrorMessage("送货单不能为空");
                }
                else if (MiscOrderMoveType.CheckRecLoc && miscOrderMaster.ReceiveLocation == null)
                {
                    SaveErrorMessage("收货库位不能为空");
                }
                else if (MiscOrderMoveType.CheckNote && miscOrderMaster.Note == null)
                {
                    SaveErrorMessage("移动原因不能为空");
                }
                else if (MiscOrderMoveType.CheckCostCenter && miscOrderMaster.CostCenter == null)
                {
                    SaveErrorMessage("成本中心不能为空");
                }
                //else if (MiscOrderMoveType.CheckRefNo && miscOrderMaster.ReferenceNo == null)
                //{
                //    SaveErrorMessage("参考数不能为空");
                //}
                else if (MiscOrderMoveType.CheckDeliverRegion && miscOrderMaster.DeliverRegion == null)
                {
                    SaveErrorMessage("发货区域不能为空");
                }
                else if (MiscOrderMoveType.CheckWBS && miscOrderMaster.WBS == null)
                {
                    SaveErrorMessage("WBS不能为空");
                }
                else
                {
                    try
                    {
                        miscOrderMgr.UpdateMiscOrder(miscOrderMaster);
                        SaveSuccessMessage("修改成功");
                    }
                    catch (Exception ex)
                    {

                        SaveErrorMessage(ex.Message);
                    }
                }
            }
            return RedirectToAction("Edit/" + miscOrderMaster.MiscOrderNo);

        }

        public ActionResult btnDelete(string id)
        {
            try
            {
                MiscOrderMaster MiscOrderMaster = base.genericMgr.FindById<MiscOrderMaster>(id);
                miscOrderMgr.DeleteMiscOrder(MiscOrderMaster);
                SaveSuccessMessage("删除成功");


            }
            catch (Exception e)
            {
                SaveErrorMessage(e.Message);

            }
            return RedirectToAction("List");

        }
        [SconitAuthorize(Permissions = "Url_Inventory_InMiscOrder_New")]
        public ActionResult btnClose(string id)
        {
            try
            {
                MiscOrderMaster MiscOrderMaster = base.genericMgr.FindById<MiscOrderMaster>(id);
                IList<MiscOrderDetail> miscOrderDetailList = base.genericMgr.FindAll<MiscOrderDetail>("from MiscOrderDetail as m where m.MiscOrderNo=?", MiscOrderMaster.MiscOrderNo);
                if (miscOrderDetailList.Count < 1)
                {
                    SaveErrorMessage("明细为空，不能执行确认");
                }
                else
                {

                    foreach (var miscOrderDetail in miscOrderDetailList)
                    {
                        CheckMiscOrderDetail(miscOrderDetail, MiscOrderMaster.MoveType, (int)com.Sconit.CodeMaster.MiscOrderType.GR);

                    }
                    miscOrderMgr.CloseMiscOrder(MiscOrderMaster);
                    SaveSuccessMessage("确认成功");
                }
            }
            catch (BusinessException e)
            {
                SaveErrorMessage(e);
            }
            catch (Exception e)
            {
                SaveErrorMessage(e.Message);
            }
            return RedirectToAction("Edit/" + id);
        }
        [SconitAuthorize(Permissions = "Url_Inventory_InMiscOrder_New")]
        public ActionResult btnCancel(string id)
        {
            try
            {
                MiscOrderMaster MiscOrderMaster = base.genericMgr.FindById<MiscOrderMaster>(id);
                miscOrderMgr.CancelMiscOrder(MiscOrderMaster);
                SaveSuccessMessage("取消成功");

            }
            catch (Exception e)
            {
                SaveErrorMessage(e.Message);

            }
            return RedirectToAction("Edit/" + id);
        }

        #region MiscOrderDetail
        public ActionResult _MiscOrderDetailNoScanHu(string MiscOrderNo, string MoveType, string Status)
        {
            MiscOrderMoveType MiscOrderMoveType = base.genericMgr.FindAll<MiscOrderMoveType>("from MiscOrderMoveType as m where m.MoveType=? and m.IOType=?", new object[] { MoveType, com.Sconit.CodeMaster.MiscOrderType.GR })[0];
            MiscOrderMaster miscOrder = base.genericMgr.FindById<MiscOrderMaster>(MiscOrderNo);
            ViewBag.editorTemplate = miscOrder.Status == com.Sconit.CodeMaster.MiscOrderStatus.Create ? "" : "ReadonlyTextBox";
            ViewBag.MoveType = MoveType;
            ViewBag.MiscOrderNo = MiscOrderNo;
            ViewBag.Status = Status;
            ViewBag.ReserveLine = MiscOrderMoveType.CheckReserveLine;
            ViewBag.ReserveNo = MiscOrderMoveType.CheckReserveNo;
            ViewBag.EBELN = MiscOrderMoveType.CheckEBELN;
            ViewBag.EBELP = MiscOrderMoveType.CheckEBELP;
            ViewBag.CheckRefNo = MiscOrderMoveType.CheckRefNo;
            return PartialView();

        }



        public ActionResult _MiscOrderDetailIsScanHu(string MiscOrderNo, string MoveType, string Status)
        {
            MiscOrderMoveType MiscOrderMoveType = base.genericMgr.FindAll<MiscOrderMoveType>("from MiscOrderMoveType as m where m.MoveType=? and m.IOType=?", new object[] { MoveType, com.Sconit.CodeMaster.MiscOrderType.GR })[0];

            ViewBag.MoveType = MoveType;
            ViewBag.MiscOrderNo = MiscOrderNo;
            ViewBag.Status = Status;
            ViewBag.ReserveLine = MiscOrderMoveType.CheckReserveLine;
            ViewBag.ReserveNo = MiscOrderMoveType.CheckReserveNo;
            ViewBag.EBELN = MiscOrderMoveType.CheckEBELN;
            ViewBag.EBELP = MiscOrderMoveType.CheckEBELP;
            return PartialView();

        }

        [GridAction]
        public ActionResult _SelectMiscOrderDetail(string MiscOrderNo, string MoveType)
        {
            ViewBag.MiscOrderNo = MiscOrderNo;
            IList<MiscOrderDetail> MiscOrderDetailList = base.genericMgr.FindAll<MiscOrderDetail>("from MiscOrderDetail as m where m.MiscOrderNo=?", MiscOrderNo);
            return View(new GridModel(MiscOrderDetailList));
        }

        [GridAction]
        public ActionResult _SelectMiscOrderLocationDetail(string MiscOrderNo, string MoveType)
        {
            ViewBag.MiscOrderNo = MiscOrderNo;
            IList<MiscOrderDetail> MiscOrderDetailList = base.genericMgr.FindAll<MiscOrderDetail>("from MiscOrderDetail as m where m.MiscOrderNo=?", MiscOrderNo);
            IList<MiscOrderLocationDetail> miscOrderLocationDetailList = base.genericMgr.FindAll<MiscOrderLocationDetail>("from MiscOrderLocationDetail as m where m.MiscOrderNo=?", MiscOrderNo);
            foreach (MiscOrderLocationDetail miscOrderLocationDetail in miscOrderLocationDetailList)
            {
                MiscOrderDetail miscOrderDetail = MiscOrderDetailList.Where(m => m.Id == miscOrderLocationDetail.MiscOrderDetailId).ToList().First();
                miscOrderLocationDetail.ReferenceItemCode = miscOrderDetail.ReferenceItemCode;
                miscOrderLocationDetail.ItemDescription = miscOrderDetail.ItemDescription;
                miscOrderLocationDetail.UnitCount = miscOrderDetail.UnitCount;
                miscOrderLocationDetail.Location = miscOrderDetail.Location;
                miscOrderLocationDetail.ReserveNo = miscOrderDetail.ReserveNo;
                miscOrderLocationDetail.ReserveLine = miscOrderDetail.ReserveLine;
                miscOrderLocationDetail.EBELN = miscOrderDetail.EBELN;
                miscOrderLocationDetail.EBELP = miscOrderDetail.EBELP;
                miscOrderLocationDetail.ManufactureParty = base.genericMgr.FindById<Hu>(miscOrderLocationDetail.HuId).ManufactureParty;

            }
            return View(new GridModel(miscOrderLocationDetailList));
        }

        public ActionResult _WebOrderDetail(string Code)
        {
            if (!string.IsNullOrEmpty(Code))
            {

                WebOrderDetail webOrderDetail = new WebOrderDetail();
                Item item = base.genericMgr.FindById<Item>(Code);
                if (item != null)
                {
                    webOrderDetail.Item = item.Code;
                    webOrderDetail.ItemDescription = item.Description;
                    webOrderDetail.UnitCount = item.UnitCount;
                    webOrderDetail.Uom = item.Uom;
                    webOrderDetail.ReferenceItemCode = item.ReferenceCode;
                }
                return this.Json(webOrderDetail);
            }
            return null;
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_StockTake_Edit")]
        public bool CheckMiscOrderDetail(MiscOrderDetail miscOrderDetail, string MoveType, int IOType)
        {
            bool isValid = true;
            if (!string.IsNullOrEmpty(MoveType))
            {

                MiscOrderMoveType miscOrderMoveType = base.genericMgr.FindAll<MiscOrderMoveType>("from MiscOrderMoveType as m where m.MoveType=? and m.IOType=?", new object[] { MoveType, IOType })[0];
                if (miscOrderMoveType.CheckEBELN && miscOrderDetail.EBELN == null)
                {
                    throw new BusinessException("要货单号不能为空");
                }
                else if (miscOrderMoveType.CheckEBELP && miscOrderDetail.EBELP == null)
                {
                    throw new BusinessException("要货单行号不能为空");
                }
                else if (miscOrderMoveType.CheckReserveLine && miscOrderDetail.ReserveLine == null)
                {
                    throw new BusinessException("预留行不能为空");
                }
                else if (miscOrderMoveType.CheckReserveNo && miscOrderDetail.ReserveNo == null)
                {
                    throw new BusinessException("预留号不能为空");
                }
                else if (miscOrderDetail.Qty == 0)
                {
                    throw new BusinessException("数量不能为空");
                }
                else if (miscOrderDetail.Item == null)
                {
                    throw new BusinessException("物料不能为空");
                }
                else if (miscOrderMoveType.CheckRefNo && string.IsNullOrWhiteSpace(miscOrderDetail.SapOrderNo))
                {
                    throw new BusinessException("Sap订单号不能为空。");
                }
                else
                {
                    if (miscOrderMoveType.CheckRefNo)
                    {
                        if (this.genericMgr.FindAllWithNativeSql<int>("select COUNT(*) from ORD_OrderMstr_4 WITH(NOLOCK) where ExtOrderNo=? ", miscOrderDetail.SapOrderNo.PadLeft(12, '0'))[0] == 0)
                        {
                            throw new BusinessException("Sap订单号不存在");
                        }
                        miscOrderDetail.SapOrderNo = miscOrderDetail.SapOrderNo.PadLeft(12, '0');
                    }
                    //IList<MiscOrderDetail> MiscOrderDetailist = base.genericMgr.FindAll<MiscOrderDetail>("from MiscOrderDetail as m where m.Item=? and m.MiscOrderNo=?", new object[] { miscOrderDetail.Item, miscOrderDetail.MiscOrderNo });
                    //if (MiscOrderDetailist.Count > 0 )
                    //{
                    //    throw new BusinessException("物料已经存在");
                    //}
                }
            }
            return isValid;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Edit")]
        public JsonResult _SaveMiscOrderDetail(
            [Bind(Prefix = "updated")]IEnumerable<MiscOrderDetail> updatedMiscOrderDetails,
            [Bind(Prefix = "deleted")]IEnumerable<MiscOrderDetail> deletedMiscOrderDetails, string MiscOrderNo, string moveType)
        {
            try
            {
                IList<MiscOrderDetail> newMiscOrderDetailList = new List<MiscOrderDetail>();
                IList<MiscOrderDetail> updateMiscOrderDetailList = new List<MiscOrderDetail>();
                if (updatedMiscOrderDetails != null)
                {
                    foreach (var miscOrderDetail in updatedMiscOrderDetails)
                    {
                        if (CheckMiscOrderDetail(miscOrderDetail, moveType, (int)com.Sconit.CodeMaster.MiscOrderType.GR))
                        {
                            updateMiscOrderDetailList.Add(miscOrderDetail);
                        }
                    }
                }

                miscOrderMgr.BatchUpdateMiscOrderDetails(MiscOrderNo, newMiscOrderDetailList, updateMiscOrderDetailList, (IList<MiscOrderDetail>)deletedMiscOrderDetails);
                SaveSuccessMessage(Resources.ORD.MiscOrderDetail.MiscOrderDetail_Saved);
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

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderMstr_Procurement_Edit")]
        public JsonResult _SaveBatchEditing([Bind(Prefix =
            "inserted")]IEnumerable<MiscOrderDetail> insertedMiscOrderDetails,
            [Bind(Prefix = "updated")]IEnumerable<MiscOrderDetail> updatedMiscOrderDetails,
            [Bind(Prefix = "deleted")]IEnumerable<MiscOrderDetail> deletedMiscOrderDetails,
            string MiscOrderNo, string moveType)
        {
            try
            {
                MiscOrderMaster miscOrderMaster = base.genericMgr.FindById<MiscOrderMaster>(MiscOrderNo);
                IList<MiscOrderDetail> newMiscOrderDetailList = new List<MiscOrderDetail>();
                IList<MiscOrderDetail> updateMiscOrderDetailList = new List<MiscOrderDetail>();
                if (insertedMiscOrderDetails != null)
                {
                    foreach (var miscOrderDetail in insertedMiscOrderDetails)
                    {
                        if (CheckMiscOrderDetail(miscOrderDetail, moveType, (int)com.Sconit.CodeMaster.MiscOrderType.GR))
                        {
                            Item item = base.genericMgr.FindById<Item>(miscOrderDetail.Item);
                            miscOrderDetail.ItemDescription = item.Description;
                            miscOrderDetail.UnitCount = item.UnitCount;
                            miscOrderDetail.Uom = item.Uom;
                            miscOrderDetail.BaseUom = item.Uom;
                            miscOrderDetail.MiscOrderNo = MiscOrderNo;
                            if (miscOrderMaster.Consignment)
                            {
                                miscOrderDetail.ManufactureParty = miscOrderMaster.ManufactureParty;
                            }
                            newMiscOrderDetailList.Add(miscOrderDetail);
                        }
                    }
                }
                if (updatedMiscOrderDetails != null)
                {
                    foreach (var miscOrderDetail in updatedMiscOrderDetails)
                    {
                        if (CheckMiscOrderDetail(miscOrderDetail, moveType, (int)com.Sconit.CodeMaster.MiscOrderType.GR))
                        {
                            updateMiscOrderDetailList.Add(miscOrderDetail);
                        }
                    }
                }
                miscOrderMgr.BatchUpdateMiscOrderDetailsAndClose(MiscOrderNo, newMiscOrderDetailList, updateMiscOrderDetailList, (IList<MiscOrderDetail>)deletedMiscOrderDetails);
                SaveSuccessMessage(Resources.ORD.MiscOrderDetail.MiscOrderDetail_Saved);
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

        #endregion

        #region private method
        private SearchStatementModel PrepareSearchStatement(GridCommand command, InMiscOrderSearchModel searchModel)
        {
            string whereStatement = "where m.Type = " + (int)com.Sconit.CodeMaster.MiscOrderType.GR;
            IList<object> param = new List<object>();
            SecurityHelper.AddRegionPermissionStatement(ref whereStatement, "m", "Region");

            HqlStatementHelper.AddLikeStatement("MiscOrderNo", searchModel.MiscOrderNo, HqlStatementHelper.LikeMatchMode.Start, "m", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Status", searchModel.Status, "m", ref  whereStatement, param);
            HqlStatementHelper.AddEqStatement("Region", searchModel.Region, "m", ref  whereStatement, param);
            HqlStatementHelper.AddEqStatement("Location", searchModel.Location, "m", ref  whereStatement, param);
            HqlStatementHelper.AddEqStatement("MoveType", searchModel.MoveType, "m", ref  whereStatement, param);


            HqlStatementHelper.AddEqStatement("CostCenter", searchModel.CostCenter, "m", ref  whereStatement, param);
            HqlStatementHelper.AddEqStatement("CreateUserName", searchModel.CreateUserName, "m", ref  whereStatement, param);
            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("EffectiveDate", searchModel.StartDate, searchModel.EndDate, "m", ref whereStatement, param);
            }
            else if (searchModel.StartDate != null & searchModel.EndDate == null)
            {
                HqlStatementHelper.AddGeStatement("EffectiveDate", searchModel.StartDate, "m", ref whereStatement, param);
            }
            else if (searchModel.StartDate == null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddLeStatement("EffectiveDate", searchModel.EndDate, "m", ref whereStatement, param);
            }
            string sortingStatement = string.Empty;
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by CreateDate desc";
            }
            else
            {
                if (command.SortDescriptors[0].Member == "StatusDescription")
                {
                    command.SortDescriptors[0].Member = "Status";
                }
                sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            }
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();
            return searchStatementModel;
        }


        [SconitAuthorize(Permissions = "Url_Inventory_InMiscOrder_View")]
        public ActionResult ImportInMiscOrderDetail(IEnumerable<HttpPostedFileBase> attachments, string MiscOrderNo)
        {
            try
            {
                foreach (var file in attachments)
                {
                    miscOrderMgr.CreateMiscOrderDetailFromXls(file.InputStream, MiscOrderNo);
                    SaveSuccessMessage("导入成功！");
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
    }
}
