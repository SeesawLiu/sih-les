IF EXISTS(SELECT * FROM SYS.OBJECTS WHERE TYPE='P' AND name='USP_Report_GetHistoryInv')
	DROP PROCEDURE USP_Report_GetHistoryInv	
GO
CREATE PROCEDURE [dbo].[USP_Report_GetHistoryInv]
(
	@Locations varchar(8000),
	@Items varchar(8000),
	@HistoryData datetime,
	@SortDesc varchar(100),
	@PageSize int,
	@Page int,
	@IsSummaryBySAPLoc bit,
	@SummaryLevel int
)
AS
BEGIN
	SET NOCOUNT ON
/*
	exec USP_Report_GetHistoryInv @Locations='',@Items='',@HistoryData='2012-07-30 00:00:00',@SortDesc=NULL,@PageSize=0,@Page=1,@IsSummaryBySAPLoc=1,@SummaryLevel=0
*/
	IF EXISTS(SELECT * FROM MD_FinanceCalendar a WHERE EndDate>@HistoryData AND IsClose=1)
	BEGIN
		SELECT Id,ABS(DATEDIFF(SECOND,StartDate,@HistoryData)) AS StartGap,ABS(DATEDIFF(SECOND,EndDate,@HistoryData)) AS EndGap
		INTO #TMPGAP FROM MD_FinanceCalendar WHERE IsClose=1
		DECLARE @StartGap bigint,@EndGap bigint,@FinanceCalendarId int,@InvDate datetime,@StartDate datetime
		SELECT @StartGap=MIN(StartGap) FROM #TMPGAP
		SELECT TOP 1 @FinanceCalendarId=Id FROM #TMPGAP WHERE StartGap=@StartGap
		SELECT @StartDate=StartDate FROM dbo.MD_FinanceCalendar WHERE Id=@FinanceCalendarId
		SET @InvDate=DATEADD(DAY,DATEDIFF(SECOND,@StartDate,@HistoryData)/(60*60*24),@StartDate)
	END
	ELSE
	BEGIN
		SELECT TOP 1 @InvDate=DATEADD(DAY,-1,EndDate) FROM MD_FinanceCalendar WHERE IsClose=1 ORDER BY Id DESC
	END	
	
	IF ISNULL(@InvDate,'')=''
	BEGIN
		RAISERROR (N'没有维护有效的财政月!' , 16, 1) WITH NOWAIT
		RETURN
	END
	--PRINT @InvDate
	--RETURN
	DECLARE @SqlHeader varchar(8000),@SqlTail varchar(8000)
	DECLARE @SqlDetail1 varchar(8000),@SqlDetail2 varchar(8000),@SqlDetail3 varchar(8000),@SqlDetail4 varchar(8000),
	@SqlDetail5 varchar(8000),@SqlDetail6 varchar(8000),@SqlDetail7 varchar(8000),@SqlDetail8 varchar(8000),
	@SqlDetail9 varchar(8000),@SqlDetail10 varchar(8000),@SqlDetail11 varchar(8000)
	DECLARE @i int
	DECLARE @max int
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameter nvarchar(4000)
	DECLARE @ItemWhere varchar(8000)=''
	DECLARE @LocationWhere varchar(8000)=''
	DECLARE @PagePara varchar(8000)=''
	DECLARE @TableSuffix varchar(50)
	SET @TableSuffix=CONVERT(varchar(6),@InvDate,112)
	
	IF(ISNULL(@Locations,'')<>'')
	BEGIN
		SET @LocationWhere=ISNULL(@LocationWhere,'')+' AND Location IN ('''+replace(@Locations,',',''',''')+''') '
	END
	
	IF(ISNULL(@Items,'')<>'')
	BEGIN
		SET @ItemWhere=ISNULL(@ItemWhere,'')+' AND lt.item in ('''+replace(@Items,',',''',''')+''') '
	END

	---排序条件
	IF ISNULL(@SortDesc,'')=''
	BEGIN
		SET @SortDesc=' ORDER BY Location ASC'
	END
		
	---查询出结果时需要的条件
	IF @Page>0
	BEGIN
		SET @PagePara='WHERE rowid BETWEEN '+cast(@PageSize*(@Page-1) as varchar(50))+' AND '++cast(@PageSize*(@Page) as varchar(50))
	END
	
	CREATE TABLE #tempInvSummary(Item varchar(50), Location varchar(50), ManufactureParty varchar(100),
			LotNo varchar(50), IsCs bit, QualifyQty decimal(18,8), InspectQty decimal(18,8), RejectQty decimal(18,8),
			TobeQualifyQty decimal(18,8), TobeInspectQty decimal(18,8), TobeRejectQty decimal(18,8))
	CREATE TABLE #tempTransSummaryNoGroup(Item varchar(50),Location varchar(50), ManufactureParty varchar(100),
			LotNo varchar(50), IsCs bit, QualifyQty decimal(18,8), InspectQty decimal(18,8), RejectQty decimal(18,8),
			TobeQualifyQty decimal(18,8), TobeInspectQty decimal(18,8), TobeRejectQty decimal(18,8))
	CREATE TABLE #tempTransSummary(Item varchar(50),Location varchar(50), ManufactureParty varchar(100),
			LotNo varchar(50), IsCs bit, QualifyQty decimal(18,8), InspectQty decimal(18,8), RejectQty decimal(18,8),
			TobeQualifyQty decimal(18,8), TobeInspectQty decimal(18,8), TobeRejectQty decimal(18,8))			
	
	CREATE TABLE #TempResult(RowId int,Item varchar(50),Location varchar(50), ManufactureParty varchar(100), LotNo varchar(50), 
			CSQty decimal(18,8), QualifyQty decimal(18,8), InspectQty decimal(18,8), RejectQty decimal(18,8),
			TobeQualifyQty decimal(18,8), TobeInspectQty decimal(18,8), TobeRejectQty decimal(18,8))
		
	--SELECT id=ROW_NUMBER()OVER(ORDER BY name),name into #tempSumTabName FROM sys.objects WHERE type=N'U' AND name like N'INV_LocTrans_%'
	SELECT @SqlHeader='',@SqlTail='',@max=19,@i=0, @SqlDetail1='',@SqlDetail2='',@SqlDetail3='',@SqlDetail4='',
	@SqlDetail5='',@SqlDetail6='',@SqlDetail7='',@SqlDetail8='',
	@SqlDetail9='',@SqlDetail10='',@SqlDetail11=''

	SELECT Item, Location, ManufactureParty, LotNo, IsCs, QualifyQty, InspectQty, RejectQty, TobeQualifyQty, TobeInspectQty, TobeRejectQty into #tempStandarInv from INV_DailyInvBalance where 1<>1
	
	
	
	--查找基准库存
	set @SqlHeader='select Item, Location, ManufactureParty, LotNo, IsCs, QualifyQty, InspectQty, RejectQty, TobeQualifyQty, TobeInspectQty, TobeRejectQty 
		from INV_DailyInvBalance_'+@TableSuffix+' AS lt where InvDate = '''+CONVERT(varchar(19),@InvDate,121)+''' '+@ItemWhere+' '+@LocationWhere+''
	print @InvDate;
	--print @i;
	--EXEC(@SqlHeader)
	INSERT INTO #tempStandarInv
	EXEC(@SqlHeader)	
	--循环结算每张表期末库存
	WHILE @i<@max
	BEGIN
		--PRINT @ItemWhere
		--PRINT @LocationWhere

		SET @SqlHeader='select Item, Location, ManufactureParty, LotNo, IsCs, 
			SUM(CASE WHEN QualityType = 0 then Qty else 0 end) as QualifyQty,
			SUM(CASE WHEN QualityType = 1 then Qty else 0 end) as InspectQty,
			SUM(CASE WHEN QualityType = 2 then Qty else 0 end) as RejectQty,
			SUM(CASE WHEN QualityType = 0 then TobeQty else 0 end) as TobeQualifyQty,
			SUM(CASE WHEN QualityType = 1 then TobeQty else 0 end) as TobeInspectQty,
			SUM(CASE WHEN QualityType = 2 then TobeQty else 0 end) as TobeRejectQty
			from 
			('

		SET @SqlDetail1='SELECT lt.Item, lt.LocTo AS Location, hu.ManufactureParty AS ManufactureParty, lt.LotNo, lt.IsCs, lt.QualityType, 
				SUM(lt.Qty) AS Qty,0 AS TobeQty
				FROM INV_LocTrans_'+Cast(@i AS varchar(10))+' AS lt
				LEFT JOIN INV_Hu AS hu ON lt.Huid=hu.Huid 
				WHERE lt.EffDate > '''+Convert(varchar(19), @InvDate, 121)+'''  and lt.EffDate <= '''+Convert(varchar(19), @HistoryData, 121)+''' '+@ItemWhere+'
				AND lt.IOType=0 AND lt.IsCs=0
				AND TransType NOT IN (201, 202, 203, 204, 205, 206)
				GROUP BY lt.Item, lt.LocTo, hu.ManufactureParty , lt.LotNo, lt.IsCs, lt.QualityType
				UNION ALL '
		
		SET @SqlDetail2='SELECT lt.Item, lt.LocFrom AS Location, hu.ManufactureParty AS ManufactureParty, lt.LotNo, lt.IsCs, lt.QualityType, 
				SUM(lt.Qty) AS Qty,0 AS TobeQty
				FROM INV_LocTrans_'+Cast(@i AS varchar(10))+' AS lt
				LEFT JOIN INV_Hu AS hu ON lt.Huid=hu.Huid 
				WHERE lt.EffDate > '''+Convert(varchar(19), @InvDate, 121)+'''  and lt.EffDate <= '''+Convert(varchar(19), @HistoryData, 121)+''' '+@ItemWhere+'
				AND lt.IOType=1 AND lt.IsCs=0
				AND TransType NOT IN (201, 202, 203, 204, 205, 206)
				GROUP BY lt.Item, lt.LocFrom, hu.ManufactureParty , lt.LotNo, lt.IsCs, lt.QualityType
				UNION ALL '
				
		SET @SqlDetail3='SELECT lt.Item, lt.LocTo AS Location, pb.Party AS ManufactureParty, lt.LotNo, lt.IsCs, lt.QualityType, 
				SUM(lt.Qty) AS Qty,0 AS TobeQty
				FROM INV_LocTrans_'+Cast(@i AS varchar(10))+' AS lt
				LEFT JOIN BIL_PlanBill AS pb ON lt.PlanBill=pb.Id 
				WHERE lt.EffDate > '''+Convert(varchar(19), @InvDate, 121)+'''  and lt.EffDate <= '''+Convert(varchar(19), @HistoryData, 121)+''' '+@ItemWhere+'
				AND lt.IOType=0 AND lt.IsCs=1 AND lt.ActBill=0
				AND TransType NOT IN (201, 202, 203, 204, 205, 206)
				GROUP BY lt.Item, lt.LocTo, pb.Party, lt.LotNo, lt.IsCs, lt.QualityType
				UNION ALL '
				
		SET @SqlDetail4='SELECT lt.Item, lt.LocFrom, pb.Party AS ManufactureParty, lt.LotNo, lt.IsCs, lt.QualityType, 
				SUM(lt.Qty) AS Qty,0 AS TobeQty
				FROM INV_LocTrans_'+Cast(@i AS varchar(10))+' AS lt
				LEFT JOIN BIL_PlanBill AS pb ON lt.PlanBill=pb.Id 
				WHERE lt.EffDate > '''+Convert(varchar(19), @InvDate, 121)+'''  and lt.EffDate <= '''+Convert(varchar(19), @HistoryData, 121)+''' '+@ItemWhere+'
				AND lt.IOType=1 AND lt.IsCs=1 AND lt.ActBill=0
				AND TransType NOT IN (201, 202, 203, 204, 205, 206)
				GROUP BY lt.Item, lt.LocFrom, pb.Party, lt.LotNo, lt.IsCs, lt.QualityType
				UNION ALL'
				
		SET @SqlDetail4='SELECT lt.Item, lt.LocTo AS Location, ab.Party AS ManufactureParty, lt.LotNo, lt.IsCs, lt.QualityType, 
				SUM(lt.PlanBillQty) AS Qty,0 AS TobeQty
				FROM INV_LocTrans_'+Cast(@i AS varchar(10))+' AS lt
				LEFT JOIN BIL_ActBill AS ab ON lt.ActBill=ab.Id 
				WHERE lt.EffDate > '''+Convert(varchar(19), @InvDate, 121)+'''  and lt.EffDate <= '''+Convert(varchar(19), @HistoryData, 121)+''' '+@ItemWhere+'
				AND lt.IOType=0 AND lt.ActBill<>0
				AND TransType NOT IN (201, 202, 203, 204, 205, 206)
				GROUP BY lt.Item, lt.LocTo, ab.Party, lt.LotNo, lt.IsCs, lt.QualityType
				UNION ALL '
				
		SET @SqlDetail5='SELECT lt.Item, lt.LocFrom AS Location, ab.Party AS ManufactureParty, lt.LotNo, lt.IsCs, lt.QualityType, 
				SUM(lt.PlanBillQty) AS Qty,0 AS TobeQty
				FROM INV_LocTrans_'+Cast(@i AS varchar(10))+' AS lt
				LEFT JOIN BIL_ActBill AS ab ON lt.ActBill=ab.Id 
				WHERE lt.EffDate > '''+Convert(varchar(19), @InvDate, 121)+'''  and lt.EffDate <= '''+Convert(varchar(19), @HistoryData, 121)+''' '+@ItemWhere+'
				AND lt.IOType=1 AND lt.ActBill<>0
				AND TransType NOT IN (201, 202, 203, 204, 205, 206)
				GROUP BY lt.Item, lt.LocFrom, ab.Party, lt.LotNo, lt.IsCs, lt.QualityType
				UNION ALL '
				
		SET @SqlDetail6='SELECT lt.Item, CASE WHEN TransType in (201,205,202,206) THEN lt.LocTo ELSE lt.LocFrom END AS Location, CASE WHEN lt.ActBill<>0 THEN ab.Party ELSE pb.Party END AS ManufactureParty, 
				lt.LotNo, lt.IsCs, lt.QualityType, 
				SUM(lt.Qty) AS Qty,0 AS TobeQty
				FROM INV_LocTrans_'+Cast(@i AS varchar(10))+' AS lt
				LEFT JOIN BIL_PlanBill AS pb ON lt.PlanBill=pb.Id
				LEFT JOIN BIL_ActBill AS ab ON lt.ActBill=ab.Id
				WHERE lt.EffDate > '''+Convert(varchar(19), @InvDate, 121)+'''  and lt.EffDate <= '''+Convert(varchar(19), @HistoryData, 121)+''' '+@ItemWhere+'
				AND TransType IN (201, 202, 203, 204, 205, 206)
				GROUP BY lt.Item, CASE WHEN TransType in (201,205,202,206) THEN lt.LocTo ELSE lt.LocFrom END,
					CASE WHEN lt.ActBill<>0 THEN ab.Party ELSE pb.Party END, lt.LotNo, lt.IsCs, lt.QualityType
				UNION ALL '
				
		SET @SqlDetail7='SELECT lt.Item, lt.LocTo AS Location, hu.ManufactureParty AS ManufactureParty, lt.LotNo, lt.IsCs, lt.QualityType, 
				0 AS QTY,SUM(-lt.Qty) AS TobeQty
				FROM INV_LocTrans_'+Cast(@i AS varchar(10))+' AS lt
				LEFT JOIN INV_Hu AS hu ON lt.Huid=hu.Huid 
				WHERE lt.EffDate > '''+Convert(varchar(19), @InvDate, 121)+'''  and lt.EffDate <= '''+Convert(varchar(19), @HistoryData, 121)+''' '+@ItemWhere+'
				AND lt.IsCs=0 AND lt.ActBill=0 AND IOType = 1 and TransType in (301, 305, 304, 308, 311, 315, 314, 318) 
				GROUP BY lt.Item, lt.LocTo,hu.ManufactureParty, lt.LotNo, lt.IsCs, lt.QualityType
				UNION ALL '
				
		SET @SqlDetail8='SELECT lt.Item, lt.LocTo AS Location, hu.ManufactureParty AS ManufactureParty, lt.LotNo, lt.IsCs, lt.QualityType, 
				0 AS QTY,SUM(-lt.Qty) AS TobeQty
				FROM INV_LocTrans_'+Cast(@i AS varchar(10))+' AS lt
				LEFT JOIN INV_Hu AS hu ON lt.Huid=hu.Huid 
				WHERE lt.EffDate > '''+Convert(varchar(19), @InvDate, 121)+'''  and lt.EffDate <= '''+Convert(varchar(19), @HistoryData, 121)+''' '+@ItemWhere+'
				AND lt.IsCs=0 AND lt.ActBill=0 AND IOType = 0 and TransType in (302, 306, 303, 307, 312, 316, 313, 317) 
				GROUP BY lt.Item, lt.LocTo ,hu.ManufactureParty, lt.LotNo, lt.IsCs, lt.QualityType	
				UNION ALL '
				
		SET @SqlDetail9='SELECT lt.Item, lt.LocTo AS Location, CASE WHEN lt.ActBill<>0 THEN ab.Party ELSE pb.Party END AS ManufactureParty, 
				lt.LotNo, 1 AS IsCs, lt.QualityType, 
				SUM(-lt.Qty) AS Qty,0 AS TobeQty
				FROM INV_LocTrans_'+Cast(@i AS varchar(10))+' AS lt
				LEFT JOIN BIL_PlanBill AS pb ON lt.PlanBill=pb.Id
				LEFT JOIN BIL_ActBill AS ab ON lt.ActBill=ab.Id
				WHERE lt.EffDate > '''+Convert(varchar(19), @InvDate, 121)+'''  and lt.EffDate <= '''+Convert(varchar(19), @HistoryData, 121)+''' '+@ItemWhere+'
				AND (lt.IsCs=1 OR (lt.IsCs=0 AND lt.ActBill<>0)) AND IOType = 1 AND lt.TransType IN (301, 305, 304, 308, 311, 315, 314, 318) 
				GROUP BY lt.Item, lt.LocTo,CASE WHEN lt.ActBill<>0 THEN ab.Party ELSE pb.Party END, lt.LotNo, lt.IsCs, lt.QualityType
				UNION ALL '
				
		SET @SqlDetail10='SELECT lt.Item, lt.LocTo AS Location, hu.ManufactureParty AS ManufactureParty, lt.LotNo, lt.IsCs, lt.QualityType, 
				0 AS QTY,SUM(-lt.Qty) AS TobeQty
				FROM INV_LocTrans_'+Cast(@i AS varchar(10))+' AS lt
				LEFT JOIN INV_Hu AS hu ON lt.Huid=hu.Huid 
				WHERE lt.EffDate > '''+Convert(varchar(19), @InvDate, 121)+'''  and lt.EffDate <= '''+Convert(varchar(19), @HistoryData, 121)+''' '+@ItemWhere+'
				AND lt.IsCs=1 AND lt.ActBill=0 AND IOType = 0 and TransType in (302, 306, 303, 307, 312, 316, 313, 317) 
				GROUP BY lt.Item, lt.LocTo ,hu.ManufactureParty, lt.LotNo, lt.IsCs, lt.QualityType'	
										
		SET @SqlTail=') as A WHERE 1=1 '+@LocationWhere+' GROUP BY Item, Location,ManufactureParty, LotNo, IsCs'
		--PRINT @SqlHeader+@SqlDetail1+@SqlDetail2+@SqlDetail3+@SqlDetail4+@SqlDetail5+@SqlDetail6+@SqlDetail7+@SqlDetail8
		--	+@SqlDetail9+@SqlDetail10+@SqlTail	
		--exec(@sql)
		INSERT INTO	#tempTransSummaryNoGroup
		exec(@SqlHeader+@SqlDetail1+@SqlDetail2+@SqlDetail3+@SqlDetail4+@SqlDetail5+@SqlDetail6+@SqlDetail7+@SqlDetail8
			+@SqlDetail9+@SqlDetail10+@SqlTail)	
		
						
		--先以之前期末库存为基准生成历史库存

		SET @i=@i+1
	END

	INSERT INTO #tempTransSummary(Item, Location, ManufactureParty, LotNo, IsCs, QualifyQty, InspectQty, RejectQty,
	TobeQualifyQty, TobeInspectQty, TobeRejectQty)
	SELECT Item, Location, ManufactureParty, LotNo, IsCs,
		SUM(QualifyQty),SUM(InspectQty),SUM(RejectQty),SUM(TobeQualifyQty),SUM(TobeInspectQty),SUM(TobeRejectQty)
	FROM #tempTransSummaryNoGroup GROUP BY Item, Location, ManufactureParty, LotNo, IsCs
	TRUNCATE TABLE #tempTransSummaryNoGroup
			
	insert into #tempInvSummary 
	(Item, Location, ManufactureParty, LotNo, IsCs , QualifyQty, InspectQty, RejectQty,
	TobeQualifyQty, TobeInspectQty, TobeRejectQty)
	select std.Item, std.Location, std.ManufactureParty, std.LotNo, std.IsCs,
	std.QualifyQty + isnull(ts.QualifyQty, 0), 
	std.InspectQty + isnull(ts.InspectQty, 0), 
	std.RejectQty + isnull(ts.RejectQty, 0),
	std.TobeQualifyQty + isnull(ts.TobeQualifyQty, 0), 
	std.TobeInspectQty + isnull(ts.TobeInspectQty, 0), 
	std.TobeRejectQty + isnull(ts.TobeRejectQty, 0)
	from #tempStandarInv as std 
	left join #tempTransSummary as ts on std.Item = ts.Item and std.Location = ts.Location
	
	
	--对于之前期末库存不存在的记录，直接记录历史库存
	insert into #tempInvSummary 
	(Item, Location, ManufactureParty, LotNo, IsCs , QualifyQty, InspectQty, RejectQty,
	TobeQualifyQty, TobeInspectQty, TobeRejectQty)
	select ts.Item, ts.Location, ts.ManufactureParty, ts.LotNo, ts.IsCs,
	ts.QualifyQty, ts.InspectQty, ts.RejectQty,
	ts.TobeQualifyQty, ts.TobeInspectQty, ts.TobeRejectQty
	from #tempStandarInv as std 
	right join #tempTransSummary as ts on std.Item = ts.Item and std.Location = ts.Location
	where std.Item is null
	

	---最后的查询结果,包含2个数据集,一个是总数一个是分页数据
	IF @IsSummaryBySAPLoc=1
	BEGIN
		--汇总到SAP库位
		SET @SqlHeader = 'SELECT  row_number() over('+@SortDesc+'), * FROM (SELECT ts.Item, l.SAPLocation AS Location, ts.ManufactureParty, ts.LotNo,
			SUM(CASE WHEN ts.IsCS = 1 THEN ts.QualifyQty+ts.InspectQty+ts.RejectQty+ts.TobeQualifyQty+ts.TobeInspectQty+ts.TobeRejectQty ELSE 0 END) AS CSQty,
			SUM(ts.QualifyQty) AS QualifyQty,SUM(ts.InspectQty) AS InspectQty,SUM(ts.RejectQty) AS RejectQty,
			SUM(ts.TobeQualifyQty) AS TobeQualifyQty,SUM(ts.TobeInspectQty) AS TobeInspectQty,SUM(ts.TobeRejectQty) AS TobeRejectQty FROM #tempInvSummary ts
			INNER JOIN MD_Location l ON ts.Location=l.Code GROUP BY ts.Item, l.SAPLocation, ts.ManufactureParty, ts.LotNo) AS T
			WHERE CSQty<>0 OR QualifyQty<>0 OR InspectQty<>0 OR RejectQty<>0 OR TobeQualifyQty<>0
			OR TobeInspectQty<>0 OR TobeRejectQty<>0'
		--PRINT @SqlHeader
		INSERT INTO #TempResult 
		EXEC(@SqlHeader)			
		
		SELECT count(1) from #TempResult
		EXEC('SELECT TOP('+@PageSize+') Item, Location, ManufactureParty, LotNo, CSQty,
			QualifyQty,InspectQty,RejectQty,TobeQualifyQty,TobeInspectQty,TobeRejectQty FROM #TempResult '+@PagePara)
	END
	ELSE
	BEGIN
		IF @SummaryLevel=0
		BEGIN
			--不汇总
			SET @SqlHeader = 'SELECT  row_number() over('+@SortDesc+'), * FROM (SELECT ts.Item, ts.Location , ts.ManufactureParty, ts.LotNo,
			SUM(CASE WHEN ts.IsCS = 1 THEN ts.QualifyQty+ts.InspectQty+ts.RejectQty+ts.TobeQualifyQty+ts.TobeInspectQty+ts.TobeRejectQty ELSE 0 END) AS CSQty,
			SUM(ts.QualifyQty) AS QualifyQty,SUM(ts.InspectQty) AS InspectQty,SUM(ts.RejectQty) AS RejectQty,
			SUM(ts.TobeQualifyQty) AS TobeQualifyQty,SUM(ts.TobeInspectQty) AS TobeInspectQty,SUM(ts.TobeRejectQty) AS TobeRejectQty FROM #tempInvSummary ts
			GROUP BY ts.Item, ts.Location, ts.ManufactureParty, ts.LotNo) AS T
				WHERE CSQty<>0 OR QualifyQty<>0 OR InspectQty<>0 OR RejectQty<>0 OR TobeQualifyQty<>0
				OR TobeInspectQty<>0 OR TobeRejectQty<>0'
			INSERT INTO #TempResult 
			EXEC(@SqlHeader)			
			
			SELECT count(1) from #TempResult
			EXEC('SELECT TOP('+@PageSize+') Item, Location, ManufactureParty, LotNo, CSQty,
				QualifyQty,InspectQty,RejectQty,TobeQualifyQty,TobeInspectQty,TobeRejectQty FROM #TempResult '+@PagePara)
		END
		ELSE IF @SummaryLevel=1
		BEGIN
			--汇总到区域	
			SET @SqlHeader = 'SELECT  row_number() over('+@SortDesc+'), * FROM (SELECT ts.Item, l.Region AS Location, ts.ManufactureParty, ts.LotNo,
				SUM(CASE WHEN ts.IsCS = 1 THEN ts.QualifyQty+ts.InspectQty+ts.RejectQty+ts.TobeQualifyQty+ts.TobeInspectQty+ts.TobeRejectQty ELSE 0 END) AS CSQty,
				SUM(ts.QualifyQty) AS QualifyQty,SUM(ts.InspectQty) AS InspectQty,SUM(ts.RejectQty) AS RejectQty,
				SUM(ts.TobeQualifyQty) AS TobeQualifyQty,SUM(ts.TobeInspectQty) AS TobeInspectQty,SUM(ts.TobeRejectQty) AS TobeRejectQty FROM #tempInvSummary ts
				INNER JOIN MD_Location l ON ts.Location=l.Code GROUP BY ts.Item, l.Region, ts.ManufactureParty, ts.LotNo) AS T
				WHERE CSQty<>0 OR QualifyQty<>0 OR InspectQty<>0 OR RejectQty<>0 OR TobeQualifyQty<>0
				OR TobeInspectQty<>0 OR TobeRejectQty<>0'
			--PRINT @SqlHeader
			INSERT INTO #TempResult 
			EXEC(@SqlHeader)			
			
			SELECT count(1) from #TempResult
			EXEC('SELECT TOP('+@PageSize+') Item, Location, ManufactureParty, LotNo, CSQty,
				QualifyQty,InspectQty,RejectQty,TobeQualifyQty,TobeInspectQty,TobeRejectQty FROM #TempResult '+@PagePara)
		END
		ELSE IF @SummaryLevel=2
		BEGIN
			--汇总到车间
			SET @SqlHeader = 'SELECT  row_number() over('+@SortDesc+'), * FROM (SELECT ts.Item, r.Workshop AS Location, ts.ManufactureParty, ts.LotNo,
				SUM(CASE WHEN ts.IsCS = 1 THEN ts.QualifyQty+ts.InspectQty+ts.RejectQty+ts.TobeQualifyQty+ts.TobeInspectQty+ts.TobeRejectQty ELSE 0 END) AS CSQty,
				SUM(ts.QualifyQty) AS QualifyQty,SUM(ts.InspectQty) AS InspectQty,SUM(ts.RejectQty) AS RejectQty,
				SUM(ts.TobeQualifyQty) AS TobeQualifyQty,SUM(ts.TobeInspectQty) AS TobeInspectQty,SUM(ts.TobeRejectQty) AS TobeRejectQty FROM #tempInvSummary ts
				INNER JOIN MD_Location l ON ts.Location=l.Code 
				INNER JOIN MD_Region r ON r.Code=l.Region GROUP BY ts.Item, r.Workshop, ts.ManufactureParty, ts.LotNo) AS T
				WHERE CSQty<>0 OR QualifyQty<>0 OR InspectQty<>0 OR RejectQty<>0 OR TobeQualifyQty<>0
				OR TobeInspectQty<>0 OR TobeRejectQty<>0'
			--PRINT @SqlHeader
			INSERT INTO #TempResult 
			EXEC(@SqlHeader)			
			
			SELECT count(1) from #TempResult
			EXEC('SELECT TOP('+@PageSize+') Item, Location, ManufactureParty, LotNo, CSQty,
				QualifyQty,InspectQty,RejectQty,TobeQualifyQty,TobeInspectQty,TobeRejectQty FROM #TempResult '+@PagePara)
		END
		ELSE IF @SummaryLevel=3
		BEGIN
			--汇总到工厂
			SET @SqlHeader = 'SELECT  row_number() over('+@SortDesc+'), * FROM (SELECT ts.Item, r.Plant AS Location, ts.ManufactureParty, ts.LotNo,
				SUM(CASE WHEN ts.IsCS = 1 THEN ts.QualifyQty+ts.InspectQty+ts.RejectQty+ts.TobeQualifyQty+ts.TobeInspectQty+ts.TobeRejectQty ELSE 0 END) AS CSQty,
				SUM(ts.QualifyQty) AS QualifyQty,SUM(ts.InspectQty) AS InspectQty,SUM(ts.RejectQty) AS RejectQty,
				SUM(ts.TobeQualifyQty) AS TobeQualifyQty,SUM(ts.TobeInspectQty) AS TobeInspectQty,SUM(ts.TobeRejectQty) AS TobeRejectQty FROM #tempInvSummary ts
				INNER JOIN MD_Location l ON ts.Location=l.Code 
				INNER JOIN MD_Region r ON r.Code=l.Region GROUP BY ts.Item, r.Plant, ts.ManufactureParty, ts.LotNo) AS T
				WHERE CSQty<>0 OR QualifyQty<>0 OR InspectQty<>0 OR RejectQty<>0 OR TobeQualifyQty<>0
				OR TobeInspectQty<>0 OR TobeRejectQty<>0'
			--PRINT @SqlHeader
			INSERT INTO #TempResult 
			EXEC(@SqlHeader)			
			
			SELECT count(1) from #TempResult
			EXEC('SELECT TOP('+@PageSize+') Item, Location, ManufactureParty, LotNo, CSQty,
				QualifyQty,InspectQty,RejectQty,TobeQualifyQty,TobeInspectQty,TobeRejectQty FROM #TempResult '+@PagePara) 	
		END
	END		
		
			
	--SELECT * FROM #tempStandarInv
	--SELECT COUNT(1) FROM #tempInvSummary
	--SELECT Item, Location, ManufactureParty, LotNo,CASE WHEN IsCS = 1 THEN QualifyQty+InspectQty+RejectQty+TobeQualifyQty+TobeInspectQty+TobeRejectQty ELSE 0 END AS CSQty,
	--QualifyQty,InspectQty,RejectQty,TobeQualifyQty,TobeInspectQty,TobeRejectQty FROM #tempInvSummary
	
	--清除临时表数据
	DROP TABLE #TempResult
	DROP TABLE #tempInvSummary
	DROP TABLE #tempStandarInv
	DROP TABLE #tempTransSummary	
END