using System;
using System.Collections.Generic;
using System.Web.Mvc;
using com.Sconit.CodeMaster;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.SYS;
using com.Sconit.Web.Util;
using com.Sconit.Entity.INV;
using com.Sconit.Service;
using com.Sconit.Entity.ORD;
using com.Sconit.Utility.Report;

namespace com.Sconit.Web.Controllers.INV
{
    public class TransferCabController : WebAppBaseController
    {
        public IFlowMgr flowMgr { get; set; }
        public IOrderMgr orderMgr { get; set; }
        public IHuMgr huMgr { get; set; }
        public IReportGen reportGen { get; set; }
        public ISecurityMgr securityMgr { get; set; }

        [SconitAuthorize(Permissions = "Url_TransferCab_View")]
        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_TransferCab_View")]
        public JsonResult HuScan(string HuId)
        {
            try
            {
                // 驾驶室生产单号
                var snRules = genericMgr.FindById<SNRule>((int)DocumentsType.ORD_Production);
                if (HuId.StartsWith(snRules.PreFixed, StringComparison.OrdinalIgnoreCase))
                {
                    var orderList = genericMgr.FindAll<OrderMaster>("from OrderMaster o where o.OrderNo=? and o.Type=?", new object[] { HuId, OrderType.Production });
                    if (orderList.Count == 0)
                    {
                        throw new BusinessException("订单{0}不存在。");
                    }
                    var order = orderList[0];
                    return Json(new { OrderNo = order.OrderNo, TraceCode = order.TraceCode });
                }
                else
                {
                    //条码
                    IList<Hu> huList = base.genericMgr.FindAll<Hu>(" from Hu where HuId=?", HuId);
                    if (huList.Count == 0)
                    {
                        throw new BusinessException(Resources.ORD.TransferOrder.TransferOrder_NotExistHuId, HuId);
                    }
                    var hu = huList[0];
                    return Json(new { HuId = HuId, Item = hu.Item, ItemDescription = hu.ItemDescription });
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

        [SconitAuthorize(Permissions = "Url_TransferCab_View")]
        public ActionResult CreateTransferCab(string orderNo, string flow, string huId)
        {
            try
            {
                orderMgr.TansferCab(orderNo, flow, huId);
                SaveSuccessMessage("{0} 移库成功！", orderNo);
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
            return Json(null);
        }
    }
}
