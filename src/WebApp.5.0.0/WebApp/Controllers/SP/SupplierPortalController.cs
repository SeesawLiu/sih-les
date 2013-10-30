
namespace com.Sconit.Web.Controllers.SP
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using com.Sconit.Entity.ORD;
    using com.Sconit.Service;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Models.SearchModels.ORD;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;
    using com.Sconit.Entity.SCM;
    using System;
    using AutoMapper;
    using com.Sconit.Entity.MD;
    using NHibernate.Criterion;
    using com.Sconit.Entity.Exception;
    using com.Sconit.Utility;
    using System.Text;
    using com.Sconit.Entity.SYS;
    using com.Sconit.Entity;
    using System.Web;

    public class SupplierPortalController : WebAppBaseController
    {
        public SupplierPortalController()
        {
        }

        [SconitAuthorize(Permissions = "Url_Supplier_SettlePrint")]
        public ActionResult SettlePrint()
        {
            ViewBag.SupplierCode = GetEncryptDencryptPortalUserName();
            return View();
        }

        [SconitAuthorize(Permissions = "Url_Supplier_SettleDetail")]
        public ActionResult SettleDetail()
        {
            ViewBag.SupplierCode = GetEncryptDencryptPortalUserName();
            return View();
        }

        [SconitAuthorize(Permissions = "Url_Supplier_ASNCreate")]
        public ActionResult ASNCreate()
        {
            ViewBag.SupplierCode = GetEncryptDencryptPortalUserName();
            return View();
        }

        [SconitAuthorize(Permissions = "Url_Supplier_ASNPrint")]
        public ActionResult ASNPrint()
        {
            ViewBag.SupplierCode = GetEncryptDencryptPortalUserName();
            return View();
        }
        [SconitAuthorize(Permissions = "Url_Supplier_ScheduleLine")]
        public ActionResult ScheduleLine()
        {
            ViewBag.SupplierCode = GetEncryptDencryptPortalUserName();
            return View();
        }

        //[SconitAuthorize(Permissions = "Url_Supplier_ASNPrint")]
        //public ActionResult ASNPrint()
        //{
        //    ViewBag.SupplierCode = GetEncryptDencryptPortalUserName();
        //    return View();
        //}

        [SconitAuthorize(Permissions = "Url_Supplier_WPM")]
        public ActionResult WPM()
        {
            ViewBag.SupplierCode = GetEncryptDencryptPortalUserName();
            return View();
        }

        [SconitAuthorize(Permissions = "Url_Supplier_Receiving")]
        public ActionResult Receiving()
        {
            ViewBag.SupplierCode = GetEncryptDencryptPortalUserName();
            return View();
        }

        [SconitAuthorize(Permissions = "Url_Supplier_MyMessage")]
        public ActionResult MyMessage()
        {
            ViewBag.SupplierCode = GetEncryptDencryptPortalUserName();
            return View();
        }

        [SconitAuthorize(Permissions = "Url_Inventory_Portal_StockDiff")]
        public ActionResult StockDiff()
        {
            ViewBag.UserCode = GetEncryptDencryptPortalUserName();
            return View();
        }

        private string GetEncryptDencryptPortalUserName()
        {
            string supplierCode = string.Empty;
            try
            {
                com.Sconit.Entity.ACC.User user = SecurityContextHolder.Get();
                Supplier supplier = base.genericMgr.FindById<Supplier>(user.Name);
                EncryptDencryptService.EncryptDencryptService encryptDencryptService = new EncryptDencryptService.EncryptDencryptService();
                supplierCode = encryptDencryptService.EncryptDencrypt(supplier.ShortCode, true, "sih_dms");
            }
            catch (Exception ex)
            {

            }
            return Server.UrlEncode(supplierCode);
        }
    }
}
