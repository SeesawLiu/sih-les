SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.objects WHERE type='P' AND name='USP_Split_RecMstr_Delete')
	DROP PROCEDURE USP_Split_RecMstr_Delete
GO

CREATE PROCEDURE USP_Split_RecMstr_Delete
(
	@RecNo varchar(4000),
	@Version int
)
AS
BEGIN
	DECLARE @OrderType tinyint
	SELECT @OrderType=OrderType From VIEW_RecMstr WHERE RecNo=@RecNo AND Version=@Version
	
	IF @OrderType IS NULL
	BEGIN
		SELECT NULL
	END
	
	DECLARE @Statement nvarchar(4000) 
	DECLARE @Parameters nvarchar(4000) 
	
	SET @Statement=N'DELETE FROM ORD_RecMstr_'+@OrderType+' WHERE RecNo=@RecNo_1'
	SET @Parameters=N'@RecNo_1 varchar(50)'
	EXEC SP_EXECUTESQL @Statement,@Parameters,@RecNo_1=@RecNo
END
GO
