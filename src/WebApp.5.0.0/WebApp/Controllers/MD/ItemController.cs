/// <summary>
/// Summary description for AddressController
/// </summary>
namespace com.Sconit.Web.Controllers.MD
{
    #region reference
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Security;
    using com.Sconit.Entity.MD;
    using com.Sconit.Entity.PRD;
    using com.Sconit.Entity.SYS;
    using com.Sconit.Service;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Models.SearchModels.MD;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;
    using NHibernate.Type;
    using com.Sconit.Entity;
    using System.Web;
    using com.Sconit.Entity.Exception;
    using System;
    #endregion

    /// <summary>
    /// This controller response to control the Item.
    /// </summary>
    public class ItemController : WebAppBaseController
    {
        #region Properties
        public IItemMgr itemMgr { get; set; }
        #endregion

        #region hql
        /// <summary>
        /// hql to get count of the Item 
        /// </summary>
        private static string selectCountStatement = "select count(*) from Item as i";

        /// <summary>
        /// hql to get all of the Item
        /// </summary>
        private static string selectStatement = "select i from Item as i";

        /// <summary>
        /// hql to get count of the Item by Item's code
        /// </summary>
        private static string duiplicateVerifyStatement = @"select count(*) from Item as i where i.Code = ?";

        /// <summary>
        /// hql to get count of the Item 
        /// </summary>
        private static string selectItemRefCountStatement = "select count(*) from ItemReference as i";

        /// <summary>
        /// hql to get all of the Item
        /// </summary>
        private static string selectItemRefStatement = "select i from ItemReference as i";

        /// <summary>
        /// hql to get count of the Item by Item's code
        /// </summary>
        private static string itemRefDuiplicateVerifyStatement = @"select count(*) from ItemReference as i where i.Id = ?";

        /// <summary>
        /// hql for ItemReference
        /// </summary>
        private static string itemAndPartyDuiplicateVerifyStatement = @"select count(*) from ItemReference as i where i.Item = ? and i.Party= ?";

        /// <summary>
        /// hql for ItemReference
        /// </summary>
        private static string itemAndPartyIsNullDuiplicateVerifyStatement = @"select count(*) from ItemReference as i where i.Item = ? and i.Party is null";

        /// <summary>
        /// hql
        /// </summary>
        private static string selectItemPackageCountStatement = "select count(*) from ItemPackage as i";

        /// <summary>
        /// hql
        /// </summary>
        private static string selectItemPackageStatement = "select i from ItemPackage as i";

        /// <summary>
        /// hql 
        /// </summary>
        private static string itemPackageDuiplicateVerifyStatement = @"select count(*) from ItemPackage as i where i.Id = ?";

        /// <summary>
        /// hql 
        /// </summary>
        private static string itemAndUnitCountIsExistStatement = @"select count(*) from ItemPackage as i where i.Item = ? and i.UnitCount = ?";
        #endregion

        #region Item
        /// <summary>
        /// Index action for Item controller
        /// </summary>
        /// <returns>rediret view</returns>
        [SconitAuthorize(Permissions = "Url_Item_View")]
        public ActionResult Index()
        {
            ViewBag.HaveEditPermission = CurrentUser.Permissions.Where(p => p.PermissionCode == "Url_Item_Edit").Count() > 0;
            return View();
        }

        /// <summary>
        /// List action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">ItemCategory Search model</param>
        /// <returns>return the result view</returns>
        [GridAction]
        [SconitAuthorize(Permissions = "Url_Item_View")]
        public ActionResult List(GridCommand command, ItemSearchModel searchModel)
        {
            ViewBag.HaveEditPermission = CurrentUser.Permissions.Where(p => p.PermissionCode == "Url_Item_Edit").Count() > 0;
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        /// <summary>
        /// AjaxList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">ItemCategory Search Model</param>
        /// <returns>return the result Model</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Item_View")]
        public ActionResult _AjaxList(GridCommand command, ItemSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<Item>(searchStatementModel, command));
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <returns>rediret view</returns>
        [SconitAuthorize(Permissions = "Url_Item_Edit")]
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
        [SconitAuthorize(Permissions = "Url_Item_Edit")]
        public ActionResult New(Item item)
        {
            bool canSave = true;
            if (ModelState.IsValid)
            {

                if (base.genericMgr.FindAll<long>(duiplicateVerifyStatement, new object[] { item.Code })[0] > 0)
                {
                    canSave = false;
                    SaveErrorMessage(Resources.ErrorMessage.Errors_Existing_Code, item.Code);

                }

                if (item.Bom != null)
                {
                    if (base.genericMgr.FindAll<BomMaster>("from BomMaster where Code= ? and IsActive = ? ", new object[] { item.Bom, true }).Count == 0)
                    {
                        canSave = false;
                        SaveErrorMessage(Resources.MD.Item.ItemErrors_NotExisting_Bom);
                    }
                }
                if (item.Routing != null)
                {
                    if (base.genericMgr.FindAll<RoutingMaster>("from RoutingMaster where Code= ? and IsActive = ? ", new object[] { item.Routing, true }).Count == 0)
                    {
                        canSave = false;
                        SaveErrorMessage(Resources.MD.Item.ItemErrors_NotExisting_Routing);
                    }
                }
                if (item.Location != null)
                {
                    if (base.genericMgr.FindAll<Location>("from Location where Code= ? and IsActive = ? ", new object[] { item.Location, true }).Count == 0)
                    {
                        canSave = false;
                        SaveErrorMessage(Resources.MD.Item.ItemErrors_NotExisting_Location);
                    }
                }
                if (canSave)
                {
                    itemMgr.CreateItem(item);
                    SaveSuccessMessage(Resources.MD.Item.Item_Added);
                    return RedirectToAction("_EditList/" + item.Code);
                }
            }

            return View(item);
        }

        /// <summary>
        /// Edit action
        /// </summary>
        /// <param name="id">Item id for edit</param>
        /// <returns>return the result view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_Item_Edit")]
        public ActionResult _EditList(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                return View("_EditList", string.Empty, id);
            }
        }

        /// <summary>
        /// Edit action
        /// </summary>
        /// <param name="id">Item id for edit</param>
        /// <returns>return the result view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_Item_Edit")]
        public ActionResult Edit(string id)
        {

            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                Item item = base.genericMgr.FindById<Item>(id);
                return PartialView(item);
            }
        }

        /// <summary>
        /// Edit action
        /// </summary>
        /// <param name="item">Item model</param>
        /// <returns>return the result view</returns>
        [SconitAuthorize(Permissions = "Url_Item_Edit")]
        public ActionResult Edit(Item item, string oldContainer, decimal oldMinUnitCount, string oldContainerDesc)
        {
            if (ModelState.IsValid)
            {
                bool canSave = true;
                if (item.Bom != null)
                {
                    if (base.genericMgr.FindAll<BomMaster>("from BomMaster where Code= ? and IsActive = ? ", new object[] { item.Bom, true }).Count == 0)
                    {
                        canSave = false;
                        SaveErrorMessage(Resources.MD.Item.ItemErrors_NotExisting_Bom);
                    }
                }
                if (item.Routing != null)
                {
                    if (base.genericMgr.FindAll<RoutingMaster>("from RoutingMaster where Code= ? and IsActive = ? ", new object[] { item.Routing, true }).Count == 0)
                    {
                        canSave = false;
                        SaveErrorMessage(Resources.MD.Item.ItemErrors_NotExisting_Routing);
                    }
                }
                if (item.Location != null)
                {
                    if (base.genericMgr.FindAll<Location>("from Location where Code= ? and IsActive = ? ", new object[] { item.Location, true }).Count == 0)
                    {
                        canSave = false;
                        SaveErrorMessage(Resources.MD.Item.ItemErrors_NotExisting_Location);
                    }
                }
                if (canSave)
                {
                    string hql = string.Empty;
                    //string hqlkb = string.Empty;
                    string hqlProc= string.Empty;
                    IList<object> paras = new List<object>();
                    //IList<object> paraskb = new List<object>();
                    IList<object> parasProc = new List<object>();
                    if (item.Container != oldContainer || item.MinUnitCount != oldMinUnitCount)
                    {
                        if (!string.IsNullOrEmpty(item.Container) && item.Container != oldContainer)
                        {
                            //hqlkb = hqlkb + "update from KanbanCard set Container = ? ";
                            hql = hql + "update from FlowDetail set Container = ? ";
                            hqlProc = hqlProc + "update from FlowDetail set Container = ? ";
                            paras.Add(item.Container);
                            //paraskb.Add(item.Container);
                            parasProc.Add(item.Container);
                        }
                        if (item.MinUnitCount!=0 && item.MinUnitCount != oldMinUnitCount)
                        {
                            if (string.IsNullOrEmpty(hql))
                            {
                                //hqlkb = hqlkb + "update from KanbanCard set Qty = ? ";
                                hql = hql + "update from FlowDetail set MinUnitCount = ?,UnitCount = ?";
                                hqlProc = hqlProc + "update from FlowDetail set MinUnitCount = ?";
                            }
                            else
                            {
                                //hqlkb = hqlkb + ", Qty = ? ";
                                hql = hql + ", MinUnitCount = ?,UnitCount = ? ";
                                hqlProc = hqlProc + ", MinUnitCount = ? ";
                            }
                            paras.Add(item.MinUnitCount);
                            paras.Add(item.MinUnitCount);
                            parasProc.Add(item.MinUnitCount);
                            //paraskb.Add(item.MinUnitCount);
//                            this.genericMgr.FindAllWithNativeSql(@"update fd set fd.UC=? from SCM_FlowDet fd inner join SCM_FlowMstr fm 
//                                        on fd.Flow=fm.Code where fd.Item=? and fm.Type=2", new object[] { item.MinUnitCount, item.Code });
                        }
                        if (!string.IsNullOrEmpty(item.ContainerDesc) && item.ContainerDesc != oldContainerDesc)
                        {
                            if (string.IsNullOrEmpty(hql))
                            {
                                hql = hql + "update from FlowDetail set ContainerDesc = ?";
                            }
                            else
                            {
                                hql = hql + ", ContainerDesc = ? ";
                            }
                            paras.Add(item.ContainerDesc);
                        }
                        if (hql != string.Empty && hql != null)
                        {
                            hql = hql + "where Item=? and exists( select 1 from FlowMaster as m where m.Code=Flow and m.Type=2 and exists( select 1 from FlowStrategy as fs where fs.Flow=m.Code and fs.Strategy<>7 ))";
                            paras.Add(item.Code);
                            base.genericMgr.Update(hql, paras.ToArray());
                        }
                        if (hqlProc != string.Empty && hqlProc != null)
                        {
                            hqlProc = hqlProc + "where Item=? and exists( select 1 from FlowMaster as m where m.Code=Flow and m.Type=1 )";
                            parasProc.Add(item.Code);
                            base.genericMgr.Update(hqlProc, parasProc.ToArray());
                        }
                        //if (hqlkb != string.Empty && hqlkb != null)
                        //{
                        //    hqlkb = hqlkb + "where Item=?";
                        //    paraskb.Add(item.Code);
                        //    base.genericMgr.Update(hqlkb, paraskb.ToArray());
                        //}
                    }
                    this.itemMgr.UpdateItem(item);
                    SaveSuccessMessage(Resources.MD.Item.Item_Updated);
                }
            }

            return PartialView(item);
        }

        /// <summary>
        /// Delete action
        /// </summary>
        /// <param name="id">Item id for delete</param>
        /// <returns>return to list view</returns>
        [SconitAuthorize(Permissions = "Url_Item_Delete")]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<Item>(id);
                SaveSuccessMessage(Resources.MD.Item.Item_Deleted);
                return View();
            }
        }

        #region 导出
        public void ExportItemXLS(ItemSearchModel searchModel)
        {
            string hql = " select i from Item as i ";
            if (!string.IsNullOrWhiteSpace(searchModel.ReferenceCode))
            {
                hql += string.Format(" and i.ReferenceCode like '{0}%' ", searchModel.ReferenceCode);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Code))
            {
                hql += string.Format(" and i.Code = '{0}' ", searchModel.Code);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Description))
            {
                hql += string.Format(" and i.Description like '{0}%' ", searchModel.Description);
            }
            if (searchModel.IsActive)
            {
                hql += string.Format(" and i.IsActive = 1 ");
            }
            else {
                hql += string.Format(" and i.IsActive = 0 ");
            }
            hql += " order by i.CreateDate asc ";
            IList<Item> exportItemList = this.genericMgr.FindAll<Item>(hql);
            ExportToXLS<Item>("ExportItemXLS", "xls", exportItemList);
        }
        #endregion
        #endregion

        #region 批量修改物料容器代码、容器描述、上线包装
        [SconitAuthorize(Permissions = "Url_Item_Edit")]
        public ActionResult BatchUpdateItem(IEnumerable<HttpPostedFileBase> attachments)
        {
            try
            {
                foreach (var file in attachments)
                {
                    itemMgr.BatchUpdateItemXls(file.InputStream);
                }
                SaveSuccessMessage("导入成功。");
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

        #region ItemReference
        /// <summary>
        /// List action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="itemRefsearchModel">ItemRef Search model</param>
        /// <param name="id">ItemRef id</param>
        /// <returns>return the result view</returns>
        [GridAction]
        [SconitAuthorize(Permissions = "Url_Item_View")]
        public ActionResult ItemRefResult(GridCommand command, ItemRefSearchModel itemRefsearchModel, string id)
        {
           
            ViewBag.ItemCode = id;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        /// <summary>
        /// AjaxList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="itemRefsearchModel">ItemRef Search Model</param>
        /// <param name="itemCode">ItemRef itemCode</param>
        /// <returns>return the result Model</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Item_View")]
        public ActionResult _AjaxItemReferenceList(GridCommand command, ItemRefSearchModel itemRefsearchModel, string itemCode)
        {
            SearchStatementModel searchStatementModel = this.ItemRefPrepareSearchStatement(command, itemRefsearchModel, itemCode);
            return PartialView(GetAjaxPageData<ItemReference>(searchStatementModel, command));
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <param name="id">ItemRef id</param>
        /// <returns>rediret view</returns>
        [SconitAuthorize(Permissions = "Url_Item_Edit")]
        public ActionResult _ItemRefNew(string id)
        {
            ItemReference itemRef = new ItemReference();
            itemRef.Item = id;
            return PartialView(itemRef);
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <param name="itemReference">ItemReference model</param>
        /// <returns>return to Edit action </returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_Item_Edit")]
        public ActionResult _ItemRefNew(ItemReference itemReference)
        {
            bool isError = false;
            if (ModelState.IsValid)
            {
                if (base.genericMgr.FindAll<long>(itemRefDuiplicateVerifyStatement, new object[] { itemReference.Id })[0] > 0)
                {
                    SaveErrorMessage(Resources.ErrorMessage.Errors_Existing_Code, itemReference.Id.ToString());
                    isError = true;
                }
                else
                {
                    if (itemReference.Party != null)
                    {
                        if (base.genericMgr.FindAll<long>(itemAndPartyDuiplicateVerifyStatement, new object[] { itemReference.Item, itemReference.Party })[0] > 0)
                        {
                            SaveErrorMessage(Resources.MD.ItemRef.ItemPartyErrors_Existing_Code);
                            isError = true;
                        }
                    }
                    else
                    {
                        if (base.genericMgr.FindAll<long>(itemAndPartyIsNullDuiplicateVerifyStatement, new object[] { itemReference.Item })[0] > 0)
                        {
                            SaveErrorMessage(Resources.MD.ItemRef.ItemPartyErrors_Existing_Code);
                            isError = true;
                        }
                    }
                }

                if (!isError)
                {
                    base.genericMgr.Create(itemReference);
                    SaveSuccessMessage(Resources.MD.ItemRef.ItemRef_Added);
                    return RedirectToAction("ItemRefResult/" + itemReference.Item);
                }
            }

            return PartialView(itemReference);
        }

        /// <summary>
        /// _ItemRefEdit action
        /// </summary>
        /// <param name="id">ItemRef id for Edit</param>
        /// <returns>return to the result view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_Item_Edit")]
        public ActionResult _ItemRefEdit(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                ItemReference itemReference = base.genericMgr.FindById<ItemReference>(id);
                return PartialView(itemReference);
            }
        }

        /// <summary>
        /// _ItemRefEdit action
        /// </summary>
        /// <param name="itemReference">ItemReference Model</param>
        /// <returns>return to _ItemRefEdit action</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_Item_Edit")]
        public ActionResult _ItemRefEdit(ItemReference itemReference)
        {
            if (ModelState.IsValid)
            {
                base.genericMgr.Update(itemReference);
                SaveSuccessMessage(Resources.MD.ItemRef.ItemRef_Updated);
            }

            ////return new RedirectToRouteResult(new RouteValueDictionary  
            ////                                       { 
            ////                                           { "action", "_ItemRefEdit" }, 
            ////                                           { "controller", "Item" },
            ////                                           { "id", itemReference.Id }
            ////                                       });
            return PartialView(itemReference);
        }

        /// <summary>
        /// ItemRefDelete action
        /// </summary>
        /// <param name="id">ItemReference id for delete</param>
        /// <param name="item">ItemReference item</param>
        /// <returns>return to ItemRefDelete action</returns>
        [SconitAuthorize(Permissions = "Url_Item_Delete")]
        public ActionResult ItemRefDelete(int? id, string item)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<ItemReference>(id);
                SaveSuccessMessage(Resources.MD.ItemRef.ItemRef_Deleted);
                return RedirectToAction("ItemRefResult/" + item);
            }
        }
        #endregion

        #region ItemPackage
        /// <summary>
        /// List action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="itemRefsearchModel">itemPackage Search model</param>
        /// <param name="id">itemPackage id</param>
        /// <returns>return the result view</returns>
        [GridAction]
        [SconitAuthorize(Permissions = "Url_Item_View")]
        public ActionResult ItemPackage(GridCommand command, ItemPackageSearchModel itemPackageSearchModel, string id)
        {
            ViewBag.ItemCode = id;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        /// <summary>
        /// AjaxList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="itemRefsearchModel">ItemPackage Search Model</param>
        /// <param name="itemCode">ItemPackage itemCode</param>
        /// <returns>return the result Model</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Item_View")]
        public ActionResult _AjaxItemPackageList(GridCommand command, ItemPackageSearchModel itemPackageSearchModel, string itemCode)
        {
            SearchStatementModel searchStatementModel = this.ItemPackagePrepareSearchStatement(command, itemPackageSearchModel, itemCode);
            return PartialView(GetAjaxPageData<ItemPackage>(searchStatementModel, command));
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <param name="id">ItemPackage id</param>
        /// <returns>rediret view</returns>
        [SconitAuthorize(Permissions = "Url_Item_Edit")]
        public ActionResult ItemPackageNew(string id)
        {
            ItemPackage itemPackage = new ItemPackage();
            itemPackage.Item = id;
            return PartialView(itemPackage);
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <param name="itemPackage">ItemPackage model</param>
        /// <returns>return to Edit action </returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_Item_Edit")]
        public ActionResult ItemPackageNew(ItemPackage itemPackage)
        {
            if (ModelState.IsValid)
            {
                if (base.genericMgr.FindAll<long>(itemPackageDuiplicateVerifyStatement, new object[] { itemPackage.Id })[0] > 0)
                {
                    SaveErrorMessage(Resources.ErrorMessage.Errors_Existing_Code, itemPackage.Id.ToString());
                }
                else if (base.genericMgr.FindAll<long>(itemAndUnitCountIsExistStatement, new object[] { itemPackage.Item, itemPackage.UnitCount })[0] > 0)
                {
                    SaveErrorMessage(Resources.MD.ItemPackage.ItemPackageErrors_Existing_ItemAndUnitCount);
                }
                else if (itemPackage.UnitCount <= 0)
                {
                    SaveErrorMessage("包装数必须大于0");
                }
                else
                {
                    if (itemPackage.Description == null)
                    {
                        itemPackage.Description = string.Empty;
                    }
                    base.genericMgr.Create(itemPackage);

                    SaveSuccessMessage(Resources.MD.ItemPackage.ItemPackage_Added);
                    return RedirectToAction("ItemPackage/" + itemPackage.Item);
                }
            }

            return PartialView(itemPackage);
        }

        /// <summary>
        /// ItemPackageEdit action
        /// </summary>
        /// <param name="id">ItemPackage id for Edit</param>
        /// <returns>return to the result view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_Item_Edit")]
        public ActionResult ItemPackageEdit(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                ItemPackage itemPackage = base.genericMgr.FindById<ItemPackage>(id);
                return PartialView(itemPackage);
            }
        }

        /////// <summary>
        /////// ItemPackageEdit action
        /////// </summary>
        /////// <param name="itemReference">ItemPackage Model</param>
        /////// <returns>return to ItemPackageEdit action</returns>
        ////[HttpPost]
        ////[SconitAuthorize(Permission = "Url_Item_Edit")]
        ////public ActionResult ItemPackageEdit(ItemPackage itemPackage)
        ////{
        ////    if (ModelState.IsValid)
        ////    {
        ////        if (itemPackage.IsDefault)
        ////        {
        ////            IList<ItemPackage> itemPackageList = base.genericMgr.FindAll<ItemPackage>("from ItemPackage as i where i.Item = ?", itemPackage.Item);
        ////            for (int i = 0; i < itemPackageList.Count; i++)
        ////            {
        ////                itemPackageList[i].IsDefault = false;
        ////                base.genericMgr.Update(itemPackageList[i]);
        ////            }
        ////        }
        ////        base.genericMgr.Update(itemPackage);
        ////        SaveSuccessMessage(Resources.MD.ItemPackage.ItemPackage_Updated);
        ////    }

        ////    ////return new RedirectToRouteResult(new RouteValueDictionary  
        ////    ////                                       { 
        ////    ////                                           { "action", "_ItemRefEdit" }, 
        ////    ////                                           { "controller", "Item" },
        ////    ////                                           { "id", itemReference.Id }
        ////    ////                                       });
        ////    return PartialView(itemPackage);
        ////}

        /// <summary>
        /// ItemPackageDelete action
        /// </summary>
        /// <param name="id">itemPackage id for delete</param>
        /// <param name="item">itemPackage item</param>
        /// <returns>return to ItemPackageDelete action</returns>
        [SconitAuthorize(Permissions = "Url_Item_Delete")]
        public ActionResult ItemPackageDelete(int? id, string item)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<ItemPackage>(id);
                SaveSuccessMessage(Resources.MD.ItemPackage.ItemPackage_Deleted);
                return RedirectToAction("ItemPackage/" + item);
            }
        }
        #endregion

        #region 短代码

        [SconitAuthorize(Permissions = "Url_ShortCode_View")]
        public ActionResult ShortIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_ShortCode_View")]
        public ActionResult ShortList(GridCommand command, ItemSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

       
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ShortCode_View")]
        public ActionResult _AjaxShortList(GridCommand command, ItemSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareShortSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<Item>(searchStatementModel, command));
        }

        public void ExportShortCodeXLS(ItemSearchModel searchModel)
        {
            var user = SecurityContextHolder.Get();
            string hql = " select i from Item as i where 1=1 ";
            IList<object> param = new List<object>();
            if (!string.IsNullOrWhiteSpace(searchModel.Code))
            {
                hql += " and i.Code=? ";
                param.Add(searchModel.Code);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Flow))
            {
//                hql += @" and exists( select 1 from FlowDetail as f where f.Item=i.Code and f.Flow=?  and  exists 
//                    (select 1 from UserPermissionView as p where p.UserId =" + user.Id + " and  p.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region +
//                    ") and  exists ( select 1 from FlowMaster as fm where fm.Flow=? and  p.PermissionCode = fm.PartyFrom or p.PermissionCode = fm.PartyTo ))  ) ";
                hql += @" and exists( select 1 from FlowDetail as f where f.Item=i.Code and f.Flow=?  and exists
                ( select 1 from FlowMaster as fm where fm.Code=f.Flow and fm.Code=? and exists
                (select 1 from UserPermissionView as p where p.UserId =" + user.Id + 
                " and (((fm.PartyFrom=p.PermissionCode or fm.PartyTo=p.PermissionCode ) and p.PermissionCategoryType in (3,4)) or  (fm.PartyTo=p.PermissionCode and p.PermissionCategoryType=2 )))))";
                param.Add(searchModel.Flow);
                param.Add(searchModel.Flow);
            }
            else
            {
                //hql += " and exists( select f from FlowDetail as f where f.Item=i.Code  and  exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + ") and (p.PermissionCode = f.PartyFrom or p.PermissionCode = f.PartyTo ))  ) ";
                hql += @" and exists( select 1 from FlowDetail as f where f.Item=i.Code   and exists
                ( select 1 from FlowMaster as fm where fm.Code=f.Flow and exists
                (select 1 from UserPermissionView as p where p.UserId =" + user.Id +
                " and (((fm.PartyFrom=p.PermissionCode or fm.PartyTo=p.PermissionCode ) and p.PermissionCategoryType in (3,4)) or  (fm.PartyTo=p.PermissionCode and p.PermissionCategoryType=2 )))))";
            }
            if (!string.IsNullOrWhiteSpace(searchModel.ReferenceCode))
            {
                hql += " and i.ReferenceCode like ? ";
                param.Add(searchModel.ReferenceCode+"%");
            }
            IList<Item> exportList = this.genericMgr.FindAll<Item>(hql,param.ToArray());
            ExportToXLS<Item>("ExportShorCode", "XLS", exportList);
        }

        #endregion

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">Item Search Model</param>
        /// <returns>return Search Statement</returns>
        private SearchStatementModel PrepareSearchStatement(GridCommand command, ItemSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("ReferenceCode", searchModel.ReferenceCode, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Code", searchModel.Code, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Description", searchModel.Description, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("IsActive", searchModel.IsActive, "i", ref whereStatement, param);
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="itemRefsearchModel">ItemRef Search Model</param>
        /// <param name="id">ItemRef id</param>
        /// <returns>return Search Statement</returns>
        private SearchStatementModel ItemRefPrepareSearchStatement(GridCommand command, ItemRefSearchModel itemRefsearchModel, string id)
        {
            string whereStatement = " where i.Item='" + id + "'";

            IList<object> param = new List<object>();

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectItemRefCountStatement;
            searchStatementModel.SelectStatement = selectItemRefStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="itemRefsearchModel">ItemPackage Search Model</param>
        /// <param name="id">ItemPackage id</param>
        /// <returns>return Search Statement</returns>
        private SearchStatementModel ItemPackagePrepareSearchStatement(GridCommand command, ItemPackageSearchModel itemPackageSearchModel, string id)
        {
            string whereStatement = " where i.Item='" + id + "'";

            IList<object> param = new List<object>();

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectItemPackageCountStatement;
            searchStatementModel.SelectStatement = selectItemPackageStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        private SearchStatementModel PrepareShortSearchStatement(GridCommand command, ItemSearchModel searchModel)
        {
            var user = SecurityContextHolder.Get();
            string whereStatement = " where 1=1 ";
            //whereStatement += @" and  exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + ") and (p.PermissionCode = f.PartyFrom or p.PermissionCode = f.PartyTo ))";
            IList<object> param = new List<object>();
            if (!string.IsNullOrWhiteSpace(searchModel.Flow))
            {
                whereStatement += @" and exists( select 1 from FlowDetail as f where f.Item=i.Code and f.Flow=?  and exists
                ( select 1 from FlowMaster as fm where fm.Code=f.Flow and fm.Code=? and exists
                (select 1 from UserPermissionView as p where p.UserId =" + user.Id +
                " and (((fm.PartyFrom=p.PermissionCode or fm.PartyTo=p.PermissionCode ) and p.PermissionCategoryType in (3,4)) or  (fm.PartyTo=p.PermissionCode and p.PermissionCategoryType=2 )))))";
                param.Add(searchModel.Flow);
                param.Add(searchModel.Flow);
            }
            else
            {
                whereStatement += @" and exists( select 1 from FlowDetail as f where f.Item=i.Code   and exists
                ( select 1 from FlowMaster as fm where fm.Code=f.Flow and exists
                (select 1 from UserPermissionView as p where p.UserId =" + user.Id +
                " and (((fm.PartyFrom=p.PermissionCode or fm.PartyTo=p.PermissionCode ) and p.PermissionCategoryType in (3,4)) or  (fm.PartyTo=p.PermissionCode and p.PermissionCategoryType=2 )))))";
            }

            HqlStatementHelper.AddLikeStatement("ReferenceCode", searchModel.ReferenceCode, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Code", searchModel.Code,  "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("IsActive", true, "i", ref whereStatement, param);
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }
    }
}
