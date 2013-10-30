using System;
using System.Web.Services;
using com.Sconit.Entity;
using com.Sconit.Service;

namespace com.Sconit.WebService
{
    [WebService(Namespace = "http://com.Sconit.WebService.OrderService/")]
    public class OrderService : BaseWebService
    {
        private IOrderMgr orderManager
        {
            get
            {
                return GetService<IOrderMgr>();
            }
        }

        private IKanbanScanOrderMgr kanbanScanOrderMgr
        {
            get
            {
                return GetService<IKanbanScanOrderMgr>();
            }
        }

        [WebMethod]
        public void AutoCloseOrder(string userCode)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));
            orderManager.AutoCloseOrder();
        }

        [WebMethod]
        public void AutoCloseASN(string userCode, DateTime dateTime)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));
            orderManager.AutoCloseASN(dateTime);
        }

        [WebMethod]
        public void OutCab(string orderNo, string userCode)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));
            orderManager.OutCab(orderNo);
        }

        [WebMethod]
        public void TansferCab(string orderNo, string flowCode, string qualityBarcode, string userCode)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));
            orderManager.TansferCab(orderNo, flowCode, qualityBarcode);
        }

        [WebMethod]
        public void CancelReportOrderOp(int orderOpReportId, string userCode)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));
            orderManager.CancelReportOrderOp(orderOpReportId);
        }

        [WebMethod]
        public void BackFlushVanOrder(string userCode)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));
            GetService<IProductionLineMgr>().BackFlushVanOrder();
        }

        //[WebMethod]
        //public void MakeupReceiveOrder()
        //{
        //    orderManager.MakeupReceiveOrder();
        //}

        //[WebMethod]
        //public void MakeupReportOrderOp()
        //{
        //    orderManager.MakeupReportOrderOp();
        //}

        //[WebMethod]
        //public void MakeupCancelReportOrderOp()
        //{
        //    orderManager.MakeupCancelReportOrderOp();
        //}

        [WebMethod]
        public void RunLeanEngine(string userCode)
        {
            //SecurityContextHolder.Set(securityMgr.GetUser(userCode));
            //orderManager.RunLeanEngine();
        }

        [WebMethod]
        public void AutoGenAnDonOrder(string userCode)
        {
            Entity.ACC.User user = securityMgr.GetUser(userCode);
            SecurityContextHolder.Set(user);
            kanbanScanOrderMgr.AutoGenAnDonOrder(user);
        }

        //[WebMethod]
        //public void MakeUpReceiveIp(string ipNo, string userCode)
        //{
        //    Entity.ACC.User user = securityMgr.GetUser(userCode);
        //    SecurityContextHolder.Set(user);
        //    IList<IpDetail> ipDetailList = GetService<IGenericMgr>().FindAll<IpDetail>("from IpDetail where IpNo = ?", ipNo);
        //    foreach (IpDetail ipDetail in ipDetailList)
        //    {
        //        if (ipDetail.IpDetailInputs != null && ipDetail.IpDetailInputs.Count > 0)
        //        {
        //            foreach (IpDetailInput ipDetailInput in ipDetail.IpDetailInputs)
        //            {
        //                ipDetailInput.ReceiveQty = ipDetailInput.ShipQty;
        //            }
        //        }
        //        else
        //        {
        //            IpDetailInput ipDetailInput = new IpDetailInput();
        //            ipDetailInput.ReceiveQty = ipDetail.Qty;
        //            ipDetail.AddIpDetailInput(ipDetailInput);
        //        }
        //    }
        //    orderManager.ReceiveIp(ipDetailList);
        //}
    }
}