IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Search_IpMstrCount') 
     DROP PROCEDURE USP_Search_IpMstrCount
/****** Object:  StoredProcedure [dbo].[USP_Search_IpMstrCount]    Script Date: 11/26/2012 11:19:55 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE  PROCEDURE [dbo].[USP_Search_IpMstrCount] 
( 
	@IPNo varchar(50), 
	@Status tinyint,	 
	@OrderTypes varchar(50), 
	@PartyFrom varchar(50), 
	@PartyTo varchar(50), 
	@StartDate datetime, 
	@EndDate datetime, 
	@Dock varchar(50), 
	@Item varchar(50), 
	@OrderNo varchar(50), 
	@WMSNo varchar(50), 
	@ManufactureParty varchar(50), 
	@Flow varchar(50), 
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
	SET @Where='WHERE 1=1 '+ISNULL(@WhereStatement,'') 

	IF(ISNULL(@OrderTypes,'')='') 
	BEGIN 
		RAISERROR ('Please choose order types to search!' , 16, 1) WITH NOWAIT 
	END 
	IF(ISNULL(@IPNo,'')<>'') 
	BEGIN 
		SET @IPNo=@IPNo+'%' 
		SET @Where=@Where+' AND i.IpNo LIKE @IPNo_1' 
	END	 
	IF(ISNULL(@OrderNo,'')<>'') 
	BEGIN 
		SET @OrderNo=@OrderNo+'%' 
		SET @Where=@Where+' AND i.OrderNo LIKE @OrderNo_1' 
	END 
	IF(ISNULL(@WMSNo,'')<>'') 
	BEGIN 
		SET @WMSNo=@WMSNo+'%' 
		SET @Where=@Where+' AND i.WMSNo LIKE @WMSNo_1' 
	END		 
	IF(ISNULL(@Flow,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND i.Flow=@Flow_1' 
	END 
	IF(ISNULL(@PartyFrom,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND i.PartyFrom=@PartyFrom_1' 
	END 
	IF(ISNULL(@PartyTo,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND i.PartyTo=@PartyTo_1 ' 
	END		 
	IF(@Status is not null) 
	BEGIN 
		SET @Where=@Where+' AND i.Status=@Status_1 ' 
	END 
	IF(ISDATE(@EndDate)=1)
	BEGIN
    set @EndDate=dateadd(day,1,@EndDate)
    END
	IF(ISDATE(@StartDate)=1 AND ISDATE(@EndDate)=1) 
	BEGIN 
		SET @Where=@Where+' AND i.CreateDate BETWEEN @StartDate_1 And @EndDate_1 ' 
	END	 
	ELSE IF(ISDATE(@StartDate)=1 AND ISDATE(@EndDate)=0) 
	BEGIN 
		SET @Where=@Where+' AND i.CreateDate > @StartDate_1' 
	END 
	ELSE IF(ISDATE(@StartDate)=0 AND ISDATE(@EndDate)=1) 
	BEGIN 
		SET @Where=@Where+' AND i.CreateDate < @EndDate_1' 
	END	 

	DECLARE @UserCode varchar(50) 
	SELECT @UserCode=Code FROM ACC_User WHERE Id=@UserId 
	IF(UPPER(@UserCode)<>'SU') 
	BEGIN 
		SELECT PermissionCode INTO #Temp FROM VIEW_UserPermission WHERE UserId=@UserId 
		IF(@IsSupplier=0) 
		BEGIN 
			SET @Where=@Where+' AND i.PartyTo IN (SELECT PermissionCode FROM #Temp) ' 
		END 
		ELSE  
		BEGIN 
			SET @Where=@Where+' AND (i.PartyFrom IN (SELECT PermissionCode FROM #Temp) OR i.PartyTo IN (SELECT PermissionCode FROM #Temp))  ' 
		END				 
	END 

	WHILE(LEN(@OrderTypes)>0) 
	BEGIN 
		IF(LEN(@OrderTypes)=1) 
		BEGIN 
			SET @Type=@OrderTypes 
		END 
		ELSE 
		BEGIN 
			SET @Type=SUBSTRING(@OrderTypes,1,CHARINDEX(',',@OrderTypes)-1) 
		END 
		--IF(CHARINDEX('OrderDetail',@WhereStatement)>0) 
		--BEGIN 
		--	SET @WhereStatement=REPLACE(@WhereStatement,'OrderDetail','ORD_OrderDet'+@Type) 
		--END			 
		IF(ISNULL(@SpliceTables,'')='') 
		BEGIN 
			SET @SpliceTables='SELECT * FROM ORD_IpMstr_'+@Type+' AS i '+REPLACE(@Where,'IPDetail','ORD_IpDet_'+@Type) 
		END 
		ELSE 
		BEGIN 
			SET @SpliceTables=@SpliceTables+' UNION ALL SELECT * FROM ORD_IpMstr_'+@Type+' AS i '+REPLACE(@Where,'IPDetail','ORD_IpDet_'+@Type) 
		END 
		SET @OrderTypes=SUBSTRING(@OrderTypes,3,LEN(@OrderTypes)) 
	END	 

	SET @CountStatement=N'SELECT COUNT(1) FROM ('+@SpliceTables+') AS T1' 
	SET @Parameter=N'@IPNo_1 varchar(50),@Status_1 tinyint,	@OrderTypes_1 varchar(50),@PartyFrom_1 varchar(50),@PartyTo_1 varchar(50),@StartDate_1 datetime,@EndDate_1 datetime,@Dock_1 varchar(50),@Item_1 varchar(50),@OrderNo_1 varchar(50),@WMSNo_1 varchar(50),@ManufactureParty_1 varchar(50),@Flow_1 varchar(50),@IsSupplier_1 bit'		 

	EXEC SP_EXECUTESQL @CountStatement,@Parameter, 
		@IPNo_1=@IPNo,@Status_1=@Status,@OrderTypes_1=@OrderTypes ,@PartyFrom_1=@PartyFrom,@PartyTo_1=@PartyTo ,@StartDate_1=@StartDate , 
		@EndDate_1=@EndDate ,@Dock_1=@Dock ,@Item_1=@Item ,@OrderNo_1=@OrderNo,@WMSNo_1=@WMSNo,@ManufactureParty_1=@ManufactureParty , 
		@Flow_1=@Flow,@IsSupplier_1=@IsSupplier 

END 

GO


