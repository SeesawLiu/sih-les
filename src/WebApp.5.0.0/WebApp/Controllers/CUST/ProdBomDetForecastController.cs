using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using com.Sconit.Entity.CUST;
using Telerik.Web.Mvc;
using com.Sconit.Entity.SCM;
using com.Sconit.Web.Util;
using com.Sconit.Web.Models.CUST;
using com.Sconit.Web.Models.SearchModels.CUST;
using com.Sconit.Web.Models;

namespace com.Sconit.Web.Controllers.CUST
{
    public class ProdBomDetForecastController : WebAppBaseController
    {
        //
        // GET: /ProdBomDetForecast/

        private static string PermissionSearch = @"
;WITH    PER
          AS ( SELECT   PermissionCode , Category , Code, CategoryType
               FROM     VIEW_UserPermission AS p
                        INNER JOIN dbo.ACC_User U ON P.UserId = U.Id
               WHERE    U.Code = ? AND Category IN ( 'Supplier', 'Region' )
             ),
        MST
          AS ( SELECT   f.Code,f.PartyFrom
               FROM     PER
                        INNER JOIN dbo.MD_Supplier b ON PER.Code = b.Code AND per.Category = 'Supplier'
                        INNER JOIN SCM_FlowMstr f ON f.PartyFrom = b.Code
               UNION ALL
               SELECT   f.Code,f.PartyFrom
               FROM     PER
               			LEFT JOIN dbo.MD_Supplier b ON PER.Code = b.Code
						INNER JOIN SCM_FlowMstr f ON f.PartyTo = per.PermissionCode
               WHERE    b.Code IS NULL AND per.Category = 'Region'
             )
";
        private static string groupItemByBDTERNative = @"
    SELECT  MATNR ,MEINS ,SUM(BDMNG) BDMNG ,BDTER ,WERKS ,d.Desc1 ,RefCode
    FROM    dbo.CUST_ProdBomDetForecast a
            INNER JOIN ( SELECT Item ,PartyFrom
                         FROM   dbo.SCM_FlowDet
                                INNER JOIN MST ON dbo.SCM_FlowDet.Flow = MST.Code
                         GROUP BY Item ,PartyFrom
                       ) b ON a.MATNR = B.ITEM
            LEFT JOIN dbo.MD_Item d ON b.Item = d.Code AND D.IsActive = 1
    GROUP BY matnr ,meins ,bdter ,WERKS ,d.Desc1 ,RefCode
";

        private static string groupItemByCHARGNative_p1 = @"
SELECT MATNR, MEINS, SUM(BDMNG) BDMNG, BDTER,WERKS,d.Desc1,RefCode,CHARG,SEQNR
FROM dbo.CUST_ProdBomDetForecast a  INNER JOIN
(SELECT Item,PartyFrom FROM dbo.SCM_FlowDet 
INNER JOIN MST ON dbo.SCM_FlowDet.Flow = MST.Code  
GROUP BY Item,PartyFrom 
) b ON a.MATNR = B.ITEM
LEFT JOIN dbo.MD_Item d ON b.Item = d.Code AND D.IsActive = 1";

        private static string groupItemByCHARGNative_p2 = @"
group by CHARG,SEQNR,matnr,meins,bdter,WERKS,d.Desc1,RefCode
";

        #region View
        public ActionResult Index()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_ProdBomDetForecast_View")]
        [GridAction]
        public ActionResult List(GridCommand command, ProdBomDetForecastChargSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return PartialView();
        }


        [SconitAuthorize(Permissions = "Url_ProdBomDetForecast_View")]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxListForBDTER(GridCommand command)
        {
            List<ProdBomDetForecastView> result = GetBdterData();

            GridModel<ProdBomDetForecastView> GridModel = new GridModel<ProdBomDetForecastView>();
            GridModel.Total = result.Count;
            GridModel.Data = result
                .OrderBy(fct => fct.BDTER)
                .ThenByDescending(fct => fct.BDMNG)
                .Skip((command.Page - 1) * command.PageSize)
                .Take(command.PageSize).ToList();
            ViewBag.Total = GridModel.Total;

            return PartialView(GridModel);
        }

        [SconitAuthorize(Permissions = "Url_ProdBomDetForecast_View")]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxListForCHARG(GridCommand command, ProdBomDetForecastChargSearchModel searchModel)
        {
            List<ProdBomDetForecastChargView> result = GetChargData(searchModel);

            GridModel<ProdBomDetForecastChargView> GridModel = new GridModel<ProdBomDetForecastChargView>();
            GridModel.Total = result.Count;
            GridModel.Data = result
                .OrderBy(pf => pf.SEQNR)
                .ThenBy(pf => pf.CHARG)
                .Skip((command.Page - 1) * command.PageSize)
                .Take(command.PageSize).ToList();
            ViewBag.Total = GridModel.Total;

            return PartialView(GridModel);
        }

        /// <summary>
        /// 根据需求时间
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProdBomDetForecast_View")]
        public ActionResult _ResultByBDTER()
        {
            return PartialView();
        }

        /// <summary>
        /// 根据车号
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SconitAuthorize(Permissions = "Url_ProdBomDetForecast_View")]
        public ActionResult _ResultByCHARG()
        {
            return PartialView();
        }
        #endregion

        #region Export
        public void ExportByBDTERXLS()
        {
            List<ProdBomDetForecastView> exportList = GetBdterData();

            ExportToXLS<ProdBomDetForecastView>("ExportMatnrBdter", "XLS", exportList);
        }

        public void ExportByCHARGXLS()
        {
            List<ProdBomDetForecastChargView> exportList = GetChargData(new ProdBomDetForecastChargSearchModel());
            if (exportList.Count >= 65535)
            {
                exportList = exportList.Skip(0).Take(65535).ToList();
            }
            ExportToXLS<ProdBomDetForecastChargView>("ExportMatnrCharg", "XLS", exportList);


        }
        #endregion


        #region private
        private string PrepareSearchStatement(GridCommand command, ProdBomDetForecastChargSearchModel searchModel)
        {

            string groupItemByCHARGNative = PermissionSearch + groupItemByCHARGNative_p1;
            if (searchModel.CHARG != null)
            {
                groupItemByCHARGNative += " WHERE CHARG = '" + searchModel.CHARG + "'";
            }
            groupItemByCHARGNative += groupItemByCHARGNative_p2;
            return groupItemByCHARGNative;
        }

        private List<ProdBomDetForecastChargView> GetChargData(ProdBomDetForecastChargSearchModel searchModel)
        {
            List<ProdBomDetForecastChargView> result = new List<ProdBomDetForecastChargView>();
            if (Session["LES_ProdBomDetForecastCharg" + this.CurrentUser.Code] == null)
            {
                IList<object[]> groupedmatnr = this.genericMgr
                    .FindAllWithNativeSql<object[]>(
                    PermissionSearch +
                    groupItemByCHARGNative_p1 +
                    groupItemByCHARGNative_p2, this.CurrentUser.Code);

                foreach (object[] obj in groupedmatnr)
                {
                    ProdBomDetForecastChargView pb = new ProdBomDetForecastChargView();
                    pb.MATNR = obj[0].ToString();
                    pb.MEINS = (obj[1] ?? string.Empty).ToString();
                    pb.BDMNG = decimal.Parse((obj[2] ?? 0).ToString());
                    pb.BDTER = DateTime.Parse(obj[3].ToString());
                    pb.WERKS = (obj[4] ?? string.Empty).ToString();
                    pb.DESC = (obj[5] ?? string.Empty).ToString();
                    pb.REFCODE = (obj[6] ?? string.Empty).ToString();
                    pb.CHARG = obj[7].ToString();
                    pb.SEQNR = obj[8].ToString();
                    result.Add(pb);
                }
                Session["LES_ProdBomDetForecastCharg"+this.CurrentUser.Code] = result;
            }
            else
            {
                result = (List<ProdBomDetForecastChargView>)Session["LES_ProdBomDetForecastCharg" + this.CurrentUser.Code];
            }
            if (searchModel.CHARG != null)
            {
                return result.Where(pdf => pdf.CHARG == searchModel.CHARG).ToList();
            }
            else
            {
                return result;
            }
        }

        private List<ProdBomDetForecastView> GetBdterData()
        {
            List<ProdBomDetForecastView> exportList = new List<ProdBomDetForecastView>();

            if (Session["LES_ProdBomDetForecastBdter" + this.CurrentUser.Code] == null)
            {
                IList<object[]> groupedmatnr = this.genericMgr.FindAllWithNativeSql<object[]>(PermissionSearch + groupItemByBDTERNative, this.CurrentUser.Code);
                foreach (object[] obj in groupedmatnr)
                {
                    ProdBomDetForecastView pb = new ProdBomDetForecastView();
                    pb.MATNR = obj[0].ToString();
                    pb.MEINS = (obj[1] ?? string.Empty).ToString();
                    pb.BDMNG = decimal.Parse((obj[2] ?? 0).ToString());
                    pb.BDTER = DateTime.Parse(obj[3].ToString());
                    pb.WERKS = (obj[4] ?? string.Empty).ToString();
                    pb.DESC = (obj[5] ?? string.Empty).ToString();
                    pb.REFCODE = (obj[6] ?? string.Empty).ToString();
                    exportList.Add(pb);
                }
                Session["LES_ProdBomDetForecastBdter" + this.CurrentUser.Code] = exportList;
            }
            else
            {
                exportList = (List<ProdBomDetForecastView>)Session["LES_ProdBomDetForecastBdter" + this.CurrentUser.Code];
            }
            return exportList;
        }
        #endregion

    }
}
