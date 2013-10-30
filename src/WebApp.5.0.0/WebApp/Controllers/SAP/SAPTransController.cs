using System.Data;
using System.Web.Mvc;
using com.Sconit.Web.Util;
using Telerik.Web.Mvc;
using com.Sconit.Web.Models.SearchModels.SI;
using System.Data.SqlClient;
using System;
using System.Linq;
using com.Sconit.Web.Models;
using System.Collections.Generic;
using com.Sconit.Entity.SAP.TRANS;
using com.Sconit.Service;
using System.ComponentModel;
using System.Reflection;
using com.Sconit.Web.Models.SearchModels.SI.SAP;
using com.Sconit.Service.SAP;
using com.Sconit.Entity.Exception;
using com.Sconit.Utility.Report;
using NHibernate.Type;

namespace com.Sconit.Web.Controllers.SAP
{
    public class SAPTransController : WebAppBaseController
    {
        //
        // GET: /SequenceOrder/

        public ITransMgr transMgr { get; set; }
        public IReportGen reportGen { get; set; }
        //public IGenericMgr genericMgr { get; set; }

        private static string selectCountStatement = "select count(*) from InvTrans as t";

        /// <summary>
        /// 
        /// </summary>
        private static string selectStatement = "select t from InvTrans as t";


        private static string InvLocSelectStatement = "select l from InvLoc as l";
        public SAPTransController()
        {

        }
        [SconitAuthorize(Permissions = "Url_SI_SAP_InvTrans_View")]
        public ActionResult SAPIndex()
        {
            return View();
        }
        [GridAction]
        [SconitAuthorize(Permissions = "Url_SI_SAP_InvTrans_View")]
        public ActionResult List(GridCommand command, InvTransSearchModel searchModel)
        {
            TempData["InvTransSearchModel"] = searchModel;
            ViewBag.PageSize = 50;
            return View();
        }

        [SconitAuthorize(Permissions = "Url_SI_SAP_InvTrans_View")]
        public ActionResult Index(SearchModel searchModel)
        {
            string sql = @"select top  " + MaxRowSize + @"  e.BatchNo, d.*,
                        a.OrderNo,a.Item,a.Uom,a.IsCS,a.PlanBill,a.ActBill,a.QualityType, 
                        a.Qty,b.Type,b.Status,a.TransType,b.ExtOrderNo,
                        a.PartyFrom,a.LocFrom,a.PartyTo,b.LocTo,e.*
                        from VIEW_LocTrans a
                        left join ORD_OrderMstr b on a.OrderNo = b.OrderNo
                        left join SAP_InvLoc c on c.SourceId = a.Id
                        left join SAP_InvTrans e on (e.FRBNR = c.FRBNR and e.SGTXT = c.SGTXT)
                        left join SAP_TransCallBack d on (d.FRBNR = c.FRBNR and d.SGTXT = c.SGTXT)
                        where d.Id>0 and c.SourceType = 0  and e.Status = @p0 and e.CreateDate > @p1 and e.CreateDate < @p2 ";

            TempData["SearchModel"] = searchModel;

            SqlParameter[] sqlParam = new SqlParameter[4];
            sqlParam[0] = new SqlParameter("@p0", searchModel.Status.HasValue ? searchModel.Status.Value : 2);

            if (searchModel.StartDate.HasValue)
            {
                sqlParam[1] = new SqlParameter("@p1", searchModel.StartDate);
            }
            else
            {
                sqlParam[1] = new SqlParameter("@p1", DateTime.Now.AddDays(-1));
            }

            if (searchModel.EndDate.HasValue)
            {
                sqlParam[2] = new SqlParameter("@p2", searchModel.EndDate);
            }
            else
            {
                sqlParam[2] = new SqlParameter("@p2", DateTime.Now);
            }

            if (searchModel.Id.HasValue)
            {
                sql += " and e.Id = @p3 ";
                sqlParam[3] = new SqlParameter("@p3", searchModel.Id);
            }

            sql += " order by d.Id desc ";

            DataSet entity = genericMgr.GetDatasetBySql(sql, sqlParam);

            ViewModel model = new ViewModel();
            model.Data = entity.Tables[0];
            model.Columns = IListHelper.GetColumns(entity.Tables[0]);
            return View(model);
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_SI_SAP_InvTrans_View")]
        public ActionResult _AjaxList(GridCommand command, InvTransSearchModel searchModel)
        {

            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            GridModel<InvTrans> gridlist = GetAjaxPageData<InvTrans>(searchStatementModel, command);
            TempData["InvTransSearchModel_1"] = searchModel;
            TempData["Total"] = gridlist.Total;
            foreach (InvTrans inv in gridlist.Data)
            {
                inv.StatusName = GetEnumDescription(inv.Status);
            }
            return View(gridlist);
            //return PartialView(GetAjaxPageData<ReceiptMaster>(searchStatementModel, command));
        }


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_SI_SAP_InvTrans_View")]
        public ActionResult _AjaxListInvLoc(string FRBNR, string SGTXT)
        {
            string hql = InvLocSelectStatement + " where l.FRBNR = ? and l.SGTXT=? ";
            IList<InvLoc> InvLocList = genericMgr.FindAll<InvLoc>(hql,new object[]{FRBNR,SGTXT},new IType[]{NHibernate.NHibernateUtil.String,NHibernate.NHibernateUtil.String});
            return View(new GridModel<InvLoc>(InvLocList));
        }

        [SconitAuthorize(Permissions = "Url_SI_SAP_InvTrans_Close")]
        public ActionResult CreateInvTrans(string FRBNRStr, string SGTXTStr)
        {
            //这里调用手工关闭的方法
            try
            {
                if (!string.IsNullOrWhiteSpace(FRBNRStr) && !string.IsNullOrWhiteSpace(SGTXTStr))
                {
                    IList<InvTrans> invTransList = new List<InvTrans>();
                    string[] FRBNRArr = FRBNRStr.Split(',');
                    string[] SGTXTArr = SGTXTStr.Split(',');
                    for (int i = 0; i < FRBNRArr.Length; i++)
                    {
                        var invTraces = this.genericMgr.FindAll<InvTrans>("select i from InvTrans as i where i.FRBNR=? and i.SGTXT=?", new object[] { FRBNRArr[i], SGTXTArr[i] });
                        invTransList.Add(invTraces.First());
                    }
                    //transMgr.ManuallyReExchangeMoveType(invTransList);
                    transMgr.ManuallyCloseMoveType(invTransList);
                }
                else
                {
                    transMgr.ReExchangeMoveType();
                }
                SaveSuccessMessage("手工关闭成功。");

            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.Message);
            }
            InvTransSearchModel searchModel = TempData["InvTransSearchModel"] != null ? TempData["InvTransSearchModel"] as InvTransSearchModel : new InvTransSearchModel();
            return RedirectToAction("List", searchModel);
        }

        public static string GetEnumDescription(object enumSubitem)
        {
            enumSubitem = (Enum)enumSubitem;
            string strValue = enumSubitem.ToString();

            FieldInfo fieldinfo = enumSubitem.GetType().GetField(strValue);

            if (fieldinfo != null)
            {

                Object[] objs = fieldinfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (objs == null || objs.Length == 0)
                {
                    return strValue;
                }
                else
                {
                    DescriptionAttribute da = (DescriptionAttribute)objs[0];
                    return da.Description;
                }
            }
            else
            {
                return "未找到的状态";
            }

        }

        public ActionResult ManuallyReExchangeMoveType(string FRBNRStr,string SGTXTStr)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(FRBNRStr) && !string.IsNullOrWhiteSpace(SGTXTStr))
                {
                    IList<InvTrans> invTransList = new List<InvTrans>();
                    string[] FRBNRArr = FRBNRStr.Split(',');
                    string[] SGTXTArr = SGTXTStr.Split(',');
                    for (int i = 0; i < FRBNRArr.Length; i++)
                    {
                        var invTraces = this.genericMgr.FindAll<InvTrans>("select i from InvTrans as i where i.FRBNR=? and i.SGTXT=?", new object[] { FRBNRArr[i], SGTXTArr[i] });
                        invTransList.Add(invTraces.First());
                    }
                    transMgr.ManuallyReExchangeMoveType(invTransList);
                }
                else
                {
                    transMgr.ReExchangeMoveType();
                }
                SaveSuccessMessage("移动类型重发成功。");

            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.Message);
            }
            InvTransSearchModel searchModel = TempData["InvTransSearchModel"] != null ? TempData["InvTransSearchModel"] as InvTransSearchModel : new InvTransSearchModel();
            return RedirectToAction("List", searchModel);
        }

        #region 导出明细
        public void SaveOrderDetailViewToClient()
        {
            InvTransSearchModel searchModel = TempData["InvTransSearchModel_1"] as InvTransSearchModel;
            TempData["InvTransSearchModel_1"] = searchModel;
            int pageSize = TempData["Total"] != null ? (int)TempData["Total"] : 100;
            TempData["Total"] = pageSize;
            GridCommand command = new GridCommand();
            command.PageSize = pageSize;
            command.Page = 1;
            SearchStatementModel searchStatementModel = PrepareSearchStatement(command, searchModel);
            GridModel<InvTrans> gridlist = GetAjaxPageData<InvTrans>(searchStatementModel, command);
            foreach (InvTrans inv in gridlist.Data)
            {
                inv.StatusName = GetEnumDescription(inv.Status);
            }
            IList<object> data = new List<object>();
            data.Add(gridlist.Data);
            reportGen.WriteToClient("InvTrans.xls", data, "InvTrans.xls");
        }
        #endregion


        private SearchStatementModel PrepareSearchStatement(GridCommand command, InvTransSearchModel searchModel)
        {
            string whereStatement = " where 1=1 ";
            if (!string.IsNullOrWhiteSpace(searchModel.items))
            {
                string items = searchModel.items.Replace("\r\n", ",");
                items = items.Replace("\n", ",");
                string[] itemArr = items.Split(',');
                string itemParam = string.Empty;
                for (int i = 0; i < itemArr.Length; i++)
                {
                    whereStatement += string.Format(" and MATNR in('{0}') ", string.Join("','", itemArr));
                }
            }
          
            IList<object> param = new List<object>();
            HqlStatementHelper.AddLikeStatement("BWART", searchModel.BWART, HqlStatementHelper.LikeMatchMode.Start, "t", ref whereStatement, param);
            HqlStatementHelper.AddBetweenStatement("BUDAT", searchModel.BUDAT != null?searchModel.BUDAT:DateTime.MinValue.ToString("yyyyMMdd"), searchModel.BUDATTo != null?searchModel.BUDATTo:DateTime.MaxValue.ToString("yyyyMMdd"), "t", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("EBELN", searchModel.EBELN, HqlStatementHelper.LikeMatchMode.Start, "t", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("EBELP", searchModel.EBELP, HqlStatementHelper.LikeMatchMode.Start, "t", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("FRBNR", searchModel.FRBNR, HqlStatementHelper.LikeMatchMode.Start, "t", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("LIFNR", searchModel.LIFNR, "t", ref whereStatement, param);

            HqlStatementHelper.AddEqStatement("Status", searchModel.Status, "t", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("SGTXT", searchModel.SGTXT, HqlStatementHelper.LikeMatchMode.Start, "t", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("LGORT", searchModel.LGORT, HqlStatementHelper.LikeMatchMode.Start, "t", ref whereStatement, param);
            //HqlStatementHelper.AddLikeStatement("MATNR", searchModel.MATNR, HqlStatementHelper.LikeMatchMode.Start, "t", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("XBLNR", searchModel.XBLNR, HqlStatementHelper.LikeMatchMode.Start, "t", ref whereStatement, param);


            HqlStatementHelper.AddLikeStatement("RSNUM", searchModel.RSNUM, HqlStatementHelper.LikeMatchMode.Start, "t", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("RSPOS", searchModel.RSPOS, HqlStatementHelper.LikeMatchMode.Start, "t", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("FRBNR", searchModel.FRBNR, HqlStatementHelper.LikeMatchMode.Start, "t", ref whereStatement, param);
            HqlStatementHelper.AddLikeStatement("XABLN", searchModel.XABLN, HqlStatementHelper.LikeMatchMode.Start, "t", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("ErrorCount", searchModel.ErrorCount, "t", ref whereStatement, param);

            if (searchModel.StartTime != null & searchModel.EndTime != null)
            {
                HqlStatementHelper.AddBetweenStatement("CreateDate", searchModel.StartTime, searchModel.EndTime, "t", ref whereStatement, param);
            }
            else if (searchModel.StartTime != null & searchModel.EndTime == null)
            {
                HqlStatementHelper.AddGeStatement("CreateDate", searchModel.StartTime, "t", ref whereStatement, param);
            }
            else if (searchModel.StartTime == null & searchModel.EndTime != null)
            {
                HqlStatementHelper.AddLeStatement("CreateDate", searchModel.EndTime, "t", ref whereStatement, param);
            }
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            if (command.SortDescriptors.Count == 0)
            {
                sortingStatement = " order by t.CreateDate desc";
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
