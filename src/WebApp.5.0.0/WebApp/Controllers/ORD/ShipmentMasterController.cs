using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using com.Sconit.Entity.Exception;
using com.Sconit.Web.Util;
using Telerik.Web.Mvc;
using com.Sconit.Web.Models;
using com.Sconit.Entity.INV;
using com.Sconit.Entity.VIEW;
using com.Sconit.Service;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.ORD;
using AutoMapper;
using com.Sconit.Utility.Report;
using com.Sconit.Utility;
using com.Sconit.Entity;
using com.Sconit.Web.Models.SearchModels.ORD;

namespace com.Sconit.Web.Controllers.ORD
{
    public class ShipmentMasterController : WebAppBaseController
    {
        public IFlowMgr flowMgr { get; set; }
        public IOrderMgr orderMgr { get; set; }
        public IHuMgr huMgr { get; set; }
        public IReportGen reportGen { get; set; }
        public IShipmentMgr shipmentMgr { get; set; }
        public INumberControlMgr numberControlMgr { get; set; }

        private static string selectStatement = "select s from ShipmentMaster as s";

        private static string selectCountStatement = "select count(*) from ShipmentMaster as s";

        [SconitAuthorize(Permissions = "Url_ShipmentMaster_View")]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GooutIndex()
        {
            return View();
        }


        public ActionResult ShipmentIndex()
        {
            return View();
        }



        [SconitAuthorize(Permissions = "Url_ShipmentMaster_View")]
        public ActionResult New()
        {
            TempData["IpMaster"] = new List<IpMaster>();
            ViewBag.CreateType = "0";
            return View();
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_ShipmentMaster_View")]
        public void IpMastrScan(string IpNo)
        {
            //手动扫描进去
            IList<IpMaster> ipMasterlist = (IList<IpMaster>)TempData["IpMaster"];
            try
            {


                if (ipMasterlist != null)
                {
                    if (ipMasterlist.Select(i => i.IpNo).Contains(IpNo))
                    {
                        throw new BusinessException(@Resources.ORD.ShipmentMaster.ShipmentMaster_ExceptionIpNo);
                    }
                    else
                    {
                        IList<IpMaster> ipMasterEntityList = base.genericMgr.FindAll<IpMaster>(" from IpMaster i where  i.IpNo=? and i.OrderType in(" + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.Distribution + ")  ", IpNo);



                        if (ipMasterEntityList.Count == 0)
                        {
                            throw new BusinessException(@Resources.ORD.ShipmentMaster.ShipmentMaster_IpNoIsNullOrEmpty);
                        }

                        foreach (var IpMasterEntity in ipMasterlist)
                        {
                            Region regin = base.genericMgr.FindById<Region>(IpMasterEntity.PartyFrom);
                            Region reginTo = base.genericMgr.FindById<Region>(ipMasterEntityList[0].PartyFrom);
                            if (regin.Workshop != reginTo.Workshop)
                            {
                                throw new BusinessException(@Resources.ORD.ShipmentMaster.ThisListWithYouOnSingleBranchDifferentCannotAdd);
                            }
                        }


                        var ipMaster = ipMasterEntityList[0];

                        if (ipMaster.Status != CodeMaster.IpStatus.Submit)
                        {
                            throw new BusinessException(@Resources.ORD.ShipmentMaster.ShipmentMaster_ShipmentMasterOnSubmitState);
                        }
                        else
                        {
                            var billofLadingDetailList = base.genericMgr.FindAll<ShipmentDetail>(" from ShipmentDetail where IpNo = ? ", IpNo);


                            if (billofLadingDetailList != null && billofLadingDetailList.Count() > 0)
                            {
                                foreach (var billofLadingDetail in billofLadingDetailList)
                                {
                                    var billofLadingMaster = base.genericMgr.FindById<ShipmentMaster>(billofLadingDetail.ShipmentNo);
                                    if (billofLadingMaster.Status == com.Sconit.CodeMaster.BillMasterStatus.Submit || billofLadingMaster.Status == com.Sconit.CodeMaster.BillMasterStatus.Close || billofLadingMaster.Status == com.Sconit.CodeMaster.BillMasterStatus.Create)
                                    {
                                        throw new BusinessException(@Resources.ORD.ShipmentMaster.ShipmentMaster_HaveCreatedTheWaybill);
                                    }
                                    else
                                    {
                                        ipMaster.IpMasterStatusDescription = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.IpStatus, Convert.ToInt32(ipMaster.Status));
                                        ipMaster.IpMasterTypeDescription = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.OrderType, Convert.ToInt32(ipMaster.OrderType));
                                        ipMasterlist.Add(ipMaster);
                                    }
                                }
                            }
                            else
                            {
                                ipMaster.IpMasterStatusDescription = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.IpStatus, Convert.ToInt32(ipMaster.Status));
                                ipMaster.IpMasterTypeDescription = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.OrderType, Convert.ToInt32(ipMaster.OrderType));
                                ipMasterlist.Add(ipMaster);
                            }
                        }

                    }
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
            TempData["IpMaster"] = ipMasterlist;
        }
        [GridAction]
        [SconitAuthorize(Permissions = "Url_ShipmentMaster_View")]
        public ActionResult _AjaxSelectIpMasterList()
        {
            IList<IpMaster> ipMasterList = new List<IpMaster>();
            if (TempData["IpMaster"] != null)
            {
                ipMasterList = (IList<IpMaster>)TempData["IpMaster"];
            }
            TempData["IpMaster"] = ipMasterList;
            return View(new GridModel(ipMasterList));
        }

        public ActionResult _IpMasterList()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_ShipmentMaster_View")]
        public ActionResult _AjaxDeleteIpMasterList(string id)
        {
            IList<IpMaster> ipMaster = (IList<IpMaster>)TempData["IpMaster"];
            var q = ipMaster.Where(i => i.IpNo != id).ToList();
            TempData["IpMaster"] = q;
            return View(new GridModel(q));
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ShipmentMaster_View")]
        public ActionResult _AjaxSearchIpMasterList(string Flow, string PartyFrom, string PartyTo)
        {
            IList<object> param = new List<object>();
            param.Add(com.Sconit.CodeMaster.IpStatus.Submit);
            string hql = " from IpMaster as i where i.Status=? and i.OrderType in(" + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.Distribution + ") ";
            if (!string.IsNullOrEmpty(Flow))
            {
                hql += " and i.Flow=?";
                param.Add(Flow);
            }
            if (!string.IsNullOrEmpty(PartyFrom))
            {
                hql += " and i.PartyFrom=?";
                param.Add(PartyFrom);
            }
            if (!string.IsNullOrEmpty(PartyTo))
            {
                hql += " and i.PartyTo=?";
                param.Add(PartyTo);
            }

            IList<IpMaster> ipMasterList = base.genericMgr.FindAll<IpMaster>(hql, param.ToArray());

            if (ipMasterList.Count < 1)
            {
                return View(new GridModel(new List<IpMaster>()));
            }

            IList<IpMaster> ipMasterListView = new List<IpMaster>();
            string whereIpNoStr = string.Empty;
            foreach (var ipMaster in ipMasterList)
            {
                if (whereIpNoStr == string.Empty)
                {
                    whereIpNoStr = "'" + ipMaster.IpNo + "'";
                }
                else
                {
                    whereIpNoStr += ",'" + ipMaster.IpNo + "'";
                }
            }

            hql = " select  d from ShipmentDetail as d  where exists (select 1 from ShipmentMaster  as m where m.Status not in ("
                + (int)com.Sconit.CodeMaster.BillMasterStatus.Cancel + ")  and m.ShipmentNo=d.ShipmentNo ) and d.IpNo in (" + whereIpNoStr + ")";
            IList<ShipmentDetail> ShipmentDetailList = base.genericMgr.FindAll<ShipmentDetail>(hql);
            foreach (var ipMaster in ipMasterList)
            {
                var q = ShipmentDetailList.Where(ip => ip.IpNo == ipMaster.IpNo);
                if (q == null || q.Count() == 0)
                {
                    ipMaster.IpMasterStatusDescription = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.IpStatus, Convert.ToInt32(ipMaster.Status));
                    ipMaster.IpMasterTypeDescription = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.OrderType, Convert.ToInt32(ipMaster.OrderType));
                    ipMasterListView.Add(ipMaster);
                }

            }


            return View(new GridModel(ipMasterListView));
        }
        [SconitAuthorize(Permissions = "Url_ShipmentMaster_View")]
        public ActionResult _SearchIpMasterList()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_ShipmentMaster_View")]
        public JsonResult CreateShipmentMaster(string VehicleNo, string Driver, string Shipper, string CaseQty, string AddressTo)
        {
            IList<IpMaster> ipMasterList = (IList<IpMaster>)TempData["IpMaster"];
            try
            {
                if (ipMasterList.Count == 0)
                {
                    throw new BusinessException("至少扫描一张送货单！");
                }
                ShipmentMaster shipmentMaster = new ShipmentMaster();
                shipmentMaster.Shipper = Shipper;
                shipmentMaster.AddressTo = AddressTo;
                shipmentMaster.ShipmentNo = numberControlMgr.GetBillOfLadingNo();
                shipmentMaster.CaseQty = CaseQty == "" ? 0 : Convert.ToInt32(CaseQty);
                shipmentMaster.VehicleNo = VehicleNo;
                shipmentMaster.Driver = Driver;
                shipmentMaster.Status = com.Sconit.CodeMaster.BillMasterStatus.Create;


                IList<ShipmentDetail> shipmentDetailList = new List<ShipmentDetail>();
                foreach (IpMaster ipMaster in ipMasterList)
                {
                    ShipmentDetail biDetail = new ShipmentDetail();
                    biDetail.ShipmentNo = shipmentMaster.ShipmentNo;
                    biDetail.IpNo = ipMaster.IpNo;
                    shipmentDetailList.Add(biDetail);

                }
                shipmentMaster.WorkShop = base.genericMgr.FindById<Region>(ipMasterList[0].PartyFrom).Workshop;
                shipmentMaster.ShipmentDetails = shipmentDetailList;


                shipmentMgr.CreateBillofLadingMaster(shipmentMaster);
                SaveSuccessMessage("运单号：" + shipmentMaster.ShipmentNo + "生成成功！");
                return Json(new { ShipmentNo = shipmentMaster.ShipmentNo });
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


        [SconitAuthorize(Permissions = "Url_ShipmentMaster_View")]
        public ActionResult checkdCreateShipmentMaster(string checkedIpNos, string VehicleNo, string Driver, string Shipper, string CaseQty, string AddressTo)
        {

            string[] checkeIpNoArray = checkedIpNos.Split(',');


            string selectStatement = string.Empty;
            IList<object> param = new List<object>();
            foreach (var IpNo in checkeIpNoArray)
            {
                if (selectStatement == string.Empty)
                {
                    selectStatement = "from IpMaster where IpNo in (?";
                }
                else
                {
                    selectStatement += ",?";
                }
                param.Add(IpNo);
            }
            selectStatement += ")";

            IList<IpMaster> ipMasterList = base.genericMgr.FindAll<IpMaster>(selectStatement, param.ToArray());

            List<Region> regionList = new List<Region>();
            foreach (var IpMasterEntity in ipMasterList)
            {
                Region region = base.genericMgr.FindById<Region>(IpMasterEntity.PartyFrom);
                regionList.Add(region);
            }

            if (regionList.Select(r => r.Workshop).Distinct().Count() > 1)
            {
                throw new BusinessException("所选送货单的分厂不一致，请从新选择");
            }
            ShipmentMaster shipmentMaster = new ShipmentMaster();
            shipmentMaster.WorkShop = regionList[0].Workshop;
            shipmentMaster.Shipper = Shipper;
            shipmentMaster.AddressTo = AddressTo;
            shipmentMaster.ShipmentNo = numberControlMgr.GetBillOfLadingNo();
            shipmentMaster.CaseQty = CaseQty == "" ? 0 : Convert.ToInt32(CaseQty);
            shipmentMaster.VehicleNo = VehicleNo;
            shipmentMaster.Driver = Driver;
            shipmentMaster.Status = com.Sconit.CodeMaster.BillMasterStatus.Create;




            IList<ShipmentDetail> shipmentDetaillList = new List<ShipmentDetail>();
            foreach (IpMaster ipMaster in ipMasterList)
            {
                ShipmentDetail shipmentDetail = new ShipmentDetail();
                shipmentDetail.ShipmentNo = shipmentMaster.ShipmentNo;
                shipmentDetail.IpNo = ipMaster.IpNo;
                shipmentDetaillList.Add(shipmentDetail);
            }
            shipmentMaster.ShipmentDetails = shipmentDetaillList;

            try
            {
                shipmentMgr.CreateBillofLadingMaster(shipmentMaster);
                SaveSuccessMessage("运单号：" + shipmentMaster.ShipmentNo + "生成成功！");
                return Json(new { ShipmentNo = shipmentMaster.ShipmentNo });
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

        [SconitAuthorize(Permissions = "Url_ShipmentMaster_View")]
        public ActionResult List(GridCommand command, ShipmentMasterSearchModel searchModel)
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
        [SconitAuthorize(Permissions = "Url_ShipmentMaster_Close")]
        public ActionResult GooutList(GridCommand command, ShipmentMasterSearchModel searchModel)
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
        [SconitAuthorize(Permissions = "Url_BillofLadingMaster_View")]
        public ActionResult _AjaxList(GridCommand command, ShipmentMasterSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<ShipmentMaster>()));
            }


            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<ShipmentMaster>(searchStatementModel, command));
        }

        #region private method
        private SearchStatementModel PrepareSearchStatement(GridCommand command, ShipmentMasterSearchModel searchModel)
        {

            IList<object> param = new List<object>();

            string whereStatement = string.Empty;
            //SecurityHelper.AddPartyFromPermissionStatement(ref whereStatement, "i", "PartyFrom", com.Sconit.CodeMaster.OrderType.Procurement, false);
            //SecurityHelper.AddPartyToPermissionStatement(ref whereStatement, "i", "PartyTo", com.Sconit.CodeMaster.OrderType.Procurement);
            //SecurityHelper.AddPartyFromAndPartyToPermissionStatement(ref whereStatement, "i", "OrderType", "i", "PartyFrom", "i", "PartyTo", com.Sconit.CodeMaster.OrderType.Procurement, false);
            if (searchModel.IsGoout)
            {
                HqlStatementHelper.AddEqStatement("Status", com.Sconit.CodeMaster.BillMasterStatus.Submit, "s", ref whereStatement, param);
            }
            HqlStatementHelper.AddLikeStatement("ShipmentNo", searchModel.ShipmentNo, HqlStatementHelper.LikeMatchMode.Anywhere, "s", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Shipper", searchModel.Shipper, HqlStatementHelper.LikeMatchMode.Start, "s", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Driver", searchModel.Driver, HqlStatementHelper.LikeMatchMode.Anywhere, "s", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("VehicleNo", searchModel.VehicleNo, HqlStatementHelper.LikeMatchMode.Anywhere, "s", ref whereStatement, param);


            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.StartDate, searchModel.EndDate, "s", ref whereStatement, param);
            }
            else if (searchModel.StartDate != null & searchModel.EndDate == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.StartDate, "s", ref whereStatement, param);
            }
            else if (searchModel.StartDate == null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.EndDate, "s", ref whereStatement, param);
            }


            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            if (command.SortDescriptors.Count == 0)
            {
                if (searchModel.IsGoout)
                {
                    sortingStatement = " order by s.CreateDate desc";
                }
                else
                {
                    sortingStatement = " order by s.CreateDate asc";
                }
            }

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }



        #endregion
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ShipmentMaster_View")]
        public ActionResult _AjaxShipmentDetailList(string ShipmentNo)
        {
            IList<ShipmentDetail> shipmentDetailList = base.genericMgr.FindAll<ShipmentDetail>(" from ShipmentDetail s where s.ShipmentNo=?", ShipmentNo);
            IList<IpMaster> ipMasterList = new List<IpMaster>();
            foreach (var billofLadingDetail in shipmentDetailList)
            {
                IpMaster ipMaster = base.genericMgr.FindById<IpMaster>(billofLadingDetail.IpNo);
                ipMaster.ShipmentNo = billofLadingDetail.ShipmentNo;
                ipMaster.IpMasterStatusDescription = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.IpStatus, Convert.ToInt32(ipMaster.Status));
                ipMaster.IpMasterTypeDescription = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.OrderType, Convert.ToInt32(ipMaster.OrderType));
                ipMasterList.Add(ipMaster);
            }
            return View(new GridModel(ipMasterList));

        }
        [SconitAuthorize(Permissions = "Url_ShipmentMaster_View")]
        public ActionResult _BillofLadingDetailList(string ShipmentNo)
        {
            ViewBag.ShipmentNo = ShipmentNo;
            return View();
        }
        [SconitAuthorize(Permissions = "Url_ShipmentMaster_View")]
        public ActionResult _Edit(string ShipmentNo)
        {
            ShipmentMaster shipmentMaster = base.genericMgr.FindById<ShipmentMaster>(ShipmentNo);
            shipmentMaster.StatusDescription = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.BillMasterStatus, (int)shipmentMaster.Status);
            return View(shipmentMaster);

        }

        [SconitAuthorize(Permissions = "Url_ShipmentMaster_View")]
        public ActionResult _ShipmentEdit(string ShipmentNo)
        {
            ShipmentMaster shipmentMaster = base.genericMgr.FindById<ShipmentMaster>(ShipmentNo);
            shipmentMaster.StatusDescription = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.BillMasterStatus, (int)shipmentMaster.Status);
            return View(shipmentMaster);

        }
        [SconitAuthorize(Permissions = "Url_ShipmentMaster_Release")]
        public string Checkouttext(string ShipmentNo)
        {
            if (string.IsNullOrEmpty(ShipmentNo))
            {
                return "错误：运单号不能为空。";
            }
            IList<ShipmentMaster> shipmentMasterList = base.genericMgr.FindAll<ShipmentMaster>(" from ShipmentMaster where ShipmentNo=?", ShipmentNo);
            if (shipmentMasterList.Count == 0)
            {
                return "错误：运单号不存在。";
            }
            if (shipmentMasterList[0].Status != com.Sconit.CodeMaster.BillMasterStatus.Submit)
            {
                return "错误：运单号不是释放状态。";
            }
            shipmentMasterList[0].StatusDescription = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.BillMasterStatus, (int)shipmentMasterList[0].Status);
            return "";
        }

        [SconitAuthorize(Permissions = "Url_ShipmentMaster_View")]
        public ActionResult _GooutEdit(string ShipmentNo)
        {
            ShipmentMaster shipmentMaster = base.genericMgr.FindById<ShipmentMaster>(ShipmentNo);
            shipmentMaster.StatusDescription = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.BillMasterStatus, (int)shipmentMaster.Status);
            return View(shipmentMaster);

        }
        [SconitAuthorize(Permissions = "Url_ShipmentMaster_View")]
        public ActionResult _Delete(string id)
        {

            try
            {
                ShipmentMaster shipmentMaster = base.genericMgr.FindById<ShipmentMaster>(id);
                shipmentMaster.ShipmentDetails = base.genericMgr.FindAll<ShipmentDetail>(" from ShipmentDetail s where s.ShipmentNo=? ", id);
                shipmentMgr.DeleteBillofLadingMaster(shipmentMaster);

                SaveSuccessMessage(id + "删除成功");
                return RedirectToAction("List");
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }
            return RedirectToAction("_Edit", new { ShipmentNo = id });
        }


        [SconitAuthorize(Permissions = "Url_ShipmentMaster_View")]
        public ActionResult _Cancel(string id)
        {
            try
            {
                ShipmentMaster shipmentMaster = base.genericMgr.FindById<ShipmentMaster>(id);
                shipmentMaster.Status = com.Sconit.CodeMaster.BillMasterStatus.Cancel;
                base.genericMgr.Update(shipmentMaster);

                SaveSuccessMessage(id + "取消成功");
                return RedirectToAction("List");
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }
            return RedirectToAction("_Edit", new { ShipmentNo = id });
        }


        [SconitAuthorize(Permissions = "Url_ShipmentMaster_View")]
        public ActionResult _Submit(string id)
        {
            try
            {
                ShipmentMaster shipmentMaster = base.genericMgr.FindById<ShipmentMaster>(id);
                shipmentMaster.Status = com.Sconit.CodeMaster.BillMasterStatus.Submit;
                shipmentMaster.SubmitDate = DateTime.Now;
                shipmentMaster.SubmitUserId = SecurityContextHolder.Get().Id;
                shipmentMaster.SubmitUserName = SecurityContextHolder.Get().FullName;
                base.genericMgr.Update(shipmentMaster);
                SaveSuccessMessage(id + "释放成功");
                return RedirectToAction("List");
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }

            return RedirectToAction("_Edit", new { ShipmentNo = id });
        }
        [SconitAuthorize(Permissions = "Url_ShipmentMaster_Close")]
        public JsonResult BillofLadingMasterFindId(string ShipmentNo)
        {
            try
            {
                if (string.IsNullOrEmpty(ShipmentNo))
                {
                    throw new BusinessException("运单号不能为空");
                }

                IList<ShipmentMaster> shipmentMasterList = base.genericMgr.FindAll<ShipmentMaster>(" from ShipmentMaster s where s.ShipmentNo=?", ShipmentNo);

                if (shipmentMasterList.Count == 0)
                {
                    throw new BusinessException("您输入的运单号错误");
                }
                if (shipmentMasterList[0].Status == com.Sconit.CodeMaster.BillMasterStatus.Close)
                {
                    throw new BusinessException("运单应经出门，如果想重新进行出门请到查询中取消");
                }
                return Json(shipmentMasterList[0]);
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
        [SconitAuthorize(Permissions = "Url_ShipmentMaster_Close")]
        public ActionResult _GooutSearch()
        {
            return View();
        }
        public ActionResult ShipmentMasterClose(string ShipmentNo, string PassPerson)
        {
            try
            {
                ShipmentMaster shipmentMaster = base.genericMgr.FindById<ShipmentMaster>(ShipmentNo);
                shipmentMaster.Status = com.Sconit.CodeMaster.BillMasterStatus.Close;
                shipmentMaster.PassPerson = PassPerson;
                shipmentMaster.PassDate = DateTime.Now;
                shipmentMaster.PassUserId = SecurityContextHolder.Get().Id;
                base.genericMgr.Update(shipmentMaster);

                object obj = ShipmentNo + "放行成功！";
                return Json(new { Alter = obj }, "text/plain");
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

        public string Print(string ShipmentNo)
        {
            ShipmentMaster ShipmentMaster = base.genericMgr.FindById<ShipmentMaster>(ShipmentNo);
            IList<ShipmentDetail> shipmentDetailList = base.genericMgr.FindAll<ShipmentDetail>(" from ShipmentDetail s where s.ShipmentNo=?", ShipmentNo);
            IList<IpMaster> ipMasterList = new List<IpMaster>();
            foreach (var billofLadingDetail in shipmentDetailList)
            {
                IpMaster ipMaster = base.genericMgr.FindById<IpMaster>(billofLadingDetail.IpNo);
                ipMaster.ShipmentNo = billofLadingDetail.ShipmentNo;
                ipMaster.IpMasterStatusDescription = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.IpStatus, Convert.ToInt32(ipMaster.Status));
                ipMaster.IpMasterTypeDescription = systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.OrderType, Convert.ToInt32(ipMaster.OrderType));
                ipMasterList.Add(ipMaster);
            }
            ShipmentMaster.ipMasters = ipMasterList;
            //PrintShipmentMaster PrintShipmentMstr = Mapper.Map<ShipmentMaster, PrintShipmentMaster>(ShipmentMaster);
            IList<object> data = new List<object>();
            data.Add(ShipmentMaster);
            data.Add(ShipmentMaster.ipMasters);
            string reportFileUrl = reportGen.WriteToFile("ShipmentMaster.xls", data);
            //reportGen.WriteToClient(orderMaster.OrderTemplate, data, orderMaster.OrderTemplate);

            return reportFileUrl;
            //reportGen.WriteToFile(orderMaster.OrderTemplate, data);
        }

    }


}
