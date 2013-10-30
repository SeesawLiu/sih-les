--select * from dbo.ORD_OrderMstr

CREATE PROCEDURE [dbo].[USP_Split_LocationLotDet_Delete]
(
	@Id int,
	@Version int
)
AS 
BEGIN
	DECLARE @Location tinyint
	SELECT @Location=Location From VIEW_LocationLotDet WHERE Id=@Id AND [Version]=@Version
	
	IF @Location='RM'
	BEGIN
		DELETE FROM INV_LocationLotDet_1 WHERE Id=@Id AND [Version]=@Version
	END
	ELSE
	BEGIN
		DELETE FROM INV_LocationLotDet_0 WHERE Id=@Id AND [Version]=@Version
	END
    
    SELECT SCOPE_IDENTITY()
END
