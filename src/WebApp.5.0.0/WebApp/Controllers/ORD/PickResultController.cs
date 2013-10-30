using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using com.Sconit.Service;
using com.Sconit.Web.Util;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.UI;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels.ORD;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.Exception;

namespace com.Sconit.Web.Controllers.ORD
{
    public class PickResultController : WebAppBaseController
    {
        public IPickTaskMgr pickTaskMgr { get; set; }

        [SconitAuthorize(Permissions = "Url_PickResult_View")]
        public ActionResult Index()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_PickResult_View")]
        public ActionResult List(GridCommand command, PickTaskSearchModel searchModel)
        {
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.SearchModel = searchModel;
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_PickResult_View")]
        public ActionResult _AjaxList(GridCommand command, PickTaskSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<PickResult>(searchStatementModel, command));
        }

        public JsonResult CancelPickResult(string ResultId)
        {
            PickResult result = base.genericMgr.FindAll<PickResult>("from PickResult where ResultId = ? ",
                ResultId).SingleOrDefault();
            if (result != null)
            {
                try
                {
                    pickTaskMgr.CancelPickResult(result);

                    SaveSuccessMessage("取消拣货结果成功,ID:" + result.ResultId);

                    return Json(new { result.ResultId });
                }
                catch (BusinessException ex)
                {
                    SaveBusinessExceptionMessage(ex);
                    return Json(null);
                }
            }
            else
            {
                SaveErrorMessage("拣货结果不存在");
                return Json(null);
            }
        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, PickTaskSearchModel searchModel)
        {
            string whereStatement = string.Empty;
            
            IList<object> param = new List<object>();
            HqlStatementHelper.AddEqStatement("PickId", searchModel.PickId, "c", ref whereStatement, param);

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = "select count(*) from PickResult  c";
            searchStatementModel.SelectStatement = "select c from PickResult as c";
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }
    }
}
