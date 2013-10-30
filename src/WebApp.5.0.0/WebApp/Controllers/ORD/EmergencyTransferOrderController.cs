
namespace com.Sconit.Web.Controllers.ORD
{
    using System.Collections.Generic;
    using System.Web.Mvc;
    using com.Sconit.Service;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;
    using System;
    using AutoMapper;
    using com.Sconit.Entity.MD;
    using NHibernate.Criterion;
    using com.Sconit.Entity.Exception;
    using com.Sconit.Utility;
    using Telerik.Web.Mvc.UI;
    using System.Collections;
    using System.Web;


    public class EmergencyTransferOrderController : WebAppBaseController
    {
        public IIpMgr ipMgr { get; set; }
        public IOrderMgr orderMgr { get; set; }
        public IStockTakeMgr stockTakeMgr { get; set; }
        public EmergencyTransferOrderController()
        {
        }

        [SconitAuthorize(Permissions = "Url_EmergencyTransferOrder_View")]
        public ActionResult Index()
        {
           
            return View();
        }
        [SconitAuthorize(Permissions = "Url_EmergencyTransferOrder_View")]
        public ActionResult ImportEmergencyFeederDetail(IEnumerable<HttpPostedFileBase> attachments)
        {
            try
            {

                foreach (var file in attachments)
                {
                    string[] strArray = orderMgr.CreateEmTransferOrderFromXls(file.InputStream);
                   object obj=null;
                   if (strArray[0] == "" && strArray[1] == "")
                     throw new  BusinessException( "没有生产要货单，导入数据错误");
                   else
                       obj = "要货单号:" + strArray[0] + "生成成功，物料代码" + strArray[1] + "没有导入";
                    return Json(new { status = obj }, "text/plain");
                }
            }
            catch (BusinessException ex)
            {
                Response.Write(ex.GetMessages()[0].GetMessageString());
            }
            return Content("");
         
        }


      
    }
}
