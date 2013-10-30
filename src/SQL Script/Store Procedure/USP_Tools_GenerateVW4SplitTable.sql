SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Tools_GenerateVW4SplitTable') 
     DROP PROCEDURE USP_Tools_GenerateVW4SplitTable 
GO 

CREATE PROCEDURE [dbo].USP_Tools_GenerateVW4SplitTable 
( 
	@tablename varchar(50),--='ORD_OrderMstr' 
	@alisaname varchar(10),--='om' 
	@splitcount int--=9 
)
AS 
BEGIN 
	set nocount on
	--USP_Tools_GenerateVW4SplitTable 'ORD_OrderMstr','om',9 
	declare @statemnet varchar(8000) 
	declare @allcolumns varchar(8000) 
	select @allcolumns = isnull(@allcolumns,'')+','+@alisaname+'.'+c.name  from sys.objects o inner join sys.all_columns c 
	on o.object_id=c.object_id  
	where o.type='U' and o.name=@tablename 
	order by o.object_id,c.column_id 
	set @allcolumns = substring(@allcolumns,2,len(@allcolumns))  
	set @statemnet='SELECT '+@allcolumns+' FROM '+@tablename+' AS '+@alisaname 
	declare @i int=0 
	while(@i<=@splitcount) 
	begin 
		set @statemnet='SELECT '+@allcolumns+' FROM '+@tablename+'_'+CAST(@i as varchar(2))+' AS '+@alisaname 
		PRINT @statemnet 
		if(@i<@splitcount) 
		begin 
			PRINT 'UNION ALL' 
		end	 
		set @i=@i+1 
	end 
END