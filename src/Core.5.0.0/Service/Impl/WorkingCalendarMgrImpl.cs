using System.Collections.Generic;
using System.Linq;
using Castle.Services.Transaction;
using com.Sconit.Entity;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.PRD;
using NHibernate;
using com.Sconit.Entity.VIEW;
using System;
using NHibernate.Criterion;
using AutoMapper;
using com.Sconit.CodeMaster;
using com.Sconit.Entity.ACC;

namespace com.Sconit.Service.Impl
{
    [Transactional]
    public class WorkingCalendarMgrImpl : BaseMgr, IWorkingCalendarMgr
    {
        public IGenericMgr genericMgr { get; set; }

        private static string selectWorkingShiftByWorkingCalendarId = "select w from WorkingShift as w where w.WorkingCalendar = ?";
        private static string deleteShiftDetailsByShift = "from ShiftDetail where Shift = ?";
        private static string deleteShiftMaster = "from ShiftMaster where Code = ?";
        private static string selectWorkingCalendarNoRegion = "select w from WorkingCalendar as w where w.Region is null";
        private static string selectWorkingCalendarByRegion = "select w from WorkingCalendar as w where w.Region = ?";
        private static string selectSpecialTimeNoRegion = "select s from SpecialTime as s where s.Region is null and StartTime <= ? and EndTime >= ?";
        private static string selectSpecialTimeByRegion = "select s from SpecialTime as s where s.Region = ? and StartTime <= ? and EndTime >= ?";

        private const string SelectStandardWorkingCalendarsStatement = @"select swc from StandardWorkingCalendar as swc where swc.Region = ? and swc.Category = ?";
        private const string SelectWorkingStatement = @"select wc from WorkingCalendar as wc where wc.Region = ? and wc.Category = ? and wc.WorkingDate between ? and ? ";

        private const string SelectStandardWorkingCalendarsStatementByProdLine = @"select swc from StandardWorkingCalendar as swc where swc.ProdLine = ? and swc.Category = ?";
        private const string SelectWorkingStatementByProdLine = @"select wc from WorkingCalendar as wc where wc.ProdLine = ? and wc.Category = ? and wc.WorkingDate between ? and ? ";

        #region public methods
        [Transaction(TransactionMode.Requires)]
        public void DeleteShiftMaster(string shiftMasterCode)
        {
            this.genericMgr.Delete(deleteShiftDetailsByShift, shiftMasterCode, NHibernateUtil.String);
            this.genericMgr.Delete(deleteShiftMaster, shiftMasterCode, NHibernateUtil.String);
        }

        [Transaction(TransactionMode.Requires)]
        public void UpdateWorkingCalendar(WorkingCalendar workingCalendar, IList<string> ShiftList)
        {
            #region 保存工作日历头
            this.genericMgr.Update(workingCalendar);
            #endregion

            #region 保存工作日历明细
            IList<ShiftMaster> assignedShiftMasterList = new List<ShiftMaster>();

            if (ShiftList != null && ShiftList.Count > 0)
            {
                assignedShiftMasterList = (from code in ShiftList
                                           select new ShiftMaster
                                            {
                                                Code = code
                                            }).ToList();
            }

            IList<WorkingShift> oldAssingedWorkingShiftList = this.genericMgr.FindAll<WorkingShift>(selectWorkingShiftByWorkingCalendarId, workingCalendar.Id);
            IList<ShiftMaster> oldAssingedShiftMasterList = new List<ShiftMaster>();

            if (oldAssingedWorkingShiftList != null && oldAssingedWorkingShiftList.Count > 0)
            {
                oldAssingedShiftMasterList = (from ws in oldAssingedWorkingShiftList
                                              select new ShiftMaster
                                           {
                                               Code = ws.Shift.Code
                                           }).ToList();
            }

            #region 删除没有授权的班次
            IList<ShiftMaster> deleteShiftMasterList = oldAssingedShiftMasterList.Except<ShiftMaster>(assignedShiftMasterList).ToList();
            if (deleteShiftMasterList.Count > 0)
            {
                foreach (ShiftMaster shiftMaster in deleteShiftMasterList)
                {
                    WorkingShift deletingWorkingShift = oldAssingedWorkingShiftList.Where(ur => ur.Shift.Code == shiftMaster.Code).SingleOrDefault();
                    if (deletingWorkingShift != null)
                    {
                        this.genericMgr.Delete(deletingWorkingShift);
                    }
                }
            }
            #endregion

            #region 保存新增授权的班次
            IList<ShiftMaster> insertingShiftMasterList = assignedShiftMasterList.Except<ShiftMaster>(oldAssingedShiftMasterList).ToList();
            if (insertingShiftMasterList.Count > 0)
            {
                IList<WorkingShift> insertingWorkingShiftList = (from shiftMaster in insertingShiftMasterList
                                                                 select new WorkingShift
                                                                 {
                                                                     WorkingCalendar = workingCalendar.Id,
                                                                     Shift = shiftMaster
                                                                 }).ToList();

                foreach (WorkingShift workingShift in insertingWorkingShiftList)
                {
                    this.genericMgr.Create(workingShift);
                }
            }
            #endregion
            #endregion
        }

        public IList<WorkingCalendarView> GetWorkingCalendarView(string regionCode, DateTime dateFrom, DateTime dateTo)
        {
            #region 获取全局的工作日历
            IList<WorkingCalendar> workingCalendarList = this.genericMgr.FindAll<WorkingCalendar>(selectWorkingCalendarNoRegion);
            #endregion

            #region 合并Region和全局的工作日历
            if (!string.IsNullOrEmpty(regionCode))
            {
                WorkingCalendarComparer workingCalendarComparer = new WorkingCalendarComparer();
                IList<WorkingCalendar> regionWorkingCalendarList = this.genericMgr.FindAll<WorkingCalendar>(selectWorkingCalendarByRegion, regionCode);

                IList<WorkingCalendar> intersectWorkingCalendarList = regionWorkingCalendarList.Intersect<WorkingCalendar>(workingCalendarList, workingCalendarComparer).ToList();
                IList<WorkingCalendar> exceptWorkingCalendarList = workingCalendarList.Except<WorkingCalendar>(regionWorkingCalendarList, workingCalendarComparer).ToList();

                workingCalendarList = exceptWorkingCalendarList.Union<WorkingCalendar>(intersectWorkingCalendarList).OrderBy(w => w.DayOfWeek).ToList();
            }
            #endregion

            if (workingCalendarList != null && workingCalendarList.Count > 0)
            {
                #region 获取工作班次
                DetachedCriteria criteria = DetachedCriteria.For<WorkingShift>();
                criteria.Add(Expression.In("WorkingCalendar", (from w in workingCalendarList select w.Id).ToArray()));
                IList<WorkingShift> workingShiftList = this.genericMgr.FindAll<WorkingShift>(criteria).Distinct().ToList();
                #endregion

                #region 获取班次明细
                criteria = DetachedCriteria.For<ShiftDetail>();
                criteria.Add(Expression.In("Shift", (from w in workingShiftList select w.Shift.Code).ToArray()));

                IList<ShiftDetail> shiftDetailList = this.genericMgr.FindAll<ShiftDetail>(criteria);
                #endregion

                #region 获取特殊时间
                #region 获取全局的特殊时间
                IList<SpecialTime> specialTimeList = this.genericMgr.FindAll<SpecialTime>(selectSpecialTimeNoRegion, new object[] { dateTo, dateFrom });
                #endregion

                #region 合并Region和全局的特殊时间
                if (!string.IsNullOrEmpty(regionCode))
                {
                    SpecialTimeComparer specialTimeComparer = new SpecialTimeComparer();
                    IList<SpecialTime> regionSpecialTimeList = this.genericMgr.FindAll<SpecialTime>(selectSpecialTimeByRegion, new object[] { regionCode, dateTo, dateFrom });

                    IList<SpecialTime> intersectSpecialTimeList = regionSpecialTimeList.Intersect<SpecialTime>(specialTimeList, specialTimeComparer).ToList();
                    IList<SpecialTime> exceptSpecialTimeList = specialTimeList.Except<SpecialTime>(regionSpecialTimeList, specialTimeComparer).ToList();

                    specialTimeList = exceptSpecialTimeList.Union<SpecialTime>(intersectSpecialTimeList).OrderBy(w => w.StartTime).ToList();
                }
                #endregion
                #endregion

                #region 循环查询日期区间获取工作和休息信息
                WorkingCalendarView firstWorkingCalendarView = null;
                WorkingCalendarView lastWorkingCalendarView = null;
                IList<WorkingCalendarView> workingCalendarViewList = new List<WorkingCalendarView>();
                DateTime cycleDateTime = new DateTime(dateFrom.Year, dateFrom.Month, dateFrom.Day);
                for (; DateTime.Compare(cycleDateTime, dateTo) <= 0; cycleDateTime = cycleDateTime.AddDays(1))
                {
                    #region 获取当前是星期几
                    WorkingCalendar workingCalendar = (from w in workingCalendarList
                                                       where w.DayOfWeek == cycleDateTime.DayOfWeek
                                                       select w).SingleOrDefault();
                    #endregion

                    if (workingCalendar != null)
                    {
                        #region 获取班次明细
                        IList<WorkingShift> currentWorkingShiftList = (from w in workingShiftList
                                                                       where w.WorkingCalendar == workingCalendar.Id
                                                                       select w).ToList();

                        IList<ShiftDetail> currentShiftDetailList = new List<ShiftDetail>();
                        //(from w in currentWorkingShiftList
                        //                                             join s in shiftDetailList on w.Shift.Code equals s.Shift
                        //                                             where (!s.StartDate.HasValue || s.StartDate <= cycleDateTime)
                        //                                                    && (!s.EndDate.HasValue || s.EndDate >= cycleDateTime)
                        //                                             select s).OrderBy(s => s.ShiftTime).ToList();
                        #endregion

                        if (currentShiftDetailList.Count > 0)
                        {
                            #region 处理工作日,循环班次明细
                            foreach (ShiftDetail currentShiftDetail in currentShiftDetailList)
                            {
                                WorkingCalendarView currentWorkingCalendarView = new WorkingCalendarView();
                                currentWorkingCalendarView.ShiftCode = currentShiftDetail.Shift;
                                currentWorkingCalendarView.ShiftName = (from w in currentWorkingShiftList where w.Shift.Code == currentShiftDetail.Shift select w.Shift.Name).Single();
                                currentWorkingCalendarView.DayOfWeek = cycleDateTime.DayOfWeek;
                                currentWorkingCalendarView.Date = cycleDateTime;
                                //string[] splitedShiftTime = currentShiftDetail.ShiftTime.Split(ShiftDetail.ShiftTimeSplitSymbol);
                                //currentWorkingCalendarView.DateFrom = Convert.ToDateTime(cycleDateTime.ToString("yyyy-MM-dd") + " " + splitedShiftTime[0]);
                                //currentWorkingCalendarView.DateTo = Convert.ToDateTime(cycleDateTime.ToString("yyyy-MM-dd") + " " + splitedShiftTime[1]);
                                if (DateTime.Compare(currentWorkingCalendarView.DateFrom, currentWorkingCalendarView.DateTo) >= 0)
                                {
                                    //如果开始日期大于等于结束日期，结束日期+1
                                    currentWorkingCalendarView.DateTo = currentWorkingCalendarView.DateTo.AddDays(1);
                                }
                                currentWorkingCalendarView.Type = com.Sconit.CodeMaster.WorkingCalendarType.Work;

                                #region 如果和上个工作时间不连续，插入一条休息时间记录
                                if (lastWorkingCalendarView != null
                                    && lastWorkingCalendarView.DateTo < currentWorkingCalendarView.DateFrom)
                                {
                                    WorkingCalendarView insertedWorkingCalendarView = new WorkingCalendarView();
                                    insertedWorkingCalendarView.DayOfWeek = currentWorkingCalendarView.DayOfWeek;
                                    insertedWorkingCalendarView.Date = currentWorkingCalendarView.Date;
                                    insertedWorkingCalendarView.DateFrom = lastWorkingCalendarView.DateTo;
                                    insertedWorkingCalendarView.DateTo = currentWorkingCalendarView.DateFrom;
                                    insertedWorkingCalendarView.Type = com.Sconit.CodeMaster.WorkingCalendarType.Rest;
                                    workingCalendarViewList.Add(insertedWorkingCalendarView);
                                }
                                #endregion

                                lastWorkingCalendarView = currentWorkingCalendarView;
                                workingCalendarViewList.Add(currentWorkingCalendarView);
                            }
                            #endregion
                        }
                        else
                        {
                            #region 休息日
                            if (lastWorkingCalendarView != null)
                            {
                                #region 如果上次的工作日历的结束时间小于当前的工作日历，插入一条休息日记录
                                if (DateTime.Compare(lastWorkingCalendarView.DateTo, cycleDateTime.Date) < 0)
                                {
                                    WorkingCalendarView insertedWorkingCalendarView = new WorkingCalendarView();
                                    insertedWorkingCalendarView.DayOfWeek = lastWorkingCalendarView.DayOfWeek;
                                    insertedWorkingCalendarView.Date = lastWorkingCalendarView.Date;
                                    insertedWorkingCalendarView.DateFrom = lastWorkingCalendarView.DateTo;
                                    insertedWorkingCalendarView.DateTo = cycleDateTime;
                                    insertedWorkingCalendarView.Type = com.Sconit.CodeMaster.WorkingCalendarType.Rest;

                                    lastWorkingCalendarView = insertedWorkingCalendarView;
                                    workingCalendarViewList.Add(lastWorkingCalendarView);
                                }
                                #endregion

                                #region 新增休息日记录
                                WorkingCalendarView currentWorkingCalendarView = new WorkingCalendarView();
                                currentWorkingCalendarView.DayOfWeek = cycleDateTime.DayOfWeek;
                                currentWorkingCalendarView.Date = cycleDateTime;
                                currentWorkingCalendarView.DateFrom = lastWorkingCalendarView.DateTo;
                                currentWorkingCalendarView.DateTo = cycleDateTime.AddDays(1);
                                currentWorkingCalendarView.Type = com.Sconit.CodeMaster.WorkingCalendarType.Rest;

                                lastWorkingCalendarView = currentWorkingCalendarView;
                                workingCalendarViewList.Add(currentWorkingCalendarView);
                                #endregion
                            }
                            #endregion
                        }

                        #region 如果第一条工作日历不是从DateFrom开始，新增工作日历补齐
                        if (firstWorkingCalendarView == null && workingCalendarViewList.Count > 0)
                        {
                            firstWorkingCalendarView = workingCalendarViewList.OrderBy(c => c.DateFrom).Take(1).Single();
                        }

                        if (firstWorkingCalendarView == null || firstWorkingCalendarView.DateFrom > dateFrom)
                        {
                            #region 新增昨天的隔夜班次
                            WorkingCalendarView overNightWorkingCalendarView = null;
                            #region 查找昨天星期几
                            WorkingCalendar lastWorkingCalendar = (from w in workingCalendarList
                                                                   where w.DayOfWeek == cycleDateTime.AddDays(-1).DayOfWeek
                                                                   select w).SingleOrDefault();
                            #endregion

                            IList<WorkingShift> lastWorkingShiftList = (from w in workingShiftList
                                                                        where w.WorkingCalendar == lastWorkingCalendar.Id
                                                                        select w).ToList();

                            IList<ShiftDetail> lastShiftDetailList = new List<ShiftDetail>();
                            //(from w in lastWorkingShiftList
                            //                                          join s in shiftDetailList on w.Shift.Code equals s.Shift
                            //                                          where (!s.StartDate.HasValue || s.StartDate <= cycleDateTime)
                            //                                                 && (!s.EndDate.HasValue || s.EndDate >= cycleDateTime)
                            //                                          select s).OrderBy(s => s.ShiftTime).ToList();

                            if (lastShiftDetailList != null && lastShiftDetailList.Count > 0)
                            {
                                foreach (ShiftDetail lastShiftDetail in lastShiftDetailList)
                                {
                                    //string[] splitedShiftTime = lastShiftDetail.ShiftTime.Split(ShiftDetail.ShiftTimeSplitSymbol);

                                    //DateTime shiftDateFrom = Convert.ToDateTime(cycleDateTime.ToString("yyyy-MM-dd") + " " + splitedShiftTime[0]);
                                    //DateTime shiftDateTo = Convert.ToDateTime(cycleDateTime.ToString("yyyy-MM-dd") + " " + splitedShiftTime[1]);
                                    //if (DateTime.Compare(shiftDateFrom, shiftDateTo) >= 0  //如果开始日期大于等于结束日期，代表为隔夜的班次
                                    //    && shiftDateTo > dateFrom)
                                    //{
                                    //    DateTime lastDate = cycleDateTime.AddDays(-1);
                                    //    overNightWorkingCalendarView = new WorkingCalendarView();
                                    //    overNightWorkingCalendarView.ShiftCode = lastShiftDetail.Shift;
                                    //    overNightWorkingCalendarView.ShiftName = (from w in lastWorkingShiftList where w.Shift.Code == lastShiftDetail.Shift select w.Shift.Name).Single();
                                    //    overNightWorkingCalendarView.DayOfWeek = lastDate.DayOfWeek;
                                    //    overNightWorkingCalendarView.Date = lastDate;
                                    //    overNightWorkingCalendarView.DateFrom = dateFrom;
                                    //    overNightWorkingCalendarView.DateTo = shiftDateTo;
                                    //    overNightWorkingCalendarView.Type = com.Sconit.CodeMaster.WorkingCalendarType.Work;

                                    //    workingCalendarViewList.Add(overNightWorkingCalendarView);
                                    //    break;
                                    //}
                                }
                            }
                            #endregion

                            if (overNightWorkingCalendarView == null || //没有隔夜班
                                (firstWorkingCalendarView != null && overNightWorkingCalendarView.DateTo < firstWorkingCalendarView.DateFrom))
                            {
                                WorkingCalendarView insertedWorkingCalendarView = new WorkingCalendarView();
                                insertedWorkingCalendarView.DayOfWeek = cycleDateTime.DayOfWeek;
                                insertedWorkingCalendarView.Date = cycleDateTime;
                                insertedWorkingCalendarView.DateFrom = overNightWorkingCalendarView != null ? overNightWorkingCalendarView.DateTo : dateFrom;
                                insertedWorkingCalendarView.DateTo = firstWorkingCalendarView != null ? firstWorkingCalendarView.DateFrom : cycleDateTime.AddDays(1);
                                insertedWorkingCalendarView.Type = com.Sconit.CodeMaster.WorkingCalendarType.Rest;

                                workingCalendarViewList.Add(insertedWorkingCalendarView);
                            }

                            firstWorkingCalendarView = workingCalendarViewList.OrderBy(c => c.DateFrom).Take(1).Single();
                            lastWorkingCalendarView = workingCalendarViewList.OrderByDescending(c => c.DateTo).Take(1).Single();
                        }
                        #endregion
                    }
                }
                #endregion

                #region 循环特殊日期覆盖工作日历视图
                if (specialTimeList != null && specialTimeList.Count > 0)
                {
                    foreach (SpecialTime specialTime in specialTimeList.OrderBy(s => s.StartTime))
                    {
                        IList<WorkingCalendarView> overlapWorkingCalendarViewList = workingCalendarViewList.Where(c => specialTime.StartTime < c.DateTo && specialTime.EndTime >= c.DateFrom).OrderBy(c => c.DateFrom).ToList();

                        #region 删除重叠的工作日历
                        foreach (WorkingCalendarView overlapWorkingCalendarView in overlapWorkingCalendarViewList)
                        {
                            if (overlapWorkingCalendarView.DateFrom < specialTime.StartTime)
                            {
                                //如果工作日历的开始日期小于特殊日期的开始日期
                                //修改这个工作日历的结束日期等于特殊日期的开始日期
                                //这种情况出现在特殊日期开始时间出现在一段工作日历的中间
                                overlapWorkingCalendarView.DateTo = specialTime.StartTime;
                            }
                            else if (overlapWorkingCalendarView.DateTo > specialTime.EndTime)
                            {
                                //如果工作日历的结束日期大于特殊日期的结束日期
                                //修改这个工作日历的开始日期等于特殊日期的结束日期
                                //这种情况出现在特殊日期结束时间出现在一段工作日历的中间
                                overlapWorkingCalendarView.DateFrom = specialTime.EndTime;
                            }
                            else
                            {
                                workingCalendarViewList.Remove(overlapWorkingCalendarView);
                            }
                        }
                        #endregion

                        #region 添加特殊日期
                        DateTime cycleSpecialTime = new DateTime(specialTime.StartTime.Year, specialTime.StartTime.Month, specialTime.StartTime.Day);
                        if (cycleSpecialTime < dateFrom)
                        {
                            cycleSpecialTime = dateFrom;
                        }

                        DateTime endSpecialTime = new DateTime(specialTime.EndTime.Year, specialTime.EndTime.Month, specialTime.EndTime.Day);
                        if (dateFrom.Date < dateTo.Date)
                        {
                            endSpecialTime = dateTo;
                        }
                        for (; DateTime.Compare(cycleSpecialTime, endSpecialTime) <= 0 && DateTime.Compare(cycleSpecialTime, dateTo) <= 0; cycleSpecialTime = cycleSpecialTime.AddDays(1))
                        {
                            WorkingCalendarView specialWorkingCalendarView = new WorkingCalendarView();
                            specialWorkingCalendarView.DayOfWeek = cycleSpecialTime.DayOfWeek;
                            specialWorkingCalendarView.Date = cycleSpecialTime.Date;
                            if (cycleSpecialTime <= specialTime.StartTime)
                            {
                                specialWorkingCalendarView.DateFrom = specialTime.StartTime;
                            }
                            else
                            {
                                specialWorkingCalendarView.DateFrom = cycleSpecialTime;
                            }
                            if (cycleSpecialTime == endSpecialTime)
                            {
                                specialWorkingCalendarView.DateTo = specialTime.EndTime;
                            }
                            else
                            {
                                specialWorkingCalendarView.DateTo = cycleSpecialTime.AddDays(1);
                            }
                            specialWorkingCalendarView.Type = specialTime.Type;
                            workingCalendarViewList.Add(specialWorkingCalendarView);
                        }
                        #endregion
                    }
                }
                #endregion

                return workingCalendarViewList.OrderBy(c => c.DateFrom).ToList();
            }
            else
            {
                return new List<WorkingCalendarView>();
            }
        }

        public WorkingCalendarType GetWorkingCalendarType(string region, DateTime dateTime)
        {
            IList<WorkingCalendarView> workingCalendarViewList = GetWorkingCalendarView(region, dateTime.AddSeconds(-1), dateTime.AddSeconds(1));
            WorkingCalendarView workingCalendarView = workingCalendarViewList.Where(c => c.DateFrom <= dateTime && c.DateTo > dateTime).SingleOrDefault();

            if (workingCalendarView != null)
            {
                return workingCalendarView.Type;
            }

            return WorkingCalendarType.Rest;
        }

        #region 根据工作日历获取窗口时间
        public DateTime GetWindowTimeAtWorkingDate(DateTime baseDate, Double intervel, CodeMaster.TimeUnit intervelTimeUnit, string partyCode, IList<WorkingCalendarView> workingCalendarViewList)
        {
            if (intervel == 0)
            {
                return baseDate;
            }

            #region 先不考虑工作日历获取目标日期
            DateTime targetDateTime = baseDate;
            switch (intervelTimeUnit)
            {
                case com.Sconit.CodeMaster.TimeUnit.Day:
                    targetDateTime = baseDate.Add(TimeSpan.FromDays(intervel));
                    break;
                case com.Sconit.CodeMaster.TimeUnit.Hour:
                    targetDateTime = baseDate.Add(TimeSpan.FromHours(intervel));
                    break;
                case com.Sconit.CodeMaster.TimeUnit.Minute:
                    targetDateTime = baseDate.Add(TimeSpan.FromMinutes(intervel));
                    break;
                case com.Sconit.CodeMaster.TimeUnit.Second:
                    targetDateTime = baseDate.Add(TimeSpan.FromSeconds(intervel));
                    break;
            };
            #endregion

            #region 考虑工作日历重新在获取目标日期
            //DateTime dateTimeNow = DateTime.Now;
            //IList<WorkingCalendarView> workingCalendarViewList = this.GetWorkingCalendarView(partyCode, dateTimeNow, dateTimeNow.Add(TimeSpan.FromDays(7)));
            return NestGetWindowTimeAtWorkingDate(baseDate, targetDateTime, partyCode, workingCalendarViewList);
            #endregion
        }

        private DateTime NestGetWindowTimeAtWorkingDate(DateTime baseDate, DateTime targetDateTime, string partyCode, IList<WorkingCalendarView> workingCalendarViewList)
        {
            DateTime nextBaseDate = targetDateTime;

            //1. 查看目标日期落在工作日历哪里，如果是在工作期间内要在加上从基准日期至工作日期中的休息时间。
            //   如果中间没有休息时间，就得到目标日期。如果加上休息时间之后落在休息时间重复2，如果落在工作时间重复1
            //2. 如果落在休息日期中，则把目标日期改为之后离休息日期最近的工作日期，在加上从基准日期至工作日期中的休息时间。
            WorkingCalendarView workingCalendarView = workingCalendarViewList.Where(c => c.DateFrom < targetDateTime && c.DateTo >= targetDateTime).SingleOrDefault();

            if (workingCalendarView == null)
            {
                //如果没有找到工作日历，说明取的工作日历范围太小，需要重新加载工作日历
                workingCalendarViewList = ReloadWorkingCalendar(baseDate, targetDateTime, partyCode);

                //使用原参数重新计算
                return NestGetWindowTimeAtWorkingDate(baseDate, targetDateTime, partyCode, workingCalendarViewList);
            }
            else
            {
                if (workingCalendarView.Type == CodeMaster.WorkingCalendarType.Work)
                {
                    //累加休息日期
                    return AccumulateWindowTimeRestTime(baseDate, nextBaseDate, targetDateTime, workingCalendarViewList, partyCode);
                }
                else
                {
                    //查找离休息日期最新的工作日期
                    WorkingCalendarView nextWorkworkingCalendarView = workingCalendarViewList.Where(c => c.Type == CodeMaster.WorkingCalendarType.Work
                        //要用大于等于，考虑到下一个日期正好是工作日期
                        && c.DateFrom >= workingCalendarView.DateTo).OrderBy(c => c.DateFrom).Take(1).SingleOrDefault();

                    if (nextWorkworkingCalendarView == null)
                    {
                        //如果没有找到工作日历，说明取的工作日历范围太小，需要重新加载工作日历
                        WorkingCalendarView lastWorkingCalendarView = workingCalendarViewList.OrderByDescending(c => c.DateTo).Take(1).Single();

                        workingCalendarViewList = ReloadWorkingCalendar(baseDate, lastWorkingCalendarView.DateTo, partyCode);

                        //使用原参数重新计算
                        return NestGetWindowTimeAtWorkingDate(baseDate, targetDateTime, partyCode, workingCalendarViewList);
                    }
                    else
                    {
                        targetDateTime = nextWorkworkingCalendarView.DateFrom;

                        //累加休息日期
                        return AccumulateWindowTimeRestTime(baseDate, nextBaseDate, targetDateTime, workingCalendarViewList, partyCode);
                    }
                }
            }
        }

        private DateTime AccumulateWindowTimeRestTime(DateTime baseDateTime, DateTime orgTargetDateTime, DateTime targetDateTime, IList<WorkingCalendarView> workingCalendarViewList, string partyCode)
        {
            //查找休息日期
            //1. 结束日期大于基准日期
            //2. 结束日期小于等于目标日期
            IList<WorkingCalendarView> restWorkingCalendarViewList = workingCalendarViewList.Where(
                c => c.DateTo > baseDateTime && c.Type == CodeMaster.WorkingCalendarType.Rest && c.DateFrom < orgTargetDateTime
                ).ToList();

            DateTime nextBaseDateTime = targetDateTime;
            if (restWorkingCalendarViewList != null && restWorkingCalendarViewList.Count > 0)
            {
                foreach (WorkingCalendarView restWorkingCalendarView in restWorkingCalendarViewList)
                {
                    if (restWorkingCalendarView.DateFrom < baseDateTime)
                    {
                        if (restWorkingCalendarView.DateTo > orgTargetDateTime)
                        {
                            //基准时间和原目标时间落在休息日期区间之内，只增加原目标时间和基准时间的休息时间间隔
                            targetDateTime = targetDateTime.Add(orgTargetDateTime.Subtract(baseDateTime));
                        }
                        else
                        {
                            //基准时间大于休息日期的开始时间，原目标日期大于休息日期的结束时间，增加基准时间和休息日期结束时间的时间间隔
                            targetDateTime = targetDateTime.Add(restWorkingCalendarView.DateTo.Subtract(baseDateTime));
                        }
                    }
                    else
                    {
                        if (restWorkingCalendarView.DateTo > orgTargetDateTime)
                        {
                            //基准时间小于休息日期的开始时间，原目标日期小于休息日期的结束时间，增加休息日期开始时间和原目标日期的时间间隔
                            targetDateTime = targetDateTime.Add(orgTargetDateTime.Subtract(restWorkingCalendarView.DateFrom));
                        }
                        else
                        {
                            //基准时间大于休息日期的开始时间，原目标日期小于休息日期的结束时间，增加休息日期开始时间和结束时间的时间间隔
                            targetDateTime = targetDateTime.Add(restWorkingCalendarView.DateTo.Subtract(restWorkingCalendarView.DateFrom));
                        }
                    }
                }

                //用原目标日期作为基准日期重新迭代计算
                return NestGetWindowTimeAtWorkingDate(nextBaseDateTime, targetDateTime, partyCode, workingCalendarViewList);
            }
            else
            {
                //目标日期和基准日期没有休息日期间隔，得到最终计算结果
                return targetDateTime;
            }
        }

        private IList<WorkingCalendarView> ReloadWorkingCalendar(DateTime baseDate, DateTime dateTo, string partyCode)
        {
            //重新加载工作日历
            if (baseDate < dateTo)
            {
                DateTime endDateTime = new DateTime(dateTo.Year, dateTo.Month, dateTo.Day).AddDays(7); //向后加载一周的工作日历
                return this.GetWorkingCalendarView(partyCode, baseDate, endDateTime);
            }
            else
            {
                DateTime startDateTime = new DateTime(dateTo.Year, dateTo.Month, dateTo.Day).AddDays(-7); //向前加载一周的工作日历
                return this.GetWorkingCalendarView(partyCode, startDateTime, baseDate);
            }
        }
        #endregion

        #region 根据工作日历获取开始时间
        public DateTime GetStartTimeAtWorkingDate(DateTime baseDate, Double intervel, CodeMaster.TimeUnit intervelTimeUnit, string partyCode, IList<WorkingCalendarView> workingCalendarViewList)
        {
            if (intervel == 0)
            {
                return baseDate;
            }

            #region 先不考虑工作日历获取目标日期
            DateTime targetDateTime = baseDate;
            switch (intervelTimeUnit)
            {
                case com.Sconit.CodeMaster.TimeUnit.Day:
                    targetDateTime = baseDate.Add(TimeSpan.FromDays(-intervel));
                    break;
                case com.Sconit.CodeMaster.TimeUnit.Hour:
                    targetDateTime = baseDate.Add(TimeSpan.FromHours(-intervel));
                    break;
                case com.Sconit.CodeMaster.TimeUnit.Minute:
                    targetDateTime = baseDate.Add(TimeSpan.FromMinutes(-intervel));
                    break;
                case com.Sconit.CodeMaster.TimeUnit.Second:
                    targetDateTime = baseDate.Add(TimeSpan.FromSeconds(-intervel));
                    break;
            };
            #endregion

            #region 考虑工作日历重新在获取目标日期
            return NestGetStartTimeAtWorkingDate(baseDate, targetDateTime, partyCode, workingCalendarViewList);
            #endregion
        }

        private DateTime NestGetStartTimeAtWorkingDate(DateTime baseDate, DateTime targetDateTime, string partyCode, IList<WorkingCalendarView> workingCalendarViewList)
        {
            DateTime nextBaseDate = targetDateTime;

            //1. 查看目标日期落在工作日历哪里，如果是在工作期间内要在加上从基准日期至工作日期中的休息时间。
            //   如果中间没有休息时间，就得到目标日期。如果加上休息时间之后落在休息时间重复2，如果落在工作时间重复1
            //2. 如果落在休息日期中，则把目标日期改为之后离休息日期最近的工作日期，在加上从基准日期至工作日期中的休息时间。
            WorkingCalendarView workingCalendarView = workingCalendarViewList.Where(c => c.DateFrom < targetDateTime && c.DateTo >= targetDateTime).SingleOrDefault();

            if (workingCalendarView == null)
            {
                //如果没有找到工作日历，说明取的工作日历范围太小，需要重新加载工作日历
                workingCalendarViewList = ReloadWorkingCalendar(baseDate, targetDateTime, partyCode);

                //使用原参数重新计算
                return NestGetStartTimeAtWorkingDate(baseDate, targetDateTime, partyCode, workingCalendarViewList);
            }
            else
            {
                if (workingCalendarView.Type == CodeMaster.WorkingCalendarType.Work)
                {
                    //累加休息日期
                    return AccumulateStartTimeRestTime(baseDate, nextBaseDate, targetDateTime, workingCalendarViewList, partyCode);
                }
                else
                {
                    //查找离休息日期最新的工作日期
                    WorkingCalendarView previousWorkworkingCalendarView = workingCalendarViewList.Where(c => c.Type == CodeMaster.WorkingCalendarType.Work
                        //要用小于等于，考虑到上一个日期正好是工作日期
                        && c.DateTo <= workingCalendarView.DateFrom).OrderBy(c => c.DateTo).Take(1).SingleOrDefault();

                    if (previousWorkworkingCalendarView == null)
                    {
                        //如果没有找到工作日历，说明取的工作日历范围太小，需要重新加载工作日历
                        WorkingCalendarView lastWorkingCalendarView = workingCalendarViewList.OrderBy(c => c.DateFrom).Take(1).Single();

                        workingCalendarViewList = ReloadWorkingCalendar(baseDate, lastWorkingCalendarView.DateFrom, partyCode);

                        //使用原参数重新计算
                        return NestGetStartTimeAtWorkingDate(baseDate, targetDateTime, partyCode, workingCalendarViewList);
                    }
                    else
                    {
                        targetDateTime = previousWorkworkingCalendarView.DateTo;

                        //累加休息日期
                        return AccumulateStartTimeRestTime(baseDate, nextBaseDate, targetDateTime, workingCalendarViewList, partyCode);
                    }
                }
            }
        }

        private DateTime AccumulateStartTimeRestTime(DateTime baseDateTime, DateTime orgTargetDateTime, DateTime targetDateTime, IList<WorkingCalendarView> workingCalendarViewList, string partyCode)
        {
            //查找休息日期
            //1. 结束日期大于基准日期
            //2. 结束日期小于等于目标日期
            IList<WorkingCalendarView> restWorkingCalendarViewList = workingCalendarViewList.Where(
                c => c.DateTo > orgTargetDateTime && c.Type == CodeMaster.WorkingCalendarType.Rest && c.DateFrom < baseDateTime
                ).ToList();

            DateTime previousBaseDateTime = targetDateTime;
            if (restWorkingCalendarViewList != null && restWorkingCalendarViewList.Count > 0)
            {
                foreach (WorkingCalendarView restWorkingCalendarView in restWorkingCalendarViewList)
                {
                    if (restWorkingCalendarView.DateFrom < orgTargetDateTime)
                    {
                        if (restWorkingCalendarView.DateTo > baseDateTime)
                        {
                            //基准时间和原目标时间落在休息日期区间之内，只增加原目标时间和基准时间的休息时间间隔
                            targetDateTime = targetDateTime.Subtract(baseDateTime.Subtract(orgTargetDateTime));
                        }
                        else
                        {
                            //基准时间大于休息日期的开始时间，原目标日期大于休息日期的结束时间，增加基准时间和休息日期结束时间的时间间隔
                            targetDateTime = targetDateTime.Subtract(restWorkingCalendarView.DateTo.Subtract(orgTargetDateTime));
                        }
                    }
                    else
                    {
                        if (restWorkingCalendarView.DateTo > baseDateTime)
                        {
                            //基准时间小于休息日期的开始时间，原目标日期小于休息日期的结束时间，增加休息日期开始时间和原目标日期的时间间隔
                            targetDateTime = targetDateTime.Subtract(baseDateTime.Subtract(restWorkingCalendarView.DateFrom));
                        }
                        else
                        {
                            //基准时间大于休息日期的开始时间，原目标日期小于休息日期的结束时间，增加休息日期开始时间和结束时间的时间间隔
                            targetDateTime = targetDateTime.Subtract(restWorkingCalendarView.DateTo.Subtract(restWorkingCalendarView.DateFrom));
                        }
                    }
                }

                //用原目标日期作为基准日期重新迭代计算
                return NestGetWindowTimeAtWorkingDate(previousBaseDateTime, targetDateTime, partyCode, workingCalendarViewList);
            }
            else
            {
                //目标日期和基准日期没有休息日期间隔，得到最终计算结果
                return targetDateTime;
            }
        }

        #endregion

        #region 生成工作日历
        public void CreateWorkingCalendar(WorkingCalendar workingCalendar)
        {
            var isProdLine = workingCalendar.Category == CodeMaster.WorkingCalendarCategory.ProdLine;
            var baseParams = new object[] { isProdLine ? workingCalendar.ProdLine : workingCalendar.Region, workingCalendar.Category };

            var items = this.genericMgr.FindAll<StandardWorkingCalendar>(isProdLine ? SelectStandardWorkingCalendarsStatementByProdLine : SelectStandardWorkingCalendarsStatement, baseParams);

            if (items.Count != 7)
            {
                if (isProdLine)
                {
                    throw new BusinessException(Resources.PRD.StandardWorkingCalendar.StandardWorkingCalendar_Errors_IncompleteInTheProdLine, workingCalendar.ProdLine);
                }
                throw new BusinessException(Resources.PRD.StandardWorkingCalendar.StandardWorkingCalendar_Errors_IncompleteInTheRegion, workingCalendar.Region);
            }

            //var now = DateTime.Now;
            //var startDate = now.Date; // 工作日历从当天开始创建或修改
            var startDate = workingCalendar.StartWorkingDate.Value;
            var endDate = workingCalendar.EndDate.Value; // 维护到指定日期

            var calendars = this.genericMgr.FindAll<WorkingCalendar>(isProdLine ? SelectWorkingStatementByProdLine : SelectWorkingStatement,
                    new object[]
                        {
                            isProdLine ? workingCalendar.ProdLine : workingCalendar.Region, workingCalendar.Category,
                            startDate, endDate
                        });

            while (startDate <= endDate)
            {
                var sameItem = items.FirstOrDefault(c => c.DayOfWeek == startDate.DayOfWeek);
                if (sameItem == null)
                {
                    if (isProdLine)
                    {
                        throw new BusinessException(Resources.PRD.StandardWorkingCalendar.StandardWorkingCalendar_Errors_IncompleteInTheProdLine, workingCalendar.ProdLine);
                    }
                    throw new BusinessException(Resources.PRD.StandardWorkingCalendar.StandardWorkingCalendar_Errors_IncompleteInTheRegion, workingCalendar.Region);
                }

                var instance = calendars.FirstOrDefault(c => c.WorkingDate == startDate);
                if (instance != null)
                {
                    instance.Shift = sameItem.Shift;
                    instance.Type = sameItem.Type;
                    this.genericMgr.Update(instance);
                }
                else
                {
                    var calendar = new WorkingCalendar
                    {
                        Region = workingCalendar.Region,
                        RegionName = sameItem.RegionName,
                        ProdLine = workingCalendar.ProdLine,
                        Shift = sameItem.Shift,
                        WorkingDate = startDate,
                        Type = sameItem.Type,
                        DayOfWeek = sameItem.DayOfWeek,
                        Category = sameItem.Category
                    };

                    this.genericMgr.Create(calendar);
                }

                startDate = startDate.AddDays(1);
            }
        }

        public void UpdateWorkingCalendar(WorkingCalendar workingCalendar)
        {
            this.genericMgr.Update(workingCalendar);
            this.genericMgr.FlushSession();

            try
            {
                if (workingCalendar.Category == WorkingCalendarCategory.ProdLine)
                {
                    var user = SecurityContextHolder.Get();
                    //this.genericMgr.UpdateWithNativeQuery("exec USP_Busi_UpdateOrderBomConsumeTime ?,?,?", new object[] { workingCalendar.ProdLine??string.Empty, user.Id, user.FullName });
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.InnerException != null)
                    {
                        throw new BusinessException(ex.InnerException.InnerException.Message);
                    }
                    else
                    {
                        throw new BusinessException(ex.InnerException.Message);
                    }
                }
                else
                {
                    throw new BusinessException(ex.Message);
                }
            }
        }
        #endregion

        #region 生产线 停线/加班时间
        public void AddProdLineSpecialTime(SpecialTime specialTime)
        {
            this.genericMgr.Create(specialTime);
            this.genericMgr.FlushSession();
            try
            {
                var user = SecurityContextHolder.Get();
                //this.genericMgr.UpdateWithNativeQuery("exec USP_Busi_UpdateOrderBomConsumeTime ?,?,?", new object[] { specialTime.ProdLine ?? string.Empty, user.Id, user.FullName });
            }
            catch (Exception ex)
            {
                throw new BusinessException(ex);
            }
        }

        public void UpdateProdLineSpecialTime(SpecialTime specialTime)
        {
            this.genericMgr.Update(specialTime);
            this.genericMgr.FlushSession();
            try
            {
                var user = SecurityContextHolder.Get();
                //this.genericMgr.UpdateWithNativeQuery("exec USP_Busi_UpdateOrderBomConsumeTime ?,?,?", new object[] { specialTime.ProdLine ?? string.Empty, user.Id, user.FullName });
            }
            catch (Exception ex)
            {
                throw new BusinessException(ex);
            }
        }

        public void DeleteProdLineSpecialTime(SpecialTime specialTime)
        {
            this.genericMgr.Delete(specialTime);
            this.genericMgr.FlushSession();
            try
            {
                var user = SecurityContextHolder.Get();
                //this.genericMgr.UpdateWithNativeQuery("exec USP_Busi_UpdateOrderBomConsumeTime ?,?,?", new object[] { specialTime.ProdLine ?? string.Empty, user.Id, user.FullName });
            }
            catch (Exception ex)
            {
                throw new BusinessException(ex);
            }
        }
        #endregion
        #endregion

        #region private method

        #endregion

        [Transaction(TransactionMode.Requires)]
        public void CreateShiftMasterAndShiftDetail(ShiftMaster shiftMaster, ShiftDetail shiftDetail)
        {
            this.genericMgr.Create(shiftMaster);
            this.genericMgr.Create(shiftDetail);
        }
    }

    class WorkingCalendarComparer : IEqualityComparer<WorkingCalendar>
    {

        public bool Equals(WorkingCalendar x, WorkingCalendar y)
        {
            return x.DayOfWeek == y.DayOfWeek;
        }

        public int GetHashCode(WorkingCalendar obj)
        {
            return obj.DayOfWeek.GetHashCode();
        }
    }

    class SpecialTimeComparer : IEqualityComparer<SpecialTime>
    {

        public bool Equals(SpecialTime x, SpecialTime y)
        {
            return x.StartTime == y.StartTime;
        }

        public int GetHashCode(SpecialTime obj)
        {
            return obj.StartTime.GetHashCode();
        }
    }
}
