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

namespace com.Sconit.Web.Controllers.CUST
{
    public class CreateOrderCodeController : WebAppBaseController
    {

        private static string selectCountStatement = "select count(*) from CreateOrderCode as f";

        private static string selectStatement = "from CreateOrderCode as f";
        
        //
        // GET: /CreateOrderCode/
        #region  public 
        public ActionResult Index()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_CreateOrderCode_View")]
        [GridAction]
        public ActionResult List(GridCommand command, CreateOrderSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [SconitAuthorize(Permissions = "Url_CreateOrderCode_View")]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxList(GridCommand command, CreateOrderSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<CreateOrderCode>(searchStatementModel, command));
        }


        [GridAction]
        [SconitAuthorize(Permissions = "Url_CreateOrderCode_View")]
        public ActionResult _Insert(GridCommand command,  CreateOrderCode createOrderCode)
        {
            if (ModelState.IsValid)
            {
                IList<CreateOrderCode> CreateOrderByCode = base.genericMgr.FindAll<CreateOrderCode>("from CreateOrderCode as f where f.Code=?", createOrderCode.Code);
                if (CreateOrderByCode.Count > 0)
                {
                    SaveErrorMessage("代码已经存在");
                   
                }
                else
                {
                    base.genericMgr.Create(createOrderCode);
                    SaveSuccessMessage("新增成功。");
                }
            }
            CreateOrderSearchModel searchModel = TempData["CreateOrderSearchModel"] != null ? TempData["CreateOrderSearchModel"] as CreateOrderSearchModel : new CreateOrderSearchModel();
            TempData["CreateOrderSearchModel"] = searchModel;
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<CreateOrderCode>(searchStatementModel, command));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_CreateOrderCode_View")]
        public ActionResult _Delete(GridCommand command, string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<CreateOrderCode>(Id);
                SaveSuccessMessage("删除成功。");
            }
            CreateOrderSearchModel searchModel = TempData["CreateOrderSearchModel"] != null ? TempData["CreateOrderSearchModel"] as CreateOrderSearchModel : new CreateOrderSearchModel();
            TempData["CreateOrderSearchModel"] = searchModel;
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<CreateOrderCode>(searchStatementModel, command));
        }


        [GridAction]
        [SconitAuthorize(Permissions = "Url_CreateOrderCode_View")]
        public ActionResult _Update(GridCommand command, CreateOrderCode CreateOrderCode, string id)
        {
            ModelState.Remove("Code");
            CreateOrderCode newCreateOrderCode = base.genericMgr.FindById<CreateOrderCode>(id);
            newCreateOrderCode.Code = id;
            newCreateOrderCode.Description = CreateOrderCode.Description;
            base.genericMgr.Update(newCreateOrderCode);
            SaveSuccessMessage("修改成功。");
            CreateOrderSearchModel searchModel = TempData["CreateOrderSearchModel"] != null ? TempData["CreateOrderSearchModel"] as CreateOrderSearchModel : new CreateOrderSearchModel();
            TempData["CreateOrderSearchModel"] = searchModel;
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<CreateOrderCode>(searchStatementModel, command));
        }


        #endregion

        #region private
        private SearchStatementModel PrepareSearchStatement(GridCommand command, CreateOrderSearchModel searchModel)
        {
            string whereStatement = string.Empty;
            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("Code", searchModel.Code, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Description", searchModel.Description, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
           
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
