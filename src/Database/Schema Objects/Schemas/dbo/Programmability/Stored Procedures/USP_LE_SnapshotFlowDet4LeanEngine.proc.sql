CREATE PROCEDURE [dbo].[USP_LE_SnapshotFlowDet4LeanEngine] --WITH ENCRYPTION
AS
BEGIN
	SET NOCOUNT ON
	declare @DateTimeNow datetime = GetDate()
	declare @Msg nvarchar(Max)
	declare @trancount int
	
	--记录日志
	set @Msg = N'获取路线明细开始'
	insert into LOG_RunLeanEngine(Lvl, Msg) values(0, @Msg)

	CREATE TABLE #tempFlowDet (
		Id int identity(1, 1),
		Flow varchar(50) NULL,
		FlowDetId int NULL,
		Item varchar(50) NOT NULL,
		Uom varchar(5) NULL,
		UC decimal(18, 8) NULL,
		ManufactureParty varchar(50) NULL,
		LocFrom varchar(50) NULL,
		LocTo varchar(50) NOT NULL,
		ExtraDmdSource varchar(256) NULL,
		MrpTotal decimal(18, 8) NULL,
		MrpTotalAdj decimal(18, 8) NULL,
		MrpWeight decimal(18, 8) NULL,
		IsRefFlow bit NULL,
		SafeStock decimal(18, 8),
		MaxStock decimal(18, 8),
		MinLotSize decimal(18, 8),
		RoundUpOpt tinyint,
		Strategy tinyint
	)
	
	CREATE NONCLUSTERED INDEX IX_TempFlowDet  ON #tempFlowDet 
	(
		Item asc,
		LocTo asc
	)
	
	create table #tempMultiSupplierGroup
	(
		RowId int identity(1, 1),
		Item varchar(50),
		LocTo varchar(50),
	)
	
	create table #tempMultiSupplierItem
	(
		RowId int identity(1, 1),
		FlowDetRowId int,
		Flow varchar(50),
		Item varchar(50), 
		LocTo varchar(50),
		MSGRowId int,
		MrpTotal decimal(18, 8),
		MrpTotalAdj decimal(18, 8),
		MrpWeight int,
		TotalQty decimal(18, 8),
		TotalWeight decimal(18, 8)
	)
	
	CREATE NONCLUSTERED INDEX IX_TempMultiSupplierItem  ON #tempMultiSupplierItem 
	(
		Item asc,
		LocTo asc
	)
	
	create table #tempSortedMultiSupplierItem
	(
		GID int, 
		FlowDetRowId int,
		Flow varchar(50)
	)
	
	create table #tempOrderBom
	(
		Item varchar(50),
		Region varchar(50),
		Location varchar(50),
		IsMatch bit,
	)
	
	create table #tempPurchase
	(
		Item varchar(50),
		Region varchar(50),
		Location varchar(50)
	)
	
	create table #tempTransfer
	(
		Item varchar(50),
		PartyFrom varchar(50),
		PartyTo varchar(50),
		LocFrom varchar(50),
		LocTo varchar(50)
	)
	
	Create table #tempExtraDmdSource
	(
		RowId int identity(1, 1),
		FlowDetId int,
		ExtraDmdSource varchar(256)
	)
	
	--获取路线
	truncate table LE_FlowMstrSnapShot
	insert into LE_FlowMstrSnapShot(Flow, [Type], Strategy, PartyFrom, PartyTo, LocFrom, LocTo, Dock, ExtraDmdSource)
	select mstr.Code as Flow, mstr.[Type], stra.Strategy, mstr.PartyFrom, mstr.PartyTo, mstr.LocFrom, mstr.LocTo, mstr.Dock, mstr.ExtraDmdSource
	from SCM_FlowMstr as mstr 
	inner join SCM_FlowStrategy as stra on mstr.Code = stra.Flow
	where mstr.IsActive = 1 and mstr.IsAutoCreate = 1 and mstr.[Type] in (1, 2, 5, 6, 7, 8) and stra.Strategy in (2, 3)
	
	-------------------↓计算每条路线的需求时间段和发单时间-----------------------
	declare @FlowRowId int
	declare @MaxFlowRowId int
	
	select @FlowRowId = MIN(Id), @MaxFlowRowId = MAX(Id) from LE_FlowMstrSnapShot
	while (@FlowRowId <= @MaxFlowRowId)
	begin
		declare @Flow varchar(50) = null
		declare @FlowType tinyint = null
		declare @FlowStrategy tinyint = null
		declare @PartyFrom varchar(50) = null
		declare @PartyTo varchar(50) = null
		declare @LocFrom varchar(50) = null
		declare @LocTo varchar(50) = null
		declare @ExtraDmdSource varchar(50) = null
		declare @WinTimeType tinyint = null
		declare @WinTimeDiff decimal(18, 8) = null  --秒
		declare @LeadTime decimal(18, 8) = null  --秒
		declare @EMLeadTime decimal(18, 8) = null  --秒
		declare @WindowTime datetime = null
		declare @WindowTime2 datetime = null
		
		
		declare @EMWindowTime datetime = null
		declare @ReqTimeFrom datetime = null
		declare @ReqTimeTo datetime = null
		declare @OrderTime datetime = null
		
		select @Flow = Flow, @FlowType = [Type], 
		@PartyFrom = PartyFrom, @PartyTo = PartyTo,
		@LocFrom = LocFrom, @LocTo = LocTo, @ExtraDmdSource = ExtraDmdSource
		from LE_FlowMstrSnapShot where Id = @FlowRowId
		
		--记录日志
		set @Msg = N'计算路线需求时间段和发单时间开始'
		insert into LOG_RunLeanEngine(Flow, Lvl, Msg) values(@Flow, 0, @Msg)
		
		set @trancount = @@trancount
		begin try
			if @trancount = 0
			begin
				begin tran
			end		
		
			select @FlowStrategy = Strategy, @WinTimeType = WinTimeType, 
			@WinTimeDiff = WinTimeDiff * 60 * 60, --小时转为秒
			@LeadTime = -LeadTime * 60 * 60, --小时转为秒
			@EMLeadTime = EMLeadTime * 60 * 60,	   --小时转为秒
			@WindowTime = NextWinTime
			from SCM_FlowStrategy with(UPDLOCK) where Flow = @Flow

			if (@WindowTime is null or @WindowTime < @DateTimeNow)
			begin  --没有设置下次窗口时间或者下次窗口开始时间小于当前时间重新查找下次窗口开始时间
				if @WindowTime is null
				begin
					set @Msg = N'窗口时间没有设置，重新计算窗口时间'
					insert into LOG_RunLeanEngine(Flow, Lvl, ErrorId, Msg) values(@Flow, 1, 10, @Msg)
				end
				else
				begin
					set @Msg = N'窗口时间' + CONVERT(varchar, @WindowTime, 120) + N'小于当前时间，重新计算窗口时间'
					insert into LOG_RunLeanEngine(Flow, Lvl, ErrorId, Msg) values(@Flow, 1, 11, @Msg)
				end
				
				exec USP_Busi_GetNextWindowTime @Flow, @WindowTime, @WindowTime output
				update SCM_FlowStrategy set NextWinTime = @WindowTime where Flow = @Flow
			end
			
			--计算本次窗口结束时间
			exec USP_Busi_GetNextWindowTime @Flow, @WindowTime, @WindowTime2 output
			
			--计算紧急窗口时间
			exec USP_Busi_AddWorkingDate @DateTimeNow, @EMLeadTime, null, @PartyTo, @EMWindowTime output
			if @EMWindowTime > @WindowTime
			begin
				set @EMWindowTime = @WindowTime
			end
		
			--计算发单时间和紧急发单时间
			exec USP_Busi_SubtractWorkingDate @WindowTime, @LeadTime, null, @PartyTo, @OrderTime output
			
			--计算下次需求开始时间，根据进料提前期计算实际需求开始时间
			exec USP_Busi_AddWorkingDate @WindowTime, @WinTimeDiff, null, @PartyTo, @ReqTimeFrom output
		
			--计算下次需求结束时间，根据进料提前期计算实际需求开始时间
			exec USP_Busi_AddWorkingDate @WindowTime2, @WinTimeDiff, null, @PartyTo, @ReqTimeTo output
			
			--记录日志
			set @Msg = N'路线需求时间为' + CONVERT(varchar, @ReqTimeFrom, 120) + N'~' + CONVERT(varchar, @ReqTimeTo, 120)
				+ '，发单时间为' + CONVERT(varchar, @OrderTime, 120)
				+ '，窗口时间为' + CONVERT(varchar, @WindowTime, 120)
				+ '，紧急窗口时间为' + CONVERT(varchar, @EMWindowTime, 120)
			insert into LOG_RunLeanEngine(Flow, Lvl, Msg) values(@Flow, 0, @Msg)

			--更新路线
			update LE_FlowMstrSnapShot set OrderTime = @OrderTime, ReqTimeFrom = @ReqTimeFrom, ReqTimeTo = @ReqTimeTo, 
			WindowTime = @WindowTime, EMWindowTime = @EMWindowTime
			where Id = @FlowRowId
			
			--获取路线明细
			insert into #tempFlowDet(Flow, FlowDetId, Item, Uom, UC, ManufactureParty,
			LocFrom, LocTo,
			ExtraDmdSource,
			MrpTotal, MrpTotalAdj, MrpWeight, IsRefFlow,
			SafeStock, MaxStock, MinLotSize, RoundUpOpt, Strategy)
			select @Flow, Id as FlowDetId, Item, Uom, UC, ManufactureParty, 
			ISNULL(LocFrom, @LocFrom) as LocFrom, ISNULL(LocTo, @LocTo) as LocTo,
			ISNULL(ExtraDmdSource, @ExtraDmdSource) as ExtraDmdSource,
			ISNULL(MrpTotal, 0), ISNULL(MrpTotalAdj, 0), ISNULL(MrpWeight, 0) as MrpWeight, 0,
			ISNULL(SafeStock, 0), ISNULL(MaxStock, 0), ISNULL(MinLotSize, 0), RoundUpOpt, @FlowStrategy
			from SCM_FlowDet
			where Flow = @Flow and (StartDate is null or (StartDate >= @DateTimeNow))
			and (EndDate is null or (EndDate <= @DateTimeNow))
			and IsAutoCreate = 1
			
			if @OrderTime <= @DateTimeNow
			begin
				--记录日志
				set @Msg = N'路线发单时间' + CONVERT(varchar, @OrderTime, 120) + '小于当前时间更新路线下次窗口时间为' + CONVERT(varchar, @WindowTime2, 120)
				insert into LOG_RunLeanEngine(Flow, Lvl, Msg) values(@Flow, 0, @Msg)
				
				--更新下次窗口时间
				update SCM_FlowStrategy set NextWinTime = @WindowTime2 where Flow = @Flow
			end
			
			if @trancount = 0 
			begin  
				commit
			end
		end try
		begin catch
			if @trancount = 0
			begin
				rollback
			end 
		
			--记录日志
			set @Msg = N'计算路线需求时间段和发单时间出现异常，异常信息：' + Error_Message()
			insert into LOG_RunLeanEngine(Flow, Lvl, ErrorId, Msg) values(@Flow, 2, 12, @Msg)
		end catch
		
		--记录日志
		set @Msg = N'计算路线需求时间段和发单时间结束'
		insert into LOG_RunLeanEngine(Flow, Lvl, Msg) values(@Flow, 0, @Msg)
		
		set @FlowRowId = @FlowRowId + 1
	end
	-------------------↑计算每条路线的需求时间段和发单时间-----------------------
	
	
	
	-------------------↓计算引用路线需求时间段和发单时间-----------------------
	--记录日志
	set @Msg = N'计算引用路线需求时间段和发单时间开始'
	insert into LOG_RunLeanEngine(Lvl, Msg) values(0, @Msg)
	
	insert into #tempFlowDet(Flow, FlowDetId, Item, UOM, UC, ManufactureParty,
	LocFrom, LocTo,
	ExtraDmdSource,
	IsRefFlow, MrpTotal, MrpTotalAdj, MrpWeight,
	SafeStock, MaxStock, MinLotSize, RoundUpOpt, Strategy)
	select mstr.Code as Flow, fdet.FlowDetId, fdet.Item, fdet.UOM, fdet.UC, fdet.ManufactureParty, 
	mstr.LocFrom, mstr.LocTo,
	mstr.ExtraDmdSource,
	1, 0, 0, 0,
	ISNULL(fdet.SafeStock, 0), ISNULL(fdet.MaxStock, 0), ISNULL(fdet.MinLotSize, 0), fdet.RoundUpOpt, stra.Strategy
	from SCM_FlowMstr as mstr 
	inner join SCM_FlowStrategy as stra on mstr.Code = stra.Flow
	inner join LE_FlowMstrSnapShot as fmstr on mstr.Code = fmstr.Flow
	inner join #tempFlowDet as fdet on mstr.RefFlow = fdet.Flow
	where mstr.IsActive = 1 and mstr.IsAutoCreate = 1
	and stra.Strategy in (2, 3)
	
	--记录日志
	set @Msg = N'计算引用路线需求时间段和发单时间结束'
	insert into LOG_RunLeanEngine(Lvl, Msg) values(0, @Msg)
	-------------------↑计算引用路线的需求时间段和发单时间-----------------------
	
	
	
	-------------------↓整车物料的消耗库位和采购入库地点匹配，添加缺失的路线-----------------------
	--记录日志
	set @Msg = N'整车物料消耗库位和采购入库地点匹配，添加缺失的移库路线开始'
	insert into LOG_RunLeanEngine(Lvl, Msg) values(0, @Msg)
	
	--汇总整车物料需求（物料+区域+库位）
	insert into #tempOrderBom(Item, Location, IsMatch)
	select Item, Location, 0 from LE_OrderBomCPTimeSnapshot group by Item, Location
	update bom set Region = loc.Region
	from #tempOrderBom as bom inner join MD_Location as loc on bom.Location = loc.Code
	
	--汇总采购入库地点（物料+区域+库位）
	insert into #tempPurchase(Item, Region, Location)
	select det.Item, mstr.PartyTo as Region, det.LocTo as Location 
	from SCM_FlowDet as det inner join SCM_FlowMstr as mstr 
	on det.Flow = mstr.Code
	where mstr.[Type] in (1, 5, 6, 8) and mstr.IsActive = 1
	
	--汇总移库路线
	insert into #tempTransfer(Item, PartyFrom, PartyTo, LocFrom, LocTo)
	select det.Item, mstr.PartyFrom, mstr.PartyTo, det.LocFrom, det.LocTo 
	from SCM_FlowDet as det inner join SCM_FlowMstr as mstr 
	on det.Flow = mstr.Code
	where mstr.[Type] in (2, 7) and mstr.IsActive = 1
	
	--第一次匹配，消耗库位和采购入库地点相同
	update bom set IsMatch = 1
	from #tempOrderBom as bom
	inner join #tempPurchase as pur on bom.Item = pur.Item and bom.Location = pur.Location
	
	--第二次匹配，消耗库位和采购入库地点通过移库路线关联
	if exists (select top 1 * from #tempOrderBom where IsMatch = 0)
	begin
		update bom set IsMatch = 1
		from #tempOrderBom as bom
		inner join #tempTransfer as tra on bom.Item = tra.Item and bom.Location = tra.LocTo
		inner join #tempPurchase as pur on tra.Item = pur.Item and tra.LocFrom = pur.Location
		where bom.IsMatch = 0
	end
	
	-------------------↓第一次添加零件从采购入库地点到整车物料消耗库位的移库路线,区域相同-----------------------
	if exists (select top 1 * from #tempOrderBom where IsMatch = 0)
	begin
		--记录日志
		insert into LOG_RunLeanEngine(Lvl, ErrorId, Item, LocFrom, LocTo, Msg)
		select 1, 13, bom.Item, pur.Location, bom.Location, N'添加零件' + bom.Item + N'从采购入库地点' + pur.Location + N'到整车物料消耗库位' + bom.Location + N'的移库路线'
		from #tempOrderBom as bom
		inner join #tempPurchase as pur on bom.Item = pur.Item and bom.Region = pur.Region
		inner join LE_FlowMstrSnapShot as mstr on mstr.PartyFrom = pur.Region and mstr.PartyTo = bom.Region
		where bom.IsMatch = 0
		
		insert into #tempFlowDet(Flow, FlowDetId, Item, UOM, UC, ManufactureParty,
		LocFrom, LocTo,
		ExtraDmdSource,
		IsRefFlow, MrpTotal, MrpTotalAdj, MrpWeight,
		SafeStock, MaxStock, MinLotSize, RoundUpOpt, Strategy)
		select mstr.Flow, 0, bom.Item, item.UOM, item.UC, null,  
		pur.Location, bom.Location,
		null,
		0, 0, 0, 0,
		0, 0, 0, 1, 3
		from #tempOrderBom as bom
		inner join #tempPurchase as pur on bom.Item = pur.Item and bom.Region = pur.Region
		inner join MD_Item as item on bom.Item = item.Code
		inner join LE_FlowMstrSnapShot as mstr on mstr.PartyFrom = pur.Region and mstr.PartyTo = bom.Region
		where bom.IsMatch = 0
		
		update bom set IsMatch = 1
		from #tempOrderBom as bom
		inner join #tempPurchase as pur on bom.Item = pur.Item and bom.Region = pur.Region
		inner join LE_FlowMstrSnapShot as mstr on mstr.PartyFrom = pur.Region and mstr.PartyTo = bom.Region
		where bom.IsMatch = 0
	end
	-------------------↑第一次添加零件从采购入库地点到整车物料消耗库位的移库路线,区域相同-----------------------
	
	-------------------↓第二次添加零件从采购入库地点到整车物料消耗库位的移库路线，区域不相同-----------------------
	if exists (select top 1 * from #tempOrderBom where IsMatch = 0)
	begin
		--记录日志
		insert into LOG_RunLeanEngine(Lvl, ErrorId, Item, LocFrom, LocTo, Msg)
		select 1, 14, bom.Item, pur.Location, bom.Location, N'添加零件' + bom.Item + N'从采购入库地点' + pur.Location + N'到整车物料消耗库位' + bom.Location + N'的移库路线'
		from #tempOrderBom as bom
		inner join #tempPurchase as pur on bom.Item = pur.Item
		inner join LE_FlowMstrSnapShot as mstr on mstr.PartyFrom = pur.Region and mstr.PartyTo = bom.Region
		where bom.IsMatch = 0
		
		insert into #tempFlowDet(Flow, FlowDetId, Item, UOM, UC, ManufactureParty,
		LocFrom, LocTo,
		ExtraDmdSource,
		IsRefFlow, MrpTotal, MrpTotalAdj, MrpWeight,
		SafeStock, MaxStock, MinLotSize, RoundUpOpt, Strategy)
		select mstr.Flow, 0, bom.Item, item.UOM, item.UC, null,  
		pur.Location, bom.Location,
		null,
		0, 0, 0, 0,
		0, 0, 0, 1, 3
		from #tempOrderBom as bom
		inner join #tempPurchase as pur on bom.Item = pur.Item
		inner join MD_Item as item on bom.Item = item.Code
		inner join LE_FlowMstrSnapShot as mstr on mstr.PartyFrom = pur.Region and mstr.PartyTo = bom.Region
		where bom.IsMatch = 0
		
		update bom set IsMatch = 1
		from #tempOrderBom as bom
		inner join #tempPurchase as pur on bom.Item = pur.Item
		inner join LE_FlowMstrSnapShot as mstr on mstr.PartyFrom = pur.Region and mstr.PartyTo = bom.Region
		where bom.IsMatch = 0
	end
	-------------------↑第二次添加零件从采购入库地点到整车物料消耗库位的移库路线，区域不相同-----------------------
	
	-------------------↓没有找到采购入库地点-----------------------
	if exists (select top 1 * from #tempOrderBom where IsMatch = 0)
	begin
		--记录日志
		insert into LOG_RunLeanEngine(Lvl, ErrorId, Item, LocTo, Msg)
		select 1, 15, Item, Location, N'零件' + Item + N'整车物料消耗库位' + Location + N'没有找到采购入库地点'
		from #tempOrderBom where IsMatch = 0
	end
	-------------------↑没有找到采购入库地点-----------------------
	
	--记录日志
	set @Msg = N'整车物料消耗库位和采购入库地点匹配，添加缺失的移库路线结束'
	insert into LOG_RunLeanEngine(Lvl, Msg) values(0, @Msg)
	-------------------↑整车物料的消耗库位和采购入库地点匹配，添加缺失的路线-----------------------
	
	
	
	-------------------↓多供应商供货的选取供应商-----------------------
	--记录日志
	set @Msg = N'选取零件和目的库位相同的路线明细开始'
	insert into LOG_RunLeanEngine(Lvl, Msg) values(0, @Msg)
	
	-------------------↓删除零件、目的库位和JIT路线相同的看板路线-----------------------
	--JIT供货的优先级大于看板供货
	--记录日志
	insert into LOG_RunLeanEngine(Flow, Lvl, ErrorId, Item, LocTo, Msg)
	select det2.Flow, 1, 16, det2.Item, det2.LocTo, N'删除零件' + det2.Item + N'目的库位' + det2.LocTo + N'和JIT路线相同的看板路线' 
	from #tempFlowDet as det2
	inner join LE_FlowMstrSnapShot as mstr2 on det2.Flow = mstr2.Flow
	inner join (select det.Item, det.LocTo 
				from #tempFlowDet as det 
				inner join LE_FlowMstrSnapShot as mstr on det.Flow = mstr.Flow
				where mstr.Strategy = 3 group by det.Item, det.LocTo) as det3
	on det2.Item = det3.Item and det2.LocTo = det3.LocTo
	where mstr2.Strategy = 2
	
	delete det2 
	from #tempFlowDet as det2
	inner join LE_FlowMstrSnapShot as mstr2 on det2.Flow = mstr2.Flow
	inner join (select det.Item, det.LocTo 
				from #tempFlowDet as det 
				inner join LE_FlowMstrSnapShot as mstr on det.Flow = mstr.Flow
				where mstr.Strategy = 3 group by det.Item, det.LocTo) as det3
	on det2.Item = det3.Item and det2.LocTo = det3.LocTo
	where mstr2.Strategy = 2
	-------------------↑删除零件、目的库位和JIT路线相同的看板路线-----------------------
	
	-------------------↓按零件和目的库位分组-----------------------
	truncate table #tempMultiSupplierGroup
	insert into #tempMultiSupplierGroup(Item, LocTo)
	select Item, LocTo from #tempFlowDet
	where LocTo is not null
	group by Item, LocTo having COUNT(*) > 1 
	
	truncate table #tempMultiSupplierItem
	insert into #tempMultiSupplierItem(FlowDetRowId, Flow, Item, LocTo, MSGRowId, MrpTotal, MrpTotalAdj, MrpWeight)
	select det.Id, det.Flow, det.Item, det.LocTo, msg.RowId, det.MrpTotal, det.MrpTotalAdj, det.MrpWeight 
	from #tempFlowDet as det
	inner join #tempMultiSupplierGroup as msg on det.Item = msg.Item and det.LocTo = msg.LocTo
	-------------------↑按零件和目的库位分组-----------------------
	
	-------------------↓零件和目的库位相同的路线都没有设置供货比例，按相同比例平均分配供货-----------------------
	--记录日志
	insert into LOG_RunLeanEngine(Flow, Lvl, ErrorId, Item, LocTo, Msg)
	select Flow, 1, 17, Item, LocTo, N'零件为' + Item + N'目的库位为' + LocTo + N'的路线都没有设置供货比例，按相同比例平均分配供货' 
	from #tempMultiSupplierItem where MSGRowId in (select MSGRowId from #tempMultiSupplierItem group by MSGRowId having SUM(MrpWeight) = 0)
	
	update #tempMultiSupplierItem set MrpWeight = 50
	where MSGRowId in (select MSGRowId from #tempMultiSupplierItem group by MSGRowId having SUM(MrpWeight) = 0)
	-------------------↑零件和目的库位相同的路线都没有设置供货比例，按相同比例平均分配供货-----------------------
	
	-------------------↓零件和目的库位相同的路线没有设置供货比例，忽略这些路线明细-----------------------
	insert into LOG_RunLeanEngine(Flow, Lvl, ErrorId, Item, LocTo, Msg)
	select Flow, 1, 18, Item, LocTo, N'零件为' + Item + N'目的库位为' + LocTo + N'的路线没有设置供货比例，忽略该条路线明细'
	from #tempMultiSupplierItem where MSGRowId in (select MSGRowId from #tempMultiSupplierItem where MrpWeight = 0)
	
	delete #tempFlowDet where Id in (select FlowDetRowId from #tempMultiSupplierItem where MrpWeight = 0)
	delete #tempMultiSupplierItem where FlowDetRowId in (select FlowDetRowId from #tempMultiSupplierItem where MrpWeight = 0)
	-------------------↑零件和目的库位相同的路线没有设置供货比例，忽略这些路线明细-----------------------
	
	-------------------↓按零件和目的库位汇总供货总量和比例总量-----------------------
	update msi set TotalQty = msg.TotalQty, TotalWeight = msg.TotalWeight
	from #tempMultiSupplierItem as msi inner join 
		(select Item, LocTo, SUM(MrpTotal + MrpTotalAdj) as TotalQty, SUM(MrpWeight) as TotalWeight 
		from #tempMultiSupplierItem group by Item, LocTo) as msg on msg.Item = msi.Item and msg.LocTo = msi.LocTo
	-------------------↑按零件和目的库位汇总供货总量和比例总量-----------------------
	
	-------------------↓所有路线都没有供过货，取比例最大的一条路线明细供货-----------------------
	truncate table #tempSortedMultiSupplierItem
	insert into #tempSortedMultiSupplierItem(GID, FlowDetRowId, Flow)
	select ROW_NUMBER() over(partition by MSGRowId order by MRPWeight) as GID, FlowDetRowId, Flow
	from #tempMultiSupplierItem where TotalQty = 0
	
	--记录日志
	insert into LOG_RunLeanEngine(Flow, Lvl, Msg)
	select Flow, 0, N'所有路线都没有供过货，取比例最大的一条路线明细供货' from #tempSortedMultiSupplierItem where GID = 1
	
	delete #tempFlowDet where Id in (select FlowDetRowId from #tempSortedMultiSupplierItem where GID <> 1)
	delete #tempMultiSupplierItem where FlowDetRowId in (select FlowDetRowId from #tempSortedMultiSupplierItem where GID <> 1)
	-------------------↑所有路线都没有供过货，取比例最大的一条路线明细供货-----------------------
	
	-------------------↓选取供货比率最小的路线明细供货-----------------------
	truncate table #tempSortedMultiSupplierItem
	insert into #tempSortedMultiSupplierItem(GID, FlowDetRowId, Flow)
	select ROW_NUMBER() over(partition by MSGRowId order by (MrpWeight / TotalWeight) / ((MrpTotal + MrpTotalAdj) / TotalQty), MrpWeight) as GID, FlowDetRowId, Flow
	from #tempMultiSupplierItem where TotalQty > 0
	
	--记录日志
	insert into LOG_RunLeanEngine(Flow, Lvl, Msg)
	select Flow, 0, N'选取供货比率最小的路线明细供货' from #tempSortedMultiSupplierItem where GID = 1
	
	delete #tempFlowDet where Id in (select FlowDetRowId from #tempSortedMultiSupplierItem where GID <> 1)
	delete #tempMultiSupplierItem where FlowDetRowId in (select FlowDetRowId from #tempSortedMultiSupplierItem where GID <> 1)
	-------------------↑选取供货比率最小的路线明细供货-----------------------
	
	drop table #tempTransfer
	drop table #tempPurchase
	
	set @Msg = N'选取零件和目的库位相同的路线明细结束'
	insert into LOG_RunLeanEngine(Lvl, Msg) values(0, @Msg)
	-------------------↑多供应商供货的选取供应商-----------------------
	
	
	
	-------------------↓计算其它需求源-----------------------
	insert into #tempExtraDmdSource(FlowDetId, ExtraDmdSource)
	select FlowDetId, ExtraDmdSource from #tempFlowDet where ExtraDmdSource is not null and ExtraDmdSource <>  ''
	
	declare @SplitSymbol1 char(1) = ','
	declare @SplitSymbol2 char(1) = '|'
	
	declare @FlowDetId int
	declare @FlowDetRowId int
	declare @MaxFlowDetRowId int

	select @FlowDetRowId = MIN(RowId), @MaxFlowDetRowId = MAX(RowId) from #tempExtraDmdSource
	while (@FlowDetRowId <= @MaxFlowDetRowId)
	begin
		select @FlowDetId = FlowDetId, @ExtraDmdSource = ExtraDmdSource from #tempFlowDet where Id = @FlowDetRowId
		
		if ISNULL(@ExtraDmdSource, '') <> ''
		begin
			if (charindex(@SplitSymbol1, @ExtraDmdSource) <> 0)
				begin
				--循环其它需求源插入缓存表中
				while(charindex(@SplitSymbol1, @ExtraDmdSource) <> 0)
				begin
					insert LE_FlowDetExtraDmdSource(FlowDetId, Location) values (@FlowDetId, substring(@ExtraDmdSource, 1, charindex(@SplitSymbol1, @ExtraDmdSource) - 1))
					set @ExtraDmdSource = stuff(@ExtraDmdSource, 1, charindex(@SplitSymbol1, @ExtraDmdSource), ' ')
				end
			end
			else if (charindex(@SplitSymbol2, @ExtraDmdSource) <> 0)
			begin
				--循环其它需求源插入缓存表中
				while(charindex(@SplitSymbol2, @ExtraDmdSource) <> 0)
				begin
					insert LE_FlowDetExtraDmdSource(FlowDetId, Location) values (@FlowDetId, substring(@ExtraDmdSource, 1, charindex(@SplitSymbol2, @ExtraDmdSource) - 1))
					set @ExtraDmdSource = stuff(@ExtraDmdSource, 1, charindex(@SplitSymbol2, @ExtraDmdSource), ' ')
				end
			end
			
			insert LE_FlowDetExtraDmdSource(FlowDetId, Location) values (@FlowDetId, Ltrim(@ExtraDmdSource))
		end
		
		set @FlowDetRowId = @FlowDetRowId + 1
	end
	
	-------------------↑计算其它需求源-----------------------
	
	
	
	truncate table LE_FlowDetSnapShot
	insert into LE_FlowDetSnapShot(Flow, FlowDetId, Item, Uom, UC, ManufactureParty, LocFrom, LocTo, IsRefFlow, SafeStock, MaxStock, MinLotSize, RoundUpOpt, Strategy)
	select Flow, FlowDetId, Item, Uom, UC, ManufactureParty, LocFrom, LocTo, IsRefFlow, SafeStock, MaxStock, MinLotSize, RoundUpOpt, Strategy from #tempFlowDet
	
	
	
	drop table #tempExtraDmdSource
	drop table #tempOrderBom
	drop table #tempSortedMultiSupplierItem
	drop table #tempMultiSupplierItem
	drop table #tempMultiSupplierGroup
	drop table #tempFlowDet
	
	--记录日志
	set @Msg = N'获取路线明细结束'
	insert into LOG_RunLeanEngine(Lvl, Msg) values(0, @Msg)
END
