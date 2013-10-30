namespace com.Sconit.Web.Controllers.ISS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Models.SearchModels.ISS;
    using com.Sconit.Web.Controllers.ACC;
    using com.Sconit.Entity.ISS;
    using com.Sconit.Service;

    public class IssueAddressController : WebAppBaseController
    {
        /// <summary>
        /// 
        /// </summary>
        private static string selectCountStatement = "select count(*) from IssueAddress as ia left join ia.ParentIssueAddress pia ";

        /// <summary>
        /// 
        /// </summary>
        private static string selectStatement = "select ia from IssueAddress as ia left join ia.ParentIssueAddress pia ";

        /// <summary>
        /// 
        /// </summary>
        private static string codeDuiplicateVerifyStatement = @"select count(*) from IssueAddress as ia where ia.Code = ?";

        //
        // GET: /IssueAddress/

        [SconitAuthorize(Permissions = "Url_IssueAddress_View")]
        public ActionResult Index()
        {
            return View();
        }


        [GridAction]
        [SconitAuthorize(Permissions = "Url_IssueAddress_View")]
        public ActionResult List(GridCommand command, IssueAddressSearchModel searchModel)
        {
            TempData["IssueAddressSearchModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_IssueAddress_View")]
        public ActionResult _AjaxList(GridCommand command, IssueAddressSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<IssueAddress>(searchStatementModel, command));
        }

        [SconitAuthorize(Permissions = "Url_IssueAddress_Edit")]
        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_IssueAddress_Edit")]
        public ActionResult New(IssueAddress issueAddress)
        {
            if (!string.IsNullOrWhiteSpace(issueAddress.ParentIssueAddressCode))
            {
                ViewBag.ParentIssueAddressCode = issueAddress.ParentIssueAddressCode;
                var parent = base.genericMgr.FindById<IssueAddress>(issueAddress.ParentIssueAddressCode);
                IssueAddress parentIssueAddress = new IssueAddress();//base.genericMgr.FindById<IssueType>(issueTypeTo.IssueTypeCode);
                parentIssueAddress.Code = issueAddress.ParentIssueAddressCode;
                issueAddress.ParentIssueAddress = parentIssueAddress;
                ModelState.Remove("ParentIssueAddress");
            }
            if (ModelState.IsValid)
            {
                //判断用户名不能重复
                if (base.genericMgr.FindAll<long>(codeDuiplicateVerifyStatement, new object[] { issueAddress.Code })[0] > 0)
                {
                    base.SaveErrorMessage(Resources.ISS.IssueAddress.Errors_Existing_IssueAddress, issueAddress.Code);
                }
                else
                {
                    base.genericMgr.Create(issueAddress);
                    SaveSuccessMessage(Resources.ISS.IssueAddress.IssueAddress_Added);
                    return RedirectToAction("Edit/" + issueAddress.Code);
                }
            }
            return View(issueAddress);
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_IssueAddress_Edit")]
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            else
            {
                IssueAddress issueAddress = base.genericMgr.FindById<IssueAddress>(id);
                return View(issueAddress);
            }
        }

        /// <summary>
        /// Delete action
        /// </summary>
        /// <param name="id">IssueAddress id for delete</param>
        /// <returns>return to List action</returns>
        [SconitAuthorize(Permissions = "Url_IssueAddress_Delete")]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<IssueAddress>(id);
                SaveSuccessMessage(Resources.ISS.IssueAddress.IssueAddress_Deleted);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_IssueAddress_Edit")]
        public ActionResult Edit(IssueAddress issueAddress)
        {
            if (!string.IsNullOrWhiteSpace(issueAddress.ParentIssueAddressCode))
            {
                ViewBag.ParentIssueAddressId = issueAddress.ParentIssueAddressCode;
                var parent = base.genericMgr.FindById<IssueAddress>(issueAddress.ParentIssueAddressCode);
                if (parent.ParentIssueAddressCode == issueAddress.Code)
                {
                    SaveErrorMessage("地点{0}已经是地点{1}的上级地点，请确认。", issueAddress.Code, issueAddress.ParentIssueAddressCode);
                    return View(issueAddress);
                }
                IssueAddress parentIssueAddress = new IssueAddress();//base.genericMgr.FindById<IssueType>(issueTypeTo.IssueTypeCode);
                parentIssueAddress.Code = issueAddress.ParentIssueAddressCode;
                issueAddress.ParentIssueAddress = parentIssueAddress;
                ModelState.Remove("ParentIssueAddress");
            }
            if (ModelState.IsValid)
            {
                base.genericMgr.Update(issueAddress);
                SaveSuccessMessage(Resources.ISS.IssueAddress.IssueAddress_Updated);
            }

            return View(issueAddress);
        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, IssueAddressSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            if (!string.IsNullOrWhiteSpace(searchModel.ParentIssueAddressCode))
            {
                HqlStatementHelper.AddEqStatement("Code", searchModel.ParentIssueAddressCode, "pia", ref whereStatement, param);
            }
            HqlStatementHelper.AddLikeStatement("Description", searchModel.Description, HqlStatementHelper.LikeMatchMode.Start, "ia", ref whereStatement, param);

            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "Code")
                {
                    command.SortDescriptors[0].Member = "ia.Code";
                }
                if (command.SortDescriptors[0].Member == "Description")
                {
                    command.SortDescriptors[0].Member = "ia.Description";
                }
                if (command.SortDescriptors[0].Member == "Sequence")
                {
                    command.SortDescriptors[0].Member = "ia.Sequence";
                }
                if (command.SortDescriptors[0].Member == "ParentIssueAddressCode")
                {
                    command.SortDescriptors[0].Member = "pia.Code";
                }
                if (command.SortDescriptors[0].Member == "ParentIssueAddressDescription")
                {
                    command.SortDescriptors[0].Member = "pia.Description";
                }
            }

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }
    }
}
