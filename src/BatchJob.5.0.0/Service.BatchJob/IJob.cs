
namespace com.Sconit.BatchJob.Job
{
    public interface IJob
    {
        void Execute(JobContext context);
    }
}
