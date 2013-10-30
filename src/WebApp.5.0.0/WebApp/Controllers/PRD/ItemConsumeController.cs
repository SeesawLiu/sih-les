/// <summary>
/// Summary description for LocationController
/// </summary>

using System;

namespace com.Sconit.Web.Controllers.PRD
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Security;
    using com.Sconit.Entity.PRD;
    using com.Sconit.Entity.SYS;
    using com.Sconit.Service;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Models.SearchModels.PRD;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;
    using com.Sconit.Web.Models.SearchModels.CUST;
    using com.Sconit.Entity.CUST;
    using com.Sconit.Entity.MD;
    using com.Sconit.Web.Models.SearchModels.MD;
    using System.Web;
    using System.IO;
    using com.Sconit.Entity.Exception;
    using Castle.Services.Transaction;
    using NPOI.HSSF.UserModel;
    using NPOI.SS.UserModel;
    using com.Sconit.Utility;
    using com.Sconit.Entity;

    /// <summary>
    /// This controller response to control the Routing.
    /// </summary>
    public class ItemConsumeController : WebAppBaseController
    {

        /// <summary>
        /// hql to get count of the RoutingMaster
        /// </summary>
        private static string selectCountStatement = "select count(*) from ItemConsume  i";

        /// <summary>
        /// hql to get all of the RoutingMaster
        /// </summary>
        private static string selectStatement = "select i from ItemConsume as i";


        /// Index action for Routing controller
        /// </summary>
        /// <returns>Index view</returns>
        [SconitAuthorize(Permissions = "Url_ItemConsume_View")]
        public ActionResult Index()
        {
            return View();
        }



      
        [GridAction]
        [SconitAuthorize(Permissions = "Url_ItemConsume_View")]
        public ActionResult List(GridCommand command, ItemConsumeSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ItemConsume_View")]
        public ActionResult _AjaxList(GridCommand command, ItemConsumeSearchModel searchModel)
        {
            TempData["GridCommand"] = command;
            TempData["searchModel"] = searchModel;
            return _GetReturnList();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_ItemConsume_View")]
        public ActionResult _Insert(ItemConsume itemConsume)
        {
            try
            {
                if (itemConsume.Qty < 0)
                {
                    throw new BusinessException("数量不能小于0。");
                }
                if (string.IsNullOrWhiteSpace(itemConsume.Item))
                {
                    throw new BusinessException("物料编号不能为空。");
                }
                else
                {
                    var item = this.genericMgr.FindById<Item>(itemConsume.Item);
                    itemConsume.ItemDesc = item.Description;
                    itemConsume.RefItemCode = item.ReferenceCode;
                }
                this.genericMgr.Create(itemConsume);
                SaveSuccessMessage("添加成功。");
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex.Message);
            }
            return _GetReturnList();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_ItemConsume_View")]
        public ActionResult _Update(ItemConsume itemConsume, string id)
        {
            try
            {
                if (itemConsume.Qty < 0)
                {
                    throw new BusinessException("数量不能小于0。");
                }
                if (itemConsume.ConsumedQty > 0)
                {
                    throw new BusinessException("已经消耗的不能删除。");
                }
                //只能改数量
                ItemConsume updateItemConsume = this.genericMgr.FindById<ItemConsume>(itemConsume.Id);
                updateItemConsume.Qty = itemConsume.Qty;
                this.genericMgr.Update(updateItemConsume);
                SaveSuccessMessage("修改成功。");
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex.Message);
            }
            return _GetReturnList();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_ItemConsume_View")]
        public ActionResult _Delete(string id)
        {
            try
            {
                ItemConsume itemConsume = this.genericMgr.FindById<ItemConsume>(Convert.ToInt32(id));

                if (itemConsume.ConsumedQty > 0)
                {
                    throw new BusinessException("已经消耗的不能删除。");
                }
                this.genericMgr.Delete(itemConsume);
                SaveSuccessMessage("删除成功。");
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex.Message);
            }
            return _GetReturnList();
        }

        public ActionResult _GetReturnList()
        {
            GridCommand command = (GridCommand)TempData["GridCommand"];
            ItemConsumeSearchModel searchModel = (ItemConsumeSearchModel)TempData["searchModel"];
            TempData["GridCommand"] = command;
            TempData["searchModel"] = searchModel;
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            GridModel<ItemConsume> gridModel = GetAjaxPageData<ItemConsume>(searchStatementModel, command);
            return PartialView(gridModel);
        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, ItemConsumeSearchModel searchModel)
        {
            string whereStatement = " where 1=1 ";
            if (searchModel.IsClose)
            {
                    whereStatement += " and  i.Qty<=i.ConsumedQty";
             }
            else{
                    whereStatement += " and  i.Qty>i.ConsumedQty";
            }

            IList<object> param = new List<object>();

            HqlStatementHelper.AddEqStatement("SourceType", searchModel.SourceType, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.ItemCode, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("PONo", searchModel.PONo, "i", ref whereStatement, param);
            //HqlStatementHelper.AddEqStatement("IsClose", searchModel.IsClose, "i", ref whereStatement, param);

            if (searchModel.EffFrom != null & searchModel.EffTo != null)
            {
                searchModel.EffTo = searchModel.EffTo.Value.AddDays(1).AddSeconds(-1);
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.DateFrom, searchModel.DateTo, "i", ref whereStatement, param);
            }
            else if (searchModel.EffFrom != null & searchModel.DateFrom == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.DateFrom, "i", ref whereStatement, param);
            }
            else if (searchModel.EffFrom == null & searchModel.DateTo != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.DateTo, "i", ref whereStatement, param);
            }

           

           
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            if (string.IsNullOrWhiteSpace(sortingStatement))
            {
                sortingStatement = " order by  CreateDate desc";
            }
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }


        //public FileStreamResult ExportCSV(ItemConsumeSearchModel searchModel)
        //{
        //    string hql = " from ItemConsume as l where 1=1";
        //    if (!string.IsNullOrEmpty(searchModel.SourceType))
        //    {
        //        hql += string.Format(" and l.SourceType='{0}'", searchModel.SourceType);
        //    }
        //    if (!string.IsNullOrEmpty(searchModel.Item))
        //    {
        //        hql += string.Format(" and l.Item='{0}'", searchModel.Item);
        //    }
        //    if (!string.IsNullOrEmpty(searchModel.PONo))
        //    {
        //        hql += string.Format(" and l.PONo='{0}'", searchModel.PONo);
        //    }
        //    if (searchModel.IsClose.HasValue && searchModel.IsClose.Value)
        //    {
        //        hql += string.Format(" and l.IsClose={0}", searchModel.IsClose.Value ? "1" : "0");
        //    }
        //    //if (searchModel.IsConsumed)
        //    //{
        //    //    hql += " and  l.Qty=l.ConsumedQty";
        //    //}
        //    if (searchModel.IsConsume != null)
        //    {
        //        if (searchModel.IsConsume.Value == 1)
        //        {
        //            hql += " and  l.Qty=l.ConsumedQty";
        //        }
        //        else
        //        {
        //            hql += " and  l.Qty>l.ConsumedQty";
        //        }
        //    }

        //    if (searchModel.EffFrom != null & searchModel.EffTo != null)
        //    {
        //        searchModel.EffTo = searchModel.EffTo.Value.AddDays(1).AddSeconds(-1);
        //        hql += string.Format(" and l.EffectiveDate between '{0}' and '{1}' ", searchModel.EffFrom.Value.ToString(), searchModel.EffTo.Value.ToString());
        //    }
        //    else if (searchModel.EffFrom != null & searchModel.EffTo == null)
        //    {
        //        hql += string.Format(" and l.EffectiveDate >= '{0}' ", searchModel.EffFrom.Value.ToString());
        //    }
        //    else if (searchModel.EffFrom == null & searchModel.EffTo != null)
        //    {
        //        hql += string.Format(" and l.EffectiveDate <= '{0}' ", searchModel.EffTo.Value.ToString());
        //    }

        //    string maxRows = this.systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.ExportMaxRows);
        //    IList<ItemConsume> itemConsumes = this.GeneMgr.FindAll<ItemConsume>(hql, 0, int.Parse(maxRows));
        //    base.FillCodeDetailDescription(itemConsumes);
        //    return ExportToCSV<ItemConsume>("ItemConsume", "csv", itemConsumes);
        //}


    }
}

