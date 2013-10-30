using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using com.Sconit.Web.Util;
using Telerik.Web.Mvc;
using com.Sconit.Web.Models.SearchModels.MD;
using com.Sconit.Web.Models;
using com.Sconit.Entity.MD;
using com.Sconit.Service;

namespace com.Sconit.Web.Controllers.MD
{
    public class ItemDiscontinueController : WebAppBaseController
    {
        /// <summary>
        /// hql to get count of the ItemDiscontinue 
        /// </summary>
        private static string ItemDiscontinueCountStatement = "select count(*) from ItemDiscontinue as i";
                                                            
        /// <summary>
        /// hql for ItemDiscontinue
        /// </summary>
        private static string selectStatement = "select i from ItemDiscontinue as i";


        /// <summary>
        /// hql to get count of the ItemDiscontinue by ItemDiscontinue's code
        /// </summary>
        private static string duiplicateVerifyStatement = @"select count(*) from ItemDiscontinue as i where i.Item = ? and i.DiscontinueItem=? and i.UnitQty=? and i.Priority=? and i.StartDate=?";


        [SconitAuthorize(Permissions = "Url_ItemDiscontinue_View")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// List action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">ItemDiscontinue Search model</param>
        /// <returns>return the result view</returns>
        [GridAction]
        [SconitAuthorize(Permissions = "Url_ItemDiscontinue_View")]
        public ActionResult List(GridCommand command, ItemDiscontinueSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        /// <summary>
        /// AjaxList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">ItemDiscontinue Search Model</param>
        /// <returns>return the result Model</returns>
       [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ItemDiscontinue_View")]
        public ActionResult _AjaxList(GridCommand command, ItemDiscontinueSearchModel searchModel)
        {
                SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
                 return PartialView(GetAjaxPageData<ItemDiscontinue>(searchStatementModel, command));

            
        }

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">ItemDiscontinueSearchModel Search Model</param>
        /// <returns>return Search Statement</returns>
        private SearchStatementModel PrepareSearchStatement(GridCommand command, ItemDiscontinueSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();
            
            HqlStatementHelper.AddLikeStatement("Item", searchModel.Item, HqlStatementHelper.LikeMatchMode.End, "i", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("DiscontinueItem", searchModel.DiscontinueItem, HqlStatementHelper.LikeMatchMode.End, "i", ref whereStatement, param);
            if (searchModel.StartDate != null )
            {
                HqlStatementHelper.AddGeStatement("StartDate", searchModel.StartDate, "i", ref whereStatement, param);
            }
             if (searchModel.EndDate != null )
            {
                HqlStatementHelper.AddLeStatement("EndDate", searchModel.EndDate, "i", ref whereStatement, param);
            }
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = ItemDiscontinueCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <returns>rediret view</returns>
       // [SconitAuthorize(Permissions = "Url_ItemDiscontinue_Edit")]
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
        [SconitAuthorize(Permissions = "Url_ItemDiscontinue_View")]
        public ActionResult New(ItemDiscontinue itemdiscon)
        {
            if (ModelState.IsValid)
            {
                if (base.genericMgr.FindAll<long>(duiplicateVerifyStatement, new object[] {itemdiscon.Item,itemdiscon.DiscontinueItem,itemdiscon.UnitQty,itemdiscon.Priority,itemdiscon.StartDate })[0] > 0)
                {
                    SaveErrorMessage("已经存在");
                }
                else if (itemdiscon.EndDate!=null && System.DateTime.Compare(System.DateTime.Parse(itemdiscon.EndDate.ToString()), System.DateTime.Parse(itemdiscon.StartDate.ToString())) < 1)
                {
                    SaveErrorMessage(Resources.MD.ItemDiscontinue.Errors_StartDateGreaterThanEndDate);
                }
                else if(itemdiscon.Item==null || itemdiscon.DiscontinueItem==null)
                {
                    SaveErrorMessage(Resources.MD.ItemDiscontinue.Error_ItemDiscontinueNotBeEmpty);
                }
                else if (itemdiscon.Item.ToString() == itemdiscon.DiscontinueItem.ToString() )
                {
                    SaveErrorMessage(Resources.MD.ItemDiscontinue.Error_ItemCannotEqualDiscontinueItem);
                }
                else
                {
                    base.genericMgr.Create(itemdiscon);
                    SaveSuccessMessage(Resources.MD.ItemDiscontinue.ItemDiscontinue_Added);
                    return RedirectToAction("Edit/" + itemdiscon.Id);
                }    
            }
            TempData["ItemDiscontinue"] = itemdiscon;
            return View(itemdiscon);
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ItemDiscontinue_View")]
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            ItemDiscontinue itemdiscon = base.genericMgr.FindById<ItemDiscontinue>(int.Parse(id));
            return View(itemdiscon);
        }

        public ActionResult _UpdateSave(ItemDiscontinue itemDiscon) {
            if (ModelState.IsValid)
            {
                if (itemDiscon.EndDate != null && System.DateTime.Compare(System.DateTime.Parse(itemDiscon.EndDate.ToString()), System.DateTime.Parse(itemDiscon.StartDate.ToString())) < 1)
                {
                    SaveErrorMessage(Resources.MD.ItemDiscontinue.Errors_StartDateGreaterThanEndDate);
                }
                else
                {
                    base.genericMgr.Update(itemDiscon);
                    SaveSuccessMessage(Resources.MD.ItemDiscontinue.ItemDiscontinue_Updated);
                    return RedirectToAction("List");
                }
            }
            return RedirectToAction("Edit/" + itemDiscon.Id);
            
        }
    }
}
