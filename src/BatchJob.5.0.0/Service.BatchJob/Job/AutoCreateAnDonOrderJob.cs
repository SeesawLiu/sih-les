using com.Sconit.BatchJob.Job;
using com.Sconit.Util;

namespace com.Sconit.Service.BatchJob.Job
{
    public class AutoCreateAnDonOrderJob : IJob
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
                OrderService.OrderService serviceProxy = new OrderService.OrderService();
                //SAPService.SAPService sapService = new SAPService.SAPService();
                if (context.JobDataMap.ContainKey("SIServiceAddress") && context.JobDataMap.ContainKey("SIServicePort"))
                {
                    serviceProxy.Url = ServiceURLHelper.ReplaceServiceUrl(serviceProxy.Url, context.JobDataMap.GetStringValue("SIServiceAddress"), context.JobDataMap.GetStringValue("SIServicePort"));
                }
                serviceProxy.AutoGenAnDonOrderAsync(context.JobDataMap.GetStringValue("UserCode"));
            }
        }
    }
}
