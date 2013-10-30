/// <summary>
/// Summary description 
/// </summary>
namespace com.Sconit.Web.Controllers.SP
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
    using com.Sconit.Utility;
    using System.Text;
    using System;
    using com.Sconit.PrintModel.ORD;
    using AutoMapper;
    using com.Sconit.Utility.Report;
    using System.ComponentModel;
    using com.Sconit.Entity.MD;
    #endregion

    /// <summary>
    /// This controller response to control the ProcurementOrderIssue.
    /// </summary>
    public class SupplierIpMasterController : WebAppBaseController
    {
        #region Properties
        public IOrderMgr orderMgr { get; set; }

        public IIpMgr ipMgr { get; set; }

        public IReportGen reportGen { get; set; }
        #endregion

        /// <summary>
        /// hql 
        /// </summary>
        private static string selectCountStatement = "select count(*) from IpMaster as i";

        /// <summary>
        /// hql 
        /// </summary>
        /// 
        private static string selectStatement = "select i from IpMaster as i";

        private static string selectIpDetailCountStatement = "select count(*) from IpDetail as i";

        private static string selectIpDetailStatement = "select i from IpDetail as i";

        private static string selectReceiveIpDetailStatement = "select i from IpDetail as i where i.IsClose = ? and i.Type = ? and i.IpNo = ?";

        #region public actions
        /// <summary>
        /// Index action for ProcurementOrderIssue controller
        /// </summary>
        /// <returns>Index view</returns>
        [SconitAuthorize(Permissions = "Url_Supplier_Deliveryorder_Query")]
        public ActionResult Index()
        {
            return View();
        }
        [SconitAuthorize(Permissions = "Url_Supplier_Deliveryorder_Detail_Query")]
        public ActionResult DetailIndex()
        {
            return View();
        }


        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_Receive")]
        public ActionResult ReceiveIndex()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_Cancel")]
        public ActionResult CancelIndex()
        {
            return View();
        }


       



        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Supplier_Deliveryorder_Query")]
        public ActionResult List(GridCommand command, IpMasterSearchModel searchModel)
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
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Supplier_Deliveryorder_Query")]
        public ActionResult _AjaxList(GridCommand command, IpMasterSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<IpMaster>()));
            }
            string whereStatement = string.Empty;
            if (searchModel.Item != null && searchModel.Item != string.Empty)
            {
                whereStatement += " and exists(select 1 from IpDetail as d where d.IpNo = i.IpNo and d.Item = '" + searchModel.Item + "')";
            }
            if (searchModel.IsPrintAsn)
            {
                whereStatement += " and  IsPrintAsn = 0";
            }
            ProcedureSearchStatementModel procedureSearchStatementModel = this.PrepareProcedureSearchStatement(command, searchModel, whereStatement);
            return PartialView(GetAjaxPageDataProcedure<IpMaster>(procedureSearchStatementModel, command));
            //if (!this.CheckSearchModelIsNull(searchModel))
            //{
            //    return PartialView(new GridModel(new List<IpMaster>()));
            //}
            //SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            //return PartialView(GetAjaxPageData<IpMaster>(searchStatementModel, command));
        }

        [GridAction]
        public ActionResult _AjaxIpDetail(string ipNo)
        {
            IList<IpDetail> orderDetList = this.genericMgr.FindAll<IpDetail>(" select d from IpDetail as d where d.IpNo=?", ipNo);
            return PartialView(new GridModel<IpDetail>(orderDetList));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_Receive")]
        public ActionResult CancelList(GridCommand command, IpMasterSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = ProcessSearchModel(command, searchModel);
            SearchStatementModel searchStatementModel = this.PrepareCancelSearchStatement(command, (IpMasterSearchModel)searchCacheModel.SearchObject);
            return View(GetPageData<IpMaster>(searchStatementModel, command));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_Cancel")]
        public ActionResult _CancelAjaxList(GridCommand command, IpMasterSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareCancelSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<IpMaster>(searchStatementModel, command));
        }


        #region 明细菜单 报表
        [SconitAuthorize(Permissions = "Url_Supplier_Deliveryorder_Detail_Query")]
        public ActionResult DetailList(GridCommand command, IpMasterSearchModel searchModel)
        {

            TempData["IpMasterSearchModel"] = searchModel;
            if (this.CheckSearchModelIsNull(searchModel))
            {
                ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
                return View();
            }
            else
            {
                SaveWarningMessage(Resources.ErrorMessage.Errors_NoConditions);
                return View(new List<IpDetail>());
            }
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxIpDetList(GridCommand command, IpMasterSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<IpDetail>()));
            }
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchDetailStatement(command, searchModel, string.Empty);
            IList<IpDetail> ipDetList = new List<IpDetail>();
            procedureSearchStatementModel.SelectProcedure = "USP_Search_PrintIpDet";
            GridModel<object[]> gridModel = GetAjaxPageDataProcedure<object[]>(procedureSearchStatementModel, command);
            if (gridModel.Data != null && gridModel.Data.Count() > 0)
            {
                #region
                ipDetList = (from tak in gridModel.Data
                             select new IpDetail
                             {
                                 Id = (int)tak[0],
                                 IpNo = (string)tak[1],
                                 OrderNo = (string)tak[2],
                                 ExternalOrderNo = (string)tak[3],
                                 ExternalSequence = (string)tak[4],
                                 Item = (string)tak[5],
                                 ReferenceItemCode = (string)tak[6],
                                 ItemDescription = (string)tak[7],
                                 Uom = (string)tak[8],
                                 UnitCount = (decimal)tak[9],
                                 Qty = (decimal)tak[10],
                                 ReceivedQty = (decimal)tak[11],
                                 LocationFrom = (string)tak[12],
                                 LocationTo = (string)tak[13],
                                 Flow = (string)tak[14],
                                 IsClose = (bool)tak[15],
                                 IsInspect = (bool)tak[16],
                                 MastPartyFrom = (string)tak[17],
                                 MastPartyTo = (string)tak[18],
                                 MastType = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.OrderType, int.Parse((tak[19]).ToString())),
                                 MastStatus = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.IpStatus, int.Parse((tak[20]).ToString())),
                                 MastCreateDate = (DateTime)tak[21],
                                 SAPLocation = (string)tak[22],
                             }).ToList();
                #endregion
            }
            procedureSearchStatementModel.PageParameters[2].Parameter = gridModel.Total;
            TempData["IpDetailPrintSearchModel"] = procedureSearchStatementModel;

            GridModel<IpDetail> gridModelOrderDet = new GridModel<IpDetail>();
            gridModelOrderDet.Total = gridModel.Total;
            gridModelOrderDet.Data = ipDetList;

            return PartialView(gridModelOrderDet);
        }
        #endregion


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_Receive")]
        public ActionResult _ReceiveIpDetailList(string id)
        {
            IList<IpDetail> ipDetailList = base.genericMgr.FindAll<IpDetail>(selectReceiveIpDetailStatement, new object[] { false, (int)com.Sconit.CodeMaster.IpDetailType.Normal, id });
            return PartialView(ipDetailList);
        }


        #region Edit 页面Detail
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Supplier_Deliveryorder_Query")]
        public ActionResult _IpDetailList(GridCommand command, IpDetailSearchModel searchModel, string ipNo)
        {
            searchModel.IpNo = ipNo;
            TempData["IpDetailSearchModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_Supplier_Deliveryorder_Query")]
        public ActionResult _AjaxIpDetailList(GridCommand command, IpDetailSearchModel searchModel, string ipNo)
        {
            SearchStatementModel searchStatementModel = this.IpDetailPrepareSearchStatement(command, searchModel, ipNo);
           // return PartialView(GetAjaxPageData<IpDetail>(searchStatementModel, command));
            GridModel<IpDetail> gridList = GetAjaxPageData<IpDetail>(searchStatementModel, command);
            int i = 0;
            foreach (IpDetail ipDetail in gridList.Data)
            {
                if (i > command.PageSize)
                {
                    break;
                }
                ipDetail.SAPLocationTo = base.genericMgr.FindById<Location>(ipDetail.LocationTo).SAPLocation;

            }
            gridList.Data = gridList.Data.Where(o => o.Type == com.Sconit.CodeMaster.IpDetailType.Normal);
            return PartialView(gridList);
        }
        #endregion

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_Receive")]
        public ActionResult ReceiveEdit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            IpMaster ip = base.genericMgr.FindById<IpMaster>(id);
            return View(ip);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_Cancel")]
        public ActionResult CancelEdit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            IpMaster ip = base.genericMgr.FindById<IpMaster>(id);
            return View(ip);
        }


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_Receive")]
        public ActionResult ReceiveIpMaster(int[] id, decimal[] currentReceiveQty)
        {
            IList<IpDetail> ipDetailList = new List<IpDetail>();
            for (int i = 0; i < currentReceiveQty.Count(); i++)
            {
                if (currentReceiveQty[i] > 0)
                {
                    IpDetail ipDet = base.genericMgr.FindById<IpDetail>(id[i]);
                    IpDetailInput input = new IpDetailInput();
                    input.ReceiveQty = currentReceiveQty[i];

                    ipDet.AddIpDetailInput(input);
                    //校验还没发
                    ipDetailList.Add(ipDet);
                }
            }

            if (ipDetailList.Count() == 0)
            {
                return HttpNotFound();
            }
            else
            {
                try
                {
                    orderMgr.ReceiveIp(ipDetailList);
                    SaveSuccessMessage(Resources.ORD.OrderMaster.OrderMaster_Received);
                    return RedirectToAction("ReceiveIndex");
                }
                catch (BusinessException ex)
                {
                    SaveBusinessExceptionMessage(ex);
                    return View();
                }
            }
        }

        [SconitAuthorize(Permissions = "Url_Supplier_Deliveryorder_Cancel")]
        public ActionResult CancelIpMaster(string ipNo)
        {
            try
            {
                ipMgr.CancelIp(ipNo);
                SaveSuccessMessage(Resources.ORD.IpMaster.IpMaster_Cancel);
                return RedirectToAction("CancelIndex");
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                return View();
            }

        }
        [SconitAuthorize(Permissions = "Url_Supplier_Deliveryorder_Query")]
        public ActionResult _Edit(string IpNo, string urlId,string orderNo)
        {
            ViewBag.UrlId = urlId;
            ViewBag.OrderNo = orderNo;
            if (string.IsNullOrEmpty(IpNo))
            {
                return HttpNotFound();
            }
            IpMaster ip = base.genericMgr.FindById<IpMaster>(IpNo);
            return View(ip);
        }



        #region 打印导出
        public void SaveToClient(string ipNo)
        {
            IpMaster ipMaster = base.genericMgr.FindById<IpMaster>(ipNo);
            IList<IpDetail> ipDetails = base.genericMgr.FindAll<IpDetail>("select id from IpDetail as id where id.IpNo=?", ipNo);
            ipMaster.IpDetails = ipDetails;
            //orderMaster.OrderDetails = orderDetails
            PrintIpMaster printIpMaster = Mapper.Map<IpMaster, PrintIpMaster>(ipMaster);
            IList<object> data = new List<object>();
            data.Add(printIpMaster);
            data.Add(printIpMaster.IpDetails);
            //string reportFileUrl = reportGen.WriteToFile(orderMaster.OrderTemplate, data);
            reportGen.WriteToClient(ipMaster.AsnTemplate, data, ipMaster.AsnTemplate);

            //return reportFileUrl;
            //reportGen.WriteToFile(orderMaster.OrderTemplate, data);
        }

        public string Print(string ipNo)
        {
            IpMaster ipMaster = base.genericMgr.FindById<IpMaster>(ipNo);
            IList<IpDetail> ipDetails = base.genericMgr.FindAll<IpDetail>("select id from IpDetail as id where id.IpNo=?", ipNo);
            ipMaster.IpDetails = ipDetails;
            //orderMaster.OrderDetails = orderDetails
            PrintIpMaster printIpMaster = Mapper.Map<IpMaster, PrintIpMaster>(ipMaster);
            IList<object> data = new List<object>();
            data.Add(printIpMaster);
            data.Add(printIpMaster.IpDetails);
            string reportFileUrl = reportGen.WriteToFile(ipMaster.AsnTemplate, data);
            //reportGen.WriteToClient(orderMaster.OrderTemplate, data, orderMaster.OrderTemplate);
            ipMaster.IsAsnPrinted = true;
            this.genericMgr.Update(ipMaster);
            return reportFileUrl;
            //reportGen.WriteToFile(orderMaster.OrderTemplate, data);
        }

        public string PrintOrders(string ipNos)
        {
            string[] ipNoArray = ipNos.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string hqlMstr = "select im from IpMaster as im where im.IpNo in(?";
            string hqlDet = "select d from IpDetail as d where d.IpNo in(?";
            List<object> paras = new List<object>();
            paras.Add(ipNoArray[0]);
            for (int i = 1; i < ipNoArray.Length; i++)
            {
                hqlMstr = hqlMstr + ",?";
                hqlDet = hqlDet + ",?";
                paras.Add(ipNoArray[i]);
            }
            hqlDet = hqlDet + ") ";
            hqlMstr = hqlMstr + ") ";

            IList<IpMaster> ipMasterList = this.genericMgr.FindAll<IpMaster>(hqlMstr, paras.ToArray());
            IList<IpDetail> ipDetailList = this.genericMgr.FindAll<IpDetail>(hqlDet, paras.ToArray());


            StringBuilder printUrls = new StringBuilder();
            foreach (var ipMaster in ipMasterList)
            {
                ipMaster.IpDetails = ipDetailList.Where(o => o.IpNo == ipMaster.IpNo).ToList();

                PrintIpMaster printIpMaster = Mapper.Map<IpMaster, PrintIpMaster>(ipMaster);
                IList<object> data = new List<object>();
                data.Add(printIpMaster);
                data.Add(printIpMaster.IpDetails);
                string reportFileUrl = reportGen.WriteToFile(ipMaster.AsnTemplate, data);
                printUrls.Append(reportFileUrl);
                printUrls.Append("||");
                ipMaster.IsAsnPrinted = true;
                this.genericMgr.Update(ipMaster);
            }
            return printUrls.ToString();
        }

        #region 明细批量导出
        public void SaveToClientDetails()
        {
            ProcedureSearchStatementModel procedureSearchStatementModel = TempData["IpDetailPrintSearchModel"] as ProcedureSearchStatementModel;
            TempData["IpDetailPrintSearchModel"] = procedureSearchStatementModel;

            GridCommand command = new GridCommand();
            command.Page = 1;
            command.PageSize = (int)procedureSearchStatementModel.PageParameters[2].Parameter;
            procedureSearchStatementModel.PageParameters[3].Parameter = 1;
            GridModel<object[]> gridModel = GetAjaxPageDataProcedure<object[]>(procedureSearchStatementModel, command);

            IList<IpDetail> ipDetailList = new List<IpDetail>();
            if (gridModel.Data != null && gridModel.Data.Count() > 0)
            {
                #region
                ipDetailList = (from tak in gridModel.Data
                                select new IpDetail
                                {
                                    Id = (int)tak[0],
                                    IpNo = (string)tak[1],
                                    OrderNo = (string)tak[2],
                                    ExternalOrderNo = (string)tak[3],
                                    ExternalSequence = (string)tak[4],
                                    Item = (string)tak[5],
                                    ReferenceItemCode = (string)tak[6],
                                    ItemDescription = (string)tak[7],
                                    Uom = (string)tak[8],
                                    UnitCount = (decimal)tak[9],
                                    Qty = (decimal)tak[10],
                                    ReceivedQty = (decimal)tak[11],
                                    LocationFrom = (string)tak[12],
                                    LocationTo = (string)tak[13],
                                    Flow = (string)tak[14],
                                    IsClose = (bool)tak[15],
                                    IsInspect = (bool)tak[16],
                                    MastPartyFrom = (string)tak[17],
                                    MastPartyTo = (string)tak[18],
                                    MastType = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.OrderType, int.Parse((tak[19]).ToString())),
                                    MastStatus = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.IpStatus, int.Parse((tak[20]).ToString())),
                                    MastCreateDate = (DateTime)tak[21],
                                    SAPLocation = (string)tak[22],
                                }).ToList();
                #endregion
            }
            IList<object> data = new List<object>();
            data.Add(ipDetailList);
            reportGen.WriteToClient("IpDetView.xls", data, "IpDetView.xls");
        }
        #endregion

        #endregion

        #endregion

        #region Private
        private SearchStatementModel PrepareSearchStatement(GridCommand command, IpMasterSearchModel searchModel)
        {
            string whereStatement = " where i.OrderType in ("
                             + (int)com.Sconit.CodeMaster.OrderType.Procurement + "," + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + ","
                             + (int)com.Sconit.CodeMaster.OrderType.SubContract + "," + (int)com.Sconit.CodeMaster.OrderType.ScheduleLine + ")";

            IList<object> param = new List<object>();
            HqlStatementHelper.AddLikeStatement("WMSNo", searchModel.WMSNo, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Dock", searchModel.Dock, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("IpNo", searchModel.IpNo, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Status", searchModel.Status, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("PartyFrom", searchModel.PartyFrom, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("PartyTo", searchModel.PartyTo, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("OrderType", searchModel.IpOrderType, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "i", ref whereStatement, param);
            SecurityHelper.AddPartyFromPermissionStatement(ref whereStatement, "i", "PartyFrom", com.Sconit.CodeMaster.OrderType.Procurement, true);
            SecurityHelper.AddPartyFromAndPartyToPermissionStatement(ref whereStatement, "i", "OrderType", "i", "PartyFrom", "i", "PartyTo", com.Sconit.CodeMaster.OrderType.Procurement, true);

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

            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by i.CreateDate desc";
            }

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        private ProcedureSearchStatementModel PrepareProcedureSearchStatement(GridCommand command, IpMasterSearchModel searchModel, string whereStatement)
        {
            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.IpNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Status, Type = NHibernate.NHibernateUtil.Int16 });
            paraList.Add(new ProcedureParameter
            {
                Parameter = (int)com.Sconit.CodeMaster.OrderType.Procurement + "," + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + ","
                            + (int)com.Sconit.CodeMaster.OrderType.SubContract + "," + (int)com.Sconit.CodeMaster.OrderType.ScheduleLine,
                Type = NHibernate.NHibernateUtil.String
            });

            paraList.Add(new ProcedureParameter { Parameter = searchModel.PartyFrom, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.PartyTo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.StartDate, Type = NHibernate.NHibernateUtil.DateTime });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.EndDate, Type = NHibernate.NHibernateUtil.DateTime });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Dock, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Item, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.OrderNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.WMSNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.ManufactureParty, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Flow, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = true, Type = NHibernate.NHibernateUtil.Boolean });
            paraList.Add(new ProcedureParameter { Parameter = CurrentUser.Id, Type = NHibernate.NHibernateUtil.Int32 });
            paraList.Add(new ProcedureParameter { Parameter = whereStatement, Type = NHibernate.NHibernateUtil.String });


            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "IpMasterStatusDescription")
                {
                    command.SortDescriptors[0].Member = "Status";
                }
            }

            pageParaList.Add(new ProcedureParameter { Parameter = command.SortDescriptors.Count > 0 ? command.SortDescriptors[0].Member : null, Type = NHibernate.NHibernateUtil.String });
            pageParaList.Add(new ProcedureParameter { Parameter = command.SortDescriptors.Count > 0 ? (command.SortDescriptors[0].SortDirection == ListSortDirection.Descending ? "desc" : "asc") : "asc", Type = NHibernate.NHibernateUtil.String });
            pageParaList.Add(new ProcedureParameter { Parameter = command.PageSize, Type = NHibernate.NHibernateUtil.Int32 });
            pageParaList.Add(new ProcedureParameter { Parameter = command.Page, Type = NHibernate.NHibernateUtil.Int32 });

            var procedureSearchStatementModel = new ProcedureSearchStatementModel();
            procedureSearchStatementModel.Parameters = paraList;
            procedureSearchStatementModel.PageParameters = pageParaList;
            procedureSearchStatementModel.CountProcedure = "USP_Search_IpMstrCount";
            procedureSearchStatementModel.SelectProcedure = "USP_Search_IpMstr";

            return procedureSearchStatementModel;
        }

        private SearchStatementModel PrepareCancelSearchStatement(GridCommand command, IpMasterSearchModel searchModel)
        {
            string whereStatement = "where  i.OrderType in ("
                                    + (int)com.Sconit.CodeMaster.OrderType.Procurement + "," + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.ScheduleLine + ","
                                    + (int)com.Sconit.CodeMaster.OrderType.SubContract + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + ")"
                                    + " and i.Status = " + (int)com.Sconit.CodeMaster.IpStatus.Submit;

            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("IpNo", searchModel.IpNo, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("PartyFrom", searchModel.PartyFrom, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("PartyTo", searchModel.PartyTo, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("OrderType", searchModel.IpOrderType, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "i", ref whereStatement, param);

            SecurityHelper.AddPartyFromPermissionStatement(ref whereStatement, "i", "PartyFrom", com.Sconit.CodeMaster.OrderType.Procurement, true);
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

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);


            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        private SearchStatementModel IpDetailPrepareSearchStatement(GridCommand command, IpDetailSearchModel searchModel, string ipNo)
        {
            string whereStatement = " where i.IpNo='" + ipNo + "'";

            IList<object> param = new List<object>();

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectIpDetailCountStatement;
            searchStatementModel.SelectStatement = selectIpDetailStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        private ProcedureSearchStatementModel PrepareSearchDetailStatement(GridCommand command, IpMasterSearchModel searchModel, string whereStatement)
        {
            whereStatement = "and exists(select 1 from IpMaster  as i where i.OrderSubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal + " and i.IpNo=d.IpNo)";
            if (!string.IsNullOrEmpty(searchModel.ExternalOrderNo))
            {
                whereStatement += " and d.ExtNo='" + searchModel.ExternalOrderNo + "'";
            }
            if (!string.IsNullOrEmpty(searchModel.ExternalSequence))
            {
                whereStatement += " and d.ExtSeq='" + searchModel.ExternalSequence + "'";
            }
            if (searchModel.IsShowGap)
            {
                whereStatement += " and d.RecQty <> d.Qty ";
            }
            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.IpNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Status, Type = NHibernate.NHibernateUtil.Int16 });
            paraList.Add(new ProcedureParameter
            {
                Parameter = (int)(int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.Procurement
                                    + "," + (int)com.Sconit.CodeMaster.OrderType.ScheduleLine,
                Type = NHibernate.NHibernateUtil.String
                // + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract
            });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.PartyFrom, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.PartyTo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.StartDate, Type = NHibernate.NHibernateUtil.DateTime });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.EndDate, Type = NHibernate.NHibernateUtil.DateTime });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Dock, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Item, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.OrderNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.WMSNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.ManufactureParty, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Flow, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = true, Type = NHibernate.NHibernateUtil.Int64 });
            paraList.Add(new ProcedureParameter { Parameter = CurrentUser.Id, Type = NHibernate.NHibernateUtil.Int32 });
            paraList.Add(new ProcedureParameter { Parameter = whereStatement, Type = NHibernate.NHibernateUtil.String });


            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "ExternalOrderNo")
                {

                    command.SortDescriptors[0].Member = "ExtNo";
                }
                else if (command.SortDescriptors[0].Member == "ExternalSequence")
                {

                    command.SortDescriptors[0].Member = "ExtSeq";
                }
                else if (command.SortDescriptors[0].Member == "LocationTo")
                {

                    command.SortDescriptors[0].Member = "LocTo";
                }
                else if (command.SortDescriptors[0].Member == "ItemDescription")
                {

                    command.SortDescriptors[0].Member = "Item";
                }
                else if (command.SortDescriptors[0].Member == "ReceivedQty")
                {

                    command.SortDescriptors[0].Member = "RecQty";
                }
                else if (command.SortDescriptors[0].Member == "UnitCountDescription")
                {

                    command.SortDescriptors[0].Member = "UCDesc";
                }
                else if (command.SortDescriptors[0].Member == "ContainerDescription")
                {

                    command.SortDescriptors[0].Member = "ContainerDesc";
                }
                else if (command.SortDescriptors[0].Member == "Sequence")
                {

                    command.SortDescriptors[0].Member = "Seq";
                }
            }
            pageParaList.Add(new ProcedureParameter { Parameter = command.SortDescriptors.Count > 0 ? command.SortDescriptors[0].Member : null, Type = NHibernate.NHibernateUtil.String });
            pageParaList.Add(new ProcedureParameter { Parameter = command.SortDescriptors.Count > 0 ? (command.SortDescriptors[0].SortDirection == ListSortDirection.Descending ? "desc" : "asc") : "asc", Type = NHibernate.NHibernateUtil.String });
            pageParaList.Add(new ProcedureParameter { Parameter = command.PageSize, Type = NHibernate.NHibernateUtil.Int32 });
            pageParaList.Add(new ProcedureParameter { Parameter = command.Page, Type = NHibernate.NHibernateUtil.Int32 });

            var procedureSearchStatementModel = new ProcedureSearchStatementModel();
            procedureSearchStatementModel.Parameters = paraList;
            procedureSearchStatementModel.PageParameters = pageParaList;
            procedureSearchStatementModel.CountProcedure = "USP_Search_IpDetCount";
            procedureSearchStatementModel.SelectProcedure = "USP_Search_IpDet";

            return procedureSearchStatementModel;
        }

        private string PrepareSearchDetailStatement(GridCommand command, IpMasterSearchModel searchModel)
        {
            StringBuilder Sb = new StringBuilder();
            string whereStatement = " select  d from IpDetail as d  where exists (select 1 from IpMaster  as o where o.OrderType in (" + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.Procurement
                                    + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract + "," + (int)com.Sconit.CodeMaster.OrderType.ScheduleLine + ")"
                                    + " and o.OrderSubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal + " and o.IpNo=d.IpNo ";

            Sb.Append(whereStatement);



            if (searchModel.Status != null)
            {
                Sb.Append(string.Format(" and o.Status = '{0}'", searchModel.Status));
            }

            if (!string.IsNullOrEmpty(searchModel.IpNo))
            {
                Sb.Append(string.Format(" and o.IpNo like '{0}%'", searchModel.IpNo));
            }
            if (!string.IsNullOrEmpty(searchModel.PartyFrom))
            {
                Sb.Append(string.Format(" and o.PartyFrom = '{0}'", searchModel.PartyFrom));
            }
            if (!string.IsNullOrEmpty(searchModel.PartyTo))
            {
                Sb.Append(string.Format(" and o.PartyTo = '{0}'", searchModel.PartyTo));

            }
            string str = Sb.ToString();
            //SecurityHelper.AddPartyFromPermissionStatement(ref str, "o", "PartyFrom", com.Sconit.CodeMaster.OrderType.Procurement, true);
            SecurityHelper.AddPartyFromAndPartyToPermissionStatement(ref str, "o", "Type", "o", "PartyFrom", "o", "PartyTo", com.Sconit.CodeMaster.OrderType.Procurement, true);
            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                Sb.Append(string.Format(" and o.CreateDate between '{0}' and '{1}'", searchModel.StartDate, searchModel.EndDate));
                //HqlStatementHelper.AddBetweenStatement("StartTime", searchModel.DateFrom, searchModel.DateTo, "o", ref whereStatement, param);
            }
            else if (searchModel.StartDate != null & searchModel.EndDate == null)
            {
                Sb.Append(string.Format(" and o.CreateDate >= '{0}'", searchModel.StartDate));

            }
            else if (searchModel.StartDate == null & searchModel.EndDate != null)
            {
                Sb.Append(string.Format(" and o.CreateDate <= '{0}'", searchModel.EndDate));

            }
            if (!string.IsNullOrEmpty(searchModel.WMSNo))
            {
                Sb.Append(string.Format(" and  o.WMSNo like '%{0}%'", searchModel.WMSNo));
            }

            Sb.Append(" )");

            if (searchModel.Flow != null)
            {
                Sb.Append(string.Format(" and d.Flow = '{0}'", searchModel.Flow));
            }

            if (!string.IsNullOrEmpty(searchModel.Item))
            {
                Sb.Append(string.Format(" and  d.Item like '{0}%'", searchModel.Item));

            }
            if (!string.IsNullOrEmpty(searchModel.OrderNo))
            {
                Sb.Append(string.Format(" and  d.OrderNo like '{0}%'", searchModel.OrderNo));
            }
            return Sb.ToString();
        }
        #endregion

    }
}
