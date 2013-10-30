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
using com.Sconit.Entity.ORD;
using System.Collections;

namespace com.Sconit.Web.Controllers.INV
{
    public class LocationTransactionController : WebAppBaseController
    {
        private static string selectCountStatement = "select count(*) from LocationTransaction as l";

        private static string selectStatement = "from  LocationTransaction as l";

        #region public

        public ActionResult Index()
        {
            return View();
        }



        /// <summary>
        /// List action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel"> LocationTransaction Search model</param>
        /// <returns>return the result view</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Menu_Inventory_InventoryTrans")]
        public ActionResult List(GridCommand command, LocationTransactionSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            if (this.CheckSearchModelIsNull(searchCacheModel.SearchObject))
            {
                TempData["_AjaxMessage"] = "";
            }
            else
            {
                SaveWarningMessage(Resources.ErrorMessage.Errors_NoConditions);
            }
            ViewBag.PageSize = this.ProcessPageSize(command.PageSize);

            return View();
        }


        /// <summary>
        ///  AjaxList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel"> LocationTransaction Search Model</param>
        /// <returns>return the result action</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Menu_Inventory_InventoryTrans")]
        public ActionResult _AjaxList(GridCommand command, LocationTransactionSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<LocationTransaction>()));
            }
            string sql = PrepareSqlSearchStatement(searchModel);
            int total = this.genericMgr.FindAllWithNativeSql<int>("select count(*) from (" + sql + ") as r1").First();
            string sortingStatement = string.Empty;

            #region
            if (command.SortDescriptors.Count != 0)
            {
                if (command.SortDescriptors[0].Member == "IOTypeDescription")
                {
                    command.SortDescriptors[0].Member = "IOType";
                }
                else if (command.SortDescriptors[0].Member == "CreateUserName")
                {
                    command.SortDescriptors[0].Member = "CreateUser";
                }
                sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            }
            #endregion

            if (string.IsNullOrWhiteSpace(sortingStatement))
            {
                sortingStatement = " order by r1.Id asc";
            }
            sql = string.Format("select * from (select RowId=ROW_NUMBER()OVER({0}),r1.* from ({1}) as r1 ) as rt where rt.RowId between {2} and {3}", sortingStatement, sql, (command.Page - 1) * command.PageSize, command.PageSize*command.Page);
            IList<object[]> searchResult = this.genericMgr.FindAllWithNativeSql<object[]>(sql);
            IList<LocationTransaction> locationTransactionList = new List<LocationTransaction>();
            if (searchResult != null && searchResult.Count > 0)
            {
                #region
                //lt.TransType,lt.EffDate,lt.OrderNo,lt.IpNo,lt.RecNo,lt.PartyFrom,lt.PartyTo,lt.Item,lt.IOType,lt.HuId,lt.LotNo,lt.Qty,au.CreateUserNm,
                //lt.CreateDate,om.ExtOrderNo,bp.Party,ba.Party 
                locationTransactionList = (from tak in searchResult
                                           select new LocationTransaction
                                           {
                                               TransactionTypeDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.TransactionType, int.Parse((tak[1]).ToString())),
                                               EffectiveDate = (DateTime)tak[2],
                                               OrderNo = (string)tak[3],
                                               IpNo = (string)tak[4],
                                               ReceiptNo = (string)tak[5],
                                               PartyFrom = (string)tak[6],
                                               PartyTo = (string)tak[7],
                                               Item = (string)tak[8],
                                               IOTypeDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.TransactionIOType, int.Parse((tak[9]).ToString())),
                                               HuId = (string)tak[10],
                                               LotNo = (string)tak[11],
                                               Qty = (decimal)tak[12],
                                               CreateUserName = (string)tak[13],
                                               CreateDate = (DateTime)tak[14],
                                               SapOrderNo = (string)tak[15],
                                               Supplier = tak[16] != null ? (string)tak[16] : (tak[17] != null ? (string)tak[17] : string.Empty),
                                           }).ToList();
                #endregion
            }
            GridModel<LocationTransaction> gridModel = new GridModel<LocationTransaction>();
            gridModel.Total = total;
            gridModel.Data = locationTransactionList;
            return PartialView(gridModel);
        }

        public void ExportXLS(LocationTransactionSearchModel searchModel)
        {
            string sql = @"select lt.TransType,lt.EffDate,lt.OrderNo,lt.IpNo,lt.RecNo,lt.PartyFrom,lt.PartyTo,lt.LocFrom,lt.LocTo,lt.Item,lt.IOType,lt.HuId,lt.LotNo,lt.Qty,(a.FirstName+a.LastName) as createUserName from VIEW_LocTrans as lt inner join ACC_User as a on lt.CreateUser=a.Id   where 1=1 ";
            IList<object> param = new List<object>();
            if (!string.IsNullOrEmpty(searchModel.CreateUserName))
            {
                sql += "and  (a.FirstName+a.LastName) like ? ";
                param.Add(searchModel.CreateUserName+"%");
            }
            if (!string.IsNullOrWhiteSpace(searchModel.PartyFrom))
            {
                sql += " and  lt.PartyFrom=?";
                param.Add(searchModel.PartyFrom);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.PartyTo))
            {
                sql += " and  lt.PartyTo = ?";
                param.Add(searchModel.PartyTo);
            } 
            if (!string.IsNullOrWhiteSpace(searchModel.LocationFrom))
            {
                sql += " and  lt.LocFrom = ?";
                param.Add(searchModel.LocationFrom);
            } if (!string.IsNullOrWhiteSpace(searchModel.LocationTo))
            {
                sql += " and  lt.LocTo = ?";
                param.Add(searchModel.LocationTo);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Item))
            {
                sql += " and  lt.Item = ?";
                param.Add(searchModel.Item);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.OrderNo))
            {
                sql += " and  lt.OrderNo = ?";
                param.Add(searchModel.OrderNo);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.TransactionType))
            {
                sql += " and  lt.TransType = ?";
                param.Add(searchModel.TransactionType);
            }
            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                sql += " and  lt.CreateDate between ? and  ?";
                param.Add(searchModel.StartDate);
                param.Add(searchModel.EndDate);
            }
            else if (searchModel.StartDate != null & searchModel.EndDate == null)
            {
                sql += " and  lt.CreateDate >=? ";
                param.Add(searchModel.StartDate);
            }
            else if (searchModel.StartDate == null & searchModel.EndDate != null)
            {
                sql += " and  lt.CreateDate <=?";
                param.Add(searchModel.EndDate);
            }

             sql+= " order by lt.Id desc";
            IList<object[]> searchList = this.genericMgr.FindAllWithNativeSql<object[]>(sql, param.ToArray());
            //lt.TransType,lt.EffDate,lt.OrderNo,lt.IpNo,lt.RecNo,lt.PartyFrom,lt.PartyTo,lt.LocFrom,lt.LocTo,
            //lt.Item,lt.IOType,lt.HuId,lt.LotNo,lt.Qty,(a.FirstName+a.LastName) as createUserName
            IList<LocationTransaction> exportList = (from tak in searchList
                                                     select new LocationTransaction
                                            {
                                                TransactionTypeDescription = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.TransactionType, Convert.ToInt32(((object)tak[0]).ToString())),
                                                EffectiveDate = (DateTime)tak[1],
                                                OrderNo = (string)tak[2],
                                                IpNo = (string)tak[3],
                                                ReceiptNo = (string)tak[4],
                                                PartyFrom = (string)tak[5],
                                                PartyTo = (string)tak[6],
                                                LocationFrom = (string)tak[7],
                                                LocationTo = (string)tak[8],
                                                Item = (string)tak[9],
                                                IOTypeDescription = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.TransactionIOType, Convert.ToInt32(((object)tak[10]).ToString())),
                                                HuId = (string)tak[11],
                                                LotNo = (string)tak[12],
                                                Qty = (decimal)tak[13],
                                                CreateUserName = (string)tak[14],
                                            }).ToList();
            ExportToXLS<LocationTransaction>("ExportList", "xls", exportList);
        }

        #endregion

        #region private
        private SearchStatementModel PrepareSearchStatement(GridCommand command, LocationTransactionSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();
            if (!string.IsNullOrEmpty(searchModel.CreateUserName))
            {
                whereStatement = "where  exists (select 1 from User as u where u.Id=l.CreateUserId and (u.FirstName+u.LastName) like '" + searchModel.CreateUserName + "%' )";
            }
            HqlStatementHelper.AddEqStatement("PartyFrom", searchModel.PartyFrom, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("PartyTo", searchModel.PartyTo, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("LocationFrom", searchModel.LocationFrom, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("LocationTo", searchModel.LocationTo, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("TransactionType", searchModel.TransactionType, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("OrderNo", searchModel.OrderNo, "l", ref whereStatement, param);

            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.StartDate, searchModel.EndDate, "l", ref whereStatement, param);
            }
            else if (searchModel.StartDate != null & searchModel.EndDate == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.StartDate, "l", ref whereStatement, param);
            }
            else if (searchModel.StartDate == null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.EndDate, "l", ref whereStatement, param);
            }

            string sortingStatement = " order by l.Id desc";

            if (command.SortDescriptors.Count != 0)
            {
                if (command.SortDescriptors[0].Member == "IOTypeDescription")
                {
                    command.SortDescriptors[0].Member = "IOType";
                }
                else if (command.SortDescriptors[0].Member == "CreateUserName")
                {
                    command.SortDescriptors[0].Member = "CreateUser";
                }
                sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            }

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }
        private string PrepareSqlSearchStatement(LocationTransactionSearchModel searchModel)
        {
            string whereStatement = @"select lt.TransType,lt.EffDate,lt.OrderNo,lt.IpNo,lt.RecNo,lt.PartyFrom,lt.PartyTo,lt.Item,lt.IOType,lt.HuId,lt.LotNo,lt.Qty,au.CreateUserNm,lt.CreateDate,om.ExtOrderNo,bp.Party,ba.Party as p2 ,lt.Id
from VIEW_LocTrans as lt with(nolock)
left join BIL_PlanBill as bp with(nolock) on lt.PlanBill=bp.Id
left join BIL_ActBill as ba with(nolock) on lt.ActBill=ba.Id
left join ORD_OrderMstr_4 as om with(nolock) on lt.OrderNo=om.OrderNo
left join ACC_User as au with(nolock) on lt.CreateUser=au.Id 
where 1=1 ";
            if (!string.IsNullOrEmpty(searchModel.PartyFrom))
            {
                whereStatement += " and lt.PartyFrom = '" + searchModel.PartyFrom + "'";
            }
            if (!string.IsNullOrEmpty(searchModel.CreateUserName))
            {
                whereStatement += " and (au.FirstName+au.LastName) = '" + searchModel.CreateUserName + "'";
            }
            if (!string.IsNullOrEmpty(searchModel.PartyTo))
            {
                whereStatement += " and lt.PartyTo = '" + searchModel.PartyTo + "'";
            }
            if (!string.IsNullOrEmpty(searchModel.LocationFrom))
            {
                whereStatement += " and lt.LocFrom = '" + searchModel.LocationFrom + "'";
            }
            if (!string.IsNullOrEmpty(searchModel.LocationTo))
            {
                whereStatement += " and lt.LocTo = '" + searchModel.LocationTo + "'";
            }
            if (!string.IsNullOrEmpty(searchModel.Item))
            {
                whereStatement += " and lt.Item = '" + searchModel.Item + "'";
            }
            if (!string.IsNullOrEmpty(searchModel.TransactionType))
            {
                whereStatement += " and lt.TransType =" + searchModel.TransactionType;
            }
            if (!string.IsNullOrEmpty(searchModel.OrderNo))
            {
                whereStatement += " and lt.OrderNo = '" + searchModel.OrderNo + "'";
            }

            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                whereStatement += string.Format(" and lt.CreateDate between '{0}' and '{1}' ", searchModel.StartDate, searchModel.EndDate);

            }
            else if (searchModel.StartDate != null & searchModel.EndDate == null)
            {
                whereStatement += " and lt.CreateDate >='" + searchModel.StartDate + "'";
            }
            else if (searchModel.StartDate == null & searchModel.EndDate != null)
            {
                whereStatement += " and lt.CreateDate <= '" + searchModel.EndDate + "'";
            }

           
            return whereStatement;
        }
        #endregion

    }
}
