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
	set @Msg = N'����OrderPlan��ʼ'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	
	--����δ���ߵ������������ţ�Ϊ�˱���δ����������δ���ߵ������ظ�
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
	
	--------------------------------����������Bom����----------------------------------
	--------------------------------������Bom������----------------------------------
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
	--------------------------------������----------------------------------
	--select case when isnull(bom.Location, '') != '' then bom.Location else (case when isnull(det.LocFrom, '') != '' then det.LocFrom else mstr.LocFrom end) end as LocFrom, 
	--bom.Item,
	--bom.BaseUom,
	--bom.ManufactureParty,
	--mstr.StartTime, 
	--mstr.OrderNo,
	--mstr.Type,
	--mstr.Flow,
	--sum(det.OrderQty * bom.BomUnitQty * bom.UnitQty) as OrderQty,  --תΪBom������λ������
	--sum(op.ReportQty * bom.BomUnitQty * bom.UnitQty) as RecQty
	--from ORD_OrderBomDet as bom inner join
	--ORD_OrderOp as op on bom.OrderNo = op.OrderNo and bom.Op = op.Op inner join
	--ORD_OrderDet_4 as det on bom.OrderDetId = det.Id inner join
	--ORD_OrderMstr_4 as mstr on det.OrderNo = mstr.OrderNo
	--where mstr.ProdLineType not in (1, 2, 3, 4, 9) --���˵�����������
	--and mstr.SubType = 0  --ֻ������ͨ���������󣬲������˻�����
	--and mstr.Status in (1, 2) --ֻ����״̬Ϊ�ͷźͽ����е�
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
	--------------------------------������----------------------------------
	--union all
	--------------------------------��ί��----------------------------------
	--select case when isnull(bom.Location, '') != '' then bom.Location else (case when isnull(det.LocFrom, '') != '' then det.LocFrom else mstr.LocFrom end) end as LocFrom, 
	--bom.Item,
	--bom.BaseUom,
	--bom.ManufactureParty,
	--mstr.StartTime, 
	--mstr.OrderNo,
	--mstr.Type,
	--mstr.Flow,
	--sum(det.OrderQty * bom.BomUnitQty * bom.UnitQty) as OrderQty,  --תΪBom������λ������
	--sum(op.ReportQty * bom.BomUnitQty * bom.UnitQty) as RecQty
	--from ORD_OrderBomDet as bom inner join
	--ORD_OrderOp as op on bom.OrderNo = op.OrderNo and bom.Op = op.Op inner join
	--ORD_OrderDet_5 as det on bom.OrderDetId = det.Id inner join
	--ORD_OrderMstr_5 as mstr on det.OrderNo = mstr.OrderNo
	--where mstr.ProdLineType not in (1, 2, 3, 4, 9) --���˵�����������
	--and mstr.SubType = 0  --ֻ������ͨ���������󣬲������˻�����
	--and mstr.Status in (1, 2) --ֻ����״̬Ϊ�ͷźͽ����е�
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
	--------------------------------��ί��----------------------------------
	--union all
	--------------------------------������----------------------------------
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
	from LE_OrderBomCPTimeSnapshot as bom  --���ÿ�����Ͷ�ϵ�������Ϊ��沢û�пۼ�
	group by
	bom.Location,
	bom.Item, 
	bom.Uom, 
	bom.ManufactureParty, 
	bom.CPTime,
	bom.OrderNo,
	bom.VanProdLine
	--------------------------------������----------------------------------
	union all
	--------------------------------������δ���������----------------------------------
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
	--------------------------------������δ���������----------------------------------
	) as BomPlan;
	--------------------------------������Bom������----------------------------------



	--------------------------------������Bomʵ������----------------------------------
	--select OrderNo, Item, ManufactureParty, LocFrom, SUM(ShipQty) as ShipQty 
	--into #tempBomConsume
	--from (
	--------------------------------��Bom�س���----------------------------------
	--select BFDet.OrderNo, BFDet.Item, BFDet.ManufactureParty, BFDet.LocFrom, sum(-(BFDet.BFQty + BFDet.BFRejQty + BFDet.BFScrapQty) * BFDet.UnitQty) as ShipQty
	--from ORD_OrderBackflushDet as BFDet 
	--inner join (select bom.Id from ORD_OrderBomDet as bom 
	--			inner join ORD_OrderMstr_4 as mstr on bom.OrderNo = mstr.OrderNo
	--			where mstr.ProdLineType not in (1, 2, 3, 4, 9) --���˵�����������
	--			and mstr.SubType = 0  --ֻ������ͨ���������󣬲������˻�����
	--			and mstr.Status in (1, 2) --ֻ����״̬Ϊ�ͷźͽ����е�
	--			union all
	--			select bom.Id from ORD_OrderBomDet as bom 
	--			inner join ORD_OrderMstr_5 as mstr on bom.OrderNo = mstr.OrderNo
	--			where mstr.ProdLineType not in (1, 2, 3, 4, 9) --���˵�����������
	--			and mstr.SubType = 0  --ֻ������ͨ���������󣬲������˻�����
	--			and mstr.Status in (1, 2) --ֻ����״̬Ϊ�ͷźͽ����е�
	--			)as GBP 
	--on BFDet.OrderBomDetId = GBP.Id
	--group by BFDet.OrderNo, BFDet.Item, BFDet.ManufactureParty, BFDet.LocFrom
	--------------------------------��Bom�س���----------------------------------
	----union all
	--------------------------------������Ͷ����������Ʒ��----------------------------------
	----select GBP.OrderNo, GBP.Item, GBP.ManufactureParty, GBP.LocFrom, sum(locDet.Qty - locDet.BFQty - locDet.VoidQty) as ShipQty
	----from PRD_ProdLineLocationDet as locDet left join INV_Hu as hu
	----on locDet.HuId = hu.HuId inner join #tempGroupedBomPlan as GBP 
	----on locDet.OrderNo = GBP.OrderNo and locDet.Item = GBP.Item
	----and locDet.LocFrom = GBP.LocFrom and ISNULL(hu.ManufactureParty, '') = ISNULL(GBP.ManufactureParty, '')
	----where locDet.IsClose = 0   --δ�رյ�Ͷ��
	----and locDet.OrderNo is not null  --Ͷ��������
	----group by GBP.OrderNo, GBP.Item, GBP.ManufactureParty, GBP.LocFrom
	--------------------------------������Ͷ����������Ʒ��----------------------------------
	--) as BFBomQty group by OrderNo, Item, ManufactureParty, LocFrom
	
	--------------------------------������Bomʵ������----------------------------------
	--------------------------------����������Bom����----------------------------------



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
	------------------------------����ȡ����----------------------------------
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
	---------------------------���ɹ�-------------------------------------
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
	left join CUST_ManualGenOrderTrace as trace on mstr.OrderNo = trace.OrderNo  --���˵�Ϊ�����������Ķ���
	where mstr.SubType = 0  --ֻ������ͨ���������󣬲������˻�����
	and mstr.Status in (1, 2) --ֻ����״̬Ϊ�ͷźͽ����е�
	and trace.OrderNo is null
	---------------------------���ɹ�-------------------------------------
	union all
	---------------------------���ƿ�-------------------------------------
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
	left join CUST_ManualGenOrderTrace as trace on mstr.OrderNo = trace.OrderNo  --���˵�Ϊ�����������Ķ���
	where mstr.SubType = 0  --ֻ������ͨ���������󣬲������˻�����
	and mstr.Status in (1, 2) --ֻ����״̬Ϊ�ͷźͽ����е�
	and trace.OrderNo is null
	---------------------------���ƿ�-------------------------------------
	union all
	-----------------------------������-------------------------------------
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
	--where mstr.SubType = 0  --ֻ������ͨ���������󣬲������˻�����
	--and mstr.Status in (1, 2) --ֻ����״̬Ϊ�ͷźͽ����е�
	---------------------------������-------------------------------------
	--union all
	---------------------------�������ջ�-------------------------------------
	--��OrderDetail������ֻ�����ջ��������Ƿ��ϡ�������OrderBomDetail�п��ǡ�
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
	--where mstr.ProdLineType not in (1, 2, 3, 4, 9) --���˵�����������
	--and mstr.SubType = 0  --ֻ������ͨ���������󣬲������˻�����
	--and mstr.Status in (1, 2) --ֻ����״̬Ϊ�ͷźͽ����е�
	--and det.OrderQty > det.RecQty --ֻ����δ����ջ���
	---------------------------�������ջ�-------------------------------------
	--union all
	---------------------------��ί��ӹ�-------------------------------------
	--��OrderDetail��ί��ӹ�ֻ�����ջ��������Ƿ��ϡ�������OrderBomDetail�п��ǡ�
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
	--where mstr.SubType = 0  --ֻ������ͨ���������󣬲������˻�����
	--and mstr.Status in (1, 2) --ֻ����״̬Ϊ�ͷźͽ����е�
	--and det.OrderQty > det.RecQty --ֻ����δ����ջ���
	---------------------------��ί��ӹ�-------------------------------------
	--union all
	---------------------------���͹�Ʒ-------------------------------------
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
	--where mstr.SubType = 0  --ֻ������ͨ���������󣬲������˻�����
	--and mstr.Status in (1, 2) --ֻ����״̬Ϊ�ͷźͽ����е�
	---------------------------���͹�Ʒ-------------------------------------
	--union all
	---------------------------��ί��ӹ����ϡ���������ί���-------------------------------------
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
	--where mstr.SubType = 0  --ֻ������ͨ���������󣬲������˻�����
	--and mstr.Status in (1, 2) --ֻ����״̬Ϊ�ͷźͽ����е�
	---------------------------��ί��ӹ����ϡ���������ί���-------------------------------------
	--union all
	---------------------------���ƻ�Э��-------------------------------------
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
	--where mstr.SubType = 0  --ֻ������ͨ���������󣬲������˻�����
	--and mstr.Status in (1, 2) --ֻ����״̬Ϊ�ͷźͽ����е�
	---------------------------���ƻ�Э��-------------------------------------
	--union all
	-----------------------------���������ģ���������ί��ӹ���-------------------------------------
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
	---------------------------���������ģ���������ί��ӹ���-------------------------------------
	------------------------------����ȡ����----------------------------------
	
	
	
	declare @trancount int = @@trancount
	
	begin try
		if @trancount = 0
		begin
            begin tran
        end
        
		truncate table LE_OrderPlanSnapshot
		insert into LE_OrderPlanSnapshot(Location, RefLocation, Item, ManufactureParty, ReqTime, OrderNo, IRType, OrderType, OrderQty, FinishQty)
		-------------------�����ܴ�������-----------------------
		select 
		LocFrom as Location,
		LocTo as RefLocation, 
		Item, 
		ManufactureParty,
		StartTime as ReqTime, 
		OrderNo,
		0 as IRType,
		Type as OrderType,
		SUM(OrderQty * UnitQty) as OrderQty,    --תΪ��浥λ
		SUM(ShipQty * UnitQty) as FinishQty     --תΪ��浥λ
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
		---------------------�����ܴ�������-----------------------
		union all
		-------------------�����ܴ�������-----------------------
		select
		LocTo as Location,
		LocFrom as RefLocation, 
		Item, 
		ManufactureParty,
		WindowTime as ReqTime,
		OrderNo,
		1 as IRType,
		Type as OrderType,
		SUM(OrderQty * UnitQty) as OrderQty,    --תΪ��浥λ
		SUM(RecQty * UnitQty) as FinishQty      --תΪ��浥λ
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
		-------------------�����ܴ�������-----------------------
		 
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
        
        set @Msg = N'����OrderPlan�����쳣���쳣��Ϣ��' + ERROR_MESSAGE()
		insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	end catch
	 
	drop table #tempPlan
	--drop table #tempBomConsume
	drop table #tempBomPlan
	drop table #tempVanOrderNo
	 
	set @Msg = N'����OrderPlan����'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
END
GO

