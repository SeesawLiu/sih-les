using com.Sconit.BatchJob.Job;
using com.Sconit.Util;

namespace com.Sconit.Service.BatchJob.Job
{
    public class PostDistributionOrderJob : IJob
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
                SAPService.SAPService sapService = new SAPService.SAPService();
                if (context.JobDataMap.ContainKey("SIServiceAddress") && context.JobDataMap.ContainKey("SIServicePort"))
                {
                    sapService.Url = ServiceURLHelper.ReplaceServiceUrl(sapService.Url, context.JobDataMap.GetStringValue("SIServiceAddress"), context.JobDataMap.GetStringValue("SIServicePort"));
                }

                sapService.PostDistributionOrder(context.JobDataMap.GetStringValue("UserCode"));
            }
        }
    }
}
