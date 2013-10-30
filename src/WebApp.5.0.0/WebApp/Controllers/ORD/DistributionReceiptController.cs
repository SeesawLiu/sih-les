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

    #endregion

    /// <summary>
    /// This controller response to control the Address.
    /// </summary>
    /// 


    public class DistributionReceiptController : WebAppBaseController
    {

        #region Properties

        public IReceiptMgr receiptMgr { get; set; }

        public IIpMgr ipMgr { get; set; }

        public IReportGen reportGen { get; set; }
        #endregion


        /// <summary>
        /// hql 
        /// </summary>
        private static string selectCountStatement = "select count(*) from ReceiptMaster as r";

        /// <summary>
        /// hql 
        /// </summary>
        private static string selectStatement = "select r from ReceiptMaster as r";

        #region public actions
        /// <summary>
        /// Index action for DistributionGoodsReceipt controller
        /// </summary>
        /// <returns>Index view</returns>
        [SconitAuthorize(Permissions = "Url_DistributionReceipt_View")]
        public ActionResult Index()
        {
            return View();
        }
        [SconitAuthorize(Permissions = "Url_DistributionReceipt_Detail")]
        public ActionResult DetailIndex()
        {
            return View();
        }



        /// <summary>
        /// List action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel"> ReceiptMaster Search model</param>
        /// <returns>return the result view</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionReceipt_View")]
        public ActionResult List(GridCommand command, ReceiptMasterSearchModel searchModel)
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
            ViewBag.OrderSubType = searchModel.OrderSubType;
            return View();
        }


        /// <summary>
        ///  AjaxList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel"> ReceiptMaster Search Model</param>
        /// <returns>return the result action</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionReceipt_View")]
        public ActionResult _AjaxList(GridCommand command, ReceiptMasterSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<ReceiptMaster>()));
            }
            if (searchModel.OrderSubType == (int)com.Sconit.CodeMaster.OrderSubType.Return)
            {
                string centre = searchModel.PartyFrom;
                searchModel.PartyFrom = searchModel.PartyTo;
                searchModel.PartyTo = centre;
            }
            //string whereStatement = string.Empty;
            //if (searchModel.Item != null && searchModel.Item != string.Empty)
            //    whereStatement += " and exists(select 1 from RecDetail as d where d.RecNo = r.RecNo and d.Item = '" + searchModel.Item + "')";
            string whereStatement = string.Empty;
            if (!string.IsNullOrWhiteSpace(searchModel.Item) && !string.IsNullOrWhiteSpace(searchModel.OrderNo))
            {
                whereStatement += " and exists(select 1 from RecDetail as d where d.RecNo = r.RecNo and d.Item = '" + searchModel.Item + "' and d.OrderNo='" + searchModel.OrderNo + "')";
            }
            else if (!string.IsNullOrWhiteSpace(searchModel.Item) && string.IsNullOrWhiteSpace(searchModel.OrderNo))
            {
                whereStatement += " and exists(select 1 from RecDetail as d where d.RecNo = r.RecNo and d.Item = '" + searchModel.Item + "')";
            }
            else if (string.IsNullOrWhiteSpace(searchModel.Item) && !string.IsNullOrWhiteSpace(searchModel.OrderNo))
            {
                whereStatement += " and exists(select 1 from RecDetail as d where d.RecNo = r.RecNo and d.OrderNo='" + searchModel.OrderNo + "')";
            }
            whereStatement += " and r.OrderSubType=" + searchModel.OrderSubType;
            ProcedureSearchStatementModel procedureSearchStatementModel = this.PrepareProcedureSearchStatement(command, searchModel, whereStatement);
            return PartialView(GetAjaxPageDataProcedure<ReceiptMaster>(procedureSearchStatementModel, command));
        }

        #region  明细菜单报表
        [SconitAuthorize(Permissions = "Url_DistributionReceipt_Detail")]
        public ActionResult DetailList(GridCommand command, ReceiptMasterSearchModel searchModel)
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
                return View(new List<ReceiptDetail>());
            }
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxRecDetList(GridCommand command, ReceiptMasterSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<ReceiptDetail>()));
            }
            string whereStatement = " and exists (select 1 from ReceiptMaster  as r where r.OrderSubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal + " and r.RecNo=d.RecNo ) ";
            if (!string.IsNullOrWhiteSpace(searchModel.OrderNo))
            {
                whereStatement += " and d.OrderNo='" + searchModel.OrderNo + "' ";
            }
            ProcedureSearchStatementModel procedureSearchStatementModel = PrepareSearchDetailStatement(command, searchModel, whereStatement);
            IList<ReceiptDetail> receiptDetailList = new List<ReceiptDetail>();
            procedureSearchStatementModel.SelectProcedure = "USP_Search_PrintRecDet";
            GridModel<object[]> gridModel = GetAjaxPageDataProcedure<object[]>(procedureSearchStatementModel, command);
            if (gridModel.Data != null && gridModel.Data.Count() > 0)
            {
                #region
                receiptDetailList = (from tak in gridModel.Data
                                     select new ReceiptDetail
                                     {
                                         Id = (int)tak[0],
                                         ReceiptNo = (string)tak[1],
                                         OrderNo = (string)tak[2],
                                         IpNo = (string)tak[3],
                                         Flow = (string)tak[4],
                                         ExternalOrderNo = (string)tak[5],
                                         ExternalSequence = (string)tak[6],
                                         Item = (string)tak[7],
                                         ReferenceItemCode = (string)tak[8],
                                         ItemDescription = (string)tak[9],
                                         Uom = (string)tak[10],
                                         LocationFrom = (string)tak[11],
                                         LocationTo = (string)tak[12],
                                         ReceivedQty = (decimal)tak[13],
                                         MastPartyFrom = (string)tak[14],
                                         MastPartyTo = (string)tak[15],
                                         MastType = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.OrderType, int.Parse((tak[16]).ToString())),
                                         MastStatus = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.ReceiptStatus, int.Parse((tak[17]).ToString())),
                                         MastCreateDate = (DateTime)tak[18],
                                         SAPLocation = (string)tak[19],

                                     }).ToList();
                #endregion
            }
            procedureSearchStatementModel.PageParameters[2].Parameter = gridModel.Total;
            TempData["ReceiptDetailPrintSearchModel"] = procedureSearchStatementModel;

            GridModel<ReceiptDetail> gridModelOrderDet = new GridModel<ReceiptDetail>();
            gridModelOrderDet.Total = gridModel.Total;
            gridModelOrderDet.Data = receiptDetailList;

            return PartialView(gridModelOrderDet);
        }

        #endregion

        [SconitAuthorize(Permissions = "Url_DistributionReceipt_View")]
        public ActionResult Edit(string receiptNo)
        {
            if (string.IsNullOrEmpty(receiptNo))
            {
                return HttpNotFound();
            }
            else
            {
                ViewBag.ReceiptNo = receiptNo;
                ReceiptMaster rm = base.genericMgr.FindById<ReceiptMaster>(receiptNo);
                return View(rm);
            }
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_DistributionReceipt_View")]
        public ActionResult _ReceiptDetailList(string receiptNo)
        {
            string hql = "select r from ReceiptDetail as r where r.ReceiptNo = ?";
            IList<ReceiptDetail> receiptDetailList = base.genericMgr.FindAll<ReceiptDetail>(hql, receiptNo);
            return PartialView(receiptDetailList);
        }

        [SconitAuthorize(Permissions = "Url_DistributionReceipt_Cancel")]
        public ActionResult Cancel(string id)
        {
            try
            {
                ReceiptMaster receiptMaster = base.genericMgr.FindById<ReceiptMaster>(id);
                //20130101上线，对于安吉或双桥移库执行收货冲销，会同时冲销送货单并更新LesINLog记录的处理结果以及中间文件的处理标志
                //if (ReceiptMaster.PartyFrom == "LOC" || ReceiptMaster.PartyFrom == "SQC")
                //{
                //    receiptMgr.CancelReceipt(ReceiptMaster);
                //    ipMgr.CancelIp(ReceiptMaster.IpNo);

                //    IList<IpDetail> ipDetailList = base.genericMgr.FindAll<IpDetail>("select i from IpDetail as i where IpNo = ?", new object[] { ReceiptMaster.IpNo });//安吉或双桥移库一个送货单只会有一个明细项
                //    string updateLesINLog = "Update FIS_LesINLog set HandResult = 'F',ErrorCause = '收货冲销操作更改处理标志为F' where WMSNo = ?";
                //    base.genericMgr.FindAllWithNativeSql(updateLesINLog, new object[] { ipDetailList[0].ExternalOrderNo });
                //    string updateWMSDatFile = "Update FIS_WMSDatFile set IsHand = 0 where WMSId = ?";
                //    base.genericMgr.FindAllWithNativeSql(updateWMSDatFile, new object[] { ipDetailList[0].ExternalOrderNo });
                //}
                //else
                receiptMgr.CancelVarietyOfReceipt(receiptMaster);

                SaveSuccessMessage("收货单取消成功");
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
            }
            return RedirectToAction("Edit", new { receiptNo = id });
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _ResultHierarchyAjax(string id)
        {
            string hql = "select r from ReceiptLocationDetail as r where r.ReceiptDetailId = ?";
            IList<ReceiptLocationDetail> ReceiptLocationDetail = base.genericMgr.FindAll<ReceiptLocationDetail>(hql, id);
            return PartialView(new GridModel(ReceiptLocationDetail));
        }

        #region 打印导出
        public void SaveToClient(string receiptNo)
        {
            ReceiptMaster receiptMaster = base.genericMgr.FindById<ReceiptMaster>(receiptNo);
            IList<ReceiptDetail> receiptDetail = base.genericMgr.FindAll<ReceiptDetail>("select rd from ReceiptDetail as rd where rd.Receipt=?", receiptNo);
            receiptMaster.ReceiptDetails = receiptDetail;
            PrintReceiptMaster printReceiptMaster = Mapper.Map<ReceiptMaster, PrintReceiptMaster>(receiptMaster);
            IList<object> data = new List<object>();
            data.Add(printReceiptMaster);
            data.Add(printReceiptMaster.ReceiptDetails);
            reportGen.WriteToClient(printReceiptMaster.ReceiptTemplate, data, printReceiptMaster.ReceiptTemplate);

        }

        public string Print(string receiptNo)
        {
            ReceiptMaster receiptMaster = base.genericMgr.FindById<ReceiptMaster>(receiptNo);
            IList<ReceiptDetail> receiptDetail = base.genericMgr.FindAll<ReceiptDetail>("select rd from ReceiptDetail as rd where rd.Receipt=?", receiptNo);
            receiptMaster.ReceiptDetails = receiptDetail;
            PrintReceiptMaster printReceiptMaster = Mapper.Map<ReceiptMaster, PrintReceiptMaster>(receiptMaster);
            IList<object> data = new List<object>();
            data.Add(printReceiptMaster);
            data.Add(printReceiptMaster.ReceiptDetails);
            return reportGen.WriteToFile(printReceiptMaster.ReceiptTemplate, data);
        }


        #region 导出明细
        public void SaveRecDetailViewToClient()
        {
            ProcedureSearchStatementModel procedureSearchStatementModel = TempData["ReceiptDetailPrintSearchModel"] as ProcedureSearchStatementModel;
            TempData["ReceiptDetailPrintSearchModel"] = procedureSearchStatementModel;

            GridCommand command = new GridCommand();
            command.Page = 1;
            command.PageSize = (int)procedureSearchStatementModel.PageParameters[2].Parameter;
            procedureSearchStatementModel.PageParameters[3].Parameter = 1;
            GridModel<object[]> gridModel = GetAjaxPageDataProcedure<object[]>(procedureSearchStatementModel, command);
            IList<ReceiptDetail> receiptDetailList = new List<ReceiptDetail>();
            if (gridModel.Data != null && gridModel.Data.Count() > 0)
            {
                #region
                receiptDetailList = (from tak in gridModel.Data
                                     select new ReceiptDetail
                                     {
                                         Id = (int)tak[0],
                                         ReceiptNo = (string)tak[1],
                                         OrderNo = (string)tak[2],
                                         IpNo = (string)tak[3],
                                         Flow = (string)tak[4],
                                         ExternalOrderNo = (string)tak[5],
                                         ExternalSequence = (string)tak[6],
                                         Item = (string)tak[7],
                                         ReferenceItemCode = (string)tak[8],
                                         ItemDescription = (string)tak[9],
                                         Uom = (string)tak[10],
                                         LocationFrom = (string)tak[11],
                                         LocationTo = (string)tak[12],
                                         ReceivedQty = (decimal)tak[13],
                                         MastPartyFrom = (string)tak[14],
                                         MastPartyTo = (string)tak[15],
                                         MastType = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.OrderType, int.Parse((tak[16]).ToString())),
                                         MastStatus = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.ReceiptStatus, int.Parse((tak[17]).ToString())),
                                         MastCreateDate = (DateTime)tak[18],
                                         SAPLocation = (string)tak[19],

                                     }).ToList();
                #endregion
            }
            IList<object> data = new List<object>();
            data.Add(receiptDetailList);
            reportGen.WriteToClient("ReceiptDetView.xls", data, "ReceiptDetView.xls");
        }
        #endregion
        #endregion

        #endregion


        #region Private
        /// <summary>
        /// Search Statement
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">ReceiptMaster Search Model</param>
        /// <returns>return ReceiptMaster search model</returns>
        private SearchStatementModel PrepareSearchStatement(GridCommand command, ReceiptMasterSearchModel searchModel)
        {
            string whereStatement = " where r.OrderType in (" + (int)com.Sconit.CodeMaster.OrderType.Distribution + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + ")";

            IList<object> param = new List<object>();


            HqlStatementHelper.AddLikeStatement("WMSNo", searchModel.WMSNo, HqlStatementHelper.LikeMatchMode.Start, "r", ref whereStatement, param);

            if (searchModel.OrderSubType.Value == (int)com.Sconit.CodeMaster.OrderSubType.Normal)
            {
                //SecurityHelper.AddPartyFromPermissionStatement(ref whereStatement, "r", "PartyFrom", com.Sconit.CodeMaster.OrderType.Distribution, false);
                //SecurityHelper.AddPartyToPermissionStatement(ref whereStatement, "r", "PartyTo", com.Sconit.CodeMaster.OrderType.Distribution);
                SecurityHelper.AddPartyFromAndPartyToPermissionStatement(ref whereStatement, "r", "OrderType", "r", "PartyFrom", "r", "PartyTo", com.Sconit.CodeMaster.OrderType.Distribution, false);
                HqlStatementHelper.AddEqStatement("PartyFrom", searchModel.PartyFrom, "r", ref whereStatement, param);
                HqlStatementHelper.AddEqStatement("PartyTo", searchModel.PartyTo, "r", ref whereStatement, param);
            }
            else if (searchModel.OrderSubType.Value == (int)com.Sconit.CodeMaster.OrderSubType.Return)
            {
                //SecurityHelper.AddPartyFromPermissionStatement(ref whereStatement, "r", "PartyTo", com.Sconit.CodeMaster.OrderType.Distribution, false);
                //SecurityHelper.AddPartyToPermissionStatement(ref whereStatement, "r", "PartyFrom", com.Sconit.CodeMaster.OrderType.Distribution);
                SecurityHelper.AddPartyFromAndPartyToPermissionStatement(ref whereStatement, "r", "OrderType", "r", "PartyTo", "r", "PartyFrom", com.Sconit.CodeMaster.OrderType.Distribution, false);
                HqlStatementHelper.AddEqStatement("PartyFrom", searchModel.PartyTo, "r", ref whereStatement, param);
                HqlStatementHelper.AddEqStatement("PartyTo", searchModel.PartyFrom, "r", ref whereStatement, param);
            }

            HqlStatementHelper.AddLikeStatement("ReceiptNo", searchModel.ReceiptNo, HqlStatementHelper.LikeMatchMode.Start, "r", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("IpNo", searchModel.IpNo, HqlStatementHelper.LikeMatchMode.Start, "r", ref whereStatement, param);

            HqlStatementHelper.AddLikeStatement("Dock", searchModel.Dock, HqlStatementHelper.LikeMatchMode.Start, "r", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Status", searchModel.Status, "r", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "r", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("OrderSubType", searchModel.OrderSubType, "r", ref whereStatement, param);

            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.StartDate, searchModel.EndDate, "r", ref whereStatement, param);
            }
            else if (searchModel.StartDate != null & searchModel.EndDate == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.StartDate, "r", ref whereStatement, param);
            }
            else if (searchModel.StartDate == null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.EndDate, "r", ref whereStatement, param);
            }

            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "ReceiptMasterStatusDescription")
                {
                    command.SortDescriptors[0].Member = "Status";
                }
            }

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by r.CreateDate desc";
            }
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        private string PrepareSearchDetailStatement(GridCommand command, ReceiptMasterSearchModel searchModel)
        {
            StringBuilder Sb = new StringBuilder();
            string whereStatement = " select  d from ReceiptDetail as d  where exists (select 1 from ReceiptMaster  as o where "
                                    + " o.OrderType in (" + (int)com.Sconit.CodeMaster.OrderType.Distribution + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + ")"
                                    + " and  o.OrderSubType = " + (int)com.Sconit.CodeMaster.OrderSubType.Normal + " and o.ReceiptNo=d.ReceiptNo ";

            Sb.Append(whereStatement);

            if (searchModel.Status != null)
            {
                Sb.Append(string.Format(" and o.Status = '{0}'", searchModel.Status));
            }
            if (searchModel.IpNo != null)
            {
                Sb.Append(string.Format(" and o.IpNo = '{0}'", searchModel.IpNo));
            }
            if (!string.IsNullOrEmpty(searchModel.ReceiptNo))
            {
                Sb.Append(string.Format(" and o.ReceiptNo like '{0}%'", searchModel.ReceiptNo));
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
            //SecurityHelper.AddPartyFromPermissionStatement(ref str, "o", "PartyFrom", com.Sconit.CodeMaster.OrderType.Distribution, true);
            SecurityHelper.AddPartyFromAndPartyToPermissionStatement(ref str, "o", "Type", "o", "PartyFrom", "o", "PartyTo", com.Sconit.CodeMaster.OrderType.Distribution, true);

            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                Sb.Append(string.Format(" and o.CreateDate between '{0}' and '{1}'", searchModel.StartDate, searchModel.EndDate));
                // HqlStatementHelper.AddBetweenStatement("StartTime", searchModel.DateFrom, searchModel.DateTo, "o", ref whereStatement, param);
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
                Sb.Append(string.Format(" and  o.WMSNo like '{0}%'", searchModel.WMSNo));
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
            if (!string.IsNullOrEmpty(searchModel.ManufactureParty))
            {
                Sb.Append(string.Format(" and  d.ManufactureParty like '{0}%'", searchModel.ManufactureParty));

            }

            return Sb.ToString();
        }

        private ProcedureSearchStatementModel PrepareProcedureSearchStatement(GridCommand command, ReceiptMasterSearchModel searchModel, string whereStatement)
        {
            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.ReceiptNo, Type = NHibernate.NHibernateUtil.String });
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
            paraList.Add(new ProcedureParameter { Parameter = searchModel.WMSNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.ManufactureParty, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Flow, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = false, Type = NHibernate.NHibernateUtil.Boolean });
            paraList.Add(new ProcedureParameter { Parameter = CurrentUser.Id, Type = NHibernate.NHibernateUtil.Int32 });
            paraList.Add(new ProcedureParameter { Parameter = whereStatement, Type = NHibernate.NHibernateUtil.String });


            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "ReceiptMasterStatusDescription")
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
            procedureSearchStatementModel.CountProcedure = "USP_Search_RecMstrCount";
            procedureSearchStatementModel.SelectProcedure = "USP_Search_RecMstr";

            return procedureSearchStatementModel;
        }

        private ProcedureSearchStatementModel PrepareSearchDetailStatement(GridCommand command, ReceiptMasterSearchModel searchModel, string whereStatement)
        {

            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.ReceiptNo, Type = NHibernate.NHibernateUtil.String });
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
            paraList.Add(new ProcedureParameter { Parameter = searchModel.WMSNo, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.ManufactureParty, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.Flow, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = false, Type = NHibernate.NHibernateUtil.Int64 });
            paraList.Add(new ProcedureParameter { Parameter = CurrentUser.Id, Type = NHibernate.NHibernateUtil.Int32 });
            paraList.Add(new ProcedureParameter { Parameter = whereStatement, Type = NHibernate.NHibernateUtil.String });


            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "StatusDescription")
                {
                    command.SortDescriptors[0].Member = "Status";
                }
                if (command.SortDescriptors[0].Member == "ReceiptNo")
                {
                    command.SortDescriptors[0].Member = "RecNo";
                }
                if (command.SortDescriptors[0].Member == "ItemDescription")
                {
                    command.SortDescriptors[0].Member = "ItemDesc";
                }
                if (command.SortDescriptors[0].Member == "ReceivedQty")
                {
                    command.SortDescriptors[0].Member = "RecQty";
                }
                if (command.SortDescriptors[0].Member == "LocationTo")
                {
                    command.SortDescriptors[0].Member = "LocTo";
                }
                if (command.SortDescriptors[0].Member == "MastType")
                {
                    command.SortDescriptors[0].Member = "MastOrderType";
                }
            }
            pageParaList.Add(new ProcedureParameter { Parameter = command.SortDescriptors.Count > 0 ? command.SortDescriptors[0].Member : null, Type = NHibernate.NHibernateUtil.String });
            pageParaList.Add(new ProcedureParameter { Parameter = command.SortDescriptors.Count > 0 ? (command.SortDescriptors[0].SortDirection == ListSortDirection.Descending ? "desc" : "asc") : "asc", Type = NHibernate.NHibernateUtil.String });
            pageParaList.Add(new ProcedureParameter { Parameter = command.PageSize, Type = NHibernate.NHibernateUtil.Int32 });
            pageParaList.Add(new ProcedureParameter { Parameter = command.Page, Type = NHibernate.NHibernateUtil.Int32 });

            var procedureSearchStatementModel = new ProcedureSearchStatementModel();
            procedureSearchStatementModel.Parameters = paraList;
            procedureSearchStatementModel.PageParameters = pageParaList;
            procedureSearchStatementModel.CountProcedure = "USP_Search_RecDetCount";
            procedureSearchStatementModel.SelectProcedure = "USP_Search_RecDet";

            return procedureSearchStatementModel;
        }

        #endregion
    }
}
