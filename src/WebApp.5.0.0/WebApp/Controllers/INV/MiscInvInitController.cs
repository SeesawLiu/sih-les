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
using System.Text; 
using System;
using System.Web.Helpers;
using com.Sconit.Entity;

namespace com.Sconit.Web.Controllers.INV
{
    public class MiscInvInitController : WebAppBaseController
    {
        private static string selectCountStatement = "select count(*) from MiscOrderMaster as m";

        private static string selectStatement = "select m from MiscOrderMaster as m";

        private static string selectDetailCountStatement = "select count(*) from MiscOrderDetail as m ";
        private static string selectDetailStatement = "select m from MiscOrderDetail as m ";

        //public IGenericMgr genericMgr { get; set; }
        public IMiscOrderMgr miscOrderMgr { get; set; }

        #region public method

        #region view
        [SconitAuthorize(Permissions = "Url_MiscInvInit_View")]
        public ActionResult Index()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_MiscInvInit_View")]
        public ActionResult List(GridCommand GridCommand, OutMiscOrderSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(GridCommand, searchModel);
            if (this.CheckSearchModelIsNull(searchCacheModel.SearchObject))
            {
            }
            else
            {
                SaveWarningMessage(Resources.ErrorMessage.Errors_NoConditions);
            }
            ViewBag.PageSize = base.ProcessPageSize(GridCommand.PageSize);
            return View();

        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_MiscInvInit_View")]
        public ActionResult _AjaxList(GridCommand command, OutMiscOrderSearchModel searchModel)
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
        [SconitAuthorize(Permissions = "Url_MiscInvInit_New")]
        public ActionResult New()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [SconitAuthorize(Permissions = "Url_MiscInvInit_New")]
        public ActionResult CreateMiscOrder([Bind(Prefix =
            "inserted")]IEnumerable<MiscOrderDetail> insertedMiscOrderDetails,
            [Bind(Prefix = "updated")]IEnumerable<MiscOrderDetail> updatedMiscOrderDetails,
            [Bind(Prefix = "deleted")]IEnumerable<MiscOrderDetail> deletedMiscOrderDetails,
            MiscOrderMaster miscOrderMaster)
        {
            try
            {
                #region MiscOrderDetailList
                IList<MiscOrderDetail> newMiscOrderDetailList = new List<MiscOrderDetail>();
                IList<MiscOrderDetail> updateMiscOrderDetailList = new List<MiscOrderDetail>();
                if (updatedMiscOrderDetails != null)
                {
                    foreach (var miscOrderDetail in updatedMiscOrderDetails)
                    {
                        if (CheckMiscOrderDetail(miscOrderMaster, miscOrderDetail))
                        {
                            if (miscOrderDetail.Id == 0)
                            {
                                newMiscOrderDetailList.Add(miscOrderDetail);
                            }
                            else
                            {
                                updateMiscOrderDetailList.Add(miscOrderDetail);
                            }
                        }
                    }
                }
                if (insertedMiscOrderDetails != null)
                {
                    foreach (var miscOrderDetail in insertedMiscOrderDetails)
                    {
                        if (CheckMiscOrderDetail(miscOrderMaster, miscOrderDetail))
                        {
                            newMiscOrderDetailList.Add(miscOrderDetail);
                        }
                    }
                }

                #endregion

                #region miscOrderMaster

                //MiscOrderMoveType miscOrderMoveType = genericMgr.FindAll<MiscOrderMoveType>("from MiscOrderMoveType as m where m.MoveType=?", new object[] { miscOrderMaster.MoveType })[0];
                miscOrderMaster.Type = com.Sconit.CodeMaster.MiscOrderType.GR;
                miscOrderMaster.MoveType = "999";
                miscOrderMaster.CancelMoveType = "992";
                miscOrderMaster.QualityType = com.Sconit.CodeMaster.QualityType.Qualified;
                #endregion

                miscOrderMgr.CreateOrUpdateMiscOrderAndDetails(miscOrderMaster, newMiscOrderDetailList, updateMiscOrderDetailList, (IList<MiscOrderDetail>)deletedMiscOrderDetails);
                SaveSuccessMessage("创建成功。");
                return View("Edit", miscOrderMaster);
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                return Json(null);
            }
        }

        #endregion

        #region MiscOrderDetail

        [SconitAuthorize(Permissions = "Url_MiscInvInit_View")]
        public ActionResult _MiscOrderDetail(string miscOrderNo)
        {
            if (!string.IsNullOrEmpty(miscOrderNo))
            {
                MiscOrderMaster miscOrder = genericMgr.FindById<MiscOrderMaster>(miscOrderNo);
                ViewBag.Status = miscOrder.Status;
                ViewBag.IsCreate = miscOrder.Status == com.Sconit.CodeMaster.MiscOrderStatus.Create ? true : false;
                ViewBag.miscOrderNo = miscOrderNo;
            }
            else
            {
                ViewBag.IsCreate = true;
                ViewBag.miscOrderNo = miscOrderNo;
            }
            ViewBag.PageSize = 100;
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_MiscInvInit_View")]
        public ActionResult _SelectMiscOrderDetail(string miscOrderNo, GridCommand command)
        {
            IList<MiscOrderDetail> miscOrderDetailList = new List<MiscOrderDetail>();
            if (!string.IsNullOrWhiteSpace(miscOrderNo))
            {
                SearchStatementModel searchStatementModel = PrepareDetailSearchStatement(command, miscOrderNo);
                return PartialView(GetAjaxPageData<MiscOrderDetail>(searchStatementModel, command));
            }
            //for (int i = 0; i < 15; i++)
            //{
            //    miscOrderDetailList.Add(new MiscOrderDetail());
            //}
            //return PartialView(new GridModel(miscOrderDetailList));
            return PartialView(new GridModel(miscOrderDetailList));
        }

        public ActionResult _WebOrderDetail(string Code)
        {
            if (!string.IsNullOrEmpty(Code))
            {
                WebOrderDetail webOrderDetail = new WebOrderDetail();
                Item item = genericMgr.FindById<Item>(Code);
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


        private SearchStatementModel PrepareDetailSearchStatement(GridCommand command, string miscOrderNo)
        {
            string whereStatement = "";
            IList<object> param = new List<object>();
            HqlStatementHelper.AddEqStatement("MiscOrderNo", miscOrderNo, "m", ref  whereStatement, param);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectDetailCountStatement;
            searchStatementModel.SelectStatement = selectDetailStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.Parameters = param.ToArray<object>();
            return searchStatementModel;
        }

        #endregion

        #region  Edit
        [SconitAuthorize(Permissions = "Url_MiscInvInit_View")]
        public ActionResult Edit(string id,string urlId)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                ViewBag.UrlId = urlId;
                MiscOrderMaster miscOrderMaster = this.genericMgr.FindById<MiscOrderMaster>(id);
                return View(miscOrderMaster);
            }
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_MiscInvInit_New")]
        public ActionResult EditMiscOrder([Bind(Prefix =
            "inserted")]IEnumerable<MiscOrderDetail> insertedMiscOrderDetails,
            [Bind(Prefix = "updated")]IEnumerable<MiscOrderDetail> updatedMiscOrderDetails,
            [Bind(Prefix = "deleted")]IEnumerable<MiscOrderDetail> deletedMiscOrderDetails,
           MiscOrderMaster miscOrderMaster)
        {
            try
            {
                #region master 只能改库位
                MiscOrderMaster oldMiscOrderMaster = this.genericMgr.FindById<MiscOrderMaster>(miscOrderMaster.MiscOrderNo);

                oldMiscOrderMaster.Location = miscOrderMaster.Location;
                #endregion

                #region Detail
                IList<MiscOrderDetail> newMiscOrderDetailList = new List<MiscOrderDetail>();
                IList<MiscOrderDetail> updateMiscOrderDetailList = new List<MiscOrderDetail>();

                if (insertedMiscOrderDetails != null)
                {
                    foreach (var miscOrderDetail in insertedMiscOrderDetails)
                    {
                        if (CheckMiscOrderDetail(miscOrderMaster, miscOrderDetail))
                        {
                            newMiscOrderDetailList.Add(miscOrderDetail);
                        }
                    }
                }
                if (updatedMiscOrderDetails != null)
                {
                    foreach (var miscOrderDetail in updatedMiscOrderDetails)
                    {
                        if (CheckMiscOrderDetail(miscOrderMaster, miscOrderDetail))
                        {
                            if (miscOrderDetail.Id == 0)
                            {
                                newMiscOrderDetailList.Add(miscOrderDetail);
                            }
                            else
                            {
                                updateMiscOrderDetailList.Add(miscOrderDetail);
                            }
                        }
                    }
                }
                #endregion
                miscOrderMgr.CreateOrUpdateMiscOrderAndDetails(oldMiscOrderMaster, newMiscOrderDetailList, updateMiscOrderDetailList, (IList<MiscOrderDetail>)deletedMiscOrderDetails);
                SaveSuccessMessage("保存成功。");
                return View("Edit", miscOrderMaster);
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                return Json(null);
            }
        }

        [SconitAuthorize(Permissions = "Url_MiscInvInit_New")]
        public ActionResult btnClose(string id)
        {
            try
            {
                MiscOrderMaster miscOrderMaster = genericMgr.FindById<MiscOrderMaster>(id);
                IList<MiscOrderDetail> miscOrderDetailList = genericMgr.FindAll<MiscOrderDetail>("from MiscOrderDetail as m where m.MiscOrderNo=?", miscOrderMaster.MiscOrderNo);
                if (miscOrderDetailList.Count < 1)
                {
                    SaveErrorMessage("明细为空，不能执行确认");
                }
                else
                {
                    foreach (var miscOrderDetail in miscOrderDetailList)
                    {
                        CheckMiscOrderDetail(miscOrderMaster, miscOrderDetail);
                    }
                    miscOrderMgr.CloseMiscOrder(miscOrderMaster);
                    SaveSuccessMessage("确认成功");
                }
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());

            }
            return RedirectToAction("Edit/" + id);
        }


        [SconitAuthorize(Permissions = "Url_MiscInvInit_New")]
        public ActionResult btnDelete(string id)
        {
            try
            {
                MiscOrderMaster MiscOrderMaster = genericMgr.FindById<MiscOrderMaster>(id);
                miscOrderMgr.DeleteMiscOrder(MiscOrderMaster);
                SaveSuccessMessage("删除成功");
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            return RedirectToAction("List");
        }



        [SconitAuthorize(Permissions = "Url_MiscInvInit_New")]
        public ActionResult btnCancel(string id)
        {
            try
            {
                MiscOrderMaster MiscOrderMaster = genericMgr.FindById<MiscOrderMaster>(id);
                miscOrderMgr.CancelMiscOrder(MiscOrderMaster);
                SaveSuccessMessage("取消成功");
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            return RedirectToAction("Edit/" + id);
        }
        #endregion

        [SconitAuthorize(Permissions = "Url_MiscInvInit_New")]
        public ActionResult ImportInMiscOrderDetail(IEnumerable<HttpPostedFileBase> attachments, string miscOrderNo)
        {
            try
            {
                foreach (var file in attachments)
                {
                    miscOrderMgr.CreateMiscInvInitDetailFromXls(file.InputStream, miscOrderNo);
                }
                SaveSuccessMessage(Resources.Global.ImportSuccess_BatchImportSuccessful);
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

        #region private method
        private bool CheckMiscOrderDetail(MiscOrderMaster miscOrderMaster, MiscOrderDetail miscOrderDetail)
        {
            if (string.IsNullOrEmpty(miscOrderDetail.Location))
            {
                miscOrderDetail.Location = miscOrderMaster.Location;
            }
            if (string.IsNullOrEmpty(miscOrderDetail.Item))
            {
                throw new BusinessException("明细行物料不能为空");
            }
            if (miscOrderDetail.Qty == 0)
            {
                throw new BusinessException("明细行数量不能为空");
            }
            Item item = genericMgr.FindById<Item>(miscOrderDetail.Item);
            miscOrderDetail.ItemDescription = item.Description;
            miscOrderDetail.UnitCount = item.UnitCount;
            miscOrderDetail.Uom = item.Uom;
            miscOrderDetail.BaseUom = item.Uom;
            // miscOrderDetail.MiscOrderNo = miscOrderNo;
            return true;
        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, OutMiscOrderSearchModel searchModel)
        {
            string whereStatement = "where   m.Type = " + (int)com.Sconit.CodeMaster.MiscOrderType.GR ;
            IList<object> param = new List<object>();
            //if (!string.IsNullOrWhiteSpace(searchModel.Item))
            //{
            //    whereStatement += " and  exists ( select 1 from MiscOrderDetail as d where d.MiscOrderNo=m.MiscOrderNo and d.Item=?  )   ";
            //    param.Add(searchModel.Item);
            //}
            SecurityHelper.AddRegionPermissionStatement(ref whereStatement, "m", "Region");

            HqlStatementHelper.AddLikeStatement("MiscOrderNo", searchModel.MiscOrderNo, HqlStatementHelper.LikeMatchMode.Start, "m", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Status", searchModel.Status, "m", ref  whereStatement, param);
            HqlStatementHelper.AddEqStatement("Region", searchModel.Region, "m", ref  whereStatement, param);
            HqlStatementHelper.AddEqStatement("Location", searchModel.Location, "m", ref  whereStatement, param);
            HqlStatementHelper.AddEqStatement("MoveType", "999", "m", ref  whereStatement, param);
            HqlStatementHelper.AddEqStatement("CreateUserName", searchModel.CreateUserName, "m", ref  whereStatement, param);
            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.StartDate, searchModel.EndDate, "m", ref whereStatement, param);
            }
            else if (searchModel.StartDate != null & searchModel.EndDate == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.StartDate, "m", ref whereStatement, param);
            }
            else if (searchModel.StartDate == null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.EndDate, "m", ref whereStatement, param);
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
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();
            return searchStatementModel;
        }
        #endregion
    }
}

