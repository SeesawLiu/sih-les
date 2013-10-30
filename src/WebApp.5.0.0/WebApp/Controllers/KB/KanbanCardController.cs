using com.Sconit.Web.Util;
namespace com.Sconit.Web.Controllers.KB
{
    #region reference
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.OleDb;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using com.Sconit.Entity;
    using com.Sconit.Entity.ACC;
    using com.Sconit.Entity.CUST;
    using com.Sconit.Entity.Exception;
    using com.Sconit.Entity.KB;
    using com.Sconit.Entity.MD;
    using com.Sconit.Entity.SCM;
    using com.Sconit.Entity.VIEW;
    using com.Sconit.Service;
    using com.Sconit.Utility;
    using com.Sconit.Utility.Report;
    using com.Sconit.Web.Models;
    using com.Sconit.Web.Models.KB;
    using com.Sconit.Web.Models.SearchModels.KB;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;
    using Telerik.Web.Mvc.UI;
    using com.Sconit.Entity.PRD;
    using com.Sconit.Service.Impl;
    using System.Web.Routing;
    using System.Text;
    using com.Sconit.Entity.ORD;
    using com.Sconit.PrintModel.ORD;
    using AutoMapper;
    using com.Sconit.Entity.SYS;
    using com.Sconit.Web.Models.SearchModels.MD;
    using NHibernate.Type;
    using NHibernate;
    using com.Sconit.PrintModel.INV;
    #endregion

    public class KanbanCardController : WebAppBaseController
    {
        public IKanbanCardMgr kanbanCardMgr { get; set; }
        public IKanbanScanMgr kanbanScanMgr { get; set; }
        public IKanbanScanOrderMgr kanbanScanOrderMgr { get; set; }
        public IReportGen reportGen { get; set; }
        public IWorkingCalendarMgr workingCalendarMgr { get; set; }
        public IFlowMgr flowMgr { get; set; }

        /// <summary>
        /// hql to get count of the KanbanCard
        /// </summary>
        private static string selectCountStatement = "select count(*) from KanbanCard  k";
        private static string selectCountStatementItemDailyConsume = "select count(*) from ItemDailyConsume u";
        private static string selectCountStatementKanbanOrder = "select count(*) from KanbanScan o";

        /// <summary>
        /// hql to get all of the KanbanCard
        /// </summary>
        private static string selectStatement = "select k from KanbanCard as k";
        private static string selectStatementItemDailyConsume = "select u from ItemDailyConsume as u";
        private static string selectStatementKanbanOrder = "select o from KanbanScan as o";

        [SconitAuthorize(Permissions = "Url_KanbanCard_New")]
        public ActionResult New()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_Calc")]
        public ActionResult Calc()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_Calc2")]
        public ActionResult Calc2()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_ItemDailyConsume")]
        public ActionResult ItemDailyConsume()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_KanbanCard_ItemDailyConsume")]
        public ActionResult ListItemDailyConsume(GridCommand command, ItemDailyConsumeSearchModel searchModel)
        {
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            //if (string.IsNullOrEmpty(searchModel.Region))
            //{
            //    SaveWarningMessage("区域不能为空");
            //    return View();
            //}

            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);

            ViewBag.SearchModel = searchModel;
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_KanbanCard_ItemDailyConsume")]
        public ActionResult _ItemDailyConsumeAjaxList(GridCommand command, ItemDailyConsumeSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.ItemDailyConsumePrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<ItemDailyConsume>(searchStatementModel, command));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_KanbanCard_ItemDailyConsume")]
        public ActionResult _ItemDailyConsumeSaveBatch([Bind(Prefix = "updated")]IEnumerable<ItemDailyConsume> updates,
            ItemDailyConsumeSearchModel searchModel)
        {
            if (updates != null)
            {
                foreach (ItemDailyConsume idc in updates)
                {
                    this.genericMgr.Update(idc);
                }
            }

            GridCommand command = new GridCommand();
            SearchStatementModel searchStatementModel = this.ItemDailyConsumePrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<ItemDailyConsume>(searchStatementModel, command));
        }

        private SearchStatementModel ItemDailyConsumePrepareSearchStatement(GridCommand command, ItemDailyConsumeSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();
            if (!string.IsNullOrEmpty(searchModel.Region))
            {
                if (string.IsNullOrEmpty(searchModel.Location))
                {
                    //all loc of the region
                    IList<Location> locs = genericMgr.FindAll<Location>("from Location where Region = ? and IsActive = ? ", new object[] { searchModel.Region, true });
                    IList<string> locCodes = new List<string>();
                    if (locs != null)
                    {
                        foreach (Location loc in locs)
                        {
                            locCodes.Add(loc.Code);
                        }
                    }
                    HqlStatementHelper.AddInStatement("Location", locCodes.ToArray(), "u", ref whereStatement, param);
                }
                else
                {
                    HqlStatementHelper.AddEqStatement("Location", searchModel.Location, "u", ref whereStatement, param);
                }
            }
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "u", ref whereStatement, param);
            if (searchModel.StartDate != null && searchModel.StartDate > DateTime.MinValue)
            {
                HqlStatementHelper.AddGeStatement("ConsumeDate", searchModel.StartDate, "u", ref whereStatement, param);
            }
            if (searchModel.EndDate != null && searchModel.EndDate > DateTime.MinValue)
            {
                HqlStatementHelper.AddLeStatement("ConsumeDate", searchModel.EndDate, "u", ref whereStatement, param);
            }

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatementItemDailyConsume;
            searchStatementModel.SelectStatement = selectStatementItemDailyConsume;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_ItemDailyConsume2")]
        public ActionResult ItemDailyConsume2()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_ItemDailyConsumePrd")]
        public ActionResult ItemDailyConsumePrd()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_KanbanCard_ItemDailyConsume2")]
        public JsonResult ModifyMaxItemDailyConsumeQty(string itemStr, string locationStr, string multiStr, string newMaxConsumeQtyStr, string startDate, string endDate)
        {
            try
            {
                if (!string.IsNullOrEmpty(itemStr))
                {
                    string[] itemArray = itemStr.Split(',');
                    string[] locationArray = locationStr.Split(',');
                    //string[] multiArray = multiStr.Split(',');
                    string[] newMaxConsumeQtyArray = newMaxConsumeQtyStr.Split(',');
                    DateTime sd = DateTime.Parse(startDate + " 00:00:00");
                    DateTime ed = DateTime.Parse(endDate + " 00:00:00");
                    for (int i = 0; i < itemArray.Length; i++)
                    {
                        //  decimal oldMaxQty = genericMgr.FindAll<ItemDailyConsume>(" from ItemDailyConsume c where c.Item = ?  and Location = ? and ConsumeDate >= ? and ConsumeDate <= ? ",new object[] {itemArray[i], locationArray[i], sd, ed }).Max(p=>p.Qty);
                        //if (oldMaxQty > Decimal.Parse(newMaxConsumeQtyArray[i]))
                        //{
                        //    throw new BusinessException("零件" + itemArray[i] + "新最大日用量小于原始日用量。");
                        //}
                        this.genericMgr.Update("Update ItemDailyConsume set Qty = ? where Item = ? and Location = ? and ConsumeDate >= ? and ConsumeDate <= ? ",
                        new object[] { Decimal.Parse(newMaxConsumeQtyArray[i]), itemArray[i], locationArray[i], sd, ed });
                    }
                    SaveSuccessMessage("更新最大日用量成功。");
                    return Json(new { });
                }
                else
                {
                    throw new BusinessException("新最大日用量不能为空。");
                }
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                return Json(null);
            }
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_KanbanCard_ItemDailyConsume2")]
        public ActionResult ListItemDailyConsume2(GridCommand command, ItemDailyConsumeSearchModel searchModel)
        {
            this.ProcessSearchModel(command, searchModel);
            ViewBag.ReadOnly = false;
            if (string.IsNullOrEmpty(searchModel.Region) && string.IsNullOrEmpty(searchModel.Item))
            {
                SaveWarningMessage("区域,跟物料必须选一个。");
                ViewData["Columns"] = new List<GridColumnSettings>().ToArray();
                ViewBag.ReadOnly = true;
                return View();
            }

            if (searchModel.StartDate == null)
            {
                SaveWarningMessage("开始日期不能为空");
                ViewData["Columns"] = new List<GridColumnSettings>().ToArray();
                ViewBag.ReadOnly = true;
                return View();
            }

            if (searchModel.EndDate == null)
            {
                SaveWarningMessage("结束日期不能为空");
                ViewData["Columns"] = new List<GridColumnSettings>().ToArray();
                ViewBag.ReadOnly = true;
                return View();
            }
            //第二次刷新值没了，先放这里
            TempData["ItemDailyConsumeSearchModel"] = searchModel;
            ViewBag.SearchModel = searchModel;
            int days = (int)searchModel.EndDate.Value.Subtract(searchModel.StartDate.Value).TotalDays + 1;

            ItemDailyConsumeView itemDailyConsumeView = this.PrepareItemDailyConsumeView(searchModel, days);

            IList<GridColumnSettings> columns = new List<GridColumnSettings>();
            columns.Add(new GridColumnSettings
            {
                Member = itemDailyConsumeView.ItemDailyConsumeHead.Item,
                Title = Resources.KB.KanbanCard.KanbanCard_Item,
                Sortable = false
            });
            columns.Add(new GridColumnSettings
            {
                Member = itemDailyConsumeView.ItemDailyConsumeHead.ItemDesc,
                Title = Resources.CUST.ItemDailyConsume.ItemDailyConsume_ItemDesc,
                Sortable = false
            });
            columns.Add(new GridColumnSettings
            {
                Member = itemDailyConsumeView.ItemDailyConsumeHead.Location,
                Title = Resources.KB.KanbanCard.KanbanCard_Location,
                Sortable = false
            });
            columns.Add(new GridColumnSettings
            {
                Member = itemDailyConsumeView.ItemDailyConsumeHead.MultiSupplyGroup,
                Title = Resources.KB.KanbanCard.KanbanCard_MultiSupplyGroup,
                Sortable = false
            });
            columns.Add(new GridColumnSettings
            {
                Member = itemDailyConsumeView.ItemDailyConsumeHead.MaxConsumeQty,
                Title = Resources.KB.KanbanCard.KanbanCard_MaxConsumeQty,
                Sortable = false
            });
            columns.Add(new GridColumnSettings
            {
                Member = itemDailyConsumeView.ItemDailyConsumeHead.NewMaxConsumeQty,
                Title = Resources.KB.KanbanCard.KanbanCard_NewMaxConsumeQty,
                Sortable = false
            });

            if (itemDailyConsumeView.ItemDailyConsumeHead.ColumnCellList != null
                && itemDailyConsumeView.ItemDailyConsumeHead.ColumnCellList.Count > 0)
            {
                for (int i = 0; i < days; i++)
                {
                    columns.Add(new GridColumnSettings
                    {
                        Member = "RowCellList[" + i + "].Qty",
                        MemberType = typeof(Decimal),
                        Title = (itemDailyConsumeView.ItemDailyConsumeHead.ColumnCellList[i].ConsumeDate.Value.Date.ToShortDateString()),
                        Sortable = false
                    });
                }
            }

            ViewData["Columns"] = columns.ToArray();
            TempData["ExportListItemDailyConsume2Columns"] = columns;
            TempData["ExportListItemDailyConsume2BodyList"] = itemDailyConsumeView.ItemDailyConsumeBodyList;
            return View(itemDailyConsumeView.ItemDailyConsumeBodyList);
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_KanbanCard_ItemDailyConsumePrd")]
        public ActionResult ListItemDailyConsumePrd(GridCommand command, ItemDailyConsumeSearchModel searchModel)
        {
            this.ProcessSearchModel(command, searchModel);
            ViewBag.ReadOnly = false;
            if (string.IsNullOrEmpty(searchModel.Region) && string.IsNullOrEmpty(searchModel.Item))
            {
                SaveWarningMessage("区域跟物料不能同时为空");
                ViewData["Columns"] = new List<GridColumnSettings>().ToArray();
                ViewBag.ReadOnly = true;
                return View();
            }

            if (searchModel.StartDate == null)
            {
                SaveWarningMessage("开始日期不能为空");
                ViewData["Columns"] = new List<GridColumnSettings>().ToArray();
                ViewBag.ReadOnly = true;
                return View();
            }

            if (searchModel.EndDate == null)
            {
                SaveWarningMessage("结束日期不能为空");
                ViewData["Columns"] = new List<GridColumnSettings>().ToArray();
                ViewBag.ReadOnly = true;
                return View();
            }
            //第二次刷新值没了，先放这里
            TempData["ItemDailyConsumeSearchModel"] = searchModel;
            ViewBag.SearchModel = searchModel;
            int days = (int)searchModel.EndDate.Value.Subtract(searchModel.StartDate.Value).TotalDays + 1;

            ItemDailyConsumeView itemDailyConsumeView = this.PrepareItemDailyConsumePrdView(searchModel, days);

            IList<GridColumnSettings> columns = new List<GridColumnSettings>();
            columns.Add(new GridColumnSettings
            {
                Member = itemDailyConsumeView.ItemDailyConsumeHead.Item,
                Title = Resources.KB.KanbanCard.KanbanCard_Item,
                Sortable = false
            });
            columns.Add(new GridColumnSettings
            {
                Member = itemDailyConsumeView.ItemDailyConsumeHead.ItemDesc,
                Title = Resources.CUST.ItemDailyConsume.ItemDailyConsume_ItemDesc,
                Sortable = false
            });
            columns.Add(new GridColumnSettings
            {
                Member = itemDailyConsumeView.ItemDailyConsumeHead.Location,
                Title = Resources.KB.KanbanCard.KanbanCard_Location,
                Sortable = false
            });
            columns.Add(new GridColumnSettings
            {
                Member = itemDailyConsumeView.ItemDailyConsumeHead.MultiSupplyGroup,
                Title = Resources.KB.KanbanCard.KanbanCard_MultiSupplyGroup,
                Sortable = false
            });
            columns.Add(new GridColumnSettings
            {
                Member = itemDailyConsumeView.ItemDailyConsumeHead.MaxConsumeQty,
                Title = Resources.KB.KanbanCard.KanbanCard_MaxConsumeQty,
                Sortable = false
            });
            columns.Add(new GridColumnSettings
            {
                Member = itemDailyConsumeView.ItemDailyConsumeHead.NewMaxConsumeQty,
                Title = Resources.KB.KanbanCard.KanbanCard_NewMaxConsumeQty,
                Sortable = false
            });

            if (itemDailyConsumeView.ItemDailyConsumeHead.ColumnCellList != null
                && itemDailyConsumeView.ItemDailyConsumeHead.ColumnCellList.Count > 0)
            {
                for (int i = 0; i < days; i++)
                {
                    columns.Add(new GridColumnSettings
                    {
                        Member = "RowCellList[" + i + "].Qty",
                        MemberType = typeof(Decimal),
                        Title = (itemDailyConsumeView.ItemDailyConsumeHead.ColumnCellList[i].ConsumeDate.Value.Date.ToShortDateString()),
                        Sortable = false
                    });
                }
            }

            ViewData["Columns"] = columns.ToArray();
            TempData["ExportListItemDailyConsumePrdColumns"] = columns;
            TempData["ExportListItemDailyConsumePrdBodyList"] = itemDailyConsumeView.ItemDailyConsumeBodyList;
            return View(itemDailyConsumeView.ItemDailyConsumeBodyList);
        }

        private ItemDailyConsumeView PrepareItemDailyConsumeView(ItemDailyConsumeSearchModel searchModel, int days)
        {
            ItemDailyConsumeView view = new ItemDailyConsumeView();
            ItemDailyConsumeHead head = new ItemDailyConsumeHead();
            view.ItemDailyConsumeHead = head;

            List<object> paras = new List<object>();
            paras.Add((int)com.Sconit.CodeMaster.FlowStrategy.KB);
            paras.Add((int)com.Sconit.CodeMaster.KBCalculation.Normal);
            paras.Add(searchModel.StartDate);
            paras.Add(searchModel.EndDate);

            string where = string.Empty;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(searchModel.Region))
            {
                sb.Append(" and fm.PartyTo in (");
                string[] regions = searchModel.Region.Split(',');
                for (int ir = 0; ir < regions.Length; ir++)
                {
                    sb.Append("?,");
                    paras.Add(regions[ir]);
                }
                sb = sb.Remove(sb.Length - 1, 1);
                where += sb.ToString() + ")";
            }
            if (!string.IsNullOrEmpty(searchModel.Location))
            {
                where += " and fm.LocTo = ?";
                paras.Add(searchModel.Location);
            }
            if (!string.IsNullOrEmpty(searchModel.Item))
            {
                where += " and ic.Item = ?";
                paras.Add(searchModel.Item);
            }

            IList<ItemDailyConsume> idcs = genericMgr.FindEntityWithNativeSql<ItemDailyConsume>(@"select ic.* from cust_itemdailyconsume ic 
                    inner join scm_flowdet fd on ic.item = fd.item 
                    inner join scm_flowmstr fm on fd.flow = fm.code
                    inner join scm_flowstrategy fg on fm.code = fg.flow
                    where fm.isactive = 1 and ic.Location = fm.LocTo and fm.FlowStrategy = ? and fg.kbcalc = ? and ic.ConsumeDate >= ? and ic.ConsumeDate <= ? " + where, paras.ToArray());


            IList<ColumnCell> columnCellList = new List<ColumnCell>();

            for (int i = 0; i < days; i++)
            {
                ColumnCell c = new ColumnCell();
                c.ConsumeDate = searchModel.StartDate.Value.AddDays(i);
                columnCellList.Add(c);
            }
            head.ColumnCellList = columnCellList;

            var s = from idc in idcs
                    group idc by new
                    {
                        Item = idc.Item,
                        ItemDesc = idc.ItemDesc,
                        Location = idc.Location,
                        MultiSupplyGroup = idc.MultiSupplyGroup
                    } into g
                    select new ItemDailyConsumeBody
                    {
                        ItemDesc = g.Key.ItemDesc,
                        Item = g.Key.Item,
                        Location = g.Key.Location,
                        MultiSupplyGroup = g.Key.MultiSupplyGroup
                    };

            IList<ItemDailyConsumeBody> bodylist = s.ToList();
            if (bodylist != null && bodylist.Count > 0)
            {
                if (head.ColumnCellList != null && head.ColumnCellList.Count > 0)
                {
                    foreach (ItemDailyConsumeBody idcBody in bodylist)
                    {
                        Decimal maxConsumeQty = 0;
                        List<RowCell> rcs = new List<RowCell>();
                        for (int i = 0; i < days; i++)
                        {
                            RowCell rc = new RowCell();
                            rc.ConsumeDate = searchModel.StartDate.Value.AddDays(i);

                            var r = from idc in idcs
                                    where (idc.Item == idcBody.Item
                                        && idc.Location == idcBody.Location
                                        && idc.MultiSupplyGroup == idcBody.MultiSupplyGroup
                                        && idc.ConsumeDate == rc.ConsumeDate)
                                    select idc;
                            if (r != null && r.ToList().Count > 0)
                            {
                                rc.Qty = r.ToList()[0].OriginalQty;
                                rc.MaxQty = r.ToList()[0].Qty;
                            }
                            else
                            {
                                rc.Qty = 0;
                            }
                            rcs.Add(rc);

                            if (rc.MaxQty > maxConsumeQty)
                            {
                                maxConsumeQty = rc.MaxQty;
                            }
                        }

                        idcBody.MaxConsumeQty = maxConsumeQty;
                        idcBody.RowCellList = rcs;
                    }
                }
            }
            view.ItemDailyConsumeBodyList = bodylist;

            return view;
        }

        private ItemDailyConsumeView PrepareItemDailyConsumePrdView(ItemDailyConsumeSearchModel searchModel, int days)
        {
            ItemDailyConsumeView view = new ItemDailyConsumeView();
            ItemDailyConsumeHead head = new ItemDailyConsumeHead();
            view.ItemDailyConsumeHead = head;

            List<object> paras = new List<object>();
            paras.Add((int)com.Sconit.CodeMaster.FlowStrategy.KB);
            paras.Add((int)com.Sconit.CodeMaster.KBCalculation.CatItem);
            paras.Add(searchModel.StartDate);
            paras.Add(searchModel.EndDate);

            string where = string.Empty;
            //if (!string.IsNullOrEmpty(searchModel.Region))
            //{
            //    where += " and fm.PartyTo = ?";
            //    paras.Add(searchModel.Region);
            //}
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(searchModel.Region))
            {
                sb.Append(" and fm.PartyTo in (");
                string[] regions = searchModel.Region.Split(',');
                for (int ir = 0; ir < regions.Length; ir++)
                {
                    sb.Append("?,");
                    paras.Add(regions[ir]);
                }
                sb = sb.Remove(sb.Length - 1, 1);
                where += sb.ToString() + ")";
            }
            if (!string.IsNullOrEmpty(searchModel.Location))
            {
                where += " and fm.LocTo = ?";
                paras.Add(searchModel.Location);
            }
            if (!string.IsNullOrEmpty(searchModel.Item))
            {
                where += " and ic.Item = ?";
                paras.Add(searchModel.Item);
            }

            IList<ItemDailyConsume> idcs = this.genericMgr.FindEntityWithNativeSql<ItemDailyConsume>(@"select ic.* from cust_itemdailyconsume ic 
                    inner join scm_flowdet fd on ic.item = fd.item 
                    inner join scm_flowmstr fm on fd.flow = fm.code 
                    inner join scm_flowstrategy fg on fm.code = fg.flow
                    where fm.isactive=1 and fm.FlowStrategy = ? and fg.kbcalc = ? and ic.ConsumeDate >= ? and ic.ConsumeDate <= ? " + where, paras.ToArray());

            IList<ColumnCell> columnCellList = new List<ColumnCell>();

            for (int i = 0; i < days; i++)
            {
                ColumnCell c = new ColumnCell();
                c.ConsumeDate = searchModel.StartDate.Value.AddDays(i);
                columnCellList.Add(c);
            }
            head.ColumnCellList = columnCellList;

            var s = from idc in idcs
                    group idc by new
                    {
                        Item = idc.Item,
                        ItemDesc = idc.ItemDesc,
                        Location = idc.Location,
                        MultiSupplyGroup = idc.MultiSupplyGroup
                    } into g
                    select new ItemDailyConsumeBody
                    {
                        Item = g.Key.Item,
                        ItemDesc = g.Key.ItemDesc,
                        Location = g.Key.Location,
                        MultiSupplyGroup = g.Key.MultiSupplyGroup
                    };

            IList<ItemDailyConsumeBody> bodylist = s.ToList();
            if (bodylist != null && bodylist.Count > 0)
            {
                if (head.ColumnCellList != null && head.ColumnCellList.Count > 0)
                {
                    foreach (ItemDailyConsumeBody idcBody in bodylist)
                    {
                        Decimal maxConsumeQty = 0;
                        List<RowCell> rcs = new List<RowCell>();
                        for (int i = 0; i < days; i++)
                        {
                            RowCell rc = new RowCell();
                            rc.ConsumeDate = searchModel.StartDate.Value.AddDays(i);

                            var r = from idc in idcs
                                    where (idc.Item == idcBody.Item
                                        && idc.Location == idcBody.Location
                                        && idc.MultiSupplyGroup == idcBody.MultiSupplyGroup
                                        && idc.ConsumeDate == rc.ConsumeDate)
                                    select idc;
                            if (r != null && r.ToList().Count > 0)
                            {
                                rc.Qty = r.ToList()[0].OriginalQty;
                                rc.MaxQty = r.ToList()[0].Qty;
                            }
                            else
                            {
                                rc.Qty = 0;
                            }
                            rcs.Add(rc);

                            if (rc.MaxQty > maxConsumeQty)
                            {
                                maxConsumeQty = rc.MaxQty;
                            }
                        }

                        idcBody.MaxConsumeQty = maxConsumeQty;
                        idcBody.RowCellList = rcs;
                    }
                }
            }
            view.ItemDailyConsumeBodyList = bodylist;

            return view;
        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_Calc")]
        public JsonResult TryCalc(string multiregion, string region, string location, DateTime startDate, DateTime endDate, int kbCalc)
        {
            try
            {
                string batchNo = KanbanUtility.GetBatchNo(multiregion, region, location, startDate, endDate, kbCalc, this.CurrentUser);
                //kanbanCardMgr.TryCalculate(multiregion, region, location, startDate, endDate, kbCalc, this.CurrentUser);
                kanbanCardMgr.TryCalculate(multiregion, startDate, endDate, kbCalc, this.CurrentUser);
                SaveSuccessMessage("计算成功，请导出！批处理号：" + batchNo);
                return Json(new { });
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                return Json(null);
            }
        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_Calc")]
        public void ExportCalc(string multiregionexport, string region, string location, DateTime startDate, DateTime endDate, int kbCalc)
        {

            string batchno = KanbanUtility.GetBatchNo(multiregionexport, region, location, startDate, endDate, kbCalc, this.CurrentUser);

            IList<KanBanCalcResult> kanBanCalcResults = this.genericMgr.FindAll<KanBanCalcResult>("from KanBanCalcResult kb where kb.BatchNo=?", batchno);
            // return ExportToCSV<KanBanCalcResult>("KanBanCalcResult", "csv", kanBanCalcResults);
            ExportToXLS<KanBanCalcResult>("KanBanCalcResult", "xls", kanBanCalcResults);

        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_Calc")]
        public void ExportCalc2(string multiregionexport, string region, string location, DateTime startDate, DateTime endDate, int kbCalc)
        {

            string batchno = KanbanUtility.GetBatchNo(multiregionexport, region, location, startDate, endDate, kbCalc, this.CurrentUser);

            IList<ProductKanBanCalcResult> kanBanCalcResults = this.genericMgr.FindAll<ProductKanBanCalcResult>("from ProductKanBanCalcResult kb where kb.BatchNo=? order by GroupNo asc, Item asc", batchno);
            ExportToXLS<ProductKanBanCalcResult>("ProductKanBanCalcResult", "xls", kanBanCalcResults);
            // return ExportToCSV<ProductKanBanCalcResult>("ProductKanBanCalcResult", "csv", kanBanCalcResults);
        }

        public ActionResult ImportKanbanCalc(IEnumerable<HttpPostedFileBase> attachments, string multiregion, string region, string location, DateTime startDate, DateTime endDate, int kbCalc)
        {
            try
            {
                string batchno = KanbanUtility.GetBatchNo(multiregion, region, location, startDate, endDate, kbCalc, this.CurrentUser);

                foreach (var file in attachments)
                {
                    kanbanCardMgr.ImportKanbanCalc(file.InputStream, batchno, this.CurrentUser);
                }
                SaveSuccessMessage("导入成功。");
            }
            catch (BusinessException ex)
            {
                string messagesStr = "";
                IList<Message> messageList = ex.GetMessages();
                foreach (Message message in messageList)
                {
                    messagesStr += message.GetMessageString();
                }

                SaveErrorMessage(messagesStr);
            }
            catch (Exception ex)
            {
                SaveErrorMessage("导入失败。 - " + ex.Message);
            }
            return Content(string.Empty);
        }

        public ActionResult ImportKanbanCalc2(IEnumerable<HttpPostedFileBase> attachments, string multiregion, string region, string location, DateTime startDate, DateTime endDate, int kbCalc)
        {
            try
            {
                string batchno = KanbanUtility.GetBatchNo(multiregion, region, location, startDate, endDate, kbCalc, this.CurrentUser);

                foreach (var file in attachments)
                {
                    kanbanCardMgr.ImportKanbanCalc2(file.InputStream, batchno, this.CurrentUser);
                }
                SaveSuccessMessage("导入成功。");
            }
            catch (BusinessException ex)
            {
                string messagesStr = "";
                IList<Message> messageList = ex.GetMessages();
                foreach (Message message in messageList)
                {
                    messagesStr += message.GetMessageString();
                }

                SaveErrorMessage(messagesStr);
            }
            catch (Exception ex)
            {
                SaveErrorMessage("导入失败。 - " + ex.Message);
            }
            return Content(string.Empty);
        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_Calc")]
        public JsonResult ExeCalc(string multiregion, string region, string location, DateTime startDate, DateTime endDate, int kbCalc)
        {
            try
            {
                // this.kanbanCardMgr.RunBatch(multiregion, region, location, startDate, endDate, kbCalc, this.CurrentUser);
                this.kanbanCardMgr.RunBatch(multiregion, location, startDate, endDate, kbCalc);
                return new JsonResult { Data = "success" };
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                return Json(null);
            }
        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_Scan")]
        public ActionResult Scan()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_Kanban_View")]
        public ActionResult Index()
        {
            return View();
        }

        //[SconitAuthorize(Permissions = "Url_KanbanCard_Scan")]
        public JsonResult Doscan(string cardno)
        {
            if (!string.IsNullOrWhiteSpace(cardno))
            {
                if (cardno.Length > 2 && cardno.Substring(0, 2).ToUpper() == "$K")
                {
                    cardno = cardno.Substring(2, cardno.Length - 2);
                }
            }
            return new JsonResult { Data = this.kanbanScanMgr.Scan(cardno, this.CurrentUser), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_Order")]
        public JsonResult OrderScan(string idStr)
        {
            try
            {
                if (!string.IsNullOrEmpty(idStr))
                {
                    //OrderHolder.CleanOrders();
                    //this.kanbanScanOrderMgr.OrderCard(searchModel.ChosenScans, searchModel.OrderTime.Value);
                    //IList<string> orderNoList = OrderHolder.GetOrders();
                    //string orderNos = string.Empty;
                    //if (orderNoList != null && orderNoList.Count > 0)
                    //{
                    //    foreach (var orderNo in orderNoList)
                    //    {
                    //        orderNos += orderNo + ",";
                    //    }
                    //}
                    //SaveSuccessMessage("收料单创建成功，单号{0}", orderNos.Substring(0, orderNos.Length - 1));
                    string[] idArray = idStr.Split(new string []{","},StringSplitOptions.RemoveEmptyEntries);
                    string hqlKanbanScan = string.Empty; 
                    List<object> para = new List<object>();
                    foreach (var id in idArray)
                    {
                        if (string.IsNullOrEmpty(hqlKanbanScan))
                        {
                            hqlKanbanScan = "from KanbanScan ks where ks.Id in(?";
                        }
                        else
                        {
                            hqlKanbanScan += ",?";
                        }
                        para.Add(id);
                    }
                    hqlKanbanScan += ")";
                    IList<KanbanScan> kbScanList = this.genericMgr.FindAll<KanbanScan>(hqlKanbanScan, para.ToArray());

                    if (kbScanList != null && kbScanList.Count > 0)
                    {
                        List<string> flowCodeList = kbScanList.Select(a => a.Flow).Distinct().ToList();
                        string hqlFlowMaster = string.Empty;
                        string hqlFlowStra = string.Empty;

                        List<object> paraFlow = new List<object>();
                        foreach (var flowCode in flowCodeList)
                        {
                            if (string.IsNullOrEmpty(hqlFlowMaster))
                            {
                                hqlFlowMaster = "from FlowMaster fm where fm.Code in(?";
                                hqlFlowStra = "from FlowStrategy fs where fs.Flow in(?";
                            }
                            else
                            {
                                hqlFlowMaster += ",?";
                                hqlFlowStra += ",?";
                            }
                            paraFlow.Add(flowCode);
                        }
                        hqlFlowMaster += ")";
                        hqlFlowStra += ")";

                        IList<FlowMaster> flowList = this.genericMgr.FindAll<FlowMaster>(hqlFlowMaster, paraFlow.ToArray());
                        IList<FlowStrategy> flowStrategyList = this.genericMgr.FindAll<FlowStrategy>(hqlFlowStra, paraFlow.ToArray());

                        List<Entity.KB.KanbanScan> kbScanOrderNowList = new List<Entity.KB.KanbanScan>();
                        foreach (var scan in kbScanList)
                        {
                            var flowStrategy = flowStrategyList.FirstOrDefault(f => f.Flow == scan.Flow);
                            if (!string.IsNullOrEmpty(scan.CardNo))
                            {
                                if (flowStrategy.IsOrderNow)
                                {
                                    kbScanOrderNowList.Add(scan);
                                }
                            }
                        }
                        if (kbScanOrderNowList.Count > 0)
                        {
                            var orderNos = kanbanScanOrderMgr.OrderCard(kbScanOrderNowList, flowList, DateTime.Now);
                            SaveSuccessMessage("收料单创建成功，单号{0}", orderNos);
                        }
                    }
                    else
                    {
                        SaveErrorMessage("请扫描看板卡。");
                    }
                }
                return Json(new { });
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex.Message);
            }
            return Json(null);
        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_Order")]
        public JsonResult OrderNow(KanbanOrderSearchModel searchModel)
        {
            ViewBag.SearchModel = searchModel;
            try
            {
                if (searchModel.OrderTime == null)
                {
                    SaveWarningMessage("窗口时间不能为空");
                }
                else
                {
                    OrderHolder.CleanOrders();
                    this.kanbanScanOrderMgr.OrderCard(searchModel.ChosenScans, searchModel.OrderTime.Value);
                    IList<string> orderNoList = OrderHolder.GetOrders();
                    string orderNos = string.Empty;
                    if (orderNoList != null && orderNoList.Count > 0)
                    {
                        foreach (var orderNo in orderNoList)
                        {
                            orderNos += orderNo + ",";
                        }
                    }
                    SaveSuccessMessage("收料单创建成功，单号{0}", orderNos.Substring(0, orderNos.Length - 1));


                    return Json(null);
                }
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex.Message);
            }
            return Json(null);
        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_Order")]
        public JsonResult MultiOrderNow(KanbanOrderSearchModel searchModel, string flowStr, string windowTimeStr)
        {
            ViewBag.SearchModel = searchModel;
            try
            {
                if (string.IsNullOrEmpty(flowStr))
                {
                    SaveErrorMessage("明细不能为空");
                }
                else
                {
                    OrderHolder.CleanOrders();
                    string[] flowArray = flowStr.Split(',');
                    string[] windowStrTimeArray = windowTimeStr.Split(',');
                   
                    if (searchModel.OrderTime != null)
                    {

                        #region 调整时间的
                        IList<SupplierRegion> supplierRegionList = new List<SupplierRegion>();
                        string sql = "Select distinct Region, RegionName, Supplier, SupplierName,Flow from KB_KanbanScan s where s.IsOrdered = 0 ";
                        //取用户有权限的所有区域
                        User user = SecurityContextHolder.Get();
                        if (user.Code.Trim().ToLower() != "su")
                        {
                            sql += " and exists (select 1 from VIEW_UserPermission as up where up.UserId =" + user.Id + " and  up.CategoryType = " + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "  and up.PermissionCode = s.Region)";
                        }
                        if (!string.IsNullOrEmpty(searchModel.Supplier))
                        {
                            sql += " and s.Supplier = '" + searchModel.Supplier + "'";
                        }

                        IList<object[]> resultList = genericMgr.FindAllWithNativeSql<object[]>(sql);
                        #endregion

                        var match = resultList.Where(p => (string)p[4] == (string)flowArray[0]).First();
                        IList<object[]> matchList = resultList.Where(p => (string)p[0] == (string)match[0]).ToList();
                        List<string> totalFlowList = new List<string>();
                        List<DateTime> totalWindowTimeList = new List<DateTime>();
                        for (int i = 0; i < matchList.Count; i++)
                        {
                            totalFlowList.Add((string)matchList[i][4]);
                            totalWindowTimeList.Add(searchModel.OrderTime.Value);
                        }
                        this.kanbanScanMgr.OrderCard(totalFlowList.ToArray(), totalWindowTimeList.ToArray());

                        IList<string> orderNoList = OrderHolder.GetOrders();
                        string orderNos = string.Empty;
                        if (orderNoList != null && orderNoList.Count > 0)
                        {
                            foreach (var orderNo in orderNoList)
                            {
                                orderNos += orderNo + ",";
                            }
                        }

                        SaveSuccessMessage("收料单创建成功，收料单号{0}", orderNos.Substring(0, orderNos.Length - 1));

                        return Json(null);

                    }
                    else
                    {
                        DateTime[] windowTimeArray = Array.ConvertAll(windowStrTimeArray, windowTime => Convert.ToDateTime(windowTime));
                        this.kanbanScanMgr.OrderCard(flowArray, windowTimeArray);
                        IList<string> orderNoList = OrderHolder.GetOrders();

                        string orderNos = string.Empty;
                        if (orderNoList != null && orderNoList.Count > 0)
                        {
                            foreach (var orderNo in orderNoList)
                            {
                                orderNos += orderNo + ",";
                            }
                        }
                        SaveSuccessMessage("收料单创建成功，收料单号{0}", orderNos.Substring(0, orderNos.Length - 1));
                        return Json(null);
                    }

                }
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex.Message);
            }
            return Json(null);
        }

        //打印
        public string PrintOrders(string orderNos)
        {
            string[] orderNoArray = orderNos.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            string hqlMstr = "select om from OrderMaster om where OrderNo in(?";
            string hqlDet = "select od from OrderDetail od where OrderNo in(?";
            List<object> paras = new List<object>();
            paras.Add(orderNoArray[0]);
            for (int i = 1; i < orderNoArray.Length; i++)
            {
                hqlMstr = hqlMstr + ",?";
                hqlDet = hqlDet + ",?";
                paras.Add(orderNoArray[i]);
            }
            hqlDet = hqlDet + ")";
            hqlMstr = hqlMstr + ")";

            IList<OrderMaster> orderMasterList = this.genericMgr.FindAll<OrderMaster>(hqlMstr, paras.ToArray());
            IList<OrderDetail> orderDetailList = this.genericMgr.FindAll<OrderDetail>(hqlDet, paras.ToArray());


            StringBuilder printUrls = new StringBuilder();
            foreach (var orderMaster in orderMasterList)
            {
                orderMaster.OrderDetails = orderDetailList.Where(o => o.OrderNo == orderMaster.OrderNo).ToList();
                if (orderMaster.OrderDetails.Count > 0)
                {
                    PrintOrderMaster printOrderMstr = Mapper.Map<OrderMaster, PrintOrderMaster>(orderMaster);
                    IList<object> data = new List<object>();
                    data.Add(printOrderMstr);
                    data.Add(printOrderMstr.OrderDetails);
                    //string reportFileUrl = reportGen.WriteToFile(orderMaster.OrderTemplate, data);
                    string reportFileUrl = reportGen.WriteToFile("ORD_KB.xls", data);
                    printUrls.Append(reportFileUrl);
                    printUrls.Append("||");
                }
            }
            return printUrls.ToString();
        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_Order")]
        public ActionResult Order(KanbanOrderSearchModel searchModel)
        {
            TempData["KanbanOrderSearchModel"] = searchModel;
            return View();
        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_Order")]
        public ActionResult ListSupplierRegion(GridCommand command, KanbanOrderSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.SearchModel = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);

            if (string.IsNullOrEmpty(searchModel.Supplier))
            {
                SaveWarningMessage("供应商不能都为空");
                return View("Order", searchModel);
            }

            return View();
        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_Order")]
        [GridAction]
        public ActionResult _OrderSupplierRegionAjaxList(GridCommand command, KanbanOrderSearchModel searchModel)
        {
            IList<SupplierRegion> supplierRegionList = new List<SupplierRegion>();
            if (!string.IsNullOrEmpty(searchModel.Supplier))
            {
                string sql = "Select distinct Region, RegionName, Supplier, SupplierName,Flow from KB_KanbanScan s where s.IsOrdered = 0 ";

                //取用户有权限的所有区域
                User user = SecurityContextHolder.Get();
                if (user.Code.Trim().ToLower() != "su")
                {
                    sql += " and exists (select 1 from VIEW_UserPermission as up where up.UserId =" + user.Id + " and  up.CategoryType = " + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "  and up.PermissionCode = s.Region)";
                }

                if (!string.IsNullOrEmpty(searchModel.Supplier))
                {
                    sql += " and s.Supplier = '" + searchModel.Supplier + "'";
                }

                IList resultList = genericMgr.FindAllWithNativeSql(sql);
                DateTime nowDate = DateTime.Now;
                foreach (object[] result in resultList)
                {
                    SupplierRegion s = new SupplierRegion();
                    s.Region = result[0].ToString();
                    s.RegionName = result[1].ToString();
                    s.Supplier = result[2].ToString();
                    s.SupplierName = result[3].ToString();
                    s.Flow = result[4].ToString();
                    supplierRegionList.Add(s);

                    #region 算交货时间
                    FlowStrategy flowStrategy = genericMgr.FindById<FlowStrategy>(s.Flow);
                    try
                    {
                        s.WindowTime = genericMgr.FindAllWithNativeSql<DateTime>("USP_Busi_GetNextWindowTime ?,?,?", new Object[] { s.Flow, flowStrategy.PreWinTime, null }, new IType[] { NHibernateUtil.String, NHibernateUtil.DateTime, NHibernateUtil.DateTime }).FirstOrDefault();

                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null)
                        {
                            if (ex.InnerException.InnerException != null)
                            {
                                s.Memo = ex.InnerException.InnerException.Message;

                            }
                            else
                            {
                                s.Memo = ex.InnerException.Message;
                            }
                        }
                        else
                        {
                            s.Memo = ex.Message;
                        }
                    }
                    #endregion

                }
            }
            return PartialView(new GridModel<SupplierRegion> { Data = supplierRegionList });
        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_Order")]
        public ActionResult ListKanbanOrder(GridCommand command, KanbanOrderSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.SearchModel = searchModel;
            ViewBag.PageSize = base.ProcessPageSize(100);

            //if (string.IsNullOrEmpty(searchModel.Supplier))
            //{
            //    SaveWarningMessage("供应商不能都为空");
            //    return View();
            //}

            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_KanbanCard_Order")]
        public ActionResult _KanbanOrderAjaxList(GridCommand command, KanbanOrderSearchModel searchModel)
        {
            GridModel<KanbanScan> kanbanScanGrid = new GridModel<KanbanScan>(new List<KanbanScan>());
            if (!string.IsNullOrEmpty(searchModel.Flow))
            {
                SearchStatementModel searchStatementModel = this.KanbanScanPrepareSearchStatement(command, searchModel);
                kanbanScanGrid = GetAjaxPageData<KanbanScan>(searchStatementModel, command);
            }
            return PartialView(kanbanScanGrid);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_KanbanCard_ItemDailyConsume")]
        public ActionResult _KanbanOrderSaveBatch([Bind(Prefix = "updated")]IEnumerable<KanbanScan> updates,
            KanbanOrderSearchModel searchModel)
        {
            if (updates != null)
            {
                foreach (KanbanScan scan in updates)
                {
                    this.genericMgr.Update(scan);
                }
            }

            GridCommand command = new GridCommand();
            SearchStatementModel searchStatementModel = this.KanbanScanPrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<KanbanScan>(searchStatementModel, command));
        }

        private SearchStatementModel KanbanScanPrepareSearchStatement(GridCommand command, KanbanOrderSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();

            if (whereStatement == string.Empty)
            {
                //whereStatement = " where IsOrdered = 0 and exists(select 1 from FlowStrategy as s where s.Flow = o.Flow and s.IsOrderNow=0)";
                //只要没结转过的都可以前台结转，不要再看自动结转选项，因为有时候可能会自动结转失败
                whereStatement = " where IsOrdered = 0 and exists(select 1 from FlowStrategy as s where s.Flow = o.Flow)";
            }

            SecurityHelper.AddRegionPermissionStatement(ref whereStatement, "o", "Region");

            // 按单个供应商结转，或者按照物流中心结转

            if (!string.IsNullOrEmpty(searchModel.Supplier))
            {

                HqlStatementHelper.AddEqStatement("Supplier", searchModel.Supplier, "o", ref whereStatement, param);
            }

            if (!string.IsNullOrEmpty(searchModel.Region))
            {
                HqlStatementHelper.AddEqStatement("Region", searchModel.Region, "o", ref whereStatement, param);
            }



            if (!string.IsNullOrEmpty(searchModel.Flow))
            {
                HqlStatementHelper.AddEqStatement("Flow", searchModel.Flow, "o", ref whereStatement, param);
            }
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatementKanbanOrder;
            searchStatementModel.SelectStatement = selectStatementKanbanOrder;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_Card")]
        public ActionResult Card()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_New")]
        public ActionResult _KanbanDetailList(string flow, string item)
        {
            if (string.IsNullOrEmpty(flow))
            {
                SaveWarningMessage("路线为必填项。");
            }
            ViewBag.PageSize = 100;
            //ViewBag.Item = item;
            //ViewBag.Flow = flow;
            return PartialView();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_KanbanCard_New")]
        public ActionResult _AjaxFlowDetail(string flow, string item, GridCommand command)
        {
            
            if (string.IsNullOrEmpty(flow))
            {
                return PartialView(new GridModel(new List<FlowDetail>()));
            }
            string hql = " from FlowDetail f where f.Flow=?";
            if (!string.IsNullOrEmpty(item))
            {
                hql += " and f.Item='" + item + "'";
            }
            var total = genericMgr.FindAll<long>("select count(*) from FlowDetail  f where f.Flow=?" + (string.IsNullOrEmpty(item) ? string.Empty : " and f.Item='" + item + "'"), flow).FirstOrDefault();
            IList<FlowDetail> flowDetailList = genericMgr.FindAll<FlowDetail>(hql, flow, (command.Page - 1) * command.PageSize, command.Page * command.PageSize);
            if (flowDetailList != null && flowDetailList.Count > 0)
            {
                foreach (FlowDetail flowDetail in flowDetailList)
                {
                    flowDetail.ItemDescription = genericMgr.FindById<Item>(flowDetail.Item).Description;
                }
            }
            var grid = new GridModel(flowDetailList);
            grid.Total = (Int32)total;
            ViewBag.Total = total;
            return PartialView(grid);
        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_New")]
        public ActionResult CreateKanbanCard(string flowIdStr, string flowPoNoStr, string flowQtyStr, string flow, DateTime? effectiveDate)
        {
            try
            {
                if (string.IsNullOrEmpty(flowIdStr))
                {
                    throw new BusinessException("明细不能为空");
                }
                if (effectiveDate == null)
                {
                    effectiveDate = DateTime.Now;
                }

                IList<FlowDetail> nonZeroFlowDetailList = new List<FlowDetail>();
                if (!string.IsNullOrEmpty(flowIdStr))
                {
                    string[] idArray = flowIdStr.Split(',');
                    string[] poNoArray = flowPoNoStr.Split(',');

                    string[] qtyArray = flowQtyStr.Split(',');

                    if (idArray != null && idArray.Count() > 0)
                    {
                        FlowMaster flowMaster = genericMgr.FindById<FlowMaster>(flow);
                        for (int i = 0; i < idArray.Count(); i++)
                        {
                            FlowDetail flowDetail = genericMgr.FindById<FlowDetail>(Convert.ToInt32(idArray[i]));
                            // flowDetail.PONo = poNoArray[i];
                            if (string.IsNullOrEmpty(flowDetail.OprefSequence))
                            {
                                throw new BusinessException("路线{0}，零件{1}行的看板号是空，不可以创建看板。", flowDetail.Flow, flowDetail.Item);
                            }
                            flowDetail.OrderQty = Convert.ToDecimal(qtyArray[i]);
                            nonZeroFlowDetailList.Add(flowDetail);
                        }
                    }
                    IList<KanbanCard> kanbanCardList = kanbanCardMgr.AddManuallyByKanbanFlow(flow, nonZeroFlowDetailList, effectiveDate.Value, this.CurrentUser, true);

                    SaveSuccessMessage("手工创建看板成功,总张数" + kanbanCardList.Count().ToString());

                }
                return View("New");
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                return Json(null);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex.Message);
                return Json(null);
            }
        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_New")]
        public ActionResult DeleteKanbanCard(string flowIdStr, string flowQtyStr)
        {
            try
            {
                if (string.IsNullOrEmpty(flowIdStr))
                {
                    throw new BusinessException("明细不能为空");
                }

                IList<FlowDetail> nonZeroFlowDetailList = new List<FlowDetail>();
                if (!string.IsNullOrEmpty(flowIdStr))
                {
                    string[] idArray = flowIdStr.Split(',');
                    string[] qtyArray = flowQtyStr.Split(',');

                    kanbanCardMgr.DeleteManuallyByKanbanFlow(idArray, qtyArray);
                    SaveSuccessMessage("删除看板成功。");


                }
                return View("New");
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                return Json(null);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex.Message);
                return Json(null);
            }
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_KanbanCard_Kanban_View")]
        public ActionResult List(GridCommand command, KanbanCardSearchModel searchModel)
        {

            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_KanbanCard_Kanban_View")]
        public ActionResult _AjaxList(GridCommand command, KanbanCardSearchModel searchModel)
        {
            if (!string.IsNullOrWhiteSpace(searchModel.CardNo))
            {
                if (searchModel.CardNo.Length > 2 && searchModel.CardNo.Substring(0, 2).ToUpper() == "$K")
                {
                    searchModel.CardNo = searchModel.CardNo.Substring(2, searchModel.CardNo.Length - 2);
                }
            }
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            //GridModel<KanbanCard> gridModelKanbanCardList = GetAjaxPageData<KanbanCard>(searchStatementModel, command);

            //if (searchModel.FreezeDate!=null && searchModel.FreezeDate.HasValue && gridModelKanbanCardList.Data != null && gridModelKanbanCardList.Data.Count() > 0)
            //{
            //    foreach (KanbanCard kanbanCard in gridModelKanbanCardList.Data)
            //    {
            //        kanbanCard.IsFreeze = kanbanCard.FreezeDate <= searchModel.FreezeDate;
            //    }
            //}
            return PartialView(GetAjaxPageData<KanbanCard>(searchStatementModel, command));
        }


        [SconitAuthorize(Permissions = "Url_KanbanFlow_View")]
        public ActionResult Import(IEnumerable<HttpPostedFileBase> flowattachments)
        {
            try
            {
                foreach (var file in flowattachments)
                {
                    kanbanCardMgr.ImportKanBanCard(file.InputStream, com.Sconit.CodeMaster.OrderType.Transfer, CurrentUser);
                    //flowMgr.ImportKanBanFlow(file.InputStream, CodeMaster.OrderType.Procurement);
                }
                SaveSuccessMessage(Resources.Global.ImportSuccess_BatchImportSuccessful);
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(Resources.ErrorMessage.Errors_Import_Failed, ex.Message);
            }

            return Content(string.Empty);
        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, KanbanCardSearchModel searchModel)
        {
            string whereStatement = " where 1=1 ";

            IList<object> param = new List<object>();
            //区域权限
            SecurityHelper.AddRegionPermissionStatement(ref whereStatement, "k", "Region");
            if (!string.IsNullOrEmpty(searchModel.Region))
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(searchModel.Region))
                {
                    sb.Append(" and k.Region in (");
                    string[] regions = searchModel.Region.Split(',');
                    for (int ir = 0; ir < regions.Length; ir++)
                    {
                        sb.Append("?,");
                        param.Add(regions[ir]);
                    }
                    sb = sb.Remove(sb.Length - 1, 1);
                    whereStatement += sb.ToString() + ")";
                }

            }
            //HqlStatementHelper.AddEqStatement("Region", searchModel.Region, "k", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Supplier", searchModel.Supplier, "k", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "k", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("CardNo", searchModel.CardNo, "k", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("OpRef", searchModel.BinTo,HqlStatementHelper.LikeMatchMode.Start, "k", ref whereStatement, param);
            //if (searchModel.KitCount != null && searchModel.KitCount.Value > 1)
            //{
            //    HqlStatementHelper.AddEqStatement("KitCount", searchModel.KitCount.Value, "k", ref whereStatement, param);
            //}
            // HqlStatementHelper.AddEqStatement("NeedReprint", searchModel.NeedReprint, "k", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("OpRefSequence", searchModel.OpRefSequence, "k", ref whereStatement, param);

            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.StartDate, searchModel.EndDate, "k", ref whereStatement, param);
            }
            else if (searchModel.StartDate != null & searchModel.EndDate == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.StartDate, "k", ref whereStatement, param);
            }
            else if (searchModel.StartDate == null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.EndDate, "k", ref whereStatement, param);
            }

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            TempData["SortStatement"] = sortingStatement;
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_Kanban_View")]
        public ActionResult Edit(string CardNo)
        {
            KanbanCard kanbanCard = genericMgr.FindById<KanbanCard>(CardNo);
            ViewBag.isfrozen = kanbanCardMgr.IsFrozenCard(kanbanCard);

            return View(kanbanCard);
        }

        public JsonResult DeleteKanban(string CardNo)
        {
            KanbanCard card = kanbanCardMgr.GetByCardNo(CardNo);
            if (card != null)
            {
                try
                {
                    IList<KanbanCard> kanbanCardList = genericMgr.FindAll<KanbanCard>("from KanbanCard as k where k.Flow = ? and k.Item = ?", new object[] { card.Flow, card.Item });
                    if (kanbanCardList.Count() != KanbanUtility.GetSeqFromKanbanSeq(card.Sequence))
                    {
                        throw new BusinessException("当前看板不是最大看板，不能删除");
                    }
                    User user = SecurityContextHolder.Get();
                    kanbanCardMgr.DeleteKanbanCard(card, user);
                    SaveSuccessMessage("更新看板卡信息成功,看板卡号:" + card.CardNo);
                    return Json(new { });

                }
                catch (BusinessException ex)
                {
                    SaveBusinessExceptionMessage(ex);
                    return Json(null);
                }
            }
            else
            {
                SaveErrorMessage("看板卡不存在");
                return Json(null);
            }
        }

        public string Print(string cardNos)
        {
            IList<object> data = new List<object>();
            IList<KanbanCard> kanbanCardList = null;

            try
            {
                if (!string.IsNullOrEmpty(cardNos))
                {
                    string[] array = cardNos.Split(',');

                    string selectStatement = string.Empty;
                    IList<object> selectPartyPara = new List<object>();
                    foreach (var para in array)
                    {
                        if (selectStatement == string.Empty)
                        {
                            selectStatement = " from KanbanCard where CardNo in (?";
                        }
                        else
                        {
                            selectStatement += ",?";
                        }
                        selectPartyPara.Add(para);
                    }
                    selectStatement += ")";

                    kanbanCardList = genericMgr.FindAll<KanbanCard>(selectStatement, selectPartyPara.ToArray());
                }
                else
                {
                    throw new BusinessException("看板单号不能为空.");
                }

                foreach (var kanbanCard in kanbanCardList)
                {
                    if (string.IsNullOrEmpty(kanbanCard.LogisticCenterCode))
                    {
                        kanbanCard.LogisticCenterCode = kanbanCard.SupplierName;
                    }
                    Item item = genericMgr.FindById<Item>(kanbanCard.Item);
                    IList<ItemKit> itemKitList = null;

                    //如果是套件
                    if (item.IsKit)
                    {
                        itemKitList = genericMgr.FindAll<ItemKit>(" from ItemKit as i where i.KitItem=? ", kanbanCard.Item);
                        foreach (var itemKit in itemKitList)
                        {
                            itemKit.ChildItemDescription = genericMgr.FindById<Item>(itemKit.ChildItem).Description;
                        }
                        kanbanCard.ItemKitList = itemKitList;
                    }
                    else
                    {
                        IList<ItemKit> newsItemKit = new List<ItemKit>();
                        ItemKit itemKit = new ItemKit();
                        itemKit.ChildItemDescription = genericMgr.FindById<Item>(kanbanCard.Item).Description;
                        itemKit.ChildItem = genericMgr.FindById<Item>(kanbanCard.Item);
                        newsItemKit.Add(itemKit);
                        kanbanCard.ItemKitList = newsItemKit;
                    }


                }
                data.Add(kanbanCardList);
                var kanbanCardList1 = kanbanCardList.Where(p => p.ItemKitList.Count == 1).ToList();

                if (kanbanCardList[0].ItemKitList.Count() == 1)
                {
                    return reportGen.WriteToFile("BarCodeOneItemKitA4.xls", data);
                }
                else
                {
                    return reportGen.WriteToFile("BarCodesTwoItemkitA4.xls", data);
                }
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);


            }

            return string.Empty;
        }

        [SconitAuthorize(Permissions = "Url_KanbanCard_Kanban_View")]
        public string PrintcardNos(string cardNos, KanbanCardSearchModel searchModel)
        {
            IList<object> data = new List<object>();
            IList<KanbanCard> kanbanCardList = null;
            string sortStatement = TempData["SortStatement"] != null ? (string)TempData["SortStatement"] : " order by CreateDate asc ";
            TempData["SortStatement"] = sortStatement;
            try
            {
                if (!string.IsNullOrEmpty(cardNos))
                {
                    //根据查询条件打印
                    if (cardNos.Equals("ALL", StringComparison.OrdinalIgnoreCase))
                    {
                        string whereStatement = " where 1=1 ";

                        IList<object> param = new List<object>();
                        //区域权限
                        SecurityHelper.AddRegionPermissionStatement(ref whereStatement, "k", "Region");
                        if (!string.IsNullOrEmpty(searchModel.Region))
                        {
                            System.Text.StringBuilder sb = new System.Text.StringBuilder();
                            if (!string.IsNullOrEmpty(searchModel.Region))
                            {
                                sb.Append(" and k.Region in (");
                                string[] regions = searchModel.Region.Split(',');
                                for (int ir = 0; ir < regions.Length; ir++)
                                {
                                    sb.Append("?,");
                                    param.Add(regions[ir]);
                                }
                                sb = sb.Remove(sb.Length - 1, 1);
                                whereStatement += sb.ToString() + ")";
                            }
                        }
                        HqlStatementHelper.AddEqStatement("Supplier", searchModel.Supplier, "k", ref whereStatement, param);
                        HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "k", ref whereStatement, param);
                        HqlStatementHelper.AddEqStatement("CardNo", searchModel.CardNo, "k", ref whereStatement, param);
                        HqlStatementHelper.AddLikeStatement("OpRef", searchModel.BinTo, HqlStatementHelper.LikeMatchMode.Start, "k", ref whereStatement, param);
                        HqlStatementHelper.AddEqStatement("OpRefSequence", searchModel.OpRefSequence, "k", ref whereStatement, param);

                        if (searchModel.StartDate != null & searchModel.EndDate != null)
                        {
                            HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.StartDate, searchModel.EndDate, "k", ref whereStatement, param);
                        }
                        else if (searchModel.StartDate != null & searchModel.EndDate == null)
                        {
                            HqlStatementHelper.AddGeStatement("CreateDate", searchModel.StartDate, "k", ref whereStatement, param);
                        }
                        else if (searchModel.StartDate == null & searchModel.EndDate != null)
                        {
                            HqlStatementHelper.AddLeStatement("CreateDate", searchModel.EndDate, "k", ref whereStatement, param);
                        }

                        kanbanCardList = this.genericMgr.FindAll<KanbanCard>("from KanbanCard k " + whereStatement + sortStatement, param.ToArray());
                    }
                    //根据选择的行数打印
                    else
                    {
                        string[] array = cardNos.Split(',');

                        string selectStatement = string.Empty;
                        IList<object> selectPartyPara = new List<object>();
                        foreach (var para in array)
                        {
                            if (selectStatement == string.Empty)
                            {
                                selectStatement = " from KanbanCard where CardNo in (?";
                            }
                            else
                            {
                                selectStatement += ",?";
                            }
                            selectPartyPara.Add(para);
                        }
                        selectStatement += ")";

                        kanbanCardList = genericMgr.FindAll<KanbanCard>(selectStatement + sortStatement, selectPartyPara.ToArray());
                    }
                }
                else
                {
                    throw new BusinessException("看板单号不能为空.");
                }

                string printUrl = string.Empty;
                if (kanbanCardList.Count() > 0)
                {
                    if (printUrl == string.Empty)
                    {
                        IList<PrintKanBanCard> printList = Mapper.Map<IList<KanbanCard>, IList<PrintKanBanCard>>(kanbanCardList);
                        data.Add(printList);
                        printUrl = reportGen.WriteToFile("KanBanCard.xls", data);
                    }
                }
                return printUrl;
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);


            }

            return string.Empty;
        }

        #region 导出

        #region 看办卡查询导出
        public void ExportKanbanCard(KanbanCardSearchModel searchModel)
        {
            StringBuilder sb = new StringBuilder();
            IList<object> param = new List<object>();
            string selectStatement = "  select k from KanbanCard as k where 1=1  ";
            SecurityHelper.AddRegionPermissionStatement(ref selectStatement, "k", "Region");
            sb.Append(selectStatement);
            if (!string.IsNullOrEmpty(searchModel.Region))
            {
                // System.Text.StringBuilder sb = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(searchModel.Region))
                {
                    sb.Append(" and k.Region in (");
                    string[] regions = searchModel.Region.Split(',');
                    for (int ir = 0; ir < regions.Length; ir++)
                    {
                        sb.Append("?,");
                        param.Add(regions[ir]);
                    }
                    sb = sb.Remove(sb.Length - 1, 1);
                    sb.Append(")");
                }

            }

            if (!string.IsNullOrWhiteSpace(searchModel.Supplier))
            {
                sb.Append(" and Supplier = ?");
                param.Add(searchModel.Supplier);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Item))
            {
                sb.Append(" and Item = ?");
                param.Add(searchModel.Item);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.CardNo))
            {
                sb.Append(" and CardNo = ?");
                param.Add(searchModel.CardNo);
            }
            if (searchModel.KBCalc != null)
            {
                sb.Append(" and KBCalc = ?");
                param.Add(searchModel.KBCalc);
            }
            if (searchModel.FreezeDate != null)
            {
                sb.Append(" and FreezeDate <=?");
                param.Add(searchModel.FreezeDate);
            }
            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                sb.Append(" and CreateDate between ? and ?");
                param.Add(searchModel.StartDate);
                param.Add(searchModel.EndDate);
            }
            else if (searchModel.StartDate != null & searchModel.EndDate == null)
            {
                sb.Append(" and CreateDate >= ?");
                param.Add(searchModel.StartDate);
            }
            else if (searchModel.StartDate == null & searchModel.EndDate != null)
            {
                sb.Append(" and CreateDate <= ?");
                param.Add(searchModel.EndDate);
            }
            IList<KanbanCard> kanbanCardList = this.genericMgr.FindAll<KanbanCard>(sb.ToString(), param.ToArray());
            ExportToXLS<KanbanCard>("ExportKanbanCard", "xls", kanbanCardList);

        }
        #endregion

        #endregion

    }

}
