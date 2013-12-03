using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using com.Sconit.Service;
using com.Sconit.Web.Util;
using Telerik.Web.Mvc;
using com.Sconit.Web.Models;
using com.Sconit.Entity.CUST;
using com.Sconit.Web.Models.SearchModels.CUST;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.Exception;

namespace com.Sconit.Web.Controllers.CUST
{
    public class OpRefMapController : WebAppBaseController
    {

        private static string selectCountStatement = "select count(*) from OpRefMap as o";

        private static string selectStatement = "from OpRefMap as o";


        public IProductionLineMgr prodLineMgr { get; set; }
        //
        // GET: /OpRefMap/
        #region  public 
        public ActionResult Index()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OpRefMap_View")]
        [GridAction]
        public ActionResult List(GridCommand command, OpRefMapSearchModel searchModel)
        {
            TempData["OpRefMapSearchModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OpRefMap_View")]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxList(GridCommand command, OpRefMapSearchModel searchModel)
        {
            TempData["GridCommand"] = command;
            TempData["searchModel"] = searchModel;
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<OpRefMap>(searchStatementModel, command));
        }


        [GridAction]
        [SconitAuthorize(Permissions = "Url_OpRefMap_View")]
        public ActionResult _Insert(OpRefMap oprefMap)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    IList<OpRefMap> oprefMapList = base.genericMgr.FindAll<OpRefMap>("from OpRefMap as o where o.Item=? and o.ProdLine=?", new object[] { oprefMap.Item, oprefMap.ProdLine });
                    if (oprefMapList.Count > 0)
                    {
                        throw new BusinessException(string.Format("生产线{0}+物料代码{1}已经存在。", oprefMap.ProdLine, oprefMap.Item));
                    }
                    else
                    {
                        if (oprefMap.IsPrimary.HasValue && oprefMap.IsPrimary.Value)
                        {
                            var checkPrimary = this.genericMgr.FindAll<OpRefMap>(" from OpRefMap as o where o.Item=? and o.OpReference=? and o.IsPrimary=? ", new object[] { oprefMap.Item, oprefMap.OpReference, true });
                            if (checkPrimary != null && checkPrimary.Count > 0)
                            {
                                throw new BusinessException(string.Format("【物料编号{0}+JIT计算工位{1}】在数据库中已经存在优先的", oprefMap.Item, oprefMap.OpReference));
                            }
                        }

                        Item item = this.genericMgr.FindById<Item>(oprefMap.Item);
                        oprefMap.Item = item.Code;
                        oprefMap.ItemDesc = item.Description;
                        oprefMap.ItemRefCode = item.ReferenceCode;
                        base.genericMgr.Create(oprefMap);
                        SaveSuccessMessage("添加成功。");
                    }
                }
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            catch (Exception e)
            {
                SaveErrorMessage(e.Message);
            }

            GridCommand command = (GridCommand)TempData["GridCommand"];
            OpRefMapSearchModel searchModel = (OpRefMapSearchModel)TempData["searchModel"];
            TempData["GridCommand"] = command;
            TempData["searchModel"] = searchModel;
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<OpRefMap>(searchStatementModel, command));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OpRefMap_View")]
        public ActionResult _Delete(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<OpRefMap>(Convert.ToInt32(Id));
                SaveSuccessMessage("删除成功。");
            }
            GridCommand command = (GridCommand)TempData["GridCommand"];
            OpRefMapSearchModel searchModel = (OpRefMapSearchModel)TempData["searchModel"];
            TempData["GridCommand"] = command;
            TempData["searchModel"] = searchModel;
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<OpRefMap>(searchStatementModel, command));
        }


        [GridAction]
        [SconitAuthorize(Permissions = "Url_OpRefMap_View")]
        public ActionResult _Update(OpRefMap oprefMap, string id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    IList<OpRefMap> oprefMapList = base.genericMgr.FindAll<OpRefMap>("from OpRefMap as o where o.Item=? and o.ProdLine=? and o.Id <> ?", new object[] { oprefMap.Item, oprefMap.ProdLine, oprefMap.Id });
                    if (oprefMapList.Count > 0)
                    {
                        throw new BusinessException(string.Format("生产线{0}+物料代码{1}已经存在。", oprefMap.ProdLine, oprefMap.Item));
                    }
                    else
                    {
                        if (oprefMap.IsPrimary.HasValue && oprefMap.IsPrimary.Value)
                        {
                            var checkPrimary = this.genericMgr.FindAll<OpRefMap>(" from OpRefMap as o where o.Item=? and o.OpReference=? and o.IsPrimary=?  and  o.Id <> ? ", new object[] { oprefMap.Item, oprefMap.OpReference, true, oprefMap.Id });
                            if (checkPrimary != null && checkPrimary.Count > 0)
                            {
                                throw new BusinessException(string.Format("【物料编号{0}+JIT计算工位{1}】在数据库中已经存在优先的", oprefMap.Item, oprefMap.OpReference));
                            }
                        }
                        OpRefMap upOpRefMap = base.genericMgr.FindById<OpRefMap>(Convert.ToInt32(id));
                        Item item = this.genericMgr.FindById<Item>(oprefMap.Item);
                        upOpRefMap.Location = oprefMap.Location;
                        upOpRefMap.ProdLine = oprefMap.ProdLine;
                        upOpRefMap.SAPProdLine = oprefMap.SAPProdLine;
                        upOpRefMap.Item = item.Code;
                        upOpRefMap.ItemDesc = item.Description;
                        upOpRefMap.ItemRefCode = item.ReferenceCode;
                        upOpRefMap.OpReference = oprefMap.OpReference;
                        upOpRefMap.RefOpReference = oprefMap.RefOpReference;
                        upOpRefMap.IsPrimary = oprefMap.IsPrimary;
                        base.genericMgr.Update(upOpRefMap);
                        SaveSuccessMessage("修改成功。");
                    }
                }
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            catch (Exception e)
            {
                SaveErrorMessage(e.Message);
            }
            GridCommand command = (GridCommand)TempData["GridCommand"];
            OpRefMapSearchModel searchModel = (OpRefMapSearchModel)TempData["searchModel"];
            TempData["GridCommand"] = command;
            TempData["searchModel"] = searchModel;
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<OpRefMap>(searchStatementModel, command));
        }

        public ActionResult _GetItem(string itemCode)
        {
            if (!string.IsNullOrWhiteSpace(itemCode))
            {
                var itemEntity = this.genericMgr.FindById<Item>(itemCode);
                return this.Json(itemEntity);
            }
            else
            {
                return null;
            }
        }

        public void ExportOpRefMapXls(OpRefMapSearchModel searchModel)
        {
            string hql = " select o from OpRefMap as o where 1=1  ";
            IList<object> parameter = new List<object>();

            if (!string.IsNullOrWhiteSpace(searchModel.ItemSearch))
            {
                hql += " and Item=? ";
                parameter.Add(searchModel.ItemSearch);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.SAPProdLineSearch))
            {
                hql += " and SAPProdLine=? ";
                parameter.Add(searchModel.SAPProdLineSearch);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.ProdLineSearch))
            {
                hql += " and ProdLine=? ";
                parameter.Add(searchModel.ProdLineSearch);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.OpReferenceSearch))
            {
                hql += " and OpReference=? ";
                parameter.Add(searchModel.OpReferenceSearch);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.CreateUserNameSearch))
            {
                hql += " and CreateUserName like ? ";
                parameter.Add(searchModel.CreateUserNameSearch+"%");
            }
            if (searchModel.IsPrimarySearch.HasValue && searchModel.IsPrimarySearch.Value)
            {
                hql += " and IsPrimary=? ";
                parameter.Add(searchModel.IsPrimarySearch.Value);
            }
            if (searchModel.CreateStartDate.HasValue)
            {
                hql += " and CreateDate>=? ";
                parameter.Add(searchModel.CreateStartDate.Value);
            }
            if (searchModel.CreateEndDate.HasValue)
            {
                hql += " and CreateDate<=? ";
                parameter.Add(searchModel.CreateEndDate.Value);
            }
            var exportList = this.genericMgr.FindAll<OpRefMap>(hql, parameter.ToArray());

            ExportToXLS<OpRefMap>("OpRefMapXls", "XLS", exportList);
        }

        [SconitAuthorize(Permissions = "Url_ItemTrace_View")]
        public ActionResult ImportOpRefMap(IEnumerable<HttpPostedFileBase> attachments)
        {
            try
            {
                foreach (var file in attachments)
                {
                    prodLineMgr.ImportOpRefMap(file.InputStream);
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

        #region private
        private SearchStatementModel PrepareSearchStatement(GridCommand command, OpRefMapSearchModel searchModel)
        {
            string whereStatement = string.Empty;
            IList<object> param = new List<object>();

            HqlStatementHelper.AddEqStatement("ProdLine", searchModel.ProdLineSearch, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("SAPProdLine", searchModel.SAPProdLineSearch, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.ItemSearch, "o", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("OpReference", searchModel.OpReferenceSearch, "o", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("CreateUserName", searchModel.CreateUserNameSearch, HqlStatementHelper.LikeMatchMode.Start, "o", ref whereStatement, param);
            if (searchModel.IsPrimarySearch.HasValue && searchModel.IsPrimarySearch.Value)
            {
                HqlStatementHelper.AddEqStatement("IsPrimary", searchModel.IsPrimarySearch.Value, "o", ref whereStatement, param);
            }

            if (searchModel.CreateStartDate != null & searchModel.CreateEndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.CreateStartDate, searchModel.CreateEndDate, "o", ref whereStatement, param);
            }
            else if (searchModel.CreateStartDate != null & searchModel.CreateEndDate == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.CreateStartDate, "o", ref whereStatement, param);
            }
            else if (searchModel.CreateStartDate == null & searchModel.CreateEndDate != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.CreateEndDate, "o", ref whereStatement, param);
            }
           
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

    }
}
