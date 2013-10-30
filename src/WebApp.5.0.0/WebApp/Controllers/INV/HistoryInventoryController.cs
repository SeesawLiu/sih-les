using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.INV;
using com.Sconit.Entity.MD;
using com.Sconit.PrintModel.INV;
using com.Sconit.Service;
using com.Sconit.Utility.Report;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels.INV;
using com.Sconit.Web.Util;
using Telerik.Web.Mvc;
using com.Sconit.Utility.Report.Operator;
using com.Sconit.Web.Models.ReportModels;
using System.Data.SqlClient;
using System.Data;
using com.Sconit.Service.Impl;

namespace com.Sconit.Web.Controllers.INV
{
    public class HistoryInventoryController : ReportBaseController
    {
        public ILocationDetailMgr IocationMgr { get; set; }
        public IReportGen reportGen { get; set; }

        public ActionResult Index()
        {
            HistoryInventorySearchModel serch = new HistoryInventorySearchModel();
            serch.TypeLocation = "0";
            TempData["Display"] = "0";
            TempData["HistoryInventorySearchModel"] = serch;
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Menu_History_Inventory")]
        public ActionResult List(GridCommand command, HistoryInventorySearchModel searchModel)
        {

           
            
           

            if (!string.IsNullOrEmpty(searchModel.SAPLocation))
                TempData["SAPLocation"] = searchModel.SAPLocation;

            TempData["Display"] = searchModel.Level;
            TempData["HistoryInventorySearchModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);

            IList<Location> locationList = GetReportLocations(searchModel.SAPLocation, searchModel.plantFrom, searchModel.plantTo, searchModel.regionFrom, searchModel.regionTo, searchModel.locationFrom, searchModel.locationTo);
            IList<Item> itemList = GetReportItems(searchModel.itemFrom, searchModel.itemTo);
            if (string.IsNullOrEmpty(ViewBag.Location))
                ViewBag.Location = Resources.View.LocationDetailView.LocationDetailView_Location;
            else
                ViewBag.Location = ViewBag.Location;



            if (string.IsNullOrEmpty(searchModel.HistoryDate.ToString()))
            {
                SaveWarningMessage("时间必填！");
                return View();
            }

            if (locationList.Count > 200)
            {
                if (string.IsNullOrEmpty(searchModel.itemFrom) && string.IsNullOrEmpty(searchModel.itemTo))
                {
                    SaveErrorMessage("物料代码为必选项！");
                    return View();
                }
            }
            if (itemList.Count > 200)
            {
                SaveErrorMessage("零件超过200个！");
                return View();
            }
            if (searchModel.TypeLocation == "0")
            {
                if (string.IsNullOrEmpty(searchModel.Level))
                {
                    SaveWarningMessage("汇总至为必选项！");
                    return View();
                }
                if (string.IsNullOrEmpty(searchModel.itemFrom) && string.IsNullOrEmpty(searchModel.itemTo) && string.IsNullOrEmpty(searchModel.locationFrom) && string.IsNullOrEmpty(searchModel.locationTo)
                    && string.IsNullOrEmpty(searchModel.plantFrom) && string.IsNullOrEmpty(searchModel.plantTo) && string.IsNullOrEmpty(searchModel.regionFrom) && string.IsNullOrEmpty(searchModel.regionTo)
                    && string.IsNullOrEmpty(searchModel.TheFactoryFrom) && string.IsNullOrEmpty(searchModel.TheFactoryTo))
                {
                    SaveWarningMessage("请根据条件查询！");
                    return View();
                }
                if (string.IsNullOrEmpty(searchModel.itemFrom))
                {
                    if (!string.IsNullOrEmpty(searchModel.itemTo))
                    {
                        SaveWarningMessage("错误：第一个物料为空的情况下，不能输入第二个物料！");
                        return View();
                    }
                }
                if (string.IsNullOrEmpty(searchModel.locationFrom))
                {
                    if (!string.IsNullOrEmpty(searchModel.locationTo))
                    {
                        SaveWarningMessage("错误：第一个库位为空的情况下，不能输入第二个库位！");
                        return View();
                    }
                }

                if (string.IsNullOrEmpty(searchModel.regionFrom))
                {
                    if (!string.IsNullOrEmpty(searchModel.regionTo))
                    {
                        SaveWarningMessage("错误：第一个区域为空的情况下，不能输入第二个区域！");
                        return View();
                    }
                }

                if (string.IsNullOrEmpty(searchModel.TheFactoryFrom))
                {
                    if (!string.IsNullOrEmpty(searchModel.TheFactoryTo))
                    {
                        SaveWarningMessage("错误：第一个工厂为空的情况下，不能输入第二个工厂！");
                        return View();
                    }
                }

                if (string.IsNullOrEmpty(searchModel.plantFrom))
                {
                    if (!string.IsNullOrEmpty(searchModel.plantTo))
                    {
                        SaveWarningMessage("错误：第一个分厂为空的情况下，不能输入第二个分厂！");
                        return View();
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(searchModel.SAPLocation))
                {
                    SaveWarningMessage("错误：SAP库位查询时，SAP库位不能为空！");
                    return View();
                }
            }

         

            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Menu_History_Inventory")]
        public ActionResult _AjaxList(GridCommand command, HistoryInventorySearchModel searchModel)
        {
            IList<Location> locationList = GetReportLocations(searchModel.SAPLocation, searchModel.plantFrom, searchModel.plantTo, searchModel.regionFrom, searchModel.regionTo, searchModel.locationFrom, searchModel.locationTo);
            IList<Item> itemList = GetReportItems(searchModel.itemFrom, searchModel.itemTo);
            if (locationList.Count > 200)
            {
                if (string.IsNullOrEmpty(searchModel.itemFrom) && string.IsNullOrEmpty(searchModel.itemTo))
                {
                    return PartialView(new GridModel(new List<HistoryInventory>()));
                }
            }
            if (itemList.Count > 200)
            {
                return PartialView(new GridModel(new List<HistoryInventory>()));
            }
            if (string.IsNullOrEmpty(searchModel.HistoryDate.ToString()))
            {
                return PartialView(new GridModel(new List<HistoryInventory>()));
            }

            if (searchModel.TypeLocation == "0")
            {
                if (string.IsNullOrEmpty(searchModel.Level))
                {
                    return PartialView(new GridModel(new List<HistoryInventory>()));
                }

                if (string.IsNullOrEmpty(searchModel.itemFrom) && string.IsNullOrEmpty(searchModel.itemTo) && string.IsNullOrEmpty(searchModel.locationFrom) && string.IsNullOrEmpty(searchModel.locationTo)
                    && string.IsNullOrEmpty(searchModel.plantFrom) && string.IsNullOrEmpty(searchModel.plantTo) && string.IsNullOrEmpty(searchModel.regionFrom) && string.IsNullOrEmpty(searchModel.regionTo)
                     && string.IsNullOrEmpty(searchModel.TheFactoryFrom) && string.IsNullOrEmpty(searchModel.TheFactoryTo))
                {
                    return PartialView(new GridModel(new List<HistoryInventory>()));
                }

                if (string.IsNullOrEmpty(searchModel.itemFrom))
                {
                    if (!string.IsNullOrEmpty(searchModel.itemTo))
                    {
                        return PartialView(new GridModel(new List<HistoryInventory>()));
                    }
                }
                if (string.IsNullOrEmpty(searchModel.locationFrom))
                {
                    if (!string.IsNullOrEmpty(searchModel.locationTo))
                    {
                        return PartialView(new GridModel(new List<HistoryInventory>()));
                    }
                }

                if (string.IsNullOrEmpty(searchModel.regionFrom))
                {
                    if (!string.IsNullOrEmpty(searchModel.regionTo))
                    {
                        return PartialView(new GridModel(new List<HistoryInventory>()));
                    }
                }

                if (string.IsNullOrEmpty(searchModel.TheFactoryFrom))
                {
                    if (!string.IsNullOrEmpty(searchModel.TheFactoryTo))
                    {
                        return PartialView(new GridModel(new List<HistoryInventory>()));
                    }
                }

                if (string.IsNullOrEmpty(searchModel.plantFrom))
                {
                    if (!string.IsNullOrEmpty(searchModel.plantTo))
                    {
                        return PartialView(new GridModel(new List<HistoryInventory>()));
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(searchModel.SAPLocation))
                {
                    return PartialView(new GridModel(new List<HistoryInventory>()));
                }
            }
          

            ReportSearchStatementModel reportSearchStatementModel = PrepareSearchStatement(command, searchModel);

            GridModel<HistoryInventory> gridModel = GetHistoryInvAjaxPageData<HistoryInventory>(reportSearchStatementModel);
           //IList<HistoryInventory> gridModel = IocationMgr.GetHistoryLocationDetails(locationCode, list, searchModel.HistoryDate, SortDescriptors, command.PageSize, command.Page);
          
            return PartialView(gridModel);
        }

        private ReportSearchStatementModel PrepareSearchStatement(GridCommand command, HistoryInventorySearchModel searchModel)
        {
            
            ReportSearchStatementModel reportSearchStatementModel = new ReportSearchStatementModel();
            reportSearchStatementModel.ProcedureName = "USP_Report_GetHistoryInv";


            IList<Location> locationList = GetReportLocations(searchModel.SAPLocation, searchModel.plantFrom, searchModel.plantTo, searchModel.regionFrom, searchModel.regionTo, searchModel.locationFrom, searchModel.locationTo);
            string locations = string.Empty;
            foreach (var lcoList in locationList)
            {
                if (locations == string.Empty)
                {
                    locations = lcoList.Code;
                }
                else
                {
                    locations += "," + lcoList.Code;
                }
            }


            IList<Item> itemList = GetReportItems(searchModel.itemFrom, searchModel.itemTo);
            string items = string.Empty;
            foreach (var ite in itemList)
            {
                if (items == string.Empty)
                {
                    items = ite.Code;
                }
                else
                {
                    items += "," + ite.Code;
                }
            }



            SqlParameter[] parm = new SqlParameter[8];

            parm[0] = new SqlParameter("@Locations", SqlDbType.VarChar, 50);
            parm[0].Value = locations;

          
            parm[1] = new SqlParameter("@Items", SqlDbType.VarChar, 4000);
            parm[1].Value = items;

            parm[2] = new SqlParameter("@HistoryData", SqlDbType.DateTime);
            parm[2].Value = searchModel.HistoryDate;

            parm[3] = new SqlParameter("@SortDesc", SqlDbType.VarChar, 100);
            parm[3].Value = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            parm[4] = new SqlParameter("@PageSize", SqlDbType.Int);
            parm[4].Value =command.PageSize;

            parm[5] = new SqlParameter("@Page", SqlDbType.Int);
            parm[5].Value = command.Page;

            parm[6] = new SqlParameter("@IsSummaryBySAPLoc", SqlDbType.Bit);
            parm[6].Value = searchModel.TypeLocation == "1" ? true : false; ;


            parm[7] = new SqlParameter("@SummaryLevel", SqlDbType.VarChar, 50);
            parm[7].Value = searchModel.Level;
            
            reportSearchStatementModel.Parameters = parm;

            return reportSearchStatementModel;
        }

    }
}
