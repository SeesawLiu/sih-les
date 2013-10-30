using com.Sconit.BatchJob.Job;
using com.Sconit.Util;

namespace com.Sconit.Service.BatchJob.Job
{
    class LesCreateOrderDATJob : IJob
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("Log.BatchJob");

        public void Execute(JobContext context)
        {
            if (!context.JobDataMap.ContainKey("UserCode"))
            {
                log.Error("User code not specified.");
            }

            if (!context.JobDataMap.ContainKey("SystemCodes"))
            {
                log.Error("User code not specified.");
            }

            if (context.JobDataMap.ContainKey("UserCode") && context.JobDataMap.ContainKey("SystemCodes"))
            {
                FISService.FISService fisService = new FISService.FISService();
                if (context.JobDataMap.ContainKey("SIServiceAddress") && context.JobDataMap.ContainKey("SIServicePort"))
                {
                    fisService.Url = ServiceURLHelper.ReplaceServiceUrl(fisService.Url, context.JobDataMap.GetStringValue("SIServiceAddress"), context.JobDataMap.GetStringValue("SIServicePort"));
                }
                fisService.ExportAsync(context.JobDataMap.GetStringValue("SystemCodes").Split(','), context.JobDataMap.GetStringValue("UserCode"));
            }
        }
    }
}