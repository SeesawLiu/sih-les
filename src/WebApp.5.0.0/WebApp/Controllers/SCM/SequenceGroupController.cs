using System.Web;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Web.Controllers.SCM
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using Service;
    using Models;
    using Util;
    using Telerik.Web.Mvc;
    using Models.SearchModels.SCM;
    using Entity.SCM;
    using System;
    using com.Sconit.Entity.PRD;
    using System.Text;
    using com.Sconit.Entity.MD;
    using com.Sconit.Entity;
    using com.Sconit.Entity.ORD;
    using NHibernate.Type;

    /// <summary>
    /// This controller response to control the Routing.
    /// </summary>
    public class SequenceGroupController : WebAppBaseController
    {
        /// <summary>
        /// hql to get count of the RoutingMaster
        /// </summary>
        private static string selectCountStatement = "select count(*) from SequenceGroup  s";

        /// <summary>
        /// hql to get all of the RoutingMaster
        /// </summary>
        private static string selectStatement = "select s from SequenceGroup as s";

        private static string duiplicateVerifyStatement = @"select count(*) from SequenceGroup as s where s.Code = ?";

        /// <summary>
        /// Index action for Routing controller
        /// </summary>
        /// <returns>Index view</returns>
        [SconitAuthorize(Permissions = "Url_SequenceGroup_View")]
        public ActionResult Index()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_SequenceGroup_View")]
        public ActionResult New()
        {
            return View();
        }
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_SequenceGroup_View")]
        public ActionResult New(SequenceGroup sequenceGroup)
        {
            if (ModelState.IsValid)
            {
                if (base.genericMgr.FindAll<long>(duiplicateVerifyStatement, new object[] { sequenceGroup.Code })[0] > 0)
                {
                    SaveErrorMessage(Resources.SCM.SequenceGroup.Errors_ExistingSequenceGroup, sequenceGroup.Code);
                }
                else
                {
                    base.genericMgr.Create(sequenceGroup);
                    SaveSuccessMessage(Resources.SCM.SequenceGroup.SequenceGroup_Added);
                    return RedirectToAction("Edit", new { code = sequenceGroup.Code });
                }
            }
            return View(sequenceGroup);
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_SequenceGroup_View")]
        public ActionResult Edit(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return HttpNotFound();
            }
            else
            {
                ViewBag.HaveEditPrevTraceCode = CurrentUser.Permissions.Where(p => p.PermissionCode == "Url_SequenceGroup_EditPrevTraceCode").Count() > 0;
                SequenceGroup sequenceGroup = base.genericMgr.FindById<SequenceGroup>(code);
                return View(sequenceGroup);
            }
        }

        [SconitAuthorize(Permissions = "Url_SequenceGroup_View")]
        public ActionResult Edit(SequenceGroup sequenceGroup)
        {
            ViewBag.HaveEditPrevTraceCode = CurrentUser.Permissions.Where(p => p.PermissionCode == "Url_SequenceGroup_EditPrevTraceCode").Count() > 0;
            if (ModelState.IsValid)
            {
                bool noenError = true;
                if (!ViewBag.HaveEditPrevTraceCode && !string.IsNullOrWhiteSpace(sequenceGroup.PreviousTraceCode))
                {
                    SaveErrorMessage("您没有修改前个Van号的权限。");
                    noenError = false;
                }
                if (sequenceGroup.IsActive && string.IsNullOrWhiteSpace(sequenceGroup.PreviousTraceCode))
                {
                    SaveErrorMessage("排序组有效的情况下，前面Van号不能为空。");
                     noenError = false;
                }
                IList<OrderSeq> orderSeqs = new List<OrderSeq>();
                if (!string.IsNullOrWhiteSpace(sequenceGroup.PreviousTraceCode))
                {
                     orderSeqs = this.genericMgr.FindAll<OrderSeq>("select o from OrderSeq as o where o.ProductLine=? and o.TraceCode=?", new object[] { sequenceGroup.ProductLine, sequenceGroup.PreviousTraceCode });
                    if (orderSeqs == null || orderSeqs.Count == 0)
                    {
                        SaveErrorMessage(string.Format("生产线{0}前面Van号{1}找不到有效的数据。", sequenceGroup.ProductLine, sequenceGroup.PreviousTraceCode));
                        noenError = false;
                    }
                }
                if (!string.IsNullOrWhiteSpace(sequenceGroup.OpReference))
                {
                    try
                    {
                        sequenceGroup.OpReference.Split('|');
                    }
                    catch (Exception)
                    {
                        SaveErrorMessage(string.Format("工位{0}填写有误，正确的格式为{1}。", sequenceGroup.OpReference, Resources.SCM.SequenceGroup.SequenceGroup_OpRefRemark));
                        noenError = false;
                    }
                }
                if (noenError)
                {
                    var dbSsequenceGroup = base.genericMgr.FindById<SequenceGroup>(sequenceGroup.Code);
                    dbSsequenceGroup.SequenceBatch = sequenceGroup.SequenceBatch;
                    dbSsequenceGroup.OpReference = sequenceGroup.OpReference;
                    dbSsequenceGroup.IsActive = sequenceGroup.IsActive;
                    if (!string.IsNullOrWhiteSpace(sequenceGroup.PreviousTraceCode))
                    {
                        dbSsequenceGroup.PreviousTraceCode = sequenceGroup.PreviousTraceCode;
                        dbSsequenceGroup.PreviousOrderNo = orderSeqs.First().OrderNo;
                        dbSsequenceGroup.PreviousSeq = orderSeqs.First().Sequence;
                        dbSsequenceGroup.PreviousSubSeq = orderSeqs.First().SubSequence;
                    }
                    base.genericMgr.Update(dbSsequenceGroup);
                    SaveSuccessMessage(Resources.SCM.SequenceGroup.SequenceGroup_Updated);
                    return View(dbSsequenceGroup);
                }
            }

            return View(sequenceGroup);
        }

        [SconitAuthorize(Permissions = "Url_SequenceGroup_View")]
        public ActionResult DeleteId(string id)
        {
            try
            {
                base.genericMgr.DeleteById<SequenceGroup>(id);
                SaveSuccessMessage(Resources.SCM.SequenceGroup.SequenceGroup_Deleted);
            }
            catch (System.Exception ex)
            {
                SaveErrorMessage(ex.Message);
            }
            return RedirectToAction("List");

        }

        /// <summary>
        /// List acion
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">RoutingMaster Search Model</param>
        /// <returns>return to the result action</returns>
        [GridAction]
        [SconitAuthorize(Permissions = "Url_SequenceGroup_View")]
        public ActionResult List(GridCommand command, SequenceGroupSearchModel searchModel)
        {

            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        /// <summary>
        /// AjaxList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">RoutingMaster Search Model</param>
        /// <returns>return to the result action</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_SequenceGroup_View")]
        public ActionResult _AjaxList(GridCommand command, SequenceGroupSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<SequenceGroup>(searchStatementModel, command));
        }

        [SconitAuthorize(Permissions = "Url_SequenceGroup_View")]
        public void Export(SequenceGroupSearchModel searchModel)
        {
            var statement = @"select o from SequenceGroup as o where 1=1 ";
            if (!string.IsNullOrWhiteSpace(searchModel.ProdLine))
            {
                statement += " and o.ProductLine ='" + searchModel.ProdLine + "'";
            }

            if (!string.IsNullOrWhiteSpace(searchModel.Code))
            {
                statement += " and o.Code like '" + searchModel.Code + "%'";
            }

            string maxRows = this.systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.ExportMaxRows);
            var items = base.genericMgr.FindAll<SequenceGroup>(statement, 0, int.Parse(maxRows));
            foreach (SequenceGroup sg in items)
            {
                //this.SequenceGroupMgr.GetDeliveryDateAndCount(sg);
            }

            FillCodeDetailDescription(items);
            ExportToXLS<SequenceGroup>("SequenceGroup", "xls", items);
        }

        [SconitAuthorize(Permissions = "Url_SequenceGroup_View")]
        public ActionResult Import(IEnumerable<HttpPostedFileBase> attachments)
        {
            try
            {
                foreach (var file in attachments)
                {
                    //SequenceGroupMgr.ImportSequenceGroup(file.InputStream, CodeMaster.SequenceGroupType.OutsideFactory);
                }
                SaveSuccessMessage(Resources.Global.ImportSuccess_BatchImportSuccessful);
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(Resources.ErrorMessage.Errors_Import_Failed, ex.Message);
            }

            return Content(string.Empty);
        }

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">RoutingMaster Search Model</param>
        /// <returns>Search Statement</returns>
        private SearchStatementModel PrepareSearchStatement(GridCommand command, SequenceGroupSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            HqlStatementHelper.AddEqStatement("ProductLine", searchModel.ProdLine, "s", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("IsActive", searchModel.IsActive, "s", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Code", searchModel.Code, HqlStatementHelper.LikeMatchMode.Start, "s", ref whereStatement, param);

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

