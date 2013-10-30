/// <summary>
/// Summary description for AddressController
/// </summary>

using System;

namespace com.Sconit.Web.Controllers.INV
{
    #region reference
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Security;
    using com.Sconit.Entity.INV;
    using com.Sconit.Entity.SYS;
    using com.Sconit.Service;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Models.SearchModels.INV;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;
    using com.Sconit.Entity.MD;
    using com.Sconit.Entity.Exception;
    using Telerik.Web.Mvc.UI;
    using System.Web;
    using System.IO;
    using com.Sconit.Utility;
    using com.Sconit.PrintModel.INV;
    using AutoMapper;
using com.Sconit.Utility.Report;
    using NHibernate;
    using NHibernate.Type;
    using NPOI.HSSF.UserModel;
    using NPOI.SS.UserModel;
    #endregion

    /// <summary>
    /// This controller response to control the StockTake.
    /// </summary>
    public class StockTakeController : WebAppBaseController
    {
        #region Properties
        public IStockTakeMgr stockTakeMgr { get; set; }
        public ILocationDetailMgr locationDetailMgr { get; set; }
        public IReportGen reportGen { get; set; }
        //public IImportMgr ImportMgr { get; set; }

        #endregion

        #region hql
        /// <summary>
        /// hql to get count of the StockTakeMaster
        /// </summary>
        private static string selectCountStatement = "select count(*) from StockTakeMaster as s";

        /// <summary>
        /// hql to get all of the StockTakeMaster
        /// </summary>
        private static string selectStatement = "select s from StockTakeMaster as s";

        /// <summary>
        /// hql to get count of the StockTakeResult
        /// </summary>
        private static string selectStockTakeResultCountStatement = "select count(*) from StockTakeResult as s";

        /// <summary>
        /// hql to get all of the StockTakeResult
        /// </summary>
        private static string selectStockTakeResultStatement = "select s from StockTakeResult s";

        #endregion

        #region StockTakeMaster


        /// <summary>
        /// Index action for StockTakeMaster controller
        /// </summary>
        /// <returns>Index view</returns>
        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_View")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// List action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">StockTakeMaster Search model</param>
        /// <returns>return the result view</returns>
        [GridAction]
        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_View")]
        public ActionResult StockTakeMasterList(GridCommand command, StockTakeMasterSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        /// <summary>
        ///  AjaxList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">StockTakeMaster Search Model</param>
        /// <returns>return the result action</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_View")]
        public ActionResult _AjaxStockTakeMasterList(GridCommand command, StockTakeMasterSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<StockTakeMaster>(searchStatementModel, command));
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <returns>New view</returns>
        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult New()
        {
            return View();
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <param name="address">StockTakeMaster Model</param>
        /// <returns>return the result view</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult New(StockTakeMaster stockTakeMaster)
        {
            try
            {
                if (stockTakeMaster.EffectiveDate == null)
                {
                    throw new BusinessException("生效日期不能为空。");
                }
                if (string.IsNullOrWhiteSpace(stockTakeMaster.Region ))
                {
                    throw new BusinessException("区域不能为空。");
                }
                stockTakeMaster.Type = com.Sconit.CodeMaster.StockTakeType.All;
                stockTakeMaster.Status = Sconit.CodeMaster.StockTakeStatus.Create;
                this.stockTakeMgr.CreateStockTakeMaster(stockTakeMaster);
                SaveSuccessMessage(Resources.INV.StockTake.StockTakeMaster_Added);
                return RedirectToAction("Edit/" + stockTakeMaster.StNo);
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex.Message);
            }
            return View(stockTakeMaster);
        }

        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                StockTakeMaster StockTakeMaster = base.genericMgr.FindById<StockTakeMaster>(id);
                return View("Edit", string.Empty, StockTakeMaster);
            }
        }

        /// <summary>
        /// StockTakeMasterEdit view
        /// </summary>
        /// <param name="id">StockTakeMaster id for edit</param>
        /// <returns>return the result view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult _StockTakeMasterEdit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                StockTakeMaster stockTakeMaster = base.genericMgr.FindById<StockTakeMaster>(id);
                stockTakeMaster.TypeDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.StockTakeType, ((int)stockTakeMaster.Type).ToString());
                stockTakeMaster.StockTakeStatusDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.StockTakeStatus, ((int)stockTakeMaster.Status).ToString());
                return PartialView(stockTakeMaster);
            }
        }

        #region Location
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult _StockTakeLocation(string stNo)
        {
            ViewBag.stNo = stNo;
            StockTakeMaster stockStakeMaster = base.genericMgr.FindById<StockTakeMaster>(stNo);
            ViewBag.Status = stockStakeMaster.Status;
            if (stockStakeMaster.Status == com.Sconit.CodeMaster.StockTakeStatus.Create)
            {
                ViewData["locations"] = base.genericMgr.FindAll<Location>("from Location where IsActive = 1 and Region = ?", stockStakeMaster.Region);
            }
            return PartialView();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult _Select(string stNo)
        {
            IList<StockTakeLocation> stockTakeLocationList = base.genericMgr.FindAll<StockTakeLocation>("from StockTakeLocation as s where s.StNo=?", stNo);
            return PartialView(new GridModel(stockTakeLocationList));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult _InsertAjaxEditing(string stNo, string Location)
        {
            if (ModelState.IsValid)
            {
                IList<StockTakeLocation> stockTakeLocationByLo = base.genericMgr.FindAll<StockTakeLocation>("from StockTakeLocation as s where s.Location=? and s.StNo=?", new object[] { Location, stNo });
                if (stockTakeLocationByLo.Count > 0)
                {
                    SaveSuccessMessage("库位已经存在");
                }
                else
                {
                    Location location = base.genericMgr.FindById<Location>(Location);
                    StockTakeLocation stLocation = new StockTakeLocation();
                    stLocation.Location = Location;
                    stLocation.LocationName = location.Name;
                    stLocation.StNo = stNo;
                    base.genericMgr.Create(stLocation);
                    SaveSuccessMessage(Resources.INV.StockTakeLocation.StockTakeLocation_Added);
                }
            }
            IList<StockTakeLocation> stockTakeLocationList = base.genericMgr.FindAll<StockTakeLocation>("from StockTakeLocation as s where s.StNo=?", stNo);
            return PartialView(new GridModel(stockTakeLocationList));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult _DeleteLocation(string stNo, string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<StockTakeLocation>(int.Parse(Id));
                SaveSuccessMessage(Resources.INV.StockTakeLocation.StockTakeLocation_Deleted);
            }
            IList<StockTakeLocation> stockTakeLocationList = base.genericMgr.FindAll<StockTakeLocation>("from StockTakeLocation as s where s.StNo=?", stNo);
            return PartialView(new GridModel(stockTakeLocationList));
        }


        [GridAction]
        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult _UpdateLocation(StockTakeLocation stLocation)
        {
            IList<StockTakeLocation> isEmpty = base.genericMgr.FindAll<StockTakeLocation>("from StockTakeLocation as s where s.Location=? and s.StNo=?", new object[] { stLocation.Location, stLocation.StNo });
            if (isEmpty.Count > 0)
            {
                SaveSuccessMessage("库位已经存在,重新选择");
            }
            else
            {
                Location location = base.genericMgr.FindById<Location>(stLocation.Location);
                stLocation.LocationName = location.Name;
                base.genericMgr.Update(stLocation);
                SaveSuccessMessage(Resources.INV.StockTakeLocation.StockTakeLocation_Updated);
            }

            IList<StockTakeLocation> stockTakeLocationList = base.genericMgr.FindAll<StockTakeLocation>("from StockTakeLocation as s where s.StNo=?", stLocation.StNo);
            return PartialView(new GridModel(stockTakeLocationList));
        }

        #endregion
        #region Item
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult _StockTakeItem(string stNo, string Status)
        {
            ViewBag.stNo = stNo;
            ViewBag.Status = Status;
            // ViewData["Item"] = base.genericMgr.FindAll<Item>();
            return PartialView();
        }


        [GridAction]
        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult _SelectItem(string stNo)
        {
            IList<StockTakeItem> stockTakeItem = base.genericMgr.FindAll<StockTakeItem>("from StockTakeItem as s where s.StNo=?", stNo);
            return PartialView(new GridModel(stockTakeItem));
        }

        public string _ItemDescription(string Item)
        {
            if (!string.IsNullOrEmpty(Item))
            {
                Item item = base.genericMgr.FindById<Item>(Item);
                if (item != null)
                {
                    return item.Description;
                }

            }
            return null;
        }

        public string _LocationDescription(string Location)
        {
            if (!string.IsNullOrEmpty(Location))
            {
                Location location = base.genericMgr.FindById<Location>(Location);
                if (location != null)
                {
                    return location.Name;
                }

            }
            return null;
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult _InsertItem(string stNo, string Item)
        {
            if (ModelState.IsValid)
            {
                IList<StockTakeItem> stockTakeByItem = base.genericMgr.FindAll<StockTakeItem>("from StockTakeItem as s where s.Item=? and s.StNo=?", new object[] { Item, stNo });
                if (stockTakeByItem.Count > 0)
                {
                    SaveSuccessMessage("零件已经存在");
                }
                else
                {
                    Item item = base.genericMgr.FindById<Item>(Item);
                    StockTakeItem stItem = new StockTakeItem();
                    stItem.Item = item.Code;
                    stItem.ItemDescription = item.Description;
                    stItem.StNo = stNo;
                    base.genericMgr.Create(stItem);
                    SaveSuccessMessage(Resources.INV.StockTakeItem.StockTakeItem_Added);
                }
            }
            IList<StockTakeItem> stockTakeItem = base.genericMgr.FindAll<StockTakeItem>("from StockTakeItem as s where s.StNo=?", stNo);
            return PartialView(new GridModel(stockTakeItem));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult _DeleteItem(string stNo, string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<StockTakeItem>(int.Parse(Id));
                SaveSuccessMessage(Resources.INV.StockTakeItem.StockTakeItem_Deleted);
            }
            IList<StockTakeItem> stockTakeItem = base.genericMgr.FindAll<StockTakeItem>("from StockTakeItem as s where s.StNo=?", stNo);
            return PartialView(new GridModel(stockTakeItem));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult _UpdateItem(StockTakeItem stItem)
        {
            IList<StockTakeItem> isEmpty = base.genericMgr.FindAll<StockTakeItem>("from StockTakeItem as s where s.Item=? and s.StNo=?", new object[] { stItem.Item, stItem.StNo });
            if (isEmpty.Count > 0)
            {
                SaveSuccessMessage("零件已经存在,重新选择");
            }
            else
            {
                Item item = base.genericMgr.FindById<Item>(stItem.Item);
                //  stItem = base.genericMgr.FindById<StockTakeItem>(stItem.Id);
                stItem.ItemDescription = item.CodeDescription;
                base.genericMgr.Update(stItem);
                SaveSuccessMessage(Resources.INV.StockTakeItem.StockTakeItem_Updated);
            }

            IList<StockTakeItem> stockTakeLocationList = base.genericMgr.FindAll<StockTakeItem>("from StockTakeItem as s where s.StNo=?", stItem.StNo);
            return PartialView(new GridModel(stockTakeLocationList));
        }

        #endregion
        #region edit

        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult btnDelete(string id)
        {
            try
            {
                StockTakeMaster StockTakeMaster = base.genericMgr.FindById<StockTakeMaster>(id);
                stockTakeMgr.DeleteStockTakeMaster(StockTakeMaster);
                SaveSuccessMessage(Resources.INV.StockTake.StockTakeMaster_Deleted);
                return RedirectToAction("StockTakeMasterList");
            }
            catch (BusinessException ex)
            {

                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
                return RedirectToAction("Edit/" + id);
            }

        }


        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult btnSubmit(string id)
        {

            try
            {
                //StockTakeMaster StockTakeMaster = base.genericMgr.FindById<StockTakeMaster>(id);
                stockTakeMgr.ReleaseAndStartStockTakeMaster(id);
                SaveSuccessMessage(Resources.INV.StockTake.StockTakeMaster_Submit);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            return RedirectToAction("Edit/" + id);
        }


        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult btnStart(string id)
        {

            try
            {
                StockTakeMaster StockTakeMaster = base.genericMgr.FindById<StockTakeMaster>(id);
                stockTakeMgr.StartStockTakeMaster(StockTakeMaster);
                SaveSuccessMessage(Resources.INV.StockTake.StockTakeMaster_Start);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            return RedirectToAction("Edit/" + id);
        }

        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult btnCancel(string id)
        {

            try
            {
                StockTakeMaster StockTakeMaster = base.genericMgr.FindById<StockTakeMaster>(id);
                stockTakeMgr.CancelStockTakeMaster(StockTakeMaster);
                SaveSuccessMessage(Resources.INV.StockTake.StockTakeMaster_Cancel);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            return RedirectToAction("Edit/" + id);
        }

        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult btnComplete(StockTakeMaster stockTakeMaster)
        {

            // StockTakeMaster stockTakeMaster = (StockTakeMaster)id;
            try
            {
                StockTakeMaster StockTakeMaster = base.genericMgr.FindById<StockTakeMaster>(stockTakeMaster.StNo);
                stockTakeMgr.CompleteStockTakeMaster(StockTakeMaster, stockTakeMaster.BaseInventoryDate);
                SaveSuccessMessage(Resources.INV.StockTake.StockTakeMaster_Complete);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            return RedirectToAction("Edit/" + stockTakeMaster.StNo);
        }

        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult btnClose(string id)
        {

            try
            {
                StockTakeMaster StockTakeMaster = base.genericMgr.FindById<StockTakeMaster>(id);
                stockTakeMgr.ManualCloseStockTakeMaster(StockTakeMaster);
                SaveSuccessMessage(Resources.INV.StockTake.StockTakeMaster_Close);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            return RedirectToAction("Edit/" + id);
        }
        #endregion
        #endregion

        #region  StockTakeDetail

        /// <summary>
        /// StockTakeDetailSearch action
        /// </summary>
        /// <param name="id">StockTakeMaster Code</param>
        /// <returns>rediret view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult _StockTakeDetailSearch(string id, string Status, bool IsScanHu)
        {
            //StockTakeDetail stockTakeDetail = base.genericMgr.FindById<StockTakeDetail>(id);
            ViewBag.IsScanHu = IsScanHu;
            ViewBag.StNo = id;
            ViewBag.Status = Status;

            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult _StockTakeDetail(GridCommand command, StockTakeDetailSearchModel searchModel, string Status, bool IsScanHu)
        {
            ViewBag.ItemCode = searchModel.ItemCode;
            ViewBag.StNo = searchModel.stNo;
            ViewBag.IsScanHu = IsScanHu;
            ViewBag.Status = Status;
            ViewData["Uom"] = base.genericMgr.FindAll<Uom>();
            TempData["StockTakeDetailSearchModel"] = searchModel;
            return PartialView();
        }


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_View")]
        public ActionResult _AjaxStockTakeDetail(GridCommand command, StockTakeDetailSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.StockTakeDetailPrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<StockTakeDetail>(searchStatementModel, command));
        }

        public ActionResult _WebBomDetail(string itemCode)
        {
            WebOrderDetail webOrderDetail = new WebOrderDetail();

            Item item = base.genericMgr.FindById<Item>(itemCode);
            if (item != null)
            {
                webOrderDetail.Item = item.Code;
                webOrderDetail.ItemDescription = item.Description;
                webOrderDetail.Uom = item.Uom;
            }
            return this.Json(webOrderDetail);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        public JsonResult _SaveStockTakeDetailBatchEditing([Bind(Prefix =
            "inserted")]IEnumerable<StockTakeDetail> insertedStockTakeDetails,
            [Bind(Prefix = "updated")]IEnumerable<StockTakeDetail> updatedStockTakeDetails,
            [Bind(Prefix = "deleted")]IEnumerable<StockTakeDetail> deletedStockTakeDetails, string id)
        {
            try
            {
                IList<StockTakeDetail> newStockTakeDetailList = new List<StockTakeDetail>();
                IList<StockTakeDetail> updateStockTakeDetailList = new List<StockTakeDetail>();
                if (insertedStockTakeDetails != null)
                {
                    foreach (var stockTakeDetail in insertedStockTakeDetails)
                    {
                        if (stockTakeDetail.Qty <= 0)
                        {
                            throw new BusinessException("数量必须大于0");

                        }
                        else if (stockTakeDetail.Item == null)
                        {
                            throw new BusinessException("物料为必填");

                        }
                        else if (stockTakeDetail.Location == null)
                        {
                            throw new BusinessException("库位为必填");
                        }
                        else
                        {
                            newStockTakeDetailList.Add(stockTakeDetail);
                        }
                    }
                }
                if (updatedStockTakeDetails != null)
                {
                    foreach (var stockTakeDetail in updatedStockTakeDetails)
                    {
                        if (stockTakeDetail.Qty <= 0)
                        {
                            throw new BusinessException("数量必须大于0");

                        }
                        else if (stockTakeDetail.Item == null)
                        {
                            throw new BusinessException("物料为必填");

                        }
                        else if (stockTakeDetail.Location == null)
                        {
                            throw new BusinessException("库位为必填");
                        }
                        else
                        {
                            updateStockTakeDetailList.Add(stockTakeDetail);
                        }
                    }
                }

                stockTakeMgr.BatchUpdateStockTakeDetails(id, newStockTakeDetailList, updateStockTakeDetailList, (IList<StockTakeDetail>)deletedStockTakeDetails);
                SaveSuccessMessage(Resources.INV.StockTake.StockTakeDetail_Saved);
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

        public JsonResult _DeleteStockTakeDetail(string Ids, string stNo)
        {
            string[] IdArray = Ids.Split(',');
            int[] ids = new int[IdArray.Length];
            IList<StockTakeDetail> StockTakeDetailDeleteList = new List<StockTakeDetail>();
            foreach (var item in IdArray)
            {
                StockTakeDetail stockTakeDetail = base.genericMgr.FindById<StockTakeDetail>(int.Parse(item));
                StockTakeDetailDeleteList.Add(stockTakeDetail);
            }

            try
            {
                stockTakeMgr.BatchUpdateStockTakeDetails(stNo, null, null, StockTakeDetailDeleteList);
                SaveSuccessMessage(Resources.INV.StockTake.StockTakeDetail_Saved);
                return Json(new { });
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

        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult ImportStcokTakeDetail(IEnumerable<HttpPostedFileBase> attachments, string stNo)
        {
            try
            {
                foreach (var file in attachments)
                {
                    stockTakeMgr.ImportStockTakeDetailFromXls(file.InputStream, stNo);
                }
                SaveErrorMessage("导入成功。");
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch(Exception e)
            {
                SaveErrorMessage(e.Message);
            }
            return Content("");
        }

        #endregion

        #region  StockTakeResult

        /// <summary>
        /// StockTakeDetailSearch action
        /// </summary>
        /// <param name="id">StockTakeMaster Code</param>
        /// <returns>rediret view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult _StockTakeResultSearch(string id, string Status, bool IsScanHu)
        {
            ViewBag.StNo = id;
            ViewBag.Status = Status;
            ViewBag.IsScanHu = IsScanHu;
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_New")]
        public ActionResult _StockTakeResult(GridCommand command, StockTakeResultSearchModel searchModel, bool IsScanHu)
        {
            ViewBag.Item = searchModel.Item;
            ViewBag.StNo = searchModel.stNo;
            ViewBag.IsScanHu = IsScanHu;
            ViewBag.Status = base.genericMgr.FindById<StockTakeMaster>(searchModel.stNo).Status.ToString();
            TempData["StockTakeResultSearchModel"] = searchModel;
            return PartialView();
        }


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_View")]
        public ActionResult _AjaxStockTakeResult(GridCommand command, StockTakeResultSearchModel searchModel)
        {
            //SearchStatementModel searchStatementModel = this.StockTakeResultPrepareSearchStatement(command, searchModel);
            //return PartialView(GetAjaxPageData<StockTakeResult>(searchStatementModel, command));
            IList<string> locationList = new List<string>();
            if (searchModel.Location != null && searchModel.Location != string.Empty)
            {
                locationList.Add(searchModel.Location);
            }
            IList<string> binList = new List<string>();
            if (searchModel.LocationBin != null && searchModel.LocationBin != string.Empty)
            {
                binList.Add(searchModel.LocationBin);
            }
            IList<string> itemList = new List<string>();
            if (searchModel.Item != null && searchModel.Item != string.Empty)
            {
                itemList.Add(searchModel.Item);
            }
            IList<StockTakeResultSummary> StockTakeResultSummaryList = stockTakeMgr.ListStockTakeResult(searchModel.stNo, searchModel.IsLoss, searchModel.IsProfit, searchModel.IsBreakEven, locationList, binList, itemList, null);
            return PartialView(new GridModel(StockTakeResultSummaryList));
        }



        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Inventory_StockTake_View")]
        public ActionResult _AjaxStockTakeResultIsScanHu(GridCommand command, StockTakeResultSearchModel searchModel)
        {
            IList<string> locationList = new List<string>();
            if (searchModel.Location != null && searchModel.Location != string.Empty)
            {
                locationList.Add(searchModel.Location);
            }
            IList<string> binList = new List<string>();
            if (searchModel.LocationBin != null && searchModel.LocationBin != string.Empty)
            {
                binList.Add(searchModel.LocationBin);
            }
            IList<string> itemList = new List<string>();
            if (searchModel.Item != null && searchModel.Item != string.Empty)
            {
                itemList.Add(searchModel.Item);
            }
            IList<StockTakeResultSummary> StockTakeResultSummaryList = stockTakeMgr.ListStockTakeResult(searchModel.stNo, searchModel.IsLoss, searchModel.IsProfit, searchModel.IsBreakEven, locationList, binList, itemList, null);
            return PartialView(new GridModel(StockTakeResultSummaryList));
        }

        public ActionResult _StockTakeResultDetail(string Location, string LocationBin, string Item, string StNo, bool listShortage, bool listProfit, bool listMatch)
        {
            if (StNo == null || StNo == string.Empty)
            {
                return null;
            }

            IList<string> locationList = new List<string>();
            if (Location != null && Location != string.Empty)
            {
                locationList.Add(Location);
            }
            IList<string> binList = new List<string>();
            if (LocationBin != null && LocationBin != string.Empty)
            {
                binList.Add(LocationBin);
            }
            IList<string> itemList = new List<string>();
            if (Item != null && Item != string.Empty)
            {
                itemList.Add(Item);
            }
            ViewBag.Status = base.genericMgr.FindById<StockTakeMaster>(StNo).Status.ToString();
            IList<StockTakeResult> stockTakeResultList = stockTakeMgr.ListStockTakeResultDetail(StNo, listShortage, listProfit, listMatch, locationList, binList, itemList, null);
            return PartialView(stockTakeResultList);
        }


        public JsonResult _AllAdjust(string StNo)
        {
            try
            {
                stockTakeMgr.AdjustStockTakeResult(StNo, null);
                SaveSuccessMessage(Resources.INV.StockTake.StockTakeResult_Adjustment, StNo);
                return Json( new { });
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
        public JsonResult _btnAdjust(string Ids, string StNo)
        {
            //ViewBag.CheckedOrders = checkedOrders;
            string[] IdArray = Ids.Split(',');
            int[] ids = new int[IdArray.Length];
            int i = 0;
            foreach (var item in IdArray)
            {
                ids[i++] = int.Parse(item);
            }

            try
            {
                stockTakeMgr.AdjustStockTakeResult(ids, null);
                SaveSuccessMessage(Resources.INV.StockTake.StockTakeResult_Adjustment, StNo);
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

        #region Print
        public string Print(string StNo)
        {
            StockTakeMaster stockTakeMaster = base.genericMgr.FindById<StockTakeMaster>(StNo);
            PrintStockTakeMaster printStockTakeMaster = Mapper.Map<StockTakeMaster, PrintStockTakeMaster>(stockTakeMaster);
            IList<object> data = new List<object>();
            data.Add(printStockTakeMaster);
            string reportFileUrl = reportGen.WriteToFile("StockTaking.xls", data);
            //reportGen.WriteToClient(orderMaster.OrderTemplate, data, orderMaster.OrderTemplate);

            return reportFileUrl;
            //reportGen.WriteToFile(orderMaster.OrderTemplate, data);
        }
        #endregion

        #region Prepare

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">StockTakeMaster Search Model</param>
        /// <returns>return StockTakeMaster search model</returns>
        private SearchStatementModel PrepareSearchStatement(GridCommand command, StockTakeMasterSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();
            SecurityHelper.AddRegionPermissionStatement(ref whereStatement, "s", "Region");
            HqlStatementHelper.AddLikeStatement("StNo", searchModel.StNo, HqlStatementHelper.LikeMatchMode.End, "s", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Type", searchModel.Type, "s", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Status", searchModel.Status, "s", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Region", searchModel.Region, "s", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("CostCenter", searchModel.CostCenter, HqlStatementHelper.LikeMatchMode.End, "s", ref whereStatement, param);

            if (searchModel.StockTakeStartDate != null & searchModel.StockTakeEndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("EffectiveDate", searchModel.StockTakeStartDate, searchModel.StockTakeEndDate, "s", ref whereStatement, param);
            }
            else if (searchModel.StockTakeStartDate != null & searchModel.StockTakeEndDate == null)
            {
                HqlStatementHelper.AddGeStatement("EffectiveDate", searchModel.StockTakeStartDate, "s", ref whereStatement, param);
            }
            else if (searchModel.StockTakeStartDate == null & searchModel.StockTakeEndDate != null)
            {
                HqlStatementHelper.AddLeStatement("EffectiveDate", searchModel.StockTakeEndDate, "s", ref whereStatement, param);
            }
            if (searchModel.StockTakeStartDate != null & searchModel.StockTakeEndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.CreateDateFrom, searchModel.CreateDateTo, "s", ref whereStatement, param);
            }
            else if (searchModel.StockTakeStartDate != null & searchModel.StockTakeEndDate == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.CreateDateFrom, "s", ref whereStatement, param);
            }
            else if (searchModel.StockTakeStartDate == null & searchModel.StockTakeEndDate != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.CreateDateTo, "s", ref whereStatement, param);
            }
            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "StockTakeStatusDescription")
                {
                    command.SortDescriptors[0].Member = "Status";
                }
                else if (command.SortDescriptors[0].Member == "TypeDescription")
                {
                    command.SortDescriptors[0].Member = "Type";
                }
            }

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by s.CreateDate desc";
            }
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">StockTakeResult Search Model</param>
        /// <returns>return StockTakeResult search model</returns>
        private SearchStatementModel StockTakeDetailPrepareSearchStatement(GridCommand command, StockTakeDetailSearchModel searchModel)
        {
            string whereStatement = " where 1=1";
            IList<object> param = new List<object>();
            HqlStatementHelper.AddEqStatement("StNo", searchModel.stNo, "s", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.ItemCode, "s", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Bin", searchModel.LocationBin, "s", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Location", searchModel.Location, "s", ref whereStatement, param);

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = "select count(*) from StockTakeDetail as s";
            searchStatementModel.SelectStatement = "select s from StockTakeDetail as s";
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">StockTakeResult Search Model</param>
        /// <returns>return StockTakeResult search model</returns>
        private SearchStatementModel StockTakeResultPrepareSearchStatement(GridCommand command, StockTakeResultSearchModel searchModel)
        {
            string whereStatement = " where 1=1";
            IList<object> param = new List<object>();
            HqlStatementHelper.AddEqStatement("StNo", searchModel.stNo, "s", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "s", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Location", searchModel.Location, "s", ref whereStatement, param);
            if (searchModel.IsAdjust == true)
            {
                searchModel.IsAdjust = false;
                HqlStatementHelper.AddEqStatement("IsAdjust", searchModel.IsAdjust, "s", ref whereStatement, param);
            }

            if (searchModel.IsLoss == true)
            {
                whereStatement += " and (DiffQty<0";
                if (searchModel.IsProfit == true)
                {
                    whereStatement += " or DiffQty>0";
                }
                if (searchModel.IsBreakEven == true)
                {
                    whereStatement += " or DiffQty=0";
                }
                whereStatement += ")";
            }
            else if (searchModel.IsProfit == true)
            {
                whereStatement += " and (DiffQty>0";
                if (searchModel.IsBreakEven == true)
                {
                    whereStatement += " or DiffQty=0";
                }
                whereStatement += ")";
            }
            else if (searchModel.IsBreakEven == true || (searchModel.IsBreakEven == false & searchModel.IsLoss == false & searchModel.IsProfit == false))
            {
                whereStatement += " and DiffQty=0";
            }

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectStockTakeResultCountStatement;
            searchStatementModel.SelectStatement = selectStockTakeResultStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        private string getSearchHql(StockTakeResultSearchModel searchModel)
        {
            string hql = "from StockTakeResult as s where s.StNo='" + searchModel.stNo + "'";
            if (searchModel.Item != null)
            {
                hql += "and s.Item='" + searchModel.Item + "'";
            }
            if (searchModel.Location != null)
            {
                hql += "and s.Location='" + searchModel.Location + "'";
            }
            if (searchModel.LocationBin != null)
            {
                hql += "and s.Bin='" + searchModel.LocationBin + "'";
            }
            if (searchModel.Item != null)
            {
                hql += "and s.Item='" + searchModel.Item + "'";
            }

            if (searchModel.IsAdjust == true)
            {
                searchModel.IsAdjust = false;
                hql += "and s.IsAdjust='" + searchModel.IsAdjust + "'";
            }

            if (searchModel.IsLoss == true)
            {
                hql += " and (DiffQty<0";
                if (searchModel.IsProfit == true)
                {
                    hql += " or DiffQty>0";
                }
                if (searchModel.IsBreakEven == true)
                {
                    hql += " or DiffQty=0";
                }
                hql += ")";
            }
            else if (searchModel.IsProfit == true)
            {
                hql += " and (DiffQty>0";
                if (searchModel.IsBreakEven == true)
                {
                    hql += " or DiffQty=0";
                }
                hql += ")";
            }
            else if (searchModel.IsBreakEven == true || (searchModel.IsBreakEven == false & searchModel.IsLoss == false & searchModel.IsProfit == false))
            {
                hql += " and DiffQty=0";
            }

            hql += "";

            return hql;
        }
        #endregion
    }
}
