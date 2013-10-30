
namespace com.Sconit.Web.Controllers.ACC
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Security;
    using com.Sconit.Entity.ACC;
    using com.Sconit.Service;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Models.SearchModels.ACC;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;
    using System.Text.RegularExpressions;
    using com.Sconit.Entity.Exception;

    public class UserController : AuthorizationBaseController
    {
        /// <summary>
        /// 
        /// </summary>
        private static string selectCountStatement = "select count(*) from User as u";

        /// <summary>
        /// 
        /// </summary>
        private static string selectStatement = "select u from User as u";

        /// <summary>
        /// 
        /// </summary>
        private static string userNameDuiplicateVerifyStatement = @"select count(*) from User as u where u.Code = ?";

        /// <summary>
        /// 
        /// </summary>
        private static string selectRolesByUserId = "select ur.Role from UserRole as ur where ur.User.Id = ?";

        /// <summary>
        /// 
        /// </summary>
        private static string selectRolesNotInUser = @"select r from Role as r where r.Id not in (select ur.Role.Id from UserRole as ur where ur.User.Id = ?)";

        /// <summary>
        /// 
        /// </summary>
        private static string selectPermissionGroupsByUserId = "select up.PermissionGroup from UserPermissionGroup as up where up.User.Id = ?";

        /// <summary>
        /// 
        /// </summary>
        private static string selectPermissionGroupsNotInUser = @"select up from PermissionGroup as up where up.Id not in (select upg.PermissionGroup.Id from UserPermissionGroup as upg where upg.User.Id = ?)";

        /// <summary>
        /// 
        /// </summary>
        private static string selectPermissionsByUserIdAndCategory = @"select p from UserPermission as up 
                                                                    join up.Permission as p 
                                                                    where p.PermissionCategory = ?
                                                                      and up.User.Id = ?";

        /// <summary>
        /// 
        /// </summary>
        private static string selectPermissionsNotInUserByCategory = @"select p from Permission as p
                                                                        where p.PermissionCategory = ?
                                                                          and p.Id not in (select p.Id 
                                                                                             from UserPermission as up 
                                                                                             join up.Permission as p
                                                                                            where up.User.Id = ?
                                                                                              and p.PermissionCategory = ?)";

        /// <summary>
        /// 
        /// </summary>
        public UserController()
        {
        }

        [SconitAuthorize(Permissions = "Url_User_View")]
        public ActionResult Index()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_User_View")]
        public ActionResult List(GridCommand command, UserSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_User_View")]
        public ActionResult _AjaxList(GridCommand command, UserSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<User>(searchStatementModel, command));
        }

        [SconitAuthorize(Permissions = "Url_User_Edit")]
        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_User_Edit")]
        public ActionResult New(User user)
        {
            if (ModelState.IsValid)
            {
                //判断用户名不能重复
                if (base.genericMgr.FindAll<long>(userNameDuiplicateVerifyStatement, new object[] { user.Code })[0] > 0)
                {
                    base.SaveErrorMessage(Resources.ACC.User.Errors_Existing_User, user.Code);
                }
                else
                {
                    user.Password = FormsAuthentication.HashPasswordForStoringInConfigFile(user.ConfirmPassword, "MD5");
                    base.genericMgr.Create(user);
                    SaveSuccessMessage(Resources.ACC.User.User_Added);
                    return RedirectToAction("Edit/" + user.Id);
                }
            }
            return View(user);
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_User_Edit")]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }

            return View(id);
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_User_Edit")]
        public ActionResult _Edit(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                User user = base.genericMgr.FindById<User>(id);
                user.NewPassword = user.Password;
                user.ConfirmPassword = user.Password;
                return PartialView(user);
            }
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_User_Edit")]
        public ActionResult _Edit(User user)
        {
            if (ModelState.IsValid)
            {
                base.genericMgr.Update(user);
                SaveSuccessMessage(Resources.ACC.User.User_Updated);
            }

            return PartialView(user);
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_User_Edit")]
        public ActionResult _UserRoles(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                ViewBag.AssignedRole = base.genericMgr.FindAll<Role>(selectRolesByUserId, id.Value);
                ViewBag.UnAssignedRole = base.genericMgr.FindAll<Role>(selectRolesNotInUser, id.Value);
                return PartialView();
            }
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_User_Edit")]
        public ActionResult _UserRoles(int? id, IList<int> assignedRoleIds)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                this.securityMgr.UpdateUserRoles(id.Value, assignedRoleIds);
                TempData["TabIndex"] = 1;
                SaveSuccessMessage(Resources.ACC.User.User_RoleAssigned);
                return new RedirectToRouteResult(new RouteValueDictionary  
                                                   { 
                                                       { "action", "Edit" }, 
                                                       { "controller", "User" } ,
                                                       { "id", id}
                                                   });
            }
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_User_Edit")]
        public ActionResult _UserPermissionGroups(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                ViewBag.AssignedPermissionGroups = base.genericMgr.FindAll<PermissionGroup>(selectPermissionGroupsByUserId, id.Value);
                ViewBag.UnAssignedPermissionGroups = base.genericMgr.FindAll<PermissionGroup>(selectPermissionGroupsNotInUser, id.Value);
                return PartialView();
            }
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_User_Edit")]
        public ActionResult _UserPermissionGroups(int? id, IList<int> assignedPermissionGroups)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                this.securityMgr.UpdateUserPermissionGroups(id.Value, assignedPermissionGroups);
                TempData["TabIndex"] = 2;
                SaveSuccessMessage(Resources.ACC.User.User_RoleAssigned);
                return new RedirectToRouteResult(new RouteValueDictionary  
                                                   { 
                                                       { "action", "Edit" }, 
                                                       { "controller", "User" } ,
                                                       { "id", id}
                                                   });
            }
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_User_Edit")]
        public ActionResult _UserPermissions(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                Prepare4AssignUserPermissions(id.Value);
                return PartialView();
            }
        }

        [HttpPost]
        [SconitAuthorize(Permissions = "Url_User_Edit")]
        public ActionResult _UserPermissions(int? id, string permissionCategoryCode, IList<int> assignedPermissionIds, string permissionCategoryType)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                this.securityMgr.UpdateUserPermissions(id.Value, permissionCategoryCode, assignedPermissionIds);
                TempData["TabIndex"] = 3;
                SaveSuccessMessage(Resources.ACC.User.User_RoleAssigned);
                return new RedirectToRouteResult(new RouteValueDictionary  
                                                   { 
                                                       { "action", "Edit" }, 
                                                       { "controller", "User" } ,
                                                       { "id", id}
                                                    });
                //object obj = new { SuccessMessage = string.Format(Resources.ORD.OrderMaster.OrderMaster_Shipped, id) };
                //return Json(obj);
                //错误
                //Response.StatusCode = 500;
                //Response.Write(ex.GetMessages()[0].GetMessageString());
                //return Json(null);

            }

        }

        [SconitAuthorize(Permissions = "Url_User_Delete")]
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                base.genericMgr.DeleteById<User>(id);
                SaveSuccessMessage(Resources.ACC.User.User_Deleted);
                return RedirectToAction("List");
            }
        }

        [SconitAuthorize(Permissions = "Url_UserFav_Edit")]
        public ActionResult _ChangePassword(int? Id)
        {
            if (Id.HasValue)
            {
                ChangePasswordModel changePasswordModel = new ChangePasswordModel();
                changePasswordModel.Id = (int)Id;
                return PartialView(changePasswordModel);
            }
            else
            {
                return HttpNotFound();
            }
        }

        [SconitAuthorize(Permissions = "Url_User_Edit")]
        [HttpPost]
        public ActionResult _ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                Regex r = new Regex("^(?:(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])|(?=.*[A-Z])(?=.*[a-z])(?=.*[^A-Za-z0-9])|(?=.*[A-Z])(?=.*[0-9])(?=.*[^A-Za-z0-9])|(?=.*[a-z])(?=.*[0-9])(?=.*[^A-Za-z0-9])).{6,}|(?:(?=.*[A-Z])(?=.*[a-z])|(?=.*[A-Z])(?=.*[0-9])|(?=.*[A-Z])(?=.*[^A-Za-z0-9])|(?=.*[a-z])(?=.*[0-9])|(?=.*[a-z])(?=.*[^A-Za-z0-9])|(?=.*[0-9])(?=.*[^A-Za-z0-9])|).{8,}");

                if (!r.IsMatch(model.ConfirmPassword))
                {
                    BusinessException ex = new BusinessException();
                    ex.AddMessage(Resources.ACC.User.User_Regex);
                    SaveBusinessExceptionMessage(ex);
                }
                else
                {
                    User user = base.genericMgr.FindById<User>(model.Id);
                    user.Password = FormsAuthentication.HashPasswordForStoringInConfigFile(model.ConfirmPassword, "MD5");
                    base.genericMgr.Update(user);
                    SaveSuccessMessage(Resources.ACC.User.User_PasswordChanged);
                }
            }

            return PartialView(model);
        }

        public ActionResult _AjaxLoadingPermissions(int userId, string permissionCategoryCode)
        {
            IList<Permission> assignedPermissionList = base.genericMgr.FindAll<Permission>(selectPermissionsByUserIdAndCategory, new object[] { permissionCategoryCode, userId });
            IList<Permission> unAssignedPermissionList = base.genericMgr.FindAll<Permission>(selectPermissionsNotInUserByCategory, new object[] { permissionCategoryCode, userId, permissionCategoryCode });

            base.TranslatePermission(assignedPermissionList);
            base.TranslatePermission(unAssignedPermissionList);

            return new JsonResult
            {
                Data = new SelectList[]{new SelectList(unAssignedPermissionList.OrderBy(p => p.Code), "Id", "LongDescription"),
                                        new SelectList(assignedPermissionList.OrderBy(p => p.Code), "Id", "LongDescription")}
            };
        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, UserSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("Code", searchModel.Code, HqlStatementHelper.LikeMatchMode.Start, "u", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("FirstName", searchModel.FirstName, HqlStatementHelper.LikeMatchMode.Start, "u", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("LastName", searchModel.LastName, HqlStatementHelper.LikeMatchMode.Start, "u", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("IsActive", searchModel.IsActive, "u", ref whereStatement, param);

            if (command.SortDescriptors.Count > 0)
            {
                if (command.SortDescriptors[0].Member == "LanguageDescription")
                {
                    command.SortDescriptors[0].Member = "Language";
                }
            }
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        private void Prepare4AssignUserPermissions(int userId)
        {
            PermissionCategory permissionCategory = base.Prepare4AssignPermissions();
            if (permissionCategory != null)
            {

                IList<Permission> assignedPermissionList = base.genericMgr.FindAll<Permission>(selectPermissionsByUserIdAndCategory, new object[] { permissionCategory.Code, userId });
                IList<Permission> unAssignedPermissionList = base.genericMgr.FindAll<Permission>(selectPermissionsNotInUserByCategory, new object[] { permissionCategory.Code, userId, permissionCategory.Code });

                base.TranslatePermission(assignedPermissionList);
                base.TranslatePermission(unAssignedPermissionList);

                ViewBag.AssignedPermissions = assignedPermissionList;
                ViewBag.UnAssignedPermissions = unAssignedPermissionList.OrderBy(p => p.Code);
            }
        }
    }
}
