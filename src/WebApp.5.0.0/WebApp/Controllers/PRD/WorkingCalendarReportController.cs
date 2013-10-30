using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using com.Sconit.Entity.VIEW;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels.PRD;
using com.Sconit.Web.Util;

namespace com.Sconit.Web.Controllers.PRD
{
    public class WorkingCalendarReportController : WebAppBaseController
    {
        [SconitAuthorize(Permissions = "Url_WorkingCalendarReport_View")]
        public ActionResult Index()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_WorkingCalendarReport_View")]
        public ActionResult List(GridCommand command, WorkingCalendarSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            var error = false;
            if (string.IsNullOrWhiteSpace(searchModel.SearchRegion))
            {
                error = true;
                SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.PRD.WorkingCalendar.WorkingCalendar_Region);
            }
            if (!searchModel.StartWorkingDate.HasValue)
            {
                error = true;
                SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.PRD.WorkingCalendar.WorkingCalendar_StartWorkingDate);
            }
            if (!searchModel.EndWorkingDate.HasValue)
            {
                error = true;
                SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.PRD.WorkingCalendar.WorkingCalendar_EndWorkingDate);
            }
            if (searchModel.StartWorkingDate.HasValue && searchModel.EndWorkingDate.HasValue && searchModel.StartWorkingDate.Value >= searchModel.EndWorkingDate.Value)
            {
                error = true;
                SaveErrorMessage(Resources.PRD.WorkingCalendar.WorkingCalendar_EndDateMustGreaterThanStartDate);
            }
            ViewBag.ReadOnly = error;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);

            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_WorkingCalendarReport_View")]
        public ActionResult _AjaxList(GridCommand command, WorkingCalendarSearchModel searchModel)
        {
            var objList = this.genericMgr.FindAllWithNativeSql<object[]>("exec USP_Busi_GetWorkingCalendarView ?,?,?,?",
                                                            new object[]
                                                               {
                                                                    "",searchModel.SearchRegion,
                                                                   searchModel.StartWorkingDate, searchModel.EndWorkingDate
                                                               });

            var list = new List<WorkingCalendarView>();
            foreach (var obj in objList)
            {
                var w = obj as object[];
                if (w == null) continue;

                list.Add(new WorkingCalendarView
                {
                    Date = (DateTime)w[0],
                    DateFrom = (DateTime)w[1],
                    DateTo = (DateTime)w[2]
                });
            }

            return PartialView(new GridModel(list));
        }
    }
}
