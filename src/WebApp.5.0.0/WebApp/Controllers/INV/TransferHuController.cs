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

namespace com.Sconit.Web.Controllers.INV
{
    public class TransferHuController : WebAppBaseController
    {
        public IFlowMgr flowMgr { get; set; }
        public IOrderMgr orderMgr { get; set; }
        public IHuMgr huMgr { get; set; }
        public IReportGen reportGen { get; set; }

        [SconitAuthorize(Permissions = "Url_TransferHu_View")]
        public ActionResult New()
        {
            TempData["Hu"] = new List<Hu>();
            return View();
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_TransferHu_View")]
        public JsonResult HuScan(string HuId)
        {
            IList<Hu> hulist = (IList<Hu>)TempData["Hu"];
            try
            {
                if (hulist != null)
                {
                    var q = hulist.Where(v => v.HuId == HuId).ToList();
                    if (q.Count > 0)
                    {
                        throw new BusinessException(@Resources.ORD.TransferHu.TransferHu_ExceptionHuid);
                    }
                }
                IList<Hu> hu = base.genericMgr.FindAll<Hu>(" from Hu where HuId=?", HuId);

                if (hu.Count == 0)
                {
                    throw new BusinessException(Resources.ORD.TransferOrder.TransferOrder_NotExistHuId, HuId);
                }

                hulist.Add(hu[0]);
                TempData["Hu"] = hulist;
                return Json(new { });
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }
            TempData["Hu"] = hulist;
            return Json(null);
        }
        [GridAction]
        [SconitAuthorize(Permissions = "Url_TransferHu_View")]
        public ActionResult _SelectTransferHuDetail()
        {
            IList<Hu> huList = new List<Hu>();
            if (TempData["Hu"] != null)
            {
                huList = (IList<Hu>)TempData["Hu"];
            }
            TempData["Hu"] = huList;
            return View(new GridModel(huList));
        }

        public ActionResult _ViewHuDetailList()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_TransferHu_View")]
        public ActionResult _DeleteHuDetail(string id)
        {
            IList<Hu> hu = (IList<Hu>)TempData["Hu"];
            var q = hu.Where(v => v.HuId != id).ToList();
            TempData["Hu"] = q;
            return View(new GridModel(q));
        }
        [GridAction]
        [SconitAuthorize(Permissions = "Url_TransferHu_View")]
        public ActionResult CreateTransferHu(string effectiveDate, string Flow)
        {
            IList<Hu> hulist = null;
            IList<string> strList = new List<string>();
            try
            {
                hulist = (IList<Hu>)TempData["Hu"];
                if (hulist == null || hulist.Count == 0)
                {
                    throw new BusinessException(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.ORD.TransferHu.TransferHu_HuId);
                }
                foreach (Hu hu in hulist)
                {
                    strList.Add(hu.HuId);
                }

                string Order = orderMgr.CreateHuTransferOrder(Flow, strList, DateTime.Now);

                SaveSuccessMessage("移库单号：{0}生成成功！", Order);
                return Json(new { });
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }
            TempData["Hu"] = hulist;
            return Json(null);
        }

    }


}
