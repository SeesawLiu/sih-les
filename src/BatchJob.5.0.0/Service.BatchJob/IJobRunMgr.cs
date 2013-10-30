using Castle.Windsor;

namespace com.Sconit.Service.BatchJob
{
    public interface IJobRunMgr
    {
        void RunBatchJobs(IWindsorContainer container);
    }
}
