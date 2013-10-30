using System.Web.Mvc;
using com.Sconit.Web.Util;
using System.Linq;

/// <summary>
///MainController 的摘要说明
/// </summary>
namespace com.Sconit.Web.Controllers
{
    [SconitAuthorize]
    public class MainController : WebAppBaseController
    {
        public MainController()
        {

        }

        public ActionResult Default()
        {
            //if (Request.Cookies[WebConstants.CookieMainPageUrlKey] != null && this.CurrentUser != null)
            //{
            //    string mainPage = Request.Cookies[WebConstants.CookieMainPageUrlKey].Values[this.CurrentUser.Code];
            //    if (string.IsNullOrWhiteSpace(mainPage))
            //    {
            //        ViewBag.MainPageUrl = "../UserFavorite/Index";
            //    }
            //    else
            //    {
            //        ViewBag.MainPageUrl = mainPage;
            //    }
            //}
            //else
            //{
            //    ViewBag.MainPageUrl = "../UserFavorite/Index";
            //}
            return View();
        }

        public ActionResult Top()
        {
            ViewBag.IsShowImage = true;
            var systemFlag = systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.SystemFlag);
            ViewBag.IsShow = systemFlag == "1";
            return View();
        }

        public ActionResult Nav()
        {
            ViewBag.UserCode = this.CurrentUser.CodeDescription;
            return PartialView(base.GetAuthrizedMenuTree());
        }

        public ActionResult Switch()
        {
            return View();
        }

        public ActionResult Main()
        {
            if(Request.Cookies[WebConstants.CookieMainPageUrlKey] != null && this.CurrentUser != null)
            {
                string mainPage = Request.Cookies[WebConstants.CookieMainPageUrlKey].Values[this.CurrentUser.Code];
                if(string.IsNullOrWhiteSpace(mainPage))
                {
                    ViewBag.MainPageUrl = "/UserFavorite/Index";
                }
                else
                {

                    ViewBag.MainPageUrl = mainPage;
                }
            }
            else
            {
                ViewBag.MainPageUrl = "/UserFavorite/Index";
            }
            var menu = systemMgr.GetAllMenu().Where(p => p.PageUrl != null && p.PageUrl.EndsWith(ViewBag.MainPageUrl)).First();
            ViewBag.MainPageName = menu.Description;
            //string name = Resources.Menu.ResourceManager.GetString(ViewBag.MainPageName);
            //if(name != null)
            //{
            //    ViewBag.MainPageName = name;
            //}
            if(string.IsNullOrWhiteSpace(ViewBag.MainPageName))
            {
                ViewBag.MainPageName = menu.Name;
            }
            ViewBag.MainPageCode = menu.Code;
            return View();
        }
    }
}