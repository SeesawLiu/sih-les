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

    #endregion


    public class DistributionRequisitionController : WebAppBaseController
    {

        #region Properties


        public IOrderMgr orderMgr { get; set; }

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
        [SconitAuthorize(Permissions = "Url_DistributionRequisition_NewIndex")]
        public ActionResult _AjaxNewList(GridCommand command, ReceiptMasterSearchModel searchModel)
        {
            string sql = this.StringBuilderPrepareSearchStatement(command, searchModel).ToString();
            IList<object[]> searchList = base.genericMgr.FindAllWithNativeSql<object[]>(sql);
            IList<OrderDetail> returnList = new List<OrderDetail>();
            if (searchList != null && searchList.Count > 0)
            {
                #region
                //det.Id,det.OrderNo,det.ExtNo,det.ExtSeq,det.Item,det.ItemDesc,det.RefItemCode,det.Uom,det.UC,det.LocFrom,
                //det.LocTo,det.OrderQty,det.RecQty,det.CreateDate,fm.Code,fm.Desc1,fm.PartyFrom,fm.PartyTo,fd.Container,fd.ContainerDesc
                returnList = (from tak in searchList
                              select new OrderDetail
                                  {
                                      Id = (int)tak[0],
                                      OrderNo = (string)tak[1],
                                      ExternalOrderNo = (string)tak[2],
                                      ExternalSequence = (string)tak[3],
                                      Item = (string)tak[4],
                                      ItemDescription = (string)tak[5],
                                      ReferenceItemCode = (string)tak[6],
                                      Uom = (string)tak[7],
                                      BaseUom = (string)tak[7],
                                      UnitCount = (decimal)tak[8],
                                      MinUnitCount = (decimal)tak[8],//上线包装
                                      LocationTo = (string)tak[9],
                                      //LocationTo = (string)tak[10],
                                      OrderedQty = (decimal)tak[11],
                                      ReceivedQty = (decimal)tak[12],
                                      CreateDate = (DateTime)tak[13],
                                      Flow = (string)tak[14],
                                      FlowDescription = (string)tak[15],
                                      MastPartyFrom = (string)tak[16],
                                      MastPartyTo = (string)tak[17],
                                      Container = (string)tak[18],
                                      ContainerDescription = (string)tak[19],
                                  }).ToList();
                #endregion
                returnList = returnList.OrderBy(r => r.Id).ToList();

                foreach (var det in returnList)
                {
                    if (det.Id != 0)
                    {
                        var tempDets = returnList.Where(rr => rr.Id == det.Id && rr.Flow!=det.Flow).ToList();
                        if (tempDets.Count > 0)
                        {
                            if (!(searchModel.IsCreate != null && searchModel.IsCreate.Value))
                            {
                                SaveErrorMessage(string.Format("销售单号{0}sap销售单号{1}物料号{2}匹配到多条路线{3}", det.OrderNo, det.ExternalOrderNo, det.Item, string.Join(",", returnList.Where(rr => rr.Id == det.Id).Select(rr => rr.Flow).ToArray())));
                            }
                            var t=returnList.Where(rr => rr.Id == det.Id).ToList();
                            foreach (var tempdet in t)
                            {
                                tempdet.Id = 0;
                            }
                        }
                    }
                }
            }
            GridModel<OrderDetail> gridModelOrderDet = new GridModel<OrderDetail>();
            gridModelOrderDet.Total = returnList.Where(rl=>rl.Id!=0).ToList().Count;
            gridModelOrderDet.Data = returnList.Where(rl => rl.Id != 0).ToList().Skip((command.Page - 1) * command.PageSize).Take(command.PageSize);
            TempData["DetailList"] = gridModelOrderDet.Data.ToList();
            return PartialView(gridModelOrderDet);
        }

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
