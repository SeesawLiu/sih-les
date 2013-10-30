
CREATE PROCEDURE [dbo].[USP_Search_ProcurementOrderCount] 
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
	@WhereStatement varchar(8000) 
) 
AS 
BEGIN 
	SET NOCOUNT ON 
	DECLARE @SpliceTables nvarchar(4000) 
	DECLARE @Type varchar(5) 
	DECLARE @CountStatement nvarchar(4000) 
	DECLARE @Parameter nvarchar(4000) 
	DECLARE @PagePara nvarchar(4000) 
	DECLARE @SortDesc nvarchar(100) 
	DECLARE @Where nvarchar(4000) 
	DECLARE @PermissionClause nvarchar(1000) 
	SET @Where='WHERE 1=1 '+@WhereStatement 

	PRINT @Where 
	IF(ISNULL(@Types,'')='') 
	BEGIN 
		RAISERROR ('Please choose order types to search!' , 16, 1) WITH NOWAIT 
	END 

	IF(ISNULL(@OrderNo,'')<>'') 
	BEGIN 
		SET @OrderNo='%'+@OrderNo+'%' 
		SET @Where=@Where+' AND o.OrderNo LIKE @OrderNo_1' 
	END 
	IF(ISNULL(@Flow,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.Flow=@Flow_1' 
	END 
	IF(@SubType is not null) 
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
		--IF(CHARINDEX('OrderDetail',@Where)>0) 
		--BEGIN 
		--	SET @Where=REPLACE(@Where,'OrderDetail','ORD_OrderDet_'+@Type) 
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

	SET @CountStatement=N'SELECT COUNT(1) FROM ('+@SpliceTables+') AS T1' 
	SET @Parameter=N'@OrderNo_1 varchar(50),@Flow_1 varchar(50),@PartyFrom_1 varchar(50),@PartyTo_1 varchar(50),@Status_1 tinyint,@Priority_1 tinyint,@ExtOrderNo_1 varchar(50),@RefOrderNo_1 varchar(50),@TraceCode_1 varchar(50),@CreateUserNm_1 varchar(100),@DateFrom_1 datetime,@DateTo_1 datetime,
	@StartTime_1 datetime,@EndTime_1 datetime,@WindowTimeFrom_1 datetime,@WindowTimeTo_1 datetime,@SubType_1 tinyint,@Sequence_1 bigint'		 
	PRINT @CountStatement 

	EXEC SP_EXECUTESQL @CountStatement,@Parameter, 
		@OrderNo_1=@OrderNo,@Flow_1=@Flow,@SubType_1=@SubType,@PartyFrom_1=@PartyFrom,@PartyTo_1=@PartyTo,@Status_1=@Status, 
		@Priority_1=@Priority,@ExtOrderNo_1=@ExtOrderNo,@RefOrderNo_1=@RefOrderNo,@TraceCode_1=@TraceCode, 
		@CreateUserNm_1=@CreateUserNm,@DateFrom_1=@DateFrom,@DateTo_1=@DateTo,@StartTime_1=@StartTime,@EndTime_1=@EndTime,
		@WindowTimeFrom_1=@WindowTimeFrom,@WindowTimeTo_1=@WindowTimeTo,@Sequence_1=@Sequence		 

END 
