using System.Web.Services;
using com.Sconit.Entity;
using com.Sconit.Entity.FIS;
using com.Sconit.Service.FIS;
using com.Sconit.Web.Util;

namespace com.Sconit.WebService.FIS
{
    //public IImportFromMesMgr 
    [WebService(Namespace = "http://com.Sconit.WebService.FIS.FISService/")]
    public class FISService : BaseWebService
    {
        public ICommonDataMgr commonDataMgr { get { return GetService<ICommonDataMgr>(); } }

        public IOutboundGen outboundGen { get { return GetService<IOutboundGen>(); } }

        public IInboundGen inboundGen { get { return GetService<IInboundGen>(); } }

        [WebMethod]
        public void CreateLog(LesINLog lesINLog)
        {
            SecurityContextHolder.Set(securityMgr.GetUser("su"));
            commonDataMgr.CreateLog(lesINLog);
        }

        //[WebMethod]
        //public LesINLog SelectLesINLogByWmsNo(string selectSql, object param)
        //{
        //    SecurityContextHolder.Set(securityMgr.GetUser("su"));
        //    return commonDataMgr.SelectLesINLogByWmsNo(selectSql, param);
        //}

        [WebMethod]
        public void UpdateLog(LesINLog lesINLog)
        {
            SecurityContextHolder.Set(securityMgr.GetUser("su"));
            commonDataMgr.UpdateLog(lesINLog);
        } 

        [WebMethod]
        public void Export(string[] systemCodeList, string userCode)
        {
            if (systemCodeList.Length > 0)
            {
                SecurityContextHolder.Set(securityMgr.GetUser(userCode));
                foreach (var systemCode in systemCodeList) 
                {
                    outboundGen.ExportData(systemCode, ServiceLocator.ObtainContainer());
                }
                //outboundGen.UploadFile();
            }
        }

        [WebMethod]
        public void TestExport(string systemCode, string userCode)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));

            outboundGen.ExportData(systemCode, ServiceLocator.ObtainContainer());
            //outboundGen.UploadFile();
        }

        [WebMethod]
        public void Import(string userCode)
        {
            SecurityContextHolder.Set(securityMgr.GetUser(userCode));
            inboundGen.DownloadFile();
            inboundGen.ImportData(ServiceLocator.ObtainContainer());
            //outboundGen.UploadFile();
        }

        [WebMethod]
        public void UploadFile()
        {
            SecurityContextHolder.Set(securityMgr.GetUser("su"));
            outboundGen.UploadFile();
        }
    }
}
