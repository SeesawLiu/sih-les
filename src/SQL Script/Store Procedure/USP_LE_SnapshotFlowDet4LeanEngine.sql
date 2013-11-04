SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.objects WHERE type='P' AND name='USP_LE_SnapshotFlowDet4LeanEngine')
	DROP PROCEDURE USP_LE_SnapshotFlowDet4LeanEngine
GO

CREATE PROCEDURE [dbo].[USP_LE_SnapshotFlowDet4LeanEngine] 
(
	@BatchNo int,
	@LERunTime datetime
)
--WITH ENCRYPTION
AS
BEGIN
	SET NOCOUNT ON
	declare @Msg nvarchar(Max)
	declare @trancount int
	
	CREATE TABLE #tempFlowDet 
	(
		Id int identity(1, 1) Primary Key,
		Flow varchar(50) NULL,
		FlowDetId int NULL,
		Item varchar(50) NOT NULL,
		Uom varchar(5) NULL,
		UC decimal(18, 8) NULL,
		MinUC decimal(18, 8) NULL,
		Container varchar(50) NULL,
		ContainerDesc varchar(50) NULL,
		UCDesc varchar(50) NULL,
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
		DeliveryCount int,  --已供货次数
		DeliveryBalance int   --余量
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
		IsMatch bit
	)
	
	create table #tempPurchase
	(
		Id int identity(1, 1) Primary Key,
		Item varchar(50),
		Region varchar(50),
		Location varchar(50)
	)
	
	create table #tempTransfer
	(
		Id int identity(1, 1) Primary Key,
		Item varchar(50),
		PartyFrom varchar(50),
		PartyTo varchar(50),
		LocFrom varchar(50),
		LocTo varchar(50)
	)
	
	Create table #tempExtraDmdSource
	(
		RowId int identity(1, 1),
		FlowDetSnapShotId int,
		FlowDetLocTo varchar(50),
		ExtraDmdSource varchar(256)
	)
	
	CREATE TABLE #temp_LOG_SnapshotFlowDet4LeanEngine(
		Flow varchar(50) NULL,
		Item varchar(50) NULL,
		LocFrom varchar(50) NULL,
		LocTo varchar(50) NULL,
		OrderNo varchar(50) NULL,
		TraceCode varchar(50) NULL,
		Lvl tinyint NULL,
		ErrorId tinyint NULL,
		Msg varchar(500) NULL,
		CreateDate datetime NULL default(GETDATE()),
	)
	
	create table #tempCompleteFlowDet  --完整路线RowId
	(
		FlowDetRowId int,
		Item varchar(50),
		LocTo varchar(50)
	)
	
	--记录日志
	set @Msg = N'获取路线明细开始'
	insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Lvl, Msg) values(0, @Msg)
	
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
		declare @WinTimeDiff decimal(18, 8) = null  --秒，进料提前期
		declare @SafeTime decimal(18, 8) = null  --秒，安全期：需求覆盖提前期，@ReqTimeTo会往后增加安全期的时间
		declare @LeadTime decimal(18, 8) = null  --秒，提前期：发单提前期
		declare @EMLeadTime decimal(18, 8) = null  --秒，紧急提前期
		declare @PrevWinTime datetime = null
		declare @WindowTime datetime = null
		declare @WindowTime2 datetime = null
		declare @EMWindowTime datetime = null
		declare @ReqTimeFrom datetime = null
		declare @ReqTimeTo datetime = null
		declare @OrderTime datetime = null
		declare @EMOrderTime datetime = null
		
		select @Flow = Flow, @FlowType = [Type], 
		@PartyFrom = PartyFrom, @PartyTo = PartyTo,
		@LocFrom = LocFrom, @LocTo = LocTo, @ExtraDmdSource = ExtraDmdSource
		from LE_FlowMstrSnapShot where Id = @FlowRowId
		
		--记录日志
		set @Msg = N'计算路线需求时间段和发单时间开始'
		insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Flow, Lvl, Msg) values(@Flow, 0, @Msg)
		
		set @trancount = @@trancount
		begin try
			if @trancount = 0
			begin
				begin tran
			end		
		
			select @FlowStrategy = Strategy, @WinTimeType = WinTimeType, 
			@WinTimeDiff = WinTimeDiff * 60 * 60, --小时转为秒
			@SafeTime = ISNULL(SafeTime, 0) * 60 * 60, --小时转为秒
			@LeadTime = -LeadTime * 60 * 60, --小时转为秒
			@EMLeadTime = -EMLeadTime * 60 * 60,	   --小时转为秒
			@PrevWinTime = PreWinTime,       --上次窗口时间
			@WindowTime = NextWinTime
			from SCM_FlowStrategy with(UPDLOCK) where Flow = @Flow
			
			--计算下次窗口开始时间
			exec USP_Busi_GetNextWindowTime @Flow, @PrevWinTime, @WindowTime output
			
			--计算紧急发单时间
			exec USP_Busi_SubtractWorkingDate @WindowTime, @EMLeadTime, null, @PartyTo, @EMOrderTime output
			
			--根据下次窗口时间计算下次紧急发单时间，如果下次紧急发单时间小于当前时间重新查找下次窗口开始时间和紧急发单时间
			if (@EMOrderTime < @LERunTime)
			begin  --紧急发单时间小于当前时间重新查找下次窗口开始时间和紧急发单时间
				set @Msg = N'根据窗口时间' + + CONVERT(varchar, @WindowTime, 120) + + N'计算的紧急发单时间' + CONVERT(varchar, @EMOrderTime, 120) + N'小于当前时间，重新计算窗口时间'
				insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Flow, Lvl, ErrorId, Msg) values(@Flow, 1, 11, @Msg)
				
				exec USP_Busi_GetNextWindowTime @Flow, @WindowTime, @WindowTime output
				exec USP_Busi_SubtractWorkingDate @WindowTime, @EMLeadTime, null, @PartyTo, @EMOrderTime output
			end
			
			--计算本次窗口结束时间
			exec USP_Busi_GetNextWindowTime @Flow, @WindowTime, @WindowTime2 output
			
			--计算紧急窗口时间
			set @EMLeadTime = 0 - @EMLeadTime
			exec USP_Busi_AddWorkingDate @LERunTime, @EMLeadTime, null, @PartyTo, @EMWindowTime output
			if @EMWindowTime > @WindowTime
			begin
				set @EMWindowTime = @WindowTime
			end
		
			--计算发单时间
			exec USP_Busi_SubtractWorkingDate @WindowTime, @LeadTime, null, @PartyTo, @OrderTime output
			
			--计算下次需求开始时间，根据进料提前期计算实际需求开始时间
			exec USP_Busi_AddWorkingDate @WindowTime, @WinTimeDiff, null, @PartyTo, @ReqTimeFrom output
		
			--计算下次需求结束时间，根据进料提前期计算实际需求开始时间
			set @SafeTime = @SafeTime + @WinTimeDiff
			exec USP_Busi_AddWorkingDate @WindowTime2, @SafeTime, null, @PartyTo, @ReqTimeTo output
			
			--记录日志
			set @Msg = N'路线需求时间为' + CONVERT(varchar, @ReqTimeFrom, 120) + N'~' + CONVERT(varchar, @ReqTimeTo, 120)
				+ '，发单时间为' + CONVERT(varchar, @OrderTime, 120)
				+ '，窗口时间为' + CONVERT(varchar, @WindowTime, 120)
				+ '，紧急窗口时间为' + CONVERT(varchar, @EMWindowTime, 120)
			insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Flow, Lvl, Msg) values(@Flow, 0, @Msg)

			--更新路线
			update LE_FlowMstrSnapShot set OrderTime = @OrderTime, ReqTimeFrom = @ReqTimeFrom, ReqTimeTo = @ReqTimeTo, 
			WindowTime = @WindowTime, EMWindowTime = @EMWindowTime
			where Id = @FlowRowId
			
			--获取路线明细
			insert into #tempFlowDet(Flow, FlowDetId, Item, Uom, UC, MinUC, ManufactureParty,
			LocFrom, LocTo,
			ExtraDmdSource,
			MrpTotal, MrpTotalAdj, MrpWeight, IsRefFlow,
			SafeStock, MaxStock, MinLotSize, RoundUpOpt, Strategy,
			Container, ContainerDesc, UCDesc)
			select @Flow, det.Id as FlowDetId, det.Item, det.Uom, det.UC, det.MinUC, det.ManufactureParty, 
			@LocFrom, @LocTo,
			ISNULL(det.ExtraDmdSource, @ExtraDmdSource) as ExtraDmdSource,
			ISNULL(det.MrpTotal, 0), ISNULL(det.MrpTotalAdj, 0), ISNULL(det.MrpWeight, 0) as MrpWeight, 0,
			ISNULL(pref.SafeStock, 0), ISNULL(pref.MaxStock, 0), ISNULL(det.MinLotSize, 0), det.RoundUpOpt, @FlowStrategy,
			det.Container, det.ContainerDesc, det.UCDesc
			from SCM_FlowDet as det
			left join INV_LocationDetPref as pref on det.Item = pref.Item and pref.Location = @LocTo
			where det.Flow = @Flow 
			--and (det.StartDate is null or (det.StartDate <= @@LERunTime))
			--and (det.EndDate is null or (det.EndDate >= @@LERunTime))
			--and det.IsAutoCreate = 1 
			--and det.IsActive = 1

			if @OrderTime <= @LERunTime
			begin
				--记录日志
				set @Msg = N'路线发单时间' + CONVERT(varchar, @OrderTime, 120) + '小于当前时间更新路线下次窗口时间为' + CONVERT(varchar, @WindowTime2, 120)
				insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Flow, Lvl, Msg) values(@Flow, 0, @Msg)
				
				--更新路线上次窗口时间
				update SCM_FlowStrategy set PreWinTime = @WindowTime, NextWinTime = @WindowTime2 where Flow = @Flow
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
			
			delete from LE_FlowMstrSnapShot where Flow = @Flow
			delete from LE_FlowDetSnapShot where Flow = @Flow
		
			--记录日志
			set @Msg = N'计算路线需求时间段和发单时间出现异常，异常信息：' + Error_Message()
			insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Flow, Lvl, ErrorId, Msg) values(@Flow, 2, 12, @Msg)
		end catch
		
		--记录日志
		set @Msg = N'计算路线需求时间段和发单时间结束'
		insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Flow, Lvl, Msg) values(@Flow, 0, @Msg)
		
		set @FlowRowId = @FlowRowId + 1
	end
	-------------------↑计算每条路线的需求时间段和发单时间-----------------------
	
	

	-------------------↓计算引用路线需求时间段和发单时间-----------------------
	--记录日志
	set @Msg = N'计算引用路线需求时间段和发单时间开始'
	insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Lvl, Msg) values(0, @Msg)
	
	insert into #tempFlowDet(Flow, FlowDetId, Item, UOM, UC, MinUC, ManufactureParty,
	LocFrom, LocTo,
	ExtraDmdSource,
	IsRefFlow, MrpTotal, MrpTotalAdj, MrpWeight,
	SafeStock, MaxStock, MinLotSize, RoundUpOpt, Strategy,
	Container, ContainerDesc, UCDesc)
	select mstr.Code as Flow, fdet.FlowDetId, fdet.Item, fdet.UOM, fdet.UC, fdet.MinUC, fdet.ManufactureParty, 
	mstr.LocFrom, mstr.LocTo,
	mstr.ExtraDmdSource,
	1, 0, 0, 0,
	ISNULL(pref.SafeStock, 0), ISNULL(pref.MaxStock, 0), ISNULL(fdet.MinLotSize, 0), fdet.RoundUpOpt, stra.Strategy,
	fdet.Container, fdet.ContainerDesc, fdet.UCDesc
	from SCM_FlowMstr as mstr 
	inner join SCM_FlowStrategy as stra on mstr.Code = stra.Flow
	inner join LE_FlowMstrSnapShot as fmstr on mstr.Code = fmstr.Flow
	inner join #tempFlowDet as fdet on mstr.RefFlow = fdet.Flow
	left join INV_LocationDetPref as pref on fdet.Item = pref.Item and mstr.LocTo = pref.Location
	where mstr.IsActive = 1 and mstr.IsAutoCreate = 1
	and stra.Strategy in (2, 3)
	and not Exists(select top 1 1 from #tempFlowDet as tDet where tdet.Item = fdet.Item and tDet.LocTo = mstr.LocTo)
	
	--记录日志
	set @Msg = N'计算引用路线需求时间段和发单时间结束'
	insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Lvl, Msg) values(0, @Msg)
	-------------------↑计算引用路线的需求时间段和发单时间-----------------------
	
	
	
	-------------------↓检验路线是否完整-----------------------	
	--while 1 = 1
	--begin
	--	insert into #tempCompleteFlowDet(FlowDetRowId, Item, LocTo)
	--	select det.Id, det.Item, det.LocTo 
	--	from #tempFlowDet as det
	--	inner join LE_FlowMstrSnapShot as mstr on det.Flow = mstr.Flow
	--	inner join #tempCompleteFlowDet as cDet on (cDet.Item = det.Item and cDet.LocTo = det.LocFrom) --采购路线是完整路线
	--	where not exists(select top 1 1 from #tempCompleteFlowDet tDet where tDet.FlowDetRowId = det.Id)  --过滤掉已经添加到完整路线表中的路线明细
		
	--	if @@ROWCOUNT = 0
	--	begin
	--		break
	--	end
	--end
	
	--insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Flow, Lvl, ErrorId, Item, LocFrom, LocTo, Msg)
	--select distinct det.Flow, 22, 16, det.Item, det.LocFrom, det.LocTo, N'因为路线不完整删除零件' + det.Item + N'来源库位' + det.LocFrom + N'目的库位' + det.LocTo + N'的路线' 
	--from #tempFlowDet as det 
	--left join #tempCompleteFlowDet as cDet on det.Id = cDet.FlowDetRowId
	--where cDet.FlowDetRowId is null
	
	--delete det 
	--from #tempFlowDet as det 
	--left join #tempCompleteFlowDet as cDet on det.Id = cDet.FlowDetRowId
	--where cDet.FlowDetRowId is null
	-------------------↑检验路线是否完整-----------------------
	
	
	
	-------------------↓整车物料的消耗库位和采购入库地点匹配，添加缺失的路线-----------------------
	--记录日志
	set @Msg = N'整车物料消耗库位和采购入库地点匹配，添加缺失的移库路线开始'
	insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Lvl, Msg) values(0, @Msg)
	
	--缓存鞍座和变速器的工作中心
	select distinct wc.WorkCenter into #tempWorkCenter from SCM_FlowMstr as mstr
	inner join PRD_ProdLineWorkCenter as wc on mstr.Code = wc.Flow
	where mstr.Code in (
	select TransFlow from CUST_ProductLineMap where [Type] = 1 and IsActive = 1
	UNION ALL
	select SaddleFlow from CUST_ProductLineMap where [Type] = 1 and IsActive = 1)
	and mstr.IsActive = 1
	
	--汇总整车物料需求（物料+区域+库位）
	insert into #tempOrderBom(Item, Location, IsMatch)
	select Item, Location, 0 from LE_OrderBomCPTimeSnapshot as bom
	where WorkCenter not in (select WorkCenter from #tempWorkCenter)
	group by Item, Location
	
	--删除鞍座和变速器的工作中心的缓存表
	drop table #tempWorkCenter
	
	--更新库位区域
	update bom set Region = loc.Region
	from #tempOrderBom as bom inner join MD_Location as loc on bom.Location = loc.Code
	
	
	--汇总采购入库地点（物料+区域+库位）
	insert into #tempPurchase(Item, Region, Location)
	--select det.Item, mstr.PartyTo as Region, ISNULL(det.LocTo, mstr.LocTo) as Location 
	select det.Item, mstr.PartyTo as Region, mstr.LocTo as Location 
	from SCM_FlowDet as det inner join SCM_FlowMstr as mstr 
	on det.Flow = mstr.Code
	where mstr.[Type] in (1, 5, 6, 8) and mstr.IsActive = 1 --and det.IsActive = 1
	
	--汇总移库路线
	insert into #tempTransfer(Item, PartyFrom, PartyTo, LocFrom, LocTo)
	--select det.Item, mstr.PartyFrom, mstr.PartyTo, ISNULL(det.LocFrom, mstr.LocFrom) as LocFrom, ISNULL(det.LocTo, mstr.LocTo) as LocTo
	select det.Item, mstr.PartyFrom, mstr.PartyTo, mstr.LocFrom, mstr.LocTo
	from SCM_FlowDet as det inner join SCM_FlowMstr as mstr 
	on det.Flow = mstr.Code
	where mstr.[Type] in (2, 7) and mstr.IsActive = 1 --and det.IsActive = 1
	
	--第一次匹配，消耗库位和采购入库地点相同
	update bom set IsMatch = 1
	from #tempOrderBom as bom
	inner join #tempPurchase as pur on bom.Item = pur.Item and bom.Location = pur.Location
	
	--第二次匹配，消耗库位和采购入库地点通过移库路线关联
	if exists (select top 1 1 from #tempOrderBom where IsMatch = 0)
	begin
		update bom set IsMatch = 1
		from #tempOrderBom as bom
		inner join #tempTransfer as tra on bom.Item = tra.Item and bom.Location = tra.LocTo
		inner join #tempPurchase as pur on tra.Item = pur.Item and tra.LocFrom = pur.Location
		where bom.IsMatch = 0
	end
	
	--第三次匹配，消耗库位和采购入库地点通过2条移库路线关联
	if exists (select top 1 1 from #tempOrderBom where IsMatch = 0)
	begin
		update bom set IsMatch = 1
		from #tempOrderBom as bom
		inner join #tempTransfer as tra1 on bom.Item = tra1.Item and bom.Location = tra1.LocTo
		inner join #tempTransfer as tra2 on tra1.Item = tra2.Item and tra1.LocFrom = tra2.LocTo
		inner join #tempPurchase as pur on tra2.Item = pur.Item and tra2.LocFrom = pur.Location
		where bom.IsMatch = 0
	end
	
	-------------------↓第一次添加零件从采购入库地点到整车物料消耗库位的移库路线,区域相同-----------------------
	if exists (select top 1 1 from #tempOrderBom where IsMatch = 0)
	begin
		--记录日志
		insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Lvl, ErrorId, Item, LocFrom, LocTo, Msg)
		select distinct 1, 13, bom.Item, pur.Location, bom.Location, N'添加零件' + bom.Item + N'从采购入库地点' + pur.Location + N'到整车物料消耗库位' + bom.Location + N'的移库路线'
		from #tempOrderBom as bom
		inner join #tempPurchase as pur on bom.Item = pur.Item and bom.Region = pur.Region
		inner join LE_FlowMstrSnapShot as mstr on mstr.PartyFrom = pur.Region and mstr.PartyTo = bom.Region and mstr.Strategy = 3
		where bom.IsMatch = 0
		
		insert into #tempFlowDet(Flow, FlowDetId, Item, UOM, UC, MinUC, ManufactureParty,
		LocFrom, LocTo,
		ExtraDmdSource,
		IsRefFlow, MrpTotal, MrpTotalAdj, MrpWeight,
		SafeStock, MaxStock, MinLotSize, RoundUpOpt, Strategy,
		Container, ContainerDesc, UCDesc)
		select distinct mstr.Flow, 0, bom.Item, item.UOM, item.MinUC, item.MinUC, null,  
		pur.Location, bom.Location,
		null,
		0, 0, 0, 0,
		ISNULL(pref.SafeStock, 0), ISNULL(pref.MaxStock, 0), 0, 0, 3,
		item.Container, item.ContainerDesc, null
		from #tempOrderBom as bom
		inner join #tempPurchase as pur on bom.Item = pur.Item and bom.Region = pur.Region
		inner join MD_Item as item on bom.Item = item.Code
		inner join LE_FlowMstrSnapShot as mstr on mstr.PartyFrom = pur.Region and mstr.PartyTo = bom.Region and mstr.Strategy = 3
		left join INV_LocationDetPref as pref on bom.Item = pref.Item and bom.Location = pref.Location
		where bom.IsMatch = 0
		
		update bom set IsMatch = 1
		from #tempOrderBom as bom
		inner join #tempPurchase as pur on bom.Item = pur.Item and bom.Region = pur.Region
		inner join LE_FlowMstrSnapShot as mstr on mstr.PartyFrom = pur.Region and mstr.PartyTo = bom.Region and mstr.Strategy = 3
		where bom.IsMatch = 0
	end
	-------------------↑第一次添加零件从采购入库地点到整车物料消耗库位的移库路线,区域相同-----------------------
	
	-------------------↓第二次添加零件从移库路线出库库位到整车物料消耗库位的移库路线，区域相同（针对缺失的Buffer至线边的移库路线）-----------------------
	if exists (select top 1 1 from #tempOrderBom where IsMatch = 0)
	begin
		--记录日志
		insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Lvl, ErrorId, Item, LocFrom, LocTo, Msg)
		select distinct 1, 14, bom.Item, tra.LocTo, bom.Location, N'添加零件' + bom.Item + N'从移库路线入库库位' + tra.LocTo + N'到整车物料消耗库位' + bom.Location + N'的移库路线'
		from #tempOrderBom as bom
		inner join #tempTransfer as tra on bom.Item = tra.Item and bom.Region = tra.PartyTo
		inner join #tempPurchase as pur on tra.Item = pur.Item and tra.LocFrom = pur.Location
		inner join LE_FlowMstrSnapShot as mstr on mstr.LocFrom = tra.LocTo and mstr.LocTo = bom.Location and mstr.Strategy = 3
		where bom.IsMatch = 0
		
		insert into #tempFlowDet(Flow, FlowDetId, Item, UOM, UC, MinUC, ManufactureParty,
		LocFrom, LocTo,
		ExtraDmdSource,
		IsRefFlow, MrpTotal, MrpTotalAdj, MrpWeight,
		SafeStock, MaxStock, MinLotSize, RoundUpOpt, Strategy,
		Container, ContainerDesc, UCDesc)
		select distinct mstr.Flow, 0, bom.Item, item.UOM, item.MinUC, item.MinUC, null,  
		tra.LocTo, bom.Location,
		null,
		0, 0, 0, 0,
		ISNULL(pref.SafeStock, 0), ISNULL(pref.MaxStock, 0), 0, 0, 3,
		item.Container, item.ContainerDesc, null
		from #tempOrderBom as bom
		inner join #tempTransfer as tra on bom.Item = tra.Item and bom.Region = tra.PartyTo
		inner join #tempPurchase as pur on tra.Item = pur.Item and tra.LocFrom = pur.Location
		inner join MD_Item as item on bom.Item = item.Code
		inner join LE_FlowMstrSnapShot as mstr on mstr.LocFrom = tra.LocTo and mstr.LocTo = bom.Location and mstr.Strategy = 3
		left join INV_LocationDetPref as pref on bom.Item = pref.Item and bom.Location = pref.Location
		where bom.IsMatch = 0
		
		update bom set IsMatch = 1
		from #tempOrderBom as bom
		inner join #tempTransfer as tra on bom.Item = tra.Item and bom.Region = tra.PartyTo
		inner join #tempPurchase as pur on tra.Item = pur.Item and tra.LocFrom = pur.Location
		inner join LE_FlowMstrSnapShot as mstr on mstr.LocFrom = tra.LocTo and mstr.LocTo = bom.Location and mstr.Strategy = 3
		where bom.IsMatch = 0
	end
	-------------------↑第二次添加零件从移库路线出库库位到整车物料消耗库位的移库路线，区域相同（针对缺失的Buffer至线边的移库路线）-----------------------
	
	-------------------↓第三次添加零件从采购入库地点到整车物料消耗库位的移库路线，区域不相同，原材料消耗库位和采购路线库位相同-----------------------
	if exists (select top 1 1 from #tempOrderBom where IsMatch = 0)
	begin
		--记录日志
		insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Lvl, ErrorId, Item, LocFrom, LocTo, Msg)
		select distinct 1, 14, bom.Item, pur.Location, bom.Location, N'添加零件' + bom.Item + N'从采购入库地点' + pur.Location + N'到整车物料消耗库位' + bom.Location + N'的移库路线'
		from #tempOrderBom as bom
		inner join #tempPurchase as pur on bom.Item = pur.Item
		inner join LE_FlowMstrSnapShot as mstr on mstr.PartyFrom = pur.Region and mstr.PartyTo = bom.Region and mstr.Strategy = 3
		where bom.IsMatch = 0
		
		insert into #tempFlowDet(Flow, FlowDetId, Item, UOM, UC, MinUC, ManufactureParty,
		LocFrom, LocTo,
		ExtraDmdSource,
		IsRefFlow, MrpTotal, MrpTotalAdj, MrpWeight,
		SafeStock, MaxStock, MinLotSize, RoundUpOpt, Strategy,
		Container, ContainerDesc, UCDesc)
		select distinct mstr.Flow, 0, bom.Item, item.UOM, item.MinUC, item.MinUC, null,  
		pur.Location, bom.Location,
		null,
		0, 0, 0, 0,
		ISNULL(pref.SafeStock, 0), ISNULL(pref.MaxStock, 0), 0, 0, 3,
		item.Container, item.ContainerDesc, null
		from #tempOrderBom as bom
		inner join #tempPurchase as pur on bom.Item = pur.Item
		inner join MD_Item as item on bom.Item = item.Code
		inner join LE_FlowMstrSnapShot as mstr on mstr.LocFrom = pur.Location and mstr.LocTo = bom.Location and mstr.Strategy = 3
		left join INV_LocationDetPref as pref on bom.Item = pref.Item and bom.Location = pref.Location
		where bom.IsMatch = 0
		
		update bom set IsMatch = 1
		from #tempOrderBom as bom
		inner join #tempPurchase as pur on bom.Item = pur.Item
		inner join LE_FlowMstrSnapShot as mstr on mstr.LocFrom = pur.Location and mstr.LocTo = bom.Location and mstr.Strategy = 3
		where bom.IsMatch = 0
	end
	-------------------↑第三次添加零件从采购入库地点到整车物料消耗库位的移库路线，区域不相同，原材料消耗库位和采购路线库位相同-----------------------
	
	-------------------↓第四次添加零件从采购入库地点到整车物料消耗库位的移库路线，区域不相同，原材料消耗库位和采购路线库位不相同-----------------------
	if exists (select top 1 1 from #tempOrderBom where IsMatch = 0)
	begin
		--记录日志
		insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Lvl, ErrorId, Item, LocFrom, LocTo, Msg)
		select distinct 1, 14, bom.Item, pur.Location, bom.Location, N'添加零件' + bom.Item + N'从采购入库地点' + pur.Location + N'到整车物料消耗库位' + bom.Location + N'的移库路线'
		from #tempOrderBom as bom
		inner join #tempPurchase as pur on bom.Item = pur.Item
		inner join LE_FlowMstrSnapShot as mstr on mstr.PartyFrom = pur.Region and mstr.PartyTo = bom.Region and mstr.Strategy = 3
		where bom.IsMatch = 0
		
		insert into #tempFlowDet(Flow, FlowDetId, Item, UOM, UC, MinUC, ManufactureParty,
		LocFrom, LocTo,
		ExtraDmdSource,
		IsRefFlow, MrpTotal, MrpTotalAdj, MrpWeight,
		SafeStock, MaxStock, MinLotSize, RoundUpOpt, Strategy,
		Container, ContainerDesc, UCDesc)
		select distinct mstr.Flow, 0, bom.Item, item.UOM, item.MinUC, item.MinUC, null,  
		pur.Location, bom.Location,
		null,
		0, 0, 0, 0,
		ISNULL(pref.SafeStock, 0), ISNULL(pref.MaxStock, 0), 0, 0, 3,
		item.Container, item.ContainerDesc, null
		from #tempOrderBom as bom
		inner join #tempPurchase as pur on bom.Item = pur.Item
		inner join MD_Item as item on bom.Item = item.Code
		inner join LE_FlowMstrSnapShot as mstr on mstr.PartyFrom = pur.Region and mstr.PartyTo = bom.Region and mstr.Strategy = 3
		left join INV_LocationDetPref as pref on bom.Item = pref.Item and bom.Location = pref.Location
		where bom.IsMatch = 0
		
		update bom set IsMatch = 1
		from #tempOrderBom as bom
		inner join #tempPurchase as pur on bom.Item = pur.Item
		inner join LE_FlowMstrSnapShot as mstr on mstr.PartyFrom = pur.Region and mstr.PartyTo = bom.Region and mstr.Strategy = 3
		where bom.IsMatch = 0
	end
	-------------------↑第四次添加零件从采购入库地点到整车物料消耗库位的移库路线，区域不相同，原材料消耗库位和采购路线库位不相同-----------------------
	
	-------------------↓没有找到采购入库地点-----------------------
	if exists (select top 1 1 from #tempOrderBom where IsMatch = 0)
	begin
		--记录日志
		insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Lvl, ErrorId, Item, LocTo, Msg)
		select distinct 1, 15, bom.Item, bom.Location, N'零件' + bom.Item + N'整车物料消耗库位' + bom.Location + N'没有找到采购入库地点或没有创建从采购路线至物料消耗库位的移库路线'
		from #tempOrderBom as bom inner join MD_Item as i on bom.Item = i.Code 
		where bom.IsMatch = 0 and i.BESKZ not in('E', 'X') --过滤掉自制件
	end
	-------------------↑没有找到采购入库地点-----------------------
	
	--记录日志
	set @Msg = N'整车物料消耗库位和采购入库地点匹配，添加缺失的移库路线结束'
	insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Lvl, Msg) values(0, @Msg)
	-------------------↑整车物料的消耗库位和采购入库地点匹配，添加缺失的路线-----------------------
	
	
	
	-------------------↓多供应商供货的选取供应商-----------------------
	--记录日志
	set @Msg = N'选取零件和目的库位相同的路线明细开始'
	insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Lvl, Msg) values(0, @Msg)
	
	-------------------↓删除零件、目的库位和JIT路线相同的看板路线-----------------------
	--JIT供货的优先级大于看板供货
	--记录日志
	insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Flow, Lvl, ErrorId, Item, LocTo, Msg)
	select distinct det2.Flow, 1, 16, det2.Item, det2.LocTo, N'删除零件' + det2.Item + N'目的库位' + det2.LocTo + N'和JIT路线相同的看板路线' 
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
	
	-------------------↓先用配额选择多供应商供货的路线-----------------------
	--记录日志
	insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Flow, Lvl, ErrorId, Item, LocTo, Msg)
	select distinct det.Flow, 1, 20, det.Item, det.LocTo, N'根据配额选择供应商' + cDet.Supplier + N'路线' + cDet.Flow + N'删除路线' + det.Flow + N'零件' + det.Item + N'目的库位' + det.LocTo + N'的路线' 
	from #tempFlowDet as det 
	inner join LE_FlowMstrSnapShot as mstr on det.Flow = mstr.Flow
	inner join (select det.Id, det.Flow, det.Item, det.LocTo, lq.Supplier
				from #tempFlowDet as det 
				inner join LE_FlowMstrSnapShot as mstr on det.Flow = mstr.Flow
				inner join LE_QuotaSnapShot as lq on det.Item = lq.Item and mstr.PartyFrom = lq.Supplier
				where mstr.[Type] = 1) as cDet --当前供货的采购路线明细
				on det.Item = cDet.Item and det.LocTo = cDet.LocTo and det.Id <> cDet.Id and mstr.PartyFrom <> cDet.Supplier
	where mstr.[Type] = 1   --只过滤掉采购的路线
	
	delete det
	from #tempFlowDet as det 
	inner join LE_FlowMstrSnapShot as mstr on det.Flow = mstr.Flow
	inner join (select det.Id, det.Item, det.LocTo 
				from #tempFlowDet as det 
				inner join LE_FlowMstrSnapShot as mstr on det.Flow = mstr.Flow
				inner join LE_QuotaSnapShot as lq on det.Item = lq.Item and mstr.PartyFrom = lq.Supplier
				where mstr.[Type] = 1) as cDet --当前供货的采购路线明细
				on det.Item = cDet.Item and det.LocTo = cDet.LocTo and det.Id <> cDet.Id
	where mstr.[Type] = 1   --只过滤掉采购的路线
	-------------------↑先用配额选择多供应商供货的路线-----------------------
	
	-------------------↓按零件和目的库位分组-----------------------
	truncate table #tempMultiSupplierGroup
	insert into #tempMultiSupplierGroup(Item, LocTo)
	select Item, LocTo from #tempFlowDet
	where LocTo is not null
	group by Item, LocTo having COUNT(1) > 1 
	
	truncate table #tempMultiSupplierItem
	insert into #tempMultiSupplierItem(FlowDetRowId, Flow, Item, LocTo, MSGRowId, MrpTotal, MrpTotalAdj, MrpWeight)
	select det.Id, det.Flow, det.Item, det.LocTo, msg.RowId, det.MrpTotal, det.MrpTotalAdj, det.MrpWeight
	from #tempFlowDet as det
	inner join #tempMultiSupplierGroup as msg on det.Item = msg.Item and det.LocTo = msg.LocTo
	-------------------↑按零件和目的库位分组-----------------------		
	
	-------------------↓零件和目的库位相同的路线都没有设置供货循环量，按零件包装设置循环量-----------------------
	--记录日志
	insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Flow, Lvl, ErrorId, Item, LocTo, Msg)
	select distinct Flow, 1, 17, Item, LocTo, N'零件为' + Item + N'目的库位为' + LocTo + N'的路线都没有设置供货循环量，按零件包装设置循环量' 
	from #tempMultiSupplierItem where MSGRowId in (select MSGRowId from #tempMultiSupplierItem group by MSGRowId having SUM(MrpWeight) = 0)
	
	update tmp set MrpWeight = (CASE WHEN det.UC = 0 THEN 1 ELSE det.UC END)
	from #tempMultiSupplierItem as tmp
	inner join #tempFlowDet as det on det.Id = tmp.FlowDetRowId
	where tmp.MSGRowId in (select MSGRowId from #tempMultiSupplierItem group by MSGRowId having SUM(MrpWeight) = 0)
	-------------------↑零件和目的库位相同的路线都没有设置供货循环量，按零件包装设置循环量-----------------------
	
	-------------------↓零件和目的库位相同的路线没有设置供货循环量，忽略这些路线明细-----------------------
	insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Flow, Lvl, ErrorId, Item, LocTo, Msg)
	select distinct Flow, 1, 18, Item, LocTo, N'零件为' + Item + N'目的库位为' + LocTo + N'的路线没有设置供货循环量，忽略该条路线明细'
	from #tempMultiSupplierItem where MSGRowId in (select MSGRowId from #tempMultiSupplierItem where MrpWeight = 0)
	
	delete #tempFlowDet where Id in (select FlowDetRowId from #tempMultiSupplierItem where MrpWeight = 0)
	delete #tempMultiSupplierItem where FlowDetRowId in (select FlowDetRowId from #tempMultiSupplierItem where MrpWeight = 0)
	-------------------↑零件和目的库位相同的路线没有设置供货循环量，忽略这些路线明细-----------------------
	
	-------------------↓计算供货次数和余量-----------------------
	update #tempMultiSupplierItem set DeliveryCount = (ISNULL(MrpTotal, 0) + ISNULL(MrpTotalAdj, 0)) / MrpWeight, DeliveryBalance = (ISNULL(MrpTotal, 0) + ISNULL(MrpTotalAdj, 0)) % MrpWeight
	-------------------↑计算供货次数和余量-----------------------
	
	-------------------↓根据供货次数、循环量选取一条路线明细供货-----------------------
	truncate table #tempSortedMultiSupplierItem
	
	insert into #tempSortedMultiSupplierItem(GID, FlowDetRowId, Flow)
	select ROW_NUMBER() over(partition by MSGRowId order by DeliveryCount asc, MrpWeight desc, DeliveryBalance desc) as GID, FlowDetRowId, Flow 
	from #tempMultiSupplierItem
	
	--记录日志
	insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Flow, Lvl, ErrorId, Msg)
	select a.Flow, 1, 30, N'零件为' + b.Item + N'目的库位为' + b.LocTo + N'的路线明细重复，系统自动选取一条路线明细' 
	from #tempSortedMultiSupplierItem as a inner join #tempMultiSupplierItem as b on a.FlowDetRowId = b.FlowDetRowId 
	where a.GID = 1
	
	delete #tempFlowDet where Id in (select FlowDetRowId from #tempSortedMultiSupplierItem where GID <> 1)
	delete #tempMultiSupplierItem where FlowDetRowId in (select FlowDetRowId from #tempSortedMultiSupplierItem where GID <> 1)
	-------------------↑根据供货次数、循环量选取一条路线明细供货-----------------------
	
	drop table #tempTransfer
	drop table #tempPurchase
	
	set @Msg = N'选取零件和目的库位相同的路线明细结束'
	insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Lvl, Msg) values(0, @Msg)
	-------------------↑多供应商供货的选取供应商-----------------------
	


	truncate table LE_FlowDetSnapShot
	insert into LE_FlowDetSnapShot(Flow, FlowDetId, Item, Uom, UC, MinUC, 
	ManufactureParty, LocFrom, LocTo, IsRefFlow, SafeStock, MaxStock, 
	MinLotSize, RoundUpOpt, Strategy, ExtraDmdSource, Container, ContainerDesc, UCDesc)
	select det.Flow, det.FlowDetId, det.Item, det.Uom, det.UC, det.MinUC,
	det.ManufactureParty, det.LocFrom, det.LocTo, det.IsRefFlow, det.SafeStock, det.MaxStock, 
	det.MinLotSize, det.RoundUpOpt, det.Strategy, det.ExtraDmdSource, det.Container, det.ContainerDesc, det.UCDesc
	from #tempFlowDet as det
	
	
	
	-------------------↓计算其它需求源-----------------------
	truncate table LE_FlowDetExtraDmdSourceSnapshot
	insert into #tempExtraDmdSource(FlowDetSnapShotId, FlowDetLocTo, ExtraDmdSource)
	select Id, LocTo, ExtraDmdSource from LE_FlowDetSnapShot where ExtraDmdSource is not null and ExtraDmdSource <>  ''
	
	declare @SplitSymbol1 char(1) = ','
	declare @SplitSymbol2 char(1) = '|'
	
	declare @FlowDetSnapShotId int
	declare @FlowDetLocTo varchar(50)
	declare @FlowDetRowId int
	declare @MaxFlowDetRowId int
	
	create table #tempFlowDetExtraDmdSource
	(
		FlowDetSnapShotId int,
		Location varchar(50)
	)

	select @FlowDetRowId = MIN(RowId), @MaxFlowDetRowId = MAX(RowId) from #tempExtraDmdSource
	while (@FlowDetRowId <= @MaxFlowDetRowId)
	begin
		select @FlowDetSnapShotId = FlowDetSnapShotId, @FlowDetLocTo = FlowDetLocTo from #tempExtraDmdSource where RowId = @FlowDetRowId
		select @ExtraDmdSource = ExtraDmdSource from LE_FlowDetSnapShot where Id = @FlowDetSnapShotId
		
		if ISNULL(@ExtraDmdSource, '') <> ''
		begin
			if (charindex(@SplitSymbol1, @ExtraDmdSource) <> 0)
				begin
				--循环其它需求源插入缓存表中
				while(charindex(@SplitSymbol1, @ExtraDmdSource) <> 0)
				begin
					insert #tempFlowDetExtraDmdSource(FlowDetSnapShotId, Location) values (@FlowDetSnapShotId, substring(@ExtraDmdSource, 1, charindex(@SplitSymbol1, @ExtraDmdSource) - 1))
					set @ExtraDmdSource = stuff(@ExtraDmdSource, 1, charindex(@SplitSymbol1, @ExtraDmdSource), ' ')
				end
			end
			else if (charindex(@SplitSymbol2, @ExtraDmdSource) <> 0)
			begin
				--循环其它需求源插入缓存表中
				while(charindex(@SplitSymbol2, @ExtraDmdSource) <> 0)
				begin
					insert #tempFlowDetExtraDmdSource(FlowDetSnapShotId, Location) values (@FlowDetSnapShotId, substring(@ExtraDmdSource, 1, charindex(@SplitSymbol2, @ExtraDmdSource) - 1))
					set @ExtraDmdSource = stuff(@ExtraDmdSource, 1, charindex(@SplitSymbol2, @ExtraDmdSource), ' ')
				end
			end
			
			insert #tempFlowDetExtraDmdSource(FlowDetSnapShotId, Location) values (@FlowDetSnapShotId, Ltrim(@ExtraDmdSource))
		end
		
		--删除和目的库位相同的其它需求源库位
		delete from #tempFlowDetExtraDmdSource where FlowDetSnapShotId = @FlowDetSnapShotId and Location = @FlowDetLocTo
		set @FlowDetRowId = @FlowDetRowId + 1
	end
	
	
	if exists(select top 1 1 from #tempFlowDetExtraDmdSource)
	begin
		insert into LE_FlowDetExtraDmdSourceSnapshot(FlowDetSnapShotId, Location) select distinct FlowDetSnapShotId, Location from #tempFlowDetExtraDmdSource
	end
	
	drop table #tempFlowDetExtraDmdSource
	-------------------↑计算其它需求源-----------------------
	
	
	--记录日志
	set @Msg = N'获取路线明细结束'
	insert into #temp_LOG_SnapshotFlowDet4LeanEngine(Lvl, Msg) values(0, @Msg)
	
	insert into LOG_SnapshotFlowDet4LeanEngine(Flow, Item, LocFrom, LocTo, OrderNo, Lvl, ErrorId, Msg, CreateDate, BatchNo) 
	select Flow, Item, LocFrom, LocTo, OrderNo, Lvl, ErrorId, Msg, CreateDate, @BatchNo from #temp_LOG_SnapshotFlowDet4LeanEngine
	
	drop table #temp_LOG_SnapshotFlowDet4LeanEngine
	drop table #tempExtraDmdSource
	drop table #tempOrderBom
	drop table #tempSortedMultiSupplierItem
	drop table #tempMultiSupplierItem
	drop table #tempMultiSupplierGroup
	drop table #tempFlowDet
	drop table #tempCompleteFlowDet
END
GO

