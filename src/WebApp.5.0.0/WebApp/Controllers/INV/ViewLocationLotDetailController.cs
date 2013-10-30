using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using com.Sconit.Web.Util;
using com.Sconit.Web.Models.SearchModels.ORD;
using com.Sconit.Web.Models;
using com.Sconit.Entity.ORD;
using System.Text;
using com.Sconit.Service;
using com.Sconit.Utility;
using com.Sconit.Web.Models.SearchModels.BIL;
using com.Sconit.Entity.BIL;
using com.Sconit.Web.Models.SearchModels.INV;
using com.Sconit.Entity.INV;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.VIEW;
using System.Data.SqlClient;
using System.Data;
using com.Sconit.Utility.Report;
using System.ComponentModel;
using com.Sconit.Persistence;
using com.Sconit.Entity.Exception;


namespace com.Sconit.Web.Controllers.INV
{
    public class ViewLocationLotDetailController : ReportBaseController
    {

        public ISqlDao sqlDao { get; set; }


        private static string selectCountStatement = @"select count(*) 
                                                        from LocationDetailView as a
                                                         ";

        /// <summary>
        /// 
        /// </summary>
        private static string selectStatement = @"select a from LocationDetailView as a";

        public IReportGen reportGen { get; set; }

        #region public
        public ActionResult Index()
        {
            LocationLotDetailSearchModel serch = new LocationLotDetailSearchModel();
            serch.TypeLocation = "0";
            TempData["Display"] = "0";
            TempData["LocationLotDetailSearchModel"] = serch;
            TempData["LocationDetailView"] = null;
            return View();
        }
  

        [GridAction]
        [SconitAuthorize(Permissions = "Menu_Inventory_ViewInventory")]
        public ActionResult List(GridCommand command, LocationLotDetailSearchModel searchModel)
        {

            //TempData["Display"] = searchModel.Level;
            //TempData["LocationLotDetailSearchModel"] = searchModel;
            //ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            //if (searchModel.hideSupper)
            //    ViewBag.HideParty = false;
            //else
            //    ViewBag.HideParty = true;
            //if(searchModel.hideLotNo)
            //    ViewBag.HideLotNo = false;
            //else
            //    ViewBag.HideLotNo = true;

            //IList<Location> locationList = GetReportLocations(searchModel.SAPLocation,searchModel.plantFrom, searchModel.plantTo, searchModel.regionFrom, searchModel.regionTo, searchModel.locationFrom, searchModel.locationTo);
            //IList<Item> itemList = GetReportItems(searchModel.itemFrom, searchModel.itemTo);
            //if (string.IsNullOrEmpty(ViewBag.Location))
            //    ViewBag.Location = Resources.View.LocationDetailView.LocationDetailView_Location;
            //else
            //ViewBag.Location = ViewBag.Location;


            //if (locationList.Count > 200)
            //{
            //    if (string.IsNullOrEmpty(searchModel.itemFrom) && string.IsNullOrEmpty(searchModel.itemTo))
            //    {
            //        SaveErrorMessage("物料代码为必选项！");
            //        return View();
            //    }
            //}
            //if (itemList.Count > 200)
            //{
            //    SaveErrorMessage("零件超过200个！");
            //    return View();
            //}

            //if (searchModel.TypeLocation == "0")
            //{
            //    if (string.IsNullOrEmpty(searchModel.Level))
            //    {
            //        SaveWarningMessage("汇总至为必选项！");
            //        return View();
            //    }
            //    if (string.IsNullOrEmpty(searchModel.itemFrom) && string.IsNullOrEmpty(searchModel.itemTo) && string.IsNullOrEmpty(searchModel.locationFrom) && string.IsNullOrEmpty(searchModel.locationTo)
            //        && string.IsNullOrEmpty(searchModel.plantFrom) && string.IsNullOrEmpty(searchModel.plantTo) && string.IsNullOrEmpty(searchModel.regionFrom) && string.IsNullOrEmpty(searchModel.regionTo)
            //        && string.IsNullOrEmpty(searchModel.TheFactory) && string.IsNullOrEmpty(searchModel.TheFactoryTo))
            //    {
            //        SaveWarningMessage("请根据条件查询！");
            //        return View();
            //    }



            //    if (string.IsNullOrEmpty(searchModel.itemFrom))
            //    {
            //        if (!string.IsNullOrEmpty(searchModel.itemTo))
            //        {
            //            SaveWarningMessage("错误：第一个物料为空的情况下，不能输入第二个物料！");
            //            return View();
            //        }
            //    }
            //    if (string.IsNullOrEmpty(searchModel.locationFrom))
            //    {
            //        if (!string.IsNullOrEmpty(searchModel.locationTo))
            //        {
            //            SaveWarningMessage("错误：第一个库位为空的情况下，不能输入第二个库位！");
            //            return View();
            //        }
            //    }

            //    if (string.IsNullOrEmpty(searchModel.regionFrom))
            //    {
            //        if (!string.IsNullOrEmpty(searchModel.regionTo))
            //        {
            //            SaveWarningMessage("错误：第一个区域为空的情况下，不能输入第二个区域！");
            //            return View();
            //        }
            //    }

            //    if (string.IsNullOrEmpty(searchModel.TheFactory))
            //    {
            //        if (!string.IsNullOrEmpty(searchModel.TheFactoryTo))
            //        {
            //            SaveWarningMessage("错误：第一个工厂为空的情况下，不能输入第二个工厂！");
            //            return View();
            //        }
            //    }

            //    if (string.IsNullOrEmpty(searchModel.plantFrom))
            //    {
            //        if (!string.IsNullOrEmpty(searchModel.plantTo))
            //        {
            //            SaveWarningMessage("错误：第一个分厂为空的情况下，不能输入第二个分厂！");
            //            return View();
            //        }
            //    }
            //}
            //else
            //{
            //    if (string.IsNullOrEmpty(searchModel.SAPLocation))
            //    {
            //        SaveWarningMessage("错误：根据SAP库位查询，SAP库位不能为空！");
            //        return View();
            //    }
            //}
            //string locations = string.Empty;
            //string items = string.Empty;
            //string[] locationArr = searchModel.locations.Split(',');
            //string[] itemArr = searchModel.items.Split(',');
            //for (int i = 0; i < locationArr.Length; i++)
            //{
            //    if (!string.IsNullOrWhiteSpace(locationArr[i]) && locationArr[i] != "null" && locationArr[i] != "NULL")
            //    {
            //        locations += locationArr[i]+",";
            //    }
            //}

            //for (int i = 0; i < itemArr.Length; i++)
            //{
            //    if (!string.IsNullOrWhiteSpace(itemArr[i]) && itemArr[i] != "null" && itemArr[i] != "NULL")
            //    {
            //        items += itemArr[i] + ",";
            //    }
            //}
            //ViewBag.Locations = locations;
            //ViewBag.Items = items;
            ViewBag.IsShowCSSupplier = searchModel.IsShowCSSupplier;
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Menu_Inventory_ViewInventory")]
        public ActionResult _AjaxList(GridCommand command, LocationLotDetailSearchModel searchModel)
        {
            if (searchModel.TypeLocation == "0")
            {
                if (string.IsNullOrEmpty(searchModel.Level))
                {
                    return PartialView(new GridModel(new List<LocationDetailView>()));
                }

                if (string.IsNullOrEmpty(searchModel.itemFrom) && string.IsNullOrEmpty(searchModel.itemTo) && string.IsNullOrEmpty(searchModel.locationFrom) && string.IsNullOrEmpty(searchModel.locationTo)
                    && string.IsNullOrEmpty(searchModel.plantFrom) && string.IsNullOrEmpty(searchModel.plantTo) && string.IsNullOrEmpty(searchModel.regionFrom) && string.IsNullOrEmpty(searchModel.regionTo))
                {
                    return PartialView(new GridModel(new List<LocationDetailView>()));
                }

                if (string.IsNullOrEmpty(searchModel.itemFrom))
                {
                    if (!string.IsNullOrEmpty(searchModel.itemTo))
                    {
                        return PartialView(new GridModel(new List<LocationDetailView>()));
                    }
                }
                if (string.IsNullOrEmpty(searchModel.locationFrom))
                {
                    if (!string.IsNullOrEmpty(searchModel.locationTo))
                    {
                        return PartialView(new GridModel(new List<LocationDetailView>()));
                    }
                }

                if (string.IsNullOrEmpty(searchModel.regionFrom))
                {
                    if (!string.IsNullOrEmpty(searchModel.regionTo))
                    {
                        return PartialView(new GridModel(new List<LocationDetailView>()));
                    }
                }

                if (string.IsNullOrEmpty(searchModel.TheFactory))
                {
                    if (!string.IsNullOrEmpty(searchModel.TheFactoryTo))
                    {
                        return PartialView(new GridModel(new List<LocationDetailView>()));
                    }
                }

                if (string.IsNullOrEmpty(searchModel.plantFrom))
                {
                    if (!string.IsNullOrEmpty(searchModel.plantTo))
                    {
                        return PartialView(new GridModel(new List<LocationDetailView>()));
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(searchModel.SAPLocation))
                {
                    return PartialView(new GridModel(new List<LocationDetailView>()));
                }
            }
           
     
            IList<Location> locationList = GetReportLocations(searchModel.SAPLocation,searchModel.plantFrom, searchModel.plantTo, searchModel.regionFrom, searchModel.regionTo, searchModel.locationFrom, searchModel.locationTo);
            IList<Item> itemList = GetReportItems(searchModel.itemFrom, searchModel.itemTo);
            if (locationList.Count > 200)
            {
                if (string.IsNullOrEmpty(searchModel.itemFrom) && string.IsNullOrEmpty(searchModel.itemTo))
                {
                    return PartialView(new GridModel(new List<LocationDetailView>()));
                }
            }
            if (itemList.Count > 200)
            {
                return PartialView(new GridModel(new List<LocationDetailView>()));
            }


            ReportSearchStatementModel reportSearchStatementModel = PrepareSearchStatement(command, searchModel);
           
            GridModel<LocationDetailView> gridModel = GetReportAjaxPageData<LocationDetailView>(reportSearchStatementModel);

            //foreach (LocationDetailView locationDetail in gridModel.Data)
            //{
            //    Item item = base.genericMgr.FindById<Item>(locationDetail.Item);
            //    locationDetail.ItemDescription = item.Description;
            //    locationDetail.Uom = item.Uom;
            //    //locationDetail.Name = base.genericMgr.FindById<Location>(locationDetail.Location).Name; ;
            //}
            reportSearchStatementModel.Parameters[3].Value = gridModel.Total;
            TempData["LocationDetailView"] = reportSearchStatementModel;
            return PartialView(gridModel);
        }

        #region 导出
        public void SaveToClient()
        {
            ReportSearchStatementModel reportSearchStatementModel = TempData["LocationDetailView"] as ReportSearchStatementModel;
            TempData["LocationDetailView"] = reportSearchStatementModel;
            GridModel<LocationDetailView> gridModel = GetReportAjaxPageData<LocationDetailView>(reportSearchStatementModel);
           
            IList<object> data = new List<object>();
            data.Add(gridModel.Data);
            reportGen.WriteToClient("LocationDetailView.xls", data, "LocationDetailView.xls");
        }

        public void ExportLocationDetailXLS(LocationLotDetailSearchModel searchModel)
        {

            DataTable locationArrayTable = new DataTable("LocationArrayTable");
            locationArrayTable.Columns.Add("Field", typeof(string));
            if (!string.IsNullOrEmpty(searchModel.locations))
            {
                string loctions = searchModel.locations.Replace("\r\n", ",");
                loctions = loctions.Replace("\n", ",");
                string[] locationArr = loctions.Split(',');
                for (int i = 0; i < locationArr.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(locationArr[i]) && locationArr[i] != "null" && locationArr[i] != "NULL")
                    {
                        locationArrayTable.Rows.Add(locationArr[i]);
                    }
                }
            }

            DataTable itemArrayTable = new DataTable("ItemArrayTable");
            itemArrayTable.Columns.Add("Field", typeof(string));
            if (!string.IsNullOrEmpty(searchModel.items))
            {
                string items = searchModel.items.Replace("\r\n", ",");
                items = items.Replace("\n", ",");
                string[] itemArr = items.Split(',');
                for (int i = 0; i < itemArr.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(itemArr[i]) && itemArr[i] != "null" && itemArr[i] != "NULL")
                    {
                        itemArrayTable.Rows.Add(itemArr[i]);
                    }
                }
            }

            SqlParameter[] parameters = new SqlParameter[9];
            parameters[0] = new SqlParameter("@LocationArrayTable", System.Data.SqlDbType.Structured);
            parameters[0].Value = locationArrayTable;

            parameters[1] = new SqlParameter("@IsSapLocation", System.Data.SqlDbType.Bit);
            parameters[1].Value = searchModel.IsSapLocation;

            parameters[2] = new SqlParameter("@ItemArrayTable", System.Data.SqlDbType.Structured);
            parameters[2].Value = itemArrayTable;

            parameters[3] = new SqlParameter("@SortCloumn", System.Data.SqlDbType.VarChar, 50);
            parameters[3].Value =  string.Empty;

            parameters[4] = new SqlParameter("@SortRule", System.Data.SqlDbType.VarChar, 50);
            parameters[4].Value =string.Empty;

            parameters[5] = new SqlParameter("@PageSize", SqlDbType.Int);
            parameters[5].Value = 65530;

            parameters[6] = new SqlParameter("@Pager", SqlDbType.Int);
            parameters[6].Value = 1;

            parameters[7] = new SqlParameter("@UserId", SqlDbType.Int);
            parameters[7].Value = CurrentUser.Id;

            parameters[8] = new SqlParameter("@RowCount", System.Data.SqlDbType.VarChar, 50);
            parameters[8].Direction = ParameterDirection.Output;

            IList<LocationDetailView> locationDetailView = new List<LocationDetailView>();
            try
            {
                DataSet dataSet = sqlDao.GetDatasetByStoredProcedure("USP_Search_LocationLotDet", parameters, false);

                //Item, Desc1, RefCode, Uom, Location, Qty, CSQty, QulifiedQty, InspectedQty, RejectedQty 
                if (dataSet.Tables[0] != null && dataSet.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        //row.ItemArray[0].ToString()
                        LocationDetailView lotDet = new LocationDetailView();
                        lotDet.Item = row.ItemArray[0].ToString();
                        lotDet.ItemDescription = row.ItemArray[1].ToString();
                        lotDet.RefrenceItemCode = row.ItemArray[2].ToString();
                        lotDet.Uom = row.ItemArray[3].ToString();
                        lotDet.Location = row.ItemArray[4].ToString();
                        lotDet.Qty = Convert.ToDecimal(row.ItemArray[5]);
                        lotDet.ConsignmentQty = Convert.ToDecimal(row.ItemArray[6]);
                        lotDet.QualifyQty = Convert.ToDecimal(row.ItemArray[7]);
                        lotDet.InspectQty = Convert.ToDecimal(row.ItemArray[8]);
                        lotDet.RejectQty = Convert.ToDecimal(row.ItemArray[9]);
                        locationDetailView.Add(lotDet);
                    }
                }
                ExportToXLS<LocationDetailView>("ExportLocationDetailXLS", "xls", locationDetailView);
            }
            catch (BusinessException be)
            {
                SaveBusinessExceptionMessage(be);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.InnerException != null)
                    {
                        SaveErrorMessage(ex.InnerException.InnerException.Message);
                    }
                    else
                    {
                        SaveErrorMessage(ex.InnerException.Message);
                    }
                }
                else
                {
                    SaveErrorMessage(ex.Message);
                }
            }
        }
        #endregion

        #region 
        public ActionResult CopyIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Menu_Inventory_ViewInventory")]
        public ActionResult _CopySearchResult(GridCommand command, LocationLotDetailSearchModel searchModel)
        {
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxCopyList(GridCommand command, LocationLotDetailSearchModel searchModel)
        {

            DataTable locationArrayTable = new DataTable("LocationArrayTable");
            locationArrayTable.Columns.Add("Field", typeof(string));
            if (!string.IsNullOrEmpty(searchModel.locations))
            {
                string loctions = searchModel.locations.Replace("\r\n", ",");
                loctions = loctions.Replace("\n", ",");
                string[] locationArr = loctions.Split(',');
                for (int i = 0; i < locationArr.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(locationArr[i]) && locationArr[i] != "null" && locationArr[i] != "NULL")
                    {
                        locationArrayTable.Rows.Add(locationArr[i]);
                    }
                }
            }

            DataTable itemArrayTable = new DataTable("ItemArrayTable");
            itemArrayTable.Columns.Add("Field", typeof(string));
            if (!string.IsNullOrEmpty(searchModel.items))
            {
                string items = searchModel.items.Replace("\r\n", ",");
                items = items.Replace("\n", ",");
                string[] itemArr = items.Split(',');
                for (int i = 0; i < itemArr.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(itemArr[i]) && itemArr[i] != "null" && itemArr[i] != "NULL")
                    {
                        itemArrayTable.Rows.Add(itemArr[i]);
                    }
                }
            }

            SqlParameter[] parameters = new SqlParameter[10];
            parameters[0] = new SqlParameter("@LocationArrayTable", System.Data.SqlDbType.Structured);
            parameters[0].Value = locationArrayTable;

            parameters[1] = new SqlParameter("@IsSapLocation", System.Data.SqlDbType.Bit);
            parameters[1].Value = searchModel.IsSapLocation;

            parameters[2] = new SqlParameter("@ItemArrayTable", System.Data.SqlDbType.Structured);
            parameters[2].Value = itemArrayTable;

            parameters[3] = new SqlParameter("@IsShowCSSupplier", System.Data.SqlDbType.Bit);
            parameters[3].Value = searchModel.IsShowCSSupplier;

            parameters[4] = new SqlParameter("@SortCloumn", System.Data.SqlDbType.VarChar,50);
            parameters[4].Value =command.SortDescriptors.Count>0? command.SortDescriptors[0].Member:string.Empty;

            parameters[5] = new SqlParameter("@SortRule", System.Data.SqlDbType.VarChar, 50);
            parameters[5].Value = command.SortDescriptors.Count > 0 ? command.SortDescriptors[0].SortDirection == ListSortDirection.Descending ? "desc" : "asc":string.Empty;

            parameters[6] = new SqlParameter("@PageSize", SqlDbType.Int);
            parameters[6].Value = command.PageSize;

            parameters[7] = new SqlParameter("@Pager", SqlDbType.Int);
            parameters[7].Value = command.Page;

            parameters[8] = new SqlParameter("@UserId", SqlDbType.Int);
            parameters[8].Value = CurrentUser.Id;

            parameters[9] = new SqlParameter("@RowCount", System.Data.SqlDbType.VarChar, 50);
            parameters[9].Direction = ParameterDirection.Output;

            IList<LocationDetailView> locationDetailView = new List<LocationDetailView>();
            try
            {
                DataSet dataSet = sqlDao.GetDatasetByStoredProcedure("USP_Search_LocationLotDet", parameters, false);

                //Item, Desc1, RefCode, Uom, Location, Qty, CSQty, QulifiedQty, InspectedQty, RejectedQty 
                //Item, Desc1, RefCode, Uom, Location, Qty, CSSupplier, QulifiedQty, InspectedQty, RejectedQty
                if (dataSet.Tables[0] != null && dataSet.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                       //row.ItemArray[0].ToString()
                        LocationDetailView lotDet = new LocationDetailView();
                        lotDet.Item = row.ItemArray[0].ToString();
                        lotDet.ItemDescription = row.ItemArray[1].ToString();
                        lotDet.RefrenceItemCode = row.ItemArray[2].ToString();
                        lotDet.Uom = row.ItemArray[3].ToString();
                        lotDet.Location = row.ItemArray[4].ToString();
                        lotDet.Qty = Convert.ToDecimal(row.ItemArray[5]);
                        if (searchModel.IsShowCSSupplier)
                        {
                            lotDet.suppliers = row.ItemArray[6].ToString();
                        }
                        else
                        {
                            lotDet.ConsignmentQty = Convert.ToDecimal(row.ItemArray[6]);
                        }
                        lotDet.QualifyQty = Convert.ToDecimal(row.ItemArray[7]);
                        lotDet.InspectQty = Convert.ToDecimal(row.ItemArray[8]);
                        lotDet.RejectQty = Convert.ToDecimal(row.ItemArray[9]);
                        locationDetailView.Add(lotDet);
                    }
                }
            }
            catch (BusinessException be)
            {
                SaveBusinessExceptionMessage(be);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.InnerException != null)
                    {
                        SaveErrorMessage(ex.InnerException.InnerException.Message);
                    }
                    else
                    {
                        SaveErrorMessage(ex.InnerException.Message);
                    }
                }
                else
                {
                    SaveErrorMessage(ex.Message);
                }
            }
            GridModel<LocationDetailView> gridModel = new GridModel<LocationDetailView>();
            gridModel.Total = string.IsNullOrWhiteSpace(parameters[8].Value.ToString()) ? 0 : Convert.ToInt32(parameters[8].Value);
            gridModel.Data = locationDetailView;
            return PartialView(gridModel);
        }
        #endregion

        #endregion
        #region private

        private ReportSearchStatementModel PrepareSearchStatement(GridCommand command, LocationLotDetailSearchModel searchModel)
        {
            ReportSearchStatementModel reportSearchStatementModel = new ReportSearchStatementModel();
            reportSearchStatementModel.ProcedureName = "USP_Report_RealTimeLocationDet";

            IList<Location> locationList = GetReportLocations(searchModel.SAPLocation,searchModel.plantFrom, searchModel.plantTo, searchModel.regionFrom, searchModel.regionTo, searchModel.locationFrom, searchModel.locationTo);
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
           

            SqlParameter[] parameters = new SqlParameter[9];

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

            //逻辑修改，默认按供应商group
            parameters[6] = new SqlParameter("@IsGroupByManufactureParty", SqlDbType.Bit);
            parameters[6].Value = true;

            parameters[7] = new SqlParameter("@IsGroupByLotNo", SqlDbType.Bit);
            parameters[7].Value = searchModel.hideLotNo;


            parameters[8] = new SqlParameter("@IsSummaryBySAPLoc", SqlDbType.Bit);
            parameters[8].Value = searchModel.TypeLocation == "1" ? true : false; ;
            reportSearchStatementModel.Parameters = parameters;

            return reportSearchStatementModel;
        }
        #endregion
    }
}
