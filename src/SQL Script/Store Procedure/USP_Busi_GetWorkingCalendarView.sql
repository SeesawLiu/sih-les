SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.OBJECTS WHERE TYPE='P' AND name='USP_Busi_GetWorkingCalendarView')
	DROP PROCEDURE USP_Busi_GetWorkingCalendarView	
GO

CREATE PROCEDURE [dbo].[USP_Busi_GetWorkingCalendarView]
(
	@ProdLine varchar(50),
	@Region varchar(50),
	@DateFrom datetime,
	@DateTo datetime
) --WITH ENCRYPTION
AS
BEGIN
	set nocount on
	declare @WorkingDateStartTimeMi int
	exec USP_Busi_GetStartTimeAsMinute @WorkingDateStartTimeMi output
	
	create table #tempWorkingCalendar2
	(
		WCRowId int Identity(1, 1),
		WorkingDate DateTime,
		DateFrom DateTime,
		DateTo DateTime,
		DateFromTimeMi int,
		DateToTimeMi int
	)
	
	--开始日期减一天，为了加入前一天的隔夜班
	declare @DateFrom2 DateTime = DATEADD(DD, -1, @DateFrom)
	
	--查找工作日历
	if ISNULL(@ProdLine, '') <> ''
	begin
		insert into #tempWorkingCalendar2(WorkingDate, DateFrom, DateTo)
		select w.WorkingDate,
		DateAdd(MI, cast(substring(d.StartTime, charindex(':', d.StartTime) + 1, len(d.StartTime) - charindex(':', d.StartTime)) as int), DateAdd(HH, cast(substring(d.StartTime, 1, charindex(':', d.StartTime) - 1) as int), cast(w.WorkingDate as datetime))) as DateFrom,
		DateAdd(MI, cast(substring(d.EndTime, charindex(':', d.EndTime) + 1, len(d.EndTime) - charindex(':', d.EndTime)) as int), DateAdd(HH, cast(substring(d.EndTime, 1, charindex(':', d.EndTime) - 1) as int), cast(w.WorkingDate as datetime))) as DateTo
		from PRD_WorkingCalendar as w
		inner join PRD_ShiftDet as d on w.Shift = d.Shift
		where w.WorkingDate between convert(datetime, convert(varchar(10), @DateFrom2, 101), 120) and convert(datetime, convert(varchar(10), @DateTo, 101), 120)
		and w.ProdLine = @ProdLine
		and w.[Type] = 0  --工作
	end
	else
	begin
		insert into #tempWorkingCalendar2(WorkingDate, DateFrom, DateTo)
		select w.WorkingDate,
		DateAdd(MI, cast(substring(d.StartTime, charindex(':', d.StartTime) + 1, len(d.StartTime) - charindex(':', d.StartTime)) as int), DateAdd(HH, cast(substring(d.StartTime, 1, charindex(':', d.StartTime) - 1) as int), cast(w.WorkingDate as datetime))) as DateFrom,
		DateAdd(MI, cast(substring(d.EndTime, charindex(':', d.EndTime) + 1, len(d.EndTime) - charindex(':', d.EndTime)) as int), DateAdd(HH, cast(substring(d.EndTime, 1, charindex(':', d.EndTime) - 1) as int), cast(w.WorkingDate as datetime))) as DateTo
		from PRD_WorkingCalendar as w
		inner join PRD_ShiftDet as d on w.Shift = d.Shift
		where w.WorkingDate between convert(datetime, convert(varchar(10), @DateFrom2, 101), 120) and convert(datetime, convert(varchar(10), @DateTo, 101), 120)
		and w.Region = @Region
		and w.[Type] = 0  --工作
	end
			
	if exists(select top 1 WCRowId from #tempWorkingCalendar2)
	begin
		--设置开始时间和结束时间的时间部分，转换为分钟
		update #tempWorkingCalendar2 set DateFromTimeMi = DATEPART(HH, DateFrom) * 60 + DATEPART(MI, DateFrom), DateToTimeMi = DATEPART(HH, DateTo) * 60 + DATEPART(MI, DateTo)
		
		--如果开始时间大于等于结束时间，结束时间加1天
		update #tempWorkingCalendar2 set DateTo = DATEADD(D, 1, DateTo) where DateFromTimeMi >= DateToTimeMi
		
		--如果开始时间小于每天的开始时间，结束时间小于等于每天的开始时间
		update #tempWorkingCalendar2 set DateFrom = DATEADD(D, 1, DateFrom), DateTo = DATEADD(D, 1, DateTo) where DateFromTimeMi < @WorkingDateStartTimeMi and DateToTimeMi <= @WorkingDateStartTimeMi

		--校验每天的开始时间不能落在在工作日历的开始时间和结束时间之间
		if exists(select top 1 * from #tempWorkingCalendar2 
						 where DateFromTimeMi < @WorkingDateStartTimeMi and DateToTimeMi > @WorkingDateStartTimeMi)
		begin
			RAISERROR(N'每天的开始时间不能在工作日历的开始时间和结束时间之间。', 16, 1)
			return
		end
		
		create table #tempRestSpecialTime
		(
			STRowId int identity(1, 1),
			StartTime datetime,
			EndTime datetime
		)
	
		if ISNULL(@ProdLine, '') <> ''
		begin
			--查找休息日期
			insert into #tempRestSpecialTime(StartTime, EndTime)
			select StartTime, EndTime from MD_SpecialTime
			where StartTime < @DateTo
			and EndTime > @DateFrom
			and [Type] = 1 --休息
			and ProdLine = @ProdLine
		end
		else
		begin
			--查找休息日期
			insert into #tempRestSpecialTime(StartTime, EndTime)
			select StartTime, EndTime from MD_SpecialTime
			where StartTime < @DateTo
			and EndTime > @DateFrom
			and [Type] = 1 --休息
			and Region = @Region
		end
		
		--根据工作时间过滤掉休息日期
		Declare @CycleWDRowId int = 1
		Declare @MaxWDRowId  int = 1
		Declare @CycleSTRowId int = 1
		Declare @MaxSTRowId  int = 1
		Declare @WDDateFrom datetime
		Declare @WDDateTo datetime
		Declare @STStartTime datetime
		Declare @STEndTime datetime	
	
		select @CycleWDRowId = MIN(WCRowId), @MaxWDRowId = MAX(WCRowId) from #tempWorkingCalendar2
		while @MaxWDRowId >= @CycleWDRowId
		begin
			select @WDDateFrom = DateFrom, @WDDateTo = DateTo from #tempWorkingCalendar2 where WCRowId = @CycleWDRowId
			 
			if (@DateFrom >= @WDDateTo)
			begin --@WDDateFrom < @WDDateTo <= @DateFrom < @DateTo
				delete from #tempWorkingCalendar2 where WCRowId = @CycleWDRowId
				set @CycleWDRowId = @CycleWDRowId + 1
				continue
			end
			else if (@DateTo <= @WDDateFrom)
			begin --@DateFrom < @DateTo <= @WDDateFrom < @WDDateTo
				delete from #tempWorkingCalendar2 where WCRowId = @CycleWDRowId
				set @CycleWDRowId = @CycleWDRowId + 1
				continue
			end
			else if @WDDateFrom <= @DateFrom and @WDDateTo >= @DateTo
			begin  --@WDDateFrom <= @DateFrom < @DateTo <= @WDDateTo
				update #tempWorkingCalendar2 set DateFrom = @DateFrom, DateTo = @DateTo where WCRowId = @CycleWDRowId
				set @WDDateFrom = @DateFrom
				set @WDDateTo = @DateTo
			end
			else if @WDDateFrom <= @DateFrom and @WDDateTo < @DateTo
			begin  --@WDDateFrom<= @DateFrom < @WDDateTo < @DateTo
				update #tempWorkingCalendar2 set DateFrom = @DateFrom where WCRowId = @CycleWDRowId
				set @WDDateFrom = @DateFrom
			end
			else if (@WDDateFrom > @DateFrom and @WDDateTo >= @DateTo)
			begin  --@DateFrom < @WDDateFrom < @DateTo <= @WDDateTo
				update #tempWorkingCalendar2 set DateTo = @DateTo where WCRowId = @CycleWDRowId
				set @WDDateTo = @DateTo
			end
			else 
			begin  --@DateFrom < @WDDateFrom < @WDDateTo < @DateTo 
				set @WDDateFrom = @WDDateFrom
			end
			 
			if exists(select top 1 STRowId from #tempRestSpecialTime)
			begin
				select @CycleSTRowId = MIN(STRowId), @MaxSTRowId = MAX(STRowId) from #tempRestSpecialTime
				while @MaxSTRowId >= @CycleSTRowId
				begin
					select @STStartTime = StartTime, @STEndTime = EndTime from #tempRestSpecialTime where STRowId = @CycleSTRowId
					
					if (@WDDateFrom >= @STEndTime)
					begin --StartTime< EndTime <= DateFrom < DateTo
						set @CycleSTRowId = @CycleSTRowId
					end
					else if (@WDDateTo <= @STStartTime)
					begin --DateFrom < DateTo <= StartTime< EndTime
						set @CycleSTRowId = @CycleSTRowId
					end
					else if @STStartTime <= @WDDateFrom and @STEndTime >= @WDDateTo
					begin  --休息时间覆盖开始和结束日期（StartTime<= DateFrom < DateTo <= EndTime）直接把开始日期赋值为结束日期
						update #tempWorkingCalendar2 set DateFrom = DateTo where WCRowId = @CycleWDRowId
					end
					else if @STStartTime <= @WDDateFrom and @STEndTime < @WDDateTo
					begin  --StartTime<= DateFrom < EndTime < DateTo
						update #tempWorkingCalendar2 set DateFrom = @STEndTime where WCRowId = @CycleWDRowId
					end
					else if (@STStartTime > @WDDateFrom and @STEndTime >= @WDDateTo)
					begin  --DateFrom < StartTime < DateTo <= EndTime
						update #tempWorkingCalendar2 set DateTo = @STStartTime where WCRowId = @CycleWDRowId
					end
					else 
					begin  --DateFrom < StartTime < EndTime < DateTo 
						--把当前的工作日期拆分为两段，第一段的结束时间为休息时间的开始时间
						update #tempWorkingCalendar2 set DateTo = @STStartTime where WCRowId = @CycleWDRowId
						
						-- 第二段的开始时间为休息时间的开始时间
						--因为休息时间的循环是排序的，并且不重叠，所以用第二段时间作为下个休息时间的参照
						insert into #tempWorkingCalendar2(WorkingDate, DateFrom, DateTo)
						select WorkingDate, @STEndTime, @WDDateTo
						from #tempWorkingCalendar2 where WCRowId = @CycleWDRowId
					end
					
					set @CycleSTRowId = @CycleSTRowId + 1
				end
			end
			
			set @CycleWDRowId = @CycleWDRowId + 1
		end
		
		drop table #tempRestSpecialTime
		
		--删除开始日期大于等于结束日期的记录
		delete from #tempWorkingCalendar2 where DateFrom >= DateTo
		
		create table #tempWorkSpecialTime
		(
			STRowId int identity(1, 1),
			StartTime datetime,
			EndTime datetime
		)
	
		if ISNULL(@ProdLine, '') <> ''
		begin
			--查找加班日期
			insert into #tempWorkSpecialTime(StartTime, EndTime)
			select StartTime, EndTime from MD_SpecialTime
			where StartTime < @DateTo
			and EndTime > @DateFrom
			and [Type] = 0 --工作
			and ProdLine = @ProdLine
		end
		else
		begin
			--查找加班日期
			insert into #tempWorkSpecialTime(StartTime, EndTime)
			select StartTime, EndTime from MD_SpecialTime
			where StartTime < @DateTo
			and EndTime > @DateFrom
			and [Type] = 0 --工作
			and Region = @Region
		end
	
		if exists(select top 1 * from #tempWorkSpecialTime)
		begin  --增加加班时间
			select @CycleSTRowId = MIN(STRowId), @MaxSTRowId = MAX(STRowId) from #tempWorkSpecialTime
			while @MaxSTRowId >= @CycleSTRowId
			begin
				select @STStartTime = StartTime, @STEndTime = EndTime from #tempWorkSpecialTime where STRowId = @CycleSTRowId
				
				if @STStartTime < @DateFrom
				begin
					set @STStartTime = @DateFrom
				end
				
				if @STEndTime > @DateTo 
				begin
					set @STEndTime = @DateTo
				end
				
				select @CycleWDRowId = MIN(WCRowId), @MaxWDRowId = MAX(WCRowId) from #tempWorkingCalendar2
				while @MaxWDRowId >= @CycleWDRowId
				begin
					if @STStartTime >= @STEndTime
					begin  --工作开始时间大于等于工作结束时间，跳出循环
						break
					end
					
					select @WDDateFrom = DateFrom, @WDDateTo = DateTo from #tempWorkingCalendar2 where WCRowId = @CycleWDRowId
				
					--@STStartTime < @WDDateFrom < @STEndTime <= @WDDateTo
					if @STStartTime < @WDDateFrom and @WDDateFrom < @STEndTime and @STEndTime <= @WDDateTo
					begin
						set @STEndTime = @WDDateFrom
					end
					--@STStartTime < @WDDateFrom < @WDDateTo < @STEndTime
					else if @STStartTime < @WDDateFrom and @WDDateTo < @STEndTime
					begin
						delete from #tempWorkingCalendar2 where WCRowId = @CycleWDRowId
					end
					--@WDDateFrom <= @STStartTime < @STEndTime <= @WDDateTo
					else if @WDDateFrom <= @STStartTime and @STEndTime <= @WDDateTo
					begin
						set @STEndTime = @STStartTime
					end
					--@WDDateFrom <= @STStartTime < @WDDateTo < @STEndTime
					else if @WDDateFrom <= @STStartTime and @STStartTime < @WDDateTo and @WDDateTo < @STEndTime
					begin
						set @STStartTime = @WDDateTo
					end
					
					set @CycleWDRowId = @CycleWDRowId + 1
				end 
				
				if (@STStartTime < @STEndTime)
				begin
					insert into #tempWorkingCalendar2(WorkingDate, DateFrom, DateTo) values(convert(datetime, convert(varchar(10), @STStartTime, 101), 120), @STStartTime, @STEndTime)
				end
				
				set @CycleSTRowId = @CycleSTRowId + 1
			end
		end
		
		drop table #tempWorkSpecialTime
	end		
	
	select WorkingDate, DateFrom, DateTo from #tempWorkingCalendar2 
	where DateFrom < DateTo order by DateFrom
	
	drop table #tempWorkingCalendar2		
END

GO