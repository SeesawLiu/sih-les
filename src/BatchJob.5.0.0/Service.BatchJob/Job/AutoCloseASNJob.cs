using System;
using com.Sconit.BatchJob.Job;
using com.Sconit.Util;

namespace com.Sconit.Service.BatchJob.Job
{
    public class AutoCloseASNJob : IJob
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
                OrderService.OrderService orderService = new OrderService.OrderService();
                if (context.JobDataMap.ContainKey("SIServiceAddress") && context.JobDataMap.ContainKey("SIServicePort"))
                {
                    orderService.Url = ServiceURLHelper.ReplaceServiceUrl(orderService.Url, context.JobDataMap.GetStringValue("SIServiceAddress"), context.JobDataMap.GetStringValue("SIServicePort"));
                }
                orderService.AutoCloseASNAsync(context.JobDataMap.GetStringValue("UserCode"), DateTime.Now.AddDays(context.JobDataMap.GetDoubleValue("DayDiff")));
            }
        }
    }
}
