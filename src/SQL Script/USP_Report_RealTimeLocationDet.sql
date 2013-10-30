IF EXISTS(SELECT * FROM SYS.OBJECTS WHERE TYPE='P' AND name='USP_Report_RealTimeLocationDet')
	DROP PROCEDURE USP_Report_RealTimeLocationDet	
GO
CREATE PROCEDURE [dbo].[USP_Report_RealTimeLocationDet]
(
	@Locations varchar(8000),
	@Items varchar(8000),
	@SortDesc varchar(100),
	@PageSize int,
	@Page int,
	@IsSummaryBySAPLoc bit,
	@SummaryLevel int,
	@IsGroupByManufactureParty bit,
	@IsGroupByLotNo bit
)
AS
BEGIN
/*
exec USP_Report_RealTimeLocationDet '','','',100,1,0,2,0,0
500368184
*/
	SET NOCOUNT ON
	DECLARE @Sql varchar(max)
	DECLARE @Where  varchar(8000)
	DECLARE @PartSuffix varchar(5)
	DECLARE @PagePara varchar(8000)
	DECLARE @TmpForLoop int
	SELECT @Sql='',@TmpForLoop=0,@Where=''
	DECLARE @LocationIds table(Id int identity(1,1),PartSuffix varchar(5))
	CREATE TABLE #TempResult
	(
		rowid int,
		Location varchar(50), 
		Item varchar(50), 
		LotNo varchar(50),
		ManufactureParty varchar(50), 
		Qty decimal(18,8), 
		CsQty decimal(18,8), 
		QualifyQty decimal(18,8), 
		InspectQty decimal(18,8), 
		RejectQty decimal(18,8),  
		ATPQty decimal(18,8), 
		FreezeQty decimal(18,8)
	)
	CREATE TABLE #TempInternal
	(
		Location varchar(50), 
		Item varchar(50), 
		LotNo varchar(50),
		ManufactureParty varchar(50), 
		Qty decimal(18,8), 
		CsQty decimal(18,8), 
		QualifyQty decimal(18,8), 
		InspectQty decimal(18,8), 
		RejectQty decimal(18,8),  
		ATPQty decimal(18,8), 
		FreezeQty decimal(18,8)
	)	
	--如果有输入的库位则只查询输入库位的表，否则全部表拼接查询
	IF ISNULL(@Locations,'')='' 
	BEGIN
		INSERT INTO @LocationIds(PartSuffix)
		SELECT DISTINCT(PartSuffix) FROM MD_Location WHERE PartSuffix IS NOT NULL OR PartSuffix<>''
	END
	ELSE
	BEGIN
		SET @Where=' WHERE det.Location in('''+replace(@Locations,',',''',''')+''')'
	    SET @sql='SELECT DISTINCT PartSuffix FROM MD_Location WHERE Code in ('''+replace(@Locations,',',''',''')+''')'
		--PRINT @sql
		INSERT INTO @LocationIds(PartSuffix)
		EXEC(@sql)
	END
	
	---查询出数据时需要的条件
	-----物料
	IF ISNULL(@Items,'')<>'' 
	BEGIN
		IF ISNULL(@Where,'')=''
		BEGIN
			SET @Where=' WHERE det.Item IN ('''+replace(@Items,',',''',''')+''')'
		END
		ELSE
		BEGIN
			SET @Where=@Where+' AND det.Item IN ('''+replace(@Items,',',''',''')+''')'
		END
	END
	--PRINT @Where
	--select * from @LocationIds
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

	DECLARE @MaxId int
	SELECT @MaxId = MAX(Id),@Sql='' FROM @LocationIds
	WHILE @TmpForLoop<@MaxId
	BEGIN
		SET @TmpForLoop=@TmpForLoop+1
		SELECT @PartSuffix=PartSuffix FROM @LocationIds WHERE Id=@TmpForLoop
		--PRINT @TmpForLoop
		IF 	@IsGroupByManufactureParty=0 AND @IsGroupByLotNo=0
		BEGIN
			SET @Sql='SELECT det.Location, det.Item,'''' as LotNo,'''' as ManufactureParty,
					SUM(det.Qty) AS Qty, 
					SUM(CASE WHEN det.IsCS = 1 THEN det.Qty ELSE 0 END) AS CSQty, 					
                    SUM(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, 
                    SUM(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                    SUM(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, 
                    SUM(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                    SUM(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
					FROM  inv_locationlotdet_'+@PartSuffix+' AS det '+@Where+' 
					GROUP BY det.Location, det.Item'
		END	
		ELSE IF @IsGroupByManufactureParty=1 AND @IsGroupByLotNo=0
		BEGIN
			SET @Sql=@Sql+'SELECT det.Location, det.Item,'''' AS LotNo, 
					CASE WHEN bill.Party IS NOT NULL THEN bill.Party ELSE hu.ManufactureParty END AS ManufactureParty,
					SUM(det.Qty) AS Qty,
					SUM(CASE WHEN det.IsCS = 1 THEN det.Qty ELSE 0 END) AS CSQty,
                    SUM(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, 
                    SUM(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                    SUM(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, 
                    SUM(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                    SUM(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
					FROM  inv_locationlotdet_'+@PartSuffix+' AS det 
					LEFT JOIN dbo.INV_Hu AS hu ON det.HuId = hu.HuId  
					LEFT  JOIN BIL_PlanBill AS bill ON det.PlanBill=bill.id AND bill.Type=0 '+@Where+' 
					GROUP BY det.Location, det.Item,CASE WHEN bill.Party IS NOT NULL THEN bill.Party ELSE hu.ManufactureParty END, det.IsCs '			
		END	
		ELSE IF @IsGroupByManufactureParty=0 AND @IsGroupByLotNo=1
		BEGIN
			SET @Sql=@Sql+'SELECT det.Location, det.Item, det.LotNo,'''' as ManufactureParty, 
					SUM(det.Qty) AS Qty, 
					SUM(CASE WHEN det.IsCS = 1 THEN det.Qty ELSE 0 END) AS CSQty,
                    SUM(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                    SUM(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                    SUM(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
					FROM  inv_locationlotdet_'+@PartSuffix+' AS det LEFT JOIN
					dbo.INV_Hu AS hu ON det.HuId = hu.HuId '+@Where+' 
					GROUP BY det.Location, det.Item, det.LotNo, det.IsCs'		
		END			
		ELSE IF @IsGroupByManufactureParty=1 AND @IsGroupByLotNo=1	
		BEGIN
			SET @Sql=@Sql+'SELECT det.Location, det.Item,det.LotNo, 
					CASE WHEN bill.Party IS NOT NULL THEN bill.Party ELSE hu.ManufactureParty END AS ManufactureParty,
					SUM(det.Qty) AS Qty, 
					SUM(CASE WHEN det.IsCS = 1 THEN det.Qty ELSE 0 END) AS CSQty,
                    SUM(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                    SUM(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                    SUM(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
					FROM  inv_locationlotdet_'+@PartSuffix+' AS det 
					LEFT JOIN dbo.INV_Hu AS hu ON det.HuId = hu.HuId  
					LEFT  JOIN BIL_PlanBill AS bill ON det.PlanBill=bill.id AND bill.Type=0 '+@Where+' 
					GROUP BY det.Location, det.Item,det.LotNo,CASE WHEN bill.Party IS NOT NULL THEN bill.Party ELSE hu.ManufactureParty END, det.IsCs '
		END	
		--PRINT @Sql
		INSERT INTO #TempInternal
		EXEC(@Sql)		
	END
	---SELECT * FROM #TempInternal
	
	---最后的查询结果,包含2个数据集,一个是总数一个是分页数据
	IF @IsSummaryBySAPLoc=1
	BEGIN
		--汇总到SAP库位
		SET @sql = 'select row_number() over('+@SortDesc+') as rowid,* FROM (SELECT l.SAPLocation as Location, t.Item, t.LotNo, t.ManufactureParty,
			SUM(t.CSQty) AS CSQty,SUM(t.Qty) as QTY, SUM(t.QualifyQty) as QualifyQty, SUM(t.InspectQty) as InspectQty, 
			SUM(t.RejectQty) as RejectQty, SUM(t.ATPQty) as ATPQty, SUM(t.FreezeQty) as FreezeQty FROM #TempInternal as t
			INNER JOIN MD_Location l ON t.Location=l.Code
			GROUP BY l.SAPLocation, t.Item, t.LotNo, t.ManufactureParty) as LocTranDet'
		
		insert into #TempResult 
		exec(@sql)	
			
		select count(1) from #TempResult
		exec('select top('+@PageSize+')  Location, Item, LotNo, ManufactureParty, Qty, CSQTY, QualifyQty, InspectQty, RejectQty, ATPQty, FreezeQty from #TempResult '+@PagePara) 		
	END
	ELSE
	BEGIN
		IF @SummaryLevel=0
		BEGIN
			--不汇总
			SET @sql = 'select row_number() over('+@SortDesc+') as rowid,Location, Item, LotNo, ManufactureParty,
			Qty, CSQTY, QualifyQty, InspectQty, RejectQty, ATPQty, FreezeQty from #TempInternal as det'
			
			insert into #TempResult 
			exec(@sql)	
			
			PRINT @sql	
			select count(1) from #TempResult
			exec('select top('+@PageSize+')  Location, Item, LotNo, ManufactureParty, Qty, CSQTY, QualifyQty, InspectQty, RejectQty, ATPQty, FreezeQty from #TempResult '+@PagePara) 
		END
		ELSE IF @SummaryLevel=1
		BEGIN
			--汇总到区域
			SET @sql = 'select row_number() over('+@SortDesc+') as rowid,* FROM (SELECT r.Code as Location, t.Item, t.LotNo, t.ManufactureParty,
				SUM(t.CSQty) AS CSQty,SUM(t.Qty) as QTY, SUM(t.QualifyQty) as QualifyQty, SUM(t.InspectQty) as InspectQty, 
				SUM(t.RejectQty) as RejectQty, SUM(t.ATPQty) as ATPQty, SUM(t.FreezeQty) as FreezeQty FROM #TempInternal as t
				INNER JOIN MD_Location l ON t.Location=l.Code
				INNER JOIN MD_Region r ON r.Code=l.Region
				GROUP BY r.Code, t.Item, t.LotNo, t.ManufactureParty) as LocTranDet'
			
			insert into #TempResult 
			exec(@sql)	
				
			select count(1) from #TempResult
			exec('select top('+@PageSize+')  Location, Item, LotNo, ManufactureParty, Qty, CSQTY, QualifyQty, InspectQty, RejectQty, ATPQty, FreezeQty from #TempResult '+@PagePara) 
		END
		ELSE IF @SummaryLevel=2
		BEGIN
			--汇总到车间
			SET @sql = 'select row_number() over('+@SortDesc+') as rowid,* FROM (SELECT r.Workshop as Location, t.Item, t.LotNo, t.ManufactureParty, 
				SUM(t.Qty) as QTY, SUM(t.CSQty) AS CSQty, SUM(t.QualifyQty) as QualifyQty, SUM(t.InspectQty) as InspectQty, 
				SUM(t.RejectQty) as RejectQty, SUM(t.ATPQty) as ATPQty, SUM(t.FreezeQty) as FreezeQty FROM #TempInternal as t
				INNER JOIN MD_Location l ON t.Location=l.Code
				INNER JOIN MD_Region r ON r.Code=l.Region
				GROUP BY r.Workshop, t.Item, t.LotNo, t.ManufactureParty) as LocTranDet'

			insert into #TempResult 
			exec(@sql)	
				
			select count(1) from #TempResult
			exec('select top('+@PageSize+') Location, Item, LotNo, ManufactureParty, Qty, CSQTY, QualifyQty, InspectQty, RejectQty, ATPQty, FreezeQty from #TempResult '+@PagePara) 
		END
		ELSE IF @SummaryLevel=3
		BEGIN
			--汇总到工厂
			SET @sql = 'select row_number() over('+@SortDesc+') as rowid,* FROM (SELECT r.Plant as Location, t.Item, t.LotNo, t.ManufactureParty, 
				SUM(t.Qty) as QTY, SUM(t.CSQty) AS CSQty, SUM(t.QualifyQty) as QualifyQty, SUM(t.InspectQty) as InspectQty, 
				SUM(t.RejectQty) as RejectQty, SUM(t.ATPQty) as ATPQty, SUM(t.FreezeQty) as FreezeQty FROM #TempInternal as t
				INNER JOIN MD_Location l ON t.Location=l.Code
				INNER JOIN MD_Region r ON r.Code=l.Region
				GROUP BY r.Plant, t.Item, t.LotNo, t.ManufactureParty) as LocTranDet'
			
			insert into #TempResult 
			exec(@sql)	
				
			select count(1) from #TempResult
			exec('select top('+@PageSize+') Location, Item, LotNo, ManufactureParty, Qty, CSQTY, QualifyQty, InspectQty, RejectQty, ATPQty, FreezeQty from #TempResult '+@PagePara) 
		END
	END	
END


GO


