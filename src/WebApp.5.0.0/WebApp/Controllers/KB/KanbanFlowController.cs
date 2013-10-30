using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;
using com.Sconit.Entity.PRD;
using com.Sconit.Entity.SCM;
using com.Sconit.Entity.VIEW;
using com.Sconit.Service;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels.SCM;
using com.Sconit.Web.Util;
using Telerik.Web.Mvc;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.BIL;
using com.Sconit.Entity.SYS;
using com.Sconit.Utility;
using com.Sconit.Entity.Exception;
using System.Web;
using com.Sconit.Utility.Report;
using NHibernate;
using NHibernate.Type;
using com.Sconit.Entity.KB;
using System.Text;

namespace com.Sconit.Web.Controllers.KB
{
    public class KanbanFlowController : WebAppBaseController
    {
        //
        // GET:

        //public IGenericMgr genericMgr { get; set; }
        public IFlowMgr flowMgr { get; set; }

        public IReportGen reportGen { get; set; }

        private static string selectCountStatement = "select count(*) from FlowMaster as f ";
        private static string selectStatement = "select f from FlowMaster as f";

        private static string selectCountDetailStatement = "select count(*) from FlowDetail as f ";
        private static string selectDetailStatement = "select f from FlowDetail as f";

        private static string selectCountBindStatement = @"select count(*) 
                                                      from FlowBinding as f join f.MasterFlow as mf join f.BindedFlow as bf";
        private static string selectBindStatement = @"select f
                                                      from FlowBinding as f join f.MasterFlow as mf join f.BindedFlow as bf";
        //private static string userNameDuiplicateVerifyStatement = @"select count(*) from FlowMaster as u where u.Code = ?";
        private static string selectCountFlowShiftDetailStatement = "select count(*) from FlowShiftDetail as f ";

        private static string selectFlowShiftDetailStatement = "select f from FlowShiftDetail as f ";
        public KanbanFlowController()
        {
        }

        [SconitAuthorize(Permissions = "Url_KanbanFlow_View")]
        public ActionResult Index()
        {
            return View();
        }

        #region FlowMaster

        [GridAction]
        [SconitAuthorize(Permissions = "Url_KanbanFlow_View")]
        public ActionResult List(GridCommand command, FlowSearchModel searchModel)
        {
            this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_KanbanFlow_View")]
        public ActionResult _AjaxList(GridCommand command, FlowSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<FlowMaster>(searchStatementModel, command));
        }

        [SconitAuthorize(Permissions = "Url_KanbanFlow_View")]
        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_KanbanFlow_Eid")]
        public ActionResult New(FlowMaster flow)
        {
            if (ModelState.IsValid)
            {
                if (this.genericMgr.FindAll<long>("select count(*) from FlowMaster as f where f.Code = ?", flow.Code)[0] > 0)
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Existing_Code, flow.Code);
                }
                else if (string.IsNullOrEmpty(flow.PartyFrom))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_PartyFrom);
                }
                else if (string.IsNullOrEmpty(flow.PartyTo))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_PartyTo);
                }
                else if (string.IsNullOrEmpty(flow.LocationTo))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_LocationTo);
                }
                //else if (string.IsNullOrEmpty(flow.ShipFrom))
                //{
                //    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_ShipFrom);
                //}
                //else if (string.IsNullOrEmpty(flow.ShipTo))
                //{
                //    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_ShipTo);
                //}
                //else if (string.IsNullOrEmpty(flow.BillAddress))
                //{
                //    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_BillAddress);
                //}
                //else if (string.IsNullOrEmpty(flow.PriceList))
                //{
                //    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_PriceList);
                //}
                else if (string.IsNullOrWhiteSpace(flow.OrderTemplate))
                {
                    base.SaveErrorMessage("error");
                }
                else
                {
                    var partyAddressList = this.genericMgr.FindAll<PartyAddress>("from PartyAddress pa where pa.Party=?", flow.PartyFrom);
                    if (partyAddressList == null || partyAddressList.Count == 0)
                    {
                        SaveErrorMessage("供应商{0}地址没有找到，", flow.PartyFrom);
                    }
                    else
                    {
                        flow.BillAddress = partyAddressList.Where(p => p.IsPrimary == true).FirstOrDefault().Address.Code;
                        flow.OrderTemplate = string.Empty;
                        flow.ReceiptTemplate = string.Empty;
                        flow.AsnTemplate = string.Empty;
                        flow.HuTemplate = string.Empty;
                        flow.FlowStrategy = com.Sconit.CodeMaster.FlowStrategy.ANDON;
                        flow.Type = com.Sconit.CodeMaster.OrderType.Transfer;
                        flowMgr.CreateFlow(flow);
                        SaveSuccessMessage(Resources.SCM.FlowMaster.FlowMaster_Added);
                        return RedirectToAction("Edit/" + flow.Code);
                    }
                }
            }
            return View(flow);
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_KanbanFlow_View")]
        public ActionResult Edit(string id, bool? isReturn)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            else
            {
                TempData["IsReturn"] = isReturn == null ? false : isReturn;
                if (isReturn.HasValue == true && isReturn == true)
                {
                    TempData["TabIndex"] = 2;
                }
                return View("Edit", string.Empty, id);
            }
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_KanbanFlow_View")]
        public ActionResult _Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            FlowMaster flow = this.genericMgr.FindById<FlowMaster>(id);
            return PartialView(flow);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_KanbanFlow_Eid")]
        public ActionResult _Edit(FlowMaster flow)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(flow.PartyFrom))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_PartyFrom);
                }
                else if (string.IsNullOrEmpty(flow.PartyTo))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_PartyTo);
                }
                else if (string.IsNullOrEmpty(flow.LocationTo))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_LocationTo);
                }
                //else if (string.IsNullOrEmpty(flow.ShipFrom))
                //{
                //    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_ShipFrom);
                //}
                //else if (string.IsNullOrEmpty(flow.ShipTo))
                //{
                //    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_ShipTo);
                //}
                //else if (string.IsNullOrEmpty(flow.BillAddress))
                //{
                //    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_BillAddress);
                //}
                //else if (string.IsNullOrEmpty(flow.PriceList))
                //{
                //    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_PriceList);
                //}
                else if (string.IsNullOrWhiteSpace(flow.OrderTemplate))
                {
                    flow.OrderTemplate = "ORD_Transfer.xls";
                    //base.SaveErrorMessage("error");
                }
                else
                {
                    flow.Type = com.Sconit.CodeMaster.OrderType.Procurement;
                    genericMgr.Update(flow);
                    SaveSuccessMessage(Resources.SCM.FlowMaster.FlowMaster_Updated);
                }
            }

            return PartialView(flow);
        }

        [SconitAuthorize(Permissions = "Url_KanbanFlow_Eid")]
        public ActionResult FlowDel(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                flowMgr.DeleteFlow(id);
                SaveSuccessMessage(Resources.SCM.FlowMaster.FlowMaster_Deleted);
                return RedirectToAction("List");
            }
        }

        [SconitAuthorize(Permissions = "Url_KanbanFlow_Eid")]
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                genericMgr.DeleteById<FlowMaster>(id);
                SaveSuccessMessage(Resources.ACC.User.User_Deleted);
                return RedirectToAction("List");
            }
        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, FlowSearchModel searchModel)
        {
            IList<object> param = new List<object>();
            string whereStatement = string.Format(" where exists(select 1 from FlowStrategy as s where s.Flow = f.Code and s.Strategy = ?)  and Type in(?,?)");
            param.Add((int)CodeMaster.FlowStrategy.ANDON);
            param.Add((int)CodeMaster.OrderType.Procurement);
            param.Add((int)CodeMaster.OrderType.Transfer);

            HqlStatementHelper.AddLikeStatement("Code", searchModel.Code, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Description", searchModel.Description, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("LocationTo", searchModel.LocationTo, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("PartyFrom", searchModel.PartyFrom, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("PartyTo", searchModel.PartyTo, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("IsActive", searchModel.IsActive, "f", ref whereStatement, param);

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        [SconitAuthorize(Permissions = "Url_KanbanFlow_View")]
        public void Export(FlowSearchModel searchModel)
        {
            var statement = string.Format("select o from FlowMasterView as o where FlowStrategy={0} and Type = {1} ", (int)CodeMaster.FlowStrategy.KB, (int)CodeMaster.OrderType.Procurement);

            if (!string.IsNullOrWhiteSpace(searchModel.Code))
            {
                statement += " and o.Code like '" + searchModel.Code + "%'";
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Description))
            {
                statement += " and o.Description like '" + searchModel.Description + "%'";
            }
            if (!string.IsNullOrWhiteSpace(searchModel.PartyFrom))
            {
                statement += string.Format(" and o.PartyFrom = '{0}'", searchModel.PartyFrom);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.PartyTo))
            {
                statement += string.Format(" and o.PartyTo = '{0}'", searchModel.PartyTo);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.LocationTo))
            {
                statement += string.Format(" and o.LocationTo = '{0}'", searchModel.LocationTo);
            }

            statement += " and o.IsActive =" + (searchModel.IsActive ? "1" : "0");

            string maxRows = this.systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.ExportMaxRows);
            var flows = this.genericMgr.FindAll<FlowMasterView>(statement, 0, int.Parse(maxRows));

            ExportToXLS<FlowMasterView>("KanBanFlow", "xls", flows);
        }

        [SconitAuthorize(Permissions = "Url_KanbanFlow_Eid")]
        public ActionResult Import(IEnumerable<HttpPostedFileBase> flowattachments)
        {
            try
            {
                foreach (var file in flowattachments)
                {
                    flowMgr.ImportKanBanFlow(file.InputStream, CodeMaster.OrderType.Transfer);
                }
                SaveSuccessMessage(Resources.Global.ImportSuccess_BatchImportSuccessful);
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(Resources.ErrorMessage.Errors_Import_Failed, ex.Message);
            }

            return Content(string.Empty);
        }

        #endregion

        #region Strategy
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_KanbanFlow_View")]
        public ActionResult _Strategy(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            FlowStrategy flowStrategy = this.flowMgr.GetFlowStrategy(id);
            FlowMaster flowMaster = genericMgr.FindById<FlowMaster>(id);
            if (flowStrategy == null)
            {
                flowStrategy = new FlowStrategy { Flow = id, Strategy = CodeMaster.FlowStrategy.ANDON };
            }
            ViewBag.qiTiaoBian = flowMaster.Type == com.Sconit.CodeMaster.OrderType.Procurement ? true : false;
            ViewBag.Strategy = (int)com.Sconit.CodeMaster.FlowStrategy.ANDON;
            ViewBag.NextWindowTime = flowStrategy.NextWindowTime;
            ViewBag.NextOrderTime = flowStrategy.NextOrderTime;
            ViewBag.WindowTimeType = flowStrategy.WindowTimeType;
            return PartialView(flowStrategy);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_KanbanFlow_Eid")]
        public ActionResult _Strategy(FlowStrategy flowStrategy)
        {
            ViewBag.Strategy = (int)com.Sconit.CodeMaster.FlowStrategy.ANDON;
            ViewBag.WindowTimeType = flowStrategy.WindowTimeType;
            flowStrategy.Strategy = com.Sconit.CodeMaster.FlowStrategy.ANDON;
            FlowMaster flowMaster = genericMgr.FindById<FlowMaster>(flowStrategy.Flow);
            ViewBag.qiTiaoBian = flowMaster.Type == com.Sconit.CodeMaster.OrderType.Procurement ? true : false;
            if (ModelState.IsValid)
            {
                flowMgr.UpdateFlowStrategy(flowStrategy);
                SaveSuccessMessage(Resources.SCM.FlowStrategy.FlowStrategy_Updated);
            }

            return PartialView(flowStrategy);
        }
        #endregion

        #region FlowDetail
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_KanbanFlowDet_View")]
        public ActionResult FlowDetailIndex()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_KanbanFlowDet_View")]
        public ActionResult FlowDetailList(GridCommand command, FlowDetailSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = 50;
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_KanbanFlowDet_View")]
        public ActionResult _AjaxFlowDetailList(GridCommand command, FlowDetailSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareFlowDetailSearchStatement(command, searchModel);
            GridModel<FlowDetail> gridList = GetAjaxPageData<FlowDetail>(searchStatementModel, command);
            if (gridList.Data != null && gridList.Data.Count() > 0)
            {
                foreach (FlowDetail flowDetail in gridList.Data)
                {
                    flowDetail.ItemDescription = this.genericMgr.FindById<Item>(flowDetail.Item).Description;
                    var kanbanCards = this.genericMgr.FindAll<KanbanCard>(" select k from KanbanCard as k where k.FlowDetailId=? ",flowDetail.Id);
                    if (kanbanCards != null && kanbanCards.Count > 0)
                    {
                        flowDetail.KanbanNo = kanbanCards.First().OpRefSequence;
                    }
                }
            }
            return PartialView(gridList);
        }


        [HttpGet]
        [SconitAuthorize(Permissions = "Url_KanbanFlowDet_View")]
        public ActionResult _DetailSearch(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            ViewBag.flow = id;
            return PartialView();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_KanbanFlowDet_View")]
        public ActionResult _Detail(GridCommand command, FlowDetailSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_KanbanFlowDet_View")]
        public ActionResult _AjaxDetailList(GridCommand command, FlowDetailSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareDetailSearchStatement(command, searchModel);
            GridModel<FlowDetail> gridList = GetAjaxPageData<FlowDetail>(searchStatementModel, command);
            if (gridList.Data != null && gridList.Data.Count() > 0)
            {
                foreach (FlowDetail flowDetail in gridList.Data)
                {
                    flowDetail.ItemDescription = this.genericMgr.FindById<Item>(flowDetail.Item).Description;
                    var kanbanCards = this.genericMgr.FindAll<KanbanCard>(" select k from KanbanCard as k where k.FlowDetailId=? ", flowDetail.Id);
                    if (kanbanCards != null && kanbanCards.Count > 0)
                    {
                        flowDetail.KanbanNo = kanbanCards.First().OpRefSequence;
                    }
                }
            }
            return PartialView(gridList);
        }

        [SconitAuthorize(Permissions = "Url_KanbanFlowDet_Edit")]
        public ActionResult _DetailNew(string id)
        {
            FlowDetail flowDetail = new FlowDetail();
            FlowMaster flowMaster = this.genericMgr.FindById<FlowMaster>(id);
            flowDetail.LocationTo = flowMaster.LocationTo;
            flowDetail.Flow = id;
            return PartialView(flowDetail);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_KanbanFlowDet_Edit")]
        public ActionResult _DetailNew(FlowDetail flowDetail, string id)
        {
            if (ModelState.IsValid)
            {
                if (false)//暂不做控制
                {
                    // base.SaveErrorMessage(Resources.ErrorMessage.Errors_Existing_Code, flowDetail.Code);
                }
                else
                {
                    IList<FlowDetail> flowDetailList = this.genericMgr.FindAll<FlowDetail>("select fd from FlowDetail as fd where fd.Flow='" + flowDetail.Flow + "' and fd.Item='" + flowDetail.Item + "'");
                    if (flowDetailList.Count > 0)
                    {
                        SaveErrorMessage("物料已经存在，请重新选择。");

                    }
                    else
                    {
                        addFlowDetailItem(flowDetail);
                        //flowDetail.Container = item.Container;
                        flowMgr.CreateFlowDetail(flowDetail);
                        SaveSuccessMessage(Resources.SCM.FlowDetail.FlowDetail_Added);
                        return RedirectToAction("_Detail/" + flowDetail.Flow);
                    }
                }
            }
            return PartialView(flowDetail);
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_KanbanFlowDet_Edit")]
        public ActionResult btnDel(int? id, string Flow)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                //genericMgr.DeleteById<FlowDetail>(id);
                flowMgr.DeleteKBFlowDetail(id.Value);
                SaveSuccessMessage(Resources.SCM.FlowDetail.FlowDetail_Deleted);
                return RedirectToAction("_Detail/" + Flow);
            }
        }

        public ActionResult GetRefItemCode(string item)
        {
            Item itemEntity = this.genericMgr.FindById<Item>(item);
            if (itemEntity == null)
            {
                itemEntity = new Item(); ;
            }
            return Json(itemEntity);
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_KanbanFlowDet_Edit")]
        public ActionResult _DetailEdit(int? id)
        {
            if (!id.HasValue || id.Value == 0)
            {
                return HttpNotFound();
            }

            FlowDetail flowDetail = this.genericMgr.FindById<FlowDetail>(id);
            FlowMaster flowMaster = genericMgr.FindById<FlowMaster>(flowDetail.Flow);
            ViewBag.Type = flowMaster.Type;
            ViewBag.Item = this.genericMgr.FindById<Item>(flowDetail.Item);
            return PartialView(flowDetail);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_KanbanFlowDet_Edit")]
        public ActionResult _DetailEdit(FlowDetail flowDetail, int? id)
        {
            if (ModelState.IsValid)
            {
                IList<FlowDetail> flowDetailList = this.genericMgr.FindAll<FlowDetail>("select fd from FlowDetail as fd where fd.Flow='" + flowDetail.Flow + "' and fd.Item='" + flowDetail.Item + "' and fd.BinTo='" + flowDetail.BinTo + "'");
                if (flowDetailList.Count > 0)
                {
                    if (flowDetailList[0].Id == flowDetail.Id)
                    {
                        FlowMaster flowMaster = genericMgr.FindById<FlowMaster>(flowDetail.Flow);
                        //flowDetail.Strategy = flowDetailList[0].Strategy;
                        addFlowDetailItem(flowDetail);
                        flowMgr.UpdateFlowDetail(flowDetail);
                        this.genericMgr.UpdateWithNativeQuery("update KB_KanbanCard set Qty=?,Container=? where FlowDetId=?", 
                            new object[] { flowDetail.MinUnitCount, flowDetail.Container, flowDetail.Id },
                            new IType[] { NHibernateUtil.Decimal, NHibernateUtil.String, NHibernateUtil.Int32 });

                        ViewBag.Type = flowMaster.Type;
                        ViewBag.Item = this.genericMgr.FindById<Item>(flowDetail.Item);
                        SaveSuccessMessage(Resources.SCM.FlowDetail.FlowDetail_Updated);
                    }
                    else
                    {
                        SaveErrorMessage("物料已经存在，请重新选择。");
                    }

                }
                else
                {
                    SaveErrorMessage("路线明细不存在，请重新选择。");
                }
            }

            TempData["TabIndex"] = 2;
            return PartialView(flowDetail);
        }


        [HttpGet]
        [SconitAuthorize(Permissions = "Url_KanbanFlowDet_Edit")]
        public ActionResult FlowDetailEdit(int? id)
        {
            if (!id.HasValue || id.Value == 0)
            {
                return HttpNotFound();
            }

            FlowDetail flowDetail = this.genericMgr.FindById<FlowDetail>(id);
            FlowMaster flowMaster = genericMgr.FindById<FlowMaster>(flowDetail.Flow);
            ViewBag.Type = flowMaster.Type;
            ViewBag.Item = this.genericMgr.FindById<Item>(flowDetail.Item);
            return View(flowDetail);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_KanbanFlowDet_Edit")]
        public ActionResult FlowDetailEdit(FlowDetail flowDetail, int? id)
        {
            if (ModelState.IsValid)
            {
                IList<FlowDetail> flowDetailList = this.genericMgr.FindAll<FlowDetail>("select fd from FlowDetail as fd where fd.Flow='" + flowDetail.Flow + "' and fd.Item='" + flowDetail.Item + "' and fd.BinTo='"+ flowDetail.BinTo +"'");
                if (flowDetailList.Count > 0)
                {
                    if (flowDetailList[0].Id == flowDetail.Id)
                    {
                        FlowMaster flowMaster = genericMgr.FindById<FlowMaster>(flowDetail.Flow);
                        //flowDetail.Strategy = flowDetailList[0].Strategy;
                        addFlowDetailItem(flowDetail);
                        flowMgr.UpdateFlowDetail(flowDetail);
                        this.genericMgr.UpdateWithNativeQuery("update KB_KanbanCard set Qty=?,Container=? where FlowDetId=?",
                                                    new object[] { flowDetail.MinUnitCount, flowDetail.Container, flowDetail.Id },
                                                    new IType[] { NHibernateUtil.Decimal, NHibernateUtil.String, NHibernateUtil.Int32 });
                        ViewBag.Type = flowMaster.Type;
                        ViewBag.Item = this.genericMgr.FindById<Item>(flowDetail.Item);
                        SaveSuccessMessage(Resources.SCM.FlowDetail.FlowDetail_Updated);
                    }
                    else
                    {
                        SaveErrorMessage("物料已经存在，请重新选择。");
                    }

                }
                else
                {
                    SaveErrorMessage("路线明细不存在，请重新选择。");
                }
            }

            //TempData["TabIndex"] = 2;
            return View(flowDetail);
        }


        public void FlowDetailDelete(string ids, FlowDetailSearchModel searchModel)
        {
            IList<object> data = new List<object>();
            //IList<FlowDetail> flowDetailList = null;
            //IList<Item> itemList = new List<Item>();
            try
            {
                if (!string.IsNullOrEmpty(ids))
                {

                    string[] array = ids.Split(',');

                    string sqlkb = string.Empty;
                    string hqlFlowDet = string.Empty;
                    IList<object> selectPartyPara = new List<object>();
                    foreach (var para in array)
                    {
                        if (hqlFlowDet == string.Empty)
                        {
                            sqlkb = "delete from KB_KanbanCard where FlowDetId in (?";
                            hqlFlowDet = "delete from SCM_FlowDet where Id in (?";
                        }
                        else
                        {
                            sqlkb += ",?";
                            hqlFlowDet += ",?";
                        }
                        selectPartyPara.Add(para);
                    }
                    sqlkb += ")";
                    hqlFlowDet += ")";

                    this.genericMgr.FindAllWithNativeSql(sqlkb, selectPartyPara.ToArray());
                    this.genericMgr.FindAllWithNativeSql(hqlFlowDet, selectPartyPara.ToArray());

                    //flowDetailList = this.genericMgr.FindAll<FlowDetail>(hqlFlowDet + sortStatement, selectPartyPara.ToArray());
                    //itemList = this.genericMgr.FindEntityWithNativeSql<Item>(sqlItem, selectPartyPara.ToArray());
                }
                else
                {
                    throw new BusinessException("路线明细Id不能为空.");
                }

              
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
        }


        private SearchStatementModel PrepareDetailSearchStatement(GridCommand command, FlowDetailSearchModel searchModel)
        {
            string whereStatement = " where f.Flow='" + searchModel.flowCode + "'";
            IList<object> param = new List<object>();
            HqlStatementHelper.AddLikeStatement("Item", searchModel.Item, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "f", ref whereStatement, param);
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountDetailStatement;
            searchStatementModel.SelectStatement = selectDetailStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        private SearchStatementModel PrepareFlowDetailSearchStatement(GridCommand command, FlowDetailSearchModel searchModel)
        {
            string whereStatement = " where exists(select 1 from FlowStrategy as s where s.Flow = f.Flow and s.Strategy = 7)";
            IList<object> param = new List<object>();
            HqlStatementHelper.AddLikeStatement("Item", searchModel.Item, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "f", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("BinTo", searchModel.BinTo,HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);

            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.StartDate, searchModel.EndDate, "f", ref whereStatement, param);
            }
            else if (searchModel.StartDate != null & searchModel.EndDate == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.StartDate, "f", ref whereStatement, param);
            }
            else if (searchModel.StartDate == null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.EndDate, "f", ref whereStatement, param);
            }
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            TempData["SortStatement"] = sortingStatement;
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountDetailStatement;
            searchStatementModel.SelectStatement = selectDetailStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        private void addFlowDetailItem(FlowDetail flowDetail)
        {
            Item item = this.genericMgr.FindById<Item>(flowDetail.Item);
            flowDetail.BaseUom = item.Uom;
            flowDetail.Uom = item.Uom;
            flowDetail.ReferenceItemCode = item.ReferenceCode;
            flowDetail.UnitCount = item.UnitCount;
            //flowDetail.UnitCountDescription = item.un;
        }

        #region 看板路线明细导出
        public void ExportKanbanFlowDetailXLS(FlowDetailSearchModel searchModel)
        {
            StringBuilder sb = new StringBuilder();
            IList<object> param = new List<object>();
            sb.Append(@" select d.Id,d.Item,d.Flow,i.RefCode,i.Desc1,d.MinUC,d.Container,d.ContainerDesc,d.BinTo,d.ProdScan,d.Uom,d.CycloidAmount,kb.OpRefSeq from SCM_FlowDet as d 
left join MD_Item as i on d.Item=i.Code
left join ( select FlowDetId,OpRefSeq from KB_KanbanCard group by FlowDetId,OpRefSeq ) as kb on d.Id=kb.FlowDetId
where exists(select 1 from SCM_FlowStrategy as s where s.Flow = d.Flow and s.Strategy = 7) ");

            if (searchModel.Flow != null)
            {
                sb.Append(" and d.Flow = ?");
                param.Add(searchModel.Flow);
            }
            if (searchModel.Item != null)
            {
                sb.Append(" and Item = ?");
                param.Add(searchModel.Item);
            }
            if (searchModel.BinTo != null)
            {
                sb.Append(" and BinTo = ?");
                param.Add(searchModel.BinTo);
            }

            if (searchModel.StartDate != null)
            {
                sb.Append(" and CreateDate >=?");
                param.Add(searchModel.StartDate);
            }
            if (searchModel.EndDate != null)
            {
                sb.Append(" and CreateDate <＝ ?");
                param.Add(searchModel.EndDate);
            }
            IList<FlowDetail> exportList = new List<FlowDetail>();
            IList<object[]> searchResult = this.genericMgr.FindAllWithNativeSql<object[]>(sb.ToString(),param.ToArray());
            if (searchResult != null && searchResult.Count > 0)
            {
                //d.Id,.Item,d.Flow,i.RefCode,i.Desc1,d.MinUC,d.Container,d.ContainerDesc,d.BinTo,d.ProdScan,d.Uom,d.CycloidAmount,kb.OpRefSeq 
                exportList = (from tak in searchResult
                                select new FlowDetail
                                {
                                    Id=(Int32)tak[0],
                                    Item = (string)tak[1],
                                    Flow = (string)tak[2],
                                    ReferenceItemCode = (string)tak[3],
                                    ItemDescription = (string)tak[4],
                                    MinUnitCount = (decimal)tak[5],
                                    Container = (string)tak[6],
                                    ContainerDescription = (string)tak[7],
                                    BinTo = (string)tak[8],
                                    ProductionScan = (string)tak[9],
                                    Uom = (string)tak[10],
                                    CycloidAmount = (int)tak[11],
                                    KanbanNo = (string)tak[12],
                                }).ToList();
            }
            ExportToXLS<FlowDetail>("ExportKanbanScanXLS", "xls", exportList);

        }
        #endregion
        #endregion

        #region FlowShiftDet
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_KanbanFlow_View")]
        public ActionResult _FlowShiftDetailSearch(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            ViewBag.flow = id;
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_KanbanFlow_View")]
        public ActionResult _AjaxFlowShiftDetailList(GridCommand command, string Shift, string FlowCode)
        {

            SearchStatementModel searchStatementModel = PrepareFlowShiftDetailSearchStatement(command, Shift, FlowCode);
            return PartialView(GetAjaxPageData<FlowShiftDetail>(searchStatementModel, command));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_KanbanFlow_View")]
        public ActionResult _FlowShiftDetailList(GridCommand command, string FlowCode, string Shift)
        {
            ViewBag.Shift = Shift;
            ViewBag.Flow = FlowCode;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [SconitAuthorize(Permissions = "Url_KanbanFlow_Eid")]
        public JsonResult _SaveflowShoftDetailEdit([Bind(Prefix = "inserted")]IEnumerable<FlowShiftDetail> insertedFlowShiftDetails,
            [Bind(Prefix = "updated")]IEnumerable<FlowShiftDetail> updatedFlowShiftDetails,
            [Bind(Prefix = "deleted")]IEnumerable<FlowShiftDetail> deletedFlowShiftDetails, string flow)
        {
            try
            {
                IList<FlowShiftDetail> existFlowShiftDetailList = genericMgr.FindAll<FlowShiftDetail>("from FlowShiftDetail s where s.Flow = ?", flow);

                IList<FlowShiftDetail> deleted = new List<FlowShiftDetail>();
                if (deletedFlowShiftDetails != null)
                {
                    deleted = deletedFlowShiftDetails.ToList();
                }

                IList<FlowShiftDetail> inserted = new List<FlowShiftDetail>();
                if (insertedFlowShiftDetails != null)
                {
                    foreach (var flowShiftDet in insertedFlowShiftDetails.ToList())
                    {
                        if (string.IsNullOrEmpty(flowShiftDet.Shift))
                        {
                            throw new BusinessException(Resources.ErrorMessage.Errors_Common_FieldCanNotBeNull, Resources.SCM.FlowShiftDetail.FlowShiftDetail_Shift);
                        }
                        if (string.IsNullOrEmpty(flowShiftDet.WindowTime))
                        {
                            throw new BusinessException(Resources.ErrorMessage.Errors_Common_FieldCanNotBeNull, Resources.SCM.FlowShiftDetail.FlowShiftDetail_WindowTime);
                        }


                        var insertedEquals = insertedFlowShiftDetails.Where(c => c.Shift == flowShiftDet.Shift);

                        if (insertedEquals.Count() > 1)
                        {
                            throw new BusinessException(Resources.PRD.ShiftDetail.ShiftDetail_SameShift);
                        }

                        if (updatedFlowShiftDetails != null)
                        {
                            var updatedEquals = updatedFlowShiftDetails.Where(c => c.Shift == flowShiftDet.Shift);
                            if (updatedEquals.Count() > 0)
                            {
                                throw new BusinessException(Resources.PRD.ShiftDetail.ShiftDetail_SameShift);
                            }
                        }

                        if (existFlowShiftDetailList != null)
                        {
                            var existEquals = existFlowShiftDetailList.Where(c => c.Shift == flowShiftDet.Shift);
                            if (existEquals.Count() > 0)
                            {
                                throw new BusinessException(Resources.PRD.ShiftDetail.ShiftDetail_SameShift);
                            }
                        }

                        inserted.Add(flowShiftDet);
                    }
                }

                IList<FlowShiftDetail> updated = new List<FlowShiftDetail>();
                if (updatedFlowShiftDetails != null)
                {
                    foreach (var flowShiftDet in updatedFlowShiftDetails.ToList())
                    {
                        if (string.IsNullOrEmpty(flowShiftDet.Shift))
                        {
                            throw new BusinessException(Resources.ErrorMessage.Errors_Common_FieldCanNotBeNull, Resources.SCM.FlowShiftDetail.FlowShiftDetail_Shift);
                        }
                        if (string.IsNullOrEmpty(flowShiftDet.WindowTime))
                        {
                            throw new BusinessException(Resources.ErrorMessage.Errors_Common_FieldCanNotBeNull, Resources.SCM.FlowShiftDetail.FlowShiftDetail_WindowTime);
                        }
                        if (insertedFlowShiftDetails != null)
                        {
                            var insertedEquals = insertedFlowShiftDetails.Where(c => c.Shift == flowShiftDet.Shift);
                            if (insertedEquals.Count() > 0)
                            {
                                throw new BusinessException(Resources.PRD.ShiftDetail.ShiftDetail_SameShift);
                            }
                        }

                        var updatedEquals = updatedFlowShiftDetails.Where(c => c.Shift == flowShiftDet.Shift && c.Id != flowShiftDet.Id);
                        if (updatedEquals.Count() > 0)
                        {
                            throw new BusinessException(Resources.PRD.ShiftDetail.ShiftDetail_SameShift);
                        }

                        if (existFlowShiftDetailList != null)
                        {
                            var existEquals = existFlowShiftDetailList.Where(c => c.Shift == flowShiftDet.Shift && c.Id != flowShiftDet.Id);
                            if (existEquals.Count() > 0)
                            {
                                throw new BusinessException(Resources.PRD.ShiftDetail.ShiftDetail_SameShift);
                            }
                        }
                        updated.Add(flowShiftDet);
                    }
                }

                flowMgr.UpdateFlowShiftDetails(flow, inserted, updated, deleted);

                object obj = new { };
                SaveSuccessMessage(Resources.SCM.FlowShiftDetail.FlowShiftDetail_Save);
                return Json(obj);
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                return Json(null);
            }
        }

        private SearchStatementModel PrepareFlowShiftDetailSearchStatement(GridCommand command, string Shift, string Flow)
        {
            string whereStatement = string.Empty;
            IList<object> param = new List<object>();
            HqlStatementHelper.AddEqStatement("Shift", Shift, "f", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Flow", Flow, "f", ref whereStatement, param);
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountFlowShiftDetailStatement;
            searchStatementModel.SelectStatement = selectFlowShiftDetailStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }
        #endregion


        //[SconitAuthorize(Permissions = "Url_KanbanFlow_View")]
        public string PrintKBLable(string ids, FlowDetailSearchModel searchModel)
        {
            IList<object> data = new List<object>();
            IList<FlowDetail> flowDetailList = null;
            IList<Item> itemList = new List<Item>();
            try
            {
                if (!string.IsNullOrEmpty(ids))
                {
                    string sortStatement = TempData["SortStatement"] != null ? (string)TempData["SortStatement"] : " order by CreateDate asc ";
                    TempData["SortStatement"] = sortStatement;
                    //根据查询条件打印
                    if (ids.Equals("ALL", StringComparison.OrdinalIgnoreCase))
                    {
                        string whereStatement = " where exists(select 1 from FlowStrategy as s where s.Flow = f.Flow and s.Strategy = 7) ";

                        IList<object> param = new List<object>();

                        HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "f", ref whereStatement, param);
                        HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "f", ref whereStatement, param);
                        HqlStatementHelper.AddLikeStatement("BinTo", searchModel.BinTo, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);

                        if (searchModel.StartDate != null & searchModel.EndDate != null)
                        {
                            HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.StartDate, searchModel.EndDate, "f", ref whereStatement, param);
                        }
                        else if (searchModel.StartDate != null & searchModel.EndDate == null)
                        {
                            HqlStatementHelper.AddGeStatement("CreateDate", searchModel.StartDate, "f", ref whereStatement, param);
                        }
                        else if (searchModel.StartDate == null & searchModel.EndDate != null)
                        {
                            HqlStatementHelper.AddLeStatement("CreateDate", searchModel.EndDate, "f", ref whereStatement, param);
                        }


                        flowDetailList = this.genericMgr.FindAll<FlowDetail>("from FlowDetail f " + whereStatement +sortStatement, param.ToArray());
                        itemList = this.genericMgr.FindEntityWithNativeSql<Item>(@"select i.* from MD_Item i inner join SCM_FlowDet fd on i.Code=fd.Item 
                                                                            inner join SCM_FlowStrategy fs on fd.Flow=fs.Flow where fs.Strategy=7 " + (string.IsNullOrEmpty(searchModel.Flow)?string.Empty:" and fd.Flow='"+searchModel.Flow+"'")
                                                                            + (string.IsNullOrEmpty(searchModel.Item) ? string.Empty : " and fd.Item='" + searchModel.Item+"'")
                                                                            + (string.IsNullOrEmpty(searchModel.BinTo) ? string.Empty : " and fd.BinTo like '" + searchModel.Item + "%'"));
                    }
                    //根据选择的行数打印
                    else
                    {
                        string[] array = ids.Split(',');

                        string sqlItem = string.Empty;
                        string hqlFlowDet = string.Empty;
                        IList<object> selectPartyPara = new List<object>();
                        foreach (var para in array)
                        {
                            if (hqlFlowDet == string.Empty)
                            {
                                sqlItem = "select i.* from MD_Item i inner join SCM_FlowDet fd on i.Code=fd.Item where fd.Id in(?";
                                hqlFlowDet = " from FlowDetail where Id in (?";
                            }
                            else
                            {
                                sqlItem += ",?";
                                hqlFlowDet += ",?";
                            }
                            selectPartyPara.Add(para);
                        }
                        sqlItem += ")";
                        hqlFlowDet += ")";

                        flowDetailList = this.genericMgr.FindAll<FlowDetail>(hqlFlowDet + sortStatement, selectPartyPara.ToArray());
                        itemList = this.genericMgr.FindEntityWithNativeSql<Item>(sqlItem, selectPartyPara.ToArray());
                    }
                }
                else
                {
                    throw new BusinessException("路线明细Id不能为空.");
                }

                foreach (var flowDet in flowDetailList)
	            {
		            flowDet.ItemDescription = itemList.FirstOrDefault(i=>i.Code==flowDet.Item).Description;
	            }

                string printUrl = string.Empty;
                if (flowDetailList.Count() > 0)
                {
                    if (printUrl == string.Empty)
                    {

                        data.Add(flowDetailList);
                        printUrl = reportGen.WriteToFile("KBLabel.xls", data);
                    }
                }
                return printUrl;
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);


            }

            return string.Empty;
        }

        #region FlowShiftDetail

        //[SconitAuthorize(Permissions = "Url_KanbanFlow_View")]
        //public ActionResult _WebShiftDetail(int shiftDetailId)
        //{
        //    IList<ShiftDetail> flowDetailList = this.genericMgr.FindAll<ShiftDetail>(" from ShiftDetail as s where s.Id='" + shiftDetailId + "'");
        //    if (flowDetailList.Count() > 0)
        //    {
        //        return this.Json(flowDetailList[0]);
        //    }
        //    return this.Json(null);
        //}

        //[HttpGet]
        //[SconitAuthorize(Permissions = "Url_KanbanFlow_View")]
        //public ActionResult _FlowShiftDetailSearch(string id)
        //{
        //    if (string.IsNullOrWhiteSpace(id))
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.flow = id;
        //    return PartialView();
        //}

        //[SconitAuthorize(Permissions = "Url_KanbanFlow_View")]
        //public ActionResult _FlowShiftDetailNew(string FlowCode)
        //{
        //    ViewBag.Flow = FlowCode;
        //    return PartialView();
        //}

        //[GridAction]
        //[SconitAuthorize(Permissions = "Url_KanbanFlow_View")]
        //public ActionResult _FlowShiftDetailList(GridCommand command, string FlowCode, string Shift)
        //{
        //    ViewBag.Shift = Shift;
        //    ViewBag.Flow = FlowCode;
        //    ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
        //    return PartialView();
        //}

        //[GridAction(EnableCustomBinding = true)]
        //[SconitAuthorize(Permissions = "Url_KanbanFlow_View")]
        //public ActionResult _AjaxFlowShiftDetailList(GridCommand command, string SearchShift, string FlowCode)
        //{

        //    SearchStatementModel searchStatementModel = PrepareFlowShiftDetailSearchStatement(command, SearchShift, FlowCode);
        //    return PartialView(GetAjaxPageData<FlowShiftDetail>(searchStatementModel, command));
        //}

        //private SearchStatementModel PrepareFlowShiftDetailSearchStatement(GridCommand command, string SearchShift, string Flow)
        //{
        //    string whereStatement = string.Empty;
        //    IList<object> param = new List<object>();
        //    HqlStatementHelper.AddEqStatement("Shift", SearchShift, "f", ref whereStatement, param);
        //    HqlStatementHelper.AddEqStatement("Flow", Flow, "f", ref whereStatement, param);

        //    if (command.SortDescriptors.Count > 0)
        //    {
        //        if (command.SortDescriptors[0].Member == "Shift_value")
        //        {
        //            command.SortDescriptors[0].Member = "Shift";
        //        }
        //    }

        //    string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
        //    SearchStatementModel searchStatementModel = new SearchStatementModel();
        //    searchStatementModel.SelectCountStatement = selectCountFlowShiftDetailStatement;
        //    searchStatementModel.SelectStatement = selectFlowShiftDetailStatement;
        //    searchStatementModel.WhereStatement = whereStatement;
        //    searchStatementModel.SortingStatement = sortingStatement;
        //    searchStatementModel.Parameters = param.ToArray<object>();

        //    return searchStatementModel;
        //}

        //[SconitAuthorize(Permissions = "Url_KanbanFlow_View")]
        //public ActionResult _SaveflowShiftDetailEdit([Bind(Prefix = "inserted")]IEnumerable<FlowShiftDetail> insertedFlowShiftDetails,
        //    [Bind(Prefix = "updated")]IEnumerable<FlowShiftDetail> updatedFlowShiftDetails,
        //    [Bind(Prefix = "deleted")]IEnumerable<FlowShiftDetail> deletedFlowShiftDetails, string flow, string SearchShift, GridCommand command)
        //{
        //    try
        //    {
        //        var FlowShiftDethql = "select f from FlowShiftDetail f where f.Flow=? and f.Shift=? and f.ShiftDetailId=? ";
        //        var countFlowShiftDet = "select count(*) from FlowShiftDetail f where f.Flow=? and f.Shift=? and f.ShiftDetailId=? ";
        //        IList<FlowShiftDetail> deleted = new List<FlowShiftDetail>();
        //        if (deletedFlowShiftDetails != null)
        //        {
        //            deleted = deletedFlowShiftDetails.ToList();
        //        }

        //        IList<FlowShiftDetail> inserted = new List<FlowShiftDetail>();
        //        if (insertedFlowShiftDetails != null)
        //        {
        //            int i = 0;
        //            // IList<Object> seq = this.genericMgr.FindAll<Object>("select Max(Sequence) from FlowShiftDetail f where f.Flow=? ", new object[] { flow });
        //            // if (seq.Count > 0) { i = Convert.ToInt32(seq[0]); }
        //            foreach (var fsDet in insertedFlowShiftDetails.ToList())
        //            {
        //                // i++;

        //                ValidatFlowShiftDet(inserted, fsDet);

        //                var detetedEquals = deleted.Where(f => f.Shift == fsDet.Shift & f.ShiftDetailId == f.ShiftDetailId);
        //                if (detetedEquals.Count() == 0)
        //                {
        //                    if (this.genericMgr.FindAll<long>(countFlowShiftDet, new object[] { flow, fsDet.Shift, fsDet.ShiftDetailId })[0] > 0)
        //                    {
        //                        throw new BusinessException(Resources.PRD.ShiftDetail.ShiftDetail_DBSameShift);
        //                    }
        //                }
        //                // fsDet.Sequence = (short)i;
        //                inserted.Add(fsDet);
        //            }
        //        }

        //        IList<FlowShiftDetail> updated = new List<FlowShiftDetail>();
        //        if (updatedFlowShiftDetails != null)
        //        {
        //            foreach (var fsDet in updatedFlowShiftDetails.ToList())
        //            {
        //                ValidatFlowShiftDet(updated, fsDet);
        //                Validat(inserted, fsDet);

        //                var detetedEquals = deleted.Where(f => f.Shift == fsDet.Shift & f.ShiftDetailId == f.ShiftDetailId);
        //                if (detetedEquals.Count() == 0)
        //                {
        //                    IList<FlowShiftDetail> list = this.genericMgr.FindAll<FlowShiftDetail>(FlowShiftDethql, new object[] { flow, fsDet.Shift, fsDet.ShiftDetailId });
        //                    if (list.Count() > 0 && list[0].Id != fsDet.Id)
        //                    {
        //                        throw new BusinessException(Resources.PRD.ShiftDetail.ShiftDetail_DBSameShift);
        //                    }

        //                }
        //                updated.Add(fsDet);
        //            }
        //        }

        //        flowMgr.UpdateFlowShiftDetais(flow, inserted, updated, deleted);

        //        SaveSuccessMessage(Resources.SCM.FlowShiftDetail.FlowShiftDetail_Save);
        //        ViewBag.Flow = flow;
        //        ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
        //        SearchStatementModel searchStatementModel = PrepareFlowShiftDetailSearchStatement(command, SearchShift, flow);
        //        return PartialView("_FlowShiftDetailList", GetAjaxPageData<FlowShiftDetail>(searchStatementModel, command).Data);
        //    }
        //    catch (BusinessException ex)
        //    {
        //        SaveBusinessExceptionMessage(ex);
        //        return Json(null);
        //    }
        //}

        //private void ValidatFlowShiftDet(IList<FlowShiftDetail> list, FlowShiftDetail flowShiftDet)
        //{
        //    if (string.IsNullOrEmpty(flowShiftDet.Shift))
        //    {
        //        throw new BusinessException(Resources.ErrorMessage.Errors_Common_FieldCanNotBeNull, Resources.PRD.ShiftDetail.ShiftDetail_Shift);
        //    }
        //    if (flowShiftDet.ShiftDetailId == 0)
        //    {
        //        throw new BusinessException(Resources.ErrorMessage.Errors_Common_FieldCanNotBeNull, Resources.PRD.ShiftDetail.ShiftDetail_ShiftDetailId);
        //    }
        //    IList<ShiftDetail> flowDetailList = this.genericMgr.FindAll<ShiftDetail>(" from ShiftDetail as s where s.Id=? and s.Shift=? ", new object[] { flowShiftDet.ShiftDetailId, flowShiftDet.Shift });
        //    if (flowDetailList.Count == 0)
        //    {
        //        throw new BusinessException(Resources.PRD.ShiftDetail.ShiftDetail_ErrorsTime);
        //    }
        //    Validat(list, flowShiftDet);
        //    flowShiftDet.StartTime = flowDetailList[0].StartTime;
        //    flowShiftDet.EndTime = flowDetailList[0].EndTime;
        //    flowShiftDet.Sequence = flowDetailList[0].Sequence;
        //}

        //private void Validat(IList<FlowShiftDetail> list, FlowShiftDetail flowShiftDet)
        //{
        //    var insertedEquals = list.Where(c => c.Shift == flowShiftDet.Shift & c.ShiftDetailId == flowShiftDet.ShiftDetailId);
        //    if (insertedEquals.Count() > 0)
        //    {
        //        throw new BusinessException(Resources.PRD.ShiftDetail.ShiftDetail_SameShift);
        //    }
        //}

        //[SconitAuthorize(Permissions = "Url_KanbanFlow_View")]
        //public ActionResult ImportFlowShiftDetail(IEnumerable<HttpPostedFileBase> attachments)
        //{
        //    try
        //    {
        //        foreach (var file in attachments)
        //        {
        //            flowMgr.CreateFlowShiftDetailXls(file.InputStream);
        //        }
        //        SaveSuccessMessage(Resources.SCM.FlowShiftDetail.FlowShiftDetail_Import);
        //    }
        //    catch (BusinessException ex)
        //    {
        //        SaveBusinessExceptionMessage(ex);
        //    }
        //    catch (Exception ex)
        //    {
        //        SaveErrorMessage("导入失败。 - " + ex.Message);
        //    }

        //    return Content(string.Empty);
        //}
        #endregion
    }
}
