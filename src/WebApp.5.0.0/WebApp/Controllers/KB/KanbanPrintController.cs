namespace com.Sconit.Web.Controllers.KB
{
    #region reference
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Security;
    using com.Sconit.Entity.KB;
    using com.Sconit.Service;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;
    using NHibernate.Type;
    #endregion

    public class KanbanPrintController : WebAppBaseController
    {
        [SconitAuthorize(Permissions = "Url_KanbanPrint_Print")]
        public ActionResult Print()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_KanbanPrint_Reception")]
        public ActionResult Reception()
        {
            return View();
        }
    }
}