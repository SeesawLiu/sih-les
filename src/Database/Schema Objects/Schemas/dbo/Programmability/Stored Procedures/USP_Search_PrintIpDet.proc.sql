


CREATE  PROCEDURE [dbo].[USP_Search_PrintIpDet] 
( 
	@IpNo varchar(50), 
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
	@WhereStatement varchar(8000), 
	@SortCloumn varchar(50)=null, 
	@SortRule varchar(50)=null, 
	@PageSize int, 
	@Page int 
) 
AS 
BEGIN 
	SET NOCOUNT ON 
	DECLARE @SpliceTables nvarchar(4000) 
	DECLARE @Type varchar(5) 
	DECLARE @Statement nvarchar(4000) 
	DECLARE @Parameter nvarchar(4000) 
	DECLARE @PagePara nvarchar(4000) 
	DECLARE @SortDesc nvarchar(100) 
	--DECLARE @Where nvarchar(4000) 
	DECLARE @PermissionClause nvarchar(1000) 
	DECLARE @DetailWhere nvarchar(4000) 
	DECLARE @MasterWhere nvarchar(4000)	 
	SET @DetailWhere='WHERE 1=1 ' 
	SET @MasterWhere='WHERE 1=1 ' 

	IF(ISNULL(@OrderTypes,'')='') 
	BEGIN 
		RAISERROR ('Please choose order types to search!' , 16, 1) WITH NOWAIT 
	END 
	IF(ISNULL(@IPNo,'')<>'') 
	BEGIN 
		SET @IpNo='%'+@IpNo+'%' 
		SET @MasterWhere=@MasterWhere+' AND i.IpNo LIKE @IPNo_1' 
	END	 
	--IF(ISNULL(@OrderNo,'')<>'') 
	--BEGIN 
	--	SET @OrderNo=@OrderNo+'%' 
	--	SET @MasterWhere=@MasterWhere+' AND i.OrderNo LIKE @OrderNo_1' 
	--END 
	IF(ISNULL(@WMSNo,'')<>'') 
	BEGIN 
		SET @WMSNo=@WMSNo+'%' 
		SET @MasterWhere=@MasterWhere+' AND i.WMSNo LIKE @WMSNo_1' 
	END	 
	IF(ISNULL(@Flow,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND i.Flow=@Flow_1' 
	END 
	IF(ISNULL(@PartyFrom,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND i.PartyFrom=@PartyFrom_1' 
	END 
	IF(ISNULL(@PartyTo,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND i.PartyTo=@PartyTo_1 ' 
	END		 
	IF(@Status is not null) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND i.Status=@Status_1 ' 
	END 
	IF(ISDATE(@EndDate)=1)
	BEGIN
    set @EndDate=dateadd(day,1,@EndDate)
    END
	IF(ISDATE(@StartDate)=1 AND ISDATE(@EndDate)=1) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND i.CreateDate BETWEEN @StartDate_1 And @EndDate_1 ' 
	END	 
	ELSE IF(ISDATE(@StartDate)=1 AND ISDATE(@EndDate)=0) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND i.CreateDate > @StartDate_1' 
	END 
	ELSE IF(ISDATE(@StartDate)=0 AND ISDATE(@EndDate)=1) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND i.CreateDate < @EndDate_1' 
	END	 

	DECLARE @UserCode varchar(50) 
	SELECT @UserCode=Code FROM ACC_User WHERE Id=@UserId 
	IF(UPPER(@UserCode)<>'SU') 
	BEGIN 
		SELECT PermissionCode INTO #Temp FROM VIEW_UserPermission WHERE UserId=@UserId 
		IF(@IsSupplier=0) 
		BEGIN 
			SET @MasterWhere=@MasterWhere+' AND i.PartyTo IN (SELECT PermissionCode FROM #Temp) ' 
		END 
		ELSE  
		BEGIN 
			SET @MasterWhere=@MasterWhere+' AND (i.PartyFrom IN (SELECT PermissionCode FROM #Temp) OR i.PartyTo IN (SELECT PermissionCode FROM #Temp))  ' 
		END				 
	END 

	IF(ISNULL(@Item,'')<>'') 
	BEGIN 
		SET @DetailWhere=@DetailWhere+' AND d.Item=@Item_1' 
	END 
	IF(ISNULL(@ManufactureParty,'')<>'') 
	BEGIN 
		SET @DetailWhere=@DetailWhere+' AND d.ManufactureParty=@ManufactureParty_1' 
	END 
	IF(ISNULL(@Dock,'')<>'') 
	BEGIN 
		SET @DetailWhere=@DetailWhere+' AND d.Dock=@Dock_1' 
	END		
	
	IF(ISNULL(@OrderNo,'')<>'') 
	BEGIN 
		SET @OrderNo=@OrderNo+'%' 
		SET @DetailWhere=@DetailWhere+' AND d.OrderNo LIKE @OrderNo_1' 
	END 	 

	IF ISNULL(@SortCloumn,'')='' 
	BEGIN 
		SET @SortDesc=' ORDER BY MastCreateDate DESC' 
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
		IF(CHARINDEX('Receive',@SortCloumn)>0) 
		BEGIN 
			SET @SortCloumn=REPLACE(@SortCloumn,'Receive','Rec') 
		END					 
		SET @SortDesc=' ORDER BY '+@SortCloumn+' '+@SortRule 
		IF(CHARINDEX('Address',@SortCloumn)>0) 
		BEGIN 
			SET @SortCloumn=REPLACE(@SortCloumn,'Address','Addr') 
		END					 
		SET @SortDesc=' ORDER BY '+@SortCloumn+' '+@SortRule		 
	END 

	IF @Page>0 
	BEGIN 
		SET @PagePara='WHERE RowId BETWEEN '+cast((@PageSize*(@Page-1)+1) as varchar(50))+' AND '++cast(@PageSize*(@Page) as varchar(50)) 
	END 

	SET @MasterWhere=REPLACE(ISNULL(@WhereStatement,''),'Where',@MasterWhere+' AND')	 
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

		IF(ISNULL(@SpliceTables,'')='') 
		BEGIN 
		SET @SpliceTables='select d.Id,d.IpNo,d.OrderNo,d.ExtNo,d.ExtSeq,d.Item,d.RefItemCode,d.ItemDesc,d.Uom,d.UC,d.Qty,d.RecQty,d.LocFrom,d.LocTo,d.Flow,d.IsClose,d.IsInspect,m.PartyFrom as MastPartyFrom,m.PartyTo as MastPartyTo,m.OrderType as MastType,m.Status as MastStatus,m.CreateDate as MastCreateDate,l.SAPLocation from ORD_IpDet_'+@Type+' AS d  left join ORD_IpMstr_'+@Type+' as m on d.IpNo=m.IpNo left join MD_Location as l on d.LocTo=l.Code '+@DetailWhere+' '+REPLACE(@MasterWhere,'IpMaster','ORD_IpMstr_'+@Type+' ') 
			--SET @SpliceTables='SELECT * FROM ORD_IpDet_'+@Type+' AS d '+@DetailWhere+' '+REPLACE(@MasterWhere,'IpMaster','ORD_IpMstr_'+@Type+' ') 
		END 
		ELSE 
		BEGIN 
		SET @SpliceTables=@SpliceTables+' union all select d.Id,d.IpNo,d.OrderNo,d.ExtNo,d.ExtSeq,d.Item,d.RefItemCode,d.ItemDesc,d.Uom,d.UC,d.Qty,d.RecQty,d.LocFrom,d.LocTo,d.Flow,d.IsClose,d.IsInspect,m.PartyFrom as MastPartyFrom,m.PartyTo as MastPartyTo,m.OrderType as MastType,m.Status as MastStatus,m.CreateDate as MastCreateDate,l.SAPLocation from ORD_IpDet_'+@Type+' AS d  left join ORD_IpMstr_'+@Type+' as m on d.IpNo=m.IpNo left join MD_Location as l on d.LocTo=l.Code '+@DetailWhere+' '+REPLACE(@MasterWhere,'IpMaster','ORD_IpMstr_'+@Type+' ') 
			--SET @SpliceTables=@SpliceTables+' UNION ALL SELECT * FROM ORD_IpDet_'+@Type+' AS d  '+@DetailWhere+' '+REPLACE(@MasterWhere,'IpMaster','ORD_IpMstr_'+@Type+' ') 
		END 
		SET @OrderTypes=SUBSTRING(@OrderTypes,3,LEN(@OrderTypes)) 
	END	 


	SET @Statement=N'SELECT TOP('+CAST(@PageSize AS VARCHAR(10))+') * FROM (SELECT *,RowId=ROW_NUMBER()OVER('+@SortDesc+') FROM ('+@SpliceTables+') AS T1 ) AS T2 '+@PagePara 
	SET @Parameter=N'@IpNo_1 varchar(50),@Status_1 tinyint,	@OrderTypes_1 varchar(50),@PartyFrom_1 varchar(50),@PartyTo_1 varchar(50),@StartDate_1 datetime,@EndDate_1 datetime,@Dock_1 varchar(50),@Item_1 varchar(50),@OrderNo_1 varchar(50),@WMSNo_1 varchar(50),@ManufactureParty_1 varchar(50),@Flow_1 varchar(50),@IsSupplier_1 bit'		 
	PRINT @Statement 

	EXEC SP_EXECUTESQL @Statement,@Parameter, 
		@IpNo_1=@IpNo,@Status_1=@Status,@OrderTypes_1=@OrderTypes ,@PartyFrom_1=@PartyFrom,@PartyTo_1=@PartyTo ,@StartDate_1=@StartDate , 
		@EndDate_1=@EndDate ,@Dock_1=@Dock ,@Item_1=@Item ,@OrderNo_1=@OrderNo,@WMSNo_1=@WMSNo,@ManufactureParty_1=@ManufactureParty , 
		@Flow_1=@Flow,@IsSupplier_1=@IsSupplier 

END 


/****** Object:  StoredProcedure [dbo].[USP_Search_IpDetCount]    Script Date: 2013-01-02 12:32:11 ******/
SET ANSI_NULLS ON


