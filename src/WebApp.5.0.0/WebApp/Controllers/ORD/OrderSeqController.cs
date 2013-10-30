using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.SYS;
using com.Sconit.Entity.VIEW;
using com.Sconit.Service;
using com.Sconit.Web.Models;
using com.Sconit.Web.Models.SearchModels.ORD;
using com.Sconit.Web.Util;

namespace com.Sconit.Web.Controllers.ORD
{
    public class OrderSeqController : WebAppBaseController
    {
        private static string selectCountStatement = "select count(*) from VanOrderSeqView as u";

        private static string selectStatement = "select u from VanOrderSeqView as u";

        /// <summary>
        /// Index action for OrderSeq controller
        /// </summary>
        /// <returns>Index view</returns>
        [SconitAuthorize(Permissions = "Url_OrderSeq_View")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// List action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">OrderSeq Search model</param>
        /// <returns>return the result view</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderSeq_View")]
        public ActionResult List(GridCommand command, OrderSeqSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            var error = false;
            if (string.IsNullOrWhiteSpace(searchModel.ProdLine))
            {
                error = true;
                SaveWarningMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.ORD.OrderMaster.OrderMaster_Flow);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.TraceCode) && !error)
            {
                if (base.genericMgr.FindAll<long>("select count(*) from OrderSeq as s where s.TraceCode = ? and s.ProductLine=? ", new object[] { searchModel.TraceCode, searchModel.ProdLine })[0] == 0)
                {
                    error = true;
                    SaveErrorMessage("Van号值加生产线找不到对应的数据。");
                }
            }

            if (error)
            {
                ViewBag.ReadOnly = true;
                return View();
            }

            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        /// <summary>
        ///  AjaxList action
        /// </summary>
        /// <param name="command">Telerik GridCommand</param>
        /// <param name="searchModel">OrderSeq Search Model</param>
        /// <returns>return the result action</returns>
        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderSeq_View")]
        public ActionResult _AjaxList(GridCommand command, OrderSeqSearchModel searchModel)
        {
            if (string.IsNullOrWhiteSpace(searchModel.ProdLine))
            {
                return PartialView(new GridModel(new List<VanOrderSeqView>()));
            }
            if (!string.IsNullOrWhiteSpace(searchModel.TraceCode) )
            {
                if (base.genericMgr.FindAll<long>("select count(*) from OrderSeq as s where s.TraceCode = ? and s.ProductLine=? ", new object[] { searchModel.TraceCode, searchModel.ProdLine })[0] == 0)
                {
                    return PartialView(new GridModel(new List<VanOrderSeqView>()));
                }
            }
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<VanOrderSeqView>(searchStatementModel, command));
            //ProcedureSearchStatementModel procedureSearchStatementModel = this.PrepareProcedureSearchStatement(command, searchModel);
            //return PartialView(GetAjaxPageDataProcedure<VanOrderSeqView>(procedureSearchStatementModel, command));
        }

        [SconitAuthorize(Permissions = "Url_OrderSeq_View")]
        public ActionResult ExportXLS(GridCommand command, OrderSeqSearchModel searchModel)
        {
            string hql = " select u from VanOrderSeqView as u where 1=1";
            IList<object> param = new List<object>();
            var error = false;
            if (string.IsNullOrWhiteSpace(searchModel.ProdLine))
            {
                error = true;
                SaveWarningMessage(Resources.ErrorMessage.Errors_Common_FieldRequired, Resources.ORD.OrderMaster.OrderMaster_Flow);
            }
            else
            {
                hql += " and  u.Flow=?";
                param.Add(searchModel.ProdLine);
            }
            long seq = 0;
            if (!string.IsNullOrWhiteSpace(searchModel.ProdLine) && !string.IsNullOrWhiteSpace(searchModel.TraceCode))
            {
                seq = this.genericMgr.FindAllWithNativeSql<long>("SELECT Seq FROM ORD_OrderSeq os where os.TraceCode=? AND os.ProdLine=? ", new object[] { searchModel.TraceCode, searchModel.ProdLine })[0];
                if (seq > 0)
                {
                    hql += " and  u.Sequence>=?";
                    param.Add(seq);
                }
                else
                {
                    error = true;
                    SaveErrorMessage("Van号值加生产线找不到对应的数据.");
                }
            }
           

            if (error)
            {
                ViewBag.ReadOnly = true;
                return RedirectToAction("Index");
            }

            var list = this.genericMgr.FindAll<VanOrderSeqView>(hql,param.ToArray());
            ExportToXLS<VanOrderSeqView>("VanOrderSeqView", "XLS", list);
            return null;
        }

        private ProcedureSearchStatementModel PrepareProcedureSearchStatement(GridCommand command, OrderSeqSearchModel searchModel)
        {
            List<ProcedureParameter> paraList = new List<ProcedureParameter>();
            List<ProcedureParameter> pageParaList = new List<ProcedureParameter>();
            paraList.Add(new ProcedureParameter { Parameter = searchModel.ProdLine, Type = NHibernate.NHibernateUtil.String });
            paraList.Add(new ProcedureParameter { Parameter = searchModel.TraceCode, Type = NHibernate.NHibernateUtil.String });

            pageParaList.Add(new ProcedureParameter { Parameter = command.SortDescriptors.Count > 0 ? command.SortDescriptors[0].Member : null, Type = NHibernate.NHibernateUtil.String });
            pageParaList.Add(new ProcedureParameter { Parameter = command.SortDescriptors.Count > 0 ? (command.SortDescriptors[0].SortDirection == ListSortDirection.Descending ? "desc" : "asc") : "asc", Type = NHibernate.NHibernateUtil.String });
            pageParaList.Add(new ProcedureParameter { Parameter = command.PageSize, Type = NHibernate.NHibernateUtil.Int32 });
            pageParaList.Add(new ProcedureParameter { Parameter = command.Page, Type = NHibernate.NHibernateUtil.Int32 });

            var procedureSearchStatementModel = new ProcedureSearchStatementModel();
            procedureSearchStatementModel.Parameters = paraList;
            procedureSearchStatementModel.PageParameters = pageParaList;
            procedureSearchStatementModel.CountProcedure = "USP_Search_VanOrderSeqCount";
            procedureSearchStatementModel.SelectProcedure = "USP_Search_VanOrderSeq";

            return procedureSearchStatementModel;
        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, OrderSeqSearchModel searchModel)
        {
            string whereStatement = string.Empty;

            IList<object> param = new List<object>();
            long seq = 0;
            if (!string.IsNullOrWhiteSpace(searchModel.ProdLine) && !string.IsNullOrWhiteSpace(searchModel.TraceCode))
            {
               seq= this.genericMgr.FindAllWithNativeSql<long>("SELECT Seq FROM ORD_OrderSeq os where os.TraceCode=? AND os.ProdLine=? ", new object[] { searchModel.TraceCode, searchModel.ProdLine })[0];
            }
            if (seq > 0)
            {
                HqlStatementHelper.AddGeStatement("Sequence", seq, "u", ref whereStatement, param);
            }
                HqlStatementHelper.AddEqStatement("Flow", searchModel.ProdLine, "u", ref whereStatement, param);
            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);
            if (string.IsNullOrWhiteSpace(sortingStatement))
            {
                sortingStatement = " ORDER BY u.Sequence ASC,u.SubSequence ASC ";
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
