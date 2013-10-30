SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_SYS_GetNextId') 
     DROP PROCEDURE USP_SYS_GetNextId
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_SYS_BatchGetNextId') 
     DROP PROCEDURE USP_SYS_BatchGetNextId
GO 

Declare @Id int
Declare @MaxId int
select IDENTITY(int, 1, 1) as Id, TabNm into #tempTab from SYS_TabIdSeq
select @Id = MIN(Id), @MaxId = MAX(Id) from #tempTab

while @Id <= @MaxId
begin
	Declare @SeqTableNm varchar(50)
	select @SeqTableNm =  'SEQ_' + TabNm from #tempTab where Id = @Id
	
	IF EXISTS(SELECT TOP 1 1 FROM sys.objects WHERE TYPE='U' AND name=@SeqTableNm)
	BEGIN
		DECLARE @DropTableStatement nvarchar(200) = 'drop table ' + @SeqTableNm 
		ExEC SP_EXECUTESQL @DropTableStatement
	END
	
	DECLARE @CreateTableStatement nvarchar(200) = 'Create table ' + @SeqTableNm + '(Id bigint identity(1,1) not null)'
	ExEC SP_EXECUTESQL @CreateTableStatement
		
	set @Id = @Id + 1
end
GO

drop table #tempTab
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
GO

CREATE PROCEDURE [dbo].[USP_SYS_BatchGetNextId] 
	@TablePrefix varchar(50), 
	@BatchSize int, 
	@NextId Bigint OUTPUT 
AS 
BEGIN
	SET NOCOUNT ON 
	IF @BatchSize <= 0
	BEGIN
		RAISERROR(N'BatchZise不能小于等于0。', 16, 1)
	END
	
	DECLARE @SeqTableNm varchar(50) = 'SEQ_' + @TablePrefix
	IF NOT EXISTS(SELECT TOP 1 1 FROM sys.objects WHERE TYPE='U' AND name=@SeqTableNm)
	BEGIN
		DECLARE @CreateTableStatement nvarchar(200) = 'Create table ' + @SeqTableNm + '(Id bigint identity(1,1) not null)'
		ExEC SP_EXECUTESQL @CreateTableStatement
	END
	
	DECLARE @Trancount INT = @@trancount
	
	if @trancount > 0
	BEGIN
		SAVE TRAN BatchGetNextId_SavePoint
    END
    ELSE
    BEGIN
		BEGIN TRAN BatchGetNextId
    END
    
    DECLARE @InsertStatement nvarchar(2000) = 'IF (EXISTS(select TOP 1 1 from ' + @SeqTableNm + ' with(XLOCK)) OR 1 = 1) BEGIN '
    set @InsertStatement = @InsertStatement + 'INSERT INTO ' + @SeqTableNm + ' default values '
	set @InsertStatement = @InsertStatement + 'SELECT @NextId = SCOPE_IDENTITY() '
	set @InsertStatement = @InsertStatement + 'SET @NextId = @NextId + @BatchSize '
    set @BatchSize = @BatchSize - 1
    if @BatchSize > 0
    begin
		set @InsertStatement = @InsertStatement + 'SET IDENTITY_INSERT ' + @SeqTableNm + ' ON '
		set @InsertStatement = @InsertStatement + 'insert into ' + @SeqTableNm + '(Id) values(@NextId) '
		set @InsertStatement = @InsertStatement + 'SET IDENTITY_INSERT ' + @SeqTableNm + ' OFF '
    end
	set @InsertStatement = @InsertStatement + 'END'
    
	EXEC SP_EXECUTESQL @InsertStatement, N'@BatchSize INT, @NextId BIGINT OUTPUT', @BatchSize, @NextId OUTPUT
	
	if @trancount > 0
	BEGIN
		ROLLBACK TRAN BatchGetNextId_SavePoint
    END
    ELSE
    BEGIN
		ROLLBACK TRAN BatchGetNextId
	END
END
GO