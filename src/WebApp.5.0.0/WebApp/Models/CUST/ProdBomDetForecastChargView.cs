using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Web.Models.CUST
{
    public class ProdBomDetForecastChargView
    {
        [Export(ExportName = "ExportMatnrCharg", ExportSeq = 10)]
        [Display(Name = "ProdBomDetForecast_BDTER", ResourceType = typeof(Resources.CUST.ProdBomDetForecast))]
        public DateTime BDTER { get; set; }

        [Export(ExportName = "ExportMatnrCharg", ExportSeq = 20)]
        [Display(Name = "ProdBomDetForecast_WERKS", ResourceType = typeof(Resources.CUST.ProdBomDetForecast))]
        public string WERKS { get; set; }

        [Export(ExportName = "ExportMatnrCharg", ExportSeq = 30)]
        [Display(Name = "ProdBomDetForecast_MATNR", ResourceType = typeof(Resources.CUST.ProdBomDetForecast))]
        public string MATNR { get; set; }

        [Export(ExportName = "ExportMatnrCharg", ExportSeq = 40)]
        [Display(Name = "ProdBomDetForecast_MEINS", ResourceType = typeof(Resources.CUST.ProdBomDetForecast))]
        public string MEINS { get; set; }

        [Export(ExportName = "ExportMatnrCharg", ExportSeq = 50)]
        [Display(Name = "ProdBomDetForecast_BDMNG", ResourceType = typeof(Resources.CUST.ProdBomDetForecast))]
        public decimal BDMNG { get; set; }

        [Export(ExportName = "ExportMatnrCharg", ExportSeq = 60)]
        [Display(Name = "ProdBomDetForecast_DESC", ResourceType = typeof(Resources.CUST.ProdBomDetForecast))]
        public string DESC { get; set; }

        [Export(ExportName = "ExportMatnrCharg", ExportSeq = 70)]
        [Display(Name = "ProdBomDetForecast_REFCODE", ResourceType = typeof(Resources.CUST.ProdBomDetForecast))]
        public string REFCODE { get; set; }

        [Export(ExportName = "ExportMatnrCharg", ExportSeq = 80)]
        [Display(Name = "ProdBomDetForecast_CHARG", ResourceType = typeof(Resources.CUST.ProdBomDetForecast))]
        public string CHARG { get; set; }

        [Export(ExportName = "ExportMatnrCharg", ExportSeq = 90)]
        [Display(Name = "ProdBomDetForecast_SEQNR", ResourceType = typeof(Resources.CUST.ProdBomDetForecast))]
        public string SEQNR { get; set; }

    }
}