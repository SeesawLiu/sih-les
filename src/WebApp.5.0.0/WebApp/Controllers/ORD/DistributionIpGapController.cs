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
    using com.Sconit.Entity.Exception;
    using com.Sconit.Utility.Report;
    using AutoMapper;
    using com.Sconit.PrintModel.ORD;
    using System;
    using com.Sconit.Utility;
    #endregion

    /// <summary>
    /// This controller response to control the DistributionOrderIssue.
    /// </summary>
    public class DistributionIpGapController : WebAppBaseController
    {
        #region Properties
        public IOrderMgr orderMgr { get; set; }
        public IIpMgr ipMgr { get; set; }
        public IReportGen reportGen { get; set; }
        #endregion

        private static string selectCountStatement = "select count(*) from IpMaster as i";

        private static string selectStatement = "select i from IpMaster as i";

        private static string selectIpDetailStatement = "select i from IpDetail as i where i.IpNo = ? and i.Type = ?";

        private static string selectAdjustIpDetailStatement = "select i from IpDetail as i where i.IsClose = ? and i.Type = ? and i.IpNo = ?";

        private static string selectAdjustHuIpDetailStatement = "select i from IpLocationDetail as i where i.IsClose = ?  and i.IpNo = ?";

        #region public actions

        #region view
        [SconitAuthorize(Permissions = "Url_DistributionIpGap_View")]
        public ActionResult Index()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpGap_View")]
        public ActionResult List(GridCommand command, IpMasterSearchModel searchModel)
        {
            this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpGap_View")]
        public ActionResult _AjaxList(GridCommand command, IpMasterSearchModel searchModel)
        {
            string whereStatement = "where i.OrderType in (" + (int)com.Sconit.CodeMaster.OrderType.Distribution + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + ")"
                                    + " and exists (select 1 from IpDetail as d where i.IpNo = d.IpNo and d.Type = " + (int)com.Sconit.CodeMaster.IpDetailType.Gap + ")";

            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel, whereStatement);
            return PartialView(GetAjaxPageData<IpMaster>(searchStatementModel, command));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpGap_View,Url_DistributionIpMaster_Cancel")]
        public ActionResult _IpDetailList(string ipNo)
        {
            IList<IpDetail> ipDetailList = base.genericMgr.FindAll<IpDetail>(selectIpDetailStatement, new object[] { ipNo, (int)com.Sconit.CodeMaster.IpDetailType.Gap });
            return PartialView(ipDetailList);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpGap_View")]
        public ActionResult Edit(string ipNo)
        {
            if (string.IsNullOrEmpty(ipNo))
            {
                return HttpNotFound();
            }

            IpMaster ipMaster = base.genericMgr.FindById<IpMaster>(ipNo);
            return View(ipMaster);
        }

        #endregion

        #region adjust


        [SconitAuthorize(Permissions = "Url_DistributionIpGap_Adjust")]
        public ActionResult Adjust()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpGap_Adjust")]
        public ActionResult AdjustList(GridCommand command, IpMasterSearchModel searchModel)
        {
            this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpGap_Adjust")]
        public ActionResult _AdjustAjaxList(GridCommand command, IpMasterSearchModel searchModel)
        {
            string whereStatement = "where i.OrderType in ("
                                  + (int)com.Sconit.CodeMaster.OrderType.Distribution + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + ")"
                                  + " and i.Status in (" + (int)com.Sconit.CodeMaster.IpStatus.Submit + "," + (int)com.Sconit.CodeMaster.IpStatus.InProcess + ")"
                                 + " and exists (select 1 from IpDetail as d where d.IsClose = 0  and d.IpNo = i.IpNo and d.Type = " + (int)com.Sconit.CodeMaster.IpDetailType.Gap + ") ";

            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel, whereStatement);
            return PartialView(GetAjaxPageData<IpMaster>(searchStatementModel, command));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpGap_Adjust")]
        public ActionResult _AdjustIpDetailList(string ipNo)
        {
            IList<IpDetail> ipDetailList = base.genericMgr.FindAll<IpDetail>(selectAdjustIpDetailStatement, new object[] { false, (int)com.Sconit.CodeMaster.IpDetailType.Gap, ipNo });
            return PartialView(ipDetailList);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpGap_Adjust")]
        public ActionResult AdjustEdit(string ipNo)
        {
            if (string.IsNullOrEmpty(ipNo))
            {
                return HttpNotFound();
            }

            #region 条码还是数量
            IList<long> huCount = base.genericMgr.FindAll<long>("select count(*) from IpLocationDetail as i where i.IsClose = ?  and i.IpNo = ? and i.HuId is not null", new object[] { false, ipNo });
            ViewBag.IsContainHu = huCount[0] == 0 ? false : true;
            #endregion

            IpMaster ipMaster = base.genericMgr.FindById<IpMaster>(ipNo);
            return View(ipMaster);
        }

        #region qty adjust
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpGap_Adjust")]
        public JsonResult AdjustIpGapGI(string idStr, string qtyStr)
        {
            return AdjustIpGap(idStr, qtyStr, com.Sconit.CodeMaster.IpGapAdjustOption.GI);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpGap_Adjust")]
        public JsonResult AdjustIpGapGR(string idStr, string qtyStr)
        {
            return AdjustIpGap(idStr, qtyStr, com.Sconit.CodeMaster.IpGapAdjustOption.GR);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpGap_Adjust")]
        public JsonResult AdjustIpGap(string idStr, string qtyStr, com.Sconit.CodeMaster.IpGapAdjustOption gapAdjustOption)
        {
            try
            {
                IList<IpDetail> ipDetailList = new List<IpDetail>();
                if (!string.IsNullOrEmpty(idStr))
                {
                    string[] idArray = idStr.Split(',');
                    string[] qtyArray = qtyStr.Split(',');

                    for (int i = 0; i < idArray.Count(); i++)
                    {
                        if (Convert.ToDecimal(qtyArray[i]) > 0)
                        {
                            IpDetail ipDetail = base.genericMgr.FindById<IpDetail>(Convert.ToInt32(idArray[i]));
                            IpDetailInput input = new IpDetailInput();
                            input.ReceiveQty = Convert.ToDecimal(qtyArray[i]);
                            ipDetail.AddIpDetailInput(input);
                            ipDetailList.Add(ipDetail);
                        }
                    }
                }
                if (ipDetailList.Count() == 0)
                {
                    throw new BusinessException("调整明细不能为空");
                }

                orderMgr.AdjustIpGap(ipDetailList, gapAdjustOption);

                SaveSuccessMessage(Resources.ORD.IpMaster.IpMaster_AsnTemplate, ipDetailList[0].IpNo);
                return Json(new { IpNo = ipDetailList[0].IpNo });
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

        #region hu adjust

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpGap_Adjust")]
        public ActionResult _AdjustHuIpDetailList(string ipNo)
        {
            IList<IpLocationDetail> ipLocationDetailList = base.genericMgr.FindAll<IpLocationDetail>(selectAdjustHuIpDetailStatement, new object[] { false, ipNo });
            if (ipLocationDetailList != null && ipLocationDetailList.Count > 0)
            {
                foreach (IpLocationDetail ipLocDetail in ipLocationDetailList)
                {
                    ipLocDetail.IpDetail = base.genericMgr.FindById<IpDetail>(ipLocDetail.IpDetailId);
                }
            }
            return PartialView(ipLocationDetailList);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpGap_Adjust")]
        public JsonResult AdjustHuIpGapGI(string idStr)
        {
            return AdjustHuIpGap(idStr, com.Sconit.CodeMaster.IpGapAdjustOption.GI);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpGap_Adjust")]
        public JsonResult AdjustHuIpGapGR(string idStr)
        {
            return AdjustHuIpGap(idStr, com.Sconit.CodeMaster.IpGapAdjustOption.GR);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpGap_Adjust")]
        public JsonResult AdjustHuIpGap(string idStr, com.Sconit.CodeMaster.IpGapAdjustOption gapAdjustOption)
        {
            try
            {
                IList<IpDetail> ipDetailList = new List<IpDetail>();
                if (!string.IsNullOrEmpty(idStr))
                {
                    string[] idArray = idStr.Split(',');

                    for (int i = 0; i < idArray.Count(); i++)
                    {
                        IpLocationDetail ipLocationDetail = base.genericMgr.FindById<IpLocationDetail>(Convert.ToInt32(idArray[i]));
                        var existIpDetail = ipDetailList.Where(d => d.Id == ipLocationDetail.IpDetailId).ToList();
                        if (existIpDetail != null && existIpDetail.Count > 0)
                        {
                            IpDetail ipDetail = existIpDetail[0];
                            IpDetailInput input = new IpDetailInput();
                            input.ReceiveQty = ipLocationDetail.Qty / existIpDetail[0].UnitQty; //转为订单单位
                            input.HuId = ipLocationDetail.HuId;
                            input.LotNo = ipLocationDetail.LotNo;
                            existIpDetail[0].AddIpDetailInput(input);
                        }
                        else
                        {
                            IpDetail ipDetail = base.genericMgr.FindById<IpDetail>(ipLocationDetail.IpDetailId);
                            IpDetailInput input = new IpDetailInput();
                            input.ReceiveQty = ipLocationDetail.Qty / ipDetail.UnitQty; //转为订单单位
                            input.HuId = ipLocationDetail.HuId;
                            input.LotNo = ipLocationDetail.LotNo;
                            ipDetail.AddIpDetailInput(input);
                            ipDetailList.Add(ipDetail);
                        }

                    }
                }
                if (ipDetailList.Count() == 0)
                {
                    throw new BusinessException("调整明细不能为空");
                }

                orderMgr.AdjustIpGap(ipDetailList, gapAdjustOption);

                SaveSuccessMessage(Resources.ORD.IpMaster.IpMaster_Adjusted, ipDetailList[0].IpNo);
                return Json(new { IpNo = ipDetailList[0].IpNo });
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

        #endregion

        #endregion

        #region private method
        private SearchStatementModel PrepareSearchStatement(GridCommand command, IpMasterSearchModel searchModel, string whereStatement)
        {

            IList<object> param = new List<object>();

            //SecurityHelper.AddPartyFromPermissionStatement(ref whereStatement, "i", "PartyFrom", com.Sconit.CodeMaster.OrderType.Distribution, false);
            //SecurityHelper.AddPartyToPermissionStatement(ref whereStatement, "i", "PartyTo", com.Sconit.CodeMaster.OrderType.Distribution);
            SecurityHelper.AddPartyFromAndPartyToPermissionStatement(ref whereStatement, "i", "OrderType", "i", "PartyFrom", "i", "PartyTo", com.Sconit.CodeMaster.OrderType.Distribution, false);

            HqlStatementHelper.AddLikeStatement("IpNo", searchModel.IpNo, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Status", searchModel.Status, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("PartyFrom", searchModel.PartyFrom, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("PartyTo", searchModel.PartyTo, "i", ref whereStatement, param);

            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.StartDate, searchModel.EndDate, "i", ref whereStatement, param);
            }
            else if (searchModel.StartDate != null & searchModel.EndDate == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.StartDate, "i", ref whereStatement, param);
            }
            else if (searchModel.StartDate == null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.EndDate, "i", ref whereStatement, param);
            }

            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "IpMasterStatusDescription")
                {
                    command.SortDescriptors[0].Member = "Status";
                }
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
