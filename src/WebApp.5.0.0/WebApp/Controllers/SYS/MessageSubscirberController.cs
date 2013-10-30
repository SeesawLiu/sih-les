using System.Data;
using System.Web.Mvc;
using com.Sconit.Web.Util;
using Telerik.Web.Mvc;
using System.Data.SqlClient;
using System;
using System.Linq;
using com.Sconit.Web.Models;
using com.Sconit.Entity.BatchJob.BAT;
using System.Collections.Generic;
using com.Sconit.Service;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Web.Controllers.SYS
{
    public class MessageSubscirberController : WebAppBaseController
    {
        private static string selectCountStatement = "select count(*) from MessageSubscirber as t";

        /// <summary>
        /// 
        /// </summary>
        private static string selectStatement = "select t from MessageSubscirber as t";


        public MessageSubscirberController()
        {

        }

        [SconitAuthorize(Permissions = "Url_MessageSubscirber_View")]
        public ActionResult Index()
        {
            ViewBag.PageSize = 50;
            //IList<MessageSubscirber> MessageSubscirberlist = base.genericMgr.FindAll<MessageSubscirber>();
            
            //IList<CodeDetail> codeDetail = systemMgr.GetCodeDetails(Sconit.CodeMaster.CodeMaster.TimeUnit);
            //foreach (CodeDetail codedet in codeDetail)
            //{
            //    codedet.Description = systemMgr.TranslateCodeDetailDescription(codedet.Description);
            //}
            //ViewData["CodeDetail"] = codeDetail;

            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxIndex(GridCommand command)
        {
            IList<MessageSubscirber> messageSubscirberlist = base.genericMgr.FindAll<MessageSubscirber>();
            return PartialView(new GridModel(messageSubscirberlist));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [SconitAuthorize(Permissions = "Url_MessageSubscirber_View")]
        public ActionResult UpdateAction(int? id, MessageSubscirber messageSubscirber)
        {
            try
            {
                ViewBag.PageSize = 50;
                this.genericMgr.Update(messageSubscirber);
                SaveSuccessMessage("修改成功。");
            }
            catch (Exception ex)
            {
                SaveErrorMessage("修改失败"+ex.Message);

            }
            
            IList<MessageSubscirber> MessageSubscirberlist = base.genericMgr.FindAll<MessageSubscirber>();
            return PartialView(new GridModel(MessageSubscirberlist));
        }


    }
}
