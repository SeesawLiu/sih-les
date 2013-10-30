SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Tools_GenerateSP4SplitTable') 
     DROP PROCEDURE USP_Tools_GenerateSP4SplitTable 
GO

CREATE PROCEDURE [dbo].USP_Tools_GenerateSP4SplitTable 
( 
	@FileName varchar(1000) 
)
AS
BEGIN 
	SET NOCOUNT ON 
	--DECLARE @FileName varchar(255)  
	DECLARE @Cmd VARCHAR(255)  
	DECLARE @xmlContent VARCHAR(max)  
	CREATE TABLE #tempXML(Id INT NOT NULL IDENTITY(1,1), Line VARCHAR(max))  
	--DROP TABLE #tempXML 
	--SELECT @FileName = 'C:\OrderMaster.hbm.xml'  
	SELECT @Cmd = 'type ' + @FileName  
	SELECT @xmlContent = ''  
	INSERT INTO #tempXML EXEC master.dbo.xp_cmdshell @Cmd  
	DECLARE @maxLine int 
	DECLARE @tmp int=1 
	DECLARE @line varchar(4000) 
	DECLARE @beginMark int=0 
	DECLARE @endMark int=0 
	SELECT @maxLine=MAX(Id) FROM #tempXML 
	WHILE(@tmp<=@maxLine) 
	BEGIN 
		SELECT @line=Line FROM #tempXML WHERE Id=@tmp 

		IF CHARINDEX('hibernate-mapping',@line)>0 
			OR CHARINDEX('<?',@line)>0 OR @line IS NULL 
		BEGIN 
			DELETE FROM #tempXML WHERE Id=@tmp 
		END	 
		IF CHARINDEX('<!--',@line)>0 
		BEGIN 
			IF CHARINDEX('-->',@line)>0 
			BEGIN 
				DELETE FROM #tempXML WHERE Id=@tmp 
			END 
			ELSE 
			BEGIN 
				SET @beginMark=@tmp 
			END 
		END 
		IF CHARINDEX('-->',@line)>0 AND @beginMark<>0 
		BEGIN 
			SET @endMark=@tmp 
			DELETE FROM #tempXML WHERE Id between @beginMark and @endMark 
			SET @beginMark=0 
		END	 
		SET @tmp=@tmp+1 
	END 
	--SELECT * FROM #tempXML 
	CREATE TABLE #temp(Id INT NOT NULL IDENTITY(1,1), Line VARCHAR(max)) 
	INSERT INTO #temp SELECT line FROM #tempXML 
	SELECT @maxLine = MAX(Id) from #temp  
	SELECT @tmp = 1  
	WHILE @tmp<=@maxLine  
	BEGIN  
		SELECT @xmlContent = @xmlContent + Line from #temp WHERE Id = @tmp  
		SELECT @tmp = @tmp + 1  
	END  
	DECLARE @data xml 
	SET @data = CAST(@xmlContent as xml) 
	CREATE TABLE #Result(RowId int Identity(1,1),[name] varchar(50),[column] varchar(50),[type] varchar(50),[length] varchar(50),[insert] varchar(50),[update] varchar(50), [module] varchar(50)) 
	INSERT INTO #Result([name],[column],[type],[length],[insert],[update],[module]) 
	SELECT * FROM (  
	SELECT T.c.value('@name','varchar(50)') as [name], 
		T.c.value('@column','varchar(50)') as [column], 
		T.c.value('@type','varchar(50)') as [type], 
		T.c.value('@length','varchar(50)') as [length], 
		T.c.value('@insert','varchar(50)') as [insert], 
		T.c.value('@update','varchar(50)') as [update], 
		'id' as [module] 
	FROM @data.nodes('/class/id') T(c)) as Result 
	UNION ALL 
	SELECT T.c.value('@name','varchar(50)') as [name], 
		T.c.value('@column','varchar(50)') as [column], 
		T.c.value('@type','varchar(50)') as [type], 
		T.c.value('@length','varchar(50)') as [length], 
		T.c.value('@insert','varchar(50)') as [insert], 
		T.c.value('@update','varchar(50)') as [update], 
		'version' as [module] 
	FROM @data.nodes('/class/version') T(c) 
	UNION ALL 
	SELECT T.c.value('@name','varchar(50)') as [name], 
		T.c.value('@column','varchar(50)') as [column], 
		T.c.value('@type','varchar(50)') as [type], 
		T.c.value('@length','varchar(50)') as [length], 
		T.c.value('@insert','varchar(50)') as [insert], 
		T.c.value('@update','varchar(50)') as [update], 
		'property' as [module] 
	FROM @data.nodes('/class/property') T(c)	 
	--SELECT CAST(@xmlContent as xml) 
	DECLARE @tableName varchar(50) 
	SELECT @tableName=@data.value ('((/class)/@table)[1]','varchar(max)') 

	UPDATE #Result SET [type]='Int32' where [column]='TransType' 
	SELECT * FROM #Result 
	----CREATE INSERT SP 
	DECLARE @SP varchar(max) 
	DECLARE @where varchar(max) 
	SET @SP='IF EXISTS(SELECT * FROM sys.objects WHERE name=''USP_Split_'+SUBSTRING(@tableName,CHARINDEX('_',@tableName)+1,LEN(@tableName))+'_Insert'') 
	DROP PROCEDURE USP_Split_'+SUBSTRING(@tableName,CHARINDEX('_',@tableName)+1,LEN(@tableName))+'_Insert' 
	PRINT '-----DROP INSERT SP' 
	PRINT @SP 
	PRINT 'GO' 
	--exec(@SP)	 
	PRINT '-----CREATE INSERT SP' 
	SET @SP='CREATE PROCEDURE USP_Split_'+SUBSTRING(@tableName,CHARINDEX('_',@tableName)+1,LEN(@tableName))+'_Insert' 
	SET @SP=@SP+CHAR(10)+'(' 
	--PRINT @SP 
	DECLARE @InsertParaEqual varchar(max)='' 
	DECLARE @UpdateParaEqual varchar(max)='' 
	DECLARE @InsertParaDefine varchar(max)='' 
	DECLARE @UpdateParaDefine varchar(max)='' 
	DECLARE @InsertAllColumns varchar(max) 
	DECLARE @UpdateAllColumns varchar(max) 
	DECLARE @InsertAllVars varchar(max) 
	DECLARE @UpdateAllVars varchar(max) 
	DECLARE @ExecInsert varchar(8000)='EXEC USP_Split_'+SUBSTRING(@tableName,CHARINDEX('_',@tableName)+1,LEN(@tableName))+'_Insert ' 
	DECLARE @ExecUpdate varchar(8000)='EXEC USP_Split_'+SUBSTRING(@tableName,CHARINDEX('_',@tableName)+1,LEN(@tableName))+'_Update ' 

	DECLARE @name varchar(50) 
	DECLARE @column varchar(50) 
	DECLARE @type varchar(50) 
	DECLARE @length varchar(50) 
	DECLARE @insert varchar(50) 
	DECLARE @update varchar(50) 
	DECLARE @module varchar(50) 
	SELECT @maxLine=MAX(RowId) FROM #Result 
	SET @tmp=1 
	WHILE(@tmp<=@maxLine) 
	BEGIN 
		--print @column 
		SELECT @name=[name],@column=[column],@type=[type],@length=[length],@insert=[insert],@module=[module] 
			FROM #Result WHERE RowId=@tmp 
		--print @column	 
		IF @insert IS NULL --AND @module<>'id' 
		BEGIN 
			IF @module<>'id' 
			BEGIN 
				SET @SP=@SP+CHAR(10) 
				SET @ExecInsert=@ExecInsert+'?,' 
			End 
			SET @InsertAllColumns=ISNULL(@InsertAllColumns,'')+@column+',' 
			SET @InsertAllVars=ISNULL(@InsertAllVars,'')+'@'+@column+'_1,' 
			SET @InsertParaEqual=isnull(@InsertParaEqual,'')+'@'+@column+'_1=@'+@column+',' 


			IF(@type is NULL) 
			BEGIN 
				IF @module<>'id' 
				BEGIN 
					SET @SP=@SP+'	@'+@column+' tinyint,' 
				END 
				SET @InsertParaDefine=@InsertParaDefine+'@'+@column+'_1 tinyint, ' 
			END	 
			ELSE IF(UPPER(@type)='INT32') 
			BEGIN 
				IF @module<>'id' 
				BEGIN 
					SET @SP=@SP+'	@'+@column+' int,' 
				END 
				SET @InsertParaDefine=@InsertParaDefine+'@'+@column+'_1 int, ' 
			END		 
			ELSE IF(UPPER(@type)='INT64') 
			BEGIN 
				IF @module<>'id' 
				BEGIN 
					SET @SP=@SP+'	@'+@column+' bigint,' 
				END 
				SET @InsertParaDefine=@InsertParaDefine+'@'+@column+'_1 bigint, ' 
			END					 
			ELSE IF(UPPER(@type)='STRING') 
			BEGIN 
				IF(@length IS NULL) 
				BEGIN 
					IF @module<>'id' 
					BEGIN 
						SET @SP=@SP+'	@'+@column+' varchar(4000),' 
					END 
					SET @InsertParaDefine=@InsertParaDefine+'@'+@column+'_1 varchar(4000), ' 
				END	 
				ELSE 
				BEGIN 
					IF @module<>'id' 
					BEGIN 
						SET @SP=@SP+'	@'+@column+' varchar('+@length+'),' 
					END 
					SET @InsertParaDefine=@InsertParaDefine+'@'+@column+'_1 varchar('+@length+'), ' 
				END 
			END	 
			ELSE IF(UPPER(@type)='DATETIME') 
			BEGIN 
				IF @module<>'id' 
				BEGIN 
					SET @SP=@SP+'	@'+@column+' datetime,' 
				END 
				SET @InsertParaDefine=@InsertParaDefine+'@'+@column+'_1 datetime, ' 
			END	 
			ELSE IF(UPPER(@type)='BOOLEAN') 
			BEGIN 
				IF @module<>'id' 
				BEGIN 
					SET @SP=@SP+'	@'+@column+' bit,' 
				END 
				SET @InsertParaDefine=@InsertParaDefine+'@'+@column+'_1 bit, ' 
			END	 
			ELSE IF(UPPER(@type)='DECIMAL') 
			BEGIN 
				IF @module<>'id' 
				BEGIN 
					SET @SP=@SP+'	@'+@column+' decimal(18,8),' 
				END 
				SET @InsertParaDefine=@InsertParaDefine+'@'+@column+'_1 decimal(18,8), ' 
			END	 
		END 

		--print @sp				 
		SET @tmp=@tmp+1	 
	END 

	IF EXISTS(SELECT 1 FROM #Result WHERE [column]<>'Id' AND module='id') 
	BEGIN 
		PRINT '<' 
		SET @ExecInsert=@ExecInsert+'?,' 
		SET @SP=@SP+CHAR(10) 
		SELECT @SP=@SP+'	@'+[column]+' varchar(4000),' FROM  #Result WHERE [column]<>'Id' AND module='id' 
	END 

	SET @SP=SUBSTRING(@SP,1,LEN(@SP)-1)+CHAR(10)+')' 
	SET @SP=@SP+CHAR(10)+'AS' 
	SET @SP=@SP+CHAR(10)+'BEGIN' 
	SET @SP=@SP+CHAR(10)+'	DECLARE @Statement nvarchar(4000)' 
	SET @SP=@SP+CHAR(10)+'	DECLARE @Parameters nvarchar(4000)' 
	SET @SP=@SP+CHAR(10)+'	SET @Statement=''INSERT INTO '+@tableName+'('+SUBSTRING(@InsertAllcolumns,1,LEN(@InsertAllcolumns)-1)+') VALUES('+SUBSTRING(@InsertAllVars,1,LEN(@InsertAllVars)-1)+')''' 
	PRINT @SP 
	SET @SP='	SET @Parameters='''+SUBSTRING(@InsertParaDefine,1,LEN(@InsertParaDefine)-1)+'''' 
	SET @SP=@SP+CHAR(10)+'	EXEC SP_EXECUTESQL @Statement,@Parameters,'+SUBSTRING(@InsertParaEqual,1,LEN(@InsertParaEqual)-1) 
	--SET @SP=@SP+CHAR(10)+'	INSERT INTO '+@tableName+'('+SUBSTRING(@InsertAllcolumns,1,LEN(@InsertAllcolumns)-1)+')' 
	--SET @SP=@SP+CHAR(10)+'	VALUES('+SUBSTRING(@InsertAllVars,1,LEN(@InsertAllVars)-1)+')' 
	IF EXISTS(SELECT 1 FROM #Result WHERE [column]='Id') 
	BEGIN 
		SET @SP=@SP+CHAR(10)+'SELECT @Id' 
	END 
	SET @SP=@SP+CHAR(10)+'END' 

	--SELECT @SP=SUBSTRING(@SP,1,LEN(@SP)-1)+') 
	--AS 
	--BEGIN 
	--	INSERT INTO '+@tableName+'('+SUBSTRING(@InsertAllcolumns,1,LEN(@InsertAllcolumns)-1)+')  
	--	VALUES('+SUBSTRING(@InsertAllVars,1,LEN(@InsertAllVars)-1)+') 
	--	SELECT SCOPE_IDENTITY() 
	--END' 
	PRINT @SP 
	PRINT 'GO' 
	PRINT SUBSTRING(@ExecInsert, 1, LEN(@ExecInsert) - 1)
	--exec(@SP) 
	--return 
	----CREATE UPDATE SP 
	SET @SP='IF EXISTS(SELECT * FROM sys.objects WHERE name=''USP_Split_'+SUBSTRING(@tableName,CHARINDEX('_',@tableName)+1,LEN(@tableName))+'_Update'') 
		DROP PROCEDURE USP_Split_'+SUBSTRING(@tableName,CHARINDEX('_',@tableName)+1,LEN(@tableName))+'_Update' 
	PRINT '-----DROP UPDATE SP' 
	PRINT @SP	 
	PRINT 'GO' 
	--exec(@SP)	 
	PRINT '-----CREATE UPDATE SP' 
	SET @SP='CREATE PROCEDURE USP_Split_'+SUBSTRING(@tableName,CHARINDEX('_',@tableName)+1,LEN(@tableName))+'_Update' 
	SET @SP=@SP+CHAR(10)+'(' 
	SET @tmp=1 
	WHILE(@tmp<=@maxLine) 
	BEGIN 
		--print @column 
		SELECT @name=[name],@column=[column],@type=[type],@length=[length],@update=[update],@module=[module] 
			FROM #Result WHERE RowId=@tmp 
		--print @column	 

		IF(@update IS NULL AND @module<>'id') 
		BEGIN	 
			SET @SP=@SP+CHAR(10) 
			SET @UpdateAllColumns=ISNULL(@UpdateAllColumns,'')+@column+',' 
			SET @UpdateAllVars=ISNULL(@UpdateAllVars,'')+@column+'=@'+@column+'_1,' 
			SET @UpdateParaEqual=isnull(@UpdateParaEqual,'')+'@'+@column+'_1=@'+@column+',' 
			SET @ExecUpdate=@ExecUpdate+'?,' 

			IF(@type is NULL) 
			BEGIN 
				SET @SP=@SP+'	@'+@column+' tinyint,' 
				SET @UpdateParaDefine=@UpdateParaDefine+'@'+@column+'_1 tinyint, ' 
			END	 
			ELSE IF(UPPER(@type)='INT32') 
			BEGIN 
				SET @SP=@SP+'	@'+@column+' int,' 
				SET @UpdateParaDefine=@UpdateParaDefine+'@'+@column+'_1 int, ' 
			END		 
			ELSE IF(UPPER(@type)='INT64') 
			BEGIN 
				SET @SP=@SP+'	@'+@column+' bigint,' 
				SET @UpdateParaDefine=@UpdateParaDefine+'@'+@column+'_1 bigint, ' 
			END					 
			ELSE IF(UPPER(@type)='STRING') 
			BEGIN 
				IF(@length IS NULL) 
				BEGIN 
					SET @SP=@SP+'	@'+@column+' varchar(4000),' 
					SET @UpdateParaDefine=@UpdateParaDefine+'@'+@column+'_1 varchar(4000), ' 
				END	 
				ELSE 
				BEGIN 
					SET @SP=@SP+'	@'+@column+' varchar('+@length+'),' 
					SET @UpdateParaDefine=@UpdateParaDefine+'@'+@column+'_1 varchar('+@length+'), ' 
				END 
			END	 
			ELSE IF(UPPER(@type)='DATETIME') 
			BEGIN 
				SET @SP=@SP+'	@'+@column+' datetime,' 
				SET @UpdateParaDefine=@UpdateParaDefine+'@'+@column+'_1 datetime, ' 
			END	 
			ELSE IF(UPPER(@type)='BOOLEAN') 
			BEGIN 
				SET @SP=@SP+'	@'+@column+' bit,' 
				SET @UpdateParaDefine=@UpdateParaDefine+'@'+@column+'_1 bit, ' 
			END	 
			ELSE IF(UPPER(@type)='DECIMAL') 
			BEGIN 
				SET @SP=@SP+'	@'+@column+' decimal(18,8),' 
				SET @UpdateParaDefine=@UpdateParaDefine+'@'+@column+'_1 decimal(18,8), ' 
			END	 
		END	 
		--print @sp				 
		SET @tmp=@tmp+1	 
	END 

	SET @SP=@SP+CHAR(10) 
	SELECT @name=[name],@column=[column],@type=[type],@length=[length] FROM #Result WHERE [module]='Id' 
	IF(UPPER(@type)='INT32') 
	BEGIN 
		SET @SP=@SP+'	@'+@column+' int,' 
		SET @UpdateParaDefine=@UpdateParaDefine+'@'+@column+'_1 int, ' 
	END		 
	ELSE IF(UPPER(@type)='INT64') 
	BEGIN 
		SET @SP=@SP+'	@'+@column+' bigint,' 
		SET @UpdateParaDefine=@UpdateParaDefine+'@'+@column+'_1 bigint, ' 
	END					 
	ELSE IF(UPPER(@type)='STRING') 
	BEGIN 
		IF(@length IS NULL) 
		BEGIN 
			SET @SP=@SP+'	@'+@column+' varchar(4000),' 
			SET @UpdateParaDefine=@UpdateParaDefine+'@'+@column+'_1 varchar(4000), ' 
		END	 
		ELSE 
		BEGIN 
			SET @SP=@SP+'	@'+@column+' varchar('+@length+'),' 
			SET @UpdateParaDefine=@UpdateParaDefine+'@'+@column+'_1 varchar('+@length+'), ' 
		END 
	END	 
	SET @where=ISNULL(@where,'')+@column+'=@'+@column+'_1' 
	SET @UpdateParaEqual=@UpdateParaEqual+'@'+@column+'_1=@'+@column+',' 
	SET @ExecUpdate=@ExecUpdate+'?,' 


	IF EXISTS(SELECT * FROM #Result WHERE [module]='version') 
	BEGIN 
		SET @SP=@SP+CHAR(10) 
		SET @SP=@SP+'	@VersionBerfore int,' 
		SET @UpdateParaDefine=@UpdateParaDefine+'@VersionBerfore_1 int, ' 
		SET @where=@where + ' AND Version=@VersionBerfore_1' 
		SET @UpdateParaEqual=@UpdateParaEqual+'@VersionBerfore_1=@VersionBerfore,' 
		SET @ExecUpdate=@ExecUpdate+'?,' 
	END 

	SET @SP=SUBSTRING(@SP,1,LEN(@SP)-1)+CHAR(10)+')' 
	SET @SP=@SP+CHAR(10)+'AS' 
	SET @SP=@SP+CHAR(10)+'BEGIN' 
	SET @SP=@SP+CHAR(10)+'	DECLARE @Statement nvarchar(4000)' 
	SET @SP=@SP+CHAR(10)+'	DECLARE @Parameters nvarchar(4000)' 
	SET @SP=@SP+CHAR(10)+'	SET @Statement=''UPDATE '+@tableName+' SET '+SUBSTRING(@UpdateAllVars,1,LEN(@UpdateAllVars)-1)+' WHERE '+@where+'''' 
	PRINT @SP 
	SET @SP='	SET @Parameters='''+SUBSTRING(@UpdateParaDefine,1,LEN(@UpdateParaDefine)-1)+'''' 
	SET @SP=@SP+CHAR(10)+'	EXEC SP_EXECUTESQL @Statement,@Parameters,'+SUBSTRING(@UpdateParaEqual,1,LEN(@UpdateParaEqual)-1) 
	--SET @SP=@SP+CHAR(10)+'	INSERT INTO '+@tableName+'('+SUBSTRING(@UpdateAllcolumns,1,LEN(@UpdateAllcolumns)-1)+')' 
	--SET @SP=@SP+CHAR(10)+'	VALUES('+SUBSTRING(@UpdateAllVars,1,LEN(@UpdateAllVars)-1)+')' 
	SET @SP=@SP+CHAR(10)+'END' 
	PRINT @SP 
	PRINT 'GO' 
	PRINT SUBSTRING(@ExecUpdate, 1, LEN(@ExecUpdate) - 1)
	--exec(@SP) 
	----END 
	DROP TABLE #tempXML 
	DROP TABLE #temp 
	DROP TABLE #Result 
END 

