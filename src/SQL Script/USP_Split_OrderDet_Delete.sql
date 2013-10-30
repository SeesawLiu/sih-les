IF EXISTS(SELECT * FROM SYS.objects WHERE type='P' AND name='USP_Split_OrderDet_Delete')
	DROP PROCEDURE USP_Split_OrderDet_Delete
GO
CREATE PROCEDURE [dbo].[USP_Split_OrderDet_Delete]
(
	@Id int,
	@Version int
)
AS 
BEGIN
	DECLARE @Statement nvarchar(4000) 
	DECLARE @Parameters nvarchar(4000) 
	DECLARE @OrderType tinyint
	SELECT @OrderType = OrderType From VIEW_OrderDet WHERE Id = @Id AND Version = @Version 
	
	SET @Statement=N'DELETE FROM ORD_OrderDet_' + CONVERT(varchar(1), @OrderType) + ' WHERE Id=@Id_1'
	SET @Parameters=N'@Id_1 int'
	EXEC SP_EXECUTESQL @Statement,@Parameters,@Id_1=@Id
END
GO