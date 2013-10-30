/// <summary>
/// Summary description for AccountController
/// </summary>
namespace com.Sconit.Web.Controllers
{
    #region reference
    using System;
    using System.Web.Mvc;
    using System.Web.Security;
    using com.Sconit.Entity.ACC;
    using com.Sconit.Entity.Exception;
    using com.Sconit.Service;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Util;
    using System.Web;
    using System.Threading;
    using System.Text.RegularExpressions;
    #endregion

    /// <summary>
    /// This controller response to control the user login or logout and so on.
    /// </summary>
    public class AccountController : WebAppBaseController
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the AccountController class.
        /// </summary>
        public AccountController()
        {
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the this.SecurityMgr which main consider the user security 
        /// </summary>
        public ISecurityMgr securityMgr { get; set; }
        #endregion

        #region public actions
        /// <summary>
        /// Index action for account controller, redirect to login action
        /// </summary>
        /// <returns>rediret view</returns>
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Login action 
        /// </summary>
        /// <returns>rediret view</returns>
        public ActionResult Login()
        {
            if (CurrentUser != null)
            {
                return RedirectToAction("Default", "Main");
            }
            var systemFlag = systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.SystemFlag);
            ViewBag.IsShow = systemFlag == "1";
            return View();
        }

        /// <summary>
        /// HttpPost Login action send the user info to action.
        /// </summary>
        /// <param name="model">User LogOn Model</param>
        /// <param name="returnUrl">the use specified the url</param>
        /// <returns>the default index view or user specified the url</returns>
        [HttpPost]
        public ActionResult Login(LogOnModel model, string returnUrl)
        {
            var systemFlag = systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.SystemFlag);
            ViewBag.IsShow = systemFlag == "1";
            if (ModelState.IsValid)
            {
                var isUserInDomain = false;
                User user = this.securityMgr.GetUserWithPermissions(model.UserName);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, Resources.ErrorMessage.Errors_Login_Password_MisMatch);
                }
                else if (!user.IsActive && user.Code != "su")
                {
                    ModelState.AddModelError(string.Empty, "用户帐号已停用。");
                }
                else
                {
                    if (this.securityMgr.IsDomainAuthenticated(model.UserName, model.Password))
                    {
                        isUserInDomain = true;
                    }

                    if (!isUserInDomain && !model.HashedPassword.Equals(user.Password, StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError(string.Empty, Resources.ErrorMessage.Errors_Login_Password_MisMatch);
                    }
                    else
                    {
                        ////判断用户停用等
                        if (user.PasswordExpired && user.Code != "su")
                        {
                            return RedirectToAction("ChangePassword");
                        }

                        FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                        Session.Add(WebConstants.UserSessionKey, user);

                        if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                            && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Default", "Main");
                        }
                    }
                }
            }

            //// If we got this far, something failed, redisplay form
            return View(model);
        }

        /// <summary>
        /// Log off action
        /// </summary>
        /// <returns>return to login view</returns>
        public ActionResult Logoff()
        {
            Session.Remove(WebConstants.UserSessionKey);
            FormsAuthentication.SignOut();
            HttpCookie _cookie = new HttpCookie(WebConstants.CookieCurrentUICultureKey, Thread.CurrentThread.CurrentUICulture.Name);
            _cookie.Expires = DateTime.Now.AddYears(-1);
            HttpContext.Response.SetCookie(_cookie);
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Change password action
        /// </summary>
        /// <returns>to change password view</returns>
        [SconitAuthorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        ///  Changed password action
        /// </summary>
        /// <param name="model">model for change password</param>
        /// <returns>return the result view</returns>
        [SconitAuthorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                bool changePasswordSucceeded = true;

                Regex r = new Regex("^(?:(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])|(?=.*[A-Z])(?=.*[a-z])(?=.*[^A-Za-z0-9])|(?=.*[A-Z])(?=.*[0-9])(?=.*[^A-Za-z0-9])|(?=.*[a-z])(?=.*[0-9])(?=.*[^A-Za-z0-9])).{6,}|(?:(?=.*[A-Z])(?=.*[a-z])|(?=.*[A-Z])(?=.*[0-9])|(?=.*[A-Z])(?=.*[^A-Za-z0-9])|(?=.*[a-z])(?=.*[0-9])|(?=.*[a-z])(?=.*[^A-Za-z0-9])|(?=.*[0-9])(?=.*[^A-Za-z0-9])|).{8,}");

                if (!r.IsMatch(model.NewPassword))
                {
                    BusinessException ex = new BusinessException();
                    ex.AddMessage(Resources.ACC.User.User_Regex);
                    SaveBusinessExceptionMessage(ex);
                    changePasswordSucceeded = false;
                }

                //// ChangePassword will throw an exception rather
                ////than return false in certain failure scenarios.
                try
                {
                    CurrentUser.Password = model.NewPassword;
                    base.genericMgr.Update(CurrentUser);
                }
                catch (BusinessException ex)
                {
                    SaveBusinessExceptionMessage(ex);
                    changePasswordSucceeded = false;
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                ////else
                ////{
                ////    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                ////}
            }

            //// If we got this far, something failed, redisplay form
            return View(model);
        }

        /// <summary>
        /// Changed the password success.
        /// </summary>
        /// <returns>Changed the password successful view</returns>
        [SconitAuthorize]
        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }
        #endregion
    }
}