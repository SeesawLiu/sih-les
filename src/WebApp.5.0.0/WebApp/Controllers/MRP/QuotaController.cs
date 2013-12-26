
namespace com.Sconit.Web.Controllers.MRP
{
    #region Retrive
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;
    using com.Sconit.Web.Models.SearchModels.MRP;
    using com.Sconit.Web.Models;
    using com.Sconit.Entity.SCM;
    using com.Sconit.Entity.Exception;
    using com.Sconit.Service;
    using com.Sconit.Entity.MD;
    using com.Sconit.Entity.SCM;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.Collections;
using com.Sconit.Utility;
    #endregion

    public class QuotaController : WebAppBaseController
    {

        private IProductionLineMgr prdMgr { get; set; }

        private static string selectCountStatement = "select count(*) from Quota as q";

        private static string selectStatement = "select q from Quota as q";

        private static string selectQuotaCycleQtyCountStatement = "select count(*) from QuotaCycleQty as q";

        private static string selectQuotaCycleQtyStatement = "select q from QuotaCycleQty as q";

        #region Quota 配额调整量维护
        [SconitAuthorize(Permissions = "Url_Quota_View")]
        public ActionResult Index()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Quota_View")]
        public ActionResult List(GridCommand command, QuotaSearchModel searchModel)
        {
            ViewBag.ItemCode = searchModel.ItemCode;
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            //if (string.IsNullOrWhiteSpace(searchModel.ItemCode))
            //{
            //    SaveWarningMessage("物料代码不能为空。");
            //}
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Quota_View")]
        public ActionResult _AjaxList(GridCommand command, QuotaSearchModel searchModel)
        {
            TempData["GridCommand"] = command;
            //if (string.IsNullOrWhiteSpace(searchModel.ItemCode))
            //{
            //    return PartialView(new GridModel(new List<Quota>()));
            //}
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            var data = GetAjaxPageData<Quota>(searchStatementModel, command);
            return PartialView(data);
            //foreach (var flowDet in data.Data)
            //{
            //    var flowMaster=this.genericMgr.FindById<FlowMaster>(flowDet.Flow);
            //    flowDet.PartyFrom = flowMaster.PartyFrom;
            //    flowDet.LocationTo = flowMaster.LocationTo;

            //    var supplier = this.genericMgr.FindById<Supplier>(flowMaster.PartyFrom);
            //    flowDet.ManufactureParty = supplier.Code;
            //    flowDet.ManufacturePartyDesc = supplier.CodeDescription;
            //    flowDet.ManufacturePartyShortCode = supplier.ShortCode;

            //    var location = this.genericMgr.FindById<Location>(flowDet.LocationTo);
            //    flowDet.SapLocation = location.SAPLocation;

            //    var item = this.genericMgr.FindById<Item>(flowDet.Item);
            //    flowDet.ItemDescription = item.Description;
            //}
            //data.Data = data.Data.OrderBy(d => d.LocationTo).ToList();
            //string locTo = string.Empty;
            //foreach (var flowDet in data.Data)
            //{
            //    var mrpWeight = data.Data.Where(d => d.LocationTo == flowDet.LocationTo).Sum(d => d.MrpWeight);
            //    if (mrpWeight > 0)
            //    {
            //        flowDet.AverageRatio = decimal.Round((Convert.ToDecimal(flowDet.MrpWeight * 100.00) / mrpWeight)).ToString() + "%";
            //    }
            //    else
            //    {
            //        flowDet.AverageRatio = "0%";
            //    }
            //    if (flowDet.LocationTo.Equals(locTo))
            //    {
            //        flowDet.LocationTo = string.Empty;
            //        flowDet.SapLocation = string.Empty;
            //    }
            //    else
            //    {
            //        locTo = flowDet.LocationTo;
            //    }
            //}
            //return PartialView(data);
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Quota_View")]
        public ActionResult _Update(Int32 id, Quota quota, QuotaSearchModel searchModel)
        {
            ViewBag.ItemCode = searchModel.ItemCode;
            try
            {
                if (quota.AdjQty < 0)
                {
                    throw new BusinessException(" 调整数不能小于0");
                }
                if (this.genericMgr.FindAll<QuotaCycleQty>("select q from QuotaCycleQty as q where q.Item=?", quota.Item).Count == 0)
                {
                    throw new BusinessException(string.Format(" 物料{0}没有维护配额循环量，更新失败。",quota.Item));
                }
                Quota upQuota = this.genericMgr.FindById<Quota>(id);
                upQuota.AdjQty = quota.AdjQty;
                this.genericMgr.Update(upQuota);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage("修改失败。" + ex.GetMessages()[0].GetMessageString());
            }
            catch (Exception ex)
            {
                SaveErrorMessage("修改失败。" + ex.Message);
            }
            GridCommand command = (GridCommand)TempData["GridCommand"];
            TempData["GridCommand"] = command;
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            var data = GetAjaxPageData<Quota>(searchStatementModel, command);
            return PartialView(data);
        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, QuotaSearchModel searchModel)
        {
            string whereStatement = string.Empty;
            IList<object> param = new List<object>();

            HqlStatementHelper.AddEqStatement("Item", searchModel.ItemCode, "q", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("IsActive", true, "q", ref whereStatement, param);

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

        #region QuotaCycleQty 配额循环量维护
        [SconitAuthorize(Permissions = "Url_QuotaCycleQty_View")]
        public ActionResult CycleQtyIndex()
        {
            ViewBag.HaveCycleEditPermission = CurrentUser.Permissions.Where(p => p.PermissionCode == "Url_QuotaCycleQty_Edit").Count() > 0;
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_QuotaCycleQty_View")]
        public ActionResult CycleQtyList(GridCommand command, QuotaSearchModel searchModel)
        {
            ViewBag.HaveCycleEditPermission = CurrentUser.Permissions.Where(p => p.PermissionCode == "Url_QuotaCycleQty_Edit").Count() > 0;
            ViewBag.ItemCode = searchModel.ItemCode;
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            //if (string.IsNullOrWhiteSpace(searchModel.ItemCode))
            //{
            //    SaveWarningMessage("物料代码不能为空。");
            //}
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_QuotaCycleQty_View")]
        public ActionResult _AjaxCycleQtyList(GridCommand command, QuotaSearchModel searchModel)
        {
            TempData["CycleQtyGridCommand"] = command;
            //if (string.IsNullOrWhiteSpace(searchModel.ItemCode))
            //{
            //    return PartialView(new GridModel(new List<QuotaCycleQty>()));
            //}
            SearchStatementModel searchStatementModel = PrepareCycleQtySearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<QuotaCycleQty>(searchStatementModel, command));
        }


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_QuotaCycleQty_Edit")]
        public ActionResult _CycleQtyInsert(QuotaCycleQty quotaCycleQty, QuotaSearchModel searchModel)
        {
            try
            {
                ViewBag.HaveCycleEditPermission = CurrentUser.Permissions.Where(p => p.PermissionCode == "Url_QuotaCycleQty_Edit").Count() > 0;
                if (string.IsNullOrWhiteSpace(quotaCycleQty.Item))
                {
                    throw new BusinessException("物料编号不能为空。");
                }
                if (quotaCycleQty.CycleQty <= 0)
                {
                    throw new BusinessException("循环量不能小于等于0。");
                }
                if (this.genericMgr.FindAll<QuotaCycleQty>(selectQuotaCycleQtyStatement + " where Item=?", quotaCycleQty.Item).Count > 0)
                {
                    throw new BusinessException(string.Format(" 物料编号{0}已经维护，请确认。 ",quotaCycleQty.Item));
                }
                var quotaList = this.genericMgr.FindAll<Quota>("select q from Quota as q where q.Item=? and Isactive=?",new object[]{quotaCycleQty.Item,true});
                if (quotaList == null || quotaList.Count == 0)
                {
                    throw new BusinessException(string.Format(" 物料编号{0}没有维护有效的调整数，请确认。 ", quotaCycleQty.Item));
                }
                else
                {
                    var supplierCodes = quotaList.Select(q => q.Supplier).ToArray();
                    var flowMasters = new List<FlowMaster>();
                    foreach (var supplier in supplierCodes)
                    {
                        var flowMasterBySupplier = this.genericMgr.FindAll<FlowMaster>("select f from FlowMaster as f where f.PartyFrom=? and exists( select 1 from FlowDetail as d where d.Flow=f.Code and d.Item=? )", new object[] { supplier, quotaCycleQty.Item });
                        //var flowMasterBySupplier = this.genericMgr.FindAll<FlowMaster>("select f from FlowMaster as f where f.PartyFrom=?", supplier);
                        if (flowMasterBySupplier == null || flowMasterBySupplier.Count == 0)
                        {
                            throw new BusinessException(string.Format(" 物料编号{0},供应商{1}没有找到有效的采购路线，请确认。 ", quotaCycleQty.Item, supplier));
                        }
                        else
                        {
                            flowMasters.Add(flowMasterBySupplier.First());
                        }
                    }
                    var locTos = flowMasters.Select(f => f.LocationTo).ToList();
                    var flowMasterGrooups = (from tak in flowMasters
                                             group tak by tak.PartyFrom
                                                 into result
                                                 select new
                                                 {
                                                     Supplier = result.Key,
                                                     FlowMasters = result.ToList()
                                                 });

                    foreach (var groups in flowMasterGrooups)
                    {
                        foreach (var locTo in locTos)
                        {
                            if (groups.FlowMasters.Where(f => f.LocationTo == locTo).Count() == 0)
                            {
                                throw new BusinessException(string.Format(" 物料编号{0},供应商{1}目的库位{2}没有找到有效的采购路线，与其他供应商不一致，请确认。 ", quotaCycleQty.Item, groups.FlowMasters.First().PartyFrom, locTo));
                            }
                        }
                    }
                }
                Item item = this.genericMgr.FindById<Item>(quotaCycleQty.Item);
                quotaCycleQty.ItemDesc = item.Description;
                quotaCycleQty.RefItemCode = item.ReferenceCode;
                this.genericMgr.Create(quotaCycleQty);
                SaveSuccessMessage("添加成功。");
                ViewBag.ItemCode = quotaCycleQty.Item;
                searchModel.ItemCode = quotaCycleQty.Item;
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage("添加失败。" + ex.GetMessages()[0].GetMessageString());
            }
            catch(Exception ex)
            {
                SaveErrorMessage("添加失败。" + ex.Message);
            }
            ViewBag.HaveCycleEditPermission = CurrentUser.Permissions.Where(p => p.PermissionCode == "Url_QuotaCycleQty_Edit").Count() > 0;
            GridCommand command = (GridCommand)TempData["CycleQtyGridCommand"];
            TempData["CycleQtyGridCommand"] = command;
            SearchStatementModel searchStatementModel = PrepareCycleQtySearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<QuotaCycleQty>(searchStatementModel, command));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_QuotaCycleQty_Edit")]
        public ActionResult _CycleQtyUpdate(Int32 id, QuotaCycleQty quotaCycleQty, QuotaSearchModel searchModel)
        {
            try
            {
                ViewBag.HaveCycleEditPermission = CurrentUser.Permissions.Where(p => p.PermissionCode == "Url_QuotaCycleQty_Edit").Count() > 0;
                ViewBag.ItemCode = searchModel.ItemCode;
                if (quotaCycleQty.CycleQty <= 0)
                {
                    throw new BusinessException(" 循环数不能小于等于0");
                }
                QuotaCycleQty upQuotaCycleQty = this.genericMgr.FindById<QuotaCycleQty>(id);
                var quotaList = this.genericMgr.FindAll<Quota>("select q from Quota as q where q.Item=? and Isactive=?", new object[] { upQuotaCycleQty.Item, true });
                if (quotaList == null || quotaList.Count == 0)
                {
                    throw new BusinessException(string.Format(" 物料编号{0}没有维护有效的调整数，请确认。 ", upQuotaCycleQty.Item));
                }
                else
                {
                    var supplierCodes = quotaList.Select(q => q.Supplier).ToArray();
                    var flowMasters = new List<FlowMaster>();
                    foreach (var supplier in supplierCodes)
                    {
                        var flowMasterBySupplier = this.genericMgr.FindAll<FlowMaster>("select f from FlowMaster as f where f.IsActive=1 and f.PartyFrom=? and exists( select 1 from FlowDetail as d where d.Flow=f.Code and d.Item=? )", new object[] { supplier, upQuotaCycleQty.Item});
                        if (flowMasterBySupplier == null || flowMasterBySupplier.Count == 0)
                        {
                            throw new BusinessException(string.Format(" 物料编号{0},供应商{1}没有找到有效的采购路线，请确认。 ", upQuotaCycleQty.Item, supplier));
                        }
                        else
                        {
                            flowMasters.Add(flowMasterBySupplier.First());
                        }
                    }
                    var locTos = flowMasters.Select(f => f.LocationTo).ToList();
                    var flowMasterGrooups = (from tak in flowMasters
                                             group tak by tak.PartyFrom
                                                 into result
                                                 select new
                                                 {
                                                     Supplier = result.Key,
                                                     FlowMasters = result.ToList()
                                                 });

                    foreach (var groups in flowMasterGrooups)
                    {
                        foreach (var locTo in locTos)
                        {
                            if (groups.FlowMasters.Where(f => f.LocationTo == locTo).Count() == 0)
                            {
                                throw new BusinessException(string.Format(" 物料编号{0},供应商{1}目的库位{2}没有找到有效的采购路线，与其他供应商不一致，请确认。 ", upQuotaCycleQty.Item, groups.FlowMasters.First().PartyFrom, locTo));
                            }
                        }
                    }
                }
                upQuotaCycleQty.CycleQty = quotaCycleQty.CycleQty;
                this.genericMgr.Update(upQuotaCycleQty);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage("修改失败。" + ex.GetMessages()[0].GetMessageString());
            }
            catch (Exception ex)
            {
                SaveErrorMessage("修改失败。" + ex.Message);
            }
            ViewBag.HaveCycleEditPermission = CurrentUser.Permissions.Where(p => p.PermissionCode == "Url_QuotaCycleQty_Edit").Count() > 0;
            GridCommand command = (GridCommand)TempData["CycleQtyGridCommand"];
            TempData["CycleQtyGridCommand"] = command;
            SearchStatementModel searchStatementModel = PrepareCycleQtySearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<QuotaCycleQty>(searchStatementModel, command));
        }


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_QuotaCycleQty_Edit")]
        public ActionResult _CycleQtyDelete(Int32 id, QuotaSearchModel searchModel)
        {
            try
            {
                ViewBag.HaveCycleEditPermission = CurrentUser.Permissions.Where(p => p.PermissionCode == "Url_QuotaCycleQty_Edit").Count() > 0;
                ViewBag.ItemCode = searchModel.ItemCode;
                this.genericMgr.DeleteById<QuotaCycleQty>(id);
                SaveSuccessMessage("删除成功。");
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage("删除失败。" + ex.GetMessages()[0].GetMessageString());
            }
            catch (Exception ex)
            {
                SaveErrorMessage("删除失败。" + ex.Message);
            }
            ViewBag.HaveCycleEditPermission = CurrentUser.Permissions.Where(p => p.PermissionCode == "Url_QuotaCycleQty_Edit").Count() > 0;
            GridCommand command = (GridCommand)TempData["CycleQtyGridCommand"];
            TempData["CycleQtyGridCommand"] = command;
            SearchStatementModel searchStatementModel = PrepareCycleQtySearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<QuotaCycleQty>(searchStatementModel, command));
        }

        /// <summary>
        /// 循环量导入
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        [SconitAuthorize(Permissions = "Url_QuotaCycleQty_Edit")]
        public ActionResult ImportQuotaCycleQty(IEnumerable<HttpPostedFileBase> attachments)
        {
            try
            {
                foreach (var file in attachments)
                {
                    Stream inputStream = file.InputStream;
                    if (inputStream.Length == 0)
                    {
                        throw new BusinessException("Import.Stream.Empty");
                    }

                    HSSFWorkbook workbook = new HSSFWorkbook(inputStream);
                    ISheet sheet = workbook.GetSheetAt(0);
                    IEnumerator rows = sheet.GetRowEnumerator();
                    ImportHelper.JumpRows(rows, 10);
                    BusinessException businessException = new BusinessException();
                    #region 列定义
                    int colItem = 1;//车系
                    int colCycleQty = 2;//车系
                    #endregion
                    IList<QuotaCycleQty> exactCycleQtyList = new List<QuotaCycleQty>();
                    IList<QuotaCycleQty> allCycleQtyList = this.genericMgr.FindAll<QuotaCycleQty>();
                    IList<Item> allItemList = this.genericMgr.FindAll<Item>();
                    int i = 10;
                    while (rows.MoveNext())
                    {
                        i++;
                        HSSFRow row = (HSSFRow)rows.Current;
                        if (!ImportHelper.CheckValidDataRow(row, 1, 2))
                        {
                            break;//边界
                        }
                        string itemCode = string.Empty;
                        decimal cycleQty = 0;
                        QuotaCycleQty quotaCycleQty = new QuotaCycleQty();
                        #region 读取数据
                        #region 物料编号
                        itemCode = ImportHelper.GetCellStringValue(row.GetCell(colItem));
                        if (string.IsNullOrWhiteSpace(itemCode))
                        {
                            businessException.AddMessage(string.Format("第{0}行物料编号不能为空", i));
                        }
                        else
                        {
                            var items = allItemList.FirstOrDefault(a => a.Code == itemCode);
                            var existsCycleQty = allCycleQtyList.FirstOrDefault(a => a.Item == itemCode);
                            //var duplicateItemTrace=
                            if (items == null)
                            {
                                businessException.AddMessage(string.Format("第{0}行{1}物料编号不存在。", i, itemCode));
                                continue;
                            }
                            else if (existsCycleQty != null)
                            {
                                quotaCycleQty = existsCycleQty;
                                quotaCycleQty.IsUpdate = true;
                            }
                            else
                            {
                                quotaCycleQty.Item = items.Code;
                                quotaCycleQty.ItemDesc = items.Description;
                                quotaCycleQty.RefItemCode = items.ReferenceCode;
                            }
                        }
                        #endregion

                        #region 循环量
                        string readCycleQty = ImportHelper.GetCellStringValue(row.GetCell(colCycleQty));
                        if (string.IsNullOrWhiteSpace(readCycleQty))
                        {
                            businessException.AddMessage(string.Format("第{0}行循环量不能为空", i));
                            continue;
                        }
                        else
                        {
                            if (decimal.TryParse(readCycleQty, out cycleQty))
                            {
                                if (cycleQty <= 0)
                                {
                                    businessException.AddMessage(string.Format("第{0}行循环量{1}不能小于等于0。", i,cycleQty));
                                    continue;
                                }
                                quotaCycleQty.CycleQty = cycleQty;
                            }
                            else
                            {
                                businessException.AddMessage(string.Format("第{0}行循环量{1}填写有误。", i));
                                continue;
                            }
                        }
                        #endregion

                        #region 验证
                        var quotaList = this.genericMgr.FindAll<Quota>("select q from Quota as q where q.Item=? and Isactive=?", new object[] { quotaCycleQty.Item, true });
                        if (quotaList == null || quotaList.Count == 0)
                        {
                            businessException.AddMessage(string.Format("第{0}行:物料编号{1}没有维护有效的调整数，请确认。 ", i, quotaCycleQty.Item));
                            continue;
                        }
                        else
                        {
                            var supplierCodes = quotaList.Select(q => q.Supplier).ToArray();
                            var flowMasters = new List<FlowMaster>();
                            foreach (var supplier in supplierCodes)
                            {
                                var flowMasterBySupplier = this.genericMgr.FindAll<FlowMaster>("select f from FlowMaster as f where f.PartyFrom=? and exists( select 1 from FlowDetail as d where d.Flow=f.Code and d.Item=? )", new object[] { supplier, quotaCycleQty.Item });
                                //var flowMasterBySupplier = this.genericMgr.FindAll<FlowMaster>("select f from FlowMaster as f where f.PartyFrom=?", supplier);
                                if (flowMasterBySupplier == null || flowMasterBySupplier.Count == 0)
                                {
                                    businessException.AddMessage(string.Format(" 第{0}行:物料编号{1},供应商{2}没有找到有效的采购路线，请确认。 ", i, quotaCycleQty.Item, supplier));
                                    continue;
                                }
                                else
                                {
                                    flowMasters.Add(flowMasterBySupplier.First());
                                }
                            }
                            var locTos = flowMasters.Select(f => f.LocationTo).ToList();
                            var flowMasterGrooups = (from tak in flowMasters
                                                     group tak by tak.PartyFrom
                                                         into result
                                                         select new
                                                         {
                                                             Supplier = result.Key,
                                                             FlowMasters = result.ToList()
                                                         });

                            foreach (var groups in flowMasterGrooups)
                            {
                                foreach (var locTo in locTos)
                                {
                                    if (groups.FlowMasters.Where(f => f.LocationTo == locTo).Count() == 0)
                                    {
                                        businessException.AddMessage(string.Format("第"+i+"行: 物料编号{0},供应商{1}目的库位{2}没有找到有效的采购路线，与其他供应商不一致，请确认。 ", quotaCycleQty.Item, groups.FlowMasters.First().PartyFrom, locTo));
                                        continue;
                                    }
                                }
                            }
                        }
                        #endregion

                        exactCycleQtyList.Add(quotaCycleQty);
                        #endregion
                    }
                    if (businessException.HasMessage)
                    {
                        throw businessException;
                    }
                    if (exactCycleQtyList == null || exactCycleQtyList.Count == 0)
                    {
                        throw new BusinessException("模版为空，请确认。");
                    }
                    foreach (QuotaCycleQty cycleQty in exactCycleQtyList)
                    {
                        if (cycleQty.IsUpdate)
                        {
                            this.genericMgr.Update(cycleQty);
                        }
                        else
                        {
                            genericMgr.Create(cycleQty);
                        }
                    }
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


        private SearchStatementModel PrepareCycleQtySearchStatement(GridCommand command, QuotaSearchModel searchModel)
        {
            string whereStatement = string.Empty;
            IList<object> param = new List<object>();

            HqlStatementHelper.AddEqStatement("Item", searchModel.ItemCode, "q", ref whereStatement, param);

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectQuotaCycleQtyCountStatement;
            searchStatementModel.SelectStatement = selectQuotaCycleQtyStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        #endregion

        #region 配额调整量查询
        [SconitAuthorize(Permissions = "Url_SupplierCycle_View")]
        public ActionResult SupplierCycle()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_SupplierCycle_View")]
        public ActionResult SupplierCycleList(GridCommand command, QuotaSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = 100;
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_SupplierCycle_View")]
        public ActionResult _AjaxSupplierCycleList(GridCommand command, QuotaSearchModel searchModel)
        {
            string searchSql = SupplierCycleSearchStatement(searchModel);
            var searchResult = this.genericMgr.FindAllWithNativeSql<object[]>(searchSql);
            //quota.Supplier,quota.SupplierNm,quota.SupplierShortCode,quota.Item,quota.ItemDesc,
            //quota.RefItemCode,quota.Weight,quota.AccumulateQty,quota.AdjQty,qc.CycleQty 
            var allResult = (from tak in searchResult
                             select new Quota
                             {
                                 Supplier = (string)tak[0],
                                 SupplierName = (string)tak[1],
                                 SupplierShortCode = (string)tak[2],
                                 Item = (string)tak[3],
                                 ItemDesc = (string)tak[4],
                                 RefItemCode = (string)tak[5],
                                 Weight = (decimal?)tak[6],
                                 AccumulateQty = (decimal?)tak[7],
                                 AdjQty = (decimal?)tak[8],
                                 CycleQty = (decimal?)tak[9],
                             }).ToList();
            GridModel<Quota> gridModel = new GridModel<Quota>();
            gridModel.Total = allResult.Count;
            gridModel.Data = allResult.Skip((command.Page - 1) * command.PageSize).Take(command.PageSize);
            return PartialView(gridModel);
        }

        public void ExportSupplierXLS(QuotaSearchModel searchModel)
        {
            string searchSql = SupplierCycleSearchStatement(searchModel);
            var searchResult = this.genericMgr.FindAllWithNativeSql<object[]>(searchSql);
            //quota.Supplier,quota.SupplierNm,quota.SupplierShortCode,quota.Item,quota.ItemDesc,
            //quota.RefItemCode,quota.Weight,quota.AccumulateQty,quota.AdjQty,qc.CycleQty 
            var allResult = (from tak in searchResult
                             select new Quota
                             {
                                 Supplier = (string)tak[0],
                                 SupplierName = (string)tak[1],
                                 SupplierShortCode = (string)tak[2],
                                 Item = (string)tak[3],
                                 ItemDesc = (string)tak[4],
                                 RefItemCode = (string)tak[5],
                                 Weight = (decimal?)tak[6],
                                 AccumulateQty = (decimal?)tak[7],
                                 AdjQty = (decimal?)tak[8],
                                 CycleQty = (decimal?)tak[9],
                             }).ToList();
            ExportToXLS<Quota>("ExportSupplierXLS", "xls", allResult);
        }
        #endregion

        private string SupplierCycleSearchStatement(QuotaSearchModel searchModel)
        {
            string searchSql = " select quota.Supplier,quota.SupplierNm,quota.SupplierShortCode,quota.Item,quota.ItemDesc,quota.RefItemCode,quota.Weight,quota.AccumulateQty,quota.AdjQty,qc.CycleQty from SCM_Quota as quota left join SCM_QuotaCycleQty as qc on quota.Item=qc.Item  where 1=1 and quota.Weigh not in(0,100) ";
            if (!string.IsNullOrWhiteSpace(searchModel.ItemCode))
            {
                searchSql += string.Format(" and quota.Item in (select Item from SCM_Quota where Supplier='{0}' and Item ='{1}') ", new object[]{ CurrentUser.Code,searchModel.ItemCode});
            }
            else
            {
                searchSql += string.Format(" and quota.Item in (select Item from SCM_Quota where Supplier='{0}') ", CurrentUser.Code);
            }
            return searchSql;
        }

        #region old
        //[GridAction]
        //[SconitAuthorize(Permissions = "Url_Quota_View")]
        //public ActionResult _Update(string id, FlowDetail flowDetail, QuotaSearchModel searchModel)
        //{
        //    ModelState.Remove("Flow");
        //    ModelState.Remove("Sequence");
        //    ModelState.Remove("Item");
        //    ModelState.Remove("ReferenceItemCode");
        //    ModelState.Remove("Uom");
        //    ModelState.Remove("UnitCount"); 
        //    try
        //    {
        //        string sql = " update SCM_FlowDet set MRPWeight=? ,MRPTotalAdj=? where id=?";
        //        base.genericMgr.FindAllWithNativeSql(sql, new object[] { flowDetail.MrpWeight, flowDetail.MrpTotalAdjust, id });
        //        SaveSuccessMessage("修改成功。");
        //    }
        //    catch (Exception ex)
        //    {
        //        SaveErrorMessage("修改失败。"+ex.Message);
        //    }
        //    GridCommand command = (GridCommand)TempData["GridCommand"];
        //    TempData["GridCommand"] = command;
        //    SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
        //    var data = GetAjaxPageData<FlowDetail>(searchStatementModel, command);
        //    foreach (var flowDet in data.Data)
        //    {
        //        var flowMaster = this.genericMgr.FindById<FlowMaster>(flowDet.Flow);
        //        //flowDet.PartyFrom = flowMaster.PartyFrom;
        //        flowDet.LocationTo = flowMaster.LocationTo;

        //        var supplier = this.genericMgr.FindById<Supplier>(flowMaster.PartyFrom);
        //        flowDet.ManufactureParty = supplier.Code;
        //        flowDet.ManufacturePartyDesc = supplier.CodeDescription;
        //        flowDet.ManufacturePartyShortCode = supplier.ShortCode;

        //        var location = this.genericMgr.FindById<Location>(flowDet.LocationTo);
        //        flowDet.SapLocation = location.SAPLocation;

        //        var item = this.genericMgr.FindById<Item>(flowDet.Item);
        //        flowDet.ItemDescription = item.Description;
        //    }
        //    data.Data = data.Data.OrderBy(d => d.LocationTo).ToList();
        //    string locTo = string.Empty;
        //    foreach (var flowDet in data.Data)
        //    {
        //        var mrpWeight = data.Data.Where(d => d.LocationTo == flowDet.LocationTo).Sum(d => d.MrpWeight);
        //        if (mrpWeight > 0)
        //        {
        //            flowDet.AverageRatio = decimal.Round((Convert.ToDecimal(flowDet.MrpWeight * 100.00) / mrpWeight)).ToString() + "%";
        //        }
        //        else
        //        {
        //            flowDet.AverageRatio = "0%";
        //        }
        //        if (flowDet.LocationTo.Equals(locTo))
        //        {
        //            flowDet.LocationTo = string.Empty;
        //            flowDet.SapLocation = string.Empty;
        //        }
        //        else
        //        {
        //            locTo = flowDet.LocationTo;
        //        }
        //    }
        //    return PartialView(data);
        //}

        //[HttpPost]
        //[SconitAuthorize(Permissions = "Url_Quota_View")]
        ////public string Inproportion(string idStr, string weightStr, string totalQtyStr, string adjQtyStr, string Location, string Item)
        //public JsonResult Inproportion(string idStr, string Location, string Item)
        //{
        //    QuotaSearchModel searchModel = new QuotaSearchModel { Location = Location, Item = Item };
        //    TempData["QuotaSearchModel"] = searchModel;
        //    try
        //    {
        //        if (string.IsNullOrEmpty(idStr))
        //        {
        //            throw new BusinessException("路线明细不能为空");
        //        }
        //        string[] idArr = idStr.Split(',');
        //        //string[] qtyArr = adjQtyStr.Split(',');
        //        //string[] weightArr = weightStr.Split(',');
        //        //string[] totalQtyArr = totalQtyStr.Split(',');

        //        decimal allWeight = 0;
        //        decimal allTotalQty = 0;
        //        decimal maxProportion = 0;
        //        IList<FlowDetail> FlowDetailList = new List<FlowDetail>();
        //        FlowDetail noNeedAdjDetail = new FlowDetail();
        //        for (int i = 0; i < idArr.Count(); i++)
        //        {
        //            FlowDetail flowDetail = base.genericMgr.FindById<FlowDetail>(Convert.ToInt32(idArr[i]));
        //            allWeight += flowDetail.MrpWeight;
        //            allTotalQty += flowDetail.MrpTotal;
        //            if (flowDetail.MrpTotal / flowDetail.MrpWeight > maxProportion)
        //            {
        //                maxProportion = flowDetail.MrpTotal / flowDetail.MrpWeight;
        //                noNeedAdjDetail = flowDetail;
        //            }

        //            FlowDetailList.Add(flowDetail);
        //        }
        //        FlowDetailList.Remove(noNeedAdjDetail);
        //        if (noNeedAdjDetail.MrpTotalAdjust != 0)
        //        {
        //            noNeedAdjDetail.MrpTotalAdjust = 0;
        //            base.genericMgr.Update(noNeedAdjDetail);
        //        }

        //        foreach (var flowDetail in FlowDetailList)
        //        {
        //            flowDetail.MrpTotalAdjust = (noNeedAdjDetail.MrpTotal * flowDetail.MrpWeight) / noNeedAdjDetail.MrpWeight - flowDetail.MrpTotal;
        //            base.genericMgr.Update(flowDetail);
        //        }

        //        //pickListMgr.CreatePickList(orderDetailList);
        //        SaveSuccessMessage(Resources.MRP.Quota.Quota_Adjusted);
        //        return Json(new { });
        //    }
        //    catch (BusinessException ex)
        //    {
        //        SaveBusinessExceptionMessage(ex);
        //    }
        //    catch (Exception ex)
        //    {
        //        SaveErrorMessage(ex);
        //    }
        //    return Json(null);
        //}

        //[GridAction(EnableCustomBinding = true)]
        //[SconitAuthorize(Permissions = "Url_Quota_View")]
        //public JsonResult Clear(string idStr, string Location, string Item)
        //{
        //    QuotaSearchModel searchModel = new QuotaSearchModel { Location = Location, Item = Item };
        //    TempData["QuotaSearchModel"] = searchModel;
        //    try
        //    {
        //        if (string.IsNullOrEmpty(idStr))
        //        {
        //            throw new BusinessException("路线明细不能为空");
        //        }
        //        string[] idArr = idStr.Split(',');

        //        for (int i = 0; i < idArr.Count(); i++)
        //        {
        //            FlowDetail flowDetail = base.genericMgr.FindById<FlowDetail>(Convert.ToInt32(idArr[i]));
        //            flowDetail.MrpTotalAdjust = 0;
        //            base.genericMgr.Update(flowDetail);

        //        }

        //        SaveSuccessMessage(Resources.MRP.Quota.Quota_Cleared);
        //        return Json(new { });
        //    }
        //    catch (BusinessException ex)
        //    {
        //        SaveBusinessExceptionMessage(ex);
        //    }
        //    catch (Exception ex)
        //    {
        //        SaveErrorMessage(ex);
        //    }
        //    return Json(null);
        //}

        //[SconitAuthorize(Permissions = "Url_Quota_View")]
        //public JsonResult Save(string idStr, string adjQtyStr, string Location, string Item)
        //{
        //    QuotaSearchModel searchModel = new QuotaSearchModel { Location = Location, Item = Item };
        //    TempData["QuotaSearchModel"] = searchModel;
        //    try
        //    {
        //        if (string.IsNullOrEmpty(idStr))
        //        {
        //            throw new BusinessException("路线明细不能为空");
        //        }
        //        string[] idArr = idStr.Split(',');
        //        string[] qtyArr = adjQtyStr.Split(',');

        //        for (int i = 0; i < idArr.Count(); i++)
        //        {
        //            FlowDetail flowDetail = base.genericMgr.FindById<FlowDetail>(Convert.ToInt32(idArr[i]));
        //            flowDetail.MrpTotalAdjust = decimal.Parse(qtyArr[i]);
        //            base.genericMgr.Update(flowDetail);

        //        }

        //        //pickListMgr.CreatePickList(orderDetailList);
        //        SaveSuccessMessage(Resources.MRP.Quota.Quota_Saved);
        //        return Json(new { });
        //    }
        //    catch (BusinessException ex)
        //    {
        //        SaveBusinessExceptionMessage(ex);
        //    }
        //    catch (Exception ex)
        //    {
        //        SaveErrorMessage(ex);
        //    }
        //    return Json(null);
        //}

        #endregion

    }
}
