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
                    //User createUser = base.genericMgr.FindById<User>(opReferenceBalance.CreateUserId);
                    //opReferenceBalance.CreateUserName = createUser.FirstName + createUser.LastName;
                }
            }
            return PartialView(gridModel);
        }

        //public void ExportXLS(OpReferenceBalanceSearchModel searchModel)
        //{
        //    string sql = @"select lt.TransType,lt.EffDate,lt.OrderNo,lt.IpNo,lt.RecNo,lt.PartyFrom,lt.PartyTo,lt.LocFrom,lt.LocTo,lt.Item,lt.IOType,lt.HuId,lt.LotNo,lt.Qty,(a.FirstName+a.LastName) as createUserName from VIEW_LocTrans as lt inner join ACC_User as a on lt.CreateUser=a.Id   where 1=1 ";
        //    IList<object> param = new List<object>();
        //    if (!string.IsNullOrEmpty(searchModel.CreateUserName))
        //    {
        //        sql += "and  exists (select 1 from User as u where u.Id=lt.CreateUser and (u.FirstName+u.LastName) like ? )";
        //        param.Add(searchModel.CreateUserName + "%");
        //    }
        //    if (!string.IsNullOrWhiteSpace(searchModel.PartyFrom))
        //    {
        //        sql += " and  lt.PartyFrom=?";
        //        param.Add(searchModel.PartyFrom);
        //    }
        //    if (!string.IsNullOrWhiteSpace(searchModel.PartyTo))
        //    {
        //        sql += " and  lt.PartyTo = ?";
        //        param.Add(searchModel.PartyTo);
        //    }
        //    if (!string.IsNullOrWhiteSpace(searchModel.LocationFrom))
        //    {
        //        sql += " and  lt.LocFrom = ?";
        //        param.Add(searchModel.LocationFrom);
        //    } if (!string.IsNullOrWhiteSpace(searchModel.LocationTo))
        //    {
        //        sql += " and  lt.LocTo = ?";
        //        param.Add(searchModel.LocationTo);
        //    }
        //    if (!string.IsNullOrWhiteSpace(searchModel.Item))
        //    {
        //        sql += " and  lt.Item = ?";
        //        param.Add(searchModel.Item);
        //    }
        //    if (!string.IsNullOrWhiteSpace(searchModel.OrderNo))
        //    {
        //        sql += " and  lt.OrderNo = ?";
        //        param.Add(searchModel.OrderNo);
        //    }
        //    if (!string.IsNullOrWhiteSpace(searchModel.TransactionType))
        //    {
        //        sql += " and  lt.TransType = ?";
        //        param.Add(searchModel.TransactionType);
        //    }
        //    if (searchModel.StartDate != null & searchModel.EndDate != null)
        //    {
        //        sql += " and  lt.CreateDate between ? and  ?";
        //        param.Add(searchModel.StartDate);
        //        param.Add(searchModel.EndDate);
        //    }
        //    else if (searchModel.StartDate != null & searchModel.EndDate == null)
        //    {
        //        sql += " and  lt.CreateDate >=? ";
        //        param.Add(searchModel.StartDate);
        //    }
        //    else if (searchModel.StartDate == null & searchModel.EndDate != null)
        //    {
        //        sql += " and  lt.CreateDate <=?";
        //        param.Add(searchModel.EndDate);
        //    }

        //    sql += " order by lt.Id desc";
        //    IList<object[]> searchList = this.genericMgr.FindAllWithNativeSql<object[]>(sql, param.ToArray());
        //    //lt.TransType,lt.EffDate,lt.OrderNo,lt.IpNo,lt.RecNo,lt.PartyFrom,lt.PartyTo,lt.LocFrom,lt.LocTo,
        //    //lt.Item,lt.IOType,lt.HuId,lt.LotNo,lt.Qty,(a.FirstName+a.LastName) as createUserName
        //    IList<OpReferenceBalance> exportList = (from tak in searchList
        //                                            select new OpReferenceBalance
        //                                   {
        //                                       TransactionTypeDescription = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.TransactionType, Convert.ToInt32(((object)tak[0]).ToString())),
        //                                       EffectiveDate = (DateTime)tak[1],
        //                                       OrderNo = (string)tak[2],
        //                                       IpNo = (string)tak[3],
        //                                       ReceiptNo = (string)tak[4],
        //                                       PartyFrom = (string)tak[5],
        //                                       PartyTo = (string)tak[6],
        //                                       LocationFrom = (string)tak[7],
        //                                       LocationTo = (string)tak[8],
        //                                       Item = (string)tak[9],
        //                                       IOTypeDescription = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.TransactionIOType, Convert.ToInt32(((object)tak[10]).ToString())),
        //                                       HuId = (string)tak[11],
        //                                       LotNo = (string)tak[12],
        //                                       Qty = (decimal)tak[13],
        //                                       CreateUserName = (string)tak[14],
        //                                   }).ToList();
        //    ExportToXLS<OpReferenceBalance>("ExportList", "xls", exportList);
        //}


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
