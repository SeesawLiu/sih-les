using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using com.Sconit.Service;
using Telerik.Web.Mvc;
using com.Sconit.Web.Models.SearchModels.INV;
using com.Sconit.Entity.INV;
using com.Sconit.Web.Models;
using com.Sconit.Web.Util;

namespace com.Sconit.Web.Controllers.INV
{
    public class HuLocationLotDetailController : WebAppBaseController
    {
        //
        // GET: /HuLocationLotDetail/

        private static string selectCountStatement = "select count(*) from LocationLotDetail as l";

        private static string selectStatement = "from  LocationLotDetail as l";

        public ActionResult Index()
        {
            return View();
        }

         [SconitAuthorize(Permissions = "Menu_Hu_Inventory")]
        public ActionResult List(GridCommand command, LocationLotDetailSearchModel searchModel)
        {
             this.ProcessSearchModel(command, searchModel);
             if (!string.IsNullOrEmpty(searchModel.Location))
            {
                TempData["_AjaxMessage"] = "";
            }
            else
            {
                SaveWarningMessage("查询条件中库位为必选项。");
            }
            ViewBag.PageSize = this.ProcessPageSize(command.PageSize);

            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Menu_Hu_Inventory")]
         public ActionResult _AjaxList(GridCommand command, LocationLotDetailSearchModel searchModel) 
        {
            if (string.IsNullOrEmpty(searchModel.Location))
            {
                return PartialView(new GridModel(new List<LocationLotDetail>()));
            }
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
           
            GridModel<LocationLotDetail> List = GetAjaxPageData<LocationLotDetail>(searchStatementModel, command);
            if (List.Data.Count()>0)
            {
                IList<string> ItemCodes = List.Data.Select(i => i.Item).Distinct().ToList();
                //IList<IpMaster> StatusAndIpNo = base.genericMgr.FindAll<IpMaster>("select r from IpMaster as r where IpNo in ()");
                string sql = "select Code,Desc1,RefCode from MD_Item where Code in (";
                foreach (string ItemCode in ItemCodes)
                {
                    sql += "'" + ItemCode + "',";
                }
                sql = sql.Substring(0, sql.Length - 1) + ")";

                IList<object[]> objectList = base.genericMgr.FindAllWithNativeSql<object[]>(sql);
                foreach (var item in objectList)
                {
                    foreach (LocationLotDetail locationLotDetail in List.Data)
                    {
                        if ((string)item[0] == locationLotDetail.Item)
                        {
                            locationLotDetail.ItemDescription = (string)item[1];
                            locationLotDetail.ReferenceItemCode = (string)item[2];
                        }
                    }
                }
            }
            return PartialView(List);
        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, LocationLotDetailSearchModel searchModel)
        {
            string whereStatement = "where HuId is not Null";

            IList<object> param = new List<object>();


            HqlStatementHelper.AddEqStatement("Location", searchModel.Location, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("HuId", searchModel.HuId, "l", ref whereStatement, param);

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by l.CreateDate desc";
            }
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

    }
}
