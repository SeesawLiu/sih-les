SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Busi_GetStartTimeAsMinute') 
     DROP PROCEDURE USP_Busi_GetStartTimeAsMinute 
GO

CREATE PROCEDURE [dbo].[USP_Busi_GetStartTimeAsMinute] 
( 
	@ReturnMinute int output
) --WITH ENCRYPTION
AS 
BEGIN 
	set nocount on
	declare @SplitSymbol varchar(1) = ':'
	declare @WorkingDateStartTime varchar(5)
	select @WorkingDateStartTime = Value from SYS_EntityPreference where Id = 10020
	
	if (charindex(@SplitSymbol, @WorkingDateStartTime) <> 0)
	begin
		set @ReturnMinute = cast(substring(@WorkingDateStartTime, 1, charindex(@SplitSymbol, @WorkingDateStartTime) - 1) as int) * 60 + cast(substring(@WorkingDateStartTime, charindex(@SplitSymbol, @WorkingDateStartTime) + 1, 2) as int)
	end
	else
	begin
		set @ReturnMinute = cast(@WorkingDateStartTime as int) * 60
	end
END 
