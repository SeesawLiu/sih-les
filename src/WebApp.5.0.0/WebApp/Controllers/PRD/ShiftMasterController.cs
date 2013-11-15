using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using com.Sconit.Entity.SCM;

namespace com.Sconit.Web.Controllers.PRD
{
    #region reference
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Security;
    using com.Sconit.Entity.SYS;
    using com.Sconit.Service;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;
    using com.Sconit.Web.Models.SearchModels.PRD;
    using com.Sconit.Entity.PRD;
    using com.Sconit.Entity.Exception;
    #endregion

    /// <summary>
    /// This controller response to control the ShiftMaster.
    /// </summary>
    public class ShiftMasterController : WebAppBaseController
    {
        /// <summary>
        /// hql to get count of the ShiftMaster
        /// </summary>
        private static string selectCountStatement = "select count(*) from ShiftMaster as sm";

        /// <summary>
        /// hql to get all of the ShiftMaster
        /// </summary>
        private static string selectStatement = "select sm from ShiftMaster as sm";

        /// <summary>
        /// hql to get count of the ShiftDetail by ShiftMaster's Code
        /// </summary>
        private static string duiplicateVerifyStatement = @"select count(*) from ShiftMaster as sm where sm.Code = ? ";

        private static string selectDetailStatement = "select sd from ShiftDetail as sd";

        #region ShiftMaster
        /// <summary>
        /// Index action for ShiftMaster controller
        /// </summary>
        /// <returns>Index view</returns>
        [SconitAuthorize(Permissions = "Url_ShiftMaster_View")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// List action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">ShiftMaster Search model</param>
        /// <returns>return the result view</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ShiftMaster_View")]
        public ActionResult List(GridCommand command, ShiftMasterSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        /// <summary>
        ///  AjaxList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">ShiftMaster Search Model</param>
        /// <returns>return the result action</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ShiftMaster_View")]
        public ActionResult _AjaxList(GridCommand command, ShiftMasterSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<ShiftMaster>(searchStatementModel, command));
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <returns>New view</returns>
        [SconitAuthorize(Permissions = "Url_ShiftMaster_Edit")]
        public ActionResult New()
        {
            return View();
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <param name="ShiftMaster">ShiftMaster Model</param>
        /// <returns>return the result view</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_ShiftMaster_Edit")]
        public ActionResult New(ShiftMaster shiftMaster)
        {
            if (ModelState.IsValid)
            {
                if (base.genericMgr.FindAll<long>(duiplicateVerifyStatement, new object[] { shiftMaster.Code })[0] > 0)
                {
                    SaveErrorMessage(Resources.ErrorMessage.Errors_Existing_Code, shiftMaster.Code);
                }
                else
                {
                    base.genericMgr.Create(shiftMaster);
                    SaveSuccessMessage(Resources.PRD.ShiftMaster.ShiftMaster_Added);
                    return RedirectToAction("Edit/" + shiftMaster.Code);
                }
            }

            return View(shiftMaster);
        }

        /// <summary>
        /// Edit view
        /// </summary>
        /// <param name="id">ShiftMaster id for edit</param>
        /// <returns>return the result view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ShiftMaster_Edit")]
        public ActionResult Edit(string id, bool? isReturn)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            else
            {
                TempData["IsReturn"] = isReturn == null ? false : isReturn;
                if (isReturn.HasValue == true && isReturn == true)
                {
                    TempData["TabIndex"] = 2;
                }
                return View("Edit", string.Empty, id);
            }
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ShiftMaster_Edit")]
        public ActionResult _Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            ShiftMaster flow = base.genericMgr.FindById<ShiftMaster>(id);
            return PartialView(flow);
        }

        /// <summary>
        /// Edit view
        /// </summary>
        /// <param name="ShiftMaster">ShiftMaster Model</param>
        /// <returns>return the partial view</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_ShiftMaster_Edit")]
        public ActionResult _Edit(ShiftMaster shiftMaster)
        {
            if (ModelState.IsValid)
            {
                var shiftMstr = base.genericMgr.FindById<ShiftMaster>(shiftMaster.Code);
                shiftMstr.Name = shiftMaster.Name;
                base.genericMgr.Update(shiftMstr);
                SaveSuccessMessage(Resources.PRD.ShiftMaster.ShiftMaster_Updated);
            }
            return PartialView(shiftMaster);
        }

        /// <summary>
        /// Delete action
        /// </summary>
        /// <param name="id">ShiftMaster id for delete</param>
        /// <returns>return to List action</returns>
        [SconitAuthorize(Permissions = "Url_ShiftMaster_Edit")]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                var selQuotedSwcHql = @"select swc from StandardWorkingCalendar as swc where swc.Shift = ?";
                var quotedSwcList = base.genericMgr.FindAll<StandardWorkingCalendar>(selQuotedSwcHql, new object[] { id });
                if (quotedSwcList.Count > 0)
                {
                    FillCodeDetailDescription(quotedSwcList);
                    var shifts = string.Join(",", quotedSwcList.Select(c => "{" + c.Region + " " + c.DayOfWeekDescription + "}").Distinct());
                    SaveErrorMessage(string.Format("标准工作日历 {0} 引用了该班次，不能删除。", shifts));
                    return RedirectToAction("Edit/" + id);
                }

                //var selQuotedFlowShiftDetailHql = @"select fsd from FlowShiftDetail as fsd where fsd.Shift = ?";
                //var quotedFlowShiftDetailList = base.genericMgr.FindAll<FlowShiftDetail>(selQuotedFlowShiftDetailHql, new object[] { id });
                //if (quotedFlowShiftDetailList.Count > 0)
                //{
                //    var flows = string.Join(",", quotedFlowShiftDetailList.Select(c => c.Flow).Distinct());
                //    SaveErrorMessage(string.Format("路线的厂商交货时段 {0} 引用了该班次，不能删除。", flows));
                //    return RedirectToAction("Edit/" + id);
                //}

                base.genericMgr.Delete(string.Format("from ShiftDetail as sd where sd.Shift = '{0}'", id));
                base.genericMgr.DeleteById<ShiftMaster>(id);
                SaveSuccessMessage(Resources.PRD.ShiftMaster.ShiftMaster_Deleted);

                return RedirectToAction("List");
            }
        }

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">ShiftMaster Search Model</param>
        /// <returns>return ShiftMaster search model</returns>
        private SearchStatementModel PrepareSearchStatement(GridCommand command, ShiftMasterSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("Code", searchModel.Code, HqlStatementHelper.LikeMatchMode.Start, "sm", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Name", searchModel.Name, HqlStatementHelper.LikeMatchMode.Start, "sm", ref whereStatement, param);

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }
        #endregion

        #region ShiftDetail

        [GridAction]
        [SconitAuthorize(Permissions = "Url_ShiftMaster_View")]
        public ActionResult _Detail(GridCommand command, ShiftDetailSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(48);
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ShiftMaster_View")]
        public ActionResult _AjaxDetailList(GridCommand command, ShiftDetailSearchModel searchModel)
        {
            string hql = selectDetailStatement + " where sd.Shift='" + searchModel.Shift + "'";
            if (searchModel.StartTime != null)
            {
                hql += " and sd.StartTime='" + searchModel.StartTime + "'";
            }
            if (searchModel.EndTime != null)
            {
                hql += " and sd.EndTime='" + searchModel.EndTime + "'";
            }
            hql += " order by Sequence";
            IList<ShiftDetail> ShiftDetailList = base.genericMgr.FindAll<ShiftDetail>(hql);
            if (ShiftDetailList.Count() < 48)
            {
                for (int i = ShiftDetailList.Count() + 1; i <= 48; i++)
                {
                    var shiftDetail = new ShiftDetail { Sequence = (short)i };
                    ShiftDetailList.Add(shiftDetail);
                }
            }
            TempData["Shift"] = searchModel.Shift;
            return PartialView(new GridModel<ShiftDetail>(ShiftDetailList));
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_ShiftMaster_Edit")]
        public JsonResult _DetailSave(List<ShiftDetail> ShiftDetail, int? SaveCount, string Shift,
            [Bind(Prefix = "inserted")]IEnumerable<ShiftDetail> insertedShiftDetails)
        {
            if (SaveCount == null || SaveCount.Value < 1 || SaveCount.Value > 48)
            {
                SaveErrorMessage("数量只能在1--48之间！");
                return Json(null);
            }
            int shiftCount = SaveCount.Value;
            try
            {
                bool IsCreate = false;
                bool IsRecommend = false;
                var inserted = new List<ShiftDetail>();
                var updated = new List<ShiftDetail>();
                var deleted = new List<ShiftDetail>();
                var dayStartTime = systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.StandardWorkTime);

                var st = ParseDateTime(dayStartTime);
                if (st == DateTime.MinValue)
                {
                    SaveErrorMessage("企业选项中工作日开始时间格式设置不正确。请输入如：06：00或14：20");
                    return Json(null);
                }
                //todo: zxy 是否需要厂商交货时段
                //var updateFlowShiftDet = new List<FlowShiftDetail>();
                if (insertedShiftDetails != null)
                {
                    ShiftDetail lastShiftDetail = null;
                    foreach (var shiftDet in insertedShiftDetails)
                    {
                        IsCreate = shiftDet.Id == 0;
                        //IList<FlowShiftDetail> flowShiftDetList = new List<FlowShiftDetail>();
                        //if (!IsCreate)
                        //{
                        //    flowShiftDetList = base.genericMgr.FindAll<FlowShiftDetail>("from FlowShiftDetail fsd where fsd.ShiftDetailId=?", new object[] { shiftDet.Id });
                        //    IsRecommend = flowShiftDetList.Count() > 0;
                        //}

                        if (SaveCount > 0)
                        {
                            ValidatTime(shiftDet, st);
                            Validate(shiftDet, lastShiftDetail);

                            if (IsCreate)
                                inserted.Add(shiftDet);
                            else
                            {
                                //if (IsRecommend)
                                //{
                                //    foreach (FlowShiftDetail fsd in flowShiftDetList)
                                //    {
                                //        fsd.StartTime = shiftDet.StartTime;
                                //        fsd.EndTime = shiftDet.EndTime;
                                //        updateFlowShiftDet.Add(fsd);
                                //    }
                                //}
                                updated.Add(shiftDet);
                            }
                        }
                        else if (!IsCreate)
                        {
                            //if (IsRecommend)
                            //    throw new BusinessException(Resources.PRD.ShiftMaster.ShiftMaster_NotDeletedShiftMaster, shiftDet.Sequence.ToString());

                            deleted.Add(shiftDet);
                        }
                        SaveCount--;
                        lastShiftDetail = shiftDet;
                    }
                    foreach (var sd in inserted)
                        base.genericMgr.Create(sd);

                    foreach (var sd in updated)
                        base.genericMgr.Update(sd);

                    foreach (var sd in deleted)
                        base.genericMgr.Delete(sd);

                    //foreach (var fsd in updateFlowShiftDet)
                    //    base.genericMgr.Update(fsd);

                    var shiftMstr = base.genericMgr.FindById<ShiftMaster>(Shift);
                    shiftMstr.ShiftCount = shiftCount;
                    base.genericMgr.Update(shiftMstr);
                }
                SaveSuccessMessage(Resources.PRD.ShiftMaster.ShiftMaster_Save);
                return Json(new { });
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                return Json(null);
            }
        }

        private void ValidatTime(ShiftDetail shiftDet, DateTime dayStartTime)
        {
            shiftDet.Start = ParseDateTime(shiftDet.StartTime);
            if (shiftDet.Start == DateTime.MinValue)
            {
                throw new BusinessException(Resources.PRD.ShiftDetail.Errors_From_LineValidatTime, shiftDet.Sequence.ToString(), Resources.PRD.ShiftDetail.Errors_Form_StartTime);
            }
            shiftDet.End = ParseDateTime(shiftDet.EndTime);
            if (shiftDet.End == DateTime.MinValue)
            {
                throw new BusinessException(Resources.PRD.ShiftDetail.Errors_From_LineValidatTime, shiftDet.Sequence.ToString(), Resources.PRD.ShiftDetail.Errors_Form_EndTime);
            }

            if (shiftDet.Start < dayStartTime)
                shiftDet.Start = shiftDet.Start.AddDays(1);
            // 如果结束结束时间小于 工作日开始时间，则视为第二天
            if (shiftDet.End <= dayStartTime)
                shiftDet.End = shiftDet.End.AddDays(1);

            if (shiftDet.Start > shiftDet.End)
            {
                throw new BusinessException(Resources.PRD.ShiftDetail.Errors_From_LineValidatTime, shiftDet.Sequence.ToString(), Resources.PRD.ShiftDetail.Errors_StartTimeGreaterThanEndTime);
            }

            dayStartTime = dayStartTime.AddDays(1);
            if (shiftDet.Start < dayStartTime && shiftDet.End > dayStartTime)
            {
                throw new BusinessException(Resources.PRD.ShiftDetail.Errors_From_LineValidatTime, shiftDet.Sequence.ToString(), Resources.PRD.ShiftDetail.Errors_StartTimeLessThanDayOfStartTimeThenEndTimeCanNotGreatherThan);
            }
        }

        private void Validate(ShiftDetail shiftDet, ShiftDetail lastDetail)
        {
            if (lastDetail == null)
                return;

            // 起始时间小于前一个班次明细的起始时间
            if (shiftDet.Start < lastDetail.Start)
            {
                throw new BusinessException("第 {0} 行开始时间 {1} 不能小于第 {2} 行中的开始时间 {3}", shiftDet.Sequence.ToString(), shiftDet.StartTime, lastDetail.Sequence.ToString(), lastDetail.StartTime);
            }

            // 开始时间小于前一个班次明细的结束时间
            if (shiftDet.Start < lastDetail.End)
            {
                throw new BusinessException("第 {0} 行开始时间 {1} 不能小于第 {2} 行中的结束时间 {3}", shiftDet.Sequence.ToString(), shiftDet.StartTime, lastDetail.Sequence.ToString(), lastDetail.EndTime);
            }
        }

        private DateTime ParseDateTime(string time)
        {
            if (string.IsNullOrWhiteSpace(time))
                return DateTime.MinValue;

            var t = time.Split(':');
            if (t.Length != 2)
                return DateTime.MinValue;

            int h;
            int.TryParse(t[0], out h);
            if (h >= 24)
                return DateTime.MinValue;

            int m;
            int.TryParse(t[1], out m);

            if (h > 59)
                return DateTime.MinValue;

            return new DateTime(2012, 1, 1, h, m, 0);
        }

        #endregion
    }
}