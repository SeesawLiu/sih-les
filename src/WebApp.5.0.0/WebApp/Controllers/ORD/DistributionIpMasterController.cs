/// <summary>
/// Summary description for IpMasterController
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
    using com.Sconit.PrintModel.ORD;
    using AutoMapper;
    using com.Sconit.Utility.Report;
    using System;
    using com.Sconit.Utility;
    using System.Text;
    using System.ComponentModel;
    using com.Sconit.Entity.MD;
    #endregion

    /// <summary>
    /// This controller response to control the IpMaster.
    /// </summary>
    public class DistributionIpMasterController : WebAppBaseController
    {
        #region Properties

        public IOrderMgr orderMgr { get; set; }

        public IIpMgr ipMgr { get; set; }

        public IReportGen reportGen { get; set; }
        #endregion

        #region private hql
        /// <summary>
        /// hql to get count of the IpMaster
        /// </summary>
        private static string selectCountStatement = "select count(*) from IpMaster as i";

        /// <summary>
        /// hql to get all of the IpMaster
        /// </summary>
        private static string selectStatement = "select i from IpMaster as i";

        /// <summary>
        /// hql to get count of the IpMaster by IpMaster's code
        /// </summary>
        //private static string duiplicateVerifyStatement = @"select count(*) from IpMaster as i where i.Code = ?";


        private static string selectIpDetailCountStatement = "select count(*) from IpDetail as i";

        private static string selectIpDetailStatement = "select i from IpDetail as i";

        private static string selectReceiveIpDetailStatement = "select i from IpDetail as i where i.IsClose = ? and i.Type = ? and i.IpNo = ?";

        private static string selectReceiveIpLocationDetailStatement = "select i from IpLocationDetail as i where i.IsClose = ? and i.IpNo = ?";
        #endregion

        #region public actions

        #region view
        /// <summary>
        /// Index action for IpMaster controller
        /// </summary>
        /// <returns>Index view</returns>
        [SconitAuthorize(Permissions = "Url_DistributionIpMaster_View")]
        public ActionResult Index()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_Distribution_IpDetail")]
        public ActionResult DetailIndex()
        {
            return View();
        }

        /// <summary>
        /// List action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">IpMaster Search model</param>
        /// <returns>return the result view</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpMaster_View")]
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



        /// <summary>
        ///  AjaxList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">IpMaster Search Model</param>
        /// <returns>return the result action</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpMaster_View")]
        public ActionResult _AjaxList(GridCommand command, IpMasterSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<IpMaster>()));
            }
            string whereStatement = string.Empty;
            if (searchModel.Item != null && searchModel.Item != string.Empty)
                whereStatement += " and exists(select 1 from IpDetail as d where d.IpNo = i.IpNo and d.Item = '" + searchModel.Item + "')";
            ProcedureSearchStatementModel procedureSearchStatementModel = this.PrepareProcedureSearchStatement(command, searchModel, whereStatement);
            return PartialView(GetAjaxPageDataProcedure<IpMaster>(procedureSearchStatementModel, command));
        }

        #region 明细菜单 报表
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
            string whereStatement = " and exists (select 1 from IpMaster  as i where  i.Type = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal + " and i.IpNo=d.IpNo) ";
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
            //ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchDetailStatement(command, searchModel, whereStatement);
            //return PartialView(GetAjaxPageDataProcedure<IpDetail>(procedureSearchStatementModel, command));
            IList<IpDetail> ipDetList = new List<IpDetail>();
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchDetailStatement(command, searchModel, whereStatement);
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

        /// <summary>
        /// Edit view
        /// </summary>
        /// <param name="id">IpMaster id for edit</param>
        /// <returns>return the result view</returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_DistributionIpMaster_View")]
        public ActionResult Edit(string IpNo)
        {
            if (string.IsNullOrEmpty(IpNo))
            {
                return HttpNotFound();
            }
            else
            {
                IpMaster ipMaster = base.genericMgr.FindById<IpMaster>(IpNo);
                return View(ipMaster);
            }
        }


        #region Edit 页面的明细列表
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpMaster_View")]
        public ActionResult IpDetail(GridCommand command, IpDetailSearchModel searchModel, string ipNo)
        {
            IpMaster ipMaster = base.genericMgr.FindById<IpMaster>(ipNo);
            ViewBag.IsCancel = ((ipMaster.Status == com.Sconit.CodeMaster.IpStatus.Cancel || ipMaster.Status == com.Sconit.CodeMaster.IpStatus.Close) ? false : true)
                && ipMaster.OrderType == com.Sconit.CodeMaster.OrderType.ScheduleLine && !ipMaster.IsAsnUniqueReceive;
            searchModel.IpNo = ipNo;
            TempData["IpDetailSearchModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpMaster_View")]
        public ActionResult _AjaxIpDetailList(GridCommand command, IpDetailSearchModel searchModel, string ipNo)
        {
            //SearchStatementModel searchStatementModel = this.IpDetailPrepareSearchStatement(command, searchModel, ipNo);
            //return PartialView(GetAjaxPageData<IpDetail>(searchStatementModel, command));
            SearchStatementModel searchStatementModel = this.IpDetailPrepareSearchStatement(command, searchModel, ipNo);
            GridModel<IpDetail> gridList = GetAjaxPageData<IpDetail>(searchStatementModel, command);
            int i = 0;
            foreach (IpDetail ipDetail in gridList.Data)
            {
                if (i > command.PageSize)
                {
                    break;
                }
                if (!string.IsNullOrEmpty(ipDetail.LocationTo))
                {
                    ipDetail.SAPLocationTo = base.genericMgr.FindById<Location>(ipDetail.LocationTo).SAPLocation;
                }
            }
            gridList.Data = gridList.Data.Where(o => o.Type == com.Sconit.CodeMaster.IpDetailType.Normal);
            return PartialView(gridList);
        }

        #endregion

        [SconitAuthorize(Permissions = "Url_IpMaster_New")]
        public ActionResult New()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_IpMaster_New")]
        public ActionResult _AjaxOrderMasterList(GridCommand command, OrderMasterSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.OrderMasterPrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<OrderMaster>(searchStatementModel, command));
        }
        #endregion

        #region receive
        [SconitAuthorize(Permissions = "Url_DistributionIpMaster_Receive")]
        [HttpGet]
        public ActionResult ReceiveIndex()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpMaster_Receive")]
        public ActionResult ReceiveList(GridCommand command, IpMasterSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpMaster_Receive")]
        public ActionResult _ReceiveAjaxList(GridCommand command, IpMasterSearchModel searchModel)
        {
            string whereStatement = " and i.Status in (" + (int)com.Sconit.CodeMaster.IpStatus.Submit + "," + (int)com.Sconit.CodeMaster.IpStatus.InProcess + ")"
                               + " and exists (select 1 from IpDetail as d where d.IsClose = 0 and d.Type = 0 and d.IpNo = i.IpNo) ";

            ProcedureSearchStatementModel procedureSearchStatementModel = this.PrepareReceiveProcedureSearchStatement(command, searchModel, whereStatement);
            return PartialView(GetAjaxPageDataProcedure<IpMaster>(procedureSearchStatementModel, command));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpMaster_Receive")]
        public ActionResult _ReceiveIpDetailList(string ipNo)
        {
            IList<IpDetail> ipDetailList = base.genericMgr.FindAll<IpDetail>(selectReceiveIpDetailStatement, new object[] { false, (int)com.Sconit.CodeMaster.IpDetailType.Normal, ipNo });
            return PartialView(ipDetailList);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpMaster_Receive")]
        public ActionResult _ReceiveHuIpDetailList(string ipNo)
        {
            IList<IpLocationDetail> ipDetailList = base.genericMgr.FindAll<IpLocationDetail>(selectReceiveIpLocationDetailStatement, new object[] { false, ipNo });
            return PartialView(ipDetailList);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpMaster_Receive")]
        public ActionResult ReceiveEdit(string ipNo)
        {
            if (string.IsNullOrEmpty(ipNo))
            {
                return HttpNotFound();
            }

            IpMaster ipMaster = base.genericMgr.FindById<IpMaster>(ipNo);
            ipMaster.IpMasterStatusDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.IpStatus, ((int)ipMaster.Status).ToString());
            ipMaster.IpMasterTypeDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.IpDetailType, ((int)ipMaster.QualityType).ToString());
            return View(ipMaster);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpMaster_Receive")]
        public JsonResult ReceiveIpMaster(string idStr, string qtyStr)
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
                    throw new BusinessException("收货明细不能为空");
                }

                orderMgr.ReceiveIp(ipDetailList);
                SaveSuccessMessage(Resources.ORD.IpMaster.IpMaster_Received, ipDetailList[0].IpNo);
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

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionIpMaster_Receive")]
        public JsonResult ReceiveHuIpMaster(string idStr)
        {
            IList<IpDetail> ipDetailList = new List<IpDetail>();
            try
            {
                if (!string.IsNullOrEmpty(idStr))
                {
                    string[] idArr = idStr.Split(',');
                    foreach (string id in idArr)
                    {
                        IpLocationDetail ipLocationDetail = base.genericMgr.FindById<IpLocationDetail>(int.Parse(id));
                        IpDetail ipDetail = base.genericMgr.FindById<IpDetail>(ipLocationDetail.IpDetailId);
                        IpDetailInput input = new IpDetailInput();
                        input.ReceiveQty = ipLocationDetail.Qty / ipDetail.UnitQty;  //转为订单单位
                        ipDetail.AddIpDetailInput(input);
                        ipDetailList.Add(ipDetail);
                    }

                }
                if (ipDetailList.Count() == 0)
                {
                    throw new BusinessException("收货明细不能为空");
                }

                orderMgr.ReceiveIp(ipDetailList);
                SaveSuccessMessage(Resources.ORD.IpMaster.IpMaster_Received, ipDetailList[0].IpNo);
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

        #region Close
        [SconitAuthorize(Permissions = "Url_DistributionIpMaster_Close")]
        public ActionResult Close(string id)
        {
            try
            {
                //IpMaster ipMstr = genericMgr.FindById<IpMaster>(id);
                ipMgr.ManualCloseIp(id);
                SaveSuccessMessage("送货单关闭成功");
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.Message);
            }
            return RedirectToAction("Edit", new { ipNo = id });
        }
        #endregion

        #region cancel
        [SconitAuthorize(Permissions = "Url_DistributionIpMaster_Cancel")]
        public ActionResult Cancel(string id)
        {
            try
            {
                ipMgr.CancelIp(id);
                SaveSuccessMessage("送货单取消成功");
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.Message);
            }
            return RedirectToAction("Edit", new { ipNo = id });
        }
        #endregion

        #region CancelIpDetail
        [SconitAuthorize(Permissions = "Url_DistributionIpMaster_Cancel")]
        public JsonResult CancelIpDetail(string Id)
        {
            try
            {
                IpDetail ipDet = base.genericMgr.FindById<IpDetail>(int.Parse(Id));
                if (ipMgr.TryCloseExpiredScheduleLineIpDetail(ipDet))
                {
                    SaveSuccessMessage("取消成功。");
                    return Json(new { IpNo = ipDet.IpNo });
                }
                else
                {
                    throw new BusinessException("取消失败。");
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

        #region 打印导出
        public void SaveToClient(string ipNo)
        {
            IpMaster ipMaster = base.genericMgr.FindById<IpMaster>(ipNo);
            IList<IpDetail> ipDetails = base.genericMgr.FindAll<IpDetail>("select id from IpDetail as id where id.IpNo=?", ipNo);
            ipMaster.IpDetails = ipDetails;
            PrintIpMaster printIpMaster = Mapper.Map<IpMaster, PrintIpMaster>(ipMaster);
            IList<object> data = new List<object>();
            data.Add(printIpMaster);
            data.Add(printIpMaster.IpDetails.OrderBy(i => i.Item).ToList());
            reportGen.WriteToClient(ipMaster.AsnTemplate, data, ipMaster.AsnTemplate);

        }

        public string Print(string ipNo)
        {
            IpMaster ipMaster = base.genericMgr.FindById<IpMaster>(ipNo);
            IList<IpDetail> ipDetails = base.genericMgr.FindAll<IpDetail>("select id from IpDetail as id where id.IpNo=?", ipNo);
            ipMaster.IpDetails = ipDetails;
            PrintIpMaster printIpMaster = Mapper.Map<IpMaster, PrintIpMaster>(ipMaster);
            IList<object> data = new List<object>();
            data.Add(printIpMaster);
            data.Add(printIpMaster.IpDetails.OrderBy(i => i.Item).ToList());
            return reportGen.WriteToFile(ipMaster.AsnTemplate, data);
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

        #region private action
        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">IpMaster Search Model</param>
        /// <returns>return IpMaster search model</returns>

        private ProcedureSearchStatementModel PrepareProcedureSearchStatement(GridCommand command, IpMasterSearchModel searchModel, string whereStatement)
        {
            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.IpNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Status, Type = NHibernate.NHibernateUtil.Int16 });
            paraList.Add(new ProcedureParameter
            {
                Parameter = (int)com.Sconit.CodeMaster.OrderType.Distribution + ","  + (int)com.Sconit.CodeMaster.OrderType.Procurement
                    + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.ScheduleLine,
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
            paraList.Add(new ProcedureParameter { Parameter = false, Type = NHibernate.NHibernateUtil.Boolean });
            paraList.Add(new ProcedureParameter { Parameter = CurrentUser.Id, Type = NHibernate.NHibernateUtil.Int32 });
            paraList.Add(new ProcedureParameter { Parameter = whereStatement, Type = NHibernate.NHibernateUtil.String });


            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "IpMasterStatusDescription")
                {
                    command.SortDescriptors[0].Member = "Status";
                }
                if (command.SortDescriptors[0].Member == "IpMasterTypeDescription")
                {
                    command.SortDescriptors[0].Member = "Type";
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

        private ProcedureSearchStatementModel PrepareReceiveProcedureSearchStatement(GridCommand command, IpMasterSearchModel searchModel, string whereStatement)
        {
            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.IpNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Status, Type = NHibernate.NHibernateUtil.Int16 });
            paraList.Add(new ProcedureParameter
            {
                Parameter = (int)com.Sconit.CodeMaster.OrderType.Distribution + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer,
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
            paraList.Add(new ProcedureParameter { Parameter = false, Type = NHibernate.NHibernateUtil.Boolean });
            paraList.Add(new ProcedureParameter { Parameter = CurrentUser.Id, Type = NHibernate.NHibernateUtil.Int32 });
            paraList.Add(new ProcedureParameter { Parameter = whereStatement, Type = NHibernate.NHibernateUtil.String });


            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "IpMasterStatusDescription")
                {
                    command.SortDescriptors[0].Member = "Status";
                }
                if (command.SortDescriptors[0].Member == "IpMasterTypeDescription")
                {
                    command.SortDescriptors[0].Member = "Type";
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

        private SearchStatementModel OrderMasterPrepareSearchStatement(GridCommand command, OrderMasterSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            if (searchModel.OrderNo != null && searchModel.OrderNo != string.Empty)
            {
                HqlStatementHelper.AddEqStatement("OrderNo", searchModel.OrderNo, "i", ref whereStatement, param);
            }
            else
            {
                if (searchModel.Flow != null && searchModel.Flow != string.Empty)
                {
                    HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "i", ref whereStatement, param);
                }
                else
                {
                    if (searchModel.PartyFrom != null && searchModel.PartyFrom != string.Empty)
                    {
                        HqlStatementHelper.AddEqStatement("Flow", searchModel.PartyFrom, "i", ref whereStatement, param);
                    }
                    if (searchModel.PartyTo != null && searchModel.PartyTo != string.Empty)
                    {
                        HqlStatementHelper.AddEqStatement("Flow", searchModel.PartyTo, "i", ref whereStatement, param);
                    }
                }
            }
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

            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.IpNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Status, Type = NHibernate.NHibernateUtil.Int16 });
            paraList.Add(new ProcedureParameter
            {
                Parameter = +(int)com.Sconit.CodeMaster.OrderType.Procurement
                   + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.ScheduleLine,
                //"," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract + "," + (int)com.Sconit.CodeMaster.OrderType.Distribution+ "," + (int)com.Sconit.CodeMaster.OrderType.Production 
                //(int)com.Sconit.CodeMaster.OrderType.CustomerGoods + ","
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
            paraList.Add(new ProcedureParameter { Parameter = false, Type = NHibernate.NHibernateUtil.Int64 });
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

        #endregion

        //[SconitAuthorize(Permissions = "Url_Distribution_IpDetail")]
        //public ActionResult DetailList(GridCommand command, IpMasterSearchModel searchModel)
        //{
        //    TempData["IpMasterSearchModel"] = searchModel;
        //    if (this.CheckSearchModelIsNull(searchModel))
        //    {
        //        TempData["_AjaxMessage"] = "";

        //    IList<IpDetail> list = base.genericMgr.FindAll<IpDetail>(PrepareSearchDetailStatement(command, searchModel)); //GetPageData<OrderDetail>(searchStatementModel, command);

        //    int value = Convert.ToInt32(base.systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.MaxRowSizeOnPage));
        //    if (list.Count > value)
        //    {
        //        SaveWarningMessage(string.Format("数据超过{0}行", value));
        //    }
        //    return View(list.Take(value));
        //    }
        //    else
        //    {
        //        SaveWarningMessage(Resources.ErrorMessage.Errors_NoConditions);
        //        return View(new List<IpDetail>());
        //    }
        //}


    }
}
