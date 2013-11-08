using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using com.Sconit.Service;
using Telerik.Web.Mvc;
using com.Sconit.Web.Util;
using com.Sconit.Web.Models;
using com.Sconit.Entity.INV;
using com.Sconit.Web.Models.SearchModels.INV;
using com.Sconit.Utility;
using com.Sconit.Entity.ACC;
using com.Sconit.Entity.SCM;
using com.Sconit.Entity.Exception;
using NHibernate.Type;
using NHibernate;
using com.Sconit.Entity.MD;

namespace com.Sconit.Web.Controllers.INV
{
    public class OpReferenceBalanceController : WebAppBaseController
    {
        public IStockTakeMgr stockTakeMgr { get; set; }

        private static string selectCountStatement = "select count(*) from OpReferenceBalance as l";

        private static string selectStatement = "from  OpReferenceBalance as l";

        #region 查询

        public ActionResult Index()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OpReferenceBalance_View")]
        public ActionResult List(GridCommand command, OpReferenceBalanceSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            //if (this.CheckSearchModelIsNull(searchCacheModel.SearchObject))
            //{
            //    TempData["_AjaxMessage"] = "";
            //}
            //else
            //{
            //    SaveWarningMessage(Resources.ErrorMessage.Errors_NoConditions);
            //}
            ViewBag.PageSize = this.ProcessPageSize(command.PageSize);

            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OpReferenceBalance_View")]
        public ActionResult _AjaxList(GridCommand command, OpReferenceBalanceSearchModel searchModel)
        {
            //if (!this.CheckSearchModelIsNull(searchModel))
            //{
            //    return PartialView(new GridModel(new List<OpReferenceBalance>()));
            //}
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            GridModel<OpReferenceBalance> gridModel = GetAjaxPageData<OpReferenceBalance>(searchStatementModel, command);
            if (gridModel.Data != null)
            {
                foreach (var opReferenceBalance in gridModel.Data)
                {
                    Item item = this.genericMgr.FindById<Item>(opReferenceBalance.Item);
                    opReferenceBalance.ReferenceItemCode = item.ReferenceCode;
                    opReferenceBalance.ItemDescription = item.Description;
                    //User createUser = base.genericMgr.FindById<User>(opReferenceBalance.CreateUserId);
                    //opReferenceBalance.CreateUserName = createUser.FirstName + createUser.LastName;
                }
            }
            return PartialView(gridModel);
        }

        public void ExportXLS(OpReferenceBalanceSearchModel searchModel)
        {
            string sql = @" select top 65530 op.Item,i.RefCode,i.Desc1,op.OpRef,op.Qty,op.CreateDate,op.CreateUserNm,op.LastModifyDate,op.LastModifyUserNm from SCM_OpRefBalance  as op  with(nolock) 
 inner join MD_Item  as i with(nolock) on op.Item=i.Code  where 1=1 ";
            IList<object> param = new List<object>();

            if (!string.IsNullOrWhiteSpace(searchModel.Item))
            {
                sql += " and  op.Item=?";
                param.Add(searchModel.Item);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.OpReference))
            {
                sql += " and  op.OpReference=?";
                param.Add(searchModel.OpReference);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.CreateUserName))
            {
                sql += " and  op.CreateUserName=?";
                param.Add(searchModel.CreateUserName);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.LastModifyUserName))
            {
                sql += " and  op.LastModifyUserName=?";
                param.Add(searchModel.LastModifyUserName);
            }
            if (searchModel.CreateStartDate != null & searchModel.CreateEndDate != null)
            {
                sql += " and  op.CreateDate between ? and  ?";
                param.Add(searchModel.CreateStartDate);
                param.Add(searchModel.CreateEndDate);
            }
            else if (searchModel.CreateStartDate != null & searchModel.CreateEndDate == null)
            {
                sql += " and  op.CreateDate >=? ";
                param.Add(searchModel.CreateStartDate);
            }
            else if (searchModel.CreateStartDate == null & searchModel.CreateEndDate != null)
            {
                sql += " and  op.CreateDate <=?";
                param.Add(searchModel.CreateEndDate);
            }

            if (searchModel.ModifyStartDate != null & searchModel.ModifyEndDate != null)
            {
                sql += " and  op.LastModifyDate between ? and  ?";
                param.Add(searchModel.ModifyStartDate);
                param.Add(searchModel.ModifyEndDate);
            }
            else if (searchModel.ModifyStartDate != null & searchModel.ModifyEndDate == null)
            {
                sql += " and  op.LastModifyDate >=? ";
                param.Add(searchModel.ModifyStartDate);
            }
            else if (searchModel.ModifyStartDate == null & searchModel.ModifyEndDate != null)
            {
                sql += " and  op.LastModifyDate <=?";
                param.Add(searchModel.ModifyEndDate);
            }

            sql += " order by op.CreateDate desc";
            IList<object[]> searchList = this.genericMgr.FindAllWithNativeSql<object[]>(sql, param.ToArray());
            //op.Item,i.RefCode,i.Desc1,op.OpRef,op.Qty,op.CreateDate,op.CreateUserNm,op.LastModifyDate,op.LastModifyUserNm 
            IList<OpReferenceBalance> exportList = (from tak in searchList
                                                    select new OpReferenceBalance
                                           {
                                               Item = (string)tak[0],
                                               ReferenceItemCode = (string)tak[1],
                                               ItemDescription = (string)tak[2],
                                               OpReference = (string)tak[3],
                                               Qty = (decimal)tak[4],
                                               CreateDate = (DateTime)tak[5],
                                               CreateUserName = (string)tak[6],
                                               LastModifyDate = (DateTime)tak[7],
                                               LastModifyUserName = (string)tak[8],
                                           }).ToList();
            ExportToXLS<OpReferenceBalance>("ExporOpReferenceBalancetList", "xls", exportList);
        }


        #endregion

        #region 调整

        public ActionResult Adjust()
        {
            return View();
        }


        [SconitAuthorize(Permissions = "Url_OpReferenceBalance_Adjust")]
        public JsonResult _Adjust(string prodLine, string traceCode, string item, string opReference, decimal? qty)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(prodLine) || string.IsNullOrWhiteSpace(traceCode) || string.IsNullOrWhiteSpace(item) || string.IsNullOrWhiteSpace(opReference) || qty == null || (qty.Value<=0))
                {
                    throw new BusinessException("路线代码,Van号，物料代码，工位，库存数不能为空。");
                }

                 base.genericMgr.FindAllWithNativeSql<object[]>("exec USP_Busi_UpdateOpRefBalance ?,?,?,?,?,?,?",
                    new object[] { prodLine,item,opReference, traceCode,qty.Value, CurrentUser.Id, CurrentUser.FullName },
                    new IType[] { NHibernateUtil.String, NHibernateUtil.String, NHibernateUtil.String, NHibernateUtil.String, NHibernateUtil.Decimal, NHibernateUtil.Int32, NHibernateUtil.String });
                 SaveSuccessMessage("调整成功。");
                 return Json(new { });
            }
            catch (BusinessException be)
            {
                SaveBusinessExceptionMessage(be);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.InnerException != null)
                    {
                        SaveErrorMessage(ex.InnerException.InnerException.Message);
                    }
                    else
                    {
                        SaveErrorMessage(ex.InnerException.Message);
                    }
                }
                else
                {
                    SaveErrorMessage(ex.Message);
                }
            }
            return Json(null);
        }
        #endregion

        #region 盘点导入

        [SconitAuthorize(Permissions = "Url_OpReferenceBalance_Stock")]
        public ActionResult Stock()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OpReferenceBalance_Stock")]
        public ActionResult ImportStockXls(IEnumerable<HttpPostedFileBase> attachments)
        {
            try
            {
                foreach (var file in attachments)
                {
                    stockTakeMgr.ImportOpReferenceBalanceStockXls(file.InputStream);
                    SaveSuccessMessage("全部导入成功！");
                }
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                SaveSuccessMessage("其它行导入成功！");
            }
            catch (Exception ex)
            {
                SaveErrorMessage("导入失败。 - " + ex.Message);
            }

            return Content(string.Empty);
        }

        #endregion


        #region private
        private SearchStatementModel PrepareSearchStatement(GridCommand command, OpReferenceBalanceSearchModel searchModel)
        {
            string whereStatement = " where 1=1 ";

            IList<object> param = new List<object>();
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("OpReference", searchModel.OpReference, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("CreateUserName", searchModel.CreateUserName, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("LastModifyUserName", searchModel.LastModifyUserName, "l", ref whereStatement, param);

            if (searchModel.CreateStartDate != null & searchModel.CreateEndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.CreateStartDate, searchModel.CreateEndDate, "l", ref whereStatement, param);
            }
            else if (searchModel.CreateStartDate != null & searchModel.CreateEndDate == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.CreateStartDate, "l", ref whereStatement, param);
            }
            else if (searchModel.CreateStartDate == null & searchModel.CreateEndDate != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.CreateEndDate, "l", ref whereStatement, param);
            }

            if (searchModel.ModifyStartDate != null & searchModel.ModifyEndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("LastModifyDate", searchModel.ModifyStartDate, searchModel.ModifyEndDate, "l", ref whereStatement, param);
            }
            else if (searchModel.ModifyStartDate != null & searchModel.ModifyEndDate == null)
            {
                HqlStatementHelper.AddGeStatement("LastModifyDate", searchModel.ModifyStartDate, "l", ref whereStatement, param);
            }
            else if (searchModel.ModifyStartDate == null & searchModel.ModifyEndDate != null)
            {
                HqlStatementHelper.AddLeStatement("LastModifyDate", searchModel.ModifyStartDate, "l", ref whereStatement, param);
            }

            string sortingStatement = " order by l.CreateDate desc";


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
