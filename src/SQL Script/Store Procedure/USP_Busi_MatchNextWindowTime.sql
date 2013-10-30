SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.objects WHERE type='P' AND name='USP_Busi_MatchNextWindowTime')
	DROP PROCEDURE USP_Busi_MatchNextWindowTime
GO

CREATE PROCEDURE [dbo].[USP_Busi_MatchNextWindowTime]
(
	@BaseTime datetime,
	@WinDate datetime,
	@WinTime varchar(256),
	@NextTime datetime output
) --WITH ENCRYPTION
AS
BEGIN
	set nocount on
	if ISNULL(@WinTime, '') <> ''
	begin
		declare @SplitSymbol1 char(1) = ','
		declare @SplitSymbol2 char(1) = '|'
		
		Create table #tempWinTime
		(
			WinTime varchar(5),
			WinDateTime datetime
		)
	
		if (charindex(@SplitSymbol1, @WinTime) <> 0)
		begin
			--循环窗口时间插入缓存表中
			while(charindex(@SplitSymbol1, @WinTime) <> 0)
			begin
				insert #tempWinTime(WinTime) values (substring(@WinTime, 1, charindex(@SplitSymbol1, @WinTime) - 1))
				set @WinTime = stuff(@WinTime, 1, charindex(@SplitSymbol1, @WinTime), ' ')
			end
		end
		else if (charindex(@SplitSymbol2, @WinTime) <> 0)
		begin
			--循环窗口时间插入缓存表中
			while(charindex(@SplitSymbol2, @WinTime) <> 0)
			begin
				insert #tempWinTime(WinTime) values (Rtrim(Ltrim(substring(@WinTime, 1, charindex(@SplitSymbol2, @WinTime) - 1))))
				set @WinTime = stuff(@WinTime, 1, charindex(@SplitSymbol2, @WinTime), ' ')
			end
		end

		insert #tempWinTime(WinTime) values (Rtrim(Ltrim(@WinTime)))
		update #tempWinTime set WinDateTime = CONVERT(DATETIME, CONVERT(varchar(10), @WinDate, 120) + ' ' + substring(WinTime, 1, charindex(':', WinTime) - 1) + ':' + substring(WinTime, charindex(':', WinTime) + 1, 2))
		select top 1 @NextTime = WinDateTime from #tempWinTime where WinDateTime > @BaseTime order by WinDateTime asc
		
		drop table #tempWinTime
	end
END
