
namespace com.Sconit.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using AutoMapper;
    using com.Sconit.Entity.MD;
    using com.Sconit.Entity.PRD;
    using com.Sconit.Entity.SCM;
    using com.Sconit.Entity.SYS;
    //using com.Sconit.Entity.SCM;
    using com.Sconit.Web.Models;
    using Telerik.Web.Mvc.UI;
    using System;
    using System;
    using com.Sconit.Entity.BIL;
    using com.Sconit.Service;
    using NHibernate.Criterion;
    using com.Sconit.Entity.ORD;
    using NHibernate.Type;
    using NHibernate;
    using com.Sconit.Entity.ISS;
    using com.Sconit.Entity.ACC;
    using com.Sconit.Entity.INP;
    using com.Sconit.Entity.CUST;
    using com.Sconit.Entity;

    using System.Web.UI.WebControls;
    using com.Sconit.Entity.INV;
    public class CommonController : WebAppBaseController
    {
        //下拉框默认只选出20条
        private static int firstRow = 0;
        private static int maxRow = 20;

        private static string selectEqItemStatement = "from Item as i where i.Code = ? and IsActive = ?";

        private static string selectLikeItemStatement = "from Item as i where i.Code like ? and IsActive = ?";

        private static string selectEqUserStatement = "from User as u where u.Code = ? and u.IsActive = ?";

        private static string selectLikeUserStatement = "from User as u where u.Code like ? and u.IsActive = ?";

        private static string selectEqItemPackageStatement = "from ItemPackage as i where  convert(VARCHAR,i.UnitCount) = ?";

        private static string selectLikeItemPackageStatement = "from ItemPackage as i where i.Item=? and convert(VARCHAR,i.UnitCount) like ? ";

        private static string selectEqSupplierStatement = "from Supplier as s where s.Code = ?";

        private static string selectEqCustomerStatement = "from Customer as c where c.Code = ?";

        private static string selectEqRegionStatement = "from Region as r where r.Code = ?";

        private static string selectLikeRoutingStatement = "from RoutingMaster as r where r.Code like ? and IsActive = ?";

        private static string selectEqBomStatement = "from BomMaster as b where b.Code = ? and IsActive = ?";

        private static string selectLikeBomStatement = "from BomMaster as b where b.Code like ? and IsActive = ?";

        private static string selectSAPLocationStatement = "  select distinct top 20  SAPLocation from md_location  where SAPLocation = '{0}'";

        private static string selectLikeSAPLocationStatement = " select distinct top 20  SAPLocation from md_location  where SAPLocation like '%{0}%' ";

        private static string selectEqIssueAddressStatement = "from IssueAddress as ia where  ia.Code = ? or ia.Description = ? order by Sequence";

        private static string selectLikeIssueAddressStatement = "from IssueAddress as ia where  ia.Code like ? or ia.Description like ?  order by Sequence";

        private static string selectEqFlowStatement = "from FlowMaster as f where f.Code = ? and f.IsActive = ? ";

        private static string selectEqLocationStatement = "from Location as l where l.Code = ?";

        private static string selectEqAddressStatement = "from Address as l where l.Code = ?";

        private static string selectEqPickStrategyStatement = " from PickStrategy as w where w.Code= ? ";

        private static string selectEqPickerStatement = "select distinct top 20 Code from MD_Picker as l where l.Code = '{0}'";

        private static string selectEqShipperStatement = "select distinct top 20 Code from MD_Shipper as l where l.Code = '{0}'";

        private static string selectShiftMastersStatement = "from ShiftMaster as mss";

        private static string selectLikeShiftMasterStatement = "from ShiftMaster as s where s.Code like ? ";

        private static string selectLikeSeqGroupStatement = "from SequenceGroup as s where s.Code like ? ";

        private static string selectSuppliersByGroupStatement = "from MultiSupplySupplier as mss where mss.GroupNo = ? ";

        private static string selectEqShiftMasterStatement = "from ShiftMaster as s where s.Code = ?";


        #region public methods
        public ActionResult _SiteMapPath(string menuContent)
        {
            //IList<com.Sconit.Entity.SYS.Menu> allMenu = systemMgr.GetAllMenu();
            //IList<MenuModel> allMenuModel = Mapper.Map<IList<com.Sconit.Entity.SYS.Menu>, IList<MenuModel>>(allMenu);

            //List<MenuModel> menuList = allMenuModel.Where(m => m.Code == menuContent).ToList();
            //this.NestGetParentMenu(menuList, menuList, allMenuModel);

            //return PartialView(menuList);
            return PartialView();
        }

        #region MoveType

        public ActionResult _MoveTypeDropDownList(string controlName, string controlId, string selectedValue, bool? includeBlankOption, string blankOptionDescription, string blankOptionValue, bool? coupled, string IOType, bool? isChange)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Coupled = coupled;
            ViewBag.SelectedValue = selectedValue;
            ViewBag.IsChange = isChange;
            IList<MiscOrderMoveType> MoveTypeList = base.genericMgr.FindAll<MiscOrderMoveType>("from MiscOrderMoveType as m where m.IOType=?", IOType);
            if (MoveTypeList == null)
            {
                MoveTypeList = new List<MiscOrderMoveType>();
            }

            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                MiscOrderMoveType blankMoveType = new MiscOrderMoveType();

                blankMoveType.Description = blankOptionDescription;
                MoveTypeList.Insert(0, blankMoveType);
            }

            return PartialView(new SelectList(MoveTypeList, "MoveType", "Description", selectedValue));

        }
        #endregion

        #region CodeMaster
        public ActionResult _CodeMasterDropDownList(com.Sconit.CodeMaster.CodeMaster code, string controlName, string controlId, string selectedValue, string ajaxActionName, bool? isSupplier,
            //string[] parentCascadeControlNames, string[] cascadingControlNames,
                                                    bool? includeBlankOption, string blankOptionDescription, string blankOptionValue, bool? enable, bool? isConsignment, bool? isShowNA, int? orderType)
        {
            IList<CodeDetail> codeDetailList = systemMgr.GetCodeDetails(code, includeBlankOption, blankOptionDescription, blankOptionValue);

            //IList<object> para = new List<object>();
            //para.Add(com.Sconit.CodeMaster.CodeMaster.OrderStatus);
            //para.Add((int)com.Sconit.CodeMaster.OrderStatus.Create);
            //codeDetailList = systemMgr.GetCodeDetails(code, includeBlankOption, blankOptionDescription, blankOptionValue);
            //IList<CodeDetail> codeDetail = base.genericMgr.FindAll<CodeDetail>("from CodeDetail c where c.Code  = ? and c.Value=?", para.ToArray());
            //if (codeDetail.Count > 0)
            //{
            //    codeDetailList.Remove(codeDetail[0]);
            //}
            //    if (codeDetailList != null && codeDetailList.Count > 0)
            //    {
            //        codeDetailList = codeDetailList.Where(q => q.Value != ((int)com.Sconit.CodeMaster.OrderStatus.Create).ToString()).ToList();
            //    }
            //}
            //else
            //{

            if (isSupplier != null && isSupplier.Value)
            {
                codeDetailList = codeDetailList.Where(q => q.Value != ((int)com.Sconit.CodeMaster.OrderStatus.Create).ToString()).ToList();
            }
            //base.genericMgr.FindAll<CodeDetail>("from CodeDetail c where c.code = 'OrderStatus' and c.value in ('1','2')");
            //systemMgr.GetCodeDetails(code, includeBlankOption, blankOptionDescription, blankOptionValue);
            //采购路线中的结算方式 不显示寄售结算  
            if (isConsignment != null)
            {
                if (code == com.Sconit.CodeMaster.CodeMaster.BillTerm)
                {
                    if ((bool)isConsignment)
                    {
                        // codeDetailList = base.genericMgr.FindAll<CodeDetail>("from CodeDetail c where c.Description in ('" + "CodeDetail_BillTerm_BAC" + "','" + "CodeDetail_BillTerm_NA" + "','" + "CodeDetail_BillTerm_BAR" + "') order by c.Sequence");
                        codeDetailList = codeDetailList.Where(p => p.Value == ((int)com.Sconit.CodeMaster.OrderBillTerm.ConsignmentBilling).ToString() || p.Value == ((int)com.Sconit.CodeMaster.OrderBillTerm.NA).ToString() || p.Value == ((int)com.Sconit.CodeMaster.OrderBillTerm.ReceivingSettlement).ToString()).ToList();
                    }
                    else
                    {
                        codeDetailList = codeDetailList.Where(p => p.Value != ((int)com.Sconit.CodeMaster.OrderBillTerm.ConsignmentBilling).ToString()).ToList();
                    }
                    if (isShowNA != null)
                    {
                        if (!(bool)isShowNA)
                        {
                            codeDetailList = codeDetailList.Where(p => p.Value != ((int)com.Sconit.CodeMaster.OrderBillTerm.NA).ToString()).ToList();
                            //IList<CodeDetail> codeDetail = base.genericMgr.FindAll<CodeDetail>("from CodeDetail c where c.Description = ?", "CodeDetail_BillTerm_NA");
                            //if (codeDetail.Count > 0)
                            //{
                            //    codeDetailList.Remove(codeDetail[0]);
                            //}
                        }
                    }
                }
            }

            //收货和发货的OrderType 不显示销售和生产
            if (code == com.Sconit.CodeMaster.CodeMaster.OrderType)
            {
                //if (controlName == "GoodsReceiptOrderType" || controlName == "IpOrderType")
                //{
                //    IList<CodeDetail> codeDetail = base.genericMgr.FindAll<CodeDetail>("from CodeDetail c where c.Description = ? or c.Description=?",
                //        new object[] { "CodeDetail_OrderType_Distribution", "CodeDetail_OrderType_Production" });
                //    if (codeDetail.Count > 0)
                //    {
                //        for (int i = 0; i < codeDetail.Count; i++)
                //        {
                //            codeDetailList.Remove(codeDetail[i]);
                //        }
                //    }
                //}
                if (orderType != null)
                {
                    codeDetailList = systemMgr.GetCodeDetails(code);
                    if (orderType.Value == (int)com.Sconit.CodeMaster.OrderType.Production)
                    {
                        codeDetailList = codeDetailList.Where(p => p.Value == ((int)com.Sconit.CodeMaster.OrderType.Production).ToString()).ToList();
                        //   base.genericMgr.FindAll<CodeDetail>("from CodeDetail as c where c.Code = ? and c.Value = ?", new object[] { com.Sconit.CodeMaster.CodeMaster.OrderType.ToString(), (int)com.Sconit.CodeMaster.OrderType.Production });
                    }
                    if (orderType.Value == (int)com.Sconit.CodeMaster.OrderType.Procurement)
                    {
                        if (isSupplier != null && isSupplier.Value)
                        {
                            codeDetailList = codeDetailList.Where(p => p.Value == ((int)com.Sconit.CodeMaster.OrderType.Procurement).ToString() || p.Value == ((int)com.Sconit.CodeMaster.OrderType.CustomerGoods).ToString() || p.Value == ((int)com.Sconit.CodeMaster.OrderType.SubContract).ToString()).ToList();
                            //base.genericMgr.FindAll<CodeDetail>("from CodeDetail as c where c.Code = ? and c.Value in (?,?,?)", new object[] { com.Sconit.CodeMaster.CodeMaster.OrderType.ToString(), (int)com.Sconit.CodeMaster.OrderType.Procurement, (int)com.Sconit.CodeMaster.OrderType.CustomerGoods, (int)com.Sconit.CodeMaster.OrderType.SubContract });
                        }
                        else
                        {
                            codeDetailList = codeDetailList.Where(p => p.Value == ((int)com.Sconit.CodeMaster.OrderType.Procurement).ToString() || p.Value == ((int)com.Sconit.CodeMaster.OrderType.CustomerGoods).ToString() || p.Value == ((int)com.Sconit.CodeMaster.OrderType.Transfer).ToString() || p.Value == ((int)com.Sconit.CodeMaster.OrderType.SubContractTransfer).ToString() || p.Value == ((int)com.Sconit.CodeMaster.OrderType.ScheduleLine).ToString()).ToList();
                            // codeDetailList = base.genericMgr.FindAll<CodeDetail>("from CodeDetail as c where c.Code = ? and c.Value in (?,?,?,?,?,?)", new object[] { com.Sconit.CodeMaster.CodeMaster.OrderType.ToString(), (int)com.Sconit.CodeMaster.OrderType.Procurement, (int)com.Sconit.CodeMaster.OrderType.SubContract, (int)com.Sconit.CodeMaster.OrderType.CustomerGoods, (int)com.Sconit.CodeMaster.OrderType.Transfer, (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer, (int)com.Sconit.CodeMaster.OrderType.ScheduleLine });
                        }
                    }
                    if (orderType.Value == (int)com.Sconit.CodeMaster.OrderType.Distribution)
                    {
                        codeDetailList = codeDetailList.Where(p => p.Value == ((int)com.Sconit.CodeMaster.OrderType.Distribution).ToString() || p.Value == ((int)com.Sconit.CodeMaster.OrderType.Transfer).ToString() || p.Value == ((int)com.Sconit.CodeMaster.OrderType.SubContractTransfer).ToString()).ToList();
                        //base.genericMgr.FindAll<CodeDetail>("from CodeDetail as c where c.Code = ? and c.Value in (?,?,?)", new object[] { com.Sconit.CodeMaster.CodeMaster.OrderType.ToString(), (int)com.Sconit.CodeMaster.OrderType.Distribution, (int)com.Sconit.CodeMaster.OrderType.Transfer, (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer });
                    }

                    #region empty codedetail
                    CodeDetail emptyCodeDetail = new CodeDetail();
                    emptyCodeDetail.Value = blankOptionValue;
                    emptyCodeDetail.Description = blankOptionDescription;
                    codeDetailList.Insert(0, emptyCodeDetail);
                    #endregion
                }
            }
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            //ViewBag.SelectedValue = selectedValue;
            ViewBag.AjaxActionName = ajaxActionName;

            ViewBag.Enable = enable;
            // codeDetailList.Add(new CodeDetail());
            //ViewBag.CascadingControlNames = cascadingControlNames;
            //ViewBag.ParentCascadeControlNames = parentCascadeControlNames;
            if (code == com.Sconit.CodeMaster.CodeMaster.FlowStrategy)
            {
                codeDetailList = codeDetailList.Where(c => c.Description != "CodeDetail_FlowStrategy_NA").ToList();
            }
            return PartialView(base.Transfer2DropDownList(code, codeDetailList, selectedValue));
        }
        public ActionResult _SAPLocationComboBox(string controlName, string controlId, string selectedValue, bool? enable, bool? isChange)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Enable = enable;

            ViewBag.isChange = isChange;
            IList<object> locationobjectList = new List<object>();
            IList<Location> locationList = new List<Location>();
            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                locationobjectList = base.genericMgr.FindAllWithNativeSql<object>(string.Format(selectSAPLocationStatement, selectedValue));
                if (locationobjectList.Count != 0)
                {
                    foreach (object obj in locationobjectList)
                    {
                        Location cation = new Location();
                        cation.SAPLocation = obj.ToString();
                        locationList.Add(cation);
                    }
                }
            }

            return PartialView(new SelectList(locationList, "SAPLocation", "SAPLocation", selectedValue));


        }
        public ActionResult _SAPLocationAjaxLoading(string text)
        {

            IList<object> locationobjectList = null;
            IList<Location> locationList = new List<Location>();
            if (text == "")
                locationobjectList = base.genericMgr.FindAllWithNativeSql<object>("select distinct top 20  SAPLocation from md_location ");
            else
                locationobjectList = base.genericMgr.FindAllWithNativeSql<object>(string.Format(selectLikeSAPLocationStatement, text));


            if (locationobjectList.Count != 0)
            {
                foreach (object obj in locationobjectList)
                {
                    Location cation = new Location();
                    cation.SAPLocation = obj.ToString();
                    locationList.Add(cation);
                }
            }

            return new JsonResult { Data = new SelectList(locationList, "SAPLocation", "SAPLocation") };
        }




        public ActionResult _CodeMasterComboBox(com.Sconit.CodeMaster.CodeMaster code, string controlName, string controlId, string selectedValue, bool? enable, bool? isChange
            )
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Enable = enable;
            ViewBag.Code = code;
            ViewBag.isChange = isChange;
            //IList<CodeDetail> codeDetailList = new List<CodeDetail>();
            //if (selectedValue != null && selectedValue.Trim() != string.Empty)
            //{
            //    codeDetailList = base.genericMgr.FindAll<CodeDetail>("from CodeDetail as c where c.Code = ? and c.Value = ?", new object[] { code, selectedValue });
            //}
            IList<CodeDetail> codeDetailList = null;
            if (selectedValue == null || selectedValue.Trim() == string.Empty)
                codeDetailList = systemMgr.GetCodeDetails(code);
            else
                codeDetailList = systemMgr.GetCodeDetails(code).Where(p => p.Value == selectedValue).ToList();

            return PartialView(base.Transfer2DropDownList(code, codeDetailList, selectedValue));
        }

        public ActionResult _CodeMasterAjaxLoading(string text, com.Sconit.CodeMaster.CodeMaster code)
        {
            //IList<CodeDetail> codeDetailList = new List<CodeDetail>();

            //string hql = "from CodeDetail as c where c.Code = ?";
            //IList<object> paraList = new List<object>();
            //paraList.Add(code);
            //if (!string.IsNullOrEmpty(text))
            //{
            //    hql += " and c.Value like ?";
            //    paraList.Add(text + "%");
            //}
            IList<CodeDetail> codeDetailList = null;
            if (text == "")
                codeDetailList = systemMgr.GetCodeDetails(code);
            else
                codeDetailList = systemMgr.GetCodeDetails(code).Where(p => p.Value == text).ToList();
            //genericMgr.FindAll<CodeDetail>(hql, paraList.ToArray(), firstRow, maxRow);


            IList<SelectListItem> itemList = Mapper.Map<IList<CodeDetail>, IList<SelectListItem>>(codeDetailList);
            foreach (var item in itemList)
            {
                item.Text = systemMgr.TranslateCodeDetailDescription(item.Text);
            }
            return new JsonResult { Data = new SelectList(itemList, "Value", "Text") };
        }

        public ActionResult _CodeMasterMultiSelectBox(com.Sconit.CodeMaster.CodeMaster code, string controlName, string controlId, string checkedValues)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;

            IList<CodeDetail> codeDetailList = null;
            codeDetailList = systemMgr.GetCodeDetails(code);

            foreach (var codeDetail in codeDetailList)
            {
                codeDetail.ChineDescription = systemMgr.TranslateCodeDetailDescription(codeDetail.Description);
            }
            ViewBag.CodeDetails = codeDetailList;
            if (!string.IsNullOrWhiteSpace(checkedValues))
            {
                ViewBag.CheckedValues = checkedValues.Split(',').ToList();
            }
            ViewBag.CodeDetails = codeDetailList;

            return PartialView();
        }
        #endregion

        #region User
        public ActionResult _UserComboBox(string controlName, string controlId, string selectedValue, bool? enable)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Enable = enable;

            IList<User> userList = new List<User>();
            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                userList = base.genericMgr.FindAll<User>(selectEqUserStatement, new object[] { selectedValue, true });
            }
            return PartialView(new SelectList(userList, "Code", "CodeDescription", selectedValue));
        }

        public ActionResult _UserAjaxLoading(string text)
        {
            IList<User> userList = new List<User>();
            userList = base.genericMgr.FindAll<User>(selectLikeUserStatement, new object[] { text + "%", true }, firstRow, maxRow);
            return new JsonResult { Data = new SelectList(userList, "Code", "CodeDescription") };
        }
        #endregion

        #region Item
        public ActionResult _ItemComboBox(string controlName, string controlId, string selectedValue, bool? enable, bool? coupled)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Enable = enable;
            ViewBag.Coupled = coupled;
            IList<Item> itemList = new List<Item>();
            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                itemList = base.genericMgr.FindAll<Item>(selectEqItemStatement, new object[] { selectedValue, true });
            }

            ViewBag.ItemFilterMode = base.systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.ItemFilterMode);
            ViewBag.ItemFilterMinimumChars = int.Parse(base.systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.ItemFilterMinimumChars));
            return PartialView(new SelectList(itemList, "Code", "CodeDescription", selectedValue));
        }

        public ActionResult _ItemAjaxLoading(string text)
        {
            AutoCompleteFilterMode fileterMode = (AutoCompleteFilterMode)Enum.Parse(typeof(AutoCompleteFilterMode), base.systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.ItemFilterMode), true);
            IList<Item> itemList = new List<Item>();

            if (fileterMode == AutoCompleteFilterMode.Contains)
            {
                itemList = base.genericMgr.FindAll<Item>(selectLikeItemStatement, new object[] { "%" + text + "%", true }, firstRow, maxRow);
            }
            else
            {
                itemList = base.genericMgr.FindAll<Item>(selectLikeItemStatement, new object[] { text + "%", true }, firstRow, maxRow);
            }

            return new JsonResult { Data = new SelectList(itemList, "Code", "CodeDescription") };
        }


        #endregion

        #region Bom
        public ActionResult _BomComboBox(string controlName, string controlId, string selectedValue)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            IList<BomMaster> bomList = new List<BomMaster>();
            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                bomList = base.genericMgr.FindAll<BomMaster>(selectEqBomStatement, new object[] { selectedValue, true });
            }

            return PartialView(new SelectList(bomList, "Code", "CodeDescription", selectedValue));
        }

        public ActionResult _BomAjaxLoading(string text)
        {

            IList<BomMaster> bomList = new List<BomMaster>();
            bomList = base.genericMgr.FindAll<BomMaster>(selectLikeBomStatement, new object[] { text + "%", true }, firstRow, maxRow);
            return new JsonResult { Data = new SelectList(bomList, "Code", "CodeDescription") };
        }
        #endregion

        #region ItemCategory
        public ActionResult _ItemCategoryDropDownList(string controlName, string controlId, string selectedValue, bool? includeBlankOption, string blankOptionDescription, string blankOptionValue, bool? enable)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            //ViewBag.SelectedValue = selectedValue;
            ViewBag.Enable = enable;
            IList<ItemCategory> itemCategoryList = base.genericMgr.FindAll<ItemCategory>("from ItemCategory as i");
            if (itemCategoryList == null)
            {
                itemCategoryList = new List<ItemCategory>();
            }

            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                ItemCategory blankitemCategory = new ItemCategory();
                blankitemCategory.Code = blankOptionValue;
                blankitemCategory.Description = blankOptionDescription;

                itemCategoryList.Insert(0, blankitemCategory);
            }
            return PartialView(new SelectList(itemCategoryList, "Code", "Description", selectedValue));
        }
        #endregion

        #region Location
        public ActionResult _LocationComboBox(string controlName, string controlId, string selectedValue, bool? enable, bool? isChange, bool? checkRegion, bool? isStockTakeLocation)
        {
            ViewBag.ControlName = controlName;
            ViewBag.Enable = enable;
            ViewBag.IsChange = isChange;
            ViewBag.ControlId = controlId;
            ViewBag.CheckRegion = checkRegion;
            ViewBag.SelectedValue = selectedValue;
            ViewBag.isStockTakeLocation = isStockTakeLocation;

            IList<Location> locationList = new List<Location>();

            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                locationList = base.genericMgr.FindAll<Location>(selectEqLocationStatement, new object[] { selectedValue });
            }
            return PartialView(new SelectList(locationList, "Code", "CodeName", selectedValue));
        }

        public ActionResult _AjaxLoadingLocation(string region, string text, bool checkRegion, bool isStockTakeLocation,string stNo)
        {
            User user = SecurityContextHolder.Get();
            IList<Location> locationList = new List<Location>();

            string hql = "from Location l where  l.Code like ? and l.IsActive = ?";
            IList<object> paramList = new List<object>();

            paramList.Add(text + "%");
            paramList.Add(true);

            if (checkRegion || !string.IsNullOrEmpty(region))
            {
                hql += " and l.Region = ?";
                paramList.Add(region);
            }

            if (isStockTakeLocation && !string.IsNullOrWhiteSpace(stNo))
            {
                hql += " and exists( select 1 from StockTakeLocation as sl where sl.Location=l.Code and sl.StNo=? )";
                paramList.Add(stNo);
            }
            if (user.Code.Trim() != "su")
            {
                //没区域的加校验
                hql += "  and exists (select 1 from UserPermissionView as u where u.UserId =" + user.Id + "and  u.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and u.PermissionCode = l.Region)";
            }
            locationList = base.genericMgr.FindAll<Location>(hql, paramList.ToArray(), firstRow, maxRow);


            return new JsonResult
            {
                Data = new SelectList(locationList, "Code", "CodeName", text)
            };
        }
        #endregion

        #region LocationTransaction


        #endregion

        #region Party
        public ActionResult _PartyDropDownList(string controlName, string controlId, string selectedValue, bool? includeBlankOption, string blankOptionDescription, string blankOptionValue)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            IList<Party> PartyList = base.genericMgr.FindAll<Party>("from Party as p");
            if (PartyList == null)
            {
                PartyList = new List<Party>();
            }

            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                Party blankParty = new Party();
                blankParty.Code = blankOptionValue;
                blankParty.Name = blankOptionDescription;

                PartyList.Insert(0, blankParty);
            }
            return PartialView(new SelectList(PartyList, "Code", "Name", selectedValue));
        }
        #endregion

        #region Tax
        public ActionResult _TaxDropDownList(string controlName, string controlId, string selectedValue, bool? includeBlankOption, string blankOptionDescription, string blankOptionValue)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            //ViewBag.SelectedValue = selectedValue;
            IList<Tax> taxList = base.genericMgr.FindAll<Tax>("from Tax as t");
            if (taxList == null)
            {
                taxList = new List<Tax>();
            }

            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                Tax blankTax = new Tax();
                blankTax.Code = blankOptionValue;
                blankTax.Name = blankOptionDescription;

                taxList.Insert(0, blankTax);
            }
            return PartialView(new SelectList(taxList, "Code", "Name", selectedValue));
        }
        #endregion

        #region Address
        public ActionResult _AddressComboBox(string controlName, string controlId, string selectedValue, bool? enable, bool? isChange, bool? checkParty, int type)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.IsChange = isChange;
            ViewBag.Enable = enable;
            ViewBag.CheckParty = checkParty;
            ViewBag.Type = type;
            IList<Address> addressList = new List<Address>();
            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                addressList = base.genericMgr.FindAll<Address>(selectEqAddressStatement, new object[] { selectedValue }, firstRow, maxRow);
            }

            return PartialView(new SelectList(addressList, "Code", "CodeAddressContent", selectedValue));
        }

        public ActionResult _AjaxLoadingAddress(string text, string party, int type, bool checkParty)
        {
            IList<Address> addressList = new List<Address>();

            string hql = string.Empty;
            IList<object> paramList = new List<object>();
            if (checkParty || !string.IsNullOrEmpty(party))
            {
                hql += "select a from PartyAddress p join p.Address as a where a.Type = ? and p.Party = ?";
                paramList.Add(type);
                paramList.Add(party);
            }
            else
            {
                hql += "select a from Address as a where a.Type = ?";
                paramList.Add(type);
            }

            if (!string.IsNullOrEmpty(text))
            {
                hql += " and a.Code like ? ";
                paramList.Add(text + "%");
            }

            addressList = base.genericMgr.FindAll<Address>(hql, paramList.ToArray(), firstRow, maxRow);

            return new JsonResult
            {
                Data = new SelectList(addressList, "Code", "CodeAddressContent", "")
            };
        }
        #endregion

        #region Routing lqy
        public ActionResult _RoutingDropDownList(string controlName, string controlId, string selectedValue, bool? includeBlankOption)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            IList<RoutingMaster> routingList = base.genericMgr.FindAll<RoutingMaster>(" from RoutingMaster as r where r.IsActive = ? ", true);
            if (routingList == null)
            {
                routingList = new List<RoutingMaster>();
            }

            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                RoutingMaster blankRoutingMaster = new RoutingMaster();
                routingList.Insert(0, blankRoutingMaster);
            }
            return PartialView(new SelectList(routingList, "Code", "Name", selectedValue));
        }

        public ActionResult _RoutingComboBox(string controlName, string controlId, string selectedValue, bool? enable)
        {
            ViewBag.ControlName = controlName;
            ViewBag.controlId = controlId;
            ViewBag.Enable = enable;
            IList<RoutingMaster> routingList = base.genericMgr.FindAll<RoutingMaster>(" from RoutingMaster as r where r.IsActive = ? ", true);
            if (routingList == null)
            {
                routingList = new List<RoutingMaster>();
            }


            return PartialView(new SelectList(routingList, "Code", "CodeName", selectedValue));
        }

        public ActionResult _RoutingAjaxLoading(string text)
        {
            //AutoCompleteFilterMode fileterMode = AutoCompleteFilterMode.StartsWith;
            IList<RoutingMaster> routingList = new List<RoutingMaster>();

            //if (fileterMode == AutoCompleteFilterMode.Contains)
            //{
            //    routingList = base.genericMgr.FindAll<RoutingMaster>(selectLikeRoutingStatement, new object[] { "%" + text + "%", true }, firstRow, maxRow);
            //}
            //else
            //{
            routingList = base.genericMgr.FindAll<RoutingMaster>(selectLikeRoutingStatement, new object[] { text + "%", true }, firstRow, maxRow);
            //}

            return new JsonResult { Data = new SelectList(routingList, "Code", "CodeName") };
        }
        #endregion

        #region PriceList
        public ActionResult _PriceListComboBox(string controlName, string controlId, string selectedValue, bool? enable, bool? isChange, bool? checkParty)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.IsChange = isChange;
            ViewBag.Enable = enable;
            ViewBag.CheckParty = checkParty;
            IList<PriceListMaster> priceList = new List<PriceListMaster>();
            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                priceList = base.genericMgr.FindAll<PriceListMaster>(" from PriceListMaster as p where p.Code = ? ", selectedValue);
            }

            return PartialView(new SelectList(priceList, "Code", "Code", selectedValue));
        }

        public ActionResult _AjaxLoadingPriceList(string party, string text, bool checkParty)
        {
            IList<PriceListMaster> priceListMasterList = new List<PriceListMaster>();

            string hql = "from PriceListMaster p where  p.IsActive = ? ";
            IList<object> paramList = new List<object>();
            paramList.Add(true);

            if (!string.IsNullOrEmpty(text))
            {
                hql += " and p.Code like ?";
                paramList.Add(text + "%");
            }
            if (checkParty)
            {
                hql += " and p.Party = ?";
                paramList.Add(party);
            }
            priceListMasterList = base.genericMgr.FindAll<PriceListMaster>(hql, paramList.ToArray(), firstRow, maxRow);

            return new JsonResult
            {
                Data = new SelectList(priceListMasterList, "Code", "Code", text)
            };
        }
        #endregion

        #region Uom
        public ActionResult _UomDropDownList(string controlName, string controlId, string selectedValue, bool? includeBlankOption, string blankOptionDescription, string blankOptionValue, bool? enable)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            //ViewBag.SelectedValue = selectedValue;
            ViewBag.Enable = enable;
            IList<Uom> uomList = base.genericMgr.FindAll<Uom>("from Uom as u order by u.Code desc");
            if (uomList == null)
            {
                uomList = new List<Uom>();
            }

            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                Uom blankUom = new Uom();
                blankUom.Code = blankOptionValue;
                blankUom.Description = blankOptionDescription;

                uomList.Insert(0, blankUom);
            }
            return PartialView(new SelectList(uomList, "Code", "Code", selectedValue));
        }

        public ActionResult _AjaxLoadingUom(string text, string item)
        {
            #region 基本单位
            IList<string> uomList = new List<string>();
            if (string.IsNullOrEmpty(item))
            {
                Item baseItem = base.genericMgr.FindById<Item>(item);
                uomList.Add(baseItem.Uom);
            }
            #endregion

            #region 单位转换
            IList<UomConversion> uomConversionList = base.genericMgr.FindAll<UomConversion>("from UomConversion as u where u.Item = ?", item);
            if (uomConversionList != null && uomConversionList.Count > 0)
            {
                foreach (UomConversion uc in uomConversionList)
                {
                    if (!uomList.Contains(uc.BaseUom))
                    {
                        uomList.Add(uc.BaseUom);
                    }
                    if (!uomList.Contains(uc.AlterUom))
                    {
                        uomList.Add(uc.AlterUom);
                    }
                }
            }
            #endregion

            return new JsonResult { Data = new SelectList(uomList) };
        }


        #endregion

        #region IssueType
        public ActionResult _IssueTypeDropDownList(string controlName, string controlId, string selectedValue, bool? includeBlankOption, string blankOptionDescription, string blankOptionValue, bool? enable, bool? coupled)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            //ViewBag.SelectedValue = selectedValue;
            ViewBag.Enable = enable;
            ViewBag.Coupled = coupled;
            IList<com.Sconit.Entity.ISS.IssueType> issueTypeList = base.genericMgr.FindAll<com.Sconit.Entity.ISS.IssueType>("from IssueType where IsActive = ?", true);
            if (issueTypeList == null)
            {
                issueTypeList = new List<com.Sconit.Entity.ISS.IssueType>();
            }

            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                com.Sconit.Entity.ISS.IssueType blankIssueType = new com.Sconit.Entity.ISS.IssueType();
                blankIssueType.Code = blankOptionValue;
                blankIssueType.Description = blankOptionDescription;

                issueTypeList.Insert(0, blankIssueType);
            }
            return PartialView(new SelectList(issueTypeList, "Code", "Description", selectedValue));
        }
        #endregion

        #region IssueNo
        public ActionResult _IssueNoDropDownList(string controlName, string controlId, string selectedValue, bool? includeBlankOption, string blankOptionDescription, string blankOptionValue, bool? enable, bool? isChange)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            //ViewBag.SelectedValue = selectedValue;
            ViewBag.Enable = enable;
            ViewBag.IsChange = isChange;
            IList<IssueNo> issueNoList = base.genericMgr.FindAll<IssueNo>("from IssueNo as ino");
            if (issueNoList == null)
            {
                issueNoList = new List<IssueNo>();
            }

            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                IssueNo blankIssueNo = new IssueNo();
                blankIssueNo.Code = blankOptionValue;
                blankIssueNo.Description = blankOptionDescription;

                issueNoList.Insert(0, blankIssueNo);
            }
            return PartialView(new SelectList(issueNoList, "Code", "Description", selectedValue));
        }
        #endregion

        #region IssueLevel
        public ActionResult _IssueLevelDropDownList(string controlName, string controlId, string selectedValue, bool? includeBlankOption, string blankOptionDescription, string blankOptionValue, bool? enable)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            //ViewBag.SelectedValue = selectedValue;
            ViewBag.Enable = enable;
            IList<IssueLevel> issueLevelList = base.genericMgr.FindAll<IssueLevel>("from IssueLevel as il");
            if (issueLevelList == null)
            {
                issueLevelList = new List<IssueLevel>();
            }

            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                IssueLevel blankIssueLevel = new IssueLevel();
                blankIssueLevel.Code = blankOptionValue;
                blankIssueLevel.Description = blankOptionDescription;

                issueLevelList.Insert(0, blankIssueLevel);
            }
            return PartialView(new SelectList(issueLevelList, "Code", "Description", selectedValue));
        }
        #endregion

        #region IssueAddress
        public ActionResult _IssueAddressDropDownList(string code, string controlName, string controlId, string selectedValue, bool? includeBlankOption, bool? enable)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            //ViewBag.SelectedValue = selectedValue;
            ViewBag.Enable = enable;
            string hql = " from IssueAddress as ia ";
            if (!string.IsNullOrEmpty(code))
            {
                hql += "where ia.Code !='" + code + "'";
            }

            IList<IssueAddress> issueAddressList = base.genericMgr.FindAll<IssueAddress>(hql);

            if (issueAddressList == null)
            {
                issueAddressList = new List<IssueAddress>();
            }

            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                IssueAddress blankIssueAddress = new IssueAddress();
                issueAddressList.Insert(0, blankIssueAddress);
            }
            return PartialView(new SelectList(issueAddressList, "Code", "CodeDescription", selectedValue));
        }


        public ActionResult _IssueAddressComboBox(string controlName, string controlId, string selectedValue)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.SelectedValue = selectedValue;
            IList<IssueAddress> issueAddressList = new List<IssueAddress>();

            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                issueAddressList = base.genericMgr.FindAll<IssueAddress>(selectEqIssueAddressStatement, new object[] { selectedValue, selectedValue });
                if (issueAddressList.Count == 0)
                {
                    IssueAddress ia = new IssueAddress();
                    ia.Code = selectedValue;
                    issueAddressList.Add(ia);
                    return PartialView(new SelectList(issueAddressList, "Code", "CodeDescription", selectedValue));

                }
            }

            return PartialView(new SelectList(issueAddressList, "Code", "CodeDescription", selectedValue));

        }

        public ActionResult _IssueAddressAjaxLoading(string text)
        {
            IList<IssueAddress> issueAddressList = new List<IssueAddress>();

            issueAddressList = base.genericMgr.FindAll<IssueAddress>(selectLikeIssueAddressStatement, new object[] { text + "%", text + "%" });

            return new JsonResult { Data = new SelectList(issueAddressList, "Code", "CodeDescription") };
        }

        #endregion

        #region LocationArea
        public ActionResult _LocationAreaDropDownList(string controlName, string controlId, string selectedValue, bool? includeBlankOption, string blankOptionDescription, string blankOptionValue, string LocationCode)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            //ViewBag.SelectedValue = selectedValue;
            IList<LocationArea> locationAreaList = base.genericMgr.FindAll<LocationArea>("from LocationArea as i where i.IsActive = ? and i.Location=?",
                                                                   new object[] { true, LocationCode });
            if (locationAreaList == null)
            {
                locationAreaList = new List<LocationArea>();
            }

            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                LocationArea blankItem = new LocationArea();
                blankItem.Code = blankOptionValue;
                blankItem.Name = blankOptionDescription;

                locationAreaList.Insert(0, blankItem);
            }
            return PartialView(new SelectList(locationAreaList, "Code", "Name", selectedValue));
        }
        #endregion

        #region LocationBin
        public ActionResult _LocationBinDropDownList(string controlName, string controlId, string selectedValue, bool? includeBlankOption, string blankOptionDescription, string blankOptionValue)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            //ViewBag.SelectedValue = selectedValue;
            IList<LocationBin> locationBinList = base.genericMgr.FindAll<LocationBin>("from LocationBin as l where l.IsActive = ?", true);
            if (locationBinList == null)
            {
                locationBinList = new List<LocationBin>();
            }

            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                LocationBin blankItem = new LocationBin();
                blankItem.Code = blankOptionValue;
                blankItem.Name = blankOptionDescription;

                locationBinList.Insert(0, blankItem);
            }
            return PartialView(new SelectList(locationBinList, "Code", "Name", selectedValue));
        }

        public ActionResult _LocationBinComboBox(string controlName, string controlId, string selectedValue, bool? enable, bool? isChange)
        {
            ViewBag.ControlName = controlName;
            ViewBag.Enable = enable;
            ViewBag.IsChange = isChange;
            ViewBag.ControlId = controlId;
            ViewBag.SelectedValue = selectedValue;
            IList<LocationBin> locationBinList = base.genericMgr.FindAll<LocationBin>("from LocationBin as l where l.IsActive=?", true);
            if (locationBinList == null)
            {
                locationBinList = new List<LocationBin>();
            }

            return PartialView(new SelectList(locationBinList, "Code", "CodeName", selectedValue));
        }

        public ActionResult _LocationBinAjaxLoading(string text)
        {
            //AutoCompleteFilterMode fileterMode = AutoCompleteFilterMode.StartsWith;
            IList<LocationBin> locationBinList = new List<LocationBin>();

            //if (fileterMode == AutoCompleteFilterMode.Contains)
            //{
            //    locationBinList = base.genericMgr.FindAll<LocationBin>("from LocationBin as l where l.Code like ? and l.IsActive = ?", new object[] { "%" + text + "%", true }, firstRow, maxRow);
            //}
            //else
            //{
            locationBinList = base.genericMgr.FindAll<LocationBin>("from LocationBin as l where l.Code like ? and l.IsActive = ?", new object[] { text + "%", true }, firstRow, maxRow);
            //}

            return new JsonResult { Data = new SelectList(locationBinList, "Code", "Name") };
        }
        #endregion

        #region ItemPackage
        public ActionResult _ItemPackageDropDownList(string controlName, string controlId, string selectedValue, bool? includeBlankOption, string blankOptionDescription, string blankOptionValue, string itemCode)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            //ViewBag.SelectedValue = selectedValue;
            IList<ItemPackage> itemPackageList = base.genericMgr.FindAll<ItemPackage>("from ItemPackage as i where  i.Item=?", itemCode);
            if (itemPackageList == null)
            {
                itemPackageList = new List<ItemPackage>();
            }

            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                ItemPackage blankItemPackage = new ItemPackage();
                blankItemPackage.UnitCount = Convert.ToDecimal(blankOptionValue);
                blankItemPackage.UnitCount = Convert.ToDecimal(blankOptionDescription);

                itemPackageList.Insert(0, blankItemPackage);
            }
            return PartialView(new SelectList(itemPackageList, "UnitCount", "UnitCount", selectedValue));
        }
        #endregion

        #region Container
        public ActionResult _ContainerDropDownList(string controlName, string controlId, string selectedValue, bool? includeBlankOption, string blankOptionDescription, string blankOptionValue)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            IList<Container> containerList = base.genericMgr.FindAll<Container>("from Container as c where  c.IsActive=?", true);
            if (containerList == null)
            {
                containerList = new List<Container>();
            }

            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                Container blankContainer = new Container();
                blankContainer.Code = blankOptionValue;
                blankContainer.Description = blankOptionDescription;

                containerList.Insert(0, blankContainer);
            }
            return PartialView(new SelectList(containerList, "Code", "Description", selectedValue));
        }

        public ActionResult _ContainerComboBox(string controlName, string controlId, string selectedValue)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.SelectedValue = selectedValue;
            IList<Container> containerList = new List<Container>();

            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                containerList = base.genericMgr.FindAll<Container>("select c from Container as c where c.Code = ?  ", new object[] { selectedValue });
            }

            return PartialView(new SelectList(containerList, "Code", "CodeDescription", selectedValue));

        }

        public ActionResult _ItemContainerAjaxLoading(string text)
        {
            IList<Container> containerList = new List<Container>();

            if (text != null && text.Trim() != string.Empty)
            {
                containerList = base.genericMgr.FindAll<Container>("select c from Container as c where c.Code like ?  ", new object[] { text + "%" });
            }
            else
            {
                containerList = this.genericMgr.FindAll<Container>();
            }
            return new JsonResult { Data = new SelectList(containerList, "Code", "CodeDescription") };
        }
        #endregion

        #region ItemPackage ComboBox
        public ActionResult _ItemPackageComboBox(string controlName, string controlId, string selectedValue, string item)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            IList<ItemPackage> itemPackageList = new List<ItemPackage>();
            if (!string.IsNullOrEmpty(item))
            {
                itemPackageList = base.genericMgr.FindAll<ItemPackage>("select i from ItemPackage as i where i.Item=?", item, firstRow, maxRow);
            }
            else if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                itemPackageList = base.genericMgr.FindAll<ItemPackage>(selectEqItemPackageStatement, selectedValue, firstRow, maxRow);
            }

            if (!string.IsNullOrEmpty(selectedValue))
            {
                if (itemPackageList.Count < 1)
                {
                    ItemPackage itemPackage = new ItemPackage();
                    itemPackage.Item = item;
                    itemPackage.IsDefault = false;
                    itemPackage.UnitCount = decimal.Parse(selectedValue);
                    itemPackage.Description = selectedValue;
                    itemPackageList.Add(itemPackage);
                }
            }

            return PartialView(new SelectList(itemPackageList, "UnitCount", "UnitCount", selectedValue));
        }

        public ActionResult _ItemPackageAjaxLoading(string text, string item)
        {
            IList<ItemPackage> itemPackageList = new List<ItemPackage>();

            if (item == null)
            {
                itemPackageList = base.genericMgr.FindAll<ItemPackage>(selectLikeItemPackageStatement, new object[] { "", text + "%" });
            }
            else
            {
                itemPackageList = base.genericMgr.FindAll<ItemPackage>(selectLikeItemPackageStatement, new object[] { item, text + "%" });
            }

            return new JsonResult { Data = new SelectList(itemPackageList, "UnitCount", "UnitCount") };
        }
        #endregion

        #region Flow
        public ActionResult _FlowComboBox(string controlName, string controlId, string selectedValue, int? type, string types, bool? isChange, bool? isSupplier, bool? isCreateHu, bool? isCreateOrder, bool? isVanOrder, bool? enable, int? prodLineType, int? flowstrategy,bool? isPicker)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Enable = enable;
            ViewBag.Type = type;
            ViewBag.Types = types;
            ViewBag.ProdLineType = prodLineType;
            ViewBag.IsChange = isChange;
            ViewBag.IsSupplier = isSupplier;
            ViewBag.IsCreateHu = isCreateHu;
            ViewBag.IsCreateOrder = isCreateOrder;
            ViewBag.IsVanOrder = isVanOrder;
            ViewBag.FlowStrategy = flowstrategy;
            ViewBag.IsPicker = isPicker;

            IList<FlowMaster> flowList = new List<FlowMaster>();
            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {

                flowList = base.genericMgr.FindAll<FlowMaster>(selectEqFlowStatement, new object[] { selectedValue.Trim(), true });
            }

            return PartialView(new SelectList(flowList, "Code", "CodeDescription", selectedValue));
        }

        public ActionResult _FlowAjaxLoading(string text, int? type, string types, bool isSupplier, bool isCreateHu, bool isCreateOrder, bool? isVanOrder, int? prodLineType, int? flowstrategy, bool isPicker)
        {
            IList<FlowMaster> flowList = new List<FlowMaster>();

            User user = SecurityContextHolder.Get();
            string selectLikeFlowStatement = null;
            object[] paramContainList = new object[] { };
            object[] paramList = new object[] { };
            if (type == null)
            {
                selectLikeFlowStatement = "from FlowMaster as f where f.IsActive = ? and  f.Code like ? ";
                paramList = new object[] { true, text + "%" };

                if (isCreateHu)
                {
                    selectLikeFlowStatement += " and f.Type in (?,?,?,?,?) ";
                    paramList = new object[] { text + "%", (int)com.Sconit.CodeMaster.OrderType.CustomerGoods, (int)com.Sconit.CodeMaster.OrderType.Procurement, (int)com.Sconit.CodeMaster.OrderType.SubContract, (int)com.Sconit.CodeMaster.OrderType.Production, (int)com.Sconit.CodeMaster.OrderType.ScheduleLine };
                }

                if (!string.IsNullOrWhiteSpace(types))
                {
                    selectLikeFlowStatement += " and f.Type in (" + types + ") ";
                }
                if (isPicker)
                {
                    selectLikeFlowStatement += " and exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + " and  (p.PermissionCategoryType =2 and p.PermissionCode = f.PartyTo)) ";
                }
                else
                {
                    selectLikeFlowStatement += " and exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + " and  ((p.PermissionCategoryType =2 and p.PermissionCode = f.PartyTo) or( p.PermissionCategoryType in(3,4) and (p.PermissionCode = f.PartyTo or p.PermissionCode = f.PartyFrom )))) ";
                }
            }
            else if ((int)type == (int)com.Sconit.CodeMaster.OrderType.Procurement)
            {
                if (isSupplier)
                {
                    selectLikeFlowStatement = "from FlowMaster as f where f.IsActive = ? and f.Code like ? and f.Type in (?,?,?,?,?) ";
                    paramList = new object[] { true, text + "%", (int)com.Sconit.CodeMaster.OrderType.CustomerGoods, (int)com.Sconit.CodeMaster.OrderType.Procurement, (int)com.Sconit.CodeMaster.OrderType.SubContract, (int)com.Sconit.CodeMaster.OrderType.Transfer, (int)com.Sconit.CodeMaster.OrderType.ScheduleLine };
                    if (user.Code.Trim().ToLower() != "su")
                    {
                        selectLikeFlowStatement += " and  exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + ") and p.PermissionCode = f.PartyFrom)";
                    }
                }
                else
                {
                    bool allowManualCreateProcurementOrder = bool.Parse(systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.AllowManualCreateProcurementOrder));
                    if (isCreateOrder && !allowManualCreateProcurementOrder)
                    {
                        selectLikeFlowStatement = "from FlowMaster as f where f.IsActive = ? and f.Code like ? and f.Type in (?,?,?) ";
                        paramList = new object[] { true, text + "%", (int)com.Sconit.CodeMaster.OrderType.Procurement, (int)com.Sconit.CodeMaster.OrderType.Transfer, (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer };
                        if (user.Code.Trim().ToLower() != "su")
                        {
                            //selectLikeFlowStatement += " and (f.IsCheckPartyFromAuthority = 0  or ( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = f.PartyFrom)))"
                            //+ " and (f.IsCheckPartyToAuthority = 0  or ( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "  and p.PermissionCode = f.PartyTo)))";
                            //要货单：采购看来源和目的
                            //要货单：移库看目的区域
                            selectLikeFlowStatement += " and exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = f.PartyTo)";
                        }
                    }
                    else
                    {
                        selectLikeFlowStatement = "from FlowMaster as f where f.IsActive = ? and f.Code like ? and f.Type in(?,?,?,?,?)";
                        paramList = new object[] { true, text + "%", (int)com.Sconit.CodeMaster.OrderType.Transfer, (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer, (int)com.Sconit.CodeMaster.OrderType.Procurement, (int)com.Sconit.CodeMaster.OrderType.CustomerGoods, (int)com.Sconit.CodeMaster.OrderType.ScheduleLine };
                        if (user.Code.Trim().ToLower() != "su")
                        {
                            //selectLikeFlowStatement += " and (f.IsCheckPartyFromAuthority = 0  or ( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + ") and p.PermissionCode = f.PartyFrom)))"
                            //                           + " and (f.IsCheckPartyToAuthority = 0  or ( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "  and p.PermissionCode = f.PartyTo)))";

                            selectLikeFlowStatement += " and ((f.Type in (" + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.Procurement + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract + "," + (int)com.Sconit.CodeMaster.OrderType.ScheduleLine + ") and ( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + ") and p.PermissionCode = f.PartyFrom)) and  (exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = f.PartyTo)))"
                                                     + " or (f.Type in (" + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + ") and ( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = f.PartyTo))))";
                        }
                    }
                }


            }
            //直送供应商
            else if ((int)type == (int)com.Sconit.CodeMaster.OrderType.OnlySupplier)
            {
                if (isSupplier)
                {
                    selectLikeFlowStatement = "from FlowMaster as f where f.IsAutoCreate = 1 and f.IsActive = ? and f.Code like ? and f.Type = ?";
                    paramList = new object[] { true, text + "%", (int)com.Sconit.CodeMaster.OrderType.Procurement };
                    if (user.Code.Trim().ToLower() != "su")
                    {
                        selectLikeFlowStatement += " and  exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + ") and p.PermissionCode = f.PartyFrom)";
                    }
                }
                else
                {
                    if (isCreateOrder)
                    {
                        selectLikeFlowStatement = "from FlowMaster as f where f.IsAutoCreate = 1 and f.IsActive = ? and f.Code like ? and f.Type = ? ";
                        paramList = new object[] { true, text + "%", (int)com.Sconit.CodeMaster.OrderType.Procurement };
                        if (user.Code.Trim().ToLower() != "su")
                        {
                            //selectLikeFlowStatement += " and (f.IsCheckPartyFromAuthority = 0  or ( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = f.PartyFrom)))"
                            //+ " and (f.IsCheckPartyToAuthority = 0  or ( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "  and p.PermissionCode = f.PartyTo)))";
                            //要货单：采购看来源和目的
                            //要货单：移库看目的区域
                            selectLikeFlowStatement += " and exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = f.PartyTo)";
                        }
                    }
                }
            }
            else if ((int)type == (int)com.Sconit.CodeMaster.OrderType.Distribution)
            {
                selectLikeFlowStatement = "from FlowMaster as f where f.IsActive = ? and  f.Code like ?";
                paramList = new object[] { true, text + "%" };
                if (user.Code.Trim().ToLower() != "su")
                {
                    selectLikeFlowStatement += " and ((f.Type = " + (int)com.Sconit.CodeMaster.OrderType.Distribution + " and ( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + ") and p.PermissionCode = f.PartyTo)) and  (exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = f.PartyFrom)))"
                                             + " or (f.Type in (" + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + ") and ( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = f.PartyFrom))))";
                }
            }
            else if ((int)type == (int)com.Sconit.CodeMaster.OrderType.Transfer)
            {
                selectLikeFlowStatement = "from FlowMaster as f where f.IsActive = ? and f.Code like ? and f.Type in (?,?) ";
                paramList = new object[] { true, text + "%", (int)com.Sconit.CodeMaster.OrderType.Transfer, (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer };
                if (user.Code.Trim().ToLower() != "su")
                {
                    selectLikeFlowStatement += " and (( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType  =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = f.PartyFrom))"
                                               + " or ( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = f.PartyTo)))";
                }
            }
            else if ((int)type == (int)com.Sconit.CodeMaster.OrderType.Production)
            {
                selectLikeFlowStatement = "from FlowMaster as f where f.IsActive = ? and f.Code like ? and f.Type in (?) ";
                paramList = new object[] { true, text + "%", (int)com.Sconit.CodeMaster.OrderType.Production };
                if (isVanOrder != null)
                {
                    if (isVanOrder.Value)
                        selectLikeFlowStatement += " and f.ProdLineType in (1,2,3,4,9) ";
                    else
                        selectLikeFlowStatement += " and f.ProdLineType not in (1,2,3,4,9) ";
                    //selectLikeFlowStatement += " and exists (select 1 from ProductLineMap as p where p.ProductLine = f.Code and p.Type=" + (int)com.Sconit.CodeMaster.ProductLineMapType.Van + ")";
                }

                if (user.Code.Trim().ToLower() != "su")
                {
                    selectLikeFlowStatement += " and (f.IsCheckPartyFromAuthority = 0  or ( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = f.PartyFrom)))"
                                                 + " and (f.IsCheckPartyToAuthority = 0  or ( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = f.PartyTo)))";
                }
            }

            if (prodLineType != null)
            {
                selectLikeFlowStatement += " and f.ProdLineType =" + prodLineType.Value;
            }

            if (flowstrategy != null)
            {
                selectLikeFlowStatement += " and exists(select 1 from FlowStrategy as s where s.Flow = f.Code and s.Strategy = " + flowstrategy.Value + ")";
            }

            flowList = base.genericMgr.FindAll<FlowMaster>(selectLikeFlowStatement, paramList, firstRow, maxRow);

            return new JsonResult { Data = new SelectList(flowList, "Code", "CodeDescription") };
        }
        #endregion

        #region AssemblyFlow
        public ActionResult _AssemblyFlowComboBox(string controlName, string controlId, string selectedValue, int? prodLineType, bool? isChange, bool? enable, int? type, int? flowstrategy)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Enable = enable;
            ViewBag.ProdLineType = prodLineType;
            ViewBag.IsChange = isChange;
            ViewBag.Type = type;
            ViewBag.Flowstrategy = flowstrategy;
            string hql = "select f from FlowMaster as f where f.Type = ? ";
            IList<object> parameter = new List<object>();
            IList<FlowMaster> flowList = new List<FlowMaster>();
            if (type.HasValue)
            {
                parameter.Add(type.Value);
            }
            else
            {
                parameter.Add((int)com.Sconit.CodeMaster.OrderType.Production);
            }
            if (!string.IsNullOrWhiteSpace(selectedValue))
            {
                hql += " and  f.Code = ?";
                parameter.Add(selectedValue.Trim());
            }
            if (prodLineType.HasValue)
            {
                if (prodLineType.Value != -1)
                {
                    hql += " and  f.ProdLineType =?";
                    parameter.Add(prodLineType.Value);
                }
            }
            else
            {
                hql += " and  f.ProdLineType in (?,?,?,?,?)";
                parameter.Add(com.Sconit.CodeMaster.ProdLineType.Assembly);
                parameter.Add(com.Sconit.CodeMaster.ProdLineType.Cab);
                parameter.Add(com.Sconit.CodeMaster.ProdLineType.Chassis);
                parameter.Add(com.Sconit.CodeMaster.ProdLineType.Special);
                parameter.Add(com.Sconit.CodeMaster.ProdLineType.Check);
            }
            if (flowstrategy.HasValue)
            {
                hql += " and exists(select 1 from FlowStrategy as s where s.Flow = f.Code and s.Strategy = ?)";
                parameter.Add(flowstrategy.Value);
            }
            flowList = base.genericMgr.FindAll<FlowMaster>(hql, parameter.ToArray());
            return PartialView(new SelectList(flowList, "Code", "CodeDescription", selectedValue));
        }

        public ActionResult _AssemblyFlowAjaxLoading(string text, int? prodLineType, int? type, int? flowstrategy)
        {
            IList<FlowMaster> flowList = new List<FlowMaster>();

            User user = SecurityContextHolder.Get();
            string hql = "select f from FlowMaster as f where f.Type = ? ";
            IList<object> parameter = new List<object>();
            if (type.HasValue)
            {
                parameter.Add(type.Value);
            }
            else
            {
                parameter.Add((int)com.Sconit.CodeMaster.OrderType.Production);
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                hql += " and  f.Code like ?";
                parameter.Add(text + "%");
            }
            if (prodLineType.HasValue)
            {
                if (prodLineType.Value != -1)
                {
                    hql += " and  f.ProdLineType =?";
                    parameter.Add(prodLineType.Value);
                }
            }
            else
            {
                hql += " and  f.ProdLineType in (?,?,?,?,?)";
                parameter.Add(com.Sconit.CodeMaster.ProdLineType.Assembly);
                parameter.Add(com.Sconit.CodeMaster.ProdLineType.Cab);
                parameter.Add(com.Sconit.CodeMaster.ProdLineType.Chassis);
                parameter.Add(com.Sconit.CodeMaster.ProdLineType.Special);
                parameter.Add(com.Sconit.CodeMaster.ProdLineType.Check);
            }

            //if (flowstrategy.HasValue)
            //{
            //    hql += " and exists(select 1 from FlowStrategy as s where s.Flow = f.Code and s.Strategy = ?)";
            //    parameter.Add(flowstrategy.Value);
            //}

            if (user.Code.Trim().ToLower() != "su")
            {
                hql += " and (f.IsCheckPartyFromAuthority = 0  or ( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and (p.PermissionCode = f.PartyFrom or p.PermissionCode = f.PartyTo ))))";
            }

            flowList = base.genericMgr.FindAll<FlowMaster>(hql, parameter.ToArray(), firstRow, maxRow);

            return new JsonResult { Data = new SelectList(flowList, "Code", "CodeDescription") };
        }
        #endregion

        #region WorkCenter
        public ActionResult _WorkCenterAjaxLoading(string text)
        {
            IList<WorkCenter> flowList = new List<WorkCenter>();

            string hql = "select w from WorkCenter as w where 1=1";
            IList<object> parameter = new List<object>();

            if (!string.IsNullOrWhiteSpace(text))
            {
                hql += " and  w.Code like ?";
                parameter.Add(text + "%");
            }

            flowList = base.genericMgr.FindAll<WorkCenter>(hql, parameter.ToArray(), firstRow, maxRow);

            return new JsonResult { Data = new SelectList(flowList, "Code", "Code") };
        }
        #endregion

        #region　pickstrategy

        public ActionResult _PickStrategyComboBox(string controlName, string controlId, string selectedValue, int? type, bool? isChange)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Type = type;
            ViewBag.IsChange = isChange;

            //using com.Sconit.CodeMaster;
            IList<PickStrategy> flowList = new List<PickStrategy>();
            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {

                flowList = base.genericMgr.FindAll<PickStrategy>(selectEqPickStrategyStatement, new object[] { selectedValue.Trim() });
            }

            return PartialView(new SelectList(flowList, "Code", "Code", selectedValue));
        }


        public ActionResult _FlowStrategyAjaxLoading(string text, int? type)
        {
            IList<PickStrategy> flowList = new List<PickStrategy>();

            string selectLikeFlowStatement = null;

            object[] paramList = new object[] { };
            if (type == null)
            {
                selectLikeFlowStatement = "from PickStrategy as w where w.Code like ? ";
                paramList = new object[] { text + "%" };
            }
            flowList = base.genericMgr.FindAll<PickStrategy>(selectLikeFlowStatement, paramList, firstRow, maxRow);

            return new JsonResult { Data = new SelectList(flowList, "Code", "Code") };
        }

        public ActionResult _FlowStrategyDropDownList(string controlName, string controlId, string selectedValue, bool? includeBlankOption, string blankOptionDescription, string blankOptionValue, bool? coupled, bool? isChange, bool? enable)
        {
            ViewBag.Enable = enable;
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Coupled = coupled;
            ViewBag.SelectedValue = selectedValue;
            ViewBag.IsChange = isChange;
            IList<CodeDetail> flowStrategyList = systemMgr.GetCodeDetails(com.Sconit.CodeMaster.CodeMaster.FlowStrategy);
            flowStrategyList = flowStrategyList.Where(f => f.Description != "CodeDetail_FlowStrategy_NA").ToList();
            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                flowStrategyList.Add(new CodeDetail {Code=string.Empty,Description=string.Empty });
            }
            IList<SelectListItem> itemList = Mapper.Map<IList<CodeDetail>, IList<SelectListItem>>(flowStrategyList);
            foreach (var item in itemList)
            {
                item.Text = systemMgr.TranslateCodeDetailDescription(item.Text);
            }
            
            return PartialView(new SelectList(itemList, "Value", "Text", selectedValue));
        }

        #endregion

        #region Supplier Combox
        public ActionResult _SupplierComboBox(string controlName, string controlId, string selectedValue, bool? isChange, bool? enable, bool? checkPermission)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.IsChange = isChange;
            ViewBag.Enable = enable;
            ViewBag.CheckPermission = checkPermission;

            IList<Supplier> supplierList = new List<Supplier>();
            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                supplierList = base.genericMgr.FindAll<Supplier>(selectEqSupplierStatement, selectedValue);
            }

            return PartialView(new SelectList(supplierList, "Code", "CodeDescription", selectedValue));
        }


        public ActionResult _AjaxLoadingSupplier(string text, bool checkPermission)
        {
            string hql = "from Supplier as s where s.Code like ? and s.IsActive = ?";
            IList<object> paraList = new List<object>();

            paraList.Add(text + "%");
            paraList.Add(true);

            User user = SecurityContextHolder.Get();
            if (user.Code.Trim().ToLower() != "su" && checkPermission)
            {
                hql += "  and exists (select 1 from UserPermissionView as u where u.UserId =" + user.Id + "and  u.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + " and u.PermissionCode = s.Code)";
            }
            IList<Supplier> supplierList = base.genericMgr.FindAll<Supplier>(hql, paraList.ToArray(), firstRow, maxRow);
            return new JsonResult { Data = new SelectList(supplierList, "Code", "CodeDescription", text) };
        }

        public ActionResult _AjaxLoadingMultiSupplySupplier(string groupNo)
        {
            IList<MultiSupplySupplier> suppliers = base.genericMgr.FindAll<MultiSupplySupplier>(selectSuppliersByGroupStatement, new object[] { groupNo });
            return new JsonResult { Data = new SelectList(suppliers, "Supplier", "Supplier") };
        }
        #endregion

        #region Item ManufactureParty
        public ActionResult _ManufacturePartyComboBox(string controlName, string controlId, string selectedValue, bool? enable, bool? isChange)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Enable = enable;
            ViewBag.IsChange = isChange;

            string hql = "select p from Region p where p.Code in(select distinct m.PartyFrom from FlowMaster as m where m.Type=" + (int)com.Sconit.CodeMaster.OrderType.Production + ")";

            IList<Region> regionList = base.genericMgr.FindAll<Region>(hql);
            if (regionList == null)
            {
                regionList = new List<Region>();
            }

            IList<Supplier> supplierList = base.genericMgr.FindAll<Supplier>("from Supplier as s");

            if (supplierList.Count > 0)
            {
                foreach (Supplier item in supplierList)
                {
                    Region region = new Region();
                    region.Code = item.Code;
                    region.Name = item.Name;
                    regionList.Add(region);
                }
            }

            return PartialView(new SelectList(regionList, "Code", "CodeDescription", selectedValue));
        }

        public ActionResult _AjaxLoadingItemManufactureParty(string text, string item)
        {

            string partyhql = "select p from Supplier p where p.Code in (select distinct  m.PartyFrom from FlowMaster as m where exists (select 1 from FlowDetail as d where d.Flow=m.Code and d.Item=?) and m.Type=" + (int)com.Sconit.CodeMaster.OrderType.Procurement + ") and p.Code like ?";
            IList<object> partyParaList = new List<object>();

            partyParaList.Add(item);
            partyParaList.Add(text + "%");
            IList<Supplier> supplierList = base.genericMgr.FindAll<Supplier>(partyhql, partyParaList.ToArray(), firstRow, maxRow);
            if (supplierList.Count > 0)
            {
                return new JsonResult { Data = new SelectList(supplierList, "Code", "CodeDescription", text) };
            }
            else
            {
                string hql = "select p from Region p where p.Code in(select distinct m.PartyFrom from FlowMaster as m where m.Type=" + (int)com.Sconit.CodeMaster.OrderType.Production + ")";
                IList<Region> regionList = base.genericMgr.FindAll<Region>(hql);
                return new JsonResult { Data = new SelectList(regionList, "Code", "CodeDescription", text) };
            }
        }

        #endregion

        #region ManufactureParty
        public ActionResult _AjaxLoadingManufactureParty(string item)
        {
            string hql = "select p from Supplier p where p.Code in (select distinct  m.PartyFrom from FlowMaster as m where  m.Type=" + (int)com.Sconit.CodeMaster.OrderType.Procurement + ")";
            if (!string.IsNullOrWhiteSpace(item))
            {
                 hql = "select p from Supplier p where p.Code in (select distinct  m.PartyFrom from FlowMaster as m where exists (select 1 from FlowDetail as d where d.Flow=m.Code and d.Item='" + item + "') and m.Type=" + (int)com.Sconit.CodeMaster.OrderType.Procurement + ")";
            }
            IList<Supplier> supplierList = base.genericMgr.FindAll<Supplier>(hql);
            return new JsonResult { Data = new SelectList(supplierList, "Code", "CodeDescription") };
        }

        #endregion

        #region Customer Combox
        public ActionResult _CustomerComboBox(string controlName, string controlId, string selectedValue, bool? isChange, bool? enable, bool? checkPermission)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.IsChange = isChange;
            ViewBag.Enable = enable;
            ViewBag.CheckPermission = checkPermission;

            IList<Customer> customerList = new List<Customer>();
            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                customerList = base.genericMgr.FindAll<Customer>(selectEqCustomerStatement, selectedValue);
            }
            return PartialView(new SelectList(customerList, "Code", "CodeDescription", selectedValue));
        }

        public ActionResult _AjaxLoadingCustomer(string text, bool checkPermission)
        {
            string hql = "from Customer as c where c.Code like ? and c.IsActive = ?";
            IList<object> paraList = new List<object>();

            paraList.Add(text + "%");
            paraList.Add(true);

            User user = SecurityContextHolder.Get();
            if (user.Code.Trim().ToLower() != "su" && checkPermission)
            {
                hql += "  and exists (select 1 from UserPermissionView as u where u.UserId =" + user.Id + "and u.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + " and u.PermissionCode = c.Code)";
            }
            IList<Customer> customerList = base.genericMgr.FindAll<Customer>(hql, paraList.ToArray(), firstRow, maxRow);

            return new JsonResult { Data = new SelectList(customerList, "Code", "CodeDescription", text) };
        }
        #endregion

        #region Region Combox
        public ActionResult _RegionComboBox(string controlName, string controlId, string selectedValue, bool? isChange, bool? enable, bool? checkPermission)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.IsChange = isChange;
            ViewBag.Enable = enable;
            ViewBag.CheckPermission = checkPermission;
            IList<Region> regionList = new List<Region>();
            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                regionList = base.genericMgr.FindAll<Region>(selectEqRegionStatement, selectedValue);
            }
            return PartialView(new SelectList(regionList, "Code", "CodeDescription", selectedValue));
        }

        public ActionResult _RegionWorkShopComboBox(string controlName, string controlId, string selectedValue, bool? isChange, bool? enable, bool? checkPermission)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.IsChange = isChange;
            ViewBag.Enable = enable;
            ViewBag.CheckPermission = checkPermission;
            IList<Region> regionList = new List<Region>();
            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                regionList = base.genericMgr.FindAll<Region>(" from Region as r where r.Workshop = ? ", selectedValue);
            }
            return PartialView(new SelectList(regionList, "Workshop", "Workshop", selectedValue));
        }

        public ActionResult _AjaxLoadingRegionWorkShop(string text)
        {
            string hql = "from Region as r where r.Code like ? ";
            IList<object> paraList = new List<object>();
            paraList.Add(text + "%");
            IList<Region> regionList = base.genericMgr.FindAll<Region>(hql, paraList.ToArray(), firstRow, maxRow);
            return new JsonResult { Data = new SelectList(regionList, "Workshop", "Workshop", text) };
        }



        public ActionResult _AjaxLoadingRegion(string text, bool checkPermission)
        {

            string hql = "from Region as r where r.Code like ? and r.IsActive = ?";
            IList<object> paraList = new List<object>();

            paraList.Add(text + "%");
            paraList.Add(true);

            User user = SecurityContextHolder.Get();
            if (user.Code.Trim().ToLower() != "su" && checkPermission)
            {
                hql += "  and exists (select 1 from UserPermissionView as u where u.UserId =" + user.Id + "and  u.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and u.PermissionCode = r.Code)";
            }
            IList<Region> regionList = base.genericMgr.FindAll<Region>(hql, paraList.ToArray(), firstRow, maxRow);
            return new JsonResult { Data = new SelectList(regionList, "Code", "CodeDescription", text) };
        }

        #endregion

        #region Operation
        public ActionResult _OperationDropDownList(string controlName, string controlId, string selectedValue, string Routing, int? CurrentOperation, bool? enable)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            //ViewBag.SelectedValue = selectedValue;
            ViewBag.Enable = enable;

            string hql = "";
            if (CurrentOperation == null)
            {
                hql = "select distinct(r.Operation)  from RoutingDetail as r where r.Routing='" + Routing + "'";
            }
            else
            {
                hql = "select distinct(r.Operation)  from RoutingDetail as r where r.Routing='" + Routing + "' and Operation > " + CurrentOperation + "";
            }
            IList<object> ObjectList = base.genericMgr.FindAll<object>(hql);
            IList<Operation> OperationList = new List<Operation>();
            if (ObjectList != null)
            {
                foreach (object obj in ObjectList)
                {
                    Operation operation = new Operation();
                    operation.OpCode = int.Parse(obj.ToString());
                    OperationList.Add(operation);
                }
            }
            return PartialView(new SelectList(OperationList, "OpCode", "OpCode", selectedValue));
        }

        #endregion

        #region OrderMaster Party
        public ActionResult _OrderMasterPartyFromComboBox(string controlName, string controlId, string selectedValue, bool? enable, int? orderType, bool? isSupplier, bool? isChange, bool? isTransfer, bool? isWmsParty)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Enable = enable;
            ViewBag.OrderType = orderType;
            ViewBag.isChange = isChange;
            ViewBag.IsSupplier = isSupplier;
            ViewBag.IsTransfer = isTransfer;
            ViewBag.IsWmsParty = isWmsParty;
            IList<Party> partyList = new List<Party>();
            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                partyList = base.genericMgr.FindAll<Party>("from Party as p where p.Code = ?", selectedValue);
            }
            return PartialView(new SelectList(partyList, "Code", "CodeDescription", selectedValue));

        }

        public ActionResult _AjaxLoadingOrderMasterPartyFrom(string text, int? orderType, bool isSupplier, bool isTransfer,bool isWmsParty)
        {
            List<Party> partyList = new List<Party>();

            //su特殊处理
            User user = SecurityContextHolder.Get();
            if (user.Code.Trim().ToLower() == "su")
            {
                if (orderType == (int)com.Sconit.CodeMaster.OrderType.Procurement)
                {
                    if (isSupplier)
                    {
                        IList<Supplier> supplierList = base.genericMgr.FindAll<Supplier>("from Supplier as s where s.IsActive = ? and s.Code like ?", new object[] { true, text + "%" }, firstRow, maxRow);
                        if (supplierList != null)
                        {
                            partyList.AddRange(supplierList);
                        }
                        if (partyList.Count < maxRow)
                        {
                            IList<Customer> customerList = base.genericMgr.FindAll<Customer>("from Customer as c where c.IsActive = ? and c.Code like ?", new object[] { true, text + "%" }, firstRow, maxRow - partyList.Count);
                            if (customerList != null)
                            {
                                partyList.AddRange(customerList);
                            }
                        }
                    }
                    else
                    {
                        partyList = base.genericMgr.FindAll<Party>("from Party as p where p.IsActive = ? and p.Code like ?", new object[] { true, text + "%" }, firstRow, maxRow).ToList();
                    }
                }
                else if (orderType == (int)com.Sconit.CodeMaster.OrderType.Distribution)
                {
                    partyList = base.genericMgr.FindAll<Party>("from Region as r where r.IsActive = ? and r.Code like ?", new object[] { true, text + "%" }, firstRow, maxRow).ToList();
                }
                else if (orderType == (int)com.Sconit.CodeMaster.OrderType.Production)
                {
                    partyList = base.genericMgr.FindAll<Party>("from Region as r where r.IsActive = ? and r.Code like ?", new object[] { true, text + "%" }, firstRow, maxRow).ToList();
                }
                else
                {
                    partyList = base.genericMgr.FindAll<Party>("from Party as p where p.IsActive = ? and p.Code like ?", new object[] { true, text + "%" }, firstRow, maxRow).ToList();
                }
            }
            else
            {
                string hql = "from Party as p where p.IsActive = ? and p.Code like ?";
                if (isWmsParty)
                {
                    hql += " and p.Code in ('SQC','LOC') ";
                    partyList = base.genericMgr.FindAll<Party>(hql, new object[] { true, text + "%" }, firstRow, maxRow).ToList();
                    return new JsonResult { Data = new SelectList(partyList, "Code", "CodeDescription", text) };
                }
                if (isTransfer)
                {
                    hql += " and p.Code not in ('SQC','LOC','SQ1') ";
                }
                if (orderType == (int)com.Sconit.CodeMaster.OrderType.Procurement)
                {
                    if (isSupplier)
                    {
                        hql += " and exists (select 1 from UserPermissionView as u where u.UserId =" + user.Id + "and u.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + ") and u.PermissionCode = p.Code)";
                    }
                    else
                    {
                        hql += " and exists (select 1 from UserPermissionView as u where u.UserId =" + user.Id + "and  u.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + ") and u.PermissionCode = p.Code)";
                    }
                }
                else if (orderType == (int)com.Sconit.CodeMaster.OrderType.Distribution)
                {
                    hql += " and exists (select 1 from UserPermissionView as u where u.UserId =" + user.Id + "and  u.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and u.PermissionCode = p.Code)";
                }
                else if (orderType == (int)com.Sconit.CodeMaster.OrderType.Production)
                {
                    hql += " and exists (select 1 from UserPermissionView as u where u.UserId =" + user.Id + "and  u.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and u.PermissionCode = p.Code)";
                }
                else
                {
                    hql += " and exists (select 1 from UserPermissionView as u where u.UserId =" + user.Id + "and  u.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + ") and u.PermissionCode = p.Code)";
                }
                partyList = base.genericMgr.FindAll<Party>(hql, new object[] { true, text + "%" }, firstRow, maxRow).ToList();
            }
            return new JsonResult { Data = new SelectList(partyList, "Code", "CodeDescription", text) };
        }

        public ActionResult _OrderMasterPartyToComboBox(string controlName, string controlId, string selectedValue, bool? enable, int? orderType, bool? isChange, bool? isTransfer)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Enable = enable;
            ViewBag.isChange = isChange;
            ViewBag.OrderType = orderType;
            ViewBag.IsTransfer = isTransfer;
            IList<Party> partyList = new List<Party>();
            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                partyList = base.genericMgr.FindAll<Party>("from Party as p where p.Code = ?", selectedValue);
            }
            return PartialView(new SelectList(partyList, "Code", "CodeDescription", selectedValue));

        }

        public ActionResult _AjaxLoadingOrderMasterPartyTo(string text, int? orderType,bool isTransfer)
        {
            List<Party> partyList = new List<Party>();

            //su特殊处理
            User user = SecurityContextHolder.Get();
            if (user.Code.Trim().ToLower() == "su")
            {
                if (orderType == (int)com.Sconit.CodeMaster.OrderType.Procurement)
                {
                    partyList = base.genericMgr.FindAll<Party>("from Region as r where r.IsActive = ? and r.Code like ?", new object[] { true, text + "%" }, firstRow, maxRow).ToList();
                }
                else if (orderType == (int)com.Sconit.CodeMaster.OrderType.Distribution)
                {
                    IList<Customer> customerList = base.genericMgr.FindAll<Customer>("from Customer as c where c.IsActive = ? and c.Code like ?", new object[] { true, text + "%" }, firstRow, maxRow);
                    if (customerList != null)
                    {
                        partyList.AddRange(customerList);
                    }
                    if (partyList.Count < maxRow)
                    {
                        IList<Region> regionList = base.genericMgr.FindAll<Region>("from Region as r where r.IsActive = ? and r.Code like ?", new object[] { true, text + "%" }, firstRow, maxRow - partyList.Count);
                        if (regionList != null)
                        {
                            partyList.AddRange(regionList);
                        }
                    }
                }
                else if (orderType == (int)com.Sconit.CodeMaster.OrderType.Production)
                {
                    partyList = base.genericMgr.FindAll<Party>("from Region as r where r.IsActive = ? and r.Code like ?", new object[] { true, text + "%" }, firstRow, maxRow).ToList();
                }
                else
                {
                    partyList = base.genericMgr.FindAll<Party>("from Region as r where r.IsActive = ? and r.Code like ?", new object[] { true, text + "%" }, firstRow, maxRow).ToList();
                }
            }
            else
            {
                string hql = "from Party as p where p.IsActive = ? and p.Code like ?";
                if (isTransfer)
                {
                    hql += " and p.Code not in ('SQC','LOC','SQ1') ";
                }
                if (orderType == (int)com.Sconit.CodeMaster.OrderType.Procurement)
                {
                    hql += " and exists (select 1 from UserPermissionView as u where u.UserId =" + user.Id + "and  u.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and u.PermissionCode = p.Code)";
                }
                else if (orderType == (int)com.Sconit.CodeMaster.OrderType.Distribution)
                {
                    hql += " and exists (select 1 from UserPermissionView as u where u.UserId =" + user.Id + "and  u.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + ") and u.PermissionCode = p.Code)";
                }
                else if (orderType == (int)com.Sconit.CodeMaster.OrderType.Production)
                {
                    hql += " and exists (select 1 from UserPermissionView as u where u.UserId =" + user.Id + "and  u.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and u.PermissionCode = p.Code)";
                }
                else
                {
                    hql += " and exists (select 1 from UserPermissionView as u where  u.UserId =" + user.Id + "and u.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + ") and u.PermissionCode = p.Code)";
                }
                partyList = base.genericMgr.FindAll<Party>(hql, new object[] { true, text + "%" }, firstRow, maxRow).ToList();
            }
            return new JsonResult { Data = new SelectList(partyList, "Code", "CodeDescription", text) };

        }
        #endregion

        #region _RegionMultiSelectBox
        public ActionResult _RegionMultiSelectBox(int? orderType, string controlName, string controlId, string checkedValues)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;

            List<Party> partyList = new List<Party>();

            //su特殊处理
            User user = SecurityContextHolder.Get();
            if (user.Code.Trim().ToLower() == "su")
            {
                if (orderType == (int)com.Sconit.CodeMaster.OrderType.Procurement)
                {
                    partyList = base.genericMgr.FindAll<Party>("from Region as r where r.IsActive = 1 ",firstRow, maxRow).ToList();
                }
                else if (orderType == (int)com.Sconit.CodeMaster.OrderType.Distribution)
                {
                    IList<Customer> customerList = base.genericMgr.FindAll<Customer>("from Customer as c where c.IsActive = ? ", true, firstRow, maxRow);
                    if (customerList != null)
                    {
                        partyList.AddRange(customerList);
                    }
                    if (partyList.Count < maxRow)
                    {
                        IList<Region> regionList = base.genericMgr.FindAll<Region>("from Region as r where r.IsActive = ? ", true, firstRow, maxRow - partyList.Count);
                        if (regionList != null)
                        {
                            partyList.AddRange(regionList);
                        }
                    }
                }
                else if (orderType == (int)com.Sconit.CodeMaster.OrderType.Production)
                {
                    partyList = base.genericMgr.FindAll<Party>("from Region as r where r.IsActive = ? ", true, firstRow, maxRow).ToList();
                }
                else
                {
                    partyList = base.genericMgr.FindAll<Party>("from Region as r where r.IsActive = ? ", true, firstRow, maxRow).ToList();
                }
            }
            else
            {
                string hql = "from Party as p where p.IsActive = ? ";
                
                if (orderType == (int)com.Sconit.CodeMaster.OrderType.Procurement)
                {
                    hql += " and exists (select 1 from UserPermissionView as u where u.UserId =" + user.Id + "and  u.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and u.PermissionCode = p.Code)";
                }
                else if (orderType == (int)com.Sconit.CodeMaster.OrderType.Distribution)
                {
                    hql += " and exists (select 1 from UserPermissionView as u where u.UserId =" + user.Id + "and  u.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + ") and u.PermissionCode = p.Code)";
                }
                else if (orderType == (int)com.Sconit.CodeMaster.OrderType.Production)
                {
                    hql += " and exists (select 1 from UserPermissionView as u where u.UserId =" + user.Id + "and  u.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and u.PermissionCode = p.Code)";
                }
                else
                {
                    hql += " and exists (select 1 from UserPermissionView as u where  u.UserId =" + user.Id + "and u.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + ") and u.PermissionCode = p.Code)";
                }
                partyList = base.genericMgr.FindAll<Party>(hql, true, firstRow, maxRow).ToList();
            }
            if (!string.IsNullOrWhiteSpace(checkedValues))
            {
                ViewBag.CheckedValues = checkedValues.Split(',').ToList();
            }
            ViewBag.Values = partyList;

            return PartialView();
        }

        #endregion

        #region Currency
        public ActionResult _CurrencyDropDownList(string controlName, string controlId, string selectedValue, bool? includeBlankOption, string blankOptionDescription, string blankOptionValue, bool? enable)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Enable = enable;
            IList<Currency> currencyList = base.genericMgr.FindAll<Currency>("from Currency as c");
            if (currencyList == null)
            {
                currencyList = new List<Currency>();
            }

            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                Currency blankCurrency = new Currency();
                blankCurrency.Code = blankOptionValue;
                blankCurrency.Name = blankOptionDescription;

                currencyList.Insert(0, blankCurrency);
            }
            return PartialView(new SelectList(currencyList, "Code", "Name", selectedValue));
        }
        #endregion

        #region Facility
        public ActionResult _ProductLineFacilityComboBox(string controlName, string controlId, string selectedValue, bool? enable)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Enable = enable;
            IList<ProductLineFacility> productLineFacilityList = new List<ProductLineFacility>();

            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                productLineFacilityList = base.genericMgr.FindAll<ProductLineFacility>("from ProductLineFacility as p where p.Code = ?", selectedValue);
            }
            return PartialView(new SelectList(productLineFacilityList, "Code", "Code", selectedValue));
        }

        public ActionResult _AjaxLoadingProductLineFacility(string productLine, string text)
        {
            IList<ProductLineFacility> facilityList = base.genericMgr.FindAll<ProductLineFacility>("from ProductLineFacility as p where p.Code like ? and p.ProductLine = ?", new object[] { text + "%", productLine }, firstRow, maxRow);

            return new JsonResult
            {
                Data = new SelectList(facilityList, "Code", "Code", "")
            };
        }
        #endregion

        #region Order
        public ActionResult _OrderComboBox(string controlName, string selectedValue, int? orderType, bool? canFeed, bool? isChange, bool? isSupplier, bool? isCreateHu)
        {
            ViewBag.ControlName = controlName;
            ViewBag.OrderType = orderType;
            ViewBag.CanFeed = canFeed;
            ViewBag.IsChange = isChange;
            ViewBag.IsSupplier = isSupplier;
            ViewBag.IsCreateHu = isCreateHu;

            IList<OrderMaster> orderList = new List<OrderMaster>();
            IList<object> para = new List<object>();
            string hql = "from OrderMaster as o where o.OrderNo = ?"; ;
            if (!string.IsNullOrEmpty(selectedValue))
            {
                para.Add(selectedValue);
                orderList = base.genericMgr.FindAll<OrderMaster>(hql, para.ToArray());
            }
            return PartialView(new SelectList(orderList, "OrderNo", "OrderNo", selectedValue));
        }

        public ActionResult _OrderAjaxLoading(string text, int? orderType, bool? canFeed, bool isSupplier, bool isCreateHu)
        {
            IList<OrderMaster> orderList = new List<OrderMaster>();

            User user = SecurityContextHolder.Get();
            string hql = "from OrderMaster as o where o.OrderNo like ?";
            IList<object> para = new List<object>();
            para.Add(text + "%");

            if (isSupplier)
            {
                hql += "and o.Type in (?,?,?)";
                para.Add((int)com.Sconit.CodeMaster.OrderType.Procurement);
                para.Add((int)com.Sconit.CodeMaster.OrderType.CustomerGoods);
                para.Add((int)com.Sconit.CodeMaster.OrderType.SubContract);
            }
            else
            {
                if (orderType != null)
                {
                    hql += " and o.Type = ?";
                    para.Add(orderType.Value);
                }
                if (isCreateHu)
                {
                    hql += "and o.Type in (?,?,?,?)";
                    para.Add((int)com.Sconit.CodeMaster.OrderType.Procurement);
                    para.Add((int)com.Sconit.CodeMaster.OrderType.CustomerGoods);
                    para.Add((int)com.Sconit.CodeMaster.OrderType.SubContract);
                    para.Add((int)com.Sconit.CodeMaster.OrderType.Production);
                }
            }
            if (canFeed != null)
            {
                if (canFeed.Value)
                {
                    hql += " and o.Status in (?,?)";
                    para.Add((int)com.Sconit.CodeMaster.OrderStatus.InProcess);
                    para.Add((int)com.Sconit.CodeMaster.OrderStatus.Complete);
                }
                else
                {
                    hql += " and o.Status not in (?,?)";
                    para.Add((int)com.Sconit.CodeMaster.OrderStatus.InProcess);
                    para.Add((int)com.Sconit.CodeMaster.OrderStatus.Complete);
                }
            }

            if (user.Code.Trim().ToLower() != "su")
            {
                //采购订单来源和目的都要看
                if (isSupplier)
                {
                    hql += " and  exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + " and  p.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + ") and p.PermissionCode = o.PartyFrom)";
                    hql += " and  exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + " and  p.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + ") and p.PermissionCode = o.PartyTo)";
                }
                //销售订单来源和目的都要看
                else if (orderType == 3)
                {
                    hql += " and  exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + " and  p.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + ") and p.PermissionCode = o.PartyFrom)";
                    hql += " and  exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + " and  p.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + ") and p.PermissionCode = o.PartyTo)";
                }
                //生产或移库订单来源和目的只要有任意一方即可
                else
                {
                    hql += " and ((exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + " and  p.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + ") and p.PermissionCode = o.PartyFrom))"
                           + " or (exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + " and  p.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + ") and p.PermissionCode = o.PartyTo)))";
                }
            }
            orderList = base.genericMgr.FindAll<OrderMaster>(hql, para.ToArray(), firstRow, maxRow);

            return new JsonResult { Data = new SelectList(orderList, "OrderNo", "OrderNo") };
        }
        #endregion

        #region InspectOrder
        public ActionResult _InspectComboBox(string controlName, string selectedValue, int? status, bool? isChange)
        {
            ViewBag.ControlName = controlName;
            ViewBag.Status = status;
            ViewBag.IsChange = isChange;

            IList<InspectMaster> inspectList = new List<InspectMaster>();

            IList<object> para = new List<object>();
            string hql = "from InspectMaster as i where i.InspectNo = ?";
            if (!string.IsNullOrEmpty(selectedValue))
            {
                para.Add(selectedValue);
                inspectList = base.genericMgr.FindAll<InspectMaster>(hql, para.ToArray());
            }

            return PartialView(new SelectList(inspectList, "InspectNo", "InspectNo"));
        }

        public ActionResult _InspectAjaxLoading(string text, int? status)
        {
            IList<InspectMaster> inspectList = new List<InspectMaster>();

            string hql = "from InspectMaster as i where  i.InspectNo like ?";
            IList<object> para = new List<object>();
            para.Add(text + "%");

            if (status != null)
            {
                hql += " and i.Status = ?";
                para.Add(status.Value);
            }
            else
            {
                hql += " and i.Status in (?,?)";
                para.Add(com.Sconit.CodeMaster.InspectStatus.Submit);
                para.Add(com.Sconit.CodeMaster.InspectStatus.InProcess);
            }

            inspectList = base.genericMgr.FindAll<InspectMaster>(hql, para.ToArray(), firstRow, maxRow);
            return new JsonResult { Data = new SelectList(inspectList, "InspectNo", "InspectNo") };
        }
        #endregion

        #region RejectOrder
        public ActionResult _RejectComboBox(string controlName, string selectedValue, int? status, bool? isChange, int? handleResult)
        {
            ViewBag.ControlName = controlName;
            ViewBag.Status = status;
            ViewBag.IsChange = isChange;
            ViewBag.HandleResult = handleResult;

            IList<RejectMaster> rejectList = new List<RejectMaster>();

            IList<object> para = new List<object>();
            string hql = "from RejectMaster as r where r.RejectNo = ?";
            if (!string.IsNullOrEmpty(selectedValue))
            {
                para.Add(selectedValue);
                rejectList = base.genericMgr.FindAll<RejectMaster>(hql, para.ToArray());
            }

            return PartialView(new SelectList(rejectList, "RejectNo", "RejectNo"));
        }

        public ActionResult _RejectAjaxLoading(string text, int? status, int? handleResult)
        {
            IList<RejectMaster> rejectList = new List<RejectMaster>();

            string hql = "from RejectMaster as r where r.RejectNo like ?";
            IList<object> para = new List<object>();
            para.Add(text + "%");

            if (status != null)
            {
                hql += " and r.Status = ?";
                para.Add(status.Value);
            }
            if (handleResult != null)
            {
                hql += " and r.HandleResult = ?";
                para.Add(handleResult.Value);
            }

            rejectList = base.genericMgr.FindAll<RejectMaster>(hql, para.ToArray(), firstRow, maxRow);
            return new JsonResult { Data = new SelectList(rejectList, "RejectNo", "RejectNo") };
        }
        #endregion

        #region FailCode && DefectCode

        public ActionResult _FailCodeComboBox(string controlName, string controlId, string selectedValue, bool? enable, bool? isChange)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Enable = enable;
            ViewBag.IsChange = isChange;
            IList<FailCode> FailCodeList = new List<FailCode>();

            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                FailCodeList = base.genericMgr.FindAll<FailCode>("from FailCode as f where f.Code = ?", selectedValue);
            }
            return PartialView(new SelectList(FailCodeList, "Code", "CodeDescription", selectedValue));
        }

        public ActionResult _AjaxLoadingFailCode(string text)
        {
            IList<FailCode> failCodeList = base.genericMgr.FindAll<FailCode>("from FailCode as f where f.Code like ?", text + "%", firstRow, maxRow);
            return new JsonResult { Data = new SelectList(failCodeList, "Code", "CodeDescription") };
        }

        public ActionResult _DefectCodeComboBox(string controlName, string controlId, string selectedValue, bool? enable, bool? isChange)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Enable = enable;
            ViewBag.IsChange = isChange;
            IList<DefectCode> DefectCodeList = new List<DefectCode>();

            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                DefectCodeList = base.genericMgr.FindAll<DefectCode>("from DefectCode as f where f.Code = ?", selectedValue);
            }
            return PartialView(new SelectList(DefectCodeList, "Code", "CodeDescription", selectedValue));
        }

        public ActionResult _AjaxLoadingDefect(string text)
        {
            IList<DefectCode> defectCodeList = base.genericMgr.FindAll<DefectCode>("from DefectCode as d where d.Code like ?", text + "%", firstRow, maxRow);
            return new JsonResult { Data = new SelectList(defectCodeList, "Code", "CodeDescription") };
        }
        #endregion

        #region ProductCode  Assemblies  FailCode DropDownList

        public ActionResult _ProductCodeDropDownList(string controlName, string controlId, string selectedValue, bool? includeBlankOption, string blankOptionDescription, string blankOptionValue, bool? enable)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Enable = enable;
            IList<object[]> ObjectList = base.genericMgr.FindAll<object[]>("select d.ProductCode,d.ProductCode as desc1 from DefectCode as d group by d.ProductCode");
            IList<ProductCode> ProductCodeList = new List<ProductCode>();
            if (ObjectList != null)
            {
                foreach (object obj in ObjectList)
                {
                    ProductCode pro = new ProductCode();
                    pro.Code = (((object[])(obj))[0]).ToString();
                    ProductCodeList.Add(pro);
                }
            }

            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                ProductCode pro = new ProductCode();
                pro.Code = "";

                ProductCodeList.Insert(0, pro);
            }
            return PartialView(new SelectList(ProductCodeList, "Code", "Code", selectedValue));
        }

        public ActionResult _AssembliesDropDownList(string controlName, string controlId, string selectedValue, bool? includeBlankOption, string blankOptionDescription, string blankOptionValue, bool? enable)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Enable = enable;
            IList<Assemblies> AssembliesList = new List<Assemblies>();
            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                Assemblies assemblies = new Assemblies();
                assemblies.Code = "";

                AssembliesList.Insert(0, assemblies);
            }
            return PartialView(new SelectList(AssembliesList, "Code", "Code", selectedValue));
        }

        public ActionResult _AssembliesAjaxLoading(string productCode)
        {
            IList<Assemblies> AssembliesList = new List<Assemblies>();
            if (productCode == "")
            {
                return new JsonResult { Data = new SelectList(AssembliesList, "Code", "Code") };
            }
            string hql = "select d.Assemblies,d.Assemblies as desc1 from DefectCode as d where d.Assemblies like ?  group by d.Assemblies";
            IList<object[]> ObjectList = base.genericMgr.FindAll<object[]>(hql, productCode + "%", firstRow, maxRow);

            if (ObjectList != null)
            {
                foreach (object obj in ObjectList)
                {
                    Assemblies assemblies = new Assemblies();
                    assemblies.Code = (((object[])(obj))[0]).ToString();
                    AssembliesList.Add(assemblies);
                }
            }

            return new JsonResult { Data = new SelectList(AssembliesList, "Code", "Code") };
        }


        public ActionResult _DefectCodeDropDownList(string controlName, string controlId, string selectedValue, bool? includeBlankOption, string blankOptionDescription, string blankOptionValue, bool? enable)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Enable = enable;
            IList<DefectCode> AssembliesList = new List<DefectCode>();
            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                DefectCode defectCode = new DefectCode();
                defectCode.Code = blankOptionDescription;
                defectCode.Description = blankOptionValue;

                AssembliesList.Insert(0, defectCode);
            }
            return PartialView(new SelectList(AssembliesList, "Code", "ComponentDefectCode", selectedValue));
        }

        public ActionResult _DefectCodeAjaxLoading(string assemblies)
        {
            IList<DefectCode> DefectCodeList = new List<DefectCode>();
            if (assemblies == "")
            {
                return new JsonResult { Data = new SelectList(DefectCodeList, "Code", "Description") };
            }
            string hql = "from DefectCode as d where d.ComponentDefectCode like ? ";
            DefectCodeList = base.genericMgr.FindAll<DefectCode>(hql, assemblies + "%", firstRow, maxRow);
            return new JsonResult { Data = new SelectList(DefectCodeList, "Code", "CodeDescription") };
        }


        #endregion

        #region ProdLineType DropDownList
        public ActionResult _ProdLineTypeDropDownList(string controlName, string controlId, string selectedValue, bool? includeBlankOption, string blankOptionDescription, string blankOptionValue, bool? enable, bool? isAssemblyFlow)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Enable = enable;
            ViewBag.isAssemblyFlow = isAssemblyFlow;
            IList<CodeDetail> getList = new List<CodeDetail>();
            getList = systemMgr.GetCodeDetails(Sconit.CodeMaster.CodeMaster.ProdLineType, includeBlankOption, blankOptionDescription, blankOptionValue);
            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                CodeDetail codeDetail = new CodeDetail();
                codeDetail.Description = blankOptionDescription;
                codeDetail.Value = blankOptionValue;

                getList.Insert(0, codeDetail);
            }
            return PartialView(base.Transfer2DropDownList(Sconit.CodeMaster.CodeMaster.ProdLineType, getList, selectedValue));
        }

        public ActionResult _ProdLineTypeAjaxLoading(string value, bool? isAssemblyFlow)
        {
            IList<CodeDetail> getList = new List<CodeDetail>();
            string hql = "from CodeDetail as d where d.Code=?  ";
            getList = base.genericMgr.FindAll<CodeDetail>(hql, "ProdLineType", firstRow, maxRow);
            if (isAssemblyFlow == null)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    getList = getList.Where(g => g.Value == value).ToList();
                }
            }
            else
            {
                var assFlow = new string[] { "1", "2", "3", "4", "9" };
                if (!isAssemblyFlow.Value)
                {
                    getList = getList.Where(g => !assFlow.Contains(g.Value)).ToList();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        getList = getList.Where(g => g.Value == value).ToList();
                    }
                }
                else
                {
                    getList = getList.Where(g => assFlow.Contains(g.Value)).ToList();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        getList = getList.Where(g => g.Value == value).ToList();
                    }
                }
            }
            foreach (var item in getList)
            {
                item.Description = systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.ProdLineType, Convert.ToInt32(item.Value));
            }
            return new JsonResult { Data = new SelectList(getList, "Value", "Description") };
        }
        #endregion

        #region 联动
        public ActionResult _AjaxLoadingIssueNo(string issueType)
        {
            IList<IssueNo> issueNoList = new List<IssueNo>();
            if (issueType == null)
            {
                issueNoList = base.genericMgr.FindAll<IssueNo>("from IssueNo i ");
            }
            else
            {
                issueNoList = base.genericMgr.FindAll<IssueNo>("select i from IssueNo i join i.IssueType it where it.Code =? and it.IsActive=?", new object[] { issueType, true });
            }
            IssueNo blankIssueNo = new IssueNo();
            blankIssueNo.Code = string.Empty;
            blankIssueNo.Description = string.Empty;
            issueNoList.Insert(0, blankIssueNo);

            return new JsonResult
            {
                Data = new SelectList(issueNoList, "Code", "Description", "")
            };
        }

        public ActionResult _AjaxLoadingLocationBin(string party, string RegionValue)
        {

            IList<LocationBin> locationBinList = new List<LocationBin>();
            if (party == null)
            {
                locationBinList = base.genericMgr.FindAll<LocationBin>("from locationBin l where l.Region is null and l.IsActive=?", true, firstRow, maxRow);
            }
            else
            {
                locationBinList = base.genericMgr.FindAll<LocationBin>("from locationBin l where l.Region=? and l.IsActive=?", new object[] { party, true }, firstRow, maxRow);
            }
            LocationBin blankLocation = new LocationBin();
            blankLocation.Code = string.Empty;
            blankLocation.Name = string.Empty;
            locationBinList.Insert(0, blankLocation);
            if (string.IsNullOrEmpty(RegionValue))
            {
                return new JsonResult
                {
                    Data = new SelectList(locationBinList, "Code", "CodeName", "")
                };
            }
            else
            {
                return new JsonResult
                {
                    Data = new SelectList(locationBinList, "Code", "Name", RegionValue)
                };
            }
        }

        public ActionResult _AjaxLoadingLocationBinFrom(string partyFrom, string regionValue)
        {
            return _AjaxLoadingLocationBin(partyFrom, regionValue);
        }

        public ActionResult _AjaxLoadingLocationBinTo(string partyTo)
        {
            return _AjaxLoadingLocationBin(partyTo, "");
        }



        public ActionResult _AjaxLoadingIssueType()
        {
            IList<com.Sconit.Entity.ISS.IssueType> issueTypeList = base.genericMgr.FindAll<com.Sconit.Entity.ISS.IssueType>("from IssueType as it where it.IsActive = ?", true);
            return new JsonResult
            {
                Data = new SelectList(issueTypeList, "Code", "Description")
            };
        }

        #endregion


        #region Picker
        public ActionResult _PickerComboBox(string controlName, string controlId, string selectedValue, bool? enable, bool? checkLocation)
        {
            ViewBag.ControlName = controlName;
            ViewBag.Enable = enable;
            ViewBag.ControlId = controlId;
            ViewBag.CheckLocation = checkLocation;
            ViewBag.SelectedValue = selectedValue;

            IList<Picker> pickerList = new List<Picker>();

            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                pickerList = base.genericMgr.FindAll<Picker>("from Picker where Code = ? ", selectedValue);
            }
            return PartialView(new SelectList(pickerList, "Code", "Description", selectedValue));
        }

        public ActionResult _AjaxLoadingPicker(string location, string text, bool checkLocation)
        {
            User user=SecurityContextHolder.Get();
            IList<Picker> pickerList = new List<Picker>();

            string hql = "from Picker l where  l.Code like ? and l.IsActive = ?";
            IList<object> paramList = new List<object>();

            paramList.Add(text + "%");
            paramList.Add(true);

            if (checkLocation || !string.IsNullOrEmpty(location))
            {
                hql += " and l.Location = ?";
                paramList.Add(location);
            }
            else if (user.Code != "su")
            {
                hql += " and exists( select 1 from Location as lo where lo.Code=l.Location and exists (select 1 from UserPermissionView as u where u.UserId =" + user.Id + "and  u.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and u.PermissionCode = lo.Region) ) ";
                //hql += "  and exists (select 1 from UserPermissionView as u where u.UserId =" + user.Id + "and  u.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and u.PermissionCode = l.Region)";
            }
            pickerList = base.genericMgr.FindAll<Picker>(hql, paramList.ToArray(), firstRow, maxRow);

            return new JsonResult { Data = new SelectList(pickerList, "Code", "Description", text) };
        }

        public ActionResult _AjaxLoadingPickerWithBlank(string item, string location, string text)
        {
            IList<Picker> pickerList = new List<Picker>();

            IList<PickRule> pickRule = base.genericMgr.FindAll<PickRule>("from PickRule where Item = ? and Location = ?",
                new object[] { item, location });
            if (pickRule != null && pickRule.Count > 0)
            {
                foreach (PickRule rule in pickRule)
                {
                    Picker p = base.genericMgr.FindAll<Picker>("from Picker where Code = ? and IsActive = ?", new object[] { rule.Picker, true }).SingleOrDefault();
                    if (p != null)
                    {
                        pickerList.Add(p);
                    }
                }

                Picker blankPicker = new Picker();
                blankPicker.Code = "";
                blankPicker.Description = "-匹配规则-";

                pickerList.Add(blankPicker);
            }

            IList<Picker> locpickers = base.genericMgr.FindAll<Picker>("from Picker where Location = ? and IsActive = ?", new object[] { location, true });
            if (locpickers != null && locpickers.Count > 0)
            {
                foreach (Picker p in locpickers)
                {
                    pickerList.Add(p);
                }

                Picker blankPicker = new Picker();
                blankPicker.Code = "";
                blankPicker.Description = "-匹配库位-";

                pickerList.Add(blankPicker);
            }

            string hql = "from Picker l where  l.Code like ? and l.IsActive = ?";
            IList<object> paramList = new List<object>();

            paramList.Add(text + "%");
            paramList.Add(true);

            IList<Picker> nolocpickers = base.genericMgr.FindAll<Picker>(hql, paramList.ToArray(), firstRow, maxRow);
            if (nolocpickers != null)
            {
                foreach (Picker p in nolocpickers)
                {
                    pickerList.Add(p);
                }
            }

            Picker realblankPicker = new Picker();
            realblankPicker.Code = "";
            realblankPicker.Description = "-不分配-";
            pickerList.Add(realblankPicker);

            return new JsonResult { Data = new SelectList(pickerList, "Code", "Description", text) };
        }
        #endregion

        #region Shipper
        public ActionResult _ShipperComboBox(string controlName, string controlId, string selectedValue, bool? enable, bool? checkLocation)
        {
            ViewBag.ControlName = controlName;
            ViewBag.Enable = enable;
            ViewBag.ControlId = controlId;
            ViewBag.CheckLocation = checkLocation;
            ViewBag.SelectedValue = selectedValue;

            IList<Shipper> shipperList = new List<Shipper>();

            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                shipperList = base.genericMgr.FindAll<Shipper>("from Shipper where Code = ? ", selectedValue);
            }
            return PartialView(new SelectList(shipperList, "Code", "Description", selectedValue));
        }

        public ActionResult _AjaxLoadingShipper(string location, string text, bool checkLocation)
        {
            IList<Shipper> shipperList = new List<Shipper>();

            string hql = "from Shipper l where  l.Code like ? and l.IsActive = ?";
            IList<object> paramList = new List<object>();

            paramList.Add(text + "%");
            paramList.Add(true);

            if (checkLocation || !string.IsNullOrEmpty(location))
            {
                hql += " and l.Location = ?";
                paramList.Add(location);
            }

            shipperList = base.genericMgr.FindAll<Shipper>(hql, paramList.ToArray(), firstRow, maxRow);

            return new JsonResult { Data = new SelectList(shipperList, "Code", "Description", text) };
        }
        #endregion

        #region ShiftMaster
        public ActionResult _ShiftMasterDropDownList(string controlName, string controlId, string groupNo, bool? includeBlankOption, string blankOptionDescription, string blankOptionValue, string selectedValue, bool? enable)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Enable = enable;
            IList<ShiftMaster> shiftMasters = base.genericMgr.FindAll<ShiftMaster>(selectShiftMastersStatement, new object[] { });
            if (shiftMasters == null)
            {
                shiftMasters = new List<ShiftMaster>();
            }

            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                ShiftMaster shiftMaster = new ShiftMaster();
                shiftMaster.Code = blankOptionValue;
                shiftMaster.Code = blankOptionDescription;

                shiftMasters.Insert(0, shiftMaster);
            }

            return PartialView(new SelectList(shiftMasters, "Code", "Code", selectedValue));
        }

        public ActionResult _ShiftMasterAjaxLoading(string text)
        {
            AutoCompleteFilterMode fileterMode = (AutoCompleteFilterMode)Enum.Parse(typeof(AutoCompleteFilterMode), base.systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.ItemFilterMode), true);
            IList<ShiftMaster> itemList = new List<ShiftMaster>();

            if (fileterMode == AutoCompleteFilterMode.Contains)
            {
                itemList = base.genericMgr.FindAll<ShiftMaster>(selectLikeShiftMasterStatement, new object[] { "%" + text + "%" }, firstRow, maxRow);
            }
            else
            {
                itemList = base.genericMgr.FindAll<ShiftMaster>(selectLikeShiftMasterStatement, new object[] { "%" + text }, firstRow, maxRow);
            }

            return new JsonResult { Data = new SelectList(itemList, "Code", "CodeName") };
        }
        #endregion

        #region OrderOperatio

        public ActionResult _OrderOperationDropDownList(string controlName, string controlId, string selectedValue, bool? includeBlankOption, string blankOptionDescription, string blankOptionValue, bool? enable, bool? isChange, string orderNo)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.OrderOperation = enable;
            ViewBag.OrderNo = orderNo;
            ViewBag.IsChange = isChange;
            IList<OrderOperation> AssembliesList = new List<OrderOperation>();
            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                var assemblies = new OrderOperation { DisplayOperation = blankOptionValue ?? string.Empty };
                AssembliesList.Insert(0, assemblies);
            }
            return PartialView(new SelectList(AssembliesList, "Id", "DisplayOperation", selectedValue));
        }

        public ActionResult _OrderOperationAjaxLoading(string orderNo)
        {
            IList<OrderOperation> list = new List<OrderOperation>();
            if (orderNo == "")
            {
                return new JsonResult { Data = new SelectList(list, "Code", "Description") };
            }
            list = base.genericMgr.FindAll<OrderOperation>("select p from OrderOperation as p where OrderNo = ?", orderNo, firstRow, maxRow);

            return new JsonResult { Data = new SelectList(list, "Id", "DisplayOperation") };
        }

        #endregion

        #region SeqGroup
        public ActionResult _SeqGroupComboBox(string controlName, string controlId, bool? includeBlankOption, string blankOptionDescription, string blankOptionValue, string selectedValue, bool? enable,bool? isChange)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Enable = enable;
            ViewBag.IsChange = isChange;
            IList<SequenceGroup> shiftMasters = base.genericMgr.FindAll<SequenceGroup>(selectLikeSeqGroupStatement, selectedValue);
            if (shiftMasters == null)
            {
                shiftMasters = new List<SequenceGroup>();
            }

            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                SequenceGroup shiftMaster = new SequenceGroup();
                shiftMaster.Code = blankOptionValue;
                shiftMaster.Code = blankOptionDescription;

                shiftMasters.Insert(0, shiftMaster);
            }

            return PartialView(new SelectList(shiftMasters, "Code", "Code", selectedValue));
        }

        public ActionResult _SeqGroupAjaxLoading(string text)
        {
            AutoCompleteFilterMode fileterMode = (AutoCompleteFilterMode)Enum.Parse(typeof(AutoCompleteFilterMode), base.systemMgr.GetEntityPreferenceValue(EntityPreference.CodeEnum.ItemFilterMode), true);
            IList<SequenceGroup> itemList = new List<SequenceGroup>();

            if (fileterMode == AutoCompleteFilterMode.Contains)
            {
                itemList = base.genericMgr.FindAll<SequenceGroup>(selectLikeSeqGroupStatement, new object[] { "%" + text + "%" }, firstRow, maxRow);
            }
            else
            {
                itemList = base.genericMgr.FindAll<SequenceGroup>(selectLikeSeqGroupStatement, new object[] { "%" + text }, firstRow, maxRow);
            }

            return new JsonResult { Data = new SelectList(itemList, "Code", "Code") };
        }

        #endregion

        #region
        public ActionResult _StatusDropDownList(int? selectedValue)
        {
            List<Status> statusList = new List<Status>();
            Status status0 = new Status();
            status0.Code = 0;
            status0.Name = "创建";
            statusList.Add(status0);
            Status status1 = new Status();
            status1.Code = 1;
            status1.Name = "成功";
            statusList.Add(status1);
            Status status2 = new Status();
            status2.Code = 2;
            status2.Name = "失败";
            statusList.Add(status2);
            selectedValue = selectedValue.HasValue ? selectedValue.Value : 2;

            return PartialView(new SelectList(statusList, "Code", "Name", selectedValue));
        }

        class Status
        {
            public int Code { get; set; }
            public string Name { get; set; }
        }

        #endregion

        #region 要货原因代码
        public ActionResult _CreateOrderCodeComboBox(string controlName, string controlId, string selectedValue, bool? enable, bool? isChange)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Enable = enable;
            ViewBag.IsChange = isChange;
            IList<CreateOrderCode> createOrderCodeList = new List<CreateOrderCode>();

            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                createOrderCodeList = base.genericMgr.FindAll<CreateOrderCode>("from CreateOrderCode as f where f.Code = ?", selectedValue);
            }
            return PartialView(new SelectList(createOrderCodeList, "Code", "CodeDescription", selectedValue));
        }

        public ActionResult _AjaxLoadingCreateOrderCode(string text)
        {
            IList<CreateOrderCode> createOrderCodeList = base.genericMgr.FindAll<CreateOrderCode>("from CreateOrderCode as f where f.Code like ?", text + "%", firstRow, maxRow);
            return new JsonResult { Data = new SelectList(createOrderCodeList, "Code", "CodeDescription") };
        }
        #endregion



        #endregion

        #region WorkCenter
        public ActionResult _WorkCenterComboBox(string controlName, string controlId, string selectedValue, bool? enable)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Enable = enable;
            IList<WorkCenter> regionList = new List<WorkCenter>();
            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                regionList = base.genericMgr.FindAll<WorkCenter>(" from WorkCenter as r where r.Code = ? ", selectedValue);
            }
            return PartialView(new SelectList(regionList, "Code", "Code", selectedValue));
        }

        public ActionResult _AjaxLoadingWorkCenter(string text)
        {
            string hql = "from WorkCenter as r where r.Code like ? ";
            IList<object> paraList = new List<object>();
            paraList.Add(text + "%");
            IList<WorkCenter> regionList = base.genericMgr.FindAll<WorkCenter>(hql, paraList.ToArray(), firstRow, maxRow);
            return new JsonResult { Data = new SelectList(regionList, "Code", "Code", text) };
        }

        #endregion

        #region Order Op WorkCenter
        public ActionResult _OrderOpWorkCenterComboBox(string controlName, string controlId, string selectedValue, bool? enable, string orderno)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Enable = enable;
            ViewBag.OrderNo = orderno;
            IList<OrderOperation> orderOps = new List<OrderOperation>();
            IList<object> paraList = new List<object>();
            var hql = " from OrderOperation as op where WorkCenter is not null and NeedReport = 1 and op.OrderNo =?";
            paraList.Add(orderno);

            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                hql += " and op.WorkCenter =?";
                paraList.Add(selectedValue);
            }

            orderOps = base.genericMgr.FindAll<OrderOperation>(hql, paraList.ToArray());
            return PartialView(new SelectList(orderOps, "Id", "WorkCenter", selectedValue));
        }

        public ActionResult _AjaxLoadingOrderOpWorkCenter(string text, string orderno)
        {
            IList<object> paraList = new List<object>();
            var hql = " from OrderOperation as op where WorkCenter is not null and NeedReport = 1 and op.OrderNo =? and op.WorkCenter like ? ";
            paraList.Add(orderno);
            paraList.Add(text + "%");
            IList<OrderOperation> orderOps = base.genericMgr.FindAll<OrderOperation>(hql, paraList.ToArray(), firstRow, maxRow);
            return new JsonResult { Data = new SelectList(orderOps, "Id", "OperationWorkCenter", text) };
        }

        #endregion


        #region ShiftMaster
        public ActionResult _ShiftMasterComboBox(string controlName, string controlId, string selectedValue, bool? enable, bool? isChange)
        {
            ViewBag.ControlName = controlName;
            ViewBag.ControlId = controlId;
            ViewBag.Enable = enable;

            ViewBag.isChange = isChange;
            IList<ShiftMaster> shiftMasterList = new List<ShiftMaster>();
            if (selectedValue != null && selectedValue.Trim() != string.Empty)
            {
                shiftMasterList = genericMgr.FindAll<ShiftMaster>(selectEqShiftMasterStatement, selectedValue);
            }

            return PartialView(new SelectList(shiftMasterList, "Code", "CodeName", selectedValue));

        }
        public ActionResult _AjaxLoadingShiftMaster(string text)
        {
            IList<ShiftMaster> shiftMasterList = new List<ShiftMaster>();
            if (text == "")
                shiftMasterList = genericMgr.FindAll<ShiftMaster>(" from ShiftMaster ", firstRow, maxRow);
            else
                shiftMasterList = genericMgr.FindAll<ShiftMaster>(selectLikeShiftMasterStatement, new object[] { "%" + text + "%" }, firstRow, maxRow);
            return new JsonResult { Data = new SelectList(shiftMasterList, "Code", "CodeName") };
        }
        #endregion

        #region private methods

        #endregion
    }
}
