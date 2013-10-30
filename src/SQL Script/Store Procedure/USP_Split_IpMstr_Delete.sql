SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.objects WHERE type='P' AND name='USP_Split_IpMstr_Delete')
	DROP PROCEDURE USP_Split_IpMstr_Delete
GO

CREATE procedure USP_Split_IpMstr_Delete
(
	@IpNo varchar(4000),
	@Version int
)
as 
begin
	DECLARE @OrderType tinyint
	SELECT @OrderType=OrderType From VIEW_IpMstr WHERE IpNo=@IpNo AND Version=@Version
	
	IF @OrderType IS NULL
	BEGIN
		SELECT NULL
	END
	
	DECLARE @Statement nvarchar(4000) 
	DECLARE @Parameters nvarchar(4000) 
	
	SET @Statement=N'DELETE FROM ORD_IpMstr_' + CONVERT(varchar, @OrderType) +' WHERE IpNo=@IpNo_1'
	SET @Parameters=N'@IpNo_1 varchar(50)'
	EXEC SP_EXECUTESQL @Statement,@Parameters,@IpNo_1=@IpNo
end
GO