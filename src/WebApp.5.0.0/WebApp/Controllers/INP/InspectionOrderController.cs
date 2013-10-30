namespace com.Sconit.Web.Controllers.INP
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Security;
    using com.Sconit.Entity.INP;
    using com.Sconit.Service;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Models.SearchModels.INP;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;
    using com.Sconit.Entity.MD;
    using com.Sconit.Entity.INV;
    using com.Sconit.Entity.SYS;
    using System;
    using AutoMapper;
    using com.Sconit.Service.Impl;
    using com.Sconit.Entity.Exception;
    using com.Sconit.Utility;
    using com.Sconit.Entity.CUST;
    using System.Text;
    using com.Sconit.Entity;
    using com.Sconit.PrintModel.INP;
    using com.Sconit.Utility.Report;
    using com.Sconit.Entity.SCM;
    using com.Sconit.Entity.ORD;
    using com.Sconit.Web.Models.SearchModels.INV;

    public class InspectionOrderController : WebAppBaseController
    {
        //
        // GET: /InspectionOrder/
        public IInspectMgr inspectMgr { get; set; }
        public IOrderMgr orderMgr { get; set; }
        public IReportGen reportGen { get; set; }
        public IFlowMgr flowMgr { get; set; }
        public IMiscOrderMgr miscOrderMgr { get; set; }


        private static string selectStatement = "select i from InspectMaster as i";

        private static string selectCountStatement = "select count(*) from InspectMaster as i";

        private static string selectInspectDetailCountStatement = "select count(*) from InspectDetail as id";

        private static string selectInspectDetailSearchModelStatement = "select id from InspectDetail as id ";

        private static string selectInspectDetailStatement = "select i from InspectDetail as i where i.InspectNo=?";

        private static string selectJudgeInspectDetailStatement = "select i from InspectDetail as i where i.InspectNo=? and i.IsJudge=False";


        private static string InspectResultSelectStatement = "select i from InspectResult as i";

        private static string InspectResultSelectCountStatement = "select count(*) from InspectResult as i";

        #region view
        [SconitAuthorize(Permissions = "Url_InspectionOrder_View")]
        public ActionResult Index()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_InspectionOrder_View")]
        [GridAction]
        public ActionResult List(GridCommand command, InspectMasterSearchModel searchModel)
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

        [SconitAuthorize(Permissions = "Url_InspectionOrder_View")]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxList(GridCommand command, InspectMasterSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<InspectMaster>()));
            }
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel, string.Empty);
            return PartialView(GetAjaxPageData<InspectMaster>(searchStatementModel, command));
        }

        [SconitAuthorize(Permissions = "Url_InspectionOrder_View")]
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                return View("Edit", string.Empty, id);
            }
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_InspectionOrder_View")]
        public ActionResult _Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                InspectMaster inspectMaster = base.genericMgr.FindById<InspectMaster>(id);

                return PartialView(inspectMaster);
            }
        }

        [SconitAuthorize(Permissions = "Url_InspectionOrder_View")]
        public ActionResult InspectionOrderDetailEdit(string inspectNo)
        {

            ViewBag.inspectNo = inspectNo;
            return PartialView();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_InspectionOrder_View")]
        public ActionResult _InspectionOrderDetailList(string inspectNo)
        {
            IList<InspectDetail> inspectDetailList = base.genericMgr.FindAll<InspectDetail>(selectInspectDetailStatement, inspectNo);

            IList<FailCode> failCodeList = base.genericMgr.FindAll<FailCode>();

            foreach (InspectDetail inspectDetail in inspectDetailList)
            {
                foreach (FailCode failCode in failCodeList)
                {
                    if (inspectDetail.FailCode == failCode.Code)
                    {
                        inspectDetail.FailCode = failCode.CodeDescription;
                    }
                }
            }

            return View(new GridModel(inspectDetailList));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_InspectionOrder_View,Url_InspectionOrder_Judge")]
        public ActionResult InspectionResult(GridCommand command, InspectResultSearchModel searchModel)
        {
            ViewBag.Item = searchModel.Item;
            TempData["InspectResultSearchModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_InspectionOrder_View,Url_InspectionOrder_Judge")]
        public ActionResult _AjaxInspectionResultList(GridCommand command, InspectResultSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.InspectResultPrepareSearchStatement(command, searchModel);
            GridModel<InspectResult> list = GetAjaxPageData<InspectResult>(searchStatementModel, command);
            IList<FailCode> failCodeList = base.genericMgr.FindAll<FailCode>();

            foreach (InspectResult inspectResult in list.Data)
            {
                foreach (FailCode failCode in failCodeList)
                {
                    if (inspectResult.FailCode == failCode.Code)
                    {
                        inspectResult.FailCodeDescription = failCode.CodeDescription;
                    }
                }
            }
            return PartialView(list);
        }
        #endregion

        #region new
        [SconitAuthorize(Permissions = "Url_InspectionOrder_New")]
        public ActionResult New()
        {
            if (TempData["InspectDetailList"] != null)
            {
                TempData["InspectDetailList"] = null;
            }
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [SconitAuthorize(Permissions = "Url_InspectionOrder_New")]
        public JsonResult New(string region, string locationFrom, [Bind(Prefix =
              "inserted")]IEnumerable<InspectDetail> insertedInspectDetails)
        {
            BusinessException businessException = new BusinessException();
            try
            {
                ViewBag.Region = region;
                ViewBag.LocationFrom = locationFrom;

                #region orderDetailList
                if (string.IsNullOrEmpty(locationFrom))
                {
                    throw new BusinessException(Resources.INP.InspectDetail.Errors_InspectDetail_LocationRequired);
                }

                IList<InspectDetail> inspectDetailList = new List<InspectDetail>();
                if (insertedInspectDetails != null && insertedInspectDetails.Count() > 0)
                {
                    int i = 1;
                    foreach (InspectDetail inspectDetail in insertedInspectDetails)
                    {
                        if (string.IsNullOrEmpty(inspectDetail.Item))
                        {
                            businessException.AddMessage("第" + (i++) + "行物料为必填。");
                            continue;
                        }
                        if (inspectDetail.InspectQty <= 0)
                        {
                            businessException.AddMessage("第" + (i++) + "报验数为大于0的数字。");
                            continue;
                        }
                        if (string.IsNullOrEmpty(inspectDetail.FailCode))
                        {
                            businessException.AddMessage("第" + (i++) + "失效代码为必填。");
                            continue;
                        }

                        Item item = base.genericMgr.FindById<Item>(inspectDetail.Item);
                        inspectDetail.ItemDescription = item.Description;
                        inspectDetail.UnitCount = item.UnitCount;
                        inspectDetail.ReferenceItemCode = item.ReferenceCode;
                        inspectDetail.Uom = item.Uom;
                        inspectDetail.LocationFrom = locationFrom;
                        inspectDetail.CurrentLocation = locationFrom;
                        inspectDetail.BaseUom = item.Uom;
                        inspectDetail.UnitQty = 1;

                        inspectDetailList.Add(inspectDetail);

                    }
                }
                #endregion
                if (businessException.HasMessage)
                {
                    throw businessException;
                }
                if (inspectDetailList != null && inspectDetailList.Count == 0)
                {
                    throw new BusinessException(Resources.INP.InspectDetail.Errors_InspectDetail_Required);
                }

                InspectMaster inspectMaster = new InspectMaster();
                inspectMaster.Region = region;
                inspectMaster.InspectDetails = inspectDetailList;

                inspectMaster.Type = com.Sconit.CodeMaster.InspectType.Quantity;
                inspectMaster.IsATP = false;

                inspectMgr.CreateAndReject(inspectMaster);
                SaveSuccessMessage(Resources.INP.InspectMaster.InspectMaster_Added);
                return Json(new { InspectNo = inspectMaster.InspectNo });
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

        [SconitAuthorize(Permissions = "Url_InspectionOrder_New")]
        public ActionResult _InspectionOrderDetail()
        {
            //IList<Item> items = base.genericMgr.FindAll<Item>(selecItemStatement, true);
            //ViewData.Add("items", items);

            IList<Uom> uoms = base.genericMgr.FindAll<Uom>();
            ViewData.Add("uoms", uoms);
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_InspectionOrder_New")]
        public ActionResult _SelectBatchEditing()
        {
            IList<InspectDetail> inspectDetailList = new List<InspectDetail>();
            return View(new GridModel(inspectDetailList));
        }


        [SconitAuthorize(Permissions = "Url_InspectionOrder_New")]
        public ActionResult _WebInspectDetail(string itemCode)
        {
            if (!string.IsNullOrEmpty(itemCode))
            {
                IList<WebOrderDetail> webOrderDetailList = new List<WebOrderDetail>();
                WebOrderDetail webOrderDetail = new WebOrderDetail();

                Item item = base.genericMgr.FindById<Item>(itemCode);
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

        #region New ByScanHu
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_InspectionOrder_New")]
        public void ItemHuIdScan(string HuId)
        {
            IList<InspectDetail> InspectDetailList = (IList<InspectDetail>)TempData["InspectDetailList"];
            try
            {
                InspectDetailList = inspectMgr.AddInspectDetail(HuId, InspectDetailList);
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch(Exception ex)
            {
                SaveErrorMessage(ex);
            }
            TempData["InspectDetailList"] = InspectDetailList;
        }

        public ActionResult _InspectionOrderDetailScanHu()
        {
            return PartialView();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_InspectionOrder_New")]
        public ActionResult _SelectInspectionOrderDetail()
        {
            IList<InspectDetail> InspectionDetailList = new List<InspectDetail>();
            if (TempData["InspectDetailList"] != null)
            {
                InspectionDetailList = (IList<InspectDetail>)TempData["InspectDetailList"];
            }
            TempData["InspectDetailList"] = InspectionDetailList;
            return PartialView(new GridModel(InspectionDetailList));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_InspectionOrder_New")]
        public ActionResult _DeleteInspectionDetail(string HuId)
        {
            IList<InspectDetail> InspectDetailList = (IList<InspectDetail>)TempData["InspectDetailList"];
            IList<InspectDetail> q = InspectDetailList.Where(v => v.HuId != HuId).ToList();
            TempData["InspectDetailList"] = q;
            return PartialView(new GridModel(q));
        }

        public void _CleanInspectionDetail()
        {
            if (TempData["InspectDetailList"] != null)
            {
                TempData["InspectDetailList"] = null;
            }
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_InspectionOrder_New")]
        public JsonResult CreateInspectionDetail(string ItemStr, string HuIdStr, string LocationStr, string InspectQtyStr, string FailCodeStr, string NoteStr)
        {
            try
            {
                if (!string.IsNullOrEmpty(ItemStr))
                {
                    string[] itemArray = ItemStr.Split(',');
                    string[] huidArray = HuIdStr.Split(',');
                    string[] locationArray = LocationStr.Split(',');
                    string[] inspectqtyArray = InspectQtyStr.Split(',');
                    string[] falicodeArray = FailCodeStr.Split(',');
                    string[] noteArray = NoteStr.Split(',');
                    IList<InspectDetail> inspectDetailList = new List<InspectDetail>();
                    int i = 0;
                    foreach (string itemcode in itemArray)
                    {
                        InspectDetail inspectDetail = new InspectDetail();
                        Item item = base.genericMgr.FindById<Item>(itemcode);
                        inspectDetail.ItemDescription = item.Description;
                        inspectDetail.UnitCount = item.UnitCount;
                        inspectDetail.ReferenceItemCode = item.ReferenceCode;
                        inspectDetail.Uom = item.Uom;

                        inspectDetail.BaseUom = item.Uom;
                        inspectDetail.UnitQty = 1;
                        inspectDetail.LocationFrom = locationArray[i];
                        inspectDetail.CurrentLocation = locationArray[i];
                        inspectDetail.FailCode = falicodeArray[i];
                        inspectDetail.Note = noteArray[i];
                        inspectDetail.HuId = huidArray[i];
                        inspectDetail.InspectQty = Convert.ToDecimal(inspectqtyArray[i]);
                        i++;
                        inspectDetailList.Add(inspectDetail);
                    }

                    if (inspectDetailList != null && inspectDetailList.Count == 0)
                    {
                        throw new BusinessException(Resources.INP.InspectDetail.Errors_InspectDetail_Required);
                    }

                    InspectMaster inspectMaster = new InspectMaster();

                    inspectMaster.InspectDetails = inspectDetailList;

                    inspectMaster.Type = com.Sconit.CodeMaster.InspectType.Barcode;
                    inspectMaster.IsATP = false;

                    inspectMgr.CreateAndReject(inspectMaster);
                    SaveSuccessMessage(Resources.INP.InspectMaster.InspectMaster_Added);
                    this._CleanInspectionDetail();
                    return Json(new {InspectNo = inspectMaster.InspectNo});
                }
                else
                {
                    throw new BusinessException(Resources.INP.InspectDetail.Errors_InspectDetail_Required);
                }

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
        #endregion
        #endregion

        #region Judge

        [SconitAuthorize(Permissions = "Url_InspectionOrder_Judge")]
        public ActionResult JudgeIndex()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_InspectionOrder_Judge")]
        [GridAction]
        public ActionResult JudgeList(GridCommand command, InspectMasterSearchModel searchModel)
        {
            this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [SconitAuthorize(Permissions = "Url_InspectionOrder_Judge")]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxJudgeList(GridCommand command, InspectMasterSearchModel searchModel)
        {
            string whereStatement = " where i.Status in ( " + (int)com.Sconit.CodeMaster.InspectStatus.Submit + "," + (int)com.Sconit.CodeMaster.InspectStatus.InProcess + ")";
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel, whereStatement);
            return PartialView(GetAjaxPageData<InspectMaster>(searchStatementModel, command));
        }

        [SconitAuthorize(Permissions = "Url_InspectionOrder_Judge")]
        public ActionResult JudgeEdit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                return View("JudgeEdit", string.Empty, id);
            }
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_InspectionOrder_Judge")]
        public ActionResult _JudgeEdit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                InspectMaster inspectMaster = base.genericMgr.FindById<InspectMaster>(id);
                inspectMaster.InspectStatusDescription = this.systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.InspectStatus, ((int)inspectMaster.Status).ToString());
                return PartialView(inspectMaster);
            }
        }

        [SconitAuthorize(Permissions = "Url_InspectionOrder_Judge")]
        public ActionResult InspectionOrderDetailJudge(string inspectNo)
        {
            ViewBag.inspectNo = inspectNo;
            return PartialView();
        }

        [SconitAuthorize(Permissions = "Url_InspectionOrder_Judge")]
        public ActionResult InspectionOrderDetailJudgeWithHu(string inspectNo)
        {
            ViewBag.inspectNo = inspectNo;
            return PartialView();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [SconitAuthorize(Permissions = "Url_InspectionOrder_Judge")]
        public ActionResult _SelectJudgeBatchEditing(string inspectNo)
        {
            IList<InspectDetail> inspectDetailList = base.genericMgr.FindAll<InspectDetail>(selectJudgeInspectDetailStatement, inspectNo);
            IList<FailCode> failCodeList = base.genericMgr.FindAll<FailCode>();

            foreach (InspectDetail inspectDetail in inspectDetailList)
            {
                foreach (FailCode failCode in failCodeList)
                {
                    if (inspectDetail.FailCode == failCode.Code)
                    {
                        inspectDetail.FailCode = failCode.CodeDescription;
                    }
                }
            }
            return View(new GridModel(inspectDetailList));
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_InspectionOrder_Judge")]
        public JsonResult Judge(string inspectNo, [Bind(Prefix = "updated")]IEnumerable<InspectDetail> updatedInspectDetails)
        {
            try
            {
                IList<InspectDetail> inspectDetailList = new List<InspectDetail>();
                if (updatedInspectDetails != null && updatedInspectDetails.Count() > 0)
                {
                    inspectDetailList = updatedInspectDetails.Where(d => (d.CurrentQualifyQty > 0 || d.CurrentRejectQty > 0 || d.CurrentReturnQty > 0)).ToList();
                }
                if (inspectDetailList == null || inspectDetailList.Count == 0)
                {
                    throw new BusinessException("没有判定明细");
                }

                //var q = inspectDetailList.Where(d => (d.CurrentRejectQty > 0 || d.CurrentReturnQty > 0) && (d.Defect == null || d.FailCode == null)).ToList();
                //var q = inspectDetailList.Where(d => (d.CurrentRejectQty > 0 || d.CurrentReturnQty > 0) && d.JudgeFailCode == null).ToList();
                //if (q != null && q.Count > 0)
                //{
                //    throw new BusinessException("判定不合格的物料{0}没选择失效模式。", q[0].Item);
                //}
                inspectMgr.JudgeInspectDetail(inspectDetailList);
                SaveSuccessMessage("报验单{0}判定成功", inspectNo);
                return Json(new {InspectNo = inspectNo});
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

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_InspectionOrder_Judge")]
        public JsonResult JudgeQualify(string inspectNo, string idStr)
        {
            string failCodeStr = string.Empty;
            return BatchJudge(inspectNo, idStr, true, failCodeStr, string.Empty);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_InspectionOrder_Judge")]
        public JsonResult JudgeReject(string inspectNo, string idStr, string failCodeStr, string notes)
        {
            return BatchJudge(inspectNo, idStr, false, failCodeStr, notes);
        }
        #endregion

        #region transfer
        [GridAction]
        [SconitAuthorize(Permissions = "Url_InspectionOrder_Transfer")]
        public ActionResult Transfer()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_InspectionOrder_Transfer")]
        public ActionResult _TransferDetailList(string inspectNo)
        {
            IList<InspectDetail> inspectDetailList = base.genericMgr.FindAll<InspectDetail>(selectJudgeInspectDetailStatement, inspectNo);
            return PartialView(inspectDetailList);
        }

        [SconitAuthorize(Permissions = "Url_InspectionOrder_Transfer")]
        public JsonResult CreateInspectTransfer(string location, string idStr, string qtyStr)
        {
            try
            {
                #region 检查库位
                if (string.IsNullOrEmpty(location))
                {
                    throw new BusinessException("库位不能为空");
                }
                Location locTo = base.genericMgr.FindById<Location>(location);
                #endregion

                if (string.IsNullOrEmpty(idStr))
                {
                    throw new BusinessException("待验明细不能为空");
                }
                string[] idArr = idStr.Split(',');

                IList<InspectDetail> inspectDetailList = new List<InspectDetail>();
                foreach (string id in idArr)
                {
                    InspectDetail inspetDetail = base.genericMgr.FindById<InspectDetail>(Convert.ToInt32(id));
                    inspectDetailList.Add(inspetDetail);
                }

                orderMgr.CreateInspectTransfer(locTo, inspectDetailList);
                SaveSuccessMessage(Resources.INP.InspectMaster.InspectMaster_Transfered);
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

        #endregion

        #region detail
        [SconitAuthorize(Permissions = "Url_InspectionOrder_Detail")]
        public ActionResult DetailIndex()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_InspectionOrder_Detail")]
        public ActionResult DetailList(GridCommand command, InspectMasterSearchModel searchModel)
        {
            if (this.CheckSearchModelIsNull(searchModel))
            {
                TempData["_AjaxMessage"] = "";

                string hql = PrepareDetailSearchStatement(command, searchModel);
                IList<InspectDetail> InspectDetailList = new List<InspectDetail>();
                IList<object[]> objectList = base.genericMgr.FindAllWithNativeSql<object[]>(hql);
                InspectDetailList = (from tak in objectList
                                     select new InspectDetail
                                         {
                                             Id = (int)tak[0],
                                             InspectNo = (string)tak[1],
                                             Sequence = (int)tak[2],
                                             Item = (string)tak[3],
                                             ItemDescription = (string)tak[4],
                                             ReferenceItemCode = (string)tak[5],
                                             UnitCount = (decimal)tak[6],
                                             Uom = (string)tak[7],
                                             BaseUom = (string)tak[8],
                                             UnitQty = (decimal)tak[9],
                                             HuId = (string)tak[10],
                                             LotNo = (string)tak[11],
                                             LocationFrom = (string)tak[12],
                                             CurrentLocation = (string)tak[13],
                                             InspectQty = (decimal)tak[14],
                                             QualifyQty = (decimal)tak[15],
                                             RejectQty = (decimal)tak[16],
                                             IsJudge = (bool)tak[17],
                                             CreateUserId = (int)tak[18],
                                             CreateUserName = (string)tak[19],
                                             CreateDate = (DateTime)tak[20],
                                             LastModifyUserId = (int)tak[21],
                                             LastModifyUserName = (string)tak[22],
                                             LastModifyDate = (DateTime)tak[23],
                                             Version = (int)tak[24],

                                             ManufactureParty = (string)tak[25],
                                             WMSSeq = (string)tak[26],
                                             IpDetailSequence = (int)tak[27],
                                             ReceiptDetailSequence = (int)tak[28],
                                             Note = (string)tak[29],
                                             FailCode = (string)tak[30],
                                             InspectStatusDescription = systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.InspectStatus, int.Parse((tak[31]).ToString())),
                                             HandledQty = (decimal)tak[32],

                                         }).ToList();

                int value = Convert.ToInt32(base.systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.MaxRowSizeOnPage));
                if (InspectDetailList.Count > value)
                {
                    SaveWarningMessage(string.Format("数据超过{0}行", value));
                }
                return View(InspectDetailList.Take(value));
            }
            else
            {
                SaveWarningMessage(Resources.ErrorMessage.Errors_NoConditions);
                return View(new List<InspectDetail>());
            }

            //return View(list.Take(value));
        }

        #region 打印导出
        public void SaveToClient(string inspectNo)
        {
            InspectMaster inspectMaster = base.genericMgr.FindById<InspectMaster>(inspectNo);
            IList<InspectDetail> inspectDetails = base.genericMgr.FindAll<InspectDetail>("select id from InspectDetail as id where id.InspectNo=?", inspectNo);
            inspectMaster.InspectDetails = inspectDetails;
            PrintInspectMaster printInspectMaster = Mapper.Map<InspectMaster, PrintInspectMaster>(inspectMaster);
            IList<object> data = new List<object>();
            data.Add(printInspectMaster);
            data.Add(printInspectMaster.InspectDetails);
            //string reportFileUrl = reportGen.WriteToFile(orderMaster.OrderTemplate, data);
            reportGen.WriteToClient("InspectOrder.xls", data, "InspectOrder.xls");

            //return reportFileUrl;
            //reportGen.WriteToFile(orderMaster.OrderTemplate, data);
        }

        public string Print(string inspectNo)
        {
            InspectMaster inspectMaster = base.genericMgr.FindById<InspectMaster>(inspectNo);
            IList<InspectDetail> inspectDetails = base.genericMgr.FindAll<InspectDetail>("select id from InspectDetail as id where id.InspectNo=?", inspectNo);
            inspectMaster.InspectDetails = inspectDetails;
            PrintInspectMaster printInspectMaster = Mapper.Map<InspectMaster, PrintInspectMaster>(inspectMaster);
            IList<object> data = new List<object>();
            data.Add(printInspectMaster);
            data.Add(printInspectMaster.InspectDetails);
            string reportFileUrl = reportGen.WriteToFile("InspectOrder.xls", data);
            //reportGen.WriteToClient(orderMaster.OrderTemplate, data, orderMaster.OrderTemplate);

            return reportFileUrl;
            //reportGen.WriteToFile(orderMaster.OrderTemplate, data);
        }

        #endregion

        #endregion

        #region 让步使用
        #region Search
        [SconitAuthorize(Permissions = "InspectionOrder_ConcessionOrder_Search")]
        public ActionResult ConcessionOrderSearch()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "InspectionOrder_ConcessionOrder_Search")]
        [GridAction]
        public ActionResult ConcessionOrderList(GridCommand command, ConcessionMasterSearchModel searchModel)
        {
            this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [SconitAuthorize(Permissions = "InspectionOrder_ConcessionOrder_Search")]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxConcessionOrderList(GridCommand command, ConcessionMasterSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = ConcessionMasterPrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<ConcessionMaster>(searchStatementModel, command));
        }


        private SearchStatementModel ConcessionMasterPrepareSearchStatement(GridCommand command, ConcessionMasterSearchModel searchModel)
        {
            string whereStatement = string.Empty;
            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("ConcessionNo", searchModel.ConcessionNo, HqlStatementHelper.LikeMatchMode.Start, "c", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Status", searchModel.Status, "c", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("CreateUserName", searchModel.CreateUserName, "c", ref whereStatement, param);
            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.StartDate, searchModel.EndDate, "c", ref whereStatement, param);
            }
            else if (searchModel.StartDate != null & searchModel.EndDate == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.StartDate, "c", ref whereStatement, param);
            }
            else if (searchModel.StartDate == null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.EndDate, "c", ref whereStatement, param);
            }
            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "ConcessionStatusDescription")
                {
                    command.SortDescriptors[0].Member = "Status";
                }
            }
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by c.CreateDate desc";
            }

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        #endregion

        #region new
        [GridAction]
        [SconitAuthorize(Permissions = "Url_ConcessionOrder_New")]
        public ActionResult ConcessionOrderNew()
        {
            return View();
        }

        public ActionResult InspectionOrderDetailList(GridCommand command, InspectResultSearchModel searchModel)
        {
            this.ProcessSearchModel(command, searchModel);
            ViewBag.InspectNo = searchModel.InspectNo;
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_RejectOrder_New")]
        public ActionResult _AjaxInspectResultNewList(GridCommand command, InspectResultSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = NewPrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<InspectResult>(searchStatementModel, command));
        }

        [SconitAuthorize(Permissions = "Url_RejectOrder_New")]
        public JsonResult CreateConcession(string handleResult, string idStr, string qtyStr)
        {
            try
            {
                if (string.IsNullOrEmpty(handleResult))
                {
                    throw new BusinessException("处理方式不能为空");
                }

                #region inspectResultList
                IList<InspectResult> inspectResultList = new List<InspectResult>();

                if (!string.IsNullOrEmpty(idStr))
                {
                    string[] idArray = idStr.Split(',');
                    string[] qtyArray = qtyStr.Split(',');
                    for (int i = 0; i < idArray.Count(); i++)
                    {
                        if (Convert.ToDecimal(qtyArray[i]) > 0)
                        {
                            InspectResult nonZeroInspectResult = base.genericMgr.FindById<InspectResult>(Convert.ToInt32(idArray[i]));
                            nonZeroInspectResult.CurrentHandleQty = Convert.ToDecimal(qtyArray[i]);
                            inspectResultList.Add(nonZeroInspectResult);
                        }
                    }
                }
                #endregion

                if (inspectResultList.Count == 0)
                {
                    throw new BusinessException("明细不能为空");
                }
                RejectMaster rejectMaster = inspectMgr.CreateRejectMaster((com.Sconit.CodeMaster.HandleResult)(Convert.ToInt32(handleResult)), inspectResultList);
                inspectMgr.ReleaseRejectMaster(rejectMaster.RejectNo);
                //  RejectMaster rej = inspectMgr.CreateConcession(handleResult, inspectResultList);
                SaveSuccessMessage("不合格品处理单创建成功");
                return Json(new {RejectNo = rejectMaster.RejectNo});
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


        private SearchStatementModel NewPrepareSearchStatement(GridCommand command, InspectResultSearchModel searchModel)
        {
            string whereStatement = " where i.JudgeQty > i.HandleQty and i.JudgeResult= " + (int)com.Sconit.CodeMaster.JudgeResult.Rejected;

            if (searchModel.InspectType != null && searchModel.InspectType.Value == (int)com.Sconit.CodeMaster.InspectType.Barcode)
            {
                whereStatement += " and i.HuId is not null";
            }
            else
            {
                whereStatement += " and i.HuId is null";
            }
            //return InspectResultPrepareSearchStatement(command, searchModel, whereStatement);
            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("InspectNo", searchModel.InspectNo, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "i", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("IpNo", searchModel.IpNo, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("ReceiptNo", searchModel.ReceiptNo, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("WMSNo", searchModel.WMSNo, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("CreateUserName", searchModel.CreateUserName, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("CurrentLocation", searchModel.CurrentLocation, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("ManufactureParty", searchModel.ManufactureParty, "i", ref whereStatement, param);

            if (searchModel.StartDate != null && searchModel.EndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.StartDate, searchModel.EndDate, "i", ref whereStatement, param);

            }
            else if (searchModel.StartDate != null && searchModel.EndDate == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.StartDate, "i", ref whereStatement, param);
            }
            else if (searchModel.StartDate == null && searchModel.EndDate != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.EndDate, "i", ref whereStatement, param);
            }

            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "DefectDescription")
                {
                    command.SortDescriptors[0].Member = "Defect";
                }
                if (command.SortDescriptors[0].Member == "CurrentHandleQty")
                {
                    command.SortDescriptors[0].Member = "HandleQty";
                }
            }
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = "select count(*) from InspectResult as i";
            searchStatementModel.SelectStatement = "select i from InspectResult as i";
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;

        }

        private SearchStatementModel InspectResultPrepareSearchStatement(GridCommand command, InspectResultSearchModel searchModel, string whereStatement)
        {
            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("InspectNo", searchModel.InspectNo, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "i", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("IpNo", searchModel.IpNo, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("ReceiptNo", searchModel.ReceiptNo, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("WMSNo", searchModel.WMSNo, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("CreateUserName", searchModel.CreateUserName, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("CurrentLocation", searchModel.CurrentLocation, "i", ref whereStatement, param);

            if (searchModel.StartDate != null && searchModel.EndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.StartDate, searchModel.EndDate, "i", ref whereStatement, param);

            }
            else if (searchModel.StartDate != null && searchModel.EndDate == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.StartDate, "i", ref whereStatement, param);
            }
            else if (searchModel.StartDate == null && searchModel.EndDate != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.EndDate, "i", ref whereStatement, param);
            }

            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "DefectDescription")
                {
                    command.SortDescriptors[0].Member = "Defect";
                }
            }
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = "select count(*) from InspectDetail as id";
            searchStatementModel.SelectStatement = "select id from InspectDetail as id";
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        #endregion


        #region New
        public ActionResult _ConcessionOrderDetailList(bool isTranster)
        {
            ViewBag.Transfer = isTranster;
            IList<Uom> uoms = base.genericMgr.FindAll<Uom>();
            ViewData.Add("uoms", uoms);
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _SelectConcessionDetailList()
        {
            return PartialView(new GridModel(new List<ConcessionDetail>()));
        }

        [SconitAuthorize(Permissions = "Url_TransferOrder_View")]
        public ActionResult _WebInserintDetail(string itemCode)
        {
            if (!string.IsNullOrEmpty(itemCode))
            {
                Item item = base.genericMgr.FindById<Item>(itemCode);
                return this.Json(item);
            }
            return null;
        }

        public JsonResult CreateConssionOrder(string manufactureParty, string PartyTo, string Consignment, [Bind(Prefix =
                 "inserted")]IEnumerable<ConcessionDetail> insertedOrderDetails)
        {
            try
            {
                if (string.IsNullOrEmpty(PartyTo))
                {
                    throw new BusinessException("区域不能为空！");
                }
                if (int.Parse(Consignment) == 1)
                {
                    if (string.IsNullOrEmpty(manufactureParty))
                    {
                        throw new BusinessException("寄售状态供应商为必填！");
                    }
                }
                else//现在后台是按明细有供应商就是寄售 所以选择非寄售的话，把供应商清空。
                {
                    manufactureParty = null;
                }
                if (insertedOrderDetails == null || insertedOrderDetails.Count() == 0)
                {
                    throw new BusinessException("让步使用单明细不能为空！");
                }
                IList<ConcessionDetail> insertedOrderDetailList = insertedOrderDetails as List<ConcessionDetail>;
                int i = 0;
                foreach (ConcessionDetail concessionDetail in insertedOrderDetailList)
                {
                    i++;
                    if (concessionDetail.Qty == 0)
                    {
                        throw new BusinessException("第" + i + "行明细数量不能为空！");
                    }
                    if (string.IsNullOrEmpty(concessionDetail.Item))
                    {
                        throw new BusinessException("第" + i + "行明细物料不能为空！");
                    }
                    else
                    {
                        Item item = base.genericMgr.FindById<Item>(concessionDetail.Item);
                        concessionDetail.ItemDescription = item.Description;
                        concessionDetail.ReferenceItemCode = item.ReferenceCode;
                        concessionDetail.Uom = (concessionDetail.Uom == null || concessionDetail.Uom == string.Empty) ? item.Uom : concessionDetail.Uom;
                        concessionDetail.BaseUom = (concessionDetail.BaseUom == null || concessionDetail.BaseUom == string.Empty) ? item.Uom : concessionDetail.BaseUom;
                        concessionDetail.UnitCount = item.UnitCount;
                        concessionDetail.UnitQty = 1;

                    }
                    if (string.IsNullOrEmpty(concessionDetail.LocationTo))
                    {
                        throw new BusinessException("第" + i + "行明细库位不能为空！");
                    }
                    concessionDetail.ManufactureParty = manufactureParty != string.Empty ? manufactureParty : null;
                    concessionDetail.LocationFrom = concessionDetail.LocationTo;
                }
                ConcessionMaster concessionMaster = new ConcessionMaster();
                concessionMaster.Region = PartyTo;
                concessionMaster.Status = com.Sconit.CodeMaster.ConcessionStatus.Create;
                concessionMaster.ManufactureParty = manufactureParty != string.Empty ? manufactureParty : null;
                concessionMaster.ConcessionDetails = insertedOrderDetailList;
                // base.genericMgr.Create(concessionMaster);


                //IList<ConcessionDetail> updatedOrderDetailList = updatedOrderDetails as List<ConcessionDetail>;
                concessionMaster = inspectMgr.CreateConssionOrder(concessionMaster);
                SaveSuccessMessage("让步使用单{0}创建成功。",concessionMaster.ConcessionNo);
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

        #region 移库
        [GridAction]
        [SconitAuthorize(Permissions = "Url_InspectionOrder_Transfer")]
        public ActionResult InspectionTransfer()
        {
            return View();
        }

        //[SconitAuthorize(Permissions = "Url_InspectionOrder_Transfer")]
        //public JsonResult CreateRejectTransfer(string location, string idStr, string qtyStr, string handleResult)
        //{
        //    try
        //    {
        //        #region 检查库位
        //        if (string.IsNullOrEmpty(location))
        //        {
        //            throw new BusinessException("库位不能为空");
        //        }
        //        Location locTo = base.genericMgr.FindById<Location>(location);
        //        #endregion


        //        if (string.IsNullOrEmpty(handleResult))
        //        {
        //            throw new BusinessException("处理方式不能为空");
        //        }

        //        #region inspectResultList
        //        IList<InspectResult> inspectResultList = new List<InspectResult>();

        //        if (!string.IsNullOrEmpty(idStr))
        //        {
        //            string[] idArray = idStr.Split(',');
        //            string[] qtyArray = qtyStr.Split(',');
        //            for (int i = 0; i < idArray.Count(); i++)
        //            {
        //                if (Convert.ToDecimal(qtyArray[i]) > 0)
        //                {
        //                    InspectResult nonZeroInspectResult = base.genericMgr.FindById<InspectResult>(Convert.ToInt32(idArray[i]));
        //                    nonZeroInspectResult.CurrentHandleQty = Convert.ToDecimal(qtyArray[i]);
        //                    inspectResultList.Add(nonZeroInspectResult);
        //                }
        //            }
        //        }
        //        #endregion

        //        if (inspectResultList.Count == 0)
        //        {
        //            throw new BusinessException("明细不能为空");
        //        }
        //       // RejectMaster rej = inspectMgr.CreateRejectTransfer(locTo, handleResult, inspectResultList);
        //        RejectMaster rejectMaster = inspectMgr.CreateRejectMaster((com.Sconit.CodeMaster.HandleResult)(Convert.ToInt32(handleResult)), inspectResultList);
        //        inspectMgr.ReleaseRejectMaster(rejectMaster.RejectNo);


        //        IList<RejectDetail> rejectDetailList = base.genericMgr.FindAll<RejectDetail>("select r from RejectDetail as r where r.RejectNo=? ", rejectMaster.RejectNo);

        //        orderMgr.CreateRejectTransfer(locTo, rejectDetailList);
        //        object obj = new { SuccessMessage = string.Format(Resources.INP.RejectMaster.RejectMaster_Transfered) };
        //        return Json(obj);
        //    }
        //    catch (BusinessException ex)
        //    {
        //        Response.TrySkipIisCustomErrors = true;
        //        Response.StatusCode = 500;
        //        Response.Write(ex.GetMessages()[0].GetMessageString());
        //        return Json(null);
        //    }
        //}

        public JsonResult CreateRejectTransfer(string manufactureParty, string PartyTo, string PartyFrom, string Consignment, string StartTime, [Bind(Prefix =
                "inserted")]IEnumerable<OrderDetail> insertedOrderDetails)
        {
            try
            {
                if (string.IsNullOrEmpty(PartyFrom))
                {
                    throw new BusinessException("来源区域不能为空！");
                }
                if (int.Parse(Consignment) == 1)
                {
                    if (string.IsNullOrEmpty(manufactureParty))
                    {
                        throw new BusinessException("寄售状态供应商为必填！");
                    }
                }
                else//现在后台是按明细有供应商就是寄售 所以选择非寄售的话，把供应商清空。
                {
                    manufactureParty = null;
                }

                if (string.IsNullOrEmpty(StartTime))
                {
                    StartTime = DateTime.Now.ToString();
                }
                if (string.IsNullOrEmpty(PartyTo))
                {
                    PartyTo = PartyFrom;
                }
                if (insertedOrderDetails == null || insertedOrderDetails.Count() == 0)
                {
                    throw new BusinessException("明细不能为空！");
                }
                IList<OrderDetail> insertedOrderDetailList = insertedOrderDetails as List<OrderDetail>;
                int i = 0;
                foreach (OrderDetail orderDetail in insertedOrderDetailList)
                {
                    i++;
                    if (orderDetail.OrderedQty == 0)
                    {
                        throw new BusinessException("第" + i + "行明细数量不能为空！");
                    }
                    if (string.IsNullOrEmpty(orderDetail.LocationFrom))
                    {
                        throw new BusinessException("第" + i + "行来源库位不能为空。");
                    }
                    if (string.IsNullOrEmpty(orderDetail.LocationTo))
                    {
                        throw new BusinessException("第" + i + "行目的库位不能为空。");
                    }
                    if (string.IsNullOrEmpty(orderDetail.Item))
                    {
                        throw new BusinessException("第" + i + "行明细物料不能为空！");
                    }

                    Item item = base.genericMgr.FindById<Item>(orderDetail.Item);
                    orderDetail.ItemDescription = item.Description;
                    orderDetail.ReferenceItemCode = item.ReferenceCode;
                    orderDetail.Uom = item.Uom;
                    orderDetail.BaseUom = item.Uom;
                    orderDetail.UnitCount = item.UnitCount;
                    orderDetail.QualityType = com.Sconit.CodeMaster.QualityType.Reject;
                    orderDetail.ManufactureParty = manufactureParty != string.Empty ? manufactureParty : null;
                    OrderDetailInput orderDetailInput = new OrderDetailInput();
                    orderDetailInput.ReceiveQty = orderDetail.OrderedQty;
                    orderDetailInput.ConsignmentParty = orderDetail.ManufactureParty != string.Empty ? orderDetail.ManufactureParty : null;
                    orderDetailInput.QualityType = orderDetail.QualityType;
                    orderDetail.AddOrderDetailInput(orderDetailInput);
                }
                string orderNo = orderMgr.CreateFreeTransferOrderMaster("报验单移库",PartyFrom, PartyTo, insertedOrderDetailList, Convert.ToDateTime(StartTime),System.DateTime.Now,true,false);
                SaveSuccessMessage("移库单{0}创建成功。", orderNo);
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
        #endregion

        #region 退货
        [GridAction]
        [SconitAuthorize(Permissions = "Url_InspectionOrder_Return")]
        public ActionResult Return()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_InspectionOrder_Return")]
        public JsonResult CreateReturnOrder(string flow, string idStr, string qtyStr, string handleResult)
        {
            try
            {
                #region 检查路线
                if (string.IsNullOrEmpty(flow))
                {
                    throw new BusinessException("路线不能为空");
                }
                FlowMaster flowMaster = base.genericMgr.FindById<FlowMaster>(flow);
                #endregion


                if (string.IsNullOrEmpty(handleResult))
                {
                    throw new BusinessException("处理方式不能为空");
                }

                #region inspectResultList
                IList<InspectResult> inspectResultList = new List<InspectResult>();

                if (!string.IsNullOrEmpty(idStr))
                {
                    string[] idArray = idStr.Split(',');
                    string[] qtyArray = qtyStr.Split(',');
                    for (int i = 0; i < idArray.Count(); i++)
                    {
                        if (Convert.ToDecimal(qtyArray[i]) > 0)
                        {
                            InspectResult nonZeroInspectResult = base.genericMgr.FindById<InspectResult>(Convert.ToInt32(idArray[i]));
                            nonZeroInspectResult.CurrentHandleQty = Convert.ToDecimal(qtyArray[i]);
                            inspectResultList.Add(nonZeroInspectResult);
                        }
                    }
                }
                #endregion

                if (inspectResultList.Count == 0)
                {
                    throw new BusinessException("明细不能为空");
                }
                //   FlowMaster returnFlow = flowMgr.GetReverseFlow(flowMaster, inspectResultList.Select(r => r.Item).Distinct().ToList());
                //    OrderMaster orderMaster = orderMgr.TransferFlow2Order(returnFlow, null);
                // RejectMaster rej= inspectMgr.CreateReturnOrder(flowMaster, handleResult, inspectResultList);
                RejectMaster rejectMaster = inspectMgr.CreateRejectMaster((com.Sconit.CodeMaster.HandleResult)(Convert.ToInt32(handleResult)), inspectResultList);
                inspectMgr.ReleaseRejectMaster(rejectMaster.RejectNo);


                IList<RejectDetail> rejectDetailList = base.genericMgr.FindAll<RejectDetail>("select r from RejectDetail as r where r.RejectNo=? ", rejectMaster.RejectNo);
                foreach (RejectDetail rejectDetail in rejectDetailList)
                {
                    rejectDetail.CurrentHandleQty = rejectDetail.HandleQty;
                }
                orderMgr.CreateReturnOrder(flowMaster, rejectDetailList);
                SaveSuccessMessage(Resources.INP.RejectMaster.RejectMaster_Returned);
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

        #region 按移动类型退货

        #region 查询退货
        public ActionResult ReturnIndex()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_InspectionOrder_ReturnIndex")]
        public ActionResult ReturnList(GridCommand GridCommand, OutMiscOrderSearchModel searchModel)
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
        [SconitAuthorize(Permissions = "Url_InspectionOrder_ReturnIndex")]
        public ActionResult _AjaxReturnList(GridCommand command, OutMiscOrderSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<MiscOrderMaster>()));
            }
            SearchStatementModel searchStatementModel = PrepareReturnSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<MiscOrderMaster>(searchStatementModel, command));
        }


        private SearchStatementModel PrepareReturnSearchStatement(GridCommand command, OutMiscOrderSearchModel searchModel)
        {
            string whereStatement = "where m.Type = " + (int)com.Sconit.CodeMaster.MiscOrderType.Return;
            IList<object> param = new List<object>();
            if (!string.IsNullOrWhiteSpace(searchModel.WMSSeq))
            {
                whereStatement += " and exists( select 1 from MiscOrderDetail as d where d.MiscOrderNo=m.MiscOrderNo and d.WMSSeq='" + searchModel.WMSSeq + "' ) ";
            }
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
            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "StatusDescription")
                {
                    command.SortDescriptors[0].Member = "Status";
                }
            }

            string sortingStatement = string.Empty;
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by CreateDate desc";
            }
            else
            {
                sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            }
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = "select count(*) from MiscOrderMaster as m";
            searchStatementModel.SelectStatement = "select m from MiscOrderMaster as m";
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();
            return searchStatementModel;
        }
        #endregion

        #region New Return
        [SconitAuthorize(Permissions = "Url_InspectionOrder_ReturnNew")]
        public ActionResult ReturnNew(string MoveType)
        {

            if (!string.IsNullOrWhiteSpace(MoveType))
            {
                MiscOrderMoveType miscOrderMoveType = base.genericMgr.FindAll<MiscOrderMoveType>("from MiscOrderMoveType as m where m.MoveType=? and m.IOType=?", new object[] { MoveType, com.Sconit.CodeMaster.MiscOrderType.Return })[0];
                TempData["MiscOrderMoveType"] = miscOrderMoveType;
            }
            MiscOrderMaster miscOrderMaster = new MiscOrderMaster();
            miscOrderMaster.EffectiveDate = System.DateTime.Now;
            miscOrderMaster.MoveType = MoveType;
            return View(miscOrderMaster);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_InspectionOrder_ReturnNew")]
        public ActionResult ReturnNew(MiscOrderMaster miscOrderMaster)
        {
            MiscOrderMoveType MiscOrderMoveType = null;
            if (ModelState.IsValid)
            {
                MiscOrderMoveType = base.genericMgr.FindAll<MiscOrderMoveType>("from MiscOrderMoveType as m where m.MoveType=? and m.IOType=?", new object[] { miscOrderMaster.MoveType, com.Sconit.CodeMaster.MiscOrderType.Return })[0];
                // TempData["MiscOrderMoveType"] = MiscOrderMoveType;

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
                    SaveErrorMessage("Note不能为空");
                }
                else if (MiscOrderMoveType.CheckCostCenter && miscOrderMaster.CostCenter == null)
                {
                    SaveErrorMessage("成本中心不能为空");
                }
                else if (MiscOrderMoveType.CheckRefNo && miscOrderMaster.ReferenceNo == null)
                {
                    SaveErrorMessage("参考数不能为空");
                }
                else if (MiscOrderMoveType.CheckDeliverRegion && miscOrderMaster.DeliverRegion == null)
                {
                    SaveErrorMessage("收货区域不能为空");
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
                        return RedirectToAction("ReturnEdit/" + miscOrderMaster.MiscOrderNo);

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
                if (miscOrderMaster.MoveType != null)
                {
                    MiscOrderMoveType = base.genericMgr.FindAll<MiscOrderMoveType>("from MiscOrderMoveType as m where m.MoveType=? and m.IOType=?", new object[] { miscOrderMaster.MoveType, com.Sconit.CodeMaster.MiscOrderType.Return })[0];
                }
            }

            TempData["MiscOrderMoveType"] = MiscOrderMoveType;
            return View(miscOrderMaster);
        }

        [SconitAuthorize(Permissions = "Url_InspectionOrder_ReturnNew")]
        public ActionResult ReturnEdit(string id)
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
                MiscOrderMoveType miscOrderMoveType = base.genericMgr.FindAll<MiscOrderMoveType>("from MiscOrderMoveType as m where m.MoveType=? and m.IOType=?", new object[] { miscOrderMaster.MoveType, com.Sconit.CodeMaster.MiscOrderType.Return })[0];
                ViewBag.editorTemplate = miscOrderMaster.Status == com.Sconit.CodeMaster.MiscOrderStatus.Create ? "" : "ReadonlyTextBox";
                TempData["MiscOrderMoveType"] = miscOrderMoveType;
                return View(miscOrderMaster);
            }
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_InspectionOrder_ReturnNew")]
        public ActionResult ReturnEdit(MiscOrderMaster miscOrderMaster)
        {

            if (ModelState.IsValid)
            {
                MiscOrderMoveType MiscOrderMoveType = base.genericMgr.FindAll<MiscOrderMoveType>("from MiscOrderMoveType as m where m.MoveType=? and m.IOType=?", new object[] { miscOrderMaster.MoveType, com.Sconit.CodeMaster.MiscOrderType.Return })[0];
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
                    SaveErrorMessage("Note不能为空");
                }
                else if (MiscOrderMoveType.CheckCostCenter && miscOrderMaster.CostCenter == null)
                {
                    SaveErrorMessage("成本中心不能为空");
                }
                else if (MiscOrderMoveType.CheckRefNo && miscOrderMaster.ReferenceNo == null)
                {
                    SaveErrorMessage("参考数不能为空");
                }
                else if (MiscOrderMoveType.CheckDeliverRegion && miscOrderMaster.DeliverRegion == null)
                {
                    SaveErrorMessage("发货区域不能为空");
                }
                else if (MiscOrderMoveType.CheckWBS && miscOrderMaster.WBS == null)
                {
                    SaveErrorMessage("WBS不能为空");
                }
                else if (MiscOrderMoveType.CheckManufactureParty && (miscOrderMaster.ManufactureParty == string.Empty || miscOrderMaster.ManufactureParty == null))
                {
                    SaveErrorMessage("供应商不能为空");
                }
                else
                {
                    try
                    {
                        miscOrderMgr.UpdateMiscOrder(miscOrderMaster);
                        SaveSuccessMessage("修改成功");
                    }
                    catch (BusinessException ex)
                    {
                        SaveBusinessExceptionMessage(ex);
                    }
                    catch(Exception ex)
                    {
                        SaveErrorMessage(ex);
                    }
                }
            }
            return RedirectToAction("ReturnEdit/" + miscOrderMaster.MiscOrderNo);

        }

        [SconitAuthorize(Permissions = "Url_Inventory_OutMiscOrder_New")]
        public ActionResult DeleteReturnOrder(string id)
        {
            try
            {
                MiscOrderMaster MiscOrderMaster = base.genericMgr.FindById<MiscOrderMaster>(id);
                miscOrderMgr.DeleteMiscOrder(MiscOrderMaster);
                SaveSuccessMessage("删除成功");
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }
            return RedirectToAction("ReturnList");
        }

        [SconitAuthorize(Permissions = "Url_InspectionOrder_ReturnNew")]
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
                        CheckMiscOrderDetail(miscOrderDetail, MiscOrderMaster.MoveType, (int)com.Sconit.CodeMaster.MiscOrderType.Return);

                    }
                    miscOrderMgr.CloseMiscOrder(MiscOrderMaster);
                    MiscOrderMaster.Type = com.Sconit.CodeMaster.MiscOrderType.Return;
                    base.genericMgr.Update(MiscOrderMaster);
                    SaveSuccessMessage("确认成功");
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
            return RedirectToAction("ReturnEdit/" + id);
        }

        [SconitAuthorize(Permissions = "Url_InspectionOrder_ReturnNew")]
        public ActionResult btnCancel(string id)
        {
            try
            {
                MiscOrderMaster MiscOrderMaster = base.genericMgr.FindById<MiscOrderMaster>(id);
                miscOrderMgr.CancelMiscOrder(MiscOrderMaster);
                MiscOrderMaster.Type = com.Sconit.CodeMaster.MiscOrderType.Return;
                base.genericMgr.Update(MiscOrderMaster);
                SaveSuccessMessage("取消成功");

            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }

            return RedirectToAction("ReturnEdit/" + id);
        }

        #endregion
        #region OrderDetail
        [SconitAuthorize(Permissions = "Url_InspectionOrder_ReturnNew")]
        public ActionResult _ReturnOrderDetail(string MiscOrderNo, string MoveType, string Status)
        {
            MiscOrderMoveType MiscOrderMoveType = base.genericMgr.FindAll<MiscOrderMoveType>("from MiscOrderMoveType as m where m.MoveType=? and m.IOType=?", new object[] { MoveType, com.Sconit.CodeMaster.MiscOrderType.Return })[0];
            MiscOrderMaster miscOrder = base.genericMgr.FindById<MiscOrderMaster>(MiscOrderNo);
            ViewBag.editorTemplate = miscOrder.Status == com.Sconit.CodeMaster.MiscOrderStatus.Create ? "" : "ReadonlyTextBox";
            ViewBag.MoveType = MoveType;
            ViewBag.MiscOrderNo = MiscOrderNo;
            ViewBag.Status = Status;
            ViewBag.ReserveLine = MiscOrderMoveType.CheckReserveLine;
            ViewBag.ReserveNo = MiscOrderMoveType.CheckReserveNo;
            ViewBag.EBELN = MiscOrderMoveType.CheckEBELN;
            ViewBag.EBELP = MiscOrderMoveType.CheckEBELP;
            ViewBag.CheckConsignment = MiscOrderMoveType.CheckConsignment;
            return PartialView();

        }



        [GridAction]
        public ActionResult _SelectReturnOrderDetail(string MiscOrderNo, string MoveType)
        {
            ViewBag.MiscOrderNo = MiscOrderNo;
            IList<MiscOrderDetail> MiscOrderDetailList = base.genericMgr.FindAll<MiscOrderDetail>("from MiscOrderDetail as m where m.MiscOrderNo=?", MiscOrderNo);
            return View(new GridModel(MiscOrderDetailList));
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
        [SconitAuthorize(Permissions = "Url_InspectionOrder_ReturnNew")]
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
                //else if (miscOrderMoveType.CheckConsignment==com.Sconit.CodeMaster.CheckConsignment.Consignment && (miscOrderDetail.ManufactureParty == null||miscOrderDetail.ManufactureParty==string.Empty))
                //{
                //    throw new BusinessException("明细行供应商不能为空。");
                //}
                else
                {
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
        public JsonResult _SaveBatchEditing([Bind(Prefix =
            "inserted")]IEnumerable<MiscOrderDetail> insertedMiscOrderDetails,
            [Bind(Prefix = "updated")]IEnumerable<MiscOrderDetail> updatedMiscOrderDetails,
            [Bind(Prefix = "deleted")]IEnumerable<MiscOrderDetail> deletedMiscOrderDetails,
            string MiscOrderNo, string moveType)
        {
            try
            {
                MiscOrderMaster miscOrderMaster = base.genericMgr.FindById<MiscOrderMaster>(MiscOrderNo);
                // MiscOrderMoveType miscOrderMoveType = base.genericMgr.FindAll<MiscOrderMoveType>("from MiscOrderMoveType as m where m.MoveType=? and m.IOType=?", new object[] { moveType, com.Sconit.CodeMaster.MiscOrderType.GI })[0];
                IList<MiscOrderDetail> newMiscOrderDetailList = new List<MiscOrderDetail>();
                IList<MiscOrderDetail> updateMiscOrderDetailList = new List<MiscOrderDetail>();
                if (insertedMiscOrderDetails != null)
                {
                    foreach (var miscOrderDetail in insertedMiscOrderDetails)
                    {
                        if (CheckMiscOrderDetail(miscOrderDetail, moveType,(int) com.Sconit.CodeMaster.MiscOrderType.Return))
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
                        if (CheckMiscOrderDetail(miscOrderDetail, moveType,(int) com.Sconit.CodeMaster.MiscOrderType.Return))
                        {
                            updateMiscOrderDetailList.Add(miscOrderDetail);
                        }
                    }
                }
                miscOrderMgr.BatchUpdateMiscOrderDetails(MiscOrderNo, newMiscOrderDetailList, updateMiscOrderDetailList,
                                                         (IList<MiscOrderDetail>) deletedMiscOrderDetails);
                SaveSuccessMessage("更新成功");
                return Json(new {MiscOrderNo = MiscOrderNo});
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

        #region
        //[HttpPost] 
        //[SconitAuthorize(Permissions = "Url_Inventory_OutMiscOrder_New")]
        //public JsonResult CreateInspectionOrderReturn(MiscOrderMaster MiscOrderMaster,string idStr, string qtyStr, string handleResult
        //    , string ReserveLineStr, string ReserveNoStr, string EBELPStr)
        //{
        //    MiscOrderMoveType MiscOrderMoveType = null;
        //    try
        //    {

        //            MiscOrderMoveType = base.genericMgr.FindAll<MiscOrderMoveType>("from MiscOrderMoveType as m where m.MoveType=? and m.IOType=?", new object[] { MiscOrderMaster.MoveType, com.Sconit.CodeMaster.MiscOrderType.GI })[0];
        //            // TempData["MiscOrderMoveType"] = MiscOrderMoveType;

        //            if (MiscOrderMoveType.CheckAsn && MiscOrderMaster.Asn == null)
        //            {
        //                throw new BusinessException("送货单不能为空");
        //            }
        //            else if (MiscOrderMoveType.CheckRecLoc && MiscOrderMaster.ReceiveLocation == null)
        //            {
        //                throw new BusinessException("收货库位不能为空");
        //            }
        //            else if (MiscOrderMoveType.CheckNote && MiscOrderMaster.Note == null)
        //            {
        //                throw new BusinessException("Note不能为空");
        //            }
        //            else if (MiscOrderMoveType.CheckCostCenter && MiscOrderMaster.CostCenter == null)
        //            {
        //                throw new BusinessException("成本中心不能为空");
        //            }
        //            else if (MiscOrderMoveType.CheckRefNo && MiscOrderMaster.ReferenceNo == null)
        //            {
        //                throw new BusinessException("参考数不能为空");
        //            }
        //            else if (MiscOrderMoveType.CheckDeliverRegion && MiscOrderMaster.DeliverRegion == null)
        //            {
        //                throw new BusinessException("收货区域不能为空");
        //            }
        //            else if (MiscOrderMoveType.CheckWBS && MiscOrderMaster.WBS == null)
        //            {
        //                throw new BusinessException("WBS不能为空");
        //            }
        //            else if (MiscOrderMoveType.CheckManufactureParty && (MiscOrderMaster.ManufactureParty == string.Empty || MiscOrderMaster.ManufactureParty == null))
        //            {
        //                throw new BusinessException("供应商不能为空");
        //            }
        //            else
        //            {
        //                IList<InspectResult> inspectResultList=this.getRightInspectResult(MiscOrderMoveType, idStr, qtyStr, ReserveLineStr, ReserveNoStr, EBELPStr);
        //                MiscOrderMaster.CancelMoveType = MiscOrderMoveType.CancelMoveType;
        //                MiscOrderMaster.QualityType = com.Sconit.CodeMaster.QualityType.Reject;
        //                inspectMgr.CreateReturnOrder(MiscOrderMaster, handleResult, inspectResultList);
        //            }


        //        TempData["MiscOrderMoveType"] = MiscOrderMoveType;
        //        return Json(MiscOrderMoveType);
        //    }
        //    catch (BusinessException ex)
        //    {
        //        Response.TrySkipIisCustomErrors = true;
        //        Response.StatusCode = 500;
        //        Response.Write(ex.GetMessages()[0].GetMessageString());
        //        return Json(null);
        //    }
        //}


        //public ActionResult _InspectionReturnDetailList(GridCommand command,InspectResultSearchModel InspectResultSearchModel, string moveType)
        //{
        //    if (!string.IsNullOrEmpty(moveType))
        //    {
        //        MiscOrderMoveType MiscOrderMoveType = base.genericMgr.FindAll<MiscOrderMoveType>("from MiscOrderMoveType as m where m.MoveType=? and m.IOType=?", new object[] { moveType, com.Sconit.CodeMaster.MiscOrderType.GI })[0];
        //        // MiscOrderMaster miscOrder = base.genericMgr.FindById<MiscOrderMaster>(MiscOrderNo);
        //        // ViewBag.editorTemplate = miscOrder.Status == com.Sconit.CodeMaster.MiscOrderStatus.Create ? "" : "ReadonlyTextBox";
        //        ViewBag.MoveType = moveType;
        //        // ViewBag.MiscOrderNo = MiscOrderNo;
        //        // ViewBag.Status = Status;
        //        ViewBag.ReserveLine = MiscOrderMoveType.CheckReserveLine;
        //        ViewBag.ReserveNo = MiscOrderMoveType.CheckReserveNo;
        //        // ViewBag.EBELN = MiscOrderMoveType.CheckEBELN;
        //        ViewBag.EBELP = MiscOrderMoveType.CheckEBELP;
        //        ViewBag.ManufactureParty = InspectResultSearchModel.ManufactureParty;
        //    }
        //    else
        //    {
        //        ViewBag.MoveType = string.Empty;
        //        ViewBag.ReserveLine = false;
        //        ViewBag.ReserveNo = false;
        //        ViewBag.EBELP = false;
        //    }
        //    return PartialView();
        //}


        //[GridAction]
        //[SconitAuthorize(Permissions = "Url_StockTake_Edit")]
        //private IList<InspectResult> getRightInspectResult(MiscOrderMoveType miscOrderMoveType, string idStr, string qtyStr
        //    ,string ReserveLineStr,string ReserveNoStr,string EBELPStr)
        //{
        //    IList<InspectResult> inspectResultList = new List<InspectResult>();
        //   // bool isValid = true;
        //    if (string.IsNullOrEmpty(idStr))
        //    {
        //        throw new BusinessException("退货明细不能为空。");
        //    }
        //    string[] idArray = idStr.Split(',');
        //    string[] qtyArray = qtyStr.Split(',');
        //    string[] ReserveLineArr = new string[idArray.Length];
        //    string[] ReserveNoArr = new string[idArray.Length];
        //    string[] EBELPStrArr = new string[idArray.Length];
        //    if (!string.IsNullOrEmpty(ReserveLineStr))
        //    {
        //        ReserveLineArr = ReserveLineStr.Split(',');
        //    }
        //    if (!string.IsNullOrEmpty(ReserveNoStr))
        //    {
        //     ReserveNoArr = ReserveNoStr.Split(',');
        //         }
        //    if (!string.IsNullOrEmpty(EBELPStr))
        //    {
        //        EBELPStrArr = EBELPStr.Split(',');
        //    }
        //    for (int i = 0; i < idArray.Count(); i++)
        //    {
        //        if (Convert.ToDecimal(qtyArray[i]) > 0)
        //        {
        //            if (miscOrderMoveType.CheckEBELP && EBELPStrArr[i] == string.Empty)
        //            {
        //                throw new BusinessException("明细部分要货单行号不能为空");
        //            }
        //            else if (miscOrderMoveType.CheckReserveLine && ReserveLineArr[i] == string.Empty)
        //            {
        //                throw new BusinessException("明细部分预留行不能为空");
        //            }
        //            else if (miscOrderMoveType.CheckReserveNo && ReserveNoArr[i] == string.Empty)
        //            {
        //                throw new BusinessException("明细部分预留号不能为空");
        //            }
        //            InspectResult nonZeroInspectResult = base.genericMgr.FindById<InspectResult>(Convert.ToInt32(idArray[i]));
        //            nonZeroInspectResult.CurrentHandleQty = Convert.ToDecimal(qtyArray[i]);
        //            nonZeroInspectResult.ReserveLine = ReserveLineArr[i];
        //            nonZeroInspectResult.ReserveNo = ReserveNoArr[i];
        //            nonZeroInspectResult.EBELP = EBELPStrArr[i];
        //            inspectResultList.Add(nonZeroInspectResult);
        //        }
        //    }

        //    return inspectResultList;
        //}
        #endregion
        #endregion

        #region private method
        private SearchStatementModel InspectResultPrepareSearchStatement(GridCommand command, InspectResultSearchModel searchModel)
        {
            string whereStatement = "where i.InspectNo = '" + searchModel.InspectNo + "'";
            IList<object> param = new List<object>();
            HqlStatementHelper.AddLikeStatement("Item", searchModel.Item, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "JudgeResultDescription")
                {
                    command.SortDescriptors[0].Member = "JudgeResult";
                }
                else if (command.SortDescriptors[0].Member == "DefectDescription")
                {
                    command.SortDescriptors[0].Member = "Defect";
                }
                else if (command.SortDescriptors[0].Member == "FailCodeDescription")
                {
                    command.SortDescriptors[0].Member = "FailCode";
                }
            }
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = InspectResultSelectCountStatement;
            searchStatementModel.SelectStatement = InspectResultSelectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();
            return searchStatementModel;
        }

        private string PrepareDetailSearchStatement(GridCommand command, InspectMasterSearchModel searchModel)
        {
            StringBuilder Sb = new StringBuilder();
            string whereStatement = "select detailResult.*,isnull(rest.handleQty,0) from ( select id.*,i.Status from INP_InspectDet as id inner join INP_InspectMstr as i on i.InpNo=id.InpNo  where 1=1";

            //if (!string.IsNullOrWhiteSpace(searchModel.IpNo) || !string.IsNullOrWhiteSpace(searchModel.ReceiptNo) || !string.IsNullOrWhiteSpace(searchModel.WMSNo) || searchModel.Status != null)
            //{
            //    whereStatement += " and exists (select 1 from INP_InspectMstr as i where  i.InpNo= id.InpNo ";
            //}

            if (searchModel.IpNo != null && searchModel.IpNo != "")
            {
                whereStatement += " and i.IpNo='" + searchModel.IpNo + "'";
            }
            if (searchModel.WMSNo != null && searchModel.WMSNo != "")
            {
                whereStatement += " and i.WMSNo='" + searchModel.WMSNo + "'";
            }
            if (searchModel.ReceiptNo != null && searchModel.ReceiptNo != "")
            {
                whereStatement += " and i.RecNo='" + searchModel.ReceiptNo + "'";
            }
            if (searchModel.Status != null)
            {
                whereStatement += " and i.Status=" + searchModel.Status;
            }
            //if (!string.IsNullOrWhiteSpace(searchModel.IpNo) || !string.IsNullOrWhiteSpace(searchModel.ReceiptNo) || !string.IsNullOrWhiteSpace(searchModel.WMSNo) || searchModel.Status != null)
            //{
            //    whereStatement += ")";
            //}
            Sb.Append(whereStatement);
            if (searchModel.InspectNo != null)
            {
                Sb.Append(string.Format(" and id.InpNo = '{0}'", searchModel.InspectNo));
            }

            if (!string.IsNullOrEmpty(searchModel.Item))
            {
                Sb.Append(string.Format(" and id.Item ='{0}'", searchModel.Item));
            }
            if (searchModel.CreateUserName != null)
            {
                Sb.Append(string.Format(" and id.CreateUserNm = '{0}'", searchModel.CreateUserName));
            }

            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                Sb.Append(string.Format(" and id.CreateDate  between '{0}' and '{1}'", new object[] { searchModel.StartDate, searchModel.EndDate }));
            }
            else if (searchModel.StartDate != null & searchModel.EndDate == null)
            {
                Sb.Append(string.Format(" and id.CreateDate >= '{0}'", searchModel.StartDate));
            }
            else if (searchModel.StartDate == null & searchModel.EndDate != null)
            {
                Sb.Append(string.Format(" and id.CreateDate <= '{0}'", searchModel.EndDate));
            }

            Sb.Append(@" ) as detailResult left join  
(select ir.inpdetid,sum(ir.HandleQty) 
as handleQty from INP_InspectResult as ir group by inpdetid) as rest
on detailResult.Id=rest.inpdetid  order by detailResult.CreateDate desc");
            return Sb.ToString();

        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, InspectMasterSearchModel searchModel, string whereStatement)
        {
            IList<object> param = new List<object>();
            SecurityHelper.AddRegionPermissionStatement(ref whereStatement, "i", "Region");
            HqlStatementHelper.AddLikeStatement("InspectNo", searchModel.InspectNo, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("CreateUserName", searchModel.CreateUserName, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Status", searchModel.Status, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Region", searchModel.Region, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Type", searchModel.Type, "i", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("IpNo", searchModel.IpNo, HqlStatementHelper.LikeMatchMode.End, "i", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("WMSNo", searchModel.WMSNo, HqlStatementHelper.LikeMatchMode.End, "i", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("ReceiptNo", searchModel.ReceiptNo, HqlStatementHelper.LikeMatchMode.End, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Type", searchModel.Type, "i", ref whereStatement, param);

            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.StartDate, searchModel.EndDate, "i", ref whereStatement, param);
            }
            else if (searchModel.StartDate != null & searchModel.EndDate == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.StartDate, "i", ref whereStatement, param);
            }
            else if (searchModel.StartDate == null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.EndDate, "i", ref whereStatement, param);
            }
            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "InspectTypeDescription")
                {
                    command.SortDescriptors[0].Member = "Type";
                }
                else if (command.SortDescriptors[0].Member == "InspectStatusDescription")
                {
                    command.SortDescriptors[0].Member = "Status";
                }
            }
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by i.CreateDate desc";
            }



            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        private JsonResult BatchJudge(string inspectNo, string idStr, bool isQualify, string failCodeStr, string notes)
        {
            try
            {
                string[] idArr = idStr.Split(',');
                string[] noteArr = null;
                if (!string.IsNullOrEmpty(notes))
                {
                    noteArr = notes.Split(',');
                }
                string[] failCodeArr = null;
                if (!string.IsNullOrEmpty(failCodeStr))
                {
                    failCodeArr = failCodeStr.Split(',');
                }

                IList<InspectDetail> inspectDetailList = new List<InspectDetail>();

                for (int i = 0; i < idArr.Length; i++)
                {
                    InspectDetail inspectDetail = base.genericMgr.FindById<InspectDetail>(Convert.ToInt32(idArr[i]));

                    inspectDetail.CurrentQualifyQty = isQualify ? inspectDetail.InspectQty : 0;
                    inspectDetail.CurrentRejectQty = isQualify ? 0 : inspectDetail.InspectQty;
                    if (failCodeArr != null)
                    {
                        inspectDetail.JudgeFailCode = failCodeArr[i];
                    }
                    if (noteArr != null)
                    {
                        inspectDetail.CurrentInspectResultNote = noteArr[i];
                    }
                    inspectDetailList.Add(inspectDetail);
                }

                inspectMgr.JudgeInspectDetail(inspectDetailList);
                SaveSuccessMessage("判定成功");
                return Json(new { InspectNo = inspectNo });
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

        #endregion

        #region 打印
        public string Prints(string InspectNos)
        {
            IList<InspectMaster> inspectMasterList = this.genericMgr.FindEntityWithNativeSql<InspectMaster>(" select * from INP_InspectMstr where InpNo in ('" + InspectNos.Replace(",", "','") + "') ");
            IList<InspectDetail> inspectDetailList = this.genericMgr.FindEntityWithNativeSql<InspectDetail>(" select * from INP_InspectDet where InpNo in  ('" + InspectNos.Replace(",", "','") + "') ");
            IList<PrintInspectMaster> printList = new List<PrintInspectMaster>();
            foreach (var inspectMaster in inspectMasterList)
            {
                foreach (InspectDetail inspectDetail in inspectDetailList.Where(d => d.InspectNo == inspectMaster.InspectNo).ToList())
                {
                    //inspectMaster.InspectDetails = inspectDetailList.Where(d => d.InspectNo == inspectMaster.InspectNo).ToList();
                    inspectMaster.InspectDetails = new List<InspectDetail>();
                    inspectMaster.InspectDetails.Add(inspectDetail);
                    PrintInspectMaster printMaster = Mapper.Map<InspectMaster, PrintInspectMaster>(inspectMaster);
                    IpMaster ipmaster = this.genericMgr.FindById<IpMaster>(inspectMaster.IpNo);
                    IpDetail ipdet = this.genericMgr.FindAll<IpDetail>(" select d from IpDetail as d where d.IpNo=? ", ipmaster.IpNo)[0];
                    Supplier sp = this.genericMgr.FindById<Supplier>(ipmaster.PartyFrom);
                    printMaster.PartyFrom = sp.Code;
                    printMaster.PartyFromName = sp.Name + "(" + sp.ShortCode + ")";
                    switch (ipdet.PSTYP)
                    {
                        case "0":
                            printMaster.ConsignmentType = "标准";
                            break;
                        case "2":
                            printMaster.ConsignmentType = "寄售";
                            break;
                        case "7":
                            printMaster.ConsignmentType = "委外";
                            break;
                        default:
                            printMaster.ConsignmentType = string.Empty;
                            break;
                    }
                    printList.Add(printMaster);
                }
            }
            IList<object> data = new List<object>();
            data.Add(printList);
            data.Add(CurrentUser.FullName);
            return reportGen.WriteToFile("InspectOrder.xls", data);
        }
        
         #endregion
    }
}
