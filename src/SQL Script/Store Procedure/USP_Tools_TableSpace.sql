SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Tools_TableSpace') 
     DROP PROCEDURE USP_Tools_TableSpace 
GO

CREATE PROCEDURE [dbo].USP_Tools_TableSpace 
AS 
begin      
	DECLARE @tblname varchar(50)            --資料表名稱(使用者資料表名稱變數)
	if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[#TableStatics_TEMP]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
	DROP TABLE [dbo].[#TableStatics_TEMP]   --暫存資料表
	 

	CREATE TABLE #TableStatics_TEMP (       --建立暫存資料表
		[name] [varchar] (50) COLLATE Chinese_Taiwan_Stroke_CI_AS NULL ,        --資料表名稱
		[rows] [varchar] (50) COLLATE Chinese_Taiwan_Stroke_CI_AS NULL ,        --資料表現有的資料列數
		[reserved] [varchar] (50) COLLATE Chinese_Taiwan_Stroke_CI_AS NULL ,    --資料庫中的物件所配置的空間大小
		[data] [varchar] (50) COLLATE Chinese_Taiwan_Stroke_CI_AS NULL ,        --資料所用的空間大小
		[index_size] [varchar] (50) COLLATE Chinese_Taiwan_Stroke_CI_AS NULL ,  --索引所用的空間大小
		[unused] [varchar] (50) COLLATE Chinese_Taiwan_Stroke_CI_AS NULL )      --保留給資料庫中之物件但尚未使用的空間大小
	 

	DECLARE cur_TableStatics CURSOR FORWARD_ONLY FOR
		SELECT name FROM sysobjects WHERE xtype='U' ORDER BY name               --取得使用者資料表名稱
	OPEN cur_TableStatics
	FETCH NEXT FROM cur_TableStatics
	INTO @tblname
		WHILE @@FETCH_STATUS = 0
			BEGIN  
				INSERT #TableStatics_TEMP
					EXEC sp_spaceused @tblname,@updateusage = N'TRUE'           --顯示資料表的相關磁碟空間資訊
				FETCH NEXT FROM cur_TableStatics
				INTO @tblname
			END
	CLOSE cur_TableStatics
	DEALLOCATE cur_TableStatics
	   
	SELECT name,rows,
		CONVERT(varchar(18),CONVERT(NUMERIC(18,2),SUBSTRING(reserved,1,LEN(reserved)-2))/1024) + 'MB' AS reserved,
		CONVERT(varchar(18),CONVERT(NUMERIC(18,2),SUBSTRING(data,1,LEN(data)-2))/1024) + 'MB' AS data,
		CONVERT(varchar(18),CONVERT(NUMERIC(18,2),SUBSTRING(index_size,1,LEN(index_size)-2))/1024) + 'MB' AS index_siz,
		CONVERT(varchar(18),CONVERT(NUMERIC(18,2),SUBSTRING(unused,1,LEN(unused)-2))/1024) + 'MB' AS unused
	FROM #TableStatics_TEMP where rows <> '0'           --查詢暫存資料表資料比數不為的資料表
	 

	DROP TABLE #TableStatics_TEMP                               --移出暫存資料表
end            
