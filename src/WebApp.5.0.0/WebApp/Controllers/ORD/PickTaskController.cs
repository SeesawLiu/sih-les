using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using com.Sconit.Service;
using com.Sconit.Web.Util;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.UI;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels.ORD;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.INV;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.Exception;
using com.Sconit.PrintModel.INV;
using AutoMapper;
using com.Sconit.Utility.Report;
using com.Sconit.Utility;

namespace com.Sconit.Web.Controllers.ORD
{
    public class PickTaskController : WebAppBaseController
    {
        public IPickTaskMgr pickTaskMgr { get; set; }
        public IHuMgr huMgr { get; set; }
        public IReportGen reportGen { get; set; }

        [SconitAuthorize(Permissions = "Url_PickTask_View")]
        public ActionResult Index()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_PickTask_Handle")]
        public ActionResult Handle()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_PickTask_New")]
        public ActionResult New()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_PickTask_View")]
        public ActionResult List(GridCommand command, PickTaskSearchModel searchModel)
        {
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            if (String.IsNullOrEmpty(searchModel.LocationFrom))
            {
                SaveWarningMessage("请选择来源库位!");
            }

            ViewBag.SearchModel = searchModel;
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_PickTask_View")]
        public ActionResult _AjaxList(GridCommand command, PickTaskSearchModel searchModel)
        {
            if (String.IsNullOrEmpty(searchModel.LocationFrom))
            {
                return PartialView(new GridModel(new List<PickTask>()));
            }
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<PickTask>(searchStatementModel, command));
        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, PickTaskSearchModel searchModel)
        {
            string whereStatement = string.Empty;
            if (!searchModel.IncludeFinished)
            {
                whereStatement += " where c.OrderedQty > c.PickedQty ";
            }
            IList<object> param = new List<object>();
            HqlStatementHelper.AddEqStatement("PickId", searchModel.PickId, "c", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("LocationFrom", searchModel.LocationFrom, "c", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("OrderNo", searchModel.OrderNo, "c", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Picker", searchModel.Picker, "c", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "c", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "c", ref whereStatement, param);
            //HqlStatementHelper.AddEqStatement("IncludeFinished", searchModel.IncludeFinished, "c", ref whereStatement, param);
            if (searchModel.ReleaseStart != null)
            {
                HqlStatementHelper.AddGeStatement("ReleaseDate", searchModel.ReleaseStart.Value, "c", ref whereStatement, param);
            }
            if (searchModel.ReleaseEnd != null)
            {
                HqlStatementHelper.AddLtStatement("ReleaseDate", searchModel.ReleaseEnd.Value, "c", ref whereStatement, param);
            }
            if (searchModel.WindowStart != null)
            {
                HqlStatementHelper.AddGeStatement("WindowTime", searchModel.WindowStart.Value, "c", ref whereStatement, param);
            }
            if (searchModel.WindowEnd != null)
            {
                HqlStatementHelper.AddLtStatement("WindowTime", searchModel.WindowEnd.Value, "c", ref whereStatement, param);
            }
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = "select count(*) from PickTask  c";
            searchStatementModel.SelectStatement = "select c from PickTask as c";
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_PickTask_Handle")]
        public ActionResult ListHandle(GridCommand command, PickTaskSearchModel searchModel)
        {
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.SearchModel = searchModel;

            if (String.IsNullOrEmpty(searchModel.LocationFrom))
            {
                SaveWarningMessage("请选择来源库位!");
            }
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_PickTask_Handle")]
        public ActionResult _AjaxListHandle(GridCommand command, PickTaskSearchModel searchModel)
        {
            if (String.IsNullOrEmpty(searchModel.LocationFrom))
            {
                return PartialView(new GridModel(new List<PickTask>()));
            }
            //SearchStatementModel searchStatementModel = this.PrepareHandleSearchStatement(command, searchModel);
            //return PartialView(GetAjaxPageData<PickTask>(searchStatementModel, command));
            string whereStatement = "select c.* from ord_picktask c inner join view_ordermstr o on c.orderno = o.orderno where "
                + "(o.Status = " + (int)CodeMaster.OrderStatus.Submit
                                                    + " or o.Status = "
                                                    + (int)CodeMaster.OrderStatus.InProcess
                                                    + ") and c.orderedqty > c.pickedqty ";
            if (searchModel.ShowHold)
            {
                whereStatement += " and c.IsHold = 1 ";
            }
            else
            {
                whereStatement += " and c.IsHold = 0 ";
            }
            if (!String.IsNullOrEmpty(searchModel.PickId)) {
                whereStatement += " and c.PickId = '" + searchModel.PickId + "'";
            }
            if (!String.IsNullOrEmpty(searchModel.LocationFrom))
            {
                whereStatement += " and c.LocationFrom = '" + searchModel.LocationFrom + "'";
            }
            if (!String.IsNullOrEmpty(searchModel.OrderNo))
            {
                whereStatement += " and c.OrderNo = '" + searchModel.OrderNo + "'";
            }
            if (!String.IsNullOrEmpty(searchModel.Picker))
            {
                whereStatement += " and c.Picker = '" + searchModel.Picker + "'";
            }
            if (!String.IsNullOrEmpty(searchModel.Item))
            {
                whereStatement += " and c.Item = '" + searchModel.Item + "'";
            }
            if (!String.IsNullOrEmpty(searchModel.Flow))
            {
                whereStatement += " and c.Flow = '" + searchModel.Flow + "'";
            }
            if (searchModel.ReleaseStart != null)
            {
                whereStatement += " and c.ReleaseDate >= '" + searchModel.ReleaseStart + "'";
            }
            if (searchModel.ReleaseEnd != null)
            {
                whereStatement += " and c.ReleaseDate < '" + searchModel.ReleaseEnd + "'";
            }
            if (searchModel.WindowStart != null)
            {
                whereStatement += " and c.WindowTime >= '" + searchModel.WindowStart + "'";
            }
            if (searchModel.WindowEnd != null)
            {
                whereStatement += " and c.WindowTime < '" + searchModel.WindowEnd + "'";
            }

            IList<PickTask> pts = base.genericMgr.FindEntityWithNativeSql<PickTask>(whereStatement);

            foreach(PickTask pt in pts)
            {
                pt.NewPicker = pickTaskMgr.GetDefaultPicker(pt);
            }

            GridModel<PickTask> GridModel = new GridModel<PickTask>();
            GridModel.Total = pts.Count;
            GridModel.Data = pts;
            ViewBag.Total = GridModel.Total;
            return PartialView(GridModel);
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_PickTask_New")]
        public ActionResult ListOrderDetail(GridCommand command, PickTaskSearchModel searchModel)
        {
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.SearchModel = searchModel;
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_PickTask_New")]
        public ActionResult _AjaxListOrderDetail(GridCommand command, PickTaskSearchModel searchModel)
        {
            string whereStatement = "select c.* from view_orderdet c inner join view_ordermstr o on c.orderno = o.orderno where "
               + "(o.Status = " + (int)CodeMaster.OrderStatus.Submit
                                                   + " or o.Status = "
                                                   + (int)CodeMaster.OrderStatus.InProcess
                                                   + ") and c.orderno = '" + searchModel.OrderNo
                                                   + "' and not exists (select 1 from ord_picktask as s where s.orddetid = c.id)";
            IList<OrderDetail> ods = base.genericMgr.FindEntityWithNativeSql<OrderDetail>(whereStatement);

            GridModel<OrderDetail> GridModel = new GridModel<OrderDetail>();
            GridModel.Total = ods.Count;
            GridModel.Data = ods;
            ViewBag.Total = GridModel.Total;
            return PartialView(GridModel);
        }

        public JsonResult AssignTask(string ChosenTasks, string NewPickers)
        {
            try
            {
                if (String.IsNullOrEmpty(ChosenTasks))
                {
                    throw new BusinessException("请先选择任务!");
                }

                pickTaskMgr.AssignPickTask(ChosenTasks.Split(','), NewPickers.Split(','));

                SaveSuccessMessage("操作成功");

                string re = "操作成功";
                return Json(new { re });
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                return Json(null);
            }
        }

        public JsonResult HoldTask(string ChosenTasks)
        {
            try
            {
                if (String.IsNullOrEmpty(ChosenTasks))
                {
                    throw new BusinessException("请先选择任务!");
                }

                pickTaskMgr.HoldPickTask(ChosenTasks.Split(','));

                SaveSuccessMessage("操作成功");

                string re = "操作成功";
                return Json(new { re });
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                return Json(null);
            }
        }

        public JsonResult UnholdTask(string ChosenTasks)
        {
            try
            {
                if (String.IsNullOrEmpty(ChosenTasks))
                {
                    throw new BusinessException("请先选择任务!");
                }

                pickTaskMgr.UnholdPickTask(ChosenTasks.Split(','));

                SaveSuccessMessage("操作成功");

                string re = "操作成功";
                return Json(new { re });
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                return Json(null);
            }
        }

        public JsonResult CreateTask(string orderno, string ChosenIds)
        {
            try
            {
                if (String.IsNullOrEmpty(ChosenIds))
                {
                    throw new BusinessException("请先选择明细!");
                }

                IList<int> odids = new List<int>();
                foreach (string s in ChosenIds.Split(',')) {
                    odids.Add(Int32.Parse(s));
                }

                pickTaskMgr.CreatePickTask(orderno, odids);

                SaveSuccessMessage("操作成功");

                string re = "操作成功";
                return Json(new { re });
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                return Json(null);
            }
        }

        public JsonResult PrintTask(string ChosenTasks)
        {
            try
            {
                if (String.IsNullOrEmpty(ChosenTasks))
                {
                    throw new BusinessException("请先选择任务!");
                }

                IList<Hu> hus = new List<Hu>();
                IList<PickTask> printTasks = new List<PickTask>();

                string[] tasks = ChosenTasks.Split(',');
                foreach (string task in tasks) {
                    PickTask picktask = base.genericMgr.FindAll<PickTask>("from PickTask where PickId = ? ",
                        task).SingleOrDefault();
                    if (picktask != null) {
                        IList<Hu> singleTaskHus = pickTaskMgr.GetHuByPickTask(picktask);
                        if (singleTaskHus != null && singleTaskHus.Count > 0) {
                            foreach (Hu sh in singleTaskHus) {
                                hus.Add(sh);
                            }

                            printTasks.Add(picktask);
                        }
                    }
                }

                foreach (var hu in hus)
                {
                    hu.ManufacturePartyDescription = base.genericMgr.FindById<Party>(hu.ManufactureParty).Name;
                }
                string template = systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.DefaultBarCodeTemplate);
                string reportFileUrl = PrintHuList(hus, template);

                foreach (PickTask t in printTasks)
                {
                    t.PrintCount++;
                    base.genericMgr.Update(t);
                }

                SaveSuccessMessage("条码已经打印!Url=" + reportFileUrl);

                object obj = new { SuccessMessage = "条码已经打印!", PrintUrl = reportFileUrl };
                return Json(obj);
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                return Json(null);
            }
        }

        public string PrintHuList(IList<Hu> huList, string huTemplate)
        {
            IList<PrintHu> printHuList = Mapper.Map<IList<Hu>, IList<PrintHu>>(huList);

            IList<object> data = new List<object>();
            data.Add(printHuList);
            data.Add(CurrentUser.FullName);
            return reportGen.WriteToFile(huTemplate, data);
        }

    }
}
