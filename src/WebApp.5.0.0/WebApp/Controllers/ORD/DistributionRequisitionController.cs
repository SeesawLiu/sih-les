/// <summary>
/// Summary description 
/// </summary>
namespace com.Sconit.Web.Controllers.ORD
{
    #region reference
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Security;
    using com.Sconit.Entity.ORD;
    using com.Sconit.Entity.SYS;
    using com.Sconit.Service;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Models.SearchModels.ORD;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;
    using com.Sconit.Service.Impl;
    using com.Sconit.Entity.Exception;
    using com.Sconit.Utility.Report;
    using com.Sconit.PrintModel.ORD;
    using AutoMapper;
    using com.Sconit.Utility;
    using System.Text;
    using System;
    using System.ComponentModel;
    using com.Sconit.Entity.LOG;
    using System.Data.SqlClient;
    using System.Data;
    using com.Sconit.Persistence;

    #endregion


    public class DistributionRequisitionController : WebAppBaseController
    {

        #region Properties


        public IOrderMgr orderMgr { get; set; }

        public ISqlDao sqlDao { get; set; }

        #endregion



        private static string selectCountStatement = "select count(*) from DistributionRequisition as d";


        private static string selectStatement = "select d from DistributionRequisition as d";

        #region public actions

        #region 销售手工拉料
        [SconitAuthorize(Permissions = "Url_DistributionRequisition_NewIndex")]
        public ActionResult NewIndex()
        {
            return View();
        }


        
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionRequisition_NewIndex")]
        public ActionResult NewList(GridCommand command, ReceiptMasterSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = 50;
            return View();
        }


        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxNewList(GridCommand command, ReceiptMasterSearchModel searchModel)
        {
            SqlParameter[] parameters = new SqlParameter[12];
            parameters[0] = new SqlParameter("@OrderNo", System.Data.SqlDbType.VarChar,50);
            parameters[0].Value = searchModel.OrderNo;

            parameters[1] = new SqlParameter("@LocFrom", System.Data.SqlDbType.VarChar, 50);
            parameters[1].Value = searchModel.LocFrom;

            parameters[2] = new SqlParameter("@Item", System.Data.SqlDbType.VarChar, 50);
            parameters[2].Value = searchModel.Item;

            parameters[3] = new SqlParameter("@ExtNo", System.Data.SqlDbType.VarChar, 50);
            parameters[3].Value = searchModel.ExtNo;

            parameters[4] = new SqlParameter("@ExtSeq", System.Data.SqlDbType.VarChar, 50);
            parameters[4].Value = searchModel.ExtSeq;

            parameters[5] = new SqlParameter("@StartDate", System.Data.SqlDbType.DateTime);
            parameters[5].Value = searchModel.StartDate;

            parameters[6] = new SqlParameter("@EndDate", System.Data.SqlDbType.DateTime);
            parameters[6].Value = searchModel.EndDate;

            parameters[7] = new SqlParameter("@SortCloumn", System.Data.SqlDbType.VarChar, 50);
            parameters[7].Value = command.SortDescriptors.Count > 0 ? command.SortDescriptors[0].Member : string.Empty;

            parameters[8] = new SqlParameter("@SortRule", System.Data.SqlDbType.VarChar, 50);
            parameters[8].Value = command.SortDescriptors.Count > 0 ? command.SortDescriptors[0].SortDirection == ListSortDirection.Descending ? "desc" : "asc" : string.Empty;

            parameters[9] = new SqlParameter("@PageSize", SqlDbType.Int);
            parameters[9].Value = command.PageSize;

            parameters[10] = new SqlParameter("@Page", SqlDbType.Int);
            parameters[10].Value = command.Page;

            parameters[11] = new SqlParameter("@RowCount", System.Data.SqlDbType.VarChar, 50);
            parameters[11].Direction = ParameterDirection.Output;

            IList<OrderDetail> returList = new List<OrderDetail>();
            try
            {
                DataSet dataSet = sqlDao.GetDatasetByStoredProcedure("USP_Search_CreateRequisitionDetail", parameters, false);

                //det.Id,det.OrderNo,det.ExtNo,det.ExtSeq,det.Item,det.ItemDesc,det.RefItemCode,det.Uom,det.UC,det.LocFrom,
                //        //det.LocTo,det.OrderQty,det.RecQty,det.CreateDate,fm.Code,fm.Desc1,fm.PartyFrom,fm.PartyTo,fd.Container,fd.ContainerDesc
                if (dataSet.Tables[0] != null && dataSet.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        //row.ItemArray[0].ToString()
                        OrderDetail det = new OrderDetail();
                        det.Id = Convert.ToInt32(row.ItemArray[0].ToString());
                        det.OrderNo = row.ItemArray[1].ToString();
                        det.ExternalOrderNo = row.ItemArray[2].ToString();
                        det.ExternalSequence = row.ItemArray[3].ToString();
                        det.Item = row.ItemArray[4].ToString();
                        det.ItemDescription = row.ItemArray[5].ToString();
                        det.ReferenceItemCode = row.ItemArray[6].ToString();
                        det.Uom = row.ItemArray[7].ToString();
                        det.BaseUom = row.ItemArray[7].ToString();
                        det.UnitCount =  Convert.ToDecimal(row.ItemArray[8]);
                        det.MinUnitCount = Convert.ToDecimal(row.ItemArray[8]);
                        det.LocationTo = row.ItemArray[9].ToString();
                        det.OrderedQty = Convert.ToDecimal(row.ItemArray[11]);
                        det.RejectedQty = Convert.ToDecimal(row.ItemArray[12]);
                        det.CreateDate = Convert.ToDateTime(row.ItemArray[13]);
                        det.Flow = row.ItemArray[14].ToString();
                        det.FlowDescription = row.ItemArray[15].ToString();
                        det.MastPartyFrom = row.ItemArray[16].ToString();
                        det.PartyTo = row.ItemArray[17].ToString();
                        det.Container = row.ItemArray[18].ToString();
                        det.ContainerDescription = row.ItemArray[19].ToString();
                        returList.Add(det);
                    }
                }
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
            GridModel<OrderDetail> gridModel = new GridModel<OrderDetail>();
            gridModel.Total = string.IsNullOrWhiteSpace(parameters[11].Value.ToString()) ? 0 : Convert.ToInt32(parameters[11].Value);
            gridModel.Data = returList;
            return PartialView(gridModel);
        }

        //[GridAction(EnableCustomBinding = true)]
        //[SconitAuthorize(Permissions = "Url_DistributionRequisition_NewIndex")]
        //public ActionResult _AjaxNewList(GridCommand command, ReceiptMasterSearchModel searchModel)
        //{
        //    string sql = this.StringBuilderPrepareSearchStatement(command, searchModel).ToString();
        //    IList<object[]> searchList = base.genericMgr.FindAllWithNativeSql<object[]>(sql);
        //    IList<OrderDetail> returnList = new List<OrderDetail>();
        //    if (searchList != null && searchList.Count > 0)
        //    {
        //        #region
        //        //det.Id,det.OrderNo,det.ExtNo,det.ExtSeq,det.Item,det.ItemDesc,det.RefItemCode,det.Uom,det.UC,det.LocFrom,
        //        //det.LocTo,det.OrderQty,det.RecQty,det.CreateDate,fm.Code,fm.Desc1,fm.PartyFrom,fm.PartyTo,fd.Container,fd.ContainerDesc
        //        returnList = (from tak in searchList
        //                      select new OrderDetail
        //                          {
        //                              Id = (int)tak[0],
        //                              OrderNo = (string)tak[1],
        //                              ExternalOrderNo = (string)tak[2],
        //                              ExternalSequence = (string)tak[3],
        //                              Item = (string)tak[4],
        //                              ItemDescription = (string)tak[5],
        //                              ReferenceItemCode = (string)tak[6],
        //                              Uom = (string)tak[7],
        //                              BaseUom = (string)tak[7],
        //                              UnitCount = (decimal)tak[8],
        //                              MinUnitCount = (decimal)tak[8],//上线包装
        //                              LocationTo = (string)tak[9],
        //                              //LocationTo = (string)tak[10],
        //                              OrderedQty = (decimal)tak[11],
        //                              ReceivedQty = (decimal)tak[12],
        //                              CreateDate = (DateTime)tak[13],
        //                              Flow = (string)tak[14],
        //                              FlowDescription = (string)tak[15],
        //                              MastPartyFrom = (string)tak[16],
        //                              MastPartyTo = (string)tak[17],
        //                              Container = (string)tak[18],
        //                              ContainerDescription = (string)tak[19],
        //                          }).ToList();
        //        #endregion
        //        returnList = returnList.OrderBy(r => r.Id).ToList();

        //        foreach (var det in returnList)
        //        {
        //            if (det.Id != 0)
        //            {
        //                var tempDets = returnList.Where(rr => rr.Id == det.Id && rr.Flow!=det.Flow).ToList();
        //                if (tempDets.Count > 0)
        //                {
        //                    if (!(searchModel.IsCreate != null && searchModel.IsCreate.Value))
        //                    {
        //                        SaveErrorMessage(string.Format("销售单号{0}sap销售单号{1}物料号{2}匹配到多条路线{3}", det.OrderNo, det.ExternalOrderNo, det.Item, string.Join(",", returnList.Where(rr => rr.Id == det.Id).Select(rr => rr.Flow).ToArray())));
        //                    }
        //                    var t=returnList.Where(rr => rr.Id == det.Id).ToList();
        //                    foreach (var tempdet in t)
        //                    {
        //                        tempdet.Id = 0;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    GridModel<OrderDetail> gridModelOrderDet = new GridModel<OrderDetail>();
        //    gridModelOrderDet.Total = returnList.Where(rl=>rl.Id!=0).ToList().Count;
        //    gridModelOrderDet.Data = returnList.Where(rl => rl.Id != 0).ToList().Skip((command.Page - 1) * command.PageSize).Take(command.PageSize);
        //    TempData["DetailList"] = gridModelOrderDet.Data.ToList();
        //    return PartialView(gridModelOrderDet);
        //}

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionRequisition_NewIndex")]
        public JsonResult CreateOrder(string idStr, string qtyStr, DateTime? WindowTime, com.Sconit.CodeMaster.OrderPriority Priority)
        {
            try
            {
                if (WindowTime==null)
                {
                    throw new BusinessException("窗口时间不能为空。");
                }
                IList<OrderDetail> orderDetailList =TempData["DetailList"] as IList<OrderDetail>;

                if (!string.IsNullOrEmpty(idStr))
                {
                  string orderNos=  orderMgr.CreateDistritutionRequsiton(idStr, WindowTime.Value, Priority, orderDetailList);
                  SaveSuccessMessage("拉料成功，生成拉料单号："+orderNos);
                  return Json(new { });
                }
                else
                {
                    throw new BusinessException("拣货明细不能为空。");
                }

            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }
            return Json(null);
        }
        #endregion

        #region 手工拉料日志
        [SconitAuthorize(Permissions = "Url_DistributionReceipt_View")]
        public ActionResult LogIndex()
        {
            return View();
        }


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionReceipt_View")]
        public ActionResult LogList(GridCommand command, ReceiptMasterSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionReceipt_View")]
        public ActionResult _AjaxLogList(GridCommand command, ReceiptMasterSearchModel searchModel)
        {
           SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
           return PartialView(GetAjaxPageData<DistributionRequisition>(searchStatementModel, command));
        }

        #endregion

        #endregion


        #region Private
        private StringBuilder StringBuilderPrepareSearchStatement(GridCommand command, ReceiptMasterSearchModel searchModel)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@" select det.Id,det.OrderNo,det.ExtNo,det.ExtSeq,det.Item,det.ItemDesc,det.RefItemCode,det.Uom,det.UC,det.LocFrom,det.LocTo,det.OrderQty,det.RecQty,det.CreateDate,fm.Code,fm.Desc1,fm.PartyFrom,fm.PartyTo,fd.Container,fd.ContainerDesc
from ORD_OrderDet_3 as det
inner join ORD_OrderMstr_3 as om on det.OrderNo=om.OrderNo
inner join SCM_FlowDet as fd on det.Item=fd.Item
inner join  SCM_FlowMstr as fm on fm.Code=fd.Flow and fm.LocTo=det.LocFrom
inner join SCM_FlowStrategy as fs on fs.Flow=fm.Code
left join LOG_DistributionRequisition as ld on ld.OrderDetId=det.Id
left join LOG_DistributionRequisition as ld2 on ld2.ExtNo=det.ExtNo and ld2.ExtSeq=det.ExtSeq
where fm.Type=1 and fs.Strategy in(3,4) and ld.Id is null and ld2.Id is null and om.Status in(2,1) and det.OrderQty>det.RecQty ");

            if (!string.IsNullOrEmpty(searchModel.OrderNo))
            {
                sb.Append(string.Format(" and det.OrderNo = '{0}'", searchModel.OrderNo));
            }
            if (!string.IsNullOrEmpty(searchModel.LocFrom))
            {
                sb.Append(string.Format(" and det.LocFrom = '{0}'", searchModel.LocFrom));
            }
            if (!string.IsNullOrEmpty(searchModel.Item))
            {
                sb.Append(string.Format(" and det.Item='{0}'", searchModel.Item));
            }
            if (!string.IsNullOrEmpty(searchModel.ExtNo))
            {
                sb.Append(string.Format(" and det.ExtNo='{0}'", searchModel.ExtNo));
            }
            if (!string.IsNullOrEmpty(searchModel.ExtSeq))
            {
                sb.Append(string.Format(" and det.ExtSeq='{0}'", searchModel.ExtSeq));
            }
            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                sb.Append(string.Format(" and  det.CreateDate between '{0}' and '{1}'", searchModel.StartDate, searchModel.EndDate));
            }
            else if (searchModel.StartDate != null & searchModel.EndDate == null)
            {
                sb.Append(string.Format(" and det.CreateDate >= '{0}'", searchModel.StartDate));
            }
            else if (searchModel.StartDate == null & searchModel.EndDate != null)
            {
                sb.Append(string.Format(" and det.CreateDate <= '{0}'", searchModel.EndDate));
            }
           
           
            if (command.SortDescriptors.Count == 0)
            {
                sb.Append(" order by CreateDate asc");
            }
            else
            {
                sb.Append(HqlStatementHelper.GetSortingStatement(command.SortDescriptors));
            }
            return sb;
        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, ReceiptMasterSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            HqlStatementHelper.AddEqStatement("OrderNo", searchModel.OrderNo, "d", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("ExternalOrderNo", searchModel.ExtNo, "d", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("LocationTo", searchModel.LocFrom, "d", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "d", ref whereStatement, param);
            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.StartDate, searchModel.EndDate, "d", ref whereStatement, param);
            }
            else if (searchModel.StartDate != null & searchModel.EndDate == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.StartDate, "d", ref whereStatement, param);
            }
            else if (searchModel.StartDate == null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.EndDate, "d", ref whereStatement, param);
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
