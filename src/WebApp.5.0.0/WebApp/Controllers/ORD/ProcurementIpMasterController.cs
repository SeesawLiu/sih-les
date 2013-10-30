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
    using System.Text;
    using System.ComponentModel;
    using com.Sconit.Entity.FIS;
    using com.Sconit.Entity.MD;
    using com.Sconit.Entity;
    //  using com.Sconit.Service.FISService;
    #endregion

    /// <summary>
    /// This controller response to control the ProcurementOrderIssue.
    /// </summary>
    public class ProcurementIpMasterController : WebAppBaseController
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


        private static string selectIpDetailCountStatement = "select count(*) from IpDetail as i";
        /// <summary>
        /// hql 
        /// </summary>
        private static string selectStatement = "select i from IpMaster as i";

        private static string selectIpDetailStatement = "select i from IpDetail as i";

        private static string selectReceiveIpDetailStatement = "select i from IpDetail as i where i.IsClose = ? and i.Type = ? and i.IpNo = ?";


        #region public actions

        #region view
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_View")]
        public ActionResult Index()
        {
            return View();
        }


        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_Detail")]
        public ActionResult DetailIndex()
        {
            return View();
        }
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_View")]
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
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_View")]
        public ActionResult _AjaxList(GridCommand command, IpMasterSearchModel searchModel)
        {

            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<IpMaster>()));
            }
            //string whereStatement = "where  i.OrderType in ("
            //                        + (int)com.Sconit.CodeMaster.OrderType.Procurement + "," + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.ScheduleLine + ","
            //                        + (int)com.Sconit.CodeMaster.OrderType.SubContract + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + ")";

            string whereStatement = string.Empty;
            if (searchModel.Item != null && searchModel.Item != string.Empty)
                whereStatement += " and exists(select 1 from IpDetail as d where d.IpNo = i.IpNo and d.Item = '" + searchModel.Item + "')";
            ProcedureSearchStatementModel procedureSearchStatementModel = this.PrepareProcedureSearchStatement(command, searchModel, whereStatement);
            return PartialView(GetAjaxPageDataProcedure<IpMaster>(procedureSearchStatementModel, command));

        }

        #region  明细菜单 明细报表查询
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_Detail")]
        public ActionResult DetailList(GridCommand command, IpMasterSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
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
            #region
            //ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchDetailStatement(command, searchModel, string.Empty);
            //GridModel<IpDetail> gridList = GetAjaxPageDataProcedure<IpDetail>(procedureSearchStatementModel, command);
            //int i = 0;
            //foreach (IpDetail ipDetail in gridList.Data)
            //{
            //    if (i > command.PageSize)
            //    {
            //        break;
            //    }
            //    ipDetail.SAPLocationTo = base.genericMgr.FindById<Location>(ipDetail.LocationTo).SAPLocation;

            //}
            //gridList.Data = gridList.Data.Where(o => o.Type == com.Sconit.CodeMaster.IpDetailType.Normal);
            //return PartialView(gridList);
            #endregion
            IList<IpDetail> ipDetList = new List<IpDetail>();
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchDetailStatement(command, searchModel, string.Empty);
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

        #region Edit 页面明细
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_View")]
        public ActionResult _IpDetailList(GridCommand command, IpDetailSearchModel searchModel, string ipNo)
        {
            IpMaster ipMaster = base.genericMgr.FindById<IpMaster>(ipNo);
            if ((ipMaster.Status == com.Sconit.CodeMaster.IpStatus.Submit || ipMaster.Status == com.Sconit.CodeMaster.IpStatus.InProcess)
               // && ipMaster.IsReceiveScanHu == false
                && CurrentUser.Permissions.Where(p => p.PermissionCode == "Url_ProcurementIpMaster_UpdateLocTo").Count() > 0)
            {
                ViewBag.IsHassUpdate = true;
            }
            else
            {
                ViewBag.IsHassUpdate = false;
            }

            ViewBag.IsCancel = ((ipMaster.Status == com.Sconit.CodeMaster.IpStatus.Cancel || ipMaster.Status == com.Sconit.CodeMaster.IpStatus.Close) ? false : true)
                && ipMaster.OrderType == com.Sconit.CodeMaster.OrderType.ScheduleLine && !ipMaster.IsAsnUniqueReceive;
            searchModel.IpNo = ipNo;
            TempData["IpDetailSearchModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_View")]
        public ActionResult _AjaxIpDetailList(GridCommand command, IpDetailSearchModel searchModel, string ipNo)
        {
            SearchStatementModel searchStatementModel = this.IpDetailPrepareSearchStatement(command, searchModel, ipNo);
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
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_View")]
        public ActionResult Edit(string ipNo)
        {
            if (string.IsNullOrEmpty(ipNo))
            {
                return HttpNotFound();
            }
            
            IpMaster ipMaster = base.genericMgr.FindById<IpMaster>(ipNo);
            if ((ipMaster.Status == com.Sconit.CodeMaster.IpStatus.Submit || ipMaster.Status == com.Sconit.CodeMaster.IpStatus.InProcess)
                && ipMaster.IsReceiveScanHu == false
                && CurrentUser.Permissions.Where(p => p.PermissionCode == "Url_ProcurementIpMaster_UpdateLocTo").Count() > 0)
            {
                ViewBag.IsHassUpdate = true;
            }
            else
            {
                ViewBag.IsHassUpdate = false;
            }
            return View(ipMaster);
        }

        #endregion

        #region receive
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_Receive")]
        public ActionResult ReceiveIndex()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_Receive")]
        public ActionResult ReceiveList(GridCommand command, IpMasterSearchModel searchModel)
        {
            this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_Receive")]
        public ActionResult _ReceiveAjaxList(GridCommand command, IpMasterSearchModel searchModel)
        {
            string whereStatement = " and i.IsRecScanHu=0 and i.Status in(" + (int)com.Sconit.CodeMaster.IpStatus.Submit + "," + (int)com.Sconit.CodeMaster.IpStatus.InProcess + ") and exists (select 1 from IpDetail as d where d.IsClose = 0 and d.Type = 0 and d.IpNo = i.IpNo)";

            ProcedureSearchStatementModel procedureSearchStatementModel = this.PrepareProcedureSearchStatement(command, searchModel, whereStatement);
            return PartialView(GetAjaxPageDataProcedure<IpMaster>(procedureSearchStatementModel, command));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_Receive")]
        public ActionResult _ReceiveIpDetailList(string ipNo)
        {
            //IList<IpDetail> ipDetailList = base.genericMgr.FindAll<IpDetail>(selectReceiveIpDetailStatement, new object[] { false, (int)com.Sconit.CodeMaster.IpDetailType.Normal, ipNo });
            ViewBag.ipNo = ipNo;
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_Receive")]
        public ActionResult _AjaxReceiveIpDetailList(string ipNo)
        {
            IList<IpDetail> ipDetailList = base.genericMgr.FindAll<IpDetail>(selectReceiveIpDetailStatement, new object[] { false, (int)com.Sconit.CodeMaster.IpDetailType.Normal, ipNo });
            ViewBag.ipNo = ipNo;
            //如果asn一次收货，则收货后会关闭
            //ipDetailList = ipDetailList.Where(o => o.ReceivedQty == 0).ToList();
            ipDetailList = ipDetailList.Where(o => o.ReceivedQty < o.Qty).ToList();
            return PartialView(new GridModel(ipDetailList));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_Receive")]
        public ActionResult ReceiveEdit(string ipNo)
        {
            if (string.IsNullOrEmpty(ipNo))
            {
                return HttpNotFound();
            }

            IpMaster ipMaster = base.genericMgr.FindById<IpMaster>(ipNo);
            return View(ipMaster);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_Receive")]
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

                orderMgr.ReceiveIp(ipDetailList, false, DateTime.Now);
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

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        public ActionResult _SaveBatchEditing(
            [Bind(Prefix = "updated")]IEnumerable<IpDetail> updatedIpDetails,string ipNo)
        {
            try
            {
                if (updatedIpDetails != null && updatedIpDetails.Count() > 0)
                {
                    //orderMgr.BatchSeqOrderChange((IList<IpDetail>)updatedOrderDetails, 2);
                    ipMgr.BatchUpIpDetLocationTo((IList<IpDetail>)updatedIpDetails);
                }
                else
                {
                    throw new BusinessException("请选择要修改的明细。");
                }

                SaveSuccessMessage("修改成功。");
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }
            IpMaster ipMaster = base.genericMgr.FindById<IpMaster>(ipNo);
            if ((ipMaster.Status == com.Sconit.CodeMaster.IpStatus.Submit || ipMaster.Status == com.Sconit.CodeMaster.IpStatus.InProcess)
               && ipMaster.IsReceiveScanHu == false
               && CurrentUser.Permissions.Where(p => p.PermissionCode == "Url_ProcurementIpMaster_UpdateLocTo").Count() > 0)
            {
                ViewBag.IsHassUpdate = true;
            }
            else
            {
                ViewBag.IsHassUpdate = false;
            }
            return View("Edit", ipMaster);
        }

        #endregion

        #region WmsReceive
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_ReceiveWMS")]
        public ActionResult ReceiveWMSIndex()
        {
            return View();
        }




        [GridAction]
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_ReceiveWMS")]
        public ActionResult _ReceiveWMSIpDetailList(GridCommand command, string wmsNo)
        {
            ViewBag.WmsNo = wmsNo;
            if (string.IsNullOrWhiteSpace(wmsNo))
            {
                SaveWarningMessage("安吉拣货单号不能为空。");
            }
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxReceiveWMSIpDetailList(GridCommand command, string wmsNo)
        {
            if (string.IsNullOrWhiteSpace(wmsNo))
            {
                return PartialView(new GridModel<WMSDatFile>(new List<WMSDatFile>()));
            }

            //IList<WMSDatFile> wMSDatFileList = base.genericMgr.FindAll<WMSDatFile>("select w from WMSDatFile as w where w.WmsNo=? and w.ReceiveTotal>(CancelQty+Qty)", WmsNo);
            IList<WMSDatFile> wMSDatFileList = this.genericMgr.FindEntityWithNativeSql<WMSDatFile>(" select * from FIS_WMSDatFile where WmsNo=? and ReceiveTotal<(CancelQty+Qty) ", wmsNo);

            #region 冲销的相互抵消
            foreach (WMSDatFile wMSDatFile in wMSDatFileList)
            {
                if (wMSDatFile.MoveType == null)
                {
                    continue;
                }
                foreach (WMSDatFile wmsFile in wMSDatFileList)
                {
                    if (wmsFile.MoveType == null)
                    {
                        continue;
                    }
                    if (wMSDatFile.MoveType + wMSDatFile.SOBKZ == "311" && wmsFile.MoveType + wmsFile.SOBKZ == "312" && wmsFile.Qty == wMSDatFile.Qty && wmsFile.WmsLine == wMSDatFile.WmsLine)
                    {
                        wmsFile.MoveType = null;
                        wMSDatFile.MoveType = null;
                        break;
                    }
                    else if (wMSDatFile.MoveType + wMSDatFile.SOBKZ == "311K" && wmsFile.MoveType + wmsFile.SOBKZ == "312K" && wmsFile.Qty == wMSDatFile.Qty && wmsFile.WmsLine == wMSDatFile.WmsLine)
                    {
                        wmsFile.MoveType = null;
                        wMSDatFile.MoveType = null;
                        break;
                    }

                    else if (wMSDatFile.MoveType + wMSDatFile.SOBKZ == "411" && wmsFile.MoveType + wMSDatFile.SOBKZ == "412" && wmsFile.Qty == wMSDatFile.Qty && wmsFile.WmsLine == wMSDatFile.WmsLine)
                    {
                        wmsFile.MoveType = null;
                        wMSDatFile.MoveType = null;
                        break;
                    }
                    else if (wMSDatFile.MoveType + wMSDatFile.SOBKZ == "411K" && wmsFile.MoveType + wMSDatFile.SOBKZ == "412K" && wmsFile.Qty == wMSDatFile.Qty && wmsFile.WmsLine == wMSDatFile.WmsLine)
                    {
                        wmsFile.MoveType = null;
                        wMSDatFile.MoveType = null;
                        break;
                    }
                }
            }
            #endregion

            #region 相互冲销的不管
            //foreach (WMSDatFile wMSDatFile in wMSDatFileList)
            //{
            //    //if (wMSDatFile.MoveType == "312" || wMSDatFile.MoveType == "412" || wMSDatFile.MoveType == "412k" || wMSDatFile.MoveType == null)
            //    if (wMSDatFile.MoveType == null)
            //    {
            //        #region 将冲销掉的改成已经处理 记录Log
            //        LesINLog lesExistenceLog = base.genericMgr.FindAll<LesINLog>("select l from LesINLog as l where l.WMSNo=?", wMSDatFile.WMSId).FirstOrDefault();

            //        #region 已经处理成功 重新发送Log
            //        if (lesExistenceLog != null)
            //        {

            //            lesExistenceLog.IsCreateDat = false;
            //            lesExistenceLog.HandResult = "S";
            //            base.genericMgr.Update(lesExistenceLog);
            //            base.genericMgr.Update("update WMSDatFile set IsHand=1 where Id=" + wMSDatFile.Id);
            //            continue;

            //        }
            //        #endregion

            //        #region 记录Log 改成已经处理
            //        LesINLog lesInLog = new LesINLog();
            //        lesInLog.Type = "MB1B";
            //        if (wMSDatFile.MoveType == null)
            //        {
            //            lesInLog.MoveType = base.genericMgr.FindById<WMSDatFile>(wMSDatFile.Id).MoveType + wMSDatFile.SOBKZ;
            //        }
            //        else
            //        {
            //            lesInLog.MoveType = wMSDatFile.MoveType + wMSDatFile.SOBKZ;
            //        }
            //        lesInLog.Sequense = "";
            //        // lesInLog.PO = (string)line[3];//
            //        //lesInLog.POLine = (string)line[4];//
            //        lesInLog.WMSNo = wMSDatFile.WMSId;
            //        lesInLog.WMSLine = wMSDatFile.WmsLine;
            //        lesInLog.Item = wMSDatFile.Item;
            //        lesInLog.HandResult = "S";
            //        lesInLog.FileName = wMSDatFile.FileName;
            //        lesInLog.HandTime = System.DateTime.Now.ToString("yyMMddHHmmss");
            //        lesInLog.IsCreateDat = false;
            //        lesInLog.ASNNo = wMSDatFile.WmsNo;
            //        lesInLog.ExtNo = wMSDatFile.WmsNo;
            //        wMSDatFile.MoveType = null;
            //        base.genericMgr.Create(lesInLog);
            //        base.genericMgr.Update("update WMSDatFile set IsHand=1 where Id=" + wMSDatFile.Id);
            //        #endregion
            //        #endregion
            //    }
            //}
            #endregion

            //默认按LES订单id号排序
            IEnumerable<WMSDatFile> wmsList = wMSDatFileList.Where(o => o.MoveType != null && o.MoveType != "312" && o.MoveType != "412").OrderBy(o => o.WmsLine);
            foreach (WMSDatFile wmsDatFile in wmsList)
            {
                //Item item = base.genericMgr.FindById<Item>(wmsDatFile.Item);
                //wmsDatFile.ReferenceItemCode = item.ReferenceCode;
                //wmsDatFile.ItemDescription = item.Description;

                OrderDetail ordDet = base.genericMgr.FindById<OrderDetail>(int.Parse(wmsDatFile.WmsLine));
                wmsDatFile.OrderNo = ordDet.OrderNo;
                wmsDatFile.OrderQty = ordDet.OrderedQty;
                wmsDatFile.ReferenceItemCode = ordDet.ReferenceItemCode;
                wmsDatFile.ItemDescription = ordDet.ItemDescription;
                wmsDatFile.LocationTo = ordDet.LocationTo;
                //wmsDatFile.ReceivedQty = ordDet.ReceivedQty;
            }
            GridModel<WMSDatFile> returnGrid = new GridModel<WMSDatFile>();
            returnGrid.Total = wmsList.Count();
            returnGrid.Data = wmsList.Skip((command.Page - 1) * command.PageSize).Take(command.PageSize);
            return PartialView(returnGrid);

        }

        public JsonResult ReceiveWMSIpMaster(string idStr, string qtyStr)
        {
            try
            {
                if (!string.IsNullOrEmpty(idStr))
                {
                    string[] idArray = idStr.Split(',');
                    string[] qtyArray = qtyStr.Split(',');
                    for (int i = 0; i < idArray.Count(); i++)
                    {
                        try
                        {
                            WMSDatFile wMSDatFile = base.genericMgr.FindById<WMSDatFile>(Convert.ToInt32(idArray[i]));
                            wMSDatFile.CurrentReceiveQty = Convert.ToDecimal(qtyArray[i]);
                            if (wMSDatFile.CurrentReceiveQty + wMSDatFile.ReceiveTotal > (wMSDatFile.Qty + wMSDatFile.CancelQty))
                            {
                                throw new BusinessException(string.Format("物料{0}唯一标识{1}本次收货数{2}大于最大收货数{3}。", wMSDatFile.Item, wMSDatFile.WMSId, wMSDatFile.CurrentReceiveQty, wMSDatFile.Qty + wMSDatFile.CancelQty - wMSDatFile.ReceiveTotal));
                            }
                            orderMgr.ReceiveWMSIpMaster(wMSDatFile);
                            SaveSuccessMessage(string.Format("物料{0}唯一标识{1}收货数{2}收货成功。", wMSDatFile.Item, wMSDatFile.WMSId, wMSDatFile.CurrentReceiveQty));
                        }
                        catch (BusinessException ex)
                        {
                            SaveBusinessExceptionMessage(ex);
                        }
                        catch (Exception ex)
                        {
                            SaveErrorMessage(ex);
                        }
                    }
                }
                else
                {
                    throw new BusinessException("收货明细不能为空。");
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
            return Json(new {  });
            #region
            //IList<LesINLog> lesINLogList = new List<LesINLog>();
            //try
            //{

            //    if (!string.IsNullOrEmpty(idStr))
            //    {
            //        string[] idArray = idStr.Split(',');
            //        string[] qtyArray = qtyStr.Split(',');
            //        string updateSql = "update WMSDatFile set IsHand=1 where WMSId in(";
            //        IList<string> updatePram = new List<string>();
            //        for (int i = 0; i < idArray.Count(); i++)
            //        {
            //            LesINLog lesInLog = new LesINLog();
            //            try
            //            {
            //                decimal recQty=Convert.ToDecimal(qtyArray[i]);
            //                if (recQty > 0)
            //                {
            //                    WMSDatFile wMSDatFile = base.genericMgr.FindById<WMSDatFile>(Convert.ToInt32(idArray[i]));
            //                    if (recQty+wMSDatFile.ReceiveTotal >= (wMSDatFile.Qty + wMSDatFile.CancelQty))
            //                    { 
            //                        throw new BusinessException(string.Format("物料{0}唯一标识{1}本次收货数{2}大于最大收货数{3}。",wMSDatFile.Item,wMSDatFile.WMSId,recQty,wMSDatFile.Qty + wMSDatFile.CancelQty-wMSDatFile.ReceiveTotal));
            //                    }
            //                    //LesINLog lesExistenceLog = base.genericMgr.FindAll<LesINLog>("select l from LesINLog as l where l.WMSNo=? ", wMSDatFile.WMSId).SingleOrDefault();
            //                    #region 已经成功处理过
            //                    //if (lesExistenceLog != null && lesExistenceLog.HandResult == "S")
            //                    //{
            //                    //    lesExistenceLog.IsCreateDat = false;
            //                    //    base.genericMgr.Update(lesExistenceLog);
            //                    //    base.genericMgr.Update("update WMSDatFile set IsHand=1 where Id=" + wMSDatFile.Id);
            //                    //    continue;
            //                    //}
            //                    #endregion

            //                    #region 获得orderdetail
            //                    IList<OrderDetail> orderDetailList = new List<OrderDetail>();
            //                    OrderDetail orderDetail = base.genericMgr.FindById<OrderDetail>(Convert.ToInt32(wMSDatFile.WmsLine));
            //                    orderDetail.WmsFileID = wMSDatFile.WMSId;
            //                    orderDetail.ManufactureParty = wMSDatFile.LIFNR;
            //                    orderDetail.ExternalOrderNo = wMSDatFile.WMSId;
            //                    orderDetail.ExternalSequence = wMSDatFile.WBS;//项目代码
            //                    OrderDetailInput orderDetailInput = new OrderDetailInput();
            //                    orderDetailInput.ShipQty = recQty;
            //                    orderDetailInput.WMSIpNo = wMSDatFile.WmsNo;//WMSNo
            //                    orderDetailInput.WMSIpSeq = wMSDatFile.WMSId;//WMS行
            //                    orderDetailInput.MoveType = wMSDatFile.MoveType + wMSDatFile.SOBKZ;//移动类型
            //                    orderDetailInput.ManufactureParty = wMSDatFile.LIFNR;//厂商代码
            //                    orderDetail.AddOrderDetailInput(orderDetailInput);
            //                    orderDetailList.Add(orderDetail);
            //                    #endregion

            //                    #region 新建Log记录
            //                    lesInLog.Type = "MB1B";
            //                    lesInLog.MoveType = wMSDatFile.MoveType + wMSDatFile.SOBKZ;
            //                    lesInLog.Sequense = "";
            //                    lesInLog.WMSNo = wMSDatFile.WMSId;
            //                    lesInLog.WMSLine = wMSDatFile.WmsLine;
            //                    lesInLog.Item = wMSDatFile.Item;
            //                    lesInLog.HandResult = "S";
            //                    lesInLog.FileName = wMSDatFile.FileName;
            //                    lesInLog.HandTime = System.DateTime.Now.ToString("yyMMddHHmmss");
            //                    lesInLog.IsCreateDat = false;
            //                    lesInLog.ASNNo = wMSDatFile.WmsNo;
            //                    #endregion

            //                    #region 拼成修改中间表Sql
            //                    updateSql += "?,";
            //                    updatePram.Add(wMSDatFile.WMSId.ToString());
            //                    #endregion

            //                    #region 调用后台方法 发货
            //                    //var ipMstr = orderMgr.ShipOrder(orderDetailList);

            //                    //var ipDetList = base.genericMgr.FindAll<IpDetail>("from IpDetail as d where d.IpNo=?", ipMstr.IpNo);
            //                    //if (ipDetList != null && ipDetList.Count > 0)
            //                    //{
            //                    //    lesInLog.Qty = ipDetList.FirstOrDefault().ReceivedQty;
            //                    //    lesInLog.QtyMark = true;
            //                    //}
            //                    lesInLog.Qty = recQty;
            //                    lesInLog.QtyMark = true;
            //                    base.genericMgr.FlushSession();
            //                    #endregion
            //                }

            //            }
            //            catch (BusinessException ex)
            //            {
            //                SaveErrorMessage( ex.GetMessages()[0].GetMessageString());
            //            }
            //            catch (Exception ex)
            //            {
            //                SaveErrorMessage(ex.Message);
            //            }
            //            //lesINLogList.Add(lesInLog);
            //        }
            //        if (updatePram != null && updatePram.Count > 0)
            //        {
            //            updateSql = updateSql.Substring(0, updateSql.Length - 1) + ")";
            //            base.genericMgr.Update(updateSql, updatePram.ToArray());
            //        }
            //    }
            //    else
            //    {
            //        throw new BusinessException("收货明细不能为空。");
            //    }
            //    BusinessException businessException = new BusinessException();
            //    #region Log
            //    foreach (var lesInLog in lesINLogList)
            //    {
            //        LesINLog lesExistenceLog = base.genericMgr.FindAll<LesINLog>("select l from LesINLog as l where l.WMSNo=? ", lesInLog.WMSNo).SingleOrDefault();
            //        #region 添加错误信息 显示到前台
            //        if (lesInLog.ErrorCause != null && lesInLog.ErrorCause != string.Empty)
            //        {
            //            businessException.AddMessage(lesInLog.ErrorCause);
            //        }
            //        #endregion

            //        #region 将Log记录到数据库
            //        if (lesExistenceLog != null)
            //        {
            //            lesExistenceLog.ErrorCause = lesInLog.ErrorCause;
            //            lesExistenceLog.IsCreateDat = false;
            //            lesExistenceLog.HandResult = lesInLog.HandResult;
            //            lesExistenceLog.HandTime = lesInLog.HandTime;
            //            lesExistenceLog.FileName = lesInLog.FileName;
            //            lesExistenceLog.Qty = lesInLog.Qty;
            //            lesExistenceLog.QtyMark = lesInLog.QtyMark;
            //            base.genericMgr.Update(lesExistenceLog);
            //            continue;
            //        }
            //        base.genericMgr.Create(lesInLog);
            //        #endregion
            //    }
            //    #endregion
            //    if (businessException.HasMessage)
            //    {
            //        throw businessException;
            //    }
            //    SaveSuccessMessage("收货成功。");
            //    return Json(new { });
            //}
            //catch (BusinessException ex)
            //{
            //    SaveBusinessExceptionMessage(ex);
            //}
            //catch (Exception ex)
            //{
            //    SaveErrorMessage(ex);
            //}
            //return Json(null);
            #endregion
        }

        private string CheckedMinus(string moveType, decimal qty, string wmsLine, List<WMSDatFile> wMSDatFileList)
        {
            // string type = "MinusMoveType";
            foreach (WMSDatFile wMSDatFile in wMSDatFileList)
            {
                if (wMSDatFile.MoveType == "311" && moveType == "312" && qty == wMSDatFile.Qty && wmsLine == wMSDatFile.WmsLine)
                {
                    wMSDatFile.MoveType = null;
                    return null;
                }
                else if (wMSDatFile.MoveType == "411" && moveType == "412" && qty == wMSDatFile.Qty && wmsLine == wMSDatFile.WmsLine)
                {
                    wMSDatFile.MoveType = null;
                    return null;
                }
                else if (wMSDatFile.MoveType == "411K" && moveType == "412K" && qty == wMSDatFile.Qty && wmsLine == wMSDatFile.WmsLine)
                {
                    wMSDatFile.MoveType = null;
                    return null;
                }
            }
            return moveType;
        }

        private string GetMoveType(string moveType, IList<CodeDetail> codeDetailList)
        {
            // string type = "MinusMoveType";
            foreach (CodeDetail codeDetail in codeDetailList)
            {
                if (codeDetail.Value == moveType)
                {
                    return "PlusMoveType";
                }
            }
            return "MinusMoveType";
        }

        private IList<OrderDetail> GeGroupDetailList(IList<OrderDetail> orderDetailList)
        {
            foreach (OrderDetail det in orderDetailList)
            {
                foreach (OrderDetail orderDetail in orderDetailList.Where(o => o.WmsFileID != det.WmsFileID && o.OrderDetailInputs.First().ShipQty != 0 && o.LocationTo == det.LocationTo))
                {
                    if (det.Id == orderDetail.Id && det.OrderDetailInputs.First().MoveType == orderDetail.OrderDetailInputs.First().MoveType && orderDetail.LocationTo == det.LocationTo)
                    {

                        det.OrderDetailInputs.First().ShipQty += orderDetail.OrderDetailInputs.First().ShipQty;
                        orderDetail.OrderDetailInputs.First().ShipQty = 0;
                    }
                }
            }
            return orderDetailList;

        }
        #endregion

        #region Close
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_Close")]
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
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_Cancel")]
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

        #region ReCreateDat
        public ActionResult ReCreateDat(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return HttpNotFound();
                }
                orderMgr.ReCreateDat(id);
                SaveSuccessMessage("Dat文件创建成功。");
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex.Message);
            }
            return RedirectToAction("Edit", new { ipNo = id });
        }
        #endregion

        #region CancelIpDetail
        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_Cancel")]
        public JsonResult CancelIpDetail(string Id)
        {
            try
            {
                IpDetail ipDet = base.genericMgr.FindById<IpDetail>(int.Parse(Id));
                if (ipMgr.TryCloseExpiredScheduleLineIpDetail(ipDet))
                {
                    object obj = new { SuccessMessage = "取消成功。", IpNo = ipDet.IpNo };
                    return Json(obj);
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

        [SconitAuthorize(Permissions = "Url_ProcurementIpMaster_Cancel")]
        public JsonResult ResumeIpDetail(string Id)
        {
            try
            {
                IpDetail ipDet = base.genericMgr.FindById<IpDetail>(int.Parse(Id));
                if (ipMgr.TryResumeClosedScheduleLineIpDetail(ipDet))
                {
                    SaveSuccessMessage("恢复成功。");
                    return Json(new { IpNo = ipDet.IpNo });
                }
                else
                {
                    throw new BusinessException("恢复失败。");
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
            ipMaster.IpDetails = ipDetails.Where(i => string.IsNullOrEmpty(i.GapReceiptNo)).ToList();
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

        #region private method
        private SearchStatementModel PrepareSearchStatement(GridCommand command, IpMasterSearchModel searchModel, string whereStatement)
        {

            IList<object> param = new List<object>();

            //SecurityHelper.AddPartyFromPermissionStatement(ref whereStatement, "i", "PartyFrom", com.Sconit.CodeMaster.OrderType.Procurement, false);
            //SecurityHelper.AddPartyToPermissionStatement(ref whereStatement, "i", "PartyTo", com.Sconit.CodeMaster.OrderType.Procurement);
            SecurityHelper.AddPartyFromAndPartyToPermissionStatement(ref whereStatement, "i", "OrderType", "i", "PartyFrom", "i", "PartyTo", com.Sconit.CodeMaster.OrderType.Procurement, false);

            HqlStatementHelper.AddLikeStatement("WMSNo", searchModel.WMSNo, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("IpNo", searchModel.IpNo, HqlStatementHelper.LikeMatchMode.Start, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Status", searchModel.Status, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("PartyFrom", searchModel.PartyFrom, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("PartyTo", searchModel.PartyTo, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("OrderType", searchModel.IpOrderType, "i", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "i", ref whereStatement, param);

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
                Parameter =  (int)com.Sconit.CodeMaster.OrderType.Procurement
                    + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + ","  + (int)com.Sconit.CodeMaster.OrderType.ScheduleLine,
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


        private ProcedureSearchStatementModel PrepareSearchDetailStatement(GridCommand command, IpMasterSearchModel searchModel, string whereStatement)
        {
            whereStatement = "and exists(select 1 from IpMaster  as i where i.IpNo=d.IpNo)";
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
                Parameter = (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.Procurement
                                  + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer
                                     + "," + (int)com.Sconit.CodeMaster.OrderType.ScheduleLine,
                // + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer
                //  + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract
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






    }
}
