using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using com.Sconit.CodeMaster;
using com.Sconit.Entity.PRD;
using com.Sconit.Service;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels.PRD;
using com.Sconit.Web.Util;
using com.Sconit.Entity.Exception;

namespace com.Sconit.Web.Controllers.PRD
{
    public class ProdLineSpecialTimeController : WebAppBaseController
    {
        #region Properties
        public IWorkingCalendarMgr workingCalendarMgr { get; set; }
        #endregion

        /// <summary>
        /// hql for SpecialTime
        /// </summary>
        private static string specialTimeSelectCountStatement = "select count(*) from SpecialTime as s";

        /// <summary>
        /// hql for SpecialTime
        /// </summary>
        private static string specialTimeSelectStatement = "select s from SpecialTime as s";

        private static string selectspecialTimeByProdLineStatement = @"select s from SpecialTime as s where s.ProdLine = ?";

        #region 停线时间
        /// <summary>
        /// SpecialTime action
        /// </summary>
        /// <returns>rediret view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProdLineSpecialTime_RestView")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// SpecialTimeList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">SpecialTime Search Model</param>
        /// <returns>return to the result view</returns>
        [GridAction]
        [SconitAuthorize(Permissions = "Url_ProdLineSpecialTime_RestView")]
        public ActionResult List(GridCommand command, SpecialTimeSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        /// <summary>
        /// _AjaxSpecialTimeList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">SpecialTime Search Model</param>
        /// <returns>return to the result view</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProdLineSpecialTime_RestView")]
        public ActionResult _AjaxList(GridCommand command, SpecialTimeSearchModel searchModel)
        {
            string whereStatement = " where s.Type=" + (int)com.Sconit.CodeMaster.WorkingCalendarType.Rest + " ";
            SearchStatementModel searchStatementModel = this.SpecialTimePrepareSearchStatement(command, searchModel, whereStatement);
            return PartialView(GetAjaxPageData<SpecialTime>(searchStatementModel, command));
        }

        /// <summary>
        /// SpecialTimeNew action
        /// </summary>
        /// <returns>rediret view</returns>
        [SconitAuthorize(Permissions = "Url_ProdLineSpecialTime_RestEdit")]
        public ActionResult New()
        {
            SpecialTime st = new SpecialTime { StartTime = DateTime.Now, EndTime = DateTime.Now };
            return View(st);
        }

        /// <summary>
        /// SpecialTimeNew action
        /// </summary>
        /// <param name="specialTime">specialTime model</param>
        /// <returns>return to the result view</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_ProdLineSpecialTime_RestEdit")]
        public ActionResult New(SpecialTime specialTime)
        {
            if (ModelState.IsValid)
            {
                if (DateTime.Compare(specialTime.EndTime, specialTime.StartTime) < 1)
                {
                    SaveErrorMessage(Resources.PRD.SpecialTime.Errors_StartDateGreaterThanEndDate);
                }
                else
                {
                    var specialTimes = base.genericMgr.FindAll<SpecialTime>(selectspecialTimeByProdLineStatement, new object[] { specialTime.ProdLine ?? string.Empty });
                    var existStartTime = specialTimes.FirstOrDefault(c => c.StartTime <= specialTime.StartTime && c.EndTime > specialTime.StartTime);

                    if (existStartTime != null)
                    {
                        SaveErrorMessage(Resources.PRD.SpecialTime.Errors_StartDateExistInRange, new string[] { existStartTime.StartTime.ToString(), existStartTime.EndTime.ToString() });
                    }
                    else
                    {
                        var existEndTime = specialTimes.FirstOrDefault(c => c.StartTime < specialTime.EndTime && c.EndTime >= specialTime.EndTime);
                        if (existEndTime != null)
                        {
                            SaveErrorMessage(Resources.PRD.SpecialTime.Errors_EndDateExistInRange, new string[] { existEndTime.StartTime.ToString(), existEndTime.EndTime.ToString() });
                        }
                        else
                        {
                            var coverdSpecialTime = specialTimes.FirstOrDefault(c => c.StartTime > specialTime.StartTime && c.EndTime < specialTime.EndTime);
                            if (coverdSpecialTime != null)
                            {
                                SaveErrorMessage(Resources.PRD.SpecialTime.Errors_CoverTheProdLine, new string[] { coverdSpecialTime.StartTime.ToString(), coverdSpecialTime.EndTime.ToString() });
                            }
                            else
                            {
                                specialTime.Type = WorkingCalendarType.Rest;
                                specialTime.Category = WorkingCalendarCategory.ProdLine;
                                //base.genericMgr.Create(specialTime);
                                try
                                {
                                    this.workingCalendarMgr.AddProdLineSpecialTime(specialTime);
                                    SaveSuccessMessage(Resources.PRD.SpecialTime.SpecialTime_Added);
                                    return RedirectToAction("Edit/" + specialTime.Id);
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
                    }
                }
            }

            return View(specialTime);
        }

        /// <summary>
        /// SpecialTimeEdit action
        /// </summary>
        /// <param name="id">SpecialTime id for edit</param>
        /// <returns>return to the result view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProdLineSpecialTime_RestEdit")]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                SpecialTime specialTime = base.genericMgr.FindById<SpecialTime>(id);
                return View(specialTime);
            }
        }

        /// <summary>
        /// SpecialTimeEdit action
        /// </summary>
        /// <param name="specialTime">specialTime model</param>
        /// <returns>return to the result view</returns>
        [SconitAuthorize(Permissions = "Url_ProdLineSpecialTime_RestEdit")]
        public ActionResult Edit(SpecialTime specialTime)
        {
            if (ModelState.IsValid)
            {
                if (DateTime.Compare(specialTime.EndTime, specialTime.StartTime) < 1)
                {
                    SaveErrorMessage(Resources.PRD.SpecialTime.Errors_StartDateGreaterThanEndDate);
                }
                else
                {
                    var specialTimes = base.genericMgr.FindAll<SpecialTime>(selectspecialTimeByProdLineStatement, new object[] { specialTime.ProdLine ?? string.Empty });
                    var existStartTime = specialTimes.FirstOrDefault(c => c.StartTime <= specialTime.StartTime && c.EndTime > specialTime.StartTime && c.Id != specialTime.Id);

                    if (existStartTime != null)
                    {
                        SaveErrorMessage(Resources.PRD.SpecialTime.Errors_StartDateExistInRange, new string[] { existStartTime.StartTime.ToString(), existStartTime.EndTime.ToString() });
                    }
                    else
                    {
                        var existEndTime = specialTimes.FirstOrDefault(c => c.StartTime < specialTime.EndTime && c.EndTime >= specialTime.EndTime && c.Id != specialTime.Id);
                        if (existEndTime != null)
                        {
                            SaveErrorMessage(Resources.PRD.SpecialTime.Errors_EndDateExistInRange,
                                             new string[] { existEndTime.StartTime.ToString(), existEndTime.EndTime.ToString() });
                        }
                        else
                        {
                            var coverdSpecialTime = specialTimes.FirstOrDefault(c => c.StartTime > specialTime.StartTime && c.EndTime < specialTime.EndTime && c.Id != specialTime.Id);
                            if (coverdSpecialTime != null)
                            {
                                SaveErrorMessage(Resources.PRD.SpecialTime.Errors_CoverTheProdLine,
                                                 new string[]
                                                     {
                                                         coverdSpecialTime.StartTime.ToString(),
                                                         coverdSpecialTime.EndTime.ToString()
                                                     });
                            }
                            else
                            {
                                try
                                {
                                    specialTime.Type = WorkingCalendarType.Rest;
                                    specialTime.Category = WorkingCalendarCategory.ProdLine;
                                    this.workingCalendarMgr.UpdateProdLineSpecialTime(specialTime);
                                    //base.genericMgr.Update(specialTime);
                                    SaveSuccessMessage(Resources.PRD.SpecialTime.SpecialTime_Updated);
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
                    }
                }
            }

            return View(specialTime);
        }

        /// <summary>
        /// SpecialTimeDelete action
        /// </summary>
        /// <param name="id">SpecialTime id for delete</param>
        /// <returns>return to SpecialTimeList action</returns>
        [SconitAuthorize(Permissions = "Url_ProdLineSpecialTime_RestDelete")]
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                SpecialTime specialTime = base.genericMgr.FindById<SpecialTime>(id.Value);
                try
                {
                    this.workingCalendarMgr.DeleteProdLineSpecialTime(specialTime);
                    //base.genericMgr.DeleteById<SpecialTime>(id);
                    SaveSuccessMessage(Resources.PRD.SpecialTime.SpecialTime_Deleted);
                }
                catch (BusinessException ex)
                {
                    SaveErrorMessage(ex.Message);
                }
            }
            return RedirectToAction("List");
        }
        #endregion

        #region 加班时间

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProdLineSpecialTime_WorkView")]
        public ActionResult WorkIndex()
        {
            return View();
        }

        /// <summary>
        /// SpecialTimeList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">SpecialTime Search Model</param>
        /// <returns>return to the result view</returns>
        [GridAction]
        [SconitAuthorize(Permissions = "Url_ProdLineSpecialTime_WorkView")]
        public ActionResult WorkList(GridCommand command, SpecialTimeSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        /// <summary>
        /// _AjaxSpecialTimeList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">SpecialTime Search Model</param>
        /// <returns>return to the result view</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProdLineSpecialTime_WorkView")]
        public ActionResult _WorkAjaxList(GridCommand command, SpecialTimeSearchModel searchModel)
        {
            string whereStatement = " where s.Type=" + (int)com.Sconit.CodeMaster.WorkingCalendarType.Work + " ";
            SearchStatementModel searchStatementModel = this.SpecialTimePrepareSearchStatement(command, searchModel, whereStatement);
            return PartialView(GetAjaxPageData<SpecialTime>(searchStatementModel, command));
        }

        /// <summary>
        /// SpecialTimeNew action
        /// </summary>
        /// <returns>rediret view</returns>
        [SconitAuthorize(Permissions = "Url_ProdLineSpecialTime_WorkEdit")]
        public ActionResult WorkNew()
        {
            SpecialTime st = new SpecialTime { StartTime = DateTime.Now, EndTime = DateTime.Now };
            return View(st);
        }

        /// <summary>
        /// SpecialTimeNew action
        /// </summary>
        /// <param name="specialTime">specialTime model</param>
        /// <returns>return to the result view</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_ProdLineSpecialTime_WorkEdit")]
        public ActionResult WorkNew(SpecialTime specialTime)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(specialTime.ProdLine))
                {
                    SaveErrorMessage(Resources.PRD.SpecialTime.SpecialTime_ProdLine);
                }
                else if (DateTime.Compare(specialTime.EndTime, specialTime.StartTime) < 1)
                {
                    SaveErrorMessage(Resources.PRD.SpecialTime.Errors_StartDateGreaterThanEndDate);
                }
                else
                {
                    var specialTimes = base.genericMgr.FindAll<SpecialTime>(selectspecialTimeByProdLineStatement, new object[] { specialTime.ProdLine ?? string.Empty });
                    var existStartTime = specialTimes.FirstOrDefault(c => c.StartTime <= specialTime.StartTime && c.EndTime > specialTime.StartTime);

                    if (existStartTime != null)
                    {
                        SaveErrorMessage(Resources.PRD.SpecialTime.Errors_StartDateExistInRange, new string[] { existStartTime.StartTime.ToString(), existStartTime.EndTime.ToString() });
                    }
                    else
                    {
                        var existEndTime = specialTimes.FirstOrDefault(c => c.StartTime < specialTime.EndTime && c.EndTime >= specialTime.EndTime);
                        if (existEndTime != null)
                        {
                            SaveErrorMessage(Resources.PRD.SpecialTime.Errors_EndDateExistInRange, new string[] { existEndTime.StartTime.ToString(), existEndTime.EndTime.ToString() });
                        }
                        else
                        {
                            var coverdSpecialTime = specialTimes.FirstOrDefault(c => c.StartTime > specialTime.StartTime && c.EndTime < specialTime.EndTime);
                            if (coverdSpecialTime != null)
                            {
                                SaveErrorMessage(Resources.PRD.SpecialTime.Errors_CoverTheProdLine, new string[] { coverdSpecialTime.StartTime.ToString(), coverdSpecialTime.EndTime.ToString() });
                            }
                            else
                            {
                                specialTime.Type = WorkingCalendarType.Work;
                                specialTime.Category = WorkingCalendarCategory.ProdLine;
                                //base.genericMgr.Create(specialTime);
                                try
                                {
                                    this.workingCalendarMgr.AddProdLineSpecialTime(specialTime);
                                    SaveSuccessMessage("加班时间新增成功。");
                                    return RedirectToAction("WorkEdit/" + specialTime.Id);
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
                    }
                }
            }

            return View(specialTime);
        }

        /// <summary>
        /// SpecialTimeEdit action
        /// </summary>
        /// <param name="id">SpecialTime id for edit</param>
        /// <returns>return to the result view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProdLineSpecialTime_WorkEdit")]
        public ActionResult WorkEdit(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                SpecialTime specialTime = base.genericMgr.FindById<SpecialTime>(id);
                return View(specialTime);
            }
        }

        /// <summary>
        /// SpecialTimeEdit action
        /// </summary>
        /// <param name="specialTime">specialTime model</param>
        /// <returns>return to the result view</returns>
        [SconitAuthorize(Permissions = "Url_ProdLineSpecialTime_WorkEdit")]
        public ActionResult WorkEdit(SpecialTime specialTime)
        {
            if (ModelState.IsValid)
            {
                if (DateTime.Compare(specialTime.EndTime, specialTime.StartTime) < 1)
                {
                    SaveErrorMessage(Resources.PRD.SpecialTime.Errors_StartDateGreaterThanEndDate);
                }
                else
                {
                    var specialTimes = base.genericMgr.FindAll<SpecialTime>(selectspecialTimeByProdLineStatement, new object[] { specialTime.ProdLine ?? string.Empty });
                    var existStartTime = specialTimes.FirstOrDefault(c => c.StartTime <= specialTime.StartTime && c.EndTime > specialTime.StartTime && c.Id != specialTime.Id);

                    if (existStartTime != null)
                    {
                        SaveErrorMessage(Resources.PRD.SpecialTime.Errors_StartDateExistInRange, new string[] { existStartTime.StartTime.ToString(), existStartTime.EndTime.ToString() });
                    }
                    else
                    {
                        var existEndTime = specialTimes.FirstOrDefault(c => c.StartTime < specialTime.EndTime && c.EndTime >= specialTime.EndTime && c.Id != specialTime.Id);
                        if (existEndTime != null)
                        {
                            SaveErrorMessage(Resources.PRD.SpecialTime.Errors_EndDateExistInRange,
                                             new string[] { existEndTime.StartTime.ToString(), existEndTime.EndTime.ToString() });
                        }
                        else
                        {
                            var coverdSpecialTime = specialTimes.FirstOrDefault(c => c.StartTime > specialTime.StartTime && c.EndTime < specialTime.EndTime && c.Id != specialTime.Id);
                            if (coverdSpecialTime != null)
                            {
                                SaveErrorMessage(Resources.PRD.SpecialTime.Errors_CoverTheProdLine,
                                                 new string[]
                                                     {
                                                         coverdSpecialTime.StartTime.ToString(),
                                                         coverdSpecialTime.EndTime.ToString()
                                                     });
                            }
                            else
                            {
                                try
                                {
                                    specialTime.Type = WorkingCalendarType.Work;
                                    specialTime.Category = WorkingCalendarCategory.ProdLine;
                                    this.workingCalendarMgr.UpdateProdLineSpecialTime(specialTime);
                                    //base.genericMgr.Update(specialTime);
                                    SaveSuccessMessage("加班时间更新成功。");
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
                    }
                }
            }

            return View(specialTime);
        }

        [SconitAuthorize(Permissions = "Url_ProdLineSpecialTime_WorkDelete")]
        public ActionResult WorkDelete(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                SpecialTime specialTime = base.genericMgr.FindById<SpecialTime>(id.Value);
                try
                {
                    this.workingCalendarMgr.DeleteProdLineSpecialTime(specialTime);
                    //base.genericMgr.DeleteById<SpecialTime>(id);
                    SaveSuccessMessage("加班时间删除成功。");
                }
                catch (BusinessException ex)
                {
                    SaveErrorMessage(ex.Message);
                }
            }
            return RedirectToAction("WorkList");
        }

        #endregion

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">SpecialTime Search Model</param>
        /// <returns>Search Statement</returns>
        private SearchStatementModel SpecialTimePrepareSearchStatement(GridCommand command, SpecialTimeSearchModel searchModel, string whereStatement)
        {
            IList<object> param = new List<object>();
            HqlStatementHelper.AddEqStatement("Category", (int)WorkingCalendarCategory.ProdLine, "s", ref whereStatement, param);
            if (searchModel.Region == null)
            {
                HqlStatementHelper.AddLikeStatement("ProdLine", string.Empty, HqlStatementHelper.LikeMatchMode.Start, "s", ref whereStatement, param);
            }
            else
            {
                HqlStatementHelper.AddEqStatement("ProdLine", searchModel.Region, "s", ref whereStatement, param);
            }

            if (searchModel.StartTime != null)
            {
                HqlStatementHelper.AddGeStatement("StartTime", searchModel.StartTime, "s", ref whereStatement, param);
            }
            if (searchModel.EndTime != null)
            {
                HqlStatementHelper.AddLeStatement("EndTime", searchModel.EndTime, "s", ref whereStatement, param);
            }

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = specialTimeSelectCountStatement;
            searchStatementModel.SelectStatement = specialTimeSelectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }
    }
}