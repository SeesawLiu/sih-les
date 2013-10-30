SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.objects WHERE type='P' AND name='USP_Split_IpDet_Delete')
	DROP PROCEDURE USP_Split_IpDet_Delete
GO

CREATE PROCEDURE USP_Split_IpDet_Delete
(
	@Id int,
	@Version int
)
as 
begin
	DECLARE @OrderType tinyint
	SELECT @OrderType=OrderType From VIEW_IpDet WHERE Id=@Id AND Version=@Version
	
	IF @OrderType IS NULL
	BEGIN
		SELECT NULL
	END
	
	DECLARE @Statement nvarchar(4000) 
	DECLARE @Parameters nvarchar(4000) 
	
	SET @Statement=N'SET ARITHABORT ON;DELETE FROM ORD_IpDet_' + CONVERT(varchar, @OrderType) + ' WHERE Id=@Id_1'
	SET @Parameters=N'@Id_1 int'
	EXEC SP_EXECUTESQL @Statement,@Parameters,@Id_1=@Id
end
GO
