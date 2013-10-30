using System;
using com.Sconit.BatchJob.Job;
using com.Sconit.Util;

namespace com.Sconit.Service.BatchJob.Job
{
    public class BackFlushVanOrderJob : IJob
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("Log.BatchJob");

        public void Execute(JobContext context)
        {
            if (!context.JobDataMap.ContainKey("UserCode"))
            {
                log.Error("User code not specified.");
            }

            if (context.JobDataMap.ContainKey("UserCode"))
            {
                OrderService.OrderService orderService = new OrderService.OrderService();
                if (context.JobDataMap.ContainKey("SIServiceAddress") && context.JobDataMap.ContainKey("SIServicePort"))
                {
                    orderService.Url = ServiceURLHelper.ReplaceServiceUrl(orderService.Url, context.JobDataMap.GetStringValue("SIServiceAddress"), context.JobDataMap.GetStringValue("SIServicePort"));
                }
                orderService.BackFlushVanOrderAsync(context.JobDataMap.GetStringValue("UserCode"));
            }
        }
    }
}
