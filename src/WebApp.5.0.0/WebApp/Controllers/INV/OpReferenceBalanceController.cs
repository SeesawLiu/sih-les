﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using com.Sconit.Service;
using Telerik.Web.Mvc;
using com.Sconit.Web.Util;
using com.Sconit.Web.Models;
using com.Sconit.Entity.INV;
using com.Sconit.Web.Models.SearchModels.INV;
using com.Sconit.Utility;
using com.Sconit.Entity.ACC;
using com.Sconit.Entity.SCM;
using com.Sconit.Entity.Exception;
using NHibernate.Type;
using NHibernate;
using com.Sconit.Entity.MD;
using System.Web.Routing;

namespace com.Sconit.Web.Controllers.INV
{
    public class OpReferenceBalanceController : WebAppBaseController
    {
        public IStockTakeMgr stockTakeMgr { get; set; }

        private static string selectCountStatement = "select count(*) from OpReferenceBalance as l";

        private static string selectStatement = "from  OpReferenceBalance as l";

        #region 查询

        public ActionResult Index()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OpReferenceBalance_View")]
        public ActionResult List(GridCommand command, OpReferenceBalanceSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            //if (this.CheckSearchModelIsNull(searchCacheModel.SearchObject))
            //{
            //    TempData["_AjaxMessage"] = "";
            //}
            //else
            //{
            //    SaveWarningMessage(Resources.ErrorMessage.Errors_NoConditions);
            //}
            ViewBag.PageSize = this.ProcessPageSize(command.PageSize);

            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OpReferenceBalance_View")]
        public ActionResult _AjaxList(GridCommand command, OpReferenceBalanceSearchModel searchModel)
        {
            //if (!this.CheckSearchModelIsNull(searchModel))
            //{
            //    return PartialView(new GridModel(new List<OpReferenceBalance>()));
            //}
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            GridModel<OpReferenceBalance> gridModel = GetAjaxPageData<OpReferenceBalance>(searchStatementModel, command);
            if (gridModel.Data != null)
            {
                foreach (var opReferenceBalance in gridModel.Data)
                {
                    Item item = this.genericMgr.FindById<Item>(opReferenceBalance.Item);
                    opReferenceBalance.ReferenceItemCode = item.ReferenceCode;
                    opReferenceBalance.ItemDescription = item.Description;
                    //User createUser = base.genericMgr.FindById<User>(opReferenceBalance.CreateUserId);
                    //opReferenceBalance.CreateUserName = createUser.FirstName + createUser.LastName;
                }
            }
            return PartialView(gridModel);
        }

        public void ExportXLS(OpReferenceBalanceSearchModel searchModel)
        {
            string sql = @" select top 65530 op.Item,i.RefCode,i.Desc1,op.OpRef,op.Qty,op.CreateDate,op.CreateUserNm,op.LastModifyDate,op.LastModifyUserNm from SCM_OpRefBalance  as op  with(nolock) 
 inner join MD_Item  as i with(nolock) on op.Item=i.Code  where 1=1 ";
            IList<object> param = new List<object>();

            if (!string.IsNullOrWhiteSpace(searchModel.ItemCode))
            {
                sql += " and  op.Item=?";
                param.Add(searchModel.ItemCode);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.OpReference))
            {
                sql += " and  op.OpRef=?";
                param.Add(searchModel.OpReference);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.CreateUserName))
            {
                sql += " and  op.CreateUserNm=?";
                param.Add(searchModel.CreateUserName);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.LastModifyUserName))
            {
                sql += " and  op.LastModifyUserNm=?";
                param.Add(searchModel.LastModifyUserName);
            }
            if (searchModel.CreateStartDate != null & searchModel.CreateEndDate != null)
            {
                sql += " and  op.CreateDate between ? and  ?";
                param.Add(searchModel.CreateStartDate);
                param.Add(searchModel.CreateEndDate);
            }
            else if (searchModel.CreateStartDate != null & searchModel.CreateEndDate == null)
            {
                sql += " and  op.CreateDate >=? ";
                param.Add(searchModel.CreateStartDate);
            }
            else if (searchModel.CreateStartDate == null & searchModel.CreateEndDate != null)
            {
                sql += " and  op.CreateDate <=?";
                param.Add(searchModel.CreateEndDate);
            }

            if (searchModel.ModifyStartDate != null & searchModel.ModifyEndDate != null)
            {
                sql += " and  op.LastModifyDate between ? and  ?";
                param.Add(searchModel.ModifyStartDate);
                param.Add(searchModel.ModifyEndDate);
            }
            else if (searchModel.ModifyStartDate != null & searchModel.ModifyEndDate == null)
            {
                sql += " and  op.LastModifyDate >=? ";
                param.Add(searchModel.ModifyStartDate);
            }
            else if (searchModel.ModifyStartDate == null & searchModel.ModifyEndDate != null)
            {
                sql += " and  op.LastModifyDate <=?";
                param.Add(searchModel.ModifyEndDate);
            }

            sql += " order by op.CreateDate desc";
            IList<object[]> searchList = this.genericMgr.FindAllWithNativeSql<object[]>(sql, param.ToArray());
            //op.Item,i.RefCode,i.Desc1,op.OpRef,op.Qty,op.CreateDate,op.CreateUserNm,op.LastModifyDate,op.LastModifyUserNm 
            IList<OpReferenceBalance> exportList = (from tak in searchList
                                                    select new OpReferenceBalance
                                           {
                                               Item = (string)tak[0],
                                               ReferenceItemCode = (string)tak[1],
                                               ItemDescription = (string)tak[2],
                                               OpReference = (string)tak[3],
                                               Qty = (decimal)tak[4],
                                               CreateDate = (DateTime)tak[5],
                                               CreateUserName = (string)tak[6],
                                               LastModifyDate = (DateTime)tak[7],
                                               LastModifyUserName = (string)tak[8],
                                           }).ToList();
            ExportToXLS<OpReferenceBalance>("ExporOpReferenceBalancetList", "xls", exportList);
        }


        #endregion

        #region 循环盘点

        public ActionResult Adjust()
        {
            return View();
        }


        [SconitAuthorize(Permissions = "Url_OpReferenceBalance_Adjust")]
        public JsonResult _Adjust(string prodLine, string traceCode, string item, string opReference, decimal? qty)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(prodLine) || string.IsNullOrWhiteSpace(traceCode) || string.IsNullOrWhiteSpace(item) || string.IsNullOrWhiteSpace(opReference) || qty == null || (qty.Value<=0))
                {
                    throw new BusinessException("路线代码,Van号，物料代码，工位，库存数不能为空。");
                }

                 base.genericMgr.FindAllWithNativeSql<object[]>("exec USP_Busi_UpdateOpRefBalance ?,?,?,?,?,?,?",
                    new object[] { prodLine,item,opReference, traceCode,qty.Value, CurrentUser.Id, CurrentUser.FullName },
                    new IType[] { NHibernateUtil.String, NHibernateUtil.String, NHibernateUtil.String, NHibernateUtil.String, NHibernateUtil.Decimal, NHibernateUtil.Int32, NHibernateUtil.String });
                 SaveSuccessMessage("调整成功。");
                 return Json(new { });
            }
            catch (BusinessException be)
            {
                SaveBusinessExceptionMessage(be);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.InnerException != null)
                    {
                        SaveErrorMessage(ex.InnerException.InnerException.Message);
                    }
                    else
                    {
                        SaveErrorMessage(ex.InnerException.Message);
                    }
                }
                else
                {
                    SaveErrorMessage(ex.Message);
                }
            }
            return Json(null);
        }
        #endregion

        #region 调整

        [SconitAuthorize(Permissions = "Url_OpReferenceBalance_ManualAdjust")]
        public ActionResult ManualAdjust()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OpReferenceBalance_ManualAdjust")]
        [GridAction]
        public ActionResult ManualAdjustList(GridCommand command, OpReferenceBalanceSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = 50;
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxManualAdjustList(GridCommand command, OpReferenceBalanceSearchModel searchModel)
        {
            if (!string.IsNullOrWhiteSpace(searchModel.successMessage))
            {
                SaveSuccessMessage(searchModel.successMessage);
            }
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            GridModel<OpReferenceBalance> gridModel = GetAjaxPageData<OpReferenceBalance>(searchStatementModel, command);
            if (gridModel.Data != null)
            {
                foreach (var opReferenceBalance in gridModel.Data)
                {
                    Item item = this.genericMgr.FindById<Item>(opReferenceBalance.Item);
                    opReferenceBalance.ReferenceItemCode = item.ReferenceCode;
                    opReferenceBalance.ItemDescription = item.Description;
                }
            }
            return PartialView(gridModel);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OpReferenceBalance_ManualAdjust")]
        public ActionResult ManualAdjustAction(int id, string currentAdjustQty)
        {
            try
            {
                decimal adjustQty = 0;
                if (string.IsNullOrWhiteSpace(currentAdjustQty))
                {
                    throw new BusinessException("数量不能为空。");
                }

                try
                {
                    adjustQty = Convert.ToDecimal(currentAdjustQty);
                }
                catch (Exception)
                {

                    throw new BusinessException(string.Format("调整数{0}填写有误。",currentAdjustQty));
                }
                OpReferenceBalance op = this.genericMgr.FindById<OpReferenceBalance>(id);
                op.Qty = op.Qty + adjustQty;
                stockTakeMgr.UpdateOpReferenceBalance(op);

                SaveSuccessMessage(string.Format("调整成功。"));
                return new RedirectToRouteResult(new RouteValueDictionary  
                                                       { 
                                                           { "action", "ManualAdjustList" }, 
                                                           { "controller", "OpReferenceBalance" },
                                                           {"successMessage","调整成功。"}
                                                       });
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }
            return Json(null);
        }

        [SconitAuthorize(Permissions = "Url_OpReferenceBalance_Stock")]
        public ActionResult ImportAdjustXls(IEnumerable<HttpPostedFileBase> attachments)
        {
            try
            {
                foreach (var file in attachments)
                {
                    stockTakeMgr.ImportOpReferenceBalanceAdjustXls(file.InputStream);
                    SaveSuccessMessage("全部导入成功！");
                }
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

        #region 盘点

        [SconitAuthorize(Permissions = "Url_OpReferenceBalance_Stock")]
        public ActionResult Stock()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OpReferenceBalance_Stock")]
        [GridAction]
        public ActionResult StockList(GridCommand command, OpReferenceBalanceSearchModel searchModel)
        {
            TempData["GridCommand"] = command;
            TempData["searchModel"] = searchModel;
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = this.ProcessPageSize(command.PageSize);
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OpReferenceBalance_Stock")]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxStockList(GridCommand command, OpReferenceBalanceSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            GridModel<OpReferenceBalance> gridModel = GetAjaxPageData<OpReferenceBalance>(searchStatementModel, command);
            if (gridModel.Data != null)
            {
                foreach (var opReferenceBalance in gridModel.Data)
                {
                    Item item = this.genericMgr.FindById<Item>(opReferenceBalance.Item);
                    opReferenceBalance.ReferenceItemCode = item.ReferenceCode;
                    opReferenceBalance.ItemDescription = item.Description;
                }
            }
            return PartialView(gridModel);
        }


        [GridAction]
        [SconitAuthorize(Permissions = "Url_OpReferenceBalance_Stock")]
        public ActionResult _Insert(OpReferenceBalance opReferenceBalance)
        {
            if (CheckOpReferenceBalance(opReferenceBalance))
            {
                try
                {
                    stockTakeMgr.CreateOpReferenceBalance(opReferenceBalance);
                    SaveSuccessMessage("添加成功。");
                }
                catch (Exception e)
                {
                    SaveErrorMessage("添加失败。"+e.Message);
                }
                
            }
            GridCommand command = (GridCommand)TempData["GridCommand"];
            OpReferenceBalanceSearchModel searchModel = (OpReferenceBalanceSearchModel)TempData["searchModel"];
            TempData["GridCommand"] = command;
            TempData["searchModel"] = searchModel;
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            GridModel<OpReferenceBalance> gridModel = GetAjaxPageData<OpReferenceBalance>(searchStatementModel, command);
            if (gridModel.Data != null)
            {
                foreach (var opref in gridModel.Data)
                {
                    Item item = this.genericMgr.FindById<Item>(opref.Item);
                    opref.ReferenceItemCode = item.ReferenceCode;
                    opref.ItemDescription = item.Description;
                }
            }
            return PartialView(gridModel);
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OpReferenceBalance_Stock")]
        public ActionResult _Update(OpReferenceBalance opReferenceBalance, string id)
        {
            if (CheckOpReferenceBalance(opReferenceBalance))
            {
                try
                {
                    OpReferenceBalance upOpReferenceBalance = base.genericMgr.FindById<OpReferenceBalance>(Convert.ToInt32(id));
                    upOpReferenceBalance.Item = opReferenceBalance.Item;
                    upOpReferenceBalance.OpReference = opReferenceBalance.OpReference;
                    upOpReferenceBalance.Qty = opReferenceBalance.Qty;
                    upOpReferenceBalance.Version = opReferenceBalance.Version;
                    stockTakeMgr.UpdateOpReferenceBalance(upOpReferenceBalance);
                    SaveSuccessMessage("修改成功。");
                }
                catch (Exception e)
                {
                    SaveErrorMessage("修改失败。" + e.Message);
                }
            }
            GridCommand command = (GridCommand)TempData["GridCommand"];
            OpReferenceBalanceSearchModel searchModel = (OpReferenceBalanceSearchModel)TempData["searchModel"];
            TempData["GridCommand"] = command;
            TempData["searchModel"] = searchModel;
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            GridModel<OpReferenceBalance> gridModel = GetAjaxPageData<OpReferenceBalance>(searchStatementModel, command);
            if (gridModel.Data != null)
            {
                foreach (var opref in gridModel.Data)
                {
                    Item item = this.genericMgr.FindById<Item>(opref.Item);
                    opref.ReferenceItemCode = item.ReferenceCode;
                    opref.ItemDescription = item.Description;
                }
            }
            return PartialView(gridModel);
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OpReferenceBalance_Stock")]
        public ActionResult _Delete(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<OpReferenceBalance>(Convert.ToInt32(Id));
                SaveSuccessMessage("删除成功。");
            }
            GridCommand command = (GridCommand)TempData["GridCommand"];
            OpReferenceBalanceSearchModel searchModel = (OpReferenceBalanceSearchModel)TempData["searchModel"];
            TempData["GridCommand"] = command;
            TempData["searchModel"] = searchModel;
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            GridModel<OpReferenceBalance> gridModel = GetAjaxPageData<OpReferenceBalance>(searchStatementModel, command);
            if (gridModel.Data != null)
            {
                foreach (var opref in gridModel.Data)
                {
                    Item item = this.genericMgr.FindById<Item>(opref.Item);
                    opref.ReferenceItemCode = item.ReferenceCode;
                    opref.ItemDescription = item.Description;
                }
            }
            return PartialView(gridModel);
        }


        [SconitAuthorize(Permissions = "Url_OpReferenceBalance_Stock")]
        public ActionResult ImportStockXls(IEnumerable<HttpPostedFileBase> attachments)
        {
            try
            {
                foreach (var file in attachments)
                {
                    stockTakeMgr.ImportOpReferenceBalanceStockXls(file.InputStream);
                    SaveSuccessMessage("全部导入成功！");
                }
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                SaveSuccessMessage("其它行导入成功！");
            }
            catch (Exception ex)
            {
                SaveErrorMessage("导入失败。 - " + ex.Message);
            }

            return Content(string.Empty);
        }


        private bool CheckOpReferenceBalance(OpReferenceBalance opReferenceBalance)
        {
            bool hasError = false;
            if (string.IsNullOrWhiteSpace(opReferenceBalance.Item))
            {
                hasError = true;
                SaveErrorMessage("物料编号不能为空。");
            }
            if (string.IsNullOrWhiteSpace(opReferenceBalance.OpReference))
            {
                hasError = true;
                SaveErrorMessage("工位不能为空。");
            }
            if (opReferenceBalance.Qty < 0)
            {
                hasError = true;
                SaveErrorMessage("库存数不能小于0。");
            }
            if (this.genericMgr.FindAllWithNativeSql<int>(" select isnull(count(*),0) as counts from SCM_OpRefBalance where Item=? and OpRef=? and Id <>? ", new object[] { opReferenceBalance.Item, opReferenceBalance.OpReference, opReferenceBalance.Id }, new IType[] { NHibernate.NHibernateUtil.String, NHibernate.NHibernateUtil.String, NHibernate.NHibernateUtil.Int32 })[0] > 0)
            {
                hasError = true;
                SaveErrorMessage("物料编号+工位已经维护，请确认！");
            }
            return !hasError;
        }

        #endregion


        #region private
        private SearchStatementModel PrepareSearchStatement(GridCommand command, OpReferenceBalanceSearchModel searchModel)
        {
            string whereStatement = " where 1=1 ";

            IList<object> param = new List<object>();
            HqlStatementHelper.AddEqStatement("Item", searchModel.ItemCode, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("OpReference", searchModel.OpReference, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("CreateUserName", searchModel.CreateUserName, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("LastModifyUserName", searchModel.LastModifyUserName, "l", ref whereStatement, param);

            if (searchModel.CreateStartDate != null & searchModel.CreateEndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.CreateStartDate, searchModel.CreateEndDate, "l", ref whereStatement, param);
            }
            else if (searchModel.CreateStartDate != null & searchModel.CreateEndDate == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.CreateStartDate, "l", ref whereStatement, param);
            }
            else if (searchModel.CreateStartDate == null & searchModel.CreateEndDate != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.CreateEndDate, "l", ref whereStatement, param);
            }

            if (searchModel.ModifyStartDate != null & searchModel.ModifyEndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("LastModifyDate", searchModel.ModifyStartDate, searchModel.ModifyEndDate, "l", ref whereStatement, param);
            }
            else if (searchModel.ModifyStartDate != null & searchModel.ModifyEndDate == null)
            {
                HqlStatementHelper.AddGeStatement("LastModifyDate", searchModel.ModifyStartDate, "l", ref whereStatement, param);
            }
            else if (searchModel.ModifyStartDate == null & searchModel.ModifyEndDate != null)
            {
                HqlStatementHelper.AddLeStatement("LastModifyDate", searchModel.ModifyStartDate, "l", ref whereStatement, param);
            }

            string sortingStatement = " order by l.CreateDate desc";


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
