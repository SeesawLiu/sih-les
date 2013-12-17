SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Busi_SubtractWorkingDate') 
     DROP PROCEDURE USP_Busi_SubtractWorkingDate 
GO

CREATE PROCEDURE [dbo].[USP_Busi_SubtractWorkingDate] 
(
	@BaseDate datetime, 
	@SubtractTime int,  --时间单位为秒
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
		set @BaseDate = CONVERT(DATETIME, CONVERT(varchar(50), @BaseDate, 120))  --去掉毫秒，保证@TargetDate >= @DateFrom这句话因为出现毫秒没有匹配，导致@ReturnDate没有赋值
		Declare @FromDate datetime = @BaseDate 
		Declare @TargetDate datetime = @BaseDate 
		Declare @CycleWDRowId int = 1 
		Declare @MinWDRowId  int = 1 
		Declare @DateFrom datetime 
		Declare @DateTo datetime 
		Declare @CycleCount int = 0
		Declare @ErrorMsg varchar(max)
		
		if @SubtractTime = 0 
		begin 
			set @ReturnDate = @FromDate 
			return 
		end 
		else if @SubtractTime > 0 
		begin 
			RAISERROR(N'间隔时间不能大于0。', 16, 1) 
		end 

		while @SubtractTime < 0 
		begin 
			set @FromDate = DateAdd(SECOND, @SubtractTime, @FromDate) 
			--如果基准日期和截止日期是同一天，截止日期-1天 
			if convert(varchar(20), @FromDate, 112) = convert(varchar(20), @BaseDate, 112) 
			begin 
				set @FromDate = DateAdd(D, -1, @FromDate) 
			end        
			--获取基准日期和截止日期之间的工作时间段
			truncate table #tempWorkingCalendar
			insert into #tempWorkingCalendar(WorkingDate, DateFrom, DateTo) exec USP_Busi_GetWorkingCalendarView @ProdLine, @Region, @FromDate, @BaseDate
			
			if exists(select top 1 RowId from #tempWorkingCalendar)
			begin
				select @CycleWDRowId = MAX(RowId), @MinWDRowId = MIN(RowId) from #tempWorkingCalendar 
				while @MinWDRowId <= @CycleWDRowId
				begin 
					select @DateFrom = DateFrom, @DateTo = DateTo from #tempWorkingCalendar where RowId = @CycleWDRowId 
					set @TargetDate = DATEADD(SECOND, @SubtractTime, @DateTo) 

					if @TargetDate >= @DateFrom 
					begin 
						set @ReturnDate = @TargetDate 
						return 
					end 
					else 
					begin 
						set @SubtractTime = DATEDIFF(SECOND, @DateFrom, @TargetDate) 
						set @BaseDate = @DateFrom 
						set @FromDate = @DateFrom 
					end 

					set @CycleWDRowId = @CycleWDRowId - 1 
				end 
			end
			else
			begin --基准日期和截止日期之间没有工作时间
				--为了避免@SubtractTime太小，循环999次都没有找到工作时间
				--这里通过@FromDate - 1天来找之前的工作结束时间
				declare @FromDateMinus1 datetime = @FromDate
				
				while 1 = 1
				begin
					set @FromDateMinus1 = DATEADD(DAY, -1, @FromDateMinus1)
					
					truncate table #tempWorkingCalendar
					insert into #tempWorkingCalendar(WorkingDate, DateFrom, DateTo) exec USP_Busi_GetWorkingCalendarView @ProdLine, @Region, @FromDateMinus1, @FromDate
					
					if exists(select top 1 RowId from #tempWorkingCalendar)
					begin
						select top 1 @FromDate = DateTo, @BaseDate = DateTo from #tempWorkingCalendar order by DateTo desc
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
