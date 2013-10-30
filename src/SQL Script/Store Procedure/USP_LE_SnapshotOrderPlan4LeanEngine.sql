SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.objects WHERE type='P' AND name='USP_LE_SnapshotOrderPlan4LeanEngine')
	DROP PROCEDURE USP_LE_SnapshotOrderPlan4LeanEngine
GO

CREATE PROCEDURE [dbo].[USP_LE_SnapshotOrderPlan4LeanEngine] 
(
	@BatchNo int
)
--WITH ENCRYPTION
AS
BEGIN
	SET NOCOUNT ON
	declare @Msg nvarchar(Max)
	set @Msg = N'快照OrderPlan开始'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	
	--缓存未下线的整车生产单号，为了避免未反冲的需求和未下线的需求重复
	select OrderNo into #tempVanOrderNo from LE_OrderBomCPTimeSnapshot group by OrderNo

	create table #tempBomPlan
	(
		Id int identity(1, 1) Primary Key,
		LocFrom varchar(50), 
		Item varchar(50), 
		BaseUom varchar(5), 
		ManufactureParty varchar(50), 
		StartTime datetime,
		OrderNo varchar(50), 
		[Type] tinyint, 
		Flow varchar(50), 
		OrderQty decimal(18, 8),
		RecQty decimal(18, 8)
	)
	
	--------------------------------↓计算生产Bom消耗----------------------------------
	--------------------------------↓计算Bom总用量----------------------------------
	insert into #tempBomPlan
	(
	LocFrom, 
	Item, 
	BaseUom, 
	ManufactureParty, 
	StartTime,
	OrderNo, 
	Type, 
	Flow, 
	OrderQty,
	RecQty
	)
	select
	LocFrom, 
	Item, 
	BaseUom, 
	ManufactureParty, 
	StartTime,
	OrderNo, 
	Type, 
	Flow, 
	OrderQty,
	RecQty
	from
	(
	--------------------------------↓生产----------------------------------
	--select case when isnull(bom.Location, '') != '' then bom.Location else (case when isnull(det.LocFrom, '') != '' then det.LocFrom else mstr.LocFrom end) end as LocFrom, 
	--bom.Item,
	--bom.BaseUom,
	--bom.ManufactureParty,
	--mstr.StartTime, 
	--mstr.OrderNo,
	--mstr.Type,
	--mstr.Flow,
	--sum(det.OrderQty * bom.BomUnitQty * bom.UnitQty) as OrderQty,  --转为Bom基本单位的用量
	--sum(op.ReportQty * bom.BomUnitQty * bom.UnitQty) as RecQty
	--from ORD_OrderBomDet as bom inner join
	--ORD_OrderOp as op on bom.OrderNo = op.OrderNo and bom.Op = op.Op inner join
	--ORD_OrderDet_4 as det on bom.OrderDetId = det.Id inner join
	--ORD_OrderMstr_4 as mstr on det.OrderNo = mstr.OrderNo
	--where mstr.ProdLineType not in (1, 2, 3, 4, 9) --过滤掉整车生产单
	--and mstr.SubType = 0  --只考虑普通订单的需求，不考虑退货单的
	--and mstr.Status in (1, 2) --只考虑状态为释放和进行中的
	--and det.OrderQty > op.ReportQty
	--group by 
	--bom.Location, 
	--det.LocFrom, 
	--mstr.LocFrom, 
	--bom.Item,
	--bom.BaseUom,
	--bom.ManufactureParty,
	--mstr.StartTime, 
	--mstr.OrderNo,
	--mstr.Type,
	--mstr.Flow
	--------------------------------↑生产----------------------------------
	--union all
	--------------------------------↓委外----------------------------------
	--select case when isnull(bom.Location, '') != '' then bom.Location else (case when isnull(det.LocFrom, '') != '' then det.LocFrom else mstr.LocFrom end) end as LocFrom, 
	--bom.Item,
	--bom.BaseUom,
	--bom.ManufactureParty,
	--mstr.StartTime, 
	--mstr.OrderNo,
	--mstr.Type,
	--mstr.Flow,
	--sum(det.OrderQty * bom.BomUnitQty * bom.UnitQty) as OrderQty,  --转为Bom基本单位的用量
	--sum(op.ReportQty * bom.BomUnitQty * bom.UnitQty) as RecQty
	--from ORD_OrderBomDet as bom inner join
	--ORD_OrderOp as op on bom.OrderNo = op.OrderNo and bom.Op = op.Op inner join
	--ORD_OrderDet_5 as det on bom.OrderDetId = det.Id inner join
	--ORD_OrderMstr_5 as mstr on det.OrderNo = mstr.OrderNo
	--where mstr.ProdLineType not in (1, 2, 3, 4, 9) --过滤掉整车生产单
	--and mstr.SubType = 0  --只考虑普通订单的需求，不考虑退货单的
	--and mstr.Status in (1, 2) --只考虑状态为释放和进行中的
	--and det.OrderQty > op.ReportQty
	--group by
	--bom.Location, 
	--det.LocFrom, 
	--mstr.LocFrom, 
	--bom.Item,
	--bom.BaseUom,
	--bom.ManufactureParty,
	--mstr.StartTime, 
	--mstr.OrderNo,
	--mstr.Type,
	--mstr.Flow
	--------------------------------↑委外----------------------------------
	--union all
	--------------------------------↓整车----------------------------------
	select
	bom.Location as LocFrom, 
	bom.Item, 
	bom.Uom as BaseUom, 
	bom.ManufactureParty, 
	bom.CPTime as StartTime,
	bom.OrderNo, 
	4 as Type, 
	bom.VanProdLine as Flow, 
	SUM(bom.OrderQty) as OrderQty,
	0 as RecQty
	from LE_OrderBomCPTimeSnapshot as bom  --不用考虑虚投料的量，因为库存并没有扣减
	group by
	bom.Location,
	bom.Item, 
	bom.Uom, 
	bom.ManufactureParty, 
	bom.CPTime,
	bom.OrderNo,
	bom.VanProdLine
	--------------------------------↑整车----------------------------------
	union all
	--------------------------------↓整车未反冲的需求----------------------------------
	select
	bom.Location as LocFrom, 
	bom.Item, 
	bom.Uom as BaseUom, 
	bom.ManufactureParty, 
	mstr.StartTime,
	bom.OrderNo, 
	4 as Type, 
	mstr.Flow as Flow, 
	SUM(bom.OrderQty) as OrderQty,
	0 as RecQty
	from ORD_OrderMstr_4 as mstr
	inner join ORD_OrderBomDet as bom on bom.OrderNo = mstr.OrderNo
	where mstr.[Status] = 3 and mstr.ProdLineType in (1,2,3,4,9) and not exists(select top 1 1 from #tempVanOrderNo where OrderNo = mstr.OrderNo) 
	group by
	bom.Location,
	bom.Item, 
	bom.Uom, 
	bom.ManufactureParty, 
	mstr.StartTime,
	bom.OrderNo,
	mstr.Flow
	--------------------------------↑整车未反冲的需求----------------------------------
	) as BomPlan;
	--------------------------------↑计算Bom总用量----------------------------------



	--------------------------------↓计算Bom实际消耗----------------------------------
	--select OrderNo, Item, ManufactureParty, LocFrom, SUM(ShipQty) as ShipQty 
	--into #tempBomConsume
	--from (
	--------------------------------↓Bom回冲量----------------------------------
	--select BFDet.OrderNo, BFDet.Item, BFDet.ManufactureParty, BFDet.LocFrom, sum(-(BFDet.BFQty + BFDet.BFRejQty + BFDet.BFScrapQty) * BFDet.UnitQty) as ShipQty
	--from ORD_OrderBackflushDet as BFDet 
	--inner join (select bom.Id from ORD_OrderBomDet as bom 
	--			inner join ORD_OrderMstr_4 as mstr on bom.OrderNo = mstr.OrderNo
	--			where mstr.ProdLineType not in (1, 2, 3, 4, 9) --过滤掉整车生产单
	--			and mstr.SubType = 0  --只考虑普通订单的需求，不考虑退货单的
	--			and mstr.Status in (1, 2) --只考虑状态为释放和进行中的
	--			union all
	--			select bom.Id from ORD_OrderBomDet as bom 
	--			inner join ORD_OrderMstr_5 as mstr on bom.OrderNo = mstr.OrderNo
	--			where mstr.ProdLineType not in (1, 2, 3, 4, 9) --过滤掉整车生产单
	--			and mstr.SubType = 0  --只考虑普通订单的需求，不考虑退货单的
	--			and mstr.Status in (1, 2) --只考虑状态为释放和进行中的
	--			)as GBP 
	--on BFDet.OrderBomDetId = GBP.Id
	--group by BFDet.OrderNo, BFDet.Item, BFDet.ManufactureParty, BFDet.LocFrom
	--------------------------------↑Bom回冲量----------------------------------
	----union all
	--------------------------------↓生产投料量（在制品）----------------------------------
	----select GBP.OrderNo, GBP.Item, GBP.ManufactureParty, GBP.LocFrom, sum(locDet.Qty - locDet.BFQty - locDet.VoidQty) as ShipQty
	----from PRD_ProdLineLocationDet as locDet left join INV_Hu as hu
	----on locDet.HuId = hu.HuId inner join #tempGroupedBomPlan as GBP 
	----on locDet.OrderNo = GBP.OrderNo and locDet.Item = GBP.Item
	----and locDet.LocFrom = GBP.LocFrom and ISNULL(hu.ManufactureParty, '') = ISNULL(GBP.ManufactureParty, '')
	----where locDet.IsClose = 0   --未关闭的投料
	----and locDet.OrderNo is not null  --投料至工单
	----group by GBP.OrderNo, GBP.Item, GBP.ManufactureParty, GBP.LocFrom
	--------------------------------↑生产投料量（在制品）----------------------------------
	--) as BFBomQty group by OrderNo, Item, ManufactureParty, LocFrom
	
	--------------------------------↑计算Bom实际消耗----------------------------------
	--------------------------------↑计算生产Bom消耗----------------------------------



	create table #tempPlan
	(
		Id int identity(1, 1) Primary Key,
		LocFrom varchar(50), 
		LocTo varchar(50), 
		Item varchar(50),
		Uom varchar(5),
		BaseUom varchar(5),
		UnitQty decimal(18, 8),
		ManufactureParty varchar(50),
		StartTime datetime, 
		WindowTime datetime, 
		OrderNo varchar(50),
		[Type] tinyint,
		Flow varchar(50),
		OrderQty decimal(18, 8),
		ShipQty decimal(18, 8),
		RecQty decimal(18, 8)
	)
	------------------------------↓获取订单----------------------------------
    insert into #tempPlan
    (
	LocFrom, 
	LocTo, 
	Item,
	Uom,
	BaseUom,
	UnitQty,
	ManufactureParty,
	StartTime, 
	WindowTime, 
	OrderNo,
	Type,
	Flow,
	OrderQty,
	ShipQty,
	RecQty
    )
    select 
    LocFrom, 
	LocTo, 
	Item,
	Uom,
	BaseUom,
	UnitQty,
	ManufactureParty,
	StartTime, 
	WindowTime, 
	OrderNo,
	Type,
	Flow,
	OrderQty,
	ShipQty,
	RecQty
    from(
	---------------------------↓采购-------------------------------------
	select 
	null as LocFrom, 
	case when isnull(det.LocTo, '') <> '' then det.LocTo else mstr.LocTo end as LocTo, 
	det.Item,
	det.Uom,
	det.BaseUom,
	det.UnitQty,
	det.ManufactureParty,
	mstr.StartTime, 
	mstr.WindowTime, 
	mstr.OrderNo,
	mstr.Type,
	mstr.Flow,
	det.OrderQty,
	det.ShipQty,
	det.RecQty
	from ORD_OrderDet_1 as det 
	inner join ORD_OrderMstr_1 as mstr on det.OrderNo = mstr.OrderNo
	left join CUST_ManualGenOrderTrace as trace on mstr.OrderNo = trace.OrderNo  --过滤掉为非整车拉动的订单
	where mstr.SubType = 0  --只考虑普通订单的需求，不考虑退货单的
	and mstr.Status in (1, 2) --只考虑状态为释放和进行中的
	and trace.OrderNo is null
	---------------------------↑采购-------------------------------------
	union all
	---------------------------↓移库-------------------------------------
	select 
	case when isnull(det.LocFrom, '') <> '' then det.LocFrom else mstr.LocFrom end as LocFrom, 
	case when isnull(det.LocTo, '') <> '' then det.LocTo else mstr.LocTo end as LocTo, 
	det.Item,
	det.Uom,
	det.BaseUom,
	det.UnitQty,
	det.ManufactureParty,
	mstr.StartTime, 
	mstr.WindowTime,
	mstr.OrderNo,
	mstr.Type,
	mstr.Flow,
	det.OrderQty,
	det.ShipQty,
	det.RecQty
	from ORD_OrderDet_2 as det inner join ORD_OrderMstr_2 as mstr on det.OrderNo = mstr.OrderNo
	left join CUST_ManualGenOrderTrace as trace on mstr.OrderNo = trace.OrderNo  --过滤掉为非整车拉动的订单
	where mstr.SubType = 0  --只考虑普通订单的需求，不考虑退货单的
	and mstr.Status in (1, 2) --只考虑状态为释放和进行中的
	and trace.OrderNo is null
	---------------------------↑移库-------------------------------------
	union all
	-----------------------------↓销售-------------------------------------
	--select 
	--case when isnull(det.LocFrom, '') <> '' then det.LocFrom else mstr.LocFrom end as LocFrom, 
	--null as LocTo, 
	--det.Item,
	--det.Uom,
	--det.BaseUom,
	--det.UnitQty,
	--det.ManufactureParty,
	--mstr.StartTime, 
	--mstr.WindowTime,
	--mstr.OrderNo,
	--mstr.Type,
	--mstr.Flow,
	--det.OrderQty,
	--det.ShipQty,
	--det.RecQty
	--from ORD_OrderDet_3 as det inner join
	--ORD_OrderMstr_3 as mstr on det.OrderNo = mstr.OrderNo
	--where mstr.SubType = 0  --只考虑普通订单的需求，不考虑退货单的
	--and mstr.Status in (1, 2) --只考虑状态为释放和进行中的
	---------------------------↑销售-------------------------------------
	--union all
	---------------------------↓生产收货-------------------------------------
	--在OrderDetail上生产只考虑收货，不考虑发料。发料在OrderBomDetail中考虑。
	--select 
	--null as LocFrom, 
	--case when isnull(det.LocTo, '') <> '' then det.LocTo else mstr.LocTo end as LocTo, 
	--det.Item,
	--det.Uom,
	--det.BaseUom,
	--det.UnitQty,
	--det.ManufactureParty,
	--null as StartTime, 
	--mstr.WindowTime,
	--mstr.OrderNo,
	--mstr.Type,
	--mstr.Flow,
	--det.OrderQty,
	--0 as ShipQty,
	--det.RecQty
	--from ORD_OrderDet_4 as det inner join
	--ORD_OrderMstr_4 as mstr on det.OrderNo = mstr.OrderNo
	--where mstr.ProdLineType not in (1, 2, 3, 4, 9) --过滤掉整车生产单
	--and mstr.SubType = 0  --只考虑普通订单的需求，不考虑退货单的
	--and mstr.Status in (1, 2) --只考虑状态为释放和进行中的
	--and det.OrderQty > det.RecQty --只考虑未完成收货的
	---------------------------↑生产收货-------------------------------------
	--union all
	---------------------------↓委外加工-------------------------------------
	--在OrderDetail上委外加工只考虑收货，不考虑发料。发料在OrderBomDetail中考虑。
	--select 
	--null as LocFrom, 
	--case when isnull(det.LocTo, '') <> '' then det.LocTo else mstr.LocTo end as LocTo, 
	--det.Item,
	--det.Uom,
	--det.BaseUom,
	--det.UnitQty,
	--det.ManufactureParty,
	--null as StartTime, 
	--mstr.WindowTime,
	--mstr.OrderNo,
	--mstr.Type,
	--mstr.Flow,
	--det.OrderQty,
	--0 as ShipQty,
	--det.RecQty
	--from ORD_OrderDet_5 as det inner join
	--ORD_OrderMstr_5 as mstr on det.OrderNo = mstr.OrderNo
	--where mstr.SubType = 0  --只考虑普通订单的需求，不考虑退货单的
	--and mstr.Status in (1, 2) --只考虑状态为释放和进行中的
	--and det.OrderQty > det.RecQty --只考虑未完成收货的
	---------------------------↑委外加工-------------------------------------
	--union all
	---------------------------↓客供品-------------------------------------
	--select 
	--null as LocFrom, 
	--case when isnull(det.LocTo, '') <> '' then det.LocTo else mstr.LocTo end as LocTo, 
	--det.Item,
	--det.Uom,
	--det.BaseUom,
	--det.UnitQty,
	--det.ManufactureParty,
	--mstr.StartTime, 
	--mstr.WindowTime,
	--mstr.OrderNo,
	--mstr.Type,
	--mstr.Flow,
	--det.OrderQty,
	--det.ShipQty,
	--det.RecQty
	--from ORD_OrderDet_6 as det inner join
	--ORD_OrderMstr_6 as mstr on det.OrderNo = mstr.OrderNo
	--where mstr.SubType = 0  --只考虑普通订单的需求，不考虑退货单的
	--and mstr.Status in (1, 2) --只考虑状态为释放和进行中的
	---------------------------↑客供品-------------------------------------
	--union all
	---------------------------↓委外加工发料——发料至委外库-------------------------------------
	--select 
	--case when isnull(det.LocFrom, '') <> '' then det.LocFrom else mstr.LocFrom end as LocFrom, 
	--case when isnull(det.LocTo, '') <> '' then det.LocTo else mstr.LocTo end as LocTo, 
	--det.Item,
	--det.Uom,
	--det.BaseUom,
	--det.UnitQty,
	--det.ManufactureParty,
	--mstr.StartTime, 
	--mstr.WindowTime,
	--mstr.OrderNo,
	--mstr.Type,
	--mstr.Flow,
	--det.OrderQty,
	--det.ShipQty,
	--det.RecQty
	--from ORD_OrderDet_7 as det inner join
	--ORD_OrderMstr_7 as mstr on det.OrderNo = mstr.OrderNo
	--where mstr.SubType = 0  --只考虑普通订单的需求，不考虑退货单的
	--and mstr.Status in (1, 2) --只考虑状态为释放和进行中的
	---------------------------↑委外加工发料——发料至委外库-------------------------------------
	--union all
	---------------------------↓计划协议-------------------------------------
	--select 
	--case when isnull(det.LocFrom, '') <> '' then det.LocFrom else mstr.LocFrom end as LocFrom, 
	--case when isnull(det.LocTo, '') <> '' then det.LocTo else mstr.LocTo end as LocTo, 
	--det.Item,
	--det.Uom,
	--det.BaseUom,
	--det.UnitQty,
	--det.ManufactureParty,
	--case when det.StartDate is not null then det.StartDate else mstr.StartTime end as StartTime, 
	--case when det.EndDate is not null then det.EndDate else mstr.WindowTime end as WindowTime, 
	--mstr.OrderNo,
	--mstr.Type,
	--mstr.Flow,
	--det.OrderQty,
	--det.ShipQty,
	--det.RecQty
	--from ORD_OrderDet_8 as det inner join
	--ORD_OrderMstr_8 as mstr on det.OrderNo = mstr.OrderNo
	--where mstr.SubType = 0  --只考虑普通订单的需求，不考虑退货单的
	--and mstr.Status in (1, 2) --只考虑状态为释放和进行中的
	---------------------------↑计划协议-------------------------------------
	--union all
	-----------------------------↓生产消耗（含生产和委外加工）-------------------------------------
	select bp.LocFrom,
	null as LocTo,
	bp.Item,
	bp.BaseUom as Uom,
	bp.BaseUom,
	1 as UnitQty,
	bp.ManufactureParty,
	bp.StartTime,
	null as WindowTime,
	bp.OrderNo,
	bp.Type,
	bp.Flow,
	bp.OrderQty,
	0 as ShipQty,
	bp.RecQty
	from #tempBomPlan as bp
	) as A
	---------------------------↑生产消耗（含生产和委外加工）-------------------------------------
	------------------------------↑获取订单----------------------------------
	
	
	
	declare @trancount int = @@trancount
	
	begin try
		if @trancount = 0
		begin
            begin tran
        end
        
		truncate table LE_OrderPlanSnapshot
		insert into LE_OrderPlanSnapshot(Location, RefLocation, Item, ManufactureParty, ReqTime, OrderNo, IRType, OrderType, OrderQty, FinishQty)
		-------------------↓汇总待发需求-----------------------
		select 
		LocFrom as Location,
		LocTo as RefLocation, 
		Item, 
		ManufactureParty,
		StartTime as ReqTime, 
		OrderNo,
		0 as IRType,
		Type as OrderType,
		SUM(OrderQty * UnitQty) as OrderQty,    --转为库存单位
		SUM(ShipQty * UnitQty) as FinishQty     --转为库存单位
		from #tempPlan 
		where LocFrom is not null and OrderQty > ShipQty
		group by 
		LocFrom, 
		LocTo,
		Item, 
		ManufactureParty,
		StartTime, 
		OrderNo,
		[Type]
		---------------------↑汇总待发需求-----------------------
		union all
		-------------------↓汇总待收需求-----------------------
		select
		LocTo as Location,
		LocFrom as RefLocation, 
		Item, 
		ManufactureParty,
		WindowTime as ReqTime,
		OrderNo,
		1 as IRType,
		Type as OrderType,
		SUM(OrderQty * UnitQty) as OrderQty,    --转为库存单位
		SUM(RecQty * UnitQty) as FinishQty      --转为库存单位
		from #tempPlan 
		where LocTo is not null and OrderQty > RecQty
		group by
		LocTo,
		LocFrom,
		Item, 
		ManufactureParty,
		WindowTime,
		OrderNo,
		[Type]
		-------------------↑汇总待收需求-----------------------
		 
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
        
        set @Msg = N'快照OrderPlan出现异常，异常信息：' + ERROR_MESSAGE()
		insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	end catch
	 
	drop table #tempPlan
	--drop table #tempBomConsume
	drop table #tempBomPlan
	drop table #tempVanOrderNo
	 
	set @Msg = N'快照OrderPlan结束'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
END
GO

