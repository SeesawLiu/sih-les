using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using com.Sconit.Service;
using com.Sconit.Web.Util;
using Telerik.Web.Mvc;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels.INV;
using com.Sconit.Entity.INV;
using com.Sconit.Entity.MD;
using NHibernate.Type;
using com.Sconit.Entity.Exception;

namespace com.Sconit.Web.Controllers.INV
{
    public class LocationDetailPrefController : WebAppBaseController
    {

        private static string selectCountStatement = "select count(*) from LocationDetailPref as f";

        private static string selectStatement = "from LocationDetailPref as f";

        public ILocationDetailMgr locationMgr { get; set; }

        //
        // GET: /LocationDetailPref/
        #region  public 
        public ActionResult Index()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_LocationDetailPref_View")]
        [GridAction]
        public ActionResult List(GridCommand command, LocationDetailPrefSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [SconitAuthorize(Permissions = "Url_LocationDetailPref_View")]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxList(GridCommand command, LocationDetailPrefSearchModel searchModel)
        {
            TempData["GridCommand"] = command;
            TempData["searchModel"] = searchModel;
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            GridModel<LocationDetailPref> gridModel = GetAjaxPageData<LocationDetailPref>(searchStatementModel, command);
            foreach (var locDetPref in gridModel.Data)
            {
                locDetPref.ReferenceItemCode = this.genericMgr.FindById<Item>(locDetPref.Item).ReferenceCode;
            }
            return PartialView(gridModel);
        }


        [GridAction]
        [SconitAuthorize(Permissions = "Url_LocationDetailPref_View")]
        public ActionResult _Insert(LocationDetailPref LocationDetailPref)
        {
            if (CheckLocDetPref(LocationDetailPref))
            {
                this.genericMgr.Create(LocationDetailPref);
                SaveSuccessMessage("添加成功。");
            }
            GridCommand command = (GridCommand)TempData["GridCommand"];
            LocationDetailPrefSearchModel searchModel = (LocationDetailPrefSearchModel)TempData["searchModel"];
            TempData["GridCommand"] = command;
            TempData["searchModel"] = searchModel;
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            GridModel<LocationDetailPref> gridModel = GetAjaxPageData<LocationDetailPref>(searchStatementModel, command);
            foreach (var locDetPref in gridModel.Data)
            {
                locDetPref.ReferenceItemCode = this.genericMgr.FindById<Item>(locDetPref.Item).ReferenceCode;
            }
            return PartialView(gridModel);
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_LocationDetailPref_View")]
        public ActionResult _Update(LocationDetailPref locationDetailPref, string id)
        {
            if (CheckLocDetPref(locationDetailPref))
            {
                LocationDetailPref upLocationDetailPref = base.genericMgr.FindById<LocationDetailPref>(Convert.ToInt32(id));
                upLocationDetailPref.Item = locationDetailPref.Item;
                upLocationDetailPref.ItemDesc = this.genericMgr.FindById<Item>(locationDetailPref.Item).Description;
                upLocationDetailPref.Location = locationDetailPref.Location;
                upLocationDetailPref.MaxStock = locationDetailPref.MaxStock;
                upLocationDetailPref.SafeStock = locationDetailPref.SafeStock;
                this.genericMgr.Update(upLocationDetailPref);
                SaveSuccessMessage("修改成功。");
            }
            GridCommand command = (GridCommand)TempData["GridCommand"];
            LocationDetailPrefSearchModel searchModel = (LocationDetailPrefSearchModel)TempData["searchModel"];
            TempData["GridCommand"] = command;
            TempData["searchModel"] = searchModel;
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            GridModel<LocationDetailPref> gridModel = GetAjaxPageData<LocationDetailPref>(searchStatementModel, command);
            foreach (var locDetPref in gridModel.Data)
            {
                locDetPref.ReferenceItemCode = this.genericMgr.FindById<Item>(locDetPref.Item).ReferenceCode;
            }
            return PartialView(gridModel);
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_LocationDetailPref_View")]
        public ActionResult _Delete(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<LocationDetailPref>(Convert.ToInt32(Id));
                SaveSuccessMessage("删除成功。");
            }
            GridCommand command=(GridCommand)TempData["GridCommand"];
            LocationDetailPrefSearchModel searchModel=(LocationDetailPrefSearchModel)TempData["searchModel"];
            TempData["GridCommand"] = command;
            TempData["searchModel"] = searchModel;
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            GridModel<LocationDetailPref> gridModel = GetAjaxPageData<LocationDetailPref>(searchStatementModel, command);
            foreach (var locDetPref in gridModel.Data)
            {
                locDetPref.ReferenceItemCode = this.genericMgr.FindById<Item>(locDetPref.Item).ReferenceCode;
            }
            return PartialView(gridModel);
        }

        public void ExportXls(LocationDetailPrefSearchModel searchModel)
        {
            string hql = " select l from LocationDetailPref as l where 1=1  ";
            IList<object> parameter = new List<object>();

            if (!string.IsNullOrWhiteSpace(searchModel.ItemCode))
            {
                hql += " and Item=? ";
                parameter.Add(searchModel.ItemCode);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.LocationCode))
            {
                hql += " and Location=? ";
                parameter.Add(searchModel.LocationCode);
            }
            var exportList = this.genericMgr.FindAll<LocationDetailPref>(hql,parameter.ToArray());
            foreach (var locDetPref in exportList)
            {
                locDetPref.ReferenceItemCode = this.genericMgr.FindById<Item>(locDetPref.Item).ReferenceCode;
            }
            ExportToXLS<LocationDetailPref>("ExportXls", "XLS", exportList);
        }

        [SconitAuthorize(Permissions = "Url_ItemTrace_View")]
        public ActionResult ImportLocDetPref(IEnumerable<HttpPostedFileBase> attachments)
        {
            try
            {
                foreach (var file in attachments)
                {
                    locationMgr.ImportLocDetPrefXls(file.InputStream);
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
        private SearchStatementModel PrepareSearchStatement(GridCommand command, LocationDetailPrefSearchModel searchModel)
        {
            string whereStatement = string.Empty;
            IList<object> param = new List<object>();

            HqlStatementHelper.AddEqStatement("Item", searchModel.ItemCode, "f", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Location", searchModel.LocationCode, "f", ref whereStatement, param);
           
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        private bool CheckLocDetPref(LocationDetailPref locationDetailPref)
        {
            bool hasError = false;
            if (string.IsNullOrWhiteSpace(locationDetailPref.Item))
            {
                hasError = true;
                SaveErrorMessage("物料编号不能为空。");
            }
            if (string.IsNullOrWhiteSpace(locationDetailPref.Location))
            {
                hasError = true;
                SaveErrorMessage("库位不能为空。");
            }
            if (locationDetailPref.SafeStock<=0)
            {
                hasError = true;
                SaveErrorMessage("安全库存必须大于0。");
            }
            if (locationDetailPref.MaxStock <= 0)
            {
                hasError = true;
                SaveErrorMessage("最大库存必须大于0。");
            }
            if (this.genericMgr.FindAllWithNativeSql<int>(" select isnull(count(*),0) as counts from INV_LocationDetPref where Item=? and Location=? and Id <>? ", new object[] { locationDetailPref.Item, locationDetailPref.Location, locationDetailPref.Id }, new IType[] { NHibernate.NHibernateUtil.String, NHibernate.NHibernateUtil.String, NHibernate.NHibernateUtil.Int32 })[0] > 0)
            {
                hasError = true;
                SaveErrorMessage("物料编号+库位已经维护，请确认！");
            }
            return !hasError;
        }
        #endregion

    }
}
