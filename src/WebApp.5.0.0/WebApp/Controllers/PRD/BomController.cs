/// <summary>
/// Summary description for BomController
/// </summary>
namespace com.Sconit.Web.Controllers.PRD
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Security;
    using com.Sconit.Entity.PRD;
    using com.Sconit.Entity.SYS;
    using com.Sconit.Service;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Models.SearchModels.PRD;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;
    using System.Web.Routing;
    
    public class BomController : WebAppBaseController
    {
        private static string selectBomMasterCountStatement = "select count(*) from BomMaster as bm";
        private static string selectBomMasterStatement = "select bm from BomMaster as bm";
        private static string DuiplicateBomMasterVerifyStatement = @"select count(*) from BomMaster as bm where bm.Code = ?";

        private static string selectBomDetailCountStatement = "select count(*) from BomDetail as bd";
        private static string selectBomDetailStatement = "select bd from BomDetail as bd";
        private static string DuiplicateBomDetailVerifyStatement = @"select count(*) from BomDetail as bd where bd.Bom = ? and bd.Item = ?";

        [SconitAuthorize(Permissions = "Url_Bom_View")]
        public ActionResult Index()
        {
            return View();
        }

        #region BomMaster
        [SconitAuthorize(Permissions = "Url_Bom_View")]
        public ActionResult _Search_Master()
        {
            return PartialView();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Bom_View")]
        public ActionResult List_Master(GridCommand command, BomMasterSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Bom_View")]
        public ActionResult _AjaxList_Master(GridCommand command, BomMasterSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareBomMasterSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<BomMaster>(searchStatementModel, command));
        }

        [SconitAuthorize(Permissions = "Url_Bom_Edit")]
        public ActionResult _New_Master()
        {
            return PartialView();
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_Bom_Edit")]
        public ActionResult _New_Master(BomMaster bomMaster)
        {
            if (ModelState.IsValid)
            {
                //判断描述不能重复
                if (base.genericMgr.FindAll<long>(DuiplicateBomMasterVerifyStatement, new object[] { bomMaster.Code })[0] > 0)
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Existing_Code, bomMaster.Code);
                }
                else
                {
                    base.genericMgr.Create(bomMaster);
                    SaveSuccessMessage(Resources.PRD.Bom.BomMaster_Added);
                    return RedirectToAction("_Edit_Master/" + bomMaster.Code);
                }
            }
            return PartialView(bomMaster);
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_Bom_Edit")]
        public ActionResult _Edit_Master(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            else
            {
                BomMaster bomMaster = base.genericMgr.FindById<BomMaster>(id);
                return PartialView(bomMaster);
            }
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_Bom_Edit")]
        public ActionResult _Edit_Master(BomMaster bomMaster)
        {
            if (ModelState.IsValid)
            {
                base.genericMgr.Update(bomMaster);
                SaveSuccessMessage(Resources.PRD.Bom.BomMaster_Updated);
            }

            return PartialView(bomMaster);
        }

        [SconitAuthorize(Permissions = "Url_Bom_Delete")]
        public ActionResult Delete_Master(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<BomMaster>(id);
                SaveSuccessMessage(Resources.PRD.Bom.BomMaster_Deleted);
                return RedirectToAction("List_Master");
            }
        }
        #endregion

        #region BomDetail
        [SconitAuthorize(Permissions = "Url_Bom_View")]
        public ActionResult _Search_Detail()
        {
            return PartialView();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Bom_View")]
        public ActionResult List_Detail(GridCommand command, BomDetailSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Bom_View")]
        public ActionResult _AjaxList_Detail(GridCommand command, BomDetailSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareBomDetailSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<BomDetail>(searchStatementModel, command));
        }

        [SconitAuthorize(Permissions = "Url_Bom_Edit")]
        public ActionResult _New_Detail()
        {
            BomDetail bomDetail = new BomDetail();
            bomDetail.ScrapPercentage = 0;
            return PartialView(bomDetail);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_Bom_Edit")]
        public ActionResult _New_Detail(BomDetail bomDetail)
        {
            ModelState.Remove("Item.Description");
            if (ModelState.IsValid)
            {
                //判断描述不能重复
                if (base.genericMgr.FindAll<long>(DuiplicateBomDetailVerifyStatement, new object[] { bomDetail.Bom,bomDetail.Item })[0] > 0)
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Existing_Code, bomDetail.Id.ToString());
                }
                else
                {
                    base.genericMgr.Create(bomDetail);
                    SaveSuccessMessage(Resources.PRD.Bom.BomDetail_Added);
                    return RedirectToAction("_Edit_Detail/" + bomDetail.Id);
                }
            }
            return PartialView(bomDetail);
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_Bom_Edit")]
        public ActionResult _Edit_Detail(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            else
            {
                BomDetail bomDetail = base.genericMgr.FindById<BomDetail>(int.Parse(id));
                return PartialView(bomDetail);
            }
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_Bom_Edit")]
        public ActionResult _Edit_Detail(BomDetail bomDetail)
        {
            ModelState.Remove("Item.Description");
            if (ModelState.IsValid)
            {
                base.genericMgr.Update(bomDetail);
                SaveSuccessMessage(Resources.PRD.Bom.BomDetail_Updated);
            }

            return PartialView(bomDetail);
        }

        [SconitAuthorize(Permissions = "Url_Bom_Delete")]
        public ActionResult Delete_Detail(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<BomDetail>(int.Parse(id));
                SaveSuccessMessage(Resources.PRD.Bom.BomDetail_Deleted);
                return RedirectToAction("List_Detail");
            }
        }
        #endregion

        private SearchStatementModel PrepareBomMasterSearchStatement(GridCommand command, BomMasterSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("Code", searchModel.BomMaster_Code, HqlStatementHelper.LikeMatchMode.Start, "bm", ref whereStatement, param);
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectBomMasterCountStatement;
            searchStatementModel.SelectStatement = selectBomMasterStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        private SearchStatementModel PrepareBomDetailSearchStatement(GridCommand command, BomDetailSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("Bom", searchModel.BomDetail_Bom, HqlStatementHelper.LikeMatchMode.Start, "bd", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Code", searchModel.BomDetail_Item, HqlStatementHelper.LikeMatchMode.Start, "bd.Item", ref whereStatement, param);

            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "StructureTypeDescription")
                {
                    command.SortDescriptors[0].Member = "StructureType";
                }
                if (command.SortDescriptors[0].Member == "BackFlushMethodDescription")
                {
                    command.SortDescriptors[0].Member = "BackFlushMethod";
                }
                if (command.SortDescriptors[0].Member == "FeedMethodDescription")
                {
                    command.SortDescriptors[0].Member = "FeedMethod";
                }
            }
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectBomDetailCountStatement;
            searchStatementModel.SelectStatement = selectBomDetailStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }
    
    }

}
