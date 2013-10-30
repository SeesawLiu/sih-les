using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using com.Sconit.Entity;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.PRD;
using com.Sconit.Entity.SYS;
using com.Sconit.Service;
using com.Sconit.Web.Models;
using com.Sconit.Web.Util;
using com.Sconit.Web.Models.SearchModels.PRD;

namespace com.Sconit.Web.Controllers.PRD
{
    public class ProdLineWorkingCalendarController : WebAppBaseController
    {
        #region Properties
        public IWorkingCalendarMgr WorkingCalendarMgr { get; set; }
        #endregion

        /// <summary>
        /// hql to get count of the StandardWorkingCalendar
        /// </summary>
        private static string selectCountStatement = "select count(*) from WorkingCalendar as wc";

        /// <summary>
        /// hql to get all of the StandardWorkingCalendar
        /// </summary>
        private static string selectStatement = "select wc from WorkingCalendar as wc";

        #region WorkingCalendar
        /// <summary>
        /// Index action for WorkingCalendar controller
        /// </summary>
        /// <returns>Index view</returns>
        [SconitAuthorize(Permissions = "Url_ProdLineWorkingCalendar_View")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// List action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">WorkingCalendar Search model</param>
        /// <returns>return the result view</returns>
        [GridAction]
        [SconitAuthorize(Permissions = "Url_ProdLineWorkingCalendar_View")]
        public ActionResult List(GridCommand command, WorkingCalendarSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            IList<CodeDetail> workingCalendarTypeList = systemMgr.GetCodeDetails(CodeMaster.CodeMaster.WorkingCalendarType);
            ViewData["WorkingCalendarType"] = base.Transfer2DropDownList(CodeMaster.CodeMaster.WorkingCalendarType, workingCalendarTypeList);

            // 判断是否有编辑生产线工作日历的权限
            var user = SecurityContextHolder.Get();
            var q = user.UrlPermissions.Where(p => p.Contains("Url_ProdLineWorkingCalendar_Edit")).ToList();
            ViewBag.NoEditPermission = q == null || q.Count() == 0;
            
            return View();
        }

        /// <summary>
        ///  AjaxList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">WorkingCalendar Search Model</param>
        /// <returns>return the result action</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProdLineWorkingCalendar_View")]
        public ActionResult _AjaxList(GridCommand command, WorkingCalendarSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<WorkingCalendar>(searchStatementModel, command));
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <returns>New view</returns>
        [SconitAuthorize(Permissions = "Url_ProdLineWorkingCalendar_Edit")]
        public ActionResult New()
        {
            return View();
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <param name="workingCalendar">WorkingCalendar Model</param>
        /// <returns>return the result view</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_ProdLineWorkingCalendar_Edit")]
        public ActionResult New(WorkingCalendar workingCalendar)
        {
            ModelState.Remove("Shift");
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(workingCalendar.ProdLine))
                {
                    SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.PRD.WorkingCalendar.WorkingCalendar_ProdLine);
                }
                else if (((TimeSpan)(workingCalendar.EndDate.Value.Date - workingCalendar.StartWorkingDate)).TotalDays < 0)
                {
                    SaveErrorMessage(Resources.PRD.WorkingCalendar.WorkingCalendar_EndDateCanNotLessThanNow);
                }
                else
                {
                    try
                    {
                        workingCalendar.Category = CodeMaster.WorkingCalendarCategory.ProdLine;
                        WorkingCalendarMgr.CreateWorkingCalendar(workingCalendar);

                        SaveSuccessMessage(Resources.PRD.WorkingCalendar.WorkingCalendar_Added);
                        return RedirectToAction("List/");
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

            return View(workingCalendar);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProdLineWorkingCalendar_Edit")]
        public ActionResult _Update(int id, WorkingCalendar workingCalendar, WorkingCalendarSearchModel searchModel, GridCommand command)
        {
            try
            {
                ModelState.Remove("EndDate");

                var newWorkingCalendar = base.genericMgr.FindById<WorkingCalendar>(id);

                newWorkingCalendar.Shift = workingCalendar.Shift;
                newWorkingCalendar.Type = workingCalendar.Type;
                newWorkingCalendar.Category = CodeMaster.WorkingCalendarCategory.ProdLine;

                this.WorkingCalendarMgr.UpdateWorkingCalendar(newWorkingCalendar);
                //SaveSuccessMessage(Resources.PRD.WorkingCalendar.WorkingCalendar_Updated);
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
            return PartialView(GetAjaxPageData<WorkingCalendar>(searchStatementModel, command));
        }

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">WorkingCalendar Search Model</param>
        /// <returns>return WorkingCalendar search model</returns>
        private SearchStatementModel PrepareSearchStatement(GridCommand command, WorkingCalendarSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            HqlStatementHelper.AddEqStatement("Category", (int)CodeMaster.WorkingCalendarCategory.ProdLine, "wc", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("ProdLine", searchModel.SearchRegion, "wc", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Shift", searchModel.SearchShift, "wc", ref whereStatement, param);

            if (searchModel.StartWorkingDate.HasValue && searchModel.EndWorkingDate.HasValue)
            {
                HqlStatementHelper.AddBetweenStatement("WorkingDate", searchModel.StartWorkingDate, searchModel.EndWorkingDate, "wc", ref whereStatement, param);
            }
            else if (searchModel.StartWorkingDate.HasValue & !searchModel.EndWorkingDate.HasValue)
            {
                HqlStatementHelper.AddGeStatement("WorkingDate", searchModel.StartWorkingDate, "wc", ref whereStatement, param);
            }
            else if (!searchModel.StartWorkingDate.HasValue & searchModel.EndWorkingDate.HasValue)
            {
                HqlStatementHelper.AddLeStatement("WorkingDate", searchModel.EndWorkingDate, "wc", ref whereStatement, param);
            }

            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "DayOfWeekDescription")
                {
                    command.SortDescriptors[0].Member = "DayOfWeek";
                }

                if (command.SortDescriptors[0].Member == "TypeDescription")
                {
                    command.SortDescriptors[0].Member = "Type";
                }
            }

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by wc.ProdLine, wc.WorkingDate ";
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
