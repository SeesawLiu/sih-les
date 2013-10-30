using Castle.Windsor;
using com.Sconit.Entity.BatchJob.BAT;

namespace com.Sconit.BatchJob.Job
{
    public class JobContext
    {
        protected Trigger trigger;
        protected com.Sconit.Entity.BatchJob.BAT.JobDetail jobDetail;
        protected JobDataMap jobDataMap;
        protected IWindsorContainer container;

        public JobContext(Trigger trigger, JobDetail jobDetail, JobDataMap jobDataMap, IWindsorContainer container)
        {
            this.trigger = trigger;
            this.jobDetail = jobDetail;
            this.jobDataMap = jobDataMap;
            this.container = container;
        }

        public Trigger Trigger
        {
            get
            {
                return trigger;
            }
        }

        public com.Sconit.Entity.BatchJob.BAT.JobDetail Job
        {
            get
            {
                return jobDetail;
            }
        }

        public JobDataMap JobDataMap
        {
            get
            {
                return jobDataMap;
            }
        }

        public IWindsorContainer Container
        {
            get
            {
                return container;
            }
        }
    }
}
