using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using com.Sconit.CodeMaster;
using com.Sconit.Entity;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.PRD;
using com.Sconit.Service;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels.PRD;
using com.Sconit.Web.Util;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Web.Controllers.PRD
{
    public class StandardWorkingCalendarController : WebAppBaseController
    {
        /// <summary>
        /// hql to get count of the StandardWorkingCalendar
        /// </summary>
        private static string selectCountStatement = "select count(*) from StandardWorkingCalendar as swc";

        /// <summary>
        /// hql to get all of the StandardWorkingCalendar
        /// </summary>
        private static string selectStatement = "select swc from StandardWorkingCalendar as swc";

        /// <summary>
        /// hql to get count of the StandardWorkingCalendar by StandardWorkingCalendar's Region
        /// </summary>
        private static string duiplicateVerifyCountStatement = @"select count(*) from StandardWorkingCalendar as swc where swc.Region = ? and swc.DayOfWeek = ? ";
        private static string duiplicateVerifyStatement = @"select swc from StandardWorkingCalendar as swc where swc.Region = ? and swc.DayOfWeek = ? ";

        #region StandardWorkingCalendar
        /// <summary>
        /// Index action for StandardWorkingCalendar controller
        /// </summary>
        /// <returns>Index view</returns>
        [SconitAuthorize(Permissions = "Url_StandardWorkingCalendar_View")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// List action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">StandardWorkingCalendar Search model</param>
        /// <returns>return the result view</returns>
        [GridAction]
        [SconitAuthorize(Permissions = "Url_StandardWorkingCalendar_View")]
        public ActionResult List(GridCommand command, StandardWorkingCalendarSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            ViewBag.SearchShift = searchModel.SearchShift;
            ViewBag.SearchRegion = searchModel.SearchRegion;
            IList<CodeDetail> workingCalendarTypeList = systemMgr.GetCodeDetails(CodeMaster.CodeMaster.WorkingCalendarType);
            ViewData["WorkingCalendarType"] = base.Transfer2DropDownList(CodeMaster.CodeMaster.WorkingCalendarType, workingCalendarTypeList);

            // 判断是否有编辑生产线标准工作日历的权限
            var user = SecurityContextHolder.Get();
            var q = user.UrlPermissions.Where(p => p.Contains("Url_StandardWorkingCalendar_Edit")).ToList();
            ViewBag.NoEditPermission = q == null || q.Count() == 0;

            return View();
        }

        /// <summary>
        ///  AjaxList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">StandardWorkingCalendar Search Model</param>
        /// <returns>return the result action</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_StandardWorkingCalendar_View")]
        public ActionResult _AjaxList(GridCommand command, StandardWorkingCalendarSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<StandardWorkingCalendar>(searchStatementModel, command));
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <returns>New view</returns>
        [SconitAuthorize(Permissions = "Url_StandardWorkingCalendar_Edit")]
        public ActionResult New()
        {
            return View();
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <param name="standardWorkingCalendar">StandardWorkingCalendar Model</param>
        /// <returns>return the result view</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_StandardWorkingCalendar_Edit")]
        public ActionResult New(StandardWorkingCalendar standardWorkingCalendar)
        {
            if (ModelState.IsValid)
            {
                if (base.genericMgr.FindAll<long>(duiplicateVerifyCountStatement, new object[] { standardWorkingCalendar.Region, standardWorkingCalendar.DayOfWeek })[0] > 0)
                {
                    this.FillCodeDetailDescription(standardWorkingCalendar);
                    SaveErrorMessage(Resources.PRD.StandardWorkingCalendar.Errors_Existing_DayOfWeekInTheRegion, standardWorkingCalendar.Region, standardWorkingCalendar.DayOfWeekDescription);
                }
                else
                {
                    standardWorkingCalendar.RegionName = base.genericMgr.FindById<Party>(standardWorkingCalendar.Region).Name;
                    standardWorkingCalendar.Category = WorkingCalendarCategory.Region;
                    base.genericMgr.Create(standardWorkingCalendar);
                    SaveSuccessMessage(Resources.PRD.StandardWorkingCalendar.StandardWorkingCalendar_Added);
                    return RedirectToAction("List/");
                }
            }

            return View(standardWorkingCalendar);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_StandardWorkingCalendar_Edit")]
        public ActionResult _Update(int id, StandardWorkingCalendar standardWorkingCalendar, StandardWorkingCalendarSearchModel searchModel, GridCommand command)
        {
            try
            {
                ModelState.Remove("Region");
                ModelState.Remove("DayOfWeek");
                var newWorkingCalendar = base.genericMgr.FindById<StandardWorkingCalendar>(id);
                newWorkingCalendar.Shift = standardWorkingCalendar.Shift;
                newWorkingCalendar.Type = standardWorkingCalendar.Type;
                newWorkingCalendar.Category = WorkingCalendarCategory.Region;
                base.genericMgr.Update(newWorkingCalendar);
                //SaveSuccessMessage(Resources.PRD.StandardWorkingCalendar.StandardWorkingCalendar_Updated);
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }

            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<StandardWorkingCalendar>(searchStatementModel, command));
        }

        /// <summary>
        /// Edit view
        /// </summary>
        /// <param name="id">StandardWorkingCalendar id for edit</param>
        /// <returns>return the result view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_StandardWorkingCalendar_Edit")]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                StandardWorkingCalendar standardWorkingCalendar = base.genericMgr.FindById<StandardWorkingCalendar>(id);
                return View(standardWorkingCalendar);
            }
        }

        /// <summary>
        /// Edit view
        /// </summary>
        /// <param name="standardWorkingCalendar">StandardWorkingCalendar Model</param>
        /// <returns>return the partial view</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_StandardWorkingCalendar_Edit")]
        public ActionResult Edit(StandardWorkingCalendar standardWorkingCalendar)
        {
            if (ModelState.IsValid)
            {
                var items = base.genericMgr.FindAll<StandardWorkingCalendar>(duiplicateVerifyStatement, new object[] { standardWorkingCalendar.Region, standardWorkingCalendar.DayOfWeek });
                var sameItem = items.FirstOrDefault(c => c.Id != standardWorkingCalendar.Id);
                if (sameItem != null)
                {
                    this.FillCodeDetailDescription(standardWorkingCalendar);
                    SaveErrorMessage(Resources.PRD.StandardWorkingCalendar.Errors_Existing_DayOfWeekInTheRegion, standardWorkingCalendar.Region, standardWorkingCalendar.DayOfWeekDescription);
                }
                else
                {
                    standardWorkingCalendar.RegionName = base.genericMgr.FindById<Party>(standardWorkingCalendar.Region).Name;
                    base.genericMgr.Update(standardWorkingCalendar);
                    SaveSuccessMessage(Resources.PRD.StandardWorkingCalendar.StandardWorkingCalendar_Updated);
                    return RedirectToAction("List/");
                }
            }

            return View(standardWorkingCalendar);
        }

        /// <summary>
        /// Delete action
        /// </summary>
        /// <param name="id">StandardWorkingCalendar id for delete</param>
        /// <returns>return to List action</returns>
        [SconitAuthorize(Permissions = "Url_StandardWorkingCalendar_Delete")]
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<StandardWorkingCalendar>(id);
                SaveSuccessMessage(Resources.PRD.StandardWorkingCalendar.StandardWorkingCalendar_Deleted);
                return RedirectToAction("List/");
            }
        }

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">StandardWorkingCalendar Search Model</param>
        /// <returns>return StandardWorkingCalendar search model</returns>
        private SearchStatementModel PrepareSearchStatement(GridCommand command, StandardWorkingCalendarSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            HqlStatementHelper.AddEqStatement("Category", (int)WorkingCalendarCategory.Region, "swc", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Region", searchModel.SearchRegion, "swc", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Shift", searchModel.SearchShift, "swc", ref whereStatement, param);

            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "DayOfWeekDescription")
                {
                    command.SortDescriptors[0].Member = "DayOfWeek";
                }
                else if (command.SortDescriptors[0].Member == "TypeDescription")
                {
                    command.SortDescriptors[0].Member = "Type";
                }
            }

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            if (string.IsNullOrEmpty(sortingStatement))
            {
                sortingStatement = " order by swc.Region asc, swc.DayOfWeek asc ";
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