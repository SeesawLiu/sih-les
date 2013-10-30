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
    using com.Sconit.Entity.CUST;
    using com.Sconit.Entity.Exception;
    using com.Sconit.Entity.SCM;
    using com.Sconit.Utility;
    using com.Sconit.PrintModel.INP;
    using com.Sconit.Utility.Report;

    public class RejectOrderController : WebAppBaseController
    {
        //
        // GET: /InspectionOrder/
        public IInspectMgr inspectMgr { get; set; }
        public IOrderMgr orderMgr { get; set; }
        public IReportGen reportGen { get; set; }
       

        private static string inspectResultSelectCountStatement = "select count(*) from InspectResult as i";
        private static string inspectResultSelectStatement = "select i from InspectResult as i ";
        private static string rejectDetailSelectStatement = "select r from RejectDetail as r ";

        private static string selectStatement = "select r from RejectMaster as r";

        private static string selectCountStatement = "select count(*) from RejectMaster as r ";

        #region view
        [SconitAuthorize(Permissions = "Url_RejectOrder_View")]
        public ActionResult Index()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_RejectOrder_View")]
        [GridAction]
        public ActionResult List(GridCommand command, RejectMasterSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [SconitAuthorize(Permissions = "Url_RejectOrder_View")]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxList(GridCommand command, RejectMasterSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = RejectMasterPrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<RejectMaster>(searchStatementModel, command));
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_RejectOrder_View")]
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                RejectMaster rejectMaster = base.genericMgr.FindById<RejectMaster>(id);
                rejectMaster.RejectMasterStatusDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.RejectStatus, ((int)rejectMaster.Status).ToString());
                rejectMaster.RejectMasterHandleResultDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.HandleResult, ((int)rejectMaster.HandleResult).ToString());
                rejectMaster.InspectTypeDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.InspectType, ((int)rejectMaster.InspectType).ToString());
                return View(rejectMaster);
            }
        }

        public ActionResult Close(string id) {
            try
            {
                //orderMgr.ManualCloseOrder(id);
                RejectMaster rejectMaster = base.genericMgr.FindById<RejectMaster>(id);
                inspectMgr.CloseRejectMaster(rejectMaster);
                SaveSuccessMessage("不合格品处理单{0}关闭成功。",id);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            return RedirectToAction("Edit", new { id = id });
        
        }

        #endregion

        #region Detail
        [SconitAuthorize(Permissions = "Url_RejectOrder_Detail")]
        public ActionResult Detail()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_RejectOrder_View")]
        [GridAction]
        public ActionResult DetailList(GridCommand command, RejectDetailSearchModel searchModel)
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

        [SconitAuthorize(Permissions = "Url_RejectOrder_View")]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxRejectDetailList(GridCommand command, RejectDetailSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<RejectDetail>()));
            }
           

            SearchStatementModel searchStatementModel = RejectDetailPrepareSearchStatement(command, searchModel);
           GridModel<RejectDetail> list=   GetAjaxPageData<RejectDetail>(searchStatementModel, command);

            IList<FailCode> failCodeList = base.genericMgr.FindAll<FailCode>();

            foreach (RejectDetail rejectDetail in list.Data)
            {
                foreach (FailCode failCode in failCodeList)
                {
                    if (rejectDetail.FailCode == failCode.Code)
                    {
                        rejectDetail.FailCode = failCode.CodeDescription;
                    }
                }
            }
            return PartialView(list);
        }

        #endregion

        #region new
        [GridAction]
        [SconitAuthorize(Permissions = "Url_RejectOrder_New")]
        public ActionResult New(GridCommand command, InspectResultSearchModel searchModel)
        {
            this.ProcessSearchModel(command,searchModel);
            ViewBag.InspectType = searchModel.InspectType == null ? (int)com.Sconit.CodeMaster.InspectType.Quantity : searchModel.InspectType.Value;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_RejectOrder_New")]
        public ActionResult _AjaxInspectResultNewList(GridCommand command, InspectResultSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = NewPrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<InspectResult>(searchStatementModel, command));
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_RejectOrder_New")]
        public ActionResult _GetHandleRsults(string text)
        {
            IList<CodeDetail> handleResults = systemMgr.GetCodeDetails(com.Sconit.CodeMaster.CodeMaster.HandleResult);
            return new JsonResult
            {
                Data = Transfer2DropDownList(com.Sconit.CodeMaster.CodeMaster.HandleResult, handleResults)
            };

        }

        [SconitAuthorize(Permissions = "Url_RejectOrder_New")]
        public string Create(string handleResult, string idStr, string qtyStr)
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
                SaveSuccessMessage("不合格品处理单创建成功");
                return rejectMaster.RejectNo;
            }
            catch (BusinessException ex)
            {
                Response.TrySkipIisCustomErrors = true;
                Response.StatusCode = 500;
                Response.Write(ex.GetMessages()[0].GetMessageString());
                return string.Empty;
            }
        }

        [SconitAuthorize(Permissions = "Url_RejectOrder_New")]
        public string CreateByHu(string handleResult, string idStr)
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

                    foreach (string id in idArray)
                    {
                        InspectResult nonZeroInspectResult = base.genericMgr.FindById<InspectResult>(Convert.ToInt32(id));

                        nonZeroInspectResult.CurrentHandleQty = nonZeroInspectResult.TobeHandleQty;
                        inspectResultList.Add(nonZeroInspectResult);
                    }
                }
                #endregion

                if (inspectResultList.Count == 0)
                {
                    throw new BusinessException("明细不能为空");
                }
                RejectMaster rejectMaster = inspectMgr.CreateRejectMaster((com.Sconit.CodeMaster.HandleResult)(Convert.ToInt32(handleResult)), inspectResultList);
                SaveSuccessMessage("不合格品处理单创建成功");
                return rejectMaster.RejectNo;
            }
            catch (BusinessException ex)
            {
                Response.TrySkipIisCustomErrors = true;
                Response.StatusCode = 500;
                Response.Write(ex.GetMessages()[0].GetMessageString());
                return string.Empty;
            }
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_RejectOrder_New")]
        public ActionResult ChooseInspectResult(GridCommand command, InspectResultSearchModel searchModel)
        {
            TempData["InspectResultSearchModel"] = searchModel;
            ViewBag.Status = searchModel.Status;
            ViewBag.InspectType = searchModel.InspectType;
            ViewBag.RejectNo = searchModel.RejectNo;

            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);

            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_RejectOrder_New")]
        public ActionResult _AjaxInspectResultList(GridCommand command, InspectResultSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PopPrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<InspectResult>(searchStatementModel, command));
        }

        [SconitAuthorize(Permissions = "Url_RejectOrder_New")]
        public JsonResult AddRejectDetails(string rejectNo, string idStr, string qtyStr)
        {
            try
            {
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
                inspectMgr.AddRejectDetails(rejectNo, inspectResultList);
                object obj = new { SuccessMessage = string.Format("不合格品处理单明细添加成功") };
                return Json(obj);
            }
            catch (BusinessException ex)
            {
                Response.TrySkipIisCustomErrors = true;
                Response.StatusCode = 500;
                Response.Write(ex.GetMessages()[0].GetMessageString());
                return Json(null);
            }


        }


        [SconitAuthorize(Permissions = "Url_RejectOrder_New")]
        public JsonResult AddHuRejectDetails(string rejectNo, string idStr)
        {
            try
            {
                #region inspectResultList
                IList<InspectResult> inspectResultList = new List<InspectResult>();

                if (!string.IsNullOrEmpty(idStr))
                {
                    string[] idArray = idStr.Split(',');
                    for (int i = 0; i < idArray.Count(); i++)
                    {

                        InspectResult nonZeroInspectResult = base.genericMgr.FindById<InspectResult>(Convert.ToInt32(idArray[i]));
                        nonZeroInspectResult.CurrentHandleQty = nonZeroInspectResult.TobeHandleQty;
                        inspectResultList.Add(nonZeroInspectResult);

                    }
                }
                #endregion

                if (inspectResultList.Count == 0)
                {
                    throw new BusinessException("明细不能为空");
                }
                inspectMgr.AddRejectDetails(rejectNo, inspectResultList);
                object obj = new { SuccessMessage = string.Format("不合格品处理单明细添加成功") };
                return Json(obj);
            }
            catch (BusinessException ex)
            {
                Response.TrySkipIisCustomErrors = true;
                Response.StatusCode = 500;
                Response.Write(ex.GetMessages()[0].GetMessageString());
                return Json(null);
            }


        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_RejectOrder_New")]
        public JsonResult _BatchSaveRejectDetail(string rejectNo, string idStr, string qtyStr)
        {
            try
            {
                IList<RejectDetail> updatedRejectDetailList = new List<RejectDetail>();
                if (!string.IsNullOrEmpty(idStr))
                {
                    string[] idArray = idStr.Split(',');
                    string[] qtyArray = qtyStr.Split(',');
                    for (int i = 0; i < idArray.Count(); i++)
                    {

                        RejectDetail noneZeroRejectDetail = base.genericMgr.FindById<RejectDetail>(Convert.ToInt32(idArray[i]));
                        noneZeroRejectDetail.CurrentHandleQty = Convert.ToDecimal(qtyArray[i]);
                        updatedRejectDetailList.Add(noneZeroRejectDetail);

                    }
                }

                inspectMgr.BatchUpdateRejectDetails(rejectNo, updatedRejectDetailList, null);

                object obj = new { SuccessMessage = string.Format("不合格品处理单明细保存成功") };
                return Json(obj);


            }
            catch (BusinessException ex)
            {
                Response.TrySkipIisCustomErrors = true;
                Response.StatusCode = 500;
                Response.Write(ex.GetMessages()[0].GetMessageString());
                return Json(null);
            }
        }


        [GridAction]
        [SconitAuthorize(Permissions = "Url_RejectOrder_New")]
        public JsonResult _BatchDeleteRejectDetail(string rejectNo, string idStr)
        {
            try
            {
                IList<RejectDetail> deletedRejectDetailList = new List<RejectDetail>();
                if (!string.IsNullOrEmpty(idStr))
                {
                    string[] idArray = idStr.Split(',');

                    for (int i = 0; i < idArray.Count(); i++)
                    {

                        RejectDetail noneZeroRejectDetail = base.genericMgr.FindById<RejectDetail>(Convert.ToInt32(idArray[i]));

                        deletedRejectDetailList.Add(noneZeroRejectDetail);

                    }
                }
                inspectMgr.BatchUpdateRejectDetails(rejectNo, null, deletedRejectDetailList);
                object obj = new { SuccessMessage = string.Format("不合格品处理单明细删除成功") };
                return Json(obj);
            }
            catch (BusinessException ex)
            {
                Response.TrySkipIisCustomErrors = true;
                Response.StatusCode = 500;
                Response.Write(ex.GetMessages()[0].GetMessageString());
                return Json(null);
            }
        }



        [SconitAuthorize(Permissions = "Url_RejectOrder_New")]
        public ActionResult RejectDetail(string rejectNo, int status, int inspectType)
        {
            ViewBag.RejectNo = rejectNo;
            ViewBag.Status = status;
            ViewBag.InspectType = inspectType;

            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_RejectOrder_New")]
        public ActionResult _AjaxRejectDetail(string rejectNo)
        {
            string hql = rejectDetailSelectStatement + " where r.RejectNo = '" + rejectNo + "'";
            IList<RejectDetail> rejectDetailList = base.genericMgr.FindAll<RejectDetail>(hql);
            IList<FailCode> failCodeList = base.genericMgr.FindAll<FailCode>();

            foreach (RejectDetail rejectDetail in rejectDetailList)
            {
                foreach (FailCode failCode in failCodeList)
                {
                    if (rejectDetail.FailCode == failCode.Code)
                    {
                        rejectDetail.FailCode = failCode.CodeDescription;
                    }
                }
            }
            return PartialView(new GridModel<RejectDetail>(rejectDetailList));
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_RejectOrder_New")]
        public ActionResult Submit(string rejectNo)
        {
            try
            {
                inspectMgr.ReleaseRejectMaster(rejectNo);
                SaveSuccessMessage("不合格品处理单释放成功");
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            return RedirectToAction("Edit/" + rejectNo);
        }
        #endregion

        #region return
        [GridAction]
        [SconitAuthorize(Permissions = "Url_RejectOrder_Return")]
        public ActionResult Return()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_RejectOrder_Return")]
        public ActionResult _ReturnDetailList(string rejectNo)
        {
            #region rejectmstr
            RejectMaster rejectMaster = base.genericMgr.FindById<RejectMaster>(rejectNo);
            ViewBag.InspectType = (int)rejectMaster.InspectType;
            #endregion

            string hql = "select r from RejectDetail r where r.RejectNo = ? and HandleQty > HandledQty";
            IList<RejectDetail> rejectDetailList = base.genericMgr.FindAll<RejectDetail>(hql, rejectNo);
            return PartialView(rejectDetailList);
        }

        [SconitAuthorize(Permissions = "Url_RejectOrder_Return")]
        public JsonResult CreateReturnOrder(string flow, string idStr, string qtyStr)
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

                if (string.IsNullOrEmpty(idStr) || string.IsNullOrEmpty(qtyStr))
                {
                    throw new BusinessException("退货明细不能为空");
                }
                string[] idArr = idStr.Split(',');
                string[] qtyArr = qtyStr.Split(',');
                IList<RejectDetail> rejectDetailList = new List<RejectDetail>();
                for (int i = 0; i < idArr.Count(); i++)
                {
                    RejectDetail rejectDetail = base.genericMgr.FindById<RejectDetail>(Convert.ToInt32(idArr[i]));
                    rejectDetail.CurrentHandleQty = Convert.ToDecimal(qtyArr[i]);
                    rejectDetailList.Add(rejectDetail);
                }
                orderMgr.CreateReturnOrder(flowMaster, rejectDetailList);
                object obj = new { SuccessMessage = string.Format(Resources.INP.RejectMaster.RejectMaster_Returned) };
                return Json(obj);
            }
            catch (BusinessException ex)
            {
                Response.TrySkipIisCustomErrors = true;
                Response.StatusCode = 500;
                Response.Write(ex.GetMessages()[0].GetMessageString());
                return Json(null);
            }
        }

        [SconitAuthorize(Permissions = "Url_RejectOrder_Return")]
        public JsonResult CreateHuReturnOrder(string flow, string idStr)
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

                if (string.IsNullOrEmpty(idStr))
                {
                    throw new BusinessException("退货明细不能为空");
                }
                string[] idArr = idStr.Split(',');
               
                IList<RejectDetail> rejectDetailList = new List<RejectDetail>();
                foreach (string id in idArr)
                {
                    RejectDetail rejectDetail = base.genericMgr.FindById<RejectDetail>(Convert.ToInt32(id));
                    rejectDetail.CurrentHandleQty = rejectDetail.HandleQty;
                    rejectDetailList.Add(rejectDetail);
                }
               
                orderMgr.CreateReturnOrder(flowMaster, rejectDetailList);
                object obj = new { SuccessMessage = string.Format(Resources.INP.RejectMaster.RejectMaster_Returned) };
                return Json(obj);
            }
            catch (BusinessException ex)
            {
                Response.TrySkipIisCustomErrors = true;
                Response.StatusCode = 500;
                Response.Write(ex.GetMessages()[0].GetMessageString());
                return Json(null);
            }
        }

        #endregion

        #region transfer
        [GridAction]
        [SconitAuthorize(Permissions = "Url_RejectOrder_Transfer")]
        public ActionResult Transfer()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_RejectOrder_Transfer")]
        public ActionResult _TransferDetailList(string rejectNo)
        {
            string hql = "select r from RejectDetail r where r.RejectNo = ? and HandleQty > HandledQty";
            IList<RejectDetail> rejectDetailList = base.genericMgr.FindAll<RejectDetail>(hql, rejectNo);
            return PartialView(rejectDetailList);
        }

        [SconitAuthorize(Permissions = "Url_RejectOrder_Transfer")]
        public JsonResult CreateRejectTransfer(string location, string idStr, string qtyStr)
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
                    throw new BusinessException("退货明细不能为空");
                }
                string[] idArr = idStr.Split(',');

                IList<RejectDetail> rejectDetailList = new List<RejectDetail>();
                foreach (string id in idArr)
                {
                    RejectDetail rejectDetail = base.genericMgr.FindById<RejectDetail>(Convert.ToInt32(id));
                    rejectDetailList.Add(rejectDetail);
                }

                orderMgr.CreateRejectTransfer(locTo, rejectDetailList);
                object obj = new { SuccessMessage = string.Format(Resources.INP.RejectMaster.RejectMaster_Transfered) };
                return Json(obj);
            }
            catch (BusinessException ex)
            {
                Response.TrySkipIisCustomErrors = true;
                Response.StatusCode = 500;
                Response.Write(ex.GetMessages()[0].GetMessageString());
                return Json(null);
            }
        }

        #endregion

        #region WorkersWaste
        [SconitAuthorize(Permissions = "Url_RejectOrder_WorkersWaste")]
        public ActionResult WorkersWaste()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [SconitAuthorize(Permissions = "Url_RejectOrder_WorkersWaste")]
        public string WorkersWasteCreate(string region, string locationFrom, [Bind(Prefix =
              "inserted")]IEnumerable<InspectDetail> insertedInspectDetails)
        {
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
                    foreach (InspectDetail inspectDetail in insertedInspectDetails)
                    {
                        if (!string.IsNullOrEmpty(inspectDetail.Item) && inspectDetail.InspectQty > 0)
                        {
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
                }
                #endregion

                if (inspectDetailList != null && inspectDetailList.Count == 0)
                {
                    throw new BusinessException("工废明细不能为空");
                }

                InspectMaster inspectMaster = new InspectMaster();
                inspectMaster.Region = region;
                inspectMaster.InspectDetails = inspectDetailList;

                inspectMaster.Type = com.Sconit.CodeMaster.InspectType.Quantity;
                inspectMaster.IsATP = false;

                inspectMgr.CreateWorkersWaste(inspectMaster);
                SaveSuccessMessage("工废创建成功");
                return "Success";

            }
            catch (BusinessException ex)
            {
                Response.TrySkipIisCustomErrors = true;
                Response.StatusCode = 500;
                Response.Write(ex.GetMessages()[0].GetMessageString());
                return string.Empty;
            }

        }

        [SconitAuthorize(Permissions = "Url_RejectOrder_WorkersWaste")]
        public ActionResult _WorkersWasteDetail()
        {
            return PartialView();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_RejectOrder_WorkersWaste")]
        public ActionResult _SelectWorkersWasteBatchEditing()
        {
            IList<InspectDetail> inspectDetailList = new List<InspectDetail>();
            return View(new GridModel(inspectDetailList));
        }

        [SconitAuthorize(Permissions = "Url_RejectOrder_WorkersWaste")]
        public ActionResult _WebWorkersWasteDetail(string itemCode)
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
        #endregion

        #region Print

        public string Print(string rejectNo)
        {
            try
            {
                RejectMaster rejectMaster = base.genericMgr.FindById<RejectMaster>(rejectNo);
                PrintRejectMaster printOrderMstr = Mapper.Map<RejectMaster, PrintRejectMaster>(rejectMaster);
                IList<RejectDetail> rejectDetailList = base.genericMgr.FindAll<RejectDetail>("select r from RejectDetail as r where r.RejectNo=?",rejectNo);
                IList<FailCode> failCodeList = base.genericMgr.FindAll<FailCode>();

                foreach (RejectDetail rejectDetail in rejectDetailList)
                {
                    foreach (FailCode failCode in failCodeList)
                    {
                        if (rejectDetail.FailCode == failCode.Code)
                        {
                            rejectDetail.FailCode = failCode.CodeDescription;
                        }
                    }
                }

                IList<PrintRejectDetail> printRejectDetailList = Mapper.Map<IList<RejectDetail>, IList<PrintRejectDetail>>(rejectDetailList);
                IList<object> data = new List<object>();
                data.Add(printOrderMstr);
                data.Add(printRejectDetailList);
                string reportFileUrl = reportGen.WriteToFile("RejectOrder.xls", data);
                return reportFileUrl;
            }
            catch (BusinessException be)
            {
                return be.GetMessages()[0].GetMessageString();
            }

        }

        #endregion

        #region private method
        private SearchStatementModel RejectMasterPrepareSearchStatement(GridCommand command, RejectMasterSearchModel searchModel)
        {
            string whereStatement = string.Empty;
            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("RejectNo", searchModel.RejectNo, HqlStatementHelper.LikeMatchMode.Start, "r", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Status", searchModel.Status, "r", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("HandleResult", searchModel.HandleResult, "r", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("CreateUserName", searchModel.CreateUserName, "r", ref whereStatement, param);

            if (searchModel.StartDate != null && searchModel.EndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.StartDate, searchModel.EndDate, "r", ref whereStatement, param);

            }
            else if (searchModel.StartDate != null && searchModel.EndDate == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.StartDate, "r", ref whereStatement, param);
            }
            else if (searchModel.StartDate == null && searchModel.EndDate != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.EndDate, "r", ref whereStatement, param);
            }

            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "RejectMasterStatusDescription")
                {
                    command.SortDescriptors[0].Member = "Status";
                }
                else if (command.SortDescriptors[0].Member == "RejectMasterHandleResultDescription")
                {
                    command.SortDescriptors[0].Member = "HandleResult";
                }
            }
            //if (!string.IsNullOrEmpty(searchModel.InspectNo))
            //{
            //    if (whereStatement == string.Empty)
            //        whereStatement +=string.Format( " where exists (select 1 from RejectDetail as d where d.InspectNo='{0}%')",searchModel.InspectNo);
            //    else
            //        whereStatement += string.Format(" and exists (select 1 from RejectDetail as d where d.InspectNo='{0}%')", searchModel.InspectNo);
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
            searchStatementModel.SelectCountStatement = inspectResultSelectCountStatement;
            searchStatementModel.SelectStatement = inspectResultSelectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        private SearchStatementModel PopPrepareSearchStatement(GridCommand command, InspectResultSearchModel searchModel)
        {
            string whereStatement = string.Empty;
            IList<RejectDetail> listRejectDetail = new List<RejectDetail>();
            listRejectDetail = base.genericMgr.FindAll<RejectDetail>("select r from RejectDetail as r where r.RejectNo=?", searchModel.RejectNo);
            string strInspResultId = string.Empty;
            for (int i = 0; i < listRejectDetail.Count; i++)
            {
                strInspResultId += "'" + listRejectDetail[i].InspectResultId + "',";
            }
            whereStatement = " where i.JudgeResult=" + (int)com.Sconit.CodeMaster.JudgeResult.Rejected + " and JudgeQty > HandleQty";
            if (strInspResultId != string.Empty)
            {
                strInspResultId = strInspResultId.Substring(0, strInspResultId.Length - 1);
                whereStatement += " and Id not in (" + strInspResultId + ")";
            }
            else
            {
                whereStatement = " where i.JudgeQty > i.HandleQty and i.JudgeResult= " + (int)com.Sconit.CodeMaster.JudgeResult.Rejected;
            }

            if (searchModel.InspectType == (int)com.Sconit.CodeMaster.InspectType.Barcode)
            {
                whereStatement += " and i.HuId is not null";
            }
            else
            {
                whereStatement += " and i.HuId is null";
            }

            return InspectResultPrepareSearchStatement(command, searchModel, whereStatement);
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
            return InspectResultPrepareSearchStatement(command, searchModel, whereStatement);
            //IList<object> param = new List<object>();

            //HqlStatementHelper.AddLikeStatement("InspectNo", searchModel.InspectNo, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            //HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "i", ref whereStatement, param);
            //HqlStatementHelper.AddLikeStatement("IpNo", searchModel.IpNo, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            //HqlStatementHelper.AddLikeStatement("ReceiptNo", searchModel.ReceiptNo, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            //HqlStatementHelper.AddLikeStatement("WMSNo", searchModel.WMSNo, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            //HqlStatementHelper.AddEqStatement("CreateUserName", searchModel.CreateUserName,"i", ref whereStatement, param);
            //HqlStatementHelper.AddEqStatement("CurrentLocation", searchModel.CurrentLocation, "i", ref whereStatement, param);

            //if (searchModel.StartDate != null && searchModel.EndDate != null)
            //{
            //    HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.StartDate, searchModel.EndDate, "i", ref whereStatement, param);

            //}
            //else if (searchModel.StartDate != null && searchModel.EndDate == null)
            //{
            //    HqlStatementHelper.AddGeStatement("CreateDate", searchModel.StartDate, "i", ref whereStatement, param);
            //}
            //else if (searchModel.StartDate == null && searchModel.EndDate != null)
            //{
            //    HqlStatementHelper.AddLeStatement("CreateDate", searchModel.EndDate, "i", ref whereStatement, param);
            //}

            //if (command.SortDescriptors.Count > 0)
            //{
            //    if (command.SortDescriptors[0].Member == "DefectDescription")
            //    {
            //        command.SortDescriptors[0].Member = "Defect";
            //    }
            //}
            //string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            //SearchStatementModel searchStatementModel = new SearchStatementModel();
            //searchStatementModel.SelectCountStatement = inspectResultSelectCountStatement;
            //searchStatementModel.SelectStatement = inspectResultSelectStatement;
            //searchStatementModel.WhereStatement = whereStatement;
            //searchStatementModel.SortingStatement = sortingStatement;
            //searchStatementModel.Parameters = param.ToArray<object>();

            //return searchStatementModel;
        }

        private SearchStatementModel RejectDetailPrepareSearchStatement(GridCommand command, RejectDetailSearchModel searchModel)
        {
            string whereStatement = " where 1=1 ";
            if (searchModel.Status != null || searchModel.HandleResult != null)
            {
                whereStatement += " and exists (select 1 from RejectMaster as rm where  rm.RejectNo= r.RejectNo ";
            }

            if (searchModel.Status != null)
            {
                whereStatement += " and rm.Status='" + searchModel.Status + "'";
            }
            if (searchModel.HandleResult != null)
            {
                whereStatement += " and rm.HandleResult='" + searchModel.HandleResult + "'";
            }

            if (searchModel.Status != null || searchModel.HandleResult != null)
            {
                whereStatement += ")";
            }

            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("RejectNo", searchModel.RejectNo, HqlStatementHelper.LikeMatchMode.Start, "r", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "r", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("CurrentLocation", searchModel.CurrentLocation, "r", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("InspectNo", searchModel.InspectNo, "r", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("IpNo", searchModel.IpNo, "r", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("ReceiptNo", searchModel.ReceiptNo, "r", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("WMSNo", searchModel.WMSNo, "r", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("JudgeUserName", searchModel.JudgeUserName, "r", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("CreateUserName", searchModel.CreateUserName, "r", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("WMSNo", searchModel.WMSNo, "r", ref whereStatement, param);
            if (searchModel.IsContainHu != null && searchModel.IsContainHu.Value)
            {
                whereStatement += " and r.HuId is not null";
            }
            else
            {
                whereStatement += " and r.HuId is  null";
            }

            if (searchModel.StartDate != null && searchModel.EndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.StartDate, searchModel.EndDate, "r", ref whereStatement, param);

            }
            else if (searchModel.StartDate != null && searchModel.EndDate == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.StartDate, "r", ref whereStatement, param);
            }
            else if (searchModel.StartDate == null && searchModel.EndDate != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.EndDate, "r", ref whereStatement, param);
            }

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = "select count(*) from RejectDetail as r";
            searchStatementModel.SelectStatement = "select r from RejectDetail as r";
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }
        #endregion
    }
}