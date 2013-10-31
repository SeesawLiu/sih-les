SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.objects WHERE type='P' AND name='USP_InvLoc_Insert')
	DROP PROCEDURE USP_InvLoc_Insert
GO

CREATE PROCEDURE USP_InvLoc_Insert
(
	@SourceType int,
	@SourceId bigint,
	@FRBNR varchar(16),
	@SGTXT varchar(5),
	@CreateUser varchar(50),
	@CreateDate datetime,
	@BWART varchar(3)
)
AS
BEGIN
	IF @SourceId > 0 AND EXISTS(SELECT TOP 1 1 FROM SAP_InvLoc WHERE SourceId = @SourceId and SourceType = @SourceType and BWART = @BWART)
	BEGIN
		RAISERROR('SourceId�ظ�', 16, 1)
		RETURN
	END 	
	
	INSERT INTO SAP_InvLoc(SourceType, SourceId, FRBNR, SGTXT, CreateUser, CreateDate) 
	VALUES (@SourceType, @SourceId, @FRBNR, @SGTXT, @CreateUser, @CreateDate)
	SELECT @@IDENTITY
END
GO
