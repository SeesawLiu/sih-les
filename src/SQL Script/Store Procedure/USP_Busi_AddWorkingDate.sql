SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Busi_AddWorkingDate') 
     DROP PROCEDURE USP_Busi_AddWorkingDate 
GO

CREATE PROCEDURE [dbo].[USP_Busi_AddWorkingDate] 
(
	@BaseDate datetime, 
	@AddTime int,  --时间单位为秒
	@ProdLine varchar(50), 
	@Region varchar(50), 
	@ReturnDate datetime output 
) --WITH ENCRYPTION
AS 
BEGIN 
	set nocount on
	begin try
		Create table #tempWorkingCalendar 
		( 
			RowId int identity(1, 1), 
			WorkingDate datetime, 
			DateFrom datetime, 
			DateTo datetime, 
		) 
		Declare @ToDate datetime = @BaseDate 
		Declare @TargetDate datetime = @BaseDate 
		Declare @CycleWDRowId int = 1 
		Declare @MaxWDRowId  int = 1 
		Declare @DateFrom datetime 
		Declare @DateTo datetime 
		Declare @CycleCount int = 0 
		Declare @ErrorMsg varchar(max)

		if @AddTime = 0 
		begin 
			set @ReturnDate = @ToDate 
			return 
		end 
		else if @AddTime < 0 
		begin 
			RAISERROR(N'间隔时间不能小于0。', 16, 1) 
		end 

		while @AddTime > 0 
		begin 
			set @ToDate = DateAdd(SECOND, @AddTime, @ToDate) 
			--如果基准日期和截止日期是同一天，截止日期+1天 
			if convert(varchar(20), @ToDate, 112) = convert(varchar(20), @BaseDate, 112) 
			begin 
				set @ToDate = DateAdd(D, 1, @ToDate) 
			end        
			--获取基准日期和截止日期之间的工作时间段 
			truncate table #tempWorkingCalendar
			insert into #tempWorkingCalendar(WorkingDate, DateFrom, DateTo) exec USP_Busi_GetWorkingCalendarView @ProdLine, @Region, @BaseDate, @ToDate 
			
			if exists(select top 1 RowId from #tempWorkingCalendar)
			begin
				select @CycleWDRowId = MIN(RowId), @MaxWDRowId = MAX(RowId) from #tempWorkingCalendar 
				while @MaxWDRowId >= @CycleWDRowId 
				begin 
					select @DateFrom = DateFrom, @DateTo = DateTo from #tempWorkingCalendar where RowId = @CycleWDRowId 
					set @TargetDate = DATEADD(SECOND, @AddTime, @DateFrom) 

					if @TargetDate <= @DateTo 
					begin 
						set @ReturnDate = @TargetDate 
						return 
					end 
					else 
					begin 
						set @AddTime = DATEDIFF(SECOND, @DateTo, @TargetDate) 
						set @BaseDate = @DateTo 
						set @ToDate = @DateTo 
					end 

					set @CycleWDRowId = @CycleWDRowId + 1 
				end 
			end
			else
			begin --基准日期和截止日期之间没有工作时间
				--为了避免@AddTime太小，循环999次都没有找到工作时间
				--这里通过@DateTo + 1天来找之前的工作结束时间
				declare @ToDatePlus1 datetime = @ToDate
				
				while 1 = 1
				begin
					set @ToDatePlus1 = DATEADD(DAY, 1, @ToDatePlus1)
					
					truncate table #tempWorkingCalendar
					insert into #tempWorkingCalendar(WorkingDate, DateFrom, DateTo) exec USP_Busi_GetWorkingCalendarView @ProdLine, @Region, @ToDate, @ToDatePlus1
					
					if exists(select top 1 RowId from #tempWorkingCalendar)
					begin
						select top 1 @ToDate = DateFrom, @BaseDate = DateFrom from #tempWorkingCalendar order by DateFrom asc
						break
					end
					
					set @CycleCount = @CycleCount + 1   
					if @CycleCount > 999 
					begin 
						set @ErrorMsg = N'区域' + @Region + N'的工作日历没有维护完整。'
						RAISERROR(@ErrorMsg, 16, 1) 
					end 
				end
			end

			set @CycleCount = @CycleCount + 1   
			if @CycleCount > 999 
			begin 
				set @ErrorMsg = N'区域' + @Region + N'的工作日历没有维护完整。'
				RAISERROR(@ErrorMsg, 16, 1) 
			end 
		end 
	end try 
	begin catch 
		set @ErrorMsg = Error_Message() 
		RAISERROR(@ErrorMsg, 16, 1) 
	end catch 
END 
