using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Windsor;
using com.Sconit.Entity.BatchJob.BAT;
using com.Sconit.Persistence;
using com.Sconit.BatchJob.Job;
using System.Threading.Tasks;
using System.Threading;
using Castle.Services.Transaction;

namespace com.Sconit.Service.BatchJob.Impl
{
    [Transactional]
    public class JobRunMgrImpl : IJobRunMgr
    {

        private static log4net.ILog log = log4net.LogManager.GetLogger("Log.BatchJob");

        public INHDao dao { get; set; }

        public void RunBatchJobs(IWindsorContainer container)
        {
            log.Info("----------------------------------Invincible's dividing line---------------------------------------");
            log.Info("BatchJobs run start.");

            IList<Trigger> tobeFiredTriggerList = this.GetTobeFiredTrigger();
            string siServiceAddress = this.dao.FindAllWithNativeSql<string>("select Value from SYS_EntityPreference where Id = ?", new object[] { com.Sconit.Entity.SYS.EntityPreference.CodeEnum.SIServiceAddress }).SingleOrDefault();
            string siServicePort = this.dao.FindAllWithNativeSql<string>("select Value from SYS_EntityPreference where Id = ?", new object[] { com.Sconit.Entity.SYS.EntityPreference.CodeEnum.SIServicePort }).SingleOrDefault();

            if (tobeFiredTriggerList != null && tobeFiredTriggerList.Count > 0)
            {
                //Parallel.ForEach(tobeFiredTriggerList, (tobeFiredTrigger) =>
                foreach (Trigger tobeFiredTrigger in tobeFiredTriggerList)
                {
                    Thread.Sleep(500);

                    JobDetail jobDetail = tobeFiredTrigger.JobDetail;
                    RunLog runLog = new RunLog();
                    try
                    {
                        #region Job运行前处理
                        BeforeJobRun(runLog, tobeFiredTrigger);
                        #endregion

                        #region 运行Job
                        JobDataMap dataMap = new JobDataMap();

                        #region 把WebService地址传入Job
                        if (!string.IsNullOrWhiteSpace(siServiceAddress)
                            && !string.IsNullOrWhiteSpace(siServicePort))
                        {
                            dataMap.PutData("SIServiceAddress", siServiceAddress);
                            dataMap.PutData("SIServicePort", siServicePort);
                        }
                        #endregion

                        #region Job参数获取
                        IList<JobParameter> jobParameterList = this.dao.FindAllWithCustomQuery<JobParameter>("from JobParameter where JobId = ?", jobDetail.Id);
                        if (jobParameterList != null && jobParameterList.Count > 0)
                        {
                            foreach (JobParameter jobParameter in jobParameterList)
                            {
                                log.Debug("Set Job Parameter Name:" + jobParameter.Key + ", Value:" + jobParameter.Value);
                                dataMap.PutData(jobParameter.Key, jobParameter.Value);
                            }
                        }
                        #endregion

                        #region Trigger参数获取
                        IList<TriggerParameter> triggerParameterList = this.dao.FindAllWithCustomQuery<TriggerParameter>("from TriggerParameter where TriggerId = ?", tobeFiredTrigger.Id);
                        if (triggerParameterList != null && triggerParameterList.Count > 0)
                        {
                            foreach (TriggerParameter triggerParameter in triggerParameterList)
                            {
                                log.Debug("Set Trigger Parameter Name:" + triggerParameter.Key + ", Value:" + triggerParameter.Value);
                                if (!dataMap.ContainKey(triggerParameter.Key))
                                {
                                    dataMap.PutData(triggerParameter.Key, triggerParameter.Value);
                                }
                            }
                        }
                        #endregion

                        #region 初始化JobRunContext
                        JobContext jobRunContext = new JobContext(tobeFiredTrigger, jobDetail, dataMap, container);
                        #endregion

                        #region 调用Job
                        IJob job = container.Resolve<IJob>(jobDetail.ServiceType);
                        log.Debug("Start run job: " + jobDetail.ServiceType);
                        job.Execute(jobRunContext);
                        this.dao.FlushSession();
                        #endregion

                        #endregion

                        #region Job运行后处理
                        AfterJobRunSuccess(runLog, tobeFiredTrigger);
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        AfterJobRunFail(runLog, tobeFiredTrigger, ex);
                    }
                    finally
                    {
                        #region 更新BatchTrigger
                        UpdateTrigger(tobeFiredTrigger);
                        #endregion
                    }
                }
                //);
            }
            else
            {
                log.Info("No job found may run in this batch.");
            }

            log.Info("BatchJobs run end.");
        }

        private IList<Trigger> GetTobeFiredTrigger()
        {
            //return this.dao.FindAllWithCustomQuery<Trigger>("from Trigger Where NextFireTime <= ? and Id=?", new object[] { DateTime.Now, 247 });
            return this.dao.FindAllWithCustomQuery<Trigger>("from Trigger Where Status = ? and NextFireTime <= ?", new object[] { CodeMaster.TriggerStatus.Open, DateTime.Now });
        }

        private void BeforeJobRun(RunLog runLog, Trigger tobeFiredTrigger)
        {
            log.Info("Start run job. JobId:" + tobeFiredTrigger.JobDetail.Id + ", JobName:" + tobeFiredTrigger.JobDetail.Name);
            runLog.JobDetailId = tobeFiredTrigger.JobDetail.Id;
            runLog.TriggerId = tobeFiredTrigger.Id;
            runLog.StartTime = DateTime.Now;
            runLog.Status = CodeMaster.JobRunStatus.InProcess;
            this.dao.Create(runLog);
            this.dao.FlushSession();
        }

        private void AfterJobRunSuccess(RunLog runLog, Trigger tobeFiredTrigger)
        {
            log.Info("Job run successful. JobId:" + tobeFiredTrigger.JobDetail.Id + ", JobName:" + tobeFiredTrigger.JobDetail.Name);
            this.dao.ExecuteUpdateWithCustomQuery("update from RunLog set EndTime = ?, Status = ? where Id = ?",
                new object[] { DateTime.Now, CodeMaster.JobRunStatus.Success, runLog.Id });
            this.dao.FlushSession();
        }

        private void AfterJobRunFail(RunLog runLog, Trigger tobeFiredTrigger, Exception ex)
        {
            try
            {
                log.Error("Job run failure. JobId:" + tobeFiredTrigger.JobDetail.Id + ", JobName:" + tobeFiredTrigger.JobDetail.Name, ex);
                if (ex.Message != null && ex.Message.Length > 255)
                {
                    this.dao.ExecuteUpdateWithCustomQuery("update from RunLog set EndTime = ?, Status = ?, Message = ? where Id = ?",
                        new object[] { DateTime.Now, CodeMaster.JobRunStatus.Failure, ex.Message.Substring(0, 255), runLog.Id });
                }
                else
                {
                    this.dao.ExecuteUpdateWithCustomQuery("update from RunLog set EndTime = ?, Status = ?, Message = ? where Id = ?",
                        new object[] { DateTime.Now, CodeMaster.JobRunStatus.Failure, ex.Message, runLog.Id });
                }
            }
            catch (Exception ex1)
            {
                log.Error("", ex1);
            }
        }

        private void UpdateTrigger(Trigger tobeFiredTrigger)
        {
            try
            {
                tobeFiredTrigger.TimesTriggered++;
                tobeFiredTrigger.PreviousFireTime = tobeFiredTrigger.NextFireTime;
                if (tobeFiredTrigger.RepeatCount != 0 && tobeFiredTrigger.TimesTriggered >= tobeFiredTrigger.RepeatCount)
                {
                    //关闭Trigger
                    log.Debug("Close Trigger:" + tobeFiredTrigger.Name);
                    tobeFiredTrigger.Status = CodeMaster.TriggerStatus.Close;
                    tobeFiredTrigger.NextFireTime = null;
                }
                else
                {
                    //设置下次运行时间
                    log.Debug("Set Trigger Next Start Time, Add:" + tobeFiredTrigger.Interval.ToString() + " " + tobeFiredTrigger.IntervalType);
                    DateTime dateTimeNow = DateTime.Now;
                    if (!tobeFiredTrigger.NextFireTime.HasValue)
                    {
                        tobeFiredTrigger.NextFireTime = dateTimeNow;
                    }

                    while (tobeFiredTrigger.NextFireTime.Value <= dateTimeNow)
                    {
                        if (tobeFiredTrigger.IntervalType == CodeMaster.TimeUnit.Year)
                        {
                            tobeFiredTrigger.NextFireTime = tobeFiredTrigger.NextFireTime.Value.AddYears(tobeFiredTrigger.Interval);
                        }
                        else if (tobeFiredTrigger.IntervalType == CodeMaster.TimeUnit.Month)
                        {
                            tobeFiredTrigger.NextFireTime = tobeFiredTrigger.NextFireTime.Value.AddMonths(tobeFiredTrigger.Interval);
                        }
                        else if (tobeFiredTrigger.IntervalType == CodeMaster.TimeUnit.Day)
                        {
                            tobeFiredTrigger.NextFireTime = tobeFiredTrigger.NextFireTime.Value.AddDays(tobeFiredTrigger.Interval);
                        }
                        else if (tobeFiredTrigger.IntervalType == CodeMaster.TimeUnit.Hour)
                        {
                            tobeFiredTrigger.NextFireTime = tobeFiredTrigger.NextFireTime.Value.AddHours(tobeFiredTrigger.Interval);
                        }
                        else if (tobeFiredTrigger.IntervalType == CodeMaster.TimeUnit.Minute)
                        {
                            tobeFiredTrigger.NextFireTime = tobeFiredTrigger.NextFireTime.Value.AddMinutes(tobeFiredTrigger.Interval);
                        }
                        else if (tobeFiredTrigger.IntervalType == CodeMaster.TimeUnit.Second)
                        {
                            tobeFiredTrigger.NextFireTime = tobeFiredTrigger.NextFireTime.Value.AddSeconds(tobeFiredTrigger.Interval);
                        }
                        else
                        {
                            throw new ArgumentException("invalid Interval Type:" + tobeFiredTrigger.IntervalType);
                        }
                    }
                    log.Debug("Trigger Next Start Time is set as:" + tobeFiredTrigger.NextFireTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                }

                this.dao.ExecuteUpdateWithCustomQuery("update from Trigger set TimesTriggered = ?, PreviousFireTime = ?, NextFireTime = ?, Status = ? where Id = ?",
                        new object[] { tobeFiredTrigger.TimesTriggered, tobeFiredTrigger.PreviousFireTime, tobeFiredTrigger.NextFireTime, tobeFiredTrigger.Status, tobeFiredTrigger.Id });
            }
            catch (Exception ex)
            {
                log.Error("Error occur when update batch trigger.", ex);
            }
        }        
    }
}
