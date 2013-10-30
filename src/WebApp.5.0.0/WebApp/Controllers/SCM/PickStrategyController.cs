using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using com.Sconit.Web.Util;
using com.Sconit.Entity.SCM;
using com.Sconit.Service;
using com.Sconit.Entity.SYS;
using com.Sconit.CodeMaster;

namespace com.Sconit.Web.Controllers.SCM
{
    public class PickStrategyController : WebAppBaseController
    {
        [GridAction]
        [SconitAuthorize(Permissions = "Url_PickStrategy_View")]
        public ActionResult Index()
        {

            IList<PickStrategy> pickStrategylist = base.genericMgr.FindAll<PickStrategy>();
            foreach (var item in pickStrategylist)
            {
                item.ShipStrategyDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.ShipStrategy, ((int)item.ShipStrategy).ToString());
                item.PickOddOptionDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.PickOddOption, ((int)item.OddOption).ToString());

            }
           
            return View(pickStrategylist);
        }

      
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_PickStrategy_View")]
        public ActionResult Edit(string id)
        {
            PickStrategy pickStrategy = base.genericMgr.FindById<PickStrategy>(id);
            return View(pickStrategy);
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_PickStrategy_View")]
        public ActionResult Edit(PickStrategy pickStrategy)
        {
            base.genericMgr.Update(pickStrategy);
            SaveSuccessMessage("修改成功");
            return RedirectToAction("Index");
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_PickStrategy_View")]
        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_PickStrategy_View")]
        public ActionResult New(PickStrategy pickStrategy)
        {
            if (ModelState.IsValid)
            {
                base.genericMgr.Create(pickStrategy);
                SaveSuccessMessage("添加成功");
                return RedirectToAction("Index");
            }
            else
            {
                return View(pickStrategy);
            }
        }
    }
}
