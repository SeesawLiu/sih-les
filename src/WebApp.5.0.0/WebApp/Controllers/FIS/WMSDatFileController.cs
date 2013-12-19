using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using com.Sconit.Web.Util;
using com.Sconit.Web.Models;
using com.Sconit.Entity.FIS;
using com.Sconit.Service;
using com.Sconit.Web.Models.SearchModels.SCM;
using com.Sconit.Web.Models.SearchModels.ORD;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.ORD;
using com.Sconit.Utility.Report;
using System.Text;


namespace com.Sconit.Web.Controllers.FIS
{
    public class WMSDatFileController : WebAppBaseController
    {
        public IReportGen reportGen { get; set; }
        //
        // GET: /ItemFlow/

        public ActionResult Index()
        {
            TempData["DatFileSearchModel"] = null;
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_ItemFlow_View")]
        public ActionResult List(GridCommand command, DatFileSearchModel searchModel)
        {
           
            //SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            TempData["DatFileSearchModel"] = searchModel;
            if (this.CheckSearchModelIsNull(searchModel))
            {
                TempData["_AjaxMessage"] = "";
            }
            else
            {
                SaveWarningMessage(Resources.ErrorMessage.Errors_NoConditions);
            }
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_ItemFlow_View")]
        public ActionResult _AjaxList(GridCommand command, DatFileSearchModel searchModel)
        {
            if (!this.CheckSearchModelIsNull(searchModel))
            {
                return PartialView(new GridModel(new List<WMSDatFile>()));
            }

            string sql = this.StringBuilderPrepareSearchStatement(command, searchModel).ToString();
            TempData["searchSql"] = sql;
            //IList<object> countList = base.genericMgr.FindAllWithNativeSql<object>("select count(*) from (" + sql + ") as t2");
            //string lesInLogSql = "select * from (" + sql;
            //IList<object[]> searchList = base.genericMgr.FindAllWithNativeSql<object[]>(lesInLogSql + string.Format(") as t2 where t2.RowId between {0} and {1}", (command.Page - 1) * command.PageSize, command.Page * command.PageSize));
            IList<object[]> searchList = base.genericMgr.FindAllWithNativeSql<object[]>(sql);
            IList<WMSDatFile> wMSDatFileList = new List<WMSDatFile>();
            var returnlList = new List<WMSDatFile>();
            if (searchList != null && searchList.Count > 0)
            {
                #region
                wMSDatFileList = (from tak in searchList
                                  select new WMSDatFile
                                {
                                    WmsLine=((int)tak[0]).ToString(),
                                    Id = tak[1] != null ? (int)tak[1] : 1,
                                    WmsNo = (string)tak[2],
                                    MoveType = (string)tak[3]+(string)tak[4],
                                    // Sequense = (string)tak[0],
                                    WMSId = (string)tak[5],
                                    Item = (string)tak[6],
                                    Uom = (string)tak[7],
                                    UMLGO = (string)tak[8],
                                    Qty = tak[9] != null ? (decimal)tak[9] : 0,
                                    IsHand = tak[10] != null ? (bool)tak[10] : false,
                                    CreateDateFormat = tak[11] != null ? (DateTime?)tak[11] : null,
                                    ItemDescription = tak[12] != null ? (string)tak[12] : string.Empty,
                                    ReferenceItemCode = (string)tak[13],
                                    OrderQty = tak[25]!=null && (decimal?)tak[25]==1?(decimal)tak[24]:(decimal)tak[14],
                                    ReceiveTotal = tak[15] != null ? (decimal)tak[15] : 0,
                                    CancelQty = tak[16] != null ? (decimal)tak[16] : 0,
                                    LGORT = tak[17] != null?(string)tak[17]:string.Empty,
                                    RequirementDate = tak[18] != null ? (DateTime?)tak[18] : null,
                                    OrderNo = (string)tak[19],
                                    PartyTo = (string)tak[20],
                                    PartyFrom = (string)tak[21],
                                    WindowTime = (DateTime)tak[22],
                                    OrderStrategyDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.FlowStrategy, int.Parse((tak[23]).ToString())),
                                    ReceiveLotSize = tak[25]!=null && (decimal?)tak[25]==1?true:false,
                                }).ToList();
                #endregion

                #region

                var cancelList = wMSDatFileList.Where(w => w.MoveType == "312" || w.MoveType == "412").ToList();
                 returnlList = wMSDatFileList.Where(w => w.MoveType != "312" && w.MoveType != "412").ToList();
                if (cancelList != null && cancelList.Count > 0)
                {
                    foreach (WMSDatFile c in cancelList)
                    {
                        if (c.MoveType == "312")
                        {
                            var cancelFile = returnlList.Where(r => r.MoveType == "311" && c.MoveType == "312" && r.SOBKZ == c.SOBKZ && r.Qty == c.Qty && r.WmsLine == c.WmsLine
                                && r.ReceiveTotal - r.CancelQty == 0 && r.WmsNo == c.WmsNo).ToList();
                            if (cancelFile != null && cancelFile.Count > 0)
                            {
                                returnlList.Remove(cancelFile.First());
                            }
                        }
                        else if (c.MoveType == "412")
                        {
                            var cancelFile = returnlList.Where(r => r.MoveType == "411" && c.MoveType == "412" && r.SOBKZ == c.SOBKZ && r.Qty == c.Qty && r.WmsLine == c.WmsLine
                                && r.ReceiveTotal - r.CancelQty == 0 && r.WmsNo == c.WmsNo).ToList();
                            if (cancelFile != null && cancelFile.Count > 0)
                            {
                                returnlList.Remove(cancelFile.First());
                            }
                        }
                    }
                }

                #region 冲销的相互抵消
                //foreach (WMSDatFile wMSDatFile in wMSDatFileList)
                //{
                //    if (wMSDatFile.MoveType == null)
                //    {
                //        continue;
                //    }
                //    foreach (WMSDatFile wmsFile in wMSDatFileList)
                //    {
                //        if (wmsFile.MoveType == null)
                //        {
                //            continue;
                //        }
                //        if (wMSDatFile.MoveType + wMSDatFile.SOBKZ == "311" && wmsFile.MoveType + wmsFile.SOBKZ == "312" && wmsFile.Qty == wMSDatFile.Qty && wmsFile.WmsLine == wMSDatFile.WmsLine
                //            && wmsFile.ReceiveTotal - wmsFile.CancelQty == 0 && wMSDatFile.ReceiveTotal - wMSDatFile.CancelQty == 0 && wmsFile.WmsNo==wMSDatFile.WmsNo)
                //        {
                //            wmsFile.MoveType = null;
                //            wMSDatFile.MoveType = null;
                //            break;
                //        }
                //        else if (wMSDatFile.MoveType + wMSDatFile.SOBKZ == "311K" && wmsFile.MoveType + wmsFile.SOBKZ == "312K" && wmsFile.Qty == wMSDatFile.Qty && wmsFile.WmsLine == wMSDatFile.WmsLine
                //            && wmsFile.ReceiveTotal - wmsFile.CancelQty == 0 && wMSDatFile.ReceiveTotal - wMSDatFile.CancelQty == 0 && wmsFile.WmsNo == wMSDatFile.WmsNo)
                //        {
                //            wmsFile.MoveType = null;
                //            wMSDatFile.MoveType = null;
                //            break;
                //        }

                //        else if (wMSDatFile.MoveType + wMSDatFile.SOBKZ == "411" && wmsFile.MoveType + wMSDatFile.SOBKZ == "412" && wmsFile.Qty == wMSDatFile.Qty && wmsFile.WmsLine == wMSDatFile.WmsLine
                //            && wmsFile.ReceiveTotal - wmsFile.CancelQty == 0 && wMSDatFile.ReceiveTotal - wMSDatFile.CancelQty == 0 && wmsFile.WmsNo == wMSDatFile.WmsNo)
                //        {
                //            wmsFile.MoveType = null;
                //            wMSDatFile.MoveType = null;
                //            break;
                //        }
                //        else if (wMSDatFile.MoveType + wMSDatFile.SOBKZ == "411K" && wmsFile.MoveType + wMSDatFile.SOBKZ == "412K" && wmsFile.Qty == wMSDatFile.Qty && wmsFile.WmsLine == wMSDatFile.WmsLine
                //            && wmsFile.ReceiveTotal - wmsFile.CancelQty == 0 && wMSDatFile.ReceiveTotal - wMSDatFile.CancelQty == 0 && wmsFile.WmsNo == wMSDatFile.WmsNo)
                //        {
                //            wmsFile.MoveType = null;
                //            wMSDatFile.MoveType = null;
                //            break;
                //        }
                //    }
                //}
                #endregion
                #endregion

            }
            //IEnumerable<WMSDatFile> wmsList = wMSDatFileList.Where(o => o.MoveType != null && o.MoveType != "312" && o.MoveType != "412");
            //var count = wMSDatFileList.Where(o => o.MoveType == null || o.MoveType == "312" || o.MoveType == "412");
            GridModel<WMSDatFile> gridModelOrderDet = new GridModel<WMSDatFile>();
            gridModelOrderDet.Total = returnlList.Count;
            gridModelOrderDet.Data = returnlList.Skip((command.Page - 1) * command.PageSize).Take(command.PageSize);
            return PartialView(gridModelOrderDet);
        }

        #region 导出
        public void SaveToClient()
        {
            try
            {
                string sql = TempData["searchSql"] as string;
                TempData["searchSql"] = sql;

                IList<object[]> searchList = base.genericMgr.FindAllWithNativeSql<object[]>(sql);
                IList<WMSDatFile> wMSDatFileList = new List<WMSDatFile>();
                var returnlList = new List<WMSDatFile>();
                if (searchList != null && searchList.Count > 0)
                {
                    #region
                    wMSDatFileList = (from tak in searchList
                                      select new WMSDatFile
                                      {
                                          WmsLine = ((int)tak[0]).ToString(),
                                          Id = tak[1] != null ? (int)tak[1] : 1,
                                          WmsNo = (string)tak[2],
                                          MoveType = (string)tak[3] + (string)tak[4],
                                          // Sequense = (string)tak[0],
                                          WMSId = (string)tak[5],
                                          Item = (string)tak[6],
                                          Uom = (string)tak[7],
                                          UMLGO = (string)tak[8],
                                          Qty = tak[9] != null ? (decimal)tak[9] : 0,
                                          IsHand = tak[10] != null ? (bool)tak[10] : false,
                                          CreateDateFormat = tak[11] != null ? (DateTime?)tak[11] : null,
                                          ItemDescription = tak[12] != null ? (string)tak[12] : string.Empty,
                                          ReferenceItemCode = (string)tak[13],
                                          OrderQty = tak[25] != null && (decimal?)tak[25] == 1 ? (decimal)tak[24] : (decimal)tak[14],
                                          ReceiveTotal = tak[15] != null ? (decimal)tak[15] : 0,
                                          CancelQty = tak[16] != null ? (decimal)tak[16] : 0,
                                          LGORT = tak[17] != null ? (string)tak[17] : string.Empty,
                                          RequirementDate = tak[18] != null ? (DateTime?)tak[18] : null,
                                          OrderNo = (string)tak[19],
                                          PartyTo = (string)tak[20],
                                          PartyFrom = (string)tak[21],
                                          WindowTime = (DateTime)tak[22],
                                          OrderStrategyDescription = systemMgr.GetCodeDetailDescription(Sconit.CodeMaster.CodeMaster.FlowStrategy, int.Parse((tak[23]).ToString())),
                                          ReceiveLotSize = tak[25] != null && (decimal?)tak[25] == 1 ? true : false,
                                      }).ToList();
                    #endregion

                    var cancelList = wMSDatFileList.Where(w => w.MoveType == "312" || w.MoveType == "412").ToList();
                    returnlList = wMSDatFileList.Where(w => w.MoveType != "312" && w.MoveType != "412").ToList();
                    if (cancelList != null && cancelList.Count > 0)
                    {
                        foreach (WMSDatFile c in cancelList)
                        {
                            if (c.MoveType == "312")
                            {
                                var cancelFile = returnlList.Where(r => r.MoveType == "311" && c.MoveType == "312" && r.SOBKZ == c.SOBKZ && r.Qty == c.Qty && r.WmsLine == c.WmsLine
                                    && r.ReceiveTotal - r.CancelQty == 0 && r.WmsNo == c.WmsNo).ToList();
                                if (cancelFile != null && cancelFile.Count > 0)
                                {
                                    returnlList.Remove(cancelFile.First());
                                }
                            }
                            else if (c.MoveType == "412")
                            {
                                var cancelFile = returnlList.Where(r => r.MoveType == "411" && c.MoveType == "412" && r.SOBKZ == c.SOBKZ && r.Qty == c.Qty && r.WmsLine == c.WmsLine
                                    && r.ReceiveTotal - r.CancelQty == 0 && r.WmsNo == c.WmsNo).ToList();
                                if (cancelFile != null && cancelFile.Count > 0)
                                {
                                    returnlList.Remove(cancelFile.First());
                                }
                            }
                        }
                    }
                }
                //IList<object> data = new List<object>();
                //data.Add(wMSDatFileList);
                //reportGen.WriteToClient("WMSDatFile.xls", data, "WMSDatFile.xls");

                ExportToXLS<WMSDatFile>("ExportWMSDatFile", "XLS", returnlList.Take(65000).ToList());
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        private SearchStatementModel PrepareSearchStatement(GridCommand command, DatFileSearchModel searchModel)
        {
            string whereStatement = string.Empty;
            if (!string.IsNullOrEmpty(searchModel.OrderNo))
            {
                whereStatement += "where WmsLine in (select Convert(varchar(50),Id) from OrderDetail  where OrderNo='"+searchModel.OrderNo+"')";
            }
            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("WmsNo", searchModel.WmsPickNo, HqlStatementHelper.LikeMatchMode.Anywhere, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("MoveType", searchModel.MoveType, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("WMSId", searchModel.WMSId, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("LGORT", searchModel.LGORT, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("UMLGO", searchModel.UMLGO, "l", ref whereStatement, param);

            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.StartDate, searchModel.EndDate, "l", ref whereStatement, param);
            }
            else if (searchModel.StartDate != null & searchModel.EndDate == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.StartDate, "l", ref whereStatement, param);
            }
            else if (searchModel.StartDate == null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.EndDate, "l", ref whereStatement, param);
            }

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
           
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by CreateDate desc";
            }
            else
            {
                sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            }
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = "select count(*) from WMSDatFile as l";
            searchStatementModel.SelectStatement = "select l from WMSDatFile as l";
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();
            return searchStatementModel;
        }

        private StringBuilder StringBuilderPrepareSearchStatement(GridCommand command, DatFileSearchModel searchModel)
        {
            StringBuilder sb = new StringBuilder();
            // sb.Append("select * from (");
            //if (command.SortDescriptors.Count > 0)
            //{
            //    sb.Append(string.Format("select RowId=ROW_NUMBER()OVER( order by {0} asc),* from ", command.SortDescriptors[0].Member));
            //}
            //else
            //{
            //    sb.Append("select RowId=ROW_NUMBER()OVER( order by WindowTime asc),* from");
            //}
            sb.Append(@" select orderDet.Id,dat.Id,dat.WmsNo,dat.MoveType,dat.SOBKZ,dat.WMSId,orderDet.Item,orderDet.Uom,dat.UMLGO,dat.Qty,dat.IsHand,dat.CreateDate,
orderDet.ItemDesc as ItemDescription,orderDet.RefItemCode as ReferenceItemCode,
orderDet.OrderQty as OrderQty,
dat.ReceiveTotal,dat.CancelQty,dat.LGORT,orderDet.CreateDate as RequirementDate,orderDet.OrderNo,m.PartyTo,m.PartyFrom,m.WindowTime,m.OrderStrategy,orderDet.UnitPrice,orderDet.RecLotSize
 from Ord_OrderDet_2 as orderDet with(nolock) 
 inner join Ord_OrderMstr_2 as m with(nolock) on orderDet.OrderNo=m.OrderNo
 left join FIS_WMSDatFile as dat with(nolock) on dat.OrderDetId=orderDet.Id
 where 1=1  and m.CreateDate>'2013-10-01' and orderDet.OrderQty>0 and m.Status not in(0,5)  ");

            //if (!string.IsNullOrEmpty(searchModel.AsnNo))
            //{
            //    sb.Append(string.Format(" and lesInlog.ASNNo='{0}'", searchModel.AsnNo));
            //}
            if (!string.IsNullOrEmpty(searchModel.OrderNo))
            {
                sb.Append(string.Format(" and orderDet.OrderNo = '{0}'", searchModel.OrderNo));
            }
            if (!string.IsNullOrEmpty(searchModel.LGORT))
            {
                sb.Append(string.Format(" and dat.LGORT = '{0}'", searchModel.LGORT));
            }
            if (!string.IsNullOrEmpty(searchModel.UMLGO))
            {
                sb.Append(string.Format(" and dat.UMLGO = '{0}'", searchModel.UMLGO));
            }
            //if (!string.IsNullOrEmpty(searchModel.OrderNo))
            //{
            //    sb.Append(string.Format(" and dat.WmsLine in(select Convert(varchar(50),Id) from view_OrderDet  where OrderNo='{0}')", searchModel.OrderNo));
            //}
            if (!string.IsNullOrEmpty(searchModel.WmsPickNo))
            {
                sb.Append(string.Format(" and dat.WmsNo='{0}'", searchModel.WmsPickNo));
            }
            if (!string.IsNullOrEmpty(searchModel.MoveType))
            {
                sb.Append(string.Format(" and dat.MoveType='{0}'", searchModel.MoveType));
            }
            if (!string.IsNullOrEmpty(searchModel.Item))
            {
                sb.Append(string.Format(" and orderDet.Item='{0}'", searchModel.Item));
            }
            if (!string.IsNullOrEmpty(searchModel.WMSId))
            {
                sb.Append(string.Format(" and dat.WMSId='{0}'", searchModel.WMSId));
            }
            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                sb.Append(string.Format(" and  m.WindowTime between '{0}' and '{1}'", searchModel.StartDate, searchModel.EndDate));
            }
            else if (searchModel.StartDate != null & searchModel.EndDate == null)
            {
                sb.Append(string.Format(" and m.WindowTime > '{0}'", searchModel.StartDate));
            }
            else if (searchModel.StartDate == null & searchModel.EndDate != null)
            {
                sb.Append(string.Format(" and m.WindowTime < '{0}'", searchModel.EndDate));
            }
            if (searchModel.IsClsoe)
            {
                sb.Append(string.Format(" and (dat.Qty+dat.CancelQty)>dat.ReceiveTotal ", searchModel.WMSId));
            }
            if (searchModel.IsNoneOut)
            {
                sb.Append(string.Format(" and dat.Id is null and orderDet.OrderQty>orderDet.RecQty"));
            }
            if (!string.IsNullOrWhiteSpace(searchModel.PartyTo))
            {
                sb.Append(string.Format(" and m.PartyTo='{0}'",searchModel.PartyTo));
            }
            if (!string.IsNullOrWhiteSpace(searchModel.PartyFrom))
            {
                sb.Append(string.Format(" and m.PartyFrom='{0}'", searchModel.PartyFrom));
            }
            else {
                sb.Append(string.Format(" and m.PartyFrom in ('SQC','LOC')", searchModel.PartyFrom));
            }
            if (searchModel.OrderStrategy!=null)
            {
                if (searchModel.OrderStrategy.Value == 1 || searchModel.OrderStrategy.Value == 0)
                {
                    sb.Append(string.Format(" and m.OrderStrategy in (0,1) "));
                }
                else
                {
                    sb.Append(string.Format(" and m.OrderStrategy={0} ", searchModel.OrderStrategy.Value));
                }
            }
            //sb.Append(") as t1");
           
           
            if (command.SortDescriptors.Count == 0)
            {
               sb.Append(" order by WindowTime asc");
            }
            else
            {
                sb.Append(HqlStatementHelper.GetSortingStatement(command.SortDescriptors));
            }
            return sb;
        }


        

    }
}
