USE [sconit5_SIH]
GO

/****** Object:  StoredProcedure [dbo].[USP_Search_ProcurementOrder]    Script Date: 11/28/2012 14:05:01 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO


 ALTER PROCEDURE [dbo].[USP_Search_ProcurementOrder] 
( 
	@OrderNo varchar(50), 
	@Flow varchar(50), 
	@Types varchar(50), 
	@SubType tinyint, 
	@PartyFrom varchar(50), 
	@PartyTo varchar(50), 
	@Status tinyint, 
	@Priority tinyint, 
	@ExtOrderNo varchar(50), 
	@RefOrderNo varchar(50), 
	@TraceCode varchar(50), 
	@CreateUserNm varchar(100), 
	@DateFrom datetime, 
	@DateTo datetime,
	@StartTime datetime, 
	@EndTime datetime,
	@WindowTimeFrom datetime, 
	@WindowTimeTo datetime,   
	@Sequence bigint, 
	@IsReturn bit, 
	@IsSupplier bit, 
	@UserId int, 
	@WhereStatement varchar(8000), 
	@SortCloumn varchar(50)=null, 
	@SortRule varchar(50)=null, 
	@PageSize int, 
	@Page int 
) 
AS 
BEGIN 
/* 
exec sp_executesql N'exec USP_Search_ProcurementOrder @p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22',N'@p0 nvarchar(4000),@p1 nvarchar(4000),@p2 nvarchar(4000),@p3 nvarchar(4000),@p4 nvarchar(4000),@p5 nvarchar(4000),@p6 smallint,@p7 smallint,@p8 nvarchar(4000),@p9 nvarchar(4000),@p10 nvarchar(4000),@p11 nvarchar(4000),@p12 datetime,@p13 datetime,@p14 bigint,@p15 bit,@p16 bit,@p17 int,@p18 nvarchar(4000),@p19 nvarchar(4000),@p20 nvarchar(4000),@p21 int,@p22 int',@p0=NULL,@p1=NULL,@p2=N'6,1,5,2,7,8',@p3=N'0',@p4=NULL,@p5=NULL,@p6=0,@p7=NULL,@p8=NULL,@p9=NULL,@p10=NULL,@p11=NULL,@p12=NULL,@p13='2012-10-19 00:00:00',@p14=NULL,@p15=0,@p16=0,@p17=2603,@p18=N'and o.OrderStrategy != 4',@p19=NULL,@p20=N'asc',@p21=20,@p22=1 
declare @i tinyint 
set @i=0 
select isnull(@i,'') 
*/ 
	SET NOCOUNT ON 
	DECLARE @SpliceTables nvarchar(4000) 
	DECLARE @Type varchar(5) 
	DECLARE @Statement nvarchar(4000) 
	DECLARE @Parameter nvarchar(4000) 
	DECLARE @PagePara nvarchar(4000) 
	DECLARE @SortDesc nvarchar(100) 
	DECLARE @Where nvarchar(4000) 
	DECLARE @PermissionClause nvarchar(1000) 
	SET @Where='WHERE 1=1 '+@WhereStatement 

	PRINT @Status 
	IF(ISNULL(@Types,'')='') 
	BEGIN 
		RAISERROR ('Please choose order types to search!' , 16, 1) WITH NOWAIT 
	END 

	IF(ISNULL(@OrderNo,'')<>'') 
	BEGIN 
		SET @OrderNo=@OrderNo+'%' 
		SET @Where=@Where+' AND o.OrderNo LIKE @OrderNo_1' 
	END 
	IF(ISNULL(@Flow,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.Flow=@Flow_1' 
	END 
	IF(ISNULL(@SubType,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.SubType=@SubType_1' 
	END	 
	IF(ISNULL(@PartyFrom,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.PartyFrom=@PartyFrom_1' 
	END 
	IF(ISNULL(@PartyTo,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.PartyTo=@PartyTo_1 ' 
	END	 

	IF(@Status is not null) 
	BEGIN 
		PRINT @Status 
		SET @Where=@Where+' AND o.Status=@Status_1 ' 
	END		 
	IF(ISNULL(@Priority,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.Priority=@Priority_1 ' 
	END	 
	IF(ISNULL(@ExtOrderNo,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.ExtOrderNo LIKE @ExtOrderNo_1 ' 
	END	 
	IF(ISNULL(@RefOrderNo,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.RefOrderNo LIKE @RefOrderNo_1 ' 
	END		 
	IF(ISNULL(@TraceCode,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.TraceCode LIKE @TraceCode_1 ' 
	END	 
	IF(ISNULL(@CreateUserNm,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.CreateUserNm=@CreateUserNm_1 ' 
	END	 
	--创建时间
    IF(ISDATE(@DateTo)=1)
	BEGIN
    set @DateTo=dateadd(day,1,@DateTo)
    END
	IF(ISDATE(@DateFrom)=1 AND ISDATE(@DateTo)=1) 
	BEGIN 
		SET @Where=@Where+' AND o.CreateDate BETWEEN @DateFrom_1 And @DateTo_1 ' 
	END	 
	ELSE IF(ISDATE(@DateFrom)=1 AND ISDATE(@DateTo)=0) 
	BEGIN 
		SET @Where=@Where+' AND o.CreateDate > @DateFrom_1' 
	END 
	ELSE IF(ISDATE(@DateFrom)=0 AND ISDATE(@DateTo)=1) 
	BEGIN 
		SET @Where=@Where+' AND o.CreateDate < @DateTo_1' 
	END	 
	--开始时间
	IF(ISDATE(@EndTime)=1)
	BEGIN
    set @EndTime=dateadd(day,1,@EndTime)
    END
	IF(ISDATE(@StartTime)=1 AND ISDATE(@EndTime)=1) 
	BEGIN 
		SET @Where=@Where+' AND o.StartTime BETWEEN @StartTime_1 And @EndTime_1 ' 
	END	 
	ELSE IF(ISDATE(@StartTime)=1 AND ISDATE(@EndTime)=0) 
	BEGIN 
		SET @Where=@Where+' AND o.StartTime > @StartTime_1' 
	END 
	ELSE IF(ISDATE(@StartTime)=0 AND ISDATE(@EndTime)=1) 
	BEGIN 
		SET @Where=@Where+' AND o.StartTime < @EndTime_1' 
	END	 
	
	--窗口时间
	IF(ISDATE(@WindowTimeTo)=1)
	BEGIN
    set @WindowTimeTo=dateadd(day,1,@WindowTimeTo)
    END
	IF(ISDATE(@WindowTimeFrom)=1 AND ISDATE(@WindowTimeTo)=1) 
	BEGIN 
		SET @Where=@Where+' AND o.WindowTime BETWEEN @WindowTimeFrom_1 And @WindowTimeTo_1 ' 
	END	 
	ELSE IF(ISDATE(@WindowTimeFrom)=1 AND ISDATE(@WindowTimeTo)=0) 
	BEGIN 
		SET @Where=@Where+' AND o.WindowTime > @WindowTimeFrom_1' 
	END 
	ELSE IF(ISDATE(@WindowTimeFrom)=0 AND ISDATE(@WindowTimeTo)=1) 
	BEGIN 
		SET @Where=@Where+' AND o.WindowTime < @WindowTimeTo_1' 
	END	 

	DECLARE @UserCode varchar(50) 
	SELECT @UserCode=Code FROM ACC_User WHERE Id=@UserId 
	IF(UPPER(@UserCode)<>'SU') 
	BEGIN 
		SELECT PermissionCode INTO #Temp FROM VIEW_UserPermission WHERE UserId=@UserId 
		IF(@IsSupplier=0 AND @IsReturn=0) 
		BEGIN 
			SET @Where=@Where+' AND o.PartyTo IN (SELECT PermissionCode FROM #Temp) ' 
		END 
		ELSE IF(@IsSupplier=0 AND @IsReturn=1) 
		BEGIN 
			SET @Where=@Where+' AND o.PartyFrom IN (SELECT PermissionCode FROM #Temp) ' 
		END 
		ELSE  
		BEGIN 
			SET @Where=@Where+' AND (o.PartyFrom IN (SELECT PermissionCode FROM #Temp) OR o.PartyTo IN (SELECT PermissionCode FROM #Temp))  ' 
		END				 
	END 

	IF ISNULL(@SortCloumn,'')='' 
	BEGIN 
		SET @SortDesc=' ORDER BY CreateDate DESC' 
	END 
	ELSE 
	BEGIN 
		IF(ISNULL(@SortRule,'')='') 
		BEGIN 
			SET @SortRule=' ACS' 
		END 
		IF(CHARINDEX('Reference',@SortCloumn)>0) 
		BEGIN 
			SET @SortCloumn=REPLACE(@SortCloumn,'Reference','Ref') 
		END 
		IF(CHARINDEX('Name',@SortCloumn)>0) 
		BEGIN 
			SET @SortCloumn=REPLACE(@SortCloumn,'Name','NM') 
		END 
		IF(CHARINDEX('UnitCount',@SortCloumn)>0) 
		BEGIN 
			SET @SortCloumn=REPLACE(@SortCloumn,'UnitCount','UC') 
		END 
		IF(CHARINDEX('Sequence',@SortCloumn)>0) 
		BEGIN 
			SET @SortCloumn=REPLACE(@SortCloumn,'Sequence','Seq') 
		END					 
		SET @SortDesc=' ORDER BY '+@SortCloumn+' '+@SortRule 
	END 

	IF @Page>0 
	BEGIN 
		SET @PagePara='WHERE RowId BETWEEN '+cast(@PageSize*(@Page-1) as varchar(50))+' AND '++cast(@PageSize*(@Page) as varchar(50)) 
	END 

	WHILE(LEN(@Types)>0) 
	BEGIN 
		IF(LEN(@Types)=1) 
		BEGIN 
			SET @Type=@Types 
		END 
		ELSE 
		BEGIN 
			SET @Type=SUBSTRING(@Types,1,CHARINDEX(',',@Types)-1) 
		END 
		--IF(CHARINDEX('OrderDetail',@WhereStatement)>0) 
		--BEGIN 
		--	SET @WhereStatement=REPLACE(@WhereStatement,'OrderDetail','ORD_OrderDet'+@Type) 
		--END			 
		IF(ISNULL(@SpliceTables,'')='') 
		BEGIN 
			SET @SpliceTables='SELECT * FROM ORD_OrderMstr_'+@Type+' AS o '+REPLACE(@Where,'OrderDetail','ORD_OrderDet_'+@Type) 
		END 
		ELSE 
		BEGIN 
			SET @SpliceTables=@SpliceTables+' UNION ALL SELECT * FROM ORD_OrderMstr_'+@Type+' AS o '+REPLACE(@Where,'OrderDetail','ORD_OrderDet_'+@Type) 
		END 
		SET @Types=SUBSTRING(@Types,3,LEN(@Types)) 
	END	 
	SET @Statement=N'SELECT TOP('+CAST(@PageSize AS VARCHAR(10))+') * FROM (SELECT *,RowId=ROW_NUMBER()OVER('+@SortDesc+') FROM ('+@SpliceTables+') AS T1 ) AS T2 '+@PagePara 
	PRINT @Statement 
	PRINT LEN(@Statement) 
	SET @Parameter=N'@OrderNo_1 varchar(50),@Flow_1 varchar(50),@PartyFrom_1 varchar(50),@PartyTo_1 varchar(50),@Status_1 tinyint,@Priority_1 tinyint,@ExtOrderNo_1 varchar(50),@RefOrderNo_1 varchar(50),@TraceCode_1 varchar(50),@CreateUserNm_1 varchar(100),
	@DateFrom_1 datetime,@DateTo_1 datetime,@StartTime_1 datetime,@EndTime_1 datetime,@WindowTimeFrom_1 datetime,@WindowTimeTo_1 datetime,@SubType_1 tinyint,@Sequence_1 bigint'		 


	EXEC SP_EXECUTESQL @Statement,@Parameter, 
		@OrderNo_1=@OrderNo,@Flow_1=@Flow,@SubType_1=@SubType,@PartyFrom_1=@PartyFrom,@PartyTo_1=@PartyTo,@Status_1=@Status, 
		@Priority_1=@Priority,@ExtOrderNo_1=@ExtOrderNo,@RefOrderNo_1=@RefOrderNo,@TraceCode_1=@TraceCode, 
		@CreateUserNm_1=@CreateUserNm,@DateFrom_1=@DateFrom,@DateTo_1=@DateTo,@StartTime_1=@StartTime,@EndTime_1=@EndTime,
		@WindowTimeFrom_1=@WindowTimeFrom,@WindowTimeTo_1=@WindowTimeTo,@Sequence_1=@Sequence	 

END 


GO


