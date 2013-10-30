SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.objects WHERE type='P' AND name='USP_Split_OrderMstr_Delete')
	DROP PROCEDURE USP_Split_OrderMstr_Delete
GO

CREATE procedure USP_Split_OrderMstr_Delete
(
	@OrderNo varchar(4000),
	@Version int
)
AS
BEGIN
	DECLARE @OrderType tinyint
	SELECT @OrderType=[Type] From VIEW_OrderMstr WHERE OrderNo = @OrderNo AND [Version] = @Version
	
	IF @OrderType IS NULL
	BEGIN
		SELECT NULL
	END
	
	DECLARE @Statement nvarchar(4000) 
	DECLARE @Parameters nvarchar(4000) 
	
	print @OrderType
	SET @Statement=N'DELETE FROM ORD_OrderMstr_'+cast(@OrderType as varchar(50))+' WHERE OrderNo=@OrderNo_1'
	SET @Parameters=N'@OrderNo_1 varchar(50)'
	EXEC SP_EXECUTESQL @Statement,@Parameters,@OrderNo_1=@OrderNo
END
GO
