using System;
using System.Collections.Generic;
using com.Sconit.Entity.VIEW;
using com.Sconit.Entity.PRD;
using com.Sconit.CodeMaster;

namespace com.Sconit.Service
{
    public interface IWorkingCalendarMgr
    {
        void DeleteShiftMaster(string shiftMasterCode);
        void UpdateWorkingCalendar(WorkingCalendar workingCalendar, IList<string> ShiftList);
        IList<WorkingCalendarView> GetWorkingCalendarView(string regionCode, DateTime dateFrom, DateTime dateTo);
        void CreateShiftMasterAndShiftDetail(ShiftMaster shiftMaster, ShiftDetail shiftDetail);
        DateTime GetWindowTimeAtWorkingDate(DateTime baseDate, Double intervel, CodeMaster.TimeUnit intervelTimeUnit, string partyCode, IList<WorkingCalendarView> workingCalendarViewList);
        DateTime GetStartTimeAtWorkingDate(DateTime baseDate, Double intervel, CodeMaster.TimeUnit intervelTimeUnit, string partyCode, IList<WorkingCalendarView> workingCalendarViewList);
        WorkingCalendarType GetWorkingCalendarType(string region, DateTime dateTime);

        void CreateWorkingCalendar(WorkingCalendar workingCalendar);
        void UpdateWorkingCalendar(WorkingCalendar workingCalendar);
        void AddProdLineSpecialTime(SpecialTime specialTime);
        void UpdateProdLineSpecialTime(SpecialTime specialTime);
        void DeleteProdLineSpecialTime(SpecialTime specialTime);
    }
}
