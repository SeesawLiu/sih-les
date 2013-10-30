using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using com.Sconit.Service;
using com.Sconit.Web.Util;
using Telerik.Web.Mvc;
using com.Sconit.Entity.CUST;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels.CUST;
using com.Sconit.Entity.Exception;

namespace com.Sconit.Web.Controllers.CUST
{
    public class ItemTraceController : WebAppBaseController
    {
        //
        // GET: /ItemTrace/

        private static string selectCountStatement = "select count(*) from ItemTrace as i";

        private static string selectStatement = "from ItemTrace as i";

        public IItemMgr itemMgr { get; set; }
        //
        // GET: /FailCode/
        #region  public
        public ActionResult Index()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_ItemTrace_View")]
        [GridAction]
        public ActionResult List(GridCommand command, ItemTraceSearchModel searchModel)
        {
            TempData["ItemTrace"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [SconitAuthorize(Permissions = "Url_ItemTrace_View")]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxList(GridCommand command, ItemTraceSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<ItemTrace>(searchStatementModel, command));
        }


        [GridAction]
        [SconitAuthorize(Permissions = "Url_ItemTrace_View")]
        public ActionResult _Delete(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<ItemTrace>(Id);
                SaveSuccessMessage("删除成功。");
            }
            IList<ItemTrace> ItemTraceList = base.genericMgr.FindAll<ItemTrace>();
            return PartialView(new GridModel(ItemTraceList));
        }


        [SconitAuthorize(Permissions = "Url_ItemTrace_View")]
        public ActionResult New()
        {

            return View();
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_ItemTrace_View")]
        public ActionResult New(ItemTrace itemTrace)
        {
            if (ModelState.IsValid)
            {
                IList<ItemTrace> itemTraceList = base.genericMgr.FindAll<ItemTrace>("from ItemTrace as i where i.Item=?", itemTrace.Item);
                if (itemTraceList.Count > 0)
                {
                    SaveErrorMessage("关键件已经存在");

                }
                else{
                base.genericMgr.Create(itemTrace);
                SaveSuccessMessage("添加成功");
                return RedirectToAction("List");
                }
            }
            return View(itemTrace);
        }

        [SconitAuthorize(Permissions = "Url_ItemTrace_View")]
        public ActionResult ImportItemTrace(IEnumerable<HttpPostedFileBase> attachments)
        {
            try
            {
                foreach (var file in attachments)
                {
                    itemMgr.CreateItemTraceXls(file.InputStream);
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
        private SearchStatementModel PrepareSearchStatement(GridCommand command, ItemTraceSearchModel searchModel)
        {
            string whereStatement = string.Empty;
            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("Item", searchModel.Item, HqlStatementHelper.LikeMatchMode.End, "i", ref whereStatement, param);

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
