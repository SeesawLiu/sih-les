using System;
using System.ServiceProcess;
using Castle.Windsor;
using Castle.Windsor.Installer;
using com.Sconit.BatchJob.WindowsService.Properties;
using com.Sconit.Service.BatchJob;

namespace com.Sconit.BatchJob.WindowsService
{
    public partial class BatchJobService : ServiceBase
    {
        private static log4net.ILog log;
        private static IWindsorContainer container;
        public System.Timers.Timer timer;

        public BatchJobService()
        {
            InitializeComponent();
            container = new WindsorContainer();
            container.Install(Configuration.FromAppConfig());
            container.Install(FromAssembly.This());
            log = log4net.LogManager.GetLogger("Log.BatchJob");
            this.ServiceName = "BatchJobService";
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                log.Info("BatchJob Service Start");

                timer = new System.Timers.Timer();

                switch ((CodeMaster.TimeUnit)Settings.Default.IntervalType)
                {
                    case CodeMaster.TimeUnit.Second:
                        timer.Interval = Settings.Default.IntervalValue * 1000;
                        break;
                    case CodeMaster.TimeUnit.Minute:
                        timer.Interval = Settings.Default.IntervalValue * 1000 * 60;
                        break;
                    case CodeMaster.TimeUnit.Hour:
                        timer.Interval = Settings.Default.IntervalValue * 1000 * 60 * 60;
                        break;
                    case CodeMaster.TimeUnit.Day:
                        timer.Interval = Settings.Default.IntervalValue * 1000 * 60 * 60 * 24;
                        break;
                    case CodeMaster.TimeUnit.Week:
                        timer.Interval = Settings.Default.IntervalValue * 1000 * 60 * 60 * 24 * 7;
                        break;
                }
                timer.Enabled = true;
                timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            }
            catch (Exception ex)
            {
                log.Error("BatchJob Service Start Failure", ex);
            }
        }

        protected override void OnStop()
        {
            log.Info("BatchJob Service Stop");
        }

        private void RunJob()
        {
            try
            {
                log.Info("RunJob Start");
                IJobRunMgr jobRunMgr = container.Resolve<IJobRunMgr>();
                jobRunMgr.RunBatchJobs(container);
                log.Info("RunJob End");
            }
            catch (Exception ex)
            {
                log.Error("Batch Job Run Failure", ex);
            }
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer.Enabled = !Settings.Default.InterruptTimer;
            RunJob();
        }
    }
}
