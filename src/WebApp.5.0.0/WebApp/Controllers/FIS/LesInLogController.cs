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
using com.Sconit.Utility.Report;
using com.Sconit.Entity.ORD;
using System.Text;


namespace com.Sconit.Web.Controllers.FIS
{
    public class LesInLogController : WebAppBaseController
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
           // ViewBag.Type = searchModel.Type;
            TempData["DatFileSearchModel"] = searchModel;
            ViewBag.IsCreateDat = searchModel.IsCreateDat;
            ViewBag.HandResult = searchModel.HandResult;
            ViewBag.Item = searchModel.Item;
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
                return PartialView(new GridModel(new List<LesINLog>()));
            }
            #region old
            //SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            //TempData["searchLesInLogStatementModel"] = searchStatementModel;
            
            //GridModel<LesINLog> List = GetAjaxPageData<LesINLog>(searchStatementModel, command);
            //IList<IpDetail> ipDetailList = base.genericMgr.FindAll<IpDetail>("select i from IpDetail as i where OrderType = 8");
            //IList<ReceiptDetail> recDetailList = base.genericMgr.FindAll<ReceiptDetail>("select d from ReceiptDetail as d where OrderType = 8");

            //foreach (LesINLog lesINLog in List.Data)
            //{
            //    IList<IpDetail> matchedIpDetail = (from ip in ipDetailList
            //                                       where ip.IpNo == lesINLog.ASNNo && ip.ExternalOrderNo == lesINLog.ExtNo
            //                                       && ip.ExternalSequence == lesINLog.POLine
            //                                       select ip).ToList();
            //    if (matchedIpDetail != null && matchedIpDetail.Count > 0)
            //    {
            //        lesINLog.ShipQty = matchedIpDetail.First().Qty;
            //        lesINLog.LocTo = matchedIpDetail.First().LocationTo;
            //    }
            //    IList<ReceiptDetail> matchedRecDetail = (from rd in recDetailList
            //                                             where rd.ReceiptNo == lesINLog.PO
            //                                             select rd).ToList();
            //    if (matchedRecDetail != null && matchedRecDetail.Count > 0)
            //    {
            //        lesINLog.ReceivedQty = matchedRecDetail.First().ReceivedQty;
            //    }
            //}

            //TempData["searchLesInLogTotal"] = List.Total;
            #endregion
           
            string sql=this.StringBuilderPrepareSearchStatement(command, searchModel).ToString();
            TempData["searchSql"] = sql;
            // sb.Append(string.Format( ") as t1 where t1.RowId between {0} and {1}",(command.Page-1)*command.PageSize,command.Page*command.PageSize));
            IList<object> countList = base.genericMgr.FindAllWithNativeSql<object>("select count(*) from ("+sql+") as t2");
            string lesInLogSql = "select * from ("+sql;
            IList<object[]> searchList = base.genericMgr.FindAllWithNativeSql<object[]>(lesInLogSql + string.Format(") as t2 where t2.RowId between {0} and {1}", (command.Page - 1) * command.PageSize, command.Page * command.PageSize));
            IList<LesINLog> lesInLogList = new List<LesINLog>();
            if (searchList != null && searchList.Count > 0)
            {
                //Id, Type, MoveType, Sequense, PO, POLine, WMSNo,
                //WMSLine, HandTime, Item, HandResult, ErrorCause, IsCreateDat, FileName, ASNNo, ExtNo
                #region
                lesInLogList = (from tak in searchList
                                select new LesINLog
                                {
                                    Id = (int)tak[1],
                                    Type = (string)tak[2],
                                    MoveType = (string)tak[3],
                                   // Sequense = (string)tak[0],
                                    PO = (string)tak[5],
                                    POLine = (string)tak[6],
                                    WMSNo = (string)tak[7],
                                    WMSLine = (string)tak[8],
                                    HandTime = Convert.ToDateTime(tak[9]),
                                    Item = (string)tak[10],
                                    HandResult = (string)tak[11],
                                    ErrorCause = tak[12] != null ? (string)tak[12] : string.Empty,
                                    IsCreateDat = (bool)tak[13],
                                    FileName = (string)tak[14],
                                    ASNNo = (string)tak[15],
                                    ExtNo = (string)tak[16],
                                    ShipQty = tak[17] != null ? (decimal)tak[17] : 0,
                                    LocTo = tak[18] != null ? (string)tak[18] : string.Empty,
                                    ReceivedQty = tak[19]!=null?(decimal)tak[19]:0,
                                }).ToList();
                #endregion
            }
            GridModel<LesINLog> gridModelOrderDet = new GridModel<LesINLog>();
            gridModelOrderDet.Total = Convert.ToInt32(countList[0]);
            gridModelOrderDet.Data = lesInLogList;

            return PartialView(gridModelOrderDet);
        }

        #region 导出
        public void SaveToClient()
        {
            try
            {
                #region old

                //SearchStatementModel searchStatementModel = TempData["searchLesInLogStatementModel"] as SearchStatementModel;
                //TempData["searchLesInLogStatementModel"] = searchStatementModel;

                //GridCommand command = new GridCommand();
                //command.Page = 1;
                //command.PageSize = (int)TempData["searchLesInLogTotal"];
                //TempData["searchLesInLogTotal"] = command.PageSize;
                //GridModel<LesINLog> List = GetAjaxPageData<LesINLog>(searchStatementModel, command);
                //int i = 0;
                //IList<LesINLog> printLesInLog = new List<LesINLog>();
                //IList<IpDetail> ipDetailList = base.genericMgr.FindAll<IpDetail>("select i from IpDetail as i where OrderType = 8");
                //IList<ReceiptDetail> recDetailList = base.genericMgr.FindAll<ReceiptDetail>("select d from ReceiptDetail as d where OrderType = 8");
                //IList<ReceiptMaster> recMstrList = base.genericMgr.FindAll<ReceiptMaster>("select m from ReceiptMaster as m where OrderType = 8");

                //foreach (LesINLog lesINLog in List.Data)
                //{
                //    i++;
                //    if (i < int.Parse(sCount)+1)
                //    {
                //        continue;
                //    }
                //    if (i > int.Parse(eCount)+1)
                //    {
                //        break;
                //    }
                //    IList<IpDetail> matchedIpDetail = (from ip in ipDetailList
                //                                       where ip.IpNo == lesINLog.ASNNo && ip.ExternalOrderNo == lesINLog.ExtNo
                //                                       && ip.ExternalSequence == lesINLog.POLine
                //                                       select ip).ToList();
                //    if (matchedIpDetail != null && matchedIpDetail.Count > 0)
                //    {
                //        lesINLog.ShipQty = matchedIpDetail.First().Qty;
                //        lesINLog.LocTo = matchedIpDetail.First().LocationTo;
                //    }
                //    IList<ReceiptDetail> matchedRecDetail = (from rd in recDetailList
                //                                             where rd.ReceiptNo == lesINLog.PO
                //                                             select rd).ToList();
                //    if (matchedRecDetail != null && matchedRecDetail.Count > 0)
                //    {
                //        lesINLog.ReceivedQty = matchedRecDetail.First().ReceivedQty;
                //    }
                //    printLesInLog.Add(lesINLog);

                //}
                #endregion

                string sql = TempData["searchSql"] as string;
                TempData["searchSql"] = sql;
                IList<object[]> searchList = base.genericMgr.FindAllWithNativeSql<object[]>(sql);
                IList<LesINLog> lesInLogList = new List<LesINLog>();
                if (searchList != null && searchList.Count > 0)
                {
                    #region
                    lesInLogList = (from tak in searchList
                                    select new LesINLog
                                    {
                                        Id = (int)tak[1],
                                        Type = (string)tak[2],
                                        MoveType = (string)tak[3],
                                        // Sequense = (string)tak[0],
                                        PO = (string)tak[5],
                                        POLine = (string)tak[6],
                                        WMSNo = (string)tak[7],
                                        WMSLine = (string)tak[8],
                                        HandTime = Convert.ToDateTime(tak[9]),
                                        Item = (string)tak[10],
                                        HandResult = (string)tak[11],
                                        ErrorCause = tak[12] != null ? (string)tak[12] : string.Empty,
                                        IsCreateDat = (bool)tak[13],
                                        FileName = (string)tak[14],
                                        ASNNo = (string)tak[15],
                                        ExtNo = (string)tak[16],
                                        ShipQty = tak[17] != null ? (decimal)tak[17] : 0,
                                        LocTo = tak[18] != null ? (string)tak[18] : string.Empty,
                                        ReceivedQty = tak[19] != null ? (decimal)tak[19] : 0,
                                    }).ToList();
                    #endregion
                }
                IList<object> data = new List<object>();
                data.Add(lesInLogList);
                reportGen.WriteToClient("LesInLog.xls", data, "LesInLog.xls");
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region private 
        private SearchStatementModel PrepareSearchStatement(GridCommand command, DatFileSearchModel searchModel)
        {
            string whereStatement = string.Empty;
            IList<object> param = new List<object>();

            HqlStatementHelper.AddLikeStatement("ASNNo", searchModel.AsnNo, HqlStatementHelper.LikeMatchMode.Anywhere, "l", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("Type", "MIGO", HqlStatementHelper.LikeMatchMode.Start, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("MoveType", searchModel.MoveType, "l", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("ExtNo", searchModel.ExtNo, HqlStatementHelper.LikeMatchMode.Anywhere, "l", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("PoLine", searchModel.PoLine, HqlStatementHelper.LikeMatchMode.Anywhere, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("HandResult", searchModel.HandResult, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("IsCreateDat", searchModel.IsCreateDat, "l", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("WMSNo", searchModel.WmsNo, "l", ref whereStatement, param);

            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddBetweenStatement("HandTime", searchModel.StartDate.Value.ToString("yyMMddHHmmss"), searchModel.EndDate.Value.ToString("yyMMddHHmmss"), "l", ref whereStatement, param);
            }
            else if (searchModel.StartDate != null & searchModel.EndDate == null)
            {
                HqlStatementHelper.AddGeStatement("HandTime", searchModel.StartDate.Value.ToString("yyMMddHHmmss"), "l", ref whereStatement, param);
            }
            else if (searchModel.StartDate == null & searchModel.EndDate != null)
            {
                HqlStatementHelper.AddLeStatement("HandTime", searchModel.EndDate.Value.ToString("yyMMddHHmmss"), "l", ref whereStatement, param);
            }

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
           
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by FileName desc";
            }
            else
            {
                sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            }
            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = "select count(*) from LesINLog as l";
            searchStatementModel.SelectStatement = "select l from LesINLog as l";
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();
            return searchStatementModel;
        }


        private StringBuilder StringBuilderPrepareSearchStatement(GridCommand command, DatFileSearchModel searchModel)
        {
            StringBuilder sb = new StringBuilder();
           // sb.Append("select * from (");
            if (command.SortDescriptors.Count > 0)
            {
                sb.Append(string.Format("select RowId=ROW_NUMBER()OVER( order by {0} desc) ,* from ", command.SortDescriptors[0].Member));
            }
            else
            {
                sb.Append("select RowId=ROW_NUMBER()OVER( order by FileName desc),* from ");
            }
            sb.Append(@"(select
lesInlog.Id, lesInlog.Type, lesInlog.MoveType, lesInlog.Sequense, lesInlog.PO, lesInlog.POLine, lesInlog.WMSNo, lesInlog.WMSLine, lesInlog.HandTime, lesInlog.Item, lesInlog.HandResult, lesInlog.ErrorCause, lesInlog.IsCreateDat, lesInlog.FileName, lesInlog.ASNNo, lesInlog.ExtNo
,ipdet.Qty as ShipQty,IpDet.LocTo as LocTo,recDet.RecQty as ReceivedQty
from FIS_LesINLog as lesInlog with(NOLOCK) left join ORD_IpDet_8 as ipDet with(NOLOCK)
on lesInlog.ASNNo=ipDet.IpNo and lesInlog.ExtNo=ipDet.ExtNo and lesInlog.POLine=ipDet.ExtSeq
left join ORD_RecDet_8 as recDet with(NOLOCK) on lesInlog.PO=recDet.RecNo where lesInlog.Type='MIGO'");

            if (!string.IsNullOrEmpty(searchModel.AsnNo))
            {
                sb.Append(string.Format(" and lesInlog.ASNNo='{0}'",searchModel.AsnNo));
            }
            if (!string.IsNullOrEmpty(searchModel.MoveType))
            {
                sb.Append(string.Format(" and lesInlog.MoveType='{0}'", searchModel.MoveType));
            }
            if (!string.IsNullOrEmpty(searchModel.ExtNo))
            {
                sb.Append(string.Format(" and lesInlog.ExtNo='{0}'", searchModel.ExtNo));
            }
            if (!string.IsNullOrEmpty(searchModel.PoLine))
            {
                sb.Append(string.Format(" and lesInlog.PoLine='{0}'", searchModel.PoLine));
            }
            if (!string.IsNullOrEmpty(searchModel.HandResult))
            {
                sb.Append(string.Format(" and lesInlog.HandResult='{0}'", searchModel.HandResult));
            }
            if (!string.IsNullOrEmpty(searchModel.Item))
            {
                sb.Append(string.Format(" and lesInlog.Item='{0}'", searchModel.Item));
            }
            if (!string.IsNullOrEmpty(searchModel.WmsNo))
            {
                sb.Append(string.Format(" and lesInlog.WMSNo='{0}'", searchModel.WmsNo));
            }
            if (searchModel.IsCreateDat!=null)
            {
                sb.Append(string.Format(" and lesInlog.IsCreateDat='{0}'", searchModel.IsCreateDat));
            }

            if (searchModel.StartDate != null & searchModel.EndDate != null)
            {
                sb.Append(string.Format(" and  lesInlog.HandTime between '{0}' and '{1}'", searchModel.StartDate.Value, searchModel.EndDate.Value));
            }
            else if (searchModel.StartDate != null & searchModel.EndDate == null)
            {
                sb.Append(string.Format(" and lesInlog.HandTime > '{0}'", searchModel.StartDate.Value));
            }
            else if (searchModel.StartDate == null & searchModel.EndDate != null)
            {
                sb.Append(string.Format(" and lesInlog.HandTime < '{0}'", searchModel.EndDate.Value));
            }
           // sb.Append(string.Format( ") as t1 where t1.RowId between {0} and {1}",(command.Page-1)*command.PageSize,command.Page*command.PageSize));
           
            sb.Append(") as t1");
            return sb;
        }
        #endregion

   
    }
}
