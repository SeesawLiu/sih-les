SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.objects WHERE type='P' AND name='USP_LE_ManualGenOrder')
	DROP PROCEDURE USP_LE_ManualGenOrder
GO

CREATE PROCEDURE [dbo].[USP_LE_ManualGenOrder]
(
	@ProdOrderNo varchar(50),
	@WindowTime datetime,
	@Priority tinyint,
	@CreateUserId int,
	@CreateUserNm varchar(50)
)
--WITH ENCRYPTION
AS
BEGIN
	SET NOCOUNT ON
	declare @DateTimeNow datetime = GetDate()
	declare @Msg nvarchar(Max)
	declare @trancount int
	
	Create table #tempMsg
	(
		Lvl tinyint,
		Msg varchar(max)
	)
	
	Create table #tempOrderBomDetTotal
	(
		Item varchar(50),
		Location varchar(50),
		OpRef varchar(50),
		OrderQty decimal(18, 8),
		ManufactureParty varchar(50)
	)
	
	Create table #tempCreatedOrderDet
	(
		Item varchar(50),
		LocTo varchar(50),
		OpRef varchar(50),
		OrderQty decimal(18, 8),
		ManufactureParty varchar(50)
	)
	
	Create table #tempOrderBomDetTBD
	(
		Item varchar(50),
		Location varchar(50),
		OpRef varchar(50),
		OrderQty decimal(18, 8),
		IsMatch bit,
		ManufactureParty varchar(50)
	)
	
	Create table #tempItemLocationTBD
	(
		Item varchar(50),
		Location varchar(50)
	)
	
	Create table #tempConsumedOpRefBalance
	(
		Item varchar(50),
		OpRef varchar(50),
		ConsumeQty decimal(18, 8),
	)
	
	Create table #tempThisConsumeOpRefBalance
	(
		Item varchar(50),
		OpRef varchar(50),
		ConsumeQty decimal(18, 8),
	)
	
	create table #tempPurchase
	(
		Flow varchar(50),
		Item varchar(50),
		Location varchar(50),
		Uom varchar(5),
		UC decimal(18, 8),
		MrpTotal decimal(18, 8),
		MrpTotalAdj decimal(18, 8),
		MrpWeight int,
	)
		
	create table #tempFlowDet
	(
		Id int identity(1, 1),
		Flow varchar(50),
		FlowType tinyint,
		FlowDetId int,
		--MSGId int,
		Item varchar(50),
		Uom varchar(5),
		UC decimal(18, 8),
		MinUC decimal(18, 8),
		UCDesc varchar(50),
		Container varchar(50),
		ContainerDesc varchar(50),
		LocFrom varchar(50),
		LocTo varchar(50),
		--OpRef varchar(50),
		MrpTotal decimal(18, 8),
		MrpTotalAdj decimal(18, 8),
		MrpWeight int,
		DeliveryCount int,
		DeliveryBalance int,
		--ReqQty decimal(18, 8),
		--OrderQty decimal(18, 8),
		RoundUpOpt tinyint,
		--OrderNo varchar(50),
		--OrderDetId int,
		--OrderDetSeq int,
		--ManufactureParty varchar(50)
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
	
	create table #tempOrderDet
	(
		RowId int identity(1, 1),
		Flow varchar(50),
		FlowDetId int,
		Item varchar(50),
		Uom varchar(5),
		UC decimal(18, 8),
		MinUC decimal(18, 8),
		UCDesc varchar(50),
		Container varchar(50),
		ContainerDesc varchar(50),
		LocFrom varchar(50),
		LocTo varchar(50),
		OpRef varchar(50),
		MrpTotal decimal(18, 8),
		MrpTotalAdj decimal(18, 8),
		MrpWeight int,
		DeliveryCount int,
		DeliveryBalance int,
		ReqQty decimal(18, 8),
		OrderQty decimal(18, 8),
		RoundUpOpt tinyint,
		OrderNo varchar(50),
		OrderDetId int,
		OrderDetSeq int,
		ManufactureParty varchar(50)
	)
	
	Create table #tempFlowMstr
	(
		RowId int identity(1, 1),
		Flow varchar(50),
		LocTo varchar(50)
	)
	
	Create table #tempOrderDetId
	(
		RowId int,
		OrderDetId int,
		OrderDetSeq int
	)
	
	declare @ProdOrderStartTime datetime
	declare @DAUAT varchar(50)
	declare @OrderStrategy tinyint
	select @ProdOrderStartTime = StartTime, @DAUAT = ShipFromTel from ORD_OrderMstr_4 where OrderNo = @ProdOrderNo
	
	--Z901	10	整车生产订单
	--Z902	10	MTS生产订单
	--Z903	10	备品备件生产订单
	--Z904	10	CKD生产订单
	--Z90R	10	返工生产订单
	--ZP01	10	研发车生产订单 
	--ZP02	10	试制车生产订单 
	if (@DAUAT = 'Z902')
	begin
		--自制件
		set @OrderStrategy = 12
	end
	else if (@DAUAT = 'Z903')
	begin
		--备品备件
		set @OrderStrategy = 9
	end
	else if (@DAUAT = 'Z904')
	begin
		--CKD
		set @OrderStrategy = 11
	end
	else if (@DAUAT = 'Z90R')
	begin
		--返工
		set @OrderStrategy = 10
	end
	else if (@DAUAT = 'ZP01' or @DAUAT = 'ZP02')
	begin
		--试制
		set @OrderStrategy = 8
	end
	else
	begin
		--手工
		set @OrderStrategy = 1
	end
	-------------------↓获取订单Bom用量-----------------------
	insert into #tempOrderBomDetTotal(Item, Location, OpRef, OrderQty, ManufactureParty)
	select bom.Item, bom.Location, ISNULL(bom.OpRef, ''), SUM(bom.OrderQty) as OrderQty, bom.ManufactureParty 
	from ORD_OrderBomDet as bom
	left join CUST_ManualGenOrderIgnoreWorkCenter as wc on bom.WorkCenter = wc.WorkCenter
	where bom.OrderNo = @ProdOrderNo and bom.OrderQty > 0 and wc.WorkCenter is null
	group by bom.Item, bom.Location, ISNULL(bom.OpRef, ''), bom.ManufactureParty
	-------------------↑获取订单Bom用量-----------------------
	
	-------------------↓获取已创建的拉料单-----------------------
	insert into #tempCreatedOrderDet(Item, LocTo, OrderQty, OpRef, ManufactureParty)
	select det.Item, det.LocTo, SUM(det.OrderQty) as OrderQty, det.OpRef, det.ManufactureParty
	from (select det.Item, ISNULL(det.LocTo, mstr.LocTo) as LocTo, ISNULL(det.BinTo, '') as OpRef, det.OrderQty, det.ManufactureParty
			from CUST_ManualGenOrderTrace as trace
			inner join ORD_OrderDet_1 as det on trace.OrderNo = det.OrderNo
			inner join ORD_OrderMstr_1 as mstr on det.OrderNo = mstr.OrderNo
			where ProdOrderNo = @ProdOrderNo and mstr.[Status] <> 5  --未取消
			union all
			select det.Item, ISNULL(det.LocTo, mstr.LocTo) as LocTo, ISNULL(det.BinTo, '') as OpRef, det.OrderQty, det.ManufactureParty 
			from CUST_ManualGenOrderTrace as trace
			inner join ORD_OrderDet_2 as det on trace.OrderNo = det.OrderNo
			inner join ORD_OrderMstr_2 as mstr on det.OrderNo = mstr.OrderNo
			where ProdOrderNo = @ProdOrderNo and mstr.[Status] <> 5  --未取消
			) as det
	group by det.Item, det.LocTo, det.OpRef, det.ManufactureParty
	-------------------↑获取已创建的拉料单-----------------------
	
	-------------------↓获取已占用的工位余量-----------------------
	insert into #tempConsumedOpRefBalance(Item, OpRef, ConsumeQty)
	select Item, OpRef, SUM(Qty) from CUST_ManualGenOrderOpRefBalanceTrace 
	where ProdOrderNo = @ProdOrderNo
	group by Item, OpRef
	-------------------↑获取已占用的工位余量-----------------------
	
	-------------------↓获取未创建的拉料需求-----------------------
	insert into #tempOrderBomDetTBD(Item, Location, OpRef, OrderQty, IsMatch, ManufactureParty)
	select bom.Item, bom.Location, bom.OpRef, bom.OrderQty - ISNULL(det.OrderQty, 0) - ISNULL(opRef.ConsumeQty, 0) as ReqQty, 0 as IsMatch, bom.ManufactureParty 
	from #tempOrderBomDetTotal as bom
	left join #tempCreatedOrderDet as det on bom.Item = det.Item and bom.Location = det.LocTo and bom.OpRef = det.OpRef
	and ((bom.ManufactureParty = det.ManufactureParty) or (ISNULL(bom.ManufactureParty, '') = '' and ISNULL(det.ManufactureParty, '') = ''))
	left join #tempConsumedOpRefBalance as opRef on bom.Item = OpRef.Item and bom.OpRef = opRef.OpRef and ISNULL(bom.ManufactureParty, '') = ''
	where (bom.OrderQty - ISNULL(det.OrderQty, 0) - ISNULL(opRef.ConsumeQty, 0)) > 0
	-------------------↑获取未创建的拉料需求-----------------------
	
	-------------------↓先考虑使用工位余量-----------------------
	insert into #tempThisConsumeOpRefBalance(Item, OpRef, ConsumeQty)
	select opRef.Item, opRef.OpRef, CASE WHEN tbd.OrderQty >= opRef.Qty THEN opRef.Qty ELSE OrderQty END
	from SCM_OpRefBalance as opRef
	inner join #tempOrderBomDetTBD as tbd on opRef.Item = tbd.Item and opRef.OpRef = tbd.OpRef
	where opRef.Qty > 0 and ISNULL(tbd.OpRef, '') <> '' and ISNULL(tbd.ManufactureParty, '') = ''
										
	if exists(select top 1 1 from #tempThisConsumeOpRefBalance)
	begin										
		update tbd set OrderQty = OrderQty - cOpRef.ConsumeQty
		from #tempOrderBomDetTBD as tbd 
		inner join #tempThisConsumeOpRefBalance as cOpRef on tbd.Item = cOpRef.Item and tbd.OpRef = cOpRef.OpRef 
		where ISNULL(tbd.OpRef, '') <> '' and ISNULL(tbd.ManufactureParty, '') = ''
		
		delete from #tempOrderBomDetTBD where OrderQty = 0										
												
		set @trancount = @@trancount
		begin try
			if @trancount = 0
			begin
				begin tran
			end
					
			--更新工位余量
			update opRef set Qty = opRef.Qty - cOpRef.ConsumeQty, LastModifyDate = @DateTimeNow, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm, [Version] = [Version] + 1
			from SCM_OpRefBalance as opRef
			inner join #tempThisConsumeOpRefBalance as cOpRef on opRef.Item = cOpRef.Item and opRef.OpRef = cOpRef.OpRef
			
			--更新工位余量日志
			insert into LOG_OpRefBalanceChange(Item, OpRef, Qty, [Status], [Version], CreateDate, CreateUserId, CreateUserNm)
			select opRef.Item, opRef.OpRef, opRef.Qty, 1, opRef.[Version], @DateTimeNow, @CreateUserId, @CreateUserNm
			from SCM_OpRefBalance as opRef
			inner join #tempThisConsumeOpRefBalance as cOpRef on opRef.Item = cOpRef.Item and opRef.OpRef = cOpRef.OpRef 
			
			--记录工位余量的消耗
			insert into CUST_ManualGenOrderOpRefBalanceTrace(ProdOrderNo, Item, OpRef, Qty, CreateUser, CreateUserNm, CreateDate)
			select @ProdOrderNo, Item, OpRef, ConsumeQty, @CreateUserId, @CreateUserNm, @DateTimeNow from #tempThisConsumeOpRefBalance
			
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
			
			set @Msg = N'更新工位余量失败，失败信息：' + Error_Message()
			insert into #tempMsg(Lvl, Msg) values(0, @Msg)
		end catch
	end
	-------------------↑先考虑使用工位余量-----------------------
	
	
	if exists(select top 1 1 from #tempOrderBomDetTBD)
	begin				
		-------------------↓过滤掉自动拉动的物料-----------------------
		--delete bom
		--from #tempOrderBomDetTBD as bom 
		--inner join SCM_FlowDet as det on bom.Item = det.Item
		--inner join SCM_FlowMstr as mstr	on det.Flow = mstr.Code
		--inner join SCM_FlowStrategy as stra on mstr.Code = stra.Flow
		--where ((det.LocTo is not null and bom.Location = det.LocTo) or (det.LocTo is null and bom.Location = mstr.LocTo))
		--and mstr.IsActive = 1 and det.IsActive = 1 and det.IsAutoCreate = 1 and mstr.IsAutoCreate = 1 and stra.Strategy <> 4 --不能过滤掉排序件
		-------------------↑过滤掉自动拉动的物料-----------------------
		
		-------------------↓过滤掉手工KB和电子看板的物料-----------------------
		delete req
		from #tempOrderBomDetTBD as req 
		inner join SCM_FlowDet as det on req.Item = det.Item
		inner join SCM_FlowMstr as mstr	on det.Flow = mstr.Code and req.Location = mstr.LocTo
		inner join SCM_FlowStrategy as stra	on stra.Flow = mstr.Code
		--where ((det.LocTo is not null and req.Location = det.LocTo) or (det.LocTo is null and req.Location = mstr.LocTo))
		where mstr.IsActive = 1 and stra.Strategy in (2, 7) --and det.IsActive = 1
		
		--过滤引用路线
		delete req
		from #tempOrderBomDetTBD as req 
		inner join SCM_FlowDet as det on req.Item = det.Item
		inner join SCM_FlowMstr as mstr	on det.Flow = mstr.RefFlow and req.Location = mstr.LocTo
		inner join SCM_FlowStrategy as stra	on stra.Flow = mstr.Code
		--where ((det.LocTo is not null and req.Location = det.LocTo) or (det.LocTo is null and req.Location = mstr.LocTo))
		where mstr.IsActive = 1 and stra.Strategy in (2, 7) --and det.IsActive = 1
		-------------------↑过滤掉手工KB和电子看板的物料-----------------------
	
		-------------------↓缓存待拉料的物料和库位-----------------------
		insert into #tempItemLocationTBD(Item, Location)
		select Item, Location from #tempOrderBomDetTBD group by Item, Location
		-------------------↑缓存待拉料的物料和库位-----------------------
		
		-------------------↓获取物流路线明细-----------------------
		insert into #tempFlowDet(Flow, FlowType, FlowDetId,
		Item, Uom, UC, MinUC, UCDesc, Container, ContainerDesc, LocFrom, LocTo,
		MrpTotal, MrpTotalAdj, MrpWeight, RoundUpOpt)
		select det.Flow, mstr.[Type], det.Id, 
		det.Item, det.Uom, det.UC, det.MinUC, det.UCDesc, det.Container, det.ContainerDesc, mstr.LocFrom, mstr.LocTo, 
		ISNULL(det.MrpTotal, 0), ISNULL(det.MrpTotalAdj, 0), ISNULL(det.MrpWeight, 0), det.RoundUpOpt
		from #tempItemLocationTBD as tbd 
		inner join SCM_FlowDet as det on tbd.Item = det.Item
		inner join SCM_FlowMstr as mstr	on det.Flow = mstr.Code --and tbd.Location = mstr.LocTo
		inner join SCM_FlowStrategy as stra	on stra.Flow = mstr.Code
		where mstr.IsActive = 1 --and det.IsActive = 1
		and mstr.[Type] in (1, 2)
		--and mstr.IsActive = 1 and (det.IsAutoCreate = 0 or mstr.IsAutoCreate = 0 or stra.Strategy = 4) and det.IsActive = 1
		
		--获取引用路线明细
		insert into #tempFlowDet(Flow, FlowType, FlowDetId,
		Item, Uom, UC, MinUC, UCDesc, Container, ContainerDesc, LocFrom, LocTo,
		MrpTotal, MrpTotalAdj, MrpWeight, RoundUpOpt)
		select mstr.Code, mstr.[Type], rdet.Id,
		rdet.Item, rdet.Uom, rdet.UC, rdet.MinUC, rdet.UCDesc, rdet.Container, rdet.ContainerDesc, mstr.LocFrom, mstr.LocTo, 
		0 as MrpTotal, 0 as MrpTotalAdj, 0 as MrpWeight, rdet.RoundUpOpt
		from #tempItemLocationTBD as tbd 
		inner join SCM_FlowDet as rdet on tbd.Item = rdet.Item
		inner join SCM_FlowMstr as rmstr on rdet.Flow = rmstr.Code
		inner join SCM_FlowMstr as mstr	on rmstr.Code = mstr.RefFlow --and tbd.Location = mstr.LocTo
		inner join SCM_FlowStrategy as stra	on stra.Flow = mstr.Code
		where rdet.Item = tbd.Item
		--and mstr.IsActive = 1 and (mstr.IsAutoCreate = 0 or stra.Strategy = 4) and rdet.IsActive = 1
		and mstr.IsActive = 1 --and rdet.IsActive = 1
		and mstr.[Type] in (1, 2)
		and not exists(select top 1 1 from #tempFlowDet as det where det.Item = rdet.Item and det.LocTo = mstr.LocTo)
		-------------------↑获取物流路线明细-----------------------
		
		-------------------↓先用配额选择多供应商供货的路线-----------------------
		delete det
		from #tempFlowDet as det 
		inner join SCM_FlowMstr as mstr on det.Flow = mstr.Code
		inner join (select det.Id, det.Item, det.LocTo 
					from #tempFlowDet as det
					inner join SCM_FlowMstr as mstr on det.Flow = mstr.Code
					inner join LE_QuotaSnapShot as lq on det.Item = lq.Item and mstr.PartyFrom = lq.Supplier
					where mstr.[Type] = 1) as cDet --当前供货的采购路线明细
					on det.Item = cDet.Item and det.LocTo = cDet.LocTo and det.Id <> cDet.Id
		where mstr.[Type] = 1   --只过滤掉采购的路线
		-------------------↑先用配额选择多供应商供货的路线-----------------------
		
		-------------------↓第一次匹配，匹配零件、库位和物流路线相同的物料-----------------------
		--更新需求的匹配标示
		update bom set IsMatch = 1
		from #tempOrderBomDetTBD as bom inner join #tempFlowDet as det on bom.Item = det.Item and bom.Location = det.LocTo
		-------------------↑第一次匹配，匹配零件、库位和物流路线相同的物料-----------------------
	
		-------------------↓第二次匹配，匹配采购入库地点是源库位-----------------------
		insert into #tempFlowDet(Flow, FlowType, FlowDetId,
		Item, Uom, UC, MinUC, UCDesc, Container, ContainerDesc, LocFrom, LocTo,
		MrpTotal, MrpTotalAdj, MrpWeight, RoundUpOpt)
		select distinct null, 2, null as FlowDetId,
		det.Item, i.Uom, i.MinUC, i.MinUC, null, i.Container, i.ContainerDesc, det.LocTo as LocFrom, bom.Location as LocTo,
		0, 0, 0, 0
		from #tempOrderBomDetTBD as bom
		inner join #tempFlowDet as det on bom.Item = det.Item
		inner join MD_Location as l on det.LocTo = l.Code
		inner join MD_Item as i on det.Item = i.Code
		where bom.IsMatch = 0 and l.IsSource = 1 and det.FlowType = 1
		
		--找路线头
		update #tempFlowDet set Flow = mstr.Code
		from #tempFlowDet as det
		inner join SCM_FlowMstr as mstr on mstr.LocFrom = det.LocFrom and mstr.LocTo = det.LocTo
		inner join SCM_FlowStrategy as stra on mstr.Code = stra.Flow
		where mstr.IsActive = 1 and det.Flow is null and stra.Strategy = 3
		
		update #tempFlowDet set Flow = mstr.Code
		from #tempFlowDet as det
		inner join SCM_FlowMstr as mstr on mstr.LocFrom = det.LocFrom and mstr.LocTo = det.LocTo
		inner join SCM_FlowStrategy as stra on mstr.Code = stra.Flow
		where mstr.IsActive = 1 and det.Flow is null and stra.Strategy <> 3
		
		if exists (select top 1 1 from #tempFlowDet where Flow is null)
		begin
			--记录日志
			insert into #tempMsg(Lvl,Msg)
			select 1, N'零件' + Item + N'没有维护从库位' + LocFrom + '到库位' + LocTo + '的移库路线，无法创建拉料单。'
			from #tempFlowDet where Flow is null
			
			update bom set IsMatch = 1
			from #tempOrderBomDetTBD as bom inner join #tempFlowDet as det on bom.Item = det.Item and bom.Location = det.LocTo
			where bom.IsMatch = 0 and det.Flow is null
		
			delete from #tempFlowDet where Flow is null
		end

		update bom set IsMatch = 1
		from #tempOrderBomDetTBD as bom inner join #tempFlowDet as det on bom.Item = det.Item and bom.Location = det.LocTo
		where bom.IsMatch = 0
		-------------------↑第二次匹配，匹配采购入库地点是源库位-----------------------
	
		-------------------↓第三次匹配，匹配直供-----------------------
		insert into #tempFlowDet(Flow, FlowType, FlowDetId,
		Item, Uom, UC, MinUC, UCDesc, Container, ContainerDesc, LocFrom, LocTo,
		MrpTotal, MrpTotalAdj, MrpWeight, RoundUpOpt)
		select distinct det.Flow, 1, null as FlowDetId,
		det.Item, det.Uom, det.UC, det.MinUC, det.UCDesc, det.Container, det.ContainerDesc, null as LocFrom, bom.Location as LocTo,
		0, 0, 0, 0
		from #tempOrderBomDetTBD as bom
		inner join #tempFlowDet as det on bom.Item = det.Item
		where bom.IsMatch = 0 and det.FlowType = 1
		
		update bom set IsMatch = 1
		from #tempOrderBomDetTBD as bom inner join #tempFlowDet as det on bom.Item = det.Item and bom.Location = det.LocTo
		where bom.IsMatch = 0
		-------------------↑第三次匹配，匹配直供-----------------------
	
		-------------------↓没有找到采购入库地点-----------------------
		if exists (select top 1 1 from #tempOrderBomDetTBD where IsMatch = 0)
		begin
			--记录日志
			insert into #tempMsg(Lvl,Msg)
			select 1, N'零件' + Item + N'消耗库位' + Location + N'没有拉料路线也没有采购入库地点，无法创建拉料单。'
			from #tempOrderBomDetTBD where IsMatch = 0
		end
		-------------------↑没有找到采购入库地点-----------------------
		
		-------------------↓按供货比例处理零件和库位相同的路线-----------------------
		-------------------↓按零件和目的库位分组-----------------------
		insert into #tempMultiSupplierGroup(Item, LocTo)
		select Item, LocTo from #tempFlowDet
		where LocTo is not null
		group by Item, LocTo having COUNT(1) > 1 
		
		insert into #tempMultiSupplierItem(FlowDetRowId, Flow, Item, LocTo, MSGRowId, MrpTotal, MrpTotalAdj, MrpWeight)
		select det.Id, det.Flow, det.Item, det.LocTo, msg.RowId, det.MrpTotal, det.MrpTotalAdj, det.MrpWeight
		from #tempFlowDet as det
		inner join #tempMultiSupplierGroup as msg on det.Item = msg.Item and det.LocTo = msg.LocTo
		-------------------↑按零件和目的库位分组-----------------------		
		
		-------------------↓零件和目的库位相同的路线都没有设置供货循环量，按零件包装设置循环量-----------------------
		update tmp set MrpWeight = (CASE WHEN det.UC = 0 THEN 1 ELSE det.UC END)
		from #tempMultiSupplierItem as tmp
		inner join #tempFlowDet as det on det.Id = tmp.FlowDetRowId
		where tmp.MSGRowId in (select MSGRowId from #tempMultiSupplierItem group by MSGRowId having SUM(MrpWeight) = 0)
		-------------------↑零件和目的库位相同的路线都没有设置供货循环量，按零件包装设置循环量-----------------------
		
		-------------------↓零件和目的库位相同的路线没有设置供货循环量，忽略这些路线明细-----------------------
		delete #tempFlowDet where Id in (select FlowDetRowId from #tempMultiSupplierItem where MrpWeight = 0)
		delete #tempMultiSupplierItem where FlowDetRowId in (select FlowDetRowId from #tempMultiSupplierItem where MrpWeight = 0)
		-------------------↑零件和目的库位相同的路线没有设置供货循环量，忽略这些路线明细-----------------------
		
		-------------------↓计算供货次数和余量-----------------------
		update #tempMultiSupplierItem set DeliveryCount = (ISNULL(MrpTotal, 0) + ISNULL(MrpTotalAdj, 0)) / MrpWeight, DeliveryBalance = (ISNULL(MrpTotal, 0) + ISNULL(MrpTotalAdj, 0)) % MrpWeight
		-------------------↑计算供货次数和余量-----------------------
		
		-------------------↓根据供货次数、循环量选取一条路线明细供货-----------------------
		insert into #tempSortedMultiSupplierItem(GID, FlowDetRowId, Flow)
		select ROW_NUMBER() over(partition by MSGRowId order by DeliveryCount asc, MrpWeight desc, DeliveryBalance desc) as GID, FlowDetRowId, Flow 
		from #tempMultiSupplierItem
		
		delete #tempFlowDet where Id in (select FlowDetRowId from #tempSortedMultiSupplierItem where GID <> 1)
		delete #tempMultiSupplierItem where FlowDetRowId in (select FlowDetRowId from #tempSortedMultiSupplierItem where GID <> 1)
		-------------------↑根据供货次数、循环量选取一条路线明细供货-----------------------
		
		-------------------↓创建要货单-----------------------
		insert into #tempOrderDet(Flow, FlowDetId,
		Item, Uom, UC, MinUC, UCDesc, Container, ContainerDesc, LocFrom, LocTo,
		ReqQty, OrderQty, OpRef, MrpTotal, MrpTotalAdj, MrpWeight, ManufactureParty, RoundUpOpt)
		select det.Flow, det.FlowDetId, 
		det.Item, det.Uom, det.UC, det.MinUC, det.UCDesc, det.Container, det.ContainerDesc, det.LocFrom, det.LocTo,
		0, tbd.OrderQty, tbd.OpRef, det.MrpTotal, det.MrpTotalAdj, det.MrpWeight, tbd.ManufactureParty, det.RoundUpOpt
		from #tempFlowDet as det inner join #tempOrderBomDetTBD as tbd on det.Item = tbd.Item and det.LocTo = tbd.Location
		
		insert into #tempFlowMstr(Flow, LocTo) select distinct Flow, LocTo from #tempOrderDet
		
		declare @FlowRowId int
		declare @MaxFlowRowId int
		
		select @FlowRowId = MIN(RowId), @MaxFlowRowId = MAX(RowId) from #tempFlowMstr
		while (@FlowRowId <= @MaxFlowRowId)
		begin
			set @trancount = @@trancount
			begin try
				declare @Flow varchar(50) = null
				declare @FlowType tinyint = null
				declare @FlowPartyFrom varchar(50) = null
				declare @FlowPartyTo varchar(50) = null
				declare @FlowLocFrom varchar(50) = null
				declare @FlowLocTo varchar(50) = null
				declare @FlowDock varchar(50) = null
				declare @OrderNo varchar(50) = null
				declare @OrderDetCount int = null
				declare @BeginOrderDetId bigint = null
				declare @EndOrderDetId bigint = null
				
				if @trancount = 0
				begin
					begin tran
				end
				
				--获取路线信息
				select @Flow = Code, @FlowType = [Type], @FlowPartyFrom = PartyFrom, @FlowPartyTo = PartyTo, @FlowLocFrom = LocFrom, @FlowLocTo = LocTo, @FlowDock = Dock
				from SCM_FlowMstr where Code = (select Flow from #tempFlowMstr where RowId = @FlowRowId)
				
				--从明细上重新获取物料消耗地点，如果是第三次匹配，直接从供应商拉动到物料消耗地点，不能取路线头上的目的库位
				select @FlowLocTo = LocTo from #tempFlowMstr where RowId = @FlowRowId
				
				--获取订单号
				exec USP_GetDocNo_ORD @Flow, 3, @FlowType, 0, 0, 1, @FlowPartyFrom, @FlowPartyTo, @FlowLocTo, @FlowLocFrom, @FlowDock, 0, @OrderNo output
				update #tempOrderDet set OrderNo = @OrderNo where Flow = @Flow and LocTo = @FlowLocTo
				
				--创建订单头
				if (@FlowType = 1)
				begin
					insert into ORD_OrderMstr_1 (
					OrderNo,              --订单号
					Flow,                 --路线
					OrderStrategy,        --策略，
					RefOrderNo,           --参考订单号,记录生产单号
					[Type],               --类型，1 采购
					SubType,              --子类型，0正常
					QualityType,          --质量状态，0良品
					StartTime,            --开始时间
					WindowTime,           --窗口时间
					PauseSeq,             --暂停工序，0
					IsQuick,              --是否快速，0
					[Priority],           --优先级，1
					[Status],             --状态，1释放
					PartyFrom,            --区域代码
					PartyFromNm,            --区域名称
					PartyTo,              --区域代码
					PartyToNm,            --区域名称
					--LocFrom,              --原材料库位
					--LocFromNm,            --原材料库位
					LocTo,                --成品库位
					LocToNm,              --成品库位
					IsInspect,            --下线检验，0
					BillAddr,			  --开票地址
					Dock,				  --道口
					IsAutoRelease,        --自动释放，0
					IsAutoStart,          --自动上线，0
					IsAutoShip,           --自动发货，0
					IsAutoReceive,        --自动收货，0
					IsAutoBill,           --自动账单，0
					IsManualCreateDet,    --手工创建明细，0
					IsListPrice,          --显示价格单，0
					IsPrintOrder,         --打印要货单，0
					IsOrderPrinted,       --要货单已打印，0
					IsPrintAsn,           --打印ASN，0
					IsPrintRec,           --打印收货单，0
					IsShipExceed,         --允许超发，0
					IsRecExceed,          --允许超收，0
					IsOrderFulfillUC,     --整包装下单，0
					IsShipFulfillUC,      --整包装发货，0
					IsRecFulfillUC,       --整包装收货，0
					IsShipScanHu,         --发货扫描条码，0
					IsRecScanHu,          --收货扫描条码，0
					IsCreatePL,           --创建拣货单，0
					IsPLCreate,           --拣货单已创建，0
					IsShipFifo,           --发货先进先出，0
					IsRecFifo,            --收货先进先出，0
					IsShipByOrder,        --允许按订单发货，0
					IsOpenOrder,          --开口订单，0
					IsAsnUniqueRec,       --ASN一次性收货，0
					RecGapTo,             --收货差异处理，0
					RecTemplate,		  --收货单模板
					OrderTemplate,		  --订单模板
					AsnTemplate,		  --送货单模板
					HuTemplate,			  --条码模板
					BillTerm,             --结算方式，0
					CreateHuOpt,          --创建条码选项，0
					ReCalculatePriceOpt,  --重新计算价格单选项，0
					CreateUser,           --创建用户
					CreateUserNm,         --创建用户名称
					CreateDate,           --创建日期
					LastModifyUser,       --最后修改用户
					LastModifyUserNm,     --最后修改用户名称
					LastModifyDate,       --最后修改日期
					ReleaseUser,          --释放用户
					ReleaseUserNm,        --释放用户名称
					ReleaseDate,          --释放日期
					[Version],            --版本，1
					FlowDesc,			  --路线描述
					ProdLineType,         --生产线类型
					PauseStatus			  --暂停状态，0
					)
					select 
					@OrderNo,			  --订单号
					mstr.Code,            --路线
					@OrderStrategy,		  --策略
					@ProdOrderNo,          --参考订单号，生产单号
					@FlowType,			  --类型
					0,					  --子类型，0正常
					0,					  --质量状态，0良品
					@DateTimeNow,         --开始时间
					@WindowTime,          --窗口时间
					0,					  --暂停工序，0
					0,					  --是否快速，0
					@Priority,			  --优先级，1
					1,					  --状态，1释放
					mstr.PartyFrom,            --区域代码
					pf.Name,            --区域名称
					mstr.PartyTo,              --区域代码
					pt.Name,            --区域名称
					--mstr.LocFrom,              --原材料库位
					--lf.Name,				--原材料库位
					@FlowLocTo,                --成品库位
					lt.Name,                --成品库位
					mstr.IsInspect,            --下线检验，0
					mstr.BillAddr,			  --开票地址
					mstr.Dock,				  --道口
					mstr.IsAutoRelease,        --自动释放，0
					mstr.IsAutoStart,          --自动上线，0
					mstr.IsAutoShip,           --自动发货，0
					mstr.IsAutoReceive,        --自动收货，0
					mstr.IsAutoBill,           --自动账单，0
					mstr.IsManualCreateDet,    --手工创建明细，0
					mstr.IsListPrice,          --显示价格单，0
					mstr.IsPrintOrder,         --打印要货单，0
					0,					  --要货单已打印，0
					mstr.IsPrintAsn,           --打印ASN，0
					mstr.IsPrintRcpt,          --打印收货单，0
					mstr.IsShipExceed,         --允许超发，0
					mstr.IsRecExceed,          --允许超收，0
					mstr.IsOrderFulfillUC,     --整包装下单，0
					mstr.IsShipFulfillUC,      --整包装发货，0
					mstr.IsRecFulfillUC,       --整包装收货，0
					mstr.IsShipScanHu,         --发货扫描条码，0
					mstr.IsRecScanHu,          --收货扫描条码，0
					mstr.IsCreatePL,           --创建拣货单，0
					0,					  --拣货单已创建，0
					mstr.IsShipFifo,           --发货先进先出，0
					mstr.IsRecFifo,            --收货先进先出，0
					mstr.IsShipByOrder,        --允许按订单发货，0
					0,					  --开口订单，0
					mstr.IsAsnUniqueRec,       --ASN一次性收货，0
					mstr.RecGapTo,             --收货差异处理，0
					mstr.RecTemplate,		  --收货单模板
					mstr.OrderTemplate,		  --订单模板
					mstr.AsnTemplate,		  --送货单模板
					mstr.HuTemplate,			  --条码模板
					mstr.BillTerm,             --结算方式，0
					mstr.CreateHuOpt,          --创建条码选项，0
					0,					  --重新计算价格单选项，0
					@CreateUserId,        --创建用户
					@CreateUserNm,        --创建用户名称
					@DateTimeNow,         --创建日期
					@CreateUserId,        --最后修改用户
					@CreateUserNm,        --最后修改用户名称
					@DateTimeNow,         --最后修改日期
					@CreateUserId,        --释放用户
					@CreateUserNm,        --释放用户名称
					@DateTimeNow,         --释放日期
					1,					  --版本，1
					mstr.Desc1,				  --路线描述
					mstr.ProdLineType,         --生产线类型
					0					  --暂停状态，0
					from SCM_FlowMstr as mstr
					inner join MD_Party as pf on mstr.PartyFrom = pf.Code
					inner join MD_Party as pt on mstr.PartyTo = pt.Code
					--left join MD_Location as lf on mstr.LocFrom = lf.Code						
					left join MD_Location as lt on lt.Code = @FlowLocTo					
					where mstr.Code = @Flow
				end
				else if @FlowType = 2
				begin 
					insert into ORD_OrderMstr_2 (
					OrderNo,              --订单号
					Flow,                 --路线
					OrderStrategy,        --策略，
					RefOrderNo,			  --参考订单号,生产单号
					[Type],               --类型，1 采购
					SubType,              --子类型，0正常
					QualityType,          --质量状态，0良品
					StartTime,            --开始时间
					WindowTime,           --窗口时间
					PauseSeq,             --暂停工序，0
					IsQuick,              --是否快速，0
					[Priority],           --优先级，1
					[Status],             --状态，1释放
					PartyFrom,            --区域代码
					PartyFromNm,            --区域名称
					PartyTo,              --区域代码
					PartyToNm,              --区域名称
					LocFrom,              --原材料库位
					LocFromNm,              --原材料库位
					LocTo,                --成品库位
					LocToNm,                --成品库位
					IsInspect,            --下线检验，0
					BillAddr,			  --开票地址
					Dock,				  --道口
					IsAutoRelease,        --自动释放，0
					IsAutoStart,          --自动上线，0
					IsAutoShip,           --自动发货，0
					IsAutoReceive,        --自动收货，0
					IsAutoBill,           --自动账单，0
					IsManualCreateDet,    --手工创建明细，0
					IsListPrice,          --显示价格单，0
					IsPrintOrder,         --打印要货单，0
					IsOrderPrinted,       --要货单已打印，0
					IsPrintAsn,           --打印ASN，0
					IsPrintRec,           --打印收货单，0
					IsShipExceed,         --允许超发，0
					IsRecExceed,          --允许超收，0
					IsOrderFulfillUC,     --整包装下单，0
					IsShipFulfillUC,      --整包装发货，0
					IsRecFulfillUC,       --整包装收货，0
					IsShipScanHu,         --发货扫描条码，0
					IsRecScanHu,          --收货扫描条码，0
					IsCreatePL,           --创建拣货单，0
					IsPLCreate,           --拣货单已创建，0
					IsShipFifo,           --发货先进先出，0
					IsRecFifo,            --收货先进先出，0
					IsShipByOrder,        --允许按订单发货，0
					IsOpenOrder,          --开口订单，0
					IsAsnUniqueRec,       --ASN一次性收货，0
					RecGapTo,             --收货差异处理，0
					RecTemplate,		  --收货单模板
					OrderTemplate,		  --订单模板
					AsnTemplate,		  --送货单模板
					HuTemplate,			  --条码模板
					BillTerm,             --结算方式，0
					CreateHuOpt,          --创建条码选项，0
					ReCalculatePriceOpt,  --重新计算价格单选项，0
					CreateUser,           --创建用户
					CreateUserNm,         --创建用户名称
					CreateDate,           --创建日期
					LastModifyUser,       --最后修改用户
					LastModifyUserNm,     --最后修改用户名称
					LastModifyDate,       --最后修改日期
					ReleaseUser,          --释放用户
					ReleaseUserNm,        --释放用户名称
					ReleaseDate,          --释放日期
					[Version],            --版本，1
					FlowDesc,			  --路线描述
					ProdLineType,         --生产线类型
					PauseStatus			  --暂停状态，0
					)
					select
					@OrderNo,			  --订单号
					mstr.Code,                 --路线
					@OrderStrategy,		 --策略
					@ProdOrderNo,          --参考订单号，生产单号
					@FlowType,			  --类型
					0,					  --子类型，0正常
					0,					  --质量状态，0良品
					@DateTimeNow,         --开始时间
					@WindowTime,        --窗口时间
					0,					  --暂停工序，0
					0,					  --是否快速，0
					@Priority,					  --优先级，1
					1,					  --状态，1释放
					mstr.PartyFrom,            --区域代码
					pf.Name,            --区域代码
					mstr.PartyTo,              --区域代码
					pt.Name,            --区域代码
					mstr.LocFrom,              --原材料库位
					lf.Name,              --原材料库位
					@FlowLocTo,                --成品库位
					lt.Name,              --成品库位
					mstr.IsInspect,            --下线检验，0
					mstr.BillAddr,			  --开票地址
					mstr.Dock,				  --道口
					mstr.IsAutoRelease,        --自动释放，0
					mstr.IsAutoStart,          --自动上线，0
					mstr.IsAutoShip,           --自动发货，0
					mstr.IsAutoReceive,        --自动收货，0
					mstr.IsAutoBill,           --自动账单，0
					mstr.IsManualCreateDet,    --手工创建明细，0
					mstr.IsListPrice,          --显示价格单，0
					mstr.IsPrintOrder,         --打印要货单，0
					0,					  --要货单已打印，0
					mstr.IsPrintAsn,           --打印ASN，0
					mstr.IsPrintRcpt,          --打印收货单，0
					mstr.IsShipExceed,         --允许超发，0
					mstr.IsRecExceed,          --允许超收，0
					mstr.IsOrderFulfillUC,     --整包装下单，0
					mstr.IsShipFulfillUC,      --整包装发货，0
					mstr.IsRecFulfillUC,       --整包装收货，0
					mstr.IsShipScanHu,         --发货扫描条码，0
					mstr.IsRecScanHu,          --收货扫描条码，0
					mstr.IsCreatePL,           --创建拣货单，0
					0,					  --拣货单已创建，0
					mstr.IsShipFifo,           --发货先进先出，0
					mstr.IsRecFifo,            --收货先进先出，0
					mstr.IsShipByOrder,        --允许按订单发货，0
					0,					  --开口订单，0
					mstr.IsAsnUniqueRec,       --ASN一次性收货，0
					mstr.RecGapTo,             --收货差异处理，0
					mstr.RecTemplate,		  --收货单模板
					mstr.OrderTemplate,		  --订单模板
					mstr.AsnTemplate,		  --送货单模板
					mstr.HuTemplate,			  --条码模板
					mstr.BillTerm,             --结算方式，0
					mstr.CreateHuOpt,          --创建条码选项，0
					0,					  --重新计算价格单选项，0
					@CreateUserId,        --创建用户
					@CreateUserNm,        --创建用户名称
					@DateTimeNow,         --创建日期
					@CreateUserId,        --最后修改用户
					@CreateUserNm,        --最后修改用户名称
					@DateTimeNow,         --最后修改日期
					@CreateUserId,        --释放用户
					@CreateUserNm,        --释放用户名称
					@DateTimeNow,         --释放日期
					1,					  --版本，1
					mstr.Desc1,				  --路线描述
					mstr.ProdLineType,         --生产线类型
					0					  --暂停状态，0
					from SCM_FlowMstr  as mstr
					inner join MD_Party as pf on mstr.PartyFrom = pf.Code
					inner join MD_Party as pt on mstr.PartyTo = pt.Code
					left join MD_Location as lf on mstr.LocFrom = lf.Code					
					left join MD_Location as lt on lt.Code = @FlowLocTo			
					where mstr.Code = @Flow
				end
				else
				begin
					RAISERROR(N'路线类型不正确。', 16, 1)
				end
				
				--查找订单数量
				select @OrderDetCount = COUNT(*) from #tempOrderDet where OrderNo = @OrderNo
				--锁定OrderDetId字段标识字段
				exec USP_SYS_BatchGetNextId 'ORD_OrderDet', @OrderDetCount, @EndOrderDetId output
				--查找开始标识
				set @BeginOrderDetId = @EndOrderDetId - @OrderDetCount
				
				--缓存订单ID
				truncate table #tempOrderDetId
				insert into #tempOrderDetId(RowId, OrderDetId, OrderDetSeq)
				select RowId, ROW_NUMBER() over (order by RowId) + @BeginOrderDetId as OrderDetId, ROW_NUMBER() over (order by RowId) as OrderDetSeq
				from #tempOrderDet where OrderNo = @OrderNo
				
				--记录订单明细ID
				update det set OrderDetId = id.OrderDetId, OrderDetSeq = id.OrderDetSeq
				from #tempOrderDet as det inner join #tempOrderDetId as id on det.RowId = id.RowId
				where OrderNo = @OrderNo
				
				--按包装圆整
				update #tempOrderDet set OrderQty = ReqQty where OrderNo = @OrderNo and ReqQty > 0 and RoundUpOpt in (0, 2)  --不圆整
				update #tempOrderDet set OrderQty = ReqQty where OrderNo = @OrderNo and ReqQty > 0 and UC <= 0  --不圆整
				update #tempOrderDet set OrderQty = ceiling(ReqQty / UC) * UC where OrderNo = @OrderNo and ReqQty > 0 and RoundUpOpt = 1 and UC > 0 --向上圆整
				--update #tempOrderDet set OrderQty = floor(ReqQty / UC) * UC where OrderNo = @OrderNo and ReqQty > 0 and RoundUpOpt = 2 and UC > 0  --向下圆整
				
				--更新工位余量
				update orb set Qty = ISNULL(tod.Balance, 0), [Version] = [Version] + 1, LastModifyDate = @DateTimeNow, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm
				from SCM_OpRefBalance as orb 
				inner join (select Item, OpRef, SUM(OrderQty - ReqQty) as Balance from #tempOrderDet 
							where OrderQty > ReqQty and ISNULL(OpRef, '') <> '' group by Item, OpRef) as tod on orb.Item = tod.Item and orb.OpRef = tod.OpRef
									
				--更新工位余量日志
				insert into LOG_OpRefBalanceChange(Item, OpRef, Qty, [Status], [Version], CreateDate, CreateUserId, CreateUserNm)
				select orb.Item, orb.OpRef, ISNULL(tod.Balance, 0), 1, orb.[Version], @DateTimeNow, @CreateUserId, @CreateUserNm
				from SCM_OpRefBalance as orb 
				inner join (select Item, OpRef, SUM(OrderQty - ReqQty) as Balance from #tempOrderDet 
							where OrderQty > ReqQty and ISNULL(OpRef, '') <> '' group by Item, OpRef) as tod on orb.Item = tod.Item and orb.OpRef = tod.OpRef
				
				--新增不存在的工位余量
				insert into SCM_OpRefBalance (Item, OpRef, Qty, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, [Version])
				select tod.Item, tod.OpRef, ISNULL(tod.Balance, 0), @CreateUserId, @CreateUserNm, @DateTimeNow, @CreateUserId, @CreateUserNm, @DateTimeNow, 1
				from (select Item, OpRef, SUM(OrderQty - ReqQty) as Balance from #tempOrderDet 
						where OrderQty > ReqQty and ISNULL(OpRef, '') <> '' group by Item, OpRef) as tod 
				left join SCM_OpRefBalance as orb on orb.Item = tod.Item and orb.OpRef = tod.OpRef
				where orb.Id is null
				
				--新增工位余量日志
				insert into LOG_OpRefBalanceChange(Item, OpRef, Qty, [Status], [Version], CreateDate, CreateUserId, CreateUserNm)
				select tod.Item, tod.OpRef, ISNULL(tod.Balance, 0), 0, 1, @DateTimeNow, @CreateUserId, @CreateUserNm
				from (select Item, OpRef, SUM(OrderQty - ReqQty) as Balance from #tempOrderDet 
						where OrderQty > ReqQty and ISNULL(OpRef, '') <> '' group by Item, OpRef) as tod 
				left join SCM_OpRefBalance as orb on orb.Item = tod.Item and orb.OpRef = tod.OpRef
				where orb.Id is null
				
				--创建订单明细
				if @FlowType = 1
				begin
					insert into ORD_OrderDet_1 (
					Id,                         --生产单明细标识
					OrderNo,                    --生产单号
					OrderType,                  --类型，1采购
					OrderSubType,               --子类型，0正常
					Seq,						--行号，1
					ScheduleType,               --计划协议类型，0
					Item,                       --物料号
					RefItemCode,				--参考物料号
					ItemDesc,                   --物料描述
					Uom,                        --单位
					BaseUom,                    --基本单位
					UC,                         --包装，1
					MinUC,                      --最小包装，1
					UCDesc,						--包装描述
					Container,					--容器代码
					ContainerDesc,				--容器描述
					QualityType,                --质量状态，0
					ManufactureParty,           --制造商
					ReqQty,                     --需求数量，1
					OrderQty,                   --订单数量，1
					ShipQty,                    --发货数量，0
					RecQty,                     --收货数量，0
					RejQty,                     --次品数量，0
					ScrapQty,                   --废品数量，0
					PickQty,                    --拣货数量，0
					UnitQty,                    --单位用量，1
					IsInspect,                  --是否检验，0
					IsProvEst,                  --是否暂估价，0
					IsIncludeTax,               --是否含税价，0
					IsScanHu,                   --是否扫描条码，0
					CreateUser,                 --创建用户
					CreateUserNm,               --创建用户名称
					CreateDate,                 --创建日期
					LastModifyUser,             --最后修改用户
					LastModifyUserNm,           --最后修改用户名称
					LastModifyDate,             --最后修改日期
					[Version],					--版本，1
					IsChangeUC,					--是否修改单包装，0
					--LocFrom,					--出库库位
					--LocFromNm,				--出库库位名称
					LocTo,						--入库库位
					LocToNm,					--入库库位名称
					BillAddr,					--开票地址
					ExtraDmdSource,				--记录FlowDetId，如果是0代表系统自动创建的路线明细
					BillTerm,					--结算方式
					BinTo						--工位
					)
					select 
					det.OrderDetId,				--生产单明细标识
					det.OrderNo,				--生产单号
					@FlowType,					--类型，1采购
					0,							--子类型，0正常
					det.OrderDetSeq,			--行号，
					0,							--计划协议类型，0
					det.Item,					--物料号
					i.RefCode,					--参考物料号
					i.Desc1,					--物料描述
					det.Uom,					--单位
					det.Uom,					--基本单位
					det.UC,						--包装，1
					det.MinUC,                  --最小包装，1
					det.UCDesc,					--包装描述
					det.Container,				--容器代码
					det.ContainerDesc,			--容器描述
					0,							--质量状态，0
					det.ManufactureParty,		--制造商
					det.ReqQty,					--需求数量，1
					det.OrderQty,				--订单数量，1
					0,							--发货数量，0
					0,							--收货数量，0
					0,							--次品数量，0
					0,							--废品数量，0
					0,							--拣货数量，0
					1,							--单位用量，1
					0,							--是否检验，0
					0,							--是否暂估价，0
					0,							--是否含税价，0
					0,							--是否扫描条码，0
					@CreateUserId,				--创建用户
					@CreateUserNm,				--创建用户名称
					@DateTimeNow,				--创建日期
					@CreateUserId,				--最后修改用户
					@CreateUserNm,				--最后修改用户名称
					@DateTimeNow,				--最后修改日期
					1,							--版本，1
					0,							--是否修改单包装，0
					--det.LocFrom,				--出库库位
					--lf.Name,					--出库库位名称
					ISNULL(det.LocTo, mstr.LocTo),	--入库库位
					ISNULL(lt.Name, mstr.LocToNm),--入库库位
					mstr.BillAddr,				--开票地址
					CONVERT(varchar, det.FlowDetId),--记录FlowDetId，如果是0代表系统自动创建的路线明细
					mstr.BillTerm,				--结算方式
					det.OpRef					--工位
					from #tempOrderDet as det
					inner join ORD_OrderMstr_1 as mstr on mstr.OrderNo = det.OrderNo
					inner join MD_Item as i on det.Item = i.Code
					left join MD_Location as lt on det.LocTo = lt.Code
					where det.OrderNo = @OrderNo
					
					--更新累计交货量
					update q set AccumulateQty = ISNULL(q.AccumulateQty, 0) + det.OrderQty, [Version] = q.[Version] + 1
					from SCM_Quota as q 
					inner join (select det.Item, mstr.PartyFrom, SUM(det.OrderQty) as OrderQty
								from #tempOrderDet as det 
								inner join ORD_OrderMstr_1 as mstr on mstr.OrderNo = det.OrderNo
								where det.OrderNo = @OrderNo
								group by det.Item, mstr.PartyFrom) as det on q.Item = det.Item and q.Supplier = det.PartyFrom								
				end
				else
				begin
					insert into ORD_OrderDet_2 (
					Id,                         --生产单明细标识
					OrderNo,                    --生产单号
					OrderType,                  --类型，1采购
					OrderSubType,               --子类型，0正常
					Seq,						--行号，1
					ScheduleType,               --计划协议类型，0
					Item,                       --物料号
					RefItemCode,				--参考物料号
					ItemDesc,                   --物料描述
					Uom,                        --单位
					BaseUom,                    --基本单位
					UC,                         --包装，1
					MinUC,                      --最小包装，1
					UCDesc,						--包装描述
					Container,					--容器代码
					ContainerDesc,				--容器描述
					QualityType,                --质量状态，0
					ManufactureParty,           --制造商
					ReqQty,                     --需求数量，1
					OrderQty,                   --订单数量，1
					ShipQty,                    --发货数量，0
					RecQty,                     --收货数量，0
					RejQty,                     --次品数量，0
					ScrapQty,                   --废品数量，0
					PickQty,                    --拣货数量，0
					UnitQty,                    --单位用量，1
					IsInspect,                  --是否检验，0
					IsProvEst,                  --是否暂估价，0
					IsIncludeTax,               --是否含税价，0
					IsScanHu,                   --是否扫描条码，0
					CreateUser,                 --创建用户
					CreateUserNm,               --创建用户名称
					CreateDate,                 --创建日期
					LastModifyUser,             --最后修改用户
					LastModifyUserNm,           --最后修改用户名称
					LastModifyDate,             --最后修改日期
					[Version],					--版本，1
					IsChangeUC,					--是否修改单包装，0
					LocFrom,					--出库库位
					LocFromNm,					--出库库位名称
					LocTo,						--入库库位
					LocToNm,					--入库库位名称
					--BillAddr,					--开票地址
					ExtraDmdSource,				--记录FlowDetId，如果是0代表系统自动创建的路线明细
					BinTo						--工位
					)
					select 
					det.OrderDetId,				--生产单明细标识
					det.OrderNo,				--生产单号
					@FlowType,					--类型，1采购
					0,							--子类型，0正常
					det.OrderDetSeq,			--行号，
					0,							--计划协议类型，0
					det.Item,					--物料号
					i.RefCode,					--参考物料号
					i.Desc1,					--物料描述
					det.Uom,					--单位
					det.Uom,					--基本单位
					det.UC,						--包装，1
					det.MinUC,                  --最小包装，1
					det.UCDesc,					--包装描述
					det.Container,				--容器代码
					det.ContainerDesc,			--容器描述
					0,							--质量状态，0
					det.ManufactureParty,		--制造商
					det.ReqQty,					--需求数量，1
					det.OrderQty,				--订单数量，1
					0,							--发货数量，0
					0,							--收货数量，0
					0,							--次品数量，0
					0,							--废品数量，0
					0,							--拣货数量，0
					1,							--单位用量，1
					0,							--是否检验，0
					0,							--是否暂估价，0
					0,							--是否含税价，0
					0,							--是否扫描条码，0
					@CreateUserId,				--创建用户
					@CreateUserNm,				--创建用户名称
					@DateTimeNow,				--创建日期
					@CreateUserId,				--最后修改用户
					@CreateUserNm,				--最后修改用户名称
					@DateTimeNow,				--最后修改日期
					1,							--版本，1
					0,							--是否修改单包装，0
					ISNULL(det.LocFrom, mstr.LocFrom),--出库库位
					ISNULL(lf.Name, mstr.LocFromNm),--出库库位
					ISNULL(det.LocTo, mstr.LocTo),	--入库库位
					ISNULL(lt.Name, mstr.LocToNm),--入库库位
					--mstr.BillAddr,				--开票地址
					CONVERT(varchar, det.FlowDetId),--记录FlowDetId，如果是0代表系统自动创建的路线明细
					det.OpRef					--工位
					from #tempOrderDet as det
					inner join ORD_OrderMstr_2 as mstr on mstr.OrderNo = det.OrderNo
					inner join MD_Item as i on det.Item = i.Code
					left join MD_Location as lf on det.LocFrom = lf.Code
					left join MD_Location as lt on det.LocTo = lt.Code
					where det.OrderNo = @OrderNo
				end
								
				if @FlowType = 1
					begin  --记录创建计划协议的订单明细
						INSERT INTO SAP_CRSL(EINDT, FRBNR, LIFNR, MATNR, MENGE, SGTXT, WERKS, OrderDetId, [Status], ErrorCount, CreateDate, LastModifyDate)
						select CONVERT(varchar(8), @WindowTime, 112), det.OrderNo, mstr.PartyFrom, det.Item, det.OrderQty, det.OrderDetSeq, r.Plant, det.OrderDetId, 0, 0, @DateTimeNow, @DateTimeNow
						from #tempOrderDet as det
						inner join ORD_OrderMstr_1 as mstr on mstr.OrderNo = det.OrderNo
						inner join MD_Region as r on mstr.PartyTo = r.Code
						where det.OrderNo = @OrderNo
					end
					else
					begin
						if exists(select top 1 1 from SCM_FlowMstr where Code = @Flow and PartyFrom in ('LOC','SQC'))
						begin
							--记录FIS_CreateProcurementOrderDAT
							INSERT INTO FIS_CreateProcurementOrderDAT(OrderNo, Van, OrderStrategy, StartTime, WindowTime, [Priority], 
							Sequence, PartyFrom, PartyTo, Dock, CreateDate, Flow, LineSeq, Item, ManufactureParty, 
							LocationTo, Bin, OrderedQty, IsShipExceed, [FileName], IsCreateDat)
							select det.OrderNo, '', mstr.OrderStrategy, mstr.StartTime, mstr.WindowTime, mstr.[Priority],
							'', ISNULL(det.LocFrom, mstr.LocFrom), ISNULL(det.LocTo, mstr.LocTo), mstr.Dock, mstr.CreateDate, mstr.Flow, det.OrderDetId, det.Item, det.ManufactureParty, 
							det.OpRef, '', det.OrderQty, mstr.IsShipExceed, '', 0
							from #tempOrderDet as det
							inner join ORD_OrderMstr_2 as mstr on mstr.OrderNo = det.OrderNo
							where det.OrderNo = @OrderNo
						end
						/**else if exists(select top 1 1 from SCM_FlowMstr where Code = @Flow and PartyFrom in ('SQC'))
						begin
							--记录FIS_CreateOrderDAT
							INSERT INTO FIS_CreateOrderDAT(OrderNo, MATNR, LIFNR, ENMNG, CHARG, COLOR, TIME_STAMP, CY_SEQNR,
							TIME_STAMP1, AUFNR, LGORT, UMLGO, LGPBE, REQ_TIME_STAMP, FLG_SORT, PLNBEZ, KTEXT, ZPLISTNO, 
							ErrorCount, IsCreateDat, CreateUserNm)
							select det.OrderNo, det.Item, det.ManufactureParty, det.OrderQty, '', '', REPLACE(REPLACE(REPLACE(CONVERT(varchar(50),mstr.CreateDate,120),':',''),'-',''),' ',''), '',
							null, '', ISNULL(det.LocFrom, mstr.LocFrom), ISNULL(det.LocTo, mstr.LocTo), '', REPLACE(REPLACE(REPLACE(CONVERT(varchar(50),mstr.WindowTime,120),':',''),'-',''),' ',''), 'N', '', '', det.OrderDetId,
							0, 0, @CreateUserNm
							from #tempOrderDet as det
							inner join ORD_OrderMstr_2 as mstr on mstr.OrderNo = det.OrderNo
							where det.OrderNo = @OrderNo
						end**/
					end
				
				--记录已经生成的拉料单
				insert into CUST_ManualGenOrderTrace(ProdOrderNo, OrderNo) values(@ProdOrderNo, @OrderNo)
					
				set @Msg = N'成功创建物流路线' + @Flow + N'的拉料单' + @OrderNo + N'。'
				insert into #tempMsg(Lvl, Msg) values(0, @Msg)
							
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
				
				set @Msg = N'创建物流路线' + @Flow + N'的拉料单失败，失败信息：' + Error_Message()
				insert into #tempMsg(Lvl, Msg) values(0, @Msg)
			end catch
			
			set @FlowRowId = @FlowRowId + 1
		end
		-------------------↑创建要货单-----------------------
		

	end
	
	select Lvl, Msg from #tempMsg
	
	drop table #tempConsumedOpRefBalance
	drop table #tempThisConsumeOpRefBalance
	drop table #tempFlowMstr
	drop table #tempOrderDetId
	drop table #tempOrderDet
	drop table #tempPurchase
	drop table #tempItemLocationTBD
	drop table #tempOrderBomDetTBD
	drop table #tempCreatedOrderDet
	drop table #tempOrderBomDetTotal
	drop table #tempMsg
	drop table #tempMultiSupplierGroup
	drop table #tempMultiSupplierItem
	drop table #tempSortedMultiSupplierItem
END

