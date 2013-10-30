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

    public class ConcessionOrderController : WebAppBaseController
    {
        //
        // GET: /InspectionOrder/
        public IInspectMgr inspectMgr { get; set; }
        public ILocationDetailMgr locationDetailMgr { get; set; }

        private static string selectStatement = "select c from ConcessionMaster as c";

        private static string selectCountStatement = "select count(*) from ConcessionMaster as c";

        #region view
        [SconitAuthorize(Permissions = "Url_ConcessionOrder_View")]
        public ActionResult Index()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_ConcessionOrder_View")]
        [GridAction]
        public ActionResult List(GridCommand command, ConcessionMasterSearchModel searchModel)
        {
            this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [SconitAuthorize(Permissions = "Url_ConcessionOrder_View")]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxList(GridCommand command, ConcessionMasterSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = ConcessionMasterPrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<ConcessionMaster>(searchStatementModel, command));
        }

        #endregion

        #region new
        [GridAction]
        [SconitAuthorize(Permissions = "Url_ConcessionOrder_New")]
        public ActionResult New()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_ConcessionOrder_New")]
        public ActionResult _RejectDetailList(string rejectNo)
        {
            string hql = "select r from RejectDetail r where r.RejectNo = ?";
            IList<RejectDetail> rejectDetailList = base.genericMgr.FindAll<RejectDetail>(hql, rejectNo);
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
            return PartialView(rejectDetailList);
        }

        [SconitAuthorize(Permissions = "Url_ConcessionOrder_View")]
        public ActionResult _ConcessionDetailList(string concessionNo)
        {
            IList<ConcessionDetail> concessionDetailList = new List<ConcessionDetail>();

            if (!string.IsNullOrEmpty(concessionNo))
            {
                string hql = "from ConcessionDetail where ConcessionNo = ?";
                concessionDetailList = base.genericMgr.FindAll<ConcessionDetail>(hql, concessionNo);
            }


            return PartialView(concessionDetailList);
        }

        #endregion

        #region action

        [SconitAuthorize(Permissions = "Url_ConcessionOrder_New")]
        public string Create(ConcessionMaster concessionMaster)
        {
            try
            {
                inspectMgr.CreateConcessionMaster(concessionMaster);
                return concessionMaster.ConcessionNo;
            }
            catch (BusinessException ex)
            {
                Response.TrySkipIisCustomErrors = true;
                Response.StatusCode = 500;
                Response.Write(ex.GetMessages()[0].GetMessageString());
                return string.Empty;
            }
        }

        [SconitAuthorize(Permissions = "Url_ConcessionOrder_New")]
        public ActionResult Edit(string id)
        {
            ConcessionMaster ConcessionMaster = base.genericMgr.FindById<ConcessionMaster>(id);
            ConcessionMaster.ConcessionStatusDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.ConcessionStatus, ((int)ConcessionMaster.Status).ToString());
            return View(ConcessionMaster);
        }

        [SconitAuthorize(Permissions = "Url_ConcessionOrder_New")]
        public ActionResult Delete(string id)
        {
            try
            {
                inspectMgr.DeleteConcessionMaster(id);
                SaveSuccessMessage(Resources.INP.ConcessionMaster.ConcessionMaster_Deleted, id);
                return RedirectToAction("List");
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
                return RedirectToAction("Edit/" + id);
            }
        }

        [SconitAuthorize(Permissions = "Url_ConcessionOrder_Submit")]
        public ActionResult Submit(string id)
        {
            try
            {
                inspectMgr.ReleaseConcessionMaster(id);
                SaveSuccessMessage(Resources.INP.ConcessionMaster.ConcessionMaster_Submited, id);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            return RedirectToAction("Edit/" + id);
        }

        [SconitAuthorize(Permissions = "Url_ConcessionOrder_Commossion")]
        public ActionResult Commossion(string id)
        {
            try
            {
                ConcessionMaster concessionMaster = base.genericMgr.FindById<ConcessionMaster>(id);
                concessionMaster.ConcessionDetails = base.genericMgr.FindAll<ConcessionDetail>("from ConcessionDetail where ConcessionNo = ?", id);

                locationDetailMgr.ConcessionToUse(concessionMaster);
                SaveSuccessMessage(Resources.INP.ConcessionMaster.ConcessionMaster_ToUsed, id);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            return RedirectToAction("Edit/" + id);
        }

        [SconitAuthorize(Permissions = "Url_ConcessionOrder_Close")]
        public ActionResult Close(string id)
        {
            try
            {
                inspectMgr.CloseConcessionMaster(id);
                SaveSuccessMessage(Resources.INP.ConcessionMaster.ConcessionMaster_Closed, id);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            return RedirectToAction("Edit/" + id);
        }

        #endregion

        #region private method
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
    }
}