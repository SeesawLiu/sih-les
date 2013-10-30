SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_SYS_GetNextId') 
     DROP PROCEDURE USP_SYS_GetNextId
GO

CREATE PROCEDURE [dbo].[USP_SYS_GetNextId] 
	@TablePrefix varchar(50), 
	@NextId Bigint OUTPUT 
AS
BEGIN
	SET NOCOUNT ON 
	DECLARE @SeqTableNm varchar(50) = 'SEQ_' + @TablePrefix
	IF NOT EXISTS(SELECT TOP 1 1 FROM sys.objects WHERE TYPE='U' AND name=@SeqTableNm)
	BEGIN
		DECLARE @CreateTableStatement nvarchar(200) = 'Create table ' + @SeqTableNm + '(Id bigint identity(1,1) not null)'
		ExEC SP_EXECUTESQL @CreateTableStatement
	END
	
	DECLARE @Trancount INT = @@trancount
	
	if @trancount > 0
	BEGIN
		SAVE TRAN GetNextId_SavePoint
    END
    ELSE
    BEGIN
		BEGIN TRAN GetNextId
    END
    
	DECLARE @InsertStatement nvarchar(200) = 'insert into ' + @SeqTableNm + ' default values;SELECT @NextId = SCOPE_IDENTITY();'
	EXEC SP_EXECUTESQL @InsertStatement, N'@NextId BIGINT OUTPUT', @NextId OUTPUT
	
	if @trancount > 0
	BEGIN
		ROLLBACK TRAN GetNextId_SavePoint
    END
	ELSE
    BEGIN
		ROLLBACK TRAN GetNextId
	END
END
