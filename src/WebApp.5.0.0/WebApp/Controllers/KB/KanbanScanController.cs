namespace com.Sconit.Web.Controllers.KB
{
    #region reference
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Security;
    using com.Sconit.Entity.KB;
    using com.Sconit.Service;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Models.SearchModels.KB;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;
    using NHibernate.Type;
    using com.Sconit.Entity.SCM;
    using com.Sconit.Entity.Exception;
    using System.Text;
    using System.Web;
    using com.Sconit.Utility;
    #endregion

    public class KanbanScanController : WebAppBaseController
    {
        public IKanbanScanMgr kanbanScanMgr { get; set; }

        private static string selectCountStatement = "select count(*) from KanbanScan as s";
        private static string selectStatement = "select s from KanbanScan as s";

        [SconitAuthorize(Permissions = "Url_KanbanScan_View")]
        public ActionResult Index()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_KanbanScan_View")]
        public ActionResult List(GridCommand command, KanbanScanSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_KanbanScan_View")]
        public ActionResult _AjaxList(GridCommand command, KanbanScanSearchModel searchModel)
        {
            if (!string.IsNullOrWhiteSpace(searchModel.CardNo))
            {
                if (searchModel.CardNo.Length > 2 && searchModel.CardNo.Substring(0, 2).ToUpper() == "$K")
                {
                    searchModel.CardNo = searchModel.CardNo.Substring(2, searchModel.CardNo.Length - 2);
                }
            }
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            GridModel<KanbanScan> gridList = GetAjaxPageData<KanbanScan>(searchStatementModel, command);
            if (gridList.Data != null && gridList.Data.Count() > 0)
            {
                foreach (KanbanScan kanbanScan in gridList.Data)
                {
                    var kanbanCards = this.genericMgr.FindAll<KanbanCard>(" select k from KanbanCard as k where k.FlowDetailId=? ", kanbanScan.FlowDetailId);
                    if (kanbanCards != null && kanbanCards.Count > 0)
                    {
                        kanbanScan.KanbanNo = kanbanCards.First().OpRefSequence;
                    }
                }
            }
            return PartialView(gridList);
        }

        [HttpGet]
        [SconitAuthorize(Permissions = "Url_KanbanScan_View")]
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                KanbanScan scan = this.genericMgr.FindById<KanbanScan>(Int32.Parse(id));
                scan.UnitCount = genericMgr.FindById<FlowDetail>(scan.FlowDetailId).UnitCount;
                ViewBag.TempKanbanCard = scan.TempKanbanCard;
                ViewBag.ScanQty = scan.ScanQty.ToString("0.##");
                return View(scan);
            }
        }

        [SconitAuthorize(Permissions = "Url_KanbanScan_View")]
        public ActionResult Edit(KanbanScan scan)
        {

            ViewBag.ScanQty = scan.ScanQty.ToString("0.##");
            ViewBag.TempKanbanCard = scan.TempKanbanCard;
            if (scan.ScanQty > scan.UnitCount)
            {
                SaveErrorMessage("扫描数不能大于看板数。");
                return View(scan);
            }
            if (string.IsNullOrEmpty(scan.TempKanbanCard))
            {
                SaveErrorMessage("临时看板卡号不能为空。");
                return View(scan);
            }
            this.kanbanScanMgr.ModifyScanQty(scan, scan.ScanQty, scan.TempKanbanCard, this.CurrentUser);
            SaveSuccessMessage(Resources.KB.KanbanScan.KanbanScan_Updated);

            return View(scan);
        }

        [SconitAuthorize(Permissions = "Url_KanbanScan_View")]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            else
            {
                try
                {
                    this.kanbanScanMgr.DeleteKanbanScan(this.genericMgr.FindById<KanbanScan>(Int32.Parse(id)), this.CurrentUser);
                    SaveSuccessMessage(Resources.KB.KanbanScan.KanbanScan_Deleted);
                }
                catch (Exception ex)
                {
                    SaveErrorMessage(Resources.KB.KanbanScan.KanbanScan_FailedDelete);
                }
                return RedirectToAction("List");
            }
        }

        [SconitAuthorize(Permissions = "Url_KanbanScan_View")]
        public ActionResult DeleteCardScan(string checkedIds)
        {
            try
            {
                string[] checkedIdArray = checkedIds.Split(',');

                string errorCards = string.Empty;
                foreach (var id in checkedIdArray)
                {
                    KanbanScan kanbanScan = this.genericMgr.FindById<KanbanScan>(Int32.Parse(id));
                    this.kanbanScanMgr.DeleteKanbanScan(kanbanScan, this.CurrentUser);
                }

            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            return RedirectToAction("List");
        }

        #region 导出 导入
        public void ExportKanbanScanXLS(KanbanScanSearchModel searchModel)
        {
            StringBuilder sb = new StringBuilder();
            IList<object> param = new List<object>();
            sb.Append(@" select ks.CardNo,ks.Seq,ks.Supplier,ks.Flow,ks.SupplierName,ks.Item,ks.RefItemCode,ks.ItemDesc,ks.ScanQty,ks.ScanTime,ks.ScanUserNm,ks.OrderTime ,
ks.OrderUserNm,ks.OrderQty,kb.OpRefSeq
from KB_KanbanScan as ks 
left join ( select FlowDetId,OpRefSeq from KB_KanbanCard group by FlowDetId,OpRefSeq ) as kb on ks.FlowDetId=kb.FlowDetId where 1=1 ");

            if (searchModel.IsTempKanbanCard.HasValue && searchModel.IsTempKanbanCard.Value)
            {
                sb.Append(" and TempKanbanCard <> '' and TempKanbanCard is not null ");
            }
            if (!string.IsNullOrWhiteSpace(searchModel.CardNo))
            {
                if (!string.IsNullOrWhiteSpace(searchModel.CardNo))
                {
                    if (searchModel.CardNo.Length > 2 && searchModel.CardNo.Substring(0, 2).ToUpper() == "$K")
                    {
                        searchModel.CardNo = searchModel.CardNo.Substring(2, searchModel.CardNo.Length - 2);
                    }
                }
                sb.Append(" and CardNo = ?");
                param.Add(searchModel.CardNo);
            }
            if (searchModel.Item != null)
            {
                sb.Append(" and Item = ?");
                param.Add(searchModel.Item);
            }
            if (searchModel.Supplier != null)
            {
                sb.Append(" and Supplier = ?");
                param.Add(searchModel.Supplier);
            }

            if (searchModel.IsNotOrdered)
            {
                sb.Append(" and IsOrdered = 1");
            }
            if (searchModel.StartDate != null)
            {
                sb.Append(" and ScanTime >=?");
                param.Add(searchModel.StartDate);
            }
            if (searchModel.EndDate != null)
            {
                sb.Append(" and ScanTime < ?");
                param.Add(searchModel.EndDate);
            }
            IList<KanbanScan> exportList = new List<KanbanScan>();
            IList<object[]> searchResult = this.genericMgr.FindAllWithNativeSql<object[]>(sb.ToString(), param.ToArray());
            if (searchResult != null && searchResult.Count > 0)
            {
                //ks.CardNo,ks.Seq,ks.Supplier,ks.Flow,ks.SupplierName,ks.Item,ks.RefItemCode,ks.ItemDesc,ks.ScanQty,ks.ScanTime,ks.ScanUserNm,ks.OrderTime ,
//ks.OrderUserNm,ks.OrderQty,kb.OpRefSeq
                exportList = (from tak in searchResult
                              select new KanbanScan
                              {
                                  CardNo = (string)tak[0],
                                  Sequence = (string)tak[1],
                                  Supplier = (string)tak[2],
                                  Flow = (string)tak[3],
                                  SupplierName = (string)tak[4],
                                  Item = (string)tak[5],
                                  ReferenceItemCode = (string)tak[6],
                                  ItemDescription = (string)tak[7],
                                  ScanQty = (decimal)tak[8],
                                  ScanTime = (DateTime?)tak[9],
                                  ScanUserName = (string)tak[10],
                                  OrderTime = (DateTime?)tak[11],
                                  OrderUserName = (string)tak[12],
                                  OrderQty = (decimal)tak[13],
                                  KanbanNo = (string)tak[14],
                              }).ToList();
            }
            ExportToXLS<KanbanScan>("ExportKanbanScanXLS", "xls", exportList);
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        [SconitAuthorize(Permissions = "Url_KanbanLost_View")]
        public ActionResult ImportKanbanScan(IEnumerable<HttpPostedFileBase> attachments)
        {
            try
            {

                foreach (var file in attachments)
                {
                    kanbanScanMgr.ImportkanbanScanXls(file.InputStream);
                }
                SaveSuccessMessage("导入成功。");
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage("导入失败。 - " + ex.Message);
            }

            return Content(string.Empty);
        }

        #endregion

        private SearchStatementModel PrepareSearchStatement(GridCommand command, KanbanScanSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();
            //if (searchModel.IsTempKanbanCard.HasValue && searchModel.IsTempKanbanCard.Value)
            //{
            //    whereStatement += " where TempKanbanCard <> '' and TempKanbanCard is not null ";
            //}

            SecurityHelper.AddRegionPermissionStatement(ref whereStatement, "s", "Region");
            HqlStatementHelper.AddEqStatement("CardNo", searchModel.CardNo, "s", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "s", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Supplier", searchModel.Supplier, "s", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("LogisticCenterCode", searchModel.LcCode, "s", ref whereStatement, param);
            //HqlStatementHelper.AddEqStatement("TempKanbanCard", searchModel.TempKanbanCard, "s", ref whereStatement, param);
            //HqlStatementHelper.AddGeStatement("ScanTime", searchModel.StartDate, "s", ref whereStatement, param);
            ////改到当日的最后一秒
            //HqlStatementHelper.AddLeStatement("ScanTime", searchModel.EndDate.Add(new TimeSpan(23, 59, 59)), "s", ref whereStatement, param);

            //if (searchModel.StartDate != null & searchModel.EndDate != null)
            //{
            //    HqlStatementHelper.AddBetweenStatement("ScanTime", searchModel.StartDate, searchModel.EndDate, "s", ref whereStatement, param);
            //}
            //else 
            if (searchModel.StartDate != null)
            {
                HqlStatementHelper.AddGeStatement("ScanTime", searchModel.StartDate, "s", ref whereStatement, param);
            }
             if ( searchModel.EndDate != null)
            {
                HqlStatementHelper.AddLeStatement("ScanTime", searchModel.EndDate, "s", ref whereStatement, param);
            }

            if (searchModel.IsNotOrdered)
            {
                HqlStatementHelper.AddEqStatement("IsOrdered", false, "s", ref whereStatement, param);
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
    }
}