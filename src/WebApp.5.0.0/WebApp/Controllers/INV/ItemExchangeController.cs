using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using com.Sconit.Web.Util;
using Telerik.Web.Mvc;
using com.Sconit.Web.Models.SearchModels.INV;
using com.Sconit.Web.Models;
using com.Sconit.Service;
using com.Sconit.Entity.INV;
using com.Sconit.Utility;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.MD;

namespace com.Sconit.Web.Controllers.INV
{
    public class ItemExchangeController : WebAppBaseController
    {

        //
        // GET: /ItemExchange/

        #region Properties
        public ILocationDetailMgr LocDetGeneMgr { get; set; }

        public IItemMgr itemMgr { get; set; }
        #endregion

        #region hql
        /// <summary>
        /// hql to get count of the ItemExchange 
        /// </summary>
        private static string ItemExchangeCountStatement = "select count(*) from ItemExchange as i";

        /// <summary>
        /// hql for ItemExchange
        /// </summary>
        private static string selectStatement = "select i from ItemExchange as i";


        #endregion

        #region public
        [SconitAuthorize(Permissions = "Url_Inventory_ItemExchange_View")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// List action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">ItemExchange Search model</param>
        /// <returns>return the result view</returns>
        [GridAction]
        [SconitAuthorize(Permissions = "Url_Inventory_ItemExchange_View")]
        public ActionResult List(GridCommand command, ItemExchangeSearchModel searchModel)
        {
            TempData["ItemExchangeSearchModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        /// <summary>
        /// AjaxList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">ItemExchange Search Model</param>
        /// <returns>return the result Model</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Inventory_ItemExchange_View")]
        public ActionResult _AjaxList(GridCommand command, ItemExchangeSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<ItemExchange>(searchStatementModel, command));
        }


        #region  New
        /// <summary>
        /// New action
        /// </summary>
        /// <returns>rediret view</returns>
        [SconitAuthorize(Permissions = "Url_Inventory_ItemExchange_New")]
        public ActionResult New()
        {
            return View();
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <param name="item">Item model</param>
        /// <returns>return to Edit action </returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_Inventory_ItemExchange_New")]
        public ActionResult New(ItemExchange itemExchange)
        {
            if (ModelState.IsValid)
            {
                IList<ItemExchange> itemExchangeList = new List<ItemExchange>();

                Item itemFrom = base.genericMgr.FindById<Item>(itemExchange.ItemFrom);
                itemExchange.BaseUom = itemFrom.Uom;
                if (itemExchange.Uom != itemFrom.Uom)
                {
                    itemExchange.UnitQty = itemMgr.ConvertItemUomQty(itemExchange.ItemFrom, itemFrom.Uom, 1, itemExchange.Uom);
                }
                else
                {
                    itemExchange.UnitQty = 1;
                }

                itemExchangeList.Add(itemExchange);
                try
                {
                    LocDetGeneMgr.InventoryExchange(itemExchangeList);
                    SaveSuccessMessage(Resources.INV.ItemExchange.ItemExchange_Added);
                    return RedirectToAction("Edit/" + itemExchange.Id);
                }
                catch (BusinessException ex)
                {
                    SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
                }
            }
            return View(itemExchange);
        }
        #endregion
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_Inventory_ItemExchange_View")]
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            ItemExchange itemExchange = base.genericMgr.FindById<ItemExchange>(int.Parse(id));
            return View(itemExchange);
        }

        [SconitAuthorize(Permissions = "Url_Inventory_ItemExchange_Cancel")]
        public ActionResult Cancel(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            else
            {
                ItemExchange itemExchange = base.genericMgr.FindById<ItemExchange>(int.Parse(id));
                IList<ItemExchange> itemExchangeList = new List<ItemExchange>();
                itemExchangeList.Add(itemExchange);
                LocDetGeneMgr.CancelInventoryExchange(itemExchangeList);
                SaveSuccessMessage(Resources.INV.ItemExchange.ItemExchange_Canceled);
                return RedirectToAction("Edit/" + id);
            }

        }

        #endregion
        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">ItemDiscontinueSearchModel Search Model</param>
        /// <returns>return Search Statement</returns>
        private SearchStatementModel PrepareSearchStatement(GridCommand command, ItemExchangeSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();
            SecurityHelper.AddRegionPermissionStatement(ref whereStatement, "i", searchModel.RegionFrom);
            SecurityHelper.AddRegionPermissionStatement(ref whereStatement, "i", searchModel.RegionFrom);

            HqlStatementHelper.AddLikeStatement("ItemFrom", searchModel.ItemFrom, HqlStatementHelper.LikeMatchMode.End, "i", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("ItemTo", searchModel.ItemTo, HqlStatementHelper.LikeMatchMode.End, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("RegionFrom", searchModel.RegionFrom, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("RegionTo", searchModel.RegionTo, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("LocationFrom", searchModel.LocationFrom, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("LocationTo", searchModel.LocationTo, "i", ref whereStatement, param);
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = ItemExchangeCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

    }
}

