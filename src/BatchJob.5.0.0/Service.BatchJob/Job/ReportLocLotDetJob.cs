using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.BatchJob.Job;
using com.Sconit.Util;

namespace com.Sconit.Service.BatchJob.Job
{
    public class ReportLocLotDetJob : IJob
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("Log.BatchJob");

        public void Execute(JobContext context)
        {
            if (!context.JobDataMap.ContainKey("UserCode"))
            {
                log.Error("User code not specified.");
            }

            if (!context.JobDataMap.ContainKey("FtpServer"))
            {
                log.Error("FtpServer not specified.");
            }

            if (!context.JobDataMap.ContainKey("FtpPort"))
            {
                log.Error("FtpPort not specified.");
            }

            if (!context.JobDataMap.ContainKey("FtpUser"))
            {
                log.Error("FtpUser not specified.");
            }

            if (!context.JobDataMap.ContainKey("FtpPass"))
            {
                log.Error("FtpPass not specified.");
            }

            if (!context.JobDataMap.ContainKey("FtpFolder"))
            {
                log.Error("FtpFolder not specified.");
            }

            if (!context.JobDataMap.ContainKey("LocalFolder"))
            {
                log.Error("LocalFolder not specified.");
            }

            if (!context.JobDataMap.ContainKey("LocalTempFolder"))
            {
                log.Error("LocalTempFolder not specified.");
            }

            if (context.JobDataMap.ContainKey("UserCode") && context.JobDataMap.ContainKey("FtpServer")
                && context.JobDataMap.ContainKey("FtpPort") && context.JobDataMap.ContainKey("FtpUser")
                && context.JobDataMap.ContainKey("FtpPass") && context.JobDataMap.ContainKey("FtpFolder")
                && context.JobDataMap.ContainKey("LocalFolder") && context.JobDataMap.ContainKey("LocalTempFolder"))
            {
                SAPService.SAPService sapService = new SAPService.SAPService();
                if (context.JobDataMap.ContainKey("SIServiceAddress") && context.JobDataMap.ContainKey("SIServicePort"))
                {
                    sapService.Url = ServiceURLHelper.ReplaceServiceUrl(sapService.Url, context.JobDataMap.GetStringValue("SIServiceAddress"), context.JobDataMap.GetStringValue("SIServicePort"));
                }

                sapService.ReportLocLotDetAsync(context.JobDataMap.GetStringValue("UserCode"),
                                           context.JobDataMap.GetStringValue("FtpServer"),
                                           context.JobDataMap.GetIntValue("FtpPort"),
                                           context.JobDataMap.GetStringValue("FtpUser"),
                                           context.JobDataMap.GetStringValue("FtpPass"),
                                           context.JobDataMap.GetStringValue("FtpFolder"),
                                           context.JobDataMap.GetStringValue("LocalFolder"),
                                           context.JobDataMap.GetStringValue("LocalTempFolder"));
            }
        }
    }
}
