using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using com.Sconit.Entity.SCM;
using com.Sconit.Service;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels.SCM;
using com.Sconit.Web.Util;
using Telerik.Web.Mvc;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.BIL;
using com.Sconit.Entity.SYS;
using com.Sconit.Utility;
using com.Sconit.Entity.PRD;
using System;

namespace com.Sconit.Web.Controllers.SCM
{
    public class AssemblyProductionFlowController : WebAppBaseController
    {
        //
        // GET:

        public IFlowMgr flowMgr { get; set; }

        private static string selectCountStatement = "select count(*) from FlowMaster as f ";
        private static string selectStatement = "select f from FlowMaster as f";

        private static string selectCountBindStatement = @"select count(*) 
                                                      from FlowBinding as f join f.MasterFlow as mf ";
        private static string selectBindStatement = @"select f
                                                      from FlowBinding as f join f.MasterFlow as mf ";
        //private static string userNameDuiplicateVerifyStatement = @"select count(*) from FlowMaster as u where u.Code = ?";

        public AssemblyProductionFlowController()
        {
        }

        [SconitAuthorize(Permissions = "Url_AssemblyProductionFlow_View")]
        public ActionResult Index()
        {
            return View();
        }

        #region FlowMaster

        [GridAction]
        [SconitAuthorize(Permissions = "Url_AssemblyProductionFlow_View")]
        public ActionResult List(GridCommand command, FlowSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_AssemblyProductionFlow_View")]
        public ActionResult _AjaxList(GridCommand command, FlowSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<FlowMaster>(searchStatementModel, command));
        }

        [SconitAuthorize(Permissions = "Url_AssemblyProductionFlow_Edit")]
        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_AssemblyProductionFlow_Edit")]
        public ActionResult New(FlowMaster flow)
        {
            if (ModelState.IsValid)
            {
                if (base.genericMgr.FindAll<long>("select count(*) from FlowMaster as f where f.Code = ?", flow.Code)[0] > 0)
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Existing_Code, flow.Code);
                }
                else if (string.IsNullOrEmpty(flow.PartyFrom))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_ProductionPartyFrom);
                }
                else if (string.IsNullOrEmpty(flow.LocationFrom))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_ProductionLocationFrom);
                }
                else if (string.IsNullOrEmpty(flow.LocationTo))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_ProductionLocationTo);
                }
                else if (flow.TaktTime == 0)
                {
                    base.SaveErrorMessage("节拍时间必须大于0。");
                }
                //else if (string.IsNullOrEmpty(flow.VirtualOpReference))
                //{
                //    base.SaveErrorMessage("虚拟工位不能为空。");
                //}
                else if (string.IsNullOrWhiteSpace(flow.Routing))
                {
                    base.SaveErrorMessage("工艺流程不能为空。");
                }
                else
                {
                    var hasError = false;
                    if (!string.IsNullOrWhiteSpace(flow.VirtualOpReference))
                    {
                        var checkeRouting = base.genericMgr.FindAll<RoutingDetail>("select r from RoutingDetail as r where r.Routing=? and r.OpReference=?", new object[] { flow.Routing, flow.VirtualOpReference });
                        if (checkeRouting == null || checkeRouting.Count == 0)
                        {
                            hasError = true;
                            base.SaveErrorMessage(string.Format("虚拟工位{0}不存在工艺流程{1}中。", flow.VirtualOpReference, flow.Routing));
                        }
                    }

                    if (!hasError)
                    {
                        flow.PartyTo = flow.PartyFrom;
                        flow.Type = com.Sconit.CodeMaster.OrderType.Production;
                        flowMgr.CreateFlow(flow);
                        SaveSuccessMessage(Resources.SCM.FlowMaster.FlowMaster_Added);
                        return RedirectToAction("Edit/" + flow.Code);
                    }
                }
            }
            return View(flow);
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_AssemblyProductionFlow_Edit")]
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            else
            {
                return View("Edit", string.Empty, id);
            }
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_AssemblyProductionFlow_Edit")]
        public ActionResult _Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            FlowMaster flow = base.genericMgr.FindById<FlowMaster>(id);
            return PartialView(flow);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_AssemblyProductionFlow_Edit")]
        public ActionResult _Edit(FlowMaster flow)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(flow.PartyFrom))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_ProductionPartyFrom);
                }
                else if (string.IsNullOrEmpty(flow.LocationFrom))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_ProductionLocationFrom);
                }
                else if (string.IsNullOrEmpty(flow.LocationTo))
                {
                    base.SaveErrorMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.SCM.FlowMaster.FlowMaster_ProductionLocationTo);
                }
                else
                {
                    flow.PartyTo = flow.PartyFrom;
                    flow.IsCheckPartyFromAuthority = flow.IsCheckPartyToAuthority;
                    flow.Type = com.Sconit.CodeMaster.OrderType.Production;
                    base.genericMgr.Update(flow);
                    SaveSuccessMessage(Resources.SCM.FlowMaster.FlowMaster_Updated);
                }
            }

            return PartialView(flow);
        }

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

        [SconitAuthorize(Permissions = "Url_AssemblyProductionFlow_Delete")]
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<FlowMaster>(id);
                SaveSuccessMessage(Resources.ACC.User.User_Deleted);
                return RedirectToAction("List");
            }
        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, FlowSearchModel searchModel)
        {
            string whereStatement = " where f.Type=" + (int)com.Sconit.CodeMaster.OrderType.Production + " and f.ProdLineType in (" +
                (int)com.Sconit.CodeMaster.ProdLineType.Chassis + "," + (int)com.Sconit.CodeMaster.ProdLineType.Cab + "," +
                (int)com.Sconit.CodeMaster.ProdLineType.Assembly + "," + (int)com.Sconit.CodeMaster.ProdLineType.Special + "," + (int)com.Sconit.CodeMaster.ProdLineType.Check + ") ";

            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("Code", searchModel.Code, HqlStatementHelper.LikeMatchMode.Start, "f", ref whereStatement, param);

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

        #region Bind
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_AssemblyProductionFlow_Edit")]
        public ActionResult _Binding(GridCommand command, FlowBindModel searchModel, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            ViewBag.flow = id;
            searchModel.BindedFlow = id;
            TempData["FlowBindModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_AssemblyProductionFlow_View")]
        public ActionResult _AjaxBinding(GridCommand command, FlowBindModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareBindSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<FlowBinding>(searchStatementModel, command));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_AssemblyProductionFlow_View")]
        public ActionResult _InsertBinding(FlowBinding flowBinding, string flow)
        {
            ModelState.Remove("Id");
            ModelState.Remove("BindedFlow.Description");
            IList<FlowBinding> stockTakeItem = null;
            if (flowBinding.BindedFlow == null)
            {
                SaveErrorMessage("分装路线不能为空。");
                return Json(null);
            }
            else if (!flowBinding.JointOpReference.HasValue)
            {
                SaveErrorMessage("合装工序不能为空。");
                return Json(null);
            }
            else if (!flowBinding.LeadTime.HasValue)
            {
                SaveErrorMessage("下线提前期不能为空。");
                return Json(null);
            }
            else
            {
                IList<FlowBinding> flowBindingList = base.genericMgr.FindAll<FlowBinding>("from FlowBinding as f where f.MasterFlow.Code=? and f.BindedFlow.Code=? ", new object[] { flow, flowBinding.BindedFlow.Code });
                if (flowBindingList.Count > 0)
                {
                    SaveErrorMessage(string.Format("分装路线{0}已经存在。", flowBinding.BindedFlow.Code));
                    return Json(null);
                }
                else
                {
                    flowBinding.MasterFlow = base.genericMgr.FindById<FlowMaster>(flow);
                    flowBinding.BindedFlow = base.genericMgr.FindById<FlowMaster>(flowBinding.BindedFlow.Code);
                    base.genericMgr.Create(flowBinding);
                    SaveSuccessMessage("添加成功。");
                    stockTakeItem = base.genericMgr.FindAll<FlowBinding>("from FlowBinding as s where s.MasterFlow.Code=?", flowBinding.MasterFlow.Code);
                    return PartialView(new GridModel(stockTakeItem));
                }
            }
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_AssemblyProductionFlow_View")]
        public ActionResult _DeleteBinding(string Id, string flow)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<FlowBinding>(Convert.ToInt32(Id));
                SaveSuccessMessage("删除成功。");
            }
            IList<FlowBinding> stockTakeItem = base.genericMgr.FindAll<FlowBinding>("from FlowBinding as s where s.MasterFlow.Code=?", flow);
            return PartialView(new GridModel(stockTakeItem));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_AssemblyProductionFlow_View")]
        public ActionResult _UpdateBinding(string Id, FlowBinding flowBinding, string flow)
        {
            ModelState.Remove("BindedFlow.Description");
            if (flowBinding.BindedFlow == null)
            {
                SaveErrorMessage("分装路线不能为空。");
                return Json(null);
            }
            else if (!flowBinding.JointOpReference.HasValue)
            {
                SaveErrorMessage("合装工序不能为空。");
                return Json(null);
            }
            else if (!flowBinding.LeadTime.HasValue)
            {
                SaveErrorMessage("下线提前期不能为空。");
                return Json(null);
            }
            else
            {
                FlowBinding updateFlowBinding = base.genericMgr.FindById<FlowBinding>(Convert.ToInt32(Id));
                updateFlowBinding.JointOpReference = flowBinding.JointOpReference;
                updateFlowBinding.LeadTime = flowBinding.LeadTime;
                flowBinding.BindedFlow = base.genericMgr.FindById<FlowMaster>(flowBinding.BindedFlow.Code);
                base.genericMgr.Update(updateFlowBinding);
                SaveSuccessMessage("修改成功");

                IList<FlowBinding> stockTakeItem = base.genericMgr.FindAll<FlowBinding>("from FlowBinding as s where s.MasterFlow.Code=?", flow);
                return PartialView(new GridModel(stockTakeItem));
            }
        }

        private SearchStatementModel PrepareBindSearchStatement(GridCommand command, FlowBindModel searchModel)
        {
            string whereStatement = " where mf.Code='" + searchModel.MasterFlow + "'";
            IList<object> param = new List<object>();

            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "BindedFlow.Code")
                {
                    command.SortDescriptors[0].Member = "mf.Code";
                }
                else if (command.SortDescriptors[0].Member == "BindedFlow.Description")
                {
                    command.SortDescriptors[0].Member = "mf.Description";
                }
            }

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountBindStatement;
            searchStatementModel.SelectStatement = selectBindStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }


        #endregion

        #region WorkCenter
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_AssemblyProductionFlow_Edit")]
        public ActionResult _ProdLineWorkCenter(GridCommand command, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            ViewBag.flow = id;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_AssemblyProductionFlow_View")]
        public ActionResult _AjaxProdLineWorkCenter(GridCommand command, string flow)
        {
            IList<ProdLineWorkCenter> prodLineWorkCenter = base.genericMgr.FindAll<ProdLineWorkCenter>("from ProdLineWorkCenter as p where p.Flow=?", flow);
            return PartialView(new GridModel(prodLineWorkCenter));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_AssemblyProductionFlow_View")]
        public ActionResult _InsertProdLineWorkCenter(ProdLineWorkCenter prodLineWorkCenter, string flow)
        {
            ModelState.Remove("Id");
            if (string.IsNullOrWhiteSpace(prodLineWorkCenter.WorkCenter))
            {
                SaveErrorMessage("工作中心不能为空。");
            }
            else
            {
                IList<ProdLineWorkCenter> prodLineWorkCenterList = base.genericMgr.FindAll<ProdLineWorkCenter>("from ProdLineWorkCenter as p where p.Flow=? and p.WorkCenter=? ", new object[] { flow, prodLineWorkCenter.WorkCenter });
                if (prodLineWorkCenterList.Count > 0)
                {
                    SaveErrorMessage(string.Format("工作中心{0}已经存在。", prodLineWorkCenterList.FirstOrDefault().WorkCenter));
                }
                else
                {
                    prodLineWorkCenter.Flow = flow;
                    base.genericMgr.Create(prodLineWorkCenter);
                    SaveSuccessMessage("添加成功。");
                }
            }
            IList<ProdLineWorkCenter> prodLineWorkCenters = base.genericMgr.FindAll<ProdLineWorkCenter>("from ProdLineWorkCenter as p where p.Flow=?", flow);
            return PartialView(new GridModel(prodLineWorkCenters));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_AssemblyProductionFlow_View")]
        public ActionResult _DeleteProdLineWorkCenter(string Id, string flow)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<ProdLineWorkCenter>(Convert.ToInt32(Id));
                SaveSuccessMessage("删除成功。");
            }
            IList<ProdLineWorkCenter> prodLineWorkCenters = base.genericMgr.FindAll<ProdLineWorkCenter>("from ProdLineWorkCenter as p where p.Flow=?", flow);
            return PartialView(new GridModel(prodLineWorkCenters));
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_AssemblyProductionFlow_View")]
        public ActionResult _UpdateProdLineWorkCenter(string Id, ProdLineWorkCenter prodLineWorkCenter, string flow)
        {
            // ModelState.Remove("BindedFlow.Description");
            if (string.IsNullOrWhiteSpace(prodLineWorkCenter.WorkCenter))
            {
                SaveErrorMessage("工作中心不能为空。");
            }
            else
            {
                ProdLineWorkCenter updateprodLineWorkCenter = base.genericMgr.FindById<ProdLineWorkCenter>(Convert.ToInt32(Id));
                updateprodLineWorkCenter.WorkCenter = prodLineWorkCenter.WorkCenter;
                base.genericMgr.Update(updateprodLineWorkCenter);
                SaveSuccessMessage("修改成功");
            }
            IList<ProdLineWorkCenter> prodLineWorkCenters = base.genericMgr.FindAll<ProdLineWorkCenter>("from ProdLineWorkCenter as p where p.Flow=?", flow);
            return PartialView(new GridModel(prodLineWorkCenters));
        }

        #endregion
    }
}
