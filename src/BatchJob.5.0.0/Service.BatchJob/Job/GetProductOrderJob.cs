using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.BatchJob.Job;
using com.Sconit.Util;

namespace com.Sconit.Service.BatchJob.Job
{
    public class GetProductOrderJob : IJob
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("Log.BatchJob");

        public void Execute(JobContext context)
        {
            if (!context.JobDataMap.ContainKey("UserCode"))
            {
                log.Error("User code not specified.");
            }

            if (!context.JobDataMap.ContainKey("DayDiff"))
            {
                log.Error("Day difference not specified.");
            }

            if (context.JobDataMap.ContainKey("UserCode") && context.JobDataMap.ContainKey("DayDiff"))
            {
                string userCode = context.JobDataMap.GetStringValue("UserCode");
                DateTime dateTimeNow = DateTime.Now;
                SAPService.SAPService sapService = new SAPService.SAPService();
                if (context.JobDataMap.ContainKey("SIServiceAddress") && context.JobDataMap.ContainKey("SIServicePort"))
                {
                    sapService.Url = ServiceURLHelper.ReplaceServiceUrl(sapService.Url, context.JobDataMap.GetStringValue("SIServiceAddress"), context.JobDataMap.GetStringValue("SIServicePort"));
                }
                //sapService.GetProdOrders(userCode, dateTimeNow.AddDays(context.JobDataMap.GetDoubleValue("DayDiff")), null);
            }
        }
    }
}