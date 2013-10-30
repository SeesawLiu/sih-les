SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.objects WHERE type='P' AND name='USP_Split_LocationLotDet_Delete')
	DROP PROCEDURE USP_Split_LocationLotDet_Delete
GO

CREATE PROCEDURE USP_Split_LocationLotDet_Delete
(
	@Id int,
	@Version int
)
AS 
BEGIN
	DECLARE @Location varchar(50)
	SELECT @Location=Location From VIEW_LocationLotDet WHERE Id=@Id AND [Version]=@Version
	
	IF @Location IS NULL
	BEGIN
		SELECT NULL
	END
	
	DECLARE @PartSuffix varchar(50)
	SELECT @PartSuffix = PartSuffix FROM MD_Location WHERE Code = @Location
	
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameters nvarchar(4000)
	SET @Statement=N'DELETE FROM INV_LocationLotDet_' + @PartSuffix + ' WHERE Id=@Id_1'
	SET @Parameters=N'@Id_1 int'
	EXEC SP_EXECUTESQL @Statement,@Parameters,@Id_1=@Id
END
GO
