SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.objects WHERE type='P' AND name='USP_Busi_GetNextWindowTime')
	DROP PROCEDURE USP_Busi_GetNextWindowTime
GO

CREATE PROCEDURE [dbo].[USP_Busi_GetNextWindowTime]
(
	@Flow varchar(50),
	@WindowTime datetime,
	@NextWindowTime datetime output
) --WITH ENCRYPTION
AS
BEGIN
	set nocount on
	set @NextWindowTime = null
	
	declare @ErrorMsg nvarchar(Max)
	declare @DateTimeNow datetime = GETDATE()
	declare @FlowType tinyint
	declare @PartyFrom varchar(50)
	declare @PartyTo varchar(50)
	declare @WinTimeType tinyint
	declare @WeekInterval int
	declare @WinTimeInterval decimal(18, 8)
	declare @WeekDay int
	
	select @FlowType = mstr.[Type], @PartyFrom = mstr.PartyFrom, @PartyTo = mstr.PartyTo,
	@WinTimeType = stra.WinTimeType, @WinTimeInterval = stra.WinTimeInterval * 60 * 60,  --小时转为秒
	@WeekInterval = stra.WeekInterval
	from SCM_FlowStrategy as stra 
	inner join SCM_FlowMstr as mstr on stra.Flow = mstr.Code
	where stra.Flow = @Flow
	
	if (@WindowTime is null or @WindowTime <= @DateTimeNow)
	begin
		set @WindowTime = @DateTimeNow
		if (@WinTimeType = 1)
		begin  --固定间隔时间，取整点
			set @WindowTime = CONVERT(DATETIME, CONVERT(varchar(13), @WindowTime, 120) + ':00')
		end
	end
	
	if (@WinTimeType = 0)
	begin  --固定窗口时间
		declare @WinTime varchar(256)
		declare @WinDate datetime = convert(datetime, convert(varchar(10), @WindowTime, 112), 120)
		if @WeekInterval > 0
		begin
			set @WinDate = DATEADD(DAY, @WeekInterval, @WinDate)
		end

		while 1 = 1
		begin
			set @WinTime = null
			if @FlowType <> 3
			begin  --非销售看PartyTo的工作日历
				select top 1 @WinTime = fsd.WinTime
				from PRD_WorkingCalendar as wc
				inner join SCM_FlowShiftDet as fsd on wc.Shift = fsd.Shift
				where wc.WorkingDate = @WinDate
				and wc.Region = @PartyTo
				and wc.[Type] = 0  --工作
				and fsd.Flow = @Flow
			end
			else
			begin  --销售看PartyFrom的工作日历
				select top 1 @WinTime = fsd.WinTime
				from PRD_WorkingCalendar as wc
				inner join SCM_FlowShiftDet as fsd on wc.Shift = fsd.Shift
				where wc.WorkingDate = @WinDate
				and wc.Region = @PartyFrom
				and wc.[Type] = 0  --工作
				and fsd.Flow = @Flow
			end
			
			if ISNULL(@WinTime, '') <> ''
			begin
				exec USP_Busi_MatchNextWindowTime @WindowTime, @WinDate, @WinTime, @NextWindowTime output
				
				if @NextWindowTime is not null
				begin
					return
				end
			end
			
			set @WinDate = DATEADD(DAY, 1, @WinDate)
			
			if DateDiff(DAY, @WindowTime, @WinDate) > 99
			begin
				set @ErrorMsg = N'路线' + @Flow + N'的窗口时间没有维护完整。'
				RAISERROR(@ErrorMsg, 16, 1) 
				return
			end
		end
	end
	else
	begin  --固定间隔时间
		if @WinTimeInterval > 0
		begin
			if @FlowType <> 3
			begin  --非销售看PartyTo的工作日历
				exec USP_Busi_AddWorkingDate @WindowTime, @WinTimeInterval, null, @PartyTo, @NextWindowTime output
			end
			else
			begin  --销售看PartyFrom的工作日历
				exec USP_Busi_AddWorkingDate @WindowTime, @WinTimeInterval, null, @PartyFrom, @NextWindowTime output
			end
		end
		else
		begin
			set @ErrorMsg = N'路线' + @Flow + N'没有设置间隔时间。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
	end
END
GO

