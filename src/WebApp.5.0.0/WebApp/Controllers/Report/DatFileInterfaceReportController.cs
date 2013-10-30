using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using com.Sconit.Web.Util;
using com.Sconit.Web.Models;
using com.Sconit.Entity.SCM;
using com.Sconit.Service;
using com.Sconit.Web.Models.SearchModels.SCM;
using com.Sconit.Web.Models.SearchModels.ORD;
using System.Text;
using com.Sconit.Entity.Exception;
using System.Data.SqlClient;
using System.Data;

namespace com.Sconit.Web.Controllers.Report
{
    public class DatFileInterfaceReportController : WebAppBaseController
    {
        #region 计划协议接口查询
        [SconitAuthorize(Permissions = "Url_Report_GetIpDatInfo")]
        public ActionResult IpIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Report_GetIpDatInfo")]
        public JsonResult _GetIpDatInfo(DatFileSearchModel searchModel)
        {
            try
            {
                if (!this.CheckSearchModelIsNull(searchModel))
                {
                    throw new BusinessException("请选择查询条件。");
                }
                // ReportSearchStatementModel reportSearchStatementModel = PrepareSearchStatement( searchModel);
                //GridModel<object[]> gridModel = GetReportAjaxPageData<object[]>(reportSearchStatementModel);
                // StringBuilder stringBd = IpDatInfoStringBuilder(gridModel.Data);
                string sql = this.GetSearchIpDatInfoSql(searchModel);
                IList<object[]> objList = base.genericMgr.FindAllWithNativeSql<object[]>(sql);
                return Json(new { Info = IpDatInfoStringBuilder(objList).ToString() });
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }
            return Json(null);
        }

        private string GetSearchIpDatInfoSql(DatFileSearchModel searchModel)
        {
            string IpDetailSql = "select * from ORD_IpDet_8 WHERE 1=1 ";
            string IpMasterSql = "select * from ORD_IpMstr_8 WHERE 1=1 ";
            string CreateIpDATSql = "select * from FIS_CreateIpDAT WHERE 1=1 ";
            string LesInLogSql = "select * from FIS_LesINLog WHERE 1=1 ";
            //string LocTransSql = "select * from VIEW_LocTrans WHERE 1=1 ";
            //string InvLocSql = "select * from sconit5_si.dbo.SI_SAP_InvLoc WHERE 1=1";
            //string InvTransSql = "select * from sconit5_si.dbo.SI_SAP_InvTrans WHERE 1=1";

            string IpMasterWhere = " and 1=1";
            string IpDetailWhere = " and 1=1";
            string LesInLogWhere = " and 1=1";

            if (!string.IsNullOrEmpty(searchModel.Supplier))
            {
                IpMasterSql += " AND PartyFrom LIKE '%" + searchModel.Supplier + "%'";
                IpMasterWhere += "  AND im.PartyFrom LIKE '%" + searchModel.Supplier + "%'";
            }
            if (!string.IsNullOrEmpty(searchModel.IpNo))
            {
                IpMasterSql += " AND IpNo LIKE  '%" + searchModel.IpNo + "%'";
                IpMasterWhere += "  AND im.IpNo LIKE  '%" + searchModel.IpNo + "%'";

                IpDetailSql += " AND IpNo LIKE  '%" + searchModel.IpNo + "%'";
                IpDetailWhere += "  AND im.IpNo LIKE  '%" + searchModel.IpNo + "%'";
            }
            if (!string.IsNullOrEmpty(searchModel.Location))
            {
                IpDetailSql += " AND LocTo LIKE  '%" + searchModel.Location + "%'";
                IpDetailWhere += " AND ipdet.LocTo LIKE  '%" + searchModel.Location + "%'";
            }
            if (!string.IsNullOrEmpty(searchModel.Item))
            {
                IpDetailSql += " AND Item LIKE  '%" + searchModel.Item + "%'";
                IpDetailWhere += " AND ipdet.Item LIKE  '%" + searchModel.Item + "%'";
            }
            if (!string.IsNullOrEmpty(searchModel.WmsNo))
            {
                LesInLogSql += " AND WmsNo LIKE  '%" + searchModel.WmsNo + "%'";
                LesInLogWhere += " AND lesLog.WmsNo LIKE  '%" + searchModel.WmsNo + "%'";
            }
            if (!string.IsNullOrEmpty(searchModel.HandResult))
            {
                LesInLogSql += " AND HandResult='" + searchModel.HandResult + "'";
                LesInLogWhere += " AND lesLog.HandResult='" + searchModel.HandResult + "'";
            }
            if (!string.IsNullOrEmpty(searchModel.MoveType))
            {
                LesInLogSql += " AND MoveType='" + searchModel.MoveType + "'";
                LesInLogWhere += " AND lesLog.MoveType='" + searchModel.MoveType + "'";
                //SET @LesInLogSql=@LesInLogSql+' AND MoveType = @MoveType_1'
            }
            if (searchModel.IsCs != null)
            {
                if (searchModel.IsCs == 1)
                {
                    IpDetailSql += " AND BillTerm=3";
                    IpDetailWhere += " AND ipdet.BillTerm =3";
                }
                else
                {
                    IpDetailSql += " AND BillTerm<>3";
                    IpDetailWhere += " AND ipdet.BillTerm <>3";
                }
            }
            if (searchModel.StartDate != null && searchModel.EndDate != null)
            {
                IpDetailSql += " AND CreateDate BETWEEN '" + searchModel.StartDate + "' And '" + searchModel.EndDate + "' ";
                IpDetailWhere += " AND ipdet.CreateDate BETWEEN '" + searchModel.StartDate + "' And '" + searchModel.EndDate + "' ";
            }
            if (searchModel.StartDate != null && searchModel.EndDate == null)
            {
                IpDetailSql += " AND CreateDate > '" + searchModel.StartDate + "' ";
                IpDetailWhere += " AND ipdet.CreateDate > '" + searchModel.StartDate + "' ";

            }
            if (searchModel.StartDate == null && searchModel.EndDate != null)
            {
                IpDetailSql += " AND CreateDate < '" + searchModel.EndDate + "' ";
                IpDetailWhere += " AND ipdet.CreateDate < '" + searchModel.EndDate + "' ";
            }

            string AllSelectSql = "select top(500) im.PartyFrom,ipdet.IpNo,ipdet.OrderNo,ipdet.Item,ipdet.ItemDesc,ipdet.RefItemCode,ipdet.Qty,ipdet.RecQty,"
    + "IpDat.CreateUserNm,IpDat.IsCreateDat,IpDat.TIME_STAMP1,IpDat.FileName, "
    + "lesLog.MoveType,lesLog.WMSNo,lesLog.HandTime,lesLog.HandResult,lesLog.ErrorCause,lesLog.IsCreateDat as isCreateLogDat,lesLog.FileName as logfilename"
                //+ ",viewLocTr.IsCS	,viewLocTr.TransType"
                //+ ",siInvLoc.SourceId,siInvTrans.Status,siInvTrans.ErrorMessage"
+ "	from (" + IpDetailSql + ") as ipdet "
+ " left join (" + IpMasterSql + ") as im on im.IpNo=ipdet.IpNo  "
+ " left join (" + CreateIpDATSql + ") as IpDat on ipdet.IpNo=IpDat.ASN_NO and ipdet.Seq=IpDat.ASN_ITEM "
+ " left join (" + LesInLogSql + ") as lesLog on ipdet.IpNo=lesLog.ASNNo and ipdet.Item=lesLog.Item	and ipdet.ExtNo=lesLog.ExtNo and ipdet.ExtSeq=lesLog.POLine"
                //+ " left join (" + LocTransSql + ") as viewLocTr on ipdet.Id=viewLocTr.IpDetId"
                //+ " left join (" + InvLocSql + ") as siInvLoc on viewLocTr.Id=siInvLoc.SourceId"
                //+ " left join (" + @InvTransSql + ") as siInvTrans on siInvLoc.FRBNR=siInvTrans.FRBNR and siInvLoc.SGTXT=siInvTrans.SGTXT"
+ "  where 1=1 " + IpMasterWhere + " " + IpDetailWhere + " " + LesInLogWhere;
            return AllSelectSql;
        }

        private ReportSearchStatementModel PrepareSearchStatement(DatFileSearchModel searchModel)
        {

            ReportSearchStatementModel reportSearchStatementModel = new ReportSearchStatementModel();
            reportSearchStatementModel.ProcedureName = "USP_Report_GetFISDatInfo";


            /*
             * @IpNo varchar(50),
    @Supplier varchar(50),
    @Location varchar(50),
    @Item varchar(50),
    @WmsNo varchar(50),
    @HandResult varchar(50),
    @MoveType varchar(50),
    --@SapLocation varchar(50),
    @IsCs varchar(50),
    @StartDate datetime, 
    @EndDate datetime
             * */

            SqlParameter[] parameters = new SqlParameter[10];

            parameters[0] = new SqlParameter("@IpNo", SqlDbType.VarChar, 50);
            parameters[0].Value = searchModel.IpNo;

            parameters[1] = new SqlParameter("@Supplier", SqlDbType.VarChar, 50);
            parameters[1].Value = searchModel.Supplier;

            parameters[2] = new SqlParameter("@Location", SqlDbType.VarChar, 50);
            parameters[2].Value = searchModel.Location;

            parameters[3] = new SqlParameter("@Item", SqlDbType.VarChar, 50);
            parameters[3].Value = searchModel.Item;

            parameters[4] = new SqlParameter("@WmsNo", SqlDbType.VarChar, 50);
            parameters[4].Value = searchModel.WmsNo;

            parameters[5] = new SqlParameter("@HandResult", SqlDbType.VarChar, 50);
            parameters[5].Value = searchModel.HandResult;

            parameters[6] = new SqlParameter("@MoveType", SqlDbType.VarChar, 50);
            parameters[6].Value = searchModel.MoveType;

            parameters[7] = new SqlParameter("@IsCs", SqlDbType.VarChar, 50);
            parameters[7].Value = searchModel.IsCs;

            parameters[8] = new SqlParameter("@StartDate", SqlDbType.DateTime);
            parameters[8].Value = searchModel.StartDate;

            parameters[9] = new SqlParameter("@EndDate", SqlDbType.DateTime);
            parameters[9].Value = searchModel.EndDate;


            reportSearchStatementModel.Parameters = parameters;

            return reportSearchStatementModel;
        }

        private StringBuilder IpDatInfoStringBuilder(IList<object[]> objList)
        {
            StringBuilder str = new StringBuilder("<table text-align=\"center\"  cellpadding=\"0\" cellspacing=\"0\" border=\"0\" class=\"display\" id=\"datatable\" width=\"100%\"><thead><tr>");

            #region Head
            #region 第一行
            str.Append("<th colspan='8'  style='border:1px solid #999999;text-align:center'>");
            str.Append("送货单信息");
            str.Append("</th>");

            str.Append("<th colspan='4'  style='border:1px solid #999999;text-align:center'>");
            str.Append("传给WmsDat文件信息");
            str.Append("</th>");

            str.Append("<th colspan='7'  style='border:1px solid #999999;text-align:center'>");
            str.Append("处理WMS Dat信息");
            str.Append("</th>");

            //str.Append("<th colspan='3'  style='border:1px solid #999999;text-align:center'>");
            //str.Append("库存事务信息");
            //str.Append("</th>");

            //str.Append("<th colspan='2'  style='border:1px solid #999999;text-align:center'>");
            //str.Append("事务传送Sap信息");
            //str.Append("</th>");
            #endregion

            #region 送货单信息
            str.Append("</tr><tr><th style='border:1px solid #999999'>");
            str.Append("供应商");
            str.Append("</th>");

            str.Append("<th style='border:1px solid #999999'>");
            str.Append(Resources.ORD.IpDetail.IpDetail_IpNo);
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append(Resources.ORD.IpDetail.IpDetail_OrderNo);
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append(Resources.ORD.IpDetail.IpDetail_Item);
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append(Resources.ORD.IpDetail.IpDetail_ItemDescription);
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append(Resources.ORD.IpDetail.IpDetail_RefItemCode);
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append(Resources.ORD.IpDetail.IpDetail_Qty);
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append(Resources.ORD.IpDetail.IpDetail_ReceivedQty);
            str.Append("</th>");
            #endregion

            #region Dat文件信息
            str.Append("<th style='border:1px solid #999999'>");
            str.Append("创建人");
            str.Append("</th>");

            str.Append("<th style='border:1px solid #999999'>");
            str.Append("是否传给Wms");
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append("传送时间");
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append("Dat文件名");
            str.Append("</th>");
            #endregion

            #region 处理WMSDat信息
            str.Append("<th style='border:1px solid #999999'>");
            str.Append("移动类型");
            str.Append("</th>");

            str.Append("<th style='border:1px solid #999999'>");
            str.Append("Wms号");
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append("处理时间");
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append("处理结果");
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append("错误原因");
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append("是否已反馈给Wms");
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append("Log文件名");
            str.Append("</th>");
            #endregion

            #region 库存事务信息
            //str.Append("<th style='border:1px solid #999999'>");
            //str.Append("是否寄售");
            //str.Append("</th>");

            //str.Append("<th style='border:1px solid #999999'>");
            //str.Append("库存事务代码");
            //str.Append("</th>");
            //str.Append("<th style='border:1px solid #999999'>");
            //str.Append("库存事务描述");
            //str.Append("</th>");
            #endregion

            #region 事务传送Sap信息

            //str.Append("<th style='border:1px solid #999999'>");
            //str.Append("传送状态");
            //str.Append("</th>");
            //str.Append("<th style='border:1px solid #999999'>");
            //str.Append("失败原因");
            //str.Append("</th>");
            #endregion

            #endregion
            str.Append("</tr></thead><tbody>");


            if (objList != null && objList.Count > 0)
            {
                int rowIndex = 0;
                foreach (object[] row in objList)
                {
                    rowIndex++;
                    if (rowIndex % 2 == 0)
                    {
                        str.Append("<tr>");
                    }
                    else
                    {
                        str.Append("<tr style='color:#5555DD'>");
                    }
                    for (int i = 0; i < row.Length; i++)
                    {
                        #region
                        if (i == 6 || i == 7)
                        {
                            str.Append("<td style='border:1px solid #999999'>");
                            str.Append(((decimal)row[i]).ToString("0.00"));
                            str.Append("</td>");
                            continue;
                        }
                        #endregion

                        #region 是否传数据给Wms
                        if (i == 9)
                        {
                            if (row[9] != null)
                            {
                                if ((bool)row[9])
                                {
                                    str.Append("<td style='border:1px solid #999999'>");
                                    str.Append("是");
                                    str.Append("</td>");
                                    continue;
                                }
                                if (!(bool)row[9])
                                {
                                    str.Append("<td style='border:1px solid #999999'>");
                                    str.Append("否");
                                    str.Append("</td>");
                                    continue;
                                }
                            }
                        }
                        #endregion
                        #region 是否反馈给安吉
                        if (i == 17)
                        {
                            if (row[17] != null)
                            {
                                if ((bool)row[17])
                                {
                                    str.Append("<td style='border:1px solid #999999'>");
                                    str.Append("是");
                                    str.Append("</td>");
                                    continue;
                                }
                                if (!(bool)row[17])
                                {
                                    str.Append("<td style='border:1px solid #999999'>");
                                    str.Append("否");
                                    str.Append("</td>");
                                    continue;
                                }
                            }
                        }
                        #endregion
                        #region 是否寄售
                        //if (i == 19)
                        //{
                        //    if (row[19] != null)
                        //    {
                        //        if ((bool)row[19])
                        //        {
                        //            str.Append("<td style='border:1px solid #999999'>");
                        //            str.Append("是");
                        //            str.Append("</td>");
                        //            continue;
                        //        }
                        //        if (!(bool)row[19])
                        //        {
                        //            str.Append("<td style='border:1px solid #999999'>");
                        //            str.Append("否");
                        //            str.Append("</td>");
                        //            continue;
                        //        }
                        //    }
                        //}
                        #endregion
                        #region 事务是否传给Sap
                        //if (i == 21)
                        //{
                        //    if (row[20] != null)
                        //    {
                        //        str.Append("<td style='border:1px solid #999999'>");
                        //        str.Append(this.systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.TransactionType, Convert.ToInt32(row[20])));
                        //        str.Append("</td>");
                        //        continue;
                        //    }
                        //}
                        #endregion
                        #region 事务传给Sap状态
                        //if (i == 22)
                        //{
                        //    if (row[22] != null)
                        //    {
                        //        if (row[i].ToString() == "1")
                        //        {
                        //            str.Append("<td style='border:1px solid #999999'>");
                        //            str.Append("成功");
                        //            str.Append("</td>");
                        //            continue;
                        //        }
                        //    }
                        //}
                        #endregion
                        str.Append("<td style='border:1px solid #999999'>");
                        str.Append(row[i]);
                        str.Append("</td>");
                    }
                    str.Append("</tr>");
                }
            }
            else
            {
                str.Append("<tr>");

                str.Append("<td colspan='30' style='border:1px solid #999999'>");
                str.Append("没有符合条件的记录。");
                str.Append("</td>");

                str.Append("</tr>");
            }

            //表尾
            str.Append("</tbody>");
            str.Append("</table>");
            return str;
        }
        #endregion

        #region  安吉移库接口查询
        [SconitAuthorize(Permissions = "Url_Report_GetOrderDatInfo")]
        public ActionResult OrderIndex()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_Report_GetOrderDatInfo")]
        public JsonResult _GetOrderDatInfo(DatFileSearchModel searchModel)
        {
            try
            {
                if (!this.CheckSearchModelIsNull(searchModel))
                {
                    throw new BusinessException("请选择查询条件。");
                }
                string sql = this.GetSearchOrderDatInfoSql(searchModel);
                IList<object[]> objList = base.genericMgr.FindAllWithNativeSql<object[]>(sql);
                return Json(new { Info = OrderDatInfoStringBuilder(objList).ToString() });
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
            }

            return Json(null);
        }

        private string GetSearchOrderDatInfoSql(DatFileSearchModel searchModel)
        {
            string OrderDetailSql = "select * from ORD_OrderDet_2 WHERE 1=1 and LocFrom like 'LOC%' or LocFrom like 'SQCK%'  ";
            string CreateOrderDATSql = "select * from FIS_CreateOrderDAT WHERE 1=1 ";
            string WMSDatFileSql = "select * from FIS_WMSDatFile WHERE 1=1 ";
            string LesInLogSql = "select * from FIS_LesINLog WHERE 1=1 ";
            //string LocTransSql = "select * from VIEW_LocTrans WHERE 1=1 ";
            //string InvLocSql = "select * from sconit5_si.dbo.SI_SAP_InvLoc WHERE 1=1";
            //string InvTransSql = "select * from sconit5_si.dbo.SI_SAP_InvTrans WHERE 1=1";

            string OrderDetailWhere = " and 1=1";
            string LesInLogWhere = " and 1=1";
            string WMSDatFileWhere = " and 1=1";
            string LocTransWhere = " and 1=1";

            if (!string.IsNullOrEmpty(searchModel.Supplier))
            {
                OrderDetailSql += " AND ManufactureParty LIKE '%" + searchModel.Supplier + "%'";
                OrderDetailWhere += "  AND od.ManufactureParty LIKE '%" + searchModel.Supplier + "%'";
            }
            if (!string.IsNullOrEmpty(searchModel.OrderNo))
            {
                OrderDetailSql += " AND OrderNo LIKE  '%" + searchModel.OrderNo + "%'";
                OrderDetailWhere += "  AND od.OrderNo LIKE  '%" + searchModel.OrderNo + "%'";
            }
            if (!string.IsNullOrEmpty(searchModel.Location))
            {
                OrderDetailSql += " AND LocTo LIKE  '%" + searchModel.Location + "%'";
                OrderDetailWhere += " AND od.LocTo LIKE  '%" + searchModel.Location + "%'";
            }
            if (!string.IsNullOrEmpty(searchModel.Item))
            {
                OrderDetailSql += " AND Item LIKE  '%" + searchModel.Item + "%'";
                OrderDetailWhere += " AND od.Item LIKE  '%" + searchModel.Item + "%'";
            }
            if (!string.IsNullOrEmpty(searchModel.WmsNo))
            {
                WMSDatFileSql += " AND WMSId LIKE  '%" + searchModel.WmsNo + "%'";
                WMSDatFileWhere += " AND WMSDat.WMSId LIKE  '%" + searchModel.WmsNo + "%'";
            }
            if (!string.IsNullOrEmpty(searchModel.WmsPickNo))//安吉拣货单号
            {
                WMSDatFileSql += " AND WmsNo LIKE  '%" + searchModel.WmsPickNo + "%'";
                WMSDatFileWhere += " AND WMSDat.WmsNo LIKE  '%" + searchModel.WmsPickNo + "%'";
            }
            if (!string.IsNullOrEmpty(searchModel.HandResult))
            {
                LesInLogSql += " AND HandResult='" + searchModel.HandResult + "'";
                LesInLogWhere += " AND lesLog.HandResult='" + searchModel.HandResult + "'";
            }
            if (!string.IsNullOrEmpty(searchModel.MoveType))
            {
                LesInLogSql += " AND MoveType='" + searchModel.MoveType + "'";
                LesInLogWhere += " AND lesLog.MoveType='" + searchModel.MoveType + "'";
                //SET @LesInLogSql=@LesInLogSql+' AND MoveType = @MoveType_1'
            }
            //if (searchModel.IsCs != null)
            //{

            //    LocTransSql += " AND IsCS='" + searchModel.IsCs + "'";
            //    LocTransWhere += " AND lesLog.IsCS='" + searchModel.IsCs + "'";
            //}
            if (searchModel.StartDate != null && searchModel.EndDate != null)
            {
                OrderDetailSql += " AND CreateDate BETWEEN '" + searchModel.StartDate + "' And '" + searchModel.EndDate + "' ";
                OrderDetailWhere += " AND od.CreateDate BETWEEN '" + searchModel.StartDate + "' And '" + searchModel.EndDate + "' ";
            }
            if (searchModel.StartDate != null && searchModel.EndDate == null)
            {
                OrderDetailSql += " AND CreateDate > '" + searchModel.StartDate + "' ";
                OrderDetailWhere += " AND od.CreateDate > '" + searchModel.StartDate + "' ";

            }
            if (searchModel.StartDate == null && searchModel.EndDate != null)
            {
                OrderDetailSql += " AND CreateDate < '" + searchModel.EndDate + "' ";
                OrderDetailWhere += " AND od.CreateDate < '" + searchModel.EndDate + "' ";
            }

            string AllSelectSql = "select top(500) od.ManufactureParty,od.OrderNo,od.Item,od.ItemDesc,od.RefItemCode,od.OrderQty,od.RecQty,"
+ "orderDat.CreateUserNm,orderDat.IsCreateDat,orderDat.TIME_STAMP1,orderDat.FileName,"
+ "WMSDat.CreateDate,WMSDat.Qty, WMSDat.IsHand,WMSDat.WMSId,WMSDat.FileName as wmsDatFileName,"
+ "lesLog.MoveType,lesLog.HandTime,lesLog.HandResult,lesLog.ErrorCause,lesLog.IsCreateDat as isCreateLogDat,lesLog.FileName as logfilename"
                //+ "viewLocTr.IsCS	,viewLocTr.TransType,"
                //+ "siInvLoc.SourceId,siInvTrans.Status,siInvTrans.ErrorMessage"
+ " from (" + OrderDetailSql + ") as od  "
+ " left join (" + CreateOrderDATSql + ") as orderDat on od.Id=orderDat.ZPLISTNO"
+ " left join (" + WMSDatFileSql + ")  as WMSDat on od.Id=WMSDat.WmsLine "
+ " left join (" + LesInLogSql + ") as lesLog on WMSDat.WMSId=lesLog.WMSNo"
                //+ " left join (" + LocTransSql + ")  as viewLocTr on od.Id=viewLocTr.OrderDetId"
                //+ " left join (" + InvLocSql + ") as siInvLoc on viewLocTr.Id=siInvLoc.SourceId "
                //+ " left join (" + InvTransSql + ") as siInvTrans on siInvLoc.FRBNR=siInvTrans.FRBNR and siInvLoc.SGTXT=siInvTrans.SGTXT"
+ " where 1=1 " + OrderDetailWhere + LesInLogWhere + LocTransWhere + WMSDatFileWhere;
            return AllSelectSql;
        }

        private StringBuilder OrderDatInfoStringBuilder(IList<object[]> objList)
        {
            StringBuilder str = new StringBuilder("<table text-align=\"center\"  cellpadding=\"0\" cellspacing=\"0\" border=\"0\" class=\"display\" id=\"datatable\" width=\"100%\"><thead><tr>");

            #region Head
            #region 第一行
            str.Append("<th colspan='7'  style='border:1px solid #999999;text-align:center'>");
            str.Append("要货单信息");
            str.Append("</th>");

            str.Append("<th colspan='4'  style='border:1px solid #999999;text-align:center'>");
            str.Append("传给WmsDat文件信息");
            str.Append("</th>");

            str.Append("<th colspan='5'  style='border:1px solid #999999;text-align:center'>");
            str.Append("读取WMS Dat信息");
            str.Append("</th>");

            str.Append("<th colspan='6'  style='border:1px solid #999999;text-align:center'>");
            str.Append("处理WMS Dat信息");
            str.Append("</th>");

            //str.Append("<th colspan='3'  style='border:1px solid #999999;text-align:center'>");
            //str.Append("库存事务信息");
            //str.Append("</th>");

            //str.Append("<th colspan='2'  style='border:1px solid #999999;text-align:center'>");
            //str.Append("事务传送Sap信息");
            //str.Append("</th>");
            #endregion

            #region 送货单信息
            str.Append("</tr><tr><th style='border:1px solid #999999'>");
            str.Append(Resources.ORD.OrderDetail.OrderDetail_ManufactureParty);
            str.Append("</th>");

            str.Append("<th style='border:1px solid #999999'>");
            str.Append(Resources.ORD.OrderDetail.OrderDetail_OrderNo);
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append(Resources.ORD.OrderDetail.OrderDetail_Item);
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append(Resources.ORD.OrderDetail.OrderDetail_ItemDescription);
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append(Resources.ORD.OrderDetail.OrderDetail_ReferenceItemCode);
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append(Resources.ORD.OrderDetail.OrderDetail_OrderedQty);
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append(Resources.ORD.OrderDetail.OrderDetail_ReceivedQty);
            str.Append("</th>");
            #endregion

            #region Dat文件信息
            str.Append("<th style='border:1px solid #999999'>");
            str.Append("创建人");
            str.Append("</th>");

            str.Append("<th style='border:1px solid #999999'>");
            str.Append("是否传给Wms");
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append("传送时间");
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append("Dat文件名");
            str.Append("</th>");
            #endregion

            #region 读取WMS Dat信息
            str.Append("<th style='border:1px solid #999999'>");
            str.Append("创建时间");
            str.Append("</th>");

            str.Append("<th style='border:1px solid #999999'>");
            str.Append("发货数");
            str.Append("</th>");

            str.Append("<th style='border:1px solid #999999'>");
            str.Append("已经处理");
            str.Append("</th>");

            str.Append("<th style='border:1px solid #999999'>");
            str.Append("Wms唯一标识");
            str.Append("</th>");

            str.Append("<th style='border:1px solid #999999'>");
            str.Append("WmsDat文件名");
            str.Append("</th>");
            #endregion

            #region 处理WMSDat信息
            str.Append("<th style='border:1px solid #999999'>");
            str.Append("移动类型");
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append("处理时间");
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append("处理结果");
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append("错误原因");
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append("是否已反馈给Wms");
            str.Append("</th>");
            str.Append("<th style='border:1px solid #999999'>");
            str.Append("Log文件名");
            str.Append("</th>");
            #endregion

            #region 库存事务信息
            //str.Append("<th style='border:1px solid #999999'>");
            //str.Append("是否寄售");
            //str.Append("</th>");

            //str.Append("<th style='border:1px solid #999999'>");
            //str.Append("库存事务代码");
            //str.Append("</th>");
            //str.Append("<th style='border:1px solid #999999'>");
            //str.Append("库存事务描述");
            //str.Append("</th>");
            #endregion

            #region 事务传送Sap信息

            //str.Append("<th style='border:1px solid #999999'>");
            //str.Append("传送状态");
            //str.Append("</th>");
            //str.Append("<th style='border:1px solid #999999'>");
            //str.Append("失败原因");
            //str.Append("</th>");
            #endregion

            #endregion
            str.Append("</tr></thead><tbody>");

            #region body
            if (objList != null && objList.Count > 0)
            {
                int rowIndex = 0;
                foreach (object[] row in objList)
                {
                    rowIndex++;
                    if (rowIndex % 2 == 0)
                    {
                        str.Append("<tr>");
                    }
                    else
                    {
                        str.Append("<tr style='color:#5555DD'>");
                    }
                    for (int i = 0; i < row.Length; i++)
                    {
                        #region
                        if (i == 6 || i == 5 || i == 12)
                        {
                            if (row[i] != null)
                            {
                                str.Append("<td style='border:1px solid #999999'>");
                                str.Append(((decimal)row[i]).ToString("0.00"));
                                str.Append("</td>");
                                continue;
                            }
                        }
                        #endregion

                        #region 是否传数据给Wms
                        if (i == 8)
                        {
                            if (row[8] != null)
                            {
                                if ((bool)row[8])
                                {
                                    str.Append("<td style='border:1px solid #999999'>");
                                    str.Append("是");
                                    str.Append("</td>");
                                    continue;
                                }
                                if (!(bool)row[8])
                                {
                                    str.Append("<td style='border:1px solid #999999'>");
                                    str.Append("否");
                                    str.Append("</td>");
                                    continue;
                                }
                            }
                        }
                        #endregion
                        #region 是否已经处理
                        if (i == 13)
                        {
                            if (row[i] != null)
                            {
                                if ((bool)row[i])
                                {
                                    str.Append("<td style='border:1px solid #999999'>");
                                    str.Append("是");
                                    str.Append("</td>");
                                    continue;
                                }
                                if (!(bool)row[i])
                                {
                                    str.Append("<td style='border:1px solid #999999'>");
                                    str.Append("否");
                                    str.Append("</td>");
                                    continue;
                                }
                            }
                        }
                        #endregion
                        #region 是否反馈给安吉
                        if (i == 20)
                        {
                            if (row[20] != null)
                            {
                                if ((bool)row[20])
                                {
                                    str.Append("<td style='border:1px solid #999999'>");
                                    str.Append("是");
                                    str.Append("</td>");
                                    continue;
                                }
                                if (!(bool)row[20])
                                {
                                    str.Append("<td style='border:1px solid #999999'>");
                                    str.Append("否");
                                    str.Append("</td>");
                                    continue;
                                }
                            }
                        }
                        #endregion
                        #region 是否寄售
                        //if (i == 22)
                        //{
                        //    if (row[22] != null)
                        //    {
                        //        if ((bool)row[22])
                        //        {
                        //            str.Append("<td style='border:1px solid #999999'>");
                        //            str.Append("是");
                        //            str.Append("</td>");
                        //            continue;
                        //        }
                        //        if (!(bool)row[22])
                        //        {
                        //            str.Append("<td style='border:1px solid #999999'>");
                        //            str.Append("否");
                        //            str.Append("</td>");
                        //            continue;
                        //        }
                        //    }
                        //}
                        #endregion
                        #region 事务是否传给Sap
                        //if (i == 24)
                        //{
                        //    if (row[23] != null)
                        //    {
                        //        str.Append("<td style='border:1px solid #999999'>");
                        //        str.Append(this.systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.TransactionType, Convert.ToInt32(row[23])));
                        //        str.Append("</td>");
                        //        continue;
                        //    }
                        //}
                        #endregion
                        #region 事务传给Sap状态
                        //if (i == 25)
                        //{
                        //    if (row[25] != null)
                        //    {
                        //        if (row[i].ToString() == "1")
                        //        {
                        //            str.Append("<td style='border:1px solid #999999'>");
                        //            str.Append("成功");
                        //            str.Append("</td>");
                        //            continue;
                        //        }
                        //    }
                        //}
                        #endregion
                        str.Append("<td style='border:1px solid #999999'>");
                        str.Append(row[i]);
                        str.Append("</td>");
                    }
                    str.Append("</tr>");
                }
            }
            else
            {
                str.Append("<tr>");

                str.Append("<td colspan='30' style='border:1px solid #999999'>");
                str.Append("没有符合条件的记录。");
                str.Append("</td>");

                str.Append("</tr>");
            }
            #endregion

            //表尾
            str.Append("</tbody>");
            str.Append("</table>");
            return str;
        }
        #endregion
    }
}
