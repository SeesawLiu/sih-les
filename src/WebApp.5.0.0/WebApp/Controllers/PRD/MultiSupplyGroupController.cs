using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using com.Sconit.Entity;

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
    using com.Sconit.Entity.MD;
    using com.Sconit.Entity.VIEW;
    using NHibernate.Criterion;
    #endregion

    /// <summary>
    /// This controller response to control the MultiSupplyGroup.
    /// </summary>
    public class MultiSupplyGroupController : WebAppBaseController
    {
        #region Properties
        public IMultiSupplyGroupMgr iMultiSupplyGroupMgr { get; set; }
        #endregion

        private static string selectViewCountStatement = "select count(*) from MultiSupplyGroupView as msg";

        private static string selectViewStatement = "select msg from MultiSupplyGroupView as msg";

        private static string duiplicateVerifyStatement = @"select count(*) from MultiSupplyGroup as msg where msg.GroupNo = ? ";

        private static string duiplicateSupplierVerifyStatement = @"select mss from MultiSupplySupplier as mss where mss.GroupNo = ? and mss.Supplier = ?";

        private static string selectSupplierByGroupNoStatement = "select mss from MultiSupplySupplier as mss where mss.GroupNo = ?";

        private static string selectMaxSupplierSeq = "select max(Seq) from MultiSupplySupplier as mss where mss.GroupNo = ? ";

        private static string duiplicateItemVerifyStatement = @"select msi from MultiSupplyItem as msi where msi.Item = ? ";

        private static string selectItemsByGroupNoStatement = "select mss from MultiSupplyItem as mss where mss.GroupNo = ?";

        #region Group
        /// <summary>
        /// Index action for MultiSupplyGroup controller
        /// </summary>
        /// <returns>Index view</returns>
        [SconitAuthorize(Permissions = "Url_MultiSupplyGroup_View")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// List action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">MultiSupplyGroup Search model</param>
        /// <returns>return the result view</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_MultiSupplyGroup_View")]
        public ActionResult List(GridCommand command, MultiSupplyGroupSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        /// <summary>
        ///  AjaxList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">MultiSupplyGroup Search Model</param>
        /// <returns>return the result action</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_MultiSupplyGroup_View")]
        public ActionResult _AjaxList(GridCommand command, MultiSupplyGroupSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            var model = GetAjaxPageData<MultiSupplyGroupView>(searchStatementModel, command);
            var list = model.Data.OrderBy(c => c.Supplier).OrderBy(c => c.GroupNo).ToList();

            var lastGroupNo = "";
            var lastSupplier = "";
            foreach (MultiSupplyGroupView view in list)
            {
                if (!string.IsNullOrWhiteSpace(lastGroupNo) && view.GroupNo == lastGroupNo)
                {
                    lastGroupNo = view.GroupNo;
                    view.DisplayGroupNo = string.Empty;
                    view.Description = string.Empty;
                    view.EffecitveSupplier = string.Empty;
                    view.TargetCycleQty = null;
                    view.AccumulateQty = null;

                    if (view.Supplier == lastSupplier)
                    {
                        lastSupplier = view.Supplier;
                        view.Sequence = null;
                        view.Supplier = string.Empty;
                        view.CycleQty = null;
                        view.SpillQty = null;
                        view.Proportion = null;
                        view.IsActive = null;
                    }
                    else
                    {
                        lastSupplier = view.Supplier;
                    }
                }
                else
                {
                    view.DisplayGroupNo = view.GroupNo;
                    lastGroupNo = view.GroupNo;
                    lastSupplier = view.Supplier;
                }
            }

            model.Data = list;
            return PartialView(model);
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <returns>New view</returns>
        [SconitAuthorize(Permissions = "Url_MultiSupplyGroup_Edit")]
        public ActionResult New()
        {
            return View();
        }

        /// <summary>
        /// New action
        /// </summary>
        /// <param name="multisupplygroup">MultiSupplyGroup Model</param>
        /// <returns>return the result view</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_MultiSupplyGroup_Edit")]
        public ActionResult New(MultiSupplyGroup multisupplygroup)
        {
            if (ModelState.IsValid)
            {
                if (base.genericMgr.FindAll<long>(duiplicateVerifyStatement, new object[] { multisupplygroup.GroupNo })[0] > 0)
                {
                    SaveErrorMessage(Resources.ErrorMessage.Errors_Existing_Code, multisupplygroup.GroupNo);
                }
                else
                {
                    base.genericMgr.Create(multisupplygroup);
                    SaveSuccessMessage(Resources.PRD.MultiSupplyGroup.MultiSupplyGroup_Added);
                    return RedirectToAction("Edit/" + multisupplygroup.GroupNo);
                }
            }

            return View(multisupplygroup);
        }

        /// <summary>
        /// Edit view
        /// </summary>
        /// <param name="id">MultiSupplyGroup id for edit</param>
        /// <returns>return the result view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_MultiSupplyGroup_Edit")]
        public ActionResult Edit(string id, bool? isReturn, int? i)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            else
            {
                //TempData["IsReturn"] = isReturn == null ? false : isReturn;
                //if (isReturn.HasValue == true && isReturn == true)
                //{
                //    TempData["TabIndex"] = 2;
                //}
                //if (tabIndex != null)
                //{
                //    TempData["TabIndex"] = tabIndex.Value;
                //}
                if (i != null)
                {
                    TempData["TabIndex"] = i.Value;
                }
                return View("Edit", string.Empty, id);
            }
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_MultiSupplyGroup_Edit")]
        public ActionResult _Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            ViewBag.GroupNo = id;
            MultiSupplyGroup group = base.genericMgr.FindById<MultiSupplyGroup>(id);
            return PartialView(group);
        }

        /// <summary>
        /// Edit view
        /// </summary>
        /// <param name="multisupplygroup">MultiSupplyGroup Model</param>
        /// <returns>return the partial view</returns>
        [HttpPost]
        [SconitAuthorize(Permissions = "Url_MultiSupplyGroup_Edit")]
        public ActionResult _Edit(MultiSupplyGroup multisupplygroup)
        {
            if (ModelState.IsValid)
            {
                base.genericMgr.Update(multisupplygroup);
                SaveSuccessMessage(Resources.SCM.FlowMaster.FlowMaster_Updated);
            }

            return PartialView(multisupplygroup);
        }

        /// <summary>
        /// Delete action
        /// </summary>
        /// <param name="id">MultiSupplyGroup id for delete</param>
        /// <returns>return to List action</returns>
        [SconitAuthorize(Permissions = "Url_MultiSupplyGroup_Delete")]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                iMultiSupplyGroupMgr.DeleteMultiSupplyGroup(id);

                SaveSuccessMessage(Resources.PRD.MultiSupplyGroup.MultiSupplyGroup_Deleted);
                return RedirectToAction("List");
            }
        }

        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">MultiSupplyGroup Search Model</param>
        /// <returns>return MultiSupplyGroup search model</returns>
        private SearchStatementModel PrepareSearchStatement(GridCommand command, MultiSupplyGroupSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("GroupNo", searchModel.GroupNo, HqlStatementHelper.LikeMatchMode.Start, "msg", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Supplier", searchModel.Supplier, "msg", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "msg", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("SubstituteGroup", searchModel.SubstituteGroup, HqlStatementHelper.LikeMatchMode.Start, "msg", ref whereStatement, param);

            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "DisplayGroupNo")
                {
                    command.SortDescriptors[0].Member = "GroupNo";
                }
            }
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectViewCountStatement;
            searchStatementModel.SelectStatement = selectViewStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }
        #endregion

        #region Supplier
        [GridAction]
        [SconitAuthorize(Permissions = "Url_MultiSupplyGroup_View")]
        public ActionResult _Supplier(GridCommand command, MultiSupplySupplierSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [SconitAuthorize(Permissions = "Url_MultiSupplyGroup_Edit")]
        public ActionResult _SupplierList(string GroupNo)
        {
            ViewBag.GroupNo = GroupNo;
            return PartialView();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_MultiSupplyGroup_Edit")]
        public ActionResult _SelectSupplierList(string id)
        {
            ViewBag.GroupNo = id;
            IList<MultiSupplySupplier> supplierList = base.genericMgr.FindAll<MultiSupplySupplier>(selectSupplierByGroupNoStatement, id);
            return View(new GridModel(supplierList));
            //return PartialView(supplierList);
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_MultiSupplyGroup_Edit")]
        public ActionResult _SupplierInsert(MultiSupplySupplier multiSupplySupplier, string GroupNo)
        {
            ModelState.Remove("Id");

            if (ModelState.IsValid)
            {
                var suppliers = base.genericMgr.FindAll<MultiSupplySupplier>(duiplicateSupplierVerifyStatement,
                    new object[] { multiSupplySupplier.GroupNo, multiSupplySupplier.Supplier });

                if (multiSupplySupplier.CycleQty <= 0)
                {
                    ModelState.AddModelError("CycleQty", "循环量必须大于0");
                }
                else if (suppliers.Count() > 0)
                {
                    ModelState.AddModelError("Supplier", string.Format(Resources.PRD.MultiSupplySupplier.MultiSupplySupplier_Existing_Supplier, multiSupplySupplier.Supplier));
                }
                else
                {
                    var dbbMaxSeq = base.genericMgr.FindAll(selectMaxSupplierSeq, multiSupplySupplier.GroupNo)[0];
                    int maxSeq = 0;
                    int.TryParse((dbbMaxSeq != null ? dbbMaxSeq.ToString() : "0"), out maxSeq);
                    multiSupplySupplier.Seq = maxSeq + 1;
                    multiSupplySupplier.IsActive = true;
                    base.genericMgr.Create(multiSupplySupplier);
                }
            }

            IList<MultiSupplySupplier> supplierList = base.genericMgr.FindAll<MultiSupplySupplier>(selectSupplierByGroupNoStatement, GroupNo);
            return PartialView(new GridModel(supplierList));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_MultiSupplyGroup_Edit")]
        public ActionResult _SupplierUpdate(MultiSupplySupplier multiSupplySupplier, string GroupNo)
        {
            if (ModelState.IsValid)
            {
                var suppliers = base.genericMgr.FindAll<MultiSupplySupplier>(duiplicateSupplierVerifyStatement,
                    new object[] { multiSupplySupplier.GroupNo, multiSupplySupplier.Supplier });

                var sameItem = suppliers.FirstOrDefault(c => c.Id != multiSupplySupplier.Id);
                if (multiSupplySupplier.CycleQty <= 0)
                {
                    ModelState.AddModelError("CycleQty", "循环量必须大于0");
                }
                else if (sameItem != null)
                {
                    ModelState.AddModelError("Supplier", string.Format(Resources.PRD.MultiSupplySupplier.MultiSupplySupplier_Existing_Supplier, multiSupplySupplier.Supplier));
                }
                else
                {
                    var supplier = base.genericMgr.FindById<MultiSupplySupplier>(multiSupplySupplier.Id);

                    #region 切换厂商
                    if (supplier.IsActive && !multiSupplySupplier.IsActive)
                    {
                        bool isValid = SwitchSupplier(multiSupplySupplier);
                        if (!isValid)
                        {
                            ModelState.AddModelError("Supplier", "没找到后续有效供应商");
                            IList<MultiSupplySupplier> supplierList = base.genericMgr.FindAll<MultiSupplySupplier>(selectSupplierByGroupNoStatement, GroupNo);
                            return View(new GridModel(supplierList));
                        }
                    }
                    #endregion

                    supplier.Supplier = multiSupplySupplier.Supplier;
                    supplier.CycleQty = multiSupplySupplier.CycleQty;
                    supplier.SpillQty = multiSupplySupplier.SpillQty;
                    supplier.IsActive = multiSupplySupplier.IsActive;
                    supplier.Proportion = multiSupplySupplier.Proportion;
                    base.genericMgr.Update(supplier);
                }
            }

            IList<MultiSupplySupplier> supplierList1 = base.genericMgr.FindAll<MultiSupplySupplier>(selectSupplierByGroupNoStatement, GroupNo);
            return View(new GridModel(supplierList1));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_MultiSupplyGroup_Edit")]
        public ActionResult _SupplierDelete(int? id, string GroupNo)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                var supplier = base.genericMgr.FindById<MultiSupplySupplier>(id);
                bool isValid = SwitchSupplier(supplier);
                if (!isValid)
                {
                    ModelState.AddModelError("Supplier", "没找到后续有效供应商");
                }
                else
                {
                    base.genericMgr.Delete(" from MultiSupplyItem where Supplier ='" + id + "' and GroupNo ='" + GroupNo + "'");
                    base.genericMgr.DeleteById<MultiSupplySupplier>(id);
                }
            }

            ViewBag.GroupNo = GroupNo;
            IList<MultiSupplySupplier> supplierList = base.genericMgr.FindAll<MultiSupplySupplier>(selectSupplierByGroupNoStatement, GroupNo);
            return View(new GridModel(supplierList));
        }

        /// <summary>
        /// 切换厂商
        /// </summary>
        /// <param name="multiSupplySupplier"></param>
        /// <returns></returns>
        private bool SwitchSupplier(MultiSupplySupplier multiSupplySupplier)
        {
            var group = base.genericMgr.FindById<MultiSupplyGroup>(multiSupplySupplier.GroupNo);

            var sameEffSupplier = group.EffSupplier == multiSupplySupplier.Supplier;
            var sameKBEffSupplier = group.KBEffSupplier == multiSupplySupplier.Supplier;
            if (sameEffSupplier || sameKBEffSupplier)
            {
                var samGroupSuppliers = base.genericMgr.FindAll<MultiSupplySupplier>("select mss from MultiSupplySupplier as mss where mss.GroupNo = ? and Id != ? and IsActive=true and AccumulateQty > CycleQty", new object[] { multiSupplySupplier.GroupNo, multiSupplySupplier.Id }).OrderBy(c => c.Seq);
                var nextSupplier = samGroupSuppliers.FirstOrDefault(c => c.Seq > multiSupplySupplier.Seq);
                if (nextSupplier == null)
                {
                    nextSupplier = samGroupSuppliers.FirstOrDefault(c => c.AccumulateQty > c.CycleQty);
                }
                if (nextSupplier == null)
                    return false;

                if (sameEffSupplier)
                {
                    group.EffSupplier = nextSupplier.Supplier;
                    group.TargetCycleQty = Convert.ToInt32(Math.Abs(nextSupplier.CycleQty - nextSupplier.SpillQty));
                }

                if (sameKBEffSupplier)
                {
                    group.KBEffSupplier = nextSupplier.Supplier;
                    group.KBTargetCycleQty = nextSupplier.CycleQty;
                }

                base.genericMgr.Update(group);
            }

            return true;
        }

        #endregion

        #region Item
       
        [GridAction]
        [SconitAuthorize(Permissions = "Url_MultiSupplyGroup_View")]
        public ActionResult _Item(GridCommand command, MultiSupplyItemSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            ViewBag.GroupNo = searchModel.GroupNo;
            return PartialView();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_MultiSupplyGroup_Edit")]
        public ActionResult _SelectItemList(string id)
        {
            ViewBag.GroupNo = id;
            IList<MultiSupplyItem> itemList = base.genericMgr.FindAll<MultiSupplyItem>(selectItemsByGroupNoStatement, id);
            return View(new GridModel(itemList));
        }

        public ActionResult _WebItem(string itemCode)
        {
            var item = base.genericMgr.FindById<Item>(itemCode);
            return this.Json(item);
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_MultiSupplyGroup_Edit")]
        public ActionResult _ItemInsert(MultiSupplyItem multiSupplyItem, string GroupNo)
        {
            ModelState.Remove("Id");
            ViewBag.GroupNo = multiSupplyItem.GroupNo;

            if (ModelState.IsValid)
            {
                var items = base.genericMgr.FindAll<MultiSupplyItem>(duiplicateItemVerifyStatement, new object[] { multiSupplyItem.Item });

                var sameItemInDiffGroup = items.FirstOrDefault(c => c.GroupNo != multiSupplyItem.GroupNo);
                if (sameItemInDiffGroup != null)
                {
                    ModelState.AddModelError("Item", string.Format(Resources.PRD.MultiSupplyItem.MultiSupplyItem_Errors_ItemOnlyBelongOneGroup, sameItemInDiffGroup.Item, sameItemInDiffGroup.GroupNo));
                }
                else
                {
                    var sameItemInSameGroupandSupplier = items.FirstOrDefault(c => c.GroupNo == multiSupplyItem.GroupNo && c.Supplier == multiSupplyItem.Supplier);
                    if (sameItemInSameGroupandSupplier != null)
                    {
                        ModelState.AddModelError("Item", string.Format(Resources.PRD.MultiSupplyItem.MultiSupplyItem_Errors_Existing_ItemInTheGroupAndSupplier, sameItemInSameGroupandSupplier.Item, sameItemInSameGroupandSupplier.GroupNo, sameItemInSameGroupandSupplier.Supplier));
                    }
                    else
                    {
                        multiSupplyItem.ItemDescription = base.genericMgr.FindById<Item>(multiSupplyItem.Item).Description;
                        base.genericMgr.Create(multiSupplyItem);
                    }
                }
            }

            IList<MultiSupplyItem> itemList = base.genericMgr.FindAll<MultiSupplyItem>(selectItemsByGroupNoStatement, GroupNo);
            return PartialView(new GridModel(itemList));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_MultiSupplyGroup_Edit")]
        public ActionResult _ItemUpdate(MultiSupplyItem multiSupplyItem, string GroupNo)
        {
            if (ModelState.IsValid)
            {
                var items = base.genericMgr.FindAll<MultiSupplyItem>(duiplicateItemVerifyStatement, new object[] { multiSupplyItem.Item });

                var sameItemInDiffGroup = items.FirstOrDefault(c => c.GroupNo != multiSupplyItem.GroupNo);
                if (sameItemInDiffGroup != null)
                {
                    ModelState.AddModelError("Item", string.Format(Resources.PRD.MultiSupplyItem.MultiSupplyItem_Errors_ItemOnlyBelongOneGroup, sameItemInDiffGroup.Item, sameItemInDiffGroup.GroupNo));
                }
                else
                {
                    var sameItemInSameGroupandSupplier = items.FirstOrDefault(c => c.GroupNo == multiSupplyItem.GroupNo && c.Supplier == multiSupplyItem.Supplier && c.Id != multiSupplyItem.Id);
                    if (sameItemInSameGroupandSupplier != null)
                    {
                        ModelState.AddModelError("Item", string.Format(Resources.PRD.MultiSupplyItem.MultiSupplyItem_Errors_Existing_ItemInTheGroupAndSupplier, sameItemInSameGroupandSupplier.Item, sameItemInSameGroupandSupplier.GroupNo, sameItemInSameGroupandSupplier.Supplier));
                    }
                    else
                    {
                        multiSupplyItem.ItemDescription = base.genericMgr.FindById<Item>(multiSupplyItem.Item).Description;
                        base.genericMgr.Update(multiSupplyItem);
                    }
                }
            }

            IList<MultiSupplyItem> itemList = base.genericMgr.FindAll<MultiSupplyItem>(selectItemsByGroupNoStatement, GroupNo);
            return View(new GridModel(itemList));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_MultiSupplyGroup_Edit")]
        public ActionResult _ItemDelete(int? id, string GroupNo)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }

            base.genericMgr.DeleteById<MultiSupplyItem>(id);

            ViewBag.GroupNo = GroupNo;
            IList<MultiSupplyItem> itemList = base.genericMgr.FindAll<MultiSupplyItem>(selectItemsByGroupNoStatement, GroupNo);
            return View(new GridModel(itemList));
        }
        #endregion

        public ActionResult ImportMultiSupplyItem(IEnumerable<HttpPostedFileBase> attachments)
        {
            try
            {
                foreach (var file in attachments)
                {
                    string retVal = iMultiSupplyGroupMgr.CreateMultiSupplyItemXlsx(file.InputStream);
                    SaveSuccessMessage(retVal);
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

        public void ExportXLS(MultiSupplyGroupSearchModel searchModel)
        {
            var selectMultiSupplyGroupView = "select v from MultiSupplyGroupView as v where 1=1 ";

            if (searchModel.GroupNo != null)
            {
                selectMultiSupplyGroupView += " and v.GroupNo like '" + searchModel.GroupNo + "%'";
            }
            if (searchModel.Supplier != null)
            {
                selectMultiSupplyGroupView += " and v.Supplier='" + searchModel.Supplier + "'";
            }
            if (searchModel.Item != null)
            {
                selectMultiSupplyGroupView += " and v.Item='" + searchModel.Item + "'";
            }
            if (searchModel.SubstituteGroup != null)
            {
                selectMultiSupplyGroupView += " and v.SubstituteGroup like '" + searchModel.SubstituteGroup + "%'";
            }

            string maxRows = this.systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.ExportMaxRows);
            IList<MultiSupplyGroupView> views = base.genericMgr.FindAll<MultiSupplyGroupView>(selectMultiSupplyGroupView, 0, int.Parse(maxRows));

            ExportToXLS<MultiSupplyGroupView>("MultiSupplyGroup", "XLS", views);
        }
    }
}