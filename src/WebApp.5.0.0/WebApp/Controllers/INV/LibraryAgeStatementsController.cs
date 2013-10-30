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
    public class LibraryAgeStatementsController : ReportBaseController
    {
        public ILocationDetailMgr IocationMgr { get; set; }
        public IReportGen reportGen { get; set; }

        public ActionResult Index()
        {
            InventoryAgeSearchModel serch = new InventoryAgeSearchModel();
            serch.TypeLocation = "0";
            TempData["Display"] = "0";
            TempData["InventoryAgeSearchModel"] = serch;
            
            //初始化数据
            ViewBag.Range1 = 7;
            ViewBag.Range2 = 14;
            ViewBag.Range3 = 30;
            ViewBag.Range4 = 60;
            ViewBag.Range5 = 90;
            ViewBag.Range6 = 180;
            ViewBag.Range7 = 360;
            ViewBag.Range8 = 720;
            ViewBag.Range9 = 1080;
            ViewBag.Range10 = 1440;
            ViewBag.Range11 = 1800;
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Menu_LibraryAge_Statements")]
        public ActionResult List(GridCommand command, InventoryAgeSearchModel searchModel)
        {
            ViewBag.Range1 = searchModel.Range1;
            ViewBag.Range2 = searchModel.Range2;
            ViewBag.Range3 = searchModel.Range3;
            ViewBag.Range4 = searchModel.Range4;
            ViewBag.Range5 = searchModel.Range5;
            ViewBag.Range6 = searchModel.Range6;
            ViewBag.Range7 = searchModel.Range7;
            ViewBag.Range8 = searchModel.Range8;
            ViewBag.Range9 = searchModel.Range9;
            ViewBag.Range10 = searchModel.Range10;
            ViewBag.Range11 = searchModel.Range11;
            TempData["Display"] = searchModel.Level;
            TempData["InventoryAgeSearchModel"] = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            IList<Location> locationList = GetReportLocations(searchModel.SAPLocation,searchModel.plantFrom, searchModel.plantTo, searchModel.regionFrom, searchModel.regionTo, searchModel.locationFrom, searchModel.locationTo);
            IList<Item> itemList = GetReportItems(searchModel.itemFrom, searchModel.itemTo);
            if (string.IsNullOrEmpty(ViewBag.Location))
                ViewBag.Location = Resources.View.LocationDetailView.LocationDetailView_Location;
            else
                ViewBag.Location = ViewBag.Location;

            if (locationList.Count > 200)
            {
                if (string.IsNullOrEmpty(searchModel.itemFrom) || string.IsNullOrEmpty(searchModel.itemTo))
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
                    && string.IsNullOrEmpty(searchModel.plantFrom) && string.IsNullOrEmpty(searchModel.plantTo) && string.IsNullOrEmpty(searchModel.regionFrom) && string.IsNullOrEmpty(searchModel.regionTo))
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


                if (string.IsNullOrEmpty(searchModel.plantFrom))
                {
                    if (!string.IsNullOrEmpty(searchModel.plantTo))
                    {
                        SaveWarningMessage("错误：第一个分厂为空的情况下，不能输入第二个分厂！");
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
            }
            else
            {
                if (string.IsNullOrEmpty(searchModel.SAPLocation))
                {
                    SaveWarningMessage("错误：根据SAP库位查询，SAP库位不能为空！");
                    return View();
                }
            }

            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Menu_LibraryAge_Statements")]
        public ActionResult _AjaxList(GridCommand command, InventoryAgeSearchModel searchModel)
        {
            IList<Location> locationList = GetReportLocations(null, searchModel.plantFrom, searchModel.plantTo, searchModel.regionFrom, searchModel.regionTo, searchModel.locationFrom, searchModel.locationTo);
            IList<Item> itemList = GetReportItems(searchModel.itemFrom, searchModel.itemTo);
            if (locationList.Count > 200)
            {
                if (string.IsNullOrEmpty(searchModel.itemFrom) || string.IsNullOrEmpty(searchModel.itemTo))
                {
                    return PartialView(new GridModel(new List<InventoryAge>()));
                }
            }
            if (itemList.Count > 200)
            {
                return PartialView(new GridModel(new List<InventoryAge>()));
            }

            if (searchModel.TypeLocation == "0")
            {
                if (string.IsNullOrEmpty(searchModel.Level))
                {
                    return PartialView(new GridModel(new List<InventoryAge>()));
                }

                if (string.IsNullOrEmpty(searchModel.itemFrom) && string.IsNullOrEmpty(searchModel.itemTo) && string.IsNullOrEmpty(searchModel.locationFrom) && string.IsNullOrEmpty(searchModel.locationTo)
                    && string.IsNullOrEmpty(searchModel.plantFrom) && string.IsNullOrEmpty(searchModel.plantTo) && string.IsNullOrEmpty(searchModel.regionFrom) && string.IsNullOrEmpty(searchModel.regionTo))
                {
                    return PartialView(new GridModel(new List<InventoryAge>()));
                }

                if (string.IsNullOrEmpty(searchModel.itemFrom))
                {
                    if (!string.IsNullOrEmpty(searchModel.itemTo))
                    {
                        return PartialView(new GridModel(new List<InventoryAge>()));
                    }
                }
                if (string.IsNullOrEmpty(searchModel.locationFrom))
                {
                    if (!string.IsNullOrEmpty(searchModel.locationTo))
                    {
                        return PartialView(new GridModel(new List<InventoryAge>()));
                    }
                }

                if (string.IsNullOrEmpty(searchModel.regionFrom))
                {
                    if (!string.IsNullOrEmpty(searchModel.regionTo))
                    {
                        return PartialView(new GridModel(new List<InventoryAge>()));
                    }
                }


                if (string.IsNullOrEmpty(searchModel.plantFrom))
                {
                    if (!string.IsNullOrEmpty(searchModel.plantTo))
                    {
                        return PartialView(new GridModel(new List<InventoryAge>()));
                    }
                }
                if (string.IsNullOrEmpty(searchModel.TheFactoryFrom))
                {
                    if (!string.IsNullOrEmpty(searchModel.TheFactoryTo))
                    {
                        return PartialView(new GridModel(new List<InventoryAge>()));
                    }
                }


            }
            else
            {
                if (string.IsNullOrEmpty(searchModel.SAPLocation))
                {
                    return PartialView(new GridModel(new List<InventoryAge>()));
                }
            }

          


            ReportSearchStatementModel reportSearchStatementModel = PrepareSearchStatement(command, searchModel);
            GridModel<InventoryAge> gridModel = GetInventoryAgeAjaxPageData<InventoryAge>(reportSearchStatementModel);

            return PartialView(gridModel);
        }

        private ReportSearchStatementModel PrepareSearchStatement(GridCommand command, InventoryAgeSearchModel searchModel)
        {
            ReportSearchStatementModel reportSearchStatementModel = new ReportSearchStatementModel();
            reportSearchStatementModel.ProcedureName = "USP_Report_InventoryAge";

            IList<Location> locationList = GetReportLocations(null
                ,searchModel.plantFrom, searchModel.plantTo, searchModel.regionFrom, searchModel.regionTo, searchModel.locationFrom, searchModel.locationTo);
            string location = string.Empty;
            foreach (var lcoList in locationList)
            {
                if (location == string.Empty)
                {
                    location = lcoList.Code;
                }
                else
                {
                    location += "," + lcoList.Code;
                }
            }


            IList<Item> itemList = GetReportItems(searchModel.itemFrom, searchModel.itemTo);
            string item = string.Empty;
            foreach (var ite in itemList)
            {
                if (item == string.Empty)
                {
                    item = ite.Code;
                }
                else
                {
                    item += "," + ite.Code;
                }
            }


            SqlParameter[] parameters = new SqlParameter[19];

            parameters[0] = new SqlParameter("@Locations", SqlDbType.VarChar, 8000);
            parameters[0].Value = location;

            parameters[1] = new SqlParameter("@Items", SqlDbType.VarChar, 8000);
            parameters[1].Value = item;

            parameters[2] = new SqlParameter("@SortDesc", SqlDbType.VarChar, 50);
            parameters[2].Value = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            parameters[3] = new SqlParameter("@PageSize", SqlDbType.Int);
            parameters[3].Value = command.PageSize;

            parameters[4] = new SqlParameter("@Page", SqlDbType.Int);
            parameters[4].Value = command.Page;

            parameters[5] = new SqlParameter("@SummaryLevel", SqlDbType.VarChar, 50);
            parameters[5].Value = searchModel.Level;

            parameters[6] = new SqlParameter("@Range1", SqlDbType.Int);
            parameters[6].Value = searchModel.Range1;

            parameters[7] = new SqlParameter("@Range2", SqlDbType.Int);
            parameters[7].Value = searchModel.Range2;

            parameters[8] = new SqlParameter("@Range3", SqlDbType.Int);
            parameters[8].Value = searchModel.Range3;

            parameters[9] = new SqlParameter("@Range4", SqlDbType.Int);
            parameters[9].Value = searchModel.Range4;


            parameters[10] = new SqlParameter("@Range5", SqlDbType.Int);
            parameters[10].Value = searchModel.Range5;


            parameters[11] = new SqlParameter("@Range6", SqlDbType.Int);
            parameters[11].Value = searchModel.Range6;

            parameters[12] = new SqlParameter("@Range7", SqlDbType.Int);
            parameters[12].Value = searchModel.Range7;

            parameters[13] = new SqlParameter("@Range8", SqlDbType.Int);
            parameters[13].Value = searchModel.Range8;

            parameters[14] = new SqlParameter("@Range9", SqlDbType.Int);
            parameters[14].Value = searchModel.Range9;


            parameters[15] = new SqlParameter("@Range10", SqlDbType.Int);
            parameters[15].Value = searchModel.Range10;

            parameters[16] = new SqlParameter("@Range11", SqlDbType.Int);
            parameters[16].Value = searchModel.Range11;

            parameters[17] = new SqlParameter("@IsSummaryBySAPLoc", SqlDbType.Bit);
            parameters[17].Value = searchModel.TypeLocation == "1" ? true : false; ;

         
            reportSearchStatementModel.Parameters = parameters;

            return reportSearchStatementModel;
        }
    }
}
